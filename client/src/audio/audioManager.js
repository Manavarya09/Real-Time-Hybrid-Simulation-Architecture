export class AudioManager {
  constructor() {
    this.context = null;
    this.masterGain = null;
    this.musicGain = null;
    this.sfxGain = null;
    this.ambientGain = null;
    
    this.sounds = new Map();
    this.music = null;
    this.ambientSources = new Map();
    
    this.isInitialized = false;
    this.isMuted = false;
    this.masterVolume = 1.0;
    this.musicVolume = 0.6;
    this.sfxVolume = 0.8;
    this.ambientVolume = 0.5;
  }

  async init() {
    try {
      this.context = new (window.AudioContext || window.webkitAudioContext)();
      
      this.masterGain = this.context.createGain();
      this.masterGain.gain.value = this.masterVolume;
      this.masterGain.connect(this.context.destination);
      
      this.musicGain = this.context.createGain();
      this.musicGain.gain.value = this.musicVolume;
      this.musicGain.connect(this.masterGain);
      
      this.sfxGain = this.context.createGain();
      this.sfxGain.gain.value = this.sfxVolume;
      this.sfxGain.connect(this.masterGain);
      
      this.ambientGain = this.context.createGain();
      this.ambientGain.gain.value = this.ambientVolume;
      this.ambientGain.connect(this.masterGain);
      
      this.createSynthSounds();
      
      this.isInitialized = true;
      console.log('[AudioManager] Initialized');
    } catch (e) {
      console.warn('[AudioManager] Web Audio not supported:', e);
    }
  }

  createSynthSounds() {
    this.sounds.set('step', this.createStepSound.bind(this));
    this.sounds.set('jump', this.createJumpSound.bind(this));
    this.sounds.set('land', this.createLandSound.bind(this));
    this.sounds.set('car', this.createCarSound.bind(this));
    this.sounds.set('carHorn', this.createHornSound.bind(this));
    this.sounds.set('siren', this.createSirenSound.bind(this));
    this.sounds.set('thunder', this.createThunderSound.bind(this));
    this.sounds.set('rain', this.createRainSound.bind(this));
    this.sounds.set('build', this.createBuildSound.bind(this));
    this.sounds.set('demolish', this.createDemolishSound.bind(this));
    this.sounds.set('uiClick', this.createUIClickSound.bind(this));
  }

  createStepSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'triangle';
    osc.frequency.value = 80 + Math.random() * 40;
    gain.gain.setValueAtTime(0.1, this.context.currentTime);
    gain.gain.exponentialDecayTo = 0.01;
    gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.1);
    osc.connect(gain);
    gain.connect(this.sfxGain);
    return { osc, gain };
  }

  createJumpSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(200, this.context.currentTime);
    osc.frequency.exponentialRampToValueAtTime(600, this.context.currentTime + 0.15);
    gain.gain.setValueAtTime(0.2, this.context.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.15);
    osc.connect(gain);
    gain.connect(this.sfxGain);
    return { osc, gain };
  }

  createLandSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(100, this.context.currentTime);
    osc.frequency.exponentialRampToValueAtTime(30, this.context.currentTime + 0.2);
    gain.gain.setValueAtTime(0.3, this.context.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.2);
    osc.connect(gain);
    gain.connect(this.sfxGain);
    return { osc, gain };
  }

  createCarSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'sawtooth';
    osc.frequency.value = 60;
    gain.gain.value = 0.05;
    osc.connect(gain);
    gain.connect(this.ambientGain);
    return { osc, gain };
  }

  createHornSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'square';
    osc.frequency.setValueAtTime(400, this.context.currentTime);
    osc.frequency.setValueAtTime(500, this.context.currentTime + 0.15);
    gain.gain.setValueAtTime(0.1, this.context.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.3);
    osc.connect(gain);
    gain.connect(this.sfxGain);
    return { osc, gain };
  }

  createSirenSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(600, this.context.currentTime);
    for (let i = 0; i < 5; i++) {
      osc.frequency.setValueAtTime(600 + i * 200, this.context.currentTime + i * 0.15);
      osc.frequency.setValueAtTime(600 + (i + 1) * 200, this.context.currentTime + i * 0.15 + 0.15);
    }
    gain.gain.value = 0.08;
    osc.connect(gain);
    gain.connect(this.ambientGain);
    return { osc, gain };
  }

  createThunderSound() {
    if (!this.context) return null;
    const bufferSize = this.context.sampleRate * 2;
    const buffer = this.context.createBuffer(1, bufferSize, this.context.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
      data[i] = (Math.random() * 2 - 1) * Math.exp(-i / (bufferSize * 0.3));
    }
    const source = this.context.createBufferSource();
    source.buffer = buffer;
    const gain = this.context.createGain();
    gain.gain.value = 0.4;
    source.connect(gain);
    gain.connect(this.ambientGain);
    return { source, gain };
  }

  createRainSound() {
    if (!this.context) return null;
    const bufferSize = this.context.sampleRate * 2;
    const buffer = this.context.createBuffer(1, bufferSize, this.context.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
      data[i] = Math.random() * 0.3;
    }
    const source = this.context.createBufferSource();
    source.buffer = buffer;
    source.loop = true;
    const filter = this.context.createBiquadFilter();
    filter.type = 'lowpass';
    filter.frequency.value = 1000;
    const gain = this.context.createGain();
    gain.gain.value = 0.15;
    source.connect(filter);
    filter.connect(gain);
    gain.connect(this.ambientGain);
    return { source, gain };
  }

  createBuildSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'square';
    osc.frequency.setValueAtTime(200, this.context.currentTime);
    osc.frequency.setValueAtTime(300, this.context.currentTime + 0.1);
    osc.frequency.setValueAtTime(400, this.context.currentTime + 0.2);
    gain.gain.setValueAtTime(0.15, this.context.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.3);
    osc.connect(gain);
    gain.connect(this.sfxGain);
    return { osc, gain };
  }

  createDemolishSound() {
    if (!this.context) return null;
    const bufferSize = this.context.sampleRate * 0.5;
    const buffer = this.context.createBuffer(1, bufferSize, this.context.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
      data[i] = (Math.random() * 2 - 1) * Math.exp(-i / (bufferSize * 0.1));
    }
    const source = this.context.createBufferSource();
    source.buffer = buffer;
    const gain = this.context.createGain();
    gain.gain.value = 0.3;
    source.connect(gain);
    gain.connect(this.sfxGain);
    return { source, gain };
  }

  createUIClickSound() {
    if (!this.context) return null;
    const osc = this.context.createOscillator();
    const gain = this.context.createGain();
    osc.type = 'sine';
    osc.frequency.value = 800;
    gain.gain.setValueAtTime(0.1, this.context.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.05);
    osc.connect(gain);
    gain.connect(this.sfxGain);
    return { osc, gain };
  }

  play(soundName, options = {}) {
    if (!this.isInitialized || this.isMuted) return;
    
    const soundFactory = this.sounds.get(soundName);
    if (!soundFactory) return;

    const sound = soundFactory();
    if (!sound) return;

    const startTime = this.context.currentTime + (options.delay || 0);
    
    if (sound.osc) {
      sound.osc.start(startTime);
      sound.osc.stop(startTime + 0.5);
    }
    if (sound.source) {
      sound.source.start(startTime);
    }
  }

  playMusic(trackName) {
    console.log(`[AudioManager] Playing music: ${trackName}`);
  }

  playAmbient(type) {
    if (!this.isInitialized) return;
    
    if (this.ambientSources.has(type)) {
      this.ambientSources.get(type).stop();
    }

    const soundFactory = this.sounds.get(type);
    if (!soundFactory) return;

    const sound = soundFactory();
    if (sound && sound.source) {
      sound.source.loop = true;
      sound.source.start();
      this.ambientSources.set(type, sound.source);
    }
  }

  stopAmbient(type) {
    const source = this.ambientSources.get(type);
    if (source) {
      source.stop();
      this.ambientSources.delete(type);
    }
  }

  setMasterVolume(value) {
    this.masterVolume = Math.max(0, Math.min(1, value));
    if (this.masterGain) {
      this.masterGain.gain.value = this.isMuted ? 0 : this.masterVolume;
    }
  }

  setMusicVolume(value) {
    this.musicVolume = Math.max(0, Math.min(1, value));
    if (this.musicGain) {
      this.musicGain.gain.value = this.musicVolume;
    }
  }

  setSFXVolume(value) {
    this.sfxVolume = Math.max(0, Math.min(1, value));
    if (this.sfxGain) {
      this.sfxGain.gain.value = this.sfxVolume;
    }
  }

  toggleMute() {
    this.isMuted = !this.isMuted;
    if (this.masterGain) {
      this.masterGain.gain.value = this.isMuted ? 0 : this.masterVolume;
    }
    return this.isMuted;
  }

  dispose() {
    if (this.context) {
      this.context.close();
    }
    this.sounds.clear();
    this.ambientSources.clear();
  }
}
