package com.nfasylum.babel.intellij.services

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

/** Tests the pure reopen-decision used when the active language changes. */
class AutoTranslateManagerTest {
    @Test
    fun `translated view reopens its original file`() {
        val plan = AutoTranslateManager.planFor(hasView = true, originalPath = "/proj/A.cs", isTranslatableExtension = true)
        assertEquals(ReopenPlan.ReopenOriginal("/proj/A.cs"), plan)
    }

    @Test
    fun `plain translatable original reopens itself to get translated`() {
        val plan = AutoTranslateManager.planFor(hasView = false, originalPath = null, isTranslatableExtension = true)
        assertTrue(plan is ReopenPlan.ReopenSelf)
    }

    @Test
    fun `non-translatable untouched file is skipped`() {
        val plan = AutoTranslateManager.planFor(hasView = false, originalPath = null, isTranslatableExtension = false)
        assertTrue(plan is ReopenPlan.Skip)
    }
}
