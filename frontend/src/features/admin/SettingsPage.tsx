import { Button, Card, NumberInput, Stack, Switch, Text } from '@mantine/core'
import { TimeInput } from '@mantine/dates'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { adminApi } from '@/shared/api/endpoints'
import type { ApprovalSettings } from '@/shared/api/types'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'

/** Low value threshold, automatic approval switch, retention period and return time (US-02, US-27, US-36). */
export default function SettingsPage() {
  const settings = useQuery({ queryKey: ['settings'], queryFn: adminApi.settings })
  return (
    <>
      <PageHeader title="Settings" description="Approval route and data retention" />
      <QueryState
        data={settings.data}
        isPending={settings.isPending}
        error={settings.error}
        onRetry={() => void settings.refetch()}
      >
        {(data) => <SettingsForm initial={data} />}
      </QueryState>
    </>
  )
}

function SettingsForm({ initial }: { initial: ApprovalSettings }) {
  const queryClient = useQueryClient()
  const [values, setValues] = useState(initial)
  const save = useMutation({
    mutationFn: () =>
      adminApi.updateSettings({
        lowValueThreshold: values.lowValueThreshold,
        autoApprovalEnabled: values.autoApprovalEnabled,
        retentionDays: values.retentionDays,
        returnTime: values.returnTime,
      }),
    onSuccess: async (saved) => {
      queryClient.setQueryData(['settings'], saved)
      notifySuccess('The settings were saved.')
    },
    onError: (error) => notifyError(error, 'Could not save the settings'),
  })

  return (
    <Card withBorder maw={560}>
      <Stack>
        <NumberInput
          label="Low value threshold (EUR)"
          description="Items with a value equal to or above it always go to a manager"
          min={0.01}
          decimalScale={2}
          value={values.lowValueThreshold}
          onChange={(value) => setValues({ ...values, lowValueThreshold: Number(value) })}
        />
        <Switch
          label="Automatic approval of routine requests"
          checked={values.autoApprovalEnabled}
          onChange={(event) =>
            setValues({ ...values, autoApprovalEnabled: event.currentTarget.checked })
          }
        />
        <NumberInput
          label="Retention period (days)"
          description="Personal data of returned loans are anonymised afterwards"
          min={30}
          max={3650}
          value={values.retentionDays}
          onChange={(value) => setValues({ ...values, retentionDays: Number(value) })}
        />
        <TimeInput
          label="Return time"
          description="Items are due at this local time on the last day"
          value={values.returnTime.slice(0, 5)}
          onChange={(event) => setValues({ ...values, returnTime: event.currentTarget.value })}
        />
        <Text size="sm" c="dimmed">
          Time zone: {values.timeZone}
        </Text>
        <Button loading={save.isPending} onClick={() => save.mutate()}>
          Save settings
        </Button>
      </Stack>
    </Card>
  )
}
