# Laporan Perubahan Backend — `BE-NV-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-017` |
| Judul | Peta lantai untuk Monitor perawat |
| Slice | Amendment 11 Nov 2026 sesudah `MVP-8` — `EPIC NV-07`, `NV-08` |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-017` |
| Trace | `NV-DEC-058`; `NV-DEC-044`, `043`, `008`; `VAL-NV-39` |
| Contract version | `0.2.2` api §7a (tambahan; §7 lain tidak berubah) |
| Dependency | `BE-NV-012` ✅, `BE-NV-015` ✅ |
| Klasifikasi | `SMALL` — satu repository, tiga berkas source, endpoint baca, tanpa persistence baru, keamanan terkait (privasi) |
| Task mode | `BACKEND` — perintah Rizki 11 Nov 2026 (jawaban empat pertanyaan "Monitor perawat", opsi "Integrasi penuh") |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, laporan ini, roadmap, kontrak §7a, keputusan `NV-DEC-058` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `67c27010` (branch `rizkiG`) |
| Tanggal | 2026-11-11 |
| Status | ✅ **SELESAI 11 November 2026** — AC 6/6: uji HTTP 5/5 `PASS` sesudah build Rizki (DLL 13.15 WIB); verifier otorisasi `PASS` (metadata gap 0, fallback 68 cocok persis, identitas wajib 24/24, endpoint telanjang baru 0); QBE Strict `PASS` (3 berkas) |

---

## 1. Masalah yang diperbaiki

Rizki memberi rancangan layar "Monitor perawat" untuk tablet/PC perawat: peta kamar dan bed satu lantai
sekaligus panel panggilan. Papan lantai selama ini hanya bisa dibaca akun TV (`/display-runtime/board`,
policy perangkat). Akun perawat tidak punya sumber data peta lantai maupun ringkasan per lantai untuk
pemilih lantai.

## 2. Proses bisnis

1. Perawat berizin `NurseViewPatientCall : Read` membuka Monitor perawat dan memilih lantai.
2. Frontend memanggil `GET /patient-calls/floor-board?floorCode=4` dan `GET /patient-calls/active?floorCode=4`.
3. Backend memvalidasi lantai (`VAL-NV-39`), lalu mengembalikan papan lantai yang sama persis dengan TV
   (identitas inisial · jenis kelamin · umur, tim jaga dari roster) dan ringkasan semua lantai.
4. Panggilan dan aksi tetap dari endpoint §7 yang sudah ada; endpoint baru hanya baca.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: `NV-DEC-058`, `NV-DEC-044`, api §7, §9; kartu `BE-NV-017`.
- Source: `NvwFloorBoardQueryService`, `NvwPatientCallQueryService.IsKnownFloorAsync`, `NvwDisplayDeviceService.GetFloorsAsync`,
  `NvwPatientCallController`, `NvwHttp`, `NvwResult`, `ApplicationDbContext` (`MstServiceUnits`, `MstRooms`, `MstBeds`, `NvwPatientCalls`).
- Governance: `AGENTS.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 45.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/DTOs/NvwFloorBoardDtos.cs` | **Baru di berkas:** `NvwNurseMonitorResponse` (`Board`, `Floors`, `ServerTimeUtc`), `NvwNurseMonitorFloorResponse` (`FloorCode`, `FloorName`, `WardNames`, `RoomCount`, `BedCount`, `WaitingCallCount`, `ClaimedCallCount`) |
| `Areas/HealthServices/NurseViewManagement/Services/NvwFloorBoardQueryService.cs` | `GetNurseMonitorAsync` (validasi lantai → `GetBoardAsync` apa adanya → ringkasan lantai); `GetMonitorFloorsAsync` (empat query terkelompok, tanpa query per baris) |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwPatientCallController.cs` | `GET floor-board` dengan `[AccessAction(Read)]` + `[AccessPermission(NurseViewPatientCall, Read)]`; konstruktor menerima `NvwFloorBoardQueryService` |

Diff source: +109/−0. Tanpa migration, tanpa berkas bersama.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru api §7a `0.2.2`; endpoint lain tidak berubah |
| Database | Tidak ada perubahan schema; hanya baca |
| Keamanan/Auth | Izin `NurseViewPatientCall : Read` (sama dengan `/active`); tanpa hardcode peran. Identitas memakai `GetBoardAsync` apa adanya, jadi tetap inisial · jenis kelamin · umur (`NV-DEC-044`) |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/patient-calls/floor-board?floorCode=` | Papan satu lantai + ringkasan semua lantai untuk Monitor perawat | `NurseViewPatientCall : Read` |

Pesan sukses: "Peta lantai berhasil diambil." `422` lantai tidak dikenal (`VAL-NV-39`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <3 berkas>` (array path) | Files evaluated **3** · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 11 Nov 2026 |
| Review diff/scope; akhiran baris | Hanya tiga berkas Nurse View; CRLF 99/519/150 baris, LF 0, tanpa BOM | `PASS` | `git diff`, hitungan byte Node |
| `dotnet build` (oleh Rizki) | DLL baru 11 Nov 13.15 WIB; endpoint menjawab `200` | `PASS` | Laporan Rizki + waktu DLL |
| `verify-authorization.sh --configuration Release` (oleh agent atas perintah Rizki "jalankan semua", 11 Nov 14.50 WIB) | `AUTHORIZATION VERIFIER: PASS`, exit 0. Modules 96, Resources 527, Actions 1998; [1] metadata gap 0; [2] fallback 68 cocok persis; [3] identitas kanonik tidak ganda; [4] identitas wajib 24/24; [5] endpoint bisnis telanjang 0 baru, 0 utang. Assembly Release belum ada, sehingga skrip memakai DLL Debug hasil build Rizki 13.15 WIB yang memuat endpoint ini. Hanya runner sementara yang dibangun; project utama tidak di-build | `PASS` | Keluaran skrip |
| Uji HTTP `uji-floor-board.mjs` F1–F5 (11 Nov 14.15 WIB) | **5/5 `PASS`**, 25 permintaan (200/201/403/422 sesuai rencana) | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-017-20261111/hasil-2026-11-11T07-15-52-642Z/` |
| Pindai log backend selama uji | 40 baris; 0 `Error`; 0 kemunculan kata sandi, nama, atau No. RM; 0 token | `PASS` | `hasil.json` → `pindaiLog` |
| Bersih-bersih | Panggilan uji 420-A ditutup (`200`), tablet uji dinonaktifkan (`200`); cek-sisa: tablet uji 0, policy Sari 22 dan Dewi 36 utuh | `PASS` | `hasil.json` → `bersihBersih`, `cek-sisa.mjs` |

Rincian uji HTTP (aktor: Sari `PERAWAT_RUANGAN` berizin tetap, DOKTER tanpa izin, TV `NVW-000001` lantai 4 yang
loginnya diatur ulang SuperAdmin untuk persiapan, tablet uji sementara di 420-A):

| No | Skenario | Bukti |
| --- | --- | --- |
| F1 | Sari `GET floor-board?floorCode=4` → `200` "Peta lantai berhasil diambil."; `Board` dibandingkan rekursif dengan `/display-runtime/board` TV lantai 4 | 0 beda di luar `asOfUtc`, `ageMinutes`, `isOverdue`, `overdueMinutes`; `serverTimeUtc` ada |
| F2 | `Floors` dibandingkan dengan `/floors` (kode, jumlah kamar, jumlah bed, urutan) dan hitungan DB | Sama: 2, 3, 4, 5, 8, 9. Lantai 4: Menunggu 1 (panggilan 420-B milik Rizki) → 2 sesudah tablet 420-A menekan → Menunggu 1, Diklaim 1 sesudah Sari mengklaim; semua lantai = DB |
| F3 | DOKTER, akun TV, akun tablet | `403`, `403`, `403` |
| F4 | `floorCode=ZZ9` dan tanpa `floorCode` | `422` `VAL-NV-39` "Lantai tidak dikenal. Pilih lantai dari daftar." (keduanya) |
| F5 | Kunci `fullName`/`medicalRecordNumber`/`patientId`/`episodeId`/`birthDate`; nama dan No. RM pasien lantai 4 | Kunci 0; identitas 0 dari 4 nilai; bed terisi hanya `P.B.U.`, `P.B.T.` + JK + umur |

Pra-cek pertama berhenti karena ada panggilan aktif di lantai 4 yang bukan buatan uji (tablet `NVT-000117` di 420-B,
ditekan 10.51 WIB, kemungkinan uji bunyi Rizki). Panggilan itu tidak disentuh. Skrip diubah agar memakai selisih
dari keadaan awal dan hanya mensyaratkan bed 420-A bersih.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Sari → `200`, `Board` sama dengan papan TV lantai yang sama | ✅ | F1 |
| AC-2 `Floors` dengan jumlah panggilan `Menunggu`/`Diklaim` | ✅ | F2 |
| AC-3 DOKTER, TV, tablet → `403` | ✅ | F3 |
| AC-4 lantai tak dikenal → `422` `VAL-NV-39` | ✅ | F4 |
| AC-5 tanpa nama lengkap dan No. RM | ✅ | F5 |
| AC-6 verifier otorisasi `PASS` | ✅ | `AUTHORIZATION VERIFIER: PASS` (§5) |
| DoD — AC-1–6 terbukti; laporan | Terpenuhi | Berkas ini |
| DoD — laporan | Terpenuhi | Berkas ini |

**QBE yang berlaku:** `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001`
(GET tidak dicatat), `QBE-PAGE-001` (satu lantai, tanpa paging sesuai kontrak).

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw` `ACTIVE`
(registry repository baris 45) · applicability `NEW CODE` · branch `rizkiG`. Selisih: registry global plugin
1.20.0 (`.claude/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) belum memuat `Nvw`; sumber
repository yang berlaku. `QBE-MOD-002` tidak berlaku (tanpa entity baru).

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Konstruktor `NvwPatientCallController` bertambah satu dependensi; build akan langsung menunjukkan bila ada masalah DI |
| Risiko tersisa | Rendah; identitas mengikuti papan TV |
| Status Git | Di-commit dan di-push 11 Nov 2026 atas perintah Rizki (branch `rizkiG`): `7f5926d8` source + dokumen, lalu commit status selesai |
| Langkah berikutnya | Merge ke branch integrasi oleh Rizki. Uji layar `FE-NV-016` sudah memakai endpoint ini (13/13 `PASS`) |
