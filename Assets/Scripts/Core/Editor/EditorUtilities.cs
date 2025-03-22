using UnityEngine.UIElements;

namespace RPGPlatformer.Core.Editor
{
    public static class EditorUtilities
    {
        public static bool IsChildOf<T>(this VisualElement v) where T : VisualElement
        {
            VisualElement currentParent = v.parent;
            while (currentParent != null)
            {
                if (currentParent is T) return true;

                currentParent = currentParent.parent;
            }

            return false;
        }
    }
}