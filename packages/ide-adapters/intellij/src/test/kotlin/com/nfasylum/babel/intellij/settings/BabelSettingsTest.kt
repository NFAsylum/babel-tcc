package com.nfasylum.babel.intellij.settings

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test

/**
 * Persistence-layer tests for [BabelSettings]. The runtime sync is stubbed so
 * no platform services are needed; we verify state roundtrips and that every
 * mutation notifies the runtime.
 */
class BabelSettingsTest {
    private fun settings(): Pair<BabelSettings, MutableList<BabelSettings.State>> {
        val captured = mutableListOf<BabelSettings.State>()
        val s = BabelSettings().apply { runtimeSync = { captured.add(it.copy()) } }
        return s to captured
    }

    @Test
    fun `loadState roundtrips through getState and syncs runtime`() {
        val (s, captured) = settings()
        val loaded = BabelSettings.State(language = "pt-BR", enabled = false, coreHostPath = "/opt/core")

        s.loadState(loaded)

        assertEquals("pt-BR", s.getState().language)
        assertEquals(false, s.getState().enabled)
        assertEquals("/opt/core", s.getState().coreHostPath)
        assertEquals("runtime synced once on load", 1, captured.size)
        assertEquals("pt-BR", captured[0].language)
    }

    @Test
    fun `setting language notifies runtime`() {
        val (s, captured) = settings()

        s.language = "es"

        assertEquals("es", s.language)
        assertTrue(captured.any { it.language == "es" })
    }

    @Test
    fun `blank core host path is normalised to null`() {
        val (s, _) = settings()

        s.coreHostPath = "   "

        assertNull(s.coreHostPath)
    }

    @Test
    fun `defaults are English passthrough and enabled`() {
        val s = BabelSettings().apply { runtimeSync = {} }
        assertEquals("en", s.language)
        assertEquals(true, s.enabled)
        assertNull(s.coreHostPath)
        assertFalse(s.readonly)
        assertTrue(s.languageOverrides.isEmpty())
    }

    @Test
    fun `readonly persists and notifies`() {
        val (s, captured) = settings()
        s.readonly = true
        assertTrue(s.readonly)
        assertTrue(captured.any { it.readonly })
    }

    @Test
    fun `effectiveLanguage returns override when set, default otherwise`() {
        val s = BabelSettings().apply { runtimeSync = {}; language = "pt-BR" }
        assertEquals("pt-BR", s.effectiveLanguage(".cs"))
        s.setLanguageOverride("cs", "es")
        assertEquals("es", s.effectiveLanguage(".cs"))
        assertEquals("pt-BR", s.effectiveLanguage(".py"))
    }

    @Test
    fun `override equal to default clears it`() {
        val s = BabelSettings().apply { runtimeSync = {}; language = "pt-BR" }
        s.setLanguageOverride("cs", "es")
        assertTrue(s.languageOverrides.containsKey("cs"))
        s.setLanguageOverride("cs", "pt-BR")
        assertFalse(s.languageOverrides.containsKey("cs"))
    }

    @Test
    fun `clearLanguageOverride falls back to default`() {
        val s = BabelSettings().apply { runtimeSync = {}; language = "pt-BR" }
        s.setLanguageOverride("cs", "es")
        s.clearLanguageOverride("cs")
        assertFalse(s.languageOverrides.containsKey("cs"))
        assertEquals("pt-BR", s.effectiveLanguage(".cs"))
    }

    @Test
    fun `new fields roundtrip through loadState`() {
        val (s, _) = settings()
        s.loadState(
            BabelSettings.State(language = "pt-BR", readonly = true, languageOverrides = mutableMapOf("cs" to "es")),
        )
        assertTrue(s.readonly)
        assertEquals("es", s.effectiveLanguage(".cs"))
    }
}
