# Laboratorium — Rekonsiliasi Bukti Lapangan

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Reconciliation ID | `LAB-REC-001` |
| Revision | `4` |
| Status | `draft` |
| Verdict putaran 1 | **`RECONCILED`** — 5 pertentangan ditutup `LAB-DEC-025` sampai `LAB-DEC-029`; 11 kemampuan dimasukkan scope lewat `LAB-DEC-030` |
| **Verdict putaran 2** | **`RECONCILED`** — 2026-09-15. Ketiga pertentangan ditutup `LAB-DEC-060` sampai `LAB-DEC-063` pada hari yang sama. Satu penahan baru terbuka: `LAB-COORD-010`, jalur baca status pembayaran milik Billing |
| **Verdict putaran 3** | **`PARTIALLY_RECONCILED`** — 2026-09-16. Sepuluh klarifikasi diadopsi `LAB-DEC-064` sampai `LAB-DEC-073`. **Tiga dari lima pertentangan ditutup pada hari yang sama**; `REC3-CONF-003` dan `REC3-CONF-005` tetap terbuka karena **bukan wewenang pemilik modul sendiri**. Satu penahan baru: `LAB-COORD-011` |
| Revision efektif | `4` — pertentangan putaran 1 dan 2 sudah ditutup; dari lima yang dibuka putaran 3, tiga ditutup dan dua tersisa |

## 0. Penutupan — 2026-09-01

Pemilik modul mengadopsi `Analisis_Konsolidasi_Modul_Laboratorium.md` sebagai baseline
requirement. Kelima pertentangan ditutup sebagai berikut.

| Pertentangan | Ditutup oleh | Arah keputusan |
|---|---|---|
| `REC-CONF-001` cakupan disiplin | `LAB-DEC-025` | **Diperluas** menjadi tiga disiplin. `LAB-DEC-002` `superseded`. Bank Darah tetap di luar |
| `REC-CONF-002` letak penanda cito | `LAB-DEC-026` | **Dipindahkan** ke pemeriksaan, ditambah penanda Duplo. `LAB-DEC-013` `amended` |
| `REC-CONF-003` bentuk hasil | `LAB-DEC-027` | **Diperluas** menjadi empat bentuk. `LAB-DEC-021` `superseded` |
| `REC-CONF-004` pendaftaran pasien | `LAB-DEC-028` | Laboratorium **memiliki** jalur pendaftaran pasien datang langsung dan rujukan luar |
| `REC-CONF-005` tarif | `LAB-DEC-029` | Tarif **ditampilkan dan dikelola**; keputusan uang tetap milik Billing |

Sebelas kemampuan pada bagian 5 dimasukkan scope lewat `LAB-DEC-030` dengan pembagian Rilis 1,
2, dan 3. Baseline alur ujung ke ujung diadopsi lewat `LAB-DEC-031`.

**Biaya yang muncul bersamaan.** Analisis konsolidasi membuka delapan hal tata kelola yang
sebelumnya tidak terlihat — matriks kewenangan, urutan status resmi, aturan pembatalan dan
koreksi, alur nilai kritis, integrasi alat, kebijakan audit, aturan tagihan, dan penyelarasan
antaraplikasi. Kedelapannya dicatat sebagai `LAB-P0-001` sampai `LAB-P0-008` dan **memblokir
sebagian besar cakupan baru**.

---
| Tanggal | 2026-09-01 |
| Sifat | **Read-only** terhadap repository aplikasi |

> **Kenapa dokumen ini ada.** Pemilik modul menyetujui blueprint pada 2026-09-01, dan pada saat
> yang sama menyerahkan tiga artifact hasil pengamatan sistem laboratorium yang berjalan hari
> ini. Bukti itu **mengubah gambaran**. Menimpa blueprint diam-diam agar terlihat cocok adalah
> tindakan yang salah; yang benar adalah memetakan selisihnya secara terbuka lalu meminta
> keputusan.

---

## 1. Bukti Baru yang Diproses

| Sumber | Sistem yang terlihat | Cakupan |
|---|---|---|
| `Laboratorium 1.md` | Aplikasi Laboratorium baru | Monitoring per kategori, OTC/rujukan, katalog dan tarif, input hasil Mikrobiologi dan Patologi Anatomi |
| `Artifact_Laboratorium_Bagian_Kedua.md` | Aplikasi Laboratorium baru, HiSys lama, RS MMC App | Registrasi, penerimaan specimen, input/konfirmasi hasil, mikrobiologi, patologi anatomi, nilai kritis, laporan |
| `Laboratorium_Bagian_Ketiga.md` | HiSys, Sysmex HCLAB | Daftar order, registrasi OTC, detail order/specimen, workstation, hasil, otorisasi, komunikasi hasil, hasil eksternal |

### Wewenang bukti ini

Menurut urutan wewenang pada `completeness-assessment-contract`, ketiga artifact adalah
**bukti sistem yang berjalan dan bukti analis** — tingkat 4 sampai 7. Keputusan pemilik modul
pada wawancara adalah **tingkat 1**.

**Artinya:** bukti ini **tidak otomatis membatalkan** keputusan yang sudah `approved`. Yang
dilakukannya adalah menunjukkan bahwa sebagian keputusan diambil tanpa mengetahui apa yang
sebenarnya dikerjakan laboratorium setiap hari. Keputusannya tetap milik pemilik modul.

Seluruh artifact juga menyatakan audio **tidak ditranskripsi**, sehingga aturan yang hanya
disampaikan lisan belum tercakup. Dokumen ini mewarisi batasan itu.

---

## 2. Ringkasan Dampak

| Kategori | Jumlah |
|---|---:|
| Keputusan `approved` yang **dikuatkan** bukti | 8 |
| Keputusan `approved` yang **bertentangan** dengan bukti | 5 |
| Kemampuan nyata yang **belum tercakup** blueprint | 11 |
| Pertanyaan terbuka blueprint yang **terjawab** bukti | 3 |

---

## 3. Keputusan yang Dikuatkan Bukti

Delapan keputusan justru terbukti benar. Ini penting dicatat agar tidak ikut dibongkar.

| Keputusan | Bukti pendukung | Catatan |
|---|---|---|
| `LAB-DEC-022` — validasi dan rilis adalah dua kewenangan terpisah | `Bagian Ketiga` RULE-005: "Hasil final menyimpan identitas validator dan otorisator sebagai **dua atribut terpisah**" | Dikuatkan langsung. Sysmex HCLAB memisahkan `Validated by` dan `Authorised by` |
| `LAB-DEC-024` — wadah fisik dipisahkan dari pemeriksaan | Specimen punya jenis, volume, nomor, dan waktu sendiri, terpisah dari daftar pemeriksaan | Dikuatkan |
| `LAB-DEC-006`, `LAB-DEC-018` — batas nilai sebagai data induk | Menu **`Nilai Rujukan`** ada tersendiri pada aplikasi Laboratorium baru | Dikuatkan. Bahkan sudah menjadi menu, bukan sekadar kolom |
| `LAB-DEC-004` — nilai kritis wajib dilaporkan dan tercatat | RS MMC App Nilai Kritis; dialog Phone Result pada HCLAB merekam penerima, waktu, hasil, petugas | Dikuatkan kuat |
| `LAB-DEC-012` — pemberitahuan tersimpan, bukan sekadar dikirim | Nilai kritis punya daftar, analisis, verifikasi, dokter lantai/perawat, dan ekspor | Dikuatkan |
| `LAB-INH-008` — diterima tidak sama dengan dinyatakan layak | HCLAB punya `Specimen Check-in` terpisah dari penilaian kelayakan | Dikuatkan |
| `LAB-DEC-019` — alasan penolakan terkendali | Keadaan specimen dicatat pada alur mikrobiologi | Dikuatkan sebagian |
| `LAB-DEC-021` — hasil punya bentuk selain angka | Mikrobiologi memakai Normal/Positif/Negatif; hasil ditandai low/high terhadap reference range | Dikuatkan, tetapi **belum cukup** — lihat `REC-CONF-003` |

---

## 4. Pertentangan yang Memerlukan Keputusan Pemilik

### `REC-CONF-001` — Cakupan disiplin: Mikrobiologi dan Patologi Anatomi

| Aspek | Isi |
|---|---|
| Keputusan yang bertentangan | `LAB-DEC-002` — cakupan **Patologi Klinik saja**; Mikrobiologi, Patologi Anatomi, dan Bank Darah dikeluarkan |
| Dampak | **`BLOCKING`** untuk batas scope seluruh modul |
| Tingkat | **Tertinggi** |

**Apa yang ditunjukkan bukti.** Ketiga artifact memperlihatkan Mikrobiologi dan Patologi
Anatomi bukan sebagai pelengkap, melainkan sebagai **bagian utama pekerjaan harian**:

| Bukti | Isi |
|---|---|
| Menu aplikasi Laboratorium baru | `Daftar Pasien Lab Patologi Klinik`, `Daftar Pasien Lab Patologi Anatomi`, `Daftar Pasien Lab Microbiologi` — **tiga daftar sejajar** |
| Menu hasil | `Hasil dan Riwayat` tersedia untuk **ketiga** kategori |
| HiSys lama | Submenu Isi Hasil terpisah untuk Mikrobiologi, Patologi Anatomi, dan Patologi Klinik |
| Sysmex HCLAB | Workstation mencakup mikrobiologi, histologi/sitologi, dan Bank Darah |
| `Laboratorium 1.md` | Menyatakan cakupan visual **paling kuat justru untuk Mikrobiologi dan Patologi Anatomi**; Patologi Klinik malah paling sedikit didemonstrasikan |

**Kenapa ini serius.** `LAB-DEC-002` diambil dengan alasan "alur sampel yang sudah dikodekan
cocok untuk pola hasil sekali jadi". Alasan itu masih benar secara teknis. Tetapi
konsekuensinya baru terlihat sekarang: Rilis 1 akan melayani **bagian terkecil** dari pekerjaan
laboratorium, sementara dua bagian yang paling banyak didemonstrasikan tetap dikerjakan di luar
sistem baru.

**Yang harus diputuskan:** apakah `LAB-DEC-002` dipertahankan, diperluas, atau diubah urutan
rilisnya.

---

### `REC-CONF-002` — Cito berada pada tingkat pemeriksaan, bukan pesanan

| Aspek | Isi |
|---|---|
| Keputusan yang bertentangan | `LAB-DEC-013` dan `BR-09` — `Urgency` melekat pada `LabOrder` |
| Dampak | **`BLOCKING`** untuk `EPIC-LAB-01` dan struktur `LabOrder` |

**Apa yang ditunjukkan bukti.**

| Bukti | Isi |
|---|---|
| `Laboratorium 1.md` CAP-005 | Pemilihan pemeriksaan menampilkan "harga, qty, subtotal, **Cito**, status cover" — Cito adalah kolom **per baris pemeriksaan** |
| `Laboratorium 1.md` CAP-012 | "Form hasil menampilkan kontrol **Cito dan Duplo** untuk pemeriksaan" |
| `Bagian Kedua` RULE-005 | "Hasil dapat ditandai Cito dan Duplo **pada tingkat pemeriksaan**" |

**Kenapa ini penting dan bukan sekadar soal letak kolom.** Cito per pemeriksaan berarti satu
pesanan dapat memuat Kalium cito bersama Kolesterol biasa. Dengan rancangan sekarang, seluruh
pesanan itu menjadi cito — sehingga Kolesterol rutin ikut menyita antrean prioritas, dan
sebaliknya laboratorium kehilangan kemampuan mendahulukan hanya yang memang mendesak.

Ada pula kemungkinan **cito berdampak tarif**, karena Cito muncul pada baris yang sama dengan
harga dan subtotal. Bila benar, itu menyentuh `LAB-INH-010`.

**Yang harus diputuskan:** apakah penanda cito dipindahkan ke `LabExamination`, dan apakah
cito berdampak pada tarif.

---

### `REC-CONF-003` — Bentuk hasil lebih dari dua

| Aspek | Isi |
|---|---|
| Keputusan yang bertentangan | `LAB-DEC-021` — hasil punya **dua** bentuk: angka dan pilihan terbatas |
| Dampak | **`BLOCKING`** untuk `LabValueBound` dan slice hasil |

**Apa yang ditunjukkan bukti.** Sekurang-kurangnya **empat** bentuk hasil dipakai:

| Bentuk | Bukti | Isi |
|---|---|---|
| Angka bersatuan | Semua artifact | Hasil, unit, reference range, penanda low/high |
| Pilihan terbatas | `Bagian Kedua` CAP-011 | Normal, Negatif, Positif |
| **Mikrobiologi berstruktur** | `Bagian Kedua` CAP-011, RULE-006 | Organisme per bakteri, antibiotik, `ug`, rentang R-S, zona per mm, hasil `R`/`I`/`S`. Setiap Bacteria Result adalah **task terpisah** yang dapat ditambah dan dikurangi |
| **Narasi Patologi Anatomi** | `Laboratorium 1.md` CAP-009, RULE-002, RULE-003 | Makroskopik, Mikroskopik, Kesimpulan — ketiganya wajib — ditambah **gambar** berukuran maksimum 2 MB |

Tabel batas nilai pada `LabValueBound` hanya memuat rentang angka dan daftar pilihan. Bentuk
ketiga dan keempat **tidak punya tempat**, dan keduanya tidak dapat dinilai kritis dengan
mekanisme batas mana pun.

**Yang harus diputuskan:** apakah kedua bentuk tambahan itu masuk Rilis 1, atau `LAB-DEC-002`
dipertahankan sehingga keduanya memang belum diperlukan. Kedua pertentangan ini saling terkait
dengan `REC-CONF-001`.

---

### `REC-CONF-004` — Laboratorium melakukan registrasi pasien sendiri

| Aspek | Isi |
|---|---|
| Keputusan yang bertentangan | Batas scope: "Pendaftaran pasien dan pembentukan kunjungan adalah milik `registration-management`, di luar scope" |
| Dampak | **`BLOCKING`** untuk batas ownership modul |

**Apa yang ditunjukkan bukti.**

| Bukti | Isi |
|---|---|
| `Laboratorium 1.md` CAP-004 | "Registrasi OTC/rujukan — mengelola pasien OTC dan data rujukan, instansi asal, dokter perujuk, surat rujukan, pembayaran, dan pemeriksaan" |
| `Bagian Ketiga` CAP-004 | "Registrasi pasien laboratorium/OTC — mencatat identitas, kontak, alamat, jenis pasien/penjamin, rujukan, dan tipe pemeriksaan **untuk pasien yang belum terdaftar**" |
| Menu | `Daftar Pasien OTC` berdiri sendiri pada aplikasi Laboratorium baru |

**Kenapa ini serius.** Blueprint mengasumsikan setiap pesanan lab menempel pada kunjungan yang
sudah dibuat Registrasi (`INV-01`). Bukti menunjukkan laboratorium menerima **pasien datang
langsung** dan **pasien rujukan dari luar** yang belum punya kunjungan sama sekali, lalu
mendaftarkannya sendiri.

Bila asumsi itu salah, `INV-01` dan seluruh rancangan yang bertumpu padanya perlu ditinjau.

**Yang harus diputuskan:** apakah pasien OTC dan rujukan luar dilayani lewat kunjungan yang
dibuat modul Registrasi, atau Laboratorium memang memiliki jalur pendaftarannya sendiri.

---

### `REC-CONF-005` — Laboratorium menampilkan perhitungan biaya

| Aspek | Isi |
|---|---|
| Keputusan yang bertentangan | `LAB-INH-010` dan `LAB-INH-012` — Laboratorium tidak punya wewenang finansial |
| Dampak | `NON_BLOCKING_STANDARD`, tetapi wajib ditegaskan |

**Apa yang ditunjukkan bukti.**

| Bukti | Isi |
|---|---|
| `Laboratorium 1.md` CAP-005 | "harga, qty, subtotal, Cito, status cover, dan **grand total**" |
| Menu | **`Tarif Laboratorium`** berdiri sendiri pada aplikasi Laboratorium baru |
| `Laboratorium 1.md` RULE-007 | Item dapat ditandai `Tidak Tercover` |
| `Bagian Kedua` | Status pembayaran ditampilkan pada daftar order |

**Penilaian.** Menampilkan harga dan total pada saat memesan **tidak sama dengan** memutuskan
tagihan. Ini kemungkinan besar tampilan bantu, dan tetap sesuai `LAB-INH-010`. Yang perlu
ditegaskan adalah menu `Tarif Laboratorium`: bila laboratorium **mengelola** tarifnya sendiri,
itu bertentangan dengan kepemilikan `master-data` pada blueprint.

**Yang harus diputuskan:** apakah tarif laboratorium dikelola Laboratorium atau Master Data.

---

## 5. Kemampuan Nyata yang Belum Tercakup Blueprint

Sebelas kemampuan berikut terlihat jelas pada bukti dan **tidak ada** pada blueprint mana pun.

| ID | Kemampuan | Bukti | Usulan penanganan |
|---|---|---|---|
| `REC-GAP-001` | Registrasi pasien OTC dan rujukan luar | Ketiga artifact | Lihat `REC-CONF-004` |
| `REC-GAP-002` | Penanda **Duplo** pada pemeriksaan | `Bagian Kedua` RULE-005; `Laboratorium 1.md` CAP-012 | Perlu keputusan: apa artinya dan apakah berdampak tarif |
| `REC-GAP-003` | Penanda **Definitif** pada hasil | `Laboratorium 1.md` CAP-008 | Perlu keputusan makna |
| `REC-GAP-004` | Nota Lab, Label Lab, Label Golongan Darah | `Bagian Kedua` CAP-009 | Kemampuan cetak; kandidat Rilis 2 |
| `REC-GAP-005` | Kirim hasil ke pasien, termasuk status terkirim | `Bagian Kedua` CAP-009; `Laboratorium 1.md` | Menyentuh privasi; perlu keputusan tersendiri |
| `REC-GAP-006` | Konfirmasi hasil lewat WhatsApp | `Bagian Kedua` CAP-008 | Menyentuh `LAB-COORD-001` dan privasi |
| `REC-GAP-007` | Laporan operasional laboratorium | `Bagian Ketiga` CAP-015 — pemeriksaan, statistik pesanan, dokter pengirim, pasien perusahaan, rekap kunjungan/biaya/penerimaan specimen, rujukan, rekonsiliasi, kelompok penyakit | Sebelumnya dinilai `NON_BLOCKING_STANDARD` pada `DEC-LAB-007`; bukti menunjukkan cakupannya jauh lebih besar |
| `REC-GAP-008` | Ekspor Excel dari daftar order dan nilai kritis | `Bagian Ketiga` CAP-002; `Bagian Kedua` CAP-014 | Kandidat Rilis 2 |
| `REC-GAP-009` | Order dari MCU | `Bagian Ketiga` CAP-003 | Sumber order tambahan; perlu keputusan scope |
| `REC-GAP-010` | Penautan hasil laboratorium **eksternal** berupa PDF ke rekam pasien | `Bagian Ketiga` CAP-014, BP-003 | Menyentuh `LAB-COORD-002` |
| `REC-GAP-011` | Quality Control dan Work Status | `Bagian Ketiga` — menu HCLAB | Belum pernah dibahas sama sekali |

---

## 6. Pertanyaan Blueprint yang Terjawab Bukti

| Pertanyaan | Jawaban dari bukti | Status |
|---|---|---|
| `DEC-LAB-005` — isi data awal batas nilai | Menu `Nilai Rujukan` sudah ada pada sistem berjalan, sehingga isinya dapat diambil dari sana | **Sumber ditemukan**, isinya tetap perlu pengesahan klinis |
| `DEC-LAB-006` — isi data awal alasan penolakan | Belum terlihat daftar alasan penolakan yang terkendali pada bukti mana pun | **Masih terbuka** |
| `LAB-COORD-001` — sarana pemberitahuan | Bukti menunjukkan **tiga** sarana dipakai: RS MMC App untuk nilai kritis, WhatsApp untuk konfirmasi DPJP, dan Phone Result untuk komunikasi hasil | **Berkembang** — bukan satu sarana, melainkan tiga |

---

## 7. Pemetaan Status: Blueprint dan Sistem Berjalan

Bukti memakai istilah status yang berbeda dari rancangan. Pemetaan ini **usulan**, bukan
keputusan.

| Status pada bukti | Sistem | Padanan pada blueprint | Keyakinan |
|---|---|---|---|
| Not Confirm / Belum Konfirmasi | HiSys | `Requested` | Sedang |
| Confirmed / Konfirmasi | HiSys | Tidak ada padanan langsung | **Rendah** |
| Menunggu | Aplikasi baru | `Requested` | Sedang |
| Dalam Proses / Sedang Pemeriksaan | Aplikasi baru | `InProcess` | Tinggi |
| Selesai | Aplikasi baru | `Completed` | Tinggi |
| Release | HCLAB | Status hasil, bukan status pesanan | Tinggi |
| Validated | HCLAB | Status hasil | Tinggi |
| Authorised | HCLAB | Status hasil | Tinggi |
| Belum Diperiksa | HiSys | `Planned` atau `Received` | **Rendah** |

**Yang paling perlu diperhatikan.** Status **`Confirmed`** tidak punya padanan pada rancangan.
Bukti menunjukkan konfirmasi adalah langkah tersendiri antara order masuk dan pekerjaan
dimulai — kemungkinan setara dengan penerimaan order oleh laboratorium, yang pada rancangan
sekarang justru diturunkan otomatis dari kelayakan wadah. Ini perlu ditelusuri lebih lanjut.

---

## 8. Dampak pada Artefak Blueprint

| Artefak | Dampak | Tindakan yang diperlukan |
|---|---|---|
| `00-interview-decisions.md` | 5 keputusan bertentangan | Amendment pass `grill-me` |
| `01-existing-capability-map.md` | Tidak terdampak | Audit ini menyangkut kode Quilvian V2, bukan sistem lain |
| `02-requirement-completeness-assessment.md` | **Basi** — basis buktinya berubah | Penilaian ulang setelah pertentangan ditutup |
| `03-domain-architecture.md` | Terdampak `REC-CONF-002` dan `REC-CONF-004` | Revisi setelah keputusan turun |
| `02-backend-architecture.md` | Terdampak `REC-CONF-002`, `REC-CONF-003` | Revisi setelah keputusan turun |
| `erd/*` | Terdampak — letak kolom cito, bentuk hasil | Revisi setelah keputusan turun |
| `contracts/*` | Terdampak `REC-CONF-002` | Revisi setelah keputusan turun |
| `04-prd-to-mvp.md` | Terdampak `REC-CONF-001` — batas MVP berubah bila cakupan berubah | Revisi setelah keputusan turun |
| `testing/acceptance-test-matrix.md` | Terdampak | Revisi mengikuti |

---

## 9. Verdict dan Langkah Berikutnya

**`BLUEPRINT_IMPACTED`.**

Blueprint yang disetujui **tetap sah** sebagai keputusan pemilik modul. Yang berubah adalah
tingkat keyakinan terhadap lima keputusan di dalamnya, setelah terlihat apa yang benar-benar
dikerjakan laboratorium.

| Yang **tetap aman** dikerjakan | Alasan |
|---|---|
| `EPIC-LAB-03` batas nilai | Dikuatkan bukti; menu `Nilai Rujukan` bahkan sudah ada di sistem berjalan |
| `EPIC-LAB-06` alasan penolakan | Tidak tersentuh satu pun pertentangan |
| `EPIC-LAB-02` pemisahan wadah dan pemeriksaan | Dikuatkan bukti |
| `EPIC-LAB-05` fakta kelayakan tagih | Tidak tersentuh |

| Yang **harus ditahan** | Pertentangan |
|---|---|
| `EPIC-LAB-01` penandaan cito | `REC-CONF-002` — letak penanda salah |
| `EPIC-LAB-04` daftar kerja | Bergantung pada cito |
| `EPIC-LAB-07` layar | Bergantung pada seluruh keputusan di atas |
| Batas MVP secara keseluruhan | `REC-CONF-001` — cakupan disiplin |

**Langkah berikutnya:** `grill-me` amendment pass untuk menutup `REC-CONF-001` sampai
`REC-CONF-005`. Kelimanya keputusan pemilik modul dan **tidak boleh** dijawab oleh dokumen ini.

---


---

## 10. Putaran 2 — Artifact "Module Artifact - Laboratorium", 2026-09-15

**Bukti baru.** Pemilik modul menyerahkan satu artifact konsolidasi tiga menu pemeriksaan —
Patologi Klinik, Patologi Anatomi, Mikrobiologi — yang menyatakan dirinya berbasis
`Laboratorium.md` ditambah **tiga putaran klarifikasi pemilik**: sepuluh jawaban awal,
kewajiban alasan pembatalan, dan pop-up alert konfirmasi akhir.

**Berkas baseline `Laboratorium.md` tidak ada di repository ini** dan tidak pernah tercatat pada
putaran 1 — yang diadopsi 2026-09-01 adalah `Analisis_Konsolidasi_Modul_Laboratorium.md` beserta
tiga artifact pengamatan. Artifact putaran 2 karena itu diperlakukan sebagai **bukti tingkat
pemilik** atas isi klarifikasinya, dan **bukti tingkat pengamatan** atas sisanya.

### 10.1 Yang menguatkan blueprint — nol pekerjaan baru

| Isi artifact | Menguatkan | Catatan |
|---|---|---|
| Tiga menu identik, berbeda hanya konteks disiplin | `LAB-DEC-025` | Tepat sama |
| Satu No. Order = satu baris, memuat beberapa `Nama Pemeriksaan` | `LAB-DEC-057`, `BE-LAB-26` | **Inilah `LabOrderedProcedure`.** Tabel yang dibangun 2026-09-15 menjawab persis kebutuhan ini |
| Satu order = satu hasil walaupun berisi beberapa pemeriksaan | `BR-23` + `LAB-DEC-055` | **Cocok justru karena `BE-LAB-27`:** pesanan kini dipecah per disiplin, sehingga satu pesanan selalu satu disiplin — dan satu disiplin selalu satu bentuk hasil |
| Order CITO ditandai pada barisnya | `LAB-DEC-026` | Penanda melekat pada pemeriksaan; tampilan baris adalah turunannya |
| Order terbaru di atas, filter sudah tersedia | `FE-LAB-*` | Kewenangan UI, tidak menyentuh backend |

### 10.2 Yang benar-benar baru — nol kemunculan di blueprint maupun source

| ID | Isi | Keadaan blueprint hari ini |
|---|---|---|
| `REC2-NEW-001` | **Status `Terkonfirmasi`** sebagai langkah tersendiri: konfirmasi sekali, merekam nama konfirmator dan tanggal/waktu, memilih dokter pemeriksa | `LabOrderStatus` = `Draft`, `Requested`, `Accepted`, `InProcess`, `Completed`, `Cancelled`, `OnHold`. **Tidak ada `Confirmed`.** `LAB-P0-002` justru masih membuka pertanyaan ini sejak 2026-09-01 |
| `REC2-NEW-002` | **Dokter pemeriksa** dipilih petugas dan tampil pada daftar serta ringkasan cetak | `LabOrder` **tidak punya** kolom dokter pemeriksa |
| `REC2-NEW-003` | **Alasan pembatalan pesanan wajib** diisi manual dan disimpan | Nol aturan pembatalan **pesanan** pada `LAB-VAL-v1`. `VAL-19` hanya mengatur pembatalan **pemeriksaan**. `LAB-P0-003` masih terbuka |
| `REC2-NEW-004` | **Pop-up alert konfirmasi akhir** sesudah `Lanjut Pembatalan`, sebelum status berubah | Nol kemunculan. Ini klarifikasi terbaru pemilik |
| `REC2-NEW-005` | **Status pembayaran pada layar lab** — `Belum ditagihkan`, `Belum Lunas`, `Lunas` — dan **Proses Pemeriksaan terkunci sampai `Lunas`** bagi pasien Mandiri/tunai | `LabOrder` nol kolom pembayaran. `RJ-BIL-GATE-DEC-003` menetapkan akibat finansial sepenuhnya milik Billing. `LAB-P0-007` masih terbuka |
| `REC2-NEW-006` | **Print membuka preview lebih dulu**, baru dicetak | Nol kemunculan pada kontrak maupun roadmap frontend |
| `REC2-NEW-007` | **Alert order baru 10 detik**, dapat ditutup manual | Kewenangan UI; belum tercatat sebagai kebutuhan |

### 10.3 Yang bertentangan — perlu keputusan pemilik, tidak dijawab dokumen ini

| ID | Pertentangan | Sisi artifact | Sisi blueprint dan source |
|---|---|---|---|
| `REC2-CONF-001` | **Letak penerimaan sampling** | Sebuah **checkbox `Sampling diterima`** di dalam pop-up Proses Pemeriksaan | Siklus hidup wadah penuh: `Planned` → `Collected` → `Received` → `Accepted`/`Rejected`, beserta pengambilan ulang, penolakan beralasan, dan penjagaan `VAL-68`/`VAL-69`. Dibangun `BE-LAB-21` sampai `BE-LAB-28` dan **sudah berjalan** |
| `REC2-CONF-002` | **Status `Accepted`** | Tidak muncul sebagai status pesanan | `Accepted` adalah status pesanan yang **diturunkan otomatis** saat wadah pertama dinyatakan layak — `state-transition-matrix` bagian 1 |
| `REC2-CONF-003` | **Pembayaran menahan pekerjaan klinis** | Proses Pemeriksaan **nonaktif** sampai `Lunas` bagi Mandiri/tunai | `RJ-BIL-GATE-DEC-003`: keputusan uang milik Billing; Laboratorium menerbitkan fakta kelayakan tagih **sesudah** wadah layak (`AC-37`), bukan sebaliknya |

**`REC2-CONF-001` dan `REC2-CONF-002` adalah satu perkara yang sama dilihat dari dua sisi.**
Artifact ini memotret layar yang berjalan hari ini, ketika penerimaan sampling memang hanya satu
centang. Blueprint memperluasnya menjadi siklus hidup wadah karena bukti lapangan putaran 1
menunjukkan penolakan dan pengambilan ulang benar-benar terjadi. Menerima artifact apa adanya
berarti membatalkan delapan task yang sudah selesai; menolaknya apa adanya berarti layar yang
dipakai petugas tidak pernah cocok dengan sistemnya.

**`REC2-CONF-003` berkonsekuensi paling jauh.** Menahan pemeriksaan sampai lunas adalah kebijakan
rumah sakit, bukan pilihan teknis — dan ia bertabrakan dengan gerbang billing yang sudah dikunci
modul `rawat-jalan`. Ia juga menyentuh `LAB-COORD-007` yang masih tertahan.

### 10.4 Yang ikut terjawab

| Pertanyaan terbuka | Dijawab artifact? |
|---|---|
| `LAB-P0-002` urutan status resmi termasuk `Confirmed` | **Sebagian** — artifact menyatakan `Terkonfirmasi` ada dan letaknya sesudah `Menunggu`. Belum menyatakan hubungannya dengan `Accepted` |
| `LAB-P0-003` aturan pembatalan dan koreksi | **Sebagian** — batas pembatalan (`Menunggu`/`Terkonfirmasi` saja) dan kewajiban alasan ditetapkan. Koreksi hasil tidak disinggung |
| `LAB-P0-007` aturan tagihan dan cakupan | **Tidak** — artifact justru menambah pertanyaan lewat `REC2-CONF-003` |

### 10.5 Verdict putaran 2

**`RECONCILED` — 2026-09-15.** Ketiga pertentangan dijawab pemilik modul pada hari yang sama.

| Pertentangan | Ditutup oleh | Arah keputusan |
|---|---|---|
| `REC2-CONF-001` letak penerimaan sampling | `LAB-DEC-060` | **Blueprint bertahan.** Centang `Sampling diterima` menjadi jalan pintas satu klik ke siklus hidup wadah, bukan penggantinya. `BE-LAB-21`..`BE-LAB-28` tetap berlaku |
| `REC2-CONF-002` keberadaan status `Accepted` | `LAB-DEC-060` | **Tetap ada**, tetap diturunkan otomatis saat wadah pertama dinyatakan layak. Ia tidak muncul di layar bukan berarti ia tidak ada |
| `REC2-CONF-003` pembayaran menahan pekerjaan klinis | `LAB-DEC-062` | **Diadopsi dengan batas tegas.** Layar mengunci tombol Proses bagi Mandiri/tunai, tetapi nilainya **dibaca dari Billing** — Laboratorium nol kolom pembayaran. `RJ-BIL-GATE-DEC-003` tidak dilanggar |

**Tujuh butir baru: lima diadopsi, dua menjadi kewenangan UI.**

| Butir | Keadaan sesudah keputusan |
|---|---|
| `REC2-NEW-001` status `Terkonfirmasi` | Diadopsi `LAB-DEC-061`. Menuntut amandemen `LAB-STATE-v1` dan satu migration |
| `REC2-NEW-002` dokter pemeriksa | Diadopsi `LAB-DEC-061`. Kolom baru pada `LabOrder` |
| `REC2-NEW-003` alasan pembatalan wajib | Diadopsi `LAB-DEC-063`. **Nol kolom baru** — sudah tersimpan pada jejak audit; yang dibutuhkan hanya mewajibkan ruas yang hari ini opsional |
| `REC2-NEW-004` alert konfirmasi akhir | **Kewenangan UI** (`LAB-DEC-063`). Backend menuntut alasannya ada, bukan berapa kali petugas ditanya |
| `REC2-NEW-005` status pembayaran mengunci Proses | Diadopsi `LAB-DEC-062`, **tertahan** `LAB-COORD-010` |
| `REC2-NEW-006` print preview | **Kewenangan UI.** Tidak menyentuh kontrak backend |
| `REC2-NEW-007` alert order baru 10 detik | **Kewenangan UI** |

**Biaya yang muncul bersamaan — ditulis supaya tidak ditemukan belakangan.**

| Hal | Akibat |
|---|---|
| `LAB-STATE-v1` | Naik revisi: `Confirmed` masuk sebagai status pesanan, beserta transisi sah dari `Requested` dan ke `Accepted` |
| `LAB-VAL-v1` | Naik revisi: aturan konfirmasi sekali, dan aturan pembatalan beralasan beserta batas statusnya |
| `LAB-API-v1` | Naik revisi: jalur konfirmasi dan jalur pembatalan beralasan |
| Migration | **Tiga kolom** pada `LabOrder`: konfirmator, waktu konfirmasi, dokter pemeriksa. Nol kolom pembayaran, dan **nol kolom alasan pembatalan** — alasannya sudah tersimpan sebagai `ReasonNote` pada `LabTransitionHistory` sejak jalur pembatalan dibangun |
| `LAB-COORD-010` | **Penahan baru.** Jalur baca status pembayaran milik Billing belum ada; sampai ada, penguncian tombol Proses tidak dapat ditegakkan backend |

**Langkah berikutnya:** amandemen kontrak — `LAB-STATE-v1`, `LAB-VAL-v1`, `LAB-API-v1` — lalu
`plan-module-delivery` menurunkannya menjadi task. `LAB-COORD-010` diajukan terpisah ke pemilik
Billing, mengikuti pola `LAB-REQ-005` dan `LAB-REQ-006`.

## 11. Putaran 3 — Artifact "Module Artifact - Laboratorium" (Menu Hasil), 2026-09-16

**Bukti baru.** Pemilik modul menyerahkan `Laboratorium (4).md`, disimpan verbatim sebagai
[`evidence/2026-09-16-menu-hasil-patologi-klinik-anatomi-mikrobiologi.md`](evidence/2026-09-16-menu-hasil-patologi-klinik-anatomi-mikrobiologi.md).
Artifact ini memotret
**satu menu**: *Menu Hasil Patologi Klinik, Patologi Anatomi dan Mikrobiologi*. Ia menyatakan
dirinya pembaruan `Laboratorium (3).md` ditambah **sepuluh jawaban klarifikasi pemilik**
bertanggal 2026-09-16, dan memuat `CAP-001`..`CAP-012`, `RULE-001`..`RULE-028`, serta
`BP-001`..`BP-008`.

**Baseline yang dirujuknya tidak ada di repository ini** — baik `Laboratorium (3).md` maupun
`01-Menu-Hasil-Patologi-Klinik-Patologi-Anatomi-dan-Mikrobiologi.txt` baris 1-48 yang dikutip
sebagai provenance. Mengikuti perlakuan putaran 2, artifact ini adalah **bukti tingkat pemilik**
atas isi kesepuluh klarifikasinya, dan **bukti tingkat pengamatan** atas sisanya. Rujukan
`Sumber baris ...` di dalamnya **tidak dapat diverifikasi** dari sini dan tidak diperlakukan
sebagai bukti terpisah.

**Satu kalimat menentukan cara membaca seluruh bagian ini.** Menu yang dipotret artifact ini
adalah slice **`S17` — "Label, nota, dan pengiriman hasil ke pasien"**, yang berstatus
`BUSINESS_DECISION_REQUIRED` sejak 2026-09-02 dengan alasan tertulis *"privasi pengiriman belum
diputuskan"* (`02-requirement-completeness-assessment.md` bagian 0.2), dan yang seluruh isinya
berdiri di atas **hasil pemeriksaan yang belum ada**: `LabExamination` hari ini nol kolom nilai
hasil, nol waktu pemeriksaan, nol status verifikasi. Yang membentuknya adalah slice `S4`, dan
`S4` tertahan `LAB-SIGN-001` sejak 2026-09-01.

Artinya artifact ini **menjawab sebagian besar pertanyaan bisnis `S17`** — dan tetap **nol
barisnya dapat dibangun** sebelum `S4` terbuka. Kedua hal itu benar bersamaan, dan keduanya
ditulis di sini supaya tidak ada yang menyimpulkan salah satunya saja.

### 11.1 Yang menguatkan blueprint dan source — nol pekerjaan baru

| Isi artifact | Menguatkan | Bukti pada source |
|---|---|---|
| `RULE-024` data terbaru paling atas | `FE-LAB-09` | `LabMonitoringService.cs:60` sudah `OrderByDescending(x => x.RequestedAt ?? x.CreateDateTime)`. **Sudah terpenuhi** |
| `RULE-023` pagination kelipatan 5 | `FE-LAB-09` | `lab-monitoring-constants.jsx` `pageSizeOptions` = `[10, 25, 50, 100]` — **keempatnya kelipatan 5**. Sudah terpenuhi tanpa perubahan |
| `RULE-006` animasi loading, `RULE-025` alert kosong, `RULE-026` alasan error | Pola frontend yang sudah berjalan | `lab-monitoring-view.jsx` sudah memakai `InformationAlert variant="danger"` untuk galat dan `emptyTitle`/`emptyDescription` untuk kosong. Pola tersedia, tinggal dipakai ulang |
| `RULE-016` nomor tujuan dari master pasien | — | **`MstPatient.WhatsAppNumber` sudah ada** (`MstPatient.cs:60`), terpisah dari `PhoneNumber`. Nol kolom baru untuk menyimpan nomor tujuan |
| `RULE-021` satu nomor order = satu dokumen hasil final | `BR-23`, `LAB-DEC-055`, `BE-LAB-27` | Cocok **justru karena** pesanan dipecah per disiplin: satu pesanan selalu satu disiplin, dan satu disiplin selalu satu bentuk hasil. Sudah dicatat putaran 2 |
| `RULE-020` seluruh pemeriksaan wajib berhasil sebelum final | `LAB-DEC-057`, `BE-LAB-26` | `LabOrderedProcedure` adalah daftar *apa yang diminta*; kelengkapan hasil diukur terhadapnya |
| Jenis Kunjungan Rawat Jalan / Rawat Inap / IGD | — | `LabMonitoringQuery.EncounterType` sudah ada dan sudah dipakai penyaring frontend |
| Alur normal `Menunggu -> Terkonfirmasi -> Diproses -> Selesai` | `LAB-DEC-061`, `LAB-STATE-v1` `r3` | **Menguatkan putaran 2.** `Confirmed` sudah masuk sebagai status pesanan 2026-09-15 dan sudah terpasang pada `LabOrder.ConfirmedAt`/`ConfirmedByUserId` |
| `Dibatalkan` hanya lahir dari aksi pembatalan user | `LAB-DEC-063` | Sejalan. Artifact **diam** soal batas status sumber; `LAB-STATE-v1` `r3` mempersempitnya ke `Requested`/`Confirmed`. **Diam bukan pertentangan** — aturan blueprint yang lebih sempit tetap berlaku |
| Status pembayaran `Belum ditagihkan` / `Belum Lunas` / `Lunas` | `LAB-DEC-062`, `REC2-NEW-005` | Pengulangan putaran 2. Tetap tertahan `LAB-COORD-010`; Laboratorium tetap nol kolom pembayaran |

### 11.2 Yang benar-benar baru — nol kemunculan di blueprint maupun source

| ID | Isi artifact | Keadaan hari ini |
|---|---|---|
| `REC3-NEW-001` | **Kolom `No. Order`** pada datatable, dan **barcode Label Lab dibangkitkan dari nomor order** (bagian 5.3 dan 5.5) | **`LabOrder` nol kolom nomor order.** Ditelusuri: `OrderNumber` nol kemunculan di seluruh `Areas/HealthServices/LaboratoryManagement/`. Yang ada barcode **wadah** (`LabSpecimen.SpecimenBarcode`), bukan nomor pesanan. Menuntut kolom baru, pembangkit nomor, keunikan, dan satu migration |
| `REC3-NEW-002` | **Kategori Periode**: Tanggal Sampling, Tanggal Order, Tanggal Pemeriksaan — default Tanggal Order | `LabMonitoringService.cs:170-180` menyaring **hanya** pada `RequestedAt ?? CreateDateTime`. Tanggal sampling **bisa** diturunkan dari `LabSpecimen.CollectedAt`; **tanggal pemeriksaan tidak ada sama sekali** — `LabExamination` nol kolom waktu pemeriksaan. Satu dari tiga pilihan belum punya sumber data |
| `REC3-NEW-003` | Penyaring **NIK**/No. RM | `MstPatient.IdentityNumber` ada (`MstPatient.cs:54`), tetapi pencarian Laboratorium hari ini hanya menjangkau `EncounterNumber`, `FullName`, dan `MedicalRecordNumber` (`LabMonitoringService.cs:232-240`). **NIK nol dijangkau** |
| `REC3-NEW-004` | **Counter `Terkirim ke Pasien`** — integer, `+1` per pengiriman berhasil | Nol tabel, nol kolom. Counter telanjang menyimpan **angka tanpa riwayat**: tidak terjawab siapa mengirim, kapan, ke nomor mana, dan mana yang gagal. Bentuk yang memenuhi `RULE-014`/`RULE-015` **dan** kebijakan audit adalah **log pengiriman**, dengan counter sebagai turunannya. Bentuk finalnya keputusan perancangan, bukan keputusan artifact |
| `REC3-NEW-005` | **Pengiriman hasil lewat WhatsApp** | `F7` (2026-09-01) diverifikasi **masih benar pada SHA berjalan**: penelusuran ulang `Twilio`, `Fonnte`, `SendMessageAsync`, `IWhatsAppService`, `SmtpClient`, dan `MailKit` di seluruh backend menghasilkan **nol**. `WhatsAppNumber` yang ada hanyalah kolom data induk, bukan pengirim. **Gerbang pengirimnya milik platform, bukan Laboratorium** |
| `REC3-NEW-006` | **Berkas PDF/final yang tidak dapat diedit pasien** (`RULE-018`) | **Nol pustaka PDF** pada `NewQuilvianSystemBackend.csproj`. Pencetakan yang berjalan hari ini adalah **cetak peramban** dari frontend (`lab-order-print-document.jsx`) — tampilan cetak, bukan berkas yang dapat dilampirkan ke pesan. `QRCoder 1.8.0` **sudah ada** dan menutup kebutuhan barcode, tetapi tidak menutup kebutuhan PDF |
| `REC3-NEW-007` | **Persetujuan Profesor dan Dokter Lab** sebagai syarat kirim (`RULE-017`) | Nol entity persetujuan hasil. `LabCriticalBoundApproval` yang ada adalah persetujuan **batas nilai**, bukan persetujuan hasil. Kedua peran ini pun **nol kemunculan** pada `LAB-PERM-v1` |
| `REC3-NEW-008` | **Kolom Dokter Konfirmator angka kritis**, berjabatan `Dokter DPJP` atau `Dokter Lantai` | Angka kritis adalah slice `S5`, tertahan `LAB-P0-004` dan `LAB-OPEN-014`. **`Dokter Lantai` nol kemunculan** di seluruh blueprint — konsep peran baru |
| `REC3-NEW-009` | **Nota Lab, Label Lab, Label Goldar** beserta preview sebelum cetak | Sudah masuk scope modul lewat `LAB-DEC-030`, ditempatkan **Rilis 2**. Nol kode. `REC2-NEW-006` sudah mencatat preview sebagai kewenangan UI |
| `REC3-NEW-010` | **Ikon gender berwarna** pada kolom Pasien — biru/navy dan merah muda (bagian 5.3) | Kewenangan UI. Dicatat supaya tidak hilang; tidak menyentuh kontrak backend |

### 11.3 Yang bertentangan — perlu keputusan pemilik, tidak dijawab artifact

> **Keadaan per 2026-09-16 sore.** `REC3-CONF-001`, `REC3-CONF-002`, dan `REC3-CONF-004`
> **ditutup pemilik modul pada hari yang sama** — lihat bagian 11.6. Keduanya yang tersisa,
> `REC3-CONF-003` dan `REC3-CONF-005`, **tidak dapat ditutup pemilik modul sendirian**: yang
> satu menuntut wewenang klinis, yang lain menuntut profil printer nyata. Tabel di bawah
> dipertahankan apa adanya sebagai catatan pertentangannya, bukan sebagai daftar tugas.

| ID | Pertentangan | Sisi artifact | Sisi blueprint dan source |
|---|---|---|---|
| `REC3-CONF-001` | **Satu datatable gabungan tiga disiplin** | Bagian 1 dan `CAP-003`: hasil Patologi Klinik, Patologi Anatomi, dan Mikrobiologi **dalam satu datatable** | `LAB-DEC-025` menetapkan **tiga daftar sejajar sebagai tiga menu**, dan `LabMonitoringQuery` **sengaja nol ruas disiplin** — alasannya ditulis panjang pada DTO-nya dan pada `lab-monitoring-constants.jsx`. Keduanya dapat benar bersamaan bila menu Hasil adalah **menu keempat** yang memang lintas disiplin, tetapi itu **penetapan yang belum pernah diambil** |
| `REC3-CONF-002` | **`Tgl Awal < Tgl Akhir` tegas** | `RULE-004`: nilai Tgl Awal **wajib lebih kecil** daripada Tgl Akhir | Aturan ini **menutup pencarian satu hari** — petugas tidak dapat mencari pesanan hari ini saja, padahal `RULE-003` justru mengizinkan tanggal hari ini dipilih pada kedua ruas. Penyaring yang berjalan hari ini inklusif (`>= mulai`, `<= sampai`). Kemungkinan besar yang dimaksud `<=`, tetapi menebak arah aturan validasi bukan wewenang dokumen ini |
| `REC3-CONF-003` | **Persetujuan Profesor dan Dokter Lab** | `RULE-017`: hasil hanya boleh dikirim sesudah disetujui keduanya | Inilah **persis** perkara `LAB-SIGN-001` yang menahan `S4`, `S4b`, `S4c`, `S5`, dan `S6` sejak 2026-09-01, dan yang permintaannya diajukan `LAB-REQ-004` pada 2026-09-09 dan **belum dijawab**. Artifact menyebut dua peran penyetuju, tetapi **tidak menetapkan** pemegang wewenang Clinical Governance, tidak menyatakan apakah persetujuan ini sama dengan tanda tangan klinis `LAB-DEC-011`, dan tidak menyebut keduanya sebagai peran pada `LAB-PERM-v1`. **Menganggapnya menutup `LAB-SIGN-001` adalah lompatan yang tidak dibuat artifact ini** |
| `REC3-CONF-004` | **Unit Layanan dapat bernilai `Radiologi`** | `RULE-008`: pendaftaran radiologi dari Kiosk menampilkan Unit Layanan `Radiologi` | Ini **menu hasil Laboratorium**. Baris berunit layanan Radiologi pada daftar hasil laboratorium berarti salah satu dari dua hal: aturan itu terbawa dari tabel generik yang dipakai bersama, atau menu ini memang memuat pesanan di luar Laboratorium. Keduanya berkonsekuensi berbeda dan **tidak dinyatakan** |
| `REC3-CONF-005` | **Ukuran cetak** A4 / 100x50 mm / 50x25 mm | `RULE-028` dan bagian 5.5 | **Artifact menyatakan sendiri** bahwa ini `Confidence: Medium` dan *"default umum implementasi, bukan ukuran klinis/regulasi yang ditetapkan oleh evidence"*. Karena itu **tidak diadopsi sebagai keputusan pemilik**; ia dicatat sebagai usulan yang menunggu profil printer nyata. Menaikkannya menjadi aturan berarti mengubah tingkat bukti diam-diam |

### 11.4 Yang ikut terjawab

| Pertanyaan terbuka | Dijawab artifact? |
|---|---|
| Gerbang `S17` *"privasi pengiriman belum diputuskan"* | **Sebagian besar ya.** Artifact menetapkan siapa penerimanya, dari mana nomornya, bentuk berkasnya, syarat persetujuan sebelum kirim, dan siapa yang boleh menekan tombolnya. Yang **belum**: persetujuan tertulis atas pengiriman data klinis ke kanal pihak ketiga, dan kepemilikan gerbangnya |
| `LAB-SIGN-001` tanda tangan klinis | **Tidak.** Lihat `REC3-CONF-003`. Artifact menyebut dua peran penyetuju tanpa menetapkan wewenang klinisnya |
| `LAB-P0-001` matriks kewenangan per peran | **Sebagian** — hanya untuk tombol aksi menu ini: Petugas Lab dan/atau Admin (`RULE-022`). Peran lain tidak disinggung |
| `LAB-P0-002` urutan status resmi | **Menguatkan**, tidak menambah. `Terkonfirmasi` sudah ditetapkan `LAB-DEC-061`; penyelarasan lintas tiga aplikasi tetap terbuka |
| `LAB-P0-004` alur nilai kritis | **Tidak.** Artifact justru menambah satu jabatan baru yang belum dikenal, `Dokter Lantai` |
| `LAB-P0-007` aturan tagihan | **Tidak.** Artifact mengulang tampilan tiga status pembayaran; jalur bacanya tetap `LAB-COORD-010` |

### 11.5 Biaya yang muncul bersamaan — ditulis supaya tidak ditemukan belakangan

| Hal | Akibat |
|---|---|
| Urutan pekerjaan | Menu ini **tidak dapat dijadwalkan** sebelum `S4` terbuka. Hasil belum ada, jadi daftar hasil, dokumen final, dan pengirimannya pun belum ada isinya. `LAB-SIGN-001` tetap penahan pertamanya |
| Migration | **Sekurang-kurangnya satu kolom nomor order pada `LabOrder`** beserta pembangkit dan keunikannya (`REC3-NEW-001`), ditambah tempat penyimpanan riwayat pengiriman (`REC3-NEW-004`). Keduanya **belum** dirancang dan tidak boleh diturunkan menjadi task sebelum dirancang |
| `LAB-API-v1` | Akan naik revisi ketika slice dibuka: ruas nomor order, ruas kategori periode pada penyaring, penyaring NIK, ruas counter pengiriman, serta jalur kirim dan kirim ulang. **Nol di antaranya boleh dibangun hari ini** — kontrak terkunci penuh sejak 2026-09-02 |
| `LAB-PERM-v1` | Dua peran penyetuju (`Profesor`, `Dokter Lab`) dan satu jabatan konfirmator (`Dokter Lantai`) belum punya padanan resource/permission |
| `LAB-INT-v1` | Kanal WhatsApp adalah **integrasi keluar pertama** modul ini ke kanal pihak ketiga. Belum terkontrak |
| **`LAB-COORD-011`** | **Penahan baru.** Gerbang pengiriman pesan (WhatsApp) **dan** pembangkit berkas PDF keduanya nol pada platform. Laboratorium tidak berwenang mengadakannya sendiri; keduanya keputusan platform |
| Privasi | Mengirim hasil laboratorium ke kanal pihak ketiga adalah pengiriman data klinis ke luar sistem. `LAB-DEC-030` sudah menandainya *"menyentuh privasi, perlu keputusan tersendiri"* sejak 2026-09-01, dan artifact ini **belum** memberi keputusan itu — ia mengatur mekanismenya, bukan izinnya |

### 11.6 Verdict putaran 3

**`PARTIALLY_RECONCILED` — 2026-09-16.**

Sepuluh butir diadopsi sebagai keputusan (`LAB-DEC-064` sampai `LAB-DEC-073`). Tujuh hal dibuka,
dan **empat ditutup pemilik modul pada hari yang sama**. Tiga sisanya tetap terbuka bukan karena
terlewat, melainkan karena **bukan wewenang pemilik modul sendiri**. Satu penahan baru dibuka
(`LAB-COORD-011`), dan `LAB-SIGN-001` **tetap** menjadi penahan pertama seluruh menu ini.

**Empat yang ditutup pemilik modul 2026-09-16:**

| Hal | Ditutup oleh | Arah keputusan |
|---|---|---|
| `REC3-CONF-001` satu datatable gabungan | `LAB-DEC-070` | **Blueprint bertahan.** Menu Hasil mengikuti pola tiga daftar sejajar per disiplin; disiplin ditentukan jalur yang dipanggil. **Artifact tidak diadopsi apa adanya pada butir ini** — alasan `LAB-DEC-025` masih berlaku. `LabMonitoringQuery` dipakai ulang, nol ruas disiplin ditambahkan |
| `REC3-CONF-002` arah pembanding tanggal | `LAB-DEC-071` | **`<=`, inklusif.** Pencarian satu hari tetap mungkin. **Mengoreksi `RULE-004` artifact**, dan **nol mengubah perilaku source** — penyaring yang berjalan sudah inklusif pada kedua ujung |
| `REC3-CONF-004` Unit Layanan `Radiologi` | `LAB-DEC-073` | **Hanya pesanan Laboratorium.** Nilai `Radiologi` terbawa dari tabel unit layanan bersama dan tidak akan pernah muncul sebagai baris. Nol koordinasi dengan Radiologi yang perlu dibuka |
| `REC3-NEW-001` nomor order | `LAB-DEC-072` | **Kolom baru berisi nomor urut terbaca**, pola `PatientEncounterNumberService` (`ENC-RSMMC-00001`, dijaga `pg_advisory_xact_lock`). Pola `LSP-{Guid:N}` milik barcode wadah **sengaja ditolak**: nomor yang dicetak di amplop pasien harus dapat disebut lewat telepon |

> **Satu peringatan dibawa serta oleh `LAB-DEC-072`, dan lebih baik ditulis sekarang.**
> `AllocateEncounterNumberAsync` — pola yang dijadikan acuan — memuat **seluruh** nomor terpakai
> ke memori lalu memindai celah pertama. Biayanya tumbuh seiring jumlah baris, dan pesanan
> laboratorium bertambah jauh lebih cepat daripada kunjungan. **Meniru polanya tanpa meniru
> kelemahan itu** adalah bagian dari perancangan, bukan detail implementasi yang boleh
> ditemukan sendiri saat menulis kode.

**Tiga yang tetap terbuka — dan kenapa bukan pemilik modul yang menutupnya:**

| Hal | Kenapa tertahan | Diajukan lewat |
|---|---|---|
| `REC3-CONF-003` / `LAB-OPEN-029` persetujuan Profesor dan Dokter Lab | Menuntut **wewenang Clinical Governance**, yang blueprint sendiri catat `belum ditetapkan`. Ini perkara `LAB-SIGN-001` yang permintaannya diajukan `LAB-REQ-004` pada 2026-09-09 dan belum dijawab | `LAB-REQ-007` |
| `LAB-OPEN-030` jabatan `Dokter Lantai` | Bertaut `LAB-P0-004` alur nilai kritis dan `LAB-OPEN-014`, keduanya terbuka sejak 2026-09-01 | `LAB-REQ-007` |
| `REC3-CONF-005` / `LAB-OPEN-032` ukuran cetak | Menuntut **profil printer dan media fisik yang nyata**, bukan keputusan di atas kertas. Artifact sendiri menandainya `Confidence: Medium` | `LAB-REQ-007` |
| `LAB-COORD-011` gerbang WhatsApp dan pembangkit PDF | **Milik platform**, bukan Laboratorium | `LAB-REQ-007` |

| Butir | Keadaan sesudah putaran ini |
|---|---|
| Aturan rentang tanggal dan loading | Diadopsi `LAB-DEC-064`, **kecuali** arah `<` versus `<=` yang dibuka sebagai `LAB-OPEN-028` |
| Keyword Search mengikuti BE | Diadopsi `LAB-DEC-065` |
| Counter `Terkirim ke Pasien` | Diadopsi `LAB-DEC-066`; **bentuk penyimpanannya** belum dirancang |
| Syarat dan bentuk pengiriman hasil | Diadopsi `LAB-DEC-067`, **tertahan** `LAB-COORD-011`, `LAB-SIGN-001`, dan izin privasi |
| Kewenangan tombol aksi | Diadopsi `LAB-DEC-068` |
| Perilaku datatable | Diadopsi `LAB-DEC-069`; dua dari empat aturannya **sudah terpenuhi** source hari ini |
| Satu datatable gabungan | **Ditutup** `LAB-DEC-070` — blueprint bertahan, tiga daftar sejajar |
| Arah pembanding tanggal | **Ditutup** `LAB-DEC-071` — `<=`, inklusif |
| Nomor order | **Ditutup** `LAB-DEC-072` — kolom baru, nomor urut terbaca. **Pekerjaannya tetap ada** |
| Unit Layanan `Radiologi` | **Ditutup** `LAB-DEC-073` — hanya pesanan Laboratorium |
| Persetujuan Profesor dan Dokter Lab | `LAB-OPEN-029`, terbuka — bertaut `LAB-SIGN-001`, diajukan `LAB-REQ-007` |
| `Dokter Lantai` dan angka kritis | `LAB-OPEN-030`, terbuka — diajukan `LAB-REQ-007` |
| Ukuran cetak | `LAB-OPEN-032`, terbuka — **tidak** diadopsi sebagai keputusan, diajukan `LAB-REQ-007` |

**Langkah berikutnya, berurutan.** Pertama `LAB-SIGN-001` — tanpanya tidak ada yang bergerak.
Lalu ketiga sisa bagian 11.3 dan `LAB-COORD-011` dijawab lewat `LAB-REQ-007`. Baru sesudah itu
`requirement-completeness-gate` menilai ulang `S17`, `hospital-domain-architect` merancang
nomor order dan log pengiriman, kontrak diamandemen, dan `plan-module-delivery` menurunkannya
menjadi task. **Menulis kode hari ini akan mendahului keenam langkah itu.**

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 3 | 2026-09-16 | **Putaran 3 — artifact "Module Artifact - Laboratorium" (Menu Hasil).** Artifact memotret satu menu yang seluruhnya adalah slice `S17`, dan `S17` berdiri di atas hasil pemeriksaan yang belum ada — `LabExamination` nol kolom hasil, nol waktu pemeriksaan, nol status verifikasi — sehingga `LAB-SIGN-001` tetap penahan pertamanya. **Sepuluh butir benar-benar baru**, dipimpin `No. Order` yang ternyata **nol ada** pada `LabOrder` padahal dipakai sebagai kolom daftar sekaligus sumber barcode Label Lab, disusul kategori periode yang salah satu pilihannya belum punya sumber data, counter pengiriman tanpa riwayat, gerbang WhatsApp yang `F7` buktikan masih nol, dan pembangkit PDF yang juga nol. **Sepuluh butir lain menguatkan tanpa pekerjaan baru** — termasuk tiga yang **sudah terpenuhi source hari ini**: urutan terbaru di atas, pagination kelipatan 5, dan `MstPatient.WhatsAppNumber` sebagai sumber nomor tujuan. **Lima pertentangan dibuka dan tidak dijawab artifact:** satu datatable gabungan versus `LAB-DEC-025`, `Tgl Awal < Tgl Akhir` yang menutup pencarian satu hari, persetujuan Profesor/Dokter Lab yang justru perkara `LAB-SIGN-001`, Unit Layanan `Radiologi` pada menu hasil Laboratorium, dan ukuran cetak yang artifact sendiri tandai `Medium`. Enam klarifikasi diadopsi `LAB-DEC-064`..`LAB-DEC-069`; satu penahan baru `LAB-COORD-011` | `draft` |
| 4 | 2026-09-16 | **Empat dari tujuh hal yang dibuka putaran 3 ditutup pemilik modul pada hari yang sama.** `LAB-DEC-070` mempertahankan pola tiga daftar sejajar dan **menolak satu datatable gabungan** — artifact tidak diadopsi apa adanya pada butir ini. `LAB-DEC-071` menjadikan pembanding tanggal inklusif dan **mengoreksi `RULE-004` artifact**, yang bila dibaca tegas justru menutup pencarian satu hari. `LAB-DEC-072` memberi `LabOrder` kolom nomor order berupa nomor urut terbaca mengikuti `PatientEncounterNumberService`, **beserta peringatan** bahwa pola acuannya memuat seluruh nomor terpakai ke memori. `LAB-DEC-073` mengunci cakupan baris pada pesanan Laboratorium saja. **Tiga sisanya tetap terbuka bukan karena terlewat**, melainkan karena bukan wewenang pemilik modul sendiri: persetujuan klinis (`LAB-SIGN-001`), jabatan `Dokter Lantai` (`LAB-P0-004`), dan ukuran cetak yang menunggu profil printer nyata. Ketiganya beserta `LAB-COORD-011` diajukan lewat `LAB-REQ-007` | `draft` |
| 1 | 2026-09-01 | Rekonsiliasi pertama terhadap tiga artifact bukti lapangan. 8 keputusan dikuatkan, 5 bertentangan, 11 kemampuan belum tercakup, 3 pertanyaan terjawab sebagian | `draft` |
| 2 | 2026-09-15 | **Putaran 2 — artifact "Module Artifact - Laboratorium".** Tujuh butir benar-benar baru dicatat, dipimpin status `Terkonfirmasi` yang selama ini justru menjadi pertanyaan terbuka `LAB-P0-002`, alasan pembatalan wajib beserta pop-up alert konfirmasi akhir, dan status pembayaran yang menahan Proses Pemeriksaan. Lima butir lain **menguatkan** blueprint tanpa pekerjaan baru — terutama satu order berisi beberapa nama pemeriksaan, yang persis dijawab `LabOrderedProcedure` milik `BE-LAB-26`, dan satu order satu hasil yang justru cocok **karena** `BE-LAB-27` memecah pesanan per disiplin. **Tiga pertentangan dibuka dan tidak dijawab dokumen ini:** letak penerimaan sampling (satu centang vs siklus hidup wadah yang sudah dibangun delapan task), keberadaan status `Accepted`, dan pembayaran yang menahan pekerjaan klinis — yang bertabrakan dengan `RJ-BIL-GATE-DEC-003` milik `rawat-jalan`. **Ketiganya ditutup pada hari yang sama** oleh `LAB-DEC-060` sampai `LAB-DEC-063`: siklus hidup wadah bertahan dan centang menjadi jalan pintasnya, status `Confirmed` diadopsi beserta dokter pemeriksa, alasan pembatalan wajib disimpan backend, dan pembayaran mengunci tombol Proses tetapi nilainya dibaca dari Billing sehingga `RJ-BIL-GATE-DEC-003` tidak dilanggar. Verdict putaran 2 menjadi `RECONCILED`, dengan satu penahan baru `LAB-COORD-010` | `draft` |
