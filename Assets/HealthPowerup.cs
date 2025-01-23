using UnityEngine;
using Photon.Pun;

public class HealthPowerup : NetworkedMonoBehaviour
{
    public float healthAmount = 50f;
    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // stream.SendNext(rb.transform.position);
    }

    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // networkedPosition = (Vector3)stream.ReceiveNext();

        // Compensate for lag
        // float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
        // networkedPosition += rb.linearVelocity * lag;
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
