using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class AutoSpriteShape : MonoBehaviour
    {
        [SerializeField] PolygonCollider2D shapeToCopy;
        [SerializeField] float tangentWeight = 1;

        public void GenerateSpline()
        {
            if (shapeToCopy != null && TryGetComponent(out SpriteShapeController ssc))
            {
                GenerateSpline(shapeToCopy, ssc, tangentWeight);
            }
            else
            {
                Debug.LogWarning($"{GetType().Name} on {gameObject.name} cannot generate sprite shape spline. " +
                    $"Collider or sprite shape controller missing.");
            }
        }

        public static void GenerateSpline(PolygonCollider2D c, SpriteShapeController ssc, float tangentWeight)
        {
            if (c == null || ssc == null)
            {
                Debug.LogWarning($"{typeof(AutoSpriteShape).Name} cannot generate sprite shape spline. " +
                    $"Collider or sprite shape controller missing.");
                return;
            }
            var pts = c.GetPath(0);
            int n = pts.Length;
            if (pts == null || n == 0)
            {
                Debug.LogWarning($"{typeof(AutoSpriteShape).Name} cannot generate sprite shape spline. " +
                    $"Collider path is null or empty.");
                return;
            }

            ssc.transform.rotation = c.transform.rotation;
            //so that in the future if you rotate c, then you can copy the same rotation to ssc, and they will match

            int m = ssc.spline.GetPointCount();
            for (int i = 0; i < m; i++)
            {
                ssc.spline.RemovePointAt(ssc.spline.GetPointCount() - 1);
                //remove all the points first, so we don't get "point too close to neighbor" errors
                //when setting a spline position
            }

            for (int i = 0; i < n; i++)
            {
                if (i == ssc.spline.GetPointCount())
                {
                    ssc.spline.InsertPointAt(i, pts[i]);
                }
                else
                {
                    ssc.spline.SetPosition(i, pts[i]);
                }

                var ip = i == n - 1 ? 0 : i + 1;
                var im = i == 0 ? n - 1 : i - 1;
                SplineUtility.CalculateTangents(pts[i], pts[im], pts[ip], ssc.transform.forward, tangentWeight,
                    out var r, out var l);
                ssc.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                ssc.spline.SetRightTangent(i, r);
                ssc.spline.SetLeftTangent(i, l);
            }

#if UNITY_EDITOR
            PrefabUtility.RecordPrefabInstancePropertyModifications(ssc);
#endif
        }
    }
}