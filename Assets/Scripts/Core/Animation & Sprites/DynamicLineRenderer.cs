using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class DynamicLineRenderer : MonoBehaviour
    {
        [SerializeField] Transform[] transforms;

        LineRenderer lineRenderer;
        Vector3[] positions;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            UpdateLine();
        }

        private void UpdateLine()
        {
            if (transforms == null || transforms.Length == 0) return;

            if (positions == null || positions.Length != transforms.Length)
            {
                positions = new Vector3[transforms.Length];
            }

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i])
                {
                    positions[i] = transforms[i].position;
                }
            }

            if (lineRenderer.positionCount != transforms.Length)
            {
                lineRenderer.positionCount = transforms.Length;
            }

            lineRenderer.SetPositions(positions);
        }
    }
}