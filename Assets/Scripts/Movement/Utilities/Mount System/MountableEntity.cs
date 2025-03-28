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
        [SerializeField] bool checkTriggerStay;

        HorizontalOrientation currentOrientation;
        Vector2 lastVelocity;

        public Vector3 VelocitySourceTransformRight => velocitySource.transform.right;
        public Vector2 LocalGravity
            => - (int)currentOrientation * localGravity 
            * ((Vector2)(mountPlatformMax.position - mountPlatformMin.position)).normalized.CCWPerp();
        public Vector3 Position => velocitySource.transform.position;
        public Vector2 Velocity => velocitySource.linearVelocity;

        public event Action<HorizontalOrientation> DirectionChanged;
        public event Action<IMounter> Mounted;
        public event Action<IMounter> MountStay;
        public event Action<IMounter> Dismounted;
        //public event Action<Vector2> ChangeInVelocity;
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

        //private void FixedUpdate()
        //{
        //    Debug.Log($"lastVelocity before {lastVelocity}");
        //    ChangeInVelocity?.Invoke(Velocity - lastVelocity);
        //    lastVelocity = Velocity;
        //    Debug.Log($"lastVelocity after {lastVelocity}");
        //}

        public void SetLocalGravity(float localGravity)
        {
            this.localGravity = localGravity;
        }

        public void EnableTriggerStay(bool val)
        {
            if (val == checkTriggerStay) return;

            checkTriggerStay = val;

            if (val)
            {
                mountTrigger.TriggerStay += MountTriggerStay;
            }
            else
            {
                mountTrigger.TriggerStay -= MountTriggerStay;
            }
        }

        private void VelocitySourceDirectionChanged(HorizontalOrientation o)
        {
            currentOrientation = o;
            DirectionChanged?.Invoke(o);
        }

        private void MountTriggerEnter(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IMounter mounter) 
                && mounter.CurrentMount != (IMountableEntity)this)
            {
                mounter.Mount(this);
                Mounted?.Invoke(mounter);
            }
        }

        private void MountTriggerStay(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IMounter mounter))
            {
                if (mounter.CurrentMount != (IMountableEntity)this)
                { 
                    mounter.Mount(this);
                    Mounted?.Invoke(mounter);
                }
                else
                {
                    MountStay?.Invoke(mounter);
                }
            }
        }

        private void MountTriggerExit(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IMounter mounter) 
                && mounter.CurrentMount == (IMountableEntity)this)
            {
                mounter.Dismount();
                Dismounted?.Invoke(mounter);
            }
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke();

            DirectionChanged = null;
            Mounted = null;
            MountStay = null;
            Dismounted = null;
            //ChangeInVelocity = null;
            Destroyed = null;

        }
    }
}