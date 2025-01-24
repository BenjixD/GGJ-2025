using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PlayerSpawner : Spawner
{

    public Transform[] spawnPoints;
    private Dictionary<int, int> playerSpawnIndices = new Dictionary<int, int>();
    private bool hasSpawned = false; // Flag to ensure the player is only spawned once

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined room");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Spawning master client");
            AssignSpawnPoints();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            // Reassign spawn points and send data to all players when a new player joins
            AssignSpawnPoints();
        }
    }

    private void AssignSpawnPoints()
    {
        List<int> availableSpawnIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawnIndices.Add(i);
        }

        List<int> playerActorNumbers = new List<int>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerActorNumbers.Add(player.ActorNumber);
        }

        // TODO: shuffle playerActorNumbers list

        // Assign spawn points to players
        playerSpawnIndices.Clear();
        for (int i = 0; i < playerActorNumbers.Count; i++)
        {
            int spawnIndex = availableSpawnIndices[i % availableSpawnIndices.Count];
            playerSpawnIndices[playerActorNumbers[i]] = spawnIndex;
        }

        // Send the spawn data to all players
        object[] data = new object[playerSpawnIndices.Count * 2];
        int index = 0;
        foreach (var kvp in playerSpawnIndices)
        {
            data[index++] = kvp.Key;
            data[index++] = kvp.Value;
        }

        photonView.RPC("ReceiveSpawnData", RpcTarget.All, data);
    }

    [PunRPC]
    private void ReceiveSpawnData(object[] data)
    {
        playerSpawnIndices.Clear();
        for (int i = 0; i < data.Length; i += 2)
        {
            int actorNumber = (int)data[i];
            int spawnIndex = (int)data[i + 1];
            playerSpawnIndices[actorNumber] = spawnIndex;
        }

        // Spawn the local player
        if (!hasSpawned)
        {
            SpawnPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
            hasSpawned = true; // Set the flag to true to prevent multiple spawns
        }
    }

    private void SpawnPlayer(int actorNumber)
    {
        if (playerSpawnIndices.TryGetValue(actorNumber, out int spawnIndex))
        {
            Transform spawnPoint = spawnPoints[spawnIndex];
            PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("No spawn point assigned for actor number " + actorNumber);
        }
    }

    protected override void Spawn(GameObject prefab)
    {

        // PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);

        // if (playerSpawnIndices.TryGetValue(actorNumber, out int spawnIndex))
        // {
        //     Transform spawnPoint = spawnPoints[spawnIndex];
        //     PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        // }
        // else
        // {
        //     Debug.LogError("No spawn point assigned for actor number " + actorNumber);
        // }
    }
}
