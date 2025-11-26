using static CrystalData;

public class UiDebugPanel : UIPanelBase
{
    public static UiDebugPanel IN;
    public static bool HasInfiniteEnergy;

    private bool shouldReviveAllRocksOnHide;

    public override void Hide()
    {
        base.Hide();

        if (this.shouldReviveAllRocksOnHide)
            TileLoadingManager.IN.DelayedReviveAllRocks();

        this.shouldReviveAllRocksOnHide = false;
    }

    public void HandleClearCurrentMapProgressButtonPress()
    {
        UIConfirmPanel.IN.Show("Delete Data?", $"All map progress data will be deleted and game will close.\nAre you sure?", () =>
        {
            GameManager.IN.DeleteCurrentLevelProgress();

            GameManager.QuitGame();
        });
    }

    public void HandleClearAllMapProgressButtonPress()
    {
        UIConfirmPanel.IN.Show("Delete Data?", $"Current map progress data will be deleted and game will close.\nAre you sure?", () =>
        {
            GameManager.IN.DeleteAllLevelProgress();
            GameManager.QuitGame();
        });
    }

    public void HandleReviveAllRocksButtonPress()
    {
        this.shouldReviveAllRocksOnHide = true;
    }

    public void AddCredits(int inAmount)
    {
        PlayerData.Data.AddCredits(inAmount);
    }

    public void SetInfiniteEnergy()
    {
        HasInfiniteEnergy = true;
    }

    public void AddGreenCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.AddCrystals(inAmount, ECrystalType.Crystal1);
    }

    public void AddBlueCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.AddCrystals(inAmount, ECrystalType.Crystal2);
    }

    public void AddRedCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.AddCrystals(inAmount, ECrystalType.Crystal3);
    }

    public void AddPurpleCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.AddCrystals(inAmount, ECrystalType.Crystal4);
    }
    
     public void AddBlackCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.AddCrystals(inAmount, ECrystalType.Crystal5);
    }

    public void RemoveGreenCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.RemoveCrystalsOfType(ECrystalType.Crystal4, inAmount, out _, out _);
    }

    public void RemoveBlueCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.RemoveCrystalsOfType(ECrystalType.Crystal5, inAmount, out _, out _);
    }

    public void RemoveRedCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.RemoveCrystalsOfType(ECrystalType.Crystal1, inAmount, out _, out _);
    }

    public void RemovePurpleCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.RemoveCrystalsOfType(ECrystalType.Crystal6, inAmount, out _, out _);
    }

    public void RemoveBlackCrystals(int inAmount)
    {
        SpaceShip.PlayerShip.RemoveCrystalsOfType(ECrystalType.Crystal7, inAmount, out _, out _);
    }
}