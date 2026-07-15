package com.nfasylum.babel.intellij.highlighting

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

/** Tests the pure keyword-recognition used by the highlighting/hover annotator. */
class BabelAnnotatorTest {
    private val map = mapOf("se" to "if", "enquanto" to "while", "classe" to "class")

    @Test
    fun `translated keywords are recognised`() {
        assertTrue(BabelAnnotator.isTranslatedKeyword("se", map))
        assertTrue(BabelAnnotator.isTranslatedKeyword("classe", map))
    }

    @Test
    fun `identifiers and unknown tokens are not keywords`() {
        assertFalse(BabelAnnotator.isTranslatedKeyword("minhaVariavel", map))
        assertFalse(BabelAnnotator.isTranslatedKeyword("", map))
    }
}
