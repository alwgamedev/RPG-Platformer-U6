using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class MaterialManager : MonoBehaviour
    {
        [SerializeField] protected bool cloneMaterialOnStart = true;

        Material material;

        Dictionary<string, int> FloatPropertyID = new();
        Dictionary<string, int> VectorPropertyID = new();//color properties also get stored here

        //can add other property types (e.g. texture property) but for now don't need it

        protected virtual void Start()
        {
            InitializeMaterial();
        }

        private void InitializeMaterial()
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (cloneMaterialOnStart)
            {
                renderer.material = new Material(renderer.material);
            }
            material = renderer.material;

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
    }
}
