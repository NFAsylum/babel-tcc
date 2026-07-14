package com.nfasylum.babel.intellij.services

import com.intellij.openapi.components.Service
import com.nfasylum.babel.intellij.BabelPlugin
import java.util.concurrent.CopyOnWriteArrayList

/**
 * Holds the currently active natural language and whether translation is enabled.
 *
 * This is the in-memory source of truth that providers read on every editor
 * event. It is deliberately independent of persistence: `BabelSettings`
 * (persistent state) pushes values in here on load and on change, so the hot
 * path never touches disk.
 */
@Service(Service.Level.APP)
class LanguageService {
    @Volatile
    var currentLanguage: String = BabelPlugin.LANGUAGE_NONE
        private set

    @Volatile
    var enabled: Boolean = true
        private set

    private val listeners = CopyOnWriteArrayList<() -> Unit>()

    /** True when files should actually be shown translated (enabled and not the passthrough "en"). */
    fun isTranslationActive(): Boolean = enabled && currentLanguage != BabelPlugin.LANGUAGE_NONE

    /** Updates the active language and notifies listeners if it changed. */
    fun setLanguage(language: String) {
        if (language == currentLanguage) return
        currentLanguage = language
        notifyChanged()
    }

    /** Enables or disables translation and notifies listeners if it changed. */
    fun setEnabled(value: Boolean) {
        if (value == enabled) return
        enabled = value
        notifyChanged()
    }

    /** Registers a callback fired whenever the active language or enabled flag changes. */
    fun addChangeListener(listener: () -> Unit) {
        listeners.add(listener)
    }

    private fun notifyChanged() {
        listeners.forEach { it() }
    }
}
