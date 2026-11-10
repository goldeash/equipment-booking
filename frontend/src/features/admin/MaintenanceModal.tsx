import {
  Alert,
  Button,
  Checkbox,
  Divider,
  List,
  Modal,
  Select,
  Stack,
  Text,
  Textarea,
  Timeline,
} from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useMaintenanceTypes } from '@/features/catalogue/useReferenceData'
import { isApiError } from '@/shared/api/apiError'
import { catalogueApi, maintenanceApi, type AffectedBooking } from '@/shared/api/endpoints'
import type { EquipmentListItem } from '@/shared/api/types'
import { formatDateTime, formatPeriod, toApiDate } from '@/shared/lib/format'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { QueryState } from '@/shared/ui/QueryState'

/** Maintenance records and full history of an item (US-30 to US-32). */
export function MaintenanceModal({
  item,
  onClose,
}: {
  item: EquipmentListItem
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const types = useMaintenanceTypes()
  const history = useQuery({
    queryKey: ['equipment', item.id, 'history'],
    queryFn: () => catalogueApi.history(item.id),
  })
  const [type, setType] = useState<string | null>('inspection')
  const [start, setStart] = useState<string | null>(null)
  const [end, setEnd] = useState<string | null>(null)
  const [outOfService, setOutOfService] = useState(true)
  const [description, setDescription] = useState('')
  const [affected, setAffected] = useState<AffectedBooking[] | null>(null)

  const save = useMutation({
    mutationFn: (confirmCancellation: boolean) =>
      maintenanceApi.create(item.id, {
        maintenanceTypeCode: type ?? 'inspection',
        startDate: toApiDate(start) ?? '',
        endDate: toApiDate(end),
        takesOutOfService: outOfService,
        description: description.trim(),
        confirmCancellation,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['equipment'] })
      notifySuccess('The maintenance was recorded.')
      onClose()
    },
    onError: (error) => {
      if (isApiError(error) && error.code === 'maintenance.confirmation_required') {
        setAffected((error.problem.affectedBookings as AffectedBooking[]) ?? [])
        return
      }
      notifyError(error, 'Could not record the maintenance')
    },
  })

  return (
    <Modal opened onClose={onClose} title={`${item.name} (${item.inventoryNumber})`} size="lg">
      <Stack>
        <Select
          label="Type"
          data={(types.data ?? []).map((entry) => ({ value: entry.code, label: entry.name }))}
          value={type}
          onChange={setType}
          allowDeselect={false}
        />
        <DatePickerInput label="Start date" value={start} onChange={setStart} required />
        <DatePickerInput
          label="End date"
          description="Leave empty while the end is not known"
          value={end}
          onChange={setEnd}
          clearable
          minDate={start ?? undefined}
        />
        <Checkbox
          label="Out of service: the item cannot be booked in this period"
          checked={outOfService}
          onChange={(event) => setOutOfService(event.currentTarget.checked)}
        />
        <Textarea
          label="Description"
          required
          autosize
          minRows={2}
          maxLength={2000}
          value={description}
          onChange={(event) => setDescription(event.currentTarget.value)}
        />
        {affected && (
          <Alert color="orange" title="Bookings in this period">
            <Text size="sm">These bookings will be cancelled and the employees notified:</Text>
            <List size="sm">
              {affected.map((booking) => (
                <List.Item key={booking.bookingId}>
                  {formatPeriod(booking.startDate, booking.endDate)},{' '}
                  {booking.requesterName ?? 'anonymised'}
                </List.Item>
              ))}
            </List>
          </Alert>
        )}
        <Button
          disabled={!start || !description.trim()}
          loading={save.isPending}
          color={affected ? 'orange' : undefined}
          onClick={() => save.mutate(affected !== null)}
        >
          {affected ? 'Cancel these bookings and save' : 'Save maintenance'}
        </Button>
        <Divider label="History" />
        <QueryState
          data={history.data}
          isPending={history.isPending}
          error={history.error}
          isEmpty={(data) => data.length === 0}
          emptyMessage="No history yet."
        >
          {(events) => (
            <Timeline bulletSize={14} lineWidth={2}>
              {events.slice(0, 30).map((event, index) => (
                <Timeline.Item key={`${event.occurredAt}-${index}`} title={event.summary}>
                  <Text size="xs" c="dimmed">
                    {formatDateTime(event.occurredAt)}
                  </Text>
                </Timeline.Item>
              ))}
            </Timeline>
          )}
        </QueryState>
      </Stack>
    </Modal>
  )
}
