using System.Text.Json;

namespace RpgGame.Localization;

/// <summary>Loads one authored language catalog for presentation-only text lookup.</summary>
public sealed class LocalizedTextCatalog
{
    private readonly IReadOnlyDictionary<string, string> _texts;

    public LocalizedTextCatalog(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Localization catalog does not exist.", filePath);
        }

        LocalizationDocument document;
        try
        {
            document = JsonSerializer.Deserialize<LocalizationDocument>(
                File.ReadAllText(filePath),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? throw new InvalidDataException("Localization catalog deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Localization catalog contains invalid JSON.", exception);
        }

        if (document.SchemaVersion != 1)
        {
            throw new InvalidDataException(
                $"Localization catalog '{filePath}' has unsupported schema version {document.SchemaVersion}.");
        }

        if (string.IsNullOrWhiteSpace(document.Locale))
        {
            throw new InvalidDataException("Localization catalog locale must be nonblank.");
        }

        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in document.Texts)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException("Localization catalog keys and values must be nonblank.");
            }

            texts.Add(key, value);
        }

        _texts = texts;
    }

    /// <summary>Returns authored prose, or the stable key when a future key is not translated yet.</summary>
    public string Resolve(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _texts.TryGetValue(key, out string? text) ? text : key;
    }

    private sealed record LocalizationDocument
    {
        public int SchemaVersion { get; init; }
        public string Locale { get; init; } = string.Empty;
        public Dictionary<string, string> Texts { get; init; } = [];
    }
}
