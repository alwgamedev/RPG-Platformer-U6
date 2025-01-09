using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class DraggableInventoryItem : DraggableItem<IInventorySlotDataContainer>, IPausable
    {
        public virtual void Pause()
        //obviously you will want to change this for draggable items that live in settings or pause menu
        {
            DisableDragging();
        }

        public virtual void Unpause()
        {
            ReenableDragging();
        }
    }
}