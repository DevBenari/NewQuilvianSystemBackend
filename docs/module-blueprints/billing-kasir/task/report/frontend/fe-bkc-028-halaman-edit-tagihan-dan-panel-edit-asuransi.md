# FE-BKC-028 — Halaman Edit Tagihan dan panel Edit Asuransi

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-028` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `1.1`) |
| Task type | Frontend, halaman kerja baru (`FE-MPY-01`) + satu panel penuh (`FE-MPY-02`) + ekstraksi komponen bersama |
| Task mode | `FRONTEND` (backend read-only — ketiga endpoint `BE-BKC-047` dibaca langsung dari source dan dikonfirmasi ada persis sesuai kontrak, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `BE-BKC-047` ✅, `BE-BKC-048` ✅, `BE-BKC-049` ✅ — ketiga endpoint dasarnya (`edit-context`, `item-payer-assignments`, `drug-billing-disposition`) dikonfirmasi ada di `BillingInvoicesController.cs`; hanya tiga endpoint milik `BE-BKC-047` yang dipakai task ini |
| Status task | **Source selesai** untuk kerangka `FE-MPY-01` (header, toolbar tiga mode, tabel rincian tagihan) dan panel `FE-MPY-02` Edit Asuransi penuh. Panel `FE-MPY-03`/`FE-MPY-04` (milik `FE-BKC-029`/`FE-BKC-030`) sengaja **belum** diisi — lihat § Keputusan butir 5. **Sesuai instruksi baku pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — hanya `node --check` pada berkas logika non-JSX (lihat § DoD). Belum di-commit |

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
3. **Panel `FE-MPY-03`/`FE-MPY-04` masih placeholder** — kasir yang menekan tombol "Edit Status
   Tagihan"/"Edit Billing" (bila kapabilitasnya aktif) akan melihat pesan "belum tersedia", bukan
   panel fungsional. Ini sesuai scope task (milik `FE-BKC-029`/`030`), tetapi berarti fitur belum
   benar-benar lengkap dari sudut pandang kasir sampai kedua task itu selesai.
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
