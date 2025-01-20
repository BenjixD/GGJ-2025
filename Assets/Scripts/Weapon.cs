using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Header("Bubble")]
    public GameObject bubblePrefab;
    public Transform bubbleSpawn;
    public float bubbleVelocity = 2.0f;
    public float bubblePrefabLifetime = 5.0f;

    [Header("Camera")]
    public Camera playerCamera;

    void Update()
    {
        // set weapon position and rotation to follow camera
        transform.position = playerCamera.transform.position + playerCamera.transform.forward * 0.5f + playerCamera.transform.right * 0.5f;
        transform.rotation = playerCamera.transform.rotation;
    }

    private void OnAttack(InputValue value)
    {
        Debug.Log("fire");
        Fire();
    }

    private void Fire()
    {
        GameObject bubble = Instantiate(bubblePrefab, bubbleSpawn.position, Quaternion.identity);
        Rigidbody rb = bubble.GetComponent<Rigidbody>();
        rb.AddForce(bubbleSpawn.forward.normalized * bubbleVelocity, ForceMode.VelocityChange);
        StartCoroutine(DestroybubbleAfterTime(bubble, bubblePrefabLifetime));
    }

    private IEnumerator DestroybubbleAfterTime(GameObject bubble, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(bubble);
    }
}
