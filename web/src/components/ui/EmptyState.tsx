import './EmptyState.css'
import { type ReactNode } from 'react'

interface EmptyStateProps {
  icon?: ReactNode
  title: string
  description?: string
  action?: ReactNode
}

export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="empty-state">
      {icon && <div className="empty-state__icon">{icon}</div>}
      <p className="empty-state__title">{title}</p>
      {description && <p className="empty-state__desc">{description}</p>}
      {action && <div className="empty-state__action">{action}</div>}
    </div>
  )
}

export function CalendarIcon() {
  return (
    <svg width="40" height="40" viewBox="0 0 40 40" fill="none" aria-hidden="true">
      <rect x="4" y="8" width="32" height="28" rx="4" stroke="currentColor" strokeWidth="2"/>
      <path d="M4 16h32" stroke="currentColor" strokeWidth="2"/>
      <path d="M13 4v8M27 4v8" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    </svg>
  )
}

export function ClipboardIcon() {
  return (
    <svg width="40" height="40" viewBox="0 0 40 40" fill="none" aria-hidden="true">
      <rect x="8" y="8" width="24" height="28" rx="3" stroke="currentColor" strokeWidth="2"/>
      <path d="M15 4h10v8H15z" stroke="currentColor" strokeWidth="2" strokeLinejoin="round"/>
      <path d="M14 20h12M14 26h8" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    </svg>
  )
}
