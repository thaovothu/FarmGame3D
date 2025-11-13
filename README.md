# 🌾 Farm Game – Wolffun Game Developer Test

A farm simulation game built with **Unity C#** demonstrating **Clean Architecture** principles.

**🎯 Goal:** Manage a farm, grow crops, raise animals, and earn **1,000,000 gold** 🏆

---

## 🎮 Quick Start

1. Open the project in **Unity Hub** (Unity 2021.3+).
2. Load the scene: `Assets/Scenes/SampleScene.unity`.
3. Press **Play** ▶️
4. Use the console commands to interact with the farm.

---

### 🧑‍🌾 Starting Resources
- 3 plots of land  
- 10 tomato seeds  
- 10 blueberry seeds  
- 2 dairy cows  
- 1 worker  
- 100 gold  
- Level 1 equipment

---

## 🧠 Gameplay Overview

### 🌱 Crops
| Crop | Growth Time | Lifespan | Sell Price | Seed Cost |
|------|--------------|-----------|-------------|------------|
| 🍅 Tomato | 10 min | 40 harvests | 5 gold | 30 gold |
| 🫐 Blueberry | 15 min | 40 harvests | 8 gold | 50 gold |
| 🍓 Strawberry | 5 min | 20 harvests | 3 gold | 40 gold (bulk) |

### 🐄 Animals
| Animal | Production Time | Lifespan | Product Price | Buy Cost |
|---------|----------------|-----------|----------------|-----------|
| 🐄 Dairy Cow | 30 min | 100 productions | 15 gold | 100 gold |

### ⚙️ Game Systems
- **Workers:** Automate tasks (2 min each), hire for 500 gold.  
- **Equipment:** Upgrade for +10% yield (cost 500 gold).  
- **Offline Progress:** Farm continues to run when game is closed.  
- **Spoilage:** Harvest within 60 min after final production or lose crops/animals.

---
## 🏗 Architecture

This project follows **Clean Architecture** with clear separation of concerns:

| Layer | Description |
|-------|--------------|
| **Domain** | Core game logic (Entities, Services, Repositories) – pure C#, no Unity dependencies |
| **Services** | Business operations (FarmService, ShopService, WorkerService) |
| **Infrastructure** | Handles saving/loading, CSV parsing, time management |
| **UI** | Unity MonoBehaviour scripts (GameController, UIManager, ConsoleUI) |
| **Config** | Editable `game_config.csv` for balancing |
| **Tests** | 29+ unit tests for domain logic (no Unity required) |

**Key Features:**
- ✅ Domain independence  
- ✅ Config-driven balancing  
- ✅ Offline progress calculation  
- ✅ Worker automation  
- ✅ Comprehensive unit tests  

---

## 🧪 Testing

Use **Unity Test Runner**:  
**Window → General → Test Runner → Run All**

Covers:
- Plant growth and harvest  
- Animal production  
- Inventory management  
- Farm services and offline progress  

✅ All tests run without Unity dependencies.

---
\