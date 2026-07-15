package com.nfasylum.babel.intellij.services

import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.ProjectActivity

/**
 * Wires up the application-level [AutoTranslateManager] once a project opens.
 * Application services are lazy, so this touch is what makes the manager
 * subscribe to language changes. Idempotent across projects.
 */
class BabelStartupActivity : ProjectActivity {
    override suspend fun execute(project: Project) {
        service<AutoTranslateManager>().ensureSubscribed()
    }
}
