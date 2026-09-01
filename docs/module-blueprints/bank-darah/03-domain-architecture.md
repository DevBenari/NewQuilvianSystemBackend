# Bank Darah — Hospital Domain Architecture

## A. Identitas arsitektur

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Blueprint revision | `3` |
| Domain architecture revision | `1` |
| Modul | Bank Darah (`bank-darah`) |
| Tanggal | `2026-09-02` |
| Backend SHA | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Kesiapan requirement | `PARTIALLY_READY` — `02-requirement-completeness-assessment.md` revisi 2 |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_PARTIAL`** |
| Baseline rujukan Indonesia | Tidak dipakai. Tidak ada `baseline_observation_ids` maupun `baseline_source_ids`. |

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
label golongan darah (terhalang `OQ-BD-011`).

### Bukti dan Decision ID yang dipertahankan

`00-interview-decisions.md` revisi 2 — `SCOPE-BD-001`, `DEC-BD-001` sampai `DEC-BD-024`,
`INV-BD-011` sampai `INV-BD-016`, `ASM-BD-001` sampai `ASM-BD-007`, `DEF-BD-003`, `DEF-BD-004` ·
`00-business-overview.md` revisi 1 · `02-existing-capability-map.md` revisi 2 — `BD-CAP-001` sampai
`BD-CAP-024` · `02-requirement-completeness-assessment.md` revisi 2 · `01-prerequisite-readiness.md`
revisi 3 — `BD-DEP-001` sampai `BD-DEP-015`.

Decision ID pemblokir yang dibawa: `DEC-BD-016`, `OQ-BD-011`, `DEF-BD-003`, `DEF-BD-004`,
`OQ-BD-010`.

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

---

## D. Katalog konsep domain

Klasifikasi menggambarkan tanggung jawab domain, bukan bentuk tabel database.

| ID | Nama bisnis | Klasifikasi | Pemilik | Identitas | Peran lifecycle | Invariant penting | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BD-DOM-01` | Order Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor order terbitan sistem | Aktif → terpenuhi sebagian → terpenuhi penuh, dibatalkan, atau kedaluwarsa | Wajib menunjuk pasien dan kunjungan yang sah; jumlah pemenuhan dihitung dari transaksi | `DEC-BD-004`, `DEC-BD-005`, `DEC-BD-006` |
| `BD-DOM-02` | Baris Kebutuhan Order | `ENTITY` di dalam `BD-DOM-01` | Bank Darah — `New` | Nomor urut dalam order | Menyimpan komponen dan jumlah yang diminta | Jumlah diminta lebih dari nol; komponen wajib dari katalog | `DEC-BD-011`, `DEC-BD-024` |
| `BD-DOM-03` | Permintaan Darah ke PMI | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor permintaan terbitan sistem | `REQUESTED` → `PARTIALLY_FULFILLED` → `FULFILLED`, `CANCELLED`, atau `CLOSED_ENCOUNTER` | Selalu atas nama satu pasien; sisa dihitung dari penerimaan nyata | `DEC-BD-003`, `DEC-BD-008`, `DEC-BD-020` |
| `BD-DOM-04` | Penerimaan Kantong | `ENTITY` di dalam `BD-DOM-03` | Bank Darah — `New` | Nomor urut penerimaan | Mencatat kedatangan fisik | Stok bertambah **hanya** lewat konsep ini | `DEC-BD-002` |
| `BD-DOM-05` | Kantong Darah Operasional | `AGGREGATE_ROOT` | Bank Darah — `New` | Identitas internal, dengan **nomor kantong terbitan PMI** sebagai identifier bisnis yang unik | Tersedia → dialokasikan → diberikan, atau menunggu keputusan → dialihkan, dikembalikan, tidak layak | Tidak pernah menjadi stok bebas; tidak boleh punya lebih dari satu alokasi aktif | `DEC-BD-001`, `DEC-BD-007`, `DEC-BD-019`, `ASM-BD-003` |
| `BD-DOM-06` | Alokasi | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Mengikat kantong pada satu baris kebutuhan order | Satu kantong hanya boleh punya satu alokasi aktif | `DEC-BD-003`, BG-BD-002 |
| `BD-DOM-07` | Bukti Kecocokan | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Gerbang sebelum pemberian | Wajib ada sebelum pemberian, kecuali lewat jalur darurat | `DEC-BD-013`, `INV-BD-012` |
| `BD-DOM-08` | Otorisasi Darurat | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Menggantikan gerbang bukti kecocokan pada keadaan darurat | Hanya oleh peran berwenang; alasan wajib; penanda melekat permanen | `DEC-BD-017` |
| `BD-DOM-09` | Pemeriksaan Golongan Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Identitas internal | Sampel diambil → hasil dicatat → hasil tervalidasi | Hasil belum tervalidasi tidak boleh dipakai untuk keperluan klinis | `DEC-BD-015`, `INV-BD-014` |
| `BD-DOM-10` | Sampel Bank Darah | `ENTITY` di dalam `BD-DOM-09` | Bank Darah — `New` | Identifier sampel terbitan sistem | Menjaga penelusuran dari pasien ke hasil | Bukan sampel Laboratorium; tidak menimbulkan tagihan Laboratorium | `DEC-BD-018` |
| `BD-DOM-11` | Golongan Darah dan Rhesus | `VALUE_OBJECT` | Platform — `Existing` | — | Dipakai pada permintaan dan pada hasil pemeriksaan | Nilai yang sama dipakai di kedua tempat, **tetapi maknanya berbeda** | `BD-CAP-016` |
| `BD-DOM-12` | Tindakan Bank Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor tindakan terbitan sistem | Dicatat → selesai | Menunjuk order, unit, dokter BDRS, petugas, kelas, dan tindakan bertarif | `DEC-BD-021`, BR-BD-004 |
| `BD-DOM-13` | Katalog Komponen Darah | `REFERENCE_DATA` | Bank Darah — `New` | Kode komponen | Dipakai order, permintaan, dan deteksi ganda | Komponen tidak boleh berupa ketikan bebas | `DEC-BD-024`, `DEC-BD-005` |
| `BD-DOM-14` | Daftar Alasan Terkendali | `REFERENCE_DATA` | Bank Darah — `New` | Kode alasan | Dipakai pembatalan, jalur darurat, dan penyelesaian kantong | Alasan tidak boleh teks bebas semata; perubahan wajib berjejak | `DEC-BD-024`, `INV-BD-016` |
| `BD-DOM-15` | Riwayat Pergerakan | `DOMAIN_EVENT` tersimpan | Bank Darah — `New` | Identitas internal | Merekam setiap perpindahan status yang berarti | Hanya bisa ditambah; tidak pernah diubah atau dihapus | `BD-CAP-009`, BG-BD-004 |
| `BD-DOM-16` | Pembaca Status Kunjungan | `Adapter/View` | Bank Darah membaca `BD-CTX-02` dan `BD-CTX-06` | — | Menjawab satu pertanyaan: apakah kunjungan ini sudah berakhir | Tidak pernah mengubah data hulu | `DEC-BD-014` |
| `BD-DOM-17` | Ringkasan Pemenuhan | `Adapter/View` | Bank Darah — turunan | — | Menyajikan jumlah diminta, diberikan, dan belum diberikan | Dihitung dari transaksi, **bukan** kolom yang bisa disunting | BG-BD-003, INV-BD-003 |
| `BD-DOM-18` | Kewenangan Unit Memesan Darah | `Extend` pada `BD-CTX-05` | Data Induk Layanan | — | Menentukan unit mana yang boleh membuat order | Bawaan menolak; tidak dikunci di kode | `DEC-BD-012`, `BD-CAP-005` |
| `BD-DOM-19` | Kontrak Sumber Biaya Bank Darah | `Extend` pada `BD-CTX-07` | Billing | — | Menerima fakta tindakan selesai | **Belum disetujui** — `DEC-BD-016` | `BD-CAP-015` |
| `BD-DOM-20` | Pasien, Kunjungan, Dokter, Unit, Kelas | `Existing` | Context hulu masing-masing | — | Rujukan | Tidak pernah diduplikasi ke Bank Darah | `BD-CAP-001`, `002`, `004`, `006` |

Tidak ada satu pun konsep di atas yang diturunkan dari nama menu, nama layar, atau nama task.
Sebagai contoh, menu `Laporan` dan menu `Setup` pada bukti navigasi **tidak** melahirkan konsep
domain bernama Laporan atau Setup: yang lahir hanyalah dua data rujukan pada `BD-DOM-13` dan
`BD-DOM-14`, karena keduanya memang dituntut keputusan bisnis, bukan dituntut oleh nama menunya.

---

## E. Model aggregate

Aggregate dipakai hanya ketika ada batas konsistensi yang harus melindungi invariant. Empat aggregate
berikut lahir dari empat lifecycle yang benar-benar berbeda.

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
| Invariant yang dilindungi | Selalu atas nama satu pasien · jumlah sisa selalu sama dengan jumlah diminta dikurangi jumlah yang benar-benar diterima · permintaan yang belum selesai tidak boleh digandakan untuk kebutuhan yang sama |
| Tindakan bisnis | Buat permintaan · catat penerimaan kantong · batalkan permintaan · tutup karena kunjungan berakhir |
| Kejadian yang diterbitkan | Permintaan dibuat · kantong diterima · permintaan terpenuhi · permintaan ditutup karena kunjungan berakhir |

### `BD-AGG-03` — Kantong Darah Operasional

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-05` Kantong Darah |
| Batas | Kantong beserta alokasinya (`BD-DOM-06`), bukti kecocokannya (`BD-DOM-07`), dan otorisasi daruratnya (`BD-DOM-08`) |
| Invariant yang dilindungi | **Satu kantong tidak boleh punya lebih dari satu alokasi aktif** · kantong tidak pernah menjadi stok bebas · kantong tidak dapat diberikan tanpa bukti kecocokan atau otorisasi darurat · kantong yang ordernya berakhir tidak dapat dialokasikan ke pasien lain sebelum diselesaikan |
| Tindakan bisnis | Alokasikan ke baris kebutuhan · catat bukti kecocokan · berikan · berikan lewat jalur darurat · tandai menunggu keputusan · alihkan ke pasien lain · kembalikan ke PMI · nyatakan tidak layak |
| Kejadian yang diterbitkan | Kantong diterima · dialokasikan · bukti kecocokan tercatat · diberikan · diberikan darurat · masuk menunggu keputusan · dialihkan · dikembalikan · dinyatakan tidak layak |

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

### `BD-AGG-04` — Pemeriksaan Golongan Darah

| Aspek | Isi |
| --- | --- |
| Root | `BD-DOM-09` Pemeriksaan Golongan Darah |
| Batas | Pemeriksaan beserta sampelnya (`BD-DOM-10`) dan status validasinya |
| Invariant yang dilindungi | Hasil yang belum tervalidasi tidak boleh dipakai untuk keperluan klinis · hasil wajib menyimpan pemeriksa dan waktu pemeriksaan · sampel bersifat opsional, tetapi bila ada wajib menyimpan waktu dan petugas pengambil |
| Tindakan bisnis | Catat pengambilan sampel · catat hasil ABO dan Rhesus · validasi hasil |
| Kejadian yang diterbitkan | Sampel diambil · hasil dicatat · hasil tervalidasi |

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

Contoh mengapa ini penting: perawat IGD memesan elektronik pada saat yang hampir bersamaan dengan
petugas Bank Darah yang menginput formulir kertas untuk kebutuhan yang sama. Keduanya tidak saling
melihat, dan tidak ada satu pun order yang bisa "menahan" yang lain dari dalam dirinya sendiri.

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

**Contoh berangka.** Diminta 3 kantong PRC untuk Tn. S. Hari pertama datang 2, status menjadi
`PARTIALLY_FULFILLED` dengan sisa 1. Tn. S pulang Senin siang, permintaan menjadi `CLOSED_ENCOUNTER`.
Selasa pagi kantong sisanya tetap diantar. Penerimaan **tetap dicatat**, kantong tetap membawa jejak
permintaan asalnya, lalu langsung masuk keadaan menunggu keputusan.

### G.3 Kantong Darah Operasional

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Diterima secara fisik | Tersedia | Petugas Bank Darah | Terikat permintaan asal | Penerimaan |
| Tersedia | Alokasikan | Dialokasikan | Petugas Bank Darah | Order aktif; tidak ada alokasi aktif lain pada kantong ini | Alokasi, pelaku, waktu |
| Dialokasikan | Catat bukti kecocokan | Dialokasikan, bukti lengkap | Petugas berwenang | — | Status pemeriksaan, pelaku, waktu |
| Dialokasikan, bukti lengkap | Berikan | Diberikan | Petugas Bank Darah | Bukti kecocokan ada | Pemberian |
| Dialokasikan | Berikan lewat jalur darurat | Diberikan, **ditandai tanpa bukti** | **Peran berwenang — `DEF-BD-004`** | Alasan wajib dari daftar terkendali | Otorisasi darurat lengkap dengan penandanya |
| Tersedia atau dialokasikan | Order berakhir | `PENDING_REVIEW` | Sistem | Order kedaluwarsa atau dibatalkan | Perubahan status |
| `PENDING_REVIEW` | Alihkan ke pasien lain | `REALLOCATED` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib | Pasien asal, alasan pelepasan, pasien tujuan |
| `PENDING_REVIEW` | Kembalikan ke PMI | `RETURNED_TO_PROVIDER` | Petugas berwenang | Proses bisnis PMI mendukung — `OQ-BD-010` | Alasan, pelaku, waktu |
| `PENDING_REVIEW` | Nyatakan tidak layak | `NOT_USABLE` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib | Alasan, pelaku, waktu |

**Status akhir yang tidak dapat dibatalkan:** `Diberikan`, `RETURNED_TO_PROVIDER`, dan `NOT_USABLE`.
Darah yang sudah diberikan tidak dapat ditarik kembali oleh sistem. Koreksi atas kekeliruan
pencatatan pemberian belum diatur siapa pun, dan dicatat sebagai gap arsitektur.

### G.4 Pemeriksaan Golongan Darah

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Catat pengambilan sampel | Sampel tercatat | Petugas pengambil | Pasien sah | Waktu, petugas, identifier sampel |
| Sampel tercatat | Catat hasil ABO dan Rhesus | Hasil tercatat | Pemeriksa | — | Pemeriksa, waktu |
| Hasil tercatat | Validasi | Hasil tervalidasi | **Peran validator — `DEF-BD-004`** | — | Validator, waktu |

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

Tidak ada kebijakan peran yang dikarang di dokumen ini. Seluruh baris `UNRESOLVED` di atas dibawa
sebagai satu keputusan terkumpul, `DEF-BD-004`.

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

Riwayat pergerakan (`BD-DOM-15`) hanya dapat ditambah. Tidak ada satu pun jalur bisnis yang boleh
mengubah atau menghapus barisnya, termasuk saat terjadi pembatalan atau pengalihan.

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

---

## L. Dampak keselamatan klinis

**Klasifikasi: relevan terhadap keselamatan, dan sebagian keputusan keselamatannya belum
terselesaikan.**

Perpindahan yang kritis bagi keselamatan pasien:

| Perpindahan | Kenapa kritis | Batas yang sudah eksplisit |
| --- | --- | --- |
| Pemberian darah | Tidak dapat ditarik kembali | Wajib ada bukti kecocokan; sistem tidak menghitung kecocokan |
| Pemberian lewat jalur darurat | Darah keluar sebelum bukti ada | Hanya peran berwenang, alasan wajib, penanda permanen, bukti menyusul, dan wajib muncul pada daftar tunggakan |
| Pengalihan kantong ke pasien lain | Kantong berpindah tujuan pasien | Kelayakan dinyatakan manusia; rantai pasien asal ke pasien tujuan tidak pernah putus |
| Pemakaian golongan darah | Salah golongan berakibat fatal | Hanya hasil pemeriksaan tervalidasi yang sah; golongan darah pada permintaan dan pada data pendaftaran pasien dilarang dipakai untuk menilai kesesuaian |

Empat batas di atas sudah eksplisit. Tiga pertanyaan keselamatan berikut **belum ada jawabannya**
dan tidak saya karang: apakah bukti kecocokan punya masa berlaku, apakah pengalihan kantong ke pasien
lain menggugurkan bukti kecocokan sebelumnya, dan hasil mana yang berlaku bila seorang pasien punya
lebih dari satu hasil golongan darah tervalidasi. Ketiganya ada pada bagian gap.

---

## M. Gap arsitektur

Enam temuan berikut muncul dari penyusunan arsitektur ini dan **belum pernah ditanyakan** kepada
pemilik mana pun. Tidak satu pun saya jawab sendiri.

| ID | Temuan | Kenapa penting | Pemilik | Dampak |
| --- | --- | --- | --- | --- |
| `ARCH-BD-GAP-01` | Bila PMI mengirim **lebih banyak** dari yang diminta, apa yang terjadi? | `DEC-BD-008` hanya mengatur kekurangan. Kelebihan kiriman membuat jumlah sisa menjadi angka negatif, dan kantong berlebih tidak punya tujuan | Pemilik proses BDRS | Menahan penetapan invariant pada `BD-AGG-02` |
| `ARCH-BD-GAP-02` | Bila seorang pasien punya lebih dari satu hasil golongan darah tervalidasi, mana yang berlaku? | Tanpa aturan ini, "golongan darah pasien" tidak punya jawaban tunggal, padahal `INV-BD-014` menuntut satu sumber sah | Pemilik proses klinis | Menahan penetapan aturan identitas pada `BD-AGG-04` |
| `ARCH-BD-GAP-03` | Apakah bukti kecocokan punya masa berlaku? | Uji kecocokan yang dilakukan beberapa hari lalu belum tentu masih sah. Bila tidak diatur, bukti lama dapat dipakai membuka gerbang pemberian | Pemilik proses klinis | **Keselamatan.** Menahan pengetatan gerbang pada `BD-AGG-03` |
| `ARCH-BD-GAP-04` | Bila kantong dialihkan ke pasien lain, apakah bukti kecocokan sebelumnya otomatis gugur? | Bukti kecocokan selalu terhadap pasien tertentu. Membawa bukti lama ke pasien baru berarti memberikan darah tanpa pemeriksaan | Pemilik proses klinis | **Keselamatan.** Menahan jalur `REALLOCATED` pada `BD-AGG-03` |
| `ARCH-BD-GAP-05` | Apakah alokasi yang keliru boleh dibatalkan sebelum pemberian, dan dengan wewenang siapa? | Petugas salah memilih kantong adalah kejadian wajar. Tanpa jalur pembatalan, kantong itu terkunci selamanya | Pemilik proses BDRS | Menahan satu perpindahan pada `BD-AGG-03` |
| `ARCH-BD-GAP-06` | Bagaimana mengoreksi pencatatan pemberian yang keliru? | Pemberian adalah status akhir yang tidak dapat dibatalkan. Kekeliruan pencatatan tetap mungkin terjadi | Pemilik proses klinis dan BDRS | Menahan jalur koreksi pada `BD-AGG-03` |

Decision ID pemblokir yang sudah ada dan tetap berlaku: `DEC-BD-016` kontrak sumber biaya ·
`OQ-BD-011` mekanik label · `DEF-BD-003` bukti kecocokan per komponen · `DEF-BD-004` peran jalur
darurat dan peran validator · `OQ-BD-010` kesediaan PMI menerima pengembalian.

---

## N. Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_PARTIAL`**

### Yang boleh diserahkan ke penyusunan blueprint

| Slice arsitektur | Isi | Alasan boleh jalan |
| --- | --- | --- |
| `BD-AGG-01` Order Darah | Order, baris kebutuhan, lifecycle lengkap termasuk kedaluwarsa | Kepemilikan jelas, invariant terwakili, seluruh keputusan bisnisnya sudah turun |
| `BD-AGG-02` Permintaan Darah ke PMI | Permintaan, penerimaan, lifecycle sampai penutupan administratif | Sama, kecuali aturan kelebihan kiriman yang dicatat sebagai gap dan tidak menghalangi bentuk dasarnya |
| `BD-AGG-05` Tindakan Bank Darah | Pencatatan tindakan beserta konteks dan rujukan tarifnya | Dinyatakan berdiri sendiri dari bagian penyerahan biaya |
| `BD-DOM-13`, `BD-DOM-14` | Katalog komponen darah dan daftar alasan terkendali | Ditetapkan penuh oleh `DEC-BD-024` |
| `BD-DOM-16`, `BD-DOM-17` | Pembaca status kunjungan dan ringkasan pemenuhan | Ditetapkan penuh oleh `DEC-BD-014` dan BG-BD-003 |
| `BD-DOM-18` | Titipan tanda kewenangan unit memesan darah | Ditetapkan penuh oleh `DEC-BD-012`; menunggu pelaksanaan pemilik data induk |

### Yang harus berhenti

| Slice arsitektur | Yang menahan |
| --- | --- |
| `BD-AGG-03` **jalur pemberian dan jalur pengalihan** | `ARCH-BD-GAP-03` masa berlaku bukti kecocokan dan `ARCH-BD-GAP-04` gugurnya bukti saat pengalihan. Keduanya menyangkut keselamatan pasien |
| `BD-AGG-03` **jalur pembatalan alokasi dan koreksi pemberian** | `ARCH-BD-GAP-05` dan `ARCH-BD-GAP-06` |
| `BD-AGG-04` **aturan hasil mana yang berlaku** | `ARCH-BD-GAP-02` |
| Penyerahan fakta biaya ke Billing | `DEC-BD-016` |
| Mekanik label golongan darah | `OQ-BD-011` |
| Penetapan peran pada jalur darurat, validasi hasil, penyelesaian kantong, dan pembatalan order | `DEF-BD-004` |

**Bagian `BD-AGG-03` yang tetap boleh dirancang** adalah penerimaan kantong, penyimpanan jejak
permintaan asal, alokasi beserta perlindungan satu-alokasi-aktif, pencatatan bukti kecocokan sebagai
konsep, keadaan menunggu keputusan, serta jalur `RETURNED_TO_PROVIDER` dan `NOT_USABLE`. Yang berhenti
adalah perpindahan menuju pemberian dan menuju pengalihan.

Pemisahan ini disengaja: bentuk datanya sudah aman untuk dirancang, sementara aturan yang menentukan
kapan darah boleh keluar dan kapan bukti lama gugur belum boleh ditebak.

---

## O. Handoff

```yaml
blueprint_id: BD-BP-001
blueprint_revision: 3
domain_architecture_revision: 1
domain_architecture_readiness: DOMAIN_ARCHITECTURE_PARTIAL
domain_architecture_scope:
  siap:
    - BD-AGG-01
    - BD-AGG-02
    - BD-AGG-05 (pencatatan tindakan saja)
    - BD-DOM-13
    - BD-DOM-14
    - BD-DOM-16
    - BD-DOM-17
    - BD-DOM-18
    - BD-AGG-03 (penerimaan, alokasi, keadaan menunggu keputusan, jalur kembali ke PMI dan tidak layak)
    - BD-AGG-04 (sampel, pencatatan hasil, validasi)
  berhenti:
    - BD-AGG-03 (pemberian, jalur darurat, pengalihan, pembatalan alokasi, koreksi pemberian)
    - BD-AGG-04 (aturan hasil mana yang berlaku)
    - penyerahan fakta biaya ke Billing
    - mekanik label golongan darah
requirement_readiness: PARTIALLY_READY
requirement_evidence_status: seluruh slice yang masuk CONFIRMED; tidak ada CONFLICT tersisa
capability_scope: [BD-SLICE-01, BD-SLICE-02, BD-SLICE-03, BD-SLICE-04, BD-SLICE-05, BD-SLICE-06, BD-SLICE-07, BD-SLICE-08, BD-SLICE-09, BD-SLICE-10]
blocking_decision_ids: [DEC-BD-016, OQ-BD-011, DEF-BD-003, DEF-BD-004, OQ-BD-010, ARCH-BD-GAP-01, ARCH-BD-GAP-02, ARCH-BD-GAP-03, ARCH-BD-GAP-04, ARCH-BD-GAP-05, ARCH-BD-GAP-06]
dependency_ids: [BD-DEP-001, BD-DEP-002, BD-DEP-003, BD-DEP-004, BD-DEP-005, BD-DEP-006, BD-DEP-007, BD-DEP-008, BD-DEP-010, BD-DEP-011, BD-DEP-012, BD-DEP-013, BD-DEP-014]
decision_revision: 2
backend_source_sha: 9522caacf29371b1fddd1584e9a71ad94fe48d19
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

### Langkah berikutnya

Slice yang siap diserahkan ke `design-business-module`. Enam gap arsitektur baru
(`ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-06`) dikembalikan ke `grill-me` sebagai closure pass lanjutan,
karena seluruhnya adalah keputusan bisnis dan klinis yang bergantung pemilik — bukan pertanyaan
tentang apa yang sudah ada di sistem.
