using UnityEditor;
using UnityEngine;

public class FireParticles : MonoBehaviour
{
    [Header("Bubble Particle Defaults")]
    public float baseRate = 10f;
    public float baseSpeed = 0.5f;
    public float maxChargeTime = 2f;

    [Header("Color Blend")]
    public float blendfactor = 0.75f;
    public float alpha = 0.7f;

    private PlayerController player;
    private Weapon weapon;
    private ParticleSystem particles;
    // Hacky way to track if charging has finished
    private float wasChargingTime = 0f;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        weapon = GetComponentInParent<Weapon>();
        particles = GetComponent<ParticleSystem>();
        UpdateParticleColors();
    }

    private Color BlendColor(Color targetColor, Color original)
    {
        return original.BlendWith(targetColor, this.blendfactor, this.alpha);
    }

    void UpdateParticleColors() {
        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        if(renderer != null && renderer.material != null) {
            renderer.material.color = BlendColor(renderer.material.color, player.MyColor());
        }
    }

    // While we're charging, fizz some bubbles
    // On fire, increase the intensity once
    void Update() {
        if(weapon.IsCharging()) {
            UpdateCharging();
        }
        else if(wasChargingTime > 0f) {
            UpdateFire();
        }
    }
    void UpdateCharging() {
        if(wasChargingTime == 0) {
            SetParticleIntensity(true); // reset to base
            particles.Play();
        }
        wasChargingTime = Mathf.Min(wasChargingTime + Time.deltaTime, maxChargeTime);
    }

    void UpdateFire() {
        SetParticleIntensity(false); // fire intensely
        particles.Play();
        wasChargingTime = 0;
    }

    void SetParticleIntensity(bool loop) {
        var main = particles.main;
        var emission = particles.emission;
        main.startSpeed = baseSpeed + wasChargingTime * 2;
        emission.rateOverTime = baseRate + wasChargingTime * 20;
        main.loop = loop;
    }
}
