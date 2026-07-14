package com.nfasylum.babel.intellij.providers

import com.intellij.lang.documentation.DocumentationProvider
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.psi.PsiElement
import com.nfasylum.babel.intellij.services.CoreBridge
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * Shows the original (English) keyword when the user hovers a translated keyword
 * in a Babel view. Only fires inside a Babel [BabelKeys.TRANSLATED_VIEW]; the
 * keyword under the cursor is looked up in the Core's translated→original map.
 *
 * The lookup itself is [originalKeyword], unit-tested; the surrounding PSI/VFS
 * plumbing runs only inside a live IDE.
 */
class HoverProvider : DocumentationProvider {
    private val log = Logger.getInstance(HoverProvider::class.java)

    override fun getQuickNavigateInfo(element: PsiElement?, originalElement: PsiElement?): String? {
        val anchor = originalElement ?: element ?: return null
        val virtualFile = anchor.containingFile?.virtualFile ?: return null
        val view = virtualFile.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return null

        val keyword = anchor.text?.trim().orEmpty()
        if (keyword.isEmpty()) return null

        val original = try {
            val extension = service<TranslationService>().dottedExtension(view.extension)
            val map = service<CoreBridge>().getKeywordMap(extension, view.language)
            originalKeyword(keyword, map)
        } catch (e: Exception) {
            log.warn("Babel: keyword hover lookup failed: ${e.message}")
            null
        } ?: return null

        return "Babel — original keyword: <b>$original</b>"
    }

    companion object {
        /** Pure lookup of a translated keyword's original spelling. Null if not a keyword. */
        fun originalKeyword(keyword: String, translatedToOriginal: Map<String, String>): String? =
            translatedToOriginal[keyword]
    }
}
