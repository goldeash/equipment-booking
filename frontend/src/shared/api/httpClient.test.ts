import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError } from './apiError'
import { buildUrl, configureAuthHandlers, request } from './httpClient'

const json = (status: number, body: unknown) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': status >= 400 ? 'application/problem+json' : 'application/json' },
  })

describe('buildUrl', () => {
  it('skips empty values and repeats array values', () => {
    expect(
      buildUrl('/equipment', { q: 'camera', page: 2, empty: '', none: undefined, ids: [1, 2] }),
    ).toBe('/api/v1/equipment?q=camera&page=2&ids=1&ids=2')
  })
})

describe('request', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    fetchMock.mockReset()
    configureAuthHandlers(null)
  })

  it('throws an ApiError with the problem details code', async () => {
    fetchMock.mockResolvedValueOnce(
      json(409, { title: 'Conflict', detail: 'The item is booked.', code: 'booking.overlap' }),
    )

    const error = await request('/bookings', { method: 'POST', body: {} }).catch(
      (caught: unknown) => caught,
    )

    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).status).toBe(409)
    expect((error as ApiError).code).toBe('booking.overlap')
    expect((error as ApiError).message).toBe('The item is booked.')
  })

  it('refreshes the token once on 401 and retries the request', async () => {
    const refreshAccessToken = vi.fn().mockResolvedValue('new-token')
    const onSessionExpired = vi.fn()
    configureAuthHandlers({
      getAccessToken: () => 'old-token',
      refreshAccessToken,
      onSessionExpired,
    })
    fetchMock
      .mockResolvedValueOnce(json(401, { code: 'auth.unauthorized' }))
      .mockResolvedValueOnce(json(200, { id: 1 }))

    const result = await request<{ id: number }>('/me')

    expect(result).toEqual({ id: 1 })
    expect(refreshAccessToken).toHaveBeenCalledTimes(1)
    const retriedHeaders = fetchMock.mock.calls[1]?.[1]?.headers as Headers
    expect(retriedHeaders.get('Authorization')).toBe('Bearer new-token')
    expect(onSessionExpired).not.toHaveBeenCalled()
  })

  it('ends the session when the refresh fails', async () => {
    const onSessionExpired = vi.fn()
    configureAuthHandlers({
      getAccessToken: () => 'expired',
      refreshAccessToken: vi.fn().mockResolvedValue(null),
      onSessionExpired,
    })
    fetchMock.mockResolvedValueOnce(json(401, { code: 'auth.unauthorized' }))

    await expect(request('/me')).rejects.toBeInstanceOf(ApiError)
    expect(onSessionExpired).toHaveBeenCalledTimes(1)
  })

  it('reports network failures as an ApiError with status 0', async () => {
    fetchMock.mockRejectedValueOnce(new TypeError('Failed to fetch'))

    const error = await request('/equipment').catch((caught: unknown) => caught)

    expect((error as ApiError).status).toBe(0)
    expect((error as ApiError).code).toBe('network.unavailable')
  })
})
