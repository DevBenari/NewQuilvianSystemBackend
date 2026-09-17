# Laporan Perubahan Backend — `BE-RWI-107`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-107` |
| Judul | Instrumen dan formulir klinis berversi (migration K1) |
| Slice | Gelombang 2 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-107` |
| Trace | `FR-KEP-039`, `FR-KEP-041`, `FR-KEP-042`; `RWI-DEC-124`, `RWI-DEC-136`; `RWI-FACT-027`; kamus data 0.4 bagian 11.4–11.6; api-contract 0.5.0 bagian 7.2; `VAL-KEP-19a`–`c`; `INT-KEP-16` |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-106` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — tiga tabel baru, mesin definisi berversi, seeder, satu grup endpoint |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Repositories/`, `Migrations/`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — enam kriteria terpetakan ke source; migration K1 **ditulis, tidak dijalankan**; `dotnet build` dan seeder **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; tiga entity baru berprefix `Cli` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-ENT-001`, `QBE-NAM-001/002`, `QBE-CFG-001`, `QBE-MOD-001/002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-PAGE-001`, `QBE-DEL-001` |
| Hak akses baru | Resource `ClinicalInstrumentConfiguration` : `Read`, `Update`, `Approve` |
| Database | Migration `20260917100000_AddClinicalInstrument` (K1) — ditulis tangan beserta snapshot; **belum diterapkan** ke database mana pun |

## 1. Masalah yang diperbaiki

Instrumen penilaian klinis — risiko jatuh, nyeri, edukasi — punya angka batas dan pita skor yang ditentukan
komite keperawatan. V1 menghitungnya di browser dengan batas yang saling bertabrakan (`RWI-FACT-027`); V2
menanamnya di controller (`RWI-FACT-036`). Setiap perubahan kebijakan berarti perubahan kode, dan pengkajian
lama tidak dapat menjawab "dihitung dengan batas yang mana".

## 2. Proses bisnis

1. Admin konfigurasi klinis (Andi) membuat instrumen "Risiko Jatuh Dewasa" untuk usia `[216, 720)` bulan →
   `POST /clinical-instruments`. Rentang usia yang bertumpuk dengan instrumen aktif sejenis → `409` (`VAL-KEP-19c`).
2. Andi membuat versi konsep → `POST /{id}/versions`. Definisi berisi bagian, isian, pilihan bernilai,
   pita kategori, isian wajib, dan interval kajian ulang. Server menormalkan JSON-nya dan menyimpan SHA-256
   heksadesimal (`DefinitionHash`). Mengubah satu angka pita menghasilkan hash yang lain.
3. Validasi simpan (`VAL-KEP-19a`, `19b`) — contoh berangka:
   - Rendah `[0,25)`, Sedang `[25,51)`, Tinggi `[50,null)` → ditolak `400` "Rentang kategori tidak boleh bertumpuk
     atau berlubang. Periksa batas Sedang dan Tinggi."
   - Morse dengan skor tertinggi 125, pita terakhir `[45,100)` → skor 100–125 tanpa kategori → `400`.
   - Isian `single` tanpa pilihan, kode isian ganda, atau pengikatan ke kolom di luar daftar kamus data 11.1 → `400`.
4. Formulir memanggil `GET /patient-assessments/instruments/resolve?instrumentKind=FallRiskScale&episodeId=` →
   Budi 67 tahun (810 bulan) → instrumen dewasa. Tanpa versi sah, versi konsep terakhir dikembalikan dengan
   `IsApproved = false`.
5. **Versi konsep tidak dipakai pasien sungguhan** (`FR-KEP-042`): dokumen boleh disimpan sebagai konsep,
   tetapi hanya dapat diselesaikan dengan versi konsep bila `ClinicalConfiguration:AllowDraftVersionsForTesting`
   bernilai `true` — pengaturan yang wajib `false` di produksi (penegakannya `BE-RWI-109`/`111`).
6. **Seeder draft** (`ClinicalInstrumentDraftSeeder`) mengisi delapan instrumen/formulir berstatus `Draft`,
   hanya bila kodenya belum ada, dengan pengubah terakhir akun SuperAdmin. Batas yang bertabrakan **ditandai**
   pada `reviewFlags` definisi, tidak dibereskan (bagian 8).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 0.4 bagian 11.4–11.6 dan 11.17; arsitektur 0.4 bagian 11.4–11.10; api-contract 7.2; validation
matrix 6.1; state matrix 5.1; `RWI-FACT-027`, `RWI-DEC-124`, `RWI-DEC-136`; form V1 risiko jatuh pada
frontend `13c3a96b` (`git show`, hanya-baca); `SlidingScaleTemplateService.cs` dan konfigurasinya (pola versi
berpengesahan); `RadiologyMasterDataSeeder.cs` (pola seeder draft); `NumberSeriesAllocator.cs`; snapshot EF.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs`, `ClinicalInstrumentVersionStatus.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Models/CliClinicalInstrument.cs`, `CliClinicalInstrumentVersion.cs`, `CliAssessmentInstrumentResponse.cs` | Baru |
| `Repositories/Configurations/HealthServices/ClinicalManagement/ClinicalInstrumentConfigurations.cs` | Baru — mapping, index bernama, FK bernama, check constraint |
| `Repositories/Configurations/HealthServices/NursingRecordAuditColumns.cs` | Baru — kolom jejak bersama tabel keperawatan V2 |
| `Repositories/ApplicationDbContext.cs` | DbSet tabel baru |
| `Migrations/20260917100000_AddClinicalInstrument.cs` | Baru — K1, `Down` menolak bila jawaban pengkajian sudah ada |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Blok entity dan relasi tabel baru (hanya penambahan) |
| `Areas/HealthServices/ClinicalManagement/DTOs/ClinicalInstrumentDtos.cs` | Baru — bentuk definisi, permintaan, balasan |
| `Areas/HealthServices/ClinicalManagement/Services/ClinicalInstrumentDefinitionEngine.cs` | Baru — normalisasi, hash, validasi pita, hitung skor |
| `Areas/HealthServices/ClinicalManagement/Services/ClinicalInstrumentService.cs` | Baru — instrumen, versi, pengesahan, resolusi |
| `Areas/HealthServices/ClinicalManagement/Controllers/ClinicalInstrumentController.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Controllers/NursingControllerExtensions.cs` | Baru — pembantu balasan bersama |
| `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs` | Baru |
| `Program.cs` | Registrasi service dan pemanggilan seeder |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru Clinical Instrument (10 endpoint). **Delta:** `GET /resolve` dipasang di grup Patient Assessment (`instruments/resolve`) karena argumen pertama `[AccessPermission]` wajib sama dengan `ControllerName`; ditambah `GET /versions/{id}` dan `PUT /{id}` |
| Database | Tiga tabel baru (K1). FK `CaseManagementEvaluationId` dipasang K3. **Delta kamus data:** kolom `RetiredByUserId` dan `RetireReason` (alasan pemensiunan wajib, api-contract 7.2); kunci definisi `reviewFlags` |
| Keamanan/Auth | Resource baru `ClinicalInstrumentConfiguration`; `ResponsesJson` sensitif dan tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Clinical Instrument

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar instrumen beserta versi sah dan konsep terakhir | `ClinicalInstrumentConfiguration : Read` |
| `GET` | `/{id}` | Instrumen beserta seluruh versi, penanda tinjauan, hasil validasi | `ClinicalInstrumentConfiguration : Read` |
| `GET` | `/versions/{versionId}` | Satu versi | `ClinicalInstrumentConfiguration : Read` |
| `POST` | `/` | Membuat instrumen | `ClinicalInstrumentConfiguration : Update` |
| `PUT` | `/{id}` | Mengubah nama, rentang usia, keaktifan | `ClinicalInstrumentConfiguration : Update` |
| `POST` | `/{id}/versions` | Versi konsep baru, disalin dari versi terakhir bila tanpa definisi | `ClinicalInstrumentConfiguration : Update` |
| `PUT` | `/versions/{versionId}` | Mengubah definisi konsep; `ExpectedDefinitionHash` wajib cocok | `ClinicalInstrumentConfiguration : Update` |
| `PATCH` | `/versions/{versionId}/approve` | Mengesahkan (lihat `BE-RWI-108`) | `ClinicalInstrumentConfiguration : Approve` |
| `PATCH` | `/versions/{versionId}/retire` | Memensiunkan versi sah; alasan wajib | `ClinicalInstrumentConfiguration : Approve` |
| `POST` | `/versions/{versionId}/score-preview` | Uji hitung tanpa menyimpan | `ClinicalInstrumentConfiguration : Read` |

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/instruments/resolve` | Versi instrumen yang berlaku bagi pasien satu episode | `PatientAssessment : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–6 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Konsistensi migration–konfigurasi–snapshot | Dibangkitkan dari satu spesifikasi (skrip scratchpad sesi ini); diff snapshot hanya penambahan (2.146 baris, nol baris lama berubah) | `PASS` | Perbandingan `diff` snapshot sebelum/sesudah |
| Nama FK, index, dan check constraint ≤ 63 karakter | Seluruhnya bernama eksplisit dan diperiksa panjangnya | `PASS` | Assertion generator |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| `dotnet ef migrations has-pending-model-changes`, verifikasi skema Postgres sekali pakai | Tidak dijalankan | `NOT RUN` | Menunggu build pemilik |
| Keluaran seeder | Tidak dijalankan; daftar penanda disusun dari source seeder (bagian 8) | `NOT RUN` | `ClinicalInstrumentDraftSeeder.cs` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tabel instrumen, versi, jawaban ada di `ClinicalManagement` | Terpenuhi (source + migration) | `CliClinicalInstrument`, `CliClinicalInstrumentVersion`, `CliAssessmentInstrumentResponse`; migration K1 |
| 2. Definisi JSON beserta hash; hash berubah bila definisi berubah | Terpenuhi | `ClinicalInstrumentDefinitionEngine.Normalize` + `Hash` (SHA-256 atas JSON ternormalisasi); `UpdateVersionAsync` menghitung ulang |
| 3. Seeder membuat versi `Draft` tiga risiko jatuh V1, Kajian Umum, Edukasi, Perencanaan Pulang, checklist MPP | Terpenuhi (source) | `ClinicalInstrumentDraftSeeder.Baselines` — ditambah draft Monitoring Nyeri agar `BE-RWI-111` punya instrumen |
| 4. Batas bertabrakan ditandai, bukan dibereskan | Terpenuhi | `reviewFlags` per definisi; pengesahan ditolak selama penanda ada (`REVIEW_FLAGS_OPEN`); Morse sengaja tetap gagal validasi pita |
| 5. Pita tidak bertumpuk/berlubang; instrumen sejenis tidak bertumpuk usia | Terpenuhi | `ValidateBands` (menutup skor terendah–tertinggi); `FindOverlappingInstrumentAsync` → `409` |
| 6. Versi `Draft` hanya dipakai menyelesaikan dokumen dengan pengaturan uji aktif | Terpenuhi | `IsDraftAllowedInThisEnvironment` dibaca `NursingAssessmentDocumentService.EnsureCanCompleteAsync` dan `CaseManagementEvaluationService.CompleteAsync` |
| DoD: laporan memuat daftar batas bertabrakan | Terpenuhi | Bagian 8 |
| DoD: verifikasi skema Postgres sekali pakai dan `dotnet build` | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `appsettings` tidak diubah; tanpa kunci `ClinicalConfiguration:AllowDraftVersionsForTesting` nilainya `false` |
| Masalah yang diketahui | Butir risiko jatuh anak dan lansia kosong karena butir V1 dibaca dari master indikator backend V1 yang tidak ada di repository ini; butir Morse disusun dari skala Morse baku dan ditandai untuk diverifikasi. Isian wajib Kajian Umum kosong (`RWI-OQ-057`) |
| Risiko tersisa | Pengesah isi instrumen belum ditunjuk (`RWI-OQ-056`) — tanpa itu seluruh dokumen V2 tidak dapat diselesaikan di produksi. Migration ditulis tangan: `dotnet ef migrations has-pending-model-changes` wajib bersih sebelum `database update` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??`; `M` pada `ApplicationDbContext.cs`, snapshot, `Program.cs` |
| Langkah berikutnya | Build pemilik; `has-pending-model-changes`; `BE-RWI-108` pengesahan |

## 8. Penanda tinjauan yang ditulis seeder

| Kode instrumen | Penanda |
| --- | --- |
| `FALL_RISK_CHILD` | V1 bertabrakan: komentar kode baris 219–220 "Sedang 25–50 (overlap 45–50)" vs kode baris 223–224 dan layar baris 491–493 "Sedang 25–44"; draft mengikuti kode dan layar. Nama instrumen anak tidak ada di V1. Butir V1 tidak tersedia. Batas usia usulan seeder |
| `FALL_RISK_ADULT` | V1 bertabrakan: kode baris 215–216 (Sedang 25–49, Tinggi ≥ 50) vs layar baris 501–502 ("25-50 Sedang", "≥50 Tinggi") — skor 50 dua kategori; draft menyalin layar dan **gagal validasi pita**. Butir dari skala Morse baku. Butir alat bantu jalan bertumpang dengan Ketergantungan Kajian Umum (`INV-KEP-04`). Batas usia usulan seeder |
| `FALL_RISK_ELDERLY` | V1 berlubang: layar baris 483–484 hanya 0–5 Rendah dan 6–16 Sedang tanpa Tinggi; kode baris 219 memetakan seluruh ≥ 6 ke Sedang. Butir V1 tidak tersedia. Batas usia usulan seeder |
| `GENERAL_NURSING_ASSESSMENT` | Isian Pernapasan, Integritas Kulit, Eliminasi, Ketergantungan belum dipetakan dari label V1 (`RLN3-CAP-05`); isian wajib kosong (`RWI-OQ-057`) |
| `PAIN_MONITORING` | Interval kajian ulang belum ditetapkan (gate `G-04`); skala per kelompok usia belum ditetapkan |
| `EDUCATION_ASSESSMENT` | Isian usulan PRD 31 (gate `G-01`); isian V1 `RLN3-CAP-08` belum dipetakan |
| `DISCHARGE_PLANNING` | "Status Rencana" informatif (gate `G-10`); butir kelompok belum ditetapkan |
| `CASE_MANAGEMENT_CHECKLIST` | Butir checklist master V1 belum dipetakan (gate `G-07`) |
