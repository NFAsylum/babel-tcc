namespace MultiLingualCode.Core.Models;

/// <summary>
/// Categorizes the syntactic kind of a source code identifier.
/// </summary>
public enum IdentifierKind
{
    /// <summary>A local variable.</summary>
    Variable,
    /// <summary>A method or constructor parameter.</summary>
    Parameter,
    /// <summary>A method or function.</summary>
    Method,
    /// <summary>A property member.</summary>
    Property,
    /// <summary>A class type.</summary>
    Class,
    /// <summary>An interface type.</summary>
    Interface,
    /// <summary>A namespace.</summary>
    Namespace,
    /// <summary>An enum type.</summary>
    Enum,
    /// <summary>A field member.</summary>
    Field,
    /// <summary>An identifier that does not fit any other category.</summary>
    Other
}
