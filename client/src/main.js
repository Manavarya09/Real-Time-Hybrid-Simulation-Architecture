import { createScene } from './scene.js';
import { WebSocketClient } from './websocket.js';
import { CityRenderer } from './cityRenderer.js';
import { CarRenderer } from './traffic/carRenderer.js';
import { RoadRenderer } from './traffic/roadRenderer.js';

class NeuroCityApp {
  constructor() {
    this.scene = null;
    this.wsClient = null;
    this.cityRenderer = null;
    this.carRenderer = null;
    this.roadRenderer = null;
    this.isInitialized = false;
    this.roadsLoaded = false;
  }

  async init() {
    console.log('[NeuroCity] Initializing...');
    
    const { scene } = createScene();
    this.scene = scene;
    
    this.cityRenderer = new CityRenderer(scene);
    this.carRenderer = new CarRenderer(scene);
    this.roadRenderer = new RoadRenderer(scene);
    
    this.wsClient = new WebSocketClient();
    this.wsClient.onMessage = (data) => this.handleWorldState(data);
    this.wsClient.onConnect = () => console.log('[NeuroCity] Server connected');
    this.wsClient.onDisconnect = () => console.log('[NeuroCity] Server disconnected');
    
    this.wsClient.connect();
    
    this.isInitialized = true;
    console.log('[NeuroCity] Initialization complete');
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
  }
}

const app = new NeuroCityApp();
app.init().catch(console.error);

window.addEventListener('beforeunload', () => {
  if (app.wsClient) {
    app.wsClient.disconnect();
  }
});
