using UnityEngine;
using static Explosion;
using static MiningToolConfig;

public class Missile : MonoBehaviour
{
    public EMiningToolType MiningToolType;
    [Range(1, 1000)] public int BaseDamage;
    [Range(1, 5)] public float LockTargetDamageMultiplier = 1;
    [Range(0, 10)] public float RangeInSeconds;
    [Range(0, 100)] public float RangeToTargetPointToExplode;
    [Space]
    [Range(0, 1000)] public float AccelerationRate;
    [Range(0, 100)] public float MaxVelocity;
    [Space]
    [Range(0, 10)] public float DelayToRotate;
    [Space]
    [SerializeField] private Rigidbody2D rigidBody2d;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Transform explosionSpawnPoint;

    [Space]
    [SerializeField] private float rotateSpeed = 1000;
    [SerializeField] private float maxDistancePredict = 100;
    [SerializeField] private float minDistancePredict = 5;
    [SerializeField] private float maxTimePrediction = 5;

    private Vector3 standardPrediction;

    private Vector3 targetPosition;
    private Rigidbody2D lockedTarget;
    private float launchTime = -1;
    private HealthEntity healthEntity;

    private TargetPoint targetPoint;

    private float delayToAccelerate;

    private HealthEntity lockedTargetHealthEntity;

    private void Awake()
    {
        this.healthEntity = this.GetComponent<HealthEntity>();

        if (this.healthEntity != null)
            healthEntity.OnDie += Expire;
    }

    public void Shoot(Vector2 inForce, Vector2 inShipVelocity, float inDelayToAccelerate)
    {
        Reset();

        this.delayToAccelerate = inDelayToAccelerate;

        this.lockedTarget = CursorManager.IN.LockedTarget;

        this.lockedTargetHealthEntity = null;

        if (this.lockedTarget == null)
        {
            this.targetPoint = Pool.Spawn<TargetPoint>("Target Point", UI.IN.UiElementsContainer, CursorManager.IN.transform.position, Quaternion.identity);
            this.targetPoint.WorldPosition = CursorManager.IN.WorldPosition;

            this.targetPosition = this.targetPoint.WorldPosition;
        }
        else
        {
            this.targetPosition = this.lockedTarget.position;
            this.lockedTargetHealthEntity = HealthEntity.GetHealthEntity(this.lockedTarget.gameObject);
        }

        this.rigidBody2d.linearVelocity = inShipVelocity;

        this.rigidBody2d.AddForce(inForce * .1f, ForceMode2D.Impulse);

        this.launchTime = 0;
        this.trailRenderer.emitting = false;
    }

    private void FixedUpdate()
    {
        if (this.launchTime == -1) return;

        if(this.launchTime > this.RangeInSeconds)
        {
            Expire();
            return;
        }

        this.launchTime += Time.deltaTime;

        if (this.launchTime < this.delayToAccelerate)
        {
            this.trailRenderer.Clear();
        }
        else
        {
            //what needs to happen here is the following:
            //if locked target, seek it
            //if target point, seek it
            //if neither or reached target point, go straight
            var lockedTargetExistsAndNotDestroyed = this.lockedTarget != null && this.lockedTarget.gameObject.activeInHierarchy;
            this.lockIcon.SetActive(lockedTargetExistsAndNotDestroyed);

            if (lockedTargetExistsAndNotDestroyed)
            {
                this.targetPosition = this.lockedTarget.position;
            }
            else
            {
                if (this.targetPoint != null)
                {
                    //there never was a locked target
                    if (Vector3.Distance(this.transform.position, this.targetPosition) < 3f)
                    {
                        //if we reach the target point hide it and go straight
                        this.targetPoint.WorldPosition = Vector2.zero;
                        Pool.Despawn(this.targetPoint.gameObject);
                        this.targetPoint = null;

                        //this makes the missile go straight
                        this.standardPrediction = Vector3.zero;
                        this.targetPosition = Vector3.zero;
                    }
                }
                else
                {
                    //no locked target, no target point, go straight
                    this.standardPrediction = Vector3.zero;
                    this.targetPosition = Vector3.zero;
                }
            }

            this.lineRenderer.enabled = true;

            //this.lineRenderer.SetPosition(0, this.lineRenderer.transform.position);
            //this.lineRenderer.SetPosition(1, this.lockedTarget.transform.position);

            if (this.launchTime > this.DelayToRotate)
            {
                //if target position is set, seek it, otherwise just go straight
                if (!this.targetPosition.Equals(Vector3.zero))
                {
                    var distanceToTarget = Vector3.Distance(this.transform.position, this.targetPosition);
                    var leadTimePercentage = Mathf.InverseLerp(this.minDistancePredict, this.maxDistancePredict, distanceToTarget);

                    PredictMovement(leadTimePercentage);

                    //if locked on, once destroyed, do not seek target anymore
                    if (this.lockedTarget != null && this.lockedTargetHealthEntity != null && !this.lockedTargetHealthEntity.IsDead)
                        RotateRocket();
                }
                else
                    this.rigidBody2d.angularVelocity = 0f;

                this.rigidBody2d.linearVelocity = this.transform.up * this.MaxVelocity;

                //if in range of target point worldspace position, explode
                if (this.targetPoint != null)
                {
                    var targetPointSubtract = (Vector2)this.transform.position - (Vector2)this.targetPosition;

                    if (targetPointSubtract.magnitude < this.RangeToTargetPointToExplode)
                    {
                        Explode();
                        return;
                    }
                }
            }
            else
            {
                float ratio = 1 - Mathf.Clamp(this.rigidBody2d.linearVelocity.magnitude / this.MaxVelocity, 0, 1);
                this.rigidBody2d.AddForce(this.AccelerationRate * ratio * this.transform.up, ForceMode2D.Force);
            }

            this.trailRenderer.emitting = true;
        }
    }

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, this.maxTimePrediction, leadTimePercentage);

        var targetVelocity = Vector2.zero;

        if (this.lockedTarget != null)
            targetVelocity = this.lockedTarget.linearVelocity;

        this.standardPrediction = (Vector2)this.targetPosition + (targetVelocity * predictionTime);
    }

    private void RotateRocket()
    {
        var heading = this.standardPrediction - this.transform.position;

        var angle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
        var rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

        var secondsToEaseRotation = 1.5f;
        var rotModifier = Mathf.Clamp01((this.launchTime - this.DelayToRotate) / secondsToEaseRotation);//gradually increase rotation speed over time

        this.rigidBody2d.MoveRotation(Quaternion.RotateTowards(this.transform.rotation, rotation, this.rotateSpeed * rotModifier * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        if(this.standardPrediction.Equals(Vector3.zero)) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.standardPrediction);
    }

    private void OnCollisionEnter2D(Collision2D inCollision)
    {
        if (inCollision.gameObject != null && inCollision.gameObject.activeInHierarchy)
        {
            if (inCollision.gameObject.TryGetComponent<HealthEntity>(out var hitHealthEntity))
            {
               //hitHealthEntity.TakeDamage(this.BaseDamage);
            }

            //Debug.Log("OnCollisionEnter2D: this.hitTarget = " + this.hitTarget);

            Explode(hitHealthEntity);
        }
    }

    private void Explode(HealthEntity inHealthEntity = null)
    {
        var explosion = Pool.Spawn<Explosion>("Explosion", GameManager.IN.ProjectilesContainer, this.transform.position, Quaternion.identity);

        if (this.explosionSpawnPoint != null)
            explosion.transform.position = this.explosionSpawnPoint.position;

        var isTargetLocked = this.lockedTarget != null;

        var config = GlobalData.GetMiningToolConfig(this.MiningToolType);
        if (config != null)
        {
            explosion.SetMiningToolConfig(config);
        }

        explosion.Explode(this.BaseDamage, inHealthEntity, isTargetLocked, this.LockTargetDamageMultiplier, true, EExplosionIgnore.Missile);

        Expire();
    }

    private void Expire(HealthEntity inHealthEntity = null)
    {
        Reset();

        Pool.Despawn(this.gameObject);
    }

    private void Reset()
    {
        this.lockedTarget = null;
        this.lockedTargetHealthEntity = null;
        this.launchTime = -1;

        this.rigidBody2d.linearVelocity = Vector2.zero;
        this.rigidBody2d.angularVelocity = 0;

        this.trailRenderer.Clear();

        this.lineRenderer.enabled = false;

        this.lockIcon.SetActive(false);

        this.standardPrediction = Vector3.zero;
        this.targetPosition = Vector3.zero;

        if (this.targetPoint != null)
        {
            this.targetPoint.WorldPosition = Vector2.zero;
            Pool.Despawn(this.targetPoint.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (this.healthEntity != null)
            healthEntity.OnDie -= Expire;
    }
}