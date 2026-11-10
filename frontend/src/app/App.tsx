import { MantineProvider } from '@mantine/core'
import { DatesProvider } from '@mantine/dates'
import { Notifications } from '@mantine/notifications'
import { QueryClientProvider } from '@tanstack/react-query'
import { useState } from 'react'
import { RouterProvider } from 'react-router'
import { createQueryClient } from './queryClient'
import { router } from './router'
import { theme } from './theme'

export function App() {
  const [queryClient] = useState(createQueryClient)

  return (
    <MantineProvider theme={theme} defaultColorScheme="light">
      <DatesProvider settings={{ firstDayOfWeek: 1 }}>
        <Notifications position="top-right" />
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />
        </QueryClientProvider>
      </DatesProvider>
    </MantineProvider>
  )
}
