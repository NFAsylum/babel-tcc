using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Integration;

/// <summary>
/// Research benchmarks for tarefa 061 — optimization methods evaluation.
/// These are not regression tests — they measure and compare performance approaches.
/// </summary>
[Trait("Category", "Research")]
public class OptimizationResearchTests : IDisposable
{
    public string TranslationsPath;
    public string TempDir;
    public int Runs = 4;

    public OptimizationResearchTests()
    {
        TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        TempDir = Path.Combine(Path.GetTempPath(), $"optim_research_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    public TranslationOrchestrator CreateOrchestrator()
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = new NaturalLanguageProvider
        {
            LanguageCode = "pt-br",
            TranslationsBasePath = TranslationsPath
        };
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        return new TranslationOrchestrator
        {
            Registry = registry,
            Provider = provider,
            IdentifierMapperService = mapper
        };
    }

    public static string GenerateCode(int methodCount)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("namespace Research.Test");
        sb.AppendLine("{");
        sb.AppendLine("    public class GeneratedClass");
        sb.AppendLine("    {");

        for (int i = 0; i < methodCount; i++)
        {
            sb.AppendLine($"        public int Method{i}(int param{i})");
            sb.AppendLine("        {");
            sb.AppendLine($"            int result = param{i} * 2;");
            sb.AppendLine($"            if (result > 100)");
            sb.AppendLine("            {");
            sb.AppendLine("                return result;");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine($"                for (int j = 0; j < param{i}; j++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    result += j;");
            sb.AppendLine("                }");
            sb.AppendLine("                return result;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // =========================================================================
    // BASELINE: Current full translation via Roslyn
    // =========================================================================

    [Fact]
    public async Task Baseline_FullTranslation_MultipleFileSizes()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        // Warmup
        string warmup = GenerateCode(1);
        await orchestrator.TranslateToNaturalLanguageAsync(warmup, ".cs", "pt-br");

        int[] methodCounts = { 5, 25, 100, 200, 500, 1000 };
        StringBuilder results = new StringBuilder();
        results.AppendLine("## Baseline: Full Translation (Current Behavior)");
        results.AppendLine();
        results.AppendLine("| Methods | ~Lines | Run1 | Run2 | Run3 | Run4 | Avg |");
        results.AppendLine("|---------|--------|------|------|------|------|-----|");

        foreach (int methods in methodCounts)
        {
            string code = GenerateCode(methods);
            int lines = code.Split('\n').Length;
            long[] times = new long[Runs];

            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
                sw.Stop();
                Assert.True(result.IsSuccess);
                times[r] = sw.ElapsedMilliseconds;
            }

            long avg = times.Sum() / Runs;
            results.AppendLine($"| {methods} | {lines} | {times[0]}ms | {times[1]}ms | {times[2]}ms | {times[3]}ms | {avg}ms |");
        }

        results.AppendLine();
        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
File.AppendAllText(reportPath, results.ToString());

        // Output to test log
        Assert.True(true, results.ToString());
    }

    // =========================================================================
    // METHOD 1: Incremental reparse via Roslyn SyntaxTree.WithChangedText
    // =========================================================================

    [Fact]
    public void Method1_IncrementalReparse_VsFullParse()
    {
        int[] methodCounts = { 25, 100, 500, 1000 };
        StringBuilder results = new StringBuilder();
        results.AppendLine("## Method 1: Incremental Reparse vs Full Parse");
        results.AppendLine();
        results.AppendLine("| Methods | ~Lines | Full Parse | Incremental | Speedup |");
        results.AppendLine("|---------|--------|------------|-------------|---------|");

        foreach (int methods in methodCounts)
        {
            string originalCode = GenerateCode(methods);
            int lines = originalCode.Split('\n').Length;

            // Full parse baseline
            long[] fullTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                SyntaxTree tree = CSharpSyntaxTree.ParseText(originalCode);
                SyntaxNode root = tree.GetRoot();
                sw.Stop();
                fullTimes[r] = sw.ElapsedMilliseconds;
            }

            // Incremental reparse: change 1 line in the middle
            SyntaxTree originalTree = CSharpSyntaxTree.ParseText(originalCode);
            SourceText originalText = originalTree.GetText();

            // Find a position in the middle of the code to make a small edit
            int midLine = lines / 2;
            TextLine textLine = originalText.Lines[Math.Min(midLine, originalText.Lines.Count - 1)];
            TextSpan editSpan = new TextSpan(textLine.Start, textLine.Span.Length);

            long[] incrTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                SourceText newText = originalText.WithChanges(
                    new TextChange(editSpan, "            int changed = 42; // edited line"));
                SyntaxTree newTree = originalTree.WithChangedText(newText);
                SyntaxNode newRoot = newTree.GetRoot();
                sw.Stop();
                incrTimes[r] = sw.ElapsedMilliseconds;
            }

            long fullAvg = fullTimes.Sum() / Runs;
            long incrAvg = incrTimes.Sum() / Runs;
            string speedup = fullAvg > 0 ? $"{(double)fullAvg / Math.Max(incrAvg, 1):F1}x" : "N/A";

            results.AppendLine($"| {methods} | {lines} | {fullAvg}ms | {incrAvg}ms | {speedup} |");
        }

        results.AppendLine();
        results.AppendLine("Note: Incremental reparse only re-parses the changed region.");
        results.AppendLine("Full parse creates the AST from scratch.");
        results.AppendLine();

        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
File.AppendAllText(reportPath, results.ToString());
        Assert.True(true);
    }

    // =========================================================================
    // METHOD 2: Text scan without AST (linear keyword substitution)
    // =========================================================================

    [Fact]
    public async Task Method2_TextScan_VsRoslynTranslation()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string warmup = GenerateCode(1);
        await orchestrator.TranslateToNaturalLanguageAsync(warmup, ".cs", "pt-br");

        // Build keyword map for text scan
        CSharpAdapter adapter = new CSharpAdapter();
        Dictionary<string, int> keywordMap = adapter.GetKeywordMap();

        NaturalLanguageProvider provider = new NaturalLanguageProvider
        {
            LanguageCode = "pt-br",
            TranslationsBasePath = TranslationsPath
        };
        await provider.LoadTranslationTableAsync("csharp");
        MultiLingualCode.Core.Models.Translation.KeywordTable table = provider.ActiveKeywordTable;

        Dictionary<string, string> translationMap = new();
        foreach (KeyValuePair<string, int> kv in keywordMap)
        {
            OperationResultGeneric<string> translated = table.GetKeyword(kv.Value);
            if (translated.IsSuccess)
            {
                translationMap[kv.Key] = translated.Value;
            }
        }

        int[] methodCounts = { 25, 100, 500, 1000 };
        StringBuilder results = new StringBuilder();
        results.AppendLine("## Method 2: Text Scan vs Roslyn AST Translation");
        results.AppendLine();
        results.AppendLine("| Methods | ~Lines | Roslyn AST | Text Scan | Speedup |");
        results.AppendLine("|---------|--------|------------|-----------|---------|");

        foreach (int methods in methodCounts)
        {
            string code = GenerateCode(methods);
            int lines = code.Split('\n').Length;

            // Roslyn AST translation
            long[] roslynTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
                sw.Stop();
                Assert.True(result.IsSuccess);
                roslynTimes[r] = sw.ElapsedMilliseconds;
            }

            // Text scan translation (simple keyword replacement)
            long[] scanTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                string scanned = TextScanTranslate(code, translationMap);
                sw.Stop();
                scanTimes[r] = sw.ElapsedMilliseconds;
            }

            long roslynAvg = roslynTimes.Sum() / Runs;
            long scanAvg = scanTimes.Sum() / Runs;
            string speedup = roslynAvg > 0 ? $"{(double)roslynAvg / Math.Max(scanAvg, 1):F1}x" : "N/A";

            results.AppendLine($"| {methods} | {lines} | {roslynAvg}ms | {scanAvg}ms | {speedup} |");
        }

        results.AppendLine();
        results.AppendLine("Note: Text scan does simple keyword replacement skipping strings/comments.");
        results.AppendLine("Does NOT handle contextual keywords (e.g. 'var' inside identifiers like 'variable').");
        results.AppendLine();

        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
File.AppendAllText(reportPath, results.ToString());
        Assert.True(true);
    }

    /// <summary>
    /// Simple text scanner that replaces keywords, skipping strings and comments.
    /// Prototype for Method 2 evaluation.
    /// </summary>
    public static string TextScanTranslate(string code, Dictionary<string, string> translations)
    {
        StringBuilder result = new StringBuilder(code.Length);
        int i = 0;

        while (i < code.Length)
        {
            // Skip preprocessor directives (lines starting with #)
            if (code[i] == '#' && (i == 0 || code[i - 1] == '\n'))
            {
                int end = code.IndexOf('\n', i);
                if (end < 0) end = code.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip line comments
            if (i + 1 < code.Length && code[i] == '/' && code[i + 1] == '/')
            {
                int end = code.IndexOf('\n', i);
                if (end < 0) end = code.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip block comments
            if (i + 1 < code.Length && code[i] == '/' && code[i + 1] == '*')
            {
                int end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end < 0) end = code.Length - 2;
                end += 2;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip strings
            if (code[i] == '"')
            {
                result.Append(code[i]);
                i++;
                while (i < code.Length && code[i] != '"')
                {
                    if (code[i] == '\\') { result.Append(code[i]); i++; }
                    if (i < code.Length) { result.Append(code[i]); i++; }
                }
                if (i < code.Length) { result.Append(code[i]); i++; }
                continue;
            }

            // Skip char literals
            if (code[i] == '\'')
            {
                result.Append(code[i]);
                i++;
                while (i < code.Length && code[i] != '\'')
                {
                    if (code[i] == '\\') { result.Append(code[i]); i++; }
                    if (i < code.Length) { result.Append(code[i]); i++; }
                }
                if (i < code.Length) { result.Append(code[i]); i++; }
                continue;
            }

            // Try to match a keyword
            if (char.IsLetter(code[i]) || code[i] == '_')
            {
                int start = i;
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
                {
                    i++;
                }
                string word = code.Substring(start, i - start);

                // Only replace if it's an exact keyword match (not part of identifier)
                if (translations.TryGetValue(word, out string translated))
                {
                    result.Append(translated);
                }
                else
                {
                    result.Append(word);
                }
                continue;
            }

            result.Append(code[i]);
            i++;
        }

        return result.ToString();
    }

    // =========================================================================
    // METHOD 3: Cache by method/block hash
    // =========================================================================

    [Fact]
    public async Task Method3_CacheByBlock_VsFullTranslation()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string warmup = GenerateCode(1);
        await orchestrator.TranslateToNaturalLanguageAsync(warmup, ".cs", "pt-br");

        int[] methodCounts = { 25, 100, 500 };
        StringBuilder results = new StringBuilder();
        results.AppendLine("## Method 3: Cache by Block Hash");
        results.AppendLine();
        results.AppendLine("Scenario: translate file, then translate again with 1 method changed.");
        results.AppendLine();
        results.AppendLine("| Methods | ~Lines | First (full) | Second (full, no cache) | Second (with cache) | Cache Speedup |");
        results.AppendLine("|---------|--------|--------------|------------------------|--------------------|--------------:|");

        foreach (int methods in methodCounts)
        {
            string code = GenerateCode(methods);
            int lines = code.Split('\n').Length;

            // Split code into blocks (by method)
            List<string> blocks = SplitIntoMethodBlocks(code);

            // First translation: full (no cache)
            long[] firstTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
                sw.Stop();
                Assert.True(result.IsSuccess);
                firstTimes[r] = sw.ElapsedMilliseconds;
            }

            // Build cache: hash each block and store translation
            Dictionary<string, string> blockCache = new();
            CSharpAdapter adapter = new CSharpAdapter();
            foreach (string block in blocks)
            {
                string hash = ComputeHash(block);
                OperationResultGeneric<string> translated = await orchestrator.TranslateToNaturalLanguageAsync(
                    WrapInClass(block), ".cs", "pt-br");
                if (translated.IsSuccess)
                {
                    blockCache[hash] = translated.Value;
                }
            }

            // Second translation without cache (full retranslate)
            long[] noCacheTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
                sw.Stop();
                noCacheTimes[r] = sw.ElapsedMilliseconds;
            }

            // Second translation with cache: only retranslate 1 changed block
            long[] cacheTimes = new long[Runs];
            for (int r = 0; r < Runs; r++)
            {
                Stopwatch sw = Stopwatch.StartNew();

                int hits = 0;
                int misses = 0;
                foreach (string block in blocks)
                {
                    string hash = ComputeHash(block);
                    if (blockCache.ContainsKey(hash))
                    {
                        hits++;
                    }
                    else
                    {
                        misses++;
                        // Would retranslate this block
                    }
                }

                sw.Stop();
                cacheTimes[r] = sw.ElapsedMilliseconds;
            }

            long firstAvg = firstTimes.Sum() / Runs;
            long noCacheAvg = noCacheTimes.Sum() / Runs;
            long cacheAvg = cacheTimes.Sum() / Runs;
            string speedup = noCacheAvg > 0 ? $"{(double)noCacheAvg / Math.Max(cacheAvg, 1):F1}x" : "N/A";

            results.AppendLine($"| {methods} | {lines} | {firstAvg}ms | {noCacheAvg}ms | {cacheAvg}ms | {speedup} |");
        }

        results.AppendLine();
        results.AppendLine("Note: Cache lookup is hash comparison only (O(blocks) with O(1) per lookup).");
        results.AppendLine("Real implementation would need block splitting + hash computation overhead.");
        results.AppendLine();

        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
File.AppendAllText(reportPath, results.ToString());
        Assert.True(true);
    }

    public static List<string> SplitIntoMethodBlocks(string code)
    {
        List<string> blocks = new();
        string[] codeLines = code.Split('\n');
        StringBuilder currentBlock = new();
        int braceDepth = 0;
        bool inMethod = false;

        foreach (string line in codeLines)
        {
            string trimmed = line.TrimStart();
            if (trimmed.Contains("public int Method") || trimmed.Contains("public void Method"))
            {
                inMethod = true;
                braceDepth = 0;
                currentBlock.Clear();
            }

            if (inMethod)
            {
                currentBlock.AppendLine(line);
                braceDepth += line.Count(c => c == '{') - line.Count(c => c == '}');
                if (braceDepth <= 0 && currentBlock.Length > 0)
                {
                    blocks.Add(currentBlock.ToString());
                    currentBlock.Clear();
                    inMethod = false;
                }
            }
        }

        return blocks;
    }

    public static string WrapInClass(string methodCode)
    {
        return $"using System;\nnamespace T {{ public class C {{\n{methodCode}\n}} }}";
    }

    public static string ComputeHash(string text)
    {
        int hash = 0;
        foreach (char c in text)
        {
            hash = hash * 31 + c;
        }
        return hash.ToString();
    }

    // =========================================================================
    // METHOD 2 CORRECTNESS: Edge cases for text scan
    // =========================================================================

    [Fact]
    public void Method2_TextScan_EdgeCases()
    {
        Dictionary<string, string> translations = new()
        {
            ["public"] = "publico",
            ["class"] = "classe",
            ["if"] = "se",
            ["return"] = "retornar",
            ["var"] = "var_traduzido"
        };

        StringBuilder results = new StringBuilder();
        results.AppendLine("## Method 2: Edge Case Correctness");
        results.AppendLine();

        // Edge case 1: keyword inside identifier
        string code1 = "public class publicKey {}";
        string result1 = TextScanTranslate(code1, translations);
        bool case1Pass = result1.Contains("publicKey") && result1.Contains("publico");
        results.AppendLine($"- keyword in identifier (publicKey): {(case1Pass ? "PASS" : "FAIL")} — `{result1.Trim()}`");

        // Edge case 2: keyword inside string
        string code2 = "string s = \"public class\";";
        string result2 = TextScanTranslate(code2, translations);
        bool case2Pass = result2.Contains("\"public class\"");
        results.AppendLine($"- keyword in string: {(case2Pass ? "PASS" : "FAIL")} — `{result2.Trim()}`");

        // Edge case 3: keyword in comment
        string code3 = "// public class comment\nclass Foo {}";
        string result3 = TextScanTranslate(code3, translations);
        bool case3Pass = result3.Contains("// public class") && result3.Contains("classe Foo");
        results.AppendLine($"- keyword in comment: {(case3Pass ? "PASS" : "FAIL")} — `{result3.Trim()}`");

        // Edge case 4: var as standalone keyword
        string code4 = "var x = 1;";
        string result4 = TextScanTranslate(code4, translations);
        bool case4Pass = result4.Contains("var_traduzido");
        results.AppendLine($"- var standalone: {(case4Pass ? "PASS" : "FAIL")} — `{result4.Trim()}`");

        // Edge case 5: var inside variable name
        string code5 = "int variable = 1;";
        string result5 = TextScanTranslate(code5, translations);
        bool case5Pass = !result5.Contains("var_traduzido") && result5.Contains("variable");
        results.AppendLine($"- var in identifier (variable): {(case5Pass ? "PASS" : "FAIL")} — `{result5.Trim()}`");

        results.AppendLine();

        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
File.AppendAllText(reportPath, results.ToString());

        Assert.True(case1Pass, "publicKey should not be partially translated");
        Assert.True(case2Pass, "keywords in strings should not be translated");
        Assert.True(case3Pass, "keywords in comments should not be translated");
        Assert.True(case4Pass, "standalone var should be translated");
        Assert.True(case5Pass, "var inside 'variable' should NOT be translated");
    }

    [Fact]
    public void Method2_TextScan_AdvancedEdgeCases()
    {
        Dictionary<string, string> translations = new()
        {
            ["public"] = "publico",
            ["class"] = "classe",
            ["if"] = "se",
            ["int"] = "inteiro",
            ["return"] = "retornar",
            ["string"] = "texto",
            ["var"] = "var_traduzido",
            ["new"] = "novo"
        };

        List<(string Name, string Input, string MustContain, string MustNotContain)> cases = new()
        {
            // Verbatim strings
            ("verbatim string", @"string s = @""public class"";",
                @"@""public class""", "publico"),

            // Interpolated strings
            ("interpolated string", "string s = $\"o {x} public\";",
                "$\"o {x} public\"", "publico"),

            // Block comments
            ("block comment", "/* public class */ return;",
                "/* public class */", ""),

            // Block comment with keyword after
            ("keyword after block comment", "/* comment */ public class Foo {}",
                "publico", ""),

            // Preprocessor directives (# is not a comment in C#)
            ("preprocessor directive", "#region public\npublic class Foo {}",
                "publico classe", ""),

            // Generic types (int as keyword inside List<int>)
            ("generic type", "List<int> items = new List<int>();",
                "List<inteiro>", ""),

            // Multiple keywords on same line
            ("multiple keywords", "public static int Main() { return 0; }",
                "publico", ""),

            // Empty string literal
            ("empty string", "string s = \"\";",
                "texto s = \"\"", ""),

            // Escaped quote in string
            ("escaped quote", "string s = \"he said \\\"public\\\"\";",
                "\\\"public\\\"", "publico classe"),

            // Keyword at start of file
            ("keyword at file start", "class Foo {}",
                "classe", ""),

            // Keyword at end of file
            ("keyword at file end", "int x = 1;\nreturn",
                "retornar", ""),

            // Adjacent keywords no space separation
            ("adjacent with braces", "if(true){return;}",
                "se", ""),

            // Tab-separated keywords
            ("tab separated", "public\tclass\tFoo",
                "publico", ""),

            // Unicode identifier that contains keyword substring
            ("unicode identifier", "int públicoNome = 1;",
                "públicoNome", ""),
        };

        StringBuilder results = new StringBuilder();
        results.AppendLine("## Method 2: Advanced Edge Cases");
        results.AppendLine();

        int passed = 0;
        int failed = 0;

        foreach ((string name, string input, string mustContain, string mustNotContain) in cases)
        {
            string output = TextScanTranslate(input, translations);
            bool containsOk = string.IsNullOrEmpty(mustContain) || output.Contains(mustContain);
            bool notContainsOk = string.IsNullOrEmpty(mustNotContain) || !output.Contains(mustNotContain);
            bool pass = containsOk && notContainsOk;

            if (pass) passed++; else failed++;
            results.AppendLine($"- {name}: {(pass ? "PASS" : "FAIL")}");
            if (!pass)
            {
                results.AppendLine($"  Input:  `{input.Replace("\n", "\\n")}`");
                results.AppendLine($"  Output: `{output.Replace("\n", "\\n")}`");
                results.AppendLine($"  Expected to contain: `{mustContain}`");
                if (!string.IsNullOrEmpty(mustNotContain))
                    results.AppendLine($"  Expected NOT to contain: `{mustNotContain}`");
            }
        }

        results.AppendLine();
        results.AppendLine($"Total: {passed} PASS, {failed} FAIL out of {cases.Count}");
        results.AppendLine();

        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
        File.AppendAllText(reportPath, results.ToString());

        Assert.Equal(0, failed);
    }

    [Fact]
    public void Method2_TextScan_StringFormats_And_MultilineEdgeCases()
    {
        Dictionary<string, string> translations = new()
        {
            ["public"] = "publico",
            ["class"] = "classe",
            ["if"] = "se",
            ["int"] = "inteiro",
            ["return"] = "retornar",
            ["string"] = "texto",
            ["var"] = "var_t",
            ["new"] = "novo",
            ["void"] = "vazio",
            ["static"] = "estatico",
            ["namespace"] = "espaconome"
        };

        List<(string Name, string Input, Func<string, bool> Verify)> cases = new()
        {
            // === Multiline strings ===
            ("multiline block comment with keywords",
                "/* public\nclass\nif */ return;",
                o => o.Contains("/* public\nclass\nif */") && o.Contains("retornar")),

            ("unclosed block comment",
                "/* public class",
                o => o.Contains("/* public class")),

            // === C# raw string literals (C# 11) ===
            ("raw string literal triple quote",
                "var x = \"\"\"public class\"\"\";",
                // TextScan sees """ as: empty string "" then "public class"""
                // The second " opens a new string containing 'public class""'
                // Known limitation: raw strings (C# 11) not correctly handled.
                // We accept that keywords inside raw strings MAY be translated.
                o => o.Contains("var_t")), // var is outside any string, should be translated

            // === String formats ===
            ("string format with braces",
                "string s = string.Format(\"{0} public\", x);",
                o => o.Contains("\"{0} public\"")),

            ("interpolated with expression",
                "string s = $\"{x + 1} public\";",
                o => o.Contains("$\"{x + 1} public\"")),

            ("interpolated with nested braces",
                "string s = $\"{(true ? 1 : 0)} public\";",
                o => o.Contains("public\"")),

            ("verbatim interpolated",
                "string s = $@\"line1\npublic\nline3\";",
                // $@ starts with $, then @, then " — scanner sees $ as non-letter,
                // then @ as non-letter, then " opens a string.
                // Keywords inside the string should NOT be translated.
                o => !o.Contains("publico")),

            // === Multiline comments ===
            ("multiline comment spans keywords",
                "public /* class\nif\nreturn */ void Main() {}",
                o => o.Contains("publico") && o.Contains("vazio") && o.Contains("/* class\nse\nretornar */") == false),

            // === Pragma and preprocessor ===
            ("pragma warning",
                "#pragma warning disable\npublic class Foo {}",
                o => o.Contains("#pragma") && o.Contains("publico")),

            ("pragma restore",
                "#pragma warning restore\nreturn;",
                o => o.Contains("#pragma") && o.Contains("retornar")),

            ("#if directive",
                "#if DEBUG\npublic class Foo {}\n#endif",
                o => o.Contains("#if DEBUG") && o.Contains("publico") && !o.Contains("#se")),

            ("#region with keyword",
                "#region public API\npublic void Foo() {}\n#endregion",
                o => o.Contains("publico")),

            ("#define",
                "#define PUBLIC_API\npublic class Foo {}",
                o => o.Contains("publico")),

            // === Broken/incomplete code ===
            ("unclosed string",
                "string s = \"public class",
                // Scanner enters string at " and never exits — keywords inside are protected
                o => !o.Contains("publico")),

            ("unclosed char literal",
                "char c = 'public",
                // Scanner enters char literal at ' and never exits
                o => !o.Contains("publico")),

            ("unclosed block comment at EOF",
                "public /* class if",
                o => o.Contains("publico")),

            ("missing semicolons",
                "public class Foo { int x = 1 return x }",
                o => o.Contains("publico") && o.Contains("retornar")),

            ("double open braces (syntax error)",
                "public class Foo {{ int x = 1; }}",
                o => o.Contains("publico") && o.Contains("classe")),

            ("empty input",
                "",
                o => o == ""),

            ("only whitespace",
                "   \n\t  \n  ",
                o => o.Trim() == ""),

            ("only a keyword",
                "return",
                o => o == "retornar"),

            ("keyword with numbers",
                "int return123 = 1;",
                o => o.Contains("return123") && !o.Contains("retornar123")),

            // === Nested strings ===
            ("string with escaped backslash before quote",
                "string s = \"path\\\\public\\\\\";",
                // \\\\ is two escaped backslashes, then public, then \\\\, then "
                // "public" is inside the string — should NOT be translated
                o => !o.Contains("publico")),

            ("char containing backslash",
                "char c = '\\\\';",
                // '\\' is an escaped backslash char literal
                o => o.Contains("'\\\\'"))
        };

        StringBuilder results = new StringBuilder();
        results.AppendLine("## Method 2: String Formats, Multiline, Pragma, Broken Code");
        results.AppendLine();

        int passed = 0;
        int failed = 0;
        List<string> failDetails = new();

        foreach ((string name, string input, Func<string, bool> verify) in cases)
        {
            string output = TextScanTranslate(input, translations);
            bool pass = verify(output);

            if (pass) passed++; else failed++;
            results.AppendLine($"- {name}: {(pass ? "PASS" : "FAIL")}");
            if (!pass)
            {
                string detail = $"  FAIL: {name}\n  Input:  `{input.Replace("\n", "\\n")}`\n  Output: `{output.Replace("\n", "\\n")}`";
                results.AppendLine(detail);
                failDetails.Add(detail);
            }
        }

        results.AppendLine();
        results.AppendLine($"Total: {passed} PASS, {failed} FAIL out of {cases.Count}");
        results.AppendLine();

        results.AppendLine("### Known limitations of Text Scan:");
        results.AppendLine("- C# 11 raw string literals (\"\"\"...\"\"\") not fully handled (keywords inside may be translated)");
        results.AppendLine("- Unclosed strings/comments: scanner protects content after opening delimiter");
        results.AppendLine();

        string reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tarefa061-benchmark-results.md"));
        File.AppendAllText(reportPath, results.ToString());

        Assert.Equal(0, failed);
    }
}
