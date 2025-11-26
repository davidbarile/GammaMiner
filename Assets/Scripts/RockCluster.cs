using System;
using System.Collections.Generic;
using UnityEngine;
using static HealthEntity;
using Sirenix.OdinInspector;

public class RockCluster : MonoBehaviour
{
    #region Fix Bug
    //public bool DoValidate;

    //public Rock[] newRocks;

    //private void OnValidate()
    //{
    //    if (DoValidate)
    //    {
    //        Debug.Log("<color=green>Do Validate</color>");
    //        for (int i = 0; i < Rocks.Length; i++)
    //        {
    //            var rock = Rocks[i];
    //            var newRock = newRocks[i];

    //            newRock.transform.localPosition = rock.transform.localPosition;
    //            newRock.transform.localScale = rock.transform.localScale;

    //            newRock.Shape.color1 = rock.Shape.color1;
    //            newRock.Shape.color2 = rock.Shape.color2;

    //            newRock.Shape.outlineColor = rock.Shape.outlineColor;

    //            newRock.Shape.outlineWidth = rock.Shape.outlineWidth;

    //            //rock.Shape.pivotType = PS2DPivotType.Manual;//requires MovePivot()

    //            newRock.Shape.points = new List<PS2DPoint>(rock.Shape.points);

    //            newRock.Shape.texture = rock.Shape.texture;

    //            newRock.Shape.SetCustomMaterial();
    //            newRock.Shape.UpdateMaterialSettings();
    //            newRock.Shape.UpdateMesh();
    //        }
    //    }
    //}
    #endregion

    [Header("Rock Style")]
    public Texture2D Texture;
    public Color FillColor = Color.clear;
    public Color OutlineColor = Color.clear;
    [Range(-1, 20)] public float OutlineWidth = -1;

    [Header("Use for double outline")]
    [SerializeField] private bool shouldCreateMiddleShape;
    [Range(0, 1)] [SerializeField] private float middleOutlineWidth;
    [SerializeField] private Color middleOutlineColor;

    [Header("Rock Health")]
    public float BaseRockHealth;
    public bool ShouldUseRigidbodyAutoMass;

    [Header("Explosion")]
    public Explosion ExplosionPrefab;
    public int ExplosionBaseDamage;

    [Header("Loot")]
    [Range(1, 20)] public int LootProbabilityWeight = 1;

    public Rock[] Rocks;

    private bool isRockHealthInitialized;

    public void Start()
    {
        if (TileEditorTool.IsEditing)
        {
            this.Rocks = this.GetComponentsInChildren<Rock>(false);
            //TileEditorTool.InitRocks(this);//TODO: name need to come back
            return;
        }

        if (this.Rocks.Length == 0)
        {
            Debug.Log($"<color=red>RockCluster.Start()   Rocks.Length = 0</color>");
            this.Rocks = this.GetComponentsInChildren<Rock>(false);
        }

        RegisterLootEvents();
    }

    /// <summary>
    /// Called from RockCluster Editor
    /// </summary>
    public void RefreshRocksArray(bool inShouldRenameRocks = false)
    {
        this.Rocks = this.GetComponentsInChildren<Rock>(false);

        if (inShouldRenameRocks)
        {
            for (int i = 0; i < this.Rocks.Length; i++)
            {
                var rock = this.Rocks[i];
                rock.Rename($"Rock_{i}");

                if (rock.Fill)
                    rock.Fill.sortingOrder = i + 1;
                TileEditorTool.SetDirty(rock);
                TileEditorTool.SetDirty(rock.gameObject);
            }
        }

        TileEditorTool.SetDirty(this);
    }

    /// <summary>
    /// Called from RockCluster Editor
    /// </summary>
    public void RefreshRocksArrayIncludeHidden()
    {
        this.Rocks = this.GetComponentsInChildren<Rock>(true);

        TileEditorTool.SetDirty(this);
    }

    //TODO: implement Rock override values
    public void UpdateRockStyle(bool inShouldGenerateChildren, bool inShouldOverrideWithClusterValues)
    {
        //Debug.Log($"{name}  UpdateRockStyle({inShouldGenerateChildren}, {inShouldOverrideWithClusterValues})");
        foreach (var rock in this.Rocks)
        {
            if (rock.Fill || rock.transform.childCount == 0)
            {
                rock.gameObject.SetActive(true);

                if (inShouldOverrideWithClusterValues)
                {
                    if (this.FillColor.a > 0 && rock.Fill != null)
                    {
                        rock.Fill.color = this.FillColor;
                        TileEditorTool.SetDirty(rock.Fill);
                    }

                    if (this.OutlineColor.a > 0 && rock.Outline != null)
                    {
                        rock.Outline.color = this.OutlineColor;
                        TileEditorTool.SetDirty(rock.Outline);
                    }
                }
            }

            TileEditorTool.SetDirty(rock);
            TileEditorTool.SetDirty(rock.gameObject);
        }
    }

    public void InitRockHealths()
    {
        //Debug.Log($"InitRockHealth() {name}");
        if (this.isRockHealthInitialized)
            return;

        this.isRockHealthInitialized = true;

        foreach (var rock in this.Rocks)
        {
            rock.InitHealth(this.ExplosionPrefab, this.ExplosionBaseDamage, this.BaseRockHealth, this.ShouldUseRigidbodyAutoMass);
        }
    }

    private void RegisterLootEvents()
    {
        foreach (var rock in this.Rocks)
        {
            rock.HealthEntity.OnDie += rock.LootSpawner.OnRockDestroy;
            
            if (rock.LootSpawner.LootProbabilityWeight != 1)
                rock.LootSpawner.LootProbabilityWeight = this.LootProbabilityWeight;
        }
    }

    private void OnDestroy()
    {
        foreach (var rock in this.Rocks)
        {
            if(rock != null)
            {
                rock.HealthEntity.OnDie -= rock.Explode;
                rock.HealthEntity.OnDie -= rock.LootSpawner.OnRockDestroy;
            }
        }
    }
}