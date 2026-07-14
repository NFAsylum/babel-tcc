package com.nfasylum.babel.intellij.providers

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

/** Tests the pure keyword lookup used by the hover tooltip. */
class HoverProviderTest {
    private val map = mapOf("se" to "if", "enquanto" to "while", "para" to "for")

    @Test
    fun `translated keyword resolves to its original`() {
        assertEquals("if", HoverProvider.originalKeyword("se", map))
        assertEquals("while", HoverProvider.originalKeyword("enquanto", map))
    }

    @Test
    fun `non-keyword returns null so no tooltip is shown`() {
        assertNull(HoverProvider.originalKeyword("minhaVariavel", map))
        assertNull(HoverProvider.originalKeyword("", map))
    }
}
