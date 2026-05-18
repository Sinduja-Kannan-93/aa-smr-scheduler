import './AppShell.css'
import { type ReactNode } from 'react'
import { useIdentity } from '../contexts/IdentityContext'
import { Spinner } from './ui/Spinner'
import { type UserRole } from '../lib/api'

interface AppShellProps {
  children: ReactNode
}

const roleLabels: Record<UserRole, string> = {
  Admin: 'Admin',
  BookingAgent: 'Booking Agent',
  Mechanic: 'Mechanic',
}

export function AppShell({ children }: AppShellProps) {
  const { users, activeUser, isLoading, switchUser } = useIdentity()

  return (
    <div className="shell">
      <header className="shell-header" role="banner">
        <div className="shell-header__inner">
          <div className="shell-header__brand">
            <AaLogo />
            <div className="shell-header__brand-text">
              <span className="shell-header__brand-name">AA SMR Scheduler</span>
              <span className="shell-header__brand-sub">Service Management</span>
            </div>
          </div>

          <div className="shell-header__right">
            {isLoading ? (
              <Spinner size="sm" color="white" />
            ) : (
              <div className="act-as">
                <label className="act-as__label" htmlFor="act-as-select">
                  Viewing as
                </label>
                <div className="act-as__select-wrap">
                  <select
                    id="act-as-select"
                    className="act-as__select"
                    value={activeUser?.id ?? ''}
                    onChange={e => switchUser(Number(e.target.value))}
                    aria-label="Switch active user"
                  >
                    {users.map(u => (
                      <option key={u.id} value={u.id}>
                        {u.fullName} — {roleLabels[u.role]}
                      </option>
                    ))}
                  </select>
                  <svg className="act-as__arrow" width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true">
                    <path d="M4 6l4 4 4-4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                  </svg>
                </div>
                {activeUser && (
                  <span className="act-as__role-badge">
                    {roleLabels[activeUser.role]}
                  </span>
                )}
              </div>
            )}
          </div>
        </div>
      </header>

      <main className="shell-main" id="main-content">
        <div className="shell-main__inner">
          {children}
        </div>
      </main>
    </div>
  )
}

function AaLogo() {
  return (
    <svg className="shell-header__logo" viewBox="0 0 48 48" fill="none" aria-label="AA" role="img">
      <rect width="48" height="48" rx="6" fill="#F5A800"/>
      <text
        x="24" y="33"
        textAnchor="middle"
        fill="#002D72"
        fontSize="22"
        fontWeight="700"
        fontFamily="Inter, sans-serif"
        letterSpacing="-1"
      >
        AA
      </text>
    </svg>
  )
}
