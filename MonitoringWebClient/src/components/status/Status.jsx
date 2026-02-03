import { useEffect, useState } from 'react';
import { shutdownServer, getStartTime } from '../../api/api';
import useSocket from '../../hooks/useSocket';
import styles from './Status.module.css';

export function Status() {
    const {isConnected} = useSocket();

    const [runningSince, setRunningSince] = useState(null);

    const shutdown = async () => {
        await shutdownServer();
    };

    useEffect(() => {
        getStartTime().then((response) => {
            response.json().then((responseBody) => {
                const unixTime = Date.parse(responseBody)
                const date = new Date(unixTime);
                setRunningSince(date.toLocaleDateString());
            });
        })
    }, [isConnected]);

    return (
        <div className={styles.statusContainer}>
            <h2>Status</h2>
            <div className={styles.elementContainer}>
                <p>{isConnected ? `Server running since ${runningSince}` : "Disconnected"}</p>
                <button onClick={shutdown} disabled={!isConnected}>Shutdown</button>
            </div>
        </div>
    );
}