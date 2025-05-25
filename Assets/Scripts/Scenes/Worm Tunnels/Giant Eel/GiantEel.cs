using UnityEngine;
using RPGPlatformer.Movement;
using RPGPlatformer.Core;
using UnityEngine.UIElements;

namespace RPGPlatformer.AIControl
{
    public class GiantEel : MonoBehaviour
    {
        [SerializeField] float vertexSpacing = .25f;
        [SerializeField] float turnSpeed = 2;
        [SerializeField] float changeDirectionThreshold = -0.125f;
        [SerializeField] float destinationToleranceSqrd = .01f;
        [SerializeField] float moveSpeed;
        [SerializeField] EelVertex[] vertices;
        [SerializeField] RandomizableVector2 movementBounds;

        LineRenderer lineRenderer;
        Vector2 moveDirection;
        Vector2 currentDestination;

        HorizontalOrientation currentOrientation => (HorizontalOrientation)Mathf.Sign(-lineRenderer.textureScale.y);

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            InitializeVertexPositions();
            InitializeWiggle();
            currentDestination = movementBounds.Value;
        }

        private void Update()
        {
            UpdateWiggle();
            LerpMoveDirection(currentDestination);
            UpdateMovement();
            if (HasReachedDestination(currentDestination))
            {
                currentDestination = movementBounds.Value;
            }
        }

        private void InitializeVertexPositions()
        {
            for (int i = 1; i < vertices.Length; i++)
            {
                vertices[i].transform.position = vertices[i - 1].transform.position - vertexSpacing * Vector3.right;
            }
        }

        private void InitializeWiggle()
        {
            for (int i = 1; i < vertices.Length; i++)
            {
                var d = 2 * ((i + 1) / 2 % 2) - 1;
                vertices[i].InitializeWiggle(d, d * (i % 2));
            }
        }

        private void UpdateWiggle()
        {
            for (int i = 1; i < vertices.Length; i++)
            {
                vertices[i].UpdateWiggle(vertices[i - 1].transform.position, vertices[i - 1].WiggleTimer, 
                    currentOrientation, Time.deltaTime);
            }
        }

        private void LerpMoveDirection(Vector2 destination)
        {
            Vector2 u = (destination - (Vector2)vertices[0].transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, u, Time.deltaTime * turnSpeed).normalized;
            vertices[0].VisualCurveGuidePoint.SetTangentDir(moveDirection);
            //or we could just have vertices[0] tang direction be 0 always
        }

        private void UpdateMovement()
        {
            Vector3 u;
            vertices[0].transform.position += Time.deltaTime * moveSpeed * (Vector3)moveDirection;
            for (int i = 1; i < vertices.Length; i++)
            {
                u = (vertices[i].transform.position - vertices[i - 1].transform.position).normalized;
                vertices[i].transform.position = vertices[i - 1].transform.position + vertexSpacing * u;
            }

            var d = vertices[0].transform.position.x - vertices[1].transform.position.x;
            if (d * (int)currentOrientation < changeDirectionThreshold)
            {
                ChangeOrientation();
            }
        }

        private void ChangeOrientation()
        {
            var s = lineRenderer.textureScale;
            s.y *= -1;
            lineRenderer.textureScale = s;
        }

        private bool HasReachedDestination(Vector2 destination)
        {
            return Vector2.SqrMagnitude((Vector2)vertices[0].transform.position - destination)
                < destinationToleranceSqrd;
        }
    }
}