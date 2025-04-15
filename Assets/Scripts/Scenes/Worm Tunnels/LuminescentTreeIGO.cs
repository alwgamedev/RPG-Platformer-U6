using RPGPlatformer.Loot;
using RPGPlatformer.Effects;
using RPGPlatformer.Combat;
using RPGPlatformer.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Core
{
    public class LuminescentTreeIGO : InteractableGameObject
    {
        [SerializeField] InventoryItemSO glowingBranchWeapon;

        Highlighter highlighter;
        bool playerHasInteracted;
        ILooter player;

        protected override void Awake()
        {
            base.Awake();

            highlighter = GetComponent<Highlighter>();
        }

        private void Start()
        {
            player = (ILooter)GlobalGameTools.Player.Combatant;
        }

        public override IEnumerable<(string, Func<bool>, Action)> InteractionOptions()
        {
            yield return ($"Search {DisplayName}", PlayerCanInteract, async () => await Search());

            foreach (var option in base.InteractionOptions())
            {
                yield return option;
            }
        }

        public async Task Search()
        {
            if (playerHasInteracted)
            {
                //if player has a branch, GameLog.Log("u don't need dat")
                //else give another branch
            }

            var tcs = new TaskCompletionSource<object>();

            void Cancel()
            {
                tcs.TrySetCanceled();
            }

            void Complete()
            {
                tcs.TrySetResult(null);
            }

            using var reg = GlobalGameTools.Instance.TokenSource.Token.Register(Cancel);

            try
            {
                playerHasInteracted = true;
                GameLog.Log("A magical glow radiates from the tree's branches...");

                highlighter.HighlightTweenComplete += Complete;
                highlighter.EnableHighlight(true);
                await tcs.Task;

                player.TakeLoot(glowingBranchWeapon.CreateInstanceOfItem().ToInventorySlotData(1));
                GameLog.Log("You take one of the branches for closer inspection.");
                GameLog.Log($"({glowingBranchWeapon.BaseData.DisplayName} has been placed in your inventory.)");
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                highlighter.HighlightTweenComplete -= Complete;
            }
        }
    }
}
