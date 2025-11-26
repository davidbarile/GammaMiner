using UnityEngine;
using Sirenix.OdinInspector;

public class Planet : MonoBehaviour
{
    private static int planetDepthIndex = 0;

    public string RandomSeedString => this.randomSeedString;
    [SerializeField] private int randomSeed = -1;
    [SerializeField] private string randomSeedString;

    [SerializeField] private Vector2 planetScaleRange = new Vector2(-1f, -1f);
    [SerializeField] private Vector2 planetRotationRange = new Vector2(-1f, -1f);
    [SerializeField] private PlanetConfig planetConfig;
    [SerializeField] private AsteroidsConfig asteroidsConfig;
    [SerializeField] private AsteroidsConfig moonsConfig;

    [Space, SerializeField] private PlanetColorsConfig planetColorsConfig;
    [SerializeField] private PlanetRingsConfig planetRingsConfig;
    [SerializeField] private PlanetColorsConfig ringColorsConfig;
    [SerializeField] private AsteroidColorsConfig asteroidColorsConfig;
    [SerializeField] private MoonColorsConfig moonColorsConfig;


    public PlanetData PlanetData => this.planetData;
    [Space, SerializeField] private PlanetData planetData;

    [SerializeField] private int spriteRendererDepthIndex = -1;

    [Header("Planet Components")]
    [SerializeField] private Transform ringFlattenTransform;

    [Space, SerializeField] private SpriteRenderer baseSpriteRenderer;
    [SerializeField] private SpriteRenderer outlineSpriteRenderer;

    [Space, SerializeField] private GameObject cratersParent;
    [SerializeField] private SpriteRenderer[] craterRenderers;

    [Space, SerializeField] private SpriteRenderer[] cloudSpriteRenderers;
    [SerializeField] private SpriteRenderer[] stripesSpriteRenderers;
    [SerializeField] private SpriteRenderer[] ringSpriteRenderers;
    [SerializeField] private SpriteRenderer[] dustSpriteRenderers;

    [Space, SerializeField] private RotateObject cloudsRotateObject;
    [SerializeField] private RotateObject stripesRotateObject;
    [SerializeField] private RotateObject ringRotateObject;
    [Space, SerializeField] private RotateObject[] cloudRotateObjects;

    [Space, SerializeField] private RotateObject[] stripesRotateObjects;
    [SerializeField] private RotateObject[] dustRotateObjects;

    [Space, SerializeField] private GameObject asteroidsParent;
    [SerializeField] private AsteroidBeltRenderer[] asteroidBeltRenderers;

    [Space, SerializeField] private GameObject moonsParent;
    [SerializeField] private AsteroidBeltRenderer[] moonsRenderers;

    //make properties public for PlanetConfigGenerator to access
    public Vector2 PlanetScaleRange => this.planetScaleRange;
    public Vector2 PlanetRotationRange => this.planetRotationRange;
    public Transform RingFlattenTransform => this.ringFlattenTransform;

    public SpriteRenderer BaseSpriteRenderer => this.baseSpriteRenderer;
    public SpriteRenderer OutlineSpriteRenderer => this.outlineSpriteRenderer;
    public GameObject CratersParent => this.cratersParent;
    public SpriteRenderer[] CloudSpriteRenderers => this.cloudSpriteRenderers;
    public SpriteRenderer[] StripesSpriteRenderers => this.stripesSpriteRenderers;
    public SpriteRenderer[] RingSpriteRenderers => this.ringSpriteRenderers;
    public SpriteRenderer[] DustSpriteRenderers => this.dustSpriteRenderers;

    public RotateObject CloudsRotateObject => this.cloudsRotateObject;
    public RotateObject StripesRotateObject => this.stripesRotateObject;
    public RotateObject RingRotateObject => this.ringRotateObject;
    public RotateObject[] CloudRotateObjects => this.cloudRotateObjects;
    public RotateObject[] StripesRotateObjects => this.stripesRotateObjects;
    public RotateObject[] DustRotateObjects => this.dustRotateObjects;

    public GameObject AsteroidsParent => this.asteroidsParent;
    public AsteroidBeltRenderer[] AsteroidBeltRenderers => this.asteroidBeltRenderers;

    public GameObject MoonsParent => this.moonsParent;
    public AsteroidBeltRenderer[] MoonsRenderers => this.moonsRenderers;

    private SpriteRenderer[] allSpriteRenderers = new SpriteRenderer[0];
    private SpriteMask[] allSpriteMasks = new SpriteMask[0];

    private void Start()
    {
        //ConfigurePlanet();

        if (this.spriteRendererDepthIndex != -1)
            SetSpriteRenderDepths(this.spriteRendererDepthIndex);   
    }

    public void SetSpriteRenderDepths(int inDepthIndex = -1)
    {
        if (this.allSpriteRenderers.Length > 0)
            return;

        if (inDepthIndex == -1)
            inDepthIndex = Planet.planetDepthIndex;

        this.allSpriteRenderers = this.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in this.allSpriteRenderers)
        {
            if (sr == null || sr.gameObject == null)
                continue;

            sr.sortingOrder += (inDepthIndex * 4);//adjust this if more layers are added
        }

        this.allSpriteMasks = this.GetComponentsInChildren<SpriteMask>(true);
        foreach (var sm in this.allSpriteMasks)
        {
            if (sm == null || sm.gameObject == null)
                continue;

            sm.frontSortingOrder += (inDepthIndex * 4);
            sm.backSortingOrder += (inDepthIndex * 4);
        }

        if (this.spriteRendererDepthIndex == -1)
        {
            this.spriteRendererDepthIndex = Planet.planetDepthIndex;
            ++Planet.planetDepthIndex;
        }
    }

    public void ConfigureFromData(PlanetData inData)
    {
        this.planetData = inData;
        //SetRandomSeed(this.planetData.RandomSeed);
        SetRandomSeed(this.planetData.RandomSeedString);
        SetPosition(this.planetData.Position);
        // var rotation = UnityEngine.Random.Range(this.planetData.RotationRange.x, this.planetData.RotationRange.y);
        // SetRotation(rotation);

        if (this.planetData.OverrideScaleRange.x != -1 && this.planetData.OverrideScaleRange.y != -1)
        {
            var scale = UnityEngine.Random.Range(this.planetData.OverrideScaleRange.x, this.planetData.OverrideScaleRange.y);
            SetScale(scale);
        }

        ConfigurePlanet();
    }

    public void ConfigureFromSeedName(string inSeedName)
    {
        SetRandomSeed(inSeedName);
        ConfigurePlanet();
    }

    /// <summary>
    /// If the random seed is -1, it will generate a random seed and save it to the planetData.
    /// If the random seed is not -1, it will use the provided seed.
    /// </summary>
    public void SetRandomSeed(int inSeed)
    {
        //UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        if (inSeed != -1)
            this.randomSeed = inSeed;

        else if (this.randomSeed == -1)
            this.randomSeed = UnityEngine.Random.Range(0, 100000);

        if (!this.planetData.Name.Equals(string.Empty))
            this.planetData.RandomSeed = this.randomSeed;

        UnityEngine.Random.InitState(this.randomSeed);
    }

    public void SetRandomSeed(string inSeedString)
    {
        if (string.IsNullOrEmpty(inSeedString))
            this.randomSeedString = PlanetManager.GetRandomSeedString();
        else
            this.randomSeedString = inSeedString;

        if (string.IsNullOrEmpty(this.planetData.Name))
            this.planetData.Name = $"Planet_{this.randomSeedString}";

        this.planetData.RandomSeedString = this.randomSeedString;//added when sleepy
            
        var hashCode = this.randomSeedString.GetHashCode();

        UnityEngine.Random.InitState(hashCode);
    }

    public void SetPosition(Vector3 inPosition)
    {
        this.transform.localPosition = inPosition;
        if (!string.IsNullOrEmpty(this.planetData.Name))
            this.planetData.Position = inPosition;
    }

    // public void SetRotation(float inRotation)
    // {
    //     this.transform.localRotation = Quaternion.Euler(0f, 0f, inRotation);
    //     if (!string.IsNullOrEmpty(this.planetData.Name))
    //         this.planetData.RotationRange = new Vector2(inRotation, inRotation);
    // }

    public void SetScale(float inScale)
    {
        this.transform.localScale = Vector3.one * inScale;
        // if (!string.IsNullOrEmpty(this.planetData.Name))
        //     this.planetData.ScaleRange = new Vector2(inScale, inScale);
    }

    /// <summary>
    /// Generates a random planet using the current random seed.
    /// If the random seed is set, it should create a consistent planet.
    /// </summary>
    [Button("Generate Random Planet")]
    public void GenerateRandomPlanet()
    {
        // var originalSeed = this.randomSeed;
        // SetRandomSeed(this.randomSeed);

        // if (originalSeed == -1)
        //     this.randomSeed = originalSeed;
        // else if (this.incrementSeedOnGenerate)
        //     ++this.randomSeed;

        var originalSeedString = this.randomSeedString;
        var rndString = PlanetManager.GetRandomSeedString();
        SetRandomSeed(rndString);

        Debug.Log($"Generating planet Seed = [{this.randomSeedString}]     hashCode = [{this.randomSeedString.GetHashCode()}]");

        if (Application.isPlaying)
            this.name = $"Planet_{rndString}";

        ConfigurePlanet();

        if (!Application.isPlaying)
             this.randomSeedString = originalSeedString;
    }

    private void GetRandomConfigs()
    {
        if (PlanetManager.IN == null)
            PlanetManager.IN = Object.FindAnyObjectByType<PlanetManager>();

        this.planetConfig = PlanetManager.IN.PlanetConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.PlanetConfigs.Length)];
        this.asteroidsConfig = PlanetManager.IN.AsteroidsConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.AsteroidsConfigs.Length)];
        this.moonsConfig = PlanetManager.IN.MoonConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.MoonConfigs.Length)];

        this.planetColorsConfig = PlanetManager.IN.PlanetColorConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.PlanetColorConfigs.Length)];
        this.ringColorsConfig = PlanetManager.IN.RingColorConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.RingColorConfigs.Length)];
        this.planetRingsConfig = PlanetManager.IN.RingConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.RingConfigs.Length)];
        this.asteroidColorsConfig = PlanetManager.IN.AsteroidsColorConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.AsteroidsColorConfigs.Length)];
        this.moonColorsConfig = PlanetManager.IN.MoonColorConfigs[UnityEngine.Random.Range(0, PlanetManager.IN.MoonColorConfigs.Length)];
    }

    [Button("Configure Planet")]
    private void ConfigurePlanet()
    {
        GetRandomConfigs();

        var pc = this.planetConfig;

        var scale = UnityEngine.Random.Range(pc.ScaleRange.x, pc.ScaleRange.y);

        if (this.PlanetScaleRange.x != -1 && this.PlanetScaleRange.y != -1)
            scale = UnityEngine.Random.Range(this.PlanetScaleRange.x, this.PlanetScaleRange.y);
  
        this.transform.localScale = Vector3.one * scale;

        var rotation = UnityEngine.Random.Range(pc.RotationRange.x, pc.RotationRange.y);
        this.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

        if (pc.HasClouds && pc.HasStripes)
        {
            this.baseSpriteRenderer.color = this.planetColorsConfig.BaseColor1;
            this.outlineSpriteRenderer.color = this.planetColorsConfig.OutlineColor1;
        }
        else
        {
            //if no clouds or stripes, randomly choose between two color sets
            int rnd = UnityEngine.Random.Range(0, 1);
            var baseColor = Color.Lerp(this.planetColorsConfig.BaseColor1, this.planetColorsConfig.BaseColor2, rnd);
            var outlineColor = Color.Lerp(this.planetColorsConfig.OutlineColor1, this.planetColorsConfig.OutlineColor2, rnd);
            this.baseSpriteRenderer.color = baseColor;
            this.outlineSpriteRenderer.color = outlineColor;
        }

        //craters
        this.cratersParent.SetActive(pc.HasCraters);
        if (pc.HasCraters)
        {
            var color = this.outlineSpriteRenderer.color;
            //var rndAlpha = UnityEngine.Random.Range(.4f, 1f);
            for (int i = 0; i < this.craterRenderers.Length; i++)
            {
                var craterSR = this.craterRenderers[i];

                var rndAlpha = UnityEngine.Random.Range(.2f, .5f);
                craterSR.color = new Color(color.r, color.g, color.b, rndAlpha);
            }
        }

        //clouds
        this.cloudsRotateObject.gameObject.SetActive(pc.HasClouds);

        if (pc.HasClouds)
        {
            this.cloudsRotateObject.transform.localRotation = Quaternion.Euler(0f, 0f, pc.CloudsAngleOffset);
            this.cloudsRotateObject.SetRotationAmount2D(pc.CloudRotationSpeed);

            for (int i = 0; i < this.cloudSpriteRenderers.Length; i++)
            {
                var cloudSR = this.cloudSpriteRenderers[i];
                cloudSR.gameObject.SetActive(pc.CloudDatas[i].IsVisible);
                cloudSR.color = this.planetColorsConfig.CloudOrStripeColors[i];
                this.cloudRotateObjects[i].SetRotationAmount2D(pc.CloudDatas[i].CloudRotationSpeed);
            }
        }

        //stripes
        this.stripesRotateObject.gameObject.SetActive(pc.HasStripes);

        if (pc.HasStripes)
        {
            this.stripesRotateObject.transform.localRotation = Quaternion.Euler(0f, 0f, pc.StripesAngleOffset);
            this.stripesRotateObject.SetRotationAmount2D(pc.StripesRotationSpeed);

            for (int i = 0; i < this.stripesSpriteRenderers.Length; i++)
            {
                var stripesSR = this.stripesSpriteRenderers[i];
                stripesSR.gameObject.SetActive(pc.StripeDatas[i].IsVisible);
                stripesSR.color = this.planetColorsConfig.CloudOrStripeColors[i];
                this.stripesRotateObjects[i].SetRotationAmount2D(pc.StripeDatas[i].CloudRotationSpeed);
            }
        }

        //rings
        var rc = this.planetRingsConfig;

        this.ringRotateObject.gameObject.SetActive(rc.HasRings);

        if (rc.HasRings)
        {
            this.ringFlattenTransform.localScale = new Vector3(rc.RingFlattenAmount, 1f, 1f) * rc.RingsScale;

            var ringRot = this.ringRotateObject;
            ringRot.transform.localScale = Vector3.one * rc.RingsScale;
            ringRot.transform.localRotation = Quaternion.Euler(0f, 0f, rc.RingsAngleOffset);
            ringRot.SetRotationAmount2D(rc.RingsRotationSpeed);

            for (int i = 0; i < this.ringSpriteRenderers.Length; i++)
            {
                var ringSR = this.ringSpriteRenderers[i];
                ringSR.gameObject.SetActive(rc.RingDatas[i].IsVisible);
                ringSR.enabled = rc.RingDatas[i].IsEnabled;
                ringSR.color = this.ringColorsConfig.RingColors[i];
            }

            for (int i = 0; i < this.dustSpriteRenderers.Length; i++)
            {
                var dustSR = this.dustSpriteRenderers[i];
                dustSR.gameObject.SetActive(rc.DustDatas[i].IsVisible);
                dustSR.enabled = rc.DustDatas[i].IsEnabled;
                dustSR.color = this.ringColorsConfig.DustColors[i];
                this.dustRotateObjects[i].SetRotationAmount2D(rc.DustDatas[i].RotationSpeed);
            }
        }

        //asteroid belts
        var ac = this.asteroidsConfig;

        this.asteroidsParent.SetActive(ac.HasEntities);

        if (ac.HasEntities)
        {
            for (int i = 0; i < this.asteroidBeltRenderers.Length; i++)
            {
                var belt = this.asteroidBeltRenderers[i];

                var shouldShow = i < ac.AsteroidBeltDatas.Count;

                belt.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    var data = ac.AsteroidBeltDatas[i];
                    belt.name = $"AsteroidBelt_{i}";

                    belt.ConfigureFromData(data, this.spriteRendererDepthIndex);
                    belt.ColorFromColorConfigs(this.asteroidColorsConfig, i);
                }
            }
        }

        //moons
        var mc = this.moonsConfig;

        this.moonsParent.SetActive(mc.HasEntities);

        if (mc.HasEntities)
        {
            for (int i = 0; i < this.moonsRenderers.Length; i++)
            {
                var moon = this.moonsRenderers[i];

                var shouldShow = i < mc.MoonDatas.Count;

                moon.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    var data = mc.MoonDatas[i];
                    moon.name = $"Moon_{i}";

                    moon.ConfigureFromData(data, this.spriteRendererDepthIndex);
                    moon.ColorMoonFromColorConfigs(this.moonColorsConfig);
                }
            }
        }
    }
    
    [Button("Add Random Seed To Favorites")]
    public void AddRandomSeedToFavorites(string inSeedName)
    {
        if (PlanetManager.IN == null)
            PlanetManager.IN = Object.FindAnyObjectByType<PlanetManager>();

        var seedName = inSeedName;

        if (string.IsNullOrEmpty(inSeedName))
        {
            var originalSeedString = this.randomSeedString;
            SetRandomSeed(this.randomSeedString);

            seedName = this.randomSeedString;

            if(Application.isPlaying)
                this.name = $"Planet_{this.randomSeedString}";
            else
                this.randomSeedString = originalSeedString;
        }

        PlanetManager.IN.AddRandomSeedToFavorites(seedName);
    }
}