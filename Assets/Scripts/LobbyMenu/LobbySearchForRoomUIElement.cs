using UnityEngine;
using TMPro;

public class LobbySearchForRoomUIElement : LobbyUIElement
{
    public void SearchRoom() {
        LobbyState.Instance.SearchForRoom();
    }
}
