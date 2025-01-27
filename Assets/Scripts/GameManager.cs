using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager: MonoBehaviourPunCallbacks {
    public GameObject winnerCanvas;
    public string winner;
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
        if(PhotonNetwork.IsMasterClient && winner == "") {
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
                this.winner = players[players.Keys.First()].MyName();
                this.photonView.RPC("DeclareWinnerRPC", RpcTarget.All, this.winner);
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
    public void DeclareWinnerRPC(string player) {
        // Spawn some canvas that says "Player X Wins!"
        this.winner = player;
        var pc = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var ic = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        var wc = FindObjectsByType<Weapon>(FindObjectsSortMode.None);
        var spc = FindObjectsByType<SprayController>(FindObjectsSortMode.None);
        var sc = FindObjectsByType<SpectatorController>(FindObjectsSortMode.None);
        foreach (var p in pc) {
            p.enabled = false;
            p.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        foreach(var i in ic) {
            i.enabled = false;
        }
        foreach(var w in wc) {
            w.enabled = false;
        }
        foreach (var s in spc) {
            s.enabled = false;
        }
        foreach (var spectator in sc) {
            spectator.enabled = false;
        }
        winnerCanvas.SetActive(true);
        StartCoroutine(PrepareBackToLobbyScene());
    }
    IEnumerator PrepareBackToLobbyScene() {
        yield return new WaitForSeconds(10);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("LobbyScene");
    }
}
