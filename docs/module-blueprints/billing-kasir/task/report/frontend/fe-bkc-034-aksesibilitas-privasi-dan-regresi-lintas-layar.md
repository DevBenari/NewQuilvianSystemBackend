# FE-BKC-034 — Aksesibilitas, Privasi, dan Regresi Lintas Layar

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-034` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`); rumpun `FE-MPY-01` s.d. `05` (`FE-BKC-028`–`031`) |
| Task type | **Audit dan verifikasi lintas layar** — bukan pembuatan fitur baru. Penelusuran papan ketik, pemeriksaan nol UUID, pemeriksaan privasi nomor polis/karyawan, dan regresi terhadap kode bersama yang dipakai ulang |
| Task mode | `FRONTEND` (audit source, tanpa perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-029` ✅ (source), `FE-BKC-030` ✅ (source), `FE-BKC-031` ✅ (source) — seluruhnya `SOURCE_DONE_PENDING_MANUAL_VERIFICATION`, diaudit dari kondisi terkini di working tree |
| Status task | **Audit selesai, satu regresi ditemukan dan diperbaiki.** `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna). Penelusuran papan ketik dan klik-coba browser sungguhan **NOT FEASIBLE** sesi ini (tidak ada environment ter-autentikasi) — audit dilakukan lewat tinjauan kode dan `git diff` menyeluruh terhadap setiap berkas yang tersentuh rumpun ini |

## Ringkasan untuk pembaca umum

Task ini tidak membangun layar baru — ia memeriksa EMPAT layar yang sudah dibangun rumpun ini
(kerangka Edit Tagihan beserta tiga panelnya, dan tab baru di Dokumen Kasir) untuk empat hal:
bisa dipakai dengan papan ketik saja, tidak pernah menampilkan ID mentah (UUID) di layar atau URL,
tidak membocorkan nomor polis/nomor karyawan di tempat yang tidak semestinya, dan **tidak merusak
layar lain** yang kebetulan berbagi komponen atau berkas dengan rumpun ini — khususnya langkah
pembayaran admisi Rawat Inap yang berbagi satu komponen (`BasePayerWorkspace`), serta Menu
Pembayaran dan Dokumen Kasir yang berkasnya langsung diubah/diperluas.

Karena seluruh pekerjaan rumpun ini masih ada di working tree yang sama (belum di-commit), audit
regresi dilakukan dengan cara yang jauh lebih akurat daripada menghafal source lama: `git diff`
terhadap setiap berkas yang tersentuh, dibandingkan baris demi baris dengan versi yang sudah
ter-commit sebelumnya. Ini menemukan **satu regresi nyata** pada tampilan Menu Pembayaran, yang
sudah diperbaiki dalam sesi ini juga (lihat temuan 5).

## Metodologi

Karena task ini murni audit, tidak ada "keputusan yang mengunci scope" seperti task pembuatan
fitur. Metodologi yang dipakai:

1. `git status`/`git diff` pada `QuilvianSystemFrontendDev` untuk mendapat daftar PASTI setiap
   berkas pra-eksisting yang tersentuh rumpun `FE-BKC-028`–`031`, dan berapa banyak baris yang
   benar-benar dihapus/diubah (bukan sekadar ditambah) pada masing-masing.
2. Pembacaan penuh source code keempat layar (`edit-tagihan-view.jsx`, `edit-asuransi-panel.jsx`,
   `edit-status-tagihan-panel.jsx`, `edit-billing-panel.jsx`) beserta dokumen cetak baru
   (`company-guarantor-invoice-document.jsx`) untuk memeriksa pola interaksi, elemen yang
   menangani klik, dan field apa saja yang dirender.
3. Penelusuran `grep` bertarget untuk pola berisiko: `<div>`/`<span>`/`<td>` dengan `onClick`
   langsung (indikasi elemen tidak dapat dijangkau papan ketik), interpolasi field mirip-ID ke
   dalam string yang dirender pengguna, dan pemakaian `policyNumber`/`employeeNumber`.
4. Untuk `BasePayerWorkspace` dan konsumen aslinya (`inpatient-admission-payment-step.jsx`):
   diperiksa langsung lewat `git status` apakah keduanya ada dalam daftar berkas yang diubah sama
   sekali.

## Temuan

### 1. Penelusuran papan ketik (keempat layar)

**Hasil: tidak ditemukan elemen interaktif yang tidak dapat dijangkau papan ketik.**

- Ditelusuri seluruh JSX `edit-tagihan-view.jsx`, `edit-asuransi-panel.jsx`,
  `edit-status-tagihan-panel.jsx`, `edit-billing-panel.jsx` untuk pola `<div>`/`<span>`/`<td>`/
  `<tr>`/`<li>` dengan `onClick` langsung — **nihil**. Setiap kontrol interaktif memakai elemen
  asli yang otomatis dapat difokus dan diaktifkan papan ketik: `BaseButton` (yang merender
  `<button>` sungguhan), `BasePayerCategorySelector`/`BaseSavedPayerCard` (keduanya juga
  `<button type="button">` asli, dikonfirmasi dari source `base-payer-workspace.jsx`), dan
  `<input type="checkbox">` mentah pada `EditBillingPanel` (sudah dilaporkan sebagai keputusan
  sadar pada laporan `FE-BKC-030`).
- **Catatan kelengkapan ARIA (bukan pemblokir fungsional):** beberapa wadah memakai
  `role="radiogroup"`/`role="tablist"` (mode toolbar Edit Tagihan, pilihan penanggung per baris
  di Edit Status Tagihan, pilihan mode Edit Billing) tetapi anak-anaknya adalah `<button>` biasa,
  bukan `role="radio"`/`role="tab"` dengan roving tabindex. Secara fungsional setiap pilihan tetap
  dapat dijangkau Tab dan diaktifkan Enter/Space satu per satu — bukan jebakan papan ketik — hanya
  saja pembaca layar tidak mendapat perilaku navigasi panah kiri/kanan standar sebuah radiogroup.
  Pola ini SAMA PERSIS dengan yang sudah dipakai `BasePayerCategorySelector` bawaan sebelum rumpun
  ini (bukan regresi baru), sehingga tidak diubah di sini — perbaikannya (bila diinginkan) adalah
  keputusan desain terpisah yang menyentuh komponen bersama.
- **NOT FEASIBLE**: urutan Tab yang sesungguhnya di browser (mis. apakah fokus terperangkap di
  dalam modal `ConfirmModal`, atau urutan visual vs. urutan DOM konsisten) tidak dapat diverifikasi
  tanpa environment ter-autentikasi pada sesi ini.

### 2. Nol UUID tampil di layar maupun URL

**Hasil: tidak ditemukan pelanggaran.**

- Digrep setiap pemakaian `.id`/`.Id`/`invoiceItemId` pada keempat layar dan dokumen cetak baru:
  seluruhnya dipakai HANYA sebagai React `key`, parameter pemanggilan thunk/hook, atau nilai yang
  dikirim ke server — tidak satu pun dirender sebagai teks yang terlihat pengguna.
  `invoiceId`/`resolvedId` pada hook editor juga hanya dipakai untuk pemanggilan API, tidak pernah
  disisipkan ke string yang ditampilkan (toast, judul, label).
- Route URL Edit Tagihan (`/.../invoices/[slug]/edit-tagihan`) memakai `invoiceRouteToken` yang
  sama dengan token privat yang sudah dipakai halaman detail/menu pembayaran invoice yang sama
  (`registerPrivateRouteToken`/`resolvePrivateRouteToken`) — tidak ada UUID mentah baru yang
  muncul di URL.

### 3. Nomor polis dan nomor karyawan tidak tampil di tempat yang tidak semestinya

**Hasil: kedua tempat kemunculannya sudah diperiksa dan sesuai konteks yang semestinya — bukan
kebocoran baru.**

- **`edit-asuransi-panel.jsx`** merender `policyNumber`/`employeeNumber` di dalam kartu pemilihan
  penanggung (`BaseSavedPayerCard`, prop `numberValue`) — tempat ini SEMESTINYA menampilkannya:
  kasir perlu membedakan kartu asuransi/rute penjamin mana yang dipilih ketika pasien punya lebih
  dari satu. Dikonfirmasi ini BUKAN pola baru: `base-payer-workspace.jsx` sendiri (baris 191-198)
  sudah memakai `numberValue={item?.policyNumber || item?.employeeNumber || ...}` tanpa masking
  sejak sebelum rumpun ini — panel baru ini murni memakai ulang komponen dan polanya apa adanya.
- **`company-guarantor-invoice-document.jsx`** merender `employeeNumber`/`employeeName` — ini
  ADALAH tujuan dokumen ini (dokumen tagihan resmi ke perusahaan penjamin secara wajar perlu
  menyebutkan karyawan mana yang ditagihkan), sesuai `03-frontend-architecture.md` §
  "Skema fitur" yang eksplisit meminta "ditambah identitas karyawan", dan mengikuti bentuk
  `InvoiceAsuransiDocument` yang sudah ada (yang juga menampilkan nomor polis tanpa masking).
- **Catatan untuk pemilik terpisah (bukan sesuatu yang diubah task ini):** panduan umum
  "Accessibility dan privacy" pada `03-frontend-architecture.md` baris 72 menyebut "Mask nomor
  identitas/provider". Baik `BaseSavedPayerCard` maupun `InvoiceAsuransiDocument`/
  `CompanyGuarantorInvoiceDocument` TIDAK menerapkan masking ini — tetapi ini adalah karakteristik
  yang sudah ada SEBELUM rumpun `FE-BKC-028`–`031` (pada `BaseSavedPayerCard`) atau sengaja
  meniru pola yang sudah ada (`InvoiceAsuransiDocument`). Task ini TIDAK mengubah
  `base-payer-workspace.jsx` untuk menambah masking, karena itu komponen bersama dengan **tepat
  satu** konsumen lain (`MPY-CQ-02`) dan menambah masking di sana adalah keputusan desain yang
  mengubah tampilan konsumen ASLI juga — di luar wewenang audit ini, dan berlawanan dengan
  Acceptance `#70` yang justru menuntut layar itu **tidak berubah** perilakunya.

### 4. Regresi langkah pembayaran admisi Rawat Inap (Acceptance `#70`)

**Hasil: terbukti benar secara konstruksi — risiko regresi nol.**

`git status` pada `QuilvianSystemFrontendDev` dikonfirmasi TIDAK menyertakan
`src/components/features/base-features/base-payer-workspace.jsx` maupun
`src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx`
(konsumen asli, satu-satunya, sesuai `MPY-CQ-02`) dalam daftar berkas yang diubah rumpun ini.
Kedua berkas itu memiliki **nol baris yang tersentuh**. `EditAsuransiPanel` (`FE-BKC-028`) memakai
`BasePayerCategorySelector`/`BaseSavedPayerCard` yang diekspor komponen itu **apa adanya**, tanpa
menambah prop atau memodifikasi definisinya sama sekali. Karena komponen bersama dan konsumen
aslinya sama-sama tidak tersentuh, Acceptance `#70` ("Langkah pembayaran pada admisi Rawat Inap
tetap berperilaku sama persis sesudah `BasePayerWorkspace` dipakai ulang") terpenuhi bukan karena
diverifikasi lewat klik-coba, melainkan karena regresi itu **mustahil secara konstruksi** — tidak
ada satu karakter pun dari jalur kode langkah pembayaran Rawat Inap yang berubah.

### 5. Regresi Menu Pembayaran dan Dokumen Kasir

**Hasil: satu regresi nyata ditemukan dan DIPERBAIKI dalam sesi ini; sisanya terbukti aman.**

Sepuluh berkas pra-eksisting tersentuh rumpun `FE-BKC-028`–`031` (dari `git status`):
`dokumen-kasir-view.jsx`, `menu-pembayaran-view.jsx`, `billing-invoice-calculation-breakdown.js`,
`billing-invoice-constants.js`, `use-dokumen-kasir-page.js`, `use-dokumen-kasir.js`,
`billing-invoice-slice.jsx`, `store.jsx`, `menu-pembayaran.module.css`, `menu-items.jsx`.

Diperiksa `git diff` tiap berkas dan dihitung baris yang BENAR-BENAR dihapus/diubah (`grep
'^-[^-]'`, bukan sekadar ditambah):

| Berkas | Baris dihapus/diubah | Status |
| --- | --- | --- |
| `dokumen-kasir-view.jsx` | 0 | Murni tambahan (blok tab baru) — aman |
| `billing-invoice-calculation-breakdown.js` | 0 | Murni tambahan (fungsi baru) — aman |
| `use-dokumen-kasir-page.js` | 0 | Murni tambahan (selector/effect baru) — aman |
| `use-dokumen-kasir.js` | 0 | Murni tambahan (callback baru) — aman |
| `billing-invoice-slice.jsx` | 0 (dari 331 baris ditambahkan) | Murni tambahan (9 thunk/reducer baru) — aman |
| `store.jsx` | 0 | Murni tambahan (registrasi reducer) — aman |
| `menu-items.jsx` | 0 | Murni tambahan (entri menu) — aman |
| `menu-pembayaran.module.css` | 0 | Murni tambahan (kelas CSS baru) — aman |
| `billing-invoice-constants.js` | Array `DOKUMEN_KASIR_REAL_TABS` diperluas (bukan dihapus-ulang); dikonfirmasi HANYA dikonsumsi lewat `Set` (`DOKUMEN_KASIR_RECOGNIZED_TABS`), bukan indeks/panjang — aman | Aman setelah diverifikasi cara pakainya |
| `menu-pembayaran-view.jsx` | 183 baris berubah (ekstraksi tabel tagihan + logika status coverage) | **Ditemukan 1 regresi, sudah diperbaiki** — lihat di bawah |

**Regresi yang ditemukan:** ekstraksi tabel rincian tagihan Menu Pembayaran ke
`billing-invoice-items-table.jsx` (dipakai ulang oleh Edit Tagihan) tidak menyertakan kelas CSS
`styles.section` (`margin-bottom: 0 !important`) yang sebelumnya membuat tabel itu menempel rapat
dengan grid 3-kolom Pembayaran tepat di bawahnya. Tanpa kelas itu, tabel hasil ekstraksi mewarisi
`margin-bottom: var(--app-footer-safe-space, 96px)` bawaan `baseStyles.tableCard` — membuka jarak
96px yang **tidak pernah ada sebelum ekstraksi ini**, murni regresi visual pada Menu Pembayaran.
Logika perhitungan status coverage/co-payment (`buildItemOutcomeById`/`resolveItemCoverageStatus`/
`resolveItemCoPayAmount`) dan pesan keadaan kosong (`emptyMessage`) sendiri dikonfirmasi
byte-persis sama dengan kode sebelum ekstraksi — hanya kelas CSS itu yang lolos.

**Perbaikan:** menambahkan prop opsional `className` pada `BillingInvoiceItemsTable` (default
kosong, tidak mengubah pemakaian Edit Tagihan yang sudah ada), lalu Menu Pembayaran meneruskan
`styles.section` di titik pemanggilannya untuk memulihkan tampilan asli persis seperti sebelum
`FE-BKC-028`. Edit Tagihan (konsumen baru, tidak pernah punya ekspektasi "menempel rapat") tidak
perlu diubah karena kartu-kartu di halaman itu memang berjarak longgar satu sama lain
(`baseStyles.tableCard` dipakai apa adanya di seluruh bagian lain halaman itu juga).

## File yang diubah pada sesi ini (perbaikan regresi, bukan fitur baru)

| File | Perubahan |
| --- | --- |
| `.../billing-invoices/billing-invoice-items-table.jsx` | **Diubah.** Tambah prop opsional `className` (default `""`), diteruskan ke `<section>` pembungkus tabel |
| `.../billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` | **Diubah.** Titik pemanggilan `<BillingInvoiceItemsTable>` kini menyertakan `className={styles.section}` untuk memulihkan `margin-bottom: 0` yang hilang saat ekstraksi `FE-BKC-028` |

Tidak ada berkas lain yang diubah — seluruh temuan lain terverifikasi aman tanpa perlu perubahan
kode.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` / `test:unit` / `build` | **SKIPPED** — instruksi baku pengguna | Tidak dijalankan sesi ini |
| Keempat layar lolos penelusuran papan ketik | **LULUS (tinjauan kode)** | Nihil elemen `<div>`/`<span>`/`<td>`/`<tr>`/`<li>` dengan `onClick` langsung di keempat layar; seluruh kontrol interaktif adalah `<button>`/`<input>` asli. Urutan Tab sesungguhnya di browser: **NOT FEASIBLE** sesi ini |
| Nol UUID tampil di layar maupun URL | **LULUS (tinjauan kode)** | Digrep menyeluruh — setiap `.id`/`.Id` hanya dipakai sebagai `key`/parameter API, tidak pernah dirender sebagai teks |
| Nomor polis/karyawan tidak tampil di tempat tidak semestinya | **LULUS (tinjauan kode)** | Kedua tempat kemunculan diperiksa dan sesuai konteks (kartu pemilihan penanggung, dokumen cetak resmi) — bukan kebocoran baru, mengikuti pola yang sudah ada sebelum rumpun ini |
| Regresi langkah pembayaran admisi Rawat Inap (Acceptance `#70`) | **LULUS (dibuktikan by construction)** | `base-payer-workspace.jsx` dan `inpatient-admission-payment-step.jsx` sama-sama nol baris berubah menurut `git status`/`git diff` |
| Regresi Menu Pembayaran dan Dokumen Kasir | **LULUS setelah perbaikan** | 9 dari 10 berkas pra-eksisting terbukti murni tambahan (`git diff` nol baris dihapus/diubah); satu regresi nyata (margin tabel Menu Pembayaran) ditemukan dan diperbaiki dalam sesi ini |
| Verifikasi manual (klik-coba: keempat layar, langkah pembayaran Rawat Inap, Menu Pembayaran, Dokumen Kasir, di browser sungguhan) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi pada sesi ini |

**Task ini belum bisa ditandai selesai sepenuhnya.** Audit kode selesai menyeluruh dan satu regresi
nyata sudah diperbaiki, tetapi DoD task ini secara eksplisit meminta lint/test/build lulus DAN
penelusuran papan ketik manual — keduanya belum dijalankan sesi ini.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi dan browser
  sungguhan untuk penelusuran papan ketik dan klik-coba regresi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

## Risiko yang tersisa

1. **Penelusuran papan ketik sesungguhnya di browser belum dilakukan** — tinjauan kode
   memastikan tidak ada elemen yang secara struktural tidak dapat dijangkau, tetapi tidak dapat
   memastikan urutan Tab yang logis/intuitif, penanganan fokus setelah modal ditutup, atau
   perilaku pembaca layar sesungguhnya.
2. **Perbaikan regresi (`className` pada `BillingInvoiceItemsTable`) belum diklik-coba** — perlu
   dikonfirmasi di browser bahwa Menu Pembayaran benar-benar kembali menempel rapat seperti
   sebelum `FE-BKC-028`, dan Edit Tagihan tidak ikut berubah tampilannya oleh perubahan ini.
3. **Masking nomor identitas (temuan 3) tetap menjadi celah yang belum ditutup** pada
   `BasePayerWorkspace`/dokumen cetak — sudah ADA sebelum rumpun ini, bukan sesuatu yang
   diperparah, tetapi juga belum diperbaiki siapa pun. Perlu keputusan pemilik produk/keamanan
   terpisah, bukan wewenang task ini.
4. **Seluruh rumpun `FE-BKC-028`–`034` masih menumpuk di working tree yang sama, belum
   di-commit** — disarankan lint/test/build dan review keseluruhan dilakukan sekali untuk seluruh
   rumpun, bukan per task.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build` untuk seluruh rumpun
   `FE-BKC-028`–`034` sekaligus.
2. Klik-coba manual: penelusuran papan ketik murni (tanpa mouse) pada keempat layar; buka langkah
   pembayaran admisi Rawat Inap dan pastikan tampilannya sama persis seperti sebelum rumpun ini;
   buka Menu Pembayaran dan pastikan tabel rincian tagihan kembali menempel rapat dengan grid
   Pembayaran (perbaikan sesi ini); buka Dokumen Kasir dan pastikan tab-tab lama tidak berubah.
3. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-028` s.d. `034`) dan
   `requirement-traceability.md` terkait, tandai `✅`, tautkan laporan masing-masing.
4. Pertimbangkan menindaklanjuti temuan 3 (masking nomor identitas pada `BasePayerWorkspace`/
   dokumen cetak) sebagai task terpisah, dengan pemilik produk/keamanan yang berwenang atas
   komponen bersama itu.
