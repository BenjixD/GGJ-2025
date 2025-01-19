using UnityEngine;
using System.Collections.Generic;

/*
 * Lobby State defines the state machine which each UI component will show up or not.
 * The state machine looks like follows:
 *
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
    LOBBY_STATE_MAIN,
    LOBBY_STATE_SEARCH,
    LOBBY_STATE_ROOM,
}

public class LobbyState : Singleton<LobbyState>
{
    private LobbyStateEnum state;
    private List<LobbyUIElement> elements;
    void Awake()
    {
        state = LobbyStateEnum.LOBBY_STATE_MAIN;
        elements = new List<LobbyUIElement>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Public Interface
    public void Register(LobbyUIElement el) {
        elements.Add(el);
    }

    public void StateChange(LobbyStateEnum newState) {
        this.state = newState;
        foreach(LobbyUIElement el in elements) {
            el.UpdateOnStateChanged(newState);
        }
    }
}
