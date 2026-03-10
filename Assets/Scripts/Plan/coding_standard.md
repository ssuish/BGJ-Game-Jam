# Unity Coding Standards

## File Organization

### Using Statements

- Place all `using` statements at the top of the file
- Order: System namespaces first, then Unity namespaces, then third-party
- Remove unused using statements

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;
```

### Class Structure Order

1. Static members and properties
2. Serialized fields with [Header] attributes
3. Public properties
4. Public events
5. Private fields
6. Unity lifecycle methods (Awake, Start, OnEnable, OnDisable, Update, FixedUpdate, etc.)
7. Public methods
8. Protected virtual methods
9. Private methods

## Naming Conventions

### Classes and Structs

- **PascalCase**: `SafetyMeterManager`, `BaseCreatureController`
- Descriptive names that clearly indicate purpose
- MonoBehaviour scripts should end with descriptive suffix when appropriate (Manager, Controller, Handler, etc.)

### Fields

- **Private fields**: camelCase - `playerInput`, `moveSpeed`, `isHuddling`
- **Public fields**: PascalCase - avoid unless necessary
- **Serialized fields**: camelCase with `[SerializeField]` attribute
- **Constants**: PascalCase or UPPER_SNAKE_CASE - `MaxValue` or `MAX_VALUE`
- **Static fields**: PascalCase - `Instance`

### Properties

- **PascalCase**: `SafetyMeterValue`, `IsHuddling`, `DistanceBetweenPlayers`
- Use auto-properties with `{ get; private set; }` for read-only public access
- Expression-bodied properties for computed values: `SafetyMeterPercent => ...`

### Methods

- **PascalCase**: `UpdateMovement`, `InitializeCharacters`, `SetPlayers`
- **Event handlers**: Prefix with `On` - `OnEnable`, `OnCheckInStateChanged`
- Descriptive verb-based names

### Events

- **PascalCase** with `On` prefix: `OnSafetyMeterValueChanged`, `OnHuddlingStateChanged`
- Use `System.Action` or `System.Action<T>` delegates
- Declare using `event` keyword

### Parameters and Local Variables

- **camelCase**: `moveInput`, `sparkTransform`, `previousValue`

## Code Style

### Comments

- **Do not include comments in code** - write self-documenting code with clear naming
- Exception: complex algorithms requiring explanation or critical warnings

### Attributes

- Use `[Header("Section Name")]` to organize Inspector fields
- Use `[SerializeField]` for private fields that need Inspector visibility
- Use `[RequireComponent(typeof(ComponentType))]` for component dependencies
- Place attributes on the line above the declaration

```csharp
[Header("Player References")]
[SerializeField] private Transform spark;
[SerializeField] private Transform bulk;
```

### Properties and Auto-Properties

- Use auto-properties for simple getters/setters
- Use private setters for read-only public access
- Use expression-bodied members for computed values

```csharp
public float SafetyMeterValue { get; private set; } = 100f;
public float SafetyMeterPercent => maxSafetyMeter <= 0f ? 0f : SafetyMeterValue / maxSafetyMeter;
public bool IsHuddling { get; private set; }
```

### Events

- Use `Action<T>` for events with parameters
- Use null-conditional operator when invoking: `OnEventName?.Invoke(value);`

```csharp
public event Action<float> OnSafetyMeterValueChanged;
public event Action<bool> OnHuddlingStateChanged;

OnSafetyMeterValueChanged?.Invoke(SafetyMeterValue);
```

### Singleton Pattern

- Use static `Instance` property with private setter
- Initialize in `Awake()`
- Include destroy logic for duplicate instances
- Consider `DontDestroyOnLoad` based on requirements

```csharp
public static SafetyMeterManager Instance { get; private set; }

private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

### Null Checking

- Use null-conditional operator: `?.`
- Check references before use, especially serialized fields
- Early return pattern for invalid states

```csharp
if (spark == null || bulk == null)
{
    return;
}
```

### Method Structure

- Keep methods focused on single responsibility
- Extract complex logic into separate methods
- Use descriptive method names instead of comments

```csharp
private void Update()
{
    if (spark == null || bulk == null)
    {
        return;
    }

    UpdateDistance();
    UpdateHuddleState();
    UpdateSafetyMeter();
}
```

## Unity-Specific Guidelines

### MonoBehaviour Lifecycle

- **Awake**: Initialize internal references, singleton setup
- **Start**: Initialize after all Awake calls complete
- **OnEnable/OnDisable**: Subscribe/unsubscribe from events, enable/disable input
- **Update**: Per-frame logic, input handling, non-physics updates
- **FixedUpdate**: Physics-based updates (use `Time.fixedDeltaTime`)
- **LateUpdate**: Camera following, post-processing

### Time Management

- Use `Time.deltaTime` for frame-rate independent updates in `Update()`
- Use `Time.fixedDeltaTime` for physics updates in `FixedUpdate()`
- Never hardcode time values without delta time

```csharp
SafetyMeterValue -= drainRatePerSecond * Time.deltaTime;
```

### Component Access

- Cache component references in `Awake()` or `Start()`
- Use `GetComponent<T>()` sparingly, never in `Update()`
- Use `[RequireComponent]` for mandatory components

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class BaseCreatureController : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
```

### Input System

- Cache `InputAction` references
- Enable/disable in `OnEnable()`/`OnDisable()`
- Use `ReadValue<T>()` for continuous input
- Use `IsPressed()` for button states

```csharp
private void OnEnable()
{
    moveAction?.Enable();
}

private void OnDisable()
{
    moveAction?.Disable();
}
```

### Serialized Fields vs Public Fields

- Prefer `[SerializeField] private` over `public` for Inspector fields
- Only use `public` fields when they need external script access
- Use properties for public read access with private modification

### Virtual Methods

- Use `protected virtual` for methods intended to be overridden
- Always call `base.MethodName()` in overrides when extending functionality

```csharp
protected virtual void InitializeCharacters()
{

}

// In derived class
protected override void InitializeCharacters()
{
    base.InitializeCharacters();
    moveSpeed = 6f;
}
```

### Value Clamping and Validation

- Use `Mathf.Clamp()` to constrain values
- Use `Mathf.Approximately()` for float comparisons
- Validate input parameters

```csharp
SafetyMeterValue = Mathf.Clamp(SafetyMeterValue, 0f, maxSafetyMeter);

if (!Mathf.Approximately(previousValue, SafetyMeterValue))
{
    OnSafetyMeterValueChanged?.Invoke(SafetyMeterValue);
}
```

## Performance Considerations

- Avoid `GameObject.Find()` in loops or frequent updates
- Cache Transform and component references
- Use object pooling for frequently instantiated objects
- Minimize allocations in `Update()` and `FixedUpdate()`
- Use struct for small data containers when appropriate
- Profile before optimizing

## Pattern Examples

### State Change Detection with Events

```csharp
private void UpdateHuddleState()
{
    bool previousHuddlingState = IsHuddling;
    IsHuddling = DistanceBetweenPlayers < huddleDistanceThreshold;

    if (previousHuddlingState != IsHuddling)
    {
        OnHuddlingStateChanged?.Invoke(IsHuddling);
    }
}
```

### Configurable Rates and Thresholds

```csharp
[Header("Configuration")]
[SerializeField] private float threshold = 5f;
[SerializeField] private float ratePerSecond = 10f;
```

### Runtime Assignment Methods

```csharp
public void SetPlayers(Transform sparkTransform, Transform bulkTransform)
{
    spark = sparkTransform;
    bulk = bulkTransform;
}
```

## File and Folder Structure

- Scripts organized by function: `/Controller`, `/Manager`, `/UI`, `/Utilities`
- One class per file
- File name matches class name exactly
- Keep related classes in the same folder

## General Principles

1. **Self-documenting code**: Clear naming over comments
2. **Single Responsibility**: Each class/method does one thing well
3. **Encapsulation**: Private by default, expose only what's necessary
4. **Consistency**: Follow established patterns throughout the project
5. **Readability**: Write code for humans to read, not just machines
6. **Unity conventions**: Follow Unity's recommended practices and lifecycle
