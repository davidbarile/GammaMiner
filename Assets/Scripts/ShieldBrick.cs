using System;
using UnityEngine;

public class ShieldBrick : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRender;
    [SerializeField] private Rigidbody2D rigidBody2d;

    public float MaxHealth;

    public Gradient HealthGradient { get; set; }

    public Action<float> OnBrickHealthChanged;

    public float DebugHealth;

    public float Health
    {
        get { return this.health; }

        set
        {
            float prevHealth = this.health;

            this.health = Mathf.Max(0, value);

            float deltaHealth = this.health - prevHealth;

            this.OnBrickHealthChanged?.Invoke(deltaHealth);

            if(this.MaxHealth > 0)
            {
                float percent = this.health / this.MaxHealth;
                this.spriteRender.color = this.HealthGradient.Evaluate(percent);
            }

            this.DebugHealth = this.health;

            this.gameObject.SetActive(this.health > 0);
        }
    }
    private float health;
}
