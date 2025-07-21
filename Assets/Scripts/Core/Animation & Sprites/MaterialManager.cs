using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class MaterialManager : MonoBehaviour
    {
        [SerializeField] protected bool cloneMaterialOnStart = true;
        [SerializeField] protected bool isTMP;
        [SerializeField] protected bool getMainTexFromSpriteRenderer;

        Material material;

        //if you end up not using these at all, then get rid of them (free up memory)
        Dictionary<string, int> FloatPropertyID = new();
        Dictionary<string, int> VectorPropertyID = new();//color properties also get stored here

        public bool IsTMP => isTMP;

        //can add other property types (e.g. texture property) but for now don't need it

        protected virtual void Awake()
        {
            InitializeMaterial();
        }

        private void InitializeMaterial()
        {
            if (isTMP)
            {
                var tmp = GetComponentInChildren<TextMeshProUGUI>();
                if (cloneMaterialOnStart)
                {
                    tmp.fontSharedMaterial = new Material(tmp.fontSharedMaterial);
                }

                material = tmp.fontSharedMaterial;
            }
            else
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (cloneMaterialOnStart)
                {
                    renderer.material = new Material(renderer.material);
                }
                material = renderer.material;

                if (getMainTexFromSpriteRenderer && renderer is SpriteRenderer sr)
                {
                    material.mainTexture = sr.sprite.texture;
                    Debug.Log($"got main tex: {material.mainTexture.name}");
                }
            }

            var floatProps = material.GetPropertyNames(MaterialPropertyType.Float);
            var vectorProps = material.GetPropertyNames(MaterialPropertyType.Vector);

            foreach (var prop in floatProps)
            {
                FloatPropertyID[prop] = Shader.PropertyToID(prop);
            }

            foreach (var prop in vectorProps)
            {
                VectorPropertyID[prop] = Shader.PropertyToID(prop);
            }
        }

        public float GetFloat(string name)
        {
            return material.GetFloat(FloatPropertyID[name]);
        }

        public void SetFloat(string name, float val)
        {
            material.SetFloat(FloatPropertyID[name], val);
        }

        public Color GetColor(string name)
        {
            return material.GetColor(VectorPropertyID[name]);
        }

        public void SetColor(string name, Color color)
        {
            material.SetColor(VectorPropertyID[name], color);
        }

        //public Color GetFaceColor()
        //{
        //    return material.GetColor(ShaderUtilities.ID_FaceColor);
        //}

        //public void SetFaceColor(Color color)
        //{
        //    material.SetColor(ShaderUtilities.ID_FaceColor, color);
        //}
    }
}
