package com.nfasylum.babel.intellij.settings

import com.intellij.openapi.components.service
import com.intellij.openapi.options.Configurable
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBCheckBox
import com.intellij.ui.components.JBLabel
import com.intellij.ui.components.JBTextField
import com.intellij.util.ui.FormBuilder
import com.nfasylum.babel.intellij.BabelPlugin
import javax.swing.JComponent
import javax.swing.JPanel

/**
 * Settings UI shown under `Preferences → Babel`: language, enable, read-only views, a read-only
 * summary of per-language overrides (managed in the status bar menu), and the advanced custom
 * Core.Host path. Apply writes through [BabelSettings], the single source of truth.
 */
class BabelSettingsConfigurable : Configurable {
    private var panel: JPanel? = null
    private lateinit var languageCombo: ComboBox<String>
    private lateinit var enabledCheck: JBCheckBox
    private lateinit var readonlyCheck: JBCheckBox
    private lateinit var hostPathField: JBTextField
    private lateinit var overridesLabel: JBLabel

    override fun getDisplayName(): String = "Babel"

    override fun createComponent(): JComponent {
        languageCombo = ComboBox(LanguagePicker.availableLanguages().toTypedArray())
        enabledCheck = JBCheckBox("Enable Babel translations")
        readonlyCheck = JBCheckBox("Open translated views as read-only")
        hostPathField = JBTextField()
        overridesLabel = JBLabel()

        val built = FormBuilder.createFormBuilder()
            .addLabeledComponent("Language:", languageCombo)
            .addComponent(enabledCheck)
            .addComponent(readonlyCheck)
            .addLabeledComponent("Per-language overrides:", overridesLabel)
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
            readonlyCheck.isSelected != s.readonly ||
            hostPathField.text.orEmpty() != (s.coreHostPath ?: "")
    }

    override fun apply() {
        if (!isModified) return
        val s = settings()
        s.language = (languageCombo.selectedItem as? String) ?: BabelPlugin.LANGUAGE_NONE
        s.enabled = enabledCheck.isSelected
        s.readonly = readonlyCheck.isSelected
        s.coreHostPath = hostPathField.text
    }

    override fun reset() {
        val s = settings()
        languageCombo.selectedItem = s.language
        enabledCheck.isSelected = s.enabled
        readonlyCheck.isSelected = s.readonly
        hostPathField.text = s.coreHostPath ?: ""
        overridesLabel.text = overridesText(s.languageOverrides)
    }

    override fun disposeUIResources() {
        panel = null
    }

    companion object {
        /** Read-only summary of the per-language overrides; they are managed in the status bar menu. */
        fun overridesText(overrides: Map<String, String>): String {
            if (overrides.isEmpty()) return "none (manage in the status bar menu)"
            return overrides.entries.joinToString(", ") { ".${it.key} → ${it.value}" } +
                "  (manage in the status bar menu)"
        }
    }
}
