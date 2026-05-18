import './Input.css'
import { type InputHTMLAttributes, type TextareaHTMLAttributes, forwardRef } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
  hint?: string
}

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string
  error?: string
  hint?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, hint, id, required, className = '', ...props }, ref) => {
    const inputId = id ?? label.toLowerCase().replace(/\s+/g, '-')
    return (
      <div className={`field ${className}`}>
        <label className="field__label" htmlFor={inputId}>
          {label}
          {required && <span className="field__required" aria-hidden="true"> *</span>}
        </label>
        <input
          ref={ref}
          id={inputId}
          className={`field__input ${error ? 'field__input--error' : ''}`}
          aria-invalid={!!error}
          aria-describedby={error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined}
          required={required}
          {...props}
        />
        {hint && !error && <p id={`${inputId}-hint`} className="field__hint">{hint}</p>}
        {error && <p id={`${inputId}-error`} className="field__error" role="alert">{error}</p>}
      </div>
    )
  }
)
Input.displayName = 'Input'

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ label, error, hint, id, required, className = '', ...props }, ref) => {
    const inputId = id ?? label.toLowerCase().replace(/\s+/g, '-')
    return (
      <div className={`field ${className}`}>
        <label className="field__label" htmlFor={inputId}>
          {label}
          {required && <span className="field__required" aria-hidden="true"> *</span>}
        </label>
        <textarea
          ref={ref}
          id={inputId}
          className={`field__input field__textarea ${error ? 'field__input--error' : ''}`}
          aria-invalid={!!error}
          aria-describedby={error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined}
          required={required}
          {...props}
        />
        {hint && !error && <p id={`${inputId}-hint`} className="field__hint">{hint}</p>}
        {error && <p id={`${inputId}-error`} className="field__error" role="alert">{error}</p>}
      </div>
    )
  }
)
Textarea.displayName = 'Textarea'
