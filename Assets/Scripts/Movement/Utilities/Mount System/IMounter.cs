using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMounter
    {
        public Transform transform { get; }
        public IMountableEntity CurrentMount { get; }

        public void Mount(IMountableEntity entity);

        public void Dismount();

        //public bool CompareTag(string tag);
    }
}