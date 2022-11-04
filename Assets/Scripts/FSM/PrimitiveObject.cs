using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading.Tasks;

public class SampleInit : PrimitiveState
{    
    struct InitJob : IJob
    {
        public void Execute()
        {
        }
    }
    InitJob job;
    public override void StateInit()
    {
        handler.delay = 0.1f;
        handler.duration = 3f;
        timer = 0;

        isDone = false;
        isReady = true;
    }

    public override void StateDoAction()
    {
        JobHandle jobHandle;
        //Timer
        timer += Time.deltaTime * Utility.frameSkipCount;

        //Set
        job = new InitJob();
        jobHandle = job.Schedule();
        jobHandle.Complete();

        //Check
        if(timer >= handler.duration)
            isDone = true;
    }

    public override void StateEnd()
    {
        isReady = false;
    }

    public override stateFlag GetNextState()
    {
        return stateFlag.move;
    }
}


public class ObjectAction
{
    public class actionValue
    {
        public float endValue;
        public float speed;
        public float timer;

        public actionValue(float endValue, float speed)
        {
            this.endValue = endValue;
            this.speed = speed;
            this.timer = 0;
        }
    }

    NativeArray<System.Numerics.Vector3> _output;
    NativeArray<System.Numerics.Vector3> output
    {
        get{
            if(!_output.IsCreated)
                _output = new NativeArray<System.Numerics.Vector3>(1, Allocator.Persistent);
            return _output;
        }
    }

    public actionValue ObjectRotate(Transform transform, actionValue value)
    {
        //Check
        if(value.timer >= value.endValue)
            return value;

        //Timer
        value.timer += value.speed;
        JobHandle jobHandle;

        //Set
        
        addVector updater = new addVector(
            transform.rotation.eulerAngles, 
            new Vector3(0, value.speed, 0), output);

        //Action
        jobHandle = updater.Schedule();
        jobHandle.Complete();
        transform.rotation = Quaternion.Euler(
            Utility.ConvertSysToEngineVector(output[0]));
        return value;
    }

    public actionValue ObjectMove(Transform transform, actionValue value)
    {    
        //Check
        if(value.timer >= value.endValue)
            return value;

        //Timer
        value.timer += value.speed;
        JobHandle jobHandle;

        //Set
        addVector updater = new addVector(
            transform.position, 
            transform.forward * value.speed, output);

        //Action
        jobHandle = updater.Schedule();
        jobHandle.Complete();
        
        transform.position = Utility.ConvertSysToEngineVector(output[0]);

        return value;
    }

    public void Dispose(){
        if(_output.IsCreated)
            _output.Dispose();
    }
}

public class SampleMove : PrimitiveState
{
    ObjectAction.actionValue rotateValue;
    ObjectAction.actionValue moveValue; 

    public override void StateInit()
    {
        handler.delay = 0;
        handler.duration = 0;
        timer = 0;

        isReady = true;
        isDone = false;

        rotateValue = new ObjectAction.actionValue(UnityEngine.Random.Range(10f, 90f), 5f);
        moveValue = new ObjectAction.actionValue(UnityEngine.Random.Range(10f, 90f), 0.5f);
    }

    ObjectAction action = new ObjectAction();

    public override void StateDoAction()
    {
        // this is regacy code when I was trying to use job system and not use rigidbody
        // rigidbody is much easier to use & faster than job system witch use with transform

        // rotateValue = action.ObjectRotate(handler.rigidbody, rotateValue);
        // moveValue = action.ObjectMove(handler.rigidbody, moveValue);

        // //Check
        // if(rotateValue.timer >= rotateValue.endValue
        //     && moveValue.timer >= moveValue.endValue)
        //     isDone = true;
        // action.Dispose();
        isDone = true;
    }

    public override void StateEnd()
    {
        isReady = false;
    }

    public override stateFlag GetNextState()
    {
        return stateFlag.idle;
    }
}

public class SampleDead : PrimitiveState
{
    public override void StateInit()
    {
        Debug.Log(this);
    }

    public override void StateDoAction()
    {
        var particleTransform =
        Utility.FindT<Transform>(handler.rigidbody.transform, "vfx");
        EffectAction action = particleTransform.GetComponent<EffectAction>();
        if(action == null)
            action = particleTransform.gameObject.AddComponent<EffectAction>();
        action.stopAction = ()=>
        {
            UnityEngine.Object.Destroy(handler.rigidbody.gameObject);
            handler.CleanUpState();
        };
        particleTransform.transform.gameObject.SetActive(true);
    }

    public override stateFlag GetNextState()
    {
        return stateFlag.dead;
    }
}

public class PrimitiveObject
{    
    NativeArray<System.Numerics.Vector3> output;

    JobHandle handler;
    List<addVector> updaters = new List<addVector>();

    public PrimitiveStateHandler stateHandler = new PrimitiveStateHandler();
    // PhysicsData data;
    public Rigidbody rigidbody;
    public System.Action customAction;

    public virtual void Init(ObjectBundle bundle){
        this.rigidbody = bundle.rigidbody;
        Dictionary<stateFlag, Type> stateMap = new Dictionary<stateFlag, Type>(){
            {stateFlag.idle, typeof(SampleInit)},
            {stateFlag.move, typeof(SampleMove)},
            {stateFlag.dead, typeof(SampleDead)}
        };

        stateHandler.Init(stateMap, "", bundle);
        //stateHandler.SetTransform(transform);
        stateHandler.SetRigidbody(rigidbody);
        //stateHandler.SetPhysicsData(data, transform);
        stateHandler.Action();
    }

    public void SetsPosition(Vector3 position){
        rigidbody.MovePosition(rigidbody.position + position);
    }

    public void Action() => stateHandler.Action();
    

    public void SetState (stateFlag flag) => stateHandler.SetCurrentState(flag);
}