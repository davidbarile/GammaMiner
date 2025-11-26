using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private bool isPersistent;
    [Space]
    [SerializeField] private Image fill;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Gradient gradient;
    [SerializeField] private bool shouldColorFillByGradient;
    [SerializeField] private bool shouldShowPercent;
    [SerializeField] private bool shouldFormatWithCommas;
    [SerializeField] private Vector3 offset;

    private Transform target;
    new private Camera camera;
    private CanvasGroup canvasGroup;

    private int currentHealth;
    private int maxHealth;
    private float delayUntilHide = -1;
    private Tween fadeInTween;
    private Tween fadeOutTween;

    [SerializeField] private bool shouldColorTextOnDecrease;
    [ShowIf("shouldColorTextOnDecrease")]
    [SerializeField] private Color decreaseColor = Color.red;

    private int lastHealth;

    private bool isDecreasing;

    private void Awake()
    {
        this.camera = Camera.main;
        this.canvasGroup = this.GetComponent<CanvasGroup>();
        this.canvasGroup.alpha = 0;
    }

    public void Init(Transform inTarget, int inMaxHealth, float inDelayUntilHide = -1)
    {
        if (inTarget != null)
            SetTarget(inTarget);

        InitHealth(inMaxHealth);
        this.delayUntilHide = inDelayUntilHide;
    }

    public void SetOffset(Vector3 inOffset)
    {
        this.offset = inOffset;
    }

    private void SetTarget(Transform inTarget)
    {
        this.transform.localScale = Vector3.one;
        this.transform.localRotation = Quaternion.identity;
        this.target = inTarget;
    }

    private void InitHealth(int inMaxHealth)
    {
        this.currentHealth = inMaxHealth;
        this.maxHealth = inMaxHealth;
        this.lastHealth = inMaxHealth;

        SetValue(1, false);
    }

    public void ChangeValue(int inAmount)
    {
        this.currentHealth += inAmount;

        SetValue(this.currentHealth);

        if (inAmount > 0)
        {
            //show healing icon on bar briefly
        }
    }

    public void SetValue(int inAmount)
    {
        this.lastHealth = this.currentHealth;
        this.currentHealth = Mathf.Clamp(inAmount, 0, this.maxHealth);

        if (this.lastHealth != this.currentHealth)
        {
            this.isDecreasing = this.currentHealth < this.lastHealth;
        }
        else if(this.currentHealth == this.maxHealth)
        {
            this.isDecreasing = false;
        }

        if (this.maxHealth > 0)
            SetValue((float)this.currentHealth / this.maxHealth);
    }

    private void SetValue(float inPercent, bool inShouldShow = true)
    {
        this.fill.fillAmount = inPercent;

        if (this.label)
        {
            if (this.shouldShowPercent)
                this.label.text = $"{Mathf.RoundToInt(inPercent * 100)}%";
            else
            {
                if (this.shouldFormatWithCommas)
                {
                    if (this.shouldColorTextOnDecrease && this.isDecreasing)
                    {
                        var colorHex = this.decreaseColor.ToHexString();
                        if (this.label.textWrappingMode != TextWrappingModes.NoWrap)
                            this.label.text = $"<color=#{colorHex}>{currentHealth:N0}</color>\n{this.maxHealth:N0}";
                        else
                            this.label.text = $"<color=#{colorHex}>{currentHealth:N0}</color>/{this.maxHealth:N0}";
                    }
                    else
                    {
                        if (this.label.textWrappingMode != TextWrappingModes.NoWrap)
                            this.label.text = $"{currentHealth:N0}\n{this.maxHealth:N0}";
                        else
                            this.label.text = $"{currentHealth:N0}/{this.maxHealth:N0}";
                    }
                }
                else
                {
                    if (this.shouldColorTextOnDecrease && this.isDecreasing)
                    {
                        var colorHex = this.decreaseColor.ToHexString();
                        if (this.label.textWrappingMode != TextWrappingModes.NoWrap)
                            this.label.text = $"<color=#{colorHex}>{currentHealth}</color>\n{this.maxHealth}";
                        else
                            this.label.text = $"<color=#{colorHex}>{currentHealth}</color>/{this.maxHealth}";
                    }
                    else
                    {
                        if (this.label.textWrappingMode != TextWrappingModes.NoWrap)
                            this.label.text = $"{currentHealth}\n{this.maxHealth}";
                        else
                            this.label.text = $"{currentHealth}/{this.maxHealth}";
                    }
                }
            }
        }

        if (this.shouldColorFillByGradient)
            this.fill.color = this.gradient.Evaluate(inPercent);

        if (inShouldShow)
            FadeIn();

        if (inPercent == 0)
            Hide();
        else if (this.delayUntilHide != -1)
            FadeOut();
    }

    public void SetGradient(Gradient inGradient)
    {
        this.gradient = inGradient;
        this.fill.color = this.gradient.Evaluate(this.fill.fillAmount);
    }

    private void LateUpdate()
    {
        if (this.target != null)
        {
            this.transform.position = this.camera.WorldToScreenPoint(this.target.transform.position);
            this.transform.localPosition += this.offset;
        }
    }

    public void Hide()
    {
        if (!this.isPersistent)
        {
            InitHealth(this.maxHealth);
            Pool.Despawn(this.gameObject);
        }
    }

    private void FadeIn()
    {
        if (!this.canvasGroup || this.canvasGroup.alpha == 1) return;

        if (this.fadeOutTween != null)
            this.fadeOutTween.Kill();

        if (this.fadeInTween != null)
            this.fadeInTween.Kill();

        float duration = .05f;

        if (this.canvasGroup.alpha > 0)
            duration *= (1 / this.canvasGroup.alpha);

        //Debug.Log($"FadeIn   this.canvasGroup.alpha = {this.canvasGroup.alpha}");

        this.fadeInTween = this.canvasGroup.DOFade(1, duration).SetEase(Ease.OutSine);
    }

    private void FadeOut()
    {
        if (this.fadeOutTween != null)
            this.fadeOutTween.Kill();

        //Debug.Log($"FadeOut   this.canvasGroup.alpha = {this.canvasGroup.alpha}    delayUntilHide = {delayUntilHide}");

        this.fadeOutTween = this.canvasGroup.DOFade(0, .2f).SetEase(Ease.InSine).SetDelay(this.delayUntilHide);
    }
}