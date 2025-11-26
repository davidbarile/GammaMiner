using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KaleidoscopeController : MonoBehaviour
{
    [SerializeField] private Camera triangleRenderCamera;
    [SerializeField] private Image overlay;
    [SerializeField] private Animation vectorArtAnim;
    [SerializeField] private RotateObject kaleidRotateObj;
    [SerializeField] private ParticleSystem[] particles;

    public static KaleidoscopeController IN;

    public void SetCameraBgColor(Color inColor)
    {
        this.triangleRenderCamera.backgroundColor = inColor;
    }

    public void SetOverlayColor(Color inColor)
    {
        this.overlay.color = inColor;
    }

    public void SetVectorArtVisible(bool inIsVisible)
    {
        this.vectorArtAnim.gameObject.SetActive(inIsVisible);
    }

    public void SetVectorArtAnimSpeed(float inSpeed)
    {
        var clipName = this.vectorArtAnim.clip.name;
        this.vectorArtAnim[clipName].speed = inSpeed * 5f;
    }

    public void SetRotationSpeed(float inSpeed)
    {
        this.kaleidRotateObj.SetRotationAmount(0, (inSpeed - .5f) * .1f, 0);
    }

    public void SetParticlesVisible(bool inIsVisible)
    {
        foreach (var particle in this.particles)
        {
            particle.gameObject.SetActive(inIsVisible);
        }
    }

    public void SetParticlesLooping(bool inShouldLoop)
    {
        foreach (var ps in this.particles)
        {
            var main = ps.main;
            main.loop = inShouldLoop;
        }
    }

    public void SetParticlesPrewarm(bool inShouldPrewarm)
    {
        foreach (var ps in this.particles)
        {
            var main = ps.main;
            main.prewarm = inShouldPrewarm;
        }
    }

    public void SetParticleColors(Color inColorMin, Color inColorMax)
    {
        foreach (var ps in this.particles)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(inColorMin, inColorMax);
        }
    }

    public void SetParticleSpeed(float inSpeed)
    {
        foreach (var ps in this.particles)
        {
            var main = ps.main;
            main.simulationSpeed = inSpeed;
        }
    }

    public void SetParticleDuration(float inDuration)
    {
        foreach (var ps in this.particles)
        {
            var main = ps.main;
            main.duration = inDuration;
        }
    }

    public void SetParticleEmission(float inEmitRate)
    {
        foreach (var ps in this.particles)
        {
            var emission = ps.emission;
            emission.rateOverTime = inEmitRate;
        }
    }

    public void HandleParticleButtonDown()
    {
        SetParticleEmission(55);
        SetParticlesVisible(true);
    }

    public void HandleParticleButtonUp()
    {
        SetParticleEmission(0);
        StartCoroutine(HideParticles());
    }

    private IEnumerator HideParticles()
    {
        yield return new WaitForSeconds(5);
        SetParticlesVisible(false);
        yield break;
    }
}
