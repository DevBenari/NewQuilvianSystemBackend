# Application Versioning V2

The backend application release is defined only by the repository-root `version.json`. API contract routing remains independently versioned (`v1`). The backend does not own or store the frontend application's actual version.

`FrontendCompatibility:MinimumSupportedVersion` is a compatibility threshold. It changes only when the backend stops supporting an older frontend, not whenever a frontend is released.

Expected frontend flow:

1. The frontend owns its version, for example `FrontendVersion = 2.1.12`.
2. It requests `GET /api/v1/Version/version`.
3. The backend returns `backendVersion = 0.0.1` and `minimumSupportedFrontendVersion = 2.1.11`.
4. The frontend compares its own version with the minimum: `2.1.12 >= 2.1.11` is compatible; `2.1.10 < 2.1.11` requires an update.

`backendVersion` must never be used as `FrontendVersion`. The response fields `frontendMinimumVersion` and `frontendRecommendedVersion` are temporary aliases of `minimumSupportedFrontendVersion` for the existing frontend and do not have independent configuration or business meaning.
