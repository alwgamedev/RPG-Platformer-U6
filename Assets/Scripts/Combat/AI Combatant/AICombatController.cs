using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AICombatController : CombatController
    {
        public IHealth currentTarget;

        public CombatStateManager CombatManager => combatManager;

        public void FireOneShot()
        {
            Combatant.CheckIfTargetInRange(currentTarget, out _);
            RunAutoAbilityCycle(false);
        }

        public void StartAttacking()
        {
            CombatManager.OnWeaponTick += FireOneShot;
            combatant.Attack();
        }

        public void StopAttacking()
        {
            CombatManager.OnWeaponTick -= FireOneShot;
        }

        public override void OnCombatExit()
        {
            base.OnCombatExit();
            if (combatant is AICombatant aic && !combatant.Health.IsDead)
            {
                aic.damageTracker.ClearTracker();
            }
        }

        public override Vector2 GetAimPosition()
        {
            if (currentTarget != null)
            {
                return currentTarget.Transform.position;
            }
            return base.GetAimPosition();
        }

        protected override void Death()
        {
            base.Death();

            combatant.Health.Stat.statBar.gameObject.SetActive(false);
            Destroy(gameObject, 1.5f);
        }
    }
}