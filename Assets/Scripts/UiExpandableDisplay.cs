using UnityEngine;

public abstract class UiExpandableDisplay : MonoBehaviour
{
    [Space, SerializeField] private GameObject fullScreenButton;
    [SerializeField] private GameObject minimizeButton;

    [Space, SerializeField] private RectTransform monitorBase;
    [Space, SerializeField, Range(0f, 3f)] private float upscaleFactor = 2f;

    protected virtual void Start()
    {
        this.fullScreenButton.SetActive(true);
        this.minimizeButton.SetActive(false);
    }

    public void HandleFullScreenButtonClick()
    {
        HUD.IN.FullScreenViewer.ShowFullScreen(this.monitorBase, this.upscaleFactor);
        this.fullScreenButton.SetActive(false);
        this.minimizeButton.SetActive(true);
    }
    
    public void HandleMinimizeButtonClick()
    {
        HUD.IN.FullScreenViewer.HideFullScreen();
        this.fullScreenButton.SetActive(true);
        this.minimizeButton.SetActive(false);
    }
}