using UnityEngine;

public class HealthEntityParent : HealthEntity
{
    public HealthEntity[] ChildHealthEntities => this.childHealthEntities;
    [SerializeField] private HealthEntity[] childHealthEntities = new HealthEntity[0];

    private bool isParentInitialized;
    private bool isPlayer;

    protected override void Start()
    {
        base.Start();

        if (this.isParentInitialized) return;
        this.isParentInitialized = true;

        if(this.childHealthEntities.Length == 0)
            this.childHealthEntities = this.GetComponentsInChildren<HealthEntity>(true);

        foreach (var child in this.childHealthEntities)
        {
            if (child == this) continue;

            child.ParentHealthEntity = this;
            
            child.OnHealthChanged += HandleChildHealthChange;
            child.OnChildDie += HandleChildDie;
        }
    }

    public void Init(int inHealth = -1, int inMaxHealth = -1, bool inIsPlayer = false)
    {
        this.isPlayer = inIsPlayer;

        base.Init(inHealth, inMaxHealth);

        Start();
    }

    private void HandleChildHealthChange(int inAmount)
    {
        //Debug.Log($"HandleChildHealthChange {inAmount}   this = {this.name}", this.gameObject);
        TakeDamage(inAmount);

        if(this.isPlayer)
            HUD.OnShipHealthChanged?.Invoke(this.Health);
    }

    private void HandleChildDie(HealthEntity inHealthEntity)
    {

    }

    public override void OnDestroy()
    {
        foreach (var child in this.childHealthEntities)
        {
            child.OnHealthChanged -= HandleChildHealthChange;
            child.OnChildDie -= HandleChildDie;
        }

        base.OnDestroy();
    }

    public void AddHealthToChildren(int inAmount)
    {
        //Debug.Log($"{this.name}  AddHealthToChildren({inAmount})");

        var amountLeft = inAmount;
        if (this.IsDead) return;

        bool ableToAdd = false;

        while (amountLeft > 0 && ableToAdd)
        {
            ableToAdd = false;

            foreach (var child in this.childHealthEntities)
            {
                if (!child.IsDead)
                {
                    var success = child.AddHealth(1);

                    if (success)
                    {
                        --amountLeft;
                        ableToAdd = true;
                    }
                }
            }

            var parentSuccess = AddHealth(1);

            if (parentSuccess)
            {
                --amountLeft;
                ableToAdd = true;
            }
        }

        if (amountLeft > 0)
            Debug.Log($"Added {inAmount - amountLeft}   {inAmount} remaining");
    }
}