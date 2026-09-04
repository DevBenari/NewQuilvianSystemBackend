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
