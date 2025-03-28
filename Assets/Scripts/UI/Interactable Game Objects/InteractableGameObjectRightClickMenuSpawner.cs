using RPGPlatformer.Core;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class InteractableGameObjectRightClickMenuSpawner : RightClickMenuSpawner
    {
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