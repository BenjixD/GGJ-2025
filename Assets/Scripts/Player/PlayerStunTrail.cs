using System.Collections;
using UnityEngine;

public class PlayerStunTrail : MonoBehaviour
{
    public float baseDuration = 0.5f;
    private bool wasStunned = false;
    private ParticleSystem ps;
    private PlayerController player;
    private float blendFactor = 0.60f;
    private float alpha = 0.9f;

    void Start() {
        ps = GetComponent<ParticleSystem>();
        player = GetComponentInParent<PlayerController>();
        var main = ps.main;
        // Set the default duration
        main.duration = baseDuration;
        // Set the start color to the player's color
        main.startColor = this.player.MyColor().BlendWith(main.startColor.color, blendFactor, alpha);
    }

    void Update() {
        if (!wasStunned && player.IsStunned()) {
            wasStunned = true;
            StartCoroutine(SpawnTrail());
        }
    }
    IEnumerator SpawnTrail() {
        ps.Play();
        while(true) {
            if(!player.IsStunned()) {
                break;
            }
            yield return new WaitForSeconds(baseDuration);
        }
        ps.Stop();
        wasStunned = false;
    }
}
