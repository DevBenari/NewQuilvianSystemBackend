# Roadmap Delivery Frontend — Accounting Phase 2 (gelombang mandiri)

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 11
blueprint_status: approved
roadmap_revision: 3                 # amandemen 10 Sep 2026 - penyelarasan FE-ACC-P2-001/002
                                    # dengan AccountingPeriodController; ACC-DEC-070
roadmap_status: APPROVED
approved_by: [Rizki]
approved_at: 2026-09-10
source_backend: 02c3219
source_frontend: e732424eb          # branch RizkiV2
decision_revision: 2.3              # ACC-DEC-070
contracts: [ACC-API-0.9, ACC-STATE-0.2, ACC-VALIDATION-0.6, ACC-PERMISSION-0.4]
scope_waves: [P2-0a, P2-3, P2-4, P2-5, P2-CTRL, P2-RECON]
```

## Baca ini lebih dahulu

Enam task frontend, seluruhnya menunggu endpoint backend pasangannya berdiri. Tidak ada task
frontend yang boleh dimulai sebelum kontraknya benar-benar dapat dipanggil — pelajaran dari MVP,
di mana layar dibangun lebih dahulu lalu ternyata nama DTO-nya berbeda (`ACC-GAP-004`).

### Tiga aturan yang mengikat seluruh task

1. **Pakai ulang komponen yang sudah ada.** Sebelas layar Accounting MVP sudah menyediakan tabel
   berhalaman, pemilih badan hukum, modal konfirmasi, dan tabel baris jurnal. Membuat tandingan
   adalah pelanggaran, bukan pilihan gaya.
2. **`globals.css` tidak disentuh.** Gaya khusus ditulis sebagai CSS Module milik layarnya.
3. **Tombol yang tidak boleh ditekan dimatikan, bukan disembunyikan** — mengikuti pola sebelas
   layar MVP.

### Yang `DEV_DISCRETION`

Warna, jarak, ikon, urutan kolom tabel, dan pilihan component library. Yang **bukan**
`DEV_DISCRETION`: isi layar, sumber datanya, hak akses tiap tombol, dan bunyi keadaan kosong
serta gagal — ketiganya sudah dikunci [`03-frontend-architecture.md`](../03-frontend-architecture.md)
bagian 11.

---

## Ringkasan task

| ID | Judul | Gelombang | Dependency backend | Status |
|---|---|---|---|---|
| `FE-ACC-P2-001` 🟡 | Layar Daftar Periksa Penutupan | `P2-4` | `BE-ACC-P2-005` | 🟡 `SEBAGIAN` |
| `FE-ACC-P2-002` 🟡 | Aksi penutupan: ajukan, setujui, tolak | `P2-4` | `BE-ACC-P2-006` | 🟡 `SEBAGIAN` |
| `FE-ACC-P2-003` 🟡 | Layar daftar Jurnal Berulang | `P2-3` | `BE-ACC-P2-007` | 🟡 `SEBAGIAN` |
| `FE-ACC-P2-004` 🟡 | Form Jurnal Berulang | `P2-3` | `BE-ACC-P2-007` | 🟡 `SEBAGIAN` |
| `FE-ACC-P2-005` 🟡 | Layar Pengaturan Akuntansi | `P2-0a` | `BE-ACC-P2-009` | 🟡 `SEBAGIAN` |
| `FE-ACC-P2-006` 🟡 | Layar Tutup Tahun | `P2-5` | `BE-ACC-P2-010` | 🟡 `SEBAGIAN` |
| `FE-ACC-P2-007` | **Penanda control account pada layar COA** | `P2-CTRL` | `BE-ACC-P2-011` | `READY` |
| `FE-ACC-P2-008` | **Layar Rekonsiliasi Control Account** | `P2-RECON` | `BE-ACC-P2-013` | `READY` sebagian — kolom subledger kini menunggu **gelombang `P2-1`**, bukan menunggu keputusan owner |

---

## 🟡 `FE-ACC-P2-001` — Layar Daftar Periksa Penutupan

| Field | Isi |
|---|---|
| Outcome | Petugas melihat apa saja yang menahan penutupan bulan, dan tahu apa yang harus dikerjakan |
| Trace | `ACC-DEC-051`, **`ACC-DEC-065`**, **`ACC-DEC-070`**; `FR-P2-023`, `FR-P2-025` |
| Kontrak | `GET /periods/{id}/closing-checklist` dan `GET /periods/{id}/closing-history`, dijaga `AccountingPeriod : Read`, pada `ACC-API-0.9`. **Bukan** `accounting-periods`, **bukan** `Period : Read` |
| Reuse | Tabel dan kartu dari layar Periode Akuntansi yang sudah ada; pemilih badan hukum |
| Cakupan | Layar anak dari Periode Akuntansi, dibuka lewat tombol Tutup Periode. `accounting-period-closing-slice.jsx` |
| Dependency | `BE-ACC-P2-005` |
| Acceptance | (1) **Tiga** penghalang (`ACC-DEC-051` + `ACC-DEC-065`) dan **enam** peringatan (`ACC-DEC-051` + `ACC-DEC-070`) tampil terpisah dan **terbaca bedanya**. (2) Butir berkeadaan `NotYetAvailable` dirender **berbeda** dari butir yang sudah diperiksa dan bersih, tanpa menampilkan angka, disertai `UnavailableReason`-nya. (3) Selama `IsComplete` bernilai `false`, spanduk kelengkapan tampil di **atas** daftar memuat `NotYetAvailableCount`. (4) **Tidak memakai cache** — layar dibuka ulang selalu memanggil endpoint lagi. (5) Tautan Lihat membuka daftar jurnal yang sudah tersaring ke periode itu. (6) Keadaan kosong berbunyi "Tidak ada penghalang." (7) Riwayat penutupan tampil dari `closing-history`; gagalnya riwayat **tidak** menghilangkan daftar periksa |
| Verifikasi | `npm run lint`; `npm run build`; unit test slice; UAT peramban `UAT-P2-14` |
| Risiko/pemilik | **Dua risiko.** (a) Menampilkan angka dari cache membuat petugas mengambil keputusan penutupan berdasarkan keadaan lama. (b) **Merender butir `NotYetAvailable` sama seperti butir bersih.** Dua dari tiga penghalang belum dapat diperiksa selama gelombang `P2-1` belum dibangun; bila dirender sama, petugas menutup periode padahal tidak ada yang pernah memeriksa shift kasirnya. Owner Frontend |
| DoD | Lint dan build hijau, unit test lulus, laporan task tertulis |
| Status | 🟡 **`SEBAGIAN`** — 10 September 2026. `npm run lint` 0 error; `npm run build` compiled; `node --test tests/unit/` **615 lulus 0 gagal** (17 uji baru). **Diuji terhadap backend sungguhan:** bentuk respons cocok 100% dengan normalizer, 3 penghalang + 6 peringatan, `notYetAvailableCount=7`, dan payload aslinya disimpan sebagai fixture. **Belum terpenuhi:** acceptance (5) — tautan Lihat belum menyaring ke periode; `UAT-P2-14` `NOT FEASIBLE`. Laporan: [`fe-acc-p2-001`](../task/report/frontend/fe-acc-p2-001-daftar-periksa-penutupan.md) |


## 🟡 `FE-ACC-P2-002` — Aksi penutupan: ajukan, setujui, tolak

| Field | Isi |
|---|---|
| Outcome | Penutupan bulan dapat ditempuh penuh oleh dua orang berbeda lewat layar |
| Trace | `ACC-DEC-052`, `ACC-DEC-055`, `ACC-DEC-016`; `FR-P2-026`, `027`, `028` |
| Kontrak | `POST /periods/{id}/submit-closing` (`AccountingPeriod : Close`), `/approve-closing` dan `/reject-closing` (`AccountingPeriod : Approve`), pada `ACC-API-0.9`. **Bukan** `accounting-periods`, **bukan** `Period : ...` |
| Reuse | Modal konfirmasi beralasan dari layar Buka Kembali Periode yang sudah ada |
| Cakupan | Tiga tombol pada layar `FE-ACC-P2-001`; modal alasan penolakan. **Tidak ada layar sendiri** |
| Dependency | `BE-ACC-P2-006`; **`FE-ACC-P2-001`** — lihat catatan urutan di bawah kartu ini |
| Acceptance | (1) Tombol Ajukan **mati** selama masih ada penghalang, dan alasannya terbaca di layar. (2) Tombol Setujui dan Tolak **mati** bila pembuka layar adalah yang mengajukan, disertai keterangan. (3) Penolakan tanpa alasan tidak dapat dikirim. (4) Tombol yang bukan haknya dimatikan, bukan disembunyikan |
| Verifikasi | `npm run lint`; `npm run build`; unit test; UAT peramban `UAT-P2-15`, `UAT-P2-16`, `UAT-P2-17`, `UAT-P2-18` |
| Risiko/pemilik | **Peran `Accounting Director` harus sudah ada** di mekanisme hak akses sebelum acceptance (2) dapat diuji sungguhan. Bergantung `BE-ACC-P2-006`. Owner Frontend |
| DoD | Lint dan build hijau, empat UAT terbukti, laporan task tertulis |
| Status | 🟡 **`SEBAGIAN`** — 10 September 2026, satu paket dengan `FE-ACC-P2-001`. Lint 0 error; build compiled; 615 uji lulus. **Alur penuh terbukti terhadap backend sungguhan** pada tahun buku uji 2019: ajukan → status 4, riwayat 1 baris; setujui oleh pengaju sendiri **403** (prinsip empat mata tegak); tolak tanpa alasan **400**; ajukan ulang **409**; tolak beralasan **200** dan periode kembali terbuka. **Satu cacat ditemukan dan diperbaiki:** urutan alasan tombol mati salah sesudah penolakan. **Belum terpenuhi:** tampilan tombol mati bagi pengguna kedua di peramban; `UAT-P2-15`..`18` `NOT FEASIBLE`. Laporan: [`fe-acc-p2-002`](../task/report/frontend/fe-acc-p2-002-aksi-penutupan-ajukan-setujui-tolak.md) |


### Urutan: `001` lebih dahulu, bukan sebaliknya

**`FE-ACC-P2-002` tidak dapat dikerjakan lebih dulu.** Cakupannya adalah *tiga tombol pada layar
`FE-ACC-P2-001`* — ia menumpang layar daftar periksa dan tidak punya layar sendiri. Tabel
dependency di atas hanya menyebut `BE-ACC-P2-006`, sehingga kartunya **terbaca seolah berdiri
sendiri**; itu yang mudah salah dibaca saat memilih task berikutnya.

Kerjakan `001` lebih dahulu, atau keduanya sebagai **satu paket**. Yang tidak bisa: memulai `002`
sementara `001` belum ada, karena tombolnya tidak punya tempat untuk diletakkan.

Ada alasan kedua untuk menjadikannya satu paket: acceptance (1) `002` berbunyi "tombol Ajukan mati
selama masih ada penghalang", dan daftar penghalang itu **milik** `001`. Menguji `002` tanpa `001`
berarti menguji tombol tanpa hal yang menentukan nyala-matinya.

## 🟡 `FE-ACC-P2-003` — Layar daftar Jurnal Berulang

| Field | Isi |
|---|---|
| Outcome | Petugas melihat template yang ada beserta jadwal terbit dan status aktifnya |
| Trace | `ACC-DEC-050`; `FR-P2-022` |
| Kontrak | `GET /recurring-journals`, `GET /{id}/runs` |
| Reuse | Komponen tabel berhalaman dan pemilih badan hukum yang sudah ada |
| Cakupan | Butir menu tingkat 2 `/accounting/recurring-journals`; `accounting-recurring-journal-slice.jsx` |
| Dependency | `BE-ACC-P2-007` |
| Acceptance | (1) Template aktif dan tidak aktif terbedakan jelas. (2) Riwayat penerbitan per periode dapat dibuka beserta tautan ke jurnal yang dihasilkan. (3) Tombol Aktifkan hanya menyala bagi yang berhak. (4) Keadaan kosong berbunyi wajar |
| Verifikasi | `npm run lint`; `npm run build`; unit test slice |
| Risiko/pemilik | Owner Frontend |
| DoD | Lint dan build hijau, laporan task tertulis |
| Status | 🟡 **`SEBAGIAN`** — 11 September 2026, satu paket dengan `FE-ACC-P2-004`. `npm run lint` 0 error (warning tidak bertambah); `npm run build` compiled 44 dtk, rute daftar dan rincian terdaftar; `node --test tests/unit/` **640 lulus 0 gagal** (13 baru). Keempat acceptance terpenuhi di kode; tombol Aktifkan dijaga **`RecurringJournal : Activate`** sesuai controller, amandemen kontrak `ACC-API-0.10` menunggu ratifikasi. **Belum:** uji peramban, diserahkan kepada Rizki; acceptance (2) belum dapat diperlihatkan dengan data nyata karena penjadwal mati dan belum ada template. Laporan: [`fe-acc-p2-003`](../task/report/frontend/fe-acc-p2-003-layar-daftar-jurnal-berulang.md) |

## 🟡 `FE-ACC-P2-004` — Form Jurnal Berulang

| Field | Isi |
|---|---|
| Outcome | Template dapat dibuat dan diubah, dengan penjaga keseimbangan yang terlihat saat mengetik |
| Trace | `ACC-DEC-050`, `ACC-DEC-019`; `FR-P2-018` |
| Kontrak | `POST /recurring-journals`, `PUT /{id}` |
| Reuse | **Komponen tabel baris dari Form Jurnal MVP**, beserta total berjalan dan penjaga keseimbangannya. Membuat komponen tandingan **dilarang** |
| Cakupan | Layar anak dari `FE-ACC-P2-003` |
| Dependency | `BE-ACC-P2-007` |
| Acceptance | (1) Total debit dan kredit tampil berjalan saat mengetik, dan tombol Simpan mati selama tidak seimbang. (2) Kolom cost center **wajib muncul** saat akun berjenis beban dipilih. (3) Satu baris hanya menerima debit **atau** kredit. (4) Komponen baris yang dipakai **terbukti** komponen yang sama dengan Form Jurnal, bukan salinan |
| Verifikasi | `npm run lint`; `npm run build`; unit test; UAT peramban `UAT-P2-13` |
| Risiko/pemilik | Menyalin komponen tabel baris membuat dua tempat yang harus diperbaiki setiap kali aturan keseimbangan berubah. Acceptance (4) ada justru untuk mencegahnya. Owner Frontend |
| DoD | Lint dan build hijau, laporan task menyebut komponen mana yang dipakai ulang |
| Status | 🟡 **`SEBAGIAN`** — 11 September 2026. Lint 0 error; build compiled, rute `/create` dan `/[slug]/update` terdaftar; 640 uji lulus. Keempat acceptance terpenuhi: **`JournalLineRow` milik `FE-ACC-006` diimpor apa adanya**, hitungan keseimbangan memakai util bersama baru alih-alih salinan ketiga. **Belum:** `UAT-P2-13`, diserahkan kepada Rizki. Laporan: [`fe-acc-p2-004`](../task/report/frontend/fe-acc-p2-004-form-jurnal-berulang.md) |

## 🟡 `FE-ACC-P2-005` — Layar Pengaturan Akuntansi

| Field | Isi |
|---|---|
| Outcome | Akun laba ditahan dapat ditetapkan per badan hukum |
| Trace | `ACC-DEC-054`; `FR-P2-032` |
| Kontrak | `GET /configuration/{legalEntityId}`, `PUT /configuration/{legalEntityId}` |
| Reuse | Pemilih akun (`ChartOfAccountOptionDto`) dari Form Jurnal |
| Cakupan | Butir menu tingkat 3 di bawah Master Data; layar kecil |
| Dependency | `BE-ACC-P2-009` |
| Acceptance | (1) Pemilih akun **hanya menampilkan akun Ekuitas yang menerima transaksi**. (2) Keadaan kosong menjelaskan bahwa tutup tahun belum dapat dijalankan. (3) Hanya yang berhak dapat menyimpan |
| Verifikasi | `npm run lint`; `npm run build`; unit test |
| Risiko/pemilik | Owner Frontend |
| DoD | Lint dan build hijau, laporan task tertulis |
| Status | 🟡 **`SEBAGIAN`** — 10 September 2026. `npm run lint` 0 error (jumlah warning tidak bertambah); `npm run build` compiled, route `/corporate/accounting/configuration` terdaftar; `node --test tests/unit/` **627 lulus 0 gagal**. Ketiga acceptance **terpenuhi**, termasuk penjaga hak akses `AccountingConfiguration : Update` pada tombol Simpan. **Belum:** UAT peramban, diserahkan kepada Rizki atas permintaannya. Laporan: [`fe-acc-p2-005`](../task/report/frontend/fe-acc-p2-005-layar-pengaturan-akuntansi.md) |

## 🟡 `FE-ACC-P2-006` — Layar Tutup Tahun

| Field | Isi |
|---|---|
| Outcome | Petugas melihat perhitungan tutup tahun sebelum memutuskan, lalu menyusun jurnal penutupnya |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`; `FR-P2-030`, `031`, `032`, `033` |
| Kontrak | `GET /year-end-closing/preview`, `POST /year-end-closing/generate` |
| Reuse | Komponen tabel dan format rupiah dari layar Neraca Saldo |
| Cakupan | Butir menu tingkat 2 `/accounting/year-end-closing`; `accounting-year-end-slice.jsx` |
| Dependency | `BE-ACC-P2-010` |
| Acceptance | (1) Layar menyatakan tegas bahwa **Pratinjau tidak membuat apa pun**. (2) Tombol Susun **mati** bila pratinjau kosong atau akun laba ditahan belum ditetapkan, disertai tautan ke Pengaturan Akuntansi. (3) Penolakan karena periode belum tertutup menampilkan **periode mana saja**, bukan pesan umum. (4) Tidak memakai cache |
| Verifikasi | `npm run lint`; `npm run build`; unit test; UAT peramban `UAT-P2-19`, `UAT-P2-20`, `UAT-P2-21`, `UAT-P2-22` |
| Risiko/pemilik | Tutup tahun terasa menakutkan bagi petugas. Acceptance (1) bukan hiasan — tanpanya orang enggan menekan Pratinjau dan justru menebak angkanya. Owner Frontend |
| DoD | Lint dan build hijau, empat UAT terbukti, laporan task tertulis |
| Status | 🟡 **`SEBAGIAN`** — 10 September 2026. Lint 0 error; build compiled, route `/corporate/accounting/year-end-closing` terdaftar; 627 uji lulus. Keempat acceptance **terpenuhi**: jaminan "Pratinjau tidak membuat apa pun" dirender sebelum tombolnya, `422` disertai tautan ke Pengaturan Akuntansi sementara `409` menampilkan daftar periode dari backend apa adanya, dan pratinjau tanpa cache. **Belum:** `UAT-P2-19` sampai `22`, diserahkan kepada Rizki. Laporan: [`fe-acc-p2-006`](../task/report/frontend/fe-acc-p2-006-layar-tutup-tahun.md) |

---

## `FE-ACC-P2-007` — Penanda control account pada layar COA

| Field | Isi |
|---|---|
| Outcome | Petugas dapat menandai sebuah akun sebagai control account, dan melihat penandanya di daftar |
| Trace | `ACC-DEC-064` |
| Kontrak | `ACC-API-0.8` grup Chart of Account. Bidang `IsControlAccount` pada `/options` — yang dibutuhkan acceptance (3) — diajukan `ACC-API-0.11` (usulan, 11 Sep 2026) dan dibangun `BE-ACC-P2-012`. `/options` **tidak** menyaring akun control; penyaringan di hook tiap layar |
| Reuse | **Layar COA dan Form Akun yang sudah ada dari MVP.** Ini penambahan satu kolom dan satu kotak centang, **bukan layar baru** |
| Cakupan | Satu kotak centang pada Form Akun, satu kolom penanda pada tabel COA, dan penjelasan singkat maknanya |
| Dependency | `BE-ACC-P2-011` |
| Acceptance | (1) Kotak centang hanya menyala bagi yang berhak mengubah akun. (2) Tabel COA menampilkan penandanya sehingga terbaca sekilas. (3) **Layar Form Jurnal menyembunyikan atau mematikan akun control dari pemilih akunnya**, disertai keterangan kenapa — supaya petugas tahu sebelum mengisi, bukan setelah ditolak |
| Verifikasi | `npm run lint`; `npm run build`; unit test |
| Risiko/pemilik | **Acceptance (3) yang paling menentukan pengalaman pemakaian.** Tanpa itu petugas baru tahu akunnya terlarang sesudah menekan Simpan dan ditolak `422`. Owner Frontend |
| DoD | Lint dan build hijau, laporan task tertulis |
| Status | `READY` |

## `FE-ACC-P2-008` — Layar Rekonsiliasi Control Account

| Field | Isi |
|---|---|
| Outcome | Saldo control account di buku besar tampil berdampingan dengan saldo subledger, beserta selisihnya |
| Trace | `ACC-DEC-066` |
| Kontrak | `GET /reconciliation/gl-balances` dijaga `AccountingReconciliation : Read` — diajukan `ACC-API-0.11` dan `ACC-PERMISSION-0.6` (usulan, 11 Sep 2026). Tampilkan **`BalanceInNormalBalance`**, bukan `Balance`. Endpoint sisi subledger belum dirancang (`BE-ACC-P2-014`). Kolom ini sebelumnya tidak ada pada kartu |
| Reuse | Komponen tabel dan format rupiah dari layar Neraca Saldo |
| Cakupan | Butir menu tingkat 2 `/accounting/reconciliation` |
| Dependency | `BE-ACC-P2-013` untuk sisi buku besar; `BE-ACC-P2-014` untuk sisi subledger, yang kini bergantung pada gelombang `P2-1` |
| Acceptance | (1) Kolom saldo buku besar tampil segera setelah `BE-ACC-P2-013` berdiri. (2) **Kolom saldo subledger dan selisih ditampilkan sebagai "belum tersedia"**, bukan disembunyikan — supaya pembaca tahu laporannya memang belum lengkap, bukan mengira selisihnya nol. (3) Tidak memakai cache |
| Risiko/pemilik | Menyembunyikan kolom yang belum ada datanya membuat laporan **terbaca seolah sudah cocok**. Itu kesalahan yang paling mahal pada layar rekonsiliasi. Owner Frontend |
| DoD | Lint dan build hijau, laporan task tertulis |
| Status | **`READY` sebagian** — sisi buku besar dapat dikerjakan sekarang. Kolom subledger menunggu `BE-ACC-P2-014`, yang **tidak lagi terblokir keputusan** sejak `ACC-DEC-071` (10 Sep 2026) dan kini hanya menunggu gelombang `P2-1` menyediakan kotak masuk kejadiannya |

## Peta butir menu yang ditambahkan

| Butir menu | Tingkat | Induk | Route | Task |
|---|:---:|---|---|---|
| Jurnal Berulang | 2 | Accounting | `/accounting/recurring-journals` | `FE-ACC-P2-003` |
| Tutup Tahun | 2 | Accounting | `/accounting/year-end-closing` | `FE-ACC-P2-006` |
| Pengaturan Akuntansi | 3 | Accounting › Master Data | `/accounting/configuration` | `FE-ACC-P2-005` |
| Rekonsiliasi Control Account | 2 | Accounting | `/accounting/reconciliation` | `FE-ACC-P2-008` |

Daftar Periksa Penutupan **bukan** butir menu — ia layar anak dari Periode Akuntansi, sesuai
`03-frontend-architecture.md` bagian 9.

## Coverage gap yang diketahui

| Gap | Isi |
|---|---|
| `ACC-TEST-0.1` | Matriks acceptance belum punya kolom bukti yang sudah ada, sehingga sembilan UAT peramban pada roadmap ini tidak punya tempat dicatat saat dijalankan |
| Butir menu Kotak Masuk Kejadian | Tidak dibuat di roadmap ini. Penanda angka pada menu (`ACC-DEC-057`) baru bermakna setelah gelombang `P2-1` |
