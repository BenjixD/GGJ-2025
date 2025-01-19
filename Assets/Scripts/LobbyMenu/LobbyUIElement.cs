using UnityEngine;

public class LobbyUIElement : MonoBehaviour
{
    public LobbyStateEnum[] visibleStates;
    public LobbyStateEnum nextState;
    void Start()
    {
        LobbyState.Instance.Register(this);
    }

    public void UpdateOnStateChanged(LobbyStateEnum newState)
    {
        foreach(LobbyStateEnum state in visibleStates) {
            if (state == newState) {
                gameObject.SetActive(true);
                return;
            }
        }
        gameObject.SetActive(false);
    }

    public void UpdateState() {
        LobbyState.Instance.StateChange(nextState);
    }
}
