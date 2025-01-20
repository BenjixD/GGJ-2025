using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Camera")]
    public Camera playerCamera;
    public float fov = 60f;
    public float mouseSensitivity = 3.0f;
    public float maxLookAngle = 60.0f;
    public bool lockCursor = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    [Header("Player Movement")]
    public float walkSpeed = 5.0f;
    public float maxVelocityChange = 10.0f;

    [Header("Sprint")]
    public float sprintSpeed = 10.0f;
    public float sprintFOV = 90.0f;
    public float sprintFOVStepTime = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private bool isSprinting = false;

    [Header("Jump")]
    public float jumpPower = 5f;
    private bool isGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
        crosshairObject = GetComponentInChildren<Image>();
    }

    void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        crosshairObject.sprite = crosshairImage;
        crosshairObject.color = crosshairColor;
    }

    void Update()
    {
        // camera look around

        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // clamp between look angles
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(0.0f, yaw, 0.0f);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0.0f, 0.0f);

        if (isSprinting)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, sprintFOVStepTime * Time.deltaTime);
        }

        CheckGround();

    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        
        // Sprint
        if (Input.GetKey(sprintKey)) // old input
        {
            targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

            Vector3 velocity = rb.linearVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);    

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            
            if (velocityChange.x != 0 || velocityChange.z != 0)
            {
                isSprinting = true;
            }
        }
        // Walk
        else
        {
            isSprinting = false;

            targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

            Vector3 velocity = rb.linearVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * 0.5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = 0.75f;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            isGrounded = true;
            Debug.DrawRay(origin, direction * distance, Color.green);
        }
        else
        {
            isGrounded = false;
        }
    }
}
