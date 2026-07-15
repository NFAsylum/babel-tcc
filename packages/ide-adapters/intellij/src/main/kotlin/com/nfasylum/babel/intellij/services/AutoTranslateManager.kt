package com.nfasylum.babel.intellij.services

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.project.Project
import com.intellij.openapi.project.ProjectManager
import com.intellij.openapi.vfs.LocalFileSystem
import com.nfasylum.babel.intellij.providers.BabelKeys
import java.util.concurrent.ConcurrentHashMap

/** What to do with an open file when the active language changes. */
sealed class ReopenPlan {
    /** Close the current (translated) view and reopen the original disk file. */
    data class ReopenOriginal(val path: String) : ReopenPlan()

    /** Close and reopen the file itself so it gets translated (was a plain original). */
    object ReopenSelf : ReopenPlan()

    /** Leave the file alone. */
    object Skip : ReopenPlan()
}

/**
 * Re-translates open editors when the active language changes (B.5, parity with
 * the VS Code `autoTranslateManager`). On change it drops the keyword cache and,
 * on the EDT, closes and reopens each affected file so [com.nfasylum.babel.intellij.providers.VirtualDocumentProvider]
 * rebuilds the view in the new language.
 *
 * Application service subscribed once at startup (see BabelStartupActivity). The
 * per-file decision is [planFor], unit-tested.
 */
@Service(Service.Level.APP)
class AutoTranslateManager {
    private val log = Logger.getInstance(AutoTranslateManager::class.java)

    @Volatile
    private var subscribed = false

    /** Subscribes to language changes exactly once (idempotent across projects). */
    fun ensureSubscribed() {
        if (subscribed) return
        synchronized(this) {
            if (subscribed) return
            subscribed = true
            service<LanguageService>().addChangeListener { onLanguageChanged() }
        }
    }

    fun onLanguageChanged() {
        service<TranslationService>().invalidateKeywordCache()
        ApplicationManager.getApplication().invokeLater { reopenTranslatableFiles() }
    }

    private fun reopenTranslatableFiles() {
        val translationService = service<TranslationService>()
        ProjectManager.getInstance().openProjects.forEach { project ->
            if (project.isDisposed) return@forEach
            val editorManager = FileEditorManager.getInstance(project)
            editorManager.openFiles.toList().forEach { file ->
                val view = file.getUserData(BabelKeys.TRANSLATED_VIEW)
                val translatable = file.extension?.let { translationService.isTranslatable(it) } ?: false
                when (val plan = planFor(view != null, view?.originalPath, translatable)) {
                    is ReopenPlan.ReopenOriginal -> {
                        val original = LocalFileSystem.getInstance().findFileByPath(plan.path)
                        if (original != null) {
                            editorManager.closeFile(file)
                            editorManager.openFile(original, false)
                        } else {
                            log.warn("Babel: original vanished on language change: ${plan.path}")
                        }
                    }
                    is ReopenPlan.ReopenSelf -> {
                        editorManager.closeFile(file)
                        editorManager.openFile(file, false)
                    }
                    ReopenPlan.Skip -> Unit
                }
            }
        }
    }

    /** Paths queued to open untranslated exactly once (Show original). */
    private val showOriginalOnce = ConcurrentHashMap.newKeySet<String>()

    /**
     * Shows the original disk file for the currently selected translated view: closes
     * the view and reopens the original with a one-shot skip flag so
     * [com.nfasylum.babel.intellij.providers.VirtualDocumentProvider] leaves it untranslated.
     * The next open (e.g. after a language change) translates it again.
     */
    fun showOriginalForSelected(project: Project) {
        val editorManager = FileEditorManager.getInstance(project)
        val current = editorManager.selectedFiles.firstOrNull() ?: return
        val view = current.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return
        val original = LocalFileSystem.getInstance().findFileByPath(view.originalPath) ?: return
        showOriginalOnce.add(view.originalPath)
        ApplicationManager.getApplication().invokeLater {
            editorManager.closeFile(current)
            editorManager.openFile(original, true)
        }
    }

    /** True (once) if the file at [path] should be opened untranslated because of Show original. */
    fun consumeShowOriginalFlag(path: String): Boolean = showOriginalOnce.remove(path)

    companion object {
        /**
         * Pure decision: a translated view reopens its original; a plain translatable
         * file reopens itself; anything else is skipped.
         */
        fun planFor(hasView: Boolean, originalPath: String?, isTranslatableExtension: Boolean): ReopenPlan = when {
            hasView && originalPath != null -> ReopenPlan.ReopenOriginal(originalPath)
            !hasView && isTranslatableExtension -> ReopenPlan.ReopenSelf
            else -> ReopenPlan.Skip
        }
    }
}
