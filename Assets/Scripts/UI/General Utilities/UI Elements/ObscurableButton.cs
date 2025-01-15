using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class ObscurableButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] GameObject obscurer;
        [SerializeField] bool invertObscurerBehavior;

        public Button Button => button;

        private void Awake()
        {
            button.onClick.AddListener(() => OnClicked(true));
        }

        public void OnClicked(bool selected)
        {
            if(obscurer != null)
            {
                if(invertObscurerBehavior)
                {
                    selected = !selected;
                }
                obscurer.SetActive(!selected);
            }
        }

        //private void ToggleObscurer()
        //{
        //    if (obscurer == null) return;

        //    Obscure(!obscurer.activeSelf);
        //}
    }
}