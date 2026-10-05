# Laporan Perubahan Frontend: Task FE-FIN-015

## 1. Ringkasan Eksekutif
- **Task ID**: `FE-FIN-015`
- **Nama Modul**: `Finance Management`
- **Nama Task**: Layar/Tab Pemantauan & Pemicu Snapshot Saldo Subledger Bulanan (4 Akun Kontrol)
- **Tanggal Selesai**: 30 September 2026
- **Status Akhir**: ✅ **SELESAI (DONE)**
- **Wewenang Keputusan Bisnis**: Product Owner (`FIN-DEC-090`, `ACC-DEC-108`, `ACC-DEC-109`, `ACC-DEC-110`)
- **Kontrak Terkunci**: `contracts/integration-contract.md` §5.6, `03-frontend-architecture.md` Bagian 16, `FIN-API-1.0` (Accounting Events)
- **Prasyarat Backend**: `BE-FIN-048` ✅ (Validasi Outbox Saldo Subledger) & `BE-FIN-049` ✅ (Layanan Kalkulasi Agregat 4 Akun Kontrol)
- **Repositori Target**: `QuilvianSystemFrontendDev`
- **Branch**: `yasmina`

---

## 2. Latar Belakang & Kebutuhan Bisnis

Dalam tata kelola akuntansi dan keuangan rumah sakit, setiap akhir bulan (tutup buku bulanan / *monthly closing*), bagian Akuntansi (*Accounting*) memerlukan kepastian bahwa angka saldo pada Buku Besar (*General Ledger*) berada dalam posisi seimbang dan cocok dengan buku pembantu (*subledger*) di modul Keuangan (*Finance*). 

Sesuai piagam rekonsiliasi akuntansi (`ACC-DEC-108`, `FIN-DEC-090`), terdapat **4 Akun Kontrol Kunci** yang wajib dilaporkan posisi saldo akhirnya setiap bulan melalui pesan outbox `SALDO-SUBLEDGER`:
1. **Kas Kasir (`1-1002`)**: Saldo fisik kasir aktif penerimaan operasional rumah sakit (penutupan kas harian kasir rawat jalan, rawat inap, dan IGD).
2. **Kas Kecil / Petty Cash (`1-1003`)**: Dana tetap kas kecil untuk kebutuhan belanja darurat atau operasional unit kerja rumah sakit.
3. **Piutang Usaha / Pasien & Penjamin (`1-2001`)**: Total akumulasi piutang pelayanan pasien umum dan klaim penjamin asuransi/BPJS yang masih berjalan (*outstanding* / *partial*).
4. **Utang Usaha / Supplier (`2-1001`)**: Total kewajiban utang tagihan faktur pembelian yang belum lunas kepada vendor farmasi, alat kesehatan, dan rekanan logistik rumah sakit.

Sebelum task `FE-FIN-015`, backend telah menyediakan fondasi kalkulasi otomatis (`BE-FIN-049`), namun petugas Finance dan staf Akuntansi belum memiliki antarmuka visual untuk:
- Memeriksa kelengkapan status snapshot 4 akun kontrol per periode akuntansi (`YYYY-MM`).
- Melihat nominal saldo berjalan masing-masing akun kontrol beserta nomor kejadian outbox dan status pengiriman ke modul Accounting.
- Memicu kalkulasi ulang atau penerbitan snapshot saldo subledger akhir bulan secara terotorisasi dengan modal konfirmasi dan catatan operasional.

---

## 3. Skenario Operasional Rumah Sakit & Alur Proses Bisnis

### Contoh Skenario Nyata:
Pada tanggal 30 September 2026 pukul 23:59 WIB, Rumah Sakit bersiap menutup periode akuntansi September 2026 (`2026-09`). Staf Finance membuka modul Pemantauan Keuangan pada tab *"Saldo Subledger Bulanan (4 Akun Kontrol)"*:
1. **Pemeriksaan Awal**: Sistem menampilkan status *"Belum Lengkap"* atau *"Lengkap (4 Akun)"* tergantung apakah snapshot untuk periode `2026-09` telah diterbitkan.
2. **Pemicu Kalkulasi**: Staf Finance yang memiliki wewenang `FinanceAccountingEvent:Create` menekan tombol *"Kalkulasi & Terbitkan Saldo Subledger"*.
3. **Konfirmasi & Catatan**: Muncul modal dialog konfirmasi. Staf memasukkan catatan operasional: *"Penutupan buku subledger September 2026 menjelang audit bulanan"*.
4. **Eksekusi Atomik di Backend**: Backend menghitung agregat saldo non-negatif dari database operasional:
   - Kas Kasir: Mengambil saldo penutupan kas harian terakhir yang sudah berstatus `CLOSED`.
   - Kas Kecil: Menghitung total saldo anggaran kas kecil yang aktif.
   - Piutang: Menjumlahkan saldo sisa tagihan pasien dan klaim penjamin yang diakui hingga akhir periode.
   - Utang Supplier: Menjumlahkan sisa kewajiban faktur pembelian yang belum lunas.
5. **Pementasan Outbox**: Backend menerbitkan 4 kejadian berjenis `SALDO-SUBLEDGER` ke antrean outbox akuntansi.
6. **Umpan Balik Visual**: Antarmuka memperbarui tabel 4 akun kontrol secara seketika dengan status kelengkapan hijau *"Lengkap (4 Akun)"*, total nominal agregat, dan nomor event outbox masing-masing akun.

```
[Staf Finance / Akuntansi]
           │
           ▼
[Pilih Periode: 2026-09] ───► Query Snapshot: GET .../subledger-balances/2026-09
           │                                 │
           │                                 ├──► Belum Ada / Tidak Lengkap (Warning)
           │                                 └──► Lengkap 4 Akun Kontrol (Positive)
           ▼
[Tombol: "Kalkulasi & Terbitkan"] ───► Cek Hak Akses (FinanceAccountingEvent:Create)
           │                                 │
           │                                 ├──► Tidak ada izin: Tombol Disabled
           │                                 └──► Ada izin: Buka Modal Konfirmasi
           ▼
[Modal Konfirmasi & Catatan] ───► POST .../subledger-balances/generate
           │
           ▼
[Database Finance Backend]
  ├── Hitung Kas Kasir (Closing Saldo Harian)
  ├── Hitung Kas Kecil (Saldo Anggaran Aktif)
  ├── Hitung Piutang Pasien & BPJS (Outstanding)
  └── Hitung Utang Supplier Farmasi (Unpaid Invoice)
           │
           ▼
[Pementasan 4 Event SALDO-SUBLEDGER ke Outbox]
           │
           ▼
[Tabel & Kartu Ringkasan Terbarui Otomatis]
```

---

## 4. Komponen dan Berkas yang Dibuat / Dimodifikasi

| No | Tipe | Berkas | Deskripsi |
|---|---|---|---|
| 1 | **Constants** | `src/lib/constants/finance/monitoring/monitoring-constants.jsx` | Menambahkan konstanta metadata 4 akun kontrol (`SUBLEDGER_CONTROL_ACCOUNTS`), kode akun baku (`1-1002`, `1-1003`, `1-2001`, `2-1001`), kategori, label, dan palet warna badge. |
| 2 | **Utils** | `src/utils/finance/monitoring/monitoring-utils.jsx` | Menambahkan fungsi helper periode akuntansi: `getDefaultAccountingPeriodCode()`, `formatAccountingPeriodLabel()`, generator pilihan periode `getRecentAccountingPeriodOptions()`, dan pemetaan metadata kategori `getSubledgerAccountCategoryMeta()`. |
| 3 | **Redux Slice** | `src/lib/state/slice/finance/monitoring/finance-monitoring-slice.jsx` | Menambahkan async thunk `fetchSubledgerSnapshotsByPeriod` dan `generateSubledgerSnapshots`, state slice `subledgerBalances`, reducer `setSubledgerSelectedPeriod`, serta penanganan extraReducers dan notifikasi toast. |
| 4 | **Custom Hook** | `src/lib/hooks/finance/monitoring/use-finance-subledger-balances.jsx` | Hook terenkapsulasi untuk memuat snapshot periode aktif secara otomatis, proteksi wewenang RBAC `FinanceAccountingEvent:Create`, manajemen modal, dan penanganan aksi generate snapshot. |
| 5 | **Table Columns** | `src/components/view/finance/monitoring/subledger-balances/subledger-balances-table-columns.jsx` | Definisi kolom tabel 4 akun kontrol: Kategori & Nama Akun, Kode Akun Monospace, Nominal Saldo (terformat mata uang IDR), Nomor Event Outbox, Versi Sumber, Tanggal Akuntansi, Status Kirim Outbox, dan Transaksi Sumber. |
| 6 | **Modal Dialog** | `src/components/view/finance/monitoring/subledger-balances/generate-subledger-snapshots-modal.jsx` | Modal konfirmasi penerbitan snapshot dengan rincian periode target, catatan operasional (maksimal 500 karakter), opsi penyesuaian kode akun lanjutan (*advanced override*), dan penanganan status loading. |
| 7 | **View Utama** | `src/components/view/finance/monitoring/finance-monitoring-view.jsx` | Menambahkan Tab 3 *"Saldo Subledger Bulanan (4 Akun Kontrol)"*, tombol pemicu pada hero header (terproteksi hak akses), 4 kartu metrik ringkasan (Kelengkapan, Total Saldo, Tanggal Akhir, Jumlah Akun), penyaring periode, tabel data, dan integrasi modal. |

---

## 5. Gerbang Keputusan Base Component (`UI Gate`)

Sesuai aturan rekayasa frontend Quilvian, seluruh elemen visual memanfaatkan base component baku tanpa pembuatan styling ad-hoc:

| Komponen / Elemen UI | Sumber Base Component | Status | Rekomendasi & Catatan |
|---|---|---|---|
| Tab Navigasi & Hero Action | `src/components/features/base-features/hero.jsx` | `REUSE` | Tombol *"Kalkulasi & Terbitkan Saldo Subledger"* dipasang pada header Hero dengan proteksi RBAC. |
| Kartu Ringkasan Metrik | `src/components/features/base-features/summary-grid.jsx` | `REUSE` | 4 kartu metrik ringkasan (Kelengkapan, Total Agregat, Tanggal Periode, Jumlah Akun) dengan tone visual harmonis. |
| Filter & Pemilih Periode | `src/components/features/base-features/data-filter.jsx` & `filter-select.jsx` | `REUSE` | Filter bar terintegrasi dengan pilihan dropdown 12 bulan terakhir dan pemilih tanggal bulan bebas. |
| Tabel 4 Akun Kontrol | `src/components/features/base-features/data-table.jsx` | `REUSE` | Tabel data berstruktur rapi dengan status loading dan empty state informatif. |
| Modal Konfirmasi Penerbitan | Modal Dialog Accessible | `REUSE` | Mengikuti pola dialog interaktif terstandarisasi dengan validasi input dan penanganan state asinkron. |
| Toast Feedback | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Menampilkan umpan balik pesan sukses atau galat dari response backend secara elegan. |

---

## 6. Spesifikasi Endpoint API (Gaya Swagger)

### Tag: `[Tags("Corporate / Finance Management / Accounting Events")]`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Parameter | Format Response |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/finance-management/accounting-events/subledger-balances/{accountingPeriodCode}` | Mengambil rincian snapshot saldo subledger 4 akun kontrol untuk periode akuntansi tertentu (`YYYY-MM`) | `FinanceAccountingEvent : Read` | Route: `accountingPeriodCode` (string, contoh: `"2026-09"`) | `ApiResponse<SubledgerPeriodSnapshotsResponse>` |
| `POST` | `/api/v1/corporate/finance-management/accounting-events/subledger-balances/generate` | Memicu kalkulasi dan penerbitan 4 event `SALDO-SUBLEDGER` ke outbox akuntansi secara atomik | `FinanceAccountingEvent : Create` | Body: `GenerateSubledgerSnapshotsRequest` (accountingPeriodCode, notes, optional code overrides) | `ApiResponse<GenerateSubledgerSnapshotsResponse>` |

---

## 7. Bukti Validasi

### 7.1 Validasi Linter (`ESLint`)
- **Perintah**: `npm run lint:errors` & `npx eslint src/components/view/finance/monitoring src/lib/constants/finance/monitoring src/utils/finance/monitoring src/lib/hooks/finance/monitoring src/lib/state/slice/finance/monitoring`
- **Direktori**: `QuilvianSystemFrontendDev`
- **Hasil**: **PASS (0 errors, 0 warnings)**

### 7.2 Validasi Production Build Next.js
- **Perintah**: `npm run build`
- **Direktori**: `QuilvianSystemFrontendDev`
- **Hasil**: **PASS (Exit code 0)**
  - Mengompilasi 421 halaman statis Next.js dengan Turbopack secara sukses.
  - Skrip `scripts/prepare-standalone.mjs` berhasil mengekspor aset statis dan publik untuk runtime mandiri (*standalone*).

### 7.3 Verifikasi Kepatuhan Kriteria Penerimaan (*Acceptance Criteria*)
1. **Pemilih Periode Akuntansi**:
   - Menyediakan dropdown pilihan periode 12 bulan terakhir (`YYYY-MM`) dan input pemilih bulan langsung.
   - Mengambil data snapshot secara otomatis setiap kali periode diubah melalui endpoint `GET .../subledger-balances/{accountingPeriodCode}`.
2. **Status Kelengkapan 4 Akun Kontrol**:
   - Menampilkan kartu ringkasan status kelengkapan (`IsComplete`):
     - Hijau (*Positive*) *"Lengkap (4 Akun)"* bila seluruh 4 akun kontrol memiliki snapshot.
     - Kuning/Oranye (*Warning*) *"Belum Diterbitkan"* bila snapshot belum lengkap atau belum dibuat.
   - Menampilkan akumulasi total saldo normal non-negatif dan tanggal akhir periode akuntansi.
3. **Penyajian Tabel 4 Akun Kontrol**:
   - Tabel menampilkan secara terinci 4 akun kontrol: Kas Kasir (`1-1002`), Kas Kecil (`1-1003`), Piutang Pasien & Penjamin (`1-2001`), dan Utang Supplier (`2-1001`).
   - Setiap baris menyajikan nominal saldo terformat mata uang IDR, nomor event outbox `SALDO-SUBLEDGER`, versi sumber, tanggal akuntansi, status pengiriman outbox, dan ID transaksi asal.
4. **Tombol Pemicu Kalkulasi & Proteksi RBAC**:
   - Tombol *"Kalkulasi & Terbitkan Saldo Subledger"* diproteksi oleh hak akses `FinanceAccountingEvent:Create`.
   - Pengguna tanpa hak akses `Create` melihat tombol dalam keadaan nonaktif (*disabled*) dengan keterangan instruktif.
   - Pengguna dengan hak akses yang sah dapat membuka modal konfirmasi, memasukkan catatan operasional opsional, dan memicu kalkulasi via `POST .../subledger-balances/generate`.
5. **Nol Perhitungan Klien**:
   - Seluruh nominal uang, status kelengkapan, dan data agregat murni dibaca apa adanya dari response backend tanpa perhitungan atau pembulatan di sisi browser klien.

---

## 8. Kesimpulan & Penandaan Roadmap

Task `FE-FIN-015` telah diselesaikan secara tuntas dan terintegrasi harmonis ke dalam layar Pemantauan Keuangan Quilvian (`/finance/monitoring`).

Status task pada registri dan roadmap diperbarui menjadi:
- `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`: **✅ SELESAI (DONE)**
- `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md`: **✅ SELESAI (DONE)**
