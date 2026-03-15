import { createScene } from './scene.js';
import { WebSocketClient } from './websocket.js';
import { CityRenderer } from './cityRenderer.js';
import { CarRenderer } from './traffic/carRenderer.js';
import { RoadRenderer } from './traffic/roadRenderer.js';
import { PlayerRenderer } from './player/playerRenderer.js';
import { PlayerController } from './player/playerController.js';

class NeuroCityApp {
  constructor() {
    this.scene = null;
    this.camera = null;
    this.wsClient = null;
    this.cityRenderer = null;
    this.carRenderer = null;
    this.roadRenderer = null;
    this.playerRenderer = null;
    this.playerController = null;
    this.isInitialized = false;
    this.roadsLoaded = false;
    this.localPlayer = null;
  }

  async init() {
    console.log('[NeuroCity] Initializing...');
    
    const { scene, camera, renderer, controls } = createScene();
    this.scene = scene;
    this.camera = camera;
    
    this.cityRenderer = new CityRenderer(scene);
    this.carRenderer = new CarRenderer(scene);
    this.roadRenderer = new RoadRenderer(scene);
    this.playerRenderer = new PlayerRenderer(scene);
    this.playerController = new PlayerController(camera, renderer.domElement);
    
    this.wsClient = new WebSocketClient();
    this.wsClient.onMessage = (data) => this.handleWorldState(data);
    this.wsClient.onConnect = () => console.log('[NeuroCity] Server connected');
    this.wsClient.onDisconnect = () => console.log('[NeuroCity] Server disconnected');
    this.wsClient.sendInput = (input) => this.sendPlayerInput(input);
    
    this.wsClient.connect();
    
    this.isInitialized = true;
    console.log('[NeuroCity] Initialization complete');
    
    this.startGameLoop();
  }

  startGameLoop() {
    const update = () => {
      if (this.localPlayer) {
        this.playerController.setPlayer(this.localPlayer);
        this.playerController.update();
        this.sendPlayerInput(this.playerController.keys);
      }
      requestAnimationFrame(update);
    };
    update();
  }

  sendPlayerInput(keys) {
    if (!this.wsClient || !this.wsClient.isConnected) return;
    
    const input = {
      moveForward: (keys.forward ? 1 : 0) + (keys.backward ? -1 : 0),
      moveRight: (keys.right ? 1 : 0) + (keys.left ? -1 : 0),
      sprint: keys.sprint || false,
      jump: keys.jump || false,
      deltaYaw: 0,
      deltaPitch: 0
    };

    const message = {
      type: 'input',
      data: input
    };

    this.wsClient.ws.send(JSON.stringify(message));
  }

  handleWorldState(worldState) {
    if (!worldState.buildings) return;
    
    this.cityRenderer.updateBuildings(worldState);

    if (worldState.roads && !this.roadsLoaded) {
      this.roadRenderer.renderRoads(worldState.roads);
      this.roadsLoaded = true;
    }

    if (worldState.cars) {
      this.carRenderer.updateCars(worldState.cars);
    }

    if (worldState.players && worldState.players.length > 0) {
      this.localPlayer = worldState.players[0];
      this.playerRenderer.updatePlayers(worldState.players);
    }
  }
}

const app = new NeuroCityApp();
app.init().catch(console.error);

window.addEventListener('beforeunload', () => {
  if (app.wsClient) {
    app.wsClient.disconnect();
  }
});
