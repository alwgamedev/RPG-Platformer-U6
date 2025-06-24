using RPGPlatformer.Core;
using RPGPlatformer.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class ExitRootManager : EvilRootManager
    {
        [SerializeField] ColliderDrivenScenePortal scenePortal;
        //on separate transform with no collider attached (want to trigger manually, and this has a collider attached)

        bool hasDeployed;

        protected override void Update() { }

        protected override void OnPlayerEnter()
        {
            if (!hasDeployed)
            {
                hasDeployed = true;
                base.OnPlayerEnter();
                EvilRoot.PlayerGrabbed += OnPlayerGrabbed;
                DeployAll();
            }
        }

        private void DeployAll()
        {
            int n = pool.Available;
            for (int i = 0; i < n; i++)
            {
                DeployRoot();
            }
        }

        private void OnPlayerGrabbed()
        {
            EvilRoot.PlayerGrabbed -= OnPlayerGrabbed;
            var ex = EvilRoot.RootHoldingPlayer as ExitRoot;
            if (ex == null)
            {
                TriggerSceneTransition();
            }
            else
            {
                ex.DeploymentComplete += Complete;

                void Complete()
                {
                    if (ex)
                    {
                        ex.DeploymentComplete -= Complete;
                    }

                    TriggerSceneTransition();
                }
            }
        }

        private void TriggerSceneTransition()
        {
            if (!scenePortal.TransitionTriggered)
            {
                scenePortal.TriggerSceneTransition();
            }
        }
    }
}