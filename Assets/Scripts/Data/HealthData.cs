using System;
using Sirenix.OdinInspector;

[Serializable]
public class HealthData
{
    public enum EHealthDataType
    {
        Hull,
        Nose,
        Wing,
        Tail,
        Thruster,
        Armor,
        Turret,
        Railgun,
        MissileLauncher,
        LaserCannon,
        ShieldGenerator,
        Reactor,
        Battery,
        Vault,
        Other
    }

    public EHealthDataType Type;
    public int Health;

    public int MaxHealth { get; set; }
    
    public bool IsEnabled { get; set; } = true;//this is for buying new parts or repairs, to know to set Health to MaxHealth - maybe not needed
    //AI ideas - LOL
    // public bool IsRepairable => this.Health < this.MaxHealth;
    // public bool IsUpgradeable => this.Health < this.MaxHealth && this.Type != EHealthDataType.Hull;

    public bool IsDead => this.Health <= 0;
}