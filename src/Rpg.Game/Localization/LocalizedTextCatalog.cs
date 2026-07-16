using RpgGame.Core.Content.Loading;

namespace RpgGame.Localization;

/// <summary>Presentation adapter over the validated core localization catalog.</summary>
public sealed class LocalizedTextCatalog
{
    private readonly LocalizationCatalog _catalog;

    public LocalizedTextCatalog(LocalizationCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public string Resolve(string key) => _catalog.Resolve(key);
}
