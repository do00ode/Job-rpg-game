using System.Text.Json;
using System.Text.Json.Serialization;

namespace RpgGame.Core.Content.Loading;

/// <summary>One raw localization file before it is parsed and merged.</summary>
public sealed record LocalizationBundleDocument(string RelativePath, string Json);

/// <summary>One actionable localization bundle error tied to a file and JSON location.</summary>
public sealed record LocalizationProblem(
    string FilePath,
    string JsonPath,
    string Code,
    string Message)
{
    public override string ToString() => $"{FilePath} {JsonPath}: {Message} [{Code}]";
}

/// <summary>Immutable text dictionary for one validated locale.</summary>
public sealed class LocalizationCatalog
{
    private readonly IReadOnlyDictionary<string, string> _texts;

    internal LocalizationCatalog(string locale, IReadOnlyDictionary<string, string> texts)
    {
        Locale = locale;
        _texts = texts;
    }

    public string Locale { get; }
    public IReadOnlyDictionary<string, string> Texts => _texts;

    public string Resolve(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _texts.TryGetValue(key, out string? text) ? text : $"??{key}??";
    }

    public bool Contains(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _texts.ContainsKey(key);
    }
}

/// <summary>Result of parsing and merging one locale's recursive bundle.</summary>
public sealed class LocalizationLoadResult
{
    internal LocalizationLoadResult(
        LocalizationCatalog? catalog,
        IReadOnlyList<LocalizationProblem> problems)
    {
        Catalog = catalog;
        Problems = problems;
    }

    public LocalizationCatalog? Catalog { get; }
    public IReadOnlyList<LocalizationProblem> Problems { get; }
    public bool IsSuccess => Catalog is not null && Problems.Count == 0;
}

/// <summary>Parses scoped localization JSON files and merges them deterministically.</summary>
public sealed class LocalizationBundleLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    public LocalizationLoadResult Load(
        string locale,
        IEnumerable<LocalizationBundleDocument> documents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(documents);

        var problems = new List<LocalizationProblem>();
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        LocalizationBundleDocument[] orderedDocuments = documents
            .Where(document => document is not null)
            .OrderBy(document => document.RelativePath, StringComparer.Ordinal)
            .ToArray();

        foreach (LocalizationBundleDocument document in orderedDocuments)
        {
            ParseDocument(locale, document, texts, problems);
        }

        IReadOnlyList<LocalizationProblem> orderedProblems = problems
            .OrderBy(problem => problem.FilePath, StringComparer.Ordinal)
            .ThenBy(problem => problem.JsonPath, StringComparer.Ordinal)
            .ThenBy(problem => problem.Code, StringComparer.Ordinal)
            .ToArray();

        if (orderedProblems.Count > 0)
        {
            return new LocalizationLoadResult(null, orderedProblems);
        }

        var stableTexts = new SortedDictionary<string, string>(texts, StringComparer.Ordinal);
        return new LocalizationLoadResult(
            new LocalizationCatalog(locale, stableTexts),
            orderedProblems);
    }

    private static void ParseDocument(
        string expectedLocale,
        LocalizationBundleDocument document,
        IDictionary<string, string> mergedTexts,
        ICollection<LocalizationProblem> problems)
    {
        JsonDocument? syntaxDocument = null;
        try
        {
            syntaxDocument = JsonDocument.Parse(document.Json);
            if (syntaxDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                Add(problems, document, "$", "json.object-required",
                    "A localization file must contain one JSON object.");
                return;
            }

            if (!syntaxDocument.RootElement.TryGetProperty("texts", out JsonElement textsElement)
                || textsElement.ValueKind != JsonValueKind.Object)
            {
                Add(problems, document, "$.texts", "texts.object-required",
                    "Localization texts must be a JSON object.");
                return;
            }

            var seenInFile = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in textsElement.EnumerateObject())
            {
                if (!seenInFile.Add(property.Name))
                {
                    Add(problems, document, "$.texts", "key.duplicate",
                        $"Localization key '{property.Name}' is duplicated in this file.");
                }
            }

            LocalizationDocument parsed = JsonSerializer.Deserialize<LocalizationDocument>(
                    document.Json,
                    SerializerOptions)
                ?? throw new InvalidDataException("Localization file deserialized to null.");

            if (parsed.SchemaVersion != 1)
            {
                Add(problems, document, "$.schemaVersion", "schema.unsupported",
                    $"Schema version {parsed.SchemaVersion} is unsupported; expected 1.");
            }

            if (!string.Equals(parsed.Locale, expectedLocale, StringComparison.Ordinal))
            {
                Add(problems, document, "$.locale", "locale.mismatch",
                    $"Localization file declares locale '{parsed.Locale}', expected '{expectedLocale}'.");
            }

            foreach ((string key, string value) in parsed.Texts)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    Add(problems, document, "$.texts", "key.blank",
                        "Localization keys must be nonblank.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    Add(problems, document, $"$.texts.{key}", "value.blank",
                        "Localization values must be nonblank.");
                    continue;
                }

                if (mergedTexts.ContainsKey(key))
                {
                    Add(problems, document, $"$.texts.{key}", "key.duplicate",
                        $"Localization key '{key}' is already defined by another bundle file.");
                    continue;
                }

                mergedTexts.Add(key, value);
            }
        }
        catch (JsonException exception)
        {
            Add(problems, document, exception.Path ?? "$", "json.invalid", exception.Message);
        }
        catch (InvalidDataException exception)
        {
            Add(problems, document, "$", "json.invalid", exception.Message);
        }
        finally
        {
            syntaxDocument?.Dispose();
        }
    }

    private static void Add(
        ICollection<LocalizationProblem> problems,
        LocalizationBundleDocument document,
        string jsonPath,
        string code,
        string message) => problems.Add(new LocalizationProblem(
            document.RelativePath,
            jsonPath,
            code,
            message));

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    private sealed record LocalizationDocument
    {
        [JsonRequired]
        public int SchemaVersion { get; init; }

        [JsonRequired]
        public string Locale { get; init; } = string.Empty;

        [JsonRequired]
        public Dictionary<string, string> Texts { get; init; } = [];
    }
}
