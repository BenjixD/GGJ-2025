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
        if (PhotonNetwork.IsMasterClient)
        {
            // only master client can load the level
            PhotonNetwork.LoadLevel(levelName);
        }
    }
}
