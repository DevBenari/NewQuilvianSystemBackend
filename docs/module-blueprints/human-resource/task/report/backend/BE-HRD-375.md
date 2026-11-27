# Laporan Perubahan Backend — `BE-HRD-375`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-375` |
| Judul | SPPD dibuat sebagai Draf, isi terkunci, dan endpoint Ajukan |
| Slice | `S-D5` amandemen / `MVP-70` — gelombang 2 · `EPIC HRD-121` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-375` |
| Trace | `FR-TRV-26`, `FR-TRV-27`, `FR-TRV-32` (bagian ajukan); `HRD-DEC-224`, `HRD-DEC-225`, `HRD-DEC-227`; API 34.1/34.4/34.5; Validation `VR-TRV-26` s.d. `VR-TRV-29`; State 32.2; Permission 32.1; AT `AT-HRD-TRV-23`, `24`, `25`, `34` (ajukan), `37` |
| Contract version | `v28` — approved 19 November 2026; tidak diubah |
| Dependency | `BE-HRD-374` ✅ |
| Klasifikasi | `MEDIUM` — satu repository, 6 berkas source (2 baru), perubahan perilaku API (`POST /` jadi Draf, `PUT` dibatasi, endpoint baru), integrasi Workflow V2 |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "lanjut BE-HRD-375"; data uji devYoga diizinkan: "Boleh, buat data uji SPPD di devYoga") |
| Target tulis | `BusinessTravelRequestService.cs`, `BusinessTravelRequestsController.cs`, `BusinessTravelDtos.cs`, `Constants/BusinessTravelRequestStatuses.cs` (baru), `Services/BusinessTravelRequestException.cs` (baru); uji baru `TravelSubmitTests.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d7d4eb3b` (branch `yoga`); `BE-HRD-374` dan dokumen `v28` belum di-commit |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai (19 November 2026) — 6/6 acceptance criteria terbukti: harness 20 uji (Workflow V2 nyata, kontrol negatif 10 mutasi tertangkap), proyek uji HR 347/347 (359/359 sesudah `BE-HRD-376`), dan API devYoga dengan akun pemohon asli Dewi Anggraeni (atasan Cahyo Pamungkas) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `BusinessTravelManagement` |
| Pemilik / prefix registry | Submodule tidak memiliki entri registry sendiri; task tidak membuat entity (hanya mengisi kolom `TrxBusinessTravelRequest` yang ada dan memanggil Workflow V2 `Wfl`, `ACTIVE / LEGACY`), sehingga `QBE-MOD-002` tidak berlaku |
| Keberlakuan | `TOUCHED LEGACY` (service, controller, DTO SPPD); `NEW CODE` (konstanta status, pengecualian modul, alur Ajukan) |
| QBE yang berlaku | `QBE-SVC-001` (alur di service, controller hanya memetakan hasil), `QBE-API-001` (`ApiResponse`, kode `200/201/400/403/404/409` kontrak), `QBE-PERM-001` (`[AccessPermission("BusinessTravelRequest","Create")]` = `ControllerName` + `[AccessAction("Create")]`), `QBE-DTO-001` (DTO, bukan entity), `QBE-VAL-001` (`VR-TRV-26` s.d. `29`), `QBE-TXN-001` (pemulihan bila submit gagal), `QBE-ENUM-001` (status dimiliki modul), `QBE-LOG-001` (aktor pada `SubmittedByUserId`/`UpdateBy`) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` (tanpa entity/schema/migration) |
| Utang lama yang tidak disentuh | Penomoran `TravelRequestNumber` memakai `CountAsync() + 1` (`QBE-CODE-003` untuk `NEW CODE`; bagian ini `TOUCHED LEGACY` di luar cakupan) — diusulkan task baru |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill |

---

## 1. Kondisi as-is terhadap kontrak

| Hal | As-is (`d7d4eb3b`) | Kontrak `v28` |
| --- | --- | --- |
| Buat SPPD | Langsung `Submitted`, `SubmittedAt` diisi, `ApprovedBudgetAmount = estimasi`; respons `BusinessTravelResponseDto` | `Draft`, anggaran disetujui 0; respons `BusinessTravelDetailResponseDto` (34.1) |
| Ubah SPPD | Boleh `Draft`/`Submitted`; menyalin estimasi ke `ApprovedBudgetAmount`; galat apa pun `400` | Hanya `Draft`/`NeedRevision`/`Submitted` lama tanpa workflow; selain itu `409` (`VR-TRV-27`) |
| Ajukan | `submitTravelRequest` FE melempar "belum tersedia"; tidak ada endpoint | `POST /{id}/submit` (34.1) |
| Detail | Tanpa ruas persetujuan | Tujuh ruas 34.4 |

**Fakta Workflow V2 yang menentukan desain:** `CreateAsync` dan `SubmitAsync` masing-masing membuka transaksi `Serializable` sendiri, sehingga Ajukan tidak dapat dibungkus satu transaksi luar. Penyetuju ditentukan saat `CreateAsync` (`WorkflowService.cs` baris ±600–680); `SubmitAsync` menerima `Draft`/`RevisionRequested`/`Returned` (baris 851–865) dan mengubah `DbUpdateException` menjadi `409`. Penyelarasan referensi (`HandleAsync`) berjalan **sesudah commit** dan menelan galat (baris 728, 972) — berdampak pada `BE-HRD-376` (lihat bagian 5).

---

## 2. Perubahan yang dikerjakan

| Berkas | Status | Isi |
| --- | --- | --- |
| `Constants/BusinessTravelRequestStatuses.cs` | Baru | `Draft`, `Submitted`, `NeedRevision`, `Approved`, `Rejected`, `Cancelled`, `Completed` |
| `Services/BusinessTravelRequestException.cs` | Baru | Pengecualian modul membawa kode HTTP (`403`, `409`, atau kode Workflow V2) |
| `DTOs/BusinessTravelDtos.cs` | Diubah | `BusinessTravelDetailResponseDto` + `WorkflowInstanceId`, `CurrentApprovalStep`, `SubmittedAt`, `ApprovedAt`, `RejectedAt`, `IsApproved`, `CanSubmit` (keterangan lewat `<summary>` properti) |
| `Services/BusinessTravelRequestService.cs` | Diubah | `CreateRequestAsync` → `Draft`, anggaran 0, mengembalikan detail; `UpdateRequestAsync` → `409` di luar status terbuka, tidak lagi menyalin anggaran; **baru** `SubmitRequestAsync`: 404 → 403 bukan pemohon → 409 ditolak final → 409 status tidak sah → pakai ulang instance `Draft`/`RevisionRequested`/`Returned` atau `CreateAsync` (`BUSINESS_TRAVEL_REQUEST`, `HRD_TRV_REQUEST`, nomor SPPD, konteks berisi nominal) → simpan id definisi/instance → `SubmitAsync` → bila gagal: bersihkan change tracker, kembalikan id workflow SPPD, hapus lunak draf workflow yang baru dibuat (pola `BE-HRD-FIX-002`), teruskan galat → bila berhasil: `Submitted`, `SubmittedAt`/`ByUserId`, `CurrentApprovalStep`. Peta kode: `404` engine (definisi tidak aktif) → `409`, `401` → `400`, lainnya apa adanya. Konstruktor menerima `WorkflowService` (DI sudah ada) |
| `Controllers/BusinessTravelRequestsController.cs` | Diubah | Action `SubmitRequest` `[HttpPost("{id}/submit")]` dengan `[AccessAction]`/`[AccessPermission]` sama dengan `POST /`, `[ProducesResponseType]` 200/400/403/404/409; `UpdateRequest` memetakan `409`/`404`; `CreateRequest` bertipe detail; `<summary>` XML dibuang dari action yang disentuh dan tidak ditambahkan pada action baru |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, 0 galat | `PASS` | Keluaran build |
| `dotnet test --filter TravelSubmitTests` (SQLite; `WorkflowService` + `HumanResourceContextService` nyata, definisi dari seeder `BE-HRD-374`) | 20/20 lulus | `PASS` | Keluaran `dotnet test` |
| — Buat SPPD | `Draft`, tanpa workflow, `CanSubmit`, anggaran 0, estimasi 5.000.000 | `PASS` (AT-23) | idem |
| — Ajukan oleh Sari | `Submitted`, instance `HRD_TRV_REQUEST` (definisi `BUSINESS_TRAVEL_REQUEST`) berstatus menunggu, satu tugas untuk akun Bima (atasan langsung), tahap 1 | `PASS` (AT-24) | idem |
| — Ubah Draf lalu ubah saat menunggu | Draf: tersimpan, anggaran tetap 0; menunggu: `409` pesan `VR-TRV-27`, tujuan tidak berubah | `PASS` (AT-25) | idem |
| — Ubah status `Approved`/`Rejected`/`Cancelled`/`AdvancePaid` | `409` | `PASS` | idem |
| — Ajukan dari `Approved`/`Cancelled`/`AdvancePaid`; ajukan dua kali | `409` pesan `VR-TRV-26`; tetap satu instance | `PASS` | idem |
| — Ajukan SPPD `Rejected` | `409` "SPPD yang ditolak tidak dapat diajukan ulang. Buat SPPD baru." | `PASS` (`VR-TRV-28`) | idem |
| — Ajukan oleh Budi / tanpa profil | `403` "Hanya pemohon SPPD yang dapat mengajukan SPPD ini."; tanpa workflow | `PASS` (AT-37) | idem |
| — Tono tanpa atasan | `400` pesan engine; SPPD tetap Draf, tanpa id workflow, tanpa instance aktif | `PASS` (`VR-TRV-29`) | idem |
| — Instance dibuat tetapi submit gagal (interceptor menolak simpan Draft → status lain) | `409`; SPPD Draf tanpa id workflow; draf workflow terhapus lunak; Ajukan berikutnya berhasil | `PASS` (`VR-TRV-29`) | idem |
| — Definisi dinonaktifkan admin | `409` memuat kode definisi; SPPD tetap Draf | `PASS` (`VR-TRV-29`) | idem |
| — SPPD lama `Submitted` tanpa workflow | Detail `CanSubmit = true`; Ajukan berhasil | `PASS` (AT-34 bagian ajukan) | idem |
| — Dikembalikan (aksi `RequestRevisionAsync` nyata oleh Bima), status disetel `NeedRevision`, ubah, Ajukan ulang | Instance sama, kembali menunggu, tetap satu instance | `PASS` (State 32.2) | idem |
| — SPPD tidak ada / atribut endpoint | `KeyNotFoundException` (404); route `{id}/submit`, hak `BusinessTravelRequest : Create` sama dengan `POST /`, `[Authorize]`, kode 200/400/403/404/409 | `PASS` | idem |
| Kontrol negatif (10 mutasi; source dipulihkan dan dibandingkan byte demi byte) | M1 status buat `Submitted`, M2 anggaran = estimasi, M3 kunci workflow dihapus, M4 cek pemohon, M5 cek ditolak, M6 pemulihan id workflow, M7 pakai ulang instance, M8 peta 404→409, M9 ubah menyalin anggaran, M10 hapus draf workflow — **semuanya tertangkap** | `PASS` | Keluaran per putaran |
| Catatan proses | Putaran pertama M6 lolos karena kasus "tanpa atasan" gagal di `CreateAsync`, bukan `SubmitAsync`; uji kegagalan submit ditambahkan dan menangkap M6/M10. Pemulihan berkas dengan `copyFileSync` mempertahankan cap waktu lama sehingga satu putaran memakai biner basi; skrip diganti menulis ulang berkas dan diuji ulang | Catatan | — |
| Proyek uji HR penuh | 347/347 lulus (327 + 20 baru) | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2563 identitas kanonik dari 2563 kemampuan terdaftar." (action baru berbagi identitas `Create`); seeder workflow 0/0 | `PASS` | `be375.log` |
| Kueri baca-saja devYoga | `MstTravelType` 0 baris, `TrxBusinessTravelRequest` 0 baris; tidak ada akun dengan atasan langsung aktif; tidak ada jabatan berhak `BusinessTravelRequest` | Temuan | `q375.mjs`, `q375b.mjs` |
| Penyiapan devYoga (izin pengguna "Boleh, buat data uji SPPD di devYoga"; "Sisip 1 baris uji") | INSERT tunggal `MstTravelType` `UJI-375-DN` "UJI-375 Dinas Dalam Negeri"; atasan langsung Dewi → Cahyo lewat API `manager-assignments` (`200`, disimpan); hak `BusinessTravelRequest` Read/Create/Update/Cancel sementara pada jabatan Dewi (Keperawatan / Perawat Rawat Jalan) | Catatan | `at-375.mjs` |
| — Putaran pertama | Login Dewi `403` "Akurasi lokasi tidak terbaca" — ruas skrip `accuracy` salah (seharusnya `accuracyMeters`); hak sementara dikembalikan identik; tidak ada SPPD terbuat | Catatan (galat skrip) | `at-375.out` |
| — Buat SPPD (AT-23) | `201` "SPPD disimpan sebagai Draf. Tekan Ajukan untuk mengirim ke atasan."; `SPPD/RSQ/2026/11/0001` `Draft`, estimasi 5.000.000, anggaran disetujui 0, tanpa workflow, `canSubmit = true` | `PASS` | idem |
| — Ubah Draf | `200`; tetap `Draft`, anggaran 0 | `PASS` | idem |
| — Ajukan oleh superadmin (AT-37) | `403` "Hanya pemohon SPPD yang dapat mengajukan SPPD ini." | `PASS` | idem |
| — Ajukan oleh Dewi (AT-24) | `200`; `Submitted`, tahap 1, `canSubmit = false`; instance `HRD_TRV_REQUEST` definisi `BUSINESS_TRAVEL_REQUEST` `InProgress`; satu tugas `Available` untuk `cahyoajah@rsmmc.com`; basis data: `SubmittedByUserId` = Dewi, `WorkflowDefinitionId` terisi, anggaran 0 | `PASS` | idem |
| — Ubah saat menunggu (AT-25) / Ajukan dua kali (VR-TRV-26) | `409` dengan pesan kontrak masing-masing; tetap satu instance aktif | `PASS` | idem |
| — Ajukan SPPD tidak ada / detail | `404`; detail `200` dengan ruas 34.4 | `PASS` | idem |
| — Pemulihan hak | `identik: true` (11 izin); sesudahnya Dewi `403` pada daftar dan Ajukan | `PASS` | idem |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. SPPD baru `Draft`, `workflowInstanceId = null`, `canSubmit = true`, anggaran 0 | Terpenuhi (harness) | AT-23 |
| 2. Ajukan → `Submitted`, instance menunggu dengan tugas untuk atasan langsung | Terpenuhi (harness) | AT-24 |
| 3. Ubah SPPD berworkflow yang menunggu → `409`, data tidak berubah | Terpenuhi (harness) | AT-25 |
| 4. Status tidak sah → `409` `VR-TRV-26`; `Rejected` → `409` `VR-TRV-28`; tanpa atasan/definisi nonaktif → pesan engine, status tidak berubah | Terpenuhi (harness) | Enam uji + M5/M6/M8/M10 |
| 5. Tanpa hak Create atau bukan pemohon → `403` | Terpenuhi: bukan pemohon di harness; tanpa hak = atribut `[AccessPermission]` yang sama dengan `POST /` (ditegakkan filter bersama) | AT-37 |
| 6. SPPD lama `Submitted` tanpa workflow → `canSubmit` dan Ajukan lolos | Terpenuhi (harness) | AT-34 bagian ajukan |
| Migration | Tidak ada | — |
| Bukti devYoga | Terpenuhi — AT-23, 24, 25, 37 dan `VR-TRV-26` lewat API dengan akun asli | `at-375.out` |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan — master jenis perjalanan | `MstTravelType` tanpa API/seeder dan kosong di devYoga; karena `TravelTypeId` ber-FK, `POST /hrd/travel/requests` selalu gagal pada basis data kosong (perilaku sejak kontrak `v25`). Diusulkan task baru |
| Temuan — atasan langsung | Tidak ada `WfpManagerAssignment` aktif di devYoga; Ajukan dari akun mana pun ditolak `VR-TRV-29` sampai Admin HR mengisinya |
| Temuan — hak SPPD | Belum ada jabatan dengan hak `BusinessTravelRequest`; perlu diatur di Akses Role |
| Temuan — penyelarasan sesudah commit | Workflow V2 memanggil penangan referensi sesudah commit dan menelan galat; integration-contract 25.1 mengandaikan satu transaksi. **Keputusan pengguna 19 November 2026: pilihan (a)** — kontrak disesuaikan dengan perilaku engine (penyegaran `v28` dicatat untuk pass blueprint), `BE-HRD-376` menyediakan penyelarasan ulang |
| Utang | Penomoran SPPD `Count + 1` |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Data uji yang tertinggal di devYoga | `MstTravelType` `UJI-375-DN`; atasan langsung Dewi → Cahyo (`acbf6fe1-…`); SPPD `SPPD/RSQ/2026/11/0001` `Submitted` menunggu Cahyo (dipakai uji `BE-HRD-376`) — disimpan sesuai pilihan pengguna |
| Task berikutnya | Bukti devYoga `BE-HRD-376` (butuh sandi Cahyo); `BE-HRD-377`, `BE-HRD-378` |
