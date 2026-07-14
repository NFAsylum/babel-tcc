package com.nfasylum.babel.intellij

/**
 * Central metadata for the Babel IntelliJ Platform plugin.
 *
 * IntelliJ plugins have no single imperative entry point the way a VS Code
 * extension has `activate()`; the lifecycle is wired declaratively through
 * plugin.xml. This object therefore holds the shared constants that the
 * services, providers and settings refer to, so the plugin id and the list of
 * translation targets live in exactly one place.
 */
object BabelPlugin {
    const val PLUGIN_ID: String = "com.nfasylum.babel"

    /** Language code that means "no translation" (show the original English source). */
    const val LANGUAGE_NONE: String = "en"

    /**
     * Natural-language codes shown in the picker as a static fallback.
     *
     * The authoritative list is whatever the C# Core reports from
     * `GetSupportedLanguages` (it scans the translations repo at runtime); this
     * constant only seeds the UI before the Core process has answered and keeps
     * the picker usable if the Core is unreachable.
     */
    val FALLBACK_LANGUAGES: List<String> = listOf(
        "en",
        "pt-BR",
        "es",
        "fr",
        "de",
        "it",
        "ja-romaji",
        "zh",
        "ar",
        "pt-ascii",
    )
}
