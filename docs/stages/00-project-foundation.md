# Stage 0: Project foundation

## Goal

Create a reliable development foundation in which every change is issue-driven, reviewable, tested in proportion to risk, documented, and merged into protected `main` only by the repository owner.

## Included scope

- Repository contributor guidance and development documentation.
- Issue and pull-request templates.
- Issue-linked branch and atomic commit conventions.
- Protected `main` rules and owner-only merging.
- Required repository-policy checks.
- A documented path to automated Unity testing and Windows builds.
- Assessment of the existing Unity prototype against later stage requirements.

## Excluded scope

- New builder features.
- Full LDraw catalog ingestion.
- AI instruction generation.
- Public deployment or releases.

## Parent issue

- [#8 Establish repository governance, documentation, and CI foundation](https://github.com/Christian-RL/ProjectBrick/issues/8)

## Acceptance and exit criteria

- [ ] Contributor and Git workflow documentation is merged.
- [ ] Issue and pull-request templates are available on GitHub.
- [ ] `main` requires pull requests, current required checks, resolved conversations, and linear history.
- [ ] Force pushes and deletion of `main` are blocked.
- [ ] GitHub Actions has read-only default permissions.
- [ ] The repository-policy check passes on its own pull request and is required by the ruleset.
- [ ] Unity Personal CI licensing limitations and local validation are documented.
- [ ] The repository owner completes final review and verifies the merge process.

## Known limitations

- Automated Unity tests and builds are not yet available on a hosted runner because the project currently uses Unity Personal and its supported activation path does not provide the licence-file flow expected by the common GameCI setup.
- Existing project files and prototype behavior still require a focused audit after the governance foundation is merged.

## Delivered pull requests

Record merged Stage 0 pull requests here as the stage progresses.
