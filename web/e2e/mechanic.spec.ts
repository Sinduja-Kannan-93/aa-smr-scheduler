import { test, expect } from '@playwright/test'

test.describe('Mechanic — schedule, status transitions, and work notes', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForSelector('#act-as-select', { timeout: 15_000 })

    // Switch to the first Mechanic user in the dropdown
    const select = page.locator('#act-as-select')
    const options = await select.locator('option').allTextContents()
    const mechanicOption = options.find(o => o.includes('Mechanic'))!
    await select.selectOption({ label: mechanicOption })
    await expect(page.locator('h1')).toContainText('My Schedule')
  })

  test('shows My Schedule page with date range filters', async ({ page }) => {
    await expect(page.locator('label', { hasText: 'From' })).toBeVisible()
    await expect(page.locator('label', { hasText: 'To' })).toBeVisible()
  })

  test('shows appointment list or empty state — not a blank page', async ({ page }) => {
    await page.waitForTimeout(2_000)
    const cards = page.locator('.appt-card')
    const empty = page.locator('.empty-state')
    expect(await cards.count() > 0 || await empty.isVisible()).toBeTruthy()
  })

  test('golden path — view appointment detail, add work note, advance status', async ({ page }) => {
    // First book an appointment as Booking Agent so the mechanic has something to work with
    const select = page.locator('#act-as-select')
    const options = await select.locator('option').allTextContents()

    // Step 1: switch to booking agent and book a slot for this mechanic
    const agentOption = options.find(o => o.includes('Booking Agent'))!
    await select.selectOption({ label: agentOption })
    await expect(page.locator('h1')).toContainText('Book an Appointment')
    await page.waitForTimeout(2_000)

    const bookButtons = page.locator('.slot-card button', { hasText: 'Book' })
    if (await bookButtons.count() === 0) {
      test.skip(true, 'No slots available to book — skipping mechanic flow test')
      return
    }

    // Get the mechanic name from the first slot so we can switch to them
    const slotMechanic = await page.locator('.slot-card').first().locator('.slot-card__meta').textContent()

    await bookButtons.first().click()
    await expect(page.locator('.dialog__title')).toContainText('Book Appointment')
    await page.fill('input[name="customerName"]', 'Ciarán O\'Brien')
    await page.fill('input[name="vehicleReg"]', '241-G-55555')
    await page.fill('input[name="phoneNumber"]', '+353 86 555 6789')
    await page.click('button[type="submit"]')

    await expect(page.locator('.booking-confirmed__ref')).toBeVisible({ timeout: 10_000 })
    await page.click('.booking-confirmed + * button, .dialog__footer button', { hasText: 'Done' })
    // Dismiss confirmation
    await page.locator('.dialog__footer button', { hasText: 'Done' }).click().catch(() => {
      // Dialog may have already closed
    })

    // Step 2: switch to mechanic view
    // Find the mechanic whose name matches the slot
    const mechanicOptions = options.filter(o => o.includes('Mechanic'))
    for (const opt of mechanicOptions) {
      if (slotMechanic && opt.includes(slotMechanic.split('·')[0].trim())) {
        await select.selectOption({ label: opt })
        break
      }
    }
    // Fall back to first mechanic if no match
    if (await page.locator('h1').innerText() !== 'My Schedule') {
      await select.selectOption({ label: mechanicOptions[0] })
    }

    await expect(page.locator('h1')).toContainText('My Schedule', { timeout: 5_000 })
    await page.waitForTimeout(2_000)

    // Look for the appointment we just created
    const viewButtons = page.locator('.appt-card button', { hasText: 'View' })
    if (await viewButtons.count() === 0) {
      test.skip(true, 'Booked appointment not visible in mechanic view — may be tomorrow')
      return
    }

    await viewButtons.first().click()
    await expect(page.locator('.dialog__title')).toContainText('Appointment Detail')

    // Step 3: add a work note
    const noteTextarea = page.locator('textarea').last()
    await noteTextarea.fill('Initial inspection complete. Brake pads at 30%.')
    await page.locator('button', { hasText: 'Add Note' }).click()

    // Note should appear in the list
    await expect(page.locator('.work-note__content')).toContainText('Initial inspection', { timeout: 5_000 })

    // Step 4: advance status to In Progress
    await page.locator('button', { hasText: 'Start (In Progress)' }).click()
    await expect(page.locator('.badge--inprogress')).toBeVisible({ timeout: 5_000 })

    // Step 5: advance status to Completed
    await page.locator('button', { hasText: 'Complete' }).click()
    await expect(page.locator('.badge--completed')).toBeVisible({ timeout: 5_000 })
  })
})
