# 《东汉末年灵帝传》Scaffold and Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the modular C# OOP Game Engine backend and the Godot-based UI Frontend under `C:\Users\beni3\opencode\donghan` folder.

**Architecture:** A decoupled multi-module architecture:
1. **Backend (C# .NET Core Console Library + Local Server)**: Hosts GameState, Agent Scheduler, and Event Oracle.
2. **Frontend (Godot 4.x .NET UI Project)**: Communicates with backend using a Local JSON/Web API or direct assemblies. In this first phase, to keep it modular and extremely fast with zero latency, we'll design Backend as a C# Class Library/Assembly, which the Godot .NET frontend directly references and runs in-process! This avoids any network overhead, simplifies execution, and fits natively in Godot's C# environment.

**Tech Stack:** C# .NET 8.0, Godot 4.x (with .NET), System.Text.Json, xUnit (for backend unit testing).

---

## Files Map

- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs` (Numerical & Chronicle Game State)
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs` (OOP orchestrator)
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs` (Interface for AI Orchestration)
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IEventOracle.cs` (Interface for Health, Childbirth, Natural Disasters)
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INarrator.cs` (Interface for story rendering)
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (xUnit test cases verifying calculations)

---

### Task 1: Scaffolding the C# Core Engine Class Library

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.sln`
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\DonghanEngine.Core.csproj`
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\DonghanEngine.Tests.csproj`

- [ ] **Step 1: Create the backend directory and solution structure using dotnet CLI**
Run: `dotnet new sln -o C:\Users\beni3\opencode\donghan\Backend` in powershell.

- [ ] **Step 2: Create Class Library for Core Logic**
Run: `dotnet new classlib -o C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core` and add to solution.

- [ ] **Step 3: Create xUnit Test Project**
Run: `dotnet new xunit -o C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests` and add to solution, then add reference to DonghanEngine.Core.

- [ ] **Step 4: Commit**
```bash
git add donghan/Backend
git commit -m "chore: scaffold backend c# projects and sln"
```

---

### Task 2: Implementing the GameState and Models

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs`

- [ ] **Step 1: Write the GameState model holding values and chronicle log**
Create `GameState.cs` holding:
```csharp
namespace DonghanEngine.Core;

public class MinisterState
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Favorability { get; set; } = 50; // 0-100
    public int Power { get; set; } = 50; // 0-100
}

public class GameState
{
    public int ImperialPower { get; set; } = 50; // 皇权 (0-100)
    public int Treasury { get; set; } = 1000;    // 国库 (0-9999)
    public int Health { get; set; } = 100;        // 皇帝健康 (0-100)
    public string ReignTitle { get; set; } = "光和"; // 年号
    public int ReignYear { get; set; } = 7;       // 年份 (光和七年)
    
    public Dictionary<string, MinisterState> Ministers { get; set; } = new();
    public List<string> Chronicle { get; set; } = new();

    public GameState()
    {
        // 预设十常侍与大将军
        Ministers["he_jin"] = new MinisterState { Name = "何进", Title = "大将军", Favorability = 50, Power = 60 };
        Ministers["zhang_rang"] = new MinisterState { Name = "张让", Title = "十常侍之首", Favorability = 60, Power = 70 };
    }

    public void ApplyNumericalDelta(int imperialPowerDelta, int treasuryDelta, int healthDelta)
    {
        ImperialPower = Math.Clamp(ImperialPower + imperialPowerDelta, 0, 100);
        Treasury = Math.Clamp(Treasury + treasuryDelta, 0, 9999);
        Health = Math.Clamp(Health + healthDelta, 0, 100);
    }
}
```

- [ ] **Step 2: Commit**
```bash
git add donghan/Backend/DonghanEngine.Core/GameState.cs
git commit -m "feat: implement game state model and core properties"
```

---

### Task 3: Defining Core AI Interfaces and C# Orchestrator

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs`
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IEventOracle.cs`
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INarrator.cs`
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs`

- [ ] **Step 1: Write the interface files `IAIScheduler.cs`, `IEventOracle.cs`, `INarrator.cs`**
- [ ] **Step 2: Write `GameEngine.cs` orchestrating turn processing**
- [ ] **Step 3: Commit**
```bash
git add donghan/Backend/DonghanEngine.Core
git commit -m "feat: add interfaces and game engine orchestrator"
```

---

### Task 4: Unit Testing Engine Logic

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: Create xUnit test file testing turn execution with mocked AI outputs**
- [ ] **Step 2: Run `dotnet test` to verify success**
- [ ] **Step 3: Commit**
```bash
git add donghan/Backend/DonghanEngine.Tests
git commit -m "test: implement unit tests verifying game engine state transitions"
```

---

### Task 5: Scaffolding the Godot 4.x .NET C# UI Project

We will generate a Godot project folder. Since Godot projects are essentially folder structures containing a `project.godot` file and source files, we will create the structure.

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Frontend\project.godot`
- Create: `C:\Users\beni3\opencode\donghan\Frontend\MainScene.tscn`
- Create: `C:\Users\beni3\opencode\donghan\Frontend\MainScene.cs`
- Create: `C:\Users\beni3\opencode\donghan\Frontend\WindowManager.cs`

- [ ] **Step 1: Setup `project.godot` file config**
- [ ] **Step 2: Add C# Godot scripts for layout & window stack**
- [ ] **Step 3: Commit**
```bash
git add donghan/Frontend
git commit -m "chore: scaffold godot net frontend project"
```
