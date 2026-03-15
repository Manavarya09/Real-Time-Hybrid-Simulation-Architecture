import * as THREE from 'three';

export class CityRenderer {
  constructor(scene) {
    this.scene = scene;
    this.buildings = new Map();
    this.materials = new Map();
  }

  updateBuildings(worldState) {
    const currentIds = new Set(worldState.buildings.map(b => b.id));
    
    for (const [id, mesh] of this.buildings) {
      if (!currentIds.has(id)) {
        this.scene.remove(mesh);
        mesh.geometry.dispose();
        this.buildings.delete(id);
      }
    }

    for (const building of worldState.buildings) {
      if (this.buildings.has(building.id)) {
        this.updateBuilding(building);
      } else {
        this.createBuilding(building);
      }
    }

    document.getElementById('tick').textContent = worldState.tick;
    document.getElementById('building-count').textContent = worldState.buildings.length;
  }

  createBuilding(building) {
    const geometry = new THREE.BoxGeometry(
      building.width,
      building.height,
      building.depth
    );
    
    const material = this.getMaterial(building.color);
    const mesh = new THREE.Mesh(geometry, material);
    
    mesh.position.set(
      building.position.x,
      building.position.y,
      building.position.z
    );
    
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    
    this.scene.add(mesh);
    this.buildings.set(building.id, mesh);
  }

  updateBuilding(building) {
    const mesh = this.buildings.get(building.id);
    if (!mesh) return;

    mesh.position.set(
      building.position.x,
      building.position.y,
      building.position.z
    );

    if (mesh.geometry.parameters.width !== building.width ||
        mesh.geometry.parameters.height !== building.height ||
        mesh.geometry.parameters.depth !== building.depth) {
      
      mesh.geometry.dispose();
      mesh.geometry = new THREE.BoxGeometry(
        building.width,
        building.height,
        building.depth
      );
    }

    const existingMaterial = mesh.material;
    const newMaterial = this.getMaterial(building.color);
    if (existingMaterial !== newMaterial) {
      mesh.material = newMaterial;
    }
  }

  getMaterial(color) {
    if (this.materials.has(color)) {
      return this.materials.get(color);
    }

    const material = new THREE.MeshStandardMaterial({
      color: new THREE.Color(color),
      roughness: 0.7,
      metalness: 0.2
    });

    this.materials.set(color, material);
    return material;
  }

  clear() {
    for (const [id, mesh] of this.buildings) {
      this.scene.remove(mesh);
      mesh.geometry.dispose();
    }
    this.buildings.clear();
  }
}
