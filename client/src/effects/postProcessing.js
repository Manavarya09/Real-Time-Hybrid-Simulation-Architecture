import * as THREE from 'three';
import { EffectComposer } from 'three/addons/postprocessing/EffectComposer.js';
import { RenderPass } from 'three/addons/postprocessing/RenderPass.js';
import { UnrealBloomPass } from 'three/addons/postprocessing/UnrealBloomPass.js';
import { ShaderPass } from 'three/addons/postprocessing/ShaderPass.js';

const FXAAShader = {
  uniforms: {
    tDiffuse: { value: null },
    resolution: { value: new THREE.Vector2(1, 1) }
  },
  vertexShader: `
    varying vec2 vUv;
    void main() {
      vUv = uv;
      gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
    }
  `,
  fragmentShader: `
    uniform sampler2D tDiffuse;
    uniform vec2 resolution;
    varying vec2 vUv;
    void main() {
      vec2 texel = vec2(1.0 / resolution.x, 1.0 / resolution.y);
      vec4 color = texture2D(tDiffuse, vUv);
      color.rgb += texture2D(tDiffuse, vUv + texel * vec2(1, 0)).rgb * 0.25;
      color.rgb += texture2D(tDiffuse, vUv + texel * vec2(-1, 0)).rgb * 0.25;
      color.rgb += texture2D(tDiffuse, vUv + texel * vec2(0, 1)).rgb * 0.25;
      color.rgb += texture2D(tDiffuse, vUv + texel * vec2(0, -1)).rgb * 0.25;
      color.rgb *= 0.2;
      gl_FragColor = color;
    }
  `
};

const VignetteShader = {
  uniforms: {
    tDiffuse: { value: null },
    darkness: { value: 0.5 },
    offset: { value: 1.0 }
  },
  vertexShader: `
    varying vec2 vUv;
    void main() {
      vUv = uv;
      gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
    }
  `,
  fragmentShader: `
    uniform sampler2D tDiffuse;
    uniform float darkness;
    uniform float offset;
    varying vec2 vUv;
    void main() {
      vec4 color = texture2D(tDiffuse, vUv);
      vec2 uv = (vUv - vec2(0.5)) * vec2(offset);
      float vignette = 1.0 - dot(uv, uv);
      color.rgb *= mix(1.0 - darkness, 1.0, vignette);
      gl_FragColor = color;
    }
  `
};

export class PostProcessing {
  constructor(renderer, scene, camera) {
    this.renderer = renderer;
    this.scene = scene;
    this.camera = camera;
    this.composer = null;
    this.bloomPass = null;
    this.vignettePass = null;
    this.fxaaPass = null;
    this.enabled = true;
    this.settings = {
      bloom: { enabled: true, strength: 0.3, radius: 0.4, threshold: 0.85 },
      vignette: { enabled: true, darkness: 0.4, offset: 1.0 },
      fxaa: { enabled: true }
    };
  }

  init() {
    const size = this.renderer.getSize(new THREE.Vector2());
    
    this.composer = new EffectComposer(this.renderer);
    
    const renderPass = new RenderPass(this.scene, this.camera);
    this.composer.addPass(renderPass);
    
    if (this.settings.bloom.enabled) {
      this.bloomPass = new UnrealBloomPass(
        new THREE.Vector2(size.x, size.y),
        this.settings.bloom.strength,
        this.settings.bloom.radius,
        this.settings.bloom.threshold
      );
      this.composer.addPass(this.bloomPass);
    }
    
    if (this.settings.vignette.enabled) {
      this.vignettePass = new ShaderPass(VignetteShader);
      this.vignettePass.uniforms.darkness.value = this.settings.vignette.darkness;
      this.vignettePass.uniforms.offset.value = this.settings.vignette.offset;
      this.composer.addPass(this.vignettePass);
    }
    
    if (this.settings.fxaa.enabled) {
      this.fxaaPass = new ShaderPass(FXAAShader);
      this.fxaaPass.uniforms.resolution.value.set(size.x, size.y);
      this.composer.addPass(this.fxaaPass);
    }
    
    console.log('[PostProcessing] Initialized');
  }

  setBloom(strength, radius, threshold) {
    if (this.bloomPass) {
      this.bloomPass.strength = strength;
      this.bloomPass.radius = radius;
      this.bloomPass.threshold = threshold;
    }
  }

  setVignette(darkness, offset) {
    if (this.vignettePass) {
      this.vignettePass.uniforms.darkness.value = darkness;
      this.vignettePass.uniforms.offset.value = offset;
    }
  }

  setNightMode(isNight, transitionSpeed = 0.1) {
    if (!this.bloomPass) return;
    
    const targetStrength = isNight ? 0.6 : 0.3;
    const targetThreshold = isNight ? 0.6 : 0.85;
    
    this.bloomPass.strength += (targetStrength - this.bloomPass.strength) * transitionSpeed;
    this.bloomPass.threshold += (targetThreshold - this.bloomPass.threshold) * transitionSpeed;
  }

  setWeatherEffects(weather, intensity) {
    if (!this.vignettePass || !this.bloomPass) return;
    
    switch (weather) {
      case 'Rain':
      case 'Storm':
        this.bloomPass.strength = 0.1 + intensity * 0.3;
        this.vignettePass.uniforms.darkness.value = 0.3 + intensity * 0.4;
        break;
      case 'Fog':
        this.bloomPass.strength = 0.1;
        this.vignettePass.uniforms.darkness.value = 0.5 + intensity * 0.3;
        break;
      case 'Snow':
        this.bloomPass.strength = 0.4 + intensity * 0.2;
        this.vignettePass.uniforms.darkness.value = 0.3;
        break;
      default:
        this.bloomPass.strength = 0.3;
        this.vignettePass.uniforms.darkness.value = 0.4;
    }
  }

  resize(width, height) {
    if (this.composer) {
      this.composer.setSize(width, height);
    }
    if (this.fxaaPass) {
      this.fxaaPass.uniforms.resolution.value.set(width, height);
    }
  }

  render() {
    if (this.enabled && this.composer) {
      this.composer.render();
    } else {
      this.renderer.render(this.scene, this.camera);
    }
  }

  toggle() {
    this.enabled = !this.enabled;
  }

  dispose() {
    if (this.composer) {
      this.composer.dispose();
    }
  }
}
