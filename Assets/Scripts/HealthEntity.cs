using System;
using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class HealthEntity : MonoBehaviour
{
    public int Health;

    public bool IsDead { get; private set; }

    public bool IsObjectPooled;

    public HealthEntityParent ParentHealthEntity { get; set; }

    public Action<HealthEntity> OnDie;
    public Action<int> OnHealthChanged;
    public Action<HealthEntity> OnChildDie;
    public int MaxHealth { get; private set; }
    private Healthbar healthbar;
    protected bool isInitialized;
    public EDestroyMode DestroyMode = EDestroyMode.HideObject;

    [Header("Optional")]
    [SerializeField] private Healthbar healthbarPrefab;
    [Range(-1, 5)] [SerializeField] private float delayUntilHideHealthbar = -1;
    [SerializeField] private Vector3 healthbarOffset = Vector3.zero;

    [SerializeField] private TextMeshPro healthText;

    [SerializeField] private CrackDamageDisplay crackDamageDisplay;

    [Range(0, 5)] public float PercentInfluenceOnParent = 0f;

    private bool isSavedDataApplied;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private float partialDamageAccumulated = 0f;

    public enum EDestroyMode
    {
        DestroyObject,
        HideObject
    }

    public static HealthEntity GetHealthEntity(GameObject inTarget, bool inShouldIncludeHealthEntityParent = true, bool inShouldIncludeChildren = true)
    {
        var healthObj = inTarget.GetComponent<HealthEntity>();

        if (healthObj == null && inShouldIncludeHealthEntityParent)
            healthObj = inTarget.GetComponentInChildren<HealthEntityParent>(true);

        if (healthObj == null && inShouldIncludeChildren)
            healthObj = inTarget.GetComponentInChildren<HealthEntity>(true);

        return healthObj;
    }

    protected virtual void Start()
    {
        if (!this.isSavedDataApplied)
        {
            this.MaxHealth = this.Health;
            this.IsDead = false;
        }

        if (this.isInitialized) return;

        this.isInitialized = true;

        this.originalScale = this.transform.localScale;
        this.originalRotation = this.transform.localRotation;
    }

    public virtual void Init(int inHealth = -1, int inMaxHealth = -1)
    {
        HideHealthText();

        if (this.isSavedDataApplied)
        {
            if (inMaxHealth != -1)
            {
                this.MaxHealth = inMaxHealth;
                RefreshCrackDamageDisplay();
            }
              
            return;
        }
        
        if (inHealth != -1)
            this.Health = inHealth;

        if (inMaxHealth != -1)
            this.MaxHealth = inMaxHealth;

        this.partialDamageAccumulated = 0f;

        RefreshCrackDamageDisplay();

        Start();
    }

    private void OnEnable()
    {
        this.OnDie += RemoveSelf;

        if (this.healthbarPrefab != null && this.healthbar == null)
        {
            StartCoroutine(SpawnHealthbarCo());
        }
    }

    private IEnumerator SpawnHealthbarCo()
    {
        while(GameManager.IN == null)
        {
            yield return null;
        }

        //Debug.Log($"HealthEntity.SpawnHealthbarCo()   Application.isPlaying = {Application.isPlaying}   go = {this.name}", this.gameObject);

        this.healthbar = Pool.Spawn<Healthbar>(this.healthbarPrefab.name, UI.IN.UiElementsContainer, this.transform.position, Quaternion.identity);

        this.healthbar.Init(this.transform, this.MaxHealth, this.delayUntilHideHealthbar);

        if (!this.healthbarOffset.Equals(Vector3.zero))
            this.healthbar.SetOffset(this.healthbarOffset);

        yield break;
    }

    private void OnDisable()
    {
        this.OnDie -= RemoveSelf;
        
        StopAllCoroutines();

        if(this.healthbar != null)  
            this.healthbar.Hide();
    }

    public void TakeDamage(float inAmount)
    {
        if (this.IsDead || inAmount <= 0 || !this.enabled) return;

        var amountInt = Mathf.FloorToInt(inAmount);

        if(inAmount < 1f)
        {
            this.partialDamageAccumulated += inAmount;
            amountInt = Mathf.FloorToInt(this.partialDamageAccumulated);
            this.partialDamageAccumulated -= amountInt;
        }

        this.Health -= amountInt;

        //Debug.Log($"{this.name} takes damage: {this.Health}/{this.MaxHealth}", this.gameObject);

        if (this.GetComponent<HealthEntityParent>() == null)
        {
            var damageToParent = Mathf.RoundToInt(inAmount * this.PercentInfluenceOnParent);
            //Debug.Log($"PARENT {this.name} takes damageToParent: {damageToParent}", this.gameObject);

            this.OnHealthChanged?.Invoke(damageToParent);
        }

        if (this.healthbar != null)
            this.healthbar.ChangeValue(-amountInt);

        if (this.Health <= 0)
        {
            this.partialDamageAccumulated = 0f;
            this.Health = 0;
            Die();
        }

        RefreshCrackDamageDisplay();
        ShowHealthText();
        Flash();
        Scale();
        RotateOffset();
    }

    private void RefreshCrackDamageDisplay()
    {
        if (this.crackDamageDisplay)
        {
            this.crackDamageDisplay.SetCrackColor();
            float healthPercent = (float)this.Health / (float)this.MaxHealth;
            float crackAmount = 1f - healthPercent;

            if (Health > MaxHealth)
                print($"<color=red>OVER!!!  Health: {this.Health}, MaxHealth: {this.MaxHealth}.    name: {this.name}</color>");

            this.crackDamageDisplay.ShowCrackDamage(crackAmount);
        }
    }

    private void Flash()
    {
        if (this.crackDamageDisplay)
        {
            this.crackDamageDisplay.SetFlashColor();
            var flashAmount = 0.1f;
            this.crackDamageDisplay.SetFlashAmount(flashAmount);

            DOTween.To(() => flashAmount, x => this.crackDamageDisplay.SetFlashAmount(x), 0f, 0.1f).SetEase(Ease.OutQuad);
        }
    }

    private void Scale()
    {
        if (this.crackDamageDisplay)
        {
            var scaledUp = this.originalScale * .9f;
            this.transform.localScale = scaledUp;
            this.transform.DOScale(this.originalScale, 0.15f).SetEase(Ease.OutQuad);
        }
    }
    
    private void RotateOffset()
    {
        if (this.crackDamageDisplay)
        {
            var randomZ = UnityEngine.Random.Range(-5f, 5f);
            var rotated = Quaternion.Euler(this.originalRotation.eulerAngles.x, this.originalRotation.eulerAngles.y, this.originalRotation.eulerAngles.z + randomZ);
            this.transform.localRotation = rotated;
        }
    }

    private void ShowHealthText()
    {
        if (this.healthText && !this.IsDead && LootManager.IN.DebugShowHealthText)
        {
            this.healthText.gameObject.SetActive(true);

            if (this.Health <= this.MaxHealth)
                this.healthText.text = $"{this.Health}";
            else
                this.healthText.text = $"<color=red>{this.Health}/{this.MaxHealth}</color>";

            CancelInvoke(nameof(HideHealthText));
            Invoke(nameof(HideHealthText), .5f);
        }
    }

    private void HideHealthText()
    {
        if (this.healthText)
            this.healthText.gameObject.SetActive(false);
    }

    public bool AddHealth(float inAmount)
    {
        if (this.Health == this.MaxHealth) return false;

        if(inAmount < 1f)
        {
            this.partialDamageAccumulated += inAmount;
            inAmount = Mathf.FloorToInt(this.partialDamageAccumulated);
            this.partialDamageAccumulated -= inAmount;
        }

        this.Health += Mathf.FloorToInt(inAmount);
        this.Health = Mathf.Clamp(this.Health, 0, this.MaxHealth);

        this.IsDead = this.Health <= 0;

        if (this.healthbar != null)
            this.healthbar.ChangeValue(Mathf.FloorToInt(inAmount));

        ShowHealthText();

        return true;
    }

    public virtual void Die()
    {
        this.IsDead = true;

        HideHealthText();

        var isParent = this.GetComponent<HealthEntityParent>() != null;

        if(!isParent)
            this.OnChildDie?.Invoke(this);

        this.OnDie?.Invoke(this);
    }

    public void ApplySaveData(int inHealth)
    {
        this.Health = inHealth;
        this.IsDead = inHealth <= 0;

        if (this.IsDead && this.gameObject.activeInHierarchy)
            this.gameObject.SetActive(false);

        this.isSavedDataApplied = true;

        RefreshCrackDamageDisplay();
    }

    /// <summary>
    /// Registered to OnDie as default behaviour
    /// </summary>
    private void RemoveSelf(HealthEntity inHealthEntity = null)
    {
        //Debug.Log($"RemoveSelf   this = {this.name}");

        var success = Pool.Despawn(this.gameObject, false);

        if (!success)
        {
            if (this.DestroyMode == EDestroyMode.DestroyObject)
                Destroy(this.gameObject);
            else if (this.DestroyMode == EDestroyMode.HideObject)
                this.gameObject.SetActive(false);
        }

        OnDestroy();

        this.Health = this.MaxHealth;
    }

    public virtual void OnDestroy()
    {
        this.OnChildDie = null;
        this.OnHealthChanged = null;
    }
}