package com.nfasylum.babel.intellij.statusbar

import com.intellij.ide.DataManager
import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.intellij.openapi.wm.StatusBar
import com.intellij.openapi.wm.StatusBarWidget
import com.intellij.util.Consumer
import com.nfasylum.babel.intellij.services.LanguageService
import com.nfasylum.babel.intellij.services.TranslationService
import com.nfasylum.babel.intellij.settings.BabelSettings
import com.nfasylum.babel.intellij.settings.LanguagePicker
import com.nfasylum.babel.intellij.statusbar.actions.ClearOverrideAction
import com.nfasylum.babel.intellij.statusbar.actions.SelectLanguageMenuAction
import com.nfasylum.babel.intellij.statusbar.actions.SetOverrideAction
import com.nfasylum.babel.intellij.statusbar.actions.ShowOriginalAction
import com.nfasylum.babel.intellij.statusbar.actions.ToggleEnableAction
import com.nfasylum.babel.intellij.statusbar.actions.ToggleReadonlyAction
import java.awt.Component
import java.awt.event.MouseEvent

/**
 * Status bar item showing the active Babel language (e.g. "Babel: pt-BR", or
 * "Babel: pt-BR (ro)", or "Babel: off"). Clicking opens a structured control menu:
 * enable/readonly toggles, default language, per-extension overrides, and "show
 * original". Text refreshes on any [LanguageService] change.
 */
class BabelStatusBarWidget(private val project: Project) : StatusBarWidget, StatusBarWidget.TextPresentation {
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

    override fun getText(): String = widgetText(
        active = languageService.isTranslationActive(),
        language = languageService.currentLanguage,
        readonly = service<BabelSettings>().readonly,
    )

    override fun getAlignment(): Float = Component.CENTER_ALIGNMENT

    override fun getTooltipText(): String = "Babel controls — click to configure"

    override fun getClickConsumer(): Consumer<MouseEvent> = Consumer { event -> showMenu(event) }

    private fun showMenu(event: MouseEvent) {
        val settings = service<BabelSettings>()
        val group = buildControlGroup(
            defaultLanguage = settings.language,
            languages = LanguagePicker.availableLanguages(),
            overrides = settings.languageOverrides,
            extensions = service<TranslationService>().supportedExtensions(),
            project = project,
        )
        val dataContext = DataManager.getInstance().getDataContext(event.component)
        JBPopupFactory.getInstance()
            .createActionGroupPopup(
                "Babel Controls",
                group,
                dataContext,
                JBPopupFactory.ActionSelectionAid.SPEEDSEARCH,
                true,
            )
            .showInBestPositionFor(dataContext)
    }

    companion object {
        /** Pure text logic: the label for a given active state, language and readonly flag. */
        fun widgetText(active: Boolean, language: String, readonly: Boolean): String = when {
            !active -> "Babel: off"
            readonly -> "Babel: $language (ro)"
            else -> "Babel: $language"
        }

        /**
         * Builds the control menu action group from plain inputs (no platform services),
         * so the structure is unit-testable.
         */
        fun buildControlGroup(
            defaultLanguage: String,
            languages: List<String>,
            overrides: Map<String, String>,
            extensions: List<String>,
            project: Project?,
        ): DefaultActionGroup {
            val group = DefaultActionGroup()
            group.add(ToggleEnableAction())
            group.add(ToggleReadonlyAction())
            group.addSeparator()

            val langGroup = DefaultActionGroup("Language: $defaultLanguage", true)
            languages.forEach { langGroup.add(SelectLanguageMenuAction(it)) }
            group.add(langGroup)
            group.addSeparator()

            extensions.forEach { ext ->
                val label = overrides[ext] ?: "default: $defaultLanguage"
                val extGroup = DefaultActionGroup("Language for .$ext: $label", true)
                extGroup.add(ClearOverrideAction(ext))
                languages.forEach { extGroup.add(SetOverrideAction(ext, it)) }
                group.add(extGroup)
            }
            group.addSeparator()
            group.add(ShowOriginalAction(project))
            return group
        }
    }
}
