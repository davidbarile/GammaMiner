using UnityEngine;
using static Explosion;
using static MiningToolConfig;

public class RailRound : MonoBehaviour
{
    public EMiningToolType MiningToolType;
    [SerializeField] private Rigidbody2D rigidBody2d;

    [SerializeField] private SpriteRenderer sr1;
    [SerializeField] private SpriteRenderer sr2;
    [SerializeField] private SpriteRenderer sr_trail;

    [SerializeField] private DamageEntity_Collision damageEntity;

    private Explosion explosion = null;

    private void Awake()
    {
        this.damageEntity = this.GetComponent<DamageEntity_Collision>();
        //this.damageEntity.OnDamage += Expire;
    }

    private void Start()
    {
        var miningToolConfig = GlobalData.GetMiningToolConfig(this.MiningToolType);
        if (miningToolConfig != null)
            this.damageEntity.SetMiningToolConfig(miningToolConfig);
    }

    public void Shoot(float inForce, float inRange, Vector2 inShipVelocity)
    {
        if (this.explosion != null)
        {
            Pool.Despawn(this.explosion.gameObject);
            this.explosion = null;
        }

        ResetRigidBody();

        this.rigidBody2d.linearVelocity = inShipVelocity;
        this.rigidBody2d.AddForce(inForce * this.transform.up, ForceMode2D.Impulse);

        CancelInvoke();
        Invoke(nameof(Expire), inRange);
    }

    public void Colorize(Color inColor1, Color inColor2, Color inTrailColor)
    {
        if(this.sr1) this.sr1.color = inColor1;
        if(this.sr2) this.sr2.color = inColor2;
        if(this.sr_trail) this.sr_trail.color = inTrailColor;
    }

    private void OnCollisionEnter2D(Collision2D inCollision)
    {
        if (inCollision.gameObject != null && inCollision.gameObject.activeInHierarchy)
        {
            if (inCollision.gameObject.TryGetComponent<HealthEntity>(out var hitHealthEntity))
            {
                //hitHealthEntity.TakeDamage(this.BaseDamage);
            }

            Explode(hitHealthEntity);
        }
    }

    private void Explode(HealthEntity inHealthEntity = null)
    {
        if (this.explosion != null) return;

        this.explosion = Pool.Spawn<Explosion>("Explosion-Railround", GameManager.IN.ProjectilesContainer, this.transform.position, Quaternion.identity);

        //Debug.Log($"Explode({inHealthEntity})  this.explosion = {this.explosion.name}  .GUID = {this.explosion?.GetInstanceID()}   this.GUID = {this.GetInstanceID()}    active = {this.gameObject.activeInHierarchy}   frame = {Time.frameCount}", this.gameObject);

        if (inHealthEntity)
        {
            if (inHealthEntity.TryGetComponent<Rock>(out var rock))
            {
                if (rock.Fill)
                    this.explosion.SetParticleColors(rock.Fill.color);
            }

            var miningToolConfig = GlobalData.GetMiningToolConfig(this.MiningToolType);
            if (miningToolConfig != null)
            {
                this.explosion.SetMiningToolConfig(miningToolConfig);
            }
        }

        //this.explosion.PlayParticles();
        this.explosion.Explode(0, inHealthEntity, false, 0, false, EExplosionIgnore.All);

        Expire();
    }

    private void Expire()
    {
        CancelInvoke();

        ResetRigidBody();

        Pool.Despawn(this.gameObject);
    }

    private void ResetRigidBody()
    {
        this.rigidBody2d.linearVelocity = Vector2.zero;
        this.rigidBody2d.angularVelocity = 0;
    }

    private void OnDestroy()
    {
        //Debug.Log($"RailRound.OnDestroy()   Application.isPlaying = {Application.isPlaying}   go = {gameObject}");
        //this.damageEntity.OnDamage -= Expire;
    }
}