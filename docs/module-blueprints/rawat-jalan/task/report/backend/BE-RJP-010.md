# Laporan Perubahan Backend — `BE-RJP-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-010` |
| Judul | Realtime daftar reservasi |
| Slice | `MVP-1`, `EPIC RP-05` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-010` |
| Trace | `FR-RP-038`; `RJ-PRI-DEC-014`, `026`; `INT-RP-07` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` — `api-contract.md` hub `/hubs/queues` (`JoinPriorityReservationBoard`, event `PriorityReservationChanged`); `permission-audit-matrix.md` RP-1 (`Read`) |
| Dependency | `BE-RJP-007` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: diperiksa 9–20 (1), diubah 6 (1), logika sedang (1), kontrak (1), keamanan (1: hak masuk group), workflow (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `Hubs/QueueHub.cs`, `QueueRealtimeService.cs`, `QueueRealtimeDtos.cs`, `OutpatientPriorityReservationController.cs`, `PatientEncounterController.cs`, `OutpatientPriorityReservationService.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-19 |
| Status | ✅ Selesai — runtime 9/9 `PASS` + regresi display |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement`; hub bersama di `Hubs/` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` (`QueueHub`, `QueueRealtimeService`, `PatientEncounterController`); `NEW CODE` (DTO, method baru) |
| QBE yang berlaku | `QBE-DTO-001` (payload kabar adalah DTO baru), `QBE-PERM-001` |
| Role access | Masuk group lewat `AccessPermissionService.HasAccessAsync(User, "OutpatientPriorityReservation", "Read")` — butir yang dicentang admin di layar Akses Role, bukan nama peran. Akun display ditolak lewat `RejectQueueDisplayDeviceAccount` (klaim perangkat, `BE-RJP-004`) |

---

## 1. Masalah yang diselesaikan

Petugas skrining dan admisi harus menekan muat ulang untuk tahu ada pasien prioritas yang baru tiba (PDF 2C–2D).
Sesudah task ini, setiap perubahan reservasi mengirim kabar ke layar admisi, dan layar memuat ulang daftarnya sendiri.

## 2. Proses bisnis

1. Layar daftar reservasi bergabung ke group `priority-reservation-board` lewat `JoinPriorityReservationBoard`.
2. Setiap kali reservasi berubah — dibuat, tiba, dikembalikan, diskrining, link dikirim ulang, dibatalkan, dilengkapi
   menjadi kunjungan, atau ditutup karena jalur lain — backend mengirim `PriorityReservationChanged` sesudah perubahan
   tersimpan.
3. Isi kabar hanya `reservationId`, `visitDate`, `status`, `eventCode`, dan `occurredAt`. Nama pasien, No. RM, alasan,
   dan checklist tidak ikut; layar mengambilnya lewat API yang dijaga hak akses.

Contoh dari uji: petugas skrining tersambung ke board. Admisi membuat reservasi, menandai tiba, petugas skrining
menyimpan checklist, lalu admisi melengkapinya menjadi kunjungan. Board menerima empat kabar berurutan:
`CREATED`, `ARRIVED`, `SCREENED`, `CONVERTED`.

Konfirmasi lewat link pasien mengirim kabar yang sama; jalurnya dibuat di `BE-RJP-012`. Penutupan Tidak Datang tidak
mengirim kabar karena berjalan di container Worker tanpa SignalR (`BE-RJP-009`).

## 3. Perubahan yang dikerjakan

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Hubs/QueueHub.cs` | Diperbarui | `JoinPriorityReservationBoard` (tolak akun display, periksa `Read`), `LeavePriorityReservationBoard`. Konstruktor menerima `AccessPermissionService` dari DI |
| `Areas/HealthServices/RegistrationManagement/Services/QueueRealtimeService.cs` | Diperbarui | `PriorityReservationEventName = "PriorityReservationChanged"`, `PriorityReservationBoardGroupName = "priority-reservation-board"`, `NotifyPriorityReservationChangedAsync` (upaya terbaik; gagal kirim dicatat tanpa data pribadi) |
| `Areas/HealthServices/RegistrationManagement/DTOS/QueueRealtimeDtos.cs` | Diperbarui | `PriorityReservationRealtimeEventResponse` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `PriorityReservationCommandResult.EventCode`; `CompleteInTransactionAsync` mengembalikan reservasi; `CloseForEncounterAsync` mengembalikan daftar reservasi yang ditutup |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientPriorityReservationController.cs` | Diperbarui | Kabar dikirim sesudah buat, kirim ulang, tiba, kembalikan, skrining, dan batal |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Diperbarui | Kabar `CONVERTED` atau `REGISTERED_ELSEWHERE` dikirim sesudah commit transaksi kunjungan |

| Aspek | Dampak |
| --- | --- |
| Kontrak | Method hub dan event baru, aditif |
| Database | Tidak ada perubahan schema. `has-pending-model-changes` bersih |
| Keamanan | Payload tanpa data pribadi; group dijaga butir `Read` |

## 4. Dokumentasi hub

| Method / event | Pemanggil | Isi | Status |
| --- | --- | --- | --- |
| `JoinPriorityReservationBoard()` | Petugas pemegang `OutpatientPriorityReservation : Read` | Bergabung ke `priority-reservation-board`. Ditolak untuk akun display dan pengguna tanpa `Read` | Tersedia |
| `LeavePriorityReservationBoard()` | Petugas | Keluar dari group | Tersedia |
| Event `PriorityReservationChanged` | Ke `priority-reservation-board` | `{ reservationId, visitDate, status, eventCode, occurredAt }` | Tersedia |

Delta: kontrak tidak menyebut `LeavePriorityReservationBoard`; ditambahkan agar layar dapat keluar dari group saat ditutup,
mengikuti pasangan join/leave yang lain di hub.

## 5. Verifikasi

Akun uji: petugas skrining (`Read` + `Screen`, pasangan Logistik dan Gudang / Staff Pengadaan) dan pengguna tanpa `Read`
(`PatientEncounter : Create` saja, pasangan Logistik dan Gudang / Petugas Gudang), dibuat seperti `BE-RJP-007`, ditambah
akun display uji dari `FE-RJP-001`. Tiga koneksi SignalR (`sr_board.cjs`) mendengarkan selama 40 detik.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build, `has-pending-model-changes`, QBE Strict (6 berkas) | 0 error; bersih; VIOLATION 0 | `PASS` |
| Petugas skrining `JoinPriorityReservationBoard` | OK | `PASS` |
| Pengguna tanpa `Read` | Ditolak "Anda tidak memiliki akses ke daftar reservasi prioritas." | `PASS` |
| Akun display | Ditolak "Akun layar display memakai JoinQueueDisplayCluster…" | `PASS` |
| `R-RP-14` a | Reservasi A: `CREATED`, `ARRIVED`, `SCREENED`, `CONVERTED` (pelengkapan lewat `POST /patient-encounters/admin`) | `PASS` |
| `R-RP-14` b | Reservasi B: `CREATED`, `CODE_REISSUED`, `CANCELLED`; reservasi C: `CREATED`, `REGISTERED_ELSEWHERE` (pendaftaran loket biasa) | `PASS` |
| Isi payload | Hanya `eventCode`, `occurredAt`, `reservationId`, `status`, `visitDate` | `PASS` |
| Kebocoran | Akun tanpa `Read` dan akun display menerima 0 kabar | `PASS` |
| Pembersihan | Kebijakan kedua pasangan 0; kedua akun uji nonaktif | `PASS` |
| Regresi `BE-RJP-004` | Akun display: `JoinQueueDisplayCluster` OK, `JoinNurseStationCluster` ditolak | `PASS` |

Skrip: `rt_rjp010.py`, `sr_board.cjs` di scratchpad sesi. Total 9/9 `PASS`.

Bagian "layar berbunyi" pada `R-RP-14` adalah sisi frontend dan diuji di `FE-RJP-005`.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-14` (sisi backend) | Terpenuhi | a, b |
| Payload tanpa nama pasien | Terpenuhi | Pemeriksaan payload |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Konstruktor `QueueHub` kini menerima `AccessPermissionService`. Tidak ada kode yang membuat `QueueHub` secara manual |
| Masalah yang diketahui | — |
| Risiko tersisa | Kabar bersifat upaya terbaik; layar wajib tetap memuat ulang saat dibuka dan saat ↻ ditekan (`INT-RP-07`) |
| Perubahan sampingan | `NONE`. Akun uji `TEST-RJP Uji board-*` tersisa nonaktif |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-011` (nama ruang tunggu prioritas) |
