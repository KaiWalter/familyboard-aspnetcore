# FamilyBoard in ASP.NET Core

This implementation of a family board - displaying a random images selected from `OneDrive` and combining various `Outlook` calendars is a reduced port of <https://github.com/KaiWalter/family-board-py>.

> _As we do not use `Google Calendar` anymore the support for it did not make it into this version._

## What I want to achieve / to learn

[ ] use the **Microsoft Graph .NET SDK** but with an almost **headless approach** - when the board is logged in once the access token is persited so that even when the machine hosting the board is rebooted the authentication would still work
[x] get started with **GitHub Actions**
[x] host **ASP.NET Core** in **Docker** on a **Raspberry Pi 3B**

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

 [ ] [That implementation doesn&#39;t look thread safe. (@davidfowl)](https://twitter.com/davidfowl/status/1439272866579562496)

## documentation backlog

[ ] app registrations: <https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade>
[ ] certificate approach
