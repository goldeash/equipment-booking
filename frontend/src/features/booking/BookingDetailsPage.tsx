import { Anchor, Button, Card, Grid, Group, List, Modal, Stack, Text, Title } from '@mantine/core'
import { DatePickerInput, type DatesRangeValue } from '@mantine/dates'
import { useDisclosure } from '@mantine/hooks'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { bookingsApi } from '@/shared/api/endpoints'
import type { BookingDetails } from '@/shared/api/types'
import {
  conditionLabels,
  formatDateTime,
  formatPeriod,
  routingReasonLabel,
  toApiDate,
} from '@/shared/lib/format'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { BookingStatusBadge } from '@/shared/ui/StatusBadge'
import { DecisionText } from './DecisionText'

/** Details of one booking with change of dates and cancellation (US-08). */
export default function BookingDetailsPage() {
  const id = Number(useParams().id)
  const queryClient = useQueryClient()
  const booking = useQuery({ queryKey: ['booking', id], queryFn: () => bookingsApi.get(id) })
  const [changeOpened, changeModal] = useDisclosure()

  const refresh = async (details: BookingDetails) => {
    queryClient.setQueryData(['booking', id], details)
    await queryClient.invalidateQueries({ queryKey: ['my-bookings'] })
  }

  const cancel = useMutation({
    mutationFn: () => bookingsApi.cancel(id),
    onSuccess: async (details) => {
      await refresh(details)
      notifySuccess('The booking was cancelled.')
    },
    onError: (error) => notifyError(error, 'Could not cancel the booking'),
  })

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
            title={`Booking #${details.id}`}
            description={formatPeriod(details.startDate, details.endDate)}
            actions={
              <Group>
                {details.canChangeDates && (
                  <Button variant="default" onClick={changeModal.open}>
                    Change dates
                  </Button>
                )}
                {details.canCancel && (
                  <Button
                    color="red"
                    variant="light"
                    loading={cancel.isPending}
                    onClick={() => cancel.mutate()}
                  >
                    Cancel booking
                  </Button>
                )}
              </Group>
            }
          />
          <Grid>
            <Grid.Col span={{ base: 12, md: 6 }}>
              <Card withBorder>
                <Stack gap="sm">
                  <Group justify="space-between">
                    <Anchor component={Link} to={`/equipment/${details.item.id}`} fw={600}>
                      {details.item.name}
                    </Anchor>
                    <BookingStatusBadge status={details.status} />
                  </Group>
                  <Text size="sm" c="dimmed">
                    {details.item.inventoryNumber}, {details.item.categoryName}
                  </Text>
                  {details.requesterName && (
                    <Text size="sm">Requested by {details.requesterName}</Text>
                  )}
                  <Text size="sm">Purpose: {details.purpose}</Text>
                  <Text size="sm" c="dimmed">
                    Requested on {formatDateTime(details.createdAt)}
                  </Text>
                </Stack>
              </Card>
            </Grid.Col>
            <Grid.Col span={{ base: 12, md: 6 }}>
              <Stack>
                <Card withBorder>
                  <Title order={5} mb="xs">
                    Decision
                  </Title>
                  <DecisionText decision={details.decision} />
                  {details.decision?.comment && (
                    <Text size="sm" mt="xs">
                      Comment: {details.decision.comment}
                    </Text>
                  )}
                  {details.decision === null && details.routing?.outcome === 'routed' && (
                    <>
                      <Text size="sm" mt="xs">
                        Sent to the department manager because:
                      </Text>
                      <List size="sm">
                        {details.routing.reasons.map((reason) => (
                          <List.Item key={reason}>{routingReasonLabel(reason)}</List.Item>
                        ))}
                      </List>
                    </>
                  )}
                </Card>
                {details.loan && (
                  <Card withBorder>
                    <Title order={5} mb="xs">
                      Loan
                    </Title>
                    <Text size="sm">Issued: {formatDateTime(details.loan.issuedAt)}</Text>
                    <Text size="sm">Due: {formatDateTime(details.loan.dueAt)}</Text>
                    {details.loan.returnedAt && (
                      <Text size="sm">
                        Returned: {formatDateTime(details.loan.returnedAt)}
                        {details.loan.returnConditionCode &&
                          ` in condition ${conditionLabels[details.loan.returnConditionCode].toLowerCase()}`}
                      </Text>
                    )}
                  </Card>
                )}
              </Stack>
            </Grid.Col>
          </Grid>
          <ChangeDatesModal
            booking={details}
            opened={changeOpened}
            onClose={changeModal.close}
            onChanged={refresh}
          />
        </>
      )}
    </QueryState>
  )
}

interface ChangeDatesModalProps {
  booking: BookingDetails
  opened: boolean
  onClose: () => void
  onChanged: (details: BookingDetails) => Promise<void>
}

function ChangeDatesModal({ booking, opened, onClose, onChanged }: ChangeDatesModalProps) {
  const [dates, setDates] = useState<DatesRangeValue<string>>([booking.startDate, booking.endDate])
  const change = useMutation({
    mutationFn: () =>
      bookingsApi.changeDates(booking.id, toApiDate(dates[0]) ?? '', toApiDate(dates[1]) ?? ''),
    onSuccess: async (details) => {
      await onChanged(details)
      notifySuccess('The new dates were requested.')
      onClose()
    },
    onError: (error) => notifyError(error, 'Could not change the dates'),
  })

  return (
    <Modal opened={opened} onClose={onClose} title="Change dates">
      <Stack>
        <Text size="sm" c="dimmed">
          The request is checked again and may need a new decision.
        </Text>
        <DatePickerInput
          type="range"
          label="New dates"
          minDate={new Date()}
          value={dates}
          onChange={setDates}
        />
        <Button
          disabled={!dates[0] || !dates[1]}
          loading={change.isPending}
          onClick={() => change.mutate()}
        >
          Request new dates
        </Button>
      </Stack>
    </Modal>
  )
}
