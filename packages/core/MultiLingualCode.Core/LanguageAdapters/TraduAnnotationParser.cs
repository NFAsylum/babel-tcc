using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Parses "tradu[lang]:" annotations from trailing comments in C# source code to extract translation hints.
/// </summary>
public class TraduAnnotationParser
{
    /// <summary>
    /// The prefix that identifies a translation annotation in a comment (e.g. "// tradu[pt-br]:NomeMetodo").
    /// </summary>
    public const string TraduPrefix = "tradu";

    /// <summary>
    /// Regex to detect a language prefix in a segment (e.g. "[pt-br]:" or "[es]:").
    /// </summary>
    public static readonly Regex LanguagePrefixRegex = new Regex(@"^\[([a-z]{2}(-[a-z]+)?)\]:(.+)$", RegexOptions.Compiled);

    /// <summary>
    /// Extracts all "tradu[lang]:" annotations from the source code and associates them with their target tokens.
    /// Supports multi-language format with | separator (e.g. "[pt-br]:Calculadora|[es]:Calculadora").
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

            string fullAnnotationText = commentText.Substring(TraduPrefix.Length);
            int tokenLine = token.GetLocation().GetLineSpan().StartLinePosition.Line;

            string[] segments = fullAnnotationText.Split('|');

            foreach (string segment in segments)
            {
                string trimmedSegment = segment.Trim();
                if (string.IsNullOrEmpty(trimmedSegment))
                {
                    continue;
                }

                Match langMatch = LanguagePrefixRegex.Match(trimmedSegment);
                if (!langMatch.Success)
                {
                    continue;
                }

                string targetLanguage = langMatch.Groups[1].Value;
                string annotationText = langMatch.Groups[3].Value;

                TraduAnnotation annotation = ParseAnnotationText(annotationText);
                annotation.SourceLine = tokenLine;
                annotation.TargetLanguage = targetLanguage;

                if (annotation.IsLiteralAnnotation)
                {
                    AssociateLiteralOnLine(rootNode, tokenLine, annotation);
                }
                else
                {
                    AssociateIdentifierOnLine(rootNode, tokenLine, annotation);

                    if (annotation.ParameterMappings.Count > 0)
                    {
                        (int startLine, int endLine) = RoslynWrapper.GetMethodRange(rootNode, tokenLine);
                        annotation.MethodStartLine = startLine;
                        annotation.MethodEndLine = endLine;
                    }
                }

                annotations.Add(annotation);
            }
        }

        return annotations;
    }

    /// <summary>
    /// Parses the text after the "tradu[lang]:" prefix into a structured annotation,
    /// handling identifier translations, literal translations, and parameter mappings.
    /// </summary>
    /// <param name="annotationText">The annotation text after the "tradu[lang]:" prefix.</param>
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
