# Bank Darah — Hospital Domain Architecture

## A. Identitas arsitektur

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Blueprint revision | `6` |
| Domain architecture revision | `6` |
| Modul | Bank Darah (`bank-darah`) |
| Tanggal | `2026-09-02` |
| Backend SHA | `792acb9331a65187d052fffd4a292d3bce2fd828` cabang `sukmagp` |
| Backend SHA saat bukti kemampuan diaudit | `9522caacf29371b1fddd1584e9a71ad94fe48d19`. Perbedaan sampai `792acb9` hanya dokumen blueprint Bank Darah, nol berkas source aplikasi, sehingga `BD-CAP-001` sampai `BD-CAP-024` tetap sahih **secara isi**. Satu celah cakupan diketahui dan dicatat di bawah |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Kesiapan requirement | `PARTIALLY_READY` — `02-requirement-completeness-assessment.md` revisi 2 |
| Register keputusan | `00-interview-decisions.md` revisi 7 — memuat `DEC-BD-001` sampai `DEC-BD-038`, `INV-BD-011` sampai `INV-BD-030`, `AC-BD-001` sampai `AC-BD-076` |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_READY`** untuk seluruh scope yang dinilai |
| Baseline rujukan Indonesia | Tidak dipakai. Tidak ada `baseline_observation_ids` maupun `baseline_source_ids`. |
| Pass ini | Revisi 6 — penyerapan `DEC-BD-038`, perluasan gerbang lokasi nonaktif ke jalur pemberian. Menutup `OQ-BD-015` |
| Celah cakupan capability audit yang diketahui | `BD-CAP-006` tidak menyebut `MstDrugStorageLocation`, yang sebenarnya sudah ada di `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs`. Ini celah **cakupan** audit, bukan bukti basi. Celah ini **tidak** mengubah kesimpulan arsitektur mana pun, karena `DEC-BD-035` justru menolak memakai ulang master farmasi itu |

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

**Catatan gerbang masuk untuk revisi 3.** `02-requirement-completeness-assessment.md` masih revisi 2
dan belum menyerap `DEC-BD-025` sampai `DEC-BD-034`. Itu tidak menghalangi pass ini, karena kesepuluh
keputusan closure hanya **menutup** blocker dan tidak satu pun menambah blocker baru pada kesiapan
requirement. Kesiapan per slice yang dipakai tetap yang tercatat di sana. Bila penilaian kelengkapan
kelak diperbarui, statusnya hanya dapat naik, tidak turun.

**Catatan gerbang masuk tambahan untuk revisi 4.** `DEC-BD-035` dan `DEC-BD-036` memperkenalkan satu
kebutuhan baru, `BR-BD-020` — pencatatan lokasi penyimpanan fisik kantong darah dan perpindahannya.
Kebutuhan itu **belum** diklasifikasikan ke dalam salah satu `BD-SLICE-*` oleh penilaian kelengkapan
requirement, karena penilaian itu masih revisi 2. Dokumen ini **tidak** menerbitkan slice requirement
baru; menerbitkan `BD-SLICE-11` atas nama gerbang requirement bukan wewenang sesi arsitektur.

Yang dilakukan dokumen ini: menempatkan `BR-BD-020` sebagai **perluasan scope** pada tiga slice yang
sudah diterima, sesuai tempat kerjanya masing-masing.

| Slice yang bertambah scope | Bagian `BR-BD-020` yang jatuh ke sana |
| --- | --- |
| `BD-SLICE-03` Penerimaan fisik kantong | Status `RECEIVED` saat kantong tiba, penetapan lokasi pertama sehingga kantong menjadi `STORED` |
| `BD-SLICE-04` Alokasi kantong | Gerbang `INV-BD-025`: kantong tanpa lokasi tidak dapat dialokasikan |
| `BD-SLICE-10` Sampling, batas Laboratorium, HCLAB, laporan, setup | Master lokasi penyimpanan darah sebagai data rujukan Setup yang ketiga |

Perpindahan lokasi (`INV-BD-026`) melintasi `BD-SLICE-03` dan `BD-SLICE-04`, karena kantong dapat
dipindahkan baik sebelum maupun sesudah dialokasikan. Ketiga slice di atas sudah berstatus
`READY_FOR_DOMAIN_DESIGN` sebelum pass ini, dan `DEC-BD-035` beserta `DEC-BD-036` **menambah aturan,
bukan menambah blocker**. Penilaian kelengkapan requirement tetap perlu disinkronkan agar `BR-BD-020`
punya rumah slice yang resmi — itu pekerjaan `requirement-completeness-gate`, bukan pekerjaan di sini.

### Bukti dan Decision ID yang dipertahankan

`00-interview-decisions.md` revisi 5 — `SCOPE-BD-001`, `DEC-BD-001` sampai `DEC-BD-036`,
`BR-BD-020`, `INV-BD-011` sampai `INV-BD-026`, `ASM-BD-001` sampai `ASM-BD-007`, `DEF-BD-003`,
`DEF-BD-004`, `AC-BD-059` sampai `AC-BD-064` ·
`00-business-overview.md` revisi 1 · `02-existing-capability-map.md` revisi 2 — `BD-CAP-001` sampai
`BD-CAP-024` · `02-requirement-completeness-assessment.md` revisi 2 · `01-prerequisite-readiness.md`
revisi 3 — `BD-DEP-001` sampai `BD-DEP-015`.

Keputusan yang diserap sampai revisi 3: `DEC-BD-025` sampai `DEC-BD-030` dari architecture gap closure
pass, ditambah empat keputusan final — `DEC-BD-031` penyelesaian konflik golongan darah lewat
pemeriksaan ulang · `DEC-BD-032` masa berlaku bukti kecocokan per komponen · `DEC-BD-033` penyelesaian
konflik di layar pemeriksaan · `DEC-BD-034` batas koreksi terhadap biaya. Turunannya `INV-BD-017`
sampai `INV-BD-024` dan `AC-BD-031` sampai `AC-BD-058`.

Keputusan yang diserap pada revisi 4: `DEC-BD-035` kepemilikan dan scope master lokasi penyimpanan
darah · `DEC-BD-036` lokasi penyimpanan sebagai gerbang kesiapan operasional kantong. Turunannya
`BR-BD-020`, `INV-BD-025`, `INV-BD-026`, dan `AC-BD-059` sampai `AC-BD-064`. Keduanya juga
**mengamandemen** `DEC-BD-024`: Setup Bank Darah kini memuat **tiga** data rujukan, bukan dua.

Keputusan yang diserap pada revisi 5: `DEC-BD-037` perlakuan terhadap kantong yang berada di lokasi
penyimpanan yang dinonaktifkan. Turunannya `INV-BD-027`, `INV-BD-028`, `AC-BD-065` sampai `AC-BD-071`,
dan `ARCH-BD-POS-06`. Keputusan ini menutup `ARCH-BD-GAP-10`.

Keputusan yang diserap pada revisi 6: `DEC-BD-038` perluasan gerbang lokasi nonaktif ke jalur
pemberian. Turunannya `INV-BD-029`, `INV-BD-030`, `AC-BD-072` sampai `AC-BD-076`, dan
`ARCH-BD-POS-07`. Keputusan ini menutup `OQ-BD-015`.

**Revisi 6 mengoreksi satu pernyataan revisi 5.** Revisi 5 mencatat bahwa gerbang lokasi nonaktif
**tidak** berlaku pada pemberian kantong yang terlanjur dialokasikan, mengikuti bunyi `DEC-BD-037`
yang memang hanya menyebut alokasi, dan membuka `OQ-BD-015` untuk menanyakannya. Pemilik proses
menjawab sebaliknya lewat `DEC-BD-038`. Pernyataan revisi 5 itu karena itu **dicabut**, bukan
diperhalus. Rangkaian ini adalah cara kerja yang diharapkan: arsitektur menerapkan keputusan apa
adanya, menandai batasnya, lalu menyerap jawabannya ketika turun.

**Penomoran `DEC-BD-037` sudah dikukuhkan.** Keputusan ini semula diberikan langsung oleh pemilik
kebutuhan dalam sesi arsitektur, sehingga `DEC-BD-037`, `INV-BD-027`, dan `INV-BD-028` sempat berstatus
ID sementara. Storage Location decision closure pass pada `00-interview-decisions.md` revisi 6 sudah
mencatatnya resmi dengan nomor yang sama, beserta `AC-BD-065` sampai `AC-BD-071`. Tidak ada rujukan
yang perlu diperbaiki, dan ketiga ID itu berhenti bersifat sementara.

**Status persetujuan.** `DEC-BD-035`, `DEC-BD-036`, dan `DEC-BD-037` berstatus `draft` dengan
`approved_by` dan `approved_at` kosong, persis seperti `DEC-BD-001` sampai `DEC-BD-034` yang sudah
diserap revisi 1 sampai 3. Dokumen ini memperlakukan ketiganya sama dengan pendahulunya dan **tidak**
menandai persetujuan atas nama pemilik proses BDRS.

Decision ID pemblokir yang tersisa, semuanya **di luar** scope desain yang dinilai atau hanya
`IMPLEMENTATION`: `DEC-BD-016` kontrak sumber biaya (di luar scope) · `OQ-BD-011` mekanik label (di
luar scope) · `DEF-BD-003` bukti kecocokan per komponen · `DEF-BD-004` peran · `OQ-BD-010` kesediaan
PMI menerima pengembalian · `OQ-BD-012` nilai jam masa berlaku · `OQ-BD-014` keadaan kantong setelah
koreksi. Tidak satu pun menahan `DESIGN` slice yang dinilai.

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
| **Lokasi penyimpanan darah** | Tempat fisik milik BDRS untuk menyimpan kantong darah — misalnya Kulkas Besar dan Kulkas Kecil. Punya penanda aktif atau nonaktif. **Bukan** gudang farmasi dan bukan lokasi penyimpanan obat. |
| **Lokasi penyimpanan obat** | Tempat penyimpanan milik Farmasi. Kata yang mirip, konsep yang berbeda: beda pemilik, beda aturan, beda lifecycle. Dua makna ini sengaja **tidak** disatukan (`DEC-BD-035`). |
| **Penempatan** | Catatan bahwa satu kantong ditaruh di satu lokasi penyimpanan, sejak kapan, dan oleh siapa. Penempatan pertama membuat kantong menjadi tersimpan; penempatan berikutnya adalah perpindahan. |
| **Perpindahan lokasi** | Kantong berpindah dari satu lokasi penyimpanan ke lokasi lain. **Tidak** mengubah status kantong, dan **tidak** menyentuh catatan penerimaan awalnya. |
| **`RECEIVED`** | Kantong sudah ada secara fisik di MMC, tetapi belum punya lokasi penyimpanan yang tercatat. Belum boleh dialokasikan kepada siapa pun. |
| **`STORED`** | Kantong sudah ditaruh pada satu lokasi penyimpanan dan penempatannya tercatat. Ini gerbang kesiapan operasional, bukan sekadar keterangan tambahan. |
| **`AVAILABLE`** | Kantong berada di dalam stok yang boleh dialokasikan. Keadaan istirahat yang dapat dimasuki berkali-kali — misalnya setelah alokasi dibatalkan. |
| **Lokasi nonaktif** | Lokasi penyimpanan yang tidak lagi boleh dipakai. Tidak dapat dipilih untuk penempatan baru, tidak pernah dihapus, dan riwayat kantong yang pernah ada di sana tetap terbaca. |
| **Lokasi operasional yang valid** | Lokasi penyimpanan yang **sedang aktif**. Kantong hanya dapat dialokasikan bila penempatan terakhirnya menunjuk lokasi seperti itu (`DEC-BD-037`). |

**Kenapa dua kata yang mirip sengaja dipisahkan.** "Lokasi penyimpanan" pada Farmasi dan "lokasi
penyimpanan" pada Bank Darah terdengar sama dan bahkan sudah punya satu master di sistem
(`MstDrugStorageLocation`). Bukti sumber memperlihatkan master itu berisi penanda farmasi yang tidak
punya arti bagi darah, dan kosong dari hal yang justru penting bagi darah. `DEC-BD-035` memilih
memisahkan keduanya. Kontrak arsitektur melarang menyatukan dua makna berbeda hanya karena katanya
sama, dan larangan itulah yang berlaku di sini.

Catatan penting soal kata "permintaan": dalam dokumen bisnis sehari-hari, orang sering menyebut
"permintaan darah" untuk dua hal berbeda — permintaan dokter ke Bank Darah, dan permintaan Bank Darah
ke PMI. Arsitektur ini memisahkan keduanya menjadi **order darah** dan **permintaan darah**, dan
perbedaan itu wajib dipertahankan di seluruh dokumen turunan.

---

## C. Peta bounded context

| ID | Context | Tanggung jawab | Konsep yang dimiliki | Hubungan dengan Bank Darah |
| --- | --- | --- | --- | --- |
| `BD-CTX-01` | **Bank Darah** | Seluruh lifecycle pemenuhan darah pasien di dalam MMC | Order darah, permintaan ke PMI, kantong operasional, alokasi, bukti kecocokan, pemeriksaan golongan darah, sampel, tindakan Bank Darah, katalog komponen, daftar alasan, **master lokasi penyimpanan darah**, **riwayat penempatan kantong**, riwayat pergerakan | — pemilik |
| `BD-CTX-02` | Registrasi dan Kunjungan | Kunjungan pasien rawat jalan dan IGD beserta status akhirnya | Kunjungan, status kunjungan | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-03` | Pasien | Identitas pasien | Pasien, golongan darah administratif | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-04` | Tenaga Kerja | Dokter dan pegawai | Dokter, pegawai | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-05` | Data Induk Layanan | Unit pelayanan, klinik, ruangan, kelas pasien, tindakan dan tarif | Unit pelayanan beserta tanda kewenangannya | Hulu. Bank Darah membaca, dan **menitipkan satu tanda kewenangan baru** |
| `BD-CTX-06` | Rawat Inap | Episode rawat inap beserta kepulangan pasien | Episode, waktu pasien meninggalkan rumah sakit | Hulu. Bank Darah **hanya membaca** |
| `BD-CTX-07` | Billing | Tarif dan akibat finansial | Kontrak sumber biaya, tagihan | Hilir. Bank Darah mengirim fakta tindakan selesai — **kontraknya belum disetujui** |
| `BD-CTX-08` | PMI | Penyediaan darah | Stok darah nasional | Luar sistem. **Tidak ada antarmuka teknis** pada MVP |
| `BD-CTX-09` | Laboratorium | Pemeriksaan laboratorium umum | Pesanan lab, sampel lab, hasil lab | **Jalan sendiri-sendiri** pada MVP. Tidak ada ketergantungan dua arah |
| `BD-CTX-10` | Penyimpanan Farmasi | Lokasi penyimpanan obat dan perbekalan farmasi | `MstDrugStorageLocation` — berkasnya berada di area Data Induk Layanan, tetapi maknanya milik Farmasi | **Tidak bersinggungan.** Bank Darah tidak membaca, tidak memperluas, dan tidak mengambil alih master ini (`DEC-BD-035`) |

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

**Terhadap Penyimpanan Farmasi (`BD-CTX-10`): dua tetangga yang sengaja tidak disambungkan.**
Sistem sudah punya `MstDrugStorageLocation` beserta tipe `ColdStorage`, rentang suhu, dan kode
rak/shelf/bin. Godaan untuk memakainya ulang besar, dan `DEC-BD-035` menolaknya. Alasannya bukan
selera teknis: bukti sumber menunjukkan master itu membawa penanda `IsPharmacyLocation`,
`IsControlledDrugStorage`, `IsHighAlertStorage`, `IsAllowDispensing`, dan sejenisnya — seluruhnya
aturan bisnis farmasi. Kantong darah bukan obat, tidak di-*dispensing*, dan tidak tunduk pada aturan
narkotika. Memakai ulang master itu berarti menaruh dua pemilik proses di atas satu tabel yang sama,
dan itu persis pola yang dilarang kontrak arsitektur.

Arahnya juga bukan sebaliknya: Bank Darah **tidak** memperluas master farmasi dengan atribut darah.
Yang dilakukan adalah membuat master sendiri yang bersih dari atribut farmasi. Penggabungan menjadi
satu `MstStorageLocation` bersama dinyatakan `DEC-BD-035` sebagai bahan evaluasi setelah MVP, dan
dokumen ini tidak mendahuluinya.

**Terhadap Laboratorium (`BD-CTX-09`): sengaja berjalan sendiri-sendiri.** `DEC-BD-015` dan
`DEC-BD-018` menempatkan pemeriksaan golongan darah dan sampelnya di dalam Bank Darah, bukan di
Laboratorium. Ini keputusan sadar, bukan kelalaian, dan disertai klausa masa depan: bila kelak
Laboratorium mengambil alih, wajib ada keputusan kepemilikan dan penyelarasan sumber kebenaran.
Dilarang ada dua sumber sah tanpa aturan prioritas (`INV-BD-015`).

### Peninjauan ulang batas ownership pada revisi 2 dan 4

Sepuluh keputusan architecture gap closure (`DEC-BD-025` sampai `DEC-BD-034`) diperiksa satu per satu
terhadap lima batas kepemilikan yang paling mudah bergeser tanpa disadari. Hasilnya: **tidak ada satu
pun batas ownership yang berpindah.** Yang berubah hanyalah ketegasan sebagian batas. Empat keputusan
final (`DEC-BD-031` sampai `DEC-BD-034`) pun tidak menggeser satu batas pun: penyelesaian konflik dan
masa berlaku per komponen tetap milik Bank Darah, layar penyelesaian tetap urusan frontend, dan
`DEC-BD-034` justru mempertegas bahwa keputusan biaya tetap milik Billing.

**Pemeriksaan ownership pada revisi 4.** `DEC-BD-035` adalah keputusan ownership tulen, sehingga
diperiksa tersendiri. Yang diputuskan: lokasi penyimpanan darah **menambah satu pemilik data baru** di
dalam Bank Darah, dan **tidak mengambil alih** apa pun dari pihak lain.

| Pertanyaan ownership | Jawaban | Bukti |
| --- | --- | --- |
| Siapa pemilik otoritatif lokasi penyimpanan darah? | Bank Darah (`BD-CTX-01`), lewat pemilik proses BDRS | `DEC-BD-035` |
| Apakah ini menduplikasi data induk bersama milik pihak lain? | **Tidak.** Master farmasi tetap utuh milik pemiliknya dan tidak disalin, tidak dibaca, dan tidak diperluas | `DEC-BD-035`, bukti sumber `MstDrugStorageLocation.cs@792acb9` |
| Apakah ada dua sumber kebenaran untuk "lokasi penyimpanan"? | **Tidak**, karena keduanya menjawab pertanyaan yang berbeda: satu untuk obat, satu untuk darah. Tidak ada kantong darah yang boleh tercatat di master farmasi, dan sebaliknya | `DEC-BD-035` |
| Siapa pemilik jawaban "kantong ini ada di mana"? | Kantong itu sendiri, lewat riwayat penempatannya di dalam `BD-AGG-03` | `DEC-BD-036`, `INV-BD-026` |
| Apakah ada context hulu yang harus berubah karenanya? | **Tidak satu pun.** Berbeda dengan `BD-DOM-18` yang menitipkan tanda kewenangan ke Data Induk Layanan, penyimpanan darah tidak menitipkan apa pun ke siapa pun | — |

**Risiko yang dicatat, bukan disembunyikan.** Dua master lokasi yang hidup berdampingan berarti dua
tempat mendaftarkan ruangan dan lemari pendingin. `DEC-BD-035` sudah mengakui hal itu dan menaruh
penggabungan menjadi `MstStorageLocation` bersama sebagai bahan evaluasi setelah MVP. Sesi arsitektur
ini mengikuti keputusan tersebut dan tidak mendahuluinya, tetapi mencatat konsekuensinya supaya
keputusan POST-MVP itu tidak hilang: bila kelak digabungkan, yang harus diselesaikan lebih dulu adalah
siapa pemilik master gabungannya, bukan bentuk tabelnya.

| Batas | Apakah bergeser? | Penjelasan |
| --- | --- | --- |
| **Billing** `BD-CTX-07` | Tidak | Tarif tetap milik Billing, dan Bank Darah tetap tidak pernah menghitungnya. `DEC-BD-034` menegaskan batasnya: koreksi pencatatan (`DEC-BD-030`) tidak pernah membalik fakta biaya secara otomatis — biaya berasal dari tindakan (`DEC-BD-021`), bukan dari kantong. Keputusan apakah biaya ditinjau tetap milik Billing dan menempel `DEC-BD-016`; `ARCH-BD-GAP-09` ditutup dari sisi Bank Darah tanpa mengarang kebijakan Billing |
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
| `BD-DOM-05` | Kantong Darah Operasional | `AGGREGATE_ROOT` | Bank Darah — `New` | Identitas internal, dengan **nomor kantong terbitan PMI** sebagai identifier bisnis yang unik | `RECEIVED` → `STORED` → `AVAILABLE` → `ALLOCATED` → `ISSUED`, atau menunggu keputusan → dialihkan, dikembalikan, tidak layak | Tidak pernah menjadi stok bebas; tidak boleh punya lebih dari satu alokasi aktif; **tidak dapat dialokasikan sebelum punya lokasi penyimpanan dan melewati `STORED`** (`INV-BD-025`), **dan tidak dapat dialokasikan selama lokasi penyimpanannya sedang nonaktif** (`INV-BD-028`) | `DEC-BD-001`, `DEC-BD-007`, `DEC-BD-019`, `DEC-BD-036`, `DEC-BD-037`, `ASM-BD-003` |
| `BD-DOM-06` | Alokasi | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Mengikat kantong pada satu baris kebutuhan order; dapat berhenti aktif lewat pembatalan | Satu kantong hanya boleh punya satu alokasi **aktif**; alokasi yang dibatalkan tidak dihapus, hanya berhenti aktif, dan menyimpan alasan, pelaku, serta waktu | `DEC-BD-003`, `DEC-BD-029`, BG-BD-002 |
| `BD-DOM-07` | Bukti Kecocokan | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal, **selalu terhadap pasangan kantong dan pasien tertentu** | Gerbang sebelum pemberian, dengan masa berlaku | Wajib ada sebelum pemberian, kecuali lewat jalur darurat · hanya membuka gerbang untuk pasien yang dituju bukti itu · berhenti membuka gerbang setelah masa berlakunya lewat | `DEC-BD-013`, `DEC-BD-027`, `DEC-BD-028`, `INV-BD-012`, `INV-BD-019`, `INV-BD-020` |
| `BD-DOM-08` | Otorisasi Darurat | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal | Menggantikan gerbang pemberian pada keadaan darurat — baik gerbang bukti kecocokan, gerbang lokasi penyimpanan aktif, maupun keduanya | Hanya oleh peran berwenang; alasan wajib; penanda melekat permanen · **wajib menyatakan gerbang mana yang dilewatinya** (`INV-BD-030`), karena penanda tanpa keterangan berhenti bermakna bagi pembaca rekam berikutnya | `DEC-BD-017`, `DEC-BD-038` |
| `BD-DOM-09` | Pemeriksaan Golongan Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Identitas internal | Sampel diambil → hasil dicatat → hasil tervalidasi | Hasil belum tervalidasi tidak boleh dipakai untuk keperluan klinis | `DEC-BD-015`, `INV-BD-014` |
| `BD-DOM-21` | Golongan Darah Sah Pasien | `Adapter/View` | Bank Darah — turunan | Dikenali lewat rujukan pasien | Menjawab satu pertanyaan: apa golongan darah sah pasien ini sekarang, atau apakah sedang bertentangan | Dihitung dari hasil tervalidasi milik `BD-DOM-09`, **bukan** kolom yang bisa disunting · bernilai kosong ketika hasil bertentangan · tidak pernah membaca `MstPatient.BloodType` | `DEC-BD-026`, `INV-BD-014`, `INV-BD-018` |
| `BD-DOM-22` | Penyelesaian Perbedaan Hasil | `ENTITY` mandiri, di luar batas `BD-AGG-04` | Bank Darah — `New` | Identitas internal, menunjuk pasien, hasil-hasil yang bertentangan, dan **pemeriksaan ulang** yang memutus | Mengakhiri keadaan tertahan pada `BD-DOM-21` | Hanya dapat ditambah · wajib menunjuk satu pemeriksaan ulang tervalidasi (`BD-DOM-09`) yang dinyatakan validator sebagai hasil berlaku · wajib menyimpan validator, waktu, dan alasan · sistem tidak menghitung mayoritas (`ARCH-BD-GAP-07` ditutup `DEC-BD-031`, `INV-BD-022`) | `DEC-BD-026`, `DEC-BD-031`, `DEF-BD-004` |
| `BD-DOM-10` | Sampel Bank Darah | `ENTITY` di dalam `BD-DOM-09` | Bank Darah — `New` | Identifier sampel terbitan sistem | Menjaga penelusuran dari pasien ke hasil | Bukan sampel Laboratorium; tidak menimbulkan tagihan Laboratorium | `DEC-BD-018` |
| `BD-DOM-11` | Golongan Darah dan Rhesus | `VALUE_OBJECT` | Platform — `Existing` | — | Dipakai pada permintaan dan pada hasil pemeriksaan | Nilai yang sama dipakai di kedua tempat, **tetapi maknanya berbeda** | `BD-CAP-016` |
| `BD-DOM-12` | Tindakan Bank Darah | `AGGREGATE_ROOT` | Bank Darah — `New` | Nomor tindakan terbitan sistem | Dicatat → selesai | Menunjuk order, unit, dokter BDRS, petugas, kelas, dan tindakan bertarif | `DEC-BD-021`, BR-BD-004 |
| `BD-DOM-13` | Katalog Komponen Darah | `REFERENCE_DATA` | Bank Darah — `New` | Kode komponen | Dipakai order, permintaan, dan deteksi ganda. **Tempat menyimpan masa berlaku bukti kecocokan per komponen** (nama kerja `CompatibilityEvidenceValidityHours`), dibaca dari konfigurasi | Komponen tidak boleh berupa ketikan bebas. Masa berlaku per komponen tidak pernah ditanam di kode (`INV-BD-023`). Kumpulan atributnya kini **boleh dibekukan** — `ARCH-BD-GAP-08` ditutup `DEC-BD-032` | `DEC-BD-024`, `DEC-BD-005`, `DEC-BD-027`, `DEC-BD-032` |
| `BD-DOM-14` | Daftar Alasan Terkendali | `REFERENCE_DATA` | Bank Darah — `New` | Kode alasan | Dipakai pembatalan, jalur darurat, dan penyelesaian kantong | Alasan tidak boleh teks bebas semata; perubahan wajib berjejak | `DEC-BD-024`, `INV-BD-016` |
| `BD-DOM-15` | Riwayat Pergerakan | `DOMAIN_EVENT` tersimpan | Bank Darah — `New` | Identitas internal | Merekam setiap perpindahan status yang berarti | Hanya bisa ditambah; tidak pernah diubah atau dihapus | `BD-CAP-009`, BG-BD-004 |
| `BD-DOM-16` | Pembaca Status Kunjungan | `Adapter/View` | Bank Darah membaca `BD-CTX-02` dan `BD-CTX-06` | — | Menjawab satu pertanyaan: apakah kunjungan ini sudah berakhir | Tidak pernah mengubah data hulu | `DEC-BD-014` |
| `BD-DOM-17` | Ringkasan Pemenuhan | `Adapter/View` | Bank Darah — turunan | — | Menyajikan jumlah diminta, diberikan, dan belum diberikan | Dihitung dari transaksi, **bukan** kolom yang bisa disunting · wajib menghormati catatan koreksi `BD-DOM-23`, sehingga pemberian yang pencatatannya dikoreksi tidak dihitung dua kali | BG-BD-003, INV-BD-003, `DEC-BD-030` |
| `BD-DOM-18` | Kewenangan Unit Memesan Darah | `Extend` pada `BD-CTX-05` | Data Induk Layanan | — | Menentukan unit mana yang boleh membuat order | Bawaan menolak; tidak dikunci di kode | `DEC-BD-012`, `BD-CAP-005` |
| `BD-DOM-19` | Kontrak Sumber Biaya Bank Darah | `Extend` pada `BD-CTX-07` | Billing | — | Menerima fakta tindakan selesai | **Belum disetujui** — `DEC-BD-016` | `BD-CAP-015` |
| `BD-DOM-20` | Pasien, Kunjungan, Dokter, Unit, Kelas | `Existing` | Context hulu masing-masing | — | Rujukan | Tidak pernah diduplikasi ke Bank Darah | `BD-CAP-001`, `002`, `004`, `006` |
| `BD-DOM-23` | Catatan Koreksi Pemberian | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal, menunjuk satu pemberian asal | Menyatakan bahwa **pencatatan** sebuah pemberian keliru | Hanya dapat ditambah · tidak pernah menghapus atau membalik pemberian asal · tidak boleh dipakai memindahkan pemberian ke pasien lain · wajib menyimpan apa yang keliru, apa yang benar, alasan terkendali, pelaku, dan waktu | `DEC-BD-030`, `INV-BD-021` |
| `BD-DOM-24` | Lokasi Penyimpanan Darah | `REFERENCE_DATA` | Bank Darah — `New` | Kode lokasi terbitan BDRS, dengan nama lokasi yang dikenali petugas | Menyediakan pilihan tempat penyimpanan; punya penanda aktif atau nonaktif | Lokasi tidak boleh berupa ketikan bebas · hanya lokasi **aktif** yang dapat menjadi tujuan penempatan baru, termasuk tujuan perpindahan (`INV-BD-027`) · **keaktifan lokasi ikut menentukan boleh tidaknya kantong di dalamnya dialokasikan** (`INV-BD-028`) · lokasi yang dinonaktifkan tidak pernah dihapus, karena riwayat lama wajib tetap terbaca · penonaktifan **tidak pernah** memindahkan kantong dengan sendirinya (`DEC-BD-037`) · **tidak** menyimpan suhu, kapasitas, maupun penanda farmasi apa pun (`DEC-BD-035`) | `DEC-BD-035`, `DEC-BD-037`, `DEC-BD-024` (diamandemen) |
| `BD-DOM-25` | Penempatan Kantong | `ENTITY` di dalam `BD-DOM-05` | Bank Darah — `New` | Identitas internal, menunjuk satu kantong dan satu lokasi | Merekam di mana kantong berada dan sejak kapan; penempatan pertama membawa kantong ke `STORED`, penempatan berikutnya adalah perpindahan | Hanya dapat ditambah (`INV-BD-026`) · satu kantong punya paling banyak **satu** penempatan yang sedang berlaku · tidak pernah mengubah catatan penerimaan awal kantong · wajib menyimpan lokasi, pelaku, dan waktu | `DEC-BD-035`, `DEC-BD-036`, `INV-BD-026` |

Tidak ada satu pun konsep di atas yang diturunkan dari nama menu, nama layar, atau nama task.
Sebagai contoh, menu `Laporan` dan menu `Setup` pada bukti navigasi **tidak** melahirkan konsep
domain bernama Laporan atau Setup: yang lahir hanyalah **tiga** data rujukan pada `BD-DOM-13`,
`BD-DOM-14`, dan `BD-DOM-24`, karena ketiganya memang dituntut keputusan bisnis, bukan dituntut oleh
nama menunya. Data rujukan ketiga masuk lewat amandemen `DEC-BD-024` oleh `DEC-BD-035`, bukan lewat
pelebaran menu Setup atas inisiatif dokumen ini.

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

**Revisi 3 tidak menambah konsep domain.** Empat keputusan final hanya mempertajam konsep yang sudah
ada: `DEC-BD-031` mengunci mekanisme `BD-DOM-22` (wajib lewat pemeriksaan ulang), `DEC-BD-032`
membekukan satu atribut pada `BD-DOM-13`, `DEC-BD-033` menempatkan penyelesaian pada layar pemeriksaan
(`BD-AGG-04`), dan `DEC-BD-034` menegaskan batas terhadap Billing. Tidak ada identitas, lifecycle, atau
tanggung jawab audit baru yang lahir.

**Dua konsep baru pada revisi 4, dan kenapa hanya dua.** `DEC-BD-035` dan `DEC-BD-036` bisa saja
diterjemahkan menjadi lima konsep — master lokasi, status `RECEIVED`, status `STORED`, perpindahan
lokasi, dan lokasi kantong saat ini. Itu tidak dilakukan.

- **`BD-DOM-24` lahir** karena lokasi penyimpanan darah punya identitas sendiri (kode dan nama yang
  dikenali petugas), punya pemilik sendiri (pemilik proses BDRS), dan punya lifecycle sendiri yang
  sederhana tetapi nyata (aktif dan nonaktif). Klasifikasinya `New`, bukan `Existing` maupun `Extend`,
  karena `DEC-BD-035` secara tegas menolak memakai ulang `MstDrugStorageLocation`. Bukti sumber
  mendukung penolakan itu: master farmasi itu berpusat pada aturan obat, bukan aturan darah.
- **`BD-DOM-25` lahir** karena "di mana kantong berada" punya riwayat, bukan hanya nilai terakhir.
  `INV-BD-026` menuntut riwayat yang hanya bertambah, dan riwayat yang punya pelaku, waktu, serta
  kewajiban audit sendiri adalah konsep, bukan kolom.
- **`RECEIVED` dan `STORED` tidak melahirkan konsep apa pun.** Keduanya status pada `BD-DOM-05`.
  Membuat konsep terpisah untuk setiap status persis larangan kontrak "jangan membuat entity per nilai
  status".
- **"Perpindahan lokasi" tidak melahirkan konsep terpisah dari `BD-DOM-25`.** Penempatan pertama dan
  perpindahan berikutnya adalah kejadian yang sama bentuknya; yang membedakan hanya ada atau tidaknya
  penempatan sebelumnya. Memecahnya menjadi dua konsep akan membuat dua tabel riwayat untuk satu
  pertanyaan yang sama.
- **"Lokasi kantong saat ini" tidak melahirkan konsep apa pun.** Ia jawaban turunan dari penempatan
  terakhir — lihat `ARCH-BD-POS-05` — mengikuti pola yang sudah dipakai `BD-DOM-17` dan `BD-DOM-21`.

**Revisi 6 juga tidak menambah konsep domain.** `DEC-BD-038` memperluas satu gerbang dan memperluas
arti satu konsep yang sudah ada. `BD-DOM-08` Otorisasi Darurat kini dapat melewati dua gerbang, bukan
satu, dan karena itu wajib menyebutkan yang mana. Tidak ada konsep "otorisasi darurat lokasi" yang
lahir terpisah dari "otorisasi darurat bukti kecocokan" — keduanya kejadian yang sama bentuknya,
dengan pelaku, alasan, waktu, dan penanda permanen yang sama. Memisahkannya berarti membuat konsep per
sebab, dan itu larangan yang sama dengan membuat entity per status.

**Revisi 5 tidak menambah konsep domain.** `DEC-BD-037` hanya mempertajam dua konsep yang sudah ada:
penanda aktif pada `BD-DOM-24` kini punya akibat yang lebih luas dari sekadar menyaring pilihan, dan
gerbang alokasi pada `BD-DOM-05` bertambah satu syarat. Tidak ada identitas, lifecycle, maupun
tanggung jawab audit baru yang lahir. Khususnya, **"lokasi nonaktif" tidak menjadi konsep tersendiri**
— ia satu nilai pada penanda yang sudah ada, dan membuatnya menjadi konsep berarti membuat entity per
nilai status.

---

## E. Model aggregate

Aggregate dipakai hanya ketika ada batas konsistensi yang harus melindungi invariant. Lima aggregate
berikut lahir dari lifecycle yang benar-benar berbeda.

### Peninjauan ulang batas aggregate pada revisi 2 dan 4

Kelima batas ditinjau ulang terhadap sepuluh keputusan closure (`DEC-BD-025` sampai `DEC-BD-034`).
**Tidak ada satu pun batas aggregate yang berpindah.** Yang berubah adalah isi invariant, daftar
tindakan, prasyarat perpindahan, dan daftar kejadiannya.

| Nama dalam permintaan tinjauan | Aggregate di dokumen ini | Batas bergeser? | Yang berubah |
| --- | --- | --- | --- |
| `BloodRequest` | `BD-AGG-01` Order Darah | Tidak | Tidak ada perubahan langsung. Angka pemenuhannya kini dihitung dengan menghormati catatan koreksi (`BD-DOM-23`) |
| `PMI Request` | `BD-AGG-02` Permintaan Darah ke PMI | Tidak | Invariant sisa diperbaiki agar tidak pernah negatif; lahir `BD-XINV-03` |
| `BloodUnit` | `BD-AGG-03` Kantong Darah Operasional | Tidak | Bertambah dua perpindahan (pembatalan alokasi, catatan koreksi) dan tiga invariant baru |
| `BloodGroupVerification` | `BD-AGG-04` Pemeriksaan Golongan Darah | Tidak | Aturan "hasil mana yang sah" tetap `BD-XINV-04`, di luar aggregate. Penyelesaian konflik kini punya prasyarat pasti: wajib lewat pemeriksaan ulang tervalidasi (`DEC-BD-031`) |
| `BloodBankProcedure` | `BD-AGG-05` Tindakan Bank Darah | Tidak | Tidak tersentuh keenam keputusan baru |

**Peninjauan ulang pada revisi 4.** Kelima batas ditinjau ulang sekali lagi terhadap `DEC-BD-035` dan
`DEC-BD-036`. **Tidak ada batas aggregate yang berpindah.** Hanya `BD-AGG-03` yang berubah isinya:
batasnya bertambah satu entity (`BD-DOM-25` Penempatan Kantong), lifecycle-nya bertambah dua status di
depan (`RECEIVED` dan `STORED`), invariant-nya bertambah dua (`INV-BD-025`, `INV-BD-026`), dan daftar
tindakannya bertambah dua (tetapkan lokasi, pindahkan lokasi).

Master lokasi (`BD-DOM-24`) **berada di luar** seluruh aggregate, sama seperti `BD-DOM-13` katalog
komponen dan `BD-DOM-14` daftar alasan. Alasannya sama pula: data rujukan punya lifecycle sendiri yang
tidak boleh dikunci oleh satu kantong mana pun. Menarik master lokasi ke dalam `BD-AGG-03` berarti
setiap penyuntingan nama kulkas harus mengunci kantong-kantong di dalamnya — harga yang tidak sepadan
dan tidak diminta aturan bisnis mana pun.

**Peninjauan ulang pada revisi 5.** `DEC-BD-037` diperiksa terhadap kelima batas. **Tidak ada batas
yang berpindah, dan tidak ada entity baru.** Yang bertambah hanya dua invariant pada `BD-AGG-03`.

Satu hal perlu dicatat karena mudah disalahpahami: `INV-BD-028` menuntut `BD-AGG-03` membaca penanda
aktif milik `BD-DOM-24`, yang berada **di luar** batasnya. Itu **bukan** alasan menarik master lokasi
masuk ke dalam aggregate. Pembacaan lintas batas pada saat gerbang diperiksa sudah menjadi pola yang
dipakai di tempat lain pada arsitektur ini — `BD-DOM-16` membaca status kunjungan milik context hulu
dengan cara yang sama. Yang dilarang adalah **menyalin** keadaan itu ke dalam kantong, dan itu justru
yang dicegah `ARCH-BD-POS-06`.

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
| Batas | Kantong beserta alokasinya (`BD-DOM-06`), bukti kecocokannya (`BD-DOM-07`), otorisasi daruratnya (`BD-DOM-08`), catatan koreksi pemberiannya (`BD-DOM-23`), dan **riwayat penempatannya** (`BD-DOM-25`) |
| Invariant yang dilindungi | **Satu kantong tidak boleh punya lebih dari satu alokasi aktif** · kantong tidak pernah menjadi stok bebas · **kantong tidak dapat dialokasikan sebelum punya lokasi penyimpanan dan melewati `STORED`** (`INV-BD-025`) · **satu kantong punya paling banyak satu penempatan yang sedang berlaku, dan riwayat penempatannya hanya dapat ditambah** (`INV-BD-026`) · **setiap penempatan baru wajib menunjuk lokasi yang sedang aktif** (`INV-BD-027`) · **kantong tidak dapat dialokasikan selama penempatan terakhirnya menunjuk lokasi yang nonaktif** (`INV-BD-028`) · **pemberian lewat jalur normal menuntut tiga syarat sekaligus — lokasi terakhir aktif, sudah melewati `STORED`, dan bukti kecocokan berlaku untuk pasien tujuan serta belum lewat masa berlakunya — dinilai ulang saat pemberian dicoba** (`INV-BD-029`, `INV-BD-019`, `INV-BD-020`) · **otorisasi darurat wajib menyatakan gerbang mana yang dilewatinya** (`INV-BD-030`) · kantong yang ordernya berakhir tidak dapat dialokasikan ke pasien lain sebelum diselesaikan · **pemberian tidak pernah dihapus maupun dibalik** (`INV-BD-021`) |
| Tindakan bisnis | **Tetapkan lokasi penyimpanan** · **pindahkan lokasi penyimpanan** · alokasikan ke baris kebutuhan · **batalkan alokasi** · catat bukti kecocokan · berikan · berikan lewat jalur darurat · **catat koreksi pencatatan pemberian** · tandai menunggu keputusan · alihkan ke pasien lain · kembalikan ke PMI · nyatakan tidak layak |
| Kejadian yang diterbitkan | Kantong diterima · **kantong disimpan pada lokasi** · **kantong masuk stok yang boleh dialokasikan** · **lokasi penyimpanan kantong dipindahkan** · dialokasikan · **alokasi dibatalkan** · bukti kecocokan tercatat · **bukti kecocokan gugur karena pengalihan** · diberikan · diberikan darurat · **koreksi pemberian dicatat** · masuk menunggu keputusan · dialihkan · dikembalikan · dinyatakan tidak layak |

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

**Kenapa penempatan berada di dalam batas kantong, sedangkan master lokasi di luarnya.** Dua hal ini
mudah tertukar. Penempatan (`BD-DOM-25`) adalah fakta tentang **kantong tertentu** — kantong ini ada
di kulkas itu sejak jam sekian. Master lokasi (`BD-DOM-24`) adalah daftar tempat yang boleh dipilih,
dan hidupnya sama sekali tidak bergantung pada kantong mana pun.

Dua invariant memaksa penempatan berada di dalam batas kantong. Pertama, satu kantong tidak boleh
punya dua lokasi yang sama-sama berlaku — kantong tidak bisa berada di dua kulkas sekaligus. Kedua,
gerbang alokasi (`INV-BD-025`) menanyakan keadaan penempatan kantong itu tepat pada saat alokasi
hendak dilakukan. Kedua pemeriksaan itu hanya dapat dijamin bila penempatan dan kantong berada dalam
satu batas konsistensi.

Sebaliknya, tidak ada satu pun invariant yang menuntut master lokasi ikut terkunci bersama kantong.
Karena itu master lokasi tetap di luar, dan kantong hanya menyimpan **penunjuk** ke lokasi, bukan
salinan nama atau atributnya.

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
| Invariant yang dilindungi | Hasil yang belum tervalidasi tidak boleh dipakai untuk keperluan klinis · hasil wajib menyimpan pemeriksa dan waktu pemeriksaan · sampel bersifat opsional, tetapi bila ada wajib menyimpan waktu dan petugas pengambil · hasil yang sudah tervalidasi tidak pernah ditimpa oleh hasil berikutnya · **perbedaan hasil hanya dapat diselesaikan lewat pemeriksaan ulang tervalidasi, tidak pernah dengan menghitung mayoritas** (`INV-BD-022`) |
| Tindakan bisnis | Catat pengambilan sampel · catat hasil ABO dan Rhesus · validasi hasil · **catat pemeriksaan ulang saat konflik** · **selesaikan konflik dengan menyatakan hasil ulang yang berlaku** |
| Kejadian yang diterbitkan | Sampel diambil · hasil dicatat · hasil tervalidasi · **hasil bertentangan terdeteksi** · **pemeriksaan ulang tervalidasi** · **perbedaan hasil diselesaikan** |

**Catatan batas.** Dua kejadian terakhir diterbitkan oleh `BD-AGG-04`, tetapi akibatnya — pasien kehilangan golongan darah sah, lalu memperolehnya kembali — terbaca pada `BD-DOM-21` yang bersifat turunan. `BD-AGG-04` tidak menyimpan jawaban "golongan darah sah pasien" di dalam dirinya, karena jawaban itu bukan miliknya sendiri melainkan milik seluruh pemeriksaan pasien tersebut. Sejak `DEC-BD-031`, keluarnya pasien dari keadaan konflik menuntut satu **pemeriksaan ulang tervalidasi** yang dinyatakan validator sebagai hasil berlaku — pemeriksaan ulang itu hidup di dalam `BD-AGG-04`, sedangkan pernyataan berlakunya terbaca pada `BD-DOM-21`.

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

### Posisi arsitektur yang diambil pada revisi 2 dan 4

Tiga hal berikut adalah **keputusan pemodelan**, bukan aturan bisnis baru. Keputusan bisnisnya sudah
turun; yang dipilih di sini hanyalah cara mewujudkannya secara logis.

| ID | Posisi | Alasan |
| --- | --- | --- |
| `ARCH-BD-POS-01` | Lewatnya masa berlaku bukti kecocokan dimodelkan sebagai **kondisi turunan** yang dihitung saat gerbang diperiksa, bukan sebagai perubahan status yang disimpan | Tidak menuntut penjadwal latar belakang, tidak membuat keadaan kantong bergantung pada kapan pekerjaan latar terakhir berjalan, dan tetap benar walaupun nilai masa berlakunya kelak diubah. Konsekuensinya, tidak ada kejadian "bukti kedaluwarsa" yang perlu diterbitkan |
| `ARCH-BD-POS-02` | Bagian bukti kecocokan pada gerbang pemberian dinyatakan sebagai **satu** pertanyaan atas tiga hal sekaligus: kantong, pasien tujuan, dan waktu. **Sejak revisi 6 posisi ini tidak lagi berdiri sendiri** — ia menjadi salah satu dari tiga syarat pada `ARCH-BD-POS-07` | `DEC-BD-013`, `DEC-BD-027`, dan `DEC-BD-028` semuanya jatuh ke predikat yang sama. Memecahnya menjadi tiga pemeriksaan terpisah membuka peluang salah satu terlewat |
| `ARCH-BD-POS-03` | Invariant "satu alokasi aktif" dinilai atas himpunan alokasi yang **sedang aktif**, bukan atas jumlah baris alokasi | `DEC-BD-029` menuntut pembatalan tanpa penghapusan. Bila invariant dinilai dari jumlah baris, riwayat yang jujur akan tampak seperti pelanggaran |

Dua posisi berikut ditambahkan pada revisi 4. Keduanya juga **keputusan pemodelan**, bukan aturan
bisnis baru.

| ID | Posisi | Alasan |
| --- | --- | --- |
| `ARCH-BD-POS-04` | `STORED` dan `AVAILABLE` dipertahankan sebagai **dua status yang berbeda**, tetapi pada MVP perpindahan di antara keduanya terjadi **sebagai akibat**, tanpa tindakan manusia tambahan dan tanpa prasyarat tambahan | `DEC-BD-036` menyerahkan pilihan ini ke sesi arsitektur, dan pemilik proses menyebut kelima status secara eksplisit. Keduanya menjawab pertanyaan yang berbeda: `STORED` menjawab "sudah ditaruh dan tercatat di mana?", `AVAILABLE` menjawab "sudah masuk stok yang boleh dialokasikan?". Perbedaan itu bukan hiasan — kantong yang alokasinya dibatalkan kembali ke `AVAILABLE`, **tidak** kembali ke `STORED` maupun `RECEIVED`, karena tonggak penempatan hanya dilewati sekali (`DEC-BD-029`). Menyatukan keduanya sekarang akan menghapus perbedaan itu, dan menutup satu-satunya tempat yang wajar bila kelak MMC menghendaki langkah pelepasan atau karantina sebelum kantong masuk stok |
| `ARCH-BD-POS-05` | Lokasi kantong saat ini adalah **jawaban turunan** dari penempatan terakhir. Yang dikunci arsitektur adalah larangannya berbeda dari riwayat, bukan cara menyimpannya | `INV-BD-026` menuntut riwayat yang hanya bertambah. Bila lokasi saat ini menjadi nilai mandiri yang boleh ditimpa, riwayat dan nilai terakhir dapat berselisih tanpa ada yang tahu mana yang benar. Apakah jawaban itu juga disimpan sebagai penunjuk pada kantong demi kemudahan baca adalah pilihan implementasi milik `design-business-module` — asalkan yang disimpan selalu sama dengan penempatan terakhir. Pola yang sama sudah dipakai `BD-DOM-17` dan `BD-DOM-21` |

**Batas yang sengaja tidak ditembus `ARCH-BD-POS-04`.** Dokumen ini **tidak** mengarang langkah
pelepasan, karantina, penyaringan ulang, maupun pemeriksaan kedua di antara `STORED` dan `AVAILABLE`.
Bukti yang disetujui tidak menyebut satu pun dari itu, dan `DEC-BD-035` justru mengeluarkan pemantauan
suhu serta sejenisnya dari MVP. Yang dilakukan hanya menjaga sambungannya tetap terbuka.

Satu posisi ditambahkan pada revisi 5, dan seperti lima sebelumnya ia **keputusan pemodelan**, bukan
aturan bisnis baru.

| ID | Posisi | Alasan |
| --- | --- | --- |
| `ARCH-BD-POS-06` | Gerbang alokasi dinyatakan sebagai **satu** pertanyaan yang dinilai tepat pada saat alokasi dicoba: *apakah kantong ini sudah melewati `STORED`, dan apakah penempatan terakhirnya menunjuk lokasi yang sedang aktif?* Keaktifan lokasi **tidak pernah disalin** ke kantong | Ini pola `ARCH-BD-POS-01` yang diterapkan pada persoalan berbeda. Bila keaktifan lokasi disalin ke kantong, maka menonaktifkan satu kulkas menuntut penyuntingan seluruh kantong di dalamnya — pekerjaan latar belakang, jendela waktu ketika sebagian kantong sudah tersentuh dan sebagian belum, dan kemungkinan salinan yang basi. Dinilai saat gerbang diperiksa, penonaktifan cukup satu penanda pada satu baris master, dan seluruh gerbang ikut tertutup pada detik yang sama tanpa satu pun kantong disentuh. Menyatukannya dengan `INV-BD-025` menjadi satu pertanyaan mengikuti alasan yang sama dengan `ARCH-BD-POS-02`: dua pemeriksaan terpisah membuka peluang salah satu terlewat |

**Kenapa posisi ini penting bagi `DEC-BD-037`.** Keputusan pemilik proses menyatakan sistem **tidak**
memindahkan kantong dengan sendirinya. `ARCH-BD-POS-06` adalah cara memenuhinya tanpa setengah-setengah:
penonaktifan lokasi tidak memindahkan kantong, tidak mengubah status kantong, dan tidak menyentuh satu
baris penempatan pun. Yang berubah hanya jawaban atas pertanyaan gerbang.

Satu posisi ditambahkan pada revisi 6, menyerap `DEC-BD-038`.

| ID | Posisi | Alasan |
| --- | --- | --- |
| `ARCH-BD-POS-07` | Gerbang pemberian **memuat seluruh gerbang alokasi**, ditambah bukti kecocokan. Predikatnya: *sudah melewati `STORED`* **dan** *penempatan terakhirnya menunjuk lokasi aktif* **dan** *bukti kecocokan berlaku untuk pasien tujuan serta belum lewat masa berlakunya*. Ketiganya dinilai ulang tepat pada saat pemberian dicoba, tidak pernah diwarisi dari saat alokasi | `DEC-BD-038` menuntut ketiganya. Menyatakannya sebagai satu predikat yang **memuat** predikat alokasi menghasilkan sifat yang mudah diuji dan sulit dilanggar: kantong tidak pernah dapat keluar lewat gerbang yang lebih longgar daripada gerbang yang dulu mengizinkannya masuk. Penilaian ulang adalah inti keputusannya — lokasi bisa saja masih aktif ketika kantong dialokasikan lalu dinonaktifkan sesudahnya, dan justru kasus itulah yang dipersoalkan `OQ-BD-015`. Bila hasil pemeriksaan saat alokasi boleh diwariskan, keputusan ini tidak berlaku apa-apa |

**Hubungan ketiga posisi gerbang.** `ARCH-BD-POS-06` adalah gerbang alokasi, `ARCH-BD-POS-02` adalah
bagian bukti kecocokan, dan `ARCH-BD-POS-07` menggabungkan keduanya menjadi gerbang pemberian. Ketiganya
memakai pola yang sama dan sudah dipakai sejak `ARCH-BD-POS-01`: **kondisi dinilai saat gerbang
diperiksa, tidak disimpan sebagai status dan tidak disalin ke kantong.** Konsekuensinya, tidak ada satu
pun dari ketiga gerbang ini yang menuntut pekerjaan latar belakang.

**Yang tidak berubah karena `ARCH-BD-POS-07`.** Jalur darurat tetap menjadi satu-satunya jalan melewati
gerbang, dan bentuknya tidak berubah (`DEC-BD-017`). Yang bertambah hanya kewajiban menyebutkan gerbang
mana yang dilewati (`INV-BD-030`), karena sejak revisi 6 penanda darurat punya dua sebab yang mungkin
dan keduanya dapat terjadi bersamaan.

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
| Kantong Darah | Penempatan Kantong | Di mana kantong berada dan sejak kapan | satu ke banyak, **maksimal satu yang sedang berlaku** | Milik kantong | Wajib sebelum kantong dapat dialokasikan (`INV-BD-025`) | Ikut kantong; **tidak pernah dihapus** (`INV-BD-026`) |
| Penempatan Kantong | Lokasi Penyimpanan Darah | Tempat kantong ditaruh | banyak ke satu | Master milik Bank Darah, berada **di luar** aggregate kantong | Wajib | Lokasi yang dinonaktifkan tidak pernah dihapus, sehingga penempatan lama tetap terbaca |
| Kantong Darah | Lokasi Penyimpanan Darah saat ini | Jawaban "kantong ini sekarang ada di mana" | banyak ke satu, **turunan** | Turunan dari penempatan terakhir (`ARCH-BD-POS-05`) | Ada sejak kantong `STORED` | Tidak menyimpan keadaan sendiri; tidak pernah boleh berbeda dari penempatan terakhir |
| Kantong Darah | Bukti Kecocokan | Gerbang sebelum pemberian | satu ke banyak | Milik kantong | Wajib sebelum pemberian | Ikut kantong |
| Bukti Kecocokan | Pasien yang dituju | Bukti selalu terhadap pasien tertentu | banyak ke satu | Pasien dimiliki context hulu | Wajib | Bukti tidak ikut berpindah ketika kantong dialihkan; ia berhenti berlaku dan tetap tersimpan |
| Kantong Darah | Catatan Koreksi Pemberian | Pernyataan bahwa pencatatan pemberian keliru | satu ke banyak | Milik kantong | Boleh kosong | Ikut kantong; tidak pernah dihapus |
| Catatan Koreksi Pemberian | Pemberian asal | Koreksi selalu menunjuk satu pemberian tertentu | banyak ke satu | Milik kantong yang sama | Wajib | Pemberian asal tetap ada selamanya |
| Penyelesaian Perbedaan Hasil | Pemeriksaan Golongan Darah yang bertentangan | Menyebut hasil-hasil yang diperselisihkan | satu ke banyak | Milik Bank Darah | Wajib minimal dua | Hasil yang disebut tetap tersimpan utuh |
| Penyelesaian Perbedaan Hasil | Pemeriksaan ulang yang memutus | Hasil ulang yang dinyatakan berlaku | banyak ke satu | Milik Bank Darah | Wajib satu (`DEC-BD-031`) | Pemeriksaan ulang tetap tersimpan; dinyatakan berlaku oleh validator |
| Penyelesaian Perbedaan Hasil | Pasien | Perbedaan selalu milik seorang pasien | banyak ke satu | Pasien dimiliki context hulu | Wajib | — |
| Golongan Darah Sah Pasien | Pemeriksaan Golongan Darah | Jawaban dihitung dari hasil tervalidasi | satu ke banyak | Turunan, tidak memiliki apa pun | — | Tidak menyimpan keadaan sendiri |
| Pemeriksaan Golongan Darah | Pasien | Hasil milik seorang pasien | banyak ke satu | Pasien dimiliki context hulu | Wajib | — |
| Pemeriksaan Golongan Darah | Sampel | Asal hasil | satu ke satu | Milik pemeriksaan | Opsional | Ikut pemeriksaan |
| Tindakan Bank Darah | Order Darah | Tindakan dilakukan atas order | banyak ke satu | Milik Bank Darah | Wajib | — |
| Tindakan Bank Darah | Tindakan bertarif | Rujukan tarif | banyak ke satu | Tarif dimiliki Billing | Wajib | — |

Tidak ada satu pun relasi di atas yang berupa penyalinan data induk. Seluruh rujukan ke pasien,
kunjungan, dokter, unit, kelas, tarif, dan **lokasi penyimpanan** disimpan sebagai penunjuk, bukan
sebagai salinan.
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
| `CLOSED_ENCOUNTER` | Kantong tetap datang | Tetap `CLOSED_ENCOUNTER` | Petugas Bank Darah | — | Penerimaan tetap tercatat; kantong tetap wajib disimpan, lalu masuk menunggu keputusan alih-alih menjadi `AVAILABLE` |
| `REQUESTED` atau `PARTIALLY_FULFILLED` | Terima kantong melebihi jumlah diminta | `FULFILLED`, sisa berhenti di 0 | Petugas Bank Darah | `BD-XINV-03` dijaga dengan token konkurensi | Penerimaan tercatat; kantong berlebih ditandai, tetap wajib disimpan, lalu masuk menunggu keputusan |

**Catatan sejak revisi 4.** Sebelum `DEC-BD-036`, kantong yang bernasib khusus — datang setelah
kunjungan berakhir, atau melebihi jumlah yang diminta — dikatakan "langsung masuk menunggu keputusan".
Sejak `DEC-BD-036` kalimat itu perlu dibaca lebih teliti: yang "langsung" adalah **nasib
administratifnya**, bukan izin melewati penyimpanan. Kantongnya tetap benda fisik yang harus masuk
kulkas, jadi ia tetap melewati `RECEIVED` lalu `STORED`; yang berbeda hanyalah dari `STORED` ia menuju
`PENDING_REVIEW`, bukan `AVAILABLE`. Rinciannya ada pada §G.3.

**Contoh berangka.** Diminta 3 kantong PRC untuk Tn. S. Hari pertama datang 2, status menjadi
`PARTIALLY_FULFILLED` dengan sisa 1. Tn. S pulang Senin siang, permintaan menjadi `CLOSED_ENCOUNTER`.
Selasa pagi kantong sisanya tetap diantar. Penerimaan **tetap dicatat**, kantong tetap membawa jejak
permintaan asalnya, tetap ditaruh di lokasi penyimpanan sebagaimana kantong lain, lalu masuk keadaan
menunggu keputusan alih-alih menjadi stok yang boleh dialokasikan.

**Contoh kelebihan kiriman.** Diminta 2 kantong PRC untuk Tn. S, yang datang 3. Kantong pertama dan
kedua masuk sebagai penerimaan biasa dan permintaan menjadi `FULFILLED` dengan sisa 0. Kantong ketiga
tetap dicatat diterima, tetap menunjuk permintaan asal, tetap wajib ditaruh di lokasi penyimpanan,
lalu masuk menunggu keputusan. Sisa permintaan **tidak** menjadi minus 1, dan kantong ketiga
**tidak** boleh dialokasikan langsung ke order Tn. S walaupun pasiennya sama — ia wajib melewati
penyelesaian `DEC-BD-019` lebih dulu.

### G.3 Kantong Darah Operasional

`DEC-BD-036` membakukan rantai utama kantong menjadi lima nama status:
`RECEIVED` → `STORED` → `AVAILABLE` → `ALLOCATED` → `ISSUED`.

Sebagian nama itu hanya membakukan sebutan yang sudah dipakai revisi 1 sampai 3; sebagian lagi
benar-benar baru.

| Sebutan pada revisi 1–3 | Nama baku sejak revisi 4 | Sifat perubahan |
| --- | --- | --- |
| — | `RECEIVED` | **Keadaan baru.** Dulu kantong yang diterima langsung dianggap siap dialokasikan |
| — | `STORED` | **Keadaan baru.** Dulu tidak ada gerbang penyimpanan sama sekali |
| Tersedia | `AVAILABLE` | Hanya penamaan |
| Dialokasikan | `ALLOCATED` | Hanya penamaan |
| Diberikan | `ISSUED` | Hanya penamaan |

`PENDING_REVIEW`, `REALLOCATED`, `RETURNED_TO_PROVIDER`, dan `NOT_USABLE` tidak berubah sedikit pun.

**Yang benar-benar berubah, bukan sekadar penamaan.** Sebelum `DEC-BD-036`, kantong yang baru diterima
**langsung** menjadi stok yang boleh dialokasikan. Sejak `DEC-BD-036`, di antara keduanya ada dua
keadaan dan satu gerbang: selama kantong belum punya lokasi penyimpanan yang tercatat, ia tidak dapat
dialokasikan kepada siapa pun (`INV-BD-025`). Ini pengetatan, bukan pelonggaran — tidak ada satu pun
jalur lama yang menjadi lebih longgar karenanya.

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Diterima secara fisik | `RECEIVED` | Petugas Bank Darah | Terikat permintaan asal | Penerimaan |
| `RECEIVED` | **Tetapkan lokasi penyimpanan** | `STORED` | Petugas Bank Darah | Lokasi dipilih dari master lokasi yang **sedang aktif** (`DEC-BD-035`, `AC-BD-062`) | Penempatan pertama: lokasi, pelaku, waktu |
| `STORED` | **Masuk stok yang boleh dialokasikan** | `AVAILABLE` | Sistem, sebagai akibat penempatan | Tidak ada tindakan manusia tambahan dan tidak ada prasyarat tambahan pada MVP (`ARCH-BD-POS-04`) | Perubahan status |
| `STORED` | Kantong berlebih, atau permintaan asalnya sudah `CLOSED_ENCOUNTER` | `PENDING_REVIEW` | Sistem | `DEC-BD-025`, `DEC-BD-020`. Kantong tetap wajib disimpan lebih dulu — nasib administratif tidak membatalkan kewajiban fisik | Perubahan status beserta sebabnya |
| `AVAILABLE` | Alokasikan | `ALLOCATED` | Petugas Bank Darah | Order aktif · tidak ada alokasi aktif lain pada kantong ini · **kantong sudah punya lokasi dan sudah melewati `STORED`** (`INV-BD-025`) · **lokasi penempatan terakhirnya sedang aktif** (`INV-BD-028`, `ARCH-BD-POS-06`) | Alokasi, pelaku, waktu |
| `ALLOCATED` | Catat bukti kecocokan | `ALLOCATED`, bukti lengkap | Petugas berwenang | — | Status pemeriksaan, pelaku, waktu |
| `ALLOCATED`, bukti lengkap | Berikan | `ISSUED` | Petugas Bank Darah | Tiga syarat sekaligus, **dinilai ulang saat pemberian dicoba**: sudah melewati `STORED` · penempatan terakhirnya menunjuk **lokasi aktif** · bukti kecocokan ada **untuk pasien tujuan** dan **belum lewat masa berlaku** (`INV-BD-029`, `ARCH-BD-POS-07`) | Pemberian, beserta rujukan bukti yang dipakai |
| `ALLOCATED`, bukti lengkap, lokasi penyimpanannya nonaktif | Dicoba diberikan lewat jalur normal | **Ditolak**, tetap `ALLOCATED` | Petugas Bank Darah | Gerbang tertutup walaupun bukti kecocokan masih berlaku dan alokasinya sah. Petugas memindahkan kantong ke lokasi aktif lebih dulu (`DEC-BD-038`) | Penolakan beserta sebabnya |
| `ALLOCATED` | Batalkan alokasi | `AVAILABLE`, atau `PENDING_REVIEW` bila order asal sudah berakhir | Petugas Bank Darah | Kantong belum diberikan; alasan dari daftar terkendali; keaktifan order asal dibaca lewat `BD-DOM-16` | Alasan, pelaku, waktu, dan alokasi mana yang berhenti aktif |
| `ISSUED` | Catat koreksi pencatatan | Tetap `ISSUED`, dengan catatan koreksi melekat | Peran berwenang — `DEF-BD-004` | Alasan dari daftar terkendali; menunjuk satu pemberian yang benar-benar ada | Apa yang keliru, apa yang benar, pelaku, dan waktu |
| `ALLOCATED` | Berikan lewat jalur darurat | `ISSUED`, **ditandai melewati gerbang** | **Peran berwenang — `DEF-BD-004`** | Alasan wajib dari daftar terkendali. Berlaku untuk kedua sebab: bukti kecocokan belum ada, lokasi penyimpanan nonaktif, atau keduanya (`DEC-BD-017`, `DEC-BD-038`) | Otorisasi darurat lengkap dengan penandanya, **beserta gerbang mana yang dilewati** (`INV-BD-030`) |
| `AVAILABLE` atau `ALLOCATED` | Order berakhir | `PENDING_REVIEW` | Sistem | Order kedaluwarsa atau dibatalkan | Perubahan status |
| `PENDING_REVIEW` | Alihkan ke pasien lain | `REALLOCATED` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib; **lokasi penempatan terakhirnya sedang aktif**, karena pengalihan adalah pengikatan kantong ke baris kebutuhan pasien lain — yaitu alokasi (`INV-BD-028`) | Pasien asal, alasan pelepasan, pasien tujuan, **dan bukti kecocokan mana saja yang gugur karenanya** |
| `PENDING_REVIEW` | Kembalikan ke PMI | `RETURNED_TO_PROVIDER` | Petugas berwenang | Proses bisnis PMI mendukung — `OQ-BD-010` | Alasan, pelaku, waktu |
| `PENDING_REVIEW` | Nyatakan tidak layak | `NOT_USABLE` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib | Alasan, pelaku, waktu |
| `STORED`, `AVAILABLE`, `ALLOCATED`, `PENDING_REVIEW`, atau `REALLOCATED` | **Pindahkan lokasi penyimpanan** | **Status tidak berubah** | Petugas Bank Darah | Lokasi tujuan sedang aktif (`INV-BD-027`); kantong belum berada pada status akhir. **Berlaku juga ketika lokasi asalnya sudah dinonaktifkan** — inilah jalur yang dipakai petugas untuk mengeluarkan kantong dari lokasi nonaktif (`DEC-BD-037`) | Penempatan baru: lokasi asal, lokasi tujuan, pelaku, waktu (`INV-BD-026`) |

**Status akhir yang tidak dapat dibatalkan:** `ISSUED`, `RETURNED_TO_PROVIDER`, dan `NOT_USABLE`.
Darah yang sudah diberikan tidak dapat ditarik kembali oleh sistem. `DEC-BD-030` **tidak** mengubah
sifat itu: catatan koreksi tidak memindahkan kantong keluar dari status `ISSUED`, tidak
mengembalikannya menjadi tersedia, dan tidak membatalkan apa pun. Ia hanya menempel.

**Penempatan adalah tonggak yang hanya dilewati sekali.** Kantong yang alokasinya dibatalkan kembali
ke `AVAILABLE`, **bukan** ke `STORED` maupun `RECEIVED` — ia sudah punya lokasi, dan lokasi itu tidak
hilang karena alokasinya batal. Begitu pula kantong yang keluar dari `PENDING_REVIEW` lewat
pengalihan. Inilah yang membedakan `STORED` dari `AVAILABLE` secara nyata, dan alasan keduanya tidak
disatukan (`ARCH-BD-POS-04`).

**Perpindahan lokasi tidak pernah menjadi perpindahan status.** `DEC-BD-036` menyatakannya tegas, dan
tabel di atas mengikutinya: baris perpindahan lokasi adalah satu-satunya baris yang kolom "Ke"-nya
berbunyi "status tidak berubah". Kantong yang sedang dialokasikan untuk Tn. S tetap dialokasikan untuk
Tn. S walaupun kulkasnya berganti. Menjadikan perpindahan lokasi sebagai perpindahan status akan
menciptakan status per atribut, dan itu dilarang kontrak.

**Kondisi turunan, bukan status tersimpan.** Lewatnya masa berlaku bukti kecocokan **tidak**
memindahkan kantong ke status baru. Kantong tetap `ALLOCATED`; yang berubah hanyalah jawaban atas
pertanyaan gerbang ketika pemberian hendak dilakukan. Karena itu tidak ada baris "bukti kedaluwarsa"
pada tabel di atas, dan tidak ada pekerjaan latar belakang yang perlu berjalan — lihat
`ARCH-BD-POS-01`.

**Contoh penempatan awal.** Kantong `PMI-00912` diterima Senin pagi dari permintaan atas nama Tn. S,
dan masuk `RECEIVED`. Petugas langsung mencoba mengalokasikannya untuk Tn. S. Sistem menolak, karena
kantong belum punya lokasi penyimpanan (`INV-BD-025`, `AC-BD-060`). Petugas menaruh kantong di "Kulkas
Besar" dan mencatat lokasinya; kantong menjadi `STORED`, lalu langsung `AVAILABLE`. Alokasi berikutnya
berhasil.

**Contoh perpindahan lokasi.** Selasa, "Kulkas Besar" perlu dibersihkan. Kantong `PMI-00912` — yang
saat itu sudah `ALLOCATED` untuk Tn. S — dipindahkan ke "Kulkas Kecil". Statusnya tetap `ALLOCATED`,
alokasinya tetap milik Tn. S, dan bukti kecocokan yang sudah tercatat tetap berlaku. Riwayat menyimpan
dua penempatan: Kulkas Besar sejak Senin, Kulkas Kecil sejak Selasa. Catatan bahwa kantong diterima
Senin pagi dari permintaan asalnya **tidak tersentuh sama sekali** (`INV-BD-026`, `AC-BD-063`).

**Contoh kantong berlebih yang tetap wajib disimpan.** Diminta 2 kantong PRC untuk Tn. S, datang 3.
Ketiganya masuk `RECEIVED` dan ketiganya wajib ditaruh di lokasi penyimpanan. Kantong pertama dan kedua
menjadi `STORED` lalu `AVAILABLE`. Kantong ketiga juga menjadi `STORED` — darahnya nyata dan harus
masuk kulkas — tetapi dari sana ia masuk `PENDING_REVIEW`, bukan `AVAILABLE`, karena permintaannya
sudah penuh (`DEC-BD-025`). Nasib administratif tidak pernah menjadi alasan membiarkan darah tidak
tersimpan.

**Contoh lokasi yang dinonaktifkan.** "Kulkas Lama" rusak dan ditandai nonaktif oleh petugas BDRS.
Empat hal terjadi sekaligus, dan satu hal sengaja **tidak** terjadi (`DEC-BD-037`):

1. "Kulkas Lama" berhenti muncul sebagai tujuan penempatan mana pun — baik penempatan pertama kantong
   yang baru datang maupun tujuan perpindahan (`INV-BD-027`, `AC-BD-062`).
2. Tiga kantong yang saat itu ada di dalamnya **tetap tercatat berada di sana**. Riwayat penempatannya
   utuh, dan statusnya tidak berubah sedikit pun — yang `AVAILABLE` tetap `AVAILABLE`.
3. Ketiga kantong itu **tidak dapat dialokasikan** selama masih tercatat di lokasi nonaktif
   (`INV-BD-028`). Gerbangnya tertutup pada detik penanda dinonaktifkan, tanpa satu pun kantong
   disentuh (`ARCH-BD-POS-06`).
4. Petugas BDRS memindahkan ketiganya ke "Kulkas Besar" lewat proses perpindahan lokasi yang biasa.
   Begitu perpindahan tercatat, gerbang alokasinya terbuka kembali.

Yang **tidak** terjadi: sistem tidak memindahkan kantong itu sendiri, dan tidak menaruhnya ke
`PENDING_REVIEW`. `DEC-BD-037` menegaskan keputusan perpindahan fisik adalah kewenangan operasional
BDRS. Sistem menutup gerbang; manusia yang memindahkan barang.

**Kenapa pembagian tugas ini masuk akal.** Kantong yang ada di dalam kulkas rusak tidak berpindah
sendiri hanya karena sebuah penanda diubah di layar — darahnya masih ada di sana secara fisik. Sistem
yang berpura-pura memindahkannya akan berbohong tentang letak barang, dan itu persis kebohongan yang
dihindari `INV-BD-026`. Yang bisa dilakukan sistem dengan jujur hanyalah berhenti menawarkan kantong
itu untuk dialokasikan sampai ada manusia yang benar-benar memindahkannya.

**Catatan untuk penyusunan blueprint: menemukan kantong di lokasi nonaktif.** Supaya petugas dapat
menjalankan perpindahan yang dituntut `DEC-BD-037`, mereka perlu tahu kantong mana saja yang tertahan.
Sama seperti kantong `RECEIVED`, ini persoalan **penyajian daftar kantong** — sebuah penyaring atas
data yang sudah ada, bukan konsep domain baru dan bukan daftar kerja operasional keempat
(`DEC-BD-023`, `AC-BD-057`). Diselesaikan `design-business-module`.

**Contoh gerbang pemberian yang tertutup karena lokasi.** Kantong `PMI-00933` dialokasikan untuk Tn. S
hari Senin, bukti kecocokan tercatat dan masih berlaku. Selasa pagi "Kulkas Besar" ditandai nonaktif
karena pintunya rusak. Selasa siang petugas mencoba memberikan kantong itu — **ditolak**, walaupun
bukti kecocokannya masih hidup dan alokasinya sah. Petugas memindahkan kantong ke "Kulkas Kecil",
perpindahan tercatat, lalu pemberian berhasil. Alokasi Tn. S tidak pernah putus sepanjang kejadian ini,
dan kantong tidak pernah kembali ke `AVAILABLE`.

Perhatikan urutannya: lokasi masih aktif ketika kantong dialokasikan Senin, dan baru dinonaktifkan
Selasa. Bila gerbangnya hanya diperiksa pada saat alokasi, kasus ini lolos begitu saja — dan justru
kasus inilah yang dipersoalkan `OQ-BD-015`. Penilaian ulang pada saat pemberian (`ARCH-BD-POS-07`)
adalah inti `DEC-BD-038`, bukan hiasan.

**Contoh jalur darurat dari lokasi nonaktif.** Keadaan yang sama, tetapi Tn. S mengalami perdarahan
hebat dan darah harus masuk sekarang juga. Peran berwenang menerbitkan otorisasi darurat dengan alasan
yang wajib diisi. Pemberian berjalan, dan rekam menyimpan penanda permanen yang menyebutkan bahwa yang
dilewati adalah gerbang **lokasi nonaktif** — bukan gerbang bukti kecocokan, karena bukti kecocokannya
memang ada dan berlaku (`INV-BD-030`). Perbedaan itu penting bagi siapa pun yang membaca rekam Tn. S
kemudian: darahnya cocok, yang dipertaruhkan adalah kondisi penyimpanannya.

**Contoh gerbang yang tertutup kembali.** Kantong `PMI-00871` dialokasikan untuk Tn. S dan diuji
kecocokan Senin pukul 16.00. Bila masa berlaku dikonfigurasi 48 jam, pemberian Rabu pukul 10.00
berhasil. Bila pemberiannya baru dicoba Kamis pukul 09.00, gerbang menolak: bukti Senin tetap terbaca
pada riwayat, tetapi tidak lagi membukanya, dan petugas harus mencatat bukti baru.

**Contoh pengalihan yang menggugurkan bukti.** Kantong `PMI-00902` sudah diuji kecocokan untuk Tn. S,
lalu Tn. S pulang dan kantong masuk menunggu keputusan. Kantong dialihkan ke Ny. R. Bukti terhadap
Tn. S langsung berhenti membuka gerbang, walaupun golongan darah Tn. S dan Ny. R kebetulan sama —
sistem tidak pernah menilai hal itu (`INV-BD-011`, `INV-BD-013`). Riwayat menyimpan tiga hal
sekaligus: bukti lama milik Tn. S, alasan pelepasan, dan kewajiban bukti baru atas nama Ny. R.
Lokasi penyimpanan kantong tidak ikut berubah karena pengalihan; kantong tetap berada di kulkas yang
sama sampai ada perpindahan yang benar-benar dicatat.

**Contoh pembatalan alokasi.** Petugas mengalokasikan `PMI-00871` ke order Tn. S, lalu menyadari
kantong itu seharusnya untuk Ny. R. Ia membatalkan alokasi dengan alasan "salah pilih kantong".
Barisnya tidak hilang, hanya berhenti aktif, sehingga kantong boleh dialokasikan ulang — dan kantong
kembali ke `AVAILABLE`, bukan ke `STORED`. Bila pada saat itu order Tn. S justru sudah berakhir,
kantong tidak kembali menjadi tersedia melainkan masuk `PENDING_REVIEW` — karena kantong tidak pernah
boleh menjadi stok bebas (`DEC-BD-007`).

**Catatan untuk penyusunan blueprint: kantong yang tertinggal di `RECEIVED`.** Kantong yang sudah tiba
tetapi belum ditaruh berada di `RECEIVED`, dan tidak ada apa pun yang otomatis mengeluarkannya dari
sana. Bukti yang disetujui **tidak** meminta daftar kerja operasional keempat untuk itu — `DEC-BD-023`
menetapkan tiga daftar kerja, dan `AC-BD-057` menegaskan pola bahwa kebutuhan baru tidak otomatis
melahirkan daftar kerja baru. Karena itu dokumen ini tidak menciptakannya. Cara petugas melihat
kantong `RECEIVED` adalah persoalan penyajian daftar kantong, dan diselesaikan
`design-business-module`, bukan dengan menambah konsep domain.

### G.4 Pemeriksaan Golongan Darah

| Dari | Tindakan | Ke | Wewenang | Prasyarat | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | Catat pengambilan sampel | Sampel tercatat | Petugas pengambil | Pasien sah | Waktu, petugas, identifier sampel |
| Sampel tercatat | Catat hasil ABO dan Rhesus | Hasil tercatat | Pemeriksa | — | Pemeriksa, waktu |
| Hasil tercatat | Validasi | Hasil tervalidasi | **Peran validator — `DEF-BD-004`** | — | Validator, waktu |
| Hasil tervalidasi | Muncul hasil tervalidasi baru yang berbeda ABO atau Rhesus-nya | Perbedaan tertahan — `BD-DOM-21` bernilai kosong | Terjadi sebagai akibat, bukan tindakan orang | `BD-XINV-04` mendeteksi perbedaannya | Kedua hasil yang bertentangan, dan sejak kapan tertahan |
| Perbedaan tertahan | Catat pemeriksaan ulang | Perbedaan masih tertahan | Petugas Bank Darah | Sampel baru dan hasil baru tercatat, lalu divalidasi (`DEC-BD-031`) | Sampel baru, hasil baru, pemeriksa, waktu |
| Perbedaan tertahan, ada hasil ulang tervalidasi | Selesaikan perbedaan | Satu golongan darah sah kembali berlaku | **Peran validator — `DEF-BD-004`** | Wajib ada pemeriksaan ulang tervalidasi; validator menyatakannya berlaku (`DEC-BD-031`, `ARCH-BD-GAP-07` ditutup) | Validator, waktu, alasan, pemeriksaan ulang yang memutus, dan hasil-hasil yang diperselisihkan |

**Mekanisme penyelesaian kini pasti.** `DEC-BD-031` menutup pertanyaan yang dulu dibuka `ARCH-BD-GAP-07`: menyelesaikan perbedaan berarti mencatat **pemeriksaan ulang** yang tervalidasi, lalu validator menyatakan hasil ulang itu yang berlaku. Sistem tidak pernah menghitung mayoritas dan tidak menuntut hasil ulang cocok dengan salah satu hasil lama — bila hasil ulang berupa nilai ketiga yang berbeda, ia tetap boleh menjadi hasil sah asalkan validator menyatakannya (`INV-BD-022`).

**Contoh.** Ny. R punya hasil tervalidasi O Positif dari kunjungan Januari. Pada kunjungan Mei, hasil tervalidasi baru menyatakan A Positif. Sejak saat itu `BD-DOM-21` bernilai kosong untuk Ny. R, dan alokasi maupun pemberian yang menuntut golongan darah sah ikut tertahan. Petugas mengambil sampel baru dan mencatat pemeriksaan ulang; hasilnya tervalidasi B Positif. Validator menyatakan hasil ulang ini yang berlaku, dan keadaan konflik ditutup. Ketiga hasil — O Positif, A Positif, B Positif — tetap terbaca penuh. Yang membuka kembali keadaan itu hanyalah pemeriksaan ulang yang dinyatakan validator, bukan waktu dan bukan sekadar hasil berikutnya.

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
| **Menetapkan lokasi penyimpanan kantong yang baru diterima** | Petugas Bank Darah, tanpa peran tambahan | **Ditetapkan `DEC-BD-036`** bersama `DEC-BD-009` |
| **Memindahkan kantong antarlokasi penyimpanan** | Petugas Bank Darah, tanpa peran tambahan | **Ditetapkan `DEC-BD-036`** bersama `DEC-BD-009` |
| **Mengelola master lokasi penyimpanan darah, termasuk menonaktifkan lokasi** | Pengelola Setup Bank Darah, yaitu pemilik proses BDRS | **Ditetapkan `DEC-BD-035`** lewat amandemen `DEC-BD-024` |
| **Memutuskan kapan kantong keluar dari lokasi yang dinonaktifkan** | Petugas BDRS, lewat proses perpindahan lokasi yang biasa. **Bukan** sistem | **Ditetapkan `DEC-BD-037`** — keputusan perpindahan fisik adalah kewenangan operasional BDRS |

Tidak ada kebijakan peran yang dikarang di dokumen ini. Seluruh baris `UNRESOLVED` di atas dibawa
sebagai satu keputusan terkumpul, `DEF-BD-004`.

**Batas wewenang yang ditegaskan `DEC-BD-037`.** Menonaktifkan lokasi dan memindahkan kantong adalah
dua kewenangan yang berbeda, dan `DEC-BD-037` sengaja tidak menyatukannya. Pengelola Setup boleh
menandai sebuah kulkas tidak layak pakai; ia **tidak** dengan itu memerintahkan kantong berpindah.
Perpindahan tetap tindakan tersendiri oleh petugas BDRS, dengan pelaku dan waktunya sendiri pada
riwayat. Sistem tidak pernah menjadi pelaku perpindahan, sehingga tidak pernah ada baris riwayat
perpindahan yang pelakunya bukan manusia.

**Kenapa ketiga baris penyimpanan tidak ikut `UNRESOLVED`.** `DEC-BD-036` menyebut pelakunya
"petugas" tanpa syarat tambahan, dan `DEC-BD-009` sudah menetapkan bahwa penerimaan, alokasi, dan
pemberian dijalankan petugas Bank Darah tanpa gerbang persetujuan. Menaruh kantong ke dalam kulkas
berada di rangkaian pekerjaan yang sama. Pengelolaan masternya mengikuti pola Setup yang sudah ada
pada `DEC-BD-024`. Tidak ada peran baru yang diciptakan di sini, dan tidak ada peran yang
diasumsikan — bila pemilik proses kelak menghendaki pembatasan lebih ketat, itu keputusan mereka lewat
`grill-me`, bukan tambalan dokumen ini.

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
| Pemberian lewat jalur darurat | Seluruh hal di atas, ditambah penanda permanen beserta **gerbang mana yang dilewati** — bukti kecocokan belum ada, lokasi penyimpanan nonaktif, atau keduanya — dan alasannya (`INV-BD-030`) |
| Penolakan pemberian karena lokasi penyimpanan nonaktif | Kantong, pasien tujuan, lokasi yang menutup gerbang, pelaku yang mencoba, dan waktu. Penolakan yang tidak terbaca membuat petugas menyangka sistem rusak, bukan menyangka ada kulkas yang bermasalah |
| Pengalihan kantong ke pasien lain | Pasien asal, alasan pelepasan, dan pasien tujuan — rantai ini tidak pernah putus |
| Hasil golongan darah dan validasinya | Pemeriksa, waktu pemeriksaan, validator, waktu validasi |
| Perubahan data rujukan komponen dan alasan | Pelaku dan waktu |
| Penerimaan kantong yang melebihi jumlah diminta | Jumlah diminta pada saat itu, jumlah yang sudah diterima, kantong mana yang ditandai berlebih, dan alasan terkendalinya |
| Pembatalan alokasi | Alokasi mana yang berhenti aktif, alasan, pelaku, waktu, dan keadaan kantong sesudahnya |
| Gugurnya bukti kecocokan karena pengalihan | Bukti mana yang berhenti berlaku, terhadap pasien siapa bukti itu dibuat, dan pengalihan mana yang menggugurkannya |
| Koreksi pencatatan pemberian | Pemberian asal yang dituju, apa yang keliru, apa yang benar, alasan terkendali, pelaku, dan waktu — pemberian asalnya sendiri tidak boleh berubah sedikit pun |
| Deteksi dan penyelesaian perbedaan hasil golongan darah | Hasil-hasil yang bertentangan, sejak kapan tertahan, validator yang menyelesaikan, waktu, dan alasannya |
| Penetapan lokasi penyimpanan pertama pada kantong | Lokasi yang dipilih, pelaku, waktu, dan perpindahan kantong dari `RECEIVED` ke `STORED` |
| Perpindahan lokasi penyimpanan kantong | Lokasi asal, lokasi tujuan, pelaku, waktu — dan penegasan bahwa status kantong serta catatan penerimaan awalnya tidak berubah (`INV-BD-026`) |
| Perubahan data rujukan lokasi penyimpanan, termasuk penonaktifan | Pelaku, waktu, dan **salinan nama lokasi pada saat kejadian**, supaya penempatan lama tetap terbaca walaupun lokasinya kelak berganti nama atau dinonaktifkan |
| Perpindahan kantong keluar dari lokasi yang dinonaktifkan | Sama seperti perpindahan lokasi lainnya — lokasi asal, lokasi tujuan, pelaku, waktu. **Pelakunya selalu manusia**, tidak pernah sistem (`DEC-BD-037`) |

Riwayat pergerakan (`BD-DOM-15`) hanya dapat ditambah. Tidak ada satu pun jalur bisnis yang boleh
mengubah atau menghapus barisnya, termasuk saat terjadi pembatalan, pengalihan, atau koreksi.

**Tiga hal yang kini tegas bersifat hanya-tambah.** Revisi 2 memperluas sifat itu ke tiga konsep di
luar riwayat pergerakan: alokasi yang dibatalkan tetap tersimpan dan hanya berhenti aktif
(`BD-DOM-06`), catatan koreksi menempel pada pemberian tanpa mengubahnya (`BD-DOM-23`), dan
penyelesaian perbedaan hasil dicatat tanpa menghapus hasil mana pun (`BD-DOM-22`). Ketiganya memakai
kebiasaan yang sudah terbukti berjalan pada `BD-CAP-009`, bukan mekanisme baru.

**Yang keempat, sejak revisi 4: riwayat penempatan.** `INV-BD-026` menempatkan `BD-DOM-25` pada
golongan yang sama. Perpindahan kantong ke kulkas lain **menambah** satu penempatan; ia tidak menimpa
penempatan sebelumnya dan tidak menyentuh catatan penerimaan awal kantong. Konsekuensinya tegas: tidak
ada satu pun jalur bisnis yang boleh "memperbaiki" lokasi kantong dengan cara mengubah catatan lama.
Salah taruh diperbaiki dengan mencatat perpindahan baru, bukan dengan menghapus jejak.

**Kenapa ini penting, bukan sekadar kerapian.** Bila kelak muncul dugaan kantong tersimpan di tempat
yang salah — kulkas rusak, kulkas yang suhunya tidak terjaga — pertanyaan yang harus dijawab adalah
"kantong ini ada di mana saja, sejak kapan sampai kapan". Pertanyaan itu hanya dapat dijawab riwayat
yang tidak pernah ditimpa. Perlu ditegaskan batasnya: riwayat ini menjawab **di mana**, bukan **dalam
kondisi apa** — lihat bagian L.

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

**Revisi 4 tidak menambah satu pun batas integrasi.** `DEC-BD-035` dan `DEC-BD-036` seluruhnya
berjalan di dalam Bank Darah: masternya milik BDRS, penempatannya milik kantong, dan tidak ada pihak
lain yang menghasilkan maupun memakai datanya. Tidak ada sambungan ke Farmasi, tidak ada sambungan ke
alat pemantau suhu, dan tidak ada pertukaran dengan PMI soal lokasi penyimpanan. Pemantauan suhu dan
IoT dikeluarkan `DEC-BD-035` dari MVP, sehingga tidak ada kontrak pihak ketiga yang perlu — apalagi
boleh — dikarang di sini.

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

Yang dulu menyisakan pertanyaan hanya `DEC-BD-030`, dan kini sudah ditutup `DEC-BD-034`. Koreksi nomor
kantong jelas tidak mengubah biaya, karena satu tindakan tetap satu tindakan berapa pun kantong di
dalamnya. Untuk kasus tepi — koreksi yang menyatakan **satu-satunya** pemberian di bawah sebuah
tindakan tidak pernah terjadi — `DEC-BD-034` menetapkan batasnya dari sisi Bank Darah: koreksi
**tidak** membalik fakta biaya secara otomatis. Bank Darah menerbitkan kejadian koreksi; apakah biaya
terlanjur terkirim perlu ditinjau ulang tetap kebijakan Billing, menempel pada `DEC-BD-016`.
`ARCH-BD-GAP-09` ditutup dari sisi Bank Darah tanpa mengarang kebijakan Billing.

### Pemeriksaan ulang pada revisi 4

`DEC-BD-035` dan `DEC-BD-036` diperiksa terhadap batas biaya. **Keduanya tidak menyentuh Billing sama
sekali.** Alasannya tetap sama seperti sebelumnya: biaya lahir dari tindakan Bank Darah, bukan dari
kantong dan bukan dari tempat kantong disimpan (`DEC-BD-021`).

- Menyimpan kantong di kulkas bukan tindakan bertarif, dan `DEC-BD-035` tidak menyebut biaya apa pun.
- Memindahkan kantong antarkulkas tidak mengubah tindakan mana pun, sehingga tidak mengubah fakta
  biaya mana pun.
- Gerbang `INV-BD-025` menahan alokasi. Tindakan yang tertahan berarti belum selesai, sehingga fakta
  biayanya memang belum lahir — pola yang sama persis dengan `DEC-BD-026` dan `DEC-BD-027`.

Klasifikasi bagian K secara keseluruhan **tidak berubah**, dan tetap tertahan pada `DEC-BD-016` untuk
alasan yang sudah ada sebelumnya, bukan alasan baru.

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
| **Alokasi kantong yang belum tersimpan** | Kantong yang belum masuk lokasi penyimpanan dapat berada di mana saja — meja, troli, tangan petugas — dan tidak ada yang tahu sejak kapan | Kantong `RECEIVED` **tidak dapat dialokasikan kepada siapa pun** sampai lokasinya tercatat (`INV-BD-025`). Gerbang ini bersifat *fail-closed*: ragu berarti menahan |
| **Perpindahan kantong antarlokasi** | Kantong yang tidak jelas berada di mana tidak dapat ditelusuri bila kelak ada dugaan penyimpanan yang tidak aman | Setiap perpindahan menambah catatan berisi lokasi asal, lokasi tujuan, pelaku, dan waktu; riwayat tidak pernah ditimpa (`INV-BD-026`) |
| **Alokasi kantong dari lokasi yang dinonaktifkan** | Lokasi dinonaktifkan justru ketika ada yang salah dengannya — rusak, tidak layak, tidak lagi dipercaya | Gerbang alokasi tertutup selama penempatan terakhir kantong menunjuk lokasi nonaktif (`INV-BD-028`). Kantong tidak dipindahkan sistem; petugas BDRS yang memindahkannya ke lokasi aktif lebih dulu (`DEC-BD-037`) |
| **Pemberian kantong yang terlanjur dialokasikan dari lokasi yang dinonaktifkan** | Alokasi bisa terjadi ketika lokasi masih aktif, lalu lokasinya dinonaktifkan sesudahnya. Kantongnya sudah terikat pasien dan bukti kecocokannya sah, sehingga tampak siap diberikan | Gerbang pemberian dinilai **ulang** saat pemberian dicoba dan memuat syarat lokasi aktif (`INV-BD-029`, `ARCH-BD-POS-07`). Jalur darurat `DEC-BD-017` tetap terbuka untuk keadaan klinis yang mendesak, dengan penanda permanen yang menyebutkan sebabnya (`INV-BD-030`) |

**Ketiga pertanyaan keselamatan yang terbuka pada revisi 1 kini sudah dijawab.** Revisi 1 mencatat
tiga hal yang sengaja tidak dikarang: apakah bukti kecocokan punya masa berlaku, apakah pengalihan
menggugurkan bukti sebelumnya, dan hasil mana yang berlaku bila ada lebih dari satu hasil tervalidasi.
Ketiganya ditutup berturut-turut oleh `DEC-BD-027`, `DEC-BD-028`, dan `DEC-BD-026`, dan ketiga
jawabannya bersifat **fail-closed** — ketika ragu, sistem menahan, bukan meneruskan.

Satu keputusan keselamatan baru muncul dan langsung tertutup: `DEC-BD-030` memastikan rekam pemberian
tidak pernah dapat dihapus, sehingga tidak ada jalan bagi sistem untuk menyatakan darah "belum
diberikan" atas darah yang secara fisik sudah masuk ke tubuh pasien.

Keputusan keselamatan revisi 3 memperkuat sisi golongan darah: `DEC-BD-031` menetapkan bahwa keluar
dari keadaan konflik golongan darah **wajib** lewat pemeriksaan ulang tervalidasi, bukan penilaian
sepihak dan bukan hitungan mayoritas oleh sistem. Ini menutup jalan paling berbahaya — melepas
penahanan tanpa bukti pemeriksaan baru — dan tetap *fail-closed*.

**Keputusan keselamatan revisi 4 memperkuat sisi penyimpanan.** `DEC-BD-036` mengubah lokasi
penyimpanan dari keterangan tambahan menjadi **gerbang**. Sebelum keputusan ini, kantong yang baru
diterima langsung dapat dialokasikan tanpa ada yang menjamin ia sudah masuk kulkas. Sesudahnya, jalur
itu tertutup. Arahnya sama dengan seluruh keputusan keselamatan sebelumnya: *fail-closed*.

**Batas yang wajib dibaca jujur: sistem mencatat tempat, bukan kondisi.** `DEC-BD-035` mengeluarkan
pemantauan suhu, IoT, dan kapasitas dari MVP. Konsekuensinya harus dinyatakan terang-terangan supaya
tidak ada yang salah paham di kemudian hari:

- Catatan bahwa kantong berada di "Kulkas Besar" adalah bukti **penempatan**, bukan bukti bahwa rantai
  dinginnya terjaga. Sistem tidak tahu, dan pada MVP memang tidak dirancang untuk tahu, apakah suhu
  kulkas itu pernah menyimpang.
- Karena itu status `STORED` tidak boleh dibaca sebagai pernyataan bahwa kantong aman dipakai. Ia
  hanya menyatakan bahwa kantong sudah punya tempat yang tercatat.
- Penilaian kelayakan kantong tetap sepenuhnya pekerjaan manusia, seperti sudah berlaku pada jalur
  `PENDING_REVIEW` dan `NOT_USABLE`.

**Pertanyaan keselamatan yang dibuka revisi 4 sudah dijawab pemiliknya: `ARCH-BD-GAP-10` tertutup.**
Revisi 4 mencatat satu celah: bukti saat itu hanya mengatur **pilihan ke depan** ketika sebuah lokasi
dinonaktifkan, dan diam soal nasib kantong yang masih berada di dalamnya. Akibatnya, kulkas yang
dinonaktifkan karena rusak tetap menyerahkan darahnya untuk dialokasikan.

`DEC-BD-037` menutup celah itu, dan menutupnya *fail-closed*: selama kantong masih tercatat di lokasi
nonaktif, gerbang alokasinya tertutup (`INV-BD-028`). Arahnya sama dengan seluruh keputusan
keselamatan sebelumnya — ketika ragu, sistem menahan.

Yang sama pentingnya adalah apa yang **tidak** dilakukan `DEC-BD-037`. Sistem tidak memindahkan
kantong, tidak mengubah statusnya, dan tidak menaruhnya ke `PENDING_REVIEW`. Ini bukan kelonggaran,
melainkan pembagian tugas yang jujur: penanda di layar tidak memindahkan darah di dalam kulkas, dan
sistem yang berpura-pura memindahkannya justru menciptakan rekam yang salah tentang letak barang.
Sistem menahan gerbang; petugas BDRS yang memindahkan dan yang menilai kelayakan fisiknya.

**Jangkauan gerbang lokasi nonaktif, setelah `DEC-BD-038`.** Revisi 5 menerapkan `DEC-BD-037` persis
sebatas bunyinya — hanya alokasi — dan membuka `OQ-BD-015` untuk menanyakan jalur pemberian. Pemilik
proses menjawab **memperluasnya**, sehingga jangkauannya kini:

- **Alokasi** — tertutup (`INV-BD-028`).
- **Pengalihan kantong ke pasien lain** — tertutup, karena pengalihan adalah pengikatan kantong ke
  baris kebutuhan pasien, yaitu alokasi dengan nama lain. Ini penurunan dari model yang sudah ada,
  bukan aturan tambahan yang dikarang.
- **Pemberian lewat jalur normal** — tertutup sejak `DEC-BD-038`, dan **dinilai ulang** pada saat
  pemberian dicoba. Pernyataan revisi 5 yang menyatakan sebaliknya sudah dicabut.
- **Pemberian lewat jalur darurat** — tetap terbuka, mengikuti `DEC-BD-017` tanpa perubahan bentuk.

Alasan pemilik proses layak dicatat utuh karena ia menjelaskan batas yang benar: penonaktifan lokasi
dapat menandakan masalah operasional atau fasilitas, sehingga sistem tidak boleh menganggap sebuah
kantong aman hanya karena kantong itu sudah dialokasikan lebih dulu. Sistem tetap **tidak** menentukan
kelayakan darah — batas `INV-BD-013` utuh; yang ditegakkannya adalah status lokasi. Penilaian apakah
darah dari kulkas rusak masih layak tetap milik manusia, lewat `NOT_USABLE` atau lewat otorisasi
darurat.

**Kenapa jalur darurat tidak ditutup sekalian.** Menutup semuanya akan membuat sistem menahan
transfusi darurat karena urusan penanda lokasi, dan itu memindahkan risiko dari satu sisi ke sisi yang
lebih berbahaya. `DEC-BD-038` memilih menahan jalur normal — yang remedinya cepat dan ada di tangan
petugas sendiri: pindahkan kantong ke kulkas aktif, catat perpindahannya, gerbang terbuka — sambil
membiarkan katup darurat berfungsi ketika waktu benar-benar tidak ada. Keduanya *fail-closed* dalam
arti yang tepat: jalan pintas tetap ada, tetapi tidak pernah gratis dan tidak pernah senyap.

**Yang masih tersisa pada sisi keselamatan.** Kembali dua hal, keduanya sempit dan keduanya menahan
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

### Gap yang lahir dari revisi 2 — seluruhnya sudah tertutup pada revisi 3

Tiga temuan berikut muncul saat revisi 2 menyerap `DEC-BD-025` sampai `DEC-BD-030`. Ketiganya
dikembalikan ke `grill-me` dan ditutup pada architecture gap final closure pass 2 September 2026.
Tidak satu pun dijawab sendiri oleh dokumen ini.

| ID | Temuan | Ditutup oleh | Bagaimana arsitektur menyerapnya |
| --- | --- | --- | --- |
| `ARCH-BD-GAP-07` | Arti "menyelesaikan perbedaan" hasil golongan darah | `DEC-BD-031` | `BD-DOM-22` kini wajib menunjuk pemeriksaan ulang tervalidasi; lifecycle `BD-AGG-04` §G.4 punya prasyarat yang dapat diuji; lahir `INV-BD-022` |
| `ARCH-BD-GAP-08` | Tempat menyimpan nilai masa berlaku bukti kecocokan | `DEC-BD-032` | Masa berlaku menjadi atribut per komponen pada `BD-DOM-13`; Setup tidak melebar; lahir `INV-BD-023` |
| `ARCH-BD-GAP-09` | Perlakuan fakta biaya saat koreksi menghapus satu-satunya pemberian | `DEC-BD-034` | Batas Bank Darah ditegaskan: koreksi tidak membalik biaya otomatis; keputusan peninjauan tetap milik Billing lewat `DEC-BD-016`; lahir `INV-BD-024` |

Selain ketiganya, `OQ-BD-013` tempat penyelesaian perbedaan hasil ditutup `DEC-BD-033` (di dalam layar
pemeriksaan golongan darah), dan bagian struktur `OQ-BD-012` ditutup `DEC-BD-032`.

Decision ID pemblokir yang tersisa, dan tidak satu pun menahan `DESIGN` slice yang dinilai:
`DEC-BD-016` kontrak sumber biaya (**di luar** scope) · `OQ-BD-011` mekanik label (**di luar** scope)
· `DEF-BD-003` bukti kecocokan per komponen (`IMPLEMENTATION`) · `DEF-BD-004` peran jalur darurat,
validator, dan pencatat koreksi (`IMPLEMENTATION`) · `OQ-BD-010` kesediaan PMI menerima pengembalian
(tidak memblokir) · `OQ-BD-012` nilai jam masa berlaku per komponen (`IMPLEMENTATION`) · `OQ-BD-014`
keadaan kantong setelah koreksi (`IMPLEMENTATION`).

### Gap yang lahir dari revisi 4 — sudah tertutup pada revisi 5

| ID | Temuan | Ditutup oleh | Bagaimana arsitektur menyerapnya |
| --- | --- | --- | --- |
| `ARCH-BD-GAP-10` | Nasib kantong yang masih berada di dalam sebuah lokasi penyimpanan pada saat lokasi itu dinonaktifkan | `DEC-BD-037` | Lahir `INV-BD-027` (penempatan baru wajib menunjuk lokasi aktif) dan `INV-BD-028` (gerbang alokasi tertutup selama kantong berada di lokasi nonaktif). Lahir `ARCH-BD-POS-06`: gerbang dinilai saat alokasi dicoba, keaktifan lokasi tidak pernah disalin ke kantong. Sistem **tidak** memindahkan kantong dan **tidak** mengubah statusnya — perpindahan tetap tindakan petugas BDRS |

Gap ini dibuka revisi 4 dan dikembalikan ke pemilik proses BDRS. Jawabannya turun sebagai
`DEC-BD-037`, dan **tidak** dijawab sendiri oleh dokumen ini. Dua kemungkinan yang dulu didaftarkan —
kantong otomatis masuk `PENDING_REVIEW`, atau kantong tetap bebas dialokasikan — keduanya ditolak
pemilik proses. Yang dipilih adalah jalan ketiga: gerbang ditutup, barang dipindahkan manusia.

### Pertanyaan yang lahir dari revisi 5 — sudah tertutup pada revisi 6

| ID | Temuan | Ditutup oleh | Bagaimana arsitektur menyerapnya |
| --- | --- | --- | --- |
| `OQ-BD-015` | Apakah gerbang lokasi nonaktif ikut berlaku pada pemberian kantong yang terlanjur dialokasikan sebelum lokasinya dinonaktifkan | `DEC-BD-038` | Gerbang pemberian diperluas menjadi tiga syarat dan **dinilai ulang** saat pemberian dicoba; lahir `INV-BD-029`, `INV-BD-030`, dan `ARCH-BD-POS-07`. Jalur darurat `DEC-BD-017` tetap terbuka, dengan kewajiban menyebutkan gerbang mana yang dilewati. Pernyataan revisi 5 yang menyatakan gerbang tidak berlaku pada pemberian **dicabut** |

Pertanyaan ini dibuka revisi 5 justru karena arsitektur menolak memperluas `DEC-BD-037` diam-diam.
Jawabannya turun dari pemilik proses, dan arahnya berlawanan dengan perilaku yang sempat berlaku —
persis fungsi sebuah pertanyaan terbuka yang jujur.

**Perbandingan lintas revisi.** Revisi 1 membuka enam gap yang menahan dua slice utuh menyangkut
keselamatan pasien; revisi 2 menutup keenamnya dan membuka tiga gap yang jauh lebih ringan; revisi 3
menutup ketiganya tanpa membuka gap baru. Revisi 4 menyerap dua keputusan penyimpanan dan membuka satu
gap keselamatan; revisi 5 menutupnya dan membuka satu pertanyaan tentang jangkauannya; revisi 6
menutup pertanyaan itu. **Tidak ada gap arsitektur maupun pertanyaan keselamatan yang masih terbuka.**
Yang tersisa tetap sama seperti sebelumnya: nilai `IMPLEMENTATION` dan dua slice yang sejak awal berada
di luar scope yang dinilai (penyerahan biaya ke Billing dan mekanik label).

---

## N. Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY`** untuk seluruh scope yang dinilai

Pada revisi 2, dua potongan sempit masih menahan status di `PARTIAL`: satu perpindahan pada
`BD-AGG-04` dan pembekuan atribut `BD-DOM-13`. Keduanya kini tertutup oleh `DEC-BD-031` dan
`DEC-BD-032`, dan `DEC-BD-033` serta `DEC-BD-034` menutup sisa pertanyaan lifecycle dan batas biaya.
Seluruh slice yang dinilai kini memenuhi syarat domain design dan boleh diserahkan ke penyusunan
blueprint.

Revisi 4 **tidak menurunkan** status itu. `DEC-BD-035` dan `DEC-BD-036` datang sebagai keputusan yang
sudah lengkap: kepemilikannya jelas (BDRS, bukan Farmasi), scope-nya dibatasi tegas, lifecycle-nya
disebut lengkap oleh pemilik proses, dan kedua invariant-nya (`INV-BD-025`, `INV-BD-026`) dapat diuji
apa adanya. Satu-satunya pilihan pemodelan yang tersisa — apakah `STORED` dan `AVAILABLE` dipisah —
memang diserahkan `DEC-BD-036` ke sesi ini, dan sudah diputuskan pada `ARCH-BD-POS-04`.

Revisi 4 membuka satu gap keselamatan, `ARCH-BD-GAP-10`, dan revisi 5 menutupnya lewat `DEC-BD-037`.
Dengan itu **tidak ada satu pun gap arsitektur yang masih terbuka**. Gerbang alokasi kini punya
predikat tunggal yang dapat diuji: kantong sudah melewati `STORED`, dan penempatan terakhirnya menunjuk
lokasi yang sedang aktif (`ARCH-BD-POS-06`).

Revisi 6 menyerap `DEC-BD-038` dan menutup `OQ-BD-015`, satu-satunya pertanyaan yang tersisa dari
rangkaian Storage Location. Gerbang pemberian kini punya predikat tunggal yang dapat diuji dan yang
**memuat** predikat alokasi (`ARCH-BD-POS-07`).

Register keputusan sejajar: `00-interview-decisions.md` revisi 7 mencatat `DEC-BD-037` dan
`DEC-BD-038` beserta `INV-BD-027` sampai `INV-BD-030` dan `AC-BD-065` sampai `AC-BD-076`, dengan nomor
yang sama seperti yang dipakai di sini. Traceability arsitektur tidak menggantung pada dokumen ini
saja.

Dua hal yang tetap **di luar** scope yang dinilai — penyerahan fakta biaya ke Billing (`DEC-BD-016`)
dan mekanik label golongan darah (`OQ-BD-011`) — sudah berada di luar scope sejak revisi 1. Keduanya
**bukan** bagian dari kesiapan ini dan tidak menurunkannya; keduanya menunggu keputusan pemiliknya
masing-masing sebagai slice tersendiri.

### Yang boleh diserahkan ke penyusunan blueprint

| Slice arsitektur | Isi | Alasan boleh jalan |
| --- | --- | --- |
| `BD-AGG-01` Order Darah | Order, baris kebutuhan, lifecycle lengkap termasuk kedaluwarsa | Kepemilikan jelas, invariant terwakili, seluruh keputusan bisnisnya sudah turun |
| `BD-AGG-02` Permintaan Darah ke PMI | Permintaan, penerimaan, kelebihan kiriman, lifecycle sampai penutupan administratif | Aturan kelebihan kiriman **kini tertutup** `DEC-BD-025`; invariant sisa dan `BD-XINV-03` sudah dapat diuji |
| `BD-AGG-03` Kantong Darah Operasional | Penerimaan, **penetapan lokasi penyimpanan**, **perpindahan lokasi**, alokasi, **pembatalan alokasi**, bukti kecocokan beserta masa berlakunya, **jalur pemberian**, jalur darurat, **jalur pengalihan**, **catatan koreksi**, keadaan menunggu keputusan, pengembalian ke PMI, dan penetapan tidak layak | Empat keputusan yang menahannya — `DEC-BD-027`, `DEC-BD-028`, `DEC-BD-029`, `DEC-BD-030` — sudah turun. Rantai lima statusnya sudah baku (`DEC-BD-036`). Kedua gerbangnya kini dapat diuji sebagai predikat tunggal: alokasi menuntut sudah `STORED` dan lokasi terakhir aktif (`ARCH-BD-POS-06`), sedangkan pemberian memuat keduanya ditambah bukti kecocokan yang berlaku, dinilai ulang saat pemberian dicoba (`INV-BD-029`, `ARCH-BD-POS-07`) |
| `BD-AGG-04` Pemeriksaan Golongan Darah | Sampel, pencatatan hasil, validasi, deteksi hasil bertentangan, penahanannya, **pemeriksaan ulang, dan penyelesaian konflik** | `DEC-BD-026` menutup aturan hasil mana yang berlaku; `DEC-BD-031` menutup cara melepas penahanan (wajib pemeriksaan ulang tervalidasi). Seluruh lifecycle-nya kini punya prasyarat yang dapat diuji |
| `BD-AGG-05` Tindakan Bank Darah | Pencatatan tindakan beserta konteks dan rujukan tarifnya | Dinyatakan berdiri sendiri dari bagian penyerahan biaya. `DEC-BD-034` menegaskan koreksi tidak membalik biaya otomatis |
| `BD-DOM-13` | Katalog komponen darah, **termasuk atribut masa berlaku bukti kecocokan per komponen** | `DEC-BD-032` menutup pertanyaan tempat simpan; kumpulan atributnya boleh dibekukan. Angka jamnya menyusul dari konfigurasi (`OQ-BD-012`) dan tidak menahan rancangan |
| `BD-DOM-14` | Daftar alasan terkendali | Ditetapkan penuh `DEC-BD-024`. Bertambah kode alasan untuk kelebihan kiriman, pembatalan alokasi, dan koreksi pemberian — penambahan isi daftar, bukan perubahan bentuk |
| `BD-DOM-16`, `BD-DOM-17`, `BD-DOM-21` | Pembaca status kunjungan, ringkasan pemenuhan, dan golongan darah sah pasien | Ketiganya turunan dan tidak menyimpan keadaan sendiri. `BD-DOM-17` kini menghormati catatan koreksi |
| `BD-DOM-18` | Titipan tanda kewenangan unit memesan darah | Ditetapkan penuh `DEC-BD-012`; menunggu pelaksanaan pemilik data induk |
| `BD-DOM-22`, `BD-DOM-23` | Penyelesaian perbedaan hasil dan catatan koreksi pemberian | Identitas, kepemilikan, sifat hanya-tambah, dan kebutuhan auditnya sudah pasti. Mekanisme `BD-DOM-22` kini terkunci `DEC-BD-031` (wajib menunjuk pemeriksaan ulang tervalidasi) |
| `BD-DOM-24` | Master lokasi penyimpanan darah, beserta penanda aktif dan nonaktif dan seluruh akibat penonaktifannya | Kepemilikannya tegas milik BDRS dan penolakan memakai ulang master farmasi sudah diputuskan (`DEC-BD-035`). Kumpulan atributnya boleh dibekukan: hanya identitas lokasi dan penanda aktif, tanpa suhu, kapasitas, maupun penanda farmasi. Akibat penonaktifan kini tertutup penuh `DEC-BD-037` |
| `BD-DOM-25` | Riwayat penempatan kantong, penetapan lokasi pertama, dan perpindahan lokasi | Identitas, batas aggregate, sifat hanya-tambah, dan kewajiban auditnya sudah pasti (`DEC-BD-036`, `INV-BD-026`). Lokasi kantong saat ini dinyatakan sebagai jawaban turunan (`ARCH-BD-POS-05`) |

### Yang tetap di luar scope, dan yang hanya menahan implementasi

Tidak ada lagi slice desain yang berhenti. Yang tersisa terbagi dua: dua slice yang sejak awal **di
luar** scope yang dinilai, dan beberapa nilai yang hanya menahan `IMPLEMENTATION`.

| Yang tersisa | Yang menahan | Sifat |
| --- | --- | --- |
| Penyerahan fakta biaya ke Billing | `DEC-BD-016` | **Di luar** scope sejak revisi 1. `DEC-BD-034` sudah menutup batas Bank Darah-nya; kontrak Billing tetap milik pemilik Billing |
| Mekanik label golongan darah | `OQ-BD-011` | **Di luar** scope sejak revisi 1. Bukan bagian kesiapan ini |
| Penetapan peran pada jalur darurat, validasi hasil, pencatatan koreksi, penyelesaian kantong, dan pembatalan order | `DEF-BD-004` | Menahan `IMPLEMENTATION`, **bukan** rancangan. Bentuk setiap alurnya sudah pasti |
| Nilai jam masa berlaku bukti kecocokan per komponen | `OQ-BD-012` | Menahan `IMPLEMENTATION`. Rancangannya cukup tahu bahwa nilainya datang dari konfigurasi katalog |
| Keadaan kantong yang tercatat keliru sesudah dikoreksi | `OQ-BD-014` | Menahan `IMPLEMENTATION` jalur koreksi. Konsep `BD-DOM-23` tetap dapat dirancang |
| Rumah slice resmi untuk `BR-BD-020` | `02-requirement-completeness-assessment.md` masih revisi 2 | **Tidak menahan `DESIGN`.** Pekerjaan `requirement-completeness-gate`, bukan sesi arsitektur. Sementara ini `BR-BD-020` diperlakukan sebagai perluasan `BD-SLICE-03`, `BD-SLICE-04`, dan `BD-SLICE-10` |

**Kenapa kini `READY`.** Kontrak menuntut, untuk `DOMAIN_ARCHITECTURE_READY`, bahwa slice yang dinilai
layak untuk domain design, bounded context-nya dapat dipertahankan, ownership yang material sudah
terselesaikan, lifecycle dan invariant penting sudah terwakili, keputusan bisnis pemblokir sudah
terselesaikan, serta konsekuensi billing dan keselamatan klinis yang material sudah eksplisit. Seluruh
syarat itu kini terpenuhi untuk scope yang dinilai: tidak ada lagi perpindahan tanpa prasyarat yang
dapat diuji, tidak ada lagi kumpulan atribut yang belum boleh dibekukan, dan tidak ada ownership yang
harus dikarang. Yang tersisa hanyalah nilai `IMPLEMENTATION` dan dua slice yang memang di luar scope.

**Batas kejujuran kesiapan ini.** `READY` berlaku untuk **scope yang dinilai**, bukan untuk seluruh
modul Bank Darah. Penyerahan biaya ke Billing dan mekanik label golongan darah tetap menunggu
pemiliknya, persis seperti sejak revisi 1. Menyatakan `READY` di sini bukan berarti kedua slice itu
ikut siap.

---

## O. Handoff

```yaml
blueprint_id: BD-BP-001
blueprint_revision: 6
domain_architecture_revision: 6
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
domain_architecture_scope:
  siap:
    - BD-AGG-01
    - BD-AGG-02 (termasuk kelebihan kiriman)
    - BD-AGG-03 (penerimaan, penetapan lokasi penyimpanan, perpindahan lokasi, alokasi, pembatalan alokasi, bukti kecocokan beserta masa berlakunya, pemberian, jalur darurat, pengalihan, catatan koreksi, menunggu keputusan, kembali ke PMI, tidak layak)
    - BD-AGG-04 (sampel, pencatatan hasil, validasi, deteksi dan penahanan hasil bertentangan, pemeriksaan ulang, penyelesaian konflik)
    - BD-AGG-05 (pencatatan tindakan saja)
    - BD-DOM-13 (termasuk atribut masa berlaku bukti kecocokan per komponen)
    - BD-DOM-14
    - BD-DOM-16
    - BD-DOM-17
    - BD-DOM-18
    - BD-DOM-21
    - BD-DOM-22
    - BD-DOM-23
    - BD-DOM-24 (master lokasi penyimpanan darah, aktif/nonaktif, beserta akibat penonaktifan)
    - BD-DOM-25 (riwayat penempatan kantong, penetapan lokasi pertama, perpindahan lokasi)
  di_luar_scope:
    - penyerahan fakta biaya ke Billing (DEC-BD-016)
    - mekanik label golongan darah (OQ-BD-011)
blood_unit_lifecycle: [RECEIVED, STORED, AVAILABLE, ALLOCATED, ISSUED]
blood_unit_branch_states: [PENDING_REVIEW, REALLOCATED, RETURNED_TO_PROVIDER, NOT_USABLE]
architecture_positions: [ARCH-BD-POS-01, ARCH-BD-POS-02, ARCH-BD-POS-03, ARCH-BD-POS-04, ARCH-BD-POS-05, ARCH-BD-POS-06, ARCH-BD-POS-07]
cross_aggregate_invariants: [BD-XINV-01, BD-XINV-02, BD-XINV-03, BD-XINV-04]
aggregate_local_invariants_baru: [INV-BD-025, INV-BD-026, INV-BD-027, INV-BD-028, INV-BD-029, INV-BD-030]
requirement_readiness: PARTIALLY_READY
requirement_evidence_status: seluruh slice yang masuk CONFIRMED; tidak ada CONFLICT tersisa
capability_scope: [BD-SLICE-01, BD-SLICE-02, BD-SLICE-03, BD-SLICE-04, BD-SLICE-05, BD-SLICE-06, BD-SLICE-07, BD-SLICE-08, BD-SLICE-09, BD-SLICE-10]
capability_scope_catatan: BR-BD-020 diperlakukan sebagai perluasan BD-SLICE-03, BD-SLICE-04, dan BD-SLICE-10; rumah slice resminya menunggu requirement-completeness-gate
blocking_design_decision_ids: []
implementation_only_open_ids: [DEF-BD-003, DEF-BD-004, OQ-BD-010, OQ-BD-012, OQ-BD-014]
out_of_scope_blocked_ids: [DEC-BD-016, OQ-BD-011]
closed_gap_ids: [ARCH-BD-GAP-01, ARCH-BD-GAP-02, ARCH-BD-GAP-03, ARCH-BD-GAP-04, ARCH-BD-GAP-05, ARCH-BD-GAP-06, ARCH-BD-GAP-07, ARCH-BD-GAP-08, ARCH-BD-GAP-09, ARCH-BD-GAP-10, OQ-BD-015]
open_architecture_gap_ids: []
blood_unit_gates:
  alokasi: sudah STORED DAN penempatan terakhir menunjuk lokasi aktif (ARCH-BD-POS-06)
  pemberian: gerbang alokasi DITAMBAH bukti kecocokan berlaku untuk pasien tujuan dan belum lewat masa berlaku; dinilai ulang saat pemberian dicoba (ARCH-BD-POS-07)
  jalur_darurat: DEC-BD-017 melewati gerbang pemberian; wajib menyebutkan gerbang mana yang dilewati (INV-BD-030)
dependency_ids: [BD-DEP-001, BD-DEP-002, BD-DEP-003, BD-DEP-004, BD-DEP-005, BD-DEP-006, BD-DEP-007, BD-DEP-010, BD-DEP-011, BD-DEP-012, BD-DEP-013, BD-DEP-014, BD-DEP-016]
resolved_dependency_ids: [BD-DEP-008]
decision_revision: 7
decision_approval_status: DEC-BD-035 sampai DEC-BD-038 masih draft, approved_by kosong — sama seperti DEC-BD-001..034
superseded_statements: revisi 5 menyatakan gerbang lokasi nonaktif tidak berlaku pada pemberian; dicabut oleh DEC-BD-038 pada revisi 6
decision_register_revision: 7
decision_register_catatan: DEC-BD-037, DEC-BD-038, INV-BD-027..030, dan AC-BD-065..076 sudah tercatat resmi pada 00-interview-decisions.md revisi 7 dengan nomor yang sama
amended_decision_ids: [DEC-BD-024]
capability_audit_coverage_gap: BD-CAP-006 tidak menyebut MstDrugStorageLocation; tidak mengubah kesimpulan karena DEC-BD-035 menolak memakai ulang master itu
backend_source_sha: 792acb9331a65187d052fffd4a292d3bce2fd828
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
| BR-BD-011 — penyelesaian konflik hasil | `DEC-BD-031` | `BD-DOM-22`, `BD-AGG-04`, `INV-BD-022` |
| BR-BD-011 — masa berlaku per komponen | `DEC-BD-032` | `BD-DOM-13`, `INV-BD-023` |
| BR-BD-011 — layar penyelesaian konflik | `DEC-BD-033` | `BD-AGG-04`, `FE-BD-009` |
| BR-BD-004 — batas koreksi terhadap biaya | `DEC-BD-034` | `BD-AGG-05`, `BD-DOM-19`, `INV-BD-024` |
| BR-BD-020 — master lokasi penyimpanan darah | `DEC-BD-035`, `DEC-BD-024` (diamandemen) | `BD-DOM-24`, `BD-CTX-10` sebagai tetangga yang tidak disentuh |
| BR-BD-020 — gerbang kesiapan penyimpanan kantong | `DEC-BD-036` | `BD-AGG-03`, `BD-DOM-05`, `INV-BD-025`, `ARCH-BD-POS-04` |
| BR-BD-020 — perpindahan lokasi yang hanya bertambah | `DEC-BD-036` | `BD-DOM-25`, `INV-BD-026`, `ARCH-BD-POS-05` |
| BR-BD-020 — akibat penonaktifan lokasi penyimpanan | `DEC-BD-037` | `BD-DOM-24`, `BD-AGG-03`, `INV-BD-027`, `INV-BD-028`, `ARCH-BD-POS-06` |
| BR-BD-007, BR-BD-020 — gerbang pemberian dan lokasi nonaktif | `DEC-BD-038`, `DEC-BD-017` | `BD-AGG-03`, `BD-DOM-08`, `INV-BD-029`, `INV-BD-030`, `ARCH-BD-POS-07` |

### Langkah berikutnya

Seluruh scope yang dinilai berstatus `DOMAIN_ARCHITECTURE_READY` dan siap dikirim ke
`design-business-module`. **Tidak ada satu pun gap arsitektur yang masih terbuka:** ketiga gap revisi 2
ditutup `DEC-BD-031` sampai `DEC-BD-034`, dan `ARCH-BD-GAP-10` yang dibuka revisi 4 ditutup
`DEC-BD-037` pada revisi 5.

**Yang perlu dikerjakan hilir, akibat revisi 4.**

| Artefak | Yang perlu diserap | Pemiliknya |
| --- | --- | --- |
| `02-backend-architecture.md`, `data/data-dictionary.md`, `contracts/` | `BD-DOM-24` master lokasi beserta penanda aktifnya, `BD-DOM-25` riwayat penempatan, status `RECEIVED`/`STORED`/`AVAILABLE`/`ALLOCATED`/`ISSUED`, gerbang alokasi tunggal (`INV-BD-025` + `INV-BD-028`), serta alur penetapan dan perpindahan lokasi | `design-business-module` |
| `03-frontend-architecture.md` | Layar Setup untuk master lokasi termasuk penonaktifan, penetapan lokasi pada penerimaan, perpindahan lokasi, dan **penyaring untuk menemukan kantong yang tertahan di lokasi nonaktif**. Tidak ada daftar kerja operasional keempat | `design-business-module` |
| `contracts/state-transition-matrix.md` | Rantai lima status, baris perpindahan lokasi yang **tidak** mengubah status, dan prasyarat lokasi aktif pada baris alokasi serta pengalihan | `design-business-module` |
| `contracts/validation-matrix.md` | Penolakan penempatan ke lokasi nonaktif (`INV-BD-027`), penolakan alokasi dari lokasi nonaktif (`INV-BD-028`), dan penolakan **pemberian** dari lokasi nonaktif pada jalur normal (`INV-BD-029`) | `design-business-module` |
| `contracts/permission-audit-matrix.md` | Kewajiban otorisasi darurat menyebutkan gerbang mana yang dilewati (`INV-BD-030`), dan pencatatan penolakan pemberian karena lokasi nonaktif | `design-business-module` |
| `roadmap/00-delivery-plan.md` | Storage Location keluar dari coverage gap dan masuk task berurutan; task alokasi kini bergantung pada task penyimpanan | `plan-module-delivery` |
| `02-requirement-completeness-assessment.md` | Rumah slice resmi untuk `BR-BD-020` | `requirement-completeness-gate` |
| `blueprint-manifest.md`, `MODULE-STATUS.md` | `domain_architecture_revision` 6, `decision_revision` 7, daftar blocker, serta penutupan `ARCH-BD-GAP-10` dan `OQ-BD-015` | `manage-module-blueprint` |

**Pencatatan register sudah selesai.** `DEC-BD-037`, `DEC-BD-038`, `INV-BD-027` sampai `INV-BD-030`,
dan `AC-BD-065` sampai `AC-BD-076` tercatat resmi pada `00-interview-decisions.md` revisi 7. Tidak ada
lagi ID sementara maupun pertanyaan terbuka pada dokumen ini.

Dua slice lama yang masih di luar scope tetap seperti sebelumnya: penyerahan biaya ke Billing
(`DEC-BD-016`) dan mekanik label golongan darah (`OQ-BD-011`). Keduanya tidak menahan slice yang sudah
siap.

**Yang menunggu pemilik proses BDRS.** `DEC-BD-035` sampai `DEC-BD-038` masih `draft` pada register.
Arsitektur ini menyerapnya dengan perlakuan yang sama seperti `DEC-BD-001` sampai `DEC-BD-034`, dan
**tidak** menandai persetujuan atas nama siapa pun.

Yang **tidak** perlu diulang: `trace-existing-capabilities`. Peta kemampuan masih sahih secara isi
karena tidak ada berkas source aplikasi yang berubah antara `9522caa` dan `792acb9` (seluruh
perbedaannya dokumen blueprint). Satu celah **cakupan** memang ditemukan — `BD-CAP-006` tidak menyebut
`MstDrugStorageLocation` yang sebenarnya ada di `Areas/HealthServices/MasterData/Models/`. Celah itu
sudah diperiksa langsung terhadap sumbernya pada pass ini dan **tidak mengubah kesimpulan mana pun**,
karena `DEC-BD-035` justru menolak memakai ulang master tersebut. Menjalankan ulang audit kemampuan
hanya untuk mencatat satu master yang sudah diputuskan tidak dipakai bukan penggunaan waktu yang
sepadan; pencatatannya cukup lewat `manage-module-blueprint`. Pola yang dipakai — token konkurensi
`BD-CAP-010` dan riwayat hanya-tambah `BD-CAP-009` — keduanya sudah tercatat pada audit yang ada, dan
riwayat penempatan `BD-DOM-25` memakai pola yang sama persis.
