using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerHUD : MonoBehaviour
{
    [Header("Configurations")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    public float lerpDuration = 2f;
    public float typingSpeed = 0.1f;
    private GameManager gameManager;
    private Image background;
    private TMP_Text winnerText;

    private void Start()
    {
        gameManager = GetComponentInParent<GameManager>();
        background = GetComponentInChildren<Image>();
        winnerText = GetComponentInChildren<TMP_Text>();
        StartCoroutine(PlayWinner());
    }

    IEnumerator PlayWinner() {
        // Fade the background color
        Color startColor = background.color;
        float elapsedTime = 0f;
        while (elapsedTime < lerpDuration)
        {
            // Gradually interpolate between the start and target color
            background.color = Color.Lerp(startColor, backgroundColor, elapsedTime / lerpDuration);
            // Increment elapsed time
            elapsedTime += Time.deltaTime;
            // Wait until the next frame
            yield return new WaitForFixedUpdate();
        }
        // Ensure the final color is set
        background.color = backgroundColor;

        // Play the winning player
        string playerName = gameManager.GetWinningPlayerName();
        string fullText = $"{playerName} Wins!";
        foreach (char c in fullText)
        {
            winnerText.text += c; // Add one character at a time
            yield return new WaitForSeconds(typingSpeed); // Wait for the typing speed duration
        }

    }
}
