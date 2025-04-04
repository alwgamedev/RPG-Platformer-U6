using System;
using UnityEngine;

[Serializable]
public class SerializableTuple<T>
{
    [SerializeField] T item1;
    [SerializeField] T item2;

    public T Item1 => item1;
    public T Item2 => item2;

    public override bool Equals(object obj)
    {
        return obj is SerializableTuple<T> tuple 
            && tuple.item1.Equals(item1) 
            && tuple.item2.Equals(item2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(item1, item2);
    }
}