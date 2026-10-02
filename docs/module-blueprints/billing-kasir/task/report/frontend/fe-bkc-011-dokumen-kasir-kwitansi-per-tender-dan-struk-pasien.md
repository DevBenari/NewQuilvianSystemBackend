# FE-BKC-011 — Dokumen Kasir: Kwitansi per Tender dan Struk Pasien

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-011` (task baru di luar `FE-BKC-001`–`010` asli, lahir dari amendment `/grill-me` 27-28 Agustus 2026 — `BKC-DEC-045`–`058`; belum ada di roadmap asli karena Menu Pembayaran/Dokumen Kasir digali setelah roadmap `revisi 1` ditulis) |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, perbaikan regresi + vertical slice baru |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Backend snapshot (dibaca sebagai bukti) | `BKC-DEC-057`/`058` sudah diimplementasikan backend (`BillingSettlementService.AddTenderAsync` mengalokasikan `TenderResponse.KwitansiNumber` otomatis per tender; endpoint lama `POST /billing/invoices/{id}/kwitansi` sudah dihapus) — lihat `task/report/backend/be-bkc-017-hardening-dan-acceptance-lintas-slice.md` dan riwayat percakapan sesi ini untuk detail rework backend |
| Status task | Source selesai ditulis, lint dan build lulus bersih. Belum di-commit. Belum diverifikasi manual ter-autentikasi (lihat *Definition of Done*) |

## Ringkasan untuk pembaca umum

Sebelum task ini, tombol "Dokumen Kasir" pada Menu Pembayaran **rusak** — ia memanggil endpoint
backend `POST /billing/invoices/{id}/kwitansi` yang sudah dihapus (backend mengubah desain nomor
Kwitansi dari "satu per invoice" menjadi "satu per pembayaran/tender", `BKC-DEC-057`). Task ini
memperbaiki Menu Pembayaran mengikuti desain baru itu, sekaligus membangun Struk Pasien
(`BKC-DEC-058`) yang sebelumnya hanya placeholder.

Perubahan yang terlihat kasir:

1. **Tabel Pembayaran (Split Tender) kini tampil di Menu Pembayaran** (sebelumnya hanya kartu
   ringkasan/progress bar, tanpa rincian per pembayaran). Setiap baris pembayaran punya tombol
   **"Cetak Kwitansi"**.
2. Menekan "Cetak Kwitansi" pada satu baris membuka modal Dokumen Kasir langsung ke tab Kwitansi
   untuk pembayaran (tender) itu — nomor Kwitansi sudah tersedia otomatis, kasir tidak perlu
   "meminta" nomor secara terpisah lagi. Kwitansi selalu bisa dicetak berapa pun status
   pembayaran itu (Berhasil/Pending/Gagal), badge menyesuaikan.
3. Tombol umum "Dokumen Kasir" (di kartu Ringkasan Pembayaran) sekarang membuka modal langsung
   ke tab **Struk Pasien** — rincian tagihan tercetak (obat/tindakan/biaya admin), datanya sama
   persis dengan tabel "Tagihan Pasien" yang sudah tampil di halaman itu.
4. Enam tab lain (Resep Obat, LMA, LML, Claim Letter, SPT, Dokumen Pasien) tetap placeholder yang
   jujur menyebutkan bahwa dokumennya milik modul lain — bukan konten kosong yang terlihat
   seperti bug.

## Proses bisnis

### Proses 1 — Cetak Kwitansi per pembayaran

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Memberi bukti bayar resmi bernomor untuk SATU transaksi pembayaran (tender), termasuk saat pasien membayar bertahap (split payment) — setiap pembayaran punya Kwitansi sendiri. |
| Pelaku | Kasir yang sedang memproses pembayaran invoice pada Menu Pembayaran. |
| Pemicu | Kasir menekan "Cetak Kwitansi" pada baris pembayaran tertentu di tabel "Pembayaran (Split Tender)". |
| Prasyarat | Tender itu sudah tersimpan di backend (nomor Kwitansi otomatis ada sejak saat itu — lihat *Proses bisnis backend* di laporan `BE-BKC-017`/rework `BKC-DEC-057`). |
| Langkah utama | 1) Kasir menekan "Cetak Kwitansi" pada baris tender. 2) Modal Dokumen Kasir terbuka langsung di tab Kwitansi, menampilkan dokumen untuk tender itu (nomor Kwitansi, metode pembayaran, nominal, badge status, nama pasien, no. RM/kunjungan/invoice). 3) Kasir dapat menekan "Cetak Kwitansi Pasien" (unduh PDF), "WhatsApp", atau "Email". |
| Aturan bisnis | Nomor Kwitansi TIDAK diminta dari frontend — sudah melekat pada data tender yang diterima dari backend (`TenderResponse.kwitansiNumber`), sehingga frontend hanya membaca, tidak pernah menghasilkan atau meminta nomor baru. Badge status dokumen mengikuti status tender (`SUCCEEDED`→"DITERIMA", `PENDING`→"MENUNGGU KONFIRMASI", `FAILED`→"GAGAL", dst.), bukan status invoice. |
| Contoh konkret | Invoice Rp1.000.000 dibayar 2 tahap: tunai Rp300.000 (langsung Berhasil) lalu QRIS Rp700.000 (sempat Pending). Baris pertama di tabel Pembayaran punya tombol Cetak Kwitansi yang menghasilkan Kwitansi No. `KWS-...-0001` senilai Rp300.000 berbadge "DITERIMA"; baris kedua punya Kwitansi No. `KWS-...-0002` senilai Rp700.000 berbadge "MENUNGGU KONFIRMASI" sampai QRIS-nya dikonfirmasi. Keduanya nomor yang berbeda, sesuai `BKC-DEC-057`. |
| Perubahan status | Tidak ada — mencetak Kwitansi murni tindakan baca/ekspor, tidak mengubah data. |
| Jalur tidak normal | Tender tanpa `kwitansiNumber` (seharusnya tidak pernah terjadi karena backend selalu mengalokasikannya saat tender dibuat) → tombol "Cetak Kwitansi" nonaktif dengan title "Nomor Kwitansi belum tersedia untuk tender ini.". Pembuatan PDF gagal (mis. `html2pdf.js` gagal memuat) → `handleActionError` menampilkan toast kegagalan, modal tetap terbuka. |
| Hasil akhir | File PDF Kwitansi terunduh ke perangkat kasir; bila lewat WhatsApp/Email, aplikasi terkait terbuka dengan pesan siap pakai (kasir melampirkan file yang sudah terunduh secara manual — `BKC-DEC-056`). |

### Proses 2 — Cetak Struk Pasien

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Rincian tagihan tercetak (bukan bukti bayar) — daftar obat/tindakan/biaya administrasi yang membentuk invoice. |
| Pelaku | Kasir. |
| Pemicu | Menekan tombol umum "Dokumen Kasir" pada kartu Ringkasan Pembayaran. |
| Prasyarat | Invoice sudah dimuat (tidak butuh tender/pembayaran apa pun — Struk Pasien berlaku sebelum maupun sesudah pembayaran). |
| Langkah utama | 1) Kasir menekan "Dokumen Kasir". 2) Modal terbuka langsung ke tab Struk Pasien, menampilkan tabel item yang identik dengan tabel "Tagihan Pasien" di halaman yang sama (item VOIDED tidak ikut ditampilkan/dihitung). 3) Kasir menekan "Cetak Struk Pasien" untuk mengunduh PDF. |
| Aturan bisnis | Tidak ada sumber data baru — item, quantity, harga, dan total diambil langsung dari `invoice.items` yang sudah dimuat Menu Pembayaran (`BKC-DEC-058`: "datanya sudah tersedia penuh di tabel Tagihan Pasien... tidak memerlukan data dari modul lain"). |
| Contoh konkret | Invoice dengan 3 item aktif (Pemeriksaan Rp150.000, Obat A Rp50.000, Obat B Rp30.000) dan 1 item VOIDED (Konsultasi Rp100.000, dibatalkan). Struk Pasien menampilkan 3 baris aktif dengan total Rp230.000 — item VOIDED tidak muncul dan tidak ikut dijumlah, sama seperti tabel Tagihan Pasien. |
| Perubahan status | Tidak ada. |
| Jalur tidak normal | Invoice tanpa item aktif → tabel menampilkan baris "Belum ada item pada invoice ini." (bukan tabel kosong tanpa keterangan). |
| Hasil akhir | File PDF Struk Pasien terunduh ke perangkat kasir. |

## Base Component Decision Gate

`UI GATE: 4 elemen — REUSE 2, EXTEND 1, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi/keputusan |
| --- | --- | --- | --- | --- |
| Tombol "Cetak Kwitansi" per baris tender, tombol "Cetak Struk Pasien"/"Cetak Kwitansi Pasien"/WhatsApp/Email di modal | `BaseButton` | `src/components/features/base-features/base-button` — sudah dipakai luas di modul ini (`menu-pembayaran-view.jsx`, `add-tender-modal.jsx`, dst.) | REUSE | Dipakai apa adanya dengan `variant`/`size`/`loading` yang sudah ada, tanpa varian baru. |
| Tabel daftar pembayaran (tender) dengan kolom aksi baru | `DataTable` (via `BillingSettlementPanel`, komponen domain yang sudah ada) | `src/components/view/.../detail/billing-settlement-panel.jsx` — sudah dipakai di halaman detail invoice biasa (`billing-invoice-detail-view.jsx`) | EXTEND | Tambah dua prop OPSIONAL (`onPrintKwitansi`, `showActions`) yang tidak mengubah perilaku default: pemakai lama (`billing-invoice-detail-view.jsx`) tidak mengirim keduanya, sehingga tampil identik seperti sebelumnya (diverifikasi langsung di source, tidak ada prop itu di pemanggilnya). Alternatif yang dipertimbangkan dan ditolak: (a) membangun tabel tender baru khusus Menu Pembayaran — menduplikasi kontrak kolom/status/format yang sudah identik di `BillingSettlementPanel`, ditolak karena redundan; (b) taruh aksi cetak di kartu Ringkasan Pembayaran tanpa tabel — tidak bisa menunjuk ke tender SPESIFIK mana yang mau dicetak saat ada lebih dari satu pembayaran (split payment), ditolak karena tidak lengkap. |
| Tab navigasi dokumen (Kwitansi/Struk Pasien/6 placeholder) di modal Dokumen Kasir | Tidak ada base Tabs (`ls src/components/features/base-features \| grep tab` → nihil; satu-satunya hit `features/tabs/base-tabs-component.jsx` diperiksa langsung dan ternyata `BaseTabsPerawat`, layout halaman perawat rawat inap dengan `Card`+`FormProvider`+fetch kunjungan — bukan primitive tab navigasi umum, katalog skill juga tidak mendaftarkannya) | `react-bootstrap` `Nav variant="tabs"` | WRAP | react-bootstrap sudah menjadi primitive tingkat yang sama dengan `Modal`/`Form` yang dipakai langsung di modal-modal modul ini (`create-settlement-modal.jsx`, dst.) — bukan komponen baru, hanya pola domain (`WRAP`) yang sudah dipakai sejak versi awal `dokumen-kasir-modal.jsx` pada sesi sebelumnya. |
| Dokumen printable Kwitansi/Struk Pasien (layout A5, tabel `data-flat-table`) | Pola `print-resep-component.jsx` (react-to-print) DIPERIKSA tapi tidak dipakai — kebutuhannya beda: WhatsApp/Email butuh file Blob yang bisa diunduh, `react-to-print` hanya membuka dialog print browser | `html2pdf.js` (dependency sudah terpasang, sebelumnya tidak dipakai) dalam komponen domain baru | WRAP (bukan `NEW` base component — pola presentational khusus percetakan, sudah didirikan sejak `kwitansi-document.jsx` versi pertama pada sesi sebelumnya; `struk-pasien-document.jsx` murni mengikuti pola yang sama, bukan pola baru) | Keputusan (bukan pilihan baru pada task ini) mengulang keputusan tervalidasi sesi sebelumnya — lihat `00-interview-decisions.md` `BKC-DEC-053`. |

## Endpoint yang dikonsumsi

Tidak ada endpoint baru. Task ini BERHENTI memanggil satu endpoint yang sudah dihapus backend:

| Method | Path | Status |
| --- | --- | --- |
| ~~`POST`~~ | ~~`/{id}/kwitansi`~~ | **Dihapus dari frontend** — endpoint backend-nya sudah tidak ada (`GetOrAllocateKwitansiNumber` di `BillingInvoicesController` dihapus saat rework `BKC-DEC-057`). Thunk `getOrAllocateKwitansiNumber`, state `kwitansi*`, dan selector terkait di `billing-invoice-slice.jsx` ikut dihapus (dead code). |

Nomor Kwitansi kini didapat cuma-cuma dari field `kwitansiNumber` pada `TenderResponse` yang
sudah dikembalikan endpoint yang SUDAH ADA dan sudah dikonsumsi sebelumnya:

| Method | Path | Field baru yang dikonsumsi |
| --- | --- | --- |
| `POST` | `.../settlements/{id}/tenders` | `kwitansiNumber` pada response (sudah ada di `TenderResponse` backend, tidak butuh perubahan thunk — hanya field tambahan yang otomatis ikut lewat `unwrapEnvelope`) |
| `GET` | `.../settlements/{id}` | Idem — daftar `tenders[].kwitansiNumber` |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `eslint` (quiet, scope diubah) | **PASS** | `npx eslint --quiet <9 file berubah/baru>` → exit 0, tanpa output. |
| `eslint` (full severity, tanpa `--quiet`) | **PASS** | `npx eslint <7 file berubah/baru>` → exit 0, nol error, nol warning. |
| `npm run build` | **PASS** | `next build` exit 0, `postbuild` (`prepare-standalone.mjs`) selesai normal. Route `/health-services/billing-management/billing/invoices/[slug]/pembayaran` terkonfirmasi ada di `.next/server/app/...` hasil build. |
| Grep anti-regresi (checklist G) | **PASS** | #3 (tombol non-base) dan #5 (utility typography Bootstrap) nihil hasil pada seluruh file berubah. #4 (`<table>` tanpa `data-flat-table`) — dua `<table>` ditemukan (dokumen Kwitansi dan Struk Pasien), keduanya SUDAH diberi `data-flat-table="true"` mengikuti konvensi `print-resep-component.jsx`. |
| Hex/warna literal (checklist D) | **DIPERTAHANKAN, dengan alasan** | Kedua dokumen printable (`kwitansi-document.jsx`, `struk-pasien-document.jsx`) memakai warna hex literal inline (mis. `#101828`, `#12b76a`, `#f79009`, `#d92d20`) untuk badge status dan layout. Ini BUKAN stylesheet aplikasi (tidak ada file `.css`/`.module.css` baru), melainkan dokumen cetak/PDF yang harus tampil konsisten di atas kertas terlepas dari tema gelap/terang aplikasi — pola yang sama dengan `print-resep-obat.css` (styling literal, bukan design token aplikasi). Keputusan ini mengulang keputusan sesi sebelumnya untuk `kwitansi-document.jsx` versi pertama, bukan keputusan baru. |
| Test otomatis | `AUTOMATED TEST: SKIPPED (opsional) — repo tidak memakai Jest (test-policy.md), dan task ini murni perbaikan wiring + komponen presentational baru; tidak ada test unit baru ditulis` | — |
| Verifikasi manual (browser, tanpa login) | **NOT DONE pada task ini** | Tidak dijalankan ulang pada task ini — pola verifikasi headless sebelumnya (lihat laporan `FE-BKC-003`) berlaku untuk halaman lain; halaman `pembayaran` khusus BUTUH data invoice+tender nyata (bukan sekadar route 401-redirect) untuk terlihat berarti, sehingga smoke test tanpa login tidak banyak membuktikan untuk task spesifik ini. |
| Verifikasi manual ter-autentikasi (klik-coba nyata: tambah tender lalu cetak Kwitansi, cetak Struk Pasien, split payment 2 tender) | **NOT DONE** | Tidak ada kredensial login yang tersedia untuk builder; sengaja tidak diminta lewat chat untuk alasan keamanan. |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint dan build sudah lulus bersih (bukti di
atas), tapi klik-coba langsung dengan invoice dan pembayaran nyata (skenario split payment,
cetak Kwitansi per baris, cetak Struk Pasien, unduh PDF benar-benar terbuka dan terbaca) belum
dijalankan.

## Risiko yang tersisa

1. `html2pdf.js` belum pernah diverifikasi benar-benar menghasilkan PDF yang valid di browser
   nyata pada task manapun sesi ini (baik versi pertama `kwitansi-document.jsx` maupun
   `struk-pasien-document.jsx` yang baru) — hanya lint/build yang lulus. Rendering `html2canvas`
   atas elemen dengan banyak `<table>`/style inline punya risiko layout PDF yang meleset dari
   tampilan di layar; ini HANYA bisa dikonfirmasi lewat klik-coba nyata.
2. `BillingSettlementPanel` sekarang dipakai di DUA konteks (`billing-invoice-detail-view.jsx`
   tanpa `onPrintKwitansi`/`showActions`, dan `menu-pembayaran-view.jsx` dengan keduanya) — bila
   task mendatang mengubah `BillingSettlementPanel` tanpa memeriksa kedua pemakai, salah satu
   bisa regresi diam-diam. Disarankan task berikutnya yang menyentuh file ini memeriksa kedua
   pemakai secara eksplisit.

## Langkah berikutnya yang direkomendasikan

1. Login dengan peran yang punya akses Menu Pembayaran, buka satu invoice, lakukan split payment
   (≥2 tender berbeda metode), lalu verifikasi: tabel Pembayaran menampilkan kedua tender dengan
   nomor Kwitansi berbeda, tombol Cetak Kwitansi per baris menghasilkan PDF yang benar (nominal,
   metode, badge status sesuai baris itu — bukan tercampur baris lain), dan tombol umum Dokumen
   Kasir membuka Struk Pasien dengan rincian item yang cocok dengan tabel Tagihan Pasien.
2. Konfirmasi WhatsApp/Email membuka aplikasi dengan pesan yang benar setelah PDF terunduh
   (`BKC-DEC-056`).
3. Setelah verifikasi manual selesai dan dicatat, `BKC-DEC-052`–`058` (masih berstatus `draft` di
   `00-interview-decisions.md`) bisa diajukan untuk approval formal Product/Domain Owner.
