import * as THREE from 'three';

export class RoadRenderer {
  constructor(scene) {
    this.scene = scene;
    this.roads = null;
    this.roadMeshes = [];
  }

  renderRoads(roadGraph) {
    if (!roadGraph || !roadGraph.nodes) return;

    this.clear();

    const roadMaterial = new THREE.MeshStandardMaterial({
      color: 0x2a2a35,
      roughness: 0.9,
      metalness: 0.1
    });

    const nodeMap = new Map();
    for (const node of roadGraph.nodes) {
      nodeMap.set(node.id, node);
    }

    const processedConnections = new Set();

    for (const node of roadGraph.nodes) {
      if (!node.neighbors) continue;

      for (const neighborId of node.neighbors) {
        const connectionKey = [node.id, neighborId].sort().join('-');
        if (processedConnections.has(connectionKey)) continue;
        processedConnections.add(connectionKey);

        const neighbor = nodeMap.get(neighborId);
        if (!neighbor) continue;

        const startX = node.x;
        const startZ = node.z;
        const endX = neighbor.x;
        const endZ = neighbor.z;

        const length = Math.sqrt(
          Math.pow(endX - startX, 2) + Math.pow(endZ - startZ, 2)
        );

        const roadGeometry = new THREE.BoxGeometry(4, 0.1, length);
        const road = new THREE.Mesh(roadGeometry, roadMaterial);

        const midX = (startX + endX) / 2;
        const midZ = (startZ + endZ) / 2;

        road.position.set(midX, 0.05, midZ);

        const angle = Math.atan2(endZ - startZ, endX - startX);
        road.rotation.y = -angle;

        road.receiveShadow = true;
        this.scene.add(road);
        this.roadMeshes.push(road);

        const lineGeometry = new THREE.BoxGeometry(0.2, 0.12, length * 0.9);
        const lineMaterial = new THREE.MeshStandardMaterial({
          color: 0xffff00,
          emissive: 0x444400
        });
        const line = new THREE.Mesh(lineGeometry, lineMaterial);
        line.position.set(midX, 0.06, midZ);
        line.rotation.y = -angle;
        this.scene.add(line);
        this.roadMeshes.push(line);
      }
    }

    for (const node of roadGraph.nodes) {
      const intersectionGeometry = new THREE.CylinderGeometry(3, 3, 0.15, 16);
      const intersection = new THREE.Mesh(intersectionGeometry, roadMaterial);
      intersection.position.set(node.x, 0.075, node.z);
      intersection.receiveShadow = true;
      this.scene.add(intersection);
      this.roadMeshes.push(intersection);
    }

    console.log(`[RoadRenderer] Rendered ${roadGraph.nodes.length} nodes`);
  }

  clear() {
    for (const mesh of this.roadMeshes) {
      this.scene.remove(mesh);
      mesh.geometry.dispose();
    }
    this.roadMeshes = [];
  }
}
