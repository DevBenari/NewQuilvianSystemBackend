# Patient Management Blueprint Manifest

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `module_name` | Patient Management |
| `module_slug` | `patient-management` |
| `module_prefix` | `Pat` — registry `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 24) |
| `area` | `HealthServices` |
| `blueprint_shape` | `SINGLE` — `shape_decided_by: USER` (`PAT-DEC-002`) |
| `revision` | `2` — perubahan keputusan yang disetujui; lihat "Riwayat revisi" |
| `status` | **`approved`** untuk cakupan satu work item `BE-PAT-MIG-001`. **Bukan** persetujuan desain modul Patient Management secara utuh |
| `created_at` | `2026-10-08` |
| `updated_at` | `2026-10-08` |
| `last_verified_at` | `null` |
| `backend_source_sha` | `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` (`QuilvianStaDeploy`) |
| `frontend_source_sha` | Tidak diperiksa — tidak ada pekerjaan frontend (`PAT-DEC-015`) |
| `approved_by` | Pemilik/leader work item, sesi `MODULE BLUEPRINT` 8 Oktober 2026 (akun Git `devbenari`) |
| `approved_at` | 8 Oktober 2026 — revision `1`; keputusan review final 8 Oktober 2026 — revision `2` |
| `decision_revision` | `00-interview-decisions.md` revision `2` |
| `input_revision_hash` | `sha256:fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885` (`00-interview-decisions.md`) |
| `contract_versions` | API `1.1.0` — `approved` final 8 Oktober 2026. Tidak ada butir `PROPOSED` |
| `active_dependency_ids` | `[]` — tidak ada task prasyarat |
| `active_roadmap_revision` | `2` |
| `supersedes` | `null` |

---

## Untuk apa blueprint ini ada

Blueprint ini dibuat **hanya** supaya `BE-PAT-MIG-001` punya roadmap, kontrak, dan traceability
yang sah sebelum diimplementasikan lewat `quilvian-engineering-skills:build-module-backend`.
Isinya adalah keputusan pengguna 8 Oktober 2026, dicatat apa adanya, ditambah bukti source yang
dibaca read-only pada snapshot di atas.

Blueprint ini **tidak** merancang modul Patient Management. Pendaftaran pasien, penggabungan
pasien, foto, kiosk, dan kemampuan pasien lain tidak dibahas, dan tidak boleh disimpulkan dari
berkas-berkas di sini.

---

## Gerbang desain

| Gerbang | Keadaan | Yang tertahan bila belum terpenuhi |
| --- | --- | --- |
| Penempatan modul dan prefix | **Terpenuhi** — `PAT-DEC-001`; registry `ACTIVE` | — |
| Task, acceptance criteria, dan batas wewenang disetujui | **Terpenuhi** 8 Oktober 2026 — `PAT-DEC-003`, `PAT-DEC-012` | — |
| Kontrak API dikunci | **Terpenuhi** — `1.1.0`, hash di bawah | — |
| Butir `PROPOSED` dikonfirmasi | **Terpenuhi** 8 Oktober 2026 — `PAT-OQ-001` sampai `PAT-OQ-004` final, ditambah `PAT-OQ-005` sampai `PAT-OQ-007`. Pertanyaan terbuka: **0** | Tidak ada. `PAT-OQ-001` tetap bersyarat pada pemeriksaan `TEST_POLICY` saat build (governance guard) |
| Otorisasi eksekusi runtime | **Terbuka** — `PAT-GATE-001` | Deployment staging, pemanggilan endpoint, dan eksekusi 715 pasien. Tidak menahan implementasi source |

---

## Hash artefak

Dihitung 8 Oktober 2026 dengan `sha256sum` terhadap berkas apa adanya di disk. Repository ini
memakai `core.autocrlf`, sehingga verifikasi wajib memakai cara yang sama atau menyebut cara lain
yang dipakai.

| Artefak | SHA-256 |
| --- | --- |
| `00-interview-decisions.md` | ~~`909bacd918dec0c202f59d19039599d4dcd0eac329da4234ea9923b6f4f08be7`~~ (revision `1`) → **`fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885`** (revision `2`) |
| `contracts/api-contract.md` | ~~`765a3b6ac5da8c03343ab1402baf37ff37928551c3590c03beb46b1dc40a9f39`~~ (`1.0.0`) → **`cb41e4d5a0f200bb8b234ad4e741413c1ba96837140fa38460a2a78dd710ad7e`** (`1.1.0`) |

Hash lama dicoret, bukan dihapus, supaya jejak perubahannya tetap terbaca.

Bila salah satunya berubah, roadmap dan traceability menjadi **stale** dan wajib ditinjau ulang
sebelum dipakai. Hash wajib dihitung ulang pada perubahan yang sama, bukan belakangan.

---

## Struktur keluaran

Berkas yang dibuat pada revision `1`, beserta alasan masing-masing wajib ada:

```text
docs/module-blueprints/patient-management/
├── blueprint-manifest.md          # jangkar identitas, revision, status, SHA, dan hash
├── 00-interview-decisions.md      # keputusan berID yang dirujuk roadmap dan traceability
├── contracts/
│   └── api-contract.md            # versi kontrak yang wajib dibawa setiap task
└── roadmap/
    ├── backend-roadmap.md         # kartu BE-PAT-MIG-001
    ├── frontend-roadmap.md        # keluaran wajib plan-module-delivery; isinya mencatat nol task
    └── requirement-traceability.md
```

Jalur laporan task dicadangkan, tetapi **tidak dibuat** sekarang:
`task/report/backend/BE-PAT-MIG-001.md`. Laporan itu milik `build-module-backend`, ditulis
sesudah build dan validasi. Skill perencanaan tidak menulis ke `task/report/**`, dan folder
kosong tidak dibuat untuk lapisan yang belum punya laporan.

### `PAT-GAP-001` — berkas pasti `SINGLE` yang belum dibuat

Aturan bentuk blueprint mewajibkan 14 berkas pada bentuk `SINGLE`. Revision `1` memuat 3 di
antaranya. 11 berkas berikut **sengaja tidak dibuat**, karena pengguna meminta paket minimum dan
melarang mengarang requirement di luar keputusannya:

| Berkas | Pemilik normalnya | Bagian yang sudah tertampung di revision `1` |
| --- | --- | --- |
| `01-existing-capability-map.md` | `trace-existing-capabilities` | Kolom Reuse dan Larangan pakai ulang pada kartu task |
| `02-backend-architecture.md` | `design-business-module` | Bagian Cakupan dan Risiko pada kartu task |
| `03-frontend-architecture.md` | `design-business-module` | `PAT-DEC-015` — tanpa layar |
| `04-prd-to-mvp.md` | `design-business-module` | Slice S1 dan DoD pada roadmap |
| `flowcharts/00-alur-utama.md` | `design-business-module` | `api-contract.md` bagian 4 |
| `data/data-dictionary.md` | `design-business-module` | `api-contract.md` bagian 8 dan 9 |
| `contracts/state-transition-matrix.md` | `design-business-module` | `api-contract.md` bagian 4.3 |
| `contracts/validation-matrix.md` | `design-business-module` | `api-contract.md` bagian 2.1 |
| `contracts/integration-contract.md` | `design-business-module` | `api-contract.md` bagian 5 dan 8 |
| `contracts/permission-audit-matrix.md` | `design-business-module` | `api-contract.md` bagian 7 |
| `testing/acceptance-test-matrix.md` | `design-business-module` | `AC-27` dan tabel Verifikasi pada kartu task |

Gap ini tidak menahan `BE-PAT-MIG-001`. Bila kelak modul Patient Management dirancang utuh,
berkas-berkas di atas dibuat lewat skill pemiliknya, dan revision manifest naik.

---

## Pemicu impact scan

Blueprint ini menjadi **stale** bila salah satu berkas berikut berubah sebelum implementasi
dimulai:

| Berkas | Bagian yang terdampak |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Route, tag, izin, logika QR, pembangkit MRN dan `PatientCode` — `api-contract.md` bagian 2, 5, 7, 9 |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` | Field yang boleh dan tidak boleh berubah — bagian 9 |
| `Models/IdentityModel.cs` | Field audit — bagian 9 |
| `Services/Security/AccessPermissionService.cs` | Contoh pola environment yang dilarang ditiru — bagian 6 |
| `Repositories/ApplicationDbContext.cs` | Pemetaan tabel `migration.rsmmc_*` — risiko `PAT-RSK-001` |
| `Tests/QuilvianSystemBackend.PharmacyTests/QuilvianSystemBackend.PharmacyTests.csproj` | Konvensi project test yang diikuti `PAT-OQ-001` |
| `.github/workflows/generate-migration-artifact.yml` | Bukti ejaan nama environment `Staging` — `PAT-OQ-002` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderNumberService.cs`, `Areas/HealthServices/RegistrationManagement/Services/KioskPatientLookupService.cs` | Contoh mekanisme baca berparameter — `PAT-OQ-007` |

Bila commit `QuilvianStaDeploy` sudah bergeser dari `103b45cc` saat build dimulai, agent build
memeriksa ulang kutipan baris pada `api-contract.md` lebih dulu, dan mencatat selisihnya.

---

## Riwayat revisi

| Revision | Tanggal | Perubahan | Berkas yang berubah |
| ---: | --- | --- | --- |
| `1` | 8 Oktober 2026 | Paket minimum dibuat. `PAT-OQ-001`–`004` berstatus `PROPOSED` dengan default aman | Seluruh enam berkas dibuat |
| `2` | 8 Oktober 2026 | Pemilik memfinalkan `PAT-OQ-001`–`004` dan menambah `PAT-OQ-005`–`007`. Kontrak naik `1.0.0` → `1.1.0`: nama status konflik QR dikunci `CONFLICT`, pemeriksaan QR sebelum helper, pencocokan `Staging` persis, default `dryRun=true`, dan larangan EF migration dipertegas. Bentuk request dan route **tidak berubah** | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md` (hanya `blueprint_revision`), manifest ini |

---

## Langkah berikutnya

1. ~~Pengguna mereview paket ini lalu mengonfirmasi `PAT-OQ-001`–`004`~~ — **selesai** 8 Oktober
   2026 pada revision `2`.
2. `BE-PAT-MIG-001` diimplementasikan lewat `quilvian-engineering-skills:build-module-backend`
   dalam sesi `TASK MODE: BACKEND` tersendiri.
3. Sesudah review source, build, dan test lulus, pemilik memutuskan `PAT-GATE-001` secara
   terpisah.
