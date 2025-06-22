using UnityEngine;
using RPGPlatformer.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using RPGPlatformer.Effects;
using RPGPlatformer.Saving;

namespace RPGPlatformer.Core
{
    public class InteractableSign : InteractableGameObject
    {
        [SerializeField] protected PopupWindow popupPrefab;
        [SerializeField] protected string title;
        [SerializeField] protected string content;//may want a custom editor with multiline text box
        [SerializeField] protected bool highlightBeforeFirstInteraction;

        protected PopupWindow activePopup;
        protected Highlighter highlighter;

        protected override void Awake()
        {
            base.Awake();
            highlighter = GetComponent<Highlighter>();
            SavingSystem.SceneLoadComplete += OnSceneLoadComplete;
        }

        //make sure saving system finishes load before we do this
        //(since load is asynchronous we can't guarantee it's done before Start)
        protected void OnSceneLoadComplete()
        {
            if (highlighter && !PlayerHasInteracted && highlightBeforeFirstInteraction)
            {
                OnUpdate += HighlightIfPlayerInRange;
            }

            SavingSystem.SceneLoadComplete -= OnSceneLoadComplete;
        }

        public override IEnumerable<(string, Func<bool>, Action)> InteractionOptions()
        {
            if (CanSpawnPopup())
            {
                yield return ($"Read {DisplayName}", CanSpawnPopup, SpawnPopup);
            }

            foreach (var option in base.InteractionOptions())
            {
                yield return option;
            }
        }

        protected virtual bool CanSpawnPopup()
        {
            return !activePopup;
        }

        protected virtual void SpawnPopup()
        {
            SpawnPopup(title, content);
        }

        protected virtual void SpawnPopup(string title, string content)
        {
            activePopup = Instantiate(popupPrefab, GameHUD.Instance.transform);
            activePopup.Configure(title, content);
            activePopup.Show();

            OnUpdate = SendNotificationIfPlayerOutOfRange;
            PlayerOutOfRange += PlayerOutOfRangeHandler;
            activePopup.Destroyed += DestroyedHandler;

            void PlayerOutOfRangeHandler()
            {
                Destroy(activePopup.gameObject);
            }

            void DestroyedHandler()
            {
                OnUpdate = null;
                PlayerOutOfRange -= PlayerOutOfRangeHandler;
                activePopup.Destroyed -= DestroyedHandler;
                if (highlighter && highlighter.HighlightActive)
                {
                    highlighter.EnableHighlight(false);
                }
                activePopup = null;
            }
        }

        protected override void OnFirstLeftClick()
        {
            base.OnFirstLeftClick();

            OnUpdate -= HighlightIfPlayerInRange;
            //if (highlighter && highlighter.HighlightActive)
            //{
            //    highlighter.EnableHighlight(false);
            //}
        }

        protected override void OnLeftClick()
        {
            if (CanSpawnPopup())
            {
                SpawnPopup();
            }
        }

        protected void HighlightIfPlayerInRange()
        {
            var p = PlayerInRange();
            if (p && !highlighter.HighlightActive)
            {
                highlighter.EnableHighlight(true);
            }
            else if (!p && highlighter.HighlightActive)
            {
                highlighter.EnableHighlight(false);
            }
        }
    }
}