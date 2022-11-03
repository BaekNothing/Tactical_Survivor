using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading.Tasks;


public class EnemyObject : PrimitiveObject
{    
    NativeArray<System.Numerics.Vector3> output;

    JobHandle handler;
    List<addVector> updaters = new List<addVector>();
    // PhysicsData data;

    public override void Init(ObjectBundle bundle){
        this.transform = bundle.transform;
        Dictionary<stateFlag, Type> stateMap = new Dictionary<stateFlag, Type>(){
            {stateFlag.idle, typeof(EnemyIdle)},
            {stateFlag.move, typeof(EnemyMove)},
            {stateFlag.attack, typeof(EnemyAttack)},
            {stateFlag.dead, typeof(EnemyDead)}
        };

        stateHandler.Init(stateMap, "", bundle);
        //stateHandler.SetTransform(transform);
        stateHandler.SetRigidbody(transform.GetComponent<Rigidbody>());
        stateHandler.Action();

        transform.position = new Vector3(UnityEngine.Random.Range(2f, 10f), 
            0.5f,
            UnityEngine.Random.Range(2f, 10f));
    }
}