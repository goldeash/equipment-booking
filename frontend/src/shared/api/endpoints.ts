import { request } from './httpClient'
import type {
  AffectedBooking,
  ApprovalQueueItem,
  ApprovalSettings,
  AssessmentLogEntry,
  BookedPeriod,
  BookingDetails,
  BookingSummary,
  Category,
  CodeNameItem,
  CurrentUser,
  EquipmentDetails,
  EquipmentHistoryEvent,
  EquipmentListItem,
  LoanListItem,
  LoanState,
  MaintenanceRecord,
  NamedItem,
  NotificationItem,
  PagedResult,
  RequesterHistory,
  Role,
  Session,
  UserAccount,
} from './types'

export type { AffectedBooking }

export interface PageParams {
  page?: number
  pageSize?: number
}

export interface EquipmentSearch extends PageParams {
  q?: string
  categoryId?: number | null
  locationId?: number | null
  featureIds?: number[]
  availableFrom?: string | null
  availableTo?: string | null
  includeRetired?: boolean
  sort?: string
}

export interface SaveEquipment {
  inventoryNumber: string
  name: string
  description: string | null
  categoryId: number
  locationId: number
  value: number
  conditionCode: string
  isSpecialised: boolean
  featureIds: number[]
}

export interface SaveCategory {
  name: string
  description: string | null
  maxLoanDays: number
  usageGuidelines: string
  isSpecialised: boolean
  autoApprovalEnabled: boolean
}

export interface CreateMaintenance {
  maintenanceTypeCode: string
  startDate: string
  endDate: string | null
  takesOutOfService: boolean
  description: string
  confirmCancellation: boolean
}

export interface SaveUser {
  email?: string
  fullName: string
  role: Role
  departmentId: number
  isActive?: boolean
  password?: string
}

export const authApi = {
  signIn: (email: string, password: string) =>
    request<Session>('/auth/sessions', {
      method: 'POST',
      body: { email, password },
      anonymous: true,
    }),
  refresh: () => request<Session>('/auth/sessions/refresh', { method: 'POST', anonymous: true }),
  signOut: () => request<undefined>('/auth/sessions/current', { method: 'DELETE', anonymous: true }),
  me: () => request<CurrentUser>('/me'),
  acknowledgePrivacyNotice: () =>
    request<CurrentUser>('/me/privacy-notice-acknowledgement', { method: 'PUT' }),
}

export const catalogueApi = {
  search: (params: EquipmentSearch) =>
    request<PagedResult<EquipmentListItem>>('/equipment', { query: { ...params } }),
  get: (id: number) => request<EquipmentDetails>(`/equipment/${id}`),
  bookedPeriods: (id: number, from: string, to: string) =>
    request<BookedPeriod[]>(`/equipment/${id}/booked-periods`, { query: { from, to } }),
  history: (id: number) => request<EquipmentHistoryEvent[]>(`/equipment/${id}/history`),
  create: (body: SaveEquipment) =>
    request<EquipmentDetails>('/equipment', { method: 'POST', body }),
  update: (id: number, body: SaveEquipment) =>
    request<EquipmentDetails>(`/equipment/${id}`, { method: 'PUT', body }),
  retire: (id: number) => request<undefined>(`/equipment/${id}/retirement`, { method: 'POST' }),
  categories: () => request<Category[]>('/categories'),
  createCategory: (body: SaveCategory) =>
    request<Category>('/categories', { method: 'POST', body }),
  updateCategory: (id: number, body: SaveCategory) =>
    request<Category>(`/categories/${id}`, { method: 'PUT', body }),
  locations: () => request<NamedItem[]>('/locations'),
  departments: () => request<NamedItem[]>('/departments'),
  features: () => request<NamedItem[]>('/features'),
  conditions: () => request<CodeNameItem[]>('/item-conditions'),
  maintenanceTypes: () => request<CodeNameItem[]>('/maintenance-types'),
}

export const bookingsApi = {
  create: (body: {
    equipmentItemId: number
    startDate: string
    endDate: string
    purpose: string
  }) => request<BookingDetails>('/bookings', { method: 'POST', body }),
  get: (id: number) => request<BookingDetails>(`/bookings/${id}`),
  mine: (params: PageParams & { status?: string | null }) =>
    request<PagedResult<BookingSummary>>('/me/bookings', { query: { ...params } }),
  changeDates: (id: number, startDate: string, endDate: string) =>
    request<BookingDetails>(`/bookings/${id}`, { method: 'PATCH', body: { startDate, endDate } }),
  cancel: (id: number) =>
    request<BookingDetails>(`/bookings/${id}/cancellation`, { method: 'POST' }),
}

export const approvalsApi = {
  queue: (params: PageParams) =>
    request<PagedResult<ApprovalQueueItem>>('/approval-queue', { query: { ...params } }),
  decide: (id: number, outcome: 'approved' | 'declined', comment: string | null) =>
    request<BookingDetails>(`/bookings/${id}/decisions`, {
      method: 'POST',
      body: { outcome, comment },
    }),
  requesterHistory: (id: number) => request<RequesterHistory>(`/bookings/${id}/requester-history`),
  assessments: (params: PageParams & { outcome?: string | null }) =>
    request<PagedResult<AssessmentLogEntry>>('/approval-assessments', { query: { ...params } }),
}

export const loansApi = {
  list: (state: LoanState, params: PageParams) =>
    request<PagedResult<LoanListItem>>('/loans', { query: { state, ...params } }),
  issue: (bookingId: number) =>
    request<BookingDetails>(`/bookings/${bookingId}/loan`, { method: 'POST' }),
  return: (bookingId: number, conditionCode: string, comment: string | null) =>
    request<BookingDetails>(`/bookings/${bookingId}/loan/return`, {
      method: 'PUT',
      body: { conditionCode, comment },
    }),
}

export const maintenanceApi = {
  list: (itemId: number) =>
    request<MaintenanceRecord[]>(`/equipment/${itemId}/maintenance-records`),
  create: (itemId: number, body: CreateMaintenance) =>
    request<MaintenanceRecord>(`/equipment/${itemId}/maintenance-records`, {
      method: 'POST',
      body,
    }),
}

export const notificationsApi = {
  list: (params: PageParams & { unreadOnly?: boolean }) =>
    request<PagedResult<NotificationItem>>('/me/notifications', { query: { ...params } }),
  unreadCount: () => request<{ count: number }>('/me/notifications/unread-count'),
  markRead: (id: number) => request<undefined>(`/me/notifications/${id}/read-state`, { method: 'PUT' }),
  markAllRead: () => request<undefined>('/me/notifications/read-state', { method: 'PUT' }),
}

export const adminApi = {
  settings: () => request<ApprovalSettings>('/settings/approval'),
  updateSettings: (body: Omit<ApprovalSettings, 'timeZone'>) =>
    request<ApprovalSettings>('/settings/approval', { method: 'PUT', body }),
  users: (params: PageParams & { q?: string; role?: string | null }) =>
    request<PagedResult<UserAccount>>('/users', { query: { ...params } }),
  createUser: (body: SaveUser) => request<UserAccount>('/users', { method: 'POST', body }),
  updateUser: (id: number, body: SaveUser) =>
    request<UserAccount>(`/users/${id}`, { method: 'PUT', body }),
  setPassword: (id: number, password: string) =>
    request<undefined>(`/users/${id}/password`, { method: 'PUT', body: { password } }),
}
