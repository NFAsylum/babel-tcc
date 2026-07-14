package com.nfasylum.babel.intellij.services

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.diagnostic.Logger

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

    companion object {
        /** Extensions the MVP intercepts. The Core also handles VisuAlg/Portugol; kept to the common set here. */
        val SUPPORTED_EXTENSIONS: Set<String> = setOf("cs", "py", "js")
    }
}
