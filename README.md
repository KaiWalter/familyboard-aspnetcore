# familyboard-aspnetcore

FamilyBoard in ASP.NET Core

## Hints

before adding secrets to `appsettings.Development.json` remove it from Git tracking:

```
git update-index --assume-unchanged appsettings.Development.json
```

revert

```
git update-index --no-assume-unchanged appsettings.Development.json
```