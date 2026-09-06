# Laporan Perubahan Backend — `BE-BKC-029`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-029` — Kategori, plafon, dan penjaga write-off selisih |
| `TASK TYPE` | Implementasi backend (percabangan kategori pada alur pengajuan/persetujuan/outstanding yang sudah ada) |
| `COMPLEXITY` | `HIGH` — mesin outstanding yang dipakai empat service disentuh (skor ≥ 1), kontrak API bertambah field pada tiga DTO (skor 2), penjaga bisnis baru (`BIL-VAL-040`–`042`) dan dua aturan lama dipertegas (`BIL-VAL-018`, `BIL-VAL-023`) |
| `CLASSIFICATION SCORE` | 4 |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/{Services,Dtos,Controllers}/` dan `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingFinancialExceptionServiceTests.cs` |
| Gelombang | `MVP-11` bagian ketiga (`EPIC BKC-09`) — **penutup gelombang `MVP-11`** |
| Trace | `FR-BKC-040`–`FR-BKC-044`; `BKC-DEC-080`, `BKC-DEC-036`; `BKC-DES-023`, `BKC-DES-024`, `BKC-DES-025` |
| Blueprint | `BIL-CASH-001` revisi `0.8` — kontrak dikunci 4 September 2026 |
| Kontrak berlaku | `BIL-API-0.7`, `BIL-VALIDATION-0.7` (`BIL-VAL-040`–`042`, `BIL-VAL-018`/`023` dipertegas), `BIL-STATE-0.7` — seluruhnya `approved` |
| Dependency | `BE-BKC-028` (source selesai, menunggu `dotnet test`) |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` — percabangan baru pada method service yang sudah ada, field baru pada tiga DTO yang sudah ada. Bukan `Trx*` legacy, bukan `LEGACY MIGRATION`, **tidak ada migration** (kolom `Category` sudah disiapkan `BE-BKC-027`) |
| QBE ID yang berlaku | `QBE-VAL-001` (invarian bisnis baru — inti task), `QBE-API-001`/`QBE-DTO-001` (field baru pada DTO request/response yang sudah ada) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`/`QBE-CFG-*`/`QBE-DB-*` (tidak ada entity/kolom/migration baru), `QBE-MOD-002/003`, `QBE-NAM-*` |

Selisih governance sama seperti `BE-BKC-025`/`028`; tidak ada selisih baru.

---

## 1. Masalah yang diselesaikan

`BE-BKC-028` memindahkan selisih yang tidak dapat ditagihkan dari `UnresolvedAmount` ke
`NonBillableResidualAmount` dan mempersistnya ke kolom hasil `BE-BKC-027` — tetapi belum ada
mekanisme bagi Finance untuk benar-benar **menyelesaikan** nominal itu. Task ini menyambungkan
selisih tersebut ke alur write-off yang **sudah ada sepenuhnya** (pengajuan, persetujuan
maker-checker, reversal) lewat satu kolom kategori, tanpa membuat mekanisme kedua.

## 2. Temuan penting sebelum implementasi: cakupan dampak lebih luas dari yang didokumentasikan

Dokumen arsitektur (`02-backend-architecture.md` § amendment revisi `0.8`) hanya menyebut
`BillingFinancialExceptionService.CalculateOutstandingAsync` sebagai method yang perlu menyaring
kategori. Pemeriksaan lapangan (`grep` seluruh repository untuk `BilWriteOffCases`) menemukan
**formula outstanding yang identik diduplikasi di empat service**, bukan satu:

| Service | Method | Dipakai untuk |
| --- | --- | --- |
| `BillingFinancialExceptionService` | `CalculateOutstandingAsync` (private) | Plafon pengajuan/persetujuan write-off dan adjustment |
| `BillingFinalizationService` | `CalculateOutstandingAsync` (private, implementasi terpisah) | Gerbang kesiapan finalisasi invoice (`PreviewAsync`/`FinalizeAsync`) |
| `BillingAllocationService` | `CalculateInvoicePositionAsync` (private) | Menentukan porsi pembayaran masuk ke outstanding vs refundable credit |
| `BillingSettlementService` | `ValidateTargetAndAmountAsync` (inline, bukan method terpisah) | Plafon nominal settlement/pembayaran yang dapat diterima |

Tanpa perbaikan di keempat tempat, write-off residual non-billable akan **tetap mengurangi**
piutang pasien pada tiga alur lain (finalisasi, alokasi pembayaran, settlement) walaupun sudah
benar di alur pengecualian finansial sendiri — persis kesalahan yang `BKC-DES-024` sengaja
mencegahnya, hanya berpindah lokasi. Roadmap task ini sendiri menyebutnya sebagai risiko yang
menyentuh "outstanding yang dipakai **seluruh alur pembayaran**" (§ Risiko), sehingga perbaikan
diterapkan ke seluruh empat lokasi secara konsisten — bukan hanya satu yang disebut eksplisit di
tabel kelas dokumen arsitektur. Ini didokumentasikan di sini alih-alih diam-diam diperbaiki supaya
delta antara dokumen dan implementasi tercatat.

## 3. Proses bisnis

**Tujuan.** Setiap rupiah yang tidak dapat ditagihkan kepada siapa pun berakhir sebagai keputusan
bernama pelaku, lewat alur maker-checker yang sudah dipercaya untuk piutang pasien.

**Pelaku.** Petugas keuangan mengajukan; atasannya (orang kedua) menyetujui. Sistem **tidak**
mengajukan sendiri (`BKC-DES-023`, tidak berubah) — task ini murni menambah kategori pada mekanisme
yang sudah ada, bukan mekanisme baru.

**Aturan bisnis.**

| Kode | Aturan |
| --- | --- |
| `BIL-VAL-040` | Write-off `NON_BILLABLE_RESIDUAL` dibatasi **sisa residual**, bukan outstanding pasien |
| `BIL-VAL-041` | Write-off `NON_BILLABLE_RESIDUAL` **tidak boleh** ditandai pelunasan penuh |
| `BIL-VAL-042` | Kategori wajib salah satu nilai terdaftar; teks asing ditolak, kosong → `PATIENT_AR` |
| `BIL-VAL-018` (dipertegas) | Hanya `PATIENT_AR` dengan pelunasan penuh yang memindahkan invoice ke `SETTLED_BY_WRITE_OFF` |
| `BIL-VAL-023` (dipertegas) | Residual kini punya jalur penyelesaian eksplisit lewat write-off, bukan tertahan tanpa tindak lanjut |

**Perubahan status.** Case berpindah `SUBMITTED` → `POSTED`/`REJECTED` (tidak berubah dari
mekanisme yang sudah ada). Status **invoice** hanya berpindah untuk kategori `PATIENT_AR`.

**Jalur tidak normal.** Pembatalan (`ReverseAsync`) menghasilkan `BilAdjustment` koreksi baru,
membuka kembali plafon kategorinya masing-masing, tanpa menghapus riwayat — mekanisme yang sama
persis dengan piutang pasien, sudah bekerja otomatis benar begitu `IsFullSettlement` residual
selalu `false` (lihat § 4).

**Hasil akhir.** Sisa selisih menjadi nol; sisa tagihan pasien **tetap seperti semula** — dibuktikan
lewat probe outstanding pada test (§ 6).

## 4. Perubahan yang dikerjakan

### 4.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/02-backend-architecture.md` § amendment revisi `0.8` (baris ±1370–1520) | Spesifikasi persis kolom `Category` (sudah ada dari `BE-BKC-027`), method baru/diperbarui, plafon bercabang |
| `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` § amendment "Residual non-billable dirutekan ke write-off" | Wording persis `BIL-VAL-040`–`042`, `BIL-VAL-018`/`023` dipertegas |
| `Areas/.../Billing/Services/BillingFinancialExceptionService.cs` | Lokasi utama perubahan |
| `Areas/.../Billing/Services/{BillingFinalizationService,BillingAllocationService,BillingSettlementService}.cs` | Ditemukan berisi formula outstanding yang identik — lihat § 2 |
| `Areas/.../Billing/Controllers/BillingFinancialExceptionsController.cs` | Konfirmasi tidak ada endpoint baru; payload yang berubah |
| `Areas/.../Billing/Dtos/{BillingWriteOffDtos.cs,BillingAdjustmentDtos.cs}` | Bentuk `CreateWriteOffRequest`/`WriteOffResponse`/`InvoiceFinancialExceptionsResponse` yang sudah ada |
| `Areas/.../Billing/Models/BilWriteOffCase.cs` | Konfirmasi `Category`/`BillingWriteOffCategories` sudah ada dari `BE-BKC-027` — tidak perlu diubah |
| `Tests/.../BillingFinancialExceptionServiceTests.cs`, `BillingFinalizationServiceTests.cs` | Pola test existing; konfirmasi regresi `PreviewReflectsWriteOffCoveredOutstandingAsReady` tetap valid (write-off tanpa `Category` eksplisit default ke `PATIENT_AR`, tersaring benar oleh filter baru) |

### 4.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Dtos/BillingWriteOffDtos.cs` | `CreateWriteOffRequest` +`Category` (opsional, `MaxLength(30)`), +`IsFullSettlement` (bool, penanda niat pengaju — lihat § 5 Catatan Desain); `WriteOffResponse` +`Category` |
| `Billing/Dtos/BillingAdjustmentDtos.cs` | `InvoiceFinancialExceptionsResponse` +`NonBillableResidualRemaining` |
| `Billing/Services/BillingFinancialExceptionService.cs` | `CreateWriteOffAsync`: resolusi+validasi kategori (`BIL-VAL-042`), penjaga `BIL-VAL-041`, plafon bercabang (`BIL-VAL-040`), `Category` masuk `PayloadHash`. `ApproveWriteOffAsync`: plafon ulang bercabang, `IsFullSettlement` hanya untuk `PATIENT_AR` (`BIL-VAL-018` dipertegas). `CalculateOutstandingAsync`: `writeOffTotal` menyaring `PATIENT_AR`, `adjustmentNet` mengecualikan reversal residual. Baru: `CalculateNonBillableResidualRemainingAsync` (dua overload), `GetNonBillableResidualWriteOffCaseIdsAsync`, `GetNonBillableResidualRemainingAsync` (publik), `ResolveWriteOffCategory`. `MapWriteOff`/audit methods +`Category` |
| `Billing/Services/BillingFinalizationService.cs` | `CalculateOutstandingAsync` (private, implementasi terpisah): filter kategori yang sama — lihat § 2 |
| `Billing/Services/BillingAllocationService.cs` | `CalculateInvoicePositionAsync`: filter kategori yang sama — lihat § 2 |
| `Billing/Services/BillingSettlementService.cs` | `ValidateTargetAndAmountAsync`: filter kategori yang sama — lihat § 2 |
| `Billing/Controllers/BillingFinancialExceptionsController.cs` | `GetByInvoice` mengisi `NonBillableResidualRemaining` lewat `GetNonBillableResidualRemainingAsync` baru |
| `Tests/.../BillingFinancialExceptionServiceTests.cs` | `SeedInvoiceWithCalculationAsync`/`WriteOffRequest` diperluas parameter opsional (backward compatible); 7 test baru — lihat § 6 |

Total: **8 berkas source/test berubah**. Tidak ada DTO/controller/entity/migration baru selain
field yang disebut di atas — task ini murni percabangan pada method yang sudah ada.

### 4.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Additive.** `Category` opsional berbawaan `PATIENT_AR` pada request (`BKC-DES-014` — konsumen lama tidak rusak); `Category`/`NonBillableResidualRemaining` baru pada response. Satu perilaku **berubah nilainya**: write-off `NON_BILLABLE_RESIDUAL` kini memakai plafon berbeda dan tidak pernah memindahkan status invoice — perubahan yang memang tujuan task |
| `DATABASE IMPACT` | **Nihil.** Kolom `Category` sudah ada dari `BE-BKC-027`. Task ini hanya MEMBACA/MENULIS kolom yang sudah disiapkan |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization/permission — `BillingWriteOff : Create/Approve` yang sudah ada tetap berlaku untuk kedua kategori |
| `VISUAL REFERENCE` | `NOT REQUIRED` — konsumen frontend (`FE-BKC-021`) di luar scope task ini |

## 5. Catatan desain: `IsFullSettlement` sebagai field request baru

Dokumen arsitektur menulis `BIL-VAL-041` seolah `IsFullSettlement` sudah ada sebagai input
pengajuan ("Finance mengajukan ... sambil mencentang 'pelunasan penuh'"). **Source sebenarnya
tidak pernah memiliki field ini pada `CreateWriteOffRequest`** — `IsFullSettlement` MURNI
diturunkan server saat approval dari `outstandingAfter == 0` (`ApproveWriteOffAsync`), tidak pernah
dikirim pengguna, untuk kategori apa pun, sejak awal. Ini delta nyata antara dokumen dan source
(dicatat, bukan didiamkan).

**Keputusan yang diambil:** menambah `IsFullSettlement` sebagai field request **baru**, opsional
berbawaan `false` (konsumen lama yang tidak mengirimnya tetap mendapat perilaku identik hari ini).
Field ini **hanya** divalidasi (`BIL-VAL-041`) untuk kategori `NON_BILLABLE_RESIDUAL` pada saat
pengajuan; untuk `PATIENT_AR`, field ini **tidak** dipakai untuk menimpa derivasi
`outstandingAfter == 0` yang sudah ada — regresi wajib nol pada piutang pasien murni lebih
diutamakan daripada mengikat literal wording dokumen. Perlindungan `PATIENT_AR`-tidak-pernah-
diganti-residual tetap ditegakkan dua lapis: divalidasi di pengajuan (jika field dikirim `true`)
**dan** dipaksa `false` di persetujuan tanpa syarat apa pun (`writeOffCase.IsFullSettlement =
!isResidual && plafonAfter == 0`) — lapis kedua inilah yang benar-benar menjaga `BIL-VAL-018`
dipertegas, terlepas dari apa yang dikirim di pengajuan.

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna: build backend dijalankan manual |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis seluruh lokasi formula outstanding | **LULUS** | Manual | `grep -rn "BilWriteOffCases"` pada seluruh source (di luar test) menunjukkan tepat 4 lokasi formula (§ 2), seluruhnya sudah menyaring `Category == PatientAr` dan mengecualikan reversal residual dari `adjustmentNet` secara konsisten |
| Verifikasi regresi `PATIENT_AR` tanpa `Category` eksplisit | **LULUS (analisis + test baru)** | Manual + `PatientArFullWriteOffStillClosesInvoiceExactlyLikeBeforeAmendment`, `PreviewReflectsWriteOffCoveredOutstandingAsReady` (existing, tidak diubah) | `Category` default resolusi menghasilkan `PATIENT_AR` persis untuk request yang tidak mengirimnya — seluruh call site existing (test lama, kode produksi apa pun yang belum diperbarui) otomatis mendapat kategori yang benar tanpa perubahan |
| Cakupan diff | **LULUS** | Manual | 8 berkas source/test berubah (§ 4.2); nol migration, nol entity, nol controller baru, nol endpoint baru |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna.

### Test baru (`BillingFinancialExceptionServiceTests.cs`)

| Test | Membuktikan |
| --- | --- |
| `WriteOffDefaultsToPatientArAndOutstandingIsIndependentOfResidualPlafon` | `BIL-AT-058` — plafon Rp 30.000 vs outstanding Rp 85.000: pengajuan Rp 45.000 residual ditolak `422` dengan pesan yang tepat menyebut selisih (bukan outstanding); kategori bawaan terbukti `PATIENT_AR` |
| `ApprovedResidualWriteOffDoesNotShiftPatientOutstandingOrInvoiceStatus` | `BIL-AT-059` — persetujuan residual Rp 30.000: status `POSTED`, `IsFullSettlement=false`, invoice tetap `OPEN`, sisa residual nol; probe `PATIENT_AR` Rp 85.000 (persis outstanding awal) tetap berhasil — outstanding pasien terbukti tidak bergeser |
| `WriteOffUnknownCategoryIsRejected` | `BIL-AT-060` butir kategori — teks asing `422`, **bukan** diam-diam `PATIENT_AR`; tidak ada case tersimpan |
| `WriteOffResidualCannotBeMarkedFullSettlement` | `BIL-AT-060` butir pelunasan — residual + `IsFullSettlement=true` pada pengajuan `422` |
| `SameIdempotencyKeyWithDifferentCategoryIsRejectedAsConflict` | Acceptance 5 — `Category` ikut `PayloadHash`; idempotency-key sama dengan kategori berbeda `409` (konflik isi), bukan replay diam-diam |
| `ReversingPostedResidualWriteOffReopensResidualPlafonOnly` | `BIL-AT-061` — reversal residual: invoice tetap `OPEN` (tidak pernah `SETTLED_BY_WRITE_OFF` untuk mulai), plafon residual kembali Rp 30.000 dan dapat diajukan ulang penuh; probe outstanding `PATIENT_AR` tetap Rp 85.000 sepanjang siklus create→approve→reverse |
| `PatientArFullWriteOffStillClosesInvoiceExactlyLikeBeforeAmendment` | Regresi eksplisit — write-off `PATIENT_AR` penuh tetap memindahkan invoice ke `SETTLED_BY_WRITE_OFF` persis seperti sebelum amendment |

**`RequesterCannotApproveOwnWriteOff` (existing, `BE-BKC-` awal) tidak diduplikasi untuk kategori
residual** — logika maker-checker (`BIL-VAL-017`) tidak bercabang kategori sama sekali (diperiksa
sebelum cabang kategori mana pun dijangkau), sehingga satu test yang sudah ada mencakup kedua
kategori secara struktural.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-058` — plafon Rp 30.000 bukan Rp 85.000 | **Tercakup** |
| `BIL-AT-059` — disetujui B, outstanding tetap, status tidak berpindah, sisa selisih nol | **Tercakup** |
| `BIL-AT-060` — tiga uji negatif (self-approve, pelunasan penuh, kategori asing) | **Tercakup** (self-approve lewat test existing yang berlaku struktural, dua lainnya test baru) |
| `BIL-AT-061` — pembatalan `POSTED`: catatan koreksi, outstanding tetap, invoice tidak dipaksa, plafon kembali dan dapat diajukan ulang | **Tercakup** |
| Field `category` wajib masuk payload hash | **Tercakup** |
| Regresi write-off piutang pasien nol | **Tercakup** — `PatientArFullWriteOffStillClosesInvoiceExactlyLikeBeforeAmendment` + `PreviewReflectsWriteOffCoveredOutstandingAsReady` (existing) |
| Build dan test lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum sepenuhnya tercapai.** Yang tersisa: `dotnet test` manual oleh pengguna.
Satu contoh kasus lengkap (pengajuan→persetujuan→pembatalan) beserta jejak audit tersanitasi
**tercakup** lewat `ReversingPostedResidualWriteOffReopensResidualPlafonOnly` — audit method
(`AuditWriteOffAsync`/`AuditWriteOffApprovalAsync`) kini menyertakan `Category` sehingga jejaknya
bernama kategori, bukan hanya nominal.

## 8. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Sama seperti `BE-BKC-028`: kolom `BilWriteOffCase.Category`/`BilCalculationVersion.NonBillableResidualAmount` (`BE-BKC-027`) **belum ada secara fisik** di basis data mana pun — migration belum dijalankan (`BKC-GATE-09`). Kode task ini AKAN GAGAL runtime bila diaktifkan sebelum migration itu dieksekusi |
| `KNOWN ISSUES` | Empat lokasi formula outstanding yang diperbaiki (§ 2) adalah duplikasi arsitektural pra-existing di luar kendali task ini — tidak direfaktor menjadi satu method bersama (di luar scope, dan `AGENTS.md` melarang refactor massal legacy yang tidak diminta); hanya filter kategorinya yang disamakan di keempat lokasi |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi dan migration `BE-BKC-027` sudah dijalankan |
| `INCIDENTAL CHANGES` | Tiga berkas di luar `BillingFinancialExceptionService.cs` (`BillingFinalizationService.cs`, `BillingAllocationService.cs`, `BillingSettlementService.cs`) tersentuh — **bukan** perluasan scope sepihak, melainkan penerapan konsisten aturan bisnis yang sama (`BKC-DES-024`) ke seluruh titik ia berlaku, sesuai risiko yang disebutkan eksplisit oleh roadmap task ini sendiri ("outstanding yang dipakai seluruh alur pembayaran") — lihat § 2 untuk penjelasan lengkap |
| `INTERRUPTIONS` | Tidak ada |
| `GIT STATUS` | 8 berkas task ini berubah, belum di-stage. Working tree juga memuat perubahan tersendiri dari `BE-BKC-022`–`028` yang belum di-build/test resmi oleh pengguna. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet test` mencakup seluruh perubahan yang menumpuk (`BE-BKC-022` s.d. `029`). Migration `BE-BKC-027` **MUST** dijalankan ke basis data sebelum kode `BE-BKC-027`–`029` dapat berfungsi di lingkungan mana pun. `MVP-11` **selesai secara source** dengan task ini — task backend berikutnya sesuai urutan: `BE-BKC-031` (verifikasi data PPN, `READY` kapan saja, bukan task kode) atau `BE-BKC-030` (masih `BLOCKED` oleh `BKC-GATE-06` — tiga sebab, lihat roadmap) |
