let socket = null;
let dotNetRef = null;

export function connect(url, dotNetObj) {
    if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) {
        console.warn("WebSocket already open or connecting");
        return;
    }

    dotNetRef = dotNetObj;
    socket = new WebSocket(url);

    socket.onopen = () => {
        console.log("WebSocket connected:", url);
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("OnJsConnected");
        }
    };

    socket.onmessage = (event) => {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("OnJsMessageReceived", event.data);
        }
    };

    socket.onclose = (event) => {
        console.log("WebSocket closed", event.code, event.reason);
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("OnJsDisconnected", event.code, event.reason || "");
        }
        socket = null;
    };

    socket.onerror = (err) => {
        console.error("WebSocket error:", err);
    };
}

export function send(obj) {
    if (!socket || socket.readyState !== WebSocket.OPEN) {
        console.warn("WebSocket is not open, cannot send");
        return;
    }

    socket.send(JSON.stringify(obj));
}

export function close() {
    if (socket) {
        socket.close(1000, "Client closing");
        socket = null;
    }
}
