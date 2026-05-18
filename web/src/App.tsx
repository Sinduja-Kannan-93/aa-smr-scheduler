import { useIdentity } from './contexts/IdentityContext'
import { AppShell } from './components/AppShell'
import { AdminPage } from './pages/AdminPage'
import { BookingPage } from './pages/BookingPage'
import { MechanicPage } from './pages/MechanicPage'
import { Spinner } from './components/ui/Spinner'

function RoleView() {
  const { activeUser, isLoading } = useIdentity()

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: '4rem' }}>
        <Spinner size="lg" />
      </div>
    )
  }

  if (!activeUser) {
    return (
      <div style={{ textAlign: 'center', padding: '4rem', color: 'var(--color-text-muted)' }}>
        No users found. Ensure the API is running and seeded.
      </div>
    )
  }

  if (activeUser.role === 'Admin') return <AdminPage />
  if (activeUser.role === 'BookingAgent') return <BookingPage />
  if (activeUser.role === 'Mechanic') return <MechanicPage />

  return null
}

export default function App() {
  return (
    <AppShell>
      <RoleView />
    </AppShell>
  )
}
