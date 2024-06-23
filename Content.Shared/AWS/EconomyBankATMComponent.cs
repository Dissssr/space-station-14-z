using Content.Shared.Containers.ItemSlots;

namespace Content.Shared.AW.Economy
{
    [RegisterComponent]
    public sealed partial class EconomyBankATMComponent : Component
    {
        public const string ATMCardId = "ATM-CardId";
        [DataField]
        public ItemSlot CardSlot = new();
    }
}