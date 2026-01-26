import styles from './App.module.css'
import { LogContainer } from './components/logContainer/LogContainer'
import { Status } from './components/status/Status'

function App() {

  return (
    <>
      <h1>Monitoring</h1>
      <div className={styles.mainContainer}>
        <Status />
        <LogContainer />
      </div>
    </>
  )
}

export default App
