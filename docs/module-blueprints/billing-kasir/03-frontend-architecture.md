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
