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

#if UNITY_WEBGL && !UNITY_EDITOR
            CopyToClipboard(textToCopyToClipboard);
#else
            GUIUtility.systemCopyBuffer = textToCopyToClipboard;
#endif
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void CopyToClipboard(string text);
#endif
}
