using TMPro;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI display;
        [SerializeField] int refreshRate = 10;//frames per display update

        int frameCount;
        float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            frameCount++;
            if (frameCount >= refreshRate)
            {
                display.text = $"FPS: {(int)(frameCount / timer)}";
                frameCount = 0;
                timer = 0;
            }
        }
    }
}