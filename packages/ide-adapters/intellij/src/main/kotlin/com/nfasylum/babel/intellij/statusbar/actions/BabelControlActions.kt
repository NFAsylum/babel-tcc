package com.nfasylum.babel.intellij.statusbar.actions

import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.ToggleAction
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.nfasylum.babel.intellij.services.AutoTranslateManager
import com.nfasylum.babel.intellij.settings.BabelSettings

/** Toggles Babel translation on/off. Reopens affected editors via the language-change path. */
class ToggleEnableAction : ToggleAction("Enabled") {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.EDT

    override fun isSelected(e: AnActionEvent): Boolean = service<BabelSettings>().enabled

    override fun setSelected(e: AnActionEvent, state: Boolean) {
        // Setting enabled flows through LanguageService, which triggers re-open + widget refresh.
        service<BabelSettings>().enabled = state
    }
}

/** Toggles whether translated views open read-only. Reopens views so the change takes effect. */
class ToggleReadonlyAction : ToggleAction("Read-only view") {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.EDT

    override fun isSelected(e: AnActionEvent): Boolean = service<BabelSettings>().readonly

    override fun setSelected(e: AnActionEvent, state: Boolean) {
        // BabelSettings notifies LanguageService on change, which reopens views + refreshes the widget.
        service<BabelSettings>().readonly = state
    }
}

/** Sets the default active language. */
class SelectLanguageMenuAction(private val language: String) : AnAction(language) {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        service<BabelSettings>().language = language
    }
}

/** Sets a per-extension language override (e.g. `.cs` in Spanish while the default is pt-BR). */
class SetOverrideAction(private val extension: String, private val language: String) : AnAction(language) {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        service<BabelSettings>().setLanguageOverride(extension, language)
    }
}

/** Clears a per-extension override, falling back to the default language. */
class ClearOverrideAction(private val extension: String) : AnAction("Use default") {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        service<BabelSettings>().clearLanguageOverride(extension)
    }
}

/** Opens the original disk file for the selected translated view (untranslated, one-shot). */
class ShowOriginalAction(private val project: Project? = null) : AnAction("Show original view") {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        val target = project ?: e.project ?: return
        service<AutoTranslateManager>().showOriginalForSelected(target)
    }
}

/** Re-translates the selected original file back into a translated view (inverse of Show original). */
class ShowTranslatedAction(private val project: Project? = null) : AnAction("Show translated view") {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        val target = project ?: e.project ?: return
        service<AutoTranslateManager>().showTranslatedForSelected(target)
    }
}
