import { notifications } from '@mantine/notifications'
import { getErrorMessage } from '@/shared/api/apiError'
import { logger } from './logger'

export const notifySuccess = (message: string): void => {
  notifications.show({ color: 'teal', title: 'Done', message })
}

export const notifyError = (error: unknown, title = 'Action failed'): void => {
  logger.warn(title, error)
  notifications.show({ color: 'red', title, message: getErrorMessage(error) })
}
