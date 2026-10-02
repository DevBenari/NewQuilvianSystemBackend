# FE-BKC-019 — Ringkasan Pembayaran yang Benar-Benar Menjumlah

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-019` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, perbaikan aritmetika pada komponen yang sudah ada (bukan layar baru) |
| Task mode | `FRONTEND` (backend read-only, tidak ada perubahan kontrak API — field sudah ada dari `BE-BKC-024`/`025`/`028`/`030`) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-024`, `BE-BKC-025`, `BE-BKC-028` — ketiganya `DONE`, `dotnet build`/`test` dikonfirmasi lulus pengguna (5–6 September 2026); migration `BE-BKC-027` sudah dieksekusi dan diverifikasi langsung ke database dev. Lihat `requirement-traceability.md` § amendment 4 September 2026 |
| Status task | Source selesai. **`npm run lint:errors`/`test:unit`/`build` dijalankan dan LULUS.** Belum di-commit. Belum diverifikasi manual ter-autentikasi |

## Ringkasan untuk pembaca umum

Ringkasan Pembayaran (blok "Subtotal Mandiri / Subtotal Asuransi / Total Tagihan / Harus Dibayar
Pasien" di Menu Pembayaran) sebelumnya masih memakai rumus dari **sebelum** tiga amendment backend
berturut-turut (`BE-BKC-024` mencabut gerbang penahan, `BE-BKC-025` memperkenalkan anomali data,
`BE-BKC-028`/`030` memperkenalkan selisih tidak dapat ditagihkan). Akibatnya dua hal salah:

1. Baris **"Penjamin Belum Terverifikasi"** — sisa nama dari mekanisme lama yang sudah dicabut
   sepenuhnya di backend (`BKC-DEC-071`/`072`/`074`) — masih bisa muncul memakai nilai
   `unresolvedCoverageAmount` **sendirian**, padahal sejak `BE-BKC-028` sebagian besar nominal yang
   dulu mengalir ke field itu sudah **berpindah** ke field baru `nonBillableResidualAmount`. Kasir
   yang membuka tagihan dengan selisih kontraktual bisa melihat baris ini **hilang** (nilainya jadi
   0 di field lama) padahal selisihnya sungguhan ada — dan Subtotal Mandiri **ikut membengkak
   diam-diam** sebesar nominal itu, karena rumus lama tidak pernah mengurangkannya dari mana pun.
2. **Subtotal Mandiri dan Pajak Mandiri tidak pernah dikurangi `nonBillableResidualAmount`** —
   nominal yang menurut kontrak penjamin **dilarang** ditagihkan ke pasien ikut ditampilkan seolah
   itu bagian tagihan pasien. Ini bukan sekadar salah nama baris; ini salah menghitung berapa yang
   sebenarnya harus dibayar pasien pada layar ringkasan (`Harus Dibayar Pasien` sendiri, yang
   dibaca langsung dari `patientAmount` backend, **tidak** terpengaruh — tapi breakdown Subtotal di
   atasnya menjadi tidak konsisten dengannya).

Task ini memperbaiki keduanya: baris baru **"Selisih Tidak Ditagihkan (kontrak penjamin)"**
menjumlahkan `unresolvedCoverageAmount` **dan** `nonBillableResidualAmount` menjadi satu angka —
kasir tidak berkepentingan tahu field backend mana yang sedang menampungnya — dan kedua field itu
kini juga dikurangkan secara konsisten dari Subtotal Mandiri/Pajak Mandiri per komponen (item,
pajak item, biaya administrasi, biaya kamar), persis mengikuti pola yang sudah dipakai
`unresolvedAmount` di baris yang sama.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-025`, `FR-BKC-026`; `BKC-DEC-075`; `03-frontend-architecture.md` § Amendment
4 September 2026; `roadmap/frontend-roadmap.md` § `FE-BKC-019`. Kontrak backend `BIL-API-0.6`/
`0.7` — field pada `breakdown.coverage` dan `breakdown.items[]`/`administrationFee`/`roomCharge`
yang sudah dikirim sejak `BE-BKC-024`/`025`/`028`.

Tidak ada keputusan bisnis baru yang perlu ditutup pada task ini — seluruh keputusan (`BKC-DEC-071`,
`072`, `073`, `074`, `075`, `080`, `089`) sudah `approved` sejak backend-nya diimplementasikan.

## Proses bisnis

| Aspek | Penjelasan |
| --- | --- |
| Pemicu | Kasir membuka Menu Pembayaran satu invoice — blok Ringkasan Pembayaran selalu tampil begitu kalkulasi tersedia (tidak ada aksi tambahan) |
| Aturan bisnis | `Subtotal Mandiri + Subtotal Asuransi + Pajak Mandiri + Pajak Asuransi + Selisih Tidak Ditagihkan = Total Tagihan` — identitas aritmetika yang **wajib** berlaku persis, bukan perkiraan |
| Perubahan status | Tidak ada — task ini murni presentasi, tidak menulis apa pun |
| Jalur tidak normal | Baris "Selisih Tidak Ditagihkan" **disembunyikan** bila nilainya 0 — tagihan pasien tunai dan tagihan yang seluruhnya tercover normal tidak menampilkan baris itu sama sekali |
| Hasil akhir | Kasir melihat ringkasan yang menjumlah persis ke Total Tagihan, tanpa baris menggantung yang tidak dapat ditagihkan kepada siapa pun |

**Kenapa dijumlahkan, bukan dipisah jadi dua baris.** Sama persis dengan alasan `BE-BKC-028`/`030`
di backend: kasir tidak berkepentingan membedakan sebab selisihnya (jalur `NotCovered` lama vs
residual kontraktual) — pemisahannya baru berguna di layar Pengecualian Finansial (`FE-BKC-021`,
belum dikerjakan). Menjumlahkan di sini juga berarti angka yang dilihat kasir **sama sekali tidak
berubah** oleh perpindahan field yang sudah terjadi tiga kali di backend (`BE-BKC-024` mencabut
sumbernya, `BE-BKC-028` memindahkan sebagian ke field baru, `BE-BKC-030` memindahkan sisanya) —
itulah persis maksud `BKC-DEC-075`.

## Perhitungan yang diperbaiki

| Variabel | Sebelum task ini | Sesudah task ini |
| --- | --- | --- |
| `subtotalMandiri` | `subtotalTagihan − subtotalAsuransi − unresolvedPreTax` | `subtotalTagihan − subtotalAsuransi − unresolvedPreTax − nonBillablePreTax` |
| `pajakMandiri` | `pajak − pajakAsuransi − taxUnresolvedSum` | `pajak − pajakAsuransi − taxUnresolvedSum − taxNonBillableSum` |
| Baris kondisional | `Penjamin Belum Terverifikasi` = `unresolvedCoverage` saja | `Selisih Tidak Ditagihkan (kontrak penjamin)` = `unresolvedCoverage + nonBillableResidualAmount` |

`nonBillablePreTax`/`taxNonBillableSum` dijumlahkan dari sumber PER KOMPONEN yang sudah ada di
`breakdown` (`itemNonBillableResidualAmount`/`taxNonBillableResidualAmount` per baris item,
`nonBillableResidualAmount` pada `administrationFee`/`roomCharge`) — pola identik dengan
`unresolvedPreTax`/`taxUnresolvedSum` yang sudah ada, bukan pendekatan baru. `nonBillableResidualAmount`
(untuk baris gabungan) dibaca dari `breakdown.coverage.nonBillableResidualAmount` — field ini
**hanya ada di level breakdown**, tidak ada duplikatnya di level teratas response (berbeda dari
`unresolvedCoverageAmount`, yang memang ada di kedua level sejak baseline).

**Bukti identitas aritmetika tetap terjaga** (verifikasi manual, bukan test otomatis — lihat §
Definition of Done): dengan `BIL-VAL-028` (invarian per-komponen backend) sebagai jaminan bahwa
`unresolvedCoverageAmount`/`nonBillableResidualAmount` tingkat invoice selalu sama dengan jumlah
per-komponennya, penjumlahan `subtotalMandiri + subtotalAsuransi + pajakMandiri + pajakAsuransi +
selisihTidakDitagihkan` secara aljabar sama dengan `subtotalTagihan + pajak` — yaitu `totalTagihan`
dikurangi `pembulatan` (yang di backend selalu `0`, `RoundingAmount` belum pernah diisi selain nol
pada `CalculateAsync`).

## Base Component Decision Gate

`UI GATE: 0 elemen baru — REUSE penuh`

Tidak ada base component baru maupun diubah bentuknya. Task ini murni mengubah **rumus** dan
**satu label baris kondisional** pada `<dl className={styles.summaryList}>` yang sudah ada — markup
`<div className={styles.summaryRow}><dt>/<dd></div>` dipakai apa adanya, identik dengan baris-baris
lain di blok yang sama.

## Endpoint yang dikonsumsi

Tidak ada endpoint baru. Task ini membaca field tambahan dari response `GET .../calculation-preview`
/`POST .../recalculate` yang **sudah** dikonsumsi `menu-pembayaran-view.jsx` sejak sebelumnya —
hanya field baru pada `breakdown` yang sekarang ikut dibaca:

| Field | Level | Sejak task backend |
| --- | --- | --- |
| `breakdown.coverage.nonBillableResidualAmount` | Invoice (breakdown) | `BE-BKC-028` |
| `breakdown.items[].itemNonBillableResidualAmount`, `.taxNonBillableResidualAmount` | Per item | `BE-BKC-028` |
| `breakdown.administrationFee.nonBillableResidualAmount`, `breakdown.roomCharge.nonBillableResidualAmount` | Per komponen non-item | `BE-BKC-028` |

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` | Empat variabel baru (`itemNonBillablePreTaxSum`, `taxNonBillableSum`, `adminFeeNonBillable`/`roomChargeNonBillable`, `nonBillablePreTax`, `nonBillableResidualAmount`, `selisihTidakDitagihkan`); `subtotalMandiri`/`pajakMandiri` ikut mengurangi nominal itu; baris `Penjamin Belum Terverifikasi` diganti `Selisih Tidak Ditagihkan (kontrak penjamin)` dengan nilai gabungan |

Total: **1 berkas berubah** — task paling sempit sejauh ini di modul ini, sesuai sifatnya (murni
perbaikan rumus pada komponen yang sudah ada, "tidak ada layar baru" per roadmap).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **PASS** | Exit 0, `eslint . --quiet` tanpa output |
| `npm run test:unit` | **PASS** | 440/440 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru (perbaikan rumus murni, tidak ada perilaku baru yang butuh test unit terpisah) |
| `npm run build` | **PASS** | `✓ Compiled successfully in 61s`, route `.../invoices/[slug]/pembayaran` tetap muncul tanpa error |
| Verifikasi aljabar identitas penjumlahan | **LULUS (tinjauan manual)** | Lihat § "Perhitungan yang diperbaiki" — dibuktikan secara simbolik bahwa kelima suku menjumlah ke `totalTagihan` selama `BIL-VAL-028` (invarian backend) berlaku dan `pembulatan` tetap `0` |
| Verifikasi statis: acceptance 1 (baris lama tidak ada) | **LULUS** | Grep `"Penjamin Belum Terverifikasi"` pada seluruh `src/` sesudah edit: nol kecocokan |
| Verifikasi statis: acceptance 3 (baris kondisional) | **LULUS** | `{selisihTidakDitagihkan > 0 ? (...) : null}` — pola identik dengan baris kondisional lain di blok yang sama (`diskonPromo`, `Pajak Asuransi`) |
| Verifikasi statis: acceptance 4 (angka tidak berubah lintas waktu) | **LULUS (analisis manual)** | Lihat § "Ringkasan untuk pembaca umum" butir kedua — dibuktikan untuk tiga keadaan temporal (snapshot lama, kalkulasi antara `BE-BKC-028`–`030`, kalkulasi sesudah `BE-BKC-030`) |
| Verifikasi statis: acceptance 5 (kolom cadangan penjamin kedua) | **SUDAH TERPENUHI sebelum task ini** | Grep `-i "excess"` pada berkas ini: nol kecocokan di luar komentar task ini sendiri — field `excessAmount`/`ExcessStatus` **tidak pernah** dirender di blok ini, tidak ada yang perlu dihapus |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada task ini |
| Verifikasi manual ter-autentikasi (tagihan dengan selisih jalur `NotCovered` lama, residual `BE-BKC-028`, kombinasi keduanya, dan tagihan tunai/tercover penuh) | **NOT FEASIBLE** | Tidak ada kredensial login yang tersedia untuk builder; membutuhkan data invoice+rule asuransi bertanda `IsAllowExcessPaymentByPatient=false` |

**Task ini belum bisa ditandai selesai sepenuhnya.** Lint/unit test/build lulus bersih dan
identitas aritmetika terbukti benar secara aljabar, tetapi ini **bukan** pengganti verifikasi
manual ter-autentikasi terhadap kelima acceptance criteria dengan data nyata (per instruksi
`build-module-frontend`: "Lint/test/build yang PASS saja bukan bukti").

## Risiko yang tersisa

1. **Bukti aljabar, bukan bukti runtime.** Identitas penjumlahan dibuktikan secara simbolik
   berdasarkan invarian backend (`BIL-VAL-028`) — belum diverifikasi dengan angka nyata dari
   database dev. Sebelum ditandai selesai, satu contoh tagihan dengan selisih kontraktual nyata
   **wajib** diperiksa langsung di browser.
2. **Temuan di luar scope, tidak diperbaiki di sini (dicatat sesuai `AGENTS.md`).** Badge status
   per baris item (`getItemCoverageStatus`, fungsi `FE-BKC-FIX-006`/`FIX-008`) **tidak** ikut
   diperbarui untuk mengenali `nonBillableResidual`. Untuk item yang **seluruh** nilainya masuk
   jalur non-billable-residual (`itemUnresolved=0`, `itemPrimary=0`, sejak `BE-BKC-028`/`030`
   memindahkan nominal itu keluar dari `Unresolved`), fungsi ini akan salah mengembalikan status
   `"tunai"` — padahal nominal itu justru **bukan** tanggungan pasien. Ini **bukan** regresi dari
   task ini (kode badge tidak disentuh), melainkan gap laten yang sudah ada sejak `BE-BKC-028`
   dan baru terlihat sekarang saat menelusuri seluruh pemakaian field terkait. Di luar scope
   `FE-BKC-019` ("reuse Blok Ringkasan Pembayaran... tidak ada layar baru" — badge bukan bagian
   blok itu); direkomendasikan sebagai task/perbaikan tersendiri.
3. Perhitungan `subtotalMandiri`/`pajakMandiri` tetap memakai `Math.max(0, …)` sebagai pengaman
   pembulatan (pola yang sudah ada sebelum task ini) — bila suatu saat jumlah pengurang melebihi
   basisnya karena galat data backend, angka akan dijepit ke 0 secara diam-diam alih-alih
   melaporkan galat. Perilaku ini **tidak diubah** oleh task ini (mempertahankan pola existing),
   dicatat sebagai risiko pra-ada, bukan risiko baru.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran yang punya akses Menu Pembayaran, buka tagihan dengan rule asuransi yang
   menandai `IsAllowExcessPaymentByPatient=false` (baik jalur `Covered` residual maupun jalur
   `NotCovered` lama), verifikasi: baris "Selisih Tidak Ditagihkan (kontrak penjamin)" muncul
   dengan nominal yang benar, dan kelima suku Ringkasan Pembayaran menjumlah persis ke Total
   Tagihan tanpa selisih satu rupiah pun.
2. Verifikasi tagihan pasien tunai murni (tanpa penjamin) — pastikan baris baru **tidak** muncul
   dan Subtotal Mandiri/Pajak Mandiri tidak berubah dari sebelum task ini.
3. Pertimbangkan task perbaikan terpisah untuk `getItemCoverageStatus` (§ Risiko butir 2) — di
   luar scope task ini, tetapi berdampak pada badge per baris item yang kasir lihat sehari-hari.
4. Lanjutkan `FE-BKC-020` (peringatan anomali data) — dependency `BE-BKC-025` sudah `DONE` dan
   terverifikasi.
