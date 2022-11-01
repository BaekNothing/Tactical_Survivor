using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBundle
{
    public int index;
    public Transform transform;
    public PrimitiveObject primitiveObject;

    public ObjectBundle(int index, Transform transform, PrimitiveObject primitiveObject)
    {
        this.transform = transform;
        this.primitiveObject = primitiveObject;
        primitiveObject.Init(this);
    }
}
