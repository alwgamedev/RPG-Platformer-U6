using RPGPlatformer.Core;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace RPGPlatformer.Effects
{
    public class SpiderEgg : MonoBehaviour
    {
        [SerializeField] SpiderEggFragment topPiece;
        [SerializeField] SpiderEggFragment leftPiece;
        [SerializeField] SpiderEggFragment rightPiece;
        [SerializeField] int numShakes;
        [SerializeField] float shakeFrequency;
        [SerializeField] float shakeForce;
        [SerializeField] float shakeEscalationRate;
        [SerializeField] float timeToDestroyAfterBreak;

        bool fractured;

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.M))
        //    {
        //        GlobalGameTools.Instance.PlayerTransform.position = transform.position + 0.5f * Vector3.up
        //            - 2 * Vector3.right;
        //    }
        //}

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!enabled || !gameObject.activeInHierarchy) return;

            if (!fractured && collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                Fracture();
            }
        }

        private async void Fracture()
        {
            fractured = true;

            //topPiece.Fracture();
            //leftPiece.Fracture();
            //rightPiece.Fracture();

            await Shake(GlobalGameTools.Instance.TokenSource.Token);

            topPiece.Break();
            leftPiece.Break();
            rightPiece.Break();

            Destroy(gameObject, timeToDestroyAfterBreak);
        }

        private async Task Shake(CancellationToken token)
        {
            var shakeForce = topPiece.Rb.mass * this.shakeForce * Vector2.right;

            for (int i = 0; i < numShakes; i++)
            {
                if (i == 1)
                {
                    shakeForce *= 2;
                }
                topPiece.Rb.AddForce((shakeEscalationRate * i + 1) * (2 * (i % 2) - 1) 
                    * shakeForce, ForceMode2D.Impulse);
                await MiscTools.DelayGameTime(shakeFrequency, token);
            }
        }
    }
}