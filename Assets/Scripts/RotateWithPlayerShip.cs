using UnityEngine;

public class RotateWithPlayerShip : MonoBehaviour
{
    private void Update()
    {
        if (SpaceShip.PlayerShip == null) return;

        this.transform.rotation = SpaceShip.PlayerShip.transform.rotation;
    }
}
