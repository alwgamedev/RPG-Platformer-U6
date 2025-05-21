using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class AnimationControl : MonoBehaviour//NOTE atm this has nothing to do with combat
    {
        [SerializeField] Animator animator;

        RuntimeAnimatorController defaultAnimatorController;
        //Animator animator;

        //trying to be performant by using id's instead of names
        //(if you give it name the animator will have to go find the id anyway, so cache ids here)
        //(useful e.g. for movement, which is setting a parameter every frame)
        Dictionary<string, int> Layers = new();
        Dictionary<string, int> States = new();
        Dictionary<string, int> Parameters = new();

        private void Awake()
        {
            if (!animator)
            {
                animator = GetComponent<Animator>();
            }

            defaultAnimatorController = animator.runtimeAnimatorController;
        }

        //lazy initialization of the id dictionaries
        private int LayerIndex(string layerName)
        {
            if (!Layers.TryGetValue(layerName, out var id))
            {

                id = animator.GetLayerIndex(layerName);
                //yes we need LayerIndex instead of hash id
                Layers[layerName] = id;
            }
            return id;
        }

        private int StateID(string stateName)
        {
            if (!States.TryGetValue(stateName, out var id))
            {
                id = Animator.StringToHash(stateName);
                States[stateName] = id;
            }

            return id;
        }

        private int ParameterID(string paramName)
        {
            if (!Parameters.TryGetValue(paramName, out var id))
            {
                id = Animator.StringToHash(paramName);
                Parameters[paramName] = id;
            }
            return id;
        }

        public bool HasState(string layer, string state)
        {
            return animator.HasState(LayerIndex(layer), StateID(state));
        }

        public AnimatorStateInfo CurrentStateInfo(string layer)
        {
            return animator.GetCurrentAnimatorStateInfo(LayerIndex(layer));
        }

        public bool GetBool(string name)
        {
            return animator.GetBool(ParameterID(name));
        }

        public void SetBool(string name, bool value)
        {
            animator.SetBool(ParameterID(name), value);
        }

        public float GetFloat(string name)
        {
            return animator.GetFloat(ParameterID(name));
        }

        public void SetFloat(string name, float value)
        {
            animator.SetFloat(ParameterID(name), value);
        }

        public void SetFloat(string name, float value, float dampTime, float deltaTime)
        {
            animator.SetFloat(ParameterID(name), value, dampTime, deltaTime);
        }

        public void SetTrigger(string name)
        {
            animator.SetTrigger(ParameterID(name));
        }

        public void ResetTrigger(string name)
        {
            animator.ResetTrigger(ParameterID(name));
        }

        public void SetRuntimeAnimatorController(RuntimeAnimatorController animController)
        {
            if (!animator || animController == null) return;

            //cache state and parameter values before swapping!

            AnimatorStateInfo[] layerInfo = new AnimatorStateInfo[animator.layerCount];

            for (int i = 0; i < animator.layerCount; i++)
            {
                layerInfo[i] = animator.GetCurrentAnimatorStateInfo(i);
            }

            Dictionary<int, float> floatParameters = new();
            Dictionary<int, bool> boolParameters = new();

            int id;

            foreach (var p in animator.parameters)
            {
                id = ParameterID(p.name);

                if (p.type == AnimatorControllerParameterType.Float)
                {
                    floatParameters[id] = animator.GetFloat(id);
                }
                else if (p.type == AnimatorControllerParameterType.Bool)
                {
                    boolParameters[id] = animator.GetBool(id);
                }
            }

            animator.runtimeAnimatorController = animController;
            //I believe setting to null will default back to the original controller


            for (int i = 0; i < animator.layerCount; i++)
            {
                animator.Play(layerInfo[i].fullPathHash, i, layerInfo[i].normalizedTime);
            }
            foreach (var p in floatParameters)
            {
                animator.SetFloat(p.Key, p.Value);
            }
            foreach (var p in boolParameters)
            {
                animator.SetBool(p.Key, p.Value);
            }

            animator.Update(0.0f);
        }

        public void RevertAnimatorOverride()
        {
            SetRuntimeAnimatorController(defaultAnimatorController);
        }

        public void PlayAnimationState(string stateName, string layerName, float normalizedTime)
        {
            animator.Play(StateID(stateName), LayerIndex(layerName), normalizedTime);
        }

        public void Freeze(bool val)//true => freeze animator, false => unfreeze animator
        {
            if (!animator) return;
            animator.enabled = !val;
        }
    }
}