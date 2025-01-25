using UnityEngine;
using TMPro;

public class LobbyCopyClipboardUIElement : LobbyUIElement
{
    public TMP_Text textToCopy;
    public int skipCharacters = 8; // Skip "Room ID: " prefix
    public void CopyText()
    {
        if (textToCopy != null)
        {
            string fullText = textToCopy.text;

            // Ensure we don't try to skip more characters than exist in the text
            string textToCopyToClipboard = fullText.Length > skipCharacters
                ? fullText.Substring(skipCharacters)
                : "";

            GUIUtility.systemCopyBuffer = textToCopyToClipboard;
        }
    }
}
