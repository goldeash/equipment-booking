import { Anchor, Badge, Group, Pagination, Stack, Table, Text } from '@mantine/core'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { approvalsApi } from '@/shared/api/endpoints'
import { formatDateTime, formatMoney, formatPeriod } from '@/shared/lib/format'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'

/** Requests of the manager's department that need a manual decision, oldest first (US-10). */
export default function ApprovalQueuePage() {
  const [page, setPage] = useState(1)
  const queue = useQuery({
    queryKey: ['approval-queue', page],
    queryFn: () => approvalsApi.queue({ page, pageSize: 20 }),
    placeholderData: keepPreviousData,
  })

  return (
    <>
      <PageHeader
        title="Approval queue"
        description="Requests of your department routed for your decision"
      />
      <QueryState
        data={queue.data}
        isPending={queue.isPending}
        error={queue.error}
        onRetry={() => void queue.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="No requests are waiting for your decision."
      >
        {(data) => (
          <Stack>
            <Table.ScrollContainer minWidth={760}>
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Requested</Table.Th>
                    <Table.Th>Employee</Table.Th>
                    <Table.Th>Item</Table.Th>
                    <Table.Th>Dates</Table.Th>
                    <Table.Th>Reasons for routing</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {data.items.map((request) => (
                    <Table.Tr key={request.bookingId}>
                      <Table.Td>{formatDateTime(request.requestedAt)}</Table.Td>
                      <Table.Td>{request.requesterName}</Table.Td>
                      <Table.Td>
                        <Anchor
                          component={Link}
                          to={`/approval-queue/${request.bookingId}`}
                          fw={600}
                        >
                          {request.item.name}
                        </Anchor>
                        <Text size="xs" c="dimmed">
                          {request.item.inventoryNumber}, {formatMoney(request.itemValue)}
                        </Text>
                      </Table.Td>
                      <Table.Td>{formatPeriod(request.startDate, request.endDate)}</Table.Td>
                      <Table.Td>
                        <Group gap={4}>
                          {request.reasons.map((reason) => (
                            <Badge key={reason.code} variant="light" color="orange" size="sm">
                              {reason.name}
                            </Badge>
                          ))}
                        </Group>
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
