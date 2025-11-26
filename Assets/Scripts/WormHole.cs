using System.Collections;
using UnityEngine;

public class WormHole : MonoBehaviour
{
    [SerializeField] private Transform innerParent;

    [SerializeField] private float distanceToTrigger = .2f;

    [SerializeField] private PointEffector2D pointEffector;

    public bool IsActivated { get; private set; }

    private void Start()
    {
        if (MiniMap.IN != null)
            MiniMap.IN.RegisterWormHole(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (this.IsActivated) return;

        this.transform.SetParent(TileLoadingManager.IN.WormHoleContainer);

        var ship = collision.gameObject.GetComponentInParent<SpaceShip>();

        if (SpaceShip.PlayerShip.Equals(ship))
        {
            var distance = Vector3.Distance(SpaceShip.PlayerShip.transform.position, this.transform.position);
            if (distance < this.distanceToTrigger)
            {
                this.IsActivated = true;

                StartCoroutine(EnterWormHoleCo());
            }
        }
    }

    private IEnumerator EnterWormHoleCo()
    {
        var origParent = SpaceShip.PlayerShip.transform.parent;

        SpaceShip.PlayerShip.transform.SetParent(this.innerParent);

        var localPos = SpaceShip.PlayerShip.transform.localPosition;

        this.pointEffector.enabled = false;

        yield return null;

        var scale = 1f;
        var rotAmount = 1f;

        while (scale > .05f)
        {
            scale *= .98f;

            this.transform.localScale = new Vector3(scale, scale, 0);
            this.transform.Rotate(0, 0, rotAmount);
            rotAmount *= 1.01f;

            SpaceShip.PlayerShip.transform.localPosition = localPos;
            yield return null;
        }

        MapManager.IN.LoadMapNum(PlayerData.Data.CurrentLevelIndex + 1);//TODO: make this go to a specific level saved in data

        yield return null;

        while (scale < 1)
        {
            scale *= 1.02f;

            this.transform.localScale = new Vector3(scale, scale, 0);
            this.transform.Rotate(0, 0, rotAmount);

            SpaceShip.PlayerShip.transform.localPosition = localPos;
            yield return null;
        }

        SpaceShip.PlayerShip.transform.SetParent(origParent);
        SpaceShip.PlayerShip.transform.localScale = Vector3.one;
        SpaceShip.PlayerShip.ResetRigidbody();

        yield return null;

        this.transform.localScale = Vector3.one;
        this.transform.rotation = Quaternion.identity;

        //this.pointEffector.forceMagnitude = 99;
        //this.pointEffector.enabled = true;

        scale = 1f;
   
        while (scale > .01f)
        {
            scale *= .95f;

            this.transform.localScale = new Vector3(scale, scale, 0);
            this.transform.Rotate(0, 0, rotAmount);
            rotAmount *= 1.05f;

            yield return null;
        }

        //this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        if (MiniMap.IN != null)
            MiniMap.IN.RemoveWormHole(this);
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (!this.isInside) return;

    //    var ship = collision.gameObject.GetComponentInParent<SpaceShip>();

    //    if (SpaceShip.PlayerShip.Equals(ship))
    //    {
    //        this.isInside = false;
    //    }
    //}
}