package com.nfasylum.babel.intellij.highlighting

import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey

/**
 * Text attributes for translated keywords. Derived from the theme's default
 * KEYWORD color, so translated keywords follow whatever color scheme the user
 * has — no hardcoded colors.
 */
object BabelColors {
    val KEYWORD: TextAttributesKey = TextAttributesKey.createTextAttributesKey(
        "BABEL_KEYWORD",
        DefaultLanguageHighlighterColors.KEYWORD,
    )
}
