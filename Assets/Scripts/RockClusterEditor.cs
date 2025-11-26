using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class RockClusterEditor : MonoBehaviour
{
    public static bool ShouldDeleteHiddenRocks;

    public static GameObject DeleteMe;

    [SerializeField] private bool shouldRenameRocks;
    private bool shouldGenerateChildren;

    private bool reset;
    private bool enableAllRocks;

#if UNITY_EDITOR
    private void Update()
    {
        //remove if it slows things down too much
        var allChildren2 = this.GetComponentsInChildren<Rock>(true);

        foreach (var child in allChildren2)
        {
            child.IsVisible = child.gameObject.activeSelf;
            TileEditorTool.SetDirty(child);
            TileEditorTool.SetDirty(child.gameObject);
        }
    }
#endif

    [Button(ButtonSizes.Large), GUIColor(1, 1, .3f), HorizontalGroup("RefreshArrayButtons")]
    private void RefreshRocksArray()
    {
        this.RockCluster.RefreshRocksArray();
        //this.reset = false;
        //Validate();
    }

    [Button(ButtonSizes.Large), GUIColor(1, 1, .3f), HorizontalGroup("RefreshArrayButtons")]
    private void RefreshRocksArrayIncludeHidden()
    {
        this.RockCluster.RefreshRocksArrayIncludeHidden();
        //this.reset = false;
        //Validate();
    }

    [PropertySpace(10), Button(ButtonSizes.Large), GUIColor(1, 1, 1)]
    private void ResetRockStyleToClusterDefaults()
    {
        this.RockCluster.UpdateRockStyle(false, true);

        foreach (var rock in this.RockCluster.Rocks)
        {
            rock.RockData = null;
            rock.IsVisible = true;
            TileEditorTool.SetDirty(rock);
        }
    }

    [Title("Create TopShape and MiddleShape so not at runtime")]
    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void Bake()
    {
        this.shouldGenerateChildren = true;

        Validate();
    }

    [Title("Careful!  This re-enables all Rocks in RockCluster")]
    [Button(ButtonSizes.Large), GUIColor(1, 0, 0)]
    private void EnableRocks()
    {
        this.enableAllRocks = true;

        Validate();
    }

    [Title("Removes TopShape and MiddleShape child elements")]
    [Button(ButtonSizes.Large), GUIColor(1, 0, 0)]
    private void Reset()
    {
        this.reset = true;

        Validate();
    }

    [PropertySpace(10), Button(ButtonSizes.Large), HorizontalGroup("ShowHideButtons")]
    private void ShowAllRocks()
    {
        SetAllRocksVisibility(true);
    }

    [PropertySpace(10), Button(ButtonSizes.Large), HorizontalGroup("ShowHideButtons")]
    private void HideAllRocks()
    {
        SetAllRocksVisibility(false);
    }

    private void SetAllRocksVisibility(bool inIsVisible)
    {
        foreach (var rock in this.RockCluster.Rocks)
        {
            rock.gameObject.SetActive(inIsVisible);
            rock.IsVisible = inIsVisible;
            TileEditorTool.SetDirty(rock);
            TileEditorTool.SetDirty(rock.gameObject);
        }

        TileEditorTool.SetDirty(this.RockCluster);
        TileEditorTool.SetDirty(this);
    }

    public RockCluster RockCluster
    {
        get
        {
            if (this.rockCluster == null)
                this.rockCluster = this.GetComponent<RockCluster>();

            return this.rockCluster;
        }
    }
    private RockCluster rockCluster;

    private void Validate()
    {
        if (this.enableAllRocks)
        {
            var allChildren = this.GetComponentsInChildren<Rock>(true);

            Debug.Log($"<color=yellow>Enable all Rocks({allChildren.Length}) for {this.RockCluster.name}</color>");
            this.enableAllRocks = false;

            foreach (var child in allChildren)
            {
                child.gameObject.SetActive(true);
            }
        }

        if (this.reset)
            Debug.Log($"<color=red>RockClusterEditor.OnValidate()   Reset</color>");
        else
            Debug.Log($"<color=yellow>RockClusterEditor.OnValidate()   Rename = {this.shouldRenameRocks}</color>");

        this.RockCluster.Rocks = this.GetComponentsInChildren<Rock>(true);

        if (this.shouldRenameRocks)
        {
            this.shouldRenameRocks = false;

            for (int i = 0; i < this.RockCluster.Rocks.Length; i++)
            {
                var rock = this.RockCluster.Rocks[i];
                rock.name = $"Rock-{i}";
                TileEditorTool.SetDirty(rock);
            }
        }

        #region Reset
        if (this.reset)
        {
            this.reset = false;

            var list = new List<Transform>();

            var allRocks = this.GetComponentsInChildren<Rock>(true);

            foreach (var rock in allRocks)
            {
                if (!rock.Fill)
                {
                    var children = rock.transform.GetComponentsInChildren<Transform>(true);
                    foreach (var child in children)
                    {
                        if (child != rock.transform)
                            list.Add(child);
                    }

                    TileEditorTool.SetDirty(rock);
                    TileEditorTool.SetDirty(rock.gameObject);
                }
            }

            print($"list.Count = {list.Count}   DeleteMe = {DeleteMe}   allRocks.Length = {allRocks.Length}");
            if (list.Count > 0)
            {
                if (DeleteMe == null)
                {
                    DeleteMe = new GameObject("DELETE ME");
                    DeleteMe.SetActive(false);
                }

                foreach (var child in list)
                {
                    child.SetParent(DeleteMe.transform);
                }
            }

            //this.RockCluster.Rocks = new Rock[0];

            return;
        }
        #endregion

        this.RockCluster.UpdateRockStyle(this.shouldGenerateChildren, false);
        this.shouldGenerateChildren = false;

        TileEditorTool.SetDirty(this.RockCluster);
        TileEditorTool.SetDirty(this);
    }
}