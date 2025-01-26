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
    public float minSpeedForSound = 0.1f;

    [Header("Sprint")]
    public float sprintSpeed = 8.0f;
    public float sprintFOV = 95.0f;
    public float sprintFOVStepTime = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private bool isSprinting = false;

    [Header("Jump")]
    public float jumpPower = 5f;
    public float gravity = -9.81f;
    private bool isGrounded = false;
    private bool wasGrounded = false;

    [Header("Player Lives")]
    public int lives = 3;

    [Header("Player Damage")]
    private float damage = 0f; // Damage % like in smash bros

    [Header("Death Objects")]
    public GameObject ringOutPrefab;
    public LevelBoundsSO levelBoundsSO;

    public AudioManager audioManager;

    // Networked Resources, used to sync player position and rotation
    // for remote players
    private bool isStunned = false;
    private float walkingSoundTimer = 0f;
    private Weapon weapon;
    // Kinda hacky, where we are instantiated determines our spawn point
    // Probably better to have a reference to the PlayerSpawner.
    private Vector3 respawnPosition;
    private Vector3 networkedPosition;
    private Quaternion networkedRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
        crosshairObject = GetComponentInChildren<Image>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        weapon = GetComponentInChildren<Weapon>();
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

        // Set bounds
        BoundsManager boundsManager = FindFirstObjectByType<BoundsManager>();
        if (boundsManager != null)
        {
            levelBoundsSO = boundsManager.levelBounds;
        }
        else
        {
            Debug.LogError("No BoundsManager found in scene");
        }
    }

    protected override void StartLocal()
    {
        RegisterToHUD();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        crosshairObject.sprite = crosshairImage;
        crosshairObject.color = crosshairColor;
        respawnPosition = transform.position;
    }

    protected override void StartRemote()
    {
        RegisterToHUD();
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

        // if (isSprinting)
        // {
        //     playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);
        // }
        // else
        // {
        //     playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, sprintFOVStepTime * Time.deltaTime);
        // }
    }

    private void UpdateMovement()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        // Sprint
        if (Input.GetKey(sprintKey) && !isStunned) // old input
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
        // Walk or Recovering from a stun
        else
        {
            isSprinting = false;

            targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

            Vector3 velocity = rb.linearVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            // If stunned, take longer to recover your movement. This makes interacting with
            // bubbles smoother as player input is not prioritized as much.
            Vector3 force = isStunned ? velocityChange * Time.fixedDeltaTime : velocityChange;
            rb.AddForce(force, ForceMode.VelocityChange);
        }

        HandleWalkingSound(rb.linearVelocity.magnitude);

        // Check if the player has just landed on the ground
        if (isGrounded && !wasGrounded)
        {
            // Play stomp sound
            audioManager.PlaySFX("stomp");
        }

        wasGrounded = isGrounded;
    }

    private void CheckOutOfBounds()
    {
        Vector3 dirFromOob = transform.position;
        bool isOob = false;
        if (transform.position.y < levelBoundsSO.minY || transform.position.y > levelBoundsSO.maxY)
        {
            dirFromOob.y = 0;
            isOob = true;
        }
        if (transform.position.x < levelBoundsSO.minX || transform.position.x > levelBoundsSO.maxX)
        {
            dirFromOob.x = 0;
            isOob = true;
        }
        if (transform.position.z < levelBoundsSO.minZ || transform.position.z > levelBoundsSO.maxZ)
        {
            dirFromOob.z = 0;
            isOob = true;
        }
        if (isOob)
        {
            Quaternion rotationToOrigin = Quaternion.LookRotation(dirFromOob - transform.position);
            PhotonNetwork.Instantiate(ringOutPrefab.name, transform.position, rotationToOrigin);

            photonView.RPC("PlaySFXRPC", RpcTarget.All, "death");

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
        if (lives > 0)
        {
            // reset player
            transform.position = respawnPosition;
            rb.linearVelocity = Vector3.zero;
            weapon.ResetBubbleGauge();
            this.photonView.RPC("TakeDamageRPC", RpcTarget.All, 0f); // Reset damage
        }
        else
        {
            // some sort of game over death??
            // could maybe add spectator mode
            DeregisterFromHUD();
            PhotonNetwork.Destroy(gameObject);  // destroy the player
        }
    }

    private void RegisterToHUD()
    {
        // Register player to HUD
        PlayerHUD hud = FindFirstObjectByType<PlayerHUD>();
        if (hud != null)
        {
            hud.Register(this);
        }
    }

    private void DeregisterFromHUD()
    {
        // Deregister player to HUD
        PlayerHUD hud = FindFirstObjectByType<PlayerHUD>();
        if (hud != null)
        {
            hud.Deregister(MyActorNumber());
        }
    }

    public void StunPlayer(float durationInSeconds)
    {
        StartCoroutine(RecoverFromStun(durationInSeconds));
    }
    public void TakeDamage(float damage)
    {
        float newDamage = this.damage + damage;
        this.photonView.RPC("TakeDamageRPC", RpcTarget.All, newDamage);
    }

    public float GetDamage()
    {
        return damage;
    }

    public void Heal(float amount)
    {
        damage = Mathf.Max(damage - amount, 0f);
    }

    public bool IsStunned()
    {
        return this.isStunned;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    private IEnumerator RecoverFromStun(float delay)
    {
        this.isStunned = true;
        yield return new WaitForSeconds(delay); // Wait for the specified time
        this.isStunned = false;
    }

    [PunRPC]
    protected void TakeDamageRPC(float newDamage)
    {
        this.damage = newDamage;
    }

    [PunRPC]
    private void PlaySFXRPC(string sfxName)
    {
        if (audioManager != null)
        {
            audioManager.PlaySFX(sfxName);
        }
        else
        {
            Debug.LogError("AudioManager not found!");
        }
    }

    private void HandleWalkingSound(float speed)
    {
        if (isGrounded && speed > minSpeedForSound)
        {
            float delay = Mathf.Lerp(0.8f, 0.3f, speed / sprintSpeed);
            // Increment timer
            walkingSoundTimer += Time.deltaTime;

            if (walkingSoundTimer >= delay)
            {
                if (!audioManager.walkingSource.isPlaying)
                {
                    audioManager.PlaySFX("walk");
                }

                // Reset timer after playing sound
                walkingSoundTimer = 0f;
            }
        }
        else
        {
            // Stop the sound and reset timer if not moving
            if (audioManager.walkingSource.isPlaying)
            {
                audioManager.walkingSource.Stop();
            }
            walkingSoundTimer = 0f;
        }
    }
}
