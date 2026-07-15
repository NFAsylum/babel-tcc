package com.nfasylum.babel.intellij.providers

import com.google.gson.Gson
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.command.WriteCommandAction
import com.intellij.openapi.fileEditor.FileDocumentManager
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.util.io.FileUtil
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.testFramework.fixtures.BasePlatformTestCase
import com.nfasylum.babel.intellij.services.CoreBridge
import com.nfasylum.babel.intellij.services.CoreResponse
import com.nfasylum.babel.intellij.services.CoreTransport
import java.util.concurrent.LinkedBlockingQueue
import java.util.concurrent.TimeUnit

/**
 * Part D regression: saving a translated view must write the reverse-translated
 * (original-language) source to the real disk file. Runs headless on the real
 * platform with a faked Core.Host, so it exercises the actual save pipeline —
 * including whether the platform routes a light-file save through setBinaryContent.
 */
class SaveIntegrationTest : BasePlatformTestCase() {
    private val gson = Gson()

    private class FakeTransport(private val responder: (String) -> String?) : CoreTransport {
        private val outbound = LinkedBlockingQueue<String>()

        @Volatile
        private var closed = false

        override fun writeLine(line: String) {
            responder(line)?.let { outbound.offer(it) }
        }

        override fun readLine(): String? {
            while (true) {
                if (closed && outbound.isEmpty()) return null
                outbound.poll(50, TimeUnit.MILLISECONDS)?.let { return it }
            }
        }

        override fun isAlive(): Boolean = !closed

        override fun close() {
            closed = true
        }
    }

    /** Wires the real CoreBridge app service to reverse any input to the given result. */
    private fun stubCoreReverse(result: String) {
        val bridge = ApplicationManager.getApplication().getService(CoreBridge::class.java)
        // Drop any transport a previous test left running so our factory takes effect.
        bridge.stop()
        bridge.transportFactory = { FakeTransport { gson.toJson(CoreResponse(success = true, result = result, error = "")) } }
    }

    private fun newTranslatedView(translated: String): Pair<TranslatedLightFile, java.io.File> {
        val ioFile = FileUtil.createTempFile("BabelSave", ".cs", true)
        ioFile.writeText("if (y) { }")
        val vFile = LocalFileSystem.getInstance().refreshAndFindFileByIoFile(ioFile) ?: error("no vfs file")
        val view = TranslatedView(vFile.path, "cs", "pt-BR", "if (y) { }", "se (y) { }")
        val light = TranslatedLightFile("BabelSave.cs", vFile.fileType, translated, view).apply { isWritable = true }
        return light to ioFile
    }

    fun `test setBinaryContent persists reverse-translated source to disk`() {
        stubCoreReverse("if (x) { }")
        val (light, ioFile) = newTranslatedView(translated = "se (x) { }")

        ApplicationManager.getApplication().runWriteAction {
            light.setBinaryContent("se (x) { }".toByteArray(light.charset))
        }

        assertEquals("disk holds original-language source", "if (x) { }", FileUtil.loadFile(ioFile))
    }

    fun `test Ctrl+S (saveAllDocuments) writes the open translated view to disk`() {
        stubCoreReverse("if (x) { }")
        val (light, ioFile) = newTranslatedView(translated = "se (y) { }")
        val editorManager = FileEditorManager.getInstance(project)
        editorManager.openFile(light, true)
        try {
            val document = FileDocumentManager.getInstance().getDocument(light) ?: error("no document")
            WriteCommandAction.runWriteCommandAction(project) { document.setText("se (x) { }") }

            // The Ctrl+S / autosave path: fires beforeAllDocumentsSaving even for the light view.
            FileDocumentManager.getInstance().saveAllDocuments()

            assertEquals("Ctrl+S persists reverse-translated source to disk", "if (x) { }", FileUtil.loadFile(ioFile))
        } finally {
            editorManager.closeFile(light)
        }
    }
}
