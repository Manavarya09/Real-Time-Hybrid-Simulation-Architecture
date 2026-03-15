import * as THREE from 'three';

export class PlayerController {
  constructor(camera, domElement) {
    this.camera = camera;
    this.domElement = domElement;
    
    this.player = null;
    
    this.keys = {
      forward: false,
      backward: false,
      left: false,
      right: false,
      sprint: false,
      jump: false
    };
    
    this.mouseMovement = { x: 0, y: 0 };
    this.isPointerLocked = false;
    
    this.yaw = 0;
    this.pitch = 0;
    
    this.minPitch = -Math.PI / 2.2;
    this.maxPitch = Math.PI / 2.2;
    
    this.moveSpeed = 0.05;
    this.lookSensitivity = 0.002;
    
    this.setupEventListeners();
  }

  setupEventListeners() {
    document.addEventListener('keydown', (e) => this.onKeyDown(e));
    document.addEventListener('keyup', (e) => this.onKeyUp(e));
    document.addEventListener('mousemove', (e) => this.onMouseMove(e));
    
    this.domElement.addEventListener('click', () => {
      this.domElement.requestPointerLock();
    });
    
    document.addEventListener('pointerlockchange', () => {
      this.isPointerLocked = document.pointerLockElement === this.domElement;
    });
  }

  onKeyDown(event) {
    switch (event.code) {
      case 'KeyW':
      case 'ArrowUp':
        this.keys.forward = true;
        break;
      case 'KeyS':
      case 'ArrowDown':
        this.keys.backward = true;
        break;
      case 'KeyA':
      case 'ArrowLeft':
        this.keys.left = true;
        break;
      case 'KeyD':
      case 'ArrowRight':
        this.keys.right = true;
        break;
      case 'ShiftLeft':
      case 'ShiftRight':
        this.keys.sprint = true;
        break;
      case 'Space':
        this.keys.jump = true;
        break;
    }
  }

  onKeyUp(event) {
    switch (event.code) {
      case 'KeyW':
      case 'ArrowUp':
        this.keys.forward = false;
        break;
      case 'KeyS':
      case 'ArrowDown':
        this.keys.backward = false;
        break;
      case 'KeyA':
      case 'ArrowLeft':
        this.keys.left = false;
        break;
      case 'KeyD':
      case 'ArrowRight':
        this.keys.right = false;
        break;
      case 'ShiftLeft':
      case 'ShiftRight':
        this.keys.sprint = false;
        break;
      case 'Space':
        this.keys.jump = false;
        break;
    }
  }

  onMouseMove(event) {
    if (!this.isPointerLocked) return;
    
    this.mouseMovement.x = event.movementX;
    this.mouseMovement.y = event.movementY;
  }

  setPlayer(player) {
    this.player = player;
  }

  update() {
    if (!this.player) return;

    this.yaw -= this.mouseMovement.x * this.lookSensitivity;
    this.pitch -= this.mouseMovement.y * this.lookSensitivity;
    this.pitch = Math.max(this.minPitch, Math.min(this.maxPitch, this.pitch));

    this.mouseMovement.x = 0;
    this.mouseMovement.y = 0;

    let moveForward = 0;
    let moveRight = 0;

    if (this.keys.forward) moveForward += 1;
    if (this.keys.backward) moveForward -= 1;
    if (this.keys.right) moveRight += 1;
    if (this.keys.left) moveRight -= 1;

    const speed = this.keys.sprint ? this.moveSpeed * 1.8 : this.moveSpeed;

    const forward = new THREE.Vector3(
      Math.sin(this.yaw),
      0,
      Math.cos(this.yaw)
    );
    const right = new THREE.Vector3(
      Math.sin(this.yaw + Math.PI / 2),
      0,
      Math.cos(this.yaw + Math.PI / 2)
    );

    const movement = new THREE.Vector3();
    movement.addScaledVector(forward, moveForward * speed);
    movement.addScaledVector(right, moveRight * speed);

    this.player.velocityX = movement.x * 1000;
    this.player.velocityZ = movement.z * 1000;
    this.player.sprint = this.keys.sprint;
    this.player.jump = this.keys.jump;
    this.player.deltaYaw = -this.mouseMovement.x * this.lookSensitivity;
    this.player.deltaPitch = -this.mouseMovement.y * this.lookSensitivity;

    this.player.x += movement.x;
    this.player.z += movement.z;
    this.player.yaw = this.yaw;
    this.player.pitch = this.pitch;

    this.camera.position.set(
      this.player.x,
      this.player.y + 1.5,
      this.player.z
    );

    const lookTarget = new THREE.Vector3(
      this.player.x + Math.sin(this.yaw) * Math.cos(this.pitch) * 10,
      this.player.y + 1.5 + Math.sin(this.pitch) * 10,
      this.player.z + Math.cos(this.yaw) * Math.cos(this.pitch) * 10
    );
    this.camera.lookAt(lookTarget);
  }
}
