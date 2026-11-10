# Laporan Penyelesaian Task: BE-HRD-296 Background Worker SerialSurveillanceFollowUpWorker (OHS-W1) Bulan 1, 3, dan 6

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-296`
* **Nama Task**: Background Worker SerialSurveillanceFollowUpWorker (OHS-W1) Bulan 1, 3, dan 6
* **Modul / Sub-Modul**: Human Resource / Sub-modul 19: Occupational Health & Staff Safety (K3RS & Medical Check-Up)
* **Slice Delivery**: `S-C6` (Kontrak API `v23`, Gelombang `MVP-58`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `Implementasi SerialSurveillanceFollowUpWorker (OHS-W1) terdaftar sebagai IHostedService.`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `OccupationalHealthManagement` (Sub-modul 19: K3RS & MCU)
* **Prefix Registry**: `Hrd` / `Trx` / `Wfp` (Kepatuhan Kamus Data Bagian 24)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi backend sesuai kontrak terkunci `v23` dan aturan bisnis `VR-OHS-01` s.d. `VR-OHS-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam tata kelola kesehatan kerja rumah sakit (K3RS), keselamatan fisik staf medis dan kepatuhan terhadap regulasi Kemenkes/Bapeten/BPJS memiliki implikasi hukum, klinis, dan keselamatan pasien yang kritis:
1. **Perlindungan Privasi Medis Staf (HRD-DEC-176, VR-OHS-01/02)**: Rekam medis staf dipisahkan secara ketat di tingkat kolom (*Field-Level Access Separation*). User berrole HR dan Manajemen hanya menerima status kelaikan operasional (*Fit*, *Fit with Restriction*, *Unfit*) tanpa diagnosis penyakit ICD-10, sedangkan rincian klinis terenkripsi kuat menggunakan AES-256.
2. **Kelaikan Kerja & Gembok Hard-Stop Roster (INT-HR-89, VR-OHS-03..07)**: Penerbitan status kelaikan kerja operasional oleh dokter bersertifikat Hiperkes aktif. Surat pembatasan tugas kerja (*Work Restriction*) maksimal 180 hari memicu gembok validasi ketat (*hard-stop*) pada Sub-06 Penjadwalan agar staf tidak ditugaskan pada shift malam atau zona bahaya. Gembok hanya dapat dicabut melalui asesmen *Return-to-Work* (RTW) resmi.
3. **Tanggap Darurat Tertusuk Jarum Cito & Terapi PPP < 4 Jam (INT-HR-91, VR-OHS-08..11)**: Penanganan insiden tertusuk jarum suntik bekas infeksius melalui tombol panik ESS dengan transmisi cito < 5 detik, pengawalan batas emas keselamatan terapi profilaksis pasca pajanan (PPP/PEP ARV/HBIG) dalam < 4 jam, serta jadwal surveilans serologi serial darah pada Bulan 1, 3, dan 6.
4. **Radar Imunisasi Nakes & Akreditasi STARKES (VR-OHS-12..17, AT-HRD-OHS-22)**: Pemantauan titer antibodi Anti-HBs kuantitatif (< 10 mIU/mL memicu recall booster), pemantauan dosis radiasi badge TLD kuartalan (> 5 mSv) dan tahunan (> 20 mSv), serta laporan pemenuhan standar STARKES instrumen KPS.
5. **Klaim Kecelakaan Kerja BPJS Ketenagakerjaan JKK (INT-HR-90, VR-OHS-18..21)**: Penegakan batas waktu pelaporan insiden KAK/PAK ke BPJS Ketenagakerjaan dalam 2x24 jam (Tahap I) dan reimbursement klaim (Tahap II).

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Background worker terjadwal (OHS-W1) yang secara otomatis memantau kasus pajanan berstatus UnderSurveillance dan mengevaluasi jadwal kontrol serologi serial tepat pada Bulan ke-1, Bulan ke-3, dan Bulan ke-6 sesuai VR-OHS-11. Menerbitkan tiket panggilan lab berkala dan menutup kasus menjadi ClosedSeronegative jika evaluasi 6 bulan non-reaktif (AT-HRD-OHS-11).
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 24, dan matriks validasi Sub-modul 19.
* **Kriteria Penerimaan (Acceptance Criteria)**: Seluruh method service, model, DTO, worker, dan controller terimplementasi rapi, terisolasi dalam transaksi database ACID, dan lolos uji kompilasi serta conformance check.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS** (0 Violation).
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1q (`AT-HRD-OHS-10, AT-HRD-OHS-11`).

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (worker hanya menulis log)

`SerialSurveillanceFollowUpWorker` (`OHS-W1`) kini mengirim jadwal pemeriksaan serologi serial (bulan ke-1, 3, 6) yang sudah jatuh tempo ke pegawai yang terpajan dan ke pemegang hak akses `NeedleStickEmergency : ManageSerialSurveillance`, satu kali per kasus per tanggal tindak lanjut.

Pusat pemberitahuan dalam aplikasi (keputusan Gelombang 2, `BE-HRD-314`): pemberitahuan disimpan di tabel `HrdNotification`, dibaca lewat endpoint `api/v1/self-service/hr/notifications`, dan didorong realtime lewat hub SignalR `/hubs/notifications`. Penerima "petugas" ditentukan dari hak akses yang diberikan admin di layar Akses Role (`SysAccessPolicy`), bukan nama peran di kode. Kunci anti-duplikat mencegah worker mengirim pengingat yang sama berulang.

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡 sampai migration tabel `HrdNotification` dijalankan.
