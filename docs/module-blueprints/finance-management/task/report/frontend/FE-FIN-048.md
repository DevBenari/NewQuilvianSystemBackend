# Laporan Perubahan Frontend — `FE-FIN-048`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-048` |
| Judul | HR: status bebas tanggungan pada exit clearance |
| Slice | Slice C — Exit Clearance & Separation (`REV-19`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` |
| Trace | `FR-FIN-206`..`210`, `FIN-DEC-170`, `193` |
| Kontrak | `FIN-API-1.9` O.4; `FIN-INTEGRATION-1.8` §`P.4` |
| Wewenang UI | `03-frontend-architecture.md` §24.1, §24.2, §24.4, §24.6 (`DEV_DISCRETION` untuk rute, komposisi, dan tata letak layar Exit Clearance HR) |
| Dependency | `BE-FIN-112` ✅ |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (constants, service, slice, store, hooks, css module, finance status card, table columns, list view, detail view, router pages, sidebar menu) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ **Selesai** |

---

## 1. Ringkasan Implementasi

Task `FE-FIN-048` mengimplementasikan antarmuka Petugas HR untuk mengelola verifikasi pemberhentian kerja (*exit clearance*) karyawan dan menegakkan gerbang status bebas tanggungan piutang Finance (`BE-FIN-112`, `FIN-DEC-170`, `FIN-DEC-193`):

### 1.1 `FE-FIN-048-A` — Panel Status Bebas Tanggungan Finance pada Exit Clearance HR
1. **Lencana Status Bebas Tanggungan (Hijau / Merah)**:
   - Jika pegawai masih memiliki saldo piutang aktif di Finance (`isFinanceCleared === false` atau `outstandingAmount > 0`), lencana merah **"Memiliki Tanggungan Piutang RS"** ditampilkan (`AC 1`).
   - Jika saldo piutang pegawai telah lunas (`isFinanceCleared === true` dan `outstandingAmount === 0`), lencana hijau **"Bebas Tanggungan"** ditampilkan (`AC 2`).
2. **Rincian Piutang yang Menahan**:
   - Menampilkan angka rupiah sisa piutang aktif (misal `Rp 1.500.000,00`) bersumber langsung dari backend tanpa perhitungan di klien.
   - Menampilkan jumlah kartu piutang aktif (misal `1 kartu aktif`).
   - Menampilkan tanggal jatuh tempo tertua dan waktu pemeriksaan terakhir.
3. **Gerbang Penguncian Tombol Penyelesaian (`AC 1`)**:
   - Tombol **"Sahkan & Selesaikan Exit Clearance"** dinonaktifkan (*disabled*) secara ketat ketika pegawai masih memiliki piutang aktif di Finance.
   - Banner peringatan bahaya (*danger alert*) memberitahukan bahwa clearance terkunci dan menuntut pelunasan piutang di kasir/Finance sebelum berkas dapat disahkan.
4. **Pemeriksaan Ulang Status Live Finance (`AC 2`)**:
   - Disediakan tombol interaktif **"Periksa Ulang Finance"** yang memicu `POST /api/v1/corporate/human-resource/lifecycle-management/exit-clearances/{id}/verify-finance`.
   - Setelah pegawai melunasi piutangnya di Finance, klik tombol ini akan memperbarui status secara live tanpa reload halaman: lencana berubah hijau, banner sukses muncul, dan tombol penyelesaian otomatis aktif terbuka.
5. **Peniadaan Kontrol Input Manual (`AC 3`, `FIN-DEC-193`)**:
   - Pada modal pembaruan checklist departemen (`UpdateExitClearanceChecklist`), bidang `IsFinanceCleared` **sengaja tidak ada** dan tidak dapat diedit tangan. Nilai status bebas tanggungan murni dan authoritative divalidasi dari modul Finance.

### 1.2 Layar Lengkap Exit Clearance HR
1. **Halaman Daftar (`/hr/exit-clearance`)**:
   - Header `Hero` dengan judul dan deskripsi modul.
   - `SummaryCards` metrik status: Total Exit Clearance, Dalam Proses, Selesai, Tertahan Piutang Finance, dan Dibatalkan.
   - `DataFilter` dengan pencarian debounced dan dua filter dropdown: Status Clearance dan Status Bebas Tanggungan Finance.
   - `DataTable` 10 kolom dengan paginasi server-side dan navigasi double-click ke halaman detail.
2. **Halaman Detail (`/hr/exit-clearance/[id]`)**:
   - Header `Hero` dengan breadcrumb, badge status, dan tombol navigasi/aksi.
   - Kartu Informasi Data Pegawai & Berkas Pemisahan (NIP, nama, unit, jabatan, no. pemisahan, tanggal mulai, batas waktu, pengesah).
   - Indikator bar visual progres penyelesaian checklist.
   - Panel khusus **Gerbang Bebas Tanggungan Finance (`FE-FIN-048-A`)**.
   - Panel Checklist Departemen & Aspek Lain (Departemen, Aset Fasilitas, Payroll, Akses IT, Dokumen HR) beserta riwayat catatan.
   - Modal pembaruan checklist departemen (non-finance).
   - Modal konfirmasi pengesahan & penyelesaian (hanya dapat dibuka bila lolos gerbang Finance).
   - Modal pembatalan berkas clearance dengan alasan wajib.
3. **Pendaftaran Menu Sidebar**:
   - Mendaftarkan butir menu "Exit Clearance Karyawan" pada grup Human Resource di `src/utils/menu-sidebar/menu-items.jsx`.

---

## 2. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 14 elemen — REUSE 10, EXTEND 0, COMPOSE 4, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Header halaman & breadcrumb | `Hero` | `src/components/features/base-features/hero.jsx` | `REUSE` | Digunakan pada halaman list dan detail |
| Metrik ringkasan status | `SummaryGrid` | `src/components/features/base-features/summary-grid.jsx` | `REUSE` | Menampilkan 5 kartu ringkasan status dari backend |
| Filter pencarian & status | `DataFilter`, `FilterSelect` | `src/components/features/base-features/data-filter.jsx` | `REUSE` | Pencarian teks, filter status clearance, dan filter bebas tanggungan |
| Tabel daftar clearance | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | 10 kolom data clearance dengan format token Quilvian |
| Lencana status clearance & finance | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Pemetaan varian success, warning, danger, neutral |
| Paginasi tabel | `Pagination` | `src/components/features/base-features/pagination.jsx` | `REUSE` | Paginasi terhubung ke state Redux |
| Tombol aksi interaktif | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Tombol verify finance, update checklist, complete, cancel |
| Alert peringatan gerbang terkunci / terbuka | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Tampilan alert bahaya saat berpiutang dan sukses saat lunas |
| Toast notifikasi umpan balik | `ToastStack` | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Notifikasi status live check, update checklist, dan pengesahan |
| Modal konfirmasi pengesahan selesai | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | Dialog pengesahan dengan catatan opsional |
| Panel Gerbang Bebas Tanggungan Finance (`FE-FIN-048-A`) | `ExitClearanceFinanceStatusCard` | Scoped CSS Module berbasis token desain Quilvian | `COMPOSE` | Merangkai StatusBadge, InformationAlert, BaseButton, dan grid rincian piutang |
| Modal pembaruan checklist departemen | `ConfirmModal` + form controls | `src/components/features/base-features/confirm-modal.jsx` | `COMPOSE` | Checklist aspek departemen non-finance dengan notice invariant |
| Modal pembatalan clearance | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | Dialog pembatalan dengan validasi alasan wajib |
| Visual bar progres penyelesaian | CSS Module progress bar | `src/style/hr/exit-clearance/exit-clearance.module.css` | `COMPOSE` | Bar persentase aspek yang selesai |

### Pilihan Bernomor untuk Elemen COMPOSE:

> **Keputusan 1: Komposisi Panel Gerbang Bebas Tanggungan Finance (`FE-FIN-048-A`)**
> - **A. COMPOSE kartu status terdedikasi (`ExitClearanceFinanceStatusCard`) dengan indikator kunci visual, detail nominal piutang, dan tombol periksa ulang live — Rekomendasi.** Menegaskan posisi Finance sebagai gerbang independen yang tidak dapat dimanipulasi di layar HR, sekaligus memberikan akses pemeriksaan live yang jelas bagi petugas.
> - **B. Hanya satu baris checkbox di dalam tabel checklist.** Mengaburkan fakta bahwa status Finance tidak dapat diedit manual dan membingungkan petugas HR mengapa baris tersebut tidak dapat dicentang.
>
> **Dipilih: Opsi A (Rekomendasi).**

> **Keputusan 2: Modal Pembaruan Checklist Aspek Departemen**
> - **A. COMPOSE modal checklist terisolasi dengan checkbox untuk 5 aspek non-finance dan banner invariant sistem (AC 3) — Rekomendasi.** Memastikan aspek operasional departemen dapat diperbarui tanpa menyentuh status Finance, membuktikan kepatuhan terhadap AC 3.
> - **B. Form inline yang mengizinkan edit langsung di tabel.** Meningkatkan risiko modifikasi tanpa sengaja dan tidak memberikan ruang untuk catatan serah terima.
>
> **Dipilih: Opsi A (Rekomendasi).**

---

## 3. Berkas yang Dibuat dan Diubah

### Berkas Baru
1. `src/lib/constants/hr/exit-clearance/exit-clearance-constants.js` — Konfigurasi endpoint, opsi status, badge map status clearance dan finance clearance, serta daftar departemen checklist.
2. `src/lib/services/hr/exit-clearance-service.js` — Service panggilan HTTP Axios (`InstanceAxios`) untuk operasi CRUD, verifikasi live Finance, checklist update, pengesahan, dan pembatalan.
3. `src/lib/state/slice/hr/exit-clearance/exit-clearance-slice.js` — Redux Toolkit slice dan async thunks untuk seluruh endpoint `BE-FIN-112`.
4. `src/lib/hooks/hr/exit-clearance/use-exit-clearance-list.js` — Custom hook untuk daftar exit clearance, pencarian debounced, filter status, dan metrik summary.
5. `src/lib/hooks/hr/exit-clearance/use-exit-clearance-detail.js` — Custom hook untuk detail exit clearance, pemeriksaan live Finance, pembaruan checklist, dan penguncian penyelesaian.
6. `src/style/hr/exit-clearance/exit-clearance.module.css` — Stylesheet CSS Module berbasis token desain Quilvian untuk panel gerbang Finance, info grid, dan checklist card.
7. `src/components/view/hr/exit-clearance/exit-clearance-finance-status-card.jsx` — Komponen panel gerbang bebas tanggungan Finance (`FE-FIN-048-A`).
8. `src/components/view/hr/exit-clearance/exit-clearance-table-columns.jsx` — Definisi kolom tabel daftar exit clearance.
9. `src/components/view/hr/exit-clearance/exit-clearance-list-view.jsx` — Tampilan halaman list exit clearance HR.
10. `src/components/view/hr/exit-clearance/exit-clearance-detail-view.jsx` — Tampilan halaman detail exit clearance HR.
11. `src/app/hr/exit-clearance/page.jsx` — App Router entry point untuk daftar exit clearance.
12. `src/app/hr/exit-clearance/[id]/page.jsx` — App Router entry point untuk detail exit clearance.

### Berkas yang Diubah
1. `src/lib/state/store.jsx` — Pendaftaran reducer `exitClearance` ke store Redux utama.
2. `src/utils/menu-sidebar/menu-items.jsx` — Pendaftaran butir menu "Exit Clearance Karyawan" (`/hr/exit-clearance`) di bawah grup Human Resource.

---

## 4. Pemenuhan Acceptance Criteria

| # | Acceptance Criteria | Bukti Implementasi |
|:--:|---|---|
| 1 | Pegawai bersisa Rp 1.500.000: lencana merah dan tombol selesai terkunci | Pada `ExitClearanceFinanceStatusCard` dan `ExitClearanceDetailView`, saat `outstandingAmount > 0` (misal Rp 1.500.000) atau `isFinanceCleared === false`, lencana merah "Memiliki Tanggungan Piutang RS" ditampilkan, banner peringatan bahaya aktif, dan tombol `[Sahkan & Selesaikan Exit Clearance]` dinonaktifkan (*disabled*) secara keras dengan tooltip & banner peringatan terkunci. |
| 2 | Setelah lunas dan diperiksa ulang: lencana hijau dan tombol aktif | Petugas mengklik tombol `[Periksa Ulang Finance]`, thunk `verifyExitClearanceFinance` memanggil backend `POST /verify-finance`. Saat saldo menjadi Rp 0 dan `isFinanceCleared === true`, status disinkronkan secara live: lencana berubah menjadi hijau "Bebas Tanggungan", banner sukses muncul, dan tombol `[Sahkan & Selesaikan Exit Clearance]` otomatis aktif terbuka. |
| 3 | Tidak ada kontrol untuk mengubah status secara manual | Pada `UpdateExitClearanceChecklistRequest` dan form modal `ConfirmModal` ubah checklist di `ExitClearanceDetailView`, properti `IsFinanceCleared` sengaja dihilangkan. Hanya 5 aspek non-finance (Departemen, Aset, Payroll, IT, HR) yang dapat diubah; status Finance murni dan authoritative divalidasi langsung ke backend Finance. |
| 4 | Verifikasi Lint & Anti-Regresi | Perintah `npx eslint` pada seluruh 14 berkas target menghasilkan **PASS (0 error, 0 warning)**. Grep anti-regresi membuktikan **nol `!important`**, **nol warna heksadesimal literal**, dan **nol tag `<button>` mentah**. |

---

## 5. Kesimpulan & Status

Seluruh fungsionalitas task `FE-FIN-048` telah selesai diimplementasikan secara komprehensif sesuai kontrak `BE-FIN-112`, `FIN-API-1.9` O.4, `FIN-INTEGRATION-1.8` §`P.4`, serta keputusan arsitektur `FIN-DEC-170` dan `FIN-DEC-193`.

Status task ditandai: ✅ **Selesai**.
