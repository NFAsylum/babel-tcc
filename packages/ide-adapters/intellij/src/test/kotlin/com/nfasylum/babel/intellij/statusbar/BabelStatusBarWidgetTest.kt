package com.nfasylum.babel.intellij.statusbar

import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.nfasylum.babel.intellij.statusbar.actions.ShowOriginalAction
import com.nfasylum.babel.intellij.statusbar.actions.ToggleEnableAction
import com.nfasylum.babel.intellij.statusbar.actions.ToggleReadonlyAction
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

/** Tests the pure status-bar label logic and control-menu assembly. */
class BabelStatusBarWidgetTest {
    @Test
    fun `active translation shows the language`() {
        assertEquals("Babel: pt-BR", BabelStatusBarWidget.widgetText(active = true, language = "pt-BR", readonly = false))
    }

    @Test
    fun `readonly active translation shows the ro suffix`() {
        assertEquals("Babel: pt-BR (ro)", BabelStatusBarWidget.widgetText(active = true, language = "pt-BR", readonly = true))
    }

    @Test
    fun `inactive translation shows off`() {
        assertEquals("Babel: off", BabelStatusBarWidget.widgetText(active = false, language = "en", readonly = false))
    }

    @Test
    fun `control menu assembles toggles, language, per-extension overrides and show original`() {
        val group = BabelStatusBarWidget.buildControlGroup(
            defaultLanguage = "pt-BR",
            languages = listOf("pt-BR", "es"),
            overrides = emptyMap(),
            extensions = listOf("cs", "py"),
            project = null,
        )

        val children = group.childActionsOrStubs
        // enable, readonly, sep, langGroup, sep, cs-group, py-group, sep, showOriginal
        assertEquals(9, children.size)
        assertTrue(children[0] is ToggleEnableAction)
        assertTrue(children[1] is ToggleReadonlyAction)
        assertTrue(children.last() is ShowOriginalAction)

        val langGroup = children[3] as DefaultActionGroup
        assertEquals("one action per language", 2, langGroup.childActionsOrStubs.size)

        val csGroup = children[5] as DefaultActionGroup
        // "Use default" + one action per language
        assertEquals(3, csGroup.childActionsOrStubs.size)
    }
}
