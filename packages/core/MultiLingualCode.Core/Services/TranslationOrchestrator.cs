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

        if (!string.IsNullOrEmpty(IdentifierMapperService.LoadedPath))
        {
            IdentifierMapperService.SaveMap();
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
}
