import { MantineProvider } from '@mantine/core'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { CurrentUser } from '@/shared/api/types'
import { refreshSession, useAuthStore } from './authStore'
import { RequireRole } from './guards'

const manager: CurrentUser = {
  id: 2,
  email: 'manager@company.example',
  fullName: 'Max Manager',
  role: 'manager',
  departmentId: 1,
  departmentName: 'Engineering',
  privacyNoticeAcknowledged: true,
}

const renderGuard = () =>
  render(
    <MantineProvider>
      <MemoryRouter>
        <RequireRole roles={['manager']}>
          <p>Approval queue</p>
        </RequireRole>
      </MemoryRouter>
    </MantineProvider>,
  )

describe('RequireRole', () => {
  afterEach(() => useAuthStore.getState().clear())

  it('renders the page for a user with the role', () => {
    useAuthStore
      .getState()
      .setSession({ accessToken: 'token', accessTokenExpiresAt: '', user: manager })

    renderGuard()

    expect(screen.getByText('Approval queue')).toBeInTheDocument()
  })

  it('shows the access denied page for other roles', () => {
    useAuthStore.getState().setSession({
      accessToken: 'token',
      accessTokenExpiresAt: '',
      user: { ...manager, role: 'employee' },
    })

    renderGuard()

    expect(screen.getByText('Access denied')).toBeInTheDocument()
    expect(screen.queryByText('Approval queue')).not.toBeInTheDocument()
  })
})

describe('refreshSession', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => vi.stubGlobal('fetch', fetchMock))
  afterEach(() => {
    vi.unstubAllGlobals()
    fetchMock.mockReset()
    useAuthStore.getState().clear()
  })

  it('sends one refresh request for concurrent callers and stores the session', async () => {
    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({ accessToken: 'new-token', accessTokenExpiresAt: '', user: manager }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    const tokens = await Promise.all([refreshSession(), refreshSession(), refreshSession()])

    expect(tokens).toEqual(['new-token', 'new-token', 'new-token'])
    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(useAuthStore.getState().status).toBe('signedIn')
    expect(useAuthStore.getState().user?.fullName).toBe('Max Manager')
  })

  it('signs the user out when the refresh token is rejected', async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify({ code: 'auth.invalid_refresh_token' }), {
        status: 401,
        headers: { 'Content-Type': 'application/problem+json' },
      }),
    )

    expect(await refreshSession()).toBeNull()
    expect(useAuthStore.getState().status).toBe('signedOut')
  })
})
