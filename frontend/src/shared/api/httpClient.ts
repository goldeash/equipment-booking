import { env } from '@/shared/config/env'
import { logger } from '@/shared/lib/logger'
import { ApiError, type ProblemDetails } from './apiError'

export type QueryValue = string | number | boolean | null | undefined | readonly (string | number)[]

export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  query?: Record<string, QueryValue>
  signal?: AbortSignal
  /** Sends the request without the access token, for sign in and refresh. */
  anonymous?: boolean
}

/** Connects the client to the session: token lookup, refresh and sign out on expiry. */
export interface AuthHandlers {
  getAccessToken: () => string | null
  refreshAccessToken: () => Promise<string | null>
  onSessionExpired: () => void
}

let authHandlers: AuthHandlers | null = null

export function configureAuthHandlers(handlers: AuthHandlers | null): void {
  authHandlers = handlers
}

export function buildUrl(path: string, query?: Record<string, QueryValue>): string {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query ?? {})) {
    if (value === undefined || value === null || value === '') {
      continue
    }
    if (Array.isArray(value)) {
      value.forEach((item) => params.append(key, String(item)))
    } else {
      params.append(key, String(value))
    }
  }
  const search = params.toString()
  return `${env.apiBaseUrl}${path}${search ? `?${search}` : ''}`
}

async function send(
  path: string,
  options: RequestOptions,
  token: string | null,
): Promise<Response> {
  const headers = new Headers({ Accept: 'application/json' })
  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }
  if (token && !options.anonymous) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  try {
    return await fetch(buildUrl(path, options.query), {
      method: options.method ?? 'GET',
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
      credentials: 'same-origin',
      signal: options.signal,
    })
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw error
    }
    logger.error('Network request failed', { path })
    throw ApiError.network()
  }
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails = { status: response.status, title: response.statusText }
  const contentType = response.headers.get('Content-Type') ?? ''
  if (contentType.includes('json')) {
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      logger.warn('The error response body is not valid JSON', { status: response.status })
    }
  }
  return new ApiError(response.status, problem)
}

/**
 * Sends a JSON request to the API. A 401 response triggers one token refresh and a retry;
 * other errors are thrown as {@link ApiError}.
 */
export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  let response = await send(path, options, authHandlers?.getAccessToken() ?? null)

  if (response.status === 401 && !options.anonymous && authHandlers) {
    const refreshedToken = await authHandlers.refreshAccessToken()
    if (refreshedToken) {
      response = await send(path, options, refreshedToken)
    }
    if (response.status === 401) {
      authHandlers.onSessionExpired()
    }
  }

  if (!response.ok) {
    const error = await toApiError(response)
    if (error.status >= 500) {
      logger.error('Server error', { path, code: error.code, traceId: error.problem.traceId })
    }
    throw error
  }

  if (response.status === 204) {
    return undefined as T
  }
  return (await response.json()) as T
}
