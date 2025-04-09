using TMPro;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI tmp;
        [SerializeField] Color blockedColor;
        [SerializeField] Color healthGainedColor;
        [SerializeField] Animation anim;

        public void PlayDamageEffect(float damage)
        {
            transform.position += Random.Range(-.3f, .3f) * Vector3.right + Random.Range(0, .4f) * Vector3.up;
            transform.Rotate(Random.Range(-5, 5) * Vector3.forward);

            if (damage < 0)
            {
                damage = (int)-damage;
                tmp.color = healthGainedColor;
                tmp.text = $"+{damage}";
            }
            else
            {
                damage = (int)damage;

                if (damage == 0)
                {
                    tmp.color = blockedColor;
                    tmp.text = "BLOCKED";
                }
                else
                {
                    tmp.text = $"{damage}";
                }
            }

            anim.Play();
        }

        //to be triggered by animation event
        public void DestroyPopup()
        {
            Destroy(gameObject);
        }
    }
}