using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using static CrystalData;
using static LootData;

[CreateAssetMenu(fileName = "RockData", menuName = "Data/RockData", order = 3)]
public class RockData : ScriptableObject
{
    [Flags]
    public enum ERockType
    {
        None = 0,
        Sand = 1 << 0,
        Dirt = 1 << 1,
        SoftStone = 1 << 2,
        MediumStone = 1 << 3,
        HardStone = 1 << 4,
        Unbreakable = 1 << 5,
        Lava = 1 << 6,
        Other = 1 << 7
    }

    [TableColumnWidth(150, Resizable = false)]
    public string Name;

    [HideIf("IsAnimatedTexture"), VerticalGroup("Atributes"), LabelWidth(100)]
    public Color FillColor = Color.black, OutlineColor = Color.black;

    [VerticalGroup("Atributes"), LabelWidth(70), Range(-1, 100)] public float BaseHealth = -1;

    //damage on impact
    [VerticalGroup("Atributes"), LabelWidth(150)] public bool DoesDamageOnImpact;
    [ShowIf("DoesDamageOnImpact"), VerticalGroup("Atributes"), LabelWidth(130)] [Range(0, 100)] public float ImpactDamageBase;
    [ShowIf("DoesDamageOnImpact"), VerticalGroup("Atributes"), LabelWidth(130)] [Range(0, 100)] public float ImpactDamagePerSecondBase;

    //animated texture
    [VerticalGroup("Atributes"), LabelWidth(150)] public bool IsAnimatedTexture;

    [Header("Main Color")]
    [ShowIf("IsAnimatedTexture"), VerticalGroup("Atributes"), LabelWidth(70)] public Color[] RockAnimColors = new Color[2];
    [ShowIf("IsAnimatedTexture"), VerticalGroup("Atributes"), LabelWidth(130)] public AnimationCurve AnimCurve;
    [ShowIf("IsAnimatedTexture"), VerticalGroup("Atributes"), LabelWidth(130)] [Range(0, .1f)] public float ChangePerFrame = .01f;

    [ShowIf("IsAnimatedTexture"), VerticalGroup("Atributes"), LabelWidth(170)] public bool ShouldAnimateOutline;
    [ShowIf("@IsAnimatedTexture && ShouldAnimateOutline"), VerticalGroup("Atributes"), LabelWidth(70)] public Color[] RockOutlineColors = new Color[2];
    [ShowIf("@IsAnimatedTexture && ShouldAnimateOutline"), VerticalGroup("Atributes"), LabelWidth(130)] public AnimationCurve OutlineAnimCurve;
    [ShowIf("@IsAnimatedTexture && ShouldAnimateOutline"), VerticalGroup("Atributes"), LabelWidth(150)] [Range(0, .1f)] public float OutlineChangePerFrame = .01f;
    [ShowIf("@IsAnimatedTexture && ShouldAnimateOutline"), VerticalGroup("Atributes"), LabelWidth(70), Range(0,10)] public float[] RockOutlineWidths = new float[2];

    [Header("Loot Info")]
    [Range(1, 10)] public int Hardness = 1;//maybe make this mining tool required or something

    public ERockType RockType;
    public ELootType LootType;
    public ECrystalType CrystalType;

    [TableColumnWidth(60, Resizable = false)]
    [Button]
    public void Select()
    {
        var tileEditor = UnityEngine.Object.FindAnyObjectByType<TileEditorTool>();

        if (tileEditor != null)
            tileEditor.SelectRockPreset(this);
    }
}