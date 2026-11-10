import {
  IconBell,
  IconCalendarEvent,
  IconCategory,
  IconChecklist,
  IconClipboardList,
  IconDeviceLaptop,
  IconListSearch,
  IconSettings,
  IconTruckDelivery,
  IconUsers,
  type Icon,
} from '@tabler/icons-react'
import type { Role } from '@/shared/api/types'

export interface NavigationItem {
  label: string
  to: string
  icon: Icon
  roles: Role[]
}

const everyone: Role[] = ['employee', 'manager', 'administrator']

/** Main navigation; each user sees only the items of the own role. */
export const navigationItems: NavigationItem[] = [
  { label: 'Catalogue', to: '/', icon: IconListSearch, roles: everyone },
  { label: 'My bookings', to: '/bookings', icon: IconCalendarEvent, roles: everyone },
  { label: 'Notifications', to: '/notifications', icon: IconBell, roles: everyone },
  { label: 'Approval queue', to: '/approval-queue', icon: IconChecklist, roles: ['manager'] },
  { label: 'Loans', to: '/loans', icon: IconTruckDelivery, roles: ['administrator'] },
  { label: 'Equipment', to: '/admin/equipment', icon: IconDeviceLaptop, roles: ['administrator'] },
  { label: 'Categories', to: '/admin/categories', icon: IconCategory, roles: ['administrator'] },
  { label: 'Users', to: '/admin/users', icon: IconUsers, roles: ['administrator'] },
  {
    label: 'Decision log',
    to: '/admin/decision-log',
    icon: IconClipboardList,
    roles: ['administrator'],
  },
  { label: 'Settings', to: '/admin/settings', icon: IconSettings, roles: ['administrator'] },
]
