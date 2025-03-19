using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MountableEntity : MonoBehaviour, IMountableEntity
    {
        [SerializeField] Rigidbody2D velocitySource;

        public Vector3 TransformRight => velocitySource.transform.right;
        public Vector3 Position => velocitySource.transform.position;
        public Vector2 Velocity => velocitySource.linearVelocity;

        public event Action<HorizontalOrientation> DirectionChanged;
        public event Action Destroyed;

        private void Awake()
        {
            if (velocitySource && velocitySource.gameObject.TryGetComponent(out IMover mover))
            {
                mover.DirectionChanged += VelocitySourceDirectionChanged;
            }
        }

        private void VelocitySourceDirectionChanged(HorizontalOrientation o)
        {
            DirectionChanged?.Invoke(o);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IMounter mounter))
            {
                mounter.Mount(this);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IMounter mounter) 
                && mounter.CurrentMount == (IMountableEntity)this)
            {
                mounter.Dismount();
            }
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke();

            DirectionChanged = null;
            Destroyed = null;

        }
    }
}