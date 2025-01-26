using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Photon.Pun;

public class SprayController : NetworkedMonoBehaviour
{
    public Camera playerCamera;
    public GameObject sprayDecalPrefab;
    public float sprayDistance = 10f;
    public float sprayCooldown = 0.5f;
    public AudioManager audioManager;

    private bool canSpray = true;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    protected override void UpdateLocal()
    {
        if (Input.GetKey(KeyCode.F) && canSpray)
        {
            Spray();
        }
    }

    private void Spray()
    {
        canSpray = false;
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, sprayDistance))
        {
            audioManager.PlaySFX("spray");
            CreateSpray(hit.point, hit.normal);
        }

        StartCoroutine(SprayCooldown());
    }

    private void CreateSpray(Vector3 position, Vector3 normal)
    {
        // Decal projector to handle cutting off at edges
        Vector3 decalPosition = position + normal * 0.01f;
        GameObject decal = PhotonNetwork.Instantiate(sprayDecalPrefab.name, decalPosition, Quaternion.LookRotation(normal));

        Vector3 directionToPlayer = playerCamera.transform.position - decal.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(normal, directionToPlayer);

        decal.transform.rotation = targetRotation;

        DecalProjector projector = decal.GetComponent<DecalProjector>();
        projector.size = new Vector3(1, 1, 0.1f); // Adjust size as needed
    }

    private IEnumerator SprayCooldown()
    {
        yield return new WaitForSeconds(sprayCooldown);
        canSpray = true; // Allow spraying again after cooldown
    }
    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}
