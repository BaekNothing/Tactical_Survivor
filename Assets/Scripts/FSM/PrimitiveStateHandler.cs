using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading.Tasks;


/// <summary>
/// This class is used to handle the state of the primitive object
/// Each handler should run in each thread 
/// </summary>
public class PrimitiveStateHandler
{
    public ObjectBundle bundle;
    public float delay = 0f;
    public float duration = 0f;

    public class stateFlagComparer : IEqualityComparer<stateFlag> 
    {
        public bool Equals(stateFlag x, stateFlag y) {
            return x == y;
        }
    
        public int GetHashCode(stateFlag x) {
            return (int)x;
        }
    }
    
    static stateFlagComparer comparer = new stateFlagComparer();
    Dictionary<stateFlag, PrimitiveState> states = new Dictionary<stateFlag, PrimitiveState>(comparer);
    PrimitiveState currentState;
    // public PhysicsData physicsData {get; private set;}
    // public Transform transform {get; private set;}
    public Rigidbody rigidbody {get; private set;}

    string sharedData;

    public void Init(Dictionary<stateFlag, Type> stateTypes, string sharedData, ObjectBundle bundle)
    {
        foreach (var stateType in stateTypes)
        {
            PrimitiveState state = (PrimitiveState)Activator.CreateInstance(stateType.Value);
            state.init(stateType.Key, this);
            states.Add(stateType.Key, state);
        }
        currentState = states[0];

        this.sharedData = sharedData;
        this.bundle = bundle;
    }

    // public void SetTransform(Transform transform){
    //     this.transform = transform;
    // }

    public void SetRigidbody(Rigidbody rigidbody){
        this.rigidbody = rigidbody;
    }

    //Frame skip may be slightly slower than the actual time
    public void Action()
    {
        if(!currentState.Ready())
            currentState.StateInit();
        currentState.StateDoAction();
        if(currentState.Done())
        {
            currentState.StateEnd();
            SetCurrentState(currentState.GetNextState());
        }
    }

    // public bool IsTransformVisible(Camera camera)
    // {
    //     var planes = GeometryUtility.CalculateFrustumPlanes(camera);
    //     var point = transform.position;
    //     return GeometryUtility.TestPlanesAABB(planes, 
    //         new Bounds(point, transform.localScale));
    // }

    public void SetCurrentState(stateFlag flag, System.Action action = null)
    {
        duration = 0;
        delay = 0;
        currentState = states[flag];
        action?.Invoke();
    }

    public void CleanUpState()
    {
        foreach (var state in states)
        {
            state.Value.GetNextState();
        }
    }
}
