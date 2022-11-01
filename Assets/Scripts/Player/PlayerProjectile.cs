using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile
{
    public Transform transform {get; private set;}
    System.Action action;
    float speed;
    public Vector3 direction {get; private set;}

    public void Action(){
        action?.Invoke();

        transform.position += direction * speed * Time.deltaTime * 2;

        if(!IsInCamera())
            Destroy();
    }

    public void Destroy(){
        StaticObjects.processingMachine.playerProjectiles.Remove(this);
        UnityEngine.Object.Destroy(transform.gameObject);
    }

    public void Init(Transform transform, Transform position, float speed)
    {
        this.transform = transform;
        this.transform.position = position.position;
        this.transform.rotation = position.rotation;
        direction = transform.forward;
        this.speed = speed;
    }

    public void SetAction(System.Action action){
        this.action = action;
    }

    public bool IsInCamera(){
        var planes = GeometryUtility.CalculateFrustumPlanes(StaticObjects.MainCamera);
        var point = transform.position;
        foreach(var plane in planes){
            if(plane.GetDistanceToPoint(point) < 0){
                return false;
            }
        }
        return true;
    }
}
