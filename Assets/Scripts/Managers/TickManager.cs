using System;
using UnityEngine;

/// <summary>
/// Centralized Tick Manager for consistent tick updates
/// </summary>
public class TickManager : MonoBehaviour
{
    public static TickManager IN;

    private float tickInterval = .02f;
    private float secondInterval = 1f;

    private float tickTimer;
    private float secondTimer;
    public static event Action OnTick;
    public static event Action OnSecondTick;

    private void Update()
    {
        this.tickTimer += Time.deltaTime;

        if (this.tickTimer >= this.tickInterval)
        {
            this.tickTimer -= this.tickInterval;
            OnTick?.Invoke();
        }

        this.secondTimer += Time.deltaTime;
        if (this.secondTimer >= this.secondInterval)
        {
            this.secondTimer -= this.secondInterval;
            OnSecondTick?.Invoke();
        }
    }
}
