# Laporan Perubahan Backend — `BE-MCU-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-010` |
| Judul | Daftar Pasien MCU dan detail order |
| Slice | `MVP-0`/`MVP-1` — `EPIC MCU-02` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-010`* |
| Trace | `FR-MCU-037`; `MCU-DEC-031`; `MCU-FE-003` |
| Contract version | `MCU-API-v1` grup Mcu Order (GET); `MCU-PERM-v1` (`McuOrder : Read`); `05-screen-brief.md` 5.1 dan 5.5 (`MCU-FE-002`) — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-002` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika sedang (1), kontrak API baru (2), persistence yang sudah ada (1), keamanan berkaitan — masking NIK (1), satu halaman/workflow (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `MedicalCheckupManagement/{Controllers,DTOs,Services}/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — keempat acceptance criteria terbukti di harness |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` — satu baris registrasi |
| QBE yang berlaku | `QBE-SVC-001` (controller hanya memanggil `McuOrderQueryService`), `QBE-API-001`, `QBE-PERM-001` (`McuOrder : Read`), `QBE-DTO-001` (NIK mentah tidak pernah keluar dari daftar), `QBE-PAGE-001` (penyaring, cari, urut, halaman), `QBE-OPT-001` (metadata dipakai penyaring layar 5.1). Tanpa endpoint tulis, `QBE-LOG-001` tidak berlaku (`GET` tidak dicatat) |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Petugas MCU belum punya satu tempat untuk melihat siapa saja peserta pada suatu tanggal, sejauh mana
pemeriksaannya, dan apa yang terjadi pada order sebelumnya.

**Contoh.** Pada 27-10-2026 kartu ringkasan menunjukkan *Terjadwal 1 · Hadir 1 · Dalam Pemeriksaan 1 ·
Dibatalkan 1*. Baris Budi Santoso menampilkan `MCU-RSMMC-000123`, RM `00012345`, NIK `3201********0001`,
dr. Rina, dan kelengkapan *3 dari 4* — Darah Lengkap dan Pemeriksaan Fisik selesai, Gula Darah Puasa
dibatalkan dokter, Thorax masih dipesan. Siti Aminah, karyawan PT Contoh yang belum terdaftar sebagai
pasien, tampil tanpa RM (*Belum terdaftar*). Membuka order Budi memperlihatkan NIK utuh, komponen beserta
nomor pesanan Lab/Rad, catatan persyaratan *"Mengaku sarapan pukul 06.00"*, dan riwayat: *Terjadwal →
Hadir* oleh Sari (Admisi), penetapan dr. Rina, lalu *Hadir → Dalam Pemeriksaan*.

---

## 2. Proses bisnis

1. Petugas membuka **Daftar Pasien MCU**; layar memuat metadata penyaring, ringkasan tanggal hari ini,
   dan daftar order tanggal itu.
2. Petugas menyaring menurut status (klik kartu), sumber, perusahaan, dokter MCU, atau mencari nama/No.
   RM/No. Order.
3. Petugas membuka satu order untuk melihat detail dan riwayat.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Peserta identitas sementara (tanpa pasien) | Tetap tampil; RM kosong; NIK dari isian peserta |
| Dokter, perusahaan, atau pesanan Lab tidak ditemukan di master | Order/komponen tetap tampil; namanya kosong (proyeksi bersyarat, bukan `Include`) |
| Order belum berkomponen (*Terjadwal*) | Kelengkapan kosong, bukan *0 dari 0* |
| Tanggal tanpa order | Ringkasan tetap sembilan status bernilai nol; daftar kosong |
| Pemeriksaan belum dimulai | Dasar mulai (*Lunas*/*Terjamin*) tidak ditampilkan |
| Order tidak ada | `404` |
| Tanpa hak akses `McuOrder : Read` | `403` dari `AccessPermissionFilter` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/api-contract.md` (grup Mcu Order), `permission-audit-matrix.md`,
`contracts/state-transition-matrix.md`, `03-domain-architecture.md` (glosarium *tuntas*),
`00-interview-decisions.md` (`MCU-DEC-031`), `04-prd-to-mvp.md` (`FR-MCU-037`), `05-screen-brief.md` 5.1 dan
5.5, model `McuOrder`, `McuOrderComponent`, `McuOrderStatusHistory`, `McuOrderDoctorAssignment`,
`McuOrderRequirementCheck`, `MstPatient`, `MstDoctor`, `MstCompanyGuarantor`, `RegPatientEncounter`,
`ApplicationUser`, `ApplicationDbContext.cs`, komponen MCU `BE-MCU-005`..`007`, `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/DTOs/McuOrderDtos.cs` (baru) | Query dan response bagian baca grup Mcu Order |
| `MedicalCheckupManagement/Services/McuLabels.cs` (baru) | Label sembilan status (`MCU-DEC-031`), sumber, komponen, persyaratan, dasar mulai, kanal; aturan *tuntas*; masking NIK — dipakai ulang task berikutnya |
| `MedicalCheckupManagement/Services/McuOrderQueryService.cs` (baru) | Metadata, ringkasan, daftar, detail, riwayat; seluruhnya `AsNoTracking` |
| `MedicalCheckupManagement/Controllers/McuOrderController.cs` (baru) | 5 endpoint `GET`; `filters/metadata` dan `summary` tepat sesudah konstruktor; tanpa `<summary>` di action |
| `Program.cs` | `AddScoped<McuOrderQueryService>()` |
| `Tests/QuilvianSystemBackend.McuTests/McuOrderQueryServiceTests.cs` (baru) | 14 uji service berbaris master induk |
| `Tests/QuilvianSystemBackend.McuTests/McuOrderControllerContractTests.cs` (baru) | 3 uji kontrak |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 5 endpoint `GET` baru sesuai `MCU-API-v1`; tidak ada endpoint lama yang berubah |
| Database | Baca saja; tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Resource `McuOrder` mulai terdaftar dengan action `Read`. NIK dimasking di daftar; NIK utuh, tanggal lahir, telepon, dan catatan persyaratan hanya di detail per order. Tidak ada log aplikasi untuk `GET` |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Definisi *n dari m* | *n dari m komponen tuntas* | *m* = seluruh komponen; *n* = selesai **atau** dibatalkan resmi | Glosarium domain (*tuntas* termasuk dibatalkan resmi) dan matriks status (semua `Completed`/`Cancelled` → *Menunggu Resume*) |
| `date` pada daftar | Penyaring | Kosong = seluruh tanggal | Pencarian lintas tanggal; layar mengirim hari ini sebagai bawaan |
| Isi `filters/metadata` | `FilterMetadataResponse` | Status (urut alur), sumber, **perusahaan** (punya harga paket atau order MCU), **dokter MCU** (dokter order atau dokter jaga), urutan, ukuran halaman | Layar 5.1 mengambil pilihan penyaring perusahaan dan dokter dari metadata ini |
| Ringkasan | Jumlah per status | Selalu sembilan baris urut alur, termasuk nol, ditambah total | Kartu ringkasan tidak boleh hilang saat nol |
| Riwayat | Status & dokter | Satu daftar gabungan ber-`Kind` `Status`/`Doctor`, urut waktu, dengan nama pelaku | Tab Riwayat menampilkan keduanya dalam satu tabel |
| Sumber NIK | — | `ParticipantNik`, atau `MstPatient.IdentityNumber` bila order tertaut pasien | Order peserta terdaftar tidak wajib menyimpan NIK sendiri |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Order

Base URL: `api/v1/health-services/medical-checkup-management/orders` — bagian baca (`BE-MCU-010`).
Endpoint tulis grup ini (`bookings`, `recommendations`, `walk-ins`, `reschedule`, `cancel`,
`arrival-confirmation`) **belum tersedia** dan menyusul di `BE-MCU-011` dst.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata penyaring Daftar Pasien MCU | `McuOrder : Read` | - | `McuOrderFilterMetadataResponse` | Tersedia |
| `GET` | `/summary` | Ringkasan jumlah per status untuk tanggal | `McuOrder : Read` | `?date` (bawaan hari ini WIB) | `McuOrderSummaryResponse` | Tersedia |
| `GET` | `/` | Daftar Pasien MCU | `McuOrder : Read` | `McuOrderPagedQuery` (date, status, source, companyGuarantorId, mcuDoctorId, search, sortBy, sortDirection, pageNumber, pageSize) | `PagedResult<McuOrderListResponse>` (NIK dimasking, *n dari m*) | Tersedia |
| `GET` | `/{id}` | Detail order | `McuOrder : Read` | - | `McuOrderDetailResponse` | Tersedia |
| `GET` | `/{id}/history` | Riwayat status & dokter | `McuOrder : Read` | - | `List<McuOrderHistoryResponse>` | Tersedia |

Kode status: `200`, `403`, `404`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests -p:RunAnalyzers=False` (ikut membangun proyek utama) | 112 lulus (95 lama + 17 baru), 0 error kompilasi | `PASS` | `Passed! - Failed: 0, Passed: 112` |
| Kriteria (1) — penyaring | Tanggal (4 dari 5 order), status, sumber, perusahaan, dokter, cari RM/nama tanpa peduli huruf/No. Order — masing-masing tepat | `PASS` | `Kriteria_1_…` |
| Halaman dan urut | 5 data, 3 halaman; halaman 2 urut nama benar | `PASS` | `Daftar_berhalaman_…` |
| Kriteria (2) — NIK dimasking | `3201********0001` (dari pasien), `3202********0007` (dari isian peserta); tidak ada NIK utuh di daftar; detail memuat NIK utuh | `PASS` | `Kriteria_2_…`, `Masking_NIK` (5 kasus) |
| Kriteria (3) — *3 dari 4* | 2 selesai + 1 dibatalkan + 1 dipesan → *3 dari 4*; order tanpa komponen → kosong | `PASS` | `Kriteria_3_…` |
| Relasi master hilang | Order dengan dokter tak dikenal tetap tampil; komponen dengan pesanan Lab tak dikenal tetap tampil | `PASS` | `Order_yang_dokternya_…`, uji detail |
| Kriteria (4) — ringkasan 27-10-2026 | Sembilan label `MCU-DEC-031` urut alur; jumlah `[1,0,1,1,0,0,0,0,1]`; total 4; tanggal lain tetap sembilan baris | `PASS` | `Kriteria_4_…` |
| Detail | Label sumber/komponen/persyaratan, nama pemeriksa persyaratan, di luar paket, dasar mulai tersembunyi sebelum mulai, `404` | `PASS` | `Detail_memuat_…` |
| Riwayat | Status + dokter urut waktu, nama pelaku, `404` | `PASS` | `Riwayat_menggabungkan_…` |
| Metadata | 9 status, 7 sumber, 1 perusahaan, 1 dokter | `PASS` | `Metadata_memuat_…` |
| Grup Swagger, 5 endpoint `McuOrder : Read`, urutan `filters/metadata` lalu `summary`, `PermissionRegistryValidator` | Sesuai; nol temuan | `PASS` | `McuOrderControllerContractTests` |
| Tanpa `<summary>` action | 0 baris `///` di tingkat action | `PASS` | Pencarian berkas |
| Checker QBE `-Path` (5 berkas + kontrol) | Berkas task nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Pencarian dan halaman pada PostgreSQL; `403` lewat HTTP; Swagger berjalan | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU (`BE-MCU-004` §7) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Penyaring tanggal/status/sumber/perusahaan/dokter bekerja | Terpenuhi | §5 |
| (2) Daftar menampilkan NIK dimasking (`3201********0001`) | Terpenuhi | §5 |
| (3) Order berkomponen 4 dengan 3 tuntas tampil *3 dari 4* | Terpenuhi | §5 |
| (4) `summary?date=2026-10-27` mengembalikan jumlah per sembilan status | Terpenuhi | §5 |
| Verifikasi: uji integrasi harness dengan baris master induk | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pencarian memakai `ToLower().Contains(...)`, sama di SQLite dan PostgreSQL, tetapi tidak memakai index; cukup untuk daftar per tanggal, perlu ditinjau bila pencarian lintas tanggal menjadi berat |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Startup backend ke devYoga masih menunggu migration `BE-MCU-002`/`003`, sehingga Swagger dan `403` lewat HTTP belum dicoba |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` (satu konversi tipe `decimal?` diperbaiki sebelum build) |
| Status Git | Perubahan task: ` M Program.cs`; `??` empat berkas baru di folder modul MCU; laporan ini; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`) |
| Langkah berikutnya | `BE-MCU-011` (booking, jadwal ulang, batal sebelum datang) kini terbuka — seluruh dependency-nya (`004`, `005`, `007`, `010`) selesai. `BE-MCU-020` tetap siap. `FE-MCU-007` dapat memakai bagian baca ini |
