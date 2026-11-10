import { Center, Loader } from '@mantine/core'
import { useEffect, type ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router'
import { ForbiddenPage } from '@/pages/errors/ErrorPage'
import type { Role } from '@/shared/api/types'
import { hasRole, refreshSession, useAuthStore } from './authStore'

/** Restores the session from the refresh cookie once and redirects anonymous users to sign in. */
export function RequireAuth({ children }: { children: ReactNode }) {
  const status = useAuthStore((state) => state.status)
  const location = useLocation()

  useEffect(() => {
    if (status === 'unknown') {
      void refreshSession()
    }
  }, [status])

  if (status === 'unknown') {
    return (
      <Center mih="60vh">
        <Loader aria-label="Loading session" />
      </Center>
    )
  }
  if (status === 'signedOut') {
    return <Navigate to="/sign-in" replace state={{ from: location.pathname }} />
  }
  return children
}

/** Shows the 403 page when the signed in user has none of the roles. */
export function RequireRole({ roles, children }: { roles: Role[]; children: ReactNode }) {
  const user = useAuthStore((state) => state.user)
  return hasRole(user, ...roles) ? children : <ForbiddenPage />
}
