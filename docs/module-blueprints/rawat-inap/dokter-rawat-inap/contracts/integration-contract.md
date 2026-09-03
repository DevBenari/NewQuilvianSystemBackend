# Integration Contract — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2` bagian X |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.3.0`: `INT-DOK-07` berubah dari "nol perubahan, satu jaminan yang perlu dipastikan" menjadi **pendaftaran tiga jenis dokumen** — `RWI-DEC-087`. Penomoran `INT-DOK-*` tetap seperti `0.2.0`; tabel pemetaan dari `0.1.0` ada di bagian 0.2 |
| Tanggal | 2 September 2026 |

---

## 0. Kenapa dokumen ini menentukan

Sub-modul ini tidak memiliki satu tabel pun dan menyentuh **enam** modul lain:
`ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement`,
`MedicalRecordManagement`, dan `BillingManagement`. Hampir seluruh wujudnya adalah integrasi.

### 0.1 Penomoran kanonis

`INT-DOK-01` s.d. `INT-DOK-07` **diambil apa adanya** dari arsitektur domain bagian X.1 dan tidak
diturunkan ulang di sini. `INT-DOK-08` dan `INT-DOK-09` adalah tambahan tingkat desain yang tidak
punya padanan domain.

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

## 10. Integrasi yang tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Penulisan status penyerahan obat | `RUL-DOK-01` |
| Penyalinan hasil laboratorium maupun radiologi ke tabel Rawat Inap | `RUL-DOK-02`, `AC-CAP015-02` |
| Penghitungan visite dari catatan dokter | `INV-DOK-07` |
| Agregasi tarif visite | Milik Billing; kebijakannya belum ada — `ARCH-GAP-012`. `RWI-DEC-085` melarang agregasi menyentuh riwayat klinis |
| Pemberitahuan otomatis kepada pengguna | Tidak ada requirement-nya; yang diminta hanya daftar pantau dan daftar percobaan ulang — `RWI-DOK-RQG-001` |
| Antrean semu untuk pasien rawat inap | `RWI-RULE-026` aturan 2 |
