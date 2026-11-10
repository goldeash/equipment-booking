import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import dayjs from 'dayjs'
import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { afterAll, afterEach, beforeAll, describe, expect, it } from 'vitest'
import { renderWithProviders } from '@/test/render'
import { BookingForm } from './BookingForm'

const server = setupServer()

beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const tomorrow = dayjs().add(1, 'day')

const range: [string, string] = [tomorrow.format('YYYY-MM-DD'), tomorrow.format('YYYY-MM-DD')]

async function submitWithPurpose(purpose: string) {
  const user = userEvent.setup()
  await user.type(screen.getByRole('textbox', { name: /purpose/i }), purpose)
  await user.click(screen.getByRole('button', { name: 'Request this item' }))
}

describe('BookingForm', () => {
  it('shows the validation message next to the purpose field', async () => {
    renderWithProviders(<BookingForm equipmentItemId={7} bookedPeriods={[]} initialRange={range} />)
    const user = userEvent.setup()

    await user.click(screen.getByRole('button', { name: 'Request this item' }))

    expect(
      await screen.findByText('Describe the purpose in at least 3 characters.'),
    ).toBeInTheDocument()
  })

  it('shows the conflicting dates when the API reports an overlap', async () => {
    server.use(
      http.post('http://localhost/api/v1/bookings', () =>
        HttpResponse.json(
          {
            title: 'Conflict',
            status: 409,
            detail: 'The item is already booked for some of the selected dates.',
            code: 'booking.overlap',
            conflictingPeriods: [
              { startDate: tomorrow.format('YYYY-MM-DD'), endDate: tomorrow.format('YYYY-MM-DD') },
            ],
          },
          { status: 409 },
        ),
      ),
    )
    renderWithProviders(<BookingForm equipmentItemId={7} bookedPeriods={[]} initialRange={range} />)

    await submitWithPurpose('Client presentation')

    expect(
      await screen.findByText('The item is already booked for some of the selected dates.'),
    ).toBeInTheDocument()
    expect(screen.getByText(/Already booked:/)).toBeInTheDocument()
  })

  it('opens the booking after a successful request', async () => {
    server.use(
      http.post('http://localhost/api/v1/bookings', () =>
        HttpResponse.json({ id: 42 }, { status: 201 }),
      ),
    )
    renderWithProviders(<BookingForm equipmentItemId={7} bookedPeriods={[]} initialRange={range} />)

    await submitWithPurpose('Client presentation')

    await waitFor(() => expect(screen.getByText('Navigated away')).toBeInTheDocument())
  })
})
