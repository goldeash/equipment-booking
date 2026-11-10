import { Anchor, Group, Pagination, SegmentedControl, Stack, Table, Text } from '@mantine/core'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { bookingsApi } from '@/shared/api/endpoints'
import { formatPeriod } from '@/shared/lib/format'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { BookingStatusBadge } from '@/shared/ui/StatusBadge'
import { DecisionText } from './DecisionText'

const filters = [
  { value: 'all', label: 'All' },
  { value: 'requested', label: 'Requested' },
  { value: 'approved', label: 'Approved' },
  { value: 'issued', label: 'On loan' },
  { value: 'overdue', label: 'Overdue' },
  { value: 'returned', label: 'Returned' },
]

/** Bookings of the signed in employee with their status and who decided (US-09). */
export default function MyBookingsPage() {
  const [status, setStatus] = useState('all')
  const [page, setPage] = useState(1)
  const params = { status: status === 'all' ? null : status, page, pageSize: 20 }
  const bookings = useQuery({
    queryKey: ['my-bookings', params],
    queryFn: () => bookingsApi.mine(params),
    placeholderData: keepPreviousData,
  })

  return (
    <>
      <PageHeader title="My bookings" description="Requests and loans of equipment" />
      <SegmentedControl
        mb="md"
        data={filters}
        value={status}
        onChange={(value) => {
          setStatus(value)
          setPage(1)
        }}
        fullWidth
        styles={{ root: { overflowX: 'auto' } }}
      />
      <QueryState
        data={bookings.data}
        isPending={bookings.isPending}
        error={bookings.error}
        onRetry={() => void bookings.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="You have no bookings with this status."
      >
        {(data) => (
          <Stack>
            <Table.ScrollContainer minWidth={640}>
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Item</Table.Th>
                    <Table.Th>Dates</Table.Th>
                    <Table.Th>Status</Table.Th>
                    <Table.Th>Decision</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {data.items.map((booking) => (
                    <Table.Tr key={booking.id}>
                      <Table.Td>
                        <Anchor component={Link} to={`/bookings/${booking.id}`} fw={600}>
                          {booking.item.name}
                        </Anchor>
                        <Text size="xs" c="dimmed">
                          {booking.item.inventoryNumber}
                        </Text>
                      </Table.Td>
                      <Table.Td>{formatPeriod(booking.startDate, booking.endDate)}</Table.Td>
                      <Table.Td>
                        <BookingStatusBadge status={booking.status} />
                      </Table.Td>
                      <Table.Td>
                        <DecisionText decision={booking.decision} />
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
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
