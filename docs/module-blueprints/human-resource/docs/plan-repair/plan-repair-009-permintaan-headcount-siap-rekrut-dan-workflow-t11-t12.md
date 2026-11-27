# PLAN-REPAIR-009 — Perbaikan Alur Permintaan Headcount Siap Direkrut dan Pilihan Permintaan Rekrutmen

| Field | Nilai |
| --- | --- |
| Tanggal | 25 November 2026 |
| Modul | Human Resource — Perencanaan Strategis SDM (`WorkforcePlanning`), Persetujuan (`WorkflowManagement`), & Rekrutmen (`RecruitmentManagement`) |
| Ditemukan saat | Analisis laporan pengguna (*select field Permintaan Headcount menampilkan "Data tidak ditemukan" pada modal Buat Permintaan Rekrutmen*) |
| Planning asal | — (mengikuti pola integrasi workflow [PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md](../PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md)) |
| Repository terdampak | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Status | Selesai, lulus uji ulang 25 November 2026 |
| Alur | Analisis → Plan & Repair → Implementasi → Testing ulang |

---

## 1. Register

| No | Masalah (yang terlihat) | Prioritas | Perbaikan | Status |
| --- | --- | :---: | --- | :---: |
| M1 | Pengajuan Permintaan Headcount (`T11`/`T12`) bentrok transaksi ganda di `WorkflowService` (`500`) dan gagal saat pengajuan ulang status `Returned` (`409`) | P0 | Pisahkan `StartWorkflowAsync` / `ResubmitWorkflowAsync` dari transaksi EF luar dengan pola kompensasi gagal-tertutup (`advisory lock` khusus PostgreSQL) | ✅ |
| M2 | Persetujuan akhir `T11`/`T12` di Kotak Masuk Persetujuan tidak mengubah status Permintaan Headcount menjadi `ReadyForRecruitment` (`Siap Direkrut`) | P0 | Kenali status akhir `"Completed"` (serta `"RevisionRequested"`, `"Withdrawn"`, `"Draft"`, `"InProgress"`) dan perbaiki query sisa kursi aktif `VR-WFP-23` di `HrdHeadcountRequestDecisionApplier` | ✅ |
| M3 | Penarikan Permintaan Headcount meninggalkan tugas yatim di Kotak Masuk, dan dialog persetujuan tidak menampilkan ringkasan dokumen (`RequestContext`) | P1 | Tambahkan `ResubmitWorkflowAsync` & `WithdrawWorkflowAsync` pada `IHrdWorkforcePlanningWorkflowIntegrationService`, tarik instance saat `WithdrawAsync`, serta kirim `RequestContext.summary` & `source` saat `SubmitAsync` | ✅ |
| M4 | Dropdown **Permintaan Headcount** pada **Buat Permintaan Rekrutmen** tidak mendukung cari jabatan, melewatkan `PartiallyFulfilled` / kuota eksternal `VR-REC-02`, dan modal sempit | P1 | Tambahkan pencarian `PositionName`/`UnitName` + filter opsional `EligibleForRequisitionOnly` + `RemainingExternalSeats` di backend; perbarui `HEADCOUNT_READY_SELECT_RESOURCE`, `HEADCOUNT_FILTERS`, `modalClassName`, dan `text-start` di `requisitions-list-view.jsx` | ✅ |
| M5 | Kotak Masuk Persetujuan menampilkan kode mentah `WorkforceHeadcountRequest` tanpa tautan buka dokumen | P2 | Daftarkan peta referensi `WORKFORCE_PLANNING_WORKFLOW_REFERENCES` pada `workforce-planning-constants.js` dan hubungkan ke `approval-inbox-view.jsx` | ✅ |

**Ringkasan: 5 dari 5 perbaikan selesai.**

---

## 2. Rincian masalah

### M1 — Transaksi ganda dan gagal pengajuan ulang pada `SubmitAsync`

- **Gejala:** Ketika definisi workflow `T11`/`T12` memiliki langkah persetujuan aktif, menekan tombol **Ajukan** pada detail Permintaan Headcount memicu HTTP `500` (*"The connection is already in a transaction"*), dan pengajuan ulang dokumen berstatus `Returned` memicu `409 Conflict` (*"Transaksi sumber masih memiliki workflow aktif"*).
- **Bukti:**
  - `HrdHeadcountRequestService.cs:472` membuka `BeginTransactionAsync`, lalu pada baris `:543` memanggil `_workflowService.StartWorkflowAsync(...)` yang memanggil `WorkflowService.CreateCoreAsync` (`WorkflowService.cs:532`) yang membuka `BeginTransactionAsync(IsolationLevel.Serializable)` kedua pada koneksi yang sama.
  - Pengajuan ulang (`Entity.RequestStatus == Returned`) selalu memanggil `CreateAsync` alih-alih `WorkflowService.SubmitAsync` pada instance yang sudah ada.
- **Penyebab:** Pola lama membungkus panggilan mesin workflow terpadu di dalam transaksi lokal (`HrdHeadcountRequestService.cs:472-576`); sudah ada di `HEAD`.
- **Contoh di rumah sakit:** Kepala Unit / staf SDM mengajukan permintaan penambahan 2 perawat ICU, namun server mengembalikan galat `500` walau sisa kursi formasi tersedia.
- **Perbaikan:** Jalankan validasi, penguncian kursi (`pg_advisory_xact_lock` khusus `IsNpgsql()`), perekaman potret gap, dan penandaan `Submitted` pada transaksi lokal terlebih dahulu, lalu panggil `StartWorkflowAsync` atau `ResubmitWorkflowAsync` di luar transaksi. Bila workflow gagal dimulai, kembalikan status dokumen (`Draft`/`Returned`) dan hapus-lunak potret gap yang baru dibuat (gagal tertutup).
- **Berkas:**
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdWorkforcePlanningWorkflowIntegrationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningWorkflowIntegrationService.cs`

### M2 — Status `"Completed"` dari mesin workflow tidak ditangani oleh applier Headcount

- **Gejala:** Setelah seluruh penyetuju menyetujui pengajuan di Kotak Masuk Persetujuan, instance workflow berstatus `Completed`, tetapi Permintaan Headcount tetap berstatus `Submitted` sehingga tidak pernah muncul di dropdown **Permintaan Headcount** pada menu **Permintaan Requisition**.
- **Bukti:** `WorkflowService.cs:2778` menyetel `instance.WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Completed` (`"Completed"`), sementara `HrdHeadcountRequestDecisionApplier.cs:58-59` hanya mengecek `"Approved"` dan `"FinalApproved"`, dan baris `:209` belum mengenali `"RevisionRequested"`. Selain itu, baris `:147-155` (`VR-WFP-23`) menghitung `.CountAsync()` tanpa menyaring `RequestStatus` aktif dan tanpa menjumlahkan `SeatFte`.
- **Penyebab:** Ketidakselarasan kontrak status antara mesin `WorkflowService` dan `HrdHeadcountRequestDecisionApplier.cs:58-59,147-155,209-230`; sudah ada di `HEAD`.
- **Contoh di rumah sakit:** Kepala Bagian SDM sudah menekan **Setujui** untuk permintaan perawat baru, namun tim rekrutmen tetap melihat dropdown **Permintaan Headcount** kosong saat hendak membuka lowongan.
- **Perbaikan:** Terima `"Completed"` bersama `"Approved"`/`"FinalApproved"`; terima `"RevisionRequested"` bersama `"Returned"`/`"ReturnedForRevision"`; tangani `"Withdrawn"`/`"Cancelled"` serta status non-terminal `"Draft"`/`"InProgress"`/`"Submitted"` secara aman; dan samakan rumus `claimedSeatsFte` (`VR-WFP-23`) dengan `GetRemainingSeatsAsync`.
- **Berkas:**
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestDecisionApplier.cs`

### M3 — Penarikan tugas yatim & ringkasan dokumen Kotak Masuk

- **Gejala:** Menarik kembali Permintaan Headcount (`WithdrawAsync`) tidak menarik instance workflow di Kotak Masuk; penyetuju di Kotak Masuk Persetujuan juga tidak melihat rincian unit, jabatan, jenis permintaan, dan jumlah kursi sebelum menyetujui.
- **Bukti:** `HrdHeadcountRequestService.cs:543-551` mengirim `RequestContext = null`; `HrdHeadcountRequestService.cs:579-612` tidak memanggil penarikan workflow.
- **Penyebab:** Integrasi `RequestContext` dan `WithdrawOnBehalfAsync` belum diterapkan pada `HrdHeadcountRequestService`; sudah ada di `HEAD`.
- **Perbaikan:** Tambahkan metode `WithdrawWorkflowAsync` pada `IHrdWorkforcePlanningWorkflowIntegrationService` yang memanggil `WorkflowService.WithdrawOnBehalfAsync`; panggil saat `WithdrawAsync`; isi `RequestContext` (`summary` tanpa nominal biaya/gaji + `source.headcountRequestId`) saat `SubmitAsync`.
- **Berkas:**
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdWorkforcePlanningWorkflowIntegrationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningWorkflowIntegrationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestService.cs`

### M4 — Pencarian, penyaringan kuota eksternal, dan lebar modal pada **Buat Permintaan Rekrutmen**

- **Gejala:** (a) Mengetik nama jabatan pada dropdown `"Cari nomor atau jabatan..."` tidak menemukan item karena backend hanya mencari nomor dan justifikasi; (b) `HEADCOUNT_FILTERS` hanya menyaring `ReadyForRecruitment` (melewatkan `PartiallyFulfilled` yang diizinkan `VR-REC-01`), tidak mengecualikan headcount yang seluruh kursinya mutasi internal atau sudah habis terpakai requisition (`VR-REC-02`), serta mengisi `requestedSeats` dengan `totalSeats` alih-alih sisa kursi eksternal; (c) modal memakai `size="lg"` (lebar tetap 430 px).
- **Bukti:** `HrdHeadcountRequestService.cs:97-102`, `HeadcountRequestDtos.cs:105-112,156-166`, `requisitions-list-view.jsx:38,183,204-207`, `recruitment-constants.js:202-209`.
- **Penyebab:** Filter query backend belum menyediakan mode khusus kandidat requisition (`EligibleForRequisitionOnly`) dan pencarian nama jabatan/unit; modal frontend masih memakai `size="lg"` dari revisi sebelumnya; sudah ada di `HEAD`.
- **Perbaikan:**
  - Backend: perluas `Search` agar mencocokkan `PositionName` dan `UnitName`; tambahkan field opsional `ExternalSeatCount`, `InternalSeatCount`, `RemainingExternalSeats` pada `HeadcountRequestListItemResponse` dan parameter opsional `EligibleForRequisitionOnly` pada `HeadcountRequestPagedQuery` (mundur kompatibel, `null` = perilaku lama).
  - Frontend: tambahkan `"eligibleForRequisitionOnly"` di `HEADCOUNT_READY_SELECT_RESOURCE.filterKeys`, ubah `HEADCOUNT_FILTERS` ke `{ eligibleForRequisitionOnly: true }`, pakai `remainingExternalSeats ?? externalSeatCount ?? totalSeats ?? 1`, perbaiki `modalClassName="region-confirm-modal-wide"` dan `text-start`, serta perjelas `emptyText`.
- **Berkas:**
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/HeadcountRequestDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestService.cs`
  - `QuilvianSystemFrontendDev/src/lib/constants/hr/recruitment/recruitment-constants.js`
  - `QuilvianSystemFrontendDev/src/components/view/hr/recruitment/requisitions-list-view.jsx`

### M5 — Label & tautan `WorkforceHeadcountRequest` di Kotak Masuk Persetujuan

- **Gejala:** Baris pengajuan Headcount di `/hr/workflow/approval-inbox` menampilkan teks mentah `"WorkforceHeadcountRequest"` dan tidak menampilkan tautan *Buka dokumen ↗*.
- **Bukti:** `approval-inbox-view.jsx:50-72,208-210` baru memuat peta `LIFECYCLE_WORKFLOW_REFERENCES` (`T15`–`T18`) dan `TRAVEL_WORKFLOW_REFERENCE_TYPE`.
- **Penyebab:** Peta referensi `WorkforcePlanning` belum ditambahkan; sudah ada di `HEAD`.
- **Perbaikan:** Tambahkan `WORKFORCE_PLANNING_WORKFLOW_REFERENCES` di `workforce-planning-constants.js` dan baca di `approval-inbox-view.jsx` tanpa mengubah pola pengenal `T15`–`T18` dan SPPD yang diuji unit test.
- **Berkas:**
  - `QuilvianSystemFrontendDev/src/lib/constants/hr/workforce-planning/workforce-planning-constants.js`
  - `QuilvianSystemFrontendDev/src/components/view/hr/workflow/approval-inbox-view.jsx`

---

## 3. Sebelum → sesudah

```text
Sebelum (modal Buat Permintaan Rekrutmen)       Sesudah
┌─ Buat Permintaan Rekrutmen (430 px) ──┐       ┌─ Buat Permintaan Rekrutmen (550 px, rata kiri) ─┐
│ Permintaan Headcount *                │       │ Permintaan Headcount *                           │
│ [Pilih permintaan yang siap direkrut▾]│       │ [Pilih permintaan yang siap direkrut            ▾]│
│  └─ [Cari nomor atau jabatan...]      │       │  └─ [Cari nomor atau jabatan...                 ]│
│     "Data tidak ditemukan"            │       │     REQ-000001 · Perawat ICU (Tersedia 2 kursi)  │
│  (ajukan headcount T11/T12 → 500/macet│       │  → RS / Unit / Jabatan / Sisa kursi luar terisi  │
│   karena transaksi ganda & Completed) │       │  (alur T11/T12 dari Pengajuan → Setujui → Siap)  │
└───────────────────────────────────────┘       └──────────────────────────────────────────────────┘
```

---

## 4. Uji ulang

| No | Pemeriksaan | Diharapkan | Hasil nyata |
| ---: | --- | --- | --- |
| 1 | Uji integrasi backend `HeadcountRequestWorkflowTests` (SQLite, mesin `WorkflowService` nyata): buat → ajukan `T11`/`T12` → setujui (`Completed` → `ReadyForRecruitment`) → tampil di filter `EligibleForRequisitionOnly` + cari jabatan → buat `JobRequisition`; juga minta revisi → ubah → ajukan ulang instance sama; tarik membatalkan instance; penolakan `ExternalSeatCount = 0` & kuota habis | Semua lulus | **Lulus 4/4** (`HeadcountRequestWorkflowTests` lewat `HrScratch.csproj`), total `15/15` lulus bersama `LifecycleApprovalWorkflowTests` (0 gagal) |
| 2 | Kompilasi backend ke folder scratch (`dotnet build -o <scratch>`) & suite `HumanResourceTests` | 0 error kompilasi & 0 regresi | `QuilvianSystemBackend.csproj`: **0 Error(s)** (`611 Warning(s)` pra-eksisting); proyek uji SQLite `HrScratch.csproj`: `15/15` lulus (`0` gagal) |
| 3 | ESLint + pemeriksa CSS (`check-css-usage.mjs`) untuk seluruh berkas frontend berubah | 0 error baru & 0 temuan CSS | ESLint: **0 error** & **0 warning baru** dibanding baseline `HEAD` (1 warning pra-eksisting di `approval-inbox-view.jsx`); CSS: `OK — 2 berkas diperiksa, 0 temuan` |
| 4 | Unit test frontend (`npm run test:unit`) dibanding baseline `HEAD` | 0 regresi | `5.547` kasus dijalankan (`5.398` lulus, `149` gagal vs `5.205` lulus / `186` gagal di baseline `HEAD`). 2 kasus pada daftar selisih (`billing-single-payment-action.test.mjs:401` dan `bpjs-feature-deactivation.test.mjs:246`) statusnya `# SKIP` di salinan `HEAD` (tanpa repo backend) dan menguji berkas modul lain, bukan regresi perubahan ini |
| 5 | `npm run build` | `✓ Compiled successfully`, exit 0 | **Exit 0**: `✓ Compiled successfully in 76s`, `935/935` halaman statis, `[prepare-standalone] Standalone runtime siap dijalankan.` |
| 6 | Uji browser Chrome headed (`test-with-agy`, port `9342`) modal Buat Permintaan Rekrutmen + Kotak Masuk Persetujuan + lebar ponsel `390 px` | Semua kasus PASS | **`hc-1125-01-headcount-requisition.mjs`: 7/7 PASS** (`H01`–`H07`; sekaligus merekam respons live server port `7184` versi lama mengembalikan `"REQ--000001:Apoteker:Draft"` sebelum proses backend dimuat ulang); **`hc-1125-02-mobile.mjs` (lebar `390 px`): 3/3 PASS** (`M01`–`M03`, `0 offenders`) |
| 7 | Uji end-to-end nyata tanpa mock pasca-restart backend & konfigurasi DB dev `QuilvianNewDevHamzah` | Ajukan `T12` → setujui di Kotak Masuk → status `ReadyForRecruitment` → tampil di dropdown Buat Permintaan Rekrutmen & kecualikan `Draft` | Server `PID 24112` aktif (`7184`/`5107`); `REQ--000002` (2 kursi `Perawat IGD`) disetujui oleh `hartini.sulistyowati@rsmmc.local` → `ReadyForRecruitment`; **`hc-1125-03-e2e-real.mjs`: 2/2 PASS** (`R01`–`R02`, `0` mock, `REQ--000001` `Draft` terkecualikan) |

---

## 5. Eksekusi Prasyarat Backend Lokal & DB Dev (`QuilvianNewDevHamzah`, Disetujui)

Setelah persetujuan eksplisit pengguna pada `25 November 2026`:
1. Proses backend lama (`PID 18740`/`8956`) dihentikan, `QuilvianSystemBackend.csproj` dikompilasi ke `bin\Debug\net9.0` (`0 Error(s)`, `6 Warning(s)`), dan backend baru dinyalakan pada port `7184`/`5107` (`PID 24112`).
2. Konfigurasi transaksional (`BEGIN; ... COMMIT;`) diterapkan secara eksklusif pada database dev `QuilvianNewDevHamzah`:
   - `MstWorkflowStep`: ditambahkan `T11_SDM` (`994f3836-6255-480f-922c-3ac3c69907d1`) dan `T12_SDM` (`3799ded4-7b4d-47a2-9ce5-945bb2556350`) dengan `ApproverSourceType = 'Position'` menunjuk jabatan **Kepala Bagian SDM** (`adda66fa-006c-4f4f-af20-bcc471e5c9af`).
   - `MstRejectionReason`: ditambahkan masing-masing 3 alasan penolakan aktif (`T11_TOLAK_01..03` untuk `HeadcountRequestWithinFormation` dan `T12_TOLAK_01..03` untuk `HeadcountRequestBeyondFormation`).
   - `WfpOrganizationAssignment`: dilengkapi `HospitalSiteId = '5ba86bc4-bffa-4691-a3be-d2a843564957'` (`SITE-MMC-001`) pada 3 penugasan utama akun penguji SDM (`09b879e3-...`, `22a2e65b-...`, `14415fb3-...`).
   - Satu contoh nyata Permintaan Headcount `REQ--000002` (`2e768791-00ab-4520-bc47-d4e329396f71`, `Perawat IGD`, `Instalasi Gawat Darurat`, 2 kursi `Rekrut Luar`) diajukan melalui aplikasi oleh `mira.safitri@rsmmc.local` dan disetujui lewat Kotak Masuk Persetujuan oleh `hartini.sulistyowati@rsmmc.local` sehingga siap dipilih di **Buat Permintaan Rekrutmen**.
