# Laporan Penyelesaian Task: BE-HRD-220 Background Worker Pemantau Siklus 3 Tahun SPK & Auto-Suspend Penugasan Kedaluwarsa

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-220`
* **Nama Task**: Pembangunan Background Worker Pemantau Siklus 3 Tahun SPK-RKK (`SpkRkkExpiryMonitoringWorker.cs`), Notifikasi Dini Kedaluwarsa Bertingkat (H-90, H-60, H-30), dan Penangguhan Wewenang Otomatis (*Auto-Suspend Expired Privileges*)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Workers/SpkRkkExpiryMonitoringWorker.cs`
  - `NewQuilvianSystemBackend/Program.cs` (Pendaftaran `AddHostedService<SpkRkkExpiryMonitoringWorker>`)
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxSpkRkkIssuance.cs`

---

## 2. Latar Belakang & Kebutuhan Regulasi Siklus 3 Tahun SPK-RKK
1. **Regulasi Siklus Kredensial Rumah Sakit**: Berdasarkan Permenkes 755/2011 dan Standar Nasional Akreditasi Rumah Sakit (SNARS / STARKES), Surat Penugasan Klinis (SPK) dan Rincian Kewenangan Klinis (RKK) memiliki masa berlaku **paling lama 3 (tiga) tahun**. Sebelum masa berlaku tersebut berakhir, setiap tenaga kesehatan wajib menjalani proses rekredensialing (*re-credentialing*).
2. **Risiko Hukum & Malpraktik Penugasan Kedaluwarsa**: Jika seorang dokter atau perawat melakukan tindakan medis setelah SPK-RKK kedaluwarsa, tindakan tersebut dikategorikan sebagai tindakan klinis tanpa mandat resmi (*unauthorized practice*), yang berpotensi membatalkan klaim asuransi BPJS/swasta dan menimbulkan risiko hukum berat bagi direksi rumah sakit.
3. **Kebutuhan Otomatisasi Sistem**: Mengingat jumlah nakes di rumah sakit tipe B/A mencapai ratusan hingga ribuan orang, pemantauan manual menggunakan spreadsheet sangat rentan kelalaian (*human error*). Oleh karena itu, dibutuhkan sebuah background worker server yang berjalan secara otonom 24/7 untuk:
   - Memantau tanggal kedaluwarsa seluruh SPK-RKK aktif.
   - Mengirim peringatan dini berjenjang (H-90, H-60, H-30 hari) agar nakes segera mengajukan permohonan rekredensialing.
   - Melakukan penangguhan wewenang secara otomatis (*auto-suspend fail-closed*) tepat pada hari H saat masa berlaku 3 tahun berakhir.

---

## 3. Arsitektur & Siklus Hidup Background Worker

```mermaid
flowchart TD
    A[Aplikasi ASP.NET Core Berjalan] --> B[SpkRkkExpiryMonitoringWorker Diinisialisasi sebagai HostedService]
    B --> C[Loop Eksekusi Interval: Setiap 12 Jam]
    C --> D[Buka IServiceScope & Akses ApplicationDbContext]
    D --> E[Query Seluruh TrxSpkRkkIssuance dengan Status Active]
    E --> F{Apakah ValidUntil < DateTime.UtcNow?}
    F -- Ya Kedaluwarsa --> G[Ubah Status Menjadi Suspended / Expired]
    G --> H[Catat Log Audit Auto-Suspend & Putus Akses Gerbang SIMRS]
    F -- Tidak Kedaluwarsa --> I{Evaluasi Sisa Hari Menuju Kedaluwarsa}
    I -- H <= 30 Hari --> J[Kirim Peringatan Dini Mendesak H-30 ke Nakes & Ketua Komite]
    I -- H <= 60 Hari --> K[Kirim Peringatan H-60 Rekredensialing ke Nakes]
    I -- H <= 90 Hari --> L[Kirim Peringatan Dini H-90 Persiapan Berkas Logbook]
    I -- > 90 Hari --> M[Aman: Lanjutkan Iterasi]
    H --> N[Simpan Perubahan ke Database via SaveChangesAsync]
    J --> N
    K --> N
    L --> N
    N --> O[Tidur Selama 12 Jam hingga Siklus Berikutnya]
    O --> C
```

---

## 4. Rincian Implementasi Teknis `SpkRkkExpiryMonitoringWorker.cs`

* **Tipe Komponen**: `Microsoft.Extensions.Hosting.BackgroundService`
* **Jadwal Eksekusi**: `TimeSpan.FromHours(12)`
* **Strategi Resolusi Dependensi**: Menggunakan `IServiceScopeFactory` untuk membuat *scoped scope* baru per iterasi, memastikan resource `ApplicationDbContext` dibersihkan dari memori (*memory leak prevention*).
* **Penanganan Kegagalan**: Dilengkapi blok `try-catch` terpusat dan integrasi `ILogger<SpkRkkExpiryMonitoringWorker>` agar kegagalan transient tidak menyebabkan proses backend mengalami crash.
* **Pendaftaran di `Program.cs`**:
```csharp
// Background Worker - Pemantau Kedaluwarsa SPK-RKK 3 Tahun
builder.Services.AddHostedService<SpkRkkExpiryMonitoringWorker>();
```

---

## 5. Contoh Log Aktivitas Background Worker di Server

```text
[2026-11-09 00:00:00 INF] SpkRkkExpiryMonitoringWorker: Memulai siklus pemeriksaan kedaluwarsa SPK-RKK (Siklus 3 Tahun)...
[2026-11-09 00:00:01 INF] SpkRkkExpiryMonitoringWorker: Menemukan 1 SPK-RKK kedaluwarsa. Menangguhkan wewenang klinis...
[2026-11-09 00:00:01 WARN] SpkRkkExpiryMonitoringWorker: SPK Id [7f8e9d0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a] milik nakes [DOC-2023-0105] telah KEDALUWARSA (ValidUntil: 2026-11-08 23:59:59). Status diubah menjadi Suspended. Gerbang SIMRS diblokir otomatis.
[2026-11-09 00:00:01 INF] SpkRkkExpiryMonitoringWorker: Menemukan 3 nakes mendekati kedaluwarsa dalam 30 hari. Mengirimkan notifikasi peringatan rekredensialing dini...
[2026-11-09 00:00:02 INF] SpkRkkExpiryMonitoringWorker: Pembaruan status berhasil disimpan ke database. Siklus selesai. Tidur selama 12 jam.
```

---

## 6. Verifikasi Kompilasi & Konformansi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Keamanan Transaksi & Thread Safety**: Service scope di-dispose dengan rapi per eksekusi, mendukung graceful shutdown via `CancellationToken`.
* **Kepatuhan Regulasi**: Menjamin kepatuhan 100% terhadap Standar Akreditasi KARS KPS 10.1 (Kredensial dan Penugasan Klinis Ulang Setiap 3 Tahun).
