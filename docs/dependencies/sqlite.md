# SQLite dependencies

- Status: approved for Stage 0 repository use and Windows x64 prototype validation
- Review issue: #16
- Target: Unity 6000.5.0f1, Windows Editor x64, Windows Standalone x64

ProjectBrick uses the `Mono.Data.Sqlite` ADO.NET provider with the official SQLite native library. Both artifacts are committed so a clean clone can compile and build without copying local machine files.

## Managed provider

| Field | Value |
| --- | --- |
| Package | `Mono.Data.Sqlite.Core` 1.0.61.1 |
| Publisher | `ahyun` on NuGet.org |
| Package URL | `https://api.nuget.org/v3-flatcontainer/mono.data.sqlite.core/1.0.61.1/mono.data.sqlite.core.1.0.61.1.nupkg` |
| Package SHA-512 | `JR0B8JiW1e55HPgk7xL8DU6nH23KUAz9p92c1nlq14+ApuT+PpAdeVBiDC/uPDD4m6Wa9EtXuzhKkMS4I/K9OQ==` |
| Package SHA-256 | `8EAF56CA0FF682C4BBFC29A6E3CF81CFA5625FDAB871BD66E3AEF52B9CF16681` |
| Selected asset | `lib/netstandard2.0/Mono.Data.Sqlite.dll` |
| Asset SHA-256 | `FF10AF566149E475D1A59F8592B6FA26A61A2F99DCF0B34ADD5DE47A25EACFCA` |
| Upstream licence | Mono class libraries are distributed under the MIT licence; retain the notice in `THIRD-PARTY-NOTICES.md` |

The NuGet package metadata does not include a licence field or upstream repository URL. Its binary identifies itself as `Mono.Data.Sqlite` version 1.0.61.0. Public distribution must recheck this provenance or replace the provider with an artifact built directly from an approved upstream source.

## Native SQLite library

| Field | Value |
| --- | --- |
| Release | SQLite 3.53.4, Windows x64 |
| Archive URL | `https://www.sqlite.org/2026/sqlite-dll-win-x64-3530400.zip` |
| Published archive SHA3-256 | `deddee963c810d1eeac3ce5e15c7c41da21a1c54d7a39cf54fbf577d2f50de3a` |
| Archive SHA-256 | `8B959B7EFF4A81F6A62FC3468F9273E5CFE78D4A927E62215AED231B654FB104` |
| Selected asset | `sqlite3.dll` |
| Asset SHA-256 | `AB57D0437795ECC757CB693F32EA224173FA9856594D95CFA6B5033E645CD1EC` |
| Licence | SQLite core is dedicated to the public domain: `https://www.sqlite.org/copyright.html` |

The native importer enables the DLL only for the Windows x64 Editor and Windows x64 Standalone player.

## Update procedure

1. Open an issue describing the version change, compatibility target, and security or maintenance reason.
2. Download from the authoritative URLs into a temporary directory outside the repository.
3. Verify the NuGet SHA-512 from NuGet's package metadata and the SQLite SHA3-256 from SQLite's download page.
4. Extract only the selected managed and native assets.
5. Replace the DLL payloads without replacing their Unity `.meta` files.
6. Update every version and hash in this document and the third-party notice when terms change.
7. Run Unity compilation, the SQLite round-trip test, and a Windows x64 build packaging check.
