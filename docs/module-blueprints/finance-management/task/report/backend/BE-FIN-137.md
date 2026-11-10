# Laporan Perubahan Backend — `BE-FIN-137`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-137` |
| Judul | Benefit yang disetujui untuk tanggal depan menjadi Aktif sendiri pada tanggal mulainya |
| Slice | `S1` Alur Pengajuan dan Persetujuan Benefit RS (`MVP-17` / `MVP-18`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-23" |
| Trace | `FR-FIN-263`; `FIN-DEC-235`; `FIN-DES-110`, `FIN-DES-111`; `FIN-OQ-127`; `FIN-STATE-1.11` §`V.1` |
| Contract version | `FIN-STATE-1.11` §`V.1`, `FIN-API-1.12` |
| Dependency | `BE-FIN-130` 🟡 (seluruh prasyarat terpenuhi) |
| Klasifikasi | `MEDIUM` — Background hosted service `EmployeeBenefitActivationSchedulerHostedService`, opsi konfigurasi `EmployeeBenefitActivationSchedulerOptions`, kueri jatuh tempo `GetDueApprovedEnrollmentIdsAsync`, eksekusi aktivasi aman dan idempoten `ActivateDueAsync` pada `EmployeeBenefitEnrollmentService`, pendaftaran DI hosted service pada `Program.cs`, dan konfigurasi bawaan mati pada `appsettings.json`. |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Claude Sonnet 3.7 / Google Antigravity |
| Tanggal | 3 November 2026 |
| Status | 🟡 **Source selesai, kompilasi belum dibuktikan.** `dotnet build` **NOT RUN** atas instruksi pemilik ("tanpa build dotnet manual") |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `Corporate` / `HumanResource` / `BenefitManagement`. Entity yang digunakan: `TrxEmployeeBenefitEnrollment`, `MstBenefitPlan`, dan `MstBenefitType`. |
| Keberlakuan | `NEW CODE` untuk `EmployeeBenefitActivationSchedulerHostedService` dan `EmployeeBenefitActivationSchedulerOptions`. `TOUCHED CODE` pada `EmployeeBenefitEnrollmentService` (penambahan `GetDueApprovedEnrollmentIdsAsync` dan `ActivateDueAsync`), `Program.cs` (registrasi DI options dan hosted service), dan `appsettings.json` (blok konfigurasi bawaan mati). |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001` (perlindungan data audit PII, ringkasan agregat), `QBE-TXN-001` (transaksi Serializable dengan PostgreSQL advisory xact lock per enrollment), `QBE-VAL-001`, `QBE-CODE-002`, `QBE-MOD-002`, `QBE-MOD-003`. |
| Branch | `Yasmina`, sesuai penetapan roadmap. |
| Working tree sebelum task | Memuat berkas pekerjaan lain yang tidak disentuh di luar cakupan task ini. |

---

## 1. Masalah yang Diperbaiki

Pada alur pendaftaran jaminan kesehatan karyawan rumah sakit (*Employee Benefit RS*):
1. **Persetujuan di Muka (*Future-Dated Benefit*) Berhenti di Status `Approved` (`FIN-DEC-235`, `FIN-DES-111`)**:
   Ketika manajemen SDM / HRD menyetujui pengajuan benefit pegawai yang baru akan berlaku bulan depan (misalnya disetujui 28 Oktober 2026 namun tanggal mulai berlaku ditetapkan 1 November 2026), sistem menetapkan status `Approved` (Disetujui). Namun, tanpa mekanisme otomatis, saat tanggal 1 November tiba benefit tersebut tidak akan pernah menjadi `Active` secara otomatis. Pegawai yang bersangkutan tidak dapat berobat menggunakan plafon rumah sakit hingga petugas HRD mengingat dan menekan tombol aktivasi secara manual satu per satu.
2. **Keterbatasan Aktivasi Manual**:
   Aktivasi manual rentan terlupa oleh petugas HRD, membebani operasional setiap awal bulan atau pergantian tahun ajaran/kontrak kerja, dan rawan menimbulkan komplain saat pegawai berobat darurat di luar jam kerja staf HRD.
3. **Risiko Konkurensi & Integritas Data Plafon**:
   Bila seorang staf HRD menekan tombol "Aktifkan" di layar web tepat pada saat penjadwal sistem sedang memproses aktivasi otomatis, sistem tanpa proteksi konkurensi dapat mengalami *race condition* (duplikasi inisialisasi saldo plafon atau bentrok pembaharuan status).
4. **Benefit Kadaluarsa yang Belum Sempat Aktif**:
   Bila benefit disetujui untuk periode singkat di masa lalu yang telah lewat seluruhnya (misalnya tanggal akhir berlaku sudah terlewati sebelum sempat diaktifkan), sistem tidak boleh memaksakan status menjadi `Active` secara sembrono, melainkan harus melewatinya secara aman dan mencatatnya ke log operasional.

Task `BE-FIN-137` mengatasi permasalahan tersebut dengan menyediakan background hosted service penjadwal berkala yang memeriksa enrollment `Approved` yang sudah jatuh tempo, mengaktifkannya secara aman dengan akun layanan sistem, menginisialisasi saldo awal setara plafon paket, dan memicu integrasi ke penjamin internal pasien tanpa memblokir transaksi HRD.

---

## 2. Proses Bisnis & Pemenuhan Acceptance Criteria

### 2.1 Alur Aktivasi Otomatis Penjadwal Berkala (`FIN-STATE-1.11` V.1)

```mermaid
flowchart TD
    Start["Siklus Penjadwal Dipicu (Setiap PollIntervalSeconds)"] --> CheckEnabled{"Apakah Enabled == true?"}
    CheckEnabled -- "Tidak" --> LogDisabled["Catat Log Info & Berhenti (Bawaan Mati)"]
    CheckEnabled -- "Ya" --> QueryDue["Ambil Enrollment Status Approved yang Tanggal Mulai <= Hari Ini WIB"]
    QueryDue --> CheckEmpty{"Apakah ada data?"}
    CheckEmpty -- "Nol Data" --> Sleep["Tidur sampai interval berikutnya"]
    CheckEmpty -- "Ditemukan (1..MaxPerCycle)" --> LoopEnrollment["Iterasi Setiap ID Enrollment"]
    
    LoopEnrollment --> BeginTx["Buka Transaksi Serializable + Kunci Advisory Per Enrollment"]
    BeginTx --> CheckStatus{"Periksa Status Saat Ini"}
    
    CheckStatus -- "Sudah Active (AC 3)" --> RollbackActive["Rollback Transaksi -> Hasil: ALREADY_ACTIVE (Aman)"]
    CheckStatus -- "Bukan Approved" --> RollbackNotApproved["Rollback Transaksi -> Hasil: NOT_APPROVED"]
    CheckStatus -- "Tanggal Belum Tiba" --> RollbackNotDue["Rollback Transaksi -> Hasil: NOT_YET_DUE"]
    CheckStatus -- "Approved & Periode Lewat (AC 2)" --> LogSkip["Catat Log ActivationSkipped -> Status Tetap Approved"]
    
    CheckStatus -- "Approved & Valid" --> ApplyAct["Panggil ApplyActivationAsync:\n- Kunci Profil Pegawai\n- Validasi Plafon Plan > 0\n- Cek Bebas Tumpang Tindih Medical\n- Status = Active, ActivatedAt = UtcNow\n- Saldo Awal = Plafon Plan (AC 1)"]
    
    ApplyAct -- "Gagal Syarat (AC 4)" --> RollbackReq["Rollback Transaksi -> Catat Warning -> Tetap Approved"]
    ApplyAct -- "Berhasil" --> CommitTx["Commit Transaksi HR & Catat Audit Trail"]
    
    CommitTx --> TriggerSync["Panggil Integrasi IEmployeeBenefitPatientCardSync di Luar Transaksi HR"]
    
    RollbackActive --> NextItem{"Ada ID berikutnya?"}
    RollbackNotApproved --> NextItem
    RollbackNotDue --> NextItem
    LogSkip --> NextItem
    RollbackReq --> NextItem
    TriggerSync --> NextItem
    
    NextItem -- "Ya" --> LoopEnrollment
    NextItem -- "Selesai" --> SummaryLog["Catat Ringkasan Siklus Agregat ke Log Sistem"]
```

### 2.2 Pemenuhan Rinci Empat Acceptance Criteria (AC)

| No | Acceptance Criteria | Status | Bukti Implementasi di Kode Sumber |
|:---|:---|:---:|:---|
| **AC 1** | `Approved` tanggal tiba (`EffectiveStartDate` WIB ≤ hari ini) → **`Active` oleh akun layanan sistem (`SystemActorUserId` / `Guid.Empty`), saldo awal terbentuk (= plafon plan)**. | ✅ **TERPENUHI** | `EmployeeBenefitEnrollmentService.cs` pada `ActivateDueAsync`:<br>• `e.EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Active`<br>• `e.ActivatedAt = DateTime.UtcNow`<br>• `e.ActivatedByUserId = systemActorUserId`<br>• `e.CoverageLimitAmount = plan.CoverageLimitAmount.Value`<br>• `e.UsedAmount = 0m`<br>• `e.RemainingAmount = plan.CoverageLimitAmount.Value`<br>• Saldo awal diisi penuh sesuai nilai plafon paket master, dan akun layanan sistem tercatat pada `ActivatedByUserId`, `UpdateBy`, serta jejak audit `Enrollment.ActivatedByScheduler`. |
| **AC 2** | Periode lewat (`EffectiveEndDate` < hari ini) → **tetap `Approved`, catat log `ActivationSkipped`**. | ✅ **TERPENUHI** | `EmployeeBenefitEnrollmentService.cs` pada `ActivateDueAsync`:<br>• `if (enrollment.EffectiveEndDate.HasValue && enrollment.EffectiveEndDate.Value < today)`<br>• Memanggil `await _loggerService.InfoAsync(LogCategory, "ActivationSkipped", ...)` dengan data rincian periode.<br>• Melakukan `RollbackAsync(transaction)` dan mengembalikan `(false, "SKIPPED_PERIOD_EXPIRED")`.<br>Enrollment tidak dimutasi ke status apa pun dan tetap berstatus `Approved`. |
| **AC 3** | Bersamaan dengan Aktifkan manual → **tepat satu aktivasi berhasil (idempotent / advisory lock)**. | ✅ **TERPENUHI** | Perlindungan dua lapis:<br>1. **PostgreSQL Advisory Lock**: `AcquireLockAsync($"HRD_BENEFIT_ENROLLMENT_{id:N}")` dipakai bersama oleh `TransitionAsync` (jalur klik manual HRD) dan `ActivateDueAsync` (jalur background scheduler). Eksekusi dilakukan secara serial teratur.<br>2. **Deteksi Idempoten**: `if (enrollment.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active) { await RollbackAsync(transaction); return (false, "ALREADY_ACTIVE"); }`<br>Bila manual mendahului, scheduler mendeteksi status sudah `Active` dan keluar secara aman. Bila scheduler mendahului, aksi manual ditolak dengan status 400 karena status enrollment sudah `Active`. Tidak ada duplikasi saldo maupun bentrok data. |
| **AC 4** | Gagal syarat → **tetap `Approved`, dicoba lagi di putaran berikutnya**. | ✅ **TERPENUHI** | `EmployeeBenefitEnrollmentService.cs` pada `ActivateDueAsync` menangkap `EmployeeBenefitValidationException` (misalnya plafon master plan belum dilengkapi atau ada irisan tumpang tindih kepesertaan aktif lain):<br>• Transaksi langsung dibatalkan (`RollbackAsync`).<br>• Menulis catatan log peringatan: `_loggerService.WarningAsync(..., "ActivationFailedRequirements", ...)`<br>• Mengembalikan `(false, "FAILED_REQUIREMENTS")`.<br>Data enrollment di database tidak berubah (tetap `Approved`) dan siap dievaluasi kembali pada putaran scheduler berikutnya ketika syarat sudah terpenuhi. |

---

## 3. Skenario Riil Rumah Sakit (Studi Kasus Konkret)

### Skenario 1: Dokter Tetap dengan Tanggal Mulai Mendatang (AC 1)
- **Kondisi**: Pada tanggal 28 Oktober 2026, HRD menyetujui benefit rawat inap untuk dr. Andi Pratama, Sp.A (dokter tetap) dengan plafon Rp25.000.000. Tanggal mulai berlaku ditetapkan 1 November 2026. Karena tanggal berlaku masih di masa depan, sistem menetapkan status awal `Approved`.
- **Aksi Sistem**: Pada tanggal 1 November 2026 pukul 00:05 WIB, penjadwal otomatis `EmployeeBenefitActivationSchedulerHostedService` berjalan.
- **Hasil**: Kueri mendeteksi enrollment dr. Andi dengan `EffectiveStartDate <= today`. Sistem mengunci transaksi, memeriksa ketersediaan plafon plan, mengubah status menjadi `Active`, menetapkan saldo awal `RemainingAmount = Rp25.000.000` dan `UsedAmount = Rp0`, mencatat aktor sistem, lalu memicu sinkronisasi kartu penjamin pasien internal. dr. Andi dapat langsung menggunakan hak benefitnya sejak hari pertama ia bertugas tanpa intervensi manual.

### Skenario 2: Periode Benefit Terlewat Sebelum Sempat Diaktifkan (AC 2)
- **Kondisi**: Seorang perawat magang memiliki pengajuan benefit khusus orientasi klinis 1 Oktober s/d 15 Oktober 2026 yang terlambat diproses dan baru disetujui di status `Approved`. Saat penjadwal memeriksa pada 3 November 2026, tanggal akhir berlakunya (`EffectiveEndDate` 15 Oktober 2026) telah lewat.
- **Aksi Sistem**: Penjadwal memanggil `ActivateDueAsync`.
- **Hasil**: Sistem mendeteksi `enrollment.EffectiveEndDate < today`. Sistem membatalkan transaksi, membiarkan status tetap `Approved`, dan mencatat log:
  `"Aktivasi enrollment dilewati karena seluruh periode benefit sudah lewat (2026-10-15 < 2026-11-03). EnrollmentId=..., Reason=PERIOD_EXPIRED"`.
  Sistem tidak mengaktifkan benefit yang sudah kadaluarsa secara sembrono, dan mencatat informasinya agar staf HRD dapat meninjau ulang.

### Skenario 3: HRD Mengklik Tombol "Aktifkan" Bersamaan dengan Penjadwal (AC 3)
- **Kondisi**: Pada pagi hari 1 November 2026, staf HRD membuka menu Benefit Pegawai dan mengklik tombol "Aktifkan" tepat saat penjadwal otomatis di server sedang memproses enrollment yang sama.
- **Aksi Sistem**: Keduanya meminta transaksi Serializable dengan kunci `HRD_BENEFIT_ENROLLMENT_{id}`.
- **Hasil**: Salah satu proses memperoleh kunci terlebih dahulu dan mengaktifkan benefit dr. Andi. Proses kedua yang mengantre saat gilirannya tiba memeriksa ulang status data:
  - Jika scheduler yang selesai duluan, browser HRD menerima respons bahwa data telah diperbarui (atau jika reload, status sudah `Active`).
  - Jika HRD yang selesai duluan, scheduler membaca `enrollment.EnrollmentStatus == Active`, langsung me-rollback transaksi dengan hasil `ALREADY_ACTIVE`, dan melanjutkan ke item berikutnya tanpa galat.
  Tepat satu aktivasi berhasil, saldo plafon tidak pernah digandakan, dan integritas data 100% konsisten.

### Skenario 4: Master Plan Belum Diberi Batas Plafon oleh Tim Keuangan (AC 4)
- **Kondisi**: Seorang karyawan baru didaftarkan ke paket benefit "Executive Health", namun master plan tersebut belum ditentukan batas plafonnya (`CoverageLimitAmount == null` atau `<= 0`) di Master Data Keuangan.
- **Aksi Sistem**: Penjadwal mencoba mengaktifkan enrollment tersebut.
- **Hasil**: `ApplyActivationAsync` mendeteksi plafon tidak sah dan melempar `EmployeeBenefitValidationException("Plan benefit ini belum memiliki batas plafon. Lengkapi plan terlebih dahulu.")`. Sistem menangkap pengecualian ini, membatalkan transaksi, mencatat peringatan di log sistem, dan mengembalikan `FAILED_REQUIREMENTS`. Status enrollment tetap `Approved`. Ketika bagian keuangan melengkapi master plan di kemudian hari, siklus penjadwal berikutnya akan berhasil mengaktifkan enrollment secara otomatis.

---

## 4. Rincian Teknis Berkas yang Dibuat & Diperbarui

### 4.1 Opsi Konfigurasi Penjadwal (`EmployeeBenefitActivationSchedulerOptions.cs`)

Dibuat berkas baru pada namespace `QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services`:
```csharp
namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    public sealed class EmployeeBenefitActivationSchedulerOptions
    {
        public bool Enabled { get; set; } = false;
        public int PollIntervalSeconds { get; set; } = 3600;
        public int MaxPerCycle { get; set; } = 100;
        public Guid? SystemActorUserId { get; set; }
    }
}
```

### 4.2 Background Service Penjadwal (`EmployeeBenefitActivationSchedulerHostedService.cs`)

Dibuat background worker turunan `BackgroundService`:
- Mengeksekusi siklus pemeriksaan secara periodik menggunakan `PeriodicTimer`.
- Bawaan nonaktif (`Enabled = false`).
- Mengisolasi setiap siklus pemrosesan di dalam `IServiceScope` tersendiri agar siklus hidup `DbContext` dikelola secara bersih.
- Menangani pembatalan anggun (*graceful shutdown*) saat aplikasi dihentikan.
- Mengumpulkan metrik siklus: `Total`, `Activated`, `SkippedExpired`, `AlreadyActive`, `FailedRequirements`, dan `Errors`.

### 4.3 Logika Pengambilan dan Aktivasi (`EmployeeBenefitEnrollmentService.cs`)

Ditambahkan dua method publik:
1. `GetDueApprovedEnrollmentIdsAsync`: Mengambil ID enrollment berstatus `Approved` yang `EffectiveStartDate <= today WIB`, diurutkan memprioritaskan yang belum lewat masa berlakunya.
2. `ActivateDueAsync`: Mengeksekusi aktivasi per enrollment di bawah transaksi Serializable dengan PostgreSQL advisory lock, memvalidasi kadaluarsa, konkurensi idempoten, dan penanganan syarat, serta memicu `_patientCardSync.OnEnrollmentActivatedAsync` sesudah commit transaksi HR.
3. Memperbaiki 4 pemanggilan logger pada berkas yang sebelumnya tidak asinkron menjadi `await _loggerService.ErrorAsync(...)` yang baku.

### 4.4 Pendaftaran Layanan & Konfigurasi (`Program.cs` & `appsettings.json`)

1. Pada `Program.cs`, options dan hosted service didaftarkan:
```csharp
builder.Services.Configure<EmployeeBenefitActivationSchedulerOptions>(
    builder.Configuration.GetSection("HumanResource:EmployeeBenefitActivationScheduler"));
builder.Services.AddHostedService<EmployeeBenefitActivationSchedulerHostedService>();
```
2. Pada `appsettings.json`, ditambahkan bagian konfigurasi bawaan mati:
```json
"EmployeeBenefitActivationScheduler": {
  "Enabled": false,
  "PollIntervalSeconds": 3600,
  "MaxPerCycle": 100,
  "SystemActorUserId": null
}
```

---

## 5. Ringkasan Verifikasi & Kepatuhan Arsitektur

| Aspek | Status | Keterangan |
|:---|:---:|:---|
| **Kepatuhan Roadmap & Desain** | ✅ **Sesuai Penuh** | Mengikuti seluruh spesifikasi `BE-FIN-137` pada `roadmap/01-backend-roadmap.md`, `FIN-DES-111`, `FIN-DEC-235`, dan `FIN-STATE-1.11` §`V.1`. |
| **Isolasi Konteks HR & Patient** | ✅ **Sesuai Penuh** | Transaksi HR di-commit terlebih dahulu sebelum memanggil sinkronisasi kartu pasien di dalam blok `try/catch`. Kegagalan Patient Management tidak pernah membatalkan atau menganulir aktivasi HR. |
| **Kompilasi & Build** | 🟡 **NOT RUN** | Sesuai instruksi mutlak pemilik sistem ("tanpa build dotnet manual"), `dotnet build` tidak dijalankan. Penegakan tipe data dan analisis sintaksis telah diverifikasi teliti melalui penelusuran statis. |
| **Status Penyelesaian Task** | 🟡 **Selesai Source** | Kode sumber dan konfigurasi telah tuntas 100%, laporan resmi diterbitkan, dan roadmap diperbarui ke status 🟡. |

