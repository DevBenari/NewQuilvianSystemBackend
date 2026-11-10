# Laporan Perubahan Backend — `BE-NV-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-015` |
| Judul | Strip Panggilan Aktif dan penanda kartu di papan TV |
| Slice | `MVP-8` — `EPIC NV-07` (gelombang backend 5, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-015` |
| Trace | `FR-NV-046`, `047`; `NV-DEC-044`, `047`, `048`; `NV-AC-030`, `031`, `037` (sisi TV); `VAL-NV-14` × tablet pada `/active-calls`; `UAT-21`, `22`; `NFR-001`, `013`; `02-backend-architecture.md` R2.4.5 (`NvwFloorBoardQueryService`, `NvwPatientCallQueryService`), R2.4.6 |
| Contract version | `0.2.1` — `approved` (`NV-DEC-057`); api §9 tidak berubah sejak `0.2.0` (`NV-DEC-055`) |
| Dependency | `BE-NV-011` ✅, `BE-NV-016` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 termasuk blueprint (2), berkas diubah 5 (1), logika sedang — urutan, nol identitas, jumlah query tetap (1), endpoint baru sesuai kontrak (1), persistence yang sudah ada (1), keamanan berkaitan — privasi layar TV (1), UI tidak ada (0) |
| Task mode | `BACKEND` — perintah Rizki 9 Nov 2026 ("Berikutnya: BE-NV-015 … lalu FE-NV-014"), target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, laporan ini, baris status roadmap backend dan frontend, serta traceability. Tanpa berkas bersama |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b83e8f05` (branch `rizkiG`; satu commit di atas `1e17628c`, hanya `requirement-traceability.md`); working tree bersih sebelum task |
| Tanggal | 2026-11-09 |
| Status | 🟡 **SEBAGIAN 10 November 2026** — perilaku terbukti runtime pagi ini (uji HTTP S1–S11 11/11, bersama uji hub `BE-NV-014`). Perbaikan otorisasi sesudah uji: CI integration gagal di invarian [2] `tools/authorization-verifier` karena `NvwDisplayRuntimeController` memakai `[AccessController]`/`[AccessAction]` tanpa `[AccessPermission]` (warisan `BE-NV-003`/`005`, lalu `GetActiveCalls` dari task ini). Atas persetujuan Rizki, `[AccessController]` dan ketiga `[AccessAction]` dibuang, policy `NurseViewDisplayRead` tetap (pola `NvwBedTabletRuntimeController`); konstanta `NurseViewPermissions.DisplayRuntime` dihapus. Diff −19/+0, QBE Strict `PASS` (2 berkas). Belum: `dotnet build` oleh Rizki, verifier otorisasi lokal, dan uji ulang singkat TV/tablet |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-NV-011` sampai `BE-NV-013`, panggilan pasien sudah tampil di Konsol Panggilan, tetapi
TV nurse station belum bisa membacanya. TV hanya punya `/board`, dan kartu bed tidak tahu bahwa
pasiennya sedang memanggil.

**Contoh.** Pasien 326-C menekan "Darurat". Perawat yang sedang berdiri di depan TV lantai 3 tidak
melihat apa-apa, karena konsol ada di PC lain dan TV tidak punya data panggilan.

---

## 2. Proses bisnis

**Tujuan:** TV lantai menampilkan daftar panggilan aktif lantainya tanpa identitas pasien, dan
kartu bed yang sedang memanggil membawa penanda prioritas (`NV-DEC-044`, `047`).

**Langkah normal:**

1. TV login dengan akun perangkatnya. Lantai diambil dari perangkat, bukan dari parameter.
2. TV memanggil `GET /display-runtime/active-calls`. Isinya panggilan `Waiting`/`Claimed` lantai
   itu: Darurat lebih dulu, lalu yang paling lama menunggu (`NV-DEC-038`).
3. Setiap item memuat nomor bed, nama bed, kamar, prioritas, status, jam tekan pertama, lama
   menunggu dari jam server, nama perawat pengklaim, dan tingkat warna (`None`/`Warning`/`Critical`).
4. TV memanggil `GET /display-runtime/board` seperti biasa. Kartu bed yang sedang memanggil kini
   membawa `ActiveCallPriority` (`1` Panggilan, `2` Darurat); kartu lain berisi `null`.
5. TV memuat ulang strip saat menerima sinyal `NurseViewPatientCallChanged` (`BE-NV-014`), dan
   bersama papan tiap 60 detik sebagai cadangan.

**Contoh urutan (AC-1, `NV-AC-030`).** 305-A memanggil biasa 10.00.00, 318-B biasa 10.00.20,
331-C Darurat 10.00.40. Pukul 10.01.00 strip berurutan 331-C (Darurat, 20 detik), 305-A (60 detik),
318-B (40 detik). Kartu 331-C membawa `activeCallPriority: 2`, kartu 305-A dan 318-B `1`.

**Contoh item strip.**
`{ "bedCode": "BD-RSMMC-00015", "bedName": "331-C", "roomName": "Kamar 331", "priority": 2, "priorityLabel": "Darurat", "callStatus": 2, "firstPressedAt": "…", "waitingSeconds": 20, "claimedByName": "Dewi", "waitLevel": "None" }`

**Yang tidak pernah ada di strip:** inisial, nama, No. RM, umur, jenis kelamin, `PatientId`,
`EpisodeId`, `CallId`, `CloseNote`.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem | Kode |
| --- | --- | --- |
| Akun tablet, akun manusia, atau akun tanpa klaim `nurse_view_device_id` | Ditolak policy `NurseViewDisplayRead` tanpa isi (`VAL-NV-14` × tablet) | `403` |
| Perangkat TV tidak lagi ditemukan atau `Inactive` | Pesan `VAL-NV-14` / perangkat nonaktif, sama dengan `/board` | `403` |
| Token terbit sebelum Atur Ulang Login terakhir | `NV-SESSION-STALE` "Sesi perangkat sudah tidak berlaku. Silakan login ulang." | `403` |
| Tidak ada panggilan aktif | `TotalActive = 0`, `Items` kosong; strip tidak memakan tempat (urusan `FE-NV-014`) | `200` |
| Ambang lama menunggu belum diisi (`NV-OQ-022`) | `ThresholdsConfigured = false`, semua `WaitLevel = "None"` | `200` |
| Panggilan dilepas pengklaim | `ClaimedByName = null`, status kembali `Waiting` | `200` |
| Pasien sudah meninggalkan bed, panggilan belum ditutup pemeriksaan berkala (≤ 60 detik) | Item tetap tampil dan kartu tetap bertanda sampai `ClosedBySystem` | `200` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`, `CLAUDE.md` backend; suite `rules/backend/*` (TASK_RULES, TASK_CLASSIFICATION, REPORT_TEMPLATE, TEST_POLICY, role-access-rules, transaction-endpoint-standard); `docs/engineering/` kontrak dan registry (`Nvw` `ACTIVE`, baris 45)
- Blueprint: kartu `BE-NV-015`; api §3, §7, §9, §10; `02-backend-architecture.md` R2.3.3, R2.4.5, R2.4.6; `00-interview-decisions.md` `NV-DEC-038`, `044`, `047`, `048`, `NV-AC-030`, `031`, `037`; acceptance-test-matrix `VAL-NV-14` × tablet
- Source: `NvwDisplayRuntimeController`; `NvwFloorBoardQueryService`; `NvwPatientCallQueryService` (`GetActiveForConsoleAsync`, `MapConsoleCallsAsync`); `NvwPatientCallWaitThresholdService` (`GetActiveAsync`, `ResolveWaitLevel`); `NvwDeviceContextService.ResolveActiveDeviceAsync`; `NvwPatientCallService` (penyimpanan `FloorCode`, `ReleaseAsync` mengosongkan `ClaimedByUserId`); `NvwPatientCall`, `NvwPatientCallConfiguration` (index unik berfilter satu panggilan aktif per bed, index `FloorCode`+`CallStatus`); enum `NvwPatientCallPriority`, `NvwPatientCallStatus`; `MstBed`, `MstRoom`, `ApplicationUser`; `Program.cs` (registrasi DI, policy `NurseViewDisplayRead`, opsi JSON tanpa `JsonStringEnumConverter`)
- Konsumen: hanya `NvwDisplayRuntimeController` yang membuat `NvwFloorBoardQueryService`; tidak ada konstruktor manual di luar DI

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwDisplayRuntimeController.cs` | Konstruktor +`NvwPatientCallQueryService`; action baru `GetActiveCalls` (`GET active-calls`), pola sama dengan `GetBoard`: policy `NurseViewDisplayRead`, `ResolveActiveDeviceAsync`, `[AccessAction(P.Read, …, SortOrder = 3)]`. `GetCurrent` dan `GetBoard` tidak berubah |
| `…/Services/NvwPatientCallQueryService.cs` | `GetActiveForDisplayAsync` (strip), `GetActivePriorityByBedAsync` (penanda kartu), helper privat `ActiveOnFloor`. Method konsol dan tablet tidak berubah |
| `…/Services/NvwFloorBoardQueryService.cs` | Konstruktor +`NvwPatientCallQueryService`; satu panggilan `GetActivePriorityByBedAsync` per lantai; `BuildCard` menerima kamus prioritas dan mengisi `ActiveCallPriority`. Urutan kartu dan field lain tidak berubah |
| `…/DTOs/NvwPatientCallDtos.cs` | `NvwDisplayCallStripResponse`, `NvwDisplayCallStripItemResponse` |
| `…/DTOs/NvwFloorBoardDtos.cs` | `NvwBedCardResponse.ActiveCallPriority` (`NvwPatientCallPriority?`) + `using …Enums` |

Diff +143/−4. Nol baris komentar baru; kelima berkas CRLF tanpa BOM, sama dengan blob `HEAD`.
Tanpa `Program.cs`: kedua service sudah terdaftar `AddScoped`, dan `NvwPatientCallQueryService`
tidak bergantung pada `NvwFloorBoardQueryService`, jadi tidak ada lingkar DI.

**Keputusan developer di luar teks kartu.**

| No | Keputusan | Alasan |
| ---: | --- | --- |
| 1 | Strip dan penanda kartu sama-sama menyaring `NvwPatientCall.FloorCode`, lalu penanda dipetakan ke kartu lewat `BedId` | Strip dan penanda selalu sepakat. `FloorCode` disalin saat panggilan dibuat agar strip tetap terbaca walau kamar dipindah lantai (`02-backend-architecture.md` R2.4.2) |
| 2 | Strip mengembalikan **semua** item aktif; `TotalActive = Items.Count` | Kapasitas dan tanda "+k panggilan lain" adalah keputusan tampilan `NV-FE-006` milik `FE-NV-014` |
| 3 | Label bed dan nama pengklaim dibaca dalam satu SQL bersama panggilan (subquery berkorelasi) | Jumlah query strip tetap untuk 0, 1, atau 10 panggilan, diklaim atau tidak (AC-6) |
| 4 | `ActiveCallPriority` diisi walau bed sudah tanpa penghuni | Sama dengan strip selama jeda ≤ 60 detik sebelum `ClosedBySystem` (`NV-DEC-052`) |
| 5 | Kartu yang memanggil **tidak** dinaikkan urutannya | AC-5: papan hanya boleh berbeda pada field baru |
| 6 | Bila ada lebih dari satu panggilan aktif per bed, penanda memakai prioritas tertinggi | Index unik `IX_NvwPatientCall_BedId_Active` sudah mencegahnya; ini hanya pengaman |
| 7 | `ClaimedByName` memakai `DisplayName`, jatuh ke `UserName` bila kosong | Sama dengan konsol (`BE-NV-012`) dan tablet (`BE-NV-011`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /display-runtime/active-calls` dan satu field opsional `activeCallPriority` di setiap kartu `/board`, persis api §9. Enum dikirim sebagai angka (`1`/`2`), sesuai kontrak, karena aplikasi tidak memasang `JsonStringEnumConverter`. Tanpa delta kontrak |
| Database | Tanpa migration, tanpa tulis. Strip: satu query panggilan + bacaan ambang. Papan: tepat satu query tambahan per permintaan (`BedId`, `Priority`), memakai index `FloorCode`+`CallStatus` |
| Keamanan/Auth | Policy `NurseViewDisplayRead` tetap; akun tablet tidak membawa `nurse_view_device_id` sehingga `403`. Projection strip tidak membaca `InpEpisode`/`MstPatient` sama sekali, jadi identitas pasien tidak mungkin ikut (`NV-DEC-044`, `NV-AC-037`). Tanpa hardcode peran |

### 3.4 Revisi 10 November 2026 — otorisasi runtime TV

**Masalah.** Push ke `QuilvianIntegrationBackend` gagal pada langkah "Verify authorization registry":
`[2] Endpoint memakai fallback kompatibilitas tanpa persetujuan` untuk
`NurseViewDisplayRuntime.GetBoard` dan `NurseViewDisplayRuntime.GetCurrent`. Verifier menghitung setiap
endpoint ber-`[AccessAction]` tanpa `[AccessPermission]` sebagai fallback, lalu membandingkannya dengan
allowlist warisan `approved-compatibility-fallback.txt` (69 endpoint). Runtime TV (`BE-NV-003`/`005`)
memasang `[AccessAction]` padahal pemeriksanya policy perangkat; `GetActiveCalls` dari task ini meniru
pola yang sama sehingga push berikutnya akan gagal dengan tiga endpoint. QBE checker dan validator
startup tidak menangkapnya; verifier hanya berjalan di CI.

**Perbaikan** (disetujui Rizki 10 Nov 2026):

| Berkas | Perubahan |
| --- | --- |
| `…/Controllers/NvwDisplayRuntimeController.cs` | Buang `[AccessController]`, tiga `[AccessAction]`, `using QuilvianSystemBackend.Attributes`, alias `P`. `[Authorize]` class dan `[Authorize(Policy = NurseViewDisplayRead)]` per method tetap |
| `…/Constants/NurseViewPermissions.cs` | Hapus `DisplayRuntime.Resource` (tidak dirujuk lagi) |

**Dampak.**

| Aspek | Dampak |
| --- | --- |
| Keamanan | Tidak berubah. TV tetap wajib klaim `nurse_view_device_id` lewat policy `NurseViewDisplayRead`; akun tablet dan manusia tetap `403`. Policy itu sudah terdaftar di `AuthorizationPolicies.ApprovedAlternativeAuthorization`, sehingga endpoint tidak dihitung naked |
| Layar Akses Role | Butir "Nurse View Display Runtime : Read" hilang. Saat startup, `AccessMenuSeeder.CloseRowsAbsentFromSourceAsync` menutup baris `SysControllerAccess`/`SysActionAccess`-nya (soft delete), policy tidak disentuh. DB dev: 0 policy aktif memakai butir itu |
| API dan frontend | Route, response, dan Swagger tag sama; frontend tidak merujuk resource itu |
| Verifier | Himpunan fallback kembali sama dengan allowlist; controller Nurse View lain seimbang (`[AccessAction]` = `[AccessPermission]`) |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Display Runtime

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/display-runtime/active-calls` | Strip Panggilan Aktif lantai milik TV, tanpa parameter lantai | Policy `NurseViewDisplayRead` (akun perangkat TV), tanpa `[AccessPermission]` |
| `GET` | `/api/v1/health-services/nurse-view-management/display-runtime/board` | Papan lantai; kartu kini membawa `activeCallPriority` | Policy `NurseViewDisplayRead`, tidak berubah |

| Class | Field |
| --- | --- |
| `NvwDisplayCallStripResponse` | `FloorCode`, `ThresholdsConfigured`, `TotalActive`, `Items[]`, `ServerTimeUtc` |
| `NvwDisplayCallStripItemResponse` | `BedCode`, `BedName`, `RoomName`, `Priority`, `PriorityLabel`, `CallStatus`, `FirstPressedAt`, `WaitingSeconds`, `ClaimedByName?`, `WaitLevel` |
| `NvwBedCardResponse` (tambahan) | `ActiveCallPriority?` — `1` Panggilan, `2` Darurat, `null` bila bed tidak memanggil |

Pesan sukses strip: "Panggilan aktif lantai berhasil diambil."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <5 berkas>` (array path) | Files evaluated **5** · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-09 |
| Review diff/scope; nol komentar; akhiran baris | Hanya lima berkas Nurse View; `GetCurrent`, `GetBoard`, method konsol dan tablet tidak berubah; CRLF 86/81/342/442/106 baris, LF 0, tanpa BOM | `PASS` | `git diff`, hitungan byte Node |
| Pasangan atribut akses | `GetActiveCalls`: `[AccessAction(P.Read …)]` + policy bernama, pola sama dengan dua action lain di controller ini (akun perangkat, tanpa `[AccessPermission]`) | `PASS` | Source |
| Penelusuran source AC-1–6 | §6 | `PASS` | Source |
| Baseline `/board` build lama (DLL 9 Nov 11.43, sebelum task ini) | TV `NVW-000002` lantai 3: `/current` `200`, `/board` `200` — 1 bangsal, 66 kartu, 4 kartu terisi (325-A, 325-B, 325-C, 326-C), **0** kartu berkunci `activeCallPriority`; `/active-calls` `404` | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-015-20261109/baseline-2026-11-09T07-52-25-718Z/` (`papan.json`, `strip.json`, `ringkas.json`), skrip `rekam-papan.mjs` |
| `dotnet build` (oleh Rizki) | Berhasil menurut pernyataan Rizki 9 Nov 2026; agent memeriksa artefaknya: DLL 15.59 (source terakhir 14.49) memuat `GetActiveCalls`, `GetActiveForDisplayAsync`, `GetActivePriorityByBedAsync`, `NvwDisplayCallStripResponse`, `ActiveCallPriority` | `PASS` | Pernyataan Rizki; pencarian simbol di DLL |
| Uji HTTP gabungan `uji-sinyal-strip.mjs` (akun TV lantai 3 dan 4, tablet, Sari, Dewi, DOKTER); perbandingan `/board` dengan baseline | S1–S11 11/11 lulus, 10 Nov 2026 08.44–08.45 WIB | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-014-015-20261110/hasil-2026-11-10T01-44-39-831Z/` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` pada 2 berkas perbaikan otorisasi (10 Nov 2026) | Files evaluated **2** · VIOLATION 0 · REVIEW 0 · `PASS` | `PASS` | Keluaran checker |
| `verify-authorization.sh --configuration Release` sesudah perbaikan | Butuh `dotnet`; dijalankan Rizki | `NOT RUN` | — |
| `dotnet build` sesudah perbaikan otorisasi | Dijalankan Rizki | `NOT RUN` | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Uji manual: `PASS` — uji HTTP 10 Nov 2026, S1–S11 11/11.

**Catatan baseline.** Perekaman baseline memerlukan atur ulang login TV `NVW-000002` oleh
SuperAdmin (persiapan lewat aplikasi, pola uji `BE-NV-010`). Sebelumnya dipastikan lewat `SELECT`
read-only bahwa TV itu terakhir daring 8 Nov 2026, tidak ada panggilan aktif, dan ambang kosong.
Sandi baru tidak ditulis ke berkas mana pun; pengaman skrip menolak penulisan berisi sandi atau token.

**Hasil uji HTTP — 10 November 2026** (skrip `uji-be-nv-014-015-20261110/uji-sinyal-strip.mjs`;
satu putaran bersama uji hub `BE-NV-014`; bukti `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-014-015-20261110/hasil-2026-11-10T01-44-39-831Z/`, termasuk
`papan-sesudah-tanpa-panggilan.json` dan `strip-tiga-panggilan.json`)

| No | Skenario | Hasil |
| --- | --- | --- |
| S1 | AC-5: `/board` TV `NVW-000002` sesudah build dibandingkan dengan baseline build lama 9 Nov: 66 dan 66 kartu, `bedId` sama, kunci tingkat atas dan bangsal sama, setiap kartu hanya bertambah `activeCallPriority` (`null`), **nol** beda nilai di luar `asOfUtc`/`ageMinutes`/`isOverdue`/`overdueMinutes` | `PASS` |
| S2 | Tanpa panggilan: `200`, `floorCode` "3", `totalActive` 0, `items` kosong, `thresholdsConfigured` false, pesan "Panggilan aktif lantai berhasil diambil." | `PASS` |
| S3 | AC-4: token tablet → `/active-calls` `403` dan `/board` `403`; Sari dan DOKTER → `/active-calls` `403` | `PASS` |
| S4 | AC-1: 325-A biasa, 325-B biasa (+1,2 dtk), 326-C Darurat (+1,2 dtk) → urutan 326-C, 325-A, 325-B; `totalActive` 3; `waitLevel` `None` | `PASS` |
| S5 | AC-3: setiap item tepat sepuluh kunci kontrak; nol kunci identitas (`patient`, `initials`, `ageLabel`, `fullName`, `medicalRecordNumber`, dsb.); nama lengkap dan No. RM pasien nol | `PASS` |
| S6 | Kartu `/board`: 326-C = 2, 325-A = 1, 325-B = 1, 63 kartu lain `null`; identitas nol | `PASS` |
| S7 | TV lantai 4 `NVW-000001`: `floorCode` "4", `totalActive` 0 saat lantai 3 punya tiga panggilan | `PASS` |
| S8 | AC-2: Sari mengklaim 325-A, Dewi mengambil alih → `claimedByName` strip sama dengan nama Dewi di respons konsol, `callStatus` 2; urutan tetap Darurat dulu (325-B yang naik Darurat, lalu 326-C, lalu 325-A) | `PASS` |
| S9 | Dewi melepas → `claimedByName` `null`, status `Menunggu` | `PASS` |
| S10 | Sesudah 325-A ditangani di tablet dan 325-B ditutup dari konsol → strip tinggal 326-C | `PASS` |
| S11 | Semua panggilan selesai → strip kosong, seluruh kartu `activeCallPriority` `null` | `PASS` |

AC-6 (jumlah query) diverifikasi lewat penelusuran source sesuai kartu; log SQL tidak dinyalakan.
Pembersihan dan pindai log sama dengan laporan `BE-NV-014` (satu putaran): nol panggilan aktif, tablet
uji nonaktif, policy dipulihkan identik, nol rahasia dan token di log.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 urutan strip sama dengan `NV-AC-030` | ✅ Terbukti 10 Nov 2026 | Uji S4 (326-C, 325-A, 325-B) dan S8 (urutan tetap sesudah naik Darurat) |
| AC-2 sesudah Dewi mengambil alih 305-A, item menampilkan `ClaimedByName = "Dewi"` | ✅ Terbukti 10 Nov 2026 (bed 325-A) | Uji S8: `claimedByName` strip = nama Dewi di respons konsol; S9 kosong lagi sesudah dilepas |
| AC-3 strip dan field kartu baru tanpa inisial, nama, No. RM, umur (`NV-AC-037`) | ✅ Terbukti 10 Nov 2026 | Uji S5, S6: kunci identitas nol; nama lengkap dan No. RM lima pasien nol di strip dan papan |
| AC-4 akun tablet → `403` di `/active-calls` | ✅ Terbukti 10 Nov 2026 | Uji S3: tablet `403` di `/active-calls` dan `/board`; Sari dan DOKTER `403` |
| AC-5 bagian lain `/board` tidak berubah | ✅ Terbukti 10 Nov 2026 | Uji S1: hanya `activeCallPriority` bertambah; nol beda nilai di luar field waktu |
| AC-6 jumlah query `/board` dan `/active-calls` sama untuk 1 maupun 10 panggilan | ✅ Terpetakan ke source, sesuai cara verifikasi kartu | `/board` tepat +1 query di luar perulangan kartu; `/active-calls` satu query berisi label bed dan nama pengklaim + bacaan ambang; log SQL tidak dinyalakan |
| DoD — AC-1–6 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — build Rizki lulus | Terpenuhi 9 Nov 2026 | Pernyataan Rizki; DLL 15.59 memuat simbol baru |
| DoD — JSON bukti tersimpan | Terpenuhi 10 Nov 2026 | Baseline 9 Nov + `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-014-015-20261110/hasil-2026-11-10T01-44-39-831Z/` |
| DoD — laporan | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (bacaan di query service, controller tipis),
`QBE-API-001` (rute dan envelope `ApiResponse<T>` mengikuti controller yang sama), `QBE-PERM-001`
(policy perangkat + `[AccessAction]`, tanpa hardcode peran), `QBE-DTO-001` (response DTO, bukan
entity), `QBE-VAL-001` (lantai dari perangkat, bukan parameter). Tidak berlaku: `QBE-ENT-*`,
`QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*` (tanpa perubahan schema), `QBE-TXN-001` (hanya baca),
`QBE-LOG-001` (GET tidak dicatat, permission-audit R2 §9), `QBE-PAGE-001` (strip satu lantai
tanpa paging sesuai kontrak), `QBE-OPT-001`, `QBE-DEL-001`.

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` (registry baris 45) · applicability `NEW CODE` · branch `rizkiG` · registry status
tidak menghalangi (`QBE-MOD-002` tidak berlaku).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Konstruktor `NvwDisplayRuntimeController` dan `NvwFloorBoardQueryService` bertambah satu dependensi; build akan langsung menunjukkan bila ada masalah. Build tertunda juga memuat perubahan konstruktor `QueueHub` dari `BE-NV-014` |
| Masalah yang diketahui | Bila kamar dipindah lantai saat panggilan masih aktif, strip dan penanda tetap mengikuti lantai saat panggilan dibuat, sehingga kartu bed itu di lantai baru belum bertanda sampai panggilan selesai. Ini konsekuensi `FloorCode` disalin (R2.4.2), bukan regresi |
| Risiko tersisa | Rendah. Regresi papan revision 1 terbukti tidak ada (S1); subquery nama pengklaim berjalan (S8). Hitungan query belum diukur dengan log SQL |
| Perubahan sampingan | `NONE`. Atur ulang login TV `NVW-000002` untuk baseline adalah persiapan uji, bukan perubahan source |
| Interupsi | `NONE` |
| Status Git | Source sebelumnya di-commit Rizki (`7cff2991`, `4a6ac9a3`); perbaikan otorisasi `M` `NvwDisplayRuntimeController.cs`, `NurseViewPermissions.cs`; laporan dan roadmap `M` |
| Langkah berikutnya | Rizki: `dotnet build`, lalu `bash ./tools/authorization-verifier/verify-authorization.sh --configuration Release` (harap `PASS`), jalankan backend, dan agent mengulang `uji-sinyal-strip.mjs` (TV `/current`, `/board`, `/active-calls` 200; tablet `403`) sebelum push |
