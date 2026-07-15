package com.nfasylum.babel.intellij.providers

import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.editor.Document
import com.intellij.openapi.fileEditor.FileDocumentManager
import com.intellij.openapi.fileEditor.FileDocumentManagerListener
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.project.ProjectManager
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * Persists edits to a Babel translated view back to the original disk file,
 * reverse-translated to the original language (DT-003).
 *
 * The translated view is a non-physical [TranslatedLightFile]; editing it does
 * not even mark the document unsaved, so the per-document save path never fires
 * for it (that was the Part D save bug). The reliable trigger is
 * [beforeAllDocumentsSaving], which the platform calls on Ctrl+S / Save All and on
 * autosave (frame deactivation) regardless of whether individual files are
 * savable — there we persist every open translated view.
 *
 * The pure reverse+baseline logic is [reverseTranslate] (unit-tested); the disk
 * write lives in [TranslatedLightFile.persistReverseTranslation].
 */
class SaveHandler : FileDocumentManagerListener {
    private val log = Logger.getInstance(SaveHandler::class.java)

    override fun beforeAllDocumentsSaving() {
        val fileDocumentManager = FileDocumentManager.getInstance()
        for (project in ProjectManager.getInstance().openProjects) {
            if (project.isDisposed) continue
            for (file in FileEditorManager.getInstance(project).openFiles) {
                val translatedFile = file as? TranslatedLightFile ?: continue
                val document = fileDocumentManager.getDocument(translatedFile) ?: continue
                translatedFile.persistReverseTranslation(document.text)
            }
        }
    }

    override fun beforeDocumentSaving(document: Document) {
        // Fallback for physical files that reach the per-document path; light views are
        // handled by beforeAllDocumentsSaving above.
        val savedFile = FileDocumentManager.getInstance().getFile(document) as? TranslatedLightFile ?: return
        savedFile.persistReverseTranslation(document.text)
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
