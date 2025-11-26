using System;
using UnityEngine;

public class Railgun : Weapon
{
    public DateTime LastShotTime;
    private RailGunData railGunData;

    public void Init(RailGunData inData)
    {
        this.railGunData = inData;
    }

    public void Shoot(Vector2 inShipVelocity)
    {
        this.LastShotTime = DateTime.UtcNow;
        var railRound = Pool.Spawn<RailRound>(this.railGunData.PrefabName, GameManager.IN.ProjectilesContainer, this.SpawnPoint.position, this.transform.rotation);
        railRound.Shoot(1, 3, inShipVelocity);
    }
}