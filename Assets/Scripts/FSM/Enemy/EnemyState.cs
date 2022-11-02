using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading.Tasks;

public class EnemyIdle : PrimitiveState
{
    struct IdleJob : IJob
    {
        Vector3 player;
        Vector3 thisTransform;
        NativeArray<bool> isFound;
        float distance;

        public IdleJob(Vector3 player, Vector3 thisTransform, NativeArray<bool> isFound, float distance)
        {
            this.player = player;
            this.thisTransform = thisTransform;
            this.isFound = isFound;
            this.distance = distance;
        }

        public void Execute()
        {
            if (Vector3.Distance(player, thisTransform) < distance)
            {
                isFound[0] = true;
            }
            else 
            {
                isFound[0] = false;
            }
        }
    }
    IdleJob job;
    NativeArray<bool> isFound;
    public override void StateInit()
    {
        handler.delay = 0.1f;
        handler.duration = UnityEngine.Random.Range(1, 3);
        timer = 0;

        isDone = false;
        isReady = true;
    }

    public override void StateDoAction()
    {
        JobHandle jobHandle;
        //Timer
        timer += Time.deltaTime * Utility.frameSkipCount;

        NativeArray<bool> isFound = new NativeArray<bool>(1, Allocator.TempJob);
        //Set
        job = new IdleJob(StaticObjects.Player.transform.position, handler.transform.position, isFound, 10f);
        jobHandle = job.Schedule();
        jobHandle.Complete();

        if(isFound[0])
            timer = handler.duration;

        //Check
        if(timer >= handler.duration)
            isDone = true;

        isFound.Dispose();
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

public class EnemyMove : PrimitiveState
{
    EnemyAction.actionValue rotateValue;
    EnemyAction.actionValue moveValue; 

    float distanceGap = 5f;
    float distanceMax = 20f;

    public override void StateInit()
    {
        handler.delay = 0;
        handler.duration = 0;
        timer = 0;

        isReady = true;
        isDone = false;

        SpreadOut();
        MoveToPlayer();
    }

    void SpreadOut()
    {
        if(handler.bundle.index == 0) return;

        Vector3 direction = handler.transform.position - 
            StaticObjects.processingMachine.objects[handler.bundle.index - 1].transform.position;
        if(direction.magnitude < handler.bundle.transform.localScale.x)
        {
            handler.transform.position += direction.normalized * (
                handler.bundle.transform.localScale.x - direction.magnitude);
        }
    }

    void MoveToPlayer(){
        //Set Direction and Distance from here to player
        Vector3 direction = StaticObjects.Player.transform.position - handler.transform.position;
        float rotate = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg - handler.transform.eulerAngles.y;
        float distance = Vector3.Distance(StaticObjects.Player.transform.position, handler.transform.position) - distanceGap;

        //May Not fount player, move to random direction
        if(distance > distanceMax)
        {
            distance = UnityEngine.Random.Range(3f, 10f);
            rotate = UnityEngine.Random.Range(-90, 90f);
        }
            
        //Face to player
        rotateValue = new EnemyAction.actionValue(rotate, 5f);
        moveValue = new EnemyAction.actionValue(distance, 0.5f);
    }

    EnemyAction action = new EnemyAction();

    public override void StateDoAction()
    {
        rotateValue = action.ObjectRotate(handler.transform, rotateValue);
        moveValue = action.ObjectMove(handler.transform, moveValue);

        //Check
        if(rotateValue.timer >= rotateValue.endValue
            && moveValue.timer >= moveValue.endValue)
            isDone = true;
        action.Dispose();
    }

    public override void StateEnd()
    {
        isReady = false;
    }

    public override stateFlag GetNextState()
    {
        if(CheckCanAtack())
            return stateFlag.attack;
        else
            return stateFlag.idle;
    }

    float attackRange = 5f;

    bool CheckCanAtack()
    {
        if(Vector3.Distance(StaticObjects.Player.transform.position, 
            handler.transform.position) < attackRange)
            return true;
        else
            return false;
    }
}

public class EnemyAttack : PrimitiveState
{
    float attakRange = 5f;

    EnemyAction action = new EnemyAction();
    public override void StateInit()
    {
        handler.delay = 0.1f;
        handler.duration = 3f;
        timer = 0;

        isDone = false;
        isReady = true;
        Utility.FindT<Animator>(handler.transform, "dummy").SetBool("hit", false);
    }

    public override void StateDoAction()
    {
        timer += Time.deltaTime * Utility.frameSkipCount;

        if(timer >= handler.duration)
        {
            if(Vector3.Distance(handler.transform.position, StaticObjects.Player.transform.position) < attakRange)
            {
                Utility.FindT<Animator>(handler.transform, "dummy").SetBool("hit", true);
                Utility.FindT<Transform>(handler.transform, "attack").gameObject.SetActive(true);
                StaticObjects.PlayerDataObject.hp--;
            }
            isDone = true;
        }
    }

    public override void StateEnd()
    {
        isReady = false;
    }

    public override stateFlag GetNextState()
    {
        action.Dispose();
        return stateFlag.move;
    }
}

public class EnemyDead : PrimitiveState
{
    EnemyAction enemyAction = new EnemyAction();
    EnemyAction.actionValue moveValue;

     public override void StateInit()
    {
        moveValue = new EnemyAction.actionValue(0, 5f);
        wasSetDeadAction = false;
        isReady = true;
    }

    bool wasSetDeadAction = false;

    public override void StateDoAction()
    {
        //Face to player
        Vector3 direction = StaticObjects.Player.transform.position - handler.transform.position;
        handler.transform.forward = direction;
        //moveValue = enemyAction.ObjectAddForce(handler.transform, moveValue);

        if(wasSetDeadAction) return;
        wasSetDeadAction = true;
        var particleTransform =
        Utility.FindT<Transform>(handler.transform, "vfx");
        Utility.FindT<Animator>(handler.transform, "dummy").SetBool("dead", true);
        EffectAction action = particleTransform.GetComponent<EffectAction>();
        if(action == null)
            action = particleTransform.gameObject.AddComponent<EffectAction>();
        action.stopAction = ()=>
        {
            enemyAction.Dispose();
            isDone = true;
            StaticObjects.processingMachine.RemoveBundle(handler.bundle);
            handler.CleanUpState();
            UnityEngine.Object.Destroy(handler.transform.gameObject);
        };
        particleTransform.transform.gameObject.SetActive(true);
    }

    public override stateFlag GetNextState()
    {
        enemyAction.Dispose();
        return stateFlag.dead;
    }
}




public class EnemyAction
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

    public actionValue ObjectAddForce(Transform transform, actionValue value)
    {
        float looseForce = 1f;
        //Check
        if(value.speed <= 0f)
            return value;

        //Timer
        value.speed -= looseForce * Time.deltaTime;
        JobHandle jobHandle;

        //Set
        addVector updater = new addVector(
            transform.position, 
            -transform.forward * value.speed, output);

        //Action
        jobHandle = updater.Schedule();
        jobHandle.Complete();
        
        transform.position = Utility.ConvertSysToEngineVector(output[0]);
        return value;
    }
}

