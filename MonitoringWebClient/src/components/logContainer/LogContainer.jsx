import styles from "./LogContainer.module.css";
import { Log } from "../log/Log";
import { useEffect, useState } from "react";

export function LogContainer() {
    const [logs, setLogs] = useState([]);

    useEffect(() => {
        const socket = new WebSocket(import.meta.env.VITE_WS_URL);

        socket.onopen = () => {
            console.log("WebSocket connection established.");
        };

        socket.onmessage = (event) => {
            const logMessage = event.data;
            setLogs((prevLogs) => [...prevLogs, logMessage]);
        };

        socket.onclose = () => {
            console.log("WebSocket connection closed.");
        };
        
        return () => {
            socket.close();
        };
    }, []);

    return (
        <div className={styles.displayerContainer}>
            {logs.map((log, index) => (
                <Log key={index} message={log} />
            ))}
        </div>
    );
}