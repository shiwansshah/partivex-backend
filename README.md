The tasks commited by Nawaraj Thapa belongs to user shiwansshah (used organizational gitlab account)

All the commits by Nawaraj Thapa belongs to User: shiwansshah

## Build prerequisites

- Install the .NET 10 SDK.
- Ensure NuGet package restore can reach `https://api.nuget.org/v3/index.json`.

## Build

```powershell
dotnet restore Partivex.slnx
dotnet build Partivex.slnx
```

If the IDE shows many errors across multiple projects at once, the first thing to verify is that package restore completed successfully. The current codebase builds cleanly after restore.
