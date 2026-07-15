package com.nfasylum.babel.intellij.services

import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.nfasylum.babel.intellij.BabelPlugin
import com.nfasylum.babel.intellij.settings.BabelSettings
import java.util.concurrent.CopyOnWriteArrayList

/**
 * Runtime API and change-notification hub for translation state. The state itself
 * lives in [BabelSettings] — the single source of truth — and this service reads
 * *through* to it instead of keeping its own copy, so the two can never drift
 * (that drift was the "Babel: off on startup" bug). [BabelSettings] calls
 * [fireChanged] on every mutation, so subscribers (status bar, auto-translate)
 * stay in sync.
 */
@Service(Service.Level.APP)
class LanguageService {
    private val log = Logger.getInstance(LanguageService::class.java)
    private val listeners = CopyOnWriteArrayList<() -> Unit>()

    /** Active default language, read through from [BabelSettings] (no cached copy). */
    val currentLanguage: String
        get() = service<BabelSettings>().language

    /** Whether translation is enabled, read through from [BabelSettings]. */
    val enabled: Boolean
        get() = service<BabelSettings>().enabled

    /** True when files should actually be shown translated (enabled and not the passthrough "en"). */
    fun isTranslationActive(): Boolean = enabled && currentLanguage != BabelPlugin.LANGUAGE_NONE

    /** Effective language for a specific file extension — honors per-language overrides. */
    fun effectiveLanguageFor(fileExtension: String): String =
        service<BabelSettings>().effectiveLanguage(fileExtension)

    /** True if translation is active for a given extension, considering per-extension overrides. */
    fun isTranslationActiveFor(fileExtension: String): Boolean {
        if (!enabled) return false
        return effectiveLanguageFor(fileExtension) != BabelPlugin.LANGUAGE_NONE
    }

    /** Registers a callback fired whenever translation state changes. */
    fun addChangeListener(listener: () -> Unit) {
        listeners.add(listener)
    }

    /** Removes a previously registered change listener (call on dispose to avoid leaks). */
    fun removeChangeListener(listener: () -> Unit) {
        listeners.remove(listener)
    }

    /** Notifies listeners that translation state changed. Called by [BabelSettings] on every mutation. */
    fun fireChanged() {
        listeners.forEach {
            try {
                it()
            } catch (e: Exception) {
                // One misbehaving listener must not stop the others from being notified.
                log.warn("Babel: language change listener threw: ${e.message}", e)
            }
        }
    }
}
