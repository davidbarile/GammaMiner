using UnityEngine;

public class SpaceStation : MonoBehaviour
{
    [SerializeField] private ShopConfig shopConfig;
    [SerializeField] private CrystalTradePricesConfig crystalTradePricesConfig;

    [SerializeField] private Collider2D showShopTrigger;
    [SerializeField] private Collider2D reactivateShopTrigger;

    private SpaceShip playerShip = null;
    private int entryFrame = -1;

    private void Start()
    {
        this.showShopTrigger.enabled = true;
        this.reactivateShopTrigger.enabled = false;

        if (MiniMap.IN != null)
            MiniMap.IN.RegisterSpaceStation(this);
    }

    private void OnDestroy()
    {
        if (MiniMap.IN != null)
            MiniMap.IN.RemoveSpaceStation(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //print($"OnCollisionEnter2D({collision.gameObject.name})");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.showShopTrigger.enabled) return;

        if(!collision.gameObject.TryGetComponent<SpaceShip>(out var ship))
            ship = collision.gameObject.GetComponentInParent<SpaceShip>();

        if (ship != null && ship.IsPlayer && this.playerShip == null)
        {
            this.entryFrame = Time.frameCount;

            this.playerShip = ship;

            this.showShopTrigger.enabled = false;
            this.reactivateShopTrigger.enabled = true;

            UIShopPanel.IN.Show(this.shopConfig);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!this.reactivateShopTrigger.enabled) return;
        if(Time.frameCount == this.entryFrame) return;

        if (!collision.gameObject.TryGetComponent<SpaceShip>(out var ship))
            ship = collision.gameObject.GetComponentInParent<SpaceShip>();

        if(ship != null && ship.Equals(this.playerShip))
        {
            this.playerShip = null;

            this.showShopTrigger.enabled = true;
            this.reactivateShopTrigger.enabled = false;
        }
    }
}
