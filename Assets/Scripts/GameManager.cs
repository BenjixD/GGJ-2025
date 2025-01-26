using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager: MonoBehaviourPunCallbacks {
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private HashSet<int> deadPlayers = new HashSet<int>();

    [Header("For Development")]
    public bool connectToNetworkOnStart = false;
    public string roomId = "";
    public bool dontEndGameOnSinglePlayer = true;


    void Awake() {
        if (connectToNetworkOnStart && !PhotonNetwork.IsConnected) {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void Start() {}

    void Update() {
        checkForWinner();
    }

    public override void OnConnectedToMaster()
    {
        if(PhotonNetwork.InRoom || this.roomId == "") {
            return;
        }
        PhotonNetwork.JoinOrCreateRoom(this.roomId, new RoomOptions(), TypedLobby.Default);
    }

    private void checkForWinner() {
        if(PhotonNetwork.IsMasterClient){
            var numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            if(players.Count == 1 && deadPlayers.Count >= numberOfPlayers - 1){
                if(deadPlayers.Count == 0 && dontEndGameOnSinglePlayer) {
                    // We're debugging the scene locally. Don't want to end the game
                    return;
                }
                // Everyone has died except one player
                // Declare the last player as the winner, network to all players
                // 1. Disable all player movements and interactions
                // 2. Show Victory Screen
                Debug.Log("Winner is " + players.Keys.First());
                // this.photonView.RPC("DeclareWinnerRPC", RpcTarget.All, players.Keys.First());
            }
        }
    }

    public void Register(PlayerController player) {
        if (PhotonNetwork.IsMasterClient) {
            players.Add(player.MyActorNumber(), player);
        }
    }
    public void Deregister(PlayerController player) {
        if (PhotonNetwork.IsMasterClient) {
            players.Remove(player.MyActorNumber());
            deadPlayers.Add(player.MyActorNumber());
        }
    }

    [PunRPC]
    public void DeclareWinnerRPC(int actorNumber) {
        // Spawn some canvas that says "Player X Wins!"
    }
}
