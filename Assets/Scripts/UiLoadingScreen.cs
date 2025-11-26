using System.Collections;
using TMPro;
using UnityEngine;

public class UiLoadingScreen : UIPanelBase
{
    public static UiLoadingScreen IN;

    [SerializeField] private TextMeshProUGUI loadingText, loadedTilesText;
    [SerializeField] private float minLoadingScreenDispayTime = 2f;
    private float timeOfShowLoadingScreen = 0f;

    public void Show(string inText)
    {
        if(!GameManager.IN.ShouldShowLoadingScreen) return;
        
        Show();
        this.loadingText.text = inText;
    }

    public override void Show()
    {
        base.Show();

        this.timeOfShowLoadingScreen = Time.time;
    }

    public void DelayedHide()
    {
        StartCoroutine(DelayedHideCo());
    }

    public override void Hide()
    {
        SetLoadedTilesText(string.Empty);
        
        base.Hide();
    }

    private IEnumerator DelayedHideCo()
    {
        while (Time.time - this.timeOfShowLoadingScreen < this.minLoadingScreenDispayTime)
            yield return null;

        Hide();
    }

    public void SetLoadedTilesText(string inText)
    {
        this.loadedTilesText.text = inText;
    }
}