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
