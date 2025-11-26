using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UiSettingsPanel : UIPanelBase
{
    public static UiSettingsPanel IN;

    [SerializeField] private TextMeshProUGUI headerText;

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;

    private int tapCounter = 0;

    public override void Show()
    {
        GameManager.PauseGame(true);

        base.Show();
        this.tapCounter = 0;

        this.headerText.text = $"Settings";

        this.musicVolumeSlider.value = PlayerData.Data.MusicVolume;
        this.effectsVolumeSlider.value = PlayerData.Data.EffectsVolume;
    }

    public override void Hide()
    {
        GameManager.PauseGame(false);
        
        base.Hide();
        UiDebugPanel.IN.Hide();
    }

    public void HandleMusicVolumeSliderChanged(Single inValue)
    {
        AudioManager.IN.SetMusicVolume(this.musicVolumeSlider.value);
    }

    public void HandleEffectsVolumeSliderChanged(Single inValue)
    {
        AudioManager.IN.SetEffectsVolume(this.effectsVolumeSlider.value);
    }

    public void HandleSecretButtonPress()
    {
        ++this.tapCounter;

        var tapsRequired = Application.isEditor ? 1 : 7;

        if (this.tapCounter >= tapsRequired)
        {
            this.tapCounter = 0;
            this.headerText.text = $"Debug  (v{Application.version})";
            UiDebugPanel.IN.Show();
        }
    }

    public void HandleClearPlayerDataButtonPress()
    {
        UIConfirmPanel.IN.Show("Delete Data?", $"All data will be deleted and game will close.\nAre you sure?", () =>
        {
            GameManager.IN.DeletePlayerPrefs();
            GameManager.QuitGame();
        });
    }
}