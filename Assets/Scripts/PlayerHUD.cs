using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviourPunCallbacks {

    [Header("Margins from bottom left")]
    public float xMarginRelative = 0.1f;
    public float yMarginRelative = 0.1f;

    [Header("Prefab For Damage Indicator")]
    public GameObject damageIndicatorPrefab;
    public Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    public Dictionary<int, GameObject> damageIndicators = new Dictionary<int, GameObject>();

    private Color pastelBase = new Color(0xD7 / 255f, 0xF8 / 255f, 0xFF / 255f, 1f); // #D7F8FF
    private float blendFactor = 0.70f;
    private float alpha = 0.6f;

    public static int MAX_LIFE_COUNT = 3;

    void Start() {

    }

    void Update() {
        UpdateAllDamageIndicators();
        UpdateAllLives();
    }

    void UpdateAllDamageIndicators() {
        foreach (int actor in players.Keys) {
            UpdateDamageIndicator(actor);
        }
    }

    void UpdateDamageIndicator(int actor, bool isDead = false) {
        PlayerController player = players[actor];
        GameObject indicator = damageIndicators[actor];
        // Get the damage and update the UI elements
        int damage = Mathf.RoundToInt(player.GetDamage());
        TMP_Text text = indicator.GetComponentInChildren<TMP_Text>();
        if(isDead) {
            text.text = "";
        }
        else if (player.IsMine()) {
            text.text = $"<b>{damage.ToString()}%</b>";
        } else {
            text.text = $"{damage.ToString()}%";
        }
    }

    void UpdateAllLives() {
        foreach (int actor in players.Keys) {
            UpdateLife(actor);
        }
    }

    void UpdateLife(int actor) {
        PlayerController player = players[actor];
        GameObject liveTracker = damageIndicators[actor].transform.Find("LifeTracker").gameObject;
        int lifeCount = player.GetLives();
        for(int i = 0; i < MAX_LIFE_COUNT; i++) {
            if(lifeCount > i) {
                liveTracker.transform.GetChild(i).gameObject.SetActive(true);
            } else {
                liveTracker.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void AnchorAll() {
        foreach (int actor in damageIndicators.Keys) {
            AnchorIndicator(actor);
        }
    }
    void AnchorIndicator(int actor) {
        // Anchors are depedent on the number of players
        // (ie, we divide up the grid by the number of players, excluding the margins)
        // We then assign each player a grid cell, and place the indicator anchored bottomleft of the cell
        // Place actors left to right.
        RectTransform t = damageIndicators[actor].GetComponent<RectTransform>();
        float gridSizeX = (1 - xMarginRelative * 2) / damageIndicators.Count;

        // Set both anchors to be the same to avoid stretching
        t.anchorMin = new Vector2(xMarginRelative + gridSizeX * (actor - 1) + gridSizeX/2, yMarginRelative); // actors start from 1
        t.anchorMax = new Vector2(xMarginRelative + gridSizeX * (actor - 1) + gridSizeX/2, yMarginRelative); // actors start from 1
        // Center pivot
        t.pivot = new Vector2(0.5f, 0.5f);
        // Reset position to center in the anchor space
        t.anchoredPosition = Vector2.zero;
    }

    public void Register(PlayerController player) {
        this.players.Add(player.MyActorNumber(), player);
        // Instantiate the indicator
        GameObject indicator = Instantiate(damageIndicatorPrefab, this.transform);
        // Update color of emblem
        indicator.transform.Find("Emblem").GetComponent<Image>().color = player.MyColor().BlendWith(this.pastelBase, this.blendFactor, this.alpha);
        this.damageIndicators.Add(player.MyActorNumber(), indicator);
        // Update the new Anchors for all players
        AnchorAll();
    }

    public void Deregister(int actorNumber) {
        UpdateLife(actorNumber);
        UpdateDamageIndicator(actorNumber, true);
        this.players.Remove(actorNumber);
        // Update the new Anchors for all players
        AnchorAll();
    }

    // Non-graceful shutdown. Player forcefully exited the room without cleanup
    public override void OnPlayerLeftRoom(Player otherPlayer) {
        this.Deregister(otherPlayer.ActorNumber);
    }
}
