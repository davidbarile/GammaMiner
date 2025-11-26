using UnityEngine;

//[RequireComponent(typeof(HealthEntity))]
public abstract class ShipComponent : MonoBehaviour
{
    //TODO: maybe delete, and have items be dead based on Hierarchy
    public HealthEntity HealthEntity => this.GetComponent<HealthEntity>();
}