# Laporan Perubahan Frontend — `FE-LAB-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-04` |
| Judul | Tampilan tarif laboratorium dan pemilih katalog |
| Slice | `S14` — katalog dan tarif laboratorium (`roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 3 |
| Trace | `FR-09.1` .. `FR-09.4`; `LAB-DEC-033`, `LAB-DEC-036`; `LAB-INH-010`; `AC-43`, `AC-47`, `AC-48`; `03-frontend-architecture.md` bagian 2 |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02. Grup Lab Catalog |
| Wewenang UI | `LAB-DEC-033` **batas kepemilikan** — tarif tetap milik Master Data, dan modul Laboratorium hanya menampilkannya. `LAB-FE-002` `DEV_DISCRETION` untuk tata letak dan pilihan komponen. Tidak ada satu pun invariant keselamatan yang tersentuh task ini |
| Dependency | `FE-LAB-01` — **selesai**. Endpoint dari `BE-LAB-07` — **selesai**, dan ketiga endpointnya diverifikasi langsung pada source backend |
| Klasifikasi | `HEAVY` — skor 9: repository 2, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 1, database 0, keamanan 0, UI/workflow 1. Duduk di batas bawah `HEAVY`, dan angkanya berasal dari jumlah berkas serta rentang dua repository — bukan dari kerumitan aturan |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — lapis modul `laboratory-management`, `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, dan satu berkas uji; serta `NewQuilvianSystemBackend` — **hanya** `docs/module-blueprints/laboratorium/task/report/frontend/FE-LAB-04.md` beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `443270f3f`, branch `YogaV2`, upstream `origin/YogaV2` |
| Commit backend yang dijadikan rujukan | `2dfc4f2`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **Selesai.** Menu tarif baca saja berdiri tanpa satu pun tombol ubah, komponen pemilih katalog berdiri dan siap dipakai ulang, dan tidak ada konstanta yang diduplikasi. Seluruh butir DoD terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

**Belum ada satu pun layar tarif maupun katalog di modul Laboratorium.** Yang sudah berdiri
hanya kerangka modul dari `FE-LAB-01` — tujuh lapisnya, `InstanceAxios` yang terpasang, dan
kontrak penanganan state-nya. Berkas konstanta modul sudah memuat alamat dasar grup
`lab-catalog`, sehingga alamatnya tidak perlu ditulis ulang.

**Ketiga endpointnya sudah ada.** Pemeriksaan langsung pada `LabCatalogController.cs`
menemukan `GET /examinations`, `GET /examinations/{procedureId}/price`, dan `GET /tariffs`.
**Tidak ada satu pun `POST`, `PUT`, maupun `DELETE`** — dan itu memang inti `LAB-DEC-033`.

**Tiga perilaku backend yang menentukan bentuk layar.**

| Perilaku | Akibatnya di layar |
| --- | --- |
| Pemeriksaan yang **belum digolongkan** disiplinnya tetap ikut selama penyaring disiplin tidak dikirim | Layar menampilkannya apa adanya sebagai "Belum digolongkan", bukan sebagai tanda hubung kosong. Backend menyebutnya keadaan sah, bukan data rusak |
| `contractPrice` kosong berarti **tidak ada kontrak** untuk penjamin itu — bukan berarti gratis | Penanda cakupan membedakan tiga keadaan: tercakup, tidak tercakup, dan penjamin belum dipilih |
| `unitPrice` kosong berarti **tarifnya belum diatur** | Pemeriksaan seperti itu tidak dihitung nol pada total; ia dihitung terpisah dan diberi peringatan |

**Yang sudah ada di frontend dan wajib dipakai ulang.** Roadmap menyebut tiga berkas konstanta
yang tidak boleh diduplikasi. Ketiganya diperiksa lebih dulu:

| Berkas | Yang dilakukan task ini |
| --- | --- |
| `master-data/insurance-tariff-constants.jsx` | **Diimpor**, bukan disalin: `ACTIVE_STATUS_OPTIONS`, `PAGE_SIZE_OPTIONS`, dan `INSURANCE_TARIFF_ROUTE_BASE` |
| `master-data/tariff-category-constants.jsx` | **Tidak disalin dan tidak dibutuhkan.** Isinya pilihan periode dan urutan yang memang tidak dipakai layar ini |
| `master-data/procedure-constants.jsx` | **Tidak disalin.** Pilihan jenis pemeriksaan diambil dari registry select bersama yang menunjuk `master-data/procedures/options` |

Ditambah `formatCurrencyIDR` dari `utils/Formatters.jsx` yang dipakai apa adanya, sehingga
tidak ada pemformat mata uang kedua yang lahir.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas laboratorium dan kepala instalasi yang perlu tahu besaran biaya
pemeriksaan — baik untuk dibaca sebagai daftar, maupun untuk dihitung sebelum memesan.

**Kenapa layar ini baca saja.** Tarif adalah data yang dipakai lintas modul: Rawat Jalan,
Rawat Inap, dan Billing membaca tarif yang sama. Bila Laboratorium boleh mengubahnya, angka
yang sama dapat berubah dari dua tempat berbeda. `LAB-DEC-033` menutup kemungkinan itu:
Laboratorium menampilkan, Master Data yang mengelola.

### 2.1 Membaca daftar tarif laboratorium

1. Pengguna membuka **Pelayanan Kesehatan → Laboratorium → Tarif Laboratorium**.
2. Di bawah judul berdiri satu panel keterangan: layar ini baca saja, dan pengelolaan tarif
   dilakukan di Master Data. Panel itu membawa satu tautan menuju layar pengelolaan yang
   sebenarnya, supaya pengguna tahu ke mana harus pergi — bukan menyimpulkan bahwa tarifnya
   tidak dapat diubah sama sekali.
3. Tabel menampilkan kode tarif, nama tarif, kode dan nama pemeriksaannya, disiplin, harga
   normal, rentang berlakunya, dan status.
4. Penyaringnya: pencarian bebas dan jenis pemeriksaan. Tidak ada tombol tambah, ubah, atau
   hapus di mana pun — juga tidak ada kolom aksi.

### 2.2 Memilih pemeriksaan beserta perkiraan biayanya

Komponen pemilih katalog dibangun pada task ini dan **dipakai layar pemesanan pada
`FE-LAB-06`**. Bentuknya:

1. Baris penyaring: pencarian bebas dan pilihan disiplin. Layar pemanggil dapat mengunci
   disiplinnya — misalnya pesanan Mikrobiologi yang sudah tahu disiplinnya — sehingga
   pilihannya tidak dapat diubah dari dalam komponen.
2. Tabel katalog menampilkan kode, nama, disiplin, harga satuan, dan satu tombol **Pilih**.
   Baris yang sudah dipilih berubah menjadi **Terpilih** dan tombolnya nonaktif.
3. Setiap kali sebuah pemeriksaan dipilih, harga berlakunya diambil dari endpoint harga —
   karena hanya di sanalah keterangan cakupan penjamin dan catatannya tersedia.
4. Panel **Pemeriksaan Terpilih** menampilkan nama, penanda cakupan penjamin, harga satuan,
   dan subtotal per baris, lalu satu baris **Total Perkiraan Biaya**.
5. Di bawahnya satu kalimat yang tidak boleh hilang: angka itu perkiraan biaya, bukan tagihan.
   Tagihan dibentuk modul Billing setelah pemeriksaan dinyatakan layak.

**Contoh `AC-43`.** Petugas memilih Hemoglobin, Urinalisa, dan Gula Darah. Ketiganya
menampilkan harga satuan masing-masing, subtotal per baris, dan total ketiganya — dan tidak
satu pun baris tagihan terbentuk, karena seluruh perintah yang dijalankan layar ini adalah
perintah baca.

### 2.3 Jalur yang tidak normal

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Pemeriksaan belum digolongkan disiplinnya | Tampil sebagai "Belum digolongkan", dan tetap dapat dipilih. Menyembunyikannya membuat katalog tampak kosong pada rumah sakit yang penggolongannya belum diisi |
| Pemeriksaan belum punya tarif | Harga satuannya tampil "Belum ada tarif", dan baris itu **tidak** dijumlahkan. Satu peringatan kuning menyebut berapa banyak yang belum bertarif, dan menyatakan total di layar lebih kecil daripada biaya sebenarnya |
| Penjamin dipilih tetapi kontraknya tidak ada | Penanda berbunyi "Tidak tercakup penjamin" — bukan dianggap gratis, dan bukan dikosongkan |
| Penjamin belum dipilih sama sekali | Penanda berbunyi "Penjamin belum dipilih", dibedakan dari tidak tercakup |
| Gagal memuat katalog atau harga | Pesan dari server ditampilkan apa adanya |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan aturan.** `AGENTS.md` frontend; `CLAUDE.md` frontend;
`rules/frontend/frontend-architecture.md`; `rules/frontend/base-component-catalog.md`;
`rules/frontend/base-component-decision-gate.md`; `rules/frontend/design-tokens.md`;
`rules/frontend/ui-consistency-checklist.md`; `rules/frontend/test-policy.md`;
`rules/frontend/REPORT_TEMPLATE.md`.

**Blueprint dan kontrak.** `roadmap/frontend-roadmap.md` bagian 3;
`03-frontend-architecture.md` bagian 2 dan 2.2; `contracts/api-contract.md` grup Lab Catalog;
`testing/acceptance-test-matrix.md` `AC-43`, `AC-47`, `AC-48`;
`roadmap/traceability.md` baris `FR-09.1` .. `FR-09.5`; `task/report/backend/BE-LAB-07.md`.

**Backend sebagai sumber kebenaran kontrak — strict read-only.**
`Areas/HealthServices/LaboratoryManagement/Controllers/LabCatalogController.cs`;
`.../DTOs/LabCatalogDtos.cs`; `.../Services/LabCatalogService.cs`;
`.../Enums/LaboratoryEnums.cs` untuk nama enum disiplin.

**Frontend sebagai acuan pola.** Kerangka `FE-LAB-01` —
`lib/constants/health-services/laboratory-management/laboratory-constants.jsx`,
`lib/services/.../lab-order.service.js`, `lib/state/slice/.../lab-order-slice.jsx`, dan
`components/view/.../laboratory-overview/`. Ditambah
`lib/constants/health-services/master-data/insurance-tariff-constants.jsx`,
`lib/constants/health-services/master-data/tariff-category-constants.jsx`,
`utils/Formatters.jsx`, `lib/hooks/select/health-service/health-service-select-resources.js`,
dan `components/features/base-features/` (data-table, data-filter, filter-select,
resource-filter-select, status-badge, information-alert, base-button, hero).

### 3.2 Berkas yang berubah

**Dua belas berkas baru, seluruhnya di dalam tujuh lapis modul:**

| Lapis | Berkas | Perubahan |
| --- | --- | --- |
| Konstanta | `lib/constants/health-services/laboratory-management/lab-catalog-constants.jsx` | Pilihan disiplin, salinan teks layar tarif dan pemilih katalog, kolom tabel tarif, penyaring bawaan, dan tiga penanda cakupan. Tiga konstanta lama **diimpor ulang**, bukan disalin |
| API service | `lib/services/health-services/laboratory-management/lab-catalog.service.js` | Tiga fungsi baca. Tidak ada satu pun fungsi tulis, dan ketiadaannya diberi keterangan |
| Redux | `lib/state/slice/health-services/laboratory-management/lab-catalog-slice.jsx` | Tiga thunk baca, penyimpanan harga per pemeriksaan, dan penanda muat per penunjuk |
| Hook | `lib/hooks/health-services/laboratory-management/use-lab-tariff-list.jsx` | Controller layar tarif: penyaring, paginasi, muat ulang. Tidak ada satu pun handler simpan |
| Hook | `lib/hooks/health-services/laboratory-management/use-lab-catalog-picker.jsx` | Controller pemilih katalog: penyaring, pilihan, pengambilan harga, dan perhitungan total |
| Komponen fitur | `components/features/health-services/laboratory-management/lab-catalog-picker/lab-catalog-picker.jsx` | Komponen pemilih katalog yang dipakai ulang layar pemesanan |
| Komponen fitur | `.../lab-catalog-picker/lab-catalog-picker-utils.js` | Perhitungan murni: harga berlaku, penanda cakupan, baris terpilih, dan penjumlahan total |
| Komponen tampilan | `components/view/health-services/laboratory-management/lab-tariffs/lab-tariff-view.jsx` | Layar tarif baca saja |
| Komponen tampilan | `.../lab-tariffs/lab-tariff-table-columns.jsx` | Definisi kolom tabel tarif, dipisah sesuai pola komposisi halaman. **Tanpa kolom aksi** |
| Style | `style/health-services/laboratory-management/lab-catalog-picker.module.css` | Jarak panel pilihan dan penekanan angka total |
| Style | `style/health-services/laboratory-management/lab-tariffs/lab-tariff.module.css` | Panel keterangan baca saja dan penekanan kolom harga |
| Route | `app/health-services/laboratory-management/lab-tariffs/page.jsx` | Route tipis: metadata dan pemanggilan view |

**Satu berkas uji dan dua berkas yang disunting:**

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/lab-catalog-picker-utils.test.mjs` | Enam uji terhadap perhitungan murni, termasuk `AC-43` dan pembedaan tiga keadaan cakupan penjamin |
| `src/lib/state/store.jsx` | Satu baris import dan satu baris pendaftaran reducer dengan kunci `labCatalog` |
| `src/utils/menu-sidebar/menu-items.jsx` | Satu butir menu **Tarif Laboratorium** di dalam grup Laboratorium yang berdiri sejak `FE-LAB-01` |

### 3.3 Kepatuhan arsitektur frontend

**Tujuh lapis dipatuhi, tanpa lapis kedelapan.** Perhitungan murni pemilih katalog ditempatkan
**bersebelahan dengan komponennya** di dalam lapis komponen fitur, bukan di
`src/utils/health-services/laboratory-management/`. Modul ini sengaja tidak membuka lapis
`utils` tersendiri, mengikuti batas yang sudah ditetapkan `FE-LAB-01`. Berkasnya tetap dapat
diuji karena ia modul biasa tanpa React.

**Kenapa layar tarif berada di `laboratory-management`, bukan `master-data`.** `LAB-FE-014`
mengikat penempatan **menu data induk** — layar yang mengelola data induk. Layar ini tidak
mengelola apa pun: ia tidak punya tombol simpan, dan endpoint yang dibacanya adalah grup Lab
Catalog milik Laboratorium, bukan endpoint master data. Layar pengelolaan tarif yang sebenarnya
tetap berada di `master-data/insurance-tariffs/`, tidak disentuh, dan justru **ditautkan** dari
layar ini.

**Bagaimana `LAB-DEC-033` ditegakkan — empat lapis.**

1. **Tidak ada fungsi tulis.** Berkas service hanya memuat tiga fungsi baca, dan ketiadaan
   fungsi tulis diberi keterangan supaya tidak terbaca sebagai kelalaian.
2. **Tidak ada thunk tulis.** Potongan Redux hanya punya tiga thunk baca.
3. **Tidak ada kolom aksi.** Definisi kolom tabel tarif tidak memuat kolom aksi sama sekali,
   dan alasannya ditulis di berkasnya.
4. **Kepemilikan dinyatakan di layar.** Panel keterangan menyebut terus terang bahwa
   pengelolaan tarif ada di Master Data, beserta tautannya.

**Penyaring tanggal dan periode sengaja tidak dirender.** Standar fitur master data mewajibkan
ketiganya tetap tampil walaupun backend belum memprosesnya — tetapi standar itu mengikat fitur
**master data**, dan layar ini bukan fitur master data: ia layar baca milik modul Laboratorium.
`LabTariffQuery` hanya menerima `procedureId`, `search`, `pageNumber`, dan `pageSize`.
Menampilkan penyaring tanggal yang tidak diproses pada layar yang tidak dapat diubah isinya
hanya menyesatkan pembacanya. Alasannya ditulis sebagai komentar pada berkas konstanta.

**Gerbang keputusan base component.**

```text
UI GATE: 8 elemen — REUSE 7, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `.../hero.jsx` | `REUSE` | Dengan satu aksi muat ulang |
| Penyaring dan pencarian | `DataFilter`, `FilterSelect` | `.../data-filter.jsx` | `REUSE` | Dipakai pada layar tarif dan pemilih katalog |
| Penyaring jenis pemeriksaan | `ResourceFilterSelect` | `.../resource-filter-select.jsx` | `REUSE` | Membaca registry `procedures` |
| Tabel tarif, katalog, dan pilihan | `DataTable` | `.../data-table.jsx` | `REUSE` | Tiga pemakaian; `sortLatestFirst={false}` agar urutan backend dipertahankan |
| Penanda cakupan penjamin | `StatusBadge` | `.../status-badge.jsx` | `REUSE` | Tiga keadaan: tercakup, tidak tercakup, belum dipilih |
| Tombol pilih dan batalkan | `BaseButton` | `.../base-button.jsx` | `REUSE` | Tidak ada `<button>` mentah |
| Peringatan dan galat | `InformationAlert` | `.../information-alert.jsx` | `REUSE` | Termasuk peringatan pemeriksaan tanpa tarif |
| Panel pilihan beserta totalnya | `DataTable` + `InformationAlert` + `BaseButton` | berkas base yang sama | `COMPOSE` | Dirangkai menjadi `LabCatalogPicker` di lapis komponen fitur |

**Keputusan untuk satu-satunya baris yang bukan `REUSE`:**

> **Keputusan: panel pemeriksaan terpilih beserta totalnya**
>
> - **A. Rangkai `DataTable`, `InformationAlert`, dan `BaseButton` menjadi satu komponen fitur
>   modul — Rekomendasi.** Tidak ada base component yang berubah, dan komponennya berdiri
>   sebagai satu kesatuan yang dapat dipakai ulang layar pemesanan tanpa menyalin apa pun.
>   Inilah bentuk yang memang diminta roadmap.
> - **B. Pakai `SummaryGrid` untuk menampilkan totalnya.** Lebih ringkas, tetapi `SummaryGrid`
>   dirancang untuk angka rekap yang berdiri sendiri, bukan untuk total yang harus dibaca
>   bersama daftar barisnya.
> - **C. Tulis panel pilihan langsung di layar pemesanan pada `FE-LAB-06`.** Menghemat satu
>   berkas sekarang, tetapi memindahkan pekerjaan yang memang dijadwalkan task ini, dan
>   membuat perhitungan biaya tidak dapat dipakai ulang layar lain.
>
> Opsi **A** yang dijalankan.

**Komponen pemilih katalog belum dipasang di layar mana pun, dan itu disengaja.** Roadmap
menempatkannya sebagai keluaran `FE-LAB-04` yang "dipakai ulang oleh layar pesanan", dan layar
pesanan adalah cakupan `FE-LAB-06`. Memasangnya sekarang pada layar tarif akan menambahkan
permukaan produk yang tidak diminta siapa pun. Perhitungannya karena itu dibuktikan lewat uji
unit, bukan lewat pemasangan yang dipaksakan.

**Token desain.** Kedua berkas style baru tidak memuat satu pun nilai warna, radius, bayangan,
atau jarak sebagai literal. Tidak ada `!important` dan tidak ada inline style.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan "Mengambil daftar tarif laboratorium..." atau "Mengambil katalog pemeriksaan..."; tombol muat ulang dan tombol pilih terkunci selama proses |
| Kosong | "Data tarif laboratorium tidak ditemukan." disertai ajakan mengubah pencarian atau memeriksanya di Master Data. Pada panel pilihan: "Belum ada pemeriksaan yang dipilih." |
| Gagal | Pesan dari server ditampilkan apa adanya di kotak merah |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" lewat `AccessDeniedGate` |
| Data tidak lengkap | Pemeriksaan tanpa tarif dan pemeriksaan tanpa disiplin **tetap tampil**, masing-masing dengan keterangannya sendiri, bukan disembunyikan |
| Kirim ganda | Tombol pilih pada baris yang sudah terpilih nonaktif, sehingga satu pemeriksaan tidak dapat masuk dua kali |
| Permintaan usang | Permintaan katalog lama dibatalkan ketika penyaring berubah, sehingga jawaban lama tidak menimpa jawaban baru |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Catalog

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-catalog/tariffs` | Tabel layar Tarif Laboratorium | `LabCatalog : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-catalog/examinations` | Tabel katalog pada komponen pemilih | `LabCatalog : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-catalog/examinations/{procedureId}/price` | Harga berlaku dan keterangan cakupan penjamin, diambil saat sebuah pemeriksaan dipilih | `LabCatalog : Read` |

Ketiganya endpoint **baca**. Grup ini memang tidak punya `POST`, `PUT`, maupun `DELETE`, dan
`AC-48` justru dibuktikan lewat ketiadaan itu.

#### Health Services / Master Data / Procedure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/procedures/options` | Penyaring jenis pemeriksaan pada layar tarif, lewat registry select bersama | Mengikuti registry yang sudah ada |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Kode keluar `0` |
| `npx eslint` pada seluruh berkas baru, termasuk peringatan | **0 error, 0 peringatan** | `PASS` | Tiga peringatan yang sempat muncul diperbaiki lebih dulu, bukan dibiarkan: hasil hook didestrukturisasi supaya dependency `useMemo` stabil, dan satu dependency `useMemo` disamakan dengan yang disimpulkan React Compiler |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 463 uji, 463 lulus, 0 gagal | `PASS` | Seluruh suite dijalankan, termasuk enam uji baru |
| Uji `S1` — disiplin yang belum digolongkan | Lulus | `PASS` | Tampil "Belum digolongkan", bukan tanda hubung kosong |
| Uji `S2` — harga kontrak menang atas harga rumah sakit | Lulus | `PASS` | Tarif yang belum diatur tetap `null`, tidak berubah menjadi nol |
| Uji `S3` (`FR-09.3`) — tiga keadaan cakupan penjamin | Lulus | `PASS` | Tercakup, tidak tercakup, dan penjamin belum dipilih dibedakan; ketiadaan kontrak **tidak** dibaca sebagai gratis |
| Uji `S4` — jawaban endpoint harga menang atas baris katalog | Lulus | `PASS` | Harga rumah sakit, harga kontrak, subtotal, dan catatannya diambil dari jawaban harga |
| Uji `S5` (`AC-43`) — tiga pemeriksaan menghasilkan subtotal dan total | Lulus | `PASS` | Ketiganya bersubtotal, totalnya `110.000`, dan tidak ada satu pun perintah tulis yang dijalankan |
| Uji `S6` — pemeriksaan tanpa tarif tidak dihitung nol | Lulus | `PASS` | Dihitung terpisah sebagai `unpricedCount`, dan layar memperingatkan bahwa total yang tampil lebih kecil daripada biaya sebenarnya |
| `npm run build` | Selesai, termasuk `postbuild` standalone | `PASS` | Kode keluar `0`. Route `/health-services/laboratory-management/lab-tariffs` terbit |
| Tinjauan: tidak ada tombol tambah, ubah, atau hapus pada menu tarif | Terpenuhi | `PASS` | Definisi kolom tabel tarif tidak memuat kolom aksi; berkas service dan slice tidak memuat satu pun fungsi maupun thunk tulis |
| Tinjauan: konstanta yang sudah ada tidak diduplikasi | Terpenuhi | `PASS` | `ACTIVE_STATUS_OPTIONS`, `PAGE_SIZE_OPTIONS`, dan `INSURANCE_TARIFF_ROUTE_BASE` diimpor dari berkas aslinya; `formatCurrencyIDR` dipakai apa adanya; pilihan jenis pemeriksaan dari registry select bersama |
| Grep anti-regresi warna literal, `!important`, inline style statis | Tidak ada temuan | `PASS` | Seluruh nilai style memakai token `var(...)` |
| Grep anti-regresi tombol non-base dan tabel mentah | Tidak ada temuan | `PASS` | Seluruh tombol `BaseButton`; seluruh tabel `DataTable` |

**Uji manual: `NOT FEASIBLE`.**

1. Kedua layar sepenuhnya bergantung pada data katalog dan tarif yang sebenarnya. Pada
   lingkungan sesi ini tidak ada backend Laboratorium yang berjalan dan tidak ada sesi login
   yang sah, sehingga tabelnya hanya akan menampilkan layar akses ditolak.
2. Pembedaan tercakup dan tidak tercakup menuntut **data kontrak penjamin yang nyata** pada
   basis data — bukan sekadar aplikasi yang menyala.

Penggantinya bukan asumsi: keenam uji unit membuktikan perhitungan yang menentukan — termasuk
`AC-43` dan pembedaan tiga keadaan cakupan — dan keluaran build membuktikan layarnya terbit.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `npm run test:e2e` | Tidak diminta task, dan lingkungan tidak menyediakan backend maupun data tarif dan kontrak penjamin |
| `npm run test:uat` | Hanya dijalankan bila diminta secara eksplisit |
| `npm run dev` | `AGENTS.md` melarang menjalankan development server tanpa kebutuhan konkret |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-43` — memilih tiga pemeriksaan menampilkan harga satuan, subtotal, dan total, **tanpa** baris tagihan terbentuk | **Terpenuhi** | Uji `S5`. Seluruh perintah yang dijalankan layar dan komponen ini adalah perintah baca; tidak ada satu pun fungsi, thunk, maupun tombol yang menyimpan |
| `AC-48` — tidak ada endpoint tulis pada grup Lab Catalog, dan upaya mengubah tarif ditolak | **Terpenuhi di sisi layar** | Layar tidak menyediakan satu pun jalan mencoba: tanpa kolom aksi, tanpa tombol ubah, dan tanpa fungsi tulis. Pembuktian penolakan `403` `VAL-50` adalah cakupan `BE-LAB-07`, dan sudah terbukti di sana |

**Definition of Done.**

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Menu tarif baca saja | **Terpenuhi** | Route `/health-services/laboratory-management/lab-tariffs` terbit; tidak ada kolom aksi maupun tombol ubah; panel keterangan menyebut kepemilikan Master Data beserta tautannya |
| Komponen pemilih katalog dapat dipakai ulang | **Terpenuhi** | `LabCatalogPicker` berdiri di lapis komponen fitur dengan props `discipline`, `insuranceProviderId`, `patientClassId`, `lockDiscipline`, dan `onSelectionChange` — cukup untuk dipasang layar pemesanan `FE-LAB-06` tanpa perubahan |
| Konstanta yang sudah ada tidak diduplikasi | **Terpenuhi** | Baris tinjauan pada bagian 6, dan tabel pemakaian ulang pada bagian 1 |

Tidak ada butir DoD yang belum terpenuhi.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` yang tersisa. Tiga peringatan lint yang sempat muncul diperbaiki, sehingga berkas baru task ini bersih dari error maupun peringatan |
| Masalah yang diketahui — 1 | **Komponen pemilih katalog belum terpasang di layar mana pun.** Ini disengaja dan sesuai roadmap: konsumennya adalah layar pemesanan pada `FE-LAB-06`. Sampai task itu dikerjakan, komponennya hanya terbukti lewat uji unit, bukan lewat pemakaian nyata |
| Masalah yang diketahui — 2 | **Cakupan penyaring disiplin bergantung pada pengisian data.** `BE-LAB-07` sudah mencatatnya: penggolongan disiplin katalog masih menunggu keputusan pihak klinis. Selama belum diisi, memilih disiplin tertentu akan menyembunyikan pemeriksaan yang sebenarnya milik disiplin itu tetapi belum digolongkan. Layar sudah menampilkan keadaan "Belum digolongkan" apa adanya, tetapi pengisian datanya bukan pekerjaan frontend |
| Masalah yang diketahui — 3 | **Status kontrak usang**, sama seperti yang sudah dicatat `FE-LAB-02` dan `FE-LAB-03`. Ketiga endpoint grup Lab Catalog sendiri sudah tertulis `Tersedia` pada `contracts/api-contract.md`, sehingga grup ini **tidak** terdampak — catatan ini hanya untuk melengkapi gambaran |
| Dependency backend | `NONE` yang menahan. Ketiga endpoint sudah ada dan diverifikasi langsung pada source backend `2dfc4f2` |
| Perubahan sampingan | `NONE`. Layar `master-data/insurance-tariffs/` tidak disentuh sama sekali — ia hanya ditautkan |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah tabel ini |
| Langkah berikutnya | Gelombang `MVP-0` **selesai seluruhnya** — `FE-LAB-01` sampai `FE-LAB-04` sudah dikerjakan. Berikutnya gelombang `MVP-1`: `FE-LAB-05` layar pendaftaran pasien laboratorium. Perlu diperiksa lebih dulu apakah dependency-nya sudah siap, karena ketiga endpoint `Lab Patient Registration` masih berstatus `Rencana (belum tersedia)` pada kontrak, dan `BE-LAB-08` adalah pasangannya |

```text
 M src/lib/state/store.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/laboratory-management/lab-tariffs/
?? src/components/features/health-services/laboratory-management/lab-catalog-picker/
?? src/components/view/health-services/laboratory-management/lab-tariffs/
?? src/lib/constants/health-services/laboratory-management/lab-catalog-constants.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-catalog-picker.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-tariff-list.jsx
?? src/lib/services/health-services/laboratory-management/lab-catalog.service.js
?? src/lib/state/slice/health-services/laboratory-management/lab-catalog-slice.jsx
?? src/style/health-services/laboratory-management/lab-catalog-picker.module.css
?? src/style/health-services/laboratory-management/lab-tariffs/
?? tests/unit/lab-catalog-picker-utils.test.mjs
```

Blok di atas hanya memuat perubahan `FE-LAB-04`. Perubahan `FE-LAB-02` dan `FE-LAB-03` yang
belum di-commit masih berdiri berdampingan pada working tree yang sama.

Tidak ada `git add`, commit, push, merge, rebase, maupun perpindahan branch yang dilakukan pada
kedua repository.
