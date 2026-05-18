import { describe, it, expect } from 'vitest'
import {
  formatTime, formatDate, formatDateTime,
  toDateInputValue, nextStatus, nextStatusLabel,
  UserSchema, SlotSchema, AppointmentStatusSchema,
} from '../lib/api'

describe('formatTime', () => {
  it('formats ISO string to HH:MM', () => {
    const result = formatTime('2024-06-15T09:30:00')
    expect(result).toMatch(/09:30/)
  })
})

describe('formatDate', () => {
  it('returns weekday, day and month', () => {
    const result = formatDate('2024-06-15T09:30:00')
    expect(result).toMatch(/Jun|15/)
  })
})

describe('formatDateTime', () => {
  it('includes day and time', () => {
    const result = formatDateTime('2024-06-15T09:30:00')
    expect(result).toMatch(/15/)
    expect(result).toMatch(/09:30/)
  })
})

describe('toDateInputValue', () => {
  it('returns YYYY-MM-DD format', () => {
    const d = new Date('2024-06-15')
    expect(toDateInputValue(d)).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  })
})

describe('nextStatus', () => {
  it('Scheduled → InProgress', () => {
    expect(nextStatus('Scheduled')).toBe('InProgress')
  })

  it('InProgress → Completed', () => {
    expect(nextStatus('InProgress')).toBe('Completed')
  })

  it('Completed → null', () => {
    expect(nextStatus('Completed')).toBeNull()
  })

  it('NoShow → null', () => {
    expect(nextStatus('NoShow')).toBeNull()
  })
})

describe('nextStatusLabel', () => {
  it('returns label for Scheduled', () => {
    expect(nextStatusLabel('Scheduled')).toBe('Start (In Progress)')
  })

  it('returns label for InProgress', () => {
    expect(nextStatusLabel('InProgress')).toBe('Complete')
  })

  it('returns null for terminal statuses', () => {
    expect(nextStatusLabel('Completed')).toBeNull()
    expect(nextStatusLabel('NoShow')).toBeNull()
  })
})

describe('Zod schemas', () => {
  it('parses valid UserRole', () => {
    expect(UserSchema.shape.role.parse('Admin')).toBe('Admin')
    expect(UserSchema.shape.role.parse('BookingAgent')).toBe('BookingAgent')
    expect(UserSchema.shape.role.parse('Mechanic')).toBe('Mechanic')
  })

  it('rejects unknown UserRole', () => {
    expect(() => UserSchema.shape.role.parse('SuperAdmin')).toThrow()
  })

  it('parses valid AppointmentStatus', () => {
    const statuses = ['Scheduled', 'InProgress', 'Completed', 'NoShow']
    statuses.forEach(s => expect(AppointmentStatusSchema.parse(s)).toBe(s))
  })

  it('parses a valid Slot', () => {
    const slot = SlotSchema.parse({
      id: 1,
      startTime: '2024-06-15T09:00:00',
      endTime: '2024-06-15T10:00:00',
      mechanicId: 2,
      mechanicName: 'Joe',
      branchId: 1,
      branchName: 'Dublin',
      serviceTypeId: 3,
      serviceTypeName: 'Oil Change',
      durationMinutes: 60,
    })
    expect(slot.id).toBe(1)
    expect(slot.durationMinutes).toBe(60)
  })
})
