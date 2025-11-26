using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UVFromSprite : MonoBehaviour
{
    public Vector2 U;
    public Vector2 V;

    private void OnValidate()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float minU = 1;
        float maxU = 0;
        float minV = 1;
        float maxV = 0;
        foreach (Vector2 uv in spriteRenderer.sprite.uv)
        {
            minU = Mathf.Min(uv.x, minU);
            maxU = Mathf.Max(uv.x, maxU);
            minV = Mathf.Min(uv.y, minV);
            maxV = Mathf.Max(uv.y, maxV);
        }

        U = new Vector2(minU, maxU);
        V = new Vector2(minV, maxV);

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(block);
        block.SetVector("U", new Vector2(minU, maxU));
        block.SetVector("V", new Vector2(minV, maxV));
        spriteRenderer.SetPropertyBlock(block);
    }
}