using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

/*
 * Lobby State defines the state machine which each UI component will show up or not.
 * The state machine looks like follows:
 *  LOBBY_STATE_LOADING
 *     |
 *     v
 *  LOBBY_STATE_MAIN <-> LOBBY_STATE_ROOM
 *     ^                    ^
 *     |                    |
 *     | -> LOBBY_STATE_SEARCH
 *
 *  (1) Start from the main menu
 *  (2) Create a Lobby --> Creates a Room w/ room ID
 *  (3) Search a Lobby --> Search a Room w/ room ID
 *  (4) In Lobby --> Start Game
 */

public enum LobbyStateEnum
{
    LOBBY_STATE_LOADING,
    LOBBY_STATE_MAIN,
    LOBBY_STATE_SEARCH,
    LOBBY_STATE_ROOM,
}

public class LobbyState : SingletonPunCallback<LobbyState>
{
    private string roomId;
    private string status;
    private LobbyStateEnum state;
    private List<LobbyUIElement> elements;
    private PlayerListManager playerList;
    protected override void Awake()
    {
        base.Awake();
        elements = new List<LobbyUIElement>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        StateChange(LobbyStateEnum.LOBBY_STATE_LOADING);
        this.status = "Connecting to Servers...";
        PhotonNetwork.ConnectUsingSettings();
    }

    // Public Interface
    public void Register(LobbyUIElement el)
    {
        elements.Add(el);
    }

    public void Register(PlayerListManager pl)
    {
        playerList = pl;
    }

    public LobbyStateEnum GetState()
    {
        return this.state;
    }

    public void SetPlayerName(string name)
    {
        PhotonNetwork.NickName = name;
    }
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(System.Guid.NewGuid().ToString());
    }

    public void SearchForRoom()
    {
        StateChange(LobbyStateEnum.LOBBY_STATE_SEARCH);
    }

    public void JoinRoom(string roomId)
    {
        PhotonNetwork.JoinRoom(roomId);
    }

    public string GetStatus()
    {
        return this.status;
    }

    public string GetRoomId()
    {
        return this.roomId;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public void StartGame()
    {
        // TODO: Start the game
        Debug.Log("start!");
        PhotonNetwork.LoadLevel("LevelSelect");
    }

    // Private methods
    private void StateChange(LobbyStateEnum newState)
    {
        this.state = newState;
        foreach (LobbyUIElement el in elements)
        {
            el.UpdateOnStateChanged(newState);
        }
    }

    // Photon Callbacks //
    public override void OnConnectedToMaster()
    {
        this.status = "Connected!";
        PhotonNetwork.JoinLobby();
        StateChange(LobbyStateEnum.LOBBY_STATE_MAIN);
    }

    public override void OnJoinedLobby()
    {
        this.status = "Joined Lobby!";
    }

    public override void OnJoinedRoom()
    {
        this.status = "Joined Room!";
        this.roomId = PhotonNetwork.CurrentRoom.Name;
        playerList.UpdatePlayerList(PhotonNetwork.PlayerList);
        StateChange(LobbyStateEnum.LOBBY_STATE_ROOM);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerList.UpdatePlayerList(PhotonNetwork.PlayerList);
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        playerList.UpdatePlayerList(PhotonNetwork.PlayerList);
    }
}
