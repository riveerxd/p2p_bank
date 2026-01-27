import { useState, useEffect, useCallback, useRef } from "react";

let socket = null;

/*window.addEventListener('beforeunload', () => {
    if (socket) {
        socket.close();
    }
});*/

window.onbeforeunload = () => {
    if (!socket) return;
    socket.onclose = () => {};
    socket.close();
};

export default function useSocket() {
    const [isConnected, setConnected] = useState(false);
    const [reconnectCount, setReconnectCount] = useState(0);
    const reconnectTimeoutRef = useRef(null);

    const messageHandlersRef = useRef(new Set());

    const handleIncomingMessage = useCallback((event) => {
        if (event.data === "CONNECTION_ESTABLISHED\n") setConnected(true);
        else if (event.data === "CONNECTION_CLOSED\n") setConnected(false);
        
        // Broadcast to all subscribers in the Ref
        messageHandlersRef.current.forEach(handler => {
            try {
                handler(event);
            } catch (err) {
                console.error("Error in message handler:", err);
            }
        });
    }, []);

    const connect = useCallback(() => {
        if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) {
            socket.removeEventListener('message', handleIncomingMessage);
            socket.addEventListener('message', handleIncomingMessage);
            return;
        }

        socket = new WebSocket(import.meta.env.VITE_WS_URL);

        socket.onopen = () => {
            console.log("WebSocket Connected");
            
            socket.addEventListener('message', handleIncomingMessage);
            
            setReconnectCount(prev => prev + 1);
        };

        socket.onclose = () => {
            setConnected(false);
            console.log("WebSocket disconnected, attempting to reconnect...");
            
            socket.removeEventListener('message', handleIncomingMessage);

            if (reconnectTimeoutRef.current) clearTimeout(reconnectTimeoutRef.current);

            reconnectTimeoutRef.current = setTimeout(() => {
                connect();
            }, import.meta.env.VITE_RECONNECT_DELAY || 3000);
        };

        socket.onerror = (err) => {
            console.error("WebSocket Error:", err);
            socket.close();
        };
    }, [handleIncomingMessage]);

    useEffect(() => {
        connect();

        return () => {
            if (reconnectTimeoutRef.current) clearTimeout(reconnectTimeoutRef.current);
            if (socket) {
                socket.removeEventListener('message', handleIncomingMessage);
            }
        };
    }, [connect, handleIncomingMessage]);
    
    const onMessage = useCallback((callback) => {
        messageHandlersRef.current.add(callback);
        return () => messageHandlersRef.current.delete(callback); 
    }, []);

    const removeOnMessage = useCallback((callback) => {
        messageHandlersRef.current.delete(callback);
    }, []);

    const onOpen = useCallback((callback) => {
        const handler = () => callback();
        socket?.addEventListener('open', handler);
        return () => socket?.removeEventListener('open', handler);
    }, []);

    const onClose = useCallback((callback) => {
         const handler = () => callback();
        socket?.addEventListener('close', handler);
        return () => socket?.removeEventListener('close', handler);
    }, []);

    return {
        isConnected,
        onOpen,
        onMessage,
        removeOnMessage,
        onClose
    };
}