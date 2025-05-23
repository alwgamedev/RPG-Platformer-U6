using RPGPlatformer.Combat;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace System.Runtime.CompilerServices//DO NOT DELETE!
{
    internal class IsExternalInit { }
}

public static class MiscTools
{
    public static readonly System.Random rng = new();

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

    public static float RandomFloat(float min, float max)
    {
        return RandomFloat(min, max, rng);
    }

    //interpolates between min and max, so you don't actually need min <= max
    //(usefully e.g. for the vector application below)
    public static float RandomFloat(float min, float max, System.Random rng)
    {
        return (max - min) * (float)rng.NextDouble() + min;
    }

    public static Vector2 RandomPointInRectangle(Vector2 min, Vector2 max)
    {
        return RandomPointInRectangle(min, max, rng);
    }

    public static Vector2 RandomPointInRectangle(Vector2 min, Vector2 max, System.Random rng)
    {
        var x = RandomFloat(min.x, max.x, rng);
        var y = RandomFloat(min.y, max.y, rng);

        return new(x, y);
    }

    public static Vector3 RandomPointInBox(Vector3 min, Vector3 max)
    {
        return RandomPointInBox(min, max, rng);
    }

    public static Vector3 RandomPointInBox(Vector3 min, Vector3 max, System.Random rng)
    {
        var x = RandomFloat(min.x, max.x, rng);
        var y = RandomFloat(min.y, max.y, rng);
        var z = RandomFloat(min.z, max.z, rng);

        return new(x, y, z);
    }
}