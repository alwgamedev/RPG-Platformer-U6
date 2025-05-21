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
        [SerializeField] float colliderWidth = .1f;
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
                nodes[i].CapsuleCollider.size = new(colliderWidth, spacing);
                nodes[i].CapsuleCollider.offset = new(0, spacing / 2);
                //else
                //{
                //    s.y = spacing / 2;
                //    nodes[i].CapsuleCollider.size = s;
                //    s = nodes[i].CapsuleCollider.offset;
                //    s.y = spacing / 4;
                //    nodes[i].CapsuleCollider.offset = s;
                //}
                var j = nodes[i].AddComponent<HingeJoint2D>();
                j.connectedBody = nodes[i - 1].Rigidbody;
                j.anchor = spacing * Vector2.up;
                j.breakAction = JointBreakAction2D.Ignore;
            }

            foreach (var n in nodes)
            {
                //no clue why n.CapsuleCollider.IsTouchingLayers(groundLayer) doesn't work
                if (Physics2D.OverlapCapsule(n.transform.position + 0.5f * spacing * Vector3.up,
                    n.CapsuleCollider.size, CapsuleDirection2D.Vertical, 0, groundLayer))
                {
                    n.Rigidbody.excludeLayers |= groundLayer;
                }
            }
        }
    }
}