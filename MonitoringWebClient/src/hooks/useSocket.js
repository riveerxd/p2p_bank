import { useState } from "react";

let socket;

export default function useSocket() {
    const [isConnected, setConnected] = useState(false);

    const openCallback = () => setConnected(true);
    const closeCallback = () => {
        setConnected(false);
        reconnect();
    }

    function connect() {
        socket = new WebSocket(import.meta.env.VITE_WS_URL);
        socket.addEventListener('open', openCallback);
        socket.addEventListener('close', closeCallback);
    }

    function reconnect() {
        setTimeout(() => {
            connect();
        }, import.meta.env.VITE_RECONNECT_DELAY || 3000);
    }

    if (!socket || socket.readyState === WebSocket.CLOSED) {
        connect();
    }

    const onOpen = (callback) => {
        socket.addEventListener('open', callback);
    }

    const onMessage = (callback) => {
        socket.addEventListener('message', callback);
    }

    const removeOnMessage = (callback) => {
        socket.removeEventListener('message', callback);
    }

    const onClose = (callback) => {
        socket.addEventListener('close', callback);
    }

    return {
        isConnected,
        onOpen,
        onMessage,
        removeOnMessage,
        onClose
    }
}