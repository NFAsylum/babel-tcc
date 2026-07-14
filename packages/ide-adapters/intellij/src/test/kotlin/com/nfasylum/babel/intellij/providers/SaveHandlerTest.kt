package com.nfasylum.babel.intellij.providers

import com.google.gson.Gson
import com.nfasylum.babel.intellij.services.CoreBridge
import com.nfasylum.babel.intellij.services.CoreResponse
import com.nfasylum.babel.intellij.services.CoreTransport
import com.nfasylum.babel.intellij.services.TranslationService
import org.junit.Assert.assertEquals
import org.junit.Test
import java.util.concurrent.LinkedBlockingQueue
import java.util.concurrent.TimeUnit

/**
 * Tests the pure reverse-translation + baseline logic of [SaveHandler] with a
 * scripted Core, no IDE required.
 */
class SaveHandlerTest {
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

    private fun translationServiceReturning(diskResult: String): TranslationService {
        val bridge = CoreBridge().apply {
            transportFactory = { FakeTransport { gson.toJson(CoreResponse(success = true, result = diskResult, error = "")) } }
        }
        return TranslationService().apply { coreBridgeProvider = { bridge } }
    }

    @Test
    fun `reverseTranslate writes original-language source to disk`() {
        // User typed the Portuguese keyword "se"; the Core reverses it to "if".
        val ts = translationServiceReturning("if (x) { }")
        val view = TranslatedView(
            originalPath = "/proj/A.cs",
            extension = "cs",
            language = "pt-BR",
            originalContent = "if (y) { }",
            shownTranslation = "se (y) { }",
        )

        val disk = SaveHandler.reverseTranslate(view, editedTranslated = "se (x) { }", translationService = ts)

        assertEquals("if (x) { }", disk)
    }

    @Test
    fun `reverseTranslate advances the 3-way merge baseline`() {
        val ts = translationServiceReturning("if (x) { }")
        val view = TranslatedView(
            originalPath = "/proj/A.cs",
            extension = "cs",
            language = "pt-BR",
            originalContent = "if (y) { }",
            shownTranslation = "se (y) { }",
        )

        SaveHandler.reverseTranslate(view, editedTranslated = "se (x) { }", translationService = ts)

        assertEquals("disk baseline advances to the written source", "if (x) { }", view.originalContent)
        assertEquals("shown baseline advances to the saved edits", "se (x) { }", view.shownTranslation)
    }
}
