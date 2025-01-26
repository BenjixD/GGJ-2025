using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager: MonoBehaviourPunCallbacks {
    public GameObject playerSpawnerPrefab;
    public Transform[] spawnPoints;

    // We need a bidirectional mapping between playerActors and spawnPoints
    // ActorID -> SpawnIndex
    // SpawnIndex -> ActorID
    // (Keep these in sync)
    private Dictionary<int, int> playerSpawnMapping = new Dictionary<int, int>();
    private Dictionary<int, int> spawnPlayerMapping = new Dictionary<int, int>();
    private PlayerSpawner mySpawner;

    [Header("For Development")]
    public bool connectToNetworkOnStart = false;
    public string roomId = "";

    void Awake() {
        if (connectToNetworkOnStart && !PhotonNetwork.IsConnected) {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void Start() {
        if (PhotonNetwork.IsMasterClient) {
            AssignSpawnPoints();
        }
    }

    public override void OnConnectedToMaster()
    {
        if(PhotonNetwork.InRoom || this.roomId == "") {
            return;
        }
        PhotonNetwork.JoinOrCreateRoom(this.roomId, new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom() {
        if (PhotonNetwork.IsMasterClient) {
            AssignSpawnPoints();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        if (PhotonNetwork.IsMasterClient)
        {
            // Reassign spawn points and send data to all players when a new player joins
            AssignSpawnPoints();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CleanupSpawnPoint(otherPlayer);
        }
    }

    private void AssignSpawnPoints() {
        // Determine spawn points for each player
        // If a player already has a spawn point, ignore them
        foreach (var player in PhotonNetwork.PlayerList) {
            if (!playerSpawnMapping.ContainsKey(player.ActorNumber)) {
                // Assign a random spawn point to the player
                // If thats taken, assign the next available spawn point
                // Note: We are assuming that the number of players is less than or equal to the number of spawn points
                int spawnIndex = Random.Range(0, spawnPoints.Length);
                while(true) {
                    if (!spawnPlayerMapping.ContainsKey(spawnIndex)) {
                        break;
                    }
                    spawnIndex = (spawnIndex + 1) % spawnPoints.Length;
                }
                playerSpawnMapping[player.ActorNumber] = spawnIndex;
                spawnPlayerMapping[spawnIndex] = player.ActorNumber;
            }
        }

        // Send the new spawn data to all players
        // We'll send a list of spawnIndexes
        // Note: ActorNumber starts at 1
        int[] data = new int[PhotonNetwork.PlayerList.Length];
        foreach(var player in PhotonNetwork.PlayerList) {
            int spawnIndex = playerSpawnMapping[player.ActorNumber];
            data[player.ActorNumber-1] = spawnIndex;
        }
        photonView.RPC("SpawnDataRPC", RpcTarget.All, data);
    }

    private void CleanupSpawnPoint(Player player) {
        int spawnIndex = playerSpawnMapping[player.ActorNumber];
        playerSpawnMapping.Remove(player.ActorNumber);
        spawnPlayerMapping.Remove(spawnIndex);
    }

    [PunRPC]
    protected void SpawnDataRPC(int[] data) {
        if(mySpawner != null) {
            // Ignore if my spawner already exists
            return;
        }
        // Otherwise create the player spawner
        mySpawner = Instantiate(playerSpawnerPrefab, spawnPoints[data[PhotonNetwork.LocalPlayer.ActorNumber-1]].position, Quaternion.identity).GetComponent<PlayerSpawner>();
    }
}
