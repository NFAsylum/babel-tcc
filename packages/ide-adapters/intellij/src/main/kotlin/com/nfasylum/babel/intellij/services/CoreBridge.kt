package com.nfasylum.babel.intellij.services

import com.google.gson.Gson
import com.google.gson.JsonObject
import com.google.gson.JsonParser
import com.intellij.ide.plugins.PluginManagerCore
import com.intellij.openapi.components.Service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.extensions.PluginId
import com.nfasylum.babel.intellij.BabelPlugin
import java.io.BufferedReader
import java.io.File
import java.io.InputStreamReader
import java.io.OutputStreamWriter
import java.io.Writer
import java.nio.charset.StandardCharsets
import java.util.concurrent.LinkedBlockingQueue
import java.util.concurrent.TimeUnit
import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.thread
import kotlin.concurrent.withLock

/** Response envelope returned by the C# Core.Host over the JSON Lines protocol (DT-002). */
data class CoreResponse(
    val success: Boolean = false,
    val result: String = "",
    val error: String = "",
)

/** How to launch the Core.Host process: a command plus its arguments. */
data class LaunchSpec(val command: String, val args: List<String>)

/** Raised when the Core engine cannot satisfy a request (crash, timeout, or a Core-side error). */
class CoreBridgeException(message: String) : RuntimeException(message)

/**
 * Bidirectional byte transport to a running Core.Host process. Extracted as an
 * interface so unit tests can drive [CoreBridge] with a scripted in-memory fake
 * instead of spawning the real .NET binary.
 */
interface CoreTransport {
    fun writeLine(line: String)

    /** Blocks until a full response line is available; returns null at end-of-stream. */
    fun readLine(): String?

    fun isAlive(): Boolean

    fun close()
}

/**
 * Kotlin port of the VS Code extension's `CoreBridge` (see
 * `packages/ide-adapters/vscode/src/services/coreBridge.ts`).
 *
 * Keeps a single long-lived Core.Host subprocess and talks to it with newline
 * delimited JSON (DT-002). Requests are serialised under a lock so each response
 * line maps unambiguously to the request that produced it — the Core answers
 * exactly one line per request, in order. A background thread drains stdout into
 * a queue so reads can honour a timeout without blocking forever.
 *
 * Registered as an application-level service; the no-arg constructor is required
 * by the platform. Tests construct it directly and replace [transportFactory].
 */
@Service(Service.Level.APP)
class CoreBridge {
    private val log = Logger.getInstance(CoreBridge::class.java)
    private val gson = Gson()

    /** Per-request timeout. Mutable so tests can shrink it. */
    var timeoutMs: Long = DEFAULT_TIMEOUT_MS

    /** Explicit Core.Host path override (a native binary or a `.dll`); null uses the bundled binary. */
    var coreHostPath: String? = null

    /** Passed to Core.Host as `--translations`; null lets the Core use its default. */
    var translationsPath: String? = null

    /** Passed to Core.Host as `--project`; null omits it. Enables identifier maps. */
    var projectPath: String? = null

    /** Seam for tests: builds a transport for a resolved [LaunchSpec]. Defaults to a real subprocess. */
    var transportFactory: (LaunchSpec) -> CoreTransport = ::spawnProcess

    private val lock = ReentrantLock()
    private val responses = LinkedBlockingQueue<String>()

    @Volatile
    private var transport: CoreTransport? = null

    /** Bumped on every (re)start; the reader thread uses it to ignore output from a superseded process. */
    @Volatile
    private var generation = 0

    private var restartCount = 0
    private var consecutiveTimeouts = 0

    @Volatile
    private var disposed = false

    // --- lifecycle --------------------------------------------------------

    /** Starts the Core.Host process if it is not already running. Idempotent. */
    fun start() {
        lock.withLock { ensureStarted() }
    }

    private fun ensureStarted() {
        val current = transport
        if (current != null && current.isAlive()) {
            return
        }
        if (restartCount >= MAX_CRASHES) {
            throw CoreBridgeException(
                "Core engine is unstable: refusing to restart after $MAX_CRASHES failed attempts.",
            )
        }
        startProcess()
    }

    private fun startProcess() {
        val spec = resolveLaunch()
        log.info("CoreBridge: starting Core.Host (${spec.command})")
        val newTransport = transportFactory(spec)
        responses.clear()
        val myGeneration = ++generation
        transport = newTransport
        restartCount++
        thread(name = "babel-core-reader-$myGeneration", isDaemon = true) {
            readLoop(newTransport, myGeneration)
        }
        log.info("CoreBridge started")
    }

    private fun readLoop(source: CoreTransport, myGeneration: Int) {
        try {
            while (generation == myGeneration) {
                val line = source.readLine() ?: break
                if (generation == myGeneration) {
                    responses.offer(line)
                }
            }
        } catch (e: Exception) {
            if (generation == myGeneration && !disposed) {
                log.warn("CoreBridge: reader thread stopped: ${e.message}")
            }
        }
    }

    /** Sends the quit sentinel and tears down the process. Safe to call more than once. */
    fun stop() {
        lock.withLock {
            disposed = true
            generation++
            transport?.let {
                try {
                    it.close()
                } catch (e: Exception) {
                    log.warn("CoreBridge: error during shutdown: ${e.message}")
                }
            }
            transport = null
        }
    }

    // --- request dispatch -------------------------------------------------

    /**
     * Sends one request and returns its [CoreResponse]. Serialised: concurrent
     * callers are queued so responses never interleave.
     */
    fun invoke(method: String, params: Map<String, String>): CoreResponse {
        lock.withLock {
            ensureStarted()
            val active = transport ?: throw CoreBridgeException("Core process unavailable")

            val request = JsonObject().apply {
                addProperty("method", method)
                add("params", gson.toJsonTree(params))
            }
            // Drop any late line left behind by a previously timed-out request so
            // ordering stays sound for this one.
            responses.clear()
            active.writeLine(gson.toJson(request))

            val line = responses.poll(timeoutMs, TimeUnit.MILLISECONDS)
            if (line == null) {
                onTimeout(method)
            }

            val response = parseResponse(line)
            consecutiveTimeouts = 0
            restartCount = 0
            if (!response.success) {
                throw CoreBridgeException("Core error for '$method': ${response.error}")
            }
            return response
        }
    }

    private fun onTimeout(method: String): Nothing {
        consecutiveTimeouts++
        // Kill the process so its stream is clean; the next call restarts fresh.
        generation++
        try {
            transport?.close()
        } catch (_: Exception) {
        }
        transport = null
        throw CoreBridgeException("Timeout after ${timeoutMs}ms for method '$method'")
    }

    private fun parseResponse(line: String): CoreResponse {
        return try {
            gson.fromJson(line, CoreResponse::class.java)
                ?: throw CoreBridgeException("Empty response from Core")
        } catch (e: Exception) {
            throw CoreBridgeException("Failed to parse Core response: $line")
        }
    }

    // --- typed API (mirrors the C# Program.cs method names) ---------------

    fun translateToNaturalLanguage(sourceCode: String, fileExtension: String, targetLanguage: String): String =
        invoke(
            "TranslateToNaturalLanguage",
            mapOf("sourceCode" to sourceCode, "fileExtension" to fileExtension, "targetLanguage" to targetLanguage),
        ).result

    fun translateFromNaturalLanguage(translatedCode: String, fileExtension: String, sourceLanguage: String): String =
        invoke(
            "TranslateFromNaturalLanguage",
            mapOf("translatedCode" to translatedCode, "fileExtension" to fileExtension, "sourceLanguage" to sourceLanguage),
        ).result

    fun applyTranslatedEdits(
        originalCode: String,
        previousTranslatedCode: String,
        editedTranslatedCode: String,
        fileExtension: String,
        sourceLanguage: String,
    ): String = invoke(
        "ApplyTranslatedEdits",
        mapOf(
            "originalCode" to originalCode,
            "previousTranslatedCode" to previousTranslatedCode,
            "editedTranslatedCode" to editedTranslatedCode,
            "fileExtension" to fileExtension,
            "sourceLanguage" to sourceLanguage,
        ),
    ).result

    /** Returns the translated-keyword -> original-keyword map for a language pair. */
    fun getKeywordMap(fileExtension: String, targetLanguage: String): Map<String, String> {
        val json = invoke(
            "GetKeywordMap",
            mapOf("fileExtension" to fileExtension, "targetLanguage" to targetLanguage),
        ).result
        return parseStringMap(json)
    }

    /** Returns the translated-identifier -> original-identifier map for the project. */
    fun getIdentifierMap(targetLanguage: String): Map<String, String> {
        val json = invoke("GetIdentifierMap", mapOf("targetLanguage" to targetLanguage)).result
        return parseStringMap(json)
    }

    fun getSupportedLanguages(): List<String> {
        val json = invoke("GetSupportedLanguages", emptyMap()).result
        return JsonParser.parseString(json).asJsonArray.map { it.asString }
    }

    private fun parseStringMap(json: String): Map<String, String> {
        val obj = JsonParser.parseString(json).asJsonObject
        return obj.entrySet().associate { (k, v) -> k to v.asString }
    }

    // --- launch resolution ------------------------------------------------

    /**
     * Resolves how to start Core.Host, mirroring the VS Code dual-track logic
     * (DT-010): prefer an explicit override, then the bundled native binary,
     * then a bundled `.dll` run via `dotnet`.
     */
    fun resolveLaunch(): LaunchSpec {
        val extraArgs = buildList {
            translationsPath?.let { add("--translations"); add(it) }
            projectPath?.let { add("--project"); add(it) }
        }

        coreHostPath?.let { path ->
            val file = File(path)
            if (file.exists()) {
                return if (file.extension.equals("dll", ignoreCase = true)) {
                    LaunchSpec("dotnet", listOf(file.absolutePath) + extraArgs)
                } else {
                    ensureExecutable(file)
                    LaunchSpec(file.absolutePath, extraArgs)
                }
            }
        }

        bundledHostFile(nativeBinaryName())?.let { native ->
            ensureExecutable(native)
            return LaunchSpec(native.absolutePath, extraArgs)
        }
        bundledHostFile(HOST_DLL_NAME)?.let { dll ->
            return LaunchSpec("dotnet", listOf(dll.absolutePath) + extraArgs)
        }

        // Last resort: assume the .NET runtime and the dll are discoverable.
        return LaunchSpec("dotnet", listOf(HOST_DLL_NAME) + extraArgs)
    }

    /** Locates a file under the installed plugin's `bin/` directory, or null if absent. */
    private fun bundledHostFile(name: String): File? {
        return try {
            val plugin = PluginManagerCore.getPlugin(PluginId.getId(BabelPlugin.PLUGIN_ID)) ?: return null
            val candidate = plugin.pluginPath.resolve("bin").resolve(name).toFile()
            if (candidate.exists()) candidate else null
        } catch (e: Exception) {
            null
        }
    }

    private fun ensureExecutable(file: File) {
        if (isWindows()) return
        try {
            file.setExecutable(true, false)
        } catch (e: Exception) {
            log.warn("CoreBridge: could not set executable bit on ${file.path}: ${e.message}")
        }
    }

    /** Default transport: spawn the real process and wrap its streams. */
    private fun spawnProcess(spec: LaunchSpec): CoreTransport {
        val process = ProcessBuilder(listOf(spec.command) + spec.args)
            .redirectErrorStream(false)
            .start()
        return ProcessTransport(process, gson)
    }

    companion object {
        const val DEFAULT_TIMEOUT_MS: Long = 10_000
        const val MAX_CRASHES: Int = 3
        const val HOST_DLL_NAME: String = "MultiLingualCode.Core.Host.dll"

        private fun isWindows(): Boolean =
            System.getProperty("os.name").orEmpty().startsWith("Windows", ignoreCase = true)

        private fun nativeBinaryName(): String =
            if (isWindows()) "MultiLingualCode.Core.Host.exe" else "MultiLingualCode.Core.Host"
    }
}

/** Real transport backed by a [Process], reading/writing UTF-8 JSON Lines. */
private class ProcessTransport(
    private val process: Process,
    private val gson: Gson,
) : CoreTransport {
    private val writer: Writer = OutputStreamWriter(process.outputStream, StandardCharsets.UTF_8)
    private val reader: BufferedReader = BufferedReader(InputStreamReader(process.inputStream, StandardCharsets.UTF_8))

    override fun writeLine(line: String) {
        writer.write(line)
        writer.write("\n")
        writer.flush()
    }

    override fun readLine(): String? = reader.readLine()

    override fun isAlive(): Boolean = process.isAlive

    override fun close() {
        try {
            writer.write(gson.toJson(mapOf("method" to "quit")))
            writer.write("\n")
            writer.flush()
        } catch (_: Exception) {
            // process may already be gone
        }
        if (!process.waitFor(DISPOSE_TIMEOUT_MS, TimeUnit.MILLISECONDS)) {
            process.destroyForcibly()
        }
    }

    private companion object {
        const val DISPOSE_TIMEOUT_MS: Long = 2_000
    }
}
