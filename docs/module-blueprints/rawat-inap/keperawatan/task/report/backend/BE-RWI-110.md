# Laporan Perubahan Backend — `BE-RWI-110`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-110` |
| Judul | Kajian Umum delapan bagian (migration K3) |
| Slice | Gelombang 3 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-110` |
| Trace | `FR-KEP-045`, `FR-KEP-046`, `FR-KEP-049`; `RWI-DEC-131`, `RWI-DEC-141`; `INV-KEP-04`; `VAL-KEP-21d`, `VAL-KEP-22c`; kamus data 11.1–11.2; api-contract 7.1, 7.4 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-107` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — kolom baru pada dua tabel legacy berisi data, perubahan perilaku tanda vital |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Repositories/`, `Migrations/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — enam kriteria terpetakan; `dotnet build` lolos; migration K3 **diterapkan** ke `QuilvianNewDevHamzah` dan kolom terverifikasi dari katalog; kontrak API runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; tabel legacy `TrxPatientAssessment`, `TrxPatientVitalSign` (bukan rename) |
| Keberlakuan | `TOUCHED LEGACY` (kolom tabel `Trx*`) + `NEW CODE` (`InpatientVitalSignService`) |
| QBE relevan | `QBE-CFG-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Database | Migration `20260917101000_AddAssessmentV2AndCaseManagementEvaluation` (K3, bersama `BE-RWI-113`); **diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |

## 1. Masalah yang diperbaiki

Kajian Umum V2 menyalin angka tanda vital ke kolomnya sendiri. Bila perawat mengoreksi tanda vital 08.00
(TD 130/80 → 140/85), kajian tetap menyimpan 130/80 — dua sumber kebenaran untuk satu fakta klinis. Tanda vital
rawat inap juga menerima isian nyeri, sehingga nyeri tercatat di dua tempat berbeda.

## 2. Proses bisnis

1. Kajian Umum (`Initial`/`Reassessment`) digambar dari definisi formulir delapan bagian `RWI-DEC-141`
   (draft seeder `GENERAL_NURSING_ASSESSMENT`): Sumber Data Pasien, Kondisi Umum, Pernapasan, Integritas Kulit,
   Skrining Nutrisi, Eliminasi (kateter), Ketergantungan (kursi roda/tongkat/walker), Status Fungsional.
   Isian yang punya kolom — keluhan, kesadaran, oksigen, gizi, fungsional, psikososial — diikat ke kolomnya;
   sisanya disimpan sebagai jawaban JSON.
2. Kondisi Umum memilih tanda vital → `VitalSignId`. Server menolak `400` bila baris itu bukan milik pasien dan
   episode yang sama atau berstatus dibatalkan/salah catat (`VAL-KEP-21d`).
3. Detail kajian membawa `ReferencedVitalSign` yang dibaca **langsung** dari baris tanda vital: koreksi 140/85
   langsung terbaca dari kajian.
4. `POST /patient-vital-signs` pada kunjungan berepisode rawat inap berjalan: `InpEpisodeId` diisi server,
   `VitalSignSource = InpatientObservation`; isian `HasPain`, `PainScale`, `PainLocation`, `PainNote` → `400`
   "Nyeri dicatat pada Monitoring Nyeri, bukan pada tanda vital." Berlaku juga pada `PUT`. Poliklinik dan IGD
   tidak berubah.
5. Migration mundur K3 menolak bila kolom baru sudah berisi data pasien.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`TrxPatientAssessment.cs`, `TrxPatientVitalSign.cs` beserta konfigurasi dan blok snapshot-nya,
`PatientVitalSignController.cs`, `InpatientClinicalContextService.FindOpenEpisodeIdAsync`, kamus data 11.1–11.2,
frontend architecture 10.4.3.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/PainAssessmentState.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | `PainAssessmentState`, `PainReassessmentDueAt`, `VitalSignId` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs` | `InpEpisodeId` |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Kolom, FK `SetNull`, dua index bernama |
| `Repositories/Configurations/HealthServices/TrxPatientVitalSignConfiguration.cs` | Kolom, FK `Restrict`, index `(InpEpisodeId, ObservationDateTime)` |
| `Migrations/20260917101000_AddAssessmentV2AndCaseManagementEvaluation.cs` | Baru — K3; `Down` menolak bila kolom baru sudah berisi data |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Properti, index, relasi baru |
| `Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentDocumentService.cs` | Penerapan dan validasi `VitalSignId`; pembaca rujukan tanda vital |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientVitalSignService.cs` | Baru — episode rawat inap dan penolakan isian nyeri |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` | Pengisian `InpEpisodeId`, penolakan nyeri pada buat/ubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`, `DTOs/PatientAssessmentDtos.cs` | `VitalSignId`, `PainAssessmentState` pada request/response; `ReferencedVitalSign` pada detail; metadata `PainAssessmentStates` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif pada payload pengkajian. **Perubahan perilaku** `POST`/`PUT /patient-vital-signs` rawat inap (`400` isian nyeri). Delta: `Idempotency-Key` tanda vital tidak dipasang karena tabel tidak punya kolomnya |
| Database | Tiga kolom `TrxPatientAssessment`, satu kolom `TrxPatientVitalSign` (nullable/berbawaan 0) — tanpa mematikan layanan |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` / `PUT` | `/`, `/{id}` | Menerima `VitalSignId` untuk Kajian Umum | `PatientAssessment : Create` / `Update` |
| `GET` | `/{id}` | Detail beserta `ReferencedVitalSign` | `PatientAssessment : Read` |

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Rawat inap: `InpEpisodeId` diisi server, isian nyeri ditolak | `PatientVitalSign : Create` |
| `PUT` | `/{id}` | Rawat inap: isian nyeri ditolak | `PatientVitalSign : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–6 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Konsistensi migration–konfigurasi–snapshot kolom baru | Nama FK dan index eksplisit sama pada ketiganya | `PASS` | Skrip generator sesi ini |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi kontrak API runtime | Tidak dijalankan (skema sudah terverifikasi, lihat lampiran) | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tiga kolom `TrxPatientAssessment`, satu `TrxPatientVitalSign` | Terpenuhi | Model, konfigurasi, migration K3, snapshot |
| 2. Kajian Umum delapan bagian sesuai 10.4.3 | Terpenuhi (definisi draft) | `ClinicalInstrumentDraftSeeder` baseline `GENERAL_NURSING_ASSESSMENT` |
| 3. Kajian Umum menyimpan `VitalSignId`, bukan nilai | Terpenuhi | `ApplyDraftAsync` mengisi `VitalSignId`; tidak menyalin kolom tanda vital |
| 4. Koreksi tanda vital ikut terbaca dari kajian | Terpenuhi | `GetVitalSignReferenceAsync` membaca baris sumber saat detail dibuka |
| 5. Input tanda vital rawat inap tidak menerima isian nyeri | Terpenuhi | `InpatientVitalSignService.HasPainFields` pada `CreateVitalSign` dan `UpdateVitalSign` → `400` |
| 6. Migration mundur menghapus kolom selama tabel belum berisi data pasien | Terpenuhi | `Down` K3 menolak bila ada baris berisi kolom baru |
| DoD: `dotnet build`, verifikasi skema | `dotnet build` lolos; skema terverifikasi dari katalog | `PASS` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `VitalSignId` hanya diterapkan pada Kajian Umum; jenis dokumen lain mengosongkannya |
| Masalah yang diketahui | Isian bagian Pernapasan, Integritas Kulit, Eliminasi, Ketergantungan masih berupa catatan sampai label V1 dipetakan (penanda seeder) |
| Risiko tersisa | Layar tanda vital rawat inap yang masih mengirim isian nyeri akan menerima `400` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` model, konfigurasi, controller; berkas baru `??` |
| Langkah berikutnya | `BE-RWI-111`, `BE-RWI-121` |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |

Migration task ini: `20260917101000_AddAssessmentV2AndCaseManagementEvaluation` (K3) — diterapkan ke `QuilvianNewDevHamzah` 17 September 2026.

