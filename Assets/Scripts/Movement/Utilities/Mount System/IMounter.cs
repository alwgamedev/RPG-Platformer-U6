namespace RPGPlatformer.Movement
{
    public interface IMounter
    {
        public IMountableEntity CurrentMount { get; }

        public void Mount(IMountableEntity entity);

        public void Dismount();

        public bool CompareTag(string tag);
    }
}