import { Anchor, Badge, Group, Pagination, Select, Stack, Table, Text } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { approvalsApi } from '@/shared/api/endpoints'
import type { AssessmentCheck } from '@/shared/api/types'
import { formatDateTime, formatMoney, routingReasonLabel } from '@/shared/lib/format'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'

const checkLabels: Record<string, string> = {
  organisation_auto_approval: 'Automatic approval on for the organisation',
  category_auto_approval: 'Automatic approval on for the category',
  value_threshold: 'Low value',
  not_specialised: 'Not specialised',
  max_duration: 'Within loan limit',
}

/** Compared values of a check, for example the item value against the threshold in force at that time. */
const checkValues = (check: AssessmentCheck): string | null => {
  if (check.observedValue === null || check.limitValue === null) {
    return null
  }

  return check.code === 'value_threshold'
    ? `${formatMoney(check.observedValue)}, threshold ${formatMoney(check.limitValue)}`
    : `${check.observedValue} of ${check.limitValue} days`
}

const Check = ({ check }: { check: AssessmentCheck }) => {
  const values = checkValues(check)
  return (
    <Group gap={4} wrap="nowrap">
      {check.passed ? (
        <IconCheck size={14} color="var(--mantine-color-teal-6)" aria-label="passed" />
      ) : (
        <IconX size={14} color="var(--mantine-color-red-6)" aria-label="failed" />
      )}
      <Text size="xs" title={check.name}>
        {checkLabels[check.code] ?? check.name}
        {values && (
          <Text span size="xs" c="dimmed">
            {' '}
            ({values})
          </Text>
        )}
      </Text>
    </Group>
  )
}

/** Log of every assessment of the approval route with all checks and the outcome (US-26). */
export default function DecisionLogPage() {
  const [outcome, setOutcome] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const params = { outcome, page, pageSize: 25 }
  const log = useQuery({
    queryKey: ['assessments', params],
    queryFn: () => approvalsApi.assessments(params),
    placeholderData: keepPreviousData,
  })

  return (
    <>
      <PageHeader
        title="Decision log"
        description="Policy checks, purpose check and outcome of every request"
      />
      <Select
        mb="md"
        maw={240}
        label="Outcome"
        placeholder="All outcomes"
        clearable
        data={[
          { value: 'approved', label: 'Approved automatically' },
          { value: 'routed', label: 'Routed to a manager' },
          { value: 'pending', label: 'Pending' },
        ]}
        value={outcome}
        onChange={(value) => {
          setOutcome(value)
          setPage(1)
        }}
      />
      <QueryState
        data={log.data}
        isPending={log.isPending}
        error={log.error}
        onRetry={() => void log.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="No assessments."
      >
        {(data) => (
          <Stack>
            <Table.ScrollContainer minWidth={900}>
              <Table striped>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Time</Table.Th>
                    <Table.Th>Request</Table.Th>
                    <Table.Th>Policy checks</Table.Th>
                    <Table.Th>Purpose</Table.Th>
                    <Table.Th>Outcome</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {data.items.map((entry) => (
                    <Table.Tr key={entry.id}>
                      <Table.Td>{formatDateTime(entry.assessedAt)}</Table.Td>
                      <Table.Td>
                        <Anchor component={Link} to={`/bookings/${entry.bookingId}`}>
                          #{entry.bookingId}
                        </Anchor>
                        <Text size="xs" c="dimmed">
                          {entry.itemName}
                        </Text>
                      </Table.Td>
                      <Table.Td>
                        <Stack gap={2}>
                          {entry.checks.map((check) => (
                            <Check key={check.code} check={check} />
                          ))}
                        </Stack>
                      </Table.Td>
                      <Table.Td>
                        <Text size="sm">{entry.purposeResult.replace('_', ' ')}</Text>
                      </Table.Td>
                      <Table.Td>
                        <Badge
                          color={
                            entry.outcome === 'approved'
                              ? 'violet'
                              : entry.outcome === 'routed'
                                ? 'orange'
                                : 'gray'
                          }
                          variant="light"
                        >
                          {entry.outcome}
                        </Badge>
                        {entry.reasons.map((reason) => (
                          <Text key={reason} size="xs" c="dimmed">
                            {routingReasonLabel(reason)}
                          </Text>
                        ))}
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
            {data.totalPages > 1 && (
              <Group justify="center">
                <Pagination total={data.totalPages} value={page} onChange={setPage} />
              </Group>
            )}
          </Stack>
        )}
      </QueryState>
    </>
  )
}
