using UnityEngine;
using DG.Tweening;

public class UiStatsDisplay : UiExpandableDisplay
{
    [SerializeField] private RectTransform content;
    [SerializeField, Range(0, 1)] private float scrollSpeed = 0.3f;

    [SerializeField] private float contentHeight = 444f;

    [SerializeField] private GameObject[] contentItems;

    private int currentIndex = 0;

    protected override void Start()
    {
        base.Start();
        
        foreach (var item in contentItems)
        {
            item.SetActive(true);
        }
    }

    public void ScrollToContentIndex(int inIndex)
    {
        if (this.content == null)
        {
            Debug.LogError("Content RectTransform is not assigned.");
            return;
        }

        var delta = Mathf.Abs(inIndex - this.currentIndex);
        var tweenDuration = Mathf.Min(delta * this.scrollSpeed, this.scrollSpeed * 2f);

        float inPosition = inIndex * this.contentHeight;

        ScrollToPosition(inPosition, tweenDuration);

        this.currentIndex = inIndex;
    }
    
    private void ScrollToPosition(float inPosition, float inDuration)
    {
        this.content.DOAnchorPosY(inPosition, inDuration)
            .SetEase(Ease.InOutSine);
            //.OnComplete(() => Debug.Log($"Scrolled to position: {inPosition}"));
    }
}