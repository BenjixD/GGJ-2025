using UnityEngine;

public class LobbyUIElement : MonoBehaviour
{
    public LobbyStateEnum[] visibleStates;
    protected virtual void Start()
    {
        LobbyState.Instance.Register(this);
    }

    public virtual void UpdateOnStateChanged(LobbyStateEnum newState)
    {
        foreach(LobbyStateEnum state in visibleStates) {
            if (state == newState) {
                gameObject.SetActive(true);
                return;
            }
        }
        gameObject.SetActive(false);
    }
}
