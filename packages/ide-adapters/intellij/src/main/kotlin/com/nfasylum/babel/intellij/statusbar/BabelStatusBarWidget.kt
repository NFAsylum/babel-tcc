package com.nfasylum.babel.intellij.statusbar

import com.intellij.openapi.components.service
import com.intellij.openapi.wm.StatusBar
import com.intellij.openapi.wm.StatusBarWidget
import com.intellij.ui.awt.RelativePoint
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.nfasylum.babel.intellij.services.LanguageService
import com.nfasylum.babel.intellij.settings.BabelSettings
import com.nfasylum.babel.intellij.settings.LanguagePicker
import com.intellij.util.Consumer
import java.awt.Component
import java.awt.event.MouseEvent

/**
 * Status bar item showing the active Babel language (e.g. "Babel: pt-BR", or
 * "Babel: off"). Clicking opens the same language picker as the action; the text
 * refreshes whenever [LanguageService] reports a change.
 */
class BabelStatusBarWidget : StatusBarWidget, StatusBarWidget.TextPresentation {
    private var statusBar: StatusBar? = null
    private val languageService = service<LanguageService>()
    private val onLanguageChange: () -> Unit = { statusBar?.updateWidget(ID()) }

    init {
        languageService.addChangeListener(onLanguageChange)
    }

    override fun ID(): String = BabelStatusBarWidgetFactory.WIDGET_ID

    override fun install(statusBar: StatusBar) {
        this.statusBar = statusBar
    }

    override fun dispose() {
        languageService.removeChangeListener(onLanguageChange)
        statusBar = null
    }

    override fun getPresentation(): StatusBarWidget.WidgetPresentation = this

    override fun getText(): String =
        widgetText(languageService.isTranslationActive(), languageService.currentLanguage)

    override fun getAlignment(): Float = Component.CENTER_ALIGNMENT

    override fun getTooltipText(): String = "Babel active language — click to change"

    override fun getClickConsumer(): Consumer<MouseEvent> = Consumer { event ->
        JBPopupFactory.getInstance()
            .createPopupChooserBuilder(LanguagePicker.availableLanguages())
            .setTitle("Select Babel Language")
            .setItemChosenCallback { chosen -> service<BabelSettings>().language = chosen }
            .createPopup()
            .show(RelativePoint(event))
    }

    companion object {
        /** Pure text logic: the label for a given active state and language. */
        fun widgetText(active: Boolean, language: String): String =
            if (active) "Babel: $language" else "Babel: off"
    }
}
