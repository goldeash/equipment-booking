import { createBrowserRouter } from 'react-router'
import { NotFoundPage } from '@/pages/errors/ErrorPage'
import { RouteErrorPage } from '@/pages/errors/RouteErrorPage'
import { AppLayout } from './layouts/AppLayout'

export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    errorElement: <RouteErrorPage />,
    children: [{ path: '*', element: <NotFoundPage /> }],
  },
])
