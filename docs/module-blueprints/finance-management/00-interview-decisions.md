# Finance Management — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `FIN-BP-001` |
| Revision | `1` |
| Status | `approved` untuk 81 keputusan (`FIN-DEC-001`..`081`) — seluruh blocker Phase 0, aturan inti AR/AP/Cash Management, UI brief frontend `FE-FIN-*` (`FIN-DEC-024`..`029`), dampak balasan Accounting (`FIN-DEC-030`..`039`), koreksi hasil impact scan `/design-business-module` atas rumpun uang muka/deposit/selisih kas (`FIN-DEC-040`..`044`), rumpun BARU Purchasing/AP penuh + agregasi AR + potongan sisi penerimaan dipicu evidence `Keuangan.md` (`FIN-DEC-045`..`051`), closure pass lanjutan yang menutup ambang nominal AP + kode PPN + status pajak AR (`FIN-DEC-052`..`055`), penyempitan gerbang `/plan-module-delivery` untuk `EPIC FIN-15` (`FIN-DEC-056`), `/grill-me` closure pass yang menutup empat pertanyaan `/plan-module-delivery` (`FIN-DEC-057`..`060`, 25 September 2026), `/grill-me` closure pass atas balasan Accounting `evidence/14` (`FIN-DEC-063`..`071`, 28 September 2026), `/grill-me` amendment pass atas temuan `/trace-existing-capabilities` REVISI 6 (`FIN-DEC-072`..`076`, 28 September 2026), penutupan `FIN-OQ-034` via `BKC-DEC-128`..`131` (`FIN-DEC-077`), `/grill-me` amendment pass penyelarasan resource hak akses (`FIN-DEC-078`..`079`, 28 September 2026), serta ratifikasi pemicu pembalikan uang muka deposit `FIN-DES-064`..`065` (`FIN-DEC-080`..`081`, 29 September 2026). Seluruh open question antar-modul `FIN-OQ-034`, `FIN-CQ-08`, `FIN-OQ-036`, dan `FIN-OQ-037` (via `BKC-DEC-132`..`134`) kini `closed`. |
| Pass | `Scope pass` — selesai 20 September 2026 · `Closure pass` — selesai 20 September 2026 · `Amendment pass` (UI brief `FE-FIN-*`) — selesai 23 September 2026 · `Amendment pass` (balasan Accounting, evidence 13) — selesai 25 September 2026 · `Amendment pass lanjutan` (koreksi impact scan `/design-business-module`) — selesai 25 September 2026 · `Amendment pass` (rumpun Purchasing/AP penuh + agregasi AR, evidence `Keuangan.md`) — selesai 25 September 2026 · `Closure pass lanjutan` (ambang nominal, kode PPN, status pajak AR) — selesai 25 September 2026 · `Closure pass` (balasan Accounting `evidence/14`) — selesai 28 September 2026 · `Amendment pass` (temuan `/trace-existing-capabilities` atas REVISI 6) — selesai 28 September 2026 · `Amendment pass` (penyelarasan resource hak akses, `FIN-CQ-08`) — selesai 28 September 2026 |
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
| `FIN-DEC-060` | Decision | **Menutup `FIN-OQ-022`.** Lima layar (Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian & Deposit Retur, Laporan Pembelian) masuk **submenu baru "Pembelian"** pada group `corporateFinance`, sejajar submenu Master Data yang sudah ada — memisahkan konsep "proses pembelian" dari "Faktur & Tagihan Supplier" (layar existing di `/finance/payable/invoice`) yang tetap di bawah pengelompokan Utang, karena layar itu daftar utang, bukan proses pembelian. Purchasing Invoice diberi label **"Faktur Pembelian"** supaya tidak tertukar dengan "Faktur & Tagihan Supplier". Batch Tagihan AR menjadi **satu butir flat "Tagihan Gabungan Penjamin"**, sejajar butir "Piutang" yang sudah ada — bukan bagian submenu Pembelian karena ia rumpun AR, bukan AP. | Yasmin (Product Owner Finance) | `approved` | Yasmin, 25 September 2026 | Jawaban eksplisit owner atas opsi yang direkomendasikan |
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

**Yang TIDAK ditutup pass ini, dan MUST ditindaklanjuti:**

- **Pemetaan payung-ke-granular** (`Finance.AP` → tujuh resource, `Finance.AR` → resource AR yang
  bersangkutan) **belum digambar**. Ini pekerjaan kecil `/design-business-module` atau langsung
  penyuntingan `contracts/permission-audit-matrix.md` — bukan keluaran `/grill-me`.
- **Migrasi data peran** untuk keenam resource yang berganti nama (`FIN-DEC-078`) belum dirancang.
  Ini menyentuh data peran yang mungkin sudah disemai di lingkungan pengembangan — **MUST**
  diperlakukan hati-hati, bukan sekadar `UPDATE` yang diimprovisasi saat implementasi.
- Pemeriksaan `rg` awal **tidak menemukan** berkas seeder statis yang secara eksplisit memuat
  keenam nama pendek ini sebagai data peran — kemungkinan mekanisme penyemaiannya berbasis registry
  runtime yang dipindai dari atribut `[AccessController]`/`[AccessPermission]` saat start-up,
  ditambah tabel peran-ke-izin di database. **Ini belum diverifikasi tuntas** dan MUST diperiksa
  `/trace-existing-capabilities` sebelum task rename dieksekusi, supaya tahu persis apa yang perlu
  dimigrasi.

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

