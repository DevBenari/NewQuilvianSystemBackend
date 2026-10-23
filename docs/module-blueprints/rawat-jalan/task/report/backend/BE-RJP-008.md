# Laporan Perubahan Backend — `BE-RJP-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-008` |
| Judul | Pelengkapan dan jalur lain |
| Slice | `MVP-1`, `EPIC RP-05` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-008` |
| Trace | `FR-RP-040` s.d. `043`; `RJ-PRI-DEC-009`, `010`, `016`, `024`, `027`; `RJ-PRI-DEC-030` usulan (1); `VAL-RP-15` s.d. `20`; `INV-RP-04` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` — `api-contract.md` *Patient Encounter (diperbarui)*; `integration-contract.md` `INT-RP-03`, `INT-RP-04` |
| Dependency | `BE-RJP-007` ✅, `BE-RJP-002` ✅ |
| Klasifikasi | `HIGH` — skor 10: diperiksa 20+ (2), diubah 3 (0), logika tinggi (2), kontrak berubah (2), keamanan (1: izin `Complete`), workflow lintas jalur pendaftaran (3) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `PatientEncounterController.cs`, `PatientEncounterDtos.cs`, `OutpatientPriorityReservationService.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-19 |
| Status | ✅ Selesai — runtime 16/16 + 2/2 `PASS`, tanpa regresi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` (`PatientEncounterController`, `PatientEncounterDtos`); `NEW CODE` (method service reservasi) |
| QBE yang berlaku | `QBE-SVC-001` (logika reservasi di service; controller hanya memanggil), `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001` |
| Role access | Endpoint pendaftaran tetap dijaga `PatientEncounter : Create`. Bila `priorityReservationId` diisi, service memeriksa `OutpatientPriorityReservation : Complete` lewat `AccessPermissionService.HasAccessAsync` (`permission-audit-matrix.md` RP-3). Temuan lama yang tidak diubah: policy `KioskRead` di `Program.cs` memakai `IsInRole("SuperAdmin"/"Administrator"/"Kiosk")` |
| Utang teknis yang disentuh | `PatientEncounterController` memakai `ApplicationDbContext` langsung (legacy, `RP.7`). Tidak dirapikan; logika baru tinggal di `OutpatientPriorityReservationService`. Service reservasi diambil per request dari `RequestServices`, mengikuti pola `ReferralDocumentStorage` agar konstruktor lama tetap sah |

---

## 1. Masalah yang diselesaikan

Reservasi yang pasiennya sudah tiba belum bisa dijadikan kunjungan. Selain itu, pasien yang mendaftar lewat kiosk atau
loket biasa pada hari reservasinya meninggalkan reservasi tetap terbuka, sehingga sore harinya tercatat "Tidak Datang"
padahal pasien sudah dilayani.

## 2. Proses bisnis

**Lengkapi reservasi (jalur utama).** Pasien Gold tiba dan ditandai Sudah Tiba. Admisi membuka Pendaftaran Pasien,
memilih poli dan dokter seperti biasa, lalu mengirim pendaftaran dengan `priorityReservationId`. Sistem:

1. memeriksa jadwal dan kuota dokter dengan aturan yang sudah ada (`VAL-RP-20`);
2. memeriksa izin `Complete`, keberadaan reservasi, pasien yang sama, status Sudah Tiba, dan tanggal hari ini;
3. di dalam satu transaksi: mengunci per pasien, memastikan reservasi belum berubah, membuat kunjungan dengan sumber
   pendaftaran `PriorityReservation (6)`, membuat antrean dari snapshot reservasi, dan menandai reservasi Menjadi
   Kunjungan beserta riwayat `CONVERTED`.

Contoh dari uji: reservasi pasien Gold dilengkapi ke Poli Penyakit Dalam. Kunjungan terbit dengan nomor antrean 25
(kelipatan 5, prioritas), dan riwayat reservasi menjadi `CREATED → ARRIVED → CONVERTED`.

**Jalur lain.** Bila pasien yang punya reservasi terbuka mendapat kunjungan Rawat Jalan berpoliklinik **tanpa**
`priorityReservationId` (kiosk atau loket biasa), reservasi ditutup sebagai Didaftarkan lewat Jalur Lain dalam
transaksi yang sama, dengan riwayat sumber `System`. Kunjungan IGD dan Laboratorium langsung tidak menyentuh reservasi.

**Membership snapshot dibatalkan.** Bila membership yang tercatat saat reservasi dibuat kemudian Dibatalkan, daftar
reservasi menandai `isPriorityRevoked = true` dan kunjungan hasil pelengkapan mendapat nomor Reguler (`RJ-PRI-DEC-024`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientEncounterController.CreateEncounterCoreAsync` (penuh), `ValidateCreateRequestAsync` (jadwal, kuota, kunjungan
aktif), `OutpatientQueueNumberAllocator`, `OutpatientEncounterRules`, `AccessPermissionService`, policy `KioskRead`,
`LabPatientRegistrationController`/`EncounterIntakeService` (jalur Laboratorium), payload pendaftaran IGD di frontend.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | Diperbarui | `PatientEncounterCreateRequest.PriorityReservationId` (`Guid?`) |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Diperbarui | (1) Kiosk menolak `priorityReservationId` (`VAL-RP-19`) sebelum pemeriksaan lain. (2) Sesudah validasi jadwal/kuota, `PrepareCompletionAsync`. (3) Di dalam transaksi, sebelum nomor kunjungan dan antrean dialokasikan: `CompleteInTransactionAsync` atau `CloseForEncounterAsync`. (4) Klasifikasi antrean dari snapshot bila ada reservasi. (5) `RegistrationSource = PriorityReservation` untuk kunjungan hasil pelengkapan. (6) `PriorityReservationConflictException` dan `DbUpdateConcurrencyException` (khusus pelengkapan) dijawab `409` dengan rollback |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `PrepareCompletionAsync`, `CompleteInTransactionAsync`, `CloseForEncounterAsync`; tipe `PriorityReservationCompletion`, `PriorityReservationCompletionResult`, `PriorityReservationConflictException`; dependensi `AccessPermissionService` |

### 3.3 Urutan kunci

Perubahan reservasi mengambil kunci per pasien (`REG_ENCOUNTER_ACTIVE_<patientId>`) **sebelum** alokator nomor antrean
mengambil kunci klaster. Urutan pasien → klaster ini sama dengan pendaftaran biasa saat saklar `BlockActiveEncounter`
hidup, sehingga tidak ada urutan kunci yang saling berlawanan. Kunci advisory bersifat reentrant di dalam satu transaksi.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /patient-encounters/admin` menerima `priorityReservationId` opsional. `POST /` dan `/kiosk` menolaknya `400`. Tanpa field ini, perilaku sama dengan sebelumnya kecuali reservasi terbuka pada tanggal yang sama ikut ditutup |
| Database | Tidak ada perubahan schema. `has-pending-model-changes` bersih |
| Keamanan | Izin `Complete` diperiksa di backend; frontend tidak menentukan kewenangan |

### 3.5 Delta terhadap kontrak

| Hal | Isi |
| --- | --- |
| Jenis kunjungan | Reservasi hanya dapat dilengkapi menjadi kunjungan Rawat Jalan berpoliklinik. Selain itu `400` "Reservasi prioritas hanya dapat dilengkapi menjadi kunjungan Rawat Jalan ke poliklinik." Kontrak tidak menyebut kasus ini |
| Pelengkapan ulang saat saklar `BlockActiveEncounter` hidup | Ditolak lebih dulu oleh aturan kunjungan aktif (`400`) sebelum aturan reservasi (`409`). Hasil akhirnya sama: tidak ada kunjungan kedua |
| `queueClassification.priorityWaitingLocationName` | Dikerjakan di `BE-RJP-011`, sesuai roadmap |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter (diperbarui)

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/admin` | Daftar kunjungan; bila `priorityReservationId` diisi, sekaligus melengkapi reservasi | `PatientEncounter : Create`; plus `OutpatientPriorityReservation : Complete` bila `priorityReservationId` diisi | `PatientEncounterCreateRequest` + `priorityReservationId?` | `ApiResponse<PatientEncounterCreateResponse>` | Tersedia |
| `POST` | `/`, `/kiosk` | Daftar kunjungan dari kiosk | Policy kiosk | `PatientEncounterCreateRequest`; `priorityReservationId` ditolak `400` | Sama | Tersedia |

Kode status pelengkapan: `400` pasien berbeda, tanggal bukan hari ini, jadwal/kuota tidak sah, atau dari kiosk; `403`
tanpa `Complete`; `404` reservasi tidak ada; `409` reservasi bukan Sudah Tiba atau baru saja diubah.

## 5. Verifikasi

Data samaran: pasien `RJPR UJI C1`..`C20`, membership Gold disisipkan SQL data uji. Untuk `R-RP-16`, kuota jadwal A
(`6b23c07c…`) diubah sementara lewat SQL menjadi sama dengan jumlah terpakai, lalu dikembalikan ke 10/5 (diperiksa ulang).
Untuk `R-RP-20`, membership snapshot diubah SQL menjadi Dibatalkan. Akun uji tanpa `Complete` dibuat seperti
`BE-RJP-007` (pasangan Logistik dan Gudang / Petugas Gudang) dan dibersihkan sesudahnya.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build, `has-pending-model-changes`, QBE Strict (3 berkas) | 0 error; bersih; VIOLATION 0 | `PASS` |
| `UAT-RP-11` | `200`; antrean prioritas nomor 25; `RegistrationSource = 6`; reservasi `Converted` menunjuk kunjungan; riwayat `CREATED, ARRIVED, CONVERTED` | `PASS` |
| `INV-RP-04` ulang | Pelengkapan ulang ditolak; tetap satu kunjungan, satu `CONVERTED` | `PASS` |
| `R-RP-17` a / `UAT-RP-12` | Reservasi Menunggu Kedatangan: `409`, tanpa kunjungan | `PASS` |
| `R-RP-17` b | Pasien berbeda: `400` `VAL-RP-16`, tanpa kunjungan | `PASS` |
| `R-RP-17` c | Tanggal besok (jadwal Selasa yang sah): `400` "Kunjungan dari reservasi hanya untuk hari ini, 19 Oktober 2026." | `PASS` |
| `VAL-RP-15` | Reservasi acak: `404` | `PASS` |
| `R-RP-27` | Kiosk membawa `priorityReservationId`: `400`; reservasi tetap Sudah Tiba | `PASS` |
| `R-RP-17` d / `R-RP-19` (lengkapi) | Akun `Read` + `Screen` + `PatientEncounter : Create`: `403` `VAL-RP-18`, tanpa kunjungan | `PASS` |
| Pembersihan akun uji | Kebijakan 0, akun nonaktif | `PASS` |
| `R-RP-16` | Jadwal A penuh: `400` "Kuota pasien pada jadwal dokter sudah penuh.", reservasi tetap Sudah Tiba; dokter lain: `200`, antrean prioritas, `Converted`; kuota A dikembalikan | `PASS` |
| `R-RP-20` | `isPriorityRevoked = true` di daftar; kunjungan nomor 14, bukan prioritas | `PASS` |
| `R-RP-21` / `UAT-RP-13` | Kiosk tanpa reservasi: `200`; reservasi Menunggu Kedatangan menjadi Didaftarkan lewat Jalur Lain menunjuk kunjungan; riwayat sumber `System` | `PASS` |
| `R-RP-21` loket | Loket tanpa `priorityReservationId`: reservasi Sudah Tiba ditutup jalur lain | `PASS` |
| `R-RP-22` | IGD `200` dan Laboratorium langsung `200`; reservasi tetap Menunggu Kedatangan | `PASS` |
| `R-RP-30` | Reguler loket (nomor 16) dan kiosk (17): `200`, bukan kelipatan 5, sumber tetap 1; IGD dan Lab `200` | `PASS` |
| `INV-RP-04` paralel (saklar hidup) | `[200, 400]`: yang kalah ditolak aturan kunjungan aktif; satu kunjungan, satu `CONVERTED` | `PASS` |
| `INV-RP-04` paralel (saklar mati, proses uji dijalankan dengan `HealthServices__Registration__BlockActiveEncounter=false`) | `[200, 409]` "Reservasi baru saja diubah petugas lain…"; satu kunjungan, satu `CONVERTED` | `PASS` |
| `INV-RP-04` ulang (saklar mati) | `409` "berstatus Menjadi Kunjungan"; tanpa kunjungan kedua | `PASS` |

Skrip: `rt_rjp008.py`, `rt_rjp008b.py`, `rt_rjp008_payload.py` di scratchpad sesi. Total 16/16 + 2/2 `PASS`.

**Percobaan yang tidak dipakai sebagai bukti.** Run pertama dan kedua memakai skenario uji yang keliru: jadwal Senin
untuk tanggal Selasa (validasi jadwal menolak lebih dulu), jadwal dengan kuota walk-in habis untuk uji paralel, dan
syarat pendaftaran pengisi yang tidak perlu. Ketiganya diperbaiki di skrip, bukan di produk.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-16` | Terpenuhi | Uji kuota |
| `R-RP-17` | Terpenuhi | a–d |
| `R-RP-20` | Terpenuhi | Uji |
| `R-RP-21` (penutupan) | Terpenuhi | Kiosk dan loket. Bagian tiket kiosk ke ruang tunggu prioritas menyusul `BE-RJP-011`/`FE-RJP-007` |
| `R-RP-22` | Terpenuhi | IGD dan Laboratorium |
| `R-RP-27` | Terpenuhi | Uji kiosk |
| `R-RP-30` | Terpenuhi, tanpa regresi | Loket, kiosk, IGD, Laboratorium |
| `UAT-RP-11` s.d. `13` | Terpenuhi | Lihat tabel |
| `R-RP-19` (lengkapi) | Terpenuhi | Uji akun tanpa `Complete` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Saklar `BlockActiveEncounter` hidup di dev. Bila dimatikan di lingkungan lain, penjaga reservasi sendiri tetap mencegah kunjungan ganda (terbukti) |
| Masalah yang diketahui | — |
| Risiko tersisa | Jalur pendaftaran ini dipakai kiosk, loket, dan IGD; regresi utama sudah diuji. Jalur pembuatan kunjungan RJ lain di luar `CreateEncounterCoreAsync` tidak ditemukan saat pemeriksaan |
| Perubahan sampingan | `NONE`. Kuota jadwal A dikembalikan 10/5. Akun uji `TEST-RJP Uji nocomplete` tersisa nonaktif |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-009` (penutupan Tidak Datang) |
