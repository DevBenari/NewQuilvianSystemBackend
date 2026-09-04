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
contracts: [ACC-API-0.1, ACC-STATE-0.1, ACC-VALIDATION-0.2, ACC-PERMISSION-0.1, ACC-TEST-0.1, ACC-MVP-0.1]
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
| `MVP-1` Kerangka dan master | `FE-ACC-001` sampai `FE-ACC-004` | `BLOCKED` | `BE-ACC-007` sampai `009` selesai |
| `MVP-1` Jurnal | `FE-ACC-005` sampai `FE-ACC-007` | `IN_PROGRESS` — 005 dan 006 `IMPLEMENTED` | `BE-ACC-010`, `BE-ACC-011` selesai |
| `MVP-2` Laporan | `FE-ACC-008`, `FE-ACC-009` | `BLOCKED` | `BE-ACC-012` selesai |
| `MVP-3` Koreksi dan saldo awal | `FE-ACC-010`, `FE-ACC-011` | `BLOCKED` | `BE-ACC-013`, `BE-ACC-014` selesai |

Dua keputusan produk menahan sebagian task, dan keduanya murah untuk diputuskan:

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
| Kontrak | `ACC-API-0.1` — belum memanggil endpoint bisnis |
| Reuse | `hero.jsx`, `access-denied-gate.jsx`, `filter-select.jsx`, pola folder `src/app/hr/` dan `src/components/view/**` |
| Cakupan | Folder rute sesuai keputusan `ACC-FE-001`, berkas rute tipis, komponen tata letak di `src/components/view/**`, komponen pemilih badan hukum, penyimpanan pilihan badan hukum antar layar |
| Dependency | **`ACC-FE-001`** harus diputuskan lebih dahulu; `BE-ACC-007` untuk daftar badan hukum |
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
| Kontrak | `ACC-API-0.1` grup Chart of Account |
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
| Kontrak | `ACC-API-0.1` grup Journal Type |
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
| Kontrak | `ACC-API-0.1` grup Accounting Period; `ACC-STATE-0.1` bagian 2 |
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
| Kontrak | `ACC-API-0.1` grup Journal, endpoint daftar |
| Reuse | `DataTable`, `DataFilter`, `filter-date-picker.jsx`, `filter-select.jsx`, `status-badge.jsx` |
| Cakupan | `accounting-journal-slice.jsx` ditulis manual mengikuti bentuk state hasil factory, didaftarkan di `store.jsx`; layar daftar dengan penyaring lengkap |
| Dependency | `FE-ACC-001`, `BE-ACC-010` |
| Acceptance | (1) Penyaring bekerja dan pagination memakai bentuk `{ pageNumber, pageSize, totalData, totalPage, items }`. (2) Status jurnal tampil berlabel Bahasa Indonesia. (3) Flag pemuatan terpisah per operasi, bukan satu penanda untuk seluruh halaman. (4) Daftar kosong menampilkan pesan yang menjelaskan sebabnya |
| Verifikasi | `npm run lint`; pemeriksaan manual |
| Risiko/pemilik | Developer |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner. Rute `/corporate/accounting/journals` berdiri, menu **Jurnal** terdaftar. Slice memakai `createMasterDataResourceSlice` + 5 thunk alur kerja — delta terhadap kalimat "ditulis manual" dicatat di laporan bagian 3. Dikerjakan di atas **`ACC-API-0.4`**. Validasi: lint PASS, 434 test PASS, build PASS. Acceptance (2), (3), (4) terbukti; (1) terbukti sebagian. Laporan: [`../task/report/frontend/fe-acc-005-daftar-jurnal.md`](../task/report/frontend/fe-acc-005-daftar-jurnal.md) |

### `FE-ACC-006` — Form jurnal dengan baris dinamis

| Field | Isi |
|---|---|
| Outcome | Petugas dapat menyusun jurnal beserta barisnya, melihat selisih debit-kredit secara langsung, dan menyimpannya walaupun belum seimbang |
| Trace | `ACC-DEC-019`, `ACC-DEC-020`, `ACC-DEC-025`; `FR-ACC-020`, `024`, `025`; `EPIC ACC-04` |
| Kontrak | `ACC-API-0.1` endpoint buat dan ubah jurnal; `ACC-VALIDATION-0.2` bagian 3 |
| Reuse | `react-hook-form` dengan `FormProvider` dan `Controller`, `summary-grid.jsx`, `base-form-control.jsx`, `resource-filter-select.jsx` |
| Cakupan | Layar form: kepala jurnal ditambah tabel baris yang dapat ditambah dan dihapus. Ringkasan total debit, total kredit, dan selisih yang dihitung ulang setiap kali angka berubah. Kolom unit biaya menjadi wajib mengikuti `RequiresCostCenter` **dari respons backend**, bukan aturan yang dihafal frontend |
| Dependency | `FE-ACC-005`, `BE-ACC-010` |
| Acceptance | (1) Tombol Ajukan mati selama selisih belum nol, dan selisihnya tampil berwarna peringatan. (2) Daftar pilihan akun hanya memuat akun yang menerima transaksi — diambil dari `/options`, tidak disaring ulang di frontend. (3) Memilih akun beban memunculkan kewajiban unit biaya. (4) Baris jurnal muat di layar tanpa gulir mendatar. (5) Menutup layar di tengah pengisian tidak menghilangkan draft yang sudah tersimpan |
| Verifikasi | `npm run lint`; `npm run test:unit`; skenario `UAT-02`, `UAT-04` di peramban |
| Risiko/pemilik | **Tertinggi pada frontend.** Developer. Ini satu-satunya layar yang tidak mengikuti pola form biasa |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`IMPLEMENTED`** — 4 September 2026, menunggu verifikasi manual owner. `react-hook-form` + `useFieldArray`, `SummaryGrid`, resource select `costCenters`. Uang dibandingkan sebagai bilangan bulat sen. Kewajiban unit biaya dibaca dari `RequiresCostCenter` pada respons `/options`. Validasi: lint PASS, 434 test PASS, build PASS. Acceptance (2) terbukti; (1), (3), (5) terbukti di kode; (4) terbukti sebagian. **`UAT-02`/`UAT-04` belum dapat dijalankan siapa pun — `BLK-ACC-02`.** Laporan: [`../task/report/frontend/fe-acc-006-form-jurnal-baris-dinamis.md`](../task/report/frontend/fe-acc-006-form-jurnal-baris-dinamis.md) |

### `FE-ACC-007` — Rincian jurnal dan tombol aksi

| Field | Isi |
|---|---|
| Outcome | Petugas dapat melihat isi jurnal, riwayat persetujuannya, dan menjalankan tindakan yang memang menjadi haknya |
| Trace | `ACC-DEC-010`, `ACC-DEC-016`; `FR-ACC-030` sampai `034`; `EPIC ACC-05` |
| Kontrak | `ACC-API-0.1` endpoint rincian dan empat endpoint aksi; `ACC-STATE-0.1` bagian 1 |
| Reuse | `base-detail-view.jsx` atau `base-detail-side-panel.jsx` sesuai keputusan `ACC-FE-003`, `confirm-modal.jsx`, `status-badge.jsx`, `toast-stack.jsx` |
| Cakupan | Layar rincian baca-saja; riwayat persetujuan; tombol Ajukan, Setujui, Tolak, Sahkan, Balik yang ditampilkan **berdasarkan `AvailableActions` dari backend**; isian alasan pada Tolak dan Balik |
| Dependency | **`ACC-FE-003`** harus diputuskan; `FE-ACC-005`, `BE-ACC-011` |
| Acceptance | (1) Tombol ditampilkan dari `AvailableActions`, **bukan** dihitung frontend. (2) Tombol Setujui tidak muncul pada jurnal buatan pengguna sendiri. (3) Setelah setiap aksi berhasil, rincian dimuat ulang dari backend — status baru tidak ditebak. (4) Tombol dimatikan selama `actionLoading` menyala, sehingga tidak terkirim dua kali. (5) Pesan `422` dan `403` dari backend ditampilkan apa adanya |
| Verifikasi | `npm run lint`; skenario `UAT-01`, `UAT-03`, `UAT-06`, `UAT-13` di peramban |
| Risiko/pemilik | Product owner untuk bentuk layar; developer untuk sisanya. Butir (1) penting: menghitung kewenangan di frontend berarti menyalin aturan bisnis ke tempat yang salah |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`BLOCKED` berantai** oleh `FE-ACC-005`. `ACC-FE-003` sendiri sudah `closed` — halaman tersendiri, `base-detail-view.jsx` |

---

## `MVP-2` — Laporan

### `FE-ACC-008` — Buku besar

| Field | Isi |
|---|---|
| Outcome | Petugas dapat menelusuri mutasi satu akun beserta saldo berjalannya |
| Trace | `ACC-DEC-030`; `FR-ACC-052`; `EPIC ACC-07` |
| Kontrak | `ACC-API-0.1` endpoint `/movements` |
| Reuse | `DataTable`, `DataFilter`, `filter-date-picker.jsx`, `resource-filter-select.jsx` |
| Cakupan | `accounting-general-ledger-slice.jsx` didaftarkan di `store.jsx`; layar mutasi dengan penyaring badan hukum, akun, dan rentang tanggal |
| Dependency | `FE-ACC-001`, `BE-ACC-012` |
| Acceptance | (1) Saldo berjalan tampil per baris. (2) Rentang tanggal terbalik ditolak dan pesannya terbaca. (3) Angka rupiah diformat konsisten |
| Verifikasi | `npm run lint`; pemeriksaan manual |
| Risiko/pemilik | Developer |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

### `FE-ACC-009` — Neraca saldo

| Field | Isi |
|---|---|
| Outcome | Manajemen dapat melihat posisi seluruh akun pada satu periode, beserta penanda seimbang |
| Trace | `ACC-DEC-030`, `ACC-DEC-037`; `FR-ACC-050`, `051`, `053`; `EPIC ACC-07` |
| Kontrak | `ACC-API-0.1` endpoint `/trial-balance` |
| Reuse | `DataTable`, `summary-grid.jsx` |
| Cakupan | Layar neraca saldo per badan hukum dan periode; ringkasan total debit, total kredit, dan penanda seimbang |
| Dependency | `FE-ACC-008`, `BE-ACC-012` |
| Acceptance | (1) Total debit dan total kredit tampil, beserta penanda seimbang. (2) Berpindah badan hukum mengubah angka dan tidak mencampurnya. (3) Layar menyebutkan bahwa laporan hanya memuat jurnal yang sudah disahkan |
| Verifikasi | `npm run lint`; skenario `UAT-14`, `UAT-15` di peramban |
| Risiko/pemilik | Developer. Butir (3) penting supaya pembaca tidak salah menafsirkan angka |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

---

## `MVP-3` — Koreksi dan saldo awal

### `FE-ACC-010` — Pembalikan dan penyesuaian di layar

| Field | Isi |
|---|---|
| Outcome | Manajer dapat mengoreksi jurnal yang sudah disahkan lewat layar, dengan memilih cara koreksinya |
| Trace | `ACC-DEC-017`, `ACC-DEC-029`; `FR-ACC-040` sampai `043`; `EPIC ACC-06` |
| Kontrak | `ACC-API-0.1` endpoint `reverse`; `ACC-VALIDATION-0.2` bagian 5 |
| Reuse | `confirm-modal.jsx`, komponen baris jurnal dari `FE-ACC-006` |
| Cakupan | Dialog pembalikan: pilihan pembalikan penuh atau jurnal penyesuaian, isian alasan wajib, dan tabel baris selisih bila memilih penyesuaian |
| Dependency | `FE-ACC-007`, `BE-ACC-013` |
| Acceptance | (1) Kedua cara koreksi tersedia beserta penjelasan singkat kapan memakai yang mana. (2) Alasan wajib diisi. (3) Baris selisih pada penyesuaian harus seimbang sebelum dapat dikirim. (4) Setelah berhasil, layar menampilkan tautan ke jurnal pembalik yang baru, dan jurnal asal tetap berstatus disahkan |
| Verifikasi | `npm run lint`; skenario `UAT-10`, `UAT-11`, `UAT-12` di peramban |
| Risiko/pemilik | Developer. Butir (1) menentukan apakah petugas memilih cara yang benar |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

### `FE-ACC-011` — Saldo awal di layar

| Field | Isi |
|---|---|
| Outcome | Saldo pembuka dapat dimasukkan lewat layar jurnal biasa, dengan penjelasan yang cukup bagi petugas |
| Trace | `ACC-DEC-018`, `ACC-DEC-033`; `FR-ACC-060`, `FR-ACC-061`; `EPIC ACC-08` |
| Kontrak | `ACC-API-0.1` grup Journal |
| Reuse | Seluruh layar jurnal yang sudah ada. **Tidak ada layar baru** |
| Cakupan | Penyesuaian kecil pada form jurnal: keterangan pembantu saat jenis `SA` dipilih, dan penanda pada daftar jurnal bahwa jurnal `SA` adalah saldo pembuka |
| Dependency | `FE-ACC-006`, `BE-ACC-014` |
| Acceptance | (1) Jenis Saldo Awal dapat dipilih dan alurnya sama dengan jurnal lain. (2) Keterangan pembantu menjelaskan bahwa persetujuan pimpinan keuangan dilakukan di luar sistem sebelum pengesahan. (3) Jurnal `SA` mudah dikenali pada daftar |
| Verifikasi | `npm run lint`; skenario `UAT-16` di peramban |
| Risiko/pemilik | Developer. Jangan membangun alur persetujuan kedua di dalam sistem — `ACC-DEC-033` menempatkannya di luar sistem |
| DoD | Layar berfungsi, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

---

## Ringkasan penghalang

| Penghalang | Task terdampak | Pemilik | Cara menutup |
|---|---|---|---|
| Blueprint dan kontrak masih `draft` | Seluruhnya | Rizki | Approval blueprint |
| Endpoint belum ada | Seluruhnya | Owner Backend | Selesaikan `BE-ACC-007` dan seterusnya |
| ~~`ACC-FE-001` letak menu~~ | — | — | **DITUTUP 4 Sep 2026.** Pilihan B, `src/app/corporate/accounting/` |
| ~~`ACC-FE-003` bentuk layar rincian~~ | — | — | **DITUTUP 4 Sep 2026.** Halaman tersendiri, `base-detail-view.jsx` |
| ~~Endpoint belum ada~~ | — | — | **DITUTUP.** Seluruh 14 task backend `DONE`; 31 endpoint berdiri |

**Kedua keputusan produk itu sudah diambil 4 September 2026**, dan seluruh endpoint backend sudah
berdiri. `FE-ACC-001` karena itu tidak lagi `BLOCKED` melainkan **`READY`**, dan task frontend
berikutnya hanya bergantung pada rantai `FE-ACC-###` di antara mereka sendiri.

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
