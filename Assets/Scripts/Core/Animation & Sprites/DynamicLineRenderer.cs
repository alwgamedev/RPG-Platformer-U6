using UnityEngine;

namespace RPGPlatformer.Core
{
    //not gonna make this ExecuteAlways, bc then we just unnecessarily complicate
    //things trying to avoid errors when fields aren't properly filled
    public class DynamicLineRenderer : MonoBehaviour
    {
        [SerializeField] Transform[] points;

        LineRenderer lineRenderer;
        Vector3[] positions;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = points.Length;
            positions = new Vector3[lineRenderer.positionCount];
        }

        private void Update()
        {
            UpdateLine();
        }

        private void UpdateLine()
        {
            for (int i = 0; i < points.Length; i++)
            {
                positions[i] = points[i].position;
            }

            lineRenderer.SetPositions(positions);
        }
    }
}