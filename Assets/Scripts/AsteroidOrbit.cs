using UnityEngine;

public class AsteroidOrbit : MonoBehaviour
{
    [SerializeField] private RotateObject rotateObjectY;
    [SerializeField] private RotateObject rotateObjectZ;
    [Space, SerializeField] private Transform orbitRadiusTransform;

    public void SetOrbitRadius(float orbitRadius)
    {
        this.orbitRadiusTransform.localPosition = new Vector3(orbitRadius, 0, 0);
    }

    public void SetOrbitSpeed(float orbitRotationSpeed)
    {
        this.rotateObjectY.SetRotationAmount(0, orbitRotationSpeed, 0);
    }

    public void SetAsteroidRotation(float asteroidRotationSpeed)
    {
        this.rotateObjectZ.SetRotationAmount2D(asteroidRotationSpeed);
    }
}