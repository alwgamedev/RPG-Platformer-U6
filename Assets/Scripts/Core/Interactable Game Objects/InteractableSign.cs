using UnityEngine;
using RPGPlatformer.UI;
using System.Collections.Generic;
using System;
using System.Collections;

namespace RPGPlatformer.Core
{
    public class InteractableSign : InteractableGameObject
    {
        [SerializeField] protected PopupWindow popupPrefab;
        [SerializeField] protected string title;
        [SerializeField] protected string content;//may want a custom editor with multiline text box

        protected PopupWindow activePopup;
        //in case we want to have any interaction with the popup while active (e.g. change text when a button pressed)
        //also to accurately know whether a popup is active (instead of just a bool)

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
                activePopup = null;
            }
        }

        protected override void OnLeftClick()
        {
            if (CanSpawnPopup())
            {
                SpawnPopup();
            }
        }
    }
}