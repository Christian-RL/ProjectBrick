# Testing

Use the smallest test level that proves the changed behavior, then run broader Unity checks when the change affects scenes, persistence, plugins, or builds.

## Current validation

The Unity Test Framework package is installed, but test assemblies and the command-line runner are tracked by issue #18. Until that issue is delivered:

1. Open **Window > General > Test Runner** in Unity.
2. Run all available EditMode tests.
3. Run all available PlayMode tests.
4. Open the affected scene and exercise its documented smoke test.
5. Build Windows x64 when changing build settings, persistence, or native plugins.
6. Record the Unity version, tests, build target, and result in the pull request.

## Test ownership

- Domain rules use EditMode tests and must not require a scene.
- Scene adapters, GameObjects, input, rendering, and lifecycle behavior use PlayMode tests.
- SQLite tests use a temporary database and delete it after the test.
- Catalog import tests use small deterministic fixtures with recorded provenance.
- Visible changes include screenshots or a short recording in the pull request.

Tests must not write databases, generated scenes, logs, or build output into tracked paths.
