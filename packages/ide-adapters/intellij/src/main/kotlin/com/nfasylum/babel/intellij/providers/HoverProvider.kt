package com.nfasylum.babel.intellij.providers

import com.intellij.lang.documentation.DocumentationProvider
import com.intellij.openapi.components.service
import com.intellij.psi.PsiElement
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * Quick Documentation (Ctrl+Q) vector for the original keyword. Complements the
 * always-on mouse-hover tooltip produced by
 * [com.nfasylum.babel.intellij.highlighting.BabelAnnotator]. Only fires inside a
 * Babel [BabelKeys.TRANSLATED_VIEW]; the keyword is looked up in the cached
 * translated->original map.
 *
 * The lookup is [originalKeyword], unit-tested.
 */
class HoverProvider : DocumentationProvider {
    override fun getQuickNavigateInfo(element: PsiElement?, originalElement: PsiElement?): String? {
        val anchor = originalElement ?: element ?: return null
        val virtualFile = anchor.containingFile?.virtualFile ?: return null
        val view = virtualFile.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return null

        val keyword = anchor.text?.trim().orEmpty()
        if (keyword.isEmpty()) return null

        val map = service<TranslationService>().keywordMap(view.extension, view.language)
        val original = originalKeyword(keyword, map) ?: return null
        return "Babel — original keyword: <b>$original</b>"
    }

    companion object {
        /** Pure lookup of a translated keyword's original spelling. Null if not a keyword. */
        fun originalKeyword(keyword: String, translatedToOriginal: Map<String, String>): String? =
            translatedToOriginal[keyword]
    }
}
