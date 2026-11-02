# Laporan Perubahan Backend — `BE-MCU-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-006` |
| Judul | API data referensi |
| Slice | `MVP-0` — katalog `EPIC MCU-01` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-006`* |
| Trace | `FR-MCU-004`; `MCU-DEC-007`, `MCU-DEC-041` |
| Contract version | `MCU-API-v1` grup Mcu Reference Data; `MCU-VAL-v1` `VAL-MCU-09`; `MCU-PERM-v1` (`McuReferenceData : Read/Create/Update`); [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) `McuExaminationType`, `McuExaminationField`, `McuFitnessCategory` — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-002` ✅, `BE-MCU-005` ✅ (konstanta izin) |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), kontrak API baru (2), persistence yang sudah ada (1), keamanan berkaitan (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `MedicalCheckupManagement/{Controllers,DTOs,Services}/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — ketiga acceptance criteria terbukti di harness |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` — satu baris registrasi |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (satu resource `McuReferenceData` = satu controller), `QBE-LOG-001`, `QBE-VAL-001` (`VAL-MCU-09` + isian dasar), `QBE-TXN-001` (ubah isian dua tahap dalam satu transaksi), `QBE-DTO-001`, `QBE-PAGE-001` (daftar jenis), `QBE-DEL-001` (jenis/kategori dinonaktifkan; isian dihapus lunak beserta pelaku), `QBE-ENT-003` (urutan = `FieldNumber`, bukan `SortOrder`) |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Formulir *Pemeriksaan MCU* (fisik, tanda vital, visus, EKG, …) dan daftar kategori kelayakan resume hanya
dapat diisi lewat seeder usulan (`BE-MCU-004`). Admin dan dokter MCU belum dapat menyusun atau
meratifikasinya sendiri.

**Contoh.** Dokter MCU meminta formulir *Visus dan Buta Warna*: butir 1 *Visus kanan*, butir 2 *Visus
kiri*, butir 3 *Buta warna* berpilihan *Normal | Parsial | Total*. Admin yang lupa mengisi daftar
pilihan butir 3 ditolak *"Isian pilihan wajib berisi daftar pilihan."* Sesudah dibenahi, formulir tampil
urut butir 1–3 walaupun dikirim acak. Kategori *Unfit sementara* yang tidak dipakai lagi cukup
dinonaktifkan — resume lama yang memakainya tetap terbaca.

---

## 2. Proses bisnis

1. Admin MCU membuka **Data Referensi** dan membuat jenis Pemeriksaan MCU beserta isiannya (nomor butir,
   kode, label, jenis isian Teks/Angka/Pilihan/Ya-Tidak, satuan, daftar pilihan, wajib).
2. Sistem memeriksa isian lalu menyimpan jenis aktif; isian disajikan urut nomor butir.
3. Admin dapat mengubah formulir: mengganti label, memindahkan nomor butir, menambah, atau membuang isian.
   Isian yang dibuang ditandai terhapus; hasil pemeriksaan lama tetap terbaca.
4. Ratifikasi master usulan (`MCU-OQ-002`/`003`) dilakukan di sini: dokter MCU mengubah keterangan yang
   bertanda *USULAN*.
5. Admin membuat, mengubah, atau menonaktifkan kategori kelayakan. Kategori tidak pernah dihapus.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Isian Pilihan tanpa daftar pilihan (kosong atau hanya pemisah) | `400` `VAL-MCU-09` |
| Nomor butir atau kode isian ganda, nomor butir < 1 | `400` |
| Kode jenis atau kode kategori sudah dipakai (tanpa peduli huruf besar/kecil) | `400` |
| Jenis pemeriksaan tanpa satu isian pun | `400` |
| Isian yang sudah dipakai hasil pemeriksaan diganti jenisnya | `409` — nilai *Normal* yang tersimpan tidak bermakna pada isian Angka |
| Menukar nomor butir 1 ↔ 2 | Berhasil; isian tetap baris yang sama (disimpan dua tahap) |
| Dua admin menyimpan kode/nomor yang sama serentak | Index unik menolak yang kedua → `400` *muat ulang lalu coba lagi* |
| Jenis/kategori tidak ada | `404` |
| Tanpa hak akses `McuReferenceData` | `403` dari `AccessPermissionFilter` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/api-contract.md` (grup Mcu Reference Data), `permission-audit-matrix.md`,
`validation-matrix.md`, `erd/data-dictionary.md` bagian 1, `02-backend-architecture.md` §5.1/§5.5/§5.6,
model dan configuration `McuExaminationType`/`Field`/`FitnessCategory`, `McuExaminationResultValue.cs`
(rujukan isian), `McuPackageController.cs`/`McuPackageService.cs`/`McuHttp.cs` (pola `BE-MCU-005`),
`ApplicationDbContext.cs`, `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/DTOs/McuReferenceDataDtos.cs` (baru) | Query, request, response jenis pemeriksaan, isian, dan kategori kelayakan |
| `MedicalCheckupManagement/Services/McuReferenceDataService.cs` (baru) | Daftar/detail/buat/ubah jenis + isian; daftar/buat/ubah kategori; validasi; simpan dua tahap |
| `MedicalCheckupManagement/Controllers/McuReferenceDataController.cs` (baru) | 7 endpoint di bawah satu resource `McuReferenceData`; tanpa `<summary>` di action |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Tambah kode `VAL-MCU-09` dan `MCU-FIELD-IN-USE` |
| `Program.cs` | `AddScoped<McuReferenceDataService>()` |
| `Tests/QuilvianSystemBackend.McuTests/McuReferenceDataServiceTests.cs` (baru) | 13 uji service |
| `Tests/QuilvianSystemBackend.McuTests/McuReferenceDataControllerContractTests.cs` (baru) | 3 uji kontrak |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 7 endpoint baru sesuai `MCU-API-v1`; tidak ada endpoint lama yang berubah |
| Database | Tanpa migration; memakai tabel `BE-MCU-002`. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Resource baru `McuReferenceData` (`Read`/`Create`/`Update`) di modul MCU; pemberiannya lewat Role Access. Tidak ada kolom sensitif |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Validasi selain `VAL-MCU-09` | Matriks hanya `VAL-MCU-09` | Isian wajib, panjang, kode tanpa spasi, nomor butir ≥ 1, nomor/kode tidak ganda, kode unik — `400` `MCU-INVALID-REQUEST` | Kebutuhan dasar `QBE-VAL-001`; index unik kamus data |
| Jenis tanpa isian | Tidak disebut | Ditolak `400` | Formulir tanpa isian tidak dapat diisi dokter |
| Ganti jenis isian yang sudah dipakai hasil | Tidak disebut | `409` `MCU-FIELD-IN-USE` | Nilai lama menjadi tak bermakna; kode `409` sesuai arti umum kontrak |
| `IsActive` pada ubah | Request belum dirinci | `UpdateMcuExaminationTypeRequest.IsActive`, `UpdateMcuFitnessCategoryRequest.IsActive` | Kriteria (3) — nonaktif tanpa hapus |
| Respons detail | `McuExaminationTypeDetailResponse` | Ditambah `Options` (daftar pilihan terpecah) dan `FieldTypeLabel` | Siap dipakai layar tanpa memecah ulang |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Reference Data

Base URL: `api/v1/health-services/medical-checkup-management` (dua sumber daya)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/examination-types` | Daftar jenis Pemeriksaan MCU | `McuReferenceData : Read` | `?search&isActive&pageNumber&pageSize` | `PagedResult<McuExaminationTypeResponse>` | Tersedia |
| `GET` | `/examination-types/{id}` | Detail jenis + isian urut nomor butir | `McuReferenceData : Read` | - | `McuExaminationTypeDetailResponse` | Tersedia |
| `POST` | `/examination-types` | Buat jenis + isian | `McuReferenceData : Create` | `CreateMcuExaminationTypeRequest` | `McuExaminationTypeDetailResponse` | Tersedia |
| `PUT` | `/examination-types/{id}` | Ubah jenis + isian, termasuk aktif/nonaktif | `McuReferenceData : Update` | `UpdateMcuExaminationTypeRequest` | `McuExaminationTypeDetailResponse` | Tersedia |
| `GET` | `/fitness-categories` | Daftar kategori kelayakan | `McuReferenceData : Read` | `?isActive` | `List<McuFitnessCategoryResponse>` | Tersedia |
| `POST` | `/fitness-categories` | Buat kategori | `McuReferenceData : Create` | `CreateMcuFitnessCategoryRequest` | `McuFitnessCategoryResponse` | Tersedia |
| `PUT` | `/fitness-categories/{id}` | Ubah kategori, termasuk aktif/nonaktif | `McuReferenceData : Update` | `UpdateMcuFitnessCategoryRequest` | `McuFitnessCategoryResponse` | Tersedia |

Kode status: `200`, `400` (`VAL-MCU-09` / `MCU-INVALID-REQUEST`), `403`, `404`, `409` (`MCU-FIELD-IN-USE`).
Aksi tulis dicatat logger dengan kalimat berisi kode jenis/kategori dan pelaku; `GET` tidak dicatat.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests -p:RunAnalyzers=False` (ikut membangun proyek utama) | 79 lulus (63 lama + 16 baru), 0 error kompilasi | `PASS` | `Passed! - Failed: 0, Passed: 79` |
| Kriteria (1) — Pilihan tanpa daftar (`null`, kosong, `"  |  | "`) | `400` `VAL-MCU-09`, pesan persis; 0 jenis tersimpan. Juga pada ubah | `PASS` | `Kriteria_1_…` (3 kasus), `Ubah_isian_menjadi_pilihan_…` |
| Kriteria (2) — urut nomor butir | Dikirim 3,1,2 → tampil 1,2,3; kode jadi huruf besar; pilihan dirapikan `Normal|Parsial|Total`; isian non-Pilihan tanpa daftar pilihan | `PASS` | `Kriteria_2_…` |
| Kriteria (3) — kategori nonaktif | `IsActive = false`, `IsDelete = false`, pelaku tercatat; hilang dari `isActive=true`, tetap di daftar penuh | `PASS` | `Kriteria_3_…` |
| Tukar nomor butir | Id isian tetap; tidak tersisa nomor negatif atau kode sementara | `PASS` | `Menukar_nomor_butir_…` |
| Buang/tambah isian | Isian dibuang ditandai terhapus dengan pelaku; isian baru ditambahkan | `PASS` | `Isian_yang_dibuang_…` |
| Ganti jenis isian yang dipakai hasil | `409`; jenis tetap Pilihan; isian lain tetap boleh berganti | `PASS` | `Isian_yang_sudah_dipakai_…` |
| Ganda, nomor < 1, kode dipakai, tidak ditemukan, nonaktif jenis | Sesuai §2 | `PASS` | Uji sisanya |
| Grup Swagger, 7 endpoint, string hak akses | Persis kontrak dan matriks | `PASS` | `McuReferenceDataControllerContractTests` |
| `PermissionRegistryValidator` | Nol temuan untuk `McuReferenceData`; 4 + 3 rute terdaftar | `PASS` | `Lolos_PermissionRegistryValidator` |
| Tanpa `<summary>` action | 0 baris `///` di tingkat action | `PASS` | Pencarian berkas |
| Checker QBE `-Path` (5 berkas + kontrol) | Berkas task nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Tukar nomor butir pada PostgreSQL | Harness SQLite melonggarkan index unik berfilter, sehingga manfaat simpan dua tahap tidak terbukti di sini | `NOT RUN` | Butuh basis data PostgreSQL dengan migration MCU |
| `403` lewat HTTP; Swagger berjalan | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU (`BE-MCU-004` §7) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Isian `FieldType = Option` tanpa daftar pilihan → `400` *"Isian pilihan wajib berisi daftar pilihan."* | Terpenuhi | §5 |
| (2) Isian ber-`FieldNumber` tampil urut nomor butir | Terpenuhi | §5 |
| (3) Kategori kelayakan dapat dinonaktifkan tanpa menghapus | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Startup backend ke devYoga masih menunggu migration `BE-MCU-002`/`003` (`BE-MCU-004` §7) |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Simpan dua tahap belum dibuktikan pada PostgreSQL sungguhan. Isi master tetap usulan sampai `MCU-OQ-002`/`003` diratifikasi |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` (satu `using` kurang di uji kontrak diperbaiki sebelum validasi) |
| Status Git | Perubahan task: ` M Program.cs`; `??` tiga berkas baru di folder modul MCU (yang sudah `??`); perubahan `McuOperationResult.cs` (berkas `BE-MCU-005`, belum di-commit); laporan ini; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`) |
| Langkah berikutnya | `BE-MCU-007` (jadwal, kuota, pelampauan, dokter jaga), `BE-MCU-010`, `BE-MCU-020` siap. `FE-MCU-004` dapat memakai grup ini |
