using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedSaveButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] Image checkMarkImage;

    Animator animator;
    Button button;

    public Button Button
    {
        get
        {
            if (!button)
            {
                button = GetComponent<Button>();
            }

            return button;
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    public void PrepareForAnimation()
    {
        buttonText.gameObject.SetActive(false);
        checkMarkImage.gameObject.SetActive(true);
    }

    public void ReturnToNormalState()//will get triggered by animation event
    {
        buttonText.gameObject.SetActive(true);
        checkMarkImage.gameObject.SetActive(false);
    }

    public void PlayEffect()
    {
        animator.Play("Effect", 0);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
