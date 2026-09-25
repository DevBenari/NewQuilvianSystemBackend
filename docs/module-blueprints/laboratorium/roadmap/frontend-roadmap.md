# Roadmap Delivery Frontend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `40` — gelombang `MVP-9` ditambahkan 2026-09-25. Sebelumnya `39` — gelombang `MVP-8` ditambahkan 2026-09-24. *Baris ini sempat tertinggal di `35` sementara riwayat sudah sampai `38`; dirapikan 2026-09-24* |
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

> **Diamandemen 2026-09-24 — decision log revisi 54.** Data induk yang **khusus Laboratorium**
> kini berada di `…/health-services/laboratory-management/master-data/` dan tampil sebagai
> sub-grup **Master Data** di menu Laboratorium: batas nilai, alasan penolakan, jenis specimen,
> organisme, antibiotik, breakpoint, profil Mikrobiologi katalog, parameter dan golongan PA,
> serta pengaturan disiplin. **Jenis pemeriksaan (`procedure`) dan tarif tetap di
> `health-services/master-data/`** karena dipakai modul lain. Tabel di atas mencatat letak
> semula; alamat lama diarahkan ke alamat baru lewat `redirects()` di `next.config.js`.

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
| 1 | `FE-LAB-10` ✅ | **Selesai 2026-09-16.** Kedua prasyaratnya, `BE-LAB-20` dan `BE-LAB-25`, sudah selesai sejak 2026-09-15 |
| 2 | `FE-LAB-11` ✅ | **Selesai 2026-09-17.** Keempat task backendnya selesai sejak 2026-09-15 |
| 3 | `FE-LAB-12` ✅ | **Selesai 2026-09-17.** Penahannya terangkat dan ditutup pada hari yang sama |

**Tidak ada siklus.** Jumlah pasangan prasyarat→task pada grafik sama dengan isi kolom
**Dependency** pada ketiga task di bawah.

> **Batas yang dilaporkan apa adanya.** Grafik ini mencakup `MVP-5a` saja. Kesembilan task
> frontend lama ditulis sebelum grafik urutan dependency menjadi kewajiban dan tidak memilikinya.
> Dicatat sebagai gap, bukan diturunkan ulang dari ingatan dokumen.

### `FE-LAB-10` ✅ — Pengelolaan jenis specimen dan daftar pantau `Lainnya`

> **Status: ✅ `SELESAI` — 2026-09-16.** Seluruh butir DoD terpenuhi dan **terbukti pada aplikasi
> yang benar-benar berjalan**, termasuk butir yang paling menentukan: tautan langsung ke layar
> kelola memuat barisnya lewat `GET /{id}` dan **nol memanggil jalur daftar**. Laporan lengkap
> beserta buktinya: [`task/report/frontend/FE-LAB-10.md`](../task/report/frontend/FE-LAB-10.md).
>
> **Dikerjakan justru karena ketiga penahan modul tidak menyentuhnya.** `LAB-SIGN-001`,
> `LAB-REQ-007`, dan `LAB-COORD-010` seluruhnya mengunci `S17` dan `S4`; task ini milik
> `MVP-5a`, kontraknya `r7`/`r8` terkunci sejak 2026-09-14, dan kedua dependency backendnya
> selesai sejak 2026-09-15.
>
> **Satu bagian verifikasi tidak dapat dijalankan, dan sebabnya ketiadaan data — bukan ketiadaan
> layar.** Skenario *"daftar pantau menampilkan cairan kista berjumlah tiga"* menuntut wadah
> berjenis `Lainnya` yang tercatat; `GET /other-usage` terhadap basis data sebenarnya
> mengembalikan `totalData 0`, karena jalur yang mengisinya adalah `FE-LAB-11` yang belum
> dikerjakan. Perilaku layarnya dibuktikan terpisah dengan jawaban yang dipasang.

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

### `FE-LAB-11` ✅ — Formulir Penerimaan Sampling/Specimen

> **Status: ✅ `SELESAI` — 2026-09-17.** Laporan:
> [`task/report/frontend/FE-LAB-11.md`](../task/report/frontend/FE-LAB-11.md).
> Lint bersih, build hijau, **1029/1029** uji unit, **6** pemeriksaan layar, dan **`AC-76`
> terbukti**: 12/12 uji layar lab lama lulus tanpa satu berkas pun disentuh.
>
> **Tiga pemeriksaan layar sengaja menguji ketiadaan** — nol kotak Jumlah/Qty, nol `select`
> metode pembayaran, dan tombol kelayakan yang tidak dapat ditekan beserta sebabnya. Ketiganya
> paling mudah lolos bila hanya tampilannya dilihat sepintas.
>
> **Satu butir DoD dicabut sebelum dikerjakan** — lihat kotak `⛔` di bawah.
>
> **Dua batas dilaporkan apa adanya:** wadah direncanakan pada pesanan **pertama** ketika
> pemeriksaannya melintasi beberapa disiplin, dan `BR-44` baru terpenuhi pada kalimatnya.
>
> **⚠ Satu temuan di luar cakupan, dan ia mendesak:** layar wadah lama `FE-LAB-07` **tidak lagi
> dapat merencanakan wadah** — `VAL-51` menolak `422` setiap permintaan tanpa `specimenTypeId`,
> dan layar itu nol mengirimnya, sejak migration `BE-LAB-21` diterapkan 2026-09-15. Perbaikannya
> kecil tetapi menuntut task tersendiri.

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
| **DoD** | Wilayah C dan D **terlihat berdampingan** sebelum kelayakan ditetapkan (`LAB-FE-010`); peringatan penguncian terlihat sebelum tombol kelayakan dapat ditekan; metode pembayaran **baca-saja tanpa kotak pilihan** dan menulis *belum dapat ditentukan* saat sumbernya tidak terjawab (`LAB-FE-011`); **nol kolom Jumlah/Qty dirender dan nol ruas `quantity` dikirim** (`LAB-DEC-050`); pembatalan pemeriksaan sesudah wadah `Layak` **mengatakan bahwa tagihannya tidak ikut batal** (`BR-44`); penamaan menu membedakan jalur ini dari pesanan dokter (`LAB-FE-009`); `AC-76` terbukti — uji layar lama lulus tanpa perubahan |

> **Dua wilayah sengaja kosong, dan itu harus terlihat oleh petugas.** Wilayah metode
> pembayaran menampilkan *belum dapat ditentukan*, bukan dikosongkan begitu saja. Wilayah
> instansi perujuk tetap memakai daftar terkendali — bila perujuknya belum terdaftar,
> pendaftaran ditolak `VAL-43` seperti hari ini. **Jalan buntunya belum hilang**, dan layar ini
> tidak boleh berpura-pura sudah hilang.

> ## ⛔ Butir Jumlah/Qty DICABUT — koreksi 2026-09-17
>
> Paragraf di tempat ini semula berbunyi: *"mengisi jumlah `3` menghasilkan tiga baris
> pemeriksaan setelah tersimpan, bukan satu baris bertuliskan '3'."* Butir itu **tidak berlaku**,
> dan butir DoD yang menuntutnya sudah diganti.
>
> **Ketiga sumber otoritatif sepakat, dan ketiganya bertanggal 2026-09-14** — hari yang sama task
> ini ditulis, sehingga besar kemungkinan ia tertulis tanpa memuat keputusan yang baru turun:
>
> | Sumber | Bunyi |
> |---|---|
> | `LAB-DEC-050`, `BR-45` | *"Kolom Jumlah tidak dibuat"*; `LAB-DEC-038` **dicabut** |
> | `LAB-API-v1` `r9` | Ruas `Quantity` pada `POST /lab-examinations` **tidak jadi dibuat** |
> | `03-frontend-architecture.md` §10.5 | Kolom Jumlah/Qty **tidak ada** — *"Tidak di layar, tidak di permintaan API"* |
>
> **Sebabnya bukan selera.** `BE-LAB-23` menemukan `LabExamination` memiliki index unik
> `(SpecimenId, ProcedureId)` di tingkat database, dipasang atas dasar `BR-20` dan `AC-35`. Dua
> baris untuk jenis pemeriksaan yang sama pada satu wadah **tidak mungkin ada**, sehingga
> "jumlah 3 menjadi tiga baris" akan ditolak database pada baris kedua.
>
> **Yang berlaku sebagai gantinya:** petugas yang memerlukan dua pemeriksaan memilih **dua butir
> katalog** yang berbeda; pengerjaan ganda satu pemeriksaan ditandai **`IsDuplo`**
> (`LAB-DEC-026`).

**Satu hal yang tetap wajib terlihat petugas (`BR-44`).** Membatalkan pemeriksaan sesudah wadah
dinyatakan `Layak` **tidak** serta-merta membatalkan tagihannya — kelayakan tagihnya sudah terbit
dan koreksinya dikerjakan Billing. Layar wajib mengatakan itu pada saat tombol batal ditekan,
bukan membiarkannya menjadi kejutan di loket.

### `FE-LAB-12` ✅ — Daftar dan detail penerimaan

> **Status: ✅ `SELESAI` — 2026-09-17**, pada hari yang sama ia sempat ditandai ⛔ `TERTAHAN`.
> Laporan: [`task/report/frontend/FE-LAB-12.md`](../task/report/frontend/FE-LAB-12.md).
> Lint bersih, build hijau, **1043/1043** uji unit, **4** pemeriksaan layar, dan **`AC-76`
> terbukti**: 18/18 spec Laboratorium lain lulus.
>
> **Dengan ini `MVP-5a` TUNTAS** — ketiga task frontendnya selesai.
>
> **Pemeriksaan yang paling mudah terlewat dibuktikan sengaja:** baris yang waktu tibanya tidak
> dicatat **ditandai terang-terangan**. Tanpa penanda itu, waktu pencatatan akan terbaca sebagai
> waktu kedatangan — persis kekeliruan yang `LAB-DEC-042` ada untuk mencegahnya, dan persis
> kekeliruan yang **tidak menimbulkan galat apa pun**.

> **Penahannya TERANGKAT 2026-09-17, pada hari yang sama ia ditemukan.** `r17` disetujui pemilik
> modul lalu dilaksanakan `BE-LAB-38`: `GET /lab-specimens` berdiri dan **terbukti menyaring pada
> waktu kedatangan sebenarnya**, dibuktikan dua arah. Ruas `orderNumber` yang ikut dibutuhkan juga
> sudah tersedia lewat `r16`/`BE-LAB-37`. **Nol penahan tersisa.**
>
> Catatan pertentangan di bawah **dipertahankan apa adanya**, bukan dihapus: urutan kejadiannya
> pantas dibaca ulang — celahnya ditemukan pada pemeriksaan pra-implementasi, sebelum satu baris
> pun ditulis, dan itulah kemunculan kelima pola yang sama pada modul ini.

> **Status: ⛔ `TERTAHAN` — 2026-09-17.** Dihentikan pada pemeriksaan pra-implementasi, **sebelum
> satu baris pun ditulis**. Penahannya bukan hambatan teknis: **jalur bacanya tidak ada**.
>
> **Yang dicari.** Cakupan task ini menuntut *"layar daftar penerimaan beserta penyaring rentang
> tanggal"* — daftar wadah **lintas pesanan**, disaring menurut waktu kedatangan sebenarnya.
>
> **Yang benar-benar tersedia**, dibaca langsung dari source pada backend `13665452`:
>
> | Jalur | Yang dikembalikan |
> |---|---|
> | `GET /lab-specimens/summary?startDate=&endDate=` | **Angka rekap saja** — `TotalWadah`, `Direncanakan`, `Diambil`, `Diterima`, `DinyatakanLayak`, `Ditolak`, `PerluAmbilUlang`, `Dibatalkan`, `Ditahan`. Nol baris |
> | `GET /lab-specimens/by-order/{labOrderId}` | Baris wadah untuk **satu pesanan** |
> | `GET /lab-specimens/by-order/{labOrderId}/history` | Riwayat satu pesanan |
> | `GET /lab-specimens/filters/metadata` | Keterangan bentuk layar |
>
> **Nol endpoint mengembalikan daftar wadah lintas pesanan.** Ditelusuri pula ke luar controller
> wadah: `LabMonitoringService` menyentuh `LabSpecimens` hanya untuk **mencacah**
> (`SpecimenCount`, `AcceptedSpecimenCount`) dan sebagai sub-query penyaring — ia memproyeksikan
> **pesanan**, bukan wadah. `LabWorklistService` nol menyentuhnya.
>
> **Setengah task ini justru sudah siap.** `GET /summary` menyaring tepat pada
> `PhysicallyReceivedAt ?? CreateDateTime`, dan komentarnya menyebut `AC-67` apa adanya — wadah
> yang tiba Senin 21.10 dan diregistrasi Selasa 08.05 **sudah** terhitung pada hari Senin. Yang
> hilang adalah barisnya, bukan aturannya.
>
> **Kenapa tidak diturunkan menjadi versi sebagian.** Layar yang menampilkan angka rekap tanpa
> baris yang diringkasnya menimbulkan pertanyaan yang tidak dapat dijawab layar itu sendiri —
> *"sembilan wadah diterima hari Senin, yang mana saja?"*. Keputusan menunda mengikuti preseden
> `FE-LAB-17`, yang ditunda pemilik modul alih-alih diturunkan menjadi versi sebagian.
>
> **Ini kemunculan KELIMA dari pola yang sama pada modul ini**, dan bentuknya bergantian:
>
> | # | Kejadian | Bentuk |
> |---:|---|---|
> | 1 | `MVP-5d` — ruas konfirmasi | Nilai **tersimpan** tanpa jalan keluar |
> | 2 | `MVP-5e` — `LabOrderedProcedure` | Tabel **ditulis** tanpa pembaca |
> | 3 | `BE-LAB-36` — nomor order | Kolom **berdiri** tanpa jalan keluar; `r16` masih menunggu |
> | 4 | `FE-LAB-19` — ruas wadah `r7` | Ruas **dituntut backend** tanpa penulis |
> | 5 | **`FE-LAB-12`** | Layar **dituntut roadmap** tanpa jalur baca |
>
> Penjaganya sama, dan sudah tertulis pada kontrak bagian 10.9: telusuri setiap ruas sampai
> **kedua** ujungnya sebelum task dimulai — bukan hanya ke ujung yang sedang dikerjakan.

#### Usul amandemen `LAB-API-v1` `r17` — **belum disetujui**

Satu endpoint baca baru pada grup Lab Specimen. **Aditif** — nol endpoint, ruas, nilai enum,
permission, maupun migration yang berubah.

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/lab-specimens` | Daftar penerimaan lintas pesanan, disaring rentang waktu kedatangan sebenarnya | `LabSpecimen : Read` |

**Penyaring:** `startDate`, `endDate`, `specimenStatus`, `search`, `pageNumber`, `pageSize` —
bentuk yang sama dengan `LabOrderPagedQuery` yang sudah berjalan sejak `r5`.

**Satu hal yang menentukan dan mudah keliru:** rentangnya wajib disaring pada
`PhysicallyReceivedAt ?? CreateDateTime`, **persis seperti `GetSummaryAsync` baris 1119-1120** —
bukan pada `CreateDateTime` saja. Bila keliru, seluruh guna kolom `PhysicallyReceivedAt` hilang
**tanpa satu pun kesalahan yang terlihat**: layarnya tetap tampil benar, hanya tanggalnya yang
salah. Itulah inti `LAB-DEC-042`, dan sudah ditulis sebagai peringatan pada DoD task ini sejak
2026-09-14.

**Ruas respons yang dibutuhkan layar**, di luar yang sudah ada pada `LabSpecimenResponse`:
`labOrderId`, `orderNumber` (**menunggu `r16`**), nama pasien, dan `createDateTime` — yang
terakhir supaya **selisih** terhadap waktu kedatangan dapat ditampilkan, sebagaimana dituntut DoD.

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

### `FE-LAB-18` ✅ — Penjaga tanggal pada penyaring Laboratorium

> **Status: ✅ `SELESAI` — 2026-09-16.** Seluruh butir DoD terpenuhi. Laporan:
> [`task/report/frontend/FE-LAB-18.md`](../task/report/frontend/FE-LAB-18.md).
>
> **Bukti: 999/999 uji unit lulus** — seluruh berkas uji dijalankan, bukan hanya milik
> Laboratorium, karena komponen yang disentuh dipakai bersama. Build produksi lulus.
> Lint **5 warning sebelum, 5 warning sesudah** — nol tambahan, dan kelimanya sudah ada
> pada baris yang tidak disentuh.
>
> **Butir DoD yang paling menentukan dibuktikan terbalik**, bukan diasumsikan: bahwa 132
> pemakai lain `FilterDatePicker` **tidak berubah perilakunya**. Tanpa prop `max`,
> `maxValue` bernilai kosong dan penjaganya hubung-singkat sebelum perbandingan apa pun.
> Tiga asersi menjaga sifat itu tetap benar.
>
> **Satu cacat dicegah sebelum sempat ada:** `todayDateValue()` menyusun tanggal dari
> komponen lokal, bukan `toISOString()`. Yang terakhir mengembalikan tanggal **kemarin**
> bagi pengguna WIB pada pukul 00:00–07:00 — persis jam petugas shift pagi mulai bekerja.
>
> **Dibuktikan juga pada aplikasi yang benar-benar berjalan**, sama seperti `FE-LAB-17`:
> 2 pemeriksaan layar Playwright lulus terhadap `.next/standalone`. `AC-99` diukur dengan
> **menghitung permintaan yang tiba**, karena satu-satunya cara membuktikan sesuatu tidak
> dikirim adalah menghitung yang sampai.
>
> **Dua kekeliruan uji dicatat di laporan, bukan disembunyikan:** pemicu pemilih tanggal
> adalah `button` ber-nama-aksesibel dan bukan `input` ber-`placeholder`, dan penyaring
> layar ini tidak sinkron dari URL. Keduanya membuat spec versi pertama gagal, dan keduanya
> kekeliruan uji — bukan cacat produk.

> **Dibuka 2026-09-16 oleh `LAB-DEC-074`, atas temuan `LAB-RDY-C02` pada audit kesiapan
> `LAB-RDY-001`.** Ini **satu-satunya task pada roadmap ini yang lahir dari audit**, bukan dari
> requirement baru — cacatnya sudah berjalan di layar yang dipakai petugas sejak `FE-LAB-09`.

| Butir | Isi |
|---|---|
| **Outcome** | Petugas tidak dapat memilih tanggal masa depan pada penyaring Laboratorium, dan tidak dapat menjalankan pencarian dengan rentang terbalik tanpa diberi tahu sebabnya |
| **Requirement/decision** | `FR-10.4`; `LAB-DEC-064`, `LAB-DEC-071`, `LAB-DEC-074` |
| **Kontrak** | **Nol perubahan kontrak.** Backend sudah inklusif pada kedua ujung (`>= mulai`, `<= sampai`); yang ditambahkan penjaga di sisi layar |
| **Cakupan** | Satu prop **opsional** `max` pada `FilterDatePicker`; satu aturan murni pembanding rentang pada `lab-monitoring-rules.js`; pemakaiannya pada tiga menu Pemeriksaan |
| **Dependency** | Nol. Tidak menunggu task backend mana pun |
| **Acceptance criteria** | `AC-98`, `AC-99` |
| **Kewenangan UI** | Hari yang melewati batas **tidak dapat diklik** dan tampil teredam, bukan disembunyikan — petugas perlu melihat bahwa tanggalnya ada tetapi tidak boleh dipilih. Tombol `Hari ini` nonaktif bila hari ini sendiri melewati batas. Rentang terbalik memunculkan alert berisi sebabnya, dan tombol Cari ditahan selama rentangnya masih terbalik |
| **Verifikasi** | Uji unit atas aturan murni pembanding rentang; uji unit atas penjaga `max` komponen; **pembuktian terbalik bahwa 132 pemakai lain nol terdampak** — komponen tanpa prop `max` wajib berperilaku persis seperti sebelumnya |
| **Risiko/pemilik** | **Sedang, dan bukan karena Laboratorium.** `FilterDatePicker` dipakai **133 berkas lintas modul**; satu kekeliruan pada default-nya menyentuh administrator, workforce, dan master data sekaligus. Pemilik: Laboratorium, dengan kehati-hatian pada komponen base |
| **DoD** | Tanggal masa depan tertolak di layar; rentang terbalik tertolak beserta sebabnya; komponen tanpa `max` terbukti tidak berubah perilakunya; seluruh uji Laboratorium tetap lulus |

### `FE-LAB-19` ✅ — Perbaikan: layar wadah mengirim jenis specimen

> **Status: ✅ `SELESAI` — 2026-09-17**, pada hari yang sama cacatnya ditemukan. Laporan:
> [`task/report/frontend/FE-LAB-19.md`](../task/report/frontend/FE-LAB-19.md).
> Lint bersih, build hijau, **1033/1033** uji unit, **18/18** pemeriksaan layar Laboratorium.
>
> **Aturan murninya dipakai ulang, bukan disalin** — `VAL-51`, `VAL-53`, dan `VAL-56` kini punya
> satu definisi yang dipakai kedua layar.
>
> **Satu uji lama sengaja dibalik, dan itu bukan regresi.** Uji `FE-LAB-07 (VAL-05)` semula
> menuntut `validatePlan({ examinations: ["a"] })` mengembalikan `{}` — ia **mengunci perilaku
> yang cacat**. Cacat ini memang lolos dari build, lint, dan uji sekaligus; yang menemukannya
> adalah penelusuran nama ruas pada seluruh `src`.

> **Dibuka 2026-09-17 atas temuan `FE-LAB-11`.** Ini **task perbaikan**, bukan requirement baru —
> dan bukan pula cacat yang lahir dari task mana pun. Ia lahir dari selisih antara backend yang
> maju dan frontend yang tidak ikut.

**Cacatnya sedang berjalan hari ini.** `BE-LAB-21` menambahkan lima ruas bahan wadah lewat
`LAB-API-v1` `r7`, dan `VAL-51` menolak **tanpa syarat** setiap wadah baru yang tidak membawa
`SpecimenTypeId`:

```csharp
if (specimenTypeId == Guid.Empty)
    throw new LabSpecimenValidationException("Pilih jenis specimen terlebih dahulu.");
```

Sementara itu `buildPlanPayload` pada `lab-specimen-rules.js` hanya mengirim dua ruas —
`examinations` dan `specimenDescription`. Akibatnya **layar wadah `FE-LAB-07` tidak lagi dapat
merencanakan wadah sama sekali**; setiap percobaan dijawab `422`. Berlaku sejak migration
`BE-LAB-21` diterapkan pada **2026-09-15**.

> **Ini bukan kejutan.** `traceability.md` revisi 18 sudah menuliskannya pada 2026-09-15 —
> *"`FE-LAB-11` berstatus mendesak karena layar wadah menjawab `422` sejak migration
> diterapkan"*. Yang belum pernah dikerjakan adalah **layar lamanya**; `FE-LAB-11` mendirikan
> layar baru dan sengaja tidak menyentuhnya, sesuai cakupannya.

| Butir | Isi |
|---|---|
| **Status** | `SIAP DIKERJAKAN` — ditulis 2026-09-17 |
| **Outcome** | Petugas dapat kembali merencanakan wadah dari layar pesanan, dan bahan yang dibawa wadah tercatat |
| **Requirement/decision** | `LAB-DEC-040`, `LAB-DEC-041`, `LAB-DEC-042`, `BR-35`; `VAL-51`..`VAL-58` |
| **Kontrak** | `LAB-API-v1` `r7` grup Lab Specimen — `approved`, terkunci. **Nol amandemen** |
| **Reuse** | **Aturan murni `lab-reception-rules.js` yang dibuat `FE-LAB-11` dipakai ulang apa adanya.** Menyalinnya akan melahirkan definisi kedua atas `VAL-51`, `VAL-53`, dan `VAL-56` — dan salinan itulah yang kelak bercabang |
| **Cakupan** | `buildPlanPayload` dan `validatePlan` meneruskan ruas bahan; daftar pilihan jenis specimen dimuat pada hook wadah; ruas wilayah C dirender pada formulir rencana |
| **Dependency** | `FE-LAB-11` ✅ — aturan murninya berasal dari sana |
| **Acceptance criteria** | `AC-58` bagian layar wadah; melengkapi `AC-62` dan `AC-64` |
| **Verifikasi** | Uji unit atas payload yang kini membawa `specimenTypeId`; **pembuktian terbalik bahwa `FE-LAB-11` nol terdampak**; seluruh uji Laboratorium tetap lulus |
| **Risiko/pemilik** | **Rendah pada kode, tinggi bila dibiarkan.** Perubahannya kecil dan terpusat; yang mahal adalah membiarkan layar yang dipakai petugas menolak setiap permintaan. Pemilik: Laboratorium |
| **DoD** | Perencanaan wadah dari layar pesanan berhasil kembali; jenis `Lainnya` menuntut keterangan; volume tanpa satuan tertolak di layar; aturan murninya **dipakai ulang, bukan disalin**; seluruh uji lama lulus |

## 6e. Task Gelombang `MVP-6` — Pengisian hasil Mikrobiologi dan Patologi Anatomi

Menurunkan `EPIC-LAB-13` dan [`03-frontend-architecture.md`](../03-frontend-architecture.md)
bagian 12. Kontrak `LAB-API-v1` **`r24`** `approved` 2026-09-18.

> **Ketiga task di bawah bergantung pada backend yang sudah berjalan**, bukan hanya pada kontrak
> yang disetujui. Layar yang dibangun terhadap endpoint yang belum ada tidak dapat diuji selain
> pada keadaan gagalnya.

### 6e.1 `FE-LAB-24` — Dua layar data induk: Organisme dan Antibiotik

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI` 2026-09-22** — [`FE-LAB-24.md`](../task/report/frontend/FE-LAB-24.md). Kedua layar berdiri: **30 berkas baru** pada 12 folder, ditambah dua registrasi (`store.jsx`, `menu-items.jsx`). `AC-116`, `AC-117`, dan `AC-118` **terbukti**. Lint 0 error 4 warning — aturan dan jumlah **sama persis** dengan kedua modul rujukan, jadi nol kelas peringatan baru. 30 uji unit baru lulus; suite naik 1556→1586 lulus dengan jumlah gagal **tetap 6**, keenamnya sudah ada sebelumnya dan nol menyentuh Laboratorium. Build hijau, kedelapan route terkompilasi. **Satu butir verifikasi belum tercapai dan disebut apa adanya:** layar belum dijalankan terhadap backend berisi data — backend gagal start karena `Jwt:Key` belum dikonfigurasi di environment ini, dan nilai itu **nol ditebak maupun diisi** dari task ini. *(Sebelum hari ini blok ini tertinggal `MENUNGGU BE-LAB-44` padahal `BE-LAB-44` selesai 2026-09-18.)* **Kedua penahan backend gugur:** `BE-LAB-44` ✅ mendirikan tabel dan empat endpoint per grup, dan **`BE-LAB-65` ✅** (2026-09-22) melengkapinya menjadi **delapan** — `filters/metadata`, `summary`, `GET /{id}`, dan `PATCH {id}/status`. Tanpa `BE-LAB-65`, kedua layar nol dapat dibangun sesuai `master-data-feature-standard`: formulir ubahnya nol punya jalur memuat baris, dan gagalnya diam. **`BE-LAB-65` juga membuka `discContentUg`**, sehingga layar Antibiotik dapat menurunkan `missingDiscContent` yang sebelumnya nol punya jalan diisi |
| **Tiga hal yang ditemukan saat dikerjakan** | **1. Satu jebakan backend ditutup di layar.** `UpdateLabOrganismRequest.IsActive` dan kembarannya bernilai `true` ketika permintaannya nol memuat ruas itu, sementara `UpdateAsync` menyetel `entity.IsActive` **tanpa syarat**. Formulir ubah yang nol mengirim status karena itu akan **menghidupkan kembali kuman yang sudah ditarik** hanya karena keterangannya disunting — nol galat, nol pesan. Ruas `Status Aktif` karena itu wajib ada pada formulir ubah, nilainya dibaca apa adanya dari barisnya, dan dua uji mengunci perilakunya. **2. Satu cacat warisan ditemukan lewat uji yang ditulis lebih dulu.** Pemeriksaan panjang maksimal pada `validateForm` modul rujukan **nol pernah menyala** — nilainya dipotong `safeInputString` lebih dulu, sehingga kode yang kepanjangan tersimpan DIAM dalam bentuk terpotong. Diperbaiki pada kedua berkas baru; `lab-specimen-types` dan `lab-susceptibility-breakpoint` **sengaja nol disentuh** dan cacatnya dilaporkan, bukan ditambal diam-diam. **3. Dua penyimpangan dari `master-data-feature-standard` dicatat terbuka**, bukan didiamkan: nol thunk `/options` (alamatnya sudah terdaftar di registry pilihan sejak `FE-LAB-34`, dan menuliskannya dua kali persis yang dilarang komentar di sana), dan satu CSS Module berisi **satu kelas** `.rowActions` — sebab layar tanpa penghapusan menaruh penonaktifan di kolom aksi, sementara bagian 7.2 melarangnya di halaman detail |
| **Outcome** | Kepala instalasi dapat mengelola daftar organisme dan panel antibiotik dari aplikasi |
| **Requirement/decision** | `FR-13.6`; `LAB-DEC-084`; `LAB-FE-014` |
| **Kontrak** | `LAB-API-v1` `r24` bagian 19.4, **dilengkapi amandemen `r31` bagian 26** `approved` 2026-09-22 — empat endpoint baseline sisanya dan ruas `discContentUg` |
| **Reuse** | **Dua modul rujukan, dan yang utama bukan yang tertulis di sini semula.** `lab-susceptibility-breakpoint` dipakai sebagai bentuk induk — ia yang paling mutakhir, punya halaman detail, dan nol CSS Module. `lab-specimen-types` dipakai untuk satu hal yang nol dimiliki breakpoint: pola data induk Laboratorium **tanpa penghapusan**, termasuk letak aksi penonaktifan di kolom aksi daftar |
| **Cakupan** | *Rencana:* dua route di `health-services/master-data/`, dua komponen tampilan, dua API service, dua potongan konstanta. **Yang benar-benar dibangun lebih besar, dan alasannya bukan pelebaran scope melainkan bentuk wajib data induk:** `master-data-feature-standard.md` menetapkan satu fitur data induk = **tujuh berkas source ditambah dua registrasi**, dan "dua API service" pada rencana ternyata berbentuk dua potongan Redux beserta hook daftar, detail, dan formulirnya. Hasilnya **2 × 14 berkas** — 2 konstanta, 2 slice, 2 utils, 6 hook, 6 tampilan, 10 route — ditambah 1 CSS Module satu kelas, 1 berkas uji, dan 2 registrasi |
| **Dependency** | ✅ Terpenuhi — `BE-LAB-44` selesai 2026-09-18, `BE-LAB-65` selesai 2026-09-22 |
| **Acceptance criteria** | `AC-116` ✅ keduanya berada di `master-data/`, **bukan** di folder Laboratorium (`LAB-FE-014`); `AC-117` ✅ **nol tombol Hapus** — hanya penonaktifan; `AC-118` ✅ baris nonaktif tetap terlihat dengan penanda, tidak hilang dari daftar |
| **Verifikasi** | Uji unit atas komponen daftar dan formulir ✅ **30 uji lulus**; layar dijalankan terhadap backend yang sudah berdiri ⚠ **belum** — backend gagal start karena `Jwt:Key` belum dikonfigurasi di environment ini. Yang dapat dibuktikan tanpa backend sudah dibuktikan: kedelapan route menjawab `200` dengan judul halaman yang benar, dan penjaga kata cadangan pada `[slug]` bekerja |
| **Risiko/pemilik** | Rendah. Polanya sudah terbukti pada `lab-specimen-types` |
| **DoD** | Kedua layar berjalan ✅ *(pada sisi kode; belum diklik di peramban)*; `AC-116`..`AC-118` ✅ terbukti; seluruh uji Laboratorium ✅ tetap lulus — nol kegagalan baru |

### 6e.2 `FE-LAB-25` — Layar pengisian laporan Patologi Anatomi

| Butir | Isi |
|---|---|
| **Status** | 🧊 **`DIBEKUKAN` 2026-09-18** — sebelumnya `MENUNGGU BE-LAB-46`. Dibekukan bersama `BE-LAB-46` oleh rekonsiliasi bukti putaran 4 (`LAB-EVD-003`): bentuk hasil PA ternyata bergantung kategori dan IHK sendirian punya **10 ruas**, sedangkan layar ini dirancang untuk **tiga**. Ditambah `Status Hasil PA`, HL7, dua waktu diagnostik, Draft/Final/Reopen, cetak bilingual, dan konfirmasi kritis yang seluruhnya belum pernah masuk rancangan layar ini. Lihat [`05-evidence-reconciliation.md`](../05-evidence-reconciliation.md) bagian 12 |
| **Outcome** | Patolog dapat mengisi makroskopik, mikroskopik, dan kesimpulan dari satu layar, lalu menyimpannya |
| **Requirement/decision** | `FR-13.5`; `LAB-FE-016`, `LAB-FE-018`, `LAB-FE-021` |
| **Kontrak** | `LAB-API-v1` `r24` bagian 19.3 |
| **Reuse** | Layar pengisian hasil Patologi Klinik (`FE-LAB-23`) — jalan masuk dari baris menu `Pemeriksaan`, pola muat dan simpan |
| **Cakupan** | Satu route anak `.../anatomic-pathology/[slug]/result`, satu komponen tampilan, satu API service |
| **Dependency** | `BE-LAB-46` |
| **Acceptance criteria** | `AC-119` ketiga ruas bertanda **wajib sebelum tombol ditekan**, bukan sebagai galat sesudahnya (`LAB-FE-018`); `AC-120` **nol tombol Validasi maupun Rilis** (`LAB-FE-016`); `AC-121` **nol tombol Simpan draft**; `AC-122` **nol lencana status hasil** di mana pun pada layar ini (`LAB-FE-015`) |
| **Verifikasi** | Uji unit; pembuktian **terbalik** untuk `AC-120`, `AC-121`, dan `AC-122` — ketiadaan elemen itu diuji, bukan diasumsikan |
| **Risiko/pemilik** | **Rendah pada kode, sedang pada godaan.** Tombol Rilis terasa melengkapi layar, dan itu sebabnya ketiadaannya diuji |
| **DoD** | Layar berjalan; `AC-119`..`AC-122` terbukti; ketiga ruas narasi **tidak muncul** pada layar non-klinis mana pun |

### 6e.3 `FE-LAB-26` — Layar pengisian hasil Mikrobiologi

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SEBAGIAN BESAR DIGANTIKAN` `FE-LAB-30`..`FE-LAB-33`** — *(diperiksa 2026-09-22; sebelumnya `MENUNGGU BE-LAB-48`, padahal `BE-LAB-48` selesai 2026-09-21)*. **Layarnya sudah dibangun**, tetapi terhadap `r26`/`r27`, bukan `r24` bagian 19.2. **JANGAN dibatalkan begitu saja:** pemeriksaan satu per satu menemukan **dua acceptance criteria yang benar-benar belum tertutup** — lihat baris di bawah. Sisanya (`AC-123`, `AC-124`, `AC-125`) sudah terpenuhi |
| **Dua AC yang sempat terbuka — DITUTUP 2026-09-22** | **`AC-126`** ✅ `Hapus Isolat` kini melewati `ConfirmModal`, dan konfirmasinya **menyebutkan akibatnya**: nama kuman beserta jumlah baris antibiotik yang ikut terbuang. Pertanyaan yang nol menyebutkan akibatnya dijawab "ya" tanpa dibaca. **`AC-127`** ✅ daftar kuman kosong kini memunculkan peringatan yang menyebut **penyebabnya** (data induk belum diisi) dan **siapa yang mengisinya** (kepala instalasi, lewat Data Master → Organisme Mikrobiologi); tombol `Tambah Isolat` ikut dinonaktifkan selama daftarnya kosong. Keduanya melekat pada `lab-microbiology-result-form.jsx` yang sudah ada — **nol layar dibangun ulang**. Lint 0 error 0 warning, build hijau, suite 1556/1562 tanpa tambahan kegagalan. **Belum diklik di peramban** |
| **Sisa** | Nol pekerjaan kode. Blok ini dapat ditandai `DIGANTIKAN` penuh begitu `AC-126` dan `AC-127` terlihat bekerja di layar |
| **Outcome** | Analis dapat mencatat status temuan, menambah isolat, dan mengisi kepekaan antibiotik per isolat dari satu layar |
| **Requirement/decision** | `FR-13.1`..`FR-13.4`; `LAB-FE-015`..`LAB-FE-017`, `LAB-FE-019`, `LAB-FE-020` |
| **Kontrak** | `LAB-API-v1` `r24` bagian 19.2 |
| **Reuse** | Pola daftar bersarang yang dapat ditambah-kurangi; pemilih ber-`options` seperti pada jenis specimen |
| **Cakupan** | Satu route anak `.../microbiology/[slug]/result`, satu komponen tampilan dengan daftar bersarang, satu API service |
| **Dependency** | `BE-LAB-48`; **dan daftar organisme serta antibiotik sudah terisi** — lihat `backend-roadmap.md` 6i.7 |
| **Acceptance criteria** | `AC-123` isolat dan kepekaannya **terlihat bersarang**, bukan dua daftar sejajar (`LAB-FE-019`); `AC-124` organisme dan antibiotik **dipilih dari daftar**, nol isian teks bebas (`LAB-FE-017`); `AC-125` menyimpan **tanpa satu pun isolat** berhasil (`FR-13.4`); `AC-126` penghapusan baris isolat **meminta konfirmasi** (`LAB-FE-020`); `AC-127` daftar organisme kosong menampilkan **keterangan penyebab dan siapa yang mengisinya**, bukan pemilih kosong |
| **Verifikasi** | Uji unit termasuk keadaan daftar induk kosong; layar dijalankan terhadap backend berisi data |
| **Risiko/pemilik** | **Sedang.** `AC-123` bukan soal selera: dua daftar sejajar membuat petugas mengira antibiotik diuji terhadap *pemeriksaan*, bukan terhadap kuman tertentu — dan salah baca itu berujung pada terapi yang keliru |
| **DoD** | Layar berjalan; `AC-123`..`AC-127` terbukti; seluruh uji Laboratorium tetap lulus |

### 6e.3b `FE-LAB-25` ❌ **DIBATALKAN** — digantikan `FE-LAB-27`, `FE-LAB-28`, `FE-LAB-29`

> Layar tiga area teks atas hasil per **pemeriksaan** tidak lagi punya dasar: `LAB-DEC-085`
> memindahkan laporan PA ke tingkat **pesanan**, dan `LAB-DEC-086` mengganti ruas tetap dengan
> formulir yang **dibangkitkan dari data induk parameter**. **Nol baris kode terbuang.**

### 6e.5 Gelombang `MVP-6b1` dan `MVP-6b2` — Patologi Anatomi sesudah dirancang ulang

Menurunkan `03-frontend-architecture.md` bagian 13. Kontrak `r25` **`approved` 2026-09-18**,
bersama `LAB-VAL-v1` `r8` dan `LAB-PERM-v1` rev 7.

> **Persetujuan kontrak nol menggerakkan status ketiga task di bawah, dan itu benar.** Penahan
> mereka tidak pernah kontraknya, melainkan **endpoint yang belum ada di kode**. `FE-LAB-27`
> tetap menunggu `BE-LAB-50`, `FE-LAB-28` menunggu `BE-LAB-52`, dan `FE-LAB-29` menunggu
> `BE-LAB-52`. Label "Rencana (belum tersedia)" pada tabel endpoint `r25` menyatakan persis itu.

#### `FE-LAB-27` — Dua layar data induk Patologi Anatomi

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI` 2026-09-23** — ketiga layar terbangun (**37 berkas**: dua fitur data induk penuh, keberlakuan ruas, dan penggolongan jenis pemeriksaan), 10 uji unit baru lulus, lint 0 error, build hijau, **9 route terkompilasi**. `AC-143` ✅ terbukti; `AC-144` dan `AC-145` terbukti pada lapis aturan, **belum diklik di peramban**. Lihat [`FE-LAB-27.md`](../task/report/frontend/FE-LAB-27.md). *(Riwayat hari itu:)* ✅ **`SIAP DIKERJAKAN` — nol penahan tersisa, 2026-09-23.** `BE-LAB-66` selesai dan **izin jabatan Kepala Instalasi sudah diberikan**; kesebelas endpoint terbukti `200` bagi akun yang akan memakai layarnya, dan ketiga `summary` mengembalikan angka sungguhan. **Dua ketiadaan yang disengaja pada grup pemetaan dibaca dari metadata**, bukan disimpulkan: `supportsStatusToggle: false` dan `hasOptionsEndpoint: false`. *(Riwayat hari itu juga:)* ⛔ **`TERTAHAN`** oleh `BE-LAB-66` — *(dikoreksi 2026-09-23; sebelumnya ✅ `SIAP DIKERJAKAN`)*. Gelombang `MVP-6b1`. `BE-LAB-50` memang **selesai 2026-09-18**, tetapi kesepuluh endpoint yang berdiri **bukan permukaan yang dituntut `master-data-feature-standard`**: ketiga grup Patologi Anatomi nol punya `filters/metadata`, `summary`, maupun `GET /{id}`, dan kedua grup berstatus nol punya `PATCH /{id}/status`. **Ini penahan yang sudah diketahui sejak 2026-09-22 dan sengaja dicatat, bukan temuan baru** — `BE-LAB-65` bagian 6ad.1 menyapu kedua puluh dua controller Laboratorium, menemukan gapnya, dan meninggalkan ketiga grup PA justru supaya task ini nol tersandung diam-diam. **Persis kelas yang sudah dua kali menahan task frontend:** `FE-LAB-24` menunggu `BE-LAB-65`, `FE-LAB-34` menunggu `BE-LAB-64`. **`BE-LAB-66` dibuka 2026-09-23** untuk menutupnya — [`backend-roadmap.md`](backend-roadmap.md) bagian 6ae — dan penahan task itu sendiri tunggal serta bukan teknis: amandemen `LAB-API-v1` `r32` belum disetujui |
| **Outcome** | Kepala instalasi dapat mengelola parameter, kategori, keberlakuan, dan **pemetaan jenis pemeriksaan** |
| **Requirement/decision** | `FR-13.11`; `LAB-FE-014` |
| **Reuse** | Layar `lab-organisms`/`lab-antibiotics` dari `FE-LAB-24`, dan `lab-specimen-types` |
| **Cakupan** | Dua route di `master-data/`, dua komponen tampilan, dua API service. Layar kategori memuat **tiga hal**: kategori, keberlakuan parameter, dan pemetaan pemeriksaan |
| **Dependency** | ✅ **Terpenuhi seluruhnya 2026-09-23** — `BE-LAB-50` dan `BE-LAB-66` keduanya selesai, **dan izin jabatan Kepala Instalasi sudah diberikan** sehingga kesebelas endpoint menjawab `200` bagi akun yang akan memakai layarnya. *(Riwayat:)* `BE-LAB-50` ✅ selesai 2026-09-18 — **dan `BE-LAB-66` 🕐 belum**, dibuka 2026-09-23. Tanpa yang kedua, ketiga layar nol dapat dibangun sesuai `master-data-feature-standard`: kartu ringkasan, penyaring berbentuk-dari-server, dan sakelar aktif per baris nol punya sumber, sementara formulir ubah yang dibuka lewat tautan langsung **gagal diam** karena nol ada `GET /{id}` |
| **Acceptance criteria** | `AC-143` **nol tombol Hapus**; `AC-144` layar pemetaan menyediakan **usulan dari `GET /suggestions`** yang **wajib dikonfirmasi manusia** sebelum disimpan; `AC-145` layar keberlakuan menampilkan penanda **wajib** per pasangan parameter-kategori |
| **Verifikasi** | Uji unit; dijalankan terhadap backend berisi data |
| **Risiko/pemilik** | Sedang. `AC-144` mudah disederhanakan menjadi tombol "terapkan semua" — dan itu menghapus pemeriksaan manusianya |
| **DoD** | Kedua layar berjalan; `AC-143`..`AC-145` terbukti |

#### `FE-LAB-28` — Layar laporan Patologi Anatomi

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI` 2026-09-23** — layar laporan berdiri (wilayah A–F), 15 uji unit lulus, lint 0 error, build hijau. **Aturan murninya dijalankan terhadap jawaban server sungguhan**: pesanan empat golongan membangkitkan **15 ruas dengan nol duplikat**, pesanan satu golongan **3 ruas**. `Simpan`/`Selesaikan`/`Buka Kembali` **sengaja nol dijalankan** terhadap pesanan pasien sungguhan. **Penahannya ditutup lebih dulu:** keenam pemeriksaan yang tersisa digolongkan atas instruksi pemilik modul — **10/10 terpetakan** — sebagai **data uji yang menunggu tinjauan `DR-LAB-003`**, terutama LBC dan FNAB. Satu cacat `FE-LAB-31` ikut ditemukan dan diperbaiki. Lihat [`FE-LAB-28.md`](../task/report/frontend/FE-LAB-28.md). *(Riwayat:)* ⚠ `MENUNGGU PEMETAAN TERISI` — gelombang `MVP-6b2`. `BE-LAB-52` **selesai 2026-09-18**, sehingga penahan teknisnya gugur. **Yang tersisa BUKAN pekerjaan programmer**: empat dari sepuluh pemeriksaan PA sudah dipetakan sebagai data uji dan **perlu ditinjau kepala instalasi**, enam sisanya belum |
| **Outcome** | Patolog dapat mengisi laporan sesuai kategori pesanan, menyelesaikannya, dan membukanya kembali |
| **Requirement/decision** | `FR-13.12`..`FR-13.17`; `LAB-FE-022`..`LAB-FE-029` |
| **Reuse** | Pola formulir dinamis; pola muat-simpan `FE-LAB-23` |
| **Cakupan** | Satu route anak `.../anatomic-pathology/[slug]/pathology-report`, satu komponen tampilan berformulir **dibangkitkan**, satu API service |
| **Dependency** | `BE-LAB-52`; **dan pemetaan jenis pemeriksaan sudah terisi** |
| **Acceptance criteria** | `AC-146` formulir **dibangkitkan dari daftar parameter server**, nol daftar ruas ditulis di kode (`LAB-FE-022`); `AC-147` **nol tombol Validasi/Rilis/Kirim** (`LAB-FE-023`); `AC-148` `Selesaikan` **terpisah** dari `Simpan` dan disertai penegasan ia bukan rilis (`LAB-FE-024`); `AC-149` konteks klinis **baca-saja** (`LAB-FE-025`); `AC-150` Waktu Efektif dan Issued **baca-saja bertanda turunan**, nol kotak isian (`LAB-FE-026`); `AC-151` `Buka Kembali` **meminta alasan** (`LAB-FE-028`); `AC-152` pesanan tanpa pemetaan menampilkan **sebab dan siapa yang mengatur**, bukan formulir kosong |
| **Verifikasi** | Uji unit termasuk keadaan **belum dipetakan** dan pesanan **dua kategori**; pembuktian terbalik untuk `AC-147` dan `AC-150` |
| **Risiko/pemilik** | **Tinggi untuk `AC-146`.** Menuliskan lima belas ruas di kode terasa lebih cepat dan membatalkan seluruh manfaat `LAB-DEC-086` |
| **DoD** | Layar berjalan; `AC-146`..`AC-152` terbukti; isi laporan **tidak muncul** pada layar non-klinis |

#### `FE-LAB-29` — Konteks klinis pada layar pemesanan

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI` 2026-09-23** — bagian konteks klinis berdiri pada layar pesanan, 9 uji unit lulus, lint 0 error, build hijau. `AC-154` dibuktikan **terbalik** pada lapis aturan. **Belum diklik di peramban**, dan **satu keputusan penempatan perlu dikonfirmasi pemilik modul** — lihat [`FE-LAB-29.md`](../task/report/frontend/FE-LAB-29.md) bagian 7. *(Riwayat:)* ✅ `SIAP DIKERJAKAN` — gelombang `MVP-6b2`. `BE-LAB-52` **selesai 2026-09-18**; kedua jalur konteks klinis berjalan dan berhak akses `LabOrder`, bukan `LabExamination` |
| **Outcome** | Dokter pemesan dapat menulis empat ruas konteks klinis saat memesan pemeriksaan PA |
| **Requirement/decision** | `FR-13.10`; `LAB-DEC-091`, `INV-40` |
| **Reuse** | Layar pemesanan yang **sudah berjalan** |
| **Cakupan** | Satu bagian tambahan pada layar pemesanan, satu API service |
| **Dependency** | `BE-LAB-52` |
| **Acceptance criteria** | `AC-153` bagian ini **hanya muncul** bila disiplin pesanannya Patologi Anatomi; `AC-154` seluruh ruasnya **opsional**, sehingga alur pemesanan disiplin lain **nol berubah** — dibuktikan **terbalik**; `AC-155` `Masa Terakhir Haid` ditandai hanya bermakna bagi sitologi ginekologi |
| **Verifikasi** | Uji unit; **pembuktian terbalik bahwa pemesanan Patologi Klinik dan Mikrobiologi nol terdampak** |
| **Risiko/pemilik** | **Tinggi pada dampaknya, rendah pada kodenya.** Ini menyentuh layar yang **sedang dipakai petugas** (`ARCH-GAP-LAB-07`) — kelas yang sama dengan `BE-LAB-21` yang sempat membuat layar wadah menjawab `422` |
| **DoD** | Bagian ini berjalan; `AC-153`..`AC-155` terbukti; **seluruh uji pemesanan lama tetap lulus** |

### 6e.4 Yang sengaja **tidak** menjadi task frontend

| Yang dipertimbangkan | Kenapa tidak |
|---|---|
| Tombol Validasi dan Rilis | `DEC-LAB-011` belum dijawab. Tombol yang muncul lalu selalu gagal lebih buruk daripada tombol yang tidak ada |
| Lencana status hasil pada daftar `Pemeriksaan` | `LAB-FE-015`. Nol status hasil disimpan; menampilkannya melahirkan status itu di kepala pengguna |
| Unggah gambar pada laporan Patologi Anatomi | `DEC-LAB-016` belum dijawab |
| Penanda nilai kritis pada hasil Mikrobiologi | `INV-28` |
| Layar antibiogram | Laporan, bukan bagian pengisian hasil |

---

## 7. Layar yang Sengaja Tidak Dibuat

Kelima layar berikut **tidak boleh** dibangun lebih dulu "sekalian", karena perilakunya belum
diputuskan:

| Layar | Penahan |
|---|---|
| Pengisian dan validasi hasil | Slice `S4` — ~~`LAB-SIGN-001`~~ ✅ **ditutup 2026-09-17** (`LAB-DEC-079`). **Layarnya tetap belum boleh dibangun**: yang terbuka izin merancang, dan arsitektur domain, amandemen kontrak, serta task backendnya **nol ada**. Pengisian hasil sendiri sudah berjalan sejak `S4a` lewat `FE-LAB-23` |
| Daftar pantau nilai kritis dan formulir pelaporan | Slice `S5` — ~~`LAB-SIGN-001`~~ ✅ ditutup, **tetapi `LAB-P0-004` dan `LAB-OPEN-014` tetap menahan**. Kerangkanya disahkan, daftar nilai kritisnya belum ada |
| Layar koreksi hasil | Slice `S6` — ~~`LAB-SIGN-001`~~ ✅ ditutup, **tetapi `LAB-P0-003` tetap menahan** |
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
| `FE-LAB-12` ✅ | `MVP-5a` | `EPIC-LAB-11` | `FE-LAB-11` ✅, `BE-LAB-37` ✅, `BE-LAB-38` ✅ | **`SELESAI`** — 2026-09-17, pada hari yang sama ia sempat ⛔ `TERTAHAN`. Dua layar berdiri: daftar penerimaan berpenyaring rentang **waktu kedatangan sebenarnya** beserta kolom Selisih, dan detail satu penerimaan yang menampilkan **setiap pemeriksaan sebagai barisnya sendiri**. Lint bersih, build hijau, 10 uji unit baru, **1043/1043** uji repository lolos, 4 pemeriksaan layar, dan **`AC-76` terbukti** — 18/18 spec Laboratorium lain lulus — [laporan](../task/report/frontend/FE-LAB-12.md). **`MVP-5a` TUNTAS** — 2026-09-17, dihentikan pada pemeriksaan pra-implementasi **sebelum satu baris pun ditulis**. **Nol endpoint mengembalikan daftar wadah lintas pesanan:** yang ada hanya `GET /summary` (angka rekap, nol baris) dan `GET /by-order/{id}` (satu pesanan). Ditelusuri pula ke `LabMonitoringService`, yang menyentuh wadah hanya untuk **mencacah**. **Setengah task ini justru sudah siap** — `GET /summary` menyaring tepat pada `PhysicallyReceivedAt ?? CreateDateTime` dan `AC-67` sudah terpenuhi di sana; yang hilang barisnya, bukan aturannya | **Kemunculan kelima pola yang sama.** Sengaja **tidak** diturunkan menjadi versi sebagian: layar berisi angka rekap tanpa baris yang diringkasnya menimbulkan pertanyaan yang tidak dapat dijawabnya sendiri. **Usul `LAB-API-v1` `r17`** ditulis lengkap pada bagian 6b — satu endpoint baca, aditif, nol migration — dan **menunggu persetujuan pemilik modul**. Satu ruasnya, `orderNumber`, ikut menunggu `r16` |
| `FE-LAB-18` ✅ | — audit | — | Nol | **`SELESAI`** — 2026-09-16. Penjaga tanggal pada penyaring Laboratorium: hari melewati batas tidak dapat diklik, rentang terbalik memunculkan sebabnya dan ditahan. **Satu-satunya task yang lahir dari audit**, bukan dari requirement — cacatnya sudah berjalan sejak `FE-LAB-09`. Prop `max` bersifat **opsional** dan 132 pemakai lain terbukti nol terdampak — [laporan](../task/report/frontend/FE-LAB-18.md) | Tidak ada. Batas jujur: nol verifikasi manual pada aplikasi berjalan saat itu; kemudian tertutup oleh 2 pemeriksaan layar |
| `FE-LAB-19` ✅ | — perbaikan | `S2` | `FE-LAB-11` ✅ | **`SELESAI`** — 2026-09-17, pada hari yang sama cacatnya ditemukan. Layar wadah `FE-LAB-07` **tidak lagi dapat merencanakan wadah sama sekali** sejak migration `BE-LAB-21` diterapkan 2026-09-15: `VAL-51` menolak `422` setiap permintaan tanpa `specimenTypeId`, dan `buildPlanPayload` nol mengirimnya. Kelima ruas bahan `r7` kini diteruskan, dan aturan murninya **dipakai ulang dari `FE-LAB-11`, bukan disalin** — [laporan](../task/report/frontend/FE-LAB-19.md) | **Cacatnya lolos dari build, lint, dan uji sekaligus.** Uji yang ada justru **mengunci perilaku cacatnya**; ia sengaja dibalik, dan itu dicatat sebagai perubahan perilaku yang disengaja, bukan uji yang diperbaiki agar lulus. Batas: pembuktian ujung-ke-ujung menunggu data pesanan pada basis data bersama |
| `FE-LAB-11` ✅ | `MVP-5a` | `EPIC-LAB-11` | `BE-LAB-21` ✅, `BE-LAB-22` ✅, `BE-LAB-23` ✅, `BE-LAB-24` ✅ | **`SELESAI`** — 2026-09-17. Layar terbesar modul ini berdiri: lima wilayah A–E pada satu halaman, merangkai **lima panggilan** dari pendaftaran sampai penetapan kelayakan. Lint bersih, build hijau, 15 uji unit baru, **1029/1029** uji repository lolos, 6 pemeriksaan layar — dan **`AC-76` terbukti**: 12/12 uji layar lab lama lulus **tanpa satu berkas pun disentuh**. Dua komponen dipakai ulang tanpa dipindahkan; `LabCatalogPicker` sengaja dipakai sebagai **komponen** karena memanggil hook-nya lagi akan melahirkan instance kedua yang state-nya terpisah — [laporan](../task/report/frontend/FE-LAB-11.md) | **Satu butir DoD dicabut sebelum dikerjakan:** kolom Jumlah/Qty bertentangan dengan `LAB-DEC-050`, `r9`, dan arsitektur §10.5; index unik `(SpecimenId, ProcedureId)` membuat "jumlah 3 → tiga baris" ditolak database pada baris kedua. **Dua batas:** wadah direncanakan pada pesanan **pertama** saat pemeriksaan melintasi beberapa disiplin — membentuk beberapa wadah sekaligus adalah keputusan yang belum diambil siapa pun; dan `BR-44` baru terpenuhi pada kalimatnya. **⚠ Temuan mendesak di luar cakupan:** `FE-LAB-07` **tidak lagi dapat merencanakan wadah** — `VAL-51` menolak `422` tanpa `specimenTypeId`, sejak migration `BE-LAB-21` diterapkan |
| `FE-LAB-10` ✅ | `MVP-5a` | `EPIC-LAB-11` | `BE-LAB-20` ✅, `BE-LAB-25` ✅ | **`SELESAI`** — 2026-09-16. Dua layar berdiri di `health-services/master-data/lab-specimen-types/`: pengelolaan jenis specimen beserta formulirnya, dan daftar pantau pemakaian `Lainnya`. Kelima butir DoD **terbukti pada aplikasi yang benar-benar berjalan** — termasuk yang paling menentukan, **tautan langsung ke layar kelola memuat barisnya lewat `GET /{id}` dan nol memanggil jalur daftar**, diukur dengan menghitung permintaan yang tiba. Lint bersih, build produksi hijau, 15 uji unit baru, 1014/1014 uji repository lolos, 7 pemeriksaan layar — [laporan](../task/report/frontend/FE-LAB-10.md). **`AC-49` terpenuhi penuh; `AC-58` dan `AC-60` terpenuhi pada bagian yang memang milik task ini** | **Nol penahan.** Ketiga penahan modul — `LAB-SIGN-001`, `LAB-REQ-007`, `LAB-COORD-010` — seluruhnya mengunci `S17` dan `S4`, dan **nol menyentuh gelombang ini**. **Satu batas disebut apa adanya:** skenario *"cairan kista berjumlah tiga"* **tidak dapat dijalankan** — `GET /other-usage` terhadap basis data sebenarnya mengembalikan `totalData 0`, karena wadah berjenis `Lainnya` baru lahir dari `FE-LAB-11`. Ketiadaan **data**, bukan ketiadaan layar. **Satu thunk sengaja berdiri tanpa pembaca:** `GET /options`, yang konsumennya `FE-LAB-11` |

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
|---:|---|---|---|
| 40 | 2026-09-25 | **Gelombang `MVP-9` diturunkan dari `EPIC-LAB-15` — tiga task frontend `FE-LAB-38`..`FE-LAB-40`** (bagian akhir dokumen), di atas kontrak `r34`, `r12`, rev 11, dan `r5` yang disetujui pemilik modul pada hari yang sama. `FE-LAB-38` dua layar data induk alasan; `FE-LAB-39` tiga tindakan dan pengesah pada Halaman Hasil Patologi Klinik — **menumpang `FE-LAB-36`**; `FE-LAB-40` antrean validasi, route **diturunkan dari konvensi** sebagai saudara `lab-worklists/cito-overdue`. **Nol tombol Validasi/Rilis di antrean**, sebab validasi adalah pernyataan sesudah hasil dilihat bersama hasil lain pasien itu. Wujud pertanyaan alasan, konfirmasi rilis, dan bentuk dua tahap antrean tetap `DEV_DISCRETION` | `DRAFT` |
| 39 | 2026-09-24 | **Gelombang `MVP-8` diturunkan dari `EPIC-LAB-14` — tiga task frontend `FE-LAB-35`..`FE-LAB-37`** (bagian akhir dokumen). `FE-LAB-35` **SIAP DIKERJAKAN** dan boleh paralel dengan backend di atas kontrak `r33` yang approved, tetapi **wajib dirilis bersama `BE-LAB-67` dan `BE-LAB-68`** — route lama dicabut pada rilis yang sama. `FE-LAB-36` halaman hasil Patologi Klinik per order menunggu `BE-LAB-69`; `FE-LAB-37` mencabut dialog Daftar Kerja **sesudah** halaman penggantinya berdiri. `FR-14.9` sengaja nol task. **Pembukuan:** header tertinggal di `35` dan dirapikan | `DRAFT` |
| 38 | 2026-09-23 | **`FE-LAB-28` selesai, dan dengan itu SELURUH task frontend Patologi Anatomi berdiri.** **Penahannya ditutup lebih dulu atas instruksi pemilik modul:** keenam jenis pemeriksaan yang tersisa digolongkan lewat akun superadmin — **10/10 terpetakan** — dan **setiap usulan ditinjau, bukan diterima begitu saja**: empat tepat, **LBC** diterima dengan catatan (lazimnya serviks, tetapi juga dipakai bahan non-ginekologi), dan **FNAB** yang nol punya usulan digolongkan `SITO_NONGIN` sejalan dengan *Sitologi Cairan Tubuh*. Keenamnya **data uji yang menunggu tinjauan `DR-LAB-003`**, bukan penggolongan resmi; `LAB-OPEN-039` bahkan mencatat FNAB sebagai varian cetak tersendiri, sehingga yang dibutuhkan kelak mungkin golongan kelima. **Pemetaan itu sekaligus menutup `AC-194` milik `BE-LAB-66`**: `unmappedProcedure` 6 → 0 pada enam penyimpanan. **Layar laporannya** membangkitkan formulir dari `fields` server, dan **aturan murninya dijalankan terhadap jawaban server sungguhan** — pesanan empat golongan menghasilkan 15 ruas dengan **nol duplikat**. Dua keputusan pantas dibaca: **`Selesaikan` dimatikan selama ada perubahan belum tersimpan**, sebab tanpanya patolog dapat melihat isi A sementara yang terkunci adalah isi B; dan **`analystUserId` dikirim ulang walau nol punya pemilih**, sebab `PUT` mengganti seluruh isi. **SATU CACAT `FE-LAB-31` DITEMUKAN DAN DIPERBAIKI:** dialog hapus isolat mengirim `onClose` yang nol dikenal `ConfirmModal`, sehingga **Batal dan Esc nol berbuat apa-apa dan satu-satunya jalan keluar adalah menekan "Ya, Hapus"** — dialog pencegah penghapusan justru memaksanya. Cacat yang sama ada di tiga layar rawat inap dan radiologi, **dilaporkan, nol disentuh**. `Simpan`/`Selesaikan`/`Buka Kembali` **sengaja nol dijalankan** terhadap pesanan pasien sungguhan. Suite 1659 → **1674 lulus**, gagal tetap 7 | `DRAFT` |
| 37 | 2026-09-23 | **Tiga task frontend dikerjakan berurutan, dan dengan itu NOL task FE Laboratorium tersisa yang penahannya adalah kode.** **`FE-LAB-31` sisa `MVP-7b`** — keempat butir yang kurang ditutup, dan **kedua jalan buntu hilang**: kolom `Kadar` kini punya pemilih satuannya (`AC-178`) dan penimpaan `Hasil` kini punya tempat menulis alasannya (`AC-187`); tabel antibiogram menjadi **dua bentuk** mengikuti metode uji (`AC-188`), dan penanda isolat tidak diuji berdiri (`AC-190`). 7 uji baru. **`FE-LAB-29`** — bagian konteks klinis berdiri pada layar pesanan; **penjaganya di pemanggil, bukan di dalam komponennya**, sehingga bagi disiplin lain komponennya nol dirender, hook-nya nol berjalan, dan permintaannya nol terkirim — itulah yang membuat `AC-154` benar-benar "nol berubah". 9 uji, `AC-154` dibuktikan **terbalik**. **`FE-LAB-27`** — **37 berkas**: dua fitur data induk penuh diturunkan dari `lab-organisms`, ditambah keberlakuan ruas (`AC-145`) dan penggolongan jenis pemeriksaan (`AC-144`); 10 uji. **`AC-144` dijaga dengan ketiadaan:** nol tombol "terapkan semua", dan tiap baris punya tombol konfirmasi sendiri yang mati selama golongan belum dipilih — pencocokan kata kunci tidak tahu apa-apa soal patologi. **Validasi ketiganya:** lint **0 error**, build **hijau**, suite unit **1633 → 1659 lulus dengan gagal TETAP 7**; ketujuhnya diperiksa satu per satu dan **nol terkait** — satu di antaranya bahkan menuntut menu `/corporate/accounting/reconciliation` yang memang belum ada. **Ketiganya belum diklik di peramban**, dan itu disebut apa adanya. **Dua hal menunggu keputusan pemilik modul:** selisih `result` wajib/opsional antara `r27` dan `AC-165`, serta penempatan bagian konteks klinis pada layar detail pesanan alih-alih layar buat | `DRAFT` |
| 36 | 2026-09-23 | **Dua koreksi status, keduanya lahir dari membaca source alih-alih membaca laporan task.** **Pertama, `FE-LAB-27` turun dari ✅ `SIAP DIKERJAKAN` menjadi ⛔ `TERTAHAN` oleh `BE-LAB-66`** yang dibuka hari ini. `BE-LAB-50` memang selesai, tetapi kesepuluh endpoint yang berdiri bukan permukaan yang dituntut `master-data-feature-standard`: ketiga grup Patologi Anatomi nol punya `filters/metadata`, `summary`, maupun `GET /{id}`. **Penahan ini sudah diketahui sejak 2026-09-22 dan sengaja dicatat** — `BE-LAB-65` bagian 6ad.1 menemukannya lalu meninggalkan ketiga grup PA justru supaya task ini nol tersandung diam-diam; yang belum terjadi adalah tasknya dibuka. Ini kali **ketiga** bentuk yang sama menahan task frontend, sesudah `FE-LAB-24`→`BE-LAB-65` dan `FE-LAB-34`→`BE-LAB-64`. **Kedua, cakupan tambahan `MVP-7b` ditandai belum tuntas.** Laporan `FE-LAB-31`/`32`/`33` menelusuri `AC-157`..`AC-170` saja dan **nol pernah mengklaim** kesembilan AC gelombang `MVP-7b`; pembacaan source menemukan **`FE-LAB-33` tuntas** sementara **`FE-LAB-31` kurang tiga hal**, dua di antaranya membuat layarnya **buntu pada jalur yang sungguh dipakai**: kolom `Kadar` menolak nilai tanpa satuan sementara pemilih satuannya nol ada (`AC-178`), dan penimpaan `Hasil` menuntut alasan yang nol punya kotak isian (`AC-187`). **Bentuknya yang pantas diingat, bukan jumlahnya** — pada keduanya ruasnya sudah sampai ke payload dan aturannya sudah lulus uji; yang tidak ada adalah permukaan untuk mengisinya, dan uji unit atas fungsi murni nol dapat menangkap itu. Rincian beserta berkas dan barisnya ada pada [`traceability.md`](traceability.md) bagian **Sinkronisasi bukti kode — 2026-09-23**. Nol baris kode disentuh pada revisi ini | `DRAFT` |
| 35 | 2026-09-22 | **`FE-LAB-24` SELESAI — kedua layar data induk Mikrobiologi berdiri, dan `B3` kini punya jalan ditutup dari aplikasi alih-alih lewat SQL langsung.** 30 berkas baru, nol layar Laboratorium lain disentuh. `AC-116`, `AC-117`, `AC-118` terbukti; `AC-117` dibuktikan **terbalik** pada tiga lapis sekaligus — nol thunk penghapusan di kedua slice, halaman detail merender **dua** aksi saja, dan kolom aksi daftar hanya `Perbarui` beserta `Aktifkan/Nonaktifkan`. **Satu jebakan backend ditutup di layar, dan ia layak dibaca pelaksana berikutnya:** `UpdateLabOrganismRequest.IsActive` bernilai `true` ketika permintaannya nol memuat ruas itu, sementara `UpdateAsync` menyetel `entity.IsActive` **tanpa syarat** — sehingga membetulkan ejaan keterangan satu kuman nonaktif akan **menghidupkannya kembali secara diam-diam**, nol galat dan nol pesan. Ruas `Status Aktif` karena itu wajib ada pada formulir ubah, nilainya dibaca apa adanya dari barisnya, dan dua uji menguncinya. **Satu cacat warisan ditemukan justru karena ujinya ditulis lebih dulu lalu gagal:** pemeriksaan panjang maksimal pada `validateForm` modul rujukan **nol pernah menyala**, sebab nilainya dipotong `safeInputString` sebelum diukur — kode yang kepanjangan tersimpan DIAM dalam bentuk terpotong, dan untuk kode organisme itu berarti penanda permanen yang berbeda dari yang diketik. Diperbaiki pada kedua berkas baru; `lab-specimen-types` dan `lab-susceptibility-breakpoint` **sengaja nol disentuh** sebab di luar cakupan, dan cacatnya **dicatat bukan ditambal diam-diam**. **Dua penyimpangan dari `master-data-feature-standard` dicatat terbuka:** nol thunk `/options` — alamatnya sudah terdaftar di registry pilihan sejak `FE-LAB-34` dan menuliskannya dua kali persis yang dilarang komentar di sana; dan satu CSS Module berisi **satu kelas**, sebab layar tanpa penghapusan menaruh penonaktifan di kolom aksi sementara bagian 7.2 melarangnya di halaman detail. **Satu butir DoD belum tercapai dan disebut apa adanya:** layar belum dijalankan terhadap backend berisi data — backend gagal start karena `Jwt:Key` belum dikonfigurasi di environment ini, dan nilainya **nol ditebak maupun diisi** dari task ini. Lint 0/4 dengan aturan dan jumlah sama persis seperti kedua modul rujukan; 30 uji baru lulus; suite 1556→1586 lulus dengan gagal **tetap 6**; build hijau | `DRAFT` |
| 34 | 2026-09-18 | **Ketiga task frontend Patologi Anatomi kini terbuka, dan satu penahan yang tersisa BUKAN pekerjaan programmer.** `BE-LAB-52` selesai, sehingga `FE-LAB-29` naik `SIAP DIKERJAKAN` dan `FE-LAB-28` kehilangan penahan teknisnya. **`FE-LAB-28` masih menunggu PEMETAAN TERISI**: empat dari sepuluh pemeriksaan PA sudah dipetakan sebagai data uji `BE-LAB-52` dan **perlu ditinjau kepala instalasi bersama `DR-LAB-003`**, enam sisanya belum sama sekali — tanpa itu formulir hasil PA kosong bagi pemeriksaan yang belum digolongkan (`INV-39`). **Dua hal yang wajib dibaca `FE-LAB-28` sebelum menulis satu baris:** bentuk formulir **dibangkitkan dari `fields` pada tanggapan** — ruas yang dipakai dua golongan datang **sekali** beserta `categoryCodes` keduanya, dan menuliskan lima belas ruas di kode akan membatalkan seluruh manfaat `LAB-DEC-086`; serta `issuedAt`/`effectiveAt` **hanya keluar, nol boleh dikirim** — mengirimnya dijawab `422`. **Peringatan `GET /{id}` dari revision 33 masih berlaku** bagi `FE-LAB-27` | `DRAFT` |
| 33 | 2026-09-18 | **`FE-LAB-27` dibuka; `FE-LAB-28` dan `FE-LAB-29` tetap tertahan, dan sekarang penahannya berbeda.** `BE-LAB-50` selesai beserta kesepuluh endpoint data induknya, sehingga `FE-LAB-27` naik `SIAP DIKERJAKAN`. `FE-LAB-28` dan `FE-LAB-29` menunggu `BE-LAB-52` yang **belum** dikerjakan — `BE-LAB-51` yang selesai menyusul hanya mendirikan tabelnya, **nol endpoint**. **Satu peringatan yang perlu dibaca `FE-LAB-27` lebih dulu:** ketiga resource data induk **nol punya `GET /{id}`**, sehingga formulir ubah yang dibuka lewat tautan langsung atau sesudah halaman disegarkan nol punya jalur memuat barisnya — kelas kesalahan yang sudah dibayar modul ini lewat `r6` sesudah `FE-LAB-03` diam-diam gagal di luar halaman daftar. Diusulkan sebagai `r26` bila pemilik modul menghendaki | `DRAFT` |
| 32 | 2026-09-18 | **Kontrak `r25`/`r8`/rev 7 disetujui; ketiga task frontend tetap tertahan, dan itu bukan kelalaian pembukuan.** Yang menahan `FE-LAB-27`, `FE-LAB-28`, dan `FE-LAB-29` tidak pernah kontraknya melainkan **endpoint yang belum ada di kode** — persis yang dinyatakan label "Rencana (belum tersedia)". `FE-LAB-27` tetap menunggu `BE-LAB-50`; `FE-LAB-28` tetap menunggu `BE-LAB-52` **dan pemetaan terisi**; `FE-LAB-29` tetap menunggu `BE-LAB-52`. **Nol acceptance criteria, cakupan, atau dependency berubah**; yang berubah hanya catatan gerbang pada bagian 6e.5 | `DRAFT` |
| 31 | 2026-09-18 | **Bagian Patologi Anatomi diturunkan ulang.** `FE-LAB-25` **dibatalkan**, digantikan `FE-LAB-27` dua layar data induk, `FE-LAB-28` layar laporan, dan `FE-LAB-29` konteks klinis pada layar pemesanan. **Nol baris kode terbuang.** **Tiga belas acceptance criteria baru, `AC-143`..`AC-155`.** **`AC-146` yang paling berisiko** dan saya beri penilaian risiko **tinggi**: formulir wajib **dibangkitkan dari daftar parameter server**, bukan dari lima belas ruas yang ditulis di kode — menuliskannya di kode terasa lebih cepat dan **membatalkan seluruh manfaat `LAB-DEC-086`**, sebab parameter keenam belas kelak akan menuntut rilis frontend. **`AC-144` juga mudah disederhanakan keliru**: layar pemetaan menyediakan usulan dari `GET /suggestions`, dan usulan itu **wajib dikonfirmasi manusia** — tombol "terapkan semua" akan menghapus pemeriksaan manusianya, dan itu persis yang `LAB-DEC-087` hendak cegah. **`FE-LAB-29` menyentuh layar yang SEDANG DIPAKAI PETUGAS** (`ARCH-GAP-LAB-07`), sehingga `AC-154` menuntut **pembuktian terbalik** bahwa pemesanan Patologi Klinik dan Mikrobiologi nol terdampak — kelas yang sama dengan `BE-LAB-21` yang sempat membuat layar wadah menjawab `422` | `DRAFT` |
| 30 | 2026-09-18 | **Gelombang `MVP-6` diturunkan — tiga task frontend, `FE-LAB-24` sampai `FE-LAB-26`** (bagian 6e). Ketiganya berstatus **`MENUNGGU`** pasangan backend-nya, dan itu disengaja: layar yang dibangun terhadap endpoint yang belum ada tidak dapat diuji selain pada keadaan gagalnya. **Sepuluh acceptance criteria, dan empat di antaranya berbentuk ketiadaan** — `AC-117` nol tombol Hapus, `AC-120` nol tombol Validasi/Rilis, `AC-121` nol Simpan draft, `AC-122` nol lencana status hasil. Keempatnya **diuji secara terbalik**, bukan diasumsikan, sebab ketiganya hal yang terasa melengkapi layar dan justru karena itu paling mungkin ditambahkan. **`AC-123` saya beri penjelasan risiko tersendiri:** isolat dan kepekaannya wajib terlihat **bersarang**, bukan dua daftar sejajar — sebab dua daftar sejajar membuat petugas mengira antibiotik diuji terhadap *pemeriksaan*, bukan terhadap kuman tertentu, dan salah baca itu berujung pada terapi yang keliru. **`FE-LAB-26` punya dependency yang bukan kode:** daftar organisme dan antibiotik **terisi**, dan `AC-127` menuntut layar menyebutkan penyebab serta siapa yang mengisinya ketika daftarnya masih kosong — bukan menampilkan pemilih kosong tanpa penjelasan. **Satu catatan pembukuan:** header dokumen ini menunjuk revision `29` sedangkan baris riwayat terakhir `26`. Baris `27` sampai `29` **tidak pernah ditulis dan tidak saya susun ulang dari ingatan**; nomor berikutnya diambil `30` agar hitungannya tidak mundur | `DRAFT` |
| 26 | 2026-09-17 | **`FE-LAB-12` SELESAI, dan `MVP-5a` TUNTAS — pada hari yang sama task itu sempat ditandai ⛔ `TERTAHAN`.** Penahannya, ketiadaan jalur baca lintas pesanan, ditutup `r17` yang disetujui pemilik modul lalu dilaksanakan `BE-LAB-38`; `orderNumber` yang ikut dibutuhkan datang dari `r16`/`BE-LAB-37`. **Tiga keputusan implementasi dicatat.** Penyaringan dikerjakan **backend**, bukan di browser — menyaring dua kali adalah cara paling cepat membuat angka pada layar berbeda dari angka pada rekap. Ekspresi waktu kedatangan ditulis **sama persis** dengan backend, dan uji unit menguncinya. Layar detail **memuat datanya sendiri** lewat `by-order`, bukan dari daftar yang kebetulan ada di memori — pelajaran `r6` yang lahir dari `FE-LAB-03`. **Pemeriksaan yang paling mudah terlewat dibuktikan sengaja:** baris yang waktu tibanya tidak dicatat **ditandai terang-terangan**, karena tanpa penanda itu waktu pencatatan akan terbaca sebagai waktu kedatangan — kekeliruan yang nol menimbulkan galat. **Aturan tanggal dipakai ulang untuk ketiga kalinya** dari `lab-monitoring-rules`, bukan disalin. **Satu kekeliruan uji dicatat:** ekspektasi selisih ditulis "10 jam" padahal 655 menit adalah 10 jam 55 menit; pada laporan keterlambatan, pembulatan ke jam membuang ketelitian yang justru dicari — ujinya diperbaiki, kodenya dipertahankan. 1043/1043 uji unit, 4 pemeriksaan layar, dan **`AC-76` terbukti**: 18/18 spec Laboratorium lain lulus | `DRAFT` |
| 25 | 2026-09-17 | **`FE-LAB-12` ditandai ⛔ `TERTAHAN` pada pemeriksaan pra-implementasi, sebelum satu baris pun ditulis — dan penahannya adalah pola yang kini muncul untuk KELIMA kalinya.** Cakupan task menuntut daftar penerimaan **lintas pesanan** berpenyaring rentang tanggal; penelusuran source membuktikan **nol endpoint mengembalikannya**. Yang ada hanya `GET /lab-specimens/summary` — angka rekap, **nol baris** — dan `GET /by-order/{id}` yang melayani satu pesanan. Penelusuran diteruskan ke luar controller wadah: `LabMonitoringService` menyentuh `LabSpecimens` hanya untuk **mencacah** dan sebagai sub-query penyaring, sementara `LabWorklistService` nol menyentuhnya. **Setengah task ini justru sudah siap dan itu pantas dicatat:** `GET /summary` menyaring tepat pada `PhysicallyReceivedAt ?? CreateDateTime`, dan komentarnya menyebut `AC-67` apa adanya — wadah yang tiba Senin 21.10 dan diregistrasi Selasa 08.05 **sudah** terhitung pada hari Senin. Yang hilang **barisnya, bukan aturannya**. **Sengaja tidak diturunkan menjadi versi sebagian**, mengikuti preseden `FE-LAB-17`: layar berisi angka rekap tanpa baris yang diringkasnya menimbulkan pertanyaan yang tidak dapat dijawab layar itu sendiri. **Usul `LAB-API-v1` `r17` ditulis lengkap** — satu endpoint baca `GET /lab-specimens`, penyaring mengikuti bentuk `LabOrderPagedQuery` yang sudah berjalan sejak `r5`, aditif, nol migration — beserta **satu peringatan yang menentukan**: rentangnya wajib disaring pada `PhysicallyReceivedAt ?? CreateDateTime` persis seperti `GetSummaryAsync`, karena bila keliru seluruh guna kolom itu hilang **tanpa satu pun kesalahan yang terlihat**; layarnya tetap tampil benar, hanya tanggalnya yang salah. **Kelima kejadiannya kini ditabelkan pada bagian 6b** supaya polanya terbaca sebagai pola, bukan sebagai lima kejadian terpisah: nilai tersimpan tanpa jalan keluar, tabel ditulis tanpa pembaca, kolom berdiri tanpa jalan keluar, ruas dituntut backend tanpa penulis, dan kini layar dituntut roadmap tanpa jalur baca | `DRAFT` |
| 24 | 2026-09-17 | **`FE-LAB-19` dibuka dan ditutup pada hari yang sama — satu cacat yang sedang berjalan, bukan requirement baru.** Layar wadah `FE-LAB-07` **berhenti dapat merencanakan wadah sama sekali** sejak migration `BE-LAB-21` diterapkan 2026-09-15: `VAL-51` menolak tanpa syarat setiap permintaan tanpa `specimenTypeId`, sementara `buildPlanPayload` hanya membawa `examinations` dan `specimenDescription`. **Yang pantas dicatat adalah bagaimana ia lolos:** build tidak melihatnya karena payload-nya JavaScript biasa; lint nol kaitannya; tinjauan kontrak nol menolong karena `r7` memang sudah disetujui — dan **uji yang ada justru mengunci perilaku cacatnya**, menyatakan rencana wadah tanpa jenis specimen adalah sah. Yang menemukannya adalah penelusuran kelima nama ruas pada seluruh `src` saat `FE-LAB-11` dikerjakan, dan hasilnya nol kemunculan. `traceability.md` revisi 18 bahkan sudah meramalkannya dua hari sebelumnya. **Perbaikannya memakai ulang aturan murni `FE-LAB-11`, bukan menyalinnya** — `VAL-51`, `VAL-53`, dan `VAL-56` kini punya satu definisi yang dipakai kedua layar; salinan kedua adalah yang kelak bercabang sehingga dua layar mulai menolak hal yang berbeda. **Satu uji lama sengaja dibalik** dan dicatat sebagai perubahan perilaku yang disengaja, bukan uji yang diperbaiki agar lulus. **Satu keputusan dicatat:** daftar jenis yang gagal dimuat **tidak** menahan petugas — menahan pekerjaan karena daftar yang belum tiba lebih buruk daripada satu `422` yang terbaca. 1033/1033 uji unit, 18/18 pemeriksaan layar Laboratorium, build hijau | `DRAFT` |
| 23 | 2026-09-17 | **Bukti pelaksanaan `FE-LAB-11`. Layar terbesar modul ini berdiri, dan satu butir DoD-nya dicabut sebelum satu baris pun ditulis.** Butir *"jumlah/Qty dapat diisi dan hasilnya tersimpan sebagai beberapa baris"* bertentangan dengan **tiga sumber otoritatif sekaligus** — `LAB-DEC-050`/`BR-45`, kontrak `r9`, dan `03-frontend-architecture.md` §10.5 — dan ketiganya bertanggal **2026-09-14**, hari yang sama task ini ditulis. Sebabnya bukan selera: `LabExamination` memiliki index unik `(SpecimenId, ProcedureId)`, sehingga "jumlah 3 menjadi tiga baris" **ditolak database pada baris kedua**. Membangunnya sesuai DoD lama berarti membangun sesuatu yang pasti gagal saat dipakai. Butirnya diganti menjadi kebalikannya, dan `BR-44` yang sebelumnya nol masuk DoD ditambahkan. **Layarnya merangkai lima panggilan** — pendaftaran → kunjungan → pesanan per disiplin → wadah → kelayakan — karena pendaftaran hanya mengembalikan kunjungan dan wadah menempel pada pesanan. **Dua komponen dipakai ulang tanpa dipindahkan, dan cara memakainya menentukan:** `useLabPatientRegistrationForm` menerima **string posisional** bukan objek beropsi, dan `LabCatalogPicker` dipakai sebagai **komponen** bukan hook — memanggil hook-nya lagi akan melahirkan instance kedua yang state-nya terpisah, sehingga layar menampilkan satu pilihan sementara yang dikirim adalah pilihan yang lain. **Tiga dari enam pemeriksaan layar menguji KETIADAAN:** nol kotak Jumlah/Qty, nol `select` metode pembayaran, dan tombol kelayakan yang tidak dapat ditekan beserta sebabnya. **`AC-76` terbukti** — 12/12 uji layar lab lama lulus tanpa satu berkas pun disentuh; 1029/1029 uji repository lolos. **Satu kekeliruan uji dicatat supaya tidak terulang:** `datetime-local` menghasilkan waktu **lokal**, dan membandingkannya terhadap `now` bertulis UTC menggeser hasilnya sebesar offset zona tanpa satu pun galat muncul — versi pertama uji `VAL-58` tertangkap di WIB dan akan **lolos** pada mesin ber-UTC. **⚠ Satu cacat berjalan ditemukan dan ia bukan lahir dari task ini:** kelima ruas wadah `r7` **nol punya penulis di frontend**, sehingga `VAL-51` menolak `422` setiap perencanaan wadah dari `FE-LAB-07` sejak migration `BE-LAB-21` diterapkan 2026-09-15. Perbaikannya kecil — ruas wilayah C task ini tinggal dipasang — tetapi cakupan melarang mengubah layar lama, sehingga ia dilaporkan sebagai task tersendiri, bukan dikerjakan diam-diam. **Dua batas dilaporkan apa adanya:** wadah direncanakan pada pesanan **pertama** saat pemeriksaannya melintasi beberapa disiplin, karena membentuk beberapa wadah sekaligus adalah keputusan yang belum diambil siapa pun; dan `BR-44` baru terpenuhi pada **kalimatnya**, belum pada jalur pembatalan barisnya | `DRAFT` |
| 22 | 2026-09-16 | **Bukti pelaksanaan `FE-LAB-10`. Gelombang `MVP-5a` dimulai, dan ia dipilih justru karena ketiga penahan modul tidak menyentuhnya.** `LAB-SIGN-001`, `LAB-REQ-007`, dan `LAB-COORD-010` seluruhnya mengunci `S17` dan `S4`; `FE-LAB-10` milik `MVP-5a`, kontraknya `r7`/`r8` terkunci sejak 2026-09-14, dan kedua dependency backendnya selesai sejak 2026-09-15. **Dua layar berdiri** di `health-services/master-data/lab-specimen-types/` — bukan di folder laboratorium, sesuai `AC-49` dan `LAB-FE-014`. **Butir DoD yang paling menentukan dibuktikan dengan menghitung permintaan yang tiba, bukan dengan melihat tampilan:** tautan langsung ke layar kelola memuat barisnya lewat `GET /{id}` dan **nol memanggil jalur daftar** — inilah jalur yang `LAB-API-v1` `r6` lahir untuk mencegah terulangnya, sesudah `FE-LAB-03` memuat barisnya dari halaman daftar yang sedang terbuka lalu diam-diam gagal di luar itu. **`VAL-63` ditegakkan di layar dan dibuktikan dua arah:** tombol Nonaktifkan pada baris `Lainnya` terakhir mati beserta sebabnya, **dan** baris biasa terbukti tetap hidup — tanpa pembuktian terbalik itu, penjaga yang salah pasang dan mematikan seluruh baris akan tetap lolos. Kalimat penolakannya diambil **kata demi kata** dari backend, dan kesamaannya diperiksa terhadap `HTTP 422` yang sungguhan. **Bukti data dijalankan terhadap basis data yang sebenarnya:** 7 jenis urut `sortOrder`, `jalanKeluarLainnyaAktif` bernilai 1, dan penolakan `VAL-63` diikuti pemeriksaan bahwa **nol baris berubah**. 15 uji unit baru, 1014/1014 uji repository lolos, 7 pemeriksaan layar, build produksi hijau. **Satu bagian verifikasi tidak dapat dijalankan dan disebut apa adanya:** skenario *"cairan kista berjumlah tiga"* menuntut wadah berjenis `Lainnya` yang tercatat, sedangkan `GET /other-usage` mengembalikan `totalData 0` — ketiadaan **data**, bukan ketiadaan layar, dan jalur yang mengisinya adalah `FE-LAB-11`. Membuat data uji pada basis data bersama adalah perubahan data di luar wewenang task frontend, dan tidak dilakukan. **Tiga selisih terhadap `master-data-feature-standard.md` dilaporkan, bukan didiamkan:** grup ini nol `DELETE` dan nol `PATCH /{id}/status`, fitur ini nol halaman detail, dan ia menambah satu CSS Module mengikuti preseden `lab-rejection-reasons`. **Satu thunk sengaja berdiri tanpa pembaca** — `GET /options`, konsumennya `FE-LAB-11` — dan itu dicatat supaya tidak terulang pola `BE-LAB-26`, yaitu sesuatu yang berdiri tanpa pembaca lalu tidak menghasilkan galat apa pun sampai seseorang membutuhkannya. **Satu kerusakan pembukuan ditemukan pada berkas ini sendiri dan diperbaiki seperlunya:** tabel Riwayat Revisi kehilangan baris pemisahnya, sehingga seluruh isinya tidak terbaca sebagai tabel; pemisahnya dikembalikan. Kerusakan lain pada tabel yang sama — nomor revisi 13 dan 14 yang masing-masing muncul dua kali, beserta satu baris pemisah yang tersesat di tengah tabel — **dicatat, tidak disentuh**, karena memperbaikinya berarti menyimpulkan ulang urutan kejadian dari ingatan dokumen | `DRAFT` |
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

---

## Gelombang `MVP-7` — Halaman Hasil Mikrobiologi (`S4b`)

Menurunkan [`03-frontend-architecture.md`](../03-frontend-architecture.md) amandemen
2026-09-21.

| Field | Nilai |
|---|---|
| Kontrak yang berlaku | `LAB-API-v1` **`r26`**, `LAB-PERM-v1` **rev 8** — keduanya `approved` 2026-09-21 |
| Frontend SHA saat direncanakan | `ebef7ebe5` |
| Slice | `S4b` saja |

**Keadaan awal yang berbeda dari perencanaan sebelumnya.** Peta kemampuan revision 4 mencabut
`F5`: frontend **sudah** punya modul Laboratorium — 17 route, 159 berkas, termasuk route daftar
pantau Mikrobiologi. Gelombang ini karena itu **menyambung**, bukan mendirikan dari nol.

```text
MVP-7a  FE-LAB-30  kerangka halaman + pemilih pemeriksaan  ──┐
                                                             │
MVP-7b  FE-LAB-31  form isolat dan antibiogram             ←─┘
        FE-LAB-32  Informasi Specimen yang dapat disunting ←─┘
                                                             
MVP-7c  FE-LAB-33  kelengkapan, konsultasi, dan konfirmator ← butuh 30
```

> **Paralel dengan backend diizinkan** karena `LAB-API-v1` `r26` sudah `approved` dan
> hash-nya terkunci. Tanpa itu, `FE-LAB-31` dan seterusnya wajib menunggu.

### `FE-LAB-30` — Kerangka halaman dan pemilih pemeriksaan

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 — lihat [`FE-LAB-30.md`](../task/report/frontend/FE-LAB-30.md). `AC-156` terbukti lewat 8 uji unit; **satu butir DoD (diagnosis) BELUM terpenuhi karena nol endpoint menyediakannya** |
| **Outcome** | Petugas membuka satu No. Order dari daftar pantau Mikrobiologi dan melihat konteks pasien, pemeriksaan, diagnosis, serta daftar pemeriksaan di dalamnya |
| **Requirement/decision** | `LAB-DEC-095` |
| **Kontrak** | `r26`; `GET /{id}/result/microbiology` dari `r24` |
| **Reuse** | `lab-monitoring-view` sebagai titik masuk; pola route `[slug]` dari `lab-orders/[slug]`; komponen kartu informasi pasien yang sudah dipakai halaman lain |
| **Cakupan** | Satu route `lab-monitoring/microbiology/[slug]`; kerangka halaman; **pemilih baris pemeriksaan** |
| **Dependency** | Nol pada frontend; kontrak sudah terkunci |
| **Acceptance criteria** | `AC-156` dua pemeriksaan dalam satu order tampil sebagai **dua tempat hasil terpisah**; memilih baris A tidak mengubah satu ruas pun pada baris B |
| **Risiko/pemilik** | **Sedang.** Godaan terbesar menampilkan dua form utuh sekaligus. Itu membuat petugas mudah mengisi antibiogram pada baris yang salah |
| **DoD** | Route hidup; pemilih bekerja; informasi pasien/pemeriksaan/diagnosis tampil baca-saja; `AC-156` terbukti |

### `FE-LAB-31` — Form isolat dan antibiogram

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** 2026-09-22 — lihat [`FE-LAB-31.md`](../task/report/frontend/FE-LAB-31.md). Permukaan terbangun, lint dan build hijau, 22 uji baru lulus. **Nol AC terbukti di layar** — verifikasi klik belum dijalankan |
| **Outcome** | Analis mengisi status temuan, menambah isolat, dan mengisi baris kepekaan antibiotik |
| **Requirement/decision** | `LAB-DEC-101`, `LAB-DEC-102`, `LAB-DEC-104`, `LAB-DEC-113` |
| **Kontrak** | `r24` bagian 19.2; `r26` bagian 21.3 untuk penanda kritis |
| **Reuse** | Pola tabel baris yang dapat ditambah/dikurangi; searchable select dari `LabOrganism` dan `LabAntibiotic` yang **endpointnya sudah berjalan** |
| **Cakupan** | Pemilih status temuan (tiga nilai); daftar isolat; tabel antibiogram per isolat; penanda kritis |
| **Dependency** | `FE-LAB-30`; backend `BE-LAB-56` untuk penanda kritis yang sungguhan |
| **Acceptance criteria** | `AC-163` baris kepekaan tanpa MIC dan tanpa zona tersimpan; `AC-164` **kultur tanpa satu pun isolat tersimpan tanpa peringatan**; `AC-165` baris tanpa interpretasi ditolak beserta sebabnya; `AC-166` ketika aturan kritis kosong, layar **menyatakannya** — bukan diam; `AC-176` status temuan menawarkan tepat tiga nilai |
| **Risiko/pemilik** | **Tinggi.** Dua jebakan: memaksa isolat terisi (mematikan hasil kultur steril), dan layar bersih yang terlihat aman padahal aturan kritis belum ada |
| **DoD** | Kelima AC terbukti; **nol** pengetikan bebas pada organisme dan antibiotik; nol pilihan `NeedsAttention`/`Critical` |

### `FE-LAB-32` — Informasi Specimen yang dapat disunting

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** 2026-09-22 — lihat [`FE-LAB-32.md`](../task/report/frontend/FE-LAB-32.md). Permukaan terbangun, lint dan build hijau, 22 uji baru lulus. **Nol AC terbukti di layar** — verifikasi klik belum dijalankan |
| **Outcome** | Petugas melengkapi dan mengoreksi specimen dari halaman hasil, dan dapat melihat nilai lamanya |
| **Requirement/decision** | `LAB-DEC-098`, `LAB-DEC-100`, `LAB-DEC-107`, `LAB-DEC-112` |
| **Kontrak** | `r26` bagian 21.4 dan 21.5 |
| **Reuse** | Pola form specimen pada `lab-orders/[slug]/specimens` yang sudah ada |
| **Cakupan** | Bagian Informasi Specimen; pemilih Spesifik Specimen **banyak pilihan**; pasangan angka + satuan volume; panel riwayat perubahan |
| **Dependency** | `FE-LAB-30`, `BE-LAB-55`, `BE-LAB-57` |
| **Acceptance criteria** | `AC-160` lebih dari satu Spesifik Specimen dapat dipilih; `AC-162` `2 swab` tampil sebagai angka dan satuan; `AC-170` sesudah Final seluruh bagian menjadi baca-saja |
| **Risiko/pemilik** | **Sedang.** Volume mudah keliru dibuat satu kotak teks bebas, dan itu mencabut `LAB-DEC-100` diam-diam |
| **DoD** | Ketiga AC terbukti; **nol** tombol menambah Spesifik Specimen baru dari halaman ini (`LAB-DEC-098` butir 5) |

### `FE-LAB-33` — Kelengkapan, konsultasi, dan dokter konfirmator

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** 2026-09-22 — lihat [`FE-LAB-33.md`](../task/report/frontend/FE-LAB-33.md). Permukaan terbangun, lint dan build hijau, 22 uji baru lulus. **Nol AC terbukti di layar** — verifikasi klik belum dijalankan |
| **Outcome** | Petugas menyimpan Draft/Final, membuka kembali, mencatat konsultasi, dan memilih dokter konfirmator |
| **Requirement/decision** | `LAB-DEC-096`, `LAB-DEC-097`, `LAB-DEC-105`, `LAB-DEC-106`, `LAB-DEC-111` |
| **Kontrak** | `r26` bagian 21.2, 21.3, 21.7 |
| **Reuse** | Pola tombol aksi dan konfirmasi yang sudah dipakai halaman Laboratorium lain |
| **Cakupan** | Tombol Simpan Draft/Final/Reopen; bagian Diagnostic Report baca-saja; bagian konsultasi; pemilih dokter konfirmator beserta jalur jatuhnya |
| **Dependency** | `FE-LAB-30`, `BE-LAB-54`, `BE-LAB-58`, `BE-LAB-59` |
| **Acceptance criteria** | `AC-157` kedua waktu tampil baca-saja; `AC-158` sesudah Final layar menyatakan hasil **belum dirilis**; `AC-168` `Analis` baca-saja dan sama dengan pengguna yang menyimpan; `AC-169` konsultasi **tidak** membuka tombol apa pun; `AC-173`/`AC-174` pemilih dokter bekerja pada **kedua** keadaan jadwal jaga |
| **Risiko/pemilik** | **Tinggi, dan risikonya kata-kata pada tombol.** Melabeli `Simpan Final` sebagai "sahkan" atau "kirim" mencabut `LAB-DEC-097` tanpa satu baris kode melanggar apa pun. Label tombol dan pernyataan "belum dirilis" **bukan** `DEV_DISCRETION` |
| **DoD** | Kelima AC terbukti; **nol** tombol kirim ke pasien; nol pilihan `HL7`; nol tombol validasi/rilis |

### Yang TIDAK menjadi task frontend gelombang ini

| Yang dikecualikan | Alasan |
|---|---|
| Tombol validasi dan rilis | `S4d`, tertahan `DEC-LAB-011` |
| Tombol kirim hasil ke pasien | `LAB-COORD-011`; dan Final bukan rilis |
| Preview dan cetak Bahasa Inggris | `LAB-COORD-013` |
| Tata letak cetak yang pasti | `LAB-OPEN-039` — kedua screenshot belum diserahkan |
| Pilihan `HL7` | `LAB-DEC-109` |

---

## Gelombang `MVP-7b` — layar `S4b` sesudah bukti cetak

Menurunkan [`03-frontend-architecture.md`](../03-frontend-architecture.md) amandemen kedua
2026-09-21. Kontrak `LAB-API-v1` **`r27`** dan `LAB-PERM-v1` **rev 9**, keduanya `approved`
2026-09-21.

### ⚠ `FE-LAB-31` dan `FE-LAB-33` BERUBAH CAKUPAN

| Task | Yang bertambah |
|---|---|
| `FE-LAB-31` | Tabel antibiogram menjadi **dua bentuk** — difusi (`UG`, `R-S`, `Zona/mm`) dan dilusi (`Kadar`, `Satuan`). Kolom `Hasil` **terisi sendiri** dan dapat ditimpa beralasan. Penanda isolat tidak diuji. Bagian set bakteri **disembunyikan** bila profil menyatakan tidak memakainya |
| `FE-LAB-33` | Pemilih kualifikasi `Definitif`/`Sementara`; pemilih jenis biakan dan metode uji; `Petugas Otorisasi` tampil kosong sebelum rilis |

AC yang bertambah: `FE-LAB-31` memperoleh `AC-179`, `AC-186`, `AC-187`, `AC-188`, `AC-189`,
`AC-190`, `AC-191`; `FE-LAB-33` memperoleh `AC-177`, `AC-183`.

> ### ✅ DITUTUP 2026-09-23 — keempat butir yang kurang kini terbangun
>
> `AC-178` pemilih satuan MIC, `AC-187` kotak alasan penimpaan, `AC-188` dua bentuk tabel
> antibiogram, dan `AC-190` penanda isolat tidak diuji — seluruhnya berdiri, dengan **7 uji
> unit baru**, lint 0 error, dan build hijau. **Kedua jalan buntu hilang:** kolom `Kadar` kini
> punya pemilih satuannya, dan penimpaan `Hasil` kini punya tempat menulis alasannya. Lihat
> [`FE-LAB-31-sisa-mvp7b.md`](../task/report/frontend/FE-LAB-31-sisa-mvp7b.md). **Belum diklik
> di peramban.**
>
> **Satu selisih kontrak dilaporkan, nol diubah:** `r27` 22.2 menurunkan `result` dari wajib
> menjadi opsional, sementara aturan frontend masih mewajibkannya mengutip `AC-165`/`VAL-83`
> milik `r24`. Perlu keputusan pemilik modul.

> *(Riwayat)* **Sinkronisasi bukti kode 2026-09-23 — cakupan tambahan ini BELUM tuntas, dan laporannya
> memang nol mengklaimnya.** Laporan `FE-LAB-31`/`32`/`33` menelusuri `AC-157`..`AC-170` saja;
> kesembilan AC di atas nol pernah diklaim maupun ditandai. Pembacaan source menemukan
> **`FE-LAB-33` tuntas** — ketiga pemilihnya berdiri — sementara **`FE-LAB-31` kurang tiga
> hal**, dan dua di antaranya membuat layarnya buntu pada jalur yang sungguh dipakai:
> tabel antibiogram masih **satu bentuk** (`AC-188`), kolom `Kadar` menolak nilai tanpa satuan
> sementara **pemilih satuannya nol ada** (`AC-178`), dan penimpaan `Hasil` menuntut alasan
> yang **nol punya kotak isian** (`AC-187`). Penanda isolat tidak diuji (`AC-190`) juga nol
> punya kontrol, walau aturannya sudah benar. Rinciannya beserta berkas dan barisnya ada pada
> [`traceability.md`](traceability.md) bagian **Sinkronisasi bukti kode — 2026-09-23**.

### `FE-LAB-34` — Tiga layar data induk baru

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** 2026-09-22 — ketiga layar terbangun sesuai `master-data-feature-standard` (38 berkas source, 6 registrasi), lint dan build hijau, 20 uji baru lulus, sepuluh route masuk keluaran build dan delapan di antaranya dipanggil `200`. **Hak aksesnya terbukti pada akun sungguhan bukan superadmin**: keenam endpoint baca dijawab `200` bagi Kepala Instalasi, dan `PATCH` status breakpoint dijawab **`403`** — pemisahan `DR-LAB-002` tegak di sisi server. **Nol kontrol diklik di peramban.** Lihat [`FE-LAB-34.md`](../task/report/frontend/FE-LAB-34.md). *(Sebelumnya `SIAP DIKERJAKAN`; blok ini sempat tertinggal `MENUNGGU PENDAHULU`.)* **Keempat penahan backend selesai:** `BE-LAB-60` ✅, `BE-LAB-62` ✅, `BE-LAB-63` ✅, dan **`BE-LAB-64` ✅** (permukaan baseline dua data induk dilengkapi 5→9 endpoint — tanpanya ketiga layar nol dapat dibangun sesuai standar) |
| **Outcome** | Wewenang klinis mengelola breakpoint; kepala instalasi mengelola profil katalog dan pengaturan disiplin |
| **Requirement/decision** | `LAB-DEC-119`, `122`, `125`, `127` |
| **Kontrak** | `r27` bagian 22.5, 22.6, 22.7 **dan `r30` bagian 25** (delapan endpoint baseline) |
| **Reuse** | `master-data-feature-standard` sepenuhnya — pola yang sama dengan layar `LabOrganism` dan `LabAntibiotic` |
| **Cakupan** | Tiga layar: breakpoint (CRUD penuh), profil Mikrobiologi katalog (CRUD penuh), pengaturan disiplin (**baca dan ubah saja** — nol tambah, nol hapus) |
| **Acceptance criteria** | `AC-182` mengubah nama konsultan mengubah footer dan **tidak** mengubah pemegang wewenang klinis; `AC-185` mengubah breakpoint **tidak** mengubah hasil lama |
| **Risiko/pemilik** | **Sedang.** Layar breakpoint dipegang wewenang klinis, bukan kepala instalasi — pemisahan itu harus terlihat pada menu, bukan hanya ditegakkan server |
| **DoD** | Ketiga layar hidup; pengaturan disiplin **nol punya tombol tambah maupun hapus**; kedua AC terbukti |

### Yang TIDAK dibangun

Susunan dua isolat berantibiogram, tampilan hasil nol pertumbuhan, dan pengulangan kop halaman
kedua — ketiganya belum pernah terlihat (`LAB-OPEN-039`).

---

## Gelombang `MVP-8` — `EPIC-LAB-14` halaman hasil Patologi Klinik dan perbaikan halaman Mikrobiologi, 2026-09-24

Menurunkan [`03-frontend-architecture.md`](../03-frontend-architecture.md) **amandemen
2026-09-24** dan [`04-prd-to-mvp.md`](../04-prd-to-mvp.md) **bagian 20**.

| Field | Nilai |
|---|---|
| Kontrak | `LAB-API-v1` **`r33`**, `LAB-VAL-v1` **`r11`**, `LAB-PERM-v1` **rev 10** — `approved` 2026-09-24 |
| Masukan | decisions **rev 72**; capability map **rev 5**; `03-frontend-architecture.md` rev 9 |
| Frontend SHA | `72607a087` (branch `YogaV2`) |
| Hash masukan | Pada [`traceability.md`](traceability.md) bagian *Traceability gelombang `MVP-8`* |
| Nomor task | `FE-LAB-35`..`FE-LAB-37` |
| Kerja paralel | `FE-LAB-35` **boleh** dikerjakan bersamaan dengan `BE-LAB-67`/`BE-LAB-68` karena kontraknya approved dan hash-nya tercatat — **tetapi wajib dirilis bersama** keduanya |

**Wewenang tampilan.** Keberadaan halaman per order, larangan modal, dan penanda berhuruf
sudah diputuskan (`LAB-FE-015`..`LAB-FE-017`). Urutan kolom, urutan baris, letak tombol, dan
navigasi papan ketik tetap **`DEV_DISCRETION`** — roadmap ini tidak memutuskannya.

### `FE-LAB-35` — Halaman Mikrobiologi mengikuti izin hasil, route netral, dan penjaga Final

| Butir | Isi |
|---|---|
| **Status** | `SIAP DIKERJAKAN` — **dirilis bersama** `BE-LAB-67` dan `BE-LAB-68` |
| **Gelombang** | `MVP-8a` |
| **Outcome** | Halaman Mikrobiologi tetap berfungsi sesudah route lama dicabut; pengguna tanpa izin hasil melihat halaman baca-saja; penolakan `409` dan `403` tampil terbaca tanpa menghilangkan isian |
| **Requirement/decision** | `FR-14.6`, `FR-14.12`; `LAB-DEC-146`, `LAB-DEC-147` |
| **Kontrak** | `r33` bagian 28.2 dan 28.4; `LAB-PERM-v1` rev 10 |
| **Reuse** | `usePermission` (`src/lib/hooks/auth/use-permission.jsx`); `use-lab-microbiology-result-editor.jsx` yang sudah berdiri |
| **Cakupan** | `lab-microbiology-result-constants.jsx:16,19,22` beralih ke route netral; penanganan `409` dan `403` pada hook editor; kontrol tulis disembunyikan bagi yang tidak memegang `LabExaminationResult : Update` |
| **Dependency** | Kontrak `r33` — tidak menunggu kode backend untuk mulai, tetapi **menunggu `BE-LAB-67` dan `BE-LAB-68` untuk diverifikasi dan dirilis** |
| **Acceptance criteria** | `AC-228`; bagian antarmuka `AC-221` — akun tanpa izin hasil melihat halaman baca-saja |
| **Verifikasi** | Unit test aturan: pemetaan `409` menjadi pesan dan pelestarian isian; lalu dijalankan terhadap backend `BE-LAB-68` — simpan hasil Final ditolak dan nilai yang diketik tetap di isian. Uji unit dijalankan dengan `node --import ./tests/helpers/register.mjs --test tests/unit/` |
| **Risiko/pemilik** | **Tinggi pada rilis.** Dirilis tanpa backend → Final/Reopen/konsultasi memanggil route yang belum ada; backend dirilis tanpa ini → halaman memanggil route yang sudah dicabut. Pemilik: implementer frontend |
| **DoD** | Nol rujukan tersisa ke `/result/microbiology/finalize\|reopen\|consultation`; `AC-228` terbukti; lint dan build hijau; laporan `task/report/frontend/FE-LAB-35.md` |

### `FE-LAB-36` — Halaman Hasil Patologi Klinik per order

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-69` untuk diverifikasi; boleh mulai di atas kontrak `r33` |
| **Gelombang** | `MVP-8c` |
| **Outcome** | Analis mengisi seluruh pemeriksaan Patologi Klinik satu order pada satu halaman, menyatakan selesai per pemeriksaan, dan melihat penanda `L`/`H` berupa huruf |
| **Requirement/decision** | `FR-14.5`, `FR-14.8`, `FR-14.10`; `LAB-DEC-135`, `LAB-DEC-141`, `LAB-DEC-149`, `LAB-FE-015`..`LAB-FE-017` |
| **Kontrak** | `r33` bagian 28.2-28.3; `LAB-VAL-v1` `r11` `VAL-120`..`VAL-123` |
| **Reuse** | Pola halaman Mikrobiologi per order (`use-lab-microbiology-workspace.jsx:78-79` — `fetchLabOrderDetail`); `usePermission`; pola loading/empty/error `LAB-DEC-069` yang sudah dipakai layar pantau |
| **Cakupan** | Route `lab-monitoring/clinical-pathology/[slug]` beserta `route-token.js`; komposisi halaman; hook dan berkas aturan murni; fungsi service untuk jalur baca per order dan route netral. Lokasi mengikuti `rules/frontend/frontend-architecture.md`; **nama berkas `DEV_DISCRETION`** |
| **Dependency** | `BE-LAB-69` |
| **Acceptance criteria** | `AC-234`, `AC-236`, `AC-237`; bagian antarmuka `AC-219` dan `AC-214` |
| **Verifikasi** | Unit test aturan: `referenceFlag` → `H 6,4` / `L 9,4`, pelestarian isian baris lain, penanganan `409` per baris; halaman dijalankan terhadap backend dengan order berisi pemeriksaan cito dan satu pemeriksaan batal |
| **Risiko/pemilik** | **Sedang.** Tiga jebakan: memanggil `GET /{id}/result` per baris alih-alih jalur per order; menghapus isian baris lain saat satu baris disimpan; dan menurunkan `L`/`H` sendiri dari `normalLow`/`normalHigh` alih-alih memakai `referenceFlag`. Pemilik: implementer frontend |
| **DoD** | Halaman hidup; **nol modal** untuk isian hasil; **nol penanda `KRITIS`**; **nol tombol Validasi/Rilis**; `OutOfReference` **belum** diberi teks final — `FR-14.9`; laporan `FE-LAB-36.md` |

### `FE-LAB-37` — Daftar Kerja membuka halaman order; dialog isi hasil dicabut

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `FE-LAB-36` |
| **Gelombang** | `MVP-8c` |
| **Outcome** | Petugas membuka hasil Patologi Klinik dari Daftar Kerja langsung ke halaman order, dan tidak ada lagi dua tempat mengisi hasil |
| **Requirement/decision** | `FR-14.11`; `LAB-DEC-149`, `LAB-FE-017` |
| **Kontrak** | — (tidak memanggil endpoint baru) |
| **Reuse** | `lab-worklist-view.jsx` dan `use-lab-worklist.jsx` yang sudah berdiri |
| **Cakupan** | Dialog `ConfirmModal` pengisian hasil (`lab-worklist-view.jsx:253-370`) beserta state pendukungnya di hook **dicabut**; setiap baris Patologi Klinik memperoleh aksi membuka halaman ordernya, dapat ditemukan tanpa petunjuk tersembunyi. Urutan cito tetap `LAB-FE-006` |
| **Dependency** | `FE-LAB-36` — dialog dicabut **sesudah** halaman penggantinya berdiri, supaya nol hari tanpa tempat mengisi hasil |
| **Acceptance criteria** | `AC-235` |
| **Verifikasi** | Daftar Kerja dijalankan: nol dialog isi hasil, aksi baris membuka halaman yang benar; uji unit Daftar Kerja yang ada tetap lulus |
| **Risiko/pemilik** | **Rendah.** Jebakannya mencabut dialog lebih dulu dari halaman pengganti. Pemilik: implementer frontend |
| **DoD** | Nol `ConfirmModal` isi hasil di Daftar Kerja; `AC-235` terbukti; laporan `FE-LAB-37.md` |

### Yang sengaja tidak menjadi task frontend

| Yang tidak dijadikan task | Alasan |
|---|---|
| Teks penanda hasil pilihan di luar rujukan | `FR-14.9` masih `OPEN DECISION` |
| Antrean validasi dan tombol Validasi/Rilis | `S4` belum lolos gerbang. **Sejak 2026-09-25 menjadi gelombang `MVP-9`** — `FE-LAB-39`, `FE-LAB-40` |
| Penanda `KRITIS` Patologi Klinik | `S5` |
| Tombol "Final semua" | Tidak diminta keputusan mana pun |

## Gelombang `MVP-9` — `EPIC-LAB-15` validasi dan rilis hasil Patologi Klinik, 2026-09-25

Menurunkan [`03-frontend-architecture.md`](../03-frontend-architecture.md) **amandemen
2026-09-25** dan [`04-prd-to-mvp.md`](../04-prd-to-mvp.md) **bagian 21**.

| Field | Nilai |
|---|---|
| Kontrak | `LAB-API-v1` **`r34`**, `LAB-VAL-v1` **`r12`**, `LAB-PERM-v1` **rev 11**, `LAB-STATE-v1` **`r5`** — `approved` 2026-09-25 |
| Masukan | decisions **rev 74**; `03-frontend-architecture.md` **rev 10**; `04-prd-to-mvp.md` **rev 8** |
| Frontend SHA | `72607a087` (branch `YogaV2`) |
| Hash masukan | Pada [`traceability.md`](traceability.md) bagian *Traceability gelombang `MVP-9`* |
| Nomor task | `FE-LAB-38`..`FE-LAB-40` |
| Gelombang | `MVP-9c` |
| Kerja paralel | Ketiganya **boleh** mulai di atas kontrak `r34` yang approved dan hash-nya tercatat, bersamaan dengan backend `MVP-9a`/`MVP-9b`. **Verifikasinya** tetap menunggu task backend pasangannya |

**Wewenang tampilan.** Penanda pengecualian wajib terlihat sebagai teks (`LAB-FE-004`); tindakan
di halaman hasil per order, bukan di antrean (diturunkan dari `LAB-DEC-149`); route diturunkan
dari konvensi `LAB-FE-001`. **Wujud pertanyaan alasan, konfirmasi sebelum rilis, tab atau
penyaring untuk dua tahap antrean, dan urutan kolom tetap `DEV_DISCRETION`** — roadmap ini tidak
memutuskannya.

**Uji unit** dijalankan dengan `node --import ./tests/helpers/register.mjs --test tests/unit/` —
`npm run test:unit` gagal di Node 20.

### `FE-LAB-38` — Dua layar data induk alasan

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-71` untuk diverifikasi; boleh mulai di atas kontrak `r34` 29.6 |
| **Gelombang** | `MVP-9c` |
| **Outcome** | Kepala instalasi mengisi dan mengelola daftar alasan pengembalian dan daftar alasan pengecualian dari aplikasi — bukan lewat SQL |
| **Requirement/decision** | `FR-15.12`; `LAB-DEC-082`, `LAB-DEC-138`, `LAB-DEC-003`, `LAB-DEC-019` |
| **Kontrak** | `r34` 29.6; `LAB-VAL-v1` `r12` `VAL-140`..`VAL-142`; `LAB-PERM-v1` rev 11 bagian 13.2 |
| **Reuse** | Layar `master-data/lab-organisms` sebagai pola lengkap; sakelar penanda sistem `master-data/lab-rejection-reasons`; `usePermission` |
| **Cakupan** | Route `master-data/lab-result-correction-reasons` dan `master-data/lab-four-eyes-exception-reasons`; daftar berhalaman, penyaring, ringkasan, tambah, ubah, aktif/nonaktif; **kode baca-saja** pada formulir ubah; sakelar *wajib catatan* hanya bagi pemegang `SystemFlag` lewat `PUT /{id}/system-flags`; **nol tombol hapus**; status **hanya** lewat `PATCH /{id}/status`. Dua fungsi service data induk. Lokasi mengikuti `rules/frontend/frontend-architecture.md`; **nama berkas `DEV_DISCRETION`** |
| **Dependency** | `BE-LAB-71` |
| **Acceptance criteria** | Pesan `VAL-140`..`VAL-142` tampil terbaca pada isian yang tepat; sakelar sistem tersembunyi bagi kepala instalasi dan tampil bagi admin |
| **Verifikasi** | Unit test aturan pemetaan galat ke ruas; kedua layar dijalankan terhadap backend `BE-LAB-71` dengan akun kepala instalasi dan akun admin |
| **Risiko/pemilik** | **Rendah.** Jebakannya menyalin ruas *Status Aktif* dari formulir ubah Organisme — permintaan ubah di sini **tidak** membawanya. Pemilik: implementer frontend |
| **DoD** | Kedua layar hidup; nol hapus; kode tak dapat diubah; lint dan build hijau; laporan `task/report/frontend/FE-LAB-38.md` |

### `FE-LAB-39` — Validasi, rilis, dan pengembalian pada Halaman Hasil Patologi Klinik

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `FE-LAB-36` (halaman per order, `MVP-8c`); `BE-LAB-73`..`BE-LAB-76` untuk diverifikasi |
| **Gelombang** | `MVP-9c` |
| **Outcome** | Dokter berkewenangan memvalidasi, perilis merilis, dan keduanya dapat mengembalikan hasil — di halaman yang sama tempat hasil dibaca, dengan pengesah dan penanda pengecualian terlihat |
| **Requirement/decision** | `FR-15.1`..`FR-15.7`, `FR-15.10`, `FR-15.14`; `LAB-FE-004`, `LAB-DEC-003`, `LAB-DEC-120`, `LAB-DEC-138` |
| **Kontrak** | `r34` 29.2, 29.3, 29.5; `LAB-VAL-v1` `r12` 14.1-14.2; `LAB-PERM-v1` rev 11 bagian 13.4 |
| **Reuse** | Halaman `FE-LAB-36` beserta hook dan berkas aturan murninya; `usePermission`; pola penanganan `403`/`409` `FE-LAB-35` |
| **Cakupan** | Per baris: keadaan dari `resultStatus`, *Validasi oleh* dan *Otorisasi oleh* beserta jabatannya, **penanda pengecualian apa adanya dari backend**; tombol Validasi (baris *Final*), Rilis dan *Kembalikan ke analis* (baris *Tervalidasi*) per izin `Validate`/`Release`/`Return`; Reopen tidak ditawarkan pada baris *Tervalidasi*/*Dirilis*; pertanyaan alasan pengecualian **sebelum** mengirim bila `resultEnteredByUserId` atau `validatedByUserId` sama dengan pengguna; alasan pengembalian wajib; label order dari `resultProgress`; penanganan `403` dua lapis, `409`, `422`, `503`; tiga fungsi service |
| **Dependency** | `FE-LAB-36`; `BE-LAB-73`..`BE-LAB-76` |
| **Acceptance criteria** | Bagian antarmuka `AC-02`, `AC-205`, `AC-233`; `LAB-FE-004`; peringatan pengisi; `403` lapis orang tampil pada barisnya |
| **Verifikasi** | Unit test pada `lab-clinical-pathology-result-rules.js`: kapan tiap tombol tampil menurut `resultStatus` dan izin; kapan alasan pengecualian diminta; pelestarian alasan terpilih saat `422`. Halaman dijalankan terhadap backend dengan tiga akun — analis, dokter A, perilis B |
| **Risiko/pemilik** | **Sedang.** Tiga jebakan: menyusun bunyi penanda sendiri; menyembunyikan tombol berdasarkan tebakan penunjukan alih-alih membiarkan backend menjawab `403` bersebab; dan menampilkan penanda **hanya** sebagai warna. Pemilik: implementer frontend |
| **DoD** | Tiga tindakan berjalan dari halaman; penanda terbaca sebagai teks; **nol tombol Validasi/Rilis di luar halaman ini**; laporan `FE-LAB-39.md` |

### `FE-LAB-40` — Antrean validasi

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `FE-LAB-39`; `BE-LAB-77` untuk diverifikasi |
| **Gelombang** | `MVP-9c` |
| **Outcome** | Pemvalidasi dan perilis menemukan hasil yang menunggu mereka tanpa membuka order satu per satu |
| **Requirement/decision** | `FR-15.9`; `LAB-DEC-135` butir 2, `LAB-FE-006`, `LAB-FE-012` |
| **Kontrak** | `r34` 29.4; `LAB-VAL-v1` `r12` `VAL-139` |
| **Reuse** | Route dan layar `lab-worklists/cito-overdue` sebagai pola; `lab-worklist.service.js` |
| **Cakupan** | Route `lab-worklists/validation-queue`; dua tahap *Menunggu Validasi* dan *Menunggu Rilis*; kolom pasien, No. RM, No. order, pemeriksaan, cito, penanda `L`/`H`, menunggu sejak; urutan **dari backend**; aksi baris **membuka Halaman Hasil order itu**; butir menu mengikuti cara `cito-overdue` ditampilkan |
| **Dependency** | `FE-LAB-39` — baris antrean membuka halaman yang sudah dapat bertindak; `BE-LAB-77` |
| **Acceptance criteria** | Bagian antarmuka `AC-196`; nol tombol Validasi/Rilis di antrean |
| **Verifikasi** | Antrean dijalankan terhadap backend: hasil cito di atas; memvalidasi dari halaman lalu kembali → baris pindah ke tahap *Menunggu Rilis* |
| **Risiko/pemilik** | **Rendah.** Jebakannya menambahkan tombol Validasi di antrean demi menghemat klik. Pemilik: implementer frontend |
| **DoD** | Antrean hidup; nol tombol tindakan; laporan `FE-LAB-40.md` |

### Yang sengaja tidak menjadi task frontend

| Yang tidak dijadikan task | Alasan |
|---|---|
| Tombol Validasi dan Rilis di antrean; tombol massal | `03-frontend-architecture.md` amandemen 2026-09-25 — dua tempat bertindak atas satu hasil |
| Cetakan *Validasi oleh* dan *Otorisasi oleh* | `S17` |
| Validasi dan rilis Mikrobiologi dan Patologi Anatomi | `S4d`, `S4e` |
| Penanda `KRITIS` dan formulir pelaporan | `S5` |
| Menyaring hasil belum dirilis bagi pembaca di luar Laboratorium | Nol pembaca hari ini; milik `S17`/`S18` |
