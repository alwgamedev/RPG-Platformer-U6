using RPGPlatformer.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public abstract class InteractableGameObject : MonoBehaviour, IInteractableGameObject, IPausable
    {
        int homeLayer;

        public bool MouseOver { get; protected set; }

        public static bool MouseOverInteractableGameObject { get; protected set; }

        public void Pause()
        {
            SetInteractable(false);
        }

        public void Unpause()
        {
            SetInteractable(true);
        }

        public void SetInteractable(bool val)
        {
            if(val)
            {
                gameObject.layer = homeLayer;
                
            }
            else
            {
                homeLayer = gameObject.layer;
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        private void OnMouseEnter()
        {
            MouseOver = true;
            MouseOverInteractableGameObject = true;
        }

        private void OnMouseExit()
        {
            MouseOver = false;
            if(!MouseOverAny())
            {
                MouseOverInteractableGameObject = false;
            }
        }

        private static bool MouseOverAny()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Physics2D.OverlapCircleAll(mousePosition, 0.05f, LayerMask.GetMask("Interactable Game Elements")).Length > 0)
            {
                return true;
            }
            return false;
        }

        protected virtual void OnDestroy()
        {
            if(Camera.main && !MouseOverAny())
            {
                MouseOverInteractableGameObject = false;
            }
        }
    }
}