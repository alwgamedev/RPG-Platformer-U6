namespace RPGPlatformer.Movement
{
    public class ClimberMovementStateMachine : ClimberMovementStateMachine<ClimberMovementStateGraph>
    {
        public ClimberMovementStateMachine() : base() { }
    }

    public class ClimberMovementStateMachine<T> : AdvancedMovementStateMachine<T>
        where T : ClimberMovementStateGraph
    {
        public ClimberMovementStateMachine() : base() { }
    }

    public class Climbing : MoveState { }

    public class ClimberMovementStateGraph : AdvancedMovementStateGraph
    {
        public readonly Climbing climbing;

        public ClimberMovementStateGraph() : base()
        {
            climbing = CreateNewVertex<Climbing>();
            AddEdgeBothWaysForAll(climbing);
        }
    }
}