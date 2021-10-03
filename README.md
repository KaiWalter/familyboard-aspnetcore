# FamilyBoard in ASP.NET Core

This implementation of a family board - displaying a random images selected from `OneDrive` and combining various `Outlook` calendars is a reduced port of <https://github.com/KaiWalter/family-board-py>.

> _As we do not use `Google Calendar` anymore the support for it did not make it into this version._

## What I want to achieve / to learn

[ ] use the **Microsoft Graph .NET SDK** but with an almost **headless approach** - when the board is logged in once the access token is persited so that even when the machine hosting the board is rebooted the authentication would still work
[x] get started with **GitHub Actions**
[x] host **ASP.NET Core** in **Docker** on a **Raspberry Pi 3B**

## my setup

Inspired by [a post from Scott Hanselman](https://www.hanselman.com/blog/how-to-build-a-wall-mounted-family-calendar-and-dashboard-with-a-raspberry-pi-and-cheap-monitor), I mounted a 24" monitor in the kitchen and attached a **Raspberry Pi W Zero** to it. I started with **Dakboard** as mentioned in the post but was immediately inspired (or triggered) to use it as a playground for testing all kinds of implementations:

- [Python with Flask - pull updates from browser](https://github.com/KaiWalter/family-board-py)
- [Python with jyserver - push updates to browser](https://github.com/KaiWalter/family-board-jyserver)
- [Azure Functions with SignalR - push updates to browser](https://github.com/KaiWalter/family-board-lambda-signalr); hosted in the cloud

Over time I added another Pi to the network with [**Pi-hole**](https://pi-hole.net/) on it which led me to shift the backend workload from cloud back into my network and thus help keeping things more isolated:

```text
+------------------------+     +------------------------+
|                        |     |                        |
| kiosk / browser        |     | server in this repo    |
| on Raspberry Pi W Zero |-----| in a Docker container  |
|                        |     | on a Raspberry Pi 3B   |
|                        |     |                        |
+------------------------+     +------------------------+
```

## hints

before adding secrets to `appsettings.Development.json` remove it from Git tracking:

```shell
git update-index --assume-unchanged appsettings.Development.json
```

get it back into tracking

```shell
git update-index --no-assume-unchanged appsettings.Development.json
```

## still to do

 [ ] add thread safety to `DiskCacheHandler` [That implementation doesn't look thread safe. (@davidfowl)](https://twitter.com/davidfowl/status/1439272866579562496)

## documentation backlog

[ ] app registrations: <https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade>
[ ] certificate approach
