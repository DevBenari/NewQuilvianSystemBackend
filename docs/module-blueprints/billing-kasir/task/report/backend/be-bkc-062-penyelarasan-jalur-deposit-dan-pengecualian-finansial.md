# BE-BKC-062 — Penyelarasan pada jalur deposit dan pengecualian finansial

## Ringkasan untuk pembaca umum

`BE-BKC-061` menutup lubang untuk tagihan yang lunas lewat pembayaran tunai/non-tunai. Tapi
tagihan juga bisa lunas lewat dua jalur lain: **dana deposit** pasien rawat inap yang dialokasikan
ke tagihan, dan **penyesuaian/write-off** (koreksi nominal tagihan yang diposting petugas Finance).
Tanpa task ini, tagihan yang lunas lewat kedua jalur itu akan tetap macet berstatus "Final" —
lubang yang sama, pintu masuk berbeda.

Task ini memasang "pemeriksa" yang sama (dibangun di `BE-BKC-060`) pada keempat titik di mana
penyesuaian/write-off benar-benar mengubah sisa tagihan pasien (diposting maupun dibalik), plus
satu titik pada alokasi deposit — dengan satu catatan penting yang ditemukan saat implementasi:
alokasi deposit ternyata **hanya boleh** menyasar tagihan yang masih berjalan (belum final), jadi
pemeriksa di titik itu untuk saat ini tidak pernah benar-benar aktif. Rinciannya di bawah.

---

- TASK ID: BE-BKC-062
- TASK TYPE: Fitur (pemasangan penyelarasan status pada empat titik pengecualian finansial + satu titik alokasi deposit)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 5 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah 2 → 1 + logika bisnis sedang (lima titik, dua pola berbeda: dalam-transaksi vs melewati konstraint existing) → 1 + kontrak API tidak ada → 0 + database hanya perilaku persistence yang sudah ada → 1 + keamanan tidak ada → 0 + UI tidak ada → 0; dinaikkan satu tingkat dari raw score 3 karena lima titik pemanggilan tersebar di dua service dengan konstraint bisnis yang berbeda-beda per titik)
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED: `Services/BillingAllocationService.cs`, `BillingFinancialExceptionService.cs` (dibaca penuh — `CreateAdjustmentAsync`, `ApproveAdjustmentAsync`, `CreateWriteOffAsync`, `ApproveWriteOffAsync`, `ReverseAdjustmentAsync`, `ReverseWriteOffAsync`), `Controllers/BillingPatientFundsController.cs`, `contracts/state-transition-matrix.md` (tabel dampak reversal write-off)
- FILES CHANGED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs`
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs`
- IMPLEMENTATION:
  1. **`BillingAllocationService.AllocateDepositAsync`** — dependency `BillingInvoiceClosureService` ditambah; sesudah `SaveChangesAsync` pertama dan sebelum `CommitAsync`, memanggil `SyncClosureInsideTransactionAsync` (helper privat baru: delegasi ke `SyncClosureAsync`, tangkap `BillingInvoiceClosureValidationException` → `BillingAllocationValidationException`, `SaveChangesAsync` lagi bila berubah). Audit `BillingInvoice.ClosureSynced` ditulis sesudah commit bila status benar-benar berubah.
  2. **`BillingFinancialExceptionService.cs`** — helper privat baru `SyncClosureInsideTransactionAsync` (pola sama) dan `AuditClosureChangeAsync` (menerima parameter `trigger` supaya satu helper dipakai keempat titik dengan label audit berbeda). Dipasang di:
     - `ApproveAdjustmentAsync` — sesudah penyesuaian diposting (trigger `AdjustmentPosted`)
     - `ApproveWriteOffAsync` — sesudah write-off diposting (trigger `WriteOffPosted`)
     - `ReverseAdjustmentAsync` — sesudah pembalikan penyesuaian (trigger `AdjustmentReversed`)
     - `ReverseWriteOffAsync` — sesudah pembalikan write-off (trigger `WriteOffReversed`)

     Keempatnya dipasang **di dalam** transaksi masing-masing (sesudah `SaveChanges` peristiwa itu sendiri, sebelum `CommitAsync`) — **bukan** berdampingan dengan `BillingArApHandoffService.RecordCorrectionIfLinkedAsync`, yang tetap berjalan di transaksi terpisahnya sendiri sesudah commit (sudah begitu sejak sebelum task ini; **tidak disentuh**).
  3. Method privat `CalculateOutstandingAsync` (pembungkus dari `BE-BKC-060`) dan kedua helper baru **tidak** duplikat isinya — helper closure ditulis sekali, dipanggil empat kali dengan `trigger` berbeda.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: **`TOUCHED LEGACY`** untuk kedua berkas. Tidak ada model, migration, maupun perubahan skema. QBE ID yang berlaku: `QBE-SVC-001` (dipatuhi, seluruh pemanggilan lewat service, bukan controller langsung).
- **Temuan yang MUST dibaca sebelum menganggap titik alokasi deposit "aktif"**: `BillingAllocationService.AllocateDepositAsync` baris 114 (sebelum task ini, tidak diubah) menolak permintaan dengan `"Hanya invoice OPEN yang dapat menerima allocation."` bila `invoice.Status != Open`. Karena alokasi deposit **tidak pernah** mengubah `invoice.Status` sendiri, pada saat `SyncClosureInsideTransactionAsync` dipanggil, status invoice **selalu** `OPEN` — dan penjaga `SyncClosureAsync` (hanya bertindak untuk `FINAL`/`CLOSED`) **selalu** melewatkannya. Wiring ini karena itu **provably tidak pernah aktif** pada source saat ini; ia dipasang untuk (a) konsistensi dengan lima titik lain yang sama-sama menggerakkan sisa tagihan, dan (b) jaring pengaman bila gerbang OPEN-only pada baris 114 kelak dilonggarkan. **Tidak ditemukan** mekanisme auto-finalize setara `TryAutoFinalizeInvoiceAsync` pada jalur deposit (`BillingPatientFundsController.AllocateDeposit` langsung mengembalikan hasil tanpa memanggil finalisasi apa pun) — bila kelak dibutuhkan, itu keputusan produk terpisah, bukan bagian task ini.
  - Empat titik pada `BillingFinancialExceptionService.cs` **genuinely aktif** — dikonfirmasi lewat pembacaan gerbang masing-masing method: `CreateWriteOffAsync`/write-off `PATIENT_AR` dapat diajukan atas invoice `FINAL` (ditolak hanya untuk `CLOSED`/`SETTLED_BY_WRITE_OFF`), sehingga posting/pembalikannya dapat menyentuh invoice `FINAL`/`CLOSED` sungguhan.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Nol perubahan bentuk — sama seperti `BE-BKC-060`/`061`, hanya nilai `status`/`closedAt` pada response invoice yang berubah pada alur normal (kini juga mencakup jalur penyesuaian/write-off).
- DATABASE IMPACT: Nol perubahan skema.
- SECURITY IMPACT: Nol perubahan hak akses. Keempat titik `BillingFinancialExceptionService` tetap diperiksa otorisasinya di pintu masuk masing-masing (`BillingAdjustment : Approve`, `BillingWriteOff : Approve`, `BillingFinancialException : Reverse` — tidak berubah).
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/check | Result | Classification | Evidence/note |
  | --- | --- | --- | --- |
  | `dotnet build QuilvianSystemBackend.csproj` | Dijalankan pengguna 22 September 2026 (sesi lain, task `BE-FIN-012`) — **gagal 2x** `CS1501 No overload for method 'SyncClosureInsideTransactionAsync' takes 5 arguments` pada `ApproveWriteOffAsync` (baris 465) dan `ReverseWriteOffAsync` (baris 720): keduanya menyisipkan argumen ke-5 (`PrescriptionClearanceReasonCodes.*`) yang tidak ada pada signature 4-parameter method ini — argumen itu tidak pernah dipakai (`PublishForClearanceChangeAsync` di baris berikutnya sudah memakai reason code literalnya sendiri secara independen). **Diperbaiki**: kedua argumen ke-5 dihapus, dua pemanggilan lain (baris 219, 596) sudah benar 4 argumen sejak awal dan tidak disentuh. Build ulang belum dikonfirmasi | **DITEMUKAN GAGAL, SUDAH DIPERBAIKI** | Pesan build pengguna; `BillingFinancialExceptionService.cs` baris 465, 720 |
  | Review diff dan scope | Dilakukan | Manual | Kelima titik pemanggilan diverifikasi terhadap pembacaan langsung gerbang masing-masing method (`invoice.Status` precondition per titik) sebelum ditulis, termasuk penelusuran `BillingPatientFundsController` untuk memastikan tidak ada auto-finalize tersembunyi pada jalur deposit |
  | `BIL-AT-123` (lunas dari deposit), `BIL-AT-125` (penyesuaian Credit), `BIL-AT-127` (penyesuaian Debit atas invoice CLOSED) | **Belum dijalankan** | NOT VERIFIED | Menuntut data invoice nyata dan database berjalan |
- WARNINGS: `BIL-AT-123` (skenario "tagihan lunas seluruhnya dari alokasi deposit") **tidak dapat lulus** dengan source saat ini, karena `AllocateDepositAsync` menolak invoice non-`OPEN` — deposit tidak pernah menjadi peristiwa yang menutup tagihan `FINAL`. Ini bukan regresi dari task ini (perilaku sudah begitu sebelumnya), tetapi acceptance test `BIL-AT-123` pada `testing/acceptance-test-matrix.md` **MUST ditinjau ulang** — skenarionya perlu diganti menjadi "tagihan `OPEN` dilunasi penuh dari deposit, lalu difinalisasi" (menggabungkan alokasi deposit dengan titik ketujuh `BKC-DES-036` dari `BE-BKC-061`) supaya benar-benar dapat lulus, atau dicatat eksplisit sebagai skenario yang belum didukung.
- KNOWN ISSUES:
  0. **Addendum 22 September 2026 (Claude Sonnet 5, sesi lain — task `BE-FIN-012`, otorisasi eksplisit pemilik repository)**: lihat baris `dotnet build` pada VALIDATION — dua pemanggilan `SyncClosureInsideTransactionAsync` di `ApproveWriteOffAsync`/`ReverseWriteOffAsync` membawa argumen ke-5 yang salah dan tidak pernah dikompilasi sebelumnya. Diperbaiki (argumen dihapus); perbaikan murni kompilasi, tidak mengubah aturan bisnis penyelarasan status apa pun. Berada di luar wewenang task `BE-FIN-*` yang menemukannya; dilakukan atas otorisasi eksplisit. `Invoke-QbeConformanceCheck.ps1` sesudah perbaikan: `PASS` (0 violation).
  1. Sama seperti `BE-BKC-060`/`061`: **belum dibangun/dijalankan sama sekali**.
  2. Titik alokasi deposit (`BillingAllocationService.AllocateDepositAsync`) adalah kode yang provably tidak pernah aktif pada source saat ini — lihat penjelasan di atas. **MUST** dikonfirmasi ulang owner apakah ini tetap diinginkan sebagai jaring pengaman, atau lebih baik dihapus sampai ada kebutuhan nyata (prinsip "yang sengaja tidak dibuat" pada blueprint umumnya menghindari kode yang tidak pernah tereksekusi).
  3. `BIL-AT-123` pada `testing/acceptance-test-matrix.md` perlu ditinjau ulang (lihat WARNINGS) — **belum** diperbaiki pada task ini karena mengubah acceptance test adalah wewenang `/design-business-module`, bukan `build-module-backend`.
  4. `BE-BKC-063` (perluasan penjaga koreksi AR, satu baris `BillingArApHandoffService.cs:150`) **belum dikerjakan** — sampai task itu selesai, koreksi AR untuk invoice yang baru `CLOSED` lewat penyesuaian/write-off pada task ini masih akan gagal tercatat diam-diam (`RecordCorrectionIfLinkedAsync` masih menolak status `CLOSED`).
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS:
  ```text
   M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs
  ?? Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs
  ```
  (`BillingManagementServiceCollectionExtensions.cs` dan `BillingFinalizationService.cs`/`BillingSettlementService.cs` adalah residu `BE-BKC-060`/`061` yang belum dibuild, bukan perubahan baru task ini)
- NEXT RECOMMENDED STEP: (1) **Pengguna menjalankan `dotnet build` secara manual** — sekarang mencakup lima berkas service yang saling bergantung dari tiga task (`BE-BKC-060`, `061`, `062`), belum pernah dikompilasi bersama sama sekali. (2) Putuskan `KNOWN ISSUES` butir 2 (pertahankan atau hapus wiring alokasi deposit yang tidak pernah aktif). (3) Lanjutkan `BE-BKC-063` (perluasan penjaga koreksi AR) — independen, satu baris, tidak bergantung pada build task ini berhasil. (4) Tinjau ulang `BIL-AT-123` di `testing/acceptance-test-matrix.md` lewat `/design-business-module`.
