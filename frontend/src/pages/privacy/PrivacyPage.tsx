import { Paper } from '@mantine/core'
import { PageHeader } from '@/shared/ui/PageHeader'
import { PrivacyNoticeText } from './PrivacyNoticeText'

export default function PrivacyPage() {
  return (
    <>
      <PageHeader title="Privacy notice" description="How the system processes personal data" />
      <Paper withBorder p="lg" maw={800}>
        <PrivacyNoticeText />
      </Paper>
    </>
  )
}
