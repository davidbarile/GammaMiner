using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager IN;

    [SerializeField] private ProgressConfig[] progressConfigs;

    public ProgressData[] ProgressDatas { get; private set; }

    private void Awake()
    {
        this.ProgressDatas = new ProgressData[this.progressConfigs.Length];
        for (int i = 0; i < this.progressConfigs.Length; i++)
        {
            this.ProgressDatas[i] = this.progressConfigs[i].ProgressData;
        }
    }
}