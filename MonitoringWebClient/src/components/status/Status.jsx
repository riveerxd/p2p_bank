import styles from './Status.module.css';

export function Status() {
    return (
        <div className={styles.statusContainer}>
            <h2>Status</h2>
            <div className={styles.elementContainer}>
                <p>Status: Online</p>
                <button>Shutdown</button>
            </div>
        </div>
    );
}