import { Badge, Card, Grid, Group, List, Skeleton, Stack, Text, Title } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useParams, useSearchParams } from 'react-router'
import { BookingForm } from '@/features/booking/BookingForm'
import { catalogueApi } from '@/shared/api/endpoints'
import { conditionLabels, DATE_FORMAT, formatMoney, formatPeriod } from '@/shared/lib/format'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { ItemStatusBadge } from '@/shared/ui/StatusBadge'

const CALENDAR_DAYS = 120

/** Item card with description, condition, booked periods and the booking form (US-04, US-06). */
export default function EquipmentPage() {
  const id = Number(useParams().id)
  const [searchParams] = useSearchParams()
  const chosenFrom = searchParams.get('from')
  const chosenTo = searchParams.get('to')
  const initialRange: [string, string] | undefined =
    chosenFrom && chosenTo ? [chosenFrom, chosenTo] : undefined
  const item = useQuery({ queryKey: ['equipment', id], queryFn: () => catalogueApi.get(id) })
  const from = dayjs().format(DATE_FORMAT)
  const to = dayjs().add(CALENDAR_DAYS, 'day').format(DATE_FORMAT)
  const periods = useQuery({
    queryKey: ['equipment', id, 'booked-periods', from],
    queryFn: () => catalogueApi.bookedPeriods(id, from, to),
  })

  return (
    <QueryState
      data={item.data}
      isPending={item.isPending}
      error={item.error}
      onRetry={() => void item.refetch()}
    >
      {(equipment) => (
        <>
          <PageHeader
            title={equipment.name}
            description={`${equipment.inventoryNumber}, ${equipment.category.name}`}
            actions={<ItemStatusBadge status={equipment.status} />}
          />
          <Grid>
            <Grid.Col span={{ base: 12, md: 7 }}>
              <Stack>
                <Card withBorder>
                  <Stack gap="xs">
                    {equipment.description && <Text>{equipment.description}</Text>}
                    <Group gap="xl">
                      <Detail label="Condition" value={conditionLabels[equipment.conditionCode]} />
                      <Detail label="Location" value={equipment.location.name} />
                      <Detail label="Value" value={formatMoney(equipment.value)} />
                      <Detail
                        label="Longest loan"
                        value={`${equipment.category.maxLoanDays} days`}
                      />
                    </Group>
                    <Group gap={4}>
                      {equipment.features.map((feature) => (
                        <Badge key={feature.id} variant="default">
                          {feature.name}
                        </Badge>
                      ))}
                      {(equipment.isSpecialised || equipment.category.isSpecialised) && (
                        <Badge variant="outline">Specialised: always decided by a manager</Badge>
                      )}
                    </Group>
                  </Stack>
                </Card>
                <Card withBorder>
                  <Title order={4} mb="xs">
                    Usage guidelines
                  </Title>
                  <Text size="sm">{equipment.category.usageGuidelines}</Text>
                </Card>
                <Card withBorder>
                  <Title order={4} mb="xs">
                    Not available in the next {CALENDAR_DAYS} days
                  </Title>
                  {periods.isPending ? (
                    <Skeleton height={60} />
                  ) : (periods.data ?? []).length === 0 ? (
                    <Text size="sm" c="dimmed">
                      The item is free on all days.
                    </Text>
                  ) : (
                    <List size="sm" spacing={4}>
                      {(periods.data ?? []).map((period) => (
                        <List.Item key={`${period.kind}-${period.startDate}`}>
                          {formatPeriod(period.startDate, period.endDate)}{' '}
                          <Text span c="dimmed" size="xs">
                            ({period.kind === 'maintenance' ? 'maintenance' : 'booked'})
                          </Text>
                        </List.Item>
                      ))}
                    </List>
                  )}
                </Card>
              </Stack>
            </Grid.Col>
            <Grid.Col span={{ base: 12, md: 5 }}>
              <Card withBorder>
                <Title order={4} mb="sm">
                  Book this item
                </Title>
                {equipment.status === 'retired' ? (
                  <Text c="dimmed">The item is retired and cannot be booked.</Text>
                ) : (
                  <BookingForm
                    equipmentItemId={equipment.id}
                    bookedPeriods={periods.data ?? []}
                    initialRange={initialRange}
                  />
                )}
              </Card>
            </Grid.Col>
          </Grid>
        </>
      )}
    </QueryState>
  )
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <Stack gap={0}>
      <Text size="xs" c="dimmed">
        {label}
      </Text>
      <Text fw={500}>{value}</Text>
    </Stack>
  )
}
