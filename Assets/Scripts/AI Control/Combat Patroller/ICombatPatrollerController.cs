namespace RPGPlatformer.AIControl
{
    public interface ICombatPatrollerController : IAIPatrollerController 
    {
        public ICombatPatroller CombatPatroller => (ICombatPatroller)Patroller;
    }
}