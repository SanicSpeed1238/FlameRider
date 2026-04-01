using UnityEngine;

public class EffectsTrigger : MonoBehaviour
{
    public ParticleSystem[] particleSystems;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            foreach (var effect in particleSystems)
            {
                effect.Play();
            }
        }
    }
}