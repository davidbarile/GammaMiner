using UnityEngine;

public class AsteroidRenderer : MonoBehaviour
{
    [SerializeField] private AsteroidOrbit asteroidOrbit;
    [SerializeField] private SpriteDepthAdjuster spriteDepthAdjuster;
    [SerializeField] private SpriteRenderer asteroidOutlineSpriteRenderer;
    [SerializeField] private SpriteRenderer asteroidFillSpriteRenderer;
    [Space, SerializeField] private Sprite[] fillSprites;
    [SerializeField] private Sprite[] outlineSprites;

    public void SetRandomSprites(AsteroidRenderer inAsteroidRenderer)
    {
        //transfer sprites from the provided AsteroidRenderer to this instance to fix pooling issue
        this.fillSprites = inAsteroidRenderer.fillSprites;
        this.outlineSprites = inAsteroidRenderer.outlineSprites;

        int randomIndex = Random.Range(0, Mathf.Min(this.fillSprites.Length, this.outlineSprites.Length));
        SetSprites(randomIndex);
    }

    private void SetSprites(int inIndex)
    {
        if (inIndex < 0 || inIndex >= this.fillSprites.Length || inIndex >= this.outlineSprites.Length)
        {
            Debug.LogError($"Invalid index {inIndex} for asteroid sprites.");
            return;
        }

        this.asteroidOutlineSpriteRenderer.sprite = this.outlineSprites[inIndex];
        this.asteroidFillSpriteRenderer.sprite = this.fillSprites[inIndex];
    }

    public void SetColors(Color outlineColor, Color fillColor)
    {
        this.asteroidOutlineSpriteRenderer.color = outlineColor;
        this.asteroidFillSpriteRenderer.color = fillColor;
    }

    public void SetScale(float inScale)
    {
        this.asteroidOutlineSpriteRenderer.transform.localScale = new Vector3(inScale, inScale, inScale);
        this.spriteDepthAdjuster.SetInitScale(new Vector3(inScale, inScale, inScale));
    }

    public void SetSortingOrders(int inDepth)
    {
        this.spriteDepthAdjuster.SetInitialSortingOrders(inDepth);
    }

    public void SetOrbitRadius(float orbitRadius)
    {
        this.asteroidOrbit.SetOrbitRadius(orbitRadius);
    }

    public void SetOrbitRotationSpeed(float orbitRotationSpeed)
    {
        this.asteroidOrbit.SetOrbitSpeed(orbitRotationSpeed);
    }

    public void SetAsteroidRotationSpeed(float asteroidRotationSpeed)
    {
        this.asteroidOrbit.SetAsteroidRotation(asteroidRotationSpeed);
    }
}