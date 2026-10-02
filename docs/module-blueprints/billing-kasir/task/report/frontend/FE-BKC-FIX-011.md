# Laporan Perubahan Frontend — `FE-BKC-FIX-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-011` (ad-hoc, di luar roadmap, permintaan langsung pengguna dengan referensi visual) |
| Judul | Halaman baru "Riwayat Pembayaran" — daftar semua pembayaran lintas invoice/pasien, dengan Kwitansi per pembayaran dan keterangan Lunas/Cicilan |
| Slice | Independen — permintaan fitur baru |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — mengonsumsi endpoint baru `BE-BKC-FIX-008` |
| Wewenang UI | `REUSE` murni — seluruh elemen UI dari base component/library yang sudah dipakai modul ini (lihat § Base Component Decision Gate) |
| Dependency | `BE-BKC-FIX-008` (backend, belum di-rebuild) |
| Klasifikasi | `MEDIUM` — halaman baru + entri menu sidebar baru, murni REUSE base component, tidak ada base component baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Sonnet 5 |
| Tanggal | 7 September 2026 |
| Status | Source selesai, lint bersih (file ini + `eslint . --quiet` repo penuh PASS 0 error). `test:unit` **TIDAK dijalankan ulang** pada giliran ini (instruksi pengguna sebelumnya masih berlaku: hindari validasi berulang yang tidak diminta). `npm run build` **sengaja TIDAK dijalankan** — pengguna eksplisit meminta dua kali pada giliran ini ("tidak usah di run and build"/"tidak usah di build"). Belum di-commit. Belum diverifikasi manual ter-autentikasi |

---

## 1. Keadaan yang ditemukan di awal

Pengguna menunjukkan referensi visual (screenshot) sebuah halaman "Riwayat Pembayaran" — tabel
berisi seluruh pembayaran pasien lintas kunjungan, dengan kolom Tanggal Kunjungan/No. RM/Nama
Pasien/Tipe Pasien/Tipe Layanan/Total Tagihan/Nama Penjamin/Klaim/Status/Aksi, beserta contoh
Kwitansi yang menampilkan field "Angsuran N/M" dan "Status Pembayaran: CICILAN/LUNAS". Halaman ini
**tidak ada** di codebase manapun yang diperiksa (dikonfirmasi lewat grep "Riwayat Pembayaran" —
nol hasil sebelum task ini). Tiga keputusan scope dikonfirmasi eksplisit ke pengguna lewat
`AskUserQuestion` sebelum implementasi (lihat `task/report/backend/BE-BKC-FIX-008.md` § 1).

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir/petugas billing yang perlu mencari riwayat pembayaran pasien mana pun (bukan
hanya invoice yang sedang berjalan).

**Alur**: buka menu "Billing dan Kasir" → "Riwayat Pembayaran" (entri baru, tepat di bawah
"Running Invoice"). Cari/filter berdasarkan nama+No. RM, rentang tanggal kunjungan, atau jenis
layanan. Setiap baris = satu invoice yang sudah punya minimal satu pembayaran, menampilkan status
Lunas/Cicilan (dengan keterangan "Angsuran N/M" bila invoice itu dibayar lebih dari satu kali).
Tombol Aksi (⋮) membuka daftar SEMUA Kwitansi invoice itu — memilih salah satu membuka halaman
Dokumen Kasir (tab Kwitansi) di tab baru, REUSE penuh alur cetak Kwitansi yang sudah ada
(`FE-BKC-011`/`FE-BKC-017`) — **tidak ada komponen cetak baru**.

**Jalur tidak normal**: invoice tanpa Kwitansi (seharusnya tidak mungkin muncul di halaman ini,
karena filter backend hanya menyertakan invoice ber-tender `SUCCEEDED`) menampilkan "-" pada kolom
Aksi, bukan tombol kosong yang membingungkan.

## 3. Perubahan yang dikerjakan

### 3.1 Redux (`billing-invoice-slice.jsx`)

Thunk baru `getPaymentHistory` (`GET .../invoices/payment-history`), state
`paymentHistory`/`Loading`/`Error`, tiga selector baru — pola identik `getBillingInvoices` yang
sudah ada di file yang sama.

### 3.2 `billing-invoice-constants.js`

- `DEFAULT_PAYMENT_HISTORY_FILTERS`, route `BILLING_INVOICE_ROUTES.paymentHistory`.
- `PAYMENT_HISTORY_STATUS_BADGE_CONFIG` (Lunas/Cicilan), `PAYMENT_HISTORY_CLAIM_BADGE_CONFIG`
  (Reimburse).
- `derivePaymentInstallmentSummary(tenders, totalBillAmount)` — helper murni, SATU sumber
  kebenaran untuk label "Angsuran N/M" dan status Lunas/Cicilan PER Kwitansi, dipakai halaman ini
  DAN (rencana lanjutan, belum dikerjakan — lihat § 5) `kwitansi-document.jsx`. Aturan dikonfirmasi
  dari dua contoh Kwitansi nyata milik pengguna: M dihitung ulang tiap dilihat (retroaktif, bukan
  dipatok saat tender terjadi); status per tender dari akumulasi pembayaran SAMPAI DENGAN tender
  itu (bukan status invoice saat ini — Kwitansi pertama pada invoice 2x cicilan tetap "CICILAN"
  walau invoice itu sudah lunas sekarang); TIDAK ADA label "Angsuran" sama sekali bila invoice
  hanya punya SATU tender sepanjang riwayatnya.

### 3.3 `use-payment-history.js` (baru)

Hook list+filter+pagination, pola identik `use-billing-invoices.js`. `buildKwitansiLink(item,
tenderId)` meregistrasi route token PER INVOICE (lazy, sekali per invoice) lalu membangun URL
Dokumen Kasir dengan `tenderId` Kwitansi yang dipilih — REUSE penuh `BILLING_INVOICE_ROUTES.
dokumenKasir`/`registerPrivateRouteToken` yang sudah ada.

### 3.4 `payment-history-view.jsx` (baru)

Halaman list: `Hero` + `DataFilter` (search, dua `FilterDatePicker` untuk rentang tanggal
kunjungan, `FilterSelect` jenis layanan + jumlah baris) + `DataTable`. Kolom Aksi memakai
`Dropdown` (`react-bootstrap`, sudah dipakai di `TableModern/actions/actions-buttons.jsx` — bukan
library baru) menampilkan satu `Dropdown.Item` per Kwitansi invoice itu.

### 3.5 Routing + menu

`src/app/.../billing/invoices/payment-history/{page.jsx,payment-history-client.jsx}` (pola
page/client identik `billing-invoices-view.jsx`'s routing). `menu-items.jsx`: entri sidebar baru
"Riwayat Pembayaran" (ikon `RiHistoryLine`, baru diimpor) tepat di bawah "Running Invoice".

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Filter tanggal kunjungan (dua field) | `REUSE` | `FilterDatePicker` (base-features) sudah ada, dipakai apa adanya |
| Filter search/jenis layanan/jumlah baris | `REUSE` | `DataFilter`/`FilterSelect` + opsi `BILLING_INVOICE_SERVICE_TYPE_OPTIONS`/`BILLING_INVOICE_PAGE_SIZE_OPTIONS` yang SUDAH ADA (dipakai ulang dari Running Invoice, tidak ada opsi baru) |
| Tabel + paginasi | `REUSE` | `DataTable` (base-features), pola identik `billing-invoices-view.jsx` |
| Badge Lunas/Cicilan/Reimburse | `REUSE` | `StatusBadge`, token className yang SUDAH ADA (`region-status-active`/`region-status-pending`/`region-status-warning`) - tidak ada nilai visual literal baru |
| Menu aksi per baris (Kwitansi majemuk) | `REUSE` | `Dropdown` (`react-bootstrap`) — library yang SUDAH menjadi dependency dan SUDAH dipakai (`TableModern/actions/actions-buttons.jsx`, ditemukan sebagai `onViewKwitansi` yang belum pernah diwiring ke view manapun) — bukan komponen baru, bukan library baru |

**`UI GATE`**: tidak ada — murni REUSE, tidak ada base component baru/extend.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` atas seluruh berkas task ini | Berhasil tanpa error | `PASS` |
| `eslint . --quiet` (repo penuh) | Exit 0, tanpa output — nol error pada seluruh repository | `PASS` |
| `npm run test:unit` | **TIDAK dijalankan ulang** pada giliran ini | `SKIPPED` (instruksi pengguna) |
| `npm run build` | **TIDAK dijalankan** | Pengguna eksplisit meminta menjalankannya sendiri (dua kali pada giliran ini) |
| Verifikasi statis: route `payment-history` tidak collision dengan route dinamis lain | `Glob src/app/.../billing/invoices/*/page.jsx` — nol hasil, tidak ada folder `[slug]` di level yang sama | `PASS` |
| Verifikasi manual (browser, ter-autentikasi) | **NOT DONE** | Tidak dijalankan pada task ini |

---

## 5. Scope yang SENGAJA belum dikerjakan — perlu keputusan pengguna

Referensi Kwitansi yang ditunjukkan pengguna (dua contoh: cicilan dan lunas langsung) menampilkan
LAYOUT YANG BERBEDA JAUH dari `kwitansi-document.jsx` yang berjalan saat ini — bukan hanya field
"Angsuran" yang hilang, tapi seluruh struktur: "Total Tagihan", "Rincian Pembayaran" (daftar
bernomor, tampak menyertakan baris seperti "1. Deposit: Rp X" atau "1. Tunai: Rp X" — mengindikasikan
breakdown metode/alokasi pembayaran, bukan cuma satu nominal), "Total Dibayar", "Sisa Pembayaran"
(muncul kondisional saat belum lunas), badge status LUNAS/CICILAN (bukan badge status TENDER
DITERIMA/PENDING/GAGAL seperti sekarang), field "Untuk pembayaran" yang menampilkan detail
layanan+nama dokter (bukan cuma `serviceType` generik), field "Atas nama" TERPISAH dari "Sudah
terima dari", dan nama petugas kasir tertulis eksplisit di bawah tanda tangan.

**Ini mengubah dokumen yang SUDAH DIPAKAI PRODUKSI untuk SETIAP pembayaran di seluruh sistem** —
bukan penambahan kecil. Sebagian data yang dibutuhkan belum jelas sumbernya di source saat ini
(mis. nama petugas kasir per tender — apakah ada kolom `CreateBy`/aktor tersimpan di `BilTender`
yang bisa dipetakan ke nama user? "Untuk pembayaran" menampilkan dokter yang mana - dokter
penanggung jawab encounter?). Helper `derivePaymentInstallmentSummary` (§ 3.2) SUDAH disiapkan
sebagai fondasi bagian "Angsuran N/M"/status Lunas-Cicilan yang PALING jelas dan sudah dikonfirmasi
lewat dua contoh Kwitansi pengguna — tapi redesain LAYOUT PENUH belum dikerjakan, menunggu
konfirmasi pengguna atas cakupannya (lihat pesan chat untuk pertanyaan lengkap).

---

## 6. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Menunggu backend (`BE-BKC-FIX-008`) di-rebuild dan frontend dev server restart |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Status Git | Baru: `use-payment-history.js`, `payment-history-view.jsx`, `payment-history/{page.jsx,payment-history-client.jsx}`. Modified: `billing-invoice-slice.jsx`, `billing-invoice-constants.js`, `menu-items.jsx`. Belum staged/commit |
| Langkah berikutnya | Rebuild backend, restart frontend dev server, verifikasi halaman baru; putuskan cakupan redesain `kwitansi-document.jsx` (§ 5) sebelum dikerjakan |

