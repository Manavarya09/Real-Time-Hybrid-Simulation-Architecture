import * as THREE from 'three';

export class PlayerRenderer {
  constructor(scene) {
    this.scene = scene;
    this.players = new Map();
  }

  updatePlayers(players) {
    const currentIds = new Set(players.map(p => p.id));

    for (const [id, mesh] of this.players) {
      if (!currentIds.has(id)) {
        this.scene.remove(mesh);
        mesh.geometry.dispose();
        this.players.delete(id);
      }
    }

    for (const player of players) {
      if (this.players.has(player.id)) {
        this.updatePlayer(player);
      } else {
        this.createPlayer(player);
      }
    }
  }

  createPlayer(player) {
    const group = new THREE.Group();

    const bodyGeometry = new THREE.CapsuleGeometry(0.4, 1, 4, 8);
    const bodyMaterial = new THREE.MeshStandardMaterial({
      color: 0x00ff88,
      roughness: 0.5,
      metalness: 0.3,
      emissive: 0x003322,
      emissiveIntensity: 0.3
    });
    const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
    body.position.y = 0.9;
    body.castShadow = true;
    group.add(body);

    const headGeometry = new THREE.SphereGeometry(0.35, 16, 16);
    const headMaterial = new THREE.MeshStandardMaterial({
      color: 0xffddaa,
      roughness: 0.8
    });
    const head = new THREE.Mesh(headGeometry, headMaterial);
    head.position.y = 2;
    head.castShadow = true;
    group.add(head);

    group.position.set(player.x, player.y, player.z);
    this.scene.add(group);
    this.players.set(player.id, group);
  }

  updatePlayer(player) {
    const mesh = this.players.get(player.id);
    if (!mesh) return;

    mesh.position.set(player.x, player.y, player.z);
    mesh.rotation.y = player.rotation || 0;
  }
}
