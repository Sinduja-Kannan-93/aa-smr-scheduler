import './Select.css'
import { type SelectHTMLAttributes, forwardRef } from 'react'

interface SelectOption {
  value: string | number
  label: string
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  options: SelectOption[]
  placeholder?: string
  error?: string
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, options, placeholder, error, id, required, className = '', ...props }, ref) => {
    const selectId = id ?? label.toLowerCase().replace(/\s+/g, '-')
    return (
      <div className={`field ${className}`}>
        <label className="field__label" htmlFor={selectId}>
          {label}
          {required && <span className="field__required" aria-hidden="true"> *</span>}
        </label>
        <div className="select-wrap">
          <select
            ref={ref}
            id={selectId}
            className={`field__input select-wrap__select ${error ? 'field__input--error' : ''}`}
            aria-invalid={!!error}
            aria-describedby={error ? `${selectId}-error` : undefined}
            required={required}
            {...props}
          >
            {placeholder && <option value="">{placeholder}</option>}
            {options.map(opt => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
          <svg className="select-wrap__arrow" width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
            <path d="M4 6l4 4 4-4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        </div>
        {error && <p id={`${selectId}-error`} className="field__error" role="alert">{error}</p>}
      </div>
    )
  }
)
Select.displayName = 'Select'
