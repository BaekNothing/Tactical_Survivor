using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodySample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    float timer = 3f;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            GetComponent<Rigidbody>().position = (this.transform.position + new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ));
            // GetComponent<Rigidbody>().AddForce(new Vector3((int)UnityEngine.Random.Range(-1f, 1f), 1, (int)UnityEngine.Random.Range(-1f, 1f)) * 300f);
            timer = UnityEngine.Random.Range(1f, 3f);
        }
    }
}
