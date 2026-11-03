# Laporan Perubahan Backend — `BE-NV-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-005` |
| Judul | Papan lantai tersusun dan tersamarkan |
| Slice | `MVP-2` — papan lantai |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-005` |
| Trace | `FR-NV-010`–`016`, `021`; `NV-DEC-006`, `007`, `008`, `011`, `013`, `015`, `018`, `019`, `020`, `025`, `026`, `027`; `02-backend-architecture.md` §4.6–§4.7; api §3 `/board` |
| Contract version | `0.1.1` — `approved`; bagian `/board` sama dengan `0.1.0` |
| Dependency | `BE-NV-003` ✅, `BE-NV-004` ✅ |
| Klasifikasi | `HEAVY` — data pasien dikirim ke perangkat tanpa pengguna manusia (risiko kebocoran identitas), membaca lima sumber data lintas modul, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, satu baris `Program.cs`, laporan, status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d936447e` (branch `rizkiG`); source di-commit Rizki sebagai `6489c32c` |
| Tanggal | 2026-11-02 sampai 2026-11-03 |
| Status | 🟡 **SEBAGIAN 3 November 2026** — QBE Strict `PASS` (5 berkas), build Rizki lulus (3 Nov 08.50), uji papan 8/8 `PASS` (51 cek), penolakan akun manusia 2/2 `PASS`, nol kebocoran identitas. Belum terbukti dengan data nyata: `NV-AC-011` (instrumen risiko jatuh belum disahkan) dan `NV-AC-013` (isolasi hanya bisa disetel DPJP berizin). `NV-AC-021` tidak bisa diuji karena tidak ada datanya di DB dev. Verifikasi developer, bukan UAT |

---

## 1. Masalah yang diselesaikan

Sesudah `BE-NV-003`, TV sudah bisa login tetapi hanya mengenal `GET /current`. Isi papan belum ada, sehingga TV
hanya menampilkan "Papan lantai sedang disiapkan."

**Contoh.** Pasien di bed 325-A lantai 3 baru diukur dengan EWS 17. Sebelumnya perawat di nurse station baru tahu
keadaannya bila membuka rekam medis. Sekarang `GET /board` milik TV lantai 3 menaruh kartu 325-A di urutan pertama
bangsalnya dengan `isCritical = true`, berisi "P.B.T. · L · 31 th", nama perawat PJ, EWS 17, dan nilai TTV, tanpa
nama lengkap maupun No. RM.

---

## 2. Proses bisnis

**Tujuan:** TV hanya membaca papan lantainya sendiri, dengan identitas pasien yang sudah disamarkan sejak di server.

**Pelaku:** TV (akun perangkat). Sumber data: Rawat Inap (bed, penempatan, episode, perawat PJ), Klinis (TTV,
pengkajian risiko jatuh, alergi), dan Nurse View (interval ukur ulang).

**Langkah normal:**

1. TV memanggil `GET /board` tanpa parameter. Lantai diambil dari perangkat yang sedang login, bukan dari permintaan.
2. Sistem mengambil kamar aktif dengan `FloorCode` lantai itu, lalu bed aktif di kamar tersebut.
3. Status bed dan penempatan berasal dari `InpBedManagementQueryService.ProjectBatchAsync` milik Rawat Inap.
   Method ini sudah `internal` dan dipanggil apa adanya (restu Hamzah); `HoldingContext` diabaikan.
4. Untuk bed terisi, identitas langsung disamarkan saat dibaca. Nama menjadi inisial (maksimal tiga huruf), kelamin
   menjadi L/P, dan tanggal lahir menjadi umur dalam th/bln/hr menurut tanggal Asia/Jakarta. Nama lengkap, No. RM,
   dan tanggal lahir tidak pernah masuk DTO.
5. Sistem menambahkan:
   - perawat PJ aktif;
   - TTV sah terakhir (`Recorded`, `Verified`, `Corrected`; TTV yang dibatalkan diabaikan);
   - tanda risiko jatuh, bila pengkajian `FallRisk` berstatus `Completed` yang terakhir bernilai `HighRisk`;
   - tanda alergi aktif berupa ya/tidak, tanpa nama zat;
   - tanda isolasi dan "tidak sesuai isolasi" bila kebutuhan isolasi pasien berbeda dengan jenis bed.
6. Tanda lewat jadwal hanya muncul bila interval untuk tingkat EWS itu sudah disahkan. Lama keterlambatan dihitung
   sebagai umur TTV dikurangi interval.
7. Kartu dikelompokkan per bangsal. Kartu kritis ditaruh paling atas.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem | Kode |
| --- | --- | --- |
| Akun manusia, termasuk SuperAdmin, membuka `/board` | Ditolak policy, tanpa isi | `403` |
| Perangkat dinonaktifkan atau sesinya basi | `VAL-NV-15` atau `NV-SESSION-STALE` (dari `BE-NV-003`) | `403` |
| Perangkat lantai 3 mengirim `?floorCode=4` | Parameter diabaikan, jawaban tetap lantai 3 | `200` |
| Lantai tanpa kamar ber-`FloorCode` | Papan dengan `wards` kosong | `200` |
| Interval belum disahkan | `intervalsConfigured = false`, tidak ada kartu bertanda lewat jadwal | `200` |
| Bed dipesan | Kartu tanpa data pasien, label "Dipesan — pasien akan datang" | `200` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpBedManagementQueryService.cs` (`ProjectBatchAsync`), `InpBedOccupancyService.cs`, `InpFloorPlanQueryService.cs`,
`FloorPlanRoomNaming`, model `InpEpisode`, `InpNurseAssignment`, `TrxPatientVitalSign`, `TrxPatientAssessment`,
`TrxPatientAllergy`, `MstRoom`, `MstBed`, `NvwVitalSignIntervalService.cs`, `NvwDeviceContextService.cs`; blueprint
§4.6–§4.7, api §3, `data/data-dictionary.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/NvwFloorBoardQueryService.cs` | **Baru.** Langkah 1–8 dan 10–11 §4.6. Semua data pendukung dimuat per kelompok episode atau pasien, bukan per bed. Langkah 9 (roster) milik `BE-NV-006`, sehingga `RosterAvailable` selalu `false` |
| `Areas/.../Helpers/NvwPatientMasking.cs` | **Baru.** `Initials`, `GenderCode`, `AgeLabel`; data kosong ditampilkan "–" atau "Umur –" |
| `Areas/.../DTOs/NvwFloorBoardDtos.cs` | **Baru.** `NvwFloorBoardResponse`, `NvwWardBoardResponse`, `NvwOnDutyNurseResponse`, `NvwBedCardResponse`, `NvwMaskedPatientResponse`, `NvwVitalSignCardResponse` sesuai api §3 |
| `Areas/.../Controllers/NvwDisplayRuntimeController.cs` | `GET /board` dengan policy `NurseViewDisplayRead`. Sebelum membaca papan, perangkat dan sesinya diperiksa ulang lewat `ResolveActiveDeviceAsync` |
| `Program.cs` | Satu baris `AddScoped<NvwFloorBoardQueryService>()` |

Tidak ada kode Rawat Inap atau Klinis yang diubah. Sesuai `TLM-NV-001` di folder episode rawat inap, akses ke modul
itu hanya baca.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /board`, bentuknya sesuai api §3 |
| Database | Tanpa migration, hanya baca |
| Keamanan/Auth | Policy yang sama dengan `/current`; tanpa jalan pintas SuperAdmin. Lantai tidak bisa dipilih dari permintaan. Identitas disamarkan sebelum masuk DTO, sehingga data asli tidak pernah ada di jawaban |

### 3.4 Keputusan developer dan delta

1. **Kartu dipesan tanpa pasien.** Data pasien hanya diisi untuk bed `Occupied`. Bed `Reserved` cukup berlabel
   "Dipesan — pasien akan datang" (`NV-AC-010`), karena pasiennya belum berada di bed itu.
2. **Urutan kartu.** Backend mengurutkan kartu kritis dulu, lalu menurut `BedCode`. Di DB dev, `BedCode` ternyata kode
   internal seperti `BD-RSMMC-00029`, sedangkan "325-A" adalah `BedName`. Contoh "301-A" di api §3 sebenarnya
   `BedName`. Akibatnya urutan dari API tidak sama dengan urutan nama. `FE-NV-005` mengurutkan ulang menurut
   `BedName`, sehingga tampilan di TV sudah benar. Lihat §7.
3. **Persiapan data uji.** Pasien uji dibuat dengan skrip `pasien-uji.mjs` dan `klinis-uji.mjs`, yang memanggil
   endpoint aplikasi memakai sesi SuperAdmin dan akun PERAWAT. Rizki semula memilih agar data dibuat lewat layar.
   Akibatnya layar Rawat Inap dan Klinis tidak ikut teruji di task ini. Papan, penolakan, dan tampilan TV tetap
   diuji dari layar.

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Display Runtime

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/display-runtime/board` | Papan lantai milik perangkat yang login: bangsal, kartu bed, identitas samaran, TTV terakhir, tanda keselamatan | policy `NurseViewDisplayRead` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Checker QBE Strict, `-Path` 5 berkas | `Files evaluated: 5`, 0 violation, 0 review | `PASS` | Keluaran perintah, 3 Nov |
| `dotnet build` oleh Rizki | Build 3 Nov 08.50. `/board` tanpa login kini `401`, artinya route terdaftar. Jumlah warning belum dinyatakan Rizki | `PASS` | Pemeriksaan route |
| B1 kebocoran identitas | Isi `/board` dicari untuk 7 nama lengkap, 7 No. RM, 7 tanggal lahir, 7 `PatientId`, 7 `EpisodeId`, nama alergen, dan nama DPJP: semuanya **nol kemunculan**. Tidak ada kunci `fullName`, `medicalRecordNumber`, `birthDate`, `patientId`, `episodeId`, `allergenName`, `doctorName`, `holdingContext`, `diagnosis` | `PASS` | `papan-2026-11-03T02-15-07-243Z/B1.json` |
| B1 waktu respons, 66 bed | 5 panggilan: 189, 106, 121, 97, 197 ms; median **121 ms** | `PASS` | Idem |
| B2 isi kartu | A: samaran, Terisi, EWS 17 `Critical`, kartu pertama, PJ, tanda alergi. B: TTV lama `High`. C: tanpa PJ, tanpa TTV. E: dipesan tanpa pasien. G: tanpa PJ. Bed kosong berlabel Kosong. 16 cek | `PASS` | `B2.json` |
| B3 tanpa interval | `intervalsConfigured = false`; TTV B berumur 275 menit tidak ditandai | `PASS` | `B3.json` |
| B4 interval Tinggi 240 menit | B ditandai lewat jadwal 35 menit (275 − 240); A pada tingkat lain tidak ditandai; sesudah interval dicabut, tanda hilang | `PASS` | `B4.json` |
| B5 TTV dibatalkan | TTV baru (`Low`, 5 menit) tampil, lalu dibatalkan; kartu kembali ke TTV `High` sebelumnya | `PASS` | `B5.json` |
| B6 pindah lantai | Transfer D ke 420-A lantai 4 `200`. Di lantai 3 bed 326-A menjadi `WaitingCleaning`, "Dibersihkan", tanpa pasien; di lantai 4 pasien muncul beserta TTV-nya | `PASS` | `B6.json` |
| B7 lantai lain | `?floorCode=4` dari perangkat lantai 3 tetap menjawab lantai 3, tanpa bed lantai 4 | `PASS` | `B7.json` |
| B8 dua bangsal | Perangkat lantai 2 (`NVW-000004`): ICU dan Ruang Bayi, 17 kartu, sama dengan jumlah bed aktif lantai 2 di DB; bed bayi bertanda `isNewbornBed` | `PASS` | `B8.json`, `board-lantai-2.json` |
| T1–T2 akun manusia | SuperAdmin dan LOKET: `GET /board` `403` dengan isi kosong | `PASS` | `tolak-2026-11-03T02-43-22-993Z` |
| L9 perubahan data | TTV baru tiba di TV dalam 57 detik dan pembatalannya dalam 60 detik, tanpa memuat ulang halaman | `PASS` | `segar-2026-11-03T02-38-35-982Z` |
| Uji TV `BE-NV-003` diulang dengan papan sungguhan | N1–N9 `PASS` (67 cek); perangkat dinonaktifkan tetap ditolak `VAL-NV-15` | `PASS` | `uji-nv-003-20261102/tv-2026-11-03T02-01-15-524Z` |
| Pencarian kredensial pada 42 JSON bukti | Nol sandi, nol token | `PASS` | Pencarian pola dan nilai env |

Seluruh bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-005-20261103/`, kecuali yang
disebut lain. Jawaban `/board` lengkap disimpan sebagai `board-lantai-3.json` dan `board-lantai-2.json`.

**Riwayat.** Putaran pertama `papan-2026-11-03T02-14-38-079Z` berhenti dengan `Failed to fetch`. Penyebabnya skrip
uji: panggilan akun TV dijalankan dari halaman tiruan. Skrip diganti memakai `page.request`, dan produk tidak diubah.

Uji manual: `PASS` untuk B1–B8, T1–T2, dan L9.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Tidak dijalankan:** `tools/authorization-verifier/verify-authorization.sh` (`NOT RUN`, memanggil `dotnet build`,
milik Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-001` kartu per bangsal; kamar tanpa `FloorCode` tidak tampil | Terpenuhi untuk pengelompokan. Bagian kamar tanpa `FloorCode` hanya terpetakan ke source (`FloorCode != null`), karena DB dev tidak punya kamar aktif tanpa `FloorCode` | B8; SELECT jumlah bed per lantai |
| `NV-AC-002` samaran, tanpa nama lengkap dan No. RM | Terpenuhi | B1, B2 |
| `NV-AC-003` kritis di urutan teratas | Terpenuhi dari sisi data; merah berkedip dibuktikan `FE-NV-005` | B2 |
| `NV-AC-004` tanpa interval, tanpa tanda | Terpenuhi | B3 |
| `NV-AC-005` lewat jadwal | Terpenuhi; diuji pada tingkat Tinggi, bukan Sedang, dengan logika yang sama | B4 |
| `NV-AC-007` TTV dibatalkan | Terpenuhi | B5, L9 |
| `NV-AC-008` tanpa PJ | Terpenuhi | B2 (C, G) |
| `NV-AC-010` dibersihkan dan dipesan | Terpenuhi | B2 (E), B6 |
| `NV-AC-011` hanya Morse tinggi bertanda | **Belum terbukti dengan data nyata.** Pengkajian A (skor 65, `HighRisk`) dan B (skor 30) hanya bisa tersimpan `InProgress`, karena penyelesaian ditolak `422` `INSTRUMENT_NOT_APPROVED`. Service hanya membaca pengkajian `Completed`, sehingga keduanya benar tanpa tanda, tetapi kasus positifnya belum terlihat | Source; `klinis-uji.mjs` |
| `NV-AC-012` alergi tanpa nama zat | Terpenuhi | B1, B2 |
| `NV-AC-013` isolasi tidak sesuai | **Belum terbukti dengan data nyata.** Isolasi sesudah rawat hanya bisa disetel DPJP dengan izin `InpatientEpisode:SetIsolation`, sedangkan akun DOKTER dev tidak punya izin itu (`403`). Bed isolasi lantai 3 (316-A, 317-A) berada di kelas VIP dengan `IsForInpatient = false`, sehingga tidak bisa dipakai admisi. Logika `RequiresIsolation != IsIsolationBed` terpetakan ke source dan sesuai `NV-DEC-020` (dua arah) | Source |
| `NV-AC-014` pindah lantai | Terpenuhi lewat HTTP; jeda paling lama 60 detik di layar dibuktikan `FE-NV-005` | B6, L9 |
| `NV-AC-015` lantai lain ditolak | Terpenuhi: lantai tidak bisa dipilih, dan akun manusia ditolak | B7, T1–T2 |
| `NV-AC-021` kamar tanpa `FloorPlanRoomKey` tetap tampil | **Tidak bisa diuji.** Semua kamar aktif di DB dev punya `FloorPlanRoomKey`. Terpetakan ke source: query kamar hanya menyaring `FloorCode` | Source; SELECT per lantai |
| `UAT-04` | Terverifikasi developer sebagian: samaran dan tanda alergi terbukti, tanda risiko jatuh belum (sama dengan `NV-AC-011`). UAT belum dijalankan | B2 |
| `UAT-05` | Terverifikasi developer; UAT belum dijalankan | B1 |
| `UAT-06` | Terverifikasi developer; UAT belum dijalankan | T1–T2 |
| `NFR-001` jumlah query tetap | Terpetakan ke source: tujuh query di service ini, ditambah query interval dan `ProjectBatchAsync`, tanpa query di dalam perulangan per bed. Jumlah query tidak diukur dari log SQL | Source |
| `NFR-002` ≤ 2 detik | Terpenuhi untuk 66 bed (median 121 ms). Catatan: hanya 5 bed yang terisi; lantai dengan 40 bed terisi belum diukur | B1 |
| `NFR-005` Asia/Jakarta | Terpenuhi: umur dihitung dengan tanggal Asia/Jakarta; jam dikirim dalam UTC dan ditampilkan TV dalam Asia/Jakarta | Source, `FE-NV-005` L1 |
| DoD — AC terpetakan | Terpenuhi, dengan tiga butir belum terbukti di atas | §6 |
| DoD — JSON bukti tersimpan | Terpenuhi | `board-lantai-3.json`, `board-lantai-2.json` |
| Risiko kartu — uji pencarian teks identitas | Terpenuhi | B1 |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw` `ACTIVE` ·
applicability `NEW CODE`; `Program.cs` = `TOUCHED LEGACY` (satu baris registrasi, restu `NV-DEC-030`).

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (query di service, controller tipis), `QBE-API-001`, `QBE-DTO-001`
(entity tidak dikembalikan langsung), `QBE-PERM-001` (policy terdaftar dari `BE-NV-003`).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration |
| Data uji di DB dev | Episode A (325-A), B (325-B), C (325-C), D (dipindah ke 420-A lantai 4; 326-A menunggu dibersihkan), E (reservasi 326-B), F (Draft, butuh isolasi), G (326-C). PJ = pegawai yang tertaut akun PERAWAT. TTV uji, dua di antaranya dibatalkan. Alergi "ZatUjiNurseView". Dua pengkajian risiko jatuh `InProgress`. Perangkat baru `NVW-000004` "TV Uji Lantai 2". Riwayat interval berujukan `UJI-DEV-NV-005` (sekarang kosong lagi). Sandi `NVW-000001`, `NVW-000003`, `NVW-000004`, `KSK-RSMMC-00009`, `QDD-RSMMC-00003` sudah direset dan hanya pernah ada di memori skrip |
| Peringatan | `NOT RUN`: `authorization-verifier`. Jumlah warning build belum dinyatakan Rizki |
| Masalah yang diketahui | (1) Urutan API memakai `BedCode` internal; perlu diganti `BedName` pada perubahan backend berikutnya. Tampilan TV tidak terdampak. (2) `RosterAvailable` selalu `false` sampai `BE-NV-006` |
| Masalah data master | Kamar VIP lantai 3 (34 bed, termasuk bed isolasi 316-A dan 317-A) memakai kelas dengan `IsForInpatient = false`, sehingga tidak bisa dipakai admisi. Data ini di luar Nurse View; perlu disampaikan ke pemilik master data |
| Risiko tersisa | Tanda risiko jatuh dan tanda isolasi belum pernah terlihat dengan data asli. Tanda risiko jatuh bergantung pada prasyarat go-live `G-06` (versi instrumen `Approved`) |
| Perubahan sampingan | `NONE` |
| Status Git | Source ada di commit `6489c32c`. Laporan ini dan pembaruan status roadmap belum di-commit |
| Langkah berikutnya | Keputusan Rizki untuk `NV-AC-011` dan `NV-AC-013`; lalu `BE-NV-006` dan `BE-NV-007` |
