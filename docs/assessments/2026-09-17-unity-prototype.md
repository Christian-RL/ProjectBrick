# Unity prototype assessment — 2026-09-17

- Issue: #15
- Unity: `6000.5.0f1 (88b47c5e7076)`
- Platform: Windows x64
- Intended builder baseline: `Assets/SceneConfig.unity`
- Assessment type: code inspection, batch import/compile, scene inspection, and isolated headless Play Mode smoke test

## Outcome

The prototype provides a working vertical slice for rectangular brick creation, logical connectivity, graph traversal, and SQLite save/load. Unity imported and compiled the project successfully. The selected `SceneConfig` scene loaded with no missing script references, and an isolated runtime smoke test successfully created two 2x2 bricks, connected them, saved model ID 1, loaded it directly, and loaded it again as the most recent model.

This evidence does not make the prototype production-ready. The current build scene is not the selected builder scene, important interaction paths remain manually unverified, persistence can leave partial or lost state on failure, and the local SQLite binaries have no recorded source.

## Verified behavior

| Area | Result | Evidence |
| --- | --- | --- |
| Unity import and compilation | Pass | Batch-mode import completed with return code 0 and no production compiler errors |
| Scene references | Pass | `SceneConfig` contained the sidebar, main camera/controller, database service, and no missing scripts |
| Visual brick construction | Pass | A 2x2 brick produced one body and four stud children with `BrickObjectData` |
| Logical connection | Pass | `BrickModelRegistry` recorded one connection and `BrickModelGraph` traversed two nodes |
| SQLite initialization | Pass | An isolated temporary database was created successfully |
| Save and direct load | Pass | A two-brick connected model saved as ID 1 and restored its connection |
| Most-recent load | Pass | The same model and connection were restored through `LoadMostRecentModel` |
| Existing user database | Preserved | It was backed up before the smoke test and restored with a matching SHA-256 afterward |

The runtime smoke test called the construction and connection APIs directly. It did not simulate mouse or keyboard input.

## Scene and repository findings

- `Assets/SceneConfig.unity` is the most complete builder candidate because it contains the sidebar, camera controller, and `BrickDatabaseService`, but it is currently untracked.
- `Assets/MainSetup.unity` is also untracked and lacks the database service.
- `Assets/Scenes/SampleScene.unity` is the only enabled build scene. It contains `BrickTestSpawner` and does not represent the selected product baseline.
- The Unity tutorial readme and layout assets remain in the project.
- `.vsconfig`, the SQLite plugin directory, and `ProjectSettings/SceneTemplateSettings.json` are untracked.
- There are no assembly definition files or test assemblies.

Issue #16 owns canonical-scene selection, tracked project settings, plugin provenance, and removal of confirmed duplicate scaffolding.

## Architecture findings

- Logical types, Unity components, input, UI, global scene state, and persistence compile into Unity's default assembly.
- `Brick`, `Stud`, and `BasicBrick` use the global namespace while neighboring code uses `BrickCode`, `ModelCode`, `MenuCode`, and `SaveLoadCode` namespaces.
- The logical `Brick` model depends on `UnityEngine.Color`.
- `BrickModelRegistry` is a static global store for scene objects and connections.
- `BrickDatabaseService` combines schema creation, SQL, scene discovery, snapshot mapping, GameObject construction, selection cleanup, and lifecycle ownership in one `MonoBehaviour`.
- The UI calls the database singleton directly.

Issue #17 owns incremental assembly boundaries and the persistence seam. The catalog stage will later replace prototype display-name and RGBA identity with stable part and colour identifiers.

## Persistence risks

1. `SaveCurrentModel` does not use a transaction. A failure can leave a saved-model row with only some bricks or connections.
2. Foreign keys are declared but the connection never enables `PRAGMA foreign_keys = ON`.
3. `LoadModel` clears the active scene before verifying that the requested model can be fully read. Issue #22 tracks preserving the active model on failure.
4. Schema versioning and migrations are absent.
5. Every loaded part becomes `BasicBrick`; subtype or catalog identity is not reconstructed.
6. SQL and scene reconstruction cannot be tested independently.

Issue #23 tracks transactional saves, enforced foreign keys, and rollback coverage.

## Connection-model risks

The prototype records both upper and lower attachment coordinates against the same top-stud array. This is sufficient for the current two-brick smoke test but cannot express independent studs, tubes, anti-studs, clips, bars, or directional connector occupancy. Issue #24 tracks the connection-point model needed before catalog connector metadata is imported.

Coordinate bounds are not validated at the public connection boundary, and disconnect logic searches all neighbor studs for matching brick references. These behaviors need focused tests before later snapping work expands.

## SQLite dependency findings

The local plugin files have no known source and must be replaced rather than trusted:

| File | Reported version | SHA-256 |
| --- | --- | --- |
| `Mono.Data.Sqlite.dll` | 1.0.61.0 | `6B98EA1B06DE98234A29AF4692BFAE7285221EAB77AD9CC8004051BEF69837FF` |
| `sqlite3.dll` | 3.53.3 | `79FD9EC89DBA3F8BD64529A2CA8E9DDE6AE6EDC486C55A1D3F1CE77975A8375C` |

The native plugin metadata enables Windows Editor x64 and Standalone Windows x64. A clean-clone compile and player packaging check are still required after issue #16 installs verified artifacts.

## Not yet verified

- Sidebar cursor placement and cancellation through real mouse input.
- Single and connected-structure selection.
- Dragging, collision sliding, snap candidate selection, and snap commit through input.
- Rotation mode and rotation snapping.
- Deleting a selected brick through the UI.
- Visible material and outline behavior in a rendered editor session.
- Persistence failure handling, rollback, migration, and malformed database behavior.
- Windows x64 player build and packaged SQLite native loading.

Issue #18 will establish repeatable EditMode and PlayMode coverage. Visible interaction checks remain manual until an appropriate Unity input-testing layer is added.

## Stage 0 recommendation

Retain the prototype as behavioral evidence. Do not extend its global assembly or persistence coupling. Normalize the repository through #16, introduce the smallest testable boundaries through #17, and lock the verified construction/connectivity/save-load path with #18 before starting catalog ingestion.
