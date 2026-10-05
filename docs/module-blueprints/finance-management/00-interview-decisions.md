# Finance Management — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `FIN-BP-001` |
| Revision | `1` |
| Status | `approved` untuk 90 keputusan (`FIN-DEC-001`..`093`) — seluruh blocker Phase 0, aturan inti AR/AP/Cash Management, UI brief frontend `FE-FIN-*` (`FIN-DEC-024`..`029`), dampak balasan Accounting (`FIN-DEC-030`..`039`), koreksi hasil impact scan `/design-business-module` atas rumpun uang muka/deposit/selisih kas (`FIN-DEC-040`..`044`), rumpun BARU Purchasing/AP penuh + agregasi AR + potongan sisi penerimaan dipicu evidence `Keuangan.md` (`FIN-DEC-045`..`051`), closure pass lanjutan yang menutup ambang nominal AP + kode PPN + status pajak AR (`FIN-DEC-052`..`055`), penyempitan gerbang `/plan-module-delivery` untuk `EPIC FIN-15` (`FIN-DEC-056`), `/grill-me` closure pass yang menutup empat pertanyaan `/plan-module-delivery` (`FIN-DEC-057`..`060`, 25 September 2026), `/grill-me` closure pass atas balasan Accounting `evidence/14` (`FIN-DEC-063`..`071`, 28 September 2026), `/grill-me` amendment pass atas temuan `/trace-existing-capabilities` REVISI 6 (`FIN-DEC-072`..`076`, 28 September 2026), penutupan `FIN-OQ-034` via `BKC-DEC-128`..`131` (`FIN-DEC-077`), `/grill-me` amendment pass penyelarasan resource hak akses (`FIN-DEC-078`..`079`, 28 September 2026), ratifikasi pemicu pembalikan uang muka deposit `FIN-DES-064`..`065` (`FIN-DEC-080`..`081`, 29 September 2026), keputusan pembawa resource payung (`FIN-DEC-082`..`084`, 29 September 2026), `/grill-me` closure pass tindak lanjut gap evidence/14, skrip migrasi database DBeaver, dan arsitektur in-process worker pengiriman outbox EPIC FIN-12 (`FIN-DEC-085`..`089`, 30 September 2026), serta `/grill-me` closure pass tindak lanjut susulan Accounting evidence/15: aturan saldo subledger dan nomor jurnal tanda terima (`FIN-DEC-090`..`093`, 30 September 2026). |
| Pass | `Scope pass` — selesai 20 September 2026 · `Closure pass` — selesai 20 September 2026 · `Amendment pass` (UI brief `FE-FIN-*`) — selesai 23 September 2026 · `Amendment pass` (balasan Accounting, evidence 13) — selesai 25 September 2026 · `Amendment pass lanjutan` (koreksi impact scan `/design-business-module`) — selesai 25 September 2026 · `Amendment pass` (rumpun Purchasing/AP penuh + agregasi AR, evidence `Keuangan.md`) — selesai 25 September 2026 · `Closure pass lanjutan` (ambang nominal, kode PPN, status pajak AR) — selesai 25 September 2026 · `Closure pass` (balasan Accounting `evidence/14`) — selesai 28 September 2026 · `Amendment pass` (temuan `/trace-existing-capabilities` atas REVISI 6) — selesai 28 September 2026 · `Amendment pass` (penyelarasan resource hak akses, `FIN-CQ-08`) — selesai 28 September 2026 · `Closure pass lanjutan` (gap evidence/14, skrip migrasi DBeaver, in-process worker EPIC FIN-12) — selesai 30 September 2026 · `Closure pass lanjutan 2` (susulan Accounting evidence/15: aturan saldo subledger & tanda terima) — selesai 30 September 2026 |
| Product/domain owner | Yasmin (owner/penggarap modul Finance AR/AP, sesuai `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md`) |
| Backend SHA | `09101d05` (branch `Yasmina`, `NewQuilvianSystemBackend`) |
| Frontend SHA | `abed49b03` (branch, `QuilvianSystemFrontendDev`) |
| Masukan | Empat dokumen analisis milik owner: `FIN-BRD-V2-0.3`, `FIN-PRD-V2-0.3`, `FIN-MVP-PRD-V2-0.3`, `FIN-ACC-XMOD-V2-0.3` — seluruhnya bertanggal analisis 18 September 2026 |
| Tanggal mulai | 20 September 2026 |

## Catatan cara kerja

Wawancara ini menjawab pertanyaan **"aturan bisnisnya bagaimana"**, bukan "apa yang sudah ada
di sistem".

**Scope dikunci tanpa audit kemampuan existing formal.** Belum ada
`01-existing-capability-map.md` untuk modul ini. Empat dokumen masukan sudah mengutip file
sumber konkret (`BilArHandoff.cs`, `BilApHandoff.cs`, `BilTender.cs`, `BilCashierShift.cs`,
`FinPettyCashBudget.cs`, `FinPettyCashBudgetMovement.cs`, `MstPettyCashCategory.cs`, model-model
`Acc*` Accounting, dsb.) sehingga bukan tebakan kosong, tetapi ini belum setara audit canonical
`/trace-existing-capabilities`. Risiko yang belum diperiksa: field persis apa yang benar-benar
ada hari ini pada `BilArHandoff`/`BilApHandoff` (mis. apakah versi/idempotency key persis seperti
diasumsikan dokumen), dan endpoint Petty Cash mana yang benar-benar sudah live vs masih
kompatibilitas alias.

Rekomendasi pada setiap pertanyaan **bukan keputusan dan bukan persetujuan**. Owner yang
berwenang tetap harus memilih. Setiap pertanyaan selalu punya opsi tambahan
`Other — tuliskan pilihan atau batasan lain`.

---

## Scope dan Outcome

**Modul:** Finance Management (`finance-management`)

**Satu kalimat batas scope:** Finance Management mengelola subledger operasional keuangan
rumah sakit — piutang (AR), utang (AP), penerimaan kasir, dan kas kecil — sebagai konsumen
fakta dari Billing dan Medical Fee, serta penerbit kejadian akuntansi ke Accounting, tanpa
pernah memiliki COA, jurnal, atau periode akuntansi.

**Konfirmasi scope:** disetujui apa adanya oleh owner pada 20 September 2026 (lihat
`FIN-DEC-000` di Decision Log).

### Di dalam scope

| ID | Kemampuan | Keterangan |
|---|---|---|
| `FIN-SC-001` | Billing Handoff Inbox | Konsumsi `BilArHandoff`/`BilApHandoff` secara idempoten |
| `FIN-SC-002` | Account Receivable | Receivable, aging, settlement, write-off, adjustment, employee benefit |
| `FIN-SC-003` | Collection / Receipt | `FinReceipt` dari tender Billing sukses, alokasi, split paid-vs-AR |
| `FIN-SC-004` | Account Payable | Supplier payable, doctor payable (konsumen dari fee yang sudah approved), payment |
| `FIN-SC-005` | Cash Management | Petty Cash (lengkapi FE di atas baseline backend existing), Bank Deposit, Daily Cash, rekonsiliasi shift kasir (read-only) |
| `FIN-SC-006` | Finance-owned master data | Bank/BankAccount, Petty Cash Category (sudah ada) |
| `FIN-SC-007` | Accounting Integration Outbox | `FinAccountingEventOutbox`, monitoring/retry — bukan endpoint penerima Accounting |
| `FIN-SC-008` | **BARU 25 September 2026.** Purchasing/AP siklus penuh | Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice (PPN/DP/potongan), Retur Pembelian & Deposit Retur, Aging AP, Rekap Purchasing AP, Laporan Tukar Faktur, Laporan Jatuh Tempo, Rekonsiliasi Tagihan — **dibangun di dalam Finance Management** (`FIN-DEC-045`), menggantikan premis `FIN-DEC-015` |
| `FIN-SC-009` | **BARU 25 September 2026.** AR Invoice Agregat ke Company/Guarantor | Dokumen tagihan resmi bernomor tunggal yang menggabungkan banyak `BilInvoice` pasien untuk satu penjamin per periode (`FIN-DEC-048`) |
| `FIN-SC-010` | **BARU 25 September 2026.** Potongan sisi penerimaan piutang | PPh 23 dan biaya admin bank yang mengurangi uang yang benar-benar diterima dari penjamin (`FIN-DEC-049`) — berdiri sendiri dari `FinPaymentDeduction` sisi Payable |

### Di luar scope — untuk modul lain

| ID | Kemampuan | Pemilik | Titik sentuh yang tetap dibahas |
|---|---|---|---|
| `FIN-OOS-001` | Kalkulasi invoice, `BilSettlement`/`BilTender`/`BilPaymentAllocation`, `BilCashierShift` | Billing/Kasir | Hanya bentuk kontrak handoff & receipt yang diterima Finance |
| `FIN-OOS-002` | COA, `AccEventType`, `AccPostingRule`, `AccAccountingPeriod`, Journal/GL | Accounting | Hanya bentuk kejadian yang diterbitkan Finance |
| `FIN-OOS-003` | `MstDoctorServiceRule`, kalkulasi/verifikasi/approval `DoctorServiceFee` | Medical Fee | Hanya titik terima fee yang sudah approved |
| `FIN-OOS-004` | Legal Entity, Cost Center | Corporate/HR | Hanya rujukan |

Dua ownership yang PRD tandai "to confirm" — **Supplier master** dan **Currency/Exchange
rate** — belum diputuskan; dicatat sebagai `FIN-OQ-002` dan `FIN-OQ-003`.

## Aktor dan Tanggung Jawab

Sumber: `FIN-PRD-V2-0.3` bagian 4 (Fact, belum digali ulang secara kritis pada pass ini).

| Peran | Tindakan utama |
|---|---|
| Finance AR Staff | Konsumsi handoff AR, verifikasi receivable, kelola klaim, alokasi penerimaan, settlement |
| Finance AP Staff | Validasi payable supplier/dokter, siapkan pembayaran, kelola adjustment |
| Finance Supervisor | Menyetujui write-off/adjustment/pembayaran sesuai kebijakan; memonitor rekonsiliasi |
| Medical Fee Admin | Menghitung, memverifikasi, dan menyesuaikan fee dokter sebelum approval (di luar scope modul ini) |
| Cashier/Treasury | Mencatat penerimaan, petty cash, setoran bank, posisi kas |
| Accounting Staff/Approver | Memelihara master/posting rule Accounting dan meninjau jurnal hasil; tidak mengubah subledger Finance |
| Auditor/Manager | Trace dan laporan read-only lintas source → Finance → Accounting |

**Belum digali:** siapa persisnya yang berwenang approve write-off/adjustment per nilai ambang
(mis. batas nominal yang butuh approval berjenjang), dan apakah role di atas sudah final atau
masih berubah. Dicatat sebagai bagian `FIN-OQ-004`.

## Business Rules dan Invariants

Baris bertipe **Fact** berasal dari `FIN-BRD-V2-0.3` bagian 10 (15 aturan bisnis yang sudah
dirumuskan owner sendiri sebelum sesi ini). Baris bertipe **Decision** adalah hasil ratifikasi
eksplisit pada sesi wawancara ini.

| # | Tipe | Aturan |
|---|---|---|
| 1 | Fact | Sumber Billing yang sudah final hanya boleh membuat Finance AR/AP tepat satu kali per handoff/version. |
| 2 | Fact | Perubahan Finance setelah finalisasi memakai record adjustment/write-off; jumlah sumber asli tidak pernah ditimpa diam-diam. |
| 3 | Fact | Alokasi pembayaran boleh parsial dan many-to-many bila bisnis membutuhkan. |
| 4 | Fact | Transaksi Finance tidak dianggap sudah terposting ke Accounting hanya karena sudah ada di Finance. |
| 5 | Decision (`FIN-DEC-001`) | Publikasi ke Accounting wajib idempotent dan tertelusur lewat `EventNumber`, `SourceTransactionId`, `SourceVersion`, `CorrelationId`, `CausationId` — kontrak `ACC-XMOD-0.2` diratifikasi apa adanya. |
| 6 | Fact | Identifier pasien tidak pernah dikirim dalam pesan Finance → Accounting. |
| 7 | Fact | Untuk kontrak Accounting saat ini, hanya IDR yang boleh dikirim. |
| 8 | Fact | Reversal bersifat kompensasi: fakta asli (tender/receipt/alokasi) tetap tertelusur, fakta reversal baru mengoreksi saldo. |
| 9 | Fact | `BilCashierShift.SystemCash` milik Billing. Finance membaca/merekonsiliasi, tidak pernah menghitung ulang atau menulis saldo shift kasir. |
| 10 | Decision (`FIN-DEC-004`) | Bila tender sukses terjadi sementara invoice sumber belum FINAL, Finance tetap mencatat `FinReceipt` secara operasional, tetapi kejadian Accounting-nya berstatus `HELD_FOR_FINALIZATION` sampai invoice final atau kebijakan pengakuan disetujui. |
| 11 | Fact | Jumlah yang sudah dibayar pasien tidak boleh dibuat ulang sebagai Finance AR. AR hanya dibuat untuk kewajiban yang masih outstanding sesuai handoff Billing yang disetujui. |
| 12 | Fact | Setiap tender Billing sukses yang masuk scope Finance membuat maksimal satu `FinReceipt`; replay dari `TenderId`/provider event/reversal source harus aman (tidak dobel). |
| 13 | Decision (`FIN-DEC-003`) | Titik pengakuan fee dokter memakai Opsi B: event AR hanya memposting AR/pendapatan; `PENGAKUAN-HUTANG-DOKTER` baru terbit setelah `DoctorServiceFee` disetujui — mencegah pemostingan ganda komponen `JASA_MEDIS`. |
| 14 | Decision (`FIN-DEC-006`) | `BilArHandoff` diperluas dengan `DebtorType` baru `EMPLOYEE_BENEFIT` beserta field pemilik manfaat (`BenefitOwnerId`) dan relasi (`SELF`/`SPOUSE`/`CHILD`/dst), bukan tabel handoff terpisah. |
| 15 | Fact | Uang kas kasir pasien dan Petty Cash adalah dua pool terpisah; disbursement Petty Cash tidak boleh mengubah `BilCashierShift.SystemCash`/`PhysicalCash`, dan penerimaan kasir tidak boleh mengubah `FinPettyCashBudget.CurrentBalance`. |

## Skenario Normal dan Exception

Belum digali secara mendalam pada pass ini — baru tercakup sepanjang yang dibutuhkan untuk
menutup klaster lintas-modul (lihat `FIN-DEC-004` untuk skenario "bayar sebelum invoice
final", dan `FIN-DEC-003` untuk skenario fee dokter). Skenario normal/pembatalan/koreksi untuk
AR, AP, dan Cash Management secara rinci ada di `FIN-OQ-004`.

## Frontend Decision Authority

Ditutup pada Amendment pass 23 September 2026, menjawab enam keputusan yang diwajibkan
`docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 5 sebelum
task `FE-FIN-*` mana pun boleh dimulai. Owner menjawab sebagai Product Owner Finance (Yasmin)
pada sesi wawancara ini — lihat `FIN-DEC-024`..`029` untuk detail dan bukti tiap keputusan.

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `FIN-DEC-024` | Struktur rute `/finance/...` | Yasmin (Product Owner Finance) | `approved` | Master data baru mengikuti `/finance/master-data/<entity>` (mis. `bank-account`, `currency`); kapabilitas lain masing-masing slug flat langsung di bawah `/finance/` (mis. `/finance/receivable`, `/finance/cash-management`, `/finance/monitoring`) | Pola existing `/finance/master-data/petty-cash-category` dan `/finance/petty-cash-budget`, `QuilvianSystemFrontendDev` |
| `FIN-DEC-025` | Penamaan & pengelompokan menu `corporateFinance` | Yasmin (Product Owner Finance) | `approved` | Master data baru ("Rekening Bank", "Mata Uang & Kurs") masuk submenu Master Data yang sudah ada; kapabilitas lain jadi butir flat sejajar Anggaran Petty Cash ("Piutang", "Setoran & Kas Harian", "Pemantauan Finance"). Penerimaan (`FE-FIN-004`) belum ditambahkan — masih `BLOCKED`. Urutan butir di dalam group tetap `DEV_DISCRETION` (roadmap bagian 6) | `src/utils/menu-sidebar/menu-items.jsx` baris 664-687, `QuilvianSystemFrontendDev` |
| `FIN-DEC-026` | Bentuk penyajian umur piutang (`FE-FIN-002`) | Yasmin (Product Owner Finance) | `approved` | Summary card berisi 4 angka kelompok umur (0-30, 31-60, 61-90, >90 hari — dikunci `FIN-DEC-010`) di atas satu tabel daftar piutang; klik baris untuk menelusuri ke tagihan asal. Bukan empat tab terpisah | Pola summary card + table pada halaman Petty Cash Budget/Category |
| `FIN-DEC-027` | Layout rekonsiliasi shift kasir (bagian `FE-FIN-004`, saat ini `BLOCKED` menunggu Owner Billing — hanya tata letaknya yang diputuskan di sini) | Yasmin (Product Owner Finance) | `approved` | Halaman penuh terpisah, bukan tab di dalam halaman Penerimaan | Preseden `/finance/petty-cash-budget` dan `FE-FIN-003` (Setoran Bank & Kas Harian) sebagai halaman berdiri sendiri |
| `FIN-DEC-028` | Bentuk koreksi & penghapusan piutang/utang (write-off, adjustment, reversal) | Yasmin (Product Owner Finance) | `approved` | Modal, bukan halaman penuh terpisah | `create-adjustment-modal.jsx`, `reverse-exception-modal.jsx` (Billing Invoices), `journal-reversal-dialog.jsx` (Accounting), `adjust-budget-modal.jsx` (Petty Cash) — seluruhnya `QuilvianSystemFrontendDev` |
| `FIN-DEC-029` | Cetak dan unduh | Yasmin (Product Owner Finance) | `approved` (ditunda dari MVP) | Tidak termasuk scope `FE-FIN-001`/`002`/`003`/`006`. Layar dibangun tanpa tombol cetak/unduh. Lingkup detail (dokumen apa, format apa) menunggu keputusan terpisah — dicatat `FIN-OQ-015` | Tidak ada UAT/acceptance criteria `FE-FIN-*` yang menyebut cetak/ekspor; hampir tidak ada preseden export di modul Finance/Billing manapun saat ini |

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `FIN-DEC-000` | Decision | Batas scope modul Finance Management (`FIN-SC-001`..`007` di dalam scope; `FIN-OOS-001`..`004` di luar scope) dikonfirmasi apa adanya. | Yasmin | `approved` | Yasmin, 20 September 2026 | Sesi wawancara ini; `FIN-BRD-V2-0.3` bagian 4 |
| `FIN-DEC-001` | Decision | Finance meratifikasi kontrak `ACC-XMOD-0.2`: arah satu arah Billing→Finance→Accounting, Finance sebagai satu-satunya penerbit kejadian, bentuk pesan 12 field wajib diterima apa adanya. | Yasmin (Finance) — menjawab pertanyaan #1 dan #2 paket Rizki | `approved` | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 3, 8; `FIN-ACC-XMOD-V2-0.3` bagian 4 |
| `FIN-DEC-002` | Decision | Katalog 17 `EventTypeCode` yang diusulkan di `FIN-ACC-XMOD-V2-0.3` bagian 6-7 (PENGAKUAN-PIUTANG s.d. PEMBALIKAN-PENERIMAAN-KASIR) diajukan sekaligus ke Accounting sebagai starting point, bukan bertahap per fase. | Yasmin (Finance) — menjawab pertanyaan #3 paket Rizki; **butuh ratifikasi bersama Accounting**, belum final dari sisi Accounting | `approved` (sisi Finance) | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 8 poin 3; `FIN-ACC-XMOD-V2-0.3` bagian 6-7 |
| `FIN-DEC-003` | Decision | Titik pengakuan fee dokter = Opsi B: event AR hanya AR/pendapatan; `PENGAKUAN-HUTANG-DOKTER` terbit terpisah setelah `DoctorServiceFee` disetujui. | Yasmin (Finance) + Medical Fee + Accounting (titik sentuh) | `approved` (sisi Finance) | Yasmin, 20 September 2026 | `FIN-ACC-XMOD-V2-0.3` bagian 7; `FIN-BRD-V2-0.3` bagian 12 |
| `FIN-DEC-004` | Decision | ~~Kebijakan publikasi pra-finalisasi = `HELD_FOR_FINALIZATION`: `FinReceipt` tercatat penuh di Finance saat diterima, kejadian Accounting ditahan sampai invoice FINAL/kebijakan pengakuan disetujui.~~ **`superseded` oleh `FIN-DEC-030`, 25 September 2026.** | Yasmin (Finance) | `superseded` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-BIL-011; `FIN-MVP-PRD-V2-0.3` FIN-COL-05 |
| `FIN-DEC-005` | Decision | Bentuk kontrak Billing collection→Finance receipt yang diminta Finance ke Billing = tabel handoff persisted `BilCollectionHandoff`, pola sama dengan `BilArHandoff`/`BilApHandoff`, dikunci `TenderId`. | Yasmin (Finance) — **titik sentuh, butuh konfirmasi owner Billing** | `approved` (sisi Finance, permintaan ke Billing) | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` bagian 5.1; `FIN-BRD-V2-0.3` bagian 5.2 |
| `FIN-DEC-006` | Decision | `BilArHandoff` diperluas dengan `DebtorType EMPLOYEE_BENEFIT` + field `BenefitOwnerId`/relasi, bukan handoff terpisah. | Yasmin (Finance) — **titik sentuh, butuh konfirmasi Billing + HR** | `approved` (sisi Finance, permintaan ke Billing/HR) | Yasmin, 20 September 2026 | `FIN-BRD-V2-0.3` bagian 6.3; `FIN-BRD-V2-0.3` bagian 12 (Open Decision "Employee-benefit handoff fields") |
| `FIN-DEC-007` | Decision | ~~Preferensi Finance untuk autentikasi pengiriman kejadian ke Accounting = service account/API key internal lewat mekanisme auth existing (bukan skema OAuth baru).~~ **Bagian syarat organisasi `superseded` oleh `FIN-DEC-036`, 25 September 2026; mekanisme token tetap `draft`, kini dilacak `FIN-OQ-016`.** | Yasmin (Finance) — menjawab pertanyaan #4 paket Rizki; **keputusan final bersama Accounting + Platform** | `superseded` (sebagian) | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 8 poin 4; `FIN-BRD-V2-0.3` bagian 12 |
| `FIN-DEC-008` | Decision | Cutover kejadian Finance→Accounting mulai berlaku sejak tanggal go-live integrasi (Phase 5) diaktifkan, tidak retroaktif; saldo AR/AP/kas sebelum tanggal itu diinput Accounting sebagai saldo awal (opening balance) manual satu kali. **Dikonfirmasi tetap berlaku apa adanya, `FIN-DEC-037`, 25 September 2026 — referensi tanggal "1 Oktober 2026" pada versi percakapan lain dinyatakan TIDAK berlaku.** | Yasmin (Finance) — menjawab pertanyaan #6 paket Rizki | `approved` (tanggal pasti mengikuti 6 gerbang Accounting, lihat `FIN-DEC-037`) | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 8 poin 6 |
| `FIN-DEC-009` | Decision (`FIN-CQ-01`) | Pencatatan keputusan Petty Cash dipisah berdasarkan jenis: aturan OPERASIONAL Petty Cash sendiri (siklus voucher, status budget, jenis movement) TETAP di blueprint `billing-kasir` (`PC-DEC-*`/`PC-DES-*`) dan TIDAK dibuka ulang di blueprint ini. Blueprint `finance-management` hanya mencatat keputusan baru yang memang milik batas Finance→Accounting (katalog event, pemetaan kategori-ke-akun — lihat `FIN-DEC-002`) dan pekerjaan FE parity di rute `/finance/`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` §9 `FIN-CQ-01`; `docs/module-blueprints/billing-kasir/blueprint-manifest.md` (`blueprint_shape: SINGLE`) |
| `FIN-DEC-010` | Decision | Bucket aging AR memakai 4 kelompok berbasis hari sejak `DueDate`: 0-30, 31-60, 61-90, dan lebih dari 90 hari. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-AR-007 |
| `FIN-DEC-011` | Decision | Alokasi pembayaran payer/asuransi/corporate yang diterima SETELAH AR tercatat dilakukan MANUAL PENUH — staf AR memilih sendiri item piutang mana yang dibayar pada setiap penerimaan, bukan pencocokan otomatis (FIFO). | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-AR-005 |
| `FIN-DEC-012` | Decision | Write-off dan adjustment AR memakai pola maker-checker: diajukan petugas Billing/AR, disetujui pengguna LAIN yang memegang wewenang approval Finance/AR (pengaju tidak boleh menyetujui permohonan sendiri). Wewenang approval ditentukan lewat konfigurasi permission/role, bukan nama jabatan yang di-hardcode. Tidak memakai ambang nominal bertingkat. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026 |
| `FIN-DEC-013` | Decision | AR untuk payer/asuransi diakui SEGERA saat handoff Billing (`BilArHandoff` tipe `PAYER`) diterima, terlepas dari kelengkapan dokumen klaim. Status kelengkapan dokumen klaim dilacak sebagai atribut terpisah (`FinReceivableDocument`) yang tidak pernah mengubah jumlah piutang asli. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-BRD-V2-0.3` bagian 6.1; `FIN-PRD-V2-0.3` FIN-AR-004 |
| `FIN-DEC-014` | Decision | Finance AP memakai master `MstSupplier` existing (`Areas/Administrator/MasterData/Models/MstSupplier.cs`, milik Administrator) apa adanya lewat `SupplierId`. Tidak membuat master Supplier baru di Finance. Menutup `FIN-OQ-002`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` temuan langsung `MstSupplier.cs` |
| `FIN-DEC-015` | Decision | ~~`FinSupplierPayable` untuk rilis pertama dipicu lewat INPUT MANUAL staf Finance AP (nomor invoice supplier, nominal, termin) — tidak menunggu modul Purchase Order/Receive Order/Supplier Invoice yang saat ini belum ada satu barisnya pun di backend.~~ **`superseded` oleh `FIN-DEC-045`, 25 September 2026 — owner memutuskan siklus Purchasing/AP penuh dibangun DI DALAM Finance Management, bukan ditunda menunggu modul terpisah yang tidak pernah dibangun.** | Yasmin (Finance) | `superseded` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` — konfirmasi langsung tidak ada modul Purchasing; `FIN-BRD-V2-0.3` bagian 7.1 |
| `FIN-DEC-016` | Decision | Identitas pemilik manfaat karyawan (`BenefitOwnerId`) ditentukan di sumber kunjungan (Registrasi/Billing) berdasarkan hubungan pasien-karyawan dan eligibilitas yang berlaku saat pelayanan. Finance TIDAK menentukan ulang identitas ini; ketidaksesuaian dikoreksi lewat workflow koreksi (bukan Finance mengubah sendiri). | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; konsisten dengan `FIN-DEC-006` |
| `FIN-DEC-017` | Decision | Nominal setoran Bank Deposit divalidasi otomatis terhadap saldo kas yang benar-benar masih tersedia untuk disetor (kas fisik dikurangi setoran sebelumnya dan koreksi/selisih yang sudah disahkan). Partial deposit diperbolehkan. Validasi WAJIB dijalankan backend saat posting untuk mencegah double allocation atau saldo menjadi minus. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; `FIN-PRD-V2-0.3` FIN-CASH-002 |
| `FIN-DEC-018` | Decision | Finance tetap memiliki master operasional `FinCurrency`/`FinExchangeRate` sendiri untuk mencatat transaksi non-IDR di level operasional, walau kontrak Accounting saat ini hanya menerima IDR. Menutup `FIN-OQ-003`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` bagian 10 |
| `FIN-DEC-019` | Decision | Doctor Payment BOLEH direkap periodik — satu `DoctorPayment` dapat melunasi banyak `DoctorPayable` sekaligus lewat `FinPaymentAllocation`, agar tiap payment tetap tertelusur balik ke fee-fee individual penyusunnya. Alasan owner: mencegah masalah sistem lama — dokter dobel/lupa dibayar karena rekonsiliasi manual lewat Excel terpisah dari sistem utama. Menutup `FIN-OQ-006`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026. **Diperkuat bukti lapangan** (`evidence/03-referensi-meeting-rs-mmc.md`): transcript "Akutansi Zoom Meeting" 15 Juli 2026 menunjukkan praktik RS MMC memang rekap bulanan per dokter lewat menu Ap Dokter, dengan volume ±220 dokter/bulan dan ±4 hari kerja karena approve satu-per-satu |
| `FIN-DEC-020` | Decision | Formula Daily Cash final apa adanya: opening + penerimaan kasir (`FinReceipt`) + penerimaan lain yang disetujui − disbursement − setoran bank = closing. Petty Cash top-up/disbursement TIDAK masuk formula ini (pool terpisah, konsisten dengan aturan bisnis #15). Menutup `FIN-OQ-007`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-CASH-003 |
| `FIN-DEC-021` | Decision | Bila `FinReceipt` yang sudah dialokasikan ke AR kemudian tender aslinya di-reverse Billing, sistem WAJIB membuat baris alokasi pembalik (reversal allocation, bukan menghapus histori) dan AR yang tadinya terbayar otomatis kembali berstatus `OUTSTANDING`. Menutup `FIN-OQ-008`. **PERINGATAN: sebagian keputusan ini adalah desain baru, bukan meniru pola yang sudah terbukti** — lihat kolom Evidence. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; konsisten aturan bisnis #8. **Bukti lapangan bersifat SEBAGIAN** (`evidence/03-referensi-meeting-rs-mmc.md`): transcript "Keuangan AP Zoom Meeting" 10 Juni 2026 memang menunjukkan pola reversal-bukan-delete dan tagihan otomatis dapat di-settle ulang — TETAPI hanya untuk skenario **full** cancel/settlement. Skenario **partial** (bayar sebagian + sisa jadi piutang) belum pernah ada di sistem lama, dan fitur Batal Settlement sendiri diakui tim RS MMC belum matang. Penanganan partial karena itu MUST diuji khusus, tidak boleh diasumsikan aman karena "sudah begitu di sistem lama" |
| `FIN-DEC-022` | Decision | Approval pembayaran AP (Supplier dan Doctor) TIDAK memakai pola maker-checker sederhana yang sama seperti AR (`FIN-DEC-012`). AP memakai approval berjenjang berdasarkan total nominal rekap pembayaran. Ambang nominal pastinya BELUM ditentukan — dicatat sebagai `FIN-OQ-010`, bukan diputuskan sekarang. Menutup `FIN-OQ-009` (bentuk aturan), membuka `FIN-OQ-010` (nilai ambang). | Yasmin (Finance) | `approved` (bentuk aturan); ambang nominal `open` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026. **Diperkuat bukti lapangan** (`evidence/03-referensi-meeting-rs-mmc.md`): di sistem lama TIDAK ADA pemisahan pengaju/penyetuju sama sekali untuk fee dokter — tiga orang tim akuntansi sama-sama membuat dan menyetujui, dan tim mengakui sendiri risikonya ("bisa saling membantu tanpa sadar dobel kerjakan dokter yang sama"). Pembayaran supplier bahkan tidak punya langkah approval terpisah. Jadi keputusan ini **menutup gap kontrol yang nyata**, bukan sekadar memilih pola berbeda |
| `FIN-DEC-023` | Decision | ~~Finance mengajukan draft nama field pesan saldo subledger per periode — `LegalEntityId`, `AccountingPeriodCode`, `ControlAccountCode`, `SubledgerBalance`, `AsOfDate` — 5 field berdiri sendiri.~~ **`superseded` oleh `FIN-DEC-035`, 25 September 2026 — Accounting menolak bentuk 5-field dan Finance menerimanya.** | Yasmin (Finance) — draft, **butuh konfirmasi Accounting** | `superseded` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; `FIN-ACC-XMOD-V2-0.3` bagian 12 |
| `FIN-DEC-024` | Decision | Struktur rute `/finance/...` untuk kapabilitas baru: master data mengikuti `/finance/master-data/<entity>` (mis. `bank-account`, `currency`), kapabilitas lain masing-masing slug flat langsung di bawah `/finance/` (mis. `receivable`, `cash-management`, `monitoring`), mengikuti pola existing apa adanya. Menjawab keputusan #1 UI brief `02-frontend-roadmap.md` bagian 5. | Yasmin (Product Owner Finance) | `approved` | Yasmin, 23 September 2026 | Pola existing `/finance/master-data/petty-cash-category`, `/finance/petty-cash-budget` — `QuilvianSystemFrontendDev` |
| `FIN-DEC-025` | Decision | Penamaan & pengelompokan menu `corporateFinance`: master data baru ("Rekening Bank", "Mata Uang & Kurs") masuk submenu Master Data yang sudah ada; kapabilitas lain jadi butir flat sejajar Anggaran Petty Cash ("Piutang", "Setoran & Kas Harian", "Pemantauan Finance"). Penerimaan (`FE-FIN-004`) belum ditambahkan karena masih `BLOCKED`. Urutan butir di dalam group tetap `DEV_DISCRETION`. Menjawab keputusan #2 UI brief. | Yasmin (Product Owner Finance) | `approved` | Yasmin, 23 September 2026 | `src/utils/menu-sidebar/menu-items.jsx` baris 664-687, `QuilvianSystemFrontendDev` |
| `FIN-DEC-026` | Decision | Bentuk penyajian umur piutang (`FE-FIN-002`): summary card berisi 4 angka kelompok umur (bucket dikunci `FIN-DEC-010`) di atas satu tabel daftar piutang, klik baris untuk menelusuri ke tagihan asal — bukan empat tab terpisah. Menjawab keputusan #3 UI brief. | Yasmin (Product Owner Finance) | `approved` | Yasmin, 23 September 2026 | Pola summary card + table pada halaman Petty Cash Budget/Category |
| `FIN-DEC-027` | Decision | Layout rekonsiliasi shift kasir (bagian `FE-FIN-004`): halaman penuh terpisah, bukan tab di dalam halaman Penerimaan. Hanya tata letak yang diputuskan di sini; aturan bisnis penerimaan/alokasi `FE-FIN-004` sendiri tetap `BLOCKED` menunggu Owner Billing. Menjawab keputusan #4 UI brief. | Yasmin (Product Owner Finance) | `approved` | Yasmin, 23 September 2026 | Preseden `/finance/petty-cash-budget` dan `FE-FIN-003` (Setoran Bank & Kas Harian) sebagai halaman berdiri sendiri |
| `FIN-DEC-028` | Decision | Bentuk koreksi & penghapusan piutang/utang (write-off, adjustment, reversal): modal, bukan halaman penuh terpisah. Menjawab keputusan #5 UI brief. | Yasmin (Product Owner Finance) | `approved` | Yasmin, 23 September 2026 | `create-adjustment-modal.jsx`, `reverse-exception-modal.jsx` (Billing Invoices), `journal-reversal-dialog.jsx` (Accounting), `adjust-budget-modal.jsx` (Petty Cash) — `QuilvianSystemFrontendDev` |
| `FIN-DEC-029` | Decision | Cetak dan unduh TIDAK termasuk scope MVP `FE-FIN-001`/`002`/`003`/`006` — tidak ada UAT/acceptance criteria yang mensyaratkannya dan hampir tidak ada preseden export di modul Finance/Billing manapun. Layar dibangun tanpa tombol cetak/unduh. Lingkup detail (dokumen apa, format apa) dicatat `FIN-OQ-015` untuk task terpisah kelak. Menjawab keputusan #6 UI brief. | Yasmin (Product Owner Finance) | `approved` (ditunda dari MVP) | Yasmin, 23 September 2026 | Roadmap `02-frontend-roadmap.md` bagian 5 poin 6 menyatakan "belum ada keputusan bisnisnya sama sekali"; grep `QuilvianSystemFrontendDev` menunjukkan nol preseden export di modul Finance dan hampir nol di Billing |
| `FIN-DEC-030` | Decision | **Menggantikan `FIN-DEC-004`.** Penerimaan (`FinReceipt`) sebelum invoice sumber `FINAL` TIDAK lagi ditahan `HELD_FOR_FINALIZATION`. Kejadian ke Accounting terbit SEGERA sebagai kode `PENERIMAAN-UANG-MUKA` (lihat `FIN-DEC-031`), dibukukan Accounting sebagai Uang Muka Pasien (kewajiban). Saat invoice final terbit, kode pemakaian (`FIN-DEC-032`) melunasi piutang dari uang muka tersebut. `HELD_FOR_FINALIZATION` dihapus dari jalur pra-finalisasi ini. **Berdampak pada kode yang sudah berjalan**: `FinanceReceiptService.cs`, `FinAccountingEventOutbox.cs`, `FinReceipt.cs`, `FinanceAccountingOutboxService.cs` (backend SHA `09101d05`) MUST direvisi mengikuti keputusan ini. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 5 butir 5, `ACC-DEC-091`; contoh selisih kas Rp 20 juta di bagian 5 dokumen yang sama |
| `FIN-DEC-031` | Decision | Kode kejadian baru **`PENERIMAAN-UANG-MUKA`** untuk seluruh penerimaan yang bersifat kewajiban ke pasien sebelum ada piutang final: (a) penerimaan pra-invoice-final (`FIN-DEC-030`), (b) deposit top-up eksplisit, (c) sisi lebih dari kelebihan bayar (`FIN-DEC-033`). Kode ini TERPISAH dari `PENERIMAAN-KASIR` (tetap khusus penerimaan yang benar-benar final, bukan pra-invoice) — sesuai peringatan Accounting bahwa top-up deposit dilarang dikirim sebagai `PENERIMAAN-KASIR`. Kode ke-18/19 pada katalog, **butuh ratifikasi balik Accounting** (lihat `FIN-OQ-017`). | Yasmin (Finance) | `approved` (sisi Finance, menunggu ratifikasi Accounting) | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 3.1, 5 butir 3 dan 5; `contracts/integration-contract.md` bagian 5.4-5.5 (definisi `PENERIMAAN-KASIR` existing) |
| `FIN-DEC-032` | Decision | ~~Satu kode kejadian gabungan untuk sisi PEMAKAIAN uang muka/deposit — dipakai baik saat invoice final melunasi piutang dari uang muka maupun saat PENGEMBALIAN uang ke pasien, dibedakan lewat `SourceTransactionId`/`Components`.~~ **`superseded` oleh `FIN-DEC-040` (pemakaian) dan `FIN-DEC-041` (pengembalian), 25 September 2026 — impact scan `/design-business-module` menemukan lawan jurnal keduanya berbeda (pemakaian: D Uang Muka Pasien/K Piutang, tidak ada kas bergerak; pengembalian: D Uang Muka Pasien/K Kas), sehingga tidak bisa satu kode.** | Yasmin (Finance) | `superseded` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 5 butir 3 dan 5 |
| `FIN-DEC-033` | Decision | ~~Kelebihan bayar (overpayment) dan pengembaliannya TIDAK mendapat kode kejadian sendiri. Sisi kelebihan terima memakai `PENERIMAAN-UANG-MUKA`, sisi pengembalian/pemakaian memakai kode gabungan.~~ **`superseded` oleh `FIN-DEC-042`, 25 September 2026 — Billing (`BilRefundableCredit`) baru mengakui kelebihan bayar SETELAH alokasi (`ALLOCATION_EXCESS`), bukan saat uang diterima, sehingga "sisi kelebihan terima memakai `PENERIMAAN-UANG-MUKA`" tidak mungkin secara temporal: pada saat uang diterima, Finance belum tahu ada kelebihan.** | Yasmin (Finance) | `superseded` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 5 butir 3 (gerbang G6) |
| `FIN-DEC-034` | Decision | Kode kejadian baru **`SELISIH-KAS-SHIFT`** untuk selisih hitung fisik kas vs sistem saat tutup shift kasir — dibukukan ke akun selisih kas/suspense, TERPISAH dari kode kewajiban pasien manapun. Data sumbernya dari `BilCashierShift` milik Billing (lihat aturan bisnis #9, `FIN-DEC-000`). **Pemicu penerbitannya `superseded` (dilengkapi) oleh `FIN-DEC-043`, 25 September 2026** — keputusan ini semula tidak menetapkan kapan kejadiannya terbit. Kode ke-6 dari 7 kode baru pada katalog, **butuh ratifikasi balik Accounting** (`FIN-OQ-017`). | Yasmin (Finance) | `approved` (kode); pemicu lihat `FIN-DEC-043` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 5 butir 3 (gerbang G6) |
| `FIN-DEC-035` | Decision | **Menggantikan `FIN-DEC-023`, menutup `FIN-OQ-011`.** Finance MENERIMA bentuk pesan saldo subledger yang diusulkan Accounting apa adanya: amplop 12 field `ACC-XMOD` yang sama + objek `SubledgerBalance` berisi `AccountingPeriodCode` (`string`, maks 7, `YYYY-MM`) dan `ControlAccountCode` (`string`, maks 50); `Amount`/`AccountingDate` di amplop dipakai langsung (boleh nol/negatif khusus pesan saldo); `SourceVersion` WAJIB bilangan bulat positif naik untuk revisi saldo periode yang sama. Kode `SALDO-SUBLEDGER` (kode ke-18 versi Accounting) diusulkan bersama tiga kode lain (`FIN-DEC-031`, `032`, `034`) — lihat `FIN-OQ-017`. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 3.3, `ACC-DEC-087` |
| `FIN-DEC-036` | Decision | **Merevisi sebagian `FIN-DEC-007`.** Tiga syarat organisasi untuk akun layanan Finance→Accounting DIRATIFIKASI sebagai keputusan Finance, karena ini syarat keras Accounting (bukan pilihan desain Finance): (1) satu akun khusus bukan akun manusia dan BUKAN `SuperAdmin`; (2) akun WAJIB punya penugasan Departemen + Jabatan khusus yang hanya diberi hak `AccountingEvent : Receive` — tanpa penugasan ini seluruh kiriman Finance akan `403`; (3) akun berhak atas badan hukum (`LegalEntityId`) yang dikirimi kejadian. Mekanisme detail token (JWT Bearer vs mekanisme existing) TETAP `open`, dilacak terpisah di `FIN-OQ-016` menunggu keputusan bersama Platform. | Yasmin (Finance) | `approved` (syarat organisasi); mekanisme token `open` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 3.4, `ACC-DEC-088` |
| `FIN-DEC-037` | Decision | **Menutup konflik bagian 6 balasan Accounting.** `FIN-DEC-008` (decision log: cutover sejak go-live, tanggal ditentukan gerbang) DIKONFIRMASI sebagai versi yang berlaku. Referensi tanggal pasti "1 Oktober 2026" pada versi percakapan/dokumen lain DINYATAKAN TIDAK BERLAKU dan digantikan mekanisme 6 gerbang (G1-G6) milik Accounting — Finance TIDAK mengejar tanggal 1 Oktober 2026, dan tidak meminta percepatan gerbang yang bukan kendali Finance (G1-G3). Timeline/roadmap internal Finance yang masih menyebut 1 Oktober 2026 MUST diperbarui. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 4, 6 — status G4 "Pengirim Finance siap: Belum dibangun" per 24 September 2026 mengonfirmasi Finance sendiri belum siap 1 Oktober |
| `FIN-DEC-038` | Decision | Jawaban Finance atas permintaan Accounting bagian 5 butir 2 (daftar `Components` per jenis kejadian): SELURUH jenis kejadian mengirim nilai `TOTAL` untuk rilis ini — belum ada pemecahan komponen apa pun. Field `Components` pada `FinanceAccountingOutboxService.cs` saat ini murni pass-through tanpa aturan bisnis, sehingga jawaban ini menutup gap tanpa menahan desain. Pemecahan komponen (mis. rincian per jenis layanan) dapat diajukan sebagai amendment terpisah bila kebutuhannya muncul. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 5 butir 2; `FinanceAccountingOutboxService.cs` baris pass-through `Components = request.Components` |
| `FIN-DEC-039` | Decision | Finance mencatat tiga hal dari balasan Accounting yang TIDAK memerlukan keputusan baru, hanya ratifikasi/catatan: (a) kontrak `ACC-XMOD` naik dari `0.2` ke `0.3` dan `ACC-XM-001` ditutup — 17 kode existing (`FIN-DEC-002`) diratifikasi APA ADANYA oleh Accounting, tidak ada perubahan bentuk yang menyentuh Finance; (b) pengecualian komponen `JASA_MEDIS` dari kejadian `PENGAKUAN-PIUTANG` (`ACC-DEC-086`) adalah perbaikan sisi Accounting yang KONSISTEN dengan `FIN-DEC-003`, tidak mengubah keputusan Finance; (c) `JournalNumber` pada balasan `201` bersifat OPSIONAL ("simpan bila ada"), bukan wajib selalu terisi — kontrak `FIN-STATE-1.0` bagian tanda terima perlu penyesuaian redaksional saat direvisi berikutnya, bukan perubahan perilaku Finance. | Yasmin (Finance) | `approved` (dicatat sebagai fakta/ratifikasi) | Yasmin, 25 September 2026 | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` bagian 1, 2, 3.2, 3.a; `ACC-DEC-082`, `083`, `086` |
| `FIN-DEC-040` | Decision | **Menggantikan sebagian `FIN-DEC-032`.** Kode `PEMAKAIAN-UANG-MUKA-DEPOSIT` DIPERSEMPIT khusus untuk pelunasan piutang dari uang muka/deposit (lawan jurnal: Debit Uang Muka Pasien, Kredit Piutang — TIDAK ada kas bergerak). Kode ini TIDAK LAGI dipakai untuk pengembalian uang ke pasien (lihat `FIN-DEC-041`). **Pemicunya** adalah baris `BilDepositMovement` bertipe `ALLOCATION` — satu-satunya tempat fakta "deposit/uang muka dipakai melunasi tagihan" benar-benar tercatat di Billing, lengkap dengan `IdempotencyKey`, `CorrelationId`, `CausationId` sendiri untuk rantai telusur. Finance membaca baris ini, tidak menghitung ulang dari sisi piutangnya sendiri (konsisten pola baca `BilTender`/`BilCashierShift` yang sudah ada). | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `Repositories/ApplicationDbContext.cs` baris 604-605 (`BilDepositAccount`, `BilDepositMovement`), `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositMovement.cs` |
| `FIN-DEC-041` | Decision | Kode kejadian baru **`PENGEMBALIAN-UANG-MUKA`** — khusus pengembalian TUNAI dari Uang Muka Pasien ke pasien (lawan jurnal: Debit Uang Muka Pasien, Kredit Kas — kas benar-benar keluar). ~~Dipicu dari DUA sumber: (a) `BilDepositMovement` bertipe `RELEASE`; (b) `BilRefundCase` berstatus `EXECUTED` **khusus** bila `RefundableCredit.SourceType = ALLOCATION_EXCESS`~~ **DIKOREKSI PARSIAL OLEH `FIN-DEC-074` (28 Sept 2026) dan `FIN-DEC-080` (29 Sept 2026):** Butir (a) `BilDepositMovement` `RELEASE` **DICABUT** karena mutasi tersebut tidak mengeluarkan kas melainkan membatalkan alokasi LIFO tagihan (dipetakan ke `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, `FIN-DES-064`). Pemicu `PENGEMBALIAN-UANG-MUKA` kini **murni** `BilRefundCase` berstatus `EXECUTED` dengan sumber `ALLOCATION_EXCESS` maupun `SETTLEMENT` (`FIN-DES-056`). Hanya `REFERRED_OUTPATIENT_ADMIN` yang ditahan. | Yasmin (Finance) | `approved` (dikoreksi parsial oleh `FIN-DEC-080`) | Yasmin, 25 September 2026; dikoreksi 29 September 2026 | Impact scan `7811c048`, `02-backend-architecture.md` bagian H (`FIN-DES-064`), `BilRefundCase.Status.Executed` |
| `FIN-DEC-042` | Decision | **Menggantikan `FIN-DEC-033`.** Kelebihan bayar (overpayment) TIDAK diperlakukan seolah diterima sebagai `PENERIMAAN-UANG-MUKA` sejak awal — itu tidak mungkin secara temporal karena Billing (`BilRefundableCredit`) baru mengakui kelebihan SETELAH alokasi pembayaran terjadi (`SourceType = ALLOCATION_EXCESS`, status `AVAILABLE`), bukan saat uang diterima. Sebagai gantinya: Finance menerbitkan kode kejadian baru **`PENGAKUAN-KELEBIHAN-BAYAR`** pada saat `BilRefundableCredit` diakui, yang memindahkan nilai kelebihan dari lawan jurnal penerimaan asli (mis. Piutang/Pendapatan) ke Uang Muka Pasien — TANPA menyentuh kas, karena kas sudah didebit penuh saat penerimaan asli. Pengembaliannya kelak (bila pasien memilih dikembalikan tunai, bukan dipakai) memakai `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`), karena saldo itu sekarang sudah berada di akun Uang Muka Pasien. Alasan: mencegah kewajiban ke pasien tak terlihat di buku besar antara pengakuan dan pengembalian, dan mencegah koreksi periode yang sudah ditutup Accounting saat pengembalian terjadi belakangan. Kode ke-4 dari 7 kode baru pada katalog, **butuh ratifikasi balik Accounting** (`FIN-OQ-017`). | Yasmin (Finance) | `approved` (sisi Finance, menunggu ratifikasi Accounting) | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `BilRefundableCredit.SourceType.AllocationExcess`, status `Available`/`Exhausted` |
| `FIN-DEC-043` | Decision | **Melengkapi `FIN-DEC-034`.** Kejadian `SELISIH-KAS-SHIFT` diterbitkan SAAT selisih disahkan — yaitu ketika `BilCashierShift.Status` menjadi `REVIEWED` (selisih hasil hitung fisik kas sudah disahkan, bukan sekadar terisi otomatis saat kas fisik diinput). `AccountingDate` pada kejadian ini MEMAKAI tanggal shift (`OpenedAt`/tanggal operasional shift), BUKAN tanggal pengesahan — supaya selisih tetap jatuh di periode akuntansi shift-nya, bukan periode saat direview. **Konsekuensi yang dicatat, bukan diputuskan di sini:** bila pengesahan terjadi setelah Accounting menutup periode shift itu, kejadian tetap terkirim tapi berpotensi tidak dapat dijurnalkan ke periode tertutup — perlu kesepakatan batas waktu pengesahan sebelum tutup bulan, dicatat sebagai titik sentuh untuk didiskusikan bersama Accounting saat gerbang G6 dibahas ulang. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs` baris 737-738 (pembedaan `PhysicalCash` terisi vs status `REVIEWED`) |
| `FIN-DEC-044` | Decision | Kode kejadian baru **`PEMBALIKAN-PENERIMAAN-UANG-MUKA`** — pembalikan penerimaan uang muka/deposit yang tender aslinya di-reverse Billing. `FinanceReceiptService` MUST memilih kode pembalikan berdasarkan `EventTypeCode` yang diterbitkan saat penerimaan ASLI dibuat, bukan menghitung ulang status finalisasi tagihan saat ini (yang mungkin sudah berubah sejak penerimaan terjadi): bila asli `PENERIMAAN-UANG-MUKA`, pembalikannya `PEMBALIKAN-PENERIMAAN-UANG-MUKA`; bila asli `PENERIMAAN-KASIR`, pembalikannya tetap `PEMBALIKAN-PENERIMAAN-KASIR` (existing, tidak berubah). Mencegah lawan jurnal pembalikan Uang Muka Pasien (D Uang Muka Pasien, K Kas) tertukar dengan lawan jurnal pembalikan `PENERIMAAN-KASIR` (D Pendapatan/Piutang, K Kas). Kode ke-7 dari 7 kode baru pada katalog, **butuh ratifikasi balik Accounting** (`FIN-OQ-017`). | Yasmin (Finance) | `approved` (sisi Finance, menunggu ratifikasi Accounting) | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` baris 190-206 |
| `FIN-DEC-045` | Decision | **Menggantikan `FIN-DEC-015`.** Siklus Purchasing/AP PENUH — Purchase Order, Tanda Terima Barang (Goods Receipt), Tukar Faktur, Purchasing Invoice, Retur Pembelian & Deposit Retur, Aging AP, Rekap Purchasing AP, Laporan Tukar Faktur, Laporan Jatuh Tempo, Rekonsiliasi Tagihan — DIBANGUN DI DALAM Finance Management, bukan ditunggu sebagai modul Procurement terpisah. `FinSupplierPayable` yang sebelumnya dipicu input manual kini dipicu dari `Purchasing Invoice` yang sudah disetujui (lihat `FIN-DEC-050`/`051`). Owner secara eksplisit memilih opsi ini meski dampaknya besar: Finance Management berubah dari pola "konsumen fakta hilir" (seperti terhadap Billing dan Medical Fee) menjadi "pemilik proses operasional hulu" untuk pengadaan — pola yang BERBEDA dari seluruh rumpun lain blueprint ini. Master `MstSupplier` existing (`FIN-DEC-014`) tetap dipakai apa adanya, kini diperkaya kebutuhan TOP/lead time/PPN/diskon yang disebut `Keuangan.md` bagian 9.2 (rincian field menyusul `/design-business-module`). | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `Keuangan.md` bagian 2.1, 4 (`FIN-AP-001`..`014`), 7.1; jawaban eksplisit owner pada `/grill-me` 25 September 2026 |
| `FIN-DEC-046` | Decision | PPN Masukan (Pajak Masukan) pada Purchasing Invoice WAJIB terhubung ke Accounting Integration Outbox sejak gelombang pertama EPIC Purchasing/AP — bukan ditunda seperti pola kejadian baru lain di blueprint ini (`FIN-OQ-017`). Kode kejadian baru untuk PPN Masukan MUST diusulkan dan diratifikasi Rizki (Accounting) SEBELUM EPIC Purchasing/AP dianggap siap `/plan-module-delivery` — dicatat `FIN-OQ-020`, mengikuti pola surat evidence yang sudah berjalan (`evidence/13-balasan-accounting-untuk-finance.md`, `FIN-OQ-017`). | Yasmin (Finance) — **titik sentuh keras, butuh ratifikasi Accounting sebelum desain dikunci** | `approved` (sisi Finance; kode kejadian menunggu Accounting) | Yasmin, 25 September 2026 | `Keuangan.md` bagian 6 (Set Purchasing Invoice: PPN %/nominal, COA PPN); jawaban eksplisit owner |
| `FIN-DEC-047` | Decision | Retur Pembelian Supplier TIDAK mengoreksi langsung Purchasing Invoice asal. Nilai retur membentuk **Deposit Retur** — entity kredit tersendiri milik satu supplier, dapat dipakai lintas Purchasing Invoice/PO di kemudian hari (pola sejenis `BilRefundableCredit` milik Billing, tapi entity Finance sendiri karena arah aliran uangnya berlawanan — kredit dari supplier ke RS, bukan RS ke pasien). Deposit Retur MUST punya saldo tersedia (`AvailableAmount`) yang berkurang saat dipakai mengurangi pembayaran Purchasing Invoice manapun milik supplier yang sama. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `Keuangan.md` bagian 2.1 poin 9, bagian 6 ("Retur" vs "Deposit Retur" disebut sebagai dua istilah terpisah secara konsisten); jawaban eksplisit owner |
| `FIN-DEC-048` | Decision | AR Invoice Agregat ke Company/Guarantor adalah DOKUMEN TAGIHAN RESMI yang dikirim ke penjamin — bernomor faktur tunggal, menggantikan peran `BilInvoice` per pasien sebagai dokumen tagihan yang dikirim keluar. `BilInvoice` individual TETAP ada apa adanya di Billing sebagai sumber/rincian di balik AR Invoice Agregat (Finance TIDAK menyalin ulang isi `BilInvoice`, hanya merujuknya), bukan digantikan atau dihapus. Konsekuensi: `FinReceivable` (satu per `BilInvoice`, `FIN-DES-010`) TETAP menjadi unit piutang internal untuk aging/alokasi per baris; AR Invoice Agregat adalah LAPISAN BARU di atasnya untuk keperluan penagihan resmi — bukan pengganti `FinReceivable`. Satu AR Invoice Agregat menaungi banyak `FinReceivable` untuk satu penjamin pada satu periode penagihan. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `Keuangan.md` bagian 2.2 poin 2, bagian 6 ("Buat AR Invoice": memilih banyak billing sekaligus); jawaban eksplisit owner |
| `FIN-DEC-049` | Decision | Potongan sisi penerimaan piutang (PPh 23 dipotong penjamin sebelum uang sampai ke RS, biaya admin bank saat pencairan) DIRANCANG sebagai entity BARU, berdiri sendiri dari `FinPaymentDeduction` (`FIN-DES-026`, sisi Payable). Arah aliran uangnya berlawanan: `FinPaymentDeduction` mengurangi uang yang RS TRANSFER keluar tanpa mengurangi utang yang lunas (`FIN-DES-028`); potongan sisi AR mengurangi uang yang RS TERIMA masuk. Apakah nilainya ikut mengurangi `FinReceivable.OutstandingAmount` (piutang dianggap lunas walau uang diterima lebih sedikit) atau murni catatan rekonsiliasi kas TANPA menyentuh saldo piutang — **belum diputuskan, dicatat `FIN-OQ-021` bagian kedua** (digabung dengan pertanyaan PPN Keluaran karena berdekatan). | Yasmin (Finance) | `approved` (entity berdiri sendiri); efeknya ke `OutstandingAmount` `open` | Yasmin, 25 September 2026 | `Keuangan.md` bagian 2.2 poin 7, bagian 9.3 (PPh 23, biaya admin bank); jawaban eksplisit owner |
| `FIN-DEC-050` | Decision | Approval Purchase Order dan Purchasing Invoice memakai STRUKTUR dua jenjang berdasarkan nominal: Supervisor Finance untuk nominal di bawah ambang, Manajer Finance untuk nominal di atas ambang. Ini checkpoint YANG BERBEDA dari `FIN-DEC-022`/`FIN-OQ-010` (approval PEMBAYARAN AP, `EPIC FIN-09`) — PO/Invoice disetujui SEBELUM menjadi utang terjadwal bayar, pembayaran disetujui SESUDAHNYA sebagai checkpoint terpisah. Angka ambang nominal PASTI BELUM ditentukan, dicatat `FIN-OQ-019` (pola sama dengan `FIN-OQ-010`: tidak memblokir pemodelan data, hanya aturan validasi angka). Jejak audit MUST mencatat jenjang mana yang berlaku untuk tiap PO/Invoice, konsisten dengan tuntutan `FIN-DEC-022` untuk sisi pembayaran. | Yasmin (Finance) | `approved` (struktur); ambang nominal `open` | Yasmin, 25 September 2026 | `Keuangan.md` bagian 13.1 poin 4 (Unresolved Question asli: "Siapa yang berwenang approval invoice, retur, pembayaran, dan rekonsiliasi?"); jawaban eksplisit owner, dibandingkan pola `FIN-DEC-022` |
| `FIN-DEC-051` | Decision | Tukar Faktur adalah ENTITY TERPISAH dari Purchasing Invoice, checkpoint serah-terima dokumen fisik faktur dari supplier yang terjadi LEBIH DULU dalam alur (`PO → Tukar Faktur → Tanda Terima/Set Invoice → Purchasing Invoice`, `Keuangan.md` bagian 7.1). Tukar Faktur mencatat tanggal terima faktur dan estimasi jatuh tempo awal (dari TOP supplier); Purchasing Invoice yang menyusul menetapkan NILAI FINAL (PPN, diskon, potongan, DP/termin) yang boleh berbeda dari estimasi awal Tukar Faktur. Satu Tukar Faktur dapat berasal dari PO maupun tanpa PO (`Keuangan.md` FIN-AP-RULE-004), dan satu Tukar Faktur berlanjut ke tepat satu Purchasing Invoice. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | `Keuangan.md` bagian 2.1 poin 2-4, bagian 7.1 langkah 2-5; jawaban eksplisit owner |
| `FIN-DEC-052` | Decision | **Menutup `FIN-OQ-019` dan `FIN-OQ-010` SEKALIGUS, disengaja bersamaan supaya satu kebijakan konsisten.** Ambang nominal approval berjenjang AP = **Rp 50.000.000**, berlaku untuk KEDUA checkpoint: (a) persetujuan Purchase Order/Purchasing Invoice (`FIN-DEC-050`) dan (b) persetujuan pembayaran AP — supplier maupun rekap jasa tenaga medis (`FIN-DEC-022`). Nominal `< Rp 50.000.000` disetujui Supervisor Finance; nominal `>= Rp 50.000.000` disetujui Manajer Finance. Jejak audit MUST mencatat jenjang yang berlaku dan nominal transaksi pada saat approval diberikan (bukan dihitung ulang belakangan bila kebijakan ambang berubah di masa depan). **Penyederhanaan yang dicatat eksplisit, bukan diam-diam dijatuhkan:** `FIN-OQ-010` versi 20 September 2026 sempat mengangkat kemungkinan ambang BERBEDA per jenis transaksi (supplier vs rekap dokter vs write-off AR, karena karakter risikonya berbeda — rekap dokter menggabungkan puluhan-ratusan fee individual). Jawaban pass ini memakai **satu ambang seragam Rp 50.000.000 untuk seluruhnya**. Bila pembedaan per jenis transaksi tetap diinginkan, ini MUST diangkat sebagai amendment terpisah — bukan diasumsikan tertutup oleh keseragaman ini. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner pada closure pass `/grill-me` 25 September 2026 |
| `FIN-DEC-053` | Decision | **Menutup `FIN-OQ-020` sisi Finance; ratifikasi Accounting tetap `open`.** Kode kejadian PPN Masukan diusulkan bernama **`PPN-MASUKAN-PEMBELIAN`**, kode ke-25 pada katalog (setelah tujuh kode `FIN-OQ-017`). Usulan ini MUST dikirim sebagai surat evidence terpisah ke Rizki (Accounting) — mengikuti pola surat `evidence/13`/`14` yang sudah berjalan — SEBELUM EPIC Purchasing/AP dianggap siap `/plan-module-delivery`, sesuai syarat keras `FIN-DEC-046`. Lawan jurnal persisnya (akun PPN Masukan mana, apakah langsung mengurangi kewajiban PPN Keluaran bulan berjalan atau menunggu rekonsiliasi SPT Masa) TIDAK diputuskan Finance sepihak — itu wewenang Accounting saat meratifikasi, konsisten dengan pola seluruh kode kejadian lain di blueprint ini. | Yasmin (Finance) — **sisi Finance saja; ratifikasi tetap wewenang Rizki** | `approved` (usulan sisi Finance); ratifikasi Accounting `open` | Yasmin, 25 September 2026 | Jawaban eksplisit owner; pola `FIN-DEC-031` dkk |
| `FIN-DEC-054` | Decision | **Menutup `FIN-OQ-021` bagian (a).** AR Invoice Agregat ke Company/Guarantor (`FIN-DEC-048`) diperlakukan **PPN-exempt sepenuhnya** — TIDAK memerlukan integrasi Faktur Pajak/PPN Keluaran pada rilis ini. **Catatan risiko yang dicatat apa adanya, bukan diverifikasi ulang oleh pass ini:** ini jawaban operasional owner Finance, bukan opini resmi konsultan pajak. Bila kelak ditemukan komponen non-medis (mis. obat OTC, ekstra bed) yang secara hukum tetap kena PPN Keluaran walau menyertai layanan kesehatan, keputusan ini MUST ditinjau ulang sebagai amendment — jangan diasumsikan tertutup permanen hanya karena tercatat di sini. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner. **MUST ditinjau ulang bila ada indikasi sebaliknya** — bukan keputusan final yang kebal koreksi |
| `FIN-DEC-055` | Decision | **Menutup `FIN-OQ-021` bagian (b).** Potongan sisi penerimaan piutang (`FIN-DEC-049` — PPh 23, biaya admin bank) **MENGURANGI `FinReceivable.OutstandingAmount`**, berfungsi sebagai "pembayaran non-tunai" yang turut melunasi piutang bersama uang tunai yang benar-benar diterima: `FinReceivable` dianggap lunas sebesar (kas diterima + potongan sah), bukan hanya sebesar kas yang diterima. Alasan: PPh 23 yang dipotong penjamin adalah kredit pajak sah milik RS (Bukti Potong) — bukan uang hilang, dan piutang yang kewajibannya sudah dipenuhi penjamin secara sah MUST NOT tercatat menggantung di aging. **Ini kebalikan (mirror) dari `FIN-DES-028` sisi Payable** (potongan TIDAK mengurangi sisa utang, karena arah uangnya sebaliknya — RS yang mentransfer, bukan menerima) — kedua aturan sengaja BERBEDA arah karena posisinya memang berbeda, bukan inkonsistensi. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner, dibandingkan eksplisit dengan `FIN-DES-028` |
| `FIN-DEC-056` | Decision | **Mempersempit `FIN-DEC-046` — hanya sesudah `/design-business-module` (revisi 4) selesai dan owner meninjau ulang konsekuensinya.** `FIN-DEC-046` semula menuntut SELURUH `EPIC Purchasing/AP` (`EPIC FIN-15`) tidak boleh diteruskan `/plan-module-delivery` sampai `PPN-MASUKAN-PEMBELIAN` diratifikasi Accounting (`FIN-OQ-020`). Sesudah desain arsitektur selesai, terbukti pola outbox transaksional yang SAMA PERSIS dengan `EPIC FIN-11` sudah menjaga risikonya: baris kejadian PPN Masukan tetap DITULIS `PENDING` saat Purchasing Invoice disetujui (`FIN-DES-043`), hanya WORKER PENGIRIMANNYA yang tertahan (`FIN-VAL-122`) — identik dengan bagaimana `EPIC FIN-11` sudah berjalan dalam seluruh gelombang `MVP-0`..`MVP-5` walau endpoint penerima Accounting (`FIN-CAP-018`) sendiri belum ada. Owner menegaskan alasan tambahan: Finance adalah **titik asal (upstream)** bagi Accounting — kesiapan Finance MUST NOT digantungkan pada kecepatan ratifikasi hilir. **Keputusan baru:** `EPIC FIN-15` (PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur, approval berjenjang, laporan) **BOLEH** diteruskan penuh ke `/plan-module-delivery` dan masuk gelombang pengiriman TANPA menunggu `FIN-OQ-020`. Yang **TETAP** tertahan `FIN-OQ-020`, sempit dan eksplisit: **AKTIVASI worker pengiriman kejadian `PPN-MASUKAN-PEMBELIAN` ke Accounting** — baris outbox-nya tetap ditulis sejak awal, hanya pengirimannya yang menunggu. `FIN-DEC-046` TIDAK dicabut, HANYA dipersempit cakupannya dari "seluruh epic" menjadi "satu worker". | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas pertanyaan langsung: "apakah bisa lanjut plan module delivery tanpa menunggu ratifikasi Rizki... karena modul finance adalah awal dari modul accounting"; preseden `EPIC FIN-11`/`FIN-CAP-018` |
| `FIN-DEC-057` | Decision | **Menutup `FIN-OQ-023`.** Deposit Retur dipakai sebagai **baris alokasi non-tunai di dalam satu `FinPayment`**, bersanding dengan alokasi transfer bank biasa — satu pembayaran boleh sebagian dari deposit, sebagian dari transfer nyata. `NetTransferAmount` hanya menghitung porsi kas sungguhan; `OutstandingAmount` tetap berkurang sebesar seluruh alokasi (tunai + deposit), memakai ulang jalur penulis yang sudah ada (`FinancePaymentService`/`FinanceSupplierPayableService`) — nol penulis `OutstandingAmount` baru. Ini memakai ulang kemampuan `FinPayment` yang SUDAH BISA melunasi banyak `FinSupplierPayable` sekaligus dalam satu pembayaran (pola rekap `FIN-DES-015`), sehingga sifat lintas-invoice Deposit Retur (`FIN-DEC-047`) terpenuhi tanpa mekanisme terpisah. **Konsekuensi arsitektur yang BELUM digambar:** `FinPaymentAllocation` dan/atau `FinSupplierReturnDepositUsage` (`FIN-DES-038`) perlu jalur untuk menandai satu baris alokasi sebagai "dari deposit" alih-alih "dari transfer bank" — ini **MUST** digambar ulang pada amendment arsitektur berikutnya sebelum `BE-FIN-036` dapat dimulai; `/grill-me` ini hanya menutup keputusan bisnisnya, bukan skemanya. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan; preseden `FIN-DES-015` (satu pembayaran, banyak utang) |
| `FIN-DEC-058` | Decision | **Menutup `FIN-OQ-024`.** Kode kejadian baru diusulkan untuk potongan AR, bernama **`POTONGAN-PIUTANG-NON-TUNAI`** (kode ke-26 pada katalog, setelah `PPN-MASUKAN-PEMBELIAN` di posisi 25) — mengikuti pola `FIN-DEC-053`: setiap pemicu bisnis berbeda mendapat kode sendiri, tidak digabung ke `AR_PAYMENT`/`PENERIMAAN-PIUTANG` (yang berarti kas masuk) maupun `PENYESUAIAN-PIUTANG` (yang berarti koreksi maker-checker disetujui — proses berbeda dari pencatatan potongan saat alokasi). Usulan ini **MUST** dikirim sebagai surat evidence terpisah ke Rizki, mengikuti pola `evidence/06`, **sebelum** `BE-FIN-040` mulai dikerjakan. **Mengikuti `FIN-DEC-056`:** gerbang ratifikasi ini HANYA menahan aktivasi worker pengiriman kode `POTONGAN-PIUTANG-NON-TUNAI`, TIDAK menahan seluruh `EPIC FIN-17` masuk `/plan-module-delivery` — baris outbox-nya tetap ditulis `PENDING` sejak potongan pertama dicatat. Lawan jurnal persisnya TIDAK diputuskan Finance sepihak, konsisten dengan pola seluruh kode lain. | Yasmin (Finance) — sisi Finance saja; ratifikasi tetap wewenang Rizki | `approved` (usulan sisi Finance); ratifikasi Accounting `open`, dicatat `FIN-OQ-026` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan; pola `FIN-DEC-053` |
| `FIN-DEC-059` | Decision | **Menutup `FIN-OQ-025`.** Endpoint `GET /purchasing/reports/aging` **dicabut** dari `contracts/api-contract.md` — layar Aging Purchasing/AP memakai `GET api/finance/payable/aging` (`FinanceApController`) yang sudah berjalan, apa adanya. Nol endpoint duplikat, nol risiko dua angka umur utang yang bisa berselisih — inilah risiko yang membuka pertanyaan ini. Pencabutan ini **MUST** dicatat sebagai revisi kontrak bernomor pada `FIN-API-1.1` (bukan dihapus diam-diam), dan `FinancePurchasingReportService`/`Controller` (`BE-FIN-037`) dibangun tanpa endpoint ini. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan |
| `FIN-DEC-060` | Decision | ~~Menutup `FIN-OQ-022`. Lima layar (Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian & Deposit Retur, Laporan Pembelian) masuk submenu baru "Pembelian" pada group `corporateFinance`, sejajar submenu Master Data yang sudah ada — memisahkan konsep "proses pembelian" dari "Faktur & Tagihan Supplier" (layar existing di `/finance/payable/invoice`) yang tetap di bawah pengelompokan Utang. Purchasing Invoice diberi label "Faktur Pembelian". Batch Tagihan AR menjadi satu butir flat "Tagihan Gabungan Penjamin", sejajar butir "Piutang" yang sudah ada.~~ **`superseded` oleh `FIN-DEC-094`, 1 Oktober 2026 — owner memutuskan seluruh menu Transaksi A/R dan A/P mengikuti bentuk dan penempatan halaman persis sistem produksi V1 (`QuilvianSystemFrontendDev1`/`QuilvianSystemBackendDev1`, sudah UAT-approved), menggantikan pengelompokan submenu "Pembelian" dengan struktur flat sesuai V1.** | Yasmin (Product Owner Finance) | `superseded` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan |
| `FIN-DEC-061` | Decision | **Dibuka dan ditutup `/design-business-module` revisi 5, 25 September 2026.** Retur Pembelian dan pemakaian Deposit Retur mendapat **dua kode kejadian baru**: **`RETUR-PEMBELIAN`** (kode ke-28) terbit saat retur `CONFIRMED` sebesar nilai retur — piutang RS ke supplier bertambah; **`PEMAKAIAN-DEPOSIT-RETUR`** (kode ke-29) terbit saat `FinPayment` `PAID` sebesar porsi yang dilunasi deposit. Untuk pembayaran yang memakai deposit, kejadian pembayaran utang yang sudah ada (`AP_PAYMENT`) **turun menjadi `TotalAmount − DepositAppliedAmount`** — pola yang sama dengan pemisahan pokok/PPN pada `BE-FIN-034`, supaya Kas di buku besar hanya bergerak sebesar uang yang benar-benar keluar. Alasan: tanpa pemisahan ini, pembayaran yang dilunasi deposit tercatat sebagai kas keluar penuh. Mengikuti `FIN-DEC-056`: ratifikasi Rizki hanya menahan worker pengiriman, tidak menahan pembangunan. Lawan jurnal persis wewenang Accounting. | Yasmin (Finance) — sisi Finance; ratifikasi wewenang Rizki | `approved` (usulan sisi Finance); ratifikasi dicatat `FIN-OQ-026` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan; temuan `FinancePaymentService.cs` baris 553-563 (`AP_PAYMENT` bernilai `TotalAmount`) |
| `FIN-DEC-062` | Decision | **Dibuka dan ditutup `/design-business-module` revisi 5, 25 September 2026.** Pembalikan potongan AR memakai kode pasangan **`PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI`** (kode ke-27), mengikuti preseden `FIN-DEC-044` bahwa setiap kode penerimaan punya kode pembalikan sendiri. Berlaku saat alokasi yang membawa potongan dibalik — termasuk pembalikan **otomatis** karena tender Billing dibatalkan (`FIN-DEC-021`), yang karena itu tidak dapat dilarang. Baris potongan pembalik dibuat, baris lama tidak dihapus. Diusulkan dalam surat yang sama dengan `POTONGAN-PIUTANG-NON-TUNAI`. | Yasmin (Finance) — sisi Finance; ratifikasi wewenang Rizki | `approved` (usulan sisi Finance); ratifikasi dicatat `FIN-OQ-026` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan; preseden `FIN-DEC-044` |

## Acceptance Criteria

Belum dirumuskan untuk AR/AP/Cash — akan ditambahkan setelah klaster berikutnya digali.
Untuk klaster lintas-modul yang sudah ditutup, acceptance criteria yang sudah dapat diuji:

1. Kejadian yang dikirim Finance ke Accounting memuat seluruh 12 field wajib `ACC-XMOD-0.2`
   tanpa data pasien (`FIN-DEC-001`).
2. Kejadian `PENGAKUAN-PIUTANG` untuk tagihan yang mengandung jasa medis dokter **tidak**
   membawa komponen `JASA_MEDIS`; komponen itu hanya muncul lewat kejadian
   `PENGAKUAN-HUTANG-DOKTER` setelah `DoctorServiceFee` berstatus Approved (`FIN-DEC-003`).
3. ~~`FinReceipt` untuk tender sukses pada invoice yang masih OPEN tetap tercatat dan terlihat di
   Finance, tetapi baris outbox terkait berstatus `HELD_FOR_FINALIZATION` sampai invoice FINAL
   (`FIN-DEC-004`).~~ **Digantikan** (`FIN-DEC-030`, 25 September 2026): `FinReceipt` untuk
   tender sukses pada invoice yang masih OPEN terkirim SEGERA ke Accounting sebagai
   `PENERIMAAN-UANG-MUKA` (`FIN-DEC-031`); tidak ada lagi baris outbox berstatus
   `HELD_FOR_FINALIZATION` untuk skenario ini.
4. Tidak ada transaksi Finance sebelum tanggal go-live integrasi yang terkirim sebagai kejadian
   Accounting; saldo sebelum tanggal itu tidak direkonstruksi otomatis dari histori Finance
   (`FIN-DEC-008`, dikonfirmasi `FIN-DEC-037`).
5. Saat invoice final terbit atas piutang yang sebagian/seluruhnya sudah dibayar lewat
   `PENERIMAAN-UANG-MUKA`, kode pemakaian gabungan (`FIN-DEC-032`) WAJIB terbit pada hari yang
   sama untuk melunasi piutang dari uang muka — sisa piutang (bila ada) tetap `OUTSTANDING`
   (`FIN-DEC-030`).
6. Kejadian yang dikirim Finance ke Accounting untuk seluruh jenis (termasuk tujuh kode baru
   `FIN-DEC-031`, `034`/`040`/`041`/`042`/`035`/`044`) memakai `Components = TOTAL`, tidak ada
   pemecahan komponen pada rilis ini (`FIN-DEC-038`).
7. Kode `PEMAKAIAN-UANG-MUKA-DEPOSIT` (`FIN-DEC-040`) hanya terbit dari baris `BilDepositMovement`
   bertipe `ALLOCATION`; kode `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`) hanya terbit dari
   `BilDepositMovement` bertipe `RELEASE` atau `BilRefundCase` `EXECUTED` dengan
   `RefundableCredit.SourceType = ALLOCATION_EXCESS`. Kedua kode TIDAK PERNAH dipertukarkan
   satu sama lain.
8. Kejadian `SELISIH-KAS-SHIFT` (`FIN-DEC-034`/`043`) hanya terbit setelah
   `BilCashierShift.Status = REVIEWED`; `AccountingDate`-nya adalah tanggal shift, bukan tanggal
   pengesahan.
9. Pembalikan penerimaan uang muka memakai `PEMBALIKAN-PENERIMAAN-UANG-MUKA` bila penerimaan
   aslinya `PENERIMAAN-UANG-MUKA`, dan `PEMBALIKAN-PENERIMAAN-KASIR` bila penerimaan aslinya
   `PENERIMAAN-KASIR` (`FIN-DEC-044`) — kode pembalikan MUST NOT dihitung ulang dari status
   finalisasi tagihan saat pembalikan terjadi.

## Open Questions dan Blocker

Ditutup pada Closure pass 20 September 2026: ~~`FIN-OQ-001`~~ (`FIN-DEC-023`, draft sisi
Finance — status detail tetap dipantau di `FIN-OQ-011`), ~~`FIN-OQ-002`~~ (`FIN-DEC-014`),
~~`FIN-OQ-003`~~ (`FIN-DEC-018`), ~~`FIN-OQ-005`~~ (`FIN-DEC-012`), ~~`FIN-OQ-006`~~
(`FIN-DEC-019`), ~~`FIN-OQ-007`~~ (`FIN-DEC-020`), ~~`FIN-OQ-008`~~ (`FIN-DEC-021`),
~~`FIN-OQ-009`~~ (`FIN-DEC-022`, bentuk aturan — nilai ambang tetap terbuka di `FIN-OQ-010`).
`FIN-OQ-004` sepenuhnya terurai menjadi keputusan `FIN-DEC-010` s.d. `023`.

Sisa yang masih genuinely terbuka:

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| ~~`FIN-OQ-010`~~ | ~~Ambang nominal approval berjenjang AP — bentuk aturannya sudah diputuskan `FIN-DEC-022`, angka rupiah persisnya belum. Perlu ditetapkan per jenis transaksi secara terpisah (supplier vs dokter vs write-off AR)?~~ **DITUTUP 25 September 2026** — satu ambang seragam Rp 50.000.000 untuk seluruh jenis transaksi AP, `FIN-DEC-052`. Pembedaan per jenis transaksi yang sempat diangkat **TIDAK dipakai**, dicatat eksplisit di `FIN-DEC-052` sebagai penyederhanaan yang bisa diamandemen terpisah bila masih diinginkan. | — | — |
| ~~`FIN-OQ-011`~~ | ~~Konfirmasi Accounting (Rizki) atas draf nama field saldo subledger per periode (`FIN-DEC-023`).~~ **DITUTUP 25 September 2026** — Accounting mengganti bentuknya, Finance menerima (`FIN-DEC-035`). | — | — |
| `FIN-OQ-016` | Mekanisme teknis token/kredensial akun layanan Finance→Accounting (mis. JWT Bearer berumur pendek vs mekanisme auth existing). Syarat organisasinya sudah diratifikasi (`FIN-DEC-036`); yang terbuka murni bentuk kredensialnya. | Platform + Yasmin (Finance) + Rizki (Accounting) | `DESIGN` — memblokir gerbang cutover G3/G4 milik Accounting, bukan pemodelan data Finance |
| `FIN-OQ-017` | **DIPERBARUI 25 September 2026 — naik dari empat menjadi TUJUH kode; surat koreksi SUDAH TERKIRIM.** Ratifikasi balik Accounting (Rizki) atas tujuh kode kejadian baru yang diusulkan sisi Finance: `SALDO-SUBLEDGER` (`FIN-DEC-035`), `PENERIMAAN-UANG-MUKA` (`FIN-DEC-031`), `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`), `PENGAKUAN-KELEBIHAN-BAYAR` (`FIN-DEC-042`), `PEMAKAIAN-UANG-MUKA-DEPOSIT` yang cakupannya sudah dipersempit (`FIN-DEC-040`), `SELISIH-KAS-SHIFT` (`FIN-DEC-034`/`043`), dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` (`FIN-DEC-044`) — termasuk nama final dan urutan kode ke-18 s.d. 24 pada katalog. **Ditutup: surat koreksi `evidence/05-koreksi-jawaban-atas-balasan-accounting.md` sudah dikirim 25 September 2026**, mengoreksi `evidence/04` yang sebelumnya hanya menyebut empat kode lama — pembaca cukup merujuk `evidence/05` bagian 2 untuk tabel tujuh kode yang berlaku. Yang tersisa murni menunggu balasan Rizki. | Accounting (Rizki) | `DESIGN` — memblokir gerbang G6 dan `EPIC FIN-11`/rumpun uang muka sampai dikonfirmasi; TIDAK memblokir MVP-0/1/4 |
| `FIN-OQ-018` | Lawan jurnal `BilRefundCase` dengan `RefundCategory = "BILLING"` bersumber `SourceType SETTLEMENT` atau `REFERRED_OUTPATIENT_ADMIN` belum digali — sengaja DIKELUARKAN dari cakupan `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`) karena kemungkinan bukan penarikan Uang Muka Pasien, melainkan koreksi piutang/pendapatan. | Yasmin (Finance) | `LATER SLICE` — tidak memblokir G6 (permintaan Accounting hanya menyebut deposit pasien dan kelebihan bayar), tidak menahan MVP manapun saat ini |
| `FIN-CAP-STALE-001` | **Catatan, bukan pertanyaan tertutup.** Impact scan 25 September 2026 (backend SHA bergerak `09101d05` → `d6cdfaf9`) menemukan `BilCollectionHandoff` SUDAH terdaftar di `Repositories/ApplicationDbContext.cs` baris 619 — `DbSet<BilCollectionHandoff> BilCollectionHandoffs`. `blocking_questions` pada `blueprint-manifest.md` (`BILLING-COLLECTION-HANDOFF`, menahan gelombang MVP-2/MVP-3 lewat `FIN-DEC-005`) mungkin sudah gugur, TAPI belum diverifikasi apakah bentuk kolomnya cocok dengan yang diminta Finance. | Yasmin (Finance) | Rekomendasi: jalankan `trace-existing-capabilities` mode impact scan sebelum `blocking_questions` ini ditutup atau MVP-2/3 direncanakan |
| `FIN-CQ-02` (dari `01-existing-capability-map.md` §9) | DTO/response shape persis dan daftar `[AccessPermission]` per endpoint Petty Cash belum dibaca detail — dibutuhkan sebagai acuan pola saat mendesain endpoint AR/AP/Doctor Payment supaya konsisten. | Yasmin (Finance) | ~~`DESIGN`~~ — **DITUTUP 20 September 2026** oleh pass `/design-business-module` |
| ~~`FIN-OQ-013`~~, ~~`FIN-OQ-014`~~ | Keduanya **DITUTUP 20 September 2026** oleh amendment revisi 2 pada `FIN-BP-001`. `FIN-DES-025` menggantikan `FinDoctorPayable` dengan `FinMedicalServicePayable`; `FIN-DES-026`..`028` menambahkan `FinPaymentDeduction`, kolom `NetTransferAmount`, dan aturan bahwa potongan tidak mengurangi sisa utang. Rinciannya ada pada bagian AMENDMENT REVISI 2 di `02-backend-architecture.md`. Keputusan arsitekturnya masih `draft` dan menunggu persetujuan owner. | — | — |
| `FIN-OQ-014` | **Amendment yang dituntut pass Medical Fee, 20 September 2026.** `MF-DEC-002` memperluas penerima jasa menjadi dokter **dan** tenaga kesehatan lain, dan `MF-DEC-008` memutuskan `FinDoctorPayable` **digantikan** satu entity utang jasa tenaga medis yang membedakan jenis penerima lewat kolom — mengikuti pola `FinPayment` yang sudah melayani supplier dan dokter sekaligus. Berkas `FIN-BP-001` yang terdampak: `02-backend-architecture.md`, `erd/payable.md`, `erd/data-dictionary.md`, `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, `contracts/state-transition-matrix.md`, `04-prd-to-mvp.md` (`EPIC FIN-08`). | Yasmin (owner Finance) | `DESIGN` untuk `EPIC FIN-08` — sudah `POST-MVP` dan **nol baris kode**, sehingga tidak membongkar apa pun. Lihat `MF-CQ-02` pada blueprint `medical-fee` |
| `FIN-OQ-013` | **Gap yang dibuka pass Medical Fee, 20 September 2026.** `MF-DEC-005` memutuskan Medical Fee menyerahkan jasa **kotor**, dan seluruh potongan per penerima per periode — PPh 21, kasbon, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, iuran kerohanian — diterapkan **Finance** saat menyusun rekap pembayaran. `FinPayment` hasil rancangan `FIN-DES-015` **tidak punya tempat sama sekali** untuk itu: ia hanya memiliki `TotalAmount` dan alokasi ke utang. Konsekuensi yang MUST diselesaikan: (a) Finance perlu entity potongan/tambahan per pembayaran; (b) invariant `FinPayment.TotalAmount = Σ alokasi` pada `FIN-DES-015` **tidak lagi berlaku apa adanya** — nilai transfer bersih berbeda dari jumlah utang yang dilunasi. | Yasmin (owner Finance + Medical Fee) | `DESIGN` untuk `EPIC FIN-09` — sudah `POST-MVP` sehingga **tidak menahan MVP**, tetapi MUST ditutup sebelum rumpun pembayaran dibangun. Lihat `MF-CQ-01` pada blueprint `medical-fee` |
| `FIN-OQ-012` | Pembayaran honor dokter di RS MMC dipisah dua tahap — tanggal 5 untuk pasien pribadi dan asuransi yang sudah membayar ke rumah sakit, tanggal 10 untuk sisanya termasuk kasus rumah sakit menalangi dulu. Apakah pola itu dipertahankan di V2? Bila ya, Finance perlu cara mengetahui status bayar tagihan sumber untuk setiap utang dokter, dan jalur datanya belum dirancang sama sekali. | Yasmin (Finance) + Billing | `DESIGN` untuk `EPIC FIN-09` — sudah `POST-MVP`, tidak menahan MVP. Dibuka 20 September 2026 dari bukti meeting (`evidence/03-referensi-meeting-rs-mmc.md`) |
| `FIN-OQ-015` | Lingkup detail cetak/unduh pada layar Finance — dokumen apa yang boleh dicetak/diunduh (mis. daftar piutang, slip setoran, rekap pembayaran) dan format apa (PDF/Excel). Bentuk keputusannya sudah ditutup `FIN-DEC-029` (ditunda dari MVP, layar dibangun tanpa tombol cetak/unduh); yang masih terbuka hanya lingkup detailnya untuk task terpisah kelak. | Yasmin (Product Owner Finance) | `LATER SLICE` — tidak memblokir `FE-FIN-001`/`002`/`003`/`006`. Dibuka 23 September 2026, Amendment pass UI brief |
| ~~`FIN-OQ-019`~~ | ~~Ambang nominal pasti untuk dua jenjang approval PO/Purchasing Invoice.~~ **DITUTUP 25 September 2026 bersamaan dengan `FIN-OQ-010`** — Rp 50.000.000, lihat `FIN-DEC-052`. | — | — |
| `FIN-OQ-020` | **Dipersempit KEDUA KALINYA 25 September 2026 (`FIN-DEC-056`).** Usulan sisi Finance sudah dikunci (`PPN-MASUKAN-PEMBELIAN`, `FIN-DEC-053`). Yang tersisa MURNI ratifikasi Accounting (Rizki) atas kode ini — lawan jurnal persis, urutan resmi pada katalog (kode ke-25). MUST dikirim sebagai surat evidence terpisah, mengikuti pola `evidence/13`/`14` yang sudah berjalan dengan `FIN-OQ-017`. | Accounting (Rizki) | `DESIGN` — **TIDAK LAGI memblokir `EPIC FIN-15` masuk `/plan-module-delivery`** (`FIN-DEC-046` dipersempit `FIN-DEC-056`, mengikuti preseden `EPIC FIN-11`/`FIN-CAP-018`). Yang MASIH tertahan HANYA aktivasi worker pengiriman kejadian `PPN-MASUKAN-PEMBELIAN` — baris outbox-nya tetap ditulis sejak Purchasing Invoice pertama disetujui |
| ~~`FIN-OQ-021`~~ | ~~Dua bagian: (a) status PPN Keluaran/Faktur Pajak pada AR Invoice Agregat, (b) efek potongan sisi AR ke `OutstandingAmount`.~~ **DITUTUP PENUH 25 September 2026** — (a) PPN-exempt sepenuhnya, `FIN-DEC-054`, **ditandai wajib ditinjau ulang bila ada indikasi sebaliknya**; (b) potongan MENGURANGI `OutstandingAmount` sebagai pembayaran non-tunai, `FIN-DEC-055`. | — | — |
| ~~`FIN-OQ-022`~~ | ~~Label dan pengelompokan butir menu untuk layar revisi 4.~~ **DITUTUP 25 September 2026** — submenu "Pembelian" untuk lima layar AP, butir flat "Tagihan Gabungan Penjamin" untuk Batch AR, Purchasing Invoice dilabeli "Faktur Pembelian" (`FIN-DEC-060`). | — | — |
| ~~`FIN-OQ-023`~~ | ~~Mekanisme Deposit Retur mengurangi `FinSupplierPayable`.~~ **DITUTUP 25 September 2026** — baris alokasi non-tunai di dalam `FinPayment` (`FIN-DEC-057`). **Konsekuensi skema belum digambar** — lihat catatan pada `FIN-DEC-057`, menahan `BE-FIN-036` sampai amendment arsitektur berikutnya | — | — |
| ~~`FIN-OQ-024`~~ | ~~Kode kejadian akuntansi untuk potongan AR.~~ **DITUTUP sisi Finance 25 September 2026** — kode `POTONGAN-PIUTANG-NON-TUNAI` diusulkan (`FIN-DEC-058`), kode ke-26. **Ratifikasi Accounting masih terbuka**, dicatat `FIN-OQ-026` | — | — |
| `FIN-OQ-026` | **Dibuka `/grill-me` 25 September 2026, diperluas `/design-business-module` revisi 5 hari yang sama.** Ratifikasi Rizki (Accounting) atas **empat** kode baru: `POTONGAN-PIUTANG-NON-TUNAI` (ke-26, `FIN-DEC-058`), `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` (ke-27, `FIN-DEC-062`), `RETUR-PEMBELIAN` (ke-28, `FIN-DEC-061`), `PEMAKAIAN-DEPOSIT-RETUR` (ke-29, `FIN-DEC-061`) — lawan jurnal persis, urutan resmi katalog. Keempatnya dikirim dalam **satu** surat evidence — **SUDAH DIKIRIM 26 September 2026** (`evidence/07-usulan-empat-kode-potongan-retur-deposit-untuk-accounting.md`). Yang tersisa murni menunggu balasan Rizki. | Accounting (Rizki) | `DESIGN` — **hanya** menahan aktivasi worker pengiriman keempat kode ini (pola `FIN-DEC-056`), TIDAK menahan `EPIC FIN-15`/`FIN-17` masuk `/plan-module-delivery` |
| ~~`FIN-OQ-025`~~ | ~~Duplikasi endpoint aging Purchasing/AP.~~ **DITUTUP 25 September 2026** — endpoint `GET /purchasing/reports/aging` dicabut; layar memakai `GET api/finance/payable/aging` existing (`FIN-DEC-059`) | — | — |

## Langkah berikutnya

**Status akhir sesi 20 September 2026:** Scope pass dan Closure pass sama-sama selesai dalam
satu sesi. 23 keputusan (`FIN-DEC-001`..`023`) `approved`, mencakup:

- Seluruh 6 pertanyaan paket Rizki (Accounting) — siap dikirim balik sebagai jawaban resmi.
- Titik pengakuan fee dokter, kebijakan pra-finalisasi, kontrak Billing collection, perluasan
  employee benefit, autentikasi, cutover.
- Kepemilikan decision log Petty Cash (dipisah dari billing-kasir — `FIN-CQ-01` tertutup).
- Aturan inti AR: aging, alokasi, wewenang write-off (maker-checker), gerbang klaim.
- Aturan inti AP: master Supplier (reuse existing), trigger payable (manual MVP), pola
  pembayaran dokter (rekap periodik + `FinPaymentAllocation`), approval berjenjang (bentuk).
- Aturan inti Cash: employee benefit owner, validasi Bank Deposit, formula Daily Cash,
  reversal setelah alokasi, master Currency.

**Yang TIDAK memblokir lanjut ke desain:** `FIN-OQ-010` (angka ambang approval AP) dan
`FIN-OQ-011` (konfirmasi Accounting atas draf field subledger) — keduanya bisa diselesaikan
paralel saat `/design-business-module` berjalan, tidak perlu menghentikan pekerjaan.

Tidak ada Conflict yang tersisa. Tidak ada keputusan bisnis/klinis/finansial kritis yang masih
tertutup rapat. Opsi lanjutan ditawarkan ke owner — lihat pesan chat.

**Keputusan owner pada 20 September 2026:** jalankan `/trace-existing-capabilities` lebih
dahulu untuk membentuk `01-existing-capability-map.md` sebelum melanjutkan wawancara AR/AP/Cash
Management, karena field persis `BilArHandoff`/`BilApHandoff`/`BilTender`/Petty Cash API belum
diaudit secara canonical (lihat "Catatan cara kerja" di atas). Setelah capability map tersedia,
lanjutkan `/grill-me` (Closure pass) untuk menutup `FIN-OQ-002` s.d. `FIN-OQ-005`.

**Status 20 September 2026 (lanjutan):** `01-existing-capability-map.md` selesai dibentuk.
Temuan yang mengubah urutan Closure pass berikutnya:

- `FIN-CQ-01` (baru): keputusan bisnis/arsitektur Petty Cash (`PC-DEC-*`/`PC-DES-*`) ternyata
  sudah `approved` di blueprint **billing-kasir**, bukan di blueprint ini, walau kodenya sudah
  pindah folder ke `Corporate/FinanceManagement`. Perlu diputuskan lebih dulu: Cash Management
  pada blueprint ini merujuk ke billing-kasir untuk Petty Cash (tidak membuka ulang
  keputusannya), atau memindahkan pencatatannya ke sini.
- `FIN-OQ-004`/`FIN-OQ-005` (AR/AP) TIDAK punya keputusan tertunda serupa di blueprint lain —
  aman dilanjutkan sepenuhnya di blueprint ini.
- Field `BilArHandoff.DebtorType` dan `BillingHandoffStatuses` terverifikasi langsung dari
  source: hanya `PAYER`/`PATIENT_GUARANTOR` dan `CREATED`/`ACKNOWLEDGED`. Ini memperkuat
  `FIN-DEC-005` dan `FIN-DEC-006` sebagai perluasan yang benar-benar dibutuhkan, bukan sekadar
  asumsi dokumen.

Detail lengkap ada di `01-existing-capability-map.md`.

**Amendment pass 23 September 2026 — UI brief frontend `FE-FIN-*`.** `02-frontend-roadmap.md`
bagian 5 mendaftar enam keputusan wajib Product Owner yang menahan **seluruh** task `FE-FIN-*`
(`roadmap_status: ACTIVE_BLOCKED_ON_UI_BRIEF`). Owner menjawab sebagai Product Owner Finance
(Yasmin) pada sesi wawancara ini, dicatat `FIN-DEC-024`..`029` (lihat bagian `Frontend Decision
Authority` untuk ringkasan per keputusan, dan `Decision Log` untuk detail lengkap beserta
evidence). Ringkas:

1. Struktur rute `/finance/...` — ikuti pola existing (`FIN-DEC-024`).
2. Penamaan & pengelompokan menu `corporateFinance` — ikuti pola existing (`FIN-DEC-025`).
3. Bentuk penyajian umur piutang — summary card + tabel (`FIN-DEC-026`).
4. Layout rekonsiliasi shift — halaman penuh (`FIN-DEC-027`).
5. Bentuk koreksi & penghapusan — modal (`FIN-DEC-028`).
6. Cetak & unduh — ditunda dari MVP, lingkup detail dibuka sebagai `FIN-OQ-015` (`FIN-DEC-029`).

**Amendment pass 25 September 2026 — dampak balasan Accounting (`evidence/13-balasan-accounting-untuk-finance.md`) atas kejadian keuangan.** Sepuluh keputusan baru (`FIN-DEC-030`..`039`), tiga keputusan lama `superseded` (`FIN-DEC-004`, `FIN-DEC-007` sebagian, `FIN-DEC-023`), satu open question ditutup (`FIN-OQ-011`), dua dibuka (`FIN-OQ-016`, `FIN-OQ-017`). Ringkas:

1. **Perubahan perilaku yang SUDAH terkode** (`FIN-DEC-030`) — `HELD_FOR_FINALIZATION` diganti terbit segera sbg `PENERIMAAN-UANG-MUKA`. Ini SATU-SATUNYA keputusan pass ini yang menuntut perubahan source code existing (`FinanceReceiptService.cs`, `FinAccountingEventOutbox.cs`, `FinReceipt.cs`, `FinanceAccountingOutboxService.cs`), belum termasuk migration bila skema outbox perlu berubah.
2. **Empat kode kejadian baru diusulkan** sisi Finance (`FIN-DEC-031`, `032`, `034`, `035`) — seluruhnya masih menunggu ratifikasi balik Accounting (`FIN-OQ-017`), BELUM boleh dianggap final sepihak.
3. **Kelebihan bayar** ikut pola uang muka, TIDAK dapat kode sendiri (`FIN-DEC-033`).
4. **Bentuk saldo subledger** Accounting diterima apa adanya, menutup `FIN-DEC-023`/`FIN-OQ-011` (`FIN-DEC-035`).
5. **Syarat akun layanan** (bukan SuperAdmin, Departemen+Jabatan Receive-only, terikat badan hukum) diratifikasi; mekanisme token tetap terbuka ke Platform (`FIN-DEC-036`, `FIN-OQ-016`).
6. **Konflik cutover** ditutup: decision log (`FIN-DEC-008`) berlaku, referensi "1 Oktober 2026" dibatalkan (`FIN-DEC-037`).
7. **Components** dijawab `TOTAL` untuk seluruh jenis kejadian pada rilis ini (`FIN-DEC-038`).
8. Ratifikasi/catatan tanpa keputusan baru: `ACC-XMOD-0.3`, 17 kode existing, pengecualian `JASA_MEDIS`, `JournalNumber` opsional pada `201` (`FIN-DEC-039`).

**Yang TIDAK ditutup pass ini, dan MUST ditindaklanjuti sebelum kode ditulis:**

- Kirim balasan resmi ke Accounting (Rizki) berisi keputusan `FIN-DEC-031`..`039` sebagai jawaban evidence 13, termasuk usulan 4 kode baru untuk diratifikasi (`FIN-OQ-017`).
- `contracts/integration-contract.md` dan `contracts/state-transition-matrix.md` menjadi **stale** menurut pemicu impact-scan `blueprint-manifest.md` sendiri (`ACC-XMOD` naik dari `0.2` ke `0.3`, dan bagian 5.5 "Penahanan sebelum tagihan final" tidak lagi berlaku). Revisi kontrak ini BUKAN pekerjaan `/grill-me` — dilakukan lewat `/design-business-module` amendment pass sebelum `FIN-DEC-030` diimplementasikan.
- `blueprint-manifest.md` (`revision: 2`) MUST dinaikkan saat kontrak turunan direvisi, mengikuti pola amendment revisi sebelumnya.
- `FIN-OQ-016` (mekanisme token) dan `FIN-OQ-017` (ratifikasi 4 kode) TIDAK memblokir MVP-0/1/4, tapi memblokir gerbang G6 dan `EPIC FIN-11`/`FIN-12`.

**Tidak ada Conflict.** Keenam keputusan konsisten dengan pola `QuilvianSystemFrontendDev` yang
sudah berjalan nyata (dikutip per keputusan pada kolom Evidence), bukan preferensi baru.

**Yang TIDAK ditutup pass ini, sengaja:** status dependency backend `BE-FIN-004` pada
`01-backend-roadmap.md` (masih tercatat `SEBAGIAN` per laporan
`task/report/backend/BE-FIN-004.md` tertanggal 21 September 2026 — pengguna menyatakan pada
sesi lain migration dan update database sudah dijalankan, tetapi klaim itu **belum diverifikasi**
lewat laporan task yang diperbarui). Pass ini murni menutup blocker UI brief; status roadmap
frontend (`ACTIVE_BLOCKED_ON_UI_BRIEF`) dan dependency `BE-FIN-004` pada
`02-frontend-roadmap.md`/`01-backend-roadmap.md` **belum diperbarui** oleh skill ini — `grill-me`
tidak membuat/mengubah roadmap. Pembaruan roadmap dan verifikasi `BE-FIN-004` adalah langkah
terpisah.

**Amendment pass lanjutan 25 September 2026 — koreksi hasil impact scan `/design-business-module`.**
Pass `/design-business-module` yang mengikuti amendment di atas dihentikan di gerbang input:
impact scan read-only (backend SHA bergerak `09101d05` → `d6cdfaf9` sejak blueprint terakhir
diaudit) menemukan `FIN-DEC-032` dan `FIN-DEC-033` yang baru saja `approved` TIDAK dapat
diimplementasikan apa adanya — keduanya menggabungkan fakta bisnis yang lawan jurnalnya berbeda
ke dalam satu kode kejadian, kesalahan sejenis yang justru diperingatkan Accounting sendiri soal
`PENERIMAAN-KASIR` pada evidence 13. Owner memilih menutup koreksinya lebih dulu lewat pass ini
sebelum desain dilanjutkan. Lima keputusan baru (`FIN-DEC-040`..`044`), dua keputusan lama
`superseded` penuh (`FIN-DEC-032`, `FIN-DEC-033`), satu dilengkapi (`FIN-DEC-034`), satu open
question diperbarui (`FIN-OQ-017`, naik dari 4 ke 7 kode), satu dibuka (`FIN-OQ-018`), satu
catatan impact scan ditambahkan (`FIN-CAP-STALE-001`). Ringkas:

1. **Kode pemakaian dipersempit** (`FIN-DEC-040`) — `PEMAKAIAN-UANG-MUKA-DEPOSIT` kini KHUSUS
   pelunasan piutang (D Uang Muka Pasien/K Piutang), dipicu `BilDepositMovement.ALLOCATION`.
2. **Kode pengembalian dipisah** (`FIN-DEC-041`) — `PENGEMBALIAN-UANG-MUKA` baru, khusus
   pengembalian tunai (D Uang Muka Pasien/K Kas), dari `BilDepositMovement.RELEASE` ATAU
   `BilRefundCase.EXECUTED` dengan `SourceType ALLOCATION_EXCESS` saja. Refund kategori BILLING
   lain (SETTLEMENT/REFERRED_OUTPATIENT_ADMIN) SENGAJA di luar cakupan — `FIN-OQ-018`.
3. **Kelebihan bayar diakui belakangan, bukan di muka** (`FIN-DEC-042`) — kode baru
   `PENGAKUAN-KELEBIHAN-BAYAR` terbit saat `BilRefundableCredit` diakui (`ALLOCATION_EXCESS`),
   bukan seolah diterima sebagai uang muka sejak awal (itu tidak mungkin secara temporal).
4. **Pemicu selisih kas shift ditetapkan** (`FIN-DEC-043`) — saat `BilCashierShift.Status =
   REVIEWED`, `AccountingDate` = tanggal shift.
5. **Kode pembalikan uang muka ditambahkan** (`FIN-DEC-044`) — `PEMBALIKAN-PENERIMAAN-UANG-MUKA`,
   dipilih dari `EventTypeCode` penerimaan asli yang dibalik.
6. **Kabar baik:** TIDAK ada blocker kepemilikan data baru — Billing sudah memiliki seluruh
   entity deposit/refund/shift, dan Finance sudah punya akses baca lewat `ApplicationDbContext`
   yang sudah terdaftar. Tidak perlu handoff baru.

**Yang TIDAK ditutup pass ini, dan MUST ditindaklanjuti sebelum surat/kode berikutnya:**

- **Kirim surat koreksi ke Accounting.** `evidence/04-jawaban-atas-balasan-accounting.md` yang
  sudah terkirim hari ini HANYA menyebut empat kode versi sebelum koreksi ini — termasuk
  `PEMAKAIAN-UANG-MUKA-DEPOSIT` versi lama yang salah menggabungkan pemakaian dan pengembalian.
  Surat susulan MUST dikirim sebelum Accounting mulai menyusun aturan posting berdasarkan surat
  yang sudah usang, supaya Rizki tidak meratifikasi kode yang keliru.
- `FIN-OQ-018` (lawan jurnal refund BILLING non-kelebihan-bayar) tetap terbuka, `LATER SLICE`.
- `FIN-CAP-STALE-001` (`BilCollectionHandoff` sudah ada di `ApplicationDbContext`, berpotensi
  menggugurkan `blocking_questions.BILLING-COLLECTION-HANDOFF` pada `blueprint-manifest.md`)
  MUST diverifikasi lewat `trace-existing-capabilities` impact scan sebelum ditutup atau
  dipakai merencanakan MVP-2/3.
- Setelah surat koreksi terkirim, `/design-business-module` dapat dilanjutkan kembali untuk
  merevisi `contracts/integration-contract.md` dan `contracts/state-transition-matrix.md`.

**Amendment pass 25 September 2026 (ketiga) — rumpun BARU Purchasing/AP penuh + agregasi AR,
dipicu evidence `docs/module-blueprints/finance-management/Keuangan.md`.** Owner meminta
perbandingan gap antara `Keuangan.md` (registry 28 kemampuan AP/AR hasil analisis video sistem
rujukan — bukan sumber otoritatif, dipakai murni untuk deteksi gap) dan blueprint ini. Tiga
gap NYATA dikonfirmasi langsung ke source (`rg` menyeluruh atas `Areas/`): **nol** kapabilitas
Purchase Order/Goods Receipt/Tukar Faktur/Purchasing Invoice di backend mana pun; **nol**
konsep agregasi AR Invoice ke Company/Guarantor; **nol** entity potongan sisi penerimaan AR.
Tujuh keputusan baru (`FIN-DEC-045`..`051`), satu keputusan lama `superseded` penuh
(`FIN-DEC-015`), tiga scope item baru (`FIN-SC-008`..`010`), tiga open question baru
(`FIN-OQ-019`..`021`). Ringkas:

1. **Keputusan berdampak paling besar** (`FIN-DEC-045`): siklus Purchasing/AP PENUH dibangun
   DI DALAM Finance Management — owner memilih ini secara eksplisit setelah konsekuensinya
   disampaikan (Finance berubah dari pola konsumen-hilir menjadi pemilik-proses-hulu, satu-satunya
   rumpun di blueprint ini yang berpola begitu).
2. **PPN Masukan wajib terhubung Accounting sejak gelombang pertama** (`FIN-DEC-046`) — BEDA
   dari pola kode kejadian lain di blueprint ini yang boleh menyusul; ini genuinely memblokir
   `/plan-module-delivery` untuk EPIC Purchasing/AP sampai `FIN-OQ-020` terjawab Rizki.
3. **Deposit Retur terpisah dan fleksibel lintas invoice** (`FIN-DEC-047`), bukan koreksi
   langsung ke invoice asal — pola mirip `BilRefundableCredit` tapi arah uang berlawanan.
4. **AR Invoice Agregat jadi dokumen resmi** (`FIN-DEC-048`), `BilInvoice` pasien tetap ada
   sebagai rincian internal, `FinReceivable` TETAP jadi unit piutang internal — AR Invoice
   Agregat adalah lapisan baru DI ATAS `FinReceivable`, bukan pengganti.
5. **Potongan sisi AR berdiri sendiri dari `FinPaymentDeduction`** (`FIN-DEC-049`) karena arah
   aliran uang berlawanan; efeknya ke `OutstandingAmount` masih `FIN-OQ-021`.
6. **Approval PO/Invoice dua jenjang** (`FIN-DEC-050`), checkpoint TERPISAH dari approval
   pembayaran (`FIN-DEC-022`) — dua kebijakan ambang nominal AP yang sebaiknya diputuskan
   bersamaan (`FIN-OQ-019` + `FIN-OQ-010`), dicatat eksplisit supaya tidak saling bertentangan.
7. **Tukar Faktur adalah entity terpisah** (`FIN-DEC-051`) dari Purchasing Invoice — checkpoint
   dokumen mendahului checkpoint nilai final.

**Yang TIDAK ditutup pass ini, dan MUST ditindaklanjuti:**

- `FIN-OQ-020` (ratifikasi Accounting kode PPN Masukan) memblokir EPIC Purchasing/AP masuk
  `/plan-module-delivery` — MUST dikirim sebagai surat evidence terpisah ke Rizki sebelum
  desain rumpun ini dianggap siap, mengikuti pola `evidence/13`/`14`.
- `FIN-OQ-019` (ambang nominal PO/Invoice) dan `FIN-OQ-010` (ambang nominal pembayaran)
  sebaiknya dijawab BERSAMAAN oleh Finance Supervisor — tidak memblokir desain, tapi berisiko
  menghasilkan dua kebijakan ambang yang tidak konsisten bila dijawab terpisah.
- `FIN-OQ-021` (PPN Keluaran/Faktur Pajak pada AR Invoice Agregat) TIDAK memblokir `FIN-SC-008`
  (Purchasing/AP), tapi MUST dijawab sebelum `FIN-SC-009` (AR Invoice Agregat) didesain penuh —
  risiko kepatuhan pajak bila diasumsikan sepihak.
- **Ini adalah `/grill-me`, bukan `/trace-existing-capabilities` maupun `/design-business-module`.**
  Skill ini TIDAK memperbarui `01-existing-capability-map.md`, `02-backend-architecture.md`,
  `03-frontend-architecture.md`, `04-prd-to-mvp.md`, maupun roadmap — walau permintaan awal
  owner menyebut "tambahkan konfigurasi BE dan FE". Tiga gap di atas sudah cukup jelas dan
  terverifikasi langsung ke source (`rg` menyeluruh, nol hasil untuk seluruh istilah AP), tetapi
  audit kemampuan CANONICAL (mengunci SHA, memeriksa `MstSupplier` field demi field, menandai
  status `Reuse`/`Extend`/`Missing` per kemampuan sesuai kontrak bukti resmi) tetap milik
  `/trace-existing-capabilities` — bukan diasumsikan selesai oleh pencarian `rg` pada pass ini.

**Tidak ada Conflict yang tersisa** pada tujuh keputusan ini. Owner memilih opsi berdampak
lebih besar pada tiga dari empat pertanyaan berpasangan rekomendasi (`FIN-DEC-045`, `046`,
`050`) — dicatat apa adanya, bukan dipertanyakan ulang setelah dipilih.

**Closure pass lanjutan 25 September 2026 — menutup empat open question dari amendment di
atas.** Owner menjawab langsung: ambang approval AP Rp 50.000.000 (seragam, menutup
`FIN-OQ-019` **dan** `FIN-OQ-010` sekaligus — `FIN-DEC-052`); usulan kode PPN Masukan
`PPN-MASUKAN-PEMBELIAN` untuk dikirim ke Rizki (`FIN-DEC-053`, `FIN-OQ-020` dipersempit murni
menunggu ratifikasi); AR Invoice Agregat PPN-exempt (`FIN-DEC-054`, ditandai wajib ditinjau
ulang bila ada indikasi sebaliknya); potongan sisi AR mengurangi `OutstandingAmount` sebagai
pembayaran non-tunai, cermin `FIN-DES-028` (`FIN-DEC-055`). Empat keputusan: `FIN-DEC-052`..`055`.

**Satu penyederhanaan dicatat eksplisit, bukan disembunyikan:** `FIN-OQ-010` versi asli
mengangkat kemungkinan ambang BERBEDA per jenis transaksi (supplier/dokter/write-off). Jawaban
pass ini memakai satu ambang seragam. Bila pembedaan tetap diinginkan, itu amendment terpisah.

**Yang MASIH terbuka setelah closure pass ini, dan sifatnya:**

| ID | Status | Memblokir apa |
|---|---|---|
| `FIN-OQ-020` | Menunggu Rizki — surat **sudah terkirim** (`evidence/06-usulan-kode-ppn-masukan-untuk-accounting.md`, 25 September 2026) | EPIC Purchasing/AP masuk `/plan-module-delivery` |
| `FIN-OQ-017` | Menunggu Rizki — surat koreksi **sudah terkirim** (`evidence/05-koreksi-jawaban-atas-balasan-accounting.md`) | Gerbang G6, `EPIC FIN-11` |
| `FIN-OQ-016` | Menunggu Platform + Rizki | Gerbang cutover G3/G4 |
| `FIN-OQ-018` | Belum digali | `LATER SLICE`, tidak memblokir |
| `FIN-OQ-015` | Menunggu keputusan terpisah | `LATER SLICE`, tidak memblokir |

**Tidak ada yang memblokir `/trace-existing-capabilities`.** Seluruh keputusan bisnis untuk
rumpun `FIN-SC-008`/`009`/`010` sudah lengkap sejauh yang bisa diputuskan tanpa pihak luar.
Langkah berikutnya: audit kanonik kemampuan existing (`MstSupplier` field demi field, status
`Reuse`/`Extend`/`Missing` resmi) sebelum `/design-business-module` merancang entity baru.

---

## Closure pass — empat pertanyaan `/plan-module-delivery`, 25 September 2026

**Pemicu.** `/plan-module-delivery` membuka empat pertanyaan (`FIN-OQ-022`..`025`) saat
menurunkan `EPIC FIN-15`/`16`/`17` menjadi task — dua di antaranya (`FIN-OQ-023`, `024`)
menahan `BE-FIN-036`/`040` dan `FE-FIN-010`/`013`; satu (`FIN-OQ-022`) hanya menahan
pendaftaran butir menu; satu (`FIN-OQ-025`) menahan satu endpoint laporan.

**Keputusan baru:** `FIN-DEC-057`..`060`, seluruhnya `approved`, Yasmin, 25 September 2026.

| Pertanyaan | Ditutup oleh | Ringkasan |
|---|---|---|
| `FIN-OQ-022` | `FIN-DEC-060` | Submenu "Pembelian" untuk lima layar AP; Batch AR jadi butir flat "Tagihan Gabungan Penjamin"; Purchasing Invoice dilabeli "Faktur Pembelian" |
| `FIN-OQ-023` | `FIN-DEC-057` | Deposit Retur dipakai sebagai baris alokasi non-tunai di dalam `FinPayment`, memakai ulang kapasitas satu-pembayaran-banyak-utang yang sudah ada |
| `FIN-OQ-024` | `FIN-DEC-058` | Kode baru `POTONGAN-PIUTANG-NON-TUNAI` (ke-26) diusulkan; ratifikasi Rizki dicatat terpisah sebagai `FIN-OQ-026` |
| `FIN-OQ-025` | `FIN-DEC-059` | `GET /purchasing/reports/aging` dicabut dari kontrak; layar memakai endpoint aging existing |

**Satu open question baru:** `FIN-OQ-026` (ratifikasi Accounting atas `POTONGAN-PIUTANG-NON-TUNAI`)
— mengikuti pola `FIN-DEC-056`, hanya menahan aktivasi worker pengiriman kode itu, bukan
`EPIC FIN-17`. Suratnya **belum** dikirim; menyusul saat diminta eksplisit, seperti `evidence/06`.

**Blocker desain yang TERSISA, dan yang MENYUSUL.** `FIN-DEC-057` menutup keputusan bisnisnya,
tetapi membuka satu pekerjaan arsitektur yang **belum digambar**: `FinPaymentAllocation` dan/atau
`FinSupplierReturnDepositUsage` (`FIN-DES-038`, `02-backend-architecture.md` C.1-C.16) perlu
jalur eksplisit untuk membedakan alokasi "dari transfer bank" versus "dari Deposit Retur".
`BE-FIN-036` **MUST NOT** dimulai sebelum ini digambar ulang — ini pekerjaan
`/design-business-module`, bukan sesuatu yang boleh diimprovisasi saat implementasi.

**Acceptance criteria yang sudah dapat diuji dari closure pass ini:**

1. `FinPayment` yang seluruhnya dilunasi dari Deposit Retur MUST punya `NetTransferAmount = 0`
   dan tidak menuntut nomor bukti transfer (`FIN-DEC-057`; bandingkan `FIN-VAL-091` yang menolak
   `NetTransferAmount = 0` untuk pembayaran biasa — aturan itu **MUST NOT** diterapkan ke
   pembayaran yang sengaja seluruhnya non-tunai; perbedaan ini sendiri MUST digambar eksplisit).
2. Kejadian `POTONGAN-PIUTANG-NON-TUNAI` MUST NOT tertulis dengan `EventTypeCode = AR_PAYMENT`
   atau `PENYESUAIAN-PIUTANG` (`FIN-DEC-058`).
3. `GET /purchasing/reports/aging` MUST NOT ada di `FinancePurchasingReportsController`
   (`FIN-DEC-059`); satu-satunya endpoint aging AP tetap `GET api/finance/payable/aging`.
4. Butir menu Pembelian/Tagihan Gabungan Penjamin MUST NOT didaftarkan sebelum `FIN-DEC-060`
   diteruskan ke implementasi frontend — halaman itu sendiri boleh dibangun lebih dulu.

**Yang sengaja di luar scope pass ini:** menulis surat evidence ke Rizki untuk
`POTONGAN-PIUTANG-NON-TUNAI` (`FIN-OQ-026`) — menyusul sebagai instruksi terpisah, pola
`evidence/06`. Menggambar ulang skema `FinPaymentAllocation`/`FinSupplierReturnDepositUsage` —
itu `/design-business-module`, bukan `/grill-me`.

**Langkah berikutnya:** `/design-business-module` amendment kecil untuk menggambar konsekuensi
skema `FIN-DEC-057` (satu-satunya yang menyentuh entity), lalu roadmap `BE-FIN-036`/`FE-FIN-010`
dapat dibuka. `BE-FIN-040`/`FE-FIN-013` tetap ⛔ `FIN-OQ-026` sampai Rizki menjawab. Tidak ada
task lain yang tertahan closure pass ini — dua belas task revisi 4 lain tetap seperti
direncanakan.

---

## Closure pass — balasan Accounting atas evidence/04, 05, 06, 07 (evidence/14), 28 September 2026

**Pemicu.** Rizki (owner Accounting) membalas ketiga surat Finance sekaligus dalam
`docs/module-blueprints/accounting/evidence/14-balasan-accounting-atas-kode-finance-05-06-07.md`.
Dari dua belas kode yang diusulkan Finance: tujuh diratifikasi apa adanya, dua diratifikasi
bersyarat, dan tiga diminta dipecah karena satu kode membawa lebih dari satu lawan jurnal.
Surat itu juga membawa empat pelurusan wajib (bagian 5) dan tujuh pertanyaan balik (bagian 7).
Pass ini menjawab sembilan keputusan yang wewenangnya ada di tangan Yasmin sebelum surat balasan
berikutnya (`evidence/15`, belum ditulis) dikirim ke Rizki.

**Keputusan baru:** `FIN-DEC-063`..`071`, seluruhnya `approved`, Yasmin, 28 September 2026.

| ID | Pertanyaan | Keputusan | Owner | Evidence/dasar |
|---|---|---|---|---|
| `FIN-DEC-063` | Kejadian apa yang dikirim saat tender uang muka dibatalkan SESUDAH sebagian/seluruhnya dipakai melunasi piutang (pertanyaan 7.1 Accounting) | Kirim **dua** kejadian: kode baru `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (debit Piutang, kredit Uang Muka Pasien — cermin pembalik `PEMAKAIAN-UANG-MUKA-DEPOSIT`) sebesar porsi yang terpakai, DITAMBAH `PEMBALIKAN-PENERIMAAN-UANG-MUKA` (kode existing, debit Uang Muka Pasien, kredit Kas) sebesar nilai tender. Hasil bersih: debit Piutang, kredit Kas — persis seperti yang diminta Accounting, dan Uang Muka Pasien kembali nol. Tidak mengubah lawan jurnal kode manapun yang sudah ada | Yasmin | `evidence/14` bagian 7.1; cermin `FIN-DEC-040` |
| `FIN-DEC-064` | Nama final dua kode pecahan selisih kas shift (`evidence/14` bagian 3.3) | Terima usulan Accounting apa adanya: `SELISIH-KAS-KURANG` (kekurangan, debit beban selisih kas/kredit Kas) dan `SELISIH-KAS-LEBIH` (kelebihan, debit Kas/kredit pendapatan selisih kas), keduanya bernilai positif. Menggantikan `SELISIH-KAS-SHIFT` bertanda yang ditolak Accounting | Yasmin | `evidence/14` bagian 3.3 |
| `FIN-DEC-065` | Pemecahan kode potongan piutang (`evidence/14` bagian 3.5) — Accounting menolak `POTONGAN-PIUTANG-NON-TUNAI` satu kode (`FIN-DEC-058`) | Terima pemecahan jadi EMPAT kode dengan nama usulan Accounting: `POTONGAN-PPH23-PIUTANG` (debit PPh 23 dibayar di muka/kredit Piutang), `PEMBALIKAN-POTONGAN-PPH23-PIUTANG`, `POTONGAN-BIAYA-BANK-PIUTANG` (debit beban administrasi bank/kredit Piutang), `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG`. `POTONGAN-PIUTANG-NON-TUNAI` dan pembaliknya (`FIN-DEC-058`) **superseded** oleh keputusan ini — alasannya benar secara akuntansi: PPh 23 adalah aset (kredit pajak), biaya bank adalah beban, satu kode tidak boleh punya dua kemungkinan akun debit | Yasmin | `evidence/14` bagian 3.5; **supersedes** `FIN-DEC-058` |
| `FIN-DEC-066` | Rename `PEMAKAIAN-DEPOSIT-RETUR` (`evidence/14` bagian 3.6, saran tidak mengikat) | Ikuti saran, ganti nama menjadi `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` — menghindari kerancuan dengan `PEMAKAIAN-UANG-MUKA-DEPOSIT` (deposit pasien) yang sama-sama memakai kata "DEPOSIT" untuk konsep berbeda | Yasmin | `evidence/14` bagian 3.6 |
| `FIN-DEC-067` | Konfirmasi tiga syarat mengikat `PENGAKUAN-KELEBIHAN-BAYAR` (`evidence/14` bagian 3.2, pertanyaan 7.2) | Setuju ketiga syarat apa adanya: (1) lawan jurnal tetap debit Piutang/kredit Uang Muka Pasien; (2) kode ini HANYA terbit bila kelebihan lahir dari pembayaran yang mengkredit Piutang (`BilRefundableCredit.SourceType = ALLOCATION_EXCESS`); (3) kode ini **MUST NOT** terbit bila pembayaran asalnya `PENERIMAAN-UANG-MUKA` — kelebihannya sudah tercatat di Uang Muka Pasien, mengirim kode ini akan mencatat kewajiban dua kali | Yasmin | `evidence/14` bagian 3.2, 7.2; cermin `FIN-DEC-042` |
| `FIN-DEC-068` | Apakah kredit retur supplier bisa memuat PPN, dan kode apa untuk porsi PPN-nya (`evidence/14` bagian 3.6, 7.3 butir pertama) | Ya, retur barang ke supplier BISA membawa porsi PPN (konsisten dengan `PPN-MASUKAN-PEMBELIAN` yang sudah diusulkan Finance untuk pembelian barang yang sama). Diusulkan kode baru `PPN-MASUKAN-RETUR-PEMBELIAN` — cermin `PPN-MASUKAN-PEMBELIAN`: debit Piutang Retur Supplier, kredit akun yang sama dengan debit `PPN-MASUKAN-PEMBELIAN` | Yasmin | `evidence/14` bagian 3.6, 7.3 |
| `FIN-DEC-069` | Apakah supplier pernah mencairkan kredit retur secara TUNAI (`evidence/14` bagian 7.3 butir kedua) | Tidak pernah — kredit retur di Finance HANYA dipakai mengurangi pembayaran utang supplier berikutnya (`PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`/`FIN-DEC-047`). Tidak perlu kode baru untuk pencairan tunai | Yasmin | `evidence/14` bagian 7.3 |
| `FIN-DEC-070` | Bentuk kejadian penanda shift kasir tertutup, terbit saat `REVIEWED` (`evidence/14` bagian 4.1, 7.4) | Kode baru, `PENUTUPAN-SHIFT-KASIR`, murni penanda status tanpa lawan jurnal (pola sama dengan `SALDO-SUBLEDGER`) — terbit saat `BilCashierShift.Status` menjadi `REVIEWED`, `SourceTransactionId` memakai `BilCashVarianceReview.Id` (lihat `FIN-CAP-024`, lebih presisi daripada `BilCashierShift.Id`) | Yasmin | `evidence/14` bagian 4.1, 7.4 |
| `FIN-DEC-071` | Sifat akuntansi refund kategori `SETTLEMENT` dan `REFERRED_OUTPATIENT_ADMIN` (`BilRefundCase.SourceType`, `evidence/14` bagian 4.2, 7.6) | Diasumsikan defensif: kedua kategori BISA mengeluarkan kas tunai ke pasien — sikap fail-closed karena biaya kesalahan (rekonsiliasi toleransi-nol Accounting gagal diam-diam) jauh lebih mahal daripada biaya mengusulkan kode yang ternyata tidak pernah dipicu. Perlu kode kejadian baru (bentuk dan nama menyusul saat `/design-business-module`), masuk gerbang G6. **Catatan risiko:** ini asumsi defensif Finance, BUKAN konfirmasi faktual dari owner Billing — bila owner Billing kelak memastikan kedua kategori itu tidak pernah mengeluarkan kas, keputusan ini MUST ditinjau ulang sebagai amendment, bukan dianggap final selamanya | Yasmin | `evidence/14` bagian 4.2, 7.6; terkait `FIN-OQ-018` |

**Pelurusan bagian 5 (`evidence/14`) — diterima seluruhnya, bukan area yang butuh keputusan
bisnis:**

1. Kosongkan `Components` pada seluruh contoh pesan (bukan teks `"TOTAL"`) — kesalahan dokumentasi
   Finance sendiri, MUST diperbaiki di `contracts/integration-contract.md` sebelum worker
   pengiriman mana pun diaktifkan.
2. Hanya `SALDO-SUBLEDGER` yang boleh bernilai nol/negatif — `SELISIH-KAS-SHIFT` sudah dipecah
   (`FIN-DEC-064`) sehingga klaim `evidence/07` bagian 2.1 sudah tidak berlaku.
3. **Fakta terverifikasi langsung ke source**, bukan keputusan: `contracts/integration-contract.md`
   baris 296-300 mengonfirmasi dugaan Accounting tentang `EventTypeCode` pengganti nama pendek
   sudah **persis benar** — `AP_CREATED` → `PENGAKUAN-HUTANG-SUPPLIER`, `AP_PAYMENT` →
   `PEMBAYARAN-HUTANG-SUPPLIER`, `AR_PAYMENT` → `PENERIMAAN-PIUTANG`, `AR_WRITEOFF` →
   `PEMUTIHAN-PIUTANG`. Tidak ada keputusan baru yang dibutuhkan di sini.
4. Contoh angka `evidence/06` (Rp 11.000.000) salah ketik, seharusnya Rp 11.100.000 — perbaikan
   dokumentasi murni.

**Open question yang tertutup pass ini:**

| ID | Status baru | Alasan |
|---|---|---|
| `FIN-OQ-017` | **CLOSED** 28 September 2026 | Ketujuh kode kejadian uang muka/deposit/kas sudah tuntas sisi ratifikasi: lima diratifikasi apa adanya, satu (`PENGAKUAN-KELEBIHAN-BAYAR`) diratifikasi bersyarat dan syaratnya diterima (`FIN-DEC-067`), satu (`SELISIH-KAS-SHIFT`) diganti dua kode baru dan namanya diterima (`FIN-DEC-064`) |
| `FIN-OQ-020` | **CLOSED** sisi ratifikasi kode, 28 September 2026 | `PPN-MASUKAN-PEMBELIAN` diratifikasi Accounting (`evidence/14` bagian 2); akun debit persis masih menunggu G2 Accounting sendiri — itu bukan lagi open question Finance |
| `FIN-OQ-026` | **CLOSED**, digantikan `FIN-OQ-028` | `POTONGAN-PIUTANG-NON-TUNAI` ditolak sebagai satu kode; ratifikasi yang ditunggu sekarang adalah atas EMPAT kode pengganti (`FIN-DEC-065`) |
| `FIN-OQ-018` | Sisi keputusan bisnis **CLOSED** oleh `FIN-DEC-071` (asumsi defensif); sisi ratifikasi kode **OPEN** sebagai `FIN-OQ-031` | Accounting sudah minta jawaban lewat `evidence/14` bagian 7.6; jawabannya assumsi defensif "bisa keluar kas", bukan penutupan definitif |

**Open question baru — seluruhnya menunggu ratifikasi Accounting lewat surat `evidence/15`
(belum ditulis):**

| ID | Butuh | Menahan apa |
|---|---|---|
| `FIN-OQ-027` | Ratifikasi kode baru `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (`FIN-DEC-063`) | Aktivasi worker pengiriman kasus pembatalan uang muka terpakai. Tidak menahan `EPIC FIN-14` secara keseluruhan (pola `FIN-DEC-056`) |
| `FIN-OQ-028` | Ratifikasi empat kode potongan piutang pengganti (`FIN-DEC-065`) | Aktivasi worker pengiriman potongan piutang non-tunai. Tidak menahan `EPIC FIN-17` masuk `/plan-module-delivery` |
| `FIN-OQ-029` | Ratifikasi kode `PPN-MASUKAN-RETUR-PEMBELIAN` (`FIN-DEC-068`) | Aktivasi worker pengiriman retur pembelian yang membawa PPN |
| `FIN-OQ-030` | Ratifikasi kode `PENUTUPAN-SHIFT-KASIR` (`FIN-DEC-070`) | Penegakan `ACC-DEC-065` sisi Accounting (tutup bulan menahan shift yang belum ditutup) |
| `FIN-OQ-031` | Bentuk dan nama kode refund kas `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` (`FIN-DEC-071`) — BELUM diusulkan, perlu dirancang dulu lewat `/design-business-module` (`SourceTransactionId` mana yang dipakai, lawan jurnal) sebelum diajukan ke Accounting | Gerbang G6 milik Accounting; sebelumnya tercatat `FIN-OQ-018` |

**Blocker desain yang TERSISA, dan yang MENYUSUL.** Sembilan keputusan pass ini murni jawaban
tertulis dan penamaan kode — **belum ada** perubahan skema atau entity yang digambar. Dua hal
MUST dikerjakan `/design-business-module` sebelum implementasi:

1. Bentuk kejadian `PENUTUPAN-SHIFT-KASIR` dan kode refund `FIN-OQ-031` — keduanya kode baru yang
   belum pernah muncul di `02-backend-architecture.md`/`erd/accounting-integration.md`.
2. Titik tulis kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` di dalam alur pembatalan tender
   Billing (`FIN-DEC-021`) — perlu dipetakan ke source konkret tempat pembatalan tender uang muka
   ditangani.

**Acceptance criteria yang sudah dapat diuji dari closure pass ini:**

1. Pembatalan tender uang muka yang sudah terpakai sebagian/seluruhnya MUST menerbitkan KEDUA
   kejadian (`PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA`), tidak
   boleh hanya satu (`FIN-DEC-063`).
2. `PENGAKUAN-KELEBIHAN-BAYAR` MUST NOT diterbitkan bila kelebihan berasal dari pembayaran
   `PENERIMAAN-UANG-MUKA` (`FIN-DEC-067` syarat 3) — kondisi ini MUST diuji eksplisit di
   `testing/acceptance-test-matrix.md` sebelum worker diaktifkan.
3. Nama kode kejadian yang ditulis ke `FinAccountingEventOutbox` MUST memakai nama hasil pass ini
   (`SELISIH-KAS-KURANG`/`LEBIH`, `POTONGAN-PPH23-PIUTANG` dkk., `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`),
   BUKAN nama versi sebelumnya (`SELISIH-KAS-SHIFT`, `POTONGAN-PIUTANG-NON-TUNAI`,
   `PEMAKAIAN-DEPOSIT-RETUR`) yang sudah `superseded`.
4. `Components` MUST tidak ada sama sekali pada seluruh pesan kejadian transaksi (pelurusan #1) —
   pesan yang membawa field ini MUST ditolak sebelum dikirim, bukan menunggu Accounting menolaknya.

**Yang sengaja di luar scope pass ini:** menulis surat balasan `evidence/15` ke Rizki — menyusul
sebagai instruksi terpisah. Merancang skema `PENUTUPAN-SHIFT-KASIR` dan kode refund `FIN-OQ-031`
— itu `/design-business-module`, bukan `/grill-me`. Memverifikasi ke owner Billing apakah
`SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` benar-benar pernah mengeluarkan kas — `FIN-DEC-071`
sengaja memakai asumsi defensif supaya pass ini tidak tertahan pihak luar; verifikasi itu boleh
menyusul kapan saja sebagai amendment.

**Langkah berikutnya:** kirim surat balasan `evidence/15` ke Rizki memuat kesembilan keputusan di
atas dan usulan empat kode baru (`FIN-OQ-027`..`030`), lalu `/design-business-module` amendment
kecil untuk `FIN-OQ-031` dan titik tulis `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`. Tidak ada task
roadmap existing yang berubah status oleh pass ini — seluruh dampak baru murni menunggu ratifikasi
Accounting (pola `FIN-DEC-056`), konsisten dengan seluruh rumpun Accounting Integration sejauh ini.

---

## Amendment pass — menutup temuan `/trace-existing-capabilities` atas AMENDMENT REVISI 6, 28 September 2026

**Pemicu.** `/design-business-module` revisi 6 (28 September 2026, pass sebelumnya pada hari yang
sama) menemukan kedua SHA sudah bergerak jauh dan menuliskan `FIN-DES-051`..`058` beserta empat
temuan yang membatalkan asumsi `FIN-DEC-070`/`071`. `/trace-existing-capabilities` menyusul pada
hari yang sama, membaca langsung kontrak as-is endpoint penerima Accounting, dan menemukan bahwa
salah satu premis desain revisi 6 (`FIN-DES-054`, penanda shift bernilai nol) **akan ditolak**
kotak masuk Accounting sebagaimana dibangun hari ini (`FIN-CQ-04`), plus dua `Conflict` frontend
(`FIN-CQ-06`, `FIN-CQ-07`). Pass ini menutup lima keputusan yang menunggu owner: tiga pengakuan
atas koreksi berbasis bukti, satu jalan keluar untuk `FIN-CQ-04`, satu penyelesaian `FIN-CQ-06`.

**Keputusan baru:** `FIN-DEC-072`..`076`, seluruhnya `approved`, Yasmin, 28 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-072` | Pengakuan koreksi pemicu penanda shift tertutup (temuan A, `02-backend-architecture.md` `E.1`) | **Diterima.** `CLOSED` (tanpa selisih) **dan** `REVIEWED` (selisih disahkan) sama-sama menerbitkan `PENUTUPAN-SHIFT-KASIR`. `CLOSED_WITH_VARIANCE` dan `PERLU_TINDAK_LANJUT` tetap tidak menerbitkan apa pun — keduanya memang harus menahan tutup bulan. Ini koreksi atas kesalahan pemicu `FIN-DEC-070`, bukan keputusan baru: shift bersih terbukti berhenti di `CLOSED` dan tidak pernah mencapai `REVIEWED` (`CashierShiftService.CloseAsync` baris 550-552) | `02-backend-architecture.md` `FIN-DES-054`; **mengoreksi** `FIN-DEC-070` |
| `FIN-DEC-073` | Pengakuan koreksi kunci kejadian selisih kas (temuan B) | **Diterima.** `SourceTransactionId` kejadian `SELISIH-KAS-KURANG`/`LEBIH` dan `PENUTUPAN-SHIFT-KASIR` memakai `BilCashierShift.Id`, **bukan** `BilCashVarianceReview.Id`. Hanya baris review yang membawa shift ke `REVIEWED` yang menerbitkan kejadian; baris `NEEDS_FOLLOW_UP` tidak. Ini menutup risiko satu selisih terjurnal dua kali (satu shift bisa punya dua baris review bernilai sama) | `02-backend-architecture.md` `FIN-DES-053`; **mengoreksi** `FIN-DEC-070`/`FIN-CAP-024` |
| `FIN-DEC-074` | Pengakuan penyempitan cakupan refund kas (temuan D) | **Diterima.** Refund kredit `SETTLEMENT` **tidak** butuh kode baru — diperlakukan sebagai perluasan cakupan `PENGAKUAN-KELEBIHAN-BAYAR` dan `PENGEMBALIAN-UANG-MUKA` yang sudah ada, karena ekonominya identik `ALLOCATION_EXCESS` (uang yang benar-benar diterima melebihi tagihan). Hanya `REFERRED_OUTPATIENT_ADMIN` yang tetap `OPEN DECISION` menunggu Billing + Accounting | `02-backend-architecture.md` `FIN-DES-056`; **mempersempit** `FIN-DEC-071` |
| `FIN-DEC-075` | Jalan keluar untuk `FIN-CQ-04` — kotak masuk Accounting menolak `Amount = 0` pada kedua jalur (transaksi dan saldo) | **Lanjutkan desain penanda bernilai nol apa adanya** (`FIN-DES-054` tidak diubah) — pola yang sama dengan `FIN-DEC-056`: Finance membangun duluan, hanya **worker pengirimannya** yang digerbang. **Sekaligus kirim permintaan tertulis terpisah** ke Accounting agar kotak masuknya diperluas menerima `Amount = 0` khusus `PENUTUPAN-SHIFT-KASIR`/`PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, atau mengaktifkan jalur pesan saldo (`BE-ACC-P2-028`). **Dicatat eksplisit:** permintaan ini levelnya lebih besar daripada ratifikasi nama kode — ia meminta perubahan aturan validasi di source Accounting sendiri, bukan sekadar penamaan | `01-existing-capability-map.md` bagian 15.4 (`FIN-CQ-04`); membuka `FIN-OQ-035` |
| `FIN-DEC-076` | Penyelesaian `FIN-CQ-06` — label menu yang sudah dibangun ("Account Payable", "Purchase Order", dst.) tidak mengikuti `FIN-DEC-060` | **Minta frontend menyesuaikan ke `FIN-DEC-060`** — submenu "Pembelian" berlabel Indonesia, ditambah butir "Tagihan Gabungan Penjamin" yang belum ada. `FIN-DEC-060` **tidak** diubah. Keputusan yang sudah disetujui owner MUST ditegakkan, bukan dikalahkan diam-diam oleh implementasi yang mendahuluinya | `01-existing-capability-map.md` bagian 15.4 (`FIN-CQ-06`); menegaskan `FIN-DEC-060` |

**Open question yang ditutup pass ini:**

| ID | Status baru |
|---|---|
| `FIN-CQ-04` | **CLOSED** sisi keputusan bisnis oleh `FIN-DEC-075` — sisi ratifikasi teknis dilanjutkan sebagai `FIN-OQ-035` |
| `FIN-CQ-06` | **CLOSED** oleh `FIN-DEC-076` — implementasinya menjadi task frontend terpisah, bukan lagi open question |

**Open question baru:**

| ID | Butuh | Menahan apa |
|---|---|---|
| `FIN-OQ-035` | Persetujuan Accounting untuk memperluas kotak masuknya menerima `Amount = 0` untuk kedua kode penanda shift, ATAU mengaktifkan jalur pesan saldo (`BE-ACC-P2-028`) | Aktivasi worker pengiriman `PENUTUPAN-SHIFT-KASIR`/`PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`. **Tidak** menahan penulisan baris outbox-nya (pola `FIN-DEC-056`). Permintaan ini **belum** dikirim — levelnya perubahan aturan validasi Accounting, MUST dikirim sebagai surat evidence tersendiri, terpisah dari `evidence/15` yang berisi ratifikasi nama kode |

**Yang TIDAK ditutup pass ini, dan MUST ditindaklanjuti:**

- `FIN-DEC-076` menghasilkan **task frontend baru** (relabel menu + tambah butir "Tagihan Gabungan
  Penjamin") yang belum tercatat di roadmap manapun — MUST masuk `/plan-module-delivery` berikutnya
  sebagai task terpisah, murni relabel/restrukturisasi tanpa menyentuh endpoint.
- Surat evidence untuk `FIN-OQ-035` belum ditulis — menyusul sebagai instruksi terpisah dari
  `evidence/15`, karena sifat permintaannya berbeda (perubahan aturan validasi, bukan ratifikasi
  nama).
- `FIN-CQ-05` (klaim keliru pada `integration-contract.md` bagian 5.10.5 butir 1 tentang
  `Components`) **tidak** memerlukan keputusan owner — ini murni koreksi kalimat dokumen berbasis
  bukti (`null` ternyata diterima kotak masuk), dan akan dibetulkan saat `/design-business-module`
  berikutnya menyentuh berkas itu.

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. `PENUTUPAN-SHIFT-KASIR` MUST terbit untuk shift `CLOSED` **maupun** `REVIEWED` (`FIN-DEC-072`) —
   pengujian yang hanya mencakup jalur `REVIEWED` **tidak** memenuhi kontrak ini.
2. Kejadian selisih kas dan penanda shift MUST memakai `BilCashierShift.Id` sebagai
   `SourceTransactionId`, **tidak pernah** `BilCashVarianceReview.Id` (`FIN-DEC-073`).
3. Worker pengiriman `PENUTUPAN-SHIFT-KASIR`/`PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` MUST NOT diaktifkan
   sebelum `FIN-OQ-035` dijawab Accounting (`FIN-DEC-075`) — baris outbox-nya tetap boleh ditulis
   `PENDING`.
4. Menu Finance MUST menampilkan submenu "Pembelian" berlabel Indonesia sebelum rumpun Purchasing
   dinyatakan selesai frontend-nya (`FIN-DEC-076`).

**Langkah berikutnya:** `/design-business-module` amendment kecil untuk memperbarui
`02-backend-architecture.md`/`integration-contract.md` mengikuti `FIN-DEC-072`..`074` (sebagian
sudah tertulis sebagai `FIN-DES-053`/`054`/`056` pada pass sebelumnya di hari yang sama — amendment
ini tinggal menegaskan status `approved`-nya) dan membetulkan `FIN-CQ-05`, lalu tulis dua surat
evidence terpisah: `evidence/15` (ratifikasi nama kode, ke Rizki) dan surat baru untuk `FIN-OQ-035`
(permintaan perluasan validasi, ke Rizki juga tetapi levelnya berbeda). Task frontend relabel menu
menyusul lewat `/plan-module-delivery`.

---

## Amendment pass — Penutupan `FIN-OQ-034` atas Pembalikan Tender Top-Up Deposit di Billing, 28 September 2026

**Pemicu.** Permintaan perbaikan `evidence/17` dari Finance kepada modul Billing Kasir telah disahkan melalui sesi `/grill-me` (keputusan `BKC-DEC-128`..`131` pada `docs/module-blueprints/billing-kasir/00-interview-decisions.md`). Pass ini mencatat pengakuan sisi Finance atas solusi tersebut dan menutup `FIN-OQ-034`.

**Keputusan baru:** `FIN-DEC-077`, `approved`, Yasmin, 28 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-077` | Pengakuan solusi Billing atas pembalikan tender top-up deposit (`FIN-OQ-034` ditutup) | **Diterima penuh.** Billing mengesahkan `BKC-DEC-128`..`131`: membatalkan alokasi tagihan terlebih dahulu secara LIFO (sehingga tagihan kembali berstatus `FINAL` dan saldo deposit tidak minus), memulihkan saldo akun deposit sementara, lalu mencatat mutasi pembalikan top-up `REVERSAL`. Di `BilDepositMovement`, Billing mencatat mutasi `RELEASE` untuk alokasi yang dibatalkan dan mutasi `REVERSAL` untuk top-up yang ditarik. Finance akan mengonsumsi kedua mutasi itu lewat intake `DEPOSIT_MOVEMENT` (`FinBillingHandoffTypes.DepositMovement`) untuk menerbitkan pasangan kejadian akuntansi: `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA`. Pendeteksi intake error `FIN-VAL-142` kini terpenuhi faktanya | `evidence/17`; `BKC-DEC-128`..`131`; **menutup** `FIN-OQ-034` |

**Open question yang ditutup:**

| ID | Status baru |
|---|---|
| `FIN-OQ-034` | **CLOSED** sisi keputusan bisnis dan kesepakatan antar-modul oleh `BKC-DEC-128`..`131` dan `FIN-DEC-077` |

---

## Amendment pass — Penyelarasan Nama Resource Hak Akses (`FIN-CQ-08`), 28 September 2026

**Pemicu.** `/trace-existing-capabilities` bagian 16.3 menemukan enam controller lama
(`FinancePaymentsController` dan lima lainnya) memakai nama resource pendek pada
`[AccessPermission]` (`Payment`, `Receipt`, `Receivable`, `SupplierPayable`, `BillingIntake`,
`AccountingEvents`), berbeda dari `contracts/permission-audit-matrix.md` (`FinancePayment`, dst.)
dan dari tujuh controller Purchasing yang baru dibangun — yang sudah memakai nama penuh sejak awal.
Ditemukan pula `Finance.AP`/`Finance.AR` dipakai frontend menyaring menu tanpa pernah terdaftar di
kontrak hak akses sama sekali, memperbesar `FIN-OQ-036` (granularitas hak akses butir menu
Purchasing). Audit menandai ini `FIN-CAP-043` (`Repair`) dan `FIN-CQ-08`, dan meminta keputusan
owner bersama Security Owner sebelum peran disemai ke lingkungan mana pun.

**Keputusan baru:** `FIN-DEC-078`, `FIN-DEC-079`, seluruhnya `approved`, Yasmin, 28 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-078` | Arah penyelarasan nama resource — kode mengikuti kontrak, atau kontrak mengikuti kode? | **Kode mengikuti kontrak.** Keenam controller lama diubah memakai nama penuh (`FinancePayment`, `FinanceReceipt`, `FinanceReceivable`, `FinanceSupplierPayable`, `FinanceBillingIntake`, `FinanceAccountingEvent`), disamakan dengan `FIN-PERM-1.2` dan tujuh controller Purchasing yang sudah memakainya. **Konsekuensi eksplisit yang MUST ditindaklanjuti:** perubahan ini mengubah string permission pada endpoint yang sudah berjalan — setiap peran yang sudah disemai dengan nama pendek di lingkungan mana pun **MUST** ikut disesuaikan; ini bukan sekadar ganti nama di kode, melainkan MUST disertai migrasi data peran saat dieksekusi | `01-existing-capability-map.md` bagian 16.3 (`FIN-CQ-08`, `FIN-CAP-043`) |
| `FIN-DEC-079` | Penyelesaian `Finance.AP`/`Finance.AR` terhadap resource granular — menu memakai resource granular sendiri, atau `Finance.AP`/`AR` didaftarkan resmi? | **Didaftarkan sebagai resource resmi tingkat kelompok** di `permission-audit-matrix.md`. Peran yang diberi `Finance.AP` atau `Finance.AR` **otomatis dianggap memiliki seluruh resource granular** di bawah kelompoknya lewat seeder peran (mis. `Finance.AP` mencakup `FinancePayment`, `FinanceSupplierPayable`, `FinancePurchaseOrder`, `FinanceGoodsReceipt`, `FinanceInvoiceExchange`, `FinancePurchasingInvoice`, `FinanceSupplierReturn`). Frontend **tidak perlu diubah** — filter menu yang sudah memakai `Finance.AP`/`AR` tetap berlaku. Ini menutup `FIN-OQ-036` sepenuhnya: pengguna ber-`Finance.AP` akan benar-benar memiliki resource granular yang dituntut endpoint, sehingga tidak ada lagi butir menu yang mengarah ke endpoint yang menolaknya. **Konsekuensi yang MUST dijaga:** setiap resource granular baru yang ditambahkan ke kelompok AP/AR di masa depan **MUST** ikut ditambahkan ke pemetaan payung-ke-granular ini pada saat yang sama — pemetaan itu sendiri menjadi satu sumber kebenaran baru yang perlu dirawat | `01-existing-capability-map.md` bagian 16.3 (`FIN-CQ-08`); **menutup** `FIN-OQ-036` |

**Open question yang ditutup pass ini:**

| ID | Status baru |
|---|---|
| `FIN-CQ-08` | **CLOSED** oleh `FIN-DEC-078` dan `FIN-DEC-079` |
| `FIN-OQ-036` | **CLOSED** oleh `FIN-DEC-079` — diselesaikan lewat pemetaan payung-ke-granular, bukan lewat resource granular di frontend |

**Yang TIDAK ditutup pass ini, dan status terkininya (30 September 2026):**

- **Pemetaan payung-ke-granular** — ✅ **SELESAI 30 September 2026.** Ditulis ke
  `contracts/permission-audit-matrix.md` AMENDMENT REVISI 7 bagian D.3a (AP, 38 pasangan) dan D.3b
  (AR, 16 pasangan). Nama mengikuti `FIN-DEC-082` (`Finance.AP.Umbrella`/`Finance.AR.Umbrella`).
- **Migrasi data peran** — ✅ **SELESAI (`BE-FIN-042`).** Skrip
  `Migrations/scripts/be-fin-042-role-permissions-migration.sql` dua tahap, idempotent. Mekanisme
  aktual berbasis `SysAccessPolicy` — koreksi di D.6.1.
- Pemeriksaan mekanisme penyemaian peran — ✅ **SELESAI** lewat `/trace-existing-capabilities` bagian
  16.2 (ditemukan saat `BE-FIN-042`). Mekanisme berbasis atribut scan + `SysAccessPolicy`, bukan
  seeder statis. Skrip migrasi dua tahap menanganinya.

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. Keenam controller lama **MUST** memakai nama resource penuh yang sama persis dengan
   `permission-audit-matrix.md` sesudah task rename selesai — nol string `"Payment"`, `"Receipt"`,
   `"Receivable"`, `"SupplierPayable"`, `"BillingIntake"`, `"AccountingEvents"` (bentuk pendek)
   tersisa di `[AccessPermission(...)]` manapun pada modul Finance.
2. Peran yang diberi `Finance.AP` **MUST** dapat memanggil seluruh endpoint Purchasing dan
   Payment tanpa penolakan hak akses — diuji dengan minimal satu peran nyata sesudah pemetaan
   payung-ke-granular diterapkan.
3. `permission-audit-matrix.md` **MUST** memuat `Finance.AP` dan `Finance.AR` sebagai baris resmi,
   beserta daftar resource granular yang dicakupnya masing-masing.

**Langkah berikutnya:** `/design-business-module` amendment kecil untuk menggambar pemetaan
payung-ke-granular pada `permission-audit-matrix.md`, lalu `build-module-backend` untuk task rename
keenam resource **beserta** migrasi data perannya — dengan urutan verifikasi mekanisme penyemaian
peran lebih dulu lewat `/trace-existing-capabilities`, supaya migrasi datanya tepat sasaran.

---

## Amendment pass — Ratifikasi FIN-DES-064 / FIN-DES-065 dan Koreksi FIN-DEC-041, 29 September 2026

**Pemicu.** Pass perencanaan roadmap 29 September 2026 mencatat blocker pada `BE-FIN-047` (pemicu `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` tidak jelas). Audit source terkini pada commit `7811c048` menemukan bahwa mutasi `BilDepositMovement` bertipe `RELEASE` hanya ditulis oleh satu sumber (`BillingSettlementService.cs:949` via `BKC-DEC-131`) untuk pembatalan alokasi tagihan secara LIFO saat tender top-up dibalik. Pada proses ini, **tidak ada kas yang bergerak keluar**; saldo deposit pasien justru bertambah kembali dan tagihan terbuka kembali. Keputusan lama `FIN-DEC-041` yang memetakan mutasi `RELEASE` ke `PENGEMBALIAN-UANG-MUKA` (kredit Kas) terbukti keliru dan akan menyebabkan kas di buku besar berkurang secara fiktif serta menggagalkan rekonsiliasi toleransi nol Accounting (`ACC-DEC-076`). Arsitektur target telah dirancang di `02-backend-architecture.md` bagian H (`FIN-DES-064`, `FIN-DES-065`), dan pass ini mengesahkan koreksi bisnis `FIN-DEC-041` serta meratifikasi kedua keputusan desain tersebut bersama Product Owner (Yasmin).

**Keputusan baru:** `FIN-DEC-080`, `FIN-DEC-081`, seluruhnya `approved`, Yasmin, 29 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-080` | Pengesahan koreksi `FIN-DEC-041` dan ratifikasi `FIN-DES-064`: pemetaan mutasi `BilDepositMovement` bertipe `RELEASE` dan pemicu refund kas | **Diterima penuh.** <br>1. Mutasi `BilDepositMovement` bertipe `RELEASE` **dicabut** dari pemicu kejadian `PENGEMBALIAN-UANG-MUKA` (yang mengkredit Kas).<br>2. Mutasi `RELEASE` resmi **dipetakan sebagai pemicu kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`** (jurnal: Debit Piutang, Kredit Uang Muka Pasien — tagihan kembali terbuka dan deposit pasien kembali utuh tanpa pergerakan kas).<br>3. Pemicu kode kejadian `PENGEMBALIAN-UANG-MUKA` (Debit Uang Muka Pasien, Kredit Kas) **dipersempit murni** pada baris `BilRefundCase` berstatus `EXECUTED` (sumber `ALLOCATION_EXCESS` dan `SETTLEMENT` per `FIN-DEC-074`/`FIN-DES-056`), satu-satunya jalur di mana kas rumah sakit benar-benar diserahkan kembali kepada pasien. | Bukti source `BillingSettlementService.cs:949` (`BKC-DEC-131`), `02-backend-architecture.md` bagian H (`FIN-DES-064`); **mengoreksi parsial** `FIN-DEC-041` dan menutup blocker `BE-FIN-047` |
| `FIN-DEC-081` | Ratifikasi `FIN-DES-065`: aturan intake mutasi `RELEASE`, mitigasi *fail-closed*, dan tindak lanjut ke Owner Billing | **Diterima penuh.** <br>1. **Aturan Intake:** Kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` **hanya diterbitkan** jika mutasi `RELEASE` berpasangan dengan mutasi `REVERSAL` yang memiliki `SettlementId` yang sama (tanda pasti pembatalan alokasi LIFO dari `BKC-DEC-131`).<br>2. **Mitigasi Fail-Closed:** Jika ditemukan mutasi `RELEASE` tanpa pasangan `REVERSAL` yang bersesuaian, baris intake dicatat berstatus **`ERROR`** (nol kejadian akuntansi diterbitkan, pesan kesalahan menunjuk `FIN-OQ-037`). Finance **dilarang keras menebak** lawan jurnalnya (apakah kas atau piutang).<br>3. **Surat Evidence Billing:** Finance resmi menerbitkan surat evidence tersendiri (`evidence/19` / `FIN-OQ-037`) kepada Owner Billing untuk meminta penanda eksplisit (rekomendasi: mengisi kolom existing `ReversesMovementId` dengan `ALLOCATION` movement ID yang dibatalkan, atau membuat `MovementType` baru). | `02-backend-architecture.md` `FIN-DES-065`; `FIN-VAL-145`; **membuka tindak lanjut** `FIN-OQ-037` |

**Dampak pada keputusan yang sudah ada:**

- `FIN-DEC-041` (`approved`, 25 September 2026): **Dikoreksi parsial.** Butir (a) yang memetakan `BilDepositMovement` `RELEASE` ke `PENGEMBALIAN-UANG-MUKA` resmi **dicabut**. Pemicu `PENGEMBALIAN-UANG-MUKA` kini murni `BilRefundCase` berstatus `EXECUTED` (sumber `ALLOCATION_EXCESS` dan `SETTLEMENT`).
- `FIN-DES-064` dan `FIN-DES-065`: Status arsitektur berubah dari `draft` menjadi **`approved`**.
- `BE-FIN-047`: Blocker pemicu resmi **terbuka**; task dapat direncanakan dan diimplementasikan.

**Open question yang dibuka/ditindaklanjuti:**

| ID | Pihak Dituju | Kebutuhan | Status | Dampak / Menahan Apa |
|---|---|---|---|---|
| `FIN-OQ-037` | Owner Billing | Permintaan penanda eksplisit pada mutasi `RELEASE` versi pembatalan alokasi (Opsi A direkomendasikan: mengisi `ReversesMovementId` menunjuk mutasi `ALLOCATION` yang dibatalkan; atau Opsi B: enum `MovementType` baru). | **CLOSED** (29 September 2026) | Disetujui dan ditutup penuh oleh Owner Billing melalui `BKC-DEC-132` (adopsi Opsi A `ReversesMovementId`), `BKC-DEC-133` (granularitas 1-ke-1 per alokasi yang dibatalkan), dan `BKC-DEC-134` (penyelarasan `BillingDepositService`). |

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. Mutasi `BilDepositMovement` bertipe `RELEASE` yang berpasangan dengan `REVERSAL` ber-`SettlementId` sama **MUST** menerbitkan kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, dan **MUST NOT** menerbitkan `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-080`).
2. Mutasi `RELEASE` tanpa pasangan `REVERSAL` ber-`SettlementId` sama **MUST** menghasilkan baris intake berstatus `ERROR` dengan pesan menunjuk `FIN-OQ-037`, serta menerbitkan **nol** baris kejadian outbox akuntansi (`FIN-DEC-081`).
3. Seluruh mutasi `RELEASE` pada satu periode akuntansi **MUST** menghasilkan **nol** kejadian `PENGEMBALIAN-UANG-MUKA`. Kode `PENGEMBALIAN-UANG-MUKA` **hanya** terbit bila ada `BilRefundCase` berstatus `EXECUTED` (`FIN-DEC-080`).

**Langkah berikutnya:** Perbarui dokumen arsitektur `02-backend-architecture.md` (mengubah status `FIN-DES-064` dan `FIN-DES-065` menjadi `approved`), sinkronkan `blueprint-manifest.md` dan kontrak integrasi/validasi, susun surat evidence `FIN-OQ-037` untuk Owner Billing (`evidence/19`), lalu lanjutkan penyesuaian roadmap pada `roadmap/01-backend-roadmap.md` untuk membuka blokir task `BE-FIN-047`.

---

## Amendment pass — Koreksi bentrok nama `Finance.AP`/`Finance.AR` pada `FIN-DEC-079`, 29 September 2026

**Pemicu.** Saat mengerjakan `BE-FIN-042` (implementasi `FIN-DEC-078`/`079`), ditemukan bahwa nama
resource `Finance.AP` dan `Finance.AR` yang didaftarkan `FIN-DEC-079` sebagai resource payung
**baru** ternyata **sudah dipakai** dua controller yang sungguh-sungguh berjalan hari ini:
`FinanceApController` (route `api/finance/payable`, `[AccessController(..., ControllerName =
"Finance.AP", ...)]`) dan `FinanceArController` (route `api/finance/receivable`, `ControllerName =
"Finance.AR"`) — keduanya "V2" endpoint AP/AR dengan aksi nyata (`View`, `Payment`, dst.) yang sama
sekali berbeda dari aksi payung yang dirancang `FIN-DEC-079` (`View`→`Read`, `Operate`→Maker,
`Approve`→Checker pada 9/4 resource granular).

**Yang membuat ini bukan sekadar kelalaian yang baru terjadi:** `FIN-DEC-059` (25 September 2026,
tiga hari SEBELUM `FIN-DEC-079`) sudah menyebut eksplisit "`FinanceApController` yang sudah
berjalan" saat menutup `FIN-OQ-025`. Audit yang mendasari `FIN-DEC-079`
(`01-existing-capability-map.md` bagian 16.3) tidak menyilangkan temuan itu dengan keputusan
`FIN-DEC-059` yang sudah ada di decision log yang sama — dua controller yang sama sekali tidak
tersebut sepanjang closure pass `FIN-CQ-08`. Membangun mekanisme ekspansi persis seperti dirancang
`FIN-DEC-079` akan **menimpa arti** resource yang sudah dipegang dua endpoint nyata itu: siapa pun
yang hari ini diberi `Finance.AP : View` untuk memakai `FinanceApController` akan otomatis juga
memegang 9 resource granular Purchasing begitu mekanisme ekspansi diaktifkan, tanpa keputusan admin
mana pun.

**Temuan kedua yang ikut ditemukan (arsitektural, bukan sekadar penamaan):** `Seeders/AccessMenuSeeder.cs`
secara eksplisit **tidak pernah** menulis `SysAccessPolicy` (baris grant) — komentar kelasnya sendiri
menyatakan "Kemampuan yang baru terdaftar tetap ditolak untuk semua orang sampai admin memberikannya
lewat layar Akses Role." Mekanisme "seeder secara otomatis mendistribusikan ke tabel izin peran" yang
diminta `FIN-DEC-079` bertentangan langsung dengan invariant itu, dan tidak punya preseden di seluruh
codebase — modul `platform-authorization` (BE-SEC-003B/012/013/014) yang paling dekat menanganinya
justru melakukan hal sebaliknya (memecah satu identitas granular jadi banyak identitas granular baru,
lewat migrasi data yang ditinjau manusia per pasangan Departemen+Posisi — bukan payung→granular
otomatis).

**Keputusan baru:** `FIN-DEC-082`, `approved`, dijawab langsung repository owner (mewakili Yasmin),
29 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-082` | Resolusi bentrok nama `Finance.AP`/`Finance.AR` antara payung baru (`FIN-DEC-079`) dan controller V2 yang sudah berjalan | **Payung memakai nama baru; `FinanceApController`/`FinanceArController` tidak disentuh sama sekali.** Nama payung diusulkan `Finance.AP.Umbrella` dan `Finance.AR.Umbrella` (provisional — dapat diganti Security Owner tanpa mengubah keputusan intinya) untuk membedakannya tegas dari resource V2 yang sudah ada. Dipilih di atas dua alternatif: (b) me-rename V2 controller — ditolak karena menuntut migrasi data peran tambahan di luar cakupan `BE-FIN-042` dan menyentuh kode yang sudah berjalan tanpa keperluan langsung; (c) menggabungkan satu nama untuk dua arti — ditolak karena mencampur dua audiens akses yang berpotensi berbeda (pengguna endpoint V2 vs pemegang payung Purchasing/AR) di balik satu tombol yang sama | Temuan `BE-FIN-042` §7 (laporan task); kontradiksi internal dengan `FIN-DEC-059` yang sudah mencatat `FinanceApController` berjalan |

**Dampak pada keputusan yang sudah ada:**

- `FIN-DEC-079` (`approved`, 28 September 2026): **Dikoreksi parsial.** Nama resource payung yang
  didaftarkan bukan lagi `Finance.AP`/`Finance.AR`, melainkan `Finance.AP.Umbrella`/
  `Finance.AR.Umbrella` (nama pasti menunggu konfirmasi Security Owner). Seluruh isi lain
  `FIN-DEC-079` (pemetaan ke 9/4 resource granular, matriks pewarisan aksi) **tetap berlaku apa
  adanya** — yang berubah murni nama payungnya, bukan cakupan atau pewarisan aksinya.
- `FIN-OQ-036`: **Dibuka kembali sebagian.** `FIN-DEC-079` sebelumnya mengklaim ini closed dengan
  alasan "frontend tidak perlu diubah — filter menu yang sudah memakai `Finance.AP`/`AR` tetap
  berlaku." Klaim itu **tidak lagi benar** bila payung berganti nama: filter menu frontend (bila
  memang membaca resource `Finance.AP`/`Finance.AR` untuk visibilitas menu Purchasing/AR, BUKAN
  untuk visibilitas menu V2) **MUST** diarahkan ke nama payung yang baru. Ini **MUST diverifikasi**
  lewat `/trace-existing-capabilities` pada `src/utils/menu-sidebar/corporateFinance.js` — belum
  diperiksa pass ini apakah filter itu benar-benar menyaring untuk menu Purchasing/AR atau untuk
  menu V2 itu sendiri (dua kemungkinan yang secara kebetulan memakai string yang sama sebelum
  pass ini).
- `03-frontend-architecture.md` §15.4 (mengklaim FIN-CAP-040/FIN-OQ-036 tertutup tuntas): **MUST
  ditinjau ulang** mengikuti koreksi di atas.

**Keputusan kedua:** `FIN-DEC-083`, `approved`, dijawab langsung repository owner (mewakili Yasmin),
29 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-083` | Titik teknis ekspansi payung→granular: materialized saat grant, atau live saat setiap request? | **Materialized saat grant diberikan admin.** Saat admin memberi `Finance.AP.Umbrella`/`Finance.AR.Umbrella : View/Operate/Approve` ke satu Departemen+Posisi lewat layar Akses Role, sistem menulis baris `SysAccessPolicy` eksplisit untuk seluruh resource granular yang dicakup pada saat itu juga — pola yang sama dengan `Migrations/scripts/be-sec-003b-policy-expansion.sql` yang sudah terbukti di modul `platform-authorization`. **Konsekuensi yang MUST dijaga:** mencabut payung TIDAK otomatis mencabut granularnya — pencabutan granular MUST jadi langkah eksplisit terpisah, konsisten dengan pola migrasi hak akses lain di codebase ini (nol cascade otomatis). **Dipilih di atas opsi live/runtime** karena opsi itu mengubah algoritma otorisasi inti (`AccessPermissionService.HasAccessAsync`/`GetEffectivePermissionsAsync`) yang dipakai SELURUH modul aplikasi, bukan hanya Finance — risiko regresi jauh lebih luas daripada nilai tambah sinkronisasi otomatisnya | Preseden `be-sec-003b-policy-expansion.sql` (BE-SEC-003B); nol perubahan pada algoritma otorisasi inti |

**Dampak lanjutan `FIN-DEC-083`:** titik tulis konkret ("saat admin memberi grant lewat layar Akses
Role") berarti perubahan terjadi di `RoleAccessController.cs` (atau service di baliknya) — bukan di
`AccessMenuSeeder.cs`, yang tetap murni registry seperti sebelumnya, konsisten dengan invariant
"seeder tidak pernah menulis `SysAccessPolicy`" yang sudah didokumentasikan.

**Langkah berikutnya:** `/design-business-module` amendment kecil untuk menggambar mekanisme
`FIN-DEC-082`/`083` secara konkret: nama final payung, titik tulis di `RoleAccessController.cs`
(alur "beri grant payung → materialize N baris granular" dan "cabut payung → **tidak** ikut mencabut
granular, dicatat sebagai perilaku yang disengaja"), serta pembaruan `permission-audit-matrix.md`
§D.2/§D.6.2 supaya sesuai keputusan ini (bukan lagi seeder). Sesudah itu, verifikasi ulang filter
menu `corporateFinance.js` lewat `/trace-existing-capabilities` untuk memastikan nama payung baru
dipakai di tempat yang benar, baru `build-module-backend` mengimplementasikan mekanismenya sebagai
task terpisah dari `BE-FIN-042` (yang §D.5/§D.6.1-nya sudah selesai dan berdiri sendiri).

> **Ditindaklanjuti 29 September 2026.** `/design-business-module` revisi 11 sudah dijalankan dan
> menghasilkan `FIN-DES-066`..`069` (`draft`). Pass itu menemukan satu kendala mekanis yang tidak
> terduga — lihat Amendment pass berikutnya di bawah.

---

## Amendment pass — Pembawa resource payung (`FIN-OQ-038`), 29 September 2026

**Pemicu.** `/design-business-module` revisi 11 (`FIN-DES-066`) menemukan kendala mekanis yang
menghentikan implementasi `FIN-DEC-082`/`083`: sebuah pasangan `(resource, action)` hanya dapat
diberikan admin bila ia **terdaftar** di registry, dan registry hanya mengenal dua sumber —
pemindaian atribut pada controller nyata, atau `[assembly: AccessExplicitPermission]`. Jalur kedua
**tidak dapat dipakai membuat resource baru**: dokumentasi atributnya menyatakan `ResourceName` wajib
menunjuk resource yang sudah terdaftar dari pemindaian endpoint, dan penanda yang menunjuk resource
tak dikenal **ditolak keras** saat registry disusun. Akibatnya **resource payung yang tidak memiliki
satu pun endpoint tidak dapat didaftarkan dengan mekanisme platform hari ini.**

**Fakta kedua yang disampaikan sebelum keputusan diambil, karena ia mengubah bobot pertanyaannya.**
Pass desain yang sama membuktikan alasan asli payung dibuat **tidak terbukti**. `FIN-OQ-036` dibuka
karena dikhawatirkan staf yang berhak melihat menu akan ditolak `403` oleh endpoint. Impact scan
frontend (`a31da3c21`) menunjukkan butir menu yang dijaga `Finance.AP`/`Finance.AR` seluruhnya milik
layar V2 dan sudah konsisten dengan endpointnya, sementara butir menu Purchasing belum pernah ada.
`FIN-DES-069` menetapkan butir menu baru memakai resource granular, bukan payung. Dengan begitu
**satu-satunya manfaat payung yang tersisa adalah kemudahan admin** — mencentang satu kotak, bukan
tiga belas. Manfaat itu nyata, tetapi bobotnya jauh lebih kecil daripada "mencegah `403`".

**Keputusan baru:** `FIN-DEC-084`, `approved` untuk sisi Finance, dijawab langsung repository owner
(mewakili Yasmin), 29 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-084` | Pembawa resource payung: perluas platform, buat controller pembawa di Finance, atau batalkan payung? | **Perluas platform (opsi D).** `platform-authorization` diperluas supaya sebuah modul dapat mendeklarasikan resource yang **dapat diberikan tetapi tidak dijaga endpoint mana pun**, lewat deklarasi opt-in yang eksplisit. Penjaga anti-typo yang ada sekarang (penanda menunjuk resource tak dikenal ditolak keras) **MUST dipertahankan** untuk kasus normal — yang ditambah hanya jalan sah untuk menyatakan "resource ini memang sengaja tanpa endpoint". **Dipilih di atas dua alternatif:** (C) controller pembawa milik Finance — ditolak karena endpoint yang keberadaannya terutama untuk membawa resource adalah jebakan jangka panjang: peninjau di kemudian hari wajar menganggapnya endpoint mati lalu menghapusnya, yang diam-diam mematikan seluruh pemberian hak lewat payung; (E) membatalkan payung sama sekali — tidak dipilih walaupun premis aslinya gugur, karena kemudahan admin tetap dinilai bernilai | `02-backend-architecture.md` AMENDMENT REVISI 11 (`FIN-DES-066`); dokumentasi `Attributes/AccessExplicitPermissionAttribute.cs` |

**Batas wewenang yang MUST dicatat apa adanya, dan inilah yang membuat keputusan ini belum tuntas.**
`FIN-DEC-084` adalah keputusan **sisi Finance**: Finance memilih jalan keluar dan **meminta**
perluasan itu. Kode yang diubah milik `platform-authorization`, dan pemiliknya adalah Security Owner
bersama pemilik modul itu — **bukan** Yasmin. Konsisten dengan pola yang sudah berjalan di blueprint
ini untuk permintaan lintas modul (`FIN-DEC-053` ke Accounting, `FIN-OQ-034`/`FIN-OQ-037` ke Billing),
jawaban owner Finance **tidak** dihitung sebagai persetujuan modul lain.

**Open question yang ditutup dan yang dibuka:**

| ID | Status baru | Keterangan |
|---|---|---|
| `FIN-OQ-038` | **CLOSED** oleh `FIN-DEC-084` | Pertanyaannya "pembawa payung yang mana" — sudah dijawab: perluasan platform |
| `FIN-OQ-039` | **DIBUKA** — ditujukan kepada Security Owner + pemilik `platform-authorization`. Surat permintaan **sudah ditulis**: `evidence/20-permintaan-perluasan-registry-resource-tanpa-endpoint.md` (29 September 2026) | Persetujuan dan penjadwalan perluasan registry supaya resource tanpa endpoint dapat dideklarasikan. **Menahan:** implementasi mekanisme ekspansi payung. **TIDAK menahan:** `BE-FIN-042` (`D.5` rename enam controller dan `D.6.1` skrip migrasi `SysAccessPolicy`) yang sudah selesai dan berdiri sendiri, maupun pekerjaan frontend mana pun |

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. Sesudah perluasan platform tersedia, `Finance.AP.Umbrella` dan `Finance.AR.Umbrella` **MUST**
   muncul sebagai baris yang dapat dicentang pada layar Akses Role, masing-masing dengan tepat tiga
   aksi (`View`, `Operate`, `Approve`).
2. Penanda yang menunjuk resource tak dikenal **MUST tetap ditolak keras** sesudah perluasan —
   perluasan ini menambah jalan sah, bukan melemahkan penjaga yang ada.
3. Memberi `Finance.AP.Umbrella : View` kepada satu Departemen x Posisi **MUST** menghasilkan baris
   `SysAccessPolicy` granular `Read` pada seluruh sembilan resource AP, dalam satu transaksi yang
   sama dengan penyimpanan payungnya.

**Dua dokumen disinkronkan status gerbangnya pada pass ini** — terbatas pada penunjuk keputusan,
bukan penulisan desain baru: `02-backend-architecture.md` bagian `I.5` dan
`contracts/permission-audit-matrix.md` bagian D.6.2 butir 7. Keduanya sebelumnya masih menyajikan
`FIN-OQ-038` sebagai pilihan yang belum diambil; sekarang keduanya menunjuk `FIN-DEC-084` dan
membawa `FIN-OQ-039` sebagai gerbang penggantinya.

**Langkah berikutnya:** ~~susun surat evidence kepada Security Owner + pemilik
`platform-authorization`~~ — **SUDAH DIKERJAKAN 29 September 2026**:
`evidence/20-permintaan-perluasan-registry-resource-tanpa-endpoint.md`, mengikuti pola surat
`evidence/17` dan `evidence/19` ke owner Billing. Surat itu menyampaikan permintaan beserta **dua**
hal yang sengaja tidak disembunyikan: (a) alasan asli payung sudah terbukti gugur sehingga manfaat
yang tersisa murni kemudahan admin, dan (b) dua alternatif yang dapat ditempuh Finance **tanpa**
melibatkan modul lain, beserta alasan penolakannya. Surat itu juga menyatakan jawaban "ditolak atau
ditunda" **dapat diterima sepenuhnya** — Finance akan mencabut rencana payung dan tetap memakai
pemberian hak granular. Yang tersisa sekarang: menunggu jawaban penerima.

---

## Closure pass — Tindak Lanjut Gap evidence/14, Migrasi Skema Database DBeaver, dan Arsitektur In-Process Worker Pengiriman EPIC FIN-12, 30 September 2026

**Pemicu.** Evaluasi kritis atas dokumen `accounting/evidence/14-balasan-accounting-atas-kode-finance-05-06-07.md` (ratifikasi, pemecahan kode, dan pertanyaan balik Accounting), penutupan gap teknis yang masih tertunda di Finance, penyediaan skrip migrasi database fisik, serta perancangan mekanisme pengiriman antrean kejadian keuangan ke modul Accounting (`EPIC FIN-12`).

**Keputusan baru:** `FIN-DEC-085` sampai `FIN-DEC-089`, seluruhnya `approved` sisi Finance, diputuskan interaktif via `/grill-me`, 30 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-085` | Ke arah mana fokus penyelesaian gap atas evidence/14 Accounting diprioritaskan? | **Fokus menyelesaikan gap teknis dan pekerjaan implementasi di sisi Finance** — memprioritaskan pemaketan eksekusi migrasi skema database fisik (tabel `FinReceiptDeduction` revisi 5, kolom `PPNAmount` retur supplier, kolom `DepositAppliedAmount` pembayaran) dan perancangan arsitektur worker pengiriman outbox (`EPIC FIN-12`). | Wawancara `/grill-me`, 30 September 2026; tindak lanjut `accounting/evidence/14` dan `finance/evidence/15` |
| `FIN-DEC-086` | Bagaimana metode penyediaan dan eksekusi skrip migrasi skema database untuk BE-FIN-041, BE-FIN-038, dan BE-FIN-043? | **Gabungkan ke dalam satu berkas skrip SQL idempotent kompatibel DBeaver** di `Migrations/scripts/be-fin-041-038-043-schema-migration-dbeaver.sql`, dibungkus dalam satu transaksi (`START TRANSACTION ... COMMIT`) lengkap dengan pemeriksaan dan pencatatan riwayat `__EFMigrationsHistory` ('9.0.18') serta kueri verifikasi skema di `verify-be-fin-041-038-043-schema.sql`. Tidak menjalankan `dotnet ef database update` langsung ke database operasional. | `Migrations/scripts/README.md`; wewenang keselamatan database `AGENTS.md` |
| `FIN-DEC-087` | Mekanisme pemanggilan apa yang digunakan oleh Worker Pengiriman Outbox (EPIC FIN-12) untuk mengirim kejadian ke Accounting? | **In-Process Service Invocation via DI** — `FinanceAccountingDeliveryWorker` memanggil `AccAccountingEventService.TerimaAsync` secara langsung di dalam proses dengan GUID akun layanan Finance. Pola ini dipilih karena kedua modul berada dalam satu aplikasi monolit ASP.NET Core (`NewQuilvianSystemBackend`), menghindari ketergantungan loopback HTTP/jaringan lokal, latensi serialization jaringan, dan tetap mematuhi isolasi layer melalui Service boundary. | Arsitektur monolit `NewQuilvianSystemBackend`; `AccAccountingEventService.cs` baris 414 |
| `FIN-DEC-088` | Bagaimana konfigurasi penjadwalan dan kebijakan coba ulang (retry policy) untuk FinanceAccountingDeliveryWorker? | **Polling berkala tiap 30 detik tanpa batas coba ulang permanen** — worker terus mencoba baris outbox yang gagal tanpa pernah memindahkan statusnya secara permanen ke `FAILED`. Setiap percobaan gagal mencatat entri jejak di `FinAccountingEventAttempt` dan menaikkan `AttemptCount`. **Penyaringan gerbang:** baris outbox yang terkena gerbang bisnis (seperti tagihan belum final `RequiresFinalization = true` atau kedua kode penanda shift kasir bernilai nol `FIN-OQ-035` yang menunggu penyesuaian validasi Accounting) **dilewati (skipped)** secara cerdas tanpa menaikkan hitungan gagal. | Kebijakan operasional fail-closed Finance; `FIN-DES-059` |
| `FIN-DEC-089` | Dari mana FinanceAccountingDeliveryWorker memperoleh identitas (UserId) akun layanan yang sah saat memanggil layanan Accounting? | **Konfigurasi `appsettings.json` dengan fallback otomatis** — GUID akun layanan dibaca dari konfigurasi `FinanceIntegration:ServiceAccountUserId`. Jika tidak dikonfigurasi, worker secara defensif melakukan pencarian otomatis akun pengguna sistem (berdasarkan username `FINANCE_SERVICE_ACCOUNT` atau fallback ke akun `SuperAdmin` aktif di database) untuk memastikan worker tetap dapat beroperasi di lingkungan pengujian/dev. | `FIN-DEC-036`; `Program.cs` seeder pattern |

**Open question yang terpengaruh pass ini:**

| ID | Status baru | Keterangan |
|---|---|---|
| `FIN-OQ-016` | **CLOSED sisi arsitektur Finance** oleh `FIN-DEC-087` & `FIN-DEC-089` | Mekanisme autentikasi akun layanan Finance diselesaikan secara internal menggunakan in-process DI invocation dengan Service Account GUID dari konfigurasi/fallback. Tidak lagi memblokir perancangan dan implementasi `FinanceAccountingDeliveryWorker`. |
| `FIN-OQ-035` | **TETAP MEMBLOKIR** aktivasi pengiriman penanda shift | Sesuai `FIN-DEC-088`, worker akan melewati baris penanda shift tertutup bernilai nol sampai Accounting meratifikasi dan menyesuaikan validasi kotak masuknya (`evidence/16`). |

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. Skrip SQL `be-fin-041-038-043-schema-migration-dbeaver.sql` MUST dapat dieksekusi secara idempotent di DBeaver tanpa menimbulkan galat duplikasi constraint atau tabel.
2. Eksekusi skrip migrasi MUST menambahkan tepat tiga entri ke tabel `__EFMigrationsHistory` untuk migration `20260928120000_AddDepositAppliedAmountToFinPayment`, `20260929120000_AddArInvoiceBatchAndReceiptDeduction`, dan `20260929130000_AddPPNAmountToFinSupplierReturn`.
3. `FinanceAccountingDeliveryWorker` MUST membaca `FinAccountingEventOutbox` berstatus `PENDING` setiap interval 30 detik dan memanggil `AccAccountingEventService.TerimaAsync`.
4. Baris outbox dengan `EventTypeCode` penanda shift kasir (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`) MUST dilewati (tetap `PENDING`, `AttemptCount` tidak bertambah) selama gerbang `FIN-OQ-035` belum dibuka.
5. Percobaan pengiriman yang berhasil MUST mengubah status baris outbox menjadi `ACKNOWLEDGED` dan mencatat `FinAccountingEventAttempt` berstatus `SUCCESS`.

**Langkah berikutnya:**
1. Eksekusi skrip `be-fin-041-038-043-schema-migration-dbeaver.sql` di DBeaver oleh pengguna.
2. Implementasikan task backend `FinanceAccountingDeliveryWorker` (`EPIC FIN-12`).

---

## Closure pass — Tindak Lanjut Susulan Accounting evidence/15: Aturan Saldo Subledger dan Perilaku Nomor Jurnal Tanda Terima, 30 September 2026

**Pemicu.** Surat susulan dari Rizki, owner Accounting (`accounting/evidence/15-susulan-accounting-aturan-saldo-dan-nomor-jurnal.md`), bertanggal 30 September 2026 mengenai empat aturan pesan saldo subledger (`ACC-XMOD-0.4` §8a) dan perilaku pergantian nomor jurnal pada tanda terima (`ACC-DEC-116`..`119`).

**Keputusan baru:** `FIN-DEC-090` sampai `FIN-DEC-093`, seluruhnya `approved` sisi Finance, diputuskan interaktif via `/grill-me`, 30 September 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-090` | Bagaimana Finance akan memenuhi kewajiban pengiriman saldo subledger untuk 4 control account (Kasir, Kas Kecil, Piutang, Hutang) tiap akhir periode (Butir 15.1)? | **Bangun service kalkulasi snapshot saldo akhir bulan (MVP slice) untuk 4 control account dan kirim 4 event SALDO-SUBLEDGER otomatis ke outbox setiap akhir periode.** Layanan snapshot bulanan ini (`BE-FIN-049`) menghitung posisi kas pas shift kasir, sisa brankas kas kecil, outstanding piutang, dan outstanding utang supplier per tanggal akhir bulan. Bila ada control account yang tidak memiliki transaksi atau bersaldo nihil, Finance tetap menerbitkan kejadian `SALDO-SUBLEDGER` dengan `Amount = 0.00` sesuai `ACC-DEC-108`. | Wawancara `/grill-me`, 30 September 2026; `accounting/evidence/15` Butir 15.1 (`ACC-DEC-108`) |
| `FIN-DEC-091` | Terkait aturan saldo normal akun pada Butir 15.2 (ACC-DEC-109), bagaimana validasi nominal (Amount) untuk SALDO-SUBLEDGER ditegakkan di FinanceAccountingOutboxService? | **Kunci validasi agar Amount untuk SALDO-SUBLEDGER wajib >= 0 (menolak nominal negatif).** Seluruh 4 control account dikirim dalam nilai saldo normal positif (termasuk akun bersaldo normal kredit seperti Utang Supplier Rp 300 juta dikirim `300000000.00`, bukan minus) atau `0.00`. Validasi `request.Amount < 0` pada `FinanceAccountingOutboxService.cs` dikunci mutlak melempar pengecualian untuk seluruh kode tanpa dispensasi (`BE-FIN-048`). | Wawancara `/grill-me`, 30 September 2026; `accounting/evidence/15` Butir 15.2 (`ACC-DEC-109`) |
| `FIN-DEC-092` | Mengenai Butir 15.3 (ACC-DEC-110), bagaimana Finance menegakkan validasi AccountingDate dan menetapkan jadwal operasional penerbitan saldo akhir bulan? | **Validasi keras bahwa AccountingDate wajib tepat hari terakhir periode, dan jadwal terbit ditetapkan otomatis setiap tanggal 1 bulan berikutnya pukul 00:05 dini hari WIB.** `FinanceAccountingOutboxService` memvalidasi bahwa `AccountingDate` sama persis dengan hari terakhir bulan pada `SubledgerBalance.AccountingPeriodCode` (misal periode `2026-09` wajib `2026-09-30`). Jadwal operasional penerbitan otomatis dilakukan pada tanggal 1 pukul 00:05 WIB guna mengunci posisi akhir bulan sebelum Accounting memulai penutupan buku. | Wawancara `/grill-me`, 30 September 2026; `accounting/evidence/15` Butir 15.3 (`ACC-DEC-110`) |
| `FIN-DEC-093` | Terkait Butir 15.4 (ACC-DEC-116 s/d 119), apakah Finance membutuhkan mekanisme rutin untuk memperbarui nomor jurnal terkini dari Accounting, dan bagaimana Finance memperlakukan status tanda terima? | **Konfirmasi bahwa Finance cukup menyimpan nomor jurnal saat tanda terima awal, AccountingEventId adalah rujukan tetap, dan EventStatus 'Gagal'/'Diabaikan' diakui sebagai respon bisnis yang sah.** Finance tidak membutuhkan mekanisme sinkronisasi/polling rutin nomor jurnal terbaru karena lifecycle modul Finance berjalan otonom. Kolom `AccountingReceiptNumber` memegang `AccountingEventId` sebagai immutable reference, sedangkan `AccountingJournalNumber` dicatat murni untuk keperluan informasi awal. | Wawancara `/grill-me`, 30 September 2026; `accounting/evidence/15` Butir 15.4 (`ACC-DEC-116`..`119`) |

**Dampak Roadmap & Langkah Berikutnya:**
1. Kirim surat balasan resmi ke Accounting: `evidence/21-balasan-finance-atas-aturan-saldo-dan-nomor-jurnal.md` (selesai dibuat).
2. Tambahkan task backend `BE-FIN-048` (pengetatan validasi `FinanceAccountingOutboxService`) dan `BE-FIN-049` (layanan snapshot kalkulasi saldo subledger bulanan) ke `01-backend-roadmap.md`.

---

## Amendment pass — Penyelarasan Menu ke Sistem Produksi V1 (Ayat Silang & Manajemen Klaim), 1 Oktober 2026

**Pemicu.** Owner membandingkan menu Keuangan pada sistem produksi V1 (`QuilvianSystemFrontendDev1` branch `master`, remote `DevBenari/QuilvianSystemFrontendDev`, dan `QuilvianSystemBackendDev1`, remote `DevBenari/QuilvianSystemBackendDev` — repository terpisah, sudah berjalan produksi dan disetujui UAT, branding "Metropolitan Medical Centre") dengan menu `finance` pada sistem governed (`QuilvianSystemFrontendDev` branch `yasmina`), lewat empat tangkapan layar menu "Transaksi A/R" dan "Transaksi A/P". Sebelum pass ini, `trace-existing-capabilities` (4 agent paralel, dilaporkan di percakapan yang sama) sudah memverifikasi bahwa dari puluhan layar V1, hanya **dua kapabilitas** yang benar-benar tidak punya rujukan bisnis nyata di sistem manapun (V1 maupun governed): **Ayat Silang** dan **Manajemen Klaim**. Sisanya sudah tercakup (langsung atau terkonsolidasi) di `finance/`, atau sengaja dimiliki modul lain (Accounting untuk COA/GL/Jurnal; Administrator untuk Master Bank/Supplier; Health Services untuk Master Tarif; Medical Fee untuk Jasa Medis AP per `FIN-OQ-012`/`013`).

**Keputusan baru:** `FIN-DEC-094` sampai `FIN-DEC-098`, seluruhnya `approved`, diputuskan interaktif via `/grill-me`, 1 Oktober 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-094` | Bagaimana bentuk dan penempatan halaman menu Transaksi A/R dan Transaksi A/P pada sistem governed, menyusul owner menunjukkan menu V1 yang sudah UAT-approved? | **Supersedes `FIN-DEC-060`.** Seluruh layar Transaksi A/R dan Transaksi A/P MUST mengikuti bentuk dan penempatan halaman persis seperti V1 — termasuk memecah kembali kapabilitas yang sebelumnya dikonsolidasikan governed (Canceled Invoice, Pemutihan Piutang, Piutang Korporat, Receiveable AR Canceled, laporan AR per jenis, Penerima Pesanan/GR) menjadi halaman/rute berdiri sendiri, bukan modal/tab di dalam layar lain. Submenu "Pembelian" (`FIN-DEC-060`) **dicabut** sebagai struktur pengelompokan; label dan susunan flat mengikuti V1 apa adanya. | Jawaban eksplisit owner, 1 Oktober 2026, atas pilihan "pecah jadi halaman berdiri sendiri" vs "tautkan ke layar konsolidasi yang ada" |
| `FIN-DEC-095` | Apa batas scope "Manajemen Klaim" — sisi keuangan (pelacakan status pelunasan) atau lifecycle verifikasi dokumen klaim penuh? | **Sisi keuangan saja.** Finance hanya melacak status pelunasan klaim per payer (asuransi/BPJS): diajukan, disetujui (sebagian/penuh), disengketakan, dibayar (sebagian/lunas). Verifikasi dokumen medis/SEP/kelengkapan administrasi klaim **tetap milik Billing/Casemix** — Finance tidak membangun ulang atau menduplikasi proses itu, hanya membaca/menautkan hasilnya bila diperlukan di masa depan (titik sentuh, bukan kepemilikan). | Jawaban eksplisit owner, 1 Oktober 2026, atas opsi yang direkomendasikan |
| `FIN-DEC-096` | Apakah Ayat Silang (field legacy `AsuransiId`/`BankId`/`TotalPembayaran`/`IsSudahTerpakai`) butuh kapabilitas baru, atau sudah tercakup mekanisme yang ada? | **Sudah tercakup alokasi penerimaan (`FE-FIN-004`/`BE-FIN-016`..`018`).** Satu setoran bank gabungan dari asuransi = satu `FinReceipt`, dialokasikan manual ke banyak `FinReceivable` memakai mekanisme alokasi yang sudah dibangun. Ayat Silang di V1 adalah nama lama untuk proses yang secara fungsional sama. **Tidak ada kapabilitas backend/frontend baru yang dibangun** — menutup gap yang tercatat di `01-existing-capability-map.md` sebagai `MISSING`/unresolved sejak awal proyek; cakupannya murni menampilkan butir menu "Ayat Silang" yang mengarah ke layar alokasi penerimaan yang sudah ada. | Jawaban eksplisit owner, 1 Oktober 2026, atas opsi yang direkomendasikan |
| `FIN-DEC-097` | Apa bentuk data "Manajemen Klaim" — entity baru, atau perluasan kapabilitas yang sudah ada? | **Perluasan `FinReceivableInvoiceBatch`/`FinanceReceivableInvoiceBatchesController` (`BE-FIN-038`/`039`, `FE-FIN-012`) — bukan entity baru.** Satu klaim = satu batch tagihan gabungan penjamin yang sudah ada, ditambah field status lifecycle: **Diajukan → Diverifikasi Payer → Disetujui (sebagian/penuh) → Dibayar Sebagian/Lunas → Ditutup**. Saat payer menyetujui nominal LEBIH KECIL dari yang ditagih, selisihnya **dicatat sebagai item terpisah yang memerlukan write-off manual** oleh staf AR memakai mekanisme write-off yang sudah ada (`FinanceReceivablesController` write-off, `FE-FIN-002`) — **tidak** otomatis terhapus/disesuaikan oleh sistem. | Jawaban eksplisit owner, 1 Oktober 2026, atas opsi "5 status + selisih jadi piutang tak tertagih (write-off manual)" |
| `FIN-DEC-098` | Siapa berwenang mengubah status klaim, terutama menandai "Disengketakan"/"Disetujui Sebagian"? | **Staf AR biasa, manual, tanpa jenjang approval tambahan** — sama dengan hak akses yang sudah dipasang pada `FinanceReceivableInvoiceBatch` (`FE-FIN-012`). Tidak ada maker-checker terpisah untuk perubahan status klaim, berbeda dari pola approval berjenjang pembayaran AP/AR. | Jawaban eksplisit owner, 1 Oktober 2026, atas opsi yang direkomendasikan |

**Open question yang ditutup pass ini:**

| ID | Status baru | Keterangan |
|---|---|---|
| — | **Ayat Silang** (tercatat `MISSING`/unresolved di `01-existing-capability-map.md` sejak audit awal, belum pernah diberi nomor `FIN-OQ` formal) | **DITUTUP** oleh `FIN-DEC-096` — bukan kapabilitas baru, tercakup alokasi penerimaan yang sudah ada |

**Penilaian bentuk blueprint.** Kedua kapabilitas (Ayat Silang, Manajemen Klaim) **tidak** memenuhi syarat pemisahan sub-modul (`COMPOSITE`) — Ayat Silang bukan kapabilitas berdiri sendiri sama sekali (`FIN-DEC-096`), dan Manajemen Klaim adalah perluasan field/status di atas aggregate yang sudah ada (`FIN-DEC-097`), tanpa bounded context, resource RBAC, atau MVP wave sendiri. Blueprint `finance-management` tetap `SINGLE`.

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. Menu sidebar Transaksi A/R dan Transaksi A/P MUST menampilkan seluruh butir sesuai daftar V1 (lihat tangkapan layar owner, 1 Oktober 2026), dengan label dan urutan mengikuti V1 apa adanya, kecuali Jasa Medis (`FIN-OQ-012`/`013`, tetap di luar scope Finance).
2. Setiap butir yang sebelumnya adalah modal/tab di dalam layar konsolidasi (Canceled Invoice, Pemutihan Piutang, Piutang Korporat, Receiveable AR Canceled, Report-* AR per jenis, Penerima Pesanan) MUST punya rute/halaman sendiri yang dapat diakses langsung dari menu, bukan hanya dari dalam layar lain.
3. Ayat Silang MUST mengarah ke layar alokasi penerimaan yang sudah ada (`FE-FIN-004`) — nol endpoint/model baru.
4. `FinReceivableInvoiceBatch` MUST memiliki field status klaim baru dengan lima nilai (`FIN-DEC-097`) dan dapat diubah staf AR tanpa approval tambahan (`FIN-DEC-098`).
5. Selisih nominal saat status "Disetujui Sebagian" MUST tercatat sebagai item terpisah yang memerlukan write-off manual — bukan pengurangan `OutstandingAmount` otomatis.

**Langkah berikutnya:** owner akan diminta memilih kelanjutan di bawah (`design-business-module` untuk menggambar skema field status + state-transition-matrix amandemen `FinReceivableInvoiceBatch`, lalu `plan-module-delivery` untuk memecah seluruh pekerjaan ini — restrukturisasi menu, halaman berdiri sendiri, field status klaim baru — menjadi task `FE-FIN-xxx`/`BE-FIN-xxx` bernomor).



---

## Addendum — `FIN-OQ-040` terjawab, dan jawabannya membuka gerbang baru, 1 Oktober 2026

**Jawaban owner.** Grup "Umur Piutang (A/R Aging)" pada menu V1 berisi **tiga** butir:
**Kasir**, **Parkir**, dan **Tenant**.

| ID | Status baru | Keterangan |
|---|---|---|
| `FIN-OQ-040` | **CLOSED** | Isi grupnya terjawab: Kasir, Parkir, Tenant |
| `FIN-OQ-043` | **DIBUKA — MEMBLOKIR dua dari tiga butir itu** | Piutang Parkir dan Tenant **tidak dapat diwakili** model piutang yang berjalan sekarang. Lihat di bawah |

### Mengapa jawaban ini tidak langsung dapat dirancang

Pemeriksaan source dilakukan sebelum menulis baris desain apa pun, dan hasilnya menghentikan dua
dari tiga butir itu:

| Temuan | Bukti |
|---|---|
| `FinReceivable` **hanya** mengenal tiga jenis debitur: `PAYER`, `PATIENT_GUARANTOR`, `EMPLOYEE_BENEFIT` | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` baris 80-85 |
| Setiap piutang **wajib** berasal dari serah terima tagihan Billing — ada `SourceHandoffKey`, `SourceHandoffId`, dan `InvoiceId` yang menunjuk `BilInvoice` | `FinReceivable.cs` baris 25-31 |
| Tidak ada jalur apa pun untuk menerbitkan piutang yang **bukan** tagihan pasien | Konsekuensi langsung dua baris di atas |
| Endpoint umur piutang **tidak punya saringan segmen sama sekali** — hanya `AsOfDate` | `FinanceReceivableDtos.cs` baris 112-115 |
| Layar Parkir dan Tenant di V1 **murni `generateDummyData()`**, nol panggilan API — sama seperti Manajemen Klaim | `src/components/view/Keuangan/transaksi-AR/tagihan/{parkir,tenant}/table/services/*.service.jsx` |

Artinya Parkir dan Tenant bukan "saringan yang belum dipasang", melainkan **sumber piutang yang
belum ada sama sekali**. Sewa lahan parkir dan sewa unit tenant adalah penagihan berulang atas
kontrak sewa — bukan tagihan pasien, tidak lahir dari Billing, dan menuntut data induk yang belum
dimiliki modul mana pun di sistem governed.

### Bentuk bisnis yang terbaca dari data contoh V1

Dicatat sebagai **petunjuk**, bukan sebagai aturan yang sudah sah — sumbernya data contoh yang
di-hardcode, bukan sistem yang berjalan:

| Hal | Parkir | Tenant |
|---|---|---|
| Objek yang disewakan | Area parkir (Basement A1/A2/B1/B2, Outdoor, Karyawan) | Unit (Lt.1 A-01, Lt.2 B-12, dst.) |
| Penyewa | — (tidak terbaca) | Nama tenant (kantin, ATM, optik, apotek, laboratorium) |
| Pola tagihan | Bulanan atau Tahunan | Bulanan atau Tahunan |
| Nominal contoh | Rp 2.500.000 bulanan; Rp 37.000.000 tahunan | Bervariasi per unit |
| Keterlambatan | Ada layar "Tagihan Keterlambatan" terpisah untuk keduanya | idem |

### `FIN-OQ-043` — yang MUST diputuskan sebelum dua butir ini dirancang

| # | Pertanyaan | Mengapa agent tidak boleh menjawabnya sendiri |
|---:|---|---|
| 1 | Apakah penagihan sewa parkir dan tenant **milik modul Finance**, atau modul pengelolaan properti/konsesi tersendiri yang hasilnya mengalir ke Finance sebagai piutang? | Pertanyaan kepemilikan bounded context — sama bentuknya dengan `FIN-DEC-095` untuk Manajemen Klaim. Menjawabnya sendiri berarti menetapkan batas modul tanpa wewenang |
| 2 | Data induk apa yang dibutuhkan: kontrak sewa, objek sewa (area/unit), tarif, masa berlaku? Siapa pemiliknya? | Menebak berarti membuat master baru yang mungkin sudah dimiliki modul lain |
| 3 | Bagaimana tagihan berulang diterbitkan — otomatis tiap periode, atau diterbitkan petugas? Siapa yang menyetujui? | Aturan bisnis dan kewenangan |
| 4 | Aturan denda keterlambatan: dihitung bagaimana, sejak kapan, siapa yang boleh membebaskannya? | V1 punya layarnya, tetapi isinya data contoh — nol aturan yang bisa dirujuk |
| 5 | Apakah piutang sewa masuk `FinReceivable` dengan jenis debitur baru, atau entity piutang tersendiri? | Keputusan skema yang menyentuh invariant "setiap piutang berasal dari Billing" |

**Yang TIDAK terhambat.** Butir **Kasir** dan seluruh isi `EPIC FIN-18` lainnya berjalan terus.
Umur piutang per segmen Kasir hanya menuntut satu saringan tambahan pada endpoint umur piutang yang
sudah ada — bukan sumber piutang baru.

---

## Amendment pass — Penutupan `FIN-OQ-043`: Piutang Non-Pasien (Parkir & Tenant), 1 Oktober 2026

**Pemicu.** `FIN-OQ-043` (dibuka pass desain hari yang sama) menahan dua butir menu "Umur Piutang —
Parkir" dan "— Tenant" karena penagihan sewa parkir dan unit tenant bukan tagihan pasien, tidak
lahir dari Billing, dan model `FinReceivable` yang berjalan tidak dapat menampungnya. V1 pun tidak
punya aturan bisnis yang bisa dirujuk — layarnya murni data contoh (`generateDummyData()`).

**Keputusan baru:** `FIN-DEC-099` sampai `FIN-DEC-104`, seluruhnya `approved`, diputuskan interaktif
via `/grill-me`, 1 Oktober 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-099` | Penagihan sewa parkir dan sewa unit tenant milik modul mana? | **Milik Finance sepenuhnya.** Finance merancang sendiri pencatatan tagihan sewa — tidak ada modul properti/konsesi terpisah di sistem ini saat ini, tidak ditunggu kehadirannya | Jawaban eksplisit owner, 1 Oktober 2026, atas opsi yang direkomendasikan |
| `FIN-DEC-100` | Bagaimana tagihan berulang disusun — dari kontrak sewa master, atau dicatat manual tiap periode? | **Dicatat manual oleh staf AR tiap periode, TANPA entity kontrak sewa.** Tidak ada `MstLeaseContract`. Staf mengisi tagihan baru setiap periode (objek sewa, penyewa, nominal, periode) — risiko tagihan terlewat bila staf lupa **diterima sadar** oleh owner, bukan ditangani sistem | Jawaban eksplisit owner atas opsi "staf mencatat manual, tanpa master kontrak" |
| `FIN-DEC-101` | Bagaimana piutang sewa ini disimpan, mengingat `FinReceivable` mewajibkan setiap barisnya berasal dari serah terima Billing (`SourceHandoffKey`/`InvoiceId`)? | **Entity piutang tersendiri, terpisah dari `FinReceivable`** — invariant "wajib dari Billing" pada `FinReceivable` **TIDAK dilonggarkan dan TIDAK mendapat pengecualian apa pun**. Piutang non-pasien punya jalur aging/write-off/alokasi sendiri yang serupa bentuknya tetapi berdiri sendiri | Jawaban eksplisit owner atas opsi yang direkomendasikan; alasan eksplisit: melonggarkan invariant `FinReceivable` berisiko disalahgunakan untuk piutang pasien juga |
| `FIN-DEC-102` | Bagaimana aturan denda keterlambatan? | **Nominal tetap, dicatat manual oleh staf AR saat menagih ulang** — tidak ada perhitungan otomatis (persentase atau tarif harian) pada rilis ini. Konsisten dengan `FIN-DEC-100`: seluruh angka pada kapabilitas ini memang dicatat manual, bukan dihitung sistem | Jawaban eksplisit owner atas opsi yang direkomendasikan |
| `FIN-DEC-103` | Siapa berwenang mencatat tagihan baru dan menghapus piutang sewa yang tidak tertagih? | **Staf AR penuh, TANPA jenjang approval untuk keduanya** — baik pencatatan maupun penghapusan. **Ini BERBEDA secara sadar** dari pola write-off `FinReceivable` (`BE-FIN-018`) yang memakai maker-checker. Owner memilih opsi tanpa approval sesudah disodorkan konsekuensinya (nol pemeriksa kedua untuk penghapusan nominal) | Jawaban eksplisit owner atas opsi "staf AR penuh tanpa approval", bukan opsi yang direkomendasikan |
| `FIN-DEC-104` | Piutang Parkir dan Piutang Tenant — satu entity atau dua? | **Satu entity, `FinNonPatientReceivable`, dengan kolom `Category` (`PARKING`/`TENANT`)** — kedua butir menu menjadi pandangan tersaring atas entity yang sama, mengikuti pola `FinReceivable.DebtorType` yang sudah ada | Jawaban eksplisit owner atas opsi yang direkomendasikan |

**Penegasan risiko yang MUST dicatat apa adanya, bukan didiamkan.** `FIN-DEC-103` membuka celah yang
tidak dimiliki kapabilitas Finance lain: staf yang sama dapat mencatat piutang **dan**
menghapusnya, tanpa pemeriksa kedua. Ini **diterima sadar** oleh owner untuk kapabilitas piutang
non-pasien **saja** — `FIN-DEC-103` **MUST NOT** dijadikan preseden untuk melonggarkan maker-checker
pada `FinReceivable` (piutang pasien) atau kapabilitas write-off mana pun yang sudah berjalan.

**Open question yang ditutup pass ini:**

| ID | Status baru | Keterangan |
|---|---|---|
| `FIN-OQ-043` | **CLOSED** oleh `FIN-DEC-099`..`104` | Kedua butir menu (Parkir, Tenant) kini dapat dirancang |

**Acceptance criteria tambahan yang sudah dapat diuji:**

1. `FinNonPatientReceivable` **MUST** punya kolom `Category` dengan tepat dua nilai: `PARKING`,
   `TENANT`. Nilai lain **MUST** ditolak.
2. `FinNonPatientReceivable` **MUST NOT** memiliki `SourceHandoffKey`, `SourceHandoffId`, atau
   `InvoiceId` — baris ini **tidak pernah** berasal dari Billing, dan upaya menyamakannya dengan
   `FinReceivable` adalah cacat desain.
3. Pencatatan tagihan baru dan penghapusan piutang sewa **MUST NOT** menuntut approval jenjang
   apa pun — keduanya selesai dalam satu aksi oleh staf AR.
4. Denda keterlambatan **MUST** berupa field nominal yang diisi manual, **MUST NOT** dihitung
   otomatis dari tanggal jatuh tempo.
5. Layar "Umur Piutang — Parkir" dan "— Tenant" **MUST** menjadi pandangan tersaring
   (`Category = PARKING` / `Category = TENANT`) atas satu endpoint umur piutang non-pasien,
   **bukan** dua endpoint terpisah.

**Langkah berikutnya:** `/design-business-module` amandemen kecil untuk menggambar `FinNonPatientReceivable`
(model, migration, endpoint, kontrak) — menyusul `AMENDMENT REVISI 13` yang sudah ada, sebagai bagian
dari revisi yang sama karena masih satu benang perubahan (penyelarasan navigasi V1). Sesudah itu,
`FIN-OQ-040`/`FIN-OQ-043` tidak lagi menahan `EPIC FIN-18` sama sekali — seluruh isinya dapat masuk
`/plan-module-delivery`.

---

## Addendum — Dua keputusan penutup sebelum perencanaan delivery, 1 Oktober 2026

**Pemicu.** Dua butir yang disodorkan pass desain sebagai hal yang **MUST** diputuskan pemilik,
dijawab langsung owner sebelum `/plan-module-delivery` dijalankan.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-105` | Menu V1 tidak punya butir "Tagihan Sewa" — satu-satunya jalan masuk ke pencatatan tagihan sewa adalah lewat laporan umur piutang, yang terasa berputar bila petugas harus mencatat tiap periode. Apakah butir menu tersendiri ditambahkan walau menyimpang dari V1? | **Ya, dibuat.** Butir menu tersendiri untuk mengelola tagihan sewa ditambahkan beserta layarnya. Ini **penyimpangan yang disetujui** dari bentuk V1 (`FIN-DEC-094`), dicatat terbuka sebagai pengecualian bernama — bukan pelonggaran umum atas aturan "ikuti V1 apa adanya" | Jawaban eksplisit owner, 1 Oktober 2026: "Jika belum ada maka buatkan konfigurasi tagihan sewanya juga" |
| `FIN-DEC-106` | Apakah batas penularan `FIN-DEC-103` (wewenang tanpa jenjang approval hanya berlaku untuk piutang sewa) diratifikasi sebagai aturan yang mengikat? | **Ya, diratifikasi.** Kelonggaran tanpa jenjang approval berlaku **hanya** pada `FinNonPatientReceivable`. Ia **MUST NOT** dijadikan dasar melonggarkan maker-checker pada `FinReceivableWriteOff`, `FinReceivableAdjustment`, atau jalur persetujuan pembayaran mana pun yang sudah berjalan | Jawaban eksplisit owner, 1 Oktober 2026: "saya menyetujui point kedua" |

### Batas `FIN-DEC-105` yang MUST dijaga

Keputusan ini menambah **jalan masuk**, bukan kapabilitas baru. Yang **tidak** ikut disetujui, dan
**MUST NOT** diturunkan diam-diam dari kata "konfigurasi":

| Yang **tidak** termasuk | Alasan |
|---|---|
| Master kontrak sewa (`MstLeaseContract` atau sejenisnya) | Ditolak tegas `FIN-DEC-100` satu putaran sebelumnya. Tidak dibuka ulang oleh keputusan ini |
| Master penyewa, master area parkir, master unit tenant | Turunan penolakan yang sama |
| Pengaturan tarif bawaan atau rumus denda | Ditolak `FIN-DEC-102` — nominal diketik petugas tiap kali |
| Penerbitan tagihan otomatis per periode | Ditolak `FIN-DEC-100` |

Bila yang dimaksud owner ternyata lebih dari jalan masuk — misalnya pengaturan tarif bawaan per
objek sewa — itu **keputusan baru** yang membuka kembali `FIN-DEC-100`/`FIN-DEC-102`, dan **MUST**
melewati `/grill-me` tersendiri. Perencanaan ini berjalan di atas bacaan yang sempit dan konservatif.

**Open question yang ditutup:** nol baru. `FIN-OQ-044` (integrasi kas dan kejadian akuntansi) **tetap
terbuka** dan tidak tersentuh kedua keputusan ini.

---

## Addendum — Jawaban `FIN-OQ-041`, `FIN-OQ-042`, `FIN-OQ-044`, 1 Oktober 2026

**Pemicu.** Yasmin (Product Owner Finance) menjawab tiga pertanyaan terbuka yang tersisa pada
`03-frontend-architecture.md` §17.5 dan `blueprint-manifest.md`. Diputuskan interaktif.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-107` | Empat pasang butir V1 yang tampak kembar: (a) Laporan Aging AR / Umur Piutang (A/R Aging); (b) Receivable AR/Invoice / Piutang Tagihan; (c) Retur Produk / Retur Pembelian Supplier; (d) Ayat Silang / Settlement AR | **Keempatnya dua layar berbeda, seluruhnya dipertahankan.** Tidak ada butir yang dibuang atau digabung. Dibuat sesuai tabel 17.2 dengan saringan bawaan yang berbeda, persis seperti perlakuan sementara sebelumnya | Jawaban eksplisit owner, 1 Oktober 2026 |
| `FIN-DEC-108` | Apakah kolom `PayerClaimReference` dibutuhkan? | **Ya, dipertahankan** sebagai kolom opsional pada migration. Tidak membawa aturan bisnis | Jawaban eksplisit owner, 1 Oktober 2026 |
| `FIN-DEC-109` | `FIN-OQ-044(a)`: apakah pelunasan sewa masuk kas harian dan setoran bank? | **Tidak.** Piutang sewa non-pasien dikelola **terpisah** dari kas harian dan setoran bank. Pelunasan sewa tetap hanya tercatat pada `FinNonPatientReceivableSettlement`. `FinReceipt` **MUST NOT** diperluas untuk sewa | Jawaban eksplisit owner, 1 Oktober 2026 |
| `FIN-DEC-110` | `FIN-OQ-044(c)`: bolehkah rilis pertama `EPIC FIN-19` berjalan tanpa penyambungan kas dan kode kejadian akuntansi? | **Boleh, dengan banner peringatan** yang tetap tampil. Batas ini sudah dikomunikasikan kepada pemilik | Jawaban eksplisit owner, 1 Oktober 2026 |

### Akibat yang MUST dijaga

1. `FIN-DEC-109` mengubah sifat `FIN-DES-075`: ketiadaan alur ke kas harian/setoran bank **bukan lagi
   batas sementara**, melainkan keputusan. Task penyambungan pelunasan sewa ke kas harian dan setoran
   bank (`FIN-19` bagian kas) **dicabut** dari roadmap, bukan lagi "tertahan".
2. Banner peringatan pada layar piutang sewa (`FE-FIN-022`, `FE-FIN-023`) **tetap** dan
   kata-katanya perlu disesuaikan: "dikelola terpisah dari kas harian", bukan "belum tersambung".
3. Risiko yang diterima sadar: uang sewa yang diterima tidak tercocokkan dengan rekening koran lewat
   kas harian. Mitigasi tetap pada alasan wajib dan jejak `IdentityModel`.

### Yang **masih terbuka**

| ID | Status | Keterangan |
|---|---|---|
| `FIN-OQ-044(b)` | **TERBUKA** | Kode kejadian akuntansi pendapatan sewa **belum** diratifikasi. Pemilik: Rizki (Accounting). Memblokir **kelengkapan akuntansi** `EPIC FIN-19`, bukan pembangunannya (`FIN-DEC-110`). Tidak dapat diputuskan Finance sepihak, mengikuti pola `FIN-DEC-053` |
| `FIN-OQ-041` | **CLOSED** oleh `FIN-DEC-107` | — |
| `FIN-OQ-042` | **CLOSED** oleh `FIN-DEC-108` | — |
| `FIN-OQ-044(a)`, `(c)` | **CLOSED** oleh `FIN-DEC-109`, `FIN-DEC-110` | Hanya bagian (b) tersisa |

**Langkah berikutnya:** amandemen kecil untuk menyelaraskan berkas turunan (`blueprint-manifest.md`,
§17.5 dan §18.2 pada `03-frontend-architecture.md`, `04-prd-to-mvp.md`, `roadmap/*`, `FIN-DES-075`)
dengan keputusan di atas. Penyelarasan itu belum dikerjakan; addendum ini baru mencatat keputusannya.

---

## Amendment pass — Penutupan gap Finance atas balasan Accounting `evidence/16`, 1 Oktober 2026

**Pemicu.** Accounting membalas surat Finance 15, 16, dan 21 lewat
`docs/module-blueprints/accounting/evidence/16-balasan-accounting-atas-surat-finance-15-16-21.md`
dengan lima pertanyaan balik (16.1–16.5). Sesi `/grill-me` ini memeriksa pertanyaan itu terhadap
source sisi Finance, menemukan gap tambahan, dan memutuskan sisi Finance secara interaktif.
Keputusan di bawah `approved` **sisi Finance** (Yasmin, 1 Oktober 2026). Butir yang menuntut
persetujuan Accounting atau modul lain ditandai tersendiri dan **belum** mengikat modul itu.

**Batas scope pass ini.** *Di dalam:* apa yang Finance kirim ke kotak masuk Accounting (penerimaan
kasir, snapshot saldo, penanda shift). *Di luar:* aturan posting dan bagan akun (Accounting), data
shift dan akun refund `REFERRED_OUTPATIENT_ADMIN` (Billing bersama Accounting), aturan internal
honor dokter (Medical Fee).

### Fakta source yang mendasari (dibaca 1 Oktober 2026, bukan keputusan)

| # | Fakta | Lokasi |
|---:|---|---|
| F1 | `PENERIMAAN-KASIR` terbit **per kuitansi**; `SourceTransactionId = receipt.ReceiptNumber`. `AccountingOutboxEventRequest` tidak punya rujukan shift maupun metode bayar | `FinanceReceiptService.cs:129-139`; `FinanceAccountingOutboxService.cs:221-234` |
| F2 | Snapshot saldo **hanya** dapat dipicu lewat endpoint POST manual; tidak ada hosted service Finance yang memanggilnya, padahal `FIN-DEC-092` menjanjikan terbit otomatis tanggal 1 pukul 00.05 WIB | `FinanceAccountingEventsController.GenerateSubledgerSnapshots` |
| F3 | Keempat saldo snapshot dipotong `Math.Max(0m, …)`: saldo negatif menjadi `0.00` tanpa jejak | `FinanceSubledgerSnapshotService.cs:92,100,111,121` |
| F4 | Snapshot menerbitkan tepat 4 baris dengan kode default; tidak ada pemetaan per akun control dan tidak ada utang honor dokter | `FinanceSubledgerSnapshotService.cs:123-129` |
| F5 | Kas kecil, piutang, dan utang memakai saldo **berjalan** (`CurrentBalance`, `OutstandingAmount`), bukan posisi pada tanggal akhir periode. Kas Kasir hanya status `CLOSED` dari hari tertutup terakhir | baris 87-121 |
| F6 | `AccountingDate` penerimaan kasir, pembaliknya, dan penanda shift dihitung dari `UtcDateTime`; batas akhir periode snapshot piutang juga UTC | `FinanceReceiptService.cs:134,207`; `FinanceBillingIntakeService.cs:1111`; snapshot baris 104 |
| F7 | Penanda shift hanya terbit untuk shift `Closed`, `Reviewed`, `Reopened`; shift berstatus terbuka tidak pernah terlihat Accounting | `FinanceBillingIntakeService.cs:1049-1054` |
| F8 | `StageEventAsync` sudah menaikkan `SourceVersion` otomatis bila kejadian sama diterbitkan ulang | `FinanceAccountingOutboxService.cs:89-97` |

### Keputusan

| ID | Pertanyaan | Keputusan | Menjawab |
|---|---|---|---|
| `FIN-DEC-111` | `PENERIMAAN-KASIR` per kuitansi atau per shift? | **Tetap per kuitansi.** Finance menambahkan **nomor shift dan metode bayar** pada kejadian penerimaan kasir dan pembaliknya; Accounting yang meringkas menjadi satu jurnal per shift per metode lewat aturan posting. Finance **tidak** membongkar `BE-FIN-024` dan idempotensi tender | Accounting 16.1 |
| `FIN-DEC-112` | Saldo tidak wajar (negatif) pada `SALDO-SUBLEDGER` | **Dikirim apa adanya, bertanda negatif**, artinya berlawanan dengan saldo normal akun. Amandemen `FIN-DEC-091`: larangan negatif **dicabut** untuk `SALDO-SUBLEDGER`. Pemotongan ke nol (F3) **MUST** dihapus | Accounting 16.3 |
| `FIN-DEC-113` | Granularitas baris saldo | **Satu baris per akun control**, lewat **pemetaan terkonfigurasi** "kelompok saldo → kode akun control". Bila daftar dari Accounting memuat akun yang belum terpetakan, snapshot **MUST** menolak terbit dengan pesan jelas (gagal tertutup), bukan mengirim sebagian | Accounting 16.2 |
| `FIN-DEC-114` | Shift tertutup sesudah snapshot; saldo bukan posisi per tanggal | Saldo dihitung sebagai **posisi per tanggal akhir periode** (transaksi bertanggal sampai akhir periode, shift `CLOSED` dan `REVIEWED`), terbit sekali, lalu **dinyatakan ulang otomatis** dengan `SourceVersion` lebih tinggi **hanya untuk akun yang nilainya berubah** | Accounting 16.4 |
| `FIN-DEC-115` | Bagaimana Accounting tahu ada shift terbuka | **Penanda pembukaan shift per shift**, kode baru bernilai nol (nama usulan `PEMBUKAAN-SHIFT-KASIR`), diterbitkan saat Finance melihat shift berstatus terbuka. Penutupan final menggantikannya; pembukaan tanpa penutupan menahan tutup bulan sisi Accounting | Accounting 16.5 |
| `FIN-DEC-116` | Konvensi tanggal akuntansi | **Seluruh `AccountingDate` dan batas periode memakai tanggal WIB (Asia/Jakarta)**, bukan UTC. Cakupan minimal tiga titik pada F6, dan **MUST** disisir ke sisa Finance sebelum diklaim tuntas | Temuan sesi ini |
| `FIN-DEC-117` | Pengirim saldo utang honor dokter | **Medical Fee menerbitkan saldonya sendiri** dengan kode akun control miliknya, **bila** G2 menandainya sebagai control account. Finance **tidak** membaca data honor dokter dan tidak memasukkannya ke snapshot | Accounting 16.2 (bagian honor) |

### Contoh

Kuitansi tunai Rp 150.000 dan kuitansi QRIS Rp 200.000 pada shift 12 tanggal 1 Oktober 2026 pukul
02.10 WIB. Finance mengirim dua kejadian, masing-masing membawa nomor shift 12 dan metodenya, dengan
`AccountingDate = 2026-10-01` (bukan 2026-09-30, `FIN-DEC-116`). Accounting menggabungkannya menjadi
dua jurnal untuk shift 12 (satu per metode). Bila kuitansi tunai dibalik sesudah shift tertutup,
pembaliknya tetap menunjuk kuitansi aslinya dan shift yang sama.

### Dampak turunan dan batas

1. **Perbaikan atas janji yang sudah ada, bukan keputusan baru.** Snapshot tidak terjadwal (F2) adalah
   pelanggaran `FIN-DEC-092` dan **MUST** dikerjakan sebagai bagian `BE-FIN-049`/penggantinya,
   memakai pola hosted service yang sudah ada di source; Finance **tidak** boleh menyatakan G4 siap
   sebelum ini terbukti jalan.
2. `FIN-DEC-112` menggantikan sebagian `FIN-DEC-091`; `FIN-DEC-091` ditandai `superseded` **hanya**
   pada klausa penolakan negatif untuk `SALDO-SUBLEDGER`. Kata "tanpa pengecualian" pada
   `evidence/21` tidak lagi berlaku.
3. Nilai `Amount` tetap mengikuti saldo normal akun (`ACC-DEC-109`); tanda minus hanya berarti
   lawan dari saldo normal itu.
4. **Migration tidak dibuat pada sesi ini.** `FIN-DEC-113` kemungkinan menuntut tabel atau
   konfigurasi baru; pembuatannya butuh instruksi dan konfirmasi terpisah dari Yasmin.
5. Pengirim penanda shift (`PENUTUPAN`/`PEMBALIKAN`) **tetap tidak diaktifkan** sampai Accounting
   menyatakan G6 siap (`FIN-OQ-035`, `evidence/16` bagian 4). `FIN-DEC-115` menambah satu kode ke
   gerbang yang sama.

### Open question

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-045` | Accounting menyetujui `FIN-DEC-111`, termasuk mengubah `ACC-DEC-062` dan memperluas kontrak dengan dimensi shift dan metode bayar | Rizki (Accounting) | `IMPLEMENTATION` sisi `PENERIMAAN-KASIR`; G4 |
| `FIN-OQ-046` | Apakah riwayat pembayaran, alokasi, pembalikan, dan setoran cukup untuk menghitung posisi **per tanggal** tanpa tabel baru? Perlu `/trace-existing-capabilities` | Yasmin / audit source | `IMPLEMENTATION` `FIN-DEC-114` |
| `FIN-OQ-047` | Ratifikasi `PEMBUKAAN-SHIFT-KASIR` dan penambahannya ke daftar tertutup nilai nol G6; nama final | Rizki (Accounting) | `IMPLEMENTATION` `FIN-DEC-115`; G6 |
| `FIN-OQ-048` | Accounting menerima konvensi tanggal WIB (`FIN-DEC-116`) | Rizki (Accounting) | `LATER SLICE` |
| `FIN-OQ-049` | Apakah Medical Fee sanggup dan bersedia menerbitkan saldo honor dokter sendiri; jadwalnya selaras dengan snapshot Finance | Yasmin sebagai owner Medical Fee | `LATER SLICE`; G2/G4 |
| `FIN-OQ-050` | Seberapa luas pola `UtcDateTime` pada `AccountingDate` di sisa Finance | Audit source | `IMPLEMENTATION` `FIN-DEC-116` |
| `FIN-OQ-051` | Persetujuan pembuatan migration untuk pemetaan akun control (`FIN-DEC-113`) | Yasmin | `IMPLEMENTATION` `FIN-DEC-113` |

### Bukan gap Finance (dicatat supaya tidak ditanyakan ulang)

Refund `REFERRED_OUTPATIENT_ADMIN`: perlakuan sementara Finance (gagal terlihat, nol kejadian, tidak
memakai `PENGEMBALIAN-UANG-MUKA`) sudah diterima Accounting (`ACC-DEC-129`). Akun debitnya ditentukan
Billing bersama Accounting dan menjadi syarat G6 mereka.

### Kriteria penerimaan yang kini dapat diuji

1. Saldo piutang negatif terkirim sebagai nilai negatif, **bukan** `0.00`.
2. Periode dengan akun control yang belum terpetakan: snapshot menolak terbit, nol baris outbox.
3. Pembayaran pukul 02.00 WIB tanggal 1 Oktober menghasilkan `AccountingDate = 2026-10-01`.
4. Dua kuitansi pada satu shift menghasilkan dua kejadian, masing-masing membawa nomor shift dan metode bayar.
5. Shift yang tertutup sesudah snapshot menaikkan `SourceVersion` hanya pada akun yang berubah.
6. Snapshot terbit otomatis tanggal 1 pukul 00.05 WIB tanpa dipicu manual, dan menjalankannya dua kali tidak menggandakan baris.

**Langkah berikutnya:** lihat penawaran di pesan sesi. Surat balasan Finance untuk Accounting
(`evidence/22`) belum ditulis.

---

## Closure pass — Penutupan `FIN-OQ-052`, `054`, `055`, `056`, `058` dan pengerasan `FIN-DEC-114`, 1 Oktober 2026

**Pemicu.** Impact scan `01-existing-capability-map.md` §18 (`7f8c3014` / `85578363b`) menemukan bahwa
Finance belum punya jalur pengiriman ke Accounting sama sekali, bahwa beberapa asumsi amandemen
sebelumnya tidak cocok dengan source, dan bahwa jalur pembayaran langsung piutang tidak meninggalkan
riwayat bertanggal. Keputusan di bawah `approved` sisi Finance (Yasmin, 1 Oktober 2026), diputuskan
interaktif lewat `/grill-me`.

**Batas scope pass ini.** *Di dalam:* apa yang Finance bangun untuk mengirim ke Accounting, dan kapan.
*Di luar:* kredensial layanan antar-modul (G3, Platform bersama Accounting), aturan posting dan bagan
akun (Accounting).

### Keputusan

| ID | Pertanyaan | Keputusan | Menutup |
|---|---|---|---|
| `FIN-DEC-118` | Apakah `EPIC FIN-12` (worker pengiriman) dibuka kembali? | **Ya, dibuka kembali sebagai syarat G4**, berupa satu paket tiga bagian: (1) worker pengiriman outbox yang menghormati gerbang yang sudah ada (penanda shift tetap `PENDING` sampai Accounting menyatakan G6 siap); (2) penjadwal snapshot tanggal 1 pukul 00.05 WIB; (3) pemicu otomatis penanda shift. Memakai pola hosted service di blok `runBackgroundJobs`. **Dibangun dalam keadaan mati** sampai mekanisme kredensial layanan (G3) diputuskan. Mencabut sebagian penundaan `EPIC FIN-12` | `FIN-OQ-058` |
| `FIN-DEC-119` | Dari mana saldo Kas Kasir dihitung? | **Rekap kas harian Finance (`FinDailyCashSnapshot`, status `CLOSED`)**, bukan status shift Billing. Snapshot **MUST menolak terbit** bila rekap hari terakhir periode belum `CLOSED`; kebiasaan memakai hari sebelumnya secara diam-diam **MUST** dihapus. Koreksi: `evidence/21` bagian 3.2 yang menulis "shift `CLOSED`/`REVIEWED`" salah baca kode dan **MUST** diluruskan ke Accounting | `FIN-OQ-052` |
| `FIN-DEC-120` | Kuitansi pembalik masuk shift yang mana? | **Shift saat pembalikan terjadi**, dengan rujukan ke kuitansi asli tetap dibawa. Menggantikan kalimat contoh `FIN-DEC-111` ("pembaliknya tetap menunjuk shift yang sama"): rujukan kuitansi asli tetap, tetapi shift adalah shift pembalikan, karena uang fisik keluar dari laci shift itu | `FIN-OQ-054` |
| `FIN-DEC-121` | "Shift terbuka" mencakup status apa? | **Semua status selain `CLOSED` dan `REVIEWED`** (`OPEN`, `HANDED_OVER`, `REOPENED`, `CLOSED_WITH_VARIANCE`, `PERLU_TINDAK_LANJUT`). Penanda pembukaan terbit saat Finance pertama kali melihat shift belum final; penutupan final menggantikannya. Memperjelas `FIN-DEC-115` | `FIN-OQ-055` |
| `FIN-DEC-122` | Siapa pengirim saldo utang honor dokter? | **Finance**, dari `FinMedicalServicePayable` miliknya sendiri. Selama tabel kosong (`BE-FIN-021` `BLOCKED`), Finance tetap mengirim `0.00`. **`FIN-DEC-117` menjadi `superseded`**: premisnya (data milik Medical Fee, Medical Fee mengirim sendiri) tidak cocok dengan source. Pemetaan `FIN-DEC-113` mendapat satu kelompok saldo tambahan | `FIN-OQ-056`, `FIN-OQ-049` |
| `FIN-DEC-123` | Bagaimana saldo Piutang dan Utang dihitung per tanggal? | **Dibangun buku mutasi** untuk Piutang dan Utang supplier (pola `FinPettyCashBudgetMovement`: waktu kejadian, saldo sebelum, saldo sesudah). **Setiap jalur yang mengubah saldo MUST menulis ke buku mutasi**, termasuk pembayaran langsung piutang. Posisi per tanggal dibaca dari mutasi terakhir sampai akhir periode. Mengeraskan `FIN-DEC-114` | Turunan `FIN-OQ-046`, `FIN-OQ-053` |

### Fakta source baru yang mendasari (dibaca 1 Oktober 2026)

| # | Fakta | Lokasi |
|---|---|---|
| F9 | Tidak ada hosted service Finance yang terdaftar; `EPIC FIN-12` ditunda atas keputusan pemilik | `Program.cs:940-953`; `roadmap/00-delivery-roadmap.md:40,235` |
| F10 | `FinReceiptDeduction` mengurangi `OutstandingAmount` **terpisah** dari alokasi, lewat panggilan sendiri ke `ApplyAllocationAsync` | `FinanceReceiptService.cs:611` |
| F11 | Pembayaran langsung piutang mengurangi `OutstandingAmount` **tanpa** baris `FinReceipt`/`FinReceiptAllocation`; jejaknya hanya satu baris outbox dan satu catatan audit | `FinanceReceivableService.cs:621-701` |
| F12 | Penghapusan langsung piutang **punya** baris `FinReceivableWriteOff` | `FinanceReceivableService.cs:729-749` |
| F13 | `AccountingDate` dihitung dari UTC pada **20 titik** di lima service, bukan tiga | capability map §18.2 |
| F14 | `FinPaymentAllocation` tidak punya tanggal sendiri; utang supplier berkurang saat pembayaran **disetujui**, bukan saat `PaidAt` | `FinancePaymentService.cs:575` |

### Koreksi atas klaim sesi ini

1. `FIN-OQ-046` sebelumnya dinyatakan "layak tanpa tabel baru". **Dicabut untuk Piutang** (F11) dan dengan syarat untuk Utang. `FIN-DEC-123` menggantikannya dengan buku mutasi; keputusan `FIN-DEC-114` tetap berlaku, hanya cara menghitungnya yang berubah.
2. Contoh `FIN-DEC-112` (piutang lebih bayar menjadi `0.00`) tidak dapat terjadi karena invarian skema. `FIN-DEC-112` tetap sah sebagai aturan, dengan Kas Kasir sebagai kandidat saldo negatif satu-satunya yang diketahui.
3. `FIN-DEC-117` ditandai `superseded` oleh `FIN-DEC-122`; `FIN-OQ-049` ikut tertutup tanpa pelaksana di Medical Fee.

### Akibat yang MUST dijaga

1. `FIN-DEC-118` mengubah janji "G4 siap" menjadi **bersyarat**: G4 baru boleh dinyatakan siap setelah ketiga bagian paket terbukti jalan. Balasan Finance ke Accounting (`evidence/22`) **MUST** mengatakannya apa adanya, tidak boleh menjanjikan pengiriman otomatis lebih dulu.
2. `FIN-DEC-123` menyentuh setiap jalur yang menulis `FinReceivable.OutstandingAmount` (alokasi, pembalikan, penyesuaian, penghapusan, pembayaran langsung, potongan) dan `FinSupplierPayable.OutstandingAmount` (pembayaran, penyesuaian). Tidak boleh ada jalur yang terlewat; satu jalur tanpa mutasi membuat saldo per tanggal salah tanpa ada yang tahu.
3. **Migration tidak dibuat pada pass ini.** `FIN-DEC-113` (pemetaan akun control) dan `FIN-DEC-123` (buku mutasi) sama-sama menuntut tabel baru; keduanya butuh instruksi dan konfirmasi terpisah dari Yasmin (`FIN-OQ-051`, diperluas).
4. Prefix tabel baru **MUST** didaftarkan di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` sebelum model dibuat (`QBE-MOD-003`).

### Contoh

Pembayaran langsung piutang Rp 500.000 pada 30 September pukul 23.50 WIB, lalu pembayaran Rp 200.000
pada 1 Oktober pukul 00.02 WIB. Buku mutasi mencatat dua baris dengan tanggal WIB masing-masing
(`FIN-DEC-116`). Snapshot periode `2026-09` membaca saldo sesudah mutasi pertama saja, sehingga angkanya
tetap benar walaupun penjadwal baru jalan pukul 00.30. Tanpa buku mutasi, kedua pembayaran ikut terhitung.

### Open question

| ID | Status | Keterangan |
|---|---|---|
| `FIN-OQ-052`, `054`, `055`, `056`, `058` | **CLOSED** oleh `FIN-DEC-119`..`122`, `118` | — |
| `FIN-OQ-053` | **CLOSED** (fakta F10) | Potongan mengurangi piutang terpisah; buku mutasi harus mencatatnya sebagai mutasi tersendiri |
| `FIN-OQ-049` | **CLOSED** oleh `FIN-DEC-122` | Tanpa pelaksana di Medical Fee |
| `FIN-OQ-051` | **DIPERLUAS** | Persetujuan migration untuk pemetaan akun control **dan** buku mutasi piutang/utang. Memblokir `IMPLEMENTATION` `FIN-DEC-113`, `123` |
| `FIN-OQ-057` | **TERBUKA** | Skenario nyata saldo negatif selain Kas Kasir. Tidak memblokir |
| `FIN-OQ-059` | **BARU** | Apakah ada jalur pengurang saldo utang supplier atau utang jasa medis yang tidak meninggalkan riwayat bertanggal, seperti F11 pada piutang? Perlu `/trace-existing-capabilities` terarah. Memblokir `IMPLEMENTATION` `FIN-DEC-123` bagian Utang |
| `FIN-OQ-060` | **BARU** | Apakah pembayaran langsung piutang (F11) ikut masuk rekap kas harian, mengingat ia tidak punya `FinReceipt`? Bila tidak, Kas Kasir dan Piutang bisa tidak sinkron. Pemilik: Yasmin. Memblokir `IMPLEMENTATION` `FIN-DEC-119` |
| `FIN-OQ-045`, `047`, `048` | **TERBUKA**, milik Accounting | Persetujuan `FIN-DEC-111`, kode `PEMBUKAAN-SHIFT-KASIR`, konvensi WIB |

### Kriteria penerimaan tambahan

1. Tanpa konfigurasi apa pun, worker pengiriman terdaftar tetapi **tidak** mengirim; menyalakannya membutuhkan aksi eksplisit.
2. Snapshot tanggal 1 pukul 00.05 WIB terbit tanpa dipicu manual; dijalankan dua kali tidak menggandakan baris.
3. Snapshot periode dengan rekap hari terakhir `OPEN`: ditolak, nol baris outbox.
4. Kuitansi pembalik di shift berbeda dari kuitansi asli: kejadian membawa shift pembalikan dan rujukan kuitansi asli.
5. Shift berstatus `CLOSED_WITH_VARIANCE` yang belum pernah terlihat sebelumnya: penanda pembukaan terbit.
6. Pembayaran langsung piutang menulis tepat satu baris buku mutasi; saldo per tanggal akhir periode tidak ikut menghitung pembayaran sesudahnya.
7. Saldo honor dokter terbit `0.00` selama tabel utang jasa kosong.

**Langkah berikutnya:** lihat penawaran di pesan sesi. Surat `evidence/22` tetap **ditahan**.

---

## Closure pass lanjutan — Sumber Kas Kasir dan pembayaran langsung piutang, 1 Oktober 2026

**Pemicu.** Pengecekan `FIN-OQ-060` menemukan fakta yang mengubah dasar `FIN-DEC-119`. Keputusan di bawah
`approved` sisi Finance (Yasmin, 1 Oktober 2026), diputuskan interaktif lewat `/grill-me`.

**Batas scope.** *Di dalam:* cara Finance menghitung dan mengirim saldo Kas Kasir, dan perlakuan pembayaran
langsung piutang. *Di luar:* akun debit untuk tunai vs bank (aturan posting Accounting), data shift
(Billing).

### Fakta source baru (dibaca 1 Oktober 2026)

| # | Fakta | Lokasi |
|---|---|---|
| F15 | `FinDailyCashSnapshot.CashReceiptAmount` dihitung dari `shifts.Sum(GetShiftCash)` atas `BilCashierShift` **tanpa melihat status shift**, bukan dari `FinReceipt` | `FinanceCashManagementService.cs:415,499` |
| F16 | Menutup rekap harian hanya memeriksa setoran `DRAFT` dan kesesuaian saldo awal; **tidak memeriksa status shift**. Rekap yang sudah `CLOSED` tidak dapat diubah dan tidak ada jalur koreksi | `FinanceCashManagementService.cs:585-611` |
| F17 | Batas hari rekap kas memakai `TimeSpan.Zero` (UTC) | `FinanceCashManagementService.cs:409` |
| F18 | Pembayaran langsung piutang menerima `PaymentMethod` (bawaan `"TRANSFER"`) tetapi **tidak menyimpan dan tidak memakainya**; tidak punya `FinReceipt`, tidak ikut shift, tidak masuk kas | `FinanceReceivableService.cs:621-701`; `FinanceArDtos.cs:17` |

### Keputusan

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-124` | Haruskah menutup rekap kas harian menuntut semua shift hari itu final? | **Tidak.** Rekap boleh ditutup walau masih ada shift belum final (pilihan pemilik: "lebih fleksibel"). Rekap harian menjadi **laporan operasional**, bukan dasar saldo ke Accounting |
| `FIN-DEC-125` | Bagaimana Kas Kasir diperbaiki bila shift selesai setelah rekap ditutup? | **Snapshot Kas Kasir dihitung langsung** saat dijalankan: kas dari shift berstatus `CLOSED`/`REVIEWED` dikurangi setoran bank `POSTED`/`VERIFIED`, sampai tanggal akhir periode dalam **tanggal WIB**. Pernyataan ulang `FIN-DEC-114` berjalan dengan menghitung ulang dari shift final. **`FIN-DEC-119` menjadi `superseded`** (premisnya keliru, lihat F15–F16). Selisih antara rekap harian yang sudah ditutup dan angka snapshot **MUST** ditampilkan sebagai informasi |
| `FIN-DEC-126` | Pembayaran langsung piutang: tunai atau non-tunai? | **Boleh tunai dan non-tunai, dengan satu mekanisme dan satu proses bisnis yang sama.** Satu-satunya perbedaan adalah metode pembayaran. **Metode wajib dipilih dan disimpan**, dan **bukti pembayaran disimpan**. Menutup `FIN-OQ-060` |

### Akibat yang MUST dijaga

1. Sebelum `FIN-DEC-126`, metode pembayaran langsung diabaikan; setelahnya ia **MUST** dibawa ke kejadian `PENERIMAAN-PIUTANG` supaya Accounting dapat menentukan akun debit. Ini menambah dimensi pada kontrak, sejalan dengan `FIN-DEC-111`.
2. Pembayaran langsung piutang **MUST** menulis satu baris buku mutasi `FIN-DEC-123`, membawa metode dan rujukan bukti.
3. Rumus Kas Kasir yang baru memakai shift final sebagai satu-satunya sumber kas masuk. **Kas tunai yang diterima langsung oleh Finance bukan bagian shift**, dan belum ada keputusan di mana ia dihitung (`FIN-OQ-063`).
4. Batas hari rekap kas (F17) masuk lingkup `FIN-DEC-116` bersama 20 titik `AccountingDate`.

### Contoh

Penjamin membayar Rp 5.000.000 tunai langsung ke Finance pada 30 September 2026 pukul 16.00 WIB, dengan
bukti kuitansi yang difoto. Finance mencatat satu pembayaran dengan metode "tunai", menyimpan foto bukti,
menurunkan piutang, menulis satu baris mutasi, dan mengirim `PENERIMAAN-PIUTANG` Rp 5.000.000 membawa
metode "tunai". Jika penjamin itu membayar transfer, prosesnya identik; hanya metodenya "transfer".

### Open question

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-060` | **CLOSED** oleh `FIN-DEC-126` | — | — |
| `FIN-OQ-061` | Dasar saldo awal Kas Kasir untuk rumus kumulatif `FIN-DEC-125` (saldo awal manual G5 atau kas sejak go-live) | Yasmin, lalu Accounting | `IMPLEMENTATION` `FIN-DEC-125` |
| `FIN-OQ-062` | Bentuk bukti pembayaran (berkas, nomor referensi, atau keduanya) dan apakah `FinReceivableDocument` dapat dipakai ulang. Perlu `/trace-existing-capabilities` terarah | Audit source | `IMPLEMENTATION` `FIN-DEC-126` |
| `FIN-OQ-063` | Tunai yang diterima langsung Finance dicatat di kas yang mana (bagian Kas Kasir, atau kas tersendiri)? | Yasmin, lalu Accounting | `IMPLEMENTATION` `FIN-DEC-125`, `126` |

### Kriteria penerimaan tambahan

1. Menutup rekap harian berhasil walau ada shift berstatus `OPEN`.
2. Snapshot Kas Kasir menjumlah hanya shift `CLOSED`/`REVIEWED`; shift `OPEN` tidak ikut.
3. Shift selesai setelah rekap ditutup: snapshot periode itu dinyatakan ulang dengan versi lebih tinggi.
4. Pembayaran langsung tunai dan transfer menghasilkan kejadian yang identik kecuali metodenya, dan bukti tersimpan pada keduanya.
5. Pembayaran langsung tanpa metode atau tanpa bukti ditolak dengan pesan jelas.

### Penutup `FIN-OQ-063`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-127` | Tunai yang diterima langsung Finance dicatat di kas yang mana? | **Komponen Kas Kasir.** Rumus Kas Kasir menjadi: kas dari shift `CLOSED`/`REVIEWED` **+ penerimaan tunai langsung Finance − setoran bank** `POSTED`/`VERIFIED`, sampai tanggal akhir periode (WIB). Tidak ada akun control baru. Penerimaan tunai langsung **MUST** punya jejak bertanggal lewat buku mutasi `FIN-DEC-123`. Melengkapi `FIN-DEC-125` |

`FIN-OQ-063` **CLOSED** oleh `FIN-DEC-127`. Pembayaran langsung non-tunai tidak masuk Kas Kasir; ia mengikuti
akun debit yang ditentukan Accounting menurut metode.

### Penutup `FIN-OQ-061`

**Fakta (F19).** Rekap harian pertama selalu berawal dari saldo awal `0`; nilai lain ditolak
(`FinanceCashManagementService.cs:608-612`). Saat ini **tidak ada** cara memasukkan saldo kas yang sudah ada pada hari go-live.

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-128` | Dari mana rumus Kas Kasir kumulatif mulai? | **Finance menyimpan satu saldo awal Kas Kasir pada tanggal cutover**, diinput manual dan disetujui, bernilai **sama** dengan saldo awal manual Accounting (G5). Rumus `FIN-DEC-125`/`127` mulai dari saldo awal ini. Nilai hanya boleh diisi sekali; perubahan sesudahnya **MUST** membawa alasan dan jejak. Kemungkinan menuntut tempat penyimpanan baru (migration, butuh konfirmasi terpisah, `FIN-OQ-051` diperluas) |

`FIN-OQ-061` **CLOSED** oleh `FIN-DEC-128`.

**Celah serupa yang ditemukan dan belum diputuskan (`FIN-OQ-064`).** Bila sebelum go-live sudah ada piutang
dan utang berjalan, Accounting memasukkannya sebagai saldo awal manual (G5). Finance menghitung Piutang
dan Utang dari tabelnya sendiri, yang hanya berisi data sejak intake pertama. Tanpa keputusan,
snapshot Piutang dan Utang berselisih dari buku besar sebesar saldo awal itu, persis seperti Kas Kasir
sebelum `FIN-DEC-128`. Pertanyaannya: apakah piutang dan utang lama dimigrasikan ke tabel Finance, atau
dicatat sebagai saldo awal tersendiri? Pemilik: Yasmin, lalu Accounting. Memblokir `IMPLEMENTATION`
`FIN-DEC-123` dan G5.

### Penutup `FIN-OQ-064`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-129` | Piutang dan utang lama sebelum go-live: dimigrasikan atau saldo awal saja? | **Dimigrasikan sebagai item tagihan di Finance** (pilihan A, dijawab Yasmin 1 Oktober 2026). Setiap tagihan lama masuk dengan identitas dokumen, debitur atau supplier, tanggal dokumen, jatuh tempo, nilai awal, dan sisa pada saat cutover. Record **MUST** diberi penanda sebagai data migrasi/opening item dan dibuka lewat **mutasi pembuka** `FIN-DEC-123` |

**Aturan yang mengikat `FIN-DEC-129`:**

1. **Rekonsiliasi sebelum dikunci.** Total sisa migrasi **MUST** direkonsiliasi dengan saldo awal AR/AP Accounting pada G5/G6 **sebelum** batch migrasi boleh disetujui dan dikunci.
2. **Sesudah diposting, tagihan lama mengikuti proses Finance yang normal:** aging, penagihan atau pembayaran, alokasi, settlement, snapshot, dan rekonsiliasi.
3. **Tidak boleh ada jurnal atau pendapatan/beban baru.** Migrasi **MUST NOT** menerbitkan kejadian akuntansi atas aktivitas sebelum go-live, karena nilainya sudah tercakup dalam saldo awal Accounting. Artinya jalur pengakuan piutang yang biasanya menulis ke outbox **MUST** dilewati untuk item berpenanda migrasi.
4. Cakupan: piutang (`FinReceivable`) dan utang supplier (`FinSupplierPayable`). Utang jasa medis (`FinMedicalServicePayable`) belum diputuskan (`FIN-OQ-065`).

**Contoh.** Piutang penjamin lama Rp 800.000.000 dari 40 tagihan dimigrasikan sebagai 40 item berpenanda
migrasi, masing-masing dengan mutasi pembuka. Jumlahnya dicocokkan dengan saldo awal AR Accounting
Rp 800.000.000; bila cocok, batch disetujui dan dikunci. Penjamin membayar Rp 100.000.000 untuk dua tagihan:
Finance mengalokasikannya seperti biasa dan mengirim `PENERIMAAN-PIUTANG`. Migrasi itu sendiri tidak
mengirim apa pun ke Accounting.

`FIN-OQ-064` **CLOSED** oleh `FIN-DEC-129`.

| ID | Pertanyaan baru | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-065` | Apakah utang jasa medis lama (`FinMedicalServicePayable`) ikut dimigrasikan? Tabelnya belum terisi dan `BE-FIN-021` `BLOCKED` | Yasmin | `LATER SLICE` |
| `FIN-OQ-066` | Mekanisme batch migrasi: bentuk impor, maker-checker persetujuan batch, siapa mengunci, dan bagaimana kegagalan sebagian ditangani. Perlu `/design-business-module` | Yasmin, lalu Accounting | `IMPLEMENTATION` `FIN-DEC-129`; G5/G6 |
| `FIN-OQ-051` | **DIPERLUAS lagi**: penanda migrasi pada item dan batch migrasi kemungkinan menambah kolom/tabel | Yasmin | `IMPLEMENTATION` |

### Kriteria penerimaan tambahan

1. Item berpenanda migrasi tidak menulis baris outbox saat diposting (nol kejadian akuntansi).
2. Batch migrasi dengan total sisa berbeda dari saldo awal Accounting **tidak dapat** disetujui atau dikunci.
3. Setelah batch dikunci, item tidak dapat diubah nilai awalnya; perubahan hanya lewat penyesuaian normal.
4. Pembayaran atas item migrasi menulis mutasi dan mengirim `PENERIMAAN-PIUTANG` seperti piutang biasa.
5. Snapshot Piutang periode go-live sama dengan saldo awal Accounting ditambah mutasi sesudah go-live.

---

## Closure pass lanjutan — Pembayaran langsung utang supplier, 1 Oktober 2026

**Pemicu.** Impact scan `01-existing-capability-map.md` §19 menemukan bahwa pembayaran langsung utang supplier
bocor seperti pembayaran langsung piutang (`FIN-OQ-059`). Keputusan di bawah `approved` sisi Finance
(Yasmin, 1 Oktober 2026), dijawab lewat `/grill-me`.

**Batas scope.** *Di dalam:* aturan pembayaran langsung utang supplier dan efeknya ke Kas Kasir. *Di luar:*
akun kredit kas atau bank (aturan posting Accounting), persetujuan berjenjang `FinPayment` yang sudah ada.

### Fakta source (dibaca 1 Oktober 2026)

| # | Fakta | Lokasi |
|---|---|---|
| F20 | Rekap kas harian memuat **pengeluaran kas** dalam rumusnya (`Opening + CashReceipt + OtherReceipt − Disbursement − BankDeposit`); nilainya diketik dari permintaan, bawaan `0` | `FinanceCashManagementService.cs:625-634` |
| F21 | `FinPayment` sudah menyimpan `PaymentMethod` (`TRANSFER`/`CASH`) dan `BankAccountId` wajib; pembayaran langsung utang supplier menerima `bankAccountId`, `paymentMethod`, `notes` tetapi tidak menyimpannya | `FinPayment.cs:40,44,109-112`; `FinanceSupplierPayableService.cs:207-274` |

### Keputusan

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-130` | Apakah pembayaran langsung utang supplier mengikuti aturan piutang? | **Ya, sama** (pilihan A). Mendukung tunai dan non-tunai, serta **menyimpan metode, sumber dana, catatan, dan bukti transaksi**. Setiap pembayaran langsung **MUST** menurunkan sisa utang dan menulis **satu mutasi** `FIN-DEC-123`. Metode `CASH` mengurangi Kas Kasir atau sumber kas terkait; metode `TRANSFER` mengurangi rekening bank sumber yang dipilih. **Metode dan sumber dana MUST diteruskan ke Accounting** agar akun kredit ditentukan eksplisit |
| `FIN-DEC-131` | Jalur langsung vs `FinPayment` | **Tetap dibedakan berdasarkan kontrol.** Transaksi sederhana atau di bawah ambang tertentu boleh langsung; transaksi bernilai besar atau berisiko **MUST** lewat `FinPayment` dengan persetujuan berjenjang. Nilai ambang dan definisi "berisiko" belum ditetapkan (`FIN-OQ-071`) |
| `FIN-DEC-132` | Koreksi rumus Kas Kasir | **Amandemen `FIN-DEC-125`/`127`**: rumus menjadi kas shift `CLOSED`/`REVIEWED` + penerimaan tunai langsung Finance **− pengeluaran kas tunai** (pembayaran supplier tunai, baik lewat `FinPayment` `CASH` maupun pembayaran langsung) − setoran bank `POSTED`/`VERIFIED`, sampai tanggal akhir periode (WIB). Menggantikan rumus tanpa pengeluaran pada `FIN-DEC-127`; pengeluaran **tidak lagi diketik manual** sebagai `DisbursementAmount` bebas pada snapshot kirim |

`FIN-DEC-127` ditandai `superseded` **hanya pada rumusnya** oleh `FIN-DEC-132`; keputusan bahwa tunai langsung Finance
adalah komponen Kas Kasir tetap berlaku.

### Akibat yang MUST dijaga

1. Jalur langsung dan jalur dokumen **MUST** menulis ke buku mutasi yang sama dan menghasilkan kejadian yang
   memuat metode dan sumber dana, supaya Accounting tidak menebak akun kredit.
2. Bukti transaksi mengikuti `FIN-OQ-068` (tempat penyimpanan belum diputuskan).
3. `FIN-DEC-131` mengubah jalur langsung dari "bebas" menjadi **terbatas**; perilaku sekarang (tanpa batas) **MUST NOT** dipertahankan setelah diterapkan.

### Contoh

Staf membayar supplier Rp 750.000 tunai lewat jalur langsung, di bawah ambang. Finance menyimpan metode `CASH`,
sumber kas, catatan, dan foto bukti; menurunkan sisa utang Rp 750.000; menulis satu mutasi; mengurangi Kas Kasir
Rp 750.000 pada tanggal WIB hari itu; dan mengirim `PEMBAYARAN-HUTANG-SUPPLIER` membawa metode `CASH`. Pembayaran
Rp 80.000.000 ke supplier yang sama, di atas ambang, ditolak di jalur langsung dan harus lewat `FinPayment`
dengan persetujuan berjenjang.

### Open question

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-067` | **CLOSED** oleh `FIN-DEC-130` | — | — |
| `FIN-OQ-071` | Nilai ambang jalur langsung, definisi "berisiko" (mis. supplier baru, rekening baru), dan siapa yang menetapkannya. Apakah ambang sama untuk piutang | Yasmin; persetujuan berjenjang bila perlu | `DESIGN` `FIN-DEC-131` |
| `FIN-OQ-072` | "Kas Kasir/cash source terkait": pembayaran tunai mengurangi **Kas Kasir** atau **Kas Kecil** (atau sumber kas lain)? Pengeluaran tunai bernilai kecil biasanya lewat kas kecil | Yasmin, lalu Accounting | `DESIGN` `FIN-DEC-132` |
| `FIN-OQ-073` | Pembayaran `TRANSFER` mengurangi "rekening bank sumber": apakah Finance perlu mencatat saldo per rekening bank, atau cukup meneruskan identitas rekening ke Accounting? | Yasmin, lalu Accounting | `DESIGN` `FIN-DEC-130` |

### Kriteria penerimaan tambahan

1. Pembayaran langsung utang supplier tanpa metode atau tanpa sumber dana ditolak dengan pesan jelas.
2. Pembayaran langsung tunai menurunkan Kas Kasir sebesar nilainya pada tanggal WIB kejadian.
3. Pembayaran langsung melewati ambang ditolak dan diarahkan ke `FinPayment`.
4. Setiap pembayaran langsung menulis tepat satu baris mutasi dan satu kejadian yang membawa metode dan sumber dana.
5. Pembayaran `FinPayment` tunai juga mengurangi Kas Kasir; tidak ada `DisbursementAmount` yang bisa diketik bebas pada snapshot.

### Penutup `FIN-OQ-072`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-133` | Pembayaran tunai ke supplier mengurangi Kas Kasir atau Kas Kecil? | **Selalu Kas Kasir** (pilihan A, Yasmin 1 Oktober 2026). Pembayaran supplier tunai, baik jalur langsung maupun `FinPayment` `CASH`, mengurangi Kas Kasir. **Kas Kecil hanya lewat mekanisme voucher kas kecil yang sudah ada** (anggaran, kategori, voucher); pembayaran supplier **MUST NOT** memotong anggaran kas kecil. Modul Kas Kecil tidak diubah. Menegaskan `FIN-DEC-132` |

`FIN-OQ-072` **CLOSED** oleh `FIN-DEC-133`. Konsekuensi yang diterima: pembayaran supplier kecil yang selama ini lewat kas kecil
tidak otomatis dikurangkan dari anggaran itu; staf memakai voucher kas kecil bila memang ingin memakainya.

### Penutup `FIN-OQ-071`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-134` | Bagaimana ambang jalur langsung ditetapkan? | **Satu ambang rupiah tetap, bisa diubah oleh pejabat berwenang, berlaku sama untuk utang dan piutang** (pilihan A, Yasmin 1 Oktober 2026). Perubahan ambang **MUST** membawa alasan dan jejak. Pembayaran di atas ambang **MUST** diarahkan ke `FinPayment` dengan persetujuan berjenjang. Kriteria "berisiko" (supplier atau rekening baru, pembayaran berulang) **tidak** dipakai pada rilis ini; pilihan B ditolak. Menjawab `FIN-DEC-131` |

`FIN-OQ-071` **CLOSED** oleh `FIN-DEC-134`. Diterima sadar: pembayaran yang dipecah-pecah di bawah ambang tidak otomatis tertangkap;
mitigasinya hanya jejak mutasi `FIN-DEC-123` dan laporan, bukan blokir otomatis.

| ID | Pertanyaan baru | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-074` | **Nilai awal ambang** (angka rupiah) dan siapa pejabat berwenang yang boleh mengubahnya. Belum disebut pada jawaban | Yasmin | `DESIGN` `FIN-DEC-134` (data konfigurasi, bukan kode) |

Kriteria penerimaan tambahan: (1) pembayaran langsung di atas ambang ditolak dan diarahkan ke `FinPayment`; (2) perubahan ambang tanpa alasan
ditolak dan meninggalkan jejak; (3) ambang yang sama berlaku pada pembayaran langsung piutang dan utang.

### Penutup `FIN-OQ-068`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-135` | Di mana bukti pembayaran disimpan? | **Finance membuat layanan penyimpanan bukti sendiri** (pilihan A, Yasmin 1 Oktober 2026), memakai pola dan konfigurasi unggah yang sudah ada (`FileStorage:UploadRootPath`, `UseStaticFiles`). **Bukan** memakai kelas HR (`WorkflowFileStorageService` dan sejenisnya) dan **bukan** `FinReceivableDocument`. Satu tabel metadata bukti (jenis, nama berkas, ukuran, jalur simpan, pengunggah) terikat ke mutasi pembayaran. Berlaku untuk pembayaran langsung piutang (`FIN-DEC-126`) dan utang (`FIN-DEC-130`) |

`FIN-OQ-068` **CLOSED** oleh `FIN-DEC-135`. Akibat: migration tabel metadata masuk `FIN-OQ-051`; prefix tabel **MUST** didaftarkan lebih dulu
di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (`QBE-MOD-003`); bila kelak Platform membuat layanan bersama, Finance yang memigrasikan.

| ID | Pertanyaan baru | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-075` | Aturan berkas bukti: jenis dan ukuran yang diterima, lama simpan, siapa boleh melihat dan menghapus, dan apakah bukti boleh diganti setelah pembayaran terkunci | Yasmin | `DESIGN` `FIN-DEC-135` |

Kriteria penerimaan tambahan: (1) pembayaran langsung tanpa bukti ditolak; (2) berkas di luar jenis atau ukuran yang ditetapkan ditolak;
(3) bukti terikat ke tepat satu mutasi dan tidak dapat dikaitkan ke pembayaran lain; (4) penyimpanan tidak memakai kelas atau tabel milik HR.

### Penutup `FIN-OQ-070`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-136` | Dari mana data migrasi berasal dan bagaimana masuk? | **Spreadsheet standar yang disiapkan staf Finance dari sistem lama, diunggah, divalidasi, lalu diposting lewat persetujuan batch** (pilihan A, Yasmin 1 Oktober 2026). Finance menetapkan **satu templat per jenis** (piutang dan utang supplier). Sistem memeriksa baris dan total **sebelum** posting; batch hanya dapat disetujui dan dikunci setelah total cocok dengan saldo awal AR/AP Accounting (`FIN-DEC-129`). Menjawab sebagian `FIN-OQ-066` (bentuk impor) |

`FIN-OQ-070` **CLOSED** oleh `FIN-DEC-136`. Yang tersisa pada `FIN-OQ-066`: persetujuan batch berjenjang (maker-checker), siapa mengunci, dan perilaku kegagalan sebagian.

| ID | Pertanyaan baru | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-076` | Jumlah kira-kira tagihan lama yang dimigrasikan (belum disebut), untuk menentukan ukuran batch, batas baris per unggahan, dan perilaku kegagalan sebagian | Yasmin | `DESIGN` `FIN-DEC-136` |
| `FIN-OQ-077` | **Persetujuan penambahan paket pembaca spreadsheet.** Proyek tidak punya satu pun; AGENTS.md melarang penambahan atau perubahan package tanpa wewenang eksplisit pada task. Pilihan paket dan lisensinya ditentukan pada tahap desain | Yasmin | `IMPLEMENTATION` `FIN-DEC-136` |

Kriteria penerimaan tambahan: (1) baris yang tidak lolos validasi tidak ikut diposting dan ditunjukkan per baris; (2) batch dengan total berbeda dari saldo awal
Accounting tidak dapat disetujui; (3) unggahan ulang batch yang sama tidak menggandakan item; (4) migrasi tidak menulis baris outbox (`FIN-DEC-129`).

### Penutup `FIN-OQ-073`

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-137` | Apakah Finance mencatat saldo per rekening bank? | **Tidak** (pilihan A, Yasmin 1 Oktober 2026). Finance menyimpan **master rekening** serta **identitas rekening sumber/tujuan** pada transaksi seperti pembayaran supplier dan setoran bank. Rekening, metode pembayaran, nominal, tanggal, dan referensi transaksi **diteruskan ke Accounting**. Saldo rekening bank, jurnal akun bank, dan rekonsiliasi rekening koran **tetap tanggung jawab Accounting**. Rekening bank **tidak** menjadi control account Finance dan **tidak** dikirim sebagai `SALDO-SUBLEDGER` |

**Penafsiran yang mengikat.** Frasa "mengurangi rekening bank" pada `FIN-DEC-130` berarti rekening itu **sumber dana transaksi**, bukan Finance menghitung
saldo berjalan rekening tersebut. `FIN-DEC-130` diperjelas, tidak digantikan.

`FIN-OQ-073` **CLOSED** oleh `FIN-DEC-137`.

Kriteria penerimaan tambahan: (1) pembayaran `TRANSFER` tanpa rekening sumber ditolak; (2) kejadian `PEMBAYARAN-HUTANG-SUPPLIER` bermetode `TRANSFER`
membawa identitas rekening sumber; (3) tidak ada baris `SALDO-SUBLEDGER` untuk rekening bank.

### Status akhir closure pass 1 Oktober 2026 (`FIN-DEC-118`..`137`)

Seluruh keputusan bisnis yang diketahui menahan desain **sudah tertutup sisi Finance**. Yang tersisa adalah data konfigurasi, aturan rinci
yang ditetapkan pada tahap desain, konfirmasi wewenang (migration, paket), dan persetujuan Accounting:

| Kelompok | ID | Sifat |
|---|---|---|
| Wewenang | `FIN-OQ-051` | Konfirmasi migration (pemetaan akun control, buku mutasi, saldo awal Kas Kasir, penanda dan batch migrasi, metadata bukti, ambang) |
| Wewenang | `FIN-OQ-077` | Penambahan paket pembaca spreadsheet |
| Data konfigurasi | `FIN-OQ-074`, `076` | Angka awal ambang; jumlah tagihan lama |
| Aturan rinci tahap desain | `FIN-OQ-066`, `075` | Maker-checker batch migrasi; aturan berkas bukti |
| Tidak memblokir | `FIN-OQ-057`, `065`, `069` | Saldo negatif selain Kas Kasir; utang jasa medis lama; piutang sewa lama |
| Milik Accounting | `FIN-OQ-045`, `047`, `048` | Persetujuan `FIN-DEC-111`, kode `PEMBUKAAN-SHIFT-KASIR`, konvensi WIB |
| Dikirim dalam surat | `evidence/22` | Belum ditulis; **MUST** memuat janji bersyarat G4 (`FIN-DEC-118`) |

---

## Addendum — Approval desain revisi 14 dan jawaban `FIN-OQ-051`, 1 Oktober 2026

**Pemicu.** Owner meninjau hasil `/design-business-module` revisi 14 dan menjawab gerbang wewenang
migration yang dibuka pass itu.

### Approval desain

| Hal | Keadaan |
|---|---|
| `FIN-DES-078`..`FIN-DES-091` | **`approved`** 1 Oktober 2026 oleh Yasmin lewat pernyataan langsung "Saya setujui FIN-DES-078...091" |
| Tujuh kontrak turunan (`FIN-API-1.5`, `FIN-INTEGRATION-1.7`, `FIN-STATE-1.6`, `FIN-VAL-1.7`, `FIN-PERM-1.7`, `FIN-TEST-1.8`, `FIN-MVP-1.9`) | **Ikut terangkat**, mengikuti preseden `status_note_revision_7`: bagian kontrak yang lahir dari pass desain yang sama ikut naik bersama approval keputusan arsitekturnya. Dicatat apa adanya; bila owner bermaksud lebih sempit, **MUST** dikoreksi |

### Keputusan

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-138` | `FIN-OQ-051` — wewenang migration revisi 14 | **Pembuatan berkas migration DIIZINKAN**, dengan syarat berkasnya **sesuai `ApplicationDbContextModelSnapshot`** — yaitu dihasilkan dari perubahan model, bukan ditulis tangan menyimpang dari snapshot. **Penerapan ke database TETAP milik Yasmin**; agent **MUST NOT** menjalankan migration, `dotnet ef database update`, maupun eksekusi SQL ke database mana pun. Jawaban eksplisit owner: *"bisa membuat file migration yg sesuai DbSnapshot. tpi untuk aplikasi ke database, yg lakukan adalah saya."* |

`FIN-OQ-051` **CLOSED** oleh `FIN-DEC-138`.

### Batas yang MUST dijaga

| Batas | Isi |
|---|---|
| Yang diizinkan | Membuat berkas migration di `Migrations/` beserta pasangan `.Designer.cs` dan pembaruan `ApplicationDbContextModelSnapshot.cs` yang konsisten |
| Yang **TIDAK** diizinkan | Menjalankan migration, memperbarui database, menjalankan SQL langsung, maupun menyentuh database di luar lingkungan pengembangan Yasmin |
| Kapan migration dibuat | **Di dalam task yang disetujui dari roadmap**, lewat `build-module-backend` — bukan sebagai pekerjaan lepas. Setiap task membawa migration-nya sendiri |
| Migration nomor 3 dan 4 | Milik `EPIC FIN-23` dan `EPIC FIN-24` yang berstatus **`OPEN DECISION`** dan di luar seluruh gelombang. Keduanya **tidak akan dibuat** sampai `FIN-OQ-075` dan `FIN-OQ-077` dijawab dan epic-nya masuk gelombang. Ini akibat disiplin roadmap, **bukan** batasan tambahan atas `FIN-DEC-138` |
| Urutan index migration nomor 4 | Pilihan antara `CREATE INDEX CONCURRENTLY` di luar transaksi atau membuat index bernama lain lebih dulu **tetap keputusan pemilik repository**, diambil bersama task yang menjalankannya — bukan sekarang |

### Akibat langsung

1. **`/plan-module-delivery` terbuka.** Satu-satunya gerbang yang menahannya (`FIN-OQ-051`) sudah tertutup.
2. `MVP-14A` dan `MVP-14B` dapat direncanakan menjadi task bernomor.
3. Setiap laporan task yang membuat migration **MUST** menyatakan bahwa migration itu **belum dijalankan**, dan menyebut langkah yang Yasmin perlu jalankan sendiri.

---

## Closure pass — Penutupan `FIN-OQ-075` dan `FIN-OQ-077`, 1 Oktober 2026

**Pemicu.** Kedua gerbang ini menahan `EPIC FIN-23` dan `EPIC FIN-24` seluruhnya. Owner menjawab
keduanya sesudah surat `evidence/22` terkirim. Keputusan di bawah `approved` sisi Finance
(Yasmin, 1 Oktober 2026).

**Batas scope pass ini.** *Di dalam:* aturan berkas bukti pembayaran Finance, dan format berkas
migrasi beserta wewenang paket yang dibutuhkannya. *Di luar:* kebijakan retensi dokumen keuangan
rumah sakit secara umum (bukan milik modul ini), dan mekanisme hak akses per pemilik transaksi
(milik Platform).

### Fakta source yang mendasari (dibaca 1 Oktober 2026)

| # | Fakta | Lokasi |
|---|---|---|
| F21 | Repository sudah punya aturan berkas yang mapan: wajib ada, tidak boleh kosong, batas ukuran, nama berkas maksimal 255 karakter, ekstensi wajib, daftar ekstensi terlarang, dan daftar ekstensi diizinkan | `WorkflowFileStorageService.ValidateFile`, baris 248-288 |
| F22 | Daftar yang sudah dipakai **dua** layanan: `.pdf`, `.jpg`, `.jpeg`, `.png`; tipe media `application/pdf`, `image/jpeg`, `image/png` | Dua layanan unggah HR |
| F23 | Daftar ekstensi dapat dikonfigurasi per fitur, pola `<Modul>:<Fitur>:AllowedExtensions` | `ResolveAllowedExtensions()` |
| F24 | Akar penyimpanan sudah berjalan: `FileStorage:UploadRootPath` bernilai `Storage/uploads` | `appsettings.json:58-59` |
| F25 | **Nol paket pembaca spreadsheet** terpasang. Yang ada hanya `SixLabors.ImageSharp` dan `QRCoder` | `QuilvianSystemBackend.csproj` |

### Keputusan

| ID | Pertanyaan | Keputusan |
|---|---|---|
| `FIN-DEC-139` | `FIN-OQ-075` — aturan berkas bukti pembayaran | **Mengikuti preseden yang sudah berlaku di repository** (pilihan A). Rinciannya di bawah |
| `FIN-DEC-140` | `FIN-OQ-077` — format berkas migrasi dan paket pembacanya | **Dua format didukung: CSV dan XLSX** (jawaban owner: *"bisa pakai csv dan xlsx"*). CSV dibaca dengan kemampuan bawaan .NET; XLSX menuntut **satu** paket pembaca, dan wewenang penambahannya **DIBERIKAN** dengan batas di bawah. Memperjelas `FIN-DEC-136` yang semula hanya menulis "spreadsheet standar" |

### `FIN-DEC-139` — rincian aturan berkas bukti

| Hal | Aturan |
|---|---|
| Jenis yang diterima | `.pdf`, `.jpg`, `.jpeg`, `.png` — sama dengan dua layanan unggah yang sudah berjalan. Tipe media diperiksa, bukan hanya ekstensinya |
| Tempat daftarnya | **Konfigurasi**, bukan tertanam di kode: `FinanceManagement:TransactionProof:AllowedExtensions`, mengikuti pola `<Modul>:<Fitur>:AllowedExtensions` yang sudah ada |
| Batas ukuran | Dari konfigurasi, bukan angka tertanam di kode. **Nilai awalnya belum ditetapkan** (`FIN-OQ-082`) |
| Pemeriksaan lain | Berkas wajib ada dan tidak kosong; nama maksimal 255 karakter; ekstensi wajib; daftar terlarang tetap ditegakkan; jalur simpan **MUST** divalidasi berada di bawah akar penyimpanan |
| Lama simpan | **Sistem tidak pernah menghapus bukti otomatis.** Retensinya mengikuti kebijakan dokumen keuangan rumah sakit, dan penghapusannya keputusan tersendiri di luar modul ini |
| Penggantian bukti | **Tidak dapat diganti** setelah baris mutasi tertulis. Koreksi dilakukan dengan **membalik pembayarannya lalu mencatat ulang** — pola "tidak pernah mengubah, selalu menambah baris" yang berlaku di seluruh blueprint ini |
| Penghapusan | Hanya penandaan (`IsDelete`), mengikuti `IdentityModel`. Berkas fisiknya **tidak** dihapus bersamaan |
| Akses | Siapa pun yang memegang `FinanceTransactionProof : Read`, **tanpa** pembatasan per pemilik transaksi pada rilis pertama |

**Batas yang diterima sadar, dan MUST disampaikan saat menyerahkan modul.** Staf AR/AP yang memegang
hak baca dapat melihat bukti pembayaran transaksi yang bukan miliknya. Pembatasan per pemilik
menuntut mekanisme hak akses yang belum dimiliki platform, dan menambahkannya akan menahan
`EPIC FIN-23` lagi. Mitigasi yang ada: jalur unduh dijaga hak akses, dan isi berkas **MUST NOT**
dicatat logger.

### `FIN-DEC-140` — rincian dukungan dua format dan batas wewenang paket

| Hal | Aturan |
|---|---|
| Format yang diterima | **CSV dan XLSX**, keduanya pada satu endpoint unggah yang sama |
| Pembaca CSV | Kemampuan bawaan .NET; **nol** paket baru |
| Pembaca XLSX | **Satu** paket, wewenang penambahannya diberikan owner |
| Lisensi paket | **MUST** permisif. `ClosedXML` (MIT) memenuhi syarat. **`EPPlus` versi 5 dan sesudahnya DILARANG** — lisensinya berubah menjadi komersial, dan memakainya memasukkan kewajiban lisensi ke sistem rumah sakit |
| Jumlah paket | **Tepat satu.** Dua pembaca XLSX sekaligus **MUST NOT** ditambahkan |
| Bentuk kode | Pembacanya **MUST** berupa lapisan terpisah di balik satu antarmuka yang memulangkan baris terurai. Validasi per baris (`FIN-VAL-186`..`191`) bekerja di atas baris terurai itu, **bukan** di atas berkasnya — sehingga aturan validasinya **tunggal** untuk kedua format |
| Templat | **Dua berkas templat** yang isinya setara, satu per format. Keduanya **MUST** dijaga sinkron; kolom yang berbeda antara keduanya adalah cacat |
| Wewenang per task | Penambahan paket **MUST** dinyatakan eksplisit pada task yang membawanya, mengikuti `AGENTS.md`. Keputusan ini memberi wewenangnya **secara prinsip**; task tetap menyatakannya sendiri |

**Risiko yang MUST dijaga, dan ia lahir langsung dari mendukung dua format.** Staf hampir pasti
memakai salah satu saja, sehingga jalur yang lain menjadi jalur yang jarang terpakai dan jarang
teruji. Karena itu:

1. Kedua jalur **MUST** diuji dengan berkas contoh yang isinya sama, dan keduanya **MUST**
   menghasilkan baris terurai yang identik.
2. CSV **MUST** menetapkan satu format angka dan tanggal pada templatnya. Excel di lokal Indonesia
   menulis `1.500.000,00`, dan baris yang tidak sesuai format **MUST** ditolak beserta nomor
   barisnya — bukan ditebak.
3. XLSX membawa sel bertipe, sehingga tanggal dan angkanya tidak ambigu. Itu justru membuat kedua
   jalur punya **bentuk kegagalan yang berbeda**, dan keduanya perlu kasus ujinya sendiri.

### Akibat yang MUST dijaga

1. **`EPIC FIN-23` tidak lagi `OPEN DECISION`.** `FIN-OQ-075` tertutup, sehingga satu-satunya yang
   tersisa adalah nilai awal batas ukuran berkas — data konfigurasi, bukan keputusan desain.
2. **`EPIC FIN-24` tidak lagi `OPEN DECISION`.** `FIN-OQ-077` tertutup.
3. **`04-prd-to-mvp.md` bagian 47, 48, dan 51 kini STALE.** Keduanya masih menyatakan kedua epic
   `OPEN DECISION` dan di luar seluruh gelombang. **MUST** diperbarui lewat pass desain, bukan
   di sini.
4. **`roadmap/00`, `01`, dan `02` kini STALE** pada bagian REV-14: keduanya mencatat kedua epic
   tanpa task. **MUST** diperbarui lewat `/plan-module-delivery` lanjutan.
5. `FIN-DEC-136` diperjelas `FIN-DEC-140`, **tidak** digantikan: jalannya tetap spreadsheet yang
   diunggah, divalidasi, lalu disetujui per batch.
6. Endpoint templat (`GET /opening-item-batches/template`) kini butuh ruas format selain `itemKind`.
   Perubahan kontraknya **MUST** digambar pass desain, bukan diputuskan di sini.

### Contoh

Staf menyiapkan 120 tagihan piutang lama di Excel lalu mengunggahnya sebagai `.xlsx`. Sistem
membacanya menjadi 120 baris terurai, memvalidasi masing-masing, dan menolak tiga baris yang
tanggal dokumennya melewati tanggal cutover beserta nomor barisnya. Staf memperbaiki ketiganya,
mengunggah ulang, lalu menyatakan saldo awal AR dari dokumen Accounting. Karena totalnya cocok,
batch disetujui dan dikunci. Bila staf yang sama menyimpan berkasnya sebagai `.csv`, hasil
terurainya **wajib identik** — itu kriteria penerimaan tersendiri.

Untuk pembayaran langsung: petugas memotret kuitansi dengan ponsel, menghasilkan `.jpg` berukuran
di bawah batas konfigurasi. Berkas tersimpan, `ProofId` terbit, pembayaran dicatat, dan satu baris
mutasi membawa `ProofId` itu. Bila nominalnya salah, petugas **tidak** mengganti buktinya —
pembayarannya dibalik, lalu dicatat ulang beserta bukti baru.

### Open question

| ID | Status | Keterangan |
|---|---|---|
| `FIN-OQ-075` | **CLOSED** oleh `FIN-DEC-139` | — |
| `FIN-OQ-077` | **CLOSED** oleh `FIN-DEC-140` | — |
| `FIN-OQ-081` | **BARU** | Nama dan versi paket pembaca XLSX yang dipakai. `ClosedXML` (MIT) adalah usulan; `EPPlus` v5+ **dilarang**. Dikonfirmasi pada task yang membawanya, mengikuti `AGENTS.md`. Pemilik: Yasmin. Memblokir `IMPLEMENTATION` bagian XLSX saja — bagian CSV, validasi, dan batch tidak tertahan |
| `FIN-OQ-082` | **BARU** | Nilai awal batas ukuran berkas bukti. Data konfigurasi; tanpa nilainya, unggah **ditolak fail-closed**. Pemilik: Yasmin. Tidak memblokir `DESIGN` |
| `FIN-OQ-074`, `076` | **TERBUKA** | Angka ambang; jumlah tagihan lama. Keduanya data konfigurasi |
| `FIN-OQ-045`, `047`, `048` | **TERBUKA**, milik Accounting | Diminta lewat `evidence/22` yang sudah terkirim |
| `FIN-OQ-079`, `080` | **TERBUKA** | Penempatan menu; apakah ambang ditampilkan kepada staf |
| `FIN-OQ-057`, `065`, `069`, `078` | **TERBUKA** | Tidak memblokir |

### Kriteria penerimaan tambahan

1. Berkas bukti ber-ekstensi di luar daftar konfigurasi ditolak beserta pesan yang dapat dipahami.
2. Berkas bukti yang ekstensinya lolos tetapi tipe medianya tidak cocok **ditolak**.
3. Tanpa nilai batas ukuran pada konfigurasi, unggah bukti **ditolak** — bukan dianggap tak terbatas.
4. Bukti yang sudah terpakai satu mutasi tidak dapat dipakai mutasi lain, dan **tidak dapat diganti**.
5. Jalur simpan yang mengarah keluar akar penyimpanan ditolak.
6. Berkas migrasi CSV dan XLSX yang isinya sama menghasilkan baris terurai yang **identik**.
7. CSV dengan format angka yang tidak sesuai templat ditolak beserta **nomor barisnya**, bukan ditebak.
8. Templat CSV dan XLSX memiliki kolom yang sama persis.
9. Tepat **satu** paket pembaca XLSX terpasang pada `QuilvianSystemBackend.csproj`.


---

## Amendment pass — Keputusan dari pembangunan `FE-FIN-027`..`032`: ambang, saldo awal, batch migrasi, selisih kas, dan menu, 4 Oktober 2026

**Pemicu.** Antara 2 dan 4 Oktober 2026 layar `FE-FIN-027`..`032` dan perubahan backend `BE-FIN-084`/`085`
dibangun. Pembangunan itu menemukan selisih antara kontrak dan source, dan sebagian sudah diputuskan owner
langsung di sesi build. Pass ini mencatat semuanya sebagai keputusan bernomor dan menutup sisanya lewat
wawancara. Seluruh keputusan di bawah `approved` sisi Finance (Yasmin).

**Mode.** `Amendment pass`. Blueprint `FIN-BP-001` revisi 15 sudah disetujui, sehingga histori approval
tidak ditimpa. Nomor revisi blueprint **tidak** dinaikkan di sini; itu pekerjaan pass desain.

**Source SHA saat pass ini.** Backend `5d6bb8bf`, frontend `ae2ed334e`; keduanya working tree bersih.
`01-existing-capability-map.md` masih pada backend `09101d05` / frontend `49b59cfaa`, sehingga **berpotensi
basi** untuk bagian yang disentuh `FE-FIN-027`..`032`. Pass ini tidak bergantung padanya: yang ditanyakan
adalah aturan bisnis, dan fakta source dibaca langsung dari kode.

### Batas scope pass ini

*Di dalam:* pencatatan empat keputusan owner yang belum bernomor; `EffectiveFrom` dan `RowVersion` pada
ambang pembayaran langsung; `FIN-OQ-079`, `080`, `081`; nama pelaku pada layar ambang dan batch migrasi;
perilaku selisih kas bila periode belum punya rekap kas harian.

*Di luar (dan sengaja tidak dikejar):*

| Hal | Pemilik |
|---|---|
| Pemberian hak akses kepada pengguna dan peran (termasuk apakah peran staf AR/AP diberi `MstDirectPaymentThreshold : Read`) | Admin dan Platform |
| Penempatan menu di luar Finance | Platform dan UI global |
| Nilai angka ambang (`FIN-OQ-074`), jumlah tagihan lama (`FIN-OQ-076`), batas ukuran berkas bukti (`FIN-OQ-082`) | Data konfigurasi |
| Pertanyaan milik Accounting (`FIN-OQ-045`, `047`, `048`) | Accounting |
| Siapa yang boleh memperbarui kontrak dan roadmap saat sebuah task selesai | Repository `QuilvianEngineeringSkills` |

### Fakta source yang mendasari (dibaca 3–4 Oktober 2026)

| # | Fakta | Lokasi |
|---|---|---|
| F26 | `EffectiveFrom` pada ambang disimpan, tetapi pemeriksaan pembayaran langsung piutang dan utang **tidak membacanya** | `FinanceReceivableService` baris 760, `FinanceSupplierPayableService` baris 303 |
| F27 | `MstDirectPaymentThreshold` tidak punya penanda versi; `PUT` menimpa baris aktif tanpa memeriksa apa pun | `DirectPaymentThresholdService.UpdateAsync` |
| F28 | `GET` ambang terbuka bagi pemegang `MstDirectPaymentThreshold : Read`; layar saat ini sementara hanya untuk pemegang `Read` **dan** `Update` | `DirectPaymentThresholdController`; `FE-FIN-031` |
| F29 | Respons ambang (`LastChangedBy`) dan batch migrasi (`ApprovedBy`) hanya memuat ID pengguna, tanpa nama | `DirectPaymentThresholdDtos.cs`, `OpeningItemBatchDtos.cs` |
| F30 | Periode tanpa rekap kas harian: saldo penutupan **0**, tanggal rekap kosong, dan `HasVariance = true` | `FinanceSubledgerBalanceCalculator.CalculateCashVarianceAsync` |
| F31 | Nol paket pembaca XLSX terpasang; unduh templat dan unggah XLSX dijawab `503` | `FinanceOpeningItemBatchesController`, `FinanceOpeningItemBatchService` |
| F32 | Permintaan setujui dan kunci saldo awal menerima `Notes` yang tidak pernah disimpan | `BE-FIN-066`, dihapus `BE-FIN-084` |
| F33 | Batas bawah nominal mutasi kas dilonggarkan khusus `SALDO-AWAL` bernilai nol oleh migration `RelaxFinCashMovementAmountForZeroOpeningBalance`, sudah diterapkan owner | `BE-FIN-084` |
| F34 | Endpoint `POST /opening-item-batches/{id}/reupload` sudah ada di source tetapi belum ada pada `api-contract.md` | `BE-FIN-085` |

### Keputusan

`FIN-DEC-141`..`144` dinyatakan langsung oleh owner pada sesi build 3 Oktober 2026 dan dicatat di sini
tanpa ditanyakan ulang. `FIN-DEC-145`..`152` diputuskan interaktif lewat `/grill-me` 4 Oktober 2026.

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-141` | Apakah permintaan setujui dan kunci saldo awal menerima catatan? | **Tidak.** `Notes` dihapus dari kedua permintaan karena tidak pernah disimpan. Memperjelas `FIN-DEC-128` | Pernyataan owner 3 Oktober 2026; `BE-FIN-084` |
| `FIN-DEC-142` | Apakah layar saldo awal menampilkan nama penyetuju? | **Ya.** Respons saldo awal memuat `ApprovedByName` | Pernyataan owner 3 Oktober 2026; `BE-FIN-084` |
| `FIN-DEC-143` | Apakah penguncian saldo awal Kas Kasir yang bernilai nol meninggalkan mutasi kas? | **Ya, selalu satu mutasi `SALDO-AWAL`, termasuk bernilai nol.** `FIN-VAL-168` diberi **satu pengecualian sempit**: hanya jenis `SALDO-AWAL` boleh nol. Nilai negatif tetap ditolak untuk semua jenis; jenis lain tetap harus lebih besar dari nol. Pengecualian dijaga di service **dan** di batasan basis data | Pernyataan owner 3 Oktober 2026; `BE-FIN-084`, migration diterapkan owner |
| `FIN-DEC-144` | Bagaimana petugas memperbaiki berkas batch yang bergalat? | **Berkas batch yang masih Draf dapat diunggah ulang** pada batch yang sama (`DRAFT` → `DRAFT`, hak `Update`). Hasil validasi lama dibuang; saldo awal Accounting yang sudah dinyatakan **dipertahankan**; jenis item tidak dapat diganti; batch `VALIDATED`, `APPROVED`, `LOCKED`, `REJECTED` tidak dapat diunggah ulang (`409`) | Pernyataan owner 3 Oktober 2026; `BE-FIN-085`; menegaskan baris "Mengunggah ulang berkas" pada `FIN-STATE-1.6` F.2 |
| `FIN-DEC-145` | Kapan ambang baru berlaku, dan apa nasib kolom tanggal berlaku? | **Selalu berlaku seketika.** Tanggal berlaku **dicabut dari kontrak** dan dari permintaan ubah ambang. Tidak ada perubahan ambang terjadwal, sejalan dengan `FIN-DES-086` yang menolak tabel riwayat. Memperjelas `FIN-DEC-134` | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-146` | Apa yang terjadi bila dua pejabat mengubah ambang bersamaan? | **Penyimpanan yang kalah ditolak** dan diminta memuat ulang. Ambang mendapat penanda versi; permintaan ubah wajib membawanya, **kecuali** penetapan ambang pertama kali karena belum ada baris | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-147` | `FIN-OQ-080` — apakah staf AR/AP boleh tahu angka ambang? | **Ya, angka ambang ditampilkan kepada staf AR/AP.** Jawaban ini **berbeda dari rekomendasi agent** (menyembunyikan angka dan mengunci `GET`); keputusan owner berlaku. Akibatnya: layar pembayaran langsung menampilkan angkanya; layar master ambang dapat dibuka pemegang `Read` dan hanya pemegang `Update` yang melihat kendali ubah; risiko pembayaran dipecah di bawah ambang tetap **diterima sadar** (`FIN-DEC-134`) | Jawaban owner |
| `FIN-DEC-148` | `FIN-OQ-079` — di mana layar-layar baru Finance ditempatkan pada menu? | **Submenu baru "Cutover & Subledger"** pada grup Keuangan, memuat Pemetaan Akun Control, Saldo Awal Cutover, dan Batch Migrasi Tagihan Lama. **Ambang Pembayaran Langsung** masuk submenu **Master Data** yang sudah ada. **Buku Kas** dan **riwayat mutasi piutang/utang** dicapai dari layar kas dan detail piutang/utang, tanpa butir menu sendiri. Menu Transaksi A/R dan A/P (`FIN-DEC-094`) **tidak berubah** | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-149` | `FIN-OQ-081` — paket pembaca XLSX | **Ditunda: rilis pertama CSV saja.** Pembaca XLSX (`BE-FIN-083`) menjadi pekerjaan slice berikutnya. Nama dan versi paket **belum dikonfirmasi** (`ClosedXML` MIT tetap usulan; `EPPlus` v5+ tetap dilarang). `FIN-DEC-140` diperjelas, **tidak** digantikan: dua format tetap tujuan, tetapi XLSX tidak ikut rilis pertama. Jawaban ini **berbeda dari rekomendasi agent** | Jawaban owner |
| `FIN-DEC-150` | Apa yang ditawarkan pemilih berkas unggah selama XLSX ditunda? | **Hanya CSV.** XLSX kembali ditawarkan ketika pembacanya ada. Pilihan XLSX pada unduh templat tetap nonaktif. Berkas XLSX yang tetap dipaksa masuk ditolak `503` dengan pesan jelas. Menunda bagian kontrak `FIN-API-1.6` F.2 "satu pemilih menerima CSV dan XLSX" | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-151` | Apakah layar ambang dan batch migrasi menampilkan nama pelaku? | **Ya, untuk keduanya.** Respons ambang memuat nama pengubah terakhir; respons batch memuat nama penyetuju. Nama terlihat oleh siapa pun yang boleh membaca layar itu | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-152` | Apa yang dinyatakan API perbandingan kas bila periode belum punya rekap kas harian? | **Dinyatakan eksplisit.** Saldo penutupan dan selisih **kosong** (bukan nol), `HasVariance` bernilai salah, dan ada penanda **belum ada rekap**. Menggantikan perilaku saat ini (nol dan selisih palsu) | Jawaban owner, rekomendasi dipilih |

### Rincian dan contoh

**Ambang (`FIN-DEC-145`, `146`, `147`, `151`).**
Pejabat A dan pejabat B sama-sama membuka layar ambang yang bernilai Rp 5.000.000. A menyimpan
Rp 8.000.000 beserta alasannya. B kemudian menyimpan Rp 3.000.000. Karena B bekerja dari nilai lama,
sistem **menolak** simpanan B dan menyuruhnya memuat ulang; layar B lalu menampilkan Rp 8.000.000,
alasan A, dan nama A beserta waktunya. Ambang yang baru disimpan **langsung berlaku**; tidak ada tanggal
berlaku yang dapat dijadwalkan.

Seorang staf AR memasukkan pembayaran langsung Rp 12.000.000 sementara ambang Rp 10.000.000. Layar
pembayaran memperingatkan sebelum dikirim: *"Nominal melewati ambang pembayaran langsung (Rp 10.000.000).
Gunakan jalur pembayaran berjenjang."* Angka ambang kini tertulis karena owner memutuskan staf boleh
mengetahuinya.

**Batch migrasi (`FIN-DEC-144`, `149`, `150`, `151`).**
Staf mengunggah `tagihan-piutang.csv`; validasi menemukan tiga baris bergalat (baris 14, 27, 31). Staf
memperbaiki berkasnya di luar sistem lalu memakai **Unggah Ulang Berkas** pada batch yang sama. Hasil
validasi lama hilang, saldo awal Accounting yang sudah dinyatakan tetap, dan staf menjalankan validasi
lagi. Bila batch sudah *Tervalidasi*, unggah ulang tidak ditawarkan; batch itu harus ditolak lalu dibuat
ulang. Pemilih berkas hanya menawarkan `.csv` pada rilis pertama.

**Selisih kas (`FIN-DEC-152`).**
Petugas membuka perbandingan kas Oktober 2026, tetapi belum ada satu pun rekap kas harian untuk bulan itu.
Sebelum keputusan ini, API menjawab "saldo penutupan Rp 0, selisih −Rp 70.000.000". Setelah keputusan ini,
API menjawab saldo penutupan dan selisih **kosong** disertai penanda *belum ada rekap*, sehingga tidak ada
angka nol yang bisa disangka saldo.

### Endpoint yang terpengaruh (bergaya Swagger)

| Grup Swagger | Method | Path | Perubahan | Status |
|---|---|---|---|---|
| `Corporate / Finance Management / Master Data / Direct Payment Threshold` | `GET` | `/` | Respons memuat penanda versi dan nama pengubah terakhir; **tanpa** tanggal berlaku | Direncanakan |
| idem | `PUT` | `/` | Permintaan: penanda versi **wajib** kecuali penetapan pertama; **tanpa** tanggal berlaku. Versi basi dijawab `409` | Direncanakan |
| `Corporate / Finance Management / Opening Item Batch` | `POST` | `/{id}/reupload` | **Baru.** `multipart/form-data`: berkas dan penanda versi. Hak `FinanceOpeningItemBatch : Update`. Kode: `400`, `404`, `409`, `422`, `503` | Sudah di source (`BE-FIN-085`) |
| idem | `GET` | `/`, `/{id}` | Respons memuat nama penyetuju | Direncanakan |
| idem | `GET` | `/template` | `format=XLSX` tetap `503` sampai slice XLSX | Sudah di source |
| `Corporate / Finance Management / Subledger Setup` | `POST` | `/opening-balances/{id}/approve`, `/lock` | Permintaan **tanpa** `Notes`; respons memuat nama penyetuju | Sudah di source (`BE-FIN-084`) |
| `Corporate / Finance Management / Accounting Events` | `GET` | `/subledger-balances/{accountingPeriodCode}/variance` | Saldo penutupan dan selisih boleh kosong; penanda *belum ada rekap*; `HasVariance` salah bila tidak ada rekap | Direncanakan |

### Akibat yang MUST dijaga

1. **Kontrak dan matriks kini STALE** pada bagian berikut, dan **MUST** diperbarui lewat
   `/design-business-module`, bukan di sini: `api-contract.md` F.1 (permintaan tanpa `Notes`, nama penyetuju),
   F.2 (endpoint `reupload`, pemilih CSV, nama penyetuju, label **Tersedia**), F.4 (versi, nama pengubah,
   tanpa tanggal berlaku, label **Tersedia**), F.6 (selisih kosong); `validation-matrix.md` `FIN-VAL-168`
   dan redaksi `FIN-VAL-192`; `state-transition-matrix.md` F.1 dan F.2; `erd/data-dictionary.md` untuk ambang.
2. **Layar yang sudah dibangun perlu disesuaikan** lewat `/plan-module-delivery`:
   - `FE-FIN-031` (layar ambang): terbuka bagi pemegang `Read`, kendali ubah hanya `Update`, tampil versi dan
     nama pengubah, tanpa tanggal berlaku; banner pembatasan `FIN-OQ-080` dihapus.
   - `FE-FIN-030` (pembayaran langsung): peringatan ambang menyebut angkanya.
   - `FE-FIN-032` (batch migrasi): pemilih hanya CSV; nama penyetuju.
   - `FE-FIN-029` (selisih kas): membaca nilai kosong dan penanda baru dari API, bukan tanggal kosong.
   - Butir menu `FIN-DEC-148` untuk submenu baru dan Master Data.
3. **Pekerjaan backend baru** (nol dikerjakan di pass ini): penanda versi dan nama pengubah pada ambang, tanggal
   berlaku dicabut dari permintaan dan respons, nama penyetuju pada batch, penanda *belum ada rekap* pada
   selisih kas. Penanda versi menuntut perubahan skema, sehingga memakai wewenang membuat migration
   `FIN-DEC-138`; **menerapkannya tetap milik Yasmin**.
4. **Peran staf AR/AP perlu hak `MstDirectPaymentThreshold : Read`** agar angka ambang terbaca di layar
   pembayaran. Pemberian hak itu milik admin dan berada di luar scope pass ini; tanpa hak itu layar
   pembayaran jatuh kembali ke peringatan tanpa angka.
5. **`04-prd-to-mvp.md` dan roadmap** perlu dicatat ulang untuk `MVP-14D`/`14E`: XLSX keluar dari rilis
   pertama dan `BE-FIN-083` pindah ke slice berikutnya.
6. **Capability map basi.** `/trace-existing-capabilities` mode impact scan disarankan sebelum pass desain.
7. **Batas yang tetap diterima sadar:** pembayaran dipecah di bawah ambang tidak terdeteksi (`FIN-DEC-134`),
   dan satu orang dapat memegang hak mencatat dan menyetujui sekaligus (`FIN-PERM-1.7` G.5). `FIN-DEC-147`
   memperlebar yang pertama karena angka kini diketahui staf; owner memilihnya dengan sadar.

### Frontend Decision Authority — tambahan

| Keputusan | Pemilik | Status | Rentang yang diizinkan |
|---|---|---|---|
| Penempatan butir menu layar baru Finance (`FIN-DEC-148`) | Owner | approved | Submenu "Cutover & Subledger"; Ambang di Master Data. **Bukan** `DEV_DISCRETION` |
| Angka ambang tampil kepada staf AR/AP (`FIN-DEC-147`) | Owner | approved | Wajib tampil pada peringatan melewati ambang bagi pemegang hak baca |
| Pemilih unggah hanya CSV selama XLSX ditunda (`FIN-DEC-150`) | Owner | approved | Dibalik saat pembaca XLSX ada |
| Nama pelaku pada layar ambang dan batch (`FIN-DEC-151`) | Owner | approved | Ditampilkan apa adanya; tanpa ID mentah |
| Urutan butir **di dalam** submenu "Cutover & Subledger" | — | **Asumsi** | Mengikuti urutan alur kerja: Pemetaan Akun → Saldo Awal → Batch Migrasi. Belum ditanyakan; owner dapat mengubahnya |
| Tata letak, warna, ikon, bentuk kartu dan tabel | — | `DEV_DISCRETION` | Seperti sebelumnya |

### Open question

| ID | Status | Keterangan |
|---|---|---|
| `FIN-OQ-079` | **CLOSED** oleh `FIN-DEC-148` | — |
| `FIN-OQ-080` | **CLOSED** oleh `FIN-DEC-147` | — |
| `FIN-OQ-081` | **DITUNDA** oleh `FIN-DEC-149` | Tetap terbuka (nama dan versi paket belum dikonfirmasi) tetapi **tidak memblokir rilis pertama**. Memblokir `LATER SLICE` bagian XLSX saja. Pemilik: Yasmin |
| `FIN-OQ-083` | **BARU** | Nasib **kolom fisik** `EffectiveFrom` pada tabel ambang: dihapus lewat migration, atau dipertahankan dan diisi tanggal perubahan. `FIN-DEC-145` hanya mencabutnya dari kontrak. Pemilik: Yasmin; diputuskan saat `/design-business-module`. Memblokir `DESIGN` kontrak F.4 dan data dictionary, bukan implementasi bagian lain |
| `FIN-OQ-074`, `076`, `082` | **TERBUKA** | Data konfigurasi; tidak berubah |
| `FIN-OQ-045`, `047`, `048` | **TERBUKA**, milik Accounting | Tidak berubah |
| `FIN-OQ-057`, `065`, `069`, `078` | **TERBUKA** | Tidak memblokir |

### Kriteria penerimaan tambahan

1. Permintaan ubah ambang tidak lagi memuat tanggal berlaku, dan ambang baru langsung berlaku pada pembayaran langsung berikutnya.
2. Permintaan ubah ambang dengan penanda versi basi ditolak `409` dan **tidak** mengubah ambang; penetapan ambang pertama tanpa penanda versi diterima.
3. Staf AR/AP pemegang `MstDirectPaymentThreshold : Read` melihat angka ambang pada peringatan layar pembayaran langsung.
4. Layar master ambang dapat dibuka pemegang `Read`; kendali ubah hanya tampil bagi pemegang `Update`.
5. Respons ambang memuat nama pengubah terakhir; respons batch migrasi memuat nama penyetuju; respons saldo awal sudah memuatnya.
6. Pemilih berkas unggah batch hanya menawarkan `.csv`; pilihan XLSX pada unduh templat nonaktif; berkas XLSX yang dipaksa masuk ditolak `503` dengan pesan jelas.
7. Berkas batch `DRAFT` dapat diunggah ulang: hasil validasi lama dibuang, saldo awal Accounting yang dinyatakan tetap, jenis item tidak berubah, penanda versi basi ditolak `409`. Batch `VALIDATED`, `APPROVED`, `LOCKED`, atau `REJECTED` ditolak `409`.
8. Mutasi `SALDO-AWAL` bernilai nol dapat tercatat; mutasi bernilai negatif ditolak untuk semua jenis; mutasi bernilai nol jenis lain ditolak `400`; mengunci saldo awal Kas Kasir bernilai nol menerbitkan tepat satu mutasi `SALDO-AWAL`.
9. Permintaan setujui dan kunci saldo awal tidak memuat dan tidak menyimpan catatan.
10. Perbandingan kas periode tanpa rekap kas harian menyatakan saldo penutupan dan selisih kosong, `HasVariance` salah, dan penanda *belum ada rekap*; layar menulis "Belum ada rekap", bukan angka nol.
11. Menu memuat submenu "Cutover & Subledger" berisi tiga layar, Ambang di Master Data, dan menu Transaksi A/R dan A/P tidak berubah.

### Langkah berikutnya

Tidak ada keputusan kritis yang masih terbuka dan memblokir desain, kecuali `FIN-OQ-083` yang hanya
menyentuh kontrak F.4. Pass ini **tidak** menulis kontrak, arsitektur, roadmap, migration, endpoint, atau UI.



---

## Amendment pass lanjutan — Penutupan open question milik Finance, 4 Oktober 2026

**Pemicu.** Sesudah pass sebelumnya (`FIN-DEC-141`..`152`), owner meminta seluruh open question yang masih
terbuka diselesaikan. Pass ini menutup yang menjadi milik Finance dan sengaja membiarkan yang bukan miliknya.
Seluruh keputusan di bawah `approved` sisi Finance (Yasmin), diputuskan interaktif lewat `/grill-me`
4 Oktober 2026.

**Mode.** `Amendment pass`. Blueprint `FIN-BP-001` revisi 15; histori approval tidak ditimpa dan nomor revisi
belum dinaikkan. SHA source sama dengan pass sebelumnya: backend `5d6bb8bf`, frontend `ae2ed334e`.
`01-existing-capability-map.md` tetap **berpotensi basi** (lihat pass sebelumnya).

### Batas scope pass ini

Scope **diperluas atas permintaan owner** dari pass sebelumnya, dan dikonfirmasi eksplisit sebelum bertanya.

*Di dalam:* `FIN-OQ-083`, `074`, `076`, `082`, `065`, `069`, `057`, dan urutan butir menu submenu
"Cutover & Subledger".

*Di luar (tetap terbuka, bukan milik Finance atau tidak dapat diputuskan sekarang):*

| Hal | Pemilik | Alasan |
|---|---|---|
| `FIN-OQ-045`, `047`, `048` | Accounting (Rizki) | Persetujuan atas kontrak kejadian; bukan keputusan Finance |
| `FIN-OQ-078` | Accounting | Bentuk jalur baca saldo awal bagi Accounting; kontrak milik Accounting, `LATER SLICE` |
| `FIN-OQ-081` | Yasmin | Sudah ditunda `FIN-DEC-149`; baru dapat diputuskan saat slice XLSX dibuka |
| Siapa yang diberi hak mengubah ambang | Admin dan Platform | Pemberian hak akses; bagian kedua `FIN-OQ-074` dicatat di sini, bukan dijawab |

### Fakta source yang mendasari (dibaca 3–4 Oktober 2026)

| # | Fakta | Lokasi |
|---|---|---|
| F35 | Berkas migrasi dibaca seluruhnya ke memori; hasil validasi per baris disimpan sebagai satu catatan; persetujuan batch berjalan dalam satu transaksi (semua item lahir, atau tidak sama sekali). Belum ada batas jumlah baris | `FinanceOpeningItemBatchService` |
| F36 | Lampiran HR dibatasi 25 MB per permintaan; kunci `MaxFileSizeBytes` bukti pembayaran sengaja belum diberi nilai bawaan | `WorkflowAttachmentController`; `Program.cs` baris 946 |
| F37 | Piutang non-pasien (`FinNonPatientReceivable`) sengaja tidak masuk buku mutasi piutang dan tidak disentuh amandemen migrasi | `02-backend-architecture.md` bagian migrasi |
| F38 | Tabel utang jasa medis di Finance belum terisi; task penulisnya `BE-FIN-021` `BLOCKED`; snapshot mengirimnya `0,00` | `FIN-DEC-122`, roadmap backend |
| F39 | Tanggal perubahan terakhir ambang sudah dicatat kolom audit tabel itu (`UpdateDateTime`, atau `CreateDateTime` bila belum pernah diubah) | `MstDirectPaymentThreshold`, `DirectPaymentThresholdService` |

### Keputusan

| ID | Pertanyaan | Keputusan | Dasar |
|---|---|---|---|
| `FIN-DEC-153` | `FIN-OQ-083` — nasib kolom fisik tanggal berlaku pada tabel ambang | **Kolom dihapus** pada migration yang **sama** dengan penanda versi ambang (`FIN-DEC-146`). Kapan ambang terakhir diubah tetap terbaca dari kolom audit dan log. Menyempurnakan `FIN-DEC-145` | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-154` | `FIN-OQ-074` — nilai awal ambang | **Sistem tidak mengisi angka bawaan.** Pejabat berwenang menetapkan ambang pertama lewat layar ambang sebagai **prasyarat go-live**; angka dan alasannya tercatat di log seperti perubahan lain. Selama belum ditetapkan, seluruh pembayaran langsung tetap ditolak (`FIN-DES-086`). Bagian "siapa pejabat berwenang" adalah pemberian hak akses dan dicatat di luar scope | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-155` | `FIN-OQ-076` — ukuran batch migrasi | **Maksimal 10.000 baris per berkas.** Berkas yang melewatinya **ditolak** beserta pesan yang menyebut batas dan menyarankan memecah berkas. Tagihan yang lebih banyak diunggah sebagai beberapa batch; **setiap batch** menyatakan saldo awal Accounting-nya sendiri, direkonsiliasi sendiri, dan disetujui sendiri (persetujuan tetap utuh per batch, `FIN-DEC-129`). Angka 10.000 adalah titik tengah pilihan owner karena perkiraan jumlah tagihan belum disebut | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-156` | `FIN-OQ-082` — batas ukuran berkas bukti pembayaran | **10 MB** (10.485.760 byte) per berkas. Nilainya diisi pada konfigurasi `FinanceManagement:TransactionProof:MaxFileSizeBytes`. Perilaku fail-closed `503` tanpa nilai tetap berlaku. Berkas yang lebih besar ditolak `413` | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-157` | `FIN-OQ-065` — utang jasa medis lama | **Tidak dimigrasikan pada rilis ini.** Saldo awal kelompok ini tetap nol (`FIN-VAL-181`) dan batch tidak mengenal jenis item jasa medis. Dibahas lagi bersama modul Medical Fee ketika penulis datanya (`BE-FIN-021`) tidak lagi `BLOCKED` | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-158` | `FIN-OQ-069` — piutang sewa non-pasien lama | **Tidak lewat batch migrasi.** Dicatat lewat layar piutang non-pasien yang sudah ada. Karena berada di luar buku mutasi, ia tidak memengaruhi posisi saldo maupun snapshot | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-159` | `FIN-OQ-057` — skenario saldo negatif selain Kas Kasir | **Tidak perlu daftar skenario.** Piutang atau utang supplier yang bersaldo negatif pada penutupan bulan **tetap diterbitkan apa adanya dan ditandai**; tidak memblokir penerbitan; penelusuran dilakukan manual. Meneguhkan `FIN-DEC-112` | Jawaban owner, rekomendasi dipilih |
| `FIN-DEC-160` | Urutan butir menu di submenu "Cutover & Subledger" | **Pemetaan Akun Control → Saldo Awal Cutover → Batch Migrasi Tagihan Lama**, mengikuti urutan kerja petugas. Menggantikan asumsi pada pass sebelumnya; memperjelas `FIN-DEC-148` | Jawaban owner, rekomendasi dipilih |

### Rincian dan contoh

**Ukuran batch (`FIN-DEC-155`).** Rumah sakit memiliki 23.000 tagihan piutang lama. Staf membaginya menjadi tiga
berkas (10.000, 10.000, dan 3.000 baris) lalu mengunggah masing-masing. Setiap batch divalidasi, dinyatakan saldo
awal Accounting-nya (misalnya tiga dokumen rujukan berbeda), direkonsiliasi, dan disetujui terpisah. Bila batch
kedua bergalat, ia saja yang ditahan; batch pertama yang sudah disetujui tidak terpengaruh. Bila staf mencoba
mengunggah 12.000 baris sekaligus, sistem menolak dengan pesan sejenis *"Berkas memuat lebih dari 10.000 baris.
Pecah menjadi beberapa berkas."*

**Berkas bukti (`FIN-DEC-156`).** Petugas memotret kuitansi dengan ponsel; hasilnya 4 MB dan diterima. Foto 14 MB
ditolak dengan pesan *"Ukuran berkas melewati batas yang diizinkan."* dan petugas mengecilkannya lebih dulu.

**Ambang saat go-live (`FIN-DEC-154`).** Pada hari persiapan go-live, pejabat berwenang membuka layar ambang yang
berjudul *Tetapkan ambang pertama*, mengisi angka dan alasannya, lalu menyimpan. Sampai langkah itu dilakukan, staf
yang mencoba pembayaran langsung melihat pesan bahwa ambang belum ditetapkan.

**Saldo negatif (`FIN-DEC-159`).** Piutang penjamin tertentu kelebihan bayar sehingga saldonya −Rp 2.000.000 pada
penutupan bulan. Snapshot tetap terbit dengan −Rp 2.000.000, layar menandainya "berlawanan dengan saldo normal akun",
dan staf menelusurinya secara manual.

### Akibat yang MUST dijaga

1. **Aturan validasi baru** diperlukan untuk batas 10.000 baris pada unggah dan unggah ulang batch; nomor
   `FIN-VAL` dan kode statusnya digambar pass desain.
2. **Migration ambang menjadi satu**: menambah penanda versi **dan** menghapus kolom tanggal berlaku
   (`FIN-DEC-146` + `153`). Membuatnya memakai wewenang `FIN-DEC-138`; **menerapkannya tetap milik Yasmin**.
3. **Konfigurasi `MaxFileSizeBytes` bernilai 10.485.760** MUST diisi sebelum go-live; tanpa itu unggah bukti tetap
   `503`. Ini langkah serah terima, bukan kode.
4. **Daftar prasyarat go-live** bertambah dan MUST dibawa serah terima modul: ambang pertama ditetapkan pejabat;
   peran staf AR/AP diberi hak baca ambang (`FIN-DEC-147`, urusan admin); konfigurasi ukuran berkas diisi;
   migration yang tertunda diterapkan.
5. **Kontrak dan matriks yang STALE** dari pass sebelumnya masih berlaku dan bertambah: kontrak unggah batch
   (batas baris), data dictionary ambang (kolom tanggal berlaku dihapus), `validation-matrix.md`. Seluruhnya
   **MUST** diperbarui lewat `/design-business-module`, bukan di sini.
6. **Utang jasa medis** tetap nol dan kembali ke meja bersama Medical Fee; batas modulnya tidak berubah
   (Medical Fee berhenti pada jasa kotor dan tidak pernah menulis ke tabel Finance).
7. **Batch pecahan membuat rekonsiliasi lebih banyak.** Staf perlu satu dokumen rujukan Accounting per batch; layar
   batch sudah mewajibkan rujukan dokumen per batch, sehingga tidak ada perubahan layar untuk ini.

### Frontend Decision Authority — tambahan

| Keputusan | Pemilik | Status | Rentang yang diizinkan |
|---|---|---|---|
| Urutan butir menu submenu "Cutover & Subledger" (`FIN-DEC-160`) | Owner | approved | Pemetaan Akun Control → Saldo Awal Cutover → Batch Migrasi Tagihan Lama. Menggantikan asumsi sebelumnya |
| Pesan penolakan batas baris dan batas ukuran berkas | Owner | approved | Wajib menyebut batas yang dilanggar dan langkah perbaikan; redaksi persisnya `DEV_DISCRETION` |

### Open question

| ID | Status | Keterangan |
|---|---|---|
| `FIN-OQ-083` | **CLOSED** oleh `FIN-DEC-153` | — |
| `FIN-OQ-074` | **CLOSED** oleh `FIN-DEC-154` | Cara penetapan diputuskan; **angka**-nya diisi pejabat saat go-live (prasyarat go-live, bukan open question). Siapa yang diberi hak mengubah ambang tetap urusan admin |
| `FIN-OQ-076` | **CLOSED** oleh `FIN-DEC-155` | — |
| `FIN-OQ-082` | **CLOSED** oleh `FIN-DEC-156` | Nilai diisi pada konfigurasi saat go-live |
| `FIN-OQ-065` | **CLOSED** (untuk rilis ini) oleh `FIN-DEC-157` | Dibuka kembali bersama Medical Fee ketika `BE-FIN-021` tidak lagi `BLOCKED` |
| `FIN-OQ-069` | **CLOSED** oleh `FIN-DEC-158` | — |
| `FIN-OQ-057` | **CLOSED** oleh `FIN-DEC-159` | — |
| `FIN-OQ-045`, `047`, `048` | **TERBUKA**, milik Accounting | Tidak dapat diputuskan Finance |
| `FIN-OQ-078` | **TERBUKA**, milik Accounting | `LATER SLICE` |
| `FIN-OQ-081` | **DITUNDA** (`FIN-DEC-149`) | Tidak memblokir rilis pertama |

### Kriteria penerimaan tambahan

1. Berkas batch berisi tepat 10.000 baris diterima; berisi 10.001 baris ditolak beserta pesan yang menyebut batas dan menyarankan memecah. Berlaku juga pada unggah ulang.
2. Dua batch hasil pecahan direkonsiliasi dan disetujui terpisah; batch yang bergalat tidak mengubah batch lain yang sudah disetujui.
3. Berkas bukti berukuran sampai 10.485.760 byte diterima, lebih dari itu ditolak `413`; tanpa nilai konfigurasi, unggah bukti tetap `503`.
4. Tabel ambang tidak lagi memiliki kolom tanggal berlaku dan memiliki penanda versi, keduanya pada migration yang sama.
5. Daftar prasyarat go-live memuat: penetapan ambang pertama, hak baca ambang bagi peran AR/AP, konfigurasi ukuran berkas bukti, dan penerapan migration yang tertunda.
6. Saldo awal utang jasa medis tetap nol dan batch tidak menawarkan jenis item jasa medis.
7. Piutang non-pasien tidak muncul pada batch migrasi; pencatatannya lewat layar piutang non-pasien.
8. Snapshot bulan yang memuat saldo negatif pada piutang atau utang supplier terbit apa adanya dan tertandai, tanpa penahanan.
9. Butir menu "Cutover & Subledger" berurutan: Pemetaan Akun Control, Saldo Awal Cutover, Batch Migrasi Tagihan Lama.

### Langkah berikutnya

**Tidak ada blocker desain dari sisi Finance.** Yang tersisa terbuka milik Accounting (`FIN-OQ-045`, `047`, `048`,
`078`) dan tidak memblokir desain; `045`/`047` memblokir `IMPLEMENTATION` sisi `PENERIMAAN-KASIR` dan
`PEMBUKAAN-SHIFT-KASIR` sebagaimana sudah tercatat. Pass ini **tidak** menulis kontrak, arsitektur, roadmap,
migration, endpoint, atau UI.

