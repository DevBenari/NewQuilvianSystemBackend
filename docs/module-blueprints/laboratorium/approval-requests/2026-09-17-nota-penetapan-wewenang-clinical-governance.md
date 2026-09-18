# Nota Permohonan Penetapan — Pemegang Wewenang Klinis Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-009` |
| `tanggal` | 2026-09-17 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `LAB-SIGN-001`; `LAB-REQ-004` bagian 0 dan 9.1; `blueprint-manifest.md` revision `41` — `clinical_governance: belum ditetapkan` |
| `status` | **`dijawab` — 2026-09-17.** Tiga nama diterima; lihat bagian 7. Ruas penetapan formalnya belum lengkap, dan sisanya dicatat apa adanya pada bagian 7.1 |
| `ditujukan kepada` | Manajemen rumah sakit — Direktur, Direktur Pelayanan Medik, atau Komite Medis, sesuai tata kelola yang berlaku |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| `menutup` | `LAB-GOV-SEAT` **sebagian** — namanya ada, barisnya pada data induk belum. Nota ini sendiri **tidak** menutup `LAB-SIGN-001`; yang menutupnya tanda tangan yang menyusul pada hari yang sama lewat `LAB-REQ-004` (`LAB-DEC-079`) |

---

## 1. Yang dimohonkan

**Satu nama.**

Rumah sakit belum menunjuk siapa yang berwenang menyatakan aturan klinis modul Laboratorium
benar. Selama nama itu belum ada, bagian sistem yang menyentuh keselamatan pasien **tidak boleh
dan tidak akan dibangun**.

Nota ini **tidak** meminta persetujuan atas aturan apa pun. Ia meminta penetapan **siapa yang
berhak memberi persetujuan itu**.

---

## 2. Kenapa nota ini terpisah dari permintaannya sendiri

`LAB-REQ-004` — permintaan tanda tangan klinis — sudah disusun lengkap dan diajukan pada
**2026-09-09**. Sampai hari ini belum dijawab, dan sebabnya bukan kelalaian siapa pun:

> **Dokumen itu tidak punya tujuan yang sah.** Ia ditujukan kepada "pemegang wewenang Clinical
> Governance modul Laboratorium", dan jabatan itu **belum pernah diisi**.

`LAB-REQ-004` bagian 9.1 menyediakan kolom penetapannya, tetapi kolom itu hanya dapat diisi oleh
pihak yang berwenang menunjuk — yaitu manajemen rumah sakit, bukan tim pembangun sistem dan bukan
pemilik modul. Karena itu permohonannya dipisahkan menjadi nota ini.

Delapan hari berlalu tanpa gerak, dan itu bukan karena permintaannya sulit. Ia hanya tidak punya
alamat.

---

## 3. Apa yang sedang tertahan

> ### Diperbarui 2026-09-17 — cakupannya ditelusuri ulang, dan hasilnya menyempit
>
> Penahan ini semula terbaca menahan **seluruh** pengisian hasil. Penelusuran sampai akarnya
> pada 2026-09-17 menunjukkan ia menahan **tepat tiga keputusan**:
>
> | Keputusan | Isi |
> |---|---|
> | `LAB-DEC-003` | Prinsip empat mata pada **validasi dan rilis** |
> | `LAB-DEC-004` | **Nilai kritis** dan kewajiban pencatatan pelaporannya |
> | `LAB-DEC-007` | **Koreksi hasil** setelah rilis |
>
> Ketiganya menentukan *"perilaku sistem saat hasil salah atau pasien dalam bahaya"* —
> dan itu memang pantas menunggu wewenang klinis.
>
> **`LAB-DEC-005`, yang menetapkan hasil diketik manual oleh analis, berdiri tanpa syarat ini.**
> Ia setara dengan `LAB-DEC-006` yang sudah dibangun penuh. Karena itu pemilik modul memecah
> slicenya pada 2026-09-17 (`LAB-DEC-076`): **pengisian hasil sudah berjalan sejak hari itu**,
> sementara validasi, rilis, nilai kritis, dan koreksi **tetap menunggu nota ini**.
>
> **Dua hal yang perlu diketahui dari pemecahan itu:**
>
> 1. **Biayanya kini dapat disebut lebih tepat.** Yang tertahan bukan lagi "hasil laboratorium",
>    melainkan **kemampuan merilis hasil kepada dokter dan pasien** — hasil dapat diisi, tetapi
>    tidak ada satu pun yang dapat menyatakannya sah.
> 2. **Ia menjadi lebih mendesak, bukan kurang.** Sejak pengisian berjalan, hasil pemeriksaan
>    akan mulai tersimpan **tanpa satu pun jalan keluar yang sah**. Semakin lama nota ini
>    menggantung, semakin banyak hasil yang menumpuk tanpa dapat dirilis.
>
> **Satu hambatan yang sempat dicatat di sini sudah ditutup, dan sisanya menjadi lebih terang.**
> Pembacaan data induk 2026-09-17 mula-mula menemukan jabatan yang dimohonkan nota ini nol ada
> pada sistem. Pemeriksaan lanjutan menunjukkan gambarannya lebih baik daripada itu:
>
> | Hal | Keadaan |
> |---|---|
> | Departemen tempat laboratorium bernaung | ✅ `Penunjang Medis` sudah ada sejak semula |
> | Jabatan analis yang mengetik hasil | ✅ `Analis Laboratorium`, **2 orang** memegangnya |
> | Jabatan pemegang wewenang klinis | ✅ **`Dokter Penanggung Jawab Laboratorium`** didirikan 2026-09-17 atas instruksi pemilik modul (`LAB-DEC-077`) |
> | **Orang yang memegangnya** | ❌ **Nol** |
>
> **Artinya yang dimohonkan nota ini kini tepat satu hal: sebuah nama.** Kursinya sudah ada,
> tinggal siapa yang mendudukinya — dan itu memang penetapan manajemen, bukan pekerjaan yang
> dapat diselesaikan dari sisi perangkat lunak.
>
> **Satu hal perlu diketahui sebelum menetapkan.** Menambahkan jabatan itu **tidak** membuka
> `LAB-SIGN-001`: penelusuran source menemukan **nol kode** yang membaca konsep Clinical
> Governance. Tanda tangan itu tindakan manusia atas sebuah dokumen. Jabatan tadi berguna untuk
> **mencatat** pemegangnya dan kelak untuk hak akses — bukan untuk menggantikan tanda tangannya.

Modul Laboratorium sudah berjalan untuk bagian yang tidak menyentuh keselamatan pasien:
pendaftaran, pemesanan, wadah dan sampel, daftar kerja, pemantauan, data induknya, dan — sejak
2026-09-17 — **pengisian hasil**. Yang **belum boleh dibangun** adalah tiga hal berikut.

| Yang tertahan | Artinya bagi pekerjaan sehari-hari |
|---|---|
| Pengisian dan validasi hasil pemeriksaan | Hasil masih dicatat **di luar sistem**, seperti sekarang |
| Penandaan dan pelaporan nilai kritis | Pelaporan nilai kritis masih **lisan**, tanpa catatan yang dapat ditelusuri |
| Koreksi hasil yang sudah dirilis | **Belum ada** mekanisme resmi memperbaiki hasil yang terlanjur dipakai dokter |

Ketiganya tertahan oleh satu hal yang sama, dan penahannya **bukan** teknis maupun anggaran.
Pemilik modul sendiri menyatakan — lewat keputusan `LAB-DEC-011` — bahwa tiga aturan berikut
**bukan wewenangnya**, karena ketiganya menentukan hal yang bersifat klinis:

| Keputusan | Yang ditentukannya |
|---|---|
| `LAB-DEC-003` | **Siapa yang boleh menyatakan sebuah angka hasil benar** |
| `LAB-DEC-004` | **Apa yang terjadi ketika hasil menunjukkan pasien dalam bahaya** |
| `LAB-DEC-007` | **Apa yang terjadi ketika hasil yang sudah dipakai dokter ternyata salah** |

Ketiganya sudah dirumuskan lengkap dan sudah disetujui dari sisi produk dan operasional. Yang
dibutuhkan adalah pernyataan pihak klinis bahwa rumusannya memang benar — atau koreksinya bila
keliru.

### 3.1 Dua perkara baru ikut menunggu wewenang yang sama

Sejak `LAB-REQ-004` diajukan, requirement lapangan menambahkan dua hal yang jatuh pada wewenang
yang persis sama:

| Perkara | Isinya |
|---|---|
| `LAB-OPEN-029` | Requirement menyebut hasil hanya boleh dikirim ke pasien setelah disetujui **Profesor dan Dokter Lab**. Apakah itu sama dengan tanda tangan klinis `LAB-DEC-011`, dan siapa pemegangnya, belum terjawab |
| `LAB-OPEN-030` | Requirement memakai jabatan **`Dokter Lantai`** pada pelaporan nilai kritis. Jabatan itu **nol kemunculan** di seluruh blueprint — konsep peran yang belum dikenal sistem |

Keduanya diajukan lewat `LAB-REQ-007` pada 2026-09-16, dan keduanya menunggu nama yang sama.

---

## 4. Jabatan yang lazim memegangnya

Ditulis sebagai bahan pertimbangan, **bukan** usulan yang mengikat. Penetapannya sepenuhnya
wewenang manajemen rumah sakit.

| Calon | Pertimbangan |
|---|---|
| **Dokter penanggung jawab laboratorium** | Paling dekat dengan pekerjaan sehari-harinya; keputusan dapat diambil cepat oleh satu orang. Risikonya: bila berhalangan, tidak ada penggantinya |
| **Komite Medis sebagai lembaga** | Keputusannya lebih kuat secara tata kelola dan tidak bergantung pada satu orang. Risikonya: perlu waktu rapat |
| **Keduanya berlapis** | Dokter penanggung jawab menandatangani, Komite Medis mengetahui. Lebih lambat sedikit, tetapi tidak menggantung bila salah satu berhalangan |

Yang dibutuhkan sistem hanya **satu hal**: ada nama yang tercatat, dan nama itu berwenang.
Bentuknya perorangan atau lembaga sama-sama dapat dijalankan.

---

## 5. Yang terjadi setelah nama ditetapkan

Berurutan, supaya jelas nota ini membuka apa.

| # | Langkah | Pelaksana | Keadaan 2026-09-17 |
|---:|---|---|---|
| 0 | Nama ditetapkan | Manajemen rumah sakit | ✅ **selesai** — tiga nama, lihat bagian 7 |
| 1 | `LAB-REQ-004` dikirim kepada nama yang ditetapkan | Pemilik modul | ✅ **selesai** — dialamatkan ulang dan dijawab pada hari yang sama |
| 2 | Tiga keputusan ditandatangani, diubah, atau ditolak | Pemegang wewenang klinis | ✅ **selesai** — **ditandatangani apa adanya, per disiplin** (`LAB-DEC-079`). `LAB-SIGN-001` ditutup |
| 3 | `LAB-REQ-007` bagian 3 dan 5 ikut terjawab | Pemegang wewenang klinis | ⛔ **belum** — `LAB-OPEN-029` (Profesor) dan `LAB-OPEN-030` (Dokter Lantai) tetap terbuka |
| 4 | Arsitektur domain untuk validasi, rilis, nilai kritis, dan koreksi disusun | Tim pembangun sistem | 🟢 **boleh dimulai** — belum dikerjakan |
| 5 | Kontrak diamandemen, pekerjaan dijadwalkan | Tim pembangun sistem | ⛔ menunggu langkah 4 |

> **Nota ini meminta satu nama. Ia menerima tiga, dan pada hari yang sama menerima tanda
> tangannya juga.** Langkah 0 sampai 2 selesai dalam satu hari, sesudah menggantung 16 hari —
> dan sebabnya sederhana: yang menahan selama ini bukan keberatan atas isinya, melainkan
> ketiadaan alamat.
>
> **Dua hal tetap perlu dibaca tepat.** Langkah 3 **tidak** ikut terjawab: bagian 5 dan 6
> `LAB-REQ-004` — dua penetapan dan tiga pertanyaan klinis — masih terbuka, dan `LAB-REQ-004`
> bagian 7.2 sudah memperingatkan bahwa keempatnya akan kembali menahan pekerjaan. Dan langkah 4
> **baru boleh dimulai, belum dimulai**: nol task dibuka, nol kontrak naik revisi, nol baris kode
> disentuh pada hari ini.

**Langkah 4 pantas diketahui sejak awal supaya harapannya tepat.** Bagian pengisian hasil belum
pernah dirancang sama sekali — ia sengaja dikecualikan dari dokumen arsitektur karena aturannya
belum sah. Tanda tangan **tidak** membuat fiturnya langsung ada; ia membuat perancangannya boleh
dimulai.

---

## 6. Yang **tidak** dimohonkan

Agar tidak salah baca:

| Hal | Keadaan |
|---|---|
| Persetujuan atas ketiga aturan klinis | **Tidak di sini.** Itu isi `LAB-REQ-004`, dan hanya dapat dijawab setelah nota ini |
| Anggaran, perangkat, atau tambahan tenaga | Tidak diminta |
| Perubahan alur kerja laboratorium hari ini | Tidak ada. Selama tertahan, hasil tetap dicatat di luar sistem seperti sekarang |
| Penilaian atas kinerja siapa pun | Tidak. Penahan ini murni ketiadaan penetapan, bukan kelalaian |

---

## 7. Halaman penetapan

> **Terisi 2026-09-17.** Jawabannya disampaikan pemilik modul sebagai penerus, bukan ditulis
> langsung di atas nota ini oleh manajemen. Isinya ditulis apa adanya; yang **tidak** disampaikan
> dibiarkan kosong dan didaftar pada bagian 7.1, bukan ditebak.

| Field | Isian |
|---|---|
| Nama pemegang wewenang Clinical Governance modul Laboratorium | **Tiga orang, satu per disiplin** — lihat tabel di bawah |
| Jabatan | Dokter spesialis penanggung jawab disiplin masing-masing. Padanannya pada data induk: `Dokter Penanggung Jawab Laboratorium` (`POS-PMJ-004`), didirikan `LAB-DEC-077` |
| Bentuk penetapan — perorangan / lembaga / berlapis | **Perorangan per disiplin** — bentuk keempat, di luar ketiga yang ditawarkan bagian 4. Lihat 7.1 butir 1 |
| Ditetapkan oleh | *(tidak disampaikan)* |
| Tanggal penetapan | *(tidak disampaikan)* — nota ini diterima 2026-09-17 |
| Berlaku sampai — bila dibatasi | *(tidak disampaikan)* |

**Nama yang ditetapkan:**

| Kode | Nama | Disiplin yang dipegangnya | Slice yang bersangkutan |
|---|---|---|---|
| `DR-LAB-001` | **dr. Aditya Pranata, Sp.PK** | Patologi Klinik | `S4` validasi dan rilis, `S5`, `S6` |
| `DR-LAB-002` | **dr. Nabila Rahmawati, Sp.MK** | Mikrobiologi Klinik | `S4b` |
| `DR-LAB-003` | **dr. Citra Maharani, Sp.PA** | Patologi Anatomi | `S4c` |

Ketiganya menutup **persis** ketiga disiplin yang `LAB-DEC-025` tetapkan sebagai scope modul.
Bank Darah memang di luar scope, dan ketiadaan nama untuknya **bukan** kekurangan.

**Pengganti bila berhalangan**, diisi bila penetapannya perorangan:

| Field | Isian |
|---|---|
| Nama pengganti | *(tidak disampaikan)* |
| Jabatan | *(tidak disampaikan)* |

### Pengesahan

| | Nama | Jabatan | Tanda tangan | Tanggal |
|---|---|---|---|---|
| Ditetapkan oleh | *(tidak disampaikan)* | | | |
| Diketahui | *(tidak disampaikan)* | | | |

---

## 7.1 Yang masih kurang dari jawaban ini — ditulis supaya tidak ditemukan belakangan

Nota ini meminta **satu nama**. Ia menerima **tiga**, dan itu lebih baik daripada yang diminta.
Tetapi empat hal ikut terbawa, dan tidak satu pun dapat diselesaikan dari sisi perangkat lunak.

| # | Yang kurang | Kenapa ia penting |
|---:|---|---|
| 1 | ~~**Bentuknya perorangan per disiplin — bentuk yang tidak ditawarkan bagian 4**~~ ✅ **Terjawab pada hari yang sama: `sendiri-sendiri`** | Ketiga keputusan yang tertahan dirumuskan **lintas disiplin**, sehingga siapa menandatangani apa perlu dinyatakan. Dijawab: **masing-masing menandatangani untuk disiplinnya sendiri**, dan aturannya **boleh** berbeda antardisiplin. **Nol perbedaan disampaikan hari ini**, jadi ketiga naskah berlaku identik; yang per disiplin adalah wewenangnya. **Satu pertanyaan menggantikannya**, `LAB-OPEN-034`: apakah kewenangan validasi/rilis melintasi disiplin — dan itu menyentuh `LAB-DEC-022`, jaminan dua pemegang kewenangan per shift |
| 2 | **Siapa yang menetapkan, dan kapan** | Nota ini ditujukan kepada manajemen rumah sakit justru karena penunjukan adalah wewenang mereka. Tanpa ruas ini, yang tercatat adalah **nama**, bukan **penetapan**. Ia cukup untuk melanjutkan pekerjaan; ia belum cukup sebagai jejak tata kelola |
| 3 | **Pengganti bila berhalangan** | Bagian 4 menandai ini sebagai risiko utama bentuk perorangan. Dengan tiga orang, risikonya tidak hilang melainkan **terbagi tiga**: bila dr. Nabila berhalangan, yang berhenti adalah Mikrobiologi saja — tetapi ia berhenti sepenuhnya |
| 4 | **Ketiganya belum ada pada data induk** | Penelusuran seluruh workspace pada 2026-09-17 menemukan **nol kemunculan** ketiga kode `DR-LAB-00x`, dan **nol kemunculan** ketiganya sebagai dokter. Satu tabrakan nama dicatat supaya pencarian berikutnya tidak tersesat: *"Citra Maharani"* muncul pada `QuilvianSystemFrontendDev/src/utils/dataPasien.jsx:242` sebagai nama **pasien dummy**, nol kaitan dengan `DR-LAB-003`. Basis data **tidak** diperiksa — nol klien SQL tersedia pada lingkungan sesi ini, dan `MstDoctor` menuntut tujuh ruas wajib yang tidak satu pun disampaikan di sini. Karena itu `LAB-GOV-SEAT` **menyempit, bukan tertutup**: kursinya ada, namanya ada, barisnya belum |

**Satu hal yang perlu diketahui terpisah, dan ia yang paling mudah salah baca.** Nota ini tidak
menyebut **`Profesor`**. `RULE-017` pada artifact Menu Hasil mensyaratkan hasil disetujui
*"Profesor dan Dokter Lab"* — **dua** penyetuju. Ketiga nama di atas adalah dokter spesialis;
nol di antaranya bergelar Profesor. Maka `LAB-OPEN-029` **menyempit tetapi tidak tertutup**:
siapa pemegang wewenang Clinical Governance kini terjawab, sedangkan apakah `RULE-017` sama
dengan tanda tangan klinis `LAB-DEC-011` — dan siapa `Profesor` itu — tetap terbuka.

---

## 8. Setelah diisi

Cukup kembalikan nota ini kepada pengaju. Nama yang tercatat akan dimasukkan ke
`blueprint-manifest.md` pada ruas `owners.clinical_governance`, dan `LAB-REQ-004` akan dikirimkan
kepada nama itu pada hari yang sama.

Bila manajemen menilai penetapan ini sebaiknya ditempuh dengan cara lain — surat keputusan
tersendiri, misalnya — jawaban itu juga cukup. Yang dibutuhkan modul ini adalah **nama yang sah**,
bukan bentuk dokumennya.
