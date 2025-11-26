using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager IN;

    [SerializeField] private bool generatePlanetsOnStart;
    [Range(-1, 100000f)] public int RandomSeed = -1;

    [SerializeField] private RandomSeedsConfig randomSeedsConfig;

    [Space, SerializeField] private Planet planetPrefab;
    [SerializeField] private Transform planetsParent;

    private Planet spawnedPlanet;

    private string[] planetConfigNames = new string[]
    {
        "Asteroid",
        "Moon",
        "Planet",
        "Ring",
        "PlanetColor",
        "RingColor",
        "AsteroidColor",
        "MoonColor"
    };

    [Header("Planet Configurations to pick from Randomly (Automatically Loaded from Resources)")]
    [SerializeField] private PlanetConfig[] planetConfigs;
    [SerializeField] private PlanetRingsConfig[] ringConfigs;
    [SerializeField] private AsteroidsConfig[] asteroidsConfigs;
    [SerializeField] private AsteroidsConfig[] moonConfigs;
    [SerializeField] private PlanetColorsConfig[] planetColorConfigs;
    [SerializeField] private PlanetColorsConfig[] ringColorConfigs;
    [SerializeField] private AsteroidColorsConfig[] asteroidsColorConfigs;
    [SerializeField] private MoonColorsConfig[] moonColorConfigs;

    public PlanetConfig[] PlanetConfigs => this.planetConfigs;
    public PlanetRingsConfig[] RingConfigs => this.ringConfigs;
    public AsteroidsConfig[] AsteroidsConfigs => this.asteroidsConfigs;
    public AsteroidsConfig[] MoonConfigs => this.moonConfigs;

    public PlanetColorsConfig[] PlanetColorConfigs => this.planetColorConfigs;
    public PlanetColorsConfig[] RingColorConfigs => this.ringColorConfigs;
    public AsteroidColorsConfig[] AsteroidsColorConfigs => this.asteroidsColorConfigs;
    public MoonColorsConfig[] MoonColorConfigs => this.moonColorConfigs;

    [Header("Planet Data")]
    [SerializeField] private Transform planetParent;
    [SerializeField] private PlanetData planetData;

    private List<Planet> spawnedPlanets = new();

    private static readonly int[] prefixLengths = { 1, 2, 2, 2, 3, 3 };
    private static readonly int[] postfixLengths = { 1, 2, 2, 2, 3, 3, 3, 4, 4, 5 };

    private static readonly char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] numbers = "0123456789".ToCharArray();
    //private static readonly char[] special = "!@#$%^&*()_+-=[]{}|;:,.<>?".ToCharArray();
    //private static readonly char[] allChars = alpha.Concat(numbers).Concat(special).ToArray();
    private static StringBuilder stringBuilder = new();

    public static string GetRandomSeedString()
    {
        stringBuilder.Clear();

        int rndLettersLength = Random.Range(1, prefixLengths.Length);

        int numLetterChars = prefixLengths[rndLettersLength];

        for (int i = 0; i < numLetterChars; i++)
        {
            stringBuilder.Append(alpha[Random.Range(1, alpha.Length)]);
        }

        stringBuilder.Append("-");

        int rndNumbersLength = Random.Range(1, postfixLengths.Length);

        int numNumeralChars = postfixLengths[rndNumbersLength];

        if (numLetterChars == 3)
            numNumeralChars = Mathf.Min(numNumeralChars, 2);
        else if (numLetterChars == 1)
            numNumeralChars = Mathf.Max(numNumeralChars, 2);
        else
            numNumeralChars = Mathf.Min(numNumeralChars, 4);

        for (int i = 0; i < numNumeralChars; i++)
        {
            stringBuilder.Append(numbers[Random.Range(1, numbers.Length)]);
        }

        return stringBuilder.ToString();
    }

    public void AddRandomSeedToFavorites(string inSeedName)
    {
        this.randomSeedsConfig.SeedNames.Add(inSeedName);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this.randomSeedsConfig);
#endif
    }

    private void Awake()
    {
        if (IN == null)
        {
            IN = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        var gm = TryGetComponent<GameManager>(out var gameManager) ? gameManager : null;

        if (gm == null)
        {
            Application.targetFrameRate = 60;
            Init();//when in Planet Scene, GameManager is not present
        }
    }

    public void Init()
    {
        // for(int i = 0; i < 100; i++)
        // {
        //     var randomSeedString = GetRandomSeedString();
        //     Debug.Log($"{randomSeedString}");
        // }

        if (this.RandomSeed == -1)
            this.RandomSeed = UnityEngine.Random.Range(0, 100000);

        Random.InitState(this.RandomSeed);//string.GetHashCode();

        LoadConfigs();

        if (this.generatePlanetsOnStart)
            SpawnRandomPlanets();
    }

    [Button("Spawn Random Planets")]
    private void SpawnRandomPlanets()
    {
        DestroyRandomPlanets();

        var numAcross = 5;
        var numDown = 5;
        var spacer = 25f;

        var counter = 0;

        for (var i = 0; i < numAcross; i++)
        {
            for (var j = 0; j < numDown; j++)
            {
                var planet = SpawnPlanetPrefab(this.planetsParent);
                planet.transform.localPosition = new Vector3(i * spacer - (numAcross * spacer) / 2, j * spacer - (numDown * spacer) / 2, 0f);

                planet.name = $"Planet_({i},{j}_{counter})";

                planet.GenerateRandomPlanet();
                planet.SetSpriteRenderDepths();

                ++counter;

                this.spawnedPlanets.Add(planet);
            }
        }
    }

    [Button("Destroy Random Planets")]
    private void DestroyRandomPlanets()
    {
        foreach (var planet in this.spawnedPlanets)
        {
            ReturnPlanetAndChildrenToPool(planet);
        }

        this.spawnedPlanets.Clear();
    }

    private void LoadConfigs()
    {
        foreach (var configName in this.planetConfigNames)
        {
            if (File.Exists($"PlanetConfigs/{configName}"))
            {
                Debug.Log($"Folder {configName} does not exist, skipping.");
                continue;
            }

            var subfolder = $"PlanetConfigs/{configName}";

            switch (configName)
            {
                case "Asteroid":
                    this.asteroidsConfigs = Resources.LoadAll(subfolder, typeof(AsteroidsConfig)).Cast<AsteroidsConfig>().ToArray();
                    break;
                case "Moon":
                    this.moonConfigs = Resources.LoadAll(subfolder, typeof(AsteroidsConfig)).Cast<AsteroidsConfig>().ToArray();
                    break;
                case "Planet":
                    this.planetConfigs = Resources.LoadAll(subfolder, typeof(PlanetConfig)).Cast<PlanetConfig>().ToArray();
                    break;
                case "Ring":
                    this.ringConfigs = Resources.LoadAll(subfolder, typeof(PlanetRingsConfig)).Cast<PlanetRingsConfig>().ToArray();
                    break;
                case "PlanetColor":
                    this.planetColorConfigs = Resources.LoadAll(subfolder, typeof(PlanetColorsConfig)).Cast<PlanetColorsConfig>().ToArray();
                    break;
                case "RingColor":
                    this.ringColorConfigs = Resources.LoadAll(subfolder, typeof(PlanetColorsConfig)).Cast<PlanetColorsConfig>().ToArray();
                    break;
                case "AsteroidColor":
                    this.asteroidsColorConfigs = Resources.LoadAll(subfolder, typeof(AsteroidColorsConfig)).Cast<AsteroidColorsConfig>().ToArray();
                    break;
                case "MoonColor":
                    this.moonColorConfigs = Resources.LoadAll(subfolder, typeof(MoonColorsConfig)).Cast<MoonColorsConfig>().ToArray();
                    break;
            }
        }
    }

    [Button("Random Planet")]
    public void SpawnRandomPlanet()
    {
        if (this.spawnedPlanet != null)
            ReturnPlanetAndChildrenToPool(this.spawnedPlanet);

        this.spawnedPlanet = SpawnPlanetPrefab(this.planetParent);
        this.spawnedPlanet.GenerateRandomPlanet();
        this.spawnedPlanet.SetSpriteRenderDepths();

        this.planetData = spawnedPlanet.PlanetData;

        this.spawnedPlanet.transform.localPosition = this.planetData.Position;
        this.spawnedPlanet.transform.localScale = Vector3.one * Random.Range(this.planetData.OverrideScaleRange.x, this.planetData.OverrideScaleRange.y);

        this.planetData.RandomSeedString = this.spawnedPlanet.RandomSeedString;
    }

    [Button("Spawn Planet From Data")]
    public void GeneratePlanetFromData()
    {
        if (this.spawnedPlanet != null)
            ReturnPlanetAndChildrenToPool(this.spawnedPlanet);

        this.spawnedPlanet = SpawnPlanetPrefab(this.planetParent);
        this.spawnedPlanet.ConfigureFromData(this.planetData);
        this.spawnedPlanet.SetSpriteRenderDepths();

        this.spawnedPlanet.transform.localPosition = this.planetData.Position;
        this.spawnedPlanet.transform.localScale = Vector3.one * Random.Range(this.planetData.OverrideScaleRange.x, this.planetData.OverrideScaleRange.y);

        if (this.planetData.OverrideScaleRange.x != -1 && this.planetData.OverrideScaleRange.y != -1)
        {
            var scale = Random.Range(this.planetData.OverrideScaleRange.x, this.planetData.OverrideScaleRange.y);
            this.spawnedPlanet.transform.localScale = Vector3.one * scale;
        }
    }

    private Planet SpawnPlanetPrefab(Transform inParent)
    {    
        if (Pool.IN == null)
            return Instantiate<Planet>(this.planetPrefab, inParent);
        else
            return Pool.Spawn<Planet>(this.planetPrefab.name, inParent, Vector3.zero, Quaternion.identity);
    }
    
    private void ReturnPlanetAndChildrenToPool(Planet inPlanet)
    {
        foreach (var beltRenderer in inPlanet.AsteroidBeltRenderers)
        {
            beltRenderer.ClearAsteroids();
        }

        if (Pool.IN != null)
            Pool.Despawn(inPlanet.gameObject);
        else
            Destroy(inPlanet.gameObject);
    }
}