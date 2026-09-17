# Repository security

Never commit credentials, private keys, access tokens, passwords, licence files, or local environment files. Store repository and CI secrets in the service's encrypted secret store and keep local development credentials outside the repository.

The pull-request policy rejects common private-key filenames and private-key headers. This check reduces accidental exposure, but it does not make committed credentials safe. Revoke an exposed credential immediately even if a commit or branch is subsequently deleted.

## Credential incident procedure

1. Revoke or rotate the credential at its provider.
2. Stop new repository writes and close open pull requests before rewriting history.
3. Preserve legitimate untracked work without copying the credential.
4. Rewrite every affected branch and tag from a fresh mirror clone.
5. Verify the sensitive paths and key markers are absent before force-pushing.
6. Restore branch protection immediately after the force-push.
7. Remove obsolete branches and replace every tainted local clone.
8. Ask GitHub Support to remove cached views and pull-request references when required.

## 2026-09-17 remediation record

- The exposed SSH credential was revoked before the repository was rewritten.
- `ProjectBrickToken` and `ProjectBrickToken.pub` were removed from all reachable branch and tag history with `git-filter-repo` 2.47.0.
- The first changed commit reported by the tool was `61ccec48cddf518ec5be2013bcb013524f6db4a3`.
- GitHub rejected updates to its read-only pull-request refs for pull requests 3, 7, 9, and 11. GitHub Support must dereference those refs and clear cached views.
- No Git LFS objects were involved.
- `Protect Main` was restored and the obsolete `development` branch was deleted after the cleaned `main` branch was verified.
