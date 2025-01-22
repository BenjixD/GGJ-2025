using UnityEngine;
using Photon.Pun;
using System.Data;

public abstract class NetworkedMonoBehaviour : MonoBehaviourPun, IPunObservable
{

    // Implement seralizing the view to send data of our player to others
    protected abstract void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info);
    // Implement deserializing the view to receive data of other players
    protected abstract void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info);
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            WriteSerializeView(stream, info);
        }
        else
        {
            ReadSerializeView(stream, info);
        }
    }

    // Implement game logic for local player
    protected virtual void AwakeLocal() { }
    // Implement game logic for remote player
    protected virtual void AwakeRemote() { }
    void Awake()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            AwakeLocal();
        }
        else
        {
            AwakeRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void StartLocal() { }
    // Implement game logic for remote player
    protected virtual void StartRemote() { }
    void Start()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            StartLocal();
        }
        else
        {
            StartRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void UpdateLocal() { }
    // Implement game logic for remote player
    protected virtual void UpdateRemote() { }
    void Update()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            UpdateLocal();
        }
        else
        {
            UpdateRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void FixedUpdateLocal() { }
    // Implement game logic for remote player
    protected virtual void FixedUpdateRemote() { }

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            FixedUpdateLocal();
        }
        else
        {
            FixedUpdateRemote();
        }
    }

    // Implement game logic for local player
    protected virtual void OnCollisionEnterLocal(Collision collision) { }
    // Implement game logic for remote player
    protected virtual void OnCollisionEnterRemote(Collision collision) { }
    void OnCollisionEnter(Collision collision)
    {
        PhotonView otherPhotonView = collision.gameObject.GetComponent<PhotonView>();
        // The person's object getting hit is the one that should handle the collision
        // - playing local, use local collision handler
        // - other object is not a network object, use local collision handler
        // - other object is a network object, but it's mine, use local collision handler
        // - other object is a network object, but it's not mine, use remote collision handler
        if (!PhotonNetwork.IsConnected ||
            otherPhotonView == null ||
            otherPhotonView.IsMine)
        {
            OnCollisionEnterLocal(collision);
        }
        else
        {
            OnCollisionEnterRemote(collision);
        }
    }

    // Common Methods for NetworkedMonoBehaviour GameObjects
    public int MyActorNumber() {
        return photonView.Owner.ActorNumber;
    }

    public Color MyColor() {
        return PlayerColors.COLORS[MyActorNumber() - 1];
    }

    public string MyName() {
        string nickName = photonView.Owner.NickName;
        return string.IsNullOrEmpty(nickName) ? "Player " + MyActorNumber() : nickName;
    }

    public bool IsMine() {
        return photonView.IsMine;
    }

    [PunRPC]
    protected void DestroyGameObject(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv == null)
        {
            return;
        }
        if(pv.IsMine) {
            PhotonNetwork.Destroy(pv.gameObject);
        }
    }
}
