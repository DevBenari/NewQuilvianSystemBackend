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
| Status | ✅ **SELESAI 10 November 2026** — AC 6/6 terpetakan ke source dan terbukti runtime. QBE Strict `PASS` (7 berkas); build `PASS` menurut pernyataan Rizki (DLL 9 Nov 15.59). Uji hub 10 Nov 2026 08.44–08.45 WIB, H1–H9 9/9 `PASS` (bagian dari uji gabungan 20/20 bersama `BE-NV-015`): sinyal hanya ke TV, konsol, dan tablet bed di lantai panggilan, tiba sebelum respons HTTP selesai terbaca; tepat lima kunci tanpa identitas; TV, tablet, dan DOKTER ditolak grup konsol (`VAL-NV-41`); tablet ditolak grup papan; tujuh jenis perubahan terkirim; perintah tetap `200` tanpa penerima; regresi TTV, antrean admisi, dan display antrean sama. Log: nol rahasia dan token; 10 galat = 10 penolakan hub yang memang diuji. `AUTOMATED TEST: NOT APPLICABLE`. Riwayat: 🟡 SEBAGIAN 9 Nov 2026 sebelum build dan uji |

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
| Uji hub gabungan `uji-sinyal-strip.mjs` (klien SignalR Node, sepuluh sambungan) | H1–H9 9/9 lulus, 10 Nov 2026 08.44–08.45 WIB | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-014-015-20261110/hasil-2026-11-10T01-44-39-831Z/` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Hasil uji hub — 10 November 2026** (skrip `uji-be-nv-014-015-20261110/uji-sinyal-strip.mjs`, klien
SignalR Node dengan token akun; satu putaran bersama uji HTTP `BE-NV-015`; bukti `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-014-015-20261110/hasil-2026-11-10T01-44-39-831Z/`)

| No | Skenario | Hasil |
| --- | --- | --- |
| H1 | Sepuluh sambungan (TV 3 `NVW-000002`, TV 4 `NVW-000001`, konsol Sari lantai 3, konsol Dewi lantai 4, empat tablet uji, DOKTER, SuperAdmin) `Connected`; setiap join sesuai akun berhasil | `PASS` |
| H2 | AC-3: TV, tablet, DOKTER memanggil `JoinNurseViewCallConsole("3")` → "Anda tidak memiliki akses ke konsol panggilan." (`VAL-NV-41`); Sari dengan `ZZ9` dan kosong → "Lantai tidak dikenal. Pilih lantai dari daftar." (`VAL-NV-39`) | `PASS` |
| H3 | AC-4: tablet `JoinNurseViewFloor` → "Akun ini bukan akun display Nurse View."; TV dan Sari `JoinNurseViewBedTablet` → "Tablet ini tidak dapat dipakai. Hubungi IT." | `PASS` |
| H4 | AC-1: tablet 325-A menekan → TV 3, konsol 3, tablet 325-A masing-masing tepat satu `Created`; TV 4, konsol 4, tiga tablet lain nol. Sinyal tiba 0,01 detik sebelum respons HTTP selesai terbaca (`NFR-008` sisi server) | `PASS` |
| H5 | AC-2: sembilan payload, semuanya tepat `callId`, `bedId`, `floorCode`, `changeType`, `occurredAtUtc`; nama lengkap dan No. RM lima pasien nol | `PASS` |
| H6 | `PressedAgain`, `Escalated`, `Claimed`, `TakenOver`, `Released`, `Handled` (tablet), `Handled` (konsol): masing-masing tepat satu di TV 3 dan konsol 3, nol di lantai 4; tablet bed bersangkutan ikut menerima | `PASS` |
| H7 | AC-1 arah sebaliknya: 420-A `Created` lalu `Cancelled` hanya ke TV 4, konsol 4, tablet 420-A | `PASS` |
| H8 | AC-6 regresi: TTV 325-B disimpan → `NurseViewBoardChanged` tepat satu di TV 3, nol di TV 4, konsol, tablet; TTV dibatalkan. `JoinAdmissionQueueBoard` SuperAdmin diterima, TV ditolak pesan lama; `JoinQueueDisplayCluster(Guid kosong)` → "Nurse station cluster tidak valid." | `PASS` |
| H9 | AC-5 padanan (cara verifikasi kartu, seperti `BE-NV-007` H10): TV 3, konsol 3, tablet 326-C diputus; "Perawat hadir" 326-C tetap `200`, `Ditangani`, nol panggilan aktif | `PASS` |

Pembersihan: empat tablet uji (`NVT-000023`–`026`) dinonaktifkan; policy jabatan Sari (20 izin) dan
Dewi (36 izin) dipulihkan identik; nol panggilan aktif lantai 3 dan 4; TTV uji dibatalkan. Login TV
`NVW-000002` dan `NVW-000001` diatur ulang sebagai persiapan. Pindai log jendela uji: 127 baris, 13
jejak aksi panggilan sesuai urutan uji, nol kemunculan 20 rahasia (sandi akun, sandi tablet dan TV,
nama lengkap dan No. RM pasien), nol token; 10 galat `DefaultHubDispatcher` sama dengan 10 penolakan
hub yang diuji, nol galat `NvwRealtimeService`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 tekan 305-A → TV, konsol, tablet lantai 3 menerima `Created`; lantai 2 tidak | ✅ Terbukti 10 Nov 2026 | Uji H4 (325-A → TV 3, konsol 3, tablet 325-A; lantai 4 nol) dan H7 (arah sebaliknya) |
| AC-2 payload tanpa identitas | ✅ Terbukti 10 Nov 2026 | Uji H5: sembilan payload tepat lima kunci; identitas nol |
| AC-3 TV/tablet atau tanpa izin → `HubException` (`VAL-NV-41`) | ✅ Terbukti 10 Nov 2026 | Uji H2: TV, tablet, DOKTER → `VAL-NV-41`; lantai tak dikenal → `VAL-NV-39` |
| AC-4 tablet memanggil `JoinNurseViewFloor` → ditolak | ✅ Terbukti 10 Nov 2026 | Uji H3 |
| AC-5 sinyal gagal tidak menggagalkan perintah | ✅ Terbukti lewat padanan dan source, sesuai cara verifikasi kartu | Uji H9 (penerima terputus → `200`); `try`/`catch` + `LogError` di `NotifyPatientCallChangedAsync`, dipanggil sesudah commit |
| AC-6 grup lain sama seperti sebelumnya | ✅ Terbukti 10 Nov 2026 | Uji H8: `NurseViewBoardChanged` saat TTV disimpan, `JoinAdmissionQueueBoard`, `JoinQueueDisplayCluster`; diff `QueueHub.cs` hanya menambah |
| DoD — AC-1–6 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — build Rizki lulus | Terpenuhi 9 Nov 2026 | Pernyataan Rizki; DLL 15.59 memuat simbol baru |
| DoD — log uji hub tersimpan | Terpenuhi 10 Nov 2026 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-014-015-20261110/hasil-2026-11-10T01-44-39-831Z/` (`hasil.json`, `percobaan.json` 69 permintaan) |
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
| Risiko tersisa | Rendah. Regresi grup hub lain sudah diuji (H8); grup display antrean hanya diuji jalur validasinya (Guid kosong), bukan pengiriman ke layar antrean nyata |
| Perubahan sampingan | `NONE` |
| Interupsi | Tidak ada |
| Status Git | Source dan laporan sudah di-commit Rizki (`7cff2991`, `4a6ac9a3`); pembaruan hasil uji ini `M` |
| Langkah berikutnya | Uji layar frontend: regresi TV `FE-NV-010`, tablet `FE-NV-011`, konsol `FE-NV-012`, strip TV `FE-NV-014` memakai sinyal yang sudah terbukti ini |
