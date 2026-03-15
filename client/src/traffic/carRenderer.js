import * as THREE from 'three';

export class CarRenderer {
  constructor(scene) {
    this.scene = scene;
    this.cars = new Map();
    this.materials = new Map();
  }

  updateCars(cars) {
    const currentIds = new Set(cars.map(c => c.id));

    for (const [id, mesh] of this.cars) {
      if (!currentIds.has(id)) {
        this.scene.remove(mesh);
        mesh.geometry.dispose();
        this.cars.delete(id);
      }
    }

    for (const car of cars) {
      if (this.cars.has(car.id)) {
        this.updateCar(car);
      } else {
        this.createCar(car);
      }
    }

    document.getElementById('car-count').textContent = cars.length;
  }

  createCar(car) {
    const group = new THREE.Group();

    const bodyGeometry = new THREE.BoxGeometry(2, 0.8, 4);
    const bodyMaterial = this.getMaterial(car.color);
    const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
    body.position.y = 0.4;
    body.castShadow = true;
    group.add(body);

    const cabinGeometry = new THREE.BoxGeometry(1.6, 0.6, 2);
    const cabinMaterial = new THREE.MeshStandardMaterial({
      color: 0x333344,
      roughness: 0.3,
      metalness: 0.8
    });
    const cabin = new THREE.Mesh(cabinGeometry, cabinMaterial);
    cabin.position.set(0, 1, -0.3);
    cabin.castShadow = true;
    group.add(cabin);

    const wheelGeometry = new THREE.CylinderGeometry(0.3, 0.3, 0.2, 16);
    const wheelMaterial = new THREE.MeshStandardMaterial({
      color: 0x111111,
      roughness: 0.9
    });

    const wheelPositions = [
      { x: -0.9, z: 1.2 },
      { x: 0.9, z: 1.2 },
      { x: -0.9, z: -1.2 },
      { x: 0.9, z: -1.2 }
    ];

    for (const pos of wheelPositions) {
      const wheel = new THREE.Mesh(wheelGeometry, wheelMaterial);
      wheel.rotation.z = Math.PI / 2;
      wheel.position.set(pos.x, 0.3, pos.z);
      group.add(wheel);
    }

    group.position.set(car.x, car.y, car.z);
    group.rotation.y = car.rotation || 0;

    this.scene.add(group);
    this.cars.set(car.id, group);
  }

  updateCar(car) {
    const mesh = this.cars.get(car.id);
    if (!mesh) return;

    mesh.position.set(car.x, car.y, car.z);
    mesh.rotation.y = car.rotation || 0;
  }

  getMaterial(color) {
    if (this.materials.has(color)) {
      return this.materials.get(color);
    }

    const material = new THREE.MeshStandardMaterial({
      color: new THREE.Color(color),
      roughness: 0.4,
      metalness: 0.6
    });

    this.materials.set(color, material);
    return material;
  }

  clear() {
    for (const [id, mesh] of this.cars) {
      this.scene.remove(mesh);
      mesh.geometry.dispose();
    }
    this.cars.clear();
  }
}
