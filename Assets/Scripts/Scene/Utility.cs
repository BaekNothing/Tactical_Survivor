using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Threading.Tasks;
using UnityEngine.UI;

public static class Utility
{
    public static readonly int frameSkipCount = 2;

    public static void SetActionInButton(Button button, Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action());
    }

    public static void ALog(string msg){
#if UNITY_EDITOR
        UnityEngine.Debug.Log(msg);
#endif
    }

    public static T ALog<T>(string msg) where T : class{
#if UNITY_EDITOR
        UnityEngine.Debug.Log(msg);
#endif
        return null;
    }
    
    public static UnityEngine.Vector3 ConvertSysToEngineVector(System.Numerics.Vector3 vector){
        return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static System.Numerics.Vector3 ConvertEngineToSysVector(UnityEngine.Vector3 vector){
        return new System.Numerics.Vector3(vector.x, vector.y, vector.z);
    }

    public static T FindT<T>(UnityEngine.Transform transform, string name) where T : UnityEngine.Component
    {
        T t = null;
        for(int i = 0; i < transform.childCount; i++){
            t = transform.GetChild(i).GetComponent<T>();
            if(t && t.transform.name.ToLower().Contains(name.ToLower()))
                return t;
        }
        return null;
    }
}

[BurstCompile]
public struct addVector : IJob
{
    public Vector3 input;
    public Vector3 target;

    [WriteOnly] public NativeArray<Vector3> output;

    public addVector(UnityEngine.Vector3 input, 
                     UnityEngine.Vector3 target, 
                     NativeArray<Vector3> output){
        this.input = Utility.ConvertEngineToSysVector(input);
        this.target = Utility.ConvertEngineToSysVector(target);
        this.output = output;
    }

    public addVector(Vector3 input, Vector3 target, NativeArray<Vector3> output)
    {
        this.input = input;
        this.target = target;
        this.output = output;
    }

    public void Execute()
    {
        output[0] = input + target;
    }
}