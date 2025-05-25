using UnityEngine;
using System.Collections.Generic;
using System;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class CurveRenderer : MonoBehaviour//, ISerializationCallbackReceiver
    {
        [Min(0)][SerializeField] int lineRendererPositionCount = 75;
        [SerializeField] CurveGuidePoint[] guidePoints;//doesn't need to be serialized?
        [SerializeField] bool lerpMode;//can be removed?
        [SerializeField] float lerpRate = 5;
        [SerializeField] float tangentWeight = 1;
        //note: numPointsDrawn is independent of pointData.Length.
        //pointData determines the curve we want to draw and numPointsDrawn
        //is number of points drawn along that curve.

        Vector3[] goalPositions;
        Vector3[] lineRendererPositions;//used as storage to get and set lineRenderer positions
        LineRenderer lineRenderer;
        Queue<CurveGuidePoint> transferQueue = new();
        int numVirtualSegments;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = lineRendererPositionCount;
        }

        private void Start()
        {
            SnapToGoalPositions();
        }

        private void Update()
        {
            if (lerpMode)
            {
                LerpTowardGoalPositions();
            }
            else
            {
                SnapToGoalPositions();
            }
        }

        private void OnValidate()
        {
            if (!lineRenderer)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            if (lineRenderer)
            {
                lineRenderer.positionCount = lineRendererPositionCount;
            }
        }

        public void HandleGuidePointChanges(VisualCurveGuidePoint[] guides)
        {
            SetGuidePoints(guides);
            SetGoalPositions();
            //if (!lerpMode)
            //{
            //    RedrawCurve();
            //}
        }

        private void SetGuidePoints(VisualCurveGuidePoint[] guides)
        {
            if (guides == null) return;

            //point of transfer queue?
            //because we don't know how many guide points are actually active yet
            //and we want to resize the array as rarely as possible
            transferQueue.Clear();//transferQueue should already be empty, but just in case

            numVirtualSegments = guides.Length - 1;
            int virtualSegs = 0;
            for (int i = 0; i < guides.Length; i++)
            {
                var guide = guides[i];
                if (guide && guide.Active())
                {
                    transferQueue.Enqueue(new CurveGuidePoint(virtualSegs, guide.Point(), 
                        guide.TangentDir() * tangentWeight));
                    virtualSegs = 1;
                }
                else
                {
                    virtualSegs++;
                }
            }

            if (guidePoints == null || guidePoints.Length != transferQueue.Count)
            {
                guidePoints = new CurveGuidePoint[transferQueue.Count];
            }

            int j = 0;
            while(transferQueue.Count != 0)
            {
                guidePoints[j] = transferQueue.Dequeue();
                j++;
            }
        }

        private void SetGoalPositions()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            if (guidePoints == null || guidePoints.Length < 2) return;

            if (goalPositions == null || goalPositions.Length != lineRendererPositionCount)
            {
                goalPositions = new Vector3[lineRendererPositionCount];
            }

            var dt = 1 / (float)(lineRenderer.positionCount - 1);//time per segment drawn
            var ds = 1 / (float)numVirtualSegments;//time per length unit
            var dl = dt / ds;//number lengthUnits covered per segment drawn
            float s = 0;//time to reach beginning of current segment
            int n = guidePoints.Length - 1;//total number path segments
            int j = 0;//path segments covered
            float l = 0;//length units covered within the current path segment


            for (int i = 0; i < lineRendererPositionCount; i++)
            {
                if (i == lineRendererPositionCount - 1)
                {
                    goalPositions[i] = guidePoints[^1].point;//^1 = length - 1 (last elmt of arry)
                    break;
                }

                while (j < n && l >= guidePoints[j + 1].virtualSegments)
                {
                    l -= guidePoints[j + 1].virtualSegments;
                    s += guidePoints[j + 1].virtualSegments * ds;
                    j++;
                }

                if (j >= n)
                {
                    break;
                }

                var a = (i * dt - s) / (guidePoints[j + 1].virtualSegments * ds);
                goalPositions[i] = CurveTools.PathWithTangents(a, guidePoints[j].point,
                    guidePoints[j].velocity, guidePoints[j + 1].point, guidePoints[j + 1].velocity);

                l += dl;//add the virtual segments covered by the new drawn segment
            }
        }

        private void LerpTowardGoalPositions()
        {
            if (goalPositions == null) return;

            GetLineRendererPositions();

            int m = Math.Min(lineRendererPositions.Length, goalPositions.Length);

            for (int i = 0; i < m; i++)
            {
                lineRendererPositions[i] = Vector3.Lerp(lineRendererPositions[i], goalPositions[i],
                    lerpRate * Time.deltaTime);
            }

            lineRenderer.SetPositions(lineRendererPositions);
        }

        private void SnapToGoalPositions()
        {
            if (goalPositions == null) return;
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            if (lineRenderer.positionCount != goalPositions.Length)
            {
                lineRenderer.positionCount = goalPositions.Length;
            }
            lineRenderer.SetPositions(goalPositions);
        }

        private void GetLineRendererPositions()
        {
            if (lineRendererPositions == null || lineRendererPositions.Length != lineRenderer.positionCount)
            {
                lineRendererPositions = new Vector3[lineRenderer.positionCount];
            }

            lineRenderer.GetPositions(lineRendererPositions);
        }
    }
}