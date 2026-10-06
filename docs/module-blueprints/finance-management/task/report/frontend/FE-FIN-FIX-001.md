# Laporan Perubahan Frontend — `FE-FIN-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-FIX-001` (ad-hoc, di luar roadmap, permintaan langsung pengguna) |
| Judul | Halaman Finance › Transaksi A/R › Tagihan/Billing diubah menjadi **Data Tagihan**: tiga tab (Perusahaan/Asuransi, Karyawan, Pasien Umum), filter, ringkasan, tabel, serta aksi Buat AR/Invoice, Print Data, dan Export Excel |
| Slice | Pasangan frontend dari `BE-FIN-FIX-001`. Memakai pola `FIX` modul lain (`FE-BKC-FIX-XXX`); modul Finance belum punya penomoran `FIX` |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap resmi, sehingga tidak ada tanda status yang dipasang pada roadmap |
| Trace | `FIN-DEC-006` (EMPLOYEE_BENEFIT belum tuntas), `FIN-DEC-048` (batch hanya untuk penjamin), `BE-FIN-FIX-001`, `Keuangan.md` bagian AR › Data Tagihan/Billing |
| Contract version | Mengonsumsi dua endpoint baca baru dari `BE-FIN-FIX-001` (**belum dikompilasi**, lihat bagian 8) dan `POST receivable-invoice-batches` yang sudah ada dengan validasi lebih ketat dari task yang sama. Tidak ada kontrak bertversi yang disetujui |
| Wewenang UI | Perintah pengguna pada sesi ini (judul, tata letak dua kolom, tab, filter, kartu ringkasan, kolom tabel, tiga tombol aksi). Batas: komponen dasar dipakai ulang, tanpa dependency baru, tanpa komponen dasar baru |
| Dependency | `BE-FIN-FIX-001` — kode ditulis, `dotnet build` **NOT RUN**, belum di-commit. Tanpa itu halaman ini menampilkan pesan gagal memuat |
| Klasifikasi | `HEAVY` — skor 9: repository tulis 0; berkas diperiksa >20 → 2; berkas diubah 14 → 2; logika (state filter, ambil-semua-halaman, rencana Buat AR) → 2; kontrak API (memakai endpoint baru) → 1; database 0; keamanan/auth 0; UI/workflow (halaman Data Tagihan, relokasi Daftar Batch, satu komponen bersama dipakai ulang) → 2 |
| Task mode | `FRONTEND`. Frontend V1 dan backend V1 dibaca sebagai rujukan, tidak diubah |
| Target tulis | `QuilvianSystemFrontendDev` saja, ditambah berkas laporan ini di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `0ed37b5c43e9cc7204cc5c6e52bc326904c7cdcf` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `46fa2a91f8c812d1d1c5e83a94aed3dddfccac93` (kode `BE-FIN-FIX-001` berada di working tree backend, belum di-commit) |
| Tanggal | 2026-10-05 |
| Status | 🟡 **KODE SELESAI DITULIS, VALIDASI BELUM DIJALANKAN.** `npm run lint`, `npm run build`, dan `npm run test:unit` **NOT RUN**; uji manual di browser **NOT FEASIBLE** (backend belum dikompilasi). Belum boleh ditandai selesai (`✅`) |

---

## 1. Keadaan yang ditemukan di awal

- Rute `/finance/receivable-invoice-batches` (menu **Tagihan/Billing**) menampilkan daftar **Tagihan Gabungan Penjamin** (Batch Tagihan). Tidak ada halaman "Data Tagihan" seperti di V1.
- V1 (`TabelTagihanBilling.jsx`) meminta hingga 1.000 baris lalu menyaring di browser, memakai nama penjamin "RS Benefit" yang ditulis langsung di kode untuk tab Karyawan, dan batas hari memakai UTC. Pendekatan itu tidak dibawa ke V2.
- Backend V2 sebelumnya hanya punya daftar piutang layak ditagih per satu penjamin. Endpoint daftar tagihan lintas kategori baru ditulis pada `BE-FIN-FIX-001`.
- `FilterDatePicker` hanya menampilkan tanggal dalam bentuk singkat (misalnya "5 Okt 2026"), sedangkan spesifikasi meminta `dd/mm/yyyy`.
- Daftar Batch Tagihan, yang berisi batch `DRAFT` yang masih harus diterbitkan, hanya dapat dicapai dari rute yang sama.

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas Finance yang menyiapkan penagihan ke penjamin.

1. Petugas membuka **Finance › Transaksi A/R › Tagihan/Billing**. Halaman berjudul **Data Tagihan**, dengan subjudul mengikuti tab yang aktif, misalnya "Data Perusahaan/Asuransi belum proses invoice".
2. Di kolom kiri (**Filter & Pencarian**) petugas mengisi: pencarian, pilihan Perusahaan/Asuransi (atau Karyawan, atau Pasien sesuai tab), Status Tagihan, Jenis Pasien, Periode, serta Tanggal Mulai dan Tanggal Akhir. Status Tagihan awalnya **Belum Dibuat** seperti V1.
3. Petugas menekan **Cari** (atau Enter di kolom pencarian). Tabel dan kartu ringkasan dimuat memakai filter yang sama. Perubahan isian *tidak* langsung memuat ulang data; hanya **Cari** yang menerapkannya, seperti V1.
4. Kartu ringkasan menampilkan nama penjamin/pasien terpilih, **Total + Tanggal** (total tagihan sesuai semua filter), dan **Total + nama penjamin** beserta jumlah pasien.
5. Petugas dapat berpindah tab **Perusahaan/Asuransi**, **Karyawan**, atau **Pasien Umum**. Pindah tab mengembalikan seluruh filter ke nilai bawaan (Status Tagihan kembali ke "Belum Dibuat") dan memuat ulang.
6. **Buat AR/ Invoice** (hanya tab Perusahaan/Asuransi): sistem menghitung tagihan yang boleh dibuat dari seluruh hasil filter, lalu menampilkan konfirmasi berisi penjamin, jumlah tagihan, total, dan periode. Setelah **Ya, Buat AR/Invoice**, satu Batch Tagihan berstatus Draf dibuat dan petugas langsung dibawa ke halaman detail batch itu.
7. **Print Data** dan **Export Excel** memakai seluruh hasil filter (bukan hanya halaman yang tampil).
8. Tombol **Daftar Batch Tagihan** di kanan atas membuka daftar batch, yang kini ada di `/finance/receivable-invoice-batches/batches`.

**Contoh:** petugas memilih asuransi "Allianz", Periode "Bulan Ini", Status "Belum Dibuat", lalu **Cari**. Muncul 42 tagihan dari 30 pasien dengan total Rp 120.500.000. Petugas menekan **Buat AR/ Invoice**; konfirmasi menyebut "Penjamin: Allianz, Jumlah tagihan: 42". Setelah dikonfirmasi, satu batch Draf terbentuk dan status 42 tagihan itu berubah menjadi **Sudah Dibuat**.

**Contoh tanggal:** Tanggal Mulai 20/10/2026 dan Tanggal Akhir 10/10/2026 → pesan kesalahan muncul di bawah kolom tanggal dan tombol **Cari** tidak aktif.

**Jalur tidak normal**

| Kejadian | Hasil |
| --- | --- |
| Tab Karyawan | Tabel kosong beserta catatan dari backend: sumber data Karyawan belum tersedia (`FIN-DEC-006`). Tombol Buat AR/Invoice nonaktif dengan penjelasan |
| Tagihan perusahaan tanpa referensi penjamin | Baris tampil dengan alasan "belum dapat dibuatkan AR/Invoice" di kolom Status Tagihan dan dilewati saat Buat AR |
| Hasil filter lebih dari satu penjamin | Buat AR ditolak dengan pesan agar memilih satu penjamin dulu |
| Hasil filter melebihi 1.000 baris saat Print/Export/Buat AR | Aksi dihentikan dengan pesan agar filter dipersempit |
| Backend menjawab 409/422 saat Buat AR | Pesan ditampilkan dan tabel dimuat ulang supaya status terbaru terlihat |
| Pengguna tidak punya hak `FinanceReceivable : Read` | Layar "akses ditolak" |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- V1: `TabelTagihanBilling.jsx` (frontend V1) dan endpoint `/MainKasir/paged` (backend V1) — perilaku filter, urutan kolom, aturan reset, awal minggu (Minggu), penamaan tab.
- V2 frontend: modul `finance/receivable` (view, hook, konstanta, utilitas batch), `FinanceBreadcrumb`, komponen dasar (`Hero`, `FilterSelect`, `FilterDatePicker`, `BaseTextField`, `BaseButton`, `DataTable`, `ConfirmModal`, `StatusBadge`, `InformationAlert`, `ToastStack`, `AccessDeniedGate`, `Pagination`), `useSelectResource`, `date-picker-utils`, `menu-items.jsx`, `globals.css` (token).
- V2 backend: `FinanceReceivablesController`, `FinanceReceivableBillingDataService`, `FinanceReceivableBillingDataDtos`, `FinanceReceivableInvoiceBatchesController`, `PatientController` (options).
- Aturan: `AGENTS.md`/`CLAUDE.md` frontend, `frontend-architecture.md`, `base-component-decision-gate.md`, `REPORT_TEMPLATE.md`.

### 3.2 Berkas yang berubah

Baru:

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/finance/receivable/billing-data/billing-data-view.jsx` | Halaman Data Tagihan: Hero, panel dua kolom, filter, kartu ringkasan, tab, tabel, konfirmasi Buat AR, toast |
| `src/components/view/finance/receivable/billing-data/billing-data-table-columns.jsx` | Definisi kolom: No, Tanggal, No. Bill, No. Reg, No. RM, Nama Pasien, Penjamin (hanya tab Perusahaan/Asuransi), Jenis Pasien, Jumlah Tagihan, Status Tagihan, AR/Invoice |
| `src/lib/hooks/finance/receivable/use-billing-data.jsx` | State tab, isian filter vs filter yang diterapkan, paginasi, muat data, ringkasan, pilihan penjamin/pasien |
| `src/lib/hooks/finance/receivable/use-billing-data-actions.jsx` | Buat AR/Invoice, Print Data, Export Excel (mengambil seluruh hasil filter) |
| `src/lib/constants/finance/receivable/billing-data-constants.jsx` | Endpoint, kategori, tab, opsi filter, nilai bawaan, batas ambil-semua |
| `src/utils/finance/receivable/billing-data-utils.js` | Penyusun query, validasi rentang tanggal, format tanggal `dd/mm/yyyy`, dokumen cetak/ekspor, ringkasan label |
| `src/style/corporate/finance/receivable/billing-data-view.module.css` | Tata letak saja (panel, kolom, kartu, tab), seluruhnya memakai token `--base-*`/`--type-*`/`--line-height-*`; titik henti responsif 991px dan 575px |
| `src/app/finance/receivable-invoice-batches/batches/page.jsx` | Rute baru Daftar Batch Tagihan |

Diubah:

| Berkas | Perubahan |
| --- | --- |
| `src/app/finance/receivable-invoice-batches/page.jsx` | Sekarang merender `BillingDataView` (metadata "Data Tagihan") |
| `src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-view.jsx` | Judul Hero daftar batch menjadi "Daftar Batch Tagihan" |
| `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx` | Konstanta `RECEIVABLE_INVOICE_BATCH_LIST_ROUTE` |
| `src/lib/hooks/finance/receivable/use-receivable-invoice-batch-detail.jsx` | Tombol kembali dari detail batch menuju daftar batch yang baru |
| `src/components/features/base-features/filter-date-picker.jsx` | Prop opsional `displayFormat="numeric"` (bawaan tetap `"short"`, sehingga konsumen lain tidak berubah) |
| `src/utils/shared/date-picker-utils.jsx` | Fungsi `formatDatePickerNumericLabel` |

Tidak berubah dengan sengaja: label menu **Tagihan/Billing** pada sidebar, `globals.css`, dan halaman lain di modul A/R.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: `app/.../page.jsx` → view → hook → konstanta/utilitas → `InstanceAxios`. Tidak ada state global baru; state lokal dengan hook, sama seperti pola Finance lain.
- Penempatan folder mengikuti modul `finance/receivable` yang sudah ada.
- Daftar penjamin diambil dari server (`GET .../billing-data/payer-options`, berpaging dan debounce 350 ms oleh `FilterSelect`); daftar Karyawan/Pasien lewat `useSelectResource("patients", …)`. Tidak ada master data yang ditulis langsung di kode dan tidak ada daftar yang dimuat utuh.
- Request data dibatalkan dengan `AbortController` saat filter berubah atau komponen ditutup; permintaan pilihan penjamin memakai penanda urutan agar jawaban terlambat dibuang. Ini mencegah hasil lama menimpa hasil baru.
- Tanpa dependency baru, tanpa komponen dasar baru, tanpa migrasi ke TypeScript.

**Keputusan komponen dasar (gerbang)**

| Elemen | Status | Dasar |
| --- | --- | --- |
| Hero + breadcrumb eyebrow | `REUSE` | `Hero`, `FinanceBreadcrumb` |
| Kolom pencarian | `REUSE` | `BaseTextField` |
| Dropdown penjamin/pasien (server-side, 2 baris per opsi) | `REUSE` | `FilterSelect` (`serverSide`, `renderOption`, `menuPortal`) |
| Dropdown Status/Jenis Pasien/Periode/jumlah baris | `REUSE` | `FilterSelect` |
| Tanggal Mulai/Akhir `dd/mm/yyyy` | `EXTEND` | `FilterDatePicker`, prop opsional baru; bawaan tidak berubah untuk konsumen lain |
| Tombol Cari, Reset, Buat AR, Print, Export, Daftar Batch | `REUSE` | `BaseButton` |
| Panel dua kolom | `COMPOSE` | CSS module tata letak saja |
| Kartu ringkasan | `COMPOSE` | CSS module + `StatusBadge` |
| Tab kategori | `COMPOSE` | `button` dengan `role="tab"` + CSS module; tidak ada komponen Tab dasar di `base-features` |
| Tabel + paginasi | `REUSE` | `DataTable` (`sortLatestFirst={false}`), `Pagination` |
| Konfirmasi Buat AR | `REUSE` | `ConfirmModal` |
| Alert, toast, gerbang akses | `REUSE` | `InformationAlert`, `ToastStack`, `AccessDeniedGate` |

**`UI GATE`**: 12 elemen — `REUSE` 8, `EXTEND` 1, `COMPOSE` 3, `WRAP` 0, `NEW` 0. Tidak ada elemen `NEW`. Satu `EXTEND` bersifat opt-in sehingga tidak mengubah perilaku bawaan komponen bersama; keputusan ini diambil atas dasar permintaan pengguna dan belum ditinjau ulang oleh pemilik UI.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan "Mengambil Data Tagihan..."; teks di atas tabel "Memuat data tagihan..."; tombol Cari menunjukkan status memproses |
| Kosong | "Tidak ada data {nama tab} yang sesuai." dengan saran mengubah Status Tagihan, periode, atau kata kunci |
| Gagal | Alert merah berisi pesan dari backend (atau pesan umum). Pemulihan: tekan **Cari** lagi atau ubah filter |
| Tanpa hak akses | `AccessDeniedGate` menampilkan layar akses ditolak bila backend menjawab 403 |
| Catatan backend | Alert informasi bila backend mengirim `notice` (misalnya tab Karyawan belum punya sumber data) |
| Aksi berjalan | Seluruh filter, tab, dan tombol lain dinonaktifkan selama Buat AR/Print/Export berjalan; hanya satu aksi dalam satu waktu |
| Rentang tanggal tidak valid | Pesan di bawah kolom tanggal, tombol Cari nonaktif |

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Receivable

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receivables/billing-data` | Tabel Data Tagihan, kartu ringkasan, dan sumber Print/Export/Buat AR | `FinanceReceivable : Read` |
| `GET` | `/v1/corporate/finance-management/receivables/billing-data/payer-options` | Pilihan Perusahaan/Asuransi (tab Perusahaan/Asuransi) | `FinanceReceivable : Read` |

#### Corporate / Finance Management / Receivable Invoice Batch

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/finance-management/receivable-invoice-batches` | Buat AR/Invoice (membuat Batch Tagihan Draf) | `FinanceReceivableInvoiceBatch : Create` |

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients/options` | Pilihan pasien (tab Karyawan dan Pasien Umum) lewat `useSelectResource` | `Patient : Read` |

**Pemetaan filter → query parameter** (`GET billing-data`)

| Filter di layar | Query parameter | Catatan |
| --- | --- | --- |
| Tab | `category` | `company` / `employee` / `generalPatient` |
| Pencarian | `search` | Dikirim bila tidak kosong |
| Perusahaan/Asuransi, Karyawan, atau Pasien | `entityId` | Tab Perusahaan/Asuransi: Id penjamin. Tab lain: Id pasien |
| Status Tagihan | `billingStatus` | `notCreated` (bawaan) / `created`; "Semua" tidak dikirim |
| Jenis Pasien | `patientType` | `outpatient` / `inpatient` / `emergency`; "Semua" tidak dikirim |
| Periode | `period` | `today` / `yesterday` / `thisWeek` / `thisMonth` / `lastMonth`; "Semua" tidak dikirim |
| Tanggal Mulai | `startDate` | `yyyy-MM-dd`; bila terisi, mengalahkan Periode |
| Tanggal Akhir | `endDate` | `yyyy-MM-dd` |
| Halaman | `pageNumber` | State `page` di layar dikirim sebagai `pageNumber` |
| Jumlah baris | `pageSize` | 10 / 25 / 50 / 100 |

Pilihan penjamin memakai `search` dan `limit` (bawaan 20) pada `payer-options`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` / `npm run lint:errors` | Tidak dijalankan atas preferensi pengguna (perintah berat hanya bila diminta) | `NOT RUN` | — |
| `npm run build` | Tidak dijalankan, alasan yang sama | `NOT RUN` | — |
| `npm run test:unit` | Tidak dijalankan, alasan yang sama | `NOT RUN` | — |
| Parser TypeScript (hanya sintaks) atas 13 berkas JS/JSX yang baru/diubah | Tanpa diagnostik sintaks | `PASS` (bukan pengganti lint/build) | Pemeriksaan satu kali di sesi ini |
| Pemeriksaan impor: setiap nama dan default yang diimpor oleh 10 berkas ada pada modul sumbernya | Cocok semua | `PASS` (skrip sementara, bukan bagian repository) | Keluaran skrip |
| Pemeriksaan tag JSX: setiap komponen yang dipakai di view, kolom, date picker, dan dua `page.jsx` terdeklarasi/terimpor | Cocok semua | `PASS` (skrip sementara) | Keluaran skrip |
| Pemeriksaan CSS: setiap `styles.x` yang dipakai ada di modul CSS, setiap `var(--…)` terdefinisi, tanpa warna/ukuran huruf/radius/bayangan/line-height literal | 32 kelas terdefinisi, 0 yang hilang, 0 nilai literal | `PASS` (skrip sementara) | Keluaran skrip |
| Prop komponen dasar yang dipakai (`ConfirmModal`, `BaseButton`, `Hero`, `InformationAlert`, `BaseTextField`, `FilterSelect`, `StatusBadge`, `DataTable`, `AccessDeniedGate`) ada pada source komponen | Ada semua | `PASS` (pembacaan source) | `grep` pada berkas komponen |
| Uji hidup di browser (daftar opsi, label terpilih, efek ke request, reset, filter gabungan, paginasi, modal Buat AR, Print/Export, responsif) | Tidak dapat dilakukan: backend `BE-FIN-FIX-001` belum dikompilasi sehingga endpoint belum ada | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` — endpoint `billing-data` dan `payer-options` belum ada di backend yang berjalan.

Uji otomatis: `AUTOMATED TEST: SKIPPED (opsional)` — `rules/frontend/test-policy.md` tidak mewajibkan test baru, dan tidak ada permintaan eksplisit untuk menulisnya.

**Tidak dijalankan:** lint, build, dan test unit (preferensi pengguna: perintah berat hanya dijalankan bila diminta); `dotnet build` backend (di luar wewenang task ini). Pemeriksaan "PASS" di atas adalah pemeriksaan statis buatan sendiri dan **tidak** boleh dibaca sebagai lint, build, atau uji fungsi yang lulus.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Judul halaman "Data Tagihan"; subjudul dinamis per tab | Terpenuhi (di source) | `billing-data-view.jsx` (`Hero title`, `getBillingDataSubtitle`) |
| Kartu dua kolom: filter + tombol aksi di kiri, ringkasan + tab di kanan | Terpenuhi (di source) | `billing-data-view.jsx`, `billing-data-view.module.css` |
| Tab Perusahaan/Asuransi, Karyawan, Pasien Umum dengan dropdown spesifik tab, tanpa master data di kode | Terpenuhi (di source) | `billing-data-constants.jsx`, `use-billing-data.jsx` |
| Filter Status Tagihan (bawaan Belum Dibuat), Jenis Pasien, Periode, Tanggal Mulai/Akhir `dd/mm/yyyy`, validasi mulai ≤ akhir | Terpenuhi (di source) | `billing-data-utils.js` (`validateBillingDataDateRange`), `FilterDatePicker` `displayFormat="numeric"` |
| Cari menerapkan filter; Reset mengembalikan nilai bawaan (tab tetap); pindah tab mengembalikan filter ke nilai bawaan | Terpenuhi (di source) | `use-billing-data.jsx` |
| Kartu ringkasan mengikuti filter | Terpenuhi (di source) | Parameter yang sama dikirim untuk tabel dan ringkasan; permintaan kedua untuk kartu "Total + penjamin" |
| Kolom tabel dan nomor urut `((halaman − 1) × ukuran halaman) + indeks + 1` | Terpenuhi (di source) | `billing-data-table-columns.jsx` (`meta.rowNumber` dari `DataTable`) |
| Buat AR/Invoice, Print Data, Export Excel | Terpenuhi (di source) | `use-billing-data-actions.jsx` |
| State memuat, kosong, gagal, tanpa akses | Terpenuhi (di source) | Bagian 4 |
| Debounce dan perlindungan hasil basi | Terpenuhi (di source) | `AbortController`, penanda urutan, debounce 350 ms `FilterSelect` |
| Responsif | Terpenuhi (di CSS), belum diuji di layar | `billing-data-view.module.css` |
| TypeScript/build/lint berhasil | **Belum terpenuhi** — repository ini JavaScript sehingga tidak ada TypeScript; lint dan build `NOT RUN` | Bagian 6 |
| Uji di browser dengan data nyata | **Belum terpenuhi** — `NOT FEASIBLE` | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tombol **Cari** memakai `BaseButton loading`, yang mengganti tulisan tombol dengan "Memproses..." selama memuat |
| Masalah yang diketahui | (1) Tab **Karyawan** tidak punya data dan tidak punya "ID Karyawan": tabel kosong dengan catatan (`FIN-DEC-006` terbuka). (2) Tagihan perusahaan tanpa referensi penjamin tidak dapat dibuatkan batch (`NO_DEBTOR_REFERENCE`); perlu perubahan di Billing dan backfill. (3) Pembatalan batch di backend belum membebaskan piutangnya, sehingga tagihan bisa tertahan sebagai "Sudah Dibuat" (cacat lama backend, dijelaskan di `BE-FIN-FIX-001`). (4) Print/Export/Buat AR dibatasi 1.000 baris karena belum ada endpoint ekspor di backend; Export Excel berupa tabel HTML berekstensi `.xls`, sama seperti V1, karena tidak boleh menambah library. (5) Rute lama daftar batch dipindah ke `/finance/receivable-invoice-batches/batches` (disetujui pengguna); tautan lama yang mengharapkan daftar batch kini membuka Data Tagihan. Breadcrumb halaman batch lain masih berlabel "Tagihan Gabungan Penjamin" tetapi `FinanceBreadcrumb` hanya menampilkan nama bagian, jadi tidak terlihat. (6) Daftar batch di frontend mencari nama debitur lewat resolver penjamin perusahaan, sedangkan referensi backend adalah Id penyedia asuransi; terlihat, tidak diubah |
| Item V1 yang sengaja tidak dibawa | Penyaringan di browser dengan `perPage` 1000 (diganti penyaringan di database); nama "RS Benefit" yang ditulis langsung di kode (backend belum punya sumber Karyawan); halaman form Buat AR terpisah dengan field sendiri (diganti konfirmasi singkat memakai Batch Tagihan yang ada); tabel terkelompok per perusahaan dengan subtotal; batas hari UTC (diganti hari bisnis WIB); gabungan Periode DAN rentang tanggal (kini rentang tanggal yang mengalahkan Periode); tab Canceled AR (sudah menjadi menu terpisah di V2). Awal minggu hari Minggu dipertahankan seperti V1 |
| Dependency backend | `BE-FIN-FIX-001`: `dotnet build` **NOT RUN**, belum di-commit, tidak ada uji dengan data. Sampai backend dikompilasi dan dijalankan, halaman ini tidak dapat menampilkan data. Perubahan validasi `POST receivable-invoice-batches` juga berasal dari task itu |
| Perubahan sampingan | `NONE`. Satu konstanta yang tidak terpakai (`BILLING_DATA_ROUTE`) dihapus sebelum penutupan task; berkas skrip pemeriksaan sementara hanya ada di folder scratchpad sesi, bukan di repository |
| Interupsi | Sesi sempat dipadatkan di tengah verifikasi impor; skrip pemeriksa saya sendiri sempat salah (backslash pada ekspresi reguler hilang) dan melaporkan 82 masalah palsu. Skrip diperbaiki dan dijalankan ulang tanpa masalah; kode aplikasi tidak berubah karena hal itu |
| Status Git | `git status --short` frontend di akhir pekerjaan: ` M src/app/finance/receivable-invoice-batches/page.jsx`, ` M src/components/features/base-features/filter-date-picker.jsx`, ` M src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-view.jsx`, ` M src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx`, ` M src/lib/hooks/finance/receivable/use-receivable-invoice-batch-detail.jsx`, ` M src/utils/shared/date-picker-utils.jsx`, `?? src/app/finance/receivable-invoice-batches/batches/`, `?? src/components/view/finance/receivable/billing-data/`, `?? src/lib/constants/finance/receivable/billing-data-constants.jsx`, `?? src/lib/hooks/finance/receivable/use-billing-data-actions.jsx`, `?? src/lib/hooks/finance/receivable/use-billing-data.jsx`, `?? src/style/corporate/finance/receivable/`, `?? src/utils/finance/receivable/billing-data-utils.js`. Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy |
| Langkah berikutnya | (1) Jalankan `dotnet build` backend lalu perbaiki bila ada galat. (2) Jalankan `npm run lint:errors`, `npm run test:unit`, dan `npm run build` frontend bila pengguna setuju. (3) Uji manual di browser: tiga tab, semua filter gabungan, Reset, paginasi, Print/Export, Buat AR sampai halaman detail batch. (4) Putuskan sumber data Karyawan (`FIN-DEC-006`) dan perbaikan referensi penjamin pada tagihan perusahaan. (5) Tinjau ulang keputusan `EXTEND` `FilterDatePicker` dan `COMPOSE` tab dengan pemilik UI |
