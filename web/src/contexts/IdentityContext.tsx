import { createContext, useContext, useState, useCallback, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api, qk, type User } from '../lib/api'
import { queryClient } from '../lib/queryClient'

interface IdentityContextValue {
  users: User[]
  activeUser: User | null
  isLoading: boolean
  switchUser: (userId: string) => void
}

const IdentityContext = createContext<IdentityContextValue | null>(null)

export function IdentityProvider({ children }: { children: ReactNode }) {
  const { data: users = [], isLoading } = useQuery({
    queryKey: qk.users(),
    queryFn: api.users.list,
  })

  const [activeUserId, setActiveUserId] = useState<string | null>(null)

  const activeUser = activeUserId
    ? (users.find(u => u.id === activeUserId) ?? null)
    : (users[0] ?? null)

  const switchUser = useCallback((userId: string) => {
    setActiveUserId(userId)
    // Reset all cached data so the new view starts fresh
    queryClient.removeQueries({ queryKey: ['appointments'] })
    queryClient.removeQueries({ queryKey: ['slots'] })
  }, [])

  return (
    <IdentityContext.Provider value={{ users, activeUser, isLoading, switchUser }}>
      {children}
    </IdentityContext.Provider>
  )
}

export function useIdentity(): IdentityContextValue {
  const ctx = useContext(IdentityContext)
  if (!ctx) throw new Error('useIdentity must be used inside IdentityProvider')
  return ctx
}
