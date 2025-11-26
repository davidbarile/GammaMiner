using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager IN;

    [SerializeField] private UiTooltip tooltip;

    private void Start()
    {
        HideTooltip();
    }

    public void ShowTooltip(string inText, Vector3 inPosition, UiTooltip.TailDirection inTailDirection = UiTooltip.TailDirection.Down)
    {
        this.tooltip.Show(inText, inPosition, inTailDirection);
    }

    public void HideTooltip()
    {
        this.tooltip.Hide();
    }

    public void SetTextColor(Color inColor)
    {
        this.tooltip.SetTextColor(inColor);
    }

    public void SetBackgroundColor(Color inColor)
    {
        this.tooltip.SetBackgroundColor(inColor);
    }
}