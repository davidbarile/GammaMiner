using UnityEngine;

public class Mount : ShipComponent
{
    public ShipComponent[] ShipComponents;

    [SerializeField] private GameObject emptyMount;//TODO: maybe an array, including destroyed states.  Or maybe an animator.
}