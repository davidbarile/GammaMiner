using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    public static MiniMap IN;

    public Transform SensorTarget;
    public ESensorType SensorType;
    public float SensorRange = 1;

    [ReadOnly]
    public List<SpaceStation> SpaceStationsInLevel = new();
    public List<WormHole> WormHolesInLevel = new();

    [Header("Rotate Objects")]
    [SerializeField] private Transform arrowTrans;
    [SerializeField] private Transform gradientTrans;

    [Header("Color")]
    [SerializeField] private Graphic arrow;
    [SerializeField] private Gradient rangeGradient;
    [SerializeField] private RectTransform gradientRectTrans;
    [SerializeField] private float closeGradientDistance;
    [SerializeField] private float farGradientDistance;

    [Header("Show/Hide")]
    [SerializeField] private GameObject[] sensorElements;

    [SerializeField] private float offset;//0-180

    [Header("Sensor Button")]
    [SerializeField] private GameObject[] sensorModeIcons;

    private bool targetExists => this.SensorTarget != null;
    private bool isInRange;

    //cached vars used in Update
    private Vector3 difference;
    private Quaternion rot;
    private float normalizedDistance;
    private float yPos;
    private float angle;

    public enum ESensorType
    {
        None,
        SpaceStation,
        Wormhole,
        Enemy,
        Crystal_Green,
        Crystal_Blue,
        Crystal_Red,
        Count
    }

    private void Start()
    {
        RefreshSensorType();
    }

    public void SetVisible(bool inIsVisible)
    {
        this.gameObject.SetActive(inIsVisible);
    }

    /// <summary>
    /// When player does or doesn't have sensor
    /// </summary>
    public void SetSensorVisibile(bool inIsVisible)
    {
        foreach (var element in this.sensorElements)
        {
            element.SetActive(inIsVisible);
        }
    }

    /// <summary>
    /// When player has not set a target
    /// </summary>
    public void SetSensorActive(bool inIsActive)
    {
        this.arrowTrans.gameObject.SetActive(inIsActive);
        this.gradientTrans.gameObject.SetActive(inIsActive);
    }

    private void Update()
    {
        RefreshNearestTarget();

        if (this.targetExists && SpaceShip.PlayerShip != null)
        {
            this.difference = SpaceShip.PlayerShip.transform.position - this.SensorTarget.position;
            this.angle = Mathf.Atan2(this.difference.y, this.difference.x) * Mathf.Rad2Deg;

            this.rot = Quaternion.Euler(0, 0, this.angle + this.offset);
            this.arrowTrans.rotation = this.rot;
            this.gradientTrans.rotation = this.rot;

            this.normalizedDistance = GetDistanceToTarget();
            this.arrow.color = this.rangeGradient.Evaluate(this.normalizedDistance);

            this.yPos = Mathf.Lerp(this.closeGradientDistance, this.farGradientDistance, this.normalizedDistance);
            this.gradientRectTrans.anchoredPosition = new Vector2(0, this.yPos);

            SetSensorActive(this.isInRange);
        }
        else
            SetSensorActive(false);
    }

    private float GetDistanceToTarget()
    {
        var distance = Vector2.Distance(SpaceShip.PlayerShip.transform.position, this.SensorTarget.position);
        this.isInRange = distance <= this.SensorRange;
        return Mathf.Clamp01(distance / this.SensorRange);
    }

    private void RefreshNearestTarget()
    {
        switch (this.SensorType)
        {
            case ESensorType.None:
                this.SensorTarget = null;
                break;
            case ESensorType.SpaceStation:
                RefreshNearestSpaceStation();
                break;
            case ESensorType.Wormhole:
                RefreshNearestWormHole();
                break;
        }
    }

    public void RegisterSpaceStation(SpaceStation inSpaceStation)
    {
        this.SpaceStationsInLevel.Add(inSpaceStation);
    }

    public void RemoveSpaceStation(SpaceStation inSpaceStation)
    {
        if (this.SpaceStationsInLevel.Contains(inSpaceStation))
        {
            this.SpaceStationsInLevel.Remove(inSpaceStation);
        }
    }

    private void RefreshNearestSpaceStation()
    {
        if (this.SensorType != ESensorType.SpaceStation) return;

        SpaceStation nearestStation = null;
        var nearestDistance = 99999f;

        foreach (var spaceStation in this.SpaceStationsInLevel)
        {
            if (spaceStation != null)
            {
                var newDistance = Vector2.Distance(SpaceShip.PlayerShip.transform.position, spaceStation.transform.position);

                if (newDistance < nearestDistance)
                {
                    nearestDistance = newDistance;
                    nearestStation = spaceStation;
                }
            }
        }

        if (nearestStation != null)
            this.SensorTarget = nearestStation.transform;
        else
            this.SensorTarget = null;
    }

    public void RegisterWormHole(WormHole inWormHole)
    {
        this.WormHolesInLevel.Add(inWormHole);
    }

    public void RemoveWormHole(WormHole inWormHole)
    {
        if (this.WormHolesInLevel.Contains(inWormHole))
        {
            this.WormHolesInLevel.Remove(inWormHole);
        }
    }

    private void RefreshNearestWormHole()
    {
        if (this.SensorType != ESensorType.Wormhole) return;

        if (SpaceShip.PlayerShip == null) return;

        WormHole nearestWormHole = null;
        var nearestDistance = 99999f;

        foreach (var wormHole in this.WormHolesInLevel)
        {
            if (wormHole != null)
            {
                var newDistance = Vector2.Distance(SpaceShip.PlayerShip.transform.position, wormHole.transform.position);

                if (newDistance < nearestDistance)
                {
                    nearestDistance = newDistance;
                    nearestWormHole = wormHole;
                }
            }
        }

        if (nearestWormHole != null)
            this.SensorTarget = nearestWormHole.transform;
        else
            this.SensorTarget = null;
    }

    private void RefreshSensorType()
    {
        var iconIndex = (int)this.SensorType;

        for (int i = 0; i < this.sensorModeIcons.Length; i++)
        {
            var shouldShow = i == iconIndex;

            var icon = this.sensorModeIcons[i];

            icon.SetActive(shouldShow);
        }
    }

    public void HandleSensorTypeButtonPress()
    {
        var iconIndex = (int)this.SensorType;

        ++iconIndex;

        iconIndex %= this.sensorModeIcons.Length;

        this.SensorType = (ESensorType)iconIndex;

        RefreshSensorType();
    }
}
