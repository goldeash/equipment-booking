import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Button, Stack, Text, Textarea } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { IconInfoCircle } from '@tabler/icons-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useNavigate } from 'react-router'
import { getErrorMessage, isApiError } from '@/shared/api/apiError'
import { bookingsApi } from '@/shared/api/endpoints'
import type { BookedPeriod } from '@/shared/api/types'
import { formatPeriod } from '@/shared/lib/format'
import { notifySuccess } from '@/shared/lib/notify'
import { bookingSchema, PURPOSE_MAX, type BookingFormValues } from './bookingSchema'

interface BookingFormProps {
  equipmentItemId: number
  bookedPeriods: BookedPeriod[]
  /** Dates chosen in the catalogue filter, used as the initial value. */
  initialRange?: [string, string]
}

const isBlocked = (date: string, periods: BookedPeriod[]) =>
  periods.some(
    (period) =>
      !dayjs(date).isBefore(period.startDate, 'day') &&
      (period.endDate === null || !dayjs(date).isAfter(period.endDate, 'day')),
  )

/** Booking in four actions: item (already chosen), dates, purpose, confirm (NFR-08). */
export function BookingForm({ equipmentItemId, bookedPeriods, initialRange }: BookingFormProps) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [conflicts, setConflicts] = useState<{ startDate: string; endDate: string }[]>([])
  const {
    control,
    register,
    handleSubmit,
    setError,
    setValue,
    formState: { errors },
  } = useForm<BookingFormValues>({
    resolver: zodResolver(bookingSchema),
    defaultValues: {
      startDate: initialRange?.[0] ?? '',
      endDate: initialRange?.[1] ?? '',
      purpose: '',
    },
  })

  const create = useMutation({
    mutationFn: (values: BookingFormValues) =>
      bookingsApi.create({ equipmentItemId, ...values, purpose: values.purpose.trim() }),
    onSuccess: async (booking) => {
      await queryClient.invalidateQueries({ queryKey: ['equipment'] })
      notifySuccess('Your request was sent.')
      await navigate(`/bookings/${booking.id}`)
    },
    onError: (error) => {
      if (isApiError(error) && error.code === 'booking.overlap') {
        setConflicts(
          (error.problem.conflictingPeriods as { startDate: string; endDate: string }[]) ?? [],
        )
      }
      setError('startDate', { message: getErrorMessage(error) })
    },
  })

  const purposeLength = useWatch({ control, name: 'purpose' }).length
  const endDate = useWatch({ control, name: 'endDate' })

  return (
    <form onSubmit={handleSubmit((values) => create.mutate(values))} noValidate>
      <Stack>
        <Controller
          control={control}
          name="startDate"
          render={({ field }) => (
            <DatePickerInput
              type="range"
              label="Dates"
              placeholder="Choose the first and the last day"
              minDate={new Date()}
              excludeDate={(date) => isBlocked(date, bookedPeriods)}
              value={[field.value || null, endDate || null]}
              onChange={([start, end]) => {
                setConflicts([])
                field.onChange(start ?? '')
                setValue('endDate', end ?? '')
              }}
              error={errors.startDate?.message ?? errors.endDate?.message}
              required
            />
          )}
        />
        {conflicts.length > 0 && (
          <Text size="sm" c="red">
            Already booked:{' '}
            {conflicts.map((period) => formatPeriod(period.startDate, period.endDate)).join(', ')}
          </Text>
        )}
        <Textarea
          label="Purpose"
          description={`What will you use the item for? ${purposeLength}/${PURPOSE_MAX}`}
          autosize
          minRows={3}
          maxLength={PURPOSE_MAX}
          error={errors.purpose?.message}
          required
          {...register('purpose')}
        />
        <Alert variant="light" color="blue" icon={<IconInfoCircle size={18} />}>
          Do not enter personal data such as names or health information in the purpose.
        </Alert>
        <Button type="submit" loading={create.isPending}>
          Request this item
        </Button>
      </Stack>
    </form>
  )
}
