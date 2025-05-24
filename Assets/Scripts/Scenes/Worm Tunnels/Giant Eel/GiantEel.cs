using UnityEngine;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    public class GiantEel : MonoBehaviour
    {
        [SerializeField] EelVertex[] vertices;
        //[SerializeField] float testWiggleSpeed;
        //[SerializeField] float testWiggleMax;

        HorizontalOrientation currentOrientation;

        private void Start()
        {
            currentOrientation = HorizontalOrientation.right;

            for (int i = 1; i < vertices.Length; i++)
            {
                vertices[i].InitializeWiggleDirection(2 * (i % 2) - 1);
                //vertices[i].wiggleMax = testWiggleMax;
                //vertices[i].wiggleSpeed = testWiggleSpeed;
            }
        }

        private void Update()
        {
            UpdateWiggle(Time.deltaTime);
        }

        private void UpdateWiggle(float dt)
        {
            //exclude v[0] (head) from wiggle
            Vector2 u;
            for (int i = 1; i < vertices.Length - 1; i++)
            {
                u = (vertices[i - 1].transform.position - vertices[i + 1].transform.position).normalized;
                vertices[i].UpdateWiggle(u, currentOrientation, dt);
            }

            u = (vertices[^2].transform.position - vertices[^1].transform.position).normalized;
            vertices[^1].UpdateWiggle(u, currentOrientation, dt);
        }
    }
}