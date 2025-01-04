namespace RPGPlatformer.Inventory
{
    public interface IDosedItem
    {
        public int Doses { get; }
        public int DosesRemaining { get; }

        public void ConsumeDose();
    }
}