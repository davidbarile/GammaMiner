using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMapGridCell : MonoBehaviour
{
    [ReadOnly]
    public Vector2Int Coords;

    public float Rotation => this.tileThumbnail.transform.rotation.eulerAngles.z;

    [SerializeField] private Image image;
    [SerializeField] private Image tileThumbnail;
    [SerializeField] private Image startIcon;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private TextMeshProUGUI coordsText;
    [Space]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private Color selectedColor = Color.white;

    private bool isSelected;

    public void SetCoords(int inX, int inY)
    {
        this.image.color = this.normalColor;
        this.Coords = new Vector2Int(inX, inY);
        this.coordsText.text = $"{inX},{inY}";
        this.gameObject.name = $"Cell ({inX},{inY})";
    }

    public void SetStartIconVisible(bool inIsVisible)
    {
        this.startIcon.gameObject.SetActive(inIsVisible);
    }

    public void SetTileSprite(MapTileData inData)
    {
        if (inData != null)
            this.tileThumbnail.sprite = inData.TileThumbnail;
        else
        {
            this.tileThumbnail.sprite = this.emptySprite;
            this.tileThumbnail.transform.rotation = Quaternion.identity;
        }
    }

    public void SetRotation(float inRotation = 0f)
    {
        inRotation = Mathf.Repeat(inRotation, 360f);
        this.tileThumbnail.transform.rotation = Quaternion.Euler(0f, 0f, inRotation);
    }

    public void HandleRollOver()
    {
        this.image.color = this.isSelected ? this.selectedColor : this.hoverColor;

        if (Input.GetMouseButton(0))
            UIMapEditor.IN.SetSelectedCell(this);
    }

    public void HandleRollOut()
    {
        this.image.color = this.isSelected ? this.selectedColor : this.normalColor;
    }

    public void HandleClick()
    {
        UIMapEditor.IN.SetSelectedCell(this);
    }

    public void SetSelected(bool inIsSelected)
    {
        this.isSelected = inIsSelected;
        this.image.color = this.isSelected ? this.selectedColor : this.normalColor;
    }
}