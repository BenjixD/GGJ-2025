using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager: MonoBehaviourPunCallbacks {
    public GameObject winnerCanvas;
    public int winnerActorNumber = -1;
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
        // Check for a winner only if you're the master client and
        // no-one has won yet
        if(PhotonNetwork.IsMasterClient && winnerActorNumber != -1) {
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
                winnerActorNumber = players.Keys.First();
                this.photonView.RPC("DeclareWinnerRPC", RpcTarget.All, players.Keys.First());
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

    public string GetWinningPlayerName() {
        if(winnerActorNumber == -1) {
            return "";
        }
        return players[winnerActorNumber].MyName();
    }

    [PunRPC]
    public void DeclareWinnerRPC(int actorNumber) {
        // Spawn some canvas that says "Player X Wins!"
        this.winnerActorNumber = actorNumber;
        var p = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var s = FindObjectsByType<SpectatorController>(FindObjectsSortMode.None);
        foreach (var player in p) {
            player.enabled = false;
        }
        foreach (var spectator in s) {
            spectator.enabled = false;
        }
        winnerCanvas.SetActive(true);
    }
}
