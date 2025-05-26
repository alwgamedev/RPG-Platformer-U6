using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class CollisionMessenger : MonoBehaviour
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

        public event Action<Collision2D> CollisionEnter;
        public event Action<Collision2D> CollisionStay;
        public event Action<Collision2D> CollisionExit;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CollisionEnter?.Invoke(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            CollisionStay?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            CollisionExit?.Invoke(collision);
        }

        private void OnDestroy()
        {
            CollisionEnter = null;
            CollisionStay = null;
            CollisionExit = null;
        }
    }
}