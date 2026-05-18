import './Badge.css'
import { type AppointmentStatus } from '../../lib/api'

type BadgeVariant = AppointmentStatus | 'neutral' | 'navy'

interface BadgeProps {
  variant?: BadgeVariant
  children: React.ReactNode
}

export function Badge({ variant = 'neutral', children }: BadgeProps) {
  return (
    <span className={`badge badge--${variant.toLowerCase()}`}>
      {children}
    </span>
  )
}

export function StatusBadge({ status }: { status: AppointmentStatus }) {
  const labels: Record<AppointmentStatus, string> = {
    Scheduled: 'Scheduled',
    InProgress: 'In Progress',
    Completed: 'Completed',
    NoShow: 'No Show',
  }
  return <Badge variant={status}>{labels[status]}</Badge>
}
