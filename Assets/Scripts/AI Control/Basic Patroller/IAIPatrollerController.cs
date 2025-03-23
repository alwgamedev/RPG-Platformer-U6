namespace RPGPlatformer.AIControl
{
    public interface IAIPatrollerController
    {
        public void BeginDefaultPatrol();

        public void BeginPatrolRest();

        public void BeginPatrol(NavigationMode mode, object param);
    }
}