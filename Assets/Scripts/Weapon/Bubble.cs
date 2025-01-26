using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UIElements;

public class Bubble : NetworkedMonoBehaviour
{
    private Rigidbody rb;
    private Collider bubbleCollider;

    public float bubbleDrag = 1.0f;
    public float bubbleMass = 0.1f;
    public float gravityScale = 0.1f;
    public float floatStrength = 1.0f;
    public float bounceForce = 10.0f;
    public float baseKeepAroundTime = 2.5f;

    [Header("Color Blend")]
    public float blendfactor = 0.75f;
    public float alpha = 0.7f;

    private Vector3 networkedPosition;
    private Vector3 networkedScale;

    private Color BlendColor(Color targetColor, Color original)
    {
        return original.BlendWith(targetColor, this.blendfactor, this.alpha);
    }

    void Awake()
    {
        bubbleCollider = GetComponent<Collider>();
        bubbleCollider.enabled = false;
        StartCoroutine(EnableColliderAfterDelay(0.1f));
        rb = GetComponent<Rigidbody>();
        // Update bubble color
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.color = BlendColor(mr.material.color, MyColor());
    }


    protected override void StartLocal()
    {
        // Use RB physics for local bubbles
        rb.useGravity = false;
        rb.linearDamping = bubbleDrag;
        rb.mass = bubbleMass;
        StartCoroutine(DestroyBubbleAfterDelay(gameObject, baseKeepAroundTime + transform.localScale.magnitude * 2));
    }

    protected override void StartRemote()
    {
        // Use kinematic interpolation for remote bubbles
        rb.isKinematic = true;
    }

    protected override void FixedUpdateLocal()
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

    protected override void FixedUpdateRemote()
    {
        rb.position = Vector3.Lerp(transform.position, networkedPosition, Time.fixedDeltaTime * 30);
        transform.localScale = Vector3.Lerp(transform.localScale, networkedScale, Time.fixedDeltaTime * 30);
    }

    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.SendNext(rb.transform.position);
        stream.SendNext(transform.localScale);
    }

    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        networkedPosition = (Vector3)stream.ReceiveNext();
        networkedScale = (Vector3)stream.ReceiveNext();

        // Compensate for lag
        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
        networkedPosition += rb.linearVelocity * lag;
    }

    // protected override void OnCollisionEnterLocal(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         Debug.Log("Collision with Player detected");
    //         // collision normal
    //         Vector3 collisionNormal = collision.contacts[0].normal;

    //         // Check if the collision is from the top
    //         if (Vector3.Dot(collisionNormal, Vector3.up) > 0.5f)
    //         {
    //             // Apply an upward force to the player
    //             rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
    //         }
    //         else
    //         {
    //             // Apply a force in the opposite direction of the collision
    //             rb.AddForce(-collisionNormal * bounceForce, ForceMode.Impulse);
    //         }

    //         // pop bubble after a short delay
    //         StartCoroutine(DestroyBubbleAfterDelay(gameObject, 0.1f));
    //     }
    // }

    protected override void OnTriggerEnterLocal(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            // collision normal
            Rigidbody playerRb = collision.GetComponent<Rigidbody>();
            PlayerController playerController = collision.GetComponent<PlayerController>();

            Vector3 collisionNormal = (collision.transform.position - transform.position).normalized;

            float scaledBounceForce = bounceForce * transform.localScale.magnitude;

            if(!photonView.IsMine) {
                playerController.TakeDamage(scaledBounceForce);
            // s * (1 - e^(-k * x))
            // The idea is that as x -> inf, the stun time approaches s
            //                  as x -> 0, the stun time approaches 0
            // Good for simulating stun time. Tune s,k.
            playerController.StunPlayer(1f * (1f - Mathf.Exp(-0.1f * scaledBounceForce)));
            } else {
                // Deal less damage to yourself and not stun
                playerController.TakeDamage(scaledBounceForce * 0.05f);
            }
            float knockbackForce = scaledBounceForce * (1 + playerController.GetDamage() / 50f);
            // Check if the collision is from the top
            if (Vector3.Dot(collisionNormal, Vector3.up) > 0.5f)
            {
                float upBounceForce = knockbackForce;
                // Cancel veritcal velocity and apply vertical knockback
                playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
                // Apply an upward force to the player
                playerRb.AddForce(Vector3.up * upBounceForce, ForceMode.Impulse);
            }
            else
            {
                // Apply a force in the opposite direction of the collision
                playerRb.AddForce(collisionNormal * knockbackForce, ForceMode.Impulse);
            }

            // pop bubble after a short delay
            StartCoroutine(DestroyBubbleAfterDelay(gameObject, 0.1f));
        }
    }

    private void OnCollisionExit(Collision other)
    {
        // Enable the collider once it exits any collider
        bubbleCollider.enabled = true;
    }

    private IEnumerator DestroyBubbleAfterDelay(GameObject bubble, float delay)
    {
        yield return new WaitForSeconds(delay);

        PhotonView bubblePhotonView = bubble.GetComponent<PhotonView>();
        if (bubblePhotonView == null)
        {
            Debug.LogError("PhotonView not found on bubble!");
            yield break;
        }
        this.photonView.RPC("DestroyGameObject", RpcTarget.All, bubble.GetPhotonView().ViewID);
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


