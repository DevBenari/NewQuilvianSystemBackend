# Billing dan Kasir — Arsitektur Frontend

> Revision `0.4`, status **approved**; semua layar tetap berstatus **Rencana (belum tersedia)** sampai source diimplementasikan. Input keputusan `0.2`; owner fungsional Product/Billing/Cashier, authority UI Frontend. Root `AGENTS.md` frontend belum ditemukan sehingga aturan visual rinci menjadi dependency build, bukan blocker desain fungsional.

## Prinsip pengalaman pengguna

UI harus memperlihatkan perbedaan antara tagihan berjalan, dana deposit, pembayaran yang sudah berhasil, saldo pasien, saldo penjamin, dan status finalisasi. Tombol tidak boleh menyiratkan “lunas” ketika saldo diselesaikan lewat write-off. Data klinis minimum saja ditampilkan; nomor kartu, token, dan payload provider tidak masuk browser log atau analytics.

## Layar dan workspace

| Workspace | Aktor | Data/status | Aksi utama | Exception yang terlihat |
| --- | --- | --- | --- | --- |
| Daftar Billing | Billing, Kasir | Encounter, jenis layanan, invoice state, outstanding | Cari/filter/buka | Stale version, invoice belum ada |
| Detail Invoice | Billing, Kasir | Item sumber, qty, tarif snapshot, coverage, diskon, tax, patient/guarantor portion | Recalculate, void eligible item, finalisasi | Order belum complete, duplicate source, harga berubah |
| Deposit Rawat Inap | Kasir | Saldo tersedia, top-up, allocation, ledger | Top-up, alokasikan progress, release sisa | Dana kurang, concurrency conflict |
| Pembayaran | Kasir | Outstanding, tender split, status provider | Tambah tender, submit, retry status | QRIS gagal/pending; tender tunai tetap sukses |
| Refund/Write-off/Adjustment | Billing/Finance | Case, reason, maker/approver, histori | Ajukan, approve/reject, reverse | Self-approval ditolak, post-final rule |
| Finalisasi | Billing | Checklist order, calculation version, patient paid, debtor AR, doctor AP basis | Preview, confirm final | Missing order/coverage/debtor |
| Shift Kasir | Kasir/Kepala Kasir | Opening, receipts, system cash, physical cash, variance | Open, handover, close, review/reopen | Selisih belum direview |
| Master Policy | Finance/IT/Dokter | Effective dates, nominal/rate, approval | View/configure sesuai hak | Overlap effective period |

Contoh pembayaran split: kasir memasukkan Tunai Rp300.000 dan QRIS Rp700.000. Tunai sukses tetapi QRIS gagal. UI mempertahankan receipt tunai, menampilkan outstanding Rp700.000, lalu hanya meminta metode pengganti untuk saldo tersebut.

## Alur utama

### Rawat inap dan progress payment

Tujuan: menerima dana tanpa menutup billing berjalan. Prasyarat: encounter ranap aktif, invoice OPEN, shift kasir aktif. Kasir membuka Deposit, melakukan top-up, lalu memilih jumlah allocation. Sistem menampilkan saldo deposit sebelum/sesudah dan versi invoice. Setelah berhasil, state deposit dan outstanding dimuat ulang. Tindakan baru tetap dapat masuk. Jika versi berubah saat submit, UI tidak menebak; tampilkan konflik dan muat ulang.

### OTC

Tujuan: memastikan pembayaran lunas sebelum layanan. Kasir menyelesaikan seluruh tender split. Hanya status settled yang mengaktifkan bukti clearance. Jika petugas lab membatalkan sebelum pemeriksaan, UI menunjukkan request refund; pelaksanaan dana tetap oleh Finance sesuai metode asal.

### Final billing

Tujuan: mengunci kalkulasi dan membuat basis AR/AP. Billing melihat checklist: semua order complete, calculation terbaru, tanggungan pasien settled atau departure exception sah, debtor penjamin valid. Konfirmasi memperlihatkan patient, primary, excess, AR per debtor, dan AP dokter “belum siap dibayar”. Sesudah sukses layar menjadi read-only; koreksi diarahkan ke Adjustment.

## State management dan integrasi FE

| Concern | Rencana |
| --- | --- |
| Route | App Router di area Health Services/Billing Management; nama final mengikuti navigasi existing saat task FE |
| API | Axios service per resource; correlation/idempotency header dibuat sekali per command dan dipertahankan saat retry |
| Server state | Hook query dengan invalidate terarah setelah command; jangan menyimpan invoice finansial sebagai cache permanen |
| Client state | Redux hanya untuk lintas-step payment draft/shift context bila pola repo membenarkan; form lokal untuk filter/modal |
| Concurrency | Kirim version/ETag; `409` menampilkan “Data berubah, muat ulang sebelum melanjutkan.” |
| Pending provider | Poll/status refresh terukur; jangan resubmit tender baru otomatis |
| Error | Pesan Indonesia dari validation contract; correlation ID boleh ditampilkan, payload sensitif tidak |
| Money/time | Decimal diterima sebagai nilai kontrak; format `id-ID`; timestamp ditampilkan Asia/Jakarta dengan sumber UTC/offset |

Lokasi target mengikuti konvensi existing setelah discovery task: route di `src/app`, API di `src/lib/services`, hook di `src/lib/hooks`, Redux registration di `src/lib/state/store.jsx`, dan komponen domain di folder Billing Management. Semua berstatus Baru/Rencana; exact path adalah `DEV_DISCRETION` selama tidak mengubah route/API contract.

## Aksi per peran dan kewenangan UI

| Aksi | Kasir | Billing | Dokter | Finance | Kepala Kasir |
| --- | :---: | :---: | :---: | :---: | :---: |
| Lihat invoice/payment | Ya | Ya | Terbatas miliknya | Ya | Ya |
| Tambah tender/top-up | Ya | Tidak | Tidak | Lihat | Lihat |
| Void item eligible | Tidak | Ya sesuai source authority | Order miliknya melalui domain sumber | Tidak | Tidak |
| Input diskon master | Ya | Ya | Tidak | Ya | Tidak |
| Approve diskon dokter | Tidak | Tidak | Ya, miliknya | Exception saja | Tidak |
| Ajukan adjustment/write-off | Tidak | Ya | Tidak | Ya | Tidak |
| Approve Finance exception | Tidak | Tidak | Tidak | Ya, bukan maker | Tidak |
| Close shift | Ya, shift sendiri | Tidak | Tidak | Lihat | Review |
| Reopen/review variance | Tidak | Tidak | Tidak | Sesuai policy | Ya |
| Finalisasi | Tidak | Ya | Tidak | Lihat/exception | Tidak |

UI menyembunyikan aksi yang tidak berhak, tetapi backend tetap sumber otorisasi. Status `403` harus dijelaskan sebagai hak tidak tersedia, bukan error umum.

## Accessibility dan privacy

Status tidak boleh mengandalkan warna saja; sertakan label dan ikon/teks. Dialog approval memiliki fokus terkelola, keyboard navigation, label nominal yang dibacakan, dan konfirmasi eksplisit. Tabel menyediakan heading, pagination, empty/loading/error states. Cetak receipt hanya memuat data minimum. Mask nomor identitas/provider; jangan render clinical narrative yang tidak diperlukan.

## DEV_DISCRETION

Frontend boleh menentukan grid, urutan panel, komponen drawer/modal, breakpoints, ikon, debounce pencarian, skeleton, dan pembagian hook/component. Frontend tidak boleh mengubah arti status, rumus nominal, kapan OTC clear, siapa approver, idempotency, ataupun menyimpulkan settlement dari tampilan. Perubahan kontrak bisnis kembali ke Product/Domain.

## Acceptance dan dependency

Minimal dibuktikan oleh `BIL-AT-005` split tender parsial, `BIL-AT-007` progress rawat inap, `BIL-AT-012` doctor discount approval, `BIL-AT-016` shift variance, `BIL-AT-020` conflict, dan `BIL-AT-024` privacy/accessibility. Build menunggu approval task roadmap per slice serta pemulihan/penetapan kontrak governance frontend.

## Amendment 2 September 2026 — Form "Buat Invoice Manual (Testing)" berbasis katalog tarif + coverage

> Status **approved** (Product/Domain Owner, 2 September 2026 13:53 WIB). Trace: `BKC-DEC-059`–`062`. Layar terdampak: `create-manual-invoice-view.jsx` (route `/health-services/billing-management/billing/invoices/create-manual`) dan `menu-pembayaran-view.jsx`. Tetap berlabel "Testing" — bukan naik status jadi fitur produksi (`BKC-DEC-059`).

### Kebutuhan fungsional layar "Buat Invoice Manual (Testing)"

| Field/kontrol | Perilaku baru | Sumber data |
| --- | --- | --- |
| Kategori Biaya | Tidak berubah — sudah dari `MstTariffCategory` (`getTariffOptions`... sebenarnya `getTariffCategoryOptions`, existing) | Ready to reuse |
| Nama Item/Layanan | **Diganti**: dari text input bebas menjadi dropdown searchable, opsi dari `GET Tariff/options` difilter `tariffCategoryId` (kategori terpilih) + `serviceUnitId`/`clinicId`/`patientClassId` (dari encounter terpilih, field baru `ActiveEncounterOptionResponse`) + `search` (ketikan kasir). Jika hasil filter konteks masih >1 baris nama sama, tampilkan semua dengan label scope, mis. `"Konsultasi Dokter Umum — RSUD Melati"` (`BKC-DEC-061`) | Reuse with adapter — data layer FE (`getTariffOptions`/`selectTariffOptions`) sudah ada, komposisi dropdown-searchable baru (pola sama field `encounterId` pada form ini, `serverSide`+`onSearchChange`) |
| Harga (Rp) | **Diganti**: dari number input bebas menjadi teks read-only, terisi otomatis `NormalPrice` dari tarif terpilih. Tidak ada event `onChange` untuk field ini | Turunan dari tarif terpilih |
| Badge coverage (baru) | Muncul di sebelah/dalam setiap opsi dropdown (dan di ringkasan setelah item dipilih) untuk pasien asuransi: `Tercover` (hijau) / `Tercover Sebagian` (kuning, mencakup kasus `NeedApproval` — lihat catatan di bawah) / `Tidak Tercover` (merah). Tersembunyi total untuk pasien tunai (`PaymentType=CASH`) | `GET catalog-charges/coverage-preview` (baru) |
| Disclaimer coverage (baru) | Teks kecil di dekat badge: *"Perkiraan — angka final dihitung ulang saat tagihan diproses di Menu Pembayaran."* Wajib ada karena preview bisa berbeda dari kalkulasi final (§ 16.2.A) | `DEV_DISCRETION` untuk penempatan/gaya; isi pesan **MUST** menyebut kata "perkiraan" dan "Menu Pembayaran" |

Field "Kategori Biaya" **MUST** dipilih lebih dulu sebelum dropdown item aktif (pola existing — tidak berubah). Field "Pasien/Kunjungan" **MUST** dipilih sebelum kategori (existing).

### Data/status/error contract

| Concern | Rencana |
| --- | --- |
| Pemicu preview coverage | Dipanggil per opsi tarif saat dropdown item dibuka (bukan per keystroke) — throttle/debounce jadi `DEV_DISCRETION`, tapi **MUST NOT** memanggil untuk setiap huruf yang diketik kasir |
| Loading | Badge menampilkan skeleton/spinner kecil per opsi, tidak memblokir keseluruhan dropdown |
| Error preview | Gagal memuat preview **MUST NOT** memblokir submit — tampilkan badge "Status coverage tidak diketahui" dan tetap izinkan kasir memilih (fail-open untuk UX, karena preview bersifat advisory bukan otoritatif) |
| Error submit (harga/tarif tidak valid, `BIL-VAL-025`) | Tampilkan pesan validasi dari backend apa adanya (Bahasa Indonesia sudah disiapkan backend) |
| Pasien tunai | Tidak memanggil endpoint preview sama sekali — hemat request, badge tidak relevan |
| Duplicate submit | Tidak berubah dari pola existing (`BaseEditorForm` sudah menangani disable-saat-submitting) |

### Menu Pembayaran — split Subtotal Mandiri/Subtotal Asuransi (`BKC-DEC-062`)

| Sebelum | Sesudah |
| --- | --- |
| Satu baris "Subtotal Tagihan" (gabungan), lalu baris pengurang "Ditanggung Penjamin" bila > 0 | Dua baris sejajar: **"Subtotal Mandiri"** (dari `patientAmount`, field calculation existing — TIDAK ada field backend baru) dan **"Subtotal Asuransi"** (dari `primaryAmount + excessAmount`, existing) |
| Pajak digabung ke total sebelum pengurangan penjamin | Pajak tetap satu baris "Pajak", ditampilkan sebagai bagian breakdown Subtotal Mandiri jika `MstTaxRule.AllocationRule="PATIENT"` sudah dikonfigurasi (CAP-07, verifikasi tertunda) — **MUST NOT** diasumsikan tanpa konfirmasi; sertakan catatan kecil bila konfigurasi belum diverifikasi |

**Ini murni perubahan tampilan/komposisi ulang field yang sudah ada di `displayedCalculation`** (`patientAmount`, `primaryAmount`, `excessAmount`, `taxAmount` — semua sudah dikonsumsi `menu-pembayaran-view.jsx` hari ini). Tidak ada field response backend baru untuk kebutuhan ini. Baris "Penjamin Belum Terverifikasi" (`unresolvedCoverageAmount`) tetap dipertahankan apa adanya — cakupannya mengecil setelah `BKC-DEC-062` (lebih sedikit item yang jatuh ke sini), bukan dihapus.

### State management dan integrasi FE — tambahan

| Concern | Rencana |
| --- | --- |
| Redux action baru | `addCatalogCharge` (POST `catalog-charges`) dan `getCatalogChargeCoveragePreview` (GET `catalog-charges/coverage-preview`) — ditambahkan ke slice existing `billing-invoice-slice.jsx`, pola sama persis `addAdhocBillingCharge`/`addBillingOtherCharge` |
| Action lama `addAdhocBillingCharge` | TETAP ADA di slice (tidak dihapus task ini — lihat "Yang sengaja tidak dibuat" pada `02-backend-architecture.md`), tapi tidak lagi dipanggil dari `use-create-manual-invoice.js` setelah amendment |
| Hook terdampak | `use-create-manual-invoice.js` (ganti submit ke `addCatalogCharge`, tambah state tarif terpilih + preview coverage), `use-menu-pembayaran.js` (tidak perlu state baru — hanya `menu-pembayaran-view.jsx` yang mengubah cara menampilkan field yang sudah ada) |

### DEV_DISCRETION tambahan

Bentuk visual badge (warna/ikon/posisi), strategi debounce pencarian tarif, dan penempatan teks disclaimer adalah `DEV_DISCRETION`. Yang **MUST NOT** didelegasikan: isi 3 status coverage dan pemetaannya (`BKC-DEC-060`), formula Subtotal Mandiri/Asuransi (`BKC-DEC-062`), dan keharusan disclaimer "perkiraan" pada badge.

### Acceptance tambahan

`BIL-AT-025`–`028` (lihat `testing/acceptance-test-matrix.md`).

## Amendment 3 September 2026 — Dokumen Kasir: modal menjadi halaman terpisah

> Status **approved** (persetujuan eksplisit pengguna dalam percakapan, 3 September 2026). Trace: `BKC-DEC-063`–`064`. Layar terdampak: `menu-pembayaran-view.jsx` (kedua titik pemicu), `dokumen-kasir-modal.jsx` (dihapus, digantikan halaman baru), `use-dokumen-kasir.js` (dipertahankan, dipakai ulang oleh halaman baru).

Perubahan murni wadah presentasi. Isi dokumen (Kwitansi per tender, Struk Pasien, enam tab
placeholder), mekanisme PDF (`html2pdf.js`), dan fitur share WhatsApp/Email TIDAK berubah —
tetap seperti `BKC-DEC-052`–`058`.

### Route baru

| Concern | Keputusan |
| --- | --- |
| Path | `/health-services/billing-management/billing/invoices/[slug]/pembayaran/dokumen-kasir` — child segment di bawah `pembayaran` yang sudah ada, mengikuti pola `[slug]` yang sama (token invoice di-decode di server component, diteruskan ke client view) |
| State tab/tender aktif | Query string `?tab=KWITANSI&tenderId=...` atau `?tab=STRUK_PASIEN` (`BKC-DEC-064`) — dibaca lewat `useSearchParams` (`next/navigation`), kasir tetap bisa berpindah tab manual di halaman (mengganti query string, bukan reload penuh) |
| Route builder | Tambah `dokumenKasir: (token, { tab, tenderId } = {}) => ...` di `billing-invoice-constants.js` (`BILLING_INVOICE_ROUTES`), pola sama dengan `pembayaran(token)` yang sudah ada |
| Data loading halaman baru | Halaman ini adalah route terpisah (bukan client state yang di-share dari `menu-pembayaran-view.jsx`) — wajib memuat ulang `invoice` (`useBillingInvoiceDetail`) dan `settlement.tenders` (`useBillingSettlement`) sendiri berdasar `invoiceRouteToken` dari `[slug]` dan `tenderId` dari query string, BUKAN memakai `useMenuPembayaran` penuh (hook itu memuat discount policy/tariff category/other-charge types yang tidak relevan untuk halaman baca/cetak ini — pemborosan request) |
| Tombol Kembali | Navigasi ke `BILLING_INVOICE_ROUTES.pembayaran(invoiceRouteToken)` (bukan `router.back()`) — pola `Link`/`BaseButton as={Link}` sama seperti `InpatientConsentPrintView` |
| Tombol Cetak | Tetap `html2pdf.js` (unduh PDF) sesuai `BKC-DEC-063`, BUKAN `window.print()` — beda dari pola `InpatientConsentPrintView`/`print-resep-component.jsx` karena kebutuhan Blob untuk lampiran WhatsApp/Email tetap berlaku |

### Perubahan titik pemicu di `menu-pembayaran-view.jsx`

| Sebelum | Sesudah |
| --- | --- |
| Tombol umum "Dokumen Kasir" memanggil `dokumenKasir.openDokumenKasir()` (buka modal, tab Struk Pasien) | Tombol yang sama menjadi navigasi (`Link`/`router.push`) ke `BILLING_INVOICE_ROUTES.dokumenKasir(token, { tab: "STRUK_PASIEN" })` |
| `BillingSettlementPanel` prop `onPrintKwitansi={dokumenKasir.openKwitansiForTender}` (buka modal, tab Kwitansi, `activeTender` dari state React) | `onPrintKwitansi` menavigasi ke `BILLING_INVOICE_ROUTES.dokumenKasir(token, { tab: "KWITANSI", tenderId: tender.id })` — identitas tender dibawa lewat query string, bukan state React (state hilang saat pindah route) |
| `<DokumenKasirModal .../>` dirender di akhir `menu-pembayaran-view.jsx` | Dihapus. `dokumen-kasir-modal.jsx` dihapus total (tidak ada konsumen lain — dikonfirmasi lewat pencarian referensi di seluruh source frontend) |
| `useDokumenKasir` dipakai via `useMenuPembayaran` untuk state modal (`open`, `activeTab`, `activeTender`, dst.) | `useDokumenKasir` TETAP ADA dan tetap dipakai `use-menu-pembayaran.js`/`menu-pembayaran-view.jsx`, tapi hanya untuk bagian yang masih relevan di halaman itu sendiri (tidak ada lagi — kedua trigger sekarang murni navigasi). Halaman baru memakai instance `useDokumenKasir` miliknya sendiri (`kwitansiPrintRef`, `strukPrintRef`, `downloadKwitansi`, `downloadStruk`, `shareViaWhatsApp`, `shareViaEmail`, `pdfBusy`), diberi `invoice` hasil load halaman baru |

### Reuse komponen (tidak berubah kontraknya)

`KwitansiDocument` dan `StrukPasienDocument` (keduanya `forwardRef`, `kwitansi-document.jsx`/`struk-pasien-document.jsx`) dipakai apa adanya oleh halaman baru, prop shape identik dengan yang dikonsumsi modal sekarang (`kwitansiDocumentProps`/`strukDocumentProps` dibentuk ulang di hook/view halaman baru dari `invoice`+`activeTender`, sama seperti dibentuk `menu-pembayaran-view.jsx` hari ini).

### DEV_DISCRETION tambahan

Layout halaman (Hero, susunan tab, kartu dokumen, action bar) mengikuti pola `InpatientConsentPrintView` (referensi terdekat: page + Hero + area dokumen + action bar Kembali/Cetak) sejauh tidak bertentangan dengan struktur tab existing modal. Nama file/hook baru, penempatan hook composition (langsung di view vs hook terpisah `use-dokumen-kasir-page.js`) adalah `DEV_DISCRETION`. Yang **MUST NOT** berubah: isi/urutan tab, data yang ditampilkan tiap dokumen, dan mekanisme PDF/share (`BKC-DEC-063`).

### Acceptance tambahan

29. Membuka halaman Dokumen Kasir langsung via URL dengan `tenderId` valid menampilkan tab
    Kwitansi untuk tender itu tanpa perlu berpindah tab manual; tanpa `tenderId`/`tab`, halaman
    default ke tab Struk Pasien (setara `openDokumenKasir()` lama).
30. Setelah `dokumen-kasir-modal.jsx` dihapus, build dan lint tidak menyisakan reference mati ke
    file itu di manapun.
31. Tombol Kembali pada halaman Dokumen Kasir selalu kembali ke Menu Pembayaran invoice yang
    sama (bukan daftar invoice), termasuk saat halaman dibuka langsung dari URL (deep link).

## Amendment 3 September 2026 (kedua) — Dokumen Kasir: tab baru "Invoice Asuransi"

> Status **draft**. Trace: `BKC-DEC-065`–`069` (approved Product/Domain Owner 3 September 2026) dan `BKC-DES-001`–`BKC-DES-009` (`02-backend-architecture.md`, draft). Frontend SHA diaudit `00210f9a5fb2f4f69e57b8c90c57c63c788da792`. Layar terdampak: halaman Dokumen Kasir yang baru dibuat pada amendment sebelumnya (`FE-BKC-017`).
>
> Amendment ini **bergantung penuh** pada slice backend. Tanpa endpoint `GET {id}/insurance-invoice-document`, tab ini tidak punya sumber data — `BKC-DEC-069` sudah menyatakan ini bukan pekerjaan frontend murni.

### Kebutuhan fungsional layar

Tab ketiga bernama **"Invoice Asuransi"** pada halaman Dokumen Kasir (`/health-services/billing-management/billing/invoices/[slug]/pembayaran/dokumen-kasir`), sejajar Kwitansi dan Struk Pasien, **sebelum** enam tab placeholder. Tab `Claim Letter` tidak disentuh dan tetap placeholder milik `InsuranceManagement` (`BKC-DEC-065`).

| No | Kebutuhan | Aturan |
| --- | --- | --- |
| 1 | Kasir dapat membuka tab "Invoice Asuransi" dan melihat lembar dokumen siap cetak | Isi lembar seluruhnya berasal dari satu panggilan `GET {id}/insurance-invoice-document`. Layar **MUST NOT** menghitung, menyaring, atau menjumlahkan rupiah sendiri |
| 2 | Lembar memuat tiga blok berurutan: kepala surat rumah sakit, blok identitas pasien + blok perusahaan asuransi, lalu tabel rincian dan total | Susunan mengikuti pola `KwitansiDocument`/`StrukPasienDocument` yang sudah ada (`BKC-DEC-065`: "pola presentasi sama dengan Kwitansi") |
| 3 | Setiap baris tabel menampilkan kolom rupiah yang ditanggung asuransi | Wajib, `BKC-DEC-069`. Bukan hanya badge status |
| 4 | Hanya baris yang benar-benar ditanggung asuransi yang tampil | `BKC-DEC-068`. Penyaringan sudah dilakukan backend; layar menampilkan `items` apa adanya dan **MUST NOT** menambah filter sendiri |
| 5 | Kasir dapat mencetak/mengunduh lembar sebagai PDF | `html2pdf.js`, sama seperti Kwitansi dan Struk Pasien (`BKC-DEC-063` masih berlaku) |
| 6 | Bila dokumen tidak dapat diterbitkan, layar menjelaskan sebabnya dengan bahasa yang dipahami kasir dan mematikan tombol cetak | Sebab diambil dari `warnings[]` yang dikirim backend — layar **MUST NOT** mengarang pesannya sendiri |
| 7 | Tab ini hanya memanggil endpoint saat tab-nya aktif | Kasir yang hanya mencetak Kwitansi tidak boleh menanggung satu permintaan tambahan; endpoint dokumen memicu kalkulasi pratinjau di server |

### Aksi per peran

| Aksi | Kasir | Petugas Billing | Dokter | Keterangan |
| --- | :---: | :---: | :---: | --- |
| Membuka tab "Invoice Asuransi" | Ya | Ya | Tidak relevan | Gerbang `[AccessPermission("BillingInvoice", "Read")]` yang sama dengan seluruh halaman Dokumen Kasir |
| Menekan "Cetak Invoice Asuransi" | Ya | Ya | — | Tombol hanya muncul saat tab aktif **dan** `isPrintable === true` |
| Mengubah isi dokumen dari layar | **Tidak** | **Tidak** | — | Dokumen murni baca; koreksi angka jalurnya lewat item invoice/Pengecualian Finansial, bukan lewat lembar cetak |
| Membagi via WhatsApp/Email | **Tidak pada rilis ini** | **Tidak** | — | Lihat § Yang sengaja tidak dibuat |

### Data, status, dan error contract

| Concern | Keputusan |
| --- | --- |
| Endpoint | `GET api/v1/health-services/billing-management/billing/invoices/{id}/insurance-invoice-document` |
| Thunk baru | `getInsuranceInvoiceDocument` pada `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx`, pola identik `previewBillingInvoiceCalculation` yang sudah ada |
| Slot state baru | `insuranceInvoiceDocument`, `insuranceInvoiceDocumentLoading`, `insuranceInvoiceDocumentError` beserta selector `selectInsuranceInvoiceDocument`/`…Loading`/`…Error` |
| Kapan dipanggil | Di dalam `use-dokumen-kasir-page.js`, hanya ketika `activeTab === "INVOICE_ASURANSI"` dan `invoice` sudah termuat. Dipanggil ulang bila `invoiceRouteToken` berubah, **tidak** dipanggil ulang setiap perpindahan tab bila datanya sudah ada |
| Sumber angka | **Hanya** dari response endpoint di atas. **MUST NOT** memakai `currentCalculation.breakdown` milik Menu Pembayaran, dan **MUST NOT** menjumlahkan `items[].grossAmount` seperti `StrukPasienDocument` — dokumen ini berbicara soal porsi penjamin, bukan total tagihan |
| Status kosong | `items` kosong → tampilkan `warnings[0]` sebagai `InformationAlert variant="info"` (bukan `danger`: ini keadaan wajar, bukan galat), sembunyikan lembar dokumen, dan jangan render tombol cetak |
| Status tidak dapat dicetak | `isPrintable === false` → lembar boleh tetap tampil bila `items` ada isinya, tetapi tombol cetak tidak dirender dan seluruh `warnings` ditampilkan. Kasus nyata: invoice `FINAL` lama yang totalnya sah tetapi rinciannya tidak tersedia |
| Peringatan bersama isi | `warnings` yang tidak kosong **MUST** tetap ditampilkan meski `items` ada isinya — misalnya "Data perusahaan asuransi tidak ditemukan pada master". Peringatan yang disembunyikan karena tabelnya sudah terisi adalah peringatan yang gagal bekerja |
| Galat sungguhan | `404`/`422`/`500` ditangani `handleActionError` yang sudah ada pada `useBillingInvoiceDetail` (toast merah), pola sama seperti seluruh aksi di modul ini |
| Penanda kesegaran | Lembar mencantumkan `calculationVersionNo` dan `calculatedAt` dalam format tanggal Indonesia di bagian bawah, agar lembar yang tercetak dapat ditelusuri ke versi kalkulasi mana |
| Penanda tagihan berjalan | Bila `invoiceStatus === "OPEN"`, lembar mencantumkan keterangan "Tagihan masih berjalan — angka dapat berubah sampai tagihan difinalkan." Ini bukan hiasan: `BKC-DEC-066` menghendaki dokumen dipakai pihak asuransi, dan lembar dari tagihan berjalan yang tidak menyebut statusnya bisa ditagihkan sebagai angka final |

### Komponen dan hook

| Berkas | Status | Peran |
| --- | --- | --- |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/invoice-asuransi-document.jsx` | **Baru** | Komponen `forwardRef` berisi lembar dokumen. Pola identik `kwitansi-document.jsx`/`struk-pasien-document.jsx`: inline style, lebar kertas tetap, tanpa dependency baru |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/dokumen-kasir-view.jsx` | Diperbarui | Tambah satu `Nav.Item` bertab `INVOICE_ASURANSI`, satu blok render bersyarat, dan satu tombol Hero "Cetak Invoice Asuransi" |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-dokumen-kasir-page.js` | Diperbarui | Memanggil thunk baru secara lazy, membentuk `invoiceAsuransiDocumentProps`, meneruskan `invoiceAsuransiPrintRef` dan `downloadInvoiceAsuransi` |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-dokumen-kasir.js` | Diperbarui | Tambah `invoiceAsuransiPrintRef` dan `downloadInvoiceAsuransi`; `buildPdf` diberi parameter opsional ukuran kertas (lihat di bawah) |
| `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx` | Diperbarui | Tambah satu thunk, tiga slot state, tiga selector, dan penanganan `clearBillingInvoiceDetail` agar dokumen ikut dibersihkan saat pindah invoice |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` | Diperbarui | Tambah konstanta nilai tab (`DOKUMEN_KASIR_TABS`) agar nilai `"INVOICE_ASURANSI"` tidak ditulis sebagai teks lepas di tiga berkas |

**Ukuran kertas.** `buildPdf` di `use-dokumen-kasir.js` hari ini mengunci `jsPDF: { format: "a5" }`. Kwitansi dan Struk Pasien memang muat di A5, tetapi tabel Invoice Asuransi punya kolom tambahan (Ditanggung Asuransi, Porsi Pasien) dan akan terpotong. Perubahan: `buildPdf(element, filenamePrefix, { format = "a5" } = {})`, dan pemanggil dokumen ini mengirim `{ format: "a4" }`. Bawaan tetap `"a5"`, sehingga Kwitansi dan Struk Pasien **tidak berubah sama sekali** — ini penambahan opsi, bukan perubahan perilaku existing. Lebar lembar pada komponen mengikuti: `210mm` (A4) alih-alih `148mm` (A5).

**Nilai tab dan navigasi.** Mekanisme query string dari `BKC-DEC-064` sudah cukup: `?tab=INVOICE_ASURANSI` bekerja tanpa route baru. Yang perlu diperbaiki: inisialisasi tab pada `use-dokumen-kasir-page.js` hari ini hanya mengenali dua jalur — `KWITANSI` bila ada `tenderId`, selain itu `openDokumenKasir()` yang selalu memaksa `STRUK_PASIEN`. Akibatnya tautan `?tab=INVOICE_ASURANSI` akan mendarat di tab yang salah. Perbaikan: inisialisasi menghormati nilai `tab` apa pun yang dikenali (`KWITANSI`, `STRUK_PASIEN`, `INVOICE_ASURANSI`, dan enam nilai placeholder), dan hanya jatuh ke `STRUK_PASIEN` bila `tab` kosong atau tidak dikenali. Ini juga memperbaiki tautan `?tab=SPT` dan sejenisnya yang hari ini diam-diam diabaikan.

**Kepala surat rumah sakit.** Nama rumah sakit hari ini ditulis sebagai teks tetap di dalam `kwitansi-document.jsx` dan `struk-pasien-document.jsx`. Komponen baru **MUST** memakai teks yang sama persis agar ketiga dokumen tidak menampilkan identitas yang berbeda. Ini penyimpangan yang sudah ada (identitas rumah sakit seharusnya berasal dari satu sumber, bukan disalin per komponen) dan **MUST NOT** diperbaiki menyelip di task ini — perapiannya task tersendiri, dan bila dikerjakan harus mengubah ketiga komponen sekaligus.

### Perubahan tampilan Menu Pembayaran (opsional, ditempatkan setelah dokumen)

Setelah `CalculationItemResponse` punya `coveredAmount` per baris, penanda "Penjamin"/"Mandiri" per baris item di Menu Pembayaran dapat berpijak pada rupiah sungguhan alih-alih gabungan `coverable` dan total invoice (jalan pintas yang dicatat pada `FE-BKC-FIX-006`). Ini **bukan** bagian scope amendment ini dan **MUST NOT** dikerjakan bersamaan — dicatat di sini agar tidak hilang, dan diusulkan sebagai `POST-MVP` pada `04-prd-to-mvp.md`. Alasan dipisah: menyentuh layar yang paling sering dipakai kasir demi perbaikan kosmetik, sementara tab baru belum terbukti berjalan.

### Penanganan state, cache, dan pengiriman ganda

| Concern | Keputusan |
| --- | --- |
| Pemuatan awal | Satu permintaan saat tab pertama kali aktif. Selama `insuranceInvoiceDocumentLoading`, tampilkan `InformationAlert variant="info"` "Menyusun Invoice Asuransi..." dan jangan render lembar setengah jadi |
| Invalidasi | Dokumen dibersihkan saat `clearBillingInvoiceDetail` (pindah invoice) dan saat halaman dilepas. **Tidak** ada cache lintas invoice |
| Data basi | Kembali ke tab ini setelah kasir menambah biaya di tab lain **tidak** memuat ulang otomatis. Kesegaran dinyatakan lewat `calculatedAt` yang tercetak di lembar, bukan lewat pemuatan ulang diam-diam yang membuat lembar berubah saat kasir sedang membacanya |
| Pengiriman ganda | Tombol cetak memakai `pdfBusy` yang sudah ada (`loading`/`loadingLabel` pada `BaseButton`), pola sama seperti Kwitansi dan Struk Pasien |
| Kegagalan PDF | Ditangani `handleActionError` dengan pesan "Gagal membuat PDF Invoice Asuransi.", pola sama seperti `downloadKwitansi`/`downloadStruk` |

### Accessibility dan privacy

- Status tidak boleh disampaikan hanya lewat warna: setiap peringatan memakai teks lengkap, bukan hanya badge berwarna (melanjutkan `BIL-AT-024`).
- Tabel rincian memakai `<thead>`/`<th>` sungguhan, bukan `<div>` bergaya tabel, agar terbaca pembaca layar.
- Nomor polis dan nomor anggota tampil di lembar karena pihak asuransi membutuhkannya untuk mengenali klaim, tetapi **MUST NOT** masuk `console.log`, telemetri, maupun `localStorage`. Nomor kartu asuransi tidak dikirim backend sama sekali (lihat `02-backend-architecture.md` § Yang sengaja tidak dibuat), jadi tidak ada di layar.
- Nama berkas PDF memakai nomor invoice, bukan nama pasien — mengikuti pola `Kwitansi-{invoiceNumber}.pdf` yang sudah ada, sehingga nama pasien tidak ikut tersebar lewat nama berkas di folder unduhan.

### DEV_DISCRETION

Didelegasikan ke pengembang:

- lebar kolom, jarak antar blok, ukuran huruf, dan garis pembatas pada lembar dokumen, sejauh mengikuti kesan visual `KwitansiDocument`;
- urutan field di dalam blok identitas pasien dan blok perusahaan asuransi;
- teks label kolom tabel (misalnya "Ditanggung Asuransi" versus "Porsi Asuransi");
- ada atau tidaknya blok tanda tangan di kaki lembar, serta labelnya;
- nama berkas hook/komponen dan penempatan komposisi hook;
- apakah "terbilang" total tanggungan ikut dicetak (`terbilangRupiah` sudah tersedia dan dipakai Kwitansi).

**MUST NOT** didelegasikan:

- daftar baris yang tampil — ditentukan backend sesuai `BKC-DEC-068`, layar tidak menyaring;
- keberadaan kolom rupiah per baris (`BKC-DEC-069`);
- sumber blok perusahaan adalah `MstInsuranceProvider`, bukan penjamin perusahaan tempat kerja (`BKC-DEC-067`);
- keharusan menampilkan `warnings` dan mematikan tombol cetak saat `isPrintable === false`;
- keharusan mencantumkan status tagihan berjalan pada lembar invoice `OPEN`;
- isi dan urutan tab yang sudah ada, termasuk `Claim Letter` yang tetap placeholder.

### Yang sengaja tidak dibuat (frontend)

| Yang ditolak | Alasan |
| --- | --- |
| Tombol WhatsApp/Email untuk Invoice Asuransi | `BKC-DEC-056` mengatur share untuk Kwitansi kepada pasien. Mengirim lembar berisi nomor polis ke kanal pesan pribadi adalah keputusan privasi tersendiri yang belum pernah diminta maupun diputuskan |
| Pratinjau cetak dalam dialog terpisah | Lembar sudah tampil apa adanya di halaman (pola `stagePaper` yang sudah ada) — dialog tambahan hanya menambah satu langkah tanpa informasi baru |
| Penyaringan/pengurutan baris dari layar | `BKC-DEC-068` menetapkan isinya; layar yang bisa menyaring membuat dua lembar berbeda untuk tagihan yang sama |
| Perbaikan penanda per baris di Menu Pembayaran | Dipisah sebagai `POST-MVP`, lihat § di atas |
| Memperbaiki kepala surat rumah sakit yang tersalin di tiga komponen | Task perapian tersendiri; mengubahnya di sini menyentuh Kwitansi dan Struk Pasien yang sudah terverifikasi |

### Acceptance tambahan

32. Membuka halaman Dokumen Kasir dengan `?tab=INVOICE_ASURANSI` mendarat langsung di tab Invoice Asuransi, bukan di tab Struk Pasien.
33. Untuk kunjungan pasien asuransi dengan sedikitnya satu item tercover, lembar menampilkan nama perusahaan asuransi, nomor polis, dan tabel berisi kolom rupiah yang ditanggung per baris; jumlah kolom itu sama dengan total tanggungan yang tercetak di kaki tabel.
34. Item yang tidak ditanggung asuransi **tidak** muncul di lembar, meskipun muncul di Struk Pasien pada invoice yang sama.
35. Untuk kunjungan tunai, tab menampilkan keterangan biru bahwa dokumen tidak dapat diterbitkan, tanpa lembar dan tanpa tombol cetak — bukan pesan galat merah.
36. Untuk invoice yang difinalkan sebelum pembaruan sistem, tab menampilkan total tanggungan beserta keterangan bahwa rincian per item tidak tersedia, dan tombol cetak tidak muncul.
37. Menekan "Cetak Invoice Asuransi" menghasilkan PDF A4 yang seluruh kolom tabelnya terbaca utuh, tanpa kolom terpotong di sisi kanan.
38. Cetak Kwitansi dan Cetak Struk Pasien tetap menghasilkan PDF A5 seperti sebelumnya (tidak ada regresi dari perubahan `buildPdf`).
39. Tab Invoice Asuransi tidak memicu permintaan `insurance-invoice-document` selama kasir belum membuka tab itu.

---

## Amendment 4 September 2026 — Menu Pembayaran: menghapus "Penjamin Belum Terverifikasi" dan menampilkan anomali data

> Status **draft**. Masukan: `BKC-DEC-070`–`075` (approved 4 September 2026) dan keputusan arsitektur `BKC-DES-010`–`014`, `BKC-DES-017`–`019` pada `02-backend-architecture.md`. Amendment ini **tidak** mengunci warna, jarak, ikon, maupun pilihan component library — seluruhnya tetap mengikuti design system yang berlaku.

### Batas amendment ini

Yang berubah hanya satu layar: **Menu Pembayaran** (`menu-pembayaran-view.jsx`). Yang **tidak** berubah: halaman Dokumen Kasir beserta ketiga tabnya, form Buat Invoice Manual (Testing), panel Tambah Biaya Lain-lain, dan seluruh layar master data.

### Peta butir menu — tidak ada butir baru

Amendment ini **tidak** menambah satu pun butir menu. Seluruh perubahan terjadi di dalam layar yang sudah terjangkau lewat butir menu yang ada.

| Butir menu | Tingkat | Induk | Route | Layar yang dituju | Butir hak akses |
| --- | --- | --- | --- | --- | --- |
| Invoice Billing | 3 | Billing Management | `/health-services/billing-management/billing-invoices` | Daftar invoice | `BillingInvoice : Read` |
| — (layar anak) | — | Invoice Billing | `/health-services/billing-management/billing-invoices/[slug]/pembayaran` | **Menu Pembayaran** — layar yang diubah amendment ini | `BillingInvoice : Read` |

Menu Pembayaran tetap **layar anak**: jalan masuknya adalah tombol pada baris daftar invoice, bukan butir menu tersendiri. Ini tidak berubah dari baseline.

### Kebutuhan fungsional layar

| No | Kebutuhan | Dasar |
| --- | --- | --- |
| 1 | Baris "Penjamin Belum Terverifikasi" **dihapus seluruhnya** dari blok Ringkasan Pembayaran | `BKC-DEC-075` |
| 2 | Badge status per baris item berhenti memakai status `belum_terverifikasi`; tersisa dua status normal — "Penjamin" dan "Tunai" — ditambah satu status khusus "Anomali Data" | `BKC-DEC-071`, `BKC-DEC-073` |
| 3 | Ketika tagihan mengandung anomali data, layar menampilkan **peringatan di atas Ringkasan Pembayaran**, bukan baris subtotal | `BKC-DEC-073`, `BKC-DES-011` |
| 4 | Peringatan itu menyebut nominal yang terdampak dan tindakan yang harus dilakukan, dalam kalimat yang dapat dibaca kasir | `BKC-DEC-073` |
| 5 | Baris baru "Selisih Tidak Ditagihkan (kontrak penjamin)" muncul **hanya bila** nilainya lebih besar dari nol | `BKC-DES-013` |
| 6 | `ExcessAmount` tidak lagi ditampilkan di mana pun | `BKC-DES-014` |
| 7 | Subtotal Mandiri, Subtotal Asuransi, Pajak Mandiri, dan Pajak Asuransi tetap dijumlah **eksak** dari hasil per komponen, seperti yang sudah berjalan sejak `FE-BKC-FIX-008` | `BKC-DEC-070` |

### Skema fitur layar — blok Ringkasan Pembayaran

```text
┌─────────────────────────────────────────────────────────────┐
│  [A]  Peringatan anomali data          (hanya bila ada)      │
│       ⚠ Penjamin kunjungan ini belum dinyatakan layak.       │
│         Rp 440.000 untuk sementara dibebankan ke pasien.     │
│         Periksa data penjamin di Registrasi sebelum menagih. │
├─────────────────────────────────────────────────────────────┤
│  [B]  Ringkasan Pembayaran                                   │
│       Subtotal Mandiri .................... Rp   440.000     │
│       Subtotal Asuransi ................... Rp         0     │
│       Pajak Mandiri ....................... Rp         0     │
│       Pajak Asuransi ...................... Rp         0     │
│       Selisih Tidak Ditagihkan ............ Rp         0  ←  │
│         (baris ini hilang bila nilainya nol)                 │
│       ─────────────────────────────────────────────────      │
│       Total Tagihan ....................... Rp   440.000     │
├─────────────────────────────────────────────────────────────┤
│  [C]  Daftar item beserta badge status per baris             │
└─────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Hak akses | Keadaan kosong dan gagal |
| --- | --- | --- | --- | --- |
| `[A]` | Peringatan anomali data | `breakdown.coverage.hasDataAnomaly`, `dataAnomalyAmount`, `anomalyMessages` dari `GET .../calculation-preview` | `BillingInvoice : Read` | Bila `hasDataAnomaly` bernilai `false`, wilayah ini **tidak dirender sama sekali** — bukan dirender kosong |
| `[B]` | Ringkasan Pembayaran | Dijumlah eksak dari `breakdown.items[].itemPrimaryAmount`/`taxPrimaryAmount` dan `breakdown.administrationFee`/`roomCharge.primaryAmount` | `BillingInvoice : Read` | Kalkulasi gagal → seluruh blok diganti `InformationAlert` merah berisi pesan dari backend apa adanya, bukan angka Rp 0 |
| `[B]` baris "Selisih Tidak Ditagihkan" | Nominal residual yang tidak boleh ditagihkan ke pasien | `breakdown.coverage.unresolvedAmount` | `BillingInvoice : Read` | Bernilai nol → baris tidak dirender |
| `[C]` | Badge status per baris | `breakdown.items[].itemPrimaryAmount`, `itemDataAnomalyAmount`, `taxPrimaryAmount`, `taxDataAnomalyAmount` | `BillingInvoice : Read` | Item tidak ditemukan di `breakdown.items` → badge "Tunai" (lebih aman daripada menebak "Penjamin") |

### Aturan penentuan badge per baris

Diturunkan langsung dari `contracts/permission-audit-matrix.md` dan dari `BKC-DES-010`; bukan dikarang di layar.

| Urutan periksa | Kondisi | Badge | Catatan |
| ---: | --- | --- | --- |
| 1 | `itemDataAnomalyAmount + taxDataAnomalyAmount > 0` | **Anomali Data** | Diperiksa lebih dulu karena ia menjelaskan kenapa baris lain terlihat "Tunai" |
| 2 | `itemPrimaryAmount + taxPrimaryAmount > 0` | **Penjamin** | Ada rupiah yang benar-benar ditanggung penjamin untuk baris ini |
| 3 | selain itu | **Tunai** | Termasuk baris tanpa aturan yang cocok dan baris `NotCovered` — keduanya memang tanggungan pasien (`BKC-DEC-072`) |

Status `belum_terverifikasi` pada `BILLING_ITEM_COVERAGE_BADGE_CONFIG` **diganti namanya** menjadi `anomali_data` dengan label "Anomali Data". Ini bukan penambahan status baru: setelah `BKC-DEC-071`, satu-satunya sebab tersisa untuk baris yang bukan Penjamin dan bukan Tunai adalah anomali data.

> **Contoh berangka.** Invoice rawat jalan pasien asuransi berisi tiga item. "Konsultasi Dokter Umum" Rp 100.000 dengan aturan `Covered` 100% → badge **Penjamin**, Subtotal Asuransi Rp 100.000. "Fisioterapi" Rp 300.000 dengan aturan `Covered` 80% → badge **Penjamin**, Rp 240.000 ke Subtotal Asuransi dan Rp 60.000 ke Subtotal Mandiri. "Vitamin C tablet" Rp 25.000 tanpa aturan yang cocok → badge **Tunai**, Rp 25.000 ke Subtotal Mandiri. Ringkasan: Subtotal Mandiri Rp 85.000, Subtotal Asuransi Rp 340.000, Total Rp 425.000. Baris "Penjamin Belum Terverifikasi" tidak ada, dan baris "Selisih Tidak Ditagihkan" juga tidak muncul karena `IsAllowExcessPaymentByPatient` bernilai bawaan `true`.

### Aksi per peran

Tidak ada aksi baru. Peringatan anomali data bersifat **informatif** — ia tidak menonaktifkan tombol pembayaran, tidak menonaktifkan finalisasi, dan tidak menambah tombol apa pun (`BKC-OQ-086` mengangkat pertanyaan apakah finalisasi seharusnya diblokir; sampai dijawab, jawabannya tidak).

| Peran | Yang dapat dilakukan saat anomali data muncul |
| --- | --- |
| Kasir | Melihat peringatan, tetap dapat menerima pembayaran, dan diharapkan menghubungi Pendaftaran |
| Supervisor Billing | Sama seperti kasir |
| Petugas Pendaftaran | Membetulkan data penjamin di modul Registrasi (di luar layar ini) |

### Penanganan keadaan

| Keadaan | Yang dilihat kasir |
| --- | --- |
| Memuat | Kerangka blok ringkasan, bukan layar kosong — tidak berubah dari sekarang |
| Kosong (invoice belum punya item) | Seluruh subtotal Rp 0, tanpa peringatan anomali, tanpa baris "Selisih Tidak Ditagihkan" |
| Gagal memuat kalkulasi | `InformationAlert` merah berisi pesan backend; blok ringkasan tidak dirender dengan angka nol |
| Anomali data | `InformationAlert` **kuning** (`variant="warning"`) di wilayah `[A]`, ditambah badge "Anomali Data" pada baris yang terdampak |
| Data basi | Tidak berubah — kalkulasi diminta ulang setiap layar dibuka |
| Pengiriman ganda | Tidak berubah — tombol pembayaran dinonaktifkan selama permintaan berjalan |
| Tanpa hak akses | `AccessDeniedGate` seperti sekarang |

### Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Baris "Penjamin Belum Terverifikasi" dihapus | **Terkunci** oleh `BKC-DEC-075` |
| Anomali data tampil sebagai peringatan, bukan baris subtotal | **Terkunci** oleh `BKC-DES-011` |
| Urutan baris di dalam Ringkasan Pembayaran | `DEV_DISCRETION` |
| Pilihan varian `InformationAlert` (kuning versus biru) untuk peringatan anomali | `DEV_DISCRETION` dengan rekomendasi `warning` — biru terbaca sebagai informasi biasa, merah terbaca sebagai kegagalan sistem, sedangkan ini adalah masalah data yang perlu ditindaklanjuti orang |
| Kalimat persis pada peringatan | **Terkunci sebagian**: kalimatnya datang dari backend (`anomalyMessages`), layar hanya merangkainya. Layar **MUST NOT** mengarang kalimatnya sendiri |
| Token warna badge "Anomali Data" | `DEV_DISCRETION`, dengan rekomendasi memakai ulang `region-status-pending` yang sudah dipakai `belum_terverifikasi` — tidak ada nilai visual literal baru |

### Yang sengaja tidak dibuat (frontend)

| Yang ditolak | Alasan |
| --- | --- |
| Tombol "Perbaiki Data Penjamin" yang menautkan ke modul Registrasi | Navigasi lintas modul dari layar kasir belum pernah diminta dan menuntut keputusan pemilik kedua modul |
| Menonaktifkan tombol pembayaran saat anomali data | Menahan pasien di kasir karena kesalahan data pendaftaran; `BKC-DEC-073` tidak memintanya |
| Menampilkan kode anomali mentah (`PAYER_NOT_ELIGIBLE`) di layar | Kode adalah kunci program. Yang dibaca kasir adalah `anomalyMessages` |
| Menghitung ulang status coverage di browser | Dua tempat memutuskan angka yang sama; yang tercetak di dokumen itulah yang akan dianggap salah |

### Acceptance tambahan

40. Untuk invoice pasien asuransi yang seluruh datanya normal, baris "Penjamin Belum Terverifikasi" **tidak ada** di Ringkasan Pembayaran, dan Subtotal Mandiri + Subtotal Asuransi + Pajak Mandiri + Pajak Asuransi menjumlah persis ke Total Tagihan.
41. Untuk invoice yang penjaminnya belum `IsEligible`, muncul peringatan kuning di atas Ringkasan Pembayaran yang menyebut nominal terdampak, dan seluruh nominal itu tampil di Subtotal Mandiri — bukan di baris tersendiri.
42. Pada invoice yang sama, tombol pembayaran tetap aktif dan pembayaran tetap dapat diselesaikan.
43. Item yang tidak punya aturan coverage yang cocok menampilkan badge "Tunai", bukan "Menunggu Verifikasi", dan nominalnya masuk Subtotal Mandiri.
44. Item dengan aturan `Covered` yang menandai `IsNeedApproval` atau mengisi `MaxAmountPerMonth` menampilkan badge "Penjamin" dan nominalnya masuk Subtotal Asuransi — bukan lagi tertahan.
45. Baris "Selisih Tidak Ditagihkan (kontrak penjamin)" muncul hanya pada invoice yang punya aturan dengan `IsAllowExcessPaymentByPatient = false` dan residual lebih besar dari nol.
46. Untuk invoice rawat inap yang berisi obat/alkes, kolom Pajak Mandiri dan Pajak Asuransi keduanya Rp 0, dan tidak ada baris pajak di rincian item.
47. Untuk invoice rawat jalan dan IGD yang berisi obat/alkes, pajak tetap muncul dan terbagi mengikuti status coverage item obatnya.

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

> Revisi `1.0`, status **draft**. Masukan: **`PC-DEC-001`–`PC-DEC-013`** (`approved` Product/Domain Owner 7 September 2026) dan keputusan arsitektur `PC-DES-001`–`PC-DES-014` pada [`02-backend-architecture.md`](./02-backend-architecture.md). Frontend SHA diaudit `12f9242ce62e4d80dbdb719f80bb0e7a2848474c`.
>
> Amendment ini **bergantung penuh** pada slice backend. Tidak ada satu pun endpoint Petty Cash yang sudah ada; seluruhnya berstatus **Rencana (belum tersedia)**.
>
> Amendment ini **tidak** mengunci warna, jarak, ikon, pilihan component library, maupun bentuk wadah presentasi. Yang dikunci adalah **keterjangkauan layar** dan **sumber datanya**.

### Batas amendment ini

Yang bertambah adalah tiga butir menu baru beserta layar-layarnya. Yang **tidak** berubah: Menu Pembayaran, halaman Dokumen Kasir beserta ketiga tabnya, form Buat Invoice Manual (Testing), layar Shift Kasir, dan seluruh layar master data yang sudah ada. Tidak satu pun berkas milik rumpun lain disentuh.

### Rujukan tampilan yang dipakai, dan batasnya

Pemilik menunjukkan tangkapan layar saat wawancara: daftar voucher berjudul "Petty Cash — Monitoring Voucher Petty Cash", kartu ringkasan "TOTAL PETTY CASH", modal "Buat Voucher", dan modal "Bukti Nota/Kasir".

Tangkapan itu dipakai sebagai **bukti kebutuhan dan penamaan field**, bukan sebagai spesifikasi yang tidak dapat ditawar. Bila ia bertentangan dengan `PC-DEC-*`, keputusan yang menang. Dua pertentangan yang sudah diketahui:

| Yang terlihat pada tangkapan layar | Yang berlaku | Dasar |
| --- | --- | --- |
| Nomor voucher berbentuk `PC-1786239462244`, tampak berbasis milidetik | Nomor berbentuk `PTC-YYYYMMDD-NNNN` yang dibuat sistem penomoran modul ini | `PC-DES-008`, `01-existing-capability-map.md` § 18.7 |
| Kategori tampak sebagai daftar tetap | Kategori adalah data induk yang dikelola Finance lewat menunya sendiri | `PC-DEC-012` |

### Kebutuhan layar

| ID layar | Nama | Jenis | Jalan masuk |
| --- | --- | --- | --- |
| `FE-PC-01` | Monitoring Voucher Petty Cash | Daftar | **Butir menu** |
| `FE-PC-02` | Buat Voucher | Isian pengajuan | **Layar anak** `FE-PC-01` — tombol "+ Buat Voucher" |
| `FE-PC-03` | Bukti Nota/Kasir | Isian nomor nota | **Layar anak** `FE-PC-01` — tombol "Input Nota" pada baris |
| `FE-PC-04` | Detail Voucher | Detail beserta riwayat perintah | **Layar anak** `FE-PC-01` — klik dua kali pada baris |
| `FE-PC-05` | Anggaran Kas Kecil | Saldo, pengisian, koreksi, dan riwayat pergerakan | **Butir menu** |
| `FE-PC-06` | Kategori Petty Cash — daftar | Daftar data induk | **Butir menu**, di dalam grup Master Data |
| `FE-PC-07` | Kategori Petty Cash — detail | Detail data induk | **Layar anak** `FE-PC-06` |
| `FE-PC-08` | Kategori Petty Cash — tambah/ubah | Form data induk | **Layar anak** `FE-PC-06` |

### Peta butir menu

Modul ini menambah **tiga** butir menu. Sisanya adalah layar anak yang dicapai dari layar induknya, dan itu dinyatakan eksplisit agar tidak ada layar yang hanya dapat dibuka lewat URL langsung.

```text
Billing Management                                  <- tingkat 0, sudah ada
├── Running Invoice                                 -> sudah ada
├── Buat Invoice Manual (Testing)                   -> sudah ada
├── Persetujuan Diskon Dokter                       -> sudah ada
├── Shift Kasir                                     -> sudah ada
├── Petty Cash                                      -> .../petty-cash/vouchers          [BARU]
├── Anggaran Kas Kecil                              -> .../petty-cash/budget            [BARU]
└── Master Data                                     <- grup tingkat 1, sudah ada
    ├── Administration Fee Policy                   -> sudah ada
    ├── Discount Policy                             -> sudah ada
    ├── Room Charge Policy                          -> sudah ada
    ├── Tax Rule                                    -> sudah ada
    ├── Register                                    -> sudah ada
    └── Kategori Petty Cash                         -> .../master-data/petty-cash-category  [BARU]
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Petty Cash | 1 | Billing Management | `/health-services/billing-management/petty-cash/vouchers` | `FE-PC-01` | `PettyCashVoucher : Read` | **Baru** |
| Anggaran Kas Kecil | 1 | Billing Management | `/health-services/billing-management/petty-cash/budget` | `FE-PC-05` | `PettyCashBudget : Read` | **Baru** |
| Kategori Petty Cash | 2 | Master Data | `/health-services/billing-management/master-data/petty-cash-category` | `FE-PC-06` | `PettyCashCategory : Read` | **Baru** |

**Berkas yang disunting saat implementasi:** `src/utils/menu-sidebar/menu-items.jsx`. Ketiga butir masuk sebagai anggota `subMenu` milik Billing Management, kecuali Kategori Petty Cash yang masuk `subItems` milik grup Master Data — mengikuti bentuk yang sudah dipakai lima butir master data yang ada.

**Pendaftaran butir menu MUST menjadi acceptance criteria salah satu task layar**, bukan pekerjaan yang menganggur di antara dua task. Modul ini punya preseden pahitnya: lima halaman `billing-management` pernah selesai dan lulus build tetapi tidak terjangkau sampai `FE-BKC-MENU-001` dikerjakan sebagai task tersendiri.

Layar `FE-PC-02`, `FE-PC-03`, `FE-PC-04`, `FE-PC-07`, dan `FE-PC-08` **sengaja tidak** mendapat butir menu. Kelimanya adalah layar anak, dan jalan masuknya sudah disebut pada tabel Kebutuhan layar di atas.

### Skema fitur — `FE-PC-01` Monitoring Voucher Petty Cash

```text
+- Petty Cash — Monitoring Voucher Petty Cash --------------- FE-PC-01 -+
|  TOTAL PETTY CASH                                                     |
|  Rp 4.700.000        Sudah dijanjikan Rp 300.000                      |
+-----------------------------------------------------------------------+
| [cari no. voucher / nama / kategori]  [Periode v] [Tgl awal] [Tgl akhir]|
| [Status v] [Kategori v] [Jumlah baris v]      [Atur ulang] [+ Buat Voucher] |
+-----------------------------------------------------------------------+
| No | No. Voucher | Nama Penerima | Kategori | Nominal | Tujuan |       |
|    |             |               |  chip    |  angka  |        |       |
|    | Tgl Pengajuan | Bukti          | Status  | Aksi             |       |
|    |               | [Input Nota]   |  chip   | [Uang Diberikan] |       |
|    |               | atau no. nota  |         | atau kosong      |       |
+-----------------------------------------------------------------------+
| memuat -> kerangka baris, bukan layar kosong                          |
| kosong -> "Data voucher petty cash tidak ditemukan."   [Atur ulang]   |
| gagal  -> "Data gagal dimuat."                         [Coba lagi]   |
+- Halaman 1 dari n ---------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kartu ringkasan | Saldo kas kecil berjalan, ditambah keterangan nominal yang sudah dijanjikan | `GET /petty-cash/budget/current` → `currentBalance`, `reservedAmount` | `PettyCashBudget : Read` | Gagal → kartu diganti pesan singkat; **tabel tetap tampil**. Saldo yang gagal dimuat **MUST NOT** dirender sebagai Rp 0 |
| Saringan | Pencarian, periode, tanggal awal/akhir, status, kategori, jumlah baris | `GET /petty-cash/vouchers/filters/metadata` untuk pilihannya; `GET /master-data/petty-cash-categories/options` untuk daftar kategori | `PettyCashVoucher : Read` | Metadata gagal → saringan memakai nilai bawaan; layar tetap dapat dipakai |
| Tabel | No. Voucher, Nama Penerima, Kategori, Nominal Voucher, Tujuan, Tanggal Pengajuan, Bukti, Status, Aksi | `GET /petty-cash/vouchers` | `PettyCashVoucher : Read` | Kosong → "Data voucher petty cash tidak ditemukan." beserta "Coba gunakan filter lain atau tambahkan data baru." |
| Kolom Status | Chip berisi `statusLabel` dari server, ditambah penanda "Dibatalkan" bila `isCancelled` bernilai `true` | `statusLabel`, `isCancelled` | `PettyCashVoucher : Read` | — |
| Kolom Bukti | Tombol "Input Nota" bila `proofReferenceNumber` kosong; chip berisi nomor notanya bila sudah ada | `proofReferenceNumber` | `PettyCashVoucher : AttachProof` untuk tombolnya | Tombol yang tidak berhak **MUST** disembunyikan, bukan ditampilkan lalu ditolak `403` |
| Kolom Aksi | Tombol yang tersedia untuk baris itu | **`availableActions`** dari server | `PettyCashVoucher : Approve`/`Reject`/`Cancel`/`Disburse` sesuai tombolnya | Baris tanpa aksi yang tersedia menampilkan kolom kosong, bukan tombol yang dinonaktifkan |
| Tombol "+ Buat Voucher" | Membuka `FE-PC-02` | — | `PettyCashVoucher : Create` | Disembunyikan bila tidak berhak |

**Aturan yang mengikat untuk kolom Aksi.** Tombol yang tampil **MUST** diturunkan dari `availableActions` yang dikirim server, **MUST NOT** disimpulkan layar dari nilai `status`. Alasannya: ketersediaan aksi bergantung pada status **dan** pada siapa penggunanya — tombol "Batalkan" hanya untuk pemohon voucher itu sendiri (`PC-DEC-007`), dan layar tidak boleh menebak aturan kepemilikan itu sendiri. `availableActions` tetap **bantuan tampilan**, bukan pengaman; backend tetap memeriksa ulang setiap permintaan.

**Kolom yang MUST NOT ditampilkan:** identitas pengguna dalam bentuk UUID. Kolom pembuat, penyetuju, dan pencair menampilkan `requestedByName`, `decidedByName`, dan `disbursedByName`.

### Skema fitur — `FE-PC-02` Buat Voucher

```text
+- Buat Voucher --------------------------------------------- FE-PC-02 -+
| Voucher Number    [ Dibuat otomatis oleh sistem ]        (hanya-baca)  |
| Nama Penerima     [ ............................ ]                     |
| Kategori          [ Pilih Kategori           v  ]                      |
| Nominal Voucher   [ Rp ......................... ]                     |
| Tujuan            [ ............................ ]                     |
|                   [ ............................ ]                     |
+-----------------------------------------------------------------------+
|                                        [ Batal ]  [ Simpan Voucher ]  |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Voucher Number | Kolom hanya-baca berisi keterangan bahwa nomor dibuat otomatis | Tidak ada — **MUST NOT** dikirim pada permintaan simpan | — | Selalu tampil sebagai keterangan, tidak pernah sebagai isian |
| Nama Penerima | Isian teks bebas | Diketik pengguna | `PettyCashVoucher : Create` | Kosong → pesan validasi dari server ditampilkan apa adanya |
| Kategori | Dropdown, hanya kategori aktif | `GET /master-data/petty-cash-categories/options` | `PettyCashCategory : Read` | Daftar kosong → "Belum ada kategori petty cash yang aktif. Hubungi Finance untuk menambahkannya." dan tombol Simpan dinonaktifkan |
| Nominal Voucher | Isian mata uang | Diketik pengguna | `PettyCashVoucher : Create` | Nol atau negatif → pesan validasi |
| Tujuan | Isian teks panjang | Diketik pengguna | `PettyCashVoucher : Create` | Kosong → pesan validasi |
| Tombol Simpan | Mengirim pengajuan | `POST /petty-cash/vouchers` | `PettyCashVoucher : Create` | Selama pengiriman berjalan, tombol dinonaktifkan agar tidak terkirim dua kali |

**Yang MUST NOT ada pada layar ini:** isian nomor voucher yang dapat diketik, isian tanggal pengajuan, dan pemilihan penerima dari daftar pegawai. Ketiganya bertentangan dengan `PC-DES-008`, perilaku server, dan `PC-DEC-011`.

### Skema fitur — `FE-PC-03` Bukti Nota/Kasir

```text
+- Bukti Nota/Kasir ----------------------------------------- FE-PC-03 -+
| Voucher   PTC-20260907-0001 — Budi Santoso — Rp 300.000               |
|                                                                       |
| Masukkan No Nota / Kwitansi   [ ......................... ]           |
+-----------------------------------------------------------------------+
|                                              [ Tutup ]  [ Simpan ]    |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Nomor voucher, nama penerima, dan nominal, agar petugas yakin sedang mengisi voucher yang benar | Baris yang diklik pada `FE-PC-01` | `PettyCashVoucher : Read` | — |
| Isian nomor nota | Satu isian teks | Diketik pengguna | `PettyCashVoucher : AttachProof` | Kosong → "Nomor nota atau kwitansi wajib diisi." |
| Tombol Simpan | Menyimpan bukti dan menyelesaikan voucher | `POST /petty-cash/vouchers/{id}/proofs` | `PettyCashVoucher : AttachProof` | Gagal → pesan dari server ditampilkan apa adanya; modal **tidak** ditutup |

Layar ini juga dipakai **mengoreksi** nomor nota pada voucher yang sudah `Selesai`. Ketika dibuka pada voucher semacam itu, isian sudah terisi nomor lama, dan menyimpannya **tidak** memindahkan status (`PC-DES-012`).

### Skema fitur — `FE-PC-05` Anggaran Kas Kecil

```text
+- Anggaran Kas Kecil --------------------------------------- FE-PC-05 -+
|  SALDO SAAT INI        SUDAH DIJANJIKAN        SISA YANG BEBAS        |
|  Rp 5.000.000          Rp 300.000              Rp 4.700.000           |
|                          [ + Tambah Anggaran ]  [ Koreksi Saldo ]     |
+-----------------------------------------------------------------------+
| Riwayat Pergerakan                                                    |
| [Jenis v] [Tgl awal] [Tgl akhir]                     [Atur ulang]     |
+-----------------------------------------------------------------------+
| No | Tanggal | Jenis | Nominal | Saldo Sebelum | Saldo Sesudah |       |
|    |         | chip  |         |               |               | Alasan|
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Tiga kartu angka | Saldo, yang sudah dijanjikan, dan sisa yang benar-benar bebas | `GET /petty-cash/budget/current` | `PettyCashBudget : Read` | Gagal → ketiga kartu diganti satu pesan; **MUST NOT** dirender Rp 0 |
| Tombol Tambah Anggaran | Membuka isian nominal dan alasan | `POST /petty-cash/budget/top-ups` | `PettyCashBudget : TopUp` | Disembunyikan bila tidak berhak |
| Tombol Koreksi Saldo | Membuka isian nominal, arah, dan alasan | `POST /petty-cash/budget/adjustments` | `PettyCashBudget : Adjust` | Disembunyikan bila tidak berhak |
| Tabel riwayat | Tanggal, jenis pergerakan, nominal, saldo sebelum dan sesudah, alasan, pelaku | `GET /petty-cash/budget/movements` | `PettyCashBudget : Read` | Kosong → "Belum ada pergerakan anggaran pada saringan ini." |

**Ketiga angka pada kartu MUST diambil dari server apa adanya.** Layar **MUST NOT** menghitung "sisa yang bebas" sendiri dengan menjumlahkan voucher yang tampil di halaman pertama — hasilnya akan menyimpang dari server begitu ada satu voucher yang tidak ikut terkirim pada halaman itu (`PC-DES-005`).

### Skema fitur — `FE-PC-06` sampai `FE-PC-08` Kategori Petty Cash

Ketiga layar ini **berbagi bentuk** dengan layar master data modul ini yang sudah ada — Tax Rule, Discount Policy, Room Charge Policy — dan karena itu **tidak digambar ulang**. Bentuknya mengikuti `rules/frontend/master-data-feature-standard.md` apa adanya: daftar dengan kartu ringkasan dan saringan, detail dengan tombol Kembali/Perbarui/Hapus, serta satu form untuk tambah dan ubah.

| Yang khusus pada fitur ini | Nilainya |
| --- | --- |
| Isian form | `categoryCode` (wajib, unik), `categoryName` (wajib), `description` (opsional), `isActive` (hanya saat ubah) |
| Kolom daftar | No, Tanggal Dibuat, Kode, Nama Kategori, Dibuat Oleh, Status |
| Pesan konfirmasi hapus | "Kategori yang sudah dipakai voucher tidak dapat dihapus." |
| Kegagalan hapus | `400` dari server ditampilkan apa adanya: kategori masih dipakai voucher |
| Isian kode | **Diketik Finance**, bukan dibuat sistem — mengikuti `MstTaxRule.Code` yang juga diisi pengguna |

### Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md), bukan dikarang ulang di sini.

| Aksi | Kasir / petugas administrasi | Kepala Kasir / Finance Operations | Finance (pengelola anggaran) |
| --- | :---: | :---: | :---: |
| Melihat daftar voucher dan saldo | Ya | Ya | Ya |
| Membuat voucher atas nama siapa saja | Ya | Ya | Tidak, kecuali diberi butirnya |
| Membatalkan voucher | Ya, **hanya yang diajukannya sendiri** | Tidak, kecuali ia pemohonnya | Tidak |
| Menyetujui atau menolak | **Tidak** | **Ya** | Tidak |
| Menekan "Uang Diberikan" | Ya | Ya | Tidak |
| Memasukkan atau mengoreksi bukti nota | Ya | Ya | Tidak |
| Menambah atau mengoreksi anggaran | Tidak | Tidak | **Ya** |
| Mengelola kategori | Tidak | Tidak | **Ya** |

Pembagian di atas adalah **saran pemetaan**. Siapa yang benar-benar mendapat butir mana ditentukan admin lewat layar Akses Role. Layar menyembunyikan aksi yang tidak berhak, tetapi backend tetap sumber otorisasi, dan `403` dijelaskan sebagai hak tidak tersedia, bukan galat umum.

### Data dan status yang dikonsumsi

| Layar | Endpoint yang dibaca | Kapan dipanggil |
| --- | --- | --- |
| `FE-PC-01` | `GET /petty-cash/vouchers/filters/metadata`, `GET /petty-cash/vouchers/summary`, `GET /petty-cash/vouchers`, `GET /petty-cash/budget/current` | Metadata sekali saat layar dibuka; ketiganya yang lain setiap saringan berubah |
| `FE-PC-02` | `GET /master-data/petty-cash-categories/options` | Saat form dibuka |
| `FE-PC-04` | `GET /petty-cash/vouchers/{id}` | Saat detail dibuka |
| `FE-PC-05` | `GET /petty-cash/budget/current`, `GET /petty-cash/budget/movements` | Saat layar dibuka dan setiap saringan riwayat berubah |
| `FE-PC-06`–`08` | Sembilan endpoint kategori | Mengikuti pola master data yang sudah berjalan |

**Cara menampilkan status.** Layar menampilkan `statusLabel` yang dikirim server apa adanya. Layar **MUST NOT** memetakan sendiri kode `WAITING_APPROVAL` menjadi kalimat Bahasa Indonesia — dua tempat yang memutuskan kalimat yang sama akan menyimpang, dan kalimatnya sudah dikunci `PC-DEC-013`.

**Setelah setiap aksi berhasil**, layar memuat ulang daftar voucher **dan** kartu saldo bersamaan. Memuat ulang salah satunya saja membuat kasir melihat voucher yang sudah `Uang Diterima` di samping saldo yang belum berkurang.

### Penanganan keadaan

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Memuat | Kerangka baris pada tabel dan kerangka angka pada kartu, bukan layar kosong |
| Kosong | "Data voucher petty cash tidak ditemukan." beserta "Coba gunakan filter lain atau tambahkan data baru." |
| Gagal memuat daftar | Pesan merah beserta tombol Coba lagi; kartu saldo tetap tampil bila ia berhasil dimuat |
| Gagal memuat saldo | Kartu diganti pesan singkat; **tabel tetap dapat dipakai**. Angka saldo yang gagal dimuat **MUST NOT** ditampilkan sebagai Rp 0 |
| Tanpa hak akses | `AccessDeniedGate` seperti layar lain di modul ini |
| Data basi | Setiap aksi berhasil memicu pemuatan ulang daftar dan saldo. Tidak ada pemuatan ulang berkala di latar |
| Pengiriman ganda | Tombol dinonaktifkan selama permintaan berjalan. Untuk "Uang Diberikan", layar **MUST** mengirim `Idempotency-Key` yang **sama** saat mencoba ulang permintaan yang gagal karena jaringan — kunci baru berarti percobaan itu dianggap penyerahan uang kedua |
| Anggaran tidak cukup saat menyetujui | Pesan dari server ditampilkan apa adanya, termasuk angka sisa yang dapat dipakai. Layar **MUST NOT** menyingkatnya menjadi "Gagal" |
| Kategori kosong | Form Buat Voucher menampilkan keterangan biru dan menonaktifkan tombol Simpan — ini keadaan wajar pada pemakaian pertama, bukan galat |

### Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Ketiga butir menu beserta route-nya | **Terkunci** oleh amendment ini |
| Kelima label status | **Terkunci** oleh `PC-DEC-013`. Layar menampilkan `statusLabel` dari server |
| Nama field pada form: Nama Penerima, Kategori, Nominal Voucher, Tujuan | **Terkunci** — mengikuti rujukan tampilan pemilik |
| Nomor voucher sebagai kolom hanya-baca | **Terkunci** oleh `PC-DES-008` |
| Tombol aksi diturunkan dari `availableActions` | **Terkunci** |
| Ketiga angka anggaran diambil dari server | **Terkunci** oleh `PC-DES-005` |
| **Wadah presentasi** — modal, drawer, atau halaman terpisah untuk `FE-PC-02` dan `FE-PC-03` | `DEV_DISCRETION`. Rujukan tampilan memakai modal dan itu **direkomendasikan**, tetapi wadahnya adalah keputusan bentuk. Yang terkunci adalah isi dan sumber datanya |
| Urutan kolom tabel, lebar kolom, dan penempatan kartu | `DEV_DISCRETION` |
| Warna chip status dan chip kategori | `DEV_DISCRETION`, dengan syarat status **tidak** disampaikan lewat warna saja — chip **MUST** memuat teksnya |
| Ikon, jarak, dan bentuk kontrol | `DEV_DISCRETION` |
| Nama berkas hook, komponen, dan penempatan komposisinya | `DEV_DISCRETION` |
| Apakah `FE-PC-04` menampilkan riwayat perintah sebagai tabel atau linimasa | `DEV_DISCRETION` |

### Yang sengaja tidak dibuat (frontend)

| Yang ditolak | Alasan |
| --- | --- |
| Tombol sunting pada voucher, pada status apa pun | Tidak ada endpoint penyuntingan, dan tidak ada keputusan yang mengaturnya. Voucher yang salah dibatalkan lalu dibuat ulang |
| Tombol "Ajukan Ulang" pada voucher yang ditolak | `PC-DEC-003` — voucher ditolak adalah catatan permanen |
| Pemilihan penerima dari daftar pegawai | `PC-DEC-011` — teks bebas |
| Penghitungan sisa anggaran di sisi layar | `PC-DES-005` — dua tempat yang menghitung uang yang sama akan menyimpang |
| Pemetaan kode status menjadi kalimat di sisi layar | `PC-DEC-013` mengunci kalimatnya; server yang mengirimnya |
| Penanda atau peringatan untuk voucher yang lama tidak bernota | `PC-DEC-006` menunda seluruh mekanisme pengingat ke rilis berikutnya. Menambahkan penandanya di layar berarti membangun setengah dari fitur yang sengaja ditunda |
| Menampilkan kas kecil di layar Shift Kasir, atau sebaliknya | `PC-DEC-001` — dua kantong yang berbeda |
| Ekspor daftar voucher ke berkas | Tidak diminta satu pun keputusan |

### Acceptance tambahan

48. Ketiga butir menu baru muncul di sidebar bagi pengguna yang memegang butir hak aksesnya, dan **tidak** muncul bagi yang tidak memegangnya.
49. Kartu TOTAL PETTY CASH menampilkan angka yang sama persis dengan yang dikirim server, dan **tidak** dihitung ulang dari daftar voucher yang tampil.
50. Kolom Aksi menampilkan tombol "Uang Diberikan" **hanya** pada baris yang `availableActions`-nya memuat aksi itu — bukan pada setiap baris berstatus `Disetujui` tanpa memeriksa kewenangan pengguna.
51. Kolom Bukti menampilkan tombol "Input Nota" ketika nomor nota belum ada, dan chip berisi nomornya ketika sudah ada.
52. Voucher yang dibatalkan pemohonnya tampil dengan penanda "Dibatalkan", dan chip statusnya **tetap** "Menunggu Persetujuan" — bukan status keenam.
53. Form Buat Voucher menampilkan Voucher Number sebagai kolom hanya-baca berketerangan otomatis, dan permintaan simpan yang terkirim **tidak** memuat nomor voucher.
54. Ketika belum ada satu pun kategori aktif, form Buat Voucher menampilkan keterangan biru dan tombol Simpan dinonaktifkan — bukan dropdown kosong tanpa penjelasan.
55. Menekan "Uang Diberikan" dua kali berturut-turut menghasilkan satu penyerahan uang, dan saldo pada kartu berkurang satu kali.
56. Setelah "Uang Diberikan" berhasil, daftar voucher **dan** kartu saldo dimuat ulang bersamaan.
57. Persetujuan yang ditolak karena anggaran tidak cukup menampilkan pesan dari server beserta angka sisa yang dapat dipakai, bukan pesan galat umum.
58. Menutup shift kasir setelah mencairkan voucher pada hari yang sama menghasilkan angka kas shift yang sama persis seperti bila voucher itu tidak pernah dicairkan.
59. Tidak ada satu pun UUID yang tampil di layar maupun di URL pada seluruh layar Petty Cash.

---

## Amendment 11 September 2026 — Rumpun baru: Edit Tagihan & Multi-Payer Coverage

> Revisi blueprint `1.1`, status **draft**. Masukan: `MPY-DEC-001`–`010`, `MPY-DES-001`–`017`, `01-existing-capability-map.md` § 19 (`CAP-40`).
>
> **Koreksi lokasi base component yang wajib dibaca implementer.** Komponen dasar generik repository ini berada di `src/components/features/base-features/`, **bukan** di `src/components/ui/`. Folder `src/components/ui/` hanya berisi pembungkus layout dan komponen khusus modul klinis. Dokumen sumber PDF menyebut lokasi yang keliru; yang berlaku adalah hasil pembacaan langsung pada `CAP-40`.

### Kebutuhan layar

| ID | Layar | Jenis | Jalan masuk |
| --- | --- | --- | --- |
| `FE-MPY-01` | Edit Tagihan | Halaman kerja per tagihan | Tombol pada Menu Pembayaran — **layar anak**, tidak mendapat butir menu sendiri |
| `FE-MPY-02` | Panel Edit Asuransi beserta perbandingan berdampingan | Panel di dalam `FE-MPY-01` | Tombol mode pada `FE-MPY-01` |
| `FE-MPY-03` | Panel Edit Status Tagihan | Panel di dalam `FE-MPY-01` | Tombol mode pada `FE-MPY-01` |
| `FE-MPY-04` | Panel Edit Billing | Panel di dalam `FE-MPY-01` | Tombol mode pada `FE-MPY-01` |
| `FE-MPY-05` | Lembar Invoice Penjamin Perusahaan | Tab pada halaman Dokumen Kasir yang sudah ada | **Layar anak** dari Dokumen Kasir |
| `FE-MPY-06` | Master Data — Rute Reimbursement Penjamin Perusahaan | Daftar dan formulir | Butir menu sendiri |
| `FE-MPY-07` | Master Data — Aturan Tanggungan Penjamin Perusahaan | Daftar dan formulir | Butir menu sendiri |

### Peta butir menu

```text
Administrator
└── Master Data
    └── Rute Reimbursement Penjamin              -> .../master-data/company-guarantor-reimbursement-routes

Health Services
└── Master Data
    └── Aturan Tanggungan Penjamin               -> .../master-data/company-guarantor-coverage-rules
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Rute Reimbursement Penjamin | 2 | Administrator › Master Data | `/administrator/master-data/company-guarantor-reimbursement-routes` | `FE-MPY-06` | `CompanyGuarantorReimbursementRoute : Read` | Baru |
| Aturan Tanggungan Penjamin | 2 | Health Services › Master Data | `/health-services/master-data/company-guarantor-coverage-rules` | `FE-MPY-07` | `CompanyGuarantorCoverageRule : Read` | Baru |

**Layar yang sengaja tidak mendapat butir menu**, beserta jalan masuknya:

| Layar | Jalan masuk | Alasan |
| --- | --- | --- |
| `FE-MPY-01` sampai `FE-MPY-04` | Tombol "Edit Tagihan" pada Menu Pembayaran, pada tagihan tertentu | Layar ini hanya bermakna dalam konteks satu tagihan. Butir menu tanpa tagihan akan membuka layar kosong |
| `FE-MPY-05` | Tab pada halaman Dokumen Kasir yang sudah ada | Mengikuti pola lembar Invoice Asuransi yang juga tab pada halaman yang sama |

Pendaftaran kedua butir menu baru **MUST** menjadi acceptance criteria salah satu task layar master data, bukan pekerjaan yang menganggur di antara dua task. Preseden yang mendasarinya ada di modul ini sendiri: lima halaman `billing-management` pernah selesai dan lulus build tetapi tidak terjangkau siapa pun sampai pendaftaran menunya dikerjakan sebagai task tersendiri.

### Skema fitur — `FE-MPY-01` Edit Tagihan

```text
+- Edit Tagihan - INV-2026-000481 - Ny. S ------------------ FE-MPY-01 -+
| Penanggung kunjungan: Penjamin - PT Sejahtera      [Kembali]          |
| [Edit Asuransi] [Edit Status Tagihan] [Edit Billing]                  |
+-----------------------------------------------------------------------+
|  panel mode aktif (FE-MPY-02 / 03 / 04)                               |
|  [Simpan Perubahan]  [Batal Edit]                                     |
+-----------------------------------------------------------------------+
| Rincian tagihan, dikelompokkan per kategori                           |
| Deskripsi | Satuan | Penanggung | Harga Satuan | QTY | Harga          |
| ...                                        SubTotal per kategori      |
+-----------------------------------------------------------------------+
| Subtotal Mandiri | Subtotal Penjamin | Pajak | Total | Harus Dibayar   |
+-----------------------------------------------------------------------+
| memuat -> kerangka baris                                              |
| gagal  -> "Data tagihan gagal dimuat."            [Coba lagi]         |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Nomor tagihan, nama pasien, penanggung kunjungan yang berlaku | `GET /{id}/edit-context` | `BillingInvoice : Read` | Gagal → seluruh layar diganti pesan beserta tombol coba lagi |
| Tombol mode | Edit Asuransi, Edit Status Tagihan, Edit Billing | `capabilities` pada edit-context | `BillingInvoice : Update` | Mode yang tidak diizinkan tampil **nonaktif beserta alasannya**, bukan disembunyikan tanpa keterangan — kasir perlu tahu mengapa |
| Panel mode | Isi mode yang sedang aktif | — | — | — |
| Tombol simpan | Simpan Perubahan, Batal Edit | — | `BillingInvoice : Update` | Simpan nonaktif selama tidak ada perubahan |
| Rincian tagihan | Baris biaya per kategori beserta penanggungnya | `calculation` pada edit-context | `BillingInvoice : Read` | Kosong → "Tagihan ini belum memiliki baris biaya." |
| Ringkasan | Subtotal Mandiri, Subtotal Penjamin, Pajak, Total, Harus Dibayar | `calculation` pada edit-context | `BillingInvoice : Read` | Gagal → seluruh layar diganti pesan |

**Label ringkasan mengikuti jenis penanggung.** Ketika kunjungan berpenjamin perusahaan, baris ringkasan berbunyi "Subtotal Penjamin"; ketika berasuransi, "Subtotal Asuransi". Keduanya membaca **ember rupiah yang sama** beserta penanda jenis payer yang dibawa response (`MPY-DES-017`). Frontend **MUST NOT** menampilkan dua baris subtotal terpisah untuk asuransi dan penjamin — salah satunya selalu nol, dan baris nol yang permanen membingungkan pembacanya.

### Skema fitur — `FE-MPY-02` Edit Asuransi

```text
+- Edit Asuransi --------------------------------------------- FE-MPY-02 -+
| Penanggung sekarang: Penjamin - PT Sejahtera                            |
| Ganti menjadi *  [ Tunai | Asuransi | Penjamin Perusahaan ]             |
|                  [ pilih kartu milik pasien              v]            |
|                  [Bandingkan]                                           |
+-------------------------------------------------------------------------+
|  SEKARANG                        |  BILA DIGANTI                        |
|  PT Sejahtera                    |  Prudential                          |
|  Total        Rp 270.000         |  Total        Rp 270.000             |
|  Ditanggung   Rp 160.000         |  Ditanggung   Rp 216.000             |
|  Mandiri      Rp 110.000         |  Mandiri      Rp  54.000             |
+-------------------------------------------------------------------------+
| Alasan perubahan * [                                        ]           |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Penanggung sekarang | Jenis dan nama penjamin yang berlaku | `currentPayer` pada edit-context | `BillingInvoice : Read` | — |
| Pilihan kartu | Kartu asuransi dan kartu penjamin perusahaan milik pasien | `availablePayerOptions` pada edit-context | `BillingInvoice : Read` | Kosong → "Pasien ini belum memiliki kartu penjamin terdaftar. Pendaftaran kartu dilakukan di Data Pasien/Registrasi." |
| Kartu tidak dapat dipakai | Tampil nonaktif beserta sebabnya | `availablePayerOptions[].blockReason` | — | Kartu kedaluwarsa atau belum layak tampil nonaktif, **bukan disembunyikan** — kasir perlu tahu kartunya ada tapi tidak dapat dipakai |
| Perbandingan berdampingan | Total, ditanggung, mandiri pada kedua sisi, beserta selisih per baris | `POST /{id}/payer-comparison-preview` | `BillingInvoice : Read` | Gagal → panel perbandingan diganti pesan beserta tombol coba lagi; isian tetap utuh |
| Alasan | Isian wajib | — | — | Kosong → simpan nonaktif |

Frontend **MUST NOT** menghitung sendiri selisih harga maupun tanggungan. Seluruh angka pada kedua kolom berasal dari server.

Sebelum menyimpan, tampilkan konfirmasi: *"Gunakan &lt;nama penjamin&gt; sebagai penanggung kunjungan ini? Tagihan akan dihitung ulang."* Sesudah berhasil, tampilkan pemberitahuan bila ada baris biaya yang penanggungnya ikut dikembalikan menjadi tanggungan pasien, beserta jumlahnya.

### Skema fitur — `FE-MPY-03` Edit Status Tagihan

Subjudul panel **MUST** berbunyi **"Ubah penanggung biaya per item"**. Label tombolnya boleh tetap "Edit Status Tagihan" mengikuti kebiasaan pengguna, tetapi tanpa subjudul itu pengguna akan menyangka yang diubah adalah status hidup-matinya tagihan.

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Daftar baris biaya | Tiap baris beserta isian penanggung: Pribadi, Asuransi, Penjamin | `itemPayerAssignments` pada edit-context | `BillingInvoice : Read` | Kosong → "Tagihan ini belum memiliki baris biaya." |
| Pilihan yang tidak tersedia | Nonaktif beserta alasannya, misalnya "Kunjungan ini tidak memakai asuransi" | `availablePayerOptions` | — | **MUST** nonaktif beserta alasan, bukan hilang begitu saja |
| Baris yang diubah | Ditandai halus | — | — | — |
| Penghitung perubahan | "3 item diubah" | — | — | Nol perubahan → tombol simpan nonaktif |
| Alasan | Isian wajib | — | — | Kosong → simpan nonaktif |

Baris berstatus dibatalkan tampil tetapi isian penanggungnya nonaktif.

### Skema fitur — `FE-MPY-04` Edit Billing

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Tiga tombol pilihan | Ditebus, Tebus Sebagian, Tidak Ditebus | `capabilities.canEditDrugBilling` | `BillingInvoice : Update` | Tidak layak → panel diganti keterangan: "Penebusan obat tidak dapat diubah untuk jenis kunjungan ini." |
| Kotak centang per baris obat | Muncul **hanya** pada mode Tebus Sebagian, dan **hanya** pada baris obat yang layak | `eligibleDrugInvoiceItemIds` | — | Tagihan tanpa baris obat layak → "Tagihan ini tidak memiliki item obat yang dapat diatur penebusannya." |
| Isian jumlah | **Selalu hanya-baca** | — | — | Tombol tambah dan kurang jumlah **MUST NOT** muncul sama sekali pada layar ini |
| Baris non-obat | Tampil hanya-baca tanpa kotak centang | — | — | — |
| Alasan | Isian wajib | — | — | Kosong → simpan nonaktif |

Pada mode Ditebus, seluruh kotak centang tampil tercentang dan hanya-baca. Pada mode Tidak Ditebus, seluruhnya kosong dan hanya-baca.

### Skema fitur — `FE-MPY-05`, `FE-MPY-06`, `FE-MPY-07`

`FE-MPY-05` mengikuti bentuk lembar Invoice Asuransi yang sudah ada, dengan tiga perbedaan isi: identitas perusahaan penjamin menggantikan identitas perusahaan asuransi, ditambah identitas karyawan, ditambah satu baris keterangan rute penggantian biaya. Komponen lembarnya bersifat presentasional murni dan menerima seluruh isinya sebagai properti, sama seperti pendahulunya.

`FE-MPY-06` dan `FE-MPY-07` memakai bentuk daftar dan formulir master data yang sudah baku di repository ini — keduanya berbagi bentuk yang sama dengan layar master data lain, sehingga cukup dirujuk, tidak perlu digambar ulang. Dua aturan isian yang mengikat:

| Layar | Aturan isian |
| --- | --- |
| `FE-MPY-06` | Ketika jenis rute "Menanggung sendiri" dipilih, isian perusahaan asuransi mitra **MUST** disembunyikan dan nilainya dikosongkan — bukan sekadar dinonaktifkan sambil menyimpan nilai lama |
| `FE-MPY-07` | Isian urun biaya **MUST** hanya-baca dan terisi otomatis mengikuti persentase tanggungan, karena nilainya diturunkan server |

### Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md), tidak dikarang ulang.

| Peran | Dapat melihat | Dapat mengubah |
| --- | --- | --- |
| Kasir | Seluruh layar rumpun ini | Ganti penanggung kunjungan, penanggung per baris, penebusan obat |
| Kepala Kasir / Finance Operations | Sama | **Sama persis** — tidak ada kewenangan tambahan, karena perubahan ini tidak memerlukan persetujuan tahap kedua |
| Admin Master Data | `FE-MPY-06`, `FE-MPY-07` | Rute reimbursement dan aturan tanggungan |
| Finance / Akuntansi | `FE-MPY-05` | — |

### Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka baris, bukan layar kosong |
| Kosong | Kalimat yang menyebut apa yang tidak ada dan apa langkah berikutnya |
| Gagal | Pesan beserta tombol coba lagi; isian yang sudah diketik **MUST** tetap utuh |
| Data basi (`409`) | "Data tagihan telah berubah. Muat ulang data sebelum menyimpan kembali." Layar memuat ulang edit-context; isian yang belum tersimpan **MUST** diberitahukan akan hilang |
| Ditolak aturan bisnis (`422`) | Pesan dari server ditampilkan apa adanya — frontend **MUST NOT** mengarang kalimatnya sendiri |
| Tidak berwenang (`403`) | Gerbang akses ditolak yang sudah ada |
| Pengiriman ganda | Tombol simpan terkunci selama perintah berjalan; kunci idempotensi dikirim mengikuti pola perintah finansial yang sudah ada |
| Pindah mode dengan perubahan belum tersimpan | Konfirmasi buang perubahan lebih dulu |
| Angka finansial | **MUST NOT** diperbarui optimistis sebelum server berhasil. Angka pada layar selalu angka terakhir dari server |

### Pemakaian ulang komponen

| Komponen | Lokasi | Keputusan |
| --- | --- | --- |
| Tombol, tabel, lencana status, modal konfirmasi, peringatan, tumpukan pesan, isian pilihan dan teks | `src/components/features/base-features/` | **Pakai ulang apa adanya** |
| `BasePayerWorkspace` beserta enam ekspor turunannya | `src/components/features/base-features/base-payer-workspace.jsx` | **Pakai ulang sebagai dasar panel Edit Asuransi.** Menutup `MPY-CQ-02`: komponen ini punya **tepat satu** konsumen hari ini, yaitu langkah pembayaran pada admisi Rawat Inap. Setiap penambahan properti **MUST** bersifat opsional berbawaan, supaya konsumen itu tidak berubah perilakunya sama sekali |
| Tabel tagihan pada Menu Pembayaran | `menu-pembayaran-view.jsx` | **Ekstrak bagian presentasionalnya** untuk dipakai bersama. **MUST NOT** menyalin seluruh berkas — ia 1345 baris dan memuat logika pembayaran yang tidak relevan |
| Tombol "Edit Tagihan" yang sudah ada pada Menu Pembayaran | `menu-pembayaran-view.jsx` | **Tidak dipakai ulang.** Tombol itu membuka tambah biaya lain-lain, tujuannya berbeda. Rumpun ini menambahkan jalan masuk tersendiri ke `FE-MPY-01` |

Komponen dasar baru **MUST NOT** dibuat tanpa gerbang keputusan komponen yang berlaku di repository ini.

### Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Keberadaan ketiga mode dan urutannya pada toolbar | Mengikat — diturunkan dari `MPY-DEC-003`, `004`, `009` |
| Subjudul "Ubah penanggung biaya per item" | **Mengikat** — mencegah salah paham yang sudah diperkirakan |
| Isian jumlah obat hanya-baca, tanpa tombol tambah/kurang | **Mengikat** — `MPY-DEC-009` |
| Perbandingan ditampilkan berdampingan pada layar lebar | Mengikat pada layar lebar; pada layar sempit boleh bertumpuk ke bawah |
| Nama butir menu, urutan, ikon, pengelompokan visual | `DEV_DISCRETION` |
| Bentuk panel: tab, modal, atau laci | `DEV_DISCRETION` |
| Warna, jarak, bentuk lencana penanggung | `DEV_DISCRETION` |
| Tombol Edit Penjamin Perusahaan | **MUST NOT dibuat** — perubahan kartu penjamin tetap lewat Data Pasien/Registrasi (`MPY-DEC-003`). Boleh menampilkan keterangan hanya-baca beserta arahan ke mana perubahan dilakukan |

### Acceptance frontend

60. Kasir dapat mengganti penanggung kunjungan dari tunai menjadi asuransi, lalu tagihan menampilkan angka hasil perhitungan server yang baru tanpa memuat ulang halaman secara manual.
61. Kartu penjamin yang masa berlakunya sudah lewat tampil pada daftar dalam keadaan nonaktif beserta alasannya, bukan disembunyikan.
62. Perbandingan berdampingan menampilkan angka yang seluruhnya berasal dari server; tidak ada satu pun selisih yang dihitung di sisi peramban.
63. Pada kunjungan tunai, pilihan Asuransi dan Penjamin pada penanggung per baris tampil nonaktif beserta alasannya.
64. Pada kunjungan rawat inap, tombol Edit Billing tidak aktif beserta keterangan jenis kunjungan.
65. Pada mode Tebus Sebagian, tombol tambah dan kurang jumlah obat tidak muncul sama sekali.
66. Sesudah penanggung kunjungan diganti, kasir melihat pemberitahuan berisi jumlah baris biaya yang penanggungnya ikut dikembalikan menjadi tanggungan pasien.
67. Ringkasan menampilkan "Subtotal Penjamin" pada kunjungan berpenjamin perusahaan dan "Subtotal Asuransi" pada kunjungan berasuransi, dan tidak pernah menampilkan keduanya sekaligus.
68. Menyimpan dengan versi data yang sudah basi menampilkan pesan muat ulang, dan tidak ada perubahan yang tersimpan sebagian.
69. Kedua butir menu master data baru terdaftar dan dapat dijangkau peran yang berwenang.
70. Langkah pembayaran pada admisi Rawat Inap tetap berperilaku sama persis sesudah `BasePayerWorkspace` dipakai ulang di modul ini.

