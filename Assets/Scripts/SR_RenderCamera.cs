using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using Rocks;

public class SR_RenderCamera : MonoBehaviour
{
    public string ScreenshotPath { get; private set; }
    [SerializeField] private string path = "Assets/Captures/";
    [SerializeField] private int fileCounter = -1;
    [SerializeField] private Camera cam;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F9))
            CaptureImage();
    }

    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    public void CaptureImage()
    {
#if UNITY_EDITOR
        if (this.cam == null)
            this.cam = GetComponent<Camera>();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = this.cam.targetTexture;

        this.cam.Render();

        Texture2D tex2D = new Texture2D(this.cam.targetTexture.width, this.cam.targetTexture.height);
        tex2D.ReadPixels(new Rect(0, 0, this.cam.targetTexture.width, this.cam.targetTexture.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = currentRT;

        var bytes = tex2D.EncodeToPNG();
        DestroyImmediate(tex2D);

        var fileName = "Tile";

        var tile = Object.FindAnyObjectByType<Tile>();

        if (tile != null)
            fileName = tile.name;

        var path = $"{this.path}{fileName}.png";

        if (this.fileCounter >= 0)
        {
            path = $"{this.path}{fileName}_{fileCounter}.png";
            fileCounter++;
        }

        this.ScreenshotPath = path;

        File.WriteAllBytes(path, bytes);

        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}