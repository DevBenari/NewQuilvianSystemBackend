# Completion Review Rules

Before completion, perform and record a review proportionate to the task.

- **Diff review:** inspect every changed file and confirm the diff implements the requested behavior only.
- **Scope review:** verify no unrelated source, dependency, configuration, workflow, migration, or generated-output change was introduced.
- **Regression review:** consider affected callers, routes, contracts, state, workflows, and error paths; run relevant validation.
- **Validation evidence:** list commands actually run and their real outcomes. Never fabricate a PASS result or infer one from an unrun command.
- **Validation classification:** report each relevant check as **PASS**, **NEW ERROR** (introduced by or clearly caused by the current diff), **EXISTING / ENVIRONMENT ISSUE** (reproducible or evidence-supported and unrelated), or **NOT RUN** (intentionally not required). Do not call a failure existing without evidence. A task with an existing/environment issue remains reviewable only when the changed scope is independently reviewed, the failure is not caused by the task, and the remaining risk is reported.
- **Secrets check:** ensure no credential, token, connection string, key, or sensitive configuration value appears in changed files or reports.
- **Shared-file impact:** review shared components, contracts, configuration, and cross-repository consumers when they are changed or affected.
- **Blueprint review:** treat blueprint artifacts as tracked documentation changes. Confirm architecture and status claims cite evidence, `MODULE-STATUS` does not mark a module `DONE` without verification evidence, and a phase is not `DONE` merely because files exist. Report stale evidence, blocked phases, and independently continuable phases distinctly.
- **Final Git status:** run `git status --short`, identify task changes versus pre-existing changes, and do not stage, commit, or push without explicit authorization.
