import { AppShell, Burger, Group, Text } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconBriefcase } from '@tabler/icons-react'
import { Outlet } from 'react-router'

/** Application frame with a header and a collapsible navigation bar on small screens. */
export function AppLayout() {
  const [navigationOpened, { toggle }] = useDisclosure()

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { mobile: !navigationOpened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" gap="sm">
          <Burger
            opened={navigationOpened}
            onClick={toggle}
            hiddenFrom="sm"
            size="sm"
            aria-label="Toggle navigation"
          />
          <IconBriefcase aria-hidden size={24} />
          <Text fw={700}>Equipment Booking</Text>
        </Group>
      </AppShell.Header>
      <AppShell.Navbar p="sm" aria-label="Main navigation" />
      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  )
}
