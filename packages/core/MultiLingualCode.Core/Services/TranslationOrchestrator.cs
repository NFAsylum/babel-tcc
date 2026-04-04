using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Orchestrates end-to-end code translation between programming languages and natural languages.
/// </summary>
public class TranslationOrchestrator
{
    /// <summary>
    /// The language registry used to resolve adapters for file extensions.
    /// </summary>
    public required LanguageRegistry Registry { get; init; }

    /// <summary>
    /// The natural language provider used for keyword and identifier translation.
    /// </summary>
    public required INaturalLanguageProvider Provider { get; init; }

    /// <summary>
    /// The identifier mapper service used for persisting and retrieving identifier translations.
    /// </summary>
    public required IdentifierMapper IdentifierMapperService { get; init; }

    /// <summary>
    /// Scoped translations for parameter names, limited to specific method line ranges.
    /// Each entry contains (Name, Translation, StartLine, EndLine).
    /// </summary>
    public List<(string Name, string Translation, int StartLine, int EndLine)> ScopedTranslations = new();

    /// <summary>
    /// Creates a new <see cref="TranslationOrchestrator"/> with the specified dependencies.
    /// </summary>
    /// <param name="registry">The language registry for resolving adapters.</param>
    /// <param name="provider">The natural language provider for translations.</param>
    /// <param name="identifierMapper">The identifier mapper for identifier persistence.</param>
    /// <returns>An operation result containing the created orchestrator, or a failure if any parameter is null.</returns>
    public static OperationResultGeneric<TranslationOrchestrator> Create(
        LanguageRegistry registry,
        INaturalLanguageProvider provider,
        IdentifierMapper identifierMapper)
    {
        if (registry is null)
        {
            return OperationResultGeneric<TranslationOrchestrator>.Fail("Registry cannot be null.");
        }

        if (provider is null)
        {
            return OperationResultGeneric<TranslationOrchestrator>.Fail("Provider cannot be null.");
        }

        if (identifierMapper is null)
        {
            return OperationResultGeneric<TranslationOrchestrator>.Fail("IdentifierMapper cannot be null.");
        }

        return OperationResultGeneric<TranslationOrchestrator>.Ok(
            new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = identifierMapper });
    }

    /// <summary>
    /// Translates source code from a programming language into a natural language representation.
    /// </summary>
    /// <param name="sourceCode">The original source code to translate.</param>
    /// <param name="fileExtension">The file extension identifying the programming language.</param>
    /// <param name="targetLanguage">The target natural language code.</param>
    /// <returns>An operation result containing the translated source code, or a failure on error.</returns>
    public async Task<OperationResultGeneric<string>> TranslateToNaturalLanguageAsync(
        string sourceCode, string fileExtension, string targetLanguage)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = Registry.GetAdapter(fileExtension);
        if (!adapterResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(adapterResult.ErrorMessage);
        }

        ILanguageAdapter adapter = adapterResult.Value;

        OperationResult loadResult = await Provider.LoadTranslationTableAsync(adapter.LanguageName);
        if (!loadResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(loadResult.ErrorMessage);
        }

        ApplyTraduAnnotations(sourceCode, targetLanguage, adapter);

        ASTNode ast = adapter.Parse(sourceCode);
        ASTNode translatedAst = ast.Clone();

        TranslateAstForward(translatedAst, targetLanguage);

        string result = adapter.Generate(translatedAst);
        return OperationResultGeneric<string>.Ok(result);
    }

    /// <summary>
    /// Translates natural-language code back into the original programming language.
    /// </summary>
    /// <param name="translatedCode">The natural-language translated code to reverse.</param>
    /// <param name="fileExtension">The file extension identifying the programming language.</param>
    /// <param name="sourceLanguage">The source natural language code the code was translated to.</param>
    /// <returns>An operation result containing the restored original source code, or a failure on error.</returns>
    public async Task<OperationResultGeneric<string>> TranslateFromNaturalLanguageAsync(
        string translatedCode, string fileExtension, string sourceLanguage)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = Registry.GetAdapter(fileExtension);
        if (!adapterResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(adapterResult.ErrorMessage);
        }

        ILanguageAdapter adapter = adapterResult.Value;

        OperationResult loadResult = await Provider.LoadTranslationTableAsync(adapter.LanguageName);
        if (!loadResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(loadResult.ErrorMessage);
        }

        string preSubstituted = adapter.ReverseSubstituteKeywords(
            translatedCode, Provider.ReverseTranslateKeyword);

        ApplyReverseTraduAnnotations(preSubstituted, sourceLanguage, adapter);

        ASTNode ast = adapter.Parse(preSubstituted);
        ASTNode originalAst = ast.Clone();

        TranslateAstReverse(originalAst, sourceLanguage);

        string result = adapter.Generate(originalAst);
        return OperationResultGeneric<string>.Ok(result);
    }

    /// <summary>
    /// Recursively translates AST nodes from programming language to natural language (forward translation).
    /// </summary>
    /// <param name="node">The AST node to translate.</param>
    /// <param name="targetLanguage">The target natural language code.</param>
    public void TranslateAstForward(ASTNode node, string targetLanguage)
    {
        switch (node)
        {
            case KeywordNode keyword:
                OperationResultGeneric<string> translatedResult = Provider.TranslateKeyword(keyword.KeywordId);
                if (translatedResult.IsSuccess)
                {
                    keyword.Text = translatedResult.Value;
                }
                break;

            case IdentifierNode identifier when identifier.IsTranslatable:
                string scopedForward = FindScopedTranslation(identifier.Name, identifier.StartLine);
                if (scopedForward != null)
                {
                    identifier.TranslatedName = scopedForward;
                    identifier.Name = scopedForward;
                }
                else
                {
                    OperationResultGeneric<string> translatedIdResult = IdentifierMapperService.GetTranslation(identifier.Name, targetLanguage);
                    if (translatedIdResult.IsSuccess)
                    {
                        identifier.TranslatedName = translatedIdResult.Value;
                        identifier.Name = translatedIdResult.Value;
                    }
                }
                break;

            case LiteralNode literal when literal.IsTranslatable:
                string literalText = $"{literal.Value}";
                OperationResultGeneric<string> translatedLitResult = IdentifierMapperService.GetLiteralTranslation(literalText, targetLanguage);
                if (translatedLitResult.IsSuccess)
                {
                    literal.Value = translatedLitResult.Value;
                }
                break;
        }

        foreach (ASTNode child in node.Children)
        {
            TranslateAstForward(child, targetLanguage);
        }
    }

    /// <summary>
    /// Recursively translates AST nodes from natural language back to the original programming language (reverse translation).
    /// </summary>
    /// <param name="node">The AST node to reverse-translate.</param>
    /// <param name="sourceLanguage">The natural language code the AST was translated to.</param>
    public void TranslateAstReverse(ASTNode node, string sourceLanguage)
    {
        switch (node)
        {
            case KeywordNode keyword:
                int keywordId = Provider.ReverseTranslateKeyword(keyword.Text);
                if (keywordId >= 0)
                {
                    OperationResultGeneric<string> originalKeywordResult = Provider.GetOriginalKeyword(keywordId);
                    if (originalKeywordResult.IsSuccess)
                    {
                        keyword.Text = originalKeywordResult.Value;
                        keyword.KeywordId = keywordId;
                    }
                }
                break;

            case IdentifierNode identifier:
                string scopedReverse = FindScopedTranslation(identifier.Name, identifier.StartLine);
                if (scopedReverse != null)
                {
                    identifier.Name = scopedReverse;
                    identifier.TranslatedName = "";
                }
                else
                {
                    OperationResultGeneric<string> originalIdResult = IdentifierMapperService.GetOriginal(identifier.Name, sourceLanguage);
                    if (originalIdResult.IsSuccess)
                    {
                        identifier.Name = originalIdResult.Value;
                        identifier.TranslatedName = "";
                    }
                }
                break;

            case LiteralNode literal when literal.IsTranslatable:
                string translatedLiteralText = $"{literal.Value}";
                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in IdentifierMapperService.Data.Literals)
                {
                    if (kvp.Value.ContainsKey(sourceLanguage)
                        && string.Equals(kvp.Value[sourceLanguage], translatedLiteralText, StringComparison.Ordinal))
                    {
                        literal.Value = kvp.Key;
                        break;
                    }
                }
                break;
        }

        foreach (ASTNode child in node.Children)
        {
            TranslateAstReverse(child, sourceLanguage);
        }
    }

    /// <summary>
    /// Extracts tradu annotations from source code and applies them as identifier and literal translations.
    /// Parameter mappings are scoped to their method range to avoid global name collisions.
    /// </summary>
    /// <param name="sourceCode">The source code containing tradu annotations.</param>
    /// <param name="targetLanguage">The target natural language code for the translations.</param>
    /// <param name="adapter">The language adapter used for comment extraction and code analysis.</param>
    public void ApplyTraduAnnotations(string sourceCode, string targetLanguage, ILanguageAdapter adapter)
    {
        ScopedTranslations.Clear();
        IdentifierMapperService.Data.Identifiers.Clear();
        IdentifierMapperService.Data.Literals.Clear();

        TraduAnnotationParser parser = new TraduAnnotationParser();
        List<TraduAnnotation> annotations = parser.ExtractAnnotations(sourceCode, adapter);

        foreach (TraduAnnotation annotation in annotations)
        {
            string lang = !string.IsNullOrEmpty(annotation.TargetLanguage)
                ? annotation.TargetLanguage
                : targetLanguage;

            if (annotation.IsLiteralAnnotation)
            {
                IdentifierMapperService.SetLiteralTranslation(
                    annotation.OriginalLiteral, lang, annotation.TranslatedLiteral);
            }
            else
            {
                IdentifierMapperService.SetTranslation(
                    annotation.OriginalIdentifier, lang, annotation.TranslatedIdentifier);

                foreach (TraduParameterMapping paramMapping in annotation.ParameterMappings)
                {
                    if (lang == targetLanguage && annotation.MethodStartLine >= 0)
                    {
                        ScopedTranslations.Add((
                            paramMapping.OriginalParameterName,
                            paramMapping.TranslatedParameterName,
                            annotation.MethodStartLine,
                            annotation.MethodEndLine));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extracts tradu annotations from translated code and applies reverse parameter mappings as scoped translations.
    /// </summary>
    /// <param name="code">The code after keyword substitution.</param>
    /// <param name="sourceLanguage">The source natural language code.</param>
    /// <param name="adapter">The language adapter used for comment extraction and code analysis.</param>
    public void ApplyReverseTraduAnnotations(string code, string sourceLanguage, ILanguageAdapter adapter)
    {
        ScopedTranslations.Clear();

        TraduAnnotationParser parser = new TraduAnnotationParser();
        List<TraduAnnotation> annotations = parser.ExtractAnnotations(code, adapter);

        foreach (TraduAnnotation annotation in annotations)
        {
            string lang = !string.IsNullOrEmpty(annotation.TargetLanguage)
                ? annotation.TargetLanguage
                : sourceLanguage;

            if (lang != sourceLanguage)
            {
                continue;
            }

            if (!annotation.IsLiteralAnnotation && annotation.ParameterMappings.Count > 0 && annotation.MethodStartLine >= 0)
            {
                foreach (TraduParameterMapping paramMapping in annotation.ParameterMappings)
                {
                    ScopedTranslations.Add((
                        paramMapping.TranslatedParameterName,
                        paramMapping.OriginalParameterName,
                        annotation.MethodStartLine,
                        annotation.MethodEndLine));
                }
            }
        }
    }

    /// <summary>
    /// Finds a scoped translation for a given name at a specific line number.
    /// </summary>
    /// <param name="name">The identifier name to look up.</param>
    /// <param name="line">The line number where the identifier appears.</param>
    /// <returns>The scoped translation if found, or null if not in scope.</returns>
    public string FindScopedTranslation(string name, int line)
    {
        foreach ((string scopedName, string translation, int startLine, int endLine) in ScopedTranslations)
        {
            if (scopedName == name && line >= startLine && line <= endLine)
            {
                return translation;
            }
        }

        return null!;
    }

    /// <summary>
    /// Applies edits from translated code back to the original code using a 3-way diff.
    /// Lines unchanged between previousTranslated and editedTranslated are copied from the original.
    /// Lines that changed are reverse-translated token-by-token.
    /// This avoids the ambiguity of ReverseSubstituteKeywords where identifiers can collide
    /// with translated keywords (e.g., variable "e" vs translated keyword "e" for "and").
    /// </summary>
    /// <param name="originalCode">The original source code on disk.</param>
    /// <param name="previousTranslatedCode">The translated code before user edits (from forward translation cache).</param>
    /// <param name="editedTranslatedCode">The translated code after user edits (what the user saved).</param>
    /// <param name="fileExtension">The file extension identifying the programming language.</param>
    /// <param name="sourceLanguage">The natural language code the code was translated to.</param>
    /// <returns>The original code with user edits applied.</returns>
    public async Task<OperationResultGeneric<string>> ApplyTranslatedEditsAsync(
        string originalCode, string previousTranslatedCode, string editedTranslatedCode,
        string fileExtension, string sourceLanguage)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = Registry.GetAdapter(fileExtension);
        if (!adapterResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(adapterResult.ErrorMessage);
        }

        ILanguageAdapter adapter = adapterResult.Value;

        OperationResult loadResult = await Provider.LoadTranslationTableAsync(adapter.LanguageName);
        if (!loadResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(loadResult.ErrorMessage);
        }

        string[] originalLines = originalCode.Split('\n');
        string[] previousLines = previousTranslatedCode.Split('\n');
        string[] editedLines = editedTranslatedCode.Split('\n');

        // LCS-based diff: find common lines between previous and edited,
        // preserving order. This handles insertions and deletions in the middle
        // without desaligning subsequent lines.
        List<DiffOp> operations = ComputeDiff(previousLines, editedLines);
        List<string> resultLines = new();

        int originalIdx = 0;

        foreach (DiffOp op in operations)
        {
            switch (op.Type)
            {
                case DiffOpType.Equal:
                    // Line unchanged — copy from original
                    if (originalIdx < originalLines.Length)
                    {
                        resultLines.Add(originalLines[originalIdx]);
                    }
                    else
                    {
                        resultLines.Add(ReverseTranslateLine(op.EditedLine, adapter));
                    }
                    originalIdx++;
                    break;

                case DiffOpType.Modified:
                    // Line modified — token-level diff
                    string origLine = originalIdx < originalLines.Length ? originalLines[originalIdx] : "";
                    resultLines.Add(ReverseTranslateLineWithDiff(origLine, op.PreviousLine, op.EditedLine));
                    originalIdx++;
                    break;

                case DiffOpType.Insert:
                    // New line added by user — reverse translate
                    resultLines.Add(ReverseTranslateLine(op.EditedLine, adapter));
                    break;

                case DiffOpType.Delete:
                    // Line removed by user — skip from original
                    originalIdx++;
                    break;
            }
        }

        string result = string.Join("\n", resultLines);
        return OperationResultGeneric<string>.Ok(result);
    }

    /// <summary>
    /// Computes a diff between previous and edited line arrays using LCS (Longest Common Subsequence).
    /// Returns a list of operations: Equal, Modified, Insert, Delete.
    /// </summary>
    public static List<DiffOp> ComputeDiff(string[] previous, string[] edited)
    {
        int n = previous.Length;
        int m = edited.Length;

        // Build LCS table
        int[,] lcs = new int[n + 1, m + 1];
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                if (previous[i - 1] == edited[j - 1])
                {
                    lcs[i, j] = lcs[i - 1, j - 1] + 1;
                }
                else
                {
                    lcs[i, j] = Math.Max(lcs[i - 1, j], lcs[i, j - 1]);
                }
            }
        }

        // Backtrack to produce diff operations
        List<DiffOp> ops = new();
        int pi = n;
        int ei = m;

        while (pi > 0 || ei > 0)
        {
            if (pi > 0 && ei > 0 && previous[pi - 1] == edited[ei - 1])
            {
                ops.Add(new DiffOp { Type = DiffOpType.Equal, PreviousLine = previous[pi - 1], EditedLine = edited[ei - 1] });
                pi--;
                ei--;
            }
            else if (ei > 0 && (pi == 0 || lcs[pi, ei - 1] >= lcs[pi - 1, ei]))
            {
                ops.Add(new DiffOp { Type = DiffOpType.Insert, EditedLine = edited[ei - 1] });
                ei--;
            }
            else
            {
                ops.Add(new DiffOp { Type = DiffOpType.Delete, PreviousLine = previous[pi - 1] });
                pi--;
            }
        }

        ops.Reverse();

        // Post-process: detect Modified lines (adjacent Delete + Insert that could be a modification)
        List<DiffOp> result = new();
        int idx = 0;
        while (idx < ops.Count)
        {
            if (idx + 1 < ops.Count && ops[idx].Type == DiffOpType.Delete && ops[idx + 1].Type == DiffOpType.Insert)
            {
                result.Add(new DiffOp { Type = DiffOpType.Modified, PreviousLine = ops[idx].PreviousLine, EditedLine = ops[idx + 1].EditedLine });
                idx += 2;
            }
            else
            {
                result.Add(ops[idx]);
                idx++;
            }
        }

        return result;
    }

    /// <summary>
    /// Reverse translates a modified line using token-level diff against the previous translated line.
    /// Tokens unchanged between previous and edited are copied from the original line.
    /// Tokens that changed are reverse translated. Tokens inside strings and comments are never reverse translated.
    /// </summary>
    /// <param name="originalLine">The original source line (from disk).</param>
    /// <param name="previousTranslatedLine">The translated line before user edits.</param>
    /// <param name="editedTranslatedLine">The translated line after user edits.</param>
    /// <returns>The original line with user edits applied.</returns>
    public string ReverseTranslateLineWithDiff(string originalLine, string previousTranslatedLine, string editedTranslatedLine)
    {
        List<string> originalTokens = TokenizeLine(originalLine);
        List<string> previousTokens = TokenizeLine(previousTranslatedLine);
        List<string> editedTokens = TokenizeLine(editedTranslatedLine);

        System.Text.StringBuilder result = new();

        for (int i = 0; i < editedTokens.Count; i++)
        {
            string editedToken = editedTokens[i];

            // If this token existed in previous at the same position and didn't change,
            // copy from original (preserves identifiers that match keywords)
            if (i < previousTokens.Count && previousTokens[i] == editedToken)
            {
                if (i < originalTokens.Count)
                {
                    result.Append(originalTokens[i]);
                }
                else
                {
                    result.Append(editedToken);
                }
            }
            else
            {
                // Token changed or is new — reverse translate if it's a word
                result.Append(ReverseTranslateToken(editedToken));
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Reverse translates a single line without diff context (for new lines with no previous version).
    /// Strings and comments are preserved without reverse translation.
    /// </summary>
    public string ReverseTranslateLine(string translatedLine, ILanguageAdapter adapter)
    {
        if (string.IsNullOrEmpty(translatedLine))
        {
            return translatedLine;
        }

        List<string> tokens = TokenizeLine(translatedLine);
        System.Text.StringBuilder result = new();

        foreach (string token in tokens)
        {
            result.Append(ReverseTranslateToken(token));
        }

        return result.ToString();
    }

    /// <summary>
    /// Reverse translates a single token. Only word tokens (identifiers/keywords) are looked up.
    /// String contents, comments, operators, and whitespace are returned unchanged.
    /// </summary>
    public string ReverseTranslateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return token;
        }

        // Only reverse translate word tokens (letters/digits/underscore)
        if (!char.IsLetter(token[0]) && token[0] != '_')
        {
            return token;
        }

        int keywordId = Provider.ReverseTranslateKeyword(token);
        if (keywordId >= 0)
        {
            OperationResultGeneric<string> originalResult = Provider.GetOriginalKeyword(keywordId);
            if (originalResult.IsSuccess)
            {
                return originalResult.Value;
            }
        }

        return token;
    }

    /// <summary>
    /// Tokenizes a line into a list of tokens preserving all characters.
    /// Each token is either a word, a string literal segment, a comment, or non-word characters.
    /// For interpolated strings (f"...", $"..."), expressions inside {} are tokenized separately.
    /// Concatenating all tokens reproduces the original line exactly.
    /// </summary>
    public static List<string> TokenizeLine(string line)
    {
        List<string> tokens = new();
        int i = 0;
        TokenizeSegment(line, ref i, tokens, '\0');
        return tokens;
    }

    /// <summary>
    /// Tokenizes a segment of text, stopping at the end or at a closing delimiter.
    /// Used recursively for expressions inside interpolated string braces.
    /// </summary>
    public static void TokenizeSegment(string line, ref int i, List<string> tokens, char stopAt)
    {
        while (i < line.Length)
        {
            if (stopAt != '\0' && line[i] == stopAt)
            {
                return;
            }

            // Comment: # (Python) or // (C#) to end of line
            if (line[i] == '#' || (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/'))
            {
                tokens.Add(line.Substring(i));
                i = line.Length;
                return;
            }

            // Detect string prefix and type
            bool isInterpolated = false;
            int prefixStart = i;

            // Check for $@" or @$" (C# interpolated verbatim)
            if (i + 2 < line.Length
                && ((line[i] == '$' && line[i + 1] == '@' && line[i + 2] == '"')
                 || (line[i] == '@' && line[i + 1] == '$' && line[i + 2] == '"')))
            {
                isInterpolated = true;
                TokenizeInterpolatedString(line, ref i, tokens, '"', 2);
                continue;
            }

            // Check for $" (C# interpolated)
            if (line[i] == '$' && i + 1 < line.Length && line[i + 1] == '"')
            {
                isInterpolated = true;
                TokenizeInterpolatedString(line, ref i, tokens, '"', 1);
                continue;
            }

            // Check for @" (C# verbatim — not interpolated, atomic token)
            if (line[i] == '@' && i + 1 < line.Length && line[i + 1] == '"')
            {
                int start = i;
                i += 2;
                while (i < line.Length)
                {
                    if (line[i] == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            i += 2;
                        }
                        else
                        {
                            i++;
                            break;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
                tokens.Add(line.Substring(start, i - start));
                continue;
            }

            // Check for string prefix (f, r, b, u, etc.) followed by quote
            if ("fFrRbBuU".Contains(line[i]) && i + 1 < line.Length)
            {
                int prefixLen = 1;
                // Double prefix (rb, fr, etc.)
                if ("fFrRbBuU".Contains(line[i + 1]) && i + 2 < line.Length && (line[i + 2] == '"' || line[i + 2] == '\''))
                {
                    prefixLen = 2;
                }

                if (i + prefixLen < line.Length && (line[i + prefixLen] == '"' || line[i + prefixLen] == '\''))
                {
                    string prefix = line.Substring(i, prefixLen);
                    bool isFString = prefix.Contains('f') || prefix.Contains('F');

                    if (isFString)
                    {
                        TokenizeInterpolatedString(line, ref i, tokens, line[i + prefixLen], prefixLen);
                    }
                    else
                    {
                        // Non-interpolated prefixed string (r, b, u, rb, br) — atomic token
                        ConsumeSimpleString(line, ref i, tokens, prefixLen);
                    }
                    continue;
                }
            }

            // Plain string literal (no prefix)
            if (line[i] == '"' || line[i] == '\'')
            {
                ConsumeSimpleString(line, ref i, tokens, 0);
                continue;
            }

            // Word token (identifier or keyword)
            if (char.IsLetter(line[i]) || line[i] == '_')
            {
                int start = i;
                while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_'))
                {
                    i++;
                }
                tokens.Add(line.Substring(start, i - start));
                continue;
            }

            // Non-word single character (whitespace, operator, delimiter, brace)
            tokens.Add(line[i].ToString());
            i++;
        }
    }

    /// <summary>
    /// Consumes a simple (non-interpolated) string literal as a single atomic token.
    /// </summary>
    public static void ConsumeSimpleString(string line, ref int i, List<string> tokens, int prefixLen)
    {
        int start = i;
        i += prefixLen;
        char quote = line[i];
        i++;

        bool isTriple = i + 1 < line.Length && line[i] == quote && line[i + 1] == quote;
        if (isTriple)
        {
            i += 2;
            string tripleQuote = new string(quote, 3);
            int endIdx = line.IndexOf(tripleQuote, i);
            if (endIdx >= 0)
            {
                i = endIdx + 3;
            }
            else
            {
                i = line.Length;
            }
        }
        else
        {
            while (i < line.Length)
            {
                if (line[i] == '\\' && i + 1 < line.Length)
                {
                    i += 2;
                }
                else if (line[i] == quote)
                {
                    i++;
                    break;
                }
                else
                {
                    i++;
                }
            }
        }

        tokens.Add(line.Substring(start, i - start));
    }

    /// <summary>
    /// Tokenizes an interpolated string (f"...", $"...", $@"...").
    /// String literal segments are atomic tokens. Expressions inside {} are tokenized recursively.
    /// </summary>
    public static void TokenizeInterpolatedString(string line, ref int i, List<string> tokens, char quote, int prefixLen)
    {
        int start = i;
        i += prefixLen; // skip prefix
        i++; // skip opening quote

        while (i < line.Length)
        {
            if (line[i] == '\\' && i + 1 < line.Length)
            {
                i += 2;
            }
            else if (line[i] == '{')
            {
                // Emit the string segment up to (including) the {
                tokens.Add(line.Substring(start, i - start + 1));
                i++; // past {
                start = i;

                // Tokenize the expression inside {} recursively
                TokenizeSegment(line, ref i, tokens, '}');

                if (i < line.Length && line[i] == '}')
                {
                    tokens.Add("}");
                    i++;
                }
                start = i;
            }
            else if (line[i] == quote)
            {
                i++;
                tokens.Add(line.Substring(start, i - start));
                return;
            }
            else
            {
                i++;
            }
        }

        // Unterminated — emit what we have
        if (start < i)
        {
            tokens.Add(line.Substring(start, i - start));
        }
    }
}
