# Laporan Perubahan Backend — `BE-MCU-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-004` |
| Judul | Seri nomor dan master usulan |
| Slice | `MVP-0` — fondasi data `EPIC MCU-01` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-004`* |
| Trace | `FR-MCU-004` (seeder); `MCU-DEC-007`; `MCU-OQ-002`, `MCU-OQ-003` (nilai usulan) |
| Contract version | `02-backend-architecture.md` §5.5 (`McuOrderNumberService`) dan §9; [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) — `McuOrder.OrderNumber`, `McuBookingImport.ImportNumber`, `McuExaminationType`, `McuExaminationField`, `McuFitnessCategory` (`MCU-BP-001` r1, `approved` 2026-10-21); QBE-CODE-001..006 |
| Dependency | `BE-MCU-002` ✅ |
| Klasifikasi | `MEDIUM` — skor 4: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), persistence yang sudah ada (1); tanpa endpoint, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | Backend: `Areas/HealthServices/MedicalCheckupManagement/{Services,Seeders}/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser sejak `BE-MCU-003` |
| Tanggal | 2026-10-22 |
| Status | Selesai — ketiga acceptance criteria terbukti. Nilai master **usulan**, tidak boleh go-live sebelum `MCU-OQ-002`/`003` diratifikasi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` (2026-10-21); memakai alokator Platform `Num` apa adanya |
| Keberlakuan | `NEW CODE` (service dan seeder MCU); `TOUCHED LEGACY` untuk `Program.cs` — hanya tiga baris registrasi |
| QBE yang berlaku | `QBE-CODE-001` (format dan kebutuhan nomor dimiliki `McuOrderNumberService`), `QBE-CODE-002` (bukan controller), `QBE-CODE-003` (tanpa `Count+1`/`Max+1`), `QBE-CODE-004` (index unik `IX_McuOrder_OrderNumber`, `IX_McuBookingImport_ImportNumber` dari `BE-MCU-002`), `QBE-CODE-005` (awalan, reset, dan deret milik MCU), `QBE-CODE-006` (`NumberSeriesAllocator`), `QBE-MOD-001` (folder modul terdaftar), `QBE-ENT-003` (tanpa `SortOrder`; isian memakai `FieldNumber`) |
| Checker QBE | `PASS` — 3 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Dua hal belum ada sebelum booking dan resume MCU dapat dibangun:

1. **Nomor order yang dapat disebut petugas.** Order MCU wajib bernomor (`MCU-RSMMC-000001`), dan
   impor booking perusahaan juga (`MCU-IMP-000001`). Tanpa penerbit nomor yang aman, dua admisi
   yang membooking bersamaan dapat memperoleh nomor kembar.
2. **Master referensi kosong.** Dokter MCU tidak dapat memilih kategori kelayakan pada resume, dan
   formulir *Tanda Vital* tidak punya isian, karena tabelnya kosong.

**Contoh.** PT Maju Jaya mengirim daftar 40 karyawan. Impor itu mendapat `MCU-IMP-000001`, dan
setiap karyawan mendapat nomor order berurutan `MCU-RSMMC-000001` dst. Dokter MCU membuka resume dan
melihat pilihan *Fit*, *Fit dengan catatan*, *Unfit sementara*, *Unfit* — masing-masing bertanda
*USULAN — belum diratifikasi dokter MCU* sampai daftar itu disahkan.

---

## 2. Proses bisnis

Task ini menyediakan dua bahan yang dipakai task berikutnya; belum ada layar atau endpoint.

**Penerbitan nomor** (dipakai `BE-MCU-011` booking dan impor):

1. Service booking meminta nomor kepada `McuOrderNumberService` beserta petugas pelakunya.
2. Nomor diterbitkan `NumberSeriesAllocator` pada transaksi dan koneksinya sendiri, deret
   `MCU-ORDER` (impor: `MCU-IMPORT`), tanpa reset tahunan.
3. Booking menyimpan nomor itu. Index unik di basis data menolak nomor kembar sebagai penjaga terakhir.

**Pengisian master usulan** (setiap kali server menyala):

1. Seeder membaca kode yang sudah ada, termasuk yang sudah dihapus admin.
2. Hanya kode yang belum ada yang ditambahkan, bertanda usulan pada keterangannya.
3. Dokter MCU meratifikasi lewat layar master (`BE-MCU-006`) dengan mengubah keterangannya.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Booking gagal disimpan sesudah nomor terbit | Nomor hangus; booking berikutnya mendapat nomor sesudahnya. Lubang deret disengaja (`INV-PLT-002`) |
| Pelaku kosong (`Guid.Empty`) | Ditolak alokator `VAL-PLT-005`; pencacah tidak naik |
| Order ke-1.000.000 | `MCU-RSMMC-1000000` — enam digit adalah lebar minimum, sama dengan Lab/Rad |
| Admin mengubah nama/keterangan/status aktif master usulan | Tidak ditimpa saat server menyala ulang |
| Admin menghapus satu kategori atau satu isian | Tidak diisi ulang |
| Admin sudah membuat jenis pemeriksaan berkode `FISIK` sendiri | Seeder tidak menambah isian ke formulir admin |
| Seeder dijalankan sebelum migration `BE-MCU-002` diterapkan | Seeder gagal (`42P01`); dengan `Seeders:ContinueOnFailure = false` startup berhenti — lihat §7 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`02-backend-architecture.md` §5.5/§9, `erd/data-dictionary.md` (kolom master dan nomor),
`00-interview-decisions.md` (`MCU-DEC-007`, `MCU-OQ-002`/`003`), `NumberSeriesAllocator.cs`,
`NumberAllocationRequest.cs`, `NumberSeriesResetPolicies.cs`, `RadOrderNumberService.cs`,
`FinanceReceivableNumberGenerator.cs`, `HmdServiceSupport.cs`, `BbkBloodOrderService.cs`
(pemakaian alokator), `LabRejectionReasonSeeder.cs`, `HemodialysisMasterDataSeeder.cs`, model dan
configuration master MCU, `Program.cs` (registrasi dan `RunStartupSeederAsync`), `appsettings.json`
(`Seeders:ContinueOnFailure`), harness uji membership (pabrik konteks SQLite).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalCheckupManagement/Services/McuOrderNumberService.cs` (baru) | `AllocateOrderNumberAsync` dan `AllocateImportNumberAsync`; deret `MCU-ORDER`/`MCU-IMPORT`, awalan `MCU-RSMMC`/`MCU-IMP`, 6 digit, `NEVER`, boleh melewati 6 digit |
| `Areas/HealthServices/MedicalCheckupManagement/Seeders/McuReferenceDataSeeder.cs` (baru) | 4 kategori kelayakan, 2 jenis pemeriksaan, 6 isian; Id tetap; idempoten; penanda `USULAN — belum diratifikasi dokter MCU` |
| `Program.cs` | Dua `using`, `AddScoped<McuOrderNumberService>()`, dan `RunStartupSeederAsync("McuReferenceDataSeeder", …)` sesudah seeder Hemodialisa, sebelum gerbang integritas permission |
| `Tests/QuilvianSystemBackend.McuTests/McuOrderNumberServiceTests.cs` (baru) | 4 uji nomor |
| `Tests/QuilvianSystemBackend.McuTests/McuReferenceDataSeederTests.cs` (baru) | 5 uji seeder |

**Isi master usulan:**

| Master | Kode | Nama / label |
| --- | --- | --- |
| Kategori kelayakan | `FIT`, `FIT_DENGAN_CATATAN`, `UNFIT_SEMENTARA`, `UNFIT` | Fit; Fit dengan catatan; Unfit sementara; Unfit |
| Jenis `TANDA_VITAL` — *Tanda Vital & Antropometri* | butir 1–5: `TEKANAN_SISTOLIK`, `TEKANAN_DIASTOLIK` (mmHg), `NADI` (x/menit), `TINGGI_BADAN` (cm), `BERAT_BADAN` (kg) | Angka, wajib |
| Jenis `FISIK` — *Pemeriksaan Fisik* | butir 1: `TEMUAN_FISIK` | Teks, wajib |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint |
| Database | Tanpa migration. Seeder menulis ke tiga tabel MCU dan alokator menulis `NumNumberSeries` saat dipakai. **Tidak ada eksekusi ke basis data mana pun** dalam task ini |
| Keamanan/Auth | Tidak ada. Pelaku penerbitan nomor tercatat pada baris deret (`CreateBy`/`UpdateBy`) |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Isian *Pemeriksaan Fisik* | §9 hanya menyebut nama jenis; isian menunggu `MCU-OQ-002` | Satu isian teks `TEMUAN_FISIK` | Formulir fisik dokter MCU belum diterima; menebak butir klinis lebih berbahaya daripada ruang tulis bebas. **Asumsi**, ikut diratifikasi |
| Kode kategori/jenis selain contoh kamus | Kamus memberi contoh `FIT`, `UNFIT_SEMENTARA`, `FISIK` | `FIT_DENGAN_CATATAN`, `UNFIT`, `TANDA_VITAL` mengikuti pola yang sama | Kamus hanya memberi contoh |
| Penanda belum diratifikasi pada isian | Kriteria (3) | Isian tidak punya kolom keterangan; penanda dibawa jenis induknya | Menambah kolom berarti mengubah skema `BE-MCU-002` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task tanpa endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests -p:RunAnalyzers=False` (ikut membangun proyek utama) | 33 lulus (24 lama + 9 baru), 0 error kompilasi | `PASS` | `Passed! - Failed: 0, Passed: 33` |
| Dua order berurutan | `MCU-RSMMC-000001` lalu `MCU-RSMMC-000002` | `PASS` | `Dua_order_berurutan_memperoleh_000001_lalu_000002` |
| Deret impor terpisah | Sesudah dua order: impor `MCU-IMP-000001`, order berikutnya `000003` | `PASS` | `Deret_impor_terpisah_dari_deret_order` |
| Pencacah durabel | Dua baris `NumNumberSeries`: `GLOBAL`, `NEVER`, nilai 2 dan 1, pelaku tercatat | `PASS` | `Pencacah_tersimpan_global_tanpa_reset_dengan_pelaku` |
| Pelaku kosong | `VAL-PLT-005`; pencacah tetap 1; nomor berikutnya `000002` | `PASS` | `Pelaku_kosong_ditolak_tanpa_menerbitkan_nomor` |
| Seeder jalan pertama | 4 kategori, 2 jenis, 6 isian; urutan butir tanda vital sesuai | `PASS` | `Jalan_pertama_…` |
| Seeder dua kali | Jalan kedua menambah 0; jumlah tetap 4/2/6 | `PASS` | `Dijalankan_dua_kali_tidak_menggandakan_data` |
| Isian admin | Nama, keterangan, status aktif yang diubah tetap; kategori dan isian yang dihapus tidak diisi ulang | `PASS` | `Isian_yang_diubah_atau_dihapus_admin_…` |
| Jenis buatan admin berkode sama | Tidak ditambahi isian; kategori dan jenis lain tetap diisi | `PASS` | `Jenis_berkode_sama_buatan_admin_…` |
| Penanda usulan | Seluruh kategori dan jenis memuat penanda; seluruh isian milik jenis bertanda | `PASS` | `Setiap_master_usulan_bertanda_belum_diratifikasi` |
| Checker QBE `-Path` (3 berkas task + kontrol) | Berkas task nol temuan; kontrol terkena `QBE-CFG-001`/`QBE-MOD-002` | `PASS` | Keluaran checker |
| Perebutan nomor serentak | Tidak dapat dibuktikan di SQLite (tanpa `pg_advisory_xact_lock`) | `NOT RUN` | Lingkup uji alokator Platform (`PLT-BE-004`); index unik tetap menjaga |
| Seeder pada PostgreSQL / startup sungguhan | Tidak dijalankan | `NOT RUN` | Migration `BE-MCU-002` belum diterapkan; eksekusi basis data di luar wewenang task |

Uji manual: `NOT APPLICABLE` — tidak ada perilaku pengguna.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Dua order berurutan memperoleh `MCU-RSMMC-000001` lalu `000002` | Terpenuhi | §5 baris *Dua order berurutan* |
| (2) Seeder dijalankan dua kali tidak menggandakan data dan tidak menimpa isian yang sudah diubah admin | Terpenuhi | §5 baris *Seeder dua kali*, *Isian admin*, *Jenis buatan admin* |
| (3) Setiap master usulan bertanda belum diratifikasi pada keterangannya | Terpenuhi | §5 baris *Penanda usulan*; isian lewat jenis induknya (§3.4) |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Startup backend ke basis data yang belum menerima migration `BE-MCU-002` akan berhenti** di `McuReferenceDataSeeder` (`42P01`), karena `Seeders:ContinueOnFailure` bernilai `false` di `appsettings.json`. Perilaku ini sama dengan seeder modul lain; jalan keluarnya menerapkan migration `BE-MCU-002` + `003` (atas izin) atau menyalakan `Seeders:ContinueOnFailure` di lingkungan pengembangan |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Nilai klinis master **tidak boleh go-live** sebelum `MCU-OQ-002`/`003` diratifikasi dokter MCU (pemilik: Yoga + dokter MCU). Isian *Pemeriksaan Fisik* adalah asumsi (§3.4) |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` |
| Status Git | Perubahan task: ` M Program.cs`; `??` `MedicalCheckupManagement/Services/`, `MedicalCheckupManagement/Seeders/` (di dalam folder modul yang sudah `??`); laporan ini; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`). Perubahan `BE-MCU-001`..`003` yang belum di-commit tidak disentuh |
| Langkah berikutnya | `BE-MCU-005` (API paket + konstanta izin), `BE-MCU-010`, `BE-MCU-020` siap. `BE-MCU-011` kini tinggal menunggu `005`, `007`, `010` |
