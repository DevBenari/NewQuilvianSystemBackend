# FE-BKC-017 — Dokumen Kasir: Modal Menjadi Halaman Terpisah

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-017` (baru di luar roadmap revisi 1 asli — lahir dari permintaan pengguna langsung 3 September 2026, dikunci lewat `/grill-me` amendment `BKC-DEC-063`–`064`) |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.5`) |
| Task type | Frontend, refactor presentasi murni (modal → halaman) |
| Task mode | `FRONTEND` (backend read-only, tidak ada perubahan kontrak API) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Status task | Source selesai, lint dan unit test lulus bersih. Belum di-commit. `npm run build` BLOCKED/INCONCLUSIVE (lingkungan). Belum diverifikasi manual ter-autentikasi |

## Ringkasan untuk pembaca umum

Sebelumnya, tombol "Dokumen Kasir" pada Menu Pembayaran (baik tombol umum di kartu Ringkasan
Pembayaran, maupun tombol "Cetak Kwitansi" per baris pembayaran di tabel Split Tender) membuka
sebuah **modal/popup** (`dokumen-kasir-modal.jsx`) berisi tab Kwitansi, Struk Pasien, dan enam tab
placeholder dokumen milik modul lain. Pengguna meminta modal itu diganti menjadi **halaman
tersendiri** dengan isi identik, ditambah tombol Cetak dan tombol Kembali ke Menu Pembayaran.

Task ini murni memindahkan wadah tampilan — tidak ada perubahan pada data yang ditampilkan,
aturan bisnis Kwitansi/Struk Pasien, mekanisme pembuatan PDF (`html2pdf.js`), maupun fitur share
WhatsApp/Email. Semua itu tetap seperti keputusan `BKC-DEC-052`–`058` yang sudah terkunci
sebelumnya (lihat `task/report/frontend/fe-bkc-011-dokumen-kasir-kwitansi-per-tender-dan-struk-pasien.md`).

Perubahan yang terlihat kasir:

1. Menekan tombol "Dokumen Kasir" atau "Cetak Kwitansi" tidak lagi membuka jendela pop-up di atas
   Menu Pembayaran — browser berpindah ke halaman baru "Dokumen Kasir" dengan tab, dokumen, dan
   tombol aksi yang sama persis.
2. Halaman baru punya tombol "Kembali" yang selalu membawa kasir kembali ke Menu Pembayaran
   invoice yang sama.
3. Tombol Cetak (unduh PDF), WhatsApp, dan Email pada tab Kwitansi tetap bekerja sama seperti
   sebelumnya.

## Keputusan yang mengunci scope (ringkas)

Dikunci lewat `/grill-me` amendment 3 September 2026 (lihat
`00-interview-decisions.md` § "Amendment lanjutan 3 September 2026"), disetujui langsung
pengguna dalam percakapan:

- `BKC-DEC-063`: mekanisme cetak/share TETAP `html2pdf.js` + WhatsApp/Email (bukan diganti
  `window.print()`), supaya kemampuan share yang sudah dipakai kasir tidak hilang.
- `BKC-DEC-064`: SATU route dipakai oleh kedua titik pemicu, tab aktif dan tender terpilih
  dikirim lewat query string (`?tab=&tenderId=`), bukan dua route terpisah.

Detail arsitektur lengkap ada di `03-frontend-architecture.md` § "Amendment 3 September 2026 —
Dokumen Kasir: modal menjadi halaman terpisah", dan entri roadmap di
`roadmap/frontend-roadmap.md` § `FE-BKC-017`.

## Proses bisnis

Proses bisnis Kwitansi (per tender) dan Struk Pasien (level invoice) TIDAK berubah dari yang
sudah didokumentasikan di `task/report/frontend/fe-bkc-011-dokumen-kasir-kwitansi-per-tender-dan-struk-pasien.md`
§ "Proses bisnis" — dokumen ini hanya mendokumentasikan perubahan titik masuk (trigger) dan
wadah tampilannya.

| Aspek | Sebelum (`FE-BKC-011`) | Sesudah (`FE-BKC-017`) |
| --- | --- | --- |
| Pemicu tombol umum "Dokumen Kasir" | `dokumenKasir.openDokumenKasir()` → modal terbuka, tab Struk Pasien | Navigasi (`Link`) ke `/health-services/billing-management/billing/invoices/{token}/pembayaran/dokumen-kasir?tab=STRUK_PASIEN` |
| Pemicu "Cetak Kwitansi" per baris tender | `dokumenKasir.openKwitansiForTender(tender)` → modal terbuka, tab Kwitansi untuk tender itu | `router.push(...)` ke rute yang sama dengan `?tab=KWITANSI&tenderId={tender.id}` |
| Sumber data halaman | State React `useMenuPembayaran` (invoice, settlement, dsb. sudah dimuat di Menu Pembayaran) | Halaman route terpisah — invoice dan tender **dimuat ulang** lewat `useBillingInvoiceDetail`+`useBillingSettlement`, tender aktif dicari dari `tenderId` query string |
| Tombol "Kembali" | Tombol Kembali di dalam modal → `onHide` (menutup modal, tetap di Menu Pembayaran) | Tombol Kembali → `Link` ke `BILLING_INVOICE_ROUTES.pembayaran(token)` (Menu Pembayaran invoice yang sama) |
| Deep link langsung (buka URL Dokumen Kasir tanpa lewat Menu Pembayaran) | Tidak mungkin (modal, bukan route) | Mungkin — halaman memuat invoice dari `[slug]`; bila `tenderId` di URL tidak ditemukan pada tender invoice itu, tampil pesan "Pembayaran (tender) pada tautan ini tidak ditemukan..." (bukan blank/crash) |

## Base Component Decision Gate

`UI GATE: 5 elemen — REUSE 4, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi/keputusan |
| --- | --- | --- | --- | --- |
| Tombol "Kembali", "Cetak Kwitansi Pasien"/"Cetak Struk Pasien"/WhatsApp/Email | `BaseButton` (dengan `as={Link}` untuk navigasi, pola sama `InpatientConsentPrintView`) | `src/components/features/base-features/base-button`; pola `as={Link}` dipakai `inpatient-consent-print-view.jsx` | REUSE | Dipakai apa adanya, tidak ada varian baru |
| Header halaman (eyebrow/title/description) | `Hero` | Dipakai luas di seluruh module, termasuk `menu-pembayaran-view.jsx` dan `InpatientConsentPrintView` | REUSE | Dipakai tanpa `isDetailHero`/`detailActions` (halaman ini tidak butuh badge status invoice di header seperti Menu Pembayaran) |
| Alert loading/error/info | `InformationAlert` | Dipakai di `dokumen-kasir-modal.jsx` (lama) dan `InpatientConsentPrintView` | REUSE | Dipakai apa adanya |
| Gate akses ditolak | `AccessDeniedGate` | Dipakai `InpatientConsentPrintView`, `menu-pembayaran-view.jsx` | REUSE | Dipakai apa adanya, `error={errorMessage}` dari `useBillingInvoiceDetail` |
| Tab navigasi dokumen (Kwitansi/Struk Pasien/6 placeholder) | `react-bootstrap` `Nav variant="tabs"` | Keputusan sudah tervalidasi sesi sebelumnya untuk `dokumen-kasir-modal.jsx` (`BKC-DEC-053` context) — tidak ada primitive Tabs generik di katalog base component | WRAP | Markup tab dipindah apa adanya dari modal ke halaman, tidak ada pola baru |
| Dokumen printable Kwitansi/Struk Pasien | `KwitansiDocument`/`StrukPasienDocument` (`forwardRef`, `html2pdf.js`) | Sudah ada, dipakai `dokumen-kasir-modal.jsx` sebelumnya | REUSE | Dipakai ulang tanpa perubahan kontrak prop |

## Endpoint yang dikonsumsi

Tidak ada endpoint baru. Halaman baru mengonsumsi endpoint yang **sudah** dikonsumsi
`menu-pembayaran-view.jsx` (via hook yang sama, dipanggil ulang secara independen karena route
terpisah):

| Method | Path | Dipakai lewat |
| --- | --- | --- |
| `GET` | `.../billing/invoices/{id}` | `useBillingInvoiceDetail` (invoice + patient) |
| `GET` | `.../billing/settlements/by-invoice/{invoiceId}` (atau setara — thunk existing `getBillingSettlementsByInvoice`) | `useBillingSettlement` (tenders, untuk mencari `tenderId` dari query string) |

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/app/.../[slug]/pembayaran/dokumen-kasir/page.jsx` | **Baru** — server component tipis, pola persis `.../pembayaran/page.jsx` |
| `src/components/view/.../menu-pembayaran/dokumen-kasir-view.jsx` | **Baru** — client view halaman Dokumen Kasir |
| `src/lib/hooks/.../billing-invoices/use-dokumen-kasir-page.js` | **Baru** — hook composisi (`useBillingInvoiceDetail`+`useBillingSettlement`+`useDokumenKasir`) khusus halaman baru |
| `src/style/health-services/billing-management/dokumen-kasir.module.css` | **Baru** — CSS Module panggung dokumen/action bar, pola sama `inpatient-consent-print.module.css` |
| `src/lib/hooks/.../billing-invoices/billing-invoice-constants.js` | Tambah route builder `dokumenKasir(token, { tab, tenderId })` pada `BILLING_INVOICE_ROUTES` |
| `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` | Hapus import/render `DokumenKasirModal` dan state turunannya (`activeTender`, `kwitansiDocumentProps`, `strukDocumentProps`, `kwitansiShareMessage`); dua titik pemicu diganti navigasi (`Link`/`router.push`) |
| `src/lib/hooks/.../billing-invoices/use-menu-pembayaran.js` | Hapus composition `useDokumenKasir` (tidak dipakai lagi setelah trigger jadi navigasi) |
| `src/components/view/.../menu-pembayaran/dokumen-kasir-modal.jsx` | **Dihapus** — tidak ada konsumen lain (dikonfirmasi lewat pencarian referensi di seluruh source frontend sebelum dihapus) |

`use-dokumen-kasir.js` (hook PDF/share `html2pdf.js`) **tidak diubah** — dipakai ulang apa adanya
oleh `use-dokumen-kasir-page.js`.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `eslint . --quiet` (scope diubah/baru, 6 file) | **PASS** | Exit 0, tanpa output |
| `eslint` (full severity, tanpa `--quiet`, 6 file) | **PASS dengan warning existing-pattern** | 0 error, 17 warning `react-hooks/refs` ("Cannot access refs during render") — pola ini SUDAH ADA di baseline repo sebelum task ini (44 total occurrence repo-wide sebelum dikurangi 17 dari file baru task ini = 27 pre-existing di file lain yang tidak disentuh task ini), berasal dari hook (`use-dokumen-kasir.js`, tidak diubah) yang mengembalikan ref bersama field lain dalam satu object — bukan pola baru yang diperkenalkan task ini, tidak diperbaiki sesuai batas cakupan task |
| `npm run test:unit` | **PASS** | 434/434 test lulus, 0 gagal — tidak ada regresi. Tidak ada file test baru ditulis (task presentational murni + wiring, konsisten pola `FE-BKC-011`/`FE-BKC-016` yang juga tanpa test baru) |
| `npm run build` | **BLOCKED/INCONCLUSIVE** | `Error: EBUSY: resource busy or locked, rmdir '.next/standalone'` — folder terkunci oleh proses `node .next/standalone/server.js` (PID 18512, dari `npm start` PID 9608) yang sedang berjalan di lingkungan builder. Pengguna diberi pilihan lewat `AskUserQuestion` untuk menghentikan proses itu; pengguna memilih **TIDAK** menghentikannya. Ini kegagalan lingkungan, bukan kegagalan kode task ini — route/import baru sudah lulus lint penuh dan tidak ada error compile yang terlihat sebelum langkah `rmdir` gagal |
| Grep anti-regresi (referensi mati) | **PASS** | `grep -r "dokumen-kasir-modal\|DokumenKasirModal"` di seluruh `src/` hanya menyisakan komentar referensi historis di file baru (`dokumen-kasir-view.jsx`), tidak ada import/JSX yang menunjuk file yang sudah dihapus |
| Verifikasi manual (browser, tanpa login) | **NOT DONE** | Tidak dijalankan pada task ini — halaman ini butuh data invoice+tender nyata (sama seperti keterbatasan `FE-BKC-011`), smoke test tanpa login tidak banyak membuktikan |
| Verifikasi manual ter-autentikasi (klik dari kedua titik pemicu, cetak PDF, tombol Kembali, deep link dengan `tenderId` valid/invalid) | **NOT DONE** | Tidak ada kredensial login yang tersedia untuk builder |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint dan unit test lulus bersih, tetapi
`npm run build` belum terverifikasi (BLOCKED lingkungan) dan klik-coba langsung (kedua titik
pemicu, cetak PDF, deep link) belum dijalankan.

## Risiko yang tersisa

1. `npm run build` belum pernah dijalankan sampai selesai untuk perubahan ini — walau lint penuh
   (termasuk type-aware JSX resolution ESLint) lulus tanpa error, `next build` melakukan
   pemeriksaan tambahan (mis. resolusi route App Router, `generateStaticParams`, dsb.) yang belum
   terbukti lulus untuk route baru `dokumen-kasir/page.jsx`.
2. Perilaku inisialisasi tab di `use-dokumen-kasir-page.js` bergantung pada urutan efek: tab
   Kwitansi dengan `tenderId` menunggu `settlement.loading` selesai sebelum memutuskan tender
   ditemukan/tidak. Belum diverifikasi hidup apakah ada race condition nyata (mis. `tenderId`
   valid tapi baru muncul di response setelah render pertama).
3. Sama seperti risiko `FE-BKC-011` yang belum tertutup: `html2pdf.js` belum pernah diverifikasi
   menghasilkan PDF valid di browser nyata pada sesi manapun — risiko ini tidak bertambah maupun
   berkurang oleh task ini (mekanismenya tidak diubah), hanya dipindahkan wadahnya.

## Langkah berikutnya yang direkomendasikan

1. Jalankan `npm run build` setelah instance app yang sedang berjalan (`.next/standalone`)
   dihentikan atau di lingkungan lain yang tidak menguncinya.
2. Login dengan peran yang punya akses Menu Pembayaran, buka satu invoice dengan minimal satu
   tender, lalu verifikasi: tombol "Dokumen Kasir" umum membuka halaman baru di tab Struk
   Pasien, tombol "Cetak Kwitansi" per baris tender membuka halaman baru langsung di tab
   Kwitansi untuk tender itu, tombol Kembali mengembalikan ke Menu Pembayaran invoice yang sama,
   dan unduh PDF/WhatsApp/Email tetap berfungsi seperti sebelumnya.
3. Uji deep link: buka URL Dokumen Kasir langsung dengan `tenderId` yang tidak ada pada invoice
   itu — pastikan pesan "tidak ditemukan" tampil, bukan blank/crash.
4. Setelah verifikasi manual selesai, `BKC-DEC-063`–`064` (saat ini `approved` berdasar
   persetujuan langsung pengguna dalam percakapan) sudah final; tidak perlu approval formal
   tambahan kecuali ada perubahan scope lanjutan.
