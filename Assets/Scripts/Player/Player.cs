using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;

public class PlayerInputObject 
{
    VariableJoystick _joystick;
    public VariableJoystick joystick
    {
        get{
            if(_joystick == null)
                _joystick = StaticObjects.UIRoot.transform.Find("Variable Joystick").GetComponent<VariableJoystick>();
            return _joystick;
        }
    }

    Button _jumpButton;
    public Button jumpButton
    {
        get{
            if(_jumpButton == null)
                _jumpButton = StaticObjects.UIRoot.transform.Find("JumpButton").GetComponent<Button>();
            return _jumpButton;
        }
    }
    
    Button _attackButton;
    public Button attackButton
    {
        get{
            if(_attackButton == null)
                _attackButton = StaticObjects.UIRoot.transform.Find("AttackButton").GetComponent<Button>();
            return _attackButton;
        }
    }

    Button _skillButton;
    public Button skillButton
    {
        get{
            if(_skillButton == null)
                _skillButton = StaticObjects.UIRoot.transform.Find("SkillButton").GetComponent<Button>();
            return _skillButton;
        }
    }
}

public class Player : MonoBehaviour
{
    Vector3 direction;

    PlayerInputObject input = new PlayerInputObject();

    void Start()
    {
        Utility.SetActionInButton(input.jumpButton, Jump);
        Utility.SetActionInButton(input.attackButton, ()=>Attack(transform, 4f));
        Utility.SetActionInButton(input.skillButton, Shot);
    }

    float shootTimer = 1f;
    void Update()
    {
        if(!_main.SceneReady) return;

        InputProcess();

        if(shootTimer <= 0f)
        {
            shootTimer = 1f;
            Shot();
        }
        shootTimer -= Time.deltaTime;
   
        ActionJump();
        SyncCamera();
    }

    void InputProcess()
    {
        if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            StaticObjects.PlayerDataObject.IncreaseSpeed();
        }
        else if(input.joystick.Horizontal != 0 || input.joystick.Vertical != 0)
        {
            Move(input.joystick.Horizontal, input.joystick.Vertical);
            StaticObjects.PlayerDataObject.IncreaseSpeed();
        }
        else
            StaticObjects.PlayerDataObject.DecreaseSpeed();

        if(Input.GetMouseButton(0))
            Attack(transform, 4f);
            
        if(Input.GetMouseButton(1))
            Shot();

        if(!jumpFlag && Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    void Move(float axisX, float axisY)
    {
        transform.position += new Vector3(axisX *  
            StaticObjects.PlayerDataObject.speed.x * Time.deltaTime, 0, 0);
        transform.position += new Vector3(0, 0, axisY * 
            StaticObjects.PlayerDataObject.speed.y * Time.deltaTime);
        var lookat = new Vector3(axisX, 0, axisY);
        transform.rotation = Quaternion.LookRotation(Vector3.Lerp(transform.forward, lookat, 0.1f));
    }

    

    bool Attack(Transform transform, float distance)
    {
        var objects = StaticObjects.processingMachine.objects;
        int index = 0;
        int length = objects.Count;
        while (index < length)
        {
            var target = objects[index];
            if(Vector3.Distance(target.transform.position, 
                transform.position) < distance)
            {
                target.primitiveObject.stateHandler.SetCurrentState(stateFlag.dead);
                length--;
                return true;
            }
            index++;
        }
        return false;
    }   

    void Shot()
    {
        var projectile = new PlayerProjectile();
        StaticObjects.processingMachine.playerProjectiles.Add(projectile);
        projectile.Init(
            Instantiate(
                Resources.Load<GameObject>("prefabs/Projectile"), 
                StaticObjects.ProjectileRoot.transform
            ).transform, this.transform, 20f);

        projectile.SetAction(()=>{
            if(Attack(projectile.transform, 1f))
                projectile.Destroy();
        });
    }

    void SyncCamera(){
        StaticObjects.MainCamera.transform.position 
            = transform.position + new Vector3(0, 7.5f, -7f);
    }

    
    void Jump()
    {
        jumpFlag = true;
        jumpForce = 1.5f;
    }

    bool jumpFlag = false;
    float gravity = 0.05f;
    float jumpForce = 1.5f;
    void ActionJump()
    {
        if(jumpFlag)
        {
            transform.position += new Vector3(0, jumpForce, 0);
            jumpForce -= gravity;
            if(transform.position.y <= 0.5f)
            {
                transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                jumpForce = 1.5f;
                jumpFlag = false;
            }
        }
    }
}
