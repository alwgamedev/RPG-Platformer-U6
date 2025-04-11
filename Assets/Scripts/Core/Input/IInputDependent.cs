namespace RPGPlatformer.Core
{
    public interface IInputDependent
    {
        public IInputSource InputSource { get; }

        public void InitializeInputSource();

        public void OnInputEnabled();

        public void OnInputDisabled();
    }
}