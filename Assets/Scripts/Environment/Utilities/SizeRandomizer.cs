using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class SizeRandomizer : MonoBehaviour
    {
        [SerializeField] bool randomizeXScale;
        [SerializeField] float minXScale;
        [SerializeField] float maxXScale;
        [SerializeField] bool randomizeYScale;
        [SerializeField] float minYScale;
        [SerializeField] float maxYScale;
        [SerializeField] Transform parent;
        [SerializeField] Transform anchor;

        private void OnEnable()
        {
            Vector3 anchorPos = default;
            if (anchor && parent)
            {
                anchorPos = anchor.position;
            }

            RandomizeSize(minXScale, maxXScale, minYScale, maxYScale, anchorPos);
        }

        public void RandomizeSize(float minXScale, float maxXScale, float minYScale, float maxYScale,
            Vector3 anchorPos)
        {
            float xScale = transform.localScale.x;
            float yScale = transform.localScale.y;

            if (minXScale <= maxXScale && randomizeXScale)
            {
                xScale = Random.Range(minXScale, maxXScale);
            }

            if (minYScale <= maxYScale && randomizeYScale)
            {
                yScale = Random.Range(minYScale, maxYScale);
            }

            transform.localScale = new Vector3(xScale, yScale, transform.localScale.z);

            if (anchor && parent)
            {
                parent.localPosition += anchorPos - anchor.position;
            }
        }
    }
}