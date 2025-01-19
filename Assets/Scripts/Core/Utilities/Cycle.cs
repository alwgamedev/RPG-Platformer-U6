using System;
using System.Collections;

namespace RPGPlatformer.Core
{
    public class Cycle<T> : IEnumerator
    {
        public T[] elements = Array.Empty<T>();
        int position = -1;

        public Cycle() { }

        public Cycle(T[] elements)
        {
            this.elements = elements;
        }

        public virtual object Current
        {
            get
            {
                if (elements.Length == 0) return null;
                return elements[position];
            }
        }

        public virtual bool MoveNext()
        {
            if (elements.Length == 0) return false;
            position++;
            position %= elements.Length;
            return true;
        }

        public virtual void Reset()
        {
            position = -1;
        }
    }

    public class ShootCycle<T> : Cycle<T>
    {
        public bool onCooldown;
        public bool itemQueued;

        public ShootCycle() : base() { }
        public ShootCycle(T[] elements) : base(elements) { }

        public override bool MoveNext()
        {
            if (itemQueued)
            {
                return false;
            }
            itemQueued = true;
            return base.MoveNext();
        }
        public bool CanFire()
        {
            return !onCooldown && itemQueued;
        }

        public virtual T Fire()//returns the "bullet" that was fired
        {
            itemQueued = false;
            onCooldown = true;
            return (T)base.Current;
        }

        public override void Reset()
        {
            base.Reset();
            onCooldown = false;
            itemQueued = false;
        }
    }
}