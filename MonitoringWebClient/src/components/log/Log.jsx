import styles from "./Log.module.css";

export function Log(props) {
    return (
        <div className={styles.logContainer}>
            {props.message}
        </div>
    );
}