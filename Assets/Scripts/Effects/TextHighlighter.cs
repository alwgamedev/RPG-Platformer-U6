using UnityEngine;

namespace RPGPlatformer.Effects
{
    public class TextHighlighter : Highlighter
    {
        Color defaultFaceColor;

        protected override void Start()
        {
            defaultFaceColor = materialManager.GetColor("_FaceColor");

            base.Awake();
        }

        protected override void SetIntensity(float val)
        {
            materialManager.SetColor("_FaceColor", defaultFaceColor * val);
        }
    }
}