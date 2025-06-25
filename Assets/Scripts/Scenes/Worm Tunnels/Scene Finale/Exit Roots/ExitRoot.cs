using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class ExitRoot : EvilRoot
    {
        public event Action DeploymentComplete;

        protected override void Awake()
        {
            base.Awake();

            //float r = MiscTools.RandomFloat(-.1f, .1f);
            //emergeGrowTime += r;
            //retreatMoveTime -= r;
        }

        public override async Task Emerge(float emergeTime, CancellationToken token)
        {
            await Emerge(emergeTime, true, false, false, token);
            //do cancel once one grabs so that they all begin retreat at the same time
        }

        public override async Task Retreat(CancellationToken token, float timeScale = 1)
        {
            if (RootHoldingPlayer == this)
            {
                await base.Retreat(token);
                ReleasePlayer(Vector2.zero);
                RootHoldingPlayer = null;
                DeploymentComplete?.Invoke();//manager will trigger scene transition
            }
            else
            {
                FollowPlayer();
                await base.Retreat(token, timeScale = 1.5f);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DeploymentComplete = null;
        }
    }
}