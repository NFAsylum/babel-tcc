package com.nfasylum.babel.intellij.services

import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.ProjectActivity
import com.nfasylum.babel.intellij.settings.BabelSettings

/**
 * Wires up the application-level [AutoTranslateManager] once a project opens.
 * Application services are lazy, so this touch is what makes the manager
 * subscribe to language changes. Idempotent across projects.
 */
class BabelStartupActivity : ProjectActivity {
    override suspend fun execute(project: Project) {
        // Load persisted settings early so LanguageService (and the status bar) reflect the
        // saved language from the start, not the default "off".
        service<BabelSettings>()
        service<AutoTranslateManager>().ensureSubscribed()
    }
}
