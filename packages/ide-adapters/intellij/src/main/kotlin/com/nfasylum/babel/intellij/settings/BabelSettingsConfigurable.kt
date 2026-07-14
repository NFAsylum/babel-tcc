package com.nfasylum.babel.intellij.settings

import com.intellij.openapi.components.service
import com.intellij.openapi.options.Configurable
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBCheckBox
import com.intellij.ui.components.JBTextField
import com.intellij.util.ui.FormBuilder
import com.nfasylum.babel.intellij.BabelPlugin
import javax.swing.JComponent
import javax.swing.JPanel

/**
 * Settings UI shown under `Preferences → Babel`. Language dropdown, an enable
 * checkbox and an advanced custom Core.Host path field. Apply writes through
 * [BabelSettings], which syncs the runtime [com.nfasylum.babel.intellij.services.LanguageService].
 */
class BabelSettingsConfigurable : Configurable {
    private var panel: JPanel? = null
    private lateinit var languageCombo: ComboBox<String>
    private lateinit var enabledCheck: JBCheckBox
    private lateinit var hostPathField: JBTextField

    override fun getDisplayName(): String = "Babel"

    override fun createComponent(): JComponent {
        languageCombo = ComboBox(LanguagePicker.availableLanguages().toTypedArray())
        enabledCheck = JBCheckBox("Enable Babel translations")
        hostPathField = JBTextField()

        val built = FormBuilder.createFormBuilder()
            .addLabeledComponent("Language:", languageCombo)
            .addComponent(enabledCheck)
            .addLabeledComponent("Custom Core.Host path (advanced):", hostPathField)
            .addComponentFillVertically(JPanel(), 0)
            .panel
        panel = built
        reset()
        return built
    }

    private fun settings(): BabelSettings = service()

    override fun isModified(): Boolean {
        val s = settings()
        return languageCombo.selectedItem != s.language ||
            enabledCheck.isSelected != s.enabled ||
            hostPathField.text.orEmpty() != (s.coreHostPath ?: "")
    }

    override fun apply() {
        val s = settings()
        s.language = (languageCombo.selectedItem as? String) ?: BabelPlugin.LANGUAGE_NONE
        s.enabled = enabledCheck.isSelected
        s.coreHostPath = hostPathField.text
    }

    override fun reset() {
        val s = settings()
        languageCombo.selectedItem = s.language
        enabledCheck.isSelected = s.enabled
        hostPathField.text = s.coreHostPath ?: ""
    }

    override fun disposeUIResources() {
        panel = null
    }
}
