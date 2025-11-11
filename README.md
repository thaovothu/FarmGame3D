# 🌾 Farm Game – Test Project

A small farm simulation game built with **Unity C#**, designed for the **Game Developer Test**.  
The goal is to manage a farm, grow crops, raise cows, and earn **1,000,000 gold**.

---

## 🎮 Gameplay Overview

- Start with:
  - 3 plots of land
  - 10 tomato seeds
  - 10 blueberry seeds
  - 2 dairy cows
  - 1 worker
  - Level 1 farm equipment

### 🧑‍🌾 Features
- Plant and harvest crops  
- Raise cows and collect milk  
- Upgrade farm equipment (+10% yield each upgrade)  
- Hire workers (automate tasks)  
- Buy more plots, seeds, and animals from the shop  
- Save game state locally (farm runs while offline)

---

## 🧠 System Design

### Core Components
| Layer | Description |
|-------|--------------|
| **Domain** | Core game logic (Entities, Services, Repositories) |
| **Infrastructure** | Handles saving/loading, CSV parsing, local time tracking |
| **UI** | Unity MonoBehaviour scripts for buttons, text, scene management |
| **Config** | Contains editable `game_config.csv` for designers |
| **Tests** | Unit tests for core logic (using Unity Test Framework) |

### Key Entities
- `Plant` – manages crop growth, harvest, and lifespan  
- `Animal` – milk production logic  
- `Worker` – automates player actions  
- `Plot` – holds crop or animal  
- `FarmService` – manages farm-wide operations  
- `ShopService` – handles buying/selling  

## ⚙️ How to Run

1. Open **Unity Hub**
2. Click **Add project**, select this folder  
3. Open scene `Farm.unity`
4. Press ▶️ **Play** in the Unity Editor  

---

## 🧪 Unit Test

Use the **Unity Test Runner**:
- Open **Window → General → Test Runner**
- Run all tests inside `Tests/DomainTests`

---

## 🛠 Technologies

- Unity Engine (2022+)
- C# (OOP, Clean Architecture)
- CSV Config Loader
- Unity Test Framework

---

## 📈 Future Improvements
- Add strawberry crops  
- Add offline production calculation  
- Add visual progress bar for each crop  
- Add player profile saving & loading system

---

## 👩‍💻 Author
**Võ Thu Thảo**  
Võ Thu Thảo, fifth-year student majoring in Software Engineering at University of Science and Technology – Da Nang. 
Passionate about game development.
