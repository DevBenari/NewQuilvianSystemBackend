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
| Status | ✅ Selesai berdasarkan validasi statis source dan skema; `dotnet build` dan eksekusi migration **NOT RUN** atas instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Registry/prefix | `Phm` `ACTIVE / LEGACY`; tabel `PhmPrescriptionItem` sudah ada |
| Keberlakuan | `NEW CODE` (enum, service); `TOUCHED LEGACY` (`PrescriptionController` masih memakai `ApplicationDbContext` langsung) |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-CFG-002`, `QBE-PAGE-001` |
| Persetujuan pemilik tabel | Muhammad Hamzah selaku pemilik `PharmacyManagement` (`RWI-DEC-062`, `RWI-DEC-150`) |
| Database | Migration `20260916004000_AddPrescriptionItemStopAndDoseKind` dibuat, **tidak dijalankan** |

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
| Database | R4: lima kolom nullable/berbawaan pada `PhmPrescriptionItem`; migration dibuat, **tidak dijalankan** |
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
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik |
| Verifikasi skema pada Postgres sekali pakai | Tidak dijalankan | `NOT RUN` | Migration belum dieksekusi |
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
| `dotnet build`, migration | Belum diverifikasi runtime | `NOT RUN` atas instruksi pemilik |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga sumber menyebut jalur berbeda: kartu roadmap `GET /prescriptions/daily?encounterId`, arsitektur `GET /episodes/{episodeId}/daily`, api-contract 12.6 `GET /episodes/{episodeId}` + `period`. Diimplementasikan mengikuti **api-contract 0.6.0** dan menerima kosakata periode kartu roadmap maupun kontrak. Kartu roadmap menyebut bawaan `Today`; bawaan dibiarkan "seluruh resep" demi kompatibilitas pemanggil lama — delta dicatat |
| Masalah yang diketahui | Aksi penghentian butir (`PATCH /items/{itemId}/stop`) milik `BE-RWI-100`, tertahan `BE-RWI-114` [BE-KEP] |
| Risiko tersisa | Kolom baru belum ada di basis data sampai migration dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (enum, service, migration); berkas lain `M` |
| Langkah berikutnya | `dotnet build`; jalankan migration R4 di Postgres sekali pakai; uji keempat periode |
