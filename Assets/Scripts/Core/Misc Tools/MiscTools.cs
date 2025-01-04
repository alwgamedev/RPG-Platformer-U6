using RPGPlatformer.Combat;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace System.Runtime.CompilerServices//DO NOT DELETE!
{
    internal class IsExternalInit { }
}

public static class MiscTools
{
    public static async Task DelayGameTime(float time, CancellationToken token)
    {
        float timer = 0;
        while (timer < time)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            await Task.Yield();
            timer += Time.deltaTime;
        }
    }
}