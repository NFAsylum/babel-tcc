package com.nfasylum.babel.intellij.statusbar

import org.junit.Assert.assertEquals
import org.junit.Test

/** Tests the pure status-bar label logic. */
class BabelStatusBarWidgetTest {
    @Test
    fun `active translation shows the language`() {
        assertEquals("Babel: pt-BR", BabelStatusBarWidget.widgetText(active = true, language = "pt-BR"))
    }

    @Test
    fun `inactive translation shows off`() {
        assertEquals("Babel: off", BabelStatusBarWidget.widgetText(active = false, language = "en"))
    }
}
