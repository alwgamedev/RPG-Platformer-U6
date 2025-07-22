using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class PolygonColliderRefresher : MonoBehaviour
    {
        public void GenerateCollider()
        {
            GenerateCollider(GetComponent<SpriteRenderer>(), GetComponent<PolygonCollider2D>());
        }

        public static void GenerateCollider(SpriteRenderer sr, PolygonCollider2D c)
        {
            if (sr == null || c == null)
                return;

            if (sr.sprite == null)
                return;

            var p = new List<Vector2>();
            sr.sprite.GetPhysicsShape(0, p);
            c.points = p.ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        }
    }
}