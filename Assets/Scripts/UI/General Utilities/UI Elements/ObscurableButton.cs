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
            button.onClick.AddListener(ToggleObscurer);
        }

        public void Obscure(bool val)
        {
            if(obscurer != null)
            {
                if(invertObscurerBehavior)
                {
                    val = !val;
                }
                obscurer.SetActive(val);
            }
        }

        private void ToggleObscurer()
        {
            if (obscurer == null) return;

            Obscure(!obscurer.activeSelf);
        }
    }
}