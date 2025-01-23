using Photon.Pun;
using UnityEngine;

public class PlayerDeathController: NetworkedMonoBehaviour {
    [Header("Color Blend")]
    public float blendfactor = 0.75f;
    public float alpha = 0.7f;
    private ParticleSystem ps;
    void Awake() {
        ps = GetComponentInChildren<ParticleSystem>();
        SetColor();
    }

    protected override void UpdateLocal() {
        CheckForCompletion();
    }

    private Color BlendColor(Color targetColor, Color original)
    {
        return original.BlendWith(targetColor, this.blendfactor, this.alpha);
    }

    void SetColor() {
        ParticleSystem.MinMaxGradient startColor = ps.main.startColor;
        // Check if the gradient mode is "Random Between Two Colors"
        if (startColor.mode == ParticleSystemGradientMode.TwoColors)
        {
            // Blend with the existing colors
            startColor.colorMin = BlendColor(startColor.colorMin, MyColor());
            startColor.colorMax = BlendColor(startColor.colorMax, MyColor());
        }
    }

    void CheckForCompletion() {
        // Check if the Particle System has stopped playing
        if (ps != null && !ps.IsAlive())
        {
            // Destroy this GameObject
            PhotonNetwork.Destroy(gameObject);
        }
    }

    protected override void WriteSerializeView(PhotonStream stream, PhotonMessageInfo info) {}
    protected override void ReadSerializeView(PhotonStream stream, PhotonMessageInfo info) {}
}
