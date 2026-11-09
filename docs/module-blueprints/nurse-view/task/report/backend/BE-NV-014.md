# Laporan Perubahan Backend — `BE-NV-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-014` |
| Judul | Sinyal realtime panggilan dan grup hub konsol serta tablet |
| Slice | `MVP-7` — `EPIC NV-06`, `NV-07`, `NV-08` (gelombang backend 6, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-014` |
| Trace | `FR-NV-048`; `NV-AC-028` (sisi server); `NV-DEC-044`, `047`; `VAL-NV-39` (hub), `41`; `NFR-008`, `015`; api §10; integration-contract R2 §8; `02-backend-architecture.md` R2.4.5, R2.5 |
| Contract version | `0.2.1` — `approved` (`NV-DEC-057`); api §10 tidak berubah sejak `0.2.0` (`NV-DEC-055`) |
| Dependency | `BE-NV-012` ✅, `BE-NV-013` ✅ (9 Nov 2026) |
| Klasifikasi | `MEDIUM` — skor 8: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 7 (1), logika sedang — pengiriman ke tiga grup, penolakan akun perangkat (1), memakai kontrak yang sudah ada (1), persistence yang sudah ada (1), keamanan berkaitan — grup hub berizin (1), satu workflow terbatas (1) |
| Task mode | `BACKEND` — perintah Rizki 9 Nov 2026 ("lanjutkan BE-NV-014 lalu ke frontend … build dan pengujian nyaa belakangan saja"), target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, `Hubs/QueueHub.cs` (berkas bersama, restu `NV-DEC-056`, baris tabel "Berkas bersama per task"), laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `589fb513` (branch `rizkiG`); perubahan `BE-NV-011`–`013` sebagian sudah di-stage Rizki, belum di-commit |
| Tanggal | 2026-11-09 |
| Status | 🟡 **SEBAGIAN 9 November 2026** — AC 6/6 terpetakan ke source; QBE Strict `PASS` (7 berkas, 0 violation, 0 review). `dotnet build` `PASS` 9 Nov 2026 menurut pernyataan Rizki — DLL 15.59, lebih baru dari source terakhir 14.49, memuat simbol baru task ini (`JoinNurseViewCallConsole`, `JoinNurseViewBedTablet`, `NotifyPatientCallChangedAsync`). Belum, dijadwalkan 10 Nov 2026 atas permintaan Rizki: uji hub (TV, konsol, tablet, serta regresi grup hub lama) |

---

## 1. Masalah yang diperbaiki

Sampai `BE-NV-013`, perubahan panggilan hanya terlihat bila layar memuat ulang sendiri: konsol
tiap 15 detik, strip TV tiap 60 detik, tablet tiap 15 detik selama ada panggilan aktif. Perawat
bisa terlambat sampai 15 detik melihat panggilan baru, dan tablet terlambat melihat bahwa
panggilannya sudah diklaim.

**Contoh.** 305-A menekan "Darurat" pukul 10.00.00. Tanpa task ini, konsol lantai 3 baru
menampilkannya paling lambat pukul 10.00.15. Dengan sinyal, konsol memuat ulang daftar dalam
hitungan detik.

---

## 2. Proses bisnis

**Tujuan:** setiap perubahan panggilan mengirim sinyal ringan tanpa identitas ke tiga penerima,
sehingga masing-masing memuat ulang bagiannya sendiri.

**Langkah normal:**

1. Perintah panggilan selesai disimpan: tekan, tekan ulang, naik Darurat, batal, perawat hadir
   (tablet); klaim, ambil alih, lepas, tutup (konsol); atau tutup sistem (pemeriksaan berkala).
2. Sesudah commit, `NvwRealtimeService.NotifyPatientCallChangedAsync` mengirim event
   `NurseViewPatientCallChanged` ke:
   - `nurse-view-floor:{lantai}` (TV lantai itu),
   - `nurse-view-call-console:{lantai}` (konsol lantai itu),
   - `nurse-view-bed-tablet:{TabletId}` untuk setiap tablet aktif di bed itu.
3. Isi event hanya `callId`, `bedId`, `floorCode`, `changeType`, `occurredAtUtc`.
4. Penerima memuat ulang: TV memuat strip, konsol memuat `GET /active`, tablet memuat `GET /current`.

| Kejadian | `changeType` |
| --- | --- |
| Tekan pertama | `Created` |
| Tekan lagi | `PressedAgain` |
| Naik Darurat | `Escalated` |
| Klaim / ambil alih / lepas | `Claimed` / `TakenOver` / `Released` |
| Perawat hadir di tablet, atau tutup dari konsol | `Handled` |
| Batal oleh pasien | `Cancelled` |
| Ditutup pemeriksaan berkala | `ClosedBySystem` |

**Masuk grup hub:**

| Method | Siapa yang boleh | Bila ditolak |
| --- | --- | --- |
| `JoinNurseViewCallConsole(floorCode)` | Akun manusia dengan `NurseViewPatientCall : Read`, lantai dikenal | Akun perangkat (display antrean, TV Nurse View, tablet) atau tanpa izin → `HubException` "Anda tidak memiliki akses ke konsol panggilan." (`VAL-NV-41`); lantai tidak dikenal → `HubException` "Lantai tidak dikenal. Pilih lantai dari daftar." (`VAL-NV-39`) |
| `LeaveNurseViewCallConsole(floorCode)` | Siapa pun yang tersambung | — |
| `JoinNurseViewBedTablet()` | Akun tablet aktif dengan sesi berlaku; tablet diambil dari akun, bukan parameter | `HubException` "Tablet ini tidak dapat dipakai. Hubungi IT." atau pesan sesi kedaluwarsa |
| `LeaveNurseViewBedTablet()` | Akun tablet | — |
| `JoinNurseViewFloor()` | Tidak diubah: hanya akun TV Nurse View aktif | Akun tablet ditolak karena bukan akun display (AC-4) |

**Jalur tidak normal:**

| Kejadian | Jawaban sistem |
| --- | --- |
| Pengiriman sinyal gagal (hub, koneksi, atau query tablet) | Perintah tetap berhasil (`200`); galat dicatat log `Error` dengan `CallId`, `BedId`, `FloorCode`, `ChangeType` (AC-5) |
| Klien memutus permintaan sesudah data tersimpan | Sinyal tetap dikirim (pengiriman memakai token pembatalan kosong) |
| Kirim ulang dengan `Idempotency-Key` yang sama | Tidak ada sinyal, karena tidak ada perubahan |
| Tablet bed diganti saat panggilan masih aktif | Sinyal dikirim ke tablet aktif bed itu sekarang, bukan hanya tablet yang membuat panggilan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: sama dengan `BE-NV-013` (`AGENTS.md`, suite `rules/backend/*`, kontrak engineering, registry baris 45)
- Blueprint: kartu `BE-NV-014`; api §10; validation `VAL-NV-39`, `41`; integration-contract R2 §8
- Source: `Hubs/QueueHub.cs` (pola `JoinAdmissionQueueBoard`, `RejectQueueDisplayDeviceAccount`, `JoinNurseViewFloor`); `NvwRealtimeService` (pola best-effort `NotifyVitalSignChangedAsync`); `NvwDeviceContextService`; `NvwBedTabletContextService`; `NvwPatientCallService`; `NvwPatientCallReconciliationService`; `NvwPatientCallQueryService.GetActiveForConsoleAsync`; klaim login di `AuthController` (`is_queue_display_account`, `is_nurse_view_account`, `is_nurse_view_bed_tablet_account`, `profile_type`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwRealtimeService.cs` | `PatientCallChangedEventName`, `BuildCallConsoleGroupName`, `BuildBedTabletGroupName`, `ChangeTypeFor`, `NotifyPatientCallChangedAsync` (satu `SendAsync` ke daftar grup; `try`/`catch` + log). `NotifyVitalSignChangedAsync` tidak berubah |
| `…/Services/NvwPatientCallService.cs` | Konstruktor menerima `NvwRealtimeService`; sinyal sesudah commit di `PressAsync`, `CloseFromTabletAsync`, dan `SaveConsoleAsync` (kini menerima jenis kejadian) |
| `…/Services/NvwPatientCallReconciliationService.cs` | Konstruktor menerima `NvwRealtimeService`; sinyal `ClosedBySystem` sesudah commit |
| `…/Services/NvwPatientCallQueryService.cs` | `IsKnownFloorAsync` publik; `GetActiveForConsoleAsync` memakainya (aturan sama, dipindah saja) |
| `…/Services/NvwResult.cs` | Kode dan pesan `VAL-NV-41` |
| `…/DTOs/NvwPatientCallDtos.cs` | `NvwPatientCallChangedEvent` |
| `Hubs/QueueHub.cs` | Berkas bersama — ringkasan di bawah |

Diff ringkas `Hubs/QueueHub.cs` (+88/−1, restu `NV-DEC-056`):

- `using …NurseViewManagement.Constants`.
- Konstruktor: +2 dependensi `NvwBedTabletContextService`, `NvwPatientCallQueryService` (baris −1
  adalah parameter terakhir lama yang kini diberi koma).
- +4 method publik `JoinNurseViewCallConsole`, `LeaveNurseViewCallConsole`,
  `JoinNurseViewBedTablet`, `LeaveNurseViewBedTablet`; +1 helper privat `IsDeviceAccount`.
- Method lain, termasuk `JoinNurseViewFloor`, `RejectQueueDisplayDeviceAccount`, dan seluruh grup
  antrean, **tidak** disentuh.

Nol baris komentar baru; ketujuh berkas CRLF tanpa BOM. Tanpa `Program.cs`: semua service sudah
terdaftar.

**Keputusan developer di luar teks kartu.**

| No | Keputusan | Alasan |
| ---: | --- | --- |
| 1 | Grup tablet = semua tablet **aktif** di bed panggilan, bukan `BedTabletId` pembuat panggilan | Panggilan melekat ke bed (`NV-DEC-050`); tablet pengganti tetap menampilkan panggilan itu lewat `GET /current` |
| 2 | Pemanggil HTTP mengirim sinyal dengan `CancellationToken.None`; pemeriksaan berkala memakai token host | Sinyal sesudah commit tidak boleh hilang hanya karena klien memutus permintaan |
| 3 | Penolakan akun perangkat memakai klaim login, bukan nama peran | Pola `RejectQueueDisplayDeviceAccount` (RJ-PRI-DEC-025); larangan hardcode role (`role-access-rules`) |
| 4 | Pemeriksaan izin konsol sebelum pemeriksaan lantai | Pengguna tanpa izin tidak bisa memetakan lantai yang dikenal |
| 5 | `LeaveNurseViewBedTablet` memakai klaim `nurse_view_bed_tablet_id`, tanpa query | Keluar grup tidak butuh validasi; meniru `LeaveNurseViewFloor` yang ringan |
| 6 | Tanpa sinyal untuk kirim ulang idempoten | Tidak ada perubahan data |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint tidak berubah. Hub bertambah 4 method dan 1 event sesuai api §10 |
| Database | Tanpa migration. Satu query baca `NvwBedTablet` per sinyal |
| Keamanan/Auth | Grup konsol berizin `NurseViewPatientCall : Read`; akun perangkat ditolak. Payload tanpa nama, inisial, No. RM, nama perawat, `CloseNote` (`NFR-015`) |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint HTTP baru. Permukaan realtime di `/hubs/queues`:

| Jenis | Nama | Keterangan |
| --- | --- | --- |
| Method hub | `JoinNurseViewCallConsole(string floorCode)` / `LeaveNurseViewCallConsole(string floorCode)` | Grup `nurse-view-call-console:{FloorCode}` |
| Method hub | `JoinNurseViewBedTablet()` / `LeaveNurseViewBedTablet()` | Grup `nurse-view-bed-tablet:{TabletId}` |
| Event | `NurseViewPatientCallChanged` | `{ "callId": "…", "bedId": "…", "floorCode": "3", "changeType": "Created", "occurredAtUtc": "…" }` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <7 berkas>` (array path) | Files evaluated **7** · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-09 |
| Review diff/scope; nol komentar; akhiran baris | Hanya berkas Nurse View + `QueueHub.cs`; method hub lama tidak berubah; CRLF | `PASS` | `git diff`, hitungan byte Node |
| Penelusuran source AC-1–6 | §6 | `PASS` | Source |
| `dotnet build` (oleh Rizki) | Berhasil menurut pernyataan Rizki 9 Nov 2026; agent memeriksa artefaknya: `bin/Debug/net9.0/QuilvianSystemBackend.dll` 15.59 (source terakhir 14.49) memuat `JoinNurseViewCallConsole`, `JoinNurseViewBedTablet`, `NotifyPatientCallChangedAsync` | `PASS` | Pernyataan Rizki; pencarian simbol di DLL |
| Uji hub (TV + konsol + tablet + regresi grup lama) | Ditunda atas permintaan Rizki | `NOT RUN` | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Rencana uji hub sesudah build** (pola `BE-NV-007`): satu TV lantai 3, satu TV lantai 4, satu
konsol Sari lantai 3, satu konsol Dewi lantai 4, satu tablet 326-C tersambung lewat klien SignalR
Node. (1) Tablet 326-C menekan → TV 3, konsol 3, tablet 326-C menerima `Created`; TV 4 dan konsol 4
tidak (AC-1). (2) Isi event hanya lima kunci (AC-2). (3) Akun TV dan tablet memanggil
`JoinNurseViewCallConsole` → `HubException` `VAL-NV-41`; `DOKTER` tanpa izin → `HubException`;
lantai `ZZ9` → `VAL-NV-39` (AC-3). (4) Tablet memanggil `JoinNurseViewFloor` → ditolak (AC-4).
(5) Klaim, lepas, tutup → `Claimed`, `Released`, `Handled`. (6) Regresi: TV tetap menerima
`NurseViewBoardChanged` saat TTV disimpan; `JoinAdmissionQueueBoard` dan `JoinQueueDisplayCluster`
tetap berperilaku sama (AC-6). (7) AC-5 lewat penelusuran source dan klien yang diputus mendadak.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 tekan 305-A → TV, konsol, tablet lantai 3 menerima `Created`; lantai 2 tidak | 🟡 Terpetakan ke source; runtime belum | `NotifyPatientCallChangedAsync` mengirim hanya ke grup lantai panggilan dan tablet bed itu; `ChangeTypeFor(Pressed) = Created` |
| AC-2 payload tanpa identitas | 🟡 Terpetakan ke source; runtime belum | `NvwPatientCallChangedEvent` hanya lima properti |
| AC-3 TV/tablet atau tanpa izin → `HubException` (`VAL-NV-41`) | 🟡 Terpetakan ke source; runtime belum | `JoinNurseViewCallConsole`: `IsDeviceAccount`, `HasAccessAsync(…, "NurseViewPatientCall", "Read")` |
| AC-4 tablet memanggil `JoinNurseViewFloor` → ditolak | 🟡 Terpetakan ke source; runtime belum | `JoinNurseViewFloor` tidak berubah; `ResolveActiveDeviceAsync` hanya mengenal `NvwDisplayDevice` |
| AC-5 sinyal gagal tidak menggagalkan perintah | 🟡 Terpetakan ke source; runtime belum | `try`/`catch` + `LogError` di `NotifyPatientCallChangedAsync`; dipanggil sesudah commit |
| AC-6 grup lain sama seperti sebelumnya | 🟡 Terpetakan ke source (diff hanya menambah); runtime belum | Diff `QueueHub.cs` §3.2 |
| DoD — AC-1–6 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — build Rizki lulus | Terpenuhi 9 Nov 2026 | Pernyataan Rizki; DLL 15.59 memuat simbol baru |
| DoD — log uji hub tersimpan | **Belum** (ditunda) | — |
| DoD — laporan | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001`, `QBE-API-001` (hub mengikuti pola grup yang
ada), `QBE-PERM-001` (izin dari layar Akses Role, tanpa hardcode peran), `QBE-DTO-001` (event
DTO, bukan entity), `QBE-VAL-001` (lantai dikenal), `QBE-LOG-001` (galat sinyal tercatat).
Tidak berlaku: `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*`, `QBE-PAGE-001`, `QBE-OPT-001`,
`QBE-DEL-001`, `QBE-TXN-001` (sinyal sengaja di luar transaksi).

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` (registry baris 45) · applicability `NEW CODE`, ditambah `TOUCHED LEGACY` untuk
`Hubs/QueueHub.cs` · branch `rizkiG`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Konstruktor `QueueHub` bertambah dua dependensi; build akan langsung menunjukkan bila ada masalah DI. Konstruktor `NvwPatientCallService` dan `NvwPatientCallReconciliationService` juga berubah |
| Masalah yang diketahui | Grup hub milik koneksi SignalR, bukan sesi: bila tablet dinonaktifkan saat masih tersambung, koneksinya tetap di grup sampai terputus, tetapi semua aksinya ditolak dan event-nya tanpa identitas |
| Risiko tersisa | Regresi grup hub lain (syarat lead `NV-DEC-030`/`056`) — diff tidak mengubah method lama, tetapi baru terbukti sesudah uji hub |
| Perubahan sampingan | `NONE` |
| Interupsi | Tidak ada |
| Status Git | `M` `QueueHub.cs`, `NvwRealtimeService.cs`; `MM` berkas `BE-NV-011`/`012`/`013` yang sudah di-stage Rizki; `??`/`A` laporan ini |
| Langkah berikutnya | Frontend `FE-NV-010` (klien realtime) memakai permukaan ini. Build backend dan uji hub dijalankan bersama uji frontend, sesuai permintaan Rizki |
