export class UIManager {
  constructor() {
    this.elements = {};
    this.resourceDisplay = null;
    this.minimap = null;
    this.crosshair = null;
    this.controlsHint = null;
    this.weatherDisplay = null;
    this.timeDisplay = null;
    this.notificationQueue = [];
  }

  init() {
    this.createHUD();
    this.createMinimap();
    this.createCrosshair();
    this.createControlsHint();
    this.createNotificationContainer();
    console.log('[UIManager] Initialized');
  }

  createHUD() {
    const hud = document.createElement('div');
    hud.id = 'hud';
    hud.innerHTML = `
      <div id="resource-panel">
        <div class="resource-row">
          <span class="resource-icon">💰</span>
          <span class="resource-label">Money:</span>
          <span id="resource-money" class="resource-value">100,000</span>
        </div>
        <div class="resource-row">
          <span class="resource-icon">⚡</span>
          <span class="resource-label">Energy:</span>
          <span id="resource-energy" class="resource-value">500/1000</span>
        </div>
        <div class="resource-row">
          <span class="resource-icon">💧</span>
          <span class="resource-label">Water:</span>
          <span id="resource-water" class="resource-value">500/1000</span>
        </div>
        <div class="resource-row">
          <span class="resource-icon">🍎</span>
          <span class="resource-label">Food:</span>
          <span id="resource-food" class="resource-value">500/1000</span>
        </div>
        <div class="resource-row">
          <span class="resource-icon">👥</span>
          <span class="resource-label">Population:</span>
          <span id="resource-population" class="resource-value">0</span>
        </div>
        <div class="resource-row">
          <span class="resource-icon">😊</span>
          <span class="resource-label">Happiness:</span>
          <span id="resource-happiness" class="resource-value">70%</span>
        </div>
      </div>
      
      <div id="time-panel">
        <div id="time-display">08:00</div>
        <div id="day-counter">Day 1</div>
        <div id="weather-display">
          <span id="weather-icon">☀️</span>
          <span id="weather-text">Clear</span>
        </div>
        <div id="temperature-display">20°C</div>
      </div>
      
      <div id="stats-panel">
        <div class="stat-row">
          <span>Buildings:</span>
          <span id="stat-buildings">0</span>
        </div>
        <div class="stat-row">
          <span>Cars:</span>
          <span id="stat-cars">0</span>
        </div>
        <div class="stat-row">
          <span>FPS:</span>
          <span id="stat-fps">60</span>
        </div>
      </div>
    `;
    document.getElementById('app').appendChild(hud);
    this.elements.hud = hud;
  }

  createMinimap() {
    const minimap = document.createElement('div');
    minimap.id = 'minimap';
    minimap.innerHTML = `
      <canvas id="minimap-canvas" width="200" height="200"></canvas>
      <div id="minimap-player"></div>
      <div id="minimap-border"></div>
    `;
    document.getElementById('app').appendChild(minimap);
    this.elements.minimap = minimap;
    this.initMinimapCanvas();
  }

  initMinimapCanvas() {
    const canvas = document.getElementById('minimap-canvas');
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#1a1a2e';
    ctx.fillRect(0, 0, 200, 200);
  }

  createCrosshair() {
    const crosshair = document.createElement('div');
    crosshair.id = 'crosshair';
    crosshair.innerHTML = `
      <div class="crosshair-line horizontal"></div>
      <div class="crosshair-line vertical"></div>
    `;
    document.getElementById('app').appendChild(crosshair);
    this.elements.crosshair = crosshair;
  }

  createControlsHint() {
    const hint = document.createElement('div');
    hint.id = 'controls-hint';
    hint.innerHTML = `
      <div class="hint-title">Controls</div>
      <div class="hint-row"><span>WASD</span> Move</div>
      <div class="hint-row"><span>Shift</span> Sprint</div>
      <div class="hint-row"><span>Space</span> Jump</div>
      <div class="hint-row"><span>Mouse</span> Look</div>
      <div class="hint-row"><span>Click</span> Lock Cursor</div>
      <div class="hint-row"><span>1-4</span> Time Scale</div>
    `;
    document.getElementById('app').appendChild(hint);
    this.elements.controlsHint = hint;
  }

  createNotificationContainer() {
    const container = document.createElement('div');
    container.id = 'notification-container';
    document.getElementById('app').appendChild(container);
    this.elements.notifications = container;
  }

  updateResources(resources) {
    if (!resources) return;
    
    if (resources.Money !== undefined) {
      document.getElementById('resource-money').textContent = 
        Math.floor(resources.Money).toLocaleString();
    }
    if (resources.Energy !== undefined) {
      document.getElementById('resource-energy').textContent = 
        `${Math.floor(resources.Energy)}/1000`;
    }
    if (resources.Water !== undefined) {
      document.getElementById('resource-water').textContent = 
        `${Math.floor(resources.Water)}/1000`;
    }
    if (resources.Food !== undefined) {
      document.getElementById('resource-food').textContent = 
        `${Math.floor(resources.Food)}/1000`;
    }
    if (resources.Population !== undefined) {
      document.getElementById('resource-population').textContent = 
        Math.floor(resources.Population).toLocaleString();
    }
    if (resources.Happiness !== undefined) {
      document.getElementById('resource-happiness').textContent = 
        `${Math.floor(resources.Happiness)}%`;
    }
  }

  updateTime(timeOfDay, dayCount) {
    const hours = Math.floor(timeOfDay);
    const minutes = Math.floor((timeOfDay - hours) * 60);
    const timeStr = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    
    const timeEl = document.getElementById('time-display');
    if (timeEl) timeEl.textContent = timeStr;
    
    const dayEl = document.getElementById('day-counter');
    if (dayEl) dayEl.textContent = `Day ${dayCount || 1}`;
  }

  updateWeather(weather, temperature) {
    const iconEl = document.getElementById('weather-icon');
    const textEl = document.getElementById('weather-text');
    const tempEl = document.getElementById('temperature-display');
    
    if (iconEl && textEl) {
      const icons = {
        'Clear': '☀️',
        'Cloudy': '☁️',
        'Rain': '🌧️',
        'Storm': '⛈️',
        'Fog': '🌫️',
        'Snow': '❄️'
      };
      iconEl.textContent = icons[weather] || '☀️';
      textEl.textContent = weather || 'Clear';
    }
    
    if (tempEl && temperature !== undefined) {
      tempEl.textContent = `${Math.floor(temperature)}°C`;
    }
  }

  updateStats(buildings, cars) {
    const buildingsEl = document.getElementById('stat-buildings');
    const carsEl = document.getElementById('stat-cars');
    
    if (buildingsEl) buildingsEl.textContent = buildings || 0;
    if (carsEl) carsEl.textContent = cars || 0;
  }

  updateFPS(fps) {
    const fpsEl = document.getElementById('stat-fps');
    if (fpsEl) fpsEl.textContent = Math.floor(fps);
  }

  updateMinimap(playerX, playerZ, buildings, mapSize = 300) {
    const canvas = document.getElementById('minimap-canvas');
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    const scale = 200 / mapSize;
    const centerX = 100;
    const centerY = 100;

    ctx.fillStyle = '#1a1a2e';
    ctx.fillRect(0, 0, 200, 200);

    if (buildings) {
      ctx.fillStyle = '#4a5568';
      for (const building of buildings) {
        const x = centerX + (building.position?.x || building.x || 0) * scale;
        const z = centerY + (building.position?.z || building.z || 0) * scale;
        const size = (building.width || 5) * scale;
        ctx.fillRect(x - size/2, z - size/2, size, size);
      }
    }

    if (playerX !== undefined && playerZ !== undefined) {
      const playerMarker = document.getElementById('minimap-player');
      if (playerMarker) {
        playerMarker.style.left = `${centerX + playerX * scale}px`;
        playerMarker.style.top = `${centerY + playerZ * scale}px`;
      }
    }
  }

  showNotification(message, type = 'info', duration = 3000) {
    const container = document.getElementById('notification-container');
    if (!container) return;

    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    container.appendChild(notification);
    
    setTimeout(() => {
      notification.classList.add('fade-out');
      setTimeout(() => notification.remove(), 500);
    }, duration);
  }

  showInteractionPrompt(text) {
    let prompt = document.getElementById('interaction-prompt');
    if (!prompt) {
      prompt = document.createElement('div');
      prompt.id = 'interaction-prompt';
      document.getElementById('app').appendChild(prompt);
    }
    prompt.textContent = text;
    prompt.style.display = 'block';
  }

  hideInteractionPrompt() {
    const prompt = document.getElementById('interaction-prompt');
    if (prompt) prompt.style.display = 'none';
  }

  showBuildingMenu(building) {
    let menu = document.getElementById('building-menu');
    if (!menu) {
      menu = document.createElement('div');
      menu.id = 'building-menu';
      menu.innerHTML = `
        <div class="menu-header">Building Info</div>
        <div class="menu-content"></div>
        <div class="menu-actions">
          <button class="btn-upgrade">Upgrade</button>
          <button class="btn-demolish">Demolish</button>
        </div>
      `;
      document.getElementById('app').appendChild(menu);
    }
    
    const content = menu.querySelector('.menu-content');
    if (content && building) {
      content.innerHTML = `
        <div class="info-row"><span>Type:</span><span>${building.type}</span></div>
        <div class="info-row"><span>Height:</span><span>${building.height}</span></div>
      `;
    }
    
    menu.style.display = 'block';
  }

  hideBuildingMenu() {
    const menu = document.getElementById('building-menu');
    if (menu) menu.style.display = 'none';
  }

  togglePointerLock() {
    const hint = this.elements.controlsHint;
    if (hint) {
      hint.style.opacity = document.pointerLockElement ? '0.3' : '1';
    }
  }
}
