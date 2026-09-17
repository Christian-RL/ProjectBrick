# Continuous integration

## Current required pipeline

The `Pull request policy` workflow runs for pull requests targeting `main`. It verifies:

- issue-linked branch naming;
- Conventional Commit pull-request titles;
- an issue-closing reference in the pull-request body;
- exclusion of generated and user-local Unity directories;
- paired Unity assets and `.meta` files;
- deletion of Unity assets together with their metadata; and
- explicit review of files larger than 50 MB.

The workflow has read-only GitHub permissions, cancels superseded runs for the same pull request, and pins third-party actions to immutable commit SHAs.

## Unity pipeline status

Automated Unity compilation, tests, and Windows builds are temporarily deferred. Unity currently does not support manual licence-file activation for Personal plans, while the common GameCI hosted-runner process depends on that activation path. Unity validation is therefore a documented local pull-request requirement for now.

The preferred future solution is a secured self-hosted Windows runner operating under an activated Unity user, restricted to trusted ProjectBrick branches. Unity Build Automation or a serial-based paid licence are alternatives.

The future required Unity pipeline will add:

1. compilation and EditMode tests;
2. PlayMode tests;
3. deterministic catalog fixture generation;
4. a Windows x64 build;
5. SQLite managed/native plugin packaging checks; and
6. test results, logs, and temporary build artifacts.

CI never publishes a pull-request build and never merges a pull request.
