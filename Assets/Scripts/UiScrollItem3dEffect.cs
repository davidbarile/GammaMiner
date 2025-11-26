using UnityEngine;
using UnityEngine.UI;

public class UiScrollItem3dEffect : MonoBehaviour
{

    [SerializeField] private AnimationCurve scaleCurve;

    private LayoutElement parentLayoutElement;
    private float minWidth;

    private void Start()
    {
        this.parentLayoutElement = this.transform.parent.GetComponent<LayoutElement>();
        this.minWidth = this.parentLayoutElement.minWidth;
    }

    private void Update()
    {
        if (this.parentLayoutElement == null) return;

        float xPos = this.transform.position.x / (float)Screen.width;
        xPos = Mathf.Clamp01(xPos); // Ensure xPos is between 0 and 1

        float scaleValue = this.scaleCurve.Evaluate(xPos);
        this.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        this.parentLayoutElement.minWidth = this.minWidth * scaleValue;
    }
}