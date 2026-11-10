import { Badge } from '@mantine/core'
import type { BookingStatus, ItemStatus } from '@/shared/api/types'
import {
  bookingStatusColors,
  bookingStatusLabels,
  itemStatusColors,
  itemStatusLabels,
} from '@/shared/lib/format'

export function BookingStatusBadge({ status }: { status: BookingStatus }) {
  return (
    <Badge color={bookingStatusColors[status]} variant="light">
      {bookingStatusLabels[status]}
    </Badge>
  )
}

export function ItemStatusBadge({ status }: { status: ItemStatus }) {
  return (
    <Badge color={itemStatusColors[status]} variant="light">
      {itemStatusLabels[status]}
    </Badge>
  )
}
