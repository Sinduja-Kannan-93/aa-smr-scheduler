# AA SMR Scheduler — Design System

## Style Direction

**Swiss / International** — precise grids, functional hierarchy, disciplined whitespace. The UI should feel like a professional tool: data-dense but never cluttered, branded but never loud.

## Palette

| Token | Value | Use |
|---|---|---|
| `--color-navy` | `#002D72` | Header, primary buttons, headings, active states |
| `--color-navy-hover` | `#1a4a96` | Navy hover state |
| `--color-yellow` | `#F5A800` | Accent CTAs (Book, Confirm), highlights |
| `--color-yellow-hover` | `#d48f00` | Yellow pressed state |
| `--color-bg` | `#F4F6F8` | Page background |
| `--color-surface` | `#FFFFFF` | Cards, dialogs, inputs |
| `--color-text-primary` | `#0D1B2A` | Body text, headings |
| `--color-text-secondary` | `#4A5568` | Supporting text |
| `--color-text-muted` | `#718096` | Labels, captions, placeholders |
| `--color-border` | `#D8E0EC` | Dividers, input borders |

## Status Colours

| Status | Colour | Background |
|---|---|---|
| Scheduled | `#2563EB` (blue) | `#EFF6FF` |
| In Progress | `#D97706` (amber) | `#FFFBEB` |
| Completed | `#16A34A` (green) | `#F0FDF4` |
| No Show | `#6B7280` (gray) | `#F9FAFB` |

## Typography

- **Font:** Inter (Google Fonts) — 400, 500, 600, 700
- **Scale:** 12 / 14 / 16 / 18 / 20 / 24 / 30px
- **Body line-height:** 1.5
- **Heading line-height:** 1.25
- **Heading tracking:** `-0.02em`
- **Badge / label tracking:** `+0.04em` uppercase

## Spacing

4px base unit: `4 / 8 / 12 / 16 / 20 / 24 / 32 / 40 / 48 / 64px`

## Radius

| Token | Value | Used on |
|---|---|---|
| `--radius-sm` | 4px | Tags, small badges |
| `--radius-md` | 6px | Buttons, inputs, selects |
| `--radius-lg` | 10px | Cards |
| `--radius-xl` | 14px | Dialogs |
| `--radius-full` | 9999px | Pill badges, spinner |

## Shadows

Four-level scale: `xs` (subtle input lift) → `sm` (card) → `md` (hover card) → `xl` (dialog/overlay). All shadows use `rgba(13, 27, 42, ...)` (navy-tinted) rather than neutral black.

## Motion

- **Fast:** 120ms — hover state changes, button presses
- **Normal:** 200ms — dialog entrance, badge transitions
- **Easing:** `cubic-bezier(0.16, 1, 0.3, 1)` (ease-out expo) for entrances; `cubic-bezier(0.4, 0, 0.2, 1)` for general

## Components

### Button
- Heights: 32 / 40 / 48px (sm / md / lg)
- Variants: `primary` (navy), `accent` (yellow), `ghost` (navy outline), `danger` (red)
- Loading: inline spinner + disabled, aria-busy

### Badge
- Pill shape (`border-radius: full`)
- Status-coloured backgrounds, not just text colour
- 11px uppercase with letter-spacing

### Card
- White surface, `--shadow-sm`, `--radius-lg`
- Padding variants: sm / md / lg
- CardHeader with title + subtitle + optional action slot

### Dialog
- Native `<dialog>` element (focus trap built-in)
- Backdrop: navy-tinted `rgba(13, 27, 42, 0.55)` + 2px blur
- Entrance: `translateY(8px) + scale(0.98)` → normal over 200ms

### Input / Textarea
- Labelled (never placeholder-only)
- Focus ring: 3px navy-tinted `rgba(0, 45, 114, 0.10)` + border-color change
- Error state: red border + red text below (not above)

## Anti-Patterns (Do Not)

- No default Tailwind/shadcn aesthetics
- No emoji as structural icons
- No dark mode
- No grey-on-grey without sufficient contrast
- No uniform padding/radius across all components
- No hover effects missing from interactive elements
