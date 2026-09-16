# Integration Contract — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.6.0`** |
| `last_changed_in` | **`0.6.0`** — bagian 12 lahir: `INT-DOK-11` s.d. `INT-DOK-22`. Kontrak ini melompati `0.5.0` karena isinya tidak bergerak pada Gelombang 1A |
| Status | **`draft`** untuk `0.6.0`. `0.4.0` **`approved`** — disetujui Muhammad Hamzah, 2026-09-09 |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-09** untuk `0.4.0`; `0.3.0` disetujui 2026-09-03 |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2` bagian X |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.4.0`: `INT-DOK-10` lahir — pelonggaran nomor konsultasi pada diagnosis terstruktur. Penomoran `INT-DOK-01` s.d. `INT-DOK-09` **tidak bergerak**; tabel pemetaan dari `0.1.0` tetap di bagian 0.2 |
| Tanggal | 2 September 2026; diamendemen 9 September 2026 |

---

## 0. Kenapa dokumen ini menentukan

Sub-modul ini tidak memiliki satu tabel pun dan menyentuh **enam** modul lain:
`ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement`,
`MedicalRecordManagement`, dan `BillingManagement`. Hampir seluruh wujudnya adalah integrasi.

### 0.1 Penomoran kanonis

`INT-DOK-01` s.d. `INT-DOK-07` **diambil apa adanya** dari arsitektur domain bagian X.1 dan tidak
diturunkan ulang di sini. `INT-DOK-08`, `INT-DOK-09`, dan `INT-DOK-10` adalah tambahan tingkat
desain yang tidak punya padanan domain. `INT-DOK-10` lahir pada `0.4.0`.

### 0.2 Pemetaan dari penomoran `0.1.0`

Pembaca dokumen lama membutuhkan tabel ini sekali saja.

| Nomor pada `0.1.0` | Isinya | Nomor sekarang |
| --- | --- | --- |
| `INT-DOK-01` | Pelonggaran konteks klinis pada konsultasi | `INT-DOK-01` — **tetap** |
| `INT-DOK-02` | Pelonggaran batas jumlah konsultasi dan resep | `INT-DOK-02` — **tetap** |
| `INT-DOK-03` | Konteks episode dan kewenangan DPJP | **`INT-DOK-08`** |
| `INT-DOK-04` | Berbagi enum dan tabel dengan `keperawatan` | **`INT-DOK-09`** |
| `INT-DOK-05` | Resep ke Farmasi | **`INT-DOK-06`** |
| `INT-DOK-06` | Pesanan dan hasil laboratorium | **`INT-DOK-04`** |
| `INT-DOK-07` | Radiologi | **`INT-DOK-05`** |
| `INT-DOK-08` | Pemicu tagihan tindakan | **`INT-DOK-03`** |
| — | Integritas dan koreksi dokumen | **`INT-DOK-07`** — baru |

---

## 1. `INT-DOK-01` — Konteks klinis episode ★ penghalang utama

| Field | Isinya |
| --- | --- |
| Produsen | `CTX-INP-CARE` — Rawat Inap, pemilik makna "episode ini sedang berjalan" |
| Konsumen | `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement` |
| Tujuan bisnis | Dokumen klinis menemukan episode yang benar **tanpa antrean semu** |
| Sumber kebenaran | `CTX-INP-CARE` |
| Arah | Baca |
| Sifat | Sinkron, di dalam proses yang sama |
| Bentuk | Satu **service konteks klinis bersama** yang mewujudkan `CON-INP-015`: pasien, kunjungan, episode beserta statusnya, dan kewenangan dokter |
| Pemilik perubahan | `ClinicalManagement` — Muhammad Hamzah, disetujui `RWI-DEC-062` |
| Status pada source | **`Missing`** — `DOK-TRC-INT-01`. Pencarian `InpEpisode` pada `DoctorConsultationController` dan `PatientAssessmentController` nihil |
| Hubungan dengan `keperawatan` | **Kembaran.** `INT-KEP-01` meminta hal yang sama pada jalur pengkajian. Keduanya **wajib dikerjakan bersama** — lihat 1.2 |
| Yang **tidak** berubah | Perilaku rawat jalan dan medical check-up; nol kolom untuk pelonggaran ini |
| Bila gagal | Permintaan ditolak `422`; tidak ada keadaan setengah jadi |
| Traceability | PRD 30.3; `RWI-DEC-062`, `RWI-DEC-070`, `RWI-DEC-080`; `INV-DOK-01` s.d. `INV-DOK-03` |

### 1.1 Perbaikan yang menyertainya — `DOK-TRC-DEF-01` ★ baru pada `0.2.0`

| Field | Isinya |
| --- | --- |
| Apa | Cabang tanpa antrean pada pembuatan catatan dokter mengambil data antrean yang **boleh kosong**, lalu menulis ke dalamnya tanpa memeriksa |
| Bukti | `BE@93b3227 DoctorConsultationController.cs` baris 258–265 dan 360–366 |
| Akibat | Kegagalan sistem — kode `500` — pada jalur yang justru dipakai pasien rawat inap dan IGD |
| Urutan | **Diperbaiki sebelum atau bersamaan** dengan penyalaan cabang episode |
| Bukti selesai | Test regresi IGD dan test jalur rawat inap tanpa antrean, keduanya hijau |

> Membuka cabang episode di atas jalur yang sudah diketahui gagal berarti mengundang pasien rawat
> inap ke dalam kegagalan yang sudah kita ketahui sebelumnya.

### 1.2 Kenapa kedua cabang wajib bersama

| Bila hanya pengkajian dibuka | Bila hanya konsultasi dibuka |
| --- | --- |
| Perawat dapat mencatat; **dokter tidak dapat sama sekali** — SOAP, diagnosis, resep, dan tindakan seluruhnya lahir dari catatan dokter | Dokter dapat mencatat; perawat tidak. Pengkajian awal keperawatan tetap di kertas |

Keduanya adalah satu pekerjaan yang kebetulan berada di dua berkas. Memecahnya menjadi dua
gelombang menghasilkan setengah ruang kerja klinis yang tidak dapat dipakai siapa pun.

---

## 2. `INT-DOK-02` — Pelonggaran batas jumlah catatan dan resep

| Field | Isinya |
| --- | --- |
| Produsen dan konsumen | `ClinicalManagement` dan `PharmacyManagement` mengubah aturan internal mereka sendiri |
| Yang diminta | Untuk kunjungan bertipe `Inpatient` dan `Emergency`: batas **satu catatan dokter per kunjungan** dan **satu resep aktif per catatan** tidak berlaku |
| Dasarnya | `RWI-RULE-026` aturan 4 dan 5; `RWI-DEC-038`, diperluas `RWI-DEC-070` |
| Keadaan keputusan | **`approved` sejak 2026-08-21.** Yang belum ada kodenya |
| Status pada source | **`Extend`** — `DOK-TRC-INT-02`. Penolakan masih ada pada `DoctorConsultationController` sekitar baris 844–850 dan 916–923, serta `PrescriptionController` sekitar baris 555–563 |
| Kenapa wajib | Pasien dirawat berhari-hari. Tanpa pelonggaran ini dokter hanya dapat menulis **satu** catatan dan **satu** resep untuk seluruh masa perawatan |
| Yang **tidak** berubah | Rawat jalan dan medical check-up tetap dibatasi seperti sekarang — `INV-DOK-05`, dibuktikan `RWI-AC-143` |
| Bukti selesai | Catatan dan resep kedua pada kunjungan rawat inap diterima; catatan dan resep kedua pada kunjungan rawat jalan **tetap ditolak dengan pesan yang sama persis** |

> **Dua pelonggaran ini satu paket.** Resep **wajib** menempel pada satu catatan dokter. Selama
> catatan kedua ditolak, dokter tidak punya tempat sah menggantungkan resep kedua — melonggarkan
> aturan resep saja tidak menghasilkan apa pun.

---

## 3. `INT-DOK-03` — Fakta klinis tindakan ke Billing

| Field | Isinya |
| --- | --- |
| Produsen | `ClinicalManagement` |
| Konsumen | `BillingManagement` |
| Tujuan bisnis | Menyatakan bahwa satu tindakan medis benar-benar dikerjakan |
| Sumber kebenaran | `CTX-CLI` untuk fakta klinis; `CTX-BIL` untuk keputusan finansial |
| Arah | **Tulis satu arah.** Tidak ada jalur balik yang mengizinkan Billing mengubah catatan klinis |
| Keadaan pada source | **Sudah ada** — `CliClinicalMilestoneFact` beserta producer, kunci idempotency dari identitas dan versi fakta, serta tujuh kemungkinan hasil penerbitan |
| Kepedulian idempotency | Kiriman identik dijawab dengan hasil yang sama, bukan tagihan kedua |
| Bila gagal | Catatan klinis **tetap tersimpan**; penerbitan masuk daftar percobaan ulang |
| Rekonsiliasi | Bila keadaan sebelumnya tidak diketahui, wajib rekonsiliasi sebelum koreksi finansial |
| Yang **tidak** diminta | Kolom status pengiriman tersendiri. Hasil penerbitan sudah menjawabnya |

### 3.1 Kenapa kunjungan sudah cukup, dan episode tidak perlu ditambahkan

Bentuk fakta klinis hari ini membawa `EncounterId`, bukan episode. Itu **tidak perlu diubah**:
`INV-INP-04` menjamin satu episode menempel pada tepat satu kunjungan, sehingga Billing selalu
dapat menurunkan episodenya. Menambah kolom episode pada kontrak milik modul lain berarti mengubah
kontrak yang sudah berjalan tanpa memperoleh informasi baru.

---

## 4. `INT-DOK-04` — Pesanan dan hasil laboratorium

| Field | Isinya |
| --- | --- |
| Produsen | `LaboratoryManagement` untuk hasil; Rawat Inap sebagai pemesan |
| Arah | **Tulis** pesanan; **baca** status dan hasil final terverifikasi |
| Sumber kebenaran | `CTX-LAB` |
| Keadaan modul tujuan | **Ada dan berjalan** — pesanan, spesimen, riwayat transisi, dua controller. Prefix `Lab` berstatus `ACTIVE` sejak 2026-09-02 |
| Temuan | Pesanan sudah terikat kunjungan tanpa gerbang antrean, sehingga pemesanan lab rawat inap **sudah mungkin hari ini**. Yang kurang: **daftar pesanan belum dapat disaring per kunjungan** |
| Yang diminta | Penanda episode pada pesanan, dan penyaring kunjungan pada daftar |
| Yang **dilarang** | Menulis maupun menyalin hasil — `RUL-DOK-02`, `AC-CAP015-02` |
| Bila gagal | Ruang kerja menampilkan keadaan apa adanya; tidak menebak dan tidak menyalin |

---

## 5. `INT-DOK-05` — Pesanan dan hasil radiologi ★ berubah total dari `0.1.0`

| Field | Isinya |
| --- | --- |
| Keadaan modul tujuan pada `0.1.0` | Dinyatakan **tidak ada** |
| Keadaan sebenarnya pada `BE@93b3227` | **Ada dan berjalan** — `RadOrder`, `RadStudy`, modalitas, lifecycle pesanan lengkap, migration `20260828093000_AddRadiologyManagement`, dan **penyaring kunjungan sudah tersedia** pada daftar |
| Arah | **Tulis** pesanan; **baca** status, studi, dan hasil final |
| Sumber kebenaran | `CTX-RAD` |
| Yang diminta | Penanda episode pada pesanan |
| Prasyarat | Baris registry `RadiologyManagement / Rad` masih `PLANNED` padahal entity-nya sudah ada. **Selisih ini dilaporkan** dan sebaiknya dinaikkan menjadi `ACTIVE` oleh pemiliknya |
| Akibat bagi MVP | `CAP-015` **masuk MVP penuh**, bukan lagi sebagian |

---

## 6. `INT-DOK-06` — Resep ke Farmasi

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** pesanan resep; **baca** status pemenuhannya |
| Sumber kebenaran | `CTX-PHM` |
| Keadaan modul tujuan | **Lengkap dan berjalan** — resep, item, racikan, review, penyiapan, ruang kerja farmasi |
| Idempotency | Wajib; kiriman ulang mengembalikan resep yang sama |
| Yang **dilarang** | Menulis status pemenuhan apa pun — `RUL-DOK-01` |
| Obat pulang | Dikirim sebagai **jenis resep**, bukan sebagai daftar terpisah milik Rawat Inap — `RWI-DEC-046` |
| Pembacaan balik | Status penyerahan dibaca untuk menutup butir daftar periksa administrasi `RWI-RULE-018`. **Bukan** ditulis oleh Rawat Inap |
| Bila gagal | Resep tidak terbentuk; dokter melihat penolakan dan dapat mengulang dengan kunci yang sama |

---

## 7. `INT-DOK-07` — Integritas dan koreksi dokumen ★ baru pada `0.2.0`

| Field | Isinya |
| --- | --- |
| Produsen dan pemilik | `MedicalRecordManagement` |
| Konsumen | `ClinicalManagement` dan ruang kerja dokter |
| Tujuan bisnis | Menandatangani, mengunci, dan **mengoreksi** dokumen klinis tanpa menimpa isi aslinya |
| Keadaan modul tujuan | **Sudah ada dan sudah dipakai** — integritas dokumen, addendum bernomor urut beserta alasan koreksi, dan pendelegasian penulis |
| Yang diminta | **Nol perubahan model**, tetapi **satu perubahan perilaku**: tiga jenis dokumen didaftarkan ke mesin keutuhan saat finalisasi — lihat 7.1 |
| Akibat bagi desain | Seluruh rancangan kolom amandemen per tabel pada `0.1.0` **tetap dicabut**. Yang ditambahkan `0.3.0` adalah langkah pendaftarannya |

### 7.1 Pertanyaan `0.2.0` sudah terjawab, dan jawabannya mengubah arah

`0.2.0` menitipkan satu pertanyaan: apakah dokumen terkunci tetap menerima koreksi? Pembacaan source
menjawabnya lebih tegas dari dugaan — koreksi **hanya** diterima pada dokumen terkunci, dan dokumen
berstatus konsep justru ditolak dengan arahan memperbaiki langsung pada catatannya (`RWI-FACT-013`).

Pada saat yang sama ditemukan celah yang lebih serius (`RWI-FACT-014`): **hanya catatan terpadu yang
terdaftar** pada mesin keutuhan, sedangkan penyuntingan catatan dokter setelah selesai sudah
dilarang. Gabungan keduanya membuat catatan dokter yang sudah diselesaikan tidak dapat disunting
maupun dikoreksi.

### 7.2 Yang diminta sekarang

| Hal | Isinya |
| --- | --- |
| Pendaftaran | Catatan dokter, kajian medis, dan tindakan didaftarkan ke mesin keutuhan **saat finalisasi**, dalam transaksi yang sama |
| Status pendaftaran | Tertanda tangan, dengan penulis dokumen sebagai penanda tangan — `RWI-DEC-086` |
| Jenis dokumen | Memakai nilai yang sudah tersedia. **Nol nilai enum baru** |
| Bila pendaftaran gagal | **Finalisasi ikut batal.** Tidak boleh lahir catatan final yang tidak dapat dikoreksi |
| Penetapan penulis pengganti | Dipakai apa adanya. Penerbitnya kepala unit rawat inap, wajib berbatas waktu — `RWI-DEC-088` |
| Yang tetap tidak berubah | Mesin keutuhan, mesin addendum, dan mesin penetapan itu sendiri |

### 7.3 Batas yang tidak dapat dijaga kontrak ini

Penetapan berhalangan bersifat **milik penulis**, bukan milik penggantinya — ia tidak menyebut siapa
yang boleh menggantikan. Karena itu pembatasan `RWI-DEC-088` bahwa hanya **DPJP aktif episode itu**
yang boleh mengoreksi **tidak dapat** ditegakkan oleh `MedicalRecordManagement`, dan memang tidak
seharusnya: modul itu tidak mengenal episode rawat inap, dan membuatnya mengenal episode berarti
menariknya masuk ke urusan modul lain.

Batas itu karena itu dijaga di sisi Rawat Inap sebagai kewenangan per pasien, sejalan `INV-DOK-13`.
Penempatan persisnya ada pada `contracts/permission-audit-matrix.md` bagian 3.

---

## 8. `INT-DOK-08` — Konteks episode dan kewenangan DPJP

| Field | Isinya |
| --- | --- |
| Arah | **Baca** dari `episode-rawat-inap` |
| Yang dibaca | Census pasien per dokter, identitas pasien, lokasi, status episode, dan **penugasan DPJP yang berlaku pada tanggal itu** |
| Kenapa berperiode | DPJP dapat berganti di tengah perawatan. Kewenangan menulis pada tanggal tertentu ditentukan penugasan yang berlaku **pada tanggal itu**, bukan penugasan terkini |
| Keadaan pada source | **`Ready to reuse`** — `DOK-TRC-CTX-01`. Census sudah dapat disaring per dokter; pemeriksaan dokter aktif per episode sudah tersedia dan sudah dipakai jalur perpindahan serta pemulangan |
| Arah tulis | **Tidak ada.** Sub-modul ini tidak pernah mengubah episode maupun penugasan |
| Bila gagal | Ruang kerja menampilkan keadaan gagal; **seluruh tombol tulis nonaktif** |

---

## 9. `INT-DOK-09` — Koordinasi dengan sub-modul `keperawatan`

Bukan integrasi antar modul, melainkan koordinasi antar sub-modul. Dicatat di sini karena tidak ada
berkas lain yang memergokinya.

| Yang dibagi | Diminta lebih dulu oleh | Yang harus dilakukan sub-modul kedua |
| --- | --- | --- |
| Jenis kajian pada `PatientAssessmentType` | `keperawatan` | Menambah nilai kajian medis, **bukan** membuat enum kedua |
| Kolom `InpEpisodeId`, `DueAt`, `PolicyId` pada tabel pengkajian | `keperawatan` | Memakai apa adanya. **Tidak meminta duplikatnya** |
| Kebijakan batas waktu kajian | `keperawatan` | Menambah baris kebijakan untuk jenis kajian medis |
| **Service konteks klinis bersama** | Keduanya | **Satu service, bukan dua.** Siapa pun yang mendarat lebih dulu membuatnya |
| Lembar CPPT | Dipakai keduanya | **Kontraknya milik sub-modul ini** (`CAP-021`). `keperawatan` menulis sebagai penulis, bukan pemilik kontrak |

> **Siapa pun yang mendarat lebih dulu membuat, yang kedua menambah.** Bila keduanya dikerjakan
> berbarengan, `INT-DOK-09` wajib dibaca kedua pelaksana supaya tidak lahir dua enum kembar dan dua
> service konteks yang berselisih.

---

## 10. `INT-DOK-10` — Pelonggaran nomor konsultasi pada diagnosis terstruktur ★ baru pada `0.4.0`

| Field | Isinya |
| --- | --- |
| Produsen dan konsumen | `ClinicalManagement` mengubah aturan internalnya sendiri |
| Yang diminta | Untuk kunjungan bertipe `Inpatient`: diagnosis terstruktur boleh menyebut **perawatan rawat inap** sebagai konteks, sehingga **nomor konsultasi tidak lagi wajib**. Kolom `ConsultationId` pada `TrxPatientDiagnosis` menjadi boleh kosong, dan kolom konteks `InpEpisodeId` ditambahkan |
| Dasarnya | `PRD-RWI-FINAL-001` `CAP-022` aturan 2 dan aturan 5; temuan `FE-RWI-044`; wewenang lintas modul `RWI-DEC-062` |
| Keadaan keputusan | **`approved` 2026-09-09.** Tidak ada keputusan bernomor tersendiri: `RWI-DEC-062` sudah memberi persetujuan atas perubahan lintas modul **yang dituntut blueprint ini**, dan `0.4.0` inilah yang menjadikannya dituntut. **Approval `0.4.0` oleh Muhammad Hamzah pada 2026-09-09 adalah tanda tangan itu** — lihat 10.1. Yang belum ada kodenya |
| Status pada source | **`Extend`** — `PatientDiagnosisDtos.cs` baris 148–152 menandai `EncounterId` dan `ConsultationId` keduanya `[Required]`; `TrxPatientDiagnosis.cs` baris 20–21 menuntut kolomnya terisi; `PatientDiagnosisController.cs` baris 326 mencari konsultasi yang cocok dan **melempar** bila tidak ketemu |
| Kenapa wajib | Diagnosis kerja lahir **pada** pemeriksaan pertama. Selama nomor konsultasi wajib, dokter harus membuat catatan harian lebih dulu semata-mata supaya ada tempat menggantungkan diagnosisnya — urutan yang terbalik dari cara kerja sebenarnya |
| Yang **tidak** berubah | Rawat jalan dan medical check-up tetap menuntut nomor konsultasi, dengan kalimat penolakan yang sama persis — `VAL-DOK-38`, `RWI-AC-143`. **IGD juga tidak ikut**, lihat 10.2. Nol baris lama berubah nilainya |
| Bila gagal | Permintaan ditolak `400` atau `422`; tidak ada diagnosis setengah jadi dan tidak ada konsultasi bayangan yang dibuatkan diam-diam |
| Bukti selesai | Diagnosis pada perawatan rawat inap tanpa nomor konsultasi **diterima** dan terbaca pada daftar masalah kajian medis pasien itu; diagnosis tanpa nomor konsultasi pada kunjungan rawat jalan **tetap ditolak dengan kalimat yang sama persis** |

### 10.1 Kenapa entri ini membalik satu baris yang sebelumnya menolak

[`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 9 mencantumkan
"melonggarkan `ConsultationId` pada resep, tindakan, dan diagnosis" sebagai hal yang **sengaja
tidak dibuat**, dengan alasan "ketiganya memang lahir dari konsultasi; yang perlu dibuka adalah
konsultasinya". Baris itu ditulis 2 September 2026.

| Untuk | Alasan itu masih berlaku? | Kenapa |
| --- | :---: | --- |
| Resep | **Ya** | Resep memang digantungkan pada satu catatan dokter, dan catatan kedua sudah dibuka `BE-RWI-043` |
| Tindakan | **Ya** | Sama; tindakan dicatat dari catatan yang menaunginya |
| **Diagnosis** | **Tidak** | Kajian medis awal adalah **dokumen dan layar tersendiri**, bukan catatan harian. Ia lahir sebelum catatan harian pertama ada |

Yang berubah bukan pendiriannya, melainkan **fakta yang tersedia**. Saat baris bagian 9 ditulis,
kajian medis belum punya layar dan belum punya kolom isian medis; keduanya baru lahir lewat
`BE-RWI-045` pada 5 September dan `FE-RWI-044` sesudahnya. Barulah kelihatan bahwa "buka
konsultasinya" **tidak menjawab** kasus ini: membuka konsultasi memang membuat dokter *bisa*
menulis, tetapi memaksanya membuat catatan harian yang tidak ia perlukan hanya demi menampung
diagnosis. `CAP-022` aturan 5 menuntut daftar masalah menjadi objek terstruktur, dan aturan 2
menuntutnya berada di dalam kajian medis.

> **Pemilik wajib membaca ini sebelum menyetujui `0.4.0`.** Amendment ini **membalik** satu baris
> pada artefak yang sudah `approved`. Ia tidak diselipkan: baris bagian 9 diperbarui, kamus data
> diperbarui, dan alasannya ditulis di sini. Menolak butir ini berarti `BE-RWI-068` tetap
> terblokir, dan itu keputusan yang sah — yang tidak sah adalah membiarkan kontrak dan arsitektur
> mengatakan dua hal yang berbeda.

### 10.2 Kenapa IGD sengaja tidak ikut

`RWI-DEC-070` dulu memperluas pelonggaran `RWI-RULE-026` ke kunjungan bertipe `Emergency`, sehingga
pertanyaan wajar berikutnya adalah kenapa pelonggaran ini berhenti di `Inpatient`.

Sebabnya kepemilikan, bukan teknis. `RWI-DEC-069` **mencabut** bagian IGD dari persetujuan lintas
modul `RWI-DEC-062` setelah diketahui pemilik `EmergencyInstallationManagement` adalah **Rizki
Gunawan**, bukan pemilik yang menandatangani `RWI-DEC-062`. Memperluas dari sini berarti memutuskan
atas nama pemilik lain — persis kekeliruan yang `RWI-DEC-069` betulkan.

**Temuan untuk pemilik IGD, dicatat dan tidak dikerjakan di sini:** pasien IGD kemungkinan besar
menghadapi keterbatasan yang sama, karena pengkajian IGD sudah dilonggarkan dari antrean lewat
`BE-IGD-026` tetapi diagnosis terstrukturnya tetap menuntut nomor konsultasi.

---

## 11. Integrasi yang tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Penulisan status penyerahan obat | `RUL-DOK-01` |
| Penyalinan hasil laboratorium maupun radiologi ke tabel Rawat Inap | `RUL-DOK-02`, `AC-CAP015-02` |
| Penghitungan visite dari catatan dokter | `INV-DOK-07` |
| Agregasi tarif visite | Milik Billing; kebijakannya belum ada — `ARCH-GAP-012`. `RWI-DEC-085` melarang agregasi menyentuh riwayat klinis |
| Pemberitahuan otomatis kepada pengguna | Tidak ada requirement-nya; yang diminta hanya daftar pantau dan daftar percobaan ulang — `RWI-DOK-RQG-001` |
| Antrean semu untuk pasien rawat inap | `RWI-RULE-026` aturan 2 |
| Pembuatan **konsultasi bayangan** demi mengisi `ConsultationId` diagnosis | Menanam baris catatan dokter yang tidak pernah ditulis siapa pun ke dalam rekam medis. `INT-DOK-10` memilih melonggarkan kolomnya, bukan memalsukan isinya |
| Pelonggaran nomor konsultasi pada **resep** dan **tindakan** | Tetap ditolak — `02-backend-architecture.md` bagian 9, dan alasannya masih berlaku bagi keduanya. Lihat 10.1 |

---

## 12. Perubahan pada `contract_version` `0.6.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Penomoran melanjutkan `INT-DOK-10`; `INT-DOK-01` s.d. `INT-DOK-10` tidak bergerak. Seluruh
integrasi di bawah bersifat **internal dalam satu basis data dan satu proses**; tidak ada sistem luar, antrean pesan,
maupun panggilan HTTP antar modul. "Transaksi yang sama" berarti satu `DbContext` transaction.

### 12.0 Ringkasan

| ID | Arah | Pemilik data | Pemakai | Sifat | Keadaan source `df3679c0` |
| --- | --- | --- | --- | --- | --- |
| `INT-DOK-11` | Baca | `InPatientManagement` | Ruang kerja dokter | Sinkron | `Extend` — census punya saringan DPJP dari query, bukan penugasan dari akun login |
| `INT-DOK-12` | Baca | `InPatientManagement` | Penjaga penulis klinis | Sinkron | `Missing` — penanda penugasan singkat dan jalur tulis konsulen/dokter jaga belum ada (`RWI-FACT-030`) |
| `INT-DOK-13` | Tulis, dipicu penutupan | `ClinicalManagement`, `MedicalRecordManagement` | `episode-rawat-inap` | Sinkron, satu transaksi | `Missing` — `RLN3-CAP-38` |
| `INT-DOK-14` | Baca | `MedicalRecordManagement` | "Catatan Saya" | Sinkron | `Reuse with adapter` untuk konsep (`RLN3-CAP-37`); `Missing` untuk catatan terkunci |
| `INT-DOK-15` | Tulis | `MedicalRecordManagement` | `ClinicalManagement` | Sinkron, satu transaksi | `Extend` — pendaftaran idempoten sudah ada (`RWI-FACT-040`), pemanggilan sejak konsep belum |
| `INT-DOK-16` | Tulis, dipicu penghentian butir | `PharmacyManagement` MAR | Resep Harian | Sinkron, satu transaksi | `Missing` — MAR belum ada |
| `INT-DOK-17` | Tulis | `PharmacyManagement`, `MasterData` | Rekonsiliasi | Sinkron | `Missing` — `RWI-FACT-032` |
| `INT-DOK-18` | Baca oleh pelaksanaan | `PharmacyManagement` order | `keperawatan` | Sinkron | `Missing` |
| `INT-DOK-19` | Tulis | `ClinicalManagement`, `LaboratoryManagement`, `RadiologyManagement` | Pesanan perawat | Sinkron | `Missing`; Lab/Rad menunggu persetujuan pemilik |
| `INT-DOK-20` | Baca-tulis | `InPatientManagement` resume | Tab Resume Medis | Sinkron | `Extend` — resume ada, tiga isian belum |
| `INT-DOK-21` | Frontend | Komponen milik `rawat-jalan` | Ruang kerja dokter | Pemakaian komponen | `Reuse with adapter` — `RLN3-CAP-03` |
| `INT-DOK-22` | Perubahan perilaku | `PharmacyManagement` template | Poliklinik, Farmasi | Pemberitahuan | `Repair` — `RWI-FACT-035` |

### 12.1 `INT-DOK-11` — Daftar pasien dokter dari penugasan aktif

| Hal | Isinya |
| --- | --- |
| Tujuan | Daftar Pasien Rawat Inap berisi tepat pasien yang boleh ditulis dokter login — `RWI-DEC-111` |
| Sumber | `GET census?assignedToMe=true` milik `episode-rawat-inap` kontrak `0.9.0` |
| Aturan | Dokter dari `ApplicationUser` → `MstDoctor`; penugasan aktif = `StartDateTime ≤ sekarang` dan `EndDateTime` kosong atau `> sekarang`, peran apa pun, **termasuk** penugasan singkat |
| Kegagalan | Akun tidak tertaut dokter → `403` "Akun Anda belum terhubung ke data dokter"; census gagal → panel kiri menampilkan galat dengan Coba Lagi, **bukan** daftar kosong |
| Contoh | dr. Ahmad DPJP Budi dan konsulen Sari; 120 pasien lain dirawat → daftar dua kartu, Total Pasien 2 |

### 12.2 `INT-DOK-12` — Penugasan singkat dan penugasan konsulen/dokter jaga

| Hal | Isinya |
| --- | --- |
| Tujuan | Menyediakan satu-satunya jalan menulis catatan terlambat yang belum pernah dimulai — `RWI-DEC-128` butir (4), `RWI-DEC-130` |
| Pemilik | `episode-rawat-inap` — `InpDoctorAssignment.AssignmentPurpose`, jalur tulis `POST /episodes/{id}/doctor-assignments/supporting` |
| Yang dibaca sub-modul ini | `AssignmentRole`, `AssignmentPurpose`, `StartDateTime`, `EndDateTime` pada saat penilaian `INV-DOK-15` |
| Batas yang wajib dijaga di sini | Penugasan `LateDocumentation` hanya memberi kewenangan **menulis**; `VAL-DOK-44` menolak verifikasi CPPT, dan penjaga `GUARD-INP-01` s.d. `04` milik episode menolak keputusan pulang, tanda tangan resume, perpindahan, dan isolasi |
| Ketergantungan | **Menahan** `RWI-AC-184` jalur "kiriman ulang diterima". Sampai jalur tulis episode ada, catatan terlambat yang belum pernah dimulai **tidak dapat ditulis sama sekali** — `RWI-DEC-130` konsekuensi (2) |

### 12.3 `INT-DOK-13` — Penutupan episode mengunci konsep dan membatalkan pesanan tertunda

| Hal | Isinya |
| --- | --- |
| Pemicu | `POST discharges/{episodeId}/close` dan `close-with-override` milik `episode-rawat-inap` |
| Langkah dalam satu transaksi | (1) status episode `Closed`; (2) `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync(encounterId)` — seluruh registrasi `Draft` kunjungan itu menjadi `LockedUnsigned`, `LockTrigger = EncounterClosed`; (3) `PatientProcedureOrderService.CancelPendingOrdersForClosureAsync(episodeId, closedByUserId)` — pesanan `Planned`/`Ordered` yang `IsExecuted = false` **dan** `IsBillingGenerated = false` menjadi `Cancelled` beralasan "episode ditutup sebelum dilaksanakan" |
| Yang tidak menahan penutupan | Adanya konsep, adanya pesanan tertunda, adanya pesanan tertunda yang **sudah tertagih** (dibiarkan, masuk daftar pantau `monitoring/billed-pending-procedure-orders` milik episode) — `RWI-DEC-129` butir (4), `RWI-DEC-143` butir (5) |
| Kegagalan teknis | Bila langkah (2) atau (3) melempar galat, **seluruh penutupan batal** dan petugas menerima "Penutupan gagal disimpan, coba lagi". Kiriman ulang aman karena kedua langkah idempoten |
| Idempotensi | Langkah (2) hanya menyentuh `Draft`; langkah (3) hanya menyentuh pesanan tertunda. Menjalankan ulang pada episode yang sudah `Closed` tidak mengubah apa pun |
| Pemberitahuan | Titik sentuh dengan `MedicalRecordManagement` — **wajib diberitahukan kepada Yoga Aji Pratama** (`RWI-DEC-138` konsekuensi 4). Mesin penguncian tidak diubah; yang bertambah hanya pemanggilnya |
| Contoh | Episode Joko ditutup 13.00: konsep SOAP dr. Yoga → `LockedUnsigned`; pesanan cek GDS Ns. Siti → `Cancelled`; penutupan selesai |

### 12.4 `INT-DOK-14` — "Catatan Saya"

| Hal | Isinya |
| --- | --- |
| Sumber tunggal | Mesin keutuhan `MedicalRecordManagement` — `RWI-DEC-142` |
| Konsep | `GET clinical-document-integrities/my-unsigned` **dipakai ulang**; permintaan tambahan query `serviceContext=Inpatient` dan empat kolom identitas minimum |
| Catatan terkunci | `GET clinical-document-integrities/my-authored` **baru** di `MedicalRecordManagement` |
| Status persetujuan | **Menunggu Yoga Aji Pratama** untuk kedua perubahan. Desain boleh disetujui; implementasi bagian terkunci tertahan |
| Batas baca | Identitas minimum + isi catatan milik sendiri + addendum melekat — `RWI-DEC-127` butir (2). Tidak membuka CPPT orang lain, resep, pesanan, maupun hasil penunjang |
| Kegagalan | Mesin keutuhan tidak terjangkau → "Catatan Saya gagal dimuat" beserta Coba Lagi, **bukan** daftar kosong — `RWI-DEC-142` jalur tidak normal (c) |
| Keadaan antara | Sebelum `INT-DOK-15` berjalan, konsep SOAP dan kajian medis rawat inap tidak tampil pada daftar karena belum terdaftar — `RWI-DEC-142` jalur tidak normal (a). Layar menulis catatan kaki "Konsep SOAP dan kajian medis belum tercakup" selama keadaan itu |

### 12.5 `INT-DOK-15` — Registrasi keutuhan sejak konsep

| Hal | Isinya |
| --- | --- |
| Pemanggil | `InpatientClinicalDocumentRegistrationService` di `ClinicalManagement` |
| Yang dipanggil | `ClinicalDocumentIntegrityService.RegisterAsync` (buat konsep), `SignAsync` (selesai), jalur batal registrasi (batal konsep) — **service yang sudah ada**, nol perubahan model `Mrc*` |
| Jenis dokumen | `ClinicalDocumentKind.Consultation` untuk SOAP, `Assessment` untuk kajian medis — nol nilai enum baru |
| Transaksi | Satu transaksi dengan penyimpanan dokumen. Gagal mendaftar → dokumen tidak tersimpan |
| Batas scope | Hanya kunjungan yang punya episode rawat inap — `RWI-DEC-144` butir (7) |
| Idempotensi | `RegisterAsync` mencari registrasi berdasarkan jenis dan id dokumen lalu mengembalikan yang ada (`RWI-FACT-040`). **Keunikan di tingkat basis data tidak ada**; dua transaksi paralel pada dokumen yang sama secara teoretis dapat membuat dua baris. Risiko diterima karena dokumen yang sama hanya dapat disunting penulisnya; unique index pada `(DocumentKind, DocumentId)` **diusulkan** kepada pemilik `MedicalRecordManagement`, tidak dikerjakan sub-modul ini |

### 12.6 `INT-DOK-16` — Penghentian butir resep ke MAR

| Hal | Isinya |
| --- | --- |
| Pemicu | `PATCH prescriptions/items/{itemId}/stop` |
| Akibat | `MedicationAdministrationService.CancelDueDosesForItemAsync(itemId, stoppedAt, "resep dihentikan")` milik `keperawatan`; dosis `Administered`, `Held`, `Refused`, `Missed` yang sudah ada **tidak disentuh** |
| Transaksi | Satu transaksi. Satu modul pemilik (`PharmacyManagement`), sehingga tidak ada pemanggilan lintas modul |
| Contoh | `RWI-DEC-121`: dosis `Due` 20.00 → `Cancelled`; 08.00 `Administered` tetap |

### 12.7 `INT-DOK-17` — Rekonsiliasi ke draft resep dan master obat

| Hal | Isinya |
| --- | --- |
| "Lanjut Sama" | Membuat butir pada draft resep episode: obat, dosis, frekuensi, dan rute disalin dari obat bawaan; penanda formularium disalin ke `IsFormularySnapshot` (`RWI-FACT-033`) |
| "Lanjut Ubah" | Sama, tetapi layar resep langsung membuka butir itu untuk diubah aturan pakainya |
| "Hentikan" | Tidak menyentuh resep |
| Draft resep | Memakai jalur draft resep yang sudah ada; bila episode belum punya draft, dibuat draft baru dengan `PrescriptionOrderType = Routine` |
| Pendaftaran non-formularium | `POST drugs/non-formulary-registrations` milik `MasterData`; `IsFormulary = false` dipaksa server. Persetujuan pemilik `MasterData` sudah ada lewat `RWI-DEC-062`; **cara pemaksaannya** dicatat di sini sebagai desain yang dimintakan persetujuan pada approval ini |
| Kegagalan | Draft resep gagal dibuat → keputusan ikut batal; tidak ada keputusan "Lanjut" tanpa butir draft |

### 12.8 `INT-DOK-18` — Order sliding scale dibaca pelaksanaan

| Hal | Isinya |
| --- | --- |
| Pembaca | Pelaksanaan sliding scale milik `keperawatan` kontrak `0.5.0` |
| Yang dibaca | Order `Active`, versi order terbaru, rentangnya, dan `GlucoseUnit` versi template asal |
| Aturan | Pelaksanaan menyimpan `OrderVersionId` yang dipakai; perubahan versi setelahnya tidak mengubah pelaksanaan lama — `RWI-DEC-146` butir (6) |
| Sumber GDS | **Bukan** integrasi sub-modul ini. GDS dibaca pelaksanaan dari `ClinicalManagement` — `RWI-DEC-148` |

### 12.9 `INT-DOK-19` — Pesanan perawat dengan dokter pemberi instruksi

| Hal | Isinya |
| --- | --- |
| Modul tujuan | `ClinicalManagement` (tindakan), `LaboratoryManagement`, `RadiologyManagement` |
| Data yang dikirim | Pesanan biasa + `InstructingDoctorId`; penginput dari akun login |
| Pemeriksaan pemberi instruksi | `InpatientClinicalContextService.GetActiveAssignmentsForDoctorAsync` dipanggil dari ketiga modul; Lab dan Rad **membaca** penugasan episode lewat service bersama, bukan menyalinnya |
| Verifikasi | Setiap modul menyimpan status verifikasinya sendiri; daftar tunggu verifikasi di ruang kerja dokter menggabungkan tiga endpoint |
| Gerbang | Persetujuan pemilik `LaboratoryManagement` dan `RadiologyManagement` belum tercatat — **menahan implementasi bagian Lab/Rad**, bukan desain. Pesanan tindakan tidak ikut tertahan |
| Batas waktu verifikasi | Keputusan klinis, menunggu pemilik klinis seperti `RWI-RULE-021`; tidak ada angka pada rilis ini |

### 12.10 `INT-DOK-20` — Tab Resume Medis

| Hal | Isinya |
| --- | --- |
| Pemilik | `episode-rawat-inap` — `InpDischargeSummary` beserta tiga isian baru |
| Yang dilakukan tab | Membaca, menyimpan draf, menandatangani lewat endpoint episode yang sama dengan `FE-INP-06` |
| Isian otomatis | `GET summary-prefill` mengusulkan diagnosis terstruktur, tindakan terlaksana, hasil penunjang final, dan resep obat pulang beserta label sumber; dokter tetap meninjau — `RWI-DEC-112` |
| Tidak menutup episode | Tanda tangan tidak memicu penutupan — PRD v`2.0` bagian 64 |
| Resume ODC | Tidak ada integrasi — `RWI-DEC-123` |

### 12.11 `INT-DOK-21` — Komponen tata letak Dokter Rawat Jalan

| Hal | Isinya |
| --- | --- |
| Kebutuhan | `UI-AC-DOK-001` s.d. `012`: struktur, ukuran, tab, dan keadaan kosong **sama persis** |
| Keadaan | `RLN3-CAP-03`: kelas tata letak CSS dan `EmptyState` dapat dipakai langsung; `SummaryBar`, `QueuePatientCard`, `ConsultationTabs`, `DoctorPatientContext` terikat data antrean |
| Bentuk yang dipilih | **Ekstraksi komponen presentasional berbasis props** ke pustaka bersama, dipakai Rawat Jalan dan Rawat Inap. Rawat Inap memberi adapter data episode |
| Yang ditolak | Menyalin keempat komponen ke folder Rawat Inap — dua salinan pasti menyimpang; memakai `doctor-clinical-base` yang tidak dipakai Rawat Jalan — melanggar `UI-AC-DOK-009` |
| Ketergantungan | **Persetujuan pemilik blueprint `rawat-jalan`** atas ekstraksi. Tanpanya task layout tertahan; task isi tab tidak ikut tertahan |

### 12.12 `INT-DOK-22` — Perubahan perilaku template resep bagi poliklinik

| Hal | Isinya |
| --- | --- |
| Yang berubah bagi semua pemakai | Pemilik template dari akun login; ubah dan hapus hanya oleh pemilik — `RWI-DEC-135` butir (2) |
| Yang tetap | Fitur Bersama pada poliklinik dan ruang kerja resep Farmasi; kolom `IsShared` |
| Kewajiban | Diberitahukan kepada pemilik blueprint `rawat-jalan` **sebelum rilis**; dicatat pada task pelaksananya sebagai syarat selesai |
| Risiko | Alur asisten yang membuatkan template atas nama dokter, bila ada, berhenti |

### 12.13 Integrasi yang sengaja tidak dibuat pada `0.6.0`

| Yang tidak dibuat | Alasan |
| --- | --- |
| Integrasi order dan hasil Gizi, Hemodialisa, Bank Darah, Rehab Medik | `RWI-DEC-108`, `113`; jawaban pemilik 15 September 2026. **Temuan dicatat:** `NutritionManagement` (`NutritionOrderController`) dan `BloodBankManagement` (`BbkBloodOrderController`) sudah ada pada `BE@df3679c0`; syarat "sampai modulnya ada" pada `RWI-DEC-108` mungkin sudah terpenuhi untuk keduanya dan diajukan sebagai pertanyaan non-blocking ke `grill-me` berikutnya |
| Notifikasi aktif ke dokter dari rentang sliding scale atau dari pesanan perawat | Gate `G-24`, `G-15`; tidak ada requirement notifikasi yang diputuskan |
| Aggregator lintas modul untuk daftar verifikasi instruksi | Tiga pemilik; penggabungan terjadi di layar |
| Penguncian konsep saat **kunjungan** rawat inap diselesaikan lewat `PatientEncounterController` | Pemicu yang sah bagi rawat inap adalah penutupan **episode**. Mengubah status kunjungan rawat inap menjadi `Completed` di luar penutupan episode tetap dilarang alur episode |
