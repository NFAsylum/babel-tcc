using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.LanguageAdapters;

public class TraduAnnotationParser
{
    public const string TraduPrefix = "tradu:";

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

    public static void AssociateIdentifierOnLine(SyntaxNode rootNode, int line, TraduAnnotation annotation)
    {
        List<SyntaxToken> identifiersOnLine = RoslynWrapper.GetIdentifierTokensOnLine(rootNode, line);

        if (identifiersOnLine.Count > 0)
        {
            annotation.OriginalIdentifier = identifiersOnLine[0].Text;
        }
    }

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
