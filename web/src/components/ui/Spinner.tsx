import './Spinner.css'

interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg'
  color?: 'navy' | 'yellow' | 'white' | 'inherit'
}

export function Spinner({ size = 'md', color = 'navy' }: SpinnerProps) {
  return (
    <span
      className={`spinner spinner--${size} spinner--${color}`}
      role="status"
      aria-label="Loading"
    />
  )
}
