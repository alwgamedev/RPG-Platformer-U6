using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class InteractableGameObjectRightClickMenuSpawner : RightClickMenuSpawner
    {
        IInteractableGameObject igo;

        protected override void Awake()
        {
            base.Awake();

            igo = GetComponent<IInteractableGameObject>();
        }

        public override void ConfigureMenu(GameObject menu)
        {
            foreach (var option in igo.InteractionOptions())
            {
                CreateAndConfigureButton(menu, menuButtonPrefab, option.Item1, () =>
                {
                    if (option.Item2())
                    {
                        option.Item3();
                    }
                });
            }

            CreateAndConfigureButton(menu, menuButtonPrefab, "Cancel", ClearMenu);
        }
    }
}