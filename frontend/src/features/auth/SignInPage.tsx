import { zodResolver } from '@hookform/resolvers/zod'
import {
  Alert,
  Button,
  Center,
  Paper,
  PasswordInput,
  Stack,
  Text,
  TextInput,
  Title,
} from '@mantine/core'
import { IconAlertCircle, IconBriefcase } from '@tabler/icons-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { z } from 'zod'
import { getErrorMessage } from '@/shared/api/apiError'
import { authApi } from '@/shared/api/endpoints'
import { useAuthStore } from './authStore'

const schema = z.object({
  email: z.email('Enter a valid e-mail address.'),
  password: z.string().min(1, 'Enter your password.'),
})

type SignInForm = z.infer<typeof schema>

export function SignInPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const status = useAuthStore((state) => state.status)
  const setSession = useAuthStore((state) => state.setSession)
  const [error, setError] = useState<string | null>(null)
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<SignInForm>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  })

  if (status === 'signedIn') {
    return <Navigate to="/" replace />
  }

  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const onSubmit = async (values: SignInForm) => {
    setError(null)
    try {
      setSession(await authApi.signIn(values.email, values.password))
      await navigate(from, { replace: true })
    } catch (caught) {
      setError(getErrorMessage(caught))
    }
  }

  return (
    <Center mih="100vh" p="md" bg="gray.0">
      <Paper withBorder shadow="sm" p="xl" radius="md" w="100%" maw={420}>
        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack>
            <Stack gap={4} align="center">
              <IconBriefcase size={40} aria-hidden />
              <Title order={2}>Equipment Booking</Title>
              <Text c="dimmed" size="sm">
                Sign in with your work e-mail address
              </Text>
            </Stack>
            {error && (
              <Alert color="red" icon={<IconAlertCircle size={18} />} role="alert">
                {error}
              </Alert>
            )}
            <TextInput
              label="E-mail"
              type="email"
              autoComplete="username"
              error={errors.email?.message}
              {...register('email')}
            />
            <PasswordInput
              label="Password"
              autoComplete="current-password"
              error={errors.password?.message}
              {...register('password')}
            />
            <Button type="submit" loading={isSubmitting} fullWidth>
              Sign in
            </Button>
          </Stack>
        </form>
      </Paper>
    </Center>
  )
}
