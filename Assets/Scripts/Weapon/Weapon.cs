using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : NetworkedMonoBehaviour
{
    [Header("Bubble")]
    public GameObject bubblePrefab;
    public Transform bubbleSpawn;
    public float bubbleVelocity = 2.0f;
    public float bubblePrefabLifetime = 5.0f;
    public float maxChargeTime = 2f;
    public float maxBubbleSize = 3f;
    public float minBubbleSize = 0.25f;

    private float chargeTime = 0f;
    private bool isCharging = false;
    private GameObject chargingBubble;

    [Header("Bubble Gauge")]
    public float maxGauge = 50f;
    public float gaugeDepletionRate = 40f;
    public float gaugeRechargeRate = 10f;
    public float emptyGaugeRechargeRate = 6f;
    public BubbleGauge bubbleGauge;
    public Image gaugeFill;

    private bool isEmpty = false;
    private float currentGauge;
    private float currentGaugeRechargeRate;

    [Header("Recoil")]
    public float recoilIntensity = 1.0f;
    public float recoilRecoverySpeed = 1.0f;

    private bool isRecovering = false;
    Vector3 originalPosition;
    Quaternion originalRotation;
    private Vector3 recoilOffset;
    private Quaternion recoilRotation;

    [Header("Camera")]
    public Camera playerCamera;

    // Networked Resources for bubble
    private bool networkedIsCharging;
    private Vector3 networkedBubblePosition;
    private Vector3 networkedBubbleScale;

    private AudioManager audioManager;

    protected override void AwakeLocal()
    {
        bubbleGauge.gameObject.SetActive(true);
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    protected override void AwakeRemote()
    {
        bubbleGauge.gameObject.SetActive(false);
    }
    protected override void StartLocal()
    {
        currentGauge = maxGauge;
        currentGaugeRechargeRate = gaugeRechargeRate;
    }

    protected override void UpdateLocal()
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
            float scale = Mathf.Lerp(minBubbleSize, maxBubbleSize, chargeTime / maxChargeTime);
            chargingBubble.transform.localScale = new Vector3(scale, scale, scale);

            // make bubble follow player
            chargingBubble.transform.position = bubbleSpawn.position;

            // deplete gauge
            if (scale < maxBubbleSize)
            {
                currentGauge -= gaugeDepletionRate * Time.deltaTime;
            }

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

    protected override void UpdateRemote()
    {
        // if (networkedIsCharging && chargingBubble == null)
        // {
        //     chargingBubble = PhotonNetwork.Instantiate(bubblePrefab.name, networkedBubblePosition, Quaternion.identity);
        // }

        if (chargingBubble != null)
        {
            // Interpolate position from network
            chargingBubble.transform.position = Vector3.Lerp(chargingBubble.transform.position, networkedBubblePosition, Time.deltaTime * 10);
            chargingBubble.transform.localScale = Vector3.Lerp(chargingBubble.transform.localScale, networkedBubbleScale, Time.deltaTime * 10);
        }
    }

    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Charging bubble
        stream.SendNext(isCharging);
        if (chargingBubble != null)
        {
            stream.SendNext(chargingBubble.transform.position);
            stream.SendNext(chargingBubble.transform.localScale);
        }
        else
        {
            stream.SendNext(Vector3.zero);
            stream.SendNext(Vector3.zero);
        }
    }

    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Charging bubble
        networkedIsCharging = (bool)stream.ReceiveNext();
        networkedBubblePosition = (Vector3)stream.ReceiveNext();
        networkedBubbleScale = (Vector3)stream.ReceiveNext();

        if (chargingBubble != null)
        {
            // Compensate for lag
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkedBubblePosition += networkedBubblePosition * lag;
        }
    }

    private void StartCharging()
    {
        // Debug.Log("chargin");
        if (chargingBubble != null || isRecovering) return;

        isCharging = true;
        chargeTime = 0f;
        chargingBubble = PhotonNetwork.Instantiate(bubblePrefab.name, bubbleSpawn.position, Quaternion.identity);

        Collider chargingBubbleCollider = chargingBubble.GetComponent<Collider>();
        if (chargingBubbleCollider != null)
        {
            Destroy(chargingBubbleCollider);
        }
    }

    private void Fire()
    {
        // Debug.Log("fire!");
        if (!isCharging || chargingBubble == null || isRecovering) return;

        isCharging = false;

        float scale = Mathf.Lerp(minBubbleSize, maxBubbleSize, chargeTime / maxChargeTime);

        // raycast from the camera to get the direction to shoot
        Camera mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000);
        }

        Vector3 direction = (targetPoint - bubbleSpawn.position).normalized;

        // fired bubble
        GameObject firedBubble = PhotonNetwork.Instantiate(bubblePrefab.name, bubbleSpawn.position, Quaternion.identity);
        firedBubble.transform.localScale = new Vector3(scale, scale, scale);

        Rigidbody rb = firedBubble.GetComponent<Rigidbody>();
        rb.AddForce(direction * bubbleVelocity, ForceMode.VelocityChange);

        Recoil();

        // destroy charging bubble
        PhotonNetwork.Destroy(chargingBubble);
        chargingBubble = null;

        // TODO: network and have distance affect?
        audioManager.PlaySFX("shoot");
    }

    private void Recoil()
    {
        // NOT WORKING; only has delay between bubbles
        // set original
        // originalPosition = transform.localPosition;
        // originalRotation = transform.localRotation;

        // recoilOffset = playerCamera.transform.forward * recoilIntensity * -1;
        // recoilRotation = Quaternion.Euler(recoilIntensity * 10, recoilIntensity * 10, 0);

        // apply recoil
        // transform.localPosition += recoilOffset;
        // transform.localRotation *= recoilRotation;

        StartCoroutine(HandleRecoilRecovery());
    }

    private IEnumerator HandleRecoilRecovery()
    {
        isRecovering = true;
        float elapsedTime = 0f;

        // Vector3 startPosition = transform.localPosition;
        // Quaternion startRotation = transform.localRotation;

        while (elapsedTime < recoilRecoverySpeed)
        {
            // transform.localPosition = Vector3.Lerp(startPosition, originalPosition, elapsedTime / recoilRecoverySpeed);
            // transform.localRotation = Quaternion.Slerp(startRotation, originalRotation, elapsedTime / recoilRecoverySpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position and rotation are set correctly
        // transform.localPosition = originalPosition;
        // transform.localRotation = originalRotation;
        isRecovering = false;
    }
}
