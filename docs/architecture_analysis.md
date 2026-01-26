# P2P Bank - Quick Notes

## What

TCP banking server. Multiple banks can talk to each other. Port 65525 default.

## How

1. Client connects via TCP
2. Server spawns new thread for each client
3. Client sends command, gets response, repeat
4. 30 sec timeout then disconnect

Thread per client model - simple but works fine for this.

## Commands

- BC - get bank IP
- AC - create account
- AD/AW/AB - deposit/withdraw/balance (can proxy to other banks)
- AR - delete account
- BA - total money
- BN - account count

Format: `AD 12345/192.168.1.5 100` the IP after slash says which bank to proxy to

## Proxy

If command targets different bank IP, it forwards the request there automaticaly and returns their response. Thats the "p2p" part.

## Database

MySQL, one table called `accounts` with account_number, balance, created_at. Basic repository class, no ORM.

## Thread safety

BankService uses lock() for writes. Multiple clients = multiple threads so yeah.

## Monitoring

Theres a separate web app for watching whats happening in real time.

Components:
- **MonitoringWebApi** - .NET API that connects to bank server and exposes websocket
- **MonitoringWebClient** - React frontend, shows logs as they come in

Special commands (not for normal clients):
- `LISTENER` - tells bank to stream logs to this connection
- `SHUTDOWN` - stops the bank server

## Logger subscribers

Logger uses subscriber pattern. When you log something, all subscribers get it:
- ConsoleLoggerSubscriber - prints to console
- FileLoggerSubscriber - writes to file
- StreamLoggerSubscriber - writes to TCP stream (for monitoring)

This way you can have logs go to multiple places at once.
