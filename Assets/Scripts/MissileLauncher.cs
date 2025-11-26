using UnityEngine;

public class MissileLauncher : Weapon
{
    public bool IsActive => this.isActive;
    [SerializeField] private bool isActive = true;
    [Range(0, 5),SerializeField] private float delayToAccelerate;
    public  MissileLauncherData MissileLauncherData { get; private set; }
    public void Init(MissileLauncherData inData)
    {
        this.MissileLauncherData = inData;
    }
    public bool Shoot(Vector2 inShipVelocity)
    {
        if (!this.isActive)
            return false;
            
        if (this.MissileLauncherData == null)
        {
            Debug.Log($"<color=red>MissileLauncher.Shoot()  this.missileLauncherData is NULL  {this.name}</color>");
            return false;
        }

        if (this.MissileLauncherData.NumMissiles <= 0)
        {
            Debug.Log($"<color=yellow>MissileLauncher.Shoot()   NumMissiles = {this.MissileLauncherData.NumMissiles}    {this.name}</color>");
            return false;
        }

        var missile = Pool.Spawn<Missile>(this.MissileLauncherData.PrefabName, GameManager.IN.ProjectilesContainer, this.SpawnPoint.position, this.SpawnPoint.rotation);

        var directionPoint = this.SpawnPoint.transform;

        if (this.SpawnPoint.childCount > 0)
            directionPoint = this.SpawnPoint.GetChild(0);

        Vector2 launchForce = Vector2.zero; //this.SpawnPoint.transform.up;

        var delay = 0f;

        if (directionPoint != this.SpawnPoint.transform)
        {
            delay = this.delayToAccelerate;
            launchForce = directionPoint.position - this.SpawnPoint.position;
        }

        missile.Shoot(launchForce, inShipVelocity, delay);

        --this.MissileLauncherData.NumMissiles;

        return true;
    }
}