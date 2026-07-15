package com.nfasylum.babel.intellij.settings

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.PersistentStateComponent
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.State
import com.intellij.openapi.components.Storage
import com.nfasylum.babel.intellij.BabelPlugin
import com.nfasylum.babel.intellij.services.CoreBridge
import com.nfasylum.babel.intellij.services.LanguageService

/**
 * Persistent Babel settings, stored in `babel.xml` and surviving IDE restarts.
 *
 * This is the persistence layer; the hot-path runtime state lives in
 * [LanguageService]. Whenever the persisted state changes (on load or via the
 * settings UI) it is pushed into the runtime through [runtimeSync], which also
 * hands the custom Core.Host path to the [CoreBridge].
 */
@Service(Service.Level.APP)
@State(name = "BabelSettings", storages = [Storage("babel.xml")])
class BabelSettings : PersistentStateComponent<BabelSettings.State> {
    /** Serialized shape. Fields are plain/nullable to stay portable across storages. */
    data class State(
        var language: String = BabelPlugin.LANGUAGE_NONE,
        var enabled: Boolean = true,
        var coreHostPath: String? = null,
        var readonly: Boolean = false,
        var languageOverrides: MutableMap<String, String> = mutableMapOf(),
    )

    private var state = State()

    /**
     * Seam for tests: applies persisted state to the running services. Defaults
     * to pushing into [LanguageService] and [CoreBridge]; tests replace it so
     * persistence can be verified without booting the platform.
     */
    var runtimeSync: (State) -> Unit = ::defaultRuntimeSync

    override fun getState(): State = state

    override fun loadState(state: State) {
        this.state = state
        runtimeSync(state)
    }

    var language: String
        get() = state.language
        set(value) {
            state.language = value
            runtimeSync(state)
        }

    var enabled: Boolean
        get() = state.enabled
        set(value) {
            state.enabled = value
            runtimeSync(state)
        }

    var coreHostPath: String?
        get() = state.coreHostPath
        set(value) {
            state.coreHostPath = value?.takeIf { it.isNotBlank() }
            runtimeSync(state)
        }

    /** When true, translated views open read-only. */
    var readonly: Boolean
        get() = state.readonly
        set(value) {
            state.readonly = value
            runtimeSync(state)
        }

    /** Read-only view of the per-extension language overrides (extension without a dot). */
    val languageOverrides: Map<String, String>
        get() = state.languageOverrides

    /** Effective language for a file extension (e.g. ".cs" -> "pt-BR"), honoring per-extension overrides. */
    fun effectiveLanguage(fileExtension: String): String {
        val ext = fileExtension.lowercase().removePrefix(".")
        return state.languageOverrides[ext] ?: state.language
    }

    /** Sets a per-extension language override. A blank value or the default language clears it. */
    fun setLanguageOverride(fileExtension: String, language: String) {
        val ext = fileExtension.lowercase().removePrefix(".")
        if (language.isBlank() || language == state.language) {
            state.languageOverrides.remove(ext)
        } else {
            state.languageOverrides[ext] = language
        }
        runtimeSync(state)
    }

    /** Removes any per-extension override, falling back to the default language. */
    fun clearLanguageOverride(fileExtension: String) {
        val ext = fileExtension.lowercase().removePrefix(".")
        state.languageOverrides.remove(ext)
        runtimeSync(state)
    }

    private fun defaultRuntimeSync(state: State) {
        try {
            val app = ApplicationManager.getApplication() ?: return
            app.getService(LanguageService::class.java)?.apply {
                setEnabled(state.enabled)
                setLanguage(state.language)
            }
            app.getService(CoreBridge::class.java)?.coreHostPath = state.coreHostPath
        } catch (e: Exception) {
            // Services may not be ready during very early load; runtime will re-sync on next change.
        }
    }
}
