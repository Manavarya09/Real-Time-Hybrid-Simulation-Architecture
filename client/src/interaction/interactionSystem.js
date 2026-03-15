import * as THREE from 'three';

export class InteractionSystem {
  constructor(camera, domElement, scene) {
    this.camera = camera;
    this.domElement = domElement;
    this.scene = scene;
    
    this.raycaster = new THREE.Raycaster();
    this.mouse = new THREE.Vector2();
    
    this.hoveredObject = null;
    this.selectedObject = null;
    this.clickableObjects = [];
    
    this.onHover = null;
    this.onClick = null;
    this.onSelect = null;
    
    this.enabled = true;
    this.debugMode = false;
    
    this.hoverMesh = null;
    
    this.setupHoverIndicator();
    this.setupEventListeners();
  }

  setupHoverIndicator() {
    const geometry = new THREE.RingGeometry(2, 2.5, 32);
    const material = new THREE.MeshBasicMaterial({
      color: 0x00ff88,
      transparent: true,
      opacity: 0.8,
      side: THREE.DoubleSide
    });
    
    this.hoverMesh = new THREE.Mesh(geometry, material);
    this.hoverMesh.rotation.x = -Math.PI / 2;
    this.hoverMesh.visible = false;
    this.hoverMesh.renderOrder = 999;
    this.scene.add(this.hoverMesh);
  }

  setupEventListeners() {
    this.domElement.addEventListener('mousemove', this.onMouseMove.bind(this));
    this.domElement.addEventListener('click', this.onMouseClick.bind(this));
    this.domElement.addEventListener('contextmenu', this.onRightClick.bind(this));
  }

  onMouseMove(event) {
    if (!this.enabled) return;
    
    const rect = this.domElement.getBoundingClientRect();
    this.mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    this.mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;
    
    this.updateHover();
  }

  onMouseClick(event) {
    if (!this.enabled) return;
    
    if (this.hoveredObject) {
      if (this.onClick) {
        this.onClick(this.hoveredObject, event);
      }
      
      if (event.button === 0) {
        this.selectObject(this.hoveredObject);
      }
    }
  }

  onRightClick(event) {
    event.preventDefault();
    
    if (this.selectedObject && this.onClick) {
      this.onClick(this.selectedObject, event, true);
    }
  }

  updateHover() {
    this.raycaster.setFromCamera(this.mouse, this.camera);
    
    const intersects = this.raycaster.intersectObjects(this.clickableObjects, false);
    
    if (intersects.length > 0) {
      const object = intersects[0].object;
      
      if (object !== this.hoveredObject) {
        this.hoveredObject = object;
        
        this.updateHoverIndicator(intersects[0].point);
        
        if (this.onHover) {
          this.onHover(object);
        }
        
        this.domElement.style.cursor = 'pointer';
      }
    } else {
      if (this.hoveredObject) {
        this.hoveredObject = null;
        this.hoverMesh.visible = false;
        this.domElement.style.cursor = 'default';
        
        if (this.onHover) {
          this.onHover(null);
        }
      }
    }
  }

  updateHoverIndicator(point) {
    this.hoverMesh.position.set(point.x, point.y + 0.5, point.z);
    this.hoverMesh.visible = true;
    
    const pulse = Math.sin(Date.now() * 0.01) * 0.2 + 0.8;
    this.hoverMesh.material.opacity = pulse;
    
    this.hoverMesh.scale.setScalar(1 + Math.sin(Date.now() * 0.005) * 0.1);
  }

  addClickableObject(object) {
    if (!this.clickableObjects.includes(object)) {
      this.clickableObjects.push(object);
    }
  }

  removeClickableObject(object) {
    const index = this.clickableObjects.indexOf(object);
    if (index > -1) {
      this.clickableObjects.splice(index, 1);
    }
  }

  addClickableObjects(objects) {
    objects.forEach(obj => this.addClickableObject(obj));
  }

  selectObject(object) {
    if (this.selectedObject) {
      this.deselectObject();
    }
    
    this.selectedObject = object;
    
    if (object.userData.originalMaterial) {
      object.material = object.userData.originalMaterial;
    } else {
      object.userData.originalMaterial = object.material;
      
      const highlightMaterial = object.material.clone();
      highlightMaterial.emissive = new THREE.Color(0x004400);
      highlightMaterial.emissiveIntensity = 0.5;
      object.material = highlightMaterial;
    }
    
    if (this.onSelect) {
      this.onSelect(object);
    }
  }

  deselectObject() {
    if (!this.selectedObject) return;
    
    if (this.selectedObject.userData.originalMaterial) {
      this.selectedObject.material = this.selectedObject.userData.originalMaterial;
    }
    
    if (this.onSelect) {
      this.onSelect(null);
    }
    
    this.selectedObject = null;
  }

  getIntersectedObject() {
    this.raycaster.setFromCamera(this.mouse, this.camera);
    const intersects = this.raycaster.intersectObjects(this.clickableObjects, false);
    
    return intersects.length > 0 ? intersects[0] : null;
  }

  getGroundIntersection() {
    this.raycaster.setFromCamera(this.mouse, this.camera);
    
    const groundPlane = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0);
    const intersection = new THREE.Vector3();
    
    if (this.raycaster.ray.intersectPlane(groundPlane, intersection)) {
      return intersection;
    }
    
    return null;
  }

  setEnabled(enabled) {
    this.enabled = enabled;
    if (!enabled) {
      this.hoveredObject = null;
      this.hoverMesh.visible = false;
      this.domElement.style.cursor = 'default';
    }
  }

  setHighlightColor(color) {
    if (this.hoverMesh) {
      this.hoverMesh.material.color.set(color);
    }
  }

  clear() {
    this.clickableObjects = [];
    this.hoveredObject = null;
    this.selectedObject = null;
    this.hoverMesh.visible = false;
  }

  dispose() {
    this.domElement.removeEventListener('mousemove', this.onMouseMove);
    this.domElement.removeEventListener('click', this.onMouseClick);
    this.domElement.removeEventListener('contextmenu', this.onRightClick);
    
    this.scene.remove(this.hoverMesh);
    this.hoverMesh.geometry.dispose();
    this.hoverMesh.material.dispose();
  }
}

export class BuildingInteraction {
  constructor(interactionSystem, uiManager) {
    this.interactionSystem = interactionSystem;
    this.uiManager = uiManager;
    
    this.interactionSystem.onHover = this.handleHover.bind(this);
    this.interactionSystem.onSelect = this.handleSelect.bind(this);
    this.interactionSystem.onClick = this.handleClick.bind(this);
  }

  handleHover(object) {
    if (object && object.userData.buildingData) {
      const data = object.userData.buildingData;
      this.uiManager.showInteractionPrompt(`Building: ${data.type} (H: ${data.height.toFixed(1)})`);
    } else {
      this.uiManager.hideInteractionPrompt();
    }
  }

  handleSelect(object) {
    if (object && object.userData.buildingData) {
      this.uiManager.showBuildingMenu(object.userData.buildingData);
    } else {
      this.uiManager.hideBuildingMenu();
    }
  }

  handleClick(object, event, isRightClick) {
    if (!object || !object.userData.buildingData) return;
    
    const data = object.userData.buildingData;
    
    if (isRightClick) {
      console.log(`[Building] Right-click on ${data.type} at (${data.x}, ${data.z})`);
    } else {
      console.log(`[Building] Left-click on ${data.type}`);
    }
  }

  attachBuildingData(mesh, buildingData) {
    mesh.userData.buildingData = buildingData;
    this.interactionSystem.addClickableObject(mesh);
  }
}
