# Blueprint Manifest — Platform Authorization & Access Control

| Field | Nilai |
|---|---|
| `blueprint_id` | `SEC-BP-001` |
| `module_name` | Platform Authorization & Access Control |
| `module_slug` | `platform-authorization` |
| `module_prefix` | `Sec` — *Security*. Disetujui pemilik sistem 2 September 2026; belum didaftarkan ke registry, lihat bagian *Prefix entity operasional* |
| `revision` | `2` |
| `status` | `approved` |
| Category | `SHARED PLATFORM CAPABILITY` |
| Backend task prefix | `BE-SEC` |
| Frontend task prefix | `FE-SEC` — **usulan**, menunggu penetapan frontend authority |
| Product/domain owner | Pemilik sistem |
| Security owner | Pemilik sistem |
| Frontend authority | Belum ditetapkan — dibutuhkan sebelum `FE-SEC-001` dimulai |
| `approved_by` / `approved_at` | Pemilik sistem / 1 September 2026; revisi 2 disetujui 2 September 2026 |
| Backend SHA saat Phase A0 | `43ba35d934d1615edbeac9952e0f19e3cca353fd` |
| Backend SHA saat audit `BE-SEC-002` | `e1d112142510baa86ccd89977bc7189c89ed012b` |
| Frontend SHA saat audit `BE-SEC-002` | `2b9e3b074f8a3839857e123515353dd2f3233ac3` |
| Compatibility | Tidak memutus endpoint existing; seluruh perubahan bersifat aditif atau pemulihan |

---

## 1. Mengapa blueprint ini ada

Hak akses Quilvian bukan milik satu modul bisnis. Ia dipakai bersama oleh Rawat Inap, IGD,
Billing, Farmasi, Laboratorium, Rekam Medis, Human Resource, dan seluruh modul lain. Sebelum
blueprint ini ada, pekerjaan yang menyentuh otorisasi tidak punya tempat tinggal: menaruhnya di
blueprint modul bisnis mana pun akan salah arsip, karena tidak satu pun modul memilikinya.

Blueprint ini menjadi rumah bagi kemampuan bersama tersebut, sejajar dengan pola
`SHARED PLATFORM CAPABILITY` yang sudah dipakai Workflow.

---

## 2. Cakupan

Yang **termasuk**:

| Cakupan | Isi |
|---|---|
| Integritas registry permission | Identitas permission kanonik, pendaftaran kemampuan ke layar Akses Role, rekonsiliasi baris usang |
| Proyeksi otorisasi organisasi | Penurunan penempatan organisasi otoritatif menjadi proyeksi yang dipakai pemeriksaan izin |
| Resolusi izin efektif | Gabungan izin dari seluruh penempatan yang sah milik satu akun |
| Infrastruktur berdampingan autentikasi | Filter, atribut, validator startup, dan pemeriksaan kelayakan penempatan |
| Fondasi Business Permission | Dasar yang dipakai fase berikutnya, tanpa mendahuluinya |

Yang **tidak termasuk**:

- Blueprint Human Resource, Billing, Operasi, Rekam Medis, dan modul bisnis lain. Blueprint ini
  tidak pernah memiliki entity operasional mereka.
- Perbaikan route `doctor-certificates` → `medical-certificates` pada frontend. Ini cacat kontrak
  yang sudah ada; perbaikannya task terpisah dan **tidak boleh** diselipkan ke task `BE-SEC` mana pun.

Yang **sudah masuk cakupan sejak revisi 2**, dengan task ID masing-masing:

| Cakupan | Task |
|---|---|
| Pemecahan granularitas technical permission | `BE-SEC-003` |
| Business Permission catalog dan pemetaan teknis | `BE-SEC-004`, `BE-SEC-005` |
| Access Profile dan penetapan organisasi | `BE-SEC-006` |
| Resolver dua sumber | `BE-SEC-007`, `BE-SEC-008` |
| API admin dan `/api/access/me` | `BE-SEC-009`, `BE-SEC-010` |
| Baseline Self Service otomatis | `BE-SEC-011` — belum didekomposisi |
| Otorisasi frontend dan Layar Manajemen Hak Akses | `FE-SEC-001`, `FE-SEC-002`, `FE-SEC-003` |

Tidak satu pun dari task di atas sudah dikerjakan.

---

## 3. Prefix entity operasional

Phase A0 (`BE-SEC-001`) **tidak membuat satu pun entity operasional baru**. Yang ditambahkan hanya
satu kolom pada tabel platform yang sudah ada (`AspNetUserOrganization.SourceAssignmentId`) beserta
dua service dan satu validator yang tidak dipersistensi.

### 3.1 Keputusan prefix — `Sec`

Audit `BE-SEC-002` menetapkan bahwa fase Business Permission **akan** membuat entity persisted.
Pemilik sistem menyetujui baris registry berikut pada 2 September 2026:

| Area | Module/pemilik | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| `Administrator` | `PlatformAuthorization / Platform Authorization & Access Control` | `SHARED PLATFORM CAPABILITY` | `Sec` | `ACTIVE` |

Kepanjangan: `Sec` = *Security*.

Audit collision pada SHA `e1d1121` membuktikan tidak ada tabrakan: nol class, nol tabel, nol berkas,
nol `DbSet` berawalan `Sec`, dan `Sec` belum ada di 18 baris prefix yang terdaftar.

### 3.2 Baris registry belum ditambahkan

Baris di atas **belum** ditulis ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.

Instruksi pemilik sistem: registry **tidak** diperbarui pada `BE-SEC-003`, karena task tersebut
tidak membuat satu pun model persisted `Sec*`. Penambahan baris registry menjadi **langkah pertama
`BE-SEC-004`**, sebelum berkas model pertama dibuat, sesuai `QBE-MOD-002` dan `QBE-MOD-003`.

### 3.3 Status `Sys*` warisan

`SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`, dan `SysAccessPolicy` tetap
**legacy/technical system registry**. Keempatnya tidak dinormalisasi, tidak diganti nama, dan
tidak didaftarkan sebagai milik `PlatformAuthorization`.

### 3.4 Nama entity yang direncanakan

`SecBusinessFeature`, `SecBusinessPermission`, `SecBusinessPermissionMapping`, `SecAccessProfile`,
`SecAccessProfilePermission`, `SecOrganizationAccessProfile`, `SecOrganizationPermissionGrant`.
Tujuh entity, seluruhnya belum dibuat.

---

## 4. Task

`BE-SEC-002` diklasifikasikan **`EPIC`** menurut `rules/backend/TASK_CLASSIFICATION.md`, sehingga
tidak pernah dikerjakan sebagai satu task. Ia didekomposisi menjadi 11 task terbatas.

| Task ID | Judul | Klasifikasi | Status |
|---|---|---|---|
| `BE-SEC-001` | Authorization Integrity Foundation | `HEAVY` | `COMPLETED` |
| `BE-SEC-002` | Business Permission & Access Profile Architecture | `EPIC` | **`CLOSED / READY FOR COMMIT`** — tidak menjadi task implementasi |
| `BE-SEC-003` | Technical Permission Granularity Hardening — pilot Dokter Rawat Jalan | `HEAVY` | `READY TO IMPLEMENT` — task berikutnya |
| `BE-SEC-004` | Registry prefix `Sec` dan skema katalog Business Permission | `HEAVY` | `NOT STARTED` |
| `BE-SEC-005` | Isi katalog Business Permission pilot | `MEDIUM` | `NOT STARTED` |
| `BE-SEC-006` | Skema Access Profile dan penetapan organisasi | `HEAVY` | `NOT STARTED` |
| `BE-SEC-007` | Resolver Business Permission mode bayangan | `MEDIUM` | `NOT STARTED` |
| `BE-SEC-008` | Aktivasi sumber izin kedua | `HEAVY` | `NOT STARTED` |
| `BE-SEC-009` | API admin Business Access | `HEAVY` | `NOT STARTED` |
| `BE-SEC-010` | `GET /api/v1/access/me` | `MEDIUM` | `NOT STARTED` |
| `BE-SEC-011` | Baseline Self Service otomatis | `TBD` | `NOT DECOMPOSED` — menunggu keputusan HR |
| `FE-SEC-001` | State izin frontend dan perbaikan guard | `HEAVY` | `NOT STARTED` — menunggu frontend authority |
| `FE-SEC-002` | Layar Manajemen Hak Akses business-oriented | `HEAVY` | `NOT STARTED` — menunggu frontend authority |
| `FE-SEC-003` | Guard tab dan tombol Dokter Rawat Jalan | `MEDIUM` | `NOT STARTED` — menunggu frontend authority |

Rincian scope, dependency, acceptance criteria, dan batas rollback setiap task ada di
`roadmap/backend-roadmap.md`.

---

## 5. Keputusan arsitektur final

Ditetapkan pemilik sistem 2 September 2026, berdasarkan audit `BE-SEC-002`.

| # | Keputusan | Isi |
|---|---|---|
| `D-ARCH-1` | Runtime architecture | **Approach A.** Endpoint tetap menegakkan `[AccessPermission(resource, action)]`. Business Permission adalah lapisan di atasnya. Resolver transisional: `EffectiveTechnicalPermissions(user) = LegacyTechnicalPermissions(user) UNION TechnicalPermissions(EffectiveBusinessPermissions(user))`. Sumber Business Permission dapat dinyalakan dan dimatikan selama migrasi |
| `D-ARCH-2` | Legacy | `SysAccessPolicy` tidak dihapus pada fase awal. Tidak boleh ada *silent privilege loss* maupun *privilege broadening* |
| `D-ARCH-3` | Granularity | 19 Business Permission hasil audit berstatus **candidate catalog**. Business Permission yang enforcement teknisnya belum granular **tidak boleh** diaktifkan |
| `D-ARCH-4` | Prefix | `Sec` = *Security*, `Administrator / PlatformAuthorization`, `SHARED PLATFORM CAPABILITY`, `ACTIVE` |
| `D-ARCH-5` | Access Profile | Bundel izin yang dapat dipakai ulang. Satu Departemen + Posisi boleh punya lebih dari satu profil. Izin efektif = UNION. Override langsung **hanya ADDITIVE GRANT** — tidak ada subtractive DENY dan tidak ada DENY precedence |
| `D-ARCH-6` | Procedure | `Approve` sensitif dan terpisah, default **fail closed**. `Execute` permission terpisah, tidak otomatis untuk seluruh dokter. `Cancel` berkonsekuensi finansial, bukan generic `Update` |
| `D-ARCH-7` | Consultation | `WriteSoap` dan `Complete` adalah capability berbeda. `Complete` adalah transisi workflow, bukan CRUD update |
| `D-ARCH-8` | Queue audio | Endpoint audio mengizinkan `QueueVoice.PlayAudio` **ATAU** `QueueDisplayRuntimeRead` — **OR**, bukan AND. Actor manusia (dokter, perawat) lewat `QueueVoice.PlayAudio`; perangkat display lewat policy yang sudah ada. `AllowAnonymous` ditolak. `QueueDisplayRuntimeRead` **tidak boleh** menjadi permission user dokter/perawat. Implementasi wajib memakai satu mekanisme yang benar-benar menghasilkan OR — dua atribut yang menghasilkan AND dilarang |
| `D-ARCH-10` | Penerima `QueueVoice.PlayAudio` | Delapan Departemen × Posisi pemegang izin antrean, mencakup 17 pengguna aktif. Keperawatan × Bidan dikecualikan karena tidak memegang izin antrean. Ditetapkan sebagai keputusan `O-1` |
| `D-ARCH-9` | Medical Certificate | Tetap `BROKEN_DEPENDENCY`. Tidak dipecah, tidak dimigrasi, tidak dipetakan sampai route frontend diperbaiki task terpisah |

---

## 6. Dokumen terkait

| Dokumen | Isi |
|---|---|
| `roadmap/backend-roadmap.md` | Daftar task beserta scope, dependency, acceptance criteria, dan batas rollback |
| `roadmap/requirement-traceability.md` | Hubungan requirement dengan bukti implementasi |
| `evidence/01-be-sec-002-audit-architecture.md` | Audit dan arsitektur `BE-SEC-002` — **architecture evidence**, bukan laporan task |
| `evidence/02-be-sec-002-decision-closure.md` | Decision closure dan epic decomposition — **architecture evidence** |
| `evidence/03-be-sec-003-pre-implementation-impact.md` | Impact report `BE-SEC-003` beserta hasil query read-only database development |
| `task/report/backend/BE-SEC-001.md` | Laporan tracked Phase A0 |
