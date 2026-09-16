# Roadmap Delivery Frontend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `21` |
| Status | `DRAFT` |
| Bentuk blueprint | `SINGLE` |
| Ditulis oleh | `plan-module-delivery` |
| Tanggal | 2026-09-02; **gelombang `MVP-5a` ditambahkan 2026-09-14** |
| Manifest | `blueprint-manifest.md` revision `26` |
| Backend SHA | `466a7127`, diverifikasi tidak berubah pada `9067fa73` (revision 1-7 ditulis pada `c87d9c0`) |
| Frontend SHA | `9cd4cd03f` (revision 1-7 ditulis pada `688daff90`) |
| Contract version | `LAB-API-v1` **r7** `approved` — r3 dikunci 2026-09-02, amandemen `r7` disetujui pemilik modul 2026-09-14 |
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

> **Status: `SELESAI` — 2026-09-07.** Keempat butir DoD terpenuhi. Tiga layar berdiri —
> pencarian pasien, pendaftaran datang langsung, dan pendaftaran rujukan luar — dan alurnya
> menyambung ke pembuatan pesanan dengan kunjungan yang **sudah terisi**. Laporan lengkap:
> [`task/report/frontend/FE-LAB-05.md`](../task/report/frontend/FE-LAB-05.md).
>
> **Satu penahan baru ditemukan dan ditutup pada sesi yang sama.** Formulir rujukan luar wajib
> menawarkan pemilihan perujuk dari daftar, tetapi `MstReferralInstitution` dan
> `MstReferralDoctor` **tidak punya satu pun endpoint** — `BE-EXT-02` memang tidak membuatnya.
> Dua endpoint bacanya dibangun atas instruksi pemilik modul, sehingga butir Verifikasi
> `BE-EXT-02` *"kedua data induk dapat dipilih dari daftar"* yang selama ini tidak pernah
> terpenuhi kini terpenuhi.
>
> **Yang belum dijalankan: verifikasi manual.** Delapan skenario tercatat pada laporan bagian 5.
> Penahannya dua dan keduanya di luar layar: data induk perujuk **masih kosong**, dan instance
> backend belum dijalankan kembali. Selama daftar perujuknya kosong, formulir rujukan luar
> tidak dapat dipakai walaupun layarnya sudah benar.
>
> **Tiga hal yang berlaku bagi siapa pun yang menyentuh layar ini kemudian.**
>
> 1. **Kunci idempotensi dibuat saat formulir dibuka**, bukan saat tombol ditekan. Memindahkan
>    pembuatannya ke penekanan tombol mematikan seluruh perlindungan `VAL-45` tanpa satu pun
>    uji ikut gagal.
> 2. **`isReplay` berarti berhasil**, bukan gagal.
> 3. **Tidak ada kotak isian nama perujuk**, dan ketiadaan itulah penegakan `AC-50`.

<details>
<summary>Catatan saat penahannya baru dicabut — 2026-09-07, sebelum task dikerjakan</summary>

> **Status: `SIAP DIKERJAKAN` — 2026-09-07.** Penahannya dicabut. `BE-LAB-08` **selesai**:
> `LabPatientRegistrationController` ada, route `lab-patient-registrations` terdaftar, ketiga
> DTO permintaan dan jawabannya ada, dan laporannya
> [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) tersedia. Payload tidak perlu ditebak —
> bentuknya terkunci pada `LAB-API-v1` r3 dan sudah terdokumentasi Swagger.
>
> **Tiga hal yang wajib diketahui sebelum layarnya dibuat.**
>
> 1. **Kunci idempotensi dibuat layar, dan dibuat saat formulir dibuka — bukan saat tombol
>    ditekan.** `idempotencyKey` wajib dikirim pada `POST /walk-in` maupun
>    `POST /external-referral`; permintaan tanpa kunci ditolak `422`. Bila kuncinya dibuat pada
>    saat penekanan tombol, penekanan kedua membawa kunci berbeda dan menghasilkan kunjungan
>    kedua — persis yang hendak dicegah. Satu kunci berlaku untuk satu percobaan pendaftaran;
>    buat kunci baru hanya ketika petugas memulai pendaftaran berikutnya.
> 2. **Jawaban membawa `isReplay`.** Bernilai benar berarti pendaftarannya sudah tercatat
>    sebelumnya dan kunjungan yang sama dikembalikan. Ini **keberhasilan**, bukan kesalahan —
>    layar meneruskan petugas ke pembuatan pesanan seperti biasa.
> 3. **Instansi dan dokter perujuk dikirim sebagai penunjuk.** Permintaannya memang tidak punya
>    ruas nama sama sekali, sehingga layar wajib menyediakan pemilihan dari daftar. Dokter
>    disaring menurut instansi yang dipilih; dokter yang tidak berpraktik pada instansi itu
>    ditolak `422`.

</details>

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

> **Status: `SELESAI` — 2026-09-07.** Keempat butir DoD terpenuhi; lint, uji, dan build
> seluruhnya lolos. Layar berdiri sebagai route bersarang `/lab-orders/[slug]/specimens`, dan
> sembilan belas uji unit menjaganya. Laporan lengkap:
> [`task/report/frontend/FE-LAB-07.md`](../task/report/frontend/FE-LAB-07.md).
>
> **Kedua invariant keselamatan ditegakkan sebagai data, bukan sebagai susunan JSX.**
> `resolveSpecimenActions` yang memutuskan ada tidaknya aksi tolak, dan ia diuji terpisah —
> selama isi wadah belum termuat, aksi tolak **tidak dibuat sama sekali**. Menaruh penjagaan itu
> di dalam JSX berarti ia dapat tergeser diam-diam oleh perapian tampilan berikutnya, tanpa satu
> pun uji ikut gagal.
>
> **Satu batas sengaja tidak dilewati.** Backend punya `POST /lab-examinations/{id}/cancel`,
> tetapi membatalkan **satu** pemeriksaan adalah keputusan klinis yang berbeda dari menolak
> wadah. Mencampurnya melanggar `VAL-13`, sehingga layar ini tidak punya satu pun aksi
> berlingkup pemeriksaan — dan ada uji yang menjaga ketiadaan itu.
>
> **Satu kerusakan di luar Laboratorium ditemukan dan diperbaiki pada sesi yang sama.** Merge
> `c71c02a07` yang masuk di tengah pengerjaan membawa **lima kelompok deklarasi kembar** pada
> `billing-management`, sehingga `npm run build` gagal — nol error menunjuk ke
> `laboratory-management`. Salah satunya lebih berat daripada gagal build: `addCase` kembar
> untuk action type yang sama membuat Redux Toolkit melempar saat *store* dibentuk, sehingga
> **seluruh aplikasi** tidak dapat dijalankan, bukan hanya layar Billing. Atas instruksi
> eksplisit pemilik repository, salinan keduanya dibuang — 70 baris dihapus, **nol**
> ditambahkan. Rinciannya pada laporan bagian 5.1; pemilik modul Billing tetap perlu meninjaunya.
>
> **Yang belum: verifikasi manual.** Delapan skenario tercatat pada laporan bagian 5, menunggu
> backend dijalankan beserta satu pesanan yang punya wadah pada beberapa status berbeda.

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

> **Status: `SELESAI` — 2026-09-07.** Ketiga butir DoD terpenuhi; lint, uji, dan build
> seluruhnya lolos. Dua layar berdiri: `/lab-worklists` dan `/lab-worklists/cito-overdue`,
> dijaga enam belas uji unit. Laporan lengkap:
> [`task/report/frontend/FE-LAB-08.md`](../task/report/frontend/FE-LAB-08.md).
>
> **Satu ancaman langsung terhadap `LAB-FE-006` ditemukan saat membaca komponen dasarnya.**
> `DataTable` mengurutkan ulang datanya sendiri secara bawaan — `sortLatestFirst` bernilai
> `true` bila tidak dimatikan — sehingga pemeriksaan **biasa** yang diminta belakangan akan
> melompati **cito** yang diminta lebih dulu. Invariantnya batal tanpa satu baris kode pun
> terlihat salah, dan backend tetap benar sepanjang waktu.
>
> **Penegakannya berlapis dua, dan lapis keduanya yang sebenarnya menjaga.** Lapis pertama
> mematikan pengurutan bawaan tabel; lapis kedua melewatkan seluruh baris ke `sortWorklistRows`
> yang menegakkan kembali urutan cito **sesudah** pengurutan pilihan petugas. Lapis pertama saja
> hanya menutup satu jalan yang kebetulan aktif secara bawaan.
>
> **Ujinya menelusuri seluruh pilihan pengurutan yang ditawarkan layar**, bukan satu contoh yang
> dipilih tangan — menambah pilihan baru tanpa melewatkannya lewat `enforceCitoFirst` akan
> membuat uji itu gagal.
>
> **Satu batas dinyatakan terbuka:** pengurutan berlaku pada halaman yang terbuka saja, karena
> `LabWorklistPagedQuery` tidak punya ruas pengurutan. Keterangannya ditampilkan pada layar,
> bukan disembunyikan.

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

> **Status: `SELESAI` — 2026-09-07.** Ketiga butir DoD terpenuhi; lint, uji, dan build
> seluruhnya lolos. Tiga route berdiri di bawah `/lab-monitoring`, dijaga empat belas uji unit.
> Laporan lengkap: [`task/report/frontend/FE-LAB-09.md`](../task/report/frontend/FE-LAB-09.md).
>
> **"Penyaringnya identik" dibuat menjadi sifat struktural, bukan janji.** Ketiga disiplin
> menunjuk **objek definisi penyaring yang sama**, bukan tiga salinan yang kebetulan seragam.
> Tiga salinan pasti bercabang cepat atau lambat — satu penyaring ditambahkan di satu layar dan
> terlupa di dua lainnya, tanpa satu pun uji gagal. Ujinya pun memeriksa **identitas objek**,
> bukan kesamaan isi, sehingga percabangan pertama langsung tertangkap.
>
> **Disiplin yang tidak dikenal tidak pernah jatuh ke disiplin mana pun.** Memilihkan disiplin
> bawaan akan menampilkan pesanan Patologi Klinik kepada petugas Mikrobiologi yang salah membuka
> tautan — daftarnya tampak wajar dan tidak ada yang menandainya keliru. Layar berhenti dengan
> pesan salah tautan, dan segmen jalurnya juga ditolak di service sebelum menjadi permintaan.
>
> **State disimpan per disiplin**, sehingga berpindah menu tidak mengosongkan penyaring yang
> baru saja disusun petugas di menu sebelumnya.

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

## 6b. Task Gelombang `MVP-5a` — Penerimaan Sampling/Specimen

Ditambahkan 2026-09-14. Menurunkan `EPIC-LAB-11` `FR-11.1`, `FR-11.2`, dan `FR-11.8` dari
`04-prd-to-mvp.md` revision 4 bagian 16, beserta `LAB-FE-009`, `LAB-FE-010`, dan `LAB-FE-011`.

> **`FR-11.9` dan `FR-11.10` tidak punya task di sini.** Keduanya berstatus `OPEN DECISION`,
> tertahan `LAB-COORD-006` dan `LAB-COORD-007`. Wilayahnya tetap digambar pada layar supaya
> tata letaknya tidak perlu dirombak, tetapi isinya belum dikunci — lihat `FE-LAB-11` bagian
> Cakupan.

### Grafik urutan dependency — `MVP-5a`

```text
[BE] BE-LAB-20 ─┐
                ├──> FE-LAB-10
[BE] BE-LAB-25 ─┘

[BE] BE-LAB-21 ─┐
[BE] BE-LAB-22 ─┤
                ├──> FE-LAB-11 ──> FE-LAB-12
[BE] BE-LAB-23 ─┤
[BE] BE-LAB-24 ─┘
```

Legenda:

- `[BE]` — cermin baca-saja dari `backend-roadmap.md`. Task itu **tidak** dimiliki roadmap ini
  dan tidak diberi gelombang di sini.
- Ketiga task frontend muncul tepat satu kali.

| Gelombang eksekusi | Task | Kenapa di sini |
|---:|---|---|
| 1 | `FE-LAB-10` | Menunggu `BE-LAB-20` dan `BE-LAB-25` |
| 2 | `FE-LAB-11` | Menunggu keempat task backend yang menyusun formulirnya |
| 3 | `FE-LAB-12` | Menampilkan hasil yang dibuat `FE-LAB-11` |

**Tidak ada siklus.** Jumlah pasangan prasyarat→task pada grafik sama dengan isi kolom
**Dependency** pada ketiga task di bawah.

> **Batas yang dilaporkan apa adanya.** Grafik ini mencakup `MVP-5a` saja. Kesembilan task
> frontend lama ditulis sebelum grafik urutan dependency menjadi kewajiban dan tidak memilikinya.
> Dicatat sebagai gap, bukan diturunkan ulang dari ingatan dokumen.

### `FE-LAB-10` — Pengelolaan jenis specimen dan daftar pantau `Lainnya`

| Butir | Isi |
|---|---|
| **Outcome** | Kepala instalasi mengelola daftar jenis specimen, melihat keterangan `Lainnya` yang sering muncul, lalu menaikkannya menjadi jenis tetap |
| **Requirement/decision** | `FR-11.1`, `FR-11.2`, `LAB-DEC-040`, BR-35 |
| **Kontrak** | `LAB-API-v1` `r7` grup Lab Specimen Type — `approved`, terkunci |
| **Reuse** | Pola layar data induk yang sudah ada pada `health-services/master-data/`, misalnya `lab-rejection-reasons` |
| **Cakupan** | Dua layar di `src/app/health-services/master-data/lab-specimen-types/`: daftar beserta formulir kelola, dan layar daftar pantau `Lainnya`. **Bukan** di folder laboratory-management |
| **Dependency** | `BE-LAB-20`, `BE-LAB-25` |
| **Acceptance criteria** | `AC-58`, `AC-60`, `AC-49` |
| **Verifikasi** | Uji komponen sesuai `rules/frontend/test-policy.md`; verifikasi manual: tujuh jenis tampil terurut; menonaktifkan `Lainnya` ditolak dengan pesan `VAL-63`; daftar pantau menampilkan "cairan kista" berjumlah tiga |
| **Risiko/pemilik** | Rendah. Layar data induk mengikuti pola yang sudah berulang. Pemilik: Laboratorium |
| **DoD** | Kedua layar dapat dicapai dari menu; berada di `master-data/`, bukan di folder laboratorium (`AC-49`); tombol kelola dijaga `LabSpecimenType : Create` dan `: Update`; keadaan kosong, gagal, dan muat ulang tertangani; tautan langsung ke layar kelola berfungsi tanpa membuka daftarnya lebih dulu |

**Butir DoD terakhir bukan formalitas.** `LAB-API-v1` `r6` lahir justru karena `FE-LAB-03`
memuat barisnya dari halaman daftar yang sedang terbuka, lalu diam-diam gagal pada tautan
langsung dan muat ulang. Pola yang sama dicegah di sini sejak awal.

### `FE-LAB-11` — Formulir Penerimaan Sampling/Specimen

| Butir | Isi |
|---|---|
| **Outcome** | Petugas menyelesaikan satu pasien rujukan luar dari identifikasi sampai penetapan kelayakan tanpa berpindah menu |
| **Requirement/decision** | `FR-11.8`, `LAB-DEC-045`, BR-40; `LAB-FE-009`, `LAB-FE-010`, `LAB-FE-011` |
| **Kontrak** | `LAB-API-v1` `r7` grup Lab Specimen, Lab Examination, Lab Catalog, Lab Patient Registration — `approved`, terkunci |
| **Reuse** | `use-lab-order-form`, `use-lab-catalog-picker`, dan `lab-specimen-workspace-view` yang sudah ada pada `9cd4cd03f`. **Layar lama tidak diubah** — komponennya dipakai ulang, bukan dipindahkan |
| **Cakupan** | Satu layar formulir memuat wilayah A sampai E sesuai `03-frontend-architecture.md` bagian 10.3. Wilayah metode pembayaran dan usulan perujuk **digambar sebagai tempat, tanpa isi yang dikunci** — keduanya menunggu `LAB-REQ-005` |
| **Dependency** | `BE-LAB-21`, `BE-LAB-22`, `BE-LAB-23`, `BE-LAB-24` |
| **Acceptance criteria** | `AC-75`, `AC-77`, dan `AC-76` sebagai regresi |
| **Verifikasi** | Uji komponen; verifikasi manual jalur berhasil `UAT-11.1` dan `UAT-11.2`; jalur gagal `UAT-11.3` dan `UAT-11.4`; **seluruh uji layar lab yang sudah ada dijalankan kembali dan lulus tanpa disentuh** |
| **Risiko/pemilik** | Sedang. Layar terbesar modul ini, dan dua wilayahnya belum punya isi. Pemilik: Laboratorium. Tata letak `DEV_DISCRETION` sesuai `LAB-FE-002` |
| **DoD** | Wilayah C dan D **terlihat berdampingan** sebelum kelayakan ditetapkan (`LAB-FE-010`); peringatan penguncian terlihat sebelum tombol kelayakan dapat ditekan; metode pembayaran **baca-saja tanpa kotak pilihan** dan menulis *belum dapat ditentukan* saat sumbernya tidak terjawab (`LAB-FE-011`); jumlah/Qty dapat diisi dan hasilnya tersimpan sebagai beberapa baris; penamaan menu membedakan jalur ini dari pesanan dokter (`LAB-FE-009`); `AC-76` terbukti — uji layar lama lulus tanpa perubahan |

> **Dua wilayah sengaja kosong, dan itu harus terlihat oleh petugas.** Wilayah metode
> pembayaran menampilkan *belum dapat ditentukan*, bukan dikosongkan begitu saja. Wilayah
> instansi perujuk tetap memakai daftar terkendali — bila perujuknya belum terdaftar,
> pendaftaran ditolak `VAL-43` seperti hari ini. **Jalan buntunya belum hilang**, dan layar ini
> tidak boleh berpura-pura sudah hilang.

**Satu hal yang perlu terlihat sejak formulir:** mengisi jumlah `3` menghasilkan **tiga baris**
pemeriksaan setelah tersimpan, bukan satu baris bertuliskan "3". Bila layar detail menampilkan
tiga baris sementara formulirnya menampilkan satu, petugas akan menyangka ada yang salah.

### `FE-LAB-12` — Daftar dan detail penerimaan

| Butir | Isi |
|---|---|
| **Outcome** | Petugas menelusuri penerimaan yang sudah dicatat, dan kepala instalasi membaca laporan penerimaan menurut waktu kedatangan sebenarnya |
| **Requirement/decision** | `FR-11.5`, `FR-11.8`, `LAB-DEC-042`, `LAB-DEC-045` |
| **Kontrak** | `LAB-API-v1` `r7` jalur baca grup Lab Specimen — `approved`, terkunci |
| **Reuse** | Pola daftar dan detail yang sudah ada pada `lab-orders` |
| **Cakupan** | Layar daftar penerimaan beserta penyaring rentang tanggal, dan layar detail satu penerimaan |
| **Dependency** | `FE-LAB-11` |
| **Acceptance criteria** | `AC-67`, `AC-75` |
| **Verifikasi** | Uji komponen; verifikasi manual: wadah yang diterima Senin 21.10 dan dicatat Selasa 08.05 muncul pada **hari Senin**; selisih kedua waktu terbaca |
| **Risiko/pemilik** | Rendah. Baca saja. Pemilik: Laboratorium |
| **DoD** | Daftar memakai **waktu penerimaan nyata**, bukan waktu sistem; selisih terhadap waktu sistem dapat dilihat kepala instalasi; layar detail menampilkan setiap baris pemeriksaan secara terpisah; tautan langsung dan muat ulang berfungsi |

**Butir DoD pertama adalah inti `LAB-DEC-042`.** Bila daftar ini keliru memakai `ReceivedAt`,
seluruh guna kolom baru itu hilang tanpa satu pun kesalahan yang terlihat — layarnya tetap
tampil benar, hanya tanggalnya yang salah.

## 6c. Task Gelombang `MVP-5b` — Kiosk dan Pemesanan per Disiplin

Ditambahkan 2026-09-15. Menurunkan `LAB-DEC-051`, `LAB-DEC-052`, `LAB-DEC-055`, dan
`LAB-DEC-056` di bawah `LAB-REQ-006`. Kontrak `LAB-API-v1` `r11` dan `LAB-VAL-v1` `r5`
`approved` 2026-09-15, sehingga kedua task boleh berjalan paralel dengan backendnya.

**Layar kiosk sudah ada di repo ini** — `src/app/kiosk/` beserta `registration/new-patient`,
`old-patient`, `patient-card`, dan `doctor-schedule`. Yang ditambahkan adalah pilihan
layanannya, bukan kiosknya.

### `FE-LAB-13` ✅ — Kiosk: pilihan layanan Laboratorium

> **Status: `SELESAI` — 2026-09-16.** Keempat butir DoD terpenuhi. Laporan lengkap beserta
> buktinya: [`task/report/frontend/FE-LAB-13.md`](../task/report/frontend/FE-LAB-13.md).
>
> **Tiga temuan dari source menentukan letak pilihannya, dan ketiganya tidak terlihat dari kartu
> task ini.** Pertama, kedua ruas sesi kiosk hanya dapat ditulis **sekali**, yaitu saat sesinya
> dibentuk — `BE-EXT-04b` sengaja tidak membangun jalur ubah. Kedua, pada alur Pasien Lama sesi
> dibentuk di langkah **pertama**, saat kartu dipindai, yakni lima langkah sebelum
> `Layanan & Dokter`; memasang pertanyaannya di langkah itu karena itu mustahil menulis apa pun.
> Ketiga, dan yang paling menentukan: pada alur Pasien Baru sesi langsung dikonsumsi kunjungan
> poliklinik — `PatientEncounterController.cs:686` menandainya `IsUsedForRegistration = true` —
> sedangkan panel `FE-LAB-14` menyaring justru yang **belum** terpakai. Sesi bertujuan
> Laboratorium dari alur itu **hilang pada detik yang sama ia dibuat**.
>
> **Akibatnya pada cakupan, dan itu keputusan pemilik modul, bukan penyempitan sepihak.** Tiga
> hal ditanyakan sebelum satu baris pun ditulis: alur mana yang dipasangi, di mana alurnya
> berhenti, dan apakah cabang poliklinik ikut menuliskan tujuan layanannya. Jawabannya
> **Pasien Lama saja**, **berhenti sesudah identitas terbaca**, dan **cabang poliklinik tidak
> menuliskan apa pun**. Alur Pasien Baru menyusul sebagai task tersendiri bila dikehendaki.
>
> **Satu temuan yang akan menjadi `400` bila tidak ketahuan lebih dulu:** `Program.cs` memanggil
> `AddControllers()` tanpa satu pun `JsonStringEnumConverter`, sehingga enum pada body JSON hanya
> terbaca sebagai angka. `targetService` karena itu dikirim sebagai `2`, bukan `"Laboratory"` —
> bentuk yang dipakai `FE-LAB-14` dan memang sah **di sana**, karena yang itu query string.
>
> **Butir DoD yang paling mudah dilanggar diam-diam dibuktikan terbalik, bukan diasumsikan:**
> muatan cabang poliklinik diperiksa kunci demi kunci dan terbukti **nol** `targetService`
> maupun `hasPhysicianRequest` — bukan berisi `null`, melainkan memang tidak ada ruasnya.
>
> **Satu perbedaan tak terhindarkan disebut apa adanya:** pasien alur Pasien Lama kini melihat
> satu layar tambahan di depan. Sesudah `Poliklinik` ditekan, nol perilaku berikutnya berubah.

| Butir | Isi |
|---|---|
| **Outcome** | Pasien memilih Laboratorium sendiri di kiosk, dan menyatakan apakah ia membawa permintaan dokter |
| **Requirement/decision** | `LAB-DEC-051`, `LAB-DEC-052`; wewenang `LAB-REQ-006` |
| **Kontrak** | Endpoint sesi kiosk milik `registration-management`, dibangun `BE-EXT-04` |
| **Cakupan** | Satu pilihan layanan pada alur kiosk yang sudah ada, dan satu pertanyaan jalur permintaan dokter. **Nol layar baru** |
| **Dependency** | `BE-EXT-04` |
| **Acceptance criteria** | `AC-93` |
| **Kewenangan UI** | Mengikuti pola layar kiosk yang sudah berjalan — huruf besar, sasaran sentuh lebar, dan langkah yang dapat dibatalkan. Pasien bukan petugas: tidak ada istilah teknis, dan **tidak ada** kata "disiplin" di layar mana pun |
| **Verifikasi** | Lint bersih; build produksi; **seluruh uji layar kiosk yang sudah ada tetap lulus tanpa disentuh**; alur lama yang tidak memilih Laboratorium berperilaku persis seperti sebelumnya |
| **Risiko/pemilik** | Sedang. Layar yang dipakai pasien tanpa pendamping. Pemilik layar: `registration-management` |
| **DoD** | Pilihan Laboratorium tersedia; jalur permintaan dokter tercatat pada sesi; **nol perilaku alur kiosk lama yang berubah**; uji lama lulus tanpa diubah |

### `FE-LAB-14` ✅ — Layar pendaftaran lab: sesi kiosk dan pemilih pemeriksaan

> **Status: `SELESAI` — 2026-09-16.** Keempat butir DoD terpenuhi. Laporan lengkap beserta
> buktinya: [`task/report/frontend/FE-LAB-14.md`](../task/report/frontend/FE-LAB-14.md).
>
> **Verifikasi layar menemukan dua cacat nyata, dan keduanya lolos dari lint, build, serta uji
> unit.** Pertama, menarik pasien dari kiosk **tidak terlihat apa-apa**: pasien terpilih dulu
> hanya dicari di dalam hasil pencarian, sedangkan pasien kiosk ditarik tanpa mengetik kata
> kunci apa pun — tombolnya terasa tidak berfungsi padahal pasiennya sudah terpilih. Kedua,
> rincian pemecahan **terhapus sendiri** sepersekian detik sesudah tampil, karena pemilih
> katalog memancarkan callback pada setiap render ulang — termasuk saat harga selesai dimuat —
> bukan hanya saat pilihan berubah. Keduanya diperbaiki dan diuji ulang.
>
> **Satu keputusan teknis menentukan apakah layar ini dapat dipakai sama sekali.** Backend
> menyediakan dua jalur baca sesi kiosk dengan isi identik; `GET /kiosk-scan-sessions/options`
> dijaga `KioskReadPolicy` yang **hanya** mengakui SuperAdmin, Administrator, dan akun kiosk —
> sehingga akan menjawab `403` untuk setiap petugas laboratorium, yakni pengguna yang justru
> dituju layar ini. Yang dipakai karena itu `admin/options`, dijaga izin aplikasi biasa
> `KioskScanSession : Read`. **Prasyarat konfigurasi:** izin itu perlu diberikan kepada peran
> petugas laboratorium; selama belum, panelnya menyebut nama izinnya apa adanya dan pendaftaran
> manual tetap berjalan.
>
> **Satu bagian verifikasi tidak selesai dan disebut apa adanya:** panel rincian **sesudah**
> simpan belum pernah dilihat pada layar sungguhan, karena pada harness bertopeng nilai kotak
> pilihan Kunjungan Pasien tidak bertahan sampai penyimpanan. Gejalanya **tidak berhasil
> dipisahkan** dari cara harness memalsukan jalur itu, sehingga **tidak** dilaporkan sebagai
> cacat produk yang terkonfirmasi. Logika yang seharusnya dibuktikan panel itu tetap teruji
> lewat uji unit.
>
> **Satu temuan di luar cakupan:** penurunan disiplin yang ditambahkan commit `4031fd3d7`
> **tidak pernah menghasilkan nilai** — ia membaca ruas yang tidak pernah ada. Akibatnya nol,
> karena `BE-LAB-29` sudah membuat backend menurunkannya dari katalog. Kodenya mati, bukan
> salah, dan task ini mencabutnya.

| Butir | Isi |
|---|---|
| **Outcome** | Petugas menarik pasien dari daftar sesi kiosk, memilih pemeriksaan sekali, dan melihat pesanan yang terbentuk beserta disiplinnya |
| **Requirement/decision** | `LAB-DEC-055`, `LAB-DEC-056` |
| **Kontrak** | `LAB-API-v1` `r11` `POST /lab-orders/by-examinations`; jalur baca sesi kiosk dari `BE-EXT-04` |
| **Cakupan** | Daftar sesi kiosk bertujuan Laboratorium pada layar pendaftaran yang sudah ada, pemilih pemeriksaan, dan pemberitahuan hasil pemecahan |
| **Dependency** | `BE-EXT-04`, `BE-LAB-27` |
| **Acceptance criteria** | `AC-86`, `AC-87` |
| **Kewenangan UI** | **Disiplin tidak ditanyakan sama sekali** (`LAB-DEC-048` butir 6). Sesudah simpan, layar **wajib** menyebutkan bahwa pilihan tadi menjadi lebih dari satu pesanan beserta disiplin masing-masing. Pemeriksaan yang belum digolongkan disebut apa adanya: tersimpan, tetapi tidak akan muncul di menu disiplin mana pun |
| **Verifikasi** | Lint bersih; build produksi; uji unit atas bentuk muatan yang dikirim; verifikasi manual: memilih Hemoglobin dan kultur darah menghasilkan dua nomor pesanan yang **terlihat jelas** di layar |
| **Risiko/pemilik** | Sedang. Satu tindakan menghasilkan lebih dari satu objek bisnis. Pemilik: Laboratorium |
| **DoD** | Daftar sesi kiosk tampil dan dapat ditarik; pemilih pemeriksaan mengirim satu permintaan; **hasil pemecahan diberitahukan, bukan dibiarkan ditemukan sendiri**; nol kotak pilihan disiplin |

**Kenapa pemberitahuan pemecahan masuk DoD, bukan sekadar saran.** Ini satu-satunya tempat pada
modul ini di mana satu tindakan petugas menghasilkan lebih dari satu objek bisnis. Tanpa
pemberitahuan, satu-satunya cara petugas mengetahuinya adalah menemukan dua baris di layar
lain — dan dugaan pertama yang wajar adalah ia tidak sengaja menekan simpan dua kali.

---

## 6d. Task Gelombang `MVP-5c` — Konfirmasi Pesanan dan Pembatalan Beralasan

**Ditambahkan 2026-09-15**, menurunkan `LAB-DEC-061` dan `LAB-DEC-063`. Kontrak `LAB-API-v1`
`r12` dan `LAB-VAL-v1` `r6` `approved` pada tanggal yang sama.

### `FE-LAB-15` ✅ — Kolom Konfirmasi dan pop-up konfirmasi

> **Status: `SELESAI` — 2026-09-16.** Keempat butir DoD terpenuhi dan **terbukti pada aplikasi
> yang benar-benar berjalan**, termasuk butir yang paling menentukan: **konfirmasi kedua tidak
> mungkin dilakukan dari layar** — dua tombol Konfirmasi, baris `Requested` aktif dan baris
> `Confirmed` nonaktif. Laporan:
> [`task/report/frontend/FE-LAB-15.md`](../task/report/frontend/FE-LAB-15.md).
>
> **Kewenangan UI ditegakkan kata demi kata:** kolomnya berbunyi `Belum Terkonfirmasi` sebelum
> konfirmasi, lalu `Dewi` · `16 Sep 2026, 09.30` sesudahnya — **nol label `Terkonfirmasi`
> tambahan**, diperiksa dari teks tabel sesudah teks bawaannya dibuang. **Nol kotak isian
> konfirmator** pada pop-up, diperiksa dari teks pop-upnya sendiri. Muatan yang dikirim tepat
> satu ruas.
>
> **Baris diperbarui dari jawaban server, bukan ditebak layar.** Kolomnya menampilkan nama dan
> waktu yang benar-benar tersimpan, dan tombolnya nonaktif karena statusnya memang sudah
> berpindah — bukan karena layar mengingat pernah menekannya.
>
> **Satu asersi uji sempat gagal, dan yang salah adalah ujinya**, bukan produknya: regexnya ikut
> mencocoki kata di dalam "Belum Terkonfirmasi" — teks yang memang seharusnya ada.
>
> **Satu bagian `AC-95` sempat tetap terbuka** — "tampil pada **ringkasan cetak**" milik
> `FE-LAB-17`, yang saat task ini selesai masih ⛔. **Ditutup pada hari yang sama**: `FE-LAB-17`
> ✅ selesai 2026-09-16, dan `AC-95` kini terpenuhi penuh.

<details>
<summary>Riwayat: keadaan sebelum task ini dikerjakan</summary>

> **Status: SIAP DIMULAI — penahan terangkat 2026-09-16, pada hari yang sama ia ditemukan.**
>
> Ketiga nilai yang wajib ditampilkan kolom Konfirmasi — `confirmedAt`, `confirmedByName`, dan
> `examinerDoctorName` — kini terbaca pada jalur daftar, dan **terbukti dari database** lewat
> [`BE-LAB-33.md`](../task/report/backend/BE-LAB-33.md). **Nol penahan tersisa.**
>
> **Riwayat penahannya dipertahankan di bawah**, bukan dihapus, karena urutan kejadiannya pantas
> dibaca ulang: celah kontrak ini **tidak** ditemukan oleh build, uji, maupun tinjauan kontrak —
> ketiganya hijau ketika celahnya masih ada. Ia ditemukan dengan membaca apa yang dibutuhkan
> layar konsumennya, saat `BE-LAB-31` hendak ditandai selesai.

<details>
<summary>Riwayat: penahan yang sudah ditutup</summary>

> **Status: ⛔ `TERTAHAN` — ditemukan 2026-09-16 saat `BE-LAB-31` selesai, sebelum satu baris pun
> ditulis.**
>
> **Penahannya konkret, bukan kehati-hatian umum.** Kewenangan UI di bawah mewajibkan kolom
> Konfirmasi menampilkan **nama konfirmator beserta tanggal dan waktu**, dan `AC-95` mewajibkan
> dokter pemeriksa **tampil pada daftar serta ringkasan cetak**. Ketiga nilai itu **tidak
> dikembalikan endpoint mana pun**: `LAB-API-v1` `r12` §7.1 hanya mendefinisikan badan permintaan
> dan tidak menambah satu pun ruas pada `LabOrderListResponse` maupun `LabOrderDetailResponse`.
>
> **Backendnya sendiri sudah siap** — `POST /lab-orders/{id}/confirm` berdiri dan terbukti,
> dan ketiga nilainya **sudah tersimpan** di database sejak `BE-LAB-30`. Yang hilang hanyalah
> jalan keluarnya.
>
> **Yang dibutuhkan:** `LAB-API-v1` `r13` menambahkan lima ruas respons — `confirmedAt`,
> `confirmedByUserId`, `confirmedByName`, `examinerDoctorId`, `examinerDoctorName`. Usulnya
> ditulis lengkap pada [`BE-LAB-31.md`](../task/report/backend/BE-LAB-31.md) bagian 7.2, beserta
> alasan kenapa kelimanya aman bagi pembaca lama. Sesudah disetujui, satu task backend kecil
> memasang ruasnya, lalu layar ini dapat dimulai.
>
> **Tombol Konfirmasinya sendiri tidak tertahan** — pemanggilan endpointnya sudah dapat dibangun
> hari ini. Yang tertahan adalah kolom yang menampilkan hasilnya.

**Ditutup 2026-09-16** oleh `LAB-API-v1` `r13` yang disetujui pemilik modul, lalu dilaksanakan
`BE-LAB-33` — dan disempurnakan `r14` beserta `BE-LAB-34`, karena `r13` menyebut DTO yang keliru.

</details>

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat mana yang belum terkonfirmasi, lalu mengonfirmasi lewat satu pop-up berisi ringkasan pasien dan pemilih dokter pemeriksa |
| **Requirement/decision** | `FR-11.12`; `LAB-DEC-061` |
| **Kontrak** | `LAB-API-v1` `r12` §7.1 `POST /lab-orders/{id}/confirm` |
| **Cakupan** | Satu kolom pada ketiga datatable pemeriksaan, satu pop-up, satu pemanggilan endpoint |
| **Dependency** | `BE-LAB-31` |
| **Acceptance criteria** | `AC-94`, `AC-95` |
| **Kewenangan UI** | Kolom Konfirmasi berisi `Belum Terkonfirmasi` sebelum konfirmasi, lalu **nama konfirmator beserta tanggal dan waktu** — **tanpa** label `Terkonfirmasi` tambahan pada kolom itu. Tombol Konfirmasi **nonaktif** sesudah berhasil sekali. **Nol kotak isian konfirmator**: namanya datang dari server, dan ruasnya memang tidak ada pada DTO permintaan |
| **Verifikasi** | Lint bersih; build produksi; uji unit atas bentuk muatan yang dikirim; verifikasi manual: konfirmasi kedua tidak mungkin dilakukan dari layar |
| **Risiko/pemilik** | Rendah. Menambah satu kolom dan satu pop-up. Pemilik: Laboratorium |
| **DoD** | Kolom menampilkan nama dan waktu sesudah konfirmasi; tombol nonaktif sesudahnya; pemilih dokter wajib terisi sebelum simpan; ketiga menu memakai komponen yang sama |

### `FE-LAB-16` ✅ — Pop-up pembatalan beralasan dan alert konfirmasi akhir

> **Status: `SELESAI` — 2026-09-16.** Ketiga butir DoD terpenuhi dan **terbukti pada aplikasi
> yang benar-benar berjalan**, termasuk butir yang paling menentukan: **menutup alert konfirmasi
> akhir tidak mengirim permintaan apa pun** — nol permintaan tercatat sesudah `Kembali` ditekan,
> dan barisnya tidak berubah. Laporan:
> [`task/report/frontend/FE-LAB-16.md`](../task/report/frontend/FE-LAB-16.md).
>
> **Dua tahap disimpan sebagai dua keadaan terpisah**, bukan satu keadaan dengan penanda di
> dalamnya — sehingga keduanya tidak dapat terbuka bersamaan dan menutup tahap 2 benar-benar
> berarti tidak ada yang dikirim.
>
> **Daftar status ditulis sebagai yang diizinkan, bukan yang dilarang**, supaya status baru yang
> kelak ditambahkan tidak otomatis menjadi dapat dibatalkan.
>
> **Satu selisih terhadap kewenangan UI, dilaporkan bukan didiamkan:** kewenangan menulis tiga
> status yang dilarang, sedangkan `VAL-75` mempersempit lebih jauh — `Accepted` dan `OnHold` juga
> ditolak backend. Yang diikuti kontraknya, karena menampilkan aksi di sana berarti memasang
> tombol yang **selalu gagal** `409`. Bila pemilik menghendaki aksinya tetap tampil, itu perubahan
> kewenangan UI tersendiri.
>
> **`FR-11.13` kini tertutup ujung ke ujung** — backend sejak `BE-LAB-32`, layar sejak task ini.

| Butir | Isi |
|---|---|
| **Outcome** | Petugas tidak dapat membatalkan pesanan tanpa menuliskan alasannya, dan tidak dapat membatalkan hanya karena salah klik |
| **Requirement/decision** | `FR-11.13`; `LAB-DEC-063` |
| **Kontrak** | `LAB-API-v1` `r12` §7.2 `PUT /lab-orders/{id}/cancel` |
| **Cakupan** | Satu pop-up berisi isian alasan, satu alert konfirmasi akhir, penyesuaian tampilnya aksi Batalkan |
| **Dependency** | `BE-LAB-32` |
| **Acceptance criteria** | `AC-96`, `AC-97` |
| **Kewenangan UI** | Pop-up **Batalkan Pemeriksaan** memuat isian Alasan Pembatalan **wajib** dan tombol `Lanjut Pembatalan`. Sesudah tombol itu dipilih, muncul **alert konfirmasi akhir** sebelum permintaan dikirim. Aksi Batalkan **tidak ditampilkan** pada pesanan yang sudah `Diproses`, `Selesai`, atau `Dibatalkan` |
| **Verifikasi** | Lint bersih; build produksi; uji unit atas muatan yang dikirim; verifikasi manual: menutup alert konfirmasi akhir **tidak** membatalkan pesanan |
| **Risiko/pemilik** | Sedang. Layar ini menghapus pekerjaan pasien bila salah. Pemilik: Laboratorium |
| **DoD** | Alasan wajib ditegakkan di layar **dan** tetap ditegakkan backend; aksi Batalkan tidak tampil pada status yang tidak sah; menutup alert tidak mengirim permintaan |

> **Label tombol alert konfirmasi akhir — DITETAPKAN 2026-09-16.**
>
> | Butir | Nilai |
> |---|---|
> | Tombol yang **benar-benar membatalkan** | **`Konfirmasi Pembatalan`** |
> | Ditetapkan oleh | Yoga Aji Pratama, pemilik modul, 2026-09-16 |
>
> Nada netral dan formal dipilih, tanpa kata "Ya", supaya selaras dengan tombol konfirmasi lain
> di sistem. Label ini **tidak boleh diubah** tanpa keputusan pemilik berikutnya.
>
> **Riwayat butir ini pantas dibaca:** sebelumnya berbunyi *"belum ditentukan; tanyakan pemilik
> modul sebelum menulis teksnya; jangan mengarang 'Ya, Batalkan' atau sejenisnya."* Larangan itu
> sudah dijalankan — labelnya ditanyakan, bukan dikarang.

### `FE-LAB-17` ✅ — Print membuka preview lebih dulu

> **Status: ✅ `SELESAI` — 2026-09-16.** Ketiga butir DoD terpenuhi dan **terbukti pada aplikasi
> yang benar-benar berjalan**: 8 pemeriksaan layar, 8 lolos. Laporan:
> [`task/report/frontend/FE-LAB-17.md`](../task/report/frontend/FE-LAB-17.md).
>
> **`AC-95` TERPENUHI PENUH, dan dengan itu `FR-11.12` tertutup ujung ke ujung.** Ketiga nama
> tanda tangan terbukti tercetak pada pesanan yang sudah dikonfirmasi.
>
> **Butir DoD yang paling menentukan dibuktikan terbalik**, bukan diasumsikan: `window.print`
> diganti pencatat, dan sesudah satu klik Cetak pada baris ia tercatat **nol kali** — kertas
> benar-benar tidak keluar sampai tombol di dalam pratinjau ditekan. Menutup pratinjau juga
> tercatat nol cetak dan nol permintaan tulis.
>
> **Satu asersi sempat gagal dan yang salah adalah ujinya, bukan produknya.** `react-to-print` v3
> mencetak lewat **iframe**, sehingga stub pada `window.print` halaman utama memang tidak akan
> tertangkap. Penyebabnya **diperiksa, bukan langsung dianggap masalah harness** — dugaan itu
> menyembunyikan cacat sungguhan bila keliru.
>
> **Satu pemeriksaan ditambahkan menyusul sesudah celahnya terlihat:** pemeriksaan tanda tangan
> semula memakai baris yang **belum** dikonfirmasi, sehingga hanya membuktikan bloknya ada, bukan
> namanya tercetak. Padahal itulah kriteria yang menjadi alasan task ini ada. Pelajaran `AC-83`
> dipakai lagi di sini.
>
> **Status sebelumnya ⛔ `TERTAHAN` pada pagi hari yang sama.** Riwayat ketiga penahannya
> **dipertahankan di bawah, bukan dihapus**, karena urutan kejadiannya pantas dibaca ulang:
> sebuah task dapat berpindah dari tertahan menjadi selesai dalam satu hari — yang berubah lebih
> dulu adalah keputusan yang diambil dan kontrak yang dibuka, bukan kodenya.
>
> **Bukan kehati-hatian umum; ketiga penahannya konkret dan terukur.**
>
> **Penahan pertama: tombol Print yang hendak diubah perilakunya tidak pernah ada.** Cakupan
> task ini berbunyi "perubahan perilaku tombol Print pada ketiga menu pemeriksaan". Pencarian
> kata `print` dan `cetak` pada seluruh view, hook, constant, service, dan slice modul
> Laboratorium menghasilkan **0 kemunculan**. Ketiga menu pemeriksaan hanya memiliki satu
> tombol, yaitu **Muat ulang**. `05-evidence-reconciliation.md` sendiri sudah mencatatnya pada
> baris `REC2-NEW-006`: *"Nol kemunculan pada kontrak maupun roadmap frontend"*. Kalimat
> "perubahan perilaku" datang dari artifact — sistem yang dipakai pemilik di tempat lain —
> bukan dari codebase ini. Yang sesungguhnya diminta adalah **membangun fitur cetak dari nol**,
> bukan mengubah satu tombol.
>
> **Penahan kedua — DITUTUP 2026-09-16.** Kewenangan UI menuntut tanda tangan **pembuat order,
> konfirmator, dan dokter pemeriksa** tetap tampil, dan sebelumnya hanya yang pertama tersedia.
> `r13` beserta `BE-LAB-33` lalu `r14` beserta `BE-LAB-34` menutupnya: `confirmedByName` dan
> `examinerDoctorName` kini terbaca pada jalur daftar maupun detail, dan `requestedByName` sudah
> ada sejak `r3`. **Ketiga tanda tangan dapat diisi.**
>
> **Penahan ketiga, ditemukan 2026-09-16 dan belum pernah tercatat: isi pesanan tidak dapat
> dibaca.** `BR-47` menetapkan pemeriksaan sedisiplin **berkumpul pada satu pesanan**, dan
> daftarnya disimpan `LabOrderedProcedure` sejak `BE-LAB-26`. Tetapi **nol DTO dan nol endpoint
> mengembalikannya** — pencarian `ProcedureNameSnapshot` pada seluruh area `LaboratoryManagement`
> menghasilkan nol kemunculan — sedangkan `LabOrder.ProcedureId` hanyalah **penunjuk wakil**,
> dinyatakan oleh komentar kodenya sendiri. `ExaminationCount` pun menghitung `LabExamination`,
> yaitu yang sedang dikerjakan dari wadah, bukan yang dipesan.
>
> Akibatnya: dokumen cetak **hanya dapat menyebut satu nama pemeriksaan**. Untuk pesanan
> Hemoglobin + Kalium yang sengaja digabung `BR-47`, dokumennya akan menyebut satu dan diam soal
> yang lain. **Pemilik modul memilih menunda, bukan mencetak apa adanya** — kelas bahaya yang sama
> dengan alasan task ini ditolak pertama kali.
>
> **Penahan ketiga — DITUTUP 2026-09-16, pada hari yang sama ia ditemukan.** `LAB-API-v1` `r15`
> disetujui lalu dilaksanakan `BE-LAB-35` ✅: `GET /lab-orders/{id}` kini mengembalikan
> `orderedProcedures`, terbukti dari database lewat 12 pemeriksaan.
>
> **⚠ Satu hal yang wajib dibaca sebelum layar ini dimulai.** `LabOrderedProcedure` berisi
> **0 baris** pada database. Artinya **seluruh 5 pesanan nyata hari ini menempuh jalur array
> kosong**, dan itulah jalur yang paling mungkin dilihat saat verifikasi layar. Layar cetak wajib
> menanganinya sebagai keadaan **sah** — bukan data rusak, bukan galat — dan mencetak
> `procedureName` sebagai isi lengkap pesanan pada jalur itu.
>
> **Ketiga keputusan pemilik — DITETAPKAN 2026-09-16.** Sebelumnya ketiganya kosong dan roadmap
> melarang mengarangnya. Larangan itu dijalankan: ketiganya ditanyakan sebelum satu baris pun
> ditulis.
>
> | Butir | Ketetapan |
> |---|---|
> | **Isi ringkasan cetak** | **Ditunda sampai `r15` jalan.** Dokumen wajib memuat daftar pemeriksaan yang benar-benar dipesan; mencetak nama wakilnya saja maupun mencetak tanpa menyebut pemeriksaan sama-sama ditolak |
> | **Letak tombol dan satuan cetak** | **Per baris** pada kolom aksi ketiga menu pemeriksaan, bersebelahan dengan Konfirmasi dan Batalkan. Satu klik membuka preview **satu pesanan** milik satu pasien — satuan yang sama dengan blok tanda tangan yang diminta DoD. Cetak rekap daftar **tidak** termasuk |
> | **Bentuk blok tanda tangan** | **Tiga kolom sejajar** di kaki halaman: Pembuat Order, Konfirmator, Dokter Pemeriksa. Masing-masing memuat nama tercetak, ruang tanda tangan, dan tanggal. **Yang belum terisi tetap dicetak dengan garis kosong** — DoD menuntut ketiganya tetap tampil, dan garis kosong itu sendiri adalah jejak bahwa pesanannya belum dikonfirmasi |
>
> Ketiga ketetapan ini **tidak boleh diubah** tanpa keputusan pemilik berikutnya.
>
> **Satu aturan turunan yang wajib diikuti ketika task ini kelak dikerjakan**, ditulis di sini
> supaya tidak ditafsirkan sendiri: bila `orderedProcedures` **kosong**, pesanan itu berpemeriksaan
> tunggal dan `procedureName` **adalah** isi lengkapnya — cetak itu. Bila **terisi**,
> `procedureName` hanyalah wakil dan **tidak boleh** dicetak sebagai isi pesanan. Lihat kontrak
> bagian 10.7.
>
> **Satu hal yang justru memudahkan ketika penahannya dibuka:** infrastruktur cetak sudah ada
> dan sudah dipakai modul lain — `react-to-print` pada resep, signa obat, surat pengantar dokter,
> kartu pasien kiosk, dan persetujuan rawat inap. Tidak ada yang perlu dibangun dari nol di sisi
> mekanismenya.
>
> **Yang dibutuhkan agar task ini dapat berjalan:**
>
> | Kebutuhan | Keadaan per 2026-09-16 |
> |---|---|
> | `LAB-API-v1` `r13` — ruas `confirmedByName`, `confirmedAt`, `examinerDoctorName` | ✅ **Terpenuhi.** `r13` lalu `r14`, dilaksanakan `BE-LAB-33` dan `BE-LAB-34` |
> | Isi ringkasan cetak dan bentuk blok tanda tangannya | ✅ **Ditetapkan** — lihat tabel ketetapan di atas |
> | Letak tombol Print dan satuan yang dicetak | ✅ **Ditetapkan** — per baris, satu pesanan |
> | `LAB-API-v1` `r15` — ruas `orderedProcedures` | ✅ **`approved` 2026-09-16** |
> | `BE-LAB-35` — pelaksanaan `r15` | ✅ **Selesai 2026-09-16**, terbukti dari database — [laporan](../task/report/backend/BE-LAB-35.md) |
>
> **Nol kebutuhan tersisa. Task ini siap dikerjakan.**
>
> **Nol berkas frontend diubah.** Task ini **tetap** tidak diturunkan menjadi versi sebagian,
> dan alasannya kini berpindah bersama penahannya — itu pantas dibaca karena kesimpulannya sama
> dua kali berturut-turut. Pada 2026-09-16 pagi alasannya **dua dari tiga tanda tangan kosong**;
> hari yang sama, sesudah penahan itu ditutup, alasannya menjadi **dokumen yang menyebut satu
> pemeriksaan padahal pesanannya memuat beberapa**. Keduanya cacat yang sama bentuknya: dokumen
> resmi yang **salah tanpa terlihat salah**. Dokumen semacam itu lebih berbahaya daripada tidak
> ada tombol cetak sama sekali, karena ketiadaan tombol segera terlihat sedangkan dokumen yang
> keliru hanya dipercaya orang.

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat ringkasan order sebelum kertas keluar |
| **Requirement/decision** | `REC2-NEW-006` — kewenangan UI, nol kontrak backend |
| **Cakupan** | Perubahan perilaku tombol Print pada ketiga menu pemeriksaan |
| **Dependency** | — |
| **Kewenangan UI** | Tombol Print **membuka preview**, tidak langsung mencetak. Pencetakan dilakukan dari preview. Tanda tangan pembuat order, konfirmator, dan dokter pemeriksa tetap tampil sesuai kebutuhan yang sudah ada |
| **Verifikasi** | Lint bersih; build produksi; verifikasi manual: satu klik Print tidak lagi mengeluarkan kertas |
| **Risiko/pemilik** | Rendah. Pemilik: Laboratorium |
| **DoD** | Print membuka preview; preview dapat dicetak; tanda tangan tetap tampil |

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
| `FE-LAB-15` ✅ | `MVP-5c` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Kolom Konfirmasi pada ketiga menu, pop-up berisi ringkasan pasien dan pemilih dokter, satu pemanggilan endpoint. Keempat butir DoD **terbukti pada aplikasi yang benar-benar berjalan**, termasuk "konfirmasi kedua tidak mungkin dilakukan dari layar". Lint bersih, build produksi hijau, 11 uji unit baru, 943/943 uji repository lolos — [laporan](../task/report/frontend/FE-LAB-15.md). **Sisanya ditutup hari itu juga:** bagian `AC-95` "tampil pada ringkasan cetak" diselesaikan `FE-LAB-17` ✅ | `BE-LAB-31` ✅ dan `BE-LAB-33` ✅ keduanya selesai. Ia sempat ⛔ `TERTAHAN` karena `r12` tidak menambah ruas respons, sehingga ketiga nilai yang wajib ditampilkan kolom Konfirmasi tidak dikembalikan endpoint mana pun. **`r13` disetujui lalu dilaksanakan `BE-LAB-33`** — tetapi `r13` menyebut DTO yang keliru: ketiga menu pemeriksaan membaca grup `Lab Monitoring`, bukan `LabOrderListResponse`. **`r14` dan `BE-LAB-34` menutupnya pada hari yang sama**: `confirmedAt`, `confirmedByName`, dan `examinerDoctorName` kini terbaca pada **jalur yang benar-benar dipakai ketiga menu**, dan terbukti dari database — [`BE-LAB-34.md`](../task/report/backend/BE-LAB-34.md). **Nol penahan tersisa** |
| `FE-LAB-16` ✅ | `MVP-5c` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Pop-up beralasan dan alert konfirmasi akhir berlabel `Konfirmasi Pembatalan`; aksi Batalkan disembunyikan pada status yang tidak sah. Ketiga butir DoD **terbukti pada aplikasi yang benar-benar berjalan**, termasuk "menutup alert tidak mengirim permintaan". 9 uji unit baru, 952/952 uji repository lolos — [laporan](../task/report/frontend/FE-LAB-16.md). **`FR-11.13` tertutup ujung ke ujung** | `BE-LAB-32` ✅ selesai 2026-09-16; **penahan terangkat dan tidak ada penahan kontrak**: layar ini hanya mengirim `cancelReason` dan membaca `orderStatus` yang sudah lama ada, sehingga tidak tersentuh celah `r13` yang menahan `FE-LAB-15`. Backend menolak `422` bila alasan kosong dan `409` bila status tidak sah, jadi penegakan di layar tidak berdiri sendiri. **Nol butir terbuka:** label tombol alert konfirmasi akhir **ditetapkan 2026-09-16** — `Konfirmasi Pembatalan` |
| `FE-LAB-17` ✅ | `MVP-5c` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Tombol Cetak per baris membuka pratinjau berisi ringkasan satu pesanan: daftar pemeriksaan terpesan, dan tiga blok tanda tangan sejajar. **Ketiga butir DoD terbukti pada aplikasi yang benar-benar berjalan** — termasuk yang paling menentukan, **satu klik Cetak tercatat nol memanggil `window.print`**. Lint bersih, build hijau, 14 uji unit baru, 977/977 uji repository lolos, 8 pemeriksaan layar — [laporan](../task/report/frontend/FE-LAB-17.md). **`AC-95` TERPENUHI PENUH; `FR-11.12` tertutup ujung ke ujung** | **Tiga penahan, dua sudah ditutup.** **(a)** Tombol Print **tidak pernah ada** — 0 kemunculan `print`/`cetak` di seluruh modul; yang diminta sebenarnya membangun fitur cetak dari nol. Ini **cakupan, bukan penahan**: infrastruktur `react-to-print` sudah dipakai lima layar lain. **(b) DITUTUP** — ketiga tanda tangan kini dapat diisi lewat `r13`/`BE-LAB-33` dan `r14`/`BE-LAB-34`. **(c) TERSISA, ditemukan 2026-09-16:** daftar pemeriksaan yang benar-benar dipesan **tidak dikembalikan DTO maupun endpoint mana pun** — `LabOrderedProcedure` berdiri sejak `BE-LAB-26` tetapi nol pembaca, dan `LabOrder.ProcedureId` hanyalah penunjuk **wakil**. Mencetak sekarang berarti dokumen resmi yang menyebut satu pemeriksaan padahal pesanannya memuat beberapa. **Ditutup hari itu juga** oleh `r15` `approved` beserta **`BE-LAB-35`** ✅. **Ketiga keputusan pemilik DITETAPKAN 2026-09-16:** isi cetak memuat daftar pemeriksaan terpesan, tombol **per baris** mencetak **satu pesanan**, dan blok tanda tangan **tiga kolom sejajar** dengan yang belum terisi tetap dicetak bergaris kosong. **⚠ Peringatan untuk pelaksananya:** `LabOrderedProcedure` berisi 0 baris pada database, sehingga 5 pesanan nyata menempuh jalur **array kosong** — jalur itu sah dan wajib mencetak `procedureName` sebagai isi lengkap |
| `FE-LAB-01` | `MVP-0` | — | — | **`SELESAI`** 2026-09-04 |
| `FE-LAB-13` ✅ | `MVP-5b` | `EPIC-LAB-11` | `BE-EXT-04` ✅, `BE-EXT-04b` ✅ | **`SELESAI`** — 2026-09-16. Langkah **Tujuan Layanan** berdiri di depan alur kiosk Pasien Lama: `Poliklinik` atau `Laboratorium`, dan bila Laboratorium dipilih ditambah satu pertanyaan surat dokter. Keempat butir DoD **terbukti pada aplikasi yang benar-benar berjalan** — termasuk yang paling menentukan, **muatan cabang poliklinik nol ruas tambahan**, diperiksa kunci demi kunci. Lint bersih, build produksi hijau, 11 uji unit baru, 963/963 uji repository lolos, 5 pemeriksaan layar — [laporan](../task/report/frontend/FE-LAB-13.md). **Panel kiosk `FE-LAB-14` kini benar-benar dapat terisi.** **Batas yang disebut apa adanya:** pasien Laboratorium belum memperoleh kunjungan maupun nomor antrean di kiosk — itu milik `BE-EXT-05` yang masih ⛔. **Cakupannya dipersempit atas keputusan pemilik modul:** alur Pasien Baru **tidak** ikut, karena sesinya langsung tertandai terpakai oleh kunjungan poliklinik dan tidak akan pernah terbaca panel Laboratorium |
| `FE-LAB-14` ✅ | `MVP-5b` | `EPIC-LAB-11` | `BE-EXT-04` ✅, `BE-LAB-27` ✅ | **`SELESAI`** — 2026-09-16. Panel sesi kiosk, pemilih pemeriksaan jamak lewat `POST /lab-orders/by-examinations`, dan pemberitahuan hasil pemecahan. Lint bersih, build produksi hijau, 11 uji unit baru, 932/932 uji repository lolos, dan tiga pemeriksaan layar dijalankan terhadap aplikasi yang benar-benar berjalan — [laporan](../task/report/frontend/FE-LAB-14.md). **Prasyarat konfigurasi:** peran petugas lab perlu izin `KioskScanSession : Read`. **Satu bagian verifikasi tidak selesai** — panel rincian sesudah simpan belum dilihat pada layar sungguhan; logikanya teruji unit |
| `FE-LAB-02` | `MVP-0` | `S3` | `BE-LAB-04`, `BE-LAB-05` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-03` | `MVP-0` | `S11` | `BE-LAB-06` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-04` | `MVP-0` | `S14` | `BE-LAB-07` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-05` | `MVP-1` | `S13a`, `S13b` | `BE-LAB-08` | **`SELESAI`** — [laporan](../task/report/frontend/FE-LAB-05.md). Verifikasi manual menunggu data induk perujuk diisi |
| `FE-LAB-06` | `MVP-1` | `S1a` | `BE-LAB-10` | **`SELESAI`** 2026-09-04 |
| `FE-LAB-07` | `MVP-2` | `S2` | `BE-LAB-12` | **`SELESAI`** — [laporan](../task/report/frontend/FE-LAB-07.md). Verifikasi manual menunggu backend dijalankan |
| `FE-LAB-08` | `MVP-3` | `S7` | `BE-LAB-14` | **`SELESAI`** — [laporan](../task/report/frontend/FE-LAB-08.md). Verifikasi manual menunggu backend dijalankan |
| `FE-LAB-09` | `MVP-3` | `S15` | `BE-LAB-15` | **`SELESAI`** — [laporan](../task/report/frontend/FE-LAB-09.md). Verifikasi manual menunggu backend dijalankan |

**Tidak ada satu pun task frontend yang terblokir per 2026-09-07.** `FE-LAB-05`, yang sejak
2026-09-04 menjadi satu-satunya task `BLOCKED`, kini terbuka: penahannya adalah **endpoint yang
belum ada**, dan endpoint itu sudah ada sejak `BE-LAB-08` selesai. Seluruh 22 task backend
Laboratorium selesai, sehingga tidak ada lagi task frontend yang menunggu pasangannya.

**Keadaan per 2026-09-07.**

| Keadaan | Task | Keterangan |
|---|---|---|
| Selesai | `FE-LAB-01` .. `FE-LAB-09` — **seluruhnya** | **Sembilan dari sembilan task frontend selesai per 2026-09-07.** Gelombang `MVP-0`, `MVP-1`, `MVP-2`, dan `MVP-3` tuntas. `FE-LAB-06` sempat dikerjakan lebih dulu ketika dependency `FE-LAB-05` diwaive pemilik modul |
| Terblokir | — | Tidak ada |
| Menunggu verifikasi manual | `FE-LAB-05`, `FE-LAB-07`, `FE-LAB-08`, `FE-LAB-09` | Keempatnya selesai pada source, uji, lint, dan build. **Dicoba dijalankan 2026-09-08 dan tetap tidak dapat diselesaikan — sebabnya lebih luas daripada yang tercatat sebelumnya.** Lihat bagian 8.1 |
| Menunggu data, bukan kode | `FE-LAB-05` | Layarnya selesai, tetapi formulir rujukan luar **belum dapat dipakai** sampai data induk instansi dan dokter perujuk diisi. Delapan skenario verifikasi manual menunggu itu |

`LAB-OPEN-018` sudah tidak menahan sejak 2026-09-04; lihat catatannya pada bagian 1.

### 8.1 Percobaan verifikasi manual — 2026-09-08

Backend dan frontend dijalankan lokal (`https://localhost:7184` dan `http://localhost:3000`)
terhadap `QuilvianNewDevYoga`. Keduanya **hidup dan sehat**. Yang menahan bukan aplikasinya.

**Basis data dev praktis kosong untuk Laboratorium.** Dihitung lewat API, bukan dugaan:

| Yang dibutuhkan skenario | Jumlah sebenarnya |
|---|---:|
| Prosedur berpenanda `IsLaboratory` | **0** — dari 1 prosedur di seluruh basis data |
| Katalog pemeriksaan laboratorium | **0** |
| Instansi perujuk / dokter perujuk | **0** / **0** |
| Batas nilai pemeriksaan | **0** |
| Pesanan laboratorium | **0** |
| Daftar kerja: belum selesai / cito terlambat | **0** / **0** |
| Monitoring ketiga disiplin | **0** / **0** / **0** |
| *pembanding:* alasan penolakan sampel (ter-seed otomatis) | 10 |

Konsekuensinya, **ke-32 skenario tidak dapat dijalankan**. Bukan hanya `FE-LAB-05` yang
menunggu data induk perujuk seperti tercatat sebelumnya: `FE-LAB-07`, `FE-LAB-08`, dan
`FE-LAB-09` sama-sama membutuhkan pesanan laboratorium, dan pesanan tidak dapat dibuat karena
**tidak ada satu pun jenis pemeriksaan laboratorium** untuk dipesan. Catatan lama *"menunggu
backend dijalankan"* karena itu menyesatkan — backend berjalan; yang tidak ada adalah datanya.

**Yang berhasil diverifikasi.** Dijalankan lewat Playwright terhadap aplikasi yang benar-benar
berjalan, **21 dari 21 pemeriksaan lolos**:

| Pemeriksaan | Hasil |
|---|---|
| Masuk sebagai `superadmin` | lolos |
| Kesembilan menu Laboratorium muncul di sidebar | 9/9 lolos |
| Kesembilan halaman terbuka, judulnya cocok, tanpa error boundary maupun galat runtime | 9/9 lolos |
| Ruas **Disiplin Laboratorium** tampil pada form Master Data → Prosedur | lolos |
| Ketiga pilihan disiplin muncul dengan label Indonesia | lolos |

Ini sekaligus menutup temuan bahwa enam route milik `FE-LAB-05`, `FE-LAB-08`, dan `FE-LAB-09`
tidak pernah terdaftar di sidebar sejak dibangun, sehingga hanya terbuka lewat pengetikan URL.

**Rantai penuh Master Data → katalog juga terbukti**, 9 dari 9, memakai satu prosedur uji yang
dibuat lalu dihapus kembali sehingga tidak ada penggolongan klinis karangan yang tertinggal:
disiplin pada tindakan non-laboratorium ditolak `400`, disiplin tak dikenal ditolak `400`,
prosedur berdisiplin Mikrobiologi tersimpan dan terbaca beserta labelnya, katalog memuatnya,
penyaring `discipline=Microbiology` menjaringnya sementara `ClinicalPathology` tidak, dan
mengosongkan disiplin mencabut golongannya.

**Yang dibutuhkan supaya ke-32 skenario dapat dijalankan**, berurutan:

1. Jenis pemeriksaan laboratorium diisi ke `MstProcedure` beserta disiplinnya — jalurnya sudah
   ada sejak 2026-09-08, daftar penggolongannya belum.
2. Tarif untuk pemeriksaan itu, supaya harga tampil saat memesan (`AC-43`).
3. Instansi dan dokter perujuk, untuk `FE-LAB-05` jalur rujukan luar.
4. Pasien dan kunjungan, untuk membuat pesanan.

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
| 21 | 2026-09-16 | **`FE-LAB-17` SELESAI. Seluruh layar modul Laboratorium tuntas, dan `FR-11.12` tertutup ujung ke ujung.** Tombol Cetak berdiri per baris pada ketiga menu pemeriksaan dan membuka pratinjau berisi ringkasan **satu** pesanan: daftar pemeriksaan yang benar-benar dipesan, beserta tiga blok tanda tangan sejajar. **Butir DoD yang paling menentukan dibuktikan terbalik**, bukan diasumsikan: `window.print` diganti pencatat, dan sesudah satu klik Cetak pada baris ia tercatat **nol kali** — kertas benar-benar tidak keluar sampai tombol di dalam pratinjau ditekan; menutup pratinjau pun tercatat nol cetak dan nol permintaan tulis. **`AC-95` TERPENUHI PENUH**: ketiga nama tanda tangan terbukti tercetak pada pesanan yang sudah dikonfirmasi. **Dua catatan proses pantas dibaca ulang.** Pertama, satu asersi sempat gagal dan **yang salah adalah ujinya** — `react-to-print` v3 mencetak lewat iframe, sehingga stub pada `window.print` halaman utama memang tidak akan tertangkap; penyebabnya diperiksa, bukan langsung dianggap masalah harness, karena dugaan itu menyembunyikan cacat sungguhan bila keliru. Kedua, **satu pemeriksaan ditambahkan menyusul**: pemeriksaan tanda tangan semula memakai baris yang belum dikonfirmasi, sehingga hanya membuktikan bloknya ada dan bukan namanya tercetak — padahal itulah kriteria yang menjadi alasan task ini ada. Pelajaran `AC-83` dipakai lagi. **Satu batas disebut apa adanya:** jalur daftar terpesan **terisi** belum pernah terlihat pada data sungguhan karena `LabOrderedProcedure` masih berisi 0 baris; ia terbukti lewat jawaban yang dipalsukan dan uji unit, sedangkan jalur kosongnya — yang ditempuh seluruh pesanan nyata hari ini — terbukti pada keduanya. Pencetakan ke printer fisik juga belum dijalankan; yang terbukti adalah permintaan cetaknya dipicu. Empat belas uji unit baru; **977 dari 977** uji repository lolos; nol berkas uji layar tertinggal | `DRAFT` |
| 20 | 2026-09-16 | **`FE-LAB-17` berpindah dari ⛔ `TERTAHAN` menjadi SIAP DIKERJAKAN — nol penahan, nol keputusan terbuka — dan seluruhnya terjadi pada hari yang sama.** `r15` disetujui lalu dilaksanakan `BE-LAB-35` ✅, terbukti dari database lewat 12 pemeriksaan. **Ketiga penahannya kini tertutup**, dan riwayatnya sengaja **dipertahankan** pada kartu task, bukan dihapus: sebuah task dapat berpindah dari tertahan menjadi siap tanpa satu baris pun ditulis untuknya — yang berubah adalah keputusan yang diambil dan kontrak yang dibuka. **Satu peringatan diteruskan dari `BE-LAB-35` dan ditulis pada kartu task, bukan disimpan di laporan backend saja:** `LabOrderedProcedure` berisi **0 baris** pada database, sehingga **seluruh 5 pesanan nyata menempuh jalur array kosong** — dan jalur itulah yang paling mungkin terlihat saat verifikasi layar nanti. Layar cetak wajib memperlakukannya sebagai keadaan **sah**, bukan data rusak, dan mencetak `procedureName` sebagai isi lengkap pesanan pada jalur itu. Peringatan ini ditulis di depan supaya pelaksananya tidak menyimpulkan fiturnya rusak ketika yang dilihatnya justru jalur normal untuk data hari ini. **Nol berkas frontend diubah pada revisi ini** | `DRAFT` |
| 19 | 2026-09-16 | **`FE-LAB-17` tetap ⛔, tetapi bentuk penahannya berubah menyeluruh: ketiga keputusan pemilik kini tertutup, dan yang tersisa tinggal satu — dan itu penahan yang baru ditemukan hari ini.** **Penahan (b) ditutup:** ketiga tanda tangan dapat diisi sejak `r13`/`BE-LAB-33` dan `r14`/`BE-LAB-34`. **Penahan (a) diturunkan statusnya menjadi cakupan, bukan penahan** — fitur cetak memang harus dibangun dari nol, tetapi `react-to-print` sudah dipakai lima layar lain. **Penahan (c) ditemukan dan ia menentukan:** daftar pemeriksaan yang benar-benar dipesan **tidak dapat dibaca siapa pun di luar backend**. `LabOrderedProcedure` berdiri sejak `BE-LAB-26` dan terisi sejak `BE-LAB-27`, tetapi nol DTO dan nol endpoint mengembalikannya; `LabOrder.ProcedureId` hanyalah **penunjuk wakil**, dinyatakan komentar kodenya sendiri; dan `ExaminationCount` menghitung yang sedang dikerjakan dari wadah, bukan yang dipesan. Mencetak hari ini berarti dokumen resmi yang menyebut satu pemeriksaan padahal pesanan gabungan `BR-47` memuat beberapa. **Pemilik modul memilih menunda, bukan mencetak apa adanya** — kelas bahaya yang sama dengan alasan task ini ditolak pertama kali, dan konsisten dengan penolakan versi dua tanda tangan kosong. Usul **`LAB-API-v1` `r15`** ditulis beserta pelaksanaannya **`BE-LAB-35`** ⛔, keduanya menunggu persetujuan. **Ketiga keputusan pemilik ditetapkan, sesudah ditanyakan dan bukan dikarang:** isi cetak ditunda sampai `r15` jalan; tombol berdiri **per baris** pada kolom aksi ketiga menu dan mencetak **satu pesanan**, bukan rekap daftar; blok tanda tangan **tiga kolom sejajar** dan **yang belum terisi tetap dicetak bergaris kosong**, karena garis kosong itu sendiri adalah jejak bahwa pesanannya belum dikonfirmasi. **Satu aturan turunan dikunci pada kartu task** supaya tidak ditafsirkan sendiri kelak: `orderedProcedures` kosong berarti pesanan berpemeriksaan tunggal dan `procedureName` adalah isi lengkapnya; terisi berarti `procedureName` hanya wakil dan **tidak boleh** dicetak sebagai isi pesanan. **Nol berkas frontend diubah** | `DRAFT` |
| 18 | 2026-09-16 | **Bukti pelaksanaan `FE-LAB-13`. Gelombang `MVP-5b` tuntas, dan panel kiosk `FE-LAB-14` kini punya yang mengisinya.** Langkah **Tujuan Layanan** berdiri di depan alur kiosk Pasien Lama, beserta satu pertanyaan surat dokter yang hanya muncul bila Laboratorium dipilih. **Tiga temuan dari source menentukan letaknya, dan ketiganya membalik dugaan yang wajar.** Pertama, kedua ruas sesi kiosk hanya dapat ditulis **sekali** — `BE-EXT-04b` sengaja tidak membangun jalur ubah. Kedua, pada alur Pasien Lama sesi dibentuk di langkah **pertama**, lima langkah sebelum `Layanan & Dokter`; memasang pertanyaannya pada langkah yang namanya paling cocok justru mustahil menulis apa pun. Ketiga, dan yang paling menentukan: pada alur Pasien Baru sesi **langsung dikonsumsi** kunjungan poliklinik lewat `PatientEncounterController.cs:686`, sedangkan panel `FE-LAB-14` menyaring justru yang belum terpakai — sesi Laboratorium dari alur itu hilang pada detik yang sama ia dibuat. **Akibatnya pada cakupan diputuskan pemilik modul, bukan disempitkan sepihak:** tiga hal ditanyakan sebelum satu baris pun ditulis — alur mana yang dipasangi, di mana alurnya berhenti, dan apakah cabang poliklinik ikut menulis tujuan layanannya. Jawabannya Pasien Lama saja, berhenti sesudah identitas terbaca, dan cabang poliklinik tidak menulis apa pun. **Satu temuan menghindarkan `400` yang pasti terjadi:** `Program.cs` nol mendaftarkan `JsonStringEnumConverter`, sehingga enum pada body JSON hanya terbaca sebagai angka; `targetService` dikirim `2`, bukan `"Laboratory"` seperti penyaring query milik `FE-LAB-14`. **Butir DoD yang paling mudah dilanggar diam-diam dibuktikan terbalik:** muatan cabang poliklinik diperiksa kunci demi kunci dan terbukti nol ruas tambahan — bukan berisi `null`, melainkan memang tidak ada ruasnya. **Satu perbedaan tak terhindarkan disebut apa adanya**, bukan disamarkan: pasien alur Pasien Lama kini melihat satu layar tambahan di depan; sesudah `Poliklinik` ditekan, nol perilaku berikutnya berubah. **Satu batas ditulis terang:** pasien Laboratorium belum memperoleh kunjungan maupun nomor antrean di kiosk — itu milik `BE-EXT-05` yang masih ⛔, dan `AC-45` melarang Laboratorium membentuknya. Sebelas uji unit baru; **963 dari 963** uji repository lolos; lima pemeriksaan layar terhadap build produksi yang benar-benar berjalan; nol berkas uji layar tertinggal | `DRAFT` |
| 17 | 2026-09-16 | **Bukti pelaksanaan `FE-LAB-16`. `FR-11.13` kini tertutup ujung ke ujung, dan gelombang `MVP-5c` tinggal `FE-LAB-17`.** Pop-up pembatalan beralasan beserta alert konfirmasi akhir berlabel `Konfirmasi Pembatalan` — label yang ditetapkan pemilik modul hari ini, sesudah roadmap melarang mengarangnya. **Ketiga butir DoD terbukti pada aplikasi yang benar-benar berjalan**, termasuk yang paling menentukan: **menutup alert konfirmasi akhir tidak mengirim permintaan apa pun**, diperiksa dengan mencatat setiap permintaan yang tiba dan memastikan jumlahnya tetap nol. **Dua keputusan implementasi pantas dibaca ulang.** Pertama, kedua tahap disimpan sebagai **dua keadaan terpisah** — bukan satu keadaan dengan penanda di dalamnya — sehingga keduanya tidak dapat terbuka bersamaan dan menutup tahap 2 benar-benar berarti tidak ada yang dikirim. Kedua, daftar status yang boleh dibatalkan ditulis sebagai **yang diizinkan**, bukan yang dilarang, supaya status baru yang kelak ditambahkan tidak otomatis menjadi dapat dibatalkan; hal itu diuji langsung. **Satu selisih terhadap kewenangan UI dilaporkan, bukan didiamkan:** kewenangan menulis tiga status yang dilarang, sedangkan `VAL-75` mempersempit lebih jauh — `Accepted` dan `OnHold` juga ditolak backend. Yang diikuti kontraknya, karena menampilkan aksi di sana berarti memasang tombol yang selalu gagal `409`; bila pemilik menghendaki lain, itu perubahan kewenangan UI tersendiri. Sembilan uji unit baru; 952 dari 952 uji repository lolos; nol berkas uji layar tertinggal | `DRAFT` |
| 16 | 2026-09-16 | **Bukti pelaksanaan `FE-LAB-15`.** Kolom Konfirmasi berdiri pada ketiga menu pemeriksaan beserta pop-up berisi ringkasan pasien dan pemilih dokter pemeriksa. **Keempat butir DoD terbukti pada aplikasi yang benar-benar berjalan**, bukan hanya lolos lint dan build — termasuk butir yang paling menentukan: **konfirmasi kedua tidak mungkin dilakukan dari layar**. **Kewenangan UI ditegakkan kata demi kata dan diperiksa dari DOM**, bukan diasumsikan: nol label `Terkonfirmasi` tambahan pada kolom baris yang sudah dikonfirmasi — diperiksa sesudah teks bawaan "Belum Terkonfirmasi" dibuang — dan nol kotak isian konfirmator pada pop-up, diperiksa dari teks pop-upnya sendiri. Muatan yang dikirim tepat satu ruas. **Satu keputusan implementasi pantas dibaca ulang:** baris diperbarui dari **jawaban server**, bukan ditebak layar, sehingga kolomnya menampilkan nama dan waktu yang benar-benar tersimpan dan tombolnya nonaktif karena statusnya memang berpindah — bukan karena layar mengingat pernah menekannya. **Satu asersi uji sempat gagal dan yang salah adalah ujinya**, bukan produknya: regexnya ikut mencocoki kata di dalam "Belum Terkonfirmasi". Sebelas uji unit baru; 943 dari 943 uji repository lolos; nol berkas uji layar tertinggal. **Satu bagian `AC-95` tetap terbuka** — "tampil pada ringkasan cetak" milik `FE-LAB-17` yang masih ⛔ | `DRAFT` |
| 15 | 2026-09-16 | **Penahan `FE-LAB-15` benar-benar terangkat — pada percobaan kedua.** Revisi 14 sempat menyatakannya siap dimulai atas dasar `BE-LAB-33`, dan **itu terlalu cepat**: `r13` menyebut `LabOrderListResponse`, padahal ketiga menu pemeriksaan membaca grup `Lab Monitoring`. Kekeliruannya ketahuan tepat ketika layar ini hendak dimulai, dan ditutup hari itu juga lewat `r14` beserta `BE-LAB-34` — ketiga ruas kini terbaca pada **jalur yang benar-benar dipanggil layar**, terbukti dari database. **Entri ini sengaja tidak menimpa revisi 14**, supaya urutan kejadiannya terbaca apa adanya: sebuah penahan pernah dinyatakan terangkat sebelum benar-benar terangkat, dan yang menemukannya adalah pekerjaan berikutnya, bukan tinjauan | `DRAFT` |
| 14 | 2026-09-16 | **`FE-LAB-15` berpindah dari ⛔ `TERTAHAN` menjadi siap dimulai — penahannya terangkat pada hari yang sama ia ditemukan.** `LAB-API-v1` `r13` disetujui lalu dilaksanakan `BE-LAB-33`, sehingga `confirmedAt`, `confirmedByName`, dan `examinerDoctorName` kini terbaca pada jalur daftar dan **terbukti dari database**. **Nol penahan tersisa.** Riwayat penahannya **dipertahankan** pada entri task, bukan dihapus, karena urutan kejadiannya pantas dibaca ulang: celah kontrak itu tidak ditemukan oleh build, uji, maupun tinjauan kontrak — ketiganya hijau ketika celahnya masih ada — melainkan dengan membaca apa yang dibutuhkan layar konsumennya. **Dua layar kini siap dikerjakan tanpa penahan apa pun:** `FE-LAB-15` dan `FE-LAB-16`. **`FE-LAB-17` tetap ⛔**, dan pembedaannya ditulis tegas supaya tidak tertukar: `r13` hanya menutup penahan tanda tangannya; tombol Print yang tidak pernah ada dan ketiga keputusan pemilik tentang isi ringkasan cetak, letak tombol, serta bentuk blok tanda tangan **tidak tersentuh** | `DRAFT` |
| 13 | 2026-09-16 | **Dua keputusan pemilik modul dicatat, dan keduanya membuka pekerjaan yang tertahan.** **Pertama, label tombol alert konfirmasi akhir `FE-LAB-16` ditetapkan: `Konfirmasi Pembatalan`.** Nada netral dan formal dipilih, tanpa kata "Ya", supaya selaras dengan tombol konfirmasi lain di sistem. Butir ini sebelumnya berbunyi "belum ditentukan; jangan mengarang" — larangan itu **dijalankan**: labelnya ditanyakan, bukan dikarang. **`FE-LAB-16` kini nol butir terbuka.** **Kedua, `LAB-API-v1` `r13` disetujui**, menambahkan lima ruas respons konfirmasi: `confirmedAt`, `confirmedByName`, dan `examinerDoctorName` pada `LabOrderListResponse`; `confirmedByUserId` dan `examinerDoctorId` pada `LabOrderDetailResponse`. Seluruhnya aditif — nol endpoint, ruas, nilai enum, permission, dan migration yang berubah. **Akibatnya pada kedua task yang tertahan berbeda, dan itu ditulis terang:** `FE-LAB-15` penahannya **tinggal satu** — pelaksanaan `r13` lewat `BE-LAB-33` — sehingga ia akan terbuka begitu task itu selesai; sedangkan `FE-LAB-17` **tetap tertahan**, karena penahan (a) — tombol Print yang tidak pernah ada — dan ketiga keputusan pemilik tentang isi ringkasan cetak, letak tombol, serta bentuk blok tanda tangan **tidak tersentuh** oleh `r13` | `DRAFT` |
| 12 | 2026-09-16 | **`FE-LAB-17` dihentikan sebelum satu baris pun ditulis, dan penahannya dicatat bernomor.** Pemeriksaan pra-implementasi menemukan bahwa **tombol Print yang hendak diubah perilakunya tidak pernah ada**: pencarian `print` dan `cetak` pada seluruh view, hook, constant, service, dan slice modul Laboratorium menghasilkan **0 kemunculan**, dan ketiga menu pemeriksaan hanya memiliki tombol **Muat ulang**. `05-evidence-reconciliation.md` sudah mencatat hal yang sama pada `REC2-NEW-006` — *"Nol kemunculan pada kontrak maupun roadmap frontend"* — tetapi cakupan task terlanjur ditulis sebagai "perubahan perilaku tombol Print", kalimat yang datang dari **artifact**, bukan dari codebase ini. Yang sesungguhnya diminta adalah membangun fitur cetak dari nol. **Penahan kedua lebih menentukan:** kewenangan UI dan DoD sama-sama menuntut tanda tangan **pembuat order, konfirmator, dan dokter pemeriksa** tetap tampil, sedangkan hanya yang pertama tersedia hari ini. Konfirmator dan dokter pemeriksa **tidak dikembalikan endpoint mana pun** — **penahan yang sama persis dengan `FE-LAB-15`**, dan tertutup oleh usul `LAB-API-v1` `r13` yang sama. **Task ini sengaja tidak diturunkan menjadi versi sebagian**, karena versi sebagian berarti mencetak dokumen resmi dengan dua dari tiga tanda tangan kosong — lebih berbahaya daripada tidak ada tombol cetak sama sekali. **Tiga keputusan pemilik yang juga belum ada dicatat:** isi ringkasan cetak, letak tombolnya — menu pemeriksaan adalah layar daftar, sehingga mencetak "ringkasan order" dari sana menuntut keputusan per baris atau per pilihan — dan bentuk blok tanda tangannya. **Satu hal yang memudahkan ketika penahannya dibuka:** infrastruktur cetak `react-to-print` sudah ada dan dipakai resep, signa obat, surat pengantar dokter, kartu pasien kiosk, serta persetujuan rawat inap; mekanismenya tidak perlu dibangun. Nol berkas frontend diubah | `DRAFT` |
| 11 | 2026-09-16 | **Bukti pelaksanaan `FE-LAB-14`, ditulis `build-module-frontend`. Sekaligus `FE-LAB-15` ditandai ⛔ `TERTAHAN` dan `FE-LAB-16` dinyatakan siap dimulai.** `FE-LAB-14` berpindah menjadi **✅ `SELESAI`**: panel **Menunggu dari Kiosk** pada layar pendaftaran, pemilih pemeriksaan **jamak** yang mengirim **satu** permintaan ke `POST /lab-orders/by-examinations`, dan pemberitahuan hasil pemecahan sebelum maupun sesudah simpan. **Verifikasi dijalankan terhadap aplikasi yang benar-benar berjalan**, bukan hanya lint dan build — dan itulah yang menyelamatkannya: **dua cacat nyata ditemukan yang seluruhnya lolos dari lint, build, dan uji unit.** Pertama, menarik pasien dari kiosk tidak terlihat apa-apa karena pasien terpilih hanya dicari di dalam hasil pencarian, sedangkan pasien kiosk ditarik tanpa mengetik kata kunci apa pun. Kedua, rincian pemecahan terhapus sendiri sepersekian detik sesudah tampil, karena pemilih katalog memancarkan callback pada setiap render ulang — termasuk saat harga selesai dimuat — bukan hanya saat pilihan berubah. **Satu keputusan teknis menentukan apakah layar ini dapat dipakai sama sekali:** dari dua jalur baca sesi kiosk yang isinya identik, yang dipakai adalah `admin/options` — karena `options` dijaga `KioskReadPolicy` yang hanya mengakui SuperAdmin, Administrator, dan akun kiosk, sehingga akan menjawab `403` untuk **setiap petugas laboratorium**, yakni pengguna yang justru dituju layar ini. **Prasyarat konfigurasi dicatat:** izin `KioskScanSession : Read` perlu diberikan kepada peran petugas lab. **Satu bagian verifikasi tidak selesai dan disebut apa adanya**, bukan didiamkan: panel rincian sesudah simpan belum pernah dilihat pada layar sungguhan karena nilai kotak pilihan Kunjungan Pasien tidak bertahan pada harness bertopeng; gejalanya tidak berhasil dipisahkan dari cara harness memalsukan jalur itu, sehingga **tidak** dilaporkan sebagai cacat produk yang terkonfirmasi, dan logikanya tetap teruji lewat uji unit. **Satu temuan di luar cakupan:** penurunan disiplin yang ditambahkan commit `4031fd3d7` tidak pernah menghasilkan nilai — ia membaca ruas yang tidak pernah ada; akibatnya nol karena `BE-LAB-29` sudah menurunkannya di backend, dan task ini mencabutnya. Sebelas uji unit baru ditulis; 932 dari 932 uji repository lolos; nol berkas uji layar tertinggal di repository | `DRAFT` |
|---:|---|---|---|
| 10 | 2026-09-15 | **Gelombang `MVP-5c` ditambahkan.** Tiga layar menurunkan `LAB-DEC-061` dan `LAB-DEC-063`: `FE-LAB-15` kolom Konfirmasi beserta pop-upnya, `FE-LAB-16` pop-up pembatalan beralasan beserta alert konfirmasi akhir, dan `FE-LAB-17` Print yang membuka preview lebih dulu. **Satu kewenangan UI ditulis tegas sebagai larangan:** nol kotak isian konfirmator — namanya datang dari server, dan ruasnya memang tidak ada pada DTO permintaan `r12`. **Satu hal sengaja dibiarkan kosong:** label tombol di dalam alert konfirmasi akhir belum ditetapkan pemilik modul, dan `FE-LAB-16` dilarang mengarangnya | `DRAFT` |
| 14 | 2026-09-15 | **Gelombang `MVP-5b` ditambahkan** — dua task, `FE-LAB-13` dan `FE-LAB-14`, di bawah persetujuan lintas modul `LAB-REQ-006`. **Temuan yang memperkecil pekerjaannya:** layar kiosk **sudah ada di repo frontend ini** — `src/app/kiosk/` beserta `registration/new-patient`, `old-patient`, `patient-card`, dan `doctor-schedule` — sehingga `FE-LAB-13` menambahkan **pilihan layanan pada alur yang sudah berjalan, bukan kiosk baru**, dan DoD-nya menuntut nol perilaku alur lama yang berubah beserta uji lama yang lulus tanpa disentuh. `FE-LAB-14` menambahkan daftar sesi kiosk dan pemilih pemeriksaan pada layar pendaftaran laboratorium yang sudah ada. **Dua kewenangan UI dikunci.** Pertama, disiplin **tidak ditanyakan sama sekali** — `LAB-DEC-048` butir 6 sudah mencabut kotak pilihannya, dan menanyakan ulang jawaban yang sudah ada di katalog hanya menambah kesempatan menjawab keliru. Kedua, layar **wajib memberitahukan hasil pemecahan**: ini satu-satunya tempat pada modul ini di mana satu tindakan petugas menghasilkan lebih dari satu objek bisnis, dan tanpa pemberitahuan dugaan pertama yang wajar adalah petugas tidak sengaja menekan simpan dua kali. Pada `FE-LAB-13` ditegaskan pula bahwa pemakainya **pasien, bukan petugas**: tidak ada istilah teknis, dan kata "disiplin" tidak muncul di layar mana pun | `DRAFT` |
| 13 | 2026-09-14 | **Gelombang `MVP-5a` ditambahkan** — tiga task `FE-LAB-10` sampai `FE-LAB-12`. `FE-LAB-10` mengelola jenis specimen beserta daftar pantau `Lainnya`, ditempatkan di `health-services/master-data/` sesuai `AC-49`, bukan di folder laboratorium. `FE-LAB-11` formulir penerimaan memuat wilayah A sampai E; **dua wilayahnya digambar sebagai tempat tanpa isi yang dikunci** karena `FR-11.9` dan `FR-11.10` menunggu `LAB-REQ-005` — dan layar itu tidak boleh berpura-pura jalan buntunya sudah hilang. `FE-LAB-12` daftar dan detail penerimaan, dengan butir DoD bahwa daftarnya memakai **waktu penerimaan nyata**, bukan `ReceivedAt`. `AC-76` masuk DoD `FE-LAB-11` sebagai regresi: seluruh uji layar lab lama wajib lulus **tanpa disentuh**. Grafik urutan dependency ditulis untuk `MVP-5a`; ketiadaannya pada gelombang lama dicatat sebagai gap | `DRAFT` |
| 12 | 2026-09-08 | **Percobaan verifikasi manual dan koreksi penahannya, ditulis manual atas instruksi pemilik modul.** Backend dan frontend dijalankan lokal terhadap `QuilvianNewDevYoga`; keduanya sehat. Ke-32 skenario tetap **tidak dapat dijalankan**, tetapi sebabnya berbeda dari yang tercatat: basis data dev tidak memiliki **satu pun** jenis pemeriksaan laboratorium — 0 dari 1 prosedur di seluruh basis data — sehingga pesanan tidak dapat dibuat sama sekali. Catatan lama *"menunggu backend dijalankan"* untuk `FE-LAB-07`, `FE-LAB-08`, dan `FE-LAB-09` karena itu menyesatkan; ketiganya menunggu **data**, bukan proses. Angkanya dihitung lewat API dan dicatat pada bagian 8.1. Yang berhasil diverifikasi dijalankan lewat Playwright terhadap aplikasi berjalan, **21 dari 21 lolos**: login, kesembilan menu Laboratorium muncul di sidebar, kesembilan halaman terbuka tanpa error boundary maupun galat runtime, dan ruas **Disiplin Laboratorium** tampil beserta ketiga pilihannya. Ini sekaligus menutup temuan bahwa enam route milik `FE-LAB-05`, `FE-LAB-08`, dan `FE-LAB-09` tidak pernah terdaftar di sidebar sejak dibangun, sehingga selama ini hanya terbuka lewat pengetikan URL. Rantai Master Data → katalog terbukti terpisah, 9 dari 9, memakai prosedur uji yang dibuat lalu dihapus kembali | `DRAFT` |
| 1 | 2026-09-02 | Roadmap frontend pertama. 9 task disusun dan dipasangkan ke gelombang backendnya, bukan ditumpuk pada `MVP-4`, setelah kontrak dikunci mengizinkan kerja paralel | `DRAFT` |
| 2 | 2026-09-04 | `FE-LAB-01` selesai dikerjakan dan divalidasi. Status task dan tautan laporannya dicatat; gerbang `LAB-OPEN-018` dinyatakan tidak lagi menahan pekerjaan frontend karena berkas aturannya sudah tersedia di runtime | `DRAFT` |
| 3 | 2026-09-04 | `FE-LAB-02` selesai dikerjakan dan divalidasi. Enam layar batas nilai berdiri beserta jalur pengajuan batas kritis yang terpisah. Tiga temuan dicatat: `AC-34` belum memuat pelaku karena respons riwayat backend tanpa nama, status sebelas endpoint pada dokumen kontrak masih tertulis `Rencana` padahal sudah ada, dan peran pemegang `LabCriticalBound : Approve` masih belum ditetapkan | `DRAFT` |
| 4 | 2026-09-04 | `FE-LAB-03` selesai dikerjakan dan divalidasi. Tiga layar alasan penolakan berdiri, dan `LAB-FE-012` ditegakkan empat lapis. Dua batas dicatat: grup endpoint ini tidak punya `GET /{id}` sehingga tidak ada halaman detail, dan frontend belum menerima daftar permission sehingga penyembunyian aksi penanda sistem hanya sedekat peran | `DRAFT` |
| 5 | 2026-09-04 | `FE-LAB-04` selesai dikerjakan dan divalidasi. Menu tarif baca saja berdiri tanpa satu pun jalur ubah, dan komponen pemilih katalog berdiri siap dipakai ulang `FE-LAB-06`. Ketiga konstanta yang dilarang diduplikasi terbukti dipakai ulang, bukan disalin. **Gelombang `MVP-0` selesai seluruhnya** | `DRAFT` |
| 11 | 2026-09-07 | **Pembaruan bukti pelaksanaan, ditulis `build-module-frontend`.** `FE-LAB-09` **selesai**, dan dengan itu **seluruh sembilan task frontend Laboratorium selesai** — gelombang `MVP-0` sampai `MVP-3` tuntas. Tiga route berdiri di bawah `/lab-monitoring`, dijaga empat belas uji unit. **Butir DoD "penyaringnya identik pada ketiganya" dibuat menjadi sifat struktural, bukan janji:** ketiga disiplin menunjuk **objek definisi penyaring yang sama**, bukan tiga salinan yang kebetulan seragam — tiga salinan pasti bercabang begitu satu penyaring ditambahkan di satu layar dan terlupa di dua lainnya, tanpa satu pun uji gagal. Ujinya memeriksa **identitas objek**, bukan kesamaan isi, sehingga percabangan pertama langsung tertangkap. Butir "tanpa duplikasi" dipenuhi dengan satu view, satu susunan kolom, satu hook, dan satu berkas gaya; ketiga halaman route masing-masing hanya sembilan baris. **Keputusan `LAB-DEC-025` dijaga dua arah:** definisi penyaringnya tidak punya ruas disiplin, dan parameter permintaannya tidak pernah membawa disiplin — keduanya dijaga uji, karena menambahkan ruas itu akan mengembalikan layar menjadi satu daftar berpenyaring yang justru ditolak keputusannya. Tautan yang menyebut disiplin tak dikenal **tidak dialihkan diam-diam** ke disiplin lain, karena menampilkan pesanan disiplin yang keliru tampak wajar dan tidak ada yang menandainya. State disimpan per disiplin supaya berpindah menu tidak mengosongkan penyaring yang baru disusun. Satu batas dicatat: verifikasi manual delapan skenario belum dijalankan | `DRAFT` |
| 10 | 2026-09-07 | **Pembaruan bukti pelaksanaan, ditulis `build-module-frontend`.** `FE-LAB-08` **selesai**: dua layar berdiri — daftar kerja `/lab-worklists` dan pantau keterlambatan `/lab-worklists/cito-overdue` — dijaga enam belas uji unit. Satuannya **pemeriksaan**, bukan pesanan, sehingga satu pesanan yang memuat Kalium cito dan Kolesterol biasa hanya menaikkan Kalium (`AC-39`). **Satu ancaman langsung terhadap `LAB-FE-006` ditemukan saat membaca komponen dasarnya, bukan saat menguji:** `DataTable` mengurutkan ulang datanya sendiri secara bawaan (`sortLatestFirst` bernilai `true` bila tidak dimatikan), sehingga pemeriksaan biasa yang diminta belakangan akan melompati cito yang diminta lebih dulu — invariantnya batal tanpa satu baris kode pun terlihat salah, dan backend tetap benar sepanjang waktu. Penegakannya dibuat berlapis dua: mematikan pengurutan bawaan tabel, **dan** melewatkan seluruh baris ke `sortWorklistRows` yang menegakkan kembali urutan cito sesudah pengurutan pilihan petugas. Ujinya menelusuri **seluruh** isi `LAB_WORKLIST_SORT_OPTIONS`, bukan satu contoh yang dipilih tangan, sehingga menambah pilihan baru tanpa melewatkannya lewat `enforceCitoFirst` akan membuat uji itu gagal. `VAL-39` dijaga uji tersendiri: baris yang jenis pemeriksaannya belum punya batas waktu tetap ditampilkan tetapi tidak dihitung terlambat. Satu cacat ditemukan **oleh uji**: `pageSize` bernilai negatif sempat menghasilkan satu baris per halaman, lalu implementasinya yang diperbaiki. Dua batas dinyatakan terbuka: pengurutan berlaku pada halaman yang terbuka saja karena kontraknya tidak punya ruas pengurutan, dan verifikasi manual delapan skenario belum dijalankan | `DRAFT` |
| 9 | 2026-09-07 | **Pembaruan bukti pelaksanaan, ditulis `build-module-frontend`.** `FE-LAB-07` **selesai untuk source dan uji**: layar wadah dan pemeriksaan berdiri sebagai route bersarang `/lab-orders/[slug]/specimens`, dengan perencanaan wadah, alur menyatakan layak, menolak, ambil ulang, menahan, dan melanjutkan. Sembilan belas uji unit menjaganya. **Kedua invariant keselamatan ditegakkan sebagai data, bukan sebagai susunan JSX** — `resolveSpecimenActions` yang memutuskan ada tidaknya aksi tolak, sehingga selama isi wadah belum termuat aksi itu **tidak dibuat sama sekali**; menaruh penjagaan di dalam JSX berarti ia dapat tergeser diam-diam oleh perapian tampilan berikutnya tanpa satu pun uji ikut gagal. `LAB-FE-009` berlapis tiga: pada data, pada kartu wadah sebagai daftar terbuka, dan sekali lagi di dalam dialog penolakan. `VAL-13` dijaga uji yang memeriksa katalog aksi tidak memuat satu pun aksi berlingkup pemeriksaan — `POST /lab-examinations/{id}/cancel` milik `BE-LAB-16` sengaja **tidak** dipakai di layar ini. **Dua hal di luar kendali task dicatat apa adanya:** `HEAD` frontend berpindah dari `72f050b50` ke `c71c02a07` di tengah pengerjaan karena merge pemilik repository — pekerjaan `FE-LAB-05` yang sudah tercommit tetap utuh — dan merge itu membawa **lima kelompok deklarasi kembar** pada `billing-management` yang membuat `npm run build` gagal — nol error menunjuk ke `laboratory-management`. Salah satunya lebih berat daripada gagal build: `addCase` kembar untuk action type yang sama membuat Redux Toolkit melempar saat *store* dibentuk, sehingga **seluruh aplikasi** tidak dapat dijalankan, bukan hanya layar Billing. Atas instruksi eksplisit pemilik repository, salinan keduanya dibuang — 70 baris dihapus, **nol** ditambahkan — dan `npm run lint:errors` serta `npm run build` kembali lolos. Yang tersisa hanya verifikasi manual delapan skenario, yang menunggu backend dijalankan | `DRAFT` |
| 8 | 2026-09-07 | **Pembaruan bukti pelaksanaan, ditulis `build-module-frontend`.** `FE-LAB-05` **selesai**, dan dengan itu **gelombang `MVP-1` frontend selesai seluruhnya**. Tiga layar berdiri: pencarian pasien, pendaftaran datang langsung, dan pendaftaran rujukan luar. Alurnya menyambung ke pembuatan pesanan dengan kunjungan yang **sudah terisi** — `useLabOrderForm` milik `FE-LAB-06` disentuh secara aditif untuk itu, dan seluruh uji lamanya tetap lolos. `AC-50` ditegakkan **secara struktural**: formulirnya tidak punya satu pun kotak isian nama perujuk, dan uji memeriksa muatan yang dikirim juga tidak punya ruas namanya. Tiga belas uji unit baru menjaga aturan murni kunci idempotensi, `VAL-43`, `VAL-44`, dan bentuk muatan. **Satu penahan baru ditemukan dan ditutup pada sesi yang sama:** `MstReferralInstitution` dan `MstReferralDoctor` ternyata **tidak punya endpoint sama sekali** — `BE-EXT-02` memang tidak membuatnya — sehingga daftar perujuk tidak punya sumber; dua endpoint bacanya dibangun atas instruksi pemilik modul, dan butir Verifikasi `BE-EXT-02` *"kedua data induk dapat dipilih dari daftar"* yang selama ini tidak pernah terpenuhi kini terpenuhi. **Satu batas dicatat apa adanya:** verifikasi manual **belum dijalankan** — delapan skenarionya menunggu data induk perujuk diisi dan backend dijalankan kembali; selama daftarnya kosong, formulir rujukan luar tidak dapat dipakai walaupun layarnya sudah benar | `DRAFT` |
| 7 | 2026-09-07 | **Penahan `FE-LAB-05` dicabut, ditulis `build-module-backend`.** `BE-LAB-08` selesai, sehingga `FE-LAB-05` berpindah dari **`BLOCKED`** menjadi **siap dikerjakan** — dan dengan itu **tidak ada lagi task frontend Laboratorium yang terblokir**. Ketiga endpoint yang dibutuhkannya tersedia dan terdokumentasi Swagger pada grup `Health Services / Laboratory Management / Lab Patient Registration`. Tiga hal ditambahkan pada kartu task sebagai syarat pelaksanaan, dan ketiganya wajib dibaca sebelum layarnya dibuat: `idempotencyKey` dibuat layar **saat formulir dibuka**, bukan saat tombol ditekan, dan wajib dikirim — tanpa itu penekanan Simpan dua kali menghasilkan dua kunjungan; jawaban membawa `isReplay` yang berarti **berhasil**, bukan gagal; serta instansi dan dokter perujuk dikirim sebagai penunjuk, karena permintaannya memang tidak punya ruas nama sama sekali | `DRAFT` |
| 6 | 2026-09-04 | `FE-LAB-05` ditandai **`BLOCKED`** setelah diverifikasi terhadap source backend: `BE-LAB-08` belum ada sama sekali. Pemilik modul memutuskan mewaive dependency itu dan mendahulukan `FE-LAB-06`, yang kemudian **selesai** — penanda cito dan duplo melekat pada baris pemeriksaan, dan `AC-40` dijaga uji unit. Satu batas kontrak dibuka: respons pesanan tidak membawa `requestedByUserId`, sehingga `VAL-03` belum dapat ditegakkan penuh di layar | `DRAFT` |
| 7 | 2026-09-04 | Dua paragraf naratif yang sudah basi disesuaikan dengan tabel status: catatan `LAB-OPEN-018` pada bagian 1 ditandai sudah ditutup, dan kalimat di bawah tabel bagian 8 diganti ringkasan keadaan yang benar-benar berlaku — lima task selesai, satu terblokir, tiga siap dikerjakan. Tidak ada status task yang berubah pada revisi ini | `DRAFT` |
