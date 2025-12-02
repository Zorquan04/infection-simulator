# InfectionSimulator — WPF Real-Time Infection Simulation

An application that simulates the spread of infection in a population of moving agents.

Frontend written in **WPF**, backend in **C#**, with motion visualization and a real-time statistics system.

---

## Features

### **Frontend (WPF)**
- Real-time agent rendering at 60 FPS.
- Accurate representation of movement, wall reflections, and collisions.
- Statistics bar updated **every 1 second**:
- number of healthy individuals,
- infected individuals,
- immune individuals,
- those who left the area,
- and the number of active agents.
- Stopping the simulation exactly after a set time (e.g., 60 seconds).

### **Backend (simulation logic)**
- Agent system with:
- health status (healthy / infected / immune),
- presence status (active / exited),
- speed, direction, and collisions.
- Population startup modes:
- **Normal day**
- **Post-epidemic world**
- Realistic infection model dependent on distance and contact time.
- Snapshot of the final population state saved to **JSON**.

### **Snapshot system**
- Each agent can create its own `Memento`.
- After the simulation is complete, a JSON is created:

```
snapshot.json
```
- Contains the full population state (positions, health, immunity, etc.).

---

## **Technologies and Tools**
- **C# / .NET**
- **WPF**
- **DispatcherTimer** + **CompositionTarget.Rendering**
- **JSON serialization**
- Git + GitHub (commit & CI expandable)

---

## **Starting**
1. Open the project in **Visual Studio 2022**.
2. Make sure the WPF project is set as the `Startup Project`.
3. Click **Start** — the simulation starts automatically.
4. The program runs until the set time expires.

---

## **Statistics Performance**
- Statistics refresh every 1 second.
- Based on the same data previously output by the console.
- Everything beautifully displayed in the UI—no flashing logs in the console.

---

## Issues resolved during implementation
- Map scaling after adding the UI panel.
- Collision offsets with the bottom and right edges.
- Animation synchronization with simulation logic.
- Stopping the entire animation after a specified time.
- Integration of the backend and frontend rendering loop.

---

## Snapshot example
After the simulation is complete, you receive the file:

```
/snapshot.json
```