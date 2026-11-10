# Laporan Penyelesaian Task: BE-HRD-314 Gateway Notifikasi Multikanal & Worker DailyApprovalDigestWorker (WF-W3) Pukul 07:00 Pagi (UnifiedNotificationGatewayService - WF-B7, INT-HR-96)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-314`
* **Nama Task**: Gateway Notifikasi Multikanal & Worker DailyApprovalDigestWorker (WF-W3) Pukul 07:00 Pagi (UnifiedNotificationGatewayService - WF-B7, INT-HR-96)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 21: Workflow Management & Unified Approval Inbox V2 Hardening
* **Slice Delivery**: `S-A7-V2` (Kontrak API `v24`, Gelombang `MVP-62`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `Implementasi UnifiedNotificationGatewayService untuk pengiriman notifikasi realtime (in-app, push, email) saat ada pengajuan tiket, pengembalian draf revisi, atau keputusan akhir. Dilengkapi background worker DailyApprovalDigestWorker (WF-W3) yang mengirimkan email rekap tiket pending setiap pukul 07:00 pagi.`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `WorkflowManagement` (Sub-modul 21: ESS & Unified Approval Inbox V2)
* **Prefix Registry**: `Hrd` / `Trx` / `Wfp` (Kepatuhan Kamus Data Bagian 24 & Konstitusi Repositori)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi backend sesuai kontrak terkunci `v24` dan aturan bisnis `VR-WF-01` s.d. `VR-WF-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam ekosistem rumah sakit modern, koordinasi perizinan kerja, pergeseran shift jaga nakes, klaim biaya, dan persetujuan operasional memiliki dampak langsung terhadap keselamatan pasien dan keberlanjutan layanan medis:
1. **Pencegahan Gangguan Operasional Layanan (WF-B1, VR-WF-08)**: Penentuan rantai persetujuan berjenjang secara dinamis multi-dimensi berdasarkan unit kerja pemohon, tingkat jabatan, dan ambang batas finansial untuk memastikan pengawasan manajerial yang tepat sasaran tanpa birokrasi manual.
2. **Integritas Tata Kelola & Anti Self-Approval Mutlak (WF-B2, HRD-DEC-183, VR-WF-02)**: Larangan mutlak bagi staf/pejabat struktural menyetujui permohonannya sendiri. Sistem secara otomatis mengalihkan tugas persetujuan (skip-level routing) ke atasan struktural di atasnya untuk menjaga independensi dan akuntabilitas.
3. **Fleksibilitas Operasional Plh Berbatas Waktu (WF-B3, HRD-DEC-184, VR-WF-03, VR-WF-09)**: Pendelegasian wewenang persetujuan kepada pejabat pengganti (Plh) berbatas waktu saat pejabat definitif berhalangan/cuti, dengan penandaan visual badge *Delegated* dan pencatatan jejak audit ganda (*dual audit logging*).
4. **Penegakan Disiplin SLA & Larangan Auto-Approve Mutlak (WF-B4, HRD-DEC-185, VR-WF-04)**: Rumah sakit memberlakukan kebijakan **Zero Auto-Approval Policy**. Permohonan yang habis batas waktunya dilarang keras disetujui otomatis oleh sistem karena berisiko fatal bagi keselamatan jadwal bangsal; sistem menandai tiket overdue dan mengeskalasikannya ke atasan lebih tinggi.
5. **Keputusan Tri-State & Perlindungan Hak Staf (WF-B5, HRD-DEC-186, VR-WF-05)**: Opsi otorisasi tri-state (*Approve*, *Reject* wajib alasan bermakna minimal 15 karakter, dan *Return for Revision* untuk perbaikan draf data dalam 7 hari tanpa membatalkan tiket).
6. **Persetujuan Massal Bersyarat Aman (WF-B6, HRD-DEC-187, VR-WF-07)**: Kemudahan bulk approval hanya diizinkan untuk permohonan berisiko rendah yang telah memenuhi **100% indikator validasi HIJAU** serta diverifikasi dengan otorisasi PIN keamanan.
7. **Sinkronisasi Transaksional Idempoten Domain Hilir (WF-B8, INT-HR-93, INT-HR-94)**: Penyelesaian tiket secara atomik dan idempoten memicu eksekusi pemotongan saldo cuti di Sub-08, penguncian roster dinas di Sub-06, pencatatan lembur di Sub-09, dan pencairan klaim di Sub-11.

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Implementasi UnifiedNotificationGatewayService untuk pengiriman notifikasi realtime (in-app, push, email) saat ada pengajuan tiket, pengembalian draf revisi, atau keputusan akhir. Dilengkapi background worker DailyApprovalDigestWorker (WF-W3) yang mengirimkan email rekap tiket pending setiap pukul 07:00 pagi.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 24, keputusan bisnis HRD-DEC-182 s.d. HRD-DEC-187, asumsi HRD-ASM-67 s.d. HRD-ASM-70, dan matriks validasi Sub-modul 21.
* **Kriteria Penerimaan (Acceptance Criteria)**: Seluruh method service, model, DTO, background worker, dan controller terimplementasi rapi, terisolasi dalam transaksi database ACID, dan lolos uji kompilasi serta conformance check.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS** (0 Violation).
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1r (`AT-HRD-WF-14, AT-HRD-WF-15`).

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (keputusan kanal notifikasi)

### Keputusan yang diterapkan

Pemilik meminta Gelombang 2 dikerjakan tuntas tanpa sesi keputusan terpisah, sehingga keputusan diambil dari bukti source dan dicatat di sini sebagai asumsi eksplisit. Platform belum memiliki tabel notifikasi, pengirim email, maupun gateway WhatsApp; yang ada hanya SignalR untuk antrean. Maka **kanal pertama adalah lonceng di aplikasi** dan **pemiliknya modul SDM** (prefix `Hrd`, folder `HrServiceManagement`), bukan platform bersama. Kanal email/push mobile/WhatsApp dicatat sebagai `RequestedChannel` untuk jejak, tanpa pengiriman.

### Masalah yang diperbaiki

Sebelumnya `UnifiedNotificationGatewayService.SendInstantNotificationAsync` hanya berisi `await Task.CompletedTask`, dan ringkasan harian dihitung tetapi tidak dikirim ke mana pun. Contoh: perawat mengajukan cuti, atasan tidak menerima apa pun dan baru tahu bila kebetulan membuka kotak masuk.

### Proses bisnis sesudah perubahan

1. Staf mengajukan permohonan (V2) → approver tahapan pertama menerima pemberitahuan "Permohonan baru menunggu persetujuan Anda" berisi nomor tiket dan batas SLA.
2. Approver menyetujui → pemohon menerima "Permohonan disetujui"; bila masih ada tahapan berikutnya, approver berikutnya diberi tahu.
3. Approver menolak atau mengembalikan untuk revisi → pemohon menerima alasan atau catatan revisi.
4. Setiap pukul 07:00 WIB worker `DailyApprovalDigestWorker` mengirim ringkasan jumlah tiket tertunda (dan yang mendesak) ke setiap approver, satu kali per hari (kunci anti-duplikat per approver per tanggal WIB).
5. Pegawai membuka lonceng: `GET /api/v1/self-service/hr/notifications` (daftar), `GET .../unread-count` (lencana), `POST .../{id}/read` dan `POST .../read-all` (tandai dibaca). Bila browser terhubung ke `/hubs/notifications`, pemberitahuan baru muncul tanpa memuat ulang.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/HrServiceManagement/Models/HrdNotification.cs` + konfigurasi EF | Entitas pemberitahuan per penerima; index unik `DedupKey` (terfilter), index penerima/status |
| `.../HrServiceManagement/Services/IHrdNotificationService.cs`, `HrdNotificationService.cs` | Kirim ke akun, profil tenaga kerja, karyawan, atau pemegang hak akses; daftar, hitung belum dibaca, tandai dibaca; dorongan SignalR |
| `Hubs/HrNotificationHub.cs` | Hub `[Authorize]`, grup per akun `user:{id}` |
| `.../HrServiceManagement/Controllers/HrdNotificationController.cs` | Endpoint pusat notifikasi, `[AccessController("HrdNotification")]`, aksi `ViewOwn` dan `MarkRead` |
| `.../WorkflowManagement/Services/UnifiedNotificationGatewayService.cs` | Gateway kini menyalurkan ke pusat pemberitahuan; ringkasan harian anti-duplikat per tanggal WIB |
| `.../WorkflowManagement/Workers/WorkflowV2Workers.cs` | `DailyApprovalDigestWorker` tidur sampai 07:00 WIB berikutnya |
| `.../WorkflowManagement/Services/WorkflowApprovalActionService.cs`, `WorkflowInstanceCreationService.cs` | Pemberitahuan instan ke pemohon dan approver; pemohon tidak lagi jatuh ke "karyawan pertama di tabel" bila akun tidak punya `ApplicantEmployeeId` |
| `Program.cs` | Registrasi `IHrdNotificationService`, pemetaan hub, `access_token` query untuk `/hubs/notifications` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<HrdNotification>` |
| `Migrations/20261111020000_AddHrdNotificationAndSuspensionAccess.cs` | Tabel `HrdNotification` (SQL idempoten) |

### Dokumentasi endpoint

#### HR Services - Notification Center

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/self-service/hr/notifications` | Daftar pemberitahuan milik saya (filter `unreadOnly`, `category`, paging) | `HrdNotification : ViewOwn` |
| `GET` | `/api/v1/self-service/hr/notifications/unread-count` | Jumlah belum dibaca untuk lencana lonceng | `HrdNotification : ViewOwn` |
| `POST` | `/api/v1/self-service/hr/notifications/{id}/read` | Tandai satu pemberitahuan sudah dibaca | `HrdNotification : MarkRead` |
| `POST` | `/api/v1/self-service/hr/notifications/read-all` | Tandai semua sudah dibaca | `HrdNotification : MarkRead` |

Hub SignalR: `/hubs/notifications`, pesan `notificationReceived` berisi `HrdNotificationDto`.

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria sesudah pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Notifikasi instan terkirim < 3 detik ke pemohon dan approver | Terpenuhi di kode untuk kanal dalam aplikasi (tulis tabel + dorongan SignalR dalam request yang sama); waktu nyata belum diukur | `WorkflowApprovalActionService.NotifyRequesterAsync/NotifyStepApproverAsync` |
| Ringkasan harian terkirim terjadwal tiap 07:00 pagi | Terpenuhi di kode (jadwal WIB + anti-duplikat) | `DailyApprovalDigestWorker.DelayUntilNextDigest` |
| Multikanal (web push, push mobile, email) | **Belum terpenuhi** — hanya kanal dalam aplikasi; kanal lain tidak ada di platform | keputusan Gelombang 2 |
| `AT-HRD-WF-14`, `AT-HRD-WF-15` | `NOT RUN` | menunggu migration dan akun uji |

Status: 🟡 sampai migration dijalankan dan uji dijalankan; kanal lain menjadi pekerjaan platform bersama bila kelak diputuskan.
