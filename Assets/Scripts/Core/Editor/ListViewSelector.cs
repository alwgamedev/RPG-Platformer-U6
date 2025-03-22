using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Core.Editor
{
    public class ListViewSelector : MouseManipulator
    {
        public ListViewSelector()
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
            if (Application.platform == RuntimePlatform.WindowsEditor 
                || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse,
                    modifiers = EventModifiers.Command
                });
            }
            else
            {
                activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse,
                    modifiers = EventModifiers.Control
                });
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            var element = e.target as VisualElement;
            if (element == null) return;

            if (element.IsChildOf<ListView>())
            {
                e.StopImmediatePropagation();
            }
        }
    }
}
