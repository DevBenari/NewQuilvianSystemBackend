# Completion Review Rules

Before completion, perform and record a review proportionate to the task.

- **Diff review:** inspect every changed file and confirm the diff implements the requested behavior only.
- **Scope review:** verify no unrelated source, dependency, configuration, workflow, migration, or generated-output change was introduced.
- **Regression review:** consider affected callers, routes, contracts, state, workflows, and error paths; run relevant validation.
- **Validation evidence:** list commands actually run and their real outcomes. Never fabricate a PASS result or infer one from an unrun command.
- **Secrets check:** ensure no credential, token, connection string, key, or sensitive configuration value appears in changed files or reports.
- **Shared-file impact:** review shared components, contracts, configuration, and cross-repository consumers when they are changed or affected.
- **Final Git status:** run `git status --short`, identify task changes versus pre-existing changes, and do not stage, commit, or push without explicit authorization.
