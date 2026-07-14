package com.nfasylum.babel.intellij.settings

import com.intellij.openapi.components.service
import com.nfasylum.babel.intellij.BabelPlugin
import com.nfasylum.babel.intellij.services.CoreBridge

/**
 * Resolves the list of natural languages offered in the picker. Prefers whatever
 * the running Core reports (authoritative, reflects the installed translations
 * repo) and falls back to [BabelPlugin.FALLBACK_LANGUAGES] when the Core is
 * unavailable.
 */
object LanguagePicker {
    fun availableLanguages(): List<String> {
        return try {
            val fromCore = service<CoreBridge>().getSupportedLanguages()
            if (fromCore.isNotEmpty()) fromCore else BabelPlugin.FALLBACK_LANGUAGES
        } catch (e: Exception) {
            BabelPlugin.FALLBACK_LANGUAGES
        }
    }
}
