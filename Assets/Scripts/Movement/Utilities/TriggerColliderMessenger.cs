using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class TriggerColliderMessenger : MonoBehaviour
    {
        public event Action<Collider2D> TriggerEnter;
        public event Action<Collider2D> TriggerStay;
        public event Action<Collider2D> TriggerExit;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (gameObject.activeInHierarchy)
            {
                TriggerEnter?.Invoke(collision);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (gameObject.activeInHierarchy)
            {
                TriggerStay?.Invoke(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (gameObject.activeInHierarchy)
            {
                TriggerExit?.Invoke(collision);
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