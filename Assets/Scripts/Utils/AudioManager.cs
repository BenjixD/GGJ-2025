using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource SFXSource;
    public AudioSource walkingSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip shoot;
    public AudioClip heal;
    public AudioClip death;
    public AudioClip walk;
    public AudioClip stomp;

    private Dictionary<string, AudioClip> sfxClips;


    private void Start()
    {
        // Initialize the dictionary
        sfxClips = new Dictionary<string, AudioClip>
        {
            { "shoot", shoot },
            { "heal", heal },
            { "death", death },
            { "walk", walk },
            { "stomp", stomp }
        };

        // Ensure the audio starts only after the player is instantiated
        StartCoroutine(WaitForPlayerAndPlayMusic());
    }

    private IEnumerator WaitForPlayerAndPlayMusic()
    {
        while (GameObject.FindWithTag("Player") == null)
        {
            yield return null;
        }

        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(string clipName)
    {
        if (sfxClips.TryGetValue(clipName, out AudioClip clip))
        {
            SFXSource.PlayOneShot(clip);
        }
    }
}
