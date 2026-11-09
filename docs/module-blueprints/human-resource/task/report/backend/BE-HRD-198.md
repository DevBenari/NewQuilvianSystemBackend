# Laporan Penyelesaian Backend Task: BE-HRD-198

## 1. Identitas Task
* **ID Task**: `BE-HRD-198`
* **Nama Task**: Background Worker Pemantauan Timer SLA, Alert 75% & Auto-Eskalasi 100% (`HrServiceSlaMonitoringWorker`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Komitmen pelayanan kepegawaian rumah sakit membutuhkan sistem pengawasan aktif (*proactive monitoring*) tanpa bergantung pada ingatan manual staf HR. Tiket permohonan yang mendekati batas tenggat waktu harus segera diberikan peringatan dini, dan tiket yang terlambat harus otomatis dieskalasikan ke pimpinan untuk investigasi penyebab keterlambatan.

Fitur-fitur utama yang disediakan dalam task ini:
1. **Background Service Pemantauan Berkala 15 Menit (`HrServiceSlaMonitoringWorker`, `HRS-W3`)**:
   - Berjalan sebagai layanan latar belakang ASP.NET Core (`IHostedService` / `BackgroundService`) yang mengevaluasi seluruh tiket aktif setiap 15 menit.
   - Mengecualikan tiket yang telah selesai (`Resolved`, `Closed`, `Cancelled`) dan tiket yang sedang ditangguhkan (`WaitingStaffResponse`).
2. **Peringatan Dini Ambang Batas 75% Durasi SLA (`Warning75`)**:
   - Jika durasi waktu pemrosesan tiket telah mencapai $\ge 75\%$ dan $< 100\%$ dari total target waktu yang dialokasikan, sistem secara otomatis:
     * Mengubah `SlaStatus` menjadi `Warning75`.
     * Menerbitkan log peringatan dini kuning untuk supervisor HRD agar memprioritaskan penyelesaian tiket tersebut.
3. **Deteksi Keterlambatan SLA & Auto-Eskalasi 100% ke Manajer SDM (`HRS-W3`, `INT-HR-68`)**:
   - Jika batas tenggat waktu `SlaDeadlineUtc` telah terlampaui tanpa adanya resolusi:
     * Sistem menandai `IsSlaBreached = true`.
     * Mengubah `SlaStatus` menjadi `Escalated`.
     * Menerbitkan log eskalasi merah dan mendaftarkan tiket ke antrean eskalasi prioritas Manajer SDM untuk tindakan korektif.
4. **Penyelarasan Siklus Hidup Gelombang MVP-39**:
   - Menuntaskan seluruh 8 task backend pada fase gelombang `MVP-39` (`BE-HRD-191` s.d. `BE-HRD-198`).

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Background Worker Service
File: `Areas/Corporate/HumanResource/HrServiceManagement/Workers/HrServiceSlaMonitoringWorker.cs`
* Kelas: `HrServiceSlaMonitoringWorker : BackgroundService`.
* Interval: `TimeSpan.FromMinutes(15)`.
* Pemanfaatan `IServiceScopeFactory` untuk isolasi koneksi basis data per siklus polling.

### B. Pendaftaran di Program.cs
File: `Program.cs`
* `builder.Services.AddHostedService<HrServiceSlaMonitoringWorker>();`

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Peringatan Dini 75% pada Surat Keterangan Gaji**:
  - Analis HR Anton memproses permohonan Surat Keterangan Gaji KPR (SLA: 24 Jam Kerja).
  - Pada jam ke-18 kerja (75% durasi), tiket belum berstatus `Resolved`.
  - `HrServiceSlaMonitoringWorker` mendeteksi kondisi ini, mengubah `SlaStatus` menjadi `Warning75`, dan memunculkan badge oranye pada antrean supervisor HR.
  - Supervisor menegur Anton untuk segera menyelesaikan verifikasi draf surat sebelum jam kerja berakhir.

* **Skenario 2 — Auto-Eskalasi Tiket Cito Terlambat**:
  - Tiket Surat Tugas Darurat Medis (Tier Cito, SLA: 4 jam) diajukan pukul 08:00 WIB.
  - Hingga pukul 12:05 WIB tiket masih belum terselesaikan karena analis berhalangan.
  - Worker mendeteksi `nowUtc > SlaDeadlineUtc`, menandai `IsSlaBreached = true` dan mengubah status menjadi `Escalated`.
  - Manajer SDM menerima peringatan eskalasi darurat dan langsung mengambil alih (*re-assign*) tiket kepada analis pengganti.

---

## 5. Ringkasan Status 8 Task Gelombang MVP-39 (Slice S-D4)

| Task ID | Nama Komponen / Task | Target MVP | Status |
| :---: | :--- | :---: | :---: |
| `BE-HRD-191` | Entitas EF Core, Enums & Migrasi PostgreSQL HrServiceManagement | `MVP-39` | ✅ Selesai |
| `BE-HRD-192` | EntityTypeConfiguration Fluent API & Indeks Terarah | `MVP-39` | ✅ Selesai |
| `BE-HRD-193` | Service & Controller Master Katalog Layanan SDM (`HrServiceCatalogService`) | `MVP-39` | ✅ Selesai |
| `BE-HRD-194` | Mesin Penomoran Terpusat Gapless Berbasis Atomic Row-Locking | `MVP-39` | ✅ Selesai |
| `BE-HRD-195` | Agregat Tiket Helpdesk SDM & Pengajuan Mandiri ESS (`HrServiceTicketService`) | `MVP-39` | ✅ Selesai |
| `BE-HRD-196` | Berkas Lampiran Tiket & Utas Klarifikasi Dua Arah (`HrServiceTicketComment`) | `MVP-39` | ✅ Selesai |
| `BE-HRD-197` | Mesin Kalkulasi SLA 3-Tier Berbasis Jam Kerja RS & Penangguhan Klarifikasi | `MVP-39` | ✅ Selesai |
| `BE-HRD-198` | Background Worker Pemantauan Timer SLA, Alert 75% & Auto-Eskalasi 100% | `MVP-39` | ✅ Selesai |

---

## 6. Verifikasi & Integritas Bukti
* **Kompilasi C#**: Bebas error dan peringatan tipe data.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-HRS-06` dan `AT-HRD-HRS-07`.
* **Kesiapan Fase**: Seluruh 8 task fase `MVP-39` resmi **SELESAI 100%**.