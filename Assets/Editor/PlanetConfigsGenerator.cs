using UnityEngine;
using UnityEditor;

public class PlanetConfigsGenerator : EditorWindow
{
    // Serialized field to drag and drop a Planet from the hierarchy
    [SerializeField] private GameObject planetsParent;
    [SerializeField] private GameObject moonsParent;

    [SerializeField] private int counterIndex;

    // SerializedObject for handling the Editor GUI
    private SerializedObject editorSerializedObject;
    private SerializedProperty planetsParentProperty;
    private SerializedProperty moonsParentProperty;
    private SerializedProperty counterIndexProperty;


    private Planet targetPlanet = null;

    private int planetConfigCounter = 1;
    private int ringConfigCounter = 1;
    private int asteroidConfigCounter = 1;
    private int moonConfigCounter = 1;
    private int planetColorConfigCounter = 1;
    private int ringColorConfigCounter = 1;
    private int asteroidColorConfigCounter = 1;
    private int moonColorConfigCounter = 1;

    [MenuItem("Tools/Open Planet Configs Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<PlanetConfigsGenerator>("Planet Configs Generator");
    }

    private void OnEnable()
    {
        // Create a SerializedObject for this EditorWindow
        this.editorSerializedObject = new SerializedObject(this);
        this.planetsParentProperty = this.editorSerializedObject.FindProperty("planetsParent");
        this.moonsParentProperty = this.editorSerializedObject.FindProperty("moonsParent");
        this.counterIndexProperty = this.editorSerializedObject.FindProperty("counterIndex");
    }

    private void OnGUI()
    {
        GUILayout.Label("This tool generates planet, color, ring, asteroid and moon configurations for the game.", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space();

        // Update and draw the serialized property for drag-and-drop
        this.editorSerializedObject.Update();

        EditorGUILayout.PropertyField(this.planetsParentProperty, new GUIContent("Planets Parent"));

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Planet Configs"))
        {
            CreateConfigsForAllPlanets("Planet");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Ring Configs"))
        {
            CreateConfigsForAllPlanets("Ring");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Asteroid Belt Configs"))
        {
            CreateConfigsForAllPlanets("Asteroid");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Moon Configs"))
        {
            CreateConfigsForAllPlanets("Moon");
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Generate Planet Color Configs"))
        {
            CreateConfigsForAllPlanets("PlanetColor");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Ring Color Configs"))
        {
            CreateConfigsForAllPlanets("RingColor");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Asteroid Color Configs"))
        {
            CreateConfigsForAllPlanets("AsteroidColor");
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(this.moonsParentProperty, new GUIContent("Moons Parent"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(this.counterIndexProperty, new GUIContent("Counter Index"));

        this.editorSerializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Moon Color Configs"))
        {
            CreateColorConfigsForAllMoons();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Generate All Prefab Configs"))
        {
            CreateConfigsForAllPlanets("Planet");
            CreateConfigsForAllPlanets("Ring");
            CreateConfigsForAllPlanets("Asteroid");
            CreateConfigsForAllPlanets("Moon");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate All Color Configs"))
        {
            CreateConfigsForAllPlanets("PlanetColor");
            CreateConfigsForAllPlanets("RingColor");
            CreateConfigsForAllPlanets("AsteroidColor");
            //CreateColorConfigsForAllMoons();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Reset Counter"))
        {
            ResetConfigCounters();
        }
    }

    private void ResetConfigCounters()
    {
        this.planetConfigCounter = this.counterIndex;
        this.ringConfigCounter = this.counterIndex;
        this.asteroidConfigCounter = this.counterIndex;
        this.moonConfigCounter = this.counterIndex;

        this.planetColorConfigCounter = this.counterIndex;
        this.ringColorConfigCounter = this.counterIndex;
        this.asteroidColorConfigCounter = this.counterIndex;
        this.moonColorConfigCounter = this.counterIndex;
    }

    private void CreateConfigsForAllPlanets(string inConfigType)
    {
        var allVisiblePlanetsInParent = this.planetsParent.GetComponentsInChildren<Planet>();

        foreach (var planet in allVisiblePlanetsInParent)
        {
            this.targetPlanet = planet;
            CreateConfig(inConfigType);
        }
    }

    private void CreateColorConfigsForAllMoons()
    {
        var allVisibleMoonsInParent = this.moonsParent.GetComponentsInChildren<MoonPalette>();

        foreach (var moonPalette in allVisibleMoonsInParent)
        {
            CreateMoonColorConfig(moonPalette);
        }
    }  

    private void CreateConfig(string inConfigType)
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("<color=red>Cannot create config when not playing.</color>");
            return;
        }

        if (inConfigType == "AsteroidColor" && !this.targetPlanet.AsteroidsParent.gameObject.activeSelf)
            return;

        if (inConfigType == "RingColor" && !this.targetPlanet.RingRotateObject.gameObject.activeSelf)
            return;

        Debug.Log($"Generating {inConfigType} Configs for <color=white>{this.targetPlanet.name}</color>");

        var rootPath = "Assets/Resources/PlanetConfigs";
        var isRootFolderValid = AssetDatabase.IsValidFolder(rootPath);

        if (!isRootFolderValid)
        {
            ResetConfigCounters();
            AssetDatabase.CreateFolder("Assets/Resources", "PlanetConfigs");
        }

        var path = $"Assets/Resources/PlanetConfigs/{inConfigType}";
        var isFolderValid = AssetDatabase.IsValidFolder(path);

        if (!isFolderValid)
            AssetDatabase.CreateFolder("Assets/Resources/PlanetConfigs", $"{inConfigType}");

        var counter = inConfigType switch
        {
            "Planet" => this.planetConfigCounter,
            "Ring" => this.ringConfigCounter,
            "Asteroid" => this.asteroidConfigCounter,
            "Moon" => this.moonConfigCounter,
            "PlanetColor" => this.planetColorConfigCounter,
            "RingColor" => this.ringColorConfigCounter,
            "AsteroidColor" => this.asteroidColorConfigCounter,
            _ => 0
        };

        var cardNumString = (counter < 10) ? $"0{counter}" : counter.ToString();
        var fileName = $"{inConfigType}Config_{cardNumString}";
        var pathAndFileName = $"{path}/{fileName}.asset";

        Debug.Log($"Creating [{inConfigType}] config for {this.targetPlanet.name} at path: {pathAndFileName}");

        switch (inConfigType)
        {
            case "Planet":
                var planetConfig = CreateInstance<PlanetConfig>();
                GeneratePlanetConfigs(planetConfig);
                AssetDatabase.CreateAsset(planetConfig, pathAndFileName);//$"Assets/Resources/YourSavePath/YourObject.asset"
                EditorUtility.SetDirty(planetConfig);
                ++this.planetConfigCounter;
                break;

            case "Ring":
                var ringConfig = CreateInstance<PlanetRingsConfig>();
                ringConfig.name = fileName;
                AssetDatabase.CreateAsset(ringConfig, pathAndFileName);
                GenerateRingConfigs(ringConfig);
                EditorUtility.SetDirty(ringConfig);
                ++this.ringConfigCounter;
                break;

            case "Asteroid":
                var asteroidConfig = CreateInstance<AsteroidsConfig>();
                asteroidConfig.name = fileName;
                AssetDatabase.CreateAsset(asteroidConfig, pathAndFileName);
                GenerateAsteroidOrMoonConfigs(asteroidConfig, false);
                EditorUtility.SetDirty(asteroidConfig);
                ++this.asteroidConfigCounter;
                break;

            case "Moon":
                var moonConfig = CreateInstance<AsteroidsConfig>();
                moonConfig.name = fileName;
                AssetDatabase.CreateAsset(moonConfig, pathAndFileName);
                GenerateAsteroidOrMoonConfigs(moonConfig, true);
                EditorUtility.SetDirty(moonConfig);
                ++this.moonConfigCounter;
                break;

            case "PlanetColor":
                var colorConfig = CreateInstance<PlanetColorsConfig>();
                colorConfig.name = fileName;
                AssetDatabase.CreateAsset(colorConfig, pathAndFileName);
                GenerateColorConfigs(colorConfig);
                EditorUtility.SetDirty(colorConfig);
                ++this.planetColorConfigCounter;
                break;

            case "RingColor":
                var ringColorConfig = CreateInstance<PlanetColorsConfig>();
                ringColorConfig.name = fileName;
                AssetDatabase.CreateAsset(ringColorConfig, pathAndFileName);
                GenerateColorConfigs(ringColorConfig);
                EditorUtility.SetDirty(ringColorConfig);
                ++this.ringColorConfigCounter;
                break;

            case "AsteroidColor":
                var asteroidColorConfig = CreateInstance<AsteroidColorsConfig>();
                asteroidColorConfig.name = fileName;
                AssetDatabase.CreateAsset(asteroidColorConfig, pathAndFileName);
                GenerateAsteroidColorConfigs(asteroidColorConfig);
                EditorUtility.SetDirty(asteroidColorConfig);
                ++this.asteroidColorConfigCounter;
                break;

            default:
                Debug.LogError($"Unknown config type: {inConfigType}");
                return;
        }
    }

    private void GeneratePlanetConfigs(PlanetConfig inPlanetConfig)
    {
        //Base planet
        if (this.targetPlanet.PlanetScaleRange.x != -1 && this.targetPlanet.PlanetScaleRange.y != -1)
            inPlanetConfig.ScaleRange = this.targetPlanet.PlanetScaleRange;
        else
            inPlanetConfig.ScaleRange = new Vector2(this.targetPlanet.transform.localScale.x, this.targetPlanet.transform.localScale.y);

        if (this.targetPlanet.PlanetRotationRange.x != -1 && this.targetPlanet.PlanetRotationRange.y != -1)
            inPlanetConfig.RotationRange = this.targetPlanet.PlanetRotationRange;
        else
            inPlanetConfig.RotationRange = new Vector2(this.targetPlanet.transform.localRotation.eulerAngles.z, this.targetPlanet.transform.localRotation.eulerAngles.z);

        // Craters
        inPlanetConfig.HasCraters = this.targetPlanet.CratersParent.activeSelf;

        // Clouds
        inPlanetConfig.HasClouds = this.targetPlanet.CloudsRotateObject.gameObject.activeSelf;
        if (inPlanetConfig.HasClouds)
        {
            inPlanetConfig.CloudsAngleOffset = this.targetPlanet.CloudsRotateObject.transform.localRotation.eulerAngles.z;
            inPlanetConfig.CloudRotationSpeed = this.targetPlanet.CloudsRotateObject.RotationAmount.z;

            for (int i = 0; i < this.targetPlanet.CloudSpriteRenderers.Length; i++)
            {
                inPlanetConfig.CloudDatas.Add(new CloudData
                {
                    IsVisible = this.targetPlanet.CloudSpriteRenderers[i].gameObject.activeSelf,
                    CloudRotationSpeed = this.targetPlanet.CloudRotateObjects[i].RotationAmount.z
                });
            }
        }

        // Stripes
        inPlanetConfig.HasStripes = this.targetPlanet.StripesRotateObject.gameObject.activeSelf;
        if (inPlanetConfig.HasStripes)
        {
            inPlanetConfig.StripesAngleOffset = this.targetPlanet.StripesRotateObject.transform.localRotation.eulerAngles.z;
            inPlanetConfig.StripesRotationSpeed = this.targetPlanet.StripesRotateObject.RotationAmount.z;

            for (int i = 0; i < this.targetPlanet.StripesSpriteRenderers.Length; i++)
            {
                inPlanetConfig.StripeDatas.Add(new CloudData
                {
                    IsVisible = this.targetPlanet.StripesSpriteRenderers[i].gameObject.activeSelf,
                    CloudRotationSpeed = this.targetPlanet.StripesRotateObjects[i].RotationAmount.z
                });
            }
        }
    }

    private void GenerateRingConfigs(PlanetRingsConfig inRingConfig)
    {
        inRingConfig.HasRings = this.targetPlanet.RingRotateObject.gameObject.activeSelf;

        if (inRingConfig.HasRings)
        {
            inRingConfig.RingsScale = this.targetPlanet.RingRotateObject.transform.localScale.x; // Assuming uniform scale for rings
            inRingConfig.RingsAngleOffset = this.targetPlanet.RingRotateObject.transform.localRotation.eulerAngles.z;
            inRingConfig.RingsRotationSpeed = this.targetPlanet.RingRotateObject.RotationAmount.z;
            inRingConfig.RingFlattenAmount = this.targetPlanet.RingFlattenTransform.localScale.x;

            for (int i = 0; i < this.targetPlanet.RingSpriteRenderers.Length; i++)
            {
                var ringData = new RingData
                {
                    IsVisible = this.targetPlanet.RingSpriteRenderers[i].gameObject.activeInHierarchy,
                    IsEnabled = this.targetPlanet.RingSpriteRenderers[i].enabled,
                };
                inRingConfig.RingDatas.Add(ringData);
            }

            for (int i = 0; i < this.targetPlanet.DustSpriteRenderers.Length; i++)
            {
                inRingConfig.DustDatas.Add(new DustData
                {
                    IsVisible = this.targetPlanet.DustSpriteRenderers[i].gameObject.activeInHierarchy,
                    IsEnabled = this.targetPlanet.DustSpriteRenderers[i].enabled,
                    RotationSpeed = this.targetPlanet.DustRotateObjects[i].RotationAmount.z
                });
            }
        }
    }

    private void GenerateAsteroidOrMoonConfigs(AsteroidsConfig inAsteroidConfig, bool inIsMoonConfig)
    {
        inAsteroidConfig.IsMoonConfig = inIsMoonConfig;
        inAsteroidConfig.HasEntities = inIsMoonConfig ? this.targetPlanet.MoonsParent.activeSelf : this.targetPlanet.AsteroidsParent.activeSelf;

        var renderersArray = inIsMoonConfig ? this.targetPlanet.MoonsRenderers : this.targetPlanet.AsteroidBeltRenderers;

        if (inAsteroidConfig.HasEntities)
        {
            for (int i = 0; i < renderersArray.Length; i++)
            {
                // Get the asteroid or moon renderer
                var renderer = renderersArray[i];

                var angleRange = renderer.AsteroidBeltData.OrbitAngleRange;

                if (angleRange.x == -1 && angleRange.y == -1)
                {
                    angleRange = new Vector2(
                        renderer.transform.localRotation.eulerAngles.z,
                        renderer.transform.localRotation.eulerAngles.z);
                }

                var data = new AsteroidBeltData
                {
                    IsVisible = renderer.gameObject.activeSelf,
                    IsMoon = inIsMoonConfig,
                    NumberOfAsteroidsRange = renderer.AsteroidBeltData.NumberOfAsteroidsRange,
                    AsteroidSizeRange = renderer.AsteroidBeltData.AsteroidSizeRange,
                    YOffsetRange = renderer.AsteroidBeltData.YOffsetRange,
                    OrbitRadiusRange = renderer.AsteroidBeltData.OrbitRadiusRange,
                    OrbitAngleRange = angleRange,
                    OrbitRotationSpeedRange = renderer.AsteroidBeltData.OrbitRotationSpeedRange,
                    AsteroidRotationSpeedRange = renderer.AsteroidBeltData.AsteroidRotationSpeedRange,
                    AsteroidBaseDepth = renderer.AsteroidBeltData.AsteroidBaseDepth,
                    AsteroidOutlineColor1 = renderer.AsteroidBeltData.AsteroidOutlineColor1,
                    AsteroidOutlineColor2 = renderer.AsteroidBeltData.AsteroidOutlineColor2,
                    AsteroidFillColor1 = renderer.AsteroidBeltData.AsteroidFillColor1,
                    AsteroidFillColor2 = renderer.AsteroidBeltData.AsteroidFillColor2,
                    MoonOutlineColor = renderer.AsteroidBeltData.MoonOutlineColor,
                    MoonFillColor = renderer.AsteroidBeltData.MoonFillColor
                };

                if (inIsMoonConfig)
                    inAsteroidConfig.MoonDatas.Add(data);
                else
                    inAsteroidConfig.AsteroidBeltDatas.Add(data);
            }
        }
    }

    private void GenerateColorConfigs(PlanetColorsConfig inColorConfig)
    {
        //base planet
        inColorConfig.BaseColor1 = this.targetPlanet.BaseSpriteRenderer.color;
        inColorConfig.BaseColor2 = this.targetPlanet.BaseSpriteRenderer.color;
        inColorConfig.OutlineColor1 = this.targetPlanet.OutlineSpriteRenderer.color;
        inColorConfig.OutlineColor2 = this.targetPlanet.OutlineSpriteRenderer.color;

        //Rings
        for (int i = 0; i < this.targetPlanet.RingSpriteRenderers.Length; i++)
        {
            var sr = this.targetPlanet.RingSpriteRenderers[i];
            var color = sr.gameObject.activeSelf ? sr.color : Color.clear;
            inColorConfig.RingColors.Add(color);
        }

        //Dust
        for (int i = 0; i < this.targetPlanet.DustSpriteRenderers.Length; i++)
        {
            var sr = this.targetPlanet.DustSpriteRenderers[i];
            var color = sr.gameObject.activeSelf ? sr.color : Color.clear;
            inColorConfig.DustColors.Add(color);
        }

        // Clouds and Stripes
        if (this.targetPlanet.CloudsRotateObject.gameObject.activeSelf)
        {
            for (int i = 0; i < this.targetPlanet.CloudSpriteRenderers.Length; i++)
            {
                var sr = this.targetPlanet.CloudSpriteRenderers[i];
                var color = sr.gameObject.activeSelf ? sr.color : Color.clear;
                inColorConfig.CloudOrStripeColors.Add(color);
            }
        }
        else if (this.targetPlanet.StripesRotateObject.gameObject.activeSelf)
        {
            for (int i = 0; i < this.targetPlanet.StripesSpriteRenderers.Length; i++)
            {
                var sr = this.targetPlanet.StripesSpriteRenderers[i];
                var color = sr.gameObject.activeSelf ? sr.color : Color.clear;
                inColorConfig.CloudOrStripeColors.Add(color);
            }
        }
        else
        {
            for (int i = 0; i < this.targetPlanet.CloudSpriteRenderers.Length; i++)
            {
                inColorConfig.CloudOrStripeColors.Add(Color.clear);
            }
        }

        EditorUtility.SetDirty(inColorConfig);
    }

    private void GenerateAsteroidColorConfigs(AsteroidColorsConfig inColorConfig)
    {
        // Asteroids
        for (int i = 0; i < this.targetPlanet.AsteroidBeltRenderers.Length; i++)
        {
            var asteroidBeltRenderer = this.targetPlanet.AsteroidBeltRenderers[i];
            inColorConfig.Outline1Colors.Add(asteroidBeltRenderer.AsteroidBeltData.AsteroidOutlineColor1);
            inColorConfig.Outline2Colors.Add(asteroidBeltRenderer.AsteroidBeltData.AsteroidOutlineColor2);
            inColorConfig.Fill1Colors.Add(asteroidBeltRenderer.AsteroidBeltData.AsteroidFillColor1);
            inColorConfig.Fill2Colors.Add(asteroidBeltRenderer.AsteroidBeltData.AsteroidFillColor2);
        }
    }

    private void CreateMoonColorConfig(MoonPalette inTargetMoonPalette)
    {
        Debug.Log($"Generating Moon Color Configs for <color=white>{inTargetMoonPalette.name}</color>");

        var rootPath = "Assets/Resources/PlanetConfigs";
        var isRootFolderValid = AssetDatabase.IsValidFolder(rootPath);

        if (!isRootFolderValid)
        {
            ResetConfigCounters();
            AssetDatabase.CreateFolder("Assets/Resources", "PlanetConfigs");
        }

        var path = $"Assets/Resources/PlanetConfigs/MoonColor";
        var isFolderValid = AssetDatabase.IsValidFolder(path);

        if (!isFolderValid)
            AssetDatabase.CreateFolder("Assets/Resources/PlanetConfigs", "MoonColor");

        var counter = this.moonColorConfigCounter;
        var cardNumString = (counter < 10) ? $"0{counter}" : counter.ToString();
        var fileName = $"MoonColorConfig_{cardNumString}";
        var pathAndFileName = $"{path}/{fileName}.asset";

        //Debug.Log($"Creating [Moon Color] config for {inTargetMoonPalette.name} at path: {pathAndFileName}");

        var moonColorConfig = CreateInstance<MoonColorsConfig>();
        moonColorConfig.name = fileName;
        AssetDatabase.CreateAsset(moonColorConfig, pathAndFileName);

        // Set the colors from the target moon palette
        moonColorConfig.OutlineColor1 = inTargetMoonPalette.OutlineSpriteRenderer.color;
        moonColorConfig.OutlineColor2 = inTargetMoonPalette.OutlineSpriteRenderer.color;
        moonColorConfig.FillColor1 = inTargetMoonPalette.FillSpriteRenderer.color;
        moonColorConfig.FillColor2 = inTargetMoonPalette.FillSpriteRenderer.color;
        
        EditorUtility.SetDirty(moonColorConfig);
        
        ++this.moonColorConfigCounter;
    } 
}