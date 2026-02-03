# Changelog

## 2026-02-03
* Oprava různých bugů

## 2026-01-29 - Jan Koubek
* Oprava bugů, zajištění správnosti komunikace
* Předělání private key v .env na konfigurační soubor
* Implementace ověření připojení monitoring klienta

## 2026-01-28 - Jan Koubek
* Implementace šifrování komunikace mezi BankServer a MonitoringWebApi
* Implementace zašifrování výzvy

## 2026-01-27 - Lukáš Hrehor
* Fix websocket listenerů po reconnectu
* Retry logika pro bank connection v log endpointu
* Fix .env nenačítání z jiné složky
* Fix reconnectu monitoringu při restartu bank serveru

## 2026-01-27 - Jan Koubek
* Fix spamování logů při vypnutí bank nodu

## 2026-01-26 - Jan Koubek
* Komprese TCP listener komunikace
* Shutdown na webu
* Fix multithreadingu BankConnectionService
* WebSocket hook + bugfixy
* Vizuální vylepšení webu
* Fix timeout bugu
* Dokumentace

## 2026-01-26 - Lukáš Hrehor
* .env config podpora
* Timeout na 30 sec
* Interface pro DB connection
* Readme a docs

## 2026-01-25 - Jan Koubek
* WebSocket pro logy v API i na webu
* Log komponenta
* Připojení k bank serveru v API
* Init web client projektu
* Fix odpojování klientů

## 2026-01-25 - Lukáš Hrehor
* Proxy pro AD, AW, AB

## 2026-01-24 - Lukáš Hrehor
* BankClient pro proxy requesty

## 2026-01-24 - Jan Koubek
* Init Web API projektu
* Logger subscriber systém
* Přesun projektu do vlastní složky

## 2026-01-23 - Lukáš Hrehor
* Úpravy configu a loggeru

## 2026-01-21 - Lukáš Hrehor
* Cleanup

## 2026-01-20 - Lukáš Hrehor
* Fix account modelu
* Logging do souboru

## 2026-01-19 - Lukáš Hrehor
* Thread safety fix
* Overflow fix u vkladů

## 2026-01-18 - Lukáš Hrehor
* Smazány debug printy
* Validace čísla účtu opravena
* BankInfo model

## 2026-01-17 - Lukáš Hrehor
* Timeout fix
* Main program hotov
* ClientHandler
* TCP server

## 2026-01-16 - Lukáš Hrehor
* CommandParser
* AR, BA, BN příkazy
* AB příkaz
* AD, AW příkazy
* ICommand, BC, AC

## 2026-01-15 - Lukáš Hrehor
* Logger
* BankService hotov
* AccountRepository
* DB connection

## 2026-01-14 - Lukáš Hrehor
* Config třída
* Account model
* Init projektu
