using System;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Loot
{
    public interface ILootDrop : IInventoryOwner, IExaminable
    {
        public string DisplayName { get; }

        public event Action OnDropDestroyed;
        public event Action PlayerOutOfRange;

        public void TakeAll();
        public void Search();

        public void BeginInspection();
        public void EndInspection();
    }
}