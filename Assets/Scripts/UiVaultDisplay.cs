using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UiVaultDisplay : ShipComponentDisplayBase
{
    public Action OnVaultIndexChanged;

    [SerializeField] private bool isScrollable;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private RectTransform content;
    [SerializeField, Range(0, 1)] private float scrollSpeed = 0.3f;

    [Space, SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private float contentWidth = 300f;
    [SerializeField] private UiVaultItem[] vaultItems;

    public int CurrentIndex => this.currentIndex;
    private int currentIndex = 0;
    
    protected override IEnumerator InitCo()
    {
        while (PlayerData.Data == null || PlayerData.Data.ShipData == null)
        {
            yield return null; // Wait until PlayerData and ShipData are initialized
        }

        HUD.OnCrystalsChanged += OnCrystalsChanged;
        PlayerData.OnShipDataChanged += OnShipDataChanged;

        OnCrystalsChanged(PlayerData.Data.ShipData);

        if(this.isScrollable)
            UpdateButtonInteractivity();

        this.isInitialized = true;
        
        yield break;
    }

    public void HandlePrevButtonClick()
    {
        ScrollToContentIndex(this.currentIndex - 1);
    }

    public void HandleNextButtonClick()
    {
        ScrollToContentIndex(this.currentIndex + 1);
    }

    private void UpdateButtonInteractivity()
    {
        this.prevButton.interactable = this.currentIndex > 0;
        this.nextButton.interactable = this.currentIndex < this.vaultItems.Length - 1 && this.vaultItems[this.currentIndex + 1].gameObject.activeSelf;
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

        float inPosition = -inIndex * (this.contentWidth + this.layoutGroup.spacing);

        ScrollToPosition(inPosition, tweenDuration);

        this.currentIndex = inIndex;

        this.OnVaultIndexChanged?.Invoke();

        UpdateButtonInteractivity();
    }

    private void ScrollToPosition(float inPosition, float inDuration)
    {
        if(!this.gameObject.activeInHierarchy)
        {
            // If the GameObject is not active, set position directly without animation
            this.content.anchoredPosition = new Vector2(inPosition, this.content.anchoredPosition.y);
            return;
        }
        
        this.content.DOAnchorPosX(inPosition, inDuration).SetEase(Ease.InOutSine).SetUpdate(true);
        //.OnComplete(() => Debug.Log($"Scrolled to position: {inPosition}"));
    }

    protected override void OnShipDataChanged()
    {
        base.OnShipDataChanged();

        this.currentIndex = 0;

        if(this.isScrollable)
            ScrollToContentIndex(this.currentIndex);
    }

    protected override void OnCrystalsChanged(ShipData inShipData)
    {
        //Debug.Log($"RefreshVaultDisplays() - Vault Count: {inShipData.VaultDatas.Count}");
        for (int i = 0; i < this.vaultItems.Length; i++)
        {
            var vaultItem = this.vaultItems[i];
            var shouldShow = i < inShipData.VaultDatas.Count;
            vaultItem.gameObject.SetActive(shouldShow);

            if (shouldShow)
            {
                var vaultData = inShipData.VaultDatas[i];
                vaultItem.Configure(vaultData);
            }
        }
    }
}