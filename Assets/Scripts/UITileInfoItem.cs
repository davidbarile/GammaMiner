using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITileInfoItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image tileThumbnail;
    [SerializeField] private TextMeshProUGUI tileSceneNameText;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;

    private bool isSelected;

    public MapTileData mapTileData;

    public void SetTileData(MapTileData inData)
    {
        this.mapTileData = inData;

        if (inData != null)
        {
            this.tileSceneNameText.text = inData.TileSceneName;
            this.tileThumbnail.sprite = inData.TileThumbnail;
        }
        else
        {
            this.tileSceneNameText.text = "Empty";
            this.tileThumbnail.sprite = null;
        }
    }

    public void HandleClick()
    {
        UIMapEditor.IN.SetSelectedTileInfoItem(this);
    }

    public void SetSelected(bool inIsSelected)
    {
        this.isSelected = inIsSelected;
        this.background.color = this.isSelected ? this.selectedColor : this.normalColor;
    }
}