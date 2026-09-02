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

## Requirement Business Permission dan Access Profile

Diturunkan dari audit `BE-SEC-002` dan keputusan pemilik sistem `D-ARCH-1` sampai `D-ARCH-9`
tertanggal 2 September 2026. Kolom bukti menunjuk artefak yang benar-benar dapat ditelusuri.

| Requirement | Keputusan owner | Backend | Frontend | Bukti target | Status coverage |
|---|---|---|---|---|---|
| `SEC-REQ-013` — Identitas technical permission harus cukup granular sehingga satu izin tidak membuka endpoint bermakna bisnis berbeda | `D-ARCH-3`, `D-ARCH-6`, `D-ARCH-7` | `BE-SEC-003` | `NOT APPLICABLE` | `evidence/02` bagian C–F; `evidence/03` matriks pemecahan 7 → 28 | Planned |
| `SEC-REQ-014` — Pemecahan identitas tidak boleh mengubah kemampuan efektif satu pun Departemen × Posisi | `D-ARCH-2` | `BE-SEC-003` | `NOT APPLICABLE` | `evidence/03` legacy parity matrix; test parity per Departemen × Posisi | Planned |
| `SEC-REQ-015` — Setiap kemampuan bisnis harus punya kode stabil yang tidak terikat nama class, route, maupun identifier teknis | `D-ARCH-1` | `BE-SEC-004`, `BE-SEC-005` | `NOT APPLICABLE` | `evidence/01` bagian I; `evidence/02` bagian B | Planned |
| `SEC-REQ-016` — Satu Business Permission memetakan ke satu atau lebih technical permission, dan pemetaan itu satu-satunya tempat nama teknis muncul | `D-ARCH-1` | `BE-SEC-005` | `NOT APPLICABLE` | `SecBusinessPermissionMapping`; test pemetaan yatim | Planned |
| `SEC-REQ-017` — Access Profile adalah bundel yang dapat dipakai ulang; satu Departemen + Posisi boleh punya lebih dari satu, dan izin efektifnya adalah UNION | `D-ARCH-5` | `BE-SEC-006` | `NOT APPLICABLE` | `SecAccessProfile`; `SecOrganizationAccessProfile`; test UNION | Planned |
| `SEC-REQ-018` — Override langsung hanya bersifat ADDITIVE GRANT; tidak ada subtractive DENY dan tidak ada DENY precedence | `D-ARCH-5` | `BE-SEC-006`, `BE-SEC-008` | `FE-SEC-002` | `SecOrganizationPermissionGrant` tanpa kolom DENY; test anti-DENY | Planned |
| `SEC-REQ-019` — Izin efektif adalah gabungan sumber legacy dan sumber Business Permission, dan sumber baru dapat dimatikan sehingga hasilnya kembali persis ke baseline `BE-SEC-001` | `D-ARCH-1`, `D-ARCH-2` | `BE-SEC-007`, `BE-SEC-008` | `NOT APPLICABLE` | `BusinessPermissionResolutionService`; test `DisablingProfileSourceReproducesA0Baseline` | Planned |
| `SEC-REQ-020` — Frontend memperoleh kode Business Permission stabil dan tidak pernah membaca `ControllerName`, `ActionName`, `SysControllerAccessId`, maupun `SysActionAccessId` | `D-ARCH-1` | `BE-SEC-010` | `FE-SEC-001`, `FE-SEC-002`, `FE-SEC-003` | `GET /api/v1/access/me`; test kontrak response | Planned |
| `SEC-REQ-021` — Endpoint audio panggilan antrean harus terlindungi tanpa `AllowAnonymous`, dan mengizinkan actor manusia maupun perangkat display lewat semantik OR yang benar-benar OR | `D-ARCH-8`, `O-1` | `BE-SEC-003` | `NOT APPLICABLE` | `evidence/02` bagian J; `evidence/03` bagian 8 (audit semantik OR, rancangan filter, dampak) dan 14.1 (keputusan `O-1` beserta delapan Departemen × Posisi penerima) | Design closed — implementasi `Planned` |
| `SEC-REQ-022` — Kemampuan yang endpoint-nya tidak ada tidak boleh dipetakan ke technical permission apa pun | `D-ARCH-9` | `BE-SEC-005` | Task frontend terpisah | BP-19 terdaftar `BLOCKED` dengan nol pemetaan; fail closed secara konstruksi | Planned |
| `SEC-REQ-023` — Kemampuan sensitif tidak diberikan otomatis kepada profil mana pun sebelum Departemen × Posisi penerimanya ditetapkan | `D-ARCH-6` | `BE-SEC-006` | `NOT APPLICABLE` | Test: `procedure.approve` tidak ada di `DOCTOR_OUTPATIENT_BASE` | Planned |

---

## Coverage Gap

### Phase A0 (`BE-SEC-001`)

Tidak ada gap requirement internal. Yang tersisa adalah keputusan pemilik sistem, bukan pekerjaan
teknis yang belum selesai:

| Butir | Sifat | Status setelah `BE-SEC-002` |
|---|---|---|
| Pemberian kemampuan sensitif kepada Departemen × Posisi | Keputusan owner | Masih terbuka; kini dipandu `SEC-REQ-023` |
| 17 policy inert (`SEMANTIC_CHANGED` dan `REMOVED_CAPABILITY`) | Sengaja fail closed | Masih terbuka |
| Dua baris proyeksi legacy-unresolved | Sengaja dipertahankan | Masih terbuka |
| Klasifikasi dua endpoint audio antrean | Di luar cakupan A0 | **Tertutup** — `D-ARCH-8` dan `O-1`; desain selesai, dikerjakan `BE-SEC-003` |
| Penerapan ke lingkungan selain development | Operasional | Masih terbuka |

### Business Permission dan Access Profile

`SEC-REQ-013` sampai `SEC-REQ-023` **belum ada satu pun yang diimplementasikan**. Yang sudah selesai
adalah desain dan penutupan keputusannya:

| Requirement | Desain | Keputusan owner | Implementasi |
| --- | --- | --- | --- |
| `SEC-REQ-013`, `SEC-REQ-014` | Selesai — `evidence/03` bagian 5 dan 11 | Tertutup | `BE-SEC-003`, belum dimulai |
| `SEC-REQ-021` | Selesai — `evidence/03` bagian 8 | Tertutup (`O-1`) | `BE-SEC-003`, belum dimulai |
| `SEC-REQ-015` … `SEC-REQ-020`, `SEC-REQ-022`, `SEC-REQ-023` | Selesai — `evidence/01` dan `evidence/02` | Tertutup | `BE-SEC-004` dan sesudahnya, belum dimulai |

Requirement yang **belum** didefinisikan dan **tidak** dianggap tercakup:

| Area | Alasan belum didefinisikan |
|---|---|
| Baseline Self Service otomatis | Definisi "pegawai aktif" adalah keputusan HR; `BE-SEC-011` belum didekomposisi |
| Penegakan data-scope (`OWN`, `SUBORDINATES`, `ORGANIZATION_SCOPE`) | Lapisan terpisah; hanya dicatat, belum ditegakkan |
| Clinical privilege per jenis tindakan | Lapisan terpisah di atas izin, bukan pengganti izin |
| Tenancy per rumah sakit pada otorisasi | Rantai otorisasi saat ini hospital-agnostic; perubahannya blueprint tersendiri |
| Otorisasi SignalR hub `/hubs/queues` | Belum diaudit |
