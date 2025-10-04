using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource idleSound;
    public AudioSource movingSound;
    public AudioSource boostingSound;
    public AudioSource driftingSound;
    public AudioSource jumpSound;

    public void PlaySound(AudioSource audio)
    {
        audio.PlayOneShot(audio.clip);
    }

    public void StartSound(AudioSource audio)
    {
        if (audio.isPlaying) return;
        audio.Play();
    }
    public void StopSound(AudioSource audio)
    {
        audio.Stop();
    }
}