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
        public bool HasChannelAnimation { get; init; } = false;
        public Func<ICombatController, CancellationTokenSource, Task<T>> Prepare { get; init; }
        //Very important: Prepare must cancel when game ends
        public new Action<ICombatController, T> OnExecute { get; init; }

        public override void Execute(ICombatController controller)
        {
            ExecuteAsync(controller, GlobalGameTools.Instance.TokenSource);
        }

        public async void ExecuteAsync(ICombatController controller, CancellationTokenSource tokenSource)
        {
            if (!CanExecuteAbility(controller)) return;

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

                controller.Combatant.Attack();
                OnExecute?.Invoke(controller, args);
                controller.OnAbilityExecute(this);

                PoolableEffect effect = GetCombatantExecuteEffect?.Invoke();
                if(effect)
                {
                    effect.PlayAtPosition(controller.Combatant.Transform);
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
                    controller.EndChannel();
                }
            }
        }

        public static void EndChannelIfTargetNotInRange(ICombatController controller, IHealth target)
        {
            controller.Combatant.CheckIfTargetInRange(target, out bool result);
            if (!result)
            {
                controller.EndChannel();
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

        public async Task<T> GetDataOnFireButtonDown(ICombatController controller, CancellationTokenSource tokenSource)
        {
            if (controller.FireButtonIsDown) return this.GetData(controller);

            TaskCompletionSource<T> tcs = new();
            CancellationTokenRegistration registration = tokenSource.Token.Register(Cancel);

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
                registration.Dispose();
            }
        }
    }
}