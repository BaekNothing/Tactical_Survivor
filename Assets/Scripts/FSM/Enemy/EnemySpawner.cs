using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    GameObject _primitiveObjectPrefab;
    GameObject primitiveObjectPrefab
    {
        get{
            if (!_primitiveObjectPrefab)
                _primitiveObjectPrefab = Resources.Load<GameObject>("prefabs/PrimitiveObject");
            return _primitiveObjectPrefab;
        }
    }

    float delay = 0f;
    float interval = 1f;

    // Update is called once per frame
    void Update()
    {
        if (!_main.SceneReady) return;
        if (delay > 0f)
        {
            delay -= Time.deltaTime;
            return;
        }
        delay = interval;

        if (ObjectProcessingMachine.maxObjectCount <= 
            StaticObjects.processingMachine.objects.Count)
            return;

        Spawn(new Vector3(UnityEngine.Random.Range(2f, 10f), 0.5f,
            UnityEngine.Random.Range(2f, 10f)));
    }
    
    void Spawn(Vector3 newPosition){
        var newObject = Instantiate(primitiveObjectPrefab, StaticObjects.EnemyRoot.transform);
        var bundle = new ObjectBundle(StaticObjects.processingMachine.objects.Count, newObject.transform, new EnemyObject());
        StaticObjects.processingMachine.objects.Add(bundle);

        bundle.primitiveObject.SetsPosition(newPosition);
    }
}
