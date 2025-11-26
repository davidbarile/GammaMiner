using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LootData;

public class UILootCountCallout : MonoBehaviour
{
    private const string TRIGGER_SHOW = "tShow";
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image icon;
    [SerializeField] private Image iconOverlay;
    [SerializeField] private Animation showAnim;
    [SerializeField, Range(0, 5f)] float delayToDestroy = 1;

  public void Show(int inCount, SpriteRenderer inIcon, LootData inLootData, Vector3 inWorldPosition)
    {
        this.transform.SetParent(UI.IN.UiElementsContainer);
        this.transform.localScale = Vector3.one;
        this.transform.position = Camera.main.WorldToScreenPoint(inWorldPosition);

        this.countText.text = $"{inCount}";

        this.icon.sprite = inIcon.sprite;
        this.icon.color = inIcon.color;

        var isCrystals = inLootData.LootType == ELootType.Crystals;
        this.iconOverlay.gameObject.SetActive(isCrystals);

        //TODO: add effects, etc based on LootType
        //switch (inLootData.LootType)
        //{
        //    case LootData.ELootType.Health:

        //        break;
        //    default:
        //        break;
        //}
    
        this.showAnim.Rewind();
        this.showAnim.Play();

        Invoke(nameof(DelayedDestroy), this.delayToDestroy);
    }

    private void DelayedDestroy()
    {
        this.showAnim.Stop();
        this.showAnim.Rewind();
        Pool.Despawn(this.gameObject);
    }
}