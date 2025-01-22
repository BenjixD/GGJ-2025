using UnityEngine;

public class PlayerColors {
    public static Color[] COLORS = new Color[] {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan,
        Color.magenta,
    };
}

public static class ColorUtilityExtensions
{
    /// <summary>
    /// Blends a base color with a target color and adjusts alpha.
    /// </summary>
    /// <param name="baseColor">The original color to blend.</param>
    /// <param name="blendColor">The target color to blend towards.</param>
    /// <param name="blendFactor">How much to blend (0.0 = original, 1.0 = target).</param>
    /// <param name="alpha">Alpha (transparency) of the resulting color.</param>
    /// <returns>A new blended Color.</returns>
    public static Color BlendWith(this Color baseColor, Color blendColor, float blendFactor, float alpha)
    {
        float r = Mathf.Lerp(baseColor.r, blendColor.r, blendFactor);
        float g = Mathf.Lerp(baseColor.g, blendColor.g, blendFactor);
        float b = Mathf.Lerp(baseColor.b, blendColor.b, blendFactor);
        return new Color(r, g, b, alpha); // Return the blended color with the specified alpha
    }
}
