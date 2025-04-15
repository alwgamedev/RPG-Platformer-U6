using RPGPlatformer.Loot;
using RPGPlatformer.Effects;
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
        [SerializeField] InventoryItemSO glowingBranchSO;

        Highlighter highlighter;
        bool playerHasInteracted;
        InventoryItem glowingBranch;
        ILooter playerLooter;
        IInventoryOwner playerInventoryOwner;

        protected override void Awake()
        {
            base.Awake();

            highlighter = GetComponent<Highlighter>();
        }

        private void Start()
        {
            var player = GlobalGameTools.Instance.Player.Combatant;
            playerLooter = (ILooter)player;
            playerInventoryOwner = (IInventoryOwner)player;

            glowingBranch = glowingBranchSO.CreateInstanceOfItem();
        }

        public override IEnumerable<(string, Func<bool>, Action)> InteractionOptions()
        {
            yield return ($"Search {DisplayName}", PlayerCanInteract, Search);

            foreach (var option in base.InteractionOptions())
            {
                yield return option;
            }
        }

        public async void Search()
        {
            if (playerHasInteracted)
            {
                if (playerInventoryOwner.HasItem(glowingBranch))
                {
                    GameLog.Log("You already have a tree branch.");
                }
                else
                {
                    GiveTreeBranchToPlayer();
                }

                return;
            }


            SetInteractable(false);

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

                GiveTreeBranchToPlayer();
                SetInteractable(true);
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

        protected override void OnLeftClick()
        {
            Search();
        }

        private void GiveTreeBranchToPlayer()
        {
            playerLooter.TakeLoot(glowingBranch.ItemCopy().ToInventorySlotData(1));
            GameLog.Log("You take one of the branches for closer inspection...");
            GameLog.Log($"({glowingBranchSO.BaseData.DisplayName} has been placed in your inventory.)");
        }
    }
}
