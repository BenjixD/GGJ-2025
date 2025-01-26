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
    private AudioManager audioManager;

    protected override void StartLocal()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        startPosition = transform.position;
    }

    protected override void StartRemote()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        startPosition = transform.position;
    }

    protected override void UpdateLocal()
    {
        Bobble();
    }

    protected override void UpdateRemote()
    {
        Bobble();
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

            this.photonView.RPC("DestroyGameObject", RpcTarget.All, gameObject.GetPhotonView().ViewID);
            audioManager.PlaySFX("heal");
        }
    }

    private void Bobble()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
