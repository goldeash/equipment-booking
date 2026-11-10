import dayjs from 'dayjs'
import type { BookingStatus, ConditionCode, ItemStatus } from '@/shared/api/types'

export const DATE_FORMAT = 'YYYY-MM-DD'

export const formatDate = (value: string | null | undefined): string =>
  value ? dayjs(value).format('D MMM YYYY') : 'Not set'

export const formatDateTime = (value: string | null | undefined): string =>
  value ? dayjs(value).format('D MMM YYYY, HH:mm') : 'Not set'

export const formatPeriod = (start: string, end: string | null): string =>
  end === null
    ? `from ${formatDate(start)}`
    : start === end
      ? formatDate(start)
      : `${formatDate(start)} to ${formatDate(end)}`

/** Converts a date picker value to the API date format (YYYY-MM-DD). */
export const toApiDate = (value: Date | string | null): string | null =>
  value === null ? null : dayjs(value).format(DATE_FORMAT)

export const today = (): string => dayjs().format(DATE_FORMAT)

const money = new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' })

export const formatMoney = (value: number): string => money.format(value)

export const bookingStatusLabels: Record<BookingStatus, string> = {
  requested: 'Requested',
  approved: 'Approved',
  declined: 'Declined',
  cancelled: 'Cancelled',
  issued: 'Issued',
  returned: 'Returned',
  overdue: 'Overdue',
}

export const bookingStatusColors: Record<BookingStatus, string> = {
  requested: 'yellow',
  approved: 'teal',
  declined: 'red',
  cancelled: 'gray',
  issued: 'blue',
  returned: 'dark',
  overdue: 'red',
}

export const itemStatusLabels: Record<ItemStatus, string> = {
  available: 'Available',
  out_of_service: 'Out of service',
  retired: 'Retired',
}

export const itemStatusColors: Record<ItemStatus, string> = {
  available: 'teal',
  out_of_service: 'orange',
  retired: 'gray',
}

export const conditionLabels: Record<ConditionCode, string> = {
  new: 'New',
  good: 'Good',
  fair: 'Fair',
  damaged: 'Damaged',
}

const routingReasonLabels: Record<string, string> = {
  high_value: 'High value item',
  specialised: 'Specialised item',
  duration_exceeded: 'Longer than the category limit',
  org_auto_approval_off: 'Automatic approval switched off',
  category_auto_approval_off: 'Automatic approval switched off for the category',
  purpose_not_confirmed: 'Purpose not confirmed by the assistant',
  assistant_unavailable: 'AI assistant not available',
}

export const routingReasonLabel = (code: string): string => routingReasonLabels[code] ?? code
