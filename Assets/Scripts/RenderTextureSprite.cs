using UnityEngine;

/// <summary>
/// This works, but KILLS the framerate.  Do not use.
/// Instead use a mesh with a SortingGroup added to it
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RenderTextureSprite : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;

    private SpriteRenderer spriteRenderer;
    private Vector2 spriteRendererSize;

    private void Start()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.spriteRendererSize = this.spriteRenderer.size;
    }

    private void Update()
    {
        //var tex2D = this.renderTexture.toTexture2D();
        var tex2D = ToTexture2D(this.renderTexture);

        var sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect);
        sprite.name = "Kaleid Render Texture";
        spriteRenderer.sprite = sprite;

        this.spriteRenderer.size = this.spriteRendererSize;
    }

    public Texture2D ToTexture2D(RenderTexture rTex)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;

        Texture2D tex = new Texture2D(rTex.width, rTex.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentActiveRT;
        return tex;
    }
}