# BE-BKC-017 — Evidence Matrix Acceptance Test (`BIL-AT-001`–`024`)

`contract_version: BIL-TEST-0.4` · Disusun 26 Agustus 2026 sebagai bagian pengerjaan `BE-BKC-017` (Hardening dan acceptance lintas-slice). Dokumen ini mengonsolidasikan bukti test untuk 24 ID di `testing/acceptance-test-matrix.md`, diverifikasi langsung terhadap source dan hasil `dotnet test` — bukan berdasarkan klaim dokumen lain. Status per ID mengikuti tiga label: **Covered** (ada test yang mengeksekusi skenario intinya), **Covered (catatan)** (skenario inti terbukti tapi detail literal — misalnya angka contoh spesifik — tidak identik), **Partial** (sebagian skenario terbukti, sebagian belum), **Tidak ditemukan** (tidak ada test).

Validasi terakhir: `dotnet build` → 0 error. `dotnet test --filter FullyQualifiedName~BillingManagement` → **155/155 pass** (139 test slice sebelumnya + 15 test RBAC baru di `AccessPermissionEnforcementTests.cs` + 1 test regresi batas UTC/WIB untuk perbaikan performa admin-fee di `BillingCalculationServiceTests.cs`).

## Ringkasan

| Status | Jumlah ID |
| --- | --- |
| Covered | 15 |
| Covered (catatan) | 5 |
| Partial | 3 |
| Tidak ditemukan | 1 |

## Tabel evidence

| ID | Status | Bukti (file:line / method) | Catatan |
| --- | --- | --- | --- |
| `BIL-AT-001` | Covered | `BillingInvoiceServiceTests.cs:21 FirstChargeCreatesOneInvoiceAndAdditionalSourceReusesIt` | — |
| `BIL-AT-002` | Covered (catatan) | `BillingInvoiceServiceTests.cs:38 IdenticalReplayIsNoOpForSameOrNewIdempotencyKey` | Menguji replay 2× key sama + 1× key baru; skenario literal "3 kali" tidak diuji verbatim, tapi semantik intinya (satu item aktif, replay konsisten) terbukti |
| `BIL-AT-003` | Covered (catatan) | `BillingInvoiceServiceTests.cs:112,150 CompletedOrFinalProducerFactsCannotUseNormalVoid` | Perilaku void-sebelum/sesudah-complete terbukti; kode pesan literal `BIL-VAL-003` tidak pernah direferensikan di source (hanya di `validation-matrix.md`) — bukan cacat fungsional, hanya penamaan pesan tidak disinkronkan ke dokumen |
| `BIL-AT-004` | Covered (catatan) | `BillingInvoiceServiceTests.cs:95 PharmacyRequiresFinalDispensedQuantity` | Prinsip "qty dispensed yang di-charge" terbukti dengan qty 3.5; skenario literal "order 10, diserahkan 7" tidak diuji verbatim |
| `BIL-AT-005` | Covered | `BillingSettlementServiceTests.cs:24 BilAt005SuccessfulTenderSurvivesAnotherTenderFailure` | Nama method secara eksplisit menyebut ID ini |
| `BIL-AT-006` | Covered | `BillingSettlementServiceTests.cs:60 BilAt006TimeoutStaysPendingAndSameRetryDoesNotDuplicateCharge` | — |
| `BIL-AT-007` | Covered | `BillingAllocationServiceTests.cs:20 BilAt007DepositEightMillionAllocatesFiveMillionAndInvoiceStaysOpen` | Cocok persis dengan contoh Rp8 juta/Rp5 juta di roadmap |
| `BIL-AT-008` | Covered | `BillingAllocationServiceTests.cs:75 BilAt008LowerRecalculationRecognizesRefundableCredit` | — |
| `BIL-AT-009` | Covered | `BillingCalculationServiceTests.cs:95 AdministrationFeeIsOncePerLocalDayAndRanapAppliesReplacementDifference` | Dua encounter RAJAL, pasien sama, hari sama → fee kedua = 0 |
| `BIL-AT-010` | Covered | Method sama dengan `BIL-AT-009` | Transfer ke RANAP menghasilkan fee = selisih (30.000 dari 50.000), `ReplacesEarlierFee = true`, tidak dobel |
| `BIL-AT-011` | Partial | `BillingDiscountServiceTests.cs:181 AdministrationFeeCategoryCannotBeDiscounted` | Separuh "diskon admin ditolak" terbukti; separuh "insurer cover flag true → coverage ikut policy" belum ada test gabungan eksplisit |
| `BIL-AT-012` | Covered | `BillingDiscountServiceTests.cs:22 MasterPromoIsEffectiveImmediatelyAndReducesPatientPortion`, `:54 DoctorDiscountWaitsForCorrectDoctorAndOnlyThenChangesItemNet` | — |
| `BIL-AT-013` | Covered | `BillingCalculationServiceTests.cs:44 CoverageWaterfallAppliesPrimaryThenExcessThenPatient` (60k/25k/15k), `BillingArApHandoffServiceTests.cs:71 InsuredInvoiceCreatesPayerArHandoffAndKeepsApNotReady` | — |
| `BIL-AT-014` | Covered (catatan) | `BillingFinancialExceptionServiceTests.cs:19,41,60 RequesterCannotApproveOwnWriteOff` + **`AccessPermissionEnforcementTests.cs:HasAccessAsync_KasirWithoutFinanceRoleCannotApproveWriteOff`** (baru) | Self-approve (maker-checker) terbukti di level domain; penolakan role/permission murni (kasir tanpa hak Finance) kini juga terbukti di level RBAC — dua lapis proteksi berbeda, keduanya sekarang punya bukti |
| `BIL-AT-015` | Covered | `BillingFinancialExceptionServiceTests.cs:96 ReversingFullWriteOffReopensInvoiceAndCreatesDebitAdjustmentIdempotently` | — |
| `BIL-AT-016` | Covered | `CashierShiftServiceTests.cs:97 VarianceIsPersistedReviewedAndReopenedWithoutDeletingHistory` | — |
| `BIL-AT-017` | Covered | `BillingSettlementServiceTests.cs:123 BilAt017LateNonCashSuccessUpdatesOriginalPendingTenderOnly`, `:342 LateQrisSuccessAfterShiftCloseDoesNotChangePhysicalShiftCash` | — |
| `BIL-AT-018` | Partial | `BillingFinalizationServiceTests.cs:110 DepartureExceptionAllowsFinalizationWithOutstandingAndRecordsDebtor` (Death), `:131 DepartureExceptionWithoutDebtorEvidenceIsRejected` | Skenario Death+debtor sukses terbukti; skenario DAMA hanya diuji pada jalur penolakan (tanpa debtor) — belum ada test sukses DAMA-dengan-debtor |
| `BIL-AT-019` | Covered | `BillingArApHandoffServiceTests.cs:20,71,107` | Self-pay AP-ready, insured AR-payer + AP-not-ready, post-final correction "once" |
| `BIL-AT-020` | Covered (catatan) | `BillingAllocationServiceTests.cs:109 BilAt020StaleVersionLosesWithoutDuplicateAllocation`, `BillingCalculationServiceTests.cs:166` | Diuji sekuensial (panggil, lalu panggil lagi dengan versi basi) lewat EF InMemory — bukan concurrency paralel nyata. Lock produksi (`pg_advisory_xact_lock`) hanya jalan bila `IsRelational()`, sehingga tidak pernah tereksekusi di test manapun di suite ini. Korektnya *hasil* (satu sukses, satu ditolak) terbukti; penguncian nyata di bawah beban paralel belum |
| `BIL-AT-021` | Covered | `BillingFinancialExceptionServiceTests.cs:126 PostedCreditAndDebitAdjustmentsNetIntoOutstandingCorrectly`, `BillingArApHandoffServiceTests.cs:107` | — |
| `BIL-AT-022` | **Covered** (baru) | **`AccessPermissionEnforcementTests.cs`**: `HasAccessAsync_KasirWithoutFinanceRoleCannotApproveWriteOff`, `HasAccessAsync_RegularCashierCannotReopenShift`, `HasAccessAsync_KepalaKasirCanReopenShiftWhenExplicitlyGranted`, `Filter_ReturnsForbiddenWhenAuthenticatedButNotAuthorized`, `Filter_ReturnsUnauthorizedWhenCallerNotAuthenticated`, `Filter_AllowsRequestThroughWhenAuthorized` | Sebelum 26 Agustus 2026: **tidak ditemukan** — seluruh "test permission" yang ada hanya reflection atas atribut `[AccessPermission]`, tidak pernah memanggil jalur otorisasi sungguhan dengan role salah. Kini diuji lewat `AccessPermissionService.HasAccessAsync` dan `AccessPermissionFilter.OnAuthorizationAsync` SUNGGUHAN (bukan mock/reflection), persis skenario matrix: "Kasir coba approve write-off/reopen shift" → `403`, tidak ada mutation |
| `BIL-AT-023` | Tidak ditemukan | — | Terstruktur tidak bisa diuji: `BKC-BLK-INT-001` masih terbuka (tidak ada consumer AR/AP nyata di lingkungan manapun untuk disimulasikan downtime-nya). Desain "record-and-expose" (`BilArHandoff`/`BilApHandoff` lokal + `GET` untuk dipull consumer) adalah scope yang disengaja, bukan diam-diam ditinggalkan — lihat `backend-roadmap.md` BE-BKC-016. Tetap tercatat sebagai gap eksplisit sampai consumer nyata tersedia |
| `BIL-AT-024` | Partial | `BillingInvoiceServiceTests.cs:294 VoidAuditDoesNotWriteClinicalDescriptionOrSourceReference`, `BillingSettlementServiceTests.cs:155` (masking `****5678`) | Dua assert privasi log spesifik ada; belum ada log-scan otomatis menyeluruh atas semua pemanggilan `LoggerService.AuditAsync`/`WarningAsync` di modul. Bagian a11y (keyboard/status-bukan-warna) adalah tanggung jawab `FE-BKC-010` (frontend) — sudah dikerjakan terpisah, lihat laporan FE terkait |

## Gap yang masih terbuka setelah pengerjaan ini

1. **`BIL-AT-023`** — terikat penuh pada `BKC-BLK-INT-001` (consumer AR/AP eksternal). Tidak bisa ditutup dari sisi kode Billing sendiri.
2. **`BIL-AT-011`, `BIL-AT-018`** — partial, butuh satu test tambahan masing-masing (kombinasi admin-fee+coverage; DAMA-dengan-debtor sukses) untuk jadi Covered penuh.
3. **`BIL-AT-020`** — korektnya hasil concurrency terbukti sekuensial; penguncian Postgres nyata (`pg_advisory_xact_lock`) tidak pernah dieksekusi oleh test manapun karena seluruh suite memakai EF InMemory (`IsRelational() == false`). Menutup ini butuh test terhadap database relasional nyata (Postgres lokal), bukan sekadar assertion tambahan.
4. **Sanitized Swagger examples** — nol, belum dikerjakan sama sekali (di luar cakupan 24 acceptance ID, tapi tetap bagian scope `BE-BKC-017`).
5. **Gap performa query admin-fee cross-invoice** — **sudah diperbaiki** 26 Agustus 2026. `CalculateAdministrationFeeAsync` kini memakai pre-filter SQL pada `TrxPatientEncounter.EncounterDate` (rentang UTC ±1 hari di sekitar businessDate WIB target) sebelum menarik `BreakdownSnapshot` ke memori, menggantikan penarikan seluruh riwayat kalkulasi pasien. Percobaan pertama sempat salah memakai `BilCalculationVersion.CalculatedAt` (jam kalkulasi dijalankan, bukan tanggal klinis) sebagai kolom filter — ditangkap oleh test yang gagal (`AdministrationFeeIsOncePerLocalDayAndRanapAppliesReplacementDifference`) sebelum sempat jadi regresi, lalu dikoreksi ke `EncounterDate`. Bukti tambahan: test baru `AdministrationFeeAcrossUtcMidnightBoundaryIsStillDetectedAsSameBusinessDay` membuktikan filter tetap benar untuk dua encounter pada businessDate WIB yang sama tapi tanggal kalender UTC berbeda (melintasi batas 17:00 UTC).
6. **Seed data Finance** — `MstDiscountPolicy`/`MstTaxRule`/`MstRoomChargePolicy` nol baris seed; `MstAdministrationFeePolicy` baru draft `Amount = 0`. Di luar wewenang implementasi kode (`BKC-BLK-DATA-001`, perlu nominal sah dari Finance).

## Metodologi

Setiap baris Covered/Partial di atas diverifikasi dengan membaca method test yang dirujuk secara langsung (bukan mengandalkan nama method saja) dan mengonfirmasi assertion-nya benar-benar menguji skenario yang diklaim. Baris "Tidak ditemukan" dikonfirmasi lewat pencarian menyeluruh nama method/pola di seluruh `QuilvianSystemBackend.Tests/BillingManagement/` sebelum disimpulkan tidak ada.
