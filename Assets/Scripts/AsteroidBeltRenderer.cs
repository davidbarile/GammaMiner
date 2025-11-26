using System.Collections.Generic;
using UnityEngine;

public class AsteroidBeltRenderer : MonoBehaviour
{
    [SerializeField] private AsteroidRenderer asteroidPrefab;
    [SerializeField] private Vector2 numberOfAsteroidsRange = Vector2.one;
    [SerializeField] private Vector2 asteroidSizeRange = Vector2.one;
    [SerializeField] private Vector2 yOffsetRange = Vector2.zero;
    [Space, SerializeField] private Vector2 orbitRadiusRange = new(5f, 10f);
    [Header("Leave -1 and set on transform for static angle, or set range for random orbit angle")]
    [SerializeField] private Vector2 orbitAngleRange = new(-1f, -1f);

    [Space, SerializeField] private Vector2 orbitRotationSpeedRange = Vector2.zero;
    [SerializeField] private Vector2 asteroidRotationSpeedRange = Vector2.zero;

    [Header("Asteroid Colors")]
    [SerializeField] private Color asteroidOutlineColor1 = Color.black;
    [SerializeField] private Color asteroidOutlineColor2 = Color.black;
    [SerializeField] private Color asteroidFillColor1 = Color.white;
    [SerializeField] private Color asteroidFillColor2 = Color.white;

    [Header("Moon Colors")]
    [SerializeField] private Color moonOutlineColor = Color.black;
    [SerializeField] private Color moonFillColor = Color.white;

    private List<AsteroidRenderer> asteroids = new();

    [Header("(Do not modify)")]
    public AsteroidBeltData AsteroidBeltData = null;

    private void Start()
    {
        if (this.asteroidPrefab.gameObject.activeInHierarchy)
            this.asteroidPrefab.gameObject.SetActive(false);

        if (this.orbitAngleRange.x == -1 && this.orbitAngleRange.y == -1)
        {
            // If both angles are -1, set a static angle from the transform
            this.orbitAngleRange = new Vector2(this.transform.localRotation.eulerAngles.z, this.transform.localRotation.eulerAngles.z);
        }

        ConfigureFromData(new AsteroidBeltData
        {
            IsVisible = true,
            IsMoon = this.transform.parent != null && this.transform.parent.name.Contains("Moons"),
            NumberOfAsteroidsRange = this.numberOfAsteroidsRange,
            AsteroidSizeRange = this.asteroidSizeRange,
            YOffsetRange = this.yOffsetRange,
            OrbitRadiusRange = this.orbitRadiusRange,
            OrbitAngleRange = this.orbitAngleRange,
            OrbitRotationSpeedRange = this.orbitRotationSpeedRange,
            AsteroidRotationSpeedRange = this.asteroidRotationSpeedRange,
            AsteroidOutlineColor1 = this.asteroidOutlineColor1,
            AsteroidOutlineColor2 = this.asteroidOutlineColor2,
            AsteroidFillColor1 = this.asteroidFillColor1,
            AsteroidFillColor2 = this.asteroidFillColor2,
            MoonOutlineColor = this.moonOutlineColor,
            MoonFillColor = this.moonFillColor
        });

        ColorFromColorConfigs(new AsteroidColorsConfig
        {
            Outline1Colors = new List<Color> { this.asteroidOutlineColor1 },
            Outline2Colors = new List<Color> { this.asteroidOutlineColor2 },
            Fill1Colors = new List<Color> { this.asteroidFillColor1 },
            Fill2Colors = new List<Color> { this.asteroidFillColor2 }
        });
    }

    public void ConfigureFromData(AsteroidBeltData inData, int inDepthSortingIndex = 6)
    {
        this.AsteroidBeltData = inData;

        ClearAsteroids();

        if (!inData.IsVisible)
        {
            this.gameObject.SetActive(false);
            return;
        }

        var numAsteroidsInBelt = Random.Range(inData.NumberOfAsteroidsRange.x, inData.NumberOfAsteroidsRange.y);
        numAsteroidsInBelt = Mathf.RoundToInt(numAsteroidsInBelt);

        float orbitAngle = Random.Range(inData.OrbitAngleRange.x, inData.OrbitAngleRange.y);

        this.transform.localRotation = Quaternion.Euler(0, 0, orbitAngle);

        var type = inData.IsMoon ? "Moon" : "Asteroid";

        for (var i = 0; i < numAsteroidsInBelt; i++)
        {
            var asteroid = SpawnAsteroidPrefab(this.transform);
            asteroid.gameObject.SetActive(true);
            asteroid.name = $"{type}Orbit_{i}";
            var yOffset = Random.Range(inData.YOffsetRange.x, inData.YOffsetRange.y);
            asteroid.transform.localPosition = new Vector3(0, yOffset, 0);
            asteroid.transform.localScale = Vector3.one;

            var maxSpacingOffset = 360 / (float)numAsteroidsInBelt * .5f;
            var orbitSpacing = 360f / numAsteroidsInBelt * i + Random.Range(0f, maxSpacingOffset);
            asteroid.transform.localRotation = Quaternion.Euler(0, orbitSpacing, 0);

            asteroid.SetRandomSprites(this.asteroidPrefab);

            var initSortingOrder = inDepthSortingIndex + (i * 2);
            asteroid.SetSortingOrders(initSortingOrder); // Set sorting order based on index

            float scale = Random.Range(inData.AsteroidSizeRange.x, inData.AsteroidSizeRange.y);
            asteroid.SetScale(scale);

            var orbitRotationSpeed = Random.Range(inData.OrbitRotationSpeedRange.x, inData.OrbitRotationSpeedRange.y);
            asteroid.SetOrbitRotationSpeed(orbitRotationSpeed);

            var asteroidRotationSpeed = Random.Range(inData.AsteroidRotationSpeedRange.x, inData.AsteroidRotationSpeedRange.y);
            asteroid.SetAsteroidRotationSpeed(asteroidRotationSpeed);

            var orbitRadius = Random.Range(inData.OrbitRadiusRange.x, inData.OrbitRadiusRange.y);
            asteroid.SetOrbitRadius(orbitRadius);

            this.asteroids.Add(asteroid);
        }
    }

    public void ColorFromColorConfigs(AsteroidColorsConfig inAsteroidColorsConfig, int inBeltRendererIndex = 0)
    {
        for (int i = 0; i < this.asteroids.Count; i++)
        {
            var asteroid = this.asteroids[i];

            var inOutline1Colors = inAsteroidColorsConfig.Outline1Colors[inBeltRendererIndex];
            var inOutline2Colors = inAsteroidColorsConfig.Outline2Colors[inBeltRendererIndex];
            var inFill1Colors = inAsteroidColorsConfig.Fill1Colors[inBeltRendererIndex];
            var inFill2Colors = inAsteroidColorsConfig.Fill2Colors[inBeltRendererIndex];

            var rnd = Random.Range(0f, 1f);
            var outlineColor = Color.Lerp(inOutline1Colors, inOutline2Colors, rnd);
            var fillColor = Color.Lerp(inFill1Colors, inFill2Colors, rnd);

            asteroid.SetColors(outlineColor, fillColor);
        }
    }

    public void ColorMoonFromColorConfigs(MoonColorsConfig inMoonColorsConfig)
    {
        for (int i = 0; i < this.asteroids.Count; i++)
        {
            var moon = this.asteroids[i];

            var rnd = Random.Range(0f, 1f);

            var outlineColor = Color.Lerp(inMoonColorsConfig.OutlineColor1, inMoonColorsConfig.OutlineColor2, rnd);
            var fillColor = Color.Lerp(inMoonColorsConfig.FillColor1, inMoonColorsConfig.FillColor2, rnd);

            moon.SetColors(outlineColor, fillColor);
        }
    }

    private AsteroidRenderer SpawnAsteroidPrefab(Transform inParent)
    {
        if (Pool.IN == null)
            return Instantiate<AsteroidRenderer>(this.asteroidPrefab, inParent);
        else
            return Pool.Spawn<AsteroidRenderer>(this.asteroidPrefab.name, inParent, Vector3.zero, Quaternion.identity);
    }

    public void ClearAsteroids()
    {
        foreach (var asteroid in this.asteroids)
        {
            Pool.Despawn(asteroid.gameObject);
        }
        this.asteroids.Clear();
    }
}