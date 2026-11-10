import dayjs from 'dayjs'
import { describe, expect, it } from 'vitest'
import { bookingSchema } from './bookingSchema'

const day = (offset: number) => dayjs().add(offset, 'day').format('YYYY-MM-DD')

describe('bookingSchema', () => {
  it('accepts future dates in order and a purpose', () => {
    expect(
      bookingSchema.safeParse({ startDate: day(1), endDate: day(3), purpose: 'Client meeting' })
        .success,
    ).toBe(true)
  })

  it('rejects a start date in the past', () => {
    const result = bookingSchema.safeParse({
      startDate: day(-1),
      endDate: day(1),
      purpose: 'Client meeting',
    })

    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.message).toBe('The start date must not be in the past.')
  })

  it('rejects an end date before the start date', () => {
    const result = bookingSchema.safeParse({
      startDate: day(3),
      endDate: day(2),
      purpose: 'Client meeting',
    })

    expect(result.error?.issues.map((issue) => issue.message)).toContain(
      'The end date must not be before the start date.',
    )
  })

  it('requires a purpose of at least three characters', () => {
    const result = bookingSchema.safeParse({ startDate: day(1), endDate: day(1), purpose: ' ab ' })

    expect(result.error?.issues[0]?.path).toEqual(['purpose'])
  })
})
