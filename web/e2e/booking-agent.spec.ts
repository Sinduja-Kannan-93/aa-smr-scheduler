import { test, expect } from '@playwright/test'

test.describe('Booking Agent — slot browser and appointment booking', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForSelector('#act-as-select', { timeout: 15_000 })

    // Switch to Booking Agent
    const select = page.locator('#act-as-select')
    const options = await select.locator('option').allTextContents()
    const agentOption = options.find(o => o.includes('Booking Agent'))!
    await select.selectOption({ label: agentOption })
    await expect(page.locator('h1')).toContainText('Book an Appointment')
  })

  test('filter bar is visible with date, service type, and branch inputs', async ({ page }) => {
    await expect(page.locator('label', { hasText: 'From' })).toBeVisible()
    await expect(page.locator('label', { hasText: 'To' })).toBeVisible()
    await expect(page.locator('label', { hasText: 'Service type' })).toBeVisible()
    await expect(page.locator('label', { hasText: 'Branch' })).toBeVisible()
  })

  test('available slots are displayed in date-grouped grid', async ({ page }) => {
    // Wait for slots to load (API call)
    await page.waitForTimeout(1_500)

    const slotCards = page.locator('.slot-card')
    const count = await slotCards.count()

    if (count === 0) {
      // EmptyState is acceptable if no slots are in range
      await expect(page.locator('.empty-state')).toBeVisible()
    } else {
      await expect(slotCards.first()).toBeVisible()
      // Each slot card has a time and a Book button
      await expect(slotCards.first().locator('.slot-card__time')).toBeVisible()
      await expect(slotCards.first().locator('button', { hasText: 'Book' })).toBeVisible()
    }
  })

  test('golden path — book a slot and receive reference number', async ({ page }) => {
    // Wait for slot data
    await page.waitForTimeout(2_000)

    const bookButtons = page.locator('.slot-card button', { hasText: 'Book' })
    const count = await bookButtons.count()

    if (count === 0) {
      test.skip(true, 'No available slots in range — seed data may be fully booked')
      return
    }

    // Open the booking dialog
    await bookButtons.first().click()
    await expect(page.locator('.dialog__title')).toContainText('Book Appointment')
    await expect(page.locator('.slot-summary')).toBeVisible()

    // Fill in customer details
    await page.fill('input[name="customerName"]', 'Aoife Murphy')
    await page.fill('input[name="vehicleReg"]', '241-D-99999')
    await page.fill('input[name="phoneNumber"]', '+353 87 123 4567')
    await page.fill('textarea[name="notes"]', 'First service since purchase')

    // Submit booking
    await page.click('button[type="submit"]')

    // Confirmation dialog should appear with a reference number
    await expect(page.locator('.dialog__title')).toContainText('Booking Confirmed', { timeout: 10_000 })
    const refEl = page.locator('.booking-confirmed__ref')
    await expect(refEl).toBeVisible()

    const refText = await refEl.textContent()
    expect(refText).toMatch(/^SMR-\d{4}-[A-Z0-9]{6}$/)
  })

  test('closing the confirmation dialog dismisses it cleanly', async ({ page }) => {
    await page.waitForTimeout(2_000)

    const bookButtons = page.locator('.slot-card button', { hasText: 'Book' })
    if (await bookButtons.count() === 0) {
      test.skip(true, 'No available slots')
      return
    }

    await bookButtons.first().click()
    // Cancel the booking dialog
    await page.click('button', { hasText: 'Cancel' })
    await expect(page.locator('.dialog__title')).not.toBeVisible()
  })
})
