# Roadmap Delivery Frontend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `7` |
| Status | `DRAFT` |
| Bentuk blueprint | `SINGLE` |
| Ditulis oleh | `plan-module-delivery` |
| Tanggal | 2026-09-02 |
| Manifest | `blueprint-manifest.md` revision `24` |
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

**Gerbang `LAB-OPEN-018` sempat menyentuh frontend, dan sudah tidak menahan.** Saat roadmap
ini disusun, rules root runtime kehilangan 10 dari 11 berkas aturan frontend — termasuk
`base-component-catalog.md`, `design-tokens.md`, `master-data-feature-standard.md`, dan
`page-composition-patterns.md` — sehingga `build-module-frontend` kehilangan pijakan pola
komponen dan token desain.

> **Ditutup 2026-09-04.** Seluruh berkas aturan frontend yang dibutuhkan sudah tersedia dan
> terbaca di runtime saat `FE-LAB-01` dikerjakan. Bukti dan rinciannya ada pada
> [`task/report/frontend/FE-LAB-01.md`](../task/report/frontend/FE-LAB-01.md) bagian 1.

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

> **Status: `SELESAI` — 2026-09-04.** Seluruh butir DoD terpenuhi. Tujuh lapis modul
> `laboratory-management` berdiri di frontend, halaman contoh **Ringkasan Laboratorium**
> terbit sebagai route `/health-services/laboratory-management/overview`, dan ketujuh baris
> kontrak penanganan state `03-frontend-architecture.md` bagian 4 ditangani — bukan hanya
> empat yang disebut DoD. Tidak ada konstanta `master-data` yang diduplikasi. Laporan lengkap
> beserta buktinya:
> [`task/report/frontend/FE-LAB-01.md`](../task/report/frontend/FE-LAB-01.md).
>
> **Gerbang `LAB-OPEN-018` tidak lagi menahan.** Pada saat task ini dikerjakan, seluruh berkas
> aturan frontend di akar rules runtime sudah tersedia dan terbaca, sehingga pijakan pola
> komponen dan token desain tidak lagi hilang.

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

> **Status: `SELESAI` — 2026-09-04.** Enam layar berdiri di
> `health-services/master-data/lab-value-bounds/`: daftar, buat, detail, ubah, riwayat, dan
> pengajuan perubahan batas kritis. Invariant `LAB-FE-011` ditegakkan tiga lapis — ruas batas
> kritis tidak dirender pada formulir ubah, payload menyalinnya dari data yang berlaku, dan
> penanda kritis setiap pilihan ikut dikunci — dua di antaranya dijaga uji unit. `LAB-FE-013`
> juga terbukti: isian angka dan isian pilihan tidak pernah tampil bersamaan. Laporan lengkap
> beserta buktinya: [`task/report/frontend/FE-LAB-02.md`](../task/report/frontend/FE-LAB-02.md).
>
> **Satu butir belum terpenuhi penuh, dan penyebabnya di backend.** `AC-34` menuntut riwayat
> memuat **pelaku**, sedangkan `LabValueBoundHistoryResponse` hanya membawa penunjuk pengguna
> tanpa nama — dan penunjuk seperti itu tidak boleh tampil di layar. Riwayat karena itu
> menampilkan kolom, nilai lama, nilai baru, waktu, alasan, dan penanda persetujuan, tetapi
> belum pelakunya.
>
> **Dua catatan lain.** Status sebelas endpoint pada `contracts/api-contract.md` masih
> tertulis `Rencana (belum tersedia)` padahal seluruhnya sudah ada sejak `BE-LAB-04` dan
> `BE-LAB-05`; dan peran pemegang `LabCriticalBound : Approve` masih belum ditetapkan,
> sehingga jalur persetujuan belum dapat dijalankan siapa pun.
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

> **Status: `SELESAI` — 2026-09-04.** Tiga layar berdiri di
> `health-services/master-data/lab-rejection-reasons/`: daftar, tambah, dan ubah.
> Invariant `LAB-FE-012` ditegakkan empat lapis — kedua penanda sistem tidak pernah menjadi
> isian, tidak pernah ikut pada payload, tampil terkunci beserta nilainya dan keterangan
> siapa yang dapat mengubahnya, dan selisih ruas terkunci yang diumumkan backend terbaca
> sebagai peringatan. Empat dari sembilan uji unit menjaga aturan itu. Kelima baris `AC-26`
> terpenuhi di sisi layar. Laporan lengkap beserta buktinya:
> [`task/report/frontend/FE-LAB-03.md`](../task/report/frontend/FE-LAB-03.md).
>
> **Dua batas yang berasal dari luar task ini.** Grup endpoint ini tidak punya `GET /{id}`,
> sehingga fitur ini tidak punya halaman detail dan formulir ubah memuat barisnya dari
> daftar. Dan frontend tidak menerima daftar permission per pengguna, sehingga penyembunyian
> aksi Setel Penanda Sistem hanya dapat sedekat **peran**, bukan sedekat
> `LabRejectionReason : SystemFlag` yang sebenarnya.
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

> **Status: `SELESAI` — 2026-09-04.** Menu **Tarif Laboratorium** berdiri baca saja di
> `laboratory-management/lab-tariffs/`: tanpa kolom aksi, tanpa tombol tambah, ubah, maupun
> hapus, dan tanpa satu pun fungsi atau thunk tulis. Panel keterangannya menyebut terus
> terang bahwa pengelolaan tarif ada di Master Data, beserta tautannya. Komponen
> `LabCatalogPicker` berdiri di lapis komponen fitur beserta perhitungan harga satuan,
> subtotal, total, dan penanda cakupan penjaminnya. Laporan lengkap beserta buktinya:
> [`task/report/frontend/FE-LAB-04.md`](../task/report/frontend/FE-LAB-04.md).
>
> **Ketiga konstanta yang tidak boleh diduplikasi memang tidak diduplikasi.**
> `insurance-tariff-constants.jsx` **diimpor ulang**; `tariff-category-constants.jsx` tidak
> dibutuhkan layar ini; dan pilihan jenis pemeriksaan diambil dari registry select bersama,
> bukan dari salinan `procedure-constants.jsx`. Pemformat mata uang memakai
> `formatCurrencyIDR` yang sudah ada.
>
> **Satu hal yang sengaja belum terpasang.** Komponen pemilih katalog belum dipakai layar
> mana pun, karena konsumennya adalah layar pemesanan pada `FE-LAB-06`. Perhitungannya
> dibuktikan enam uji unit, bukan pemasangan yang dipaksakan.
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

> **Status: `BLOCKED` — 2026-09-04.** Dependency backendnya belum ada, dan buktinya diambil
> dari source `2dfc4f2`: tidak ada `LabPatientRegistrationController`, tidak ada route
> `lab-patient-registrations` di seluruh `Areas/`, tidak ada DTO `RegisterLabWalkInRequest`,
> dan tidak ada laporan `BE-LAB-08.md`. Roadmap backend sendiri menandai `BE-LAB-08` sebagai
> `Siap direncanakan`, tertahan endpoint pelaksana `INT-05` yang kepemilikannya ada pada
> pemegang `registration-management`.
>
> Task ini **tidak dikerjakan** dan tidak boleh dikerjakan dengan menebak payload. Ia
> menunggu `BE-LAB-08`.
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

> **Status: `SELESAI` — 2026-09-04.** Tiga layar berdiri di
> `laboratory-management/lab-orders/`: daftar, buat, dan detail. Penanda cito dan duplo
> melekat pada **baris pemeriksaan** — daftar pesanan tidak punya kolom kesegeraan, kartu
> pesanan tidak punya kontrolnya, dan kedua perintahnya menerima `examinationId`. `AC-40`
> dijaga uji unit. Laporan lengkap beserta buktinya:
> [`task/report/frontend/FE-LAB-06.md`](../task/report/frontend/FE-LAB-06.md).
>
> **Dependency `FE-LAB-05` diwaive pemilik modul, 2026-09-04.** Dasarnya: endpoint
> pasangannya sudah selesai, task ini tidak memakai ulang satu pun artefak `FE-LAB-05`, dan
> `AC-11` menyatakan pesanan lab memang dapat dibuat dari kunjungan yang sudah ada.
>
> **Satu butir DoD terpenuhi sebagian.** Kontrol tanpa kewenangan disembunyikan atau
> dinonaktifkan untuk tiga dari empat keadaan. Keadaan keempat — dokter lain yang bukan
> pemesan — belum dapat dicegah layar karena `LabOrderDetailResponse` tidak membawa
> `requestedByUserId`, padahal itulah yang dibandingkan backend saat menegakkan `VAL-03`.
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
| `FE-LAB-01` | `MVP-0` | — | — | **`SELESAI`** 2026-09-04 |
| `FE-LAB-02` | `MVP-0` | `S3` | `BE-LAB-04`, `BE-LAB-05` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-03` | `MVP-0` | `S11` | `BE-LAB-06` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-04` | `MVP-0` | `S14` | `BE-LAB-07` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-05` | `MVP-1` | `S13a`, `S13b` | `BE-LAB-08` | **`BLOCKED`** — `BE-LAB-08` belum ada |
| `FE-LAB-06` | `MVP-1` | `S1a` | `BE-LAB-10` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-07` | `MVP-2` | `S2` | `BE-LAB-12` | Siap direncanakan |
| `FE-LAB-08` | `MVP-3` | `S7` | `BE-LAB-14` | Siap direncanakan |
| `FE-LAB-09` | `MVP-3` | `S15` | `BE-LAB-15` | Siap direncanakan |

**Tidak ada task frontend yang tertahan oleh keputusan yang belum diambil.** Pernyataan itu
masih berlaku: satu-satunya task yang `BLOCKED` — `FE-LAB-05` — tertahan oleh **endpoint yang
belum ada**, bukan oleh keputusan yang belum diputuskan.

**Keadaan per 2026-09-04.**

| Keadaan | Task | Keterangan |
|---|---|---|
| Selesai | `FE-LAB-01`, `FE-LAB-02`, `FE-LAB-03`, `FE-LAB-04`, `FE-LAB-06` | Gelombang `MVP-0` selesai seluruhnya; `FE-LAB-06` dikerjakan lebih dulu setelah dependency `FE-LAB-05` diwaive pemilik modul |
| Terblokir | `FE-LAB-05` | `BE-LAB-08` belum ada sama sekali pada source backend. Penahannya endpoint pelaksana `INT-05` milik pemegang `registration-management` |
| Siap dikerjakan | `FE-LAB-07`, `FE-LAB-08`, `FE-LAB-09` | Endpoint pasangannya — `BE-LAB-12`, `BE-LAB-14`, `BE-LAB-15` — seluruhnya sudah selesai. Ketiganya berantai: `07` menopang `08`, dan `08` menopang `09` |

`LAB-OPEN-018` sudah tidak menahan sejak 2026-09-04; lihat catatannya pada bagian 1.

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-02 | Roadmap frontend pertama. 9 task disusun dan dipasangkan ke gelombang backendnya, bukan ditumpuk pada `MVP-4`, setelah kontrak dikunci mengizinkan kerja paralel | `DRAFT` |
| 2 | 2026-09-04 | `FE-LAB-01` selesai dikerjakan dan divalidasi. Status task dan tautan laporannya dicatat; gerbang `LAB-OPEN-018` dinyatakan tidak lagi menahan pekerjaan frontend karena berkas aturannya sudah tersedia di runtime | `DRAFT` |
| 3 | 2026-09-04 | `FE-LAB-02` selesai dikerjakan dan divalidasi. Enam layar batas nilai berdiri beserta jalur pengajuan batas kritis yang terpisah. Tiga temuan dicatat: `AC-34` belum memuat pelaku karena respons riwayat backend tanpa nama, status sebelas endpoint pada dokumen kontrak masih tertulis `Rencana` padahal sudah ada, dan peran pemegang `LabCriticalBound : Approve` masih belum ditetapkan | `DRAFT` |
| 4 | 2026-09-04 | `FE-LAB-03` selesai dikerjakan dan divalidasi. Tiga layar alasan penolakan berdiri, dan `LAB-FE-012` ditegakkan empat lapis. Dua batas dicatat: grup endpoint ini tidak punya `GET /{id}` sehingga tidak ada halaman detail, dan frontend belum menerima daftar permission sehingga penyembunyian aksi penanda sistem hanya sedekat peran | `DRAFT` |
| 5 | 2026-09-04 | `FE-LAB-04` selesai dikerjakan dan divalidasi. Menu tarif baca saja berdiri tanpa satu pun jalur ubah, dan komponen pemilih katalog berdiri siap dipakai ulang `FE-LAB-06`. Ketiga konstanta yang dilarang diduplikasi terbukti dipakai ulang, bukan disalin. **Gelombang `MVP-0` selesai seluruhnya** | `DRAFT` |
| 6 | 2026-09-04 | `FE-LAB-05` ditandai **`BLOCKED`** setelah diverifikasi terhadap source backend: `BE-LAB-08` belum ada sama sekali. Pemilik modul memutuskan mewaive dependency itu dan mendahulukan `FE-LAB-06`, yang kemudian **selesai** — penanda cito dan duplo melekat pada baris pemeriksaan, dan `AC-40` dijaga uji unit. Satu batas kontrak dibuka: respons pesanan tidak membawa `requestedByUserId`, sehingga `VAL-03` belum dapat ditegakkan penuh di layar | `DRAFT` |
| 7 | 2026-09-04 | Dua paragraf naratif yang sudah basi disesuaikan dengan tabel status: catatan `LAB-OPEN-018` pada bagian 1 ditandai sudah ditutup, dan kalimat di bawah tabel bagian 8 diganti ringkasan keadaan yang benar-benar berlaku — lima task selesai, satu terblokir, tiga siap dikerjakan. Tidak ada status task yang berubah pada revisi ini | `DRAFT` |
