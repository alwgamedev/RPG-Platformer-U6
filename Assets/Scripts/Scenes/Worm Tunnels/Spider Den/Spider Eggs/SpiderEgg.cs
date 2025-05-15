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
        [SerializeField] Rigidbody2D babySpider;
        [SerializeField] float spiderSpawnAccel;


        bool fractured;
        //System.Random rng = new();

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.M))
        //    {
        //        GlobalGameTools.Instance.PlayerTransform.position = transform.position + 0.5f * Vector3.up
        //            - 2 * Vector3.right;
        //    }
        //}

        private void Start()
        {
            if (babySpider)
            {
                babySpider.gameObject.SetActive(false);
            }
        }

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

            await Shake(GlobalGameTools.Instance.TokenSource.Token);

            topPiece.Break();
            leftPiece.Break();
            rightPiece.Break();

            SpawnBabySpider();

            Destroy(gameObject, timeToDestroyAfterBreak);
        }

        private void SpawnBabySpider()
        {
            if (babySpider)
            {
                babySpider.transform.SetParent(null);
                babySpider.gameObject.SetActive(true);
                babySpider.AddForce(babySpider.mass * spiderSpawnAccel *
                    new Vector2(MiscTools.RandomFloat(-0.5f, 0.5f)/*(float)rng.NextDouble() - .5f*/, 1), 
                    ForceMode2D.Impulse);
            }
        }

        private async Task Shake(CancellationToken token)
        {
            var shakeForce = topPiece.Rb.mass * this.shakeForce * Vector2.right;

            var n = MiscTools.rng.Next(numShakes / 2, 2 * numShakes);
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