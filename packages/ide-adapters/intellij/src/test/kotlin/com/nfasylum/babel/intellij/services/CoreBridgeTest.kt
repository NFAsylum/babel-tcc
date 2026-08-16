package com.nfasylum.babel.intellij.services

import com.google.gson.Gson
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Assert.fail
import org.junit.Test
import java.util.concurrent.LinkedBlockingQueue
import java.util.concurrent.TimeUnit

/**
 * Unit tests for [CoreBridge] that never spawn the real .NET Core.Host. A
 * scripted [FakeTransport] stands in for the subprocess, so the request/response
 * framing, the timeout path and the lifecycle can be exercised deterministically.
 */
class CoreBridgeTest {
    private val gson = Gson()

    /** In-memory transport: records requests and emits scripted response lines. */
    private class FakeTransport : CoreTransport {
        val requests = mutableListOf<String>()
        val outbound = LinkedBlockingQueue<String>()

        @Volatile
        var closed = false

        /** Invoked on every write so a test can script the matching response. */
        var onWrite: ((String) -> Unit)? = null

        /** Scripted stderr, as the real transport would have captured it from the process. */
        var stderr: String = ""

        override fun drainStderr(): String {
            val captured = stderr
            stderr = ""
            return captured
        }

        override fun writeLine(line: String) {
            requests.add(line)
            onWrite?.invoke(line)
        }

        override fun readLine(): String? {
            while (true) {
                if (closed && outbound.isEmpty()) return null
                val next = outbound.poll(50, TimeUnit.MILLISECONDS)
                if (next != null) return next
            }
        }

        override fun isAlive(): Boolean = !closed

        override fun close() {
            closed = true
        }
    }

    private fun bridgeWith(fake: FakeTransport): CoreBridge =
        CoreBridge().apply { transportFactory = { fake } }

    @Test
    fun `start then stop opens and closes the transport`() {
        val fake = FakeTransport()
        val bridge = bridgeWith(fake)

        bridge.start()
        assertTrue("transport should be alive after start", fake.isAlive())

        bridge.stop()
        assertTrue("transport should be closed after stop", fake.closed)
    }

    @Test
    fun `translate happy path returns the Core result`() {
        val fake = FakeTransport()
        fake.onWrite = {
            fake.outbound.offer(gson.toJson(CoreResponse(success = true, result = "se (x) { }", error = "")))
        }
        val bridge = bridgeWith(fake)

        val translated = bridge.translateToNaturalLanguage("if (x) { }", ".cs", "pt-BR")

        assertEquals("se (x) { }", translated)
        assertEquals("exactly one request sent", 1, fake.requests.size)
        assertTrue("request carries the method", fake.requests[0].contains("TranslateToNaturalLanguage"))
        assertTrue("request carries the target language", fake.requests[0].contains("pt-BR"))
        bridge.stop()
    }

    @Test
    fun `core-side error is surfaced as CoreBridgeException`() {
        val fake = FakeTransport()
        fake.onWrite = {
            fake.outbound.offer(gson.toJson(CoreResponse(success = false, result = "", error = "unknown extension")))
        }
        val bridge = bridgeWith(fake)

        try {
            bridge.translateToNaturalLanguage("x", ".xyz", "pt-BR")
            fail("expected CoreBridgeException")
        } catch (e: CoreBridgeException) {
            assertTrue(e.message!!.contains("unknown extension"))
        }
        bridge.stop()
    }

    @Test
    fun `translate times out and kills the transport when the Core never answers`() {
        val fake = FakeTransport()
        // onWrite left null: the fake never produces a response line.
        val bridge = bridgeWith(fake).apply { timeoutMs = 200 }

        val start = System.nanoTime()
        try {
            bridge.translateToNaturalLanguage("if (x) { }", ".cs", "pt-BR")
            fail("expected a timeout CoreBridgeException")
        } catch (e: CoreBridgeException) {
            assertTrue("message should mention timeout", e.message!!.contains("Timeout"))
        }
        val elapsedMs = (System.nanoTime() - start) / 1_000_000

        assertTrue("should not block far past the timeout (was ${elapsedMs}ms)", elapsedMs < 2_000)
        assertTrue("timed-out transport should be killed", fake.closed)
    }

    @Test
    fun `timeout message carries what the Core wrote to stderr`() {
        val fake = FakeTransport()
        // A Core that died mid-request: no response line, but a stack trace on stderr.
        fake.stderr = "Unhandled exception. System.IO.FileNotFoundException: translations"
        val bridge = bridgeWith(fake).apply { timeoutMs = 200 }

        try {
            bridge.translateToNaturalLanguage("if (x) { }", ".cs", "pt-BR")
            fail("expected a timeout CoreBridgeException")
        } catch (e: CoreBridgeException) {
            assertTrue("message should mention timeout", e.message!!.contains("Timeout"))
            assertTrue("message should carry the stderr", e.message!!.contains("Core.Host stderr:"))
            assertTrue("message should name the cause", e.message!!.contains("FileNotFoundException"))
        }
    }

    @Test
    fun `a silent Core leaves the error message without a stderr section`() {
        val fake = FakeTransport()
        val bridge = bridgeWith(fake).apply { timeoutMs = 200 }

        try {
            bridge.translateToNaturalLanguage("if (x) { }", ".cs", "pt-BR")
            fail("expected a timeout CoreBridgeException")
        } catch (e: CoreBridgeException) {
            assertTrue("no stderr means no stderr section", !e.message!!.contains("Core.Host stderr"))
        }
    }

    @Test
    fun `getSupportedLanguages parses a JSON array result`() {
        val fake = FakeTransport()
        fake.onWrite = {
            fake.outbound.offer(
                gson.toJson(CoreResponse(success = true, result = """["en","pt-BR","es"]""", error = "")),
            )
        }
        val bridge = bridgeWith(fake)

        val languages = bridge.getSupportedLanguages()

        assertEquals(listOf("en", "pt-BR", "es"), languages)
        bridge.stop()
    }

    @Test
    fun `resolveLaunch runs Core via dotnet, from bundle when present else from PATH`() {
        val bridge = CoreBridge().apply {
            translationsPath = "/tmp/translations"
            projectPath = "/tmp/project"
        }

        val spec = bridge.resolveLaunch()

        // The test sandbox bundles core-host/, so resolveLaunch may return the bundled
        // native binary; without a bundle it falls back to `dotnet <dll>`. Accept both.
        val runsCore = spec.command == "dotnet" || spec.command.endsWith("MultiLingualCode.Core.Host")
        assertTrue("launches Core via dotnet or the bundled native binary", runsCore)
        if (spec.command == "dotnet") {
            assertTrue(spec.args.any { it.endsWith(CoreBridge.HOST_DLL_NAME) })
        }
        // Either way the translation/project args must be forwarded to the Core.
        assertTrue(spec.args.contains("--translations"))
        assertTrue(spec.args.contains("/tmp/translations"))
        assertTrue(spec.args.contains("--project"))
        assertTrue(spec.args.contains("/tmp/project"))
    }
}
