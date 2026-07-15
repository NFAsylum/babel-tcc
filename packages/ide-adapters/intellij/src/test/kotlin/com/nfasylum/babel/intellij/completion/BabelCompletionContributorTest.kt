package com.nfasylum.babel.intellij.completion

import org.junit.Assert.assertEquals
import org.junit.Test

/** Tests the pure completion-candidate logic. */
class BabelCompletionContributorTest {
    @Test
    fun `one candidate per translated keyword, stably ordered`() {
        val map = mapOf("se" to "if", "enquanto" to "while", "classe" to "class")

        val candidates = BabelCompletionContributor.keywordCandidates(map)

        assertEquals(listOf("classe", "enquanto", "se"), candidates)
    }

    @Test
    fun `empty map yields no candidates`() {
        assertEquals(emptyList<String>(), BabelCompletionContributor.keywordCandidates(emptyMap()))
    }
}
