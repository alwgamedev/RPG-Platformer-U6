using RPGPlatformer.Movement;
using System;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class RopeGenerator
    {
        [SerializeField] int nodes;
        [SerializeField] float spacing;
        [SerializeField] ClimbableObject nodePrefab;

        VisualCurveGuide vcg;
        float spacing2;

        private void GenerateRope()
        {
            spacing2 = spacing * spacing;
        }
    }
}