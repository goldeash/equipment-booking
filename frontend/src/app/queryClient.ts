import { QueryClient } from '@tanstack/react-query'
import { isApiError } from '@/shared/api/apiError'

const NON_RETRYABLE_STATUSES = new Set([400, 401, 403, 404, 409, 422])

export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        retry: (failureCount, error) =>
          !(isApiError(error) && NON_RETRYABLE_STATUSES.has(error.status)) && failureCount < 2,
      },
      mutations: { retry: false },
    },
  })
}
