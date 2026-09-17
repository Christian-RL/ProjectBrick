# ProjectBrick Contributor Guide

## Project goal

ProjectBrick is a LEGO-style brick model-building application. Users should be able to construct models digitally from an accurate, current inventory of brick geometry and available colours, then generate clear step-by-step building instructions so the model can be recreated with physical pieces.

The long-term product may also help users obtain the required physical pieces through retailer or marketplace links, or a prepared shopping cart. A hosted website for downloading the application is also planned, but neither commerce nor the website is a current priority.

LEGO is a trademark of the LEGO Group. Do not imply endorsement or affiliation, and do not add third-party models, instructions, branding, or catalog data until their licence and permitted use have been checked.

## Technical direction

- Unity owns the desktop application, 3D interaction, rendering, scenes, and GUI.
- C# contains the application and domain logic, including model construction, connectivity, persistence, validation, and instruction presentation.
- Python will be used for AI research, training, evaluation, and inference services where appropriate.
- Keep the boundary between Unity/C# and Python explicit and replaceable, using a documented data format or service API rather than embedding Python assumptions throughout Unity code.
- SQLite is the current local persistence technology. Keep database access isolated from scene and UI code.

## Current priorities

1. Make brick placement, connection, removal, save, and load behavior reliable.
2. Define a stable internal representation for parts, colours, connections, submodels, and complete builds.
3. Evaluate lawful, maintainable sources for part geometry, identifiers, colour availability, and example instructions. Record provenance and licence terms for imported data.
4. Build import and validation tooling so catalog updates are repeatable rather than manual.
5. Establish a deterministic instruction-generation baseline before adding AI. AI output must be checked for build order, stability, visibility, inventory accuracy, and physical feasibility.

## Catalog data direction

- Use the official LDraw Parts Library as the preferred source for part geometry, identifiers, headers, categories, attribution, and rendering colour definitions.
- Evaluate the LDCad Shadow Library as the preferred source for connection and snapping metadata. Keep its CC BY-SA data and derivatives identifiable so attribution and share-alike obligations can be met.
- Treat Rebrickable as a provisional enrichment source for external identifiers, elements, set inventories, and historically observed part-colour combinations. Record and approve its applicable terms before redistributing derived data in a public build.
- Do not assume that a rendering colour is physically available for every part. Store part designs, colours, physical elements, aliases, and observed part-colour combinations as distinct concepts.
- Preserve the source release, download URL, hashes, author, licence, attribution, importer version, and validation status for imported catalog records.
- Generate Unity meshes and catalog assets before runtime through a repeatable offline import pipeline. Runtime code should consume versioned generated assets rather than parse the full source library at startup.
- Treat render geometry, collision geometry, occupancy, and connection metadata as separate representations.
- Begin catalog work with a representative fixture set before attempting a full-library import. Report missing geometry, mappings, connectors, licences, and unsupported special parts rather than silently guessing.

## Development stages

Track major stages as GitHub milestones and document each stage under `docs/stages/`. The planned sequence is:

1. Project foundation and repository governance.
2. Data-source and licence validation.
3. Catalog schema and import pipeline.
4. Unity mesh, material, and part-asset generation.
5. Connections, snapping, collision, and construction validation.
6. Builder experience and persistence maturity.
7. Deterministic instruction generation and validation.
8. AI-assisted instruction generation.
9. Physical-parts and purchasing integrations.
10. Public download website and distribution.

Existing prototype functionality does not make a stage complete. Close a stage only when its documented acceptance criteria, tests, limitations, and exit criteria are satisfied.

## GitHub development process

- All features, bugs, research spikes, data-source changes, documentation work, and maintenance work begin with a GitHub issue.
- Use parent issues and sub-issues when a feature contains independently reviewable work. Use milestones for major development stages.
- Every working branch must start from the latest `main` and reference its issue: `feature/<issue>-<slug>`, `fix/<issue>-<slug>`, `spike/<issue>-<slug>`, `docs/<issue>-<slug>`, or `chore/<issue>-<slug>`.
- Keep branches short-lived. Do not maintain a permanent `develop` branch and do not push directly to `main`.
- Use Conventional Commits with a useful scope, for example `feat(catalog): parse LDraw headers` or `fix(sqlite): package the Windows x64 plugin`.
- Keep every commit atomic: one coherent change, directly relevant tests and documentation, no unrelated formatting, and a working or structurally valid repository state.
- Preserve meaningful atomic commits and prefer rebase merging for a linear `main` history. Squash only when preserving the branch commits adds no review value.
- Every branch must be merged through a GitHub pull request that links and closes its issue, explains the behavior and approach, lists validation, identifies documentation and data-licence effects, and supplies screenshots for visible Unity changes.
- Required CI must pass and all review conversations must be resolved before merge.
- The repository owner performs the final review and merge. Agents must never merge a pull request, enable auto-merge, push directly to `main`, or bypass branch protection.
- Delete merged branches only after confirming that the change is present on `main`.

## Authorship and credentials

- Use only the repository owner's configured Git name, GitHub-verified email, and authenticated GitHub account for commits, pushes, issues, and pull requests.
- Before the first write operation, verify `git config user.name`, `git config user.email`, and the authenticated GitHub account. Stop if they do not identify the repository owner.
- Do not add Codex, AI, bot, generated-by, or co-author attribution to source files, branches, commits, issues, pull requests, or release notes.
- Never request, display, write, or commit passwords, personal access tokens, Unity credentials, signing keys, API secrets, or private training data. The repository owner must enter secrets directly into the relevant secure settings.

## Pull request and CI requirements

Protect `main` with a GitHub ruleset that requires a pull request, required status checks, resolved conversations, an up-to-date branch, and linear history, while blocking force pushes and deletion. For a single-owner repository, use zero required GitHub approvals because GitHub does not allow a pull-request author to approve their own PR; the owner-only manual merge is the review gate.

The pull-request pipeline should include, as applicable:

- Branch-name, linked-issue, pull-request-title, prohibited-file, Unity `.meta`, large-file, secret, and data-provenance policy checks.
- Markdown and documentation checks.
- Unity compilation plus EditMode and PlayMode tests using the Unity version recorded by the project.
- A Windows x64 build that verifies the managed SQLite assembly and native x64 SQLite library are packaged correctly.
- Python formatting, linting, type checking, tests, and deterministic catalog-fixture generation when Python tooling is present.
- Uploaded test results, build logs, and temporary build artifacts without publishing or deploying from a pull request.

Give GitHub Actions read-only permissions by default, pin third-party actions to immutable commit SHAs, keep secrets out of forked or untrusted jobs, and never let CI merge automatically.

## Documentation process

- Keep `README.md` as the concise project entry point and put contributor workflow in `CONTRIBUTING.md`.
- Maintain `docs/vision.md`, `docs/roadmap.md`, development setup and testing guides, architecture documentation, and Architecture Decision Records for significant or difficult-to-reverse decisions.
- Maintain a data-source register recording each provider, dataset, URL, version or download date, licence, attribution, redistribution conditions, update process, internal consumers, and approval status.
- Give every major stage a document containing its goal, user outcome, dependencies, scope, exclusions, architecture, data and licence considerations, parent and sub-issues, acceptance criteria, test plan, exit criteria, delivered pull requests, known limitations, and final status.
- Update documentation in the same pull request when behavior, architecture, schemas, save formats, setup, build steps, user workflows, data sources, licences, or stage criteria change.
- Update `CHANGELOG.md` from merged pull requests when preparing a release rather than adding conversational development history to it.

## Engineering guidelines

- Prefer the smallest change that solves the current problem; avoid unrelated refactors.
- Preserve existing Unity asset GUIDs and always include matching `.meta` files for assets.
- Do not edit generated Unity folders or IDE files such as `Library/`, `Temp/`, `Logs/`, `obj/`, solution files, or generated project files unless the task specifically requires it.
- Keep domain logic independent of Unity UI where practical so it can be tested without running a scene.
- Use stable external part and colour identifiers, while maintaining internal IDs where needed. Avoid identifying parts only by display names.
- Validate imported geometry, connection points, dimensions, colour mappings, duplicate IDs, and source provenance.
- Treat generated instructions as untrusted until they pass deterministic validation. Prefer reproducible outputs and retain enough metadata to explain how an instruction sequence was produced.
- Never commit credentials, private API keys, model-provider secrets, or proprietary training data.
- Add focused tests for construction rules, serialization, catalog imports, and instruction validation when those areas change.
- Do not commit complete upstream datasets or large generated catalogs until their distribution, update, storage, and licence strategy has been approved. Use small deterministic fixtures for tests.

## Out of scope for now

- Building and hosting the public download website.
- Retailer integrations, affiliate links, pricing, or automated shopping carts.
- Large-scale AI training before the data model, licensed datasets, baseline instruction generator, and evaluation criteria are defined.

When a task requires a product or architecture choice not covered here, document the assumption in the change and favor designs that keep data providers, AI providers, and commerce providers replaceable.
