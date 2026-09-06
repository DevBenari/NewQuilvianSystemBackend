# FE-BKC-018 — Tab "Invoice Asuransi" pada Halaman Dokumen Kasir

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-018` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, vertical slice baru (satu tab + satu pemanggilan data + satu komponen cetak) |
| Task mode | `FRONTEND` (backend read-only, tidak ada perubahan kontrak API — endpoint sudah ada dari `BE-BKC-023`) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-023` — **`dotnet build`/`test` dikonfirmasi lulus pengguna 5 September 2026** (per `requirement-traceability.md`); `BKC-GATE-03` **ditutup** 5 September 2026 lewat `BKC-DEC-092` (Security Owner: `BillingInvoice:Read` dipakai ulang apa adanya, tidak ada permission baru) — lihat `00-interview-decisions.md` § amendment 5 September 2026 |
| Status task | Source selesai. **`npm run lint:errors`/`test:unit`/`build` dijalankan dan LULUS** (giliran lanjutan — instruksi "Tanpa test build project" hanya berlaku pada giliran sebelumnya). Belum di-commit. Belum diverifikasi manual ter-autentikasi |

## Ringkasan untuk pembaca umum

Halaman Dokumen Kasir (`FE-BKC-017`) sebelumnya punya tiga tab nyata (Kwitansi, Struk Pasien) dan
enam tab placeholder milik modul lain. Task ini menambah tab nyata **keempat**: "Invoice
Asuransi" — lembar yang siap diserahkan kasir/petugas billing ke perusahaan asuransi, berisi
identitas pasien, blok perusahaan asuransi (nama, nomor polis), dan tabel baris yang **benar-benar
ditanggung** asuransi beserta rupiahnya. Lembar ini **hanya membaca**; tidak ada aksi ubah data
dari layar ini.

Perubahan yang terlihat kasir:

1. Tab baru "Invoice Asuransi" muncul sejajar Kwitansi dan Struk Pasien, sebelum enam tab
   placeholder.
2. Membuka tab ini memuat data sekali (tidak dipanggil sebelum tab dibuka), lalu menampilkan
   salah satu dari empat keadaan: peringatan biru "tidak dapat diterbitkan" (pasien tunai/penjamin
   perusahaan/penjamin belum tercatat), ringkasan total tanpa rincian (tagihan lama sebelum
   pembaruan sistem), lembar lengkap siap cetak, atau pesan error merah (permintaan gagal).
3. Tombol "Cetak Invoice Asuransi" hanya muncul saat tab ini aktif **dan** lembarnya dinyatakan
   dapat dicetak oleh server — mengunduh PDF **A4** (bukan A5 seperti Kwitansi/Struk Pasien),
   karena tabelnya punya dua kolom tambahan ("Ditanggung Asuransi", "Porsi Pasien") yang terpotong
   di lebar A5.
4. Tautan langsung `?tab=INVOICE_ASURANSI` (atau nilai tab lain yang dikenali, termasuk enam tab
   placeholder) kini mendarat tepat di tab itu — sebelumnya seluruh nilai tab di luar kombinasi
   `KWITANSI`+`tenderId` diam-diam diarahkan ke Struk Pasien.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-018`, `FR-BKC-019`; `BKC-DEC-065`–`069`; `03-frontend-architecture.md` §
Amendment 3 September 2026 (kedua); `roadmap/frontend-roadmap.md` § `FE-BKC-018`. Kontrak backend
`BIL-API-0.5`/`0.6` — `GET .../invoices/{id}/insurance-invoice-document` (`BE-BKC-023`).

Satu keputusan tambahan ditutup **pada sesi ini**, di luar sepuluh acceptance criteria roadmap:
`BKC-DEC-092` (5 September 2026, Security Owner) — hak akses `BillingInvoice:Read` yang sudah ada
dipakai ulang apa adanya untuk membaca/mencetak lembar ini; **tidak** dibuat permission tersendiri.
Ini menutup `BKC-GATE-03`, satu-satunya sisa gerbang governance yang menahan task ini.

## Proses bisnis

| Aspek | Penjelasan |
| --- | --- |
| Pemicu | Kasir/petugas billing membuka tab "Invoice Asuransi" pada halaman Dokumen Kasir |
| Prasyarat | Invoice sudah dimuat (`documentReady`); kunjungan berpenjamin asuransi dengan sedikitnya satu baris tercover untuk lembar lengkap muncul |
| Langkah utama | Satu `GET .../insurance-invoice-document`; server menentukan `payerKind`, `isPrintable`, dan menyaring baris (`BKC-DEC-068`, hanya `CoveredAmount > 0`) — frontend murni menampilkan, tidak menyaring ulang |
| Aturan bisnis | Tombol cetak **hanya** muncul saat `isPrintable === true` (dikonfirmasi server: payer asuransi ditemukan **dan** ada sedikitnya satu baris tercover) |
| Perubahan status | Tidak ada — endpoint ini murni baca, task ini tidak menulis apa pun |
| Jalur tidak normal | Empat keadaan wajar (tunai/penjamin perusahaan/penjamin belum tercatat/tagihan lama) dijawab `200` dengan `warnings[]`, ditampilkan sebagai `InformationAlert variant="info"` (biru) — **bukan** galat merah |
| Hasil akhir | Lembar siap cetak (PDF A4), atau keterangan kenapa lembar tidak dapat diterbitkan |

**Empat kemungkinan tampilan tab** (turunan `payer`/`items.length` dari response, bukan cabang
terpisah per `payerKind`/`isPerItemBreakdownAvailable` — lebih sederhana dan otomatis benar untuk
kombinasi keduanya):

| Kondisi | Tampilan |
| --- | --- |
| `payer` kosong (tunai/penjamin perusahaan/penjamin belum tercatat/provider tidak ditemukan) | Hanya `warnings[]` (biru), tanpa lembar, tanpa tombol cetak |
| `payer` ada, `items.length === 0` (tagihan lama tanpa rincian, **atau** memang nol baris tercover) | `warnings[]` + ringkasan total (`dl.totalsSummary`) tanpa lembar, tanpa tombol cetak |
| `payer` ada, `items.length > 0` | Lembar lengkap (`InvoiceAsuransiDocument`, A4) + tombol cetak |
| Tagihan `OPEN` (`isFromLockedSnapshot === false`) | Catatan tambahan "Tagihan masih berjalan — angka dapat berubah sampai tagihan difinalkan." — **ditambahkan di layar**, tidak dikirim `warnings[]` backend |

## Base Component Decision Gate

`UI GATE: 4 elemen — REUSE 4, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Tombol "Cetak Invoice Asuransi" | `BaseButton` | Pola persis tombol "Cetak Struk Pasien" yang sudah ada di file yang sama | REUSE | Dipakai apa adanya |
| Alert loading/error/info (empat keadaan tab) | `InformationAlert` | Dipakai luas di file yang sama (Kwitansi, error invoice) | REUSE | Dipakai apa adanya — `variant="info"` untuk warning bisnis, `variant="danger"` khusus kegagalan permintaan |
| Tab navigasi | `react-bootstrap` `Nav variant="tabs"` (sudah ada) | Satu `Nav.Item`/`Nav.Link` baru ditambahkan ke `Nav` yang sudah ada, pola identik Kwitansi/Struk Pasien | REUSE | Tidak ada primitive baru |
| Dokumen printable | Pola `forwardRef` + `html2pdf.js` milik `KwitansiDocument`/`StrukPasienDocument` | Struktur inline-style + `forwardRef` disalin dari `struk-pasien-document.jsx` | REUSE (komponen baru, pola lama) | `invoice-asuransi-document.jsx` baru, lebar `210mm` (A4) menggantikan `148mm` (A5) — satu-satunya perbedaan struktural dari pola sumbernya |

Tidak ada base component baru. Ringkasan total tanpa rincian (`dl.totalsSummary`) memakai markup
`<dl>`/`<dt>`/`<dd>` polos dengan dua kelas CSS Module baru (`totalsSummary`, plus selector
`dt`/`dd` di dalamnya) — bukan base component, murni styling halaman ini sendiri (tidak ada
primitive "summary card" di katalog untuk dipakai ulang).

## Endpoint yang dikonsumsi

Satu endpoint baru dikonsumsi (sudah ada di backend sejak `BE-BKC-023`, `dotnet build`/`test`
dikonfirmasi lulus pengguna 5 September 2026):

| Method | Path | Dipakai lewat |
| --- | --- | --- |
| `GET` | `.../billing/invoices/{id}/insurance-invoice-document` | Thunk baru `getInsuranceInvoiceDocument` (`billing-invoice-slice.jsx`), dipanggil `use-dokumen-kasir-page.js` hanya saat `activeTab === "INVOICE_ASURANSI"` |

Hak akses: `BillingInvoice : Read` (dipakai ulang, `BKC-DEC-092` — lihat § "Keputusan yang
mengunci scope").

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/components/view/.../menu-pembayaran/invoice-asuransi-document.jsx` | **Baru** — komponen cetak A4, pola `forwardRef` disalin dari `struk-pasien-document.jsx` |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | Thunk baru `getInsuranceInvoiceDocument`; tiga slot state (`insuranceInvoiceDocument`/`Loading`/`Error`); reducer `clearInsuranceInvoiceDocument`; tiga selector baru |
| `src/lib/hooks/.../billing-invoices/use-dokumen-kasir-page.js` | Efek fetch baru (dipicu `activeTab`, dijaga per `invoiceId`); perbaikan efek inisialisasi tab (`RECOGNIZED_TABS`, menghormati seluruh nilai tab yang dikenali dari query string, bukan cuma `KWITANSI`+`tenderId`); `insuranceInvoiceDocumentProps` dan field-field baru pada nilai kembalian hook |
| `src/lib/hooks/.../billing-invoices/use-dokumen-kasir.js` | `buildPdf` menerima `paperSize` opsional (bawaan `"a5"`, TIDAK mengubah Kwitansi/Struk Pasien); `openDokumenKasir` menerima `initialTab` opsional; ref + `downloadInsuranceInvoice` baru (A4) |
| `src/lib/hooks/.../billing-invoices/billing-invoice-constants.js` | `PLACEHOLDER_TABS` (dulu lokal di view) dipindah ke sini sebagai `DOKUMEN_KASIR_PLACEHOLDER_TABS`, diekspor — satu sumber kebenaran dipakai baik oleh view (label Nav) maupun hook (pengenalan tab dari query string) |
| `src/components/view/.../menu-pembayaran/dokumen-kasir-view.jsx` | `Nav.Link` baru "Invoice Asuransi"; blok tampilan tab (empat keadaan, lihat § "Proses bisnis"); tombol cetak pada `heroActions`; impor `PLACEHOLDER_TABS` dari constants alih-alih didefinisikan lokal |
| `src/style/health-services/billing-management/dokumen-kasir.module.css` | Dua kelas baru: `.totalsSummary` (grid dua kolom) dan selector `dt`/`dd` di dalamnya — untuk keadaan "tagihan lama tanpa rincian" |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **PASS** | Exit 0, `eslint . --quiet` tanpa output — nol error pada seluruh repository (bukan hanya berkas task ini) |
| `npm run test:unit` | **PASS** | 440/440 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru ditulis pada task ini (murni wiring + satu komponen presentasi, konsisten pola `FE-BKC-017`/`FE-BKC-011`) |
| `npm run build` | **PASS** | `✓ Compiled successfully in 60s`, `Finished TypeScript` tanpa galat, 275/275 halaman statis di-generate, `postbuild`/`prepare-standalone` sukses. Route `/health-services/billing-management/billing/invoices/[slug]/pembayaran/dokumen-kasir` muncul di daftar route (dinamis, `ƒ`) tanpa error |
| Verifikasi statis: seluruh nilai casing response (camelCase/PascalCase) | **LULUS (tinjauan manual)** | Setiap field dari `InsuranceInvoiceDocumentResponse` dibaca dengan pola `field ?? field.PascalCase` yang konsisten dengan seluruh hook/view lain di file yang sama |
| Verifikasi statis: regresi Kwitansi/Struk Pasien (paper size, filename) | **LULUS (tinjauan manual)** | `buildPdf(element, prefix)` tanpa argumen ketiga tetap memakai bawaan `paperSize: "a5"`, identik dengan perilaku sebelum task ini; kedua pemanggil (`downloadKwitansi`, `downloadStruk`) **tidak diubah** |
| Verifikasi statis: tab lama (Kwitansi/Struk Pasien) tidak ikut tersentuh alur pengenalan tab baru | **LULUS (tinjauan manual)** | `wantsKwitansiForTender`/`openKwitansiForTender` (jalur Kwitansi+tenderId) **tidak diubah satu baris pun** — hanya cabang `else` (sebelumnya selalu Struk Pasien) yang diperluas |
| Grep konsistensi nama endpoint | **LULUS** | `insurance-invoice-document` pada thunk baru sama persis dengan route backend `BillingInvoicesController` (`GET {id:guid}/insurance-invoice-document`, `BE-BKC-023.md`) |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada task ini |
| Verifikasi manual ter-autentikasi (lima keadaan penjamin, cetak PDF A4, deep link `?tab=INVOICE_ASURANSI`) | **NOT FEASIBLE** | Tidak ada kredensial login yang tersedia untuk builder; membutuhkan data invoice+penjamin asuransi nyata |

**Task ini belum bisa ditandai selesai sepenuhnya.** Lint/unit test/build lulus bersih, tetapi
`lint:errors` yang lulus hanya membuktikan tidak ada error sintaks/aturan statis — **bukan** bukti
perilaku benar. Verifikasi manual ter-autentikasi terhadap kelima acceptance criteria yang
membutuhkan data penjamin nyata (2–6, 9, 10) masih menunggu.

## Risiko yang tersisa

1. Perbaikan pengenalan tab (`RECOGNIZED_TABS`) mengubah cabang `else` pada efek inisialisasi yang
   sama dipakai jalur Kwitansi tanpa `tenderId` (`?tab=KWITANSI` tanpa `tenderId` kini eksplisit
   diakui `RECOGNIZED_TABS`, mendarat di tab Kwitansi dengan `activeTender=null` — menampilkan
   pesan "pilih pembayaran" yang sudah ada). Perilaku ini **diniatkan** konsisten dengan "setiap
   nilai tab yang dikenali dihormati", tetapi belum diverifikasi hidup di browser — lint/test/build
   tidak dapat membuktikan perilaku runtime interaktif ini.
2. `insuranceIsFromLockedSnapshot`/catatan "tagihan masih berjalan" murni derivasi frontend
   (backend tidak mengirim warning ini) — bila kelak backend menambahkan warning yang sama,
   pesan akan tampil dobel. Dicatat sebagai risiko integrasi masa depan, bukan bug saat ini.
3. `lint:errors`/`test:unit`/`build` yang lulus membuktikan tidak ada error sintaks, error tipe,
   maupun regresi pada 440 test existing — **tidak** membuktikan kelima keadaan penjamin (tunai,
   penjamin perusahaan, penjamin belum tercatat, tagihan lama, tagihan berjalan) benar-benar
   tampil sesuai desain, PDF A4 benar-benar tidak terpotong, atau tombol cetak benar-benar hanya
   muncul saat `isPrintable`. Ketiganya butuh verifikasi manual ter-autentikasi dengan data nyata.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran yang punya akses Menu Pembayaran, buka invoice pasien asuransi dengan
   sedikitnya satu baris tercover, verifikasi: tab baru muncul di posisi yang benar, lembar
   tampil lengkap dengan kolom "Ditanggung Asuransi"/"Porsi Pasien" terisi benar, tombol cetak
   menghasilkan PDF A4 yang tidak terpotong, dan Kwitansi/Struk Pasien tetap A5 tanpa regresi.
2. Uji kelima keadaan penjamin (tunai, penjamin perusahaan, penjamin belum tercatat, tagihan lama
   tanpa rincian, tagihan `OPEN` yang masih berjalan) satu per satu, dan uji deep link
   `?tab=INVOICE_ASURANSI` beserta beberapa tab placeholder untuk memastikan perbaikan
   pengenalan tab benar-benar bekerja.
3. Setelah verifikasi manual selesai dan tidak ada temuan, task ini dapat ditandai selesai penuh
   — lint/unit test/build sudah lulus (§ "Definition of Done"), backend yang dikonsumsi
   (`BE-BKC-023`) sudah terverifikasi lulus pengguna, dan `BKC-GATE-03` sudah tertutup.
