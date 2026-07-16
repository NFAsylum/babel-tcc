package com.nfasylum.babel.intellij.providers

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.fileTypes.FileType
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VfsUtil
import com.intellij.testFramework.LightVirtualFile
import com.nfasylum.babel.intellij.services.TranslationService

/**
 * A translated view backed by an in-memory file that — unlike a plain
 * [LightVirtualFile] — persists edits back to the original disk file.
 *
 * The plugin.xml `FileDocumentManagerListener` ([SaveHandler]) does not reliably
 * fire for light files, so saving the translated view was a no-op (Part D bug).
 * `setBinaryContent` is the hook `LoadTextUtil.write` invokes when a document is
 * saved; overriding it lets us reverse-translate the edited buffer and write the
 * original-language source to disk (DT-003), keeping the disk file compilable.
 * [SaveHandler] is kept as a fallback (double protection); a double-fire is
 * idempotent because the 3-way-merge baseline advances after the first write.
 */
class TranslatedLightFile(
    name: String,
    fileType: FileType,
    translatedText: String,
    val view: TranslatedView,
) : LightVirtualFile(name, fileType, translatedText) {
    private val log = Logger.getInstance(TranslatedLightFile::class.java)

    override fun setBinaryContent(content: ByteArray, newModificationStamp: Long, newTimeStamp: Long, requestor: Any?) {
        // setBinaryContent normally is not invoked for non-physical light files (the actual
        // Ctrl+S path is SaveHandler.beforeAllDocumentsSaving), but if the platform ever routes
        // here, persist too. If persist fails, toDisk fails-open to the original disk content —
        // the buffer diverges from disk until the next save, but disk is never corrupted.
        persistReverseTranslation(String(content, charset))
        super.setBinaryContent(content, newModificationStamp, newTimeStamp, requestor)
    }

    /**
     * Reverse-translates the edited translated buffer and writes it to the original
     * file on disk, advancing the merge baseline. Extracted so tests can drive it
     * directly. Fail-open: a missing original is logged, not fatal.
     */
    fun persistReverseTranslation(editedTranslated: String) {
        val diskContent = SaveHandler.reverseTranslate(view, editedTranslated, service<TranslationService>())
        val original = LocalFileSystem.getInstance().findFileByPath(view.originalPath)
        if (original == null) {
            log.warn("Babel: original file not found, cannot persist save: ${view.originalPath}")
            return
        }
        ApplicationManager.getApplication().runWriteAction {
            VfsUtil.saveText(original, diskContent)
        }
        log.info("Babel: persisted reverse-translated source to ${view.originalPath}")
    }
}
