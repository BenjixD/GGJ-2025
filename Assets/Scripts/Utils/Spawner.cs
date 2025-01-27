using UnityEngine;
using Photon.Pun;
using System.Collections;
using Photon.Realtime;

public abstract class Spawner: MonoBehaviourPunCallbacks {
    public GameObject prefab;
    [Header("For Development")]
    public bool connectToNetworkOnStart = false;
    public string roomId = "";

    void Awake() {
        if (connectToNetworkOnStart && !PhotonNetwork.IsConnected) {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void Start() {
        StartCoroutine(WaitThenSpawn());
    }

    public override void OnConnectedToMaster()
    {
        if(PhotonNetwork.InRoom || this.roomId == "") {
            return;
        }
        PhotonNetwork.JoinOrCreateRoom(this.roomId, new RoomOptions(), TypedLobby.Default);
    }

    private IEnumerator WaitThenSpawn() {
        while (!PhotonNetwork.InRoom) {
            yield return null;
        }
        Spawn(this.prefab);
    }
    protected abstract void Spawn(GameObject prefab);
}
