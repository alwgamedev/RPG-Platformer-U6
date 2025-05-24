using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class GiantEel : MonoBehaviour
    {
        //vertices go from back to front of eel (so v[0] is tail of eel, v[^1] is nose of eel)
        [SerializeField] EelVertex vertices;
    }
}