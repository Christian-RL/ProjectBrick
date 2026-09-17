# Architecture overview

ProjectBrick uses Unity for its desktop runtime and presentation, C# for domain and application behavior, SQLite for local persistence, and future Python services for AI research and inference.

## Intended dependency direction

```text
ProjectBrick.Unity
    -> ProjectBrick.Infrastructure.Sqlite
    -> ProjectBrick.Application
    -> ProjectBrick.Domain
```

- **Domain** owns logical bricks, connectivity, model structure, and validation rules.
- **Application** owns use cases, persistence contracts, and transport-neutral model snapshots.
- **Infrastructure.Sqlite** implements persistence without creating Unity GameObjects.
- **Unity** owns scenes, `MonoBehaviour` adapters, input, rendering, selection, snapping presentation, and conversion between scene objects and application snapshots.

Dependencies point inward. Domain and Application code must not reference scenes, UI objects, or SQLite. Python integrations must cross a documented data or service boundary rather than becoming assumptions inside Unity components.

## Current prototype

The prototype still compiles into Unity's default assembly. Logical model classes use Unity value types, global registries hold scene state, and `BrickDatabaseService` combines SQL with scene discovery and GameObject creation. Issue #17 introduces the first assembly and persistence seams without redesigning all domain types.

Significant or difficult-to-reverse changes require an Architecture Decision Record.
