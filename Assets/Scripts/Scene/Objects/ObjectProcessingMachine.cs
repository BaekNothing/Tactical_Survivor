using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;

/// <summary>
/// Every physical event will be processed by this class
/// </summary>
public class ObjectProcessingMachine : MonoBehaviour
{   
    public static readonly int maxObjectCount = 1500;

    public bool isReady = false;
 
    public List<ObjectBundle> objects { get; private set; } = new List<ObjectBundle>();
    public List<PlayerProjectile> playerProjectiles { get; private set; } = new List<PlayerProjectile>();

    GameObject _primitiveObjectPrefab;
    GameObject primitiveObjectPrefab
    {
        get{
            if (!_primitiveObjectPrefab)
                _primitiveObjectPrefab = Resources.Load<GameObject>("prefabs/PrimitiveObject");
            return _primitiveObjectPrefab;
        }
    }
#region  CompareGroup
    GameObject _rigidBodyObjectPrefab;
    GameObject rigidBodyObjectPrefab
    {
        get{
            if (!_rigidBodyObjectPrefab)
                _rigidBodyObjectPrefab = Resources.Load<GameObject>("prefabs/RigidBodyObject");
            return _rigidBodyObjectPrefab;
        }
    }

    enum CompareState{
        optimized,
        rigidbody
    }
    CompareState state; 
    public void SwitchCompareTarget()
    {
        foreach(Transform child in StaticObjects.EnemyRoot.GetComponentInChildren<Transform>())
            Destroy(child.gameObject);
        objects.Clear();
        rigidbodyObjects.Clear();

        if(state == CompareState.optimized)
        {
            InitRigidBodyObjects();   
        }
        else
        {
            InitOptimizedStateObjects();
        }
    }

    List<GameObject> rigidbodyObjects = new List<GameObject>();
    void InitRigidBodyObjects()
    {
        for (int i = 0; i < maxObjectCount; i++)
        {
            var newObject = Instantiate(rigidBodyObjectPrefab, StaticObjects.EnemyRoot.transform);
            rigidbodyObjects.Add(newObject);
            newObject.transform.position =  
                new Vector3(UnityEngine.Random.Range(2f, 10f), 0.5f,
                UnityEngine.Random.Range(2f, 10f));
        }
        state = CompareState.rigidbody;
    }
#endregion

    void Start()
    {
        InitOptimizedStateObjects();
        isReady = true;
    }


    void InitOptimizedStateObjects(){
        for (int i = 0; i < maxObjectCount; i++)
        {
            var newObject = Instantiate(primitiveObjectPrefab, StaticObjects.EnemyRoot.transform);
            var bundle = new ObjectBundle(i, newObject.GetComponent<Rigidbody>(), new EnemyObject());
            StaticObjects.processingMachine.objects.Add(bundle);

            bundle.primitiveObject.SetsPosition(
                new Vector3(UnityEngine.Random.Range(2f, 10f), 0.5f,
                UnityEngine.Random.Range(2f, 10f)));            
        }
        state = CompareState.optimized;
    }

    void Update(){
        if (!_main.SceneReady) return;
        UpdateStateAction();
        UpdateProjectileAction();
        //SortBundleListbyTransform();
    }

    int stateCounter = 1;
    void UpdateStateAction(){
        for (int i = 0; i < objects.Count; i++)
        {
            if ((((byte)(i) & 0x1) ^ stateCounter) == 0x1)
            {
                objects[i].primitiveObject.Action();
            }
        }
        stateCounter = (stateCounter + 1) & 0x1;
    }

    int projectileCounter = 0;
    void UpdateProjectileAction()
    {
        for (int i = 0; i < playerProjectiles.Count; i++)
        {
            if ((((byte)(i) & 0x1) ^ projectileCounter) == 0x1)
            {
                if (playerProjectiles[i].transform)
                    playerProjectiles[i].Action();
            }
        }
        projectileCounter = (projectileCounter + 1) & 0x1;
    }

    // void SortBundleListbyTransform(){
    //     if (Time.frameCount % 60 == 0)
    //     {
    //         for (int i = 0; i < objects.Count - 3; i++)
    //         {
    //             var prev = objects[i];
    //             var compA = objects[i + 1];
    //             var compB = objects[i + 2];

    //             if (Vector3.Distance(prev.transform.position, compA.transform.position) >
    //                 Vector3.Distance(prev.transform.position, compB.transform.position))
    //             {
    //                 objects[i + 1] = compB;
    //                 objects[i + 2] = compA;
    //             }
    //         }

    //         objects.Sort((a, b) =>
    //         {
    //             int result = a.transform.position.x.CompareTo(b.transform.position.x);
    //             if (result == 0)
    //                 result = a.transform.position.y.CompareTo(b.transform.position.y);
    //             if (result == 0)
    //                 result = a.transform.position.z.CompareTo(b.transform.position.z);
    //             return result;
    //         });
    //     }
    // }

    // void CheckPhysicalEvent(ObjectBundle targetObject, Transform player)
    // {
    //     if(Vector3.Distance(targetObject.transform.position, 
    //         player.transform.position) < 4f)
    //     {
    //         targetObject.primitiveObject.stateHandler.SetCurrentState(stateFlag.dead);
    //         targetObject.primitiveObject.stateHandler.Action();
    //         objects.Remove(targetObject);  
    //     }
    // }

    void OnApplicationQuit(){
        foreach (var obj in objects)
        {
            obj.primitiveObject.stateHandler.CleanUpState();
        }
    }

    public void RemoveBundle(ObjectBundle bundle) {
        objects.Remove(bundle);
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].index = i;
        }
    } 
}
