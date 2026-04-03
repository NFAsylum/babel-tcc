using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Models;

public class ValidationResultTests
{
    [Fact]
    public void Constructor_WhenCreated_DiagnosticsIsEmpty()
    {
        ValidationResult result = new ValidationResult();

        Assert.NotNull(result.Diagnostics);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void IsValid_WhenNewInstance_DefaultsToFalse()
    {
        ValidationResult result = new ValidationResult();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Diagnostics_AfterAddingOne_IncreasesCount()
    {
        ValidationResult result = new ValidationResult();
        result.Diagnostics.Add(new Diagnostic
        {
            Severity = DiagnosticSeverity.Error,
            Message = "test error",
            Line = 1,
            Column = 0
        });

        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void Diagnostic_WithAllProperties_SetsValuesCorrectly()
    {
        Diagnostic diag = new Diagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = "unused variable",
            Line = 10,
            Column = 5
        };

        Assert.Equal(DiagnosticSeverity.Warning, diag.Severity);
        Assert.Equal("unused variable", diag.Message);
        Assert.Equal(10, diag.Line);
        Assert.Equal(5, diag.Column);
    }

    [Fact]
    public void DiagnosticSeverity_WhenEnumerated_HasThreeValues()
    {
        Array values = Enum.GetValues(typeof(DiagnosticSeverity));
        Assert.Equal(3, values.Length);
    }
}
