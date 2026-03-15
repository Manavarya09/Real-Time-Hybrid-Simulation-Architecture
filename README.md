# NeuroCity Engine

# -Real-Time-Hybrid-Simulation-Architecture

A production-quality hybrid simulation engine with C# backend and Three.js frontend.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    NeuroCity Engine                             │
├─────────────────────────────────────────────────────────────────┤
│  BACKEND (C# / .NET 8)          FRONTEND (Three.js / Vite)    │
│  ─────────────────────          ──────────────────────────    │
│  ┌──────────────────┐           ┌──────────────────────────┐   │
│  │  GameEngine      │  JSON    │  WebSocketClient        │   │
│  │  - Update()      │◄────────►│  - receive world state  │   │
│  │  - Broadcast()   │  WS      │                          │   │
│  └────────┬─────────┘           └──────────┬─────────────┘   │
│           │                                 │                  │
│  ┌────────▼─────────┐                       ▼                  │
│  │ SimulationLoop   │           ┌──────────────────────────┐   │
│  │ - 20 ticks/sec    │           │  CityRenderer           │   │
│  │ - Fixed timestep │           │  - BoxGeometry buildings │   │
│  └────────┬─────────┘           │  - Materials / shadows  │   │
│           │                      └──────────────────────────┘   │
│  ┌────────▼─────────┐                                              │
│  │ WorldState        │                                              │
│  │ - Buildings[]     │                                              │
│  │ - Tick counter    │                                              │
│  └──────────────────┘                                              │
│                                                                  │
│  ┌──────────────────┐                                              │
│  │ CityGenerator   │                                              │
│  │ - 20x20 grid    │                                              │
│  │ - Procedural     │                                              │
│  └──────────────────┘                                              │
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

```
NeuroCity/
├── Server/                           # C# Backend
│   ├── Core/
│   │   ├── GameEngine.cs             # Main game engine
│   │   ├── SimulationLoop.cs        # Fixed timestep loop
│   │   └── WorldState.cs             # Serializable world data
│   ├── Entities/
│   │   └── Entity.cs                 # Entity & Building classes
│   ├── Networking/
│   │   └── WebSocketServer.cs        # WebSocket server
│   ├── CityGeneration/
│   │   └── CityGenerator.cs          # Procedural city gen
│   ├── Program.cs                    # Entry point
│   └── NeuroCity.Server.csproj       # Project file
│
└── client/                           # Three.js Frontend
    ├── src/
    │   ├── main.js                   # Application entry
    │   ├── scene.js                  # Three.js scene setup
    │   ├── websocket.js              # WebSocket client
    │   └── cityRenderer.js           # Building renderer
    ├── index.html
    ├── package.json
    └── vite.config.js
```

## Prerequisites

- **.NET 8 SDK**: https://dotnet.microsoft.com/download
- **Node.js 18+**: https://nodejs.org/
- **npm** (comes with Node.js)

## Setup Instructions

### 1. Backend Setup

```bash
cd C:\NeuroCity\Server
dotnet restore
dotnet build
```

### 2. Frontend Setup

```bash
cd C:\NeuroCity\client
npm install
```

## Running the Engine

### Start the Backend Server

```bash
cd C:\NeuroCity\Server
dotnet run
```

The server will start on `ws://localhost:5000`.

### Start the Frontend

```bash
cd C:\NeuroCity\client
npm run dev
```

Open your browser to `http://localhost:3000`.

## Key Features

### Simulation Tick System
- Runs at 20 ticks per second using fixed timestep
- Independent from network frame rate
- Broadcasts world state every 60 ticks (3 seconds)

### Entity System
- Base `Entity` class with ID and 3D position
- `Building` extends Entity with height, width, depth, color, type
- JSON serialization for network transfer

### City Generation
- 20x20 procedural grid (400 buildings)
- Four building types: residential, commercial, industrial, skyscraper
- Random heights, colors, and dimensions per type

### WebSocket Protocol

**Server → Client (World State):**
```json
{
  "buildings": [
    {
      "id": "guid",
      "position": { "x": -100, "y": 10, "z": -50 },
      "type": "skyscraper",
      "height": 65,
      "width": 8,
      "depth": 8,
      "color": "#4A90A4"
    }
  ],
  "tick": 12345,
  "timestamp": 1699999999999
}
```

## Engine Design Patterns

1. **Fixed Timestep Loop**: Simulation runs independently at consistent rate
2. **Component-Based Entities**: Buildings are data-driven with visual components
3. **Separation of Concerns**: Rendering (Three.js) completely decoupled from simulation (C#)
4. **Server Authority**: All world state originates from server
5. **Message-Passing**: WebSocket uses JSON for language-agnostic communication

## Troubleshooting

### Port Already in Use
If port 5000 or 3000 is busy, modify:
- Server: Edit `WebSocketServer.cs` → `Port` constant
- Client: Edit `vite.config.js` → `server.port`

### WebSocket Connection Failed
1. Ensure backend is running first
2. Check firewall settings
3. Verify localhost resolves correctly

## Next Steps (Phase 2)

- Add entity movement/updates in real-time
- Implement client-side prediction
- Add player entities
- Collision detection
- Day/night cycle
- Resource management system
