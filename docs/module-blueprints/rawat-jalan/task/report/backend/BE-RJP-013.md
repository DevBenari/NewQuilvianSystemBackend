# Laporan Perubahan Backend — `BE-RJP-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-013` |
| Judul | Ganti tanggal reservasi |
| Slice | `MVP-3`, `EPIC RP-09` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` r2, bagian `BE-RJP-013` |
| Trace | `FR-RP-090` s.d. `096`; `RJ-PRI-DEC-034` s.d. `039`; `INV-RP-01`, `03` (revisi `34`), `05`, `06`, `10`; `VAL-RP-03` s.d. `06`, `11`, `12`, `14`, `22` s.d. `25` |
| Contract version | `RJ-PRI-RESV-001@1.2.0` `approved` — `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `integration-contract.md`, `permission-audit-matrix.md` bagian *Amendment RP-GT* |
| Desain | `02-backend-architecture.md` GT.1–GT.14; `data/data-dictionary.md` RP-GT |
| Dependency | `BE-RJP-007` ✅, `BE-RJP-010` ✅ |
| Klasifikasi | `HIGH` — skor 9: diperiksa 9–20 (1), diubah 8 (1), logika tinggi (2: urutan pemeriksaan, kunci pasien, konkurensi), kontrak baru (2), keamanan berkaitan (1: kode link), workflow (2) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-039`) |
| Target tulis | 8 berkas source + migration (lihat 3.1) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b0285ff1` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-21 |
| Status | ✅ Selesai — runtime 23/23 `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE / LEGACY`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:23`) |
| Keberlakuan | `NEW CODE` (method, DTO, dan kolom baru di aggregate RP yang sudah patuh). Tidak ada entity atau prefix baru |
| Arketipe endpoint | Transaksi ber-lifecycle: aksi `POST /{id}/reschedule`. Tidak ada `PUT /{id}` umum, `PATCH /status`, maupun `DELETE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-TXN-001`, `QBE-CFG-001`, `QBE-ENT-001` (entity yang disentuh tetap mewarisi `IdentityModel`) |
| Role access | Tidak ada butir baru. Action `Reschedule` memakai `[AccessAction("Create", …)]` + `[AccessPermission("OutpatientPriorityReservation", "Create")]`, sama persis dengan `ControllerName` dan aksi `Create` yang sudah ada (`RJ-PRI-DEC-037`). Deskripsi ketiga `[AccessAction("Create")]` diseragamkan. Tidak ada pemeriksaan nama peran |

---

## 1. Masalah yang diselesaikan

Sebelum task ini, pasien yang ingin pindah tanggal harus dibatalkan lalu dibuatkan reservasi baru (`RJ-PRI-DEC-029`).
Akibatnya, reservasi lama sudah hilang bila reservasi baru ditolak. Reservasi lama juga dapat tertinggal dan tercatat
Tidak Datang bila admisi lupa membatalkannya (`F-GT-2`, `F-GT-3`).

## 2. Proses bisnis

1. Admisi membuka detail reservasi yang masih Menunggu Kedatangan, sebelum hari kunjungan.
2. Admisi mengirim tanggal baru dan alasan.
3. Sistem memeriksa, berurutan: fitur aktif; isian lengkap; reservasi ada; versi data cocok; status; hari ini sebelum
   tanggal lama; tanggal baru tidak sama; jendela tanggal; status member hari ini; lalu, di bawah kunci pasien, reservasi
   terbuka dan kunjungan aktif pada tanggal baru.
4. Bila semua lolos, sistem mengganti tanggal dan ketujuh kolom snapshot, menerbitkan kode link baru, dan menulis riwayat
   `RESCHEDULED` dengan tanggal lama dan baru. Semuanya dalam satu transaksi.
5. Admisi menerima link baru dan teks WA versi ganti tanggal, sekali saja.

Bila satu pemeriksaan gagal, tidak ada yang berubah.

Contoh dari uji: reservasi `RSV-…` untuk 22 Okt 2026 dipindah pada 21 Okt ke 25 Okt. Nomornya tetap. Link lama menjawab
404, dan link baru menjawab "Konfirmasi baru bisa dilakukan" untuk 25 Okt. Riwayatnya bertambah "Tanggal kunjungan diganti
22 Okt → 25 Okt" atas nama petugas. Layar admisi yang tersambung menerima kabar dengan kedua tanggal.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Models/RegPriorityReservationHistory.cs` | Diperbarui | Konstanta `EventRescheduled = "RESCHEDULED"`; kolom `FromVisitDate`, `ToVisitDate` (`DateTime?`) |
| `Repositories/Configurations/HealthServices/RegPriorityReservationHistoryConfiguration.cs` | Diperbarui | Kedua kolom bertipe `date`, nullable, tanpa index |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `RescheduleAsync` (urutan GT.4, transaksi sendiri, `LockPatientAsync`, `RowVersion`, tangkapan unique violation); `CanReschedule` dan tanggal riwayat di detail; nama kejadian; teks WA versi ganti tanggal; `PreviousVisitDate` pada hasil perintah; `HistoryEntry` menerima tanggal |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientPriorityReservationController.cs` | Diperbarui | Action `Reschedule` → `POST /{id}/reschedule`; kabar realtime meneruskan `PreviousVisitDate`; deskripsi `[AccessAction("Create")]` diseragamkan |
| `Areas/HealthServices/RegistrationManagement/Services/QueueRealtimeService.cs` | Diperbarui | `NotifyPriorityReservationChangedAsync` menerima `previousVisitDate` opsional. Dua pemanggil lain tidak berubah |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientPriorityReservationDtos.cs` | Diperbarui | DTO baru `ReschedulePriorityReservationRequest`; `CanReschedule`; `FromVisitDate`/`ToVisitDate` pada riwayat |
| `Areas/HealthServices/RegistrationManagement/DTOS/QueueRealtimeDtos.cs` | Diperbarui | `PreviousVisitDate` pada `PriorityReservationRealtimeEventResponse` |
| `Migrations/20261021082302_AddPriorityReservationHistoryVisitDates.cs` (+ `.Designer.cs`) | Baru | `ADD COLUMN "FromVisitDate" date NULL`, `"ToVisitDate" date NULL`; `Down` menghapus keduanya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui | Hanya dua properti baru (6 baris) |

### 3.2 Aturan yang diterapkan

| Aturan | Penerapan |
| --- | --- |
| `VAL-RP-22` | Hari ini ≥ tanggal kunjungan: `409` "Tanggal reservasi tidak bisa diganti pada hari kunjungan. Batalkan reservasi lalu buat reservasi baru." |
| `VAL-RP-23` | Tanggal baru = tanggal lama: `400` |
| `VAL-RP-24` | `newVisitDate` kosong: `400` "Tanggal kunjungan baru wajib diisi." |
| `VAL-RP-25` | Bukan member Aktif hari ini: `409`, pesan menyebut tanggal lama tetap berlaku |
| `VAL-RP-03`/`04`/`05`/`06`/`11`/`12`/`14` | Dipakai ulang dengan kalimat yang sama |
| `INV-RP-01` | Pemeriksaan reservasi terbuka di bawah kunci pasien; unique index `UX_RegPriorityReservation_Open_Patient_VisitDate` menjadi lapis kedua |
| `INV-RP-03` (revisi `34`) | Snapshot hanya diganti oleh `RescheduleAsync` |
| `INV-RP-05` | Kode baru hanya di respons; tersimpan sebagai hash |
| `INV-RP-06` | Satu riwayat `RESCHEDULED` per penggantian, dalam transaksi yang sama |
| `INV-RP-10` | Status Menunggu Kedatangan dan hari ini < tanggal lama |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: satu endpoint dan empat field baru. Klien lama tetap jalan |
| Database | Dua kolom nullable di `RegPriorityReservationHistory`. Migration diterapkan ke `QuilvianNewDevSukma` atas `RJ-PRI-DEC-039`, sesudah `dotnet ef migrations list` memastikan tidak ada migration modul lain yang `(Pending)` |
| Keamanan | Log hanya `EntityId`, nomor, status, controller, aksi. Kode, hash, alasan, dan tanggal tidak dicatat (terbukti `R-RP-39`) |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Outpatient Priority Reservation

Base URL: `api/v1/health-services/registration-management/outpatient-priority-reservations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/reschedule` | Ganti tanggal sebelum hari kunjungan | `OutpatientPriorityReservation : Create` | `{ "expectedRowVersion": "…", "newVisitDate": "2026-10-28", "reason": "Pasien ada acara keluarga pada tanggal semula" }` | `ApiResponse<PriorityReservationIssuedResponse>` | Tersedia |
| `GET` | `/{id}` | Detail (+ `canReschedule`, riwayat + `fromVisitDate`/`toVisitDate`) | `OutpatientPriorityReservation : Read` | — | `ApiResponse<PriorityReservationDetailResponse>` | Diperbarui |

Kode status `reschedule`: `200` berhasil; `400` isian atau tanggal tidak sah; `403` tanpa `Create`; `404` reservasi tidak
ada; `409` status, hari kunjungan, member, tanggal terisi, versi, atau fitur nonaktif. Event hub `PriorityReservationChanged`
menambah `previousVisitDate` (hanya pada `RESCHEDULED`).

Delta terhadap kontrak: tidak ada. Penolakan `expectedRowVersion` kosong (`400` "Versi data reservasi wajib dikirim.")
sama dengan perintah petugas lain (`BE-RJP-007`).

## 5. Verifikasi

| Pemeriksaan | Perintah | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| Build | `dotnet build QuilvianSystemBackend.csproj -c Release -p:UseSharedCompilation=false -o <scratchpad>/be-build` | 0 error, 296 warning; tidak ada warning di berkas yang disentuh | `PASS` |
| Migration pending modul lain | `dotnet ef migrations list --configuration Release` | Tidak ada `(Pending)` | `PASS` |
| Model vs migration | `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." | `PASS` |
| Penerapan DB uji | `dotnet ef database update --configuration Release` (`QuilvianNewDevSukma`) | `Applying migration '20261021082302_AddPriorityReservationHistoryVisitDates'. Done.` Kolom `FromVisitDate`/`ToVisitDate` `date`, nullable | `PASS` |
| QBE Strict | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <8 berkas> -Mode Strict` | VIOLATION 0, REVIEW 0, Final result `PASS` | `PASS` |
| `dotnet test` | — | Tidak ada project test | `NOT RUN` |

Runtime HTTP ke backend uji `https://localhost:7185` (build scratchpad, `Development`) terhadap `QuilvianNewDevSukma`.
Data samaran: delapan pasien `RJPR UJI G1`..`G8` (suffix `5917`) dengan membership Gold yang disisipkan SQL data uji.
Tanggal relatif `H+n` dari hari ini (21 Okt 2026).

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `R-RP-31` a | `H+1 → H+4`: `200`, tanggal baru, nomor dan status tetap, `rowVersion` baru, `canReschedule` tetap `true` | `PASS` |
| `R-RP-31` b | Link baru 43 karakter, teks WA "sudah diganti … tidak berlaku lagi", tanpa poli/dokter | `PASS` |
| `R-RP-31` c | Riwayat `RESCHEDULED`: status 1→1, sumber Staff, `fromVisitDate`/`toVisitDate`, alasan, nama pelaku | `PASS` |
| `R-RP-31` d | Daftar: tidak lagi di `H+1`, tampil di `H+4` | `PASS` |
| `R-RP-37` | Link lama `404`; link baru `200 NOT_YET_OPEN` tanggal baru; hash DB = SHA-256 kode baru, 64 hex, berbeda | `PASS` |
| `R-RP-32` | Reservasi hari ini: `409` `VAL-RP-22`, `canReschedule = false`, baris tidak berubah | `PASS` |
| `R-RP-33` | Reservasi Dibatalkan: `409` `VAL-RP-11`, `canReschedule = false` | `PASS` |
| `R-RP-34` a–f | Kemarin `400`; `H+31` `400` "paling jauh 30 hari … 20 November 2026"; tanggal sama `400`; tanggal dengan reservasi terbuka `409` menyebut nomornya; tanggal dengan kunjungan aktif `409`; tanggal kosong `400` | `PASS` |
| `R-RP-34` tetap | Keenam penolakan: tanggal, hash, snapshot, `rowVersion`, status tetap; tanpa riwayat `RESCHEDULED` | `PASS` |
| `R-RP-35` | Membership Kedaluwarsa: `409` `VAL-RP-25` menyebut 22 Oktober 2026; reservasi dan link lama tetap (`NOT_YET_OPEN`) | `PASS` |
| `R-RP-36` | Snapshot dirusak (SQL data uji), lalu diganti: ketujuh kolom sama dengan klasifikasi hari ini | `PASS` |
| `R-RP-38` a | Alasan 6 karakter: `400` | `PASS` |
| `R-RP-38` b | Akun uji `Read` + `Screen`: ganti tanggal `403`, detail `200`, reservasi tidak berubah | `PASS` |
| Pembersihan akun uji | Kebijakan pasangan uji 0, akun uji nonaktif | `PASS` |
| `R-RP-40` | Dua permintaan paralel: `[200, 409]`, satu riwayat `RESCHEDULED` | `PASS` |
| `R-RP-42` | Kirim ulang tetap memakai teks WA biasa; tandai tiba hari H tetap jalan dan `canReschedule = false`; reservasi baru untuk member Kedaluwarsa tetap `409` (`R-RP-24`) | `PASS` |
| `R-RP-41` | Klien SignalR board: satu event `RESCHEDULED` dengan `visitDate` baru dan `previousVisitDate` lama, tanpa nama pasien; 12 event lain dengan `previousVisitDate` kosong | `PASS` |
| `R-RP-39` | Riwayat lengkap (31 c). Log berkas dan konsol tanpa 24 nilai rahasia (kode, hash) dan 15 alasan uji | `PASS` |

Total 23/23 `PASS`. Skrip: `rt_rjp013.py`, `sr_one.cjs`, beserta helper `rt_common.py`, `rjp_common.py`, `rjp_users.py`,
dan `dbq.py` di scratchpad sesi.

Percobaan pertama berhenti sebelum pemeriksaan pertama karena helper uji membaca kolom `date` sebagai objek Python
(galat skrip, bukan backend). Skrip diperbaiki dengan `::text`, lalu dijalankan ulang dari awal dengan pasien baru.

Keadaan DB uji sesudah run: pasien berawalan `RJPR UJI G` bertambah 16 (8 dari percobaan pertama dan 8 dari run penuh).
Reservasi mereka: 10 Menunggu Kedatangan, 1 Sudah Tiba, 1 Dibatalkan, ditambah sisa run lain. Riwayat `RESCHEDULED` = 3.
Kunjungan uji = 3. Semua data samaran dan akan tertutup otomatis sebagai Tidak Datang setelah tanggalnya lewat.

Belum tercakup di task ini: sisi layar `R-RP-41` (baris hilang dari layar tanggal lama) menjadi milik `FE-RJP-009`.
Penutupan akhir hari tidak diuji ulang, karena kodenya tidak disentuh dan himpunannya tidak beririsan dengan ganti
tanggal (`02` GT.2).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 23 / `R-RP-31` | Terpenuhi | a–d |
| AC 24 / `R-RP-32` | Terpenuhi | Uji |
| AC 25 / `R-RP-33` | Terpenuhi | Uji |
| AC 26 / `R-RP-34` | Terpenuhi | a–f, tetap |
| AC 27 / `R-RP-35` | Terpenuhi | Uji |
| AC 28 / `R-RP-36` | Terpenuhi | Uji |
| AC 29 / `R-RP-37` | Terpenuhi | Uji |
| AC 30 / `R-RP-38` | Terpenuhi, termasuk `403` dengan akun non-SuperAdmin | a, b |
| AC 31 / `R-RP-39` | Terpenuhi | 31 c, pemeriksaan log |
| AC 32 / `R-RP-40` | Terpenuhi | Uji paralel |
| AC 33 / `R-RP-41` | Terpenuhi sisi backend; sisi layar di `FE-RJP-009` | Klien SignalR |
| Regresi / `R-RP-42` | Terpenuhi | Uji |
| Migration dibuat dan diterapkan ke DB uji | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Risiko tersisa | Migration belum diterapkan ke DB master/staging/production; diterapkan saat rilis. Frontend belum memakai endpoint ini sampai `FE-RJP-009` |
| Perubahan sampingan | `dotnet ef migrations add` ikut menata ulang dua blok tak berkaitan di snapshot (`PhmPrescriptionStockReservation`: urutan `HasIndex` dan `HasOne`). Keduanya dikembalikan ke urutan semula, sehingga diff snapshot hanya 6 baris. `.Designer.cs` migration baru adalah keluaran tool apa adanya. Akun uji `TEST-RJP Uji resched` tersisa dalam keadaan nonaktif |
| Interupsi | — |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `FE-RJP-009` (dialog Ganti Tanggal, riwayat, realtime dua tanggal) |
