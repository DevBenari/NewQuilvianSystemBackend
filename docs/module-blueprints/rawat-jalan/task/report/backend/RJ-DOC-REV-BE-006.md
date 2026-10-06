# Laporan Perubahan Backend — `RJ-DOC-REV-BE-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-006` |
| Judul | Grouping ICD-10 berdasarkan ICD Diagnosa |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 2d |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.2` |
| Trace | `RJ-DOC-DEC-007`; temuan `A6`; CSV pemilik `icd10_202609281504.csv` dan `icd_diagnosa_202609281508.csv` diterima `1/10/2026` |
| Contract version | Penambahan field pada `GET /patient-diagnoses/master-options` (non-breaking) |
| Dependency | CSV pemilik — terpenuhi |
| Klasifikasi | `HEAVY` — tabel master baru, kolom FK baru, seeder startup, perubahan respons API |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/**`, `Areas/HealthServices/ClinicalManagement/**`, `Repositories/**`, `Seeders/**`, `SeedData/ICD10/**`, `Migrations/**`, `Program.cs`, `QuilvianSystemBackend.csproj` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001..005` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai — `97,5%` kode ICD-10 terpetakan; sisanya disebut di bagian 7 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` (entity), `ClinicalManagement` (endpoint pencarian) |
| Registry | `Master / Reference / MasterData` — prefix `Mst`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (`MstDiagnosisGroup`, seeder); `TOUCHED LEGACY` (`MstDiagnosis`, `PatientDiagnosisController`) |
| QBE yang berlaku | QBE-ENT-001, QBE-NAM-002, QBE-CFG-001, QBE-MOD-001, QBE-CODE-004 (`SourceCode` unik), QBE-DTO-001, QBE-API-001 |

---

## 1. Masalah yang diperbaiki

Pencarian ICD-10 pada SOAP hanya mengembalikan daftar kode datar. Pemilik meminta hasil
dikelompokkan menurut **ICD Diagnosa** — Daftar Tabulasi Dasar (DTD) yang dipakai rumah sakit,
contoh kelompok `001.0 Kolera` menaungi `A00`, `A00.0`, `A00.1`, `A00.9`.

---

## 2. Proses bisnis

1. Saat aplikasi start, seeder membaca dua CSV milik rumah sakit dari `SeedData/ICD10/`.
2. **Kelompok DTD**: dari `4199` baris `icd_diagnosa`, hanya `528` baris yang berisi rentang ICD-10
   (`no_terperinci`) yang merupakan kelompok diagnosa. `3671` baris lain berisi kode tindakan
   ICD-9-CM (contoh `00.01 Therapeutic ultrasound…`) dan **tidak** diimpor.
3. **Pemetaan kode ke kelompok**, berurutan:
   1. pemetaan eksplisit dari `icd10` CSV (kolom `icddid`);
   2. bila tidak ada, rentang kode kelompok. Contoh: `C91.3` masuk rentang `C 91 - C 95` →
      `087.0 Leukemia`. Bila lebih dari satu rentang memuatnya, yang **tersempit** menang.
4. Kode yang sudah punya kelompok tidak disentuh lagi pada start berikutnya, sehingga koreksi
   manual tidak tertimpa.
5. Dokter mengetik pada pencarian diagnosa. Respons membawa `diagnosisGroupId`,
   `diagnosisGroupDtdNumber`, `diagnosisGroupName`, sehingga layar dapat mengelompokkan hasil.
   Mengetik nama kelompok (mis. `kolera`) juga menemukan kode-kodenya. Filter
   `diagnosisGroupId` tersedia untuk menampilkan satu kelompok.

**Keandalan pemetaan rentang** diuji terhadap `1000` pemetaan eksplisit CSV: rentang memberi kelompok
yang sama untuk `995` kode (`99,5%`). Selisih `5` kode diselesaikan oleh pemetaan eksplisit yang selalu menang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstDiagnosis.cs`, `MstDiagnosisChapter.cs`, `MstDiagnosisConfiguration.cs`, `Icd10DiagnosisSeeder.cs`,
`PatientDiagnosisController.cs` (`master-options`), `PatientDiagnosisDtos.cs`, `Program.cs`
(blok seeder), `QuilvianSystemBackend.csproj`, kedua CSV pemilik.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstDiagnosisGroup.cs` (baru) | Master kelompok DTD |
| `Repositories/Configurations/HealthServices/MstDiagnosisGroupConfiguration.cs` (baru) | Tabel, index unik `SourceCode` |
| `Areas/HealthServices/MasterData/Models/MstDiagnosis.cs` | `DiagnosisGroupId` + navigasi |
| `Repositories/Configurations/HealthServices/MstDiagnosisConfiguration.cs` | FK `SetNull` + index |
| `Repositories/ApplicationDbContext.cs` | DbSet `MstDiagnosisGroups` |
| `Seeders/IcdDiagnosisGroupSeeder.cs` (baru) | Impor kelompok + pemetaan idempoten, pengurai rentang ICD-10 |
| `SeedData/ICD10/icd_diagnosa_dtd.csv`, `icd10_dtd_mapping.csv` (baru) | Salinan CSV pemilik |
| `QuilvianSystemBackend.csproj` | Kedua CSV disalin ke output/publish |
| `Program.cs` | Seeder dipanggil saat startup |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs` | `master-options`: filter `diagnosisGroupId`, cari nama kelompok, tiga field kelompok |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientDiagnosisDtos.cs` | Tiga field kelompok |
| `Migrations/20261001043557_AddMstDiagnosisGroup.cs` + `.Designer.cs`, snapshot | `CreateTable` + `AddColumn` + `4` index + `1` FK |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Penambahan field dan parameter opsional pada `master-options`. Tidak ada yang dihapus |
| Database | Tabel baru `MstDiagnosisGroup` (`528` baris), kolom baru `MstDiagnosis.DiagnosisGroupId` (`18079` terisi). Diterapkan ke `QuilvianNewDevSukma` |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses `PatientDiagnosis : Read` tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-diagnoses/master-options?search=&diagnosisGroupId=` | Pilihan ICD-10, kini membawa `diagnosisGroupId`, `diagnosisGroupDtdNumber`, `diagnosisGroupName`; pencarian juga mencocokkan nama kelompok | `PatientDiagnosis : Read` |

Endpoint master CRUD `MstDiagnosisGroup` (sembilan baseline) **tidak** dibuat: kelompok DTD adalah
data referensi resmi yang dikelola lewat impor CSV, bukan disunting di layar. Dicatat sebagai delta.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | :---: |
| `dotnet build … -o <scratchpad>/build` | `0 Error(s)`; CSV ikut tersalin ke output | `PASS` |
| `dotnet ef migrations add AddMstDiagnosisGroup` | `1` `CreateTable`, `1` `AddColumn`, `4` `CreateIndex`, `1` `AddForeignKey` | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` |
| `dotnet ef database update` ke `QuilvianNewDevSukma` | `Applying migration '20261001043557_AddMstDiagnosisGroup'. Done.` | `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `26` berkas, `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` |
| R1 Start pertama (seeder) | `19.017 ms`; `528` kelompok; `18079` dari `18543` kode ICD-10 terpetakan | `PASS` |
| R2 Start kedua (idempoten) | `621 ms`; jumlah tetap `528` / `18079` | `PASS` |
| R3 `master-options?search=kolera` | `A00`, `A00.0`, `A00.1`, `A00.9` — semuanya `001.0 Kolera` | `PASS` |
| R4 `search=I10`, `J18`, `leukemia` | `I10` → `145.0 Hipertensi esensial (primer)`; `J18*` → `169.0 Pneumonia`; `C91*` → `087.0 Leukemia` | `PASS` |
| R5 Pemetaan eksplisit menang atas rentang | `A18`, `A70`, `A92`, `B05.9` mengikuti CSV | `PASS` |
| `dotnet test` | — | `NOT RUN` — tidak ada project test |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Mengikuti CSV pemilik | Terpenuhi — kelompok dan pemetaan eksplisit dari CSV; pemetaan rentang hanya untuk kode yang tidak ada di CSV | R1–R5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **`icd10_202609281504.csv` hanya berisi `1000` baris (`A00` – `C16.3`)** — tampak terpotong batas ekspor. Kode lain dipetakan lewat rentang (akurasi teruji `99,5%`). Bila pemilik mengirim ulang ekspor lengkap, ganti `SeedData/ICD10/icd10_dtd_mapping.csv`; kode yang belum berkelompok akan dipetakan ulang otomatis, tetapi kode yang **sudah** berkelompok tidak berubah tanpa pengosongan manual |
| Masalah yang diketahui | `464` kode ICD-10 tanpa kelompok, terbanyak `Y` (98), `X` (77), `T` (63), `D` (60) — rentangnya tidak tercantum di sumber DTD. Beberapa pemetaan eksplisit CSV tampak janggal (mis. `A18` → *Meningitis tuberkulosa*, `B05.9` → *Rubela*); diikuti apa adanya karena milik sumber |
| Risiko tersisa | Start pertama pada database baru menambah ±`19` detik |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` MstDiagnosis.cs, MstDiagnosisConfiguration.cs, ApplicationDbContext.cs, PatientDiagnosisController.cs, PatientDiagnosisDtos.cs, Program.cs, csproj, snapshot; `??` model/konfigurasi/seeder baru, dua CSV, dua berkas migration, laporan |
| Langkah berikutnya | `RJ-DOC-REV-FE-003` mengelompokkan hasil pencarian diagnosa per kelompok DTD |
