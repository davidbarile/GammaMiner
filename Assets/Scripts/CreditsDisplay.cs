using System.Collections;
using TMPro;
using UnityEngine;

public class CreditsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private IEnumerator Start()
    {
        while (PlayerData.Data == null)
        {
            yield return null;
        }
        
        PlayerData.OnCreditsChanged += SetCreditsText;
        SetCreditsText(PlayerData.Data.Credits);
    }

    private void OnDestroy()
    {
        PlayerData.OnCreditsChanged -= SetCreditsText;
    }

    private void SetCreditsText(int inAmount)
    {
        this.text.text = $"{inAmount:N0}";
    }
}