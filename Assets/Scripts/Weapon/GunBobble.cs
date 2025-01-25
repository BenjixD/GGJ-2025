using UnityEngine;

public class GunBobble : MonoBehaviour
{
    [Header("Charging Settings")]
    public float jitterAmount = 0.02f; // Magnitude of the jitter (position offset)
    public float jitterSpeed = 50f; // Speed of jitter (how rapidly it changes)
    public float rotationJitterAmount = 2f; // Magnitude of jitter (rotation in degrees)
    public float maxChargeTime = 2f;
    [Header("Recoil Settings")]
    public float recoilAmount = 0.05f; // How much the gun moves backward
    public float recoilRotationAmount = 5f; // How much the gun rotates upward
    public float recoilScale = 2f; // How much recoil scales with charge time
    public float recoilSpeed = 10f; // How quickly the recoil happens
    public float returnSpeed = 5f; // How quickly the gun returns to normal
    private Vector3 recoilOffset; // Current recoil offset
    private Quaternion recoilRotation; // Current recoil rotation
    [Header("Bobbling Settings")]
    public float bobSpeed = 5f; // Speed of the bobbing
    public float bobAmount = 0.05f; // Vertical bobbing amount
    public float swayAmount = 0.025f; // Side-to-side bobbing amount
    public float tiltAmount = 2f; // Amount of tilt (in degrees)

    [Header("Running Offets")]
    public Vector3 runPositionOffset = new Vector3(0.2f, -0.2f, -0.5f); // Position offset for running
    public Vector3 runRotationOffset = new Vector3(-20f, 10f, 0f); // Rotation offset for running

    private float timer = 0f; // Tracks the time for sine wave calculations
    private PlayerController player;
    private Weapon weapon;
    private Rigidbody rb;
    // Hacky way to track if charging has finished
    private float wasChargingTime = 0f;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        weapon = GetComponentInParent<Weapon>();
        rb = GetComponentInParent<Rigidbody>();
    }

    // 3 gun movement modes (top to bottom priority)
    // 1. Shooting - gun recoils
    // 2. Walking - gun bobs
    // 3. Idle/Stunned - gun is still

    void Update() {
        float speed = rb.linearVelocity.magnitude;
        if(weapon.IsCharging()) {
            UpdateCharging();
        }
        else if(wasChargingTime > 0f) {
            UpdateFire();
        }
        else if(!player.IsStunned() && player.IsGrounded() && speed > 0.1f) {
            UpdateWalking(speed);
        }
        else {
            UpdateIdle();
        }
        // Soften the recoil if any
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * returnSpeed);
        recoilRotation = Quaternion.Lerp(recoilRotation, Quaternion.identity, Time.deltaTime * returnSpeed);
    }
    void UpdateCharging() {
        // Jitter the gun when charging
        timer += Time.deltaTime * jitterSpeed;
        // Create random jitter for position
        Vector3 positionJitter = new Vector3(
            Random.Range(-jitterAmount, jitterAmount),
            Random.Range(-jitterAmount, jitterAmount),
            Random.Range(-jitterAmount, jitterAmount)
        ) + recoilOffset;
        // Create random jitter for rotation
        Quaternion rotationJitter = Quaternion.Euler(new Vector3(
            Random.Range(-rotationJitterAmount, rotationJitterAmount),
            Random.Range(-rotationJitterAmount, rotationJitterAmount),
            Random.Range(-rotationJitterAmount, rotationJitterAmount)
        )) * recoilRotation;
        // Apply jitter with smooth transition
        transform.localPosition = Vector3.Lerp(transform.localPosition, positionJitter, timer);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, rotationJitter, timer);
        wasChargingTime = Mathf.Min(wasChargingTime + Time.deltaTime, maxChargeTime);
    }

    void UpdateFire() {
        // Apply a quick backward movement (kickback) and upward rotation
        recoilOffset = new Vector3(0, 0, -recoilAmount * (1 + wasChargingTime * recoilScale)); // Move gun backward along Z-axis
        recoilRotation = Quaternion.Euler(-recoilRotationAmount * (1 + wasChargingTime * recoilScale), Random.Range(-recoilRotationAmount, recoilRotationAmount), 0); // Rotate upward with slight randomness
        wasChargingTime = 0;
    }

    void UpdateWalking(float speed) {
        // Increment the timer based on movement speed
        timer += Time.deltaTime * bobSpeed * (player.IsSprinting() ? 1.5f : 1f);

        // Calculate the bobbing (vertical + horizontal sway)
        float bobOffsetY = Mathf.Sin(timer) * bobAmount; // Smooth vertical motion
        float swayOffsetX = Mathf.Cos(timer * 2f) * swayAmount; // Slightly faster horizontal motion

        // Calculate tilt (rotation on z-axis for sway)
        float tiltZ = Mathf.Cos(timer) * tiltAmount;

        Vector3 targetPosition = new Vector3(swayOffsetX, bobOffsetY, 0f) + recoilOffset;
        Quaternion targetRotation = Quaternion.Euler(0, 0, tiltZ) * recoilRotation;
        if(player.IsSprinting() && !player.IsStunned()) {
            targetPosition += runPositionOffset;
            targetRotation *= Quaternion.Euler(runRotationOffset);
        }

        // Apply the bobbing effect
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
        // Apply the tilt effect
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
    }

    void UpdateIdle() {
        // Reset the bobbing and tilting when the player stops moving
        timer = 0f;
        Vector3 targetPosition = Vector3.zero + recoilOffset;
        Quaternion targetRotation = Quaternion.identity * recoilRotation;
        if(player.IsSprinting() && !player.IsStunned()) {
            targetPosition += runPositionOffset;
            targetRotation *= Quaternion.Euler(runRotationOffset);
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * recoilSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * recoilSpeed);
    }
}
