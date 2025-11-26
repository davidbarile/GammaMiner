using UnityEngine;

public class UiFullScreenViewer : MonoBehaviour
{
    [SerializeField] private GameObject fullScreenParent;

    private RectTransform enlargedObject;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector2 originalDimensions;
    private int originalSiblingIndex;

    private Vector2 halfVector = new Vector2(0.5f, 0.5f);

    public void ShowFullScreen(RectTransform inObjectToEnlarge, float inScale = 1f)
    {
        if (this.enlargedObject != null)
            HideFullScreen();

        this.originalParent = inObjectToEnlarge.parent;
        this.originalSiblingIndex = inObjectToEnlarge.GetSiblingIndex();
        this.originalPosition = inObjectToEnlarge.anchoredPosition;
        this.originalAnchorMin = inObjectToEnlarge.anchorMin;
        this.originalAnchorMax = inObjectToEnlarge.anchorMax;
        this.originalPivot = inObjectToEnlarge.pivot;
        this.originalDimensions = new Vector2(inObjectToEnlarge.rect.width, inObjectToEnlarge.rect.height);

        inObjectToEnlarge.anchorMin = this.halfVector;
        inObjectToEnlarge.anchorMax = this.halfVector;
        inObjectToEnlarge.pivot = this.halfVector;
        inObjectToEnlarge.anchoredPosition = Vector2.zero;

        this.fullScreenParent.transform.localScale = inScale * Vector3.one;

        inObjectToEnlarge.SetParent(this.fullScreenParent.transform, false);
        inObjectToEnlarge.localPosition = Vector3.zero;
        inObjectToEnlarge.localScale = Vector3.one;

        inObjectToEnlarge.sizeDelta = this.originalDimensions;

        this.enlargedObject = inObjectToEnlarge;

        this.gameObject.SetActive(true);
    }

    public void HideFullScreen()
    {
        if (this.enlargedObject != null)
        {
            this.enlargedObject.SetParent(this.originalParent, false);
            this.enlargedObject.SetSiblingIndex(this.originalSiblingIndex);
            this.enlargedObject.localScale = Vector3.one;
            this.enlargedObject.localPosition = Vector3.zero;
            this.enlargedObject.anchoredPosition = this.originalPosition;
            this.enlargedObject.anchorMin = this.originalAnchorMin;
            this.enlargedObject.anchorMax = this.originalAnchorMax;
            this.enlargedObject.pivot = this.originalPivot;
            this.enlargedObject.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.originalDimensions.x);
            this.enlargedObject.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.originalDimensions.y);
            this.enlargedObject = null;
        }

        this.gameObject.SetActive(false);
    }
}
