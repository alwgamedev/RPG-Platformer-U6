using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class SpiderWebBarrier : MonoBehaviour
    {
        [SerializeField] LineRenderer topRenderer;
        [SerializeField] LineRenderer bottomRender;
        [SerializeField] HingeJoint2D[] joints;
        [SerializeField] Joint2D breakPointTop;
        [SerializeField] Joint2D breakPointBottom;
        [SerializeField] Vector2 topBreakAcceleration;
        [SerializeField] Vector2 bottomBreakAcceleration;
        [SerializeField] float snapTime;

        Rigidbody2D breakPointTopRb;
        Rigidbody2D breakPointBottomRb;

        Collider2D[] colliders;

        bool broken;
        bool snapping;
        float snapTimer;
        Vector2[] initialSnapAnchors;

        private void Awake()
        {
            breakPointTopRb = breakPointTop.GetComponent<Rigidbody2D>();
            breakPointBottomRb = breakPointBottom.GetComponent<Rigidbody2D>();

            colliders = new Collider2D[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i].TryGetComponent(out Collider2D c))
                {
                    colliders[i] = c;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                GlobalGameTools.Instance.Player.Combatant.transform.position = breakPointBottom.transform.position
                    - 2 * Vector3.right;
            }

            if (!broken && Input.GetKeyDown(KeyCode.P))
            {
                Break();
            }
            else if (snapping)
            {
                snapTimer -= Time.deltaTime;
                if (snapTimer <= 0)
                {
                    snapping = false;
                    Destroy(gameObject);
                    return;
                }

                var q = snapTime / snapTimer;
                q = q * q * q;
                q = q * q;
                var s = topRenderer.textureScale;
                topRenderer.textureScale = new(s.x, q);
                s = bottomRender.textureScale;
                bottomRender.textureScale = new(s.x, q);

                /*r*/
                q = Mathf.Max(1 / q, .2f);

                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i].anchor = q * initialSnapAnchors[i];
                }
            }
        }

        private void Break()
        {
            breakPointBottom.enabled = false;
            //breakJointRb.AddForce(breakJointRb.mass * breakAcceleration);

            initialSnapAnchors = new Vector2[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {
                //j.anchor = Vector2.zero;
                initialSnapAnchors[i] = joints[i].anchor;
                joints[i].autoConfigureConnectedAnchor = false;
                joints[i].useLimits = true;
                if (colliders[i])
                {
                    colliders[i].enabled = false;
                }
                //j.anchor = Vector2.zero;
            }

            broken = true;
            snapping = true;
            snapTimer = snapTime;

            breakPointTopRb.AddForce(breakPointTopRb.mass * topBreakAcceleration, ForceMode2D.Impulse);
            breakPointBottomRb.AddForce(breakPointBottomRb.mass * bottomBreakAcceleration, ForceMode2D.Impulse);
        }
    }
}