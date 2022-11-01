using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerDataObject", menuName = "ScriptableObject/Object", order = 0)]
public class PlayerDataObject : ScriptableObject
{
    public int hp = 10; 
    public Vector2 speed = new Vector2(10, 10);
    
    public void DecreaseSpeed(){
        speed -= speed * 0.1f * Time.deltaTime;
        if(speed.magnitude < new Vector2(10, 10).magnitude)
            speed = new Vector2(10, 10);
    }

    public void IncreaseSpeed(){
        speed += speed * 0.1f * Time.deltaTime;
        if(speed.magnitude > new Vector2(20, 20).magnitude)
            speed = new Vector2(20, 20);
    }
}
