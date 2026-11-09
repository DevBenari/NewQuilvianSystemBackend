# Laporan Perubahan Backend — `BE-NV-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-012` |
| Judul | Konsol Panggilan: daftar dan aksi perawat |
| Slice | `MVP-7` — `EPIC NV-06`, `NV-08` (gelombang backend 5, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-012` |
| Trace | `FR-NV-039`, `041`, `044` (kejadian perawat), `049`, `050`; `NV-DEC-038`, `040`, `043`, `044`, `053`; `AD-06`; `VAL-NV-33`–`39`; state §5; `contracts/api-contract.md` R2 §7; permission-audit R2 §6–§10; `NFR-011`, `013`, `014`, `015` |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`) |
| Dependency | `BE-NV-011` ✅ (9 Nov 2026), `BE-NV-016` ✅ (6 Nov 2026) |
| Klasifikasi | `MEDIUM` — skor 8: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 6 (1), logika kompleks — klaim tunggal, token versi, penyamaran identitas (2), endpoint baru sesuai kontrak (1), persistence yang sudah ada (1), keamanan berkaitan — resource izin baru dan data pasien tersamar (1), UI tidak ada (0) |
| Task mode | `BACKEND` — perintah Rizki 9 Nov 2026 ("lanjutkan" sesudah `BE-NV-011` ✅; gelombang 5 boleh paralel, task ini dipilih karena berada di jalur `BE-NV-014` dan `FE-NV-012`), target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, laporan ini, baris status roadmap dan traceability. Tanpa berkas bersama |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `589fb513` (branch `rizkiG`); perubahan `BE-NV-011` sudah di-stage Rizki, belum di-commit |
| Build saat diuji | DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 9 Nov 2026 09.51, lebih baru dari source terakhir (08.51); memuat `NvwPatientCallController`, `GetActiveForConsoleAsync`, `CloseFromConsoleAsync`, `NvwConsoleCloseRequest`, `NurseViewPatientCall` |
| Tanggal | 2026-11-09 |
| Status | ✅ **SELESAI 9 November 2026** — AC 10/10 terpetakan ke source dan terbukti runtime. QBE Strict `PASS` (6 berkas); pasangan atribut akses 6/6; build `PASS` menurut pernyataan Rizki; baris izin `NurseViewPatientCall` Read/Update terbit saat startup. Uji HTTP **17/17 `PASS`**: 74 permintaan, sama dengan log. Nol kata sandi, token, identitas pasien, dan keterangan tutup di log. Kedua policy jabatan uji dipulihkan identik. Riwayat: 🟡 SEBAGIAN 9 Nov 2026 pagi sebelum build dan uji |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-NV-011`, pasien sudah bisa memanggil dari tablet, tetapi belum ada yang bisa
melihat panggilannya. Perawat tidak punya daftar panggilan per lantai dan tidak bisa menyatakan
"saya tangani".

**Contoh.** Pasien 331-C menekan "Darurat" pukul 10.00.40. Tanpa task ini, panggilan itu
tersimpan di database tetapi tidak muncul di mana pun.

---

## 2. Proses bisnis

**Tujuan:** perawat berizin melihat panggilan aktif satu lantai, dengan Darurat paling atas lalu
yang paling lama menunggu. Satu panggilan paling banyak punya satu pengklaim, dan setiap
perpindahan klaim tercatat.

**Langkah normal:**

1. Perawat membuka Konsol Panggilan dengan akunnya sendiri dan memilih lantai (`GET /floors`).
2. `GET /active?floorCode=3` mengembalikan panggilan `Waiting`/`Claimed` lantai itu. Setiap
   item memuat bed, kamar, inisial · jenis kelamin · umur pasien, prioritas, lama menunggu dari
   jam server, pengklaim, tingkat warna, dan aksi yang tersedia.
3. Sari menekan "Saya tangani" (`POST /{id}/claim`): status `Claimed`, jejak `Claimed`.
4. Bila Sari berhalangan, Dewi menekan "Ambil alih" (`POST /{id}/take-over`). Pengklaim
   berganti, dan jejak `TakenOver` mencatat Sari sebagai pengklaim sebelumnya.
5. Pengklaim boleh melepas (`POST /{id}/release`): kembali `Waiting`, jam awal tetap.
6. Panggilan selesai lewat "Perawat hadir" di tablet (`BE-NV-011`), atau lewat "Tutup panggilan"
   di konsol dengan alasan (`POST /{id}/close`): `Handled`, `IsBedsideConfirmed = false`, jejak
   `ClosedFromConsole`.

**Contoh urutan (AC-1).** 305-A memanggil biasa 10.00.00, 318-B biasa 10.00.20, 331-C Darurat
10.00.40. Pukul 10.01.00 daftar berurutan 331-C (Darurat), 305-A (60 detik), 318-B (40 detik).

**Aksi yang tersedia per item** (dihitung backend, `AvailableActions`):

| Keadaan | Pengguna berizin `Update` | Pengguna hanya `Read` (`AD-06`) |
| --- | --- | --- |
| `Waiting` | `Claim`, `Close` | — |
| `Claimed` oleh saya | `Release`, `Close` | — |
| `Claimed` oleh orang lain | `TakeOver`, `Close` | — |

**Jalur tidak normal:**

| Kejadian | Jawaban sistem | Kode |
| --- | --- | --- |
| Klaim panggilan yang sudah diklaim | "Panggilan sudah diambil ‹nama›. Gunakan Ambil alih bila perlu." (`VAL-NV-33`) | `409` |
| Ambil alih panggilan `Waiting`, atau klaim sendiri; lepas panggilan `Waiting` | "Panggilan ini tidak sedang diklaim perawat lain." (`VAL-NV-34`) | `409` |
| Lepas klaim milik orang lain | "Hanya perawat yang mengklaim yang bisa melepas panggilan ini." (`VAL-NV-35`) | `403` |
| Tutup tanpa alasan, atau alasan di luar 2–4 | "Pilih alasan menutup panggilan." (`VAL-NV-36`) | `400` |
| "Lainnya" dengan keterangan < 5 atau > 250 karakter, atau keterangan > 250 karakter | "Keterangan wajib diisi untuk alasan Lainnya (5–250 karakter)." (`VAL-NV-36`) | `400` |
| Tindakan apa pun pada panggilan selesai | "Panggilan ini sudah selesai." (`VAL-NV-37`) | `409` |
| `Version` lama, atau kalah klaim bersamaan | "Panggilan sudah berubah. Daftar dimuat ulang." (`VAL-NV-38`) | `409` |
| Lantai kosong atau tanpa kamar aktif | "Lantai tidak dikenal. Pilih lantai dari daftar." (`VAL-NV-39`) | `422` |
| Panggilan tidak ada | "Panggilan tidak ditemukan." | `404` |
| Pengguna tanpa izin `NurseViewPatientCall` | Ditolak mesin hak akses | `403` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: sama dengan `BE-NV-011` (`AGENTS.md`, suite `rules/backend/*`, kontrak engineering,
  registry baris 45)
- Blueprint: kartu `BE-NV-012`; api R2 §7; validation `VAL-NV-31`–`41`; state §5;
  `02-backend-architecture.md` R2.1, R2.4.5, R2.4.6, R2.10, R2.11; permission-audit R2 §6–§10;
  kamus data `NvwPatientCall`, `NvwPatientCallEvent`
- Source: `NurseViewPermissions`, `NvwPatientMasking` (format inisial `R.A.`, kosong `–`),
  `NvwFloorBoardQueryService` (pola penyamaran di memori dan `AppDateTimeHelper.OperationalDate`),
  `NvwPatientCallWaitThresholdService` (`GetActiveAsync`, `ResolveWaitLevel`),
  `NvwDisplayDeviceService` (`GetFloorsAsync`, pemeriksaan lantai dikenal), `NvwBedTabletController`
  dan `NvwPatientCallWaitThresholdController` (pola `[AccessController]`/`[AccessAction]`/log),
  `AccessPermissionService.HasAccessAsync` (lolos untuk peran SuperAdmin selama
  `EnforceClinicalPolicyForSuperAdmin = false`), `NvwHttp`
- DB dev (SELECT read-only): ambang aktif 0; bed berpenghuni lantai 3 dan 4 untuk uji; jabatan
  akun uji

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwPatientCallController.cs` | **Baru.** 6 endpoint api §7. `[AccessController]` modul `HEALTH_SERVICE_NURSE_VIEW`, resource `NurseViewPatientCall`, `SortOrder = 4`; `[AccessAction]` + `[AccessPermission]` `Read` (2 endpoint) dan `Update` (4 endpoint). Log aksi lewat `NvwHttp.RespondAsync` di teks pesan: Id panggilan, kode bed, aksi, status, prioritas, pengklaim. Tanpa identitas pasien dan tanpa `CloseNote` |
| `…/DTOs/NvwPatientCallDtos.cs` | **Baru.** `NvwConsoleCallListResponse`, `NvwConsoleCallResponse` (dengan `NvwMaskedPatientResponse` yang sudah ada), `NvwConsoleCallVersionRequest`, `NvwConsoleCloseRequest` |
| `…/Services/NvwPatientCallQueryService.cs` | `GetActiveForConsoleAsync` (lantai dikenal, ambang aktif, urutan `Priority` turun lalu `FirstPressedAt` naik, satu query per jenis data: bed, episode, pengklaim). Inisial, jenis kelamin, dan umur disamarkan di memori; nama lengkap tidak pernah keluar dari service. `MapConsoleCallAsync` untuk response perintah; `ConsoleActions`. Konstruktor menerima `NvwPatientCallWaitThresholdService` |
| `…/Services/NvwPatientCallService.cs` | `ClaimAsync`, `TakeOverAsync`, `ReleaseAsync`, `CloseFromConsoleAsync`. Satu perintah = satu `SaveChanges` (panggilan + satu jejak `ActorKind = Nurse`). `TakenOver` dan `Released` mengisi `PreviousClaimerUserId`. Tutup mengisi `IsBedsideConfirmed = false`, `CloseReason`, `CloseNote` (dipangkas), `ClosedByUserId`; jejak menyalin keterangan ke `Note`. Konflik token versi → `VAL-NV-38` |
| `…/Services/NvwResult.cs` | Kode dan pesan `VAL-NV-33`–`36`, `39`; pesan "Panggilan tidak ditemukan." |
| `…/Constants/NurseViewPermissions.cs` | Resource `PatientCall` = `NurseViewPatientCall` |

Nol baris komentar baru; enam berkas CRLF tanpa BOM. Tanpa berkas bersama dan tanpa
`Program.cs`: `NvwPatientCallService`, `NvwPatientCallQueryService`, dan
`NvwPatientCallWaitThresholdService` sudah terdaftar.

**Keputusan developer di luar teks kartu.**

| No | Keputusan | Alasan |
| ---: | --- | --- |
| 1 | `AvailableActions` kosong bagi pengguna tanpa izin `Update` (diperiksa lewat `AccessPermissionService.HasAccessAsync` saat `GET /active`) | `AD-06` membolehkan peran pantau-saja; backend pemegang wewenang `AvailableActions` (`API_RULES`) |
| 2 | Klaim pada panggilan `Claimed` diperiksa **sebelum** versi → `VAL-NV-33` dengan nama pengklaim; kalah balapan pada versi yang sama → `VAL-NV-38` | AC-4 menyebut `VAL-NV-38` untuk klaim bersamaan; R2.11 menyebut pesan `VAL-NV-33` untuk perawat kedua. Keduanya terjadi sesuai waktu: bila permintaan kedua membaca sesudah yang pertama tersimpan, ia mendapat `VAL-NV-33`; bila keduanya membaca bersamaan, token versi memberi `VAL-NV-38` |
| 3 | Lepas pada panggilan `Waiting` → `409` `VAL-NV-34` | Tabel "Transisi yang tidak sah" state §5 |
| 4 | Keterangan boleh diisi pada alasan 2 dan 3, tetap paling banyak 250 karakter; dipangkas; kosong menjadi `null` | api §7 hanya mewajibkan keterangan untuk alasan 4 |
| 5 | Lama menunggu panggilan yang sudah selesai dihitung sampai `ClosedAt`, dan `WaitLevel` selalu `None` | Response perintah tutup memuat panggilan yang sudah selesai; jam lama menunggu tidak berjalan lagi |
| 6 | `GET /floors` memakai `NvwBedTabletService.GetFloorsAsync` | R2.4.6 menyebut `NvwBedTabletService` (pilihan lantai) untuk controller ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Enam endpoint sesuai api §7. Tidak ada endpoint lama yang berubah perilaku |
| Database | Tanpa migration, tanpa perubahan skema. Menulis `NvwPatientCall` dan `NvwPatientCallEvent`; membaca `MstRoom`, `MstBed`, `InpEpisode`, `MstPatient`, `AspNetUsers`, `NvwPatientCallWaitThreshold`. Baris izin resource `NurseViewPatientCall` (`Read`, `Update`) lahir otomatis lewat `AccessMenuSeeder` saat startup |
| Keamanan/Auth | Resource izin baru, tanpa hardcode peran. Tanpa pemeriksaan roster (`NV-DEC-043`). Response konsol hanya inisial · jenis kelamin · umur (`NV-DEC-044`); `CloseNote` tidak ada di response dan log (permission-audit R2 §10) |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Patient Call

Base URL: `api/v1/health-services/nurse-view-management/patient-calls`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floors` | Pilihan lantai untuk konsol | `NurseViewPatientCall : Read` |
| `GET` | `/active?floorCode=3` | Panggilan aktif satu lantai, berurutan | `NurseViewPatientCall : Read` |
| `POST` | `/{id}/claim` | "Saya tangani" | `NurseViewPatientCall : Update` |
| `POST` | `/{id}/take-over` | "Ambil alih" dari pengklaim lain | `NurseViewPatientCall : Update` |
| `POST` | `/{id}/release` | "Lepas" oleh pengklaim | `NurseViewPatientCall : Update` |
| `POST` | `/{id}/close` | "Tutup panggilan" dengan alasan | `NurseViewPatientCall : Update` |

Body klaim, ambil alih, lepas: `{ "version": n }`. Body tutup:
`{ "reason": 3, "note": null, "version": n }` (alasan `2` Tablet tidak dapat dipakai, `3` Sudah
ditangani — lupa konfirmasi di tablet, `4` Lainnya + keterangan 5–250 karakter).

**Contoh item.**
`{ "bedCode": "BD-RSMMC-00030", "bedName": "325-B", "patient": { "initials": "R.A.", "genderCode": "P", "ageLabel": "72 th" }, "priorityLabel": "Darurat", "statusLabel": "Menunggu", "waitingSeconds": 20, "pressCount": 1, "claimedByName": null, "isClaimedByMe": false, "waitLevel": "None", "availableActions": ["Claim", "Close"], "version": 0 }`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <6 berkas>` (array path) | Files evaluated **6** · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-09 |
| Pasangan `[AccessAction]`/`[AccessPermission]` | 6 endpoint, 6 pasangan; argumen ke-1 `[AccessPermission]` = `ControllerName`, argumen ke-2 = nama `[AccessAction]` | `PASS` | Review controller |
| Review diff/scope; nol komentar; akhiran baris | Hanya berkas modul Nurse View; nol `//`/`/*`; 6 berkas CRLF tanpa BOM | `PASS` | `grep`, hitungan byte Node |
| `dotnet build` | Dijalankan Rizki 9 Nov 2026, berhasil (pernyataan Rizki). Backend dijalankan Rizki; ketiga rute contoh tanpa login → `401` | `PASS` menurut pernyataan Rizki; jumlah warning tidak dinyatakan | DLL memuat kelima simbol task ini (pemeriksaan byte agent) |
| Baris izin hasil `AccessMenuSeeder` (SELECT read-only) | `NurseViewPatientCall` · `Read` dan `Update`, aktif, `VisibleInRoleAccess` true | `PASS` | Kueri agent 9 Nov 09.54 WIB |
| Uji HTTP no. 1–17 — 9 Nov 09.55 WIB (`uji-konsol.mjs`) | **17/17 `PASS`** — rincian di bawah | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-012-20261109/hasil-2026-11-09T02-55-05-411Z/hasil.json`, `percobaan.json` (74 permintaan), `policy-asli-sari.json`, `policy-asli-dewi.json` |
| Pemindaian log backend (`quilvian-backend-20261109.json`) | 74 permintaan di jendela uji = 74 rekaman skrip, tanpa permintaan lain. 11 log aksi konsol = 11 perintah konsol berhasil; setiap pesan memuat Id panggilan, kode bed, aksi, status, prioritas, pengklaim. Sandi env keempat akun, nama dan No. RM keempat pasien, keterangan tutup uji no. 14, pola sandi tablet, ruas `password` berisi nilai, dan token: **semuanya 0** sepanjang hari. Nol `Error` | `PASS` | `hasil-…/pindai-log-agent.json` (skrip `pindai-log.mjs`) |
| Keadaan DB sesudah uji (SELECT read-only) | Tablet `NVT-000017`–`000020` status 3; 0 panggilan aktif lantai 3/4; policy Perawat Rawat Inap 20 dan Petugas Pendaftaran 36 butir; 0 izin `NurseViewPatientCall` di jabatan mana pun; 11 jejak `ActorKind = Nurse`, 1 berisi `Note` | `PASS` | Kueri agent 9 Nov 09.57 WIB |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan:** `dotnet build`, `dotnet run`, `dotnet ef` — milik Rizki. Tanpa migration.

### Rencana uji HTTP sesudah build

Aktor: **Sari** = akun `PERAWAT_RUANGAN` (Keperawatan/Perawat Rawat Inap) dan **Dewi** = akun
`ADMISI` (Pendaftaran/Petugas Pendaftaran), keduanya diberi izin sementara
`NurseViewPatientCall` Read/Update (salin → tambah → pulihkan; jumlah `GET /policies` dicocokkan
dengan baris DB lebih dulu). Akun `PERAWAT` tidak dipakai karena berperan SuperAdmin. Tanpa
izin: akun `DOKTER`. Tablet uji dibuat lewat SuperAdmin (persiapan) di bed berpenghuni 326-C,
325-A, 325-B (lantai 3) dan 420-A (lantai 4).

| No | Langkah | Hasil yang diharapkan | AC |
| ---: | --- | --- | --- |
| 1 | `GET /floors` | `200`, berisi lantai 3 dan 4 | — |
| 2 | `GET /active` lantai `ZZ9` dan tanpa lantai | `422` `VAL-NV-39` ×2 | AC-8 |
| 3 | `DOKTER`: daftar, lantai, klaim, tutup | `403` ×4 | AC-6 |
| 4 | Daftar lantai 3 tanpa panggilan | `ThresholdsConfigured` false, `Items` kosong, "Lantai 3" | AC-9 |
| 5 | Biasa 326-C, biasa 325-A, Darurat 325-B; daftar | Urutan 325-B, 326-C, 325-A; `WaitLevel` `None`; aksi `Claim`, `Close` | AC-1, AC-9 |
| 6 | Isi item | Hanya inisial · jenis kelamin · umur; tanpa nama lengkap, No. RM, Id pasien/episode, tanggal lahir | AC-7 |
| 7 | Sari klaim; Dewi klaim; Dewi ambil alih | `200`; `409` `VAL-NV-33` dengan nama Sari; `200`; jejak `Claimed`(Sari) + `TakenOver`(Dewi, sebelumnya Sari); jam awal tetap | AC-2 |
| 8 | Ambil alih klaim sendiri; ambil alih dan lepas panggilan `Waiting` | `409` `VAL-NV-34` ×3 | — |
| 9 | Sari ambil alih kembali; Dewi melepas; Sari melepas | `200`; `403` `VAL-NV-35`; `200` `Waiting`, jam awal tetap, jejak `Released` | AC-3 |
| 10 | Klaim dengan versi lama | `409` `VAL-NV-38` | — |
| 11 | Sari dan Dewi klaim bersamaan | Tepat satu `200`, satu `409`; satu jejak `Claimed` | AC-4 |
| 12 | Klaim, lalu pasien membatalkan dari tablet | `409` `VAL-NV-31`; tetap `Claimed` | AC-10 |
| 13 | Tutup tanpa alasan / alasan 1 / Lainnya tanpa keterangan, 3 huruf, 251 huruf; lalu alasan 3; lalu klaim | `400` `VAL-NV-36` ×5; `200` `Handled`, `IsBedsideConfirmed` false, jejak `ClosedFromConsole`; `409` `VAL-NV-37` | AC-5 |
| 14 | Tutup alasan Lainnya + keterangan khas | `CloseNote` dan `Note` jejak tersimpan; keterangan tidak ada di response maupun log | AC-5, permission-audit §10 |
| 15 | Tutup alasan 2 oleh pengklaim | `200`; lantai 3 kosong | — |
| 16 | Sari mengklaim panggilan lantai 4 | `200` (izin saja, tanpa pemeriksaan roster) | AC-6 |
| 17 | Klaim Id acak | `404` | — |

### Hasil uji HTTP — 9 November 2026

Verifikasi developer oleh agent, bukan UAT. Putusan diambil dari JSON mentah, log backend, dan
SELECT read-only. Tablet uji: `NVT-000017` (326-C), `NVT-000018` (325-A), `NVT-000019` (325-B),
`NVT-000020` (420-A).

| No | Terukur |
| ---: | --- |
| 1 | `200`; lantai `2, 3, 4, 5, 8, 9` |
| 2 | `ZZ9` dan tanpa lantai → `422` `VAL-NV-39` ×2 |
| 3 | `DOKTER`: daftar, lantai, klaim, tutup → `403` ×4 |
| 4 | `floorName` "Lantai 3", `thresholdsConfigured` false, `items` kosong, `serverTimeUtc` terisi |
| 5 | Urutan 325-B/Darurat, 326-C/Panggilan, 325-A/Panggilan; ketiganya `waitLevel` `None`, aksi `Claim`+`Close` |
| 6 | Pasien `P.U.P. · L · 41 th`, `P.P. · L · 34 th`, `P.B.T. · L · 31 th`; objek `patient` hanya tiga kunci; tanpa kunci identitas lain; nama lengkap dan No. RM ketiga pasien tidak muncul |
| 7 | Sari klaim `200` (Diklaim, `isClaimedByMe`, aksi `Release`+`Close`); Dewi klaim `409` `VAL-NV-33` "Panggilan sudah diambil ‹nama Sari›. Gunakan Ambil alih bila perlu."; daftar Dewi menampilkan `TakeOver`+`Close`; Dewi ambil alih `200`; jejak `Pressed`, `Claimed`(Sari), `TakenOver`(Dewi, sebelumnya Sari); jam awal sama |
| 8 | Ambil alih klaim sendiri, ambil alih `Waiting`, lepas `Waiting` → `409` `VAL-NV-34` ×3 |
| 9 | Sari ambil alih kembali `200`; Dewi melepas → `403` `VAL-NV-35`; Sari melepas → `200` Menunggu; DB pengklaim kosong, `FirstPressedAt` tetap; jejak `Released` (pelaku dan sebelumnya Sari) |
| 10 | Klaim versi lama → `409` `VAL-NV-38`; tetap Menunggu |
| 11 | Klaim bersamaan: Sari `200`, Dewi `409` **`VAL-NV-38`**; satu jejak `Claimed` |
| 12 | Sari klaim; tablet membatalkan → `409` `VAL-NV-31`; tetap Diklaim |
| 13 | Tanpa alasan, alasan 1 → `400` "Pilih alasan…"; Lainnya tanpa keterangan, `abc`, 251 huruf → `400` "Keterangan wajib…"; alasan 3 → `200` Ditangani, aksi kosong; DB `IsBedsideConfirmed` false, `CloseReason` 3, `ClosedByUserId` Sari, `CloseNote` kosong; jejak `ClosedFromConsole` (Sari, alasan 3); klaim sesudahnya → `409` `VAL-NV-37` |
| 14 | Dewi menutup panggilan Darurat dengan Lainnya + keterangan berspasi di tepi → `200`; `CloseNote` dan `Note` jejak sama dengan keterangan yang dipangkas; keterangan tidak ada di response |
| 15 | Pengklaim menutup alasan 2 → `200`; daftar lantai 3 kosong |
| 16 | Tablet 420-A memanggil; daftar lantai 4 memuatnya; Sari klaim `200` lalu tutup `200` |
| 17 | Id acak → `404` `NV-NOT-FOUND` "Panggilan tidak ditemukan." |

Pembersihan di `finally`: keempat tablet nonaktif; tidak ada panggilan tersisa; policy Perawat
Rawat Inap (20 → 20) dan Petugas Pendaftaran (36 → 36) dipulihkan identik.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 urutan Darurat dulu lalu paling lama menunggu | ✅ Terbukti 9 Nov 2026 | `GetActiveForConsoleAsync`; uji no. 5 |
| AC-2 klaim Sari; klaim Dewi `409` `VAL-NV-33` dengan nama Sari; Dewi ambil alih; jejak keduanya; lama menunggu dari jam awal | ✅ Terbukti 9 Nov 2026 | `ClaimAsync`, `TakeOverAsync`; uji no. 7 |
| AC-3 Sari melepas → `Waiting`, jam awal tetap; Dewi melepas klaim Sari → `403` `VAL-NV-35` | ✅ Terbukti 9 Nov 2026 | `ReleaseAsync`; uji no. 9 |
| AC-4 klaim bersamaan → tepat satu berhasil, satu `409` | ✅ Terbukti 9 Nov 2026 | Token `Version`; uji no. 11: yang kalah `VAL-NV-38`, satu jejak `Claimed` |
| AC-5 tutup tanpa alasan / Lainnya tanpa keterangan → `400`; alasan 3 → `Handled`, `IsBedsideConfirmed = false`, jejak `ClosedFromConsole` | ✅ Terbukti 9 Nov 2026 | `CloseFromConsoleAsync`; uji no. 13–15 |
| AC-6 tanpa izin → `403`; Sari berizin tanpa roster mengklaim lantai 4 → diterima | ✅ Terbukti 9 Nov 2026 | Uji no. 3, 16; tidak ada pemeriksaan roster di source |
| AC-7 item hanya "R.A. · P · 72 th" | ✅ Terbukti 9 Nov 2026 | Uji no. 6 dan pindaian log |
| AC-8 lantai tidak dikenal → `422` `VAL-NV-39` | ✅ Terbukti 9 Nov 2026 | Uji no. 2 |
| AC-9 ambang kosong → `WaitLevel = None`, `ThresholdsConfigured = false` | ✅ Terbukti 9 Nov 2026 | Uji no. 4, 5 |
| AC-10 batal pasien pada panggilan diklaim → `409` `VAL-NV-31` | ✅ Terbukti 9 Nov 2026 | Uji no. 12; menutup bukti runtime `VAL-NV-31` pada `Claimed` yang dititipkan `BE-NV-011` |
| DoD — AC-1–10 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — build Rizki lulus | Terpenuhi | Pernyataan Rizki; DLL 9 Nov 09.51 memuat kode task ini |
| DoD — JSON bukti tersimpan | Terpenuhi | Folder `hasil-2026-11-09T02-55-05-411Z/` |
| DoD — laporan | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (metadata Access
lengkap, tanpa hardcode peran), `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001` (aktor dan teks
pesan; `CloseNote` tidak dicatat), `QBE-TXN-001`, `QBE-OPT-001` (`GET /floors` dipakai konsol).
Tidak berlaku: `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*`, `QBE-PAGE-001` (daftar satu
lantai tanpa paging sesuai kontrak), `QBE-DEL-001`.

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` (registry baris 45) · applicability `NEW CODE` · branch `rizkiG`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sinyal realtime belum dikirim; itu bagian `BE-NV-014`. Konsol memuat ulang daftar lewat `GET /active` |
| Masalah yang diketahui | (1) Peran pantau-saja (`AD-06`) belum ada di DB dev, sehingga `AvailableActions` kosong untuk pengguna hanya-`Read` baru terbukti lewat source. (2) `ClosedByUserId` = pengklaim saat "Perawat hadir" pada panggilan yang sudah diklaim (`BE-NV-011`) tidak termasuk AC task ini dan belum diuji runtime; source-nya sederhana (`call.ClosedByUserId = call.ClaimedByUserId`). (3) Nama pengklaim di log adalah nama staf, bukan identitas pasien |
| Risiko tersisa | Identitas bocor lewat field baru, atau klaim ganda bila token versi keliru. Uji no. 6, 11, dan pemindaian log menunjukkan keduanya tertangani |
| Perubahan sampingan | Kode: `NONE`. Data di DB dev bersama akibat uji: tablet `NVT-000017`–`000020` dibuat lalu dinonaktifkan; 4 panggilan selesai beserta jejaknya tersimpan sebagai riwayat; policy Perawat Rawat Inap dan Petugas Pendaftaran diubah sementara lalu dipulihkan identik. Baris izin `NurseViewPatientCall` lahir dari seeder saat startup (bawaan aplikasi) |
| Interupsi | Backend sempat belum hidup sesudah build; dijalankan Rizki |
| Status Git | `M` `NvwPatientCallService.cs`, `NvwPatientCallQueryService.cs`, `NvwResult.cs` (di atas perubahan `BE-NV-011` yang sudah di-stage), `NurseViewPermissions.cs`; `??` `NvwPatientCallController.cs`, `NvwPatientCallDtos.cs`; laporan ini dan baris status roadmap/traceability. Bukti uji di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-012-20261109/` (di-ignore git) |
| Langkah berikutnya | `BE-NV-013` (tutup otomatis saat pindah/pulang) dan `BE-NV-015` (strip TV) boleh dimulai. `BE-NV-014` menunggu `BE-NV-013`; `FE-NV-012` menunggu `FE-NV-010` |
