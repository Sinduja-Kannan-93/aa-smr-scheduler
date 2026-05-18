import { test, expect } from '@playwright/test'

test.describe('Admin — today\'s schedule view', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
    await page.waitForSelector('#act-as-select', { timeout: 15_000 })

    // Ensure Admin user is selected (should be default)
    const select = page.locator('#act-as-select')
    const options = await select.locator('option').allTextContents()
    const adminOption = options.find(o => o.includes('Admin'))!
    await select.selectOption({ label: adminOption })
    await expect(page.locator('h1')).toContainText("Today's Schedule")
  })

  test('shows page title and today\'s date as subtitle', async ({ page }) => {
    await expect(page.locator('h1')).toHaveText("Today's Schedule")
    // Subtitle contains a day name (Mon–Sun)
    const subtitle = page.locator('.page-header__subtitle')
    await expect(subtitle).toBeVisible()
    const text = await subtitle.textContent()
    expect(text).toMatch(/Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday/)
  })

  test('stat cards row is always visible', async ({ page }) => {
    await page.waitForTimeout(1_500)
    await expect(page.locator('.admin-stats')).toBeVisible()
    const statCards = page.locator('.stat-card')
    await expect(statCards).toHaveCount(4)
  })

  test('stat card values are numeric', async ({ page }) => {
    await page.waitForTimeout(1_500)
    const values = await page.locator('.stat-card__value').allTextContents()
    values.forEach(v => {
      expect(Number(v.trim())).not.toBeNaN()
    })
  })

  test('shows mechanic cards or empty state — not a blank page', async ({ page }) => {
    await page.waitForTimeout(2_000)
    const mechanicCards = page.locator('.mechanic-card')
    const emptyState = page.locator('.empty-state')

    const hasCards = await mechanicCards.count() > 0
    const hasEmpty = await emptyState.isVisible()

    expect(hasCards || hasEmpty).toBeTruthy()
  })

  test('appointment rows inside mechanic cards show time, customer, and status badge', async ({ page }) => {
    await page.waitForTimeout(2_000)
    const apptRows = page.locator('.appt-row')

    if (await apptRows.count() === 0) {
      test.skip(true, 'No appointments today — seed data or timing issue')
      return
    }

    const firstRow = apptRows.first()
    await expect(firstRow.locator('.appt-row__time')).toBeVisible()
    await expect(firstRow.locator('.appt-row__customer')).toBeVisible()
    await expect(firstRow.locator('.badge')).toBeVisible()
  })
})
