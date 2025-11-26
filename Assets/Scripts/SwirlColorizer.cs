using UnityEngine;

public class SwirlColorizer : MonoBehaviour
{
    [SerializeField] private bool doValidate;

    [SerializeField] private Color color = Color.white;

    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private void OnValidate()
    {
        if (!this.doValidate) return;

        //this.doValidate = false;

        if(this.spriteRenderers == null || this.spriteRenderers.Length == 0)
        {
            this.spriteRenderers = this.GetComponentsInChildren<SpriteRenderer>(true);
        }

        foreach (var sr in this.spriteRenderers)
        {
            sr.color = this.color;
        }
    }
}