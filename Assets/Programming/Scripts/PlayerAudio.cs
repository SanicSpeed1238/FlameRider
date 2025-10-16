using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource idleSound;
    public AudioSource movingSound;
    public AudioSource boostingSound;
    public AudioSource driftingSound;
    public AudioSource jumpSound;
    public AudioSource respawnSound;

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

    public void StopAllAudio()
    {
        idleSound.volume = 0;
        movingSound.volume = 0;
        boostingSound.volume = 0;
        driftingSound.volume = 0;
        jumpSound.volume = 0;
        respawnSound.volume = 0;
    }
}