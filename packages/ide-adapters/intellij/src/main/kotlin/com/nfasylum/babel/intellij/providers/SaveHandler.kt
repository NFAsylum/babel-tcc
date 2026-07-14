package com.nfasylum.babel.intellij.providers

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.editor.Document
import com.intellij.openapi.fileEditor.FileDocumentManager
import com.intellij.openapi.fileEditor.FileDocumentManagerListener
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VfsUtil
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * Reverse translation on save (Commit 2.3). When the user saves a Babel
 * translated view, the edits — a mix of untouched English from disk and new
 * text typed in the display language — are reverse-translated back to pure
 * original source, which is what gets written to disk. The file on disk is
 * therefore always in the original language (DT-003).
 *
 * The reverse merge itself is [TranslationService.toDisk] (Core
 * `ApplyTranslatedEdits`), which fails open to the on-disk original so a broken
 * engine can never corrupt the file.
 *
 * NOTE: the listener wiring runs only inside a live IDE; the pure reverse+baseline
 * logic is [reverseTranslate], unit-tested in SaveHandlerTest.
 */
class SaveHandler : FileDocumentManagerListener {
    private val log = Logger.getInstance(SaveHandler::class.java)

    override fun beforeDocumentSaving(document: Document) {
        val savedFile = FileDocumentManager.getInstance().getFile(document) ?: return
        val view = savedFile.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return

        val translationService = service<TranslationService>()
        val diskContent = reverseTranslate(view, document.text, translationService)

        val originalFile = LocalFileSystem.getInstance().findFileByPath(view.originalPath)
        if (originalFile == null) {
            log.warn("Babel: original file vanished, cannot write back: ${view.originalPath}")
            return
        }

        ApplicationManager.getApplication().runWriteAction {
            VfsUtil.saveText(originalFile, diskContent)
        }
        log.info("Babel: wrote reverse-translated source to ${view.originalPath}")
    }

    companion object {
        /**
         * Reverse-translates [editedTranslated] back to disk source and advances
         * the view's 3-way-merge baseline. Pure apart from the Core round-trip in
         * [translationService], so it is unit-testable. Returns the content to
         * write to disk.
         */
        fun reverseTranslate(
            view: TranslatedView,
            editedTranslated: String,
            translationService: TranslationService,
        ): String {
            val diskContent = translationService.toDisk(
                originalCode = view.originalContent,
                previousTranslatedCode = view.shownTranslation,
                editedTranslatedCode = editedTranslated,
                extension = view.extension,
                language = view.language,
            )
            // Advance the baseline: disk is now diskContent, and the shown view is the just-saved edits.
            view.originalContent = diskContent
            view.shownTranslation = editedTranslated
            return diskContent
        }
    }
}
