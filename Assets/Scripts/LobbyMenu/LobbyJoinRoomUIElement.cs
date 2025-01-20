using UnityEngine;
using TMPro;

public class LobbyJoinRoomUIElement : LobbyUIElement
{
    public TMP_InputField playerNameInput;
    public TMP_InputField roomIdInput;
    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateOnStateChanged(LobbyStateEnum newState)
    {
        base.UpdateOnStateChanged(newState);
    }

    public void JoinRoom() {
        string playerName = playerNameInput.text;

        if (playerName == "") {
            // TODO: Raise some status error
            Debug.Log("Player name cannot be empty");
            return;
        }
        LobbyState.Instance.SetPlayerName(playerName);

        if (roomIdInput == null || roomIdInput.text == "") {
            LobbyState.Instance.CreateRoom();
        }
        else {
            string roomId = roomIdInput.text;
            LobbyState.Instance.JoinRoom(roomId);
        }
    }
}
