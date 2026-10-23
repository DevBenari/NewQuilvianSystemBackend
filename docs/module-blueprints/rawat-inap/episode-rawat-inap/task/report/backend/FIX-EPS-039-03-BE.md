# Laporan Perubahan Backend — `FIX-EPS-039-03-BE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-039-03-BE` (bagian backend `FIX-EPS-039-03`) |
| Judul | Data rujukan faskes luar disimpan pada episode rawat inap (K-01 = opsi B) |
| Slice | Repair admisi rawat inap — jenis kunjungan Rujukan pada Pasien Baru dan Pasien Lama |
| Roadmap | **Tidak ada entri roadmap.** Register status dipegang [PLAN-REPAIR-EPS-039](../../../docs/plan-repair/plan-repair-039-audit-lima-tipe-pendaftaran-admisi.md) bagian 1, sama seperti `FIX-EPS-038`/`-039` |
| Trace | [ISSUE-EPS-039](../../../docs/issue/issue-039-audit-lima-tipe-pendaftaran-admisi.md) `ISS-EPS-039-03`; `RWI-DEC-269`, `RWI-AC-390`, `FR-RI-180`; keputusan pemilik K-01 = B, 23-10-2026 |
| Contract version | Perluasan additive `POST /episodes` dan `GET /episodes/{id}` (field opsional `externalReferral`); tidak ada field lama yang berubah |
| Dependency | Tidak ada. Task frontend `FIX-EPS-039-03` bergantung pada task ini |
| Klasifikasi | `MEDIUM` — satu entity existing (5 kolom), 2 DTO, 1 service, 1 migration additive |
| Task mode | `BACKEND` (frontend baca-saja) |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `Migrations/**`, laporan ini |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `488faa22` (`MHamzah`) |
| Tanggal | 2026-10-23 |
| Status | Source, migration, build, dan QBE Strict selesai; migration diterapkan pemilik 23-10-2026 (`dotnet ef database update`, bersama 11 migration modul lain yang tertunda sejak 15-10-2026); verifikasi runtime belum dilakukan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Pemilik / prefix registry | Inpatient / `Inp`, lifecycle `ACTIVE` (`RWI-DEC-068`) |
| Keberlakuan | `TOUCHED LEGACY` — menambah kolom pada entity existing `InpEpisode`; tidak ada entity, tabel, atau prefix baru |
| QBE yang berlaku | `QBE-DTO-001` (entity tidak diekspos; DTO baru), `QBE-VAL-001` (validasi request dan invarian), `QBE-API-001` (envelope dan status existing), `QBE-ENT-002` (nullability semantik), `QBE-AUD-001` (tidak menyentuh audit) |
| QBE yang tidak berlaku | `QBE-MOD-002/003`, `QBE-NAM-002/004` (tidak ada model baru), `QBE-CODE-*` (tidak ada nomor bisnis), `QBE-PERM-001` (tidak ada endpoint baru; metadata akses existing dipakai) |

---

## 1. Masalah yang diperbaiki

Pada jenis kunjungan "Rujukan", layar admisi mewajibkan nomor surat rujukan, tanggal dan jam,
faskes perujuk, dokter perujuk, dan diagnosa. Namun tidak ada satu pun field di server untuk
menyimpannya, sehingga datanya hilang begitu admisi selesai. Endpoint rujukan kunjungan modul
Pendaftaran (opsi A) ternyata tidak mendukung kunjungan rawat inap: tujuan tercatat
"Laboratorium" dan faskes wajib mitra. Pemilik memutuskan opsi B: menyimpannya di episode rawat
inap sebagai teks, sesuai `RWI-DEC-269`.

---

## 2. Proses bisnis

1. Petugas memilih jenis kunjungan **Rujukan** lalu mengisi data surat rujukan.
2. Saat episode Draft dibuat (`POST /episodes`), layar mengirim objek `externalReferral`.
3. Server memeriksa kelengkapan dan tanggal, lalu menyimpan kelima isian pada episode.
4. Data rujukan dibaca kembali lewat `GET /episodes/{id}` untuk langkah Konfirmasi dan pelanjutan.

Aturan:

| Aturan | Contoh |
| --- | --- |
| Objek rujukan opsional; kunjungan Umum tidak mengirimnya | Pasien datang sendiri → episode tanpa rujukan, sama seperti sebelumnya |
| Bila objek dikirim, nomor, tanggal dan jam, faskes, dan diagnosa wajib; dokter perujuk opsional | Nomor "445/RS-X", faskes "Puskesmas Sejahtera", diagnosa "DBD derajat II", dokter kosong → diterima |
| Panjang maksimum: nomor 250, faskes 200, dokter 150, diagnosa 1000 karakter | Nomor 251 karakter → `400` "Nomor surat rujukan paling banyak 250 karakter." |
| Tanggal rujukan tidak boleh melewati waktu sekarang (toleransi 5 menit) | Rujukan bertanggal besok → `400` "Tanggal dan jam rujukan tidak boleh melewati waktu sekarang." |
| Waktu tanpa zona dianggap UTC, sama seperti `EncounterReferralService.ToUtc`; layar mengirim UTC | `2026-10-23T02:30:00Z` tersimpan apa adanya (09.30 WIB) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`,
`blocker-rantai-migration.md`; `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md`, `API_RULES.md`,
`TEST_POLICY.md`; `InpEpisode.cs`, `InpEpisodeConfiguration.cs`, `InpatientEpisodeDtos.cs`,
`InpatientNewbornDtos.cs`, `InpEpisodeService.cs` (pembuatan episode dan proyeksi detail),
`InpatientEpisodeController.cs`, `EncounterReferralService.cs` (pola UTC dan alasan opsi A gagal),
`Migrations/20261022130000_MakeTransferHandoverPlacementsNullable.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` | 5 kolom nullable: `ExternalReferralNumber` (250), `ExternalReferralDateTime`, `ExternalReferralFacilityName` (200), `ExternalReferralDoctorName` (150), `ExternalReferralDiagnosis` (1000) |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | DTO baru `InpatientAdmissionExternalReferral` dengan pesan validasi berbahasa Indonesia; properti `ExternalReferral` pada `OpenAdmissionRequest` dan `InpatientEpisodeDetailResponse` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | `ValidateExternalReferral` (kelengkapan, tanggal masa depan), `ApplyExternalReferral` (trim, UTC) dipanggil saat episode dibuat; proyeksi `ExternalReferral` pada detail |
| `Migrations/20261023062116_AddInpEpisodeExternalReferral.cs` (+ `.Designer.cs`) | 5 `AddColumn` nullable pada `public."InpEpisode"`; `Down` menghapus kelima kolom |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Lima kolom baru, **ditambah** penyelarasan `CliTransferHandover` (lihat 3.3) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Additive: `externalReferral` opsional pada request `POST /episodes` dan response `GET /episodes/{id}`. Konsumen lama tidak terdampak |
| Database | Migration `20261023062116_AddInpEpisodeExternalReferral`: 5 kolom nullable, tanpa data default, tanpa indeks. Sudah diterapkan pemilik 23-10-2026 (`dotnet ef database update`, bersama 11 migration modul lain yang tertunda sejak 15-10-2026). Percobaan pertama dengan `--no-build` di atas build Debug yang tidak memuat Designer dan snapshot menjawab "No migrations were applied … Done." tanpa menerapkan apa pun, gejala yang tercatat di `QuilvianSystemBackend.csproj` (blok `SkipMigrationMetadata`). Setelah build biasa, migration diterapkan. Saat dibuat otomatis, EF juga memunculkan perubahan `CliTransferHandover` karena migration manual `20261022130000_MakeTransferHandoverPlacementsNullable` tidak memperbarui snapshot. Operasi itu **dibuang** dari migration ini agar tidak terulang; snapshot diselaraskan. `dotnet ef migrations has-pending-model-changes` menjawab tidak ada perubahan tersisa |
| Keamanan/Auth | Tidak ada endpoint baru; `[AccessPermission("InpatientEpisode", "Create"/"Read")]` existing berlaku. Data rujukan tidak dimasukkan ke log |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuka admisi; kini menerima `externalReferral` opsional | `InpatientEpisode : Create` |
| `GET` | `/{id}` | Detail episode; kini mengembalikan `externalReferral` (null bila tidak ada) | `InpatientEpisode : Read` |

Bentuk `externalReferral`:

| Field | Tipe | Wajib bila objek dikirim | Batas |
| --- | --- | :---: | --- |
| `referralNumber` | string | Ya | 250 |
| `referralDateTime` | datetime (UTC) | Ya | tidak di masa depan |
| `referralFacilityName` | string | Ya | 200 |
| `referralDoctorName` | string | Tidak | 150 |
| `referralDiagnosis` | string | Ya | 1000 |

Kode status: `400` bila isian rujukan tidak lengkap, terlalu panjang, atau bertanggal di masa
depan; kode lain tidak berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Berhasil, 0 error; tidak ada warning dari berkas yang diubah | `PASS` | Keluaran build. Konfigurasi Release dipakai karena aplikasi pemilik sedang berjalan dari `bin\Debug` dan mengunci berkasnya |
| `dotnet ef migrations add AddInpEpisodeExternalReferral -c Release --no-build` | Migration terbentuk | `PASS` | Berkas migration dan Designer |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah, setelah migration dirapikan |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (4 berkas) | 0 VIOLATION, 0 REVIEW | `PASS` | Laporan QBE |
| Verifikasi runtime `POST`/`GET` | Tidak dijalankan | `NOT RUN` | Butuh migration diterapkan dan aplikasi dijalankan ulang; keduanya wewenang pemilik |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (plan repair `FIX-EPS-039-03`) | Status | Bukti |
| --- | --- | --- |
| AC1 — `POST /episodes` menerima dan `GET /episodes/{id}` mengembalikan kelima field | Terpenuhi di source; runtime belum | DTO, `ApplyExternalReferral`, proyeksi detail |
| AC2 — tanpa rujukan, perilaku lama tidak berubah | Terpenuhi di source | Objek opsional; `ApplyExternalReferral` keluar bila null |
| AC3 — rujukan tidak lengkap atau tanggal di masa depan → `400` berbahasa Indonesia | Terpenuhi di source; runtime belum | Atribut DTO + `ValidateExternalReferral` |
| AC4 — migration, Designer, snapshot tersedia; database diperbarui pemilik | Terpenuhi — migration diterapkan pemilik 23-10-2026 (`dotnet ef database update`, bersama 11 migration modul lain yang tertunda sejak 15-10-2026) | `20261023062116_AddInpEpisodeExternalReferral` |

**Belum terpenuhi:** verifikasi runtime; koreksi data rujukan lewat
`PUT /episodes/{id}` (di luar scope, lihat catatan).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning paket `SixLabors.ImageSharp` (NU1902/NU1903) dan warning build lain sudah ada sebelumnya; bukan dari task ini |
| Masalah yang diketahui | Rujukan belum dapat dikoreksi setelah episode dibuat (`UpdateAdmissionRequest` tidak membawanya). Snapshot model juga memuat penyelarasan `CliTransferHandover` milik migration `20261022130000`; ini sengaja, bukan perubahan skema baru |
| Dependency backend | `NONE` |
| Perubahan sampingan | Penyelarasan snapshot `CliTransferHandover` (dipertahankan, alasan di 3.3). Folder `bin/Release` dan `obj` hasil build tidak di-track |
| Status Git | `MHamzah` @ `488faa22`: berkas task ini ` M` (model, DTO, service, snapshot) dan `??` (migration + Designer, laporan ini). Perubahan pihak lain yang tidak disentuh: `PatientBillingSummaryService.Breakdown.cs`, `InpEpisodeService.NewbornCandidates.cs` |
| Langkah berikutnya | Pemilik memulai ulang backend (migration sudah diterapkan 23-10-2026). Bagian frontend sudah dikerjakan di [`FIX-EPS-039-03-FE`](../frontend/FIX-EPS-039-03-FE.md); uji layar R1–R5 di laporan itu sekaligus menjadi verifikasi runtime task ini |
