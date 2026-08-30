# Human Resource — Kontrak Integrasi

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/integration-contract.md` |
| `contract_version` | `v1` |
| `last_changed_in` | `v1` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`), bersama pemilik modul tetangga untuk setiap batas |
| `approved_by` / `approved_at` | **Belum ada** |
| `input_revision` | `00-interview-decisions.md` revision `10`; `flows/00-module-context-flow.md` |
| `input_hash` — decision log | `91d62d4ea81aa11fd5bf4c1c922b6c8dbe1ad273a1609e4897bae0ecafa590c0` |
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
