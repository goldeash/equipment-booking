/** RFC 9457 problem details returned by the API for every error. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  code?: string
  traceId?: string
  errors?: Record<string, string[]>
  [extension: string]: unknown
}

/** Error thrown by the HTTP client for every unsuccessful response or network failure. */
export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly problem: ProblemDetails

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? 'The request failed.')
    this.name = 'ApiError'
    this.status = status
    this.code = problem.code ?? 'server.unexpected'
    this.problem = problem
  }

  /** Validation messages keyed by camelCase field name. */
  get fieldErrors(): Record<string, string[]> {
    return this.problem.errors ?? {}
  }

  static network(): ApiError {
    return new ApiError(0, {
      title: 'Network error',
      detail: 'The server could not be reached. Check your connection and try again.',
      code: 'network.unavailable',
    })
  }
}

export const isApiError = (error: unknown): error is ApiError => error instanceof ApiError

/** Turns any thrown value into a message that can be shown to the user. */
export function getErrorMessage(error: unknown): string {
  if (isApiError(error)) {
    return error.message
  }
  return 'Something went wrong. Please try again.'
}
