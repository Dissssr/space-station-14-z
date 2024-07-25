using System.Numerics;
using Content.Shared.VendingMachines;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using FancyWindow = Content.Client.UserInterface.Controls.FancyWindow;

namespace Content.Client.VendingMachines.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class VendingMachineMenu : FancyWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public event Action<ItemList.ItemListSelectedEventArgs>? OnItemSelected;
        public event Action<string>? OnSearchChanged;

        public VendingMachineMenu()
        {
            MinSize = new Vector2(250, 150); // Corvax-Resize
			SetSize = new Vector2(450, 150); // Corvax-Resize
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            SearchBar.OnTextChanged += _ =>
            {
                OnSearchChanged?.Invoke(SearchBar.Text);
            };

            VendingContents.OnItemSelected += args =>
            {
                OnItemSelected?.Invoke(args);
            };
        }

        /// <summary>
        /// Populates the list of available items on the vending machine interface
        /// and sets icons based on their prototypes
        /// </summary>
        public void Populate(List<VendingMachineInventoryEntry> inventory, out List<int> filteredInventory,  string? filter = null)
        {
            filteredInventory = new();

            if (inventory.Count == 0)
            {
                VendingContents.Clear();
                var outOfStockText = Loc.GetString("vending-machine-component-try-eject-out-of-stock");
                VendingContents.AddItem(outOfStockText);
                SetSizeAfterUpdate(outOfStockText.Length, VendingContents.Count);
                return;
            }

            while (inventory.Count != VendingContents.Count)
            {
                if (inventory.Count > VendingContents.Count)
                    VendingContents.AddItem(string.Empty);
                else
                    VendingContents.RemoveAt(VendingContents.Count - 1);
            }

            var longestEntry = string.Empty;
            var spriteSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpriteSystem>();

            var filterCount = 0;
            for (var i = 0; i < inventory.Count; i++)
            {
                var entry = inventory[i];
                var vendingItem = VendingContents[i - filterCount];
                vendingItem.Text = string.Empty;
                vendingItem.Icon = null;

                var itemName = entry.ID;
                Texture? icon = null;
                if (_prototypeManager.TryIndex<EntityPrototype>(entry.ID, out var prototype))
                {
                    itemName = prototype.Name;
                    icon = spriteSystem.GetPrototypeIcon(prototype).Default;
                }

                // search filter
                if (!string.IsNullOrEmpty(filter) &&
                    !itemName.ToLowerInvariant().Contains(filter.Trim().ToLowerInvariant()))
                {
                    VendingContents.Remove(vendingItem);
                    filterCount++;
                    continue;
                }

                if (itemName.Length > longestEntry.Length)
                    longestEntry = itemName;

                vendingItem.Text = $"{itemName} [{entry.Price} TK] [{entry.Amount}]";
                vendingItem.Icon = icon;
                filteredInventory.Add(i);
            }

            SetSizeAfterUpdate(longestEntry.Length, inventory.Count);
        }

        private void SetSizeAfterUpdate(int longestEntryLength, int contentCount)
        {
            SetSize = new Vector2(Math.Clamp((longestEntryLength + 2) * 12, 250, 300),
                Math.Clamp(contentCount * 50, 150, 350));
        }
    }
}
