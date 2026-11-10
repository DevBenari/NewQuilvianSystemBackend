# Backend Roadmap — Rehab Medik

| Field | Nilai |
|---|---|
| Roadmap ID | `RHB-RM-BE-001` |
| Revision | `2` (2026-11-05). Kriteria `BE-RHB-003` dibatasi ke backend. Lihat *Catatan revisi*. |
| Status | `approved` (Sukma Giri Pratama, 2026-11-05 — instruksi "lanjutkan" setelah review roadmap r1) |
| Blueprint | `RHB-BP-001` r1, `approved` (Sukma Giri Pratama, 2026-11-05) |
| Cakupan | `MVP-0` s.d. `MVP-4` (`EPIC RHB-00` s.d. `RHB-07`). `POST-MVP` dicatat di akhir. |
| SHA baseline | BE `6bb3fba7` (`sukmagp`), FE `0c06c093` (`sukmagpV2`) |
| Kontrak masukan | Set kontrak `v1` (`api-contract`, `state-transition-matrix`, `validation-matrix`, `integration-contract`, `permission-audit-matrix`). Hash per berkas ada di [`blueprint-manifest.md`](../blueprint-manifest.md). |
| Owner | Sukma Giri Pratama |

> **Batas dokumen.**
> 1. Roadmap ini **bukan** izin menulis kode. Setiap task dikerjakan lewat
>    `build-module-backend` dengan task ID dan approval eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md`
>    backend dan `docs/engineering/`, bukan dari roadmap ini.
> 3. Sesuai RHB-DEC-015, agent **boleh menulis** model, configuration, dan file migration, tetapi
>    **tidak menjalankan** `dotnet build`, `dotnet ef`, `database update`, `dotnet test`, commit,
>    maupun push. Owner menjalankan build dan migration secara manual, lalu hasilnya dicatat di
>    laporan task. Sebelum hasil itu tercatat, task tidak boleh `✅`.
> 4. Automated backend test tidak dibuat (`rules/backend/TEST_POLICY.md`). Bukti verifikasinya
>    adalah QBE preflight, review diff/scope, build (oleh owner), verifikasi kontrak API, dan
>    verifikasi proses bisnis/DB manual.
> 5. Task yang mengubah berkas milik modul lain wajib menyebut persetujuan pemiliknya di laporan
>    task: Rekam Medis (RHB-DEC-019, bukti RHB-OQ-012), ClinicalManagement (RHB-DEC-018,
>    RHB-OQ-015), Billing (RHB-OQ-015).

## Legenda tanda status

Legenda ini ditetapkan owner (instruksi 2026-11-05) dan **berlaku untuk roadmap ini**.

| Tanda | Arti | Boleh dijadwalkan? |
|:---:|---|---|
| ✅ | **SELESAI** — bukti penerimaan tercatat di `task/report/backend/` | Sudah selesai |
| 🟡 | **PENDING** — seluruh prasyaratnya terpenuhi, tinggal dikerjakan | Ya |
| ⛔ | **BLOCKED** — ada prasyarat yang belum tersedia, baik task lain yang belum ✅ maupun keputusan/gerbang | Tidak |

> **Selisih dengan aturan canonical.** Pada `rules/rule-output/status-task-roadmap.md`, 🟡
> berarti *SEBAGIAN* (source sudah ada tetapi kriterianya belum terbukti), dan task tanpa tanda
> berarti belum dikerjakan. Roadmap ini memakai 🟡 sebagai *PENDING/siap dijadwalkan* atas
> permintaan owner. Konsekuensinya:
> - Task yang source-nya sudah ditulis tetapi build/verifikasinya belum dijalankan owner tetap
>   ditulis 🟡 dengan keterangan "menunggu build owner" di baris Status kartunya, bukan ✅.
> - Sesudah sebuah task menjadi ✅, task yang menunggunya naik dari ⛔ ke 🟡 bila seluruh
>   prasyaratnya sudah ✅.
>
> Jenis ⛔ dibedakan di tabel gelombang: **menunggu task** (tertahan urutan) atau **menunggu
> keputusan** (node `{...}`).

## Grafik Urutan Dependency

Grafik dipecah per slice karena roadmap memuat 16 task (batas keterbacaan 15 node). Node
berlabel `[S-x]` adalah cermin baca-saja dari slice lain pada roadmap ini. Tandanya disalin dari
slice pemiliknya.

### Ringkasan antar-slice

```text
S-A ─> S-B ─> S-C ─┬────────┬─> S-E
                   │        │
                   └─> S-D ─┘
```

`S-A` = Fondasi (MVP-0) · `S-B` = Permintaan dan episode (MVP-1) · `S-C` = Asesmen, edukasi,
program (MVP-2) · `S-D` = Sesi dan billing (MVP-3) · `S-E` = Reasesmen dan penutupan (MVP-4)

### Slice S-A — Fondasi

```text
BE-RHB-000 ✅ ─> BE-RHB-001 ✅ ─┬─> BE-RHB-002 ✅
                                │
                                ├─> BE-RHB-004 ✅
                                │
                                └─┬─> BE-RHB-005 ✅ ─> BE-RHB-006 ✅
                                  │
BE-RHB-003 ✅ ────────────────────┘
```

### Slice S-B — Permintaan dan episode

```text
BE-RHB-002 ✅ [S-A] ─┬─> BE-RHB-007 ✅ ─> BE-RHB-008 ✅
                     │
BE-RHB-006 ✅ [S-A] ─┘
```

### Slice S-C — Asesmen, edukasi, program

```text
BE-RHB-004 ✅ [S-A] ─┬─> BE-RHB-009 ✅ ─┬─> BE-RHB-011 ✅
                     │                  │
                     ├─> BE-RHB-010 ✅ ─┘
                     │
BE-RHB-008 ✅ [S-B] ─┘
```

`BE-RHB-009` dan `BE-RHB-010` masing-masing menunggu `BE-RHB-004` **dan** `BE-RHB-008`.

### Slice S-D — Sesi dan handoff billing

```text
BE-RHB-011 ✅ [S-C] ─> BE-RHB-012 ✅ ─┬─> BE-RHB-013 ✅
                                      │
{RHB-OQ-015 ✅} ──────────────────────┘
```

### Slice S-E — Reasesmen dan penutupan

```text
BE-RHB-011 ✅ [S-C] ─> BE-RHB-014 ✅ ─┬─> BE-RHB-015 ✅
                                      │
BE-RHB-012 ✅ [S-D] ──────────────────┘
```

Jumlah pasangan prasyarat → task pada grafik ada **20** (ditambah satu gerbang
`RHB-OQ-015`), sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RHB-000` ✅, `BE-RHB-003` ✅ — boleh paralel |
| 2 | `BE-RHB-000` | `BE-RHB-001` ✅ |
| 3 | `BE-RHB-001` (+ `BE-RHB-003` untuk 005) | `BE-RHB-002` ✅, `BE-RHB-004` ✅, `BE-RHB-005` ✅ — boleh paralel |
| 4 | `BE-RHB-005` | `BE-RHB-006` ✅ |
| 5 | `BE-RHB-002`, `BE-RHB-006` | `BE-RHB-007` ✅ |
| 6 | `BE-RHB-007` | `BE-RHB-008` ✅ |
| 7 | `BE-RHB-004`, `BE-RHB-008` | `BE-RHB-009` ✅, `BE-RHB-010` ✅ — boleh paralel |
| 8 | `BE-RHB-009`, `BE-RHB-010` | `BE-RHB-011` ✅ |
| 9 | `BE-RHB-011` | `BE-RHB-012` ✅, `BE-RHB-014` ✅ — boleh paralel |
| 10 | `BE-RHB-012`, `BE-RHB-014` | `BE-RHB-015` ✅ |
| — | keputusan `RHB-OQ-015` ✅ (2026-11-09) dan `BE-RHB-012` ✅ | `BE-RHB-013` ✅ |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `BE-RHB-000` | Baris registry `Rhb` terverifikasi checker | QBE-MOD-002/003, NAM-004; RHB-DEC-012, 014 | Registry | Prosedur baris `Opt`/`Mdf` | Registry (sudah ditulis) | — | lihat kartu | Checker QBE + kontrol negatif | Pemegang registry | Kartu |
| `BE-RHB-001` | 20 model, 18 enum, 20 configuration, DbSet, konstanta | RHB-DEC-001..021 | Data dictionary v1 | `IdentityModel`, pola `HmdEpisodeConfiguration` | `RehabilitationManagement/{Models,Enums,Constants}`, `Repositories/Configurations/...`, `ApplicationDbContext.cs` | `BE-RHB-000` | kartu | Build owner, QBE | Rehab | Kartu |
| `BE-RHB-002` | Migration `AddRehabilitationManagement` | RHB-DEC-015 | Data dictionary v1 §5 | — | `Migrations/` | `BE-RHB-001` | kartu | Owner: generate/verify, apply dev, `has-pending-model-changes` | Snapshot tidak cocok — owner | Kartu |
| `BE-RHB-003` | `ClinicalInstrumentKind` 8–13 | RHB-DEC-017, 018 | BE arch §7.2 | Enum existing | `ClinicalManagement/Enums/ClinicalInstrumentKind.cs` | — | kartu | Review diff, build owner | Modul Cli | Kartu |
| `BE-RHB-004` | Jenis dokumen RM 47–52 + penegakan + gateway | RHB-DEC-007, 019 | INT-RHB-05 | `ClinicalDocumentIntegrityService` | `MedicalRecordManagement/...` (2 berkas), `RhbDocumentIntegrityGateway` | `BE-RHB-001` | kartu | Review diff, build owner | ✅ butuh bukti RHB-OQ-012 | Kartu |
| `BE-RHB-005` | Konfigurasi layanan rehab (9 endpoint + 2 sub-resource) + seeder `RhbDiscipline` | RHB-DEC-004, 017, 020 | API v1 *Rehab Discipline* | Standar master data | `RehabDisciplineController`, `RhbConfigurationService`, DTO, seeder | `BE-RHB-001`, `BE-RHB-003` | kartu | API manual, DB | Rehab | Kartu |
| `BE-RHB-006` | Lookup + pemeriksaan encounter + kewenangan profesi | RHB-DEC-016; RHB-INV-01, 08 | API v1 *Rehab Lookup*; INT-RHB-01..04, 07 | `HmdServiceSupport.CheckEncounterAsync`, `InpatientClinicalContextService`, `ClinicalInstrumentService` | `RehabLookupController`, `RhbLookupService`, `RhbServiceSupport`, `RhbProfessionAuthority` | `BE-RHB-005` | kartu | API manual (anti-IDOR) | Rehab | Kartu |
| `BE-RHB-007` | Permintaan + disposisi + buka episode | FR-RHB-001..003; FEAT-001..003 | API v1 *Rehab Request*; state §1; V-REQ-* | `NumberSeriesAllocator` | `RehabRequestController`, `RhbRequestService`, `RhbStatusHistory` writer | `BE-RHB-002`, `BE-RHB-006` | kartu | API + DB manual | Rehab | Kartu |
| `BE-RHB-008` | Worklist + detail + timeline episode | FEAT-007..029 (baca) | API v1 *Rehab Episode* | — | `RehabEpisodeController`, `RhbEpisodeService` | `BE-RHB-007` | kartu | API manual | Rehab | Kartu |
| `BE-RHB-009` | Asesmen medis + profesi + pengukuran + tanda tangan | FR-RHB-011..016; FEAT-007..010 | API v1 *Medical/Discipline Assessment*; state §3 | Gateway RM, mesin instrumen | 2 controller, `RhbAssessmentService` | `BE-RHB-004`, `BE-RHB-008` | kartu | API + DB manual | Rehab | Kartu |
| `BE-RHB-010` | Edukasi + persetujuan | FR-RHB-020; FEAT-015 | API v1 *Rehab Education* | Gateway RM | `RehabEducationController`, `RhbEducationService` | `BE-RHB-004`, `BE-RHB-008` | kartu | API manual | Rehab | Kartu |
| `BE-RHB-011` | Tujuan + program berversi + aktivasi | FR-RHB-010, 021, 022; FEAT-012, 013 | API v1 *Rehab Goal*, *Rehab Program*; state §4, §8 | Gateway RM | 2 controller, `RhbGoalService`, `RhbProgramService` | `BE-RHB-009`, `BE-RHB-010` | kartu | API + DB manual | Rehab | Kartu |
| `BE-RHB-012` | Sesi terapi | FR-RHB-030..032, 036, 037; FEAT-016..018 | API v1 *Rehab Session*; state §5 | Gateway RM, `RhbServiceSupport` | `RehabSessionController`, `RhbSessionService` | `BE-RHB-011` | kartu | API + DB manual | Rehab | Kartu |
| `BE-RHB-013` | Handoff billing sesi | FR-RHB-033..035; FEAT-032; RHB-DEC-021 | API v1 *Rehab Billing Handoff*; INT-RHB-08/09 | `TrxPatientProcedure`, `ClinicalMilestoneFactProducer` (pola `HmdBillingHandoffService`) | `RehabSessionBillingController`, `RhbBillingHandoffService`, `PatientProcedureSource` +1 | `BE-RHB-012`, `{RHB-OQ-015}` | kartu | API + DB + invoice | Billing/Cli | Kartu |
| `BE-RHB-014` | Reasesmen + efek keputusan | FR-RHB-040, 041; FEAT-024, 025 | API v1 *Rehab Progress Review*; state §7 | Gateway RM | `RehabProgressReviewController`, `RhbProgressReviewService` | `BE-RHB-011` | kartu | API + DB manual | Rehab | Kartu |
| `BE-RHB-015` | Ringkasan akhir + tutup episode + pembaca berkas RM | FR-RHB-050..052, 060, 061; FEAT-026..029 | API v1 *Rehab Discharge Summary*; INT-RHB-06 | `IMedicalRecordDocumentSource` (pola `HmdMedicalRecordDocumentSource`) | `RehabDischargeSummaryController`, `RhbDischargeService`, `RhbMedicalRecordDocumentSource`, `Program.cs` | `BE-RHB-012`, `BE-RHB-014` | kartu | API + DB + berkas RM | Rehab/RM | Kartu |

## Kartu task

### `BE-RHB-000` ✅ — Prasyarat governance registry (bukan task kode)

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). Checker QBE `-Path` `ReportOnly` dijalankan dua kali terpisah. Probe `RhbRegistryProbe` di `RehabilitationManagement` **tidak** terkena QBE-MOD-002 (hanya QBE-CFG-001, wajar karena probe tanpa configuration). Kontrol `ZzzRegistryProbe` **terkena** QBE-MOD-002. Kedua probe dihapus; `git status --short Areas/` hanya menyisakan perubahan `BE-RHB-003`. Konfirmasi pemegang registry belum dilakukan dan tidak memblokir. [Laporan](../task/report/backend/BE-RHB-000.md) |
| Outcome | Bukti bahwa model `Rhb*` ber-`[Table]` di `Areas/HealthServices/RehabilitationManagement/` tidak terkena QBE-MOD-002 |
| Requirement/decision | RHB-DEC-012, 014; QBE-MOD-002/003, QBE-NAM-004 |
| Kontrak | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Reuse | Prosedur `BE-OPT-000` (probe sementara lalu dihapus) |
| Cakupan | Tanpa perubahan kode permanen |
| Dependency | — |
| Acceptance criteria | (1) Checker QBE tidak melaporkan QBE-MOD-002 untuk probe `Rhb*` di folder `RehabilitationManagement`. (2) Kontrol negatif `Zzz*` di folder tak terdaftar tetap terkena. (3) Probe dihapus, dan `git status --short Areas/` bersih. |
| Verifikasi | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` dijalankan owner, atau agent dengan izin eksplisit, karena checker ini termasuk pemeriksaan yang dilarang dijalankan agent tanpa izin |
| Risiko/pemilik | Tanpa bukti ini model pertama tidak boleh dibuat. Pemegang registry. |
| DoD | Bukti checker tercatat di `task/report/backend/BE-RHB-000.md` |

### `BE-RHB-001` ✅ — Fondasi model dan konstanta

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). 45 berkas: 20 model, 20 configuration, 18 enum, 3 konstanta, 20 `DbSet`. QBE Strict `PASS` (45 berkas, 0 violation). `dotnet build` (perintah owner) `PASS`: 0 error, 426 warning legacy, 0 warning Rehab, 13m31s. Delta kamus data dicatat di laporan §3.3. Migration belum ada (`BE-RHB-002`). [Laporan](../task/report/backend/BE-RHB-001.md) |
| Outcome | 20 model `Rhb*`, 18 enum, 20 configuration EF (index, unique parsial, check constraint, FK `Restrict`, `Version` concurrency), 20 `DbSet`, `RehabilitationPermissions`, `RhbNumberSeries`, `RhbErrorCodes` |
| Requirement/decision | RHB-DEC-001..021; RHB-INV-01..10 |
| Kontrak | `data/data-dictionary.md` v1; `02-backend-architecture.md` §4–5 |
| Reuse | `IdentityModel`; pola `HemodialysisPermissions`, `HmdEpisodeConfiguration` |
| Cakupan | `Areas/HealthServices/RehabilitationManagement/{Models,Enums,Constants}/`, `Repositories/Configurations/HealthServices/RehabilitationManagement/`, `Repositories/ApplicationDbContext.cs`. Tanpa sub-folder `MasterData`. |
| Dependency | `BE-RHB-000` |
| Acceptance criteria | (1) Setiap kolom, tipe, panjang, wajib, dan bawaan sesuai kamus data. (2) Unique parsial `UX_RhbProgram_ActivePerDiscipline`, `UX_RhbAssessment_MedicalFinal`, dan check `CK_RhbRequest_Source`, `CK_RhbMeasurement_Owner` ada di configuration. (3) Seluruh FK ke modul lain `Restrict`. (4) Konstanta resource/action sama persis dengan `permission-audit-matrix` §2. (5) Tidak ada `Trx*`, `SortOrder` tersimpan, maupun controller. |
| Verifikasi | QBE preflight; review diff terhadap kamus data; `dotnet build` oleh owner |
| Risiko/pemilik | Salah ketik nama resource menyebabkan 403 permanen. Rehab. |
| DoD | Kriteria 1–5 terbukti, build owner sukses, laporan task |

### `BE-RHB-002` ✅ — Migration skema Rehab

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). `Migrations/20261105045613_AddRehabilitationManagement` dibuat lewat `ef migrations add` (RHB-DEC-022). `Up`: 20 `CreateTable` + 77 `CreateIndex`, tanpa operasi atas tabel lain. 53 FK `Restrict`. Build `PASS` (0 error, 4m16s). `has-pending-model-changes` bersih. Diterapkan ke `QuilvianNewDevSukma` lewat skrip idempoten khusus Rehab, karena 8 migration modul lain masih pending dan tidak disentuh. Katalog: 20 tabel, 3 UX, 4 CK, 53 FK. [Laporan](../task/report/backend/BE-RHB-002.md) |
| Outcome | `Migrations/<timestamp>_AddRehabilitationManagement.cs` + `Designer` + snapshot |
| Requirement/decision | RHB-DEC-015 |
| Kontrak | `02-backend-architecture.md` §7; kamus data §5 |
| Reuse | — |
| Cakupan | `Migrations/` saja |
| Dependency | `BE-RHB-001` |
| Acceptance criteria | (1) `Up` hanya `CREATE TABLE`/index untuk 20 tabel `Rhb*`. (2) `Down` menghapus 20 tabel tanpa menyentuh tabel lain. (3) Owner menjalankan `has-pending-model-changes` dengan hasil tidak ada perubahan tertunda. (4) Diterapkan ke DB dev pemilik (`QuilvianNewDevSukma`) oleh owner. |
| Verifikasi | Owner: build, `has-pending-model-changes`, apply dev, cek tabel di DB |
| Risiko/pemilik | Migration tulisan tangan tidak cocok dengan snapshot. Bila meragukan, owner men-generate ulang dengan `dotnet ef migrations add` dan agent membandingkan hasilnya dengan kamus data. Owner. |
| DoD | Kriteria 1–4 terbukti, laporan task |

### `BE-RHB-003` ✅ — Jenis instrumen rehab di ClinicalManagement

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). `dotnet build` Debug dijalankan owner: `PASS`. DLL ditulis 11:11:37, sesudah perubahan enum, dan keenam nilai `Rehab*` ada di DLL. Review diff `PASS` (1 berkas, +27/−1). Kriteria r2 (1)–(2) terpenuhi. Bagian FE ada di `FE-RHB-002`. QBE checker `NOT RUN`, karena bukan kriteria kartu ini (task tanpa entity). [Laporan](../task/report/backend/BE-RHB-003.md) |
| Outcome | `ClinicalInstrumentKind`: `RehabBarthelIndex = 8`, `RehabBergBalanceScale = 9`, `RehabTimedUpAndGo = 10`, `RehabLawtonIadl = 11`, `RehabGussSwallowing = 12`, `RehabFoisOralIntake = 13` |
| Requirement/decision | RHB-DEC-017, 018 |
| Kontrak | `02-backend-architecture.md` §7.2 |
| Reuse | Enum existing |
| Cakupan | `Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs` saja (TOUCHED LEGACY) |
| Dependency | — |
| Acceptance criteria (r2) | (1) Nilai 1–7 tidak bergeser. (2) Endpoint `clinical-instruments` existing menerima jenis 8–13 tanpa perubahan lain di backend, dan bila ada daftar label per jenis **di backend**, label baru ditambahkan. Pilihan jenis di FE dikerjakan `FE-RHB-002` (r2). |
| Verifikasi | Review diff; grep pemakaian `ClinicalInstrumentKind` (switch/label) yang perlu cabang baru; build owner |
| Risiko/pemilik | Switch tanpa `default` di modul keperawatan. Pemilik Cli (persetujuan menurut RHB-DEC-018). |
| DoD | Kriteria terbukti, laporan menyebut penyetuju |

### `BE-RHB-004` ✅ — Jenis dokumen rehab di mesin keutuhan Rekam Medis

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). Bukti persetujuan RM `RHB-OQ-012` ditutup 2026-11-06 lewat konfirmasi owner ([evidence](../evidence/RHB-OQ-012-persetujuan-rekam-medis.md); bentuk: pernyataan owner, belum ada artefak dari Yoga sendiri). Sebelumnya 🟡 SEBAGIAN (2026-11-05): source selesai. `dotnet build` Debug (perintah owner): 0 error, 0 warning di folder Rehab. QBE Strict: 0 violation. Kriteria (1)–(4) terpenuhi secara statis. Kriteria (3) punya catatan: `SignAsync` meneruskan ke `SignAsync` RM yang menyimpan sendiri, dan aturan transaksinya ditulis di gateway. Delta: label timeline RM. DoD persetujuan RM terpenuhi 2026-11-06. [Laporan](../task/report/backend/BE-RHB-004.md) |
| Outcome | `ClinicalDocumentKind` 47–52; keenamnya masuk `JenisYangDitegakkan`; `RhbDocumentIntegrityGateway` (register/ensure/sign/cancel) |
| Requirement/decision | RHB-DEC-007, 019 |
| Kontrak | INT-RHB-05; `02-backend-architecture.md` §7.2 |
| Reuse | `ClinicalDocumentIntegrityService` |
| Cakupan | `MedicalRecordManagement/Enums/ClinicalDocumentKind.cs`, `MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` (hanya himpunan + komentar persetujuan), `RehabilitationManagement/Services/RhbDocumentIntegrityGateway.cs` |
| Dependency | `BE-RHB-001` |
| Acceptance criteria | (1) Nilai 1–46 tidak bergeser. (2) `DitegakkanUntuk(kind)` true untuk 47–52. (3) Gateway tidak menyimpan sendiri dan ikut transaksi pemanggil. (4) `MedicalRecordTimelineService` menyatakan jenis 47–52 "belum tersedia" sampai `BE-RHB-015` mendaftarkan pembacanya. |
| Verifikasi | Review diff; build owner |
| Risiko/pemilik | Tanpa penegakan, dokumen tampil "Ditandatangani" tetapi bisa disunting (Temuan Kritis HD). Pemilik RM Yoga Aji Pratama. |
| DoD | Kriteria terbukti + bukti persetujuan RM dilampirkan (`evidence/`) |

### `BE-RHB-005` ✅ — Konfigurasi layanan rehab

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). 15 endpoint, service, DTO, dan seeder (4 layanan tidak aktif + 9 peta instrumen; startup kedua menambah 0, terbukti idempoten). Dua build Debug (perintah owner): 0 error. QBE Strict: 0 violation. Uji API manual di dev: 26/26 sesuai. Cek DB read-only `PASS`. Kriteria (1), (2), (4), (5) terbukti di runtime. Kriteria (3): `DELETE` layanan yang belum dipakai → 200; penolakan untuk layanan yang sudah dipakai terbukti di runtime pada verifikasi `BE-RHB-007` (409 `RHB-VAL-072`, 2026-11-05). `errors` disesuaikan ke `{ code, fields }` (validation-matrix v1). [Laporan](../task/report/backend/BE-RHB-005.md) |
| Outcome | `RehabDisciplineController` (9 endpoint baseline + `GET/PUT /{id}/procedures`, `/{id}/instruments`, `/medical-instruments`); `RhbConfigurationService`; seeder idempoten 4 baris `RhbDiscipline` (O&P tidak aktif) |
| Requirement/decision | RHB-DEC-004, 017, 020; V-CFG-01/02 |
| Kontrak | API v1 *Rehab Discipline*; permission `RehabConfiguration` |
| Reuse | Standar endpoint master data |
| Cakupan | Controller, service, `RehabDisciplineDtos.cs`, seeder, DI `Program.cs` |
| Dependency | `BE-RHB-001`, `BE-RHB-003` |
| Acceptance criteria | (1) Aktivasi tanpa profesi klinis dijawab 422 `RHB-VAL-070`. (2) Kode ganda dijawab 409 `RHB-VAL-071`. (3) `DELETE` ditolak bila disiplin sudah dipakai permintaan/episode. (4) Tindakan hanya dari `MstProcedure` aktif. (5) Instrumen hanya dari `ClinicalInstrumentKind` yang terdaftar. |
| Verifikasi | API manual (Swagger) di dev; cek DB |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria terbukti, build owner, laporan |

### `BE-RHB-006` ✅ — Lookup, pemeriksaan encounter, kewenangan profesi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). 7 endpoint `rehab-lookups`, `RhbServiceSupport`, `RhbProfessionAuthority`. Build Debug (perintah owner): 0 error, 0 warning Rehab. QBE Strict: 0 violation (6 berkas). Uji API manual: 35/35 (fase 1: 30/30; fase 2 dengan konsep instrumen diizinkan lewat env: 5/5). Cek DB anti-IDOR: 22 kunjungan A dan 16 kunjungan B, semuanya milik pasien masing-masing dan terbuka, tanpa irisan. Kriteria (1), (4) terbukti di runtime. (2) terbukti di runtime kecuali cabang RWI pulang fisik (statis; data dev tidak ada). (3) batas 3 karakter dan pemotongan terbukti di runtime, batas 20 terbukti statis. (5) terbukti statis; uji runtime di `BE-RHB-009`. Delta: `RHB-VAL-077`. [Laporan](../task/report/backend/BE-RHB-006.md) |
| Outcome | 7 endpoint `rehab-lookups`; `RhbServiceSupport.CheckEncounterAsync`; `RhbProfessionAuthority` |
| Requirement/decision | FR-RHB-004; RHB-DEC-016; RHB-INV-01, 08 |
| Kontrak | API v1 *Rehab Lookup*; INT-RHB-01..04, 07 |
| Reuse | Pola `HmdServiceSupport.CheckEncounterAsync`, `InpatientClinicalContextService` (resolusi profesi, **tanpa** meniru pencocokan teks), `ClinicalInstrumentService.ResolveForPatientAsync` |
| Cakupan | `RehabLookupController`, `RhbLookupService`, `RhbServiceSupport`, `RhbProfessionAuthority`, DTO |
| Dependency | `BE-RHB-005` |
| Acceptance criteria | (1) `/encounters` tanpa `patientId` dijawab 400. (2) Hanya kunjungan milik pasien yang belum berakhir. Kunjungan RWI pasien yang sudah pulang fisik tidak muncul. (3) `/patients` minimal 3 karakter dan maksimal 20 hasil. (4) `/instruments` hanya mengembalikan versi `Approved` (atau konsep bila lingkungan mengizinkan). (5) `RhbProfessionAuthority` mencocokkan `MstProfession.Id`, bukan teks. |
| Verifikasi | API manual dengan dua pasien (uji anti-IDOR) |
| Risiko/pemilik | IDOR lewat lookup. Rehab. |
| DoD | Kriteria terbukti, laporan |

### `BE-RHB-007` ✅ — Permintaan dan disposisi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). 15 endpoint `rehab-requests`, `RhbRequestService`, pembukaan episode (`RhbEpisodeService`), `RhbStatusHistoryWriter`. Build Debug (perintah owner): 0 error, 0 warning Rehab. QBE Strict: 0 violation (8 berkas). Uji API manual: 63/63 sesuai (bagian 1 42/50, 8 `FAIL` karena cara uji lalu diulang 8/8; bagian 2 13/13). Kriteria (1)–(7) terbukti di runtime: UAT-01, UAT-02 (409 `RHB-VAL-007` + kandidat, lanjut dengan alasan), UAT-03 (akun uji `Read`+`Return`: 4 aksi disposisi 403, `return` 200), accept dari Draft/Rejected 409, balapan dua accept 200+409 dengan 0 episode yatim (nomor `RHB-EPS-2026-000003` hangus sesuai `INV-PLT-002`), versi basi 409, log tanpa indikasi. Uji 409 `RHB-VAL-072` yang diteruskan dari `BE-RHB-005` lolos. Delta: `RHB-VAL-078..080`, kunjungan wajib sejak draf. Akun uji sudah dinonaktifkan, dan kebijakan Akses Role uji dikembalikan ke kosong. [Laporan](../task/report/backend/BE-RHB-007.md) |
| Outcome | 15 endpoint `rehab-requests`; `accept` membuka `RhbEpisode` dalam transaksi yang sama; riwayat status |
| Requirement/decision | FR-RHB-001..003; FEAT-001..003; RHB-INV-02, 07; RHB-DEC-008, 010 |
| Kontrak | API v1 *Rehab Request*; state §1; V-REQ-01..09, V-GEN-01/02 |
| Reuse | `NumberSeriesAllocator` (`RHB_REQUEST`, `RHB_EPISODE`) |
| Cakupan | `RehabRequestController`, `RhbRequestService`, `RhbEpisodeService` (bagian pembukaan), penulis `RhbStatusHistory`, DTO |
| Dependency | `BE-RHB-002`, `BE-RHB-006` |
| Acceptance criteria | (1) UAT-01. (2) UAT-02 (409 + kandidat, lanjut dengan alasan). (3) UAT-03 (`Return` ≠ `Disposition`, 403). (4) `accept` dari status selain `Submitted` dijawab 409. (5) Penerimaan gagal di tengah tidak meninggalkan episode yatim. (6) `expectedVersion` basi dijawab 409. (7) Log tanpa indikasi. |
| Verifikasi | API + DB manual; review log |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria 1–7, build owner, laporan |

### `BE-RHB-008` ✅ — Worklist dan ruang kerja episode (baca)

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-05). 6 endpoint `rehab-episodes` (metadata, summary, list, detail, timeline, status-history). Build Debug (perintah owner): 0 error, 0 warning Rehab. QBE Strict: 0 violation (4 berkas). Uji API manual: 28/28. (1) `pageSize=500` dibatasi menjadi 100 dan daftar tanpa kolom sensitif (runtime). (2) Aksi menurut status dan hak: SuperAdmin mendapat 3 aksi, pengguna `RehabEpisode : Read` saja mendapat 0 (runtime); status selain *Menunggu asesmen medis* terbukti lewat kode. (3) Timeline kosong dijawab 200 `[]` (runtime); urutan kronologis terbukti lewat kode, dan uji runtime-nya menyusul di `BE-RHB-009`. Akun uji sudah dinonaktifkan, dan kebijakan Akses Role dikembalikan ke kosong. [Laporan](../task/report/backend/BE-RHB-008.md) |
| Outcome | 6 endpoint `rehab-episodes` (metadata, summary, list, detail, timeline, status-history) |
| Requirement/decision | FEAT-007..029 (tampilan) |
| Kontrak | API v1 *Rehab Episode* |
| Reuse | — |
| Cakupan | `RehabEpisodeController`, `RhbEpisodeService` |
| Dependency | `BE-RHB-007` |
| Acceptance criteria | (1) Daftar tanpa kolom sensitif dan `pageSize` maksimum 100. (2) Detail menyertakan aksi yang tersedia menurut status dan hak pengguna. (3) Timeline kronologis. Tipe dokumen yang belum ada slice-nya tampil kosong, bukan galat. |
| Verifikasi | API manual |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria, laporan |

### `BE-RHB-009` ✅ — Asesmen medis dan profesi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). 2 controller × 6 endpoint (`rehab-medical-assessments`, `rehab-discipline-assessments`), `RhbAssessmentService`. Build Debug (perintah owner): 0 error, 0 warning Rehab. QBE Strict: 0 violation. Uji API manual: medis 27/27, profesi 12/12 (run pertama 28/37 karena skrip belum mengirim `accuracyMeters` saat login akun uji; run lanjutan tanpa perubahan source), instrumen dengan izin konsep 6/6. (1) UAT-04: episode *Perencanaan*, dua layanan diresepkan. (2) UAT-05: 409 `RHB-VAL-021` + addendum RM jenis 47 201, teks asli utuh. (3) 403 `RHB-VAL-031` untuk tanpa profesi dan profesi berbeda. (4) 409 `RHB-VAL-033`. (5) 422 `RHB-VAL-035` tanpa izin konsep. Delta: resep draft disimpan sebagai `RhbEpisodeDiscipline` yang berlaku setelah asesmen final (3 pembaca disaring), `RHB-VAL-081`/`082`, draft medis kedua 409 `RHB-VAL-020`. [laporan](../task/report/backend/BE-RHB-009.md) |
| Outcome | `RehabMedicalAssessmentController`, `RehabDisciplineAssessmentController`, `RhbAssessmentService` (draft, ubah, tanda tangan, batal; pengukuran instrumen + terstruktur; resep disiplin → `RhbEpisodeDiscipline`, episode `Planning`) |
| Requirement/decision | FR-RHB-011..016; RHB-DEC-005, 006, 017 |
| Kontrak | API v1 *Medical/Discipline Assessment*; state §2–3; V-ASM-*, V-MEA-01 |
| Reuse | Gateway RM, mesin instrumen |
| Cakupan | 2 controller, service, DTO |
| Dependency | `BE-RHB-004`, `BE-RHB-008` |
| Acceptance criteria | (1) UAT-04. (2) UAT-05 (409 + addendum RM jenis 47). (3) Profesi tidak cocok dijawab 403 `RHB-VAL-031`. (4) Asesmen medis final kedua dijawab 409 `RHB-VAL-033`. (5) Instrumen konsep di non-dev dijawab 422 `RHB-VAL-035`. |
| Verifikasi | API + DB manual |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria, build owner, laporan |

### `BE-RHB-010` ✅ — Edukasi dan persetujuan

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). 4 endpoint `rehab-education-records`, `RhbEducationService`, dokumen RM jenis 52. Build Debug (perintah owner): 0 error, 0 warning Rehab. QBE Strict: 0 violation. Uji API manual 26/26. (1) tanda tangan dengan `NotAssessed` → 422 `RHB-VAL-083`. (2) `Refused`/`PartiallyAgreed` tanpa catatan → 422 `RHB-VAL-083`, saat simpan dan ubah. (3) ubah/tanda tangan ulang sesudah final → 409 `RHB-VAL-021`; addendum RM jenis 52 201. Delta: `RHB-VAL-083`/`084`; kontrak v1 tanpa aksi batal draft. [laporan](../task/report/backend/BE-RHB-010.md) |
| Outcome | 4 endpoint `rehab-education-records`; dokumen jenis 52 |
| Requirement/decision | FR-RHB-020; FEAT-015 |
| Kontrak | API v1 *Rehab Education* |
| Reuse | Gateway RM |
| Cakupan | `RehabEducationController`, `RhbEducationService`, DTO |
| Dependency | `BE-RHB-004`, `BE-RHB-008` |
| Acceptance criteria | (1) Tanda tangan dengan `NotAssessed` ditolak. (2) `Refused` tanpa catatan ditolak. (3) Edukasi final tidak bisa diubah. |
| Verifikasi | API manual |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria, laporan |

### `BE-RHB-011` ✅ — Tujuan dan program berversi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-06). 5 endpoint `rehab-goals` + 8 endpoint `rehab-programs`, `RhbGoalService`, `RhbProgramService`, dokumen RM jenis 48. Build Debug (perintah owner): 0 error, 0 warning Rehab. QBE Strict: 0 violation. Uji API manual 42/42. (1) UAT-06: 422 dengan daftar prasyarat (042/043/044/045/041 di runtime; 040 lewat kode). (2) UAT-07: program Aktif + episode Terapi berjalan. (3) v1→v2: tepat satu aktif per layanan, v1 Superseded. (4) dua PUT bersamaan → 200 + 409 `RHB-VAL-010`. (5) 409 `RHB-VAL-046`. Delta: `errors.details` daftar prasyarat, `revise` tujuan 201, `authoredEncounterId` opsional pada revisi program, aktivasi oleh penulis. [laporan](../task/report/backend/BE-RHB-011.md) |
| Outcome | `RehabGoalController` + `RehabProgramController`; aktivasi = tanda tangan (jenis 48) + versi lama `Superseded` + episode `Active` dalam satu transaksi |
| Requirement/decision | FR-RHB-010, 021, 022; RHB-DEC-005, 011; RHB-INV-03, 10 |
| Kontrak | API v1 *Rehab Goal*, *Rehab Program*; state §4, §8; V-PRG-*, V-GOL-* |
| Reuse | Gateway RM; advisory lock (pola HD) untuk `ProgramVersion` |
| Cakupan | 2 controller, 2 service, DTO |
| Dependency | `BE-RHB-009`, `BE-RHB-010` |
| Acceptance criteria | (1) UAT-06 (422 daftar prasyarat `RHB-VAL-040..045`). (2) UAT-07. (3) Revisi v1→v2: satu aktif per disiplin. (4) Simpan bersamaan dijawab 409. (5) Tujuan yang dipakai program aktif tidak bisa di-`PUT` (409 `RHB-VAL-046`). |
| Verifikasi | API + DB manual |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria, build owner, laporan |

### `BE-RHB-012` ✅ — Sesi terapi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). 12 endpoint `rehab-sessions`, `RhbSessionService`, dokumen RM jenis 49, nomor `RHB-SES`. Build owner: 0 error (426 warning bawaan, tidak ada di Rehab). QBE Strict: 0 violation. Uji API manual: 43/44 + ulang addendum 5/5 (FAIL karena uji memakai aktor bukan penulis; aturan RM benar). (1) UAT-09 422 `051`. (2) `RHB-VAL-052` **terbukti lewat kode** — DB dev tidak punya rawat inap pulang fisik. (3) 422 `053`. (4) UAT-12: 422 `056` lalu *Dihentikan dini* + program perlu telaah. (5) 409 `020`. (6) `NotDone` tanpa intervensi. (7) billing tetap `NotRequired`, 0 tindakan bertarif — **jangan go-live sebelum `BE-RHB-013`**. Delta: `PUT` untuk sesi selesai/henti dini yang belum final; kunjungan `not-done` bawaan dari program; `programInterventionId` di rincian program. [laporan](../task/report/backend/BE-RHB-012.md) |
| Outcome | 12 endpoint `rehab-sessions` (tanpa billing); dokumen jenis 49; deviasi menyetel `NeedsReview` |
| Requirement/decision | FR-RHB-030..032, 036, 037; FEAT-016..018; RHB-INV-04, 06; RHB-DEC-009, 013 |
| Kontrak | API v1 *Rehab Session*; state §5; V-SES-01..10 |
| Reuse | `RhbServiceSupport`, gateway RM, `NumberSeriesAllocator` (`RHB_SESSION`) |
| Cakupan | `RehabSessionController`, `RhbSessionService`, DTO |
| Dependency | `BE-RHB-011` |
| Acceptance criteria | (1) UAT-09. (2) Pasien RWI sudah pulang dijawab 422 `RHB-VAL-052`. (3) Tanpa verifikasi identitas dijawab 422. (4) UAT-12. (5) Tanda tangan saat `InProgress` dijawab 409. (6) `NotDone` tersimpan tanpa intervensi. (7) Hingga `BE-RHB-013` selesai, tanda tangan **tidak** membentuk tagihan (`BillingHandoffStatus` tetap `NotRequired`), dan hal ini tertulis di laporan. |
| Verifikasi | API + DB manual |
| Risiko/pemilik | Sesi yang ditandatangani sebelum `BE-RHB-013` aktif tidak tertagih. Mitigasi: jangan go-live sebelum `BE-RHB-013`. Untuk sesi lama, sediakan backfill handoff lewat `retry` di `BE-RHB-013`. Rehab. |
| DoD | Kriteria, build owner, laporan |

### `BE-RHB-013` ✅ — Handoff billing sesi

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). `RhbBillingHandoffService`: `TrxPatientProcedure` per intervensi di transaksi tanda tangan (`RHB-SESINT-{id}`, `RehabilitationSession = 5`, dokter = PJ episode), fakta charge per tindakan sesudah commit, retry idempoten + backfill sesi lama; `RehabSessionBillingController` (`GET`/`retry`, Resource `RehabBillingHandoff`). Tanpa migration. Build owner: 0 error (426 warning bawaan). QBE Strict `PASS`. Uji API 11/13 (dua FAIL dari data uji: pemetaan tindakan tanpa tarif nonaktif) lalu uji ulang 3/3. DB: 5 tindakan rehab, masing-masing tepat 1 fakta + 1 charge line; tindakan bertarif *Synced* ke invoice. AC (1) UAT-08, (2) UAT-10, (3) retry tanpa ganda — delta: tindakan tanpa tarif diterima Billing (`TARIFF_NOT_FOUND` di Billing), bukan *Gagal*, (4) backfill `RHB-SES-2026-000001`, (5) pesan galat tanpa data klinis (kode). Persetujuan `RHB-OQ-015` ([bukti](../evidence/RHB-OQ-015-persetujuan-clinical-billing.md)). [laporan](../task/report/backend/BE-RHB-013.md) |
| Outcome | Saat tanda tangan, `TrxPatientProcedure` dibentuk per intervensi (`IdempotencyKey RHB-SESINT-{id}`, `ProcedureSource = RehabilitationSession`, `DoctorId` = penanggung jawab episode). Fakta charge dipancarkan sesudah commit. `RehabSessionBillingController` (status + retry). `PatientProcedureSource.RehabilitationSession = 5`. |
| Requirement/decision | FR-RHB-033..035; FEAT-032; RHB-DEC-021 |
| Kontrak | API v1 *Rehab Billing Handoff*; INT-RHB-08/09; state §6 |
| Reuse | `HmdSessionService` (pembentukan tindakan), `HmdBillingHandoffService` (emisi + status), `ClinicalMilestoneFactProducer`, `BillingSourceContract.ProcedureSourceContext` |
| Cakupan | `RhbBillingHandoffService`, controller, perubahan di `RhbSessionService.SignAsync`, `ClinicalManagement/Enums/PatientProcedureSource.cs` |
| Dependency | `BE-RHB-012`, `{RHB-OQ-015}` |
| Acceptance criteria | (1) UAT-08 (2 item tagihan). (2) UAT-10 (`NotDone` tanpa tagihan). (3) UAT-11 (`Failed` → retry → tanpa ganda). (4) Sesi final lama (tertanda tangan sebelum task ini) dapat diserahkan lewat `retry`. (5) `BillingHandoffError` tanpa data klinis. |
| Verifikasi | API + DB + cek invoice di modul Billing |
| Risiko/pemilik | Konsumen Billing yang memfilter `ProcedureSource`. Pemilik Billing/Cli. |
| DoD | Kriteria, persetujuan pemilik di laporan, build owner |

### `BE-RHB-014` ✅ — Reasesmen dan keputusan

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). 6 endpoint `rehab-progress-reviews` (5 kontrak + `cancel`), `RhbProgressReviewService`, `RhbMeasurementRules` (aturan pengukuran bersama, refactor perilaku-netral dari `BE-RHB-009`, regresi 2/2), dokumen RM jenis 50. Build owner: 0 error (426 warning bawaan, tidak ada di Rehab). QBE Strict: 0 violation. Uji API: 23/26 lalu uji ulang 11/11 — tiga FAIL karena build yang tidak memuat perubahan (dua) dan urutan pemeriksaan (satu, diperbaiki). (1) 422 `RHB-VAL-036` (satuan berbeda). (2) Hentikan → program *Stopped*. (3) Ubah program → `NeedsReview`. (4) pembanding baseline 320 → 400 m, selisih +80. Delta: `POST /{id}/cancel`, `RHB-VAL-085`, riwayat `ready-to-close` untuk reasesmen medis. Residu: 2 draft uji di EP1. [laporan](../task/report/backend/BE-RHB-014.md) |
| Outcome | 5 endpoint `rehab-progress-reviews`; efek keputusan saat tanda tangan; dokumen jenis 50 |
| Requirement/decision | FR-RHB-040, 041; FEAT-024, 025 |
| Kontrak | API v1 *Rehab Progress Review*; state §7; V-MEA-02 |
| Reuse | Gateway RM; mesin instrumen |
| Cakupan | `RehabProgressReviewController`, `RhbProgressReviewService`, DTO |
| Dependency | `BE-RHB-011` |
| Acceptance criteria | (1) Instrumen beda tanpa alasan dijawab 422 `RHB-VAL-036`. (2) `Stop` → program `Stopped`. (3) `Modify` → `NeedsReview`. (4) Detail menampilkan baseline pembanding dengan instrumen dan satuan yang sama. |
| Verifikasi | API + DB manual |
| Risiko/pemilik | — Rehab |
| DoD | Kriteria, laporan |

### `BE-RHB-015` ✅ — Ringkasan akhir, penutupan episode, pembaca berkas RM

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-11-09). 3 endpoint `rehab-discharge-summaries`, `RhbDischargeService`, `RhbMedicalRecordDocumentSource` (jenis 47–52, didaftarkan di `Program.cs`). Build owner: 0 error (426 warning bawaan, tidak ada di Rehab). QBE Strict: 0 violation. Uji API: 20/22 lalu uji ulang 8/8 (dua FAIL karena susunan skrip: V-DIS-01 menolak lebih dulu). (1) UAT-13: EP4 *Closed*, program aktif → *Completed* (varian reasesmen *Lanjutkan*). (2) UAT-14: 409 `060` + daftar sesi berlangsung; EP1 409 dengan 2 draft reasesmen residu. (3) 409 `023` 4/4. (4) rincian pasien lain → 404. (5) berkas RM memuat 6 jenis dokumen final + tanda pernah diralat. (6) UAT-16: sapuan statis 13 controller + runtime 905 baris log Rehab, 0 data klinis. Residu: EP1 tetap terbuka (draft reasesmen). [laporan](../task/report/backend/BE-RHB-015.md) |
| Outcome | 3 endpoint `rehab-discharge-summaries`; tanda tangan (jenis 51) → episode `Closed` + program aktif `Completed`; `RhbMedicalRecordDocumentSource` untuk jenis 47–52, didaftarkan di `Program.cs` |
| Requirement/decision | FR-RHB-050..052, 060, 061; FEAT-026..029 |
| Kontrak | API v1 *Rehab Discharge Summary*; INT-RHB-06; V-DIS-01/02 |
| Reuse | Pola `HmdMedicalRecordDocumentSource` |
| Cakupan | `RehabDischargeSummaryController`, `RhbDischargeService`, `RhbMedicalRecordDocumentSource`, `Program.cs` |
| Dependency | `BE-RHB-012`, `BE-RHB-014` |
| Acceptance criteria | (1) UAT-13. (2) UAT-14 (409 + daftar). (3) Dokumen baru pada episode `Closed` dijawab 409 `RHB-VAL-023`. (4) `ReadDetailAsync` untuk pasien lain mengembalikan `null`. (5) Berkas RM menampilkan dokumen final rehab beserta addendumnya. (6) UAT-16: log bebas data klinis (sapuan seluruh modul). |
| Verifikasi | API + DB manual; buka berkas RM pasien uji |
| Risiko/pemilik | Persetujuan RM (OQ-012). Rehab/RM. |
| DoD | Kriteria, build owner, laporan |

## POST-MVP (tidak dijadwalkan)

| Kemampuan | Feature | Penghalang |
|---|---|---|
| Antrean, jadwal seri, kehadiran | FEAT-004..006 | UNR-005 |
| Koordinasi multidisiplin | FEAT-014 | Desain konferensi tim |
| Ortotik-prostetik | FEAT-011, 019..021 | RHB-DEC-004, pengadaan |
| Program rumah + teach-back | FEAT-022, 023 | Media edukasi |
| SATUSEHAT + antrean kirim ulang | FEAT-030, 031 | UNR-008 |
| Rekonsiliasi klaim | FEAT-033 | UNR-006, 007 |
| Perluasan mesin instrumen | RHB-OQ-013 | Pemilik Cli |
| Tautan CPPT ↔ sesi | RHB-OQ-014 | Pemilik CPPT |

## Catatan revisi

| Revision | Tanggal | Perubahan | Dasar |
|---|---|---|---|
| 1 | 2026-11-05 | Roadmap awal | Blueprint `RHB-BP-001` r1 |
| 2 | 2026-11-05 | Kriteria (2) `BE-RHB-003` dibatasi ke sisi backend. Pilihan jenis instrumen di FE pindah ke `FE-RHB-002` (frontend-roadmap r2). Dependency dan grafik tidak berubah. | Temuan laporan `BE-RHB-003` |
