import type { ComponentType } from 'react'
import { createBrowserRouter, type RouteObject } from 'react-router'
import { RequireAuth, RequireRole } from '@/features/auth/guards'
import { SignInPage } from '@/features/auth/SignInPage'
import { NotFoundPage } from '@/pages/errors/ErrorPage'
import { RouteErrorPage } from '@/pages/errors/RouteErrorPage'
import type { Role } from '@/shared/api/types'
import { AppLayout } from './layouts/AppLayout'

type PageModule = Promise<{ default: ComponentType }>

/** Route whose page is loaded on demand (code splitting) and optionally limited to roles. */
const page = (path: string, load: () => PageModule, roles?: Role[]): RouteObject => ({
  path,
  lazy: async () => {
    const { default: Page } = await load()
    return {
      Component: roles
        ? () => (
            <RequireRole roles={roles}>
              <Page />
            </RequireRole>
          )
        : Page,
    }
  },
})

export const router = createBrowserRouter([
  { path: '/sign-in', element: <SignInPage />, errorElement: <RouteErrorPage /> },
  {
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    errorElement: <RouteErrorPage />,
    children: [
      page('/', () => import('@/features/catalogue/CataloguePage')),
      page('/equipment/:id', () => import('@/features/catalogue/EquipmentPage')),
      page('/bookings', () => import('@/features/booking/MyBookingsPage')),
      page('/bookings/:id', () => import('@/features/booking/BookingDetailsPage')),
      page('/notifications', () => import('@/features/notifications/NotificationsPage')),
      page('/privacy', () => import('@/pages/privacy/PrivacyPage')),
      page('/approval-queue', () => import('@/features/approvals/ApprovalQueuePage'), ['manager']),
      page('/approval-queue/:id', () => import('@/features/approvals/ApprovalRequestPage'), [
        'manager',
      ]),
      page('/loans', () => import('@/features/loans/LoansPage'), ['administrator']),
      page('/admin/equipment', () => import('@/features/admin/AdminEquipmentPage'), [
        'administrator',
      ]),
      page('/admin/categories', () => import('@/features/admin/AdminCategoriesPage'), [
        'administrator',
      ]),
      page('/admin/users', () => import('@/features/admin/AdminUsersPage'), ['administrator']),
      page('/admin/decision-log', () => import('@/features/admin/DecisionLogPage'), [
        'administrator',
      ]),
      page('/admin/settings', () => import('@/features/admin/SettingsPage'), ['administrator']),
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
