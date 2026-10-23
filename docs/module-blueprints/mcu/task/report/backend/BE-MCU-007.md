# Laporan Perubahan Backend — `BE-MCU-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-007` |
| Judul | API jadwal, kuota, pelampauan, dokter jaga |
| Slice | `MVP-0` — jadwal `EPIC MCU-01` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-007`* |
| Trace | `FR-MCU-005`, `FR-MCU-006` (dan fondasi `FR-MCU-011`); `MCU-DEC-012`, `029`, `037`, `041`, `042` |
| Contract version | `MCU-API-v1` grup Mcu Daily Schedule; `MCU-VAL-v1` `VAL-MCU-10`..`15`; `MCU-PERM-v1` (`McuSchedule : Read/Update/ApproveQuotaOverride`); [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) bagian 2 — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-002` ✅, `BE-MCU-005` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika kompleks — konkurensi kuota (2), kontrak API baru (2), persistence yang sudah ada (1), keamanan berkaitan — action izin khusus (1), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `MedicalCheckupManagement/{Controllers,DTOs,Services}/`, `Program.cs` (registrasi), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — keempat kriteria terbukti di harness. Penguncian baris PostgreSQL di bawah beban serentak sungguhan belum dibuktikan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` — satu baris registrasi |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (action `ApproveQuotaOverride` terpisah dari `Update`), `QBE-LOG-001` (pelampauan: tanggal, jumlah, pelaku), `QBE-VAL-001` (`VAL-MCU-10`..`15`), `QBE-TXN-001` (atur rentang dalam transaksi; pakai/lepas kuota ikut transaksi pemanggil), `QBE-DTO-001`, `QBE-DEL-001` (tidak ada hapus jadwal; tutup = `IsOpen`) |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Booking MCU (`BE-MCU-011`) belum punya tempat menanyakan "apakah tanggal ini buka dan masih ada kursi",
dan kepala unit belum dapat mengizinkan rombongan yang melebihi kuota. Tanpa penjaga konkurensi, dua
petugas admisi yang membooking kursi terakhir bersamaan sama-sama berhasil dan kuota bocor.

**Contoh.** 27-10-2026 berkuota 30 dan sudah penuh. Booking ke-31 ditolak *"Kuota MCU tanggal 27-10-2026
sudah penuh (30/30). Minta persetujuan kepala unit untuk melampaui."* Kepala unit menyetujui pelampauan 5
beralasan *Kontrak tahunan PT Contoh*; booking ke-31 sampai ke-35 diterima, ke-36 ditolak *(35/35)*. Admin
yang mencoba menurunkan kuota 27-10 menjadi 25 padahal sudah 26 peserta terjadwal ditolak *"Sudah ada 26
peserta terjadwal; kuota tidak boleh di bawah 26."* Dokter jaga 27–29 Oktober diatur dr. Rina, dan
kalender menampilkan namanya.

---

## 2. Proses bisnis

1. Admin/kepala unit membuka **Jadwal MCU**, memilih rentang tanggal, lalu mengatur buka/tutup, kuota, dan
   dokter MCU jaga (`PUT daily-schedules`). Seluruh rentang tersimpan atau tidak sama sekali.
2. Kalender (`GET daily-schedules?from&to`) menampilkan setiap tanggal — termasuk yang belum diatur
   (dianggap tutup) — beserta kapasitas, terpakai, sisa, dokter jaga, dan riwayat pelampauan.
3. Kepala unit menyetujui **pelampauan** pada satu tanggal dengan jumlah dan alasan (`POST
   daily-schedules/{tanggal}/quota-overrides`).
4. Service booking, *walk-in*, jadwal ulang, dan tutup hari memanggil **pakai kuota**
   (`ReserveAsync`) dan **lepas kuota** (`ReleaseAsync`) di dalam transaksinya sendiri.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Kuota baru < terpakai | `409` `VAL-MCU-14`; pada rentang, pesan diawali tanggalnya; rentang tidak tersimpan |
| Menutup tanggal yang sudah berpeserta | `409` |
| Tanggal belum diatur atau tutup saat dipakai/dilampaui | `409` `VAL-MCU-10` *"MCU tutup pada 27-10-2026."* |
| Kuota penuh | `409` `VAL-MCU-11` menyebut terpakai/kapasitas |
| Dua petugas mengambil kursi terakhir bersamaan | Satu berhasil; yang kalah `409` `VAL-MCU-12` *"Kuota baru saja terpakai petugas lain. Muat ulang."* |
| Kalah balapan tetapi kursi masih tersisa | Memuat ulang lalu berhasil (paling banyak 3 percobaan) |
| Pelampauan tanpa alasan | `400` `VAL-MCU-13` |
| Pengguna tanpa `ApproveQuotaOverride` | `403` dari `AccessPermissionFilter` |
| Langkah pemanggil sesudah pakai kuota gagal | Transaksi pemanggil dibatalkan → pemakaian kuota ikut batal |
| Lepas kuota saat terpakai 0 | Tidak berubah; tidak pernah negatif |
| Admin mengatur jadwal ketika booking sedang mengambil kursi | `Version` berbeda → `409` *muat ulang lalu coba lagi* |
| Tanggal lampau, rentang terbalik/> 92 hari, kuota negatif, dokter tidak dikenal | `400` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`contracts/api-contract.md` (grup Mcu Daily Schedule), `permission-audit-matrix.md`,
`validation-matrix.md` bagian 2, `testing/acceptance-test-matrix.md` (`AT-MCU-04`..`07`),
`erd/data-dictionary.md` bagian 2, `02-backend-architecture.md` §5.5, `McuDailySchedule`,
`McuQuotaOverride` beserta configuration-nya, `MstDoctor`, `HmdScheduleService.cs` (pola `Version++` dan
`DbUpdateConcurrencyException`), komponen MCU `BE-MCU-005`/`006`, `ApplicationDbContext.cs`, `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/DTOs/McuScheduleDtos.cs` (baru) | Query, request, response jadwal; `McuQuotaReservation` untuk service pemanggil |
| `MedicalCheckupManagement/Services/McuScheduleService.cs` (baru) | Baca rentang, atur rentang, pelampauan, `ReserveAsync`, `ReleaseAsync`, `CheckNotPast` (`VAL-MCU-15`) |
| `MedicalCheckupManagement/Controllers/McuScheduleController.cs` (baru) | 3 endpoint; tanpa `<summary>` di action |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Tambah kode `VAL-MCU-10`..`15` dan `MCU-CONCURRENCY` |
| `Program.cs` | `AddScoped<McuScheduleService>()` |
| `Tests/QuilvianSystemBackend.McuTests/McuScheduleServiceTests.cs` (baru) | 13 uji service, termasuk balapan dua `DbContext` |
| `Tests/QuilvianSystemBackend.McuTests/McuScheduleControllerContractTests.cs` (baru) | 3 uji kontrak |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint baru sesuai `MCU-API-v1`; tidak ada endpoint lama yang berubah |
| Database | Tanpa migration; memakai tabel `BE-MCU-002`. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Resource baru `McuSchedule` dengan action `Read`, `Update`, dan `ApproveQuotaOverride` (bertipe `Update` di Role Access); pemberiannya lewat `SysAccessPolicies` |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| `GET` mengembalikan tanggal yang belum diatur | Tidak disebut | Ikut dikembalikan dengan `IsConfigured = false`, tutup | Kalender tidak berlubang |
| Ruas respons tambahan | `McuDailyScheduleResponse` memuat dokter jaga | Juga `Capacity`, `RemainingQuota`, `Version`, `QuotaOverrides` | Sisa kuota diminta kontrak; riwayat pelampauan adalah jejak `MCU-PERM-v1` bagian 3 |
| Pesan `VAL-MCU-14` pada rentang | Satu tanggal | Diawali tanggal yang bermasalah bila rentang > 1 hari | Menunjuk tanggal yang harus dibenahi |
| Menutup tanggal berpeserta | Tidak disebut | `409` | Booking yang sudah ada akan kehilangan jadwal |
| `from` lampau, rentang > 92 hari, `AdditionalQuota` < 1 | Tidak disebut | `400` `MCU-INVALID-REQUEST` | Kebutuhan dasar `QBE-VAL-001` |
| `VAL-MCU-12` vs coba ulang | Yang kalah mendapat `VAL-MCU-12` | Yang kalah memuat ulang; berhasil bila kursi masih ada, `VAL-MCU-12` bila habis | Kalah balapan pada kursi ke-28 dari 30 tidak boleh ditolak |
| `VAL-MCU-15` | Berlaku pada booking/rekomendasi/jadwal ulang | Disediakan sebagai `CheckNotPast`; dipanggil service pemanggil | *Walk-in* selalu hari ini; aturan milik pemanggil |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Daily Schedule

Base URL: `api/v1/health-services/medical-checkup-management/daily-schedules`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Jadwal + sisa kuota + dokter jaga per tanggal | `McuSchedule : Read` | `?from&to` (bawaan hari ini + 29 hari; paling panjang 92 hari) | `List<McuDailyScheduleResponse>` | Tersedia |
| `PUT` | `/` | Atur hari buka/kuota dan dokter MCU jaga untuk satu rentang tanggal | `McuSchedule : Update` | `UpsertMcuDailyScheduleRangeRequest` (from, to, isOpen, quota, dutyMcuDoctorId?) | `List<McuDailyScheduleResponse>` | Tersedia |
| `POST` | `/{scheduleDate}/quota-overrides` | Setujui pelampauan kuota | `McuSchedule : ApproveQuotaOverride` | `CreateMcuQuotaOverrideRequest` (additionalQuota, reason) | `McuDailyScheduleResponse` | Tersedia |

Kode status: `200`, `400` (`VAL-MCU-13` / `MCU-INVALID-REQUEST`), `403`, `409` (`VAL-MCU-10`, `14`,
`MCU-CONCURRENCY`). `scheduleDate` berbentuk `yyyy-MM-dd`. Aksi tulis dicatat logger: rentang, buka/tutup,
kuota; pelampauan: tanggal, jumlah, kapasitas baru; pelaku dari token.

**Metode untuk service lain (bukan endpoint):** `ReserveAsync(tanggal, pelaku)` → `409` `VAL-MCU-10`/`11`/`12`;
`ReleaseAsync(tanggal, pelaku)`; `CheckNotPast(tanggal)` → kalimat `VAL-MCU-15` atau `null`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests -p:RunAnalyzers=False` (ikut membangun proyek utama) | 95 lulus (79 lama + 16 baru), 0 error kompilasi | `PASS` | `Passed! - Failed: 0, Passed: 95` |
| Kriteria (1) | Kuota 25 saat terpakai 26 → `409` `VAL-MCU-14`, pesan persis; kuota tetap 30; kuota 26 diterima | `PASS` | `Kriteria_1_…` |
| Kriteria (1), rentang | Pesan diawali tanggal; tidak satu hari pun tersimpan | `PASS` | `Rentang_menyebut_…` |
| Kriteria (2) + `AT-MCU-04`/`06` | Ke-31 `VAL-MCU-11` (30/30); tanpa alasan `VAL-MCU-13`; pelampauan 5 tercatat dengan alasan dan pelaku; ke-31..35 diterima; ke-36 ditolak (35/35) | `PASS` | `AT_MCU_04_dan_06_…` |
| Kriteria (2), bagian izin | Action `ApproveQuotaOverride` terpisah dari `Update`, terdaftar dan ditegakkan | `PASS` | `McuScheduleControllerContractTests` |
| Kriteria (3) / `AT-MCU-05` | Dua konteks, sisa 1 kursi: tepat satu berhasil, yang kalah `VAL-MCU-12`; `UsedQuota` = kuota = 30 | `PASS` | `Kriteria_3_…` |
| Kalah balapan, kursi masih ada | Coba ulang berhasil; terpakai 30 (tanpa token `Version` hasilnya 29) | `PASS` | `Yang_kalah_balapan_…` |
| Ikut transaksi pemanggil | Pemanggil membatalkan transaksi → terpakai kembali 0 | `PASS` | `Pemakaian_kuota_ikut_dibatalkan_…` |
| Kriteria (4) | Rentang 3 hari menyimpan dr. Rina; `GET` mengembalikan namanya, tanggal yang belum diatur tetap muncul; atur ulang tanpa dokter mengosongkannya | `PASS` | `Kriteria_4_…` |
| `VAL-MCU-10`, lepas kuota, dokter tidak dikenal, tanggal/rentang tidak sah, `CheckNotPast` | Sesuai §2 | `PASS` | Uji sisanya |
| Grup Swagger, route, 3 endpoint, `PermissionRegistryValidator` | Persis kontrak; nol temuan | `PASS` | Uji kontrak |
| Tanpa `<summary>` action | 0 baris `///` di tingkat action | `PASS` | Pencarian berkas |
| Checker QBE `-Path` (5 berkas + kontrol) | Berkas task nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Penguncian baris PostgreSQL di bawah permintaan serentak sungguhan | Token `Version` terbukti di SQLite; perilaku `UPDATE … WHERE "Version"` serentak di PostgreSQL tidak dijalankan | `NOT RUN` | Butuh basis data dengan migration MCU |
| `403` lewat HTTP; Swagger berjalan | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU (`BE-MCU-004` §7) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kuota baru di bawah terpakai → `409` *"Sudah ada 26 peserta terjadwal; kuota tidak boleh di bawah 26."* | Terpenuhi | §5 |
| (2) Pelampauan oleh kepala unit beralasan tercatat; tanpa `ApproveQuotaOverride` → `403` | Terpenuhi — pencatatan di harness; `403` lewat registrasi action izin (bukan panggilan HTTP) | §5 |
| (3) Dua pemakaian kuota bersamaan atas sisa terakhir → tepat satu berhasil (tingkat service) | Terpenuhi | §5 |
| (4) `PUT` rentang menyimpan `dutyMcuDoctorId`; `GET` mengembalikan nama dokter jaga | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | Atomik sungguhan milik PostgreSQL dicatat apa adanya (§5, `NOT RUN`) |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `ReserveAsync` menyimpan konteks saat dipanggil, sehingga perubahan lain yang sudah ditambahkan pemanggil ikut tersimpan pada saat itu — tetap di dalam transaksi pemanggil. Service booking (`BE-MCU-011`) wajib membuka transaksi sebelum memanggilnya |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Balapan di PostgreSQL sungguhan belum diuji. Startup backend ke devYoga masih menunggu migration `BE-MCU-002`/`003` |
| Perubahan sampingan | Kontrol negatif QBE dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` |
| Status Git | Perubahan task: ` M Program.cs`; `??` tiga berkas baru di folder modul MCU; perubahan `McuOperationResult.cs` (berkas `BE-MCU-005`, belum di-commit); laporan ini; tanda roadmap/traceability. Uji baru di `Tests/` diabaikan `.gitignore` (perlu `git add -f`) |
| Langkah berikutnya | `BE-MCU-010` (Daftar Pasien MCU dan detail order) dan `BE-MCU-020` siap. `BE-MCU-011` (booking) menunggu `BE-MCU-010` saja. `FE-MCU-005` dapat memakai grup ini |
