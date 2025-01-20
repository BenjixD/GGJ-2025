using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour
{
    private Rigidbody rb;
    private Collider bubbleCollider;

    public float bubbleDrag = 1.0f;
    public float bubbleMass = 0.1f;
    public float gravityScale = 0.1f;
    public float floatStrength = 1.0f;
    public float bounceForce = 10.0f; 

    void Awake()
    {
        bubbleCollider = GetComponent<Collider>();
        bubbleCollider.enabled = false;
        StartCoroutine(EnableColliderAfterDelay(0.1f));
    }
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = bubbleDrag;
        rb.mass = bubbleMass;
    }

    void FixedUpdate()
    {
        // gravity
        rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);

        // random floating movement
        Vector3 randomFloat = new Vector3(
            Mathf.PerlinNoise(Time.time, 0.0f) - 0.5f,
            Mathf.PerlinNoise(0.0f, Time.time) - 0.5f,
            Mathf.PerlinNoise(Time.time, Time.time) - 0.5f
        ) * floatStrength;

        rb.AddForce(randomFloat, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
             // collision normal
            Vector3 collisionNormal = collision.contacts[0].normal;

            // Check if the collision is from the top
            if (Vector3.Dot(collisionNormal, Vector3.up) > 0.5f)
            {
                // Apply an upward force to the player
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }
            else
            {
                // Apply a force in the opposite direction of the collision
                rb.AddForce(-collisionNormal * bounceForce, ForceMode.Impulse);
            }

            // pop bubble after a short delay
            StartCoroutine(DestroyBubbleAfterDelay());
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        // Enable the collider once it exits any collider
        bubbleCollider.enabled = true;
    }

    private IEnumerator DestroyBubbleAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bubbleCollider != null)
        {
            bubbleCollider.enabled = true;
        }
    }

}


