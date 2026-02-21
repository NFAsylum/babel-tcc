namespace MultiLingualCode.Core.Exceptions;

/// <summary>
/// Thrown when a file extension has no registered language adapter.
/// </summary>
public class UnsupportedLanguageException : Exception
{
    public string FileExtension { get; }

    public UnsupportedLanguageException(string fileExtension)
        : base($"No language adapter registered for file extension '{fileExtension}'.")
    {
        FileExtension = fileExtension;
    }

    public UnsupportedLanguageException(string fileExtension, string message)
        : base(message)
    {
        FileExtension = fileExtension;
    }
}
