# Accounting — Frontend Architecture

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` |
| Revision | `3` |
| Status | `draft` — approval adalah tindakan manusia, belum diberikan |
| Cakupan | MVP tulang punggung akuntansi (`ACC-DEC-009`) |
| Frontend SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) — baseline **saat dokumen ini disusun**. Baseline blueprint kini `31a82c8` (`QuilvianIntegrationFrontend`); kutipan di bawah tetap berlaku, lihat `evidence/02-frontend-rebaseline-impact-scan.md` |
| Masukan | `02-backend-architecture.md@3`, `contracts/api-contract.md@ACC-API-0.1` |
| Sumber konvensi | `QuilvianSystemFrontendDev/CLAUDE.md@fc49cc7` |

Dokumen ini memuat **kontrak fungsional** frontend: layar apa yang dibutuhkan, siapa boleh
melakukan apa, data dan status apa yang dikonsumsi, serta bagaimana keadaan tidak normal
ditangani. Rincian arsitektur mendalam ada di sisi backend.

---

## 1. Aturan yang mengikat

Enam aturan berikut berasal dari `CLAUDE.md` frontend dan tidak bisa ditawar.

| Aturan | Akibatnya bagi Accounting |
|---|---|
| Panggilan API hanya di Redux slice, di dalam `createAsyncThunk`, memakai `InstanceAxios` | Komponen akuntansi tidak boleh memanggil API sendiri. Data diambil lewat `useSelector`, permintaan dipicu lewat `useDispatch` |
| Saat mengubah berkas, tulis seluruh isinya | Berlaku saat implementasi |
| Teks yang dilihat pengguna ditulis Bahasa Indonesia | "Jurnal Umum", "Buku Besar", "Neraca Saldo", "Ajukan", "Setujui", "Sahkan", "Balik" |
| CSS Modules terpusat di `src/style/**`, dilarang `style={{ ... }}` | Berkas gaya diletakkan mengikuti struktur folder yang sudah ada |
| `createBy` diambil dari pengguna yang sedang masuk lewat `selectUserInfo` | Pembuat, pengaju, penyetuju, dan pengesah **tidak pernah** berasal dari isian form |
| Daftar memakai `DataTable` dan `DataFilter` | Dilarang membuat tabel atau panel penyaring manual |

Dua tambahan dari pengalaman modul lain:

- **`src/app/globals.css` tidak boleh disentuh.** Kebutuhan gaya khusus diselesaikan di berkas
  CSS Module milik modul.
- Komponen bernama `BaseDataTable` di `src/components/features/TableModern/BaseTable.jsx`
  **tidak dipakai siapa pun dan tidak boleh dipakai**. Yang benar adalah `DataTable` dan
  `DataFilter` di `base-features/`.

---

## 2. Komponen yang dipakai ulang

Modul ini **tidak membuat komponen dasar tandingan**. Seluruhnya sudah tersedia di
`src/components/features/base-features/@fc49cc7`.

| Komponen | Dipakai untuk |
|---|---|
| `data-table.jsx` | Daftar akun, daftar jurnal, baris jurnal, buku besar, neraca saldo, periode |
| `data-filter.jsx` | Panel penyaring pada setiap halaman daftar |
| `filter-date-picker.jsx` | Penyaring rentang tanggal akuntansi |
| `filter-select.jsx`, `resource-filter-select.jsx` | Penyaring badan hukum, jenis jurnal, status, akun |
| `confirm-modal.jsx` | Konfirmasi tindakan berisiko: Sahkan, Balik, Tutup Periode, Buka Kembali |
| `status-badge.jsx` | Status jurnal dan status periode |
| `base-detail-card.jsx`, `base-detail-view.jsx` | Halaman rincian jurnal |
| `summary-grid.jsx` | Ringkasan total debit, total kredit, dan selisih |
| `hero.jsx` | Kepala halaman |
| `access-denied-gate.jsx`, `access-denied-alert.jsx` | Menutup halaman dari pengguna tanpa hak |
| `toast-stack.jsx` | Pemberitahuan berhasil dan gagal |
| `base-form-control.jsx`, `base-text-field.jsx` | Isian form |

---

## 3. Redux slice

Setiap sumber data punya satu slice di `src/lib/state/slice/`. **Setiap slice baru wajib
didaftarkan di `src/lib/state/store.jsx`** — slice yang tidak terdaftar tidak akan pernah punya
state.

| Slice (nama sementara) | Isi | Rilis pertama? |
|---|---|:---:|
| `accounting-chart-of-account-slice.jsx` | Daftar akun, rincian, susunan pohon, opsi isian pilihan | Ya |
| `accounting-journal-type-slice.jsx` | Jenis jurnal dan opsinya | Ya |
| `accounting-journal-slice.jsx` | Daftar jurnal, rincian, serta aksi ajukan, setujui, tolak, sahkan, balik | Ya |
| `accounting-period-slice.jsx` | Daftar periode, periode berjalan, bangkitkan, tutup, buka kembali | Ya |
| `accounting-general-ledger-slice.jsx` | Mutasi buku besar, neraca saldo, saldo per akun | Ya |
| `accounting-posting-rule-slice.jsx` | Pemetaan kejadian ke akun | Tidak — Phase 2 |
| `accounting-event-inbox-slice.jsx` | Kotak masuk kejadian dan daftar gagal | Tidak — Phase 2 |

**Untuk daftar akun dan jenis jurnal, gunakan factory yang sudah ada.** Berkas
`src/lib/state/slice/master-data-resource-slice-factory.jsx@fc49cc7` sudah menyediakan pola CRUD
master data lengkap. Keduanya master ber-CRUD standar, jadi slice-nya dibangun dari factory itu,
bukan ditulis dari nol.

Jurnal **tidak** memakai factory, karena punya lima aksi di luar CRUD biasa. Slice jurnal ditulis
manual tetapi tetap mengikuti bentuk state hasil factory:

- flag pemuatan terpisah per operasi: `loading`, `listLoading`, `detailLoading`, `optionsLoading`,
  `actionLoading`;
- error terpisah dengan penamaan sejajar: `error`, `listError`, `detailError`, `actionError`;
- pagination berbentuk `{ pageNumber, pageSize, totalData, totalPage, items }` dengan `pageSize`
  bawaan 25;
- pesan error dinormalisasi lewat `normalizeErrorMessage`.

**Contoh pesan yang benar.** Ketika pengesahan ditolak karena periode tertutup, pengguna melihat
"Periode September 2026 sudah ditutup sementara. Hanya jurnal penyesuaian dan pembalikan yang
masih dapat disahkan." Bukan "Request failed with status code 422".

---

## 4. Layar yang dibutuhkan

`ACC-DEC-009` dan `ACC-DEC-030` menetapkan **delapan layar** untuk rilis pertama.

| # | Layar | Kebutuhan fungsional | Rilis pertama? |
|---:|---|---|:---:|
| 1 | COA | Daftar berhalaman dengan penyaring badan hukum, jenis akun, dan status. Ada tampilan susunan induk-anak | Ya |
| 2 | Form Akun | Tambah dan ubah akun, termasuk memilih induk | Ya |
| 3 | Daftar Jurnal | Daftar berhalaman dengan penyaring badan hukum, rentang tanggal, jenis, status, dan nomor | Ya |
| 4 | Form Jurnal | Kepala jurnal ditambah tabel baris yang dapat ditambah dan dihapus, dengan total berjalan | Ya |
| 5 | Rincian Jurnal | Tampilan baca-saja, tombol aksi sesuai kewenangan, dan riwayat persetujuan | Ya |
| 6 | Buku Besar | Mutasi per akun dan rentang tanggal, dengan saldo berjalan | Ya |
| 7 | Neraca Saldo | Saldo seluruh akun pada satu periode, dengan penanda seimbang | Ya |
| 8 | Periode Akuntansi | Daftar periode, bangkitkan setahun, tutup, buka kembali | Ya |
| 9 | Jenis Jurnal | Master jenis jurnal | Ya, layar kecil |
| 10 | Pemetaan Posting | Pengaturan kejadian menjadi akun | Tidak — Phase 2 |
| 11 | Kotak Masuk Kejadian | Kejadian masuk, gagal, dan pengulangan | Tidak — Phase 2 |
| 12 | Laba Rugi dan Neraca | Laporan keuangan | Tidak — Phase 2 per `ACC-DEC-030` |

> **Nama layar 1 diubah menjadi `COA` pada 4 September 2026, keputusan owner.** Nama sebelumnya
> *Daftar Akun* tertukar dengan akun pengguna. Perubahan ini **hanya label tampilan** — nol
> perubahan rute, entity, endpoint, atau permission; `AccChartOfAccount` dan
> `ChartOfAccount : Read` tetap seperti semula. Istilah **daftar akun** dalam prosa dokumen ini
> dan pada seluruh kontrak tetap dipakai sebagai nama konsep akuntansinya, bukan nama layar.

### Pemilih badan hukum ada di semua layar

`ACC-DEC-037` memisahkan pembukuan per badan hukum. Akibatnya **setiap layar akuntansi wajib
punya pemilih badan hukum**, dan pilihan itu ikut ke setiap permintaan.

Pilihan badan hukum yang sedang aktif sebaiknya bertahan saat pengguna berpindah antar layar
akuntansi, supaya tidak perlu memilih ulang setiap kali. Cara menyimpannya diserahkan ke
developer.

**Peringatan yang penting.** Menyembunyikan badan hukum yang bukan hak pengguna di layar
**bukan** pengamanan. Backend tetap menolak permintaan atas badan hukum yang bukan haknya. Layar
hanya membuat pekerjaan lebih nyaman.

### Form Jurnal perlu perhatian khusus

Ini satu-satunya layar yang tidak mengikuti pola form biasa, karena berisi tabel baris yang
bertambah dan berkurang, dengan total yang dihitung ulang setiap kali angka berubah.

Yang sudah pasti:

- Form memakai `react-hook-form` dengan `FormProvider` dan `Controller`, sesuai library yang
  sudah dipakai di seluruh aplikasi.
- Total debit, total kredit, dan selisihnya ditampilkan terus-menerus memakai `summary-grid`.
- Tombol Ajukan tidak aktif selama selisih belum nol.
- Daftar pilihan akun hanya memuat akun yang menerima transaksi dan aktif, karena
  `GET /options` memang sudah menyaringnya. Frontend tidak perlu menyaring ulang.
- Kolom unit biaya menjadi wajib begitu akun yang dipilih berjenis beban. Frontend mengetahuinya
  dari `RequiresCostCenter` pada `ChartOfAccountOptionDto`, **bukan** dari aturan yang dihafal
  sendiri.

**Contoh tampilannya.** Petugas mengisi tiga baris: debit Beban Obat Rp 3.000.000, debit Beban
Alat Habis Pakai Rp 1.500.000, kredit Persediaan Farmasi Rp 4.000.000. Ringkasan menampilkan
total debit Rp 4.500.000, total kredit Rp 4.000.000, dan selisih Rp 500.000 berwarna peringatan.
Tombol Ajukan tetap mati sampai selisih menjadi nol.

Bentuk barisnya sederhana dan muat di layar tanpa gulir mendatar: akun, keterangan, debit,
kredit, dan unit biaya. `ACC-DEC-019` memilih satu dimensi saja, dan `ACC-DEC-020` menghapus
kolom mata uang serta kurs.

---

## 5. Aksi per peran

Tombol ditampilkan berdasarkan `AvailableActions` yang **dikirim backend** pada
`JournalDetailDto`. Frontend tidak menghitung sendiri kapan sebuah tombol boleh muncul.

Ini disengaja: aturan `ACC-DEC-016` — pembuat tidak boleh menyetujui jurnalnya sendiri —
bergantung pada data, bukan hanya pada peran. Menghitungnya di frontend berarti menyalin aturan
bisnis ke tempat yang salah, dan menyalin berarti suatu saat akan berbeda.

| Status jurnal | Tombol yang mungkin muncul | Syarat dari backend |
|---|---|---|
| `Draft` | Ubah, Hapus, Ajukan | Punya hak `Journal : Update`, `Delete`, `Submit` |
| `PendingApproval` | Setujui, Tolak | Punya hak `Journal : Approve`, **dan bukan pembuat jurnal itu** |
| `Approved` | Sahkan, Tolak | Punya hak `Journal : Post` untuk Sahkan; `Journal : Approve` untuk Tolak |
| `Posted` | Balik, Cetak | Punya hak `Journal : Reverse`; jurnal belum pernah dibalik |
| `Rejected` | Sunting kembali | Pembuatnya |

Halaman yang seluruhnya di luar hak akses pengguna ditutup memakai `access-denied-gate.jsx` yang
sudah ada, bukan dengan pengalihan halaman buatan sendiri.

---

## 6. Penanganan keadaan tidak normal

| Keadaan | Yang harus terjadi di layar |
|---|---|
| Sedang memuat | Penanda memuat per bagian, memakai flag terpisah dari slice. Bukan satu penanda untuk seluruh halaman |
| Daftar kosong | Pesan yang menjelaskan sebabnya dan langkah berikutnya. Contoh: "Belum ada jurnal pada rentang tanggal ini." |
| Gagal memuat | Pesan Bahasa Indonesia hasil `normalizeErrorMessage`, disertai tombol Coba Lagi |
| Data sudah berubah orang lain | Setelah setiap aksi berhasil, rincian jurnal dimuat ulang dari backend. Jangan menebak status baru di sisi frontend |
| Tombol ditekan dua kali | Tombol aksi dimatikan selama `actionLoading` menyala, sehingga satu jurnal tidak terkirim dua kali |
| Ditolak karena periode tertutup | Pesan `422` dari backend ditampilkan apa adanya, karena sudah menyebut nama periodenya |
| Ditolak karena menyetujui jurnal sendiri | Pesan `403` ditampilkan apa adanya. Tombolnya idealnya sudah tidak muncul, tetapi pesan tetap disiapkan |
| Jurnal belum seimbang | Selisih ditampilkan terus-menerus di ringkasan, jadi pengguna tahu sebelum menekan Ajukan |

Satu hal yang perlu ditegaskan soal memuat ulang: **jangan menyimpulkan status baru di frontend.**
Setelah menekan Sahkan, jangan langsung mengubah tampilan menjadi "Disahkan" tanpa menunggu
jawaban backend. Backend bisa saja menolak karena periode tertutup di detik terakhir.

---

## 7. Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `ACC-FE-001` | Letak menu Accounting di navigasi | Product owner | **`closed`** — 4 Sep 2026 | **Pilihan B: `src/app/corporate/accounting/`** | Keputusan owner Rizki, 4 September 2026. Sempat ditetapkan pilihan A pada hari yang sama lalu **diubah owner menjadi B** sebelum implementasi selesai |
| `ACC-FE-002` | Layar mana yang masuk rilis pertama | Rizki | `closed` | Delapan layar, sesuai `ACC-DEC-009` dan `ACC-DEC-030` | Decision log, 1 September 2026 |
| `ACC-FE-003` | Rincian jurnal sebagai halaman tersendiri atau panel samping | Product owner | **`closed`** — 4 Sep 2026 | **Halaman tersendiri**, memakai `base-detail-view.jsx` | Keputusan owner Rizki, 4 September 2026; `base-detail-view.jsx` dipakai **79 berkas**, `base-detail-side-panel.jsx` hanya **1**, diukur di `@1a86d933` |
| `ACC-FE-004` | Susunan dan urutan kolom pada tabel daftar | Developer | `DEV_DISCRETION` | Bebas, selama memakai `DataTable` | `data-table.jsx@fc49cc7` |
| `ACC-FE-005` | Penempatan berkas CSS Module | Developer | `DEV_DISCRETION` | Di bawah `src/style/`, mengikuti struktur yang sudah ada | `CLAUDE.md@fc49cc7` aturan 4 |
| `ACC-FE-006` | Pemilihan ikon | Developer | `DEV_DISCRETION` | Dari `react-icons`, terutama `react-icons/fa6` | `CLAUDE.md@fc49cc7` |
| `ACC-FE-007` | Bentuk konfirmasi tindakan berisiko | Developer | `DEV_DISCRETION` | Memakai `confirm-modal.jsx` yang sudah ada | `confirm-modal.jsx@fc49cc7` |
| `ACC-FE-008` | Cara mempertahankan pilihan badan hukum antar layar | Developer | `DEV_DISCRETION` | Bebas, selama tidak menjadi satu-satunya pengaman | `ACC-DEC-037` |

### `ACC-FE-001` — di mana menu Accounting diletakkan

Struktur `src/app/@fc49cc7` berisi `administrator`, `health-services`, `hr`, `kiosk`,
`queue-display`, `self-services`, dan `settings`. Di backend, Accounting berada di bawah
`Areas/Corporate/`, sama seperti Human Resource yang di frontend memakai folder `hr`.

Tiga pilihan, dan **owner produk yang memutuskan**:

- **A. Folder baru `src/app/accounting/`** — sejajar dengan `hr`, mengikuti kebiasaan yang sudah
  ada bahwa satu domain Corporate mendapat satu folder tingkat atas. Paling konsisten dengan yang
  sudah berjalan.
- **B. Folder baru `src/app/corporate/accounting/`** — lebih cocok dengan susunan backend, tetapi
  menjadi satu-satunya folder yang memakai tingkat `corporate`, sedangkan `hr` tidak. Menimbulkan
  dua pola sekaligus.
- **C. Menumpang di `src/app/settings/`** — **tidak dianjurkan**; Accounting adalah modul
  operasional harian, bukan pengaturan.
- **D. Other — tuliskan pilihan atau batasan lain.**

Struktur komponen mengikuti pola yang sudah berlaku: berkas rute di `src/app/**` tipis, sedangkan
isi halaman berada di `src/components/view/**` dengan susunan folder yang mencerminkan
`src/app/**`.

### Keputusan 4 September 2026 — keduanya `closed`

Owner Rizki memutuskan keduanya sekaligus, dan `ACC-TD-009` ditutup.

| Keputusan | Pilihan | Alasan yang menentukan |
|---|---|---|
| `ACC-FE-001` | **B — `src/app/corporate/accounting/`** | **Keputusan owner.** Susunan frontend dibuat mengikuti susunan backend `Areas/Corporate/AccountingManagement/`, sehingga satu domain mudah ditelusuri lintas repository. Folder `corporate/` dibuat baru karena belum ada |
| `ACC-FE-003` | **Halaman tersendiri** | Diukur di `@1a86d933`: `base-detail-view.jsx` dipakai **79 berkas**, `base-detail-side-panel.jsx` hanya **1**. Rincian jurnal memuat tabel baris, riwayat persetujuan, dan tombol aksi sekaligus — panel samping akan memotongnya bila barisnya banyak. Halaman tersendiri juga dapat di-bookmark dan dibagikan tautannya |

Jalur yang mengikat seluruh task frontend berikutnya:

| Lapisan | Jalur |
|---|---|
| Rute (berkas tipis) | **`src/app/corporate/accounting/`** |
| Isi halaman | **`src/components/view/corporate/accounting/`** |
| Konstanta | **`src/lib/constants/corporate/accounting/`** |
| Hook | **`src/lib/hooks/corporate/accounting/`** |
| CSS Module | **`src/style/corporate/accounting/`** |
| URL yang dilihat pengguna | **`/corporate/accounting`** |

Layar rincian jurnal (`FE-ACC-007`) memakai **`base-detail-view.jsx`**, bukan panel samping.

**Segmen `corporate/` dipakai konsisten di kelima lapisan**, bukan hanya di rute. Menaruh rute di
`corporate/accounting` tetapi isinya di `view/accounting` akan memutus cermin antara `src/app/` dan
`src/components/view/` yang selama ini dipegang repository.

**Konsekuensi yang diterima owner:** `hr` tetap berada di tingkat atas walaupun ia juga domain
Corporate, sehingga untuk sementara ada dua pola berdampingan. Pembaca berikutnya perlu tahu bahwa
`corporate/` adalah pola yang dituju, dan `hr` adalah peninggalan sebelum pola itu ditetapkan.

---

## 8. Yang tidak dikerjakan frontend

Agar batasnya jelas:

- Frontend **tidak** menghitung saldo, total, atau keseimbangan sebagai sumber kebenaran. Angka
  yang ditampilkan berasal dari backend. Perhitungan di layar hanya membantu petugas melihat
  selisih sebelum mengajukan.
- Frontend **tidak** memutuskan siapa boleh menyetujui atau mengesahkan. Layar hanya menampilkan
  tombol sesuai `AvailableActions`; keputusan tetap di backend.
- Frontend **tidak** membangkitkan nomor jurnal maupun menentukan periode akuntansi. Keduanya
  ditetapkan backend.
- Frontend **tidak** menyimpan data akuntansi di penyimpanan browser.
- Frontend **tidak** memperkenalkan lapisan penerjemahan bahasa baru. Teks ditulis langsung dalam
  Bahasa Indonesia, sesuai kebiasaan yang berlaku.


---

# PHASE 2 (`ACC-PH-006`) — Rencana, belum tersedia

| Field | Nilai |
|---|---|
| Cakupan | `ACC-P2-S1` sampai `ACC-P2-S4` |
| Status | **`approved`** — Rizki, 8 September 2026 |
| Frontend SHA | `e732424eb` (branch `RizkiV2`) |
| Traceability | `ACC-DEC-044` sampai `ACC-DEC-057` |

Seluruh aturan bagian 1 tetap berlaku penuh, terutama dua yang paling sering dilanggar:

1. **Pakai ulang komponen dan gaya yang sudah ada.** Jangan membuat tandingan tabel, tombol,
   modal, atau pemilih yang sudah dipakai sebelas layar MVP.
2. **`globals.css` tidak disentuh.** Gaya khusus Phase 2 ditulis sebagai CSS Module milik
   layarnya sendiri.

## 9. Peta butir menu Phase 2

Melanjutkan letak menu yang diputuskan `ACC-FE-001`. Enam butir bertambah, seluruhnya di dalam
menu Accounting yang sudah ada.

| # | Butir menu | Tingkat | Induk | Route | Layar yang dituju | Hak akses penjaga |
|---:|---|:---:|---|---|---|---|
| 1 | Kotak Masuk Kejadian | 2 | Accounting | `/accounting/accounting-events` | Kotak Masuk Kejadian | `AccountingEvent : Read` |
| 2 | Aturan Posting | 3 | Accounting › Master Data | `/accounting/posting-rules` | Aturan Posting | `PostingRule : Read` |
| 3 | Jenis Kejadian | 3 | Accounting › Master Data | `/accounting/event-types` | Jenis Kejadian | `EventType : Read` |
| 4 | Jurnal Berulang | 2 | Accounting | `/accounting/recurring-journals` | Jurnal Berulang | `RecurringJournal : Read` |
| 5 | Tutup Tahun | 2 | Accounting | `/accounting/year-end-closing` | Tutup Tahun | `YearEndClosing : Read` |
| 6 | Pengaturan Akuntansi | 3 | Accounting › Master Data | `/accounting/configuration` | Pengaturan Akuntansi | `AccountingConfiguration : Update` |

### Layar anak yang tidak muncul sebagai butir menu

| Layar anak | Layar induk yang menjadi jalan masuknya |
|---|---|
| Rincian Kejadian | Kotak Masuk Kejadian, lewat klik satu baris |
| Form Aturan Posting | Aturan Posting, lewat tombol Tambah dan Ubah |
| Form Jenis Kejadian | Jenis Kejadian, lewat tombol Tambah dan Ubah |
| Form Jurnal Berulang | Jurnal Berulang, lewat tombol Tambah dan Ubah |
| Riwayat Penerbitan Template | Rincian Jurnal Berulang |
| Daftar Periksa Penutupan | Periode Akuntansi (layar MVP), lewat tombol Tutup Periode |

**Daftar Periksa Penutupan sengaja bukan butir menu tersendiri.** Ia hanya bermakna dalam konteks
satu periode yang hendak ditutup; menaruhnya di menu akan menimbulkan pertanyaan "periode yang
mana" sebelum petugas sempat memilih apa pun.

### Penanda angka pada butir Kotak Masuk Kejadian

`ACC-DEC-057` menetapkan pemberitahuan memakai penanda jumlah pada menu, tanpa Hub baru dan tanpa
surel. Wujudnya: butir menu Kotak Masuk Kejadian membawa angka jumlah kejadian berstatus
**Gagal**, diambil dari `GET /accounting-events/summary` saat menu dimuat dan saat layar Accounting
dibuka.

Kejadian **Tertahan** **tidak** ikut dihitung pada penanda itu, karena ia bukan gangguan yang
menuntut tindakan segera — ia menunggu pekerjaan pemetaan yang wajar dijadwalkan.

## 10. Layar Phase 2 yang dibutuhkan

| # | Layar | Kebutuhan fungsional | Slice |
|---:|---|---|---|
| 13 | Kotak Masuk Kejadian | Daftar berhalaman dengan penyaring status, jenis, periode, dan badan hukum. Tab cepat: Semua, Tertahan, Gagal | `ACC-P2-S1` |
| 14 | Rincian Kejadian | Isi pesan, riwayat percobaan, jurnal yang dihasilkan, tombol Coba Ulang dan Abaikan | `ACC-P2-S1` |
| 15 | Aturan Posting | Daftar aturan per badan hukum beserta pasangan akun dan perlakuannya | `ACC-P2-S1` |
| 16 | Form Aturan Posting | Pilih jenis kejadian, akun debit, akun kredit, dan perlakuan | `ACC-P2-S1` |
| 17 | Jenis Kejadian | Master jenis kejadian; layar kecil | `ACC-P2-S1` |
| 18 | Jurnal Berulang | Daftar template beserta status aktif dan jadwal terbitnya | `ACC-P2-S2` |
| 19 | Form Jurnal Berulang | Kepala template ditambah tabel baris dengan total berjalan | `ACC-P2-S2` |
| 20 | Daftar Periksa Penutupan | Dua penghalang dan lima peringatan, beserta tombol Ajukan | `ACC-P2-S3` |
| 21 | Tutup Tahun | Pratinjau perhitungan dan tombol Susun Jurnal Penutup | `ACC-P2-S4` |
| 22 | Pengaturan Akuntansi | Menetapkan akun laba ditahan per badan hukum; layar kecil | `ACC-P2-S4` |

**Form Jurnal Berulang memakai ulang komponen Form Jurnal.** Keduanya sama-sama kepala ditambah
tabel baris dengan total berjalan dan penjaga keseimbangan. Membuat komponen tandingan adalah
pelanggaran aturan bagian 1, dan akan membuat dua tempat yang harus diperbaiki setiap kali aturan
keseimbangan berubah.

## 11. Skema fitur per layar

### 11.1 Kotak Masuk Kejadian

```
+--------------------------------------------------------------+
| [Pemilih Badan Hukum v]                                      |
+--------------------------------------------------------------+
| ( Semua ) ( Tertahan 3 ) ( Gagal 2 ) ( Terjurnal )           |
+--------------------------------------------------------------+
| [Cari nomor kejadian] [Jenis v] [Periode v]      [Segarkan]  |
+--------------------------------------------------------------+
| No Kejadian | Jenis | Tgl Akuntansi | Nilai | Status | Jurnal|
| EVT-100     | ...   | 08/09/2026    | ...   | Terjur | JU/... |
| EVT-101     | ...   | 08/09/2026    | ...   | Tertah | -      |
+--------------------------------------------------------------+
|                                        < 1 2 3 >             |
+--------------------------------------------------------------+
```

| Wilayah | Isinya | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| Pemilih badan hukum | Daftar badan hukum | `GET /legal-entities/primary` | — | — | "Badan hukum utama belum ditetapkan" |
| Tab cepat | Jumlah per status | `GET /accounting-events/summary` | `AccountingEvent : Read` | Angka 0 tidak ditampilkan | Tab tampil tanpa angka |
| Tabel | Daftar kejadian berhalaman | `GET /accounting-events` | `AccountingEvent : Read` | "Belum ada kejadian keuangan yang diterima." | "Daftar kejadian gagal dimuat." + tombol Coba Lagi |

Baris berstatus **Gagal** dan **Tertahan** diberi penanda warna berbeda memakai token warna yang
sudah ada; **jangan** menambah warna baru ke `globals.css`.

### 11.2 Rincian Kejadian

```
+--------------------------------------------------------------+
| EVT-100  [Tertahan]                  [Coba Ulang] [Abaikan]  |
+--------------------------------------------------------------+
| Jenis: ...        Modul asal: Finance                        |
| No transaksi asal: AR-2026-09-00871                          |
| Tgl akuntansi: 08/09/2026   Tgl dokumen: 28/08/2026          |
| Nilai: Rp 10.000.000                                         |
+--------------------------------------------------------------+
| Jurnal yang dihasilkan:  (belum ada)                         |
+--------------------------------------------------------------+
| Riwayat percobaan                                            |
| # | Waktu | Hasil | Pesan                                    |
| 1 | ...   | Gagal | Koneksi database terputus                |
+--------------------------------------------------------------+
| Isi pesan asli                              [Lihat/Sembunyi] |
+--------------------------------------------------------------+
```

| Wilayah | Isinya | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| Kepala | Nomor, status, tombol aksi | `GET /accounting-events/{id}` | `AccountingEvent : Read` | — | "Kejadian tidak ditemukan." |
| Tombol Coba Ulang | — | `POST /accounting-events/{id}/retry` | `AccountingEvent : Retry` | Mati bila status bukan Gagal | Pesan galat, status tidak berubah |
| Tombol Abaikan | Modal berisi kolom alasan **wajib** | `PATCH /accounting-events/{id}/ignore` | `AccountingEvent : Ignore` | Mati bila status bukan Gagal | Pesan galat |
| Jurnal yang dihasilkan | Tautan ke Rincian Jurnal | Dari respons yang sama | `Journal : Read` | "(belum ada)" — wajar untuk Tertahan dan Gagal | — |
| Riwayat percobaan | Tabel percobaan | Dari respons yang sama | `AccountingEvent : Read` | "Belum pernah dicoba." | — |
| Isi pesan asli | Teks mentah, tersembunyi secara bawaan | Dari respons yang sama | `AccountingEvent : Read` | — | — |

**Tombol Abaikan wajib memunculkan modal konfirmasi yang menyebut bahwa tindakan ini tidak dapat
dibatalkan**, karena memang tidak dapat. Kolom alasan tidak boleh kosong.

### 11.3 Daftar Periksa Penutupan

```
+--------------------------------------------------------------+
| Tutup Periode 2026-09              [Ajukan Penutupan]        |
+--------------------------------------------------------------+
| PENGHALANG (harus nol)                                       |
|  x 3 jurnal belum disahkan                    [Lihat]        |
|  x 2 kejadian keuangan gagal                  [Lihat]        |
+--------------------------------------------------------------+
| PERINGATAN (boleh dilewati)                                  |
|  ! 1 kejadian tertahan                        [Lihat]        |
|  ! Penyusutan belum dijalankan                               |
+--------------------------------------------------------------+
```

| Wilayah | Isinya | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| Penghalang | Dua jenis penghalang beserta jumlahnya | `GET /accounting-periods/{id}/closing-checklist` | `Period : Read` | "Tidak ada penghalang." Tombol Ajukan menyala | "Daftar periksa gagal dimuat." |
| Peringatan | Lima jenis peringatan | Dari respons yang sama | `Period : Read` | "Tidak ada peringatan." | — |
| Tombol Ajukan | — | `POST /accounting-periods/{id}/submit-closing` | `Period : Close` | **Mati** selama masih ada penghalang | Pesan galat |
| Tautan Lihat | Membuka daftar tersaring | Layar Jurnal atau Kotak Masuk Kejadian | Sesuai layar tujuan | — | — |

**Tombol Ajukan mati, bukan disembunyikan**, selama masih ada penghalang. Menyembunyikannya membuat
petugas bertanya-tanya di mana tombolnya; mematikannya beserta daftar penghalang di atasnya
menjelaskan sendiri apa yang harus dikerjakan lebih dulu.

Layar persetujuan bagi **pimpinan keuangan** memakai layar yang sama, dengan tombol berbeda:
`[Setujui Penutupan]` dan `[Tolak]`, dijaga `Period : Approve`. Bila pembuka layar adalah orang
yang mengajukan, kedua tombol itu **mati** beserta keterangan "Penutupan tidak dapat disetujui
oleh yang mengajukan."

### 11.4 Tutup Tahun

```
+--------------------------------------------------------------+
| [Badan Hukum v]  [Tahun Buku: 2026 v]          [Pratinjau]   |
+--------------------------------------------------------------+
| Pendapatan                                                   |
|   4-1001 Pendapatan Rawat Jalan          Rp   800.000.000    |
|   4-1002 Pendapatan Rawat Inap           Rp   500.000.000    |
| Beban                                                        |
|   5-1001 Beban Obat                      Rp   300.000.000    |
|   5-2001 Beban Gaji                      Rp   600.000.000    |
+--------------------------------------------------------------+
| Laba tahun 2026                          Rp   400.000.000    |
| Dipindahkan ke: 3-3001 Laba Ditahan                          |
+--------------------------------------------------------------+
|                            [Susun Jurnal Penutup]            |
+--------------------------------------------------------------+
```

| Wilayah | Isinya | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| Pratinjau | Saldo per akun dan selisihnya | `GET /year-end-closing/preview` | `YearEndClosing : Read` | "Tidak ada saldo yang perlu ditutup." | "Masih ada periode yang belum ditutup." beserta daftarnya |
| Baris tujuan | Akun laba ditahan | Dari respons yang sama | `YearEndClosing : Read` | "Akun laba ditahan belum ditetapkan" + tautan ke Pengaturan Akuntansi | — |
| Tombol Susun | — | `POST /year-end-closing/generate` | `YearEndClosing : Generate` | Mati bila pratinjau kosong atau akun belum ditetapkan | Pesan galat |

**Pratinjau tidak membuat apa pun.** Ini penting dinyatakan di layar, karena tutup tahun terasa
menakutkan bagi petugas; menekan Pratinjau harus aman sepenuhnya.

## 12. Aksi per peran Phase 2

| Aksi | Viewer | Staff | Approver | Manager | Director | Auditor | Administrator |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Melihat kotak masuk kejadian | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Coba ulang kejadian gagal | | | | ✓ | | | ✓ |
| Abaikan kejadian gagal | | | | ✓ | | | |
| Kelola aturan posting dan jenis kejadian | | | | ✓ | | | ✓ |
| Kelola template jurnal berulang | | ✓ buat | | ✓ | | | ✓ |
| Ajukan penutupan periode | | | | ✓ | | | |
| **Setujui atau tolak penutupan** | | | | | **✓** | | |
| Susun jurnal penutup tahun | | | | ✓ | | | |

Tombol yang tidak boleh ditekan peran tertentu **dimatikan, bukan disembunyikan**, mengikuti pola
yang sudah dipakai sebelas layar MVP.

## 13. Redux slice Phase 2

Melanjutkan bagian 3. Enam slice bertambah, mengikuti pola penamaan yang sudah ada.

| Slice | Isi | Kapan dikosongkan |
|---|---|---|
| `accounting-event-slice.jsx` | Daftar kejadian, rincian, ringkasan jumlah | Saat badan hukum berganti |
| `accounting-posting-rule-slice.jsx` | Daftar dan rincian aturan posting | Saat badan hukum berganti |
| `accounting-event-type-slice.jsx` | Master jenis kejadian | Saat keluar dari modul |
| `accounting-recurring-journal-slice.jsx` | Template beserta riwayat penerbitannya | Saat badan hukum berganti |
| `accounting-period-closing-slice.jsx` | Daftar periksa penutupan | Setiap kali layar dibuka — **tidak boleh dari cache** |
| `accounting-year-end-slice.jsx` | Pratinjau tutup tahun | Setiap kali Pratinjau ditekan |

**Dua slice terakhir sengaja tidak memakai cache.** Daftar periksa penutupan dan pratinjau tutup
tahun adalah angka yang dihitung saat diminta; menampilkan angka lama membuat petugas mengambil
keputusan penutupan berdasarkan keadaan yang sudah berubah.

## 14. Yang tidak dikerjakan frontend pada Phase 2

| Yang ditolak | Alasan |
|---|---|
| Layar penerbitan kejadian | Accounting adalah **penerima**, bukan penerbit. Yang menerbitkan adalah Finance |
| Layar pencarian pasien dari kejadian | Dilarang `ACC-DEC-056`. Penelusuran dilakukan dengan membuka modul asalnya |
| Pemberitahuan langsung lewat SignalR | Ditolak `ACC-DEC-057`. Memakai penanda jumlah pada menu |
| Layar Laba Rugi dan Neraca | Tetap ditunda `ACC-DEC-030`; bukan bagian dari keempat slice Phase 2 |
| Komponen tabel baris jurnal tandingan untuk template | Form Jurnal Berulang memakai ulang komponen Form Jurnal |
