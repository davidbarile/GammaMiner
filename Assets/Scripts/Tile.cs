using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rocks
{
    public class Tile : MonoBehaviour
    {
        public int NumTilesToShowPerFrame = 10;

        public bool ShouldShowTilesGraduallyOnStart = true;
        public bool ShouldRandomizeRocksList = true;
        public bool ShouldMergeTilePresentation = true;

        [SerializeField] private bool shouldShowOnEnable;
        [SerializeField] private bool shouldRegisterTileWhenLoaded;

        public WormHole[] WormHoles { get; private set; } = { };
        public RockCluster[] AllRockClusters { get; private set; } = { };
        public List<Rock> AllRocks { get; private set; } = new();

        public bool IsShowing { get; private set; }

        private void Start()
        {
            if (TileEditorTool.IsEditing) return;
            if (!this.shouldRegisterTileWhenLoaded) return;

            //foreach (var wormHole in this.WormHoles)
            //{
            //    MiniMap.IN.RegisterWormHole(wormHole);
            //}
        }

        public void Init()
        {
            this.AllRockClusters = this.GetComponentsInChildren<RockCluster>(false);

            foreach (var cluster in this.AllRockClusters)
            {
                foreach (var rock in cluster.Rocks)
                {
                    this.AllRocks.Add(rock);
                }
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            var tileEditor = Object.FindAnyObjectByType<TileEditorTool>();
            if (tileEditor == null) return;

            this.WormHoles = Object.FindObjectsByType<WormHole>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            //if (!TileEditorTool.IsEditing) return;

            var scene = SceneManager.GetActiveScene();
            this.name = scene.name;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            if (this.shouldShowOnEnable)
                Show();
        }

        public void Show()
        {
            this.IsShowing = true;
            StartCoroutine(ShowCo());
        }

        private IEnumerator ShowCo()
        {
            var clusters = this.GetComponentsInChildren<RockCluster>(false);

            if (this.ShouldMergeTilePresentation)
            {
                List<Rock> allRocksList = new();

                foreach (var cluster in clusters)
                {
                    foreach (var rock in cluster.Rocks)
                    {
                        allRocksList.Add(rock);
                    }
                }

                if (this.ShouldRandomizeRocksList)
                    allRocksList.RandomizeList();

                var counter = 0;

                foreach (var rock in allRocksList)
                {
                    if (this.ShouldShowTilesGraduallyOnStart)
                    {
                        if (counter % this.NumTilesToShowPerFrame == 0)// && !TileEditorTool.IsEditing)
                            yield return null;
                    }

                    if (rock.HealthEntity != null && !rock.HealthEntity.IsDead)
                        rock.gameObject.SetActive(true);

                    ++counter;
                }
            }
            else
            {
                foreach (var cluster in clusters)
                {
                    var counter = 0;

                    var rocksList = cluster.Rocks.ToList();

                    if (this.ShouldRandomizeRocksList)
                        rocksList.RandomizeList();

                    foreach (var rock in rocksList)
                    {
                        if (this.ShouldShowTilesGraduallyOnStart)
                        {
                            if (counter % this.NumTilesToShowPerFrame == 0)// && !TileEditorTool.IsEditing)
                                yield return null;
                        }

                        rock.gameObject.SetActive(true);

                        ++counter;
                    }
                }
            }
        }

        public void Hide()
        {
            this.IsShowing = false;

            var clusters = this.GetComponentsInChildren<RockCluster>(false);

            foreach (var cluster in clusters)
            {
                foreach (var rock in cluster.Rocks)
                {
                    rock.gameObject.SetActive(false);
                }
            }
        }

        public void OnDestroy()
        {
            foreach (var wormHole in this.WormHoles)
            {
                if (wormHole != null && !wormHole.IsActivated)
                    Destroy(wormHole.gameObject);
            }
        }
    }
}