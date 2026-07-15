package com.nfasylum.babel.intellij.services

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.diagnostic.Logger
import java.util.concurrent.ConcurrentHashMap

/**
 * Translation logic shared by the editor providers, kept free of platform
 * editor/VFS types so it can be unit-tested without booting the IDE.
 *
 * Every call is fail-open: if the Core engine is unreachable or errors, the
 * original English source is returned unchanged and the failure is logged. A
 * missing Core.Host must degrade to "show the real code", never crash the IDE.
 */
@Service(Service.Level.APP)
class TranslationService {
    private val log = Logger.getInstance(TranslationService::class.java)

    /** Seam for tests: resolves the [CoreBridge]; defaults to the application service. */
    var coreBridgeProvider: () -> CoreBridge = {
        ApplicationManager.getApplication().getService(CoreBridge::class.java)
    }

    /** True if a file with this extension (no leading dot) is one Babel translates. */
    fun isTranslatable(extension: String): Boolean =
        extension.lowercase() in SUPPORTED_EXTENSIONS

    /** The Core expects a leading-dot extension (".cs"); normalise whatever the VFS gives us. */
    fun dottedExtension(extension: String): String =
        if (extension.startsWith(".")) extension else ".$extension"

    /**
     * Forward-translates disk source into the display language. Returns the
     * original code untouched when translation is off or the Core fails.
     */
    fun toDisplay(originalCode: String, extension: String, language: String): String {
        return try {
            coreBridgeProvider().translateToNaturalLanguage(originalCode, dottedExtension(extension), language)
        } catch (e: Exception) {
            log.warn("TranslationService: forward translate failed, showing original: ${e.message}")
            originalCode
        }
    }

    /**
     * Reverse-translates the edited translated view back to disk source using a
     * 3-way merge against the original English and the previously shown
     * translation (Core `ApplyTranslatedEdits`). Falls back to the on-disk
     * original if the Core fails, so a broken engine can never corrupt the file.
     */
    fun toDisk(
        originalCode: String,
        previousTranslatedCode: String,
        editedTranslatedCode: String,
        extension: String,
        language: String,
    ): String {
        return try {
            coreBridgeProvider().applyTranslatedEdits(
                originalCode,
                previousTranslatedCode,
                editedTranslatedCode,
                dottedExtension(extension),
                language,
            )
        } catch (e: Exception) {
            log.warn("TranslationService: reverse translate failed, keeping original on disk: ${e.message}")
            originalCode
        }
    }

    private val keywordMapCache = ConcurrentHashMap<String, Map<String, String>>()

    /**
     * Cached translated-keyword -> original-keyword map for an (extension, language)
     * pair. The map is static per language, so it is fetched from the Core once and
     * reused — critical because the annotator queries it per token. A failed or empty
     * fetch is not cached, so a transient Core outage retries on the next call.
     */
    fun keywordMap(extension: String, language: String): Map<String, String> {
        val key = "${dottedExtension(extension)}::$language"
        keywordMapCache[key]?.let { return it }
        val map = try {
            coreBridgeProvider().getKeywordMap(dottedExtension(extension), language)
        } catch (e: Exception) {
            log.warn("TranslationService: keyword map fetch failed: ${e.message}")
            emptyMap()
        }
        if (map.isNotEmpty()) {
            keywordMapCache[key] = map
        }
        return map
    }

    /** Drops the cached keyword maps (call when the translations source may have changed). */
    fun invalidateKeywordCache() {
        keywordMapCache.clear()
    }

    /**
     * File extensions Babel can translate, e.g. ["cs", "js", "py"]. Kept in sync with
     * [isTranslatable] via the single [SUPPORTED_EXTENSIONS] source of truth so the
     * per-extension override menu only offers extensions that actually translate.
     */
    fun supportedExtensions(): List<String> = SUPPORTED_EXTENSIONS.sorted()

    companion object {
        /**
         * Extensions Babel intercepts, matching the adapters the Core.Host registers:
         * C# (.cs), Python (.py), JavaScript (.js), VisuAlg (.alg) and Portugol Studio (.por).
         * VisuAlg and Portugol are the main teaching languages in Brazil — the primary audience.
         */
        val SUPPORTED_EXTENSIONS: Set<String> = setOf("cs", "py", "js", "alg", "por")
    }
}
