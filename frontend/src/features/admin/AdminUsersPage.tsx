import {
  Badge,
  Button,
  Checkbox,
  Group,
  Modal,
  Pagination,
  PasswordInput,
  Select,
  Stack,
  Table,
  TextInput,
} from '@mantine/core'
import { useDebouncedValue } from '@mantine/hooks'
import { IconPlus, IconSearch } from '@tabler/icons-react'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useDepartments } from '@/features/catalogue/useReferenceData'
import { adminApi } from '@/shared/api/endpoints'
import type { Role, UserAccount } from '@/shared/api/types'
import { notifyError, notifySuccess } from '@/shared/lib/notify'
import { PageHeader } from '@/shared/ui/PageHeader'
import { QueryState } from '@/shared/ui/QueryState'

const roles = [
  { value: 'employee', label: 'Employee' },
  { value: 'manager', label: 'Department manager' },
  { value: 'administrator', label: 'Administrator' },
]

const PASSWORD_MIN = 12

/** User accounts, roles and departments (US-34). */
export default function AdminUsersPage() {
  const [search, setSearch] = useState('')
  const [debouncedSearch] = useDebouncedValue(search, 300)
  const [role, setRole] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const [editing, setEditing] = useState<UserAccount | 'new' | null>(null)
  const params = { q: debouncedSearch.trim() || undefined, role, page, pageSize: 25 }
  const users = useQuery({
    queryKey: ['users', params],
    queryFn: () => adminApi.users(params),
    placeholderData: keepPreviousData,
  })

  return (
    <>
      <PageHeader
        title="Users"
        actions={
          <Button leftSection={<IconPlus size={16} />} onClick={() => setEditing('new')}>
            Add user
          </Button>
        }
      />
      <Group mb="md" align="flex-end">
        <TextInput
          label="Search"
          placeholder="Name or e-mail"
          leftSection={<IconSearch size={16} />}
          value={search}
          onChange={(event) => {
            setSearch(event.currentTarget.value)
            setPage(1)
          }}
        />
        <Select
          label="Role"
          placeholder="All roles"
          clearable
          data={roles}
          value={role}
          onChange={(value) => {
            setRole(value)
            setPage(1)
          }}
        />
      </Group>
      <QueryState
        data={users.data}
        isPending={users.isPending}
        error={users.error}
        onRetry={() => void users.refetch()}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="No users found."
      >
        {(data) => (
          <Stack>
            <Table.ScrollContainer minWidth={700}>
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Name</Table.Th>
                    <Table.Th>E-mail</Table.Th>
                    <Table.Th>Role</Table.Th>
                    <Table.Th>Department</Table.Th>
                    <Table.Th>State</Table.Th>
                    <Table.Th />
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {data.items.map((user) => (
                    <Table.Tr key={user.id}>
                      <Table.Td>{user.fullName}</Table.Td>
                      <Table.Td>{user.email}</Table.Td>
                      <Table.Td>{roles.find((entry) => entry.value === user.role)?.label}</Table.Td>
                      <Table.Td>{user.departmentName}</Table.Td>
                      <Table.Td>
                        {user.isActive ? (
                          <Badge color="teal" variant="light">
                            Active
                          </Badge>
                        ) : (
                          <Badge color="gray" variant="light">
                            Inactive
                          </Badge>
                        )}
                      </Table.Td>
                      <Table.Td>
                        <Button size="xs" variant="subtle" onClick={() => setEditing(user)}>
                          Edit
                        </Button>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
            {data.totalPages > 1 && (
              <Group justify="center">
                <Pagination total={data.totalPages} value={page} onChange={setPage} />
              </Group>
            )}
          </Stack>
        )}
      </QueryState>
      {editing && (
        <UserModal user={editing === 'new' ? null : editing} onClose={() => setEditing(null)} />
      )}
    </>
  )
}

function UserModal({ user, onClose }: { user: UserAccount | null; onClose: () => void }) {
  const queryClient = useQueryClient()
  const departments = useDepartments()
  const [email, setEmail] = useState(user?.email ?? '')
  const [fullName, setFullName] = useState(user?.fullName ?? '')
  const [role, setRole] = useState<Role>(user?.role ?? 'employee')
  const [departmentId, setDepartmentId] = useState<string | null>(
    user ? String(user.departmentId) : null,
  )
  const [isActive, setIsActive] = useState(user?.isActive ?? true)
  const [password, setPassword] = useState('')
  const passwordWeak =
    password.length > 0 &&
    (password.length < PASSWORD_MIN || !/\d/.test(password) || !/[A-Za-z]/.test(password))
  const invalid =
    !fullName.trim() ||
    !departmentId ||
    (user === null && (!email.trim() || password.length === 0)) ||
    passwordWeak

  const save = useMutation({
    mutationFn: async () => {
      const body = { fullName: fullName.trim(), role, departmentId: Number(departmentId), isActive }
      if (user === null) {
        return adminApi.createUser({ ...body, email: email.trim(), password })
      }
      const updated = await adminApi.updateUser(user.id, body)
      if (password) {
        await adminApi.setPassword(user.id, password)
      }
      return updated
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['users'] })
      notifySuccess('The user was saved.')
      onClose()
    },
    onError: (error) => notifyError(error, 'Could not save the user'),
  })

  return (
    <Modal opened onClose={onClose} title={user ? 'Edit user' : 'Add user'}>
      <Stack>
        <TextInput
          label="E-mail"
          type="email"
          required
          disabled={user !== null}
          value={email}
          onChange={(event) => setEmail(event.currentTarget.value)}
        />
        <TextInput
          label="Full name"
          required
          value={fullName}
          onChange={(event) => setFullName(event.currentTarget.value)}
        />
        <Select
          label="Role"
          data={roles}
          value={role}
          onChange={(value) => setRole((value as Role | null) ?? 'employee')}
          allowDeselect={false}
        />
        <Select
          label="Department"
          required
          data={(departments.data ?? []).map((department) => ({
            value: String(department.id),
            label: department.name,
          }))}
          value={departmentId}
          onChange={setDepartmentId}
        />
        {user && (
          <Checkbox
            label="Active"
            checked={isActive}
            onChange={(event) => setIsActive(event.currentTarget.checked)}
          />
        )}
        <PasswordInput
          label={user ? 'New password' : 'Password'}
          description={`At least ${PASSWORD_MIN} characters with letters and digits${user ? '; leave empty to keep the current one' : ''}`}
          required={user === null}
          value={password}
          error={passwordWeak ? 'The password is too weak.' : undefined}
          onChange={(event) => setPassword(event.currentTarget.value)}
        />
        <Button disabled={invalid} loading={save.isPending} onClick={() => save.mutate()}>
          Save
        </Button>
      </Stack>
    </Modal>
  )
}
