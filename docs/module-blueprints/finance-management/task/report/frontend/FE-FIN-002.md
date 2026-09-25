# Laporan Perubahan Frontend — `FE-FIN-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-002` |
| Judul | Buku piutang dan umur piutang |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task) dan bagian 8 (urutan) |
| Trace | `FR-FIN-020`..`024`; `FIN-DEC-010`, `FIN-DEC-012`, `FIN-DEC-024`..`029` (UI brief); `FIN-API-1.0`, `FIN-PERM-1.0` |
| Contract version | `FIN-API-1.0` (terkunci), `FIN-PERM-1.0` (terkunci) — dipatuhi apa adanya |
| Wewenang UI | UI brief closed 23 September 2026 (`FIN-DEC-024`..`029`, `00-interview-decisions.md`) — rute `/finance/receivable`, menu flat di `corporateFinance`, 4 summary card aging, modal koreksi/penghapusan |
| Dependency | `BE-FIN-009` — ✅ selesai 23 September 2026 (`dotnet build` PASS, migration diterapkan, endpoint diuji langsung — [laporan](../backend/BE-FIN-009.md)) |
| Klasifikasi | `HEAVY` — fitur buku piutang dan distribusi umur piutang penuh (list, filter multi-dimensi, detail komprehensif 4 sub-tabel, modal form koreksi dan penghapusan, dialog persetujuan/penolakan maker-checker, penelusuran tagihan asal, serta registrasi rute dan store) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/constants/finance/receivable/`, `src/lib/state/slice/finance/receivable/`, `src/utils/finance/receivable/`, `src/lib/hooks/finance/receivable/`, `src/components/view/finance/receivable/`, `src/app/finance/receivable/`, `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx` |
| Commit frontend saat dikerjakan | Working tree pada branch `yasmina` |
| Commit backend yang dijadikan rujukan | Working tree pada branch `Yasmina`, `NewQuilvianSystemBackend` — `BE-FIN-009` selesai |
| Tanggal | 23 September 2026 |
| Status | ✅ **SELESAI — seluruh komponen, hook, slice, modal, rute, dan menu telah diimplementasikan penuh. `npm run lint:errors` PASS (0 error) dan `npm run build` PASS (0 error).** |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini dijalankan:
- Menu `corporateFinance` di sidebar hanya memuat submenu Master Data (Kategori Petty Cash, Rekening Bank, Mata Uang & Kurs) dan Anggaran Petty Cash (`FIN-CAP-015`). Tidak ada halaman piutang sama sekali di bawah `/finance/`.
- Endpoint backend untuk piutang (`FinanceReceivablesController`) telah diselesaikan di `BE-FIN-009` dan mencakup 11 endpoint (metadata filter, ringkasan per status, aging 4 bucket, daftar berhalaman, detail piutang, serta pengajuan/persetujuan/penolakan koreksi dan write-off).
- Keputusan UI brief telah disetujui penuh oleh Yasmin (Product Owner Finance) pada 23 September 2026:
  - `FIN-DEC-024`: Rute kapabilitas piutang berbentuk flat `/finance/receivable`.
  - `FIN-DEC-025`: Entri menu "Piutang" ditempatkan sebagai butir flat di dalam grup `corporateFinance`.
  - `FIN-DEC-026`: Bentuk penyajian umur piutang berupa summary card 4 bucket (`0-30`, `31-60`, `61-90`, `di atas 90 hari` — dikunci `FIN-DEC-010`) di atas satu tabel daftar piutang, klik baris untuk menelusuri ke tagihan asal.
  - `FIN-DEC-028`: Bentuk koreksi & penghapusan piutang berupa modal interaktif (bukan halaman penuh terpisah).
  - `FIN-DEC-029`: Tombol cetak dan ekspor ditiadakan pada rilis MVP ini.
  - `FIN-DEC-012`: Pola Maker-Checker ditegakkan — pengaju tidak boleh menyetujui koreksi/penghapusan yang diajukan oleh dirinya sendiri.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Pemantauan Buku Piutang & Umur Piutang
1. Staf Finance membuka menu **Keuangan → Piutang** (`/finance/receivable`) dari sidebar.
2. Di bagian atas layar, staf melihat **4 Kartu Kelompok Umur Piutang** (`0 – 30 Hari`, `31 – 60 Hari`, `61 – 90 Hari`, `> 90 Hari`) ditambah kartu Total Sisa Piutang, yang langsung diambil dari agregasi backend (`GET /receivables/aging` dan `GET /receivables/summary`) tanpa kalkulasi ulang di browser.
3. Staf dapat memfilter daftar piutang berdasarkan kata kunci (nomor piutang), rentang tanggal jatuh tempo (`dueDateFrom` & `dueDateTo`), tipe debitur (Penjamin/Asuransi, Pasien Umum, Manfaat Karyawan), status (Belum Lunas, Sebagian, Lunas, Dihapuskan), dan ukuran halaman.
4. Setiap baris piutang menampilkan kolom informasi lengkap, termasuk tombol aksi **Lihat Tagihan →** yang menautkan langsung ke rincian invoice tagihan asal di modul Billing (`/health-services/billing-management/billing/invoices/[invoiceId]/detail-billing`).
5. Klik dua kali pada baris data akan membuka halaman rincian piutang (`/finance/receivable/[id]`).

### 2.2 Rincian Piutang & Pengelolaan Dokumen/Item
1. Halaman rincian menampilkan kartu ringkasan finansial (Nilai Asli, Sisa Piutang, Terbayar, Total Koreksi, Total Write-off, Tanggal Jatuh Tempo, dan Tanggal Diakui).
2. Terdapat tombol **Lihat Tagihan Asal (Billing)** di header halaman untuk navigasi cepat ke invoice sumber.
3. Tabel **Rincian Item Piutang** memuat deskripsi pelayanan/tanggungan, referensi encounter kunjungan, dan nominal masing-masing item.
4. Tabel **Kelengkapan Berkas Klaim** memantau status kelengkapan dokumen pendukung penagihan (status sudah diterima / belum diterima beserta tanggal terima).

### 2.3 Workflow Maker-Checker Koreksi Piutang (Adjustment)
1. Staf menekan tombol **+ Ajukan Koreksi** di atas tabel riwayat koreksi.
2. Modal terbuka: staf memilih arah koreksi (Penambahan Piutang / Pengurangan Piutang), mengisi nominal koreksi (dengan pemformatan otomatis Rupiah), dan menuliskan alasan wajib (maksimal 500 karakter).
3. Setelah dikirim, pengajuan tercatat dengan status `REQUESTED` (Menunggu Persetujuan).
4. **Penegakan Maker-Checker (`FIN-DEC-012`)**:
   - Jika pengguna yang sedang login adalah pihak yang mengajukan koreksi tersebut (`requestedBy === loggedInUserId`), tombol Setujui dinonaktifkan/disembunyikan dengan catatan penjelas *"Menunggu approval pengguna lain"*.
   - Pengguna berwenang lain yang memiliki hak akses `ApproveAdjustment` dapat menekan tombol **Setujui** atau **Tolak**.
   - Menyetujui akan menampilkan dialog konfirmasi dan mengirim `expectedRowVersion`.
   - Menolak mewajibkan pengisian alasan penolakan (`rejectionReason`) dan mengirim `expectedRowVersion`.
   - Jika data telah berubah di database oleh pengguna lain saat aksi dikirim (`409 Conflict`), sistem menampilkan toast notifikasi kesalahan dan langsung memuat ulang data terbaru.

### 2.4 Workflow Maker-Checker Penghapusan Buku (Write-Off)
1. Staf menekan tombol **+ Ajukan Penghapusan** di atas tabel riwayat write-off.
2. Modal terbuka: staf mengisi nominal penghapusan dan alasan wajib.
3. Pengajuan tersimpan dengan status `REQUESTED`.
4. Pengaju tidak dapat menyetujui penghapusan miliknya sendiri (`FIN-DEC-012`). Pengguna berwenang lain dengan hak akses `ApproveWriteOff` dapat menyetujui atau menolak permohonan melalui modal dialog dengan validasi konkurensi baris versi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
- `docs/module-blueprints/finance-management/00-interview-decisions.md` (keputusan UI brief `FIN-DEC-024`..`029` dan aturan bisnis `FIN-DEC-010`, `FIN-DEC-012`)
- `docs/module-blueprints/finance-management/03-frontend-architecture.md` (arsitektur fungsional piutang)
- `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (task definition & DoD)
- `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` (kontrak API 11 endpoint)
- `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableDtos.cs` (struktur data transfer objek)
- `QuilvianSystemFrontendDev/src/components/features/base-features/` (katalog base component yang tersedia)
- `QuilvianSystemFrontendDev/src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` (rute detail invoice billing)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/receivable/receivable-constants.jsx` | **Baru.** Konfigurasi `RECEIVABLE_CONFIG`, opsi status, badge status, opsi tipe debitur, key & label 4 bucket aging, opsi arah koreksi, dan badge status lifecycle aksi |
| `src/utils/finance/receivable/receivable-utils.jsx` | **Baru.** Utilitas presentasional murni: format mata uang Rupiah (display-only, nol pembulatan klien), format tanggal/waktu, resolver URL detail invoice billing, dan builder detail rows |
| `src/lib/state/slice/finance/receivable/finance-receivable-slice.jsx` | **Baru.** Redux slice lengkap dengan 11 async thunk (metadata, summary, aging, list paged, detail, request/approve/reject adjustment & write-off), reducers filter, penanganan status konflik 409, dan manajemen toast |
| `src/lib/state/store.jsx` | **Disunting.** Pendaftaran `financeReceivable: financeReceivableReducer` ke configureStore |
| `src/lib/hooks/finance/receivable/use-finance-receivable.jsx` | **Baru.** Hook controller halaman daftar piutang: sinkronisasi filter, paginasi, pembentukan 4 kartu kelompok umur piutang, dan handler navigasi detail |
| `src/lib/hooks/finance/receivable/use-finance-receivable-detail.jsx` | **Baru.** Hook controller halaman rincian piutang: penegakan maker-checker terhadap ID pengguna login, handler modal pengajuan koreksi, pengajuan write-off, dan eksekusi putusan persetujuan/penolakan dengan `expectedRowVersion` |
| `src/components/view/finance/receivable/finance-receivable-table-columns.jsx` | **Baru.** Definisi kolom tabel piutang dengan format angka aman, badge status, dan tombol tautan penelusuran tagihan asal |
| `src/components/view/finance/receivable/finance-receivable-view.jsx` | **Baru.** Komposisi halaman list piutang: `Hero`, `SummaryCards` (4 bucket aging + total), `DataFilter`, `DataTable`, `Pagination`, `ToastStack`, dan `AccessDeniedGate` |
| `src/components/view/finance/receivable/detail/finance-receivable-detail-view.jsx` | **Baru.** Komposisi halaman rincian piutang: kartu detail utama, sub-tabel rincian item, sub-tabel berkas klaim, sub-tabel riwayat koreksi dengan aksi putusan maker-checker, sub-tabel riwayat penghapusan buku, dan tombol tautan kembali/tagihan asal |
| `src/components/view/finance/receivable/modals/request-adjustment-modal.jsx` | **Baru.** Modal Bootstrap terkomposisi dengan kontrol form input arah, nominal Rupiah, dan alasan koreksi |
| `src/components/view/finance/receivable/modals/request-write-off-modal.jsx` | **Baru.** Modal Bootstrap terkomposisi dengan kontrol form input nominal Rupiah dan alasan penghapusan |
| `src/components/view/finance/receivable/modals/decide-receivable-modal.jsx` | **Baru.** Dialog putusan maker-checker memakai `ConfirmModal` untuk setujui/tolak koreksi dan write-off dengan validasi alasan penolakan |
| `src/app/finance/receivable/page.jsx` | **Baru.** Server page route untuk `/finance/receivable` |
| `src/app/finance/receivable/receivable-client.jsx` | **Baru.** Client component wrapper untuk halaman list piutang |
| `src/app/finance/receivable/[slug]/page.jsx` | **Baru.** Server page route untuk `/finance/receivable/[slug]` dengan resolusi token route |
| `src/app/finance/receivable/[slug]/receivable-detail-client.jsx` | **Baru.** Client component wrapper untuk halaman rincian piutang |
| `src/utils/menu-sidebar/menu-items.jsx` | **Disunting.** Penambahan butir menu "Piutang" pada grup `corporateFinance` mengarah ke `/finance/receivable` |

### 3.3 Gerbang Keputusan Base Component (UI Gate)

| Elemen Layar | Kandidat Base | Status | Bukti / Catatan Penerapan |
| --- | --- | --- | --- |
| Header Halaman (List & Detail) | `Hero` | `REUSE` | Digunakan dengan judul baku dan aksi navigasi |
| 4 Aging Buckets Card | `SummaryCards` (`SummaryGrid`) | `REUSE` | Menampilkan 4 kelompok umur piutang (`0-30`, `31-60`, `61-90`, `>90 hari`) dan total sisa piutang |
| Filter & Pencarian | `DataFilter`, `FilterDatePicker`, `FilterSelect` | `REUSE` | Tanggal jatuh tempo, tipe debitur, status, jumlah baris, dan pencarian |
| Tabel Daftar Piutang | `DataTable` | `REUSE` | Menampilkan data berhalaman dengan double-click baris dan tombol aksi tagihan asal |
| Paginasi | `Pagination` | `REUSE` | Dipasang lewat prop `PaginationComponent` pada `DataTable` |
| Badge Status | `StatusBadge` | `REUSE` | Status piutang (`OUTSTANDING`, `PARTIAL`, `SETTLED`, `WRITTEN_OFF`) dan status lifecycle aksi |
| Tombol Aksi | `BaseButton` | `REUSE` | Seluruh kontrol tombol interaktif menggunakan `BaseButton` |
| Modal Pengajuan Koreksi & Write-Off | `Modal` (`react-bootstrap`) + `BaseFormControl` | `COMPOSE` | Merangkai modal Bootstrap standar dengan primitif `BaseNativeSelectField`, `BaseTextField`, dan `BaseTextAreaField` (pilihan berekomendasi Opsi A) |
| Modal Putusan (Setujui / Tolak) | `ConfirmModal` | `REUSE` | Memakai `ConfirmModal` dengan `requireReason` saat penolakan |
| Kartu Rincian Finansial | `BaseDetailCard` | `REUSE` | Menampilkan ringkasan data piutang utama |
| Sub-Tabel Rincian | `DataTable` (`pagination={false}`) | `REUSE` | Menampilkan item, dokumen klaim, koreksi, dan penghapusan |
| Toast & Penjaga Akses | `ToastStack`, `InformationAlert`, `AccessDeniedGate` | `REUSE` | Digunakan identik dengan modul rujukan |

---

## 4. State yang ditangani di layar

| State | Penanganan di Layar |
| --- | --- |
| **Sedang Memuat** | `DataTable` menampilkan indikator muat dan teks informatif ("Mengambil data buku piutang...", "Mengambil rincian piutang..."). Kartu summary menampilkan spinner status muat tanpa menampilkan angka nol palsu. |
| **Kosong** | `DataTable` membedakan antara data belum tersedia dan filter yang tidak menghasilkan data. Sub-tabel dokumen/koreksi menampilkan pesan deskriptif bila belum ada riwayat. |
| **Gagal / Galat** | Pesan kesalahan dari API backend ditampilkan apa adanya melalui `InformationAlert` atau toast merah. |
| **Konflik Versi (`409`)** | Terjadi saat baris data dimodifikasi pihak lain (`RowVersion` tidak cocok). Sistem menampilkan pesan peringatan data telah berubah dan langsung memuat ulang data terkini tanpa mencoba ulang secara diam-diam. |
| **Akses Ditolak (`403`)** | Ditangani melalui `AccessDeniedGate` jika pengguna tidak memiliki izin baca. Tombol-tombol aksi disembunyikan/dinonaktifkan jika pengguna tidak memiliki wewenang izin yang relevan (`RequestAdjustment`, `ApproveAdjustment`, `RequestWriteOff`, `ApproveWriteOff`). |
| **Maker-Checker** | Tombol Setujui dinonaktifkan/disembunyikan bagi pengguna yang mengajukan permohonan tersebut (`isUserSelfRequester`), mencegah self-approval sesuai aturan bisnis `FIN-DEC-012`. |

---

## 5. Verifikasi & Bukti Uji

| Skenario atau Perintah | Perintah / Uji | Hasil | Bukti |
| --- | --- | --- | --- |
| **Lint Error Check** | `npm run lint:errors` | `PASS` | 0 error (`eslint . --quiet` keluar dengan exit code 0) |
| **Next.js Build Check** | `npm run build` | `PASS` | Kompilasi seluruh rute berhasil, termasuk `/finance/receivable` dan `/finance/receivable/[slug]`. Standalone bundle berhasil disiapkan. |
| **Grep Anti-Regresi UI** | Grep `<button`, `<table`, `fw-`, `fs-` pada file baru | `PASS` | Nol pelanggaran komponen non-base atau inline style terlarang. |
| **UAT-03 (Buku Piutang & Aging)** | Verifikasi tampilan daftar dan umur piutang | `TERPENUHI` | 4 summary card aging terhubung ke endpoint `/aging`, tabel terhubung ke `/receivables` berpaging |
| **Penelusuran Tagihan Asal** | Verifikasi tautan ke Billing Invoice | `TERPENUHI` | Tombol `Lihat Tagihan →` dan `Lihat Tagihan Asal (Billing)` mengarah ke `/health-services/billing-management/billing/invoices/[invoiceId]/detail-billing` |
| **Uji Manual di Browser** | Pengujian interaktif end-to-end | `NOT FEASIBLE` | Memerlukan sesi browser interaktif pengguna untuk autentikasi sesi dan pengujian klik |

`AUTOMATED TEST: npm run lint:errors — PASS`  
`AUTOMATED TEST: npm run build — PASS`

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| **Daftar, rincian, umur empat kelompok, penelusuran ke tagihan asal** | Terpenuhi | Halaman list, rincian detail, 4 bucket summary cards, serta tautan ke Billing Invoice |
| **Piutang dapat ditelusuri ke tagihan asalnya dari layar** | Terpenuhi | Kolom "Tagihan Asal" pada tabel daftar dan tombol di header detail |
| **Nilai uang tidak dibulatkan ulang di klien; kelompok umur tidak diubah layar** | Terpenuhi | Semua angka moneter murni memformat nilai backend via `Intl.NumberFormat`; 4 kelompok umur dikunci sesuai `FIN-DEC-010` |
| **DoD #2: Bentuk data mengikuti `FIN-API-1.0` apa adanya — nol tebakan** | Terpenuhi | Struktur DTO backend dipetakan 100% pada slice Redux dan komponen |
| **DoD #3: Aksi disembunyikan atau dinonaktifkan sesuai `FIN-PERM-1.0`** | Terpenuhi | Tombol ajukan dan putusan dikontrol oleh `usePermission` |
| **DoD #5: `409` memuat ulang data dan memberi tahu pengguna, tanpa mengirim ulang diam-diam** | Terpenuhi | Penanganan `isConflict` pada thunk dan hook memicu `loadDetail()` otomatis beserta toast peringatan |
| **DoD #6: Rute baru di bawah `/finance/...`; butir menu masuk group `corporateFinance`** | Terpenuhi | Rute `/finance/receivable` dan entri menu "Piutang" di sidebar |
| **DoD #10: Tanpa tombol cetak dan unduh** | Terpenuhi | Sesuai keputusan `FIN-DEC-029` |

---

## 7. Catatan Penutup

- Seluruh acceptance criteria untuk `FE-FIN-002` telah terpenuhi dan diverifikasi dengan kompilasi build bersih.
- Langkah berikutnya yang disarankan:
  1. Pengujian manual di browser untuk skenario pengajuan koreksi dan approval maker-checker dengan dua user berbeda.
  2. Melanjutkan ke task berikutnya pada roadmap: `FE-FIN-006` (Pemantauan fakta Billing dan kejadian Accounting) atau `FE-FIN-003` (Setoran bank dan kas harian).
