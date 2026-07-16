using RpgGame.Core.Equipment;
using RpgGame.Core.State;
using Xunit;

namespace RpgGame.Core.Tests.Equipment;

public sealed class EquipmentScreenProjectionTests
{
    private const string JamesId = "actor.hero.james";
    private const string SwordItemId = "item.equipment.iron-sword";

    [Fact]
    public void Resolve_ShowsAllSlotsAndSeparatesWeaponAttackFromStrength()
    {
        GameState state = CreateState();

        EquipmentScreenModel screen = new EquipmentScreenProjectionResolver(TestContent.LoadCatalog())
            .Resolve(state, JamesId);

        Assert.Equal(JamesId, screen.ActorId);
        Assert.Equal(
            [EquipmentSlotIds.MainHandWeapon, EquipmentSlotIds.BodyArmor, EquipmentSlotIds.AccessoryOne],
            screen.Slots.Select(slot => slot.SlotId));
        Assert.Null(screen.Slots[0].EquippedItem);
        Assert.Null(screen.Slots[1].EquippedItem);
        Assert.Null(screen.Slots[2].EquippedItem);
        Assert.Equal(7, screen.CurrentStats.Intelligence);
        Assert.Equal(7, screen.CurrentStats.Spirit);
        Assert.Equal(0, screen.CurrentStats.WeaponAttack);
    }

    [Fact]
    public void PreviewEquipmentChange_IronSwordRaisesWeaponAttackWithoutMutatingStateOrStrength()
    {
        GameState state = CreateState();
        var resolver = new EquipmentScreenProjectionResolver(TestContent.LoadCatalog());

        EquipmentPreviewModel preview = resolver.PreviewEquipmentChange(
            state,
            JamesId,
            EquipmentSlotIds.MainHandWeapon,
            SwordItemId);

        Assert.Equal(0, preview.Current.CurrentStats.WeaponAttack);
        Assert.Equal(4, preview.PreviewStats.WeaponAttack);
        Assert.Equal(preview.Current.CurrentStats.Strength, preview.PreviewStats.Strength);
        Assert.Equal(SwordItemId, preview.CandidateItem!.ItemId);
        Assert.Empty(state.ActorProgress[JamesId].EquippedItems);
    }

    [Fact]
    public void PreviewEquipmentChange_UnequipReturnsWeaponAttackToZero()
    {
        GameState state = CreateState(SwordItemId);

        EquipmentPreviewModel preview = new EquipmentScreenProjectionResolver(TestContent.LoadCatalog())
            .PreviewEquipmentChange(state, JamesId, EquipmentSlotIds.MainHandWeapon, null);

        Assert.Equal(4, preview.Current.CurrentStats.WeaponAttack);
        Assert.Equal(0, preview.PreviewStats.WeaponAttack);
        Assert.Equal(SwordItemId, state.ActorProgress[JamesId].EquippedItems[EquipmentSlotIds.MainHandWeapon]);
    }

    [Fact]
    public void PreviewEquipmentChange_ConfirmedEquipMatchesProjectedStats()
    {
        GameState state = CreateState();
        var content = TestContent.LoadCatalog();
        var resolver = new EquipmentScreenProjectionResolver(content);
        EquipmentPreviewModel preview = resolver.PreviewEquipmentChange(
            state, JamesId, EquipmentSlotIds.MainHandWeapon, SwordItemId);
        var session = new GameSession();
        session.ReplaceState(state);

        new EquipmentService(content, session).EquipItem(
            JamesId, SwordItemId, EquipmentSlotIds.MainHandWeapon);

        Assert.Equal(
            preview.PreviewStats,
            resolver.Resolve(session.Current, JamesId).CurrentStats);
    }

    private static GameState CreateState(string? equippedItemId = null)
    {
        var equipped = new Dictionary<string, string>(StringComparer.Ordinal);
        if (equippedItemId is not null)
        {
            equipped[EquipmentSlotIds.MainHandWeapon] = equippedItemId;
        }

        return new GameState
        {
            SaveId = "equipment-screen-test",
            Inventory = new Dictionary<string, int>(StringComparer.Ordinal) { [SwordItemId] = 1 },
            ActivePartyActorIds = [JamesId],
            ActorProgress = new Dictionary<string, ActorProgressState>(StringComparer.Ordinal)
            {
                [JamesId] = new ActorProgressState
                {
                    ActorId = JamesId,
                    ClassId = "class.martial.vanguard",
                    EquippedItems = equipped,
                },
            },
        };
    }
}
