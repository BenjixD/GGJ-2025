using UnityEngine;
using Photon.Pun;

public abstract class NetworkedMonoBehaviour : MonoBehaviourPun, IPunObservable
{

    // Implement seralizing the view to send data of our player to others
    protected abstract void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info);
    // Implement deserializing the view to receive data of other players
    protected abstract void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info);
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            WriteSerializeView(stream, info);
        }
        else {
            ReadSerializeView(stream, info);
        }
    }

    // Implement game logic for local player
    protected virtual void AwakeLocal() {}
    // Implement game logic for remote player
    protected virtual void AwakeRemote() {}
    void Awake() {
        if (!PhotonNetwork.IsConnected || photonView.IsMine) {
            AwakeLocal();
        }
        else {
            AwakeRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void StartLocal() {}
    // Implement game logic for remote player
    protected virtual void StartRemote() {}
    void Start()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine) {
            StartLocal();
        }
        else {
            StartRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void UpdateLocal() {}
    // Implement game logic for remote player
    protected virtual void UpdateRemote() {}
    void Update()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine) {
            UpdateLocal();
        }
        else {
            UpdateRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void FixedUpdateLocal() {}
    // Implement game logic for remote player
    protected virtual void FixedUpdateRemote() {}

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine) {
            FixedUpdateLocal();
        }
        else {
            FixedUpdateRemote();
        }
    }
}
