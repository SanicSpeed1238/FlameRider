using UnityEngine;
using System.Collections;

public class FlameTrailObject : MonoBehaviour
{
    public float speedBoost = 1f;

    public void CullParticles()
    {
        StartCoroutine(CullParticlesTimer());
    }
    IEnumerator CullParticlesTimer()
    {
        yield return new WaitForSeconds(2f);
        ParticleSystem trailParticles = GetComponentInChildren<ParticleSystem>();
        var main = trailParticles.main;
        main.cullingMode = ParticleSystemCullingMode.Pause;
    }
}