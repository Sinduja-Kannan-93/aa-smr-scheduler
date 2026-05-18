import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { QueryClient } from '@tanstack/react-query'
import { IdentityProvider, useIdentity } from '../contexts/IdentityContext'
import { type User } from '../lib/api'

// vi.mock is hoisted — data must be inline, not referencing outer const
vi.mock('../lib/api', async importOriginal => {
  const actual = await importOriginal<typeof import('../lib/api')>()
  return {
    ...actual,
    api: {
      ...actual.api,
      users: {
        list: vi.fn().mockResolvedValue([
          { id: 1, fullName: 'Alice Admin', role: 'Admin', mechanicId: null, mechanicName: null, branchId: 1 },
          { id: 2, fullName: 'Bob Agent', role: 'BookingAgent', mechanicId: null, mechanicName: null, branchId: 1 },
          { id: 3, fullName: 'Carlos Mechanic', role: 'Mechanic', mechanicId: 10, mechanicName: 'Carlos Mechanic', branchId: 1 },
        ] satisfies User[]),
      },
    },
  }
})

function TestConsumer() {
  const { activeUser, users, switchUser } = useIdentity()
  return (
    <div>
      <p data-testid="active">{activeUser?.fullName ?? 'none'}</p>
      <p data-testid="count">{users.length}</p>
      <button onClick={() => switchUser(2)}>Switch to Bob</button>
    </div>
  )
}

function wrapper({ children }: { children: React.ReactNode }) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return (
    <QueryClientProvider client={qc}>
      <IdentityProvider>{children}</IdentityProvider>
    </QueryClientProvider>
  )
}

describe('IdentityContext', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('defaults to first user', async () => {
    render(<TestConsumer />, { wrapper })
    await waitFor(() => expect(screen.getByTestId('active').textContent).toBe('Alice Admin'))
  })

  it('exposes the full user list', async () => {
    render(<TestConsumer />, { wrapper })
    await waitFor(() => expect(screen.getByTestId('count').textContent).toBe('3'))
  })

  it('switches active user when switchUser is called', async () => {
    render(<TestConsumer />, { wrapper })
    await waitFor(() => screen.getByText('Switch to Bob'))
    fireEvent.click(screen.getByText('Switch to Bob'))
    await waitFor(() => expect(screen.getByTestId('active').textContent).toBe('Bob Agent'))
  })

  it('throws when used outside provider', () => {
    const spy = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    expect(() => render(<TestConsumer />)).toThrow()
    spy.mockRestore()
  })
})
