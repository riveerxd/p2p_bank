# P2P Bank Diagrams

## Proxy Flow

This is how banks talk to eachother. If you try to deposit to an account on another bank, your bank forwards the request.

```mermaid
sequenceDiagram
    participant C as Client
    participant A as Bank A (10.0.0.1)
    participant B as Bank B (10.0.0.2)

    C->>A: AD 55555/10.0.0.2 1000
    Note over A: IP doesn't match mine,<br/>forward to other bank
    A->>B: AD 55555/10.0.0.2 1000
    Note over B: IP matches, execute locally
    B->>B: deposit 1000 to account 55555
    B-->>A: AD
    A-->>C: AD
```

## Connection Handling

What happens when a client connects to the bank server.

```mermaid
flowchart TD
    A[Client connects] --> B[Server accepts connection]
    B --> C[Spawn new thread]
    C --> D[Set 30sec timeout]
    D --> E{Read command}
    E -->|got command| F[Parse & Execute]
    F --> G[Send response]
    G --> E
    E -->|timeout| H[Close connection]
    E -->|client disconnected| H
    E -->|null/empty| H
    H --> I[Cleanup & log]
```

## Architecture

```mermaid
flowchart TB
    subgraph Clients
        C1[Telnet/PuTTY]
        C2[Other clients]
    end

    subgraph Server
        TCP[TcpBankServer]
        CH[ClientHandler threads]
        CP[CommandParser]

        subgraph Commands
            BC & AC & AD & AW & AB & AR & BA & BN
        end

        BS[BankService]
    end

    subgraph Network
        BC2[BankClient]
        OB[Other Banks]
    end

    subgraph Data
        AR2[AccountRepository]
        DB[(MySQL)]
    end

    C1 & C2 -->|TCP| TCP
    TCP --> CH
    CH --> CP
    CP --> Commands
    Commands --> BS
    Commands -->|proxy requests| BC2
    BC2 -->|TCP| OB
    BS --> AR2
    AR2 --> DB
```

## Command Flow

How a command gets parsed and executed.

```mermaid
flowchart LR
    A[Input: AD 12345/10.0.0.1 500] --> B[Split by whitespace]
    B --> C[Lookup 'AD' in dictionary]
    C --> D[ADCommand.Execute]
    D --> E{Local or proxy?}
    E -->|local| F[BankService.Deposit]
    E -->|proxy| G[BankClient.SendCommand]
    F --> H[Return response]
    G --> H
```

## Monitoring Flow

How the web dashboard gets logs from the bank server.

```mermaid
sequenceDiagram
    participant W as Web Dashboard
    participant A as Monitoring API
    participant B as Bank Server

    W->>A: Connect WebSocket /log
    A->>B: TCP connect
    A->>B: LISTENER
    Note over B: Switches to log streaming mode

    loop Every log event
        B-->>A: [2024-01-26 12:00:00] [CMD] ...
        A-->>W: WebSocket message
    end

    W->>A: GET /shutdown
    A->>B: SHUTDOWN
    Note over B: Server stops
```

## Logger Subscriber System

The logger uses a subscriber pattern so multiple things can receive logs at the same time.

```mermaid
flowchart TD
    L[Logger] --> C[ConsoleLoggerSubscriber]
    L --> F[FileLoggerSubscriber]
    L --> S[StreamLoggerSubscriber]

    C --> Console
    F --> bank.log
    S --> TCPStream
```

When something calls `logger.LogInfo("whatever")`, all subscribers get notified. The StreamLoggerSubscriber is used for the monitoring - it writes to the TCP stream that the API is reading from.
