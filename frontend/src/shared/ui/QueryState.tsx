import { Alert, Button, Skeleton, Stack, Text } from '@mantine/core'
import { IconAlertCircle } from '@tabler/icons-react'
import type { ReactNode } from 'react'
import { getErrorMessage } from '@/shared/api/apiError'

interface QueryStateProps<T> {
  data: T | undefined
  isPending: boolean
  error: unknown
  onRetry?: () => void
  skeletonRows?: number
  emptyMessage?: string
  isEmpty?: (data: T) => boolean
  children: (data: T) => ReactNode
}

/** Renders skeletons while loading, an error with retry on failure, a message when empty, otherwise the content. */
export function QueryState<T>({
  data,
  isPending,
  error,
  onRetry,
  skeletonRows = 4,
  emptyMessage = 'Nothing to show.',
  isEmpty,
  children,
}: QueryStateProps<T>) {
  if (isPending) {
    return (
      <Stack gap="sm" aria-busy="true" aria-label="Loading">
        {Array.from({ length: skeletonRows }, (_, index) => (
          <Skeleton key={index} height={36} radius="sm" />
        ))}
      </Stack>
    )
  }
  if (error || data === undefined) {
    return (
      <Alert
        color="red"
        icon={<IconAlertCircle size={18} />}
        title="Could not load the data"
        role="alert"
      >
        <Stack gap="xs" align="flex-start">
          <Text size="sm">{getErrorMessage(error)}</Text>
          {onRetry && (
            <Button size="xs" variant="light" color="red" onClick={onRetry}>
              Try again
            </Button>
          )}
        </Stack>
      </Alert>
    )
  }
  if (isEmpty?.(data)) {
    return (
      <Text c="dimmed" ta="center" py="xl">
        {emptyMessage}
      </Text>
    )
  }
  return children(data)
}
