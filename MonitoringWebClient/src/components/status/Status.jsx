import { shutdownServer } from '../../api/api';
import useSocket from '../../hooks/useSocket';
import styles from './Status.module.css';

export function Status() {
    const {isConnected} = useSocket();

    const shutdown = async () => {
        await shutdownServer();
    };

    return (
        <div className={styles.statusContainer}>
            <h2>Status</h2>
            <div className={styles.elementContainer}>
                <p>{isConnected ? "Connected" : "Disconnected"}</p>
                <button onClick={shutdown} disabled={!isConnected}>Shutdown</button>
            </div>
        </div>
    );
}