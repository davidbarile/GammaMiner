using TMPro;
using UnityEngine;

public class UiSplashScreen : UIPanelBase
{
    public static UiSplashScreen IN;

    [SerializeField] private GameObject playButton;

    [SerializeField] private TextMeshProUGUI loadingText;

    public void SetPlayButtonVisibility(bool inShouldShow)
    {
        this.playButton.SetActive(inShouldShow);
    }

    public void SetLoadingText(string inText)
    {
        this.loadingText.text = inText;
    }
}