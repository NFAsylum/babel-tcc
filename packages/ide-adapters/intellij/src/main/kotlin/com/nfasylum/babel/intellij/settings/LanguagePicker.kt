package com.nfasylum.babel.intellij.settings

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.service
import com.nfasylum.babel.intellij.BabelPlugin
import com.nfasylum.babel.intellij.services.CoreBridge
import java.util.concurrent.atomic.AtomicReference

/**
 * Non-blocking language list. Returns the last cached Core result (or FALLBACK
 * if none yet) and kicks off a background refresh from Core.Host. Safe to call
 * from the EDT — never spawns/waits for the subprocess synchronously.
 */
object LanguagePicker {
    private val cached = AtomicReference<List<String>>()

    @Volatile
    private var refreshing = false

    fun availableLanguages(): List<String> {
        cached.get()?.let { return it }
        maybeRefresh()
        return BabelPlugin.FALLBACK_LANGUAGES
    }

    private fun maybeRefresh() {
        if (refreshing) return
        refreshing = true
        ApplicationManager.getApplication().executeOnPooledThread {
            try {
                val fromCore = service<CoreBridge>().getSupportedLanguages()
                if (fromCore.isNotEmpty()) {
                    cached.set(fromCore)
                }
            } catch (_: Exception) {
                // Leave cache empty; next call falls back and re-tries.
            } finally {
                refreshing = false
            }
        }
    }

    /** Test seam. */
    fun preloadForTests(languages: List<String>) { cached.set(languages) }
    fun resetForTests() { cached.set(null); refreshing = false }
}
