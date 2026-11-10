import {
  Anchor,
  Button,
  Group,
  Modal,
  Pagination,
  Select,
  Stack,
  Table,
  Tabs,
  Text,
  Textarea,
} from '@mantine/core'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { useConditions } from '@/features/catalogue/useReferenceData'
import { loansApi } from '@/shared/api/endpoints'
import type { LoanListItem, LoanState } from '@/shared/api/types'
import { formatDateTime, formatPeriod } from '@/shared/lib/format'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { BookingStatusBadge } from '@/shared/ui/StatusBadge'

/** Issue and return of items by administrators (US-13, US-14, US-15). */
export default function LoansPage() {
  const [state, setState] = useState<LoanState>('ready')
  const [page, setPage] = useState(1)
  const [returning, setReturning] = useState<LoanListItem | null>(null)
  const queryClient = useQueryClient()
  const loans = useQuery({
    queryKey: ['loans', state, page],
    queryFn: () => loansApi.list(state, { page, pageSize: 20 }),
    placeholderData: keepPreviousData,
  })

  const issue = useMutation({
    mutationFn: loansApi.issue,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['loans'] })
      notifySuccess('The item was issued.')
    },
    onError: (error) => notifyError(error, 'Could not issue the item'),
  })

  return (
    <>
      <PageHeader title="Loans" description="Hand out and receive equipment" />
      <Tabs
        value={state}
        onChange={(value) => {
          setState((value as LoanState | null) ?? 'ready')
          setPage(1)
        }}
        mb="md"
      >
        <Tabs.List>
          <Tabs.Tab value="ready">Ready to issue</Tabs.Tab>
          <Tabs.Tab value="active">On loan</Tabs.Tab>
          <Tabs.Tab value="overdue">Overdue</Tabs.Tab>
        </Tabs.List>
      </Tabs>
      <QueryState
        data={loans.data}
        isPending={loans.isPending}
        error={loans.error}
        onRetry={() => void loans.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="Nothing in this list."
      >
        {(data) => (
          <Stack>
            <Table.ScrollContainer minWidth={760}>
              <Table striped>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Item</Table.Th>
                    <Table.Th>Employee</Table.Th>
                    <Table.Th>Dates</Table.Th>
                    <Table.Th>Due</Table.Th>
                    <Table.Th>Status</Table.Th>
                    <Table.Th />
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {data.items.map((loan) => (
                    <Table.Tr key={loan.bookingId}>
                      <Table.Td>
                        <Anchor component={Link} to={`/bookings/${loan.bookingId}`} fw={600}>
                          {loan.item.name}
                        </Anchor>
                        <Text size="xs" c="dimmed">
                          {loan.item.inventoryNumber}
                        </Text>
                      </Table.Td>
                      <Table.Td>{loan.requesterName ?? 'Anonymised'}</Table.Td>
                      <Table.Td>{formatPeriod(loan.startDate, loan.endDate)}</Table.Td>
                      <Table.Td>{loan.dueAt ? formatDateTime(loan.dueAt) : 'Not issued'}</Table.Td>
                      <Table.Td>
                        <BookingStatusBadge status={loan.status} />
                      </Table.Td>
                      <Table.Td>
                        {loan.status === 'approved' ? (
                          <Button
                            size="xs"
                            loading={issue.isPending && issue.variables === loan.bookingId}
                            onClick={() => issue.mutate(loan.bookingId)}
                          >
                            Issue
                          </Button>
                        ) : (
                          <Button size="xs" variant="light" onClick={() => setReturning(loan)}>
                            Record return
                          </Button>
                        )}
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
      {returning && <ReturnModal loan={returning} onClose={() => setReturning(null)} />}
    </>
  )
}

function ReturnModal({ loan, onClose }: { loan: LoanListItem; onClose: () => void }) {
  const queryClient = useQueryClient()
  const conditions = useConditions()
  const [condition, setCondition] = useState<string | null>('good')
  const [comment, setComment] = useState('')
  const needsComment = condition === 'damaged' && !comment.trim()
  const save = useMutation({
    mutationFn: () => loansApi.return(loan.bookingId, condition ?? 'good', comment.trim() || null),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['loans'] })
      notifySuccess('The return was recorded.')
      onClose()
    },
    onError: (error) => notifyError(error, 'Could not record the return'),
  })

  return (
    <Modal opened onClose={onClose} title={`Return of ${loan.item.name}`}>
      <Stack>
        <Select
          label="Condition"
          data={(conditions.data ?? []).map((item) => ({ value: item.code, label: item.name }))}
          value={condition}
          onChange={setCondition}
          allowDeselect={false}
        />
        <Textarea
          label="Comment"
          description="Required when the item is damaged"
          autosize
          minRows={2}
          maxLength={1000}
          value={comment}
          error={needsComment ? 'Describe the damage.' : undefined}
          onChange={(event) => setComment(event.currentTarget.value)}
        />
        <Button disabled={needsComment} loading={save.isPending} onClick={() => save.mutate()}>
          Record return
        </Button>
      </Stack>
    </Modal>
  )
}
