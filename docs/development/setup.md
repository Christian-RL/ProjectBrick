# Local development setup

## Required software

- Git and GitHub CLI.
- Unity Hub with Unity `6000.5.0f1` installed.
- Visual Studio with the Managed Game development workload, or another Unity-compatible C# editor.
- PowerShell 7 for repository scripts.

The repository root contains `.vsconfig` once repository hygiene issue #16 is delivered. Visual Studio can use it to install the required workload.

## Clone and open

1. Clone `https://github.com/Christian-RL/ProjectBrick.git` over HTTPS.
2. Confirm `git config user.name` and `git config user.email` identify the repository owner before making changes.
3. Open the repository folder in Unity Hub with Unity `6000.5.0f1`.
4. Allow package import and script compilation to finish.
5. Confirm the Console contains no compiler errors before entering Play Mode.

Do not add generated `Library`, `Temp`, `Logs`, `obj`, build, IDE, database, licence, or credential files.

## Current Stage 0 limitations

The current prototype has unverified local SQLite binaries and multiple candidate scenes. Issue #16 will establish the reproducible SQLite dependency and canonical builder scene. Do not copy an unknown DLL into a pull request while that work remains open.

Unity Personal tests and builds run locally until issue #10 provides a supported automated runner.
