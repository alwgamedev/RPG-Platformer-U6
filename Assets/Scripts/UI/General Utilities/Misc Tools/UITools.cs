using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public static class UITools
    {
        public static IEnumerator FadeIn(this CanvasGroup canvasGroup, float duration)
        {
            while(canvasGroup.alpha < 1)
            {
                yield return null;
                canvasGroup.alpha += Time.deltaTime / duration;
            }
            canvasGroup.alpha = 1;
        }

        public static IEnumerator FadeOut(this CanvasGroup canvasGroup, float duration)
        {
            while(canvasGroup.alpha > 0)
            {
                yield return null;
                canvasGroup.alpha -= Time.deltaTime / duration;
            }
            canvasGroup.alpha = 0;
        }

        public static string FormatLinesOfText(List<string> lines)
        {
            if (lines == null) return null;

            string text = "";

            for(int i = 0; i < lines.Count; i++)
            {
                if (lines[i] != null && lines[i].Length > 0)
                {
                    text += lines[i];
                }
                if(i < lines.Count - 1)
                {
                    text += "\n";
                }
            }

            return text;
        }

        public static float WorldWidth(this RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return corners[3].x - corners[0].y;
        }

        public static float WorldHeight(this RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return corners[1].y - corners[0].y;
        }

        public static void RepositionToFitInArea(this RectTransform rectTransform, RectTransform boundingRect,
            bool shiftByObjectDimensions = true)
            //shiftByObjectDimensions means e.g. that if the object is too far right, then the object will be
            //translated left by its WIDTH. When false, the object is translated the minimal amount necessary to get within bounds.
            //The object needs to be no larger than the boundingRect.
        {
            Canvas.ForceUpdateCanvases();//to make sure corners will be accurate

            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);
            Vector3[] boundingCorners = new Vector3[4];
            boundingRect.GetComponent<RectTransform>().GetWorldCorners(boundingCorners);

            CorrectHorizontalPosition();
            CorrectVerticalPosition();

            void CorrectHorizontalPosition()
            {
                if (objectCorners[3].x > boundingCorners[3].x)
                {
                    float shift = shiftByObjectDimensions ?
                        objectCorners[3].x - objectCorners[0].x
                        : objectCorners[3].x - boundingCorners[0].x;

                    rectTransform.transform.position -= shift * Vector3.right;
                }
                else if (objectCorners[0].x < boundingCorners[0].x)
                {
                    float shift = shiftByObjectDimensions ?
                        objectCorners[3].x - objectCorners[0].x
                        : boundingCorners[0].x - objectCorners[0].x;

                    rectTransform.transform.position += shift * Vector3.right;
                }
            }

            void CorrectVerticalPosition()
            {

                if (objectCorners[3].y < boundingCorners[3].y)
                {
                    float shift = shiftByObjectDimensions ?
                        objectCorners[1].y - objectCorners[0].y
                        : boundingCorners[3].y - objectCorners[3].y;

                    rectTransform.transform.position += shift * Vector3.up;
                }
                else if (objectCorners[1].y > boundingCorners[1].y)
                {
                    float shift = shiftByObjectDimensions ?
                        objectCorners[1].y - objectCorners[0].y
                        : objectCorners[1].y - boundingCorners[1].y;

                    rectTransform.transform.position -= shift * Vector3.up;
                }
            }
        }

        public static bool IsLeftMouseButtonEvent(this PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left;
        }

        public static bool IsRightMouseButtonEvent(this PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Right;
        }
    }
}