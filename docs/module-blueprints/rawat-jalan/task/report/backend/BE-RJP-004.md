# Laporan Perubahan Backend — `BE-RJP-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-004` |
| Judul | Group realtime display |
| Slice | `MVP-0`, `EPIC RP-02` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-004` |
| Trace | `FR-RP-011`, `012`; `RJ-PRI-DEC-025`; `RJ-DOC-DEC-057`; `CONF-RP-01`; `INT-RP-06`; `api-contract.md` hub `/hubs/queues` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 6: diperiksa 9–20 (1), diubah 3 (0), logika sedang (1), kontrak berubah (2), keamanan berkaitan (1), workflow terbatas (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `Hubs/QueueHub.cs`, `Areas/HealthServices/RegistrationManagement/Services/QueueRealtimeService.cs`, `.../DTOS/QueueRealtimeDtos.cs`, dokumen `rawat-jalan` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`) |
| Tanggal | 2026-10-16 |
| Status | ✅ Selesai — runtime 5/5 `PASS`. **Rilis wajib bersama `FE-RJP-001`** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement`; hub bersama di `Hubs/` |
| Pemilik / prefix | Registration, `Reg` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-DTO-001` (payload display adalah DTO baru, bukan entity), `QBE-API-001`, `QBE-PERM-001` (akses group mengikuti klaim cluster yang ada) |
| Catatan role access | Akun display dikenali dari klaim login perangkat `is_queue_display_account`/`profile_type = QueueDisplayDevice` yang diterbitkan `AuthController`, bukan dari nama peran. Pemeriksaan `IsInRole("SuperAdmin")` yang sudah ada di hub adalah kode lama dan dicatat sebagai temuan, tidak diubah |

---

## 1. Masalah yang diperbaiki

Layar display bergabung ke group realtime ruang kerja perawat. Akibatnya ia menerima payload lengkap, termasuk
`PatientId`, `EncounterId`, dan antrean prioritas, lalu menampilkan panggilan langsung dari payload itu. Nomor prioritas
muncul sekilas, dan `RJ-DOC-DEC-057` dilanggar (`CONF-RP-01`).

## 2. Proses bisnis

1. Akun perangkat display bergabung ke group display per cluster lewat `JoinQueueDisplayCluster`.
2. Bila akun display mencoba bergabung ke group perawat atau dokter, hub menolaknya.
3. Setiap perubahan antrean tetap dikirim lengkap ke perawat dan dokter, seperti sebelumnya.
4. Untuk antrean non-prioritas, sistem juga mengirim event `QueueDisplayChanged` berisi payload ringkas ke group
   display. Antrean prioritas tidak pernah dikirim ke group display.

Contoh dari uji: perawat dan display terhubung ke cluster Rawat Jalan Spesialis. Satu pasien Reguler (nomor 23) dan satu
pasien Gold (nomor 15) didaftarkan. Perawat menerima dua kabar lengkap. Display menerima satu kabar, yaitu nomor 23,
tanpa `patientId` maupun `encounterId`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`Hubs/QueueHub.cs` (penuh), `QueueRealtimeService.cs` (group, `NotifyQueueChangedAsync`, `BuildPayload`),
`QueueRealtimeDtos.cs`, `Program.cs` (policy display), `Controllers/AuthController.cs` (klaim login display), frontend
`queue-realtime-client.js` dan `use-queue-display-screen.jsx` (konsumen).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Hubs/QueueHub.cs` | Tambah `JoinQueueDisplayCluster`/`LeaveQueueDisplayCluster`. `RejectQueueDisplayDeviceAccount()` di keenam method join group perawat dan dokter |
| `Areas/HealthServices/RegistrationManagement/Services/QueueRealtimeService.cs` | Konstanta `QueueDisplayEventName = "QueueDisplayChanged"`, `BuildQueueDisplayClusterGroupName`, pengiriman payload ringkas ke group display hanya untuk antrean non-prioritas, `BuildDisplayPayload` |
| `Areas/HealthServices/RegistrationManagement/DTOS/QueueRealtimeDtos.cs` | DTO `QueueDisplayRealtimeEventResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking untuk layar display lama**: akun display tidak bisa lagi bergabung ke group perawat. `FE-RJP-001` memindahkan layar ke `JoinQueueDisplayCluster`. Sebelum `FE-RJP-001` dirilis, display tetap memuat ulang daftar berkala lewat API |
| Database | Tidak ada perubahan schema; `has-pending-model-changes` bersih |
| Keamanan/Auth | Perangkat publik tidak lagi menerima `PatientId`, `EncounterId`, aktor, pesan bebas, maupun data member. Group IGD (`JoinEmergencyQueue`) tidak disentuh |

## 4. Dokumentasi endpoint

#### Hub realtime `/hubs/queues`

| Method / event | Kegunaan | Hak akses |
| --- | --- | --- |
| `JoinQueueDisplayCluster(clusterId)` | Bergabung ke group display | Klaim cluster perangkat, atau pengguna yang berhak atas cluster itu |
| `LeaveQueueDisplayCluster(clusterId)` | Keluar dari group display | Login |
| `JoinNurseStationCluster`, `JoinAccessibleNurseStationClusters`, `JoinDoctorQueue`, `JoinAccessibleDoctorQueues`, `JoinDoctorClinicQueue`, `JoinAccessibleDoctorClinicQueues` | Tidak berubah untuk petugas. Akun display ditolak | Tidak berubah |
| Event `QueueDisplayChanged` | Payload ringkas untuk display | — |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Build | 0 error | `PASS` | Keluaran build |
| `has-pending-model-changes` | "No changes have been made to the model since the last migration." exit 0 | `PASS` | Keluaran perintah |
| QBE Strict | VIOLATION 0 | `PASS` | Keluaran QBE |
| `R-RP-05` — akun display memanggil `JoinNurseStationCluster` | Ditolak: `HubException: Akun layar display memakai JoinQueueDisplayCluster, bukan group ruang kerja perawat atau dokter.` | `PASS` | `sr_listen.cjs` |
| Akun display `JoinQueueDisplayCluster`; petugas `JoinNurseStationCluster` | Keduanya OK | `PASS` | `sr_listen.cjs` |
| `R-RP-04` a — display menerima Reguler, tidak menerima prioritas | 1 event, Reguler saja | `PASS` | `rt_mvp0.py` |
| `R-RP-04` b — isi payload display | Hanya `clinicId`, `doctorCallExpiresAt`, `doctorId`, `eventType`, `nurseCallExpiresAt`, `nurseStationClusterIds`, `occurredAt`, `queueCode`, `queueDate`, `queueId`, `queueNumber`, `queueStatus`, `queueStatusName`, `serviceUnitId` | `PASS` | `rt_mvp0.py` |
| `R-RP-04` c — petugas tetap menerima kedua antrean | 2 event lengkap | `PASS` | `rt_mvp0.py` |

Uji manual: layar display sungguhan diuji bersama `FE-RJP-001`.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-04` | Terpenuhi | a, b, c |
| `R-RP-05` | Terpenuhi | Penolakan hub |
| `UAT-RP-04` | Terpenuhi | Akun display ditolak |
| Rilis bersama `FE-RJP-001` tercatat | Terpenuhi | Roadmap dan laporan ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Temuan lama: `QueueHub.IsCurrentUserSuperAdmin` memakai `IsInRole("SuperAdmin")`/klaim peran. Tidak diubah (di luar scope) |
| Masalah yang diketahui | — |
| Risiko tersisa | Bila backend dirilis tanpa `FE-RJP-001`, display berhenti menerima realtime sampai frontend menyusul |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat ringkasan sesi |
| Langkah berikutnya | `FE-RJP-001` |
