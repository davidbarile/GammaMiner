using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiTooltip : MonoBehaviour
{
    public enum TailDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Graphic background;
    [SerializeField] private Graphic[] tails;

    public void Show(string inText, Vector3 inPosition, TailDirection inTailDirection = TailDirection.Down)
    {
        this.tooltipText.text = inText;
        this.transform.position = inPosition;
        SetTailDirection(inTailDirection);
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void SetTailDirection(TailDirection inTailDirection)
    {
        for (int i = 0; i < this.tails.Length; i++)
        {
            this.tails[i].transform.parent.gameObject.SetActive(i == (int)inTailDirection);
        }
    }

    public void SetTextColor(Color inColor)
    {
        this.tooltipText.color = inColor;
    }

    public void SetBackgroundColor(Color inColor)
    {
        this.background.color = inColor;

        for (int i = 0; i < this.tails.Length; i++)
        {
            this.tails[i].color = inColor;
        }
    }
}