# Laporan Perubahan Backend — `BE-RWI-099`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-099` |
| Judul | Resep Harian dan lima kolom butir resep (migration R4) |
| Slice | Gelombang 1 — `DOK-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-099` |
| Trace | `FR-DOK-086`; `RWI-DEC-121`, `RWI-DEC-134`; kamus data 0.5 bagian 13.1; api-contract 0.6.0 bagian 12.6; arsitektur 0.5 bagian 11.5.1, 11.5.10, 11.5.11 |
| Contract version | `0.6.0` |
| Dependency | — |
| Klasifikasi | `MEDIUM` — migration lima kolom pada tabel `PharmacyManagement`, perluasan endpoint baca yang sudah ada |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, `Repositories`, `Migrations`, `Program.cs`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 16 September 2026 |
| Status | ✅ Selesai; validasi statis source dan skema. **`dotnet build` PASS dan migration R4 diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** (bagian 8); uji runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Registry/prefix | `Phm` `ACTIVE / LEGACY`; tabel `PhmPrescriptionItem` sudah ada |
| Keberlakuan | `NEW CODE` (enum, service); `TOUCHED LEGACY` (`PrescriptionController` masih memakai `ApplicationDbContext` langsung) |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-CFG-002`, `QBE-PAGE-001` |
| Persetujuan pemilik tabel | Muhammad Hamzah selaku pemilik `PharmacyManagement` (`RWI-DEC-062`, `RWI-DEC-150`) |
| Database | Migration `20260916004000_AddPrescriptionItemStopAndDoseKind` dibuat; **diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |

## 1. Masalah yang diperbaiki

Dokter yang visite pagi tidak punya satu daftar obat yang sedang berjalan: endpoint resep per episode
hanya mengembalikan kepala resep tanpa butir dan tanpa saring periode, dan butir resep belum punya
tempat mencatat penghentian maupun jenis dosis.

## 2. Proses bisnis

1. dr. Rina membuka Resep Harian Budi hari rawat ke-4 dan memilih "Minggu Ini" →
   `GET /prescriptions/episodes/{episodeId}?period=ThisWeek`.
2. Jendela dihitung pada **waktu rumah sakit (Asia/Jakarta)**: Senin 1 September 00.00 WIB sampai Senin
   8 September 00.00 WIB. Resep hari ke-1 (Ceftriaxone, Paracetamol) dan hari ke-3 (Omeprazole) tampil.
3. Setiap resep membawa **butir** — termasuk yang sudah dihentikan beserta `IsStopped`, `StoppedAt`,
   `StoppedByName`, `StopReason` — dan **racikan beserta bahannya**. Obat pulang ikut tampil dan terbedakan
   lewat `PrescriptionOrderType = Discharge`.
4. "Hari Ini" = 00.00–24.00 WIB hari berjalan; "Bulan Ini" = tanggal 1 sampai tanggal 1 bulan berikutnya;
   "Rentang" = `from` 00.00 sampai `to` + 1 hari 00.00 (keduanya termasuk).
5. **Jalur tidak normal:** `period=Range&from=2026-09-03` tanpa `to` → `422` "Periode Rentang wajib
   menyebut tanggal awal dan tanggal akhir."; `from` setelah `to` → `422`; nilai periode tidak dikenal →
   `400`.
6. Tanpa `period` dan tanpa tanggal, seluruh resep episode dikembalikan seperti sebelum task ini, sehingga
   pemanggil lama tidak berubah hasilnya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PhmPrescriptionItem.cs`, `PhmPrescription.cs`, `PhmPrescriptionCompound*.cs`,
`PhmPrescriptionItemConfiguration.cs`, `PrescriptionController.cs`, `PrescriptionDtos.cs`,
`ApplicationDbContextModelSnapshot.cs`, migration `20260916003000` (pola migration manual), kamus data
13.1 dan 13.13.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Enums/PrescriptionDoseKind.cs` | Baru — `Fixed = 0`, `SlidingScale = 1` |
| `Areas/HealthServices/PharmacyManagement/Models/PhmPrescriptionItem.cs` | Lima kolom `IsStopped`, `StoppedAt`, `StoppedByUserId`, `StopReason`, `DoseKind` + navigasi `StoppedByUser` |
| `Repositories/Configurations/HealthServices/PhmPrescriptionItemConfiguration.cs` | Bawaan, panjang, konversi enum, FK `Restrict`, index `(PrescriptionId, IsStopped)` dan `StoppedByUserId` |
| `Migrations/20260916004000_AddPrescriptionItemStopAndDoseKind.cs` | Baru — R4; `Down` ditolak bila sudah ada butir dihentikan atau berdosis skala |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Lima properti, dua index, satu relasi |
| `Areas/HealthServices/PharmacyManagement/Services/InpatientPrescriptionService.cs` | Baru — penyelesai periode zona waktu rumah sakit dan pembaca Resep Harian |
| `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs` | `InpatientPrescriptionListItem : PrescriptionResponse` + DTO butir, racikan, bahan |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` | `GET episodes/{episodeId}` menerima `period`, `from`, `to`; memakai service |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionItemController.cs`, `DTOs/PrescriptionItemDtos.cs` | Isian opsional `DoseKind` pada buat/ubah butir (dipakai `BE-RWI-103`) |
| `Program.cs` | Registrasi `InpatientPrescriptionService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /prescriptions/episodes/{episodeId}`: query baru `period`, `from`, `to`; response menjadi `PagedResult<InpatientPrescriptionListItem>` yang memuat seluruh field lama plus `Items` dan `Compounds` (tambahan murni) |
| Database | R4: lima kolom nullable/berbawaan pada `PhmPrescriptionItem`; migration dibuat; **diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |
| Keamanan/Auth | Nol hak akses baru; tetap `Prescription : Read` |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Prescription

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Resep Harian: saring `period` (`Today`/`today`, `ThisWeek`/`week`, `ThisMonth`/`month`, `Range`) atau `from`/`to`, `orderType`; butir termasuk yang dihentikan, racikan beserta bahan | `Prescription : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kesesuaian kolom dengan kamus data 13.1 dan DDL 13.13 | Nama, tipe, nullability, bawaan, index sama | `PASS` | Model, configuration, migration, snapshot |
| Snapshot hanya bertambah | `git diff --diff-algorithm=histogram` nol baris terhapus | `PASS` | Diff snapshot |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` (perintah ringan pemilik: `-m:1`, tanpa shared compilation, tanpa analyzer) | Build ke-5 pada 17 September 2026: **0 error, 212 warning** | `PASS` | Bagian 8 |
| Migration diterapkan ke `QuilvianNewDevHamzah` (database dev pribadi pemilik) | Tercatat di `__EFMigrationsHistory`, nol `Pending`; model = snapshot (`has-pending-model-changes` bersih) | `PASS` | Bagian 8 |
| Verifikasi kontrak API runtime | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Lima kolom baru sesuai kamus data | Terpenuhi (statis) | Model, configuration, migration R4, snapshot |
| 2. Saring Hari Ini, Minggu Ini, Bulan Ini, Rentang | Terpenuhi (statis) | `InpatientPrescriptionService.ResolvePeriod` |
| 3. Racikan dan obat pulang ikut tampil | Terpenuhi (statis) | `GetDailyPrescriptionsAsync` tanpa saring jenis kecuali diminta; `Compounds` beserta `Ingredients` |
| 4. `period=Range` tanpa `from`/`to` ditolak `422` | Terpenuhi (statis) | Cabang `Range` pada `ResolvePeriod` |
| 5. Migration mundur menghapus kolom selama belum berisi data pasien sungguhan | Terpenuhi (statis) | `Down` menghapus kelima kolom; ditolak bila ada jejak penghentian/dosis skala |
| `dotnet build`, migration | Terpenuhi | `PASS` 17 September 2026 — build 0 error; migration diterapkan ke `QuilvianNewDevHamzah` — bagian 8 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga sumber menyebut jalur berbeda: kartu roadmap `GET /prescriptions/daily?encounterId`, arsitektur `GET /episodes/{episodeId}/daily`, api-contract 12.6 `GET /episodes/{episodeId}` + `period`. Diimplementasikan mengikuti **api-contract 0.6.0** dan menerima kosakata periode kartu roadmap maupun kontrak. Kartu roadmap menyebut bawaan `Today`; bawaan dibiarkan "seluruh resep" demi kompatibilitas pemanggil lama — delta dicatat |
| Masalah yang diketahui | Aksi penghentian butir (`PATCH /items/{itemId}/stop`) milik `BE-RWI-100`, tertahan `BE-RWI-114` [BE-KEP] |
| Risiko tersisa | Kolom baru sudah ada di `QuilvianNewDevHamzah`; database lingkungan lain belum dimigrasi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (enum, service, migration); berkas lain `M` |
| Langkah berikutnya | Uji keempat periode |

## 8. Verifikasi susulan — 17 September 2026

Atas permintaan pemilik, build dan migration dijalankan setelah laporan ini ditulis. Perintah build
yang dipakai persis perintah pemilik:
`dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`.

| Langkah | Hasil | Tindakan |
| --- | --- | --- |
| Build ke-1 | Gagal dalam 12 detik — `CS1002` pada `ApplicationDbContextModelSnapshot.cs`: relasi `ConsultationId` milik `TrxPatientProcedure` kehilangan `;` (sisa suntingan `BE-RWI-097`) | `;` ditambahkan |
| Build ke-2 | Pemeriksaan tipe penuh: **1 error** — `CS1931` pada `CpptVerificationService.cs`: variabel query `episode` bentrok dengan variabel lokal `episode` (kode `BE-RWI-096`) | Variabel query diganti nama `episodeBerjalan`, logika tidak berubah |
| Build ke-3 | 0 error. `dotnet ef` menolak model: FK `SupersedesDecisionId` (merujuk tabelnya sendiri) dan FK `ReconciliationItemId` pada `PhmMedicationReconciliationDecision` bernama sama setelah dipotong 63 karakter — EF Core tidak memberi akhiran unik pada FK yang merujuk tabelnya sendiri. **Galat ini juga akan membuat aplikasi gagal saat `DbContext` pertama dipakai** | Nama constraint eksplisit `FK_PhmMedicationReconciliationDecision_SupersedesDecisionId` pada configuration, migration R5, dan snapshot |
| Build ke-4 | 0 error. Snapshot gagal dibaca EF: lima navigasi koleksi (`Decisions`, `Versions`, `Ranges`) ditulis di blok relasi, sebelum relasinya dideklarasikan | Dipindahkan ke bagian navigasi snapshot; diperiksa tanpa build dengan simulasi urutan navigasi (0 galat), perbandingan DDL snapshot vs model runtime di 632 tabel, dan SQL migration vs model runtime (0 temuan) |
| Build ke-5 | **0 error, 212 warning** (garis dasar 211) | — |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | — |
| `dotnet ef database update --no-build` ke `QuilvianNewDevHamzah` | `Done.` Delapan migration diterapkan: `20260916000000` s.d. `20260916007000` (termasuk R3, R7 milik task sebelumnya, lalu R4, R5, R6, R8) | `migrations list` sesudahnya: nol `Pending` |

**Yang masih `NOT RUN`:** uji kontrak API dan proses bisnis runtime, uji jalur mundur migration
(`Down`), serta regresi yang disyaratkan kartu roadmap. Database yang disentuh hanya
`QuilvianNewDevHamzah` milik pemilik; database tim, staging, dan production tidak disentuh.
