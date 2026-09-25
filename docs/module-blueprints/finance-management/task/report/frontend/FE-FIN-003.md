# Laporan Perubahan Frontend — `FE-FIN-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-003` |
| Judul | Setoran bank dan kas harian |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-05`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task) dan bagian 8 (urutan) |
| Trace | `FR-FIN-060`..`065`; `FIN-DEC-016`, `FIN-DEC-017`, `FIN-DEC-024`, `FIN-DEC-025`, `UAT-13`..`15`; `FIN-API-1.0`, `FIN-PERM-1.0` |
| Contract version | `FIN-API-1.0` (terkunci), `FIN-PERM-1.0` (terkunci) — dipatuhi apa adanya |
| Wewenang UI | UI brief closed 23 September 2026 (`FIN-DEC-024`..`025`, `00-interview-decisions.md`) — rute `/finance/cash-management`, menu flat "Setoran & Kas Harian" di `corporateFinance`, ringkasan metrik posisi kas hari berjalan, modal pembuatan draf setoran, modal rincian setoran (posting, verifikasi, pembatalan), modal breakdown kas harian, dan modal penutupan kas |
| Dependency | `BE-FIN-006` — ✅ selesai 23 September 2026 (`dotnet build` PASS, migration diterapkan, endpoint diuji langsung — [laporan](../backend/BE-FIN-006.md)) |
| Klasifikasi | `HEAVY` — fitur setoran bank dan kas harian terintegrasi penuh (list setoran bank dengan filter & pagination, modal pembuatan setoran dengan pengecekan saldo kas kasir otomatis dari backend, modal detail setoran dengan kontrol alur posting/verifikasi/pembatalan dan konkurensi row version, riwayat kas harian dengan filter & pagination, modal rincian breakdown shift kasir & setoran, modal penutupan kas dengan validasi draft blocker, registrasi slice & store, dan registrasi rute & menu sidebar) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/constants/finance/cash-management/`, `src/lib/state/slice/finance/cash-management/`, `src/utils/finance/cash-management/`, `src/lib/hooks/finance/cash-management/`, `src/components/view/finance/cash-management/`, `src/app/finance/cash-management/`, `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx` |
| Commit frontend saat dikerjakan | Working tree pada branch `yasmina` |
| Commit backend yang dijadikan rujukan | Working tree pada branch `Yasmina`, `NewQuilvianSystemBackend` — `BE-FIN-006` selesai |
| Tanggal | 23 September 2026 |
| Status | ✅ **SELESAI — seluruh komponen, hook, slice, modal, rute, dan menu telah diimplementasikan penuh. `npm run lint:errors` PASS (0 error) dan `npm run build` PASS (0 error).** |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini dijalankan:
- Menu `corporateFinance` di sidebar telah memiliki submenu Master Data, Anggaran Petty Cash, dan Piutang (`FE-FIN-002`), namun belum memiliki halaman operasional kas dan setoran bank.
- Endpoint backend untuk setoran bank (`FinanceBankDepositsController`) dan kas harian (`FinanceDailyCashController`) telah diselesaikan pada task `BE-FIN-006` dan siap dikonsumsi:
  - 7 endpoint Bank Deposits: daftar berhalaman, rincian per ID, pengecekan saldo kas kasir tersedia (`GET /available-balance`), pembuatan draf setoran, posting setoran, verifikasi bank, dan pembatalan setoran.
  - 4 endpoint Daily Cash: posisi kas hari ini (`GET /current`), riwayat kas harian berhalaman, rincian breakdown kas harian (`GET /{cashDate}/breakdown` memuat perbandingan snapshot kasir vs setoran bank), dan penutupan kas harian (`POST /{cashDate}/close`).
- Keputusan bisnis dan UI yang mengikat:
  - `FIN-DEC-016`: Draf setoran dibuat manual oleh staf finance dengan memilih rekening bank tujuan dan nominal.
  - `FIN-DEC-017`: **Nol perhitungan klien**. Saldo kas kasir tersedia dihitung oleh backend (`GET /available-balance`). Klien dilarang mengurangi atau menghitung sendiri sisa kas kasir.
  - `UAT-13` / `FIN-VAL-060`: Setoran melebihi kas kasir ditolak dengan kode `422 Unprocessable Entity`.
  - `UAT-14`: Penanganan konkurensi optimistik (`409 Conflict`) pada aksi posting, verifikasi, pembatalan, dan penutupan kas dengan menyertakan `expectedRowVersion`.
  - `UAT-15` / `FR-FIN-065`: Penutupan kas harian membekukan angka (`CLOSED`).
  - `FIN-VAL-064`: Backend menolak penutupan kas (`422`) bila masih terdapat transaksi setoran bank berstatus `DRAFT` pada tanggal tersebut.
  - `FIN-DEC-024` & `FIN-DEC-025`: Rute berbentuk `/finance/cash-management` dan entri menu "Setoran & Kas Harian" flat di dalam grup `corporateFinance`.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Pemantauan Posisi Kas Hari Berjalan
1. Pengguna membuka menu **Keuangan → Setoran & Kas Harian** (`/finance/cash-management`).
2. Di bagian atas layar, pengguna disambut oleh ringkasan metrik posisi kas hari ini (`SummaryCards`):
   - **Saldo Awal Kas** (posisi pembukaan kas hari ini).
   - **Kas Masuk Kasir** (akumulasi kas riil dari seluruh shift kasir hari ini yang dihitung backend).
   - **Setoran ke Bank** (total kasir yang telah disetor ke rekening bank).
   - **Pengeluaran Kas** (pengeluaran operasional hari ini).
   - **Saldo Kas Berjalan** beserta indikator status `Berjalan (Buka)` atau `Ditutup (Beku)`.

### 2.2 Pembuatan Draf Setoran Bank & Pengecekan Saldo Kas
1. Pengguna menekan tombol **+ Buat Draf Setoran Bank** di hero banner.
2. Modal pembuatan setoran terbuka:
   - Pengguna memilih tanggal setoran (default hari ini) dan shift kasir (opsional).
   - Sistem secara otomatis memanggil endpoint backend `GET /available-balance` dan menampilkan kartu ringkasan:
     - Penerimaan Kasir
     - Total Setoran Terdaftar
     - **Saldo Tersedia Disetor** (hasil agregasi backend).
   - Pengguna memilih rekening bank tujuan dari daftar rekening master data.
   - Pengguna menginput nominal setoran. Jika nominal melebihi saldo kas yang tersedia, formulir menampilkan peringatan visual bahwa backend akan menolak setoran yang melebihi saldo kas (sesuai `UAT-13`).
   - Pengguna dapat mengisi nomor bukti slip bank dan catatan.
3. Setelah formulir disimpan, draf tersimpan dan daftar setoran diperbarui secara otomatis.

### 2.3 Siklus Hidup Setoran Bank (Posting, Verifikasi, Pembatalan)
1. Pada tab **Daftar Setoran Bank**, pengguna melihat seluruh transaksi setoran bank dengan filter status, rekening bank, dan rentang tanggal.
2. Menekan tombol **Detail →** pada baris setoran membuka modal rincian setoran lengkap beserta jejak audit pembuat dan waktu transaksi:
   - **Posting Setoran (dari `DRAFT`)**: Staf finance menekan tombol *Posting Setoran*, mengonfirmasi nomor slip setor bank. Sistem mengirim `expectedRowVersion` ke backend. Jika saldo kas mencukupi, status berubah menjadi `POSTED`.
   - **Verifikasi Bank (dari `POSTED`)**: Setelah dana diverifikasi masuk ke rekening koran, staf menekan *Verifikasi Bank*. Sistem mengirim `expectedRowVersion` dan memperbarui status menjadi `VERIFIED`.
   - **Pembatalan Setoran**: Pada status `DRAFT` atau `POSTED`, staf dapat membatalkan setoran dengan memasukkan alasan pembatalan wajib. Saldo kas kasir akan kembali tersedia untuk disetor.
   - **Penanganan Konflik 409**: Jika data telah diubah oleh pengguna lain, sistem menampilkan pesan galat konkurensi dan segera memuat ulang data versi terkini dari server.

### 2.4 Riwayat Kas Harian, Rincian Breakdown, & Penutupan Kas
1. Pengguna beralih ke tab **Riwayat & Posisi Kas Harian**.
2. Pengguna melihat daftar posisi kas harian historis beserta statusnya (`OPEN` / `CLOSED`).
3. Menekan tombol **Rincian** membuka modal komprehensif yang menampilkan:
   - Snapshot ringkasan angka kas tanggal tersebut.
   - Rincian seluruh shift kasir yang bertugas pada tanggal tersebut (ID shift, nama kasir, waktu buka/tutup, penerimaan kas, non-kas, status).
   - Rincian transaksi setoran bank yang tertaut pada tanggal tersebut.
4. **Penutupan Kas Harian (`POST /{cashDate}/close`)**:
   - Untuk tanggal dengan status `OPEN`, staf berwenang dapat menekan tombol **Tutup Kas**.
   - Modal penutupan menampilkan peringatan kebijakan bahwa penutupan akan membekukan posisi kas (`UAT-15`) dan backend akan menolak jika masih ada draf setoran yang belum selesai (`FIN-VAL-064`).
   - Staf dapat mengonfirmasi saldo awal, penerimaan lain, pengeluaran kas, serta catatan berita acara penutupan kas.
   - Setelah penutupan berhasil, status berubah menjadi `CLOSED` dan angka kas dibekukan secara permanen.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat dan disunting

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/cash-management/cash-management-constants.jsx` | **Baru.** Konfigurasi `CASH_MANAGEMENT_CONFIG`, permission registry `FIN-PERM-1.0`, konstanta status setoran bank (`DRAFT`, `POSTED`, `VERIFIED`, `CANCELLED`), badge status setoran, konstanta status kas harian (`OPEN`, `CLOSED`), badge status kas, dan opsi paginasi. |
| `src/utils/finance/cash-management/cash-management-utils.jsx` | **Baru.** Fungsi utilitas pemformat mata uang Rupiah (`formatMoney`), format tanggal (`formatDate`), format tanggal & waktu (`formatDateTime`), sanitasi teks, serta builder baris detail setoran bank dan kas harian. |
| `src/lib/state/slice/finance/cash-management/finance-cash-management-slice.jsx` | **Baru.** Redux slice lengkap dengan 11 async thunk API backend (`fetchBankDeposits`, `fetchBankDepositDetail`, `fetchAvailableCashBalance`, `createBankDeposit`, `postBankDeposit`, `verifyBankDeposit`, `cancelBankDeposit`, `fetchCurrentDailyCash`, `fetchDailyCashHistory`, `fetchDailyCashBreakdown`, `closeDailyCash`), penanganan status kode 409 & 422, filter, dan sistem notifikasi toast. |
| `src/lib/state/store.jsx` | **Disunting.** Pendaftaran `financeCashManagement: financeCashManagementReducer` ke Redux store aplikasi. |
| `src/lib/hooks/finance/cash-management/use-finance-bank-deposits.jsx` | **Baru.** Hook controller setoran bank: sinkronisasi daftar dan paginasi, pemuatan opsi rekening bank, kontrol modal buat draf, modal detail setoran, pengecekan saldo kas kasir otomatis, eksekusi posting, verifikasi, dan pembatalan dengan `expectedRowVersion`. |
| `src/lib/hooks/finance/cash-management/use-finance-daily-cash.jsx` | **Baru.** Hook controller kas harian: pemantauan posisi kas hari ini, riwayat kas harian, kontrol modal rincian breakdown per shift/setoran, dan eksekusi penutupan kas harian. |
| `src/components/view/finance/cash-management/bank-deposits/bank-deposits-table-columns.jsx` | **Baru.** Definisi kolom tabel setoran bank dengan nomor baris, nomor setoran, tanggal, rekening bank tujuan, nominal, slip setor, badge status, dan tombol aksi detail. |
| `src/components/view/finance/cash-management/daily-cash/daily-cash-table-columns.jsx` | **Baru.** Definisi kolom tabel kas harian dengan tanggal kas, saldo awal, penerimaan kasir, setoran bank, pengeluaran, saldo akhir, badge status (OPEN/CLOSED), dan tombol aksi rincian serta tutup kas. |
| `src/components/view/finance/cash-management/modals/create-bank-deposit-modal.jsx` | **Baru.** Modal formulir draf setoran bank yang menampilkan kartu ringkasan saldo kas kasir tersedia dari backend, validasi input nominal, peringatan saldo, dan penanganan galat backend. |
| `src/components/view/finance/cash-management/modals/bank-deposit-detail-modal.jsx` | **Baru.** Modal rincian setoran bank lengkap dengan status, nominal, rekening bank, audit trail, serta dialog aksi konfirmasi untuk posting, verifikasi, dan pembatalan dengan kontrol konkurensi. |
| `src/components/view/finance/cash-management/modals/daily-cash-breakdown-modal.jsx` | **Baru.** Modal rincian kas harian yang menyajikan snapshot posisi kas, tabel rincian shift kasir (penerimaan tunai & non-tunai per shift), dan tabel rincian setoran bank terkait. |
| `src/components/view/finance/cash-management/modals/close-daily-cash-modal.jsx` | **Baru.** Modal penutupan kas harian dengan peringatan pembekuan angka, verifikasi draft blocker (`FIN-VAL-064`), formulir penyesuaian kas, dan eksekusi tutup kas dengan `expectedRowVersion`. |
| `src/components/view/finance/cash-management/finance-cash-management-view.jsx` | **Baru.** View utama yang menyatukan `Hero`, `SummaryCards` posisi kas hari berjalan, navigasi tab interaktif antara Setoran Bank dan Riwayat Kas Harian, `DataFilter`, `DataTable`, `RegionPagination`, integrasi 4 modal, dan `ToastStack`. |
| `src/app/finance/cash-management/page.jsx` | **Baru.** Server component rute `/finance/cash-management` dengan metadata halaman SEO ramah rumah sakit. |
| `src/app/finance/cash-management/cash-management-client.jsx` | **Baru.** Client component wrapper untuk Next.js App Router. |
| `src/utils/menu-sidebar/menu-items.jsx` | **Disunting.** Pendaftaran butir menu flat "Setoran & Kas Harian" di grup `corporateFinance` dengan ikon `RiWallet3Line` dan rute `/finance/cash-management`. |

---

## 4. Verifikasi dan Bukti Kepatuhan

### 4.1 Validasi Otomatis (Linting & Kompilasi Build)
- `npm run lint:errors`: **PASS (0 error)**. Tidak ditemukan kesalahan linting pada seluruh berkas yang dibuat atau disunting.
- `npm run build`: **PASS (0 error)**. Halaman `/finance/cash-management` terkompilasi bersih tanpa kegagalan pada Next.js App Router.

### 4.2 Matriks Kepatuhan Acceptance Criteria

| Kriteria / UAT | Status | Bukti Implementasi |
| --- | :---: | --- |
| **Nol Perhitungan Klien (`FIN-DEC-017`)** | ✅ PATUH | Saldo kas kasir tersedia diambil langsung dari endpoint `GET /bank-deposits/available-balance`; klien murni menampilkan data dari server. |
| **Setoran Melebihi Kas Ditolak (`UAT-13`, `FIN-VAL-060`)** | ✅ PATUH | Form pembuatan setoran menampilkan peringatan jika nominal melebihi saldo tersedia; jika backend menolak `422`, pesan kesalahan ditampilkan jelas di modal. |
| **Penanganan Konkurensi Optimistik (`UAT-14`)** | ✅ PATUH | Aksi posting, verifikasi, pembatalan, dan penutupan kas menyertakan `ExpectedRowVersion`; penolakan `409 Conflict` otomatis memicu reload data terbaru. |
| **Penutupan Membekukan Angka (`FR-FIN-065`, `UAT-15`)** | ✅ PATUH | Posisi kas berstatus `CLOSED` ditampilkan beku (read-only/frozen); tombol tutup kas dinonaktifkan untuk tanggal yang sudah ditutup. |
| **Draft Blocker pada Penutupan Kas (`FIN-VAL-064`)** | ✅ PATUH | Modal penutupan kas memuat alert peringatan bahwa setoran berstatus `DRAFT` akan menghalangi penutupan; pesan galat 422 dari backend disajikan langsung. |
| **Rute & Menu Flat (`FIN-DEC-024`, `FIN-DEC-025`)** | ✅ PATUH | Rute berada di `/finance/cash-management` dan menu sidebar terdaftar di grup `corporateFinance` sebagai "Setoran & Kas Harian". |

---

## 5. Kesimpulan

Task **`FE-FIN-003`** telah diselesaikan 100% secara akurat dan mematuhi seluruh konstitusi rekayasa Quilvian. Seluruh kontrak API `FIN-API-1.0` telah terhubung, kontrol konkurensi dan validasi bisnis ditegakkan, build Next.js lulus tanpa kesalahan, dan sistem siap untuk melanjutkan ke task berikutnya.
