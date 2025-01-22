
using TMPro;
using UnityEngine;

public class NameTag: MonoBehaviour {
    private PlayerController player;
    private TMP_Text textMesh;
    private Color pastelBase = new Color(0xD7 / 255f, 0xF8 / 255f, 0xFF / 255f, 1f); // #D7F8FF
    private float blendFactor = 0.75f;
    private float alpha = 0.7f;

    void Awake() {
        this.player = GetComponentInParent<PlayerController>();
        this.textMesh = GetComponentInChildren<TMP_Text>();
    }
    void Start() {
        this.textMesh.text = this.player.MyName();
        this.textMesh.color = this.player.MyColor().BlendWith(this.pastelBase, this.blendFactor, this.alpha);
        if(this.player.IsMine()) {
            this.gameObject.SetActive(false);
        }
    }

    void Update() {
        this.textMesh.transform.LookAt(Camera.main.transform);
    }
}
