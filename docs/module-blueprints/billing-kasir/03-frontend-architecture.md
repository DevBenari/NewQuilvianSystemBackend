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


---

## Amendment 15 September 2026 — Revisi Petty Cash: satu halaman kerja, pencairan langsung

> Revisi `1.2`, status **approved** (`PC-DEC-026`, 15 September 2026). Masukan: **`PC-DEC-016`–`PC-DEC-025`** (`approved` Product/Domain Owner 15 September 2026) dan keputusan arsitektur `PC-DES-015`–`PC-DES-025` pada [`02-backend-architecture.md`](./02-backend-architecture.md). Frontend SHA diaudit `1f2f2c93c9e4369db6c60246776de4c3bd52b3af`.
>
> **Berbeda dari amendment 7 September**, seluruh layar Petty Cash kini **sudah ada dan berjalan** (`01-existing-capability-map.md` § 20.2). Amendment ini menggabungkan dua halaman menjadi satu, membuang aksi yang tidak berlaku lagi, dan menambah tiga aksi baru.
>
> Amendment ini **tidak** mengunci warna, jarak, ikon, pilihan component library, maupun bentuk wadah presentasi.

### Batas amendment ini

Yang berubah hanya rumpun Petty Cash. Menu Pembayaran, Dokumen Kasir, Running Invoice, Shift Kasir, dan seluruh master data lain **tidak disentuh**. Layar Kategori Petty Cash (`FE-PC-06`–`FE-PC-08`) juga **tidak disentuh sama sekali** (`PC-DEC-020`).

### Kebutuhan layar setelah revisi

| ID layar | Nama | Jenis | Jalan masuk | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-PC-09` | **Petty Cash** — satu halaman kerja | Ringkasan + anggaran + monitoring + riwayat | **Butir menu** (satu-satunya) | **Baru** — menggabungkan `FE-PC-01` dan `FE-PC-05` |
| `FE-PC-01` | Monitoring Voucher Petty Cash | Daftar | **Menjadi panel** di dalam `FE-PC-09` | **Diperbarui** — bukan halaman sendiri lagi |
| `FE-PC-05` | Anggaran Kas Kecil | Panel anggaran | **Menjadi panel** di dalam `FE-PC-09` | **Diperbarui** — bukan halaman sendiri lagi |
| `FE-PC-02` | Buat Permintaan Petty Cash | Isian | Layar anak `FE-PC-09` | **Diperbarui** — judul berubah dari "Buat Voucher" |
| `FE-PC-03` | Bukti Nota/Kasir | Isian nomor nota | Layar anak `FE-PC-09` | Tidak berubah |
| `FE-PC-04` | Detail Voucher | Detail + riwayat perintah | Layar anak `FE-PC-09` | **Diperbarui** — tambah saldo sebelum/sesudah dan riwayat pengembalian |
| `FE-PC-10` | Kembalikan Sisa Uang | Isian nominal + alasan | Layar anak `FE-PC-09` | **Baru** |
| `FE-PC-11` | Batalkan Pencairan | Isian alasan | Layar anak `FE-PC-09` | **Baru** |
| `FE-PC-12` | Kelola Periode Anggaran | Daftar periode + buat/aktifkan/tutup | Layar anak `FE-PC-09` | **Baru** |
| `FE-PC-06`–`FE-PC-08` | Kategori Petty Cash | Master data | Butir menu Master Data | Tidak disentuh |

### Peta butir menu

Dua butir menu menjadi **satu**. Ini konsekuensi langsung `PC-DEC-016`/FE-PC-001 dokumen revisi: monitoring dan anggaran tidak boleh lagi menjadi dua sumber data yang terpisah.

```text
Billing Management                                  <- tingkat 0, sudah ada
├── Running Invoice                                 -> sudah ada
├── Buat Invoice Manual (Testing)                   -> sudah ada
├── Persetujuan Diskon Dokter                       -> sudah ada
├── Shift Kasir                                     -> sudah ada
├── Petty Cash                                      -> .../petty-cash              [DIUBAH]
├── ~~Anggaran Kas Kecil~~                          -> DIHAPUS, menjadi panel      [HAPUS]
└── Master Data                                     <- grup tingkat 1, sudah ada
    └── Kategori Petty Cash                         -> tidak berubah
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Petty Cash | 1 | Billing Management | `/health-services/billing-management/petty-cash` | `FE-PC-09` | `PettyCashVoucher : Read` | **Diubah** — dari `.../petty-cash/vouchers` |
| ~~Anggaran Kas Kecil~~ | — | — | — | — | — | **Dihapus dari sidebar** |
| Kategori Petty Cash | 2 | Master Data | `/health-services/billing-management/master-data/petty-cash-category` | `FE-PC-06` | `PettyCashCategory : Read` | Tidak berubah |

**Route lama MUST tetap hidup sebagai pengalihan**, bukan dihapus (FE-PC-012 dokumen revisi):

| Route lama | Perlakuan |
| --- | --- |
| `/health-services/billing-management/petty-cash/vouchers` | Pengalihan permanen ke `/health-services/billing-management/petty-cash` |
| `/health-services/billing-management/petty-cash/budget` | Pengalihan permanen ke halaman yang sama, dengan panel Anggaran dalam keadaan terbuka |

Alasannya bukan kenyamanan: kedua route itu sudah berjalan di produksi sejak awal September, sehingga penanda halaman dan tautan yang sudah dibagikan petugas akan mati tanpa pengalihan.

**Berkas yang disunting saat implementasi:** `src/utils/menu-sidebar/menu-items.jsx` (satu butir dihapus, satu `pathname` diubah). Pendaftaran butir menu **MUST** menjadi acceptance criteria salah satu task layar.

### Skema fitur — `FE-PC-09` Petty Cash (halaman gabungan)

```text
+- Petty Cash ---------------------------------------------- FE-PC-09 -+
|  Periode: 1 Sep 2026 - 30 Sep 2026   [Kelola Periode]  (Finance)      |
|                                                                       |
|  +-----------+ +-----------+ +-----------+ +-----------+ +---------+  |
|  | ANGGARAN  | | SALDO     | | PEMAKAIAN | | SISA      | | MENUNGGU|  |
|  | PERIODE   | | SAAT INI  | | PERIODE   | | ANGGARAN  | | BUKTI   |  |
|  | 10.000.000| | 4.700.000 | | 5.300.000 | | 4.700.000 | |    3    |  |
|  +-----------+ +-----------+ +-----------+ +-----------+ +---------+  |
|                                                                       |
|  [+ Buat Permintaan] (Kasir)   [Tambah Saldo] (Kasir)                 |
|                                                                       |
|  +- Monitoring Permintaan ------------------------------------------+ |
|  | Filter: periode | status | kategori | penerima | cari            | |
|  |------------------------------------------------------------------| |
|  | No.Voucher | Penerima | Kategori | Nominal | Status | Aksi       | |
|  | PTC-...001 | Budi     | Transport| 300.000 | Menunggu Bukti | ... | |
|  +------------------------------------------------------------------+ |
|                                                                       |
|  +- Riwayat Pergerakan Anggaran ------------------------------------+ |
|  | Waktu | Jenis | Nominal | Saldo Sebelum | Saldo Sesudah | Pelaku | |
|  +------------------------------------------------------------------+ |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses tombol | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Kepala halaman | Periode aktif, tombol Kelola Periode | `GET /budget/overview` | `PettyCashBudget : Create`/`Activate`/`Close` untuk tombolnya | "Belum ada periode anggaran aktif. Finance perlu membuat periode lebih dulu." beserta tombol Kelola Periode | "Gagal memuat anggaran. Coba muat ulang." |
| Lima kartu ringkasan | Anggaran periode, saldo, pemakaian, sisa, jumlah menunggu bukti | `GET /budget/overview` — **satu panggilan**, layar **MUST NOT** menjumlahkannya sendiri dari daftar | — | Seluruh kartu menampilkan `Rp 0` / `0` | Kartu menampilkan tanda hubung, bukan `Rp 0` — supaya gagal-memuat tidak terbaca sebagai saldo nol |
| Tombol aksi | Buat Permintaan, Tambah Saldo | — | `PettyCashVoucher : Create`, `PettyCashBudget : TopUp` | — | Tombol nonaktif selama pengiriman berlangsung |
| Monitoring Permintaan | Daftar voucher berhalaman beserta filter | `GET /vouchers` | Aksi per baris mengikuti `availableActions` dari server | "Belum ada permintaan petty cash pada periode ini." | "Gagal memuat daftar permintaan." beserta tombol coba lagi |
| Riwayat Pergerakan Anggaran | Ledger berhalaman | `GET /budget/movements` | `PettyCashBudget : Read` | "Belum ada pergerakan anggaran." | "Gagal memuat riwayat pergerakan." |

**Aksi per baris pada Monitoring Permintaan**, seluruhnya digerakkan `availableActions` dari server — layar **MUST NOT** menyimpulkannya sendiri dari status:

| Status baris | Aksi yang muncul | Hak akses |
| --- | --- | --- |
| `Menunggu Pencairan` | Cairkan, Batalkan, Detail | `Disburse`, `Cancel` |
| `Menunggu Bukti` | Input Nota, Kembalikan Sisa, Batalkan Pencairan, Detail | `AttachProof`, `Return`, `Reverse` |
| `Selesai` | Koreksi Nota, Kembalikan Sisa, Batalkan Pencairan, Detail | `AttachProof`, `Return`, `Reverse` |
| `Dibatalkan (Uang Dikembalikan)` | Detail saja | `Read` |
| `Ditolak (arsip)` | Detail saja | `Read` |

### Penanda status

| Label | Kapan | Catatan |
| --- | --- | --- |
| `Menunggu Pencairan` | Permintaan dibuat, uang belum keluar | Menggantikan `Menunggu Persetujuan` |
| `Menunggu Bukti` | Uang sudah keluar, nota belum masuk | **Label berubah** dari `Uang Diterima` untuk kode yang sama |
| `Selesai` | Nota sudah masuk | Tidak berubah |
| `Dibatalkan (Uang Dikembalikan)` | Pencairan dibalik | Baru |
| `Dibatalkan` | `isCancelled` bernilai benar | Penanda terpisah, bukan status — tetap seperti sebelumnya |
| `Ditolak (arsip)` | Hanya pada baris sebelum 15 September 2026 | Baru; menandai data warisan |

`Disetujui` dan `Ditolak` sebagai label aktif **MUST dihapus** dari seluruh layar, begitu pula tombol Setujui dan Tolak beserta pemanggilan endpointnya.

### Skema fitur — `FE-PC-12` Kelola Periode Anggaran

```text
+- Kelola Periode Anggaran --------------------------------- FE-PC-12 -+
|  [+ Buat Periode]                                                     |
|  Periode              | Plafon     | Saldo     | Status  | Aksi       |
|  1-30 Sep 2026        | 10.000.000 | 4.700.000 | Aktif   | Tutup      |
|  1-31 Okt 2026        | 12.000.000 |         0 | Draf    | Aktifkan   |
|  1-31 Agu 2026        |  8.000.000 |         0 | Ditutup | Detail     |
+-----------------------------------------------------------------------+
```

| Wilayah | Sumber data | Hak akses | Keadaan gagal |
| --- | --- | --- | --- |
| Daftar periode | `GET /budget/periods` | `PettyCashBudget : Read` | "Gagal memuat daftar periode." |
| Buat Periode | `POST /budget/periods` | `PettyCashBudget : Create` | Pesan `BIL-VAL-102` ditampilkan apa adanya dari server |
| Aktifkan | `POST /budget/periods/{id}/activate` | `PettyCashBudget : Activate` | Pesan `BIL-VAL-103` |
| Tutup | `POST /budget/periods/{id}/close` | `PettyCashBudget : Close` | Pesan `BIL-VAL-104` |

**Dialog Tutup Periode MUST menampilkan sisa saldo dan meminta periode penerus** ketika sisanya lebih besar dari nol, disertai kalimat yang menyatakan sisa itu akan dipindahkan — bukan hilang. Tanpa kalimat itu, Finance tidak punya cara mengetahui ke mana uangnya pergi.

### Penanganan keadaan

| Keadaan | Perlakuan |
| --- | --- |
| Memuat | Kartu ringkasan dan kedua tabel memuat mandiri; kegagalan salah satu **MUST NOT** mengosongkan yang lain |
| Kosong | Kalimat per wilayah seperti tabel di atas, bukan satu halaman kosong |
| Gagal | Pesan dari server ditampilkan apa adanya; kode `BIL-VAL-*` sudah berbahasa pengguna |
| Data basi | Setiap aksi yang berhasil **MUST** memuat ulang `GET /budget/overview` **dan** daftar yang terdampak dari server — saldo **MUST NOT** dihitung di layar |
| Pengiriman ganda | Tombol dinonaktifkan selama permintaan berjalan **dan** setiap `POST` membawa `Idempotency-Key`; keduanya, bukan salah satu |
| Perubahan bersamaan | `409` ditampilkan sebagai "Data sudah diubah petugas lain. Halaman dimuat ulang." lalu halaman dimuat ulang otomatis |

### Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Penggabungan dua halaman menjadi satu | **Dikunci** — `PC-DEC-016`, FE-PC-001 dokumen revisi |
| Route kanonik dan pengalihan route lama | **Dikunci** — FE-PC-012 |
| Sumber data tiap wilayah | **Dikunci** — tabel skema fitur di atas |
| Label status | **Dikunci** — `contracts/state-transition-matrix.md` |
| Hak akses tiap tombol | **Dikunci** — `contracts/permission-audit-matrix.md` |
| Urutan wilayah di halaman | **Dikunci** longgar — ringkasan di atas, monitoring di tengah, riwayat di bawah (PRD § 14 dokumen revisi) |
| Bentuk wadah tiap panel (kartu, tab, laci, akordeon) | `DEV_DISCRETION` |
| Warna, jarak, ikon, tipografi, component library | `DEV_DISCRETION` |
| Bentuk `FE-PC-10`/`FE-PC-11` (modal atau laci) | `DEV_DISCRETION` |
| Cara menampilkan saldo sebelum/sesudah pada `FE-PC-04` | `DEV_DISCRETION` selama keduanya terbaca |

### Berkas frontend yang terdampak

| Berkas | Perlakuan |
| --- | --- |
| `src/app/health-services/billing-management/petty-cash/page.jsx` | **Baru** — halaman kanonik |
| `src/app/health-services/billing-management/petty-cash/vouchers/page.jsx` | Menjadi pengalihan |
| `src/app/health-services/billing-management/petty-cash/budget/page.jsx` | Menjadi pengalihan |
| `src/components/view/health-services/billing-management/petty-cash/petty-cash-vouchers-view.jsx` | **Dipakai ulang sebagai panel**, bukan ditulis ulang |
| `.../petty-cash/petty-cash-budget-view.jsx` | **Dipakai ulang sebagai panel** |
| `.../petty-cash/create-voucher-modal.jsx`, `attach-proof-modal.jsx`, `top-up-budget-modal.jsx`, `adjust-budget-modal.jsx`, `voucher-detail-modal.jsx` | Dipakai ulang; `voucher-detail-modal` diperbarui |
| `.../petty-cash/return-voucher-modal.jsx`, `reverse-voucher-modal.jsx`, `budget-period-modal.jsx` | **Baru** |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js`, `use-petty-cash-budget.js` | Diperbarui — aksi approve/reject dibuang, aksi baru ditambah |
| `src/lib/state/slice/.../petty-cash-voucher-slice.jsx`, `petty-cash-budget-slice.jsx` | Diperbarui |
| `src/lib/hooks/.../petty-cash/petty-cash-voucher-constants.js` | Diperbarui — kosakata status dan label |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui — satu butir dihapus, satu `pathname` diubah |

Logika data yang sudah ada **MUST dipakai ulang**, bukan ditulis ulang di halaman baru. Modul ini sudah punya preseden mahalnya: rumus Subtotal/Pajak pada Menu Pembayaran tiga kali menyimpang karena ditulis ulang di tempat berbeda (lihat catatan pada `billing-invoice-calculation-breakdown.js`).

---

# Amendment 18 September 2026 — Penutupan gap `FINAL`→`CLOSED`

`revisi blueprint 1.3` · status **approved** (`BKC-DEC-105`, 18 September 2026) · input: **`BKC-DEC-100`–`BKC-DEC-102`**; keputusan arsitektur `BKC-DES-028`–`BKC-DES-035`.

## Nol layar baru, nol perubahan source frontend yang diwajibkan

Amendment ini **tidak menuntut satu pun perubahan pada source frontend**, dan itu kesimpulan berbukti, bukan perkiraan. Pemeriksaan langsung pada `billing-invoice-constants.js` (`01-existing-capability-map.md` § 21) menemukan status `CLOSED` **sudah** terdaftar di dua tempat yang dibutuhkan:

| Yang sudah ada | Akibatnya |
| --- | --- |
| `CLOSED` pada daftar opsi filter status tagihan | Kasir sudah dapat menyaring tagihan tertutup tanpa perubahan apa pun |
| `CLOSED` pada peta badge status, memakai token warna yang sudah dipakai badge lain modul ini | Badge tampil benar begitu backend mulai mengirim nilai itu |

Perubahan yang dirasakan pengguna karena itu datang **seluruhnya dari backend**: tagihan yang sudah dibayar lunas berhenti tampil sebagai `Final` dan mulai tampil sebagai `Closed`, dan kolom waktu penutupan yang selama ini selalu kosong mulai terisi.

## Satu butir yang MUST diverifikasi saat implementasi, bukan dirancang ulang

`MODULE-STATUS.md` mencatat bahwa `FE-BKC-009` pernah punya gap "`isFinal` tidak mencakup status `CLOSED`" yang **sudah** diperbaiki 30 Agustus 2026. Perbaikan itu lahir ketika `CLOSED` praktis tidak pernah muncul pada alur normal, sehingga cakupannya belum pernah teruji terhadap invoice yang **baru** berpindah ke `CLOSED`.

Yang MUST diverifikasi (pembacaan source, bukan perancangan ulang):

1. Setiap tempat yang memakai `isFinal` atau pemeriksaan status setara untuk **mengunci** aksi penyuntingan tagihan memperlakukan `CLOSED` sama seperti `FINAL`. Bila ada yang terlewat, layar akan menawarkan tombol sunting pada tagihan yang backend pasti tolak.
2. Layar yang menampilkan waktu penutupan tidak mengandaikan nilainya selalu kosong.
3. Tagihan yang **kembali** dari `CLOSED` ke `FINAL` (`BKC-DES-031`) tampil kembali pada daftar tagihan yang masih punya sisa — ini otomatis bila daftarnya menyaring berdasarkan data backend, dan hanya bermasalah bila ada penyaringan yang dihitung sendiri di sisi layar.

Ketiganya adalah pekerjaan verifikasi pada task frontend, dan **MUST NOT** dijadikan alasan menulis ulang komponen atau hook yang sudah berjalan.

## Kewenangan UI

Nihil yang baru. Amendment ini tidak menambah menu, route, tab, modal, maupun kontrol baru, sehingga tidak ada ruang `DEV_DISCRETION` yang perlu dibuka. Kosakata label yang tampil ke pengguna (`Open`, `Final`, `Closed`, `Settled by Write-off`) **MUST** tetap memakai peta label yang sudah ada — **MUST NOT** diterjemahkan ulang menjadi "Lunas" atau istilah lain di satu layar saja, karena label yang berbeda-beda antar layar untuk status yang sama adalah sumber kebingungan yang mahal ditelusuri.

> **Catatan bagi pembaca yang mencari kata "Lunas".** Badge `Lunas`/`Cicilan` yang sudah ada pada Riwayat Pembayaran dan Kwitansi **bukan** status invoice — ia dihitung dari akumulasi pembayaran, dan sudah bekerja sejak sebelum amendment ini. Keduanya hidup berdampingan dan menjawab pertanyaan berbeda: badge `Lunas` menjawab "berapa yang sudah dibayar", status `Closed` menjawab "apakah tagihan ini masih berjalan". Amendment ini **tidak** menggabungkan keduanya dan **tidak** mengubah badge yang sudah ada.

Trace **`BKC-DEC-100`–`102`**, `BKC-DES-028`–`035`.

---

## Amendment 21 September 2026 — Layar pemeriksaan surat ke modul konsumen

Masukan `BKC-DEC-106`–`109`. Status **draft**.

### Mengapa layar ini ada

`BKC-DEC-108` memutuskan Billing mencatat surat yang belum diambil konsumen dan membuatnya dapat
diperiksa — tetapi tidak menggantungkan perilakunya pada itu. Keterlihatan hanya berguna bila
ada permukaannya. Tanpa layar ini, keputusan itu berhenti sebagai kolom di tabel yang tidak
pernah dilihat siapa pun.

### Kebutuhan layar

| ID | Layar | Kegunaan |
| --- | --- | --- |
| `BIL-SCR-41` | Surat ke Modul Konsumen | Melihat surat yang belum diambil Finance maupun Farmasi, dan mencatat pengakuan penerimaan bila diperlukan pemulihan |

Satu layar, tanpa layar anak. Tidak ada layar baru bagi petugas Farmasi maupun kasir — keduanya
tidak memakai permukaan ini sama sekali.

### Peta butir menu

| Butir menu | Tingkat | Induk | Route | Layar | Butir hak akses |
| --- | --- | --- | --- | --- | --- |
| Surat ke Modul Konsumen | 2 | Billing dan Kasir | `/billing/consumer-handoffs` | `BIL-SCR-41` | `BillingConsumerHandoff : Read` |

Penempatan butir di bawah induk yang sudah ada mengikuti pola modul ini; urutan persisnya dan
penamaan yang tampil di layar adalah `DEV_DISCRETION` selama butir hak aksesnya tepat.

### Skema fitur layar `BIL-SCR-41`

```text
┌──────────────────────────────────────────────────────────────┐
│  Surat ke Modul Konsumen                                     │
├──────────────────────────────────────────────────────────────┤
│  [ Jenis: Semua ▾ ]  [ Dari tanggal ]  [ Sampai tanggal ]    │  ← A
├──────────────────────────────────────────────────────────────┤
│  Jenis   │ Tujuan   │ Terbit        │ Rujukan   │ Tindakan   │  ← B
│  ────────┼──────────┼───────────────┼───────────┼─────────── │
│  Terima  │ Keuangan │ 21 Sep 09:12  │ INV-00123 │ [ Akui ]   │
│  Resep   │ Farmasi  │ 21 Sep 09:12  │ INV-00123 │ [ Akui ]   │
├──────────────────────────────────────────────────────────────┤
│  Menampilkan 2 dari 2 surat yang belum diambil               │  ← C
└──────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Hak akses tombol | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| A — Penyaring | Jenis surat, rentang tanggal terbit | Dipilih pengguna; tidak ada endpoint tersendiri | — | — | Rentang terbalik ditolak sebelum dikirim |
| B — Daftar | Jenis, modul tujuan, waktu terbit, rujukan tagihan, tombol akui | `GET /consumer-handoffs/pending` | Tombol akui: `BillingConsumerHandoff : Acknowledge` | "Tidak ada surat yang menggantung. Seluruh fakta sudah diambil kedua modul." | "Daftar surat tidak dapat dimuat. Coba muat ulang." |
| C — Ringkasan | Jumlah surat yang belum diambil | Dihitung dari hasil yang sama | — | Menampilkan nol | Disembunyikan bila daftar gagal dimuat |

Keadaan kosong di sini adalah **kabar baik**, bukan kegagalan. Kalimatnya sengaja menyatakan itu
supaya petugas tidak menyangka layarnya rusak.

### Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md`, tidak dikarang ulang.

| Peran | Melihat daftar | Mengakui surat |
| --- | :---: | :---: |
| Petugas Billing / Kepala Kasir | Ya | Tidak |
| Finance Operations | Ya | Ya |
| Administrator sistem | Ya | Ya |
| Petugas Farmasi | Tidak | Tidak |

Tombol akui **MUST** disembunyikan, bukan sekadar dinonaktifkan, bagi peran yang tidak berwenang.

### Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka daftar; penyaring tetap dapat diubah |
| Kosong | Kalimat pada tabel di atas, tanpa nada kegagalan |
| Gagal | Pesan beserta tombol muat ulang; penyaring tidak hilang |
| Data basi | Daftar dimuat ulang setelah pengakuan berhasil, karena baris yang diakui hilang dari daftar |
| Pengiriman ganda | Tombol akui dikunci sampai jawabannya kembali. Pengakuan kedua atas surat yang sama dijawab penolakan yang ramah, bukan galat merah |

### Yang sengaja tidak dibuat di frontend

| Yang wajar diharapkan | Alasan tidak dibuat |
| --- | --- |
| Tombol menerbitkan ulang surat | Penerbitan **MUST NOT** dapat dipicu manusia. Surat yang dapat diterbitkan manual adalah surat yang dapat dipalsukan |
| Tombol menghapus surat | Baris bersifat tetap (`BKC-DEC-109`) |
| Layar keadaan clearance resep bagi petugas Farmasi | Keadaan itu tampil di layar kerja Farmasi sendiri, dirancang pada blueprint Farmasi. Billing tidak membuat layar untuk modul lain |
| Peringatan otomatis surat menggantung | Ambang waktu dan penerimanya belum diputuskan (`BKC-OQ-101`) |

### Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Keberadaan layar, isi wilayah, sumber data, hak akses tombol | Terkunci dokumen ini |
| Bunyi keadaan kosong dan gagal | Terkunci dokumen ini — keduanya menyangkut kejelasan bagi petugas |
| Urutan butir menu, penamaan tampilan, warna, jarak, ikon, component library | `DEV_DISCRETION` |

Trace `BKC-DEC-106`–`109`, `BKC-DES-036`–`041`.

---

# Amendment 24 September 2026 — Integrasi Rawat Inap: Layar Clearance Handoffs & Rincian Tagihan Ranap

> Revision `1.5`, status **draft**. Input: `BKC-DEC-112`–`119`, `BKC-AC-080`–`087`, `BKC-DES-042`–`050`. Layar terdampak: `consumer-handoffs-view.jsx` (perluasan tab Rawat Inap) dan `menu-pembayaran-view.jsx` / `invoice-detail-view.jsx` (rincian sewa kamar & badge financial clearance).

## 1. Peta Butir Menu

| Tingkat | Induk | Butir menu | Route target | Layar yang dituju | Hak akses yang menjaga |
| :---: | --- | --- | --- | --- | --- |
| 2 | Billing Management | Surat ke Modul Konsumen | `/health-services/billing-management/consumer-handoffs` | `consumer-handoffs-view.jsx` | `[AccessPermission("BillingConsumerHandoff", "Read")]` |
| 2 | Billing Management | Kasir & Pembayaran | `/health-services/billing-management/cashier/payment` | `menu-pembayaran-view.jsx` | `[AccessPermission("BillingPayment", "Read")]` |
| 2 | Billing Management | Detail Tagihan Pasien | `/health-services/billing-management/invoices/[id]` | `invoice-detail-view.jsx` | `[AccessPermission("BillingInvoice", "Read")]` |

*Catatan: Seluruh menu di atas sudah ada; amendment ini memperkaya fitur pada layar tersebut tanpa menambah butir menu baru pada sidebar.*

## 2. Skema Fitur Layar — Tab Rawat Inap pada Surat ke Modul Konsumen

Layar ini (`consumer-handoffs-view.jsx`) diperluas untuk menampilkan surat fakta kelayakan pemulangan rawat inap (`BilInpatientClearanceHandoff`) yang menggantung atau berstatus pembatalan (`REVOKED`).

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  Surat ke Modul Konsumen                                                               │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  [ Tab: Semua | Keuangan | Farmasi | Rawat Inap (2) ]                                  │  ← A
│  Penyaring: [ Status: Semua ▾ ] [ Tanggal: Hari Ini ▾ ]                                │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  Jenis        │ Tujuan     │ Pasien / Encounter │ Status Kelayakan │ Sisa Tagihan │ Aksi   │  ← B
│  ─────────────┼────────────┼────────────────────┼──────────────────┼──────────────┼─────── │
│  Izin Pulang  │ Rawat Inap │ Ny. Dewi / ENC-091 │ [ CLEARED ]      │ Rp 0         │ [Akui] │
│  Izin Pulang  │ Rawat Inap │ Tn. Agus / ENC-084 │ [ REVOKED ]      │ Rp 450.000   │ [Detail│
│               │            │ (Tagihan Susulan)  │                  │              │ /Cek]  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  Menampilkan 2 surat kelayakan rawat inap aktif                                        │  ← C
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### Tabel Wilayah Layar

| Wilayah | Isi | Sumber data | Hak akses tombol | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| A — Tab & Filter | Tab penyaring target modul (tambah tab "Rawat Inap"), filter status (`ALL`, `CLEARED`, `BLOCKED`, `REVOKED`) | State lokal komponen | — | — | — |
| B — Tabel Daftar Surat | Kolom: Jenis Surat, Tujuan (`Rawat Inap`), Data Pasien/Encounter, Status Kelayakan dengan Badge Berwarna, Sisa Tagihan Pasien, Tombol Aksi | `GET /consumer-handoffs/pending?targetModule=INPATIENT` dan `GET /invoices/encounter/{encounterId}/inpatient-summary` | Tombol Akui: `BillingConsumerHandoff:Acknowledge`; Tombol Evaluasi Ulang: `BillingInpatient:Clearance` | "Tidak ada surat kelayakan rawat inap yang menggantung. Seluruh izin pulang telah diakui bangsal." | "Gagal memuat daftar surat rawat inap. Periksa koneksi jaringan." |
| C — Ringkasan Footer | Total antrean surat dan status sinkronisasi | Kalkulasi lokal | — | Menampilkan nol | Disembunyikan bila gagal |

## 3. Skema Fitur Layar — Panel Ringkasan Rawat Inap pada Detail Tagihan

Pada layar rincian pembayaran rawat inap (`menu-pembayaran-view.jsx`), ditambahkan panel informasi khusus rawat inap:

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  STATUS KELAYAKAN PEMULANGAN (FINANCIAL CLEARANCE): [ CLEARED / LAYAK PULANG ]        │  ← D
├────────────────────────────────────────────────────────────────────────────────────────┤
│  Rincian Sewa Kamar:                                                                   │  ← E
│  - Kamar Kelas 1 (Bed 102) | 10 Okt 22:30 s/d 11 Okt 08:00 (Masuk Malam: 20%)  Rp200rb│
│  - ICU (Bed ICU-02)        | 11 Okt 08:00 s/d 12 Okt 10:00 (Pro-rata)         Rp2.400rb│
│  Biaya Administrasi Rawat Inap (7% - Maksimal Rp6.000.000):                Rp1.820.000 │
│  Kredit Biaya Administrasi Rajal (Pengurang):                             - Rp  50.000 │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  Konsolidasi Pelayanan Gawat Darurat (IGD):                                            │  ← F
│  - [IGD] Tindakan Resusitasi & Oksigenasi                                  Rp  650.000 │
│  - [IGD] Infus RL & Obat Emergensi                                         Rp  280.000 │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### Tabel Wilayah Panel Rawat Inap

| Wilayah | Isi | Sumber Data | Hak Akses | Keterangan & Perilaku |
| --- | --- | --- | --- | --- |
| D — Banner Clearance | Status kelayakan: Hijau (`CLEARED`), Merah (`BLOCKED`), Merah Kuning (`REVOKED` - Tagihan Susulan Masuk) | `GET /invoices/encounter/{encounterId}/inpatient-summary` | Read: Kasir & Petugas Ranap | Memperingatkan kasir jika pasien tertahan karena sisa tagihan atau tagihan susulan |
| E — Rincian Kamar & Admin | Durasi sewa kamar bertingkat (diskon jam malam, pro-rata menit), biaya admin 7% (cap Rp6 jt), dan kredit admin rajal | DTO `InpatientBillingSummaryResponse` | Read: Kasir | Memberikan transparansi perhitungan biaya kamar yang adil bagi keluarga pasien |
| F — Rincian Alihan IGD | Daftar tindakan/obat yang diberikan di IGD sebelum rawat inap dengan penanda khusus `[IGD]` | `BilInvoiceItem` dengan `SourceDomain = "EMERGENCY"` | Read: Kasir | Non-destruktif; kwitansi memisahkan pos biaya gawat darurat dan rawat inap |

## 4. Aksi per Peran

| Peran | Melihat Status Clearance | Menjalankan Evaluasi Ulang | Menerima Pembayaran Ranap | Mengakui Surat Handoff |
| --- | :---: | :---: | :---: | :---: |
| Kasir Rawat Inap | Ya | Ya | Ya | Tidak |
| Perawat Bangsal Ranap | Ya (di bangsal) | Ya (permintaan) | Tidak | Ya (konfirmasi pulang) |
| Kepala Kasir / Supervisor | Ya | Ya | Ya | Ya |
| Pasien / Keluarga | Melihat di kwitansi | Tidak | Tidak | Tidak |

## 5. Penanganan Keadaan Khusus (Exception Handling)

| Keadaan | Penanganan pada Antarmuka Frontend |
| --- | --- |
| Tagihan Susulan Masuk (`REVOKED`) | Banner status otomatis berganti merah berkedip dengan teks: *"PERINGATAN: Tagihan susulan baru saja tercatat setelah pasien dinyatakan lunas. Izin pulang dicabut otomatis. Minta pasien menyelesaikan sisa tagihan."* |
| Deposit Tindakan Besar Kurang (`BIL-VAL-121`) | Modal peringatan kuning muncul saat kasir/dokter memeriksa verifikasi deposit: *"Deposit tindakan besar belum terpenuhi. Pasien memiliki kekurangan setoran porsi tanggung jawab sebesar Rp X.XXX.XXX."* |
| Perpindahan Kamar >1 Kali di Hari yang Sama | Tabel rincian kamar menampilkan durasi jam:menit riil masing-masing kamar beserta label *"Perhitungan Pro-Rata Durasi Hunian"*. |
| Alihan Rajal ke Ranap | Biaya admin rajal ditampilkan tercoret (Rp 0) dengan keterangan *"Digantikan Biaya Admin Ranap"*, dan jika sudah dibayar muncul baris kredit hijau pengurang total tagihan. |

## 6. Kewenangan UI (UI Authority)

| Komponen | Status Kewenangan |
| --- | --- |
| Badge warna status clearance (Hijau: `CLEARED`, Merah: `BLOCKED`, Kuning: `REVOKED`) | Terkunci dokumen ini |
| Penampilan breakdown sewa kamar jam masuk dan pro-rata | Terkunci dokumen ini |
| Penempatan layout kartu rincian, animasi transisi, dan typography | `DEV_DISCRETION` |

Trace `BKC-DEC-112`–`119`, `BKC-AC-080`–`087`, `BKC-DES-042`–`050`.



# Amendment 24 September 2026 — Revisi UI Billing: Filter, Default, Asuransi, Diskon Dokter, Refund (Revisi 1.6)

| Field | Nilai |
|---|---|
| Basis keputusan | `00-interview-decisions.md` — `BUI-DEC-001`–`015` |
| Basis bukti | `01-existing-capability-map.md` bagian 23 |
| Backend yang dipakai | `02-backend-architecture.md` — amendment revisi 1.6 (`BUI-DES-001`, `002`) |
| Sifat | **Pertahankan component existing, ubah hanya bagian yang diperlukan** — instruksi eksplisit owner |

Hierarki wewenang yang dipakai di seluruh amendment ini:
`security/privacy/invariant → keputusan bisnis owner (BUI-DEC-*) → konvensi project → DEV_DISCRETION`.

## 1. Peta Layar yang Disentuh

| Layar | File | Keputusan yang berlaku |
|---|---|---|
| Daftar Billing | `billing-invoices-view.jsx` (via `billing-invoices-client.jsx`) | `BUI-DES-005` |
| Edit Asuransi | `edit-asuransi-panel.jsx` (di dalam `edit-tagihan-view.jsx`) | `BUI-DES-003`, `BUI-DES-004` |
| Edit Status Tagihan | `edit-tagihan-view.jsx` (blok `payerKindChoices`) | `BUI-DES-003` (nilai default ikut `BUI-DES-001` backend) |
| Card Billing & Status Tagihan | Komponen kartu ringkasan pada `edit-tagihan-view.jsx`/layar invoice | `BUI-DES-007` |
| Catatan Penting | Baru — lokasi ditentukan `DEV_DISCRETION` saat implementasi | `BUI-DES-008` (`OPEN DECISION` sumber data) |
| Form Diskon Dokter (Apply Discount) | `menu-pembayaran-view.jsx` | `BUI-DES-009` |
| Terminologi Drug/Obat | `edit-tagihan-view.jsx` + penyisiran ulang menyeluruh | `BUI-DES-006` |
| Modal Ajukan Refund | `create-refund-modal.jsx` | `BUI-DES-010`, `BUI-DES-011` |
| Menu Pembayaran (sumber tombol aksi) | `menu-pembayaran-view.jsx`, `billing-financial-exception-panel.jsx` | `BUI-DES-012` |
| Riwayat Pembayaran (target tombol aksi) | `payment-history-view.jsx` | `BUI-DES-012` |

## 2. `BUI-DES-003` — Payment Method: Sumber Data dan Layout

**Sumber data (`BUI-CQ-04` tertutup `BUI-DEC-015`):** `PaymentMethodRow[]` dari response
`GET /{id}/edit-context`, **bukan** `panel.categories` (hook `useEditAsuransiPanel`).

**Kontrak data yang dipakai:**

| Field | Tipe | Kegunaan |
|---|---|---|
| `Code` | `"CASH" \| "INSURANCE" \| "COMPANY_GUARANTOR"` | Kunci tombol |
| `Label` | `string` | Teks tombol — sudah Indonesia ("Tunai", "Asuransi", "Penjamin Perusahaan") |
| `IsSelected` | `bool` | Menentukan tombol mana yang tersorot — mengikuti `BUI-DES-001` otomatis |
| `IsEnabled` | `bool` | Menentukan tombol dapat diklik atau tidak |

**Layout (`BUI-DEC-006`):** Komponen baru `EditAsuransiPaymentMethodRow` (nama sementara,
`DEV_DISCRETION`) merender ketiga baris `PaymentMethodRow` sebagai grid/flex **3 kolom** dalam
satu baris. **Bukan** memodifikasi `.categorySelector` pada `base-payer-workspace.module.css`
(grid 2 kolom yang sudah ada) — komponen itu tetap dipakai apa adanya untuk keperluan aslinya
(memilih kandidat perbandingan, lihat bagian 3), sesuai batas `BUI-DEC-015`.

**Yang MUST NOT terjadi:**
- Frontend MUST NOT menghitung ulang "item mana yang tercover" sendiri untuk menentukan tombol
  terpilih. `IsSelected` dibaca apa adanya.
- Klik tombol MUST tetap memicu alur simpan yang sudah ada (`PUT /{id}/payment-source`) —
  amendment ini tidak mengubah cara penyimpanan, hanya cara tampil dan sumber nilai default.

## 3. `BUI-DES-004` — Filter Perbandingan Asuransi

**Target:** blok `optionsForSelectedCategory` pada `edit-asuransi-panel.jsx`, saat
`selectedCategory === "INSURANCE"`.

**Aturan filter (`BUI-DEC-004`):** dari `AvailablePayerOptions` (sudah dikembalikan lengkap oleh
backend, sudah mengecualikan asuransi aktif pasien per `BUI-DEC-005`/`CAP-BUI-05`), tampilkan
**hanya** baris dengan `PayerType === "INSURANCE"`.

```text
Sebelum filter: [CASH, INSURANCE(Prudential), INSURANCE(AXA), COMPANY_GUARANTOR(PT Maju)]
Sesudah filter (kategori "Asuransi" dipilih): [Prudential, AXA]
```

**Catatan penting:** filter ini HANYA berlaku pada tampilan **daftar kandidat perbandingan**.
Kategori `CASH` dan `COMPANY_GUARANTOR` **tetap ada** sebagai pilihan `selectedCategory` itu
sendiri pada `BasePayerCategorySelector` (mengganti penjamin ke Tunai atau ke Penjamin
Perusahaan tetap mungkin) — yang disaring hanya isi daftar KETIKA kategori Asuransi yang
sedang aktif dipilih, sesuai batas yang dicatat `BUI-DEC-004`.

`BUI-DEC-005` **tidak butuh perubahan kode apa pun** — backend sudah mengecualikan asuransi
aktif sebelum data sampai ke frontend (`CAP-BUI-05`).

## 4. `BUI-DES-005` — Filter Tanggal dan Default Data Billing

**Endpoint:** `GET /invoices` (sudah ada), parameter `StartDate`, `EndDate`, `Status` (sudah ada
pada `BillingInvoiceQuery`).

**State awal (`BUI-DEC-002`):**

```js
{ startDate: today(), endDate: today(), status: "OPEN" }
```

Nilai ini MUST menjadi query pertama yang dikirim saat layar dibuka — **bukan** query kosong
yang lalu difilter di klien.

**Komponen filter (`BUI-DEC-001`):** dua input tanggal ditambahkan pada baris filter yang sudah
ada di `billing-invoices-view.jsx`, mengikuti pola komponen filter tanggal yang sudah dipakai
layar Billing lain di modul ini (`DEV_DISCRETION` untuk komponen tanggal persisnya — date picker
yang sudah dipakai project, bukan komponen baru).

**Perilaku:** mengubah filter tanggal MUST menggantikan default hari ini (bukan menambahkannya),
sesuai `BUI-DEC-002`.

## 5. `BUI-DES-006` — Penggantian Label Drug → Obat / Medicine

Cakupan (`BUI-DEC-003`, dengan `Assumption` yang dicatat di sana): label UI saja.

| Lokasi ditemukan pass ini | Tindakan |
|---|---|
| `edit-tagihan-view.jsx` | Ganti string "Drug" → "Obat / Medicine" |

**MUST disisir ulang saat implementasi** (di luar cakupan pencarian pass trace yang hanya
menyisir `components/view` dan `app` dengan word-boundary): `hooks/`, `utils/`, konstanta
label lain, placeholder form, judul kolom tabel yang mungkin memakai string "Drug" dalam bentuk
lain (mis. dalam template literal atau i18n key) yang tidak tertangkap pencarian kata utuh.

**Yang MUST NOT berubah:** nilai data master obat (`DrugName`/nama obat pada
`MstDrug`/`DrugBillingDispositionItemResponse`) — field-field itu adalah IDENTIFIER kode, bukan
teks tampilan, dan tetap dipetakan apa adanya dari backend (`DrugName`, `MedicineName`, `Obat`
tiga alias yang sudah tersedia di `DrugBillingDispositionItemResponse` — frontend bebas memilih
alias mana yang dipakai untuk merender, ketiganya bernilai sama).

## 6. `BUI-DES-007` — Card Billing dan Card Status Tagihan Compact

Murni tata letak. Tidak ada perubahan data, endpoint, atau state. Kebutuhan:

| Kebutuhan | Detail |
|---|---|
| Kurangi whitespace | `DEV_DISCRETION` — padding/margin komponen kartu existing |
| Datatable naik ke atas | Urutan render dalam kartu diubah; komponen tabel itu sendiri (`BillingInvoiceItemsTable`) **dipakai ulang apa adanya**, tidak dibuat ulang |
| Responsive dipertahankan | MUST diuji pada breakpoint mobile/tablet/desktop yang sudah jadi standar project |

## 7. `BUI-DES-008` — Catatan Penting: Container Siap, Sumber Data `OPEN DECISION`

**Status: sebagian desain, `BUI-CQ-05` belum tertutup.** Bagian ini TIDAK memblokir sisa
amendment — `BUI-DEC-009` boleh masuk gelombang implementasi belakangan setelah `BUI-CQ-05`
dijawab, gelombang lain berjalan tanpa menunggunya.

**Yang DIDESAIN sekarang:** kontrak komponen presentasi (bentuk data yang diterimanya), bukan
sumber datanya.

```js
// Kontrak komponen — bentuk data yang DIHARAPKAN, sumber pengisinya belum diputuskan
{
  timelineEntries: [
    { stage: "KIOSK", label: "Kiosk", timestamp, note },
    { stage: "ADMISI", label: "Admisi", timestamp, note },
    { stage: "IGD", label: "IGD", timestamp, note },
    { stage: "RAWAT_INAP", label: "Rawat Inap", timestamp, note },
  ]
}
```

**Yang TIDAK didesain sekarang, dan sebabnya (lihat `02-backend-architecture.md` bagian 8):**
tidak ditemukan satu pun endpoint yang mengonsolidasikan note pasien lintas tahap kunjungan.
Mewujudkannya sesuai spesifikasi literal (Kiosk→Admisi→IGD→Rawat Inap dalam satu timeline)
menuntut kemampuan backend baru yang melintasi bounded context di luar Billing — di luar batas
scope yang dikunci `00-interview-decisions.md` untuk pass ini.

**Rekomendasi untuk `BUI-CQ-05`:** desain lanjutan (di luar amendment ini) sebaiknya menunggu
`hospital-domain-architect` bila cakupan penuh (lintas modul) yang dikehendaki, ATAU
dipersempit ke sumber yang sudah ada per modul (mis. hanya note dari encounter Billing sendiri
bila ada) sebagai langkah pertama yang lebih kecil.

## 8. `BUI-DES-009` — Upload Memo Dokter TTD

**Field yang dipakai:** `ApplyDiscountRequest.DoctorDiscountMemoFile` (`string?`, sudah ada,
belum pernah dikonsumsi frontend).

**Validasi wajib (`BUI-DEC-010`):**

| Aturan | Lapis |
|---|---|
| Tombol submit form Apply Discount MUST nonaktif/tertolak selama `DoctorDiscountMemoFile` kosong | Frontend, `BUI-VAL-01` (lihat `contracts/validation-matrix.md`) |
| Pesan yang ditampilkan: "Memo Dokter TTD wajib diunggah sebelum pengajuan diskon dapat dikirim." | Frontend |

**Komponen upload:** adaptasi dari pola `UploadPhotoField.jsx`
(`src/components/features/UplodFoto/`) — **dipakai ulang strukturnya** (file input + preview +
tombol hapus + integrasi `react-hook-form`), **bukan** dipakai langsung (komponen sumber
khusus gambar JPG/PNG dengan fitur webcam yang tidak relevan di sini). Komponen baru
`DoctorDiscountMemoUpload` (nama sementara) MUST:

- Menerima tipe file dokumen memo (PDF dan/atau gambar hasil pindai — `DEV_DISCRETION` tipe
  MIME persis, mengikuti konvensi upload dokumen lain di project bila ada preseden)
- Preview: nama file + ikon/thumbnail, bukan wajib render gambar penuh
- Tombol Hapus: mengosongkan nilai field, mengembalikan form ke keadaan tervalidasi wajib

**Status: `BUI-CQ-06` belum tertutup — mekanisme UPLOAD SESUNGGUHNYA (bagaimana `File` menjadi
`string` yang dikirim) `OPEN DECISION`.** Endpoint upload ditandai `Rencana (belum tersedia)`
pada `contracts/api-contract.md`. Komponen frontend MUST dibangun dengan titik integrasi upload
sebagai **fungsi yang disuntikkan** (prop `onUpload`), bukan hardcode ke satu endpoint tertentu
— sehingga saat `BUI-CQ-06` terjawab, penyambungannya adalah pengisian satu fungsi, bukan
menulis ulang komponen.

**Risiko yang dicatat (dari `01-existing-capability-map.md` `CAP-BUI-10`):** validasi wajib ini
MURNI di frontend. Backend TIDAK menegakkan `[Required]` pada `DoctorDiscountMemoFile` — jalur
API langsung (di luar UI) tetap bisa mengirim pengajuan tanpa memo. Ini **bukan** cacat yang
diperbaiki amendment ini (di luar scope `BUI-DEC-010` yang eksplisit membatasi diri ke UI), tapi
MUST dicatat sebagai rekomendasi terbuka untuk pass keamanan/backend terpisah.

## 9. `BUI-DES-010` dan `BUI-DES-011` — Modal Ajukan Refund

### 9.1 `BUI-DES-010` — Refundable Credit Lama Digantikan, Bukan Disembunyikan Terpisah

Dropdown "Refundable Credit" tunggal pada `create-refund-modal.jsx` **dihapus sebagai bagian
dari penggantian seluruh isi modal** oleh `BUI-DES-011`, bukan pekerjaan tersendiri. `props`
`refundableCredits`/`refundableCreditsLoading`/`refundableCreditsError`/`refundableCreditId`
pada modal lama **tidak dipakai lagi** oleh bentuk baru.

### 9.2 `BUI-DES-011` — Bentuk Baru Modal

**Kontrak data yang dipakai (seluruhnya sudah ada di backend, `CAP-BUI-12`):**

| Kebutuhan | Endpoint | Dipanggil kapan |
|---|---|---|
| Daftar item billing yang bisa direfund | `GET /{id}/refundable-items` → `BillingRefundableItemResponse[]` (kini + `TransactionDate`, `BUI-DES-002`) | Saat radio "Billing" dipilih |
| Sisa deposito | `GET /{id}/remaining-deposit` → `RemainingDepositResponse` | Saat radio "Deposito" dipilih |
| Kirim pengajuan | `POST /{id}/refunds` dengan `CreateRefundRequest` | Saat submit |

**Struktur form baru:**

```text
( ) Billing          ( ) Deposito
────────────────────────────────
[Jika Billing dipilih]
☐ | Nama Item        | Tanggal     | Nominal
☐ | Konsultasi Dokter | 20 Sep 2026 | Rp 350.000
☑ | Obat Amoxicillin  | 20 Sep 2026 | Rp 75.000
────────────────────────────────
Total Refund: Rp 75.000   ← otomatis, jumlah baris tercentang

[Jika Deposito dipilih]
Sisa Deposito: Rp 2.500.000
Nominal Refund: Rp 2.500.000   ← otomatis, tidak dapat diketik manual melebihi ini

Alasan: [textarea, tetap seperti sekarang]
```

**Pemetaan ke `CreateRefundRequest`:**

| Field form | Field request |
|---|---|
| Radio "Billing" dipilih | `RefundCategory = "BILLING"` |
| Radio "Deposito" dipilih | `RefundCategory = "DEPOSITO"` |
| Checkbox item tercentang | `SelectedBillingItemIds = [...]` |
| Total refund (dihitung otomatis) | `RequestedAmount` |
| Alasan | `Reason` |

**Yang MUST NOT terjadi:**
- Frontend MUST NOT mengetik ulang total secara manual saat sumber "Billing" — nilai `Total
  Refund` adalah HASIL penjumlahan baris tercentang, bukan input bebas (mengunci
  `BillingRefundableItemResponse.RefundableAmount` per baris sebagai satu-satunya sumber
  nominal, `BUI-DEC-012`).
- Frontend MUST NOT mengizinkan nominal refund Deposito melebihi
  `RemainingDepositResponse.RemainingDepositAmount`.
- Frontend MUST NOT membuat gate persetujuan baru berbeda antar sumber — keduanya memakai jalur
  persetujuan `POST /{id}/refunds` yang sama persis (`BUI-DEC-012`).

## 10. `BUI-DES-012` — Pemindahan Tombol Aksi ke Riwayat Pembayaran

**Sumber (dihapus):** blok pemicu `openRefund`/`openAdjustment`/`openWriteOff` pada
`menu-pembayaran-view.jsx` yang merender `BillingFinancialExceptionPanel`.

**Target (ditambahkan):** kolom/section baru **"Aksi"** pada `payment-history-view.jsx`,
**terpisah** dari kolom `"Kwitansi"` yang sudah ada di sana (baris ~197, cetak struk — konsep
berbeda, TIDAK disentuh).

**Yang dipakai ulang tanpa perubahan:**

| Aset | Dipakai ulang untuk |
|---|---|
| `create-refund-modal.jsx` (bentuk baru per `BUI-DES-011`) | Trigger "Ajukan Refund" |
| `create-adjustment-modal.jsx` | Trigger "Ajukan Adjustment" |
| Modal write-off yang sudah ada (referensi: `WRITE_OFF_*` konstanta pada `billing-financial-exception-constants.js`) | Trigger "Ajukan Write-Off" |
| `use-billing-financial-exception.js` (hook) | Seluruh state/handler `openRefund`/`openAdjustment`/`openWriteOff`, `refreshCases`, dsb — dipindah pemanggilannya ke `payment-history-view.jsx`, isi hook TIDAK diubah |

**Yang MUST diverifikasi saat implementasi:** hook `use-billing-financial-exception.js`
kemungkinan menerima `invoiceId`/context dari scope `menu-pembayaran-view.jsx` — MUST dipastikan
`payment-history-view.jsx` punya akses context yang sama (invoice/encounter yang sedang dilihat)
sebelum hook dipindah, supaya pemanggilannya tidak pincang.

**Yang MUST NOT terjadi:** kolom "Kwitansi" yang sudah ada MUST tidak berubah perilakunya sama
sekali — ini murni penambahan kolom/section baru di sebelahnya.

## 11. Aksi per Peran (Ringkasan)

| Peran | Aksi baru yang terlihat |
|---|---|
| Kasir | Filter tanggal Billing; default hari ini status OPEN; payment method horizontal; perbandingan hanya asuransi; upload memo saat mengajukan diskon dokter; modal refund dua sumber; tombol refund/adjustment/write-off kini di Riwayat Pembayaran (bukan lagi di Menu Pembayaran) |
| Dokter (pengaju diskon) | Wajib mengunggah memo sebelum mengajukan diskon |
| Supervisor/Finance (penyetuju refund/adjustment/write-off) | Tidak berubah — alur persetujuan sama, hanya lokasi tombol pemicu yang berpindah |

## 12. Penanganan Keadaan Layar

| Keadaan | Perilaku |
|---|---|
| Daftar item refundable kosong (semua sudah pernah diajukan) | Tampilkan pesan eksplisit, bukan tabel kosong tanpa keterangan — pola `InformationAlert` yang sudah dipakai modal lama |
| Sisa deposito Rp 0 | Radio "Deposito" tetap dapat dipilih, nominal tampil Rp 0, submit MUST ditolak (nominal harus > 0, aturan existing `CreateRefundRequest.RequestedAmount` `Range("0.01", ...)`) |
| Memo diunggah lalu dihapus sebelum submit | Tombol submit kembali nonaktif (`BUI-DEC-010` butir "remove") |
| Filter tanggal Billing menghasilkan nol invoice | Tampilkan pesan kosong yang jelas, bukan tabel kosong polos |
| Kartu penjamin pasien belum ada (Edit Asuransi kategori Asuransi/Penjamin dipilih tanpa kartu terdaftar) | **Sudah ditangani** — `InformationAlert` "Pasien ini belum memiliki kartu penjamin terdaftar" (existing, tidak berubah) |

## 13. Angka yang MUST NOT Dihitung Frontend

| Angka | Sumber wajib |
|---|---|
| Status default payment method (`IsSelected`) | `PaymentMethodRow` dari server (`BUI-DES-003`) |
| Total Refund sumber Billing | Jumlah `RefundableAmount` baris tercentang, bukan hitungan ulang manual |
| Nominal Refund sumber Deposito | `RemainingDepositResponse.RemainingDepositAmount` apa adanya |
| Status coverage per item ("tercover"/"tidak") | `calculation.Breakdown.Items` / `ItemPayerAssignments` dari server — sudah berlaku sebelumnya, tidak berubah amendment ini |

## 14. Matriks `DEV_DISCRETION`

| Keputusan | Wewenang | Catatan |
|---|---|---|
| Komponen date picker untuk filter tanggal Billing | `DEV_DISCRETION` | Ikuti pola yang sudah dipakai layar Billing lain |
| Tipe MIME persis yang diterima upload memo (PDF saja / PDF+gambar) | `DEV_DISCRETION` | Sampai ada preseden project yang lebih spesifik |
| Nama komponen baru (`EditAsuransiPaymentMethodRow`, `DoctorDiscountMemoUpload`, dst) | `DEV_DISCRETION` | Nama sementara pada dokumen ini bukan penetapan final |
| Bentuk kartu compact (susunan elemen persis) | `DEV_DISCRETION` | `BUI-DEC-008` — pertahankan component existing |
| Lokasi visual "Catatan Penting" pada layar | `DEV_DISCRETION` | Menunggu `BUI-CQ-05` untuk isinya; penempatan visual tidak menunggu |

## 15. Ketergantungan pada Backend

| Kebutuhan frontend | Menunggu |
|---|---|
| `BUI-DES-003` (default tombol benar) | `BUI-DES-001` (backend) — **MUST selesai lebih dulu**, kalau tidak tombol akan menampilkan default yang salah walau UI sudah benar |
| `BUI-DES-011` (kolom Tanggal pada datatable refund) | `BUI-DES-002` (backend) |
| Sisanya (`BUI-DES-004`–`007`, `009` bagian validasi, `010`, `012`) | Nol dependency backend baru — seluruhnya memakai endpoint yang sudah ada |

---

# Amendment 13 Oktober 2026 — Pelunasan Gabungan Lintas Kunjungan (Revisi 1.9)

> `revisi blueprint`: `1.9`, status **approved** (14 Oktober 2026). Masukan: `BKC-DEC-118`, `BKC-AC-091`, `BKC-DEC-142`, `BKC-DEC-143`,
> `BKC-DES-057`–`BKC-DES-062`. Frontend SHA diaudit: `ae216044`.
> Endpoint yang dipakai: lihat `contracts/api-contract.md` amendment revisi 1.9 — ketiganya masih
> berlabel **Rencana (belum tersedia)**, sehingga pekerjaan frontend **MUST** menunggu backend.

## 1. Kebutuhan Layar

| ID layar | Nama layar | Status | Peran sasaran |
|---|---|---|---|
| `BIL-SCR-43` | Pelunasan Gabungan Lintas Kunjungan | **Baru** | Kasir, Supervisor Kasir |
| `BIL-SCR-42` | Menu Pembayaran (sudah ada) | **Diperbarui** | Kasir, Supervisor Kasir |

## 2. Peta Butir Menu

`BIL-SCR-43` **sengaja tidak mendapat butir menu tersendiri**, dan dinyatakan sebagai **layar
anak**:

| Layar | Jenis | Jalan masuknya | Route | Butir hak akses |
|---|---|---|---|---|
| `BIL-SCR-43` Pelunasan Gabungan | Layar anak | Dari `BIL-SCR-42` Menu Pembayaran, lewat tombol "Bayar Gabungan dengan Tagihan Lain" | `/health-services/billing-management/billing/invoices/[slug]/pembayaran-gabungan` | `BillingConsolidatedSettlement : Read` untuk membuka, `BillingConsolidatedSettlement : Create` untuk memproses |

Alasan tidak diberi butir menu: layar ini **selalu** butuh satu tagihan jangkar sebagai titik
awal. Butir menu tingkat atas akan membuka layar tanpa tagihan jangkar, yaitu keadaan yang tidak
punya makna. Pola ini sama dengan Menu Pembayaran dan Edit Tagihan yang juga layar anak dari
tagihan.

Nol perubahan pada `src/utils/menu-sidebar/menu-items.jsx`. Ini disebutkan eksplisit karena
pernah terjadi lima halaman `billing-management` selesai namun tak terjangkau sampai
`FE-BKC-MENU-001` dikerjakan sebagai task tersendiri — pada amendment ini keterjangkauan dijamin
lewat tombol di layar induk, dan pemasangan tombol itu **MUST** menjadi acceptance criteria task
`BIL-SCR-43`, bukan pekerjaan yang menganggur.

Nama tombol, ikon, penempatan, serta apakah layar ini dibuka sebagai halaman penuh, modal, atau
drawer tetap `DEV_DISCRETION`.

## 3. Skema Fitur `BIL-SCR-43` — Pelunasan Gabungan

```text
+- Pelunasan Gabungan - Ny. Sari Mandasari ------------------ BIL-SCR-43 -+
| Tagihan jangkar: INV-RANAP-000412   Unit: RANAP                       |
| Sisa tagihan jangkar: Rp 8.000.000                                    |
+-----------------------------------------------------------------------+
| Tagihan lain pasien ini yang masih bersisa:                           |
| [v] No. Tagihan        | Unit  | Terbit      | Sisa        | Hubungan |
| [v] INV-IGD-000311     | IGD   | 09 Okt 08:15| 1.500.000   | Rujukan  |
| [ ] INV-RAJAL-000120   | RAJAL | 02 Okt 09:30|   250.000   | Pasien sama |
+-----------------------------------------------------------------------+
| Urutan pelunasan (ditetapkan sistem, tagihan terbit paling awal dulu):|
|   1. INV-IGD-000311  (IGD)    2. INV-RANAP-000412 (RANAP)             |
+-----------------------------------------------------------------------+
| Total sisa terpilih: Rp 9.500.000                                     |
| Pembayaran:  [Metode v] [Nominal ............]  [+ Tambah metode]     |
| Total dibayar: Rp 6.000.000                                           |
+-----------------------------------------------------------------------+
| Perkiraan hasil:                                                      |
|   INV-IGD-000311   -> lunas          (teralokasi Rp 1.500.000)        |
|   INV-RANAP-000412 -> sisa 3.500.000 (teralokasi Rp 4.500.000)        |
+-----------------------------------------------------------------------+
| memuat -> kerangka baris pada daftar tagihan, bukan layar kosong      |
| kosong -> "Pasien ini tidak punya tagihan lain yang masih bersisa."   |
|           [Kembali ke pembayaran biasa]                               |
| gagal  -> "Daftar tagihan gagal dimuat."            [Coba lagi]       |
+--------------------------- [Batal] [Proses Pembayaran Gabungan] ------+
```

### Tabel wilayah `BIL-SCR-43`

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | Nama pasien, nomor dan unit tagihan jangkar, sisa tagihan jangkar | `GET /invoices/{id}` (sudah ada) | `BillingInvoice : Read` | Gagal → seluruh layar diganti pesan beserta tombol coba lagi |
| Daftar tagihan lain | Nomor tagihan, unit layanan, tanggal terbit, sisa, dan keterangan hubungan | `GET /invoices/{id}/consolidation-candidates` | `BillingConsolidatedSettlement : Read` | Kosong → "Pasien ini tidak punya tagihan lain yang masih bersisa." beserta tautan kembali ke pembayaran biasa. Gagal → "Daftar tagihan gagal dimuat." beserta tombol coba lagi |
| Keterangan hubungan | Teks terbaca: "Rujukan" (`REFERRAL_CHAIN`), "Tautan tercatat" (`RECORDED_LINK`), atau "Pasien sama" (`SAME_PATIENT`) | Field `RelationType` pada response yang sama | `BillingConsolidatedSettlement : Read` | — |
| Urutan pelunasan | Urutan yang ditetapkan sistem, hanya ditampilkan, **tidak dapat diubah petugas** | Dihitung dari `InvoiceOpenedAt` pada response kandidat | `BillingConsolidatedSettlement : Read` | — |
| Bagian pembayaran | Metode pembayaran dan nominal, boleh lebih dari satu metode | `MstPaymentMethod` lewat endpoint master yang sudah dipakai Menu Pembayaran | `BillingConsolidatedSettlement : Create` | Gagal memuat metode → bagian pembayaran dinonaktifkan beserta pesan, daftar tagihan tetap terbaca |
| Perkiraan hasil | Simulasi alokasi per tagihan sebelum diproses | **Dihitung di sisi klien** dari sisa tagihan dan urutan pelunasan | `BillingConsolidatedSettlement : Read` | Bila nominal belum diisi, bagian ini disembunyikan |
| Tombol Proses | Mengirim permintaan pembentukan pembayaran gabungan | `POST /consolidated-settlements` | `BillingConsolidatedSettlement : Create` | Tombol **MUST** disembunyikan bagi petugas tanpa hak `Create`, bukan ditampilkan lalu ditolak kode `403` |
| Tombol Batal | Kembali ke Menu Pembayaran tanpa menyimpan apa pun | — | — | — |

> **Catatan "Perkiraan hasil".** Angka ini dihitung di sisi klien sebagai **bantuan baca**, dan
> **MUST NOT** diperlakukan sebagai kebenaran. Yang sah adalah hasil dari backend pada response
> `POST`. Bila keduanya berbeda — misalnya karena biaya susulan masuk di sela waktu — layar
> **MUST** menampilkan hasil dari backend, bukan perkiraan tadi.

## 4. Perubahan pada `BIL-SCR-42` Menu Pembayaran

| Yang berubah | Isi | Sumber data | Butir hak akses |
|---|---|---|---|
| Satu tombol baru | "Bayar Gabungan dengan Tagihan Lain", menuju `BIL-SCR-43` | — | `BillingConsolidatedSettlement : Read` |
| Penanda jumlah tagihan lain | Angka kecil pada tombol, menandai ada berapa tagihan lain yang masih bersisa | `GET /invoices/{id}/consolidation-candidates` | `BillingConsolidatedSettlement : Read` |

Tombol ini **MUST** disembunyikan bila daftar kandidat kosong, agar kasir tidak membuka layar yang
pasti kosong. Seluruh bagian lain Menu Pembayaran — termasuk jalur pembayaran satu tagihan —
**MUST NOT** berubah sama sekali (`BKC-DES-061`).

## 5. Aksi per Peran

Diturunkan dari `contracts/permission-audit-matrix.md` amendment revisi 1.9, bukan dikarang ulang.

| Peran | Membuka `BIL-SCR-43` | Memilih tagihan peserta | Memproses pembayaran | Mencetak ulang kwitansi |
|---|:---:|:---:|:---:|:---:|
| Kasir | Ya | Ya | Ya | Ya |
| Supervisor Kasir | Ya | Ya | Ya | Ya |
| Petugas Billing (tanpa loket) | Ya | Tidak | Tidak | Ya |
| Finance / AR | Ya | Tidak | Tidak | Tidak |
| Perawat bangsal | Tidak | Tidak | Tidak | Tidak |

## 6. Penanganan Keadaan

| Keadaan | Perilaku yang mengikat |
|---|---|
| Memuat | Daftar tagihan menampilkan kerangka baris. Tombol Proses dinonaktifkan selama pemuatan |
| Kosong | "Pasien ini tidak punya tagihan lain yang masih bersisa." beserta tautan kembali ke pembayaran biasa |
| Gagal | "Daftar tagihan gagal dimuat." beserta tombol coba lagi. Pilihan tagihan yang sudah ditandai **MUST** dipertahankan setelah percobaan ulang berhasil |
| Data basi | Bila backend menolak dengan kode `409` karena sisa tagihan berubah (`BIL-VAL-137`), layar menampilkan "Sisa tagihan sudah berubah karena ada biaya baru yang masuk. Muat ulang daftar tagihan lalu ulangi." beserta tombol muat ulang. Layar **MUST NOT** memproses ulang secara otomatis — keluarga pasien perlu diberi tahu lebih dulu bahwa nominalnya berubah |
| Tagihan dipegang loket lain | Bila backend menolak dengan kode `409` karena `BIL-VAL-138`, baris tagihan itu ditandai tidak dapat dipilih beserta keterangannya, dan kasir tetap dapat melanjutkan dengan tagihan lain |
| Pengiriman ganda | Tombol Proses dinonaktifkan sejak permintaan dikirim sampai response diterima. `IdempotencyKey` dibentuk **sekali** saat layar dibuka dan dipakai ulang pada percobaan ulang, supaya penekanan dua kali tidak menghasilkan dua pembayaran |
| Pembayaran sebagian | Status `PARTIALLY_SETTLED` **MUST** ditampilkan sebagai keadaan **normal**, bukan galat merah. Bunyinya menyebut tagihan mana yang masih bersisa dan berapa |

## 7. Privasi dan Aksesibilitas

| Hal | Ketentuan |
|---|---|
| Privasi | Layar ini menampilkan nominal rupiah lintas kunjungan dan hanya untuk peran loket serta Billing/Finance. Nominal **MUST NOT** terbawa ke layar perawat bangsal, mengikuti aturan yang sudah berlaku sejak `BKC-DEC-117` |
| Aksesibilitas | Daftar tagihan memakai tabel bertanda kepala kolom yang benar, setiap kotak pilih punya label terbaca pembaca layar yang menyebut nomor tagihan dan unit layanannya, dan pesan galat ditautkan ke bagian yang bersangkutan. Pemindaian aksesibilitas otomatis masih belum dapat dijalankan — keadaan yang sudah tercatat sejak `FE-BKC-010` dan tidak diselesaikan amendment ini |
| Responsif | Pada lebar telepon, tabel tagihan beralih menjadi daftar kartu. Bagian "Urutan pelunasan" dan "Perkiraan hasil" **MUST** tetap terbaca karena keduanya yang dijelaskan kasir kepada keluarga pasien |

## 8. Kewenangan UI

| Keputusan | Kewenangan |
|---|---|
| Keberadaan setiap wilayah layar dan sumber datanya | **Mengikat** — dikunci skema di atas |
| Hak akses yang menjaga setiap tombol | **Mengikat** — dari `contracts/permission-audit-matrix.md` |
| Bunyi pesan kosong, gagal, dan data basi | **Mengikat** — sejalan `contracts/validation-matrix.md` |
| Urutan pelunasan tidak dapat diubah petugas | **Mengikat** — `BKC-DES-060` |
| Halaman penuh, modal, atau drawer | `DEV_DISCRETION` |
| Nama tombol, ikon, warna, jarak, dan component library | `DEV_DISCRETION` |
| Letak penanda jumlah tagihan lain pada Menu Pembayaran | `DEV_DISCRETION` |

## 9. Dependency Pengerjaan

| Kebutuhan frontend | Menunggu |
|---|---|
| Seluruh `BIL-SCR-43` | **Ketiga endpoint backend** pada `contracts/api-contract.md` revisi 1.9 — masih **Rencana (belum tersedia)**. Frontend **MUST NOT** dimulai sebelum backend tersedia, karena nol endpoint existing yang dapat dipakai sebagai pengganti |
| Tombol pada `BIL-SCR-42` | `GET /invoices/{id}/consolidation-candidates` |
| Seeder hak akses `BillingConsolidatedSettlement` | **MUST** terpasang lebih dulu, kalau tidak seluruh layar akan ditolak kode `403` meski kodenya sudah benar |


---

# Amendment 14 Oktober 2026 — Kelayakan Pulang Lintas Kunjungan (Revisi 2.0)

> `revisi blueprint`: `2.0`, status **approved** (14 Oktober 2026). Masukan: `BKC-DEC-144`, `BKC-AC-094`,
> `BKC-DES-063`–`BKC-DES-069`. Frontend SHA diaudit: `6e5edd6df`.
> Endpoint yang dipakai: lihat `contracts/api-contract.md` amendment revisi `2.0` — dua endpoint baru
> masih berlabel **Rencana (belum tersedia)**, sehingga pekerjaan frontend **MUST** menunggu backend.

## 1. Kebutuhan Layar

| ID layar | Nama layar | Status | Peran sasaran |
|---|---|---|---|
| `BIL-SCR-44` | Rekonsiliasi Rangkaian Kunjungan | **Baru** | Petugas Billing, Supervisor Kasir |
| `BIL-SCR-41` | Consumer Handoffs Kasir (sudah ada) | **Diperbarui** | Kasir, Supervisor Kasir |

## 2. Peta Butir Menu

`BIL-SCR-44` **sengaja tidak mendapat butir menu tersendiri**, dan dinyatakan sebagai **layar anak**:

| Layar | Jenis | Jalan masuknya | Route | Butir hak akses |
|---|---|---|---|---|
| `BIL-SCR-44` Rekonsiliasi Rangkaian Kunjungan | Layar anak | Dari `BIL-SCR-41` Consumer Handoffs, lewat baris pasien berstatus perlu rekonsiliasi | `/health-services/billing-management/billing/consumer-handoffs/[slug]/rekonsiliasi-kunjungan` | `BillingInpatient : Read` untuk membuka, `BillingInpatient : Reconcile` untuk memutuskan |

Alasan tidak diberi butir menu: layar ini selalu membutuhkan satu surat kelayakan sebagai titik
awal. Butir menu tingkat atas akan membukanya tanpa konteks, yaitu keadaan yang tidak punya makna.

Nol perubahan pada `src/utils/menu-sidebar/menu-items.jsx`. Keterjangkauan dijamin lewat baris pada
layar induk, dan pemasangan jalan masuk itu **MUST** menjadi acceptance criteria task `BIL-SCR-44`.

Nama tombol, ikon, penempatan, serta apakah layar ini dibuka sebagai halaman penuh, modal, atau
drawer tetap `DEV_DISCRETION`.

## 3. Skema Fitur `BIL-SCR-44` — Rekonsiliasi Rangkaian Kunjungan

```text
+- Rekonsiliasi Rangkaian Kunjungan - Ny. Sari Mandasari ---- BIL-SCR-44 -+
| Status kelayakan: PERLU REKONSILIASI                                   |
| Sebab: Penelusuran berhenti pada batas aman                            |
| Kedalaman tertelusuri: 5 tingkat   Tagihan ditemukan: 4                |
+------------------------------------------------------------------------+
| Kewajiban yang ditemukan:                                              |
| Tkt | No. Tagihan        | Unit  | Sisa Pasien  | Dihitung | Aksi       |
|  0  | INV-RANAP-000412   | RANAP |  3.500.000   | Ya       | [Kecualikan]|
|  1  | INV-IGD-000311     | IGD   |          0   | Ya       | [Kecualikan]|
|  2  | INV-RAJAL-000120   | RAJAL |    250.000   | Ya       | [Kecualikan]|
|  3  | INV-RAJAL-000087   | RAJAL |    900.000   | Tidak    | [Ikutkan]   |
|     |  ^ pasien berbeda                                                |
+------------------------------------------------------------------------+
| Total yang menahan kepulangan: Rp 3.750.000                            |
+------------------------------------------------------------------------+
| memuat -> kerangka baris pada tabel kewajiban                          |
| kosong -> "Tidak ada kewajiban yang ditemukan pada rangkaian ini."     |
| gagal  -> "Rincian kewajiban gagal dimuat."        [Coba lagi]         |
+------------------------- [Kembali] [Selesaikan Rekonsiliasi] ----------+
```

### Tabel wilayah `BIL-SCR-44`

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | Nama pasien, status kelayakan, sebab perlu rekonsiliasi, kedalaman tertelusuri, jumlah tagihan ditemukan | `GET /inpatient-clearance/encounter/{encounterId}/obligations` | `BillingInpatient : Read` | Gagal → seluruh layar diganti pesan beserta tombol coba lagi |
| Tabel kewajiban | Tingkat rangkaian, nomor tagihan, unit layanan, sisa tagihan pasien, penanda dihitung atau tidak, dan alasan bila tidak dihitung | Field `obligations` pada response yang sama | `BillingInpatient : Read` | Kosong → "Tidak ada kewajiban yang ditemukan pada rangkaian ini." |
| Tombol per baris | Ikutkan atau Kecualikan satu tagihan | `POST /inpatient-clearance/{id}/resolve-reconciliation` | `BillingInpatient : Reconcile` | Tombol **MUST** disembunyikan bagi petugas tanpa hak `Reconcile`, bukan ditampilkan lalu ditolak kode `403` |
| Kolom alasan | Wajib diisi saat mengikutkan atau mengecualikan | Dikirim pada field `Reason` | `BillingInpatient : Reconcile` | Kosong → tombol Selesaikan dinonaktifkan beserta keterangan bahwa alasan wajib diisi |
| Total | Jumlah kewajiban yang dihitung | Field `totalOutstanding` | `BillingInpatient : Read` | — |
| Tombol Selesaikan | Mengirim keputusan rekonsiliasi | `POST /inpatient-clearance/{id}/resolve-reconciliation` | `BillingInpatient : Reconcile` | Dinonaktifkan selama permintaan berjalan |
| Tombol Kembali | Kembali ke Consumer Handoffs tanpa menyimpan apa pun | — | — | — |

> **Catatan yang mengikat.** Baris dengan penanda tidak dihitung **MUST** tetap ditampilkan beserta
> alasannya, bukan disembunyikan. Petugas perlu tahu sistem menemukan tagihan itu dan sengaja tidak
> memakainya — menyembunyikannya membuat petugas menyimpulkan sistem melewatkannya.

## 4. Perubahan pada `BIL-SCR-41` Consumer Handoffs Kasir

| Yang berubah | Isi | Sumber data | Butir hak akses |
|---|---|---|---|
| Nilai status baru | Baris pasien dapat berstatus perlu rekonsiliasi, dibedakan dari tertahan | Field `clearanceStatus` pada response handoff yang sudah ada | `BillingInpatient : Read` |
| Jalan masuk ke `BIL-SCR-44` | Baris berstatus perlu rekonsiliasi dapat dibuka | — | `BillingInpatient : Read` |
| Kolom jumlah tagihan | Menampilkan berapa tagihan yang dinilai pada rangkaian | Field `ChainInvoiceCount` | `BillingInpatient : Read` |

Status perlu rekonsiliasi **MUST** dibedakan secara visual dari status tertahan, karena tindakan
petugasnya berbeda: tertahan berarti menagih uang, perlu rekonsiliasi berarti memeriksa data lebih
dulu. Cara membedakannya — warna, ikon, atau label — tetap `DEV_DISCRETION`.

## 5. Proyeksi untuk Perawat Bangsal

Ini bagian paling sensitif pada amendment ini, dan aturannya **mengikat**, bukan `DEV_DISCRETION`.

| Yang **boleh** dilihat perawat | Yang **MUST NOT** dilihat perawat |
|---|---|
| Status kelayakan, termasuk nilai perlu rekonsiliasi | Nominal rupiah apa pun |
| Daftar unit layanan yang masih punya penghambat, misalnya "ada tagihan belum lunas di IGD" | Sisa tagihan per unit |
| Penanda bahwa penghambat berasal dari unit lain | Nomor tagihan dan rincian layanannya |

Alasannya: perawat perlu tahu **mengapa** pasien belum boleh pulang agar tidak memulangkannya, tetapi
nominal tagihan bukan haknya. Aturan ini berlaku sejak `BKC-DEC-117` dan tidak dilonggarkan di sini.
Pengujiannya eksplisit pada `BIL-AT-175`.

## 6. Aksi per Peran

Diturunkan dari `contracts/permission-audit-matrix.md` amendment revisi `2.0`.

| Peran | Membuka `BIL-SCR-44` | Melihat rincian rupiah | Memutuskan rekonsiliasi |
|---|:---:|:---:|:---:|
| Supervisor Kasir | Ya | Ya | Ya |
| Petugas Billing | Ya | Ya | **Tidak** (`BKC-DEC-150`) |
| Kasir | Ya | Ya | **Ya** (`BKC-DEC-150`) |
| Finance / AR | Ya | Ya | Tidak |
| Perawat bangsal | Tidak | **Tidak** | Tidak |

## 7. Penanganan Keadaan

| Keadaan | Perilaku yang mengikat |
|---|---|
| Memuat | Tabel kewajiban menampilkan kerangka baris. Tombol keputusan dinonaktifkan selama pemuatan |
| Kosong | "Tidak ada kewajiban yang ditemukan pada rangkaian ini." Layar tetap menampilkan sebab perlu rekonsiliasi |
| Gagal | "Rincian kewajiban gagal dimuat." beserta tombol coba lagi |
| Data basi | Bila backend menolak dengan kode `409` karena surat sudah berpindah versi, layar menampilkan bahwa penilaian sudah diperbarui lalu memuat ulang. Keputusan yang belum terkirim **MUST NOT** dikirim diam-diam ke versi baru |
| Pengiriman ganda | Tombol Selesaikan dinonaktifkan sejak permintaan dikirim. `IdempotencyKey` dibentuk sekali saat layar dibuka dan dipakai ulang pada percobaan ulang |
| Status berubah saat layar terbuka | Bila penilaian ulang terjadi di latar dan statusnya sudah bukan perlu rekonsiliasi, layar memberi tahu bahwa rekonsiliasi tidak lagi dibutuhkan dan menawarkan kembali ke layar induk |

## 8. Kewenangan UI

| Keputusan | Kewenangan |
|---|---|
| Keberadaan setiap wilayah layar dan sumber datanya | **Mengikat** |
| Baris tidak dihitung tetap ditampilkan beserta alasannya | **Mengikat** |
| Nominal rupiah tidak pernah masuk proyeksi perawat | **Mengikat** — privasi, `BKC-DEC-117` |
| Alasan wajib diisi sebelum keputusan dikirim | **Mengikat** — `BIL-VAL-144` |
| Status perlu rekonsiliasi dibedakan dari tertahan | **Mengikat** pada keberadaan pembedanya; bentuk pembedanya `DEV_DISCRETION` |
| Halaman penuh, modal, atau drawer | `DEV_DISCRETION` |
| Warna, ikon, jarak, dan component library | `DEV_DISCRETION` |

## 9. Dependency Pengerjaan

| Kebutuhan frontend | Menunggu |
|---|---|
| Seluruh `BIL-SCR-44` | **Dua endpoint backend** pada `contracts/api-contract.md` revisi `2.0` — masih **Rencana (belum tersedia)** |
| Tombol keputusan pada `BIL-SCR-44` | **Terjawab** — `BKC-DEC-150`: tombol tampil untuk Kasir dan Supervisor Kasir |
| Proyeksi perawat bangsal | `BKC-OQ-121` — persetujuan pemilik Rawat Inap atas daftar penghambat yang menyebut unit lain |
| Seeder hak akses Action `Reconcile` | **MUST** terpasang lebih dulu, kalau tidak layar akan ditolak kode `403` meski kodenya benar |


---

# Amendment 14 Oktober 2026 — Keputusan Rantai Permanen (Revisi 2.1)

> `revisi blueprint`: `2.1`, status **approved** (14 Oktober 2026). Masukan: `BKC-DEC-150`, `BKC-DEC-151`,
> `BKC-DES-070`–`BKC-DES-074`. Frontend SHA diaudit: `6e5edd6df`.
> Endpoint yang dipakai: `contracts/api-contract.md` amendment revisi `2.1` — masih **Rencana (belum
> tersedia)**.

## 1. Kebutuhan Layar

| ID layar | Nama layar | Status |
|---|---|---|
| `BIL-SCR-44` | Rekonsiliasi Rangkaian Kunjungan | **Diperbarui** — dari rancangan revisi `2.0` |

Nol layar baru dan nol butir menu baru. Peta butir menu revisi `2.0` tetap berlaku: `BIL-SCR-44` adalah
layar anak dari `BIL-SCR-41` Consumer Handoffs.

## 2. Skema Fitur `BIL-SCR-44` — sesudah revisi `2.1`

```text
+- Rekonsiliasi Rangkaian Kunjungan - Ny. Sari Mandasari ----- BIL-SCR-44 -+
| Status kelayakan: PERLU REKONSILIASI      Surat versi: 3                |
+-------------------------------------------------------------------------+
| Kewajiban yang ditemukan:                                               |
| Tkt | No. Tagihan       | Unit  | Sisa     | Kejanggalan     | Aksi       |
|  0  | INV-RANAP-000412  | RANAP | 3.500.000| -               | -          |
|  1  | INV-IGD-000311    | IGD   |        0 | Rujukan berputar| [Ikutkan]  |
|     |                   |       |          |                 | [Kecualikan]|
|  2  | INV-RAJAL-000087  | RAJAL |  900.000 | Pasien berbeda  | [Kecualikan]|
|     |  ^ tombol Ikutkan tidak tersedia untuk pasien berbeda               |
+-------------------------------------------------------------------------+
| Keputusan permanen yang sudah berlaku:                                  |
| Kunjungan        | Keputusan   | Oleh       | Waktu        | Alasan      |
| (kosong)                                                                |
+------------------------------------------------- [Kembali] -------------+

Saat tombol Kecualikan ditekan:
+- Konfirmasi Keputusan Permanen ----------------------------------------+
| Anda akan mengecualikan INV-RAJAL-000087 (RAJAL) dari penilaian         |
| kelayakan pulang rawat inap ini.                                        |
|                                                                         |
| Keputusan ini PERMANEN dan TIDAK DAPAT DICABUT oleh siapa pun.          |
| Tagihan tersebut tetap menjadi piutang di invoicenya sendiri, tetapi    |
| tidak akan lagi menahan kepulangan pasien ini.                          |
|                                                                         |
| Alasan (wajib): [.................................................]    |
| Jangan menulis diagnosis atau keluhan pasien di kolom ini.              |
|                                                                         |
|                                   [Batal]  [Ya, kecualikan permanen]    |
+-------------------------------------------------------------------------+
```

### Tabel wilayah — yang berubah dari revisi `2.0`

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kolom Kejanggalan | Kode kejanggalan per baris, dalam bahasa petugas: "Rujukan berputar", "Pasien berbeda", "Rantai terlalu panjang", "Lebih dari satu admisi aktif", "Tagihan tidak ditemukan" | Response rincian kewajiban revisi `2.0` | `BillingInpatient : Read` | Baris tanpa kejanggalan menampilkan tanda strip dan tanpa tombol |
| Tombol Ikutkan | Menetapkan keputusan mengikutkan | `POST /inpatient-clearance/{id}/resolve-reconciliation` | `BillingInpatient : Reconcile` | **MUST** disembunyikan untuk baris berkejanggalan "Pasien berbeda" (`BKC-DES-074`), dan untuk peran tanpa `Reconcile` |
| Tombol Kecualikan | Membuka **konfirmasi wajib** sebelum keputusan dikirim | Endpoint yang sama | `BillingInpatient : Reconcile` | Disembunyikan untuk peran tanpa `Reconcile` |
| Konfirmasi keputusan permanen | Menyebut tagihan, unitnya, sifat permanen, dan bahwa tagihan tetap menjadi piutang di invoicenya | — | `BillingInpatient : Reconcile` | Tombol kirim nonaktif selama alasan kosong |
| Riwayat keputusan permanen | Kunjungan, arah keputusan, nama petugas, waktu, dan alasan | `GET /inpatient-clearance/encounter/{encounterId}/chain-decisions` | `BillingInpatient : Read` | Kosong → "Belum ada keputusan permanen untuk rawat inap ini." Gagal → "Riwayat keputusan gagal dimuat." beserta tombol coba lagi |
| Baris yang sudah diputuskan | Menampilkan keputusan beserta pemutusnya, **tanpa** tombol aksi apa pun | Response riwayat keputusan | `BillingInpatient : Read` | — |

## 3. Aturan yang mengikat

| Aturan | Kewenangan | Sebab |
|---|---|---|
| Kalimat konfirmasi **MUST** menyebut kata "permanen" dan "tidak dapat dicabut", serta bahwa tagihan tetap menjadi piutang di invoicenya sendiri | **Mengikat** | Pemilik memilih pengecualian tidak dapat dicabut; konfirmasi inilah satu-satunya kesempatan petugas berhenti sebelum keputusan berlaku selamanya |
| Konfirmasi **MUST** muncul juga untuk keputusan Ikutkan, dengan bunyi yang menyebut sifat permanennya | **Mengikat** | Keputusan Ikutkan juga permanen (`BKC-DES-073`) |
| Tombol Ikutkan tidak pernah tampil untuk kunjungan milik pasien lain | **Mengikat** | `BKC-DES-074` |
| Baris yang sudah punya keputusan tidak menampilkan tombol apa pun | **Mengikat** | Keputusan tidak dapat dibalik (`BIL-VAL-149`) |
| Petunjuk agar kolom alasan tidak memuat diagnosis atau keluhan | **Mengikat** | Privasi — kolom `Reason` adalah teks bebas |
| Bentuk konfirmasi: dialog, halaman, atau langkah tersendiri | `DEV_DISCRETION` | — |
| Warna, ikon, dan penempatan riwayat keputusan | `DEV_DISCRETION` | — |

## 4. Penanganan Keadaan Tambahan

| Keadaan | Perilaku yang mengikat |
|---|---|
| Ditolak `409` karena keputusan sudah ada (`BIL-VAL-147`) | Layar menampilkan bahwa kunjungan ini sudah diputuskan, lalu memuat ulang riwayat keputusan supaya petugas melihat siapa yang memutuskan. Layar **MUST NOT** menawarkan "coba lagi" untuk keputusan yang sama |
| Ditolak `422` karena arah berlawanan (`BIL-VAL-149`) | Layar menampilkan keputusan yang sudah berlaku beserta pemutusnya |
| Pengiriman ganda | `IdempotencyKey` dibentuk sekali per pembukaan konfirmasi; penekanan berulang mengirim kunci yang sama sehingga hanya satu keputusan lahir |

## 5. Aksi per Peran — sesudah `BKC-DEC-150`

| Peran | Membuka `BIL-SCR-44` | Melihat rincian rupiah | Menetapkan keputusan permanen | Membaca riwayat keputusan |
|---|:---:|:---:|:---:|:---:|
| Kasir | Ya | Ya | **Ya** | Ya |
| Supervisor Kasir | Ya | Ya | Ya | Ya |
| Petugas Billing | Ya | Ya | **Tidak** | Ya |
| Finance / AR | Ya | Ya | Tidak | Ya |
| Perawat bangsal | Tidak | Tidak | Tidak | Tidak |

## 6. Dependency Pengerjaan

| Kebutuhan frontend | Menunggu |
|---|---|
| Riwayat keputusan dan konfirmasi | Endpoint revisi `2.1` — masih **Rencana (belum tersedia)** |
| Tombol evaluasi ulang yang sudah ada di layar kasir | Perbaikan constraint `REEVALUATED_BY_CASHIER` — **nol perubahan frontend**, tombolnya tidak perlu diubah; begitu backend diperbaiki, tombol itu berfungsi |




---

# Amendment 14 Oktober 2026 — Kode Kejanggalan Asal Transfer (Revisi 2.2)

> `revisi blueprint`: `2.2`, status **draft**. Masukan: `BKC-DES-080`, `BIL-API-1.9`. Backend SHA `d268a41f`, Frontend SHA `0b45dd8cd`.

## 1. Peta Butir Menu

Nol butir menu baru dan nol layar baru. `BIL-SCR-44` tetap dijangkau dari layar induk revisi `2.0`.

## 2. Perubahan pada `BIL-SCR-44` — Rekonsiliasi Rantai Kunjungan

```text
+------------------------------------------------------------------------+
| No | Invoice          | Unit | Sisa (Rp) | Kejanggalan          | Aksi    |
|----|------------------|------|-----------|----------------------|---------|
|  1 | INV-IGD-000418   | IGD  | 1.500.000 | Asal transfer belum  | [Ikutkan]   |
|    |                  |      |           | tertaut              | [Kecualikan]|
+------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Kolom Kejanggalan | Label baru **"Asal transfer belum tertaut"** untuk `UNLINKED_TRANSFER_ORIGIN`, di samping lima label revisi `2.0` | `GET /inpatient-clearance/encounter/{encounterId}/obligations` | `BillingInpatient : Read` | Tidak berubah |
| Tombol Ikutkan dan Kecualikan | **Keduanya** tersedia untuk kejanggalan ini, karena pasiennya pasti sama | `POST /inpatient-clearance/{id}/resolve-reconciliation` | `BillingInpatient : Reconcile` | Tidak berubah; konfirmasi permanen revisi `2.1` berlaku |
| Kode tak dikenal | Kode di luar daftar label **MUST** tampil apa adanya dengan label "Kejanggalan lain", bukan disembunyikan | — | — | — |

Bunyi label adalah usulan desain dan tunduk pada persetujuan pemilik. Warna, ikon, dan letak tetap `DEV_DISCRETION`.

## 3. Ringkasan Tagihan Pasien

Item tagihan kunjungan asal transfer ikut tergabung pada ringkasan tagihan, karena penggabungan
terjadi di backend (`PatientBillingSummaryService.Breakdown.cs:100-118`). Nol perubahan frontend.
Daftar `LinkedEncounters` pada response itu belum dibaca frontend (nol kemunculan pada `src/`);
menampilkannya **tidak** diwajibkan revisi ini.

---

# Amendment 16 Oktober 2026 — Biaya Admin Rawat Jalan Digantikan dan Tinjauan Void Keliru (Revisi 2.3)

> `revisi blueprint`: `2.3`, status **draft**. Masukan: `BKC-DES-083`–`BKC-DES-089`, `BIL-API-2.0`.
> Backend SHA `738e03a0`, Frontend SHA `8ad45262b`.

## 1. Peta Butir Menu

| Butir | Tingkat | Induk | Route (usulan) | Layar | Hak akses yang menjaga |
|---|---|---|---|---|---|
| **Tinjauan Void Biaya Administrasi** (baru) | 3 | Billing Management → **Approval & Kontrol** (`src/utils/menu-sidebar/menu-items.jsx:2372`), bersebelahan dengan "Rekonsiliasi Tagihan Klinis" | `/health-services/billing-management/billing/superseded-item-reviews` | `BIL-SCR-45` | `requiredPermission: { resource: "BillingSupersededItemReview", action: "Read" }` |

Induk dan route di atas adalah **usulan**, mengikuti pola butir "Rekonsiliasi Tagihan Klinis" pada kelompok yang sama.
Bunyi label, urutan di dalam kelompok, dan ikon tunduk pada persetujuan Frontend Owner. Ketiganya tidak menghalangi
pembangunan, karena dapat diubah tanpa menyentuh kontrak.

`BIL-SCR-42` dan panel ringkasan rawat inap tidak menambah butir menu; keduanya layar yang sudah ada.

## 2. Perubahan pada `BIL-SCR-42` — Menu Pembayaran

Saat ini baris biaya administrasi hanya tampil bila `appliedAmount > 0`
(`src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.jsx:824-837`).
Akibatnya, tagihan poli yang admin-nya digantikan akan tampak seolah tidak pernah punya admin.

```text
+--------------------------------------------------------------------+
| Rincian Tagihan                                                    |
|  ...                                                               |
|  Biaya Administrasi        Rp 0     [Digantikan rawat inap]        |
|                            Tagihan rawat inap: INV-RANAP-000207    |
|  ...                                                               |
+--------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Baris Biaya Administrasi | Tampil **juga** bila `isSupersededByInpatientAdmission = true`, dengan nominal Rp 0 dan label "Digantikan rawat inap" | `GET /invoices/{id}/calculation-preview` → `breakdown.administrationFee` | `BillingInvoice : Read` | Tidak berubah dari perilaku yang ada |
| Keterangan tagihan pengganti | Nomor tagihan rawat inap dari `supersededByInpatientInvoiceId` | Sama, ditambah pembacaan nomor invoice yang sudah ada | `BillingInvoice : Read` | Nomor gagal dimuat → tampilkan "Tagihan rawat inap" tanpa nomor; label tetap tampil |
| ~~Baris item tarif admin yang dikeluarkan~~ | **Dicabut** bersama `BKC-DES-092` (`BKC-DEC-167`: item tarif admin tidak dipakai) | — | — | — |

## 3. Perubahan pada panel ringkasan rawat inap

Berkas: `src/components/view/health-services/billing-management/billing-invoices/detail/billing-inpatient-summary-panel.jsx`.

| Wilayah | Sebelum | Sesudah | Sumber data |
|---|---|---|---|
| Baris "Kredit Biaya Administrasi Rajal (Pengurang)" (`:321-327`) | Membaca `referredOutpatientAdminCreditedAmount` | Tetap; artinya kini kredit yang **terpakai** | `breakdown.administrationFee` |
| Baris "Biaya Administrasi Rajal Dibatalkan" (`:78-80`, `:329-333`) | Membaca `referredOutpatientAdminVoided` | **Dihapus.** Field-nya hilang dari API |  — |
| Daftar asal (baru) | — | Satu baris per elemen `referredOutpatientAdminOrigins`: nomor tagihan poli, status, dan perlakuan ("Admin dihapus di tagihan poli" / "Dikreditkan Rp …" / "Tanpa admin") | Sama |
| Keterangan sisa kredit (baru) | — | Bila `referredOutpatientAdminUnappliedCreditAmount > 0`: "Sisa kredit Rp … akan menjadi kredit refund saat tagihan difinalisasi" | Sama |

Bunyi label adalah usulan desain. Warna, ikon, dan tata letak tetap `DEV_DISCRETION`.

## 4. Layar baru `BIL-SCR-45` — Tinjauan Void Biaya Administrasi

| Field | Isi |
|---|---|
| Pelaku | Supervisor Kasir |
| Jalan masuk | Butir menu pada bagian 1 |
| Tujuan | Memeriksa item yang dulu di-void oleh cacat lama, lalu memutuskan Pulihkan, Biarkan, atau Perlu Koreksi Pasca-Tutup |

```text
+------------------------------------------------------------------------------------------+
| Tinjauan Void Biaya Administrasi                                                         |
| [Belum ditinjau v] [Status tagihan: Semua v] [Cari nomor tagihan ____] [Muat ulang]      |
|------------------------------------------------------------------------------------------|
| Tagihan       | Pasien        | Item                         | Nilai     | Status | Aksi |
| INV-RJ-002311 | Budi S. / RM… | Administrasi Rekam Medis     | 25.000    | OPEN   | [Pulihkan] [Biarkan] |
| INV-RJ-001876 | Sari M. / RM… | Paket Administrasi Kartu     | 15.000    | CLOSED | [Perlu Koreksi] [Biarkan] |
|------------------------------------------------------------------------------------------|
| < 1 2 3 >                                                                                |
+------------------------------------------------------------------------------------------+

Dialog keputusan
+---------------------------------------------------------------+
| Pulihkan item "Administrasi Rekam Medis" Rp 25.000?           |
| Keputusan ini final dan tidak dapat diubah.                   |
| Alasan (wajib, minimal 10 karakter): [______________________] |
|                                    [Batal]  [Simpan Keputusan]|
+---------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Filter | Keadaan tinjauan (bawaan "Belum ditinjau"), status tagihan, nomor tagihan | Query `GET /superseded-item-reviews` | `BillingSupersededItemReview : Read` | — |
| Tabel | Nomor tagihan, nama pasien dan nomor RM, deskripsi item, nilai, status tagihan, tanggal void, penanda "tertaut ke rawat inap" (hanya informasi), keputusan bila sudah ada *(penanda "kategori tarif admin" dari adendum `BKC-DES-092` dicabut)* | `GET /superseded-item-reviews` | `BillingSupersededItemReview : Read` | Kosong: "Tidak ada item yang perlu ditinjau." Gagal: "Daftar tidak dapat dimuat. Coba lagi." beserta tombol coba lagi |
| Tombol aksi | Hanya keputusan yang ada di `allowedDecisions` | Response baris | `BillingSupersededItemReview : Decide`; tanpa hak itu tombol tidak tampil | — |
| Dialog keputusan | Ringkasan item, peringatan final, isian alasan | `POST /superseded-item-reviews/{invoiceItemId}/decision` dengan header `Idempotency-Key` dibuat sekali per pembukaan dialog | `BillingSupersededItemReview : Decide` | `409` data berubah → "Data telah berubah. Muat ulang sebelum melanjutkan." dan daftar dimuat ulang. `409` sudah diputuskan → baris diperbarui dari server. `422` → pesan dari server ditampilkan apa adanya |

Aturan perilaku:

1. Tombol simpan dinonaktifkan selama permintaan berjalan. `Idempotency-Key` yang sama dipakai bila pengguna menekan ulang setelah jaringan putus, sehingga keputusan tidak tercatat dua kali.
2. Setelah keputusan tersimpan, baris diperbarui dari response, bukan dari asumsi lokal.
3. Nama pasien dan nomor RM **MUST NOT** dikirim ke logger frontend maupun pelacak galat.
4. Layar dapat dipakai dengan papan ketik; dialog mengunci fokus dan dapat ditutup dengan `Esc`.

Komponen, warna, jarak, dan bentuk tabel tetap `DEV_DISCRETION` mengikuti base component Quilvian.

## 5. Keterjangkauan layar

| Layar | Butir menu | Atau layar anak dari |
|---|---|---|
| `BIL-SCR-45` | Tinjauan Void Biaya Administrasi | — |
| `BIL-SCR-42` (diperbarui) | Sudah ada | — |
| Panel ringkasan rawat inap (diperbarui) | — | Detail invoice rawat inap (sudah ada) |

# Amendment 22 Oktober 2026 — Benefit Karyawan RS di Popup Pembayaran dan Pembayaran Langsung Kelebihan Plafon (Revisi 2.4)

> `revisi blueprint`: `2.4`, status **diterapkan di source, belum dibuild atau diuji runtime**. Masukan: adendum backend §16
> (`02-backend-architecture.md`). Frontend branch `yasmina` (huruf kecil, SHA dasar `24939831`); GitHub menolak `Yasmina`
> karena nama branch peka huruf besar-kecil.

## 1. Layar yang berubah

| Layar | Perubahan |
|---|---|
| `BIL-SCR-42` Menu Pembayaran | Memuat `FinalizeInvoiceModal` yang sudah ada; sisa yang harus dibayar untuk kunjungan benefit memakai `remainingPatientObligation` Backend; layar tidak lagi menandai lunas selama kelebihan plafon (E) belum dibayar atau ditangguhkan |
| Popup Pilih Metode Pembayaran (`AddTenderModal`) | Kartu **Benefit Karyawan RS**, kartu **Saldo Deposit Pasien (RANAP)**, lalu kelompok Tunai/QRIS/Transfer Bank di dalam satu popup |
| Panel benefit di modal finalisasi | Total kewajiban memakai nilai Backend; menampilkan kelebihan yang sudah dibayar langsung |

## 2. Kartu Benefit Karyawan RS

1. Tampil hanya bila preview finalisasi Backend (`GET finalizations/invoices/{id}/preview`, `employeeBenefitSplit`) menyatakan `isEmployeeBenefitVisit`. Tidak ada angka yang dihitung di klien.
2. Status: `Eligible` (pemilik, nomor pegawai, sisa plafon) atau **Memerlukan verifikasi HR** disertai alasan Backend; bila tidak dapat dipastikan, aksi disabled (gagal tertutup).
3. **Gunakan Benefit RS** membuka rincian di popup yang sama (tanpa popup baru): total biaya, porsi aturan penjamin, ditanggung Benefit RS, kelebihan di atas plafon (total, sudah dibayar, belum dibayar), sisa tagihan pasien (K), dan total yang harus diselesaikan pasien.
4. Benefit **bukan metode pembayaran**: tidak membuat tender, tidak memotong plafon, dan tidak pernah ditulis sebagai `paymentMethodId`. Plafon dipotong hanya saat finalisasi.
5. **Lanjut ke Finalisasi Benefit** (atau **Finalisasi: Tangguhkan ke AR Karyawan** bila masih ada kelebihan belum dibayar) membuka `FinalizeInvoiceModal`; aktif setelah K lunas. Pilihan D, consent, dan konfirmasi divalidasi Backend.

## 3. Alur pembayaran kunjungan benefit

1. Kasir membayar sisa tagihan pasien (K) lebih dulu, lalu kelebihan di atas plafon (E), lewat tender biasa (Tunai, QRIS, Transfer Bank, atau split). Backend mengalokasikan K ke `INVOICE` dan E ke `BENEFIT_EXCESS`.
2. `startPayment` membuat settlement dengan jumlah `remainingPatientObligation` dari preview Backend **terbaru** (setelah rekalkulasi), bukan angka layar. Kunjungan non-benefit tetap memakai `PatientAmount`.
3. Kelebihan yang tidak dibayar langsung ditangguhkan menjadi AR Karyawan di modal finalisasi. Tidak ada persetujuan otomatis.
4. Setelah tender masuk, preview dimuat ulang; setelah finalisasi berhasil, invoice, settlement, dan kalkulasi dimuat ulang.
5. Jalur "kewajiban pasien nol" tidak dipakai selama kelebihan plafon masih harus dibayar atau ditangguhkan.

## 4. Popup pembayaran: deposit RANAP

1. Kartu deposit ditentukan dari invoice (`serviceType = RANAP`), bukan dari tombol pembuka popup; tombol "Bayar dengan Saldo Deposit" yang terpisah dihapus.
2. **Gunakan Deposit** memilih deposit tanpa transaksi; **Semua** mengisi `min(saldo, sisa tagihan)` dari quote Backend. Pilihan deposit hanya state lokal modal (tidak ditulis ke baris split payment); metode biasa membatalkannya.
3. Tombol utama langsung memotong deposit (tanpa langkah konfirmasi kedua dan tanpa keterangan tambahan di popup). Alokasi lewat `POST patient-funds/deposits/{encounterId}/allocations` dengan quote terbaru, satu permintaan pada satu waktu, dan percobaan ulang dengan key yang sama. Konflik `409` tidak diulang otomatis.
4. Nominal dibatasi saldo dan sisa tagihan; kelebihan ke saldo refund tidak diproses lewat popup ini.

## 5. Hal yang belum dikerjakan / keputusan tertunda

| Hal | Status |
|---|---|
| Pilihan "Bayar Sendiri" yang tersimpan | Tidak dibuat; tidak ada endpoint dan maknanya belum jelas |
| Tab histori terpadu (HR, Billing, Finance, Payroll) dari transaksi kasir | Tidak dibuat; sumber data tetap `HrdBenefitBalanceMovement`, `BilArHandoff`, `FinReceivable`, `FinReceivableMovement` |
| Aturan cakupan penjamin internal | Diatur admin di Master Data; tanpa aturan, kartu benefit menampilkan porsi penjamin Rp0 |
| Verifikasi runtime, mobile, dan 20 skenario penerimaan | Belum dijalankan |

# Amendment 22 Oktober 2026 — Status Item Rincian Tagihan Penjamin Perusahaan dan Hitung Ulang Cakupan (Revisi 2.5)

> `revisi blueprint`: `2.5`, status **diterapkan di source, belum dibuild atau diuji runtime**. Masukan: adendum backend §17
> (`02-backend-architecture.md`). Frontend branch `yasmina` (huruf kecil, SHA dasar `24939831`).

## 1. Masalah

`resolveItemCoverageStatus` mengembalikan `tunai` bila item tidak ada pada hasil kalkulasi **dan** bila porsi penjamin item = 0, tanpa melihat alasannya. Pada invoice
Benefit Karyawan RS, nol akibat aturan yang tidak dikenali mesin (adendum backend §17.1), hasil kalkulasi usang, atau data yang belum dimuat semuanya tampil sebagai "Tunai".
Kunci `anomali_data` juga dikembalikan resolver tetapi tidak punya entri di `BILLING_ITEM_COVERAGE_BADGE_CONFIG`, sehingga badge menampilkan teks mentah.

## 2. Aturan status (satu sumber: `billing-invoice-calculation-breakdown.js`)

| Kondisi (urut evaluasi) | Badge |
|---|---|
| Item tidak ada di hasil kalkulasi: invoice tunai / selain itu | Tunai / **Menunggu Verifikasi** |
| `dataAnomaly > 0` | Menunggu Verifikasi (`anomali_data`) |
| `unresolved > 0` | Menunggu Verifikasi |
| Porsi penjamin > 0, sisa pasien (item + pajak) > Rp0,50 / selain itu | **Campuran** / Penjamin |
| Porsi penjamin = 0 pada invoice tunai | Tunai |
| `coverable = false`, atau alasan `NO_MATCHING_RULE`, `EXPLICIT_NOT_COVERED`, `CASH_ASSIGNED_*`, `CATEGORY_NOT_COVERABLE` | **Pribadi** |
| Alasan `PAYER_NOT_ELIGIBLE` / `COVERAGE_UNRESOLVED` | Menunggu Verifikasi |
| Nol tanpa alasan: rincian per item tidak tersedia, atau penjamin perusahaan (snapshot sebelum alasan dilaporkan) | **Menunggu Verifikasi** (bukan Tunai) |
| Nol tanpa alasan pada asuransi (mesin asuransi belum melaporkan alasan) | Tunai (perilaku lama dipertahankan) |

Prinsip: status berasal dari nominal dan alasan Backend, bukan dari nama penjamin. "Pribadi" berarti hasil kalkulasi yang sah menetapkan item ditanggung pasien pada
invoice berpenjamin; "Tunai" hanya untuk invoice yang tunai sejak awal. Satu rumus sisa pasien dipakai badge Campuran dan keterangan "Co-payment pasien"
(`itemPatientResidual`: `netAmount` − porsi penjamin − unresolved). `netAmount` Backend sudah memuat pajak (`ApplyInvoiceTax`: `NetAmount = Gross − Diskon + Pajak`), jadi pajak
tidak boleh ditambahkan lagi. Revisi awal adendum ini menambahkannya dan salah menampilkan "Campuran" serta "Co-payment pasien" sebesar PPN pada item yang ditanggung penuh; sudah dikoreksi
dan dijaga tes regresi.

`buildItemOutcomeById(calculation, { isCashInvoice })` mengembalikan `Map` ber-`context` (`payerKind`, `isPerItemAllocationAvailable`, `isCashInvoice`);
`isCashInvoice` dari layar hanya petunjuk untuk snapshot lama tanpa `payerKind`. Tooltip badge (`resolveItemCoverageHint`) menjelaskan alasan Backend atau mengapa status belum pasti.

## 3. Kesegaran perhitungan dan Hitung Ulang Cakupan (Menu Pembayaran)

1. Layar membaca `calculation-preview` (aturan terbaru, tidak disimpan); pembagian Benefit RS dan finalisasi membaca versi **tersimpan**. `resolveCalculationFreshness` membandingkan porsi penjamin, porsi pasien, porsi belum terverifikasi, dan selisih tidak ditagihkan antara pratinjau dan versi tersimpan; `calculationCurrent` dari preview finalisasi Backend dipakai sebagai penanda kedua.
2. Bila salah satunya menunjukkan tertinggal (invoice OPEN, bukan tunai), muncul pemberitahuan **"Perhitungan cakupan tersimpan belum terbaru"** dengan tombol **Hitung Ulang Cakupan**. Tidak ada perhitungan ulang otomatis.
3. Tombol memanggil satu `POST invoices/{id}/recalculate` (alasan tercatat di audit), memuat ulang invoice, lalu pratinjau dan preview finalisasi, sehingga badge, ringkasan, dan pembagian Benefit RS sejajar. Item dan penugasan payer (termasuk Pribadi pilihan kasir) tidak berubah.
4. Saat pratinjau dimuat, header Rincian Tagihan menampilkan "Memuat perhitungan terbaru…"; badge tidak dikunci ke "Tunai" selama itu.
5. **Tidak ada finalisasi langsung dari tombol Proses Pembayaran, termasuk bila tagihan pasien Rp0.** Finalisasi menutup invoice, meneruskan piutang penjamin ke Finance, dan pada kunjungan benefit memotong plafon HR,
   jadi selalu didahului konfirmasi kasir yang eksplisit:
   - Kunjungan benefit yang dapat dipakai: tombol membuka **modal finalisasi yang sudah ada** (alasan wajib, rincian pembagian Benefit RS, dan centang konfirmasi alokasi pembiayaan).
   - Selain itu (mis. asuransi 100%): dialog **"Selesaikan tagihan tanpa pembayaran pasien?"**; `POST invoices/{id}/complete` hanya dipanggil dari tombol "Ya, Selesaikan Tagihan".
   Bila perhitungan ulang pada klik itu mengubah kewajiban dari > 0 menjadi 0, invoice dan pratinjau dimuat ulang lebih dulu dan tampil toast "Perhitungan Diperbarui" berisi angka sebelum dan sesudah, baru konfirmasi dibuka.
   Revisi awal poin ini hanya menahan kasus > 0 menjadi 0, sehingga layar yang sudah menampilkan Rp0 masih menyelesaikan invoice dalam satu klik; sudah dikoreksi.

## 4. Berkas

`billing-invoice-calculation-breakdown.js`, `billing-invoice-constants.js` (badge `pribadi`, `campuran`, `anomali_data`), `billing-invoice-items-table.jsx` (`getItemCoverageHint`),
`use-menu-pembayaran.js` (`calculationFreshness`, `recalculateCoverage`), `menu-pembayaran-view.jsx`, `edit-tagihan-view.jsx`, `detail-invoice-billing-view.jsx`,
`tests/unit/billing-invoice-calculation-breakdown.test.mjs`.

## 5. Belum dikerjakan

| Hal | Status |
|---|---|
| Badge Biaya Administrasi/Biaya Kamar pada Detail Invoice | Masih `penjamin`/`tunai` dari nominal; tidak disentuh |
| Verifikasi runtime dan unit test `node --test` | Belum dijalankan (hanya ESLint pada berkas yang diubah) |

# Amendment 22 Oktober 2026 — Fase Pembayaran, Konfirmasi Benefit RS, dan Status Serah Terima Finance (Revisi 2.6)

> `revisi blueprint`: `2.6`, status **diterapkan di source, belum dibuild atau diuji runtime**. Masukan: adendum backend §18. Frontend branch lokal `yasmina` (huruf kecil).

## 1. Masalah

`isLunas` diturunkan dari "sisa pasien = 0" (dan flag `PaymentStatus = LUNAS` Backend yang artinya *tidak ada sisa pembayaran pasien*). Pada invoice Benefit Karyawan RS yang ditanggung penuh, nilainya benar **sebelum** kasir mengonfirmasi apa pun, sehingga invoice OPEN tampil
"Lunas · Ditanggung Penjamin", tombol Proses Pembayaran berubah "✓ Tagihan Sudah Lunas" dan terkunci, serta banner hijau menulis tetap *"Piutang ... telah diteruskan ke Finance (CREATED)"*. Teks itu **bukan data Backend**: tidak ada finalisasi, pemotongan plafon HR, mutasi USE, maupun handoff.

## 2. Fase pembayaran (satu sumber: `billing-payment-phase.js`)

| Fase | Kondisi | "Lunas"? | Lencana |
|---|---|---|---|
| `FINALIZED` | Status invoice FINAL/CLOSED (finalisasi tersimpan) | Ya | Selesai |
| `AWAITING_BENEFIT_CONFIRMATION` | OPEN, kunjungan Benefit RS, sisa kewajiban 0, tanpa uang masuk | **Tidak** | Menunggu Konfirmasi Benefit RS |
| `AWAITING_COMPLETION` | OPEN, bukan benefit, sisa 0, tanpa uang masuk | **Tidak** | Menunggu Penyelesaian Tagihan |
| `PATIENT_PAID` | OPEN, sisa 0 dan ada uang yang benar-benar diterima | Ya (pembayaran pasien) | Lunas · Sisa Rp0 |
| `PATIENT_DUE` | OPEN, masih ada sisa (benefit: K + E sisa dari Backend) | Tidak | Belum Lunas / Cicilan |
| `NOT_CALCULATED` | Belum ada hasil kalkulasi | Tidak | Belum Lunas |

"Lunas" tidak pernah berasal dari kewajiban 0 saja. Sisa kewajiban yang dipakai sudah dikurangi uang yang masuk (kunjungan benefit: `remainingPatientObligation` Backend).

## 3. Panel fase dan tombol (`billing-payment-phase-panel.jsx`)

1. **Menunggu Konfirmasi Benefit RS**: kartu informasi yang menyatakan terus terang plafon belum dipotong dan piutang belum diteruskan ke Finance, dengan rincian dari preview Backend: pemilik benefit (nama, nomor pegawai, hubungan), total biaya, porsi menurut aturan penjamin, sisa plafon, "Akan ditanggung Benefit RS", dan kelebihan di atas plafon bila ada. Bila benefit tidak dapat dipastikan: kartu peringatan berisi alasan Backend.
2. **Tombol Proses Pembayaran tetap aktif** ("Proses Pembayaran (Konfirmasi Benefit RS)") selama fase menunggu. Klik membuka **modal finalisasi yang sudah ada** (alasan wajib, rincian pembagian, centang konfirmasi); finalisasi baru berjalan dari modal itu (lihat Rev 2.5 poin 5). Tombol **tidak aktif (gagal tertutup)** bila preview finalisasi belum termuat (kunjungan benefit belum dapat dibedakan) atau benefit tidak dapat dipastikan, dengan alasan tampil di bawah tombol.
3. **Setelah finalisasi**: "Tagihan Selesai — Ditanggung Benefit RS" (atau "Ditanggung Penjamin"/"Lunas & Selesai"), memuat hanya yang benar-benar terjadi: Ditanggung Benefit RS, Diterima Kasir (hanya uang masuk; tidak pernah menampilkan Rp0 sebagai kas), dan Ditangguhkan ke AR Karyawan bila ada.
4. **Serah terima Finance** dibaca dari handoff Backend (`financeStage`): Handoff dibuat · Intake belum diproses · Intake gagal · Piutang sudah diakui · Perlu rekonsiliasi. Handoff CREATED tidak pernah disebut piutang diakui; respons lama tanpa `financeStage` tampil "Status Finance belum diketahui".
5. Porsi Benefit RS internal, utang karyawan (`EMPLOYEE_BENEFIT`), piutang penjamin pasien, dan piutang penjamin luar dibedakan lewat `resolveHandoffKind` (PAYER + `isInternalBenefit` ≠ PAYER luar ≠ EMPLOYEE_BENEFIT).

## 4. Hitung ulang dan konfirmasi

`startPayment` menyimpan perhitungan ulang lebih dulu bila versi tersimpan tertinggal dari pratinjau (juga pada kewajiban nol), sehingga kasir mengonfirmasi angka yang akan difinalisasi. Backend menolak penyelesaian bila penyegaran otomatis mengubah angka (adendum backend §18.2).

## 5. Finance › Data Tagihan A/R

Tab: **Perusahaan/Asuransi** (PAYER luar), **Benefit RS Internal** (PAYER internal), **Piutang Karyawan** (EMPLOYEE_BENEFIT), **Pasien Umum**. Header grup memuat pemilik benefit (nama dan nomor pegawai). "Buat AR/Invoice" tidak tersedia untuk Benefit RS Internal (alasan `INTERNAL_BENEFIT`). Pesan keadaan kosong berasal dari Backend dan menjelaskan kapan setiap kategori terisi; pesan "jalur EMPLOYEE_BENEFIT belum tersedia" dihapus.

## 6. Berkas

`billing-payment-phase.js` (baru), `billing-payment-phase-panel.jsx` (baru), `menu-pembayaran-view.jsx`, `use-menu-pembayaran.js`, `billing-data-constants.jsx`, `grouped-billing-table.jsx`, `billing-data-view.jsx`, `tests/unit/billing-payment-phase.test.mjs` (baru).

## 7. Belum dikerjakan

| Hal | Status |
|---|---|
| Penyegaran status Finance otomatis di layar kasir | Dibaca saat halaman dimuat; belum ada polling. Intake Finance diolah saat Finance membuka Data Tagihan (adendum backend §18.6) |
| Halaman riwayat benefit per karyawan di HR | Buku mutasi sudah diperkaya di Backend; tampilan HR belum diubah |
| Kategori Benefit RS Internal di halaman Finance selain Data Tagihan | Belum |
| Verifikasi runtime, mobile, dan unit test `node --test` | Belum dijalankan (hanya ESLint) |

# Amendment 22 Oktober 2026 — Running Invoice mengikuti komponen reusable dan Pratinjau Draft pada Progress Pembayaran (Revisi 2.7)

> `revisi blueprint`: `2.7`, status **diterapkan di source, belum dibuild atau diuji runtime**. Tidak ada perubahan Backend: kontrak yang dipakai sudah memadai. Frontend branch lokal `yasmina` (HEAD sekarang merge dari `QuilvianIntegrationFrontend`; berbeda dari SHA acuan `ec528982`).

## 1. Running Invoice (`BIL-SCR` daftar invoice)

**Akar masalah tampilan:** halaman ini tidak memakai kelas `dataPage`, padahal DataFilter/DataTable mengambil token `--base-*` (permukaan, radius, bayangan, tipografi) dari kelas itu. Tanpa token, komponen reusable jatuh ke gaya bawaan, dan CSS lokal menambalnya dengan keluarga token lain (`--color-*`) plus selector rapuh:
`nth-child` / `nth-last-child` yang sudah tidak sesuai urutan kontrol filter (komentarnya masih menyebut Status sebagai kontrol pertama, padahal sekarang Tanggal Mulai), `[class*="filterBar"]`, `[class*="searchBox"]`, `[class*="tableWrapper"]`, serta penimpaan global `thead th`, `tbody td`, `tbody tr`.

| Perubahan | Rincian |
|---|---|
| Kerangka | Pola halaman daftar kanonik (sama dengan Master Data Bank): `administrator-region-page` + `dataPage` + `regionTheme`, shell `region-page-shell` + `dataShell` + `tableShell`; Hero, DataFilter, DataTable tanpa kelas penimpa |
| CSS lokal | Dipangkas dari 553 menjadi ~200 baris: hanya isi sel (`nowrap`, `wrapText`, `amountCell`) dan menu aksi. Tidak ada `nth-child`, `[class*=]`, atau penimpaan `th/td/tr` |
| Isi sel | Kelas `tableStyles` yang sama dengan halaman daftar lain (`rowNumber`, `dateCell`, `codeCell`, `primaryCell`, `ordinaryCell`) |
| Nomor urut | Tetap dari `context.rowNumber` DataTable = `(halaman − 1) × ukuran halaman + indeks + 1` (1, 2, 3, … sesuai pagination); `pageNumber`/`pageSize` berasal dari respons Backend |
| Tipe Pasien | `StatusBadge` dengan kelas `region-status-*` yang sudah ada, varian dari `primaryPayerType` (CASH → hijau, INSURANCE → teal, COMPANY_GUARANTOR → kuning); sebelumnya span berwarna heksa literal. Penjamin perusahaan tidak lagi ungu (tidak ada token ungu) |
| Tipe Layanan | Teks biasa (kategori, bukan status) |
| Kolom Status | **Semantik tidak berubah**: status pembayaran authoritative bila ada, kalau tidak status siklus hidup invoice |
| Rata kanan | Kolom Total Tagihan: judul dan isi sama-sama rata kanan lewat `align="right"` DataTable |
| Aksi | `onKeyDown` pada sel aksi tidak lagi sampai ke baris (sebelumnya Enter/Spasi pada tombol aksi malah membuka Menu Pembayaran); menu memakai `popperConfig={{ strategy: "fixed" }}` supaya tidak terpotong area gulir horizontal tabel |
| Responsif | DataFilter/DataTable memakai aturan responsif bawaan (flex wrap, pembungkus tabel `overflow-x: auto`); lebar kolom 64/150/130/230/200/140/130/160/180/140/130/72 dengan `minWidth` |

## 2. Progress pembayaran: Terkonfirmasi vs Draft (`billing-payment-progress-state.js`)

Tiga nilai yang dipisah:

| Nilai | Sumber | Makna |
|---|---|---|
| `confirmedPaidAmount` | `SuccessfulAmount` seluruh settlement invoice (tender sukses + alokasi deposit), dari Backend | Satu-satunya dasar "Terkonfirmasi" dan "Lunas" |
| `draftPaymentAmount` | Baris lokal berstatus draft dengan metode dipilih dan nominal > 0 | Rencana; tidak mengubah status keuangan apa pun |
| `previewPaidAmount` | `min(total, terkonfirmasi + draft + diproses + menyinkronkan)` | Pratinjau bilah |

Aturan per status baris: `draft` dihitung bila valid (tanpa metode = belum lengkap, dilaporkan terpisah); `submitting` dihitung sebagai **diproses** (bukan sukses) supaya bilah tidak turun saat dikirim; `failed` **tidak** dihitung, dilaporkan sebagai "Gagal, perlu diulang"; `succeeded` tidak dihitung dari baris (sudah ada di terkonfirmasi), kecuali sempit saat menunggu Backend disegarkan (`awaitingSync` + `syncBaseline`): dihitung "menyinkronkan" hanya selama terkonfirmasi di layar belum naik sebesar nominalnya, sehingga bilah tidak turun-naik dan tidak pernah dihitung dua kali (`addBillingTender.fulfilled` hanya menyisipkan tender dan tidak memperbarui `SuccessfulAmount`). Tender PENDING provider bukan terkonfirmasi dan bukan draft: tampil sebagai "Menunggu konfirmasi provider".

Rumus: `previewPercent = total > 0 ? clamp(preview / total × 100, 0, 100) : 0` (dua desimal). Bilah hanya 100% bila memang lengkap; selisih kecil (mis. Rp0,23) dibatasi 99,99%.

Label: **Lunas** hanya bila terkonfirmasi memenuhi total (hasil Backend). Rencana yang memenuhi sisa: **Siap Diproses** bila penilaian kesiapan layar (`evaluatePaymentSubmission`) menyatakan siap, atau **Nominal Terpenuhi** bila belum (baris belum lengkap); saat baris dikirim: **Sedang Diproses**. Status invoice dan "Lunas" di header tetap berasal dari fase otoritatif (Rev 2.6), tidak pernah dari draft.

`BillingPaymentProgress` menerima prop baru `progress` (hasil `computePaymentProgress`); pemakaian lama (`paidAmount` + `totalAmount`, Detail Invoice Billing) berperilaku sama. Bilah dua segmen (terkonfirmasi berisi penuh; draft bergaris) supaya terbedakan tanpa bergantung warna; rincian: Terkonfirmasi, Draft pembayaran, Sedang diproses, Gagal, Menunggu provider, Total rencana. `role="progressbar"` dengan `aria-valuenow` dan `aria-valuetext` yang memisahkan tiga angka dan menyatakan "belum lunas".

Reaktivitas: nilai diturunkan dari `settlement.rows` pada setiap render, jadi memilih metode, mengisi/mengubah nominal, Salin Nominal, menambah/menghapus baris, dan perubahan split langsung menggeser bilah. Tidak ada `POST` selama kasir hanya mengedit; pengiriman tetap hanya dari Proses Pembayaran. Setelah sukses, draft berpindah ke terkonfirmasi lewat penyegaran settlement dari Backend; setelah gagal, terkonfirmasi tidak berubah dan baris ditandai gagal untuk percobaan ulang yang aman (idempotency key dipertahankan).
Total progress kunjungan benefit = sudah dibayar + sisa kewajiban (K + E sisa) dari Backend; selain itu `PatientAmount`.

## 3. Kontrak API (tidak ada perubahan Backend)

`GET invoices/{id}/settlements` dan `GET settlements/{id}` (`SettlementResponse`: `SuccessfulAmount`, `PendingAmount`, `CollectibleAmount`, `Tenders`, `IsFullyPaid`, `PaymentStatus`); `GET invoices` (`InvoiceSummaryResponse`: `TotalPaidAmount`, `IsFullyPaid`, `PaymentStatus`). Catatan: `PaymentStatus = LUNAS` pada daftar invoice berarti **tidak ada sisa pembayaran pasien** (juga untuk invoice berpenjamin penuh yang masih OPEN); kolom Status Running Invoice tetap menampilkannya apa adanya.

## 4. Berkas

`billing-invoices-view.jsx`, `billing-invoices-view.module.css`, `billing-invoice-constants.js` (`BILLING_INVOICE_PATIENT_PAYER_BADGE_CONFIG`), `billing-payment-progress-state.js` (baru), `billing-payment-progress.jsx`, `billing-payment-progress.module.css`, `use-billing-settlement.js` (`awaitingSync`, `syncBaseline`), `menu-pembayaran-view.jsx`, `tests/unit/billing-payment-progress-state.test.mjs` (baru).

## 5. Belum dikerjakan

| Hal | Status |
|---|---|
| Verifikasi visual Running Invoice terhadap screenshot acuan | Screenshot tidak tersedia pada permintaan ini; perapian mengikuti pola halaman daftar kanonik, belum dibandingkan piksel demi piksel |
| Verifikasi runtime, responsif, dan unit test `node --test` | Belum dijalankan (hanya ESLint) |
| Kolom Status Running Invoice untuk invoice berpenjamin penuh yang OPEN | Tetap "Lunas" (status pembayaran Backend); mengubahnya memerlukan keputusan produk dan kontrak Backend |

# Amendment 22 Oktober 2026 — Perbaikan "traits is not defined" pada Terima Pembayaran Iuran Membership (Revisi 2.8)

> `revisi blueprint`: `2.8`, status **diterapkan di source, belum dibuild atau diuji runtime**. Tidak ada perubahan Backend: audit baca-saja tidak menemukan cacat. Frontend branch lokal `yasmina` (branch `Yasmina` tidak ada di remote lokal; HEAD merge dari `QuilvianIntegrationFrontend`).

## 1. Akar masalah (terbukti dari kode, bukan dugaan)

`use-membership-fee-charge-detail.js`, fungsi `submitPayment`, mengirim `isManualCashierConfirmed: !traits.isCash` dan `confirmationNote: !traits.isCash ? … : null` pada argumen `addBillingTender`, tetapi `traits` tidak dideklarasikan, di-import, maupun menjadi parameter di berkas itu. `validatePayment` hanya mengambil `{ errors }` dan `submitPayment` hanya `{ reference }` dari `validateMembershipFeePaymentForm`, yang sebenarnya mengembalikan `{ errors, traits, reference }`. Literal argumen dievaluasi **sebelum** `dispatch`, sehingga `ReferenceError` terlempar di dalam `submitPayment` (tidak lewat slice, jadi tanpa `statusCode`) dan tampil sebagai "traits is not defined" di modal dan toast "Pembayaran Gagal".

Urutan di `submitPayment` membuat gejala di lapangan konsisten: settlement MEMBERSHIP_FEE sudah dibuat (POST settlements berhasil) → `ReferenceError` sebelum POST tender → tidak ada tender → `GET …/payments` mengembalikan `200 data: []`. Itu hasil sah (belum ada tender), bukan bukti POST gagal. Pemakaian lain `traits` (modal bayar, modal konfirmasi manual, util) sudah dideklarasikan benar.

## 2. Perbaikan Frontend

| Berkas | Perubahan |
|---|---|
| `membership-fee-charge-utils.js` | `buildMembershipFeeTenderPayload` (fungsi murni): satu-satunya tempat `traits` diturunkan untuk muatan tender; isian tidak valid → `payload: null`. `validateMembershipFeePaymentForm` kini juga memeriksa rekening aktif milik metode dan shift kasir (`hasActiveShift`; `undefined` = belum diketahui, backend yang memutuskan). Baru: `evaluatePaymentPreflight`, `resolveTenderResult`, `pickReceiptPayment`, `classifyPaymentError`, `isPendingTenderStatus` |
| `use-membership-fee-charge-detail.js` | Tidak lagi memakai `traits`; muatan dari util. `loadLatestState` memuat tagihan + riwayat bersamaan (riwayat `[]` = kosong yang sah). `submitPayment` dirombak (§3). `hasActiveShift` dari state shift |
| `membership-fee-pay-modal.jsx` | Prop `hasActiveShift`; peringatan shift untuk semua metode; `onExited` mengembalikan langkah ke isian (sebelumnya langkah konfirmasi dan centang verifikasi terbawa ke pembukaan berikutnya); klik yang diabaikan karena sedang diproses (`null`) tidak memindahkan tampilan |
| `membership-fee-charge-detail-view.jsx` | Meneruskan `hasActiveShift` |

## 3. Alur bayar (`submitPayment`)

1. **Lanjutkan Konfirmasi** hanya memanggil `validatePayment` (tanpa dispatch). **Konfirmasi Pembayaran** adalah satu-satunya yang memanggil `submitPayment`.
2. Kunci kirim (`payInFlightRef`) dipasang sebelum `await` pertama; klik ganda dibalas `null` dan tidak membuat tender kedua.
3. Baca ulang tagihan + riwayat dari Backend. Gagal dimuat → tidak ada yang dikirim. `evaluatePaymentPreflight`: tagihan masih dapat dibayar; tidak ada tender PENDING/CREATED; nominal = nominal yang dikonfirmasi di layar (iuran dibayar penuh; `AllowPartialPayment = false` tetap di Backend).
4. Settlement MEMBERSHIP_FEE terbuka dilanjutkan (`openSettlementId`, termasuk sisa percobaan lama yang berhenti sebelum tender); bila tidak ada, dibuat satu. Backend menolak settlement kedua untuk tagihan yang sama.
5. Satu tender penuh (`Idempotency-Key`, `ExpectedRowVersion` sesi). Non-tunai dikirim `isManualCashierConfirmed: true` + catatan; tunai tidak.
6. Status tender dibaca dari respons (`resolveTenderResult`), status akhir dari tagihan yang dimuat ulang (`buildPaymentOutcome`). PAID/aktif hanya bila Backend menyatakannya. Kwitansi hanya untuk tender `SUCCEEDED` di riwayat Backend.

## 4. Penanganan galat (`classifyPaymentError`)

| Jenis | Kondisi | Perilaku |
|---|---|---|
| `CONFLICT` | 409 | Satu toast peringatan, sesi ditutup, data dimuat ulang; tidak ada kirim ulang otomatis |
| `UNCERTAIN` | Tanpa jawaban server / 408 / 5xx / respons settlement tanpa ID | Satu toast peringatan "Hasil Pembayaran Belum Pasti", sesi ditutup, data dimuat ulang. Pembayaran baru hanya lewat pemeriksaan sebelum-kirim di atas (tender PENDING/CREATED atau status berubah menahan pembayaran) |
| `REJECTED` | 4xx lain | Satu pemberitahuan di dalam modal, isian dipertahankan; kunci idempotensi langkah itu diganti |
| `RUNTIME` | Galat aplikasi (tanpa `statusCode`) | Dicatat ke konsol non-produksi; kasir hanya melihat pesan umum, bukan teks teknis |
| Tender `FAILED/EXPIRED/REVERSED` (200) | Ditolak tegas | Pemberitahuan di modal; kunci tender diganti; tidak ada kwitansi |

Alasan sesi ditutup pada hasil tidak pasti: hash payload tender di Backend memuat `ExpectedRowVersion`, sehingga mengirim ulang dengan kunci yang sama setelah versi settlement berubah akan ditolak 409. Membaca ulang lalu memulai sesi baru lebih aman daripada memutar ulang secara buta.

## 5. Audit Backend (baca-saja, tanpa perubahan)

| Butir | Temuan |
|---|---|
| `GetPayments` / `GetPaymentsAsync` | Mengembalikan `List` (kosong bila tidak ada tender) dalam `ApiResponse` 200; 404 hanya bila tagihan tidak ada |
| `CreateAsync` settlement | Menerima `Purpose = MEMBERSHIP_FEE` + `MembershipFeeChargeId`; menolak target dengan settlement aktif (409); idempotent per kunci |
| `AddTenderAsync` | Referensi wajib untuk konfirmasi kasir manual; rekening wajib untuk transfer; referensi duplikat pada tender SUCCEEDED → 409; shift aktif wajib untuk tunai dan konfirmasi manual non-tunai; hash payload memuat `ExpectedRowVersion` |
| Pelunasan penuh | `ValidateSettlementTargetAsync`: `AllowPartialPayment = false` → nominal harus sama dengan sisa |
| PAID dan aktivasi | `ReconcileSettlementAsync` menerapkan dana setelah tender sukses; aktivasi membership lewat `TryConfirmMembershipFeeAsync` / `ConfirmChargeAsync` setelah commit |
| Settlement yatim | `BuildDetailAsync` menampilkannya sebagai `OpenSettlementId` dengan sisa penuh, sehingga FE melanjutkannya |

## 6. Pengujian

`tests/unit/membership-fee-charge-utils.test.mjs`: muatan Tunai / Transfer Mandiri / QRIS, penolakan isian tidak valid (rekening, referensi kosong/spasi, rekening nonaktif atau milik metode lain), aturan shift, klasifikasi galat, status tender, pemeriksaan sebelum-kirim, kwitansi hanya dari tender berhasil, riwayat `[]` sah, dan kontrak sumber (tidak ada `traits` tanpa deklarasi; Lanjutkan Konfirmasi tidak mengirim; kunci kirim sebelum `await`; baca-ulang sebelum settlement; satu tender). **Belum dijalankan** (hanya ESLint pada berkas yang diubah: bersih).

## 7. Belum dikerjakan / batas

| Hal | Status |
|---|---|
| Verifikasi runtime, `node --test`, dan stack trace asli | Belum; akar masalah ditetapkan dari pembacaan kode sumber lokal. Bila bundle yang berjalan berbeda dari source ini, hasil harus dikonfirmasi ulang |
| Settlement Draft yatim dari percobaan yang gagal sebelum perbaikan | Dilanjutkan otomatis oleh pembayaran berikutnya; tidak ada pembersihan data |
| Centang "Saya telah memverifikasi bukti transaksi" untuk non-tunai | Dipertahankan (pernyataan kasir yang sesuai `isManualCashierConfirmed`); syarat tombol Konfirmasi Pembayaran |


# Amendment 23 Oktober 2026 — Kesiapan Finalisasi, Hitung Ulang Tagihan di Modal, dan Angka Benefit dari Kalkulasi Tertinggal (Revisi 2.9)

> `revisi blueprint`: `2.9`, status **diterapkan di source, belum dibuild atau diuji runtime**. Frontend: branch lokal `yasmina` (branch `Yasmina` pada remote tidak dapat diverifikasi dari sesi ini; pekerjaan hanya pada working tree yang sedang aktif, tanpa pindah branch). Kontrak Backend: lihat `02-backend-architecture.md` §19 (kode `BIL-FIN-023..026`, tanpa endpoint baru).

## 1. Masalah

Modal Finalisasi hanya menerima `employeeBenefitSplit`. Saat Backend menyatakan kalkulasi tersimpan tertinggal (`calculationCurrent=false`; mis. Barang Habis Pakai Rp5.000 ditambahkan sesudah versi 1), modal tetap menampilkan pembagian dari versi lama — "Bayar Langsung Rp0" — mengirim `directPayAmount: 0`, dan baru ditolak 422 generik. Halaman Menu Pembayaran pun memakai `remainingPatientObligation` dan status "LUNAS" dari versi lama sebagai angka otoritatif.

## 2. Perilaku baru

| Situasi | Perilaku |
| --- | --- |
| Modal Finalisasi dibuka | Kesiapan dimuat ulang dari Backend; tombol Finalisasi ditahan selama pemuatan |
| Kalkulasi tertinggal (`BIL-FIN-023` / `calculationCurrent=false`) | Kartu **Kesiapan Finalisasi** menampilkan checklist (order, kalkulasi, benefit, kewajiban pasien — yang terakhir "belum dapat dipastikan"), peringatan "Tagihan mengalami perubahan. Hitung ulang tagihan sebelum melanjutkan finalisasi.", dan tombol **Hitung Ulang Tagihan**. Angka Benefit RS / Sisa Kewajiban / Bayar Langsung / AR Karyawan tampil **"Belum final"**, bukan Rp0. Tombol Finalisasi tidak aktif |
| Hitung Ulang Tagihan | Satu Recalculate terkontrol (endpoint `POST invoices/{id}/recalculate`, alasan tercatat), lalu invoice (RowVersion baru) dan preview (kesiapan + pembagian benefit) dimuat ulang. Hanya dari klik kasir; tidak ada hitung ulang atau kirim ulang finalisasi otomatis. Angka bayar langsung/AR diturunkan dari preview terbaru, jadi modal otomatis memakai angka baru |
| Order belum selesai, review, tarif, pelayanan tertunda, benefit tidak dapat dipakai | Tombol ditahan; `blockingReasons` Backend ditampilkan apa adanya |
| Kewajiban pasien belum lunas | **Tidak** menahan tombol (masih dapat diselesaikan sah): modal menjelaskan bahwa item non-cover dan kelebihan plafon dibayar di kasir (Tunai, QRIS, Transfer Bank) lewat Proses Pembayaran, atau dipilih penangguhan yang diizinkan. Tidak ada piutang otomatis |
| Departure exception dipilih | `BIL-FIN-022` (pelayanan tertunda) tidak menahan — sama dengan Backend (BKC-DEC-033); penahan lain dan kalkulasi tertinggal tetap menahan |
| 422 dari Backend | Pesan kini menyebut penyebab; checklist 422 menggantikan preview (reducer sudah ada), sehingga modal langsung menampilkan alasan dan aksi hitung ulang. Isian form yang valid dipertahankan |
| Menu Pembayaran saat tertinggal | `remainingPatientObligation` benefit dan status "LUNAS" invoice/settlement tidak dipakai sebagai angka otoritatif (`areBenefitNumbersAuthoritative`, `isStoredCalculationStale`); layar memakai pratinjau terbaru sampai dihitung ulang. Menambah Biaya Lain-lain kini juga memuat ulang kesiapan finalisasi |

## 3. Komponen dan berkas

| Berkas | Peran |
| --- | --- |
| `billing-finalization-readiness.js` (baru) | Fungsi murni: `resolveFinalizationReadiness(preview, { isDepartureException })`, `resolveFinalizationSettlementAmounts`, `resolveTotalObligation`, konstanta kode `FINALIZATION_BLOCKING_CODE` dan alasan Recalculate |
| `use-billing-finalization.js` | Parameter opsional `runRecalculate`/`reload`; mengembalikan `readiness`, `recalculate`, `recalculating`, `settlementAmounts`; `form` berisi angka turunan; pagar `canAttemptFinalize` sebelum `dispatch`; `openFinalize` memuat ulang preview |
| `finalize-invoice-modal.jsx` + `.module.css` | Props `readiness`, `previewLoading`, `onRecalculate`, `recalculating`, `paidAmount`; kartu kesiapan; angka "Belum final"; metrik Sisa Kewajiban Pasien dan Sudah Dibayar; tombol ditahan (`notReady`) |
| `use-menu-pembayaran.js`, `menu-pembayaran-view.jsx` | Teruskan `runRecalculate`/`reload`; gating angka benefit dan status lunas; props modal |
| `detail-invoice-billing-view.jsx` | Teruskan `runRecalculate`/`reload` dan props modal |
| `tests/unit/billing-finalization-readiness.test.mjs` (baru) | 16 tes: kasus lapangan, kode, jalur departure, angka bayar langsung/AR, kontrak sumber hook/modal/Menu Pembayaran, kontrak Backend |

## 4. Alur yang diharapkan

Item covered → Benefit RS · Item non-covered → kewajiban pasien · Kewajiban pasien → Tunai / QRIS / Transfer Bank (Proses Pembayaran; tender SUCCEEDED; PENDING tidak dihitung lunas) · Kalkulasi terkini + kesiapan valid → Finalisasi · Finalisasi → handoff Finance (tidak berubah).

## 5. Belum dikerjakan / batas

| Hal | Status |
| --- | --- |
| Verifikasi runtime, responsif, dan `node --test` | Belum dijalankan (hanya ESLint: 0 error; satu peringatan lama `set-state-in-effect` di hook finalisasi bukan dari perubahan ini) |
| Penangguhan AR yang dipra-pilih (`outstandingMode` bawaan `DEFER_TO_PATIENT_AR`) | Tidak diubah: tetap menuntut alasan, nomor perjanjian, jatuh tempo, dan centang konfirmasi; mengubah bawaan adalah keputusan produk |
| Panel "Preview dan Finalisasi Invoice" pada Detail Invoice Billing | Tidak diubah selain pemasangan props modal; kolom "Kalkulasi Terkini" sudah menampilkan status Backend |


# Amendment 23 Oktober 2026 — Tahap RS Benefit: Konfirmasi Skema Sebelum Pembayaran Pribadi (Revisi 3.0)

> **DIGANTIKAN oleh Revisi 3.1** (Tahap A/B dihapus atas keputusan pemilik; baca Revisi 3.1 di bawah untuk perilaku yang berlaku). `revisi blueprint`: `3.0`, status **diterapkan di source, belum dibuild atau diuji runtime**. Frontend branch lokal `yasmina` (branch `Yasmina` di remote tidak dapat diverifikasi; pekerjaan hanya pada working tree aktif). Backend tidak berubah: `02-backend-architecture.md` §20.

## 1. Akar masalah tombol

Tombol "Lanjut ke Finalisasi Benefit" ada di drawer Pilih Metode Pembayaran (`add-tender-modal.jsx`, kartu Benefit Karyawan RS):

```
disabled={busy || !canFinalizeBenefit || benefit.patientPortion > 0}
```

Dengan kewajiban pribadi (K) Rp5.000 `patientPortion > 0`, sehingga tombol **selalu** disabled. Alasannya hanya `title` pada tombol disabled (tidak tampil andal) — jadi tampak rusak. Faktor lain diperiksa dan **bukan** penyebab: `canFinalizeBenefit` (= invoice OPEN dan izin; benar), `calculationCurrent` (angka layar sudah hasil hitung ulang), `financingChoice` (tidak disentuh oleh drawer/pembayaran), overlay, dan handler (`onOpenBenefitFinalization` hanya `finalization.openFinalize()`).

Mengapa K > 0 dikunci: `onOpenBenefitFinalization` membuka modal **Finalisasi** (POST saat submit), dan Backend menolak finalisasi selama sisa pasien belum lunas. Jadi tidak ada langkah "konfirmasi skema" sebelum K dibayar; kasir dipaksa membayar dulu baru boleh meninjau Benefit RS — kebalikan dari prioritas bisnis.

## 2. Alur sekarang vs target

| Urutan | Sebelum | Sesudah |
| --- | --- | --- |
| 1 | Kasir memilih Benefit RS di drawer | sama; kartu terbuka otomatis sampai skema dikonfirmasi |
| 2 | Tombol finalisasi terkunci selama K > 0 | **Tahap A — Konfirmasi Skema Benefit** (tinjau; tanpa POST) dapat dilakukan walau K > 0 |
| 3 | Bayar K lewat Tunai/QRIS/Transfer | Tahap B: bayar K (settlement yang ada) |
| 4 | Finalisasi (konfirmasi benefit diminta lagi di modal) | Modal Finalisasi meneruskan konfirmasi Tahap A bila angka skema sama; finalisasi tetap dipagari kesiapan Backend |
| 5 | Handoff Finance | tidak berubah |

## 3. Perilaku baru

| Elemen | Perilaku |
| --- | --- |
| Tahap A | Tombol **Konfirmasi Skema Benefit**: aktif selama benefit dapat dipakai dan angka otoritatif; **tidak** terkunci oleh K. Hanya mengubah status layar (sidik jari: versi kalkulasi, pemilik, hubungan, porsi aturan, porsi Benefit RS, kelebihan plafon; **tidak** memuat K/pembayaran). Tidak ada POST, plafon tidak dipotong, invoice tidak difinalisasi |
| Tahap B | Tombol **Lanjut ke Finalisasi Invoice** (atau "Finalisasi: Tangguhkan ke AR Karyawan" bila ada kelebihan plafon) hanya membuka modal Finalisasi. Bila belum siap, **alasan tampil** di bawah tombol (`role="status"`) dengan aksi: bayar kewajiban pribadi, atau Hitung Ulang Cakupan bila kalkulasi tertinggal |
| Penghalang | `NOT_OPEN`, `LOADING`, `UNUSABLE` (alasan HR), `STALE` (aksi hitung ulang), `ORDERS_INCOMPLETE`, `HARD_BLOCK` (alasan Backend), `PERSONAL_DUE` (aksi bayar) — satu sumber teks (`describeBenefitSchemeBlock`) |
| Kalkulasi tertinggal | Kartu tidak menampilkan angka lama; menampilkan peringatan dan **Hitung Ulang Cakupan** |
| Pilihan pembiayaan | Memilih atau membayar lewat Tunai/QRIS/Transfer **tidak** menyentuh `financingChoice` (diuji: drawer dan `use-billing-settlement.js` tidak memuatnya) |
| Halaman utama | Setelah Tahap A, banner "Skema Benefit RS sudah dikonfirmasi (Tahap A)" menunjukkan sisa kewajiban pribadi dan bahwa plafon belum dipotong |
| Modal Finalisasi | Centang konfirmasi alokasi diteruskan dari Tahap A **hanya** selama sidik jari sama; bila angka berubah otomatis lepas |
| Label nominal | "Total Biaya" → **Subtotal Item (sebelum pajak)**; "Porsi Sesuai Aturan Penjamin" → **… (termasuk pajak)**; "Sisa Tagihan Pasien (K)" → **Kewajiban Pribadi, Item Non-Cover (K)** (rekonsiliasi di `02-backend-architecture.md` §20.4) |

## 4. Berkas

| Berkas | Perubahan |
| --- | --- |
| `billing-finalization-readiness.js` | `buildBenefitSchemeFingerprint`, `resolveBenefitSchemeStages`, `describeBenefitSchemeBlock`, konstanta `BENEFIT_SCHEME_BLOCK/ACTION` |
| `use-billing-finalization.js` | Status konfirmasi (terikat invoice + sidik jari), `benefitScheme`, `confirmBenefitScheme` (tanpa dispatch), carry-over ke finalisasi |
| `add-tender-modal.jsx` + `payment-method-picker.module.css` | Tahap A/B, alasan terlihat, label nominal, kartu stale |
| `finalize-invoice-modal.jsx` | Prop `schemeConfirmed` (centang terkunci bila sama dengan Tahap A) |
| `menu-pembayaran-view.jsx` | Props drawer dan banner Tahap A |
| `tests/unit/billing-benefit-scheme-stages.test.mjs` (baru) | 15 tes: Tahap A/B, sidik jari, penghalang, kontrak sumber drawer/hook/view, kontrak Backend |

## 5. Belum dikerjakan / batas

| Hal | Status |
| --- | --- |
| Verifikasi runtime, tampilan mobile/desktop, `node --test` | Belum (hanya ESLint: 0 error) |
| Konfirmasi tahap A tidak bertahan setelah muat ulang halaman | Disengaja: status layar, bukan data; kasir cukup mengonfirmasi lagi (angka sama dalam satu klik) |
| Memblokir pembayaran K sebelum Tahap A | Tidak dilakukan: pembayaran pribadi tetap independen agar tidak merusak alur kasir yang ada; prioritas Benefit RS ditegakkan lewat urutan tampilan (kartu terbuka, tombol utama) |


# Amendment 23 Oktober 2026 — RS Benefit Otomatis sebagai Baris Pertama Metode Pembayaran (Revisi 3.1)

> `revisi blueprint`: `3.1`, status **diterapkan di source, belum dibuild atau diuji runtime**. **Menggantikan Revisi 3.0** (Tahap A/B dihapus atas keputusan pemilik). Frontend branch lokal `yasmina`; Backend tidak berubah (`02-backend-architecture.md` §21).

## 1. Struktur akhir

Daftar Metode Pembayaran (halaman Menu Pembayaran), bila kunjungan memakai Benefit Karyawan RS:

```
Metode Pembayaran
1. Benefit Karyawan RS          [Otomatis · Ditanggung RS]
   Eligible · Bayu Ramadhan (EMP-001) · Sisa plafon Rp9.440.648,80      Nominal  Rp788.544
2. Tunai                        (Copy Nominal)                           Nominal  Rp5.000
+ Tambah Metode Pembayaran

Ringkasan: Total Tagihan · Ditanggung Benefit RS · Kewajiban Pribadi · Sudah Dibayar · Draft · Sisa Belum Dibayar
Bilah progres: [alokasi Benefit RS ░░░ | terkonfirmasi | draft]   (Benefit RS = alokasi penjamin, bukan kas masuk)
[Proses Pembayaran]    [Finalisasi Invoice]  (di luar drawer; ditahan dengan alasan bila belum siap)
```

Seluruh angka dari preview Backend; tidak ada rumus baru di klien.

## 2. Perilaku

| Elemen | Perilaku |
| --- | --- |
| Baris Benefit RS | Baris pertama otomatis (nomor 1) dari `employeeBenefitSplit`; read-only (tanpa input, edit, hapus); tidak punya `paymentMethodId`; tidak masuk `settlement.rows`; tidak pernah menjadi `BilTender`. Status: `ACTIVE` (nominal tampil), `STALE` (kalkulasi tertinggal: tanpa angka, tombol Hitung Ulang Cakupan), `UNUSABLE` (alasan HR dari Backend; pembiayaan lain mengikuti aturan yang ada, tidak ada peralihan otomatis ke bayar pribadi) |
| Baris pribadi pertama | Untuk kunjungan benefit saja: metode **Tunai** dipilih dan nominal memakai **Copy Nominal** yang sudah ada (mengikuti sisa terbaru dari Backend dikurangi baris lain). Sekali per baris — mengganti metode (QRIS, Transfer Bank, Debit, ...), mematikan Copy Nominal, atau membagi nominal ke beberapa baris tidak pernah ditimpa. Baris tambahan lewat "+ Tambah Metode Pembayaran" tetap kosong (aturan umum tidak diubah). Tidak ada tender atau baris yang ditambahkan otomatis |
| Drawer Pilih Metode Pembayaran | Kartu Benefit ringkas: Benefit Karyawan RS, Eligible, pemilik, Ditanggung Benefit RS (otomatis), Sisa plafon, Sisa kewajiban pribadi. Tanpa tahap, tombol konfirmasi/finalisasi, atau penjelasan panjang |
| Ringkasan | Menambah "Ditanggung Benefit RS"; label "Kewajiban Pribadi" dan "Sisa Belum Dibayar" untuk kunjungan benefit (selalu terlihat) |
| Bilah progres | Segmen alokasi Benefit RS di depan (titik-titik, terbedakan tanpa warna); segmen pribadi mengisi sisa lebar. "Lunas", terkonfirmasi, draft, dan pending tetap hanya untuk kewajiban pribadi; draft 100% tidak pernah Lunas. Tanpa penjamin hasilnya identik dengan sebelumnya |
| Finalisasi Invoice utama | Di bawah Proses Pembayaran, di luar drawer. Hanya membuka modal Finalisasi; finalisasi tetap dipagari kesiapan Backend. Aktif bila kalkulasi terkini, order lengkap, tanpa penahan, dan sisa pribadi lunas; selain itu tetap terlihat dengan alasan dan langkah (bayar kewajiban pribadi / Hitung Ulang Cakupan). Pada fase "menunggu konfirmasi Benefit RS" (tanpa kewajiban pribadi) tombol utama Proses Pembayaran sudah membuka modal Finalisasi, jadi tombol ini tidak diduplikasi |
| Konfirmasi alokasi | Tetap satu: centang di modal Finalisasi (kontrak yang ada). Tidak ada tahap konfirmasi terpisah |
| Finalisasi otomatis | Backend memfinalisasi otomatis setelah tender sukses bila siap (`02-backend-architecture.md` §21.1); tombol utama adalah cadangan saat itu tertahan |
| Stale | `remainingPatientObligation` benefit dan status "LUNAS" invoice dari versi lama tidak dipakai sebagai angka otoritatif (Rev 2.9) |

## 3. Berkas

| Berkas | Perubahan |
| --- | --- |
| `billing-finalization-readiness.js` | `resolveBenefitAllocation`, `resolveMainFinalizeAction`, `describeFinalizeBlock`, konstanta `BENEFIT_ALLOCATION_STATE`/`FINALIZE_BLOCK`/`FINALIZE_ACTION` (menggantikan fungsi Tahap A/B) |
| `billing-benefit-row-defaults.js` (baru) | `planBenefitRowDefaults`: default baris pribadi pertama (murni, diuji) |
| `use-billing-finalization.js` | `benefitAllocation`, `mainFinalizeAction`; sisa konfirmasi Tahap A dihapus |
| `use-menu-pembayaran.js` | Efek default baris pribadi (hanya `updateRow`) |
| `benefit-allocation-row.jsx` (baru), `menu-pembayaran.module.css` | Baris Benefit RS read-only |
| `menu-pembayaran-view.jsx` | Baris Benefit RS, ringkasan, tombol Finalisasi Invoice utama, props drawer/modal |
| `add-tender-modal.jsx`, `payment-method-picker.module.css` | Kartu Benefit ringkas; tahap/tombol/penjelasan dihapus |
| `finalize-invoice-modal.jsx` | Checkbox konfirmasi kembali seperti semula |
| `billing-payment-progress-state.js`, `billing-payment-progress.jsx`, `.module.css` | Segmen alokasi penjamin |
| `tests/unit/billing-benefit-allocation.test.mjs` (baru, menggantikan tes Tahap A/B) | 26 tes |

## 4. Belum dikerjakan / batas

| Hal | Status |
| --- | --- |
| Verifikasi runtime, mobile/desktop, `node --test` | Belum (hanya ESLint: 0 error) |
| Aturan "baris baru selalu kosong" pada hook settlement | Tidak diubah (dikunci tes pihak lain); default benefit dilakukan lewat `updateRow` di hook halaman |
| Default Tunai | Hanya bila metode Tunai ada pada master metode yang tersedia; bila tidak ada, hanya nominal yang terisi |


# Amendment 23 Oktober 2026 — Split Payment Saldo Deposit Pasien (RANAP): Pilih Metode = Draft (Revisi 3.2)

> `revisi blueprint`: `3.2`, status **diterapkan di source, belum dibuild atau diuji runtime**. Frontend branch lokal `yasmina`; Backend tidak berubah (`02-backend-architecture.md` §22).

## 1. Akar masalah

Drawer "Pilih Metode Pembayaran" meleburkan **memilih** dan **memotong** deposit: setelah "Gunakan Deposit", tombol utama berlabel "Bayar dengan Deposit" dan memanggil `onPayWithDeposit` → `allocateDepositDirectly` (POST alokasi) saat itu juga. Akibatnya saldo terpotong sebelum kasir selesai menyusun pembayaran, pilihan deposit hanya hidup di state lokal modal (tidak tampil di daftar Metode Pembayaran, ringkasan, atau progress), dan sisa tidak bisa dipecah ke Tunai/QRIS/Transfer/Debit sebelum apa pun diproses.

## 2. Struktur akhir

```
Drawer "Pilih Metode Pembayaran"  (RANAP)
  Saldo Deposit Pasien (RANAP)   Saldo Tersedia · Sisa Tagihan · Dipilih dari Deposit (draft) · Sisa Setelah Draft ·
                                 Saldo Deposit Setelah Dialokasikan (Estimasi)
  Tunai · QRIS · Transfer Bank · Debit/Lainnya
  [Batal]  [Pilih Metode]        <- tidak ada POST, saldo tidak berkurang

Menu Pembayaran > Metode Pembayaran
  1. Deposit Pasien   Draft · Belum Diproses   Rp1.000.000     (nominal bisa diubah, bisa dihapus)
  2. Metode Pembayaran  [+ Pilih Metode]       Rp0
  + Tambah Metode Pembayaran
Ringkasan: Sisa Tagihan Sebelum Pembayaran Rp1.248.734 · Draft Deposit Pasien Rp1.000.000 · Sisa Belum Dialokasikan Rp248.734
           (+ Tunai 248.734 → Total Draft Rp1.248.734 · Sisa Belum Dialokasikan Rp0) · Saldo Deposit Setelah Dialokasikan (Estimasi)
[Proses Pembayaran]              <- satu-satunya yang memulai transaksi Backend
```

## 3. Perilaku

| Elemen | Perilaku |
| --- | --- |
| "Pilih Metode" (deposit) | Memeriksa nominal terhadap kutipan Backend TERBARU (GET, hanya baca), menyimpan **draft** frontend, menutup drawer. Tidak ada POST alokasi, saldo tidak dipotong, tidak ada settlement/`BilPaymentAllocation`, invoice tidak lunas |
| Draft | `{localId, type:"DEPOSIT", encounterId, invoiceId, depositAccountId, amount, reason, status}`; satu per invoice; **bukan** baris tender dan tidak punya `PaymentMethodId`. Status: `DRAFT` → `PROCESSING` → `SUCCEEDED` (dihapus setelah total terkonfirmasi mencerminkannya) / `UNKNOWN` (hasil tidak pasti) / kembali `DRAFT` (ditolak atau 409) |
| Nominal awal | min(saldo deposit, sisa tagihan); kasir boleh mengubah (inline pada baris draft). Saldo tidak pernah dialokasikan seluruhnya otomatis. Nominal di atas saldo atau sisa tagihan ditolak (overpayment ke saldo refund tidak diaktifkan; lihat §6) |
| Sisa untuk metode lain | Draft memesan nominalnya dari sisa yang boleh dialokasikan ke baris lain (`reservedDepositAmount`): "Sisa Belum Dialokasikan" dan Copy Nominal langsung menghitung sisa setelah deposit; mengubah/menghapus draft memperbaruinya saat itu juga, tanpa transaksi |
| Ringkasan | Sisa Tagihan Sebelum Pembayaran · Draft Deposit Pasien · Draft Pembayaran Lain · Total Draft · Sisa Belum Dialokasikan · Saldo Deposit Setelah Dialokasikan (Estimasi) |
| Progress | `ConfirmedPaidAmount` (tidak berubah oleh draft) dipisah dari `draftDepositAmount`, `draftTenderAmount`, dan pratinjau; deposit yang sedang dikirim = diproses; "Lunas" tetap hanya dari Backend |
| Proses Pembayaran | (1) draft deposit dikirim lebih dulu lewat `allocateDepositDirectly` (kutipan terbaru → token versi deposit/invoice/kalkulasi; kunci idempotensi sendiri); (2) hanya bila tercatat, baris Tunai/QRIS/Transfer/Debit lewat settlement + tender seperti biasa; (3) langkah penutup yang sudah ada (muat ulang, evaluasi kesiapan finalisasi, laporan hasil). Deposit saja tidak membuat settlement kosong |
| Kegagalan sebagian | Deposit gagal/ditolak/409: tidak ada tender dikirim, draft dan baris tender dipertahankan, data dimuat ulang, tanpa percobaan ulang otomatis. Deposit tercatat + tender gagal: deposit tidak dikirim ulang, baris gagal dipertahankan, toast menjelaskan, kasir menekan Proses Pembayaran lagi (hanya sisa yang dikirim). Hasil deposit tidak pasti: nominal dikunci, dikirim ulang dengan kunci yang sama (replay Backend) |
| Konsistensi tampilan | Draft deposit tidak dihitung ganda setelah dikirim: bagian yang belum tercermin = nominal − kenaikan total terkonfirmasi sejak percobaan pertama (`confirmedBaseline`) |
| Riwayat Pembayaran | Tidak berubah: hanya pembayaran berhasil; draft tidak pernah tampil di sana, deposit yang sudah dialokasikan tidak tampil sebagai form baru |

## 4. Berkas

| Berkas | Perubahan |
| --- | --- |
| `billing-deposit-draft.js` (baru) | Logika murni: bentuk draft, validasi, ringkasan, pemesanan sisa, transisi status, klasifikasi galat, validasi terhadap kutipan |
| `use-billing-deposit-draft.js` (baru) | State draft frontend (tanpa Backend) |
| `deposit-draft-row.jsx` (baru) | Baris "Deposit Pasien" (Draft/Belum Diproses), nominal inline, hapus |
| `add-tender-modal.jsx` | Tombol utama selalu "Pilih Metode"; `onSelectDeposit` menggantikan `onPayWithDeposit`; label blok deposit; batas nominal |
| `menu-pembayaran-view.jsx` | `handleSelectDeposit`, baris draft, ringkasan, parameter progress; `handlePayWithDeposit` dihapus |
| `use-menu-pembayaran.js` | Draft hook, `runDepositDraftStep`, integrasi `startPayment` (deposit → tender), deposit hook dipindah di atas settlement |
| `use-billing-settlement.js` | `depositDraft` memesan sisa; `submittableRowCount`; `submitAllocations` tidak membuat settlement kosong; `confirmedOffset` |
| `billing-split-payment-rows.js`, `billing-payment-progress-state.js`, `billing-payment-progress.jsx` | `evaluatePaymentSubmission` (reserved), `isPristineRow`, progress deposit |
| `tests/unit/billing-deposit-draft.test.mjs` (baru) | 43 tes |

## 5. Alur API

| Langkah | Panggilan |
| --- | --- |
| Pilih Metode | `GET patient-funds/deposits/{encounterId}/allocations/quote` (hanya baca) |
| Proses Pembayaran, deposit | `GET .../allocations/quote` → `POST patient-funds/deposits/{encounterId}/allocations` (Idempotency-Key, `ExpectedDepositRowVersion`, `ExpectedInvoiceRowVersion`, `ExpectedCalculationVersion`) |
| Proses Pembayaran, tender | rekalkulasi bila perlu → `POST patient-funds/settlements` → `POST settlements/{id}/tenders` per baris (urutan, Idempotency-Key, `ExpectedRowVersion`) |
| Setelah | muat ulang settlement, invoice, pratinjau, saldo deposit, kesiapan finalisasi |

## 6. Belum dikerjakan / batas

| Hal | Status |
| --- | --- |
| Verifikasi runtime, mobile/desktop | Belum (ESLint file berubah: 0 masalah; `node --test` hanya file billing terkait) |
| Atomisitas deposit + tender | Tidak ada di Backend (§22); dilaporkan sebagai kegagalan sebagian, bukan disembunyikan |
| Overpayment deposit → saldo refund | Diizinkan Backend, **tidak diaktifkan** di layar; keputusan Finance |
| Draft hilang saat halaman dimuat ulang | Disengaja (draft bukan transaksi; baris tender draft juga hanya state lokal) |
| Tabrakan dengan pekerjaan paralel | `startPayment`/`concludePayment` (langkah penutup tombol tunggal) dipakai apa adanya; deposit dihitung satu pembayaran sah pada ringkasan penutup |


# Amendment 26 Oktober 2026 — Banner dan Tombol Hitung Ulang Cakupan Bukan Alur Rutin (Revisi 3.3)

> `revisi blueprint`: `3.3`, status **diterapkan di source, belum dibuild atau diuji runtime**. Frontend branch lokal `yasmina` (branch `Yasmina` pada remote tidak dapat diverifikasi dari sesi ini — GitHub MCP gagal terhubung dan remote mengembalikan 404; pekerjaan hanya pada working tree aktif, tanpa pindah branch). Kontrak Backend: `02-backend-architecture.md` §23 (hitung otomatis di transaksi yang sama; field aditif `calculationStaleReason`).

## 1. Akar masalah

Banner "Perhitungan cakupan tersimpan belum terbaru" dan tombol "Hitung Ulang Cakupan" punya **dua pemicu**: penanda Backend (`calculationCurrent=false`) **dan** selisih pratinjau live vs versi tersimpan (`resolveCalculationFreshness`). Karena Backend dulu tidak menghitung saat item ditambah, pemicu pertama menyala setelah hampir setiap penambahan; pemicu kedua menyala bahkan untuk selisih wajar (biaya kamar RANAP bergerak menurut waktu). Kasir terbiasa menekan tombol yang secara bisnis seharusnya tak pernah perlu.

## 2. Perilaku sekarang

| Keadaan | Perilaku |
| --- | --- |
| Menambah item / biaya lain-lain / diskon berhasil | Backend sudah menulis versi kalkulasi di transaksi yang sama. Layar **memuat ulang** invoice (RowVersion + versi terbaru), pratinjau/rincian, dan preview finalisasi (pembagian Benefit RS). **Tidak ada** hitung ulang dari FE dan tidak ada perhitungan cakupan di sisi klien |
| Backend menolak (perhitungan gagal) | Pesan spesifik Backend ditampilkan apa adanya; tidak ada toast sukses; perubahan tagihan dibatalkan di Backend |
| Membuka / me-refresh / keluar-masuk Menu Pembayaran | Tidak menulis versi kalkulasi (hanya pratinjau baca-saja) |
| Selisih pratinjau vs tersimpan saja | **Tidak** memunculkan banner/tombol. Hanya dipakai untuk penyelarasan diam-diam di dalam aksi Proses Pembayaran (`needsSyncBeforePayment`), seperti sebelumnya |
| Backend melaporkan `calculationCurrent=false` (keadaan tidak normal: invoice lama, atau aturan tanggungan master berubah) | Peringatan **"Perhitungan tagihan belum selaras"** dengan kalimat spesifik menurut `calculationStaleReason`, angka Benefit RS ditandai belum final, dan tombol pemulihan **"Selaraskan Perhitungan"** (aksi eksplisit; `ExpectedRowVersion` + alasan + audit di Backend). Kegagalan pemulihan memakai pesan Backend; fallback: "Perhitungan tagihan belum berhasil diperbarui. Periksa kembali perubahan tagihan atau hubungi petugas yang berwenang." |
| Kesiapan finalisasi (`readiness`) | `needsRecalculation` tidak berubah (stale atau belum pernah dihitung); kini membawa `calculationStaleReason` |

## 3. Berkas

| Berkas | Perubahan |
| --- | --- |
| `use-menu-pembayaran.js` | `calculationFreshness.needsRecalculation` = penanda Backend saja; `needsSyncBeforePayment` untuk penyelarasan di dalam Proses Pembayaran; `staleReason`/`staleMessage`; `confirmAdhoc` memuat ulang invoice + pratinjau + preview finalisasi; callback diskon ikut memuat ulang preview finalisasi (hook diskon dipindah ke bawah finalisasi); toast pemulihan "Perhitungan Tagihan Diperbarui" hanya bila sukses |
| `menu-pembayaran-view.jsx` | Banner lama diganti peringatan keadaan tidak normal; tombol "Hitung Ulang Cakupan" → "Selaraskan Perhitungan" (tiga lokasi, semua pemulihan eksplisit) |
| `benefit-allocation-row.jsx`, `add-tender-modal.jsx`, `finalize-invoice-modal.jsx` | Teks mengikuti penamaan baru; modal Finalisasi menambahkan kalimat sebab dari Backend |
| `billing-finalization-readiness.js` | `CALCULATION_STALE_REASON`, `describeCalculationStaleReason`, `calculationStaleReason` pada readiness |
| `billing-invoice-calculation-breakdown.js`, `billing-payment-outcome.js` | Teks petunjuk/komentar tidak lagi merujuk tombol rutin |
| `use-billing-invoice-detail.js` | Fallback galat hitung ulang yang aman; komentar diperbarui (halaman tidak memicu hitung saat dibuka) |
| `tests/unit/billing-finalization-readiness.test.mjs` (+6 tes), `billing-invoice-calculation-breakdown.test.mjs`, `billing-single-payment-action.test.mjs` | Kontrak baru dikunci |

## 4. Belum dikerjakan / batas

| Hal | Status |
| --- | --- |
| Verifikasi runtime (browser, mobile/desktop) | Belum. Hanya ESLint (0 masalah) dan `node --test` pada 14 berkas billing terkait (234 tes lulus) |
| Perubahan penjamin / penetapan penanggung per item | Menu Pembayaran tidak punya alur ubah-penanggung. Halaman Edit Tagihan memakai `payment-source`, `item-payer-assignments`, dan `drug-billing-disposition`, yang di Backend **sudah** menghitung ulang secara atomik; hook-nya memanggil `onSaved` sesudah sukses dan tidak diubah. Perubahan penjamin di Registrasi berada di kepemilikan terpisah dan tidak diaudit |
| Kebijakan aturan tanggungan master yang berubah | NEEDS BUSINESS DECISION (`02-backend-architecture.md` §23.5); layar hanya menampilkan sebab `COVERAGE_RULE_CHANGED` |
