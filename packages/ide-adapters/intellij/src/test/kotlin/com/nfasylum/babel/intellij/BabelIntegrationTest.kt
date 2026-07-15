package com.nfasylum.babel.intellij

import com.google.gson.Gson
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.service
import com.intellij.testFramework.LightVirtualFile
import com.intellij.testFramework.fixtures.BasePlatformTestCase
import com.nfasylum.babel.intellij.providers.BabelKeys
import com.nfasylum.babel.intellij.providers.TranslatedView
import com.nfasylum.babel.intellij.services.CoreBridge
import com.nfasylum.babel.intellij.services.CoreResponse
import com.nfasylum.babel.intellij.services.CoreTransport
import com.nfasylum.babel.intellij.services.LanguageService
import com.nfasylum.babel.intellij.services.TranslationService
import com.nfasylum.babel.intellij.settings.BabelSettings
import java.util.concurrent.LinkedBlockingQueue
import java.util.concurrent.TimeUnit

/**
 * Integration tests that boot the real (headless) IntelliJ platform. Unlike the
 * unit tests, these resolve the services exactly as declared in plugin.xml, so
 * they catch registration/wiring mistakes the fakes would hide. The Core.Host
 * subprocess is still faked — no .NET required.
 */
class BabelIntegrationTest : BasePlatformTestCase() {
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

    fun `test declared services resolve and are wired through plugin_xml`() {
        val bridge = ApplicationManager.getApplication().getService(CoreBridge::class.java)
        assertNotNull("CoreBridge application service must be registered", bridge)
        bridge.stop()
        bridge.transportFactory = {
            FakeTransport { gson.toJson(CoreResponse(success = true, result = "se (x) { }", error = "")) }
        }

        val translationService = service<TranslationService>()
        assertNotNull("TranslationService must be registered", translationService)

        val display = translationService.toDisplay("if (x) { }", "cs", "pt-BR")
        assertEquals("real service graph forward-translates", "se (x) { }", display)
    }

    fun `test settings sync flips language service to active`() {
        val settings = service<BabelSettings>()
        val languageService = service<LanguageService>()

        settings.enabled = true
        settings.language = "pt-BR"

        assertTrue("changing settings activates runtime translation", languageService.isTranslationActive())

        settings.language = BabelPlugin.LANGUAGE_NONE
        assertFalse("English passthrough deactivates translation", languageService.isTranslationActive())
    }

    fun `test status bar widget reflects the configured language, not off`() {
        val settings = service<BabelSettings>()
        settings.enabled = true
        settings.language = "pt-BR"
        val widget = com.nfasylum.babel.intellij.statusbar.BabelStatusBarWidget(project)
        try {
            assertEquals("Babel: pt-BR", widget.getText())
        } finally {
            widget.dispose()
            settings.language = "en"
        }
    }

    fun `test per-extension override drives effective language`() {
        val settings = service<BabelSettings>()
        val languageService = service<LanguageService>()
        settings.enabled = true
        settings.language = "pt-BR"
        settings.setLanguageOverride("cs", "es")
        try {
            assertEquals("es", languageService.effectiveLanguageFor("cs"))
            assertEquals("pt-BR", languageService.effectiveLanguageFor("py"))
            assertTrue(languageService.isTranslationActiveFor("cs"))
        } finally {
            settings.clearLanguageOverride("cs")
        }
    }

    fun `test translated view user data roundtrips on a real LightVirtualFile`() {
        val light = LightVirtualFile("A.cs", "se (x) { }")
        val view = TranslatedView("/proj/A.cs", "cs", "pt-BR", "if (x) { }", "se (x) { }")

        light.putUserData(BabelKeys.TRANSLATED_VIEW, view)

        assertEquals(view, light.getUserData(BabelKeys.TRANSLATED_VIEW))
        assertEquals("cs", light.extension)
    }
}
