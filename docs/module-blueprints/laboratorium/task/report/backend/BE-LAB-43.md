# Laporan Perubahan Backend — `BE-LAB-43`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-43` (backend) dan `FE-LAB-23` (layar pengisian), dilaporkan bersama |
| Judul | Pengisian hasil pemeriksaan beserta layarnya — slice `S4a` |
| Trace | `LAB-DEC-005`, `LAB-DEC-006`, `LAB-DEC-076`; **menutup `REC3-NEW-002` sepenuhnya** |
| Kontrak | `LAB-API-v1` **`r21`**, **`r22`**, dan **`r23`** — ketiganya `approved` 2026-09-17 |
| Klasifikasi | `HIGH` — menyentuh keselamatan pasien pada batasnya |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-17 |
| Status | **✅ `SELESAI`** — 15 pemeriksaan API + 4 pemeriksaan jalur baca, 1078/1078 uji unit, 5 pemeriksaan layar, 14 regresi; migration diterapkan; nol baris uji tertinggal |

---

## 1. Penahan yang ditelusuri sampai akarnya

`LAB-SIGN-001` menahan `S4` sejak 2026-09-01. Penelusurannya pada 2026-09-17 menemukan ia
menahan **tepat tiga keputusan** — dan ketiganya diberi caveat yang sama, kata demi kata,
*"Menunggu tanda tangan klinis terpisah"*:

| Keputusan | Isi | Caveat |
|---|---|:---:|
| `LAB-DEC-003` | Empat mata pada **validasi dan rilis** | **Ya** |
| `LAB-DEC-004` | **Nilai kritis** dan pelaporannya | **Ya** |
| `LAB-DEC-007` | **Koreksi** setelah rilis | **Ya** |
| `LAB-DEC-005` | **Hasil diketik manual oleh analis** | **Tidak** |
| `LAB-DEC-006` | Batas nilai | **Tidak** — dan **sudah dibangun** sebagai `LabValueBound` |

`LAB-DEC-011` menyebut ketiganya bernama, dan alasannya: *"Ketiga keputusan itu menentukan
perilaku sistem saat hasil salah atau pasien dalam bahaya."*

**`LAB-DEC-005` berdiri setara `LAB-DEC-006`** — dan yang kedua sudah dibangun penuh. Pemilik
modul memecah slicenya lewat `LAB-DEC-076`: `S4a` pengisian berjalan, `S4` validasi dan rilis
tetap tertahan.

> **Nama `S4b` sengaja tidak dipakai** — ia sudah menjadi milik Mikrobiologi. `S4` dipersempit
> di tempatnya, sehingga seluruh rujukan lama kepada "`S4` tertahan" tetap benar.

## 2. Disiplin yang dipertahankan, bukan ditemukan

`LabExaminationStatus` **sudah memuat komentar** yang menyatakan `Pending`, `InProcess`,
`Completed`, `Validated`, dan `Released` sengaja ditahan, karena *"menambahkan statusnya lebih
dulu berarti menjanjikan perilaku yang belum diputuskan pihak klinis"*.

**Disiplin itu dipertahankan apa adanya.** Nol status ditambahkan; enum itu tidak disentuh.

> *"Hasil sudah diisi"* dibaca dari `ResultEnteredAt != null` — sebuah **fakta yang tercatat**,
> bukan **janji tentang apa yang terjadi berikutnya**. Perbedaan itu persis yang memisahkan
> `S4a` dari `S4`.

## 3. Yang dibangun

Tujuh kolom nullable pada `LabExamination`, satu endpoint, satu migration.

| Kolom | Alasan yang tidak terlihat dari namanya |
|---|---|
| `ResultNumeric`, `ResultOptionId` | Tepat satu terisi; bentuknya ditentukan `LabValueBound.ResultForm` |
| `ResultValueBoundId` | **Batas yang berlaku saat itu.** Tanpa ini, Kalium 3,4 yang hari ini di bawah normal dapat menjadi normal besok bila batasnya digeser — **berlaku surut pada hasil yang sudah tercetak** |
| `ResultUnitSnapshot` | Snapshot, sebab yang sama dengan `ProcedureNameSnapshot` |
| **`ExaminedAt`** | **Kapan diperiksa**, bukan kapan diketik. Analis dapat mengerjakan pukul 21.10 dan mengetik pukul 08.05 keesokan harinya — persis alasan `LAB-DEC-042` memisahkan keduanya pada wadah. **Inilah kolom yang ketiadaannya menahan pilihan Tanggal Pemeriksaan (`REC3-NEW-002`)** |
| `ResultEnteredAt`, `ResultEnteredByUserId` | Diturunkan server |

**Empat hal sengaja tidak diterima dari pemanggil:** satuan, batas nilai, waktu pengetikan, dan
pelakunya. Menerimanya berarti mengizinkan sebuah hasil mengaku diperiksa dengan batas yang tidak
pernah berlaku baginya.

### 3.1 Peringatan `BE-EXT-05` diterapkan, dan di sini justru lebih berbahaya

`GetCurrentUserId()` mengembalikan `Guid.Empty` ketika pelakunya tidak dikenali.
`ResultEnteredByUserId` **nol ber-foreign key** — diverifikasi dari `pg_constraint`,
`LabExamination` hanya punya tiga FK dan tidak satu pun menunjuk pengguna, mengikuti
`UrgencyMarkedByUserId` pada entity yang sama.

**Karena itu database tidak akan menolaknya** — dan itulah yang membuatnya lebih berbahaya
daripada kasus `BE-EXT-05`: di sana `Guid.Empty` gagal keras; di sini ia **tersimpan diam-diam
sebagai pelaku yang tidak pernah ada**. Service menulis `null`.

## 4. Verifikasi — 15 pemeriksaan terhadap aplikasi yang berjalan

| Kelompok | Hasil |
|---|---|
| Jalur berhasil `Numeric` dan `Choice`, satuan diturunkan server | 6/6 |
| **Pemilihan batas menurut jenis kelamin** | **2/2** |
| `VAL-77`, `VAL-78`, `VAL-80` (tiga arah), `VAL-81`, `VAL-82` | 7/7 |

### 4.1 Pemeriksaan yang dirancang agar gagal bila implementasinya salah

Hemoglobin pada database punya **dua** batas: **13–17 (laki-laki)** dan **12–15 (perempuan)**.
Pasien pemeriksaan itu **perempuan**.

**Hb 12,5 dikirim.** Bila batasnya dipilih benar, ia **normal**. Bila keliru memilih batas
laki-laki, ia **di bawah normal**. Jawabannya `isOutOfNormalRange = false`, dan baris yang
tersimpan mencatat batas **12,0–15,0**.

Pemeriksaan yang hanya menguji "hasil tersimpan" tidak akan pernah menangkap kekeliruan itu —
angkanya tetap tersimpan, hanya artinya yang salah.

### 4.2 Yang dibuktikan tidak berubah

`ExaminationStatus` ketiga baris uji tetap `2` sesudah hasil diisi. **Nol status disentuh.**

### 4.3 Kebersihan

Ketiga baris uji dikembalikan ke keadaan semula — ketujuh kolom `NULL`. Hitungan ulang dari
koneksi baru: `LabExamination` **7** baris, sisa ber-`ResultEnteredAt` **0**, sebaran
`ExaminationStatus` tetap `2,3`.

## 5. Yang **tidak** dibangun

| Hal | Alasan |
|---|---|
| Validasi, rilis, nilai kritis, koreksi | `LAB-SIGN-001` — `S4` |
| Status hasil | Sama; menambahkannya berarti menjanjikan perilaku yang belum diputuskan |
| Penandaan nilai kritis | `isOutOfNormalRange` adalah **keterangan**, bukan penandaan kritis. `LAB-DEC-004` tertahan |
| Layar pengisian | Task frontend tersendiri. **Endpoint ini karena itu belum punya pemanggil** — lihat bagian 6 |
| Pilihan `Tanggal Pemeriksaan` pada Kategori Periode | Kolomnya kini ada; penyaringnya amandemen tersendiri |

## 6. Batas yang disebut apa adanya

**Endpoint ini belum punya pemanggil.** Layar pengisian hasil adalah task frontend berikutnya,
dan sampai ia berdiri, jalur tulis ini hanya dapat dipakai lewat API.

Ini **bukan** keadaan yang sama dengan `BE-EXT-04`: di sana kolom berdiri tanpa **satu pun cara**
mengisinya, sedangkan di sini jalur tulisnya lengkap dan terbukti bekerja — yang belum ada hanya
layarnya. Dicatat supaya bedanya tidak kabur.

## 7. Langkah berikutnya

| # | Hal | Keadaan |
|---:|---|---|
| 1 | Layar pengisian hasil | Terbuka — nol penahan |
| 2 | Pilihan `Tanggal Pemeriksaan` pada Kategori Periode | Terbuka — kolomnya sudah ada |
| 3 | Validasi, rilis, nilai kritis, koreksi | **`LAB-SIGN-001`** lewat `LAB-REQ-009` → `LAB-REQ-004` |

---

## 8. Susulan pada hari yang sama — jalur baca dan layarnya (`r22`, `FE-LAB-23`)

Bagian 6 mencatat *"endpoint ini belum punya pemanggil"*. Utang itu ditutup pada hari yang sama.

### 8.1 Kenapa jalur baca dibutuhkan, dan kenapa layar tidak boleh menebaknya

Layar harus tahu **bentuk hasil** sebelum menampilkan isian: kotak angka untuk `Numeric`, daftar
pilihan untuk `Choice`. Bentuk itu ditentukan `LabValueBound` yang berlaku bagi **pasien
tertentu** — dibedakan jenis kelamin dan kelompok umur — sehingga **tidak dapat diturunkan dari
jenis pemeriksaannya saja**.

**Menebaknya berarti menampilkan kotak angka untuk pemeriksaan yang hanya menerima pilihan**, dan
petugas baru mengetahuinya sesudah `422` datang.

`r22` menambahkan `GET /lab-examinations/{id}/result` beserta dua ruas waktu pada daftar kerja.
**Jalur pemilihan batasnya dipakai bersama dengan jalur tulis** — dua salinan aturan yang sama
pasti bercabang, dan cabangnya membuat layar menampilkan rujukan yang berbeda dari yang dipakai
menilai hasilnya.

### 8.2 Dua hal yang sengaja TIDAK dikirim

| Yang ditahan | Alasan |
|---|---|
| `criticalLow`, `criticalHigh` | Batas kritis adalah `LAB-DEC-004`, tertahan `LAB-SIGN-001`. Mengirimkannya mengundang layar menandai nilai kritis — dan penandaan itu menjanjikan alur pelaporan yang belum diputuskan |
| `LabValueOption.IsCritical` | Sebab yang sama |

Diverifikasi dari respons nyata: ketiganya **nol kemunculan**.

### 8.3 Dua ruas pada daftar kerja adalah WAKTU, bukan status

`examinedAt` dan `resultEnteredAt`. Tanpa keduanya, daftar kerja tidak dapat membedakan
pemeriksaan yang sudah diisi dari yang belum, dan analis akan mengetik ulang hasil yang sudah ada
tanpa satu pun tanda.

Disiplin `r21` tetap berlaku — dan **ada uji unit yang menjaganya**: keadaan baris dibaca dari
waktu, sehingga uji itu akan gagal lebih dulu bila kelak seseorang menyandarkannya pada status.

### 8.4 Batas `S4a` ditulis di layar, bukan hanya di kode

Dialog memuat keterangan tetap: *"Menyimpan hasil di sini belum memvalidasi maupun merilisnya.
Keduanya menunggu penetapan wewenang klinis."*

Petugas perlu tahu bahwa menekan Simpan **belum** membuat hasilnya sah dikirim ke mana pun. Ada
pemeriksaan layar yang menjaga kalimat itu tetap ada.

### 8.5 Verifikasi

| Yang dijalankan | Hasil |
|---|---|
| `GET /result` pada empat keadaan | **4/4** — `Numeric` dengan rujukan, `Choice` dengan 5 pilihan, gugur dengan alasannya, dan **Hemoglobin membaca 12,0–15,0** yaitu batas yang sama dengan jalur tulis |
| Batas kritis tidak bocor | **3/3** ruas nol kemunculan |
| Daftar kerja membawa kedua waktu | ✅ |
| Uji unit repository | **1078 / 1078** — 11 baru |
| Build produksi frontend | Hijau |
| Pemeriksaan layar `S4a` | **5 / 5** |
| Regresi empat spec layar lain | **14 / 14** |

**Pemeriksaan layar yang paling menentukan:** bentuk `Choice` merender `select` dan **nol**
merender kotak angka; bentuk `Numeric` sebaliknya. Itulah yang membuktikan layar membaca bentuknya
alih-alih menebaknya.

**Satu pemeriksaan memeriksa muatan kunci demi kunci:** `resultNumeric` terkirim, `resultOptionId`
**tidak ada**, dan keempat ruas yang diturunkan backend — `unit`, `resultValueBoundId`,
`resultEnteredAt`, `resultEnteredByUserId` — **nol ikut**.

### 8.6 Satu kekeliruan saya sendiri, dicatat

Spec layar mula-mula membuka `/lab-worklists/pending`; rute yang benar `/lab-worklists`. Kelima
uji gagal pada pemuatan halaman, bukan pada perilakunya. Diperbaiki, dan dicatat karena bentuk
kegagalannya — lima uji gagal serentak di baris yang sama — adalah tanda khas salah rute, bukan
salah logika.

---

## 9. Susulan `r23` — `REC3-NEW-002` ditutup sepenuhnya

### 9.1 Kenapa sekarang, dan bukan pada `r18`

`r18` menolak mencantumkan `ExaminationDate` dengan alasan tertulis: `LabExamination` nol punya
kolom waktu pemeriksaan, sehingga mencantumkannya berarti mendirikan **pilihan yang tidak menuju
ke mana-mana**.

**Dua syarat kini terpenuhi, bukan satu:**

| Syarat | Dipenuhi oleh |
|---|---|
| Kolomnya ada | `LabExamination.ExaminedAt` — `r21` |
| **Ada yang mengisinya** | Jalur tulis `r21` beserta layar pengisiannya `r22` |

**Syarat kedua yang menentukan.** Kolom tanpa penulis adalah persis kesalahan `BE-EXT-04`, dan
mencantumkan pilihannya saat kolomnya baru berdiri akan mengulanginya dari sisi yang berbeda —
layar menawarkan pilihan yang datanya nol mungkin terisi.

### 9.2 Penjagaan `r18` bekerja sebagaimana dirancang

Uji unit dan pemeriksaan layar yang mengunci **ketiadaan** pilihan ketiga adalah **gerbangnya**.
Keduanya diperbarui bersama amandemen ini, dan **perubahan uji itulah buktinya** bahwa
prasyaratnya berubah — bukan dicabut diam-diam supaya kodenya lolos.

Dicatat karena inilah gunanya menguji ketiadaan: ia memaksa penambahan kelak **melewati
pemeriksaan sadar**, bukan menyelinap.

### 9.3 Verifikasi — penyaringnya terbukti menyaring, bukan sekadar menerima

Diuji terhadap aplikasi yang berjalan. **Satu hasil diisi** lewat endpoint `r21` dengan waktu
pemeriksaan **2026-09-10**, lalu penyaringnya dibandingkan:

| Penyaring | Sebelum diisi | Sesudah diisi |
|---|---:|---:|
| `ExaminationDate` rentang setahun | **0** | **1** |
| `OrderDate` rentang setahun | 4 | **4** |
| `ExaminationDate` 2026-09-10 | — | **1** |
| `ExaminationDate` 2026-09-11 | — | **0** |

**Pasangan yang menentukan adalah 1 lawan 4 pada rentang setahun yang sama.** Bila kedua kategori
menyaring pada waktu yang sama, keduanya akan mengembalikan angka yang sama — dan uji yang hanya
memeriksa "`200` dan ada isinya" tidak akan pernah menangkapnya.

`09-11` mengembalikan **0** membuktikan pembedaan harinya bekerja, dan `dateCategory=99` tetap
ditolak `400`.

**Kebersihan:** baris uji dikembalikan `NULL`; hitungan ulang dari koneksi baru —
`LabExamination` **7**, sisa ber-`ExaminedAt` **0**, sisa ber-`ResultEnteredAt` **0**.

| Yang dijalankan | Hasil |
|---|---|
| Build backend | 0 error |
| Uji unit repository | **1079 / 1079** |
| Build produksi frontend | Hijau |
| Pemeriksaan layar lima spec Laboratorium | **19 / 19** |

### 9.4 `REC3-NEW-002` tertutup

Ketiga pilihan yang artifact tawarkan kini berdiri, dan **ketiganya punya sumber data yang
benar-benar dapat terisi**.
