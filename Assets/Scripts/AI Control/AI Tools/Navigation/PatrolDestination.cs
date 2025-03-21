namespace RPGPlatformer.AIControl
{
    public class PatrolDestination : MbNavigationParameters
    {
        public override object Content => transform.position;
    }
}