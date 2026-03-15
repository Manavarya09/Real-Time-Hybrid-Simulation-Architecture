const WS_URL = 'ws://localhost:5000';

export class WebSocketClient {
  constructor() {
    this.ws = null;
    this.isConnected = false;
    this.onMessage = null;
    this.onConnect = null;
    this.onDisconnect = null;
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 10;
    this.reconnectDelay = 2000;
  }

  connect() {
    this.ws = new WebSocket(WS_URL);
    
    this.ws.onopen = () => {
      console.log('[WebSocket] Connected to server');
      this.isConnected = true;
      this.reconnectAttempts = 0;
      this.updateStatus(true);
      if (this.onConnect) this.onConnect();
    };

    this.ws.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (this.onMessage) this.onMessage(data);
      } catch (e) {
        console.error('[WebSocket] Parse error:', e);
      }
    };

    this.ws.onclose = () => {
      console.log('[WebSocket] Disconnected from server');
      this.isConnected = false;
      this.updateStatus(false);
      if (this.onDisconnect) this.onDisconnect();
      this.attemptReconnect();
    };

    this.ws.onerror = (error) => {
      console.error('[WebSocket] Error:', error);
    };
  }

  attemptReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      console.log(`[WebSocket] Reconnecting... (${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
      setTimeout(() => this.connect(), this.reconnectDelay);
    }
  }

  updateStatus(connected) {
    const statusEl = document.getElementById('status');
    const infoText = document.querySelector('#info p');
    if (statusEl && infoText) {
      if (connected) {
        statusEl.classList.add('connected');
        infoText.innerHTML = '<span id="status" class="connected"></span>Connected';
      } else {
        statusEl.classList.remove('connected');
        infoText.innerHTML = '<span id="status"></span>Disconnected - Reconnecting...';
      }
    }
  }

  disconnect() {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    this.isConnected = false;
  }
}
