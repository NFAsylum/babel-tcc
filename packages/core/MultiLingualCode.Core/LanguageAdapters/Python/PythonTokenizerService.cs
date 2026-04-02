using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.LanguageAdapters.Python;

/// <summary>
/// Manages a persistent Python subprocess that tokenizes Python source code via JSON Lines protocol.
/// The subprocess is started lazily on the first <see cref="Tokenize"/> call and reused for subsequent calls.
/// </summary>
public class PythonTokenizerService : IDisposable
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    public static readonly Version MinimumPythonVersion = new(3, 8);

    public readonly object Lock = new();
    public Process Process = null!;
    public string ResolvedPythonPath = "";
    public string ResolvedPythonArgs = "";
    public bool PythonResolved = false;
    public bool Disposed = false;

    /// <summary>
    /// Tokenizes Python source code by sending it to the persistent Python subprocess.
    /// </summary>
    /// <param name="sourceCode">The Python source code to tokenize.</param>
    /// <returns>An operation result containing the list of tokens, or an error message on failure.</returns>
    public OperationResultGeneric<List<PythonToken>> Tokenize(string sourceCode)
    {
        lock (Lock)
        {
            if (Disposed)
            {
                return OperationResult.Fail<List<PythonToken>>("PythonTokenizerService has been disposed.");
            }

            OperationResult ensureResult = EnsureProcessRunning();
            if (!ensureResult.IsSuccess)
            {
                return OperationResult.Fail<List<PythonToken>>(ensureResult.ErrorMessage);
            }

            OperationResultGeneric<List<PythonToken>> result = SendTokenizeRequest(sourceCode);

            if (!result.IsSuccess && IsProcessDead())
            {
                // Process died; try one restart
                OperationResult restartResult = StartProcess();
                if (!restartResult.IsSuccess)
                {
                    return OperationResult.Fail<List<PythonToken>>(
                        $"Python subprocess crashed and restart failed: {restartResult.ErrorMessage}");
                }

                result = SendTokenizeRequest(sourceCode);
            }

            return result;
        }
    }

    /// <summary>
    /// Sends the quit command to the Python subprocess and releases all resources.
    /// </summary>
    public void Dispose()
    {
        lock (Lock)
        {
            if (Disposed)
            {
                return;
            }

            Disposed = true;
            StopProcess();
        }
    }

    /// <summary>
    /// Ensures the Python subprocess is running, starting it if necessary.
    /// </summary>
    /// <returns>A success result if the process is running; otherwise a failure with an error message.</returns>
    public OperationResult EnsureProcessRunning()
    {
        if (Process != null && !Process.HasExited)
        {
            return OperationResult.Ok();
        }

        return StartProcess();
    }

    /// <summary>
    /// Starts (or restarts) the Python subprocess.
    /// </summary>
    /// <returns>A success result if the process started; otherwise a failure with an error message.</returns>
    public OperationResult StartProcess()
    {
        StopProcess();

        if (!PythonResolved)
        {
            OperationResult resolveResult = ResolvePython();
            if (!resolveResult.IsSuccess)
            {
                return resolveResult;
            }
        }

        string scriptPath = GetScriptPath();
        if (!File.Exists(scriptPath))
        {
            return OperationResult.Fail(
                $"Python tokenizer script not found at: {scriptPath}");
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = ResolvedPythonPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (!string.IsNullOrEmpty(ResolvedPythonArgs))
            {
                startInfo.Arguments = $"{ResolvedPythonArgs} \"{scriptPath}\"";
            }
            else
            {
                startInfo.Arguments = $"\"{scriptPath}\"";
            }

            Process started = Process.Start(startInfo)!;

            if (started == null || started.HasExited)
            {
                return OperationResult.Fail("Failed to start Python subprocess.");
            }

            Process = started;
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to start Python subprocess: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a tokenize request to the subprocess and reads the response.
    /// </summary>
    /// <param name="sourceCode">The Python source code to tokenize.</param>
    /// <returns>The parsed token list, or an error.</returns>
    public OperationResultGeneric<List<PythonToken>> SendTokenizeRequest(string sourceCode)
    {
        if (Process == null || Process.HasExited)
        {
            return OperationResult.Fail<List<PythonToken>>("Python subprocess is not running.");
        }

        try
        {
            string request = JsonSerializer.Serialize(new { source = sourceCode });
            Process.StandardInput.WriteLine(request);
            Process.StandardInput.Flush();

            Task<string> readTask = Process.StandardOutput.ReadLineAsync()!;
            if (!readTask.Wait(RequestTimeout))
            {
                StopProcess();
                return OperationResult.Fail<List<PythonToken>>(
                    $"Python subprocess timed out after {RequestTimeout.TotalSeconds} seconds.");
            }

            string responseLine = readTask.Result;
            if (responseLine == null)
            {
                return OperationResult.Fail<List<PythonToken>>(
                    "Python subprocess closed its output stream unexpectedly.");
            }

            TokenizerResponse response;
            try
            {
                response = JsonSerializer.Deserialize<TokenizerResponse>(responseLine, JsonOptions)!;
            }
            catch (JsonException ex)
            {
                return OperationResult.Fail<List<PythonToken>>(
                    $"Invalid JSON response from Python subprocess: {ex.Message}");
            }

            if (response == null)
            {
                return OperationResult.Fail<List<PythonToken>>(
                    "Python subprocess returned null JSON response.");
            }

            List<PythonToken> tokens = response.Tokens ?? new List<PythonToken>();

            if (!response.Ok)
            {
                // The Python tokenizer may return partial tokens before the error.
                // We intentionally discard them and return Fail because partial token
                // lists could lead to incorrect translations if processed downstream.
                string errorDetail = response.Error ?? "Unknown tokenization error";
                return OperationResult.Fail<List<PythonToken>>(
                    $"Python tokenizer error: {errorDetail}");
            }

            return OperationResult.Ok(tokens);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return OperationResult.Fail<List<PythonToken>>(
                $"Error communicating with Python subprocess: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether the Python subprocess has exited.
    /// </summary>
    /// <returns>True if the process is null or has exited; otherwise false.</returns>
    public bool IsProcessDead()
    {
        return Process == null || Process.HasExited;
    }

    /// <summary>
    /// Stops the current Python subprocess, sending the quit command first.
    /// </summary>
    public void StopProcess()
    {
        if (Process == null)
        {
            return;
        }

        try
        {
            if (!Process.HasExited)
            {
                try
                {
                    Process.StandardInput.WriteLine("{\"cmd\":\"quit\"}");
                    Process.StandardInput.Flush();
                    Process.WaitForExit(2000);
                }
                catch
                {
                    // Ignore errors when sending quit; we will kill the process anyway.
                }

                if (!Process.HasExited)
                {
                    Process.Kill();
                }
            }
        }
        catch
        {
            // Ignore errors during cleanup.
        }
        finally
        {
            Process.Dispose();
            Process = null!;
        }
    }

    /// <summary>
    /// Resolves the Python executable path by trying known candidates in order of priority.
    /// Verifies that the found Python is version 3.8 or higher.
    /// </summary>
    /// <returns>A success result if Python was found; otherwise a failure listing tried candidates.</returns>
    public OperationResult ResolvePython()
    {
        // Check environment variable override first
        string envPython = Environment.GetEnvironmentVariable("BABEL_TCC_PYTHON") ?? "";
        if (!string.IsNullOrWhiteSpace(envPython))
        {
            OperationResultGeneric<Version> versionResult = TryGetPythonVersion(envPython, "");
            if (versionResult.IsSuccess)
            {
                if (versionResult.Value >= MinimumPythonVersion)
                {
                    ResolvedPythonPath = envPython;
                    ResolvedPythonArgs = "";
                    PythonResolved = true;
                    return OperationResult.Ok();
                }

                return OperationResult.Fail(
                    $"Python at '{envPython}' (from BABEL_TCC_PYTHON) is version {versionResult.Value}, " +
                    $"but version {MinimumPythonVersion} or higher is required.");
            }
        }

        // Standard candidates in order of priority
        (string Executable, string Args)[] candidates =
        {
            ("python3", ""),
            ("python", ""),
            ("py", "-3")
        };

        List<string> triedPaths = new();

        foreach ((string executable, string args) in candidates)
        {
            string candidateDescription = !string.IsNullOrEmpty(args) ? $"{executable} {args}" : executable;
            triedPaths.Add(candidateDescription);

            OperationResultGeneric<Version> versionResult = TryGetPythonVersion(executable, args);
            if (!versionResult.IsSuccess)
            {
                continue;
            }

            if (versionResult.Value < MinimumPythonVersion)
            {
                continue;
            }

            ResolvedPythonPath = executable;
            ResolvedPythonArgs = args;
            PythonResolved = true;
            return OperationResult.Ok();
        }

        return OperationResult.Fail(
            "Python 3.8+ not found. Tried the following candidates: " +
            string.Join(", ", triedPaths) + ". " +
            "Please install Python 3.8 or higher, or set the BABEL_TCC_PYTHON environment variable " +
            "to the path of your Python executable.");
    }

    /// <summary>
    /// Attempts to run a Python executable and parse its version from <c>--version</c> output.
    /// </summary>
    /// <param name="executable">The executable name or path.</param>
    /// <param name="extraArgs">Extra arguments to pass before <c>--version</c> (e.g. "-3" for py launcher).</param>
    /// <returns>The parsed Python version, or a failure if the executable could not be run or is not Python 3.</returns>
    public static OperationResultGeneric<Version> TryGetPythonVersion(string executable, string extraArgs)
    {
        try
        {
            string arguments = !string.IsNullOrEmpty(extraArgs) ? $"{extraArgs} --version" : "--version";

            ProcessStartInfo startInfo = new()
            {
                FileName = executable,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = Process.Start(startInfo)!;
            if (process == null)
            {
                return OperationResult.Fail<Version>("Failed to start process.");
            }

            string output = process.StandardOutput.ReadToEnd();
            string errorOutput = process.StandardError.ReadToEnd();
            process.WaitForExit(5000);

            // Python may write version to stdout or stderr depending on version
            string versionOutput = !string.IsNullOrWhiteSpace(output) ? output : errorOutput;
            versionOutput = versionOutput.Trim();

            // Expected format: "Python 3.x.y"
            if (!versionOutput.StartsWith("Python 3."))
            {
                return OperationResult.Fail<Version>(
                    $"Not Python 3: '{versionOutput}'");
            }

            string versionString = versionOutput.Substring("Python ".Length);

            // Handle version strings with extra suffixes like "3.12.0a1"
            int suffixIndex = versionString.IndexOfAny("abcrf+-".ToCharArray());
            if (suffixIndex >= 0)
            {
                versionString = versionString.Substring(0, suffixIndex);
            }

            if (Version.TryParse(versionString, out Version version))
            {
                return OperationResult.Ok(version);
            }

            return OperationResult.Fail<Version>(
                $"Could not parse version from: '{versionOutput}'");
        }
        catch (Exception)
        {
            return OperationResult.Fail<Version>(
                $"Executable '{executable}' not found or not accessible.");
        }
    }

    /// <summary>
    /// Resolves the path to <c>tokenizer_service.py</c> relative to the executing assembly location.
    /// </summary>
    /// <returns>The full path to the tokenizer service Python script.</returns>
    public static string GetScriptPath()
    {
        string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        return Path.Combine(assemblyDir, "LanguageAdapters", "Python", "tokenizer_service.py");
    }

    /// <summary>
    /// Internal model for deserializing the JSON response from the Python tokenizer subprocess.
    /// </summary>
    public class TokenizerResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether tokenization completed without errors.
        /// </summary>
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        /// <summary>
        /// Gets or sets the error message when tokenization fails.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; } = "";

        /// <summary>
        /// Gets or sets the list of tokens produced by the tokenizer.
        /// </summary>
        [JsonPropertyName("tokens")]
        public List<PythonToken> Tokens { get; set; } = new();
    }
}
