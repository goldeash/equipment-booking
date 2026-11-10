import {
  Badge,
  Button,
  Checkbox,
  Modal,
  NumberInput,
  Stack,
  Table,
  TextInput,
  Textarea,
} from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useCategories } from '@/features/catalogue/useReferenceData'
import { catalogueApi, type SaveCategory } from '@/shared/api/endpoints'
import type { Category } from '@/shared/api/types'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'

const empty: SaveCategory = {
  name: '',
  description: null,
  maxLoanDays: 7,
  usageGuidelines: '',
  isSpecialised: false,
  autoApprovalEnabled: true,
}

/** Categories with loan limits, usage guidelines and approval switches (US-02, US-27). */
export default function AdminCategoriesPage() {
  const categories = useCategories()
  const [editing, setEditing] = useState<Category | 'new' | null>(null)

  return (
    <>
      <PageHeader
        title="Categories"
        actions={
          <Button leftSection={<IconPlus size={16} />} onClick={() => setEditing('new')}>
            Add category
          </Button>
        }
      />
      <QueryState
        data={categories.data}
        isPending={categories.isPending}
        error={categories.error}
        onRetry={() => void categories.refetch()}
      >
        {(data) => (
          <Table.ScrollContainer minWidth={640}>
            <Table striped highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Name</Table.Th>
                  <Table.Th>Longest loan</Table.Th>
                  <Table.Th>Approval</Table.Th>
                  <Table.Th />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {data.map((category) => (
                  <Table.Tr key={category.id}>
                    <Table.Td>{category.name}</Table.Td>
                    <Table.Td>{category.maxLoanDays} days</Table.Td>
                    <Table.Td>
                      {category.isSpecialised ? (
                        <Badge variant="outline">Specialised</Badge>
                      ) : category.autoApprovalEnabled ? (
                        <Badge color="teal" variant="light">
                          Automatic allowed
                        </Badge>
                      ) : (
                        <Badge color="gray" variant="light">
                          Manual only
                        </Badge>
                      )}
                    </Table.Td>
                    <Table.Td>
                      <Button size="xs" variant="subtle" onClick={() => setEditing(category)}>
                        Edit
                      </Button>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        )}
      </QueryState>
      {editing && (
        <CategoryModal
          category={editing === 'new' ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}
    </>
  )
}

function CategoryModal({ category, onClose }: { category: Category | null; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [values, setValues] = useState<SaveCategory>(category ? { ...category } : empty)
  const update = <K extends keyof SaveCategory>(key: K, value: SaveCategory[K]) =>
    setValues((current) => ({ ...current, [key]: value }))
  const invalid = !values.name.trim() || !values.usageGuidelines.trim() || values.maxLoanDays < 1
  const save = useMutation({
    mutationFn: () =>
      category
        ? catalogueApi.updateCategory(category.id, values)
        : catalogueApi.createCategory(values),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['categories'] })
      notifySuccess('The category was saved.')
      onClose()
    },
    onError: (error) => notifyError(error, 'Could not save the category'),
  })

  return (
    <Modal opened onClose={onClose} title={category ? 'Edit category' : 'Add category'}>
      <Stack>
        <TextInput
          label="Name"
          required
          value={values.name}
          onChange={(event) => update('name', event.currentTarget.value)}
        />
        <Textarea
          label="Description"
          autosize
          value={values.description ?? ''}
          onChange={(event) => update('description', event.currentTarget.value || null)}
        />
        <NumberInput
          label="Longest loan (days)"
          required
          min={1}
          max={365}
          value={values.maxLoanDays}
          onChange={(value) => update('maxLoanDays', Number(value))}
        />
        <Textarea
          label="Usage guidelines"
          description="What the items may be used for"
          required
          autosize
          minRows={3}
          value={values.usageGuidelines}
          onChange={(event) => update('usageGuidelines', event.currentTarget.value)}
        />
        <Checkbox
          label="Specialised: requests always go to a manager"
          checked={values.isSpecialised}
          onChange={(event) => update('isSpecialised', event.currentTarget.checked)}
        />
        <Checkbox
          label="Allow automatic approval"
          checked={values.autoApprovalEnabled}
          onChange={(event) => update('autoApprovalEnabled', event.currentTarget.checked)}
        />
        <Button disabled={invalid} loading={save.isPending} onClick={() => save.mutate()}>
          Save
        </Button>
      </Stack>
    </Modal>
  )
}
