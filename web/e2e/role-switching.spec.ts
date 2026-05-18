import { test, expect } from '@playwright/test'

test.describe('Act-As role switching', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    // Wait for the app to load users from the API
    await page.waitForSelector('#act-as-select', { timeout: 15_000 })
  })

  test('defaults to Admin view showing Today\'s Schedule', async ({ page }) => {
    // First user in seed data is Alice Admin
    await expect(page.locator('h1')).toContainText("Today's Schedule")
    await expect(page.locator('.act-as__role-badge')).toContainText('Admin')
  })

  test('switching to BookingAgent shows Book an Appointment', async ({ page }) => {
    const select = page.locator('#act-as-select')

    // Find and select the BookingAgent user
    const options = await select.locator('option').allTextContents()
    const bookingAgentOption = options.find(o => o.includes('Booking Agent'))
    expect(bookingAgentOption).toBeTruthy()

    await select.selectOption({ label: bookingAgentOption! })
    await expect(page.locator('h1')).toContainText('Book an Appointment')
    await expect(page.locator('.act-as__role-badge')).toContainText('Booking Agent')
  })

  test('switching to Mechanic shows My Schedule', async ({ page }) => {
    const select = page.locator('#act-as-select')

    const options = await select.locator('option').allTextContents()
    const mechanicOption = options.find(o => o.includes('Mechanic'))
    expect(mechanicOption).toBeTruthy()

    await select.selectOption({ label: mechanicOption! })
    await expect(page.locator('h1')).toContainText('My Schedule')
    await expect(page.locator('.act-as__role-badge')).toContainText('Mechanic')
  })

  test('switching roles resets the view — no stale content from previous role', async ({ page }) => {
    const select = page.locator('#act-as-select')
    const options = await select.locator('option').allTextContents()

    // Go to booking agent
    const agentOption = options.find(o => o.includes('Booking Agent'))!
    await select.selectOption({ label: agentOption })
    await expect(page.locator('h1')).toContainText('Book an Appointment')

    // Switch to Admin — should not show booking content
    const adminOption = options.find(o => o.includes('Admin'))!
    await select.selectOption({ label: adminOption })
    await expect(page.locator('h1')).toContainText("Today's Schedule")
    await expect(page.locator('h1')).not.toContainText('Book an Appointment')
  })
})
