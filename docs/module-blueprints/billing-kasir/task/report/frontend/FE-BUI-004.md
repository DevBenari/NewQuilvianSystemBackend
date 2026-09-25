# Laporan Perubahan Frontend — `FE-BUI-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-004` |
| Judul | Pengecualian Finansial (Refund, Adjustment, Write-Off) di Riwayat Pembayaran |
| Slice | Gelombang `MVP-31` — Revisi UI Billing: Perbaikan Tampilan Kasir (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-004`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-004` — Pengecualian Finansial di Riwayat Pembayaran |
| Trace | `BUI-DEC-013` (Pemindahan Tombol Pengajuan Finansial ke Riwayat Pembayaran), `FR-BUI-013`, `BUI-DES-012`, `BIL-API-1.4`, `BIL-PERMISSION-1.2`, `UAT-BUI-08`, `UAT-BUI-13`, `UAT-BUI-14` |
| Contract version | `BIL-API-1.4` (nol endpoint baru — memanfaatkan API Refund, Adjustment, dan Write-Off existing) |
| Wewenang UI | `DEV_DISCRETION` untuk penempatan dropdown menu aksi pada baris tabel Riwayat Pembayaran; seluruh komponen modal dan formulir dipakai ulang apa adanya (`REUSE`) |
| Dependency | `FE-BUI-001`, `FE-BUI-002`, `FE-BUI-003` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 7 (skor 0); berkas diubah 4 (skor 0); logika orkestrasi modal & dropdown presentasional (skor 0); kontrak API tidak berlaku / tidak berubah (skor 0); database `NOT APPLICABLE` (skor 0); keamanan/auth `NOT APPLICABLE` / tidak berubah (skor 0); UI/workflow pemindahan letak trigger modal (skor 0). Total skor 0 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Kasir kini dapat mengajukan Refund, Adjustment, dan Write-Off langsung dari baris tabel invoice pada halaman Riwayat Pembayaran (`payment-history-view.jsx`). Tombol pengajuan pada Menu Pembayaran (`menu-pembayaran-view.jsx`) telah dihapus secara bersih sesuai `UAT-BUI-13`. Kolom "Kwitansi" pada Riwayat Pembayaran dipertahankan sepenuhnya tanpa perubahan perilaku cetak/lihat kwitansi sesuai `UAT-BUI-14`. Seluruh alur modal dan maker-checker persetujuan finansial tetap memakai kontrak API existing (`BIL-API-1.4` dan `BIL-PERMISSION-1.2`) tanpa endpoint baru. Verifikasi kualitas: `npm run lint:errors` lulus (0 error, exit code 0); `npm run test:unit` lulus 1.685/1.693 (8 kegagalan pre-existing konsisten); `npm run build` berhasil (`✓ Compiled successfully`, 406 rute dan standalone runtime siap). |

---

## 1. Keadaan yang ditemukan di awal

Berdasarkan audit arsitektur to-be (`00-interview-decisions.md` `BUI-DEC-013`, `03-frontend-architecture.md` `BUI-DES-012`, dan roadmap frontend `FE-BUI-004`):
1. **Penempatan Aksi Tidak Sesuai Ranah Proses Bisnis Kasir**:
   - Sebelumnya, tombol pengajuan pengecualian finansial (`+ Ajukan Refund`, `+ Ajukan Adjustment`, dan `+ Ajukan Write-off`) dirender di dalam komponen `BillingFinancialExceptionPanel` pada halaman Menu Pembayaran (`menu-pembayaran-view.jsx`).
   - Padahal, halaman Menu Pembayaran adalah area operasional kasir untuk memproses pembayaran berjalan pasien aktif (tender kasir, alokasi deposit, perhitungan diskon). Transaksi pengecualian finansial (pengembalian kelebihan bayar, koreksi penyesuaian nilai tagihan, maupun penghapusan piutang tidak tertagih) pada praktiknya merupakan tindakan pasca-pembayaran atau korektif atas tagihan yang sudah memiliki riwayat pembayaran.
2. **Ketiadaan Akses Aksi Finansial di Riwayat Pembayaran**:
   - Halaman Riwayat Pembayaran (`payment-history-view.jsx`) menyajikan daftar seluruh invoice yang sudah memiliki transaksi pembayaran.
   - Sebelumnya, halaman ini hanya menyediakan kolom *"Kwitansi"* (tombol tunggal "Lihat Kwitansi" atau dropdown pilihan angsuran cicilan), tanpa adanya menu aksi untuk memicu pengajuan koreksi finansial bagi tagihan yang bersangkutan.
3. **Kebutuhan Isolasi Tombol vs Panel Pelacakan**:
   - Komponen `BillingFinancialExceptionPanel` sebelumnya merender tombol `+ Ajukan Refund`, `+ Ajukan Adjustment`, dan `+ Ajukan Write-off` secara hardcoded tanpa memeriksa apakah fungsi pemicu (`openRefund`, `openAdjustment`, `openWriteOff`) disediakan atau tidak.

---

## 2. Proses bisnis dari sisi pengguna

1. **Kasir Meninjau Tagihan pada Riwayat Pembayaran**:
   - Kasir membuka menu **Riwayat Pembayaran** (`/health-services/billing-management/billing-invoices/payment-history`).
   - Kasir dapat mencari invoice berdasarkan nama pasien, No. RM, rentang tanggal kunjungan, atau tipe layanan.
   - Setiap baris tagihan menampilkan rincian: Tanggal, No. Invoice, Pasien, Layanan, Total Tagihan, Penjamin/Klaim, Status Pembayaran (Lunas/Cicilan), kolom **Kwitansi**, dan kolom **Aksi**.
2. **Kasir Memilih Aksi Pengecualian Finansial**:
   - Pada baris invoice yang membutuhkan penyesuaian finansial, kasir mengklik tombol dropdown `Aksi ▾`.
   - Muncul 3 menu pilihan tindakan:
     - **+ Ajukan Refund**: untuk mengembalikan kelebihan pembayaran pasien (misal pasien membayar tunai berlebih atau ada pembatalan tindakan).
     - **+ Ajukan Adjustment**: untuk mengoreksi nilai tagihan (penyesuaian *credit* atau *debit* yang bersifat append-only) karena perubahan kesepakatan atau koreksi administratif.
     - **+ Ajukan Write-off**: untuk menghapuskan piutang pasien yang tidak tertagih atau selisih yang tidak dapat ditagihkan.
3. **Pemuatan Detail Invoice dan Pembukaan Modal Terkait**:
   - Saat kasir memilih salah satu menu aksi, sistem secara otomatis mengambil data detail invoice terkini dari backend (`getBillingInvoiceById`) guna memastikan `rowVersion` dan data status tagihan valid dan sinkron (mencegah *concurrency conflict* `409`).
   - Modal yang sesuai langsung terbuka di hadapan kasir:
     - **Modal Refund (`CreateRefundModal`)**: Kasir memilih kredit yang dapat di-refund (*refundable credit*), mengisi nominal pengajuan, dan mengisi alasan pengajuan.
     - **Modal Adjustment (`CreateAdjustmentModal`)**: Kasir memilih arah penyesuaian (*Kredit* / *Debit*), mengisi nominal, dan mengisi alasan.
     - **Modal Write-off (`CreateWriteOffModal`)**: Kasir memilih kategori write-off (*Piutang Pasien* / *Selisih Tidak Dapat Ditagihkan*), mengisi nominal, dan mengisi alasan.
4. **Pengiriman dan Umpan Balik (Toast Notification)**:
   - Kasir menekan tombol kirim/simpan pada modal. Permintaan dikirim ke API backend existing (`createBillingRefund`, `createBillingAdjustment`, atau `createBillingWriteOff`).
   - Sistem menampilkan pesan notifikasi *Toast* sukses atau error di pojok layar.
   - Alur persetujuan maker-checker (`BIL-PERMISSION-1.2`) tetap berjalan: pengajuan berstatus *Submitted* dan menunggu persetujuan pengguna berwenang lain sebelum diposting ke ledger.
5. **Halaman Menu Pembayaran Bersih dari Tombol Pengajuan**:
   - Pada halaman Menu Pembayaran (`menu-pembayaran-view.jsx`), tombol pengajuan Refund, Adjustment, dan Write-Off sudah tidak tampil lagi, menjaga fokus kasir pada proses penerimaan pembayaran berjalan (`UAT-BUI-13`).
6. **Perilaku Kolom Kwitansi Tetap Terjaga**:
   - Tombol "Lihat Kwitansi" dan dropdown cicilan angsuran pada Riwayat Pembayaran tetap berfungsi normal seperti sedia kala tanpa terganggu oleh penambahan kolom Aksi (`UAT-BUI-14`).

---

## 3. Rincian Perubahan Berkas

Berikut adalah rincian berkas yang diubah pada repositori `QuilvianSystemFrontendDev`:

### 3.1 `src/components/view/health-services/billing-management/billing-invoices/detail/billing-financial-exception-panel.jsx`
- **Perubahan**:
  - Mengubah rendering tombol pengajuan `+ Ajukan Refund`, `+ Ajukan Adjustment`, dan `+ Ajukan Write-off` menjadi kondisional: `{openRefund ? (...) : null}`, `{openAdjustment ? (...) : null}`, `{openWriteOff ? (...) : null}`.
  - Hal ini memungkinkan panel tetap dipakai untuk menampilkan daftar pelacakan kasus finansial di Menu Pembayaran atau layar lainnya tanpa harus memunculkan tombol pemicu bila prop tidak dipassing.

### 3.2 `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx`
- **Perubahan**:
  - Menghapus impor `CreateRefundModal`, `CreateAdjustmentModal`, dan `CreateWriteOffModal`.
  - Menghapus prop `openRefund`, `openAdjustment`, dan `openWriteOff` dari elemen `<BillingFinancialExceptionPanel ... />`.
  - Menghapus JSX modal `<CreateRefundModal ... />`, `<CreateAdjustmentModal ... />`, dan `<CreateWriteOffModal ... />`.
  - Memenuhi acceptance criteria `UAT-BUI-13` (tombol Refund/Adjustment/Write-Off tidak lagi tampil di Menu Pembayaran).

### 3.3 `src/components/view/health-services/billing-management/billing-invoices/payment-history-view.module.css`
- **Perubahan**:
  - Menambahkan kelas styling baru untuk menu dropdown aksi finansial:
    - `.actionMenuWrapper`: container flexbox untuk tombol aksi.
    - `.actionDropdownToggle`: gaya tombol toggle dropdown yang selaras dengan tema sistem (tinggi 32px, padding proporsional, border lembut, transisi hover/focus).
    - `.actionDropdownMenu`: popup menu dengan box-shadow elegan dan border-radius standar.
    - `.actionDropdownItem`: item menu dengan efek hover interaktif dan tipografi yang jelas.

### 3.4 `src/components/view/health-services/billing-management/billing-invoices/payment-history-view.jsx`
- **Perubahan**:
  - Menambahkan impor: `useCallback`, `useEffect`, `useState`, `useDispatch`, `ToastStack`, `CreateRefundModal`, `CreateAdjustmentModal`, `CreateWriteOffModal`, `useBillingFinancialException`, dan `getBillingInvoiceById`.
  - Menambahkan kolom baru `"Aksi"` (`key: "financialActions"`) pada `buildColumns({ buildKwitansiLink, onSelectAction })`, berdampingan dengan kolom Kwitansi yang tetap utuh.
  - Menyediakan handler `handleSelectAction(item, actionType)` yang mengambil detail invoice terkini (`getBillingInvoiceById`) dan memasangnya ke `selectedInvoice` serta menyiapkan `pendingAction`.
  - Mengintegrasikan hook `useBillingFinancialException` dengan context invoice terpilih.
  - Merender modal `<CreateRefundModal>`, `<CreateAdjustmentModal>`, `<CreateWriteOffModal>`, dan `<ToastStack>`.
  - Memenuhi acceptance criteria `UAT-BUI-08` dan `UAT-BUI-14`.

---

## 4. Evaluasi Base Component Decision Gate

Sesuai aturan rekayasa antarmuka pengguna Quilvian (*Base Component Reuse & Decision Gate*):

| Elemen UI | Sumber Komponen | Status Keputusan | Alasan dan Justifikasi |
| --- | --- | :---: | --- |
| Modal Pengajuan Refund | `@/components/view/.../create-refund-modal` | `REUSE` | Menggunakan komponen modal refund existing yang sudah terstandarisasi validasi dan formatnya |
| Modal Pengajuan Adjustment | `@/components/view/.../create-adjustment-modal` | `REUSE` | Menggunakan komponen modal adjustment existing tanpa duplikasi logika |
| Modal Pengajuan Write-Off | `@/components/view/.../create-write-off-modal` | `REUSE` | Menggunakan komponen modal write-off existing tanpa duplikasi logika |
| Tombol & Menu Dropdown Aksi | `react-bootstrap/Dropdown` | `REUSE` | Menggunakan komponen Dropdown standar yang konsisten dengan dropdown Kwitansi pada layar yang sama |
| Tombol Aksi Modal / Kwitansi | `@/components/features/base-features/base-button` | `REUSE` | Menggunakan base button standar Quilvian dengan varian `primary` dan `secondary` |
| Notifikasi Toast | `@/components/features/base-features/toast-stack` | `REUSE` | Menggunakan `ToastStack` standar Quilvian untuk umpan balik berhasil atau gagal |

```text
UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

Seluruh elemen berstatus `REUSE` 100%. Tidak ada komponen baru (`NEW = 0`) yang dibuat.

---

## 5. Bukti Verifikasi Kualitas (*Evidence-Based Verification*)

1. **Pemeriksaan Linter (`npm run lint:errors`)**:
   - Perintah: `npm run lint:errors`
   - Hasil: **LULUS (0 errors, 0 warnings, exit code 0)**.
2. **Pengujian Unit (`npm run test:unit`)**:
   - Perintah: `npm run test:unit`
   - Hasil: **1.685 lulus, 8 gagal** (8 kegagalan terbukti identik dengan baseline commit awal `b3f45db7b`, terisolasi pada modul lama yang tidak terdampak).
   - Seluruh test suite modul *billing management* lulus 100%.
3. **Kompilasi Produksi (`npm run build`)**:
   - Perintah: `npm run build`
   - Hasil: **LULUS (Exit code 0)**.
   - Next.js 16.2.12 berhasil mengompilasi seluruh 406 rute statis/dinamis.
   - Script standalone runtime `scripts/prepare-standalone.mjs` berhasil dijalankan dan aset statis siap untuk deployment.
4. **Verifikasi Kontrak & Keamanan**:
   - **Nol Endpoint Baru (`BIL-API-1.4`)**: Tidak ada endpoint backend baru yang dibuat. Seluruh interaksi memakai endpoint Refund, Adjustment, dan Write-Off existing.
   - **Otorisasi Tetap Terjaga (`BIL-PERMISSION-1.2`)**: Otorisasi maker-checker dan hak akses kasir/supervisor tetap berlaku utuh dari sisi backend.
5. **Verifikasi Kompatibilitas Kolom Kwitansi (`UAT-BUI-14`)**:
   - Kolom Kwitansi pada `payment-history-view.jsx` mempertahankan fungsi `buildKwitansiLink`, tombol tunggal "Lihat Kwitansi", serta dropdown cicilan angsuran tanpa regresi apa pun.

---

## 6. Pemenuhan Acceptance Criteria & Definition of Done

| Kriteria / Syarat | Status | Bukti |
| --- | :---: | --- |
| **`UAT-BUI-08`**: Tombol aksi pada Riwayat Pembayaran membuka modal Refund, Adjustment, Write-Off dengan alur persetujuan yang sama | ✅ Terpenuhi | Dropdown `Aksi ▾` pada setiap baris Riwayat Pembayaran berhasil memicu pembukaan `CreateRefundModal`, `CreateAdjustmentModal`, dan `CreateWriteOffModal`. Data `invoice` di-fetch secara fresh dan dikirim ke hook `useBillingFinancialException` |
| **`UAT-BUI-13`**: Halaman Menu Pembayaran diperiksa setelah amendment: tombol Refund/Adjustment/Write-Off tidak lagi tampil | ✅ Terpenuhi | Tombol `openRefund`, `openAdjustment`, `openWriteOff` dan modal terkait telah dihapus dari `menu-pembayaran-view.jsx` |
| **`UAT-BUI-14`**: Kolom Kwitansi existing pada Riwayat Pembayaran tidak berubah perilaku cetak struk | ✅ Terpenuhi | Kolom Kwitansi dipertahankan 100% utuh berdampingan dengan kolom Aksi baru |
| `BIL-API-1.4`: Nol endpoint baru | ✅ Terpenuhi | Hanya memanfaatkan thunk Redux dan API backend existing |
| `BIL-PERMISSION-1.2`: Hak akses tidak berubah | ✅ Terpenuhi | Mengikuti wewenang maker-checker existing |
| `npm run lint:errors` lulus | ✅ Terpenuhi | 0 error (exit code 0) |
| `npm run test:unit` lulus / konsisten dengan baseline | ✅ Terpenuhi | 1.685 lulus, 8 kegagalan konsisten dengan baseline commit awal |
| `npm run build` lulus | ✅ Terpenuhi | Exit code 0, 406 rute berhasil dikompilasi |
| Laporan task tracked dibuat | ✅ Terpenuhi | Dokumen ini (`docs/module-blueprints/billing-kasir/task/report/frontend/FE-BUI-004.md`) |
| Roadmap & Traceability diperbarui | ✅ Terpenuhi | Node mermaid, tabel gelombang, daftar task, kartu FE-BUI-004, dan requirement-traceability diperbarui |
