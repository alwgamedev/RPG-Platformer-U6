using TMPro;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class CombatantHealthBarCanvas : HidableUI
    {
        [SerializeField] TextMeshProUGUI tmp;
        [SerializeField] StatBarItem statBar;

        private void Start()
        {
            var cc = GetComponentInParent<ICombatController>();
            if (cc != null)
            {
                Configure(cc);
            }

            var mover = GetComponentInParent<IMover>();
            if(mover != null)
            {
                mover.UpdatedXScale += Unflip;
            }
        }

        public void Configure(ICombatController cc)
        {
            cc.Combatant.Health.Stat.statBar = statBar;
            tmp.text = $"{cc.Combatant.DisplayName} (Level {cc.Combatant.CombatLevel})";
            cc.CombatEntered += Show;
            cc.CombatExited += Hide;
        }

        private void Unflip(HorizontalOrientation orientation)
        {
            transform.localScale = new((int)orientation * transform.localScale.x, transform.localScale.y,
                transform.localScale.z);
        }
    }
}