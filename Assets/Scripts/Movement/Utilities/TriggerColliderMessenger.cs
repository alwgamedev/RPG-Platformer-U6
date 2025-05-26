using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class TriggerColliderMessenger : MonoBehaviour
    {
        Collider2D c;

        public Collider2D Collider
        {
            get
            {
                if (c == null)
                {
                    c = GetComponent<Collider2D>();
                }

                return c;
            }
        }

        public event Action<Collider2D> TriggerEnter;
        public event Action<Collider2D> TriggerStay;
        public event Action<Collider2D> TriggerExit;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy)
            {
                TriggerEnter?.Invoke(collider);
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy)
            {
                TriggerStay?.Invoke(collider);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy)
            {
                TriggerExit?.Invoke(collider);
            }
        }

        private void OnDestroy()
        {
            TriggerEnter = null;
            TriggerStay = null;
            TriggerExit = null;
        }
    }
}