import dayjs from 'dayjs'
import { z } from 'zod'

export const PURPOSE_MIN = 3
export const PURPOSE_MAX = 1000

/** Validation of the booking form (US-06): dates in order, not in the past, a purpose of 3 to 1000 characters. */
export const bookingSchema = z
  .object({
    startDate: z.string({ error: 'Choose the dates.' }).min(1, 'Choose the dates.'),
    endDate: z.string({ error: 'Choose the dates.' }).min(1, 'Choose the dates.'),
    purpose: z
      .string()
      .trim()
      .min(PURPOSE_MIN, `Describe the purpose in at least ${PURPOSE_MIN} characters.`)
      .max(PURPOSE_MAX, `The purpose must not exceed ${PURPOSE_MAX} characters.`),
  })
  .refine((value) => !dayjs(value.startDate).isBefore(dayjs(), 'day'), {
    path: ['startDate'],
    message: 'The start date must not be in the past.',
  })
  .refine((value) => !dayjs(value.endDate).isBefore(dayjs(value.startDate), 'day'), {
    path: ['startDate'],
    message: 'The end date must not be before the start date.',
  })

export type BookingFormValues = z.infer<typeof bookingSchema>
