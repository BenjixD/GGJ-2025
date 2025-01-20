using TMPro;

public enum StatusDataEnum
{
    STATUS,
    ROOMID,
}

public class LobbyStatusUIElement : LobbyUIElement
{
    public StatusDataEnum statusData;
    private TextMeshProUGUI status;
    protected override void Start()
    {
        base.Start();
        status = this.gameObject.GetComponent<TextMeshProUGUI>();
    }

    public override void UpdateOnStateChanged(LobbyStateEnum newState)
    {
        base.UpdateOnStateChanged(newState);
        status.text = GetDataByDataType(statusData);
    }

    public string GetDataByDataType(StatusDataEnum dataType)
    {
        switch (dataType)
        {
            case StatusDataEnum.STATUS:
                return LobbyState.Instance.GetStatus();
            case StatusDataEnum.ROOMID:
                return "RoomID: " + LobbyState.Instance.GetRoomId();
            default:
                return "";
        }
    }
}
