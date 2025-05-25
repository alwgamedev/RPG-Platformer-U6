using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class GiantEel : MonoBehaviour
    {
        [SerializeField] EelVertex[] vertices;

        HorizontalOrientation currentOrientation;

        private void Start()
        {
            currentOrientation = HorizontalOrientation.right;

            for (int i = 1; i < vertices.Length; i++)
            {
                var d = 2 * ((i + 1) / 2 % 2) - 1;
                vertices[i].InitializeWiggle(d, d * (i % 2));
            }
        }

        private void Update()
        {
            UpdateWiggle(Time.deltaTime);
        }

        private void UpdateWiggle(float dt)
        {
            for (int i = 1; i < vertices.Length; i++)
            {
                vertices[i].UpdateWiggle(vertices[i - 1].transform.position, vertices[i - 1].WiggleTimer, 
                    currentOrientation, dt);
            }
        }
    }
}