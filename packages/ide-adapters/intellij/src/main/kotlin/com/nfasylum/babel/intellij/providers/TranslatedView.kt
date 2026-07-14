package com.nfasylum.babel.intellij.providers

import com.intellij.openapi.util.Key

/**
 * Metadata attached (via user data) to every Babel `LightVirtualFile` so the
 * save handler and the providers can recover what a translated view maps back
 * to on disk.
 *
 * @property originalPath absolute path of the real file on disk (always English).
 * @property extension file extension without a leading dot, e.g. "cs".
 * @property language natural-language code the view was rendered in.
 * @property originalContent the English source as read from disk when opened.
 * @property shownTranslation the translated text currently presented; updated
 *   after each successful save so the next 3-way merge has a correct baseline.
 */
data class TranslatedView(
    val originalPath: String,
    val extension: String,
    val language: String,
    val originalContent: String,
    var shownTranslation: String,
)

/** Shared user-data keys for the Babel plugin. */
object BabelKeys {
    /** Present on a LightVirtualFile iff it is a Babel translated view. */
    val TRANSLATED_VIEW: Key<TranslatedView> = Key.create("babel.translatedView")
}
