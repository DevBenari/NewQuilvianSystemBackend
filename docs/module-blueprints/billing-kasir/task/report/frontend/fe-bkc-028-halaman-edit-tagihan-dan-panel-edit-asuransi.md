# FE-BKC-028 — Halaman Edit Tagihan dan panel Edit Asuransi

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-028` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `1.1`) |
| Task type | Frontend, halaman kerja baru (`FE-MPY-01`) + satu panel penuh (`FE-MPY-02`) + ekstraksi komponen bersama |
| Task mode | `FRONTEND` (backend read-only — ketiga endpoint `BE-BKC-047` dibaca langsung dari source dan dikonfirmasi ada persis sesuai kontrak, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-047` ✅, `BE-BKC-048` ✅, `BE-BKC-049` ✅ — ketiga endpoint dasarnya (`edit-context`, `item-payer-assignments`, `drug-billing-disposition`) dikonfirmasi ada di `BillingInvoicesController.cs`; hanya tiga endpoint milik `BE-BKC-047` yang dipakai task ini |
| Status task | 🟡 **Sebagian.** Source untuk kerangka `FE-MPY-01`, panel `FE-MPY-02`/`03`/`04`, dan tiga amendment UI/UX 14 September 2026 (refactor tabel tunggal; rapikan Menu Pembayaran & Edit Tagihan; perbaikan whitespace/placement/comparison sizing) sudah lengkap ditulis. `npm run lint`/`test:unit`/`build` dan klik-coba ter-autentikasi **belum dijalankan** — lihat § DoD. Belum di-commit |

## Amendment 14 September 2026 — Refactor tabel tunggal (konsolidasi Edit Asuransi/Status Tagihan/Billing)

**Latar belakang.** Setelah `FE-BKC-029` dan `FE-BKC-030` menyelesaikan panelnya masing-masing,
pengguna meninjau tampilan gabungan ketiganya dan menemukan pola yang secara eksplisit ingin
dihindari: setiap panel (Edit Asuransi, Edit Status Tagihan, Edit Billing) merender **tabel mini
miliknya sendiri** ("Ubah penanggung biaya per item", "Deskripsi/Jumlah/Ditebus") di ATAS, lalu
tabel "Rincian Tagihan" baca-saja yang sama (`BillingInvoiceItemsTable`) dirender LAGI di bawahnya
— dua tabel menampilkan baris tagihan yang sama, salah satunya untuk edit dan satunya untuk
pratinjau. Task ini me-refactor ketiganya menjadi **satu tabel tagihan** yang kolom Status-nya
berubah menjadi kontrol inline sesuai mode aktif, mengikuti referensi visual yang diberikan
pengguna.

**Perubahan struktural:**

1. **`billing-invoice-items-table.jsx`** (dipakai bersama Menu Pembayaran) diberi dua prop opsional
   baru — `renderStatusCell(item)` dan `getRowClassName(item)` — yang bila TIDAK dikirim (kasus
   Menu Pembayaran) menjaga perilaku baca-saja lama persis seperti sebelumnya. Ini satu-satunya
   perubahan pada komponen bersama ini; **nol dampak** pada Menu Pembayaran.
2. **`edit-tagihan-view.jsx`** kini memanggil ketiga hook panel (`useEditAsuransiPanel`,
   `useEditStatusTagihanPanel`, `useEditBillingPanel`) langsung di level halaman (Rules of Hooks —
   ketiganya dipanggil tanpa syarat setiap render, hanya hasil milik mode aktif yang dipakai
   membangun `renderStatusCell`), lalu merender **satu** `BillingInvoiceItemsTable`:
   - Mode **Edit Status Tagihan**: kolom Status menjadi grup tombol Pribadi/Asuransi/Penjamin per
     baris (logika/markup dipindah persis dari tabel mini lama, hanya lokasinya berpindah).
   - Mode **Edit Billing**: kolom Status menjadi badge Ditebus/Tidak Ditebus (mode `ALL`/`NONE`)
     atau kotak centang (mode `PARTIAL`, hanya baris obat) — baris non-obat tetap badge coverage
     biasa.
   - Mode **Edit Asuransi** (tanpa membandingkan): badge biasa, tidak berubah — interaksi mode ini
     tetap di pemilihan kandidat penjamin, bukan per-baris.
3. **`edit-asuransi-panel.jsx`**: blok perbandingan (`Bandingkan`) yang sebelumnya hanya
   menampilkan ringkasan `<dl>` Total/Ditanggung/Mandiri kini menyertakan **dua**
   `BillingInvoiceItemsTable` berdampingan (SEKARANG vs BILA DIGANTI) memakai komponen tabel yang
   SAMA untuk kedua sisi — sesuai instruksi eksplisit "jangan implementasikan dua logika tabel
   berbeda". Status per baris tiap sisi diturunkan dari `PerItemComparison`
   (`PayerComparisonPreviewResponse`, murni data server). Tabel tunggal utama disembunyikan
   sementara saat mode ini menampilkan perbandingan, supaya tagihan yang sama tidak pernah dirender
   tiga kali sekaligus.
4. **`edit-status-tagihan-panel.jsx`** dan **`edit-billing-panel.jsx`**: `<table>` mini masing-masing
   **dihapus seluruhnya**. Keduanya kini murni strip kontrol (toggle mode/penghitung item diubah,
   alasan, bar Batal/Simpan) dan menerima hasil hook (`panel`) sebagai prop dari parent, bukan
   memanggil hook sendiri — supaya state pilihan/centang bisa dipakai bersama oleh kolom Status
   pada tabel tunggal.
5. **Ketiga hook panel** (`use-edit-asuransi-panel.js`, `use-edit-status-tagihan-panel.js`,
   `use-edit-billing-panel.js`) diberi tambahan field `isDirty` (murni derivasi dari state yang
   sudah ada, tidak ada state baru) untuk gerbang konfirmasi ganti mode.
6. **Gerbang ganti mode dengan draft belum disimpan** (§31 spesifikasi task): `edit-tagihan-view.jsx`
   kini menahan `setActiveMode` di belakang `requestModeChange` — bila mode aktif `isDirty`,
   `ConfirmModal` "Perubahan belum disimpan. Buang perubahan dan pindah mode?" muncul lebih dulu.
   Draft mode yang TIDAK sedang ditampilkan tidak pernah hilang tanpa sengaja karena hook-nya tetap
   hidup (dipanggil tanpa syarat).
7. **Entry point ganda diselesaikan** (§2 spesifikasi task) — `menu-pembayaran-view.jsx`: tombol
   yang tadinya berlabel **"🔀 Edit Penanggung"** (padahal ini yang navigasi ke halaman Edit Tagihan
   terpusat) diganti labelnya menjadi **"✏️ Edit Tagihan"**; tombol lain yang tadinya JUGA berlabel
   **"✏️ Edit Tagihan"** (padahal fungsinya toggle form "Tambah Biaya Lain-Lain" yang tidak
   berhubungan sama sekali dengan payer editing) diganti menjadi **"➕ Tambah Biaya Lain-lain"**.
   Kedua tombol dan fiturnya **tidak dihapus** — hanya label yang diperjelas supaya tidak lagi
   bertabrakan nama. `href`/handler masing-masing tidak berubah.

**Constraint kontrak yang ditemukan (dilaporkan apa adanya, TIDAK disiasati):**

1. **§17/§28 "Alasan opsional" TIDAK dapat dipenuhi tanpa mengubah backend.**
   `BillingPayerEditService` (backend) menolak keras (`400`, `BIL-VAL-062` *"Alasan perubahan wajib
   diisi"*) permintaan dengan `Reason` kosong pada ketiga endpoint (`PUT /payment-source`,
   `PUT /item-payer-assignments`, `PUT /drug-billing-disposition`). Task ini `FRONTEND MODE` dan
   secara eksplisit dilarang mengubah kontrak API. Field "Alasan perubahan" karena itu **tetap
   wajib** (tombol Simpan tetap tergerbang `Boolean(reason.trim())`) di ketiga panel — bukan diam-
   diam dikirim nilai dummy untuk menyiasati validasi backend. Direkomendasikan: task backend
   terpisah melonggarkan `BIL-VAL-062` bila produk memang menghendaki alasan opsional.
2. **§12-14 "Live calculation preview" hanya tersedia untuk Edit Asuransi.** Backend hanya
   menyediakan satu endpoint pratinjau tanpa efek samping (`POST /payer-comparison-preview`) untuk
   ganti payer kunjungan. Untuk Edit Status Tagihan dan Edit Billing, **tidak ada** endpoint
   pratinjau — keduanya hanya endpoint mutate langsung (`PUT`). Menghitung pratinjau sendiri di
   React eksplisit dilarang spesifikasi task ini ("Jangan menghitung coverage sendiri di React"),
   dan task ini dilarang menambah endpoint baru. Konsekuensinya: Ringkasan Pembayaran pada dua mode
   itu baru berubah **setelah** "Simpan Perubahan" berhasil (memuat ulang `edit-context`), bukan
   reaktif per-klik draft seperti Edit Asuransi. Ini didokumentasikan langsung sebagai komentar di
   `edit-tagihan-view.jsx` supaya tidak terulang salah paham di sesi berikutnya.

**File yang ikut berubah pada amendment ini** (tambahan dari daftar § File yang diubah/ditambah
di bawah, yang mencatat kondisi ASLI sebelum amendment):

| File | Perubahan |
| --- | --- |
| `billing-invoice-items-table.jsx` | Tambah prop opsional `renderStatusCell`, `getRowClassName` (backward compatible) |
| `edit-tagihan-view.jsx` | Restrukturisasi: satu tabel, ketiga hook panel dipanggil di level halaman, gerbang ganti mode |
| `edit-asuransi-panel.jsx` | Blok perbandingan kini merender dua `BillingInvoiceItemsTable` berdampingan, bukan hanya `<dl>` |
| `edit-status-tagihan-panel.jsx` | `<table>` mini dihapus; jadi strip kontrol menerima `panel` sebagai prop |
| `edit-billing-panel.jsx` | `<table>` mini dihapus; jadi strip kontrol menerima `panel` sebagai prop |
| `use-edit-asuransi-panel.js`, `use-edit-status-tagihan-panel.js`, `use-edit-billing-panel.js` | Tambah field `isDirty` (derivasi, tanpa state baru) |
| `menu-pembayaran-view.jsx` | Label dua tombol diperjelas (§2) — `href`/handler tidak berubah |

**Validasi amendment:** sama seperti seluruh sesi task ini — **`npm run lint`/`test:unit`/`build`
TIDAK dijalankan** sesuai instruksi eksplisit pengguna ("jangan lakukan build automatis, biarkan
saya yang build secara manual"). Diverifikasi lewat pembacaan ulang menyeluruh source (tag JSX
terbuka/tertutup, prop yang dikonsumsi memang dikirim, tidak ada import yang tidak terpakai).
`git status --short` dikonfirmasi hanya menyentuh 9 berkas di atas — nihil perubahan sampingan.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi (sama seperti seluruh rumpun task frontend modul ini)
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna) — `lint`/`test:unit`/`build` menunggu dijalankan pengguna sendiri

## Amendment 14 September 2026 (lanjutan) — Rapikan UI/UX Menu Pembayaran dan Edit Tagihan

**Latar belakang.** Setelah amendment "Refactor tabel tunggal" di atas, pengguna meninjau tampilan
lebih lanjut lewat screenshot dan menemukan masalah tampilan/layout murni (bukan struktural): (1)
header "Rincian Tagihan" pada Menu Pembayaran menampilkan dua tombol berdampingan padahal
seharusnya hanya satu; (2) halaman Edit Tagihan masih terasa seperti dua card form sempit
bertumpuk di tengah halaman, bukan satu workspace lebar; (3) kartu kategori penjamin masih
menampilkan "-" sebagai filler; (4) Ringkasan Pembayaran hanya `<dl>` polos tanpa hierarki visual;
(5) tabel tagihan pada Edit Tagihan memakai kotak scroll internal pendek (360px) yang cocok untuk
Menu Pembayaran tetapi tidak untuk halaman penuh.

**1. Header "Rincian Tagihan" — hanya `Edit Tagihan` (§2-4 spesifikasi).**
`menu-pembayaran-view.jsx`: tombol "➕ Tambah Biaya Lain-lain" dipindah keluar dari
`.tagihanHeaderArea` (yang sekarang HANYA berisi judul + tombol "✏️ Edit Tagihan") ke baris aksi
sekunder baru (`.secondaryActionsRow`, class baru di `menu-pembayaran.module.css`) tepat di
bawahnya, rata kanan, variant `ghost` (bukan `secondary`) supaya terasa sebagai aksi sekunder,
bukan setara "Edit Tagihan". **Fitur, form, endpoint, dan validasi "Tambah Biaya Lain-lain" TIDAK
disentuh sama sekali** — hanya lokasi tombol pemicunya (`onClick`, `disabled`, `title` persis
sama). Posisi barunya BUKAN "dikembalikan ke lokasi lama" secara harfiah — audit source
mengonfirmasi kedua tombol itu sudah berdampingan di header sejak sebelum sesi ini (bukan sesuatu
yang dipindahkan session-session sebelumnya di sesi ini) — melainkan dipisah ke baris tersendiri
sesuai instruksi eksplisit "HANYA Edit Tagihan di header", supaya tidak ada dua tujuan aksi
tercampur dalam satu baris judul.

**2. Edit Tagihan full-width, satu workspace card (§5-8 spesifikasi).**
`edit-tagihan.module.css`: `.page` kehilangan `max-width: 1080px; margin: 0 auto;`, diganti
`width: 100%`. `edit-tagihan-view.jsx`: wrapper halaman kini `region-page-shell` +
`baseStyles.dataShell` + `styles.page` — pola persis yang dipakai `menu-pembayaran-view.jsx`
(`region-page-shell ${baseStyles.dataShell} ${styles.paymentShell}`). Dua card terpisah
(`.headerCard` lalu `.panelCard` yang membungkus panel aktif) digabung menjadi **satu**
`.workspaceCard`: judul+tombol Kembali → subjudul penanggung kunjungan → toolbar tiga mode →
`<hr className={styles.workspaceDivider}>` → konten mode aktif (`.panelBody`) — seluruhnya di
dalam satu elemen. Ketiga komponen panel (`EditAsuransiPanel`, `EditStatusTagihanPanel`,
`EditBillingPanel`) tidak lagi merender `<div className={styles.panelCard}>` pembungkus sendiri —
kontennya (termasuk bar Batal Edit/Simpan Perubahan di ujung masing-masing) sekarang mengalir
langsung di dalam `.panelBody` milik parent, sehingga secara visual jadi satu workspace, bukan
kotak-dalam-kotak.

**3. Payer category — tidak ada lagi "-" filler (§9-10 spesifikasi).**
`base-payer-workspace.jsx` (`BasePayerCategorySelector`, komponen BASE bersama — juga dipakai
langkah pembayaran admisi Rawat Inap): baris `<em>{category.subtitle || category.description ||
"-"}</em>` diubah jadi hanya merender `<em>` bila memang ada `subtitle`/`description` — bila tidak
ada, elemen itu sama sekali tidak dirender (bukan diganti teks kosong). Perubahan **aditif murni**:
kategori yang sudah mengirim `subtitle`/`description` tidak berubah tampilannya sama sekali;
konsumen lain komponen ini (Rawat Inap) hanya terdampak bila kategorinya JUGA sebelumnya jatuh ke
fallback "-" — dan bila iya, itu perbaikan yang sama, bukan regresi. Tidak ada perubahan pada
`BaseSavedPayerCard` (kartu kandidat asuransi/penjamin tersimpan) — fallback "-" di situ (untuk
`title`/`numberValue` yang benar-benar kosong) tetap dipertahankan karena itu bukan filler
kosmetik, melainkan indikator data yang sungguh tidak ada.

Kompaksi tinggi kartu kategori (§9, "buat lebih compact") **TIDAK dikerjakan pada amendment ini** —
`BasePayerCategorySelector`/`BaseSavedPayerCard` adalah komponen BASE bersama dengan konsumen lain
(Rawat Inap); mengubah dimensi/padding-nya secara visual berisiko regresi di luar cakupan yang bisa
diverifikasi sesi ini tanpa environment ter-autentikasi. Diprioritaskan sesuai §30 spesifikasi
task (item compaction ada di urutan lebih rendah dari struktur/lebar/tabel/summary yang sudah
dikerjakan) dan dilaporkan sebagai risiko tersisa, bukan diam-diam dilewati.

**4. Tabel tagihan — page-level scroll, bukan kotak scroll pendek (§14-16 spesifikasi).**
`billing-invoice-items-table.jsx`: prop opsional baru `scrollable` (default `true` — perilaku Menu
Pembayaran TIDAK berubah sama sekali). `edit-tagihan-view.jsx` dan kedua tabel perbandingan di
`edit-asuransi-panel.jsx` mengirim `scrollable={false}`, melepas kelas `.itemTableScroll`
(`max-height: 360px; overflow-y: auto;`) sehingga tabel tumbuh mengikuti konten dan halaman
memakai scroll level-halaman, sesuai preferensi eksplisit spesifikasi ("page-level scroll lebih
disukai").

**5. Ringkasan Pembayaran — hierarki visual (§19-21 spesifikasi).**
`edit-tagihan.module.css`: `<dl>` polos (memakai class `.comparisonColumn` yang sebenarnya dibuat
untuk konteks lain) diganti class baru khusus ringkasan (`.summaryCard`, `.summaryHeading`,
`.summaryList`, `.summaryRow`, `.summaryTotalGroup`) mengikuti pola `.summaryRow`/`.summaryTotal`
yang SUDAH dipakai `menu-pembayaran.module.css` — bukan warna/gradient baru, murni border +
typography + `text-align: right` dan `font-variant-numeric: tabular-nums` pada angka, dua baris
Total Tagihan/Harus Dibayar Pasien dipisah kelompok sendiri dengan border-top dan font lebih tebal
supaya lebih menonjol. `.summaryCard` juga membawa
`margin-bottom: var(--app-footer-safe-space, 96px) !important` (lihat butir 7 di bawah).

**6. Loading state saat pratinjau (§23 spesifikasi).**
Baris "Menghitung ulang tagihan..." (`.summaryLoadingHint`) muncul di atas Ringkasan Pembayaran
saat `asuransiPanel.comparisonLoading` true (mode Edit Asuransi, saat tombol "Bandingkan" ditekan)
— satu-satunya mode yang memang punya endpoint pratinjau (lihat constraint §22 di bawah).

**7. Footer overlap (§26 spesifikasi).**
Diaudit: `base-data-components.module.css` `.tableCard` sudah membawa
`margin-bottom: var(--app-footer-safe-space, 96px)` bawaan — token yang sama dipakai luas di
seluruh project (dikonfirmasi dipakai `menu-pembayaran.module.css`, `base-editor.module.css`, dan
belasan modul lain, dihitung dinamis oleh `Footer.jsx`). Section Ringkasan Pembayaran (section
PALING BAWAH halaman Edit Tagihan) sudah memakai `baseStyles.tableCard` sejak awal, jadi ruang
aman footer semestinya sudah ada; `.summaryCard` di atas menambahkan `margin-bottom` yang sama
secara eksplisit (dengan `!important`, pola yang sama dipakai `menu-pembayaran.module.css` untuk
alasan spesifisitas urutan modul CSS) supaya tidak bergantung diam-diam pada urutan muat CSS.
**Tidak dipakai trik `z-index`** sesuai larangan eksplisit spesifikasi.

**Constraint kontrak yang dikonfirmasi ULANG masih berlaku (tidak berubah dari amendment
sebelumnya, source backend diperiksa ulang pada sesi ini):**

1. **§11 "Alasan opsional":** label ketiga panel diubah menjadi "Alasan perubahan (opsional)" TANPA
   asterisk (`required: false` pada definisi field — dikonfirmasi hanya mengubah tampilan visual,
   `BaseTextAreaField`/`BaseTextField` tidak dibungkus `<form>` di panel manapun sehingga atribut
   HTML `required` yang ikut terpasang tidak pernah punya efek fungsional di sini). **Tombol Simpan
   TETAP digerbang non-aktif selama textarea kosong** (`panel.canSave` di ketiga hook, tidak
   diubah) — karena `BillingPayerEditService` (backend) masih menolak keras (`400`, `BIL-VAL-062`)
   permintaan dengan alasan kosong pada ketiga endpoint edit. Ini **bukan** "opsional penuh"
   sebagaimana secara eksplisit diperingatkan pada instruksi task — dilaporkan apa adanya.
2. **§22 "Summary must react":** dikonfirmasi ulang backend HANYA punya satu endpoint pratinjau
   (`payer-comparison-preview`, Edit Asuransi). Edit Status Tagihan dan Edit Billing tetap tidak
   punya endpoint pratinjau — Ringkasan pada dua mode itu baru berubah setelah Simpan Perubahan
   berhasil (reload `edit-context`), bukan reaktif per-draft. Tidak ada endpoint baru dibuat, tidak
   ada perhitungan coverage disimulasikan di React (keduanya dilarang eksplisit task ini).

**File yang berubah pada amendment ini:**

| File | Perubahan |
| --- | --- |
| `menu-pembayaran-view.jsx` | Tombol "Tambah Biaya Lain-lain" dipindah dari header Rincian Tagihan ke baris aksi sekunder baru |
| `menu-pembayaran.module.css` | Kelas baru `.secondaryActionsRow`; komentar `.tagihanHeaderActions` diperbarui |
| `edit-tagihan.module.css` | `.page` full-width; `.headerCard`+`.panelCard` digabung jadi `.workspaceCard`+`.workspaceDivider`+`.panelBody`; kelas ringkasan baru (`.summaryCard` dst.) |
| `edit-tagihan-view.jsx` | Header+toolbar+panel digabung satu `<div className={styles.workspaceCard}>`; page shell disamakan Menu Pembayaran; tabel utama `scrollable={false}`; Ringkasan dirender ulang dengan kelas baru + loading hint |
| `edit-asuransi-panel.jsx` | Wrapper `panelCard` dihapus (fragment); kedua tabel perbandingan `scrollable={false}`; label Reason → "(opsional)" visual |
| `edit-status-tagihan-panel.jsx` | Wrapper `panelCard` dihapus (fragment); label Reason → "(opsional)" visual |
| `edit-billing-panel.jsx` | Wrapper `panelCard` dihapus (fragment); label Reason → "(opsional)" visual |
| `billing-invoice-items-table.jsx` | Prop opsional baru `scrollable` (default `true`, Menu Pembayaran tidak terdampak) |
| `base-payer-workspace.jsx` | `BasePayerCategorySelector`: `<em>` fallback "-" dihapus, hanya dirender bila ada subtitle/description sungguhan (aditif, base component bersama Rawat Inap) |

**Base Component Decision Gate (amendment ini):**

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Workspace card gabungan, divider, baris ringkasan | `REUSE` (styling lokal) | Class CSS module lokal baru (`.workspaceCard` dst.), bukan komponen React baru — murni penataan ulang elemen yang sudah ada (`BaseButton`, `BaseTextAreaField`, dst. tidak diganti) |
| Baris aksi sekunder "Tambah Biaya Lain-lain" | `REUSE` | `BaseButton` variant `ghost` (variant yang sudah ada di `VARIANT_CLASS`, bukan variant baru) |
| Penghapusan fallback "-" pada `BasePayerCategorySelector` | `EXTEND` (aditif, tidak mengubah default konsumen existing) | Base component bersama Rawat Inap — perubahan hanya menghapus rendering fallback teks, tidak mengubah props/behavior API komponen; konsumen yang sudah mengirim `subtitle`/`description` sungguhan (termasuk kemungkinan Rawat Inap) tidak berubah tampilannya sama sekali |
| Kompaksi tinggi kartu payer | **Tidak dikerjakan** | Lihat butir 3 di atas — dilaporkan sebagai risiko tersisa, bukan gerbang keputusan yang menunggu (bukan `NEW`, tidak butuh komponen baru — murni diprioritaskan lebih rendah dan ditinggalkan eksplisit) |

`UI GATE: 0 elemen NEW, 1 elemen EXTEND aditif (base-payer-workspace.jsx, tidak mengubah default konsumen existing), sisanya REUSE`

**Validasi amendment ini:** sama seperti amendment sebelumnya — **`npm run lint`/`test:unit`/`build`
TIDAK dijalankan** sesuai instruksi eksplisit pengguna yang berulang di sesi ini. Diverifikasi
lewat pembacaan ulang menyeluruh source (tag JSX terbuka/tertutup, prop yang dikonsumsi memang
dikirim). `git status --short` dikonfirmasi menyentuh persis 10 berkas di atas — nihil perubahan
sampingan (satu berkas lain, `billing-invoices-view.jsx`, sudah berubah dari pekerjaan lain di
working tree yang sama sebelum amendment ini dimulai — tidak disentuh).

- MANUAL TEST: NOT FEASIBLE — tidak ada environment ter-autentikasi pada sesi ini untuk QA visual 5 breakpoint (1366×768, 1440×900, 1920×1080, 1024×768, 768px) maupun skenario Cash/Insurance/Company Guarantor/multiple comparison/long table yang diminta §35 spesifikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

**Risiko tersisa (amendment ini):**

1. Kompaksi tinggi kartu payer category/saved-payer (§9 spesifikasi) belum dikerjakan — lihat butir 3 di atas.
2. Tidak ada verifikasi visual nyata (screenshot/browser) atas restrukturisasi ini — seluruh klaim "full-width", "satu workspace", "tidak ada dash" didasarkan pembacaan source dan CSS, bukan render sungguhan.
3. Constraint Reason wajib dan absennya endpoint pratinjau untuk 2 dari 3 mode (lihat di atas) tetap berlaku — task backend terpisah dibutuhkan bila produk menghendaki penuh sesuai spesifikasi asli.

## Amendment 14 September 2026 (lanjutan 2) — Perbaikan whitespace, placement Tambah Biaya Lain-lain, dan comparison table sizing

**Latar belakang.** Pengguna meninjau screenshot terbaru dan menemukan tiga masalah konkret sisa
dari dua amendment sebelumnya: (1) tombol toggle "Tambah Biaya Lain-lain" masih ada padahal
seharusnya dihapus dan section-nya selalu tampil langsung di bawah tabel Rincian Tagihan; (2)
jarak vertikal sangat besar sebelum Ringkasan Pembayaran; (3) pada mode perbandingan asuransi,
masing-masing tabel membutuhkan scroll horizontal sendiri di desktop.

**1. Tombol "Tambah Biaya Lain-lain" dihapus, section selalu tampil (§2-5 spesifikasi).**
`menu-pembayaran-view.jsx`: `useState` `isEditing`/`setIsEditing` **dihapus seluruhnya** (diaudit
lebih dulu — hanya dipakai untuk toggle tombol dan gating render, tidak ada dependency lain).
Tombol toggle dan baris `.secondaryActionsRow` (dibuat amendment sebelumnya) **dihapus dari
render**, bukan disembunyikan CSS. Section `Tambah Biaya Lain-Lain` (`<section
className={styles.adhocPanel}>`) sekarang **selalu dirender** tanpa kondisi apa pun, dan
**dipindah ke bawah tabel Rincian Tagihan** — sebelumnya JSX-nya justru berada DI ATAS tabel
(ditemukan saat audit: urutan lama bertentangan dengan komentar CSS `.adhocPanel` sendiri yang
sejak awal berbunyi *"Duduk tepat di bawah tabel Tagihan Pasien"*, jadi ini menyelaraskan JSX
dengan niat desain aslinya, bukan mendesain ulang). **Nol perubahan** pada form/field/validasi/
`onSubmit={confirmAdhoc}`/`handleAdhocChange`/endpoint/audit logging — hanya visibility dan
posisi.

**2. Akar penyebab whitespace ditemukan dan diperbaiki (§6-9 spesifikasi).**
Diaudit `baseStyles.tableCard` (base component bersama): membawa `margin-bottom:
var(--app-footer-safe-space, 96px)` bawaan — benar untuk elemen paling bawah halaman, tetapi
salah untuk tabel mana pun yang masih diikuti section lain.
- Menu Pembayaran: tabel Rincian Tagihan **sudah** memakai `className={styles.section}`
  (`margin-bottom: 0 !important`) sejak sebelum sesi ini — tidak ada perubahan diperlukan di situ.
  Section "Tambah Biaya Lain-Lain" yang baru dipindah juga diberi `className={styles.section}`
  yang sama, supaya jaraknya ke grid Promo/Diskon/Ringkasan konsisten.
- **Edit Tagihan: DITEMUKAN belum pernah diberi override ini sama sekali** — tabel tagihan utama
  (`edit-tagihan-view.jsx`) dan kedua tabel perbandingan asuransi (`edit-asuransi-panel.jsx`)
  masing-masing mewarisi `margin-bottom: 96px` tanpa disadari, persis akar penyebab "jarak vertikal
  sangat besar sebelum Ringkasan Pembayaran" yang dilaporkan. Kelas baru `.flushTableCard`
  (`edit-tagihan.module.css`) dibuat dan dipasang ke ketiganya via prop `className` yang memang
  sudah didukung `BillingInvoiceItemsTable` sejak awal — tidak ada API baru pada komponen.
- Tidak ditemukan `height`/`min-height`/`max-height` tetap lain pada container terkait (diaudit
  `.adhocPanel`, `.editWorkspace` [sudah dihapus], `.paymentThreeColGrid`, `.workspaceCard`,
  `.summaryCard`) selain `.itemTableScroll` (360px, sudah dinonaktifkan `scrollable={false}` pada
  amendment sebelumnya untuk Edit Tagihan) dan `.itemTableScroll` bawaan Menu Pembayaran (memang
  disengaja untuk tabel yang berbagi halaman dengan 3-kolom Pembayaran).

**3. Comparison table sizing — akar penyebab horizontal scroll ditemukan dan diperbaiki
(§13-27 spesifikasi).**
Diaudit `baseStyles.dataTable` (base component bersama, dipakai HAMPIR SELURUH tabel aplikasi):
membawa `min-width: max(840px, 100%)` — benar untuk tabel satu-kolom penuh, tetapi memaksa DUA
tabel di dalam grid 2-kolom `.comparisonGrid` melebihi lebar sel grid-nya masing-masing, persis
penyebab horizontal scroll yang dilaporkan.
- `billing-invoice-items-table.jsx`: prop opsional baru `compact` (default `false`, Menu
  Pembayaran dan tabel utama Edit Tagihan TIDAK terdampak). Saat `true`, tabel memakai kelas baru
  `.compactTable` (`billing-invoice-items-table.module.css`): `min-width: 0 !important` (melepas
  batas 840px), `table-layout: fixed; width: 100%`, padding lebih rapat (`0.5rem 0.55rem` vs
  bawaan `~0.85rem`), dan proporsi kolom via `nth-child` mengikuti panduan §16 (Deskripsi 38%,
  Satuan 9%, Status 13%, Harga Satuan 16%, Qty 7%, Harga 17%). Deskripsi (`nth-child(1)`) dan
  Satuan (`nth-child(2)`) diizinkan wrap (`white-space: normal; word-break: break-word` pada
  Deskripsi); keempat kolom lain (Status, Harga Satuan, Qty, Harga) tetap `white-space: nowrap`.
- `edit-asuransi-panel.jsx`: kedua tabel perbandingan diberi prop `compact` — **komponen dan
  logika render yang SAMA PERSIS** dengan tabel utama (§28 spesifikasi: dilarang membuat komponen
  tabel comparison terpisah; tidak ada `PrimaryInsuranceTable.jsx`/`ComparisonInsuranceTable.jsx`
  baru).
- `edit-tagihan.module.css` `.comparisonGrid`: diganti dari `repeat(auto-fit, minmax(240px,
  1fr))` (yang MEMBIARKAN lebar intrinsik tabel memaksa sel grid melebar) menjadi
  `grid-template-columns: minmax(0, 1fr) minmax(0, 1fr)` eksplisit — angka `0` pada `minmax`
  secara eksplisit mengizinkan sel mengecil di bawah lebar konten intrinsiknya (§14 spesifikasi).
  `.comparisonColumn` diberi `min-width: 0` tambahan. Breakpoint tablet `@media (max-width:
  1024px)` (pola sama dengan `.paymentThreeColGrid`) meruntuhkan grid jadi 1 kolom — pada lebar
  itu `.itemTableScroll` TIDAK dipakai (comparison table tetap `scrollable={false}`), sehingga
  scroll fallback di layar sempit berasal dari `overflow-x: auto` bawaan `baseStyles.tableWrapper`
  (§26: "scroll acceptable sebagai fallback" pada mobile/tablet), bukan mekanisme baru.
- **Bug tambahan ditemukan saat audit ini dan ikut diperbaiki:** `.numericCell { text-align:
  right }` sebelumnya diselector bersarang `.itemTableScroll .numericCell` — begitu
  `scrollable={false}` dipakai (amendment sebelumnya), kelas `.itemTableScroll` tidak lagi
  terpasang sehingga kolom Harga Satuan/Qty/Harga pada tabel utama Edit Tagihan DAN kedua tabel
  perbandingan diam-diam kembali rata kiri tanpa disadari siapa pun. Diperbaiki jadi selector
  berdiri sendiri dengan `!important` (mengalahkan `baseStyles.dataTable th { text-align: left }`
  bawaan).

**4. Edit Tagihan lain — tidak ada regresi disengaja (§30 spesifikasi).**
Struktur workspace tunggal, tabel tunggal, inline status edit, label Reason opsional (dengan
gerbang Simpan yang tetap dipertahankan), dan tidak-ada-dash dari dua amendment sebelumnya **tidak
diubah** pada amendment ini — hanya ditambah `className={styles.flushTableCard}` (kosmetik
spacing) dan prop `compact` pada tabel comparison.

**5. Add Other Cost pada Edit Tagihan (§31 spesifikasi) — TIDAK APLIKABEL.**
Diaudit: `edit-tagihan-view.jsx`, ketiga panel mode, dan hook `use-billing-invoice-edit-tagihan.js`
sama sekali tidak memiliki form/section "Tambah Biaya Lain-lain" — fitur ini eksklusif milik Menu
Pembayaran. Tidak ada duplikasi untuk dicegah, tidak ada perubahan diperlukan di Edit Tagihan
untuk butir ini.

**File yang berubah pada amendment ini:**

| File | Perubahan |
| --- | --- |
| `menu-pembayaran-view.jsx` | `isEditing`/`setIsEditing` dihapus; tombol toggle dihapus; section Tambah Biaya Lain-lain dipindah ke bawah tabel, selalu dirender |
| `menu-pembayaran.module.css` | `.secondaryActionsRow`, `.editWorkspace`, `@keyframes editWorkspaceFadeIn` dihapus (tidak dipakai lagi) |
| `edit-tagihan-view.jsx` | Tabel utama diberi `className={styles.flushTableCard}` |
| `edit-tagihan.module.css` | Kelas baru `.flushTableCard`; `.comparisonGrid` diubah ke `minmax(0,1fr)` eksplisit + breakpoint 1024px; `.comparisonColumn` diberi `min-width: 0` |
| `edit-asuransi-panel.jsx` | Kedua tabel perbandingan diberi prop `compact` dan `className={styles.flushTableCard}` |
| `billing-invoice-items-table.jsx` | Prop opsional baru `compact` (default `false`, tabel lain tidak terdampak) |
| `billing-invoice-items-table.module.css` | Kelas baru `.compactTable` + proporsi kolom; bug fix `.numericCell` (selector berdiri sendiri + `!important`) |

**Base Component Decision Gate (amendment ini):** `UI GATE: 0 elemen NEW, 0 elemen EXTEND yang
mengubah default konsumen lain, seluruh elemen REUSE.` Prop `compact` pada
`BillingInvoiceItemsTable` adalah domain component milik modul ini sendiri (bukan base component
`src/components/features/base-features/`), default `false` menjaga seluruh 3 pemanggil lama
(Menu Pembayaran, tabel utama Edit Tagihan) identik seperti sebelumnya.

**Validasi amendment ini:** **`npm run lint`/`test:unit`/`build` TIDAK dijalankan** sesuai
instruksi eksplisit pengguna yang berulang di sesi ini. Diverifikasi lewat pembacaan ulang
menyeluruh source dan CSS (tag JSX terbuka/tertutup, spesifisitas selector, prop yang dikirim
memang dikonsumsi). `git status --short` dikonfirmasi menyentuh persis 7 berkas di atas — nihil
perubahan sampingan.

- MANUAL TEST: NOT FEASIBLE — tidak ada environment ter-autentikasi/browser pada sesi ini untuk QA 6 breakpoint (1366×768, 1440×900, 1600×900, 1920×1080, 1024×768, 768px) maupun skenario long description/multiple categories/large Rupiah yang diminta §38 spesifikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

**Risiko tersisa (amendment ini):**

1. Tidak ada verifikasi visual nyata — seluruh klaim "tanpa horizontal scroll", "whitespace hilang" didasarkan audit CSS/source (spesifisitas selector, nilai `min-width`/`margin-bottom` yang saling menimpa), bukan render sungguhan di browser pada breakpoint yang diminta.
2. Proporsi kolom `.compactTable` (38/9/13/16/7/17%) adalah estimasi mengikuti panduan §16 spesifikasi (eksplisit "guideline, bukan hardcode mutlak") — belum divalidasi dengan data nyata (nama obat panjang, dsb.) yang diminta §38.
3. Bug `.numericCell` yang ditemukan (butir 3 di atas) sudah ada sejak amendment sebelumnya (saat prop `scrollable` ditambahkan) dan kemungkinan sudah terlihat di screenshot yang mendasari task ini — kini sudah ikut diperbaiki dalam amendment yang sama, tidak menunggu laporan terpisah.

## Update 12 September 2026 — pembersihan pasca `BE-BKC-FIX-009`

Delta kontrak `BE-BKC-047` yang dilaporkan di § Keputusan butir 2 (di bawah) sudah ditutup di sisi
backend lewat `BE-BKC-FIX-009` (`InvoiceEditContextResponse.Items` ditambahkan). Menyusul itu,
halaman ini **dibersihkan mengikutinya** pada sesi yang sama:

- `use-billing-invoice-edit-tagihan.js`: dispatch `getBillingInvoiceById`/`clearBillingInvoiceDetail`
  dan seluruh selector `selectBillingInvoiceDetail*` **dihapus**. Hook kini hanya memuat
  `edit-context` — satu panggilan API, bukan dua.
- `edit-tagihan-view.jsx`: sumber `rawItems` untuk tabel "Rincian tagihan" diganti dari
  `invoiceDetail.items` menjadi `editContext.items`. Tidak ada perubahan pada logika pengelompokan
  kategori, status coverage, maupun ringkasan — hanya sumber datanya yang berpindah, nilainya
  identik karena `BE-BKC-FIX-009` memakai rumus pemetaan (`MapItems`) yang sama persis dengan
  `GET /{id}` yang tadinya dipanggil di sini.

Tidak ada berkas baru maupun berkas yang dihapus dari daftar § File yang diubah/ditambah aslinya —
seluruh perubahan ini terjadi di dalam dua berkas yang memang sudah baru sejak awal task ini
(`use-billing-invoice-edit-tagihan.js`, `edit-tagihan-view.jsx`). `node --check` pada
`use-billing-invoice-edit-tagihan.js` pasca-perubahan: lulus. `npm run lint`/`test:unit`/`build`
dan klik-coba tetap **belum dijalankan**, sama seperti status sebelumnya.

## Ringkasan untuk pembaca umum

Kasir sebelumnya tidak punya cara mengganti penanggung sebuah kunjungan (dari Tunai ke Asuransi,
atau ke Penjamin Perusahaan) setelah kunjungan itu berjalan — pilihan penanggung hanya ditentukan
sekali di awal oleh Pendaftaran. Task ini menambahkan halaman baru "Edit Tagihan", dibuka lewat
tombol baru di layar Menu Pembayaran, tempat kasir dapat:

1. Melihat penanggung yang berlaku sekarang beserta rincian tagihannya.
2. Memilih penanggung kandidat (Tunai/Asuransi/Penjamin Perusahaan) dari kartu yang sudah
   terdaftar milik pasien.
3. Menekan "Bandingkan" untuk melihat berdampingan: total, porsi ditanggung, dan porsi mandiri —
   **sekarang** vs **bila diganti** — seluruh angkanya dihitung server, bukan peramban.
4. Mengisi alasan, lalu menyimpan. Tagihan dihitung ulang otomatis, dan bila ada baris biaya yang
   penanggungnya ikut kembali ke pasien, kasir diberi tahu jumlahnya.

Halaman ini juga jadi kerangka bersama untuk dua panel lain (Edit Status Tagihan, Edit Billing)
yang akan diisi task terpisah (`FE-BKC-029`/`030`) — tombolnya sudah aktif/nonaktif mengikuti hak
akses sungguhan dari server, tapi isi panelnya menyusul.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-068`, `070`–`072`; `MPY-DEC-003`, `MPY-DES-015`, `MPY-DES-017`.
`frontend-roadmap.md` § `FE-BKC-028`. `03-frontend-architecture.md` § "Amendment 11 September
2026" (wireframe, tabel Wilayah, Kewenangan UI, Acceptance frontend `60`–`70`).

**Temuan yang wajib dilaporkan (bukan `DEV_DISCRETION`, bukti dari source):**

1. **Tombol "Edit Tagihan" yang sudah ada di Menu Pembayaran ternyata untuk fitur lain.**
   Diperiksa langsung: tombol itu (`menu-pembayaran-view.jsx`, sekitar baris 654 sebelum task ini)
   men-toggle form inline "Tambah Biaya Lain-Lain" — bukan navigasi. Sesuai
   `03-frontend-architecture.md` § "Pemakaian ulang komponen" ("Tidak dipakai ulang. Rumpun ini
   menambahkan jalan masuk tersendiri"), jalan masuk baru diberi label **berbeda**, "🔀 Edit
   Penanggung", supaya dua tombol dengan tujuan berlainan tidak terlihat sama. Tombol lama
   **tidak diubah** (di luar scope).
2. **Delta kontrak `BE-BKC-047` (dilaporkan, bukan ditebak):** `InvoiceEditContextResponse`
   didokumentasikan sebagai *"seluruh bahan layar dalam satu panggilan"*, tetapi DTO
   sesungguhnya (`BillingPayerEditDtos.cs`) **tidak membawa** deskripsi/satuan/harga satuan/qty/
   nama kategori per baris — hanya `CalculationItemResponse` (`CategoryId`/`CategoryCode`/
   `GrossAmount`/`NetAmount`, dipakai murni untuk status coverage) dan
   `ItemPayerAssignmentResponse` (`ItemName`/`PayerKind`/`Amount`, tanpa satuan/harga/qty). Field
   yang dibutuhkan tabel "Rincian tagihan" pada wireframe **hanya ada** di `InvoiceItemResponse`
   milik `GET /{id}` (endpoint lama, sudah dipakai Menu Pembayaran). Daripada menebak bentuk
   payload `edit-context` yang belum ada atau mengubah backend diam-diam, halaman ini memanggil
   **kedua** endpoint yang sudah nyata ada — persis pola `use-billing-invoice-detail.js`/
   `use-menu-pembayaran.js` yang juga memisahkan `invoiceDetail` (item mentah) dari `calculation`
   (breakdown). Tidak ada perubahan backend. **Layak disosialisasikan ke Backend/API Owner**
   sebagai gap dokumentasi `BE-BKC-047` untuk rumpun task berikutnya yang memakai endpoint ini.
3. **Label "Subtotal Penjamin" vs "Subtotal Asuransi" (Acceptance `#67`) memakai
   `calculation.breakdown.payerKind`**, bukan `currentPayer.paymentType`. Diperiksa:
   `CalculationBreakdownResponse.PayerKind` (`BillingInvoiceDtos.cs`, komentar "BE-BKC-044/
   MPY-DES-017") memang dirancang persis untuk kebutuhan label ini — lebih otoritatif daripada
   menyimpulkannya sendiri dari field lain.
4. **Kartu penjamin kandidat memakai bentuk daftar kartu (`BaseSavedPayerCard`), bukan
   `<select>`** walau sketsa ASCII wireframe menunjukkan `[ pilih kartu... v]`. Alasan: (a)
   Acceptance `#61` eksplisit berbunyi *"tampil **pada daftar** dalam keadaan nonaktif"*, (b)
   `03-frontend-architecture.md` § "Kewenangan UI" menandai bentuk kontrol sebagai
   `DEV_DISCRETION`, dan (c) `BaseSavedPayerCard` sudah mendukung `disabled`+alasan per kartu
   secara native — pola yang sudah ada, bukan komponen baru.
5. **Panel `FE-MPY-03` (Edit Status Tagihan) dan `FE-MPY-04` (Edit Billing) sengaja belum
   diisi.** Keduanya milik `FE-BKC-029`/`FE-BKC-030` yang belum dikerjakan. Toolbar tiga mode
   tetap dibangun penuh dan nonaktif/aktifnya **sungguhan** mengikuti `capabilities` dari
   `edit-context` (bukan hardcode) — begitu kedua task itu selesai, mereka tinggal mengisi
   komponen panel di slot yang sudah ada (`activeMode === INVOICE_EDIT_MODES.ITEM_STATUS`/
   `DRUG_BILLING` pada `edit-tagihan-view.jsx`), tidak perlu membongkar kerangka.
6. **Konfirmasi sebelum simpan memakai `ConfirmModal` generik** (`base-features/confirm-modal.jsx`),
   bukan modal baru. Alasannya, "Alasan perubahan" sudah diisi di dalam panel (sesuai wireframe
   `FE-MPY-02`), sehingga modal konfirmasi hanya perlu menampilkan kalimat template yang
   **mengikat**: *"Gunakan &lt;nama penjamin&gt; sebagai penanggung kunjungan ini? Tagihan akan
   dihitung ulang."* — persis kalimat pada `03-frontend-architecture.md` § `FE-MPY-02`, tanpa
   field tambahan.
7. **Tabel "Rincian tagihan" dan fungsi turunan status coverage per baris diekstrak** dari
   `menu-pembayaran-view.jsx` ke lokasi bersama, sesuai instruksi eksplisit reuse pada arsitektur
   (§ "Pemakaian ulang komponen": *"Ekstrak bagian presentasionalnya... MUST NOT menyalin seluruh
   berkas"*). Perilaku Menu Pembayaran **tidak berubah** — lihat § File yang diubah.

## Proses bisnis

**Tujuan:** kasir mengganti penanggung kunjungan (payer) setelah kunjungan berjalan, dengan angka
dampaknya terlihat dulu sebelum disimpan.

**Pelaku:** Kasir dan Kepala Kasir/Finance Operations — kewenangan sama persis
(`03-frontend-architecture.md` § "Aksi per peran": tidak ada jenjang approval kedua).

**Pemicu:** Kasir menekan "🔀 Edit Penanggung" pada Menu Pembayaran untuk satu tagihan tertentu.

**Prasyarat:** Tagihan berstatus dapat diedit (`capabilities.canEditPaymentSource` bernilai
benar); pasien punya minimal satu kartu asuransi/penjamin perusahaan terdaftar bila kandidatnya
bukan Tunai.

**Langkah utama:**

1. Halaman memuat konteks edit (`GET /edit-context`) dan detail invoice (`GET /{id}`) sekaligus.
2. Kasir memilih kategori kandidat (Tunai/Asuransi/Penjamin Perusahaan).
3. Bila bukan Tunai, kasir memilih satu kartu dari daftar kartu tersimpan pasien untuk kategori
   itu. Kartu yang tidak berlaku (kedaluwarsa, dsb.) tetap tampil tapi nonaktif beserta alasannya.
4. Kasir menekan "Bandingkan" (`POST /payer-comparison-preview`, murni baca, tanpa efek samping)
   dan melihat angka Total/Ditanggung/Mandiri berdampingan — sisi sekarang vs sisi kandidat.
5. Kasir mengisi "Alasan perubahan" (wajib).
6. Kasir menekan "Simpan Perubahan" → muncul konfirmasi → kasir menekan "Ya, Ganti Penanggung".
7. Sistem mengganti penanggung dan menghitung ulang tagihan secara atomik
   (`PUT /payment-source`). Berhasil → notifikasi sukses (plus jumlah baris yang penanggungnya
   ikut dikembalikan ke pasien bila ada) dan halaman memuat ulang data terbaru.

**Aturan bisnis:**

- Seluruh angka perbandingan dan hasil akhir berasal dari server — frontend tidak menghitung
  selisih apa pun sendiri (`MUST NOT`, § `FE-MPY-02`).
- Memilih penanggung yang sama persis dengan yang sedang berlaku ditolak server (pesan
  "Penanggung yang dipilih sama dengan yang sedang dipakai...") — frontend menampilkan pesan itu
  apa adanya, tidak mengarang kalimat sendiri.
- Tombol "Edit Penjamin Perusahaan" **tidak dibuat** — perubahan kartu penjamin pasien tetap
  lewat Data Pasien/Registrasi (`MPY-DEC-003`).

**Perubahan status:** tidak ada status invoice baru (`MPY-DEC-001`: satu payer aktif per
kunjungan tetap dipertahankan). Yang berubah hanyalah **field penanggung** kunjungan
(`RegPatientEncounterGuarantor`, dimiliki `RegistrationManagement`, diperbarui lewat
`BillingPayerEditService`).

| Dari penanggung | Tindakan | Ke penanggung | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| Tunai/Asuransi/Penjamin | Ganti penanggung | Tunai | Kasir/Kepala Kasir | Alasan wajib diisi |
| Tunai/Asuransi/Penjamin | Ganti penanggung | Asuransi | Kasir/Kepala Kasir | Kartu asuransi terdaftar dan berlaku; alasan wajib |
| Tunai/Asuransi/Penjamin | Ganti penanggung | Penjamin Perusahaan | Kasir/Kepala Kasir | Kartu penjamin terdaftar dan berlaku; alasan wajib |

**Jalur tidak normal:**

- Data tagihan sudah berubah sejak dimuat (`409`) → pesan **mengikat**: *"Data tagihan telah
  berubah. Muat ulang data sebelum menyimpan kembali."*, data dimuat ulang otomatis.
- Ditolak aturan bisnis (`422`, mis. penanggung sama) → pesan server ditampilkan apa adanya.
- Kartu kandidat sudah tidak berlaku → kartu tetap terlihat, nonaktif beserta alasannya (bukan
  hilang).
- Tidak berwenang (`403`) → gerbang akses yang sudah ada di seluruh aplikasi (`InstanceAxios`
  interceptor, `notifyAccessDenied`), tidak ada penanganan baru yang ditulis.

**Hasil akhir:** penanggung kunjungan berubah, tagihan berisi angka hasil perhitungan terbaru,
dan baris biaya yang penanggung lamanya tidak lagi tersedia otomatis kembali ke tanggungan
pasien (`AUTO`, dengan alasan otomatis dari server).

## Base Component Decision Gate

`UI GATE: 0 elemen NEW, 0 elemen EXTEND yang mengubah default konsumen lain, seluruh elemen REUSE`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Tombol, alert, toast, modal konfirmasi | `REUSE` | `BaseButton`, `InformationAlert`, `ToastStack`, `ConfirmModal` dari `base-features/` dipakai apa adanya |
| Kategori kandidat (Tunai/Asuransi/Penjamin Perusahaan) | `REUSE` | `BasePayerCategorySelector` (`base-payer-workspace.jsx`) — diberi 3 kategori, bukan komponen baru |
| Kartu kandidat + status nonaktif+alasan | `REUSE` | `BaseSavedPayerCard` dipakai langsung, `disabled`+`statusLabel` sudah didukung native |
| Isian alasan | `REUSE` | `BaseTextAreaField` (`base-form-control.jsx`) |
| Gerbang akses halaman | `REUSE` | `AccessDeniedGate`, pola identik `menu-pembayaran-view.jsx` |
| `BasePayerWorkspace` (komposit) | **TIDAK dipakai** | Tata letak `FE-MPY-02` (satu kolom: banner→kategori→kartu→bandingkan→alasan) berbeda dari tata letak dua-kolom komposit ini (daftar tersimpan di kiri, editor di kanan) yang dirancang untuk konteks Rawat Inap. Dipakai **sebagian**: hanya dua dari enam ekspornya (`BasePayerCategorySelector`, `BaseSavedPayerCard`), bukan komposit utuh. **Nol perubahan** pada `base-payer-workspace.jsx` sendiri — satu-satunya konsumen existing (langkah pembayaran admisi Rawat Inap) sama sekali tidak tersentuh |
| Tabel rincian tagihan berkelompok | `REUSE (diekstrak)` | Diambil dari `menu-pembayaran-view.jsx`, dipakai ulang persis di kedua halaman — lihat § File yang diubah |

Tidak ada komponen dasar baru yang dibuat. Tidak ada gerbang keputusan komponen yang menunggu
pengguna.

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/edit-context` | Seluruh bahan layar Edit Tagihan (kepala tagihan, kalkulasi, penanggung sekarang, pilihan kartu, kapabilitas edit) | `BillingInvoice : Read` | `-` | `InvoiceEditContextResponse` |
| `POST` | `/{id}/payer-comparison-preview` | Pratinjau perbandingan angka bila penanggung diganti — 100% baca, tanpa efek samping | `BillingInvoice : Read` | Body: kategori+kartu kandidat | `PayerComparisonPreviewResponse` |
| `PUT` | `/{id}/payment-source` | Mengganti penanggung kunjungan yang berlaku dan menghitung ulang tagihan secara atomik | `BillingInvoice : Update` | Body: penanggung baru, `ExpectedRowVersion`, alasan; header `Idempotency-Key` | `InvoiceEditResultResponse` |
| `GET` | `/{id}` | **Dipakai ulang** (bukan endpoint baru) — sumber deskripsi/satuan/harga satuan/qty/kategori tabel rincian, lihat § Keputusan butir 2 | `BillingInvoice : Read` | `-` | `InvoiceDetailResponse` |

Kode status yang mungkin muncul: `404` (tagihan tidak ditemukan/tidak aktif), `400` (permintaan
tidak valid, mis. kandidat sama dengan yang berlaku), `409` (data sudah berubah sejak dimuat —
kasir diminta muat ulang), `422` (ditolak aturan bisnis, pesan server ditampilkan apa adanya),
`403` (kasir tidak berwenang untuk tindakan ini).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/app/.../billing/invoices/[slug]/edit-tagihan/page.jsx` | **Baru.** Route tipis — decode token, render view, pola identik `[slug]/pembayaran/page.jsx` |
| `.../billing-invoices/edit-tagihan/edit-tagihan-view.jsx` | **Baru.** Kerangka `FE-MPY-01` — header, toolbar tiga mode (aktif/nonaktif dari `capabilities`), switch panel aktif, tabel rincian tagihan, ringkasan |
| `.../billing-invoices/edit-tagihan/edit-asuransi-panel.jsx` | **Baru.** Panel `FE-MPY-02` penuh |
| `.../billing-invoices/billing-invoice-items-table.jsx` | **Baru.** Tabel rincian berkelompok, diekstrak dari `menu-pembayaran-view.jsx`, dipakai kedua halaman |
| `src/lib/hooks/.../billing-invoices/use-billing-invoice-edit-tagihan.js` | **Baru.** Hook halaman — resolusi token privat, muat `edit-context`+`GET /{id}` paralel, mode toolbar, penanganan `409` |
| `.../use-edit-asuransi-panel.js` | **Baru.** Hook panel — pilih kategori/kartu, bandingkan, konfirmasi, simpan, `Idempotency-Key`/`CorrelationId` dibangkitkan sekali saat modal konfirmasi dibuka |
| `.../billing-invoice-calculation-breakdown.js` | **Diubah** (milik `FE-BKC-022`). Ditambah `buildItemOutcomeById`/`resolveItemCoverageStatus`/`resolveItemCoPayAmount` — logika yang tadinya inline di `menu-pembayaran-view.jsx`, sekarang dipakai dua halaman. `computeBillingCalculationBreakdown` (existing) **tidak diubah** |
| `.../billing-invoice-constants.js` | **Diubah.** Tambah `BILLING_INVOICE_ROUTES.editTagihan`, `INVOICE_PAYER_TYPES`/`_LABELS`, `INVOICE_EDIT_MODES`/`_OPTIONS`. Seluruh export lama **tidak diubah** |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | **Diubah.** Tambah 3 thunk (`getInvoiceEditContext`, `previewPayerComparison`, `switchInvoicePaymentSource`), state+reducer+selector pasangannya, 2 reducer clear baru. Thunk/selector lama **tidak diubah** |
| `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` | **Diubah.** (a) Tabel rincian tagihan diganti pemanggilan `BillingInvoiceItemsTable` (perilaku identik); (b) `itemOutcomeById`/`getItemCoverageStatus`/`getItemCoPayInfo` lokal diganti pemanggilan util bersama (perilaku identik); (c) tombol baru "🔀 Edit Penanggung" ditambahkan bersebelahan dengan tombol "✏️ Edit Tagihan" lama (tidak diubah). **Catatan:** berkas ini sudah punya perubahan belum ter-commit dari `FE-BKC-FIX-008` sebelum task ini dimulai (tercatat `blueprint-manifest.md`) — perubahan itu dipertahankan apa adanya, tidak ditimpa |
| `src/style/.../menu-pembayaran.module.css` | **Diubah.** Tambah `.tagihanHeaderActions` (pembungkus flex dua tombol) |
| `src/style/.../billing-invoice-items-table.module.css` | **Baru.** Style tabel yang diekstrak |
| `src/style/.../edit-tagihan.module.css` | **Baru.** Style halaman dan panel `FE-MPY-02` |

Total: **8 berkas baru, 7 berkas diubah** (2 di antaranya sudah punya perubahan tertunda dari task
lain sebelum sesi ini, dipertahankan).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` | **SKIPPED** — instruksi baku pengguna (build/test frontend dijalankan manual sendiri) | Tidak dijalankan sesi ini |
| `npm run test:unit` | **SKIPPED** — sama seperti di atas | Tidak dijalankan sesi ini |
| `npm run build` | **SKIPPED** — sama seperti di atas | Tidak dijalankan sesi ini |
| Sintaks berkas logika non-JSX valid | **LULUS** | `node --check` pada `billing-invoice-calculation-breakdown.js`, `billing-invoice-constants.js`, `use-billing-invoice-edit-tagihan.js`, `use-edit-asuransi-panel.js`, dan `billing-invoice-slice.jsx` (via salinan `.js` sementara karena `node --check` menolak ekstensi `.jsx`) — kelimanya `exit 0`, nol galat sintaks |
| Sintaks berkas JSX baru | **BELUM DIVERIFIKASI ALAT** | `node --check` tidak dapat mem-parse JSX; tidak ada `eslint`/build dijalankan (lihat baris SKIPPED di atas). Diverifikasi manual lewat pembacaan ulang menyeluruh (tag terbuka/tertutup, import dipakai) |
| Ketiga endpoint `BE-BKC-047` dipakai | **LULUS (tinjauan kode)** | Dicocokkan satu-satu terhadap `BillingInvoicesController.cs` baris 482–575 — lihat tabel Endpoint |
| Nol perhitungan finansial di peramban | **LULUS (tinjauan kode)** | `edit-asuransi-panel.jsx` hanya merender field dari `comparison`/`result` response, tidak ada operator aritmetika pada nilai uang di file ini maupun `use-edit-asuransi-panel.js` |
| Regresi `BasePayerWorkspace`/admisi Rawat Inap | **LULUS (tinjauan kode)** | `base-payer-workspace.jsx` **tidak disentuh sama sekali** — dikonfirmasi lewat `git status --short` (nihil perubahan pada berkas itu); task ini hanya mengimpor dua ekspor bernamanya |
| `git status --short` (frontend) | **Dilaporkan** | 5 berkas `M`, 6 entri `??` (4 berkas + 2 folder baru) — persis daftar pada § File yang diubah, tidak ada berkas lain yang tersentuh |
| Verifikasi manual (klik-coba: ganti Tunai→Asuransi, kartu kedaluwarsa nonaktif, bandingkan, simpan, notifikasi baris ter-reset, tolakan `409`/`422`) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi/browser pada sesi ini — pola yang sama seperti seluruh task frontend rumpun ini sebelumnya |

**Task ini belum bisa ditandai selesai.** Source untuk `FE-MPY-01` (kerangka) dan `FE-MPY-02`
selesai dan ditinjau kode secara menyeluruh terhadap kontrak backend nyata, tetapi lint/test/build
serta klik-coba ter-autentikasi belum dijalankan sesi ini.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna) — lint/test:unit/build menunggu dijalankan pengguna sendiri

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat `npm run lint`/`test:unit`/`build` maupun browser** pada
   sesi ini — murni tinjauan kode terhadap kontrak backend nyata dan pola frontend existing.
2. ~~**Delta kontrak `BE-BKC-047`** belum disosialisasikan formal ke pemilik arsitektur backend.~~
   **RESOLVED 12 September 2026** — ditutup `BE-BKC-FIX-009` (`InvoiceEditContextResponse.Items`
   ditambahkan); halaman ini sudah dibersihkan mengikutinya (lihat § "Update 12 September 2026" di
   atas). Panggilan `GET /{id}` yang terpisah sudah dihapus, bukan lagi risiko yang tersisa.
3. ~~**Panel `FE-MPY-03`/`FE-MPY-04` masih placeholder**~~ **RESOLVED** — `FE-BKC-029`/`030`
   menyelesaikan keduanya, dan amendment 14 September 2026 di atas menggabungkan ketiga panel
   menjadi satu tabel tunggal. Risiko baru pengganti: lihat butir 6 dan 7 di bawah.
6. **Reason tetap wajib walau spesifikasi UI meminta opsional** (lihat § Amendment 14 September
   2026, constraint 1) — kasir tidak bisa menyimpan tanpa mengisi alasan pada ketiga mode, sampai
   ada task backend terpisah yang melonggarkan `BIL-VAL-062`.
7. **Ringkasan Pembayaran tidak reaktif-per-draft untuk Edit Status Tagihan dan Edit Billing**
   (lihat § Amendment 14 September 2026, constraint 2) — kasir baru melihat angka baru setelah
   menekan Simpan, bukan saat masih mengubah draft. Membutuhkan endpoint pratinjau baru di backend
   bila produk menghendaki perilaku reaktif penuh seperti Edit Asuransi.
4. **`menu-pembayaran-view.jsx` dan `billing-invoice-constants.js` sudah punya perubahan tertunda
   dari `FE-BKC-FIX-008`** sebelum task ini dimulai (tercatat `blueprint-manifest.md`,
   *"BELUM di-commit dan BELUM pernah dibangun sekalipun"*). Task ini menumpuk perubahan baru di
   atasnya tanpa memverifikasi ulang perubahan lama tersebut — risiko gabungan keduanya belum
   pernah lolos satu `build` pun.
5. **Kolom `ExcessAmount`/`DataAnomalyAmount` pada `PayerComparisonSummaryResponse` tidak
   ditampilkan** di blok perbandingan — hanya Total/Ditanggung/Mandiri sesuai wireframe
   `FE-MPY-02` yang eksplisit tidak menyebut baris lain. Bila kunjungan punya anomali data,
   perbandingan yang terlihat kasir tidak menunjukkannya secara eksplisit (nilainya tetap
   terhitung benar di baliknya, hanya tidak dirender terpisah).

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build`, lalu klik-coba manual (ganti
   penanggung ketiga jenis, kartu kedaluwarsa, tolakan `409`/`422`, regresi langkah pembayaran
   admisi Rawat Inap) sendiri.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-028`) dan
   `requirement-traceability.md`, tandai `✅`, tautkan laporan ini.
3. Sosialisasikan delta kontrak `BE-BKC-047` (§ Keputusan butir 2) ke Backend/API Owner —
   pertimbangkan menambah field item mentah ke `InvoiceEditContextResponse` pada revisi
   berikutnya supaya `edit-context` benar-benar cukup sendiri seperti niat desainnya.
4. Lanjutkan `FE-BKC-029` (Edit Status Tagihan) dan `FE-BKC-030` (Edit Billing) — keduanya tinggal
   mengisi komponen panel pada slot yang sudah disiapkan `edit-tagihan-view.jsx`.
