import { useState, useEffect, useCallback, useRef } from "react";

// Keep the socket instance outside or in a ref to persist across renders
let socket = null;

export default function useSocket() {
    const [isConnected, setConnected] = useState(false);
    const reconnectTimeoutRef = useRef(null);

    const connect = useCallback(() => {
        if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) {
            return;
        }

        socket = new WebSocket(import.meta.env.VITE_WS_URL);

        socket.onopen = () => {
            console.log("WebSocket Connected");
            setConnected(true);
        };

        socket.onclose = () => {
            setConnected(false);
            console.log("WebSocket disconnected, attempting to reconnect...");
            
            if (reconnectTimeoutRef.current) clearTimeout(reconnectTimeoutRef.current);
            
            reconnectTimeoutRef.current = setTimeout(() => {
                connect();
            }, import.meta.env.VITE_RECONNECT_DELAY || 3000);
        };

        socket.onerror = (err) => {
            console.error("WebSocket Error:", err);
            socket.close();
        };
    }, []);

    useEffect(() => {
        connect();
        
        return () => {
            if (reconnectTimeoutRef.current) clearTimeout(reconnectTimeoutRef.current);
        };
    }, [connect]);

    const onOpen = useCallback((callback) => socket?.addEventListener('open', callback), []);
    const onMessage = useCallback((callback) => socket?.addEventListener('message', callback), []);
    const removeOnMessage = useCallback((callback) => socket?.removeEventListener('message', callback), []);
    const onClose = useCallback((callback) => socket?.addEventListener('close', callback), []);

    return {
        isConnected,
        onOpen,
        onMessage,
        removeOnMessage,
        onClose
    };
}