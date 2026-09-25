# Nota Permohonan Penetapan — Pemegang Kewenangan Validasi Hasil Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-013` |
| `tanggal` | 2026-09-18 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `ditujukan kepada` | **dr. Bima Prasetya, Sp.PK** — Kepala Instalasi Laboratorium |
| `koreksi alamat` | **2026-09-23.** Nota ini semula ditujukan kepada **dr. Arya Wicaksana, Sp.Rad**, dan itu **keliru** — beliau bukan Kepala Instalasi Laboratorium. Nama yang benar dinyatakan pemilik modul pada 2026-09-23. **Akibatnya nyata dan menjelaskan kenapa nota ini tak berjawab selama lima hari: ia berada di tangan yang bukan pemiliknya.** Tanggal pengajuan **tidak** diubah, supaya umur penahannya tetap terbaca apa adanya |
| `menutup` | `DEC-LAB-011` |
| `rujukan` | `LAB-DEC-022`; `LAB-REQ-004` bagian 5.1, diajukan 2026-09-09 dan belum dijawab; `02-requirement-completeness-assessment.md` bagian 0B.4 |
| `status` | `menunggu jawaban` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| `yang diminta` | **Dua jawaban.** Satu tentang jaminan tenaga, satu tentang siapa yang berhak menunjuk |

Dokumen ini dapat diteruskan apa adanya. Ia ditulis agar dapat dibaca tanpa membuka satu pun
dokumen desain lainnya.

---

## 1. Yang dimohonkan

**Dua jawaban, dan keduanya pendek.**

| # | Pertanyaan | Bentuk jawaban |
|---:|---|---|
| 1 | Dapatkah rumah sakit menjamin setiap shift memiliki **sekurang-kurangnya dua** pemegang kewenangan validasi hasil? | Ya / Tidak |
| 2 | **Siapa** yang berwenang menetapkan seseorang sebagai pemegang kewenangan validasi? | Kepala instalasi / Komite Medis / pemegang wewenang klinis per disiplin / lainnya |

Nota ini **tidak** meminta nama orang-orangnya. Ia meminta **siapa yang berhak menyebut nama
itu**, dan apakah jumlahnya dapat dijamin.

---

## 2. Kenapa dua pertanyaan pendek ini menahan tiga bagian sekaligus

Pada 2026-09-17 rumah sakit menyelesaikan penahan terbesar modul ini: tiga aturan keselamatan
laboratorium ditandatangani pihak klinis, per disiplin, setelah menggantung enam belas hari.
Salah satunya — **prinsip empat mata** — berbunyi: *petugas yang mengisi hasil tidak boleh
memvalidasi dan merilis hasil yang sama.*

Aturan itu kini sah. Tetapi ia **belum dapat dijalankan**, dan sebabnya satu hal:

> Sistem harus tahu **siapa** pemegang kewenangan validasi. `LAB-DEC-022` menetapkan kewenangan
> itu diberikan **per orang, bukan per jabatan** — seorang analis senior boleh memegangnya,
> seorang kepala ruangan boleh tidak. Artinya harus ada **pihak yang memberikannya**.
>
> Pihak itulah yang belum ditetapkan.

Selama itu kosong, sistem tidak punya cara yang sah untuk memberikan hak validasi kepada
siapa pun — dan hasil yang sudah diketik analis **tidak dapat dinyatakan sah oleh siapa pun**.

---

## 3. Apa yang sedang tertahan

Tiga bagian, dan ketiganya hanya menunggu nota ini:

| Bagian | Isinya | Disiplin |
|---|---|---|
| `S4` | Validasi dan rilis hasil | Patologi Klinik |
| `S4d` | Validasi dan rilis hasil | Mikrobiologi Klinik |
| `S4e` | Validasi dan rilis hasil | Patologi Anatomi |

**Ketiganya tidak menunggu hal lain dari sisi perangkat lunak.** Aturannya sudah ditandatangani,
bentuk datanya sudah diputuskan, dan pengisian hasil sudah berjalan sejak 2026-09-17.

### Biayanya hari ini, dinyatakan apa adanya

Analis sudah dapat mengetik hasil pemeriksaan ke dalam sistem. **Nol di antaranya dapat
dirilis.** Setiap hari yang berjalan menambah tumpukan hasil yang tersimpan tanpa satu pun jalan
keluar yang sah — dan hasil yang tidak dapat dirilis tidak dapat dikirim ke dokter pemesan
maupun pasien.

---

## 4. Kenapa pertanyaan pertama bukan sekadar soal administrasi

Pertanyaan tentang **dua pemegang per shift** terlihat seperti urusan jadwal. Ia bukan.

Prinsip empat mata menyediakan **jalur pengecualian**: bila benar-benar tidak ada orang kedua,
validasi oleh pengisi sendiri tetap diizinkan, dengan alasan tercatat dan penanda permanen
*"divalidasi oleh pengisi sendiri"*.

Yang perlu disadari sejak awal:

> Bila sebuah shift hanya punya **satu** pemegang kewenangan validasi, maka **setiap** hasil yang
> ia kerjakan akan lewat jalur pengecualian itu. Dalam sebulan, "pengecualian" berubah menjadi
> kebiasaan — dan prinsip empat mata berhenti berarti apa pun.
>
> Yang membuatnya sulit terdeteksi: **tidak ada aturan yang dilanggar**. Pengujian sistem tetap
> lulus. Penandanya tetap tercatat. Semuanya tampak benar, dan justru itu bahayanya.

Karena itu jawaban **"Tidak"** pada pertanyaan pertama pun merupakan jawaban yang sah dan
berguna. Ia hanya perlu diketahui sekarang, bukan setelah sistem berjalan.

---

## 5. Satu hal yang perlu diperjelas lebih dulu

Ditulis terbuka karena menyangkut **batas wewenang**, bukan menyangkut orang.

Modul ini melayani **tiga disiplin laboratorium**: Patologi Klinik, Mikrobiologi Klinik, dan
Patologi Anatomi (`LAB-DEC-025`). Pada 2026-09-17 rumah sakit menetapkan **tiga** pemegang
wewenang klinis, satu untuk tiap disiplin:

| Kode | Nama | Disiplin |
|---|---|---|
| `DR-LAB-001` | dr. Aditya Pranata, Sp.PK | Patologi Klinik |
| `DR-LAB-002` | dr. Nabila Rahmawati, Sp.MK | Mikrobiologi Klinik |
| `DR-LAB-003` | dr. Citra Maharani, Sp.PA | Patologi Anatomi |

Menetapkan siapa yang boleh **menyatakan sebuah angka hasil benar** adalah penilaian kompetensi
atas pekerjaan laboratorium. Karena itu dua hal perlu diperjelas, dan keduanya dapat dijawab
singkat pada halaman 7:

| Hal | Kenapa ditanyakan |
|---|---|
| **Instalasi mana** yang dipimpin — Instalasi Laboratorium, atau Instalasi Penunjang Medis yang menaungi Laboratorium bersama unit lain | Data induk sistem menempatkan Laboratorium di bawah departemen `Penunjang Medis`. Keduanya lazim, dan cakupannya berbeda |
| Apakah wewenang **menetapkan** ada pada kepala instalasi **sendiri**, pada **ketiga pemegang wewenang klinis** sesuai disiplinnya, atau pada **keduanya berlapis** | Ketiganya sama-sama sah secara tata kelola. Yang dibutuhkan sistem hanya kepastian **satu** di antaranya, supaya penetapannya punya dasar yang dapat ditelusuri |

Bila jawabannya "berlapis" — misalnya kepala instalasi mengusulkan dan pemegang wewenang klinis
disiplin terkait menyetujui — itu pun jawaban yang lengkap, dan sistem dapat menjalankannya.

> ### Pembaruan 2026-09-23 — alamat nota dikoreksi, dan satu dari dua hal di atas ikut terjawab
>
> Kepala Instalasi Laboratorium adalah **dr. Bima Prasetya, Sp.PK**, dinyatakan pemilik modul pada
> 2026-09-23. Nota ini semula ditujukan kepada dr. Arya Wicaksana, Sp.Rad — **keliru orang**.
>
> **Hal pertama pada tabel di atas dengan sendirinya terjawab:** yang dipimpin adalah **Instalasi
> Laboratorium**, bukan Instalasi Penunjang Medis yang menaungi beberapa unit. Baris itu nol perlu
> diisi lagi.
>
> **Hal kedua tetap terbuka, dan koreksi ini justru menajamkannya.** `Sp.PK` adalah spesialisasi
> **Patologi Klinik** — satu dari tiga disiplin yang dilayani modul ini, dan spesialisasi yang sama
> dengan `DR-LAB-001`. Menetapkan pemegang kewenangan validasi bagi **Mikrobiologi Klinik** dan
> **Patologi Anatomi** karena itu berarti menetapkan di luar disiplin beliau sendiri. Itu **bukan**
> alasan untuk menolak kewenangannya — banyak rumah sakit memang menempatkan penetapan pada
> jabatan, bukan pada disiplin — melainkan alasan kenapa pertanyaan kedua **tetap perlu dijawab
> eksplisit** alih-alih dianggap sudah jelas.
>
> Bentuk jawaban yang dibutuhkan nol berubah: cukup satu pilihan pada bagian 7.3.

---

## 6. Yang **tidak** dimohonkan

Agar tidak salah baca:

| Hal | Keadaan |
|---|---|
| Nama orang-orang yang akan memegang kewenangan validasi | **Tidak di sini.** Itu dilakukan setelah pertanyaan 2 dijawab, oleh pihak yang ditunjuk jawabannya |
| Persetujuan atas aturan empat mata itu sendiri | Sudah ditandatangani pihak klinis pada 2026-09-17. Tidak dibuka kembali |
| Persetujuan perubahan **batas kritis** | Perkara terpisah (`DEC-LAB-012`), wewenang klinis, dan **sengaja tidak dibundel** ke nota ini |
| Tambahan tenaga atau anggaran | Tidak diminta. Pertanyaan 1 menanyakan **keadaan yang ada**, bukan meminta penambahan |
| Perubahan alur kerja laboratorium hari ini | Tidak ada |

> **Nota ini sengaja dibuat pendek dan tunggal isinya.** Permintaan sebelumnya — `LAB-REQ-004` —
> memuat tiga tanda tangan, dua penetapan, dan tiga pertanyaan klinis sekaligus. Ketiga tanda
> tangannya turun; **kelima butir lainnya tidak**, termasuk pertanyaan yang kini diajukan ulang
> di sini. Pelajarannya diambil: satu nota, satu perkara.

---

## 7. Halaman penetapan

### 7.1 Kejelasan cakupan

| Field | Isian |
|---|---|
| Instalasi yang dipimpin | ☐ Instalasi Laboratorium ☐ Instalasi Penunjang Medis ☐ lainnya: …………… |

### 7.2 Pertanyaan 1 — jaminan tenaga

| Field | Isian |
|---|---|
| Dapatkah dijamin **minimal dua** pemegang kewenangan validasi per shift? | ☐ Ya ☐ Tidak |
| Bila **Tidak**: shift mana yang diperkirakan hanya punya satu? | …………………………………………… |
| Bila **Tidak**: apakah jalur pengecualian empat mata dapat diterima untuk shift tersebut? | ☐ Ya ☐ Tidak |

### 7.3 Pertanyaan 2 — siapa yang berwenang menetapkan

| Field | Isian |
|---|---|
| Wewenang menetapkan pemegang kewenangan validasi ada pada | ☐ Kepala instalasi<br>☐ Komite Medis<br>☐ Pemegang wewenang klinis per disiplin (`DR-LAB-001`/`002`/`003`)<br>☐ Berlapis, yaitu: ……………………<br>☐ Lainnya: …………………… |
| Apakah wewenang itu boleh didelegasikan? | ☐ Ya, kepada …………… ☐ Tidak |
| Apakah penetapannya berlaku per disiplin, atau satu penetapan untuk seluruh laboratorium? | ☐ Per disiplin ☐ Seluruh laboratorium |

### 7.4 Pengesahan

| Field | Isian |
|---|---|
| Nama | dr. Bima Prasetya, Sp.PK |
| Jabatan | |
| Tanggal | |
| Tanda tangan | |

| | Nama | Jabatan | Tanda tangan | Tanggal |
|---|---|---|---|---|
| Mengetahui | | | | |

---

## 8. Setelah diisi

Cukup kembalikan nota ini kepada pengaju. Yang terjadi berikutnya, berurutan:

| # | Langkah | Pelaksana |
|---:|---|---|
| 1 | Jawaban dibukukan sebagai keputusan, menutup `DEC-LAB-011` | Pemilik modul |
| 2 | Arsitektur domain untuk validasi dan rilis disusun — `S4`, `S4d`, `S4e` | Tim pembangun sistem |
| 3 | Kontrak diamandemen, pekerjaan dijadwalkan | Tim pembangun sistem |
| 4 | Pihak yang ditunjuk jawaban pertanyaan 2 menetapkan orang-orangnya | Sesuai jawaban |

**Langkah 2 pantas diketahui sejak awal supaya harapannya tepat.** Jawaban nota ini membuat
bagian validasi dan rilis **boleh dirancang** — ia tidak membuat fiturnya langsung ada.

Bila rumah sakit menilai penetapan ini sebaiknya ditempuh dengan cara lain — surat keputusan
tersendiri, atau rapat Komite Medis — jawaban itu juga cukup. Yang dibutuhkan modul ini adalah
**kepastian siapa yang berhak**, bukan bentuk dokumennya.
