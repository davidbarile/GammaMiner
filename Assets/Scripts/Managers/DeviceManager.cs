using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    public enum EDeviceType
    {
        Actual,
        Mobile,
        Desktop,
        All
    }

    public static DeviceManager IN;

    [SerializeField] private EDeviceType deviceOverride;

    public static EDeviceType CurrentDeviceType
    {
        get
        {
            if (Application.isEditor && IN != null && IN.deviceOverride != EDeviceType.Actual)
            {
                return IN.deviceOverride;
            }
            else if (Application.isMobilePlatform)
            {
                return EDeviceType.Mobile;
            }
            else
            {
                return EDeviceType.Desktop;
            }
        }
    }

    public static bool IsMobileDevice
    {
        get
        {
            return CurrentDeviceType == EDeviceType.Mobile || CurrentDeviceType == EDeviceType.All;
        }
    }

    public static bool IsDesktopDevice
    {
        get
        {
            return CurrentDeviceType == EDeviceType.Desktop || CurrentDeviceType == EDeviceType.All;
        }
    }
}