using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector3 tooltipOffset = new(0, 200, 0);

    public string TooltipText;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(this.TooltipText))
            return;
            
        TooltipManager.IN.ShowTooltip(this.TooltipText, this.transform.position + tooltipOffset, UiTooltip.TailDirection.Down);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.IN.HideTooltip();
    }

    private void OnDisable()
    {
        TooltipManager.IN.HideTooltip();
    }
}