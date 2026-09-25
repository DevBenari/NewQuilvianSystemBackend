# Finance Management — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `FIN-BP-001` |
| Revision | `1` |
| Status | `approved` untuk 44 keputusan (`FIN-DEC-001`..`044`) — seluruh blocker Phase 0, aturan inti AR/AP/Cash Management, UI brief frontend `FE-FIN-*` (`FIN-DEC-024`..`029`), dampak balasan Accounting (`FIN-DEC-030`..`039`), dan koreksi hasil impact scan `/design-business-module` atas rumpun uang muka/deposit/selisih kas (`FIN-DEC-040`..`044`, Amendment pass lanjutan 25 September 2026) tertutup. `FIN-DEC-004`, `FIN-DEC-007`, `FIN-DEC-023` `superseded` oleh amendment pertama; `FIN-DEC-032`, `FIN-DEC-033`, dan sebagian `FIN-DEC-034` `superseded` oleh amendment lanjutan — lihat masing-masing untuk penggantinya. Item non-blocking tetap `open`: `FIN-OQ-010` (ambang nominal AP), `FIN-OQ-015` (lingkup detail cetak/unduh), `FIN-OQ-016` (mekanisme token akun layanan), `FIN-OQ-017` (ratifikasi balik Accounting atas TUJUH kode baru — naik dari empat, lihat `FIN-DEC-040`..`044`), `FIN-OQ-018` (lawan jurnal refund kategori BILLING non-kelebihan-bayar). `FIN-OQ-011` `closed` oleh `FIN-DEC-035`. Lihat `FIN-CQ-02` untuk satu closure question yang didelegasikan ke `/design-business-module`. |
| Pass | `Scope pass` — selesai 20 September 2026 · `Closure pass` — selesai 20 September 2026 · `Amendment pass` (UI brief `FE-FIN-*`) — selesai 23 September 2026 · `Amendment pass` (balasan Accounting, evidence 13) — selesai 25 September 2026 · `Amendment pass lanjutan` (koreksi impact scan `/design-business-module`) — selesai 25 September 2026 |
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
| `FIN-DEC-015` | Decision | `FinSupplierPayable` untuk rilis pertama dipicu lewat INPUT MANUAL staf Finance AP (nomor invoice supplier, nominal, termin) — tidak menunggu modul Purchase Order/Receive Order/Supplier Invoice yang saat ini belum ada satu barisnya pun di backend. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` — konfirmasi langsung tidak ada modul Purchasing; `FIN-BRD-V2-0.3` bagian 7.1 |
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
| `FIN-DEC-041` | Decision | Kode kejadian baru **`PENGEMBALIAN-UANG-MUKA`** — khusus pengembalian TUNAI dari Uang Muka Pasien ke pasien (lawan jurnal: Debit Uang Muka Pasien, Kredit Kas — kas benar-benar keluar). Dipicu dari DUA sumber: (a) `BilDepositMovement` bertipe `RELEASE`; (b) `BilRefundCase` berstatus `EXECUTED` **khusus** bila `RefundableCredit.SourceType = ALLOCATION_EXCESS` (kelebihan bayar yang sudah direklasifikasi lewat `FIN-DEC-042`). **Cakupan ini SENGAJA TIDAK memasukkan** `BilRefundCase` dengan `RefundCategory = "BILLING"` bersumber `SourceType SETTLEMENT` atau `REFERRED_OUTPATIENT_ADMIN` — sifat akuntansinya belum digali dan berpotensi berbeda (kemungkinan koreksi piutang/pendapatan, bukan penarikan Uang Muka Pasien). Dicatat `FIN-OQ-018`, tidak menahan gerbang G6 karena permintaan Accounting hanya menyebut deposit pasien dan kelebihan bayar. Kode ke-3 dari 7 kode baru pada katalog, **butuh ratifikasi balik Accounting** (`FIN-OQ-017`). | Yasmin (Finance) | `approved` (sisi Finance, menunggu ratifikasi Accounting) | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `BilDepositMovement.MovementType.Release`, `BilRefundCase.Status.Executed`, `BilRefundableCredit.SourceType.AllocationExcess` |
| `FIN-DEC-042` | Decision | **Menggantikan `FIN-DEC-033`.** Kelebihan bayar (overpayment) TIDAK diperlakukan seolah diterima sebagai `PENERIMAAN-UANG-MUKA` sejak awal — itu tidak mungkin secara temporal karena Billing (`BilRefundableCredit`) baru mengakui kelebihan SETELAH alokasi pembayaran terjadi (`SourceType = ALLOCATION_EXCESS`, status `AVAILABLE`), bukan saat uang diterima. Sebagai gantinya: Finance menerbitkan kode kejadian baru **`PENGAKUAN-KELEBIHAN-BAYAR`** pada saat `BilRefundableCredit` diakui, yang memindahkan nilai kelebihan dari lawan jurnal penerimaan asli (mis. Piutang/Pendapatan) ke Uang Muka Pasien — TANPA menyentuh kas, karena kas sudah didebit penuh saat penerimaan asli. Pengembaliannya kelak (bila pasien memilih dikembalikan tunai, bukan dipakai) memakai `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`), karena saldo itu sekarang sudah berada di akun Uang Muka Pasien. Alasan: mencegah kewajiban ke pasien tak terlihat di buku besar antara pengakuan dan pengembalian, dan mencegah koreksi periode yang sudah ditutup Accounting saat pengembalian terjadi belakangan. Kode ke-4 dari 7 kode baru pada katalog, **butuh ratifikasi balik Accounting** (`FIN-OQ-017`). | Yasmin (Finance) | `approved` (sisi Finance, menunggu ratifikasi Accounting) | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `BilRefundableCredit.SourceType.AllocationExcess`, status `Available`/`Exhausted` |
| `FIN-DEC-043` | Decision | **Melengkapi `FIN-DEC-034`.** Kejadian `SELISIH-KAS-SHIFT` diterbitkan SAAT selisih disahkan — yaitu ketika `BilCashierShift.Status` menjadi `REVIEWED` (selisih hasil hitung fisik kas sudah disahkan, bukan sekadar terisi otomatis saat kas fisik diinput). `AccountingDate` pada kejadian ini MEMAKAI tanggal shift (`OpenedAt`/tanggal operasional shift), BUKAN tanggal pengesahan — supaya selisih tetap jatuh di periode akuntansi shift-nya, bukan periode saat direview. **Konsekuensi yang dicatat, bukan diputuskan di sini:** bila pengesahan terjadi setelah Accounting menutup periode shift itu, kejadian tetap terkirim tapi berpotensi tidak dapat dijurnalkan ke periode tertutup — perlu kesepakatan batas waktu pengesahan sebelum tutup bulan, dicatat sebagai titik sentuh untuk didiskusikan bersama Accounting saat gerbang G6 dibahas ulang. | Yasmin (Finance) | `approved` | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs` baris 737-738 (pembedaan `PhysicalCash` terisi vs status `REVIEWED`) |
| `FIN-DEC-044` | Decision | Kode kejadian baru **`PEMBALIKAN-PENERIMAAN-UANG-MUKA`** — pembalikan penerimaan uang muka/deposit yang tender aslinya di-reverse Billing. `FinanceReceiptService` MUST memilih kode pembalikan berdasarkan `EventTypeCode` yang diterbitkan saat penerimaan ASLI dibuat, bukan menghitung ulang status finalisasi tagihan saat ini (yang mungkin sudah berubah sejak penerimaan terjadi): bila asli `PENERIMAAN-UANG-MUKA`, pembalikannya `PEMBALIKAN-PENERIMAAN-UANG-MUKA`; bila asli `PENERIMAAN-KASIR`, pembalikannya tetap `PEMBALIKAN-PENERIMAAN-KASIR` (existing, tidak berubah). Mencegah lawan jurnal pembalikan Uang Muka Pasien (D Uang Muka Pasien, K Kas) tertukar dengan lawan jurnal pembalikan `PENERIMAAN-KASIR` (D Pendapatan/Piutang, K Kas). Kode ke-7 dari 7 kode baru pada katalog, **butuh ratifikasi balik Accounting** (`FIN-OQ-017`). | Yasmin (Finance) | `approved` (sisi Finance, menunggu ratifikasi Accounting) | Yasmin, 25 September 2026 | Impact scan `/design-business-module`, 25 September 2026 — `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` baris 190-206 |

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
| `FIN-OQ-010` | Ambang nominal approval berjenjang AP — bentuk aturannya sudah diputuskan `FIN-DEC-022`, angka rupiah persisnya belum. **Diperluas 20 September 2026** setelah bukti meeting masuk: ambangnya perlu ditetapkan **per jenis transaksi secara terpisah** (supplier vs dokter vs write-off AR), karena karakter risikonya berbeda — honor dokter adalah rekap bulanan yang menggabungkan puluhan sampai ratusan fee per dokter dengan nominal total besar, sedangkan write-off AR umumnya keputusan per-transaksi tunggal. Jejak audit juga MUST mencatat **tingkat approval mana** yang berlaku untuk tiap transaksi, bukan hanya siapa yang menyetujui. | Yasmin (Finance) + Finance Supervisor | `DESIGN` untuk `EPIC FIN-09` — tidak memblokir pemodelan data, hanya aturan validasi angka. `EPIC FIN-09` sudah `POST-MVP` |
| ~~`FIN-OQ-011`~~ | ~~Konfirmasi Accounting (Rizki) atas draf nama field saldo subledger per periode (`FIN-DEC-023`).~~ **DITUTUP 25 September 2026** — Accounting mengganti bentuknya, Finance menerima (`FIN-DEC-035`). | — | — |
| `FIN-OQ-016` | Mekanisme teknis token/kredensial akun layanan Finance→Accounting (mis. JWT Bearer berumur pendek vs mekanisme auth existing). Syarat organisasinya sudah diratifikasi (`FIN-DEC-036`); yang terbuka murni bentuk kredensialnya. | Platform + Yasmin (Finance) + Rizki (Accounting) | `DESIGN` — memblokir gerbang cutover G3/G4 milik Accounting, bukan pemodelan data Finance |
| `FIN-OQ-017` | **DIPERBARUI 25 September 2026 — naik dari empat menjadi TUJUH kode.** Ratifikasi balik Accounting (Rizki) atas tujuh kode kejadian baru yang diusulkan sisi Finance: `SALDO-SUBLEDGER` (`FIN-DEC-035`), `PENERIMAAN-UANG-MUKA` (`FIN-DEC-031`), `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`), `PENGAKUAN-KELEBIHAN-BAYAR` (`FIN-DEC-042`), `PEMAKAIAN-UANG-MUKA-DEPOSIT` yang cakupannya sudah dipersempit (`FIN-DEC-040`), `SELISIH-KAS-SHIFT` (`FIN-DEC-034`/`043`), dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` (`FIN-DEC-044`) — termasuk nama final dan urutan kode ke-18 s.d. 24 pada katalog. **`evidence/04-jawaban-atas-balasan-accounting.md` yang sudah dikirim ke Rizki hari ini HANYA menyebut empat kode lama (termasuk `PEMAKAIAN-UANG-MUKA-DEPOSIT` versi sebelum dipersempit) — MUST disusuli surat koreksi sebelum Accounting mulai menyusun aturan posting, supaya Rizki tidak meratifikasi kode yang keliru.** | Accounting (Rizki) | `DESIGN` — memblokir gerbang G6 dan `EPIC FIN-11`/rumpun uang muka sampai dikonfirmasi; TIDAK memblokir MVP-0/1/4 |
| `FIN-OQ-018` | Lawan jurnal `BilRefundCase` dengan `RefundCategory = "BILLING"` bersumber `SourceType SETTLEMENT` atau `REFERRED_OUTPATIENT_ADMIN` belum digali — sengaja DIKELUARKAN dari cakupan `PENGEMBALIAN-UANG-MUKA` (`FIN-DEC-041`) karena kemungkinan bukan penarikan Uang Muka Pasien, melainkan koreksi piutang/pendapatan. | Yasmin (Finance) | `LATER SLICE` — tidak memblokir G6 (permintaan Accounting hanya menyebut deposit pasien dan kelebihan bayar), tidak menahan MVP manapun saat ini |
| `FIN-CAP-STALE-001` | **Catatan, bukan pertanyaan tertutup.** Impact scan 25 September 2026 (backend SHA bergerak `09101d05` → `d6cdfaf9`) menemukan `BilCollectionHandoff` SUDAH terdaftar di `Repositories/ApplicationDbContext.cs` baris 619 — `DbSet<BilCollectionHandoff> BilCollectionHandoffs`. `blocking_questions` pada `blueprint-manifest.md` (`BILLING-COLLECTION-HANDOFF`, menahan gelombang MVP-2/MVP-3 lewat `FIN-DEC-005`) mungkin sudah gugur, TAPI belum diverifikasi apakah bentuk kolomnya cocok dengan yang diminta Finance. | Yasmin (Finance) | Rekomendasi: jalankan `trace-existing-capabilities` mode impact scan sebelum `blocking_questions` ini ditutup atau MVP-2/3 direncanakan |
| `FIN-CQ-02` (dari `01-existing-capability-map.md` §9) | DTO/response shape persis dan daftar `[AccessPermission]` per endpoint Petty Cash belum dibaca detail — dibutuhkan sebagai acuan pola saat mendesain endpoint AR/AP/Doctor Payment supaya konsisten. | Yasmin (Finance) | ~~`DESIGN`~~ — **DITUTUP 20 September 2026** oleh pass `/design-business-module` |
| ~~`FIN-OQ-013`~~, ~~`FIN-OQ-014`~~ | Keduanya **DITUTUP 20 September 2026** oleh amendment revisi 2 pada `FIN-BP-001`. `FIN-DES-025` menggantikan `FinDoctorPayable` dengan `FinMedicalServicePayable`; `FIN-DES-026`..`028` menambahkan `FinPaymentDeduction`, kolom `NetTransferAmount`, dan aturan bahwa potongan tidak mengurangi sisa utang. Rinciannya ada pada bagian AMENDMENT REVISI 2 di `02-backend-architecture.md`. Keputusan arsitekturnya masih `draft` dan menunggu persetujuan owner. | — | — |
| `FIN-OQ-014` | **Amendment yang dituntut pass Medical Fee, 20 September 2026.** `MF-DEC-002` memperluas penerima jasa menjadi dokter **dan** tenaga kesehatan lain, dan `MF-DEC-008` memutuskan `FinDoctorPayable` **digantikan** satu entity utang jasa tenaga medis yang membedakan jenis penerima lewat kolom — mengikuti pola `FinPayment` yang sudah melayani supplier dan dokter sekaligus. Berkas `FIN-BP-001` yang terdampak: `02-backend-architecture.md`, `erd/payable.md`, `erd/data-dictionary.md`, `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, `contracts/state-transition-matrix.md`, `04-prd-to-mvp.md` (`EPIC FIN-08`). | Yasmin (owner Finance) | `DESIGN` untuk `EPIC FIN-08` — sudah `POST-MVP` dan **nol baris kode**, sehingga tidak membongkar apa pun. Lihat `MF-CQ-02` pada blueprint `medical-fee` |
| `FIN-OQ-013` | **Gap yang dibuka pass Medical Fee, 20 September 2026.** `MF-DEC-005` memutuskan Medical Fee menyerahkan jasa **kotor**, dan seluruh potongan per penerima per periode — PPh 21, kasbon, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, iuran kerohanian — diterapkan **Finance** saat menyusun rekap pembayaran. `FinPayment` hasil rancangan `FIN-DES-015` **tidak punya tempat sama sekali** untuk itu: ia hanya memiliki `TotalAmount` dan alokasi ke utang. Konsekuensi yang MUST diselesaikan: (a) Finance perlu entity potongan/tambahan per pembayaran; (b) invariant `FinPayment.TotalAmount = Σ alokasi` pada `FIN-DES-015` **tidak lagi berlaku apa adanya** — nilai transfer bersih berbeda dari jumlah utang yang dilunasi. | Yasmin (owner Finance + Medical Fee) | `DESIGN` untuk `EPIC FIN-09` — sudah `POST-MVP` sehingga **tidak menahan MVP**, tetapi MUST ditutup sebelum rumpun pembayaran dibangun. Lihat `MF-CQ-01` pada blueprint `medical-fee` |
| `FIN-OQ-012` | Pembayaran honor dokter di RS MMC dipisah dua tahap — tanggal 5 untuk pasien pribadi dan asuransi yang sudah membayar ke rumah sakit, tanggal 10 untuk sisanya termasuk kasus rumah sakit menalangi dulu. Apakah pola itu dipertahankan di V2? Bila ya, Finance perlu cara mengetahui status bayar tagihan sumber untuk setiap utang dokter, dan jalur datanya belum dirancang sama sekali. | Yasmin (Finance) + Billing | `DESIGN` untuk `EPIC FIN-09` — sudah `POST-MVP`, tidak menahan MVP. Dibuka 20 September 2026 dari bukti meeting (`evidence/03-referensi-meeting-rs-mmc.md`) |
| `FIN-OQ-015` | Lingkup detail cetak/unduh pada layar Finance — dokumen apa yang boleh dicetak/diunduh (mis. daftar piutang, slip setoran, rekap pembayaran) dan format apa (PDF/Excel). Bentuk keputusannya sudah ditutup `FIN-DEC-029` (ditunda dari MVP, layar dibangun tanpa tombol cetak/unduh); yang masih terbuka hanya lingkup detailnya untuk task terpisah kelak. | Yasmin (Product Owner Finance) | `LATER SLICE` — tidak memblokir `FE-FIN-001`/`002`/`003`/`006`. Dibuka 23 September 2026, Amendment pass UI brief |

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
