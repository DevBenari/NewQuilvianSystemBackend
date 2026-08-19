# Laporan Perubahan Backend — `BE-IGD-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-001` |
| Judul | Seluruh service IGD didaftarkan sehingga endpoint benar-benar dapat dipanggil |
| Slice | S0 — Modul benar-benar hidup |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `CAP-16` (status `Repair`), `IGD-DEC-046` |
| Contract version | API `0.2.0` — **tidak berubah** |
| Commit backend saat dikerjakan | `d2682c3ca045d95d293564dd6f4bdad9d6df8f6c` |
| Tanggal | 14 Agustus 2026 |
| Jenis perubahan | Perbaikan penghalang saat aplikasi berjalan (*runtime blocker*), bukan penambahan fitur |

---

## 1. Masalah yang diperbaiki

### 1.1 Apa yang terjadi sebelum perubahan ini

Modul Instalasi Gawat Darurat (IGD) sudah punya kode yang lengkap: sembilan controller,
tujuh service, model, dan tabelnya. Tetapi seluruh 52 endpoint IGD **gagal dipanggil**.

Penyebabnya satu hal saja. Aplikasi ASP.NET Core memakai mekanisme bernama *dependency
injection* — disingkat DI, artinya "aplikasi yang menyiapkan sendiri objek yang dibutuhkan
sebuah kelas". Supaya aplikasi tahu cara menyiapkan objek `EmergencyVisitService`, kelas itu
harus didaftarkan lebih dulu di `Program.cs`. Kedelapan service IGD tidak pernah didaftarkan.

Akibatnya, ketika petugas membuka layar IGD:

1. Permintaan masuk ke aplikasi.
2. Aplikasi mencoba membuat `EmergencyVisitController`.
3. Controller itu meminta `EmergencyVisitService`.
4. Aplikasi tidak tahu cara membuatnya, lalu berhenti dengan galat.
5. Petugas menerima kode `500` — "kesalahan sistem".

> **Contoh nyata:** perawat menekan tombol "Daftar Kunjungan IGD" pada pukul 07.00. Layar
> tidak menampilkan daftar apa pun, hanya pesan kegagalan sistem. Yang menyesatkan, kode
> modul IGD sendiri **tidak pernah dijalankan** — kegagalan terjadi sebelum barisan pertama
> kode IGD sempat berjalan. Karena itu memeriksa isi tabel, memeriksa data, atau memeriksa
> aturan bisnis tidak akan pernah menemukan sebabnya.

### 1.2 Mengapa task ini dikerjakan paling awal

Selama endpoint IGD tidak dapat dipanggil, seluruh task IGD berikutnya tidak dapat dibuktikan
berjalan. Menambahkan fitur baru di atas modul yang tidak bisa dipanggil sama saja dengan
menulis kode yang tidak pernah dieksekusi.

---

## 2. Proses bisnis yang kembali dapat berjalan

Perubahan ini **tidak mengubah satu pun aturan bisnis**. Yang berubah hanyalah kenyataan
bahwa alur di bawah ini sekarang dapat dijalankan, sebelumnya tidak.

### 2.1 Tujuan

Petugas IGD dapat memakai seluruh layar IGD tanpa menerima kegagalan sistem.

### 2.2 Pelaku

| Pelaku | Perannya dalam alur ini |
| --- | --- |
| Perawat IGD | Membuka daftar kunjungan, mengisi penilaian triase, mencatat observasi |
| Dokter IGD | Mencatat resusitasi, tindakan, dan keputusan akhir pasien (*disposition*) |
| Admin IGD | Mengatur data pengaturan IGD lewat menu master data |
| Tim Backend/API | Pemilik perbaikan ini |

### 2.3 Pemicu

Aplikasi dinyalakan. Pendaftaran service terjadi satu kali saat aplikasi mulai berjalan,
bukan setiap ada permintaan.

### 2.4 Prasyarat

Tidak ada. Perubahan ini tidak membutuhkan data, migration, atau konfigurasi baru.

### 2.5 Langkah utama

1. Aplikasi menyala dan membaca daftar pendaftaran service pada `Program.cs`.
2. Delapan service IGD kini ikut terdaftar sebagai *scoped*, yaitu satu objek untuk satu
   permintaan HTTP — pola yang sama dengan 90-an service lain di aplikasi ini.
3. Petugas membuka layar IGD.
4. Aplikasi membuat controller IGD beserta service yang dimintanya.
5. Kode modul IGD dijalankan dan membalas sesuai aturan bisnisnya sendiri.

### 2.6 Aturan bisnis

Tidak ada aturan bisnis baru. Aturan pemeriksaan hak akses, validasi, dan status yang sudah
tertulis di dalam controller IGD tetap berlaku apa adanya. Perubahan ini hanya membuat
aturan-aturan itu mendapat kesempatan untuk dijalankan.

### 2.7 Perubahan status

Tidak ada. Tidak ada satu pun status data yang berubah oleh perubahan ini.

### 2.8 Jalur tidak normal

| Kejadian | Sebelum perubahan | Sesudah perubahan |
| --- | --- | --- |
| Pengguna belum masuk | `401` — sudah benar sejak dulu, karena pemeriksaan sesi terjadi sebelum controller dibuat | Tetap `401` |
| Pengguna masuk tetapi tidak punya hak akses IGD | `500` — kegagalan sistem, padahal seharusnya penolakan yang sopan | `403` sesuai aturan hak akses yang sudah ada di controller |
| Pengguna berhak membuka daftar kunjungan | `500` | `200` beserta datanya |
| Data yang dicari tidak ada | `500` | `404` sesuai aturan controller |

Baris kedua adalah perubahan yang paling terasa bagi pengguna: penolakan hak akses yang
sebelumnya menyamar sebagai kerusakan sistem, sekarang tampil sebagai penolakan yang jelas.

### 2.9 Hasil akhir

Seluruh 52 endpoint IGD ditambah 6 endpoint pengaturan IGD dapat dipanggil. Perilaku setiap
endpoint ditentukan oleh kodenya masing-masing, bukan oleh perubahan ini.

---

## 3. File yang diubah

Hanya satu berkas yang disentuh, sesuai batas scope task.

| File | Perubahan |
| --- | --- |
| `Program.cs` | Menambah 2 baris `using` dan 8 baris pendaftaran service IGD |

### 3.1 Rincian perubahan

**Tambahan `using`** (setelah `...ClinicalManagement.Services`):

```csharp
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Services;
```

Dua baris, bukan satu, karena `EmergencySettingService` memang tinggal di area master data,
bukan di folder IGD. Ini bukan kesalahan penulisan, melainkan keadaan kode yang sudah ada.

**Tambahan pendaftaran** (setelah `PrescriptionFinalCheckService`):

```csharp
// Instalasi Gawat Darurat (IGD). Tanpa pendaftaran ini seluruh controller IGD gagal
// dibuat oleh dependency injection, sehingga endpoint-nya membalas 500 sebelum kode
// modul sempat dijalankan. Pola mengikuti service lain: kelas konkret, tanpa interface.
builder.Services.AddScoped<EmergencyDocumentNumberService>();
builder.Services.AddScoped<EmergencyVisitService>();
builder.Services.AddScoped<EmergencyTriageService>();
builder.Services.AddScoped<EmergencyResuscitationService>();
builder.Services.AddScoped<EmergencyObservationService>();
builder.Services.AddScoped<EmergencyDispositionService>();
builder.Services.AddScoped<EmergencyTransferService>();
builder.Services.AddScoped<EmergencySettingService>();
```

`EmergencyDocumentNumberService` sengaja ditulis paling atas karena empat service lain
membutuhkannya. Urutan penulisan sebenarnya tidak memengaruhi hasil — aplikasi menyusun
sendiri urutan pembuatannya — tetapi urutan ini membuat ketergantungannya terbaca.

### 3.2 Yang sengaja tidak diubah

| Yang tidak disentuh | Alasan |
| --- | --- |
| Isi service dan controller IGD | Di luar scope task. Task ini hanya soal pendaftaran |
| Migration dan skema database | Tidak ada perubahan data sama sekali |
| Hak akses dan permission | Sudah tertulis di controller, tidak diubah |
| Nama folder `Controller` (tunggal) di modul IGD | Utang teknis yang dijadwalkan pada `BE-IGD-013` |

---

## 4. Verifikasi

### 4.1 Yang sudah dijalankan

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Tidak ada pendaftaran ganda | Pencarian `Scan(`, `Scrutor`, `FromAssembly` pada seluruh berkas `.cs` dan `.csproj`; satu-satunya hasil adalah `ApplyConfigurationsFromAssembly` milik EF Core, yang tidak mendaftarkan service | Lulus — tidak ada mekanisme pendaftaran tersembunyi, sehingga risiko pendaftaran ganda pada roadmap tertutup |
| Delapan kelas benar-benar ada | Penelusuran berkas; tujuh di `Areas/HealthServices/EmergencyInstallationManagement/Services/`, satu (`EmergencySettingService`) di `Areas/HealthServices/MasterData/Services/` | Lulus |
| Seluruh kebutuhan controller IGD terpenuhi | Pembacaan constructor sepuluh controller. Kebutuhannya hanya tiga jenis: `ApplicationDbContext` (terdaftar lewat `AddDbContext`), `LoggerService` (sudah terdaftar), dan service IGD (kini terdaftar) | Lulus — tidak ada kebutuhan yang tersisa tanpa pendaftaran |
| Seluruh kebutuhan service IGD terpenuhi | Pembacaan constructor delapan service. Kebutuhannya hanya `ApplicationDbContext` dan `EmergencyDocumentNumberService` | Lulus |
| Build project | `dotnet build QuilvianSystemBackend.csproj` | **Lulus** — 0 galat, 125 peringatan, seluruhnya peringatan lama yang tidak berkaitan dengan perubahan ini |

Rangkaian pemeriksaan constructor di atas bukan formalitas. Justru itulah bentuk kegagalan
yang sedang diperbaiki: satu saja kebutuhan yang belum terdaftar sudah cukup membuat controller
gagal dibuat, dan gejalanya sama persis dengan sebelum perbaikan.

### 4.2 Yang belum dijalankan, beserta alasannya

| Yang belum diuji | Alasan | Cara menutupnya |
| --- | --- | --- |
| Aplikasi menyala sungguhan lalu endpoint IGD dipanggil (acceptance criteria nomor 1–4) | `appsettings.Development.json` mengarah ke basis data pengembangan **bersama** di `160.22.250.77`, bukan basis data lokal. Menyalakan aplikasi akan menjalankan seeder yang menulis ke basis data itu. Aturan repository melarang perubahan basis data non-lokal tanpa izin eksplisit | Jalankan `dotnet run` pada lingkungan lokal, lalu panggil `GET /api/v1/health-services/emergency-installation-management/emergency-visits` memakai token yang berhak. Harapannya `200`, bukan `500` |
| Test aktivasi controller otomatis | Repository ini belum memiliki project test sama sekali. Membuatnya adalah keputusan arsitektur tersendiri dan berada di luar scope `BE-IGD-001` yang berbunyi "`Program.cs` saja" | Perlu keputusan pemilik: buat project test, atau catat sebagai bukti manual pada `BE-IGD-014` |

Karena dua hal di atas, task ini **belum boleh dinyatakan selesai penuh**. Yang sudah terbukti
adalah kode terdaftar dan project berhasil dibangun. Yang belum terbukti adalah endpoint
benar-benar membalas saat aplikasi menyala.

---

## 5. Dokumentasi endpoint

Perubahan ini **tidak menambah, menghapus, atau mengubah satu pun endpoint**. Daftar berikut
adalah grup Swagger yang sebelumnya terdaftar tetapi selalu gagal, dan kini dapat dipanggil.

Seluruh endpoint memerlukan pengguna yang sudah masuk (*authenticated*).

### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

7 endpoint. Service pendukung: `EmergencyVisitService`, `EmergencyDocumentNumberService`.

### Health Services / Emergency Installation Management / Emergency Triage

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triages`

6 endpoint. Service pendukung: `EmergencyTriageService`.

### Health Services / Emergency Installation Management / Emergency Triage Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triage-details`

5 endpoint. Tidak memakai service khusus, tetapi tetap gagal sebelumnya karena berada dalam
satu modul yang sama.

### Health Services / Emergency Installation Management / Emergency Resuscitation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-resuscitations`

6 endpoint. Service pendukung: `EmergencyResuscitationService`, `EmergencyDocumentNumberService`.

### Health Services / Emergency Installation Management / Emergency Observation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observations`

6 endpoint. Service pendukung: `EmergencyObservationService`, `EmergencyDocumentNumberService`.

### Health Services / Emergency Installation Management / Emergency Observation Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observation-details`

5 endpoint.

### Health Services / Emergency Installation Management / Emergency Procedure Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-procedure-details`

5 endpoint.

### Health Services / Emergency Installation Management / Emergency Transfer

Base URL: `api/v1/health-services/emergency-installation-management/emergency-transfers`

6 endpoint. Service pendukung: `EmergencyTransferService`, `EmergencyDocumentNumberService`.

### Health Services / Emergency Installation Management / Emergency Disposition

Base URL: `api/v1/health-services/emergency-installation-management/emergency-dispositions`

6 endpoint. Service pendukung: `EmergencyDispositionService`.

### Health Services / Master Data / Emergency Installation Management / Emergency Setting

Base URL: `api/v1/health-services/master-data/emergency-installation-management/emergency-settings`

6 endpoint. Service pendukung: `EmergencySettingService`.

Jumlah keseluruhan: 52 endpoint pada sembilan grup IGD, ditambah 6 endpoint pengaturan IGD
di area master data.

### Kode status dan artinya

| Kode | Arti teknis | Arti bagi pengguna |
| --- | --- | --- |
| `200` | Berhasil | Permintaan diproses dan datanya tersedia |
| `400` | Permintaan tidak valid | Isian tidak lengkap atau melanggar aturan IGD |
| `401` | Belum masuk | Sesi habis; petugas perlu masuk ulang |
| `403` | Tidak berwenang | Petugas sudah masuk tetapi tidak punya hak untuk tindakan ini |
| `404` | Tidak ditemukan | Data kunjungan atau penilaian yang dibuka tidak ada |
| `500` | Kesalahan sistem | Sebelum perbaikan ini, **seluruh** endpoint IGD membalas kode ini |

---

## 6. Dampak migration dan konfigurasi

| Aspek | Dampak |
| --- | --- |
| Migration | Tidak ada. Tidak ada migration dibuat maupun dijalankan |
| Skema database | Tidak berubah |
| Data | Tidak ada data yang ditulis, diubah, atau dihapus |
| Konfigurasi dan kredensial | Tidak disentuh |
| Kontrak API | API `0.2.0` tidak berubah, sehingga tidak ada dampak ke frontend selain endpoint yang kini berfungsi |

---

## 7. Risiko tersisa

| No | Risiko | Akibat nyata bila diabaikan |
| ---: | --- | --- |
| 1 | Endpoint IGD kini benar-benar berjalan, tetapi enam tabel master IGD masih kosong (`BE-IGD-003`) | Perawat dapat membuka layar triase, namun tidak menemukan satu pun level triase untuk dipilih. Penilaian tidak dapat disimpan |
| 2 | Target waktu tunggu masih bertipe angka biasa (`BE-IGD-002`) | Level yang targetnya belum ditetapkan diperlakukan sebagai "harus dilayani seketika", sehingga peringatan palsu dapat membanjiri layar perawat |
| 3 | Belum ada bukti runtime dan belum ada test otomatis | Tidak ada yang mencegah pendaftaran ini terhapus lagi tanpa ketahuan pada perubahan berikutnya |

---

## 8. Bukti penelusuran

| Klaim | Bukti |
| --- | --- |
| Delapan service tidak terdaftar sebelum perubahan | `NewQuilvianSystemBackend` + `Program.cs` baris 259–388 (sebelum diubah) + `d2682c3` |
| Tidak ada mekanisme pendaftaran otomatis | `NewQuilvianSystemBackend` + pencarian `Scan(`/`Scrutor`/`FromAssembly` seluruh repo + `d2682c3` |
| Kebutuhan sepuluh controller IGD | `Areas/HealthServices/EmergencyInstallationManagement/Controller/*.cs` dan `Areas/HealthServices/MasterData/Controllers/EmergencySettingController.cs`, constructor masing-masing + `d2682c3` |
| Lokasi `EmergencySettingService` di area master data | `Areas/HealthServices/MasterData/Services/EmergencySettingService.cs` baris 8 dan 13 + `d2682c3` |
| Tidak ada berkas `.cs` yang berubah sejak blueprint diaudit | `git diff --name-only e5331a0..d2682c3 -- "*.cs"` menghasilkan daftar kosong |

---

## 9. Catatan gate

Roadmap `docs/module-blueprints/igd/roadmap/backend-roadmap.md` masih berstatus `DRAFT`
dengan `approved_by` kosong, karena urutan prioritas belum pernah dibaca Product/Domain Owner.
Pengerjaan tetap dilanjutkan dengan pertimbangan berikut, dan pertimbangan ini perlu diketahui
pemilik:

1. Task ini tidak mengambil satu pun keputusan bisnis baru.
2. Kontrak API `0.2.0` tidak berubah sama sekali.
3. Isi task bersumber dari blueprint revision 4 yang **sudah** disetujui pada 14 Agustus 2026.
4. Yang belum disetujui adalah **urutan prioritas**, bukan kebenaran isi task.

Bila pemilik menghendaki urutan yang berbeda, perubahan ini tetap aman karena sifatnya
memperbaiki penghalang, bukan menambah perilaku baru.
