using System.Collections;
using UnityEngine;

public class ShipComponentDisplayBase : MonoBehaviour
{
    protected bool isInitialized;

    protected virtual void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (this.isInitialized)
            return;

        StartCoroutine(InitCo());
    }

    protected virtual IEnumerator InitCo()
    {
        // while (PlayerData.Data == null || PlayerData.Data.ShipData == null)
        // {
        //     yield return null;
        // }

        // this.isInitialized = true;

        yield break;
    }

    protected virtual void OnDestroy()
    {
        HUD.OnCrystalsChanged -= OnCrystalsChanged;
        PlayerData.OnShipDataChanged -= OnShipDataChanged;
    }

    protected virtual void OnShipDataChanged()
    {
        OnCrystalsChanged(PlayerData.Data.ShipData);
    }
    
    protected virtual void OnCrystalsChanged(ShipData inShipData)
    {
        // To be overridden in derived classes
    }
}