# Roadmap Delivery Frontend — Accounting

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 4
blueprint_status: approved
roadmap_revision: 2
roadmap_status: APPROVED
approved_by: [Rizki]
approved_at: 2026-09-01
source_backend: aa837d784ff51cb2b889cf975ada3a204018f1f5
source_frontend: 31a82c8052a3c59445ae49e6f1ccce2bf717d6c0
decision_revision: 1.1
contracts: [ACC-API-0.5, ACC-STATE-0.1, ACC-VALIDATION-0.3, ACC-PERMISSION-0.3, ACC-TEST-0.1, ACC-MVP-0.1]
```

## Baca ini lebih dahulu

**FINAL OWNER APPROVAL sudah diberikan** Rizki, 1 September 2026, atas `ACC-BP-001` revisi 5.
Roadmap ini `APPROVED`. **`ACC-FE-001` dan `ACC-FE-003` sudah `closed` sejak 4 September 2026**, sehingga rantai frontend tidak lagi tertahan keputusan UI. Rutenya `src/app/corporate/accounting/`, isinya `src/components/view/corporate/accounting/`, dan URL yang dilihat pengguna `/corporate/accounting`.

Selain itu, seluruh task frontend menunggu endpoint-nya benar-benar ada. Kontrak API sudah
tertulis, tetapi berstatus `draft` dan **belum satu pun terpasang di kode**. Pekerjaan paralel
frontend hanya boleh dimulai untuk kontrak yang sudah disetujui, terkunci versinya, dan
endpoint-nya sudah berdiri.

### Jangan tertukar dua penomoran ini

| Pola | Artinya | Contoh |
|---|---|---|
| `FE-ACC-###` | **Task** pada roadmap ini | `FE-ACC-006` |
| `ACC-FE-###` | **Keputusan wewenang UI** pada `03-frontend-architecture.md` | `ACC-FE-001` letak menu |

`FE` di **depan** berarti task; di **belakang** berarti keputusan.

### Aturan yang mengikat seluruh task frontend

| Aturan | Ketentuan |
|---|---|
| Panggilan API | **Hanya** di `src/lib/state/slice/**` di dalam `createAsyncThunk`, memakai `InstanceAxios` |
| Pendaftaran slice | Setiap slice baru **wajib** didaftarkan di `src/lib/state/store.jsx` |
| Tabel dan penyaring | Wajib `DataTable` dan `DataFilter` dari `base-features/`. Dilarang membuat tabel manual |
| Gaya | CSS Modules di `src/style/**`. Dilarang `style={{ ... }}`. **`src/app/globals.css` tidak boleh disentuh** |
| Teks | Bahasa Indonesia untuk seluruh teks yang dilihat pengguna |
| Identitas pelaku | `createBy` dari `selectUserInfo`, tidak pernah dari isian form |
| Keputusan bisnis | Frontend **tidak** menghitung saldo, tidak menentukan periode, tidak membangkitkan nomor jurnal, dan tidak memutuskan siapa boleh menyetujui |
| Implementasi | Wajib lewat `quilvian-engineering-skills:build-module-frontend`, satu task per pemanggilan |

---

## Ringkasan gelombang

| Gelombang | Task | Status | Syarat mulai |
|---|---|---|---|
| `MVP-1` Kerangka dan master | `FE-ACC-001` sampai `FE-ACC-004` | **`IMPLEMENTED`** — keempatnya selesai 4 Sep 2026 | `BE-ACC-007` sampai `009` selesai |
| `MVP-1` Jurnal | `FE-ACC-005` sampai `FE-ACC-007` | **`IMPLEMENTED` — 005, 006, dan 007 selesai** | `BE-ACC-010`, `BE-ACC-011`, `BE-ACC-015` selesai |
| `MVP-2` Laporan | `FE-ACC-008`, `FE-ACC-009` | **`IN_PROGRESS`** — `FE-ACC-008` `IMPLEMENTED`; `FE-ACC-009` `READY` | `BE-ACC-012` selesai |
| `MVP-3` Koreksi dan saldo awal | `FE-ACC-010`, `FE-ACC-011` | **`READY`** — keduanya dapat dimulai, dan dapat berjalan paralel | `BE-ACC-013`, `BE-ACC-014` selesai |

Dua keputusan produk pernah menahan sebagian task. **Keduanya sudah ditutup 4 September 2026**, dan nol keputusan produk tersisa:

| Keputusan | Menahan | Pemilik |
|---|---|---|
| ~~`ACC-FE-001` letak menu Accounting~~ | — | **`closed` 4 Sep 2026** — pilihan B, `src/app/corporate/accounting/` |
| ~~`ACC-FE-003` bentuk layar rincian jurnal~~ | — | **`closed` 4 Sep 2026** — halaman tersendiri, `base-detail-view.jsx` |

---

## `MVP-1` — Kerangka dan master

### `FE-ACC-001` — Kerangka modul, rute, dan pemilih badan hukum

| Field | Isi |
|---|---|
| Outcome | Modul Accounting punya rute, tata letak, dan pemilih badan hukum yang dipakai bersama seluruh layar berikutnya |
| Trace | `ACC-DEC-037`; `ACC-FE-001`, `ACC-FE-008`; `EPIC ACC-01` sampai `ACC-08` |
| Kontrak | `ACC-API-0.5` — belum memanggil endpoint bisnis |
| Reuse | `hero.jsx`, `access-denied-gate.jsx`, `filter-select.jsx`, pola folder `src/app/hr/` dan `src/components/view/**` |
| Cakupan | Folder rute sesuai keputusan `ACC-FE-001`, berkas rute tipis, komponen tata letak di `src/components/view/**`, komponen pemilih badan hukum, penyimpanan pilihan badan hukum antar layar |
| Dependency | ~~`ACC-FE-001`~~ **`closed` 4 Sep 2026**; `BE-ACC-007` `DONE` untuk daftar badan hukum |
| Acceptance | (1) Rute dapat dibuka dan menampilkan tata letak. (2) Pemilih badan hukum tampil dan pilihannya bertahan saat berpindah antar layar akuntansi. (3) Pengguna tanpa hak melihat `access-denied-gate`, bukan halaman kosong. (4) Tidak ada `style={{ }}` dan `globals.css` tidak tersentuh |
| Verifikasi | `npm run lint`; `npm run test:unit`; pemeriksaan manual di peramban |
| Risiko/pemilik | Product owner untuk letak menu; developer untuk sisanya. **Menyembunyikan badan hukum yang bukan hak pengguna bukan pengamanan** — backend tetap menolak |
| DoD | Rute berjalan, lint bersih, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner di peramban. Rute `/corporate/accounting` berdiri, menu terdaftar di kelompok *Perusahaan*, pemilih badan hukum memakai ulang resource select `legalEntities`. Validasi: `lint:errors` PASS, `build` PASS, 434 unit test PASS. Acceptance (4) terbukti; (1) terbukti sebagian; (2) dan (3) menuntut sesi login. Laporan: [`../task/report/frontend/fe-acc-001-kerangka-modul-rute-dan-pemilih-badan-hukum.md`](../task/report/frontend/fe-acc-001-kerangka-modul-rute-dan-pemilih-badan-hukum.md) |

### `FE-ACC-002` — Daftar dan form daftar akun

| Field | Isi |
|---|---|
| Outcome | Administrator dapat menyusun daftar akun bertingkat lewat layar, lengkap dengan tampilan susunan induk-anak |
| Trace | `ACC-DEC-022`, `ACC-DEC-023`, `ACC-DEC-024`; `FR-ACC-001` sampai `005`; `EPIC ACC-01` |
| Kontrak | `ACC-API-0.5` grup Chart of Account |
| Reuse | **`master-data-resource-slice-factory.jsx`** untuk slice — jangan menulis slice CRUD dari nol. `DataTable`, `DataFilter`, `base-form-control.jsx`, `confirm-modal.jsx` |
| Cakupan | `accounting-chart-of-account-slice.jsx` dari factory, didaftarkan di `store.jsx`; layar daftar dengan penyaring badan hukum, jenis akun, status; layar form; tampilan pohon; tombol nonaktifkan dengan konfirmasi |
| Dependency | `FE-ACC-001`, `BE-ACC-007` |
| Acceptance | (1) Daftar berhalaman dengan `pageSize` bawaan 25. (2) Pesan galat `409` dari backend tampil sebagai kalimat Bahasa Indonesia, misalnya saat kode akun kembar. (3) Penonaktifan akun bersaldo menampilkan pesan backend beserta jumlah saldonya. (4) Slice terdaftar di `store.jsx` |
| Verifikasi | `npm run lint`; `npm run test:unit`; skenario `UAT-01`, `UAT-17` di peramban |
| Risiko/pemilik | Developer. Memakai factory adalah keharusan, bukan pilihan |
| DoD | Layar berfungsi, pesan galat terbaca pengguna, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner. Slice dari factory + 2 thunk tersendiri (deactivate berbadan permintaan, `/tree`), 5 rute, tampilan tabel dan pohon. Acceptance (4) terbukti; (1)–(3) terbukti di source, belum dilihat berjalan. `lint:errors` PASS, `build` PASS, 434 unit test PASS. Laporan: [`../task/report/frontend/fe-acc-002-daftar-dan-form-daftar-akun.md`](../task/report/frontend/fe-acc-002-daftar-dan-form-daftar-akun.md) |

### `FE-ACC-003` — Master jenis jurnal

| Field | Isi |
|---|---|
| Outcome | Administrator dapat mengatur jenis jurnal dan awalan nomornya lewat layar |
| Trace | `ACC-DEC-010`; `FR-ACC-006`, `FR-ACC-007`; `EPIC ACC-02` |
| Kontrak | `ACC-API-0.5` grup Journal Type |
| Reuse | `master-data-resource-slice-factory.jsx`, `DataTable`, `DataFilter` |
| Cakupan | `accounting-journal-type-slice.jsx` dari factory, didaftarkan di `store.jsx`; satu layar daftar dan form sederhana |
| Dependency | `FE-ACC-001`, `BE-ACC-008` |
| Acceptance | (1) Jenis bertanda sistem tidak dapat diubah kode maupun awalan nomornya — tombolnya dinonaktifkan, dan bila tetap dikirim, pesan backend ditampilkan. (2) Slice terdaftar di `store.jsx` |
| Verifikasi | `npm run lint`; pemeriksaan manual |
| Risiko/pemilik | Rendah. Developer |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner. Slice dari factory, 4 rute, isian kode dan awalan dinonaktifkan pada jenis sistem. **Delta `ACC-GAP-004` ditangani**: form mengikuti source yang sudah mencabut `RequiresApproval`, bukan kontrak. Acceptance (2) terbukti; (1) terbukti di source. `lint:errors` PASS, `build` PASS, 434 unit test PASS. Laporan: [`../task/report/frontend/fe-acc-003-master-jenis-jurnal.md`](../task/report/frontend/fe-acc-003-master-jenis-jurnal.md) |

### `FE-ACC-004` — Periode akuntansi

| Field | Isi |
|---|---|
| Outcome | Manajer dapat membangkitkan periode setahun, menutup bertahap, dan membuka kembali dengan alasan tertulis |
| Trace | `ACC-DEC-012`, `ACC-DEC-026`, `ACC-DEC-027`, `ACC-DEC-028`; `FR-ACC-010` sampai `015`; `EPIC ACC-03` |
| Kontrak | `ACC-API-0.5` grup Accounting Period; `ACC-STATE-0.1` bagian 2 |
| Reuse | `DataTable`, `status-badge.jsx`, `confirm-modal.jsx` |
| Cakupan | `accounting-period-slice.jsx` ditulis manual (punya aksi di luar CRUD), didaftarkan di `store.jsx`; layar daftar periode; tombol bangkitkan, tutup, buka kembali; isian alasan yang wajib |
| Dependency | `FE-ACC-001`, `BE-ACC-009` |
| Acceptance | (1) Tiga status tampil dengan penanda yang berbeda dan berlabel Bahasa Indonesia: Terbuka, Tutup Sementara, Tutup Permanen. (2) Tombol Buka Kembali menampilkan isian alasan, dan tidak dapat dikirim bila kosong. (3) Setelah membuka kembali periode tutup permanen, layar menampilkan status **Tutup Sementara** — dimuat ulang dari backend, bukan ditebak. (4) Tombol tutup dan buka kembali hanya muncul bagi pemegang haknya |
| Verifikasi | `npm run lint`; skenario `UAT-08`, `UAT-09` di peramban |
| Risiko/pemilik | Developer. Butir (3) menguji bahwa frontend tidak menyimpulkan status sendiri |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED` dengan satu acceptance TIDAK DAPAT DIPENUHI** — 4 September 2026. Slice ditulis manual sesuai roadmap, 3 aksi domain, tiga status berpenanda Bahasa Indonesia, muat ulang dari backend sesudah tiap aksi. Acceptance (1)–(3) terbukti di source. **Acceptance (4) tidak dapat dipenuhi**: repository frontend tidak punya mekanisme hak akses sisi klien dan backend tidak menyediakan `AvailableActions` pada periode — diusulkan `ACC-GAP-010`. `lint:errors` PASS, `build` PASS, 434 unit test PASS. Laporan: [`../task/report/frontend/fe-acc-004-periode-akuntansi.md`](../task/report/frontend/fe-acc-004-periode-akuntansi.md) |

---

## `MVP-1` — Jurnal

### `FE-ACC-005` — Daftar jurnal

| Field | Isi |
|---|---|
| Outcome | Petugas dapat mencari jurnal berdasarkan badan hukum, rentang tanggal, jenis, status, dan nomor |
| Trace | `FR-ACC-020`; `EPIC ACC-04` |
| Kontrak | `ACC-API-0.5` grup Journal, endpoint daftar |
| Reuse | `DataTable`, `DataFilter`, `filter-date-picker.jsx`, `filter-select.jsx`, `status-badge.jsx` |
| Cakupan | `accounting-journal-slice.jsx` ditulis manual mengikuti bentuk state hasil factory, didaftarkan di `store.jsx`; layar daftar dengan penyaring lengkap |
| Dependency | `FE-ACC-001`, `BE-ACC-010` |
| Acceptance | (1) Penyaring bekerja dan pagination memakai bentuk `{ pageNumber, pageSize, totalData, totalPage, items }`. (2) Status jurnal tampil berlabel Bahasa Indonesia. (3) Flag pemuatan terpisah per operasi, bukan satu penanda untuk seluruh halaman. (4) Daftar kosong menampilkan pesan yang menjelaskan sebabnya |
| Verifikasi | `npm run lint`; pemeriksaan manual |
| Risiko/pemilik | Developer |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner. Rute `/corporate/accounting/journals` berdiri, menu **Jurnal** terdaftar. Slice memakai `createMasterDataResourceSlice` + 5 thunk alur kerja — delta terhadap kalimat "ditulis manual" dicatat di laporan bagian 3. Dikerjakan di atas **`ACC-API-0.4`**. Validasi: lint PASS, 434 test PASS, build PASS. Acceptance (2), (3), (4) terbukti; (1) terbukti sebagian. **Terbukti di peramban 4 Sep 2026**: daftar memuat `JB/2026/09/00001`, status berlabel **Menunggu Persetujuan**, paginasi menampilkan "1 sampai 1 dari 1 data" — acceptance (2) TERBUKTI, (1) terbukti sebagian karena baru satu baris data. Laporan: [`../task/report/frontend/fe-acc-005-daftar-jurnal.md`](../task/report/frontend/fe-acc-005-daftar-jurnal.md) |

### `FE-ACC-006` — Form jurnal dengan baris dinamis

| Field | Isi |
|---|---|
| Outcome | Petugas dapat menyusun jurnal beserta barisnya, melihat selisih debit-kredit secara langsung, dan menyimpannya walaupun belum seimbang |
| Trace | `ACC-DEC-019`, `ACC-DEC-020`, `ACC-DEC-025`; `FR-ACC-020`, `024`, `025`; `EPIC ACC-04` |
| Kontrak | `ACC-API-0.5` endpoint buat dan ubah jurnal; `ACC-VALIDATION-0.3` bagian 3 |
| Reuse | `react-hook-form` dengan `FormProvider` dan `Controller`, `summary-grid.jsx`, `base-form-control.jsx`, `resource-filter-select.jsx` |
| Cakupan | Layar form: kepala jurnal ditambah tabel baris yang dapat ditambah dan dihapus. Ringkasan total debit, total kredit, dan selisih yang dihitung ulang setiap kali angka berubah. Kolom unit biaya menjadi wajib mengikuti `RequiresCostCenter` **dari respons backend**, bukan aturan yang dihafal frontend |
| Dependency | `FE-ACC-005`, `BE-ACC-010` |
| Acceptance | (1) Tombol Ajukan mati selama selisih belum nol, dan selisihnya tampil berwarna peringatan. (2) Daftar pilihan akun hanya memuat akun yang menerima transaksi — diambil dari `/options`, tidak disaring ulang di frontend. (3) Memilih akun beban memunculkan kewajiban unit biaya. (4) Baris jurnal muat di layar tanpa gulir mendatar. (5) Menutup layar di tengah pengisian tidak menghilangkan draft yang sudah tersimpan |
| Verifikasi | `npm run lint`; `npm run test:unit`; skenario `UAT-02`, `UAT-04` di peramban |
| Risiko/pemilik | **Tertinggi pada frontend.** Developer. Ini satu-satunya layar yang tidak mengikuti pola form biasa |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner. `react-hook-form` + `useFieldArray`, `SummaryGrid`, resource select `costCenters`. Uang dibandingkan sebagai bilangan bulat sen. Kewajiban unit biaya dibaca dari `RequiresCostCenter` pada respons `/options`. Validasi: lint PASS, 434 test PASS, build PASS. Acceptance (2) terbukti; (1), (3), (5) terbukti di kode; (4) terbukti sebagian. **`UAT-02`/`UAT-04` belum dapat dijalankan siapa pun — `BLK-ACC-02`.** **Terbukti di peramban 4 Sep 2026**: jurnal seimbang tersimpan dan terajukan, acceptance (1) dan (4) TERBUKTI. Empat cacat ditemukan owner saat menguji dan sudah diperbaiki — lihat laporan bagian 10 dan 11. Acceptance (3) dan (5) belum teruji. Laporan: [`../task/report/frontend/fe-acc-006-form-jurnal-baris-dinamis.md`](../task/report/frontend/fe-acc-006-form-jurnal-baris-dinamis.md) |

### `FE-ACC-007` — Rincian jurnal dan tombol aksi

| Field | Isi |
|---|---|
| Outcome | Petugas dapat melihat isi jurnal, riwayat persetujuannya, dan menjalankan tindakan yang memang menjadi haknya |
| Trace | `ACC-DEC-010`, `ACC-DEC-016`; `FR-ACC-030` sampai `034`; `EPIC ACC-05` |
| Kontrak | `ACC-API-0.5` endpoint rincian dan **lima** endpoint aksi; `ACC-STATE-0.1` bagian 1; `ACC-PERMISSION-0.3` bagian 5 |
| Reuse | `base-detail-view.jsx` atau `base-detail-side-panel.jsx` sesuai keputusan `ACC-FE-003`, `confirm-modal.jsx`, `status-badge.jsx`, `toast-stack.jsx` |
| Cakupan | Layar rincian baca-saja; riwayat persetujuan; tombol Ajukan, Setujui, Tolak, Sahkan, Balik yang ditampilkan **berdasarkan `AvailableActions` dari backend**; isian alasan pada Tolak dan Balik |
| Dependency | ~~`ACC-FE-003`~~ **`closed` 4 Sep 2026**; `FE-ACC-005` `IMPLEMENTED`, `BE-ACC-011` `DONE`, `BE-ACC-015` `DONE` (`ActionByName`) |
| Acceptance | (1) Tombol ditampilkan dari `AvailableActions`, **bukan** dihitung frontend. (2) Tombol Setujui tidak muncul pada jurnal buatan pengguna sendiri. (3) Setelah setiap aksi berhasil, rincian dimuat ulang dari backend — status baru tidak ditebak. (4) Tombol dimatikan selama `actionLoading` menyala, sehingga tidak terkirim dua kali. (5) Pesan `422` dan `403` dari backend ditampilkan apa adanya |
| Verifikasi | `npm run lint`; skenario `UAT-01`, `UAT-03`, `UAT-06`, `UAT-13` di peramban |
| Risiko/pemilik | Product owner untuk bentuk layar; developer untuk sisanya. Butir (1) penting: menghitung kewenangan di frontend berarti menyalin aturan bisnis ke tempat yang salah |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 7 September 2026, menunggu verifikasi manual owner di peramban. Rute `/corporate/accounting/journals/[slug]` memakai pola token privat COA; layar rincian baca-saja dengan kepala jurnal, baris, dan riwayat; tombol dibangun dari `AvailableActions`. **Acceptance (1), (2), dan (4) terkunci 10 unit test baru**; (3) dan (5) terbukti di source. Ditemukan dan diperbaiki: kelima thunk aksi sebelumnya tidak pernah menyalakan `actionLoading` karena berada di luar factory — reducer slice Accounting dikomposisi, factory tidak disentuh. `lint:errors` PASS, `build` PASS, **444 unit test PASS**. Laporan: [`../task/report/frontend/fe-acc-007-rincian-jurnal-dan-tombol-aksi.md`](../task/report/frontend/fe-acc-007-rincian-jurnal-dan-tombol-aksi.md) |

---

## `MVP-2` — Laporan

### `FE-ACC-008` — Buku besar

| Field | Isi |
|---|---|
| Outcome | Petugas dapat menelusuri mutasi satu akun beserta saldo berjalannya |
| Trace | `ACC-DEC-030`; `FR-ACC-052`; `EPIC ACC-07` |
| Kontrak | `ACC-API-0.5` endpoint `/movements` |
| Reuse | `DataTable`, `DataFilter`, `filter-date-picker.jsx`, `resource-filter-select.jsx` |
| Cakupan | `accounting-general-ledger-slice.jsx` didaftarkan di `store.jsx`; layar mutasi dengan penyaring badan hukum, akun, dan rentang tanggal |
| Dependency | `FE-ACC-001`, `BE-ACC-012` |
| Acceptance | (1) Saldo berjalan tampil per baris. (2) Rentang tanggal terbalik ditolak dan pesannya terbaca. (3) Angka rupiah diformat konsisten |
| Verifikasi | `npm run lint`; pemeriksaan manual |
| Risiko/pemilik | Developer |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 7 September 2026, menunggu verifikasi manual owner di peramban. Rute `/corporate/accounting/general-ledger` berdiri, menu **Buku Besar** terdaftar, slice ditulis manual (grup GL nol mutasi — factory akan menurunkan 10 thunk yang endpoint-nya tidak ada). **Acceptance (1) dan (3) terkunci 7 unit test baru**; (2) terbukti di source — pesan `400` backend diteruskan apa adanya, aturannya tidak disalin ke frontend. `lint:errors` PASS, `build` PASS, **452 unit test PASS**, 0 warning. **Belum dapat dilihat bekerja**: nol jurnal disahkan (`BLK-ACC-02`). Laporan: [`../task/report/frontend/fe-acc-008-buku-besar.md`](../task/report/frontend/fe-acc-008-buku-besar.md) |

### `FE-ACC-009` — Neraca saldo

| Field | Isi |
|---|---|
| Outcome | Manajemen dapat melihat posisi seluruh akun pada satu periode, beserta penanda seimbang |
| Trace | `ACC-DEC-030`, `ACC-DEC-037`; `FR-ACC-050`, `051`, `053`; `EPIC ACC-07` |
| Kontrak | `ACC-API-0.5` endpoint `/trial-balance` |
| Reuse | `DataTable`, `summary-grid.jsx` |
| Cakupan | Layar neraca saldo per badan hukum dan periode; ringkasan total debit, total kredit, dan penanda seimbang |
| Dependency | `FE-ACC-008`, `BE-ACC-012` |
| Acceptance | (1) Total debit dan total kredit tampil, beserta penanda seimbang. (2) Berpindah badan hukum mengubah angka dan tidak mencampurnya. (3) Layar menyebutkan bahwa laporan hanya memuat jurnal yang sudah disahkan |
| Verifikasi | `npm run lint`; skenario `UAT-14`, `UAT-15` di peramban |
| Risiko/pemilik | Developer. Butir (3) penting supaya pembaca tidak salah menafsirkan angka |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`READY`** — 7 September 2026, terbuka oleh selesainya `FE-ACC-008`. `BE-ACC-012` `DONE`, endpoint `/trial-balance` berdiri, dan `accounting-general-ledger-slice.jsx` sudah ada sehingga task ini tinggal menambah satu thunk beserta layarnya |

---

## `MVP-3` — Koreksi dan saldo awal

### `FE-ACC-010` — Pembalikan dan penyesuaian di layar

| Field | Isi |
|---|---|
| Outcome | Manajer dapat mengoreksi jurnal yang sudah disahkan lewat layar, dengan memilih cara koreksinya |
| Trace | `ACC-DEC-017`, `ACC-DEC-029`; `FR-ACC-040` sampai `043`; `EPIC ACC-06` |
| Kontrak | `ACC-API-0.5` endpoint `reverse`; `ACC-VALIDATION-0.3` bagian 5 |
| Reuse | `confirm-modal.jsx`, komponen baris jurnal dari `FE-ACC-006` |
| Cakupan | Dialog pembalikan: pilihan pembalikan penuh atau jurnal penyesuaian, isian alasan wajib, dan tabel baris selisih bila memilih penyesuaian |
| Dependency | `FE-ACC-007`, `BE-ACC-013` |
| Acceptance | (1) Kedua cara koreksi tersedia beserta penjelasan singkat kapan memakai yang mana. (2) Alasan wajib diisi. (3) Baris selisih pada penyesuaian harus seimbang sebelum dapat dikirim. (4) Setelah berhasil, layar menampilkan tautan ke jurnal pembalik yang baru, dan jurnal asal tetap berstatus disahkan |
| Verifikasi | `npm run lint`; skenario `UAT-10`, `UAT-11`, `UAT-12` di peramban |
| Risiko/pemilik | Developer. Butir (1) menentukan apakah petugas memilih cara yang benar |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`READY`** — 7 September 2026, terbuka oleh selesainya `FE-ACC-007`. Kedua dependency lunas: `FE-ACC-007` `IMPLEMENTED`, `BE-ACC-013` `DONE`, endpoint `POST /journals/{id}/reverse` terverifikasi ada di source `6d4fc26`. **Ada butir yang menunggu task ini**: `FE-ACC-007` menampilkan tombol Balik yang selalu mengirim `CorrectionType: FullReversal` karena field itu `[Required]` sementara pemilihannya adalah cakupan task ini. Dialog di sini menggantikannya dengan pilihan yang sebenarnya |

### `FE-ACC-011` — Saldo awal di layar

| Field | Isi |
|---|---|
| Outcome | Saldo pembuka dapat dimasukkan lewat layar jurnal biasa, dengan penjelasan yang cukup bagi petugas |
| Trace | `ACC-DEC-018`, `ACC-DEC-033`; `FR-ACC-060`, `FR-ACC-061`; `EPIC ACC-08` |
| Kontrak | `ACC-API-0.5` grup Journal |
| Reuse | Seluruh layar jurnal yang sudah ada. **Tidak ada layar baru** |
| Cakupan | Penyesuaian kecil pada form jurnal: keterangan pembantu saat jenis `SA` dipilih, dan penanda pada daftar jurnal bahwa jurnal `SA` adalah saldo pembuka |
| Dependency | `FE-ACC-006`, `BE-ACC-014` |
| Acceptance | (1) Jenis Saldo Awal dapat dipilih dan alurnya sama dengan jurnal lain. (2) Keterangan pembantu menjelaskan bahwa persetujuan pimpinan keuangan dilakukan di luar sistem sebelum pengesahan. (3) Jurnal `SA` mudah dikenali pada daftar |
| Verifikasi | `npm run lint`; skenario `UAT-16` di peramban |
| Risiko/pemilik | Developer. Jangan membangun alur persetujuan kedua di dalam sistem — `ACC-DEC-033` menempatkannya di luar sistem |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`READY`** — 7 September 2026. Kedua dependency lunas: `FE-ACC-006` `IMPLEMENTED`, `BE-ACC-014` `DONE` (**nol baris kode berubah** — ia memverifikasi jalur jurnal yang sudah ada). Jenis jurnal `SA` sudah terisi seeder sejak 3 September 2026. Task paling ringan yang tersisa: **tidak ada layar baru**. Status `BLOCKED` sebelumnya sudah usang sejak 4 September 2026 |

---

## Ringkasan penghalang

| Penghalang | Task terdampak | Pemilik | Cara menutup |
|---|---|---|---|
| ~~Blueprint dan kontrak masih `draft`~~ | — | — | **DITUTUP 1 Sep 2026.** Blueprint `approved`; kontrak kini `ACC-API-0.5` |
| ~~`ACC-FE-001` letak menu~~ | — | — | **DITUTUP 4 Sep 2026.** Pilihan B, `src/app/corporate/accounting/` |
| ~~`ACC-FE-003` bentuk layar rincian~~ | — | — | **DITUTUP 4 Sep 2026.** Halaman tersendiri, `base-detail-view.jsx` |
| ~~Endpoint belum ada~~ | — | — | **DITUTUP.** Seluruh **15** task backend `DONE`; 31 endpoint berdiri |

**Nol penghalang tersisa.** Tidak ada satu pun task frontend yang tertahan keputusan produk,
kontrak, atau endpoint. Yang tersisa hanya urutan di antara `FE-ACC-###` sendiri, dan satu-satunya
rantai yang benar-benar mengikat adalah `FE-ACC-009` yang menunggu `FE-ACC-008`.

### Koreksi register — 7 September 2026

Sampai hari ini `FE-ACC-008`, `FE-ACC-010`, dan `FE-ACC-011` masih tertulis **`BLOCKED` berantai**
padahal prasyaratnya sudah lunas. Untuk dua di antaranya keadaan itu **sudah tidak benar sejak 4
September 2026**, dan tabel ringkasan gelombang bahkan masih menandai `MVP-1` kerangka `BLOCKED`
padahal keempat task-nya `IMPLEMENTED`.

Sebabnya bukan perubahan keputusan, melainkan register yang tidak ikut diperbarui: status ditulis
ketika seluruh backend memang belum ada, lalu `BE-ACC-007`..`015` selesai berurutan tanpa ada yang
menelusuri balik task frontend mana yang ikut terbuka karenanya.

| Task | Tertulis | Sebenarnya | Terbuka sejak |
|---|---|---|---|
| `MVP-1` kerangka | `BLOCKED` | `IMPLEMENTED` | 4 Sep 2026 |
| `FE-ACC-008` | `BLOCKED` berantai | **`READY`** | 4 Sep 2026 — `BE-ACC-012` `DONE` 3 Sep, `FE-ACC-001` `IMPLEMENTED` 4 Sep |
| `FE-ACC-009` | `BLOCKED` berantai | `BLOCKED` — **hanya oleh `FE-ACC-008`** | — |
| `FE-ACC-010` | `BLOCKED` berantai | **`READY`** | 7 Sep 2026 — dibuka oleh `FE-ACC-007` |
| `FE-ACC-011` | `BLOCKED` berantai | **`READY`** | 4 Sep 2026 — `BE-ACC-014` `DONE` 3 Sep, `FE-ACC-006` `IMPLEMENTED` 4 Sep |

Setiap dependency diperiksa ulang terhadap source `6d4fc26`, bukan terhadap catatan status:
`GeneralLedgerController` memuat `GET /movements`, `/trial-balance`, dan
`/account-balance/{accountId}`; `JournalController` memuat `POST /journals/{id}/reverse`; dan
jenis jurnal `SA` sudah terisi seeder sejak 3 September 2026.

**Akibatnya `FE-ACC-008`, `FE-ACC-010`, dan `FE-ACC-011` dapat dikerjakan paralel.** Frontend
berdiri di **7 dari 11**.

Pelajarannya untuk register ini: status `BLOCKED` berantai perlu menyebut **task mana** yang
menahannya, bukan kata "berantai" saja. Tanpa nama itu, tidak ada yang tahu kapan ia berhenti
menahan — dan itulah yang membuat tiga task menganggur tiga hari.

### Kolom `Kontrak` ikut dirapikan — 7 September 2026

Kesebelas task masih menulis **`ACC-API-0.1`**, empat revisi tertinggal. Frontmatter roadmap
menulis `ACC-VALIDATION-0.2` dan `ACC-PERMISSION-0.1` yang juga sudah naik. Sama seperti status
`BLOCKED` di atas, ini boilerplate yang tidak pernah ikut diperbarui — bukan catatan sejarah.

| Rujukan | Sebelumnya | Sekarang |
|---|---|---|
| API | `ACC-API-0.1` (12 tempat) | **`ACC-API-0.5`** |
| Validasi | `ACC-VALIDATION-0.2` (3 tempat) | **`ACC-VALIDATION-0.3`** |
| Permission | `ACC-PERMISSION-0.1` (1 tempat) | **`ACC-PERMISSION-0.3`** |
| State | `ACC-STATE-0.1` | **tidak berubah** — memang masih `0.1` |

**Kolom `Kontrak` menyatakan kontrak yang MENGIKAT task, bukan yang dipakai saat mengerjakannya.**
Untuk task yang sudah `IMPLEMENTED`, versi yang benar-benar dipakai tercatat pada baris `Status`
masing-masing dan pada laporan task-nya — `FE-ACC-001`..`004` di atas `ACC-API-0.3`,
`FE-ACC-005`/`006` di atas `0.4`, `FE-ACC-007` di atas `0.5`. Kalimat *"dikerjakan di atas
`ACC-API-0.4`"* pada status `FE-ACC-005` karena itu **sengaja dibiarkan** — ia fakta sejarah,
bukan boilerplate.

Dua perbaikan ketelitian lain pada baris `Kontrak` `FE-ACC-007`:

- *"empat endpoint aksi"* → **lima**. Benar saat roadmap ditulis (`BE-ACC-011` menurunkan submit,
  approve, reject, post), tetapi `reverse` menyusul lewat `BE-ACC-013` dan baris `Cakupan` task
  yang sama sudah menyebut lima tombol. Kedua baris itu saling bertentangan sampai hari ini.
- `ACC-PERMISSION-0.3` bagian 5 ditambahkan. Di situlah aturan pembuat-bukan-penyetuju
  (`ACC-DEC-016`) diwajibkan berada di service — dasar acceptance (2) task itu, dan sebelumnya
  tidak dirujuk sama sekali.

## Ruang `DEV_DISCRETION`

Lima hal berikut memang diserahkan ke developer dan **tidak** perlu menunggu keputusan produk:
susunan dan urutan kolom tabel, penempatan berkas CSS Module, pemilihan ikon, bentuk konfirmasi
tindakan berisiko, dan cara mempertahankan pilihan badan hukum antar layar. Rinciannya di
`03-frontend-architecture.md` bagian 7.

## Yang sengaja tidak ada di roadmap ini

| Yang tidak ada | Alasan |
|---|---|
| Layar pemetaan posting dan kotak masuk kejadian | Phase 2 menurut `ACC-DEC-009` |
| Layar Laba Rugi dan Neraca | `ACC-DEC-030` membatasi laporan MVP pada Neraca Saldo dan Buku Besar |
| Layar tutup buku berdaftar periksa | Ditunda; penutupan periode tetap tersedia lewat `FE-ACC-004` |
| Komponen tabel atau penyaring baru | `DataTable` dan `DataFilter` sudah ada dan wajib dipakai |
