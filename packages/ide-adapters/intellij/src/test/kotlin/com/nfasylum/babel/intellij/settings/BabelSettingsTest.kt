package com.nfasylum.babel.intellij.settings

import org.junit.Assert.assertEquals
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
    }
}
