import { Button, Modal, Stack } from '@mantine/core'
import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/shared/api/endpoints'
import { PrivacyNoticeText } from '@/pages/privacy/PrivacyNoticeText'
import { useAuthStore } from './authStore'

/** Shown at the first sign in (US-35). The user acknowledges the notice; it is not a consent request. */
export function PrivacyNoticeModal() {
  const user = useAuthStore((state) => state.user)
  const setUser = useAuthStore((state) => state.setUser)
  const acknowledge = useMutation({
    mutationFn: authApi.acknowledgePrivacyNotice,
    onSuccess: setUser,
  })

  const opened = user !== null && !user.privacyNoticeAcknowledged

  return (
    <Modal
      opened={opened}
      onClose={() => undefined}
      withCloseButton={false}
      closeOnClickOutside={false}
      closeOnEscape={false}
      title="Privacy notice"
      size="lg"
    >
      <Stack>
        <PrivacyNoticeText />
        <Button onClick={() => acknowledge.mutate()} loading={acknowledge.isPending}>
          I have read the privacy notice
        </Button>
      </Stack>
    </Modal>
  )
}
