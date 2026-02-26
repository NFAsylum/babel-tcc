using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Parses "tradu:" annotations from trailing comments in C# source code to extract translation hints.
/// </summary>
public class TraduAnnotationParser
{
    /// <summary>
    /// The prefix that identifies a translation annotation in a comment (e.g. "// tradu:NomeMetodo").
    /// </summary>
    public const string TraduPrefix = "tradu:";

    /// <summary>
    /// Extracts all "tradu:" annotations from the source code and associates them with their target tokens.
    /// </summary>
    /// <param name="sourceCode">The C# source code containing tradu annotations in comments.</param>
    /// <returns>A list of parsed annotations with their associated identifiers or literals.</returns>
    public List<TraduAnnotation> ExtractAnnotations(string sourceCode)
    {
        List<TraduAnnotation> annotations = new List<TraduAnnotation>();

        if (string.IsNullOrEmpty(sourceCode))
        {
            return annotations;
        }

        SyntaxTree syntaxTree = RoslynWrapper.ParseSourceCode(sourceCode);
        SyntaxNode rootNode = RoslynWrapper.GetRoot(syntaxTree);

        foreach (SyntaxToken token in rootNode.DescendantTokens())
        {
            string commentText = RoslynWrapper.GetTrailingCommentText(token);
            if (!commentText.StartsWith(TraduPrefix))
            {
                continue;
            }

            string annotationText = commentText.Substring(TraduPrefix.Length);
            TraduAnnotation annotation = ParseAnnotationText(annotationText);

            int tokenLine = token.GetLocation().GetLineSpan().StartLinePosition.Line;
            annotation.SourceLine = tokenLine;

            if (annotation.IsLiteralAnnotation)
            {
                AssociateLiteralOnLine(rootNode, tokenLine, annotation);
            }
            else
            {
                AssociateIdentifierOnLine(rootNode, tokenLine, annotation);
            }

            annotations.Add(annotation);
        }

        return annotations;
    }

    /// <summary>
    /// Parses the text after the "tradu:" prefix into a structured annotation,
    /// handling identifier translations, literal translations, and parameter mappings.
    /// </summary>
    /// <param name="annotationText">The annotation text after the "tradu:" prefix.</param>
    /// <returns>A parsed annotation with the translated identifier, literal, or parameter mappings.</returns>
    public TraduAnnotation ParseAnnotationText(string annotationText)
    {
        TraduAnnotation annotation = new TraduAnnotation();

        if (string.IsNullOrEmpty(annotationText))
        {
            return annotation;
        }

        if (annotationText.StartsWith("\"") && annotationText.EndsWith("\"") && annotationText.Length >= 2)
        {
            annotation.IsLiteralAnnotation = true;
            annotation.TranslatedLiteral = annotationText.Substring(1, annotationText.Length - 2);
            return annotation;
        }

        if (annotationText.Contains(",") && annotationText.Contains(":"))
        {
            string[] parts = annotationText.Split(',');
            annotation.TranslatedIdentifier = parts[0];

            for (int partIndex = 1; partIndex < parts.Length; partIndex++)
            {
                string paramPart = parts[partIndex];
                int colonIndex = paramPart.IndexOf(':');
                if (colonIndex > 0)
                {
                    string originalParamName = paramPart.Substring(0, colonIndex);
                    string translatedParamName = paramPart.Substring(colonIndex + 1);
                    annotation.ParameterMappings.Add(new TraduParameterMapping
                    {
                        OriginalParameterName = originalParamName,
                        TranslatedParameterName = translatedParamName
                    });
                }
            }

            return annotation;
        }

        annotation.TranslatedIdentifier = annotationText;
        return annotation;
    }

    /// <summary>
    /// Associates the annotation with the first identifier token found on the specified source line.
    /// </summary>
    /// <param name="rootNode">The root syntax node of the parsed source code.</param>
    /// <param name="line">The zero-based line number where the annotation was found.</param>
    /// <param name="annotation">The annotation to associate with an identifier.</param>
    public static void AssociateIdentifierOnLine(SyntaxNode rootNode, int line, TraduAnnotation annotation)
    {
        List<SyntaxToken> identifiersOnLine = RoslynWrapper.GetIdentifierTokensOnLine(rootNode, line);

        if (identifiersOnLine.Count > 0)
        {
            annotation.OriginalIdentifier = identifiersOnLine[0].Text;
        }
    }

    /// <summary>
    /// Associates the annotation with the first string literal token found on the specified source line.
    /// </summary>
    /// <param name="rootNode">The root syntax node of the parsed source code.</param>
    /// <param name="line">The zero-based line number where the annotation was found.</param>
    /// <param name="annotation">The annotation to associate with a string literal.</param>
    public static void AssociateLiteralOnLine(SyntaxNode rootNode, int line, TraduAnnotation annotation)
    {
        List<SyntaxToken> tokensOnLine = RoslynWrapper.GetAllTokensOnLine(rootNode, line);

        foreach (SyntaxToken token in tokensOnLine)
        {
            if (RoslynWrapper.IsStringLiteralToken(token))
            {
                annotation.OriginalLiteral = token.ValueText;
                return;
            }
        }
    }
}
