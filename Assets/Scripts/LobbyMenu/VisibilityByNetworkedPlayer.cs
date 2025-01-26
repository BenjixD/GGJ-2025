using UnityEngine;
using Photon.Pun;

public class VisibilityByNetworkedPlayer : MonoBehaviourPunCallbacks
{
    public bool showForMasterOnly;
    public bool showForNonMasterOnly;
    private bool iAmMasterClient;
    void Start()
    {
        iAmMasterClient = PhotonNetwork.IsMasterClient;
        if(showForMasterOnly && !iAmMasterClient) {
            gameObject.SetActive(false);
        }
        if (showForNonMasterOnly && iAmMasterClient) {
            gameObject.SetActive(false);
        }
    }
}
