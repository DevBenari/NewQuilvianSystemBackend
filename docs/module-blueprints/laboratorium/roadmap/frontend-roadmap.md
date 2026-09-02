# Roadmap Delivery Frontend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `1` |
| Status | `DRAFT` |
| Bentuk blueprint | `SINGLE` |
| Ditulis oleh | `plan-module-delivery` |
| Tanggal | 2026-09-02 |
| Manifest | `blueprint-manifest.md` revision `23` |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Contract version | `LAB-API-v1` r3 `approved`, dikunci 2026-09-02 |
| Masukan | Decisions rev `21`; capability map rev `2`; `03-frontend-architecture.md` rev 3 |
| Slice in scope | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |

> **Titik berangkat: nol.** `CAP-21` membuktikan pada `688daff90` tidak ada satu berkas pun
> untuk Laboratorium di frontend. Pencarian `laboratory-management`, `labOrder`, `labSpecimen`,
> dan `lab-order` pada `src` tidak menghasilkan apa pun. Porsi pekerjaan frontend Rilis 1 adalah
> **seratus persen**.

---

## 1. Kenapa Frontend Boleh Berjalan Paralel

`plan-module-delivery` langkah 2 hanya mengizinkan kerja backend dan frontend berjalan
bersamaan untuk kontrak yang sudah `approved`, versioned, dan terkunci. Syarat itu terpenuhi
sejak 2026-09-02. Karena itu setiap task di bawah dipasangkan ke gelombang backendnya, bukan
ditumpuk pada satu gelombang `MVP-4` seperti rencana `04-prd-to-mvp.md` bagian 14.

**Yang tetap perlu diperhatikan.** Task frontend boleh **dibangun** paralel, tetapi hanya dapat
**diuji ujung-ke-ujung** setelah endpoint backend pasangannya tersedia. Kolom Dependency pada
setiap task menyebut pasangannya.

**Gerbang `LAB-OPEN-018` juga menyentuh frontend.** Rules root runtime kehilangan 10 dari 11
berkas aturan frontend — termasuk `base-component-catalog.md`, `design-tokens.md`,
`master-data-feature-standard.md`, dan `page-composition-patterns.md`. Selama belum diperbarui,
`build-module-frontend` kehilangan pijakan pola komponen dan token desain.

---

## 2. Aturan yang Mengikat Seluruh Task

### 2.1 Tujuh lapis wajib

Seluruh berkas berada di tujuh lapis berikut, mengikuti `pharmacy-management@688daff90` sebagai
acuan (`LAB-DEC-010`, `LAB-FE-001`):

| Lapis | Lokasi |
|---|---|
| Route halaman | `src/app/health-services/laboratory-management/<menu>/page.jsx` |
| Komponen fitur | `src/components/features/health-services/laboratory-management/<fitur>/` |
| Komponen tampilan | `src/components/view/health-services/laboratory-management/<menu>/` |
| Konstanta | `src/lib/constants/health-services/laboratory-management/` |
| Hook | `src/lib/hooks/health-services/laboratory-management/` |
| API service | `src/lib/services/health-services/laboratory-management/` |
| Style | `src/style/health-services/laboratory-management/<menu>/` |

Membuat pola penamaan route baru, atau menaruh berkas di luar tujuh lapis itu, **tidak boleh**.

### 2.2 Layar data induk tidak berada di folder Laboratorium

`LAB-FE-014` mengikat: seluruh menu data induk berada di `health-services/master-data/`,
mengikuti konvensi frontend yang sudah berjalan. **`LAB-DEC-034` tidak berlaku di frontend.**

| Menu data induk | Letak |
|---|---|
| Batas nilai pemeriksaan | `…/health-services/master-data/lab-value-bounds/` |
| Pilihan hasil terbatas | Menyatu dengan layar batas nilai |
| Alasan penolakan sampel | `…/health-services/master-data/lab-rejection-reasons/` |
| Jenis pemeriksaan | `…/health-services/master-data/procedure/` — **dipakai ulang** |
| Tarif dan cakupan penjamin | `…/health-services/master-data/insurance-tariffs/` — **dipakai ulang** |

Yang tetap di folder `laboratory-management` hanya layar **operasional**: pesanan, wadah dan
pemeriksaan, daftar kerja, dan monitoring per disiplin.

### 2.3 Enam invariant keselamatan yang tidak boleh diserahkan pada selera

| ID | Yang wajib | Kenapa |
|---|---|---|
| `LAB-FE-006` | Cito selalu di atas biasa pada daftar kerja | Urutan menentukan pekerjaan mana dikerjakan lebih dulu |
| `LAB-FE-009` | Seluruh pemeriksaan yang ditopang satu wadah terlihat sebelum tombol tolak | Petugas perlu tahu apa saja yang akan gugur |
| `LAB-FE-010` | Peringatan bahwa menolak wadah menggugurkan seluruh pemeriksaannya | Penolakan tidak dapat dibatalkan |
| `LAB-FE-011` | Batas kritis tampil sebagai **pengajuan**, tanpa jalur simpan langsung | Batas kritis menentukan kapan pasien dinyatakan dalam bahaya |
| `LAB-FE-012` | Penanda terkunci pada kolom kesalahan internal dan kolom wajib catatan **terlihat** | Gagal saat disimpan bukan pengganti kolom yang terlihat terkunci |
| `LAB-FE-013` | Bentuk isian batas nilai mengikuti bentuk hasilnya | Isian angka dan isian pilihan tidak sama |

Bentuk visualnya `DEV_DISCRETION`; **keberadaannya tidak**.

### 2.4 Yang sengaja dibiarkan `DEV_DISCRETION`

Nama menu yang dibaca pengguna, susunan kolom tabel, pilihan modal atau halaman terpisah, serta
warna penanda cito dan penanda kritis — dengan satu syarat: warnanya dapat dibedakan pengguna
dengan gangguan penglihatan warna. Roadmap **tidak** mengubah keempatnya menjadi keputusan
produk.

---

## 3. Task Gelombang `MVP-0`

### `FE-LAB-01` — Kerangka modul dan kontrak penanganan state

| Butir | Isi |
|---|---|
| **Outcome** | Tujuh lapis folder modul `laboratory-management` berdiri, beserta pola penanganan state yang dipakai seluruh layar berikutnya |
| **Requirement/decision** | `LAB-DEC-010`, `LAB-FE-001`, `LAB-FE-002` |
| **Kontrak** | `03-frontend-architecture.md` bagian 2 dan 4 |
| **Reuse** | `CAP-22` pola tujuh lapis `pharmacy-management`, `CAP-23` `axiosInstance` dan potongan Redux |
| **Cakupan** | Struktur folder, `axiosInstance` terpasang, potongan Redux dasar, dan penanganan state muat, kosong, gagal, coba lagi, serta data basi |
| **Dependency** | — |
| **Acceptance criteria** | Ditelusuri lewat tinjauan struktur, bukan uji fungsional |
| **Verifikasi** | Tinjauan: seluruh berkas berada di tujuh lapis; tidak ada pola route baru; tidak ada duplikasi `procedure-constants.jsx`, `insurance-tariff-constants.jsx`, maupun `tariff-category-constants.jsx` yang sudah ada di `688daff90` |
| **Risiko/pemilik** | Rendah, tetapi menentukan. Kekeliruan struktur di sini menular ke delapan task berikutnya. Pemilik: Frontend |
| **DoD** | Tujuh lapis ada, satu halaman contoh dapat dibuka, keempat state tertangani, tidak ada konstanta yang diduplikasi |

### `FE-LAB-02` — Layar batas nilai dan pengajuan batas kritis

| Butir | Isi |
|---|---|
| **Outcome** | Kepala instalasi mengelola batas nilai, dan perubahan batas kritis hanya dapat ditempuh lewat jalur pengajuan |
| **Requirement/decision** | `FR-07.4`, `FR-03.1` .. `FR-03.5`, `LAB-FE-011`, `LAB-FE-013` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Value Bound dan Lab Critical Bound Approval |
| **Reuse** | Pola layar data induk `master-data` yang sudah ada; `master-data/procedure/` sebagai sumber pilihan jenis pemeriksaan |
| **Cakupan** | Layar di `…/master-data/lab-value-bounds/`: daftar, detail beserta pilihannya, formulir buat dan ubah, tombol nonaktifkan, layar riwayat, dan **jalur pengajuan terpisah** untuk batas kritis |
| **Dependency** | `FE-LAB-01`; endpoint dari `BE-LAB-04` dan `BE-LAB-05` |
| **Acceptance criteria** | `AC-24`, `AC-28`, `AC-33`, `AC-34` |
| **Verifikasi** | Uji komponen: isian berubah bentuk mengikuti bentuk hasil — angka meminta satuan, pilihan meminta daftar pilihan. Uji ujung-ke-ujung: **tidak ada satu pun tombol simpan langsung untuk batas kritis**; perubahan batas kritis hanya muncul sebagai pengajuan |
| **Risiko/pemilik** | **Tinggi.** `LAB-FE-011` adalah invariant keselamatan. Menyediakan jalur simpan langsung untuk batas kritis, walaupun backend menolaknya, tetap pelanggaran — pengguna tidak boleh dibiarkan mengira jalan itu ada. Pemilik: Frontend |
| **DoD** | Layar ada di `master-data/lab-value-bounds/`, bentuk isian mengikuti bentuk hasil, jalur pengajuan terpisah dan terlihat, riwayat dapat dibuka, tidak ada jalur simpan langsung untuk batas kritis |

### `FE-LAB-03` — Layar alasan penolakan sampel

| Butir | Isi |
|---|---|
| **Outcome** | Kepala instalasi mengelola alasan penolakan, dan kolom yang tidak boleh ia ubah terlihat terkunci sejak awal |
| **Requirement/decision** | `FR-07.5`, `FR-06.1`, `FR-06.2`, `LAB-FE-012` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Rejection Reason |
| **Reuse** | Pola layar data induk `master-data` yang sudah ada |
| **Cakupan** | Layar di `…/master-data/lab-rejection-reasons/`: daftar, formulir tambah dan ubah, pengurutan, tombol aktif/nonaktif, dan **penanda terkunci yang terlihat** pada kolom kesalahan internal serta kolom wajib catatan |
| **Dependency** | `FE-LAB-01`; endpoint dari `BE-LAB-06` |
| **Acceptance criteria** | `AC-26` |
| **Verifikasi** | Uji komponen: kolom kesalahan internal tampil nonaktif beserta keterangannya bagi kepala instalasi, dan aktif bagi administrator sistem. Uji ujung-ke-ujung: menonaktifkan alasan aktif terakhir memunculkan pesan `VAL-38` yang dapat dipahami petugas |
| **Risiko/pemilik** | Sedang. `LAB-FE-012` mensyaratkan kolom **terlihat terkunci**, bukan sekadar gagal saat disimpan — pengguna harus tahu sebelum mencoba. Pemilik: Frontend |
| **DoD** | Layar ada di `master-data/lab-rejection-reasons/`, penanda terkunci terlihat, seluruh pesan gagal tampil dalam bahasa yang dipahami petugas |

### `FE-LAB-04` — Tampilan tarif laboratorium dan pemilih katalog

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat daftar tarif pemeriksaan laboratorium tanpa satu pun tombol ubah, dan memilih pemeriksaan dari katalog yang tersaring per disiplin |
| **Requirement/decision** | `FR-09.1` .. `FR-09.4`, `LAB-DEC-033` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Catalog |
| **Reuse** | `…/master-data/insurance-tariffs/` **sudah ada dan dipakai ulang**; `insurance-tariff-constants.jsx` dan `tariff-category-constants.jsx` dipakai ulang, **tidak disalin** |
| **Cakupan** | Menu Tarif Laboratorium **baca saja**, ditambah komponen pemilih katalog yang menampilkan harga satuan, subtotal, total, dan penanda cakupan penjamin — dipakai ulang oleh layar pesanan |
| **Dependency** | `FE-LAB-01`; endpoint dari `BE-LAB-07` |
| **Acceptance criteria** | `AC-43`, `AC-48` |
| **Verifikasi** | Uji ujung-ke-ujung: memilih tiga pemeriksaan menampilkan harga satuan, subtotal, dan total, **tanpa** baris tagihan terbentuk. Tinjauan: tidak ada satu pun tombol tambah, ubah, atau hapus pada menu tarif |
| **Risiko/pemilik** | Rendah. Godaannya menambahkan tombol ubah "sekalian karena datanya sudah tampil" — `LAB-DEC-033` melarangnya. Pemilik: Frontend |
| **DoD** | Menu tarif baca saja, komponen pemilih katalog dapat dipakai ulang, konstanta yang sudah ada tidak diduplikasi |

---

## 4. Task Gelombang `MVP-1`

### `FE-LAB-05` — Layar pendaftaran pasien laboratorium

| Butir | Isi |
|---|---|
| **Outcome** | Petugas laboratorium mendaftarkan pasien datang langsung maupun rujukan luar tanpa berpindah aplikasi |
| **Requirement/decision** | `FR-08.1` .. `FR-08.5`, `LAB-DEC-032`, `LAB-DEC-035` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Patient Registration |
| **Reuse** | Pola formulir dan pencarian yang sudah dipakai modul Health Services lain |
| **Cakupan** | Layar pencarian pasien terdaftar, formulir pendaftaran datang langsung, dan formulir pendaftaran rujukan luar dengan **pemilihan** instansi dan dokter perujuk dari daftar |
| **Dependency** | `FE-LAB-01`; endpoint dari `BE-LAB-08`; data induk dari `BE-EXT-02` |
| **Acceptance criteria** | `AC-44`, `AC-46`, `AC-50` |
| **Verifikasi** | Uji ujung-ke-ujung: setelah pendaftaran berhasil, petugas langsung dapat membuat pesanan lab pada layar berikutnya memakai penunjuk kunjungan yang dikembalikan. Jalur gagal: **isian instansi perujuk tidak menerima teks bebas** — hanya pilihan dari daftar; penolakan Registrasi tampil apa adanya tanpa menyisakan data setengah jadi di layar |
| **Risiko/pemilik** | Sedang. Pencarian pasien wajib mendahului pendaftaran baru (`FR-08.1`), jika tidak akan lahir data pasien ganda. Pemilik: Frontend |
| **DoD** | Tiga layar ada, instansi dan dokter perujuk dipilih dari daftar, penolakan Registrasi diteruskan apa adanya, alur menyambung ke pembuatan pesanan |

### `FE-LAB-06` — Layar pesanan laboratorium dan penanda cito

| Butir | Isi |
|---|---|
| **Outcome** | Dokter dan petugas melihat pesanan beserta pemeriksaannya, dan dokter pemesan dapat menandai pemeriksaannya sebagai cito |
| **Requirement/decision** | `FR-07.1`, `FR-01.1` .. `FR-01.3`, `LAB-FE-008` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Order dan Lab Examination |
| **Reuse** | Komponen pemilih katalog dari `FE-LAB-04`; pola daftar dan detail modul Health Services lain |
| **Cakupan** | Layar daftar pesanan, layar detail beserta pemeriksaannya, formulir pemesanan, dan penanda cito serta duplo **pada tingkat pemeriksaan** |
| **Dependency** | `FE-LAB-04`, `FE-LAB-05`; endpoint dari `BE-LAB-10` |
| **Acceptance criteria** | `AC-18`, `AC-40`, `AC-43` |
| **Verifikasi** | Uji ujung-ke-ujung: penanda cito hanya dapat ditekan oleh dokter pemesan; bagi pengguna lain tombolnya **tersembunyi atau nonaktif, bukan gagal saat ditekan**. Penanda cito melekat pada baris pemeriksaan, bukan pada pesanan |
| **Risiko/pemilik** | Sedang. Menempatkan penanda cito di tingkat pesanan melanggar `LAB-DEC-026` dan menggagalkan `AC-40`. Pemilik: Frontend |
| **DoD** | Layar pesanan ada, penanda cito berada pada baris pemeriksaan, kontrol tanpa kewenangan tersembunyi atau nonaktif sejak awal, harga tampil saat memesan |

---

## 5. Task Gelombang `MVP-2`

### `FE-LAB-07` — Layar wadah dan pemeriksaan

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat seluruh pemeriksaan yang ditopang satu wadah sebelum memutuskan menolak, dan diperingatkan bahwa penolakan menggugurkan semuanya |
| **Requirement/decision** | `FR-07.2`, `FR-02.1` .. `FR-02.3`, `FR-02.5`, `LAB-FE-009`, `LAB-FE-010` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Specimen dan Lab Examination |
| **Reuse** | Pola daftar dan formulir yang sudah dipakai; alasan penolakan dibaca dari `GET /lab-specimens/rejection-reasons` yang sudah ada |
| **Cakupan** | Layar perencanaan wadah, layar daftar wadah beserta pemeriksaan yang ditopangnya, alur menyatakan layak, alur menolak beserta peringatannya, dan alur ambil ulang |
| **Dependency** | `FE-LAB-06`; endpoint dari `BE-LAB-12` |
| **Acceptance criteria** | `AC-35`, `AC-36`, `AC-38` |
| **Verifikasi** | Uji ujung-ke-ujung: membuka layar penolakan wadah berisi dua pemeriksaan menampilkan **kedua** pemeriksaan, dan peringatan muncul **sebelum** penolakan dikonfirmasi. Uji komponen: tidak ada jalur menolak satu pemeriksaan saja pada wadah berisi lebih dari satu |
| **Risiko/pemilik** | **Tinggi.** `LAB-FE-009` dan `LAB-FE-010` adalah dua invariant keselamatan sekaligus. Petugas yang menolak wadah tanpa tahu isinya menggugurkan pekerjaan yang tidak ia maksud. Pemilik: Frontend |
| **DoD** | Seluruh pemeriksaan terlihat sebelum tombol tolak, peringatan muncul sebelum konfirmasi, tidak ada jalur penolakan per pemeriksaan, alur ambil ulang meminta sebab |

---

## 6. Task Gelombang `MVP-3`

### `FE-LAB-08` — Layar daftar kerja dan pantau keterlambatan

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat pekerjaan yang belum selesai dengan cito selalu di urutan atas, dan kepala instalasi melihat pesanan cito yang melewati batas waktunya |
| **Requirement/decision** | `FR-07.3`, `FR-04.1` .. `FR-04.3`, `LAB-FE-006` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Worklist |
| **Reuse** | Pola tabel dan penyaring modul Health Services lain |
| **Cakupan** | Layar daftar kerja dan layar pantau keterlambatan cito, beserta penanda kelebihan waktunya |
| **Dependency** | `FE-LAB-07`; endpoint dari `BE-LAB-14` |
| **Acceptance criteria** | `AC-10`, `AC-17`, `AC-39` |
| **Verifikasi** | Uji ujung-ke-ujung: pesanan cito yang masuk belakangan tetap berada di urutan pertama. **Uji komponen: urutan cito tidak dapat dibatalkan oleh pengurutan kolom yang dipilih pengguna** |
| **Risiko/pemilik** | **Tinggi.** `LAB-FE-006` menyatakan urutan ini invariant keselamatan. Bila pengurutan kolom biasa dapat menggeser cito ke bawah, invariantnya batal walaupun backend sudah benar. Pemilik: Frontend |
| **DoD** | Dua layar ada, cito selalu di atas dalam keadaan apa pun, kelebihan waktu tampil dalam satuan yang dipahami petugas |

### `FE-LAB-09` — Tiga layar monitoring per disiplin

| Butir | Isi |
|---|---|
| **Outcome** | Tiga menu sejajar — Patologi Klinik, Patologi Anatomi, Mikrobiologi — masing-masing menampilkan pesanan disiplinnya sendiri |
| **Requirement/decision** | `FR-10.1`, `FR-10.2` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Monitoring |
| **Reuse** | Satu komponen tabel dan penyaring dipakai ketiga layar; hanya sumber datanya yang berbeda |
| **Cakupan** | Tiga route terpisah dengan penyaring identik: pasien, nomor rekam medis, nomor pesanan, periode, jenis kunjungan, unit atau ruangan, penjamin, status pesanan, status wadah, dan penanda cito |
| **Dependency** | `FE-LAB-08`; endpoint dari `BE-LAB-15` |
| **Acceptance criteria** | `AC-41` |
| **Verifikasi** | Uji ujung-ke-ujung: ketiga layar dibuka dengan data campuran, masing-masing hanya menampilkan pesanan berdisiplin sesuai jalurnya |
| **Risiko/pemilik** | Rendah. Tiga menu terpisah adalah keputusan sadar — menyatukannya menjadi satu layar berpenyaring disiplin memaksa petugas memilih disiplin setiap kali membuka layar. Pemilik: Frontend |
| **DoD** | Tiga route ada, komponen tabel dan penyaringnya dipakai bersama tanpa duplikasi, penyaringnya identik pada ketiganya |

---

## 7. Layar yang Sengaja Tidak Dibuat

Kelima layar berikut **tidak boleh** dibangun lebih dulu "sekalian", karena perilakunya belum
diputuskan:

| Layar | Penahan |
|---|---|
| Pengisian dan validasi hasil | Slice `S4` — `LAB-SIGN-001` |
| Daftar pantau nilai kritis dan formulir pelaporan | Slice `S5` — `LAB-SIGN-001` |
| Layar koreksi hasil | Slice `S6` — `LAB-SIGN-001` |
| Kotak pemberitahuan dokter | Slice `S8` — `LAB-COORD-001`; kepemilikannya di platform |
| Penyuntingan pesanan oleh dokter | Slice `S1b` — `LAB-AMD-001` |

---

## 8. Ringkasan Status Task

| Task | Gelombang | Slice | Pasangan backend | Status rencana |
|---|---|---|---|---|
| `FE-LAB-01` | `MVP-0` | — | — | Siap direncanakan |
| `FE-LAB-02` | `MVP-0` | `S3` | `BE-LAB-04`, `BE-LAB-05` | Siap direncanakan |
| `FE-LAB-03` | `MVP-0` | `S11` | `BE-LAB-06` | Siap direncanakan |
| `FE-LAB-04` | `MVP-0` | `S14` | `BE-LAB-07` | Siap direncanakan |
| `FE-LAB-05` | `MVP-1` | `S13a`, `S13b` | `BE-LAB-08` | Siap direncanakan |
| `FE-LAB-06` | `MVP-1` | `S1a` | `BE-LAB-10` | Siap direncanakan |
| `FE-LAB-07` | `MVP-2` | `S2` | `BE-LAB-12` | Siap direncanakan |
| `FE-LAB-08` | `MVP-3` | `S7` | `BE-LAB-14` | Siap direncanakan |
| `FE-LAB-09` | `MVP-3` | `S15` | `BE-LAB-15` | Siap direncanakan |

**Tidak ada task frontend yang `BLOCKED` oleh keputusan yang belum diambil.** Seluruhnya
tertahan hanya oleh ketersediaan endpoint pasangannya, dan oleh `LAB-OPEN-018` yang menyentuh
kelengkapan aturan frontend di runtime.

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-02 | Roadmap frontend pertama. 9 task disusun dan dipasangkan ke gelombang backendnya, bukan ditumpuk pada `MVP-4`, setelah kontrak dikunci mengizinkan kerja paralel | `DRAFT` |
