# Laporan Perubahan Backend — `BE-RWI-102`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-102` |
| Judul | Template dan versi protokol sliding scale (migration R6) |
| Slice | Gelombang 1 — `DOK-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-102` |
| Trace | `FR-DOK-094`, `FR-DOK-095`; `RWI-DEC-136`, `RWI-DEC-146`, `RWI-DEC-147`, `RWI-DEC-155`; `RWI-OQ-097`; `VAL-DOK-54`, `54a`, `54b`, `54c`, `54d`, `55`, `55e`; state matrix 0.6.0 bagian 8.6; api-contract 12.10; kamus data 13.4–13.8 |
| Contract version | `0.6.0` |
| Dependency | — |
| Klasifikasi | `HEAVY` — lima tabel baru (R6 memuat tabel order untuk `BE-RWI-103`), validasi keselamatan rentang dosis insulin |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, `Areas/HealthServices/ClinicalManagement/Enums`, `Repositories`, `Migrations`, `Program.cs`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai berdasarkan validasi statis source dan skema; `dotnet build` dan eksekusi migration **NOT RUN** atas instruksi pemilik. **`RWI-OQ-097` tetap terbuka** |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Registry/prefix | `Phm` `ACTIVE`; lima entity `PhmSlidingScale*` memakai prefix pemilik (bukan `Mst`, arsitektur 11.5.4 mengikuti preseden `LabValueBound`) |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-001/002`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-ENUM-001` |
| Hak akses baru | Resource `SlidingScaleTemplate` : `Read`, `Update`, **`Approve`** |
| Persetujuan pemilik | Muhammad Hamzah selaku pemilik `PharmacyManagement` (`RWI-DEC-147`); kewenangan pemakaian `RWI-DEC-155` |
| Database | Migration `20260916006000_AddSlidingScale` dibuat, **tidak dijalankan** |

## 1. Masalah yang diperbaiki

Sliding scale insulin sebelumnya hanya catatan bebas (`RWI-DEC-145`). Satu salah ketik atau rentang
yang bertumpuk berarti dosis insulin yang salah. Task ini membangun lapis pertama `RWI-DEC-146`:
protokol standar berversi yang disahkan orang lain dari pengubahnya.

## 2. Proses bisnis

1. Pengubah konfigurasi (hak `Update`) membuat template "SSI-DEWASA" lalu versi `Draft` beserta satuan
   gula darah (mg/dL atau mmol/L, wajib) dan rentang, misalnya `[… , 150)` 0 unit, `[150, 200)` 2 unit,
   `[200, 250)` 4 unit, `[250, 300)` 6 unit, `[300, …)` 8 unit.
2. Setiap penyimpanan divalidasi `SlidingScaleRangeValidator`: tepat satu rentang terbuka ke bawah,
   tepat satu terbuka ke atas, setiap batas atas sama persis dengan batas bawah rentang berikutnya, dosis
   tidak negatif. `LastModifiedByUserId` dicatat.
3. Pengguna **lain** yang memegang hak `Approve` mengesahkan → versi `Approved`, hash SHA-256 definisi
   tersimpan; versi `Approved` sebelumnya menjadi `Retired` pada **transaksi yang sama**. Index unik
   parsial basis data menolak dua versi `Approved` untuk satu template.
4. **Jalur tidak normal:**
   - Rentang 200–260 dan 250–299 → `400` "Rentang 200–260 dan 250–299 bertumpuk. Setiap nilai gula darah
     harus jatuh ke tepat satu rentang."
   - Versi dimulai dari 150 tanpa rentang di bawahnya → `400` "Rentang belum menutup seluruh nilai gula
     darah. Tambahkan rentang untuk nilai di bawah 150."
   - Rentang 150–200 lalu 210–250 → `400` "… Tambahkan rentang untuk nilai 200 sampai 210."
   - Pengubah terakhir mencoba mengesahkan → `403` "Versi ini terakhir diubah oleh Anda. Pengesahan harus
     dilakukan pengguna lain."
   - Mengubah versi `Approved`/`Retired` → `409` "Versi yang sudah disahkan tidak dapat diubah. Buat versi
     baru."
   - Belum ada versi sah → daftar menandai `HasApprovedVersion = false`, pesan "Belum ada protokol
     sliding scale yang disahkan." (`200`, bukan galat).
5. Versi `Draft` tidak dapat dipakai membuat order: `SlidingScaleOrderService.CreateAsync` menolak `409`
   (`VAL-DOK-55`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 13.4–13.8, 13.13; arsitektur 0.5 bagian 11.4–11.9; state matrix 8.6; validation matrix
10.6; permission-audit-matrix 6.1, 7, 8.10, 9; registry prefix; `NumberSeriesAllocator.cs`
(untuk R6 bagian order); pola configuration `BbkBloodUnitAllocationConfiguration` (index bernama) dan
`AccJournalLineConfiguration` (check constraint).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/BloodGlucoseUnit.cs` | Baru — `MgPerDl = 1`, `MmolPerL = 2` (dirancang `keperawatan`, dibuat lebih dulu karena kolom `GlucoseUnit` membutuhkannya) |
| `Areas/HealthServices/PharmacyManagement/Enums/SlidingScaleVersionStatus.cs`, `SlidingScaleOrderStatus.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Models/PhmSlidingScaleTemplate.cs`, `PhmSlidingScaleTemplateVersion.cs`, `PhmSlidingScaleRange.cs`, `PhmSlidingScaleOrder.cs`, `PhmSlidingScaleOrderVersion.cs` | Baru — lima entity R6 |
| `Repositories/Configurations/HealthServices/PharmacyManagement/SlidingScaleConfigurations.cs` | Baru — lima configuration; check constraint `CK_PhmSlidingScaleRange_SingleOwner` dan `CK_PhmSlidingScaleRange_DoseUnits`; unique parsial satu `Approved` per template |
| `Repositories/ApplicationDbContext.cs` | Lima `DbSet` |
| `Migrations/20260916006000_AddSlidingScale.cs` | Baru — R6; `Down` ditolak bila sudah ada order |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Lima blok entity dan lima blok relasi |
| `Areas/HealthServices/PharmacyManagement/DTOs/SlidingScaleDtos.cs` | Baru — DTO template, versi, rentang, order |
| `Areas/HealthServices/PharmacyManagement/Services/SlidingScaleRangeValidator.cs` | Baru — validator rentang, pembentuk baris, hash definisi, pembanding kesetaraan |
| `Areas/HealthServices/PharmacyManagement/Services/SlidingScaleTemplateService.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Controllers/SlidingScaleTemplateController.cs` | Baru |
| `Program.cs` | Registrasi `SlidingScaleTemplateService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru Sliding Scale Template (6 endpoint); tambahan murni |
| Database | R6: lima tabel baru kosong; migration dibuat, **tidak dijalankan** |
| Keamanan/Auth | `Approve` terpisah dari `Update`; aturan pengesah ≠ pengubah terakhir dari data versi, bukan nama jabatan; pasangan atribut hak akses cocok huruf demi huruf |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Sliding Scale Template

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar template beserta versi sah yang berlaku | `SlidingScaleTemplate : Read` |
| `GET` | `/{id}` | Template beserta seluruh versinya | `SlidingScaleTemplate : Read` |
| `POST` | `/` | Membuat template tanpa versi | `SlidingScaleTemplate : Update` |
| `POST` | `/{id}/versions` | Membuat versi `Draft` beserta rentang | `SlidingScaleTemplate : Update` |
| `PUT` | `/versions/{versionId}` | Mengubah versi `Draft`; pengubah terakhir dicatat | `SlidingScaleTemplate : Update` |
| `POST` | `/versions/{versionId}/approve` | Mengesahkan; versi sah lama menjadi `Retired` pada transaksi yang sama | `SlidingScaleTemplate : Approve` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kesesuaian lima tabel dengan kamus data 13.4–13.8 dan DDL 13.13 | Kolom, tipe, bawaan, index, check constraint sama | `PASS` | Model, configuration, migration, snapshot |
| Uji meja validator dengan kedua contoh penolakan kartu roadmap (200–260 & 250–299; mulai dari 150) | Menghasilkan penolakan bertumpuk dan berlubang sesuai pesan | `PASS` | Penelusuran logika `SlidingScaleRangeValidator.Validate` |
| Nama constraint ≤ 63 karakter mengikuti pemotongan EF Core | Sesuai | `PASS` | Perhitungan nama sesi ini |
| Snapshot hanya bertambah | Nol baris terhapus | `PASS` | `git diff --diff-algorithm=histogram` |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik |
| Verifikasi skema pada Postgres sekali pakai | Tidak dijalankan | `NOT RUN` | Migration belum dieksekusi |
| Verifikasi kontrak API dan proses bisnis runtime | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tabel template dan versi dengan status `Draft`, `Approved`, `Retired` | Terpenuhi (statis) | `PhmSlidingScaleTemplate`, `PhmSlidingScaleTemplateVersion`, enum, migration R6 |
| 2. Rentang tidak bertumpuk dan tidak berlubang | Terpenuhi (statis) | `SlidingScaleRangeValidator.Validate`, dipanggil saat buat, ubah, dan sahkan |
| 3. Hanya satu versi `Approved` per template | Terpenuhi (statis) | Unique parsial `IX_PhmSlidingScaleTemplateVersion_Template_Approved` + pemensiunan di service |
| 4. Pengesah bukan pengubah terakhir | Terpenuhi (statis) | `ApproveVersionAsync` → `403` VAL-DOK-54c |
| 5. Pengesahan memensiunkan versi sah sebelumnya pada transaksi yang sama | Terpenuhi (statis) | Satu transaksi; versi lama disimpan `Retired` lebih dulu, lalu versi baru `Approved`, lalu commit |
| 6. Versi `Draft` tidak dapat dipakai membuat order | Terpenuhi (statis) | `SlidingScaleOrderService.CreateAsync` → `409` VAL-DOK-55 |
| `RWI-OQ-097` dicatat masih terbuka | Terpenuhi | Metadata, bagian 7, roadmap |
| `dotnet build`, migration | Belum diverifikasi runtime | `NOT RUN` atas instruksi pemilik |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **`RWI-OQ-097` masih terbuka**: nama pengesah isi protokol belum ditunjuk manajemen rumah sakit. Mesin dapat dibangun dan diuji, tetapi di produksi nol versi boleh dinaikkan `Approved` sampai hak `Approve` diberikan kepada orang yang ditunjuk |
| Masalah yang diketahui | Nomor versi berikutnya dihitung dari versi terakhir template yang sama; dua draft bersamaan dijaga index unik `(TemplateId, VersionNumber)` dan dijawab `409`, bukan dialokasikan provider number-series (nomor versi bukan kode bisnis lintas modul) |
| Risiko tersisa | Angka klinis protokol tidak diisi blueprint; data awal menunggu pemilik klinis (arsitektur 11.9) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (14); berkas lain `M` |
| Langkah berikutnya | `dotnet build`; jalankan migration R6; uji kedua contoh penolakan dan pengesahan oleh akun berbeda |
