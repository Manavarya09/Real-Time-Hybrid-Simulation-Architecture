export class SettingsManager {
  constructor() {
    this.settings = this.getDefaultSettings();
    this.listeners = new Map();
    this.isLoaded = false;
  }

  getDefaultSettings() {
    return {
      graphics: {
        quality: 'high',
        resolutionScale: 1.0,
        antiAliasing: true,
        shadows: true,
        shadowQuality: 'high',
        textureQuality: 'high',
        bloom: true,
        ambientOcclusion: true,
        vsync: true,
        maxFPS: 60
      },
      audio: {
        masterVolume: 1.0,
        musicVolume: 0.6,
        sfxVolume: 0.8,
        ambientVolume: 0.5,
        voiceVolume: 1.0,
        muteOnFocusLoss: false
      },
      game: {
        difficulty: 'normal',
        simulationSpeed: 1.0,
        autoSave: true,
        autoSaveInterval: 300,
        showHUD: true,
        showMinimap: true,
        showDebugInfo: false,
        enableDisasters: true
      },
      camera: {
        defaultMode: 'orbit',
        mouseSensitivity: 1.0,
        invertY: false,
        smoothness: 0.1,
        fov: 60,
        nearClip: 0.1,
        farClip: 2000
      },
      controls: {
        keyForward: 'KeyW',
        keyBackward: 'KeyS',
        keyLeft: 'KeyA',
        keyRight: 'KeyD',
        keyJump: 'Space',
        keySprint: 'ShiftLeft',
        keyCrouch: 'KeyC',
        keyInteract: 'KeyE',
        keyBuild: 'KeyB',
        keyDemolish: 'KeyX',
        keyPause: 'Escape'
      },
      network: {
        serverAddress: 'localhost:5000',
        autoReconnect: true,
        reconnectAttempts: 10,
        timeout: 5000
      },
      ui: {
        theme: 'dark',
        language: 'en',
        showFPS: true,
        showPing: true,
        showCoordinates: true,
        chatEnabled: true,
        notificationsEnabled: true
      }
    };
  }

  async load() {
    try {
      const saved = localStorage.getItem('neurocity_settings');
      if (saved) {
        const parsed = JSON.parse(saved);
        this.mergeSettings(parsed);
      }
      this.isLoaded = true;
      this.notifyListeners('load');
      console.log('[Settings] Loaded from storage');
    } catch (e) {
      console.warn('[Settings] Failed to load settings:', e);
      this.isLoaded = true;
    }
  }

  async save() {
    try {
      localStorage.setItem('neurocity_settings', JSON.stringify(this.settings));
      this.notifyListeners('save');
      console.log('[Settings] Saved to storage');
    } catch (e) {
      console.error('[Settings] Failed to save settings:', e);
    }
  }

  mergeSettings(saved) {
    const merge = (target, source) => {
      for (const key in source) {
        if (source[key] && typeof source[key] === 'object' && !Array.isArray(source[key])) {
          if (!target[key]) target[key] = {};
          merge(target[key], source[key]);
        } else {
          target[key] = source[key];
        }
      }
    };
    
    merge(this.settings, saved);
  }

  get(path) {
    const keys = path.split('.');
    let value = this.settings;
    
    for (const key of keys) {
      if (value && value[key] !== undefined) {
        value = value[key];
      } else {
        return undefined;
      }
    }
    
    return value;
  }

  set(path, value) {
    const keys = path.split('.');
    let target = this.settings;
    
    for (let i = 0; i < keys.length - 1; i++) {
      if (!target[keys[i]]) {
        target[keys[i]] = {};
      }
      target = target[keys[i]];
    }
    
    const oldValue = target[keys[keys.length - 1]];
    target[keys[keys.length - 1]] = value;
    
    this.notifyListeners('change', {
      path,
      oldValue,
      newValue: value
    });
  }

  reset(category = null) {
    if (category) {
      this.settings[category] = this.getDefaultSettings()[category];
    } else {
      this.settings = this.getDefaultSettings();
    }
    
    this.notifyListeners('reset', { category });
    this.save();
  }

  exportToJSON() {
    return JSON.stringify(this.settings, null, 2);
  }

  importFromJSON(jsonString) {
    try {
      const parsed = JSON.parse(jsonString);
      this.mergeSettings(parsed);
      this.notifyListeners('import');
      this.save();
      return true;
    } catch (e) {
      console.error('[Settings] Failed to import:', e);
      return false;
    }
  }

  on(event, callback) {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event).push(callback);
  }

  off(event, callback) {
    if (!this.listeners.has(event)) return;
    
    const callbacks = this.listeners.get(event);
    const index = callbacks.indexOf(callback);
    if (index > -1) {
      callbacks.splice(index, 1);
    }
  }

  notifyListeners(event, data = {}) {
    if (!this.listeners.has(event)) return;
    
    for (const callback of this.listeners.get(event)) {
      try {
        callback(data);
      } catch (e) {
        console.error('[Settings] Listener error:', e);
      }
    }
  }

  applyGraphicsSettings(renderer, scene) {
    const g = this.settings.graphics;
    
    renderer.setPixelRatio(window.devicePixelRatio * g.resolutionScale);
    renderer.shadowMap.enabled = g.shadows;
    
    if (g.quality === 'low') {
      renderer.shadowMap.type = THREE.BasicShadowMap;
    } else if (g.quality === 'medium') {
      renderer.shadowMap.type = THREE.PCFShadowMap;
    } else {
      renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    }
  }

  applyAudioSettings(audioManager) {
    if (!audioManager) return;
    
    const a = this.settings.audio;
    
    audioManager.setMasterVolume(a.masterVolume);
    audioManager.setMusicVolume(a.musicVolume);
    audioManager.setSFXVolume(a.sfxVolume);
  }

  applyCameraSettings(camera) {
    const c = this.settings.camera;
    
    camera.fov = c.fov;
    camera.near = c.nearClip;
    camera.far = c.farClip;
    camera.updateProjectionMatrix();
  }
}

export const settings = new SettingsManager();
