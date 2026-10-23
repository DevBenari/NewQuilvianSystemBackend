# Laporan Perubahan Backend — `BE-RJP-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-009` |
| Judul | Penutupan Tidak Datang |
| Slice | `MVP-1`, `EPIC RP-04` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-009` |
| Trace | `FR-RP-036`; `RJ-PRI-DEC-013`; `INT-RP-08`; `INV-RP-06`, `07` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` — `state-transition-matrix.md` RP-B (Hari kunjungan berakhir → Tidak Datang) |
| Dependency | `BE-RJP-006` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: diperiksa 3–8 (0), diubah 3 (0), logika sedang (1), kontrak (1: transisi sistem), keamanan (0), workflow terjadwal (3) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `OutpatientPriorityReservationClosureHostedService.cs` (baru), `OutpatientPriorityReservationService.cs`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-19 |
| Status | ✅ Selesai — runtime 5/5 dua kali + pemeriksaan peran `Web` `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; hosted service hanya menjadwalkan), `QBE-CFG-001` (konfigurasi lewat `PriorityReservationOptions`) |
| Role access | Tidak ada endpoint. Riwayat ditulis dengan `Source = System` dan pelaku kosong |

---

## 1. Masalah yang diselesaikan

Reservasi yang pasiennya tidak datang tetap terbuka selamanya. Daftar admisi besok pagi masih memuat reservasi kemarin,
dan aturan "satu reservasi terbuka per pasien per tanggal" tidak pernah dilepas.

## 2. Proses bisnis

Setiap hari pukul 00.05 WIB, dan sekali setiap aplikasi mulai, sistem menutup semua reservasi Menunggu Kedatangan atau
Sudah Tiba yang tanggal kunjungannya sebelum hari ini. Statusnya menjadi Tidak Datang dan riwayatnya mendapat satu baris
`NO_SHOW`. Reservasi Dibatalkan, Menjadi Kunjungan, dan Didaftarkan lewat Jalur Lain tidak disentuh.

Contoh dari uji: reservasi Menunggu Kedatangan dan Sudah Tiba bertanggal kemarin menjadi Tidak Datang saat server mulai.
Reservasi Dibatalkan bertanggal kemarin tetap Dibatalkan, dan reservasi hari ini tetap Menunggu Kedatangan. Saat server
dimulai ulang, tidak ada yang ditutup lagi dan riwayat `NO_SHOW` tetap satu.

## 3. Perubahan yang dikerjakan

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationClosureHostedService.cs` | Baru | `BackgroundService`: jalan saat start, lalu menunggu sampai `Closure:RunAtLocalTime` WIB berikutnya. Jeda memakai `PatientMembershipLifecycleHostedService.DelayUntilNextRun` yang sudah ada. Mati bila `Enabled` atau `Closure:Enabled` bernilai `false` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `CloseExpiredAsync`: batch 200 baris per transaksi, riwayat `NO_SHOW` sumber `System`, batas 500 batch per putaran. Konflik `RowVersion` membatalkan batch itu saja; putaran berikutnya memilih ulang |
| `Program.cs` | Diperbarui | `AddHostedService<OutpatientPriorityReservationClosureHostedService>()` di blok `runBackgroundJobs`, sehingga hanya berjalan pada `Runtime:Role` `Worker`/`All` |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru |
| Database | Tidak ada perubahan schema. `has-pending-model-changes` bersih |
| Realtime | Tidak mengirim kabar realtime, karena container Worker tidak memegang SignalR (desain RP.6) |

## 4. Verifikasi

Data samaran: empat reservasi pasien `RJPR UJI NS-*` dibuat lewat API hari ini (satu ditandai tiba, satu dibatalkan),
lalu tiga di antaranya dimundurkan ke kemarin lewat SQL data uji (izin `RJ-PRI-DEC-031`). Server uji kemudian dimulai
ulang dua kali.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build, `has-pending-model-changes`, QBE Strict (3 berkas) | 0 error; bersih; VIOLATION 0 | `PASS` |
| Start pertama sesudah build (sebelum data dimundurkan) | Log "Penutupan reservasi prioritas selesai: 6 reservasi ditutup" — reservasi uji lama bertanggal 16–18 Okt | Catatan |
| `R-RP-18` run 1 a, b | Kemarin Menunggu Kedatangan dan Sudah Tiba: Tidak Datang, `ClosedAt` terisi, satu `NO_SHOW` sumber `System` tanpa pelaku. Log: 2 ditutup | `PASS` |
| `R-RP-18` run 1 c, d | Kemarin Dibatalkan tidak disentuh; hari ini tetap Menunggu Kedatangan | `PASS` |
| `UAT-RP-10` run 1 | Detail: "Tidak Datang", riwayat terakhir "Tidak datang" tanpa pelaku, semua `can*` `false` | `PASS` |
| `R-RP-18` run 2 (start ulang) | Log: 0 ditutup; keadaan dan jumlah `NO_SHOW` sama dengan run 1 | `PASS` |
| `Runtime:Role=Web` | Tidak ada log penutupan maupun pekerjaan membership selama ±25 detik sesudah start | `PASS` |

Skrip: `rt_rjp009.py` di scratchpad sesi. Total 5/5 per run.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`. Penjadwalan pukul 00.05 tidak ditunggu secara nyata; perhitungan jedanya
memakai fungsi yang sama dengan pekerjaan membership (`BE-CRM-020`).

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-18` | Terpenuhi | Run 1 dan run 2 |
| `UAT-RP-10` | Terpenuhi | Detail reservasi |
| Hanya pada `Worker`/`All` | Terpenuhi | Uji peran `Web` |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Risiko tersisa | Antara 00.00 dan 00.05 WIB, reservasi kemarin masih terbuka. Dapat diterima karena admisi belum bekerja pada jam itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-010` (realtime daftar reservasi) |
