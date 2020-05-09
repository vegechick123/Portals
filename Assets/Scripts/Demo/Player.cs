using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PortalTraveller
{

    public static Player player;

    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float smoothMoveTime = 0.1f;
    public float jumpForce = 8;
    public float gravity = -18;

    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2(-40, 90);
    public float rotationSmoothTime = 0.1f;
    public VariableJoystick variableJoystick;
    MyCharacterController controller;
    Camera cam;
    Rigidbody rigibody;
    public float yaw;
    public float pitch;
    public GameObject releaseButton;
    float smoothYaw;
    float smoothPitch;

    float yawSmoothV;
    float pitchSmoothV;
    float verticalVelocity;
    Vector3 velocity;
    Vector3 smoothV;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    //Input
    Vector2 input;
    bool jumpButton;
    float mX;
    float mY;
    ///

    bool jumping;
    float lastGroundedTime;
    bool disabled;
    [HideInInspector]
    public PickUp holdObject;
    public GameObject holdGraphicObject;
    override protected void Awake()
    {
        base.Awake();
        if (player == null)
            player = this;
        else
            Debug.LogError("多个玩家");
        rigibody = GetComponent<Rigidbody>();
    }
    void Start()
    {
        cam = Camera.main;
#if UNITY_STANDALONE_WIN
        //这里的代码在IOS和Android平台都会编译
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
#endif


        controller = GetComponent<MyCharacterController>();

        yaw = transform.eulerAngles.y;
        pitch = cam.transform.localEulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;


    }
    private void FixedUpdate()
    {
        
        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;

        float currentSpeed = (Input.GetKey(KeyCode.LeftShift)) ? runSpeed : walkSpeed;
        Vector3 targetVelocity = inputDir * currentSpeed;

        float verticalVelocity = transform.InverseTransformDirection(rigibody.velocity).y + gravity * Time.fixedDeltaTime;

        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothV, smoothMoveTime);


        rigibody.velocity = transform.TransformDirection(velocity + new Vector3(0, verticalVelocity, 0)) * transform.lossyScale.z;

        var flags = controller.Move(velocity * Time.fixedDeltaTime);
        if (flags == CollisionFlags.Below)
        {
            jumping = false;
            lastGroundedTime = Time.time;
            verticalVelocity = 0;
        }

        if (jumpButton)
        {
            float timeSinceLastTouchedGround = Time.time - lastGroundedTime;
            if (controller.isGrounded || (!jumping && timeSinceLastTouchedGround < 0.15f))
            {
                jumping = true;
                verticalVelocity = jumpForce;
            }
        }



        // Verrrrrry gross hack to stop camera swinging down at start

    }
    public void HoldObject(PickUp holdObject)
    {
#if UNITY_ANDROID
        releaseButton.SetActive(true);
#endif
        holdGraphicObject = GameObject.Instantiate(holdObject.portalPhysicsObject.graphicsObject, graphicsObject.transform);
        holdGraphicObject.transform.localPosition = new Vector3(0.6f, -0.4f, 1);
        holdObject.Hold(this);
        if (graphicsClone != null && graphicsClone.activeInHierarchy)
            EnterPortalThreshold();
    }
    public void ReleaseHoldObject()
    {
#if UNITY_ANDROID
        releaseButton.SetActive(false);
#endif
        Debug.Log(2);
        if (holdObject != null)
        {
            if (graphicsClone != null)
            {
                Transform cubeClone = graphicsClone.transform.Find(holdGraphicObject.name);
                if (cubeClone != null)
                    Destroy(cubeClone.gameObject);
            }
            Destroy(holdGraphicObject);
            holdObject.Release(graphicsObject.transform.TransformPoint(0f, 0f, 0), transform.TransformDirection(new Vector3(0, 1f, 5)));
            holdObject = null;

        }
    }
    void UpdateInputs()
    {
#if UNITY_STANDALONE_WIN
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mX = Input.GetAxisRaw("Mouse X");
        float mMag = Mathf.Sqrt(mX * mX + mY * mY);
        mY = Input.GetAxisRaw("Mouse Y");
        if ( mMag> 5)
        {
            mX = 0;
            mY = 0;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 3f, 1 << LayerMask.NameToLayer("Cube")))
            {
                FPSDisplay.PutMessage("Hit", true);
                    holdObject = raycastHit.collider.gameObject.GetComponent<PickUp>();
                    HoldObject(holdObject); 
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            ReleaseHoldObject();
        }
#endif
#if UNITY_ANDROID
        mX = 0;
        mY = 0;
        input = new Vector2(variableJoystick.Horizontal, variableJoystick.Vertical);
        foreach (Touch touch in Input.touches)
        {

            FPSDisplay.PutMessage(touch.phase.ToString() + touch.deltaPosition, true);
            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit raycastHit;
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out raycastHit, 3f, 1 << LayerMask.NameToLayer("Cube")))
                {
                    FPSDisplay.PutMessage("Hit", true);
                    holdObject = raycastHit.collider.gameObject.GetComponent<PickUp>();
                    HoldObject(holdObject); 
                }
                else
                {
                    FPSDisplay.PutMessage("NotHit", true);
                }
            }
            else if (touch.phase == TouchPhase.Moved && touch.position.x > Screen.width / 2)
            {
                // Construct a ray from the current touch coordinates
                mX += touch.deltaPosition.x / Screen.width * 100;
                mY += touch.deltaPosition.y / Screen.height * 100;
            }
        }
#endif
        jumpButton = Input.GetButton("Jump");

    }
    void Update()
    {
        UpdateInputs();
        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Break();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            disabled = !disabled;
        }
        if (disabled)
        {
            return;
        }



        yaw += mX * mouseSensitivity;
        pitch -= mY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);

        transform.rotation *= Quaternion.Euler(Vector3.up * mX * mouseSensitivity);
        cam.transform.localEulerAngles = Vector3.right * smoothPitch;
    }
    private void LateUpdate()
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[2, 2] = -1;
        matrix *= cam.transform.worldToLocalMatrix;
        cam.worldToCameraMatrix = matrix;
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot, Matrix4x4 matrix)
    {
        if(holdObject!=null)
            holdObject.toMatrix = matrix * transform.worldToLocalMatrix * holdObject.toMatrix;
        
        base.Teleport(fromPortal, toPortal, pos, rot, matrix);
        rigibody.velocity = transform.TransformDirection(velocity);

        


        Matrix4x4 camMatrix = Matrix4x4.identity;
        camMatrix[2, 2] = -1;
        camMatrix *= cam.transform.worldToLocalMatrix;
        cam.worldToCameraMatrix = camMatrix;
        
    }

}