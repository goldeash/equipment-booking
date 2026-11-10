import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Checkbox,
  Grid,
  Modal,
  MultiSelect,
  NumberInput,
  Select,
  Stack,
  TextInput,
  Textarea,
} from '@mantine/core'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  useCategories,
  useConditions,
  useFeatures,
  useLocations,
} from '@/features/catalogue/useReferenceData'
import { isApiError } from '@/shared/api/apiError'
import { catalogueApi } from '@/shared/api/endpoints'
import type { EquipmentDetails } from '@/shared/api/types'
import { notifyError, notifySuccess } from '@/shared/lib/notify'

const schema = z.object({
  inventoryNumber: z
    .string()
    .trim()
    .min(1, 'Enter the inventory number.')
    .max(30)
    .regex(/^[A-Za-z0-9-]+$/, 'Letters, digits and hyphens only.'),
  name: z.string().trim().min(1, 'Enter the name.').max(150),
  description: z.string().max(2000),
  categoryId: z.string().min(1, 'Choose a category.'),
  locationId: z.string().min(1, 'Choose a location.'),
  value: z.number({ error: 'Enter the value.' }).min(0, 'The value must not be negative.'),
  conditionCode: z.string().min(1, 'Choose the condition.'),
  isSpecialised: z.boolean(),
  featureIds: z.array(z.string()),
})

type FormValues = z.infer<typeof schema>

interface EquipmentFormModalProps {
  item: EquipmentDetails | null
  opened: boolean
  onClose: () => void
}

/** Create or edit an item (US-01); server validation errors are shown next to the fields. */
export function EquipmentFormModal({ item, opened, onClose }: EquipmentFormModalProps) {
  const queryClient = useQueryClient()
  const categories = useCategories()
  const locations = useLocations()
  const features = useFeatures()
  const conditions = useConditions()
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    values: {
      inventoryNumber: item?.inventoryNumber ?? '',
      name: item?.name ?? '',
      description: item?.description ?? '',
      categoryId: item ? String(item.category.id) : '',
      locationId: item ? String(item.location.id) : '',
      value: item?.value ?? 0,
      conditionCode: item?.conditionCode ?? 'new',
      isSpecialised: item?.isSpecialised ?? false,
      featureIds: item?.features.map((feature) => String(feature.id)) ?? [],
    },
  })

  const save = useMutation({
    mutationFn: (values: FormValues) => {
      const body = {
        ...values,
        description: values.description.trim() || null,
        categoryId: Number(values.categoryId),
        locationId: Number(values.locationId),
        featureIds: values.featureIds.map(Number),
      }
      return item ? catalogueApi.update(item.id, body) : catalogueApi.create(body)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['equipment'] })
      notifySuccess(item ? 'The item was updated.' : 'The item was added.')
      onClose()
    },
    onError: (error) => {
      if (isApiError(error) && error.code === 'equipment.duplicate_inventory_number') {
        setError('inventoryNumber', { message: error.message })
        return
      }
      if (isApiError(error)) {
        for (const [field, messages] of Object.entries(error.fieldErrors)) {
          setError(field as keyof FormValues, { message: messages.join(' ') })
        }
      }
      notifyError(error, 'Could not save the item')
    },
  })

  const options = (list: { id: number; name: string }[] | undefined) =>
    (list ?? []).map((entry) => ({ value: String(entry.id), label: entry.name }))

  return (
    <Modal opened={opened} onClose={onClose} title={item ? 'Edit item' : 'Add item'} size="lg">
      <form onSubmit={handleSubmit((values) => save.mutate(values))} noValidate>
        <Stack>
          <Grid>
            <Grid.Col span={{ base: 12, sm: 5 }}>
              <TextInput
                label="Inventory number"
                required
                error={errors.inventoryNumber?.message}
                {...register('inventoryNumber')}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 7 }}>
              <TextInput label="Name" required error={errors.name?.message} {...register('name')} />
            </Grid.Col>
          </Grid>
          <Textarea
            label="Description"
            autosize
            minRows={2}
            error={errors.description?.message}
            {...register('description')}
          />
          <Grid>
            <Grid.Col span={{ base: 12, sm: 6 }}>
              <Controller
                control={control}
                name="categoryId"
                render={({ field }) => (
                  <Select
                    label="Category"
                    required
                    data={options(categories.data)}
                    value={field.value || null}
                    onChange={(value) => field.onChange(value ?? '')}
                    error={errors.categoryId?.message}
                  />
                )}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6 }}>
              <Controller
                control={control}
                name="locationId"
                render={({ field }) => (
                  <Select
                    label="Location"
                    required
                    data={options(locations.data)}
                    value={field.value || null}
                    onChange={(value) => field.onChange(value ?? '')}
                    error={errors.locationId?.message}
                  />
                )}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6 }}>
              <Controller
                control={control}
                name="value"
                render={({ field }) => (
                  <NumberInput
                    label="Value (EUR)"
                    required
                    min={0}
                    decimalScale={2}
                    value={field.value}
                    onChange={(value) =>
                      field.onChange(typeof value === 'number' ? value : Number(value))
                    }
                    error={errors.value?.message}
                  />
                )}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6 }}>
              <Controller
                control={control}
                name="conditionCode"
                render={({ field }) => (
                  <Select
                    label="Condition"
                    required
                    data={(conditions.data ?? []).map((condition) => ({
                      value: condition.code,
                      label: condition.name,
                    }))}
                    value={field.value}
                    onChange={(value) => field.onChange(value ?? 'good')}
                    allowDeselect={false}
                  />
                )}
              />
            </Grid.Col>
          </Grid>
          <Controller
            control={control}
            name="featureIds"
            render={({ field }) => (
              <MultiSelect
                label="Features"
                searchable
                data={options(features.data)}
                value={field.value}
                onChange={field.onChange}
              />
            )}
          />
          <Controller
            control={control}
            name="isSpecialised"
            render={({ field }) => (
              <Checkbox
                label="Specialised item: requests always go to a manager"
                checked={field.value}
                onChange={(event) => field.onChange(event.currentTarget.checked)}
              />
            )}
          />
          <Button type="submit" loading={save.isPending}>
            Save
          </Button>
        </Stack>
      </form>
    </Modal>
  )
}
