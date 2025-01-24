using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class LevelSelect : MonoBehaviourPunCallbacks
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel(int levelIndex)
    {
        // Scene names should follow this pattern
        string levelName = "Level" + levelIndex;
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            if (photonView != null)
            {
                photonView.RPC("LoadLevelForAllPlayers", RpcTarget.All, levelName);
            }
            else
            {
                Debug.LogError("PhotonView is null!");
            }
        }
        else
        {
            Debug.LogError($"Scene '{levelName}' not found in Build Settings!");
        }
    }

    // RPC to load the level for all players
    [PunRPC]
    void LoadLevelForAllPlayers(string levelName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(levelName);
        }
    }
}
