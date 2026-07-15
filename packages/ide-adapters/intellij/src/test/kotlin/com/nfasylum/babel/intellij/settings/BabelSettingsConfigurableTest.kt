package com.nfasylum.babel.intellij.settings

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

/** Pure tests for the settings dialog's overrides summary. */
class BabelSettingsConfigurableTest {
    @Test
    fun `empty overrides show a manage hint`() {
        assertEquals("none (manage in the status bar menu)", BabelSettingsConfigurable.overridesText(emptyMap()))
    }

    @Test
    fun `overrides are summarised per extension`() {
        val text = BabelSettingsConfigurable.overridesText(linkedMapOf("cs" to "es", "py" to "pt-BR"))
        assertTrue(text.contains(".cs → es"))
        assertTrue(text.contains(".py → pt-BR"))
    }
}
