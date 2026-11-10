import { describe, expect, it } from 'vitest'
import { formatMoney, formatPeriod, routingReasonLabel, toApiDate } from './format'

describe('format helpers', () => {
  it('formats single days, ranges and open ended periods', () => {
    expect(formatPeriod('2026-11-10', '2026-11-10')).toBe('10 Nov 2026')
    expect(formatPeriod('2026-11-10', '2026-11-12')).toBe('10 Nov 2026 to 12 Nov 2026')
    expect(formatPeriod('2026-11-10', null)).toBe('from 10 Nov 2026')
  })

  it('formats money in euro', () => {
    expect(formatMoney(1199)).toBe('€1,199.00')
  })

  it('converts picker values to API dates', () => {
    expect(toApiDate('2026-11-10')).toBe('2026-11-10')
    expect(toApiDate(null)).toBeNull()
  })

  it('labels routing reasons and keeps unknown codes', () => {
    expect(routingReasonLabel('high_value')).toBe('High value item')
    expect(routingReasonLabel('something_new')).toBe('something_new')
  })
})
