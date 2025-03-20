using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class MountableEntity : MonoBehaviour, IMountableEntity
    {
        [SerializeField] Rigidbody2D velocitySource;
        [SerializeField] Transform mountPlatformMax;
        [SerializeField] Transform mountPlatformMin;
        //these will be used for "MountLevel" = normal to gravity direction.
        //just easier bc mount collider could be the child of child of... several different
        //things with rotation and local scales messed (we usually will want +/- transform.right
        //of the mount game object, but sorting out the +/- is not straightforward because sprites may
        //or may NOT have local scale flipped
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
        }

        private void VelocitySourceDirectionChanged(HorizontalOrientation o)
        {
            currentOrientation = o;
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