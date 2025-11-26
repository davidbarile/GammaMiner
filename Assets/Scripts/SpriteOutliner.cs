using System.Collections.Generic;
using UnityEngine;

public class SpriteOutliner : MonoBehaviour
{
    [SerializeField] private bool clear;
    [SerializeField] private bool includeHidden = false;
    [Space, SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0,10)] private float outlineWidth = 5f;
    
    [SerializeField] private bool sampleCorners;
    [SerializeField] private Material outlineMaterial;
    [SerializeField, Range(-20, 10)] private int renderDepth = -1;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [SerializeField] private List<SpriteRenderer> outlineSpriteRenderers;

    private void OnValidate()
    {
        if (this.clear)
        {
            this.clear = false;
            foreach (var outlineRenderer in this.outlineSpriteRenderers)
            {
                if (outlineRenderer != null)
                {
                    DestroyImmediate(outlineRenderer.gameObject);
                }
            }

            this.spriteRenderers = new SpriteRenderer[0];
            this.outlineSpriteRenderers.Clear();

            return;
        }

        if (this.spriteRenderers.Length == 0)
        {
            this.spriteRenderers = GetComponentsInChildren<SpriteRenderer>(this.includeHidden);
            CreateSpriteOutline();
        }

        foreach (var outlineSr in this.outlineSpriteRenderers)
        {
            if (outlineSr != null)
            {
                outlineSr.sharedMaterial.SetColor("_OutlineColor", this.outlineColor);
                outlineSr.sharedMaterial.SetFloat("_OutlineThickness", this.outlineWidth);
                outlineSr.sharedMaterial.SetFloat("CORNERS_ON", this.sampleCorners ? 1f : 0f);
                outlineSr.sortingOrder = this.renderDepth;
            }
        }
    }

    private void Start()
    {
        CreateSpriteOutline();
    }

    private void CreateSpriteOutline()
    {
        if (this.outlineSpriteRenderers.Count > 0) return;

        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                var go = new GameObject($"{spriteRenderer.name}_outline");
                go.transform.SetParent(spriteRenderer.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = spriteRenderer.transform.localScale;

                var outlineSr = go.AddComponent<SpriteRenderer>();
                outlineSr.sprite = spriteRenderer.sprite;
                outlineSr.material = this.outlineMaterial;
                outlineSr.sortingLayerID = spriteRenderer.sortingLayerID;
                outlineSr.flipX = spriteRenderer.flipX;
                outlineSr.flipY = spriteRenderer.flipY;

                this.outlineSpriteRenderers.Add(outlineSr);
            }
        }
    }
}