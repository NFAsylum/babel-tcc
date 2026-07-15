package com.nfasylum.babel.intellij.statusbar

import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.nfasylum.babel.intellij.statusbar.actions.ShowOriginalAction
import com.nfasylum.babel.intellij.statusbar.actions.ShowTranslatedAction
import com.nfasylum.babel.intellij.statusbar.actions.ToggleEnableAction
import com.nfasylum.babel.intellij.statusbar.actions.ToggleReadonlyAction
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
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

    private fun group(viewToggle: ViewToggle) = BabelStatusBarWidget.buildControlGroup(
        defaultLanguage = "pt-BR",
        languages = listOf("pt-BR", "es"),
        overrides = emptyMap(),
        extensions = listOf("cs", "py"),
        viewToggle = viewToggle,
        project = null,
    )

    @Test
    fun `control menu assembles toggles, language and per-extension overrides`() {
        val children = group(ViewToggle.SHOW_ORIGINAL).childActionsOrStubs
        // enable, readonly, sep, langGroup, sep, cs-group, py-group, sep, showOriginal
        assertEquals(9, children.size)
        assertTrue(children[0] is ToggleEnableAction)
        assertTrue(children[1] is ToggleReadonlyAction)

        val langGroup = children[3] as DefaultActionGroup
        assertEquals("one action per language", 2, langGroup.childActionsOrStubs.size)

        val csGroup = children[5] as DefaultActionGroup
        // "Use default" + one action per language
        assertEquals(3, csGroup.childActionsOrStubs.size)
    }

    @Test
    fun `view toggle is Show original when a translated view is focused`() {
        assertTrue(group(ViewToggle.SHOW_ORIGINAL).childActionsOrStubs.last() is ShowOriginalAction)
    }

    @Test
    fun `view toggle is Show translated when an original is focused`() {
        assertTrue(group(ViewToggle.SHOW_TRANSLATED).childActionsOrStubs.last() is ShowTranslatedAction)
    }

    @Test
    fun `no view toggle is shown when nothing relevant is focused`() {
        val children = group(ViewToggle.NONE).childActionsOrStubs
        assertEquals(7, children.size)
        assertFalse(children.last() is ShowOriginalAction)
        assertFalse(children.last() is ShowTranslatedAction)
    }
}
