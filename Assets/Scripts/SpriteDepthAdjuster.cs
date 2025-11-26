using System.Collections.Generic;
using UnityEngine;

public class SpriteDepthAdjuster : MonoBehaviour
{
    public static int SortingOrderIndex = 0;
    [SerializeField] private Transform asteroidTransform;

    [SerializeField] private float depthMultiplier = 1f;
    [SerializeField] private int minDepth = -200;
    [Space, SerializeField] private float sizeMultiplier = .005f;

    [SerializeField] private SpriteRenderer[] spriteRenderers;
    private List<int> initialSortingOrders = new();
    private Vector3 initialScale = Vector3.one;

    public void SetInitScale(Vector3 inScale)
    {
        this.initialScale = inScale;
        this.asteroidTransform.localScale = inScale;
    }

    public void SetInitialSortingOrders(int inDepth)
    {
        this.initialSortingOrders.Clear();
        for (var i = 0; i < this.spriteRenderers.Length; i++)
        {
            // Set the initial sorting order based on the provided base depth
            this.spriteRenderers[i].sortingOrder = inDepth + i;
            this.initialSortingOrders.Add(this.spriteRenderers[i].sortingOrder);
        }
    }

    private void Update()
    {
        var depth = 0;

        for (int i = 0; i < this.spriteRenderers.Length; i++)
        {
            if (i >= this.initialSortingOrders.Count)
            {
                Debug.Log($"[{name}].SpriteRenderer index {i} exceeds initialSortingOrders count {this.initialSortingOrders.Count}");
                continue;
            }

            // Adjust the sorting order based on the z position of the GameObject
            var relativeZ = this.transform.parent.position.z - this.asteroidTransform.position.z;
            var baseDepth = relativeZ > 0 ? this.minDepth : this.initialSortingOrders[i];
            int newSortingOrder = baseDepth + Mathf.RoundToInt(-1 * relativeZ * this.depthMultiplier);
            spriteRenderers[i].sortingOrder = this.initialSortingOrders[i] + newSortingOrder;
            depth = newSortingOrder;

            var sortingLayerName = relativeZ > 0 ? "Planets" : "PlanetRings";
            spriteRenderers[i].sortingLayerName = sortingLayerName;
        }

        var relativeZ2 = this.transform.parent.parent.position.z - this.asteroidTransform.position.z;

        this.asteroidTransform.localScale = this.initialScale + (this.sizeMultiplier * -relativeZ2 * Vector3.one);

        //Debug.Log($"Depth: {depth},   Position Z: {relativeZ2},   Scale: {this.transform.localScale.x}. count {this.initialSortingOrders.Count}");
    }
}