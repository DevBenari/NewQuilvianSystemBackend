# Laporan Penyelesaian Task: BE-HRD-209 Scheduler Radar Pemantau SIP 5 Tahun & Fail-Closed Action H-0

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-209`
* **Nama Task**: Pembangunan Background Worker Scheduler Radar Pemantauan Masa Berlaku SIP 5 Tahun Bertingkat (H-90 s.d. H-7) & Eksekusi Aksi Fail-Closed H-0
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Workers/LicenseExpiryMonitoringWorker.cs`
  - `NewQuilvianSystemBackend/Program.cs` (registrasi `AddHostedService`)

---

## 2. Latar Belakang & Kebutuhan Regulasi Keselamatan Pasien
Dalam regulasi kedokteran dan hukum perumahsakitan Indonesia, seorang dokter atau tenaga kesehatan yang berpraktik dengan Surat Izin Praktik (SIP) yang telah habis masa berlakunya dikategorikan sebagai **praktik tanpa izin sah (*unauthorized medical practice*)**, yang dapat berakibat pada sanksi pidana dan penolakan klaim BPJS Kesehatan:
1. **Radar Pemantauan Bertingkat 5-Tier (`CRD-B2`)**: Untuk mencegah keterlambatan perpanjangan SIP, sistem menjalankan pemantauan berjenjang:
   - **Tier H-90 (61 - 90 hari)**: Peringatan Awal ESS (*Employee Self-Service*). Notifikasi mandiri dikirimkan ke nakes untuk mempersiapkan kecukupan Satuan Kredit Profesi (SKP) di platform Plataran Sehat / SatuSehat SDMK.
   - **Tier H-60 (31 - 60 hari)**: Peringatan Tingkat Unit. Notifikasi diteruskan ke Kepala Ruang Rawat / Kepala Instalasi untuk memantau pengajuan berkas perpanjangan ke Dinas Kesehatan / DPMPTSP.
   - **Tier H-30 (8 - 30 hari)**: Peringatan Kritis Komite. Berkas perpanjangan harus sudah masuk ke meja Sub-Komite Kredensial untuk penjadwalan sidang pleno perpanjangan SPK-RKK.
   - **Tier H-7 (1 - 7 hari)**: Peringatan Darurat Direktur. Eskalasi langsung ke Direktur Pelayanan Medik & HR Director bahwa masa berlaku nakes tersisa hitungan hari.
   - **Tier Expired H-0 (0 hari / Lewat)**: Tindakan **Fail-Closed Otomatis**.
2. **Aksi Fail-Closed Otomatis H-0 (`INT-HR-69`, `INT-HR-71`)**:
   - Sistem secara instan memblokir hak nakes dalam membuat order klinis, resep obat, atau instruksi medis di Electronic Medical Record (EMR) SIMRS.
   - Sistem memblokir penugasan jadwal nakes di roster dinas Sub-06 Scheduling sehingga kepala ruangan tidak dapat menugaskan nakes tersebut pada shift pelayanan aktif.

---

## 3. Skenario Riil Siklus Kerja Worker

### Skenario Operasional Harian Background Worker
1. Background worker `LicenseExpiryMonitoringWorker` aktif secara otomatis setiap 6 jam di background ASP.NET Core runtime.
2. Worker memindai seluruh lisensi aktif non-seumur-hidup di tabel `WfpCredentialLicense`.
3. Worker membandingkan `ExpiredDate` terhadap tanggal hari ini (`DateTime.UtcNow.Date`).
4. Jika masa berlaku SIP dokter bedah spesialis tersisa 25 hari, worker memperbarui kolom `ExpiryRadarStatus` menjadi `H30`.
5. Tepat pada pergantian hari di mana tanggal kedaluwarsa telah terlewati (`daysRemaining <= 0`), worker mengubah status menjadi `ExpiredH0`, mengaktifkan flag `BlocksClinicalServiceWhenInvalid = true` dan `BlocksSchedulingWhenInvalid = true`.
6. Worker memanggil `ExecuteFailClosedForExpiredLicensesAsync`:
   - Menangguhkan kewenangan klinis aktif nakes (`ClinicalPrivilegeStatus.Suspended`).
   - Mencatat log audit peringatan sistem ke file dan event log rumah sakit.

---

## 4. Karakteristik Teknis Worker

| Karakteristik | Implementasi |
| :--- | :--- |
| **Base Class** | `Microsoft.Extensions.Hosting.BackgroundService` |
| **Siklus Eksekusi** | Interval periodik setiap 6 jam dengan delay inisiasi aplikasi 15 detik |
| **Scope Management** | `IServiceScopeFactory.CreateScope()` terisolasi untuk injeksi `ApplicationDbContext` dan `ICredentialVerificationService` |
| **Graceful Shutdown** | Memantau sinyal pembatalan `CancellationToken stoppingToken` saat aplikasi me-restart atau maintenance tanpa memotong transaksi database |
| **Ketahanan Kesalahan** | Penanganan eksepsi `try-catch` terpusat sehingga kegagalan satu siklus tidak mematikan service utama |

---

## 5. Verifikasi Kompilasi & Kesiapan Produksi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Pendaftaran Program.cs**: Telah terdaftar secara resmi sebagai Hosted Service menggunakan `builder.Services.AddHostedService<LicenseExpiryMonitoringWorker>()`.
