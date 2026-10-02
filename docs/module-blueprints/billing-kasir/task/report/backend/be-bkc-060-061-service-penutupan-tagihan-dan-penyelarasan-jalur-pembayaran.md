# BE-BKC-060, BE-BKC-061 — Service penutupan tagihan dan penyelarasan jalur pembayaran

## Ringkasan untuk pembaca umum

Sebelum perubahan ini, tagihan pasien yang sudah dibayar lunas **tetap tampil berstatus "Final"**,
tidak pernah berubah menjadi "Closed" (tertutup). Penyebabnya: syarat lama untuk berpindah ke
"Closed" adalah "sistem piutang mengonfirmasi penerimaan tagihan" — sebuah sistem yang **belum
pernah dibangun** di aplikasi ini. Karena syarat itu tidak pernah terpenuhi, semua tagihan lunas
menumpuk di status "Final" selamanya.

Perbaikan ini mengganti syaratnya: begitu **sisa tagihan pasien mencapai nol** (dihitung dari
pembayaran, deposit, dan penyesuaian yang sudah tercatat), sistem sendiri yang memindahkan status
tagihan menjadi "Closed" — tanpa menunggu sistem lain. Dua task ini membangun mesin perhitungannya
(`BE-BKC-060`) dan memasangnya pada jalur pembayaran (`BE-BKC-061`), termasuk satu titik tambahan
yang ditemukan saat implementasi: tagihan yang **otomatis** difinalisasi karena baru saja dibayar
lunas sekaligus (bukan dicicil) juga ikut tertutup pada saat yang sama, bukan menunggu peristiwa
lain yang mungkin tidak pernah datang.

---

- TASK ID: BE-BKC-060, BE-BKC-061
- TASK TYPE: Refactor + fitur (konsolidasi perhitungan finansial, penyelarasan status invoice otomatis)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 4 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah 5 → 1 + logika bisnis sedang → 1 + kontrak API tidak ada → 0 + database hanya perilaku persistence yang sudah ada → 1 + keamanan tidak ada → 0 + UI tidak ada → 0)
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED: `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs`, `BilSettlement.cs`, `BilArHandoff.cs`; `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilSettlementConfiguration.cs`; `Services/BillingFinalizationService.cs`, `BillingFinancialExceptionService.cs`, `BillingSettlementService.cs`, `BillingAllocationService.cs`, `BillingArApHandoffService.cs`; `BillingManagementServiceCollectionExtensions.cs`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- FILES CHANGED:
  - **Baru**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs`
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs`
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs`
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
- IMPLEMENTATION:
  1. **`BillingInvoiceClosureService` (baru, `BE-BKC-060`, `BKC-DES-028`)** — satu tempat untuk dua tanggung jawab yang sebelumnya terpisah:
     - Dua overload `CalculateOutstandingAsync` (perhitungan sisa tagihan pasien), dipindahkan **apa adanya** dari dua salinan identik yang sebelumnya ada di `BillingFinalizationService` dan `BillingFinancialExceptionService` (dikonfirmasi identik lewat `01-existing-capability-map.md` § 21). Rumusnya sendiri **tidak diubah satu suku pun**.
     - `SyncClosureAsync(invoiceId, actorUserId, occurredAt, ct)` — method baru yang menyelaraskan status `FINAL`↔`CLOSED` berdasarkan sisa tagihan: `FINAL` dengan sisa 0 → `CLOSED` (`ClosedAt` diisi `occurredAt`); `CLOSED` dengan sisa >0 → kembali `FINAL` (`ClosedAt` dikosongkan, `BKC-DES-031`). Status `OPEN`/`SETTLED_BY_WRITE_OFF` tidak disentuh. Tidak membuka transaction sendiri, tidak memanggil `SaveChangesAsync` sendiri (`BKC-DES-030`) — pemanggil yang memegang keduanya. Mengambil kunci penasihat `BIL_INVOICE_LEDGER_{invoiceId:N}` sendiri (`BKC-DES-032`), dipanggil **sesudah** kunci event-specific milik pemanggil.
  2. **`BillingFinalizationService.cs`** — salinan privat `CalculateOutstandingAsync` dihapus, diganti pemanggilan `_closureService.CalculateOutstandingAsync(...)`. **Titik ketujuh (`BKC-DES-036`, temuan baru — lihat di bawah)**: `FinalizeAsync` kini juga memanggil `SyncClosureAsync` tepat sesudah `invoice.Status = Final` di-`SaveChanges`, dalam transaksi yang sama, sebelum commit.
  3. **`BillingFinancialExceptionService.cs`** — salinan privat `CalculateOutstandingAsync` diganti menjadi pembungkus tipis yang mendelegasikan ke `BillingInvoiceClosureService` lalu menangkap `BillingInvoiceClosureValidationException` dan membungkusnya ulang sebagai `BillingFinancialExceptionValidationException` — pesan galat yang sampai ke layar (`"Invoice belum memiliki hasil perhitungan terkini."`) **tetap identik**. Kedua titik pemanggil yang sudah ada (`CreateWriteOffAsync`, `ApproveWriteOffAsync`) **tidak berubah sama sekali** — perubahan sepenuhnya tersembunyi di balik method yang sama.
  4. **`BillingSettlementService.cs` (`BE-BKC-061`, `BKC-DES-029`)** — `ReconcileTenderAsync` memanggil `SyncClosureAsync` tepat sesudah `SaveChangesAsync` pertama (supaya perhitungan `AsNoTracking` melihat alokasi yang baru ditulis) dan sebelum `CommitAsync`, **hanya** bila `tender.Settlement.InvoiceId.HasValue` (settlement `DEPOSIT_TOP_UP` tidak punya invoice untuk diselaraskan). Bila status berubah, `SaveChangesAsync` dipanggil sekali lagi, lalu audit ditulis sesudah commit.
  5. **Audit** — satu event baru `BillingInvoice.ClosureSynced` (kategori `HealthServices.BillingManagement.Billing`), ditulis di kedua file pemanggil (`BillingFinalizationService`, `BillingSettlementService`) sesudah commit, hanya bila status benar-benar berubah. Payload: `InvoiceId`, status sebelum/sesudah, sisa tagihan terhitung, `Trigger` (`Finalization`/`TenderReconciled`), `ActorUserId`.
  6. **`BillingManagementServiceCollectionExtensions.cs`** — satu baris `services.AddScoped<BillingInvoiceClosureService>();`.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE` ([MODULE_OWNERSHIP_PREFIX_REGISTRY.md](../../../../engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md) baris 20)
  - Keberlakuan: **`TOUCHED LEGACY`** untuk empat berkas service/DI yang dimodifikasi; **`NEW CODE`** untuk `BillingInvoiceClosureService.cs` — **bukan** entity persisted, sehingga `QBE-MOD-002`/`QBE-MOD-003` (registrasi modul sebelum model dibuat) **tidak berlaku** untuk task ini. QBE ID yang berlaku: `QBE-SVC-001` (Module Service, dipatuhi — dipanggil service lain, bukan diakses controller langsung), `QBE-NAM-002` (prefix `Bil` sudah terdaftar, dipakai apa adanya).
  - Tidak ada model, migration, maupun perubahan skema pada task ini.
- **Temuan di luar scope roadmap tertulis, ditutup sebagai bagian task ini atas persetujuan eksplisit pengguna**: saat memasang titik pemanggilan `BE-BKC-061`, ditemukan `BillingSettlementService.TryAutoFinalizeInvoiceAsync` (mekanisme existing, tidak dibuat task ini) yang memanggil `BillingFinalizationService.FinalizeAsync` segera sesudah tender lunas pada invoice `OPEN`. Untuk jalur bukan-departure-exception, gerbang finalisasi (`FinalizeAsync` baris 99) **membuktikan** sisa tagihan sudah nol tepat pada momen invoice menjadi `FINAL` — tetapi `BKC-DES-029` (enam peristiwa yang disetujui) tidak menyebut finalisasi sebagai pemicu `SyncClosureAsync`. Tanpa penambahan, skenario **paling umum** (bayar lunas sekali bayar pada invoice `OPEN` → auto-finalize) akan tetap macet di `FINAL` selamanya — mereproduksi persis bug yang sedang diperbaiki. Pengguna diberi tahu lewat `AskUserQuestion` dan memilih menutupnya sekarang. Dicatat sebagai **`BKC-DES-036`** pada `00-interview-decisions.md` dan `02-backend-architecture.md`; **belum** ditambahkan ke `contracts/state-transition-matrix.md`/`testing/acceptance-test-matrix.md` sebagai baris tersendiri — lihat KNOWN ISSUES.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (task ini `BACKEND MODE`, bukan `MODULE BLUEPRINT MODE`; pembaruan blueprint pada sesi yang sama dilakukan lewat skill `design-business-module`/`grill-me`/`plan-module-delivery` terpisah, sudah tercatat di `blueprint-manifest.md` revisi 1.3)
- API CONTRACT IMPACT: **Nol perubahan bentuk.** Dua field yang sudah ada pada response invoice (`status`, `closedAt`) kini benar-benar terisi nilai baru pada alur normal — `status` dapat bernilai `CLOSED`, `closedAt` tidak lagi selalu kosong. Konsisten dengan `contracts/api-contract.md` amendment 18 September 2026.
- DATABASE IMPACT: **Nol perubahan skema.** Tidak ada tabel, kolom, index, maupun migration baru. Kolom `BilInvoice.Status`/`ClosedAt` yang sudah ada sejak baseline kini benar-benar ditulis oleh alur normal.
- SECURITY IMPACT: **Nol perubahan hak akses.** Perpindahan status dijalankan sebagai efek sistem di dalam transaksi peristiwa yang sudah diperiksa otorisasinya di pintu masuknya masing-masing (rekonsiliasi tender, finalisasi). Tidak ada endpoint baru, tidak ada `[AccessPermission]` baru.
- VISUAL REFERENCE: NOT REQUIRED (backend murni, tidak ada perubahan UI)
- VALIDATION:
  | Command/check | Result | Classification | Evidence/note |
  | --- | --- | --- | --- |
  | `dotnet build QuilvianSystemBackend.csproj` | **Belum dijalankan** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna berulang pada sesi ini: "jangan lakukan build secara automatis". Satu percobaan build sempat dijalankan tanpa sepengetahuan sebelumnya dan **dihentikan** (`TaskStop`) begitu diingatkan — hasilnya tidak sempat diketahui dan tidak dipakai sebagai bukti apa pun di laporan ini |
  | Review diff dan scope | Dilakukan | Manual | Seluruh titik pemanggilan diverifikasi silang terhadap pembacaan langsung source (`TryAutoFinalizeInvoiceAsync`, urutan kunci penasihat, konstraint `BilSettlement.InvoiceId` XOR `DepositAccountId`) sebelum ditulis |
  | Verifikasi kesetaraan rumus (`BIL-AT-134`(b)) | **Belum dijalankan** | NOT VERIFIED | Menuntut data invoice nyata (sekurang-kurangnya 10 invoice representatif) dan lingkungan database — di luar kemampuan sesi ini tanpa build/run aplikasi |
- WARNINGS: Perpindahan status kini terjadi pada **tujuh** titik kode (enam peristiwa `BKC-DES-029` + satu titik finalisasi `BKC-DES-036`), tersebar di tiga service. Task berikutnya (`BE-BKC-062`) menambah empat titik lagi pada `BillingFinancialExceptionService`/`BillingAllocationService` — total sembilan titik pemanggilan `SyncClosureAsync`/`CalculateOutstandingAsync` di seluruh modul sesudah `BE-BKC-062`.
- KNOWN ISSUES:
  1. **Belum dibangun/dijalankan sama sekali** — nol bukti compile-time maupun runtime pada laporan ini. Risiko realistis: kesalahan penamaan property, signature overload yang tidak cocok, atau ketidakcocokan tipe yang hanya terlihat saat `dotnet build`.
  2. `BKC-DES-036` (titik ketujuh) belum dituliskan sebagai baris tersendiri pada `contracts/state-transition-matrix.md` (masih tercakup implisit dalam narasi kontrak "FINAL, syarat sisa tagihan nol → CLOSED", tanpa penyebutan eksplisit bahwa finalisasi sendiri adalah salah satu momen pemicunya) maupun sebagai acceptance test bernomor tersendiri di `testing/acceptance-test-matrix.md` (skenario auto-finalize-lalu-lunas paling dekat dengan `BIL-AT-121` tapi tidak identik — `BIL-AT-121` mengasumsikan invoice **sudah** `FINAL` sebelum pembayaran, bukan baru menjadi `FINAL` lewat auto-finalize akibat pembayaran yang sama). **MUST** ditutup sebagai amendment kecil terpisah pada `/design-business-module` sebelum modul ini dianggap tuntas didesain, supaya blueprint tidak diam-diam lebih sempit dari source.
  3. `BE-BKC-062` (penyelarasan pada jalur deposit dan pengecualian finansial) **belum dikerjakan** — invoice yang lunas murni lewat alokasi deposit atau penyesuaian `Credit` masih akan macet di `FINAL` sampai task itu selesai.
  4. `BE-BKC-063` (perluasan penjaga koreksi AR, satu baris di `BillingArApHandoffService.cs:150`) **belum dikerjakan** — lubang koreksi AR yang menjadi motivasi utama `BKC-DEC-101` masih terbuka untuk invoice yang baru menjadi `CLOSED` lewat task ini.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE (menuntut environment ter-autentikasi dan database berjalan; di luar kemampuan sesi ini)
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: Sesi sempat menjalankan `dotnet build` tanpa otorisasi eksplisit di tengah `BE-BKC-060`, bertentangan dengan instruksi pengguna yang berlaku sepanjang sesi ini. Dihentikan (`TaskStop`) segera setelah pengguna menegur, sebelum hasilnya diketahui. Pekerjaan source yang sudah ditulis **tidak** dibatalkan — hanya proses build-nya yang dihentikan. Tidak ada pekerjaan yang hilang.
- GIT STATUS:
  ```text
   M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs
   M Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs
  ?? Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs
  ```
  (di luar itu, hanya perubahan `docs/module-blueprints/billing-kasir/**` dari pass desain/perencanaan pada sesi yang sama — lihat riwayat blueprint terpisah)
- NEXT RECOMMENDED STEP: (1) **Pengguna menjalankan `dotnet build` secara manual** dan melaporkan hasilnya — tidak ada langkah lain yang aman dilanjutkan sebelum ini, karena empat berkas saling bergantung dan belum pernah dikompilasi bersama. (2) Setelah build hijau, lanjutkan `BE-BKC-062` (jalur deposit dan pengecualian finansial) dan `BE-BKC-063` (perluasan penjaga koreksi AR) — keduanya independen satu sama lain. (3) Tutup `KNOWN ISSUES` butir 2 (`BKC-DES-036` belum tertulis di kontrak/acceptance test) sebelum modul ditandai `DESIGN_APPROVED` penuh untuk revisi `1.3`.
