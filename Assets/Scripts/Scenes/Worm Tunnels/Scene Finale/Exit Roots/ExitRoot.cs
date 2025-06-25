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

            float r = MiscTools.RandomFloat(-.1f, .1f);
            emergeGrowTime += r;
            retreatMoveTime -= r;
        }

        public override async Task Emerge(CancellationToken token)
        {
            await Emerge(true, false, false, token);
        }

        public override async Task Retreat(CancellationToken token)
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
                bool PlayerHeld()
                {
                    return RootHoldingPlayer;
                }

                FollowPlayer();
                await vcg.LerpLengthScale(dormantLengthScale, retreatMoveTime,
                token, PlayerHeld);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DeploymentComplete = null;
        }
    }
}