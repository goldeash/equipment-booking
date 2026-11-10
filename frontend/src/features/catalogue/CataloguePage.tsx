import {
  Anchor,
  Badge,
  Card,
  Grid,
  Group,
  MultiSelect,
  Pagination,
  Select,
  SimpleGrid,
  Stack,
  Table,
  Text,
  TextInput,
} from '@mantine/core'
import { DatePickerInput, type DatesRangeValue } from '@mantine/dates'
import { useDebouncedValue, useMediaQuery } from '@mantine/hooks'
import { IconSearch } from '@tabler/icons-react'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router'
import { catalogueApi } from '@/shared/api/endpoints'
import type { EquipmentListItem } from '@/shared/api/types'
import { formatMoney, toApiDate } from '@/shared/lib/format'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'
import { ItemStatusBadge } from '@/shared/ui/StatusBadge'
import { useCategories, useFeatures, useLocations } from './useReferenceData'

const PAGE_SIZE = 20

interface ItemListProps {
  items: EquipmentListItem[]
  itemLink: (id: number) => string
}

const sortOptions = [
  { value: 'name', label: 'Name (A to Z)' },
  { value: '-name', label: 'Name (Z to A)' },
  { value: 'value', label: 'Value (low to high)' },
  { value: '-value', label: 'Value (high to low)' },
  { value: 'inventoryNumber', label: 'Inventory number' },
]

/** Catalogue search with keyword, category, location, features and availability on dates (US-03). */
export default function CataloguePage() {
  const [search, setSearch] = useState('')
  const [debouncedSearch] = useDebouncedValue(search, 300)
  const [categoryId, setCategoryId] = useState<string | null>(null)
  const [locationId, setLocationId] = useState<string | null>(null)
  const [featureIds, setFeatureIds] = useState<string[]>([])
  const [dates, setDates] = useState<DatesRangeValue<string>>([null, null])
  const [sort, setSort] = useState<string>('name')
  const [page, setPage] = useState(1)
  const isMobile = useMediaQuery('(max-width: 48em)')

  const categories = useCategories()
  const locations = useLocations()
  const features = useFeatures()
  const [from, to] = dates
  const hasRange = from !== null && to !== null

  const params = {
    q: debouncedSearch.trim() || undefined,
    categoryId: categoryId ? Number(categoryId) : null,
    locationId: locationId ? Number(locationId) : null,
    featureIds: featureIds.map(Number),
    availableFrom: hasRange ? toApiDate(from) : null,
    availableTo: hasRange ? toApiDate(to) : null,
    sort,
    page,
    pageSize: PAGE_SIZE,
  }
  const itemLink = (itemId: number) =>
    hasRange && params.availableFrom && params.availableTo
      ? `/equipment/${itemId}?from=${params.availableFrom}&to=${params.availableTo}`
      : `/equipment/${itemId}`
  const result = useQuery({
    queryKey: ['equipment', params],
    queryFn: () => catalogueApi.search(params),
    placeholderData: keepPreviousData,
  })

  const resetPage =
    <T,>(setter: (value: T) => void) =>
    (value: T) => {
      setter(value)
      setPage(1)
    }

  return (
    <>
      <PageHeader
        title="Equipment catalogue"
        description="Find an item and check when it is free"
      />
      <Card withBorder mb="md">
        <Grid>
          <Grid.Col span={{ base: 12, md: 4 }}>
            <TextInput
              label="Search"
              placeholder="Name, description or inventory number"
              leftSection={<IconSearch size={16} />}
              value={search}
              onChange={(event) => resetPage(setSearch)(event.currentTarget.value)}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
            <DatePickerInput
              type="range"
              label="Free on dates"
              placeholder="Any dates"
              clearable
              minDate={new Date()}
              value={dates}
              onChange={resetPage(setDates)}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
            <Select
              label="Sort by"
              data={sortOptions}
              value={sort}
              onChange={(value) => resetPage(setSort)(value ?? 'name')}
              allowDeselect={false}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 12, sm: 4 }}>
            <Select
              label="Category"
              placeholder="All categories"
              clearable
              data={(categories.data ?? []).map((category) => ({
                value: String(category.id),
                label: category.name,
              }))}
              value={categoryId}
              onChange={resetPage(setCategoryId)}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 12, sm: 4 }}>
            <Select
              label="Location"
              placeholder="All locations"
              clearable
              data={(locations.data ?? []).map((location) => ({
                value: String(location.id),
                label: location.name,
              }))}
              value={locationId}
              onChange={resetPage(setLocationId)}
            />
          </Grid.Col>
          <Grid.Col span={{ base: 12, sm: 4 }}>
            <MultiSelect
              label="Features"
              placeholder="Any features"
              clearable
              searchable
              data={(features.data ?? []).map((feature) => ({
                value: String(feature.id),
                label: feature.name,
              }))}
              value={featureIds}
              onChange={resetPage(setFeatureIds)}
            />
          </Grid.Col>
        </Grid>
      </Card>

      <QueryState
        data={result.data}
        isPending={result.isPending}
        error={result.error}
        onRetry={() => void result.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="No items match the filters."
      >
        {(data) => (
          <Stack>
            <Text size="sm" c="dimmed">
              {data.totalCount} items{hasRange ? ' free on the selected dates' : ''}
            </Text>
            {isMobile ? (
              <ItemCards items={data.items} itemLink={itemLink} />
            ) : (
              <ItemTable items={data.items} itemLink={itemLink} />
            )}
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

function ItemTable({ items, itemLink }: ItemListProps) {
  return (
    <Table.ScrollContainer minWidth={760}>
      <Table striped highlightOnHover>
        <Table.Thead>
          <Table.Tr>
            <Table.Th>Item</Table.Th>
            <Table.Th>Category</Table.Th>
            <Table.Th>Location</Table.Th>
            <Table.Th>Features</Table.Th>
            <Table.Th ta="right">Value</Table.Th>
            <Table.Th>Status</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {items.map((item) => (
            <Table.Tr key={item.id}>
              <Table.Td>
                <Anchor component={Link} to={itemLink(item.id)} fw={600}>
                  {item.name}
                </Anchor>
                <Text size="xs" c="dimmed">
                  {item.inventoryNumber}
                </Text>
              </Table.Td>
              <Table.Td>
                {item.categoryName}
                {item.isSpecialised && (
                  <Badge ml={6} size="xs" variant="outline">
                    Specialised
                  </Badge>
                )}
              </Table.Td>
              <Table.Td>{item.locationName}</Table.Td>
              <Table.Td>
                <Group gap={4}>
                  {item.features.slice(0, 3).map((feature) => (
                    <Badge key={feature} size="xs" variant="default">
                      {feature}
                    </Badge>
                  ))}
                </Group>
              </Table.Td>
              <Table.Td ta="right">{formatMoney(item.value)}</Table.Td>
              <Table.Td>
                <ItemStatusBadge status={item.status} />
              </Table.Td>
            </Table.Tr>
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  )
}

function ItemCards({ items, itemLink }: ItemListProps) {
  return (
    <SimpleGrid cols={1}>
      {items.map((item) => (
        <Card key={item.id} withBorder component={Link} to={itemLink(item.id)}>
          <Group justify="space-between" wrap="nowrap">
            <Stack gap={0}>
              <Text fw={600}>{item.name}</Text>
              <Text size="xs" c="dimmed">
                {item.inventoryNumber}, {item.categoryName}, {item.locationName}
              </Text>
            </Stack>
            <ItemStatusBadge status={item.status} />
          </Group>
        </Card>
      ))}
    </SimpleGrid>
  )
}
