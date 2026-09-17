# Contributing to ProjectBrick

ProjectBrick uses an issue-first, short-lived branch workflow. `main` is protected and changes reach it only through a pull request reviewed and merged by the repository owner.

## Workflow

1. Create or select a GitHub issue with scope, acceptance criteria, validation, and documentation expectations.
2. Split independently reviewable work into sub-issues.
3. Branch from the latest `main` using `<type>/<issue>-<lowercase-slug>`.
4. Make atomic Conventional Commits with focused tests and documentation.
5. Push the issue branch and open a pull request that closes its issue.
6. Resolve required checks and review conversations.
7. The repository owner reviews and merges the pull request. Contributors and agents must not merge or enable auto-merge.

Allowed branch types are `feature`, `fix`, `spike`, `docs`, and `chore`. Example: `feature/42-ldraw-mesh-import`.

Commit examples:

```text
feat(catalog): parse LDraw part headers
fix(sqlite): package the Windows x64 native library
test(connections): cover rotated stud alignment
docs(data): record LDraw attribution requirements
ci(policy): validate issue-linked pull requests
```

Keep one coherent change in each commit. Include directly relevant tests and documentation, preserve Unity `.meta` files, and avoid unrelated reformatting.

## Pull requests

Pull request titles follow Conventional Commits and the body must contain `Closes #<issue>`, `Fixes #<issue>`, or `Resolves #<issue>`. Include validation evidence, documentation and data-source effects, risks, and screenshots for visible Unity changes.

Rebase merging is preferred to preserve meaningful atomic commits and maintain linear history. Do not push directly to `main`, force-push `main`, bypass required checks, or commit credentials.

## Local Unity validation

Until a supported Unity Personal CI activation path or secured Windows runner is available, Unity checks are performed locally with the project version recorded in `ProjectSettings/ProjectVersion.txt`.

For relevant changes:

1. Open the project and allow Unity to finish importing and compiling.
2. Confirm that the Console has no compiler errors.
3. Run applicable EditMode and PlayMode tests.
4. Build Windows x64 when changing scenes, build settings, persistence, or native plugins.
5. Record the checks and results in the pull request.

Do not add Unity credentials to source control or issue discussions.
