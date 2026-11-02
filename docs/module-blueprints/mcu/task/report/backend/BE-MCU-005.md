# Laporan Perubahan Backend — `BE-MCU-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-005` |
| Judul | API paket MCU |
| Slice | `MVP-0` — katalog `EPIC MCU-01` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-005`* |
| Trace | `FR-MCU-002`, `FR-MCU-003`; `MCU-DEC-007`, `015`, `020`, `025`, `041` |
| Contract version | `MCU-API-v1` grup Mcu Package; `MCU-VAL-v1` `VAL-MCU-01`..`08`; `MCU-PERM-v1` (`McuPackage : Read/Create/Update`); [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) bagian 1 — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-002` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika sedang (1), kontrak API baru (2), persistence yang sudah ada (1), keamanan berkaitan (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `Areas/HealthServices/MedicalCheckupManagement/{Constants,Controllers,DTOs,Services}/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser sejak `BE-MCU-003` |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-01`..`03` dan seluruh kriteria terbukti di harness. Swagger berjalan **belum** dibuka (lihat §7) |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` (2026-10-21) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` — satu baris registrasi |
| QBE yang berlaku | `QBE-SVC-001` (controller hanya memanggil `McuPackageService`), `QBE-API-001` (`ApiResponse<T>`, `PagedResult<T>`, kode status kontrak), `QBE-PERM-001` (`[AccessController]`/`[AccessAction]`/`[AccessPermission]` dari konstanta), `QBE-LOG-001` (setiap aksi tulis dicatat beserta pelaku), `QBE-VAL-001` (`VAL-MCU-01`..`08`), `QBE-TXN-001` (harga perusahaan dalam transaksi + kunci), `QBE-DTO-001` (tanpa entity EF di kontrak), `QBE-PAGE-001` (search/sort/paging), `QBE-OPT-001` (`filters/metadata` dan `options` dipakai layar `FE-MCU-003` dan booking), `QBE-DEL-001` (paket dinonaktifkan, komponen dihapus lunak beserta pelaku) |
| Checker QBE | `PASS` — 7 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Admin MCU belum dapat menyusun paket yang dijual, sehingga booking (`BE-MCU-011`) tidak punya paket untuk
dipilih dan tidak tahu harga yang berlaku bagi peserta perusahaan.

**Contoh.** Admin menyusun *Paket Basic* Rp 350.000 berisi Darah Lengkap (nilai pengembalian Rp 100.000),
Gula Darah Puasa (Rp 40.000, persyaratan *puasa 10–12 jam*), dan Thorax (Rp 250.000). Total nilai
pengembalian Rp 390.000 melebihi harga paket, sehingga sistem menolak dengan pesan *"Total nilai
pengembalian Rp 390.000 melebihi harga paket Rp 350.000."* — membatalkan seluruh komponen tidak boleh
mengembalikan uang lebih banyak daripada yang dibayar. Sesudah Thorax diturunkan menjadi Rp 150.000,
paket tersimpan. PT Contoh lalu mendapat harga khusus Rp 300.000 untuk 1 Oktober–31 Desember 2026, dan
admisi yang membooking karyawan PT Contoh pada 27 Oktober melihat Rp 300.000.

---

## 2. Proses bisnis

1. Admin MCU membuka **Paket MCU** dan membuat paket: kode, nama, tarif paket umum, dan komponen
   (Laboratorium, Radiologi, atau Pemeriksaan MCU) beserta nilai pengembalian, nilai jasa, dan persyaratan.
2. Sistem memeriksa `VAL-MCU-01`..`08` lalu menyimpan paket aktif.
3. Admin dapat mengubah isi paket. Komponen yang tetap ada dipertahankan barisnya; komponen yang dibuang
   ditandai terhapus agar order lama tetap terbaca.
4. Admin menambah **harga perusahaan** per masa berlaku, dan dapat **mengakhiri** masa berlakunya.
5. Admisi memanggil **pilihan paket** dengan perusahaan dan tanggal pemeriksaan, lalu melihat harga yang
   berlaku pada tanggal itu dan persyaratan yang harus disampaikan ke peserta.
6. Paket yang tidak dijual lagi **dinonaktifkan**: hilang dari pilihan, tetap ada di daftar.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Total nilai pengembalian > harga umum | `400` `VAL-MCU-04`; paket tidak tersimpan / tidak berubah |
| Tarif bukan tarif paket, nonaktif, atau tidak ada | `400` `VAL-MCU-03` |
| Kode paket sudah dipakai (tanpa peduli huruf besar/kecil) | `400` `VAL-MCU-01` |
| Paket tanpa komponen | `400` `VAL-MCU-02` |
| Komponen Lab dengan prosedur non-Lab, Rad tanpa modalitas, Pemeriksaan MCU tanpa jenis | `400` `VAL-MCU-06` |
| Pemeriksaan sama dua kali | `400` `VAL-MCU-07` — *"Gula Darah Puasa sudah ada di paket ini."* |
| Nilai negatif | `400` `VAL-MCU-08` |
| Harga perusahaan beririsan dengan harga lain | `400` `VAL-MCU-05` — menyebut irisan periodenya; harga lama tetap |
| Dua admin menambah harga perusahaan serentak | Diantrekan kunci `pg_advisory_xact_lock`; yang kedua melihat harga yang pertama |
| Dua admin menyimpan kode yang sama serentak | Index unik menolak yang kedua → `400` `VAL-MCU-01` |
| Mengakhiri harga sebelum tanggal mulai, atau memperpanjangnya | `400` |
| Paket/harga tidak ada | `404` |
| Tanpa hak akses `McuPackage` | `403` dari `AccessPermissionFilter` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/api-contract.md`, `permission-audit-matrix.md`, `validation-matrix.md`,
`testing/acceptance-test-matrix.md`, `erd/data-dictionary.md` bagian 1, `02-backend-architecture.md`
§5.1/§5.5/§5.6/§6, `05-screen-brief.md` (layar paket dan booking), model dan configuration paket MCU,
`McuOrderComponent.cs` (rujukan `PackageComponentId`), `MstTariff`, `MstProcedure`, `MstRadModality`,
`MstCompanyGuarantor`, `HmdMachineController.cs`, `HmdHttp.cs`, `HmdOperationResult.cs`,
`HemodialysisPermissions.cs`, `HmdServiceSupport.cs`, `HmdResourceDtos.cs`, `PagedResult.cs`,
`ApiResponse.cs`, `AccessPermissionAttribute.cs`, `AccessActionAttribute.cs`, `AccessTypes.cs`,
`LoggerService.cs`, `PermissionRegistryValidator.cs`, `MembershipPermissionRegistryTests.cs`, `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Constants/MedicalCheckupPermissions.cs` (baru) | Modul `HEALTH_SERVICE_MEDICAL_CHECKUP_MANAGEMENT`; 8 resource dan seluruh action `MCU-PERM-v1` |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` (baru) | `McuResult<T>` (200/400/403/404/409) dan `McuErrorCodes` (`VAL-MCU-01`..`08`) — dipakai ulang task MCU berikutnya |
| `MedicalCheckupManagement/Controllers/McuHttp.cs` (baru) | Pemetaan hasil → HTTP; log aplikasi dengan isi di teks pesan |
| `MedicalCheckupManagement/DTOs/McuPackageDtos.cs` (baru) | Query, request, response grup Mcu Package |
| `MedicalCheckupManagement/Services/McuPackageService.cs` (baru) | Daftar, pilihan + harga berlaku, detail, buat, ubah, status, tambah/akhiri harga perusahaan; validasi `VAL-MCU-01`..`08` |
| `MedicalCheckupManagement/Controllers/McuPackageController.cs` (baru) | 9 endpoint; tanpa `<summary>` di action; `filters/metadata` tepat sesudah konstruktor |
| `Program.cs` | `AddScoped<McuPackageService>()` |
| `Tests/QuilvianSystemBackend.McuTests/McuPackageServiceTests.cs` (baru) | 26 uji service |
| `Tests/QuilvianSystemBackend.McuTests/McuPackageControllerContractTests.cs` (baru) | 4 uji kontrak: grup Swagger, 9 endpoint + hak akses, urutan action, `PermissionRegistryValidator` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 9 endpoint baru sesuai `MCU-API-v1`; tidak ada endpoint lama yang berubah |
| Database | Tanpa migration; memakai tabel `BE-MCU-002`. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Resource baru `McuPackage` (`Read`/`Create`/`Update`) di modul `HEALTH_SERVICE_MEDICAL_CHECKUP_MANAGEMENT`; pemberiannya lewat Role Access (`SysAccessPolicies`). Tidak ada kolom sensitif pada katalog |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Pesan `VAL-MCU-06` untuk Lab dan Pemeriksaan MCU | Satu contoh kalimat (Radiologi) | *"Komponen Laboratorium wajib memilih prosedur laboratorium."*, *"Komponen Pemeriksaan MCU wajib memilih jenis pemeriksaan."* | Dibentuk dari pola contoh |
| Isian wajib, panjang, spasi pada kode, rentang tanggal, perusahaan tidak aktif | Tidak disebut matriks | `400` kode `MCU-INVALID-REQUEST` | Kebutuhan dasar `QBE-VAL-001` |
| Pesan `VAL-MCU-05` | Contoh *"… pada 1–31 Desember 2026."* | Menyebut **irisan** periode baru dengan harga yang sudah ada; periode tanpa batas ditulis *"… dan seterusnya"* | Irisan cocok dengan contoh `AT-MCU-02` dan menunjuk hari yang bentrok |
| `PATCH …/end` | Tidak merinci aturan | Hanya memendekkan; memperpanjang ditolak | Memperpanjang dapat menimpa harga lain; admin menambah harga baru |
| Respons `PATCH status` dan `…/end` | `object` | `McuPackageStatusResponse`, `McuCompanyPriceEndResponse` (objek kecil) | Bentuk `object` tetap terpenuhi |
| Pasangan komponen saat ubah paket | Tidak disebut | Dipasangkan menurut `Id`, lalu menurut pemeriksaan yang sama; ganti jenis/pemeriksaan = komponen baru | Order lama tetap menunjuk baris yang sama (`McuOrderComponent.PackageComponentId`) |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Package

Base URL: `api/v1/health-services/medical-checkup-management/packages`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata penyaring | `McuPackage : Read` | - | `McuPackageFilterMetadataResponse` | Tersedia |
| `GET` | `/` | Daftar paket | `McuPackage : Read` | `McuPackagePagedQuery` (search, isActive, sortBy, sortDirection, pageNumber, pageSize) | `PagedResult<McuPackageListResponse>` | Tersedia |
| `GET` | `/options` | Pilihan paket aktif + harga berlaku + persyaratan | `McuPackage : Read` | `?companyGuarantorId&date` | `List<McuPackageOptionResponse>` | Tersedia |
| `GET` | `/{id}` | Detail paket + komponen + persyaratan + harga perusahaan | `McuPackage : Read` | - | `McuPackageDetailResponse` | Tersedia |
| `POST` | `/` | Buat paket | `McuPackage : Create` | `CreateMcuPackageRequest` | `McuPackageDetailResponse` | Tersedia |
| `PUT` | `/{id}` | Ubah paket dan isinya | `McuPackage : Update` | `UpdateMcuPackageRequest` | `McuPackageDetailResponse` | Tersedia |
| `PATCH` | `/{id}/status` | Aktifkan/nonaktifkan | `McuPackage : Update` | `McuPackageStatusRequest` | `McuPackageStatusResponse` | Tersedia |
| `POST` | `/{id}/company-prices` | Tambah harga perusahaan | `McuPackage : Update` | `CreateMcuCompanyPriceRequest` | `McuCompanyPriceResponse` | Tersedia |
| `PATCH` | `/{id}/company-prices/{priceId}/end` | Akhiri masa berlaku harga perusahaan | `McuPackage : Update` | `EndMcuCompanyPriceRequest` | `McuCompanyPriceEndResponse` | Tersedia |

Kode status: `200`, `400` (`VAL-MCU-*` / `MCU-INVALID-REQUEST`, kode di `errors.code`), `403`, `404`.
Aksi tulis dicatat logger dengan kalimat berisi kode paket/periode dan pelaku; `GET` tidak dicatat.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 63 lulus (33 lama + 30 baru) | `PASS` | `Passed! - Failed: 0, Passed: 63` |
| `AT-MCU-01` | `400` `VAL-MCU-04`, pesan persis kontrak; 0 paket tersimpan | `PASS` | `AT_MCU_01_…` |
| `AT-MCU-02` | `400` `VAL-MCU-05` *"PT Contoh sudah punya harga untuk paket ini pada 1–31 Desember 2026."*; harga lama tetap 1 Okt–31 Des, Rp 300.000 | `PASS` | `AT_MCU_02_…` |
| `AT-MCU-03` | Tarif bukan paket, tarif paket nonaktif, dan tarif tidak ada → `400` `VAL-MCU-03`; juga pada harga perusahaan | `PASS` | `AT_MCU_03_…` (3 kasus), `Harga_perusahaan_dengan_tarif_bukan_paket_ditolak` |
| `VAL-MCU-01`, `02`, `06` (3 kasus), `07`, `08` | Ditolak dengan kode dan pesan matriks | `PASS` | Uji `VAL_MCU_*` |
| Jalur berhasil buat paket | Kode jadi huruf besar; 3 komponen; persyaratan; total pengembalian Rp 290.000 | `PASS` | `Paket_sah_tersimpan_…` |
| Ubah paket | Id komponen tetap (lewat Id **dan** tanpa Id); Thorax ditandai terhapus dengan pelaku; persyaratan lama ditandai terhapus | `PASS` | `Ubah_paket_mempertahankan_…` |
| Ubah paket gagal validasi | Tidak ada perubahan tersimpan (rollback) | `PASS` | `Ubah_paket_tetap_menegakkan_…` |
| Harga berlaku | Dalam masa: Rp 300.000 *Company*; sesudah masa: Rp 350.000 *General*; perorangan: Rp 350.000 | `PASS` | `Pilihan_memakai_harga_perusahaan_…` |
| Status | Nonaktif hilang dari pilihan, tetap di daftar (jumlah komponen dan total benar) | `PASS` | `Paket_nonaktif_…` |
| Akhiri harga | Sebelum mulai / diperpanjang ditolak; sesudah dipendekkan, harga baru Desember diterima; paket salah → `404` | `PASS` | `Mengakhiri_harga_…` |
| Format periode | 5 bentuk kalimat | `PASS` | `Periode_ditulis_dalam_bahasa_Indonesia` |
| Grup Swagger, route, 9 endpoint, string hak akses | Persis kontrak dan matriks | `PASS` | `McuPackageControllerContractTests` |
| `PermissionRegistryValidator` | Nol temuan untuk `McuPackage`; 9 rute terdaftar | `PASS` | `Lolos_PermissionRegistryValidator` |
| `filters/metadata` sesudah konstruktor; tanpa `<summary>` action | Action pertama = `filters/metadata`; 0 baris `///` di tingkat action | `PASS` | Uji kontrak; pencarian berkas |
| Checker QBE `-Path` (7 berkas + kontrol) | Berkas task nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| `403` tanpa hak akses | Ditegakkan `AccessPermissionFilter` bersama; tidak diuji lewat HTTP | `NOT RUN` | Registrasi atribut dibuktikan uji kontrak dan validator |
| Perebutan harga perusahaan / kode serentak | `pg_advisory_xact_lock` dan pemetaan `23505` hanya berlaku di PostgreSQL | `NOT RUN` | SQLite tanpa kunci advisory |
| Swagger pada aplikasi berjalan | Tidak dibuka | `NOT RUN` | Startup ke devYoga menunggu migration MCU (`BE-MCU-004` §7) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-01` lulus — contoh Rp 390.000 > Rp 350.000 ditolak, paket tidak tersimpan | Terpenuhi | §5 |
| `AT-MCU-02` lulus | Terpenuhi | §5 |
| `AT-MCU-03` lulus | Terpenuhi | §5 |
| `GET options?companyGuarantorId&date` mengembalikan harga yang berlaku pada tanggal itu | Terpenuhi | §5 baris *Harga berlaku* |
| Controller tanpa `<summary>` XML; `filters/metadata` tepat sesudah konstruktor | Terpenuhi | §5 |
| Verifikasi: uji unit service; `PermissionRegistryValidator` lulus | Terpenuhi | §5 |
| DoD: Swagger menampilkan grup dengan nama persis | Terpenuhi di tingkat atribut (`[Tags]` persis kontrak); **belum dilihat pada Swagger berjalan** | §5 dan §7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Swagger belum dibuka karena startup backend ke devYoga diperkirakan berhenti di `McuReferenceDataSeeder` sampai migration `BE-MCU-002`/`003` diterapkan (`BE-MCU-004` §7) |
| Masalah yang diketahui | **Dikoreksi 2026-10-22 lewat `BE-MCU-012`:** komponen yang disimpan bersamaan mendapat `CreateDateTime` kembar sehingga urutan di detail paket bisa tertukar; kini dinaikkan per komponen (`McuPackageService.AddComponent`). Lihat [`BE-MCU-012`](BE-MCU-012.md) §3.4 |
| Risiko tersisa | Peran yang memegang `McuPackage` belum dipetakan di Role Access (`MCU-GAP-02`, prasyarat UAT). Perebutan serentak hanya terbukti di PostgreSQL |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` (satu baris keliru di service dan rujukan atribut privat di uji diperbaiki sebelum validasi) |
| Status Git | Perubahan task: ` M Program.cs`; `??` `MedicalCheckupManagement/{Constants,Controllers,DTOs}/` dan dua berkas service baru; laporan ini; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`). Perubahan `BE-MCU-001`..`004` yang belum di-commit tidak disentuh |
| Langkah berikutnya | `BE-MCU-006` (data referensi) dan `BE-MCU-007` (jadwal) kini terbuka; `BE-MCU-010` dan `BE-MCU-020` tetap siap. `FE-MCU-003` dapat memakai grup ini |
