import styles from "./LogContainer.module.css";
import { Log } from "../log/Log";
import { useEffect, useState } from "react";
import useSocket from "../../hooks/useSocket";

export function LogContainer() {
    const [logs, setLogs] = useState([]);
    const { isConnected, onMessage } = useSocket();

    useEffect(() => {
        const callback = (event) => {
            const logMessage = event.data;
            if (logMessage !== "CONNECTION_ESTABLISHED\n" && logMessage !== "CONNECTION_CLOSED\n")
            setLogs((prevLogs) => [...prevLogs, logMessage]);
        };
        const unsub = onMessage(callback);

        return unsub;
    }, [isConnected]);
    return (
        <div className={styles.displayerContainer}>
            <h2>Logs</h2>
            <div className={styles.logsContainer}>
                {logs.map((log, index) => (
                    <div key={index}>
                        <Log message={log} />
                    </div>
                ))}
            </div>
        </div>
    );
}