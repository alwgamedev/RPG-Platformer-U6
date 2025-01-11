using TMPro;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI tmp;
        [SerializeField] Animation anim;

        public void PlayDamageEffect(float damage)
        {
            transform.position += Random.Range(-.3f, .3f) * Vector3.right + Random.Range(0, .3f) * Vector3.up;
            transform.Rotate(Random.Range(-5, 5) * Vector3.forward);
            tmp.text = $"{(int)damage}";
            anim.Play();
        }

        //to be triggered by animation event
        public void DestroyPopup()
        {
            Destroy(gameObject);
        }
    }
}