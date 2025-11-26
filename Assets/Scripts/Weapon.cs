using UnityEngine;

public abstract class Weapon : ShipComponent
{
    public Transform SpawnPoint => this.spawnPoint != null ? this.spawnPoint : this.transform;
    [Header("If null, will use this.transform")]
    [SerializeField] private Transform spawnPoint;

    // public int AmmoCount;
    // public int MaxAmmoCount;
}