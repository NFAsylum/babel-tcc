package com.nfasylum.babel.intellij.services

import com.google.gson.Gson
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import java.util.concurrent.LinkedBlockingQueue
import java.util.concurrent.TimeUnit

/**
 * Tests the pure translation logic in [TranslationService] with a fake
 * [CoreBridge] (scripted transport), so no IDE and no .NET binary are needed.
 */
class TranslationServiceTest {
    private val gson = Gson()

    private class FakeTransport(private val responder: (String) -> String?) : CoreTransport {
        private val outbound = LinkedBlockingQueue<String>()

        @Volatile
        private var closed = false

        override fun writeLine(line: String) {
            responder(line)?.let { outbound.offer(it) }
        }

        override fun readLine(): String? {
            while (true) {
                if (closed && outbound.isEmpty()) return null
                outbound.poll(50, TimeUnit.MILLISECONDS)?.let { return it }
            }
        }

        override fun isAlive(): Boolean = !closed

        override fun close() {
            closed = true
        }
    }

    private fun serviceWith(responder: (String) -> String?): TranslationService {
        val bridge = CoreBridge().apply { transportFactory = { FakeTransport(responder) } }
        return TranslationService().apply { coreBridgeProvider = { bridge } }
    }

    @Test
    fun `isTranslatable accepts supported extensions case-insensitively`() {
        val ts = TranslationService()
        assertTrue(ts.isTranslatable("cs"))
        assertTrue(ts.isTranslatable("CS"))
        assertTrue(ts.isTranslatable("py"))
        assertFalse(ts.isTranslatable("txt"))
        assertFalse(ts.isTranslatable("md"))
    }

    @Test
    fun `dottedExtension normalises with a single leading dot`() {
        val ts = TranslationService()
        assertEquals(".cs", ts.dottedExtension("cs"))
        assertEquals(".cs", ts.dottedExtension(".cs"))
    }

    @Test
    fun `toDisplay returns the Core translation on success`() {
        val ts = serviceWith { request ->
            assertTrue("forward method used", request.contains("TranslateToNaturalLanguage"))
            gson.toJson(CoreResponse(success = true, result = "se (x) { }", error = ""))
        }

        val display = ts.toDisplay("if (x) { }", "cs", "pt-BR")

        assertEquals("se (x) { }", display)
    }

    @Test
    fun `toDisplay fails open to the original code when the Core errors`() {
        val ts = serviceWith { gson.toJson(CoreResponse(success = false, result = "", error = "boom")) }

        val display = ts.toDisplay("if (x) { }", "cs", "pt-BR")

        assertEquals("original is preserved on failure", "if (x) { }", display)
    }

    @Test
    fun `toDisk reverse-translates via ApplyTranslatedEdits`() {
        val ts = serviceWith { request ->
            assertTrue("reverse merge method used", request.contains("ApplyTranslatedEdits"))
            gson.toJson(CoreResponse(success = true, result = "if (y) { }", error = ""))
        }

        val disk = ts.toDisk(
            originalCode = "if (x) { }",
            previousTranslatedCode = "se (x) { }",
            editedTranslatedCode = "se (y) { }",
            extension = "cs",
            language = "pt-BR",
        )

        assertEquals("if (y) { }", disk)
    }

    @Test
    fun `toDisk keeps the original on disk when the Core fails`() {
        val ts = serviceWith { gson.toJson(CoreResponse(success = false, result = "", error = "boom")) }

        val disk = ts.toDisk("if (x) { }", "se (x) { }", "se (y) { }", "cs", "pt-BR")

        assertEquals("if (x) { }", disk)
    }
}
