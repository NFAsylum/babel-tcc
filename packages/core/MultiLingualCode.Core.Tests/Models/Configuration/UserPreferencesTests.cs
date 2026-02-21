using MultiLingualCode.Core.Models.Configuration;

namespace MultiLingualCode.Core.Tests.Models.Configuration;

public class UserPreferencesTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var prefs = new UserPreferences();

        Assert.Equal("pt-br", prefs.LanguageCode);
        Assert.True(prefs.TranslateKeywords);
        Assert.True(prefs.TranslateIdentifiers);
        Assert.Equal("", prefs.TranslationsPath);
        Assert.True(prefs.Enabled);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ReturnsDefaults()
    {
        var prefs = UserPreferences.LoadFrom("nonexistent.json");

        Assert.Equal("pt-br", prefs.LanguageCode);
        Assert.True(prefs.Enabled);
    }

    [Fact]
    public async Task LoadFromAsync_NonExistentFile_ReturnsDefaults()
    {
        var prefs = await UserPreferences.LoadFromAsync("nonexistent.json");

        Assert.Equal("pt-br", prefs.LanguageCode);
        Assert.True(prefs.Enabled);
    }

    [Fact]
    public void SaveTo_AndLoadFrom_RoundTrip()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"prefs_{Guid.NewGuid()}.json");
        try
        {
            var original = new UserPreferences
            {
                LanguageCode = "es-es",
                TranslateKeywords = true,
                TranslateIdentifiers = false,
                TranslationsPath = "/path/to/translations",
                Enabled = false
            };

            original.SaveTo(tempFile);

            var loaded = UserPreferences.LoadFrom(tempFile);

            Assert.Equal("es-es", loaded.LanguageCode);
            Assert.True(loaded.TranslateKeywords);
            Assert.False(loaded.TranslateIdentifiers);
            Assert.Equal("/path/to/translations", loaded.TranslationsPath);
            Assert.False(loaded.Enabled);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void SaveTo_CreatesDirectoryIfNeeded()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"prefs_test_{Guid.NewGuid()}");
        var tempFile = Path.Combine(tempDir, "settings.json");
        try
        {
            var prefs = new UserPreferences { LanguageCode = "fr-fr" };
            prefs.SaveTo(tempFile);

            Assert.True(File.Exists(tempFile));

            var loaded = UserPreferences.LoadFrom(tempFile);
            Assert.Equal("fr-fr", loaded.LanguageCode);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsCorrectly()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"prefs_{Guid.NewGuid()}.json");
        try
        {
            var original = new UserPreferences { LanguageCode = "de-de" };
            original.SaveTo(tempFile);

            var loaded = await UserPreferences.LoadFromAsync(tempFile);

            Assert.Equal("de-de", loaded.LanguageCode);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
