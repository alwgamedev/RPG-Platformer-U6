using RPGPlatformer.Combat;
using System;
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
            await Task.Yield();
            //previously I was checking token before the task.yield, so keep an eye out on whether this causes issues
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            timer += Time.deltaTime;
        }
    }

    public static async Task WaitUntil(Func<bool> p, float timeOut, CancellationToken token)
    {
        float timer = 0;
        while (!p() && timer < timeOut)
        {
            await Task.Yield();
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            timer += Time.deltaTime;
        }
    }
}