using UnityEngine;
using TMPro;

public class LobbyStartGameUIElement : LobbyUIElement
{
    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateOnStateChanged(LobbyStateEnum newState)
    {
        if(!LobbyState.Instance.IsMasterClient()) {
            gameObject.SetActive(false);
            return;
        }
        base.UpdateOnStateChanged(newState);
    }

    public void StartGame() {
        LobbyState.Instance.StartGame();
    }
}
