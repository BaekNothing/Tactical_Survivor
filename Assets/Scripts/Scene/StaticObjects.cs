using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class StaticObjects
{
    static Camera _MainCamera;
    public static Camera MainCamera{
        get{
            if(_MainCamera == null)
            {
                _MainCamera = Camera.main;
            }   
            return _MainCamera;
        }
    }

    static GameObject _GameRoot;
    public static GameObject GameRoot{
        get{
            if(_GameRoot == null)
            {
                _GameRoot = GameObject.Find("GameRoot");
            }   
            return _GameRoot;
        }
    }

    static ObjectProcessingMachine _processingMachine;
    public static ObjectProcessingMachine processingMachine
    {
        get{
            if(_processingMachine == null)
                _processingMachine = StaticObjects.GameRoot.GetComponent<ObjectProcessingMachine>();
            return _processingMachine;
        }
    }

    static GameObject _EntityRoot;
    public static GameObject EntityRoot{
        get{
            if (_EntityRoot == null)
            {
                if((_EntityRoot = GameObject.Find("EntityRoot")) == null)
                {
                    _EntityRoot = new GameObject("EntityRoot");
                }
                _EntityRoot.transform.position = Vector3.zero;
            }
            return _EntityRoot;
        }
    }

    static GameObject _EnemyRoot;
    public static GameObject EnemyRoot{
        get{
            if (_EnemyRoot == null)
            {
                if(( _EnemyRoot = GameObject.Find("EnemyRoot")) == null)
                {
                    _EnemyRoot = new GameObject("EnemyRoot");
                }
                _EnemyRoot.transform.SetParent(EntityRoot.transform);
                _EnemyRoot.transform.position = Vector3.zero;
            }
            return _EnemyRoot;
        }
    }
    
    static Player _Player;
    public static Player Player{
        get{
            if(_Player == null)
            {
                _Player = GameObject.Find("Player").GetComponent<Player>();
            }   
            return _Player;
        }
    }

    static PlayerDataObject _PlayerDataObject;
    public static PlayerDataObject PlayerDataObject{
        get{
            if(_PlayerDataObject == null)
            {
                _PlayerDataObject = Resources.Load<PlayerDataObject>("PlayerDataObject");
            }
            return _PlayerDataObject;
        }
    }

    static GameObject _ProjectileRoot;
    public static GameObject ProjectileRoot{
        get{
            if (_ProjectileRoot == null)
            {
                if((_ProjectileRoot = GameObject.Find("ProjectileRoot")) == null)
                {
                    _ProjectileRoot = new GameObject("ProjectileRoot");
                }
                _ProjectileRoot.transform.SetParent(GameRoot.transform);
                _ProjectileRoot.transform.position = Vector3.zero;
            }
            return _ProjectileRoot;
        }
    }

    static GameObject _UICamera;
    public static GameObject UICamera{
        get{
            if (_UICamera == null)
            {
                if((_UICamera = GameObject.Find("UICamera")) == null)
                {
                    _UICamera = new GameObject("UICamera");
                }
                _UICamera.transform.SetParent(GameRoot.transform);
                _UICamera.transform.position = Vector3.zero;
            }
            return _UICamera;
        }
    }

    static GameObject _UIRoot;
    public static GameObject UIRoot{
        get{
            if (_UIRoot == null)
            {
                if((_UIRoot = GameObject.Find("UIRoot")) == null)
                {
                    _UIRoot = new GameObject("UIRoot");
                }
                _UIRoot.transform.SetParent(GameRoot.transform);
                _UIRoot.transform.position = Vector3.zero;
            }
            return _UIRoot;
        }
    }
}
