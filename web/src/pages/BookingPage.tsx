import './BookingPage.css'
import { useState, type FormEvent } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  api, qk, formatDate, formatTime, toDateInputValue,
  type Slot, type BookAppointmentResponse,
} from '../lib/api'
import { Button } from '../components/ui/Button'
import { Input, Textarea } from '../components/ui/Input'
import { Select } from '../components/ui/Select'
import { Dialog } from '../components/ui/Dialog'
import { Card } from '../components/ui/Card'
import { EmptyState, CalendarIcon } from '../components/ui/EmptyState'
import { Spinner } from '../components/ui/Spinner'

export function BookingPage() {
  const qc = useQueryClient()

  const today = new Date()
  const nextWeek = new Date(today)
  nextWeek.setDate(today.getDate() + 7)

  const [from, setFrom] = useState(toDateInputValue(today))
  const [to, setTo] = useState(toDateInputValue(nextWeek))
  const [serviceTypeId, setServiceTypeId] = useState('')
  const [branchId, setBranchId] = useState('')
  const [selectedSlot, setSelectedSlot] = useState<Slot | null>(null)
  const [confirmed, setConfirmed] = useState<BookAppointmentResponse | null>(null)

  const { data: serviceTypes = [] } = useQuery({
    queryKey: qk.serviceTypes(),
    queryFn: api.serviceTypes.list,
  })

  const { data: branches = [] } = useQuery({
    queryKey: qk.branches(),
    queryFn: api.branches.list,
  })

  const slotsParams = { from, to, serviceTypeId: serviceTypeId ? Number(serviceTypeId) : undefined, branchId: branchId ? Number(branchId) : undefined }

  const { data: slots = [], isLoading: slotsLoading } = useQuery({
    queryKey: qk.slots(slotsParams),
    queryFn: () => api.slots.list(slotsParams),
  })

  const { mutate: bookSlot, isPending: booking } = useMutation({
    mutationFn: api.appointments.book,
    onSuccess: result => {
      setConfirmed(result)
      setSelectedSlot(null)
      qc.invalidateQueries({ queryKey: qk.slots(slotsParams) })
      qc.invalidateQueries({ queryKey: ['appointments'] })
    },
  })

  const handleBook = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!selectedSlot) return
    const fd = new FormData(e.currentTarget)
    bookSlot({
      slotId: selectedSlot.id,
      customerName: fd.get('customerName') as string,
      vehicleReg: fd.get('vehicleReg') as string,
      phoneNumber: fd.get('phoneNumber') as string,
      notes: fd.get('notes') as string,
    })
  }

  const grouped = groupByDate(slots)

  return (
    <div className="booking-page">
      <div className="page-header">
        <h1 className="page-header__title">Book an Appointment</h1>
        <p className="page-header__subtitle">Browse available slots and book for a customer</p>
      </div>

      {/* Filters */}
      <Card className="booking-filters">
        <div className="booking-filters__grid">
          <Input label="From" type="date" value={from} onChange={e => setFrom(e.target.value)} min={toDateInputValue(today)} />
          <Input label="To" type="date" value={to} onChange={e => setTo(e.target.value)} min={from} />
          <Select
            label="Service type"
            options={serviceTypes.map(s => ({ value: s.id, label: s.name }))}
            placeholder="All services"
            value={serviceTypeId}
            onChange={e => setServiceTypeId(e.target.value)}
          />
          <Select
            label="Branch"
            options={branches.map(b => ({ value: b.id, label: b.name }))}
            placeholder="All branches"
            value={branchId}
            onChange={e => setBranchId(e.target.value)}
          />
        </div>
      </Card>

      {/* Slot list */}
      <div className="booking-results">
        {slotsLoading ? (
          <div className="booking-loading">
            <Spinner size="lg" />
            <p>Searching for available slots…</p>
          </div>
        ) : slots.length === 0 ? (
          <EmptyState
            icon={<CalendarIcon />}
            title="No available slots"
            description="Try adjusting your filters or selecting a wider date range."
          />
        ) : (
          Object.entries(grouped).map(([date, daySlots]) => (
            <div key={date} className="slot-group">
              <h2 className="slot-group__date">{date}</h2>
              <div className="slot-grid">
                {daySlots.map(slot => (
                  <SlotCard key={slot.id} slot={slot} onBook={() => setSelectedSlot(slot)} />
                ))}
              </div>
            </div>
          ))
        )}
      </div>

      {/* Book dialog */}
      <Dialog
        open={selectedSlot !== null}
        onClose={() => setSelectedSlot(null)}
        title="Book Appointment"
        footer={
          <>
            <Button variant="ghost" onClick={() => setSelectedSlot(null)} disabled={booking}>Cancel</Button>
            <Button variant="accent" form="book-form" type="submit" loading={booking}>Confirm Booking</Button>
          </>
        }
      >
        {selectedSlot && (
          <>
            <SlotSummary slot={selectedSlot} />
            <form id="book-form" onSubmit={handleBook} className="book-form">
              <Input label="Customer name" name="customerName" required placeholder="Full name" />
              <Input label="Vehicle registration" name="vehicleReg" required placeholder="e.g. 241-D-12345" />
              <Input label="Phone number" name="phoneNumber" type="tel" required placeholder="+353…" />
              <Textarea label="Notes" name="notes" placeholder="Any additional notes…" rows={3} />
            </form>
          </>
        )}
      </Dialog>

      {/* Confirmation dialog */}
      <Dialog
        open={confirmed !== null}
        onClose={() => setConfirmed(null)}
        title="Booking Confirmed"
        footer={<Button variant="accent" onClick={() => setConfirmed(null)}>Done</Button>}
      >
        {confirmed && (
          <div className="booking-confirmed">
            <div className="booking-confirmed__check">
              <svg width="32" height="32" viewBox="0 0 32 32" fill="none" aria-hidden="true">
                <circle cx="16" cy="16" r="16" fill="#F0FDF4"/>
                <path d="M10 16l4 4 8-8" stroke="#16A34A" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </div>
            <p className="booking-confirmed__label">Reference number</p>
            <p className="booking-confirmed__ref">{confirmed.referenceNumber}</p>
            <p className="booking-confirmed__hint">Share this reference with the customer for check-in.</p>
          </div>
        )}
      </Dialog>
    </div>
  )
}

function SlotCard({ slot, onBook }: { slot: Slot; onBook: () => void }) {
  return (
    <div className="slot-card">
      <div className="slot-card__time">
        {formatTime(slot.startTime)} – {formatTime(slot.endTime)}
      </div>
      <div className="slot-card__service">{slot.serviceTypeName}</div>
      <div className="slot-card__meta">
        <span>{slot.mechanicName}</span>
        <span className="slot-card__dot">·</span>
        <span>{slot.branchName}</span>
      </div>
      <Button variant="accent" size="sm" onClick={onBook} className="slot-card__btn">
        Book
      </Button>
    </div>
  )
}

function SlotSummary({ slot }: { slot: Slot }) {
  return (
    <div className="slot-summary">
      <div className="slot-summary__row">
        <span className="slot-summary__key">Date</span>
        <span className="slot-summary__val">{formatDate(slot.startTime)}</span>
      </div>
      <div className="slot-summary__row">
        <span className="slot-summary__key">Time</span>
        <span className="slot-summary__val">{formatTime(slot.startTime)} – {formatTime(slot.endTime)}</span>
      </div>
      <div className="slot-summary__row">
        <span className="slot-summary__key">Service</span>
        <span className="slot-summary__val">{slot.serviceTypeName} ({slot.durationMinutes} min)</span>
      </div>
      <div className="slot-summary__row">
        <span className="slot-summary__key">Mechanic</span>
        <span className="slot-summary__val">{slot.mechanicName}</span>
      </div>
      <div className="slot-summary__row">
        <span className="slot-summary__key">Branch</span>
        <span className="slot-summary__val">{slot.branchName}</span>
      </div>
    </div>
  )
}

function groupByDate(slots: Slot[]): Record<string, Slot[]> {
  return slots.reduce<Record<string, Slot[]>>((acc, slot) => {
    const key = formatDate(slot.startTime)
    return { ...acc, [key]: [...(acc[key] ?? []), slot] }
  }, {})
}
