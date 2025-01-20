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

    [Header("Charging")]
    private float chargeTime = 0f;
    public float maxChargeTime = 2f;
    public float maxBubbleSize = 3f;
    
    private bool isCharging = false;
    private GameObject chargingBubble;

    [Header("Camera")]
    public Camera playerCamera;


    void Update()
    {
        // set weapon position and rotation to follow camera
        transform.position = playerCamera.transform.position + playerCamera.transform.forward * 0.5f + playerCamera.transform.right * 0.5f;
        transform.rotation = playerCamera.transform.rotation;

        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Fire();
        }

        if (isCharging && chargingBubble != null)
        {
            chargeTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, maxBubbleSize, chargeTime / maxChargeTime);
            chargingBubble.transform.localScale = new Vector3(scale, scale, scale);

            // make bubble follow player
            chargingBubble.transform.position = bubbleSpawn.position;
        }
    }

    private void StartCharging()
    {
        Debug.Log("chargin");
        if (chargingBubble != null) return; 

        isCharging = true;
        chargeTime = 0f;
        chargingBubble = Instantiate(bubblePrefab, bubbleSpawn.position, Quaternion.identity);
    }

    private void Fire()
    {
        Debug.Log("fire!");
        if (!isCharging || chargingBubble == null) return;

        isCharging = false;
        
        // new fired bubble
        GameObject firedBubble = Instantiate(bubblePrefab, bubbleSpawn.position, Quaternion.identity);
        float scale = Mathf.Lerp(1f, maxBubbleSize, chargeTime / maxChargeTime);
        firedBubble.transform.localScale = new Vector3(scale, scale, scale);

        Rigidbody rb = firedBubble.GetComponent<Rigidbody>();
        rb.AddForce(bubbleSpawn.forward.normalized * bubbleVelocity, ForceMode.VelocityChange);
        StartCoroutine(DestroybubbleAfterTime(firedBubble, bubblePrefabLifetime));

        // destroy charging bubble
        Destroy(chargingBubble);
        chargingBubble = null;
    }

    private IEnumerator DestroybubbleAfterTime(GameObject bubble, float time)
    {
        yield return new WaitForSeconds(time);
        if (bubble)
        {
            Destroy(bubble);
        }
    }
}
