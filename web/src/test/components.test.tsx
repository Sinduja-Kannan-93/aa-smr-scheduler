import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from '../components/ui/Button'
import { Badge, StatusBadge } from '../components/ui/Badge'
import { EmptyState } from '../components/ui/EmptyState'
import { Input } from '../components/ui/Input'
import { Spinner } from '../components/ui/Spinner'

describe('Button', () => {
  it('renders children', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByText('Click me')).toBeTruthy()
  })

  it('calls onClick handler', () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Go</Button>)
    fireEvent.click(screen.getByRole('button'))
    expect(onClick).toHaveBeenCalledOnce()
  })

  it('is disabled when loading', () => {
    render(<Button loading>Save</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Save</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('sets aria-busy when loading', () => {
    render(<Button loading>Save</Button>)
    expect(screen.getByRole('button')).toHaveAttribute('aria-busy', 'true')
  })

  it('applies variant class', () => {
    render(<Button variant="accent">Accent</Button>)
    expect(screen.getByRole('button').className).toContain('btn--accent')
  })
})

describe('Badge', () => {
  it('renders text', () => {
    render(<Badge>Active</Badge>)
    expect(screen.getByText('Active')).toBeTruthy()
  })

  it('applies variant class', () => {
    const { container } = render(<Badge variant="navy">Navy</Badge>)
    expect(container.firstChild).toHaveClass('badge--navy')
  })
})

describe('StatusBadge', () => {
  it('renders Scheduled label', () => {
    render(<StatusBadge status="Scheduled" />)
    expect(screen.getByText('Scheduled')).toBeTruthy()
  })

  it('renders InProgress as "In Progress"', () => {
    render(<StatusBadge status="InProgress" />)
    expect(screen.getByText('In Progress')).toBeTruthy()
  })

  it('renders NoShow as "No Show"', () => {
    render(<StatusBadge status="NoShow" />)
    expect(screen.getByText('No Show')).toBeTruthy()
  })

  it('applies inprogress class for InProgress status', () => {
    const { container } = render(<StatusBadge status="InProgress" />)
    expect(container.firstChild).toHaveClass('badge--inprogress')
  })
})

describe('EmptyState', () => {
  it('renders title', () => {
    render(<EmptyState title="Nothing here" />)
    expect(screen.getByText('Nothing here')).toBeTruthy()
  })

  it('renders description when provided', () => {
    render(<EmptyState title="Empty" description="Try again later" />)
    expect(screen.getByText('Try again later')).toBeTruthy()
  })

  it('renders action when provided', () => {
    render(<EmptyState title="Empty" action={<button>Retry</button>} />)
    expect(screen.getByText('Retry')).toBeTruthy()
  })
})

describe('Input', () => {
  it('renders with label', () => {
    render(<Input label="Email" />)
    expect(screen.getByLabelText('Email')).toBeTruthy()
  })

  it('shows required marker when required', () => {
    render(<Input label="Name" required />)
    expect(screen.getByText('*', { exact: false })).toBeTruthy()
  })

  it('shows error message', () => {
    render(<Input label="Email" error="Invalid email" />)
    expect(screen.getByText('Invalid email')).toBeTruthy()
  })

  it('marks input as aria-invalid on error', () => {
    render(<Input label="Email" error="Required" />)
    expect(screen.getByLabelText('Email')).toHaveAttribute('aria-invalid', 'true')
  })

  it('accepts user input', () => {
    render(<Input label="City" />)
    const input = screen.getByLabelText('City') as HTMLInputElement
    fireEvent.change(input, { target: { value: 'Dublin' } })
    expect(input.value).toBe('Dublin')
  })
})

describe('Spinner', () => {
  it('renders with status role', () => {
    render(<Spinner />)
    expect(screen.getByRole('status')).toBeTruthy()
  })

  it('has aria-label', () => {
    render(<Spinner />)
    expect(screen.getByLabelText('Loading')).toBeTruthy()
  })
})
