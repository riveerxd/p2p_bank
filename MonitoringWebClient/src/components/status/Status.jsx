import useSocket from '../../hooks/useSocket';
import styles from './Status.module.css';

export function Status() {
    const {isConnected} = useSocket();

    return (
        <div className={styles.statusContainer}>
            <h2>Status</h2>
            <div className={styles.elementContainer}>
                <p>{isConnected ? "Connected" : "Disconnected"}</p>
                <button>Shutdown</button>
            </div>
        </div>
    );
}