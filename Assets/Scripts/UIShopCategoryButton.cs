using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopCategoryButton : MonoBehaviour
{
    public bool IsSelected;

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private Graphic[] selectedHighlightGraphics;
    [SerializeField] private Color[] selectedHighlightColors;


    public ShopCategoryData CategoryData { get; private set; }

    public void Configure(ShopCategoryData inData)
    {
        this.CategoryData = inData;
        this.label.text = inData.Name;
        this.icon.sprite = inData.Sprite;
    }

    public void SetSelected(bool inIsSelected)
    {
        if (this.selectedHighlight)
            this.selectedHighlight.SetActive(inIsSelected);
            
        foreach (var graphic in this.selectedHighlightGraphics)
        {
            graphic.color = inIsSelected ? this.selectedHighlightColors[1] : this.selectedHighlightColors[0];
        }
    }

    public void HandleClick()
    {
        UIShopPanel.IN.SelectCategory(this);
    }
}