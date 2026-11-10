import { List, Stack, Text } from '@mantine/core'

/** Privacy notice of the system (GDPR, BA section 2.8). */
export function PrivacyNoticeText() {
  return (
    <Stack gap="sm">
      <Text>
        The equipment booking system processes your name, work e-mail address, department and role,
        your bookings, loans and returns, condition notes and the log of decisions on your requests.
        The legal basis is the legitimate interest of the organisation in managing its property
        (Article 6(1)(f) GDPR).
      </Text>
      <List spacing="xs">
        <List.Item>
          Your department manager sees your return history (number of loans and late returns) when
          deciding on a request you submitted.
        </List.Item>
        <List.Item>
          Automatic approvals never decline a request; every request that is not approved
          automatically is decided by a person.
        </List.Item>
        <List.Item>
          Do not enter personal data in the purpose of a booking. The text may be processed by an AI
          service; your identity and history are never sent to it.
        </List.Item>
        <List.Item>
          Personal data of returned loans are anonymised after the retention period set by the
          organisation; the history of the equipment is kept.
        </List.Item>
      </List>
    </Stack>
  )
}
