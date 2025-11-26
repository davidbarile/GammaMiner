using UnityEngine;
using UnityEngine.AI;

public class MinerDrone : ShipBase
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    

    private void Awake()
    {
        this.navMeshAgent.updateRotation = false;
        this.navMeshAgent.updateUpAxis = false;
    }

    private void Update()
    {
        this.navMeshAgent.destination = SpaceShip.PlayerShip.transform.position;
    }
}