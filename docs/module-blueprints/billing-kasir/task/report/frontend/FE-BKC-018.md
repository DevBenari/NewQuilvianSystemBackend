# FE-BKC-018 — Tab "Invoice Asuransi" pada Halaman Dokumen Kasir

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-018` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, vertical slice baru (satu tab + satu pemanggilan data + satu komponen cetak) |
| Task mode | `FRONTEND` (backend read-only, tidak ada perubahan kontrak API — endpoint sudah ada dari `BE-BKC-023`) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `QuilvianIntegrationFrontend` — bukan `yasmina` seperti versi laporan sebelumnya, lihat § 0) |
| Dependency | `BE-BKC-023` — dikonfirmasi ADA lewat pembacaan langsung `BillingInvoicesController.cs` baris 303 dan `BillingInsuranceInvoiceDtos.cs` (nama file DTO sebenarnya, bukan `BillingInvoiceDtos.cs` seperti dugaan awal) pada sesi ini |
| Status task | **Dikerjakan ulang dari nol pada sesi ini** (lihat § 0). Source selesai. `eslint . --quiet` (repo penuh) **PASS 0 error**; `npm run lint:errors` **PASS**; `test:unit` **PASS 440/440** (tanpa regresi). `npm run build` **TIDAK dijalankan** — pengguna eksplisit meminta menjalankannya sendiri di giliran ini ("tidak usah di run and build, nnt saya lakukan sendiri"). Belum di-commit. Belum diverifikasi manual ter-autentikasi |

## 0. Kenapa laporan ini ditulis ulang (bukan task baru)

Versi laporan ini **sebelumnya** mengklaim task selesai (8 berkas, lint/test/build lulus). Amendment
roadmap 7 September 2026 (`frontend-roadmap.md`, ditulis SEBELUM giliran ini) sudah membuktikan lewat
`Glob`/`grep`/`git log`/`git reflog`/`git stash` bahwa **tidak satu pun** berkas yang diklaim laporan
lama benar-benar ada di repository manapun yang bisa diperiksa — status task diturunkan jadi "belum
dikerjakan", `READY_FOR_TASK_APPROVAL`. Sesi ini (giliran saat ini) memverifikasi ulang temuan yang
sama secara independen sebelum mulai (`Glob **/invoice-asuransi-document.jsx` = nol hasil, `grep
getInsuranceInvoiceDocument|INVOICE_ASURANSI` di seluruh `src/` = nol hasil), lalu benar-benar
mengerjakan task ini dari nol — bukan memverifikasi laporan lama, bukan melanjutkan pekerjaan yang
ternyata tidak pernah ada.

**Catatan lingkungan yang ikut ditemukan**: checkout frontend saat ini ada di branch
`QuilvianIntegrationFrontend` (bukan `yasmina` yang disebut riwayat versi laporan sebelumnya) — git
status bersih (tidak ada perubahan tak ter-commit dari sesi lain saat giliran ini dimulai, di luar
perubahan task-task lain pada sesi percakapan yang sama yang sudah ikut terbawa). Dependency backend
`BE-BKC-023` dikonfirmasi ADA pada checkout backend `NewQuilvianSystemBackend` branch `Yasmina`.

## Ringkasan untuk pembaca umum

Halaman Dokumen Kasir (`FE-BKC-017`) sebelumnya punya dua tab nyata (Kwitansi, Struk Pasien) dan
enam tab placeholder milik modul lain. Task ini menambah tab nyata **ketiga**: "Invoice
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
   pembaruan sistem, atau memang nol baris tercover), lembar lengkap siap cetak, atau pesan error
   merah (permintaan gagal).
3. Tombol "Cetak Invoice Asuransi" hanya muncul saat tab ini aktif **dan** lembarnya dinyatakan
   dapat dicetak oleh server (`isPrintable`) — mengunduh PDF **A4** (bukan A5 seperti
   Kwitansi/Struk Pasien), karena tabelnya punya dua kolom tambahan ("Ditanggung Asuransi",
   "Porsi Pasien") yang terpotong di lebar A5.
4. Tautan langsung `?tab=INVOICE_ASURANSI` (atau nilai tab lain yang dikenali, termasuk enam tab
   placeholder) kini mendarat tepat di tab itu — sebelumnya seluruh nilai tab di luar kombinasi
   `KWITANSI`+`tenderId` diam-diam diarahkan ke Struk Pasien.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-018`, `FR-BKC-019`; `BKC-DEC-065`–`069`; `03-frontend-architecture.md` §
Amendment 3 September 2026 (kedua); `roadmap/frontend-roadmap.md` § `FE-BKC-018`. Kontrak backend
`BIL-API-0.5`/`0.6` — `GET .../invoices/{id}/insurance-invoice-document` (`BE-BKC-023`).

Ringkasan lima keputusan yang membentuk isi lembar (semua `approved`, `00-interview-decisions.md`):
`BKC-DEC-065` (dokumen milik billing-kasir sendiri, bukan Claim Letter modul lain — pola presentasi
sama dengan Kwitansi: HTML + `html2pdf.js`); `BKC-DEC-066` (untuk tiga pihak — pasien, RS, asuransi
— harus cukup meyakinkan, bukan sekadar badge); `BKC-DEC-067` (sumber "informasi perusahaan" adalah
`MstInsuranceProvider`, BUKAN `MstCompanyGuarantor` — Company Guarantor eksplisit di luar scope);
`BKC-DEC-068` (HANYA baris tercover asuransi yang tampil, disaring SERVER, bukan layar); `BKC-DEC-069`
(setiap baris WAJIB menampilkan rupiah tercover per item, bukan hanya badge — sudah diekspos
`BE-BKC-023`, bukan kalkulasi ulang frontend).

## Proses bisnis

| Aspek | Penjelasan |
| --- | --- |
| Pemicu | Kasir/petugas billing membuka tab "Invoice Asuransi" pada halaman Dokumen Kasir |
| Prasyarat | Invoice sudah dimuat (`documentReady`); kunjungan berpenjamin asuransi dengan sedikitnya satu baris tercover untuk lembar lengkap muncul |
| Langkah utama | Satu `GET .../insurance-invoice-document`; server menentukan `payerKind`, `isPrintable`, dan menyaring baris (`BKC-DEC-068`, hanya `CoveredAmount > 0`) — frontend murni menampilkan, tidak menyaring ulang |
| Aturan bisnis | Tombol cetak **hanya** muncul saat `isPrintable === true` (dibaca langsung dari respons, tidak diturunkan ulang dari `payer`/`items` di layar) |
| Perubahan status | Tidak ada — endpoint ini murni baca, task ini tidak menulis apa pun |
| Jalur tidak normal | Empat keadaan wajar (tunai/penjamin perusahaan/penjamin belum tercatat/tagihan lama) dijawab `200` dengan `warnings[]`, ditampilkan sebagai `InformationAlert variant="info"` (biru) — **bukan** galat merah |
| Hasil akhir | Lembar siap cetak (PDF A4), atau keterangan kenapa lembar tidak dapat diterbitkan |

**Empat kemungkinan tampilan tab** (turunan `payer`/`items.length` dari response):

| Kondisi | Tampilan |
| --- | --- |
| `payer` kosong (tunai/penjamin perusahaan/penjamin belum tercatat/provider tidak ditemukan) | Hanya `warnings[]` (biru), tanpa lembar, tanpa tombol cetak |
| `payer` ada, `items.length === 0` (tagihan lama tanpa rincian, **atau** memang nol baris tercover) | `warnings[]` + ringkasan total (`dl.totalsSummary`) tanpa lembar, tanpa tombol cetak |
| `payer` ada, `items.length > 0` | Lembar lengkap (`InvoiceAsuransiDocument`, A4) + tombol cetak (bila `isPrintable`) |
| `isFromLockedSnapshot === false` (tagihan `OPEN`, belum final) | Catatan tambahan "Tagihan masih berjalan — angka dapat berubah sampai tagihan difinalkan." — **ditambahkan di layar** dari field yang SUDAH ada di respons yang sama, bukan derivasi status invoice terpisah |

## Base Component Decision Gate

`UI GATE: 4 elemen — REUSE 4, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Tombol "Cetak Invoice Asuransi" | `BaseButton` | Pola persis tombol "Cetak Struk Pasien" pada file yang sama | REUSE | Dipakai apa adanya |
| Alert loading/error/info (empat keadaan tab) | `InformationAlert` | Dipakai luas di file yang sama (Kwitansi, error invoice) | REUSE | Dipakai apa adanya — `variant="info"` untuk warning bisnis, `variant="danger"` khusus kegagalan permintaan |
| Tab navigasi | `react-bootstrap` `Nav variant="tabs"` (sudah ada) | Satu `Nav.Item`/`Nav.Link` baru ditambahkan ke `Nav` yang sudah ada, pola identik Kwitansi/Struk Pasien | REUSE | Tidak ada primitive baru |
| Dokumen printable | Pola `forwardRef` + `html2pdf.js` milik `KwitansiDocument`/`StrukPasienDocument` | Struktur inline-style + `forwardRef` disalin dari `struk-pasien-document.jsx` | REUSE (komponen baru, pola lama) | `invoice-asuransi-document.jsx` baru, lebar `210mm` (A4) menggantikan `148mm` (A5) — satu-satunya perbedaan struktural dari pola sumbernya |

Ringkasan total tanpa rincian (`dl.totalsSummary`) memakai markup `<dl>`/`<dt>`/`<dd>` polos dengan
dua kelas CSS Module baru (`totalsSummary`, plus selector `dt`/`dd` di dalamnya) — bukan base
component, murni styling halaman ini sendiri, memakai token desain yang SUDAH dipakai file CSS yang
sama (`--space-2`/`--space-4`, `--color-border`, `--color-surface-soft`, `--color-text-muted`,
`--radius-lg`) — dikonfirmasi ada di codebase lewat grep sebelum dipakai, tidak ada nilai visual
literal baru.

## Endpoint yang dikonsumsi

Satu endpoint dikonsumsi (sudah ada di backend, `BE-BKC-023`, dikonfirmasi ada lewat pembacaan
langsung `BillingInvoicesController.cs` baris 303 dan `BillingInsuranceInvoiceDtos.cs` pada sesi ini):

| Method | Path | Dipakai lewat |
| --- | --- | --- |
| `GET` | `.../billing/invoices/{id}/insurance-invoice-document` | Thunk baru `getInsuranceInvoiceDocument` (`billing-invoice-slice.jsx`), dipanggil `use-dokumen-kasir-page.js` hanya saat `activeTab === "INVOICE_ASURANSI"` |

Hak akses: `BillingInvoice:Read` (dipakai ulang — `BKC-DEC-092`, tidak ada permission baru).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/components/view/.../menu-pembayaran/invoice-asuransi-document.jsx` | **Baru** — komponen cetak A4, pola `forwardRef` disalin dari `struk-pasien-document.jsx`; blok payer (`MstInsuranceProvider`, `BKC-DEC-067`) dan kolom "Ditanggung Asuransi"/"Porsi Pasien" per baris (`BKC-DEC-069`) |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | Thunk baru `getInsuranceInvoiceDocument`; tiga slot state (`insuranceInvoiceDocument`/`Loading`/`Error`); reducer `clearInsuranceInvoiceDocument`; tiga selector baru |
| `src/lib/hooks/.../billing-invoices/use-dokumen-kasir-page.js` | Efek fetch baru (dipicu `activeTab === "INVOICE_ASURANSI"`, dijaga per `invoiceId` lewat ref supaya tidak fetch berulang); perbaikan efek inisialisasi tab (`DOKUMEN_KASIR_RECOGNIZED_TABS`, menghormati seluruh nilai tab yang dikenali dari query string, bukan cuma `KWITANSI`+`tenderId`); `insuranceInvoiceDocumentProps` dan field-field baru pada nilai kembalian hook |
| `src/lib/hooks/.../billing-invoices/use-dokumen-kasir.js` | `buildPdf` menerima `paperSize` opsional (bawaan `"a5"`, TIDAK mengubah Kwitansi/Struk Pasien — kedua pemanggil lama tidak dikirim argumen ketiga); `openDokumenKasir` menerima `initialTab` opsional (bawaan tetap `"STRUK_PASIEN"`); ref `insuranceInvoicePrintRef` + `downloadInsuranceInvoice` baru (A4) |
| `src/lib/hooks/.../billing-invoices/billing-invoice-constants.js` | `DOKUMEN_KASIR_PLACEHOLDER_TABS` (dulu lokal di view sebagai `PLACEHOLDER_TABS`) dipindah ke sini, diekspor; `DOKUMEN_KASIR_REAL_TABS` dan `DOKUMEN_KASIR_RECOGNIZED_TABS` (Set) baru — satu sumber kebenaran dipakai baik oleh view (label Nav) maupun hook (pengenalan tab dari query string) |
| `src/components/view/.../menu-pembayaran/dokumen-kasir-view.jsx` | `Nav.Link` baru "Invoice Asuransi"; blok tampilan tab (empat keadaan, lihat § "Proses bisnis"); tombol cetak pada `heroActions` (gated `isPrintable`); impor `DOKUMEN_KASIR_PLACEHOLDER_TABS` dari constants alih-alih didefinisikan lokal; `formatMoney` lokal baru untuk ringkasan total |
| `src/style/health-services/billing-management/dokumen-kasir.module.css` | Dua kelas baru: `.totalsSummary` (grid dua kolom) dan selector `dt`/`dd` di dalamnya — untuk keadaan "tagihan lama tanpa rincian"; seluruh nilai memakai token desain yang sudah dipakai file ini |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `eslint . --quiet` (repo penuh) | **PASS** | Exit 0, tanpa output — nol error pada seluruh repository |
| `npm run lint:errors` | **PASS** | Exit 0, tanpa output |
| `npm run test:unit` | **PASS** | 440/440 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru ditulis pada task ini (murni wiring + satu komponen presentasi, konsisten pola `FE-BKC-017`/`FE-BKC-011`) |
| `npm run build` | **TIDAK DIJALANKAN** | Pengguna eksplisit meminta menjalankannya sendiri pada giliran ini ("tidak usah di run and build, nnt saya lakukan sendiri") |
| Verifikasi statis: seluruh nilai casing response (camelCase/PascalCase) | **LULUS (tinjauan manual)** | Setiap field dari `InsuranceInvoiceDocumentResponse`/`InsuranceInvoicePayerResponse`/`InsuranceInvoiceItemResponse`/`InsuranceInvoiceTotalResponse` (dibaca langsung dari `BillingInsuranceInvoiceDtos.cs` backend, BUKAN ditebak dari laporan lama) dibaca dengan pola `field ?? field.PascalCase` yang konsisten dengan seluruh hook/view lain di file yang sama |
| Verifikasi statis: regresi Kwitansi/Struk Pasien (paper size, filename) | **LULUS (tinjauan manual)** | `buildPdf(element, prefix)` tanpa argumen ketiga tetap memakai bawaan `paperSize: "a5"`, identik dengan perilaku sebelum task ini; kedua pemanggil (`downloadKwitansi`, `downloadStruk`) **tidak diubah** |
| Verifikasi statis: tab lama (Kwitansi/Struk Pasien) tidak ikut tersentuh alur pengenalan tab baru | **LULUS (tinjauan manual)** | `wantsKwitansiForTender`/`openKwitansiForTender` (jalur Kwitansi+tenderId) **tidak diubah satu baris pun** — hanya cabang `else` (sebelumnya selalu Struk Pasien) yang diperluas |
| Grep konsistensi nama endpoint | **LULUS** | `insurance-invoice-document` pada thunk baru sama persis dengan route backend `BillingInvoicesController.cs` baris 303 |
| Nama berkas PDF memakai nomor invoice, bukan nama pasien | **LULUS (tinjauan manual)** | `buildPdf` membentuk filename dari `invoice?.invoiceNumber` — pola yang SUDAH ada, tidak diubah; `InvoiceAsuransiDocument` sendiri tidak membentuk nama berkas apa pun |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada task ini |
| Verifikasi manual ter-autentikasi (lima keadaan penjamin, cetak PDF A4, deep link `?tab=INVOICE_ASURANSI`) | **NOT FEASIBLE** | Tidak ada kredensial login yang tersedia untuk builder; membutuhkan data invoice+penjamin asuransi nyata |

**Task ini belum bisa ditandai selesai sepenuhnya.** Lint/unit test lulus bersih dan `build` sengaja
tidak dijalankan (pengguna akan menjalankannya sendiri), tetapi ini **bukan** bukti perilaku benar.
Verifikasi manual ter-autentikasi terhadap kelima acceptance criteria yang membutuhkan data penjamin
nyata (2–6, 9, 10) masih menunggu — sama seperti versi laporan sebelumnya, TAPI kali ini source-nya
benar-benar ada di disk (dikonfirmasi lint/test berjalan terhadap berkas-berkas itu).

## Risiko yang tersisa

1. Perbaikan pengenalan tab (`DOKUMEN_KASIR_RECOGNIZED_TABS`) mengubah cabang `else` pada efek
   inisialisasi yang sama dipakai jalur Kwitansi tanpa `tenderId` (`?tab=KWITANSI` tanpa `tenderId`
   kini eksplisit diakui, mendarat di tab Kwitansi dengan `activeTender=null` — menampilkan pesan
   "pilih pembayaran" yang sudah ada). Perilaku ini **diniatkan** konsisten dengan "setiap nilai tab
   yang dikenali dihormati", tetapi belum diverifikasi hidup di browser.
2. Catatan "tagihan masih berjalan" kini dibaca LANGSUNG dari `isFromLockedSnapshot` pada respons
   yang sama (bukan derivasi status invoice terpisah seperti versi laporan lama) — lebih sederhana
   dan tidak berisiko duplikasi warning, karena field itu sudah eksplisit dimaksudkan backend untuk
   tujuan ini (komentar DTO: "true bila angkanya dibaca dari BilCalculationVersion tersimpan").
3. Lint/test lulus membuktikan tidak ada error sintaks/tipe maupun regresi pada 440 test existing —
   **tidak** membuktikan kelima keadaan penjamin (tunai, penjamin perusahaan, penjamin belum
   tercatat, tagihan lama, tagihan berjalan) benar-benar tampil sesuai desain, PDF A4 benar-benar
   tidak terpotong, atau tombol cetak benar-benar hanya muncul saat `isPrintable`. Ketiganya butuh
   verifikasi manual ter-autentikasi dengan data nyata.

## Langkah berikutnya yang direkomendasikan

1. Jalankan `npm run build` (pengguna, sesuai permintaan eksplisit pada giliran ini).
2. Login dengan peran yang punya akses Menu Pembayaran, buka invoice pasien asuransi dengan
   sedikitnya satu baris tercover, verifikasi: tab baru muncul di posisi yang benar, lembar
   tampil lengkap dengan kolom "Ditanggung Asuransi"/"Porsi Pasien" terisi benar, tombol cetak
   menghasilkan PDF A4 yang tidak terpotong, dan Kwitansi/Struk Pasien tetap A5 tanpa regresi.
3. Uji kelima keadaan penjamin (tunai, penjamin perusahaan, penjamin belum tercatat, tagihan lama
   tanpa rincian, tagihan `OPEN` yang masih berjalan) satu per satu, dan uji deep link
   `?tab=INVOICE_ASURANSI` beserta beberapa tab placeholder untuk memastikan perbaikan
   pengenalan tab benar-benar bekerja.
4. Setelah verifikasi manual selesai dan tidak ada temuan, task ini dapat ditandai selesai penuh.
