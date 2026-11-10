import { env } from '@/shared/config/env'

/** Central logger; errors are always written, diagnostics only in development. */
export const logger = {
  error(message: string, details?: unknown): void {
    console.error(`[equipment-booking] ${message}`, details ?? '')
  },
  warn(message: string, details?: unknown): void {
    if (env.isDevelopment) {
      console.warn(`[equipment-booking] ${message}`, details ?? '')
    }
  },
}
