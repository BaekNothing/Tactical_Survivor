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

        StaticObjects.PlayerDataObject.hp = 10;
    }

    float shootTimer = 1f;
    void Update()
    {
        if(!_main.SceneReady) return;

        ProcessInputs();
        ProcessActions();
        ProcessCamera();
        ProcessUI();
    }

#region Inputs
    void ProcessInputs()
    {
        if(takeDown) return;

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

#if (UNITY_EDITOR)
        if(Input.GetMouseButtonUp(0))
            Attack(transform, 4f);
            
        if(Input.GetMouseButtonUp(1))
            Shot();

        if(Input.GetKeyUp(KeyCode.Space))
            Jump();
#endif
    }

    //이게 여기있는게 맞나?
    void Move(float axisX, float axisY)
    {
        transform.position += new Vector3(axisX *  
            StaticObjects.PlayerDataObject.speed.x * Time.deltaTime, 0, 0);
        transform.position += new Vector3(0, 0, axisY * 
            StaticObjects.PlayerDataObject.speed.y * Time.deltaTime);
        var lookat = new Vector3(axisX, 0, axisY);
        transform.rotation = Quaternion.LookRotation(Vector3.Lerp(transform.forward, lookat, 0.1f));
    }


    float attackTimer;
    float attackCoolTime = 0.5f;    

    bool Attack(Transform transform, float distance)
    {
        var objects = StaticObjects.processingMachine.objects;
        int index = 0;
        int length = objects.Count;
        while (index < length)
        {
            var target = objects[index];
            if(Vector3.Distance(target.rigidbody.position, 
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

    bool RangeAttack(Transform transform, float distance)
    {
        var objects = StaticObjects.processingMachine.objects;
        int index = 0;
        int length = objects.Count;
        while (index < length)
        {
            var target = objects[index];
            if(Vector3.Distance(target.rigidbody.position, 
                transform.position) < distance)
            {
                target.primitiveObject.stateHandler.SetCurrentState(stateFlag.dead);
                length--;
            }
            index++;
        }
        return false;
    }
    
    float skillTimer;
    float skillCoolTime = 3f;

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

    void Jump()
    {
        if(jumpFlag && takeDown) return;
        
        if(!jumpFlag)
        {
            jumpFlag = true;
            jumpForce = gravity;
            jumpHoldTime = 0.3f;
        }
        else
        {
            takeDown = true;
            takeDownDelay = 0.2f;
        }
        Utility.ALog("Jump");
    }

#endregion

#region Actions
    void ProcessActions()
    {
        ActionJump();
    }
    
    //Jump    
    readonly float gravity = 20f;
    bool jumpFlag = false;
    float jumpHoldTime = 0f;
    float jumpForce = 0f;

    //Take Down
    bool takeDown = false;
    float takeDownDelay = 0f;
    float takeDownMultiplier = 3f;


    void ActionJump()
    {
        if(jumpFlag && !takeDown)
        {
            transform.position += new Vector3(0, jumpForce, 0) * Time.deltaTime;
            
            if( -0.1f <= jumpForce && jumpForce <= 0.1f && jumpHoldTime > 0)
            {
                jumpHoldTime -= Time.deltaTime;
            }
            else
            {
                jumpForce -= gravity * Time.deltaTime;
            }

            if(transform.position.y <= 0.5f)
            {
                transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                jumpFlag = false;
            }
        }

        if(takeDown)
        {
            float atkRange = 6f;

            if(takeDownDelay > 0f)
            {
                takeDownDelay -= Time.deltaTime;
            }
            else
            {
                transform.position += new Vector3(0, -gravity * takeDownMultiplier, 0) * Time.deltaTime;
                if(transform.position.y <= 0.5f)
                {
                    shakeValue = 90f;
                    Utility.FindT<Transform>(transform, "takeDown").gameObject.SetActive(true);
                    RangeAttack(transform, atkRange);
                    transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                    takeDown = false;
                    jumpFlag = false;
                }
            }
        }
    }



#endregion


#region Camera
    void ProcessCamera(){
        SyncCamera();
        ShakeCamera();
    }

    void SyncCamera(){
        StaticObjects.MainCamera.transform.position 
            = transform.position + new Vector3(0, 7.5f, -7f);
    }

    
    float shakeValue = 0f;
    void ShakeCamera(){
        if(shakeValue <= 0) return;
        
        StaticObjects.MainCamera.transform.position +=
            new Vector3(0, Mathf.Sin(shakeValue), 0);
        
        shakeValue -= 360f * Time.deltaTime;
    }
#endregion

#region  UI

    Text _hpText;
    public Text hpText
    {
        get{
            if(_hpText == null)
                _hpText = StaticObjects.UIRoot.transform.Find("HPText").GetComponent<Text>();
            return _hpText;
        }
    }

    void ProcessUI(){
        UpdateUI();
    }

    void UpdateUI(){
        hpText.text = StaticObjects.PlayerDataObject.hp.ToString();        
    }

#endregion
}

