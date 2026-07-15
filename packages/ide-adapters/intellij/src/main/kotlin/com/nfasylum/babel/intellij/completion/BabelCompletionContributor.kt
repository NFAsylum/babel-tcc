package com.nfasylum.babel.intellij.completion

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.openapi.components.service
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import com.nfasylum.babel.intellij.providers.BabelKeys
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * Suggests translated keywords (e.g. `se`, `enquanto`) while typing inside a
 * Babel translated view (B.4, parity with the VS Code `completionProvider`).
 * Only contributes inside a [BabelKeys.TRANSLATED_VIEW]; keywords come from the
 * cached translated->original map.
 *
 * The candidate list is [keywordCandidates], unit-tested.
 */
class BabelCompletionContributor : CompletionContributor() {
    init {
        extend(
            CompletionType.BASIC,
            PlatformPatterns.psiElement(),
            object : CompletionProvider<CompletionParameters>() {
                override fun addCompletions(
                    parameters: CompletionParameters,
                    context: ProcessingContext,
                    result: CompletionResultSet,
                ) {
                    val view = parameters.originalFile.virtualFile
                        ?.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return
                    val map = service<TranslationService>().keywordMap(view.extension, view.language)
                    keywordCandidates(map).forEach { keyword ->
                        result.addElement(LookupElementBuilder.create(keyword).withTypeText("Babel keyword"))
                    }
                }
            },
        )
    }

    companion object {
        /** Pure: the translated keywords to offer, one per map key, in stable order. */
        fun keywordCandidates(translatedToOriginal: Map<String, String>): List<String> =
            translatedToOriginal.keys.sorted()
    }
}
