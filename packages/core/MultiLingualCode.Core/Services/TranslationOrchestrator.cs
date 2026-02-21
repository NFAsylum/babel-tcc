using MultiLingualCode.Core.Exceptions;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Central coordinator for translating source code between programming and natural languages.
/// Orchestrates the flow: get adapter -> parse -> translate AST -> generate code.
/// </summary>
public class TranslationOrchestrator
{
    private readonly LanguageRegistry _registry;
    private readonly INaturalLanguageProvider _provider;
    private readonly IdentifierMapper _identifierMapper;

    public TranslationOrchestrator(
        LanguageRegistry registry,
        INaturalLanguageProvider provider,
        IdentifierMapper identifierMapper)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(identifierMapper);

        _registry = registry;
        _provider = provider;
        _identifierMapper = identifierMapper;
    }

    /// <summary>
    /// Translates source code from a programming language to a natural language representation.
    /// Flow: get adapter by extension -> parse -> load translation table -> walk AST translating nodes -> generate.
    /// </summary>
    /// <param name="sourceCode">Original source code.</param>
    /// <param name="fileExtension">File extension to determine the language adapter (e.g. ".cs").</param>
    /// <param name="targetLanguage">Target natural language code (e.g. "pt-br").</param>
    /// <returns>Source code with keywords and identifiers translated to the target language.</returns>
    public async Task<string> TranslateToNaturalLanguageAsync(
        string sourceCode, string fileExtension, string targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(sourceCode);
        ArgumentNullException.ThrowIfNull(fileExtension);
        ArgumentNullException.ThrowIfNull(targetLanguage);

        var adapter = GetAdapterOrThrow(fileExtension);

        await _provider.LoadTranslationTableAsync(adapter.LanguageName);

        var ast = adapter.Parse(sourceCode);
        var translatedAst = ast.Clone();

        TranslateAstForward(translatedAst, targetLanguage);

        return adapter.Generate(translatedAst);
    }

    /// <summary>
    /// Translates code from a natural language representation back to the original programming language.
    /// Flow: get adapter by extension -> parse -> load translation table -> walk AST reversing translations -> generate.
    /// </summary>
    /// <param name="translatedCode">Source code with translated keywords/identifiers.</param>
    /// <param name="fileExtension">File extension to determine the language adapter (e.g. ".cs").</param>
    /// <param name="sourceLanguage">The natural language used in the translated code (e.g. "pt-br").</param>
    /// <returns>Source code with original programming language keywords and identifiers.</returns>
    public async Task<string> TranslateFromNaturalLanguageAsync(
        string translatedCode, string fileExtension, string sourceLanguage)
    {
        ArgumentNullException.ThrowIfNull(translatedCode);
        ArgumentNullException.ThrowIfNull(fileExtension);
        ArgumentNullException.ThrowIfNull(sourceLanguage);

        var adapter = GetAdapterOrThrow(fileExtension);

        await _provider.LoadTranslationTableAsync(adapter.LanguageName);

        var ast = adapter.Parse(translatedCode);
        var originalAst = ast.Clone();

        TranslateAstReverse(originalAst, sourceLanguage);

        return adapter.Generate(originalAst);
    }

    private ILanguageAdapter GetAdapterOrThrow(string fileExtension)
    {
        var adapter = _registry.GetAdapter(fileExtension);
        if (adapter is null)
            throw new UnsupportedLanguageException(fileExtension);
        return adapter;
    }

    private void TranslateAstForward(ASTNode node, string targetLanguage)
    {
        switch (node)
        {
            case KeywordNode keyword:
                var translated = _provider.TranslateKeyword(keyword.KeywordId);
                if (translated != null)
                    keyword.OriginalKeyword = translated;
                break;

            case IdentifierNode identifier when identifier.IsTranslatable:
                var translatedId = _identifierMapper.GetTranslation(identifier.Name, targetLanguage);
                if (translatedId != null)
                {
                    identifier.TranslatedName = translatedId;
                    identifier.Name = translatedId;
                }
                break;
        }

        foreach (var child in node.Children)
        {
            TranslateAstForward(child, targetLanguage);
        }
    }

    private void TranslateAstReverse(ASTNode node, string sourceLanguage)
    {
        switch (node)
        {
            case KeywordNode keyword:
                var keywordId = _provider.ReverseTranslateKeyword(keyword.OriginalKeyword);
                if (keywordId >= 0)
                {
                    // Get the original programming language keyword from the keyword table
                    var provider = _provider as NaturalLanguageProvider;
                    var originalKeyword = provider?.ActiveKeywordTable?.GetKeyword(keywordId);
                    if (originalKeyword != null)
                    {
                        keyword.OriginalKeyword = originalKeyword;
                        keyword.KeywordId = keywordId;
                    }
                }
                break;

            case IdentifierNode identifier:
                var originalId = _identifierMapper.GetOriginal(identifier.Name, sourceLanguage);
                if (originalId != null)
                {
                    identifier.Name = originalId;
                    identifier.TranslatedName = null;
                }
                break;
        }

        foreach (var child in node.Children)
        {
            TranslateAstReverse(child, sourceLanguage);
        }
    }
}
