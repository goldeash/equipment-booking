import {
  ActionIcon,
  Button,
  Group,
  Menu,
  Pagination,
  Stack,
  Switch,
  Table,
  Text,
  TextInput,
} from '@mantine/core'
import { useDebouncedValue, useDisclosure } from '@mantine/hooks'
import { IconDots, IconPlus, IconSearch } from '@tabler/icons-react'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { catalogueApi } from '@/shared/api/endpoints'
import type { EquipmentDetails, EquipmentListItem } from '@/shared/api/types'
import { conditionLabels, formatMoney } from '@/shared/lib/format'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { ItemStatusBadge } from '@/shared/ui/StatusBadge'
import { EquipmentFormModal } from './EquipmentFormModal'
import { MaintenanceModal } from './MaintenanceModal'

/** Administration of the equipment catalogue (US-01, US-30 to US-32). */
export default function AdminEquipmentPage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [debouncedSearch] = useDebouncedValue(search, 300)
  const [includeRetired, setIncludeRetired] = useState(false)
  const [page, setPage] = useState(1)
  const [editing, setEditing] = useState<EquipmentDetails | null>(null)
  const [maintenanceItem, setMaintenanceItem] = useState<EquipmentListItem | null>(null)
  const [formOpened, form] = useDisclosure()
  const params = {
    q: debouncedSearch.trim() || undefined,
    includeRetired,
    page,
    pageSize: 25,
    sort: 'inventoryNumber',
  }
  const items = useQuery({
    queryKey: ['equipment', 'admin', params],
    queryFn: () => catalogueApi.search(params),
    placeholderData: keepPreviousData,
  })

  const retire = useMutation({
    mutationFn: catalogueApi.retire,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['equipment'] })
      notifySuccess('The item was retired.')
    },
    onError: (error) => notifyError(error, 'Could not retire the item'),
  })

  const openEdit = async (id: number) => {
    try {
      setEditing(await catalogueApi.get(id))
      form.open()
    } catch (error) {
      notifyError(error, 'Could not load the item')
    }
  }

  return (
    <>
      <PageHeader
        title="Equipment"
        actions={
          <Button
            leftSection={<IconPlus size={16} />}
            onClick={() => {
              setEditing(null)
              form.open()
            }}
          >
            Add item
          </Button>
        }
      />
      <Group mb="md" align="flex-end">
        <TextInput
          label="Search"
          leftSection={<IconSearch size={16} />}
          value={search}
          onChange={(event) => {
            setSearch(event.currentTarget.value)
            setPage(1)
          }}
        />
        <Switch
          label="Show retired items"
          checked={includeRetired}
          onChange={(event) => {
            setIncludeRetired(event.currentTarget.checked)
            setPage(1)
          }}
        />
      </Group>
      <QueryState
        data={items.data}
        isPending={items.isPending}
        error={items.error}
        onRetry={() => void items.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="No items found."
      >
        {(data) => (
          <Stack>
            <Table.ScrollContainer minWidth={760}>
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Inventory number</Table.Th>
                    <Table.Th>Name</Table.Th>
                    <Table.Th>Category</Table.Th>
                    <Table.Th>Condition</Table.Th>
                    <Table.Th ta="right">Value</Table.Th>
                    <Table.Th>Status</Table.Th>
                    <Table.Th />
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {data.items.map((item) => (
                    <Table.Tr key={item.id}>
                      <Table.Td>{item.inventoryNumber}</Table.Td>
                      <Table.Td>{item.name}</Table.Td>
                      <Table.Td>{item.categoryName}</Table.Td>
                      <Table.Td>{conditionLabels[item.conditionCode]}</Table.Td>
                      <Table.Td ta="right">{formatMoney(item.value)}</Table.Td>
                      <Table.Td>
                        <ItemStatusBadge status={item.status} />
                      </Table.Td>
                      <Table.Td>
                        <Menu position="bottom-end">
                          <Menu.Target>
                            <ActionIcon
                              variant="subtle"
                              aria-label={`Actions for ${item.inventoryNumber}`}
                            >
                              <IconDots size={16} />
                            </ActionIcon>
                          </Menu.Target>
                          <Menu.Dropdown>
                            <Menu.Item
                              disabled={item.status === 'retired'}
                              onClick={() => void openEdit(item.id)}
                            >
                              Edit
                            </Menu.Item>
                            <Menu.Item onClick={() => setMaintenanceItem(item)}>
                              Maintenance and history
                            </Menu.Item>
                            <Menu.Item
                              color="red"
                              disabled={item.status === 'retired'}
                              onClick={() => retire.mutate(item.id)}
                            >
                              Retire
                            </Menu.Item>
                          </Menu.Dropdown>
                        </Menu>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
            <Group justify="space-between">
              <Text size="sm" c="dimmed">
                {data.totalCount} items
              </Text>
              {data.totalPages > 1 && (
                <Pagination total={data.totalPages} value={page} onChange={setPage} />
              )}
            </Group>
          </Stack>
        )}
      </QueryState>
      <EquipmentFormModal item={editing} opened={formOpened} onClose={form.close} />
      {maintenanceItem && (
        <MaintenanceModal item={maintenanceItem} onClose={() => setMaintenanceItem(null)} />
      )}
    </>
  )
}
