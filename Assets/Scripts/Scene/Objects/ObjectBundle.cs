using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBundle
{
    public int index;
    public Rigidbody rigidbody;
    public PrimitiveObject primitiveObject;

    public ObjectBundle(int index, Rigidbody rigidbody, PrimitiveObject primitiveObject)
    {
        this.rigidbody = rigidbody;
        this.primitiveObject = primitiveObject;
        primitiveObject.Init(this);
    }
}
