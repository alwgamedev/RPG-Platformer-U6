using UnityEngine;
using System.Collections.Generic;
using RPGPlatformer.Core;

namespace RPGPlatformer.Environment
{
    public class EvilRootManager : MonoBehaviour
    {
        [SerializeField] Transform spawnMax;
        [SerializeField] Transform spawnMin;

        ObjectPool pool;
        System.Random rng = new();

        private void Awake()
        {
            pool = GetComponent<ObjectPool>();
        }

        //private void Start()
        //{
        //    foreach (var r in roots)
        //    {
        //        if (r)
        //        {
        //            r.gameObject.SetActive(false);
        //            dormant.Enqueue(r);
        //        }
        //    }
        //}

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.M))
        //    {
        //        DeployRoot();
        //    }
        //}

        //private void DeployRoot()
        //{
        //    if (dormant.Count != 0)
        //    {
        //        DeployRoot(dormant.Dequeue());
        //    }
        //}

        private void SpawnRoot()
        {
            var r = (EvilRoot)pool.GetObject();
            r.transform.SetParent(transform);

        }

        //private void DeployRoot(EvilRoot root)
        //{
        //    if (!root || root.gameObject.activeSelf)
        //        return;

        //    root.CycleComplete += CompletionHandler;
        //    root.BeforeSetActive();
        //    root.gameObject.SetActive(true);
        //    root.OnDeploy(root.transform.position.x > transform.position.x, GlobalGameTools.Instance.TokenSource.Token);

        //    void CompletionHandler()
        //    {
        //        root.gameObject.SetActive(false);
        //        dormant.Enqueue(root);
        //        root.CycleComplete -= CompletionHandler;
        //    }
        //}
    }
}