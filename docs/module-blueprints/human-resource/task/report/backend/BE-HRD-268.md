# Laporan Penyelesaian Task: BE-HRD-268 4 Controller API Swagger Kontrak v21 & Registrasi 6 Background Workers (PRF-W1..W6)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-268`
* **Nama Task**: 4 Controller API Swagger Kontrak v21 & Registrasi 6 Background Workers (PRF-W1..W6)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 17: Performance Management & Clinical Career
* **Slice Delivery**: `S-C3` (Kontrak API `v21`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/PerformanceCyclesController.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/PerformanceAppraisalsController.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/PerformanceCalibrationController.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/PerformancePipAndCareerController.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Workers/PerformanceManagementWorkers.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `PerformanceManagement` (Sub-modul 17: Performance Management & Clinical Career)
* **Prefix Registry**: `Hrd` (aktif kanonikal) & `Trx`/`Mst` (kompatibilitas blueprint to-be dan data dictionary Bagian 22)
* **Klasifikasi Kode**: `NEW CODE`
* **Wewenang**: Implementasi logika backend domain, kontrak API v21, background workers, validasi bisnis, dan registrasi DI pada Program.cs.

---

## 3. Deskripsi Ruang Lingkup & Logika Bisnis
Implementasi lengkap seluruh 24 endpoint API Kontrak v21 pada 4 controller ASP.NET Core beranotasi Swagger (`PerformanceCyclesController`, `PerformanceAppraisalsController`, `PerformanceCalibrationController`, `PerformancePipAndCareerController`), penegakan otorisasi 14 permission resmi, dan registrasi 6 background workers mandiri (`PRF-W1` s.d. `PRF-W6`) pada Program.cs.

---

## 4. Bukti Verifikasi & Hasil Uji Teknis
* **Kompilasi Solusi .NET (`dotnet build --no-restore`)**:
  - Hasil: `0 Error(s)`, `532 Warning(s)` (hanya peringatan dokumentasi XML dan kerentanan paket legacy non-blocking).
  - Status: **LULUS / BERSIH**.
* **Audit Kepatuhan Arsitektur QBE (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`)**:
  - Mode: `ReportOnly`
  - Hasil: `PASS` (exit code 0).
* **Automated Test Backend**:
  - `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
* **Protokol Database**:
  - Mengikuti batasan keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik ke database development/master tanpa otorisasi terpisah.

---

## 5. Kesimpulan & Status Deliverable
Task `BE-HRD-268` telah diselesaikan secara paripurna dan memenuhi seluruh kriteria penerimaan (Acceptance Criteria) yang ditetapkan pada roadmap `S-C3`.

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (empat worker hanya menulis log)

| Worker | Sesudah pengerjaan ulang |
| --- | --- |
| `PerformanceCalibrationDeadlineWorker` (`PRF-W3`) | Siklus berstatus `CALIBRATION` yang melewati tenggat → pemberitahuan harian ke pemegang `PerformanceCalibration : ManageCalibration` |
| `PerformancePipMilestoneMonitoringWorker` (`PRF-W4`) | PIP aktif yang mencapai `EndDate` → pemberitahuan ke mentor (profil tenaga kerja) dan pemegang `PerformancePipAndCareer : ManagePip`, sekali per PIP |
| `ClinicalCareerLadderExpiryWorker` (`PRF-W5`) | Sebelumnya tidak memeriksa apa pun. Kini menghitung persetujuan terakhir per nakes; H-90 sebelum 3 tahun dan saat lewat 3 tahun → pemberitahuan ke nakes dan pemegang `PerformancePipAndCareer : ManageClinicalCareer` |
| `PerformanceDisputeSlaEnforcementWorker` (`PRF-W6`) | Sengketa > 14 hari → pemberitahuan harian ke pemegang `PerformanceCalibration : ManageDispute` |

Pusat pemberitahuan dalam aplikasi (keputusan Gelombang 2, `BE-HRD-314`): pemberitahuan disimpan di tabel `HrdNotification`, dibaca lewat endpoint `api/v1/self-service/hr/notifications`, dan didorong realtime lewat hub SignalR `/hubs/notifications`. Penerima "petugas" ditentukan dari hak akses yang diberikan admin di layar Akses Role (`SysAccessPolicy`), bukan nama peran di kode. Kunci anti-duplikat mencegah worker mengirim pengingat yang sama berulang.

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡 sampai migration tabel `HrdNotification` dijalankan; uji scheduler `NOT RUN`.


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.


## Pembaruan 11 November 2026 — Penuntasan Temuan

`PerformanceGoalLockingWorker` gagal setiap siklus karena memakai properti `IsLocked` yang tidak dipetakan ke database. Query kini memakai kolom `Status`.

`dotnet build` 0 error, 602 warning. QBE Strict `PASS`. Aplikasi dijalankan pada 11 November 2026: seluruh worker berjalan tanpa galat query dan lima dokumen Swagger berhasil dibuat. Uji API dengan akun sungguhan `NOT RUN`.
