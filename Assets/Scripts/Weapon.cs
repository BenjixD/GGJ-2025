using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Bubble")]
    public GameObject bubblePrefab;
    public Transform bubbleSpawn;
    public float bubbleVelocity = 2.0f;
    public float bubblePrefabLifetime = 5.0f;
    public float maxChargeTime = 2f;
    public float maxBubbleSize = 3f;

    private float chargeTime = 0f;
    private bool isCharging = false;
    private GameObject chargingBubble;

    [Header("Bubble Gauge")]
    public float maxGauge = 100f;
    public float gaugeDepletionRate = 20f;
    public float gaugeRechargeRate = 10f;
    public float emptyGaugeRechargeRate = 6f;
    public BubbleGauge bubbleGauge;
    public Image gaugeFill;

    private bool isEmpty = false;
    private float currentGauge;
    private float currentGaugeRechargeRate;

    [Header("Camera")]
    public Camera playerCamera;

    void Start()
    {
        currentGauge = maxGauge;
        currentGaugeRechargeRate = gaugeRechargeRate;
    }

    void Update()
    {
        // set weapon position and rotation to follow camera
        transform.position = playerCamera.transform.position + playerCamera.transform.forward * 0.5f + playerCamera.transform.right * 0.5f + playerCamera.transform.up * -0.25f;
        transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(-10, -15, 0);

        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        if (Input.GetMouseButtonUp(0) && !isEmpty)
        {
            Fire();
        }

        if (isCharging && chargingBubble != null && !isEmpty)
        {
            chargeTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, maxBubbleSize, chargeTime / maxChargeTime);
            chargingBubble.transform.localScale = new Vector3(scale, scale, scale);

            // make bubble follow player
            chargingBubble.transform.position = bubbleSpawn.position;

            // deplete gauge
            currentGauge -= gaugeDepletionRate * Time.deltaTime;
            if (currentGauge <= 0)
            {
                // auto fire if empty
                currentGauge = 0;
                isEmpty = true;
                gaugeFill.color = Color.red;

                // slower recharge rate
                currentGaugeRechargeRate = emptyGaugeRechargeRate;
                Fire();
            }
        }
        else
        {
            // replenish gauge
            currentGauge += currentGaugeRechargeRate * Time.deltaTime;
            if (currentGauge > maxGauge)
            {
                currentGauge = maxGauge;
                isEmpty = false;
                gaugeFill.color = new Color(0.35f, 0.66f, 1.0f);

                // back to normal recharge rate
                currentGaugeRechargeRate = gaugeRechargeRate;
            }
        }

        bubbleGauge.SetGauge(currentGauge);
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
        
        // fired bubble
        // GameObject firedBubble = Instantiate(bubblePrefab, bubbleSpawn.position, Quaternion.identity);
        // float scale = Mathf.Lerp(1f, maxBubbleSize, chargeTime / maxChargeTime);
        // firedBubble.transform.localScale = new Vector3(scale, scale, scale);

        // Rigidbody rb = firedBubble.GetComponent<Rigidbody>();
        // rb.AddForce(bubbleSpawn.forward.normalized * bubbleVelocity, ForceMode.VelocityChange);

        // new fired bubble at crosshair
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 spawnPosition;
        if (Physics.Raycast(ray, out hit))
        {
            spawnPosition = hit.point;
        }
        else
        {
            spawnPosition = ray.GetPoint(1000);
        }

        GameObject firedBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
        float scale = Mathf.Lerp(1f, maxBubbleSize, chargeTime / maxChargeTime);
        firedBubble.transform.localScale = new Vector3(scale, scale, scale);

        Rigidbody rb = firedBubble.GetComponent<Rigidbody>();
        rb.AddForce(ray.direction * bubbleVelocity, ForceMode.VelocityChange);

        Collider bubbleCollider = firedBubble.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(bubbleCollider, playerCollider, true);
        StartCoroutine(ReenableCollision(bubbleCollider, playerCollider, 0.1f));

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

    private IEnumerator ReenableCollision(Collider bubbleCollider, Collider playerCollider, float delay)
{
    yield return new WaitForSeconds(delay);
    Physics.IgnoreCollision(bubbleCollider, playerCollider, false);
}


}
