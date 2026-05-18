import './MechanicPage.css'
import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  api, qk, formatDate, formatTime, formatDateTime,
  toDateInputValue, nextStatus, nextStatusLabel,
  type AppointmentListItem, type AppointmentDetail,
} from '../lib/api'
import { useIdentity } from '../contexts/IdentityContext'
import { Button } from '../components/ui/Button'
import { StatusBadge } from '../components/ui/Badge'
import { Card, CardHeader } from '../components/ui/Card'
import { Input, Textarea } from '../components/ui/Input'
import { Dialog } from '../components/ui/Dialog'
import { EmptyState, ClipboardIcon } from '../components/ui/EmptyState'
import { Spinner } from '../components/ui/Spinner'

export function MechanicPage() {
  const { activeUser } = useIdentity()
  const mechanicId = activeUser?.mechanicId ?? ''

  const today = new Date()
  const nextWeek = new Date(today)
  nextWeek.setDate(today.getDate() + 7)

  const [from, setFrom] = useState(toDateInputValue(today))
  const [to, setTo] = useState(toDateInputValue(nextWeek))
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const listParams = { mechanicId, from, to }

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: qk.appointments(listParams),
    queryFn: () => api.appointments.list(listParams),
    enabled: !!mechanicId,
  })

  return (
    <div className="mechanic-page">
      <div className="page-header">
        <h1 className="page-header__title">My Schedule</h1>
        <p className="page-header__subtitle">{activeUser?.fullName}</p>
      </div>

      <div className="mechanic-filters">
        <Input label="From" type="date" value={from} onChange={e => setFrom(e.target.value)} />
        <Input label="To" type="date" value={to} onChange={e => setTo(e.target.value)} min={from} />
      </div>

      {isLoading && (
        <div className="mechanic-loading">
          <Spinner size="lg" />
          <p>Loading your appointments…</p>
        </div>
      )}

      {!isLoading && appointments.length === 0 && (
        <EmptyState
          icon={<ClipboardIcon />}
          title="No appointments in this range"
          description="Try widening your date range."
        />
      )}

      {!isLoading && appointments.length > 0 && (
        <div className="mechanic-list">
          {appointments.map(appt => (
            <AppointmentRow key={appt.id} appt={appt} onOpen={() => setSelectedId(appt.id)} />
          ))}
        </div>
      )}

      <AppointmentDetailDialog
        appointmentId={selectedId}
        onClose={() => setSelectedId(null)}
      />
    </div>
  )
}

function AppointmentRow({ appt, onOpen }: { appt: AppointmentListItem; onOpen: () => void }) {
  return (
    <Card className="appt-card" padding="sm">
      <div className="appt-card__inner">
        <div className="appt-card__time-col">
          <span className="appt-card__date">{formatDate(appt.startUtc)}</span>
          <span className="appt-card__time">
            {formatTime(appt.startUtc)} – {formatTime(appt.endUtc)}
          </span>
        </div>
        <div className="appt-card__info">
          <span className="appt-card__customer">{appt.customerName}</span>
          <span className="appt-card__service">{appt.serviceTypeName}</span>
          <span className="appt-card__reg">{appt.vehicleRegistration}</span>
        </div>
        <div className="appt-card__status">
          <StatusBadge status={appt.status} />
        </div>
        <Button variant="ghost" size="sm" onClick={onOpen}>
          View
        </Button>
      </div>
    </Card>
  )
}

function AppointmentDetailDialog({
  appointmentId,
  onClose,
}: {
  appointmentId: string | null
  onClose: () => void
}) {
  const qc = useQueryClient()
  const [note, setNote] = useState('')
  const [noShowOpen, setNoShowOpen] = useState(false)

  const { data, isLoading } = useQuery({
    queryKey: qk.appointment(appointmentId ?? ''),
    queryFn: () => api.appointments.get(appointmentId!),
    enabled: appointmentId !== null,
  })

  const { mutate: addNote, isPending: addingNote } = useMutation({
    mutationFn: (content: string) => api.appointments.addNote(appointmentId!, content),
    onSuccess: () => {
      setNote('')
      qc.invalidateQueries({ queryKey: qk.appointment(appointmentId!) })
    },
  })

  const { mutate: updateStatus, isPending: updatingStatus } = useMutation({
    mutationFn: (status: Parameters<typeof api.appointments.patchStatus>[1]) =>
      api.appointments.patchStatus(appointmentId!, status),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.appointment(appointmentId!) })
      qc.invalidateQueries({ queryKey: ['appointments', 'list'] })
    },
  })

  const next = data ? nextStatus(data.status) : null
  const nextLabel = data ? nextStatusLabel(data.status) : null

  return (
    <Dialog
      open={appointmentId !== null}
      onClose={onClose}
      title="Appointment Detail"
      size="lg"
      footer={
        data ? (
          <div className="detail-footer">
            {next && (
              <Button
                variant="accent"
                loading={updatingStatus}
                onClick={() => updateStatus(next)}
              >
                {nextLabel}
              </Button>
            )}
            {data.status === 'Scheduled' && (
              <Button variant="ghost" onClick={() => setNoShowOpen(true)}>
                Mark No Show
              </Button>
            )}
            <Button variant="ghost" onClick={onClose}>Close</Button>
          </div>
        ) : undefined
      }
    >
      {isLoading && (
        <div className="detail-loading">
          <Spinner size="md" />
        </div>
      )}

      {data && (
        <AppointmentDetailView
          data={data}
          note={note}
          onNoteChange={setNote}
          onAddNote={() => addNote(note)}
          addingNote={addingNote}
        />
      )}

      {/* No show confirm */}
      <Dialog
        open={noShowOpen}
        onClose={() => setNoShowOpen(false)}
        title="Confirm No Show"
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => setNoShowOpen(false)}>Cancel</Button>
            <Button
              variant="danger"
              loading={updatingStatus}
              onClick={() => { updateStatus('NoShow'); setNoShowOpen(false) }}
            >
              Mark No Show
            </Button>
          </>
        }
      >
        <p className="confirm-text">
          This appointment will be marked as No Show and cannot be reverted. Continue?
        </p>
      </Dialog>
    </Dialog>
  )
}

function AppointmentDetailView({
  data,
  note,
  onNoteChange,
  onAddNote,
  addingNote,
}: {
  data: AppointmentDetail
  note: string
  onNoteChange: (v: string) => void
  onAddNote: () => void
  addingNote: boolean
}) {
  return (
    <>
      {/* Header summary */}
      <div className="detail-summary">
        <div className="detail-summary__row">
          <span className="detail-summary__ref">{data.referenceNumber}</span>
          <StatusBadge status={data.status} />
        </div>
        <div className="detail-summary__grid">
          <DetailField label="Customer" value={data.customerName} />
          <DetailField label="Vehicle" value={data.vehicleRegistration} />
          <DetailField label="Phone" value={data.customerPhone} />
          <DetailField label="Service" value={data.serviceTypeName} />
          <DetailField label="Branch" value={data.branchName} />
          <DetailField
            label="Slot"
            value={`${formatDate(data.startUtc)}, ${formatTime(data.startUtc)}–${formatTime(data.endUtc)}`}
          />
        </div>
        {data.notes && (
          <div className="detail-notes-block">
            <span className="detail-summary__label">Booking notes</span>
            <p className="detail-notes-block__text">{data.notes}</p>
          </div>
        )}
      </div>

      {/* Work notes */}
      <div className="work-notes">
        <h3 className="work-notes__heading">Work Notes ({data.workNotes.length})</h3>
        {data.workNotes.length === 0 ? (
          <p className="work-notes__empty">No work notes yet.</p>
        ) : (
          <ul className="work-notes__list">
            {data.workNotes.map(wn => (
              <li key={wn.id} className="work-note">
                <p className="work-note__content">{wn.body}</p>
                <p className="work-note__meta">
                  {wn.authorName} · {formatDateTime(wn.createdUtc)}
                </p>
              </li>
            ))}
          </ul>
        )}

        {data.status !== 'Completed' && data.status !== 'NoShow' && (
          <div className="work-notes__add">
            <Textarea
              label="Add a work note"
              value={note}
              onChange={e => onNoteChange(e.target.value)}
              rows={3}
              placeholder="Describe work performed…"
            />
            <Button
              variant="primary"
              size="sm"
              onClick={onAddNote}
              disabled={!note.trim()}
              loading={addingNote}
            >
              Add Note
            </Button>
          </div>
        )}
      </div>
    </>
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
