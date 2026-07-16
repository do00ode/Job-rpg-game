using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Equipment;
using RpgGame.Core.State;
using RpgGame.Core.Tests.Combat;
using Xunit;

namespace RpgGame.Core.Tests.Equipment;

public sealed class EquipmentServiceTests
{
    private const string ActorId = "actor.hero.james";
    private const string SwordItemId = "item.test.sword";
    private const string SecondSwordItemId = "item.test.second-sword";
    private const string PotionItemId = "item.test.potion";

    [Fact]
    public void EquipItem_RequiresKnownActorOwnedEquipmentAndCompatibleSlot()
    {
        (EquipmentService equipment, GameSession session) = CreateService();

        Assert.Throws<KeyNotFoundException>(() => equipment.EquipItem(
            "actor.hero.unknown", SwordItemId, EquipmentSlotIds.MainHandWeapon));
        Assert.Throws<KeyNotFoundException>(() => equipment.EquipItem(
            ActorId, "item.test.unknown", EquipmentSlotIds.MainHandWeapon));
        Assert.Throws<InvalidOperationException>(() => equipment.EquipItem(
            ActorId, SwordItemId, EquipmentSlotIds.MainHandWeapon));
        Assert.Throws<ArgumentException>(() => equipment.EquipItem(
            ActorId, PotionItemId, EquipmentSlotIds.MainHandWeapon));

        session.UpdateInventory(new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [SwordItemId] = 1,
        });
        Assert.Throws<ArgumentException>(() => equipment.EquipItem(
            ActorId, SwordItemId, "slot.armor.body"));
        Assert.Empty(session.Current.ActorProgress[ActorId].EquippedItems);
    }

    [Fact]
    public void EquipItem_ReplacesSlotWithoutChangingInventory_AndUnequipIsNoOpWhenEmpty()
    {
        (EquipmentService equipment, GameSession session) = CreateService(new Dictionary<string, int>(
            StringComparer.Ordinal)
        {
            [SwordItemId] = 1,
            [SecondSwordItemId] = 1,
        });

        equipment.EquipItem(ActorId, SwordItemId, EquipmentSlotIds.MainHandWeapon);
        equipment.EquipItem(ActorId, SecondSwordItemId, EquipmentSlotIds.MainHandWeapon);

        Assert.Equal(
            SecondSwordItemId,
            session.Current.ActorProgress[ActorId].EquippedItems[EquipmentSlotIds.MainHandWeapon]);
        Assert.Equal(1, session.Current.Inventory[SwordItemId]);
        Assert.Equal(1, session.Current.Inventory[SecondSwordItemId]);

        equipment.UnequipItem(ActorId, EquipmentSlotIds.MainHandWeapon);
        GameState afterUnequip = session.Current;
        equipment.UnequipItem(ActorId, EquipmentSlotIds.MainHandWeapon);

        Assert.Empty(session.Current.ActorProgress[ActorId].EquippedItems);
        Assert.Same(afterUnequip, session.Current);
        Assert.Equal(1, session.Current.Inventory[SwordItemId]);
        Assert.Equal(1, session.Current.Inventory[SecondSwordItemId]);
    }

    private static (EquipmentService Equipment, GameSession Session) CreateService(
        Dictionary<string, int>? inventory = null)
    {
        var session = new GameSession();
        session.ReplaceState(new GameState
        {
            SaveId = "equipment-service-test",
            Inventory = inventory ?? new Dictionary<string, int>(StringComparer.Ordinal),
            ActorProgress = new Dictionary<string, ActorProgressState>(StringComparer.Ordinal)
            {
                [ActorId] = new ActorProgressState
                {
                    ActorId = ActorId,
                    ClassId = "class.test",
                },
            },
        });
        return (new EquipmentService(new TestCatalog(
            Item(SwordItemId),
            Item(SecondSwordItemId),
            Item(PotionItemId),
            Weapon("equipment.test.sword", SwordItemId),
            Weapon("equipment.test.second-sword", SecondSwordItemId)), session), session);
    }

    private static ItemDefinition Item(string id) => new()
    {
        Id = id,
        DisplayNameKey = $"{id}.name",
        DescriptionKey = $"{id}.description",
    };

    private static EquipmentDefinition Weapon(string id, string itemId) => new()
    {
        Id = id,
        ItemId = itemId,
        SlotId = EquipmentSlotIds.MainHandWeapon,
        Attack = 1,
    };
}
