# Laporan Perubahan Backend — `BE-KSK-PO-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-002` |
| Judul | Kuota menghitung pendaftaran online dan dikunci per jadwal |
| Slice | `EPIC KSK-PO-01` — fondasi data, kuota, dan penutupan (`FR-KSK-PO-002`), gelombang teknis 2, rilis `MVP-0` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-002` |
| Trace | `INV-PO-02`; `KSK-DEC-030`; `KSK-DSN-PO-009`; `GATE-PO-06`, `GATE-PO-17`; `CAP-PO-10`, `CAP-PO-34`; `02-backend-architecture.md` PO.4.3; `contracts/api-contract.md` *Patient Encounter (diperluas)* |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-001` ✅ |
| Klasifikasi | `MEDIUM` — dua berkas, tetapi menyentuh jalur pembuatan kunjungan bersama petugas, kiosk, dan IGD |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/Services/DoctorScheduleQuotaService.cs`, `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`) |
| Tanggal | 2026-11-19 |
| Status | ✅ Selesai (2026-11-20) — AC 1, 3, 4 terbukti di runtime pada 2026-11-19; AC 2 terbukti di runtime pada 2026-11-20 lewat `BE-KSK-PO-005` (satu pendaftaran online sungguhan menaikkan `usedAppointment` dan `usedTotal` sebesar 1). Sebelumnya 🟡 |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` (`PatientEncounterController` dengan logika di controller) dan `NEW CODE` (method baru di `DoctorScheduleQuotaService`) |
| QBE yang berlaku | `QBE-SVC-001` (logika kuota dan kunci tetap di service; controller hanya memanggil), `QBE-TXN-001` (kunci dan pemeriksaan ulang di dalam transaksi yang sudah ada), `QBE-LOG-001` (tidak ada log baru yang memuat data sensitif) |
| Utang teknis yang tidak dirapikan | Logika pembuatan kunjungan masih di `PatientEncounterController` (legacy). Task ini menempel di titik yang sama dengan kunci pasien, tanpa memindahkan logika |

## 1. Masalah yang diperbaiki

Ada dua masalah:

1. **Kuota belum mengenal pendaftaran online.** Pasien yang mendaftar online untuk Rabu harus langsung memakai satu kuota appointment dokter (`KSK-DEC-030`), supaya kuotanya tidak habis oleh pendaftar lain sebelum ia datang.
2. **Slot terakhir bisa terpakai berkali-kali.** Kuota diperiksa sebelum transaksi dan tanpa kunci per jadwal. Uji pembanding di backend lama membuktikannya: lima pendaftaran bersamaan ke jadwal berkuota **1** semuanya berhasil, sehingga dokter mendapat **lima** pasien.

## 2. Proses bisnis

**Hitungan kuota.** Setiap layar dan validasi yang memakai `DoctorScheduleQuotaService` kini menjumlahkan antrean poli aktif **ditambah** pendaftaran online berstatus Menunggu Check-in pada jadwal dan tanggal yang sama. Pendaftaran online dihitung sebagai appointment.

Contoh: jadwal dr. Sari hari Rabu punya kuota appointment 2. Sudah ada 1 pasien appointment di loket, lalu Ibu Rina mendaftar online. Kuota appointment Rabu menjadi penuh (1 + 1 = 2), dan pendaftar ketiga ditolak "Kuota appointment pada jadwal dokter sudah penuh."

| Status pendaftaran online | Dihitung |
| --- | :---: |
| Menunggu Check-in | Ya |
| Menjadi Kunjungan | Tidak, karena sudah diwakili antreannya sendiri |
| Didaftarkan lewat Jalur Lain, Tidak Datang, Dibatalkan | Tidak, kuotanya dilepas |

**Kunci kuota.** Saat kunjungan dibuat dengan jadwal dokter, backend mengambil kunci per jadwal dan tanggal **sesudah** kunci pasien, lalu menghitung ulang kuotanya. Permintaan kedua menunggu sampai yang pertama selesai, lalu melihat kuota yang sudah terpakai. Contoh: lima petugas menekan Simpan bersamaan untuk slot terakhir dr. Hadi. Satu berhasil; empat lainnya menerima "Kuota pasien pada jadwal dokter sudah penuh." Jalur pendaftaran online (`BE-KSK-PO-005`) dan ubah jadwal petugas (`BE-KSK-PO-010`) akan memakai kunci yang sama dengan urutan yang sama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DoctorScheduleQuotaService.cs` dan seluruh pemanggilnya (`PatientEncounterController` validasi kuota dan endpoint `doctor-schedule-quotas`, `OutpatientEncounterScheduleService`); blok transaksi dan kunci pasien `PatientEncounterController.cs:1078-1106`; `ValidateDoctorScheduleForEncounterAsync`/`ValidateDoctorScheduleQuotaAsync` (`:2818-2930`); pola kunci `pg_advisory_xact_lock` di `OutpatientPriorityReservationService`, `OutpatientEncounterScheduleService`, `OutpatientQueueNumberAllocator`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/DoctorScheduleQuotaService.cs` | `GetUsagesAsync` menambahkan jumlah `RegOnlineRegistration` berstatus `AwaitingCheckIn` (tidak terhapus) ke `Total` dan `Appointment` per jadwal; method baru `AcquireQuotaLockAsync(scheduleId, visitDate)` (`pg_advisory_xact_lock(hashtext('REG_SCHEDULE_QUOTA_{id:N}_{yyyyMMdd}'))`, dilewati di provider non-Postgres) dan `BuildQuotaLockKey` |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Di `CreateEncounterCoreAsync`, sesudah blok kunci pasien dan sebelum pembuatan kunjungan: bila `DoctorScheduleId` terisi, ambil kunci kuota, muat ulang jadwal, lalu jalankan ulang `ValidateDoctorScheduleQuotaAsync`. Penuh → `400` dengan pesan yang sama seperti validasi biasa |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response tidak berubah. Perilaku: `GET .../patient-encounters/doctor-schedule-quotas` dan validasi kuota ikut menghitung pendaftaran online (sesuai kontrak `v2` "perilaku diperbarui"); permintaan bersamaan pada slot terakhir kini ditolak `400` |
| Database | Tanpa schema baru, tanpa migration. Kunci advisory hanya hidup selama transaksi |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses tidak berubah |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters` | Membuat kunjungan (petugas). Kini dengan kunci kuota per jadwal | `PatientEncounter : Create` (tidak berubah) |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/kiosk` | Membuat kunjungan dari kiosk. Kunci yang sama | `KioskRead` (tidak berubah) |
| `GET` | `/api/v1/health-services/registration-management/patient-encounters/doctor-schedule-quotas` | Sisa kuota, kini termasuk pendaftaran online Menunggu Check-in | `PatientEncounter : Read` (tidak berubah) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -o <scratchpad>` | Build succeeded, 0 error | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| R1 regresi kuota, backend lama (7184) vs baru (7185), DB `QuilvianNewDevSukma`, hari ini, H+1, H+7, tanpa data uji | JSON kuota identik, 63 jadwal per tanggal | `PASS` | Skrip `r1_regression.py` (scratchpad) |
| R1 diulang dengan pemakaian nyata (6 slot terpakai oleh kunjungan uji) | JSON kuota identik, 65 jadwal | `PASS` | Idem. Membuktikan query pendaftaran online berjalan di Postgres tanpa galat |
| R4 backend baru: 5 `POST patient-encounters` bersamaan (pasien uji berbeda) ke jadwal uji Poli Paru berkuota 1 | 1 berhasil (`200`), 4 ditolak `400` "Kuota pasien pada jadwal dokter sudah penuh."; waktu respons bertahap 1,21–1,38 s (diproses bergantian) | `PASS` | Skrip `r4_race.py` |
| R4 pembanding backend lama (tanpa kunci), skenario sama dengan 5 pasien lain | 5 dari 5 berhasil pada kuota 1 | Bukti masalah lama | Skrip `r4_race.py` ke 7184 |
| Harness in-memory `GetUsagesAsync` (Q0–Q8) | 9/9 PASS: tanpa online = hitungan antrean; +1 Menunggu Check-in = Total +1, Appointment +1, WalkIn tetap; status 2–5, terhapus, tanggal lain, jadwal lain tidak dihitung; jadwal yang hanya punya pendaftaran online muncul; `Kind` tanggal tidak berpengaruh; kuota appointment menjadi penuh; kunci dilewati di non-Postgres; format kunci | `PASS` | Harness `ApplicationDbContext` + provider in-memory di scratchpad, tanpa menyentuh DB |
| AC 2 runtime: `doctor-schedule-quotas` sebelum/sesudah satu pendaftaran online sungguhan | Dijalankan 2026-11-20 lewat endpoint `BE-KSK-PO-005` pada jadwal uji berkuota appointment 6: `usedAppointment` 1→2, `usedTotal` 1→2, sisa kuota opsi publik 5→4; 34 kiriman yang ditolak tidak mengubah hitungan | `PASS` | [`BE-KSK-PO-005`](BE-KSK-PO-005.md) T1d, T1e, T7 |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

**Data uji runtime (dibuat dan dibersihkan lewat API):** 4 jadwal dokter uji `TEST-PO002 <jam>` (dinonaktifkan) dan 13 kunjungan uji untuk pasien `KSKTEST*` dan `RJPR UJI*` (seluruhnya dibatalkan dengan alasan "Data uji BE-KSK-PO-002"). Tujuh kunjungan di antaranya sempat tercatat sebagai IGD karena kesalahan skrip uji (`encounterType` 2); semuanya sudah dibatalkan. Sesudah pembersihan, kuota terpakai hari ini kembali 0.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tanpa pendaftaran online, angka kuota sama persis dengan sebelum perubahan (regresi petugas, kiosk, IGD) | Terpenuhi | R1 (dua kali, termasuk dengan pemakaian nyata). Hitungan berasal dari service yang sama untuk ketiga kanal |
| (2) Satu pendaftaran Menunggu Check-in mengurangi sisa appointment dan total sebesar 1; status lain tidak | Terpenuhi | Logika terbukti (harness Q1–Q6), query berjalan di Postgres (R1), dan runtime dengan baris nyata 2026-11-20 ([`BE-KSK-PO-005`](BE-KSK-PO-005.md) T1d). Status selain Menunggu Check-in belum dapat dibuat di runtime sampai `BE-KSK-PO-007`/`008`/`010`; dibuktikan harness Q3–Q5 |
| (3) Urutan kunci pasien → kuota sama di semua jalur | Terpenuhi untuk jalur yang ada | `CreateEncounterCoreAsync`: kunci pasien (`:1088-1096`) lalu kunci kuota. Jalur pendaftaran online (`BE-KSK-PO-005`) kini memakai urutan yang sama: kunci pasien lalu `AcquireQuotaLockAsync`. Jalur ubah jadwal (`BE-KSK-PO-010`) wajib mengikuti |
| (4) Dua permintaan kunjungan bersamaan pada slot terakhir: tepat satu berhasil | Terpenuhi | R4: 1 dari 5 berhasil; pembanding backend lama 5 dari 5 |
| DoD: kriteria 1–4 terbukti; build, QBE Strict PASS | Terpenuhi | Kriteria 2 terbukti di runtime pada 2026-11-20 lewat `BE-KSK-PO-005` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kunci kuota juga berlaku untuk kunjungan IGD yang membawa `DoctorScheduleId`, karena validasi kuota yang sudah ada juga berlaku untuk mereka. Hasilnya hanya antre pada jadwal yang sama, tanpa aturan baru |
| Masalah yang diketahui | Pindah jadwal kunjungan (`OutpatientEncounterScheduleService`) menghitung kuota tanpa kunci kuota. Ini di luar cakupan task dan desain; dicatat sebagai temuan untuk blueprint `rawat-jalan` (`GATE-PO-06`) |
| Risiko tersisa | Titik sentuh ini mengubah perilaku jalur RJ. Pencatatannya di decision log `rawat-jalan` (`GATE-PO-06`) masih menjadi tindak lanjut pemilik |
| Perubahan sampingan | `NONE` di repository. Keluaran build, skrip uji, ledger data uji, dan log backend uji berada di scratchpad |
| Interupsi | `NONE`. Backend uji 7185 dihentikan sesudah uji; backend pengguna (7184) tidak disentuh |
| Status Git | `M Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`, `M Areas/HealthServices/RegistrationManagement/Services/DoctorScheduleQuotaService.cs`, ditambah perubahan `BE-KSK-PO-001`/`003` dan dokumen yang belum di-commit |
| Langkah berikutnya | `BE-KSK-PO-004` (opsi publik dan pencocokan pasien lama), lalu `BE-KSK-PO-005`, yang sekaligus menutup AC 2 task ini |
