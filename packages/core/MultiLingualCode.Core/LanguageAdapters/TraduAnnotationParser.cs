using System.Text.RegularExpressions;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Parses "tradu[lang]:" annotations from trailing comments in source code to extract translation hints.
/// Language-agnostic: relies on ILanguageAdapter for comment extraction and code analysis.
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
    /// <param name="sourceCode">The source code containing tradu annotations in comments.</param>
    /// <param name="adapter">The language adapter used for comment extraction and code analysis.</param>
    /// <returns>A list of parsed annotations with their associated identifiers or literals.</returns>
    public List<TraduAnnotation> ExtractAnnotations(string sourceCode, ILanguageAdapter adapter)
    {
        List<TraduAnnotation> annotations = new List<TraduAnnotation>();

        if (string.IsNullOrEmpty(sourceCode))
        {
            return annotations;
        }

        List<TrailingComment> comments = adapter.ExtractTrailingComments(sourceCode);

        foreach (TrailingComment comment in comments)
        {
            if (!comment.Text.StartsWith(TraduPrefix))
            {
                continue;
            }

            string fullAnnotationText = comment.Text.Substring(TraduPrefix.Length);
            int tokenLine = comment.Line;

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
                    string literal = adapter.GetFirstStringLiteralOnLine(sourceCode, tokenLine);
                    if (!string.IsNullOrEmpty(literal))
                    {
                        annotation.OriginalLiteral = literal;
                    }
                }
                else
                {
                    List<string> identifiers = adapter.GetIdentifierNamesOnLine(sourceCode, tokenLine);
                    if (identifiers.Count > 0)
                    {
                        annotation.OriginalIdentifier = identifiers[0];
                    }

                    if (annotation.ParameterMappings.Count > 0)
                    {
                        (int startLine, int endLine) = adapter.GetContainingMethodRange(sourceCode, tokenLine);
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

        int separatorIndex = annotationText.IndexOf(':');
        if (separatorIndex > 0 && separatorIndex < annotationText.Length - 1)
        {
            annotation.TranslatedIdentifier = annotationText.Substring(separatorIndex + 1);
            return annotation;
        }

        annotation.TranslatedIdentifier = annotationText;
        return annotation;
    }
}
