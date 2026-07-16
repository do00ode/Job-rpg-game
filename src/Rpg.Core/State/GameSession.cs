using RpgGame.Core.Content;

namespace RpgGame.Core.State;

/// <summary>
/// In-memory owner of the active campaign snapshot across Godot scene transitions.
/// </summary>
/// <remarks>
/// This service deliberately does not know how scenes work or where saves live. Application
/// use cases replace state here, and scene controllers observe <see cref="StateChanged"/> to
/// refresh presentation. Its narrow mutations create replacement snapshots so callers cannot
/// accidentally change shared collection instances without a notification.
/// </remarks>
public sealed class GameSession : IGameSession
{
    private GameState? _current;

    /// <inheritdoc />
    public bool HasActiveGame => _current is not null;

    /// <inheritdoc />
    public GameState Current => _current
        ?? throw new InvalidOperationException("No game is active. Start or load a game first.");

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public void ReplaceState(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Publish(state);
    }

    /// <inheritdoc />
    public void UpdateLocation(MapLocationState location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ValidateLocation(location);

        if (Current.Location == location)
        {
            return;
        }

        Publish(Current with { Location = location });
    }

    /// <inheritdoc />
    public void UpdateInventory(IReadOnlyDictionary<string, int> inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var replacement = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach ((string itemId, int quantity) in inventory)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException(
                    "Inventory item IDs cannot be blank.",
                    nameof(inventory));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException(
                    $"Inventory quantity for '{itemId}' must be positive; received {quantity}.",
                    nameof(inventory));
            }

            if (!replacement.TryAdd(itemId, quantity))
            {
                throw new ArgumentException(
                    $"Inventory contains duplicate item ID '{itemId}'.",
                    nameof(inventory));
            }
        }

        if (InventoriesEqual(Current.Inventory, replacement))
        {
            return;
        }

        Publish(Current with { Inventory = replacement });
    }

    /// <inheritdoc />
    public void UpdateActorProgress(string actorId, ActorProgressState progress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentNullException.ThrowIfNull(progress);

        if (!Current.ActorProgress.TryGetValue(actorId, out ActorProgressState? existing))
        {
            throw new KeyNotFoundException($"Actor progress for '{actorId}' does not exist.");
        }

        if (!string.Equals(progress.ActorId, actorId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Replacement progress belongs to '{progress.ActorId}', not '{actorId}'.",
                nameof(progress));
        }

        if (existing == progress)
        {
            return;
        }

        var replacement = new Dictionary<string, ActorProgressState>(
            Current.ActorProgress,
            StringComparer.Ordinal)
        {
            [actorId] = progress,
        };
        Publish(Current with { ActorProgress = replacement });
    }

    /// <inheritdoc />
    public bool GetEventFlag(string flagId)
    {
        ValidateFlagId(flagId);
        return Current.EventFlags.TryGetValue(flagId, out bool value) && value;
    }

    /// <inheritdoc />
    public void SetEventFlag(string flagId, bool value = true)
    {
        ValidateFlagId(flagId);

        if (Current.EventFlags.TryGetValue(flagId, out bool existing) && existing == value)
        {
            return;
        }

        var flags = new Dictionary<string, bool>(Current.EventFlags, StringComparer.Ordinal)
        {
            [flagId] = value,
        };
        Publish(Current with { EventFlags = flags });
    }

    private void Publish(GameState state)
    {
        _current = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool InventoriesEqual(
        IReadOnlyDictionary<string, int>? current,
        IReadOnlyDictionary<string, int> replacement)
    {
        if (current is null || current.Count != replacement.Count)
        {
            return false;
        }

        foreach ((string itemId, int quantity) in current)
        {
            if (!replacement.TryGetValue(itemId, out int replacementQuantity)
                || replacementQuantity != quantity)
            {
                return false;
            }
        }

        return true;
    }

    private static void ValidateLocation(MapLocationState location)
    {
        if (!ContentId.IsValid(location.MapId)
            || !location.MapId.StartsWith("map.", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Map ID '{location.MapId}' must be a canonical map.* ID.",
                nameof(location));
        }

        if (location.X < 0 || location.Y < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(location),
                "Exploration tile coordinates cannot be negative.");
        }

        if (location.Facing is not ("north" or "east" or "south" or "west"))
        {
            throw new ArgumentException(
                $"Facing '{location.Facing}' is not north/east/south/west.",
                nameof(location));
        }
    }

    private static void ValidateFlagId(string flagId)
    {
        if (!ContentId.IsValid(flagId)
            || !flagId.StartsWith("flag.", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Event flag '{flagId}' must be a canonical flag.* ID.",
                nameof(flagId));
        }
    }
}
