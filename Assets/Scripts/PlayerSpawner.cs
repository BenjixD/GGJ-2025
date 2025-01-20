using UnityEngine;
using Photon.Pun;

public class PlayerSpawner: Spawner {
    protected override void Spawn(GameObject prefab) {
        PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
    }
}
