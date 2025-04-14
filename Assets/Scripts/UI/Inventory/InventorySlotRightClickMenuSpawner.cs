using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    //needs to be attached to an IInventoryItemHolder like an InventorySlotUI
    [RequireComponent(typeof(IInventoryItemHolder))]
    public class InventorySlotRightClickMenuSpawner : RightClickMenuSpawner
    {
        //[SerializeField] protected Button menuButtonPrefab;

        protected IInventorySlotDataContainer slotComponent;

        protected override void Awake()
        {
            base.Awake();

            slotComponent = GetComponent<IInventorySlotDataContainer>();
        }

        public override bool CanCreateMenu()
        {
            return slotComponent.Item != null && menuPrefab && menuButtonPrefab;
        }

        //NOTE: the menu should probably have a vertical layout group on it and a content size fitter, like the inventory
        public override void ConfigureMenu(GameObject menu)
        {
            InventoryItem item = slotComponent.Item;
            foreach (var entry in item.RightClickActions())
            {
                CreateAndConfigureButton(menu, menuButtonPrefab, entry.Item1, entry.Item2);
            }
            CreateAndConfigureButton(menu, menuButtonPrefab, $"Examine {item.BaseData.DisplayName}", item.Examine);
            CreateAndConfigureDropButtons(menu, menuButtonPrefab, slotComponent);
            CreateAndConfigureButton(menu, menuButtonPrefab, "Cancel", ClearMenu);
        }

        protected virtual void CreateAndConfigureDropButtons(GameObject menu, Button menuButtonPrefab, 
            IInventorySlotDataContainer data, string dropVerb = "Drop")
        {
            if(data.Quantity == 1)
            {
                CreateAndConfigureButton(menu, menuButtonPrefab, 
                    $"{dropVerb} {data.Item.BaseData.DisplayName}", () => data.Item.ReleaseFromInventory(1));
            }
            else if(data.Quantity > 1)
            {
                void DropX()
                {
                    GameLog.ListenForNumericalInput();
                    GameLog.InputField.InputSubmitted += InputHandler;
                    GameLog.InputField.OnReset += Complete;
                }

                void InputHandler(object input)
                {
                    if (input != null && input is int val)
                    {
                        data?.Item?.ReleaseFromInventory(val);
                    }
                    Complete();
                }

                void Complete()
                {
                    GameLog.InputField.InputSubmitted -= InputHandler;
                    GameLog.InputField.OnReset -= Complete;
                }

                CreateAndConfigureButton(menu, menuButtonPrefab, $"{dropVerb} 1", () => data.Item.ReleaseFromInventory(1));
                CreateAndConfigureButton(menu, menuButtonPrefab, $"{dropVerb} 5", () => data.Item.ReleaseFromInventory(5));
                CreateAndConfigureButton(menu, menuButtonPrefab, $"{dropVerb} 10", () => data.Item.ReleaseFromInventory(10));
                CreateAndConfigureButton(menu, menuButtonPrefab, $"{dropVerb} X", DropX);
                CreateAndConfigureButton(menu, menuButtonPrefab, $"{dropVerb} All", 
                    () => data.Item.ReleaseFromInventory(data.Quantity));
            }
        }
    }
}