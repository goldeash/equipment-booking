import { MantineProvider } from '@mantine/core'
import { DatesProvider } from '@mantine/dates'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, type RenderResult } from '@testing-library/react'
import type { ReactElement } from 'react'
import { createMemoryRouter, RouterProvider } from 'react-router'

/** Renders a component with the providers of the application and a memory router. */
export function renderWithProviders(ui: ReactElement, path = '/'): RenderResult {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  const router = createMemoryRouter(
    [
      { path, element: ui },
      { path: '*', element: <div>Navigated away</div> },
    ],
    { initialEntries: [path] },
  )
  return render(
    <MantineProvider>
      <DatesProvider settings={{}}>
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />
        </QueryClientProvider>
      </DatesProvider>
    </MantineProvider>,
  )
}
