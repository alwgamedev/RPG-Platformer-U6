using RPGPlatformer.Combat;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    //because you can't call methods on children in an animation event
    public class PillBugContainer : MonoBehaviour
    {
        AICombatController cc;

        private void Start()
        {
            cc = GetComponentInChildren<AICombatController>();
        }

        public void ExecuteStoredAction()
        {
            cc.ExecuteStoredAction();
        }
    }
}