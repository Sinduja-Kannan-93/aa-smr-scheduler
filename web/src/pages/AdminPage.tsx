import './AdminPage.css'
import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api, qk, formatTime, formatDate, formatDateTime, type MechanicSchedule } from '../lib/api'
import { StatusBadge } from '../components/ui/Badge'
import { Card, CardHeader } from '../components/ui/Card'
import { Dialog } from '../components/ui/Dialog'
import { Button } from '../components/ui/Button'
import { EmptyState, CalendarIcon } from '../components/ui/EmptyState'
import { Spinner } from '../components/ui/Spinner'

export function AdminPage() {
  const today = new Date()
  const todayLabel = today.toLocaleDateString('en-IE', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
  })

  const [selectedId, setSelectedId] = useState<string | null>(null)

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
            <MechanicCard key={mechanic.mechanicId} mechanic={mechanic} onOpen={setSelectedId} />
          ))}
        </div>
      )}

      <AppointmentDetailDialog appointmentId={selectedId} onClose={() => setSelectedId(null)} />
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

function MechanicCard({ mechanic, onOpen }: { mechanic: MechanicSchedule; onOpen: (id: string) => void }) {
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
            <li key={appt.id} className="appt-row" onClick={() => onOpen(appt.id)} role="button" tabIndex={0}
              onKeyDown={e => e.key === 'Enter' && onOpen(appt.id)}>
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

function AppointmentDetailDialog({ appointmentId, onClose }: { appointmentId: string | null; onClose: () => void }) {
  const { data, isLoading } = useQuery({
    queryKey: qk.appointment(appointmentId ?? ''),
    queryFn: () => api.appointments.get(appointmentId!),
    enabled: appointmentId !== null,
  })

  return (
    <Dialog
      open={appointmentId !== null}
      onClose={onClose}
      title="Appointment Detail"
      size="lg"
      footer={<Button variant="ghost" onClick={onClose}>Close</Button>}
    >
      {isLoading && (
        <div className="detail-loading"><Spinner size="md" /></div>
      )}
      {data && (
        <div className="admin-detail">
          <div className="admin-detail__summary">
            <div className="admin-detail__row">
              <span className="admin-detail__ref">{data.referenceNumber}</span>
              <StatusBadge status={data.status} />
            </div>
            <div className="admin-detail__grid">
              <DetailField label="Customer" value={data.customerName} />
              <DetailField label="Vehicle" value={data.vehicleRegistration} />
              <DetailField label="Phone" value={data.customerPhone} />
              <DetailField label="Service" value={data.serviceTypeName} />
              <DetailField label="Branch" value={data.branchName} />
              <DetailField label="Slot" value={`${formatDate(data.startUtc)}, ${formatTime(data.startUtc)}–${formatTime(data.endUtc)}`} />
            </div>
            {data.notes && (
              <div className="admin-detail__notes-block">
                <span className="admin-detail__label">Booking notes</span>
                <p>{data.notes}</p>
              </div>
            )}
          </div>

          <div className="work-notes">
            <h3 className="work-notes__heading">Work Notes ({data.workNotes.length})</h3>
            {data.workNotes.length === 0 ? (
              <p className="work-notes__empty">No work notes yet.</p>
            ) : (
              <ul className="work-notes__list">
                {data.workNotes.map(wn => (
                  <li key={wn.id} className="work-note">
                    <p className="work-note__content">{wn.body}</p>
                    <p className="work-note__meta">{wn.authorName} · {formatDateTime(wn.createdUtc)}</p>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </Dialog>
  )
}

function DetailField({ label, value }: { label: string; value: string }) {
  return (
    <div className="detail-field">
      <span className="detail-field__label">{label}</span>
      <span className="detail-field__value">{value}</span>
    </div>
  )
}
