using RPGPlatformer.Movement;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(VisualCurveGuide))]
    public class RopeGenerator : GenericClimbableObject<RopeNode>
    {
        [SerializeField] int numNodes;
        [SerializeField] float spacing;
        [SerializeField] RopeNode nodePrefab;

        VisualCurveGuide vcg;

        protected override void Awake()
        {
            vcg = GetComponent<VisualCurveGuide>();
            GenerateNodes();
            Configure();
            vcg.SetGuidePoints(nodes.Select(x => x.CurveGuidePoint).ToArray());
        }

        private void GenerateNodes()
        {
            int groundLayer = LayerMask.GetMask("Ground");
            nodes = new RopeNode[numNodes];

            nodes[0] = Instantiate(nodePrefab, transform);
            nodes[0].Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            nodes[0].CapsuleCollider.enabled = false;

            for (int i = 1; i < numNodes; i++)
            {
                nodes[i] = Instantiate(nodePrefab, transform);
                nodes[i].gameObject.name = $"Rope Node {i}";
                nodes[i].transform.localPosition = -i * spacing * Vector3.up;
                var s = nodes[i].CapsuleCollider.size;
                s.y = spacing;
                nodes[i].CapsuleCollider.size = s; 
                var j = nodes[i].AddComponent<HingeJoint2D>();
                j.connectedBody = nodes[i - 1].Rigidbody;
                j.anchor = spacing * Vector2.up;
                j.breakAction = JointBreakAction2D.Ignore;
            }

            foreach (var n in nodes)
            {
                if (Physics2D.OverlapCircle(n.transform.position, spacing / 2, groundLayer))
                {
                    n.Rigidbody.excludeLayers |= groundLayer;
                }
            }
        }
    }
}