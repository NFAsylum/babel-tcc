package com.nfasylum.babel.intellij.providers

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.application.ReadAction
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.fileEditor.FileDocumentManager
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.fileEditor.FileEditorManagerListener
import com.intellij.openapi.fileEditor.impl.LoadTextUtil
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.testFramework.LightVirtualFile
import com.nfasylum.babel.intellij.services.AutoTranslateManager
import com.nfasylum.babel.intellij.services.LanguageService
import com.nfasylum.babel.intellij.services.TranslationService
import com.nfasylum.babel.intellij.settings.BabelSettings

/**
 * The killer feature (DT-003): the `.cs`/`.py` file on disk stays original
 * English, while the editor shows a translated view.
 *
 * When a real, translatable file is opened and translation is active, we read
 * its source off the EDT, ask the Core for the translated rendering, then swap
 * the editor to a [LightVirtualFile] holding that translation. The swap and the
 * translated view are tagged with [BabelKeys.TRANSLATED_VIEW] so this listener
 * ignores its own light files (no recursion) and [SaveHandler] can map edits
 * back to disk.
 *
 * NOTE: the actual editor swap can only be exercised under `runIde` with a
 * running Core.Host; the translation decision logic lives in [TranslationService]
 * and is unit-tested there.
 */
class VirtualDocumentProvider : FileEditorManagerListener {
    private val log = Logger.getInstance(VirtualDocumentProvider::class.java)

    override fun fileOpened(source: FileEditorManager, file: VirtualFile) {
        // Ignore our own translated views to avoid re-entrancy.
        if (file.getUserData(BabelKeys.TRANSLATED_VIEW) != null) return
        if (file is LightVirtualFile) return

        val extension = file.extension ?: return
        val translationService = service<TranslationService>()
        if (!translationService.isTranslatable(extension)) return

        // Show original: one-shot request to open this file untranslated.
        if (service<AutoTranslateManager>().consumeShowOriginalFlag(file.path)) return

        // Honor per-extension language overrides and the enabled flag.
        val languageService = service<LanguageService>()
        if (!languageService.isTranslationActiveFor(extension)) return
        val language = languageService.effectiveLanguageFor(extension)

        val readonly = service<BabelSettings>().readonly

        // Translation may block (subprocess round-trip); keep it off the EDT.
        ApplicationManager.getApplication().executeOnPooledThread {
            val original = ReadAction.compute<String, RuntimeException> { LoadTextUtil.loadText(file).toString() }
            val translated = translationService.toDisplay(original, extension, language)

            // Nothing to show differently (translation off/failed/no-op): leave the original open.
            if (translated == original) return@executeOnPooledThread

            ApplicationManager.getApplication().invokeLater {
                if (!file.isValid) return@invokeLater
                val view = TranslatedView(file.path, extension, language, original, translated)
                val light = TranslatedLightFile(file.name, file.fileType, translated, view).apply {
                    isWritable = !readonly
                    putUserData(BabelKeys.TRANSLATED_VIEW, view)
                }
                log.info("Babel: showing ${if (readonly) "readonly" else "editable"} translated view of ${file.name} in $language")
                source.closeFile(file)
                source.openFile(light, true)
            }
        }
    }

    /**
     * Persists a translated view when its tab closes, so edits made after the last
     * save are not silently lost (the light-file document is not tracked as "unsaved",
     * so IntelliJ would not otherwise prompt or persist on close). Skips views with no
     * pending edit to avoid spurious disk writes.
     */
    override fun fileClosed(source: FileEditorManager, file: VirtualFile) {
        val translatedFile = file as? TranslatedLightFile ?: return
        val document = FileDocumentManager.getInstance().getDocument(translatedFile) ?: return
        if (document.text != translatedFile.view.shownTranslation) {
            translatedFile.persistReverseTranslation(document.text)
        }
    }
}
