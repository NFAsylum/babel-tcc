package com.nfasylum.babel.intellij.highlighting

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.openapi.components.service
import com.intellij.psi.PsiElement
import com.intellij.psi.impl.source.tree.LeafPsiElement
import com.nfasylum.babel.intellij.providers.BabelKeys
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * Two features in one pass over the tokens of a Babel translated view (B.1 + B.2):
 * paints each translated keyword with the theme keyword color, and attaches a
 * mouse-hover tooltip showing the original (English) keyword.
 *
 * The keyword map comes from [TranslationService.keywordMap], which caches per
 * language — annotate() runs per token, so it must never hit the subprocess
 * directly. The pure decision is [isTranslatedKeyword], unit-tested.
 */
class BabelAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        // Only leaf tokens; container elements would double-annotate the same range.
        if (element !is LeafPsiElement) return
        val view = element.containingFile?.virtualFile?.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return

        val text = element.text ?: return
        val map = service<TranslationService>().keywordMap(view.extension, view.language)
        if (!isTranslatedKeyword(text, map)) return

        val original = map[text]
        holder.newAnnotation(HighlightSeverity.INFORMATION, "Original: $original")
            // LeafPsiElement is both PsiElement and ASTNode; use its TextRange to disambiguate.
            .range(element.textRange)
            .tooltip("Original: <b>$original</b>")
            .textAttributes(BabelColors.KEYWORD)
            .create()
    }

    companion object {
        /** True when [text] is a translated keyword (present in the translated->original map). */
        fun isTranslatedKeyword(text: String, translatedToOriginal: Map<String, String>): Boolean =
            translatedToOriginal.containsKey(text)
    }
}
