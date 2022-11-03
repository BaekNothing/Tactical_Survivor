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
        job = new IdleJob(StaticObjects.Player.transform.position, handler.rigidbody.position, isFound, 10f);
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

        //SpreadOut();
        MoveToPlayer();
    }

    // rigidBody님이 해주시니까 필요없음.
    // void SpreadOut()
    // {
    //     if(handler.bundle.index == 0) return;

    //     Vector3 direction = handler.transform.position - 
    //         StaticObjects.processingMachine.objects[handler.bundle.index - 1].transform.position;
    //     if(direction.magnitude < handler.bundle.transform.localScale.x)
    //     {
    //         handler.transform.position += direction.normalized * (
    //             handler.bundle.transform.localScale.x - direction.magnitude);
    //     }
    // }

    void MoveToPlayer(){
        //Set Direction and Distance from here to player
        Vector3 direction = StaticObjects.Player.transform.position - handler.rigidbody.position;
        float rotate = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg - handler.rigidbody.rotation.y;
        float distance = Vector3.Distance(StaticObjects.Player.transform.position, handler.rigidbody.position) - distanceGap;

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
        // 6frame 정도 차이가 난다!!
        // rotateValue = action.ObjectRotate(handler.transform, rotateValue);
        // moveValue = action.ObjectMove(handler.transform, moveValue);

        // rotateValue = action.ObjectRotate(handler.rigidbody, rotateValue);
        // moveValue = action.ObjectMove(handler.rigidbody, moveValue);

        var result = action.ObjectMoveRotate(handler.rigidbody, moveValue, rotateValue);
        moveValue = result.Item1;
        rotateValue = result.Item2;
        
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
            handler.rigidbody.position) < attackRange)
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
        Utility.FindT<Animator>(handler.rigidbody.transform, "dummy").SetBool("hit", false);
    }

    public override void StateDoAction()
    {
        timer += Time.deltaTime * Utility.frameSkipCount;

        if(timer >= handler.duration)
        {
            if(Vector3.Distance(handler.rigidbody.position, StaticObjects.Player.transform.position) < attakRange)
            {
                Utility.FindT<Animator>(handler.rigidbody.transform, "dummy").SetBool("hit", true);
                Utility.FindT<Transform>(handler.rigidbody.transform, "attack").gameObject.SetActive(true);
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
        Vector3 direction = StaticObjects.Player.transform.position - handler.rigidbody.position;
        handler.rigidbody.transform.forward = direction;
        //moveValue = enemyAction.ObjectAddForce(handler.transform, moveValue);

        if(wasSetDeadAction) return;
        wasSetDeadAction = true;
        var particleTransform =
        Utility.FindT<Transform>(handler.rigidbody.transform, "vfx");
        Utility.FindT<Animator>(handler.rigidbody.transform, "dummy").SetBool("dead", true);
        EffectAction action = particleTransform.GetComponent<EffectAction>();
        if(action == null)
            action = particleTransform.gameObject.AddComponent<EffectAction>();
        action.stopAction = ()=>
        {
            enemyAction.Dispose();
            isDone = true;
            StaticObjects.processingMachine.RemoveBundle(handler.bundle);
            handler.CleanUpState();
            UnityEngine.Object.Destroy(handler.rigidbody.gameObject);
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

    public Tuple<actionValue, actionValue> ObjectMoveRotate(Rigidbody rigidbody, actionValue moveValue, actionValue rotateValue)
    {
        //Check
        if(moveValue.timer >= moveValue.endValue
            && rotateValue.timer >= rotateValue.endValue)
            return new Tuple<actionValue, actionValue>(moveValue, rotateValue);

        //Timer
        moveValue.timer += moveValue.speed;
        rotateValue.timer += rotateValue.speed;


        //Set
        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0, rotateValue.speed, 0));
            
        var direction = rigidbody.transform.forward + new Vector3(0, moveValue.speed, 0);
        direction.Normalize();
        direction *= moveValue.speed;

        //Action
        rigidbody.MovePosition(rigidbody.position + direction);

        return new Tuple<actionValue, actionValue>(moveValue, rotateValue);
    }

    public void Dispose(){
        if(_output.IsCreated)
            _output.Dispose();
    }

    /// <summary>
    /// it will add force to rigidbody by "value.speed * rigidbody.mass"
    /// </summary>
    public actionValue ObjectAddForce(Rigidbody rigidbody, Transform forceOrigin, actionValue value)
    {
        //Check
        if(value.timer >= value.endValue)
            return value;

        //Timer
        value.timer += value.speed;
        rigidbody.AddExplosionForce(value.speed, rigidbody.transform.position, rigidbody.mass * value.speed);

        return value;
    }
}

