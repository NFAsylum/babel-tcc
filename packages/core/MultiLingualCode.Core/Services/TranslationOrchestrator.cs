using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Services;

public class TranslationOrchestrator
{
    public LanguageRegistry Registry { get; }
    public INaturalLanguageProvider Provider { get; }
    public IdentifierMapper IdentifierMapperService { get; }

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
            new TranslationOrchestrator(registry, provider, identifierMapper));
    }

    public TranslationOrchestrator(
        LanguageRegistry registry,
        INaturalLanguageProvider provider,
        IdentifierMapper identifierMapper)
    {
        Registry = registry;
        Provider = provider;
        IdentifierMapperService = identifierMapper;
    }

    public async Task<OperationResultGeneric<string>> TranslateToNaturalLanguageAsync(
        string sourceCode, string fileExtension, string targetLanguage)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = Registry.GetAdapter(fileExtension);
        if (!adapterResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(adapterResult.ErrorMessage);
        }

        ILanguageAdapter adapter = adapterResult.Value;

        await Provider.LoadTranslationTableAsync(adapter.LanguageName);

        ApplyTraduAnnotations(sourceCode, targetLanguage);

        ASTNode ast = adapter.Parse(sourceCode);
        ASTNode translatedAst = ast.Clone();

        TranslateAstForward(translatedAst, targetLanguage);

        string result = adapter.Generate(translatedAst);
        return OperationResultGeneric<string>.Ok(result);
    }

    public async Task<OperationResultGeneric<string>> TranslateFromNaturalLanguageAsync(
        string translatedCode, string fileExtension, string sourceLanguage)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = Registry.GetAdapter(fileExtension);
        if (!adapterResult.IsSuccess)
        {
            return OperationResultGeneric<string>.Fail(adapterResult.ErrorMessage);
        }

        ILanguageAdapter adapter = adapterResult.Value;

        await Provider.LoadTranslationTableAsync(adapter.LanguageName);

        string preSubstituted = adapter.ReverseSubstituteKeywords(
            translatedCode, Provider.ReverseTranslateKeyword);

        ASTNode ast = adapter.Parse(preSubstituted);
        ASTNode originalAst = ast.Clone();

        TranslateAstReverse(originalAst, sourceLanguage);

        string result = adapter.Generate(originalAst);
        return OperationResultGeneric<string>.Ok(result);
    }

    public void TranslateAstForward(ASTNode node, string targetLanguage)
    {
        switch (node)
        {
            case KeywordNode keyword:
                OperationResultGeneric<string> translatedResult = Provider.TranslateKeyword(keyword.KeywordId);
                if (translatedResult.IsSuccess)
                {
                    keyword.OriginalKeyword = translatedResult.Value;
                }
                break;

            case IdentifierNode identifier when identifier.IsTranslatable:
                OperationResultGeneric<string> translatedIdResult = IdentifierMapperService.GetTranslation(identifier.Name, targetLanguage);
                if (translatedIdResult.IsSuccess)
                {
                    identifier.TranslatedName = translatedIdResult.Value;
                    identifier.Name = translatedIdResult.Value;
                }
                break;

            case LiteralNode literal when literal.IsTranslatable:
                string literalText = literal.Value.ToString() ?? "";
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

    public void TranslateAstReverse(ASTNode node, string sourceLanguage)
    {
        switch (node)
        {
            case KeywordNode keyword:
                int keywordId = Provider.ReverseTranslateKeyword(keyword.OriginalKeyword);
                if (keywordId >= 0)
                {
                    NaturalLanguageProvider provider = (NaturalLanguageProvider)Provider;
                    OperationResultGeneric<string> originalKeywordResult = provider.GetActiveKeywordTable().GetKeyword(keywordId);
                    if (originalKeywordResult.IsSuccess)
                    {
                        keyword.OriginalKeyword = originalKeywordResult.Value;
                        keyword.KeywordId = keywordId;
                    }
                }
                break;

            case IdentifierNode identifier:
                OperationResultGeneric<string> originalIdResult = IdentifierMapperService.GetOriginal(identifier.Name, sourceLanguage);
                if (originalIdResult.IsSuccess)
                {
                    identifier.Name = originalIdResult.Value;
                    identifier.TranslatedName = "";
                }
                break;

            case LiteralNode literal when literal.IsTranslatable:
                string translatedLiteralText = literal.Value.ToString() ?? "";
                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in IdentifierMapperService.Data.Literals)
                {
                    if (kvp.Value.TryGetValue(sourceLanguage, out string? translatedValue)
                        && translatedValue is not null
                        && string.Equals(translatedValue, translatedLiteralText, StringComparison.Ordinal))
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

    public void ApplyTraduAnnotations(string sourceCode, string targetLanguage)
    {
        TraduAnnotationParser parser = new TraduAnnotationParser();
        List<TraduAnnotation> annotations = parser.ExtractAnnotations(sourceCode);

        foreach (TraduAnnotation annotation in annotations)
        {
            if (annotation.IsLiteralAnnotation)
            {
                IdentifierMapperService.SetLiteralTranslation(
                    annotation.OriginalLiteral, targetLanguage, annotation.TranslatedLiteral);
            }
            else
            {
                IdentifierMapperService.SetTranslation(
                    annotation.OriginalIdentifier, targetLanguage, annotation.TranslatedIdentifier);

                foreach (TraduParameterMapping paramMapping in annotation.ParameterMappings)
                {
                    IdentifierMapperService.SetTranslation(
                        paramMapping.OriginalParameterName, targetLanguage, paramMapping.TranslatedParameterName);
                }
            }
        }
    }
}
