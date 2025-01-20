using Photon.Pun;
using UnityEngine;

public class NetworkManager: MonoBehaviourPunCallbacks {
    [Header("Network Settings")]
    public int sendRate = 10;
    public int SerializationRate = 10;
    void Start() {
        PhotonNetwork.SendRate = this.sendRate;
        PhotonNetwork.SerializationRate = this.SerializationRate;
    }
}
