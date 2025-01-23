using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class PlayerController : NetworkedMonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Audio")]
    public AudioListener audioListener;
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
    public float sprintSpeed = 8.0f;
    public float sprintFOV = 90.0f;
    public float sprintFOVStepTime = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private bool isSprinting = false;

    [Header("Jump")]
    public float jumpPower = 5f;
    public float gravity = -9.81f;
    private bool isGrounded = false;

    [Header("Player Lives")]
    public int lives = 3;
    public Vector3 respawnPosition;

    [Header("Player Damage")]
    private float damage = 0f; // Damage % like in smash bros

    [Header("Death Objects")]
    public GameObject ringOutPrefab;

    // Networked Resources, used to sync player position and rotation
    // for remote players
    private Vector3 networkedPosition;
    private Quaternion networkedRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
        crosshairObject = GetComponentInChildren<Image>();
        if (photonView.IsMine)
        {
            audioListener.enabled = true;
            playerCamera.enabled = true;
        }
        else
        {
            audioListener.enabled = false;
            playerCamera.enabled = false;
            rb.isKinematic = true;
        }
    }
    protected override void StartLocal()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        crosshairObject.sprite = crosshairImage;
        crosshairObject.color = crosshairColor;
    }

    protected override void UpdateLocal()
    {
        UpdateCamera();
        CheckOutOfBounds();
    }

    protected override void FixedUpdateLocal()
    {
        UpdateMovement();
        UpdateGravity();
        CheckGround();
    }

    protected override void FixedUpdateRemote()
    {
        // Interpolate position and rotation from network
        rb.position = Vector3.Lerp(rb.position, networkedPosition, Time.fixedDeltaTime * 10);
        rb.rotation = Quaternion.Lerp(rb.rotation, networkedRotation, Time.fixedDeltaTime * 10);
    }

    // Player movement is updated using RBs and force locally
    // We sync positions and rotations over the wire
    // Note: Order of stream is important
    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.SendNext(rb.position);
        stream.SendNext(rb.rotation);
    }

    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        networkedPosition = (Vector3)stream.ReceiveNext();
        networkedRotation = (Quaternion)stream.ReceiveNext();

        // Compensate for lag
        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
        networkedPosition += rb.linearVelocity * lag;
    }


    private void UpdateCamera()
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
    }

    private void UpdateMovement()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        // Sprint
        if (Input.GetKey(sprintKey)) // old input
        {
            float adjustedSprintSpeed = isGrounded ? sprintSpeed : sprintSpeed * 0.6f;
            targetVelocity = transform.TransformDirection(targetVelocity) * adjustedSprintSpeed;

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

    private void CheckOutOfBounds() {
        Vector3 dirFromOob = transform.position;
        bool isOob = false;
        if (transform.position.y < -20 || transform.position.y > 30) {
            dirFromOob.y = 0;
            isOob = true;
        } if(transform.position.x < -40 || transform.position.x > 75) {
            dirFromOob.x = 0;
            isOob = true;
        } if(transform.position.z < -30 || transform.position.z > 70) {
            dirFromOob.z = 0;
            isOob = true;
        }
        if (isOob)
        {
            Quaternion rotationToOrigin = Quaternion.LookRotation(dirFromOob - transform.position);
            PhotonNetwork.Instantiate(ringOutPrefab.name, transform.position, rotationToOrigin);
            Respawn();
        }
    }

    private void UpdateGravity()
    {
        if (!isGrounded)
        {
            Vector3 downwardsVelocity = new Vector3(0, gravity, 0) * Time.fixedDeltaTime;
            rb.linearVelocity += downwardsVelocity;
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

    private void Respawn()
    {
        lives--;
        Debug.Log("current lives: " + lives);
        if (lives > 0)
        {
            // reset player
            transform.position = respawnPosition;
            rb.linearVelocity = Vector3.zero;
            damage = 0f;
        }
        else
        {
            // some sort of game over death??
            // could maybe add spectator mode
            PhotonNetwork.Destroy(gameObject);  // destroy the player
        }
    }

    public void TakeDamage(float damage)
    {
        this.damage += damage;
        Debug.Log("Player Damage: " + this.damage);
    }

    public float GetDamage()
    {
        return damage;
    }
}
