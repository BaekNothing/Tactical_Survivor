using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading.Tasks;

public enum stateFlag
{
    idle,
    move,
    attack,
    dead
}

/// <summary>
/// Calculate in StateAction must be in IJob
/// </summary>
public class PrimitiveState
{
    public PrimitiveStateHandler handler {get; private set;}
    public stateFlag state {get; private set;}

    protected bool isReady = false;
    protected bool isDone = false;

    protected float timer = 0f;

    public void init(stateFlag state, PrimitiveStateHandler handler)
    {
        this.handler = handler;
        this.state = state;
    }

    public bool Ready()
    {
        return isReady;
    }

    public virtual void StateInit()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// When you override this function, you must set "strect : IJob" in SetJobAction()
    /// and handlerJob = strect.Schedule();
    /// </summary>
    public virtual void StateDoAction()
    {    
        // job = new Job();
        //handlerJob = job.Schedule();
        //handlerJob.Complete();
        throw new NotImplementedException();
    }

    public bool Done()
    {
        return isDone;
    }

    public virtual void StateEnd()
    {
        throw new NotImplementedException();
    }

    public virtual stateFlag GetNextState()
    {
        throw new NotImplementedException();
    }
}