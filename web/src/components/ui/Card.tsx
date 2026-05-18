import './Card.css'
import { type ReactNode } from 'react'

interface CardProps {
  children: ReactNode
  className?: string
  padding?: 'sm' | 'md' | 'lg'
}

interface CardHeaderProps {
  title: string
  subtitle?: string
  action?: ReactNode
}

export function Card({ children, className = '', padding = 'md' }: CardProps) {
  return (
    <div className={`card card--pad-${padding} ${className}`}>
      {children}
    </div>
  )
}

export function CardHeader({ title, subtitle, action }: CardHeaderProps) {
  return (
    <div className="card-header">
      <div className="card-header__text">
        <h3 className="card-header__title">{title}</h3>
        {subtitle && <p className="card-header__subtitle">{subtitle}</p>}
      </div>
      {action && <div className="card-header__action">{action}</div>}
    </div>
  )
}
