import './AdminPage.css'
import { useQuery } from '@tanstack/react-query'
import { api, qk, formatTime, formatDate, type MechanicSchedule } from '../lib/api'
import { StatusBadge } from '../components/ui/Badge'
import { Card, CardHeader } from '../components/ui/Card'
import { EmptyState, CalendarIcon } from '../components/ui/EmptyState'
import { Spinner } from '../components/ui/Spinner'

export function AdminPage() {
  const today = new Date()
  const todayLabel = today.toLocaleDateString('en-IE', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
  })

  const { data: schedule = [], isLoading, isError } = useQuery({
    queryKey: qk.today(),
    queryFn: api.appointments.today,
    refetchInterval: 60_000,
  })

  const totalCount = schedule.reduce((sum, m) => sum + m.appointments.length, 0)

  return (
    <div className="admin-page">
      <div className="page-header">
        <h1 className="page-header__title">Today's Schedule</h1>
        <p className="page-header__subtitle">{todayLabel}</p>
      </div>

      <div className="admin-stats">
        <StatCard label="Mechanics on duty" value={schedule.length} />
        <StatCard label="Total appointments" value={totalCount} />
        <StatCard
          label="In Progress"
          value={schedule.flatMap(m => m.appointments).filter(a => a.status === 'InProgress').length}
          accent
        />
        <StatCard
          label="Completed"
          value={schedule.flatMap(m => m.appointments).filter(a => a.status === 'Completed').length}
        />
      </div>

      {isLoading && (
        <div className="admin-loading">
          <Spinner size="lg" />
          <p>Loading today's schedule…</p>
        </div>
      )}

      {isError && (
        <EmptyState
          title="Could not load schedule"
          description="The API may be offline. Retrying every 60 seconds."
        />
      )}

      {!isLoading && !isError && schedule.length === 0 && (
        <EmptyState
          icon={<CalendarIcon />}
          title="No appointments today"
          description="There are no appointments scheduled for today across any branch."
        />
      )}

      {!isLoading && !isError && schedule.length > 0 && (
        <div className="admin-grid">
          {schedule.map(mechanic => (
            <MechanicCard key={mechanic.mechanicId} mechanic={mechanic} />
          ))}
        </div>
      )}
    </div>
  )
}

function StatCard({ label, value, accent = false }: { label: string; value: number; accent?: boolean }) {
  return (
    <div className={`stat-card ${accent ? 'stat-card--accent' : ''}`}>
      <span className="stat-card__value">{value}</span>
      <span className="stat-card__label">{label}</span>
    </div>
  )
}

function MechanicCard({ mechanic }: { mechanic: MechanicSchedule }) {
  const { mechanicName, appointments } = mechanic
  const completed = appointments.filter(a => a.status === 'Completed').length
  const inProgress = appointments.filter(a => a.status === 'InProgress').length

  return (
    <Card className="mechanic-card">
      <CardHeader
        title={mechanicName}
        subtitle={`${appointments.length} appointment${appointments.length !== 1 ? 's' : ''} · ${completed} done · ${inProgress} in progress`}
      />
      {appointments.length === 0 ? (
        <p className="mechanic-card__empty">No appointments</p>
      ) : (
        <ul className="mechanic-card__list">
          {appointments.map(appt => (
            <li key={appt.id} className="appt-row">
              <div className="appt-row__time">
                <span className="appt-row__time-start">{formatTime(appt.startUtc)}</span>
                <span className="appt-row__time-sep">–</span>
                <span className="appt-row__time-end">{formatTime(appt.endUtc)}</span>
              </div>
              <div className="appt-row__info">
                <span className="appt-row__customer">{appt.customerName}</span>
                <span className="appt-row__detail">
                  {appt.vehicleRegistration} · {appt.serviceTypeName}
                </span>
              </div>
              <div className="appt-row__status">
                <StatusBadge status={appt.status} />
              </div>
            </li>
          ))}
        </ul>
      )}
    </Card>
  )
}
