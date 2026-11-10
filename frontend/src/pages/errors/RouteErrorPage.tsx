import { useEffect } from 'react'
import { isRouteErrorResponse, useRouteError } from 'react-router'
import { isApiError } from '@/shared/api/apiError'
import { logger } from '@/shared/lib/logger'
import { ErrorPage, ForbiddenPage, NotFoundPage } from './ErrorPage'

/** Error boundary of the router: shows a page for render errors and failed route loads. */
export function RouteErrorPage() {
  const error = useRouteError()

  useEffect(() => {
    logger.error('Route error', error)
  }, [error])

  const status = isRouteErrorResponse(error) ? error.status : isApiError(error) ? error.status : 500
  if (status === 404) {
    return <NotFoundPage />
  }
  if (status === 403) {
    return <ForbiddenPage />
  }
  return (
    <ErrorPage
      code="500"
      title="Something went wrong"
      message="An unexpected error occurred. Reload the page or try again later."
    />
  )
}
