using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _main : MonoBehaviour
{    
    public static bool SceneReady = false;

    ObjectProcessingMachine _objectProcessingMachine;
    ObjectProcessingMachine objectProcessingMachine{
        get{
            if(_objectProcessingMachine == null)
                _objectProcessingMachine = transform.GetComponent<ObjectProcessingMachine>();
            return _objectProcessingMachine;
        }
    }


    void Start()
    {
        // Application.targetFrameRate = 30;    
        // Screen.SetResolution(1080, 1920, true);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
    }

    void Update(){
        if(CheckSceneReady())
            SceneReady = true;
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    bool CheckSceneReady(){
        if (!objectProcessingMachine.isReady)
            return false;
        return true;
    }

    void OnApplicationQuit(){
        SceneReady = false;
    }
}