import { create } from 'zustand'
import { authApi } from '@/shared/api/endpoints'
import { configureAuthHandlers } from '@/shared/api/httpClient'
import type { CurrentUser, Role, Session } from '@/shared/api/types'

type SessionState = 'unknown' | 'signedIn' | 'signedOut'

interface AuthState {
  status: SessionState
  accessToken: string | null
  user: CurrentUser | null
  setSession: (session: Session) => void
  setUser: (user: CurrentUser) => void
  clear: () => void
}

/**
 * Session of the signed in user. The access token lives only in memory; the refresh token is an
 * HttpOnly cookie that scripts cannot read.
 */
export const useAuthStore = create<AuthState>((set) => ({
  status: 'unknown',
  accessToken: null,
  user: null,
  setSession: (session) =>
    set({ status: 'signedIn', accessToken: session.accessToken, user: session.user }),
  setUser: (user) => set({ user }),
  clear: () => set({ status: 'signedOut', accessToken: null, user: null }),
}))

let refreshInFlight: Promise<string | null> | null = null

/** Refreshes the access token once, even when several requests fail with 401 at the same time. */
export function refreshSession(): Promise<string | null> {
  refreshInFlight ??= authApi
    .refresh()
    .then((session) => {
      useAuthStore.getState().setSession(session)
      return session.accessToken
    })
    .catch(() => {
      useAuthStore.getState().clear()
      return null
    })
    .finally(() => {
      refreshInFlight = null
    })
  return refreshInFlight
}

export function initialiseAuth(): void {
  configureAuthHandlers({
    getAccessToken: () => useAuthStore.getState().accessToken,
    refreshAccessToken: refreshSession,
    onSessionExpired: () => useAuthStore.getState().clear(),
  })
}

export async function signOut(): Promise<void> {
  try {
    await authApi.signOut()
  } finally {
    useAuthStore.getState().clear()
  }
}

export const hasRole = (user: CurrentUser | null, ...roles: Role[]): boolean =>
  user !== null && roles.includes(user.role)
