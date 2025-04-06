using System;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Effects;
using System.Threading;

namespace RPGPlatformer.Combat
{
    public interface IPowerUpAbility
    {
        public float MaxPowerUpMultiplier { get; }
    }

    //Instance needs to fill in:
    //(*) TicksToAchieveMaxPower
    //(*) PowerGainRate
    //(*) GetData function 
    //(*) GetDataOnPowerUpStarted bool
    //(*) OnPowerUpStarted
    public class PowerUpAbility<T> : AsyncAbility<(T, int)>, IPowerUpAbility
    {
        public bool HasPowerUpAnimation { get; init; }
        public bool GetDataOnPowerUpStarted { get; init; } //(whether you want to get data like aim position at the beginning or end of the power up)
        public Func<PoolableEffect> GetCombatantPowerUpEffect { get; init; }
        [Range(1, 100)] public int TicksToAchieveMaxPower { get; init; }//after this number of ticks, no more power will be gained from the power-up
        [Range(1, 100)] public float PowerGainRate { get; init; }//number of ticks to increase power multiplier by 1
        public float MaxPowerUpMultiplier => 1 + (TicksToAchieveMaxPower / PowerGainRate);
        public Func<ICombatController, T> GetData { get; init; } //(e.g.get aim position or get target Health)

        protected float ComputePowerMultiplier(int power)
        {
            return 1 + (power / PowerGainRate);
        }

        public PowerUpAbility() : base()
        {
            Prepare = PowerUp;
        }

        public async Task<(T, int)> PowerUp(ICombatController controller, CancellationTokenSource tokenSource)
        {
            TaskCompletionSource<(T, int)> tcs = new();
            using var registration = tokenSource.Token.Register(Cancel);

            int ticks = 0;
            T data = default;
            PoolableEffect effect = null;

            void SetResult()
            {
                if (!GetDataOnPowerUpStarted)
                {
                    data = GetData(controller);
                }
                tcs.TrySetResult((data, ticks));
            }

            void GainPower()
            {
                if (ticks < TicksToAchieveMaxPower)
                {
                    ticks++;
                }
                else
                {
                    controller.MaximumPowerAchieved();
                }
            }

            void StopGainingPower()
            {
                controller.TickTimer.NewTick -= GainPower;
            }

            void StartAbilityPowerUp()
            {
                controller.OnFireButtonUp += SetResult;
                controller.OnMaximumPowerAchieved += StopGainingPower;
                controller.TickTimer.NewTick += GainPower;
                controller.StartPowerUp(this);
                if (HasPowerUpAnimation)
                {
                    controller.PlayPowerUpAnimation(AnimationState, CombatStyle);
                }

                effect = GetCombatantPowerUpEffect?.Invoke();
                if(effect)
                {
                    effect.PlayAtPosition(controller.Combatant.transform);
                }

                if (GetDataOnPowerUpStarted)
                {
                    data = GetData(controller);
                }
            }

            void Cancel()
            {
                tcs.SetCanceled();
                //TaskCancelledException will be caught by the underlying AsyncAbility
                //(it is important that it's caught there instead of in the PowerUp,
                //b/c if we catch it here the AsyncAbility will not know it was cancelled)
            }

            try
            {
                if(controller.FireButtonIsDown)
                {
                    StartAbilityPowerUp();
                }
                else
                {
                    controller.OnFireButtonDown += StartAbilityPowerUp;
                }
                controller.OnChannelEnded += Cancel;
                return await tcs.Task;
            }
            finally
            {
                controller.OnFireButtonDown -= StartAbilityPowerUp;
                controller.OnFireButtonUp -= SetResult;
                controller.OnChannelEnded -= Cancel;
                controller.OnMaximumPowerAchieved -= StopGainingPower;
                controller.TickTimer.NewTick -= GainPower;

                if (controller != null && controller.PoweringUp)
                {
                    controller.EndPowerUp();
                }

                if (effect && !effect.RequeAutomaticallyAtEndOfEffect)
                {
                    effect.Stop();
                }
            }
        }
    }
}