# Accounting — Frontend Architecture

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` |
| Revision | `7` — 28 September 2026, **approved Rizki (`GATE-DESAIN-0928`)**: 11.2 mengikuti `ACC-DEC-097` (Isi Pesan Asli sebagai tabel berlabel) dan layar pesan saldo `FE-ACC-P2-015`; 11.3 penghalang keempat rekonsiliasi saldo subledger (`ACC-DEC-107`, `111`); 11.6 pemilih periode dan sisi subledger (`ACC-DEC-109`, `110`, `112`..`114`). Sebelumnya `6` — 14 September 2026, `ACC-DEC-076` menutup `ACC-GAP-013` (butir penghalang baru `Evaluated`, toleransi nol) dan `ACC-DEC-077` mencabut peringatan `SUSPENSE_ACCOUNT_BALANCE`. Sebelumnya `5` — 10 September 2026, `ACC-DEC-071` menutup `DEC-ACC-P2-011` (sumber saldo subledger) dan membuka `ACC-GAP-013`. Sebelumnya `4`, penyelarasan bagian 9-14 dengan backend Phase 2 yang sudah berdiri |
| Status | `draft` — approval adalah tindakan manusia, belum diberikan |
| Cakupan | **Dua bagian.** Bagian 1-8: MVP tulang punggung akuntansi (`ACC-DEC-009`). Bagian 9-14: Phase 2 (`ACC-PH-006`), mencakup `ACC-DEC-044` sampai `ACC-DEC-066` |
| Frontend SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) — baseline **saat dokumen ini disusun**. Baseline blueprint kini `31a82c8` (`QuilvianIntegrationFrontend`); kutipan di bawah tetap berlaku, lihat `evidence/02-frontend-rebaseline-impact-scan.md` |
| Masukan | `02-backend-architecture.md@3`, `contracts/api-contract.md@ACC-API-0.1` (bagian 1-8); `contracts/api-contract.md@ACC-API-0.9` (bagian 9-14), `roadmap/frontend-roadmap-phase2.md@3`, `00-interview-decisions.md@6` |
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

#### Amandemen 29 September 2026 — **approved** Rizki (`GATE-DESAIN-0929`) (`ACC-DEC-116`..`121`)

**Gap yang ditemukan saat audit.** Baris `Draft` (Ubah, Hapus) dan `Rejected` (Sunting kembali) pada
tabel di atas **belum pernah dibangun**: `FE-ACC-007` hanya memetakan lima tombol (Ajukan, Setujui,
Tolak, Sahkan, Balik), dan thunk `deleteJournal` tidak dipakai layar mana pun. Hari ini jurnal
draft hanya dapat diubah lewat alamat langsung dan tidak dapat dihapus lewat layar. Amandemen ini
menjadikan keduanya bagian Rincian Jurnal — tetap dari `AvailableActions`, bukan dihitung layar.

| Tombol | Muncul bila `AvailableActions` memuat | Yang terjadi |
|---|---|---|
| **Ubah** | `update` | Membuka form ubah jurnal yang sudah ada (`/corporate/accounting/journals/[slug]/update`, `FE-ACC-006`) |
| **Hapus** | `delete` | Dialog konfirmasi, lalu `DELETE /journals/{id}`. Berhasil → kembali ke Daftar Jurnal dan pesan backend tampil sebagai toast. Gagal → pesan backend apa adanya |

**Jurnal hasil kejadian** (`sourceAccountingEventNumber` terisi):

| Hal | Isi |
|---|---|
| Baris rincian **Asal** | "Kejadian EVT-UJI-034A" — tautan ke Rincian Kejadian bila berhak `AccountingEvent : Read`, teks biasa bila tidak. Jurnal manual: "Jurnal manual" |
| Ubah | Tidak muncul — backend tidak mengirim `update` (`ACC-DEC-119`) |
| Hapus | Muncul pada `Draft` dan `Rejected`. Pesan dialognya menyebut akibat: "Kejadian EVT-UJI-034A akan kembali berstatus Gagal dan menahan tutup bulan sampai dicoba ulang atau diabaikan." (`ACC-DEC-116`, `118`) |

Skema wilayah atas Rincian Jurnal hasil kejadian `Draft`:

```text
┌ JU/2031/01/00003 · Draft ───────────────── [Hapus] [Ajukan] ┐
│ Asal: Kejadian EVT-UJI-034A ↗                                  │
│ Tanggal 20 Jan 2031 · Jenis JU · Periode 2031-01               │
└────────────────────────────────────────────────────────────────┘
```

`DEV_DISCRETION`: letak kedua tombol (bersama tombol aksi yang sudah ada atau di kepala halaman),
rupa, ikon, dan rupa tautan Asal. Terkunci: tombol hanya dari `AvailableActions`, bunyi akibat pada
dialog Hapus jurnal hasil kejadian, dan tautan Asal dijaga `AccountingEvent : Read`.

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

Melanjutkan letak menu yang diputuskan `ACC-FE-001`. **Tujuh** butir bertambah, seluruhnya di
dalam menu Accounting yang sudah ada.

Butir ketujuh menyusul lewat amandemen roadmap 9 September 2026 (`ACC-DEC-066`). Penanda control
account `ACC-DEC-064` **tidak** menambah butir menu — ia menumpang layar COA yang sudah ada.

| # | Butir menu | Tingkat | Induk | Route | Layar yang dituju | Hak akses penjaga |
|---:|---|:---:|---|---|---|---|
| 1 | Kotak Masuk Kejadian | 2 | Accounting | `/accounting/accounting-events` | Kotak Masuk Kejadian | `AccountingEvent : Read` |
| 2 | Aturan Posting | 3 | Accounting › Master Data | `/accounting/posting-rules` | Aturan Posting | `PostingRule : Read` |
| 3 | Jenis Kejadian | 3 | Accounting › Master Data | `/accounting/event-types` | Jenis Kejadian | `EventType : Read` |
| 4 | Jurnal Berulang | 2 | Accounting | `/accounting/recurring-journals` | Jurnal Berulang | `RecurringJournal : Read` |
| 5 | Tutup Tahun | 2 | Accounting | `/accounting/year-end-closing` | Tutup Tahun | `YearEndClosing : Read` |
| 6 | Pengaturan Akuntansi | 3 | Accounting › Master Data | `/accounting/configuration` | Pengaturan Akuntansi | `AccountingConfiguration : Update` |
| 7 | Rekonsiliasi Control Account | 2 | Accounting | `/accounting/reconciliation` | Rekonsiliasi Control Account | `AccountingReconciliation : Read` |

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
| 20 | Daftar Periksa Penutupan | **Tiga** penghalang — **empat** sejak amandemen 28 September 2026, ditambah rekonsiliasi saldo subledger — beserta peringatannya, masing-masing dengan keadaan sudah/belum diperiksa, beserta tombol Ajukan | `ACC-P2-S3` |
| 21 | Tutup Tahun | Pratinjau perhitungan dan tombol Susun Jurnal Penutup | `ACC-P2-S4` |
| 22 | Pengaturan Akuntansi | Menetapkan akun laba ditahan per badan hukum; layar kecil | `ACC-P2-S4` |
| 23 | Penanda Control Account | **Bukan layar baru.** Satu kotak centang pada Form Akun dan satu kolom penanda pada tabel COA yang sudah ada (`ACC-DEC-064`) | `ACC-P2-S5` |
| 24 | Rekonsiliasi Control Account | Saldo buku besar berdampingan dengan saldo subledger, beserta selisihnya (`ACC-DEC-066`), **per periode akuntansi** (`ACC-DEC-114`) | `ACC-P2-S5` |

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
| Tombol Coba Ulang | — | `POST /accounting-events/{id}/retry` | `AccountingEvent : Retry` | Mati bila status bukan Gagal **atau Tertahan** (`ACC-DEC-092`, 24 September 2026) | Pesan galat, status tidak berubah |
| Tombol Abaikan | Modal berisi kolom alasan **wajib** | `PATCH /accounting-events/{id}/ignore` | `AccountingEvent : Ignore` | Mati bila status bukan Gagal | Pesan galat |
| Jurnal yang dihasilkan | Tautan ke Rincian Jurnal | Dari respons yang sama | `Journal : Read` | "(belum ada)" — wajar untuk Tertahan dan Gagal. Untuk pesan saldo berstatus **Tercatat**: "Tidak dijurnal — pesan saldo subledger" (`ACC-DEC-087`, `FE-ACC-P2-015`) | — |
| Riwayat percobaan | Tabel percobaan | Dari respons yang sama | `AccountingEvent : Read` | "Belum pernah dicoba." | — |
| Isi pesan asli | **Tabel dua kolom berlabel** "Keterangan — Isi yang dikirim Finance" (`ACC-DEC-097`), tersembunyi secara bawaan dengan tombol Lihat/Sembunyikan | Dari respons yang sama (`RawPayload`) | `AccountingEvent : Read` | — | **Isi yang bukan JSON sah tampil apa adanya sebagai teks** — pesan asli tidak pernah hilang dari layar |

**Tombol Abaikan wajib memunculkan modal konfirmasi yang menyebut bahwa tindakan ini tidak dapat
dibatalkan**, karena memang tidak dapat. Kolom alasan tidak boleh kosong.

**Isi Pesan Asli sebagai tabel berlabel — `ACC-DEC-097`, 28 September 2026.** Baris sebelumnya
menulis "teks mentah"; layar yang dibangun `FE-ACC-P2-012` menampilkannya sebagai tabel, dan owner
memutuskan **dokumen yang mengikuti layar**. Aturannya:

| Bidang pesan | Bunyi di tabel |
|---|---|
| Tanggal dan waktu | Diformat, contoh "30 November 2026" |
| Nilai | Rupiah, contoh "Rp 425.000.000" |
| Mata uang | "Rupiah (IDR)" |
| Badan hukum | **Nama** badan hukum, bukan `Guid` |
| Nomor teknis (`CorrelationId`, `CausationId`, dan sejenisnya) | Nilai apa adanya dengan keterangan "(untuk tim IT)" |
| Rincian saldo (`SubledgerBalance`) | Periode dan kode akun kontrol, berlabel Indonesia |

Seluruh bidang pesan tetap tampil, sehingga `FR-P2-006` ("pesan asli disimpan utuh dan dapat
dilihat kembali") tetap terpenuhi. **Contoh:** pesan bertanggal akuntansi 30 November 2026 senilai
425.000.000 tampil sebagai baris "Tanggal Akuntansi — 30 November 2026" dan "Nilai —
Rp 425.000.000", bukan `{ "AccountingDate": "2026-11-30", "Amount": 425000000.00, … }`.
Letak, gaya, dan komponen tabelnya `DEV_DISCRETION` mengikuti komponen rincian yang sudah ada.

### 11.3 Daftar Periksa Penutupan

```
+--------------------------------------------------------------+
| Tutup Periode 2026-09              [Ajukan Penutupan]        |
+--------------------------------------------------------------+
| ! Daftar periksa masih sebagian - 7 butir belum dapat        |
|   diperiksa. Lihat keterangan tiap baris.                    |
+--------------------------------------------------------------+
| PENGHALANG (harus nol)                                       |
|  x 3 jurnal belum disahkan                    [Lihat]        |
|  ? Kejadian keuangan gagal                                   |
|      Belum dapat diperiksa: kotak masuk kejadian             |
|      keuangan belum berdiri.                                 |
|  ? Shift kasir belum ditutup                                 |
|      Belum dapat diperiksa: kejadian CASH_SHIFT_CLOSED       |
|      belum mengalir.                                         |
+--------------------------------------------------------------+
| PERINGATAN (boleh dilewati)                                  |
|  v Tidak ada jurnal yang belum seimbang                      |
|  ? Penyusutan belum dijalankan                               |
|      Belum dapat diperiksa: penjadwal jurnal berulang        |
|      belum berdiri (BE-ACC-P2-008).                          |
+--------------------------------------------------------------+
```

Tiga lambang, tiga arti yang **tidak boleh** tertukar: `x` sudah diperiksa dan bermasalah,
`v` sudah diperiksa dan bersih, `?` belum dapat diperiksa sama sekali.

| Wilayah | Isinya | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| Spanduk kelengkapan | Muncul **hanya** bila `IsComplete` bernilai `false`, memuat `NotYetAvailableCount` | `GET /periods/{id}/closing-checklist` | `AccountingPeriod : Read` | Disembunyikan saat `IsComplete` benar | Ikut hilang bersama isi layar |
| Penghalang | **Tiga** penghalang beserta jumlah dan keadaannya | Respons yang sama, bidang `Blockers` | `AccountingPeriod : Read` | "Tidak ada penghalang." Tombol Ajukan menyala | "Daftar periksa gagal dimuat." |
| Peringatan | Seluruh peringatan beserta jumlah dan keadaannya | Respons yang sama, bidang `Warnings` | `AccountingPeriod : Read` | "Tidak ada peringatan." | — |
| Riwayat penutupan | Ajukan, setujui, tolak - berurut menaik beserta pelaku, waktu, dan catatannya | `GET /periods/{id}/closing-history` | `AccountingPeriod : Read` | "Periode ini belum pernah diajukan." | "Riwayat gagal dimuat." Isi layar lain tetap tampil |
| Tombol Ajukan | — | `POST /periods/{id}/submit-closing` | `AccountingPeriod : Close` | **Mati** selama masih ada penghalang | Pesan galat |
| Tautan Lihat | Membuka daftar tersaring | Layar Jurnal atau Kotak Masuk Kejadian | Sesuai layar tujuan | — | — |

**Route grup ini `periods`, bukan `accounting-periods`, dan hak aksesnya `AccountingPeriod`,
bukan `Period`.** Revisi sebelumnya menyalin keduanya dari bagian Phase 2 `api-contract.md` yang
saat itu keliru; kontraknya sudah diperbaiki 10 September 2026. Nama `Period` menghasilkan `403`
permanen yang **tidak dapat diperbaiki dari layar Akses Role**, karena hak yang dicari tidak
pernah ada untuk diberikan.

**Tombol Ajukan mati, bukan disembunyikan**, selama masih ada penghalang. Menyembunyikannya membuat
petugas bertanya-tanya di mana tombolnya; mematikannya beserta daftar penghalang di atasnya
menjelaskan sendiri apa yang harus dikerjakan lebih dulu.

Layar persetujuan bagi **pimpinan keuangan** memakai layar yang sama, dengan tombol berbeda:
`[Setujui Penutupan]` dan `[Tolak]`, dijaga `AccountingPeriod : Approve`. Bila pembuka layar
adalah orang yang mengajukan, kedua tombol itu **mati** beserta keterangan "Penutupan tidak dapat
disetujui oleh yang mengajukan."

#### Tiga penghalang, bukan dua

`ACC-DEC-051` menetapkan dua penghalang. `ACC-DEC-065` memperluasnya menjadi **tiga** pada
9 September 2026, bertambah **shift kasir yang belum ditutup**.

| # | Penghalang | `Code` | Keadaan hari ini | Sumber |
|---:|---|---|---|---|
| 1 | Jurnal belum disahkan | `UNPOSTED_JOURNALS` | `Evaluated` — **satu-satunya yang sudah dapat diperiksa** | `ACC-DEC-051` |
| 2 | Kejadian keuangan gagal | `FAILED_EVENTS` | `NotYetAvailable` — kotak masuk kejadian belum berdiri | `ACC-DEC-051` |
| 3 | Shift kasir belum ditutup | `OPEN_CASH_SHIFTS` | `NotYetAvailable` — kejadian `CASH_SHIFT_CLOSED` belum mengalir | `ACC-DEC-065` |

Penghalang ketiga ada karena kas datang **per shift**: shift yang melewati pergantian bulan dan
belum ditutup membuat kasnya tidak pernah sampai ke buku besar, sementara periodenya tetap dapat
ditutup dengan angka kas yang belum lengkap. Penegakannya lewat kejadian, bukan dengan membaca
tabel Finance — batas `ACC-DEC-061` tetap utuh.

#### Penghalang keempat — rekonsiliasi saldo subledger *(28 September 2026, `GATE-DESAIN-0928`)*

| # | Penghalang | `Code` | Keadaan | Sumber |
|---:|---|---|---|---|
| 4 | Rekonsiliasi saldo subledger belum bersih | `SUBLEDGER_RECONCILIATION` | `NotYetAvailable` sebelum titik mulai rekonsiliasi badan hukum itu; `Evaluated` sesudahnya | `ACC-DEC-076`, `107`, `111` |

**Titik mulai** adalah periode saldo subledger pertama yang diterima dari Finance (`ACC-DEC-107`).
Sebelum itu butir ini "belum dapat diperiksa" dan **tidak menahan**; alasannya sudah siap dibaca:
"Finance belum pernah mengirim saldo subledger untuk badan hukum ini." atau "Rekonsiliasi saldo
subledger berlaku mulai periode 2026-11." Sesudah titik mulai, `Count` = jumlah control account
yang menahan, dan `Message` memuat rinciannya, contoh "1 control account belum menerima saldo
subledger, 1 berselisih."

| Wilayah | Isinya | Sumber data | Hak akses penjaga |
|---|---|---|---|
| Baris penghalang keempat | Dirender seperti penghalang lain — layar **tidak** menanam jumlah butir (aturan ke-5 di bawah) | `GET /periods/{id}/closing-checklist` | `AccountingPeriod : Read` |
| Tautan Lihat pada baris itu | Membuka layar Rekonsiliasi Control Account (bagian 11.6) **untuk periode yang sama** | — | `AccountingReconciliation : Read`; tanpa hak itu, tautan tidak ditampilkan dan pesannya tetap terbaca |
| Penolakan saat Ajukan | `409` dengan kalimat yang sama dengan `Message` butir itu | `POST /periods/{id}/submit-closing` | `AccountingPeriod : Close` |

**Tutup Permanen juga dapat ditolak** karena rekonsiliasi belum bersih (`ACC-DEC-111`), dari layar
Periode Akuntansi MVP yang sudah ada. Layar itu cukup menampilkan pesan `409` apa adanya — nol
perubahan rupa. Tidak ada tombol pengecualian di layar mana pun (`ACC-DEC-113`).

#### Keadaan `NotYetAvailable` — bedanya wajib terbaca

Setiap butir daftar periksa membawa `State`, bernilai `Evaluated` atau `NotYetAvailable`, dan
`UnavailableReason` yang terisi hanya pada yang kedua. Akar responsnya membawa `IsComplete` dan
`NotYetAvailableCount`.

**Butir `NotYetAvailable` selalu mengembalikan `Count` bernilai `0`.** Nol di sana berarti "belum
ada yang memeriksa" — **bukan** "sudah diperiksa dan bersih".

> **Layar WAJIB membedakan keduanya.** Bila butir `NotYetAvailable` dirender sama seperti butir
> `Evaluated` bernilai nol, layar berbohong: petugas melihat daftar penghalang yang tampak bersih,
> menekan Ajukan dengan yakin, dan menutup periode padahal **tidak ada yang pernah memeriksa
> shift kasirnya**. Ini bukan cacat tampilan — ini angka kas yang tidak pernah sampai ke buku
> besar, ditutup permanen.

Yang mengikat implementasi:

1. Butir `NotYetAvailable` dirender dengan lambang dan warna **berbeda** dari butir bersih, dan
   `UnavailableReason`-nya ditampilkan apa adanya — kalimatnya sudah siap dibaca pengguna.
2. Butir `NotYetAvailable` **tidak menampilkan angka**. Menulis "0 shift kasir belum ditutup"
   adalah persis kebohongan yang dilarang di atas.
3. Selama `IsComplete` bernilai `false`, spanduk kelengkapan tampil di **atas** daftar, memuat
   `NotYetAvailableCount`. Menaruhnya di bawah membuatnya terlewat.
4. Tombol Ajukan **tetap menyala** bila `CanSubmitClosing` benar walaupun `IsComplete` masih
   `false` — backend memang mengizinkannya. Yang wajib menyertainya adalah keterangan bahwa
   sebagian pemeriksaan belum berjalan, supaya keputusannya diambil sadar.
5. Jumlah butir **tidak boleh** ditanam di kode layar. `Blockers` dan `Warnings` selalu berisi
   seluruh butirnya apa pun keadaannya, jadi layar cukup merender apa yang datang. Saat gelombang
   `P2-1` menyalakan sebuah butir, bentuk responsnya tidak berubah dan layar ini tidak perlu
   disentuh.

Penghalang shift kasir akan **kembali** sebagai `NotYetAvailable` selama gelombang `P2-1` belum
dibangun. Itu keadaan yang benar, bukan cacat yang perlu dilaporkan.

#### Enam peringatan, bukan lima

`ACC-DEC-051` menyebut lima peringatan. `ACC-DEC-070` memperluasnya menjadi **enam** pada
10 September 2026, bertambah **kejadian keuangan tertahan** — yang sejak awal memang dikembalikan
`AccPeriodClosingService` berkode `HELD_EVENTS`. Untuk selisih ini pun yang benar adalah kode.

| # | Peringatan | `Code` | Keadaan hari ini | Sumber |
|---:|---|---|---|---|
| 1 | Jurnal belum seimbang | `UNBALANCED_JOURNALS` | `Evaluated` — **satu-satunya yang sudah dapat diperiksa** | `ACC-DEC-051` |
| 2 | Kejadian keuangan tertahan | `HELD_EVENTS` | `NotYetAvailable` | **`ACC-DEC-070`** |
| 3 | Integrasi belum cocok | `INTEGRATION_MISMATCH` | `NotYetAvailable` | `ACC-DEC-051` |
| 4 | Penyusutan belum dijalankan | `DEPRECIATION_NOT_RUN` | `NotYetAvailable` — menunggu `BE-ACC-P2-008` | `ACC-DEC-051` |
| 5 | ~~Saldo tertinggal di akun sementara~~ | ~~`SUSPENSE_ACCOUNT_BALANCE`~~ | **DICABUT dari requirement** `ACC-DEC-077`, 14 September 2026 — bertentangan dengan `ACC-DEC-046` yang melarang akun sementara. Source `b3ab542e` **masih** mengembalikannya sebagai `NotYetAvailable`; pencabutannya di source menyusul lewat task tersendiri | `ACC-DEC-051`, dicabut `ACC-DEC-077` |
| 6 | Selisih saldo awal dan saldo akhir | `OPENING_CLOSING_MISMATCH` | `NotYetAvailable` — **definisinya menunggu keputusan owner** | `ACC-DEC-051` |

**Keadaan requirement per 14 September 2026 (`ACC-DEC-077`):** peringatan yang berlaku tinggal
**lima**. `DEPRECIATION_NOT_RUN` dan `OPENING_CLOSING_MISMATCH` tetap tercantum, tetapi definisi
keduanya **belum diputuskan owner** dan tidak boleh diisi tebakan. Keterangan "menunggu
`BE-ACC-P2-008`" pada butir 4 sudah basi — penjadwalnya berdiri sejak 10 September 2026; yang
menahan sekarang adalah definisi "penyusutan", bukan penjadwalnya.

Kejadian tertahan tetap **peringatan, bukan penghalang**: ia menunggu pekerjaan pemetaan yang
wajar dijadwalkan, bukan gangguan yang menuntut tindakan segera. Alasan yang sama dipakai
`ACC-DEC-057` untuk tidak menghitungnya pada penanda angka di menu (bagian 9).

Sekalipun begitu ia pantas muncul saat tutup bulan, karena kejadian tertahan adalah kejadian
keuangan yang **tidak pernah menjadi jurnal** — transaksi yang belum sampai ke buku besar.

**Tiga penghalang ditambah enam peringatan berarti `NotYetAvailableCount` bernilai `7` hari ini**
(dua penghalang dan lima peringatan), dan `IsComplete` bernilai `false`. Angka itu **tetap tidak
boleh ditanam di kode layar** — lihat aturan ke-5 di atas. Ia dicantumkan di sini hanya supaya
pembaca dokumen dapat mencocokkan dengan apa yang tampil di layar saat menguji.

> **`ACC-GAP-013` — penghalang keempat menyusul, dan mesinnya belum siap menampungnya.**
>
> `ACC-DEC-071` (10 September 2026) menetapkan rekonsiliasi control account menjadi **bagian
> proses penutupan periode**: bila saldo subledger belum diterima, atau hasilnya belum cocok,
> penutupan **tidak boleh dianggap selesai**. Artinya daftar periksa ini akan bertambah dari
> **tiga** penghalang menjadi **empat** saat `BE-ACC-P2-014` berdiri.
>
> **Layar tidak perlu diubah.** Aturan ke-5 di atas sudah melarang menanam jumlah butir di kode,
> jadi penghalang keempat akan terender sendiri. Yang perlu diubah hanyalah kalimat "tiga
> penghalang" pada dokumen ini, kontrak, dan acceptance `FE-ACC-P2-001` — pada saat itu tiba,
> bukan sekarang.
>
> **Yang belum beres ada di mesinnya, bukan di layar.** `CanSubmitClosing` hari ini dihitung
> `penghalangAktif == 0`, dan `penghalangAktif` **hanya** menghitung butir ber-`State = Evaluated`
> yang `Count`-nya lebih dari nol. Penghalang berkeadaan `NotYetAvailable` **tidak menahan apa
> pun** — itu memang disengaja, dan itulah sebabnya spanduk kelengkapan ada.
>
> Tetapi `ACC-DEC-071` menuntut keadaan "saldo subledger **belum diterima**" **menahan**
> penutupan. Kalau butir itu dimodelkan sebagai `NotYetAvailable`, ia tidak akan menahan apa-apa,
> dan keputusan owner tidak tertegakkan.
>
> **Jalan keluar yang disarankan — tanpa mengubah mesinnya sama sekali:** modelkan butir keempat
> sebagai butir **`Evaluated`** yang `Count`-nya adalah *berapa control account yang saldo
> subledger-nya belum diterima untuk periode ini*. Nol berarti semua sudah diterima; lebih dari
> nol menahan penutupan, persis seperti `UNPOSTED_JOURNALS`. Pemeriksaannya memang **dapat**
> dijalankan begitu `BE-ACC-P2-014` ada — yang belum ada hanya datanya, dan "data belum datang"
> justru temuan yang sah, bukan alasan untuk tidak memeriksa.
>
> Alternatifnya mengubah `CanSubmitClosing` agar sebagian penghalang `NotYetAvailable` ikut
> menahan. Itu menambah aturan baru pada mesin yang sudah bekerja, demi hasil yang sama.
>
> ~~Satu hal masih perlu diputuskan owner: apakah butir keempat ini **butir baru**, atau peringatan
> `INTEGRATION_MISMATCH` yang sudah ada **dinaikkan** menjadi penghalang.~~
>
> **DIPUTUSKAN — `ACC-DEC-076`, 14 September 2026. `ACC-GAP-013` ditutup.** Owner memilih
> **butir baru ber-`State = Evaluated`**, persis jalan keluar di atas. Saldo subledger yang belum
> lengkap **maupun** tidak cocok sama-sama menahan penutupan, dengan **toleransi selisih nol**.
> Contoh: selisih Rp 500 antara saldo Kas Kasir di buku besar dan di subledger tetap menahan.
> Implementasinya menunggu Wave D (rekonsiliasi subledger), karena datanya belum ada.
>
> **Dirancang dan approved 28 September 2026 (`GATE-DESAIN-0928`)** — lihat *Penghalang keempat* di atas dan
> `02-backend-architecture.md` bagian 23. Mesin `CanSubmitClosing` memang tidak diubah; yang
> ditambahkan hanya titik mulai (`ACC-DEC-107`), supaya butir ini tidak menahan setiap tutup bulan
> sebelum Finance mengirim saldo apa pun.

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

### 11.5 Penanda Control Account pada layar COA

`ACC-DEC-064` mengunci **empat kelompok akun** dari jurnal manual umum: **Kas Kasir, Kas Kecil,
Piutang, dan Hutang**. Pencatatan ke keempatnya hanya sah lewat kejadian akuntansi atau subledger.
Jurnal manual **tetap boleh** menyentuh akun non-control — penyesuaian, akrual, koreksi beban, dan
jurnal penutup.

**Ini bukan layar baru.** Ia tiga sentuhan kecil pada layar MVP yang sudah ada, dikerjakan
`FE-ACC-P2-007`:

| Tempat | Yang ditambahkan | Hak akses penjaga |
|---|---|---|
| Form Akun (COA) | Satu kotak centang "Control account" beserta penjelasan singkat maknanya | `ChartOfAccount : Update` |
| Tabel COA | Satu kolom penanda, supaya terbaca sekilas mana yang terkunci | `ChartOfAccount : Read` |
| **Pemilih akun pada Form Jurnal** | Akun control **dimatikan atau disembunyikan** dari pemilih, disertai keterangan kenapa | `Journal : Create` |

**Sentuhan ketiga yang paling menentukan.** Tanpa itu petugas baru tahu akunnya terlarang sesudah
menekan Simpan dan ditolak `422` — sesudah mengisi seluruh baris jurnal. Penjagaan di sisi backend
tetap ada dan tidak digantikan; yang ditambahkan di sini adalah memberitahu petugas **sebelum** ia
mengisi, bukan sesudah.

Penandanya disimpan sebagai kolom karena **tidak dapat diturunkan** dari data lain: `Kas Kasir`
dan `Piutang` sama-sama berjenis `Asset`, tetapi tidak setiap akun `Asset` adalah control account.
Layar membacanya apa adanya dari DTO akun, tanpa menyimpulkan sendiri dari `AccountType`.

### 11.6 Rekonsiliasi Control Account

`ACC-DEC-066` menempatkan rekonsiliasi control account di Phase 2. Isinya tiga: perbandingan saldo
buku besar, perbandingan saldo subledger, dan laporan selisih. Sisi buku besar dikerjakan
`FE-ACC-P2-008` ✅; sisi subledger dan selisih dikerjakan **`FE-ACC-P2-016`**.

> **28 September 2026 — `ACC-DEC-114`, approved `GATE-DESAIN-0928`.** Isian "Saldo per tanggal" pada layar
> yang sudah berdiri **diganti pemilih periode akuntansi**. Saldo subledger dari Finance terikat
> periode (`ACC-DEC-071`), sehingga perbandingan hanya bermakna per periode. Sketsa di bawah
> menggantikan sketsa lama.

```
+----------------------------------------------------------------------+
| Rekonsiliasi Control Account          Periode: [ September 2026 v ]  |
+----------------------------------------------------------------------+
| ! Belum bersih - 1 control account belum menerima saldo subledger,   |
|   1 saldonya bertanggal cut-off bukan akhir periode, 1 berselisih.   |
|   Buku besar per 30/09/2026 - dihitung 03/10/2026 09.15              |
+----------------------------------------------------------------------+
| Akun              | Buku Besar  | Subledger       | Selisih  | Keadaan |
|-------------------|-------------|-----------------|----------|---------|
| 1-1101 Kas Kasir  | 25.000.000  | 25.000.000      | 0        | Cocok   |
| 1-1102 Kas Kecil  |          0  | belum diterima  | -        | Belum   |
| 1-1201 Piutang    | 80.000.000  | 78.500.000      | -        | Cut-off |
|                   |             | cut-off 15/09   |          | salah   |
| 2-1101 Utang Sup. | 30.000.000  | 29.999.500      | 500      | Selisih |
+----------------------------------------------------------------------+
```

| Wilayah | Isinya | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| Pemilih periode | Periode akuntansi badan hukum utama | `GET /periods` yang sudah ada — dipakai ulang lewat slice periode, bukan panggilan baru | `AccountingPeriod : Read` | "Belum ada periode akuntansi. Bangkitkan periode lebih dahulu." | "Daftar periode gagal dimuat." |
| Spanduk keadaan | `ReconciliationState` beserta `StateMessage`, tanggal batas buku besar, dan `EvaluatedAt` | `GET /reconciliation/subledger-comparison` | `AccountingReconciliation : Read` | — | Ikut hilang bersama tabel |
| Tabel | Satu baris per akun: kode dan nama, Buku Besar (`GlBalance`), Subledger (`SubledgerBalance` beserta cut-off dan versinya), Selisih (`Difference`), Keadaan (`ItemStatus`) | Respons yang sama | `AccountingReconciliation : Read` | "Belum ada control account yang ditandai pada badan hukum ini." | "Rekonsiliasi gagal dimuat." + tombol Coba Lagi |
| Tautan ke pesan saldo | Nomor kejadian saldo membuka Rincian Kejadian | `SubledgerAccountingEventId` | `AccountingEvent : Read` — tanpa hak itu nomornya tampil sebagai teks biasa | — | — |

**Tiga keadaan laporan wajib terbaca bedanya.**

| `ReconciliationState` | Yang dibaca petugas | Contoh bunyi |
|---|---|---|
| `BelumBerlaku` | Rekonsiliasi belum menahan tutup bulan periode ini | "Rekonsiliasi belum berlaku untuk periode ini. Finance belum pernah mengirim saldo subledger." — atau "… berlaku mulai periode November 2026." |
| `Bersih` | Semua akun wajib cocok | "Seluruh control account cocok dengan saldo subledger." |
| `BelumBersih` | Penutupan tertahan | `StateMessage` apa adanya, dengan jumlah akun per keadaan |

**Empat keadaan per akun, dan larangan yang tetap berlaku.**

| `ItemStatus` | Kolom Subledger | Kolom Selisih |
|---|---|---|
| `Cocok` | Nilai, cut-off, versi | `0` |
| `Berselisih` | Nilai, cut-off, versi | Nilai selisih; positif berarti buku besar lebih besar |
| `BelumDiterima` | **"Belum diterima"** — tidak pernah `Rp 0` | **"-"** — tidak pernah `0` |
| `CutOffBukanAkhirPeriode` | Nilai beserta keterangan tanggal cut-off-nya | **"-"** — belum dapat dibandingkan |

Akun ber-`IsBlocking` benar ditandai berbeda dari yang tidak; akun yang tidak wajib (`IsRequired`
salah — akun induk, akun nonaktif bersaldo nol) tetap tampil tetapi tidak ditandai menahan.

> **Kolom yang belum ada datanya ditampilkan, bukan disembunyikan.** Menyembunyikan kolom subledger
> dan selisih membuat laporan rekonsiliasi **terbaca seolah sudah cocok** — pembacanya menyimpulkan
> selisihnya nol, padahal tidak ada yang pernah dibandingkan. Pada layar rekonsiliasi itu kesalahan
> yang paling mahal.

Ini pola yang sama dengan `NotYetAvailable` pada 11.3: **"belum diperiksa" dan "sudah diperiksa,
nol" wajib terbaca bedanya.** Dua layar, satu kaidah.

Layar ini **tidak memakai cache**, sama seperti daftar periksa penutupan dan pratinjau tutup tahun.
Tidak ada tombol "hitung ulang sekarang" selain memuat ulang; angkanya terikat periode yang dipilih,
bukan waktu membuka layar (`ACC-DEC-071`, `112`). Rekonsiliasi periode yang sudah ditutup permanen
memberi jawaban yang sama selamanya.

**Tidak ada tombol untuk mengecualikan atau mengesahkan selisih** (`ACC-DEC-113`). Selisih
dibereskan di sumbernya — Finance menyatakan ulang saldo, atau Accounting membalik jurnal yang
keliru — lalu layar dimuat ulang.

**Rekonsiliasi juga menjadi bagian penutupan periode** (`ACC-DEC-071` butir 6): hasilnya adalah
penghalang keempat pada Daftar Periksa Penutupan (bagian 11.3), dan tautan Lihat pada butir itu
membuka layar ini untuk periode yang sama.

**`DEV_DISCRETION`** (`ACC-DEC-114`): periode bawaan saat layar dibuka, bentuk pemilih periode,
serta letak, warna, dan ikon penanda keadaan — memakai komponen dan token yang sudah ada, tanpa
menyentuh `globals.css`.

**Butir menu.** Peta butir menu bagian 9 sudah memuat "Rekonsiliasi Control Account"
(`AccountingReconciliation : Read`). Temuan 28 September 2026: butirnya **tidak ada** di
`menu-items.jsx` pada `RizkiV2`, walaupun rutenya tetap terbangun — layar hanya terjangkau lewat
alamat langsung. Butir itu ditambahkan `f6b1498fe` dan hilang lewat resolusi merge `8f01cf06c`
("merge dengan branch rizki", 18 September 2026); masih ada pada `f37e949ea` (15 September 2026). Pemulihannya ikut direncanakan bersama `FE-ACC-P2-016`.

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
| Menandai akun sebagai control account | | | | ✓ | | | ✓ |
| Melihat rekonsiliasi control account | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

Tombol yang tidak boleh ditekan peran tertentu **dimatikan, bukan disembunyikan**, mengikuti pola
yang sudah dipakai sebelas layar MVP.

## 13. Redux slice Phase 2

Melanjutkan bagian 3. **Tujuh** slice bertambah, mengikuti pola penamaan yang sudah ada.

| Slice | Isi | Kapan dikosongkan |
|---|---|---|
| `accounting-event-slice.jsx` | Daftar kejadian, rincian, ringkasan jumlah | Saat badan hukum berganti |
| `accounting-posting-rule-slice.jsx` | Daftar dan rincian aturan posting | Saat badan hukum berganti |
| `accounting-event-type-slice.jsx` | Master jenis kejadian | Saat keluar dari modul |
| `accounting-recurring-journal-slice.jsx` | Template beserta riwayat penerbitannya | Saat badan hukum berganti |
| `accounting-period-closing-slice.jsx` | Daftar periksa penutupan | Setiap kali layar dibuka — **tidak boleh dari cache** |
| `accounting-year-end-slice.jsx` | Pratinjau tutup tahun | Setiap kali Pratinjau ditekan |
| `accounting-reconciliation-slice.jsx` | Saldo control account buku besar lawan subledger | Setiap kali layar dibuka — **tidak boleh dari cache** |

**Tiga slice terakhir sengaja tidak memakai cache.** Daftar periksa penutupan, pratinjau tutup
tahun, dan rekonsiliasi control account adalah angka yang dihitung saat diminta; menampilkan angka
lama membuat petugas mengambil keputusan penutupan berdasarkan keadaan yang sudah berubah.

Penanda control account `ACC-DEC-064` **tidak menambah slice** — ia memakai
`accounting-chart-of-account-slice.jsx` yang sudah ada dari MVP, bertambah satu bidang pada DTO
akunnya.

## 14. Yang tidak dikerjakan frontend pada Phase 2

| Yang ditolak | Alasan |
|---|---|
| Layar penerbitan kejadian | Accounting adalah **penerima**, bukan penerbit. Yang menerbitkan adalah Finance |
| Layar pencarian pasien dari kejadian | Dilarang `ACC-DEC-056`. Penelusuran dilakukan dengan membuka modul asalnya |
| Pemberitahuan langsung lewat SignalR | Ditolak `ACC-DEC-057`. Memakai penanda jumlah pada menu |
| Layar Laba Rugi dan Neraca | Tetap ditunda `ACC-DEC-030`; bukan bagian dari keempat slice Phase 2 |
| Komponen tabel baris jurnal tandingan untuk template | Form Jurnal Berulang memakai ulang komponen Form Jurnal |

## 15. Amendment 24 November 2026 — buku besar, laba rugi, dan neraca (ACC-FIN-005 + 008) — **approved** Rizki (`GATE-DESAIN-1124`)

Dasar: `ACC-DEC-141`..`155`; kontrak `ACC-API-0.17`; backend `02-backend-architecture.md` bagian 25.
Source acuan: `RizkiV2` `22894248`.

### 15.1 Hierarki wewenang yang dipakai

| Lapisan | Isi untuk slice ini |
|---|---|
| Keamanan, privasi, invariant | Hanya jurnal `Posted`; hak `GeneralLedger : Read` ditegakkan backend; kolom Asal tanpa pengenal pasien (`ACC-DEC-056`); pengenal di alamat memakai token rute privat yang sudah ada, bukan Guid mentah |
| Brief produk yang disetujui | `ACC-DEC-152` (dua menu, rute, isi ringkasan) dan PRD bagian 7.1 serta 7.4 (skema laporan, tombol aksi tidak di header halaman) |
| Konvensi proyek | Pola halaman Neraca Saldo: `Hero` + `AccountingHeroLegalEntity`, `DataFilter`, `FilterSelect`, `DataTable`, `StatusBadge`, `BaseButton`, `AccessDeniedGate`; module CSS per layar; **`globals.css` tidak disentuh** |
| `DEV_DISCRETION` | Warna, jarak, ikon, bentuk baris yang dapat dibuka-tutup, letak peringatan, kerangka pemuatan |

### 15.2 Peta butir menu

Disunting di `src/utils/menu-sidebar/menu-items.jsx`, grup Accounting yang sudah ada.

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Buku Besar | 1 | Accounting | `/corporate/accounting/general-ledger` | `FE-L1` | `GeneralLedger : Read` | Sudah ada |
| Neraca Saldo | 1 | Accounting | `/corporate/accounting/trial-balance` | `FE-L2` | `GeneralLedger : Read` | Sudah ada |
| **Laba Rugi** | 1 | Accounting | `/corporate/accounting/income-statement` | `FE-L3` | `GeneralLedger : Read` | **Baru**, sesudah Neraca Saldo |
| **Neraca** | 1 | Accounting | `/corporate/accounting/balance-sheet` | `FE-L4` | `GeneralLedger : Read` | **Baru**, sesudah Laba Rugi |

Butir Accounting yang ada tidak memakai `requiredPermission`; kedua butir baru mengikuti pola yang
sama supaya grup tetap seragam. Penjagaan layar dilakukan `AccessDeniedGate` dan backend. Kartu
beranda Accounting (`accounting-constants.jsx`) mendapat dua entri tambahan dengan pola entri
`general-ledger` yang ada.

### 15.3 Layar

| ID | Layar | Rute | Status | Jalan masuk |
|---|---|---|---|---|
| `FE-L1` | Buku Besar | `/corporate/accounting/general-ledger` | Diperbarui | Menu; tautan dari baris akun `FE-L3`/`FE-L4` |
| `FE-L2` | Neraca Saldo | `/corporate/accounting/trial-balance` | Diperbarui (tombol unduh saja) | Menu |
| `FE-L3` | Laba Rugi | `/corporate/accounting/income-statement` | Baru | Menu |
| `FE-L4` | Neraca | `/corporate/accounting/balance-sheet` | Baru | Menu |

Rincian Jurnal (`/corporate/accounting/journals/[slug]`) dan Rincian Kejadian
(`/corporate/accounting/accounting-events/[slug]`) sudah ada dan menjadi tujuan tautan; keduanya tidak
berubah.

### 15.4 Skema `FE-L1` Buku Besar

```text
+- Buku Besar ------------------------------------------------- FE-L1 -+
| [Hero: judul, deskripsi, badan hukum]                                |
+----------------------------------------------------------------------+
| [Akun v] [Unit Biaya v (opsional)] [Tgl Awal] [Tgl Akhir] [25 baris] |
|                                     [Atur ulang] [Unduh CSV]         |
+----------------------------------------------------------------------+
| Saldo Awal | Total Debit | Total Kredit | Saldo Akhir                |
| ! Saldo ini hanya bagian unit Radiologi.   (bila unit dipilih)       |
+----------------------------------------------------------------------+
| Tanggal | No. Jurnal (tautan) | Asal (tautan bila kejadian) | Unit |   |
|         | Keterangan          | Debit | Kredit | Saldo Berjalan     |
+----------------------------------------------------------------------+
| memuat -> kerangka baris                                             |
| belum pilih akun -> "Pilih akun untuk melihat mutasinya."            |
| kosong -> "Tidak ada mutasi pada rentang ini."        [Atur ulang]   |
| gagal  -> "Buku besar gagal dimuat."                  [Coba lagi]    |
+- Halaman 1 dari n ------------------------ [< Sebelumnya] [Berikutnya >]
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan | Akun, unit biaya, rentang tanggal, ukuran halaman | Pilihan akun dan unit biaya dari endpoint pilihan yang sudah dipakai form jurnal | `GeneralLedger : Read` | Unit biaya kosong = seluruh baris |
| Ringkasan | Saldo awal, total debit, total kredit, saldo akhir | `GET /general-ledger/movements/summary` | `GeneralLedger : Read` | Gagal → strip ringkasan diganti "Ringkasan gagal dimuat." tanpa menahan tabel |
| Catatan unit | Kalimat bagian unit (`ACC-DEC-147` butir 1) | Dari saringan | — | Tidak tampil bila unit kosong |
| Tabel | Baris mutasi, kolom Asal dan Unit baru | `GET /general-ledger/movements` | `GeneralLedger : Read` | Lihat skema |
| Tautan No. Jurnal | Membuka Rincian Jurnal lewat token rute privat (`registerPrivateRouteToken`, cakupan dan jenis token milik konfigurasi jurnal yang ada) | `JournalId` di baris | `Journal : Read` di layar tujuan | — |
| Tautan Asal | Hanya bila `SourceKind = 2`: membuka Rincian Kejadian lewat token rute privat kotak masuk | `AccountingEventId` di baris | `AccountingEvent : Read` di layar tujuan | Asal lain tampil sebagai teks |
| Unduh CSV | Seluruh baris sesuai saringan | `GET /general-ledger/movements/export` | `GeneralLedger : Read` | `422` → pesan backend apa adanya; galat lain → "Berkas gagal diunduh." |

**Masuk dari laporan (`ACC-DEC-148` butir 4).** Layar membaca saringan awal dari alamat:
`?account={token}&from={yyyy-MM-dd}&to={yyyy-MM-dd}&costCenter={token}`. Akun dan unit biaya memakai
token rute privat, bukan Guid. Bila token tidak dapat diurai — misalnya alamat dibuka di tab lain yang
tidak menyimpan tokennya — layar tampil dengan saringan kosong dan pesan "Saringan dari tautan tidak
dapat dibaca. Pilih akun kembali." **Contoh:** klik baris "Kas Kasir" kolom 30 Nov 2026 pada Neraca →
Buku Besar terbuka untuk akun Kas Kasir, 1 Januari–30 November 2026.

### 15.5 Skema `FE-L2` Neraca Saldo

Skema yang ada tidak berubah. Satu tombol **Unduh CSV** ditambahkan di baris saringan, aktif bila
periode sudah dipilih, memanggil `GET /general-ledger/trial-balance/export` dengan saringan yang
sama. Gagal → "Berkas gagal diunduh."

### 15.6 Skema `FE-L3` Laba Rugi

```text
+- Laba Rugi -------------------------------------------------- FE-L3 -+
| [Hero: judul, deskripsi, badan hukum]                                |
+----------------------------------------------------------------------+
| [Periode v] [Unit Biaya v (opsional)] [ ] Tampilkan akun bersaldo nol|
|                                          [Unduh CSV] [Cetak]         |
+----------------------------------------------------------------------+
| Total Pendapatan | Total Beban | Laba (Rugi) Bersih   (kolom YTD)    |
| ! Hanya baris jurnal yang mencatat unit biaya ini ... (bila unit)    |
| i Periode November 2025 belum ada di sistem. (bila tanpa pembanding) |
+----------------------------------------------------------------------+
|                  | Nov 2026 | Nov 2025 | Selisih | Jan-Nov 26 | ...  |
| PENDAPATAN       |          |          |         |            |      |
|   Pendapatan Layanan (subtotal)                                      |
|     Pendapatan Rawat Jalan (tautan)                                  |
|     Potongan Pendapatan          (5.000.000)                         |
| Total Pendapatan                                                     |
| BEBAN ...                                                            |
| Total Beban                                                          |
| LABA (RUGI) BERSIH                                                   |
+----------------------------------------------------------------------+
| belum pilih periode -> "Pilih periode untuk menyusun laba rugi."     |
| kosong -> "Belum ada pendapatan maupun beban yang disahkan pada      |
|            periode ini."                                             |
| gagal  -> "Laba rugi gagal dimuat."                   [Coba lagi]    |
+----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan | Periode, unit biaya, akun nol | Pilihan periode dari endpoint periode yang sudah dipakai Neraca Saldo | `GeneralLedger : Read` | — |
| Ringkasan | Total pendapatan, total beban, laba bersih — kolom YTD | `GET /general-ledger/income-statement` (`Sections[].Total`, `NetIncome`) | `GeneralLedger : Read` | Ikut keadaan tabel |
| Catatan unit | `CostCenterNotice` | Respons yang sama | — | Tidak tampil bila kosong |
| Catatan pembanding | `ComparisonUnavailableReason`; kolom pembanding dan selisih tampil `—` | Respons yang sama | — | — |
| Tabel bertingkat | Baris `Sections[].Rows` berurutan; indentasi dari `Depth`; `RowKind = 2` tebal sebagai subtotal; nilai negatif dalam kurung | Respons yang sama | `GeneralLedger : Read` | Lihat skema |
| Tautan baris akun | `RowKind = 1`: buka `FE-L1` akun itu dengan rentang kolom yang diklik — kolom periode atau YTD — dan unit biaya terpilih | `AccountId` + rentang dari respons | `GeneralLedger : Read` | — |
| Unduh CSV | Laporan apa adanya, termasuk pilihan akun nol | `GET /general-ledger/income-statement/export` | `GeneralLedger : Read` | "Berkas gagal diunduh." |
| Cetak | `window.print()`; tampilan cetak menyembunyikan saringan dan tombol lewat `@media print` di module CSS layar, mengikuti pola laporan AR Finance dan rincian tagihan Billing | — | `GeneralLedger : Read` | — |

Kolom YTD dan kolom periode dapat sama-sama tampil; pada layar sempit, penataan keenam kolom angka
adalah `DEV_DISCRETION` (misalnya kolom pertama tetap, sisanya bergulir).

### 15.7 Skema `FE-L4` Neraca

```text
+- Neraca ----------------------------------------------------- FE-L4 -+
| [Hero: judul, deskripsi, badan hukum]                                |
+----------------------------------------------------------------------+
| [Periode v] [ ] Tampilkan akun bersaldo nol      [Unduh CSV] [Cetak] |
+----------------------------------------------------------------------+
| Total Aset | Total Liabilitas | Total Ekuitas | [Seimbang] / [Tidak] |
| ! Tahun buku 2026 belum ditutup.        (bila UnclosedFiscalYears)   |
| ! Neraca tidak seimbang, selisih Rp 100. Periksa Neraca Saldo.       |
+----------------------------------------------------------------------+
|                         | 30 Nov 2026 | 31 Des 2025 | Selisih        |
| ASET ...                                                             |
| Total Aset                                                           |
| LIABILITAS ...                                                       |
| EKUITAS                                                              |
|   Modal, Laba Ditahan (tautan)                                       |
|   Laba (Rugi) Tahun Lalu Belum Ditutup   (baris hitungan, tanpa      |
|   Laba (Rugi) Tahun Berjalan              tautan)                    |
| Total Ekuitas                                                        |
| TOTAL LIABILITAS DAN EKUITAS                                         |
+----------------------------------------------------------------------+
| belum pilih periode / kosong / gagal: pola sama dengan FE-L3         |
+----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Ringkasan | Total aset, liabilitas, ekuitas, status seimbang (`StatusBadge`) | `GET /general-ledger/balance-sheet` | `GeneralLedger : Read` | Ikut keadaan tabel |
| Peringatan tutup tahun | Menyebut setiap tahun di `UnclosedFiscalYears` | Respons yang sama | — | Tidak tampil bila daftar kosong |
| Peringatan tidak seimbang | `Difference` dalam rupiah (`ACC-DEC-154`) | Respons yang sama | — | Tidak tampil bila `IsBalanced` |
| Tabel bertingkat | Seperti `FE-L3`, satu pasang kolom; baris `RowKind = 3` tampil miring dan tidak bertautan | Respons yang sama | `GeneralLedger : Read` | — |
| Tautan baris akun | Buka `FE-L1` akun itu, dari awal data sampai tanggal kolom yang diklik | `AccountId` + `AsOfDate`/`ComparisonAsOfDate` | `GeneralLedger : Read` | — |
| Unduh CSV, Cetak | Seperti `FE-L3` | `GET /general-ledger/balance-sheet/export` | `GeneralLedger : Read` | "Berkas gagal diunduh." |

### 15.8 Aksi per peran

Satu-satunya hak yang dipakai adalah `GeneralLedger : Read`, dipegang seluruh peran Accounting. Tidak
ada tombol yang dijaga hak lain kecuali tautan ke layar tujuan, yang dijaga layar tujuan sendiri.

### 15.9 State dan data

| Hal | Rancangan |
|---|---|
| Redux | `accounting-general-ledger-slice.jsx` yang ada diperluas dengan tiga thunk dan tiga potongan state: ringkasan mutasi, laba rugi, neraca. Tanpa cache lintas kunjungan, seperti neraca saldo |
| Unduhan | Tidak lewat Redux. Hook layar memanggil `InstanceAxios` dengan `responseType: "blob"` lalu memicu unduhan dengan nama dari `Content-Disposition`, mengikuti pola hook Finance (`use-supplier-payable-movements.jsx`). Tombol mati selama unduhan berjalan supaya tidak terkirim dua kali |
| Hook | `use-general-ledger.jsx` dan `use-trial-balance.jsx` diperluas; `use-income-statement.jsx` dan `use-balance-sheet.jsx` baru di folder `general-ledger` yang sama |
| Konstanta | Bunyi keadaan kosong dan catatan di `lib/constants/corporate/accounting/general-ledger/` |
| Penyusunan pohon | **Tidak** dilakukan frontend; urutan dan kedalaman dibaca apa adanya dari respons (`02-backend-architecture.md` bagian 25.6) |
| Basi | Mengganti periode, unit biaya, atau badan hukum membatalkan hasil sebelumnya; respons yang datang terlambat untuk saringan lama dibuang |
| Aksesibilitas | Nilai negatif dibaca sebagai "minus", bukan hanya tanda kurung; baris subtotal punya penanda selain tebal |

### 15.10 Berkas yang tersentuh

| Berkas | Status |
|---|---|
| `src/app/corporate/accounting/income-statement/page.jsx`, `income-statement-client.jsx` | Baru |
| `src/app/corporate/accounting/balance-sheet/page.jsx`, `balance-sheet-client.jsx` | Baru |
| `src/components/view/corporate/accounting/general-ledger/income-statement-view.jsx`, `balance-sheet-view.jsx` | Baru |
| `src/components/view/corporate/accounting/general-ledger/general-ledger-view.jsx`, `trial-balance-view.jsx` | Diperbarui |
| `src/lib/hooks/corporate/accounting/general-ledger/use-income-statement.jsx`, `use-balance-sheet.jsx` | Baru |
| `src/lib/hooks/corporate/accounting/general-ledger/use-general-ledger.jsx`, `use-trial-balance.jsx` | Diperbarui |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | Diperbarui |
| `src/style/corporate/accounting/income-statement-view.module.css`, `balance-sheet-view.module.css` | Baru |
| `src/utils/menu-sidebar/menu-items.jsx`, `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Diperbarui |

### 15.11 Yang tidak dikerjakan frontend pada slice ini

| Hal | Alasan |
|---|---|
| Menyusun CSV atau PDF di peramban | `ACC-DEC-149` — CSV dari backend, PDF lewat cetak peramban |
| Menghitung ulang total atau saldo | Angka hanya dari backend, supaya layar, CSV, dan neraca saldo sepakat |
| Tombol Cetak di Buku Besar dan Neraca Saldo | Buku besar berhalaman sehingga cetak hanya memuat satu halaman; neraca saldo hanya diberi CSV (`ACC-DEC-153`) |
| Tautan ke layar modul sumber | `ACC-DEC-148` butir 3 |
| Menyentuh `globals.css` atau tema | Konvensi frontend Quilvian |


## 16. Amendment 25 November 2026 — Master COA: kode otomatis, jenis warisan, klasifikasi (ACC-FIN-001) — **approved** Rizki (`GATE-DESAIN-1125`)

Dasar: `ACC-DEC-162`..`170`, `172`, `173`; kontrak `ACC-API-0.18`; backend `02-backend-architecture.md`
bagian 26. Source acuan: `RizkiV2` `a2ddb52d2`.

### 16.1 Hierarki wewenang yang dipakai

| Lapisan | Isi untuk amendment ini |
|---|---|
| Keamanan, privasi, invariant | Kode final ditetapkan backend, layar hanya menampilkan pratinjau (`ACC-DEC-164`); jenis akun anak dikunci layar **dan** backend (`ACC-DEC-166`); klasifikasi tidak ditebak dari nama atau kode (`ACC-DEC-168`); pengenal di alamat memakai token rute privat yang ada |
| Brief produk yang disetujui | `ACC-DEC-162` (induk dipilih dari daftar, tingkat mengikuti induk), `163`/`164` (kode otomatis), `166` (jenis warisan), `167`/`168` (klasifikasi, peringatan neraca, pengisian lewat layar Bagan Akun) |
| Konvensi proyek | Pola form master data yang ada (`BaseEditorView` lewat `chart-of-account-form-view.jsx`), `DataFilter`/`FilterSelect`, `DataTable`, `StatusBadge`, `InformationAlert`, `BaseDetailView`; aksi rincian master data hanya **Kembali, Perbarui, Hapus** (`AGENTS.md` bagian Aturan Master Data); `globals.css` tidak disentuh |
| `DEV_DISCRETION` | Letak dan urutan isian klasifikasi di dalam form, bunyi keterangan isian, bentuk penanda "ikut induk", warna lencana belum lengkap — sepanjang memakai komponen di atas |

### 16.2 Peta butir menu

**Tidak ada butir menu baru.** Seluruh layar amendment ini dijangkau dari butir yang sudah ada:

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| COA | 2 | Accounting → Master Data | `/corporate/accounting/chart-of-accounts` | `FE-C1` | `ChartOfAccount : Read` (dijaga layar) | Sudah ada |
| Neraca | 1 | Accounting | `/corporate/accounting/balance-sheet` | `FE-L4` | `GeneralLedger : Read` | Sudah ada |

Layar anak beserta jalan masuknya:

| Layar anak | Rute | Induk (jalan masuk) |
|---|---|---|
| `FE-C2` Tambah Akun | `/corporate/accounting/chart-of-accounts/create` | Tombol Tambah di `FE-C1` (lama) |
| `FE-C3` Perbarui Akun | `/corporate/accounting/chart-of-accounts/[slug]/update` | Tombol Perbarui di `FE-C4` (lama) |
| `FE-C4` Rincian Akun | `/corporate/accounting/chart-of-accounts/[slug]` | Klik dua kali baris `FE-C1` (lama) |
| `FE-C1` tersaring "Belum lengkap" | `/corporate/accounting/chart-of-accounts?klasifikasi=belum-lengkap` | Tautan **Lengkapi di Bagan Akun** pada peringatan `FE-L4` (baru) |

### 16.3 Layar

| ID | Layar | Status | Yang berubah |
|---|---|---|---|
| `FE-C1` | Daftar COA | Diperbarui | + kolom Klasifikasi; + saringan Klasifikasi (Semua / Belum lengkap) yang juga dibaca dari alamat; tampilan pohon memberi penanda akun belum lengkap |
| `FE-C2` | Tambah Akun | Diperbarui | Kode menjadi pratinjau tanpa isian; jenis terkunci bila induk dipilih; saldo normal diisi usulan induk; + isian klasifikasi |
| `FE-C3` | Perbarui Akun | Diperbarui | + isian klasifikasi; kode tetap dapat diubah selama belum dipakai jurnal `Posted` (lama) |
| `FE-C4` | Rincian Akun | Diperbarui | + baris Klasifikasi dan Sub-klasifikasi beserta asal warisannya. Aksi **tetap** Kembali dan Perbarui |
| `FE-L4` | Neraca | Diperbarui | + peringatan klasifikasi belum lengkap dengan tautan ke `FE-C1` tersaring. Angka dan tabel tidak berubah |

### 16.4 Skema `FE-C2` Tambah Akun

```text
+- Tambah Akun ------------------------------------------------ FE-C2 -+
| [Hero: Akuntansi / Tambah Akun]                                      |
+----------------------------------------------------------------------+
| Badan Hukum        [PT Metropolitan Medical Centre v]                |
| Akun Induk         [1-1000 — Kas dan Setara Kas    v]                |
| Kode Akun          1-1005   (kode otomatis — final saat disimpan)    |
| Tingkat Akun       3        (terkunci: tingkat induk + 1)            |
| Jenis Akun         Aset     (terkunci: mengikuti induk 1-1000)       |
| Saldo Normal       [Debit v]  (usulan dari induk, boleh diganti)     |
| Nama Akun          [______________________]                          |
| Menerima Transaksi [x]   Control Account [ ]                         |
| Klasifikasi        [Ikut induk: Lancar            v]                 |
| Sub-klasifikasi    [Ikut induk: Kas dan Setara Kas v]                |
| Berlaku Mulai [____]   Keterangan [__________]                       |
+----------------------------------------------------------------------+
| pratinjau memuat -> "Menyusun kode..."                               |
| pratinjau gagal  -> pesan backend apa adanya; tombol Simpan mati     |
|                      contoh "Akun 1-1004 sudah memiliki transaksi…"  |
+--------------------------------------------------- [Batal] [Simpan] -+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Akun Induk | Akun induk aktif, belum menerima transaksi, tingkat < 5 (penyaring lama di `use-chart-of-account-editor`) | `GET /chart-of-accounts/tree` (lama) | `ChartOfAccount : Read` | Kosong → "Tanpa induk (akun tingkat pertama)" |
| Kode Akun | Teks, bukan isian. Tanpa induk dan tanpa jenis → "Pilih jenis atau induk untuk melihat kode." | `GET /chart-of-accounts/next-code` | `ChartOfAccount : Create` | Gagal → pesan backend (`VAL-COA-06`, `13`, `14`, induk bertransaksi, tingkat 5); Simpan mati sampai induk atau jenis diganti |
| Tingkat Akun | Terkunci (lama) | Dari pratinjau | — | — |
| Jenis Akun | Pilihan aktif hanya bila tanpa induk; bila berinduk, terkunci dan terisi jenis induk | Dari pratinjau | — | — |
| Saldo Normal | Diisi `SuggestedNormalBalance` saat induk dipilih, **kecuali** petugas sudah mengubahnya sendiri | Dari pratinjau | — | Tanpa induk → kosong, wajib dipilih (lama) |
| Klasifikasi | Pilihan: "Ikut induk: {nilai}" (atau "Ikut induk: belum diisi"), Lancar, Tidak Lancar. Tampil hanya untuk jenis Aset dan Liabilitas | Pilihan tetap di konstanta; label warisan dari pratinjau | — | Jenis lain → isian tidak tampil |
| Sub-klasifikasi | Pilihan: "Ikut induk: {nilai}", lima sub-klasifikasi. Tampil hanya untuk Aset; aktif hanya bila klasifikasi efektif (isian atau warisan) Lancar | Konstanta + pratinjau | — | Tidak Lancar → isian mati dengan keterangan "Hanya untuk aset lancar." |
| Simpan | `POST` tanpa `accountCode`; kelompok `classification` dikirim hanya bila salah satu isian bukan "Ikut induk" | `POST /chart-of-accounts` | `ChartOfAccount : Create` | Toast memakai **kode dari respons**, bukan pratinjau; bila berbeda → "Akun tersimpan dengan kode 1-1006 karena 1-1005 baru saja dipakai." `409`/`400` → pesan backend apa adanya |

**Pratinjau dan data basi.** Pratinjau dipanggil saat badan hukum, induk, atau jenis (tanpa induk)
berganti. Respons yang datang untuk pilihan lama dibuang (pola `requestId` yang dipakai
`accounting-general-ledger-slice` dan `accounting-period-slice`). Tombol Simpan mati selama pratinjau
memuat dan selama simpan berjalan, supaya tidak terkirim dua kali.

### 16.5 Skema `FE-C3` Perbarui Akun

Skema lama tidak berubah, ditambah dua isian klasifikasi di bawah Control Account:

```text
| Kode Akun          [1-1004    ]  (lama: terkunci bila sudah Posted)  |
| ...                                                                  |
| Klasifikasi        [Ikut induk: Lancar            v]                 |
| Sub-klasifikasi    [Ikut induk: Kas dan Setara Kas v]                |
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Klasifikasi, Sub-klasifikasi | Terisi nilai **milik akun** dari rincian (`ReportClassification`, `CurrentAssetCategory`); `null` tampil "Ikut induk: {nilai efektif induk}" | `GET /chart-of-accounts/{id}` (diperluas) | `ChartOfAccount : Update` (lama, layar) | Aturan tampil dan aktif sama dengan `FE-C2` |
| Simpan | `PUT` lama; kelompok `classification` **selalu** dikirim utuh dari kedua isian (ganti penuh), supaya memilih "Ikut induk" benar-benar mengosongkan | `PUT /chart-of-accounts/{id}` | `ChartOfAccount : Update` | `409` `VAL-COA-11` → pesan backend apa adanya, menyebut kode turunan |
| Pindah induk | Label "Ikut induk" mengikuti induk **tersimpan** sampai disimpan; sesudah simpan, rincian dimuat ulang | — | — | `409` `VAL-COA-05` → pesan backend |

### 16.6 Skema `FE-C4` Rincian Akun

Dua baris rincian ditambahkan pada `detailFields` yang ada, sesudah Control Account:

| Baris | Bunyi | Sumber |
|---|---|---|
| Klasifikasi Laporan | "Lancar" · "Lancar (ikut 1-1000)" · "Belum diisi" (lencana peringatan) · "Tidak berlaku" untuk Ekuitas, Pendapatan, Beban | `EffectiveReportClassification`, `ReportClassificationSourceAccountCode`, `ClassificationGap` |
| Sub-klasifikasi Aset Lancar | "Kas dan Setara Kas (ikut 1-1000)" · "Belum diisi" · "Tidak berlaku" | `EffectiveCurrentAssetCategory`, `CurrentAssetCategorySourceAccountCode` |

Aksi tetap **Kembali** dan **Perbarui** (`AGENTS.md`); mengubah klasifikasi lewat Perbarui.

### 16.7 Skema `FE-C1` Daftar COA

```text
| [Jenis v] [Status v] [Transaksi v] [Klasifikasi: Belum lengkap v] [25 baris v] |
| Kode | Nama Akun | Jenis | Saldo Normal | Tingkat | Transaksi | Control | Klasifikasi | Status |
| 1-2001 | Piutang Pasien Umum | Aset | Debit | 3 | Ya | — | [Belum lengkap] | Aktif |
| kosong (saringan Belum lengkap) -> "Seluruh akun sudah terklasifikasi."          |
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan Klasifikasi | `FilterSelect` dua pilihan: Semua, Belum lengkap → `UnclassifiedOnly=true`. Nilai awal dibaca dari `?klasifikasi=belum-lengkap` | `GET /chart-of-accounts` (diperluas) | `ChartOfAccount : Read` | Badan hukum belum terpilih → saringan mati (`VAL-COA-12`) |
| Kolom Klasifikasi | Nilai efektif singkat: "Lancar · Kas", "Tidak Lancar", lencana "Belum lengkap" bila `ClassificationGap` terisi, "—" untuk jenis lain | Respons daftar | — | — |
| Tampilan pohon | Penanda kecil "belum lengkap" pada simpul dengan `ClassificationGap` | `GET /tree` (diperluas) | — | — |

### 16.8 Skema `FE-L4` Neraca — tambahan

Satu `InformationAlert` di bawah peringatan yang ada (tahun belum ditutup, tidak seimbang):

```text
| ! Klasifikasi laporan belum lengkap: 3 akun bersaldo (1002 Kas Besar,  |
|   1-1003 Kas Kecil, 1-1004 Bank Operasional). Rasio keuangan yang      |
|   bergantung padanya belum dapat dihitung.   [Lengkapi di Bagan Akun]  |
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Peringatan klasifikasi | Jumlah dan paling banyak lima kode + nama; sisanya "dan {n} akun lain" | `UnclassifiedAccountCount`, `ClassificationGaps` di `GET /balance-sheet` | `GeneralLedger : Read` | Tidak tampil bila 0 |
| Tautan | Membuka `FE-C1` dengan `?klasifikasi=belum-lengkap` | — | `ChartOfAccount : Read` di layar tujuan | — |

Tampilan cetak neraca **tidak** memuat peringatan ini, mengikuti perlakuan peringatan lain di cetak
yang sudah ada; CSV tidak berubah.

### 16.9 Aksi per peran

| Aksi | Hak | Peran (matriks `ACC-PERMISSION`) |
|---|---|---|
| Melihat klasifikasi, menyaring belum lengkap | `ChartOfAccount : Read` | Seluruh peran Accounting |
| Tambah akun (pratinjau kode, klasifikasi awal) | `ChartOfAccount : Create` | Accounting Administrator |
| Perbarui akun termasuk klasifikasi | `ChartOfAccount : Update` | Accounting Administrator |
| Membaca peringatan di neraca | `GeneralLedger : Read` | Seluruh peran Accounting |

### 16.10 State dan data

| Hal | Rancangan |
|---|---|
| Redux | `accounting-chart-of-account-slice.jsx` + satu thunk `getChartOfAccountNextCode` dengan pola `requestId`; thunk `createChartOfAccount`/`updateChartOfAccount` lama tidak berubah bentuk |
| Pembentuk payload | `buildPayload` di `chart-of-account-utils.jsx`: mode create tidak lagi mengirim `accountCode`, mengirim `accountType` hanya tanpa induk, menambah `classification`; mode update menambah `classification` utuh |
| Validasi layar | `validateForm`: hapus wajib Kode Akun pada create; Jenis Akun wajib hanya tanpa induk. Aturan klasifikasi lain tidak diulang di layar selain menyembunyikan dan mematikan isian (bagian 16.4) — backend tetap penegak |
| Konstanta | Pilihan klasifikasi dan sub-klasifikasi beserta label di `chart-of-account-constants.jsx`; kolom dan saringan baru di konfigurasi daftar yang sama |
| Basi | Pratinjau: respons lama dibuang. Daftar: saringan dari alamat dibaca sekali saat layar dibuka |
| Aksesibilitas | Kode otomatis dibaca sebagai teks bersama keterangannya; lencana "Belum lengkap" punya teks, bukan warna saja |

### 16.11 Berkas yang tersentuh

| Berkas | Status |
|---|---|
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-editor.jsx` | Diperbarui (pratinjau, kunci jenis, usulan saldo normal, toast kode final) |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account.jsx` | Diperbarui (saringan Klasifikasi + baca alamat) |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-detail.jsx` | Diperbarui (baris klasifikasi) |
| `src/lib/constants/corporate/accounting/chart-of-account/chart-of-account-constants.jsx` | Diperbarui |
| `src/utils/corporate/accounting/chart-of-account/chart-of-account-utils.jsx` | Diperbarui |
| `src/lib/state/slice/corporate/accounting/accounting-chart-of-account-slice.jsx` | Diperbarui |
| `src/components/view/corporate/accounting/chart-of-account/chart-of-account-view.jsx` | Diperbarui (kolom, penanda pohon) |
| `src/components/view/corporate/accounting/general-ledger/balance-sheet-view.jsx`, `src/lib/hooks/corporate/accounting/general-ledger/use-balance-sheet.jsx` | Diperbarui (peringatan) |
| `src/app/corporate/accounting/chart-of-accounts/page.jsx` | Diperbarui bila pembacaan alamat membutuhkan `Suspense` (pola `general-ledger/page.jsx`) |

### 16.12 Yang tidak dikerjakan frontend pada amendment ini

| Hal | Alasan |
|---|---|
| Menyusun kode akun di peramban | `ACC-DEC-164`: kode final dari backend |
| Tombol Ubah Klasifikasi di Rincian Akun | `AGENTS.md` (aksi rincian master data) — `ACC-DEC-173` butir 4 |
| Menebak klasifikasi dari nama atau kode | `ACC-DEC-168` |
| Neraca terkelompok Lancar/Tidak Lancar | `OQ-173-1` |
| Layar impor bagan akun | Belum diputuskan; impor `gl_coa` lewat skrip `POST` berkode |
| Butir menu baru | Tidak dibutuhkan (bagian 16.2) |
| Menyentuh `globals.css` atau tema | Konvensi frontend Quilvian |

---

## 17. Amendment 26 November 2026 — Rasio Keuangan (ACC-FIN-008-04) — **approved** Rizki (`GATE-DESAIN-1126`)

Dasar: `ACC-DEC-161`, `174`..`183`, turunan `184` (disahkan `GATE-DESAIN-1126`); kontrak `ACC-API-0.19`; backend
`02-backend-architecture.md` bagian 27. Source acuan: `RizkiV2` `64ba2c07e` + `FE-ACC-FIN-001-03`
(belum di-commit).

### 17.1 Hierarki wewenang yang dipakai

| Lapisan | Isi untuk amendment ini |
|---|---|
| Keamanan, privasi, invariant | Rasio dihitung backend; layar tidak menghitung ulang, tidak membulatkan ulang, dan tidak mengganti status. Tidak ada warna sehat/tidak sehat (`ACC-DEC-161` butir 9). Rasio berstatus selain Dihitung tidak pernah tampil sebagai angka |
| Brief produk yang disetujui | `ACC-DEC-180` (menu sendiri "Rasio Keuangan" tepat sesudah Neraca, rute `/corporate/accounting/financial-ratios`), `181` (rumus, pembilang, penyebut, status, alasan), `179` (kali/persen dua desimal), `182` (CSV + Cetak), `176` (dua kolom) |
| Konvensi proyek | Pola layar laporan yang ada (`balance-sheet-view.jsx`): `Hero` + `AccountingHeroLegalEntity`, `DataFilter`/`FilterSelect` periode, tombol `BaseButton` Unduh CSV/Cetak/Muat Ulang, `InformationAlert`, `StatusBadge`, `DataTable`; pola `requestId` di slice; aturan cetak `screenOnly`/`printRoot`; `globals.css` tidak disentuh |
| `DEV_DISCRETION` | Susunan sel rumus dan pembilang/penyebut di dalam tabel, nada lencana status (memakai nada `StatusBadge` yang ada, bukan warna baru), bunyi keadaan kosong, ikon menu dari pustaka ikon yang sudah dipakai menu Akuntansi |

### 17.2 Peta butir menu

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Neraca | 1 | Accounting | `/corporate/accounting/balance-sheet` | `FE-L4` | `GeneralLedger : Read` | Sudah ada |
| **Rasio Keuangan** | 1 | Accounting | `/corporate/accounting/financial-ratios` | `FE-L5` | `GeneralLedger : Read` | **Baru**, tepat sesudah Neraca (`ACC-DEC-180`) |

Layar anak: tidak ada. Tautan keluar: **Lengkapi di Bagan Akun** ke `FE-C1` tersaring
(`/corporate/accounting/chart-of-accounts?klasifikasi=belum-lengkap`), memakai konstanta yang sama dengan
`FE-ACC-FIN-001-03`.

### 17.3 Layar

| ID | Layar | Rute | Status | Jalan masuk |
|---|---|---|---|---|
| `FE-L5` | Rasio Keuangan | `/corporate/accounting/financial-ratios` | Baru | Menu |

### 17.4 Skema `FE-L5` Rasio Keuangan

```text
+- Rasio Keuangan --------------------------------------------- FE-L5 -+
| [Hero: judul, deskripsi, badan hukum]                                |
+----------------------------------------------------------------------+
| Laporan Rasio Keuangan              [Unduh CSV] [Cetak] [Muat Ulang] |
| [Periode v]                                                          |
+----------------------------------------------------------------------+
| ! Klasifikasi laporan belum lengkap: 4 akun bersaldo (1-1003 Kas     |
|   Kecil, ...). Current, Quick, dan Cash Ratio belum dapat dihitung.  |
|                                          [Lengkapi di Bagan Akun]    |
| i Tahun buku 2025 belum ada di sistem. (bila pembanding tak ada)     |
+----------------------------------------------------------------------+
| Rasio            | 30 Sep 2026               | 31 Des 2025           |
|------------------+---------------------------+-----------------------|
| Current Ratio    | [Tidak dapat dihitung]    | [Tidak dapat dihitung]|
| Aset Lancar ÷    | Klasifikasi laporan belum | Tahun buku 2025 belum |
| Liabilitas Lancar| lengkap pada 4 akun ...   | ada di sistem.        |
|------------------+---------------------------+-----------------------|
| Debt to Asset    | 0,00%                     | [Tidak dapat dihitung]|
| Total Liabilitas | Rp 0 ÷ Rp 3.230.000       | ...                   |
| ÷ Total Aset     |                           |                       |
|------------------+---------------------------+-----------------------|
| Return on Assets | [Tidak dapat dihitung]    | ...                   |
| Laba YTD ÷ rata- | Saldo awal tahun buku     |                       |
| rata Total Aset  | 2026 tidak tersedia ...   |                       |
| ... delapan baris, urutan tetap                                      |
+----------------------------------------------------------------------+
| Rasio tidak memakai ambang sehat/tidak sehat. Angka dihitung dari    |
| jurnal yang sudah disahkan, dengan klasifikasi akun terkini.         |
+----------------------------------------------------------------------+
| belum pilih periode / gagal: pola sama dengan FE-L4                  |
+----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Pemilih periode | Daftar periode badan hukum, terbaru di atas | `GET /accounting-periods` (lama) | `GeneralLedger : Read` di layar | "Badan hukum ini belum memiliki periode akuntansi." |
| Peringatan klasifikasi | Jumlah dan paling banyak lima akun penyebab per kolom kini; tombol **Lengkapi di Bagan Akun** | `ClassificationGaps` di `GET /general-ledger/financial-ratios` | `GeneralLedger : Read`; layar tujuan dijaga `ChartOfAccount : Read` | Tidak tampil bila tidak ada akun penyebab |
| Info pembanding | `ComparisonUnavailableReason` | Respons yang sama | — | Tidak tampil bila pembanding ada |
| Tabel rasio | Delapan baris: nama, rumus dalam kata-kata; per kolom hasil (`1,25x`/`12,50%`) atau lencana status, pembilang ÷ penyebut dalam rupiah, alasan, catatan | Respons yang sama | `GeneralLedger : Read` | Belum pilih periode: "Pilih periode untuk menghitung rasio keuangan."; gagal: pesan backend + Coba lagi |
| Keterangan kaki | Tanpa ambang; jurnal disahkan; klasifikasi terkini | Konstanta | — | — |
| Unduh CSV | Berkas dari backend | `GET /general-ledger/financial-ratios/export` | `GeneralLedger : Read` | "Berkas gagal diunduh." |
| Cetak | `window.print()`; filter, tombol, dan peringatan klasifikasi tidak tercetak; tabel dan info pembanding tercetak | — | — | — |

**Status di layar.** Dihitung → angka. N/A → lencana "N/A" + "Penyebut bernilai nol.". Tidak dapat
dihitung → lencana + alasan. Tidak sah → lencana + alasan + pembilang ÷ penyebut tetap tampil. Catatan
ekuitas negatif tampil di bawah angka sebagai teks keterangan, bukan warna.

### 17.5 Aksi per peran

Satu-satunya hak yang dipakai `GeneralLedger : Read`, dipegang seluruh peran Accounting. Tautan ke Bagan
Akun dijaga layar tujuan.

### 17.6 State dan data

| Hal | Rancangan |
|---|---|
| Redux | `accounting-general-ledger-slice.jsx` + thunk `getFinancialRatios` dan potongan state `financialRatios` dengan pola `requestId` (seperti `getBalanceSheet`) |
| Hook | `use-financial-ratio.jsx` baru di `lib/hooks/corporate/accounting/general-ledger/`, meniru `use-balance-sheet.jsx` (periode, muat ulang, unduhan blob lewat `InstanceAxios`, cetak) |
| Format | Hasil dibaca apa adanya dari `Value` lalu ditulis dengan satuan dari `Unit` dan koma desimal; pembilang/penyebut memakai `formatStatementAmount` yang ada |
| Konstanta | `financial-ratio-constants.jsx` baru: config layar, label status, bunyi keadaan kosong, keterangan kaki |
| Basi | Mengganti periode atau badan hukum membatalkan hasil sebelumnya; respons terlambat dibuang (`requestId`) |
| Aksesibilitas | Status ditulis sebagai teks, bukan warna saja; hasil negatif dibaca "minus" |

### 17.7 Berkas yang tersentuh

| Berkas | Status |
|---|---|
| `src/app/corporate/accounting/financial-ratios/page.jsx` + `financial-ratios-client.jsx` | Baru (pola `balance-sheet`) |
| `src/components/view/corporate/accounting/general-ledger/financial-ratio-view.jsx` | Baru |
| `src/lib/hooks/corporate/accounting/general-ledger/use-financial-ratio.jsx` | Baru |
| `src/lib/constants/corporate/accounting/general-ledger/financial-ratio-constants.jsx` | Baru |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | Diperbarui (+1 thunk, +1 potongan state) |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui (+1 butir sesudah Neraca) |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Diperbarui bila beranda Akuntansi mendaftar kartu laporan (pola Neraca) |
| `src/style/corporate/accounting/financial-ratio-view.module.css` | Baru bila aturan cetak dan sel rumus membutuhkannya; memakai token desain, aturan cetak meniru `balance-sheet-view.module.css` |

### 17.8 Yang tidak dikerjakan frontend pada amendment ini

| Hal | Alasan |
|---|---|
| Menghitung atau membulatkan rasio di peramban | Bagian 17.1 |
| Warna sehat/tidak sehat, grafik tren | `ACC-DEC-161` butir 9; tren di luar tahap pertama |
| Tautan pembilang/penyebut ke Neraca atau Laba Rugi | `ACC-DEC-181` |
| Saringan unit biaya | `ACC-DEC-184` butir 5 |
| Mengubah layar Neraca atau Laba Rugi | Tidak dibutuhkan |
| Menyentuh `globals.css` atau tema | Konvensi frontend Quilvian |

---

## 18. Amendment 26 November 2026 — Anggaran (ACC-FIN-013) — **draft**, menunggu gerbang desain Rizki

Dasar: `ACC-DEC-185`..`199`; kontrak `ACC-API-0.20`; backend `02-backend-architecture.md` bagian 28.
Source acuan: `RizkiV2` `64ba2c07e` + pekerjaan 26 November belum di-commit. Modul referensi visual:
**Jurnal** (`journals`: daftar → buat → rincian → ubah, aksi ajukan/setujui/tolak di rincian).

### 18.1 Hierarki wewenang yang dipakai

| Lapisan | Isi untuk amendment ini |
|---|---|
| Keamanan, privasi, invariant | Status dan aturan baris ditegakkan backend; layar hanya menyembunyikan/mematikan tombol menurut status dan hak. Penyetuju ≠ pengaju diperiksa backend. Pengenal di alamat memakai token rute privat yang ada (scope `accounting`, type `budget`) |
| Brief produk yang disetujui | `ACC-DEC-189` (sebar rata), `190` (alur), `191` (revisi, bulan terkunci), `192` (layar + CSV), `193` (hak), `195` (butir menu "Anggaran" sesudah Rasio Keuangan), `196` (hapus) |
| Konvensi proyek | Pola jurnal: `DataFilter`/`FilterSelect`, `DataTable`, `StatusBadge`, `BaseDetailView` atau pola rincian jurnal, `BaseButton`, modal konfirmasi/alasan yang dipakai Tolak jurnal, `InformationAlert`, `AccessDeniedGate`; thunk pola `requestId`; unduhan blob `accounting-download-utils`; `globals.css` tidak disentuh |
| `DEV_DISCRETION` | Susunan grid isian baris (kolom bulan, total, tombol sebar rata), urutan tombol aksi di rincian, bunyi keadaan kosong — sepanjang memakai komponen di atas. Bila grid isian butuh komponen yang belum ada di `base-features`, gerbang keputusan komponen `build-module-frontend` memutuskannya |

### 18.2 Peta butir menu

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Rasio Keuangan | 1 | Accounting | `/corporate/accounting/financial-ratios` | `FE-L5` | `GeneralLedger : Read` | Sudah ada |
| **Anggaran** | 1 | Accounting | `/corporate/accounting/budgets` | `FE-B1` | `Budget : Read` | **Baru**, tepat sesudah Rasio Keuangan (`ACC-DEC-195`) |

Layar anak:

| Layar anak | Rute | Induk (jalan masuk) |
|---|---|---|
| `FE-B2` Buat Anggaran | `/corporate/accounting/budgets/create` | Tombol **Buat Anggaran** di `FE-B1` (`Budget : Create`) |
| `FE-B3` Rincian Anggaran | `/corporate/accounting/budgets/[slug]` | Klik baris `FE-B1`; sesudah buat/revisi |
| `FE-B4` Ubah Baris Anggaran | `/corporate/accounting/budgets/[slug]/lines` | Tombol **Ubah Baris** di `FE-B3` (`Budget : Update`, status Draft/Ditolak) |

### 18.3 Skema `FE-B1` Daftar Anggaran

```text
+- Anggaran ---------------------------------------------------- FE-B1 -+
| [Hero: judul, deskripsi, badan hukum]                                 |
+-----------------------------------------------------------------------+
| Daftar Anggaran                                     [+ Buat Anggaran] |
| [Tahun buku v] [Status v]                                       [↻]   |
+-----------------------------------------------------------------------+
| Tahun | Versi | Status      | Total Pendapatan | Total Beban | Diajukan | Disetujui |
| 2027  | 2     | [Disetujui] | Rp 14,4 M        | Rp 12,1 M   | 03/07 A  | 05/07 B   |
| 2027  | 1     | [Digantikan]| ...                                                    |
+-----------------------------------------------------------------------+
| kosong: "Belum ada anggaran. Buat anggaran untuk tahun buku yang sudah dibangkitkan." |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak | Kosong / gagal |
|---|---|---|---|---|
| Saringan | Tahun buku (dari periode), status | `GET /accounting-periods` (lama), konstanta | `Budget : Read` | — |
| Tabel | Versi terbaru di atas | `GET /budgets` | `Budget : Read` | Kosong: bunyi di skema; gagal: pesan + Coba lagi |
| Buat Anggaran | Ke `FE-B2` | — | `Budget : Create` | Tersembunyi tanpa hak |

### 18.4 Skema `FE-B2` Buat Anggaran

Form satu langkah: Tahun buku (pilihan dari tahun buku yang sudah punya periode), Keterangan (opsional,
≤ 500). **Simpan** → `POST /budgets` → ke `FE-B3` versi baru. Galat `409` (versi untuk tahun itu sudah
ada) ditampilkan di bawah isian Tahun dengan saran membuka versi yang ada atau membuat Revisi.

### 18.5 Skema `FE-B3` Rincian Anggaran

```text
+- Anggaran 2027 · Versi 2 ------------------------------------- FE-B3 -+
| [Kembali]   [Disetujui]                                                 |
| Aksi menurut status dan hak:                                            |
|  Draft/Ditolak: [Ubah Baris] [Unduh CSV] [Ajukan] [Hapus]               |
|  Diajukan:      [Tarik] (pengaju) / [Setujui] [Tolak] (penyetuju)       |
|  Disetujui:     [Unduh CSV] [Revisi]                                    |
+------------------------------------------------------------------------+
| Kepala: badan hukum, tahun, versi, asal revisi, keterangan, diajukan,   |
| disetujui/ditolak (+ alasan)                                            |
| ! Ditolak: "{alasan}"                     (bila Ditolak)                |
+------------------------------------------------------------------------+
| Ringkasan per bulan: Pendapatan | Beban | Selisih  (Jan..Des, Total)    |
| Bulan terkunci ditandai (periode bukan Terbuka)                         |
+------------------------------------------------------------------------+
| Baris: Akun | Unit Biaya | Jan .. Des | Total        (baca saja)        |
+------------------------------------------------------------------------+
| Riwayat: Diajukan A 03/07 · Disetujui B 05/07 · ...                     |
+------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber | Hak | Kosong / gagal |
|---|---|---|---|---|
| Aksi | Hanya yang sah untuk status; Setujui/Tolak tidak tampil bagi pengaju (backend tetap menolak `403`) | `GET /budgets/{id}` (`AvailableActions`) | Per tombol: `Update`, `Approve`, `Delete`, `Create` (Revisi), `Read` (Unduh) | Galat aksi tampil di atas kepala |
| Tolak | Modal alasan wajib 1–500 | `POST /{id}/reject` | `Budget : Approve` | Alasan kosong → tombol mati |
| Ringkasan bulan | Total per bulan per jenis dari respons, tidak dihitung ulang | `MonthTotals` | — | — |
| Baris | Baca saja | `Lines` | — | Kosong: "Belum ada baris. Ubah Baris atau unggah CSV." |

### 18.6 Skema `FE-B4` Ubah Baris Anggaran

```text
+- Ubah Baris · Anggaran 2027 Versi 2 --------------------------- FE-B4 -+
| [Kembali] [Unduh CSV] [Unggah CSV] [Tambah Baris] [Simpan]             |
| i Bulan Jan–Jun terkunci: nilainya harus sama dengan versi berlaku.    |
+------------------------------------------------------------------------+
| Akun [v] | Unit Biaya [v] | Tahunan [ ] [Sebar rata] | Jan..Des [ ] | Total | [x] |
+------------------------------------------------------------------------+
| Galat impor CSV: daftar {baris, kolom, pesan} (maks. 50)               |
+------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber | Hak | Gagal |
|---|---|---|---|---|
| Pilihan akun | Akun Pendapatan/Beban aktif penerima transaksi | `GET /chart-of-accounts/options` (lama) disaring jenis | `ChartOfAccount : Read` di endpoint itu | — |
| Pilihan unit | Unit biaya aktif badan hukum | `GET /cost-centers` (lama, pola `ResourceFilterSelect`) | — | — |
| Sebar rata | Mengisi 12 bulan dari isian tahunan (bagian 28.8); dapat diubah per bulan | Hitungan layar | — | — |
| Bulan terkunci | Isian mati pada bulan terkunci versi revisi | `LockedMonths` | — | Backend tetap menolak `422` |
| Simpan | `PUT /{id}/lines` seluruh baris | — | `Budget : Update` | Galat `400` per baris disorot di barisnya |
| Unggah CSV | `POST /{id}/lines/import`, konfirmasi "mengganti seluruh baris" | — | `Budget : Update` | Daftar galat per nomor baris |

### 18.7 Aksi per peran

| Aksi | Hak |
|---|---|
| Melihat daftar, rincian, unduh CSV | `Budget : Read` |
| Buat, Revisi | `Budget : Create` |
| Ubah keterangan, Ubah Baris, Unggah CSV, Ajukan, Tarik | `Budget : Update` |
| Setujui, Tolak | `Budget : Approve` |
| Hapus | `Budget : Delete` |

Peran tidak di-hardcode; admin mencentang di Akses Role.

### 18.8 State dan data

| Hal | Rancangan |
|---|---|
| Redux | Slice baru `accounting-budget-slice.jsx` (daftar, rincian, aksi) dengan pola `requestId`; didaftarkan di `store.jsx` seperti slice Accounting lain |
| Hook | `use-budget.jsx` (daftar), `use-budget-detail.jsx` (rincian + aksi), `use-budget-lines-editor.jsx` (grid, sebar rata, unggah) |
| Kirim ganda | Tombol aksi mati selama permintaan berjalan |
| Basi | Sesudah aksi status, rincian dimuat ulang dari server; layar tidak mengubah status sendiri |
| Angka | Total per bulan dan per jenis dibaca dari respons; grid menjumlah baris yang sedang diedit hanya sebagai tampilan |
| Aksesibilitas | Status berteks; isian bulan berlabel nama bulan dan akun |

### 18.9 Berkas yang tersentuh (perkiraan)

| Berkas | Status |
|---|---|
| `src/app/corporate/accounting/budgets/` (`page.jsx`, `create/page.jsx`, `[slug]/page.jsx`, `[slug]/lines/page.jsx`, klien) | Baru |
| `src/components/view/corporate/accounting/budget/` (daftar, form buat, rincian, editor baris) | Baru |
| `src/lib/hooks/corporate/accounting/budget/` | Baru |
| `src/lib/constants/corporate/accounting/budget/budget-constants.jsx` | Baru |
| `src/lib/state/slice/corporate/accounting/accounting-budget-slice.jsx` + `store.jsx` | Baru / Diperbarui |
| `src/utils/menu-sidebar/menu-items.jsx`, `accounting-constants.jsx` (kartu beranda) | Diperbarui |
| `src/style/corporate/accounting/budget-*.module.css` | Baru bila dibutuhkan, token desain saja |

### 18.10 Yang tidak dikerjakan frontend pada amendment ini

| Hal | Alasan |
|---|---|
| Laporan realisasi lawan target | 008-05 |
| Persetujuan bertingkat | `ACC-DEC-190` |
| Menghitung status atau menolak isian bulan terkunci tanpa backend | Backend penegak |
| Menyentuh `globals.css` atau tema | Konvensi |
