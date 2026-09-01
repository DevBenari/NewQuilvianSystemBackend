# Requirement Traceability — Platform Authorization & Access Control

`blueprint_id`: `SEC-BP-001`

Requirement di bawah diturunkan dari audit Phase 1 dan keputusan owner pada penetapan Phase A0.
Kolom bukti menunjuk artefak yang benar-benar dapat ditelusuri di repository.

| Requirement | Keputusan owner | Backend | Frontend | Bukti target | Status coverage |
|---|---|---|---|---|---|
| `SEC-REQ-001` — Setiap endpoint terproteksi harus punya identitas permission yang dapat didaftarkan dan diberikan admin | `D1` | `BE-SEC-001` | `NOT APPLICABLE` | `PermissionRegistryDescriptor`; `PermissionRegistryInvariantTests`; `CanonicalSecurityContractTests`; source mismatch `89 → 0` | Covered |
| `SEC-REQ-002` — Identitas yang didaftarkan seeder harus identik dengan yang dicari runtime | `D1` | `BE-SEC-001` | `NOT APPLICABLE` | `PermissionRegistryDescriptor.BuildCore`; `SeederIdentityMatchesRuntimeIdentity`; `AuthorizationIdentityAlwaysComesFromAccessPermission` | Covered |
| `SEC-REQ-003` — Registry yang tidak lagi dideklarasikan source harus ditutup tanpa hard delete dan tanpa memindahkan policy | `D4` | `BE-SEC-001` | `NOT APPLICABLE` | `AccessMenuSeeder.CloseRowsAbsentFromSourceAsync`; `ReconcileClosesStaleRegistryRowsWithoutHardDelete`; registry usang `59 → 0` | Covered |
| `SEC-REQ-004` — Izin efektif adalah gabungan seluruh penempatan organisasi yang sah, tanpa DENY precedence | `N3` | `BE-SEC-001` | `NOT APPLICABLE` | `AccessPermissionService.HasAccessAsync`; `OrganizationAuthorizationProjectionTests`; `EffectivePermissionsAreUnionOfActiveAssignments`; smoke test D | Covered |
| `SEC-REQ-005` — Penempatan yang dihapus, dibatalkan, nonaktif, belum berlaku, atau sudah berakhir tidak boleh memberi izin | `N3` | `BE-SEC-001` | `NOT APPLICABLE` | `OrganizationAuthorizationProjectionService.IsAssignmentValid`; `InvalidAssignmentNeverProjects`; `CancelledOrganizationAssignmentDeniesAccess`; smoke test F | Covered |
| `SEC-REQ-006` — Perubahan penempatan lewat jalur resmi HR harus tercermin pada otorisasi, termasuk pencabutannya | `N3` | `BE-SEC-001` | `NOT APPLICABLE` | `WfpOrganizationAssignmentController` (5 mutasi); `DepartmentTransferRevokesStaleProjection`; `DeactivatingAssignmentRevokesProjection`; smoke test E | Covered |
| `SEC-REQ-007` — Skema harus mendukung penempatan berulang, effective dating, riwayat, dan rehire | Persetujuan index | `BE-SEC-001` | `NOT APPLICABLE` | Migration `A0AuthorizationIntegrityProjection`; index terverifikasi pada database development | Covered |
| `SEC-REQ-008` — Satu proyeksi otorisasi harus dapat ditelusuri ke penempatan otoritatif yang menghasilkannya | Persetujuan `SourceAssignmentId` | `BE-SEC-001` | `NOT APPLICABLE` | Kolom `SourceAssignmentId`; index unik terfilter; 47 ter-backfill, 2 legacy-unresolved, 0 ambigu | Partial: 2 baris warisan sengaja dibiarkan `null` |
| `SEC-REQ-009` — Registry tidak boleh membuat pemberian hak otomatis | `D2`, `N2` | `BE-SEC-001` | `NOT APPLICABLE` | `ReconcileNeverCreatesAccessPolicy`; `SysAccessPolicy` 498 → 498, Departemen × Posisi 11 → 11 | Covered |
| `SEC-REQ-010` — Policy yang menunjuk registry tertutup tidak boleh tetap mengotorisasi | `D4` | `BE-SEC-001` | `NOT APPLICABLE` | `StaleRegistryAuthorizationTests` (4 test); smoke test C | Covered |
| `SEC-REQ-011` — Pemulihan grant inert hanya boleh untuk padanan yang terbukti identik, tanpa perluasan hak | Instruksi safe rebind | `BE-SEC-001` | `NOT APPLICABLE` | Klasifikasi 51 `EXACT_EQUIVALENT` / 5 `SEMANTIC_CHANGED` / 12 `REMOVED_CAPABILITY` / 0 `AMBIGUOUS`; 28 rebind, 23 dedupe; bukti CSV per baris | Covered |
| `SEC-REQ-012` — Perilaku SuperAdmin tidak boleh berubah | `N2` | `BE-SEC-001` | `NOT APPLICABLE` | Tiga test SuperAdmin existing tetap hijau; smoke test G | Covered |

---

## Coverage Gap

Tidak ada gap requirement internal Phase A0. Yang tersisa adalah keputusan pemilik sistem, bukan
pekerjaan teknis yang belum selesai:

| Butir | Sifat |
|---|---|
| Pemberian kemampuan sensitif kepada Departemen × Posisi | Keputusan owner, bukan implementasi |
| 17 policy inert (`SEMANTIC_CHANGED` dan `REMOVED_CAPABILITY`) | Sengaja fail closed |
| Dua baris proyeksi legacy-unresolved | Sengaja dipertahankan, tidak ditebak |
| Klasifikasi dua endpoint audio antrean | Di luar cakupan A0 |
| Penerapan ke lingkungan selain development | Operasional |

Requirement untuk Business Permission, Access Profile, baseline Self Service, otorisasi frontend,
dan perancangan ulang Role Management **belum** didefinisikan pada blueprint ini dan **tidak**
dianggap tercakup.
