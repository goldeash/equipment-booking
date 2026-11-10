import { Badge, Text } from '@mantine/core'
import { IconRobot } from '@tabler/icons-react'
import type { DecisionInfo } from '@/shared/api/types'

/** Who decided on a request; decisions of the AI assistant are always marked (US-09, US-28). */
export function DecisionText({ decision }: { decision: DecisionInfo | null }) {
  if (decision === null) {
    return <Text c="dimmed">Waiting for a decision</Text>
  }
  if (decision.source === 'ai_assistant') {
    return (
      <Badge color="violet" variant="light" leftSection={<IconRobot size={14} aria-hidden />}>
        Approved by the AI assistant
      </Badge>
    )
  }
  return (
    <Text>
      {decision.outcome === 'approved' ? 'Approved' : 'Declined'} by{' '}
      {decision.decidedByName ?? 'a manager'}
    </Text>
  )
}
