# Laporan Perubahan Backend — `BE-OPT-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-005` |
| Judul | API data referensi |
| Slice | `MVP-0` — `EPIC OPT-11` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-005` |
| Trace | `FR-OPT-110`, `FR-OPT-111`; `OPT-DEC-009`, `031`, `040`, `041`; `OPT-VAL-070`..`073`; `UAT-24`, `UAT-25` |
| Contract version | `OPT-API-v1` grup Opt Reference Data, `OPT-VAL-v1` bagian 6, `OPT-PERM-v1` — `approved` sisi Optik |
| Dependency | `BE-OPT-002` ✅, `BE-OPT-004` ✅ |
| Klasifikasi | `MEDIUM` — satu controller (5 endpoint), satu service, DTO, dan penolong bersama |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-AUD-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Admin Optik belum bisa mencatat atribut barang Optik (warna, ukuran frame, indeks lensa) maupun mengubah
angka bawaan unit Optik. Petugas juga belum punya katalog barang Optik yang menampilkan stok depo Optik.

---

## 2. Proses bisnis

1. **Admin Optik mendaftarkan barang.** Barangnya sudah ada di master barang (Master Data). Admin cukup mengisi profil Optiknya. **Contoh:** barang `FRM-A-HTM-52` diberi kategori Frame, warna Hitam, ukuran `52-18-140`. Barang yang tidak ada di master ditolak "Barang tidak ditemukan di master barang.".
2. **Petugas mencari di katalog.** **Contoh:** cari kategori Frame, warna "hitam", ukuran "52" → muncul `FRM-A-HTM-52` dengan stok tersedia 17. Stok 17 berasal dari 20 di depo Optik dikurangi 3 yang ditahan; batch kedaluwarsa tidak dihitung. Bila depo Optik belum didaftarkan Farmasi, kolom stok kosong.
3. **Admin Optik mengubah pengaturan.** **Contoh (`UAT-24`):** pengingat 30 → 21 hari, sehingga riwayat mencatat `PickupReminderDays: 30 → 21` dan versi naik 0 → 1. Dokumen yang sudah dibuat tetap memakai nilai lamanya.

**Jalur tidak normal:**
- Batas tutup 20 hari dengan pengingat 30 (`UAT-25`) → ditolak "Batas tutup harus lebih lama dari batas pengingat.".
- Dua admin mengubah bersamaan → yang kedua ditolak `409` "Pengaturan sudah diubah petugas lain. Muat ulang halaman.".
- Pengaturan belum ada → `404`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`McuReferenceDataController.cs`, `McuHttp.cs`, `McuOperationResult.cs`, `McuReferenceDataService.cs` (pola); `DrugStockService.GetAvailableQuantitiesAsync` (aturan stok siap-pakai); `Responses/PagedResult.cs`; `Constants/AccessTypes.cs`; `Services/Security/PermissionRegistryValidator.cs`; `Tests/QuilvianSystemBackend.PatientManagementTests/MembershipPermissionRegistryTests.cs` (pola uji registri).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptReferenceDataController.cs` | **Baru.** Lima endpoint (bagian 4). Tanpa `<summary>` XML pada action; metadata paling atas |
| `Areas/HealthServices/OpticalManagement/Controllers/OptHttp.cs` | **Baru.** Pemetaan hasil ke `200/400/403/404/409/503` dan logger teks-pesan (pola `McuHttp`) |
| `Areas/HealthServices/OpticalManagement/Services/OptReferenceDataService.cs` | **Baru.** Metadata, katalog + stok depo Optik, upsert profil, baca/ubah pengaturan + `OptSettingRevision`, validasi `OPT-VAL-070..073`, concurrency `Version` |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Baru.** `OptResult<T>` + `OptErrorCodes` |
| `Areas/HealthServices/OpticalManagement/Services/OptLabels.cs` | **Baru.** Label Indonesia 13 enum Optik |
| `Areas/HealthServices/OpticalManagement/DTOs/OptReferenceDataDtos.cs` | **Baru.** DTO metadata, katalog, profil, pengaturan |
| `Program.cs` | **Diperbarui.** `AddScoped<OptReferenceDataService>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be005ReferenceDataTests.cs`, `OpticalPermissionRegistryTests.cs` | **Baru** (diabaikan Git). 16 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint `OPT-API-v1` tersedia. **Delta:** `GET settings` menerima `serviceUnitId` opsional (bawaan unit Optik tertua), dan respons pengaturan memuat 20 riwayat terakhir. Keduanya penambahan, bukan perubahan kontrak |
| Database | Tanpa migration. Menulis `OptItemProfile`, `OptSetting`, `OptSettingRevision` |
| Keamanan/Auth | `[AccessPermission("OptReferenceData", "Read" / "Update" / "UpdateSetting")]`; lolos `PermissionRegistryValidator`. Kalimat log tanpa data sensitif |

**Delta terhadap kontrak (`api-contract.md` konvensi):** profil barang tidak memakai `ExpectedVersion` karena
`OptItemProfile` memang tidak punya kolom `Version` di kamus data. Upsert profil bersifat "simpanan terakhir yang berlaku".

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Reference Data

Base URL: `api/v1/health-services/optical-management/reference-data`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/metadata` | Daftar pilihan berlabel Indonesia (13 kelompok enum) | `OptReferenceData : Read` |
| `GET` | `/item-catalog` | Katalog barang Optik: penyaring kategori, warna, ukuran, indeks, kata kunci; stok depo Optik | `OptReferenceData : Read` |
| `PUT` | `/item-profiles/{drugId}` | Buat/ubah profil Optik untuk satu barang | `OptReferenceData : Update` |
| `GET` | `/settings` | Pengaturan unit Optik + riwayat perubahan | `OptReferenceData : Read` |
| `PUT` | `/settings` | Ubah pengaturan (berlaku ke depan) | `OptReferenceData : UpdateSetting` |

Kode status: `200` berhasil; `400` isian tidak sah (`OPT-VAL-070..073`, kategori kosong); `404` barang atau pengaturan tidak ditemukan; `409` pengaturan sudah diubah petugas lain.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 33 lulus, 0 gagal (16 baru) | `PASS` | Keluaran `dotnet test` |
| Metadata: 7 status pesanan berlabel, tanpa "Tidak diketahui" | Lulus | `PASS` | Uji metadata |
| `UAT-24` pengingat 30 → 21 | Riwayat `PickupReminderDays: 30 → 21`, versi 1 | `PASS` | Uji |
| Tanpa perubahan → tanpa riwayat, versi tetap | Lulus | `PASS` | Uji |
| Uang muka 50 → 0 tersimpan 0 | Lulus | `PASS` | Uji |
| `OPT-VAL-070..073` (termasuk `UAT-25`) | Kode dan kalimat kontrak; tanpa riwayat | `PASS` | Uji teori 4 kasus |
| Versi basi | `409`, data tidak berubah | `PASS` | Uji |
| Profil barang tidak ada → `404`; kategori kosong → `400`; buat lalu ubah tanpa baris ganda | Lulus | `PASS` | Uji |
| Katalog: saring warna/ukuran/merk/indeks; stok 20 − 3 ditahan = 17; batch kedaluwarsa tidak dihitung; barang tanpa stok = 0; tanpa depo = kosong | Lulus | `PASS` | Uji katalog |
| Controller memetakan `200/400/409/404` | Lulus | `PASS` | Uji controller |
| `PermissionRegistryValidator` atas 5 endpoint Optik | Nol temuan; 5 endpoint terlihat | `PASS` | Uji registri |
| QBE checker atas 7 berkas + kontrol multi-baris | Kontrol terkena; 7 berkas bersih | `PASS` | Keluaran checker; probe dihapus |

**Tidak dijalankan:** panggilan HTTP nyata ke backend yang menyala, karena butuh tabel Optik di devYoga (migration belum diterapkan) dan wewenang database tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-VAL-070`..`073` | Terpenuhi | Uji teori |
| (2) Ubah pengaturan tidak mengubah dokumen lama dan menulis riwayat lama→baru (`UAT-24`, `25`) | Terpenuhi | Uji; dokumen membekukan nilainya sendiri (`BE-OPT-002`) |
| (3) Profil hanya untuk barang yang ada | Terpenuhi | Uji `404` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Validasi `OPT-VAL-071` juga menolak batas pengingat di luar 1–365 hari dengan kalimat "Batas pengingat harus 1–365 hari." (rentang dari kamus data); kontrak hanya menulis kalimat untuk batas tutup |
| Masalah yang diketahui | Masa garansi hanya ditolak bila negatif; kontrak tidak menetapkan batas atas |
| Risiko tersisa | Stok kosong di katalog sampai Farmasi mendaftarkan depo Optik dan barangnya (`OPT-APR-005`). Barang Optik di master wajib `IsPrescribable = false` (`FR-OPT-110`); itu urusan data Master Data, tidak ditegakkan endpoint ini |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`; `?? Areas/HealthServices/OpticalManagement/` bertambah `Controllers/`, `DTOs/`, dan tiga berkas `Services/` |
| Langkah berikutnya | `BE-OPT-010` (kunjungan Optik). Sejajar dengan itu `BE-OPT-003` (rujukan di Registrasi) dan `BE-OPT-030` (perluasan Billing) |
