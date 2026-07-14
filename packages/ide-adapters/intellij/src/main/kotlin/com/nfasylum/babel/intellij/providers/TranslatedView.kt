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
 * @property originalContent the English source currently on disk; advanced to
 *   the freshly written source after each save so the next 3-way merge baseline
 *   is correct.
 * @property shownTranslation the translated text currently presented; advanced
 *   to the just-saved edits after each successful save.
 */
data class TranslatedView(
    val originalPath: String,
    val extension: String,
    val language: String,
    var originalContent: String,
    var shownTranslation: String,
)

/** Shared user-data keys for the Babel plugin. */
object BabelKeys {
    /** Present on a LightVirtualFile iff it is a Babel translated view. */
    val TRANSLATED_VIEW: Key<TranslatedView> = Key.create("babel.translatedView")
}
