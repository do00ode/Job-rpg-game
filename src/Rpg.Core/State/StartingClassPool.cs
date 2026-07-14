using RpgGame.Core.Content;
using RpgGame.Core.Content.Definitions;

namespace RpgGame.Core.State;

/// <summary>
/// Resolves the deterministic set of class IDs that a new campaign may select.
/// </summary>
public static class StartingClassPool
{
    /// <summary>
    /// Combines every validated base/mod rule. Exclusion wins globally, making conflicts
    /// predictable and independent from mod discovery or file enumeration order.
    /// </summary>
    public static IReadOnlyList<string> Resolve(IContentCatalog content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var included = new HashSet<string>(StringComparer.Ordinal);
        var excluded = new HashSet<string>(StringComparer.Ordinal);

        foreach (StartingClassRuleDefinition rule in
                 content.GetAll<StartingClassRuleDefinition>())
        {
            // Explicit JSON nulls are reported by validation. Treat them as empty here so
            // validation can continue aggregating other useful authoring errors.
            included.UnionWith(rule.IncludeClassIds ?? []);
            excluded.UnionWith(rule.ExcludeClassIds ?? []);
        }

        included.ExceptWith(excluded);
        return included.Order(StringComparer.Ordinal).ToArray();
    }
}
