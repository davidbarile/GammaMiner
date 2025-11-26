using System.Collections;
using NavMeshPlus.Components;
using UnityEngine;

public class NavMeshManager : MonoBehaviour
{
    public static NavMeshManager IN;

    public NavMeshSurface NavSurface;

    [SerializeField] private NavMeshModifier walkableArea;

    [SerializeField] private bool shouldRefreshNavMesh;

    private bool isFlaggedForNavMeshRebuild;

    private void Awake()
    {
        this.NavSurface.gameObject.SetActive(true);
    }

    public void RebuildNavMesh()
    {
        if (this.isFlaggedForNavMeshRebuild || !this.shouldRefreshNavMesh) return;

        StartCoroutine(DelayedBuildNavMeshCo(0, true));
    }

    public void FlagNavMeshForRebuild()
    {
        if (this.isFlaggedForNavMeshRebuild || !this.shouldRefreshNavMesh) return;

        this.isFlaggedForNavMeshRebuild = true;

        StopAllCoroutines();
        StartCoroutine(DelayedBuildNavMeshCo(10, false));
    }

    private IEnumerator DelayedBuildNavMeshCo(int inFramesDelay, bool inShouldUpdateNavMeshPosition)
    {
        for (int i = 0; i < inFramesDelay; i++)
        {
            yield return null;
        }

        if (inShouldUpdateNavMeshPosition)
            this.walkableArea.transform.position = new Vector3(TileLoadingManager.PlayerPosition.x, TileLoadingManager.PlayerPosition.y, 0);

        yield return null;

        this.NavSurface.UpdateNavMesh(this.NavSurface.navMeshData);

        this.isFlaggedForNavMeshRebuild = false;
    }
}