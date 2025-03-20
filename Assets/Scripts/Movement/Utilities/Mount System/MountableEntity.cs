using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MountableEntity : MonoBehaviour, IMountableEntity
    {
        [SerializeField] Rigidbody2D velocitySource;
        [SerializeField] Transform mountPlatformMax;
        [SerializeField] Transform mountPlatformMin;
        [SerializeField] TriggerColliderMessenger mountTrigger;
        [SerializeField] float localGravity;

        HorizontalOrientation currentOrientation;

        public Vector3 VelocitySourceTransformRight => velocitySource.transform.right;
        public Vector2 LocalGravity
            => (int)currentOrientation * localGravity 
            * ((Vector2)(mountPlatformMax.position - mountPlatformMin.position)).normalized.CCWPerp();
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

            if (mountTrigger)
            {
                mountTrigger.TriggerEnter += MountTriggerEnter;
                mountTrigger.TriggerExit += MountTriggerExit;
            }
        }

        private void VelocitySourceDirectionChanged(HorizontalOrientation o)
        {
            currentOrientation = o;
            DirectionChanged?.Invoke(o);
        }

        private void MountTriggerEnter(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IMounter mounter))
            {
                mounter.Mount(this);
            }
        }

        private void MountTriggerExit(Collider2D collider)
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