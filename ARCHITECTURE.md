# 🏗 Architecture Documentation

## Overview

This Farm Game follows **Clean Architecture** (also known as Hexagonal Architecture or Ports & Adapters) to achieve:
- **Testability**: Domain logic can be tested without Unity
- **Maintainability**: Clear separation of concerns
- **Extensibility**: Easy to add new features
- **Flexibility**: Can replace UI or infrastructure without touching core logic

---

## Layer Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Unity MonoBehaviour Classes                             │  │
│  │  - GameController.cs (orchestrates game flow)            │  │
│  │  - UIManager.cs (displays data to player)               │  │
│  │  - ConsoleUI.cs (alternative console interface)         │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────┬───────────────────────────────────────┘
                         │ calls
┌────────────────────────▼───────────────────────────────────────┐
│                      Application Layer                          │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Domain Services (Orchestrate business operations)       │  │
│  │  - FarmService.cs (farm operations)                      │  │
│  │  - ShopService.cs (buy/sell logic)                       │  │
│  │  - WorkerService.cs (automation logic)                   │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────┬───────────────────────────────────────┘
                         │ uses
┌────────────────────────▼───────────────────────────────────────┐
│                         Domain Layer                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Entities (Pure Business Logic - NO Unity!)              │  │
│  │  - Plant.cs (crop lifecycle, growth, harvest)            │  │
│  │  - Animal.cs (animal lifecycle, production)              │  │
│  │  - Worker.cs (worker state machine)                      │  │
│  │  - Plot.cs (land management)                             │  │
│  │  - Farm.cs (aggregate root)                              │  │
│  │  - Inventory.cs (resource management)                    │  │
│  │  - Task.cs (worker task)                                 │  │
│  │                                                            │  │
│  │  Config (Data structures)                                 │  │
│  │  - GameConfig.cs (configuration data models)             │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────▲───────────────────────────────────────┘
                         │ used by
┌────────────────────────┴───────────────────────────────────────┐
│                     Infrastructure Layer                        │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  External Concerns                                         │  │
│  │  - ConfigLoader.cs (CSV file reading)                     │  │
│  │  - SaveSystem.cs (JSON serialization, file I/O)          │  │
│  │  - TimeService.cs (offline progress calculation)         │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
```

---

## Dependency Rules

### The Dependency Rule
**Source code dependencies must point inward**. Nothing in an inner circle can know anything about something in an outer circle.

1. **Domain Layer** depends on nothing
2. **Application Layer** depends on Domain
3. **Infrastructure Layer** depends on Domain
4. **Presentation Layer** depends on Application and Infrastructure

### Key Points
- ✅ Domain entities have **ZERO Unity dependencies**
- ✅ Domain services depend only on entities
- ✅ UI layer can be completely replaced
- ✅ Infrastructure can be swapped (file system, database, cloud)

---

## Components Breakdown

### 1. Domain Layer (Core Business Logic)

#### Entities
**Pure C# classes representing business concepts**

```csharp
// Example: Plant.cs
public class Plant
{
    public string Id { get; set; }
    public CropType CropType { get; set; }
    public DateTime PlantedTime { get; set; }
    public bool IsAlive { get; set; }
    
    // Business logic methods
    public int GetReadyHarvestCount(DateTime currentTime, float equipmentBonus)
    {
        // Pure C# logic - no Unity!
        // Fully testable
    }
}
```

**Characteristics:**
- No dependencies on frameworks
- Contains business rules
- Fully unit testable
- High cohesion

#### Value Objects
- `CropType`, `AnimalType` enums
- Configuration structs

---

### 2. Application Layer (Use Cases)

#### Services
**Orchestrate domain entities to perform use cases**

```csharp
// Example: FarmService.cs
public class FarmService
{
    private readonly GameConfig _config;
    
    public FarmService(GameConfig config)
    {
        _config = config;  // Dependency injection
    }
    
    public bool PlantCrop(Farm farm, string plotId, CropType cropType, DateTime currentTime)
    {
        // Orchestrates multiple entities
        var plot = farm.GetPlotById(plotId);
        if (!plot.IsEmpty()) return false;
        
        if (!farm.Inventory.UseSeed(cropType)) return false;
        
        var plant = CreatePlantFromConfig(cropType, currentTime);
        plot.PlantCrop(plant);
        
        return true;
    }
}
```

**Characteristics:**
- Coordinates domain entities
- Implements use cases
- Stateless (receives data as parameters)
- Depends only on domain entities

---

### 3. Infrastructure Layer (Technical Concerns)

#### ConfigLoader
**Reads CSV and converts to domain objects**

```csharp
public class ConfigLoader
{
    public static GameConfig LoadConfig(string filePath)
    {
        // File I/O - kept out of domain
        var lines = File.ReadAllLines(filePath);
        
        // Parse and create domain objects
        return ParseConfig(lines);
    }
}
```

#### SaveSystem
**Handles persistence**

```csharp
public class SaveSystem
{
    public static void Save(Farm farm)
    {
        // JSON serialization
        // File system access
        // All technical concerns isolated here
    }
}
```

#### TimeService
**Handles offline progress calculation**

```csharp
public class TimeService
{
    public void ProcessOfflineProgress(Farm farm, DateTime currentTime)
    {
        // Calculates what happened while game was closed
        // Uses domain entities, but isolated from UI
    }
}
```

**Characteristics:**
- Handles external systems (files, databases, APIs)
- Converts between domain objects and external formats
- Can be swapped without changing domain

---

### 4. Presentation Layer (User Interface)

#### GameController
**Main orchestrator - bridges UI and application**

```csharp
public class GameController : MonoBehaviour
{
    private FarmService _farmService;
    private Farm _farm;
    
    private void Awake()
    {
        // Initialize services
        _config = ConfigLoader.LoadConfig(...);
        _farmService = new FarmService(_config);
        
        // Load or create farm
        _farm = LoadOrCreateFarm();
    }
    
    // Public methods for UI to call
    public bool PlantCrop(string plotId, CropType cropType)
    {
        return _farmService.PlantCrop(_farm, plotId, cropType, DateTime.Now);
    }
}
```

#### UIManager
**Displays data to player**

```csharp
public class UIManager : MonoBehaviour
{
    private GameController _gameController;
    
    public void UpdateDisplay()
    {
        // Read from domain objects
        var farm = _gameController.Farm;
        
        // Display to UI
        goldText.text = farm.Inventory.Gold.ToString();
    }
}
```

**Characteristics:**
- Only layer that knows about Unity
- Thin layer - mostly display logic
- Can be replaced with different UI (console, web, mobile)

---

## Data Flow Example

### Player Plants a Tomato

```
User clicks "Plant Tomato on Plot 1"
         │
         ▼
    UIManager.OnPlantButton()
         │
         ▼
    GameController.PlantCrop(plotId, CropType.Tomato)
         │
         ▼
    FarmService.PlantCrop(farm, plotId, cropType, currentTime)
         │
         ├──> Check plot is empty (Plot.IsEmpty())
         │
         ├──> Check player has seeds (Inventory.HasSeed())
         │
         ├──> Use seed (Inventory.UseSeed())
         │
         ├──> Create plant from config (new Plant(...))
         │
         └──> Place on plot (Plot.PlantCrop(plant))
         │
         ▼
    Return success/failure
         │
         ▼
    UIManager shows message
```

**Notice:**
- UI only knows about GameController
- GameController only knows about Services
- Services only know about Domain Entities
- Each layer has clear responsibility

---

## Key Design Patterns

### 1. Service Layer Pattern
**Problem**: Where to put business logic that coordinates multiple entities?  
**Solution**: Create service classes (FarmService, ShopService)

### 2. Repository Pattern
**Problem**: How to abstract data access?  
**Solution**: SaveSystem acts as a repository for Farm data

### 3. Dependency Injection
**Problem**: How to avoid tight coupling?  
**Solution**: Services receive dependencies via constructor

```csharp
public class FarmService
{
    private readonly GameConfig _config;
    
    public FarmService(GameConfig config)  // DI via constructor
    {
        _config = config;
    }
}
```

### 4. Strategy Pattern
**Problem**: Different crops have different behaviors  
**Solution**: CropType enum + config-driven behavior

### 5. Command Pattern
**Problem**: Worker tasks need to be queued and executed later  
**Solution**: Task queue with FarmTask objects

---

## Testing Strategy

### Unit Tests (Domain Layer)
```csharp
[Test]
public void Plant_AfterGrowthTime_HasReadyHarvest()
{
    // Arrange
    var plantTime = DateTime.Now.AddMinutes(-15);
    var plant = new Plant(CropType.Tomato, plantTime, 10f, 1, 40, 5);
    
    // Act
    var readyCount = plant.GetReadyHarvestCount(DateTime.Now, 0);
    
    // Assert
    Assert.AreEqual(1, readyCount);
}
```

**No Unity required!** Tests run fast and don't need game engine.

### Integration Tests (Service Layer)
```csharp
[Test]
public void PlantCrop_WithValidPlot_Succeeds()
{
    var config = CreateTestConfig();
    var farmService = new FarmService(config);
    var farm = farmService.InitializeNewFarm();
    
    var success = farmService.PlantCrop(farm, plotId, CropType.Tomato, DateTime.Now);
    
    Assert.IsTrue(success);
}
```

---

## Benefits of This Architecture

### ✅ Testability
- Domain logic testable without Unity
- Fast test execution
- High code coverage possible

### ✅ Maintainability
- Clear separation of concerns
- Easy to locate bugs (which layer?)
- Changes isolated to specific layers

### ✅ Extensibility
- Add new crops: Just add to config + enum
- Add new features: Follow same pattern
- No ripple effects across layers

### ✅ Flexibility
- Replace UI: Just rewrite Presentation layer
- Change save format: Just modify SaveSystem
- Add multiplayer: Add new Infrastructure component

### ✅ Team Collaboration
- Different developers can work on different layers
- Clear interfaces between layers
- Less merge conflicts

---

## Comparison with Traditional Unity Architecture

### ❌ Traditional Approach
```csharp
public class PlantBehaviour : MonoBehaviour
{
    public int harvestCount;
    public float growthTime;
    
    void Update()
    {
        // Growth logic mixed with Unity update loop
        // Hard to test
        // Tightly coupled to Unity
    }
    
    void OnMouseDown()
    {
        // UI logic mixed with game logic
        // Hard to change UI
    }
}
```

**Problems:**
- Game logic tied to Unity
- Hard to unit test
- Difficult to change UI
- Can't calculate offline progress

### ✅ Clean Architecture Approach
```csharp
// Domain (pure C#)
public class Plant
{
    public int GetReadyHarvestCount(DateTime currentTime, float bonus)
    {
        // Pure logic - easily testable
    }
}

// UI (Unity)
public class PlantUI : MonoBehaviour
{
    void OnMouseDown()
    {
        gameController.HarvestPlot(plotId);  // Delegate to controller
    }
}
```

**Benefits:**
- Business logic independent
- Easy to test
- Flexible UI
- Offline progress possible

---

## Future Enhancements

### Easy to Add
- More crop/animal types (just config)
- Fertilizer system (new domain entity)
- Weather system (new service)
- Achievements (new tracking in Farm entity)

### Architectural Improvements
- Add interfaces for services (better DI)
- Event system for loose coupling
- CQRS pattern for complex queries
- State pattern for worker states

---

## Conclusion

This architecture provides:
1. **Clean separation** between game logic and Unity
2. **High testability** through pure C# entities
3. **Easy maintenance** through clear responsibilities
4. **Simple extensibility** through config-driven design
5. **Flexibility** to change UI or infrastructure

**Perfect for game development teams** where game designers need to balance the game without programmer assistance, and where different team members work on different aspects (UI, logic, infrastructure).
