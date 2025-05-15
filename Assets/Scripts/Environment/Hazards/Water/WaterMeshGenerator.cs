using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class WaterMeshGenerator : MonoBehaviour
    {
        [Min(.01f)][SerializeField] float halfWidth;
        [Min(.01f)][SerializeField] float halfHeight;
        [SerializeField] float surfaceColliderBuffer;
        [Min(2)][SerializeField] int numSprings;
        [SerializeField] float springConstant;
        [SerializeField] float dampingFactor;
        [SerializeField] float agitationScale;
        [SerializeField] float agitationPower = 1;//higher power allows you to agitate water more, and throw player less
        [SerializeField] float waveSpreadRate;
        [SerializeField] int waveSmoothingIterations;

        Spring1D[] springs;
        float[] deltas;//i -> springs[i + 1].disp - springs[i].disp

        float width;
        float height;
        float springSpacing;

        Mesh mesh;
        Vector3[] vertices;
        Vector3 v;

        private void OnValidate()
        {
            SetDimensions();
        }

        private void Start()
        {
            SetDimensions();
            InitializeSprings();
            GenerateMesh();
        }

        private void FixedUpdate()
        {
            UpdateSprings();
            PropagateWaves();
        }

        private void Update()
        {
            UpdateMeshVertices();
        }


        //INITIALIZATION

        private void SetDimensions()
        {
            width = 2 * halfWidth;
            height = 2 * halfHeight;

            numSprings = Mathf.Max(numSprings, 2);
            springSpacing = width / (numSprings - 1);

            if (TryGetComponent(out BoxCollider2D b))
            {
                b.size = new(width, height + surfaceColliderBuffer);
                b.offset = 0.5f * surfaceColliderBuffer * Vector2.up;
            }
        }

        private void InitializeSprings()
        {
            springs = new Spring1D[numSprings];
            for (int i = 0; i < numSprings; i++)
            {
                springs[i] = new();
            }

            deltas = new float[numSprings - 1];
        }

        public void GenerateMesh()
        {
            var meshFilter = GetComponent<MeshFilter>();

            mesh = new();

            vertices = new Vector3[2 * numSprings];
            var uv = new Vector2[2 * numSprings];
            var triangles = new int[6 * (numSprings - 1)];

            float x;
            float y = halfHeight;
            for (int i = 0; i < numSprings; i++)
            {
                x = -halfWidth + i * springSpacing;
                vertices[i] = new(x, y);
                vertices[i + numSprings] = new(x, -y);
            }

            for (int i = 0; i < numSprings; i++)
            {
                x = i / (numSprings - 1);
                uv[i] = new (x, 1);
                uv[numSprings + i] = new(x, 0);
            }

            for (int i = 0; i < numSprings - 1; i++)
            {
                triangles[6 * i] = i;
                triangles[6 * i + 1] = numSprings + i;
                triangles[6 * i + 2] = numSprings + i + 1;
                triangles[6 * i + 3] = numSprings + i + 1;
                triangles[6 * i + 4] = i + 1;
                triangles[6 * i + 5] = i;
                //all triangles oriented CCW for consistent surface normal
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            meshFilter.mesh = mesh;
        }


        //MESH

        private void UpdateMeshVertices()
        {
            for (int i = 0; i < numSprings; i++)
            {
                v = vertices[i];
                v.y = halfHeight + springs[i].Displacement;
                vertices[i] = v;
            }

            mesh.vertices = vertices;
        }


        //PHYSICS

        public void AgitateWater(float x, float y, float halfWidth, float velocityY)
        {
            x -= transform.position.x - this.halfWidth;

            if (x < 0 || x > width)
                return;

            int iMin = (int)Mathf.Clamp((x - halfWidth) / springSpacing, 0, numSprings - 1);
            int iMax = (int)Mathf.Clamp((x + halfWidth) / springSpacing, 0, numSprings - 1);

            velocityY = Mathf.Sign(velocityY) * Mathf.Pow(Mathf.Abs(velocityY), agitationPower);
            velocityY *= agitationScale * Time.deltaTime;

            for (int i = iMin; i <= iMax; i++)
            {
               PushSpring(i, velocityY);
            }
        }

        //world y-coord of wave at given world x-coord
        public float WaveYPosition(float x)
        {
            x -= transform.position.x - halfWidth;
            if (x < 0 || x > width)
            {
                return transform.position.y - halfHeight;
            }

            float d = x / springSpacing;
            int i = (int)d;
            if (i >= numSprings - 1)
            {
                return transform.position.y + vertices[i].y;
            }
            return transform.position.y +
                Mathf.Lerp(vertices[i].y, vertices[i + 1].y, d - i);
        }

        private void UpdateSprings()
        {
            foreach (var s in springs)
            {
                s.Update(springConstant, dampingFactor);
            }
        }

        private void PropagateWaves()
        {
            for (int i = 0; i < waveSmoothingIterations; i++)
            {
                for (int j = 1; j < numSprings; j++)
                {
                    deltas[j - 1] = waveSpreadRate * (springs[j].Displacement - springs[j - 1].Displacement);

                    springs[j - 1].ApplyAcceleration(deltas[j - 1]);
                    springs[j].ApplyAcceleration(-deltas[j - 1]);
                }

                for (int j = 1; j < numSprings; j++)
                {
                    springs[j - 1].ApplyVelocity(deltas[j - 1]);
                    springs[j].ApplyVelocity(-deltas[j - 1]);
                }
            }
        }

        private void PushSpring(int i, float dv)
        {
            springs[i].ApplyAcceleration(dv);
        }
    }
}