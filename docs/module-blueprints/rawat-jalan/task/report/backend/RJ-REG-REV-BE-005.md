# Laporan Perubahan Backend — `RJ-REG-REV-BE-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-BE-005` |
| Judul | Tracking dan reschedule appointment |
| Slice | Batch 3 — `RJ-REG-REV-008` (REV-10) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `3` |
| Trace | `RJ-REG-DEC-007` |
| Contract version | Controller baru `OutpatientAppointment` (aditif) |
| Dependency | — |
| Klasifikasi | `HEAVY` — entity + migration + transaksi lintas antrean |
| Task mode | `CROSS-REPO MODE` (`RJ-REG-DEC-001`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a52b3c98` (`sukmagp`), belum di-commit |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

**Backend Governance Preflight:** Area `HealthServices`, Module `RegistrationManagement`, prefix `Reg` (ACTIVE;
entity baru `RegEncounterReschedule`, `NEW CODE`), `RegQueue`/`RegPatientEncounter` `TOUCHED LEGACY`.
QBE: `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-002`, `QBE-SVC-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`,
`QBE-TXN-001`, `QBE-CODE-003` (nomor antrean dari alokator bersama, tanpa Max+1), `QBE-DTO-001`, `QBE-PAGE-001`.

## 1. Masalah yang diperbaiki

Tidak ada layar untuk melihat booking (appointment) Rawat Jalan maupun memindahkan jadwalnya. Pasien yang minta
pindah hari harus dibatalkan lalu didaftarkan ulang — nomor kunjungan, penjamin, dan rujukan ikut hilang.

## 2. Proses bisnis

**Hasil audit relasi:** source tidak punya entity booking. Appointment = `RegPatientEncounter` dengan
`IsAppointment = true` yang antreannya (`RegQueue`) sudah dialokasikan pada tanggal kunjungan saat mendaftar.
Unique index `IX_RegQueue_EncounterId` (baris hidup) membatasi satu antrean per kunjungan.

Reschedule:
1. Hanya appointment Rawat Jalan berklinik yang **belum datang**: status ≤ `Menunggu Dokter`, bukan
   `Screening Perawat`, belum check-in, antrean belum dipanggil/diskrining.
2. Tanggal baru ≥ hari ini; jadwal dokter baru milik poli dan service unit yang sama, aktif, berlaku,
   `IsAllowAppointment`, cocok hari/tanggal; kuota pasien dan kuota appointment pada tanggal itu belum penuh;
   bukan jadwal yang sama; penjamin (asuransi/perusahaan) masih berlaku pada tanggal baru.
3. Pasien tidak boleh punya kunjungan hidup lain di poli + tanggal yang sama → `409`.
4. Di dalam satu transaksi: kunci `FOR UPDATE` kunjungan + kunci advisory pasien yang sama dengan pendaftaran;
   antrean **yang sama dipindah** (ID antrean tetap) dengan nomor baru dari `OutpatientQueueNumberAllocator` dan
   klasifikasi member untuk tanggal baru; kunjungan mendapat tanggal, jadwal, dokter, ruang baru; riwayat
   `RegEncounterReschedule` (lama/baru, alasan, pelaku) disimpan. Tidak ada kunjungan baru.
5. Sesudah commit: notifikasi realtime batal (salinan keadaan lama) dan dibuat (keadaan baru); audit tanpa alasan
   dan data pasien.

Contoh: `ENC-RSMMC-00283` antrean `I004` 19 Okt 2026 → 20 Okt 2026 jadwal lain, antrean menjadi `I006`; riwayat
mencatat `I004 → I006`.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/RegistrationManagement/Models/RegEncounterReschedule.cs` | Entity riwayat |
| `Repositories/Configurations/HealthServices/RegEncounterRescheduleConfiguration.cs` | Mapping, index `(EncounterId, RescheduledAt)`, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<RegEncounterReschedule>` |
| `…/Services/OutpatientAppointmentService.cs` | Daftar, detail, metadata, reschedule |
| `…/Controllers/OutpatientAppointmentController.cs` | Controller baru (`AccessController` `OutpatientAppointment`) |
| `…/DTOS/OutpatientAppointmentDtos.cs` | Query, item, detail, riwayat, request/response |
| `Program.cs` | Registrasi service |
| `Migrations/20261014050013_AddEncounterRescheduleHistory.cs` (+ Designer, snapshot) | Satu tabel |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Controller baru, aditif |
| Database | Migration `20261014050013_AddEncounterRescheduleHistory` (1 tabel + index + FK) **diterapkan ke `QuilvianNewDevSukma`**; EF tanpa drift |
| Keamanan/Auth | Resource baru `OutpatientAppointment` (`Read`, `Reschedule`) — **perlu dicentang admin**. Tidak memakai nama role |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Outpatient Appointment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/outpatient-appointments` | Daftar booking (tanggal maks 92 hari, poli, dokter, status, cari) | `OutpatientAppointment : Read` |
| `GET` | `/outpatient-appointments/filters/metadata` | Opsi status, poli, dokter | `OutpatientAppointment : Read` |
| `GET` | `/outpatient-appointments/{id}` | Detail + riwayat reschedule | `OutpatientAppointment : Read` |
| `POST` | `/outpatient-appointments/{id}/reschedule` | Pindah tanggal/jadwal | `OutpatientAppointment : Reschedule` |

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build / EF / QBE Strict / migration apply | `0 Error` / no changes / `PASS` / applied | `PASS` |
| R0–R4 daftar, filter `Arrived`, detail, metadata | sesuai | `PASS` |
| R5 tolak: alasan kosong, tanggal lampau, format, jadwal sama, jadwal tidak cocok hari, jadwal tidak ada | `400` semua | `PASS` |
| R6 `404`; R7 `401` | sesuai | `PASS` |
| R9 dua reschedule paralel | `[200, 400]` — satu berhasil, satu "tidak ada yang diubah" | `PASS` |
| R10 ID tetap, tanggal/jadwal/antrean pindah, riwayat 1 | sesuai (`I004 → I006`) | `PASS` |
| R11 tidak ada kunjungan ganda | 1 baris | `PASS` |
| R12 bentrok pasien + poli + tanggal `409` | — | `NOT FEASIBLE` — penjaga kunjungan aktif pendaftaran sudah menolak membuat kunjungan kedua; cabang `409` dibuktikan lewat kode |
| R13 appointment batal `400`; R14 filter `Cancelled`; R15 rentang > 92 hari `400` | sesuai | `PASS` |
| Layar T1–T7 (`RJ-REG-REV-FE-006`) | 7/7 | `PASS` |

Run 1 menemukan `500` karena desain awal membuat antrean baru (melanggar `IX_RegQueue_EncounterId`); diperbaiki
menjadi pemindahan antrean di tempat → run 2 **37/37 PASS**.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar berhalaman dengan filter | Terpenuhi | R1, R2, R14, R15 |
| 2. Detail pasien/dokter/poli/jadwal/antrean/status/riwayat | Terpenuhi | R3, R10, T6 |
| 3. Reschedule valid: ID tetap, antrean pindah, riwayat | Terpenuhi (antrean dipindah di tempat, bukan batal+baru — delta karena unique index) | R10 |
| 4. Penolakan `400` | Terpenuhi (kuota penuh & penjamin tidak berlaku lewat kode) | R5, R13 |
| 5. Bentrok `409` | Kode ada; uji `NOT FEASIBLE` | R12 |
| 6. Paralel tanpa antrean ganda | Terpenuhi | R9, R11 |
| 7. Tanpa izin `403` | `401` diuji; `403` lewat kode | R7 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Snapshot umur pada kunjungan tidak dihitung ulang untuk tanggal baru (selisih hari tidak material) |
| Risiko tersisa | Notifikasi realtime tidak diuji lewat WebSocket |
| Perubahan sampingan | Data uji: pasien `REGREV UJI/LAYAR/WNA *`, kunjungan uji sudah dibatalkan |
| Status Git | Berkas di atas `M`/`??` |
| Langkah berikutnya | Admin mencentang `OutpatientAppointment : Read/Reschedule`; UAT |
