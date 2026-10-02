# Laporan Perubahan Backend — `BE-RWI-103`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-103` |
| Judul | Order sliding scale per pasien |
| Slice | Gelombang 2 — `DOK-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-103` |
| Trace | `FR-DOK-096`, `FR-DOK-097`, `FR-DOK-098`, `FR-DOK-099`; `RWI-DEC-146`; `RUL-DOK-03`; `VAL-DOK-55`, `55a`, `55b`, `55c`, `55d`; state matrix 0.6.0 bagian 8.4 dan 8.7; api-contract 12.11; `INT-DOK-18`; kamus data 13.7, 13.8 |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-102` ✅ |
| Klasifikasi | `HEAVY` — protokol dosis insulin per pasien berversi, penjaga benturan, nomor bisnis dari provider bersama |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, `Program.cs`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai; validasi statis source. **`dotnet build` PASS 17 September 2026**; tabel order ikut migration R6 yang sudah diterapkan (bagian 8); uji runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Registry/prefix | `Phm` `ACTIVE`; tabel order dibuat migration R6 (`BE-RWI-102`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` pada `PrescriptionItemController` (isian `DoseKind`) |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-001/002/003/004/006`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` |
| Hak akses baru | Resource `SlidingScaleOrder` : `Read`, `Create`, `Update` |
| Nomor bisnis | `OrderNumber` dari `NumberSeriesAllocator` deret `PHM_SLIDING_SCALE_ORDER`, awalan `SSO`, reset tahunan, 6 digit (contoh `SSO-2026-000001`); unique index di basis data |
| Database | Nol migration tambahan (tabel lahir di R6) |

## 1. Masalah yang diperbaiki

Protokol sah (`BE-RWI-102`) belum dapat diterapkan pada pasien. Order per pasien wajib **menyalin**
rentang versi sah supaya template yang berganti besok tidak diam-diam mengubah dosis pasien yang sedang
berjalan, dan setiap penyesuaian wajib beralasan serta tercatat sebagai versi.

## 2. Proses bisnis

1. dr. Ahmad menandai butir insulin pada draft resep Budi sebagai berdosis skala
   (`DoseKind = SlidingScale` lewat isian baru pada butir resep).
2. dr. Ahmad memesan "SSI-DEWASA" v2 → `POST /sliding-scale-orders`. Rentang v2 **disalin** menjadi
   rentang order versi 1; nomor `SSO-2026-000001`.
3. Ia memotong dosis tiap rentang menjadi separuh dengan alasan "pasien sensitif insulin" → versi order
   ditandai `IsAdjusted`; GDS 280 yang biasanya 6 unit kini 3 unit.
4. Besoknya v3 template disahkan → order Budi **tidak berubah**, karena rentangnya tersalin dan versi
   order tetap menunjuk v2.
5. dr. Rina menyesuaikan order pada versi 1 → versi 2. dr. Ahmad yang layarnya masih membaca versi 1
   mengirim penyesuaian `ExpectedVersionNumber = 1` → `409` "Protokol sudah diubah dokter lain. Muat ulang
   lalu periksa kembali." Dua penyesuaian bersamaan juga dijaga index unik `(OrderId, VersionNumber)`.
6. Menghentikan order (`PATCH /{id}/stop`, alasan wajib) → `Stopped`; penyesuaian berikutnya `409`.
   Pelaksanaan milik `keperawatan` membaca order `Active` saja. Penghentian lewat butir insulinnya
   dilakukan jalur penghentian butir resep (`BE-RWI-100`) yang memanggil
   `StopForPrescriptionItemAsync` dalam transaksinya.
7. **Jalur tidak normal lain:** versi template belum `Approved` → `409`; butir bukan berdosis skala,
   sudah dihentikan, resep bukan draft, atau butir sudah punya order → `409`; rentang diubah tanpa alasan
   → `400`; bukan dokter berpenugasan aktif → `403`; perawatan ditutup → `422`.
8. Tidak ada jalur baca hasil laboratorium (`RUL-DOK-03`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 13.1, 13.7, 13.8; state matrix 8.4, 8.7; validation matrix 10.6; api-contract 12.11;
`PrescriptionItemController.cs`, `PrescriptionItemDtos.cs`, `NumberSeriesAllocator.cs`,
`NumberAllocationRequest.cs`, `BbkBloodOrderService.cs` (pola alokator).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/SlidingScaleOrderService.cs` | Baru — pesan, sesuaikan, hentikan, `StopForPrescriptionItemAsync`, baca |
| `Areas/HealthServices/PharmacyManagement/Controllers/SlidingScaleOrderController.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/DTOs/SlidingScaleDtos.cs` | DTO order (bersama `BE-RWI-102`) |
| `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionItemDtos.cs` | `DoseKind` opsional pada buat/ubah butir |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionItemController.cs` | Menulis `DoseKind`; butir dengan order aktif tidak boleh diturunkan ke dosis tetap (`409`) |
| `Program.cs` | Registrasi `SlidingScaleOrderService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru Sliding Scale Order (5 endpoint); isian opsional `DoseKind` pada butir resep (tanpa isian = perilaku lama) |
| Database | Nol migration tambahan |
| Keamanan/Auth | Resource baru `SlidingScaleOrder`; kewenangan dokter dari penugasan aktif pada episode; `AdjustmentReason` dan `StopReason` sensitif, tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Sliding Scale Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Memesan protokol pada butir insulin draft resep; rentang disalin | `SlidingScaleOrder : Create` |
| `GET` | `/episodes/{episodeId}` | Order satu episode, saring `status` | `SlidingScaleOrder : Read` |
| `GET` | `/{id}` | Order beserta seluruh versi dan rentangnya | `SlidingScaleOrder : Read` |
| `POST` | `/{id}/versions` | Menyesuaikan → versi order baru; `ExpectedVersionNumber` wajib cocok | `SlidingScaleOrder : Update` |
| `PATCH` | `/{id}/stop` | Menghentikan order; alasan wajib | `SlidingScaleOrder : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–6 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` (perintah ringan pemilik: `-m:1`, tanpa shared compilation, tanpa analyzer) | Build ke-5 pada 17 September 2026: **0 error, 212 warning** | `PASS` | Bagian 8 |
| Verifikasi kontrak API dan proses bisnis runtime (termasuk pengesahan versi template baru) | Tidak dijalankan | `NOT RUN` | Menunggu build dan migration R6 |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Order hanya dari versi `Approved` | Terpenuhi (statis) | `CreateAsync` → `409` VAL-DOK-55 |
| 2. Rentang tersalin, bukan dirujuk | Terpenuhi (statis) | `SlidingScaleRangeValidator.BuildRows(..., orderVersionId)` membentuk baris rentang milik versi order |
| 3. Penyesuaian wajib beralasan dan membuat versi order baru | Terpenuhi (statis) | `CreateAsync` (`IsAdjusted` bila berbeda) dan `AdjustAsync` (versi N+1, alasan wajib) |
| 4. Versi template baru tidak mengubah order berjalan | Terpenuhi (statis) | Versi order menyimpan `TemplateVersionId` asal; penyesuaian mempertahankannya; pengesahan template tidak menyentuh tabel order |
| 5. Nomor versi order basi ditolak | Terpenuhi (statis) | `ExpectedVersionNumber` → `409` VAL-DOK-55c; index unik `(OrderId, VersionNumber)` |
| 6. Menghentikan order atau butir insulinnya menghentikan pelaksanaan berikutnya | Terpenuhi (statis) | `StopAsync` → `Stopped`; `StopForPrescriptionItemAsync` untuk jalur butir (dipanggil `BE-RWI-100`); order `Stopped` tidak dapat disesuaikan |
| `dotnet build` | Terpenuhi | `PASS` 17 September 2026 — bagian 8 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kontrak menyebut `Idempotency-Key` pada `POST /`; kamus data 13.7 tidak punya kolomnya. Kiriman ganda dijaga unique parsial `PrescriptionItemId` dan dijawab `409` — delta kontrak dicatat |
| Masalah yang diketahui | Jalur butir-insulin-dihentikan baru aktif setelah `BE-RWI-100` (⛔ menunggu `BE-RWI-114` [BE-KEP]) memanggil `StopForPrescriptionItemAsync`; `CheckFrequencyCode` masih usulan gate `G-22` |
| Risiko tersisa | Tanpa versi `Approved` (`RWI-OQ-097`) order tidak dapat dibuat di produksi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (service, controller); berkas lain `M` |
| Langkah berikutnya | Uji skenario v2→v3 dan kiriman versi basi |

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
