# Git and review workflow

All ProjectBrick work begins with a GitHub issue and ends in an owner-reviewed pull request. The issue records intent and acceptance criteria; commits record coherent implementation steps; the pull request records the final review and validation evidence.

## Branches and commits

Create branches from the latest `main` using the format defined in `CONTRIBUTING.md`. Keep branches short-lived and use parent and sub-issues instead of growing a branch beyond one reviewable outcome.

Use Conventional Commits. A commit is atomic when it contains one understandable change, includes its directly relevant test and documentation updates, and leaves the repository in a working or structurally valid state.

## Review ownership

All GitHub activity uses the repository owner's configured Git identity and authenticated account. GitHub does not allow a pull-request author to approve their own request, so the ruleset requires zero formal approvals. The manual control is stricter operationally: only the repository owner performs the final review and merge, and automation never merges.

## Merge and release history

Prefer rebase merging so meaningful branch commits remain visible in a linear history. Use squash merging only when the intermediate commits have no lasting review value. Delete a branch only after confirming the change is present on `main`.

Use Semantic Versioning for distributable releases. Build release notes from merged pull requests and update `CHANGELOG.md` during release preparation.
