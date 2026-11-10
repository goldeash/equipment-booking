import { Button, Card, Grid, Group, List, Stack, Text, Textarea, Title } from '@mantine/core'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router'
import { approvalsApi, bookingsApi } from '@/shared/api/endpoints'
import { formatDate, formatDateTime, formatPeriod, routingReasonLabel } from '@/shared/lib/format'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { BookingStatusBadge } from '@/shared/ui/StatusBadge'

/** Decision of the department manager with the reasons for routing and the requester's return history (US-11, US-12). */
export default function ApprovalRequestPage() {
  const id = Number(useParams().id)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [comment, setComment] = useState('')
  const [commentError, setCommentError] = useState<string | null>(null)
  const booking = useQuery({ queryKey: ['booking', id], queryFn: () => bookingsApi.get(id) })
  const history = useQuery({
    queryKey: ['requester-history', id],
    queryFn: () => approvalsApi.requesterHistory(id),
  })

  const decide = useMutation({
    mutationFn: (outcome: 'approved' | 'declined') =>
      approvalsApi.decide(id, outcome, comment.trim() || null),
    onSuccess: async (details) => {
      await queryClient.invalidateQueries({ queryKey: ['approval-queue'] })
      notifySuccess(
        details.status === 'approved' ? 'The request was approved.' : 'The request was declined.',
      )
      await navigate('/approval-queue')
    },
    onError: (error) => notifyError(error, 'Could not save the decision'),
  })

  const decline = () => {
    if (!comment.trim()) {
      setCommentError('A comment is required to decline a request.')
      return
    }
    decide.mutate('declined')
  }

  return (
    <QueryState
      data={booking.data}
      isPending={booking.isPending}
      error={booking.error}
      onRetry={() => void booking.refetch()}
    >
      {(details) => (
        <>
          <PageHeader
            title={details.item.name}
            description={`Request #${details.id} by ${details.requesterName ?? 'an employee'}`}
            actions={<BookingStatusBadge status={details.status} />}
          />
          <Grid>
            <Grid.Col span={{ base: 12, md: 7 }}>
              <Card withBorder>
                <Stack gap="xs">
                  <Text>
                    <b>Dates:</b> {formatPeriod(details.startDate, details.endDate)}
                  </Text>
                  <Text>
                    <b>Purpose:</b> {details.purpose}
                  </Text>
                  <Text size="sm" c="dimmed">
                    Requested on {formatDateTime(details.createdAt)}
                  </Text>
                  {details.routing && (
                    <>
                      <Text fw={600} mt="sm">
                        Why you decide this request
                      </Text>
                      <List size="sm">
                        {details.routing.reasons.map((reason) => (
                          <List.Item key={reason}>{routingReasonLabel(reason)}</List.Item>
                        ))}
                      </List>
                      {details.routing.summary && (
                        <Text size="sm" mt="xs">
                          Summary by the AI assistant: {details.routing.summary}
                        </Text>
                      )}
                    </>
                  )}
                </Stack>
              </Card>
            </Grid.Col>
            <Grid.Col span={{ base: 12, md: 5 }}>
              <Stack>
                <Card withBorder>
                  <Title order={5} mb="xs">
                    Return history of the employee
                  </Title>
                  <QueryState
                    data={history.data}
                    isPending={history.isPending}
                    error={history.error}
                    skeletonRows={2}
                  >
                    {(data) => (
                      <Stack gap={4}>
                        <Text size="sm">Loans: {data.loanCount}</Text>
                        <Text size="sm">Late returns: {data.lateReturnCount}</Text>
                        <Text size="sm">
                          Last late return:{' '}
                          {data.lastLateReturnAt ? formatDate(data.lastLateReturnAt) : 'none'}
                        </Text>
                      </Stack>
                    )}
                  </QueryState>
                </Card>
                {details.status === 'requested' && (
                  <Card withBorder>
                    <Stack>
                      <Textarea
                        label="Comment"
                        description="Required when you decline"
                        autosize
                        minRows={2}
                        maxLength={1000}
                        value={comment}
                        error={commentError}
                        onChange={(event) => {
                          setComment(event.currentTarget.value)
                          setCommentError(null)
                        }}
                      />
                      <Group grow>
                        <Button
                          color="teal"
                          loading={decide.isPending && decide.variables === 'approved'}
                          onClick={() => decide.mutate('approved')}
                        >
                          Approve
                        </Button>
                        <Button
                          color="red"
                          variant="light"
                          loading={decide.isPending && decide.variables === 'declined'}
                          onClick={decline}
                        >
                          Decline
                        </Button>
                      </Group>
                    </Stack>
                  </Card>
                )}
              </Stack>
            </Grid.Col>
          </Grid>
        </>
      )}
    </QueryState>
  )
}
