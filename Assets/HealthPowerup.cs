using UnityEngine;
using Photon.Pun;

public class HealthPowerup : NetworkedMonoBehaviour
{
    public float healthAmount = 50f;

    [Header("Animation")]
    public float bobbingSpeed = 2.0f;
    public float bobbingHeight = 0.25f;
    public float rotationSpeed = 90.09f;

    private Vector3 startPosition;

    protected override void StartLocal()
    {
        startPosition = transform.position;
    }

    protected override void UpdateLocal()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

    }

    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    protected override void OnTriggerEnterLocal(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            playerController.Heal(healthAmount);
            Debug.Log("heal: " + healthAmount);

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
