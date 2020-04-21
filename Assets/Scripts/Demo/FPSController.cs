using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : PortalTraveller {

    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float smoothMoveTime = 0.1f;
    public float jumpForce = 8;
    public float gravity = -18;

    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2 (-40, 90);
    public float rotationSmoothTime = 0.1f;

    MyCharacterController controller;
    Camera cam;
    Rigidbody rigibody;
    public float yaw;
    public float pitch;
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
    private void Awake()
    {
        rigibody = GetComponent<Rigidbody>();
    }
    void Start () {
        cam = Camera.main;
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        controller = GetComponent<MyCharacterController> ();

        yaw = transform.eulerAngles.y;
        pitch = cam.transform.localEulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;
    }
    private void FixedUpdate()
    {
        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;

        float currentSpeed = (Input.GetKey(KeyCode.LeftShift)) ? runSpeed : walkSpeed;
        Vector3 targetVelocity = inputDir* currentSpeed;

        float verticalVelocity= transform.InverseTransformDirection(rigibody.velocity).y+gravity*Time.fixedDeltaTime;

        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothV, smoothMoveTime);


        rigibody.velocity =transform.TransformDirection(velocity+new Vector3(0,verticalVelocity,0));

        var flags = controller.Move(velocity*Time.fixedDeltaTime);
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
        float mMag = Mathf.Sqrt(mX * mX + mY * mY);
        if (mMag > 5)
        {
            mX = 0;
            mY = 0;
        }

        yaw += mX * mouseSensitivity;
        pitch -= mY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);
        transform.
        transform.rotation *= Quaternion.Euler(Vector3.up * mX*mouseSensitivity);
        cam.transform.localEulerAngles = Vector3.right * smoothPitch;
    }
    void UpdateInputs()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        jumpButton = Input.GetButton("Jump");
        mX = Input.GetAxisRaw("Mouse X");
        mY = Input.GetAxisRaw("Mouse Y");
    }
    void Update () {
        UpdateInputs();
        if (Input.GetKeyDown (KeyCode.P)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Break ();
        }
        if (Input.GetKeyDown (KeyCode.O)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            disabled = !disabled;
        }
        if (disabled) {
            return;
        }

    }
    private void LateUpdate()
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[2, 2] = -1;
        matrix *= cam.transform.worldToLocalMatrix;
        cam.worldToCameraMatrix = matrix;
    }
    
    public override void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) {
        transform.position = pos;
        transform.rotation = rot;
        transform.localScale= toPortal.lossyScale;
        rigibody.velocity = transform.TransformDirection(velocity);
        
        //Vector3 eulerRot = rot.eulerAngles;
        //float delta = Mathf.DeltaAngle (smoothYaw, eulerRot.y);
        //yaw += delta;
        //smoothYaw += delta;
        //transform.eulerAngles = Vector3.up * smoothYaw;
        //velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (velocity));
        Physics.SyncTransforms ();
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[2, 2] = -1;
        matrix *= cam.transform.worldToLocalMatrix;
        cam.worldToCameraMatrix = matrix;
    }

}