/** Types of the Equipment Booking API, matching the OpenAPI document in docs/api/openapi.json. */

export type Role = 'employee' | 'manager' | 'administrator'

export type BookingStatus =
  'requested' | 'approved' | 'declined' | 'cancelled' | 'issued' | 'returned' | 'overdue'

export type ItemStatus = 'available' | 'out_of_service' | 'retired'

export type ConditionCode = 'new' | 'good' | 'fair' | 'damaged'

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface CurrentUser {
  id: number
  email: string
  fullName: string
  role: Role
  departmentId: number
  departmentName: string
  privacyNoticeAcknowledged: boolean
}

export interface Session {
  accessToken: string
  accessTokenExpiresAt: string
  user: CurrentUser
}

export interface NamedItem {
  id: number
  name: string
}

export interface CodeNameItem {
  code: string
  name: string
}

export interface EquipmentListItem {
  id: number
  inventoryNumber: string
  name: string
  categoryId: number
  categoryName: string
  locationId: number
  locationName: string
  value: number
  conditionCode: ConditionCode
  status: ItemStatus
  isSpecialised: boolean
  features: string[]
}

export interface CategorySummary {
  id: number
  name: string
  maxLoanDays: number
  usageGuidelines: string
  isSpecialised: boolean
}

export interface EquipmentDetails {
  id: number
  inventoryNumber: string
  name: string
  description: string | null
  category: CategorySummary
  location: NamedItem
  value: number
  conditionCode: ConditionCode
  status: ItemStatus
  isSpecialised: boolean
  features: NamedItem[]
  retiredAt: string | null
}

export interface BookedPeriod {
  startDate: string
  endDate: string | null
  kind: 'booking' | 'maintenance'
}

export interface EquipmentHistoryEvent {
  occurredAt: string
  kind: string
  summary: string
  bookingId: number | null
}

export interface Category {
  id: number
  name: string
  description: string | null
  maxLoanDays: number
  usageGuidelines: string
  isSpecialised: boolean
  autoApprovalEnabled: boolean
}

export interface BookingItemInfo {
  id: number
  inventoryNumber: string
  name: string
  categoryName: string
}

export interface DecisionInfo {
  source: 'manager' | 'ai_assistant'
  outcome: 'approved' | 'declined'
  decidedByName: string | null
  comment: string | null
  decidedAt: string
}

export interface LoanInfo {
  issuedAt: string
  dueAt: string
  returnedAt: string | null
  returnConditionCode: ConditionCode | null
  returnComment: string | null
}

export interface RoutingInfo {
  outcome: 'pending' | 'approved' | 'routed'
  purposeResult: string
  reasons: string[]
  summary: string | null
  assessedAt: string
}

export interface BookingSummary {
  id: number
  item: BookingItemInfo
  startDate: string
  endDate: string
  status: BookingStatus
  decision: DecisionInfo | null
  createdAt: string
}

export interface BookingDetails {
  id: number
  item: BookingItemInfo
  requesterId: number | null
  requesterName: string | null
  startDate: string
  endDate: string
  purpose: string
  status: BookingStatus
  decision: DecisionInfo | null
  loan: LoanInfo | null
  routing: RoutingInfo | null
  createdAt: string
  canCancel: boolean
  canChangeDates: boolean
}

export interface ApprovalQueueItem {
  bookingId: number
  item: BookingItemInfo
  itemValue: number
  requesterId: number
  requesterName: string
  startDate: string
  endDate: string
  purpose: string
  reasons: CodeNameItem[]
  summary: string | null
  requestedAt: string
}

export interface RequesterHistory {
  requesterId: number
  requesterName: string
  loanCount: number
  lateReturnCount: number
  lastLateReturnAt: string | null
}

export type LoanState = 'ready' | 'active' | 'overdue'

export interface LoanListItem {
  bookingId: number
  item: BookingItemInfo
  requesterName: string | null
  startDate: string
  endDate: string
  status: BookingStatus
  issuedAt: string | null
  dueAt: string | null
}

export interface MaintenanceRecord {
  id: number
  maintenanceTypeCode: string
  startDate: string
  endDate: string | null
  takesOutOfService: boolean
  description: string
  recordedByName: string
  createdAt: string
}

export interface AffectedBooking {
  bookingId: number
  startDate: string
  endDate: string
  status: BookingStatus
  requesterName: string | null
}

export interface NotificationItem {
  id: number
  type: string
  title: string
  body: string
  bookingId: number | null
  createdAt: string
  readAt: string | null
}

export interface ApprovalSettings {
  lowValueThreshold: number
  autoApprovalEnabled: boolean
  retentionDays: number
  returnTime: string
  timeZone: string
}

export interface UserAccount {
  id: number
  email: string
  fullName: string
  role: Role
  departmentId: number
  departmentName: string
  isActive: boolean
  createdAt: string
}

export interface AssessmentLogEntry {
  id: number
  bookingId: number
  itemName: string
  inventoryNumber: string
  assessedAt: string
  valueCheckPassed: boolean
  specialisedCheckPassed: boolean
  durationCheckPassed: boolean
  organisationAutoApprovalEnabled: boolean
  categoryAutoApprovalEnabled: boolean
  purposeResult: string
  outcome: string
  reasons: string[]
}
