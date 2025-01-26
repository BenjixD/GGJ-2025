using UnityEngine;

public class SpectatorController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 3f;
    public float maxLookAngle = 85f; // Limit for looking up/down

    private Camera spectatorCamera;
    private float pitch = 0f; // Tracks vertical rotation (up/down)

    private void Start()
    {
        spectatorCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Move();
        Look();
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0f;

        // vertical movement (E and Q)
        if (Input.GetKey(KeyCode.E))
        {
            upDown = 1f;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            upDown = -1f;
        }

        Vector3 forward = spectatorCamera.transform.forward;
        Vector3 right = spectatorCamera.transform.right;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * vertical + right * horizontal + Vector3.up * upDown;
        Vector3 move = direction * moveSpeed * Time.deltaTime;

        transform.position += move;
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(Vector3.up, mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        spectatorCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
