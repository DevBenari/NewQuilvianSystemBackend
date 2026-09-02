# Bank Darah — Hospital Domain Architecture

## A. Identitas arsitektur

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Blueprint revision | `5` |
| Domain architecture revision | `2` |
| Modul | Bank Darah (`bank-darah`) |
| Tanggal | `2026-09-02` |
| Backend SHA | `9dc7637adbafb321ad8078d5c52ebe5e4398fe86` cabang `sukmagp` |
| Backend SHA saat bukti kemampuan diaudit | `9522caacf29371b1fddd1584e9a71ad94fe48d19`. Perbedaannya hanya dokumen blueprint, nol berkas source aplikasi, sehingga `BD-CAP-001` sampai `BD-CAP-024` tetap sahih |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Kesiapan requirement | `PARTIALLY_READY` — `02-requirement-completeness-assessment.md` revisi 2 |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_PARTIAL`** |
| Baseline rujukan Indonesia | Tidak dipakai. Tidak ada `baseline_observation_ids` maupun `baseline_source_ids`. |
| Pass ini | Revisi 2 — pass ulang untuk menyerap `DEC-BD-025` sampai `DEC-BD-030` hasil architecture gap closure pass |

### Scope yang dinilai

Diterima ke sesi arsitektur ini:

| Slice | Kesiapan requirement | Keterangan gerbang masuk |
| --- | --- | --- |
| `BD-SLICE-01` Order darah pasien | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-02` Permintaan darah ke PMI | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-03` Penerimaan fisik kantong | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-04` Alokasi kantong | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-05` Pemberian darah | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-06` Kedaluwarsa dan kantong menunggu keputusan | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-07` Penyelesaian kantong | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |
| `BD-SLICE-08` Tindakan Bank Darah | `PARTIALLY_READY` | Diterima **hanya bagian pencatatan tindakannya**, yang dinyatakan berdiri sendiri pada penilaian kelengkapan. Bagian penyerahan biaya ditolak masuk |
| `BD-SLICE-09` Golongan darah | `PARTIALLY_READY` | Diterima **hanya bagian pemeriksaan dan validasi golongan darahnya**. Bagian label ditolak masuk |
| `BD-SLICE-10` Sampling, batas Laboratorium, HCLAB, laporan, setup | `READY_FOR_DOMAIN_DESIGN` | Diterima penuh |

Ditolak masuk sesi ini: kontrak penyerahan biaya ke Billing (terhalang `DEC-BD-016`) dan mekanik
label golongan darah (terhalang `OQ-BD-011`). Keduanya sudah ditolak sejak revisi 1 dan tetap di luar
scope yang dinilai, bukan temuan baru.

**Catatan gerbang masuk untuk revisi 2.** `02-requirement-completeness-assessment.md` masih revisi 2
dan belum menyerap `DEC-BD-025` sampai `DEC-BD-030`. Itu tidak menghalangi pass ini, karena keenam
keputusan baru hanya **menutup** blocker dan tidak satu pun menambah blocker baru pada kesiapan
requirement. Kesiapan per slice yang dipakai tetap yang tercatat di sana. Bila penilaian kelengkapan
kelak diperbarui, statusnya hanya dapat naik, tidak turun.

### Bukti dan Decision ID yang dipertahankan

`00-interview-decisions.md` revisi 3 — `SCOPE-BD-001`, `DEC-BD-001` sampai `DEC-BD-030`,
`INV-BD-011` sampai `INV-BD-021`, `ASM-BD-001` sampai `ASM-BD-007`, `DEF-BD-003`, `DEF-BD-004` ·
`00-business-overview.md` revisi 1 · `02-existing-capability-map.md` revisi 2 — `BD-CAP-001` sampai
`BD-CAP-024` · `02-requirement-completeness-assessment.md` revisi 2 · `01-prerequisite-readiness.md`
revisi 3 — `BD-DEP-001` sampai `BD-DEP-015`.

Keputusan baru yang diserap pass ini: `DEC-BD-025` kelebihan kiriman PMI · `DEC-BD-026` hasil
golongan darah yang berlaku · `DEC-BD-027` masa berlaku bukti kecocokan · `DEC-BD-028` gugurnya
bukti saat pengalihan · `DEC-BD-029` pembatalan alokasi · `DEC-BD-030` koreksi pencatatan
pemberian. Turunannya `INV-BD-017` sampai `INV-BD-021` dan `AC-BD-031` sampai `AC-BD-050`.

Decision ID pemblokir yang dibawa: `DEC-BD-016`, `OQ-BD-011`, `DEF-BD-003`, `DEF-BD-004`,
`OQ-BD-010`, ditambah tiga pertanyaan yang lahir dari closure pass: `OQ-BD-012` nilai jam masa
berlaku · `OQ-BD-013` tempat penyelesaian perbedaan hasil · `OQ-BD-014` keadaan kantong setelah
koreksi.

**Dokumen ini tidak membuat source code, migration, endpoint API, komponen UI, maupun task
implementasi.** Karena tidak ada endpoint yang dirancang di tahap ini, bagian bergaya Swagger sengaja
tidak disajikan; kontrak API disusun pada `design-business-module`.

---

## B. Ubiquitous language

Satu kata, satu makna. Bila sebuah kata dipakai berbeda oleh unit berbeda, perbedaannya
dipertahankan, tidak disatukan.

| Istilah | Makna bisnis yang berlaku |
| --- | --- |
| **MMC** | Rumah sakit pemilik dan pengguna Quilvian. Bukan pemasok darah. |
| **PMI** | Palang Merah Indonesia. Satu-satunya penyedia darah yang sah, berada di luar sistem. |
| **BDRS** | Unit Bank Darah Rumah Sakit di dalam MMC yang menjalankan modul ini. |
| **Order darah** | Permintaan kebutuhan darah **untuk seorang pasien**, lahir dari unit pelayanan. |
| **Permintaan darah** | Permintaan pasokan **kepada PMI** atas nama satu pasien. Berbeda dari order darah, dan tidak boleh disamakan. |
| **Kantong darah** | Satu kantong fisik yang sudah diterima MMC. Bukan stok nasional PMI. |
| **Alokasi** | Ikatan satu kantong pada satu baris kebutuhan order. Belum berarti darah keluar. |
| **Pemberian** | Kantong benar-benar diserahkan untuk pasien. Tindakan yang tidak dapat dibatalkan. |
| **Bukti kecocokan** | Catatan bahwa pemeriksaan kecocokan **sudah dinyatakan selesai** oleh petugas berwenang. Bukan hasil hitungan sistem. |
| **Jalur darurat** | Pemberian sebelum bukti kecocokan tercatat, hanya oleh peran berwenang, ditandai permanen. |
| **Kunjungan berakhir** | Rawat jalan dan IGD: status akhir kunjungan. Rawat inap: saat pasien benar-benar meninggalkan rumah sakit. Dua makna berbeda yang sengaja dipertahankan. |
| **Menunggu keputusan** | Keadaan kantong yang ordernya sudah berakhir dan nasibnya belum ditetapkan. |
| **Golongan darah diminta** | Keterangan pada permintaan. **Bukan** hasil pemeriksaan. |
| **Golongan darah hasil pemeriksaan** | Hasil periksa milik Bank Darah, punya status validasi. Satu-satunya yang sah untuk keperluan klinis. |
| **Hasil bertentangan** | Keadaan ketika hasil tervalidasi terbaru berbeda ABO atau Rhesus-nya dari hasil sah sebelumnya. Selama keadaan ini berlangsung, pasien **tidak punya** golongan darah sah sama sekali. |
| **Masa berlaku bukti kecocokan** | Rentang waktu setelah pemeriksaan kecocokan dinyatakan selesai, yang selama itu buktinya masih membuka gerbang pemberian. Lewat rentang itu, bukti tetap tersimpan tetapi berhenti membuka gerbang. |
| **Kantong berlebih** | Kantong yang datang dari PMI melebihi jumlah yang diminta. Tetap dicatat diterima, tetapi tidak pernah menjadi milik order pasien mana pun. |
| **Pembatalan alokasi** | Melepas ikatan kantong dari satu baris kebutuhan sebelum darah diberikan. Bukan penghapusan: barisnya tetap ada dan berhenti aktif. |
| **Catatan koreksi pemberian** | Catatan tambahan yang menyatakan bahwa **pencatatan** sebuah pemberian keliru. Bukan pernyataan bahwa darahnya tidak jadi diberikan, dan bukan pembatalan. |

Catatan penting soal kata "permintaan": dalam dokumen bisnis sehari-hari, orang sering menyebut
"permintaan darah" untuk dua hal berbeda — permintaan dokter ke Bank Darah, dan permintaan Bank Darah
ke PMI. Arsitektur ini memisahkan keduanya menjadi **order darah** dan **permintaan darah**, dan
perbedaan itu wajib dipertahankan di seluruh dokumen turunan.

---

## C. Peta bounded context

| ID | Context | Tanggung jawab | Konsep yang dimiliki | Hubungan dengan Bank Darah |
| --- | --- | --- | --- | --- |
| `BD-CTX-01` | **Bank Darah** | Seluruh lifecycle pemenuhan darah pasien di dalam MMC | Order darah, permintaan ke PMI, kantong operasional, alokasi, bukti kecocokan, pemeriksaan golongan darah, sampel, tindakan Bank Darah, katalog komponen, daftar alasan, riwayat pergerakan | — pemilik |
| `BD-CTX-02` | Registrasi dan Kunjungan | Kunjungan pasien rawat jalan dan IGD beserta status akhirnya | Kunjungan, status kunjungan | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-03` | Pasien | Identitas pasien | Pasien, golongan darah administratif | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-04` | Tenaga Kerja | Dokter dan pegawai | Dokter, pegawai | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-05` | Data Induk Layanan | Unit pelayanan, klinik, ruangan, kelas pasien, tindakan dan tarif | Unit pelayanan beserta tanda kewenangannya | Hulu. Bank Darah membaca, dan **menitipkan satu tanda kewenangan baru** |
| `BD-CTX-06` | Rawat Inap | Episode rawat inap beserta kepulangan pasien | Episode, waktu pasien meninggalkan rumah sakit | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-07` | Billing | Tarif dan akibat finansial | Kontrak sumber biaya, tagihan | Hilir. Bank Darah mengirim fakta tindakan selesai — **kontraknya belum disetujui** |
| `BD-CTX-08` | PMI | Penyediaan darah | Stok darah nasional | Luar sistem. **Tidak ada antarmuka teknis** pada MVP |
| `BD-CTX-09` | Laboratorium | Pemeriksaan laboratorium umum | Pesanan lab, sampel lab, hasil lab | **Jalan sendiri-sendiri** pada MVP. Tidak ada ketergantungan dua arah |

### Sifat hubungan antarcontext

**Terhadap hulu (`BD-CTX-02` sampai `BD-CTX-06`): Bank Darah menyesuaikan diri, tidak menuntut.**
Bank Darah menyimpan rujukan dan membaca status, tetapi tidak pernah mengubah data milik mereka dan
tidak menuntut mereka berubah demi kebutuhannya. Satu-satunya titipan adalah tanda kewenangan
memesan darah pada unit pelayanan (`DEC-BD-012`), dan itu pun mengikuti pola yang sudah ada di sana.

**Terhadap Billing (`BD-CTX-07`): kontrak terbitan pihak hilir.** Billing yang menetapkan bentuk
fakta biaya yang boleh diterimanya. Bank Darah menyesuaikan. Kontraknya belum boleh dipakai sampai
`DEC-BD-016` disetujui.

**Terhadap PMI (`BD-CTX-08`): tidak ada sambungan sama sekali.** Seluruh pertukaran terjadi lewat
manusia dan dokumen fisik. Karena tidak ada antarmuka, tidak ada lapisan penerjemah yang perlu
dirancang. Yang dirancang hanya pencatatan sisi MMC-nya.

**Terhadap Laboratorium (`BD-CTX-09`): sengaja berjalan sendiri-sendiri.** `DEC-BD-015` dan
`DEC-BD-018` menempatkan pemeriksaan golongan darah dan sampelnya di dalam Bank Darah, bukan di
Laboratorium. Ini keputusan sadar, bukan kelalaian, dan disertai klausa masa depan: bila kelak
Laboratorium mengambil alih, wajib ada keputusan kepemilikan dan penyelarasan sumber kebenaran.
Dilarang ada dua sumber sah tanpa aturan prioritas (`INV-BD-015`).

### Peninjauan ulang batas ownership pada revisi 2

Keenam keputusan baru diperiksa satu per satu terhadap lima batas kepemilikan yang paling mudah
bergeser tanpa disadari. Hasilnya: **tidak ada satu pun batas ownership yang berpindah.** Yang berubah
hanyalah ketegasan sebagian batas.

| Batas | Apakah bergeser? | Penjelasan |
| --- | --- | --- |
| **Billing** `BD-CTX-07` | Tidak | Tarif tetap milik Billing, dan Bank Darah tetap tidak pernah menghitungnya. `DEC-BD-030` tidak menyentuh Billing sama sekali: biaya berasal dari tindakan (`DEC-BD-021`), bukan dari kantong, sehingga koreksi nomor kantong tidak mengubah fakta biaya apa pun. Satu pertanyaan baru muncul dan **tidak** saya jawab sendiri — lihat `ARCH-BD-GAP-09` |
| **Laboratorium** `BD-CTX-09` | Tidak, tetapi jaraknya melebar | `DEC-BD-026` membuat Bank Darah kini memiliki bukan hanya hasil pemeriksaannya, melainkan juga **aturan hasil mana yang sah** dan **keadaan tertahan** ketika hasil bertentangan. Bank Darah juga memiliki gerbangnya, bukan pemeriksaannya: `DEC-BD-027` mengatur berapa lama bukti kecocokan membuka gerbang, sedangkan uji kecocokannya sendiri tetap milik proses klinis dan Laboratorium. Konsekuensinya, klausa masa depan pada `INV-BD-015` menjadi lebih berat: bila kelak Laboratorium mengambil alih golongan darah, yang harus dipindahkan bukan cuma datanya, melainkan juga aturan prioritas dan keadaan tertahan itu |
| **Pasien** `BD-CTX-03` | Tidak | `DEC-BD-026` melahirkan `BD-DOM-21`, jawaban atas pertanyaan "apa golongan darah sah pasien ini sekarang". Konsep itu **turunan**, bukan atribut pasien: ia dihitung dari pemeriksaan milik Bank Darah dan hanya menyimpan rujukan pasien. `MstPatient.BloodType` tetap data administratif dan tetap dilarang menjadi sumber klinis (`INV-BD-014`) |
| **Kunjungan** `BD-CTX-02` dan `BD-CTX-06` | Tidak | Tetap hanya dibaca. Yang bertambah adalah satu titik baca baru: `DEC-BD-029` menuntut Bank Darah mengetahui apakah order asal masih aktif pada saat alokasi dibatalkan, dan keaktifan itu turun dari status kunjungan lewat `BD-DOM-16`. Membaca lebih sering bukan berarti memiliki |
| **Unit Pelayanan** `BD-CTX-05` | Tidak | Tidak satu pun dari keenam keputusan menyentuhnya. Titipan Bank Darah tetap tepat satu, yaitu tanda kewenangan memesan darah (`BD-DOM-18`). Tidak ada titipan kedua |

---

## D. Katalog konsep domain

Klasifikasi menggambarkan tanggung jawab domain, bukan bentuk tabel database.

| ID | Nama bisnis | Klasifikasi | Pemilik | Identitas | Peran lifecycle | Invariant penting | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BD-DOM-01` | Order Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor order terbitan sistem | Aktif → terpenuhi sebagian → terpenuhi penuh, dibatalkan, atau kedaluwarsa | Wajib menunjuk pasien dan kunjungan yang sah; jumlah pemenuhan dihitung dari transaksi | `DEC-BD-004`, `DEC-BD-005`, `DEC-BD-006` |
| `BD-DOM-02` | Baris Kebutuhan Order | `ENTITY` di dalam `BD-DOM-01` | Bank Darah — `New` | Nomor urut dalam order | Menyimpan komponen dan jumlah yang diminta | Jumlah diminta lebih dari nol; komponen wajib dari katalog | `DEC-BD-011`, `DEC-BD-024` |
| `BD-DOM-03` | Permintaan Darah ke PMI | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor permintaan terbitan sistem | `REQUESTED` → `PARTIALLY_FULFILLED` → `FULFILLED`, `CANCELLED`, atau `CLOSED_ENCOUNTER` | Selalu atas nama satu pasien; sisa dihitung dari penerimaan nyata | `DEC-BD-003`, `DEC-BD-008`, `DEC-BD-020` |
| `BD-DOM-04` | Penerimaan Kantong | `ENTITY` di dalam `BD-DOM-03` | Bank Darah — `New` | Nomor urut penerimaan | Mencatat kedatangan fisik, termasuk kedatangan yang melebihi jumlah diminta | Stok bertambah **hanya** lewat konsep ini; penerimaan tidak pernah ditolak karena kelebihan, dan tidak pernah membuat sisa permintaan menjadi negatif | `DEC-BD-002`, `DEC-BD-025` |
| `BD-DOM-05` | Kantong Darah Operasional | `AGGREGATE_ROOT` | Bank Darah — `New` | Identitas internal, dengan **nomor kantong terbitan PMI** sebagai identifier bisnis yang unik | Tersedia → dialokasikan → diberikan, atau menunggu keputusan → dialihkan, dikembalikan, tidak layak | Tidak pernah menjadi stok bebas; tidak boleh punya lebih dari satu alokasi aktif | `DEC-BD-001`, `DEC-BD-007`, `DEC-BD-019`, `ASM-BD-003` |
| `BD-DOM-06` | Alokasi | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Mengikat kantong pada satu baris kebutuhan order; dapat berhenti aktif lewat pembatalan | Satu kantong hanya boleh punya satu alokasi **aktif**; alokasi yang dibatalkan tidak dihapus, hanya berhenti aktif, dan menyimpan alasan, pelaku, serta waktu | `DEC-BD-003`, `DEC-BD-029`, BG-BD-002 |
| `BD-DOM-07` | Bukti Kecocokan | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal, **selalu terhadap pasangan kantong dan pasien tertentu** | Gerbang sebelum pemberian, dengan masa berlaku | Wajib ada sebelum pemberian, kecuali lewat jalur darurat · hanya membuka gerbang untuk pasien yang dituju bukti itu · berhenti membuka gerbang setelah masa berlakunya lewat | `DEC-BD-013`, `DEC-BD-027`, `DEC-BD-028`, `INV-BD-012`, `INV-BD-019`, `INV-BD-020` |
| `BD-DOM-08` | Otorisasi Darurat | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Menggantikan gerbang bukti kecocokan pada keadaan darurat | Hanya oleh peran berwenang; alasan wajib; penanda melekat permanen | `DEC-BD-017` |
| `BD-DOM-09` | Pemeriksaan Golongan Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Identitas internal | Sampel diambil → hasil dicatat → hasil tervalidasi | Hasil belum tervalidasi tidak boleh dipakai untuk keperluan klinis | `DEC-BD-015`, `INV-BD-014` |
| `BD-DOM-21` | Golongan Darah Sah Pasien | `Adapter/View` | Bank Darah — turunan | Dikenali lewat rujukan pasien | Menjawab satu pertanyaan: apa golongan darah sah pasien ini sekarang, atau apakah sedang bertentangan | Dihitung dari hasil tervalidasi milik `BD-DOM-09`, **bukan** kolom yang bisa disunting · bernilai kosong ketika hasil bertentangan · tidak pernah membaca `MstPatient.BloodType` | `DEC-BD-026`, `INV-BD-014`, `INV-BD-018` |
| `BD-DOM-22` | Penyelesaian Perbedaan Hasil | `ENTITY` mandiri, di luar batas `BD-AGG-04` | Bank Darah — `New` | Identitas internal, menunjuk pasien dan hasil-hasil yang bertentangan | Mengakhiri keadaan tertahan pada `BD-DOM-21` | Hanya dapat ditambah · wajib menyimpan validator, waktu, dan alasan · **mekanisme persisnya belum ditetapkan — `ARCH-BD-GAP-07`** | `DEC-BD-026`, `DEF-BD-004` |
| `BD-DOM-10` | Sampel Bank Darah | `ENTITY` di dalam `BD-DOM-09` | Bank Darah — `New` | Identifier sampel terbitan sistem | Menjaga penelusuran dari pasien ke hasil | Bukan sampel Laboratorium; tidak menimbulkan tagihan Laboratorium | `DEC-BD-018` |
| `BD-DOM-11` | Golongan Darah dan Rhesus | `VALUE_OBJECT` | Platform — `Existing` | — | Dipakai pada permintaan dan pada hasil pemeriksaan | Nilai yang sama dipakai di kedua tempat, **tetapi maknanya berbeda** | `BD-CAP-016` |
| `BD-DOM-12` | Tindakan Bank Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor tindakan terbitan sistem | Dicatat → selesai | Menunjuk order, unit, dokter BDRS, petugas, kelas, dan tindakan bertarif | `DEC-BD-021`, BR-BD-004 |
| `BD-DOM-13` | Katalog Komponen Darah | `REFERENCE_DATA` | Bank Darah — `New` | Kode komponen | Dipakai order, permintaan, dan deteksi ganda. **Calon** tempat menyimpan masa berlaku bukti kecocokan bila nilainya ditetapkan per komponen | Komponen tidak boleh berupa ketikan bebas. Kumpulan atributnya **belum boleh dibekukan** — `ARCH-BD-GAP-08` | `DEC-BD-024`, `DEC-BD-005`, `DEC-BD-027` |
| `BD-DOM-14` | Daftar Alasan Terkendali | `REFERENCE_DATA` | Bank Darah — `New` | Kode alasan | Dipakai pembatalan, jalur darurat, dan penyelesaian kantong | Alasan tidak boleh teks bebas semata; perubahan wajib berjejak | `DEC-BD-024`, `INV-BD-016` |
| `BD-DOM-15` | Riwayat Pergerakan | `DOMAIN_EVENT` tersimpan | Bank Darah — `New` | Identitas internal | Merekam setiap perpindahan status yang berarti | Hanya bisa ditambah; tidak pernah diubah atau dihapus | `BD-CAP-009`, BG-BD-004 |
| `BD-DOM-16` | Pembaca Status Kunjungan | `Adapter/View` | Bank Darah membaca `BD-CTX-02` dan `BD-CTX-06` | — | Menjawab satu pertanyaan: apakah kunjungan ini sudah berakhir | Tidak pernah mengubah data hulu | `DEC-BD-014` |
| `BD-DOM-17` | Ringkasan Pemenuhan | `Adapter/View` | Bank Darah — turunan | — | Menyajikan jumlah diminta, diberikan, dan belum diberikan | Dihitung dari transaksi, **bukan** kolom yang bisa disunting · wajib menghormati catatan koreksi `BD-DOM-23`, sehingga pemberian yang pencatatannya dikoreksi tidak dihitung dua kali | BG-BD-003, INV-BD-003, `DEC-BD-030` |
| `BD-DOM-18` | Kewenangan Unit Memesan Darah | `Extend` pada `BD-CTX-05` | Data Induk Layanan | — | Menentukan unit mana yang boleh membuat order | Bawaan menolak; tidak dikunci di kode | `DEC-BD-012`, `BD-CAP-005` |
| `BD-DOM-19` | Kontrak Sumber Biaya Bank Darah | `Extend` pada `BD-CTX-07` | Billing | — | Menerima fakta tindakan selesai | **Belum disetujui** — `DEC-BD-016` | `BD-CAP-015` |
| `BD-DOM-20` | Pasien, Kunjungan, Dokter, Unit, Kelas | `Existing` | Context hulu masing-masing | — | Rujukan | Tidak pernah diduplikasi ke Bank Darah | `BD-CAP-001`, `002`, `004`, `006` |
| `BD-DOM-23` | Catatan Koreksi Pemberian | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal, menunjuk satu pemberian asal | Menyatakan bahwa **pencatatan** sebuah pemberian keliru | Hanya dapat ditambah · tidak pernah menghapus atau membalik pemberian asal · tidak boleh dipakai memindahkan pemberian ke pasien lain · wajib menyimpan apa yang keliru, apa yang benar, alasan terkendali, pelaku, dan waktu | `DEC-BD-030`, `INV-BD-021` |

Tidak ada satu pun konsep di atas yang diturunkan dari nama menu, nama layar, atau nama task.
Sebagai contoh, menu `Laporan` dan menu `Setup` pada bukti navigasi **tidak** melahirkan konsep
domain bernama Laporan atau Setup: yang lahir hanyalah dua data rujukan pada `BD-DOM-13` dan
`BD-DOM-14`, karena keduanya memang dituntut keputusan bisnis, bukan dituntut oleh nama menunya.

**Tiga konsep baru pada revisi 2, dan kenapa hanya tiga.** Enam keputusan baru bisa saja diterjemahkan
menjadi enam konsep, satu per keputusan. Itu tidak dilakukan, karena empat di antaranya tidak
memperkenalkan identitas, lifecycle, maupun tanggung jawab audit yang benar-benar berbeda:

- `DEC-BD-025` tidak melahirkan konsep "kantong berlebih". Kantong berlebih tetap kantong biasa
  (`BD-DOM-05`); yang berbeda hanya keadaan awalnya saat lahir. Membuat konsep terpisah berarti
  membuat entity per status, dan itu dilarang kontrak.
- `DEC-BD-027` tidak melahirkan konsep "bukti kedaluwarsa". Kedaluwarsa adalah kondisi turunan atas
  `BD-DOM-07`, bukan benda baru — lihat `ARCH-BD-POS-01`.
- `DEC-BD-028` tidak melahirkan konsep apa pun. Ia hanya mempertegas aturan identitas `BD-DOM-07`
  bahwa bukti selalu terikat pasangan kantong dan pasien.
- `DEC-BD-029` tidak melahirkan konsep "pembatalan alokasi". Pembatalan adalah perpindahan pada
  `BD-DOM-06`, bukan entity tersendiri.

Yang benar-benar baru hanya tiga: `BD-DOM-21` karena pertanyaan "golongan darah sah pasien" tidak
punya pemilik sebelumnya, `BD-DOM-22` karena penyelesaian perbedaan punya pelaku, waktu, dan alasan
sendiri yang harus tahan audit, dan `BD-DOM-23` karena koreksi pencatatan adalah pernyataan bisnis
tersendiri yang tidak boleh menyamar sebagai perubahan pada pemberian aslinya.

---

## E. Model aggregate

Aggregate dipakai hanya ketika ada batas konsistensi yang harus melindungi invariant. Lima aggregate
berikut lahir dari lifecycle yang benar-benar berbeda.

### Peninjauan ulang batas aggregate pada revisi 2

Kelima batas ditinjau ulang terhadap keenam keputusan baru. **Tidak ada satu pun batas aggregate yang
berpindah.** Yang berubah adalah isi invariant, daftar tindakan, dan daftar kejadiannya.

| Nama dalam permintaan tinjauan | Aggregate di dokumen ini | Batas bergeser? | Yang berubah |
| --- | --- | --- | --- |
| `BloodRequest` | `BD-AGG-01` Order Darah | Tidak | Tidak ada perubahan langsung. Angka pemenuhannya kini dihitung dengan menghormati catatan koreksi (`BD-DOM-23`) |
| `PMI Request` | `BD-AGG-02` Permintaan Darah ke PMI | Tidak | Invariant sisa diperbaiki agar tidak pernah negatif; lahir `BD-XINV-03` |
| `BloodUnit` | `BD-AGG-03` Kantong Darah Operasional | Tidak | Bertambah dua perpindahan (pembatalan alokasi, catatan koreksi) dan tiga invariant baru |
| `BloodGroupVerification` | `BD-AGG-04` Pemeriksaan Golongan Darah | Tidak | Aturan "hasil mana yang sah" **tidak** dimasukkan ke dalamnya, melainkan menjadi `BD-XINV-04` |
| `BloodBankProcedure` | `BD-AGG-05` Tindakan Bank Darah | Tidak | Tidak tersentuh keenam keputusan baru |

**Peringatan penamaan.** Nama `BloodRequest` untuk order pasien berisiko mengembalikan kekacauan yang
sengaja dibereskan bagian B: dalam bahasa bisnis Bank Darah, "permintaan" adalah permintaan ke PMI,
sedangkan yang datang dari unit pelayanan adalah "order". Penamaan teknis dibekukan pada
`design-business-module`, dan dokumen ini hanya menandai risikonya, tidak menetapkan namanya.

**Kenapa aturan hasil golongan darah tidak masuk ke `BD-AGG-04`.** Pertanyaan "hasil mana yang sah"
tidak dapat dijawab dari dalam satu pemeriksaan, karena jawabannya bergantung pada **seluruh**
pemeriksaan tervalidasi milik pasien itu. Memperluas batas `BD-AGG-04` menjadi "semua pemeriksaan satu
pasien" akan membuat setiap pencatatan hasil mengunci seluruh riwayat pasien tersebut, dan itu harga
yang tidak sepadan. Karena itu aturannya ditempatkan sebagai invariant lintas aggregate `BD-XINV-04`,
mengikuti pola yang sudah dipakai `BD-XINV-01` dan `BD-XINV-02`.

### `BD-AGG-01` — Order Darah

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-01` Order Darah |
| Batas | Order beserta baris kebutuhannya (`BD-DOM-02`) |
| Invariant yang dilindungi | Order selalu menunjuk pasien dan kunjungan yang sah · setiap baris punya komponen dari katalog dan jumlah lebih dari nol · order manual wajib menyimpan dokter peminta, unit asal, dan pelaku input |
| Tindakan bisnis | Buat order dari unit pelayanan · buat order manual oleh Bank Darah · batalkan order |
| Kejadian yang diterbitkan | Order dibuat · order dibatalkan · order kedaluwarsa · order terpenuhi |

### `BD-AGG-02` — Permintaan Darah ke PMI

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-03` Permintaan Darah |
| Batas | Permintaan beserta catatan penerimaannya (`BD-DOM-04`) |
| Invariant yang dilindungi | Selalu atas nama satu pasien · **jumlah sisa tidak pernah bernilai negatif**, yaitu jumlah diminta dikurangi jumlah diterima, dengan batas bawah nol (`INV-BD-017`) · permintaan yang belum selesai tidak boleh digandakan untuk kebutuhan yang sama · penerimaan fisik tidak pernah ditolak karena kelebihan |
| Tindakan bisnis | Buat permintaan · catat penerimaan kantong, termasuk yang melebihi jumlah diminta · batalkan permintaan · tutup karena kunjungan berakhir |
| Kejadian yang diterbitkan | Permintaan dibuat · kantong diterima · **kantong berlebih diterima** · permintaan terpenuhi · permintaan ditutup karena kunjungan berakhir |

**Kenapa kelebihan kiriman tidak membuat permintaan gagal.** Permintaan adalah catatan administratif; kantong adalah benda fisik. Menolak mencatat kantong ketiga karena angka administratifnya sudah penuh berarti membiarkan darah nyata berada di kulkas tanpa jejak di sistem — keadaan yang jauh lebih berbahaya daripada angka yang tidak rapi. `DEC-BD-025` memilih menjaga kebenaran fisik, lalu menyalurkan ketidakrapiannya ke `PENDING_REVIEW` yang memang sudah dirancang untuk itu.

### `BD-AGG-03` — Kantong Darah Operasional

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-05` Kantong Darah |
| Batas | Kantong beserta alokasinya (`BD-DOM-06`), bukti kecocokannya (`BD-DOM-07`), otorisasi daruratnya (`BD-DOM-08`), dan catatan koreksi pemberiannya (`BD-DOM-23`) |
| Invariant yang dilindungi | **Satu kantong tidak boleh punya lebih dari satu alokasi aktif** · kantong tidak pernah menjadi stok bebas · kantong tidak dapat diberikan tanpa bukti kecocokan **yang berlaku untuk pasien tujuan dan belum lewat masa berlakunya**, atau tanpa otorisasi darurat (`INV-BD-019`, `INV-BD-020`) · kantong yang ordernya berakhir tidak dapat dialokasikan ke pasien lain sebelum diselesaikan · **pemberian tidak pernah dihapus maupun dibalik** (`INV-BD-021`) |
| Tindakan bisnis | Alokasikan ke baris kebutuhan · **batalkan alokasi** · catat bukti kecocokan · berikan · berikan lewat jalur darurat · **catat koreksi pencatatan pemberian** · tandai menunggu keputusan · alihkan ke pasien lain · kembalikan ke PMI · nyatakan tidak layak |
| Kejadian yang diterbitkan | Kantong diterima · dialokasikan · **alokasi dibatalkan** · bukti kecocokan tercatat · **bukti kecocokan gugur karena pengalihan** · diberikan · diberikan darurat · **koreksi pemberian dicatat** · masuk menunggu keputusan · dialihkan · dikembalikan · dinyatakan tidak layak |

**Kenapa alokasi ditempatkan di dalam kantong, bukan di dalam order.** Ada dua aturan yang
bersaing memperebutkan batas konsistensi yang sama. Pertama, satu kantong tidak boleh terpakai dua
kali. Kedua, jumlah kantong yang dialokasikan tidak boleh melebihi jumlah yang diminta pada order.
Keduanya tidak dapat dilindungi satu batas konsistensi sekaligus.

Yang dipilih adalah melindungi aturan pertama, karena akibat pelanggarannya jauh lebih berat: satu
kantong yang sama diberikan kepada dua pasien. Akibat pelanggaran aturan kedua hanyalah kelebihan
alokasi yang masih bisa dikoreksi tanpa membahayakan siapa pun.

**Contoh nyata.** Dua petugas membuka daftar kantong pada saat hampir bersamaan, dan keduanya
memilih kantong yang sama untuk dua pasien berbeda. Karena alokasi dilindungi batas konsistensi
kantong, tepat satu petugas berhasil dan satunya menerima penolakan yang jelas. Sebaliknya, bila
dua petugas mengalokasikan dua kantong berbeda untuk order yang hanya membutuhkan satu, keduanya
bisa saja berhasil — dan itu ditangani dengan pemeriksaan terpisah pada sisi order, bukan dengan
mengorbankan perlindungan yang pertama.

**Bagaimana pembatalan alokasi menjaga invariant yang sama.** `DEC-BD-029` menambah satu perpindahan,
bukan satu pengecualian. Alokasi yang dibatalkan **tidak dihapus**: barisnya tetap ada, berhenti
aktif, dan menyimpan alasan, pelaku, serta waktu. Invariant "satu alokasi aktif" karena itu dinilai
atas himpunan alokasi yang **sedang aktif**, bukan atas jumlah barisnya — lihat `ARCH-BD-POS-03`.
Dengan begitu satu kantong bisa punya riwayat panjang berisi tiga atau empat alokasi, dan tetap tidak
pernah melanggar aturan yang melindunginya.

**Kenapa catatan koreksi berada di dalam batas kantong, bukan di luarnya.** Catatan koreksi
(`BD-DOM-23`) harus selalu konsisten dengan pemberian yang dikoreksinya: tidak boleh ada koreksi yang
menunjuk pemberian yang tidak ada, dan tidak boleh ada dua koreksi yang saling bertentangan atas satu
pemberian yang sama. Konsistensi itu hanya dapat dijamin bila keduanya berada dalam satu batas.
Menempatkan koreksi di luar akan mengubahnya menjadi catatan yang kebenarannya hanya dijaga niat baik.

**Batas yang sengaja tidak ditembus.** Catatan koreksi berhenti pada pernyataan "pencatatannya
keliru". Ia tidak mengubah pasien tujuan, tidak mengembalikan kantong ke keadaan tersedia, dan tidak
membatalkan pemberian. Nasib kantong yang tercatat keliru dinyatakan terpisah oleh manusia, dan
mekanismenya belum ditetapkan — `OQ-BD-014`.

**Satu gerbang, tiga keputusan.** `DEC-BD-013`, `DEC-BD-027`, dan `DEC-BD-028` tampak seperti tiga
aturan berbeda, tetapi ketiganya jatuh ke satu pertanyaan yang sama pada saat pemberian hendak
dilakukan: *apakah ada bukti kecocokan untuk kantong ini, terhadap pasien ini, yang belum lewat masa
berlakunya?* Menjawabnya sebagai satu pertanyaan mencegah lahirnya tiga mekanisme terpisah yang bisa
saling bertentangan — lihat `ARCH-BD-POS-02`.

### `BD-AGG-04` — Pemeriksaan Golongan Darah

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-09` Pemeriksaan Golongan Darah |
| Batas | Pemeriksaan beserta sampelnya (`BD-DOM-10`) dan status validasinya |
| Invariant yang dilindungi | Hasil yang belum tervalidasi tidak boleh dipakai untuk keperluan klinis · hasil wajib menyimpan pemeriksa dan waktu pemeriksaan · sampel bersifat opsional, tetapi bila ada wajib menyimpan waktu dan petugas pengambil · hasil yang sudah tervalidasi tidak pernah ditimpa oleh hasil berikutnya |
| Tindakan bisnis | Catat pengambilan sampel · catat hasil ABO dan Rhesus · validasi hasil |
| Kejadian yang diterbitkan | Sampel diambil · hasil dicatat · hasil tervalidasi · **hasil bertentangan terdeteksi** · **perbedaan hasil diselesaikan** |

**Catatan batas.** Dua kejadian terakhir diterbitkan oleh `BD-AGG-04`, tetapi akibatnya — pasien kehilangan golongan darah sah, lalu memperolehnya kembali — terbaca pada `BD-DOM-21` yang bersifat turunan. `BD-AGG-04` tidak menyimpan jawaban "golongan darah sah pasien" di dalam dirinya, karena jawaban itu bukan miliknya sendiri melainkan milik seluruh pemeriksaan pasien tersebut.

### `BD-AGG-05` — Tindakan Bank Darah

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-12` Tindakan Bank Darah |
| Batas | Tindakan beserta konteksnya: unit, dokter BDRS, petugas, kelas pasien, tindakan bertarif |
| Invariant yang dilindungi | Menunjuk satu order yang sah · tarif tidak pernah dihitung sendiri, selalu merujuk milik Billing · satu tindakan menghasilkan **paling banyak satu** fakta biaya |
| Tindakan bisnis | Catat tindakan · nyatakan tindakan selesai |
| Kejadian yang diterbitkan | Tindakan selesai — **penyalurannya ke Billing tertahan `DEC-BD-016`** |

### Aturan yang tidak dapat dilindungi satu aggregate

Dua aturan berikut melintasi lebih dari satu order atau permintaan, sehingga tidak dapat dijaga oleh
batas konsistensi mana pun. Keduanya perlu pemeriksaan tersendiri beserta pengaman perebutan data.

| ID | Aturan | Kenapa melintas |
| --- | --- | --- |
| `BD-XINV-01` | Tidak boleh ada dua order aktif dengan pasien, kunjungan, dan komponen yang sama (`DEC-BD-005`) | Melibatkan seluruh order milik pasien itu, bukan satu order saja |
| `BD-XINV-02` | Tidak boleh ada dua permintaan aktif ke PMI untuk kebutuhan yang sama (`DEC-BD-008`) | Melibatkan seluruh permintaan yang masih berjalan |
| `BD-XINV-03` | Kantong yang lahir dari satu permintaan tidak boleh membuat sisa permintaan itu menjadi negatif (`DEC-BD-025`, `INV-BD-017`) | Penerimaan dicatat di dalam `BD-AGG-02`, sedangkan kantong yang lahir darinya adalah `BD-AGG-03` yang berdiri sendiri. Dua penerimaan yang hampir bersamaan dapat sama-sama merasa masih di dalam kuota |
| `BD-XINV-04` | Seorang pasien memiliki paling banyak satu golongan darah sah pada satu waktu (`DEC-BD-026`, `INV-BD-018`) | Melibatkan seluruh pemeriksaan tervalidasi milik pasien itu, bukan satu pemeriksaan saja |

Contoh mengapa ini penting: perawat IGD memesan elektronik pada saat yang hampir bersamaan dengan
petugas Bank Darah yang menginput formulir kertas untuk kebutuhan yang sama. Keduanya tidak saling
melihat, dan tidak ada satu pun order yang bisa "menahan" yang lain dari dalam dirinya sendiri.

Keempat aturan ini menuntut pemeriksaan tersendiri beserta pengaman perebutan data. Pola token
konkurensi yang sudah terbukti berjalan pada `BD-CAP-010` cukup untuk keperluan itu; tidak ada
mekanisme baru yang perlu diciptakan.

### Posisi arsitektur yang diambil pada revisi 2

Tiga hal berikut adalah **keputusan pemodelan**, bukan aturan bisnis baru. Keputusan bisnisnya sudah
turun; yang dipilih di sini hanyalah cara mewujudkannya secara logis.

| ID | Posisi | Alasan |
| --- | --- | --- |
| `ARCH-BD-POS-01` | Lewatnya masa berlaku bukti kecocokan dimodelkan sebagai **kondisi turunan** yang dihitung saat gerbang diperiksa, bukan sebagai perubahan status yang disimpan | Tidak menuntut penjadwal latar belakang, tidak membuat keadaan kantong bergantung pada kapan pekerjaan latar terakhir berjalan, dan tetap benar walaupun nilai masa berlakunya kelak diubah. Konsekuensinya, tidak ada kejadian "bukti kedaluwarsa" yang perlu diterbitkan |
| `ARCH-BD-POS-02` | Gerbang pemberian dinyatakan sebagai **satu** pertanyaan atas tiga hal sekaligus: kantong, pasien tujuan, dan waktu | `DEC-BD-013`, `DEC-BD-027`, dan `DEC-BD-028` semuanya jatuh ke predikat yang sama. Memecahnya menjadi tiga pemeriksaan terpisah membuka peluang salah satu terlewat |
| `ARCH-BD-POS-03` | Invariant "satu alokasi aktif" dinilai atas himpunan alokasi yang **sedang aktif**, bukan atas jumlah baris alokasi | `DEC-BD-029` menuntut pembatalan tanpa penghapusan. Bila invariant dinilai dari jumlah baris, riwayat yang jujur akan tampak seperti pelanggaran |

---

## F. Model relasi

| Sumber | Tujuan | Makna | Kardinalitas | Arah kepemilikan | Wajib? | Ketergantungan lifecycle |
| --- | --- | --- | --- | --- | --- | --- |
| Order Darah | Pasien | Order dibuat untuk seorang pasien | banyak ke satu | Pasien dimiliki context hulu | Wajib | Order tidak pernah menghapus pasien |
| Order Darah | Kunjungan | Order terikat konteks pelayanan | banyak ke satu | Kunjungan dimiliki context hulu | Wajib | Berakhirnya kunjungan mengakhiri order |
| Order Darah | Unit pelayanan asal | Menunjukkan asal kebutuhan | banyak ke satu | Unit dimiliki context hulu | Wajib | — |
| Order Darah | Dokter peminta | Penanggung jawab indikasi klinis | banyak ke satu | Dokter dimiliki context hulu | Wajib | — |
| Order Darah | Baris Kebutuhan | Rincian komponen dan jumlah | satu ke banyak | Milik order | Minimal satu | Ikut order |
| Baris Kebutuhan | Komponen darah | Komponen yang diminta | banyak ke satu | Katalog milik Bank Darah | Wajib | — |
| Permintaan Darah | Order Darah | Permintaan lahir dari kebutuhan order | banyak ke satu | Milik Bank Darah | Wajib | Berakhirnya kunjungan menutup permintaan |
| Permintaan Darah | Penerimaan Kantong | Catatan kedatangan | satu ke banyak | Milik permintaan | Boleh kosong | Ikut permintaan |
| Penerimaan Kantong | Kantong Darah | Penerimaan melahirkan kantong | satu ke banyak | Kantong berdiri sendiri | Wajib | Kantong tetap hidup setelah permintaan ditutup |
| Kantong Darah | Permintaan asal | Jejak asal-usul | banyak ke satu | Milik Bank Darah | Wajib | **Tidak pernah putus**, termasuk setelah kantong dialihkan |
| Kantong Darah | Alokasi | Ikatan ke baris kebutuhan | satu ke banyak, **maksimal satu aktif** | Milik kantong | Boleh kosong | Ikut kantong |
| Alokasi | Baris Kebutuhan Order | Kantong ini untuk kebutuhan itu | banyak ke satu | Order dimiliki `BD-AGG-01` | Wajib | — |
| Kantong Darah | Bukti Kecocokan | Gerbang sebelum pemberian | satu ke banyak | Milik kantong | Wajib sebelum pemberian | Ikut kantong |
| Bukti Kecocokan | Pasien yang dituju | Bukti selalu terhadap pasien tertentu | banyak ke satu | Pasien dimiliki context hulu | Wajib | Bukti tidak ikut berpindah ketika kantong dialihkan; ia berhenti berlaku dan tetap tersimpan |
| Kantong Darah | Catatan Koreksi Pemberian | Pernyataan bahwa pencatatan pemberian keliru | satu ke banyak | Milik kantong | Boleh kosong | Ikut kantong; tidak pernah dihapus |
| Catatan Koreksi Pemberian | Pemberian asal | Koreksi selalu menunjuk satu pemberian tertentu | banyak ke satu | Milik kantong yang sama | Wajib | Pemberian asal tetap ada selamanya |
| Penyelesaian Perbedaan Hasil | Pemeriksaan Golongan Darah yang bertentangan | Menyebut hasil-hasil yang diperselisihkan | satu ke banyak | Milik Bank Darah | Wajib minimal dua | Hasil yang disebut tetap tersimpan utuh |
| Penyelesaian Perbedaan Hasil | Pasien | Perbedaan selalu milik seorang pasien | banyak ke satu | Pasien dimiliki context hulu | Wajib | — |
| Golongan Darah Sah Pasien | Pemeriksaan Golongan Darah | Jawaban dihitung dari hasil tervalidasi | satu ke banyak | Turunan, tidak memiliki apa pun | — | Tidak menyimpan keadaan sendiri |
| Pemeriksaan Golongan Darah | Pasien | Hasil milik seorang pasien | banyak ke satu | Pasien dimiliki context hulu | Wajib | — |
| Pemeriksaan Golongan Darah | Sampel | Asal hasil | satu ke satu | Milik pemeriksaan | Opsional | Ikut pemeriksaan |
| Tindakan Bank Darah | Order Darah | Tindakan dilakukan atas order | banyak ke satu | Milik Bank Darah | Wajib | — |
| Tindakan Bank Darah | Tindakan bertarif | Rujukan tarif | banyak ke satu | Tarif dimiliki Billing | Wajib | — |

Tidak ada satu pun relasi di atas yang berupa penyalinan data induk. Seluruh rujukan ke pasien,
kunjungan, dokter, unit, kelas, dan tarif disimpan sebagai penunjuk, bukan sebagai salinan.
Penyalinan hanya boleh terjadi pada nilai yang memang harus dibekukan pada saat kejadian, seperti
kode dan nama tindakan beserta tarifnya pada `BD-AGG-05`, mengikuti pola yang sudah terbukti berjalan
pada `BD-CAP-008`.

---

## G. Model lifecycle dan perubahan status

### G.1 Order Darah

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Pemeriksaan invariant | Kejadian audit |
| --- | --- | --- | --- | --- | --- | --- |
| — | Buat order | Aktif | Unit pelayanan berwenang, atau petugas Bank Darah untuk jalur manual | Pasien dan kunjungan sah; unit punya kewenangan | `BD-XINV-01` deteksi ganda | Order dibuat, pelaku tercatat |
| Aktif | Sebagian kantong diberikan | Terpenuhi sebagian | Petugas Bank Darah | Ada pemberian yang sah | Jumlah dihitung dari transaksi | Perubahan status |
| Aktif atau terpenuhi sebagian | Seluruh kantong diberikan | Terpenuhi penuh | Petugas Bank Darah | — | — | Perubahan status |
| Aktif atau terpenuhi sebagian | Batalkan | Dibatalkan | Pihak berwenang | Alasan dari daftar terkendali | — | Alasan, pelaku, waktu |
| Aktif atau terpenuhi sebagian | Kunjungan berakhir | Kedaluwarsa | Sistem | `BD-DOM-16` menyatakan kunjungan berakhir | — | Perubahan status beserta sumber sinyalnya |

**Koreksi dan pembukaan kembali.** Order yang sudah kedaluwarsa **tidak** dapat dihidupkan kembali
(`ASM-BD-002`). Pasien yang masih membutuhkan darah dibuatkan order baru pada kunjungan baru.
Pembatalan tidak menghapus apa pun; ia hanya mengubah status dan menyimpan alasannya.

### G.2 Permintaan Darah ke PMI

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Buat permintaan | `REQUESTED` | Petugas Bank Darah | Ada order aktif; `BD-XINV-02` lolos | Permintaan dibuat |
| `REQUESTED` | Terima sebagian | `PARTIALLY_FULFILLED` | Petugas Bank Darah | Kantong ada secara fisik | Penerimaan, jumlah, pelaku, waktu |
| `REQUESTED` atau `PARTIALLY_FULFILLED` | Terima sisa | `FULFILLED` | Petugas Bank Darah | Jumlah diterima sama dengan diminta | Penerimaan |
| `REQUESTED` atau `PARTIALLY_FULFILLED` | Batalkan | `CANCELLED` | Pihak berwenang | Alasan dari daftar terkendali | Alasan, pelaku, waktu |
| `REQUESTED` atau `PARTIALLY_FULFILLED` | Kunjungan berakhir | `CLOSED_ENCOUNTER` | Sistem | `BD-DOM-16` menyatakan kunjungan berakhir | Perubahan status |
| `CLOSED_ENCOUNTER` | Kantong tetap datang | Tetap `CLOSED_ENCOUNTER` | Petugas Bank Darah | — | Penerimaan tetap tercatat; kantong masuk menunggu keputusan |
| `REQUESTED` atau `PARTIALLY_FULFILLED` | Terima kantong melebihi jumlah diminta | `FULFILLED`, sisa berhenti di 0 | Petugas Bank Darah | `BD-XINV-03` dijaga dengan token konkurensi | Penerimaan tercatat; kantong berlebih ditandai dan langsung masuk menunggu keputusan |

**Contoh berangka.** Diminta 3 kantong PRC untuk Tn. S. Hari pertama datang 2, status menjadi
`PARTIALLY_FULFILLED` dengan sisa 1. Tn. S pulang Senin siang, permintaan menjadi `CLOSED_ENCOUNTER`.
Selasa pagi kantong sisanya tetap diantar. Penerimaan **tetap dicatat**, kantong tetap membawa jejak
permintaan asalnya, lalu langsung masuk keadaan menunggu keputusan.

**Contoh kelebihan kiriman.** Diminta 2 kantong PRC untuk Tn. S, yang datang 3. Kantong pertama dan
kedua masuk sebagai penerimaan biasa dan permintaan menjadi `FULFILLED` dengan sisa 0. Kantong ketiga
tetap dicatat diterima, tetap menunjuk permintaan asal, lalu langsung masuk menunggu keputusan. Sisa
permintaan **tidak** menjadi minus 1, dan kantong ketiga **tidak** boleh dialokasikan langsung ke
order Tn. S walaupun pasiennya sama — ia wajib melewati penyelesaian `DEC-BD-019` lebih dulu.

### G.3 Kantong Darah Operasional

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Diterima secara fisik | Tersedia | Petugas Bank Darah | Terikat permintaan asal | Penerimaan |
| Tersedia | Alokasikan | Dialokasikan | Petugas Bank Darah | Order aktif; tidak ada alokasi aktif lain pada kantong ini | Alokasi, pelaku, waktu |
| Dialokasikan | Catat bukti kecocokan | Dialokasikan, bukti lengkap | Petugas berwenang | — | Status pemeriksaan, pelaku, waktu |
| Dialokasikan, bukti lengkap | Berikan | Diberikan | Petugas Bank Darah | Bukti kecocokan ada **untuk pasien tujuan** dan **belum lewat masa berlaku** (`ARCH-BD-POS-02`) | Pemberian, beserta rujukan bukti yang dipakai |
| Dialokasikan | Batalkan alokasi | Tersedia, atau menunggu keputusan bila order asal sudah berakhir | Petugas Bank Darah | Kantong belum diberikan; alasan dari daftar terkendali; keaktifan order asal dibaca lewat `BD-DOM-16` | Alasan, pelaku, waktu, dan alokasi mana yang berhenti aktif |
| Diberikan | Catat koreksi pencatatan | Tetap `Diberikan`, dengan catatan koreksi melekat | Peran berwenang — `DEF-BD-004` | Alasan dari daftar terkendali; menunjuk satu pemberian yang benar-benar ada | Apa yang keliru, apa yang benar, pelaku, dan waktu |
| Dialokasikan | Berikan lewat jalur darurat | Diberikan, **ditandai tanpa bukti** | **Peran berwenang — `DEF-BD-004`** | Alasan wajib dari daftar terkendali | Otorisasi darurat lengkap dengan penandanya |
| Tersedia atau dialokasikan | Order berakhir | `PENDING_REVIEW` | Sistem | Order kedaluwarsa atau dibatalkan | Perubahan status |
| `PENDING_REVIEW` | Alihkan ke pasien lain | `REALLOCATED` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib | Pasien asal, alasan pelepasan, pasien tujuan, **dan bukti kecocokan mana saja yang gugur karenanya** |
| `PENDING_REVIEW` | Kembalikan ke PMI | `RETURNED_TO_PROVIDER` | Petugas berwenang | Proses bisnis PMI mendukung — `OQ-BD-010` | Alasan, pelaku, waktu |
| `PENDING_REVIEW` | Nyatakan tidak layak | `NOT_USABLE` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib | Alasan, pelaku, waktu |

**Status akhir yang tidak dapat dibatalkan:** `Diberikan`, `RETURNED_TO_PROVIDER`, dan `NOT_USABLE`.
Darah yang sudah diberikan tidak dapat ditarik kembali oleh sistem. `DEC-BD-030` **tidak** mengubah
sifat itu: catatan koreksi tidak memindahkan kantong keluar dari status `Diberikan`, tidak
mengembalikannya menjadi tersedia, dan tidak membatalkan apa pun. Ia hanya menempel.

**Kondisi turunan, bukan status tersimpan.** Lewatnya masa berlaku bukti kecocokan **tidak**
memindahkan kantong ke status baru. Kantong tetap `Dialokasikan`; yang berubah hanyalah jawaban atas
pertanyaan gerbang ketika pemberian hendak dilakukan. Karena itu tidak ada baris "bukti kedaluwarsa"
pada tabel di atas, dan tidak ada pekerjaan latar belakang yang perlu berjalan — lihat
`ARCH-BD-POS-01`.

**Contoh gerbang yang tertutup kembali.** Kantong `PMI-00871` dialokasikan untuk Tn. S dan diuji
kecocokan Senin pukul 16.00. Bila masa berlaku dikonfigurasi 48 jam, pemberian Rabu pukul 10.00
berhasil. Bila pemberiannya baru dicoba Kamis pukul 09.00, gerbang menolak: bukti Senin tetap terbaca
pada riwayat, tetapi tidak lagi membukanya, dan petugas harus mencatat bukti baru.

**Contoh pengalihan yang menggugurkan bukti.** Kantong `PMI-00902` sudah diuji kecocokan untuk Tn. S,
lalu Tn. S pulang dan kantong masuk menunggu keputusan. Kantong dialihkan ke Ny. R. Bukti terhadap
Tn. S langsung berhenti membuka gerbang, walaupun golongan darah Tn. S dan Ny. R kebetulan sama —
sistem tidak pernah menilai hal itu (`INV-BD-011`, `INV-BD-013`). Riwayat menyimpan tiga hal
sekaligus: bukti lama milik Tn. S, alasan pelepasan, dan kewajiban bukti baru atas nama Ny. R.

**Contoh pembatalan alokasi.** Petugas mengalokasikan `PMI-00871` ke order Tn. S, lalu menyadari
kantong itu seharusnya untuk Ny. R. Ia membatalkan alokasi dengan alasan "salah pilih kantong".
Barisnya tidak hilang, hanya berhenti aktif, sehingga kantong boleh dialokasikan ulang. Bila pada saat
itu order Tn. S justru sudah berakhir, kantong tidak kembali menjadi tersedia melainkan masuk menunggu
keputusan — karena kantong tidak pernah boleh menjadi stok bebas (`DEC-BD-007`).

### G.4 Pemeriksaan Golongan Darah

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Catat pengambilan sampel | Sampel tercatat | Petugas pengambil | Pasien sah | Waktu, petugas, identifier sampel |
| Sampel tercatat | Catat hasil ABO dan Rhesus | Hasil tercatat | Pemeriksa | — | Pemeriksa, waktu |
| Hasil tercatat | Validasi | Hasil tervalidasi | **Peran validator — `DEF-BD-004`** | — | Validator, waktu |
| Hasil tervalidasi | Muncul hasil tervalidasi baru yang berbeda ABO atau Rhesus-nya | Perbedaan tertahan — `BD-DOM-21` bernilai kosong | Terjadi sebagai akibat, bukan tindakan orang | `BD-XINV-04` mendeteksi perbedaannya | Kedua hasil yang bertentangan, dan sejak kapan tertahan |
| Perbedaan tertahan | Selesaikan perbedaan | Satu golongan darah sah kembali berlaku | **Peran validator — `DEF-BD-004`** | **Mekanismenya belum ditetapkan — `ARCH-BD-GAP-07`** | Validator, waktu, alasan, dan hasil-hasil yang diperselisihkan |

**Apa yang sudah pasti dan apa yang belum.** Yang sudah pasti sepenuhnya: perbedaan terdeteksi, pemakaian tertahan, dan tidak ada hasil yang ditimpa. Yang belum: apakah menyelesaikan perbedaan berarti validator menyatakan salah satu hasil tidak sah, atau menuntut pemeriksaan ketiga sebagai penengah. `DEC-BD-026` tidak menyebutkan mekanismenya, dan dokumen ini tidak mengarangnya.

**Contoh.** Ny. R punya hasil tervalidasi O Positif dari kunjungan Januari. Pada kunjungan Mei, hasil tervalidasi baru menyatakan A Positif. Sejak saat itu `BD-DOM-21` bernilai kosong untuk Ny. R, dan alokasi maupun pemberian yang menuntut golongan darah sah ikut tertahan. Kedua hasil tetap terbaca penuh. Yang membuka kembali keadaan itu hanyalah tindakan validator, bukan waktu dan bukan hasil berikutnya.

---

## H. Tanggung jawab authorization

Bank Darah adalah **pemakai** model keamanan, bukan pemiliknya. Pola hak akses tingkat controller dan
tindakan sudah tersedia dan dipakai apa adanya (`BD-CAP-013`). Tidak ada model keamanan baru.

| Tanggung jawab | Siapa | Status |
| --- | --- | --- |
| Membuat order darah | Unit pelayanan yang **dikonfigurasi berwenang**; bawaan menolak | Ditetapkan `DEC-BD-012` |
| Membuat order manual | Petugas Bank Darah | Ditetapkan `DEC-BD-004` |
| Memproses permintaan, menerima kantong, mengalokasikan, memberikan | Petugas Bank Darah | Ditetapkan `DEC-BD-009` |
| Menyetujui alur normal | **Tidak ada** gerbang persetujuan | Ditetapkan `DEC-BD-009` |
| Menyatakan bukti kecocokan selesai | Petugas berwenang | Bentuknya ditetapkan `DEC-BD-013`; perannya `UNRESOLVED` |
| Memakai jalur darurat | Peran berwenang, kandidat Dokter BDRS | **`UNRESOLVED` — `DEF-BD-004`** |
| Memvalidasi hasil golongan darah | Peran validator | **`UNRESOLVED` — `DEF-BD-004`** |
| Menyelesaikan kantong menunggu keputusan | Petugas berwenang | Bentuknya ditetapkan `DEC-BD-019`; perannya `UNRESOLVED` |
| Membatalkan order | Pihak berwenang | Perannya `UNRESOLVED` |
| **Membatalkan alokasi kantong** | Petugas Bank Darah, tanpa peran tambahan | **Ditetapkan `DEC-BD-029`** — satu-satunya baris baru yang perannya sudah pasti |
| **Mencatat koreksi pencatatan pemberian** | Peran berwenang | **`UNRESOLVED` — `DEF-BD-004`** |
| **Menyelesaikan perbedaan hasil golongan darah** | Peran validator | **`UNRESOLVED` — `DEF-BD-004`** |

Tidak ada kebijakan peran yang dikarang di dokumen ini. Seluruh baris `UNRESOLVED` di atas dibawa
sebagai satu keputusan terkumpul, `DEF-BD-004`.

**Kenapa pembatalan alokasi tidak ikut ditunda.** Dua baris baru pada revisi 2 tetap `UNRESOLVED`
karena keduanya menyentuh kebenaran klinis: mengoreksi catatan pemberian dan memutuskan golongan darah
mana yang sah. Pembatalan alokasi berbeda — `DEC-BD-029` menyatakannya sebagai kekeliruan
administratif biasa, bukan tindakan klinis, sehingga wewenangnya sudah selesai dan tidak perlu
menunggu `DEF-BD-004`.

**Batas keamanan tambahan yang perlu diperhatikan.** Bank Darah memegang data pasien yang bersifat
sensitif. Nomor kantong berasal dari PMI dan bukan terbitan MMC, sehingga MMC tidak dapat menjamin
nomor itu bebas dari keterangan yang tidak diinginkan. Kantong dan sampel yang identifier-nya
diterbitkan sendiri wajib mengikuti pola yang sudah terbukti pada `BD-CAP-008`: tidak memuat nama
pasien, nomor rekam medis, tanggal lahir, maupun keterangan pribadi lainnya, dan bukan alat
otorisasi.

---

## I. Model audit dan histori

Yang wajib dapat ditelusuri kembali, mengikuti pola yang sudah terbukti berjalan (`BD-CAP-009`):

| Perubahan | Yang wajib tersimpan |
| --- | --- |
| Setiap perpindahan status order, permintaan, dan kantong | Status sebelum, status sesudah, pelaku, waktu, dan korelasi ke proses yang memicunya |
| Pembatalan order, pembatalan permintaan, dan penyelesaian kantong | Kode alasan **beserta salinan teksnya pada saat kejadian**, supaya penonaktifan alasan di kemudian hari tidak mengubah makna riwayat lama |
| Pemberian darah | Pelaku, waktu, kantong, pasien, order, dan rujukan bukti kecocokan |
| Pemberian lewat jalur darurat | Seluruh hal di atas, ditambah penanda permanen bahwa pemberian dilakukan sebelum bukti tercatat, dan alasannya |
| Pengalihan kantong ke pasien lain | Pasien asal, alasan pelepasan, dan pasien tujuan — rantai ini tidak pernah putus |
| Hasil golongan darah dan validasinya | Pemeriksa, waktu pemeriksaan, validator, waktu validasi |
| Perubahan data rujukan komponen dan alasan | Pelaku dan waktu |
| Penerimaan kantong yang melebihi jumlah diminta | Jumlah diminta pada saat itu, jumlah yang sudah diterima, kantong mana yang ditandai berlebih, dan alasan terkendalinya |
| Pembatalan alokasi | Alokasi mana yang berhenti aktif, alasan, pelaku, waktu, dan keadaan kantong sesudahnya |
| Gugurnya bukti kecocokan karena pengalihan | Bukti mana yang berhenti berlaku, terhadap pasien siapa bukti itu dibuat, dan pengalihan mana yang menggugurkannya |
| Koreksi pencatatan pemberian | Pemberian asal yang dituju, apa yang keliru, apa yang benar, alasan terkendali, pelaku, dan waktu — pemberian asalnya sendiri tidak boleh berubah sedikit pun |
| Deteksi dan penyelesaian perbedaan hasil golongan darah | Hasil-hasil yang bertentangan, sejak kapan tertahan, validator yang menyelesaikan, waktu, dan alasannya |

Riwayat pergerakan (`BD-DOM-15`) hanya dapat ditambah. Tidak ada satu pun jalur bisnis yang boleh
mengubah atau menghapus barisnya, termasuk saat terjadi pembatalan, pengalihan, atau koreksi.

**Tiga hal yang kini tegas bersifat hanya-tambah.** Revisi 2 memperluas sifat itu ke tiga konsep di
luar riwayat pergerakan: alokasi yang dibatalkan tetap tersimpan dan hanya berhenti aktif
(`BD-DOM-06`), catatan koreksi menempel pada pemberian tanpa mengubahnya (`BD-DOM-23`), dan
penyelesaian perbedaan hasil dicatat tanpa menghapus hasil mana pun (`BD-DOM-22`). Ketiganya memakai
kebiasaan yang sudah terbukti berjalan pada `BD-CAP-009`, bukan mekanisme baru.

**Kenapa ini penting bagi keselamatan, bukan sekadar kerapian.** Bila catatan pemberian dapat dihapus,
maka rekam yang dibaca kemudian akan menyatakan darah "belum diberikan" padahal secara fisik mungkin
sudah masuk ke tubuh pasien. Sifat hanya-tambah adalah cara arsitektur ini mencegah kebohongan yang
tidak disengaja.

---

## J. Model integrasi

| Batas | Penghasil | Pemakai | Sumber kebenaran | Arah | Sifat | Kepedulian pengulangan | Bila gagal |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Order elektronik dari unit pelayanan | `BD-CTX-02` dan unit pelayanan | Bank Darah | Unit pelayanan atas kebutuhan klinis | Masuk | Belum ditentukan bentuk teknisnya | Order ganda dicegah `BD-XINV-01` | Jalur manual tetap tersedia sebagai penyangga |
| Status kunjungan | `BD-CTX-02` dan `BD-CTX-06` | Bank Darah | Context hulu masing-masing | Baca saja | Dibaca saat dibutuhkan | — | Bila status tidak terbaca, order **tidak** boleh otomatis dianggap masih aktif maupun sudah berakhir; keadaannya dilaporkan apa adanya |
| Permintaan ke PMI | Bank Darah | PMI | PMI atas ketersediaan darah | Keluar, **lewat manusia** | Manual sepenuhnya | Tidak berlaku — tidak ada pengiriman otomatis | Tidak ada kegagalan teknis yang mungkin terjadi |
| Fakta biaya tindakan | Bank Darah | `BD-CTX-07` Billing | Billing atas akibat finansial | Keluar | Mengikuti pola yang sudah berjalan | Pengiriman ulang wajib dikenali sebagai kiriman ulang, bukan tagihan baru | **Belum dapat dirancang — `DEC-BD-016`** |
| Golongan darah pasien | Bank Darah | Bank Darah sendiri | Bank Darah, sesuai `DEC-BD-015` | Internal | — | — | — |
| HCLAB | — | — | — | — | **Tidak ada sambungan pada MVP** | — | — |

**Rekonsiliasi yang perlu dipikirkan pada tahap blueprint.** Karena permintaan ke PMI berjalan
manual, tidak ada mekanisme otomatis yang memastikan apa yang dicatat MMC sama dengan apa yang
dicatat PMI. Pencocokan itu dilakukan manusia. Arsitektur ini tidak mengarang mekanisme rekonsiliasi
apa pun.

---

## K. Dampak billing

**Klasifikasi: berdampak pada charge, dan dependency billing-nya belum terselesaikan.**

Yang sudah pasti: biaya berasal dari tindakan Bank Darah, bukan dari kantong. Beberapa kantong dalam
satu tindakan tidak menghasilkan beberapa tagihan. Tarif dimiliki Billing dan tidak pernah dihitung
Bank Darah.

Yang belum: penambahan konteks sumber Bank Darah pada kontrak Billing belum disetujui pemiliknya
(`DEC-BD-016`). Sampai itu turun, kejadian "tindakan selesai" boleh dirancang sebagai kejadian
domain, tetapi penyalurannya ke Billing tidak boleh dibekukan menjadi kontrak.

Tidak ada kebijakan tarif, penjamin, klaim, pengembalian dana, maupun pembalikan transaksi yang
disimpulkan sendiri di dokumen ini.

### Pemeriksaan ulang pada revisi 2

Keenam keputusan baru diperiksa terhadap batas biaya. **Lima di antaranya tidak menyentuh Billing sama
sekali**, karena biaya berasal dari tindakan dan bukan dari kantong (`DEC-BD-021`):

- `DEC-BD-025` kelebihan kiriman menambah kantong, bukan tindakan. Tidak ada tagihan tambahan.
- `DEC-BD-026` dan `DEC-BD-027` menahan gerbang. Tindakan yang tertahan berarti belum selesai,
  sehingga fakta biayanya memang belum lahir.
- `DEC-BD-028` mengalihkan kantong. Bila kelak diberikan, biayanya lahir dari tindakan pada pasien
  tujuan, bukan dari kantongnya.
- `DEC-BD-029` membatalkan alokasi sebelum pemberian. Tidak ada tindakan selesai, tidak ada biaya.

Yang menyisakan pertanyaan hanya `DEC-BD-030`. Koreksi nomor kantong jelas tidak mengubah biaya, karena
satu tindakan tetap satu tindakan berapa pun kantong di dalamnya. Tetapi bila sebuah koreksi menyatakan
bahwa **satu-satunya** pemberian di bawah sebuah tindakan tidak pernah terjadi, apakah fakta biaya yang
terlanjur terkirim perlu ditinjau ulang — itu kebijakan Billing, bukan kebijakan Bank Darah, dan
**tidak saya jawab sendiri**. Dicatat sebagai `ARCH-BD-GAP-09`.

---

## L. Dampak keselamatan klinis

**Klasifikasi: relevan terhadap keselamatan, dan sebagian keputusan keselamatannya belum
terselesaikan.**

Perpindahan yang kritis bagi keselamatan pasien:

| Perpindahan | Kenapa kritis | Batas yang sudah eksplisit |
| --- | --- | --- |
| Pemberian darah | Tidak dapat ditarik kembali | Wajib ada bukti kecocokan yang berlaku untuk pasien tujuan dan belum lewat masa berlakunya; sistem tidak menghitung kecocokan |
| Pemberian lewat jalur darurat | Darah keluar sebelum bukti ada | Hanya peran berwenang, alasan wajib, penanda permanen, bukti menyusul, dan wajib muncul pada daftar tunggakan |
| Pengalihan kantong ke pasien lain | Kantong berpindah tujuan pasien | Kelayakan dinyatakan manusia; rantai pasien asal ke pasien tujuan tidak pernah putus; **bukti kecocokan lama gugur otomatis** dan pasien tujuan wajib punya bukti sendiri |
| Pemakaian bukti kecocokan yang sudah lama | Kondisi pasien dan kantong bisa berubah sejak pemeriksaan | Bukti berhenti membuka gerbang setelah masa berlakunya lewat; selama nilainya belum dikonfigurasi, gerbang tertutup |
| Koreksi pencatatan pemberian | Rekam yang salah bisa menyesatkan pembaca berikutnya | Pemberian tidak pernah dihapus atau dibalik; koreksi hanya menempel; dilarang dipakai memindahkan pemberian ke pasien lain |
| Pemakaian golongan darah | Salah golongan berakibat fatal | Hanya hasil pemeriksaan tervalidasi yang sah; golongan darah pada permintaan dan pada data pendaftaran pasien dilarang dipakai untuk menilai kesesuaian |

**Ketiga pertanyaan keselamatan yang terbuka pada revisi 1 kini sudah dijawab.** Revisi 1 mencatat
tiga hal yang sengaja tidak dikarang: apakah bukti kecocokan punya masa berlaku, apakah pengalihan
menggugurkan bukti sebelumnya, dan hasil mana yang berlaku bila ada lebih dari satu hasil tervalidasi.
Ketiganya ditutup berturut-turut oleh `DEC-BD-027`, `DEC-BD-028`, dan `DEC-BD-026`, dan ketiga
jawabannya bersifat **fail-closed** — ketika ragu, sistem menahan, bukan meneruskan.

Satu keputusan keselamatan baru muncul dan langsung tertutup: `DEC-BD-030` memastikan rekam pemberian
tidak pernah dapat dihapus, sehingga tidak ada jalan bagi sistem untuk menyatakan darah "belum
diberikan" atas darah yang secara fisik sudah masuk ke tubuh pasien.

**Yang masih tersisa pada sisi keselamatan.** Dua hal, keduanya sempit dan keduanya menahan
implementasi, bukan rancangan: nilai jam masa berlaku bukti kecocokan belum ditetapkan pemilik klinis
(`OQ-BD-012`), dan peran yang berhak memakai jalur darurat, memvalidasi hasil, serta mencatat koreksi
belum ditetapkan (`DEF-BD-004`). Selama keduanya kosong, gerbang bersikap menolak — itu perilaku yang
disengaja, bukan kelalaian.

---

## M. Gap arsitektur

### Gap revisi 1 — seluruhnya sudah tertutup

Enam temuan yang dibuka revisi 1 dikembalikan ke `grill-me` dan ditutup pada architecture gap closure
pass 2 September 2026. Tidak satu pun dijawab sendiri oleh dokumen ini.

| ID | Temuan | Ditutup oleh | Bagaimana arsitektur menyerapnya |
| --- | --- | --- | --- |
| `ARCH-BD-GAP-01` | Kelebihan kiriman dari PMI | `DEC-BD-025` | Invariant `BD-AGG-02` diperbaiki; lahir `BD-XINV-03` |
| `ARCH-BD-GAP-02` | Hasil golongan darah mana yang berlaku | `DEC-BD-026` | Lahir `BD-DOM-21`, `BD-DOM-22`, dan `BD-XINV-04` |
| `ARCH-BD-GAP-03` | Masa berlaku bukti kecocokan | `DEC-BD-027` | Aturan identitas `BD-DOM-07` diperluas; lahir `ARCH-BD-POS-01` |
| `ARCH-BD-GAP-04` | Gugurnya bukti saat pengalihan | `DEC-BD-028` | `BD-DOM-07` kini terikat pasangan kantong dan pasien; lahir `ARCH-BD-POS-02` |
| `ARCH-BD-GAP-05` | Pembatalan alokasi sebelum pemberian | `DEC-BD-029` | Satu perpindahan baru pada `BD-AGG-03`; lahir `ARCH-BD-POS-03` |
| `ARCH-BD-GAP-06` | Koreksi pencatatan pemberian | `DEC-BD-030` | Lahir `BD-DOM-23` di dalam batas `BD-AGG-03` |

### Gap baru yang lahir dari revisi 2

Tiga temuan berikut muncul saat menyerap keputusan-keputusan itu, dan **belum pernah ditanyakan**
kepada pemilik mana pun. Tidak satu pun saya jawab sendiri.

| ID | Temuan | Kenapa penting | Pemilik | Dampak |
| --- | --- | --- | --- | --- |
| `ARCH-BD-GAP-07` | Apa artinya "menyelesaikan perbedaan" hasil golongan darah? Apakah validator menyatakan salah satu hasil tidak sah, atau wajib ada pemeriksaan ketiga sebagai penengah? | `DEC-BD-026` menetapkan bahwa perbedaan ditahan dan diselesaikan validator, tetapi tidak menyebutkan caranya. Tanpa ini, satu perpindahan pada `BD-DOM-22` tidak punya prasyarat yang dapat diuji | Pemilik proses klinis | Menahan **satu perpindahan** pada `BD-AGG-04`. Deteksi dan penahanannya sendiri sudah lengkap dan tetap dapat dirancang |
| `ARCH-BD-GAP-08` | Di mana nilai masa berlaku bukti kecocokan disimpan? | `DEC-BD-027` menyebut nilainya konfigurasi, sedangkan `DEC-BD-024` sudah mengunci Setup MVP pada tepat dua hal. Bila nilainya per komponen, ia menumpang pada `BD-DOM-13` dan Setup **tidak** melebar. Bila nilainya satu angka global, Setup melebar melampaui `DEC-BD-024` dan itu perlu persetujuan | Pemilik proses klinis, bersama pemilik proses BDRS | Menahan pembekuan kumpulan atribut `BD-DOM-13`. Terikat langsung pada `OQ-BD-012` — kedua pertanyaan sebaiknya dijawab bersamaan |
| `ARCH-BD-GAP-09` | Bila koreksi menyatakan bahwa satu-satunya pemberian di bawah sebuah tindakan tidak pernah terjadi, apakah fakta biaya yang terlanjur terkirim perlu ditinjau ulang? | `DEC-BD-021` menetapkan biaya berasal dari tindakan. `DEC-BD-030` memungkinkan pemberian dinyatakan salah catat. Pertemuan keduanya belum pernah dibahas, dan kebijakan pembalikan biaya milik Billing | Pemilik BillingManagement | Menambah satu pertanyaan pada kontrak yang memang sudah tertahan `DEC-BD-016`. Tidak menahan apa pun yang baru |

Decision ID pemblokir yang sudah ada dan tetap berlaku: `DEC-BD-016` kontrak sumber biaya ·
`OQ-BD-011` mekanik label · `DEF-BD-003` bukti kecocokan per komponen · `DEF-BD-004` peran jalur
darurat, peran validator, dan peran pencatat koreksi · `OQ-BD-010` kesediaan PMI menerima
pengembalian · `OQ-BD-012` nilai jam masa berlaku · `OQ-BD-013` tempat penyelesaian perbedaan hasil
· `OQ-BD-014` keadaan kantong setelah koreksi.

**Perbandingan dengan revisi 1.** Enam gap tertutup, tiga gap baru lahir, dan bobotnya turun jauh.
Gap revisi 1 menahan **dua slice utuh** — jalur pemberian dan jalur pengalihan — dan dua di
antaranya menyangkut keselamatan pasien. Ketiga gap baru tidak menahan satu slice pun secara utuh:
`ARCH-BD-GAP-07` menahan satu perpindahan, `ARCH-BD-GAP-08` menahan satu kumpulan atribut, dan
`ARCH-BD-GAP-09` menempel pada kontrak yang sejak awal memang sudah tertahan.

---

## N. Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_PARTIAL`**

Status keseluruhannya tetap `PARTIAL`, tetapi isinya berubah besar. Pada revisi 1, yang berhenti
adalah **dua slice utuh yang menyangkut keselamatan pasien**. Pada revisi 2, keduanya berjalan, dan
yang tersisa hanya potongan-potongan sempit.

### Yang boleh diserahkan ke penyusunan blueprint

| Slice arsitektur | Isi | Alasan boleh jalan |
| --- | --- | --- |
| `BD-AGG-01` Order Darah | Order, baris kebutuhan, lifecycle lengkap termasuk kedaluwarsa | Kepemilikan jelas, invariant terwakili, seluruh keputusan bisnisnya sudah turun |
| `BD-AGG-02` Permintaan Darah ke PMI | Permintaan, penerimaan, kelebihan kiriman, lifecycle sampai penutupan administratif | Aturan kelebihan kiriman **kini tertutup** `DEC-BD-025`; invariant sisa dan `BD-XINV-03` sudah dapat diuji |
| `BD-AGG-03` Kantong Darah Operasional | Penerimaan, alokasi, **pembatalan alokasi**, bukti kecocokan beserta masa berlakunya, **jalur pemberian**, jalur darurat, **jalur pengalihan**, **catatan koreksi**, keadaan menunggu keputusan, pengembalian ke PMI, dan penetapan tidak layak | Empat keputusan yang menahannya — `DEC-BD-027`, `DEC-BD-028`, `DEC-BD-029`, `DEC-BD-030` — sudah turun. Gerbang pemberiannya dapat dinyatakan sebagai satu pertanyaan yang dapat diuji (`ARCH-BD-POS-02`) |
| `BD-AGG-04` Pemeriksaan Golongan Darah | Sampel, pencatatan hasil, validasi, **deteksi hasil bertentangan, dan penahanannya** | `DEC-BD-026` menutup aturan hasil mana yang berlaku. Deteksi dan penahanannya lengkap; hanya cara melepas penahanan yang tertunda |
| `BD-AGG-05` Tindakan Bank Darah | Pencatatan tindakan beserta konteks dan rujukan tarifnya | Dinyatakan berdiri sendiri dari bagian penyerahan biaya. Tidak tersentuh keenam keputusan baru |
| `BD-DOM-14` | Daftar alasan terkendali | Ditetapkan penuh `DEC-BD-024`. Bertambah kode alasan untuk kelebihan kiriman, pembatalan alokasi, dan koreksi pemberian — penambahan isi daftar, bukan perubahan bentuk |
| `BD-DOM-16`, `BD-DOM-17`, `BD-DOM-21` | Pembaca status kunjungan, ringkasan pemenuhan, dan golongan darah sah pasien | Ketiganya turunan dan tidak menyimpan keadaan sendiri. `BD-DOM-17` kini menghormati catatan koreksi |
| `BD-DOM-18` | Titipan tanda kewenangan unit memesan darah | Ditetapkan penuh `DEC-BD-012`; menunggu pelaksanaan pemilik data induk |
| `BD-DOM-22`, `BD-DOM-23` | Penyelesaian perbedaan hasil dan catatan koreksi pemberian, **sebagai konsep** | Identitas, kepemilikan, sifat hanya-tambah, dan kebutuhan auditnya sudah pasti |

### Yang harus berhenti

| Yang berhenti | Yang menahan | Seberapa luas |
| --- | --- | --- |
| `BD-AGG-04` — **satu perpindahan saja**: prasyarat melepas penahanan hasil bertentangan | `ARCH-BD-GAP-07` | Satu baris pada tabel lifecycle G.4. Sisa `BD-AGG-04` jalan |
| `BD-DOM-13` — **pembekuan kumpulan atributnya**, bukan konsepnya | `ARCH-BD-GAP-08` bersama `OQ-BD-012` | Katalog komponen darah tetap boleh dirancang; yang ditunda hanya apakah ia memikul atribut masa berlaku |
| Penyerahan fakta biaya ke Billing | `DEC-BD-016`, kini ditambah `ARCH-BD-GAP-09` | Sudah di luar scope sesi ini sejak revisi 1. Bukan temuan baru |
| Mekanik label golongan darah | `OQ-BD-011` | Sudah di luar scope sesi ini sejak revisi 1. Bukan temuan baru |
| Penetapan peran pada jalur darurat, validasi hasil, pencatatan koreksi, penyelesaian kantong, dan pembatalan order | `DEF-BD-004` | Menahan `IMPLEMENTATION`, **bukan** rancangan. Bentuk setiap alurnya sudah pasti |
| Nilai jam masa berlaku bukti kecocokan | `OQ-BD-012` | Menahan `IMPLEMENTATION`. Rancangannya cukup tahu bahwa nilainya datang dari konfigurasi |
| Tempat penyelesaian perbedaan hasil pada layar | `OQ-BD-013` | Urusan frontend, di luar dokumen ini. Dicatat agar tidak hilang |
| Keadaan kantong yang tercatat keliru sesudah dikoreksi | `OQ-BD-014` | Menahan `IMPLEMENTATION` jalur koreksi. Konsep `BD-DOM-23` tetap dapat dirancang |

**Kenapa masih `PARTIAL` dan bukan `READY`.** Dua alasan, dan keduanya jujur. Pertama, `ARCH-BD-GAP-07`
membuat satu perpindahan pada `BD-AGG-04` tidak punya prasyarat yang dapat diuji — dan kontrak
melarang menyerahkan slice yang prasyaratnya masih harus dikarang. Kedua, `ARCH-BD-GAP-08` membuat
kumpulan atribut `BD-DOM-13` belum boleh dibekukan, padahal `design-business-module` justru bertugas
membekukan kontrak. Keduanya sempit dan cepat ditutup, tetapi keduanya nyata.

**Bagian yang sengaja tetap boleh jalan.** Sembilan baris pada tabel pertama tidak menunggu apa pun,
termasuk `BD-AGG-03` yang pada revisi 1 berhenti separuh. Slice-slice itu memenuhi syarat "slice
arsitektur siap yang berdiri sendiri" dan boleh diserahkan lebih dulu.

---

## O. Handoff

```yaml
blueprint_id: BD-BP-001
blueprint_revision: 5
domain_architecture_revision: 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_PARTIAL
domain_architecture_scope:
  siap:
    - BD-AGG-01
    - BD-AGG-02 (termasuk kelebihan kiriman)
    - BD-AGG-03 (penerimaan, alokasi, pembatalan alokasi, bukti kecocokan beserta masa berlakunya, pemberian, jalur darurat, pengalihan, catatan koreksi, menunggu keputusan, kembali ke PMI, tidak layak)
    - BD-AGG-04 (sampel, pencatatan hasil, validasi, deteksi dan penahanan hasil bertentangan)
    - BD-AGG-05 (pencatatan tindakan saja)
    - BD-DOM-14
    - BD-DOM-16
    - BD-DOM-17
    - BD-DOM-18
    - BD-DOM-21
    - BD-DOM-22 (sebagai konsep)
    - BD-DOM-23 (sebagai konsep)
  berhenti:
    - BD-AGG-04 (prasyarat perpindahan yang melepas penahanan hasil bertentangan)
    - BD-DOM-13 (pembekuan kumpulan atribut; konsepnya sendiri siap)
    - penyerahan fakta biaya ke Billing
    - mekanik label golongan darah
architecture_positions: [ARCH-BD-POS-01, ARCH-BD-POS-02, ARCH-BD-POS-03]
cross_aggregate_invariants: [BD-XINV-01, BD-XINV-02, BD-XINV-03, BD-XINV-04]
requirement_readiness: PARTIALLY_READY
requirement_evidence_status: seluruh slice yang masuk CONFIRMED; tidak ada CONFLICT tersisa
capability_scope: [BD-SLICE-01, BD-SLICE-02, BD-SLICE-03, BD-SLICE-04, BD-SLICE-05, BD-SLICE-06, BD-SLICE-07, BD-SLICE-08, BD-SLICE-09, BD-SLICE-10]
blocking_decision_ids: [DEC-BD-016, OQ-BD-011, DEF-BD-003, DEF-BD-004, OQ-BD-010, OQ-BD-012, OQ-BD-013, OQ-BD-014, ARCH-BD-GAP-07, ARCH-BD-GAP-08, ARCH-BD-GAP-09]
closed_gap_ids: [ARCH-BD-GAP-01, ARCH-BD-GAP-02, ARCH-BD-GAP-03, ARCH-BD-GAP-04, ARCH-BD-GAP-05, ARCH-BD-GAP-06]
dependency_ids: [BD-DEP-001, BD-DEP-002, BD-DEP-003, BD-DEP-004, BD-DEP-005, BD-DEP-006, BD-DEP-007, BD-DEP-008, BD-DEP-010, BD-DEP-011, BD-DEP-012, BD-DEP-013, BD-DEP-014]
decision_revision: 3
backend_source_sha: 9dc7637adbafb321ad8078d5c52ebe5e4398fe86
frontend_source_sha: afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254
baseline_reference_coverage: NOT_YET_AVAILABLE
contract_versions: []
```

### Jejak dari kebutuhan ke domain

| Kebutuhan | Keputusan | Konsep domain |
| --- | --- | --- |
| BR-BD-001..003, BR-BD-010, BR-BD-019 | `DEC-BD-004`, `DEC-BD-005`, `DEC-BD-006`, `DEC-BD-014` | `BD-AGG-01`, `BD-DOM-16`, `BD-XINV-01` |
| BR-BD-017 | `DEC-BD-002`, `DEC-BD-003`, `DEC-BD-008`, `DEC-BD-020` | `BD-AGG-02`, `BD-XINV-02` |
| BR-BD-018 | `DEC-BD-002` | `BD-DOM-04`, `BD-DOM-05` |
| BR-BD-005, BR-BD-006 | `DEC-BD-003` | `BD-AGG-03`, `BD-DOM-06` |
| BR-BD-007, BR-BD-008 | `DEC-BD-013`, `DEC-BD-017` | `BD-DOM-07`, `BD-DOM-08`, `BD-DOM-17` |
| BR-BD-009 | `DEC-BD-007`, `DEC-BD-019` | Jalur penyelesaian pada `BD-AGG-03` |
| BR-BD-004 | `DEC-BD-021` | `BD-AGG-05`, `BD-DOM-19` |
| BR-BD-011 | `DEC-BD-015` | `BD-AGG-04`, `BD-DOM-11` |
| BR-BD-012, BR-BD-013 | `DEC-BD-018`, `DEC-BD-015` | `BD-DOM-10`, batas `BD-CTX-09` |
| BR-BD-014 | `DEC-BD-022` | Tidak ada konsep domain — sengaja |
| BR-BD-015 | `DEC-BD-023` | Tidak ada konsep domain; hanya tiga daftar kerja |
| BR-BD-016 | `DEC-BD-024` | `BD-DOM-13`, `BD-DOM-14` |
| BR-BD-017 — kelebihan kiriman | `DEC-BD-025` | `BD-AGG-02`, `BD-XINV-03` |
| BR-BD-011 — hasil mana yang sah | `DEC-BD-026` | `BD-DOM-21`, `BD-DOM-22`, `BD-XINV-04` |
| BR-BD-007 — gerbang pemberian | `DEC-BD-027`, `DEC-BD-028` | `BD-DOM-07`, `ARCH-BD-POS-01`, `ARCH-BD-POS-02` |
| BR-BD-006 — koreksi alokasi | `DEC-BD-029` | `BD-DOM-06`, `ARCH-BD-POS-03` |
| BR-BD-007 — koreksi pemberian | `DEC-BD-030` | `BD-DOM-23`, `BD-DOM-17` |

### Langkah berikutnya

Sembilan baris pada tabel "yang boleh diserahkan" siap dikirim ke `design-business-module`, termasuk
`BD-AGG-03` yang pada revisi 1 berhenti separuh.

Tiga gap baru (`ARCH-BD-GAP-07`, `ARCH-BD-GAP-08`, `ARCH-BD-GAP-09`) dikembalikan ke `grill-me`,
karena ketiganya keputusan bisnis dan klinis yang bergantung pemilik, bukan pertanyaan tentang apa
yang sudah ada di sistem. Sebaiknya digabung dengan `OQ-BD-012` dan `OQ-BD-013` yang lahir dari pass
sebelumnya — `ARCH-BD-GAP-08` dan `OQ-BD-012` bahkan sebaiknya dijawab dalam satu tarikan, karena
keduanya menanyakan hal yang sama dari dua sisi: berapa nilainya, dan di mana ia disimpan.

Yang **tidak** perlu diulang: `trace-existing-capabilities`. Peta kemampuan masih sahih karena tidak
ada berkas source aplikasi yang berubah, dan keenam keputusan baru tidak memunculkan kebutuhan bukti
implementasi yang belum ditelusuri. Pola yang dipakai revisi 2 — token konkurensi `BD-CAP-010` dan
riwayat hanya-tambah `BD-CAP-009` — keduanya sudah tercatat pada audit yang ada.
