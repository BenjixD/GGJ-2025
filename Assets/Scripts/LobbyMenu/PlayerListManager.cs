using UnityEngine;
using TMPro;
using Photon.Realtime;

public class PlayerListManager : MonoBehaviour
{
    public Transform contentArea;
    public GameObject playerListItemPrefab; // The prefab for each player in the list

    protected virtual void Start()
    {
        LobbyState.Instance.Register(this);
    }

    public void UpdatePlayerList(Player[] players)
    {
        // Clear existing list items
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // Add a new item for each player in the room
        foreach (Player player in players)
        {
            // Instantiate the prefab
            GameObject playerItem = Instantiate(playerListItemPrefab, contentArea);

            // Set the player's name
            TMP_Text playerNameText = playerItem.GetComponentInChildren<TMP_Text>();
            playerNameText.text = player.NickName;
            if (player.IsLocal)
            {
                playerItem.GetComponent<TMP_Text>().color = Color.green;
            }
        }
    }
}
