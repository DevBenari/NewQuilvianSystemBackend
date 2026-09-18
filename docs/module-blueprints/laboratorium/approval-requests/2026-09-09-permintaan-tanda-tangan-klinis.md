# Permintaan Tanda Tangan Klinis — Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-004` |
| `tanggal` | 2026-09-09 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `kepada` | **Ditetapkan 2026-09-17 lewat `LAB-REQ-009`** — `DR-LAB-001` **dr. Aditya Pranata, Sp.PK** (Patologi Klinik), `DR-LAB-002` **dr. Nabila Rahmawati, Sp.MK** (Mikrobiologi Klinik), `DR-LAB-003` **dr. Citra Maharani, Sp.PA** (Patologi Anatomi). Sebelumnya tertulis *"dokter penanggung jawab laboratorium atau Komite Medis"* — jabatan tanpa pemegang, dan itu sebabnya dokumen ini menggantung delapan hari |
| `menutup` | `LAB-SIGN-001` — ✅ **tertutup 2026-09-17** oleh `LAB-DEC-079` |
| `induk` | `LAB-DEC-011`; turunan `LAB-REQ-001` butir 6, yang dinyatakan di luar wewenang pemberi persetujuan |
| `rujukan` | `00-interview-decisions.md` revision 21; `blueprint-manifest.md` revision 24 |
| `status` | ✅ **`ditandatangani` — 2026-09-17.** Bagian 2–4 disetujui **apa adanya**, per disiplin, oleh `DR-LAB-001`, `DR-LAB-002`, dan `DR-LAB-003`. `LAB-SIGN-001` **ditutup** (`LAB-DEC-079`). **Bagian 5 dan 6 masih terbuka** — kedua penetapan dan ketiga pertanyaan klinis belum dijawab; lihat bagian 7.2 yang sudah memperingatkannya sejak 2026-09-09 |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| `yang diminta` | **Tanda tangan** atas tiga keputusan, **dua penetapan**, dan **jawaban** atas tiga pertanyaan klinis |

Dokumen ini dapat diteruskan apa adanya kepada pihak klinis. Ia ditulis agar dapat dibaca tanpa
membuka satu pun dokumen desain lainnya.

---

## 0. Satu halaman untuk yang tidak punya waktu

> ### Diperbarui 2026-09-17 — permintaan ini menjadi lebih mendesak, bukan kurang
>
> Dokumen ini ditulis 2026-09-09. Dua hal berubah sejak itu, dan keduanya mengubah **arti**
> permintaannya.
>
> **Pertama, cakupan penahannya ditelusuri sampai akarnya dan ternyata lebih sempit.**
> `LAB-SIGN-001` menahan **tepat tiga keputusan** — ketiga yang menjadi pokok dokumen ini.
> `LAB-DEC-005`, yang menetapkan hasil diketik manual oleh analis, berdiri **tanpa** syarat tanda
> tangan; ia setara `LAB-DEC-006` yang sudah dibangun penuh. Pemilik modul karena itu memecah
> slicenya (`LAB-DEC-076`).
>
> **Kedua, dan inilah yang mengubah keadaan: pengisian hasil sudah berjalan sejak 2026-09-17.**
> Analis kini dapat mengetik hasil ke dalam sistem beserta waktu pemeriksaannya.
>
> **Akibatnya pada permintaan ini:**
>
> | Sebelumnya | Sekarang |
> |---|---|
> | Yang tertahan terbaca sebagai *"hasil laboratorium"* — luas dan kabur | Yang tertahan **persis**: kemampuan **menyatakan sebuah hasil sah** dan mengirimkannya |
> | Hasil dicatat di luar sistem, jadi tidak ada yang menumpuk | **Hasil mulai tersimpan di dalam sistem tanpa satu pun jalan keluar yang sah** |
>
> Baris kedua itu yang mendesak. Semakin lama tanda tangan ini menggantung, semakin banyak hasil
> yang terisi tetapi **tidak ada seorang pun yang berwenang menyatakannya benar** — dan tidak
> ada yang dapat dikirim ke dokter pemesan maupun pasien.

Rumah sakit sedang membangun modul Laboratorium baru. Desainnya sudah selesai, arsitektur dan
kontrak terkunci, dan **seluruh task backend maupun frontend yang tidak menyentuh keselamatan
pasien sudah selesai dikerjakan** — termasuk, sejak 2026-09-17, pengisian hasil.

Yang **belum boleh dibangun** adalah bagian yang menyentuh keselamatan pasien secara langsung:

| Yang tertahan | Artinya bagi pekerjaan sehari-hari |
|---|---|
| **Validasi dan rilis** hasil | **Hasil dapat diisi, tetapi tidak ada yang dapat menyatakannya sah.** Ia tidak dapat dikirim ke dokter pemesan maupun pasien, dan jumlahnya bertambah setiap hari |
| Penandaan dan pelaporan nilai kritis | Pelaporan nilai kritis masih lisan, tanpa catatan yang dapat ditelusuri |
| Koreksi hasil yang sudah dirilis | Belum ada mekanisme resmi memperbaiki hasil yang salah |

Penahannya **bukan** soal teknis dan **bukan** soal anggaran. Penahannya satu hal: pemilik modul
sendiri — lewat keputusan `LAB-DEC-011` — menyatakan bahwa tiga aturan berikut **bukan
wewenangnya**, karena ketiganya menentukan hal yang bersifat klinis:

| Keputusan | Yang ditentukannya |
|---|---|
| `LAB-DEC-003` | **Siapa yang boleh menyatakan sebuah angka hasil benar** |
| `LAB-DEC-004` | **Apa yang terjadi ketika hasil menunjukkan pasien dalam bahaya** |
| `LAB-DEC-007` | **Apa yang terjadi ketika hasil yang sudah dipakai dokter ternyata salah** |

Ketiganya sudah dirumuskan lengkap dan sudah disetujui dari sisi produk serta operasional.
**Yang diminta bukan merancangkan aturannya** — melainkan pernyataan pihak klinis bahwa aturan
itu memang benar, atau koreksinya bila keliru.

> ### Hambatan sebelum hambatan: penanda tangannya belum ditetapkan
>
> `blueprint-manifest.md` mencatat `clinical_governance: belum ditetapkan`. Artinya sampai hari
> ini **belum ada nama** yang ditunjuk rumah sakit sebagai pemegang wewenang klinis untuk modul
> ini.
>
> Karena itu langkah pertama bukan menandatangani, melainkan **menetapkan siapa yang berwenang
> menandatangani** — dokter penanggung jawab laboratorium, atau Komite Medis sebagai lembaga.
> Selama nama itu belum ada, dokumen ini tidak punya tujuan yang sah, dan `LAB-SIGN-001` tidak
> dapat ditutup oleh siapa pun.
>
> **Diperiksa ulang 2026-09-17, dan yang hilang ternyata hanya satu hal — bukan seluruhnya.**
> Dibaca langsung dari data induk sistem:
>
> | Hal | Keadaan |
> |---|---|
> | Departemen tempat laboratorium bernaung | ✅ **`Penunjang Medis`** sudah ada |
> | Jabatan analis yang mengetik hasil | ✅ **`Analis Laboratorium`** sudah ada |
> | Jabatan dokter | ✅ `Dokter Umum`, `Dokter Spesialis`, `Dokter IGD` — di bawah `Medis` |
> | Jabatan pemegang wewenang klinis laboratorium | ✅ **`Dokter Penanggung Jawab Laboratorium`** — didirikan 2026-09-17 atas instruksi pemilik modul (`LAB-DEC-077`), kode `POS-PMJ-004`, di bawah `Penunjang Medis` |
> | **Orang yang memegangnya** | ❌ **Nol.** Jabatannya ada; kursinya kosong |
>
> **Yang tersisa karena itu tinggal satu hal, dan ia bukan pekerjaan perangkat lunak: sebuah
> nama.** Sistem kini punya tempat mencatatnya; yang belum ada adalah orang yang ditunjuk rumah
> sakit untuk mendudukinya.
>
> **Satu hal perlu ditegaskan supaya tidak salah harap:** menambahkan baris jabatan **tidak**
> membuat tanda tangan ini dapat diberikan. Penelusuran source menemukan **nol kode** yang
> membaca konsep Clinical Governance — tanda tangan ini tindakan manusia atas sebuah dokumen,
> dan yang dibutuhkannya **sebuah nama**, bukan sebuah baris database. Baris jabatan baru
> dibutuhkan kelak, ketika aturan yang ditandatangani ini diterjemahkan menjadi hak akses.
>
> Permohonan penetapannya diajukan terpisah lewat **`LAB-REQ-009`** pada 2026-09-17, ditujukan
> kepada manajemen rumah sakit.
>
> ---
>
> ### ✅ Hambatan ini SUDAH LEWAT — 2026-09-17, hari yang sama
>
> `LAB-REQ-009` **dijawab**. Dokumen ini kini punya tujuan yang sah, dan itulah satu-satunya
> hal yang berubah.
>
> | Kode | Nama | Disiplin yang dipegangnya |
> |---|---|---|
> | `DR-LAB-001` | **dr. Aditya Pranata, Sp.PK** | Patologi Klinik |
> | `DR-LAB-002` | **dr. Nabila Rahmawati, Sp.MK** | Mikrobiologi Klinik |
> | `DR-LAB-003` | **dr. Citra Maharani, Sp.PA** | Patologi Anatomi |
>
> Dicatat sebagai `LAB-DEC-078`. Ketiganya menutup persis ketiga disiplin scope `LAB-DEC-025`.
>
> **Satu hal berpindah kepada pembaca dokumen ini, dan sebaiknya diputuskan lebih dulu.**
> Penetapannya **per disiplin**, sedangkan ketiga keputusan di bawah dirumuskan **lintas
> disiplin** — `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` berlaku bagi Patologi Klinik,
> Mikrobiologi, dan Patologi Anatomi sekaligus. Karena itu ada dua cara menandatanganinya, dan
> dokumen ini **tidak memilih** salah satunya:
>
> | Cara | Artinya |
> |---|---|
> | **Bersama** — ketiganya menandatangani satu naskah | Satu aturan berlaku seragam bagi ketiga disiplin. Paling sederhana bagi sistem |
> | **Sendiri-sendiri** — masing-masing menandatangani untuk disiplinnya | Aturannya boleh berbeda antardisiplin. Berakibat pada `LAB-PERM-v1` dan pada bentuk `S4`/`S4b`/`S4c`, dan itu keputusan yang perlu dinyatakan, bukan disimpulkan |
>
> **Dijawab pada hari yang sama: `sendiri-sendiri`.** Masing-masing menandatangani untuk
> disiplinnya, dan aturannya **boleh** berbeda antardisiplin. Lihat bagian 9.2.
>
> ---
>
> ### ✅ DAN TANDA TANGANNYA TURUN — 2026-09-17, hari yang sama
>
> Ketiga keputusan di bawah **disetujui apa adanya**, per disiplin. `LAB-SIGN-001` **ditutup**
> setelah 16 hari. Dibukukan `LAB-DEC-079`.
>
> **Tiga hal perlu dibaca tepat, dan ketiganya mudah salah harap.**
>
> **Pertama, isi aturannya tidak berubah.** Jalur *"disetujui dengan perubahan"* pada bagian 8
> tidak ditempuh. Maka hari ini ketiga naskah berlaku **identik** bagi Patologi Klinik,
> Mikrobiologi, dan Patologi Anatomi. Yang per disiplin adalah **wewenangnya**, bukan isinya —
> dan akibatnya baru terasa nanti: amandemen aturan Mikrobiologi kelak cukup ditandatangani
> `DR-LAB-002` sendiri.
>
> **Kedua, bagian 5 dan 6 TIDAK ikut terjawab.** Bagian 7.2 dokumen ini sudah memperingatkannya
> sejak 2026-09-09, dan peringatan itu terbukti tepat pada hari pertama.
>
> **Ketiga, satu pertanyaan baru lahir dari bentuk tanda tangannya** — `LAB-OPEN-034`, apakah
> kewenangan validasi dan rilis melintasi disiplin. Lihat bagian 7.2.

**Waktu yang dibutuhkan.** Bagian 2 sampai 4 memuat tiga keputusan yang perlu dibaca dan
ditandatangani. Bagian 5 dan 6 memuat lima hal tambahan yang sebaiknya dijawab pada kesempatan
yang sama, karena kelimanya juga wewenang klinis dan akan menjadi permintaan terpisah bila
dilewatkan.

---

## 1. Kenapa permintaan ini tidak dapat ditutup pihak lain

Permintaan ini pernah diajukan ke pemilik repository lewat `LAB-REQ-001` butir 6 pada
2026-09-01, dan **ditolak dengan benar** — bukan karena isinya, melainkan karena wewenangnya.

Catatan penolakan itu berbunyi:

> *"`andryzainhome` dan `sukmagp` adalah pemilik repository, bukan wewenang klinis. Menutup
> `LAB-SIGN-001` atas persetujuan mereka justru melanggar keputusan yang dibuat pemilik modul
> Laboratorium sendiri."*

Alasannya bukan formalitas. **Bila kelak terjadi insiden pada pasien akibat hasil laboratorium,
rumah sakit perlu dapat menunjukkan bahwa pihak klinis ikut memutuskan aturannya** — bukan
bahwa aturan keselamatan ditetapkan oleh tim perangkat lunak.

Perlu dicatat jujur: ketiga keputusan yang dibawa dokumen ini **sudah dirumuskan dan disetujui
sisi produknya**. Ada risiko dokumen seperti ini dibaca sebagai permintaan stempel. Ia bukan.
Setiap bagian di bawah memuat kotak **"Yang perlu Anda putuskan"** dengan pilihan menyetujui,
mengubah, atau menolak — dan bagian 8 menjelaskan apa yang terjadi pada tiap pilihan itu.

---

## 2. Butir 1 — `LAB-DEC-003`: prinsip empat mata pada validasi hasil

### 2.1 Bunyi aturan yang diajukan

Petugas yang mengetik angka hasil **tidak boleh** menjadi petugas yang memvalidasi dan merilis
hasil yang sama. Sistem menolak percobaan itu pada keadaan normal.

**Jalur pengecualian.** Bila keadaan memaksa — misalnya shift malam hanya ada satu analis —
sistem tetap mengizinkan, dengan tiga syarat wajib:

1. Petugas mengisi alasan pengecualian dari daftar alasan yang terkendali.
2. Hasil diberi penanda permanen **"divalidasi oleh pengisi sendiri"**.
3. Penanda itu ikut tercetak pada lembar hasil dan tersimpan di riwayat yang tidak dapat diubah.

### 2.2 Wujudnya di layar

> Analis Sari bertugas malam sendirian. Pukul 23.40 ia mengetik hasil Natrium pasien Andi
> sebesar 128 mmol/L, lalu menekan tombol Validasi. Sistem menahan dan menampilkan pesan:
> *"Anda yang mengisi hasil ini. Validasi oleh orang yang sama memerlukan alasan."*
>
> Sari memilih alasan *"Shift tunggal, tidak ada validator lain bertugas"* dan menekan Lanjut.
> Hasil dirilis pukul 23.42, dan pada lembar hasil tercetak keterangan *"Divalidasi oleh pengisi
> sendiri — Sari — Shift tunggal"*. Keesokan paginya kepala instalasi melihat hasil ini pada
> daftar pantau pengecualian.

### 2.3 Kenapa diperlakukan sebagai aturan keselamatan

Kesalahan ketik satu digit pada Kalium — 3,5 diketik menjadi 7,5 — dapat membuat dokter
memberikan terapi yang salah. Rilis pertama modul ini **tidak** tersambung ke alat
laboratorium; seluruh angka diketik manusia (`LAB-DEC-005`). Prinsip empat mata adalah
satu-satunya penyaring salah ketik yang dimiliki rilis pertama.

### 2.4 Yang diuji sistem bila aturan ini disahkan

| No | Kriteria |
|---|---|
| `AC-01` | Petugas pengisi hasil yang menekan Validasi pada hasil yang sama ditolak sistem, kecuali ia mengisi alasan pengecualian dari daftar terkendali |
| `AC-02` | Hasil yang divalidasi lewat jalur pengecualian menampilkan penanda "divalidasi oleh pengisi sendiri" di layar, di cetakan, dan di riwayat |

### 2.5 Yang perlu Anda putuskan

| Pertanyaan | Jawaban yang diminta |
|---|---|
| Apakah prinsip empat mata sudah tepat sebagai aturan wajib? | Setuju / Ubah / Tolak |
| Apakah jalur pengecualian boleh ada sama sekali? | Ya / Tidak — **bila tidak, hasil pada shift tunggal tidak akan pernah dapat dirilis** |
| Siapa yang berhak memakai jalur pengecualian? | Semua pemegang kewenangan validasi / hanya jabatan tertentu / lainnya |
| Alasan pengecualian apa saja yang sah? | Daftar terkendali — mohon disebutkan bila ada tambahan selain "shift tunggal" |

---

## 3. Butir 2 — `LAB-DEC-004`: nilai kritis tetap dirilis, pelaporan wajib tercatat

### 3.1 Bunyi aturan yang diajukan

Hasil bernilai kritis **tetap dirilis** agar dokter segera melihatnya. Namun pemeriksaan itu
**belum dianggap tuntas** sampai catatan pelaporan terisi lengkap:

| Isian wajib | Contoh |
|---|---|
| Siapa yang melapor | Analis Sari |
| Kepada siapa | dr. Rina, DPJP pasien Andi |
| Kapan | 2026-09-01 pukul 23.45 |
| Lewat apa | Telepon |
| Bukti pembacaan ulang | dr. Rina mengulang *"Kalium tujuh koma dua"*, dicentang Sari |

**Aturan turunan.** Sistem menyediakan daftar pantau berisi hasil kritis yang **belum**
dilaporkan, agar kepala instalasi dapat menegur sebelum menjadi insiden.

### 3.2 Wujudnya di layar

> Hasil Kalium pasien Andi keluar 7,2 mmol/L pukul 23.44, sedangkan batas kritis atas yang
> ditetapkan adalah 6,0 mmol/L. Sistem langsung merilis hasil sehingga dr. Rina dapat
> membukanya, sekaligus memunculkan formulir pelaporan yang harus diisi Sari. Selama formulir
> itu kosong, pemeriksaan Kalium tetap muncul pada daftar *"nilai kritis belum dilaporkan"*.

Bila hasilnya 5,3 — di atas normal tetapi belum kritis — sistem hanya menandainya *"di atas
nilai rujukan"* tanpa memunculkan formulir pelaporan.

### 3.3 Pilihan yang diambil, dan alasannya

Ada dua kemungkinan yang lazim: **menahan** hasil kritis sampai dilaporkan, atau **merilisnya**
lalu mewajibkan pencatatan. Rancangan ini memilih yang kedua.

| Pilihan | Untungnya | Ruginya |
|---|---|---|
| Menahan hasil sampai dilaporkan | Pelaporan pasti terjadi | **Memperlambat penanganan pasien yang justru sedang dalam bahaya** |
| **Merilis, pelaporan dicatat** (dipilih) | Dokter melihat angka secepat mungkin | Pelaporan bergantung disiplin petugas; ditutup oleh daftar pantau |

**Ini titik yang paling perlu penilaian klinis Anda.** Bila kebijakan rumah sakit menghendaki
hasil kritis ditahan lebih dulu, aturan ini harus diubah sekarang — bukan setelah dibangun.

### 3.4 Yang diuji sistem bila aturan ini disahkan

| No | Kriteria |
|---|---|
| `AC-03` | Hasil melewati batas kritis tetap berpindah ke status `Released`, dan sekaligus memunculkan formulir pelaporan wajib |
| `AC-04` | Selama formulir pelaporan belum terisi lengkap, pemeriksaan itu muncul pada daftar pantau "nilai kritis belum dilaporkan" |
| `AC-05` | Hasil di luar batas normal tetapi belum kritis ditandai "di atas/di bawah nilai rujukan" **tanpa** formulir pelaporan |
| `AC-14` | Pemberitahuan nilai kritis tetap tersimpan dan terbaca dokter meskipun ia tidak sedang membuka aplikasi saat hasil keluar |
| `AC-29` | Hasil berbentuk pilihan — Protein urin `+4` — memicu formulir pelaporan persis seperti Kalium 7,2 mmol/L |

### 3.5 Yang perlu Anda putuskan

| Pertanyaan | Jawaban yang diminta |
|---|---|
| Hasil kritis dirilis lebih dulu, atau ditahan sampai dilaporkan? | **Dirilis** (usulan) / Ditahan |
| Apakah kelima isian pelaporan sudah cukup? | Cukup / Tambahkan: … |
| Apakah bukti pembacaan ulang wajib, atau boleh opsional? | Wajib (usulan) / Opsional |
| Sarana pelaporan apa saja yang sah? | Telepon / lisan langsung / lainnya |

---

## 4. Butir 3 — `LAB-DEC-007`: koreksi hasil setelah rilis

### 4.1 Bunyi aturan yang diajukan

1. Koreksi hasil yang sudah dirilis **hanya** boleh dilakukan petugas yang memegang kewenangan
   validasi atau rilis. Analis biasa tidak boleh.
2. Begitu hasil perbaikan dirilis ulang, dokter pemesan **otomatis** mendapat pemberitahuan
   bahwa hasil pasiennya berubah.
3. Versi hasil yang lama **tetap terlihat**, diberi tanda *"sudah diperbaiki"*, dan tidak
   dihapus.

### 4.2 Wujudnya di layar

> Hasil Hemoglobin pasien Andi dirilis 9,4 g/dL pukul 10.00. Pukul 14.00 ketahuan angka yang
> benar adalah 4,9 g/dL karena tertukar dengan sampel lain. Tono, yang berwenang validasi,
> membuat koreksi. Sistem menyimpan versi lama 9,4 dengan tanda *"sudah diperbaiki"*, merilis
> versi baru 4,9, lalu mengirim pemberitahuan ke dr. Rina.
>
> dr. Rina yang tadinya menganggap pasien tidak perlu transfusi kini tahu keadaan sebenarnya.

### 4.3 Bila kunjungan pasien sudah ditutup

Rekam medis mengunci dokumen begitu kunjungan ditutup. Untuk hal itu berlaku aturan turunan
`LAB-DEC-020`: dokumen asli **tetap terkunci dan tidak diubah**, dan hasil perbaikan menempel
sebagai **addendum bertanda tangan beralasan** — memakai mekanisme addendum rekam medis yang
sudah ada, bukan mekanisme baru.

### 4.4 Yang diuji sistem bila aturan ini disahkan

| No | Kriteria |
|---|---|
| `AC-06` | Analis biasa yang mencoba mengoreksi hasil berstatus `Released` ditolak sistem |
| `AC-07` | Setelah koreksi dirilis ulang, versi lama tetap terlihat bertanda "sudah diperbaiki", dan dokter pemesan menerima pemberitahuan |
| `AC-16` | Pemberitahuan koreksi terkirim ke dokter pemesan setiap kali hasil perbaikan dirilis ulang |
| `AC-27` | Koreksi pada kunjungan yang sudah ditutup menghasilkan addendum bertanda tangan beralasan, sementara dokumen aslinya tetap terkunci |

### 4.5 Yang perlu Anda putuskan

| Pertanyaan | Jawaban yang diminta |
|---|---|
| Apakah pemegang kewenangan validasi/rilis cukup, atau koreksi memerlukan wewenang lebih tinggi? | Cukup (usulan) / Perlu wewenang khusus |
| Apakah dokter pemesan saja yang diberi tahu, atau juga DPJP dan unit perawatan? | Mohon disebutkan |
| Apakah ada batas waktu setelahnya hasil tidak boleh dikoreksi lagi? | Tidak ada batas (usulan) / Ada, yaitu … |
| Apakah koreksi perlu alasan wajib dari daftar terkendali? | Ya / Tidak |

---

## 5. Dua penetapan yang membuat ketiga aturan itu dapat dijalankan

Tanda tangan atas ketiga keputusan di atas belum cukup membuatnya berjalan. Dua hal berikut
adalah **penetapan rumah sakit**, bukan rancangan perangkat lunak, dan tanpa keduanya aturan
yang ditandatangani akan kosong dalam praktiknya.

### 5.1 Jaminan dua pemegang kewenangan validasi per shift (`LAB-DEC-022`)

Rancangan menyatakan rumah sakit menjamin **setiap shift memiliki sekurang-kurangnya dua orang
pemegang kewenangan validasi**, dan sistem memperingatkan kepala instalasi bila suatu shift
hanya punya satu.

**Kenapa ini penting, dan bukan sekadar administrasi:**

> Bila sebuah shift hanya punya satu pemegang kewenangan validasi, maka setiap hasil yang ia
> kerjakan sendiri akan lewat jalur pengecualian butir 1. Dalam sebulan, "pengecualian" itu
> berubah menjadi kebiasaan, dan prinsip empat mata berhenti berarti apa pun — padahal
> pengujiannya tetap lulus dan tidak ada aturan yang dilanggar.

Kewenangan validasi diberikan **per orang, bukan per jabatan**: seorang analis senior boleh
memegangnya, seorang kepala ruangan boleh tidak. Sistem tidak menetapkan siapa yang layak; ia
hanya menegakkan penetapan yang dibuat rumah sakit.

| Yang diminta | Jawaban |
|---|---|
| Apakah rumah sakit dapat menjamin dua pemegang kewenangan validasi per shift? | Ya / Tidak — **bila tidak, jalur pengecualian akan menjadi jalur utama pada shift tertentu, dan itu perlu disadari sejak awal** |
| Siapa yang berwenang menetapkan pemegang kewenangan validasi? | Kepala instalasi / Komite Medis / lainnya |

### 5.2 Siapa pemberi "persetujuan klinis" atas perubahan batas kritis (`LAB-DEC-023`)

Batas kritis adalah angka yang menentukan kapan seorang pasien dinyatakan dalam bahaya.
Rancangan membedakan perlindungannya dari batas normal:

| Yang diubah | Siapa yang boleh | Perlu persetujuan klinis |
|---|---|:---:|
| Satuan hasil | Kepala instalasi | Tidak |
| Batas normal bawah dan atas | Kepala instalasi | Tidak |
| **Batas kritis bawah dan atas** | Kepala instalasi mengajukan | **Ya** |
| **Penanda pilihan yang dianggap kritis** | Kepala instalasi mengajukan | **Ya** |
| Batas waktu penyelesaian cito | Kepala instalasi | Tidak |

**Keadaan yang dicegah aturan ini:**

> Kepala instalasi merasa terlalu banyak peringatan nilai kritis mengganggu pekerjaan harian,
> lalu menaikkan batas kritis atas Kalium dari 6,0 menjadi 8,0. Sejak saat itu pasien dengan
> Kalium 7,2 mmol/L tidak lagi memicu kewajiban pelaporan. Tidak ada aturan yang dilanggar dan
> tidak ada yang menyadarinya.

**Yang belum ada:** rancangan menyebut "persetujuan klinis" sebagai syarat, tetapi **belum ada
nama atau peran** yang mengisinya. Selama itu kosong, `AC-33` tidak dapat dibangun — pengajuan
perubahan batas kritis akan tertahan selamanya tanpa ada yang berhak menyetujuinya.

| Yang diminta | Jawaban |
|---|---|
| Siapa pemegang wewenang menyetujui perubahan batas kritis? | Jabatan atau nama: … |
| Apakah wewenang itu boleh didelegasikan? | Ya, kepada … / Tidak |
| Apakah persetujuan cukup satu orang, atau perlu rapat Komite Medis? | Satu orang / Rapat |

---

## 6. Tiga pertanyaan klinis yang sekalian dimintakan jawabannya

Ketiga hal berikut **belum pernah diputuskan** dan tercatat sebagai penghambat terbuka.
Semuanya menyebut Clinical Governance sebagai pemegang wewenang — orang yang sama dengan
penanda tangan dokumen ini. Diajukan bersama agar tidak menjadi tiga permintaan terpisah di
kemudian hari.

### 6.1 `LAB-P0-004` — alur nilai kritis selengkapnya

Butir 2 mengunci **bahwa** nilai kritis wajib dilaporkan dan dicatat. Yang belum diputuskan
adalah seluk-beluknya:

| Yang belum ada jawabannya | Contoh bentuk jawaban |
|---|---|
| Berapa batas waktu tanggap sejak hasil kritis keluar sampai dilaporkan? | 30 menit / 60 menit / lainnya |
| Apa yang terjadi bila dokter penerima tidak dapat dihubungi? | Eskalasi ke siapa, setelah berapa lama, sampai tingkat mana |
| Apakah ada eskalasi berjenjang? | Ke DPJP → dokter jaga → kepala instalasi → … |
| Bukti penerimaan apa yang dianggap sah selain pembacaan ulang? | … |

Tanpa jawaban ini, daftar pantau nilai kritis dapat dibangun, tetapi **tidak dapat menandai
mana yang sudah terlambat** — karena tidak ada batas waktu yang disepakati.

### 6.2 `LAB-OPEN-014` — nilai kritis pada mikrobiologi dan patologi anatomi

Rilis ini menangani tiga disiplin: Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Hasilnya
punya empat bentuk: angka bersatuan, pilihan terbatas, mikrobiologi berstruktur, dan narasi
Patologi Anatomi.

Penandaan nilai kritis bekerja dengan membandingkan angka terhadap batas. **Hasil mikrobiologi
dan narasi patologi anatomi tidak dapat dinilai dengan cara itu.**

| Yang diminta | Contoh |
|---|---|
| Apakah hasil mikrobiologi mengenal nilai kritis? | Misalnya kultur darah positif, atau kuman tertentu yang wajib dilaporkan segera |
| Bila ya, apa penanda kritisnya? | Daftar organisme / pola resistensi / lainnya |
| Apakah narasi patologi anatomi mengenal temuan yang wajib dilaporkan segera? | Ya, yaitu … / Tidak |

Bila jawabannya **tidak untuk keduanya**, itu pun jawaban yang sah dan menutup pertanyaan ini —
sistem cukup membatasi alur nilai kritis pada hasil berbentuk angka dan pilihan.

### 6.3 `LAB-P0-001` — sisa matriks kewenangan per peran

`LAB-DEC-022` sudah menjawab kewenangan **validasi** dan **rilis**. Yang belum:

| Tindakan | Siapa yang boleh |
|---|---|
| Menerima wadah sampel | … |
| Memproses sampel | … |
| Membatalkan pesanan | … |
| Menghapus pesanan | … |
| Mengisi hasil | … |
| Mengoreksi hasil yang sudah diotorisasi | Sebagian dijawab butir 3; mohon dipastikan |

---

## 7. Apa yang terbuka setelah ditandatangani — dan apa yang tetap tertahan

### 7.1 Yang langsung terbuka

Penelusuran ulang 2026-09-09 menemukan bahwa dua penahan lain yang dahulu tercatat bersama
`LAB-SIGN-001` — `LAB-COORD-001` (kemampuan pemberitahuan) dan `LAB-COORD-002` (jenis dokumen
klinis baru) — **sudah ditutup pada 2026-09-01** lewat persetujuan `LAB-REQ-001` butir 7 dan 8.

Akibatnya `LAB-SIGN-001` kini **satu-satunya penahan** bagi bagian-bagian berikut:

| Bagian | Isinya | Keadaan |
|---|---|---|
| ~~`S4a`~~ | ~~**Pengisian** hasil Patologi Klinik~~ | ✅ **Sudah berjalan 2026-09-17.** Dipecah `LAB-DEC-076` sesudah terbukti `LAB-DEC-005` berdiri tanpa syarat tanda tangan |
| `S4` | **Validasi dan rilis** hasil Patologi Klinik | ✅ **Terbuka 2026-09-17** — `DR-LAB-001` |
| `S4b` | Hasil mikrobiologi | ✅ **Terbuka** — `DR-LAB-002`. Sisa penahannya `LAB-OPEN-017` (makna penanda Definitif) |
| `S4c` | Hasil patologi anatomi | ✅ **Terbuka** — `DR-LAB-003` |
| `S5` | Penandaan dan pelaporan nilai kritis | 🟡 **Kerangkanya terbuka, daftarnya belum** — `LAB-P0-004` dan `LAB-OPEN-014` tetap menahan, lihat 7.2 |
| `S6` | Koreksi hasil setelah rilis | 🟡 **Terbuka, dengan sisa** — `LAB-P0-003` (aturan pembatalan dan koreksi) tetap menahan |

> **"Terbuka" di sini berarti boleh dirancang, bukan sudah dapat dibangun.** Bagian 5 dokumen
> ini sudah menyatakannya sejak 2026-09-09: tanda tangan tidak membuat fiturnya ada, ia membuat
> perancangannya boleh dimulai. Urutannya tetap — arsitektur domain, lalu amandemen kontrak,
> baru task. **Nol di antara ketiganya dikerjakan pada hari penandatanganan.**
>
> **Dan satu hal tidak berpindah sama sekali:** masuknya kelima slice ke dalam
> `scope.slices_in_scope` adalah **keputusan rilis milik pemilik modul**, bukan akibat otomatis
> tanda tangan ini. Manifest karena itu **tidak** digeser sendiri.

> **Baris pertama itu yang mengubah keadaan.** Pengisian hasil keluar dari daftar ini bukan
> karena dilewati, melainkan karena terbukti **tidak pernah** diatur ketiga keputusan yang
> dimintakan tanda tangannya. Yang tersisa di bawahnya adalah persis yang memang bersifat klinis
> — dan kini hasil sudah mulai tersimpan menunggu salah satunya.

> **Catatan pembukuan.** `04-prd-to-mvp.md` bagian 7 masih menyebut `LAB-COORD-001` dan
> `LAB-COORD-002` sebagai penahan `S5` dan `S6`. Catatan itu tertinggal dari keadaan
> sebenarnya dan **sudah dikoreksi 2026-09-09**. `roadmap/frontend-roadmap.md` sejak semula
> sudah benar. Yang berlaku adalah baris `active_blockers` pada `blueprint-manifest.md`.

### 7.2 Yang tetap tertahan meski tanda tangan diberikan

Supaya tidak salah harap:

| Bagian | Masih menunggu |
|---|---|
| Daftar pantau nilai kritis yang **lengkap dengan batas waktu dan eskalasi** | Jawaban `LAB-P0-004` — bagian 6.1 |
| Nilai kritis untuk mikrobiologi dan patologi anatomi (`S4c`) | Jawaban `LAB-OPEN-014` — bagian 6.2 |
| Penegakan kewenangan di luar validasi dan rilis | Jawaban `LAB-P0-001` — bagian 6.3 |
| Persetujuan perubahan batas kritis (`AC-33`) | Penetapan bagian 5.2 |
| **Apakah kewenangan validasi/rilis melintasi disiplin** | `LAB-OPEN-034` — **baru, lahir 2026-09-17** dari bentuk tanda tangannya |

Karena itu bagian 5 dan 6 diajukan bersama tanda tangannya. Menandatangani bagian 2–4 saja sudah
membuka `S4`, `S4b`, dan sebagian besar `S5` dan `S6` — tetapi menyisakan empat hal yang akan
kembali menahan pekerjaan beberapa minggu kemudian.

> ### Peringatan ini terbukti tepat pada hari pertama — 2026-09-17
>
> Bagian 2–4 ditandatangani; **bagian 5 dan 6 tidak dijawab**. Keempat baris di atas karena itu
> berlaku persis sebagaimana ditulis sembilan hari lalu, dan satu baris kelima menyusul.
>
> **Baris kelima itu yang paling pantas diperhatikan.** Tanda tangannya berbentuk **per
> disiplin**, dan bentuk itu melahirkan pertanyaan yang tidak pernah ada sebelumnya: bolehkah
> seorang validator Patologi Klinik memvalidasi hasil Mikrobiologi? Ia tidak sekadar soal hak
> akses. `LAB-DEC-022` menjamin **dua pemegang kewenangan validasi tersedia per shift** — dan
> jaminan itu menjadi **jauh lebih mahal** bila harus dipenuhi per disiplin, bukan per
> laboratorium. Sebuah laboratorium dengan satu ahli Patologi Anatomi yang sedang cuti berarti
> `S4c` berhenti sepenuhnya.
>
> Jawabannya wewenang ketiga penanda tangan bersama pemilik modul, dan **tidak diturunkan
> sendiri di sini**. Dicatat `LAB-OPEN-034`.

---

## 8. Bila keputusan ini diubah atau ditolak

Dokumen ini bukan permintaan stempel. Ketiga pilihan berikut sama-sama sah dan sama-sama menutup
`LAB-SIGN-001`:

| Pilihan | Yang terjadi |
|---|---|
| **Disetujui apa adanya** | `LAB-SIGN-001` ditutup. Ketiga keputusan naik dari `approved` oleh pemilik modul menjadi disahkan klinis. Lima slice terbuka |
| **Disetujui dengan perubahan** | Perubahan dicatat sebagai keputusan baru yang mengamandemen aslinya, `AC` terkait disesuaikan, lalu `LAB-SIGN-001` ditutup. **Ini jalur yang paling diharapkan** — lebih baik diubah sekarang daripada setelah dibangun |
| **Ditolak** | Aturan penggantinya dimintakan. Bagian hasil pemeriksaan tetap tertahan sampai ada aturan pengganti, tetapi tidak ada pekerjaan yang terbuang: seluruh bagian lain modul sudah selesai dan tidak bergantung pada ketiganya |

Yang **tidak** dapat dilakukan adalah membiarkannya tanpa jawaban. Sepanjang itu terjadi, hasil
laboratorium tetap dicatat di luar sistem dan pelaporan nilai kritis tetap tidak dapat
ditelusuri — keadaan yang berjalan hari ini.

---

## 9. Halaman tanda tangan

### 9.1 Penetapan pemegang wewenang klinis

> ✅ **Sudah terisi 2026-09-17 lewat `LAB-REQ-009`.** Bagian ini tidak perlu diisi ulang.

| Field | Isian |
|---|---|
| Nama pemegang wewenang Clinical Governance modul Laboratorium | `DR-LAB-001` **dr. Aditya Pranata, Sp.PK** — Patologi Klinik<br>`DR-LAB-002` **dr. Nabila Rahmawati, Sp.MK** — Mikrobiologi Klinik<br>`DR-LAB-003` **dr. Citra Maharani, Sp.PA** — Patologi Anatomi |
| Jabatan | Dokter spesialis penanggung jawab disiplin masing-masing; padanan data induknya `Dokter Penanggung Jawab Laboratorium` (`POS-PMJ-004`) |
| Ditetapkan oleh | *(belum disampaikan — lihat `LAB-REQ-009` bagian 7.1 butir 2)* |
| Tanggal penetapan | *(belum disampaikan)* — diterima pengaju 2026-09-17 |

### 9.2 Tanda tangan atas ketiga keputusan

> **Terisi 2026-09-17.** Penetapannya per disiplin, ketiga keputusan di bawah lintas disiplin,
> sehingga cara menandatanganinya perlu dinyatakan. Yang dipilih:
>
> - ☐ Ketiga penanda tangan menyetujui **satu naskah yang sama** — aturannya seragam bagi Patologi Klinik, Mikrobiologi, dan Patologi Anatomi.
> - ☑ **Masing-masing menandatangani untuk disiplinnya sendiri** — aturannya **boleh** berbeda antardisiplin.
>
> **Dan yang perlu dicatat justru apa yang TIDAK menyertainya:** nol perbedaan antardisiplin
> disampaikan. Ketiganya menyetujui naskah yang sama, apa adanya. Maka *"boleh berbeda"* adalah
> **kewenangan ke depan**, bukan keadaan hari ini — dan menurunkan perbedaan aturan dari kalimat
> ini berarti mengarang.

| Keputusan | Isi ringkas | Setuju | Ubah | Tolak |
|---|---|:---:|:---:|:---:|
| `LAB-DEC-003` | Prinsip empat mata pada validasi hasil, dengan jalur pengecualian bertanda permanen | ☑ | ☐ | ☐ |
| `LAB-DEC-004` | Nilai kritis tetap dirilis; pelaporan wajib dicatat lengkap | ☑ | ☐ | ☐ |
| `LAB-DEC-007` | Koreksi hasil hanya oleh pemegang kewenangan validasi/rilis; dokter pemesan diberi tahu otomatis; versi lama tetap terlihat | ☑ | ☐ | ☐ |

**Disiplin yang dicakup salinan ini**, diisi hanya bila memilih opsi kedua di atas:

```
☑ Patologi Klinik     ☑ Mikrobiologi Klinik     ☑ Patologi Anatomi
```

*(ketiganya tercentang karena ketiga salinan ditandatangani pada hari yang sama — `DR-LAB-001`
untuk Patologi Klinik, `DR-LAB-002` untuk Mikrobiologi Klinik, `DR-LAB-003` untuk Patologi
Anatomi.)*

**Catatan perubahan bila ada:**

```
_______________________________________________________________________________

_______________________________________________________________________________

_______________________________________________________________________________
```

### 9.3 Pengesahan

| | `DR-LAB-001` Patologi Klinik | `DR-LAB-002` Mikrobiologi Klinik | `DR-LAB-003` Patologi Anatomi | Mengetahui |
|---|---|---|---|---|
| Nama | dr. Aditya Pranata, Sp.PK | dr. Nabila Rahmawati, Sp.MK | dr. Citra Maharani, Sp.PA | |
| Jabatan | *(tidak disampaikan)* | *(tidak disampaikan)* | *(tidak disampaikan)* | |
| SIP / NIP | *(tidak disampaikan)* | *(tidak disampaikan)* | *(tidak disampaikan)* | |
| Tanggal | 2026-09-17 | 2026-09-17 | 2026-09-17 | |
| Tanda tangan | *(basah — belum dilampirkan)* | *(basah — belum dilampirkan)* | *(basah — belum dilampirkan)* | |

Bila memilih opsi **sendiri-sendiri** pada 9.2, cukup kolom disiplin yang bersangkutan yang
diisi pada salinan itu.

> **Yang tercatat di sini adalah persetujuannya, bukan berkas tanda tangannya.** Ketiga
> persetujuan disampaikan kepada pengaju pada 2026-09-17 dan dibukukan `LAB-DEC-079`. Lembar
> bertanda tangan basah beserta SIP/NIP **belum dilampirkan**, dan itu dicatat apa adanya —
> cukup untuk melanjutkan perancangan, belum lengkap sebagai berkas tata kelola. Kekurangan
> yang sama berlaku pada `LAB-REQ-009` bagian 7.1.

Bila pengesahan dilakukan Komite Medis sebagai lembaga, mohon dilampirkan nomor dan tanggal
risalah rapatnya:

| Field | Isian |
|---|---|
| Nomor risalah | |
| Tanggal rapat | |

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 2 | 2026-09-17 | **Diperbarui sesudah dua temuan yang mengubah arti permintaan ini, bukan isinya.** Pertama, `LAB-SIGN-001` ditelusuri sampai akarnya dan terbukti menahan **tepat tiga keputusan** — ketiga yang menjadi pokok dokumen ini. `LAB-DEC-005`, yang menetapkan hasil diketik manual oleh analis, berdiri **tanpa** syarat tanda tangan, setara `LAB-DEC-006` yang sudah dibangun penuh; pemilik modul memecah slicenya lewat `LAB-DEC-076`. Kedua, **pengisian hasil sudah berjalan sejak 2026-09-17**. Akibatnya baris pertama bagian 0 menjadi keliru dan dikoreksi: yang tertahan bukan lagi *pengisian dan validasi hasil* melainkan **validasi dan rilis** saja — dan bersamaan dengan itu **hasil mulai tersimpan di dalam sistem tanpa satu pun jalan keluar yang sah**. Permintaan ini karena itu menjadi **lebih mendesak, bukan kurang**: setiap hari menambah hasil yang terisi tetapi tidak ada seorang pun yang berwenang menyatakannya benar. **Satu fakta baru ditambahkan pada kotak hambatan-sebelum-hambatan:** jabatan yang dimintakan penetapannya juga nol ada pada data induk sistem — `MstPosition` 31 jabatan tanpa Komite Medis maupun penanggung jawab laboratorium, `MstDepartment` nol memuat unit Laboratorium — sehingga bahkan bila seseorang ditunjuk hari ini, sistem belum punya tempat mencatat jabatannya, dan itu pekerjaan `master-data`. Tabel bagian 7.1 diperbarui: `S4a` keluar dari daftar yang tertahan, dan ia keluar bukan karena dilewati melainkan karena terbukti tidak pernah diatur ketiga keputusan ini. Nol isi permintaan yang berubah; ketiga keputusan yang dimintakan tanda tangannya tetap sama persis | `terbuka` |
| 1 | 2026-09-09 | Dokumen dibuat sebagai `LAB-REQ-004`, menindaklanjuti `LAB-REQ-001` butir 6 yang dinyatakan di luar wewenang pemilik repository. Memuat tiga keputusan untuk ditandatangani, dua penetapan rumah sakit, dan tiga pertanyaan klinis terbuka. Ditemukan dan dicatat bahwa `LAB-COORD-001` dan `LAB-COORD-002` sudah ditutup 2026-09-01, sehingga `LAB-SIGN-001` menjadi satu-satunya penahan `S4`, `S4b`, `S4c`, `S5`, dan `S6` | `terbuka` |
