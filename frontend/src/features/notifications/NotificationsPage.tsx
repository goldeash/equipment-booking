import { Anchor, Button, Card, Group, Pagination, Stack, Switch, Text } from '@mantine/core'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { notificationsApi } from '@/shared/api/endpoints'
import { formatDateTime } from '@/shared/lib/format'
import { notifyError } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'

/** In-app notifications about decisions, reminders and overdue loans (US-17 to US-19). */
export default function NotificationsPage() {
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [page, setPage] = useState(1)
  const queryClient = useQueryClient()
  const list = useQuery({
    queryKey: ['notifications', unreadOnly, page],
    queryFn: () => notificationsApi.list({ unreadOnly, page, pageSize: 20 }),
    placeholderData: keepPreviousData,
  })
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['notifications'] })
  const markRead = useMutation({
    mutationFn: notificationsApi.markRead,
    onSuccess: invalidate,
    onError: (error) => notifyError(error),
  })
  const markAll = useMutation({
    mutationFn: notificationsApi.markAllRead,
    onSuccess: invalidate,
    onError: (error) => notifyError(error),
  })

  return (
    <>
      <PageHeader
        title="Notifications"
        actions={
          <Group>
            <Switch
              label="Unread only"
              checked={unreadOnly}
              onChange={(event) => {
                setUnreadOnly(event.currentTarget.checked)
                setPage(1)
              }}
            />
            <Button
              variant="default"
              size="xs"
              loading={markAll.isPending}
              onClick={() => markAll.mutate()}
            >
              Mark all as read
            </Button>
          </Group>
        }
      />
      <QueryState
        data={list.data}
        isPending={list.isPending}
        error={list.error}
        onRetry={() => void list.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="No notifications."
      >
        {(data) => (
          <Stack>
            {data.items.map((notification) => (
              <Card
                key={notification.id}
                withBorder
                bg={notification.readAt ? undefined : 'blue.0'}
              >
                <Group justify="space-between" align="flex-start" wrap="nowrap">
                  <Stack gap={2}>
                    <Text fw={600}>{notification.title}</Text>
                    <Text size="sm">{notification.body}</Text>
                    <Group gap="xs">
                      <Text size="xs" c="dimmed">
                        {formatDateTime(notification.createdAt)}
                      </Text>
                      {notification.bookingId && (
                        <Anchor
                          component={Link}
                          to={`/bookings/${notification.bookingId}`}
                          size="xs"
                        >
                          Open booking
                        </Anchor>
                      )}
                    </Group>
                  </Stack>
                  {!notification.readAt && (
                    <Button
                      size="xs"
                      variant="subtle"
                      onClick={() => markRead.mutate(notification.id)}
                    >
                      Mark as read
                    </Button>
                  )}
                </Group>
              </Card>
            ))}
            {data.totalPages > 1 && (
              <Group justify="center">
                <Pagination total={data.totalPages} value={page} onChange={setPage} />
              </Group>
            )}
          </Stack>
        )}
      </QueryState>
    </>
  )
}
