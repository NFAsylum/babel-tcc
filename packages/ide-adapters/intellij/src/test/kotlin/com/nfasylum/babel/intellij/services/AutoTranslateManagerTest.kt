package com.nfasylum.babel.intellij.services

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

/** Tests the pure reopen-decision used when the active language changes. */
class AutoTranslateManagerTest {
    @Test
    fun `translated view reopens its original when its language changed`() {
        val plan = AutoTranslateManager.planFor(
            currentViewLanguage = "pt-BR",
            originalPath = "/proj/A.cs",
            isTranslatableExtension = true,
            newEffectiveLanguage = "es",
            translationActive = true,
        )
        assertEquals(ReopenPlan.ReopenOriginal("/proj/A.cs"), plan)
    }

    @Test
    fun `translated view is skipped when its effective language did not change`() {
        val plan = AutoTranslateManager.planFor(
            currentViewLanguage = "es",
            originalPath = "/proj/A.cs",
            isTranslatableExtension = true,
            newEffectiveLanguage = "es",
            translationActive = true,
        )
        assertTrue("override-pinned view should not flicker", plan is ReopenPlan.Skip)
    }

    @Test
    fun `translated view reopens original when translation turned off`() {
        val plan = AutoTranslateManager.planFor(
            currentViewLanguage = "pt-BR",
            originalPath = "/proj/A.cs",
            isTranslatableExtension = true,
            newEffectiveLanguage = "en",
            translationActive = false,
        )
        assertEquals(ReopenPlan.ReopenOriginal("/proj/A.cs"), plan)
    }

    @Test
    fun `plain translatable original reopens itself to get translated`() {
        val plan = AutoTranslateManager.planFor(
            currentViewLanguage = null,
            originalPath = null,
            isTranslatableExtension = true,
            newEffectiveLanguage = "pt-BR",
            translationActive = true,
        )
        assertTrue(plan is ReopenPlan.ReopenSelf)
    }

    @Test
    fun `non-translatable file is skipped`() {
        val plan = AutoTranslateManager.planFor(
            currentViewLanguage = null,
            originalPath = null,
            isTranslatableExtension = false,
            newEffectiveLanguage = "en",
            translationActive = false,
        )
        assertTrue(plan is ReopenPlan.Skip)
    }
}
