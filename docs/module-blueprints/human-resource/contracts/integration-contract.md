# Human Resource — Kontrak Integrasi

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/integration-contract.md` |
| `contract_version` | `v5` |
| `last_changed_in` | `v7` — bagian 11 (`S-D1`) ditambahkan 3 November 2026; bagian lain tidak berubah sejak `v4` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`), bersama pemilik modul tetangga untuk setiap batas |
| `approved_by` / `approved_at` | **Hanya bagian 11** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `00-interview-decisions.md` revision `15`; `flows/00-module-context-flow.md` |
| `input_hash` — decision log | `da1d74f2e417fd31815cf69b401f390277c361e404d38579bcfa75e0f125f083` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Dampak kompatibilitas | Tidak ada kontrak lintas modul yang berubah. Sebagian besar batas justru **belum** punya kontrak |

---

## 0. Mengapa dokumen ini ada, dan apa yang jujur harus disampaikan

Dokumen ini menetapkan **di mana modul HR berhenti dan modul lain mulai**, dan bagaimana keduanya
berbicara.

Kenyataan yang harus disampaikan apa adanya: **sebagian besar batas lintas modul HR belum punya
kontrak sama sekali.** Dari tujuh titik sentuh yang teridentifikasi, hanya **satu** yang benar-
benar punya jalur kode, dan itu pun berhenti di batas yang sudah diputuskan. Sisanya adalah
batas yang **diketahui ada** tetapi bentuknya **belum disepakati siapa pun**.

Dokumen ini menulis kenyataan itu, bukan mengarang kontrak yang belum ada.

### 0.1 Aturan yang mengikat seluruh dokumen

1. **Tidak boleh ada dua modul yang menjadi sumber kebenaran untuk fakta yang sama.**
2. Setiap kali sebuah alur HR membaca atau menulis data di seberang batas, titik sentuhnya
   **wajib** tercatat di sini lebih dulu.
3. Batas yang bertanda `[OPEN]` **MUST NOT** dijadikan dasar implementasi. Menebak bentuknya
   berarti menetapkan kontrak lintas modul secara sepihak.

---

## 1. Ringkasan seluruh batas

| ID | Batas | Modul di seberang | Arah | Keadaan kontrak |
| --- | --- | --- | --- | --- |
| `INT-HR-01` | Serah terima payroll | Finance | HR mengirim | **`[DECISION]` sampai batas; `[OPEN]` sesudahnya** |
| `INT-HR-02` | Jadwal praktik dokter | Health Services | Tidak ada aliran data | **`[DECISION]` — sengaja terpisah** |
| `INT-HR-03` | Akun aplikasi dan hak akses | Administrator / Identity | HR meminta, Identity mengerjakan | **`[OPEN]`** |
| `INT-HR-04` | Penyimpanan berkas dan dokumen | Shared platform | HR menyimpan metadata | **`[OPEN]` sebagian; sudah dipakai tanpa kontrak tertulis** |
| `INT-HR-05` | Pemberitahuan kepada pegawai | Belum diketahui pemiliknya | HR mengirim | **`[OPEN]`** |
| `INT-HR-06` | Data klinis untuk penilaian praktik profesional | Health Services | Health Services mengirim | **`[BLOCKED]`** |
| `INT-HR-07` | Pengecekan kewenangan klinis saat pelayanan | Health Services | Health Services bertanya | **`[BLOCKED]`** |

### 1.1 Integrasi di dalam modul HR sendiri

Selain batas lintas modul, ada empat titik sambung **di dalam** modul HR yang penting dicatat
karena melintasi bounded context dan punya risiko konsistensi.

| ID | Sambungan | Dari | Ke | Keadaan |
| --- | --- | --- | --- | --- |
| `INT-HR-08` | Jadwal kerja menjadi acuan kehadiran | Penjadwalan | Kehadiran | `[EXISTING]` |
| `INT-HR-09` | Cuti yang berjalan menandai hari kehadiran | Cuti | Kehadiran | `[EXISTING]` |
| `INT-HR-10` | Lembur dicocokkan dengan kehadiran nyata | Kehadiran | Lembur | `[EXISTING]` |
| `INT-HR-11` | Persetujuan bersama melayani seluruh domain | Seluruh domain | Persetujuan | `[EXISTING]` |

---

## 2. `INT-HR-01` — Serah terima payroll ke Finance

### 2.1 Batas yang sudah diputuskan

`[DECISION]` `HRD-DEC-009`, `approved` 27 Agustus 2026.

| Milik HR | Milik Finance |
| --- | --- |
| Mengumpulkan masukan dari kehadiran, cuti, dan lembur | Membayar gaji kepada pegawai |
| Merekonsiliasi selisih antar masukan | Membukukan jurnal akuntansi |
| Menghitung komponen gaji sisi HR | Menghitung dan menyetorkan pajak |
| Mengunci periode dan menjalankan serah terima | Melaporkan kepada pihak luar |
| Membatalkan atau memperbaiki serah terima sisi HR | Memperbaiki di sisinya sendiri |

**Kalimat batas yang mengikat:** tanggung jawab HR atas payroll **berhenti setelah serah terima
dijalankan**.

**Bukti bahwa batas ini benar-benar dijaga hari ini:** tidak ada satu pun dari 1.343 endpoint HR
yang mengubah status pembayaran. Rantainya berhenti pada endpoint serah terima `[EXISTING]`.

### 2.2 Apa yang benar-benar terjadi hari ini

Tidak ada satu alur "putaran payroll" tunggal. Yang ada adalah **tiga jalur domain terpisah**,
masing-masing menulis snapshot ke tabel masukannya sendiri.

| Jalur | Apa yang ditulis | Apa yang ditandai pada data sumbernya |
| --- | --- | --- |
| Kehadiran | Baris masukan kehadiran per pegawai per periode | Hari kehadiran ditandai terkunci dan status masukannya menjadi selesai diproses |
| Lembur | Baris masukan lembur per realisasi | Realisasi lembur ditandai sudah diserahkan ke payroll |
| Cuti | Baris masukan variabel per permohonan cuti yang selesai | — |

Ketiganya **memeriksa status putaran payroll sebelum menulis** dan menolak bila statusnya sudah
terminal `[EXISTING]`. Inilah satu-satunya pengendalian tingkat putaran yang benar-benar
ditegakkan kode.

### 2.3 Sifat integrasi

| Aspek | Isi |
| --- | --- |
| Sinkron atau asinkron | **Sinkron.** Petugas menekan Jalankan Serah Terima dan menunggu hasilnya |
| Idempotensi | **Ya, terbukti.** Kehadiran memakai penanda hasil idempoten, lembur memakai kunci idempotensi, cuti memeriksa baris yang sudah ada `[EXISTING]` |
| Batas waktu | Tidak ditemukan pengaturan batas waktu khusus. Untuk periode besar, ini perlu dinilai saat implementasi |
| Percobaan ulang | Menjalankan ulang aman karena idempoten |
| Antrean gagal | **Tidak ada.** Kegagalan ditangani lewat `repair` dan `rollback` yang dipicu petugas, bukan antrean otomatis |
| Rekonsiliasi | Tersedia per domain: kehadiran dan cuti lewat endpoint rekonsiliasi; lembur lewat aksi rekonsiliasi yang dapat memperbaiki |

### 2.4 Yang **MUST NOT** dirancang sekarang

| Pertanyaan terbuka | Isi | Pemilik | Dampak bila ditebak |
| --- | --- | --- | --- |
| `HRD-Q-10` | Bentuk data serah terima apa yang diterima Finance, dan apakah Finance menarik sendiri atau HR mengirim | Pemilik produk bersama Finance | Kontrak lintas modul ditetapkan sepihak. Bila Finance ternyata mengharapkan bentuk lain, seluruh jalur serah terima dibongkar ulang |
| `HRD-Q-11` | Apa yang terjadi bila Finance menolak satu batch yang sudah diserahkan. Apakah HR memakai pembatalan yang sudah ada, atau Finance memperbaiki di sisinya | Pemilik produk bersama Finance | Bila HR membangun jalur pembatalan yang ternyata tidak dipakai Finance, pekerjaan itu terbuang. Bila HR tidak membangunnya padahal Finance mengharapkannya, batch yang ditolak menggantung tanpa jalan keluar |

**Yang sudah pasti dan boleh dipegang:** `Payroll Executed` **bukan** `Employee Paid`.

### 2.5 Kesenjangan di sisi HR sendiri

Sebelum bicara dengan Finance, ada tiga hal di sisi HR yang **belum ada**:

| Kesenjangan | Keadaan | Pertanyaan terkait |
| --- | --- | --- |
| Kalkulasi lintas domain menjadi angka gaji | **`MISSING`.** Tiga snapshot masukan itu adalah **masukan**, bukan hasil hitung. Tidak ditemukan service yang mengagregasinya | — |
| Persetujuan tingkat putaran payroll | **`MISSING`.** Serah terima hanya digerbangi status yang tidak terminal, bukan oleh persetujuan siapa pun | — |
| Cara putaran payroll dimulai dan dimajukan | **`[OPEN]`.** Tidak ditemukan controller maupun service yang menuliskan status putaran | `HRD-Q-49` |

---

### 2.9 Batas MVP setelah `HRD-DEC-035`

`HRD-DEC-035` memindahkan **orkestrasi putaran payroll** keluar dari jalur kritis MVP
administratif. Akibatnya bagi batas ini tegas dan perlu dibaca apa adanya:

| Pertanyaan | Jawaban |
| --- | --- |
| Apakah `HRD-Q-10` dan `HRD-Q-11` masih terbuka? | **Ya.** Keduanya tetap `EXTERNAL_OWNER / FINANCE` |
| Apakah keduanya memblokir MVP administratif? | **Tidak.** Keduanya hanya memblokir penyelesaian batas serah terima itu sendiri |
| Apa yang tetap dikerjakan HR pada MVP? | Menghasilkan **masukan HR yang siap payroll** — kesiapan kehadiran, masukan cuti, masukan lembur, rekonsiliasi sisi HR, dan validasi kesiapan |
| Apa yang pindah ke `POST-MVP`? | Pembuatan putaran payroll, pemajuan statusnya, perhitungan, persetujuan, dan serah terima final |

**Batas MVP berbunyi `HR produces payroll-ready HR inputs`, bukan
`HR creates/pays/finalizes payroll run`.**

**Dua penegasan yang mengikat:**

1. `Payroll Executed` **MUST NOT** dianggap sama dengan `Employee Paid`. Keduanya peristiwa
   berbeda, dimiliki pihak berbeda.
2. Modul HR **MUST NOT** membuat putaran payroll sebelum batas dengan Finance disepakati lewat
   keputusan tersendiri. `HRD-DEC-035` **tidak** menyetujui opsi itu; ia hanya menundanya.

**Bukti yang mendasari.** `TrxPayrollRun` terbukti **hanya pernah dibaca** (`AsNoTracking`),
tidak pernah dibuat maupun dimajukan. Jalur serah terima kehadiran mengandaikan putaran payroll
sudah ada, dan tidak ada yang membuatnya.

## 3. `INT-HR-02` — Jadwal praktik dokter

### 3.1 Keputusan yang mengikat

`[DECISION]` `HRD-DEC-006`, `approved` 27 Agustus 2026.

> **Jadwal kerja dan jadwal praktik adalah dua hal berbeda.** Jadwal kerja HR dipakai untuk
> kehadiran, lembur, dan tunjangan shift. Jadwal praktik dokter tetap milik Health Services dan
> dipakai untuk pendaftaran pasien. HR **bukan** sumber kebenaran jadwal praktik dan **bukan**
> jalur kritis pendaftaran pasien.

### 3.2 Bentuk integrasinya

**Tidak ada aliran data antar keduanya, dan itu disengaja.**

| Pertanyaan | Jawaban |
| --- | --- |
| Apakah HR mengirim jadwal kerja ke Health Services? | **Tidak.** |
| Apakah Health Services mengirim jadwal praktik ke HR? | **Tidak.** |
| Apakah pendaftaran pasien berhenti bila seluruh endpoint HR mati? | **Tidak.** Ini kriteria yang dapat diuji — `AC-F00-01` |

### 3.3 Titik sentuh yang tersisa, dan cara menanganinya

Satu-satunya persinggungan: seorang dokter melayani pasien pada jam yang **tidak ada** dalam
jadwal kerjanya.

`[DECISION]` `HRD-DEC-013` dan `HRD-DEC-025`:

1. Jam itu dicatat sebagai **pengecualian kehadiran yang menunggu keputusan atasan**;
2. Jenis pengecualiannya **baru dan terpisah**, bukan jenis yang berarti *jadwal tidak dapat
   diselesaikan*;
3. Atasan menentukan klasifikasinya: lembur, koreksi jadwal, tercatat tanpa kompensasi, atau
   klasifikasi resmi lain;
4. **Tidak pernah otomatis menjadi lembur.**

**Keadaan hari ini:** `MISSING` sepenuhnya. Tidak ada jalur kode yang mendeteksi skenario ini.

---

## 4. `INT-HR-03` — Akun aplikasi dan hak akses

| Aspek | Isi |
| --- | --- |
| Modul di seberang | Administrator / Identity |
| Yang dibutuhkan HR | Meminta pembuatan akun saat pegawai masuk; meminta pencabutan akses saat pegawai keluar |
| Yang **MUST NOT** dilakukan HR | Membuat tabel akun, role, atau permission sendiri |
| Keadaan kontrak | **`[OPEN]`** — `HRD-DEP-003`. Tidak ada bukti integrasi apa pun dari sisi HR |

### 4.1 Kenyataan yang harus disampaikan

**Pencabutan akun aplikasi tidak berjalan otomatis saat pegawai keluar.** Ini bukan dugaan —
source pengunduran diri sendiri memuat peringatan eksplisit tentang hal ini `[EXISTING]`.

Akibatnya, hari ini: seorang pegawai dapat menyelesaikan seluruh proses pengunduran diri di
dalam sistem, daftar periksa offboarding terbentuk, **dan akun aplikasinya tetap aktif** sampai
seseorang mencabutnya secara manual di luar sistem.

### 4.2 Yang perlu disepakati sebelum dirancang

| Pertanyaan | Pemilik |
| --- | --- |
| Bagaimana bentuk permintaan pembuatan akun dari HR ke Identity | Pemilik produk bersama Administrator/Identity |
| Bagaimana bentuk permintaan pencabutan akses | Sama |
| Apa yang terjadi bila akun belum ada saat pegawai mulai bekerja | Sama |
| Siapa yang bertanggung jawab bila pencabutan gagal | Sama |
| Apakah pencabutan bersifat langsung atau terjadwal pada tanggal terakhir bekerja | Sama |

**Sampai kelimanya dijawab, `INT-HR-03` MUST NOT dirancang.** Yang boleh dilakukan sekarang
hanyalah menyediakan tempat pada daftar periksa offboarding untuk mencatat bahwa pencabutan
sudah dikerjakan — dan itu pun sebagai catatan manual, bukan integrasi.

---

## 5. `INT-HR-04` — Penyimpanan berkas dan dokumen

| Aspek | Isi |
| --- | --- |
| Modul di seberang | Shared platform |
| Yang dibutuhkan HR | Menyimpan lampiran cuti, bukti koreksi kehadiran, sertifikat pelatihan, dokumen kepegawaian, lampiran persetujuan |
| Yang disimpan HR sendiri | **Metadata dan rujukan letak berkas**, bukan isi berkasnya |
| Keadaan kontrak | **`[OPEN]` sebagian.** Sudah dipakai hari ini lewat layanan penyimpanan berkas, tetapi kontraknya belum tertulis — `HRD-DEP-006` |

### 5.1 Perilaku yang sudah terbukti

| Perilaku | Keadaan |
| --- | --- |
| Unggah bukti koreksi kehadiran | `[EXISTING]` |
| Unggah lampiran permohonan cuti | `[EXISTING]` |
| Unggah lampiran pada instance persetujuan | `[EXISTING]` |
| Unggahan kedua **menggantikan** yang pertama | `[EXISTING]` — berkas baru disimpan, field basis data ditimpa, perubahan disimpan, **baru kemudian** berkas fisik lama dihapus. Tidak ada berkas yatim, dan tidak perlu memanggil endpoint hapus lebih dulu |
| Setiap simpanan memakai nama yang dibangkitkan acak | `[EXISTING]` — mencegah tabrakan nama selama jeda sebelum penghapusan |

### 5.2 Yang belum disepakati

| Pertanyaan | Pemilik |
| --- | --- |
| Berapa lama berkas disimpan sebelum boleh dihapus | Pemilik produk bersama shared platform |
| Batas ukuran dan jenis berkas yang diterima | Sama |
| Apa yang terjadi bila penyimpanan tidak dapat dihubungi saat pegawai mengunggah | Sama |
| Siapa yang boleh mengunduh berkas milik pegawai lain | Pemilik keamanan |

**Perilaku yang sudah dirancang untuk kegagalan unggah:** bila unggahan gagal, permohonan
**tetap tersimpan sebagai draft** dan berkasnya dapat diunggah ulang. Pegawai tidak kehilangan
isian yang sudah diketik.

---

## 6. `INT-HR-05` — Pemberitahuan kepada pegawai

| Aspek | Isi |
| --- | --- |
| Modul di seberang | **Belum diketahui pemiliknya** |
| Yang dibutuhkan HR | Memberi tahu pegawai saat pengajuannya diputuskan; saat ia dipanggil kembali dari cuti; saat koreksi dibuat atas namanya; saat tugas persetujuannya mendekati batas waktu |
| Keadaan kontrak | **`[OPEN]`** |

### 6.1 Empat kebutuhan pemberitahuan yang lahir dari keputusan yang sudah dikunci

| Kebutuhan | Sumber keputusan | Keadaan |
| --- | --- | --- |
| Pegawai diberi tahu setelah pemanggilan kembali disetujui, sebelum diterapkan | `HRD-DEC-024` | **Belum diverifikasi** apakah mekanismenya ada |
| Pegawai diberi tahu saat HR membuat koreksi atas namanya | `HRD-DEC-028` | **`MISSING`** |
| Penyetuju diingatkan saat tugasnya mendekati batas waktu | `HRD-DEC-030` | **`MISSING`** |
| Eskalasi dikirim saat batas waktu terlewat | `HRD-DEC-030` | **`MISSING`** |

### 6.2 Mengapa ini penting dan tidak boleh dilewat

`HRD-DEC-024` menyatakan pegawai **tidak boleh** memblokir keputusan pemanggilan kembali dengan
tidak mengonfirmasi. Tetapi keputusan itu hanya adil bila pegawai **benar-benar diberi tahu**
lebih dulu. Tanpa jalur pemberitahuan, penandaan "pemberitahuan sudah tersampaikan" oleh HR
Manager menjadi klaim yang tidak dapat dibuktikan.

Hal yang sama berlaku untuk `HRD-DEC-030`: mesin eskalasi yang tidak dapat mengirim pengingat
hanyalah pemindah tugas tanpa peringatan.

**Kesimpulan:** `INT-HR-05` adalah **prasyarat** bagi `HRD-DEC-024`, `HRD-DEC-028`, dan
`HRD-DEC-030` untuk benar-benar bekerja. Pemiliknya perlu ditetapkan sebelum ketiganya
diimplementasikan.

---

## 7. `INT-HR-06` dan `INT-HR-07` — Batas klinis

| Aspek | Isi |
| --- | --- |
| Modul di seberang | Health Services |
| Keadaan | **`[BLOCKED]`** — `HRD-DEP-005`, `HRD-DEP-007` |

Kedua batas ini **tidak dirancang** dan **tidak boleh dirancang** dari alur blueprint ini.

| ID | Batas | Mengapa terblokir |
| --- | --- | --- |
| `INT-HR-06` | Data klinis pasien, tindakan, dan volume layanan sebagai sumber angka penilaian praktik profesional | Penilaian praktik profesional (`S-C1`) `BLOCKED` menunggu `requirement-completeness-gate`, `hospital-domain-architect`, lalu Komite Medik |
| `INT-HR-07` | Pengecekan kewenangan klinis saat pelayanan berlangsung | Sama. Menetapkannya sekarang berarti mengarang batas kewenangan praktik dokter |

### 7.1 Satu-satunya yang boleh dicatat

`[DECISION]` `HRD-DEC-005`, berstatus **`draft`** menunggu Komite Medik:

> Kredensial dan kewenangan klinis yang kedaluwarsa **tidak** menghentikan pelayanan. HR
> menyediakan pengecekan dan daftar pantau kedaluwarsa; modul klinis menampilkan peringatan dan
> mencatat siapa yang tetap melanjutkan beserta alasannya.

Ini adalah **posisi sementara yang aman-gagal**, bukan desain. Dasarnya: sistem tidak boleh
menciptakan hambatan yang membahayakan pasien. Keputusan blokir keras ditahan sampai Komite Medik
memutuskan per skenario klinis — `HRD-Q-08`.

**Yang dilarang:** merancang bentuk API pengecekan, bentuk peringatan, maupun daftar skenario
mana yang boleh diblokir keras.

---

## 8. Sambungan di dalam modul HR

Keempat sambungan ini melintasi bounded context di dalam HR sendiri. Kesalahan di sini tidak
melibatkan modul lain, tetapi akibatnya sama besar.

### 8.1 `INT-HR-08` — Jadwal kerja menjadi acuan kehadiran

| Aspek | Isi |
| --- | --- |
| Arah | Penjadwalan → Kehadiran |
| Cara kerja | Pemroses kehadiran memanggil penyelesai jadwal untuk mengetahui jadwal yang berlaku pada satu tanggal `[EXISTING]` |
| Urutan sumber jadwal | Roster terbit, roster terkonfirmasi, roster selesai, jadwal kerja tetap, kehadiran jarak jauh, perjalanan dinas, penimpaan manual, atau tidak terselesaikan |
| Bila tidak terselesaikan | Hari itu tetap diproses, tetapi memunculkan pengecualian bertipe jadwal tidak terselesaikan `[EXISTING]` |
| Risiko | Salah urutan prioritas membuat kehadiran **seluruh unit** salah pada satu hari |

**Yang sudah terbukti dan tidak boleh dirusak:** tukar shift yang diterapkan **benar-benar
mengubah** apa yang dihitung pemroses kehadiran pada hari yang ditukar. Baris penugasan shift
ditandai bersumber tukar shift dan merupakan penimpaan manual, dan penyelesai jadwal
memungutnya tanpa pengecualian apa pun `[EXISTING]` — dibuktikan pada `PHASE 2B.1`.

### 8.2 `INT-HR-09` — Cuti yang berjalan menandai hari kehadiran

| Aspek | Isi |
| --- | --- |
| Arah | Cuti → Kehadiran |
| Cara kerja | Saat cuti mulai berjalan, sistem menulis baris integrasi kehadiran dan menandai hari-hari itu sebagai cuti `[EXISTING]` |
| Batas transaksi | Satu transaksi memuat perubahan status eksekusi, baris buku besar saldo, **dan** baris integrasi kehadiran |
| Idempotensi | Menjalankan ulang eksekusi yang sudah berjalan tidak memotong saldo dua kali `[EXISTING]` |

**Keterputusan yang harus diketahui sebelum kemampuan izin pulang cepat dirancang.** Cuti **per
jam** dan pengecualian kehadiran *pulang cepat* adalah **dua mekanisme yang terputus**. Baris
integrasi kehadiran hanya membawa jumlah menit; jam mulai dan jam selesai pada permohonan cuti
**tidak pernah** dibaca sisi kehadiran. Blok yang membebaskan pengecualian pulang cepat hanya
berjalan untuk cuti **sehari penuh** `[EXISTING]`.

Akibatnya hari ini: seorang pegawai yang mengajukan dan disetujui cuti per jam untuk pulang cepat
**tetap** dapat memperoleh pengecualian pulang cepat pada hari yang sama, karena kedua mekanisme
tidak saling mengetahui.

Ini **bukan cacat yang diperbaiki diam-diam** — ini fakta arsitektur yang harus diketahui supaya
kesalahan yang sama tidak diulang saat izin pulang cepat dirancang `[DECISION]` `HRD-DEC-029`.

### 8.3 `INT-HR-10` — Lembur dicocokkan dengan kehadiran nyata

| Aspek | Isi |
| --- | --- |
| Arah | Kehadiran → Lembur |
| Cara kerja | Perhitungan realisasi lembur membaca kehadiran nyata hari itu, lalu mencari waktu yang bertumpuk `[EXISTING]` |
| Hasil pencocokan | Siap, kehadiran belum diproses, kehadiran tidak ditemukan, kehadiran tidak lengkap, tidak ada waktu yang bertumpuk, tarif tidak dapat ditentukan, atau diblokir kebijakan |
| Risiko | Lembur dihitung dari rencana, bukan dari kehadiran nyata. Ini **tidak terjadi** hari ini, dan tidak boleh terjadi |

### 8.4 `INT-HR-11` — Persetujuan bersama melayani seluruh domain

| Aspek | Isi |
| --- | --- |
| Arah | Seluruh domain → Persetujuan, lalu kembali |
| Cara kerja | Setiap domain punya service integrasi workflow-nya sendiri yang membuat instance, dan service siklus hidup yang menerjemahkan status workflow kembali menjadi status domain `[EXISTING]` |
| Pemisahan yang wajib dijaga | `[DECISION]` `HRD-DEC-018` — instance persetujuan **MUST NOT** memuat aturan bisnis domain mana pun |
| Bukti pemisahan itu nyata | Langkah dan matriks persetujuan dibatasi per definisi workflow, sehingga cuti dan lembur benar-benar memakai aturan yang berbeda `[EXISTING]` |

**Kesenjangan yang tercatat:** batas waktu, pengingat, dan eskalasi ada sebagai **konfigurasi**
pada langkah workflow, tetapi **tidak ada satu pun pemrosesan terjadwal yang membacanya dan
bertindak** `[EXISTING]`. `[DECISION]` `HRD-DEC-030` menetapkan pembangunan mesin itu sebagai
target.

**Satu tabel yang tidak boleh dijadikan dasar desain.** Ditemukan satu tabel persetujuan cuti
lama yang hanya hidup di lapisan skema — ada sebagai konfigurasi dan tabel hasil migration,
tetapi **tidak ada satu pun jalur baca maupun tulis yang aktif**. Seluruh persetujuan cuti yang
benar-benar berjalan memakai mesin generik. Klasifikasinya: **`LEGACY_UNUSED`** di lapisan kode,
kandidat dipensiunkan sebagai target, dan **penghapusannya `[BLOCKED]`** oleh `HRD-Q-05` — tidak
ada bukti bahwa tabelnya kosong.

---

## 9. Ringkasan kesiapan integrasi

| ID | Batas | Boleh dirancang sekarang | Yang menghalangi |
| --- | --- | :---: | --- |
| `INT-HR-01` | Serah terima payroll | **Sebagian** — sampai batas `HRD-DEC-009` | `HRD-Q-10`, `HRD-Q-11` untuk sesudahnya |
| `INT-HR-02` | Jadwal praktik dokter | **Ya** — dan jawabannya adalah **tidak ada integrasi** | — |
| `INT-HR-03` | Akun aplikasi | **Tidak** | Kontrak dengan Identity belum ada — `HRD-DEP-003` |
| `INT-HR-04` | Penyimpanan berkas | **Sebagian** — pemakaiannya sudah berjalan | Kebijakan retensi dan batas berkas belum ada — `HRD-DEP-006` |
| `INT-HR-05` | Pemberitahuan | **Tidak** | Pemiliknya belum ditetapkan |
| `INT-HR-06` | Data klinis untuk penilaian praktik | **Tidak** | `S-C1` `BLOCKED` |
| `INT-HR-07` | Pengecekan kewenangan klinis | **Tidak** | `S-C1` `BLOCKED` |
| `INT-HR-08` | Jadwal ke kehadiran | **Ya** | — |
| `INT-HR-09` | Cuti ke kehadiran | **Ya**, dengan catatan keterputusan cuti per jam | `HRD-Q-47` untuk izin pulang cepat |
| `INT-HR-10` | Kehadiran ke lembur | **Ya** | — |
| `INT-HR-11` | Persetujuan bersama | **Ya** | — |

---

## 10. Traceability

| Batas | Decision ID | Flow |
| --- | --- | --- |
| `INT-HR-01` serah terima payroll | `HRD-DEC-009`; `HRD-Q-10`, `HRD-Q-11`, `HRD-Q-49` | `flows/10-payroll-processing-handoff.md` |
| `INT-HR-02` jadwal praktik | `HRD-DEC-006`, `HRD-DEC-013`, `HRD-DEC-025` | `flows/00-module-context-flow.md`, `flows/02-attendance.md` |
| `INT-HR-03` akun aplikasi | `HRD-DEP-003`; `HRD-Q-50` | `flows/11-lifecycle-offboarding.md` |
| `INT-HR-04` penyimpanan berkas | `HRD-DEP-006` | `flows/07-attendance-correction.md`, `flows/03-leave.md` |
| `INT-HR-05` pemberitahuan | `HRD-DEC-024`, `HRD-DEC-028`, `HRD-DEC-030` | `flows/03-leave.md`, `flows/09-unified-approval.md` |
| `INT-HR-06`, `INT-HR-07` batas klinis | `HRD-DEC-005` (`draft`); `HRD-DEP-005`, `HRD-DEP-007`, `HRD-Q-08` | Tidak ada flow — sengaja tidak dibuat |
| `INT-HR-08` jadwal ke kehadiran | — | `flows/05-work-scheduling.md`, `flows/06-shift-change-swap.md` |
| `INT-HR-09` cuti ke kehadiran | `HRD-DEC-029` | `flows/03-leave.md`, `flows/08-early-leave-permission.md` |
| `INT-HR-10` kehadiran ke lembur | — | `flows/04-overtime.md` |
| `INT-HR-11` persetujuan bersama | `HRD-DEC-011`, `HRD-DEC-018`, `HRD-DEC-030` | `flows/09-unified-approval.md` |

---

## 11. Perencanaan Tenaga Kerja — `S-D1` (`last_changed_in` `v7`, 3 November 2026)

`S-D1` hanya punya **satu** batas dengan modul lain, dan batas itu ditunda. Seluruh sambungan
lainnya berada di dalam modul HR. Tidak ada panggilan asinkron, antrean pesan, maupun sistem luar.

### 11.1 Batas dengan modul lain

#### `INT-HR-12` — Angka beban dari data pelayanan pasien

| Aspek | Isi |
| --- | --- |
| Arah | Health Services → HR |
| Kegunaan | Mengisi angka beban metode rasio, misalnya rata-rata pasien ICU terisi per hari |
| Keadaan | **Tidak dirancang pada rilis ini.** `HRD-DEP-005` `MISSING`; `HRD-DEC-060` `MANUAL_WORKLOAD_INPUT` menetapkan isian manual beserta sumbernya |
| Pengganti selama rilis ini | Layar angka beban; petugas menyalin dari rekap sensus yang sudah ada |
| Risiko bila dipaksakan sekarang | HR menjadi pembaca data pasien tanpa kontrak dan tanpa batas privasi |

### 11.2 Sambungan di dalam modul HR

| ID | Arah | Cara kerja | Bila sumber tidak tersedia | Risiko |
| --- | --- | --- | --- | --- |
| `INT-HR-13` | Perencanaan → Penjadwalan | Penjadwalan membaca standar berlaku per unit per tanggal lewat `GET .../workforce-planning/staffing-standards/effective` sebelum roster diterbitkan (`HRD-DEC-058` `SCHEDULING_READS_STANDARD`) | `404` berarti unit belum punya standar; Penjadwalan **MUST NOT** mengarang aturan minimum sendiri (`WFP_OWNS_STANDARD`) | Dua sumber aturan minimum bila Penjadwalan membuatnya sendiri |
| `INT-HR-14` | Penjadwalan → Perencanaan | Ringkasan kekurangan tenaga per unit per bulan masuk ke potret bulanan (`WFP_RECEIVES_SHORTAGE_SUMMARY`) | **Belum ada** — Penjadwalan belum memeriksa standar (`HRD-CAP-59`). Kolom `ShortageSummaryCount` tetap kosong | Rencana tahun depan tanpa bukti kekurangan harian. Bentuk kontraknya ditetapkan desain Penjadwalan; `S-D1` hanya menyediakan tempatnya |
| `INT-HR-15` | Workforce Core → Perencanaan | Membaca penempatan organisasi dan jabatan utama yang aktif per tanggal; membaca SK untuk kursi internal dan "akan kosong" | Penempatan jabatan yang belum diisi membuat lapisan terisi bertanda belum lengkap | Angka terisi salah bila penempatan lama bertanggal salah (`HRD-TF-013`) |
| `INT-HR-16` | Cuti, Kompetensi, Lifecycle → Perencanaan | Membaca cuti yang disetujui, hasil kompetensi aktif, dan pengunduran diri yang disetujui | Lapisannya kosong dan bertanda belum lengkap (`UNKNOWN_IS_NOT_ZERO`) | Gap kompetensi terbaca 0 padahal datanya belum ada |
| `INT-HR-17` | Perencanaan → Rekrutmen | Kursi luar yang siap direkrut muncul di antrean Recruiter; pemenuhan ditautkan manual dari penempatan (`HRD-DEC-066` `MANUAL_LINK_UNTIL_RECRUITMENT_EXISTS`) | Rekrutmen (`S-D2`) belum punya API; tautan manual tetap jalan | Rekrutmen di luar sistem tanpa tautan membuat kursi tidak pernah terpenuhi |
| `INT-HR-18` | Perencanaan ↔ Persetujuan Bersama | Lima jenis rujukan `T9` s.d. `T13`; keputusan akhir diteruskan ke domain. Langkah Direktur memakai matriks persetujuan dengan sumber `Position` dan penelusuran unit induk (`HRD-DEC-072`) | Penyetuju tidak ditemukan → pengajuan ditolak `400` (`WorkflowService.cs:534-541`) | Matriks tanpa baris cadangan membuat `T9` dan `T12` tidak dapat diajukan sama sekali |

**Idempotensi.** Keputusan akhir workflow dapat terkirim ulang. `HrdWorkforcePlanningWorkflowIntegrationService`
**MUST** memeriksa status dokumen sebelum menerapkan: keputusan "setujui" untuk dokumen yang sudah
`Approved`/`Applied`/`ReadyForRecruitment` diabaikan dan dicatat, bukan menghasilkan versi formasi
kedua. Scheduler harian juga **MUST** idempoten: menjalankannya dua kali pada hari yang sama tidak
mengubah kursi yang sudah `Fulfilled` dan tidak membuat potret bulanan kedua.

### 11.3 Ringkasan kesiapan

| ID | Batas | Boleh dirancang sekarang | Yang menghalangi |
| --- | --- | :---: | --- |
| `INT-HR-12` | Angka beban dari data pasien | **Tidak** | `HRD-DEP-005` |
| `INT-HR-13` | Standar ke Penjadwalan | **Ya** — kontrak bacanya pada `api-contract.md` bagian 13 | Pemakaiannya dirancang di sisi Penjadwalan |
| `INT-HR-14` | Ringkasan kekurangan ke Perencanaan | **Sebagian** — tempatnya ada, bentuknya menunggu desain Penjadwalan | `HRD-CAP-59` |
| `INT-HR-15` | Penempatan dan SK | **Ya** | — |
| `INT-HR-16` | Cuti, kompetensi, pengunduran diri | **Ya** | — |
| `INT-HR-17` | Antrean Recruiter | **Ya**, dengan tautan manual | `S-D2` `BLOCKED` untuk bentuk otomatisnya |
| `INT-HR-18` | Persetujuan `T9` s.d. `T13` | **Ya** | Pemetaan peran Finance / Anggaran dan Direktur untuk rilis |


---

## 12. Rekrutmen dan Hiring — `S-D2` (`last_changed_in` `v8`, 3 November 2026)

### 12.1 Batas dengan modul/sistem eksternal

#### `INT-HR-19` — Verifikasi Registrasi Tenaga Kesehatan (SatuSehat SDMK / Konsil KTKI-KKI)
- **Arah**: Kemenkes / Konsil -> HR Rekrutmen.
- **Kegunaan**: Verifikasi keaslian dan keaktifan nomor STR pelamar klinis secara realtime API.
- **Status Rilis Ini**: Ditunda (`POST-MVP`). Pada rilis ini pelamar mengunggah berkas STR dan Recruiter melakukan verifikasi manual terhadap web resmi KTKI/KKI (`HRD-DEC-076`).

### 12.2 Sambungan di dalam modul HR

| ID | Arah | Cara Kerja | Bila Sumber Tidak Tersedia | Risiko / Pencegahan |
| --- | --- | --- | --- | --- |
| `INT-HR-20` | Perencanaan (`S-D1`) -> Rekrutmen (`S-D2`) | `HrdJobRequisition` menarik alokasi kursi luar dari `HrdHeadcountRequest` berstatus `APPROVED` (`HRD-DEC-073`) | Jika formasi belum disetujui, requisition tidak dapat dibuat | Mencegah pembukaan lowongan tanpa plafon formasi |
| `INT-HR-21` | Master Data (`01`) -> Rekrutmen (`S-D2`) | Evaluasi nominal draf surat penawaran terhadap rentang `MstSalaryGrade` (`HRD-DEC-079`) | `404` jika grade tidak ditemukan; penawaran draf ditolak | Mencegah penawaran di luar skala upah rumah sakit |
| `INT-HR-22` | Occupational Health (`18`) -> Rekrutmen (`S-D2`) | Dokter K3RS menginput status kelaikan kerja (`FIT`, `UNFIT`, dll); modul rekrutmen membaca kesimpulan tanpa detail rekam medis (`HRD-DEC-078`) | Status kelaikan tetap `PENDING`; penawaran definitif tertahan | Melindungi privasi medis kandidat dan keselamatan kerja RS |
| `INT-HR-23` | Rekrutmen (`S-D2`) -> Workforce Core (`02`) | Serah terima kandidat `FINAL_ACCEPTED` membuat draf `WorkforceProfile` berstatus `Pending Onboarding` dengan deteksi NIK anti-duplikasi (`HRD-DEC-075`) | Jika gagal simpan, status handoff tetap `PENDING` dan dapat diulang (*retryable*) | Mencegah duplikasi entitas pegawai mantan karyawan |
| `INT-HR-24` | Rekrutmen (`S-D2`) -> Perencanaan (`S-D1`) | Saat penempatan resmi berlaku di Workforce Core, `FulfilledSeats` pada requisition dan headcount request bertambah otomatis | Jika penempatan dibatalkan sebelum berlaku, kursi kembali terbuka | Sinkronisasi akurat antara rekrutmen dan gap tenaga |

### 12.3 Idempotency dan Kegagalan Komunikasi

- **Submit Pendaftaran Pelamar**: Idempotent berdasarkan `NationalIdNumber` (NIK KTP). Submit berulang untuk NIK yang sama mengembalikan data kandidat yang sudah terdaftar.
- **Serah Terima Onboarding**: Idempotent berdasarkan `JobOfferId`. Eksekusi berulang tidak menciptakan ID profil pegawai ganda.
- **Penanganan Pembatalan Kursi**: Jika kandidat menolak penawaran (*Offer Rejected*) atau batal hadir dinas (*No-Show*), kursi otomatis kembali ke status `In-Progress`, memicu notifikasi promosi cepat bagi kandidat peringkat 1 di `HrdTalentPool` (`HRD-DEC-080`).


---

## 13. Onboarding dan Kontrak Kerja — `S-C4-ONB` (`last_changed_in` `v9`, 4 November 2026)

Status `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan". Seluruh sambungan di bawah **belum ada di kode**. Seluruh pihak berada di backend yang
sama; sambungan berupa pemanggilan service, bukan HTTP, kecuali disebut lain.

| ID | Arah | Isi | Sinkron? | Bila gagal | Idempotency |
| --- | --- | --- | :---: | --- | --- |
| `INT-HR-25` | 04 Rekrutmen → 05 | Serah terima `FinalAccepted` membuka kasus onboarding, draf kontrak pertama, dan permintaan akun dalam **satu transaksi** dengan serah terima (`HRD-DEC-083`) | Ya | Seluruhnya batal; serah terima tetap `Generated` dan dapat diulang | Unik `HiringHandoffId`; pengulangan mengembalikan kasus yang sama (`VR-ONB-01`) |
| `INT-HR-26` | 05 → 02 Workforce Core | Seluruh penulisan baris kontrak lewat `WfpContractHistoryWriteService` | Ya, ikut transaksi pemanggil | Transaksi 05 batal | Kunci advisory per pegawai; nomor kontrak unik per pegawai |
| `INT-HR-27` | 05 → 02 Workforce Core | Penempatan organisasi, jabatan, dan gaji awal saat kontrak pertama ditandatangani; relasi atasan saat siap kerja, lewat `WfpNewHireAssignmentService` | Ya, ikut transaksi pemanggil | Penandatanganan atau siap kerja batal | Satu penempatan utama per jenis per tanggal (`HRD-DEC-049`) |
| `INT-HR-28` | 05 → 01 Master Data HR | Salinan jenis dan status kepegawaian, tanggal masuk, tanggal akhir kontrak dan masa percobaan (`EmployeeEmploymentRecordService`); akun login (`EmployeeUserAccountService`) | Ya | Salinan: transaksi 05 batal. **Akun: tidak membatalkan kasus** — permintaan `Failed` dan dapat diulang (`VR-ONB-75`) | Satu akun per profil |
| `INT-HR-29` | 05 → 20 | Tidak Diperpanjang dan Tidak Lulus menjadi `TrxEmployeeSeparation` (`ContractNonRenewal`, `ProbationFailed`) berstatus disetujui, lalu exit clearance dibuka (`ResignationLifecycleHandoffService.HandoffLifecycleSeparationAsync`) | Ya, ikut transaksi penerapan keputusan | Penerapan keputusan batal; langkah inbox dapat diulang | Satu pemisahan per keputusan (`EmployeeSeparationId` tersimpan) |
| `INT-HR-30` | 05 ↔ `BC-HR-07` Persetujuan Bersama | `T15` hasil masa percobaan, `T16` keputusan kontrak, `T17` koreksi kontrak, `T18` batch data awal — masing-masing satu langkah `SiteHr`, `AllowSelfApproval = false` | Ya | Langkah tanpa penyetuju ditolak (`UNRESOLVED_STEP_FAILS_CLOSED`) | Satu instance per dokumen |
| `INT-HR-31` | 06 Penjadwalan → 05 | Kesiapan dijadwalkan per pegawai per tanggal: `Eligible`, `NotReady`, `ContractHold`, ditambah penanda Didampingi dan peringatan kredensial (`HrdEmploymentReadinessReadService`) | Ya, baca | 06 **MUST** menolak menjadwalkan bila kesiapan tidak dapat dibaca (gagal tertutup) | Baca saja |
| `INT-HR-32` | 05 → `CredentialingManagement` | Baca `WfpCertification` untuk butir sertifikat wajib; baca `WfpClinicalPrivilege` untuk peringatan kredensial | Ya, baca | Butir otomatis tetap `Pending`; peringatan kredensial tampil | Baca saja |
| `INT-HR-33` | 10 Payroll → 02/05 | Payroll membaca tanggal mulai kontrak (gaji sejak tanggal itu, `HRD-DEC-088` `PAY_FROM_CONTRACT_START`) dan jenis pemisahan `ContractNonRenewal` untuk uang kompensasi PKWT | Ya, baca | Milik 10 | Baca saja |
| `INT-HR-34` | 05 → Administrator/IT | Permintaan akses kerja `WorkAccess` muncul di layar permintaan akses; IT memberi peran lewat layar peran yang sudah ada lalu menandai selesai | Tidak — kerja manusia | Permintaan tetap `Requested` dan tampil di daftar | Satu permintaan per kasus |
| `INT-HR-35` | 04 Rekrutmen → 05 | Tawaran memuat jenis dan lama kontrak; draf kontrak pertama menyalinnya (`HRD-DEC-095`) | Ya | Tawaran tanpa jenis kontrak tidak dapat diajukan (`VR-ONB-50`) | — |

**Kesiapan integrasi.**

| Sambungan | Keadaan pihak lawan hari ini | Memblokir |
| --- | --- | --- |
| `INT-HR-25`, `INT-HR-35` | Model `S-D2` ada, service serah terima `BE-HRD-061` belum | Implementasi kasus otomatis; desain tidak |
| `INT-HR-27` | Service penempatan baru; aturan tumpang tindih sudah ada di `HrdInitialPlacementBatchService` | Tidak |
| `INT-HR-28` (akun) | Logika ada di `EmployeeController` (`HRD-TF-053`), perlu diekstrak | Tidak |
| `INT-HR-29` | `ResignationLifecycleHandoffService` ada (`HRD-TF-054`) | Tidak |
| `INT-HR-31` | Penjadwalan 06 belum dirancang ulang (`S-B4`) | Rilis penahan jadwal menunggu 06 membaca kontrak ini |
| `INT-HR-34` | Pemetaan peran `MAPPING_REQUIRED` (`HRD-DEC-032`) | Rilis akses kerja |


---

## 7. Titik Sambung Slice `S-B4` — Penjadwalan, Roster, dan On-Call (`last_changed_in` `v10`, 4 November 2026)

| ID | Arah integrasi | Deskripsi kontrak | Synchronous / Asynchronous | Perilaku bila pihak lawan gagal | Catatan |
| --- | --- | --- | --- | --- | --- |
| `INT-HR-36` | 06 → 07 Kehadiran | `AttendanceScheduleResolverService` membaca `TrxShiftAssignment` berstatus `Published` sebagai jadwal primer pegawai shift. Koreksi mundur jadwal memicu rekalkulasi absensi | Sync (baca) & Async (event rekalkulasi) | Bila service gagal, kehadiran memakai jadwal tetap bawaan; rekalkulasi di-retry via message queue | Menutup `HRD-TF-055` a |
| `INT-HR-37` | 06 ↔ Health Services (Laboratorium) | `LabConfirmingDoctorResolver` (`LAB-DEC-111`) membaca `TrxOnCallAssignment` berstatus `Active` untuk dokter konfirmator hasil kritis lab. Status `Active` tetap terjaga saat terjadi pemanggilan | Sync (baca) | Dokter konfirmator gagal ditemukan; lab melakukan eskalasi manual ke dokter jaga onsite | Menutup `HRD-TF-053` & `HRD-DEC-103` |
| `INT-HR-38` | 06 → 09 Lembur & 10 Payroll | Penugasan siaga standby diteruskan ke Payroll untuk tunjangan siaga. Transaksi pemanggilan fisik (*call-in*) diteruskan ke modul Lembur/Payroll sebagai hak insentif panggil cito | Async (event call-in & rekap bulanan) | Transaksi disimpan lokal di 06 dan di-reconcile saat proses cut-off payroll | `HRD-DEC-103` |
| `INT-HR-39` | 06 → Health Services (Nurse View) | Modul Nurse View membaca roster shift terbit untuk menentukan perawat jaga dan penanggung jawab shift ruangan (`NV-DEC-021`) | Sync (baca) | Tampilan Nurse View fallback ke daftar staf unit tanpa penanda shift aktif | `HRD-TF-055` d |
| `INT-HR-40` | 06 ← 05 Lifecycle (`INT-HR-31`) | Validasi 06 mengecek profil pegawai: siap kerja (`HRD-DEC-088`), batas kontrak aktif (`HRD-DEC-089`), dan status `Didampingi` (`HRD-DEC-087`, `HRD-DEC-100`) | Sync (baca) | **Gagal tertutup:** bila status profil tidak terbaca, sistem memblokir penjadwalan (`B6`) | Melaksanakan janji `INT-HR-31` |


---

## 8. Bagian `S-B1` — Kontrak Integrasi Administrasi Kehadiran Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

### 8.1 Daftar Kontrak Integrasi `INT-HR-41` s.d. `INT-HR-44`

| ID Kontrak | Modul Terkait | Jenis Integrasi | Arah Data | Deskripsi & Titik Sentuh |
| --- | --- | :---: | :---: | --- |
| `INT-HR-41` | IT Hardware / Ingestion Service | Asinkron (Event/REST) | Inbound | Penyerapan log mentah biometrik (mesin fingerprint/face/mobile) secara *immutable* ke `HrdAttendanceRawLog`. |
| `INT-HR-42` | Sub-Modul 06 (`S-B4` Penjadwalan) | Sinkron (Service Read) | Inbound | Pembacaan penugasan shift roster aktif (`TrxShiftAssignment`) per tanggal & staf oleh mesin `ScheduleResolver`. |
| `INT-HR-43` | Sub-Modul 06 (`S-B4` Penjadwalan) | Asinkron (Internal Event) | Inbound | Menerima sinyal rekalkulasi dari koreksi mundur roster (`INT-HR-38`) dan memicu hitung ulang jam kerja harian. |
| `INT-HR-44` | Sub-Modul 10 (`S-B5` Payroll) | Sinkron (REST/Command) | Outbound | Penyerahan rekapitulasi kehadiran final via `AttendancePayrollHandoff.Execute` dan eksekusi `Rollback` bila periode dibuka kembali. |


---

## 9. Bagian `S-B2` — Kontrak Integrasi Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Bagian ini mendokumentasikan batas integrasi teknis, kontrak antarmuka (*interface contract*), dan protokol pertukaran data untuk **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit** dengan modul-modul terkait.

### 9.1 Ringkasan Titik Integrasi Slice `S-B2`

| ID Integrasi | Modul Sumber | Modul Sasaran | Protokol / Pola | Sifat | Deskripsi Singkat |
| --- | --- | --- | :---: | :---: | --- |
| `INT-HR-45` | `LeaveManagement` (08) | `SchedulingManagement` (06) | In-Process Service Call | Sinkron | Query ketersediaan tenaga unit (*Safe Staffing Coverage*) untuk mendeteksi ancaman kekurangan perawat shift (`HRD-DEC-111`). |
| `INT-HR-46` | `LeaveManagement` (08) | `SchedulingManagement` (06) | Domain Event / Service Call | Asinkron | Pemulihan status jadwal shift pegawai menjadi aktif (*Scheduled*) setelah leave recall darurat disetujui HR Manager (`HRD-DEC-113`). |
| `INT-HR-47` | `LeaveManagement` (08) | `AttendanceManagement` (07) | In-Process Query Contract | Sinkron | Resolusi status presensi harian staf cuti menjadi `OnLeave` agar tidak terhitung mangkir atau lupa tap (`HRD-DEC-104`). |
| `INT-HR-48` | `LeaveManagement` (08) | `PayrollManagement` (10) | Batch Snapshot Handoff | Terjadwal | Ekspor rekonsiliasi retrospektif cuti pending yang baru disetujui pasca-cut-off payroll bulan lalu (`HRD-DEC-114`). |

### 9.2 Rincian Spesifikasi Integrasi

#### 9.2.1 `INT-HR-45`: Evaluasi Kuota Tenaga Unit 24/7 (Safe Staffing Coverage Warning)
* **Konteks:** Menjaga rasio keselamatan pasien (*patient safety*) saat staf klinis mengajukan cuti.
* **Kontrak Antarmuka:**
  ```csharp
  public interface ISafeStaffingQueryService
  {
      Task<StaffingCoverageEvaluationResult> EvaluateCoverageAsync(
          Guid unitId, 
          DateOnly targetDate, 
          Guid? shiftId, 
          CancellationToken cancellationToken = default);
  }

  public record StaffingCoverageEvaluationResult(
      bool IsWarningTriggered,
      int MinimumRequiredStaff,
      int ScheduledStaffCount,
      int RemainingStaffIfApproved,
      string ShiftName,
      string UnitName,
      string AdvisoryMessage
  );
  ```
* **Penanganan Kegagalan (Fallback):** Jika service penjadwalan gagal diakses, sistem menggunakan prinsip *fail-open with audit warning*: permohonan cuti tetap diteruskan ke kotak masuk atasan disertai catatan audit: *"Evaluasi kuota jadwal tidak dapat diverifikasi secara otomatis; mohon verifikasi jadwal manual"*.

#### 9.2.2 `INT-HR-46`: Pemulihan Jadwal Shift Pasca Leave Recall
* **Konteks:** Mengembalikan kesiapan kerja perawat/dokter yang dipanggil dinas darurat RS.
* **Payload Event:**
  ```json
  {
    "eventId": "EVT-RECALL-RESTORE-20261113-001",
    "eventType": "LeaveRecallExecutedEvent",
    "workforceProfileId": "e3b0c442-98fc-1c14-9af3-4c5678901234",
    "effectiveRecallDate": "2026-11-13",
    "restoredDays": 2.0,
    "leaveRequestId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "executedByHrManagerId": "c4d5e6f7-a8b9-0123-cdef-456789012345",
    "timestamp": "2026-11-12T14:30:00Z"
  }
  ```
* **Tindakan Target:** Modul 06 menerima event ini dan memperbarui entitas `TrxShiftAssignment` pada rentang `effectiveRecallDate` ke depan dari status `OnLeave` menjadi `Scheduled` aktif.

#### 9.2.3 `INT-HR-47`: Resolusi Presensi Harian Staf Cuti
* **Konteks:** Mengintegrasikan permohonan cuti disetujui ke jadwal kehadiran presensi harian modul 07.
* **Kontrak Antarmuka:** `IHrdLeaveReadService.IsWorkforceOnApprovedLeaveAsync(workforceId, targetDate)`. Jika mengembalikan `true`, maka `HrdAttendanceDaily` otomatis diberi status `OnLeave` dengan pengecualian absensi dibebaskan (*Waived*).

#### 9.2.4 `INT-HR-48`: Serah Terima Rekonsiliasi Retrospektif ke Payroll
* **Konteks:** Menjaga hak penghasilan staf dan akurasi keuangan RS atas cuti yang disetujui terlambat pasca closing payroll.
* **Kontrak Data:** Objek snapshot `LeavePayrollReconciliationBatchDto` berisi daftar cuti bulan lampau yang disetujui setelah cut-off, jumlah hari pemotongan/tunjangan yang disesuaikan, dan nomor dokumen rujukan.


---

## 10. Bagian `S-B3` — Kontrak Integrasi Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Bagian ini mendefinisikan antarmuka pertukaran data, event domain sinkron/asinkron, struktur payload, serta penanganan kegagalan (*fallback strategy*) untuk **Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit**.

### 10.1 Ringkasan Peta Integrasi

| ID Integrasi | Arah Integrasi | Target Modul / Komponen | Protokol / Pola | Frekuensi | Keterangan Fungsional |
| :--- | :---: | :--- | :---: | :---: | :--- |
| `INT-HR-49` | Outbound | Sub-modul 08 Leave Management | Domain Event (Asinkron) | Per Event Approval | Kredit saldo cuti pengganti (CTO) ke `TrxLeaveBalanceLedger` |
| `INT-HR-50` | Inbound | Sub-modul 07 Attendance Management | Direct Service Call (C#) | Realtime saat verifikasi | Kueri log jam tap biometrik aktual mesin absensi |
| `INT-HR-51` | Inbound | Sub-modul 06 Scheduling Management | Event / Service Query | Realtime saat submit | Validasi jam kerja reguler & aktivasi lembur tugas on-call medis |
| `INT-HR-52` | Outbound | Sub-modul 10 Payroll Management | Snapshot Transfer (REST/DTO) | Bulanan (Cut-off tgl 20) | Serah terima data lembur terverifikasi & rapel retrospektif |

---

### 10.2 Rincian Kontrak Integrasi

#### 10.2.1 `INT-HR-49`: Kredit Saldo Cuti Pengganti (CTO) ke Buku Besar Cuti (08)
* **Konteks:** Menyalurkan hak libur pengganti atas lembur yang disetujui opsi CTO ke buku besar saldo cuti pegawai (`HRD-DEC-118`).
* **Payload Event:**
  ```json
  {
    "eventId": "EVT-CTO-GRANT-20261114-0042",
    "eventType": "CompensatoryTimeOffGrantedEvent",
    "workforceProfileId": "e3b0c442-98fc-1c14-9af3-4c5678901234",
    "overtimeRealizationId": "f7a8b9c0-1234-5678-9abc-def012345678",
    "leaveTypeCode": "CTO",
    "grantedHours": 8.0,
    "grantedDays": 1.0,
    "effectiveDate": "2026-11-15",
    "expiryDate": "2027-02-13",
    "referenceDocNumber": "OTR/2026/11/0142",
    "timestamp": "2026-11-14T20:45:00Z"
  }
  ```
* **Tindakan Target:** Modul 08 menerima event ini dan memposting mutasi kredit baru pada `TrxLeaveBalanceLedger` bertipe `CompensatoryTimeOffGrant` dengan penanda kedaluwarsa 90 hari kalender.

#### 10.2.2 `INT-HR-50`: Kueri Log Presensi Biometrik Aktual dari Sub-Modul 07
* **Konteks:** Memverifikasi kesesuaian waktu dinas lembur fisik terhadap mesin absensi (`HRD-DEC-119`).
* **Kontrak Antarmuka:**
  ```csharp
  public interface IAttendanceBiometricQueryService
  {
      Task<AttendanceDailyBiometricLogDto> GetBiometricLogForDateAsync(
          Guid workforceProfileId, 
          DateTime targetDate, 
          CancellationToken ct = default
      );
  }
  ```
* **Struktur Kembalian (`AttendanceDailyBiometricLogDto`):**
  - `bool HasBiometricTap`: Menandakan apakah ada catatan fisik tap masuk/keluar.
  - `DateTime? ActualCheckIn`: Jam tap masuk mesin presensi.
  - `DateTime? ActualCheckOut`: Jam tap keluar mesin presensi.
  - `int TotalPhysicalExcessMinutes`: Selisih kelebihan waktu kerja di luar shift reguler.
* **Penanganan Kegagalan (Fallback):** Bila service presensi 07 mengalami downtime, sistem menandai klaim dengan status `PendingBiometricRetry` dan menjadwalkan ulang kueri melalui background scheduler tanpa memblokir input klaim staf.

#### 10.2.3 `INT-HR-51`: Validasi Jadwal Roster & Aktivasi Tugas On-Call Medis
* **Konteks:** Memastikan lembur tidak bertabrakan dengan shift reguler di modul 06 serta mengotomasi klaim lembur darurat cito bagi dokter/nakes jaga on-call yang dipanggil bertugas aktif ke RS (`HRD-ASM-24`).
* **Kontrak Data:** Kueri status penugasan shift pada jam pelaksanaan lembur. Jika staf sedang dalam status siaga `OnCallActiveDuty`, sistem secara otomatis mengizinkan pembukaan klaim lembur pasca-dinas cito tanpa memerlukan form SPKL pra-dinas.

#### 10.2.4 `INT-HR-52`: Serah Terima Rekonsiliasi Lembur & Rapel Retrospektif ke Payroll (10)
* **Konteks:** Menyerahkan paket data lembur bulanan yang telah dikunci snapshot immutable pada cut-off tgl 20 serta rapel retrospektif periode lalu ke modul penggajian (`HRD-DEC-121`).
* **Kontrak Data Snapshot:**
  ```json
  {
    "periodYear": 2026,
    "periodMonth": 11,
    "cutoffDate": "2026-11-20",
    "totalPayableHours": 1420.5,
    "totalPayableEmployees": 84,
    "currentMonthItems": [
      {
        "workforceProfileId": "e3b0c442-98fc-1c14-9af3-4c5678901234",
        "overtimeRealizationId": "f7a8b9c0-1234-5678-9abc-def012345678",
        "workDate": "2026-11-12",
        "payableHoursTier1": 1.0,
        "payableHoursTier2": 2.5,
        "isHolidayRate": false
      }
    ],
    "retrospectiveAdjustmentItems": [
      {
        "workforceProfileId": "b1c2d3e4-5678-90ab-cdef-1234567890ab",
        "originalPeriodYear": 2026,
        "originalPeriodMonth": 10,
        "overtimeRealizationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "payableHoursTier1": 1.0,
        "payableHoursTier2": 1.5,
        "adjustmentReason": "Late Approval Discrepancy Resolved"
      }
    ]
  }
  ```
* **Penanganan Kegagalan:** Serah terima bersifat atomik dua arah. Jika modul Payroll belum siap menerima, status periode tetap `CutoffLocked` dan proses transfer dapat dicoba ulang tanpa mengubah isi data lembur yang terkunci.


## 11. Bagian S-B5 — Kontrak Integrasi Penggajian Pegawai Rumah Sakit (Sub-modul 10, last_changed_in v14, 5 November 2026)

Bagian ini mendefinisikan kontrak integrasi lintas bounded context untuk siklus pemrosesan gaji (Payroll), mencakup serah terima paket pembayaran final ke modul Finance & Kasir Rumah Sakit, penerimaan sinyal callback konfirmasi pembayaran/penolakan dari Finance, penarikan potongan pinjaman karyawan dari Sub-modul 11, serta pembaruan tabel tarif pajak PPh 21 TER PMK 168/2023.

### 11.1 INT-HR-53: Serah Terima Berkas Final Penggajian ke Modul Finance & Kasir
* **Arah Integrasi:** Human Resource (Payroll Engine) $\rightarrow$ Finance & Accounting (Cashier / Treasury / General Ledger)
* **Karakteristik Komunikasi:** Asynchronous Message Queue / Direct REST Webhook dengan Verifikasi Checksum SHA-256 dan Idempotency-Key.
* **Tujuan:** Menyerahkan daftar instruksi pembayaran gaji bersih (*net pay*) seluruh pegawai rumah sakit yang telah berstatus `ApprovedFinal` kepada Bagian Keuangan untuk diproses pencairan bank (*disbursement*) dan pencatatan jurnal akuntansi.
* **Triggers:** Approval final Direktur SDM/Keuangan atas batch penggajian (`trx_payroll_runs` status `ApprovedFinal` $\rightarrow$ `HandoffReady`).
* **Endpoint / Event Target:** `POST /api/v1/finance/payroll-handoffs/receive-batch`
* **Struktur Payload Serah Terima (`PayrollFinanceHandoffPayload`):**
  ```json
  {
    "handoffBatchId": "d4c3b2a1-8765-4321-cba0-987654321def",
    "payrollRunId": "c3b2a109-7654-3210-ba98-876543210cba",
    "periodYear": 2026,
    "periodMonth": 11,
    "batchNumber": "PHB-202611-001",
    "paymentMethod": "BankTransferBatch",
    "sourceBankAccountId": "acc-rs-operasional-mandiri-01",
    "totalEmployees": 320,
    "totalGrossAmount": 2150000000.00,
    "totalDeductionsAmount": 345000000.00,
    "totalNetAmount": 1805000000.00,
    "currency": "IDR",
    "handoffTimestamp": "2026-11-23T14:30:00Z",
    "handedOverByUserId": "usr-dir-keuangan-01",
    "integrityChecksum": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
    "disbursementLines": [
      {
        "detailId": "a1b2c3d4-0001-4000-8000-000000000001",
        "workforceProfileId": "e3b0c442-98fc-1c14-9af3-4c5678901234",
        "employeeNik": "EMP-2024-001",
        "employeeName": "dr. Siti Aminah, Sp.A",
        "destinationBankCode": "MANDIRI",
        "destinationBankAccountNumber": "1370012345678",
        "destinationAccountHolder": "Siti Aminah",
        "transferAmount": 18500000.00,
        "paymentReference": "PAY-202611-EMP001"
      }
    ]
  }
  ```
* **Jaminan Integritas & Keamanan:** Checksum SHA-256 dihitung dari urutan data kanonikal seluruh baris `disbursementLines`. Jika modul Finance mendeteksi selisih 1 Rupiah pun atau perbedaan hash, batch serah terima akan ditolak secara otomatis (*integrity failure*).
* **Penanganan Kegagalan & Retry:** Modul HRD menerapkan mekanisme pengiriman ulang bertahap (*exponential backoff retry*: 1m, 5m, 15m) hingga maksimum 5 kali bila layanan Finance tidak merespons. Status handoff tetap `Dispatched` hingga Finance mengembalikan konfirmasi penerimaan (*Ack*).

---

### 11.2 INT-HR-54: Callback Konfirmasi Pembayaran / Penolakan Batch dari Finance
* **Arah Integrasi:** Finance & Accounting $\rightarrow$ Human Resource (Payroll Engine)
* **Karakteristik Komunikasi:** Webhook Callback / Event-Driven Consumer (`payroll.finance.disbursement.status`)
* **Tujuan:** Menginformasikan sistem HRD bahwa proses transfer bank oleh Finance telah berhasil diselesaikan (*PaymentConfirmed*) atau ditolak/gagal karena anomali rekening/likuiditas (*PaymentRejected*), untuk memicu pelepasan slip gaji digital kepada pegawai atau rollback status terkendali.
* **Endpoint Penerima HRD:** `POST /api/v1/hr/payroll/handoffs/callback`
* **Struktur Callback Sukses (`FinancePaymentConfirmationCommand`):**
  ```json
  {
    "handoffBatchId": "d4c3b2a1-8765-4321-cba0-987654321def",
    "payrollRunId": "c3b2a109-7654-3210-ba98-876543210cba",
    "status": "PaymentConfirmed",
    "bankDisbursementReference": "TRX-MANDIRI-CORP-20261125-9988",
    "actualDisbursedAt": "2026-11-25T08:15:22Z",
    "confirmedByFinanceOfficer": "usr-fin-treasury-02",
    "notes": "Seluruh transfer kliring batch Mandiri Corporate berhasil cair."
  }
  ```
* **Struktur Callback Penolakan (`FinanceRejectionCallbackCommand`):**
  ```json
  {
    "handoffBatchId": "d4c3b2a1-8765-4321-cba0-987654321def",
    "payrollRunId": "c3b2a109-7654-3210-ba98-876543210cba",
    "status": "PaymentRejected",
    "rejectionReason": "Ditemukan nomor rekening tidak valid pada 2 pegawai dan limit harian bank belum dinaikkan.",
    "rejectedByFinanceOfficer": "usr-fin-manager-01",
    "rejectedAt": "2026-11-24T10:00:00Z"
  }
  ```
* **Aturan Perubahan Status & Rollback Terkendali (`HRD-DEC-126`):**
  1. Bila `status == PaymentConfirmed`: Status `trx_payroll_runs` berubah menjadi `Completed`, status `trx_payroll_handoff_batches` menjadi `PaymentConfirmed`, dan pemicu pelepasan slip gaji `trx_payroll_payslips` (`Released`) aktif pada tgl 25.
  2. Bila `status == PaymentRejected`: Status `trx_payroll_runs` dialihkan ke `NeedCorrection`, gembok immutable dibuka secara terkendali dengan audit log lengkap, dan payroll officer dapat melakukan koreksi data nomor rekening pegawai.

---

### 11.3 INT-HR-55: Integrasi Penarikan Potongan Koperasi, Pinjaman & Restitusi (Sub-modul 11)
* **Arah Integrasi:** Human Resource Sub-modul 11 (Kesejahteraan & Benefit) $\rightarrow$ Sub-modul 10 (Payroll Engine)
* **Karakteristik Komunikasi:** Internal Service Query / Inter-Context Read Model
* **Tujuan:** Menyerap total tagihan angsuran pinjaman koperasi karyawan, pinjaman darurat rumah sakit, atau selisih restitusi klaim kesehatan yang wajib dipotongkan langsung dari slip gaji bulan berjalan.
* **Endpoint / Query:** `GET /api/v1/hr/welfare/deductions/active-installments?periodYear=2026&periodMonth=11`
* **Struktur Respons Data Potongan (`ActivePayrollDeductionResponse`):**
  ```json
  [
    {
      "workforceProfileId": "e3b0c442-98fc-1c14-9af3-4c5678901234",
      "deductionCategory": "CooperativeLoanInstallment",
      "referenceNumber": "LOAN-KOP-2026-088",
      "installmentNumber": 5,
      "totalInstallments": 12,
      "installmentAmount": 500000.00,
      "remainingPrincipal": 3500000.00,
      "notes": "Angsuran pinjaman belanja koperasi bulan ke-5"
    },
    {
      "workforceProfileId": "b1c2d3e4-5678-90ab-cdef-1234567890ab",
      "deductionCategory": "HospitalEmergencyLoan",
      "referenceNumber": "EMG-RS-2026-012",
      "installmentNumber": 2,
      "totalInstallments": 3,
      "installmentAmount": 1000000.00,
      "remainingPrincipal": 1000000.00,
      "notes": "Pinjaman darurat berobat keluarga"
    }
  ]
  ```
* **Aturan Proteksi Take-Home Pay:** Total akumulasi potongan pinjaman tidak boleh membuat take-home pay pegawai menjadi negatif atau bernilai di bawah batas minimum tunjangan operasional yang ditetapkan regulasi internal rumah sakit (maksimal 50% dari penghasilan kotor).

---

### 11.4 INT-HR-56: Integrasi Sinkronisasi Tabel Tarif Pajak PPh 21 TER PMK 168/2023
* **Arah Integrasi:** Central Master Configuration / Tax Table Repository $\rightarrow$ Payroll Calculation Engine
* **Karakteristik Komunikasi:** Read-through Cache & Sync Event
* **Tujuan:** Memastikan rumus pemotongan PPh 21 bulanan menggunakan tabel resmi Tarif Efektif Rata-Rata (TER Kategori A, B, dan C) sesuai Peraturan Menteri Keuangan Nomor 168 Tahun 2023, serta tarif progresif Pasal 17 UU HPP untuk masa pajak terakhir (Desember).
* **Struktur Parameter Tarif TER:**
  ```json
  {
    "effectiveYear": 2026,
    "regulationBasis": "PMK-168/PMK.03/2023",
    "ptkpCategories": [
      { "category": "TER_A", "ptkpStatus": ["TK/0", "TK/1", "K/0"], "brackets": 44 },
      { "category": "TER_B", "ptkpStatus": ["TK/2", "TK/3", "K/1", "K/2"], "brackets": 40 },
      { "category": "TER_C", "ptkpStatus": ["K/3"], "brackets": 41 }
    ],
    "lastUpdated": "2026-01-02T00:00:00Z",
    "version": "v2024.1"
  }
  ```
* **Pencegahan Human Error:** Parameter tarif pajak disimpan secara terpusat pada `hrd_payroll_settings` dan tabel master pajak tanpa hardcoded nilai di dalam kode program C#, sehingga perubahan regulasi pemerintah dapat disesuaikan tanpa redeploy aplikasi.

## 12. Bagian S-B6 — Kontrak Integrasi Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, last_changed_in v15, 5 November 2026)

Bagian ini mendefinisikan kontrak integrasi antar-modul dan eksternal (**`INT-HR-57` s.d. `INT-HR-60`**) untuk **Sub-modul 11 (Restitution & Payroll Deduction Integration — Slice `S-B6`)**, mencakup sinkronisasi jadwal piutang dengan Keuangan, callback realisasi pemotongan pasca gajian, protokol voucher kasir darurat 24/7, dan pelunasan pinjaman penutupan kerja.

---

### 12.1 `INT-HR-57`: Sinkronisasi Jadwal Cicilan Piutang Aktif Finance (Inbound Finance $
ightarrow$ HRD)

- **Tujuan Integrasi:** Menarik daftar komitmen angsuran pinjaman, kasbon staf, dan cicilan koperasi yang tercatat pada buku besar piutang Departemen Keuangan (*source of truth*) untuk disiapkan pada penggajian bulan berjalan (`HRD-DEC-129`).
- **Jadwal & Frekuensi Eksekusi:** Terjadwal otomatis setiap tanggal **17 pukul 01:00 WIB** (H-3 sebelum batas akhir penutupan absensi/lembur tanggal 20).
- **Protokol & Endpoint:** REST API HTTPS over TLS 1.3, Mutual TLS (mTLS) + Bearer Service Token.
  - Endpoint Penyedia (Finance): `GET /api/v1/finance/receivables/payroll-deductions/active`
  - Query Params: `periodMonth=11&periodYear=2026`
- **Spesifikasi Kontrak Payload Response:**
```json
{
  "syncTimestamp": "2026-11-17T01:00:15Z",
  "periodMonth": 11,
  "periodYear": 2026,
  "totalActiveContracts": 142,
  "totalScheduledAmount": 128500000.00,
  "items": [
    {
      "externalReferenceId": "FIN-LN-2025-0811",
      "employeeId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
      "employeeNip": "KRY-2022-0045",
      "category": "KasbonPinjamanRS",
      "priorityLevel": 2,
      "totalPrincipal": 10000000.00,
      "monthlyInstallment": 1000000.00,
      "currentInstallmentNumber": 4,
      "totalInstallments": 10,
      "remainingPrincipal": 7000000.00
    }
  ]
}
```
- **Penanganan Kesalahan & Ketahanan Jaringan:** Jika Finance tidak merespons dalam 30 detik (timeout), modul HRD melakukan *retry* eksponensial (3x pada menit ke-5, ke-15, dan ke-30). Jika tetap gagal, sistem menerbitkan alert prioritas tinggi kepada Administrator Payroll dan Finance untuk sinkronisasi manual sebelum tanggal 20.

---

### 12.2 `INT-HR-58`: Callback Eksekusi Realisasi Pemotongan Penggajian (Outbound HRD $
ightarrow$ Finance)

- **Tujuan Integrasi:** Mengirimkan bukti realisasi pemotongan gaji aktual yang berhasil dieksekusi setelah siklus penggajian bulanan disahkan dan ditransfer (tgl 25) guna memotong saldo buku besar piutang pegawai di Departemen Keuangan secara atomik (`HRD-DEC-129`).
- **Jadwal & Pemicu:** Otomatis dipicu segera setelah status siklus penggajian berubah menjadi `Approved` / `HandoffReady` pada tanggal 25.
- **Protokol & Endpoint:** Asynchronous Webhook dengan HMAC-SHA256 Signature Header `X-Quilvian-Signature`.
  - Endpoint Konsumen (Finance): `POST /api/v1/finance/receivables/payroll-deductions/execution-callback`
- **Spesifikasi Kontrak Payload Request:**
```json
{
  "payrollRunId": "b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e",
  "disbursementDate": "2026-11-25T08:30:00Z",
  "executionSummary": {
    "totalDeductedSuccessfully": 115000000.00,
    "totalDeferredDueToCap": 13500000.00,
    "affectedEmployeeCount": 142
  },
  "executions": [
    {
      "externalReferenceId": "FIN-LN-2025-0811",
      "employeeId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
      "scheduledAmount": 1000000.00,
      "actualDeductedAmount": 1000000.00,
      "deferredAmount": 0.00,
      "status": "DeductedInFull",
      "newRemainingPrincipal": 6000000.00
    },
    {
      "externalReferenceId": "FIN-COOP-2026-0342",
      "employeeId": "f7e8d9c0-b1a2-3f4e-5d6c-7b8a9e0f1a2b",
      "scheduledAmount": 1500000.00,
      "actualDeductedAmount": 500000.00,
      "deferredAmount": 1000000.00,
      "status": "PartiallyDeducted",
      "deferralReason": "Batas 50% PP 36/2021 tercapai. Sisa tagihan dialihkan ke siklus Desember 2026.",
      "newRemainingPrincipal": 2500000.00
    }
  ]
}
```
- **Idempotensi & Konfirmasi:** Finance wajib mengembalikan HTTP `200 OK` dengan payload konfirmasi nomor jurnal transaksi piutang. Jika Finance gagal memproses, event masuk antrean *dead-letter queue* untuk rekonsiliasi otomatis tanpa mengubah status slip gaji yang sudah terlanjur ditransfer.

---

### 12.3 `INT-HR-59`: Kliring Voucher Pencairan Darurat Kasir RS (Outbound HRD $
ightarrow$ Kasir RS 24/7)

- **Tujuan Integrasi:** Menyediakan verifikasi instan secara online dan real-time bagi loket Kasir Rumah Sakit 24/7 saat mencairkan dana tunai restitusi darurat medis staf (*Urgent Disbursement Cito* sesuai `HRD-DEC-133`).
- **Pihak Terlibat:** Modul HRD (Penerbit Voucher) $
ightarrow$ Modul Kasir RS (Loket Pelayanan Kasir 24 Jam).
- **Protokol:** REST API Internal dengan enkripsi token satu kali pakai (One-Time Token SHA-256).
- **Alur Transaksi Kliring Kasir:**
  1. Nakes menunjukkan QR Code voucher pada gawai ESS ke loket kasir IGD.
  2. Kasir memindai barcode / menginput nomor voucher: `GET /api/v1/corporate/hrd/restitutions/cashier-vouchers/{voucherNumber}/verify?token={token}`.
  3. Modul HRD mengembalikan data validasi: Status `Valid`, Nama Pegawai, Unit Kerja, dan Jumlah Rupiah Tunai yang wajib diserahkan.
  4. Kasir menyerahkan uang dan menekan tombol konfirmasi: `POST /api/v1/corporate/hrd/restitutions/cashier-vouchers/{voucherNumber}/disburse`.
  5. Sistem HRD seketika mengubah status klaim menjadi `DisbursedViaCashier`, mengunci token, dan mencoret klaim dari antrean transfer slip gaji bulanan.

---

### 12.4 `INT-HR-60`: Penyelesaian Sisa Piutang pada Formulir Exit Clearance (HRD Sub-11 $
ightarrow$ HRD Sub-20 Exit Clearance)

- **Tujuan Integrasi:** Menghubungkan modul pemotongan gaji dengan proses penyelesaian masa kerja staf rumah sakit (Exit Clearance Sub-Modul 20) saat seorang pegawai mengundurkan diri (*resign*) atau terkena PHK (`HRD-DEC-132`).
- **Titik Pemicu Integrasi:** Terbitnya draf formulir Exit Clearance staf di Sub-Modul 20.
- **Aturan Integrasi Antar-Sub-Modul:**
  1. Sub-Modul 20 mengirim permintaan audit piutang ke Sub-Modul 11 via internal query service.
  2. Sub-Modul 11 menarik seluruh saldo pokok pinjaman aktif staf dari tabel `trx_payroll_deduction_schedules`.
  3. Sub-Modul 11 menerbitkan komponen `FinalSeparationDeduction` untuk dihitung pada slip penutupan (*Final Clearance Payslip*), memotong hingga 100% dari hak pesangon dan hak sisa cuti.
  4. Bila seluruh utang lunas, Sub-Modul 11 mengirim sinyal hijau `FinancialObligationsCleared = true` ke Sub-Modul 20.
  5. Bila hak finansial tidak mencukupi, Sub-Modul 11 menerbitkan `OutstandingReceivableStatement` berkode bayar kasir dan menahan persetujuan formulir Exit Clearance (`FinancialObligationsCleared = false`) hingga kasir mengonfirmasi setoran pelunasan tunai.


---

## 13. Kontrak Integrasi Sub-Modul 12 (Slice S-D3 Benefit Management, INT-HR-61 s.d. INT-HR-64)

Bagian ini mendefinisikan spesifikasi kontrak integrasi teknis antara **Sub-Modul 12 (Benefit Management — Slice `S-D3`)** dengan sistem eksternal rumah sakit (SIMRS Registrasi & Kasir, Portal BPJS Kesehatan/TK) serta sub-modul internal HRD (Sub-11 Restitusi & Pemotongan Gaji, Sub-20 Exit Clearance).

---

### 13.1 `INT-HR-61`: Integrasi Real-Time SIMRS Admission & Cashiering
- **Tujuan Integrasi:** Memfasilitasi layanan berobat nakes dan keluarga inti di fasilitas RS Quilvian sendiri (`HRD-DEC-136`). Petugas pendaftaran SIMRS memvalidasi eligibilitas status staf, dan kasir SIMRS memotong saldo plafon tahunan nakes pada buku besar `HrdBenefitBalanceMovement` secara real-time saat transaksi ditutup.
- **Tipe Interaksi:** Sinkronus (REST API over TLS 1.3 internal LAN).
- **Arah Aliran Data:** Dua arah (`SIMRS Admission/Billing` <-> `HRD Benefit Management`).
- **Endpoint Terkait:**
  - `GET /api/v1/corporate/hrd/benefits/inhouse/coverage-inquiry`
  - `POST /api/v1/corporate/hrd/benefits/inhouse/deduct-balance`
- **Spesifikasi Payload Deduct Balance:**
  ```json
  {
    "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "simrsRegistrationNumber": "REG-20261105-00123",
    "patientNationalId": "3171012304950001",
    "grossBillAmount": 1500000.00,
    "discountAppliedAmount": 450000.00,
    "plafonDeductedAmount": 1050000.00,
    "excessAmount": 0.00,
    "serviceDate": "2026-11-05T10:30:00Z"
  }
  ```
- **SLA & Keandalan:** Waktu respons maksimal 500 ms (P99). Jika koneksi ke HRD mengalami kegagalan/timeout, kasir SIMRS menerapkan mode fallback offline: mencatat nomor kartu pegawai secara manual dan mengirimkan batch sinkronisasi asinkronus saat koneksi pulih.
- **Pencegahan Double-Deduction:** Menggunakan kunci idempoten berbasis `simrsRegistrationNumber` (indeks unik pada kolom referensi buku besar).

---

### 13.2 `INT-HR-62`: Integrasi Sinkronisasi Data BPJS Kesehatan & Ketenagakerjaan
- **Tujuan Integrasi:** Memfasilitasi pendaftaran massal kepesertaan BPJS Kesehatan PPU dan BPJS Ketenagakerjaan pegawai baru serta pemutakhiran data tanggungan keluarga inti ke portal resmi BPJS.
- **Tipe Interaksi:** Asinkronus (Batch API / Export Berkas Terstandarisasi e-Dabu & SIPP BPJS).
- **Arah Aliran Data:** Satu arah (`HRD Benefit Management` -> `Portal BPJS e-Dabu / SIPP`).
- **Frekuensi Eksekusi:** Otomatis setiap tanggal 10 dan 20 setiap bulan, atau on-demand saat onboarding pegawai baru selesai.
- **Struktur Berkas Handoff:** Format file CSV/Excel enkripsi terstandarisasi BPJS mencakup NIK 16 digit, Nama Lengkap, Nomor KK, Tanggal Lahir, Upah Dasar Perhitungan Iuran (Sub-10), dan Kode Faskes Tingkat Pertama (FKTP) pilihan nakes.
- **Penanganan Gagal:** Baris data dengan NIK tidak terdaftar di Disdukcapil diisolasi ke dalam laporan anomali (*reconciliation exception report*) untuk diverifikasi ulang oleh HR Benefit Officer.

---

### 13.3 `INT-HR-63`: Integrasi Pendelegasian Ekses Biaya Berobat ke Pemotongan Gaji (`S-B6`)
- **Tujuan Integrasi:** Menyelesaikan sisa tagihan berobat (*out-of-pocket excess*) nakes di RS Quilvian yang melebihi batas plafon tahunan, di mana pegawai memilih untuk melunasinya melalui pemotongan slip gaji bulan berjalan (Sub-Modul 11 `S-B6`).
- **Tipe Interaksi:** Asinkronus (Message Queue / Domain Event `OnHospitalCareExcessGenerated`).
- **Arah Aliran Data:** Satu arah (`HRD Benefit Management` -> `HRD Restitution & Deduction Integration Sub-11`).
- **Spesifikasi Payload Event:**
  ```json
  {
    "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "simrsRegistrationNumber": "REG-20261105-00123",
    "excessAmount": 750000.00,
    "deductionCategory": "HospitalCareExcess",
    "targetPayrollPeriod": "2026-11",
    "authorizationSignedLetterPath": "/storage/hrd/excess-auth/AUTH-EXC-20261105-01.pdf"
  }
  ```
- **Ketentuan Bisnis:** Pemotongan ekses medis ini tunduk pada aturan *Hard-Cap 50%* PP 36/2021 dan hirarki prioritas ke-2 (kewajiban RS) pada Sub-Modul 11. Jika sisa batas 50% tidak mencukupi, sisa ekses otomatis dijadwalkan ke siklus cicilan bulan berikutnya.

---

### 13.4 `INT-HR-64`: Integrasi Penonaktifan Benefit & Kliring Medis Exit Clearance (Sub-20)
- **Tujuan Integrasi:** Memastikan seluruh fasilitas asuransi dan subsidi berobat internal dinonaktifkan tepat pada tanggal efektif pemutusan hubungan kerja (`EffectiveTerminationDate`), serta memvalidasi bebas tunggakan medis ekses pada checklist Exit Clearance Sub-Modul 20 (`HRD-DEC-139`).
- **Tipe Interaksi:** Sinkronus & Event-Driven (`ExitClearance Sub-20` <-> `HRD Benefit Management`).
- **Arah Aliran Data:** Dua arah.
- **Alur Kerja Integrasi:**
  1. Saat pengajuan resign/PHK diproses, checklist Exit Clearance Sub-20 memanggil query verifikasi `CheckBenefitObligationsCleared(employeeId)`.
  2. Modul Benefit memeriksa apakah ada tagihan ekses rawat inap yang belum terbayar di kasir atau belum dialokasikan ke pesangon penutupan. Jika masih ada tunggakan, gerbang Exit Clearance berstatus `BLOCKED`.
  3. Tepat pada pukul 23:59:59 WIB pada `EffectiveTerminationDate`, event listener `OnEmployeeTerminated` mengubah `TrxEmployeeInsuranceEnrollment.status = 'Terminated'` dan menerbitkan berkas PDF Surat Pengantar Mutasi Mandiri BPJS Kesehatan.


---

## 14. Kontrak Integrasi Sub-Modul 13 (Slice S-D4 HR Service Management & Request — Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS)

Bagian ini mendefinisikan seluruh antarmuka integrasi antar-modul (*integration contracts*) untuk **Sub-Modul 13 (Slice `S-D4`)**, mencakup pemutakhiran otomatis data profil master kepegawaian Sub-02, pengarsipan permanen e-Dossier, penarikan riwayat slip gaji final Sub-10, serta pemantauan dan eskalasi SLA melalui sistem notifikasi rumah sakit.

---

### 14.1 `INT-HR-65`: Integrasi Pemutakhiran Otomatis Profil Kepegawaian ke Sub-Modul 02 (`WorkforceCore`)
- **Tujuan Integrasi:** Mengeksekusi mutasi perubahan data pribadi non-transaksional staf rumah sakit (rekening bank payroll, status pernikahan, kontak darurat KTP) secara otomatis pada basis data induk Sub-02 (`MstWorkforceProfile` & `TrxEmployeeBankDetail`) tepat setelah tiket diverifikasi dan disetujui resmi oleh analis HR Desk (`HRD-DEC-143`).
- **Tipe Interaksi:** Sinkronus Terisolasi via Service Interface `IWorkforceProfileSyncService` dalam transaksi database atomik.
- **Arah Aliran Data:** Satu arah (`HrServiceManagement` -> `WorkforceCore Sub-02`).
- **Alur Kerja Integrasi:**
  1. Analis HR mengklik "Setujui Permohonan Pemutakhiran" pada `FE-HRD-183`.
  2. `WorkforceProfileSyncService` membuka transaksi atomik:
     a. Mengambil snapshot data lama dari `mst_workforce_profiles` atau `trx_employee_bank_details`.
     b. Menyimpan rekaman sebelum-dan-sesudah ke tabel `hrd_profile_change_histories`.
     c. Menjalankan query update data baru pada entitas master Sub-02.
  3. Jika gagal, transaksi di-rollback penuh tanpa merusak status tiket; jika sukses, tiket ditandai `Resolved` dan event `HRS_PROFILE_CHANGE_AUTO_APPLIED` dipicu.
- **Payload Data Mutasi:**
  ```json
  {
    "hrServiceRequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "employeeId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "componentModified": "Bank_Account",
    "payload": {
      "bankName": "Bank Central Asia (BCA)",
      "bankAccountNumber": "8812093812",
      "bankAccountHolderName": "Ns. Siti Rahmawati, S.Kep",
      "bankBranch": "KCP RS Quilvian",
      "supportingDocumentPath": "/storage/hrd/requests/202611/BukuTabungan_Siti.pdf"
    },
    "approvedByUserId": "1b9d6bcd-bbfd-4b2d-9b5d-ab8dfbbd4bed"
  }
  ```

---

### 14.2 `INT-HR-66`: Integrasi Pengarsipan Permanen Dokumen Resmi ke E-Dossier Sub-02
- **Tujuan Integrasi:** Menyimpan salinan berkas digital (PDF resmi bertanda tangan elektronik dan ber-QR Code) yang telah berstatus `Issued` ke dalam repositori berkas kepegawaian (*Employee E-Dossier*) Sub-Modul 02 (`HRD-DEC-145`).
- **Tipe Interaksi:** Sinkronus via Service `IEmployeeDossierArchiveService`.
- **Arah Aliran Data:** Satu arah (`HrServiceManagement` -> `WorkforceCore Sub-02`).
- **Ketentuan Bisnis:** Berkas yang tersimpan di e-Dossier diklasifikasikan ke dalam kategori dokumen `Official_Letters` (Surat Resmi Kepegawaian), bersifat abadi (*read-only archive*), dan langsung muncul di tab "Dokumen Saya" pada antarmuka portal ESS pegawai. Jika dokumen dicabut keabsahannya (`Revoked`), berkas tidak dihapus dari e-Dossier melainkan disematkan watermark visual "REVOKED / TIDAK BERLAKU".

---

### 14.3 `INT-HR-67`: Integrasi Penarikan Data Gaji Final dari Sub-Modul 10 (`PayrollProcessing`)
- **Tujuan Integrasi:** Menyediakan data riwayat penghasilan staf medis yang valid dan akurat untuk penyusunan Surat Keterangan Gaji (*Salary Certificate* KPR/Bank) tanpa memperkenankan input manual (`HRD-DEC-142`).
- **Tipe Interaksi:** Sinkronus Query via Interface `IPayrollHistoryQueryService` (Read-Only).
- **Arah Aliran Data:** Satu arah (`PayrollProcessing Sub-10` -> `HrServiceManagement`).
- **Ketentuan Bisnis:**
  1. Service membaca slip gaji terakhir yang berstatus `Finalized` dari `trx_payroll_payslips` (`INT-HR-56`).
  2. Data yang diekstrak mencakup Gaji Pokok, Tunjangan Tetap, Total Penerimaan Kotor (Bruto), Total Potongan Wajib, dan Penerimaan Bersih (Netto).
  3. Jika pegawai belum memiliki slip gaji final (misal: pegawai baru bulan pertama), sistem menolak pembuatan draf dengan pesan instruksi resmi (`VR-HRS-05`).

---

### 14.4 `INT-HR-68`: Integrasi Notifikasi Eskalasi SLA ke Sistem Pesan Rumah Sakit (*Notification Hub*)
- **Tujuan Integrasi:** Mengirimkan peringatan dini (*early warning*) kepada HR Supervisor pada 75% durasi SLA dan notifikasi eskalasi merah keterlambatan kepada Manajer SDM pada 100% durasi SLA (`HRD-DEC-141`).
- **Tipe Interaksi:** Asinkronus via Message Bus / SignalR Hub (`INotificationDispatcherService`).
- **Arah Aliran Data:** Satu arah (`HrServiceManagement` -> `Hospital Notification Hub`).
- **Kanal Pengiriman:** Notifikasi In-App UI (lonceng notifikasi header), Webhook bot internal rumah sakit, dan email dinas resmi supervisor/manajer SDM.


---

## 15. Kontrak Integrasi Sub-Modul 14 (Slice `S-C1` Credentialing, License & Clinical Privilege)

Bagian ini mendefinisikan seluruh titik integrasi lintas domain dan sistem eksternal untuk **Sub-Modul 14 (Slice `S-C1`)**, mencakup penegakan wewenang klinis SIMRS/EMR real-time (`INT-HR-69`), pertukaran data evaluasi kinerja OPPE dengan Sub-15 (`INT-HR-70`), penguncian otomatis jadwal shift Sub-06 saat lisensi mati (`INT-HR-71`), serta pengarsipan berkas sah ke e-Dossier Sub-02 (`INT-HR-72`).

---

### 15.1 `INT-HR-69`: Integrasi Real-Time Penegakan Batas Kewenangan Klinis SIMRS/EMR Gate
- **Tujuan Integrasi:** Menyediakan antarmuka validasi gerbang wewenang klinis performa tinggi (*Clinical Gate*) yang dipanggil langsung oleh sistem pelayanan SIMRS / Electronic Medical Record (EMR) setiap kali tenaga medis hendak meresepkan obat keras, mengorder tindakan medis invasif, atau menjadwalkan prosedur operasi bedah, menyelesaikan posisi sementara `HRD-DEC-005` menjadi *Hard-Stop* bertingkat (`HRD-DEC-149`).
- **Tipe Interaksi:** Sinkronus API RESTful performa tinggi ber-cache Redis terdistribusi dengan target latensi p99 < 50ms.
- **Arah Aliran Data:** SIMRS / EMR (Consumer) -> Sub-Modul 14 Credentialing Management (Provider).
- **Spesifikasi Payload Request:**
  ```json
  {
    "workforceProfileId": "c4d32a8b-1111-4a9c-9c71-abcdef123456",
    "procedureCatalogCode": "PROC-SURG-LAP-001",
    "hospitalUnitId": "u9f8e7d6-2222-4b8a-8a62-123456abcdef",
    "patientEpisodeId": "EPISODE-RAWAT-INAP-20261105-089"
  }
  ```
- **Spesifikasi Payload Response:**
  ```json
  {
    "gateVerdict": "ALLOW_UNDER_SUPERVISION", // ALLOW_INDEPENDENT, ALLOW_UNDER_SUPERVISION, DENIED_NOT_PRIVILEGED, DENIED_LICENSE_EXPIRED
    "supervisionLevel": 3,
    "assignedSupervisorDoctorId": "d8e7f6a5-3333-4c9b-7b53-fedcba654321",
    "assignedSupervisorDoctorName": "dr. Bambang Soeprapto, Sp.B-KBD",
    "spkIssuanceNumber": "SPK/RSQ-DIR/XI/2026/0045",
    "spkValidUntil": "2029-11-05",
    "requiresCoSignature": true,
    "coSignatureDeadlineHours": 24,
    "denialReason": null
  }
  ```
- **Penanganan Kegagalan / Fallback:** Jika cluster cache Redis tidak dapat dijangkau, sistem beralih langsung (*failover*) ke query indeks komposit database PostgreSQL (`workforce_profile_id`, `privilege_catalog_id`, `is_suspended`). Jika koneksi jaringan HRD dan SIMRS putus total (*network partition*), SIMRS beralih ke mode darurat lokal dengan wajib membubuhkan flag audit `EMERGENCY_OFFLINE_OVERRIDE` yang dilaporkan saat koneksi pulih.

---

### 15.2 `INT-HR-70`: Integrasi Penarikan Rekam Jejak Kinerja OPPE dari Sub-Modul 15
- **Tujuan Integrasi:** Menarik agregasi nilai evaluasi praktik profesional berkelanjutan (*Ongoing Professional Practice Evaluation* - OPPE) dan audit insiden keselamatan pasien dokter dari Sub-Modul 15 Performance Management saat sistem menginisiasi draf berkas rekredensialing berkala pada H-60 (`HRD-DEC-151`).
- **Tipe Interaksi:** Asinkronus Event / Internal Service Bus Query via `IOppePerformanceQueryService`.
- **Arah Aliran Data:** Sub-Modul 15 Performance Management (Provider) -> Sub-Modul 14 Credentialing (Consumer).
- **Dataset yang Ditukarkan:** `WorkforceId`, `EvaluationCycleYears` (3 tahun), `OverallOppeScore` (0-100), `ClinicalCompetenceScore`, `PatientSafetyIncidentCount`, `MedicalRecordCompletenessRate`, `EthicsDisciplineComplianceFlag`.
- **Penanganan Kegagalan:** Jika Sub-15 belum menyelesaikan finalisasi nilai tahunan berjalan, sistem menandai draf berkas rekredensialing dengan status `OPPE_PENDING_CLOSURE` dan memberikan peringatan pada antrean sekretariat komite.

---

### 15.3 `INT-HR-71`: Integrasi Pemblokiran Roster Jadwal Dinas Sub-Modul 06 Saat SIP Kedaluwarsa
- **Tujuan Integrasi:** Mengirimkan sinyal otomatis (*fail-closed lock*) ke Sub-Modul 06 Scheduling & Roster Management tepat saat tanggal kedaluwarsa lisensi SIP (H-0 pukul 00:00) tercapai, guna memblokir nakes dari penugasan roster jaga bangsal/IGD/OK dan mencegah risiko hukum malpraktik faskes tanpa izin operasional (`HRD-DEC-146`, `CRD-B2`).
- **Tipe Interaksi:** Asinkronus Domain Event `OnSipExpiredIntegrationEvent` melalui message broker / MediatR event handler.
- **Arah Aliran Data:** Sub-Modul 14 Credentialing Management (Publisher) -> Sub-Modul 06 Scheduling Management (Subscriber).
- **Dampak pada Sub-06:** Seluruh slot shift jadwal dinas yang belum berjalan otomatis ditandai status `BLOCKED_LICENSE_EXPIRED`, memicu peringatan merah pada dashboard kepala ruangan untuk melakukan penggantian nakes (*Ganti Petugas* sesuai `HRD-Q-103`).

---

### 15.4 `INT-HR-72`: Integrasi Sinkronisasi Master Pegawai & E-Dossier Sub-Modul 02 (`WorkforceCore`)
- **Tujuan Integrasi:** 
  1. Menarik data profil induk pegawai klinis (nama lengkap bergelar, NIP, rumpun profesi, unit penempatan primer) dari Sub-02.
  2. Mengarsipkan secara otomatis berkas digital PDF Surat Penugasan Klinis (SPK) dan RKK yang telah disahkan Direktur Utama ke repositori berkas elektronik abadi pegawai (*Employee E-Dossier* di `mst_workforce_profile_documents`) sesuai [`HRD-DEC-148`].
- **Tipe Interaksi:** Sinkronus Service Call via `IWorkforceDossierArchiveService`.
- **Arah Aliran Data:** Sub-Modul 14 Credentialing (Publisher/Writer) -> Sub-Modul 02 Workforce Core (Storage).
- **Karakteristik Arsip:** Metadata arsip mencakup `DocumentCategory = 'CLINICAL_PRIVILEGE_SPK'`, nomor register SPK/RKK, hash SHA-256 berkas, dan penanda retensi abadi (*immutable*).
