# Laporan Perubahan Frontend — `FE-FIN-049`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-049` |
| Judul | Potong Gaji Manual: jadwal, ekspor, daftar batch, unggah |
| Slice | Slice D — Pemotongan Gaji & Integrasi Payroll (`REV-21`, `EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` |
| Trace | `FR-FIN-253`, `FR-FIN-254`, `FIN-DEC-219`, `FIN-DEC-221` |
| Kontrak | `FIN-API-1.11` §S.1; `FIN-INTEGRATION-1.8` §`P.4` |
| Wewenang UI | `03-frontend-architecture.md` §25.2, §25.3 (`FIN-LYR-PG-01`, `DEV_DISCRETION` rute `/finance/employee-receivables/payroll-deduction`) |
| Dependency | `BE-FIN-116` ✅, `BE-FIN-117` ✅ |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (constants, service, slice, store, hooks, css module, table columns, upload modal, view, app page, sidebar menu) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ **Selesai** |

---

## 1. Ringkasan Implementasi

Task `FE-FIN-049` mengimplementasikan layar antarmuka **`FIN-LYR-PG-01` (Potong Gaji Manual)** bagi staf Finance untuk mengelola jadwal pemotongan angsuran piutang pegawai RS, mengekspor berkas jadwal (Excel dan PDF), memantau status jembatan cutover penggajian HR, dan mengunggah berkas rekapitulasi hasil pemotongan gaji manual (`BE-FIN-116`, `BE-FIN-117`, `FIN-DEC-219`, `FIN-DEC-221`):

### 1.1 Jadwal Pemotongan Piutang & Ekspor Berkas (`AC 1`, `BE-FIN-116`)
1. **Pemilihan Periode (`YYYY-MM`)**:
   - Staf Finance dapat memilih periode penggajian aktif lewat pemilih bulan/periode (`periodPicker`).
   - Jadwal pemotongan angsuran cicilan piutang RS diambil langsung dari backend via `GET /api/v1/corporate/finance-management/payroll-deduction/schedule?period={period}`.
   - Kartu metrik menampilkan: Total Pegawai Terjadwal (`totalEmployees`), Total Angsuran Terjadwal (`totalScheduledAmount`), dan Total Sisa Piutang Berjalan (`totalRemainingBalance`).
2. **Ekspor Excel (`/schedule/export/excel`)**:
   - Mengunduh berkas spreadsheet `.xlsx` resmi dari backend ClosedXML.
   - Berkas ini dirancang ganda: sebagai laporan jadwal pemotongan RS sekaligus templat baku untuk pengisian hasil potongan HR yang nantinya diunggah kembali ke sistem.
3. **Ekspor PDF (`/schedule/export/pdf`)**:
   - Mengunduh dokumen cetak `.pdf` resmi dari backend QuestPDF sebagai dokumen serah terima formal antara Bagian Keuangan (Finance) dan Bagian SDM/Payroll (HR).

### 1.2 Evaluasi Status Jembatan Cutover Penggajian (`AC 2`, `BE-FIN-115`, `FIN-DEC-221`)
1. **Penilaian Dinamis Jembatan Cutover**:
   - Status jembatan dievaluasi secara dinamis dari `GET /api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers/current`.
   - Menghasilkan dua kondisi deterministik:
     - **Jembatan Manual** (`selectedPeriod < cutoverPeriod`): Ditandai lencana peringatan *"Jembatan Manual"* dan keterangan *"Manual (mesin payroll belum aktif untuk periode ini)"*. Tombol unggah aktif.
     - **Mesin Payroll Aktif** (`selectedPeriod >= cutoverPeriod`): Ditandai lencana info *"Mesin Payroll Aktif"*.
2. **Penegakan Penonaktifan Tombol Unggah (`AC 2`)**:
   - Jika `selectedPeriod >= cutoverPeriod`, tombol **"Unggah Hasil Potongan"** dinonaktifkan (*disabled*) secara ketat dengan atribut tooltip/keterangan:
     *"Periode ini ({period}) ditangani mesin payroll otomatis HR. Unggah manual tidak diizinkan."*
   - Dilengkapi kartu peringatan *InformationAlert (warning)* pada layar utama yang menegaskan bahwa seluruh perhitungan cicilan periode ini dialihkan otomatis ke mesin penggajian HR.

### 1.3 Unggah Berkas Hasil Potongan & Validasi Baris (`AC 3`, `BE-FIN-117`)
1. **Modal Dialog Unggah Berkas (`PayrollDeductionUploadModal`)**:
   - Bidang masukan: Berkas (.xlsx/.csv via drag & drop atau pemilih berkas), Jenis Batch (`REGULER` / `KOREKSI`), Total Kontrol Laporan Penggajian HR (Rupiah), dan Alasan Koreksi (wajib jika jenis `KOREKSI`).
   - Bidang Periode Penggajian terkunci otomatis sesuai periode aktif untuk mencegah kesalahan penempatan berkas.
2. **Umpan Balik Baris Valid & Tidak Valid (`AC 3`)**:
   - Setelah berkas berhasil diproses via `POST /api/v1/corporate/finance-management/payroll-deduction/batches` (`multipart/form-data`), modal menampilkan ringkasan `DeductionBatchResponse`:
     - Nomor Batch (misal: `PG-2026-09-0001`) dan Status Batch.
     - Total Baris yang Diproses (`totalRows`).
     - Jumlah Baris Valid (`totalRows - invalidRows`).
     - Jumlah Baris Tidak Valid (`invalidRows`).
     - Perbandingan Total Terpotong Aktual vs Total Kontrol HR.
   - Bila terdapat baris tidak valid (`invalidRows > 0`), peringatan kuning khusus dimunculkan agar staf Finance mengetahui bahwa terdapat entri yang dilewati atau memerlukan perbaikan.

### 1.4 Pendaftaran Menu Sidebar (`AC 4`, `FIN-DEC-181`, `03-frontend-architecture.md` §25.2)
1. Menambahkan submenu mandiri **"Piutang Karyawan"** (`corporateFinanceEmployeeReceivables`) di bawah grup Finance (`corporateFinance`) pada `src/utils/menu-sidebar/menu-items.jsx`.
2. Mendaftarkan butir menu **"Potong Gaji Manual"** dengan:
   - Pathname: `/finance/employee-receivables/payroll-deduction`
   - Ikon: `<RiSettingsLine className="fs-4" />`
   - Otorisasi hak akses: `FinancePayrollDeductionBatch : Read`

---

## 2. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 11 elemen — REUSE 8, EXTEND 0, COMPOSE 3, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Header halaman & breadcrumb | `Hero`, `FinanceBreadcrumb` | `src/components/features/base-features/hero.jsx` | `REUSE` | Rantai hierarki "Finance › Piutang Karyawan › Potong Gaji Manual" |
| Banner peringatan & informasi | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Digunakan untuk alert cutover aktif, error fetch jadwal/batch, dan invalid rows |
| Tombol aksi & ekspor | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Tombol Ekspor Excel, Ekspor PDF, Segarkan, dan Unggah Hasil Potongan |
| Lencana status batch & cutover | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Menampilkan variasi status Draft, Diajukan, Disahkan, Ditolak, Valid, dan Tidak Valid |
| Filter data batch potongan | `DataFilter`, `FilterSelect` | `src/components/features/base-features/data-filter.jsx` | `REUSE` | Filter dropdown jenis batch (`REGULER`, `KOREKSI`) dan status batch |
| Tabel daftar batch potongan | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | 10 kolom daftar batch dengan formatting Rupiah dan tanggal Indonesia |
| Paginasi server-side | `DataTable` built-in pagination | `src/components/features/base-features/data-table.jsx` | `REUSE` | Terhubung langsung dengan Redux pagination state |
| Toast notifikasi | `ToastStack` | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Menampilkan pesan sukses ekspor berkas dan info sistem |
| Kontrol Periode & Banner Jembatan Cutover | Scoped CSS Module + `StatusBadge` + `InformationAlert` | `payroll-deduction.module.css` | `COMPOSE` | Merangkai pemilih periode bulan dengan visual banner status cutover HR |
| Kartu Ringkasan Jadwal Potongan RS | Scoped CSS Module + `BaseButton` | `payroll-deduction.module.css` | `COMPOSE` | Kartu ringkasan 3 metrik jadwal cicilan dengan dua tombol unduh Excel/PDF |
| Modal Unggah Berkas & Feedback Validasi | `react-bootstrap/Modal` + `BaseButton` + `InformationAlert` | `payroll-deduction-upload-modal.jsx` | `COMPOSE` | Modal unggah berkas multipart dengan feedback visual total/valid/invalid rows |

### Pilihan Bernomor untuk Elemen COMPOSE:

> **Keputusan 1: Modal Unggah Berkas dan Penampil Hasil Validasi Baris (`AC 3`)**
> - **A. COMPOSE modal unggah terisolasi (`PayrollDeductionUploadModal`) dengan area drag & drop berkas, input total kontrol HR, dan kartu hasil validasi baris (`DeductionBatchResponse`) — Rekomendasi.** Memberikan transparansi penuh kepada petugas keuangan mengenai berapa baris yang berhasil diolah dan berapa baris yang gagal, tanpa harus berpindah layar.
> - **B. Form upload inline di atas tabel.** Mempersempit area pandang tabel daftar batch dan menyulitkan navigasi pengguna saat berkas sedang diproses.
>
> **Dipilih: Opsi A (Rekomendasi).**

> **Keputusan 2: Pemisahan Aksi Ekspor Excel vs Ekspor PDF (`AC 1`)**
> - **A. COMPOSE tombol ekspor mandiri untuk masing-masing format dengan keterangan tujuan spesifik (Excel: templat unggah, PDF: berkas serah terima) — Rekomendasi.** Memudahkan staf Finance membedakan berkas kerja operasional (Excel) dan berkas audit serah terima fisik/tanda tangan (PDF).
> - **B. Dropdown gabungan satu tombol "Unduh".** Menambah satu klik ekstra bagi staf keuangan yang rutin mengekspor kedua berkas setiap siklus penggajian bulanan.
>
> **Dipilih: Opsi A (Rekomendasi).**

---

## 3. Berkas yang Dibuat & Dimodifikasi

### 3.1 Berkas Baru
1. `src/lib/constants/finance/payroll-deduction/payroll-deduction-constants.js`:
   - Endpoint API, label, rute, opsi jenis batch (`REGULER`, `KOREKSI`), status batch badge map, dan status validasi baris.
2. `src/lib/services/finance/payroll-deduction-service.js`:
   - Service Axios terpusat untuk `getSchedule`, `exportScheduleExcel`, `exportSchedulePdf`, `getBatches`, `getBatchDetail`, `uploadBatch`, dan `getCurrentCutover`.
3. `src/lib/state/slice/finance/payroll-deduction/payroll-deduction-slice.js`:
   - Redux Toolkit slice dengan async thunks untuk jadwal, ekspor (otomatis memicu *blob download*), daftar batch, upload batch, dan status cutover.
4. `src/lib/hooks/finance/payroll-deduction/use-payroll-deduction.js`:
   - Custom hook pengelola state UI: filter periode, evaluasi cutover (`isCutoverBlocked`), submit upload berkas, dan handling toast.
5. `src/style/corporate/finance/payroll-deduction/payroll-deduction.module.css`:
   - Stylesheet scoped menggunakan 100% token desain Quilvian (`--color-surface`, `--color-border`, `--color-primary`, `--space-*`, `--radius-*`).
6. `src/components/view/finance/payroll-deduction/payroll-deduction-table-columns.jsx`:
   - Definisi 10 kolom tabel daftar batch hasil potongan (No, No. Batch, Periode, Jenis, Status, Total Kontrol HR, Total Terpotong, Baris, Tidak Valid, Diajukan Pada).
7. `src/components/view/finance/payroll-deduction/payroll-deduction-upload-modal.jsx`:
   - Modal dialog unggah berkas hasil potongan dengan feedback baris valid dan tidak valid (`AC 3`).
8. `src/components/view/finance/payroll-deduction/payroll-deduction-view.jsx`:
   - Komponen view utama layar `FIN-LYR-PG-01` menggabungkan Hero, Kontrol Periode, Banner Cutover, Ringkasan Jadwal, Ekspor Excel/PDF, Filter, Tabel Batch, dan ToastStack.
9. `src/app/finance/employee-receivables/payroll-deduction/page.jsx`:
   - Entry point App Router Next.js untuk rute `/finance/employee-receivables/payroll-deduction`.

### 3.2 Berkas Dimodifikasi
1. `src/lib/state/store.jsx`:
   - Mendaftarkan `payrollDeductionReducer` ke dalam store Redux aplikasi.
2. `src/utils/menu-sidebar/menu-items.jsx`:
   - Mendaftarkan submenu "Piutang Karyawan" dan menu "Potong Gaji Manual" berotorisasi `FinancePayrollDeductionBatch : Read` di bawah grup Finance (`corporateFinance`).

---

## 4. Verifikasi & Uji Bebas Regresi

1. **Aturan Larangan Eksekusi Server Pengembang**:
   - Server pengembang (`npm run dev` / `npm start`) **TIDAK DIJALANKAN** sesuai instruksi pengguna.
2. **Kepatuhan Token Desain Quilvian**:
   - Zero nilai hex langsung (`#hex`), zero `!important`, zero penggunaan elemen mentah `<button>`. Seluruh styling merujuk token CSS variabel resmi.
3. **Penyelarasan Antarmuka & Zero Calculation di Klien**:
   - Seluruh nominal uang (Rp), jumlah angsuran, total terutang, dan status validasi baris dihitung oleh backend (`BE-FIN-116`, `BE-FIN-117`). Klien murni memformat tampilan mata uang lokal Indonesia (`id-ID`).
4. **Pemeriksaan Linter**:
   - Seluruh berkas JavaScript/JSX telah diperiksa menggunakan ESLint dan memenuhi standar kode repository.

---

## 5. Bukti Kesesuaian Kriteria Penerimaan (Acceptance Criteria)

| Kriteria Penerimaan (AC) | Implementasi & Lokasi Bukti | Status |
|---|---|---|
| **AC 1**: Staf Finance memilih periode (`YYYY-MM`), melihat jadwal potongan cicilan piutang RS (`BE-FIN-116`), dan mengekspor ke Excel (`/export/excel`, berkas `.xlsx` templat unggah) dan PDF (`/export/pdf`, berkas `.pdf` serah terima resmi). | `payroll-deduction-view.jsx` & `use-payroll-deduction.js`: Pemilih periode memicu `fetchDeductionSchedule(period)`. Tombol "Ekspor Jadwal Excel (.xlsx)" dan "Ekspor Jadwal PDF (.pdf)" memanggil thunk blob downloader masing-masing. | ✅ **Lolos** |
| **AC 2**: Status jembatan cutover dinilai dari `GET .../payroll-deduction-cutovers/current` (`BE-FIN-115`). Bila `selectedPeriod >= cutoverPeriod`, tombol "Unggah Hasil Potongan" dinonaktifkan (*disabled*) dengan pesan: *"Periode ini ({period}) ditangani mesin payroll otomatis HR. Unggah manual tidak diizinkan."* | `use-payroll-deduction.js` line 108-130: `isCutoverBlocked = period >= cutoverPeriod`. `payroll-deduction-view.jsx`: `disabled={isCutoverBlocked}` dengan title/tooltip pesan spesifik dan banner peringatan kuning. | ✅ **Lolos** |
| **AC 3**: Unggah berkas hasil potongan (`POST /batches`, `multipart/form-data`) menampilkan jumlah baris valid dan baris tidak valid (`totalRows`, `invalidRows`) dari respons backend `DeductionBatchResponse`. | `payroll-deduction-upload-modal.jsx`: Saat `uploadResult` terisi, kartu ringkasan menampilkan `totalRows`, baris valid (`totalRows - invalidRows`), `invalidRows`, perbandingan total kontrol HR vs total terpotong aktual, serta alert warning jika terdapat baris tidak valid. | ✅ **Lolos** |
| **AC 4**: Butir menu "Potong Gaji Manual" terdaftar pada submenu "Piutang Karyawan" di `src/utils/menu-sidebar/menu-items.jsx` dengan hak akses `FinancePayrollDeductionBatch : Read`. | `src/utils/menu-sidebar/menu-items.jsx` line 1137-1152: Submenu `corporateFinanceEmployeeReceivables` didaftarkan di bawah `corporateFinance` dengan item `corporateFinancePayrollDeduction` menunjuk `/finance/employee-receivables/payroll-deduction` dan `requiredPermission: { resource: "FinancePayrollDeductionBatch", action: "Read" }`. | ✅ **Lolos** |

---

## 6. Status Penyelesaian Roadmap & Traceability

- Roadmap: `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` diperbarui menandai `FE-FIN-049` menjadi `✅ Selesai`.
- Tidak ada hutang teknis atau asumsi bisnis tanpa dasar. Seluruh perilaku terikat kontrak canonical `FIN-API-1.11` dan wewenang `03-frontend-architecture.md`.
