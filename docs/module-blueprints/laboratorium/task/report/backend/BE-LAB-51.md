# Laporan Perubahan Backend — `BE-LAB-51`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-51` (backend) |
| Judul | Laporan Patologi Anatomi dan konteks klinis — gelombang `MVP-6b2`, slice `S4c` |
| Trace | `FR-13.10`, `FR-13.12`; `LAB-DEC-085`, `LAB-DEC-088`, `LAB-DEC-091`..`LAB-DEC-094`; `INV-32`, `INV-35`, `INV-36`, `INV-38`, `INV-40` |
| Kontrak | `02-backend-architecture.md` bagian 15.5-15.7 dan 15.10; `erd/data-dictionary.md` bagian 15.5-15.7. **Nol endpoint**, sehingga `LAB-API-v1` nol tersentuh |
| Klasifikasi | `MEDIUM` — tiga tabel baru, nol menyentuh tabel berisi data |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-18 |
| Status | **✅ `SELESAI`** — build hijau 0 error; migration **diterapkan**; `AC-132`..`AC-135` terbukti pada database sungguhan, **dua di antaranya dibuktikan terbalik** |

---

## 1. Gerbang yang dilewati

| Gerbang | Hasil |
| --- | --- |
| Blueprint canonical | ✅ `LAB-BP-001` revision 68 |
| Task `SIAP DIKERJAKAN` | ✅ dinaikkan sesudah `BE-LAB-50` selesai |
| Requirement/decision ID | ✅ `FR-13.10`, `FR-13.12`, `LAB-DEC-085`/`088`/`091`..`094` |
| Kontrak | ✅ arsitektur bagian 15.5-15.7 dan kamus data bagian 15.5-15.7, keduanya turunan `r25` yang sudah `approved` |
| Dependency | ✅ `BE-LAB-50` selesai **dan migration-nya terterap** — bukan hanya source-nya ada |
| Acceptance criteria | ✅ `AC-132`..`AC-135` |
| Manifest revision/hash | ✅ keempat `input_hashes` masih cocok persis |
| Source SHA | ✅ nol bergeser sejak `BE-LAB-50` beberapa jam sebelumnya |

---

## 2. Yang dibangun

### 2.1 Tiga model, dan alasan pemisahannya

| Model | Isi | Yang tidak terlihat dari namanya |
| --- | --- | --- |
| `LabPathologyReport` | `LabOrderId` (unik), `FindingStatus?`, `AnalystUserId?`, `FinalizedAt?`, `FinalizedByUserId?`, `ReopenCount` | **Per PESANAN, bukan per pemeriksaan.** Patolog menulis satu narasi untuk seluruh bahan yang datang bersama — makroskopik menggambarkan jaringannya, bukan salah satu pemeriksaan atas jaringan itu |
| `LabPathologyReportValue` | `LabPathologyReportId`, `LabPathologyParameterId`, `ParameterNameSnapshot` (200), `Value` (`text`) | **Baris, bukan kolom** — itulah yang membuat golongan berikutnya cukup menambah data induk, bukan migration |
| `LabPathologyOrderContext` | `LabOrderId` (unik), `InitialDiagnosis?`, `RelevantHistory?`, `LastMenstrualPeriod?` (`date`), `ClinicalNote?` | **Tabel tersendiri karena PENULISNYA orang lain** — dokter pemesan, bukan patolog |

### 2.2 Satu enum

`LabPathologyFindingStatus` — `Normal = 1`, `NeedsAttention = 2`, `Critical = 3`. **Nilai temuan,
bukan status lifecycle**, dan penggolongannya manual: narasi diagnostik nol punya batas atas dan
bawah untuk dibandingkan, berbeda dari Patologi Klinik yang menilai kritis terhadap
`LabValueBound`.

### 2.3 Tiga hal yang paling menentukan pada task ini

**Pertama, nol kolom status lifecycle** (`INV-36`). Tidak ada `Draft`, `Final`, maupun
`Released`. Selesai-tidaknya dibaca dari `FinalizedAt` — fakta yang tercatat, bukan janji tentang
apa berikutnya; pola yang sama sudah dipakai `S4a` lewat `ResultEnteredAt != null`.

Sebabnya bukan kerapian: bila `Final` diperlakukan sebagai **rilis**, maka `LAB-DEC-003` yang
menuntut empat mata pada rilis bertabrakan dengan `LAB-DEC-090` pada orang yang sama. **Final
berarti patolog selesai menulis, bukan hasilnya boleh keluar.** Rilis adalah `S4e`, dan ia masih
tertahan `DEC-LAB-011`.

**Kedua, nol kolom `IssuedAt` dan `EffectiveAt`** (`INV-38`, `LAB-DEC-092`). Artifact memintanya
sebagai isian manual; keduanya ditolak karena sistemnya **sudah** mencatat kejadiannya —
diturunkan dari `FinalizedAt` dan `LabSpecimen.CollectedAt`. Menyimpannya berarti dua sumber
kebenaran untuk satu kejadian, dan yang satu pasti menyimpang tanpa ada yang tahu.

**Ketiga, `AnalystUserId` dan `FinalizedByUserId` nullable TANPA foreign key.** Mengikuti
`ResultEnteredByUserId` dan peringatan `REG-ACTOR-FK`. Ketiadaan baris konfigurasi relasi pada
`LabPathologyReportConfiguration` adalah **keputusan yang ditulis sebagai komentar**, bukan
kelalaian — tanpa catatan itu, pemelihara berikutnya akan menambahkannya sebagai "perbaikan".

---

## 3. Verifikasi

| # | Pemeriksaan | Hasil | Bukti |
| ---: | --- | --- | --- |
| 1 | `dotnet build -p:RunAnalyzers=False` | ✅ **0 error** | **Nol warning berasal dari berkas BE-LAB-51** — penelusuran `LabPathologyReport`, `LabPathologyOrderContext`, dan `LaboratoryEnums` pada keluaran kompilasi penuh menghasilkan nol baris |
| 2 | Migration hanya memuat tiga tabel baru | ✅ | 3 `CreateTable`; **0** `AddColumn` / `AlterColumn` / `DropColumn` |
| 3 | **`AC-132`** ketiga index unik parsial | ✅ **3/3** | `pg_index.indpred` = `("IsDelete" = false)` pada ketiganya |
| 4 | **`AC-133`** nol status hasil, nol `IssuedAt`/`EffectiveAt` | ✅ **dibuktikan dua arah** | Kolom `LabPathologyReport` berjumlah **tujuh** dan seluruhnya dikenali; pencarian terbalik `IssuedAt`/`EffectiveAt`/`ReportStatus`/`Status` menghasilkan **0 baris** |
| 5 | **`AC-134`** nol `ALTER TABLE` pada tabel berisi data | ✅ **dibuktikan terbalik** | `LabExamination` nol punya kolom `Pathology%` (**0 baris**); `LabOrder` nol punya kolom konteks klinis (**0 baris**) |
| 6 | **`AC-135`** kolom pelaku nullable tanpa FK | ✅ **dibuktikan terbalik** | `LabPathologyReport` punya **tepat satu** FK — `LabOrderId`; pencarian FK yang menyentuh `AnalystUserId`/`FinalizedByUserId` menghasilkan **0 baris** |
| 7 | Tipe kolom sesuai rancangan | ✅ | `Value` = `text`, `ParameterNameSnapshot` = `varchar(200)`, `LastMenstrualPeriod` = `date`, `ReopenCount` default `0` |
| 8 | Ketiga tabel kosong | ✅ **0 / 0 / 0** | Benar dan disengaja — task ini **nol endpoint**; pengisinya `BE-LAB-52` |

### 3.1 Keluaran pemeriksaan database — read-only

```text
===== 1. KETIGA TABEL BERDIRI =====
LabPathologyOrderContext / LabPathologyReport / LabPathologyReportValue   (3 baris)

===== 2. AC-132 — INDEX UNIK WAJIB PARSIAL =====
IX_LabPathologyOrderContext_LabOrderId          | ("IsDelete" = false)
IX_LabPathologyReportValue_ReportId_ParameterId | ("IsDelete" = false)
IX_LabPathologyReport_LabOrderId                | ("IsDelete" = false)
(3 baris)

===== 3. AC-133 — KOLOM LabPathologyReport =====
Id                | uuid                     | NO  | NULL
LabOrderId        | uuid                     | NO  | NULL
FindingStatus     | integer                  | YES | NULL
AnalystUserId     | uuid                     | YES | NULL
FinalizedAt       | timestamp with time zone | YES | NULL
FinalizedByUserId | uuid                     | YES | NULL
ReopenCount       | integer                  | NO  | 0
(7 baris)

===== 3b. AC-133 TERBALIK: IssuedAt / EffectiveAt / status lifecycle =====
(0 baris)

===== 4. AC-135 — FOREIGN KEY pada LabPathologyReport =====
FK_LabPathologyReport_LabOrder_LabOrderId | LabOrderId
(1 baris)

===== 4b. AC-135 TERBALIK: FK menyentuh kolom pelaku =====
(0 baris)

===== 5. AC-134 TERBALIK — LabExamination kolom Pathology* =====
(0 baris)

===== 5b. AC-134 TERBALIK — LabOrder kolom konteks klinis =====
(0 baris)

===== 6. TIPE KOLOM =====
LabPathologyOrderContext | InitialDiagnosis      | text              | NULL
LabPathologyOrderContext | LastMenstrualPeriod   | date              | NULL
LabPathologyReportValue  | ParameterNameSnapshot | character varying | 200
LabPathologyReportValue  | Value                 | text              | NULL

===== 7. KETIGA TABEL MASIH KOSONG =====
LabPathologyReport | 0 ; LabPathologyReportValue | 0 ; LabPathologyOrderContext | 0
```

> **Empat dari delapan pemeriksaan berbentuk KETIADAAN**, dan itu memang yang diminta: `AC-133`
> dan `AC-134` menuntut pembuktian terbalik. Pemeriksaan yang hanya menegaskan apa yang **ada**
> nol akan menangkap kolom yang menyelinap masuk.

### 3.2 Wewenang eksekusi database

Migration diterapkan terhadap dev pemilik modul (`QuilvianNewDevYoga`), melanjutkan wewenang yang
diberikan pemilik modul pada sesi yang sama untuk `BE-LAB-50`. Nol reset database, nol perubahan
credential, nol eksekusi di luar dev pemilik. `dotnet ef migrations list` sesudahnya menunjukkan
`AddLabPathologyMasterData` **dan** `AddLabPathologyReport` keduanya terterap.

---

## 4. Yang **tidak** dibangun

| Hal | Alasan |
| --- | --- |
| Enam endpoint laporan dan konteks klinis | `BE-LAB-52` — cakupan task ini **tanpa endpoint** |
| Service, DTO, validasi `VAL-92`..`VAL-102` | `BE-LAB-52` |
| Riwayat per kejadian `Reopen` beserta alasannya | Jejak audit, dibangun bersama jalurnya di `BE-LAB-52`. `ReopenCount` di sini hanya rekapnya, dan itu ditulis eksplisit pada model |
| Validasi dan rilis laporan | `S4e`, tertahan `DEC-LAB-011` |
| Kolom gambar Patologi Anatomi | `DEC-LAB-016` |
| Ruas sumber HL7 | `LAB-COORD-012` |

---

## 5. Temuan yang dibawa keluar

### 5.1 `LabResultForm` nol punya nilai `AnatomicPathologyNarrative`

`02-backend-architecture.md` bagian 15.9 mencatat `LabResultForm` sebagai *"tidak berubah"* dan
menyebut nilai **`AnatomicPathologyNarrative = 4`** yang *"artinya dipertegas"*. Penelusuran
menemukan enum itu hanya memuat **`Numeric = 1`** dan **`Choice = 2`** — nilai 3 dan 4 nol ada.

Perluasannya milik bagian 14 (Mikrobiologi) yang diturunkan menjadi **`BE-LAB-45`**, dan task itu
**belum pernah dibangun**. Jadi ini bukan selisih baru, melainkan akibat urutan gelombang:
`MVP-6b` dikerjakan mendahului `MVP-6c`.

**Nol menahan `BE-LAB-51`** — ketiga tabel task ini tidak menyentuh `LabResultForm`, dan
`LabPathologyFindingStatus` adalah enum yang berbeda. Dicatat supaya bagian 15.9 tidak terbaca
sebagai jaminan bahwa nilainya sudah ada, dan supaya `BE-LAB-52` memeriksanya lebih dulu bila
jalurnya kelak perlu membedakan bentuk hasil.

### 5.2 `AC-135` separuhnya belum dapat diuji, dan itu sesuai urutan

`AC-135` berbunyi dua hal: kolomnya **nullable tanpa foreign key**, dan **penulisnya menulis
`null` bukan `Guid.Empty`**.

Bagian pertama terbukti penuh pada database. Bagian kedua **belum punya penulis** — task ini nol
endpoint dan nol service. Ia diwariskan ke `BE-LAB-52`, dan bahayanya sudah ditulis pada model:
tanpa foreign key, `Guid.Empty` **tidak akan ditolak database** melainkan tersimpan diam-diam
sebagai pelaku yang tidak pernah ada. Itu justru lebih berbahaya daripada `BE-EXT-05`, yang
setidaknya gagal terang-terangan.

### 5.3 Build repository sempat macet 70 menit

`dotnet build` dengan restore implisit menggantung: 70 menit berjalan, ~15.700 detik CPU, nol
keluaran. Dihentikan dan diulang dengan `--no-restore -m:1`, selesai **6,8 menit**; build
berikutnya 2,4 menit. Dicatat sebagai temuan lingkungan, bukan temuan kode — nol kaitan dengan
perubahan task ini, dan `BUILD-PERFORMANCE-BASELINE.md` perlu tahu.

---

## 6. Berkas

### 6.1 Baru — 8 berkas

| Jalur | Isi |
| --- | --- |
| `Areas/.../Models/LabPathologyReport.cs` | Laporan per pesanan |
| `Areas/.../Models/LabPathologyReportValue.cs` | Nilai per parameter |
| `Areas/.../Models/LabPathologyOrderContext.cs` | Konteks klinis pesanan |
| `Repositories/.../LabPathologyReportConfiguration.cs` | Unik parsial `LabOrderId`; index `FinalizedAt` |
| `Repositories/.../LabPathologyReportValueConfiguration.cs` | Unik parsial pasangan; `Value` bertipe `text` |
| `Repositories/.../LabPathologyOrderContextConfiguration.cs` | Unik parsial `LabOrderId`; `date` dan tiga `text` |
| `Migrations/20260918080248_AddLabPathologyReport.cs` (+ `.Designer.cs`) | Tiga tabel |

### 6.2 Diubah — 3 berkas

| Jalur | Perubahan |
| --- | --- |
| `Areas/.../Enums/LaboratoryEnums.cs` | `LabPathologyFindingStatus` ditambahkan di akhir; nol enum lama disentuh |
| `Repositories/ApplicationDbContext.cs` | Tiga `DbSet` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disegarkan EF |

**Perubahan tidak terkait di working tree dipertahankan.** Nol `git add`, nol commit, nol push.

---

## 7. Langkah berikutnya

| # | Langkah | Keadaan |
| ---: | --- | --- |
| 1 | `BE-LAB-52` — enam endpoint, delapan DTO, satu service, nol migration | **Siap dikerjakan** |
| 2 | Isi pemetaan jenis pemeriksaan lewat `GET /suggestions` lalu `POST` | Kepala instalasi + `DR-LAB-003` — penahan `FE-LAB-28` |
| 3 | Putuskan apakah `GET /{id}` diusulkan sebagai `r26` | Terbuka sejak `BE-LAB-50` |
| 4 | Periksa `LabResultForm` sebelum `BE-LAB-52` menyentuh bentuk hasil | Lihat bagian 5.1 |
