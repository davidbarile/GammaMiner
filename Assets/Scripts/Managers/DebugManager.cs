using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public static DebugManager IN;
    [SerializeField] private TextMeshProUGUI debugText;

    private void Start()
    {
        ClearLog();
    }

    public void Log(string inMessage, float inDelay = 0f)
    {
        this.debugText.text += $"{inMessage}\n";

        if(inDelay > 0f)
        {
            Invoke(nameof(ClearLog), inDelay);
        }
    }

    public void ClearLog()
    {
        this.debugText.text = string.Empty;
    }
}