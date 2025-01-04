//using UnityEngine;

//public class MobManager : MonoBehaviour
//{
//    [SerializeField] AIPatroller[] mob;

//    Transform player;

//    bool mobActive;

//    private void Start()
//    {
//        player = GameObject.Find("Player").transform;
//    }

//    void ActivateMob()
//    {
//        foreach (var minion in mob)
//        {
//            minion.gameObject.SetActive(true);
//        }
//        mobActive = true;
//    }

//    void DeactivateMob()
//    {
//        foreach(var minion in mob)
//        {
//            minion.gameObject.SetActive(false);
//        }
//    }
//}