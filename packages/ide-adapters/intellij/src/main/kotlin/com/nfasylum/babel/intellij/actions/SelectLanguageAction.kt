package com.nfasylum.babel.intellij.actions

import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.components.service
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.nfasylum.babel.intellij.settings.BabelSettings
import com.nfasylum.babel.intellij.settings.LanguagePicker

/**
 * Command-palette entry "Babel: Select Language". Shows a popup of the available
 * languages; the choice is written to [BabelSettings], which syncs the runtime
 * language and triggers re-translation of open views.
 */
class SelectLanguageAction : AnAction() {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    override fun actionPerformed(e: AnActionEvent) {
        val languages = LanguagePicker.availableLanguages()
        JBPopupFactory.getInstance()
            .createPopupChooserBuilder(languages)
            .setTitle("Select Babel Language")
            .setItemChosenCallback { chosen -> service<BabelSettings>().language = chosen }
            .createPopup()
            .showInBestPositionFor(e.dataContext)
    }
}
