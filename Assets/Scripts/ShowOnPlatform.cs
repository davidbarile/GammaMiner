using UnityEngine;
using static DeviceManager;

public class ShowOnPlatform : MonoBehaviour
{
    [SerializeField] private EDeviceType showOnDevice;

    private void Start()
    {
        if (showOnDevice == EDeviceType.All)
        {
            gameObject.SetActive(true);
        }
        else if (showOnDevice == EDeviceType.Mobile && !DeviceManager.IsMobileDevice)
        {
            gameObject.SetActive(false);
        }
        else if (showOnDevice == EDeviceType.Desktop && DeviceManager.IsMobileDevice)
        {
            gameObject.SetActive(false);
        }
    }
}