import { Button, Center, Stack, Text, Title } from '@mantine/core'
import { Link } from 'react-router'

interface ErrorPageProps {
  code: string
  title: string
  message: string
}

/** Full page message for 403, 404 and unexpected errors. */
export function ErrorPage({ code, title, message }: ErrorPageProps) {
  return (
    <Center mih="60vh" p="md">
      <Stack align="center" gap="sm" maw={480}>
        <Text c="dimmed" fw={700} fz={48} lh={1}>
          {code}
        </Text>
        <Title order={2} ta="center">
          {title}
        </Title>
        <Text c="dimmed" ta="center">
          {message}
        </Text>
        <Button component={Link} to="/" variant="light">
          Go to the catalogue
        </Button>
      </Stack>
    </Center>
  )
}

export function NotFoundPage() {
  return (
    <ErrorPage code="404" title="Page not found" message="The page does not exist or was moved." />
  )
}

export function ForbiddenPage() {
  return (
    <ErrorPage
      code="403"
      title="Access denied"
      message="Your role does not allow you to open this page."
    />
  )
}
