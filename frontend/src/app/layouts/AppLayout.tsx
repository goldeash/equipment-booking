import {
  ActionIcon,
  AppShell,
  Burger,
  Group,
  Indicator,
  Menu,
  NavLink,
  Text,
  UnstyledButton,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import {
  IconBell,
  IconBriefcase,
  IconChevronDown,
  IconLogout,
  IconShieldLock,
} from '@tabler/icons-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Outlet, useLocation, useNavigate } from 'react-router'
import { PrivacyNoticeModal } from '@/features/auth/PrivacyNoticeModal'
import { signOut, useAuthStore } from '@/features/auth/authStore'
import { notificationsApi } from '@/shared/api/endpoints'
import { navigationItems } from '../navigation'

const roleLabels = {
  employee: 'Employee',
  manager: 'Department manager',
  administrator: 'Administrator',
}

/** Application frame: header with notifications and user menu, role based navigation, page content. */
export function AppLayout() {
  const [navigationOpened, { toggle, close }] = useDisclosure()
  const user = useAuthStore((state) => state.user)
  const location = useLocation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const unread = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: notificationsApi.unreadCount,
    refetchInterval: 60_000,
    enabled: user !== null,
  })

  const items = navigationItems.filter((item) => user !== null && item.roles.includes(user.role))
  const isActive = (to: string) =>
    to === '/' ? location.pathname === '/' : location.pathname.startsWith(to)

  const handleSignOut = async () => {
    await signOut()
    queryClient.clear()
    await navigate('/sign-in', { replace: true })
  }

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { mobile: !navigationOpened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group gap="sm" wrap="nowrap">
            <Burger
              opened={navigationOpened}
              onClick={toggle}
              hiddenFrom="sm"
              size="sm"
              aria-label="Toggle navigation"
            />
            <IconBriefcase aria-hidden size={24} />
            <Text fw={700} visibleFrom="xs">
              Equipment Booking
            </Text>
          </Group>
          <Group gap="xs" wrap="nowrap">
            <Indicator
              label={unread.data?.count}
              size={18}
              disabled={!unread.data?.count}
              offset={4}
            >
              <ActionIcon
                component={Link}
                to="/notifications"
                variant="subtle"
                size="lg"
                aria-label="Notifications"
              >
                <IconBell size={20} />
              </ActionIcon>
            </Indicator>
            {user && (
              <Menu position="bottom-end" withinPortal>
                <Menu.Target>
                  <UnstyledButton aria-label="User menu">
                    <Group gap={6} wrap="nowrap">
                      <div>
                        <Text size="sm" fw={600} lh={1.2}>
                          {user.fullName}
                        </Text>
                        <Text size="xs" c="dimmed" lh={1.2} visibleFrom="xs">
                          {roleLabels[user.role]}, {user.departmentName}
                        </Text>
                      </div>
                      <IconChevronDown size={14} aria-hidden />
                    </Group>
                  </UnstyledButton>
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Item
                    component={Link}
                    to="/privacy"
                    leftSection={<IconShieldLock size={16} />}
                  >
                    Privacy notice
                  </Menu.Item>
                  <Menu.Item
                    color="red"
                    leftSection={<IconLogout size={16} />}
                    onClick={() => void handleSignOut()}
                  >
                    Sign out
                  </Menu.Item>
                </Menu.Dropdown>
              </Menu>
            )}
          </Group>
        </Group>
      </AppShell.Header>
      <AppShell.Navbar p="sm" aria-label="Main navigation">
        {items.map((item) => (
          <NavLink
            key={item.to}
            component={Link}
            to={item.to}
            label={item.label}
            leftSection={<item.icon size={18} aria-hidden />}
            active={isActive(item.to)}
            onClick={close}
          />
        ))}
      </AppShell.Navbar>
      <AppShell.Main>
        <Outlet />
        <PrivacyNoticeModal />
      </AppShell.Main>
    </AppShell>
  )
}
