using Photon.Pun;
using UnityEngine;

public class VisibilityByNetworkedPlayer : MonoBehaviour
{
    public bool showForMasterOnly;
    public bool showForNonMasterOnly;
    private bool iAmMasterClient;
    void Start()
    {
        iAmMasterClient = PhotonNetwork.IsMasterClient;
        if(showForMasterOnly && !iAmMasterClient) {
            gameObject.SetActive(false);
        } else if (!showForNonMasterOnly && iAmMasterClient) {
            gameObject.SetActive(false);
        }
    }
}
