using System;
using System.Threading;
using System.Threading.Tasks;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;

namespace RPGPlatformer.Combat
{
    //Description: waits for a Task<T> Prepare to complete, like aiming or powering up, then passes along the T result to the OnExecute method
    public class AsyncAbility<T> : AttackAbility
    {
        //public override bool IsAsyncAbility => true;
        public bool DelayedReleaseOfChannel { get; init; } = true;
        public bool HasChannelAnimation { get; init; } = false;
        public Func<ICombatController, CancellationTokenSource, Task<T>> Prepare { get; init; }
        //Prepare is responsible for cancelling itself when the token is cancelled
        public new Action<ICombatController, T> OnExecute { get; init; }

        public override void Execute(ICombatController controller)
        {
            ExecuteAsync(controller, GlobalGameTools.Instance.TokenSource);
        }

        public async void ExecuteAsync(ICombatController controller, CancellationTokenSource tokenSource)
        {
            if (!CanBeExecuted(controller)) return;

            //NOTE: although it would be convenient, we don't cancel OnChannelEnded here.
            //It is better to do that in the "innermost layer" of tasks (so in the Prepare function)
            //or else we get issues.
            //(Hence you need to make sure that every Prepare function you write cancels itself OnChannelEnded)

            try
            {
                controller.StartChannel();

                if (HasChannelAnimation)
                {
                    controller.PlayChannelAnimation(AnimationState, CombatStyle);
                }

                T args = await Prepare(controller, tokenSource);
                if (tokenSource.IsCancellationRequested || controller == null || !controller.ChannelingAbility)
                {
                    throw new TaskCanceledException();
                }
                //NOTE: Prepare function is responsible for throwing TaskCanceledException when cts is cancelled,
                //(but I'm still checking cts.IsCancellatioRequested after just to be safe)

                controller.Combatant.TriggerCombat();
                OnExecute?.Invoke(controller, args);
                controller.OnAbilityExecute(this);

                PoolableEffect effect = GetCombatantExecuteEffect?.Invoke();
                if(effect)
                {
                    effect.PlayAtPosition(controller.Combatant.transform);
                }

                UpdateCombatantStats(controller.Combatant);
            }
            catch (TaskCanceledException)
            {
                return;
                //ensures that if the channel is canceled, the ability does not execute.
            }
            finally
            {
                if (controller != null && controller.ChannelingAbility)
                { 
                    controller.EndChannel(DelayedReleaseOfChannel);
                }
            }
        }

        public static void EndChannelIfTargetNotInRange(ICombatController controller, IHealth target,
            bool delayedReleaseOfChannel)
        {
            if (!controller.Combatant.TargetInRange(target))
            {
                controller.EndChannel(delayedReleaseOfChannel);
            }
        }
    }

    //Description: an async ability that waits for the next fire button down to "GetData" from the controller (e.g. get aim position or target),
    //then invokes OnExecute immediately after.
    public class AbilityThatGetsDataOnNextFireButtonDownAndExecutesImmediately<T> : AsyncAbility<T>
    {
        public Func<ICombatController, T> GetData { get; init; }

        public AbilityThatGetsDataOnNextFireButtonDownAndExecutesImmediately() : base()
        {
            Prepare = GetDataOnFireButtonDown;
        }

        public async Task<T> GetDataOnFireButtonDown(ICombatController controller, 
            CancellationTokenSource tokenSource)
        {
            if (controller.FireButtonIsDown) return this.GetData(controller);

            TaskCompletionSource<T> tcs = new();
            using var registration = tokenSource.Token.Register(Cancel);

            void GetData()
            {
                tcs.TrySetResult(this.GetData(controller));
            }
            void Cancel()
            {
                tcs.TrySetCanceled();
            }

            try
            {
                controller.OnFireButtonDown += GetData;
                controller.OnChannelEnded += Cancel;
                return await tcs.Task;
            }
            finally
            {
                controller.OnFireButtonDown -= GetData;
                controller.OnChannelEnded -= Cancel;
            }
        }
    }
}