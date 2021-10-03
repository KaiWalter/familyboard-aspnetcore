# FamilyBoard in ASP.NET Core

This implementation of a family board - displaying a random images selected from `OneDrive` and combining various `Outlook` calendars is a reduced port of <https://github.com/KaiWalter/family-board-py>.

## Hints

before adding secrets to `appsettings.Development.json` remove it from Git tracking:

```shell
git update-index --assume-unchanged appsettings.Development.json
```

get it back into tracking

```shell
git update-index --no-assume-unchanged appsettings.Development.json
```

## documentation backlog

[ ] app registrations: <https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade>
[ ] certificate approach
