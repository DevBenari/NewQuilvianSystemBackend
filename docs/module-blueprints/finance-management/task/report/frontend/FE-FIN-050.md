# Laporan Perubahan Frontend — `FE-FIN-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-050` |
| Judul | Rincian batch potong gaji: ajukan, sahkan, tolak, koreksi |
| Slice | Slice D — Pemotongan Gaji & Integrasi Payroll (`REV-21`, `EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` |
| Trace | `FR-FIN-255`..`257`, `FIN-DEC-220`, `FIN-DEC-222` |
| Kontrak | `FIN-API-1.11` §S.1; `FIN-PERM-1.12` |
| Wewenang UI | `03-frontend-architecture.md` §25.2, §25.3 (`FIN-LYR-PG-02`, layar anak `/finance/employee-receivables/payroll-deduction/[id]`) |
| Dependency | `FE-FIN-049` ✅, `BE-FIN-117` ✅, `BE-FIN-118` ✅ |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (service, slice, hooks, css module, table columns, action modals, detail view, router dynamic page, parent list navigation) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ **Selesai** |

---

## 1. Ringkasan Implementasi

Task `FE-FIN-050` mengimplementasikan layar anak **`FIN-LYR-PG-02` (Rincian Batch Potong Gaji)** bagi penyelia dan pejabat Finance untuk memeriksa hasil validasi berkas potongan per baris, menegakkan prinsip pemisahan wewenang pembuat-pemeriksa (*maker-checker separation*), mengajukan batch dengan bukti laporan penggajian, dan mengesahkan atau menolak batch secara atomik (`BE-FIN-117`, `BE-FIN-118`, `FIN-DEC-220`, `FIN-DEC-222`):

### 1.1 Penegakan Prinsip Maker-Checker pada Pengesahan (`AC 1`, `FIN-DEC-220`)
1. **Penyembunyian Tombol Sahkan bagi Pengaju (`AC 1`)**:
   - Berdasarkan aturan ketat tata kelola keuangan (*financial governance* `FIN-DEC-220`), petugas yang mengajukan batch dilarang mengesahkan batch miliknya sendiri.
   - Pada `use-payroll-deduction-detail.js` dan `payroll-deduction-detail-view.jsx`, status pengaju (`isSubmitter`) dievaluasi dari ID pengguna aktif terhadap `submittedBy` batch. Jika pengguna aktif adalah pihak yang mengajukan batch, tombol **"Sahkan Batch"** / **"Sahkan Koreksi"** **TIDAK DITAMPILKAN** di layar.
   - Layar menampilkan *InformationAlert* informatif bertema tata kelola (*governance notice*) yang menjelaskan kepada pengaju bahwa pengesahan wajib dilakukan oleh penyelia atau pejabat keuangan yang berbeda.
   - Sebagai pengaman lapis ganda, backend (`BE-FIN-117` baris 664) secara mandiri menolak pengesahan oleh pengaju dengan kode `403 Forbidden` (*"Pengaju tidak boleh mengesahkan batchnya sendiri"*).

### 1.2 Penandaan Baris Tidak Valid dengan Pesan Backend (`AC 2`, `BE-FIN-117`)
1. **Tabel Baris Item Hasil Potongan**:
   - Menampilkan rincian data per pegawai: nomor baris, kode angsuran cicilan piutang RS, NIP, nama pegawai, status pemotongan (`BERHASIL`, `SEBAGIAN`, `GAGAL`), nominal terpotong aktual, nominal total terutang, dan selisih koreksi (bila batch Koreksi).
2. **Umpan Balik Validasi Spesifik**:
   - Setiap baris yang berstatus `INVALID` ditandai dengan lencana merah **"TIDAK VALID"** dan menyajikan pesan kesalahan orisinal langsung dari backend (`validationMessage`), misalnya:
     - *"NIP pegawai tidak ditemukan dalam jadwal angsuran periode ini."*
     - *"Nominal terpotong melebihi total kewajiban terutang."*
     - *"Kode angsuran tidak valid atau duplikat pada berkas."*

### 1.3 Pengajuan Batch dengan Nomor Laporan & Bukti Wajib (`AC 3`, `BE-FIN-117`)
1. **Modal Pengajuan (`SubmitDeductionBatchModal`)**:
   - Memfasilitasi input Nomor Laporan Penggajian HR (`payrollReportNumber`, string wajib) dan Dokumen Bukti Transaksi (`ProofId`, Guid wajib).
   - Pengunggahan berkas bukti mengintegrasikan komponen baku `TransactionProofUploadField` ke endpoint terpusat `/v1/corporate/finance-management/transaction-proofs`.
2. **Pemberitahuan Galat bila Bukti Kosong (`AC 3`)**:
   - Jika staf mencoba mengajukan batch tanpa mengunggah bukti fisik laporan HR, sistem menampilkan pesan validasi: *"Dokumen bukti laporan penggajian HR wajib diunggah sebelum pengajuan."*
   - Backend memvalidasi integritas relasi `ProofId` dan menolak pengajuan tanpa dokumen bukti.

### 1.4 Pengesahan Batch Koreksi oleh Pejabat Berwenang (`AC 4`, `BE-FIN-118`, `FIN-PERM-1.12`)
1. **Otorisasi Khusus Batch Koreksi (`ApproveCorrection`)**:
   - Untuk batch berjenis `REGULER`, tombol pengesahan membutuhkan izin `FinancePayrollDeductionBatch : Approve`.
   - Untuk batch berjenis `KOREKSI`, pengesahan hanya dapat dilakukan oleh pejabat yang memiliki hak akses khusus `FinancePayrollDeductionBatch : ApproveCorrection` (`FIN-PERM-1.12`). Pengguna biasa atau penyelia reguler tanpa hak akses ini tidak dapat melihat/menekan tombol pengesahan koreksi.
2. **Pengesahan Atomik ke Subledger**:
   - Memanggil `POST /batches/{id}/approve-correction` yang secara atomik menerapkan penyesuaian nominal angsuran, mencatat mutasi pembalikan potongan gaji di subledger, dan menyesuaikan tunggakan terbawa periode berikutnya (`BE-FIN-118`).

### 1.5 Deteksi Angsuran Terjadwal yang Belum Ada di Berkas
1. **Panel Missing Scheduled Installments**:
   - Membandingkan daftar angsuran aktif pada periode terkait terhadap baris yang ada di berkas HR.
   - Bila terdapat cicilan yang belum dicantumkan oleh HR, panel oranye memuat tabel daftar angsuran yang terlewat agar petugas dapat meminta konfirmasi ke HR (HR harus mencantumkan status `GAGAL` jika memang tidak terpotong).
   - Bila seluruh cicilan telah tercakup, panel hijau menampilkan konfirmasi kelengkapan berkas.

---

## 2. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 15 elemen — REUSE 9, EXTEND 0, COMPOSE 6, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Header halaman & breadcrumb | `Hero`, `FinanceBreadcrumb` | `src/components/features/base-features/hero.jsx` | `REUSE` | Rantai hierarki "Finance › Piutang Karyawan › Potong Gaji Manual › [No. Batch]" |
| Banner peringatan & informasi | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Digunakan untuk notice maker-checker, alert invalid rows, dan penolakan |
| Tombol aksi (Kembali, Sahkan, Tolak, Batalkan, Ajukan) | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Varian `primary`, `success`, `danger`, `ghost`, `subtle` |
| Lencana status batch, hasil, & validasi | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Menampilkan variasi status Reguler, Koreksi, Valid, Tidak Valid, Berhasil, Sebagian, Gagal |
| Tabel baris item hasil potongan | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | Tabel 9 kolom dengan styling token wilayah administrasi |
| Tabel angsuran yang belum ada di berkas | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | Tabel 7 kolom memuat cicilan yang belum dilaporkan |
| Unggah bukti transaksi laporan HR | `TransactionProofUploadField` | `src/components/view/finance/shared/transaction-proof-upload-field.jsx` | `REUSE` | Pengunggah berkas resmi ke endpoint transaction-proofs |
| Toast notifikasi | `ToastStack` | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Umpan balik aksi pengajuan, pengesahan, penolakan, pembatalan |
| Gerbang akses hak pengguna | `AccessDeniedGate` | `src/components/features/base-features/access-denied-gate.jsx` | `REUSE` | Menangani penolakan akses `403` secara anggun |
| Panel Ringkasan Metadata Batch | Scoped CSS Module + `StatusBadge` | `payroll-deduction.module.css` | `COMPOSE` | Merangkai 8 metrik utama batch dan status verifikasi |
| Kolom Validasi Baris & Pesan Backend (`AC 2`) | `StatusBadge` + teks error terstruktur | `payroll-deduction-detail-columns.jsx` | `COMPOSE` | Menampilkan lencana merah dan pesan error backend di bawah lencana |
| Panel Angsuran Terjadwal yang Belum Ada | `InformationAlert` + `DataTable` | `payroll-deduction-detail-view.jsx` | `COMPOSE` | Kartu kondisional deteksi kelengkapan baris berkas terhadap jadwal aktif |
| Modal Pengajuan Batch dengan Bukti (`AC 3`) | `react-bootstrap/Modal` + `TransactionProofUploadField` + `BaseButton` | `payroll-deduction-action-modals.jsx` | `COMPOSE` | Formulir nomor laporan HR dan unggah bukti transaksi |
| Modal Pengesahan Batch Atomik (`AC 1`, `AC 4`) | `react-bootstrap/Modal` + `BaseButton` | `payroll-deduction-action-modals.jsx` | `COMPOSE` | Dialog konfirmasi transaksi atomik dengan catatan opsional |
| Modal Penolakan & Pembatalan Batch | `react-bootstrap/Modal` + `BaseButton` | `payroll-deduction-action-modals.jsx` | `COMPOSE` | Dialog input alasan wajib sebelum penolakan atau pembatalan |

### Pilihan Bernomor untuk Elemen COMPOSE:

> **Keputusan 1: Penyajian Pesan Validasi Baris pada Tabel Item (`AC 2`)**
> - **A. COMPOSE kolom validasi terpadu (`StatusBadge` + teks pesan galat orisinal backend di bawahnya) — Rekomendasi.** Petugas dapat langsung membaca alasan mengapa suatu baris ditolak tanpa harus mengarahkan kursor (*hover*) atau membuka dialog tambahan.
> - **B. Tooltip pada ikon peringatan kecil.** Menyulitkan pembacaan cepat ketika terdapat banyak baris bermasalah dan tidak ramah pada perangkat layar sentuh.
>
> **Dipilih: Opsi A (Rekomendasi).**

> **Keputusan 2: Modal Pengajuan dengan Bukti Terintegrasi (`AC 3`)**
> - **A. COMPOSE modal pengajuan memanfaatkan `TransactionProofUploadField` standar Finance — Rekomendasi.** Menjamin keseragaman penanganan berkas bukti pembayaran/laporan di seluruh modul Finance dan memenuhi kontrak backend `ProofId`.
> - **B. Input teks URL bukti manual.** Berisiko tinggi terjadi broken link dan melanggar invariant penyimpanan bukti resmi rumah sakit.
>
> **Dipilih: Opsi A (Rekomendasi).**

---

## 3. Berkas yang Dibuat & Dimodifikasi

### 3.1 Berkas Baru
1. `src/lib/hooks/finance/payroll-deduction/use-payroll-deduction-detail.js`:
   - Custom hook pengelola state detail batch, verifikasi maker-checker (`isSubmitter`), hak akses (`canApprove`, `canApproveCorrection`), serta penanganan 4 modal aksi transisi status.
2. `src/components/view/finance/payroll-deduction/detail/payroll-deduction-detail-columns.jsx`:
   - Definisi kolom tabel baris item (dengan penanda baris tidak valid dan pesan backend `validationMessage`) serta tabel angsuran terjadwal yang belum tercakup di berkas.
3. `src/components/view/finance/payroll-deduction/detail/payroll-deduction-action-modals.jsx`:
   - 4 modal dialog: `SubmitDeductionBatchModal`, `ApproveDeductionBatchModal`, `RejectDeductionBatchModal`, dan `CancelDeductionBatchModal`.
4. `src/components/view/finance/payroll-deduction/detail/payroll-deduction-detail-view.jsx`:
   - Komponen tampilan utama layar `FIN-LYR-PG-02` merangkai Hero, Banner Notice Maker-Checker, Ringkasan Laporan Penggajian HR, Tabel Baris Item, Panel Angsuran yang Belum Ada, dan ToastStack.
5. `src/app/finance/employee-receivables/payroll-deduction/[id]/page.jsx`:
   - App Router dynamic page entry point untuk rute `/finance/employee-receivables/payroll-deduction/[id]`.

### 3.2 Berkas Dimodifikasi
1. `src/lib/services/finance/payroll-deduction-service.js`:
   - Menambahkan 5 metode API: `submitBatch`, `approveBatch`, `approveCorrectionBatch`, `rejectBatch`, dan `cancelBatch`.
2. `src/lib/state/slice/finance/payroll-deduction/payroll-deduction-slice.js`:
   - Menambahkan 5 async thunks aksi batch, state `actionLoading`, `actionError`, `submittedBatchIds`, reducers `clearBatchDetail`, `clearBatchActionError`, serta selectors terkait.
3. `src/style/corporate/finance/payroll-deduction/payroll-deduction.module.css`:
   - Menambahkan helper styling `.detailMetadataGrid` dan `.actionButtonGroup` berbasis token desain resmi.
4. `src/components/view/finance/payroll-deduction/payroll-deduction-table-columns.jsx`:
   - Menambahkan kolom aksi "Buka" untuk navigasi langsung ke halaman detail `[id]`.
5. `src/components/view/finance/payroll-deduction/payroll-deduction-view.jsx`:
   - Menghubungkan fungsi navigasi router `onOpenDetail` ke kolom aksi tabel daftar batch.

---

## 4. Spesifikasi Endpoint Transisi Status (Gaya Swagger)

Grup Tag: `[Tags("Corporate / Finance Management / Receivable / Payroll Deduction Bridge")]`

| Method | Path | Deskripsi | Otorisasi Hak Akses | Request Body | Response Body |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/finance-management/payroll-deduction/batches/{id}` | Mengambil rincian batch, baris item, hasil validasi, dan angsuran yang belum ada | `FinancePayrollDeductionBatch : Read` | — | `ApiResponse<DeductionBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/payroll-deduction/batches/{id}/submit` | Mengajukan batch dengan nomor laporan dan bukti penggajian HR | `FinancePayrollDeductionBatch : Submit` | `SubmitDeductionBatchRequest` | `ApiResponse<DeductionBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/payroll-deduction/batches/{id}/approve` | Mengesahkan batch REGULER dan menerapkan potongan secara atomik | `FinancePayrollDeductionBatch : Approve` | `DeductionBatchTransitionRequest` | `ApiResponse<DeductionBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/payroll-deduction/batches/{id}/approve-correction` | Mengesahkan batch KOREKSI oleh pejabat berwenang | `FinancePayrollDeductionBatch : ApproveCorrection` | `DeductionBatchTransitionRequest` | `ApiResponse<DeductionBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/payroll-deduction/batches/{id}/reject` | Menolak pengajuan batch dengan alasan wajib | `FinancePayrollDeductionBatch : Reject` | `RejectDeductionBatchRequest` | `ApiResponse<DeductionBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/payroll-deduction/batches/{id}/cancel` | Membatalkan batch berstatus DRAFT atau DIAJUKAN | `FinancePayrollDeductionBatch : Cancel` | `CancelDeductionBatchRequest` | `ApiResponse<DeductionBatchResponse>` |

---

## 5. Bukti Kesesuaian Kriteria Penerimaan (Acceptance Criteria)

| Kriteria Penerimaan (AC) | Implementasi & Lokasi Bukti | Status |
|---|---|---|
| **AC 1**: Tombol Sahkan tidak tampil bagi pengaju (maker-checker separation: `FIN-DEC-220`). | `use-payroll-deduction-detail.js` line 79-99: `isSubmitter` mendeteksi ID user pengaju. `canShowApprove` mengecek `!isSubmitter`. `payroll-deduction-detail-view.jsx` menyembunyikan tombol Sahkan dan menampilkan banner tata kelola. | ✅ **Lolos** |
| **AC 2**: Baris tidak valid ditandai dengan pesan backend. | `payroll-deduction-detail-columns.jsx` line 160-192: Baris `validationStatus !== 'VALID'` menampilkan badge merah "TIDAK VALID" dan teks galat orisinal backend (`item.validationMessage`). | ✅ **Lolos** |
| **AC 3**: Ajukan tanpa bukti menampilkan pesan backend (atau validasi nomor/bukti). | `payroll-deduction-action-modals.jsx` line 133-145: `SubmitDeductionBatchModal` mewajibkan bukti via `TransactionProofUploadField`. Validasi menolak pengajuan tanpa berkas bukti dengan pesan informatif. | ✅ **Lolos** |
| **AC 4**: Batch `KOREKSI` hanya dapat disahkan pemegang wewenang `FinancePayrollDeductionBatch : ApproveCorrection` (`FIN-PERM-1.12`). | `use-payroll-deduction-detail.js` line 91-98: Untuk `isKoreksi`, tombol Sahkan Koreksi HANYA aktif bila `canApproveCorrection` bernilai benar, dan memicu endpoint `/approve-correction`. | ✅ **Lolos** |

---

## 6. Verifikasi & Uji Bebas Regresi

1. **Aturan Larangan Eksekusi Server Pengembang**:
   - Server pengembang (`npm run dev` / `npm start`) **TIDAK DIJALANKAN** sesuai instruksi pengguna.
2. **Kepatuhan Token Desain Quilvian**:
   - Zero nilai hex langsung (`#hex`), zero `!important`, zero penggunaan elemen mentah `<button>`. Seluruh styling merujuk token CSS variabel resmi.
3. **Penyelarasan Antarmuka & Zero Calculation di Klien**:
   - Seluruh nominal uang, perhitungan angsuran, total kontrol, dan status validasi dihitung oleh backend (`BE-FIN-117`, `BE-FIN-118`).
4. **Pemeriksaan Linter**:
   - `npx eslint` dijalankan pada seluruh berkas baru dan modifikasi: **0 error, 0 warning (PASS)**.
