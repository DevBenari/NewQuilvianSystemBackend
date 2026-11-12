# Laporan Penyelesaian Backend Task: BE-HRD-197

## 1. Identitas Task
* **ID Task**: `BE-HRD-197`
* **Nama Task**: Mesin Kalkulasi SLA 3-Tier Berbasis Jam Kerja RS & Penangguhan Klarifikasi (`HrServiceSlaEngine`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Pengukuran kinerja layanan kepegawaian rumah sakit membutuhkan akurasi penghitungan waktu kerja nyata (*real working hours*). Permohonan surat yang diajukan pada Jumat sore pukul 16:30 tidak adil jika tenggat waktunya habis pada hari Sabtu atau Minggu ketika kantor administrasi HRD tutup. Sebaliknya, permohonan darurat penugasan klinis dokter (*Cito*) harus berjalan tanpa henti 24 jam.

Fitur-fitur utama yang dibangun dalam task ini:
1. **Mesin Kalkulasi SLA 3-Tier (`HrServiceSlaEngine`, `HRS-B2`)**:
   - **Tier Standar (24 Jam)** & **Tier Khusus (72 Jam)**:
     * Dihitung berdasarkan jam operasional resmi kantor HRD RS Quilvian: **Senin s.d. Jumat pukul 08:00 – 17:00 WIB** (9 jam kerja per hari kerja).
     * Secara otomatis melompati akhir pekan (Sabtu & Minggu). Jika staf mengajukan tiket pada Jumat pukul 16:00 dengan SLA 24 jam, batas tenggat waktu akan jatuh pada hari Rabu minggu berikutnya pukul 14:00 WIB.
   - **Tier Cito (4 Jam Kalender)**:
     * Dihitung secara linier **24/7 jam kalender tanpa henti** demi mobilitas penugasan medis darurat.
2. **Penangguhan Timer SLA saat Klarifikasi Staf (`HRS-W2`)**:
   - Ketika analis HR mengubah status tiket menjadi `WaitingStaffResponse` (menunggu kelengkapan dokumen atau klarifikasi staf), timer SLA ditangguhkan (*paused*) dan waktu penangguhan dicatat pada `SlaPausedAtUtc`.
   - Ketika staf merespons, timer dilanjutkan (*resumed*), durasi penangguhan diakumulasikan ke `TotalPausedMinutes`, dan deadline `SlaDeadlineUtc` dimajukan secara proporsional.
3. **Engine Evaluasi Progres Durasi SLA**:
   - Metode `EvaluateSlaProgress` menghitung persentase durasi yang telah berjalan (`ElapsedPercentage`) dan status keterlambatan (`IsBreached`) secara real-time.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Antarmuka & Layanan Mesin SLA
File: `Areas/Corporate/HumanResource/HrServiceManagement/Services/IHrServiceSlaEngine.cs` & `HrServiceSlaEngine.cs`
* Metode `CalculateSlaDeadline`: Kalkulasi deadline berbasis kalender kerja RS (WIB = UTC+7).
* Metode `PauseSlaTimer` & `ResumeSlaTimer`: Pengelolaan jeda timer pada siklus klarifikasi.
* Metode `EvaluateSlaProgress`: Evaluasi elapsed hours dan persentase durasi berjalan.

### B. Penyimpanan Status di Basis Data
* Penambahan kolom `SlaPausedAtUtc` dan `TotalPausedMinutes` pada `TrxHrServiceRequest`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Pengajuan Surat Keterangan Kerja pada Jumat Sore**:
  - Bidan Dewi mengajukan Surat Keterangan Kerja (Tier Standar, SLA 24 Jam Kerja) pada hari Jumat pukul 15:00 WIB (sisa 2 jam kerja hari Jumat).
  - Algoritma `HrServiceSlaEngine` menghitung:
    * Sisa jam kerja Jumat (15:00 - 17:00) = 2 jam (sisa target = 22 jam).
    * Sabtu dan Minggu dilewati penuh.
    * Senin (08:00 - 17:00) = 9 jam (sisa target = 13 jam).
    * Selasa (08:00 - 17:00) = 9 jam (sisa target = 4 jam).
    * Rabu: 08:00 + 4 jam = **Rabu pukul 12:00 WIB**.
  - Deadline `SlaDeadlineUtc` tepat ditetapkan jatuh pada hari Rabu pukul 12:00 WIB.

* **Skenario 2 — Penangguhan Timer karena Ijazah Belum Terlampir**:
  - Pada hari Selasa pukul 10:00 WIB, analis HR meminta scan ijazah legalisir dan mengubah status ke `WaitingStaffResponse` (timer membeku).
  - Bidan Dewi baru mengunggah ijazah pada hari Selasa pukul 15:00 WIB (tertunda 5 jam).
  - Sistem melanjutkan timer dan memundurkan tenggat waktu SLA sebesar 5 jam ke depan, sehingga deadline baru menjadi hari Rabu pukul 17:00 WIB.

---

## 5. Verifikasi & Integritas Bukti
* **Presisi Waktu**: Akurat membedakan jam kerja (9 jam/hari) dengan jam kalender 24 jam.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-HRS-03`, `AT-HRD-HRS-04`, dan `AT-HRD-HRS-05`.