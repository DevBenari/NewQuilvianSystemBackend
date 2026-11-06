# Laporan Perubahan Backend — `BE-NV-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-007` |
| Judul | Sinyal realtime saat TTV disimpan |
| Slice | `MVP-4` — push TTV |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-007` |
| Trace | `FR-NV-032`; `NV-DEC-023`, `026`, `027`; `02-backend-architecture.md` §4.8, §4.10; api §4; `TLM-NV-001` keperawatan permintaan 1 |
| Contract version | `0.1.2` — `approved`; api §4 tidak berubah sejak `0.1.0` |
| Dependency | `BE-NV-005` 🟡 (bagian papan yang dipakai task ini sudah terbukti) |
| Klasifikasi | `HEAVY` — menyentuh `QueueHub` bersama, satu controller milik keperawatan (restu `NV-DEC-027`, `TLM-NV-001`), dan keamanan grup realtime; tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `NvwRealtimeService.cs` (baru), `NvwDeviceContextService.cs`, `NvwFloorBoardDtos.cs`, `Hubs/QueueHub.cs`, `Program.cs`, `PatientVitalSignController.cs`; laporan; status roadmap; `TLM-NV-001` |
| Model | Claude Opus 5.5 |
| Commit backend | Ditulis di atas `7771096f`; di-commit Rizki sebagai `55335caf` (3 Nov 14.43). Build memakai `0efd9ecf` (perbaikan endpoint IGD milik Rizki) |
| Tanggal | Kode 2026-11-03; uji 2026-11-04 |
| Status | ✅ **SELESAI 4 November 2026** — QBE Strict `PASS` (6 berkas), build Rizki lulus (DLL 3 Nov 16.33). Uji hub 11/11 `PASS` (38 cek): TV lantai 3 menerima satu sinyal tanpa identitas untuk `POST`, `PUT`, dan `cancel`; TV lantai 2, `verify`, dan TTV IGD tidak memicu sinyal; akun manusia dan token basi ditolak; `JoinInpatientBedSelection` ditolak untuk akun TV. Kartu yang berubah di layar (`NV-AC-019` sisi tampilan) dibuktikan di `FE-NV-007`. Verifikasi developer, bukan UAT |

---

## 1. Masalah yang diselesaikan

Sebelum task ini, TV hanya melihat TTV baru lewat muat ulang setiap 60 detik (`FR-NV-030`). Pasien yang EWS-nya
memburuk bisa terlambat terlihat hampir satu menit.

**Contoh.** Pukul 14.20 perawat Sari menyimpan TTV pasien bed 325-B. Sebelumnya, TV lantai 3 baru memuat ulang paling
lambat pukul 14.21. Sesudah task ini, backend mengirim sinyal `NurseViewBoardChanged` ke grup TV lantai 3 pada detik yang
sama. Pada uji 4 November, sinyal sudah sampai sebelum jawaban HTTP simpan TTV diterima browser.

---

## 2. Proses bisnis

**Tujuan:** TV lantai pasien tahu bahwa kartu sebuah bed perlu dimuat ulang begitu TTV rawat inap pasien itu disimpan,
diubah, atau dibatalkan, tanpa menerima data pasien lewat sinyal.

**Pelaku:** perawat (menyimpan TTV), TV (akun perangkat yang bergabung ke grup lantai), backend (pengirim sinyal).

**Langkah normal:**

1. Saat halaman papan dibuka, TV memanggil `JoinNurseViewFloor()` di `QueueHub` tanpa parameter.
2. Hub mencari perangkat milik akun itu, memeriksa status `Active` dan cap sesi token (pemeriksaan yang sama dengan
   `/board`), lalu memasukkan koneksi ke grup `nurse-view-floor:{FloorCode}` milik perangkat.
3. Perawat menyimpan TTV lewat `PatientVitalSignController` (`POST`, `PUT`, atau `PATCH cancel`).
4. Sesudah `SaveChangesAsync` berhasil, controller memanggil `NvwRealtimeService.NotifyVitalSignChangedAsync(InpEpisodeId)`.
5. Service mencari penempatan bed aktif episode itu, lalu bed dan kamarnya. Untuk setiap bed aktif di kamar aktif yang
   punya `FloorCode`, service mengirim `{ eventType: "VitalSignChanged", bedId, floorCode, occurredAtUtc }` ke grup
   lantai itu.
6. TV yang menerima sinyal memuat ulang papan lewat `/board` (`FE-NV-007`).

**Jalur tidak normal:**

| Kejadian | Jawaban sistem |
| --- | --- |
| TTV tanpa `InpEpisodeId` (poliklinik, IGD) | Tidak ada sinyal |
| Episode tanpa penempatan aktif (Draft, dipesan, sudah pulang) | Tidak ada sinyal |
| Query atau pengiriman gagal | Galat ditangkap dan dicatat `LogError`; simpan TTV tetap `200` |
| Tidak ada TV tersambung, atau TV terputus mendadak | Pengiriman ke grup kosong tetap berhasil; simpan TTV `200` |
| Akun manusia, perangkat nonaktif, atau token terbit sebelum Atur Ulang Login memanggil `JoinNurseViewFloor` | `HubException` dengan pesan `VAL-NV-14`, `VAL-NV-15`, atau `NV-SESSION-STALE` |
| `PATCH verify` dan `PATCH notify-doctor` | Tidak memanggil sinyal (`TLM-NV-001` §3, disetujui Hamzah) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`Hubs/QueueHub.cs`, `InpBedRealtimeService.cs` (pola best-effort), `InpBedOccupancyService.BedManagement.cs`,
`InpBedAccessService.cs` (`CanSubscribeToBedSelectionUnitAsync`), `InpBedManagementQueryService.cs` (`ProjectBatchAsync`,
penempatan aktif), `InpBedPlacement.cs`, `PatientVitalSignController.cs`, `InpatientVitalSignService.cs`,
`EmergencyRealtimeService.cs`, `NvwDeviceContextService.cs`, `NvwDisplayRuntimeController.cs`, `NvwHttp.cs`,
`NvwFloorBoardQueryService.cs`, `NvwDisplayDeviceService.cs`, `Program.cs` (SignalR, JWT hub, policy, registrasi);
blueprint §4.8, §4.10, api §4, `TLM-NV-001`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../NurseViewManagement/Services/NvwRealtimeService.cs` | **Baru.** `BuildFloorGroupName`, `NotifyVitalSignChangedAsync(Guid? inpEpisodeId)`. Satu query penempatan aktif → bed → kamar, lalu satu `SendAsync` per bed. Seluruh isi dibungkus `try/catch` dengan `LogError` |
| `Areas/.../NurseViewManagement/Services/NvwDeviceContextService.cs` | `FindFloorCodeAsync(userId)` untuk `LeaveNurseViewFloor` |
| `Areas/.../NurseViewManagement/DTOs/NvwFloorBoardDtos.cs` | DTO payload `NvwBoardChangedEvent` (`EventType`, `BedId`, `FloorCode`, `OccurredAtUtc`) |
| `Hubs/QueueHub.cs` | Konstruktor menerima `NvwDeviceContextService`. `JoinNurseViewFloor()` dan `LeaveNurseViewFloor()` tanpa parameter. Metode lama tidak berubah |
| `Program.cs` | `AddScoped<NvwRealtimeService>()` (registrasi ke-5 dari 6 yang direstui `NV-DEC-030`) |
| `Areas/.../ClinicalManagement/Controllers/PatientVitalSignController.cs` | Konstruktor menerima `NvwRealtimeService`; satu baris `await _nurseViewRealtime.NotifyVitalSignChangedAsync(entity.InpEpisodeId);` sesudah `SaveChangesAsync` pada `POST`, `PUT`, `PATCH cancel` |
| `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001-…md` | Jawaban Hamzah dan status permintaan 1 selesai, dengan tautan ke laporan ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | api §4 kini tersedia. Endpoint, request, response, dan kode status `PatientVitalSign` tidak berubah |
| Database | Tanpa migration, hanya baca. `JoinNurseViewFloor` memperbarui `LastOnlineAt` perangkat lewat pemeriksaan yang sama dengan `/board` |
| Keamanan/Auth | Lantai selalu diambil dari perangkat, tidak dari klien. Payload tanpa nama, No. RM, ID pasien/episode/encounter, nilai TTV, maupun EWS. Akun TV tetap ditolak `JoinInpatientBedSelection`, karena izinnya diperiksa lewat Akses Role |
| Waktu respons simpan TTV | Bertambah satu query dan satu pengiriman sesudah commit. Terukur `PUT` 84 ms, `cancel` 51 ms, `POST` tanpa TV 55 ms; `POST` pertama 552 ms (panggilan pertama sesudah backend menyala) |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint HTTP baru. `QueueHub` (`/hubs/queues`):

| Metode hub | Pemanggil | Hasil |
| --- | --- | --- |
| `JoinNurseViewFloor()` | Akun perangkat `Active` dengan sesi berlaku | Masuk grup `nurse-view-floor:{FloorCode}` |
| `LeaveNurseViewFloor()` | Akun perangkat | Keluar grup; diam bila perangkat tidak ditemukan |

| Event | Contoh payload |
| --- | --- |
| `NurseViewBoardChanged` | `{ "eventType": "VitalSignChanged", "bedId": "54a97f6d-…", "floorCode": "3", "occurredAtUtc": "2026-11-04T01:28:5…Z" }` |

Penolakan dikirim sebagai `HubException`, misalnya "Akun ini bukan akun display Nurse View." atau "Sesi perangkat sudah
tidak berlaku. Silakan login ulang."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Checker QBE Strict, `-Path` 5 berkas | `Files evaluated: 5`, 0 violation, 0 review | `PASS` | Keluaran perintah, 3 Nov |
| Checker QBE Strict, `Program.cs` | `Files evaluated: 1`, 0 violation, 0 review | `PASS` | Keluaran perintah, 3 Nov |
| Grep komentar dan akhiran baris | 0 komentar baru; 6 berkas CRLF | `PASS` | Keluaran perintah |
| `dotnet build` oleh Rizki | DLL 3 Nov 16.33 (commit `0efd9ecf`); H1 membuktikan `JoinNurseViewFloor` dari build ini berjalan. Jumlah warning belum dinyatakan Rizki | `PASS` | DLL, H1 |
| H1 bergabung ke grup lantai | Prasyarat `/board` lantai 3 `200` dengan 325-B `Occupied`; tiga koneksi (dua di TV lantai 3, satu di TV lantai 2) `Connected` dan `JoinNurseViewFloor` diterima. 5 cek | `PASS` | `sinyal-2026-11-04T01-28-21-738Z/H1.json` |
| H2 akun manusia | SuperAdmin dan LOKET ditolak "Akun ini bukan akun display Nurse View." 2 cek | `PASS` | `H2.json` |
| H3 `NV-AC-022` | Akun TV `JoinInpatientBedSelection` ditolak "Anda tidak memiliki akses ke unit pelayanan ini." 1 cek | `PASS` | `H3.json` |
| H4 `POST` TTV pasien B | `200`; tersimpan sebagai observasi rawat inap (sumber 5); kedua koneksi lantai 3 menerima tepat satu sinyal, 0,03 detik sebelum jawaban HTTP sampai; payload hanya 4 kunci, lantai `3`, bed 325-B; tanpa ID pasien/episode/encounter; TV lantai 2 menerima 0. 9 cek | `PASS` | `H4.json` |
| H5 `PUT` | `200`, sinyal tiba 0,01 detik sebelum jawaban HTTP, payload 4 kunci. 3 cek | `PASS` | `H5.json` |
| H6 `PATCH verify` | `200`, 0 sinyal dalam 6 detik. 2 cek | `PASS` | `H6.json` |
| H7 `PATCH cancel` dan `LeaveNurseViewFloor` | Leave diterima; cancel `200`; koneksi yang masih bergabung menerima 1 sinyal, koneksi yang keluar 0. 4 cek | `PASS` | `H7.json` |
| H8 TTV IGD (`TLM-NV-001-AC-3`) | `POST` `200`, tersimpan bukan rawat inap (sumber 8), cancel `200`; TV lantai 3 dan 2 menerima 0. 5 cek | `PASS` | `H8.json` |
| H9 token basi | Sesudah Atur Ulang Login `NVW-000004`, koneksi lama ditolak "Sesi perangkat sudah tidak berlaku…". 2 cek | `PASS` | `H9.json` |
| H10 semua TV terputus mendadak (`TLM-NV-001-AC-2` sisi klien) | Kedua konteks TV ditutup tanpa Leave; `POST` `200` (55 ms), GET `200`, cancel `200`. 3 cek | `PASS` | `H10.json` |
| H11 log backend | `quilvian-backend-20261104.json`: 0 baris "Gagal mengirim sinyal papan Nurse View". 2 cek | `PASS` | `H11.json` |
| SELECT baca saja pada 3 TTV uji | Dua TTV pasien B terhubung ke episode B; TTV IGD tanpa episode; ketiganya `Cancelled` (4) dan tidak aktif | `PASS` | Keluaran perintah, 4 Nov |
| Pencarian kredensial pada bukti JSON | 12 berkas, 0 sandi, 0 token | `PASS` | Pencarian pola dan nilai env |

Bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-007-20261103/`. Klien hub adalah
`@microsoft/signalr` 8.0.17 milik frontend, dimuat ke halaman TV dan halaman admin di Edge headless, sehingga
autentikasinya memakai cookie sesi yang sama dengan aplikasi.

**`TLM-NV-001-AC-2` sisi server.** SignalR berjalan di proses yang sama dengan API, jadi hub tidak bisa dimatikan
terpisah. Bila prosesnya mati, simpan TTV ikut mati. Kegagalan yang mungkin terjadi adalah query penempatan atau
`SendAsync`. Keduanya berada di dalam `try/catch` yang hanya mencatat `LogError`, dan pemanggil tidak menerima
pengecualian. Jalur ini terbukti lewat review kode; jalur klien terputus terbukti lewat H10.

Uji manual: `PASS` untuk H1–H11.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Tidak dijalankan:** `tools/authorization-verifier/verify-authorization.sh` (`NOT RUN`; tidak ada atribut akses yang
berubah, dan skrip itu memanggil `dotnet build` milik Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-019` TTV disimpan → kartu berubah tanpa menunggu 60 detik | **Sisi backend terpenuhi:** sinyal tiba di TV lantai pasien sebelum jawaban simpan TTV sampai. Perubahan kartu di layar dibuktikan `FE-NV-007` | H4, H5, H7 |
| `NV-AC-022` akun TV ditolak `inpatient-bed-selection`; sinyal tanpa nama, No. RM, nilai TTV | Terpenuhi | H3, H4, H5 |
| `TLM-NV-001-AC-1` commit berhasil → satu sinyal ke TV lantai pasien | Terpenuhi | H4 |
| `TLM-NV-001-AC-2` hub tidak tersedia → simpan TTV tetap berhasil | Terpenuhi: klien terputus lewat H10, kegagalan server lewat review kode (§5) | H10, source |
| `TLM-NV-001-AC-3` TTV tanpa `InpEpisodeId` → tanpa sinyal | Terpenuhi | H8 |
| `TLM-NV-001-AC-4` sinyal tanpa nama, No. RM, nilai TTV, EWS | Terpenuhi | H4 |
| Verifikasi kartu: join → simpan TTV → event diterima | Terpenuhi | H1, H4 |
| Verifikasi kartu: matikan hub → simpan TTV tetap `200` | Terpenuhi dengan padanan H10 (hub tidak bisa dimatikan terpisah) | H10 |
| Verifikasi kartu: akun TV `JoinInpatientBedSelection` → ditolak | Terpenuhi | H3 |
| Prasyarat kerja: konfirmasi Hamzah soal penulis dan pemilik `PatientVitalSignController` | Terpenuhi 3 Nov: Rizki menulis di task Nurse View; controller milik Hamzah (`RWI-DEC-061`, `062`) | `TLM-NV-001` §7 |
| DoD — laporan dirujuk dari folder TLM keperawatan | Terpenuhi | `TLM-NV-001` §7 baris 2026-11-04 |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw` `ACTIVE` ·
applicability `NEW CODE` (`NvwRealtimeService`, DTO, dua metode hub). `Hubs/QueueHub.cs` dan `Program.cs` = berkas
bersama, restu lead `NV-DEC-030`, hanya penambahan. `PatientVitalSignController.cs` (ClinicalManagement, milik Hamzah) =
`TOUCHED LEGACY`, restu `NV-DEC-027` dan `TLM-NV-001`. Pelanggaran lama di controller itu (`ApplicationDbContext`
langsung, `QBE-SVC-001`) tidak diperbaiki karena di luar wewenang.

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (query di service Nurse View), `QBE-DTO-001` (payload DTO, bukan
entity), `QBE-MOD-001` (service di modul pemilik).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration |
| Data uji di DB dev | Tiga TTV uji (dua pasien B 325-B, satu encounter IGD), semuanya sudah dibatalkan. Sandi `NVW-000003` dan `NVW-000004` direset skrip |
| Peringatan | `NOT RUN`: `authorization-verifier`. Jumlah warning build belum dinyatakan Rizki |
| Masalah yang diketahui | (1) TTV ukuran dokter dari catatan SOAP rawat inap (`DoctorConsultationVitalSignService`) belum mengirim sinyal; TV melihatnya lewat muat ulang 60 detik. Hamzah: task terpisah bila dibutuhkan. (2) Koneksi yang sudah bergabung tetap di grup sampai terputus, walaupun login perangkatnya diatur ulang. Dampaknya kecil karena payload tanpa identitas, dan TV keluar ke login paling lambat 60 detik lewat `/board` `403` |
| Temuan di luar Nurse View | `QueueHub.JoinEmergencyQueue` tidak memeriksa apa pun selain login, jadi akun TV pun bisa bergabung. Payload `EmergencyRealtimeEvent` memuat `PatientId` dan `EncounterId`. TV Nurse View tidak pernah memanggilnya; kode IGD tidak disentuh. Diteruskan ke pemilik IGD |
| Risiko tersisa | Autosave Draft lewat `PUT` ikut memicu sinyal; TV memuat ulang papan tanpa perubahan data (diterima Hamzah) |
| Perubahan sampingan | `NONE` |
| Status Git | Source di `55335caf`. Laporan ini, status roadmap, traceability, dan `TLM-NV-001` belum di-commit |
| Langkah berikutnya | `FE-NV-007`: papan bergabung ke grup lantai dan memuat ulang saat `NurseViewBoardChanged` |
