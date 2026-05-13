using UnityEngine;

public class EffectsTrigger : MonoBehaviour
{
    public ParticleSystem[] particleSystems;
    public AudioSource[] soundEffects;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            foreach (var effect in particleSystems)
            {
                effect.Play();
            }

            if (!GameState.Instance.isPlaying) return;
            foreach (var sound in soundEffects)
            {
                sound.PlayOneShot(sound.clip);
            }
        }
    }
}