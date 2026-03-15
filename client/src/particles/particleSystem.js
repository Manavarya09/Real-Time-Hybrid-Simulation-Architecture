import * as THREE from 'three';

export class ParticleSystem {
  constructor(scene) {
    this.scene = scene;
    this.particleSystems = new Map();
  }

  createRain(intensity = 1.0) {
    const count = 5000 * intensity;
    const geometry = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);
    const velocities = new Float32Array(count);

    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 500;
      positions[i * 3 + 1] = Math.random() * 200;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 500;
      velocities[i] = 50 + Math.random() * 50;
    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute('velocity', new THREE.BufferAttribute(velocities, 1));

    const material = new THREE.PointsMaterial({
      color: 0x8899aa,
      size: 0.5,
      transparent: true,
      opacity: 0.6,
      blending: THREE.AdditiveBlending
    });

    const particles = new THREE.Points(geometry, material);
    particles.userData = { type: 'rain', velocities, baseY: 0 };
    this.scene.add(particles);
    this.particleSystems.set('rain', particles);

    return particles;
  }

  createSnow(intensity = 1.0) {
    const count = 3000 * intensity;
    const geometry = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);
    const velocities = new Float32Array(count * 2);
    const sizes = new Float32Array(count);

    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 500;
      positions[i * 3 + 1] = Math.random() * 200;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 500;
      velocities[i * 2] = (Math.random() - 0.5) * 5;
      velocities[i * 2 + 1] = 5 + Math.random() * 10;
      sizes[i] = 1 + Math.random() * 2;
    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute('velocity', new THREE.BufferAttribute(velocities, 2));
    geometry.setAttribute('size', new THREE.BufferAttribute(sizes, 1));

    const material = new THREE.PointsMaterial({
      color: 0xffffff,
      size: 2,
      transparent: true,
      opacity: 0.8,
      map: this.createSnowflakeTexture(),
      blending: THREE.NormalBlending
    });

    const particles = new THREE.Points(geometry, material);
    particles.userData = { type: 'snow', velocities, sizes };
    this.scene.add(particles);
    this.particleSystems.set('snow', particles);

    return particles;
  }

  createSnowflakeTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = 32;
    canvas.height = 32;
    const ctx = canvas.getContext('2d');
    
    ctx.fillStyle = 'white';
    ctx.beginPath();
    ctx.arc(16, 16, 8, 0, Math.PI * 2);
    ctx.fill();
    
    const texture = new THREE.CanvasTexture(canvas);
    return texture;
  }

  createDust(intensity = 1.0) {
    const count = 1000 * intensity;
    const geometry = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);

    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 200;
      positions[i * 3 + 1] = Math.random() * 20;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 200;
    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));

    const material = new THREE.PointsMaterial({
      color: 0xaa9988,
      size: 1,
      transparent: true,
      opacity: 0.3
    });

    const particles = new THREE.Points(geometry, material);
    particles.userData = { type: 'dust' };
    this.scene.add(particles);
    this.particleSystems.set('dust', particles);

    return particles;
  }

  createFire(x, y, z, intensity = 1.0) {
    const count = 200 * intensity;
    const geometry = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);
    const colors = new Float32Array(count * 3);

    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 5;
      positions[i * 3 + 1] = Math.random() * 10;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 5;

      const t = Math.random();
      colors[i * 3] = 1.0;
      colors[i * 3 + 1] = 0.3 + t * 0.5;
      colors[i * 3 + 2] = t * 0.2;
    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

    const material = new THREE.PointsMaterial({
      size: 1.5,
      transparent: true,
      opacity: 0.8,
      vertexColors: true,
      blending: THREE.AdditiveBlending,
      map: this.createFireTexture()
    });

    const particles = new THREE.Points(geometry, material);
    particles.position.set(x, y, z);
    particles.userData = { type: 'fire', baseY: y };
    this.scene.add(particles);

    return particles;
  }

  createFireTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = 32;
    canvas.height = 32;
    const ctx = canvas.getContext('2d');
    
    const gradient = ctx.createRadialGradient(16, 16, 0, 16, 16, 16);
    gradient.addColorStop(0, 'rgba(255, 255, 255, 1)');
    gradient.addColorStop(0.3, 'rgba(255, 200, 0, 1)');
    gradient.addColorStop(0.7, 'rgba(255, 50, 0, 0.5)');
    gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
    
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, 32, 32);
    
    return new THREE.CanvasTexture(canvas);
  }

  createExplosion(x, y, z, intensity = 1.0) {
    const count = 500 * intensity;
    const geometry = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);
    const velocities = new Float32Array(count * 3);

    for (let i = 0; i < count; i++) {
      positions[i * 3] = x;
      positions[i * 3 + 1] = y;
      positions[i * 3 + 2] = z;
      
      const theta = Math.random() * Math.PI * 2;
      const phi = Math.random() * Math.PI;
      const speed = 10 + Math.random() * 30;
      
      velocities[i * 3] = Math.sin(phi) * Math.cos(theta) * speed;
      velocities[i * 3 + 1] = Math.cos(phi) * speed;
      velocities[i * 3 + 2] = Math.sin(phi) * Math.sin(theta) * speed;
    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute('velocity', new THREE.BufferAttribute(velocities, 3));

    const material = new THREE.PointsMaterial({
      color: 0xff6600,
      size: 2,
      transparent: true,
      opacity: 1,
      blending: THREE.AdditiveBlending
    });

    const particles = new THREE.Points(geometry, material);
    particles.userData = { 
      type: 'explosion', 
      velocities,
      life: 2.0,
      originY: y
    };
    this.scene.add(particles);

    return particles;
  }

  update(deltaTime) {
    const gravity = -9.8;

    for (const [type, particles] of this.particleSystems) {
      const positions = particles.geometry.attributes.position.array;
      const count = positions.length / 3;

      if (type === 'rain') {
        const velocities = particles.userData.velocities;
        
        for (let i = 0; i < count; i++) {
          positions[i * 3 + 1] -= velocities[i] * deltaTime;

          if (positions[i * 3 + 1] < 0) {
            positions[i * 3 + 1] = 150 + Math.random() * 50;
            positions[i * 3] = (Math.random() - 0.5) * 500;
            positions[i * 3 + 2] = (Math.random() - 0.5) * 500;
          }
        }
      }
      else if (type === 'snow') {
        const velocities = particles.userData.velocities;
        
        for (let i = 0; i < count; i++) {
          positions[i * 3 + 1] -= velocities[i * 2 + 1] * deltaTime;
          positions[i * 3] += velocities[i * 2] * deltaTime;
          positions[i * 3 + 2] += Math.sin(Date.now() * 0.001 + i) * 0.5 * deltaTime;

          if (positions[i * 3 + 1] < 0) {
            positions[i * 3 + 1] = 100 + Math.random() * 50;
            positions[i * 3] = (Math.random() - 0.5) * 500;
            positions[i * 3 + 2] = (Math.random() - 0.5) * 500;
          }
        }
      }
      else if (type === 'dust') {
        const time = Date.now() * 0.001;
        
        for (let i = 0; i < count; i++) {
          positions[i * 3] += Math.sin(time + i * 0.1) * 0.5 * deltaTime;
          positions[i * 3 + 1] = (Math.sin(time * 2 + i * 0.2) + 1) * 5;
        }
      }
      else if (type === 'fire') {
        const baseY = particles.userData.baseY;
        
        for (let i = 0; i < count; i++) {
          positions[i * 3 + 1] += (5 + Math.random() * 10) * deltaTime;
          
          const spread = positions[i * 3 + 1] / 10;
          positions[i * 3] += (Math.random() - 0.5) * spread * deltaTime;
          positions[i * 3 + 2] += (Math.random() - 0.5) * spread * deltaTime;

          if (positions[i * 3 + 1] > baseY + 15) {
            positions[i * 3 + 1] = baseY;
            positions[i * 3] = (Math.random() - 0.5) * 5;
            positions[i * 3 + 2] = (Math.random() - 0.5) * 5;
          }
        }
      }
      else if (type === 'explosion') {
        const velocities = particles.userData.velocities;
        
        particles.userData.life -= deltaTime;
        
        if (particles.userData.life <= 0) {
          this.scene.remove(particles);
          this.particleSystems.delete('explosion');
          continue;
        }

        particles.material.opacity = particles.userData.life / 2;

        for (let i = 0; i < count; i++) {
          positions[i * 3] += velocities[i * 3] * deltaTime;
          positions[i * 3 + 1] += velocities[i * 3 + 1] * deltaTime - gravity * deltaTime;
          positions[i * 3 + 2] += velocities[i * 3 + 2] * deltaTime;
          
          velocities[i * 3 + 1] += gravity * deltaTime;
        }
      }

      particles.geometry.attributes.position.needsUpdate = true;
    }
  }

  setIntensity(type, intensity) {
    const particles = this.particleSystems.get(type);
    if (!particles) return;

    particles.material.opacity = Math.min(1, intensity * 0.8);
  }

  remove(type) {
    const particles = this.particleSystems.get(type);
    if (particles) {
      this.scene.remove(particles);
      particles.geometry.dispose();
      particles.material.dispose();
      this.particleSystems.delete(type);
    }
  }

  clear() {
    for (const [type, particles] of this.particleSystems) {
      this.scene.remove(particles);
      particles.geometry.dispose();
      particles.material.dispose();
    }
    this.particleSystems.clear();
  }
}
