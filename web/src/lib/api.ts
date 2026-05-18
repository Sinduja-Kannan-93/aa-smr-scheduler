import { z } from 'zod'

// ── Response envelope ────────────────────────────────────
const apiResponse = <T extends z.ZodTypeAny>(data: T) =>
  z.object({ success: z.boolean(), data: data.nullable(), error: z.string().nullable() })

// ── Domain schemas ───────────────────────────────────────
export const UserRoleSchema = z.enum(['Admin', 'BookingAgent', 'Mechanic'])
export type UserRole = z.infer<typeof UserRoleSchema>

export const UserSchema = z.object({
  id: z.string(),
  fullName: z.string(),
  role: UserRoleSchema,
  mechanicId: z.string().nullable(),
  mechanicName: z.string().nullable(),
  branchId: z.string().nullable(),
})
export type User = z.infer<typeof UserSchema>

export const BranchSchema = z.object({
  id: z.string(),
  name: z.string(),
  city: z.string(),
  address: z.string(),
})
export type Branch = z.infer<typeof BranchSchema>

export const ServiceTypeSchema = z.object({
  id: z.string(),
  code: z.string(),
  name: z.string(),
  durationMinutes: z.number(),
})
export type ServiceType = z.infer<typeof ServiceTypeSchema>

export const SlotSchema = z.object({
  id: z.string(),
  startUtc: z.string(),
  endUtc: z.string(),
  mechanicId: z.string(),
  mechanicName: z.string(),
  branchId: z.string(),
  branchName: z.string(),
  serviceTypeId: z.string(),
  serviceTypeName: z.string(),
  durationMinutes: z.number(),
})
export type Slot = z.infer<typeof SlotSchema>

export const AppointmentStatusSchema = z.enum(['Scheduled', 'InProgress', 'Completed', 'NoShow'])
export type AppointmentStatus = z.infer<typeof AppointmentStatusSchema>

export const AppointmentListItemSchema = z.object({
  id: z.string(),
  referenceNumber: z.string(),
  startUtc: z.string(),
  endUtc: z.string(),
  customerName: z.string(),
  vehicleRegistration: z.string(),
  serviceTypeName: z.string(),
  status: AppointmentStatusSchema,
  mechanicName: z.string(),
  branchName: z.string(),
})
export type AppointmentListItem = z.infer<typeof AppointmentListItemSchema>

export const WorkNoteSchema = z.object({
  id: z.string(),
  body: z.string(),
  createdUtc: z.string(),
  authorName: z.string(),
})
export type WorkNote = z.infer<typeof WorkNoteSchema>

export const AppointmentDetailSchema = AppointmentListItemSchema.extend({
  customerPhone: z.string(),
  notes: z.string().nullable(),
  workNotes: z.array(WorkNoteSchema),
})
export type AppointmentDetail = z.infer<typeof AppointmentDetailSchema>

export const MechanicScheduleSchema = z.object({
  mechanicId: z.string(),
  mechanicName: z.string(),
  appointments: z.array(AppointmentListItemSchema),
})
export type MechanicSchedule = z.infer<typeof MechanicScheduleSchema>

export const BookAppointmentResponseSchema = z.object({
  id: z.string(),
  referenceNumber: z.string(),
})
export type BookAppointmentResponse = z.infer<typeof BookAppointmentResponseSchema>

// ── Request types ────────────────────────────────────────
export interface BookAppointmentRequest {
  slotId: string
  customerName: string
  customerPhone: string
  vehicleRegistration: string
  notes: string
}

// ── HTTP client ──────────────────────────────────────────
async function request<T>(schema: z.ZodType<T>, path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })

  const json: unknown = await res.json()

  if (!res.ok) {
    const parsed = z.object({ error: z.string().nullable() }).safeParse(json)
    const message = parsed.success ? (parsed.data.error ?? 'Request failed') : 'Request failed'
    throw new Error(message)
  }

  return schema.parse(json)
}

const wrap = <T extends z.ZodTypeAny>(schema: T) => apiResponse(schema)

// ── API functions ────────────────────────────────────────
export const api = {
  users: {
    list: () =>
      request(wrap(z.array(UserSchema)), '/api/users').then(r => r.data ?? []),
  },

  branches: {
    list: () =>
      request(wrap(z.array(BranchSchema)), '/api/branches').then(r => r.data ?? []),
  },

  serviceTypes: {
    list: () =>
      request(wrap(z.array(ServiceTypeSchema)), '/api/service-types').then(r => r.data ?? []),
  },

  slots: {
    list: (params: { from: string; to: string; serviceTypeId?: string; branchId?: string }) => {
      const q = new URLSearchParams({ from: params.from, to: params.to })
      if (params.serviceTypeId) q.set('serviceTypeId', params.serviceTypeId)
      if (params.branchId) q.set('branchId', params.branchId)
      return request(wrap(z.array(SlotSchema)), `/api/slots?${q}`).then(r => r.data ?? [])
    },
  },

  appointments: {
    book: (body: BookAppointmentRequest) =>
      request(
        wrap(BookAppointmentResponseSchema),
        '/api/appointments',
        { method: 'POST', body: JSON.stringify(body) }
      ).then(r => r.data!),

    today: () =>
      request(wrap(z.array(MechanicScheduleSchema)), '/api/appointments/today').then(r => r.data ?? []),

    list: (params: { mechanicId: string; from: string; to: string }) => {
      const q = new URLSearchParams({
        mechanicId: params.mechanicId,
        from: params.from,
        to: params.to,
      })
      return request(wrap(z.array(AppointmentListItemSchema)), `/api/appointments?${q}`).then(r => r.data ?? [])
    },

    get: (id: string) =>
      request(wrap(AppointmentDetailSchema), `/api/appointments/${id}`).then(r => r.data!),

    addNote: (id: string, body: string, authorMechanicId: string) =>
      request(
        wrap(z.object({}).passthrough()),
        `/api/appointments/${id}/notes`,
        { method: 'POST', body: JSON.stringify({ body, authorMechanicId }) }
      ),

    patchStatus: (id: string, status: AppointmentStatus) =>
      request(
        wrap(z.object({}).passthrough()),
        `/api/appointments/${id}/status`,
        { method: 'PATCH', body: JSON.stringify({ status }) }
      ),
  },
}

// ── Query key factory ────────────────────────────────────
export const qk = {
  users: () => ['users'] as const,
  branches: () => ['branches'] as const,
  serviceTypes: () => ['serviceTypes'] as const,
  slots: (params: object) => ['slots', params] as const,
  today: () => ['appointments', 'today'] as const,
  appointments: (params: object) => ['appointments', 'list', params] as const,
  appointment: (id: string) => ['appointments', id] as const,
}

// ── Helpers ──────────────────────────────────────────────
export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString('en-IE', { hour: '2-digit', minute: '2-digit' })
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-IE', { weekday: 'short', day: 'numeric', month: 'short' })
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString('en-IE', {
    day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit',
  })
}

export function toDateInputValue(d: Date): string {
  return d.toISOString().slice(0, 10)
}

export function nextStatusLabel(status: AppointmentStatus): string | null {
  if (status === 'Scheduled') return 'Start (In Progress)'
  if (status === 'InProgress') return 'Complete'
  return null
}

export function nextStatus(status: AppointmentStatus): AppointmentStatus | null {
  if (status === 'Scheduled') return 'InProgress'
  if (status === 'InProgress') return 'Completed'
  return null
}
