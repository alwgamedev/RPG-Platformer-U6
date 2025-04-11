namespace RPGPlatformer.AIControl
{
    public class PatrolDestination : MBNavigationParameters
    {
        public override object Content => transform.position;
    }
}