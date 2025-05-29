using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.SceneManagement
{
    public class PerformanceGroup : MonoBehaviour
    {
        [SerializeField] RandomizableVector2 bounds;
        [SerializeField] GameObject[] gameObjects;

        enum State
        {
            uninitialized, groupActive, groupInactive
        }

        State state = State.uninitialized;

        //(halfWidth, halfHeight) of camera view in world space
        Vector2 cameraSize => 
            new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);

        //don't want to do a start, just in case it messes with any starts on the game objects

        private void Update()
        {
            switch (state)
            {
                case State.uninitialized:
                    SetGroupActive(PlayerInBounds());
                    break;
                case State.groupActive:
                    if (!PlayerInBounds())
                    {
                        SetGroupActive(false);
                    }
                    break;
                case State.groupInactive:
                    if (PlayerInBounds())
                    {
                        SetGroupActive(true);
                    }
                    break;
            }
        }

        private void SetGroupActive(bool val)
        {
            state = val ? State.groupActive : State.groupInactive;
            foreach (var go in gameObjects)
            {
                if (go)
                {
                    go.SetActive(val);
                }
            }
        }

        private bool PlayerInBounds()
        {
            return bounds.PointIsInBounds(GlobalGameTools.Instance.PlayerTransform.position, cameraSize);
        }
    }
}
