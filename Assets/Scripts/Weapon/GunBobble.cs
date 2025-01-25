using UnityEngine;

public class GunBobble : MonoBehaviour
{
    [Header("Bobbling Settings")]
    public float bobSpeed = 5f; // Speed of the bobbing
    public float bobAmount = 0.1f; // Vertical bobbing amount
    public float sideBobAmount = 0.05f; // Side-to-side bobbing amount
    public float tiltAmount = 2f; // Amount of tilt (in degrees)

    private float timer = 0f; // Tracks the time for sine wave calculations
    private PlayerController player;
    private Weapon weapon;
    private Rigidbody rb;

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
        if(!player.IsStunned() && speed > 0.1f) {
            UpdateWalking(speed);
        }
        else {
            UpdateIdle();
        }
    }

    void UpdateWalking(float speed) {
        // Increment the timer based on movement speed
        timer += Time.deltaTime * bobSpeed;

        // Calculate the new X,Y position (side-to-side),(up-and-down)
        // float newY = Mathf.Sin(timer) * bobAmount;
        // float newX = Mathf.Cos(timer) * sideBobAmount;

        // Calculate the new X,Y position (side-to-side),(up-and-down)
        float newY = Mathf.PingPong(timer, 1f) > 0.5f ? bobAmount : -bobAmount;
        float newX = Mathf.PingPong(timer * 1.5f, 1f) > 0.5f ? sideBobAmount : -sideBobAmount;
        // Calculate the tilt angle (tilt left and right as you move)
        // float tiltAngle = Mathf.Sin(timer) * tiltAmount;
        // Snappy tilt movement
        float tiltAngle = Mathf.PingPong(timer, 1f) > 0.5f ? tiltAmount : -tiltAmount;


        // Apply the bobbing effect
        transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
        // Apply the tilt effect
        transform.localRotation = Quaternion.identity * Quaternion.Euler(0, 0, tiltAngle);
    }

    void UpdateIdle() {
        // Reset the bobbing and tilting when the player stops moving
        timer = 0f;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
