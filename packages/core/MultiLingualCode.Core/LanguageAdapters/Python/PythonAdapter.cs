using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using System.Text;

namespace MultiLingualCode.Core.LanguageAdapters.Python;

/// <summary>
/// Language adapter for Python that uses a persistent Python subprocess for tokenization.
/// Implements IDisposable to ensure the Python subprocess is cleaned up.
/// </summary>
public class PythonAdapter : ILanguageAdapter, IDisposable
{
    public readonly PythonTokenizerService Tokenizer = new();

    /// <summary>
    /// Disposes the underlying Python tokenizer subprocess.
    /// </summary>
    public void Dispose()
    {
        Tokenizer.Dispose();
    }

    /// <summary>
    /// The name of the programming language handled by this adapter.
    /// </summary>
    public string LanguageName => "Python";

    /// <summary>
    /// The file extensions associated with Python source files.
    /// </summary>
    public string[] FileExtensions => [".py"];

    /// <summary>
    /// The version of this adapter implementation.
    /// </summary>
    public string Version => "1.0.0";

    /// <summary>
    /// Parses Python source code into an AST with keyword, identifier, and literal nodes.
    /// </summary>
    public ASTNode Parse(string sourceCode)
    {
        StatementNode root = new StatementNode
        {
            StatementKind = "Module",
            RawText = sourceCode,
            StartPosition = 0,
            EndPosition = sourceCode.Length,
            StartLine = 0,
            EndLine = sourceCode.Split('\n').Length - 1
        };

        OperationResultGeneric<List<PythonToken>> result = Tokenizer.Tokenize(sourceCode);
        if (!result.IsSuccess)
        {
            return root;
        }

        int[] lineOffsets = ComputeLineOffsets(sourceCode);

        foreach (PythonToken token in result.Value)
        {
            int startPos = GetAbsoluteOffset(lineOffsets, token.StartLine, token.StartCol);
            int endPos = GetAbsoluteOffset(lineOffsets, token.EndLine, token.EndCol);
            int startLine0 = token.StartLine - 1;
            int endLine0 = token.EndLine - 1;

            if (token.IsKeyword)
            {
                int keywordId = PythonKeywordMap.GetId(token.String);
                if (keywordId >= 0)
                {
                    root.Children.Add(new KeywordNode
                    {
                        KeywordId = keywordId,
                        Text = token.String,
                        StartPosition = startPos,
                        EndPosition = endPos,
                        StartLine = startLine0,
                        EndLine = endLine0,
                        Parent = root
                    });
                }
            }
            else if (token.TypeName == "NAME")
            {
                root.Children.Add(new IdentifierNode
                {
                    Name = token.String,
                    IsTranslatable = true,
                    StartPosition = startPos,
                    EndPosition = endPos,
                    StartLine = startLine0,
                    EndLine = endLine0,
                    Parent = root
                });
            }
            else if (token.TypeName == "STRING")
            {
                string value = ExtractStringContent(token.String);
                root.Children.Add(new LiteralNode
                {
                    Value = value,
                    Type = LiteralType.String,
                    IsTranslatable = true,
                    StartPosition = startPos,
                    EndPosition = endPos,
                    StartLine = startLine0,
                    EndLine = endLine0,
                    Parent = root
                });
            }
            else if (token.TypeName == "NUMBER")
            {
                root.Children.Add(new LiteralNode
                {
                    Value = token.String,
                    Type = LiteralType.Number,
                    IsTranslatable = false,
                    StartPosition = startPos,
                    EndPosition = endPos,
                    StartLine = startLine0,
                    EndLine = endLine0,
                    Parent = root
                });
            }
        }

        return root;
    }

    /// <summary>
    /// Generates source code from an AST by applying text replacements to the original source.
    /// </summary>
    public string Generate(ASTNode ast)
    {
        StatementNode root = (StatementNode)ast;
        string originalSource = root.RawText;

        if (string.IsNullOrEmpty(originalSource) || ast.Children.Count == 0)
        {
            return originalSource;
        }

        List<(int Start, int End, string NewText)> replacements = new();
        CSharpAdapter.CollectReplacements(ast, replacements);

        if (replacements.Count == 0)
        {
            return originalSource;
        }

        replacements.Sort((a, b) => b.Start.CompareTo(a.Start));

        string result = originalSource;
        foreach ((int start, int end, string newText) in replacements)
        {
            if (start >= 0 && end <= result.Length && start < end)
            {
                result = string.Concat(result.AsSpan(0, start), newText, result.AsSpan(end));
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a mapping of Python keyword text to their numeric identifiers.
    /// </summary>
    public Dictionary<string, int> GetKeywordMap() => PythonKeywordMap.GetMap();

    /// <summary>
    /// Converts translated keywords in Python code back to their original keyword text.
    /// Skips comments (#) and all string variants to avoid false positives.
    /// </summary>
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword)
    {
        StringBuilder result = new StringBuilder(translatedCode.Length);
        int i = 0;

        while (i < translatedCode.Length)
        {
            // Skip comments: # to end of line
            if (translatedCode[i] == '#')
            {
                int start = i;
                while (i < translatedCode.Length && translatedCode[i] != '\n')
                {
                    i++;
                }
                result.Append(translatedCode, start, i - start);
                continue;
            }

            // Skip strings: detect prefix + quote
            if (IsStringStart(translatedCode, i, out int quoteEnd, out string quoteChar))
            {
                int start = i;
                i = quoteEnd; // past the opening quote(s)
                bool isTriple = quoteChar.Length == 3;

                while (i < translatedCode.Length)
                {
                    if (isTriple)
                    {
                        if (i + 2 < translatedCode.Length
                            && translatedCode[i] == quoteChar[0]
                            && translatedCode[i + 1] == quoteChar[0]
                            && translatedCode[i + 2] == quoteChar[0])
                        {
                            i += 3;
                            break;
                        }
                    }
                    else
                    {
                        if (translatedCode[i] == quoteChar[0])
                        {
                            i++;
                            break;
                        }
                    }

                    if (translatedCode[i] == '\\' && i + 1 < translatedCode.Length)
                    {
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }

                result.Append(translatedCode, start, i - start);
                continue;
            }

            // Word tokens: check if translated keyword
            if (char.IsLetter(translatedCode[i]) || translatedCode[i] == '_')
            {
                int wordStart = i;
                while (i < translatedCode.Length && (char.IsLetterOrDigit(translatedCode[i]) || translatedCode[i] == '_'))
                {
                    i++;
                }

                string word = translatedCode.Substring(wordStart, i - wordStart);
                int keywordId = lookupTranslatedKeyword(word);

                if (keywordId >= 0)
                {
                    string originalKeyword = PythonKeywordMap.GetText(keywordId);
                    if (!string.IsNullOrEmpty(originalKeyword))
                    {
                        result.Append(originalKeyword);
                        continue;
                    }
                }

                result.Append(word);
                continue;
            }

            result.Append(translatedCode[i]);
            i++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Validates Python source code by attempting to tokenize it.
    /// Note: this validates lexical structure only, not full syntax.
    /// </summary>
    public ValidationResult ValidateSyntax(string sourceCode)
    {
        OperationResultGeneric<List<PythonToken>> result = Tokenizer.Tokenize(sourceCode);

        if (result.IsSuccess)
        {
            return new ValidationResult { IsValid = true, Diagnostics = new List<Diagnostic>() };
        }

        return new ValidationResult
        {
            IsValid = false,
            Diagnostics = new List<Diagnostic>
            {
                new Diagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = result.ErrorMessage,
                    Line = 0,
                    Column = 0
                }
            }
        };
    }

    /// <summary>
    /// Extracts all distinct user-defined identifiers from Python source code.
    /// </summary>
    public List<string> ExtractIdentifiers(string sourceCode)
    {
        OperationResultGeneric<List<PythonToken>> result = Tokenizer.Tokenize(sourceCode);
        if (!result.IsSuccess)
        {
            return new List<string>();
        }

        return result.Value
            .Where(t => t.TypeName == "NAME" && !t.IsKeyword)
            .Select(t => t.String)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Extracts all trailing comments (# comments) from Python source code.
    /// </summary>
    public List<TrailingComment> ExtractTrailingComments(string sourceCode)
    {
        List<TrailingComment> comments = new();

        if (string.IsNullOrEmpty(sourceCode))
        {
            return comments;
        }

        OperationResultGeneric<List<PythonToken>> result = Tokenizer.Tokenize(sourceCode);
        if (!result.IsSuccess)
        {
            return comments;
        }

        foreach (PythonToken token in result.Value)
        {
            if (token.TypeName == "COMMENT")
            {
                string text = token.String;
                // Remove # prefix and optional leading space
                if (text.StartsWith("# "))
                {
                    text = text.Substring(2);
                }
                else if (text.StartsWith("#"))
                {
                    text = text.Substring(1);
                }

                comments.Add(new TrailingComment
                {
                    Text = text,
                    Line = token.StartLine - 1 // Convert to 0-based
                });
            }
        }

        return comments;
    }

    /// <summary>
    /// Gets the names of all identifiers on a specific line.
    /// </summary>
    public List<string> GetIdentifierNamesOnLine(string sourceCode, int line)
    {
        OperationResultGeneric<List<PythonToken>> result = Tokenizer.Tokenize(sourceCode);
        if (!result.IsSuccess)
        {
            return new List<string>();
        }

        int line1 = line + 1; // Convert 0-based to 1-based
        return result.Value
            .Where(t => t.TypeName == "NAME" && !t.IsKeyword && t.StartLine == line1)
            .Select(t => t.String)
            .ToList();
    }

    /// <summary>
    /// Gets the text of the first string literal on a specific line.
    /// </summary>
    public string GetFirstStringLiteralOnLine(string sourceCode, int line)
    {
        OperationResultGeneric<List<PythonToken>> result = Tokenizer.Tokenize(sourceCode);
        if (!result.IsSuccess)
        {
            return "";
        }

        int line1 = line + 1;
        List<PythonToken> matches = result.Value
            .Where(t => t.TypeName == "STRING" && t.StartLine == line1)
            .ToList();

        if (matches.Count == 0)
        {
            return "";
        }

        return ExtractStringContent(matches[0].String);
    }

    /// <summary>
    /// Gets the line range of the method (def) containing the specified line.
    /// Uses indentation-based detection for Python's block structure.
    /// </summary>
    public (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line)
    {
        string[] lines = sourceCode.Split('\n');
        int targetLine0 = line;

        // Find the def keyword above the target line with indentation <= target line
        int defLine = -1;
        int defIndent = -1;

        for (int i = targetLine0; i >= 0; i--)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("def ") || trimmed.StartsWith("async def "))
            {
                int indent = lines[i].Length - lines[i].TrimStart().Length;
                defLine = i;
                defIndent = indent;
                break;
            }
        }

        if (defLine < 0)
        {
            return (-1, -1);
        }

        // Include decorators above the def
        int startLine = defLine;
        for (int i = defLine - 1; i >= 0; i--)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("@"))
            {
                startLine = i;
            }
            else if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }
            else
            {
                break;
            }
        }

        // Find end of method: next line with indentation <= def's indentation (non-empty)
        int endLine = lines.Length - 1;
        for (int i = defLine + 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            int indent = lines[i].Length - lines[i].TrimStart().Length;
            if (indent <= defIndent)
            {
                endLine = i - 1;
                break;
            }
        }

        return (startLine, endLine);
    }

    /// <summary>
    /// Computes the character offset of the start of each line in the source code.
    /// Index 0 is unused (Python lines are 1-based). lineOffsets[1] = offset of line 1, etc.
    /// </summary>
    public static int[] ComputeLineOffsets(string source)
    {
        List<int> offsets = new() { 0, 0 }; // index 0 unused, line 1 starts at offset 0
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == '\n')
            {
                offsets.Add(i + 1);
            }
        }
        return offsets.ToArray();
    }

    /// <summary>
    /// Converts a (1-based line, 0-based col) position to an absolute character offset.
    /// </summary>
    public static int GetAbsoluteOffset(int[] lineOffsets, int line, int col)
    {
        if (line >= 1 && line < lineOffsets.Length)
        {
            return lineOffsets[line] + col;
        }
        return 0;
    }

    /// <summary>
    /// Extracts the content of a Python string literal, removing quotes and prefixes.
    /// </summary>
    public static string ExtractStringContent(string raw)
    {
        string s = raw;

        // Remove prefixes: f, r, b, u, rb, br, fr, rf (case-insensitive)
        while (s.Length > 0 && "fFrRbBuU".Contains(s[0]))
        {
            s = s.Substring(1);
        }

        // Remove triple quotes or single quotes
        if (s.StartsWith("\"\"\"") && s.EndsWith("\"\"\"") && s.Length >= 6)
        {
            return s.Substring(3, s.Length - 6);
        }
        if (s.StartsWith("'''") && s.EndsWith("'''") && s.Length >= 6)
        {
            return s.Substring(3, s.Length - 6);
        }
        if (s.StartsWith("\"") && s.EndsWith("\"") && s.Length >= 2)
        {
            return s.Substring(1, s.Length - 2);
        }
        if (s.StartsWith("'") && s.EndsWith("'") && s.Length >= 2)
        {
            return s.Substring(1, s.Length - 2);
        }

        return s;
    }

    /// <summary>
    /// Detects if position i in the code is the start of a Python string literal,
    /// handling all prefix combinations (r, b, f, u, rb, br, fr, rf) and triple quotes.
    /// </summary>
    /// <param name="code">The source code.</param>
    /// <param name="i">Current position in the code.</param>
    /// <param name="quoteEnd">Set to the position right after the opening quote(s).</param>
    /// <param name="quoteChar">Set to the quote string: "\"", "'", "\"\"\"", or "'''".</param>
    /// <returns>True if a string starts at position i.</returns>
    public static bool IsStringStart(string code, int i, out int quoteEnd, out string quoteChar)
    {
        quoteEnd = i;
        quoteChar = "";

        int prefixStart = i;

        // Skip known string prefixes
        while (i < code.Length && "fFrRbBuU".Contains(code[i]))
        {
            i++;
        }

        if (i >= code.Length || (code[i] != '"' && code[i] != '\''))
        {
            // Not a string — but only reset if we consumed at least one prefix char
            if (i > prefixStart)
            {
                quoteEnd = prefixStart;
            }
            return false;
        }

        char q = code[i];

        // Triple quote?
        if (i + 2 < code.Length && code[i + 1] == q && code[i + 2] == q)
        {
            quoteChar = new string(q, 3);
            quoteEnd = i + 3;
            return true;
        }

        quoteChar = q.ToString();
        quoteEnd = i + 1;
        return true;
    }
}
