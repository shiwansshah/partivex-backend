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

## Parts Feature QA Checklist

- `POST /api/parts` creates a part with unique `partCode`, non-negative `price`, and non-negative `stock`.
- `PUT /api/parts/{id}` updates an active part and enforces the same validation rules.
- `GET /api/parts` returns active parts and supports `searchTerm`, `pageNumber`, `pageSize`, `sortBy`, and `sortDirection`.
- `GET /api/parts/{id}` returns the part when active, otherwise returns `404`.
- `DELETE /api/parts/{id}` soft-deletes only when `stock` is `0`; returns a validation error when stock is greater than `0`.
- Re-creating a part with a code used by an inactive record is allowed.
