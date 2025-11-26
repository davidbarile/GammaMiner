using UnityEngine;

public class CrackDamageDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer rockOutlineSpriteRenderer;
    [SerializeField] private SpriteRenderer crackSpriteRenderer;

    [SerializeField] private Color crackColor = Color.black;
    [SerializeField] private Color flashColor = Color.white;

    [SerializeField, Range(0f, 1f)] private float crackAmount;

    private Material crackMaterial;

    private void Start()
    {
        SetCrackColor();
        SetFlashColor();
        SetFlashAmount(0f);
    }

    public void SetFlashAmount(float inFlashAmount)
    {
        if (this.crackMaterial == null)
            this.crackMaterial = this.crackSpriteRenderer.material;

        this.crackSpriteRenderer.material.SetFloat("_FlashAmount", inFlashAmount);

        this.crackSpriteRenderer.material = this.crackMaterial;
    }

    public void SetFlashColor(Color inFlashColor = default)
    {
        if (this.crackMaterial == null)
            this.crackMaterial = this.crackSpriteRenderer.material;

        this.crackSpriteRenderer.material.SetColor("_FlashColor", inFlashColor == default ? this.flashColor : inFlashColor);

        this.crackSpriteRenderer.material = this.crackMaterial;
    }

    public void SetCrackColor()
    {
        if (this.crackMaterial == null)
            this.crackMaterial = this.crackSpriteRenderer.material;

        var rockColor = this.rockOutlineSpriteRenderer != null ? this.rockOutlineSpriteRenderer.color : this.crackColor;

        this.crackMaterial.SetColor("_CrackColor", rockColor);
        this.crackSpriteRenderer.material = this.crackMaterial;
    }

    public void ShowCrackDamage(float inCrackAmount)
    {
        if (this.crackMaterial == null)
            this.crackMaterial = this.crackSpriteRenderer.material;

        this.crackSpriteRenderer.material.SetFloat("_CrackAmount", inCrackAmount);

        this.crackSpriteRenderer.material = this.crackMaterial;
    }

    private void OnDestroy()
    {
        if (this.crackMaterial != null)
        {
            Destroy(this.crackMaterial);
            this.crackMaterial = null;
        }
    }
}