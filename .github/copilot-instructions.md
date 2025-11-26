# GitHub Copilot Instructions for Gamma Miner

This document provides context and guidelines for GitHub Copilot when working with the Gamma Miner Unity project.

## Project Overview

Gamma Miner is a Unity 2D space mining game featuring modular ships, destructible environments, and hierarchical damage systems. The game uses a component-based architecture with ScriptableObjects for data management.

## Architecture Patterns

### Core Systems

1. **Health System**
   - `HealthEntity` - Base health component with partial damage and visual effects
   - `HealthEntityParent` - Manages child health entities in hierarchical structures
   - `HealthEntityVisualDamage` - Handles visual damage feedback and crack overlays
   - Always use events for health state changes: `OnHealthChanged`, `OnDeath`, `OnDamage`

2. **Ship System**
   - `Ship` - Base ship class with modular component system
   - `PlayerShip` - Player-specific ship implementation
   - `ShipPart` - Base class for modular ship components
   - Ships use hierarchical health with parts as child entities

3. **Weapon System**
   - `Weapon` - Base weapon class extending `ShipPart`
   - `LaserWeapon`, `RailGun`, `MissileLauncher` - Specific weapon implementations
   - Energy management through battery/reactor systems

4. **Data Management**
   - Use ScriptableObjects for configuration: `ShipData`, `WeaponData`, etc.
   - Runtime data in plain C# classes: `ShipRuntimeData`
   - Separate configuration from runtime state

### Managers and Singletons

- `GameManager` - Central game state management
- `LootManager` - Handles loot spawning and collection
- `UIManager` - UI state and transitions
- Use singleton pattern with `.IN` property for manager access

## Coding Conventions

### Unity-Specific

```csharp
// Use SerializeField for inspector-exposed private fields
[SerializeField] private float maxHealth = 100f;

// Use RequireComponent for dependencies
[RequireComponent(typeof(SpriteRenderer))]
public class HealthEntityVisualDamage : MonoBehaviour

// Cache component references in Awake/Start
private SpriteRenderer spriteRenderer;
private void Awake() {
    spriteRenderer = GetComponent<SpriteRenderer>();
}

// Use events for loose coupling
public UnityEvent<float> OnHealthChanged;
public UnityEvent OnDeath;
```

### Naming Conventions

- **Classes**: PascalCase (`HealthEntity`, `PlayerShip`)
- **Methods**: PascalCase (`TakeDamage`, `OnHealthChanged`)
- **Properties**: PascalCase with "this." prefix (`this.MaxHealth`, `this.CurrentHealth`)
- **Fields**: camelCase with "this." prefix (`this.maxHealth`, `this.currentHealth`)
- **Events**: PascalCase with "On" prefix (`OnDeath`, `OnHealthChanged`)
- **Coroutines**: PascalCase with "Coroutine" suffix (`MoveToPositionCoroutine`)

### Component Patterns

```csharp
// Standard MonoBehaviour structure
public class ExampleComponent : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float configValue = 1f;
    
    [Header("Runtime")]
    [SerializeField] private bool debugMode = false;
    
    private ComponentType cachedComponent;
    
    private void Awake() {
        // Cache components
    }
    
    private void Start() {
        // Initialize state
    }
    
    private void OnEnable() {
        // Subscribe to events
    }
    
    private void OnDisable() {
        // Unsubscribe from events
    }
}
```

## Common Patterns

### Health Entity Setup

```csharp
// Always check for null and validate health values
public void TakeDamage(float damage) {
    if (damage <= 0f) return;
    
    currentHealth = Mathf.Max(0f, currentHealth - damage);
    OnHealthChanged?.Invoke(currentHealth / maxHealth);
    
    if (currentHealth <= 0f) {
        OnDeath?.Invoke();
    }
}
```

### Collider Generation

```csharp
// Use existing Navigation2D infrastructure for sprite-to-polygon conversion
// Leverage NavMeshAssetManager.CreateMeshFromSprite() patterns
// Always validate collider points and ensure clockwise winding
```

### Weapon Implementation

```csharp
// Weapons extend ShipPart and use energy system
public class CustomWeapon : Weapon {
    protected override void Fire() {
        if (!CanFire()) return;
        
        // Consume energy
        ConsumeEnergy(energyCost);
        
        // Create projectile/effect
        // Handle cooldown
        
        base.Fire(); // Call parent for common functionality
    }
}
```

### Data Scriptable Objects

```csharp
[CreateAssetMenu(fileName = "New Ship Data", menuName = "Gamma Miner/Ship Data")]
public class ShipData : ScriptableObject {
    [Header("Basic Stats")]
    public string shipName;
    public float maxHealth;
    public float maxSpeed;
    
    [Header("Components")]
    public WeaponData[] defaultWeapons;
    public float energyCapacity;
}
```

## File Organization

```
Assets/
├── Scripts/
│   ├── Managers/           # Singleton managers
│   ├── Ships/              # Ship-related classes
│   ├── Weapons/            # Weapon implementations
│   ├── Health/             # Health system components
│   ├── UI/                 # User interface scripts
│   ├── Data/               # ScriptableObject definitions
│   └── Editor/             # Editor tools and automation
├── Resources/
│   ├── Ships/              # Ship prefabs
│   ├── Weapons/            # Weapon prefabs
│   └── Data/               # ScriptableObject assets
└── Prefabs/                # General prefabs
```

## Performance Considerations

- Cache component references in `Awake()` or `Start()`
- Use object pooling for frequently spawned objects (projectiles, particles)
- Minimize `GetComponent()` calls in `Update()` methods
- Use events instead of polling for state changes
- Prefer `TryGetComponent()` over null checks

## Testing Patterns

```csharp
// Use conditional compilation for debug features
#if UNITY_EDITOR
[Header("Debug")]
[SerializeField] private bool debugVisualization = false;

private void OnDrawGizmos() {
    if (!debugVisualization) return;
    // Draw debug information
}
#endif
```

## Common Gotchas

1. **Health Entity Hierarchy** - Always ensure parent-child relationships are properly configured
2. **Collider Scaling** - Account for parent scale when setting collider sizes
3. **Event Cleanup** - Unsubscribe from events in `OnDisable()` to prevent memory leaks
4. **Null Checks** - Always validate components and references, especially in event handlers
5. **Prefab Overrides** - Be careful when modifying nested prefabs to avoid breaking references

## Integration Points

- **Health System**: Use `OnDeath` events for loot spawning and cleanup
- **Weapon System**: Integrate with energy management and mount points
- **UI System**: Connect health bars and status indicators through events
- **Navigation2D**: Leverage existing sprite-to-mesh conversion for colliders

## Editor Tools

When creating editor tools:
- Extend `Editor` or `EditorWindow` classes
- Use `[MenuItem]` for menu integration
- Implement undo/redo with `Undo.RecordObject()`
- Validate selections before processing
- Provide clear progress feedback for batch operations

This document should be updated as the project evolves and new patterns emerge.