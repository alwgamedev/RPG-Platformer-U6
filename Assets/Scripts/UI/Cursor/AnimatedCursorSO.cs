using UnityEngine;

namespace RPGPlatformer.UI
{
    [CreateAssetMenu(fileName = "Animated Cursor", menuName = "Custom UI Elements/Animated Cursor")]
    public class AnimatedCursorSO : ScriptableObject
    {
        public AnimatedCursor animation;
    }
}