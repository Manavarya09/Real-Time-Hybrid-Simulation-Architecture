import * as THREE from 'three';

export class CameraSystem {
  constructor(camera, domElement) {
    this.camera = camera;
    this.domElement = domElement;
    
    this.mode = 'orbit';
    this.target = new THREE.Vector3(0, 0, 0);
    
    this.orbitControls = null;
    this.firstPersonController = null;
    this.thirdPersonController = null;
    this.cinematicController = null;
    
    this.cameraModes = {
      orbit: this.initOrbit.bind(this),
      firstPerson: this.initFirstPerson.bind(this),
      thirdPerson: this.initThirdPerson.bind(this),
      topDown: this.initTopDown.bind(this),
      cinematic: this.initCinematic.bind(this)
    };
    
    this.offset = new THREE.Vector3();
    this.smoothness = 0.1;
  }

  initOrbit(controls) {
    this.mode = 'orbit';
    this.orbitControls = controls;
    this.orbitControls.enabled = true;
    this.camera.position.set(200, 150, 200);
    this.camera.lookAt(0, 0, 0);
  }

  initFirstPerson() {
    this.mode = 'firstPerson';
    if (this.orbitControls) this.orbitControls.enabled = false;
    
    this.camera.position.set(0, 2, 50);
    this.camera.rotation.set(0, 0, 0);
  }

  initThirdPerson() {
    this.mode = 'thirdPerson';
    if (this.orbitControls) this.orbitControls.enabled = false;
  }

  initTopDown() {
    this.mode = 'topDown';
    if (this.orbitControls) this.orbitControls.enabled = false;
    
    this.camera.position.set(0, 300, 0);
    this.camera.lookAt(0, 0, 0);
    this.camera.rotation.z = 0;
  }

  initCinematic() {
    this.mode = 'cinematic';
    if (this.orbitControls) this.orbitControls.enabled = false;
    
    this.cinematicTime = 0;
    this.cinematicPath = this.generateCinematicPath();
    this.cinematicIndex = 0;
  }

  generateCinematicPath() {
    const points = [];
    const radius = 150;
    const height = 80;
    
    for (let i = 0; i < 20; i++) {
      const angle = (i / 20) * Math.PI * 2;
      points.push(new THREE.Vector3(
        Math.cos(angle) * radius,
        height + Math.sin(i * 0.5) * 30,
        Math.sin(angle) * radius
      ));
    }
    
    return points;
  }

  setMode(modeName) {
    if (this.cameraModes[modeName]) {
      this.cameraModes[modeName]();
      console.log(`[CameraSystem] Switched to ${modeName} mode`);
    }
  }

  setTarget(x, y, z) {
    this.target.set(x, y, z);
  }

  followObject(object, offset = null) {
    if (offset) {
      this.offset.copy(offset);
    }
    
    const targetPosition = new THREE.Vector3();
    targetPosition.copy(object.position).add(this.offset);
    
    this.camera.position.lerp(targetPosition, this.smoothness);
    this.camera.lookAt(object.position);
  }

  update(deltaTime, playerData) {
    switch (this.mode) {
      case 'firstPerson':
        this.updateFirstPerson(deltaTime, playerData);
        break;
      case 'thirdPerson':
        this.updateThirdPerson(deltaTime, playerData);
        break;
      case 'topDown':
        this.updateTopDown(deltaTime, playerData);
        break;
      case 'cinematic':
        this.updateCinematic(deltaTime);
        break;
      case 'orbit':
        if (this.orbitControls) {
          this.orbitControls.target.lerp(this.target, this.smoothness);
        }
        break;
    }
  }

  updateFirstPerson(deltaTime, playerData) {
    if (!playerData) return;
    
    this.camera.position.set(
      playerData.x,
      playerData.y + 1.7,
      playerData.z
    );
    
    if (playerData.rotation !== undefined) {
      this.camera.rotation.y = -playerData.rotation;
    }
    if (playerData.pitch !== undefined) {
      this.camera.rotation.x = -playerData.pitch;
    }
  }

  updateThirdPerson(deltaTime, playerData) {
    if (!playerData) return;
    
    const distance = 10;
    const height = 5;
    
    const idealPosition = new THREE.Vector3(
      playerData.x - Math.sin(playerData.rotation || 0) * distance,
      playerData.y + height,
      playerData.z - Math.cos(playerData.rotation || 0) * distance
    );
    
    this.camera.position.lerp(idealPosition, this.smoothness * 2);
    
    const lookTarget = new THREE.Vector3(
      playerData.x,
      playerData.y + 2,
      playerData.z
    );
    this.camera.lookAt(lookTarget);
  }

  updateTopDown(deltaTime, playerData) {
    if (!playerData) return;
    
    const height = 200;
    const targetPos = new THREE.Vector3(playerData.x, height, playerData.z);
    
    this.camera.position.lerp(targetPos, this.smoothness);
    this.camera.lookAt(playerData.x, 0, playerData.z);
  }

  updateCinematic(deltaTime) {
    if (!this.cinematicPath || this.cinematicPath.length === 0) return;
    
    this.cinematicTime += deltaTime * 0.1;
    
    const index = Math.floor(this.cinematicTime) % this.cinematicPath.length;
    const nextIndex = (index + 1) % this.cinematicPath.length;
    const t = this.cinematicTime % 1;
    
    const position = new THREE.Vector3().lerpVectors(
      this.cinematicPath[index],
      this.cinematicPath[nextIndex],
      t
    );
    
    this.camera.position.lerp(position, 0.05);
    this.camera.lookAt(this.target);
  }

  cycleMode() {
    const modes = Object.keys(this.cameraModes);
    const currentIndex = modes.indexOf(this.mode);
    const nextIndex = (currentIndex + 1) % modes.length;
    
    this.setMode(modes[nextIndex]);
    return modes[nextIndex];
  }

  zoom(delta) {
    if (this.mode === 'orbit' && this.orbitControls) {
      this.orbitControls.dollyIn(delta);
    } else if (this.mode === 'topDown') {
      this.camera.position.y = Math.max(50, Math.min(500, this.camera.position.y - delta * 10));
    }
  }

  reset() {
    this.camera.position.set(200, 150, 200);
    this.camera.lookAt(0, 0, 0);
    this.target.set(0, 0, 0);
    
    if (this.orbitControls) {
      this.orbitControls.reset();
    }
  }

  getState() {
    return {
      mode: this.mode,
      position: this.camera.position.clone(),
      rotation: this.camera.rotation.clone(),
      target: this.target.clone()
    };
  }

  setState(state) {
    if (state.mode) this.setMode(state.mode);
    if (state.target) this.target.copy(state.target);
    if (state.position) this.camera.position.copy(state.position);
    if (state.rotation) this.camera.rotation.copy(state.rotation);
  }
}
