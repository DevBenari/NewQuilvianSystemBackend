# Laporan Perubahan Backend — `BE-RHB-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-012` |
| Judul | Sesi terapi (tanpa handoff billing) |
| Slice | S-D — Sesi dan handoff billing |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-012` |
| Trace | FR-RHB-030..032, 036, 037; FEAT-016..018; RHB-INV-04, 06; RHB-DEC-009, 013; `state-transition-matrix.md` §5; V-SES-01..10 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Session* |
| Dependency | `BE-RHB-011` ✅ (2026-11-06) |
| Klasifikasi | `STANDARD` — endpoint transaksi baru, tanpa schema baru, memakai mesin keutuhan RM dan deret nomor `RHB_SESSION` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). Build owner `PASS` (0 error; 426 warning bawaan, tidak ada di folder Rehab; 5m10s). QBE Strict `PASS`. Uji API manual: run utama 43/44, dan satu FAIL berasal dari desain uji, lalu diulang 5/5 (§5). AC2 terbukti lewat kode karena DB dev tidak punya rawat inap yang pasiennya sudah pulang fisik. |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru; `RhbSession`, `RhbSessionIntervention` dari `BE-RHB-002`. |
| Jenis capability | **Transaksi — aggregate ber-lifecycle + worklist.** Dibuat lewat `POST /by-program/{programId}/start` atau `/not-done`; perpindahan status `complete`, `stop-early`, `sign`, `cancel`. Worklist memakai `filters/metadata`, `summary`, dan `GET /`. Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001, QBE-API-001, standar hak akses (`RehabilitationPermissions.Session`), INV-PLT-002 (nomor dari `NumberSeriesAllocator`, hangus bila gagal tersimpan) |
| Lintas modul | Tulis lewat gateway: `MrcClinicalDocumentIntegrity` jenis 49 (persetujuan RM `RHB-OQ-012` ✅). Baca saja: `RegPatientEncounter`, `InpEpisode`, `MstPatient`, `MstProcedure`. **Billing tidak disentuh** (`BE-RHB-013`, menunggu `RHB-OQ-015`). |

## 1. Masalah yang diperbaiki

Program sudah aktif, tetapi terapis belum bisa mencatat sesi yang benar-benar dijalankan. Setiap sesi
harus terikat ke kunjungan yang masih terbuka dan melewati cek keselamatan. Tindakan yang dicatat
harus tindakan aktual, deviasi harus tercatat, dan sesi yang tidak terlaksana tidak boleh pernah
tertagih.

**Contoh.** Terapis membuka program v3, memilih kunjungan hari ini, dan mencentang verifikasi
identitas dan program. Ia mencatat "TD 120/80, tidak pusing" serta kontraindikasi "nyeri lutut
ringan, beban dikurangi", lalu memulai sesi `RHB-SES-2026-000001`. Ia mencatat dua tindakan aktual
beserta respons pasien, menekan selesai, lalu menandatangani.

Di sesi berikutnya pasien pusing. Henti dini ditolak sampai tindakan segera dicatat. Setelah dicatat,
sesi berstatus *Dihentikan dini* dan program otomatis ditandai *perlu telaah*. Pasien yang tidak datang
dicatat *Tidak terlaksana*, tanpa tindakan dan tanpa tagihan.

## 2. Proses bisnis

1. **Mulai** (`Create`, profesi = layanan program):
   - program harus *Aktif* (409 `RHB-VAL-050`);
   - kunjungan harus milik pasien dan masih terbuka (422 `RHB-VAL-051`), dan pasien rawat inap belum pulang fisik (422 `RHB-VAL-052`);
   - identitas dan program sudah diverifikasi, dan kondisi terkini terisi (422 `RHB-VAL-053`);
   - kontraindikasi yang ditemukan wajib dijelaskan (422 `RHB-VAL-054`);
   - nomor `RHB-SES-yyyy-nnnnnn` terbit, dan dokumen didaftarkan ke Rekam Medis jenis 49.
2. **Tidak terlaksana** (`Create`). Alasan wajib (422 `RHB-VAL-058`); tanpa intervensi; tidak pernah tertagih (RHB-INV-06).
3. **Isi sesi** (`Update`, hanya pelaksana):
   - tindakan harus dari daftar layanan (`RHB-VAL-042`);
   - intervensi rencana harus milik program sesi;
   - intervensi di luar rencana wajib deviasi *Dimodifikasi* (422 `RHB-VAL-057`);
   - deviasi wajib alasan; eskalasi wajib tindakan segera dan pihak yang dihubungi (422 `RHB-VAL-056`);
   - deviasi bertanda "memengaruhi sesi berikutnya" membuat program *perlu telaah*.
4. **Selesai** (`Update`). Minimal satu intervensi aktual (422 `RHB-VAL-055`).
5. **Henti dini / eskalasi** (`Update`). Alasan dan tindakan segera wajib (422 `RHB-VAL-056`); program ditandai *perlu telaah* (bawaan).
6. **Tanda tangan** (`Sign`, hanya pelaksana):
   - sesi yang masih berlangsung ditolak (409 `RHB-VAL-020`);
   - ada intervensi tanpa respons ditolak (422 `RHB-VAL-059`);
   - status *Final* dan kunci Rekam Medis tersimpan dalam satu transaksi;
   - `BillingHandoffStatus` tetap `NotRequired` sampai `BE-RHB-013`.
7. **Batal** (`Update`). Hanya sesi yang berlangsung atau tidak terlaksana, dengan alasan wajib. Rekam Medis menandai *Dibatalkan*.
8. **Final**: ubah dan batal dijawab 409 `RHB-VAL-021`; koreksi lewat addendum Rekam Medis oleh penulis.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Controllers/RehabSessionController.cs` | Baru | 12 endpoint, Resource `RehabSession`, aksi `Read`/`Create`/`Update`/`Sign` |
| `Services/RhbSessionService.cs` | Baru | Aturan §2, worklist (saringan periode, status, status rekam, layanan, pelaksana, `mine`, billing, cari), ringkasan, rincian dengan intervensi rencana |
| `DTOs/RehabSessionDtos.cs` | Baru | Query, request (`Start`, `RecordNotDone`, `Update`, `StopEarly`, `RehabVersionRequest`), list/detail, metadata, ringkasan |
| `DTOs/RehabProgramDtos.cs` | Diperbarui | `RehabProgramInterventionItem.ProgramInterventionId`, supaya layar sesi bisa merujuk intervensi rencana (penambahan, tidak memutus klien) |
| `Services/RhbProgramService.cs` | Diperbarui | Mengisi `ProgramInterventionId` di rincian program |
| `Program.cs` | Diperbarui | `AddScoped<RhbSessionService>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada |
| Hak akses | Controller baru `RehabSession` dengan Read, Create, Update, dan Sign (tipe `Update`). Tidak ada nama peran di kode. Profesi dan pelaksana adalah kewenangan yang melekat pada data. |
| Logger | id, nomor sesi, status, status rekam, status billing, versi. Kondisi, kontraindikasi, respons, dan deviasi tidak dicatat. |
| Billing | Tidak ada `TrxPatientProcedure` yang dibentuk. Ini disengaja (AC7) dan tercatat sebagai risiko go-live (§8). |

### 3.3 Delta terhadap kontrak dan desain

| Delta | Rincian |
| --- | --- |
| `PUT` juga untuk sesi *Selesai*/*Dihentikan dini* yang belum ditandatangani | Matriks §5 hanya menyebut `update` saat `InProgress`. Tetapi V-SES-10 menuntut respons saat tanda tangan, dan batal hanya untuk `InProgress`/`NotDone`. Tanpa delta ini, terapis yang lupa mengisi respons sebelum "selesai" tidak bisa menandatangani maupun membatalkan sesinya. Yang boleh diubah di status itu hanya intervensi, respons, dan hasil segera; jenis deviasi tetap; sesi *Selesai* tetap wajib ≥1 intervensi. |
| `not-done` tanpa `encounterId` | Model mewajibkan `EncounterId` dan Rekam Medis membutuhkan kunjungan, maka dipakai kunjungan penyusunan program. Kunjungan yang dikirim cukup milik pasien (boleh sudah ditutup). |
| Catatan `not-done` | Disimpan di `ImmediateOutcomeText`, karena model tidak punya kolom catatan tidak terlaksana |
| `stop-early.needsProgramReview` | Bawaan `true` (UAT-12: henti dini menjadikan program "perlu telaah") |
| `PUT` menolak `DeviationType = StoppedEarly` | Henti dini hanya lewat `POST /{id}/stop-early` agar `EndedAt` dan status ikut berubah |
| Urutan cek kunjungan | Kepemilikan diperiksa sebelum status pulang fisik, supaya kunjungan pasien lain tidak membocorkan informasi pulang (selalu 051) |
| `RehabProgramInterventionItem.programInterventionId` | Field baru di rincian program dan `plannedInterventions` sesi |
| Ringkasan | `total` tidak menghitung sesi dibatalkan; `awaitingSignature` = selesai, henti dini, atau tidak terlaksana yang masih draf |

## 4. Validasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| Build owner (2026-11-09): `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `PASS`, exit 0, **0 error**, 426 warning bawaan (tidak ada di folder `RehabilitationManagement`), 5m10s. Build pertama terputus karena sesi berakhir, lalu diulang dengan perintah owner apa adanya. |
| QBE Strict | `PASS`. Controller tanpa `ApplicationDbContext`; pasangan `[AccessAction]`/`[AccessPermission]` identik untuk keempat aksi; grep `IsInRole`/`UserType`/nama peran = 0. |
| Review diff | 3 berkas baru, 3 berkas lama diubah (§3.1) |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API manual

**Lingkungan.**
- Backend Debug di `https://localhost:7184`, DB dev `QuilvianNewDevSukma`, 2026-11-09.
- Program v3 layanan uji A (aktif sejak `BE-RHB-011`), episode `RHB-EPS-2026-000001`.
- Kunjungan terbuka pasien A `fe5d09ba…`, kunjungan selesai `4a48f1aa…` (status *Completed*), dan kunjungan pasien B.
- Akun uji berprofesi Dokter Umum dengan butir `RehabSession` (+ `RehabProgram : Read`).

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Controller akses `RehabSession` terdaftar; akun uji login; metadata memuat 4 status, 6 alasan tidak terlaksana, 4 status billing; rencana program memuat `programInterventionId` | PASS ×5 |
| 2 | V-SES-01: program v1 (*Superseded*) → 409 `RHB-VAL-050` | PASS |
| 3 | SuperAdmin tanpa profesi → 403 `RHB-VAL-031` | PASS |
| 4 | **AC1 / UAT-09**: kunjungan sudah ditutup → 422 `RHB-VAL-051` "Pilih kunjungan pasien hari ini yang masih terbuka."; tanpa kunjungan dan kunjungan pasien lain → 422 `051` | PASS ×3 |
| 5 | **AC3**: identitas belum diverifikasi → 422 `RHB-VAL-053`; program belum diverifikasi + kondisi kosong → 422 `053` (2 field) | PASS ×2 |
| 6 | V-SES-05: kontraindikasi tanpa keterangan → 422 `RHB-VAL-054` | PASS |
| 7 | Mulai → 201 `RHB-SES-2026-000001` *Berlangsung*, keutuhan *Draf*, billing *Tidak ditagihkan*, aksi `update/complete/stop-early/cancel`, 2 intervensi rencana | PASS |
| 8 | **AC5**: tanda tangan saat berlangsung → 409 `RHB-VAL-020` | PASS |
| 9 | V-SES-06: selesai tanpa intervensi → 422 `055`; V-SES-08: di luar rencana tanpa *Dimodifikasi* → 422 `057`; tindakan tidak terdaftar → 422 `042`; deviasi tanpa alasan → 422 `056` | PASS ×4 |
| 10 | Isi 2 intervensi rencana + respons → 200 v1; bukan pelaksana → 403 `RHB-VAL-022`; versi basi → 409 `RHB-VAL-010` | PASS ×3 |
| 11 | Selesai → 200, waktu selesai terisi, aksi `update/sign` | PASS |
| 12 | **AC7**: tanda tangan → 200 *Final*, keutuhan *Ditandatangani*, billing tetap *Tidak ditagihkan*, intervensi tanpa tindakan bertarif | PASS |
| 13 | Ubah dan batal sesi final → 409 `RHB-VAL-021` | PASS |
| 14 | Addendum Rekam Medis jenis 49 | FAIL di run utama (SuperAdmin bukan penulis → 403 dari aturan RM). Diulang dengan akun penulis + butir `ClinicalNoteAddendum : Create`: bukan penulis 403, penulis **201**, catatan asli tetap → PASS |
| 15 | **AC4 / UAT-12**: henti dini tanpa tindakan segera → 422 `056`; dengan tindakan segera → *Dihentikan dini*, program **perlu telaah** | PASS ×2 |
| 16 | V-SES-10: intervensi tanpa respons → 422 `059`; delta: respons dilengkapi sesudah henti dini (deviasi tetap) → 200; tanda tangan → 200 *Final* | PASS ×3 |
| 17 | V-SES-09: tidak terlaksana tanpa alasan → 422 `058` | PASS |
| 18 | **AC6**: tidak terlaksana tanpa kunjungan → 201, 0 intervensi, kunjungan = kunjungan penyusunan program, aksi `sign/cancel`; diisi intervensi → 409; tanda tangan → 200 *Final*, tidak ditagihkan | PASS ×3 |
| 19 | Eskalasi tanpa pihak yang dihubungi → 422 `056`; batal tanpa alasan → 422 `008`; batal sesi berlangsung → 200 *Dibatalkan* + keutuhan *Dibatalkan* | PASS ×3 |
| 20 | Worklist `mine` 4 sesi; saringan status *Tidak terlaksana* → 1; cari nomor sesi → 1; ringkasan selesai 1 / henti dini 1 / tidak terlaksana 1 / berlangsung 0 / total 3 (batal tidak dihitung); per episode + program → 4 | PASS ×5 |
| 21 | Ruang kerja episode: `sessionFinal` = 3, timeline memuat sesi | PASS |

Rekap: run utama **43/44**, run ulang addendum **5/5**. Satu-satunya FAIL disebabkan uji memakai
aktor yang salah (bukan penulis). Perilaku itu benar menurut aturan Rekam Medis, dan tidak ada perubahan
source di antara kedua run.

**AC2 (pasien RWI sudah pulang → 422 `RHB-VAL-052`) terbukti lewat kode, tidak di runtime.** Seluruh DB
dev tidak memuat satu pun `InpEpisode` dengan `PhysicallyLeftAt` terisi, dan pasien episode rehab tidak
punya kunjungan rawat inap. Cabangnya ada di `StartAsync`: kunjungan milik pasien, masih terbuka, dan
`PhysicallyDischarged` dari `RhbServiceSupport.CheckEncounterAsync` (pemeriksaan yang sama dengan
`BE-RHB-006`, juga terbukti statis di sana).

## 6. Bukti database (kueri READ ONLY)

| Data | Hasil |
| --- | --- |
| `RhbSession` + keutuhan jenis 49 | `…000001` *Selesai*/Final; `…000002` *Dihentikan dini*/Final (deviasi 2); `…000003` *Tidak terlaksana* (alasan 1)/Final; `…000004` dibatalkan, keutuhan *Cancelled*; `…000005` *Tidak terlaksana* (alasan 3)/Final. Yang final semuanya keutuhan *Signed* oleh pelaksana. |
| Billing | `BillingHandoffStatus = 0` di kelima sesi; 0 intervensi dengan `PatientProcedureId`; 0 `TrxPatientProcedure` baru pada kunjungan uji |
| `RhbSessionIntervention` | 3 aktif, 1 terhapus lunak (diganti saat melengkapi sesi henti dini) |
| Program v3 | `NeedsReview = true` |
| `RhbStatusHistory` (Session) | `start → complete → sign`, `start → stop-early → sign`, `not-done → sign` ×2, `start → cancel` |
| `MrcClinicalNoteAddendum` jenis 49 | 1 |

**Pembersihan.**
- 0 dari 24 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pasangan uji: 0 baris;
- layanan uji A nonaktif;
- backend dihentikan.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-09 | Terpenuhi | §5 #4 |
| (2) Pasien RWI sudah pulang → 422 `RHB-VAL-052` | Terpenuhi lewat kode | §5 catatan AC2 (data dev tidak tersedia) |
| (3) Tanpa verifikasi identitas → 422 | Terpenuhi | §5 #5 |
| (4) UAT-12 | Terpenuhi | §5 #15, §6 |
| (5) Tanda tangan saat `InProgress` → 409 | Terpenuhi | §5 #8 |
| (6) `NotDone` tersimpan tanpa intervensi | Terpenuhi | §5 #18, §6 |
| (7) Tanda tangan tidak membentuk tagihan sampai `BE-RHB-013`, dan tertulis di laporan | Terpenuhi | §5 #12, §6, berkas ini |
| DoD: build owner, laporan | Terpenuhi | §4, berkas ini |

## 8. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| **Sesi final belum tertagih** | Sampai `BE-RHB-013` selesai, sesi yang ditandatangani tidak membentuk tindakan bertarif. **Jangan go-live** sebelum `BE-RHB-013`; sesi lama perlu backfill lewat `retry` handoff di task itu. `BE-RHB-013` masih menunggu keputusan `RHB-OQ-015`. |
| AC2 di runtime | Uji ulang saat DB punya pasien rawat inap yang sudah pulang fisik dan punya program rehab aktif |
| `NeedsReview` program v3 | Tetap `true` setelah uji; reset lewat reasesmen `Modify`/revisi (`BE-RHB-014`) |
| Task berikutnya | `BE-RHB-014` (reasesmen) 🟡. `BE-RHB-015` menunggu `BE-RHB-014`. `FE-RHB-008` menunggu `FE-RHB-007`. |
