# Rawat Inap — Hospital Domain Architecture

## A. Identitas arsitektur

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Architecture revision | `0.2` — pass pertama `0.1` ditambah amendment Dokter Rawat Inap pada [Bagian Kedua](#bagian-kedua--amendment-dokter-rawat-inap) |
| Architecture status | `draft` — belum disetujui manusia |
| Tanggal | Pass pertama 21 Agustus 2026; amendment Dokter Rawat Inap 2 September 2026 (`Asia/Jakarta`) |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_PARTIAL`** untuk dokumen keseluruhan. Scope Dokter Rawat Inap — `CAP-015` dan `CAP-020` s.d. `CAP-025` — berstatus **`DOMAIN_ARCHITECTURE_READY`**, lihat bagian AB |
| Kesiapan requirement masukan | `PARTIALLY_READY`, dari [`02-requirement-completeness-gate.md`](./02-requirement-completeness-gate.md) revision `1.0`, SHA-256 `cc32db172b2441b2967ce3507c89b81f12fc103bbd3b3a92bc7bc49d77005ffe` |
| Bukti bisnis | [`00-interview-decisions.md`](../00-interview-decisions.md) revision `2`, SHA-256 `1be53ca22ce811ed584135a49f6f51fc2499802b7604878aca7fe1024d3ae435` |
| Bukti keadaan saat ini | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.2` |
| Backend snapshot | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| Frontend snapshot | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| Baseline rujukan | `indonesia-hospital-domain-reference`, `references/inpatient.md`, seluruhnya `REFERENCE_ONLY` |
| Prefix modul | `Inp`, sesuai registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 20, lifecycle `PLANNED` |
| Batas tulis | Hanya dokumen ini. Tidak ada schema, migration, endpoint, UI, task, atau source aplikasi yang dibuat |

> **Baris bukti di atas adalah bukti pass pertama** — requirement gate revision `1.0`, decision log
> revision `2`, capability map revision `1.2`, beserta SHA source Agustus 2026. Bukti yang dipakai
> amendment Dokter Rawat Inap **berbeda dan lebih baru**, dan didaftar pada bagian O.

> **Apa yang dikerjakan dokumen ini.** Dokumen ini menerjemahkan aturan bisnis yang sudah
> diputuskan menjadi **peta makna**: konsep apa saja yang hidup di dalam Rawat Inap, siapa
> pemiliknya, apa yang harus selalu benar, dan bagaimana sesuatu berubah dari satu keadaan ke
> keadaan berikutnya.
>
> Dokumen ini **bukan** rancangan tabel database, bukan daftar endpoint, dan bukan rancangan
> layar. Ketiganya dikerjakan `/qv-design` setelah dokumen ini disetujui.

### A.1 Scope yang dirancang

Hanya slice yang dinyatakan siap oleh gerbang kelengkapan requirement. Slice lain **tidak**
dirancang, walaupun tetangganya sudah siap.

| Slice | Nama | Kesiapan requirement |
| --- | --- | --- |
| `INP-S01` | Admisi dan pemesanan tempat tidur, jalur datang langsung dan poliklinik | `READY_FOR_DOMAIN_DESIGN` |
| `INP-S02` | Penempatan tempat tidur, census, dan lama dirawat | `READY_FOR_DOMAIN_DESIGN` |
| `INP-S03` | Perpindahan pasien dan pindah kelas | `READY_FOR_DOMAIN_DESIGN` |
| `INP-S04` | Penugasan perawat penanggung jawab | `READY_FOR_DOMAIN_DESIGN` |
| `INP-S07` sebagian | Keputusan pulang dan resume pulang, untuk tiga cara pulang | Slice siap yang berdiri sendiri |
| `INP-S08` sebagian | Daftar periksa administrasi, kelayakan keuangan, penutupan episode | Slice siap yang berdiri sendiri |
| `INP-S12` | Bayi baru lahir dan boks bayi | `READY_FOR_DOMAIN_DESIGN` |
| `INP-S13` | Riwayat status, audit, dan dua dari tiga daftar pantau | `READY_FOR_DOMAIN_DESIGN` |
| `INP-S14` | Pengaturan yang dapat diubah admin | `READY_FOR_DOMAIN_DESIGN` |

### A.2 Scope yang sengaja tidak dirancang

| Slice | Alasan berhenti | Decision ID |
| --- | --- | --- |
| ~~`INP-S05` dokumentasi klinis dan visite~~ | ~~Dua alternatifnya menghasilkan model domain yang berbeda total~~ — **bagian dokter sudah dirancang** pada Bagian Kedua setelah `DEC-INP-001` dan `DEC-INP-008` ditutup. Bagian keperawatan tetap belum dirancang | `DEC-INP-001` `CLOSED` |
| ~~`INP-S06` resep dan obat pulang~~ | ~~Bergantung pada keputusan yang sama~~ — **sudah dirancang** pada Bagian Kedua bagian S.5 | `DEC-INP-001` `CLOSED` |
| `INP-S09` serah terima IGD | Menentukan kunjungan mana yang menjadi jangkar episode | `DEC-INP-002` |
| `INP-S10` persetujuan umum | Keputusan hukum dan privasi | `DEC-INP-003` |
| `INP-S11` jenis kelamin dan isolasi | Satu-satunya butir `CONFLICT`, menyentuh pengendalian infeksi | `DEC-INP-004` |
| `INP-S15` interoperabilitas SATUSEHAT | Menentukan pemilik data riwayat lokasi | `DEC-INP-005` |
| Serah terima klinis antar shift | Belum pernah dibahas, ditandai `SAFETY_CHECK` oleh baseline | `DEC-INP-006` |
| Cara pulang meninggal dan kabur | Sisi klinisnya masih terbuka | `DEC-INP-007` |

### A.3 Dua syarat yang diwariskan gerbang requirement

Gerbang kelengkapan requirement menitipkan dua syarat supaya slice yang dirancang sekarang tidak
perlu dibongkar ketika keputusan yang terbuka akhirnya turun. Keduanya **sudah dipatuhi** di
dalam arsitektur ini:

| Syarat | Bagaimana dipatuhi |
| --- | --- |
| Catatan penempatan dirancang sebagai riwayat yang dapat dibaca ulang | `CON-INP-007` Penempatan Tempat Tidur adalah baris berperiode dengan waktu mulai dan waktu berakhir, bukan penanda keadaan terakhir. Lihat bagian F.2 |
| Pemeriksaan kelayakan penempatan dirancang sebagai titik yang dapat diisi aturan tambahan | Perintah `CMD-INP-03` Tempatkan Pasien memanggil satu pemeriksaan bernama **Kelayakan Penempatan** yang isinya berupa daftar aturan, bukan syarat yang ditanam mati. Lihat bagian E.1 |

---

## B. Ubiquitous language

Istilah berikut dipakai dengan makna yang sama di seluruh arsitektur. Bila satu kata dipakai dua
departemen dengan arti berbeda, perbedaannya dipertahankan, tidak disatukan.

| Istilah | Makna bisnis yang dipakai di sini | Catatan |
| --- | --- | --- |
| **Episode rawat inap** | Satu rangkaian perawatan menginap satu pasien, dari admisi dibuka sampai episode ditutup | Bukan sinonim kunjungan. Satu kunjungan menampung tepat satu episode, tetapi kunjungan juga dipakai rawat jalan dan IGD |
| **Kunjungan** | Catatan kedatangan pasien milik modul Registrasi, dipakai sebagai jangkar episode | Istilah teknisnya `TrxPatientEncounter` |
| **Admisi** | Proses menerima pasien untuk dirawat inap: memilih penjamin, DPJP, kelas, dan tempat tidur | Admisi adalah **proses**, bukan status. Statusnya `Draft` lalu `Admitted` |
| **Pemesanan tempat tidur** | Penguncian sementara satu tempat tidur untuk satu calon pasien, berlaku 2 jam | Pasiennya belum tentu ada di kamar |
| **Penempatan tempat tidur** | Fakta bahwa satu pasien menempati satu tempat tidur, sejak waktu tertentu sampai waktu tertentu | Ini yang menjadi sumber kebenaran penghunian, bukan kolom status pada master tempat tidur |
| **Penghunian** | Keadaan sebuah tempat tidur sedang dipakai pasien | Diturunkan dari penempatan yang masih aktif |
| **Perpindahan** | Berpindahnya pasien ke tempat tidur lain **di dalam episode yang sama** | Tidak membuat episode baru dan tidak membuat kunjungan baru |
| **Pindah kelas** | Perpindahan yang membuat kelas perawatan berubah | Bukan jenis tindakan tersendiri; ia akibat dari perpindahan |
| **DPJP** | Dokter Penanggung Jawab Pelayanan untuk episode itu, pada rentang waktu tertentu | Satu episode boleh berganti DPJP; yang berlaku adalah yang aktif pada saat itu |
| **Perawat penanggung jawab** | Perawat yang bertanggung jawab atas pasien pada rentang waktu tertentu | Boleh kosong sementara tanpa menahan tindakan apa pun |
| **Census** | Daftar pasien yang sedang dirawat inap beserta lokasi dan penanggung jawabnya | Turunan, bukan data yang disimpan tersendiri |
| **Lama dirawat** | Selisih tanggal masuk dan tanggal keluar, paling sedikit 1 hari | Dihitung, bukan disimpan. Bertambah setiap pergantian tanggal |
| **Rencana pulang** | Keadaan ketika pasien sudah diizinkan pulang tetapi episodenya belum ditutup | Status `DischargePending`. Tempat tidur masih dipegang |
| **Cara pulang** | Alasan berakhirnya perawatan: atas izin DPJP, atas permintaan sendiri, atau dirujuk | Dua cara lain, meninggal dan kabur, di luar scope arsitektur ini |
| **Resume pulang** | Ringkasan resmi perawatan milik episode, ditandatangani DPJP | Berbeda dari surat keterangan untuk pasien, yang tetap milik modul Klinis |
| **Daftar periksa administrasi** | Daftar butir yang harus ditandai sebelum episode boleh ditutup | Butirnya diatur admin, sebagian dapat dinonaktifkan |
| **Kelayakan keuangan** | Pernyataan bahwa urusan pembayaran episode ini sudah beres | Selama Billing belum operasional, ditandai manual kasir |
| **Penutupan episode** | Tindakan mengakhiri episode yang sekaligus melepas tempat tidur | Berbeda dari keputusan pulang, yang hanya mengubah status menjadi rencana pulang |
| **Sesi koreksi** | Jendela waktu ketika episode yang sudah ditutup boleh dibetulkan catatannya | Konsep arsitektur baru, lihat bagian G.4. Episode **tetap** berstatus `Closed` selama sesi berjalan |
| **Boks bayi** | Tempat tidur khusus bayi baru lahir yang berada di kamar ibunya | Terdaftar sebagai tempat tidur tersendiri, bukan bagian dari tempat tidur ibu |

**Satu kata yang sengaja tidak dipakai:** "kamar". Dalam percakapan sehari-hari "pasien pindah
kamar" bisa berarti pindah tempat tidur di kamar yang sama, atau benar-benar pindah kamar. Di
dalam arsitektur ini yang dipakai selalu **tempat tidur**, karena tempat tidurlah satuan yang
ditempati satu pasien.

---

## C. Peta bounded context

### C.1 Context yang dimiliki modul Rawat Inap

| ID | Nama | Tanggung jawab bisnis | Konsep yang dimiliki |
| --- | --- | --- | --- |
| `CTX-INP-CARE` | Episode Perawatan Rawat Inap | Seluruh lifecycle satu episode menginap: admisi, penghunian tempat tidur, penanggung jawab, perpindahan, pemulangan, penutupan, dan jejaknya | `CON-INP-001` s.d. `CON-INP-012` |
| `CTX-INP-CONFIG` | Konfigurasi Rawat Inap | Angka dan daftar yang boleh diubah admin tanpa mengubah program | `CON-INP-013`, `CON-INP-014` |

**Kenapa hanya dua, bukan satu context per tahap.** Admisi, perawatan berjalan, dan pemulangan
sering terlihat seperti tiga hal berbeda karena dikerjakan orang yang berbeda di layar yang
berbeda. Tetapi ketiganya mengubah **objek yang sama** dan **kolom status yang sama**. Memecahnya
menjadi tiga context akan membuat satu lifecycle dijaga tiga pemilik, dan itu justru sumber
ketidakkonsistenan. Karena itu satu lifecycle tetap satu context.

**Kenapa konfigurasi dipisah.** Pengaturan dan daftar butir administrasi tidak punya lifecycle
episode. Ia diubah admin kapan saja, berlaku pada pembacaan berikutnya, dan tidak pernah "selesai"
atau "batal". Umur dan pemiliknya berbeda, jadi batasnya juga berbeda.

### C.2 Context milik modul lain yang dipakai

| ID | Context | Milik modul | Hubungan | Yang dipakai Rawat Inap |
| --- | --- | --- | --- | --- |
| `CTX-REG` | Registrasi dan Kunjungan | `RegistrationManagement` | Rawat Inap **mengikuti** bentuk yang sudah ada | Kunjungan sebagai jangkar episode, penjamin, kelas pasien pada kunjungan |
| `CTX-PAT` | Identitas Pasien | `PatientManagement` | Rawat Inap **mengikuti** | Identitas pasien, jenis kelamin, nomor rekam medis |
| `CTX-MST` | Master Fasilitas dan Layanan | `MasterData` HealthServices | **Bermitra**, lihat catatan di bawah | Tempat tidur, kamar, unit layanan, kelas pasien |
| `CTX-WFP` | Tenaga Kerja dan Praktisi | `Corporate/HumanResource` | Rawat Inap **mengikuti** | Dokter untuk DPJP, pegawai untuk perawat |
| `CTX-CLI` | Dokumentasi Klinis | `ClinicalManagement` | **Pelanggan–pemasok** sejak amendment; lihat bagian Q.1 | Catatan dokter/SOAP, CPPT, kajian medis, tindakan, dan event visite |
| `CTX-PHM` | Farmasi | `PharmacyManagement` | **Pelanggan–pemasok** sejak amendment; lihat bagian Q.1 | Resep harian, obat pulang, dan status pemenuhannya |
| `CTX-EMG` | Instalasi Gawat Darurat | `EmergencyInstallationManagement` | **Belum ditentukan** | Di luar scope, lihat `DEC-INP-002` |
| `CTX-BIL` | Billing | `BillingManagement` | Tidak dipakai pada MVP untuk episode; **hilir satu arah** untuk fakta klinis tindakan, lihat bagian Q.1 | Digantikan penandaan manual, lihat `RWI-RULE-028` |
| `CTX-LAB` | Laboratorium | `LaboratoryManagement` | **Pelanggan–pemasok**, ditambahkan amendment | Pesanan dan hasil final terverifikasi |
| `CTX-RAD` | Radiologi | `RadiologyManagement` | **Pelanggan–pemasok**, ditambahkan amendment | Pesanan, studi, dan hasil final terverifikasi |
| `CTX-MRC` | Integritas Dokumen Rekam Medis | `MedicalRecordManagement` | **Pelanggan–pemasok**, ditambahkan amendment | Tanda tangan, penguncian, addendum, dan pendelegasian penulis |

**Catatan tentang hubungan dengan `CTX-MST`.** Ini satu-satunya hubungan yang tidak murni
"mengikuti", dan perlu dinyatakan terang-terangan karena mengandung ketegangan.

`MstBed` dan `MstRoom` dimiliki `CTX-MST`, dan Rawat Inap **tidak** mengambil alih kepemilikan
itu. Baseline `ID-INP-CAP-003` dan pasal 6 juga tegas: domain rawat inap bukan pemilik otoritatif
master tempat tidur, kapasitas kamar, maupun kebijakan pemeliharaan.

Namun `RWI-DEC-039` menetapkan kolom `MstBed.BedStatus` turun kedudukan menjadi **salinan** dari
catatan penempatan milik Rawat Inap, dan salinan itu ditulis Rawat Inap dalam transaksi yang sama.
Artinya Rawat Inap menulis ke dalam model milik context lain.

Ini disebut hubungan **bermitra**, bukan mengikuti, dan konsekuensinya tiga:

1. `CTX-MST` tetap pemilik master tempat tidur: kode, nama, kamar, peruntukan, aktif atau tidak,
   dan keadaan non-pasien seperti `Cleaning`, `Maintenance`, `Blocked`.
2. `CTX-INP-CARE` menjadi pemilik **makna penghunian**: siapa menempati, sejak kapan, sampai kapan.
3. Nilai `Reserved` dan `Occupied` pada kolom status tempat tidur adalah **hasil turunan**, bukan
   masukan. Tidak boleh ada manusia yang menyetelnya langsung.

Bila kelak dibentuk context tersendiri untuk pengelolaan tempat tidur dan kapasitas — sebagaimana
disinggung baseline `ID-INP-CAP-003` — maka `CON-INP-006` dan `CON-INP-007` pada bagian D adalah
dua konsep yang paling wajar berpindah ke sana. Arsitektur ini sengaja menempatkan keduanya
sebagai satu kelompok yang dapat dipindahkan utuh.

---

## D. Katalog konsep domain

Klasifikasi yang dipakai: `AGGREGATE_ROOT`, `ENTITY`, `VALUE_OBJECT`, `REFERENCE_DATA`,
`DOMAIN_EVENT`, `EXTERNAL_CONTRACT`. Klasifikasi ini menggambarkan tanggung jawab domain, bukan
bentuk tabel database.

Kepemilikan memakai `Existing`, `Extend`, `New`, atau `Adapter/View`.

### D.1 Konsep yang dimiliki Rawat Inap

| ID | Nama bisnis | Klasifikasi | Ownership | Identitas | Peran dalam lifecycle | Invariant penting | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `CON-INP-001` | Episode Rawat Inap | `AGGREGATE_ROOT` | `New` | Nomor episode yang dapat dibaca manusia, unik | Akar seluruh lifecycle: `Draft` → `Admitted` → `DischargePending` → `Closed`, plus `Cancelled` | `INV-INP-01`, `INV-INP-03`, `INV-INP-04`, `INV-INP-05`, `INV-INP-06` | `RWI-RULE-003`, `RWI-RULE-005` |
| `CON-INP-002` | Penugasan DPJP | `ENTITY` | `New` | Episode + urutan | Berperiode. Ditutup saat dialihkan atau saat episode ditutup | `INV-INP-03` | `RWI-RULE-030` |
| `CON-INP-003` | Penugasan Perawat Penanggung Jawab | `ENTITY` | `New` | Episode + urutan | Berperiode. Boleh kosong sementara | Paling banyak satu aktif pada satu waktu | `RWI-RULE-033` |
| `CON-INP-004` | Resume Pulang | `ENTITY` | `New` | Satu per episode | Dibuat saat rencana pulang, ditandatangani DPJP, terkunci setelah penutupan | `INV-INP-05` | `RWI-RULE-032` |
| `CON-INP-005` | Penandaan Daftar Periksa Administrasi | `ENTITY` | `New` | Episode + butir | Ditandai sebelum penutupan | Butir wajib yang belum ditandai menahan penutupan | `RWI-RULE-018` |
| `CON-INP-006` | Pemesanan Tempat Tidur | `ENTITY` | `New` | Episode + tempat tidur + waktu mulai | Berumur pendek. Gugur sendiri saat dibaca setelah lewat batas | `INV-INP-02` | `RWI-RULE-001`, `RWI-RULE-002` |
| `CON-INP-007` | Penempatan Tempat Tidur | `ENTITY` | `New` | Episode + tempat tidur + waktu mulai | **Berperiode.** Satu baris per tempat tidur yang pernah ditempati, dengan waktu mulai dan waktu berakhir | `INV-INP-01`, `INV-INP-02`, `INV-INP-07` | `RWI-RULE-027`, `RWI-RULE-008` |
| `CON-INP-008` | Penandaan Kelayakan Keuangan | `ENTITY` | `New` | Episode + urutan | Berubah dari `Pending` ke `Cleared` atau `Blocked`, boleh berulang | Hanya peran kasir atau billing yang boleh mengubah | `RWI-RULE-028` |
| `CON-INP-009` | Riwayat Status Episode | `ENTITY` | `New` | Episode + nomor urut | Ditulis setiap perpindahan status. Tidak dapat diubah dan tidak dapat dihapus | `INV-INP-08` | `RWI-RULE-031` |
| `CON-INP-010` | Sesi Koreksi Episode | `ENTITY` | `New` | Episode + urutan | Dibuka supervisor pada episode `Closed`, ditutup setelah perbaikan selesai | Episode tetap `Closed` selama sesi berjalan | `RWI-RULE-020`, lihat bagian G.4 |
| `CON-INP-011` | Cara Pulang | `VALUE_OBJECT` | `New` | — | Melekat pada episode saat keputusan pulang diambil | Hanya tiga nilai yang berlaku pada scope ini | `RWI-RULE-011` |
| `CON-INP-012` | Lama Dirawat | `VALUE_OBJECT` | `New` | — | **Dihitung, tidak disimpan.** Selisih tanggal, paling sedikit 1 hari | Tidak berubah karena sesi koreksi | `RWI-RULE-019` |
| `CON-INP-013` | Pengaturan Rawat Inap | `REFERENCE_DATA` | `New` | Satu baris berlaku | Diubah admin, berlaku pada pembacaan berikutnya | Perubahan menyimpan pengubah dan waktunya | `RWI-RULE-034` |
| `CON-INP-014` | Butir Daftar Periksa Administrasi | `REFERENCE_DATA` | `New` | Kode butir | Diubah admin. Butir dapat dinonaktifkan | Butir nonaktif tidak menahan penutupan | `RWI-RULE-018` |

### D.2 Konsep milik context lain yang dipakai ulang

Tidak satu pun dari konsep berikut disalin, ditiru, atau dibuatkan versi Rawat Inap-nya. Ini
pertahanan langsung terhadap duplikasi.

| ID | Nama bisnis | Klasifikasi | Ownership | Pemilik | Dipakai untuk apa |
| --- | --- | --- | --- | --- | --- |
| `CON-EXT-001` | Pasien | `EXTERNAL_CONTRACT` | `Existing` | `CTX-PAT` | Identitas pasien yang dirawat |
| `CON-EXT-002` | Kunjungan | `EXTERNAL_CONTRACT` | `Existing` | `CTX-REG` | Jangkar episode. Satu episode menempel pada tepat satu kunjungan |
| `CON-EXT-003` | Penjamin Kunjungan | `EXTERNAL_CONTRACT` | `Existing` | `CTX-REG` | Cara bayar saat masuk |
| `CON-EXT-004` | Tempat Tidur | `EXTERNAL_CONTRACT` | `Existing` | `CTX-MST` | Sasaran pemesanan dan penempatan |
| `CON-EXT-005` | Kamar | `EXTERNAL_CONTRACT` | `Existing` | `CTX-MST` | Lokasi tempat tidur, penentu kelas |
| `CON-EXT-006` | Unit Layanan | `EXTERNAL_CONTRACT` | `Existing` | `CTX-MST` | Bangsal tempat pasien dirawat |
| `CON-EXT-007` | Kelas Pasien | `EXTERNAL_CONTRACT` | `Existing` | `CTX-MST` | Kelas perawatan dan kelas yang ditagihkan |
| `CON-EXT-008` | Dokter | `EXTERNAL_CONTRACT` | `Existing` | `CTX-WFP` | Ditunjuk sebagai DPJP |
| `CON-EXT-009` | Pegawai dan Profil Tenaga Kerja | `EXTERNAL_CONTRACT` | `Existing` | `CTX-WFP` | Ditunjuk sebagai perawat penanggung jawab |
| `CON-EXT-010` | Salinan Status Ketersediaan Tempat Tidur | `EXTERNAL_CONTRACT` | `Adapter/View` | `CTX-MST` memiliki kolomnya; `CTX-INP-CARE` menulis nilainya | Menjaga agar seluruh pembaca lama tetap bekerja tanpa diubah |

### D.3 Konsep yang dipertimbangkan lalu ditolak

Dicatat supaya tidak diusulkan ulang di kemudian hari.

| Konsep yang ditolak | Kenapa ditolak |
| --- | --- |
| Entity "Admisi" terpisah dari Episode | Admisi adalah **tahap** dalam lifecycle episode, bukan objek dengan identitas sendiri. Memisahkannya akan memecah satu lifecycle menjadi dua pemilik |
| Entity "Census" | Census adalah pertanyaan yang dijawab dari penempatan yang masih aktif, bukan data yang disimpan. Menyimpannya berarti membuat sumber kebenaran kedua yang bisa berbeda dari yang pertama |
| Entity "Lama Dirawat" | Hasil hitungan dari dua tanggal. Menyimpannya membuat angka basi setiap pergantian tanggal |
| Entity per status episode | Status adalah nilai, bukan objek. `RWI-RULE-003` sudah cukup dijaga satu kolom status ditambah riwayatnya |
| Entity "Bangsal Rawat Inap" milik Rawat Inap | Sudah ada `CON-EXT-006` Unit Layanan milik `CTX-MST`. Membuat versi sendiri adalah duplikasi master |
| Entity "Pasien Rawat Inap" | Duplikasi identitas pasien. Baseline pasal 11 dan aturan modul melarangnya secara tegas |
| Status episode keenam untuk "sedang dikoreksi" | Bertabrakan dengan `RWI-DEC-009` dan `RWI-AC-004` yang mengunci lima status. Digantikan `CON-INP-010` Sesi Koreksi, lihat bagian G.4 |
| Aggregate tersendiri untuk penghunian tempat tidur | Perpindahan dan penutupan wajib utuh bersama perubahan status episode. Dua aggregate yang selalu berubah bersama dalam satu transaksi sebenarnya satu aggregate |

---

## E. Model aggregate

### E.1 `AGG-INP-EPISODE` — Episode Rawat Inap

| Field | Nilai |
| --- | --- |
| Akar | `CON-INP-001` Episode Rawat Inap |
| Isi batas | `CON-INP-002` s.d. `CON-INP-012` |
| Di luar batas | Seluruh konsep `CON-EXT-*`, dirujuk lewat identitasnya saja |
| Alasan batas ini | Seluruh aturan penutupan, pembatalan, dan perpindahan menuntut episode dan penempatan berubah **bersamaan atau tidak sama sekali** |

#### Invariant yang dijaga aggregate ini

| ID | Invariant | Dasar |
| --- | --- | --- |
| `INV-INP-01` | Episode berstatus `Admitted` atau `DischargePending` wajib punya **tepat satu** penempatan aktif | `RWI-DEC-014`, INV-02 pada PRD |
| `INV-INP-03` | Episode berstatus `Draft`, `Admitted`, atau `DischargePending` wajib punya **tepat satu** DPJP aktif | `RWI-RULE-030` aturan 3 |
| `INV-INP-04` | Satu episode menempel pada **tepat satu** kunjungan, dan satu kunjungan menampung **paling banyak satu** episode | `RWI-RULE-005` |
| `INV-INP-05` | Satu episode punya **paling banyak satu** resume pulang | `RWI-RULE-032` aturan 1 |
| `INV-INP-06` | Episode `Closed` tidak dapat diubah kecuali ada sesi koreksi yang sedang terbuka | `RWI-RULE-020` |
| `INV-INP-07` | Selama perpindahan berlangsung, pasien tidak pernah tercatat tanpa tempat tidur | `RWI-RULE-008` |
| `INV-INP-08` | Setiap perpindahan status meninggalkan **tepat satu** baris riwayat | `RWI-RULE-031` aturan 3 |
| `INV-INP-09` | Episode `Draft` boleh **tanpa** pemesanan maupun penempatan aktif | Turunan `RWI-RULE-015` dan `RWI-RULE-022`, lihat catatan di bawah |

**Catatan tentang `INV-INP-09`, dan kenapa ini penting.** Tabel pada `RWI-RULE-003` menuliskan
status `Draft` berpasangan dengan tempat tidur `Reserved`. Dibaca harfiah, itu berarti episode
`Draft` selalu memegang pemesanan. Tetapi dua aturan lain membuktikan sebaliknya:

- `RWI-RULE-002` membuat pemesanan gugur setelah 2 jam, sedangkan `RWI-RULE-022` baru
  membatalkan episode `Draft` setelah 1 hari. Di antara jam ke-2 dan jam ke-24, episode `Draft`
  hidup tanpa pemesanan.
- `RWI-RULE-015` menyatakan bila penempatan ditolak karena tempat tidur sudah diambil pasien
  lain, episode **tetap** `Draft` dan seluruh isian admisi tetap utuh — juga tanpa pemesanan.

Karena itu pasangan pada tabel `RWI-RULE-003` diperlakukan sebagai **gambaran keadaan yang lazim**,
bukan sebagai invariant. Invariant yang sebenarnya adalah `INV-INP-09`. Ini klarifikasi arsitektur,
bukan perubahan aturan bisnis; tidak ada satu pun keputusan yang dibatalkan olehnya.

#### Invariant yang **tidak** dapat dijaga aggregate ini

| ID | Invariant | Kenapa di luar aggregate | Cara menjaganya |
| --- | --- | --- | --- |
| `INV-INP-02` | Satu tempat tidur pada satu waktu dipegang **paling banyak satu** pemesanan aktif atau satu penempatan aktif, tidak boleh keduanya dan tidak boleh berganda | Melibatkan banyak episode sekaligus, sehingga tidak bisa diperiksa dari dalam satu episode | Jaminan keunikan pada penyimpanan ditambah penguncian baris tempat tidur saat memesan, menempatkan, dan memindahkan. Rincian caranya adalah pekerjaan `/qv-design` |

Ini bukan kelemahan yang disembunyikan, melainkan sifat yang wajar: aturan "satu bed satu pasien"
memang aturan antar-episode. Yang penting adalah menyatakannya, bukan berpura-pura aggregate bisa
menjaganya.

#### Perintah bisnis pada aggregate ini

| ID | Perintah | Siapa yang boleh | Prasyarat | Akibat |
| --- | --- | --- | --- | --- |
| `CMD-INP-01` | Buka Admisi | Petugas admisi | Pasien terdaftar; kunjungan tersedia atau dibuat | Episode `Draft`, DPJP pertama ditetapkan |
| `CMD-INP-02` | Pesan Tempat Tidur | Petugas admisi | Episode `Draft`; tempat tidur `Available` dan dapat dipesan | Pemesanan aktif, berlaku sepanjang batas pada pengaturan |
| `CMD-INP-03` | Tempatkan Pasien | Petugas admisi | Episode `Draft`; **Kelayakan Penempatan** terpenuhi | Episode `Admitted`, penempatan aktif dibuka |
| `CMD-INP-04` | Batalkan Admisi | Petugas admisi selagi `Draft`; supervisor atau kepala ruangan selagi `Admitted` | Belum ada catatan klinis; alasan wajib | Episode `Cancelled`, pemesanan dan penempatan ditutup |
| `CMD-INP-05` | Pindahkan Pasien | Kepala ruangan, perawat pelaksana, supervisor, atau DPJP aktif | Episode `Admitted`; tempat tidur tujuan lolos Kelayakan Penempatan; alasan wajib | Penempatan lama ditutup dan penempatan baru dibuka **dalam satu tindakan utuh** |
| `CMD-INP-06` | Alihkan DPJP | Kepala ruangan atau supervisor | Episode belum `Closed`; alasan wajib | Penugasan lama ditutup, penugasan baru dibuka |
| `CMD-INP-07` | Tugaskan Perawat | Kepala ruangan | Episode `Admitted` atau `DischargePending` | Penugasan perawat lama ditutup, yang baru dibuka |
| `CMD-INP-08` | Putuskan Pasien Boleh Pulang | DPJP aktif | Episode `Admitted`; cara pulang dipilih | Episode `DischargePending` |
| `CMD-INP-09` | Susun dan Tandatangani Resume Pulang | DPJP aktif | Episode `DischargePending` | Resume tertandatangani |
| `CMD-INP-10` | Tandai Butir Daftar Periksa | Petugas admisi | Episode `DischargePending` | Butir tertandai |
| `CMD-INP-11` | Tandai Kelayakan Keuangan | Petugas kasir atau billing | Episode belum `Closed`; catatan wajib | Penandaan baru tersimpan |
| `CMD-INP-12` | Tutup Episode | Petugas admisi, atau supervisor bila menembus gerbang keuangan | Seluruh syarat penutupan terpenuhi | Episode `Closed`, penempatan ditutup, tempat tidur kembali tersedia |
| `CMD-INP-13` | Buka Sesi Koreksi | Supervisor | Episode `Closed`; alasan wajib | Sesi koreksi terbuka. Status episode **tidak** berubah |
| `CMD-INP-14` | Tutup Sesi Koreksi | Supervisor | Ada sesi koreksi terbuka | Sesi ditutup, daftar perubahan tersimpan |

**Kelayakan Penempatan.** Perintah `CMD-INP-03` dan `CMD-INP-05` tidak memeriksa syarat satu per
satu di dalam badannya, melainkan memanggil satu pemeriksaan bernama Kelayakan Penempatan yang
isinya berupa **daftar aturan**. Pada scope ini daftar itu berisi tiga aturan:

1. tempat tidur aktif dan tidak sedang `Cleaning`, `Maintenance`, atau `Blocked`;
2. tempat tidur tidak sedang dipegang pemesanan atau penempatan milik episode lain;
3. bila ada pemesanan milik episode ini yang masih berlaku, pemesanan itu dipakai.

Bentuk ini dipilih supaya keputusan `DEC-INP-004` tentang isolasi dan pemisahan jenis kelamin
dapat ditambahkan kelak sebagai aturan keempat, **tanpa membongkar** perintah penempatan maupun
perpindahan. Inilah pemenuhan syarat kedua pada bagian A.3.

---

## F. Model relasi

### F.1 Relasi yang material

| Sumber | Tujuan | Makna bisnis | Kardinalitas | Wajib | Ketergantungan lifecycle |
| --- | --- | --- | --- | ---: | --- |
| `CON-INP-001` Episode | `CON-EXT-002` Kunjungan | Episode berlangsung di dalam satu kunjungan | 1 : 1 | Ya | Episode tidak dapat ada tanpa kunjungan |
| `CON-INP-001` Episode | `CON-EXT-001` Pasien | Episode merawat satu pasien | banyak : 1 | Ya | Diturunkan dari kunjungan, tidak disimpan ulang |
| `CON-INP-001` Episode | `CON-INP-002` Penugasan DPJP | Episode punya riwayat DPJP | 1 : banyak | Ya, minimal satu | Ikut mati bersama episode |
| `CON-INP-002` Penugasan DPJP | `CON-EXT-008` Dokter | Satu penugasan menunjuk satu dokter | banyak : 1 | Ya | Dokter hidup mandiri |
| `CON-INP-001` Episode | `CON-INP-003` Penugasan Perawat | Episode punya riwayat perawat | 1 : banyak | **Tidak** | Ikut mati bersama episode |
| `CON-INP-001` Episode | `CON-INP-006` Pemesanan | Episode dapat memesan tempat tidur | 1 : banyak sepanjang waktu, paling banyak 1 aktif | Tidak | Ikut mati bersama episode |
| `CON-INP-001` Episode | `CON-INP-007` Penempatan | Episode menempati tempat tidur, mungkin berpindah-pindah | 1 : banyak sepanjang waktu, tepat 1 aktif saat `Admitted` | Ya saat `Admitted` | Ikut mati bersama episode |
| `CON-INP-006` Pemesanan | `CON-EXT-004` Tempat Tidur | Pemesanan mengunci satu tempat tidur | banyak : 1 | Ya | Tempat tidur hidup mandiri |
| `CON-INP-007` Penempatan | `CON-EXT-004` Tempat Tidur | Penempatan menghuni satu tempat tidur | banyak : 1 | Ya | Tempat tidur hidup mandiri |
| `CON-INP-001` Episode | `CON-INP-004` Resume Pulang | Episode diringkas satu resume | 1 : 0..1 | Tidak sampai rencana pulang | Ikut mati bersama episode |
| `CON-INP-001` Episode | `CON-INP-005` Penandaan Daftar Periksa | Episode menandai butir administrasi | 1 : banyak | Tidak | Ikut mati bersama episode |
| `CON-INP-005` Penandaan | `CON-INP-014` Butir Daftar Periksa | Penandaan merujuk satu butir | banyak : 1 | Ya | Butir hidup mandiri |
| `CON-INP-001` Episode | `CON-INP-008` Penandaan Kelayakan Keuangan | Episode punya riwayat penandaan keuangan | 1 : banyak | Tidak | Ikut mati bersama episode |
| `CON-INP-001` Episode | `CON-INP-009` Riwayat Status | Episode meninggalkan jejak perpindahan status | 1 : banyak | Ya, minimal satu | **Tidak ikut dihapus.** Riwayat bertahan melewati koreksi |
| `CON-INP-001` Episode | `CON-INP-010` Sesi Koreksi | Episode `Closed` dapat dibuka untuk dikoreksi | 1 : banyak | Tidak | Ikut mati bersama episode |

### F.2 Kenapa penempatan berbentuk baris berperiode

Ini pemenuhan syarat pertama pada bagian A.3, dan alasannya perlu ditulis supaya tidak hilang.

Ada dua cara memodelkan penghunian tempat tidur:

| Cara | Bentuknya | Akibatnya |
| --- | --- | --- |
| Penanda keadaan terakhir | Satu kolom pada episode berisi tempat tidur yang sedang ditempati | Riwayat lokasi hilang setiap pasien pindah. Pindah kelas tidak dapat ditelusuri. Charge kamar per hari tidak dapat direkonstruksi |
| **Baris berperiode** | Satu baris per tempat tidur yang pernah ditempati, dengan waktu mulai dan waktu berakhir | Riwayat lokasi utuh. Pindah kelas terbaca. Charge kamar per hari dapat direkonstruksi. Siap menjadi bahan pengiriman interoperabilitas |

Arsitektur ini memilih yang kedua. Contohnya, episode Tn. Budi 21 sampai 25 September 2026:

| Tempat tidur | Kelas saat itu | Mulai | Berakhir |
| --- | --- | --- | --- |
| Melati 3B | Kelas 2 | 21 Sept 10:40 | 23 Sept 09:30 |
| Anggrek 1A | Kelas 1 | 23 Sept 09:30 | 25 Sept 13:10 |

Dari dua baris itu dapat dijawab tiga pertanyaan sekaligus tanpa data tambahan: pasien pindah
kelas dari 2 ke 1 pada 23 September; lama dirawat 4 hari; dan charge kamar terbagi 2 hari kelas 2
ditambah 2 hari kelas 1. Inilah sebabnya `RWI-RULE-007` yang menyatakan "kelas yang ditagihkan
mengikuti kamar yang ditempati" dapat dijalankan tanpa menyimpan kelas secara terpisah.

Bentuk ini juga yang membuat `DEC-INP-005` dapat dijawab ke arah mana pun kelak: bila riwayat
lokasi ternyata harus dikirim sebagai bagian kunjungan, data mentahnya sudah lengkap dan tinggal
dibaca; tidak ada yang perlu dibongkar.

### F.3 Bayi baru lahir dan ibunya

`RWI-RULE-014` menetapkan bayi mendapat episode dan kunjungan sendiri, dan boks bayi terdaftar
sebagai tempat tidur tersendiri di kamar ibu. Dalam arsitektur ini itu berarti **dua episode yang
sepenuhnya terpisah**, masing-masing dengan penempatannya sendiri, dan tidak ada hubungan khusus
antara keduanya.

Yang belum ada: penanda bahwa bayi ini dirawat gabung dengan ibu itu. Tanpa penanda, sistem tidak
dapat menjawab "bayi siapa yang ada di boks kamar Melati 3" selain lewat kesamaan kamar. Ini
dicatat sebagai `ARCH-GAP-002` pada bagian M, tidak memblokir, dan dikembalikan ke `/grill-me`.

---

## G. Model lifecycle dan status

### G.1 Lifecycle episode rawat inap

Status awal: `Draft`.

| Dari | Tindakan | Ke | Siapa yang boleh | Prasyarat | Invariant yang diperiksa |
| --- | --- | --- | --- | --- | --- |
| — | Buka admisi | `Draft` | Petugas admisi | Kunjungan tersedia atau dibuat | `INV-INP-03`, `INV-INP-04` |
| `Draft` | Tempatkan pasien | `Admitted` | Petugas admisi | Kelayakan Penempatan terpenuhi | `INV-INP-01`, `INV-INP-02` |
| `Draft` | Batalkan | `Cancelled` | Petugas admisi | Alasan wajib | Pemesanan ditutup |
| `Draft` | Telantar melewati batas | `Cancelled` | **Sistem**, dihitung saat dibaca | Tidak disentuh melewati batas pada pengaturan | Kunjungan yang terlanjur dibuat ikut ditandai batal |
| `Admitted` | Batalkan | `Cancelled` | Supervisor atau kepala ruangan | Belum ada catatan klinis; alasan wajib | Penempatan ditutup bersamaan |
| `Admitted` | Pindahkan pasien | `Admitted` | Kepala ruangan, perawat, supervisor, DPJP aktif | Kelayakan Penempatan tujuan terpenuhi; alasan wajib | `INV-INP-07` — utuh atau batal seluruhnya |
| `Admitted` | Putuskan boleh pulang | `DischargePending` | DPJP aktif | Cara pulang dipilih | — |
| `DischargePending` | Tutup episode | `Closed` | Petugas admisi | Kelima syarat penutupan terpenuhi | Penempatan ditutup bersamaan |
| `DischargePending` | Tutup episode menembus gerbang keuangan | `Closed` | Supervisor | Alasan wajib; episode ditandai dan masuk laporan | Penempatan ditutup bersamaan |
| `Closed` | Buka sesi koreksi | `Closed` | Supervisor | Alasan wajib | Status **tidak** berubah, lihat G.4 |

Status terminal: `Closed` dan `Cancelled`.

**Perpindahan yang secara tegas tidak diizinkan:** `Closed` → `Admitted`, `Cancelled` → mana pun,
`DischargePending` → `Admitted`, dan `Draft` → `DischargePending`. Pasien yang benar-benar kembali
dirawat selalu mendapat episode baru dan kunjungan baru.

### G.2 Lifecycle pemesanan tempat tidur

| Dari | Tindakan | Ke | Pelaku |
| --- | --- | --- | --- |
| — | Pesan | `Aktif` | Petugas admisi |
| `Aktif` | Dipakai untuk menempatkan | `Terpakai` | Petugas admisi |
| `Aktif` | Lewat batas waktu | `Gugur` | **Sistem**, dihitung saat dibaca |
| `Aktif` | Admisi dibatalkan | `Dibatalkan` | Ikut perintah pembatalan |

Yang penting dari tabel ini: kolom "Pelaku" untuk baris `Gugur` berisi **sistem**, bukan orang.
`RWI-RULE-031` aturan 6 menuntut baris riwayat semacam itu ditandai sebagai dilakukan sistem,
sehingga audit tidak salah menuduh siapa pun.

### G.3 Lifecycle penempatan tempat tidur

| Dari | Tindakan | Ke | Pemicu |
| --- | --- | --- | --- |
| — | Pasien menempati | `Aktif` | `CMD-INP-03` Tempatkan Pasien |
| `Aktif` | Pasien pindah | `Berakhir` | `CMD-INP-05`, bersamaan dengan pembukaan penempatan baru |
| `Aktif` | Episode ditutup | `Berakhir` | `CMD-INP-12` |
| `Aktif` | Admisi dibatalkan | `Berakhir` | `CMD-INP-04` |

Tidak ada satu pun jalur yang menutup penempatan tanpa menutup atau memindahkan episodenya. Inilah
bentuk nyata `INV-INP-01` dan `INV-INP-07`.

### G.4 Sesi koreksi — dan kenapa ia bukan status keenam

Ini keputusan arsitektur yang paling perlu dijelaskan, karena menyelesaikan ketegangan antara dua
aturan yang sama-sama sudah dikunci.

**Ketegangannya.** `RWI-DEC-009` mengunci status episode hanya lima nilai, dan `RWI-AC-004` menguji
bahwa nilai di luar itu ditolak. Sementara `RWI-RULE-020` mengharuskan ada jendela waktu ketika
episode `Closed` boleh dibetulkan catatannya oleh supervisor. Kalau jendela itu dibuat sebagai
status keenam, dua keputusan yang sudah disetujui langsung dilanggar.

**Penyelesaiannya.** Jendela itu dimodelkan sebagai konsep tersendiri, `CON-INP-010` Sesi Koreksi,
yang punya lifecycle sendiri dan **tidak menyentuh status episode sama sekali**.

| Yang ditanyakan | Jawabannya |
| --- | --- |
| Status episode selama sesi berjalan | Tetap `Closed` |
| Apakah tempat tidur kembali dipegang | Tidak |
| Apakah pasien muncul di census | Tidak |
| Apakah lama dirawat bertambah | Tidak |
| Apa yang membuka penyuntingan | Adanya sesi koreksi yang terbuka, bukan status episode |
| Apa yang tersimpan | Nama supervisor, waktu buka, alasan, waktu tutup, dan daftar apa saja yang berubah |

Contohnya persis kasus pada `RWI-RULE-020`: episode Ibu Sari ditutup 15 Agustus dengan cara pulang
"kabur". Pada 17 Agustus supervisor membuka sesi koreksi, mengubah cara pulang, lalu menutup
sesinya. Sepanjang 17 Agustus itu status episode tetap `Closed`, bed `MELATI-03` tetap ditempati
pasien lain tanpa terganggu, dan lama dirawat Ibu Sari tetap 3 hari.

**Satu akibat yang perlu diketahui `/qv-design`.** Karena status episode tidak berubah selama sesi
koreksi, tabel riwayat status pada `CON-INP-009` **tidak** akan mencatat apa-apa. Karena itu daftar
perubahan selama koreksi harus disimpan pada sesi koreksinya sendiri, pada tingkat kolom yang
berubah. Tanpa itu, koreksi menjadi satu-satunya perubahan pada episode yang tidak meninggalkan
jejak — dan itu justru bertentangan dengan tujuan `RWI-RULE-031`.

### G.5 Lima syarat penutupan episode

Ini tempat berkumpulnya paling banyak aturan, jadi disajikan sebagai satu daftar periksa.

| No | Syarat | Dasar | Siapa yang memenuhi |
| ---: | --- | --- | --- |
| 1 | Episode berstatus `DischargePending` | `RWI-RULE-003` | DPJP lewat `CMD-INP-08` |
| 2 | Cara pulang sudah dipilih dan syarat khas cara itu terpenuhi | `RWI-RULE-011` | DPJP |
| 3 | Resume pulang sudah ditandatangani DPJP | `RWI-RULE-032` aturan 4 | DPJP |
| 4 | Seluruh butir wajib pada daftar periksa administrasi sudah ditandai | `RWI-RULE-018` | Petugas admisi |
| 5 | Kelayakan keuangan bernilai `Cleared` | `RWI-RULE-009`, `RWI-RULE-028` | Petugas kasir atau billing |

Bila syarat 5 belum terpenuhi, hanya supervisor yang boleh menutup, dengan alasan wajib, dan
episode itu ditandai serta masuk laporan tersendiri. Keempat syarat lainnya **tidak** punya jalan
keluar.

---

## H. Tanggung jawab authorization

Bagian ini menyatakan batas wewenang bisnis. Tidak ada peran baru yang dikarang; seluruhnya
diambil dari aturan yang sudah dikunci.

### H.1 Wewenang per tindakan

| Tindakan | Memulai | Melihat | Mengubah | Menyetujui | Membatalkan atau mengoreksi |
| --- | --- | --- | --- | --- | --- |
| Admisi | Petugas admisi | Petugas admisi, perawat, DPJP, supervisor | Petugas admisi | — | Petugas admisi selagi `Draft`; supervisor atau kepala ruangan selagi `Admitted` |
| Pemesanan tempat tidur | Petugas admisi | Sama seperti di atas | Petugas admisi | — | Petugas admisi |
| Penempatan dan perpindahan | Kepala ruangan, perawat pelaksana, supervisor, DPJP aktif | Sama seperti di atas | Pelaku yang sama | — | Tidak ada pembatalan tersendiri; koreksi lewat perpindahan berikutnya |
| Pengalihan DPJP | Kepala ruangan atau supervisor | Sama seperti di atas | Pelaku yang sama | — | Lewat pengalihan berikutnya |
| Penugasan perawat | Kepala ruangan | Sama seperti di atas | Kepala ruangan | — | Lewat penugasan berikutnya |
| Keputusan pulang | DPJP aktif | Sama seperti di atas | DPJP aktif | — | Supervisor lewat sesi koreksi |
| Resume pulang | DPJP aktif | Sama seperti di atas | DPJP aktif sampai episode ditutup | DPJP aktif dengan tanda tangan | Supervisor lewat sesi koreksi |
| Daftar periksa administrasi | Petugas admisi | Sama seperti di atas | Petugas admisi | — | Petugas admisi selama episode belum ditutup |
| Kelayakan keuangan | Petugas kasir atau billing | Sama seperti di atas | Petugas kasir atau billing | — | Penandaan berikutnya |
| Penutupan episode | Petugas admisi, atau supervisor bila menembus gerbang keuangan | Sama seperti di atas | — | — | Supervisor lewat sesi koreksi |
| Sesi koreksi | Supervisor | Supervisor dan auditor | Supervisor | — | — |
| Pengaturan dan butir daftar periksa | Admin | Admin | Admin | — | Admin |

### H.2 Dua tingkat kewenangan yang berbeda sifatnya

Ini pembedaan yang menentukan bentuk arsitektur, dan sudah dibuktikan capability map.

| Tingkat | Contohnya | Dijaga di mana |
| --- | --- | --- |
| **Kewenangan peran** — "peran ini boleh melakukan tindakan ini" | Hanya kasir yang boleh menandai kelayakan keuangan | Mesin hak akses yang sudah ada, berbasis pasangan controller dan action |
| **Kewenangan per pasien** — "orang ini boleh melakukan tindakan ini **terhadap pasien ini**" | Hanya DPJP aktif episode itu yang boleh meminta perpindahan | **Tidak dapat** dijaga mesin hak akses yang ada. Harus dijaga di dalam service Rawat Inap |

Dasar: `RWI-RULE-030` aturan 6, dan bukti `RWI-TF-014` yang menunjukkan mesin hak akses hanya
mengenal peran terhadap endpoint.

**Konsekuensi arsitektur:** kewenangan per pasien adalah tanggung jawab domain, bukan tanggung
jawab infrastruktur. Karena itu ia harus menjadi bagian dari perintah bisnis `CMD-INP-05` dan
`CMD-INP-06`, bukan lapisan yang dipasang di luar. Risiko "lupa memanggil penjaga" sudah dicatat
sebagai `RWI-RISK-004` dan diturunkan oleh kewajiban test pada `RWI-DEC-051`.

---

## I. Model audit dan histori

### I.1 Perubahan yang wajib punya jejak tahan lama

| Kejadian | Yang wajib tersimpan | Disimpan di mana |
| --- | --- | --- |
| Perpindahan status episode | Dari status, ke status, pelaku, waktu, alasan, nomor urut, dan penanda dilakukan sistem atau orang | `CON-INP-009` Riwayat Status |
| Pemesanan dibuat, dipakai, gugur, dibatalkan | Pelaku, waktu, tempat tidur, alasan | `CON-INP-006` ditambah `CON-INP-009` |
| Penempatan dibuka dan ditutup | Pelaku, waktu mulai, waktu berakhir, tempat tidur, alasan perpindahan | `CON-INP-007` |
| Penugasan dan pengalihan DPJP | Dokter, masa berlaku, pengalih, alasan | `CON-INP-002` |
| Penugasan dan penggantian perawat | Perawat, masa berlaku, penugas | `CON-INP-003` |
| Penandaan kelayakan keuangan | Nilai, pelaku, waktu, catatan | `CON-INP-008` |
| Penandaan butir daftar periksa | Butir, pelaku, waktu | `CON-INP-005` |
| Penandatanganan resume | Penandatangan, waktu | `CON-INP-004` |
| Penutupan yang menembus gerbang keuangan | Supervisor, waktu, alasan, penanda | `CON-INP-009` ditambah penanda pada episode |
| Sesi koreksi | Supervisor, waktu buka, alasan, waktu tutup, daftar kolom yang berubah | `CON-INP-010` |

### I.2 Tiga sifat yang wajib dipenuhi

| Sifat | Isinya | Dasar |
| --- | --- | --- |
| **Ditulis bersamaan** | Baris jejak ditulis dalam transaksi yang sama dengan perubahan yang dijejakinya. Berhasil dua-duanya, atau tidak ada yang berubah | `RWI-RULE-031` aturan 3 |
| **Satu pintu** | Seluruh perubahan status wajib lewat satu titik di dalam service. Tidak boleh ada jalur yang menyetel status langsung | `RWI-RULE-031` aturan 4 |
| **Tidak dapat diubah** | Baris jejak tidak dapat disunting dan tidak dapat dihapus. Koreksi dilakukan dengan menambah baris baru | `RWI-RULE-031` aturan 5 |

### I.3 Apa yang tidak dianggap jejak audit

Catatan aktivitas yang ditulis `LoggerService` **bukan** jejak audit domain. Keluarannya berupa
berkas log untuk Grafana Loki, tidak terikat transaksi database, tidak dapat disaring per episode,
dan tidak dapat ditampilkan sebagai riwayat pasien. Bukti: `RWI-TF-019`.

Catatan aktivitas tetap berguna untuk menelusuri kejadian teknis, tetapi tidak boleh dipakai
sebagai satu-satunya bukti bahwa sebuah tindakan bisnis pernah terjadi.

---

## J. Model integrasi

### J.1 Batas internal

| ID | Produsen | Konsumen | Tujuan bisnis | Sumber kebenaran | Arah | Sifat |
| --- | --- | --- | --- | --- | --- | --- |
| `INT-INP-01` | `CTX-REG` | `CTX-INP-CARE` | Kunjungan sebagai jangkar episode | `CTX-REG` | Baca | Sinkron |
| `INT-INP-02` | `CTX-MST` | `CTX-INP-CARE` | Daftar tempat tidur, kamar, unit layanan, kelas pasien | `CTX-MST` | Baca | Sinkron |
| `INT-INP-03` | `CTX-INP-CARE` | `CTX-MST` | Menuliskan salinan status ketersediaan tempat tidur | `CTX-INP-CARE` untuk maknanya; `CTX-MST` untuk kolomnya | Tulis | Sinkron, satu transaksi |
| `INT-INP-04` | `CTX-WFP` | `CTX-INP-CARE` | Dokter dan pegawai untuk DPJP dan perawat | `CTX-WFP` | Baca | Sinkron |
| `INT-INP-05` | `CTX-PAT` | `CTX-INP-CARE` | Identitas pasien | `CTX-PAT` | Baca | Sinkron |

**Tentang `INT-INP-03` dan risikonya.** Ini satu-satunya arah tulis keluar. Karena salinan dan
sumbernya berada di dua context yang berbeda, selisih tetap mungkin terjadi — misalnya bila kelak
ada jalur lain yang menyetel kolom itu. Karena itu `RWI-RULE-027` aturan 6 mewajibkan satu laporan
selisih, dan arsitektur ini memperlakukan laporan itu sebagai **bagian dari kontrak integrasi**,
bukan sebagai fitur tambahan yang boleh ditunda.

Cara memeriksanya sederhana: sebuah tempat tidur dianggap selisih bila kolom statusnya menyatakan
`Available` padahal masih ada penempatan aktif atasnya, atau sebaliknya menyatakan `Occupied`
padahal tidak ada penempatan aktif mana pun.

### J.2 Batas eksternal

**Tidak ada satu pun batas eksternal yang dirancang pada revisi ini.**

Ini bukan karena tidak dibutuhkan, melainkan karena keputusannya belum ada. Baseline
`ID-INP-INT-001` sampai `ID-INP-INT-005` menandai interoperabilitas SATUSEHAT sebagai kepedulian
berbobot tinggi untuk integrasi, audit, dan billing sekaligus, dan PRD baris 814 menyebutnya. Namun
`DEC-INP-005` belum terjawab, sehingga merancang kontraknya berarti mengarang kebijakan.

Yang **sudah** disiapkan arsitektur ini supaya keputusan itu tidak mahal kelak: seluruh bahan yang
dibutuhkan pengiriman sudah tersimpan dalam bentuk yang dapat dibaca ulang — riwayat lokasi pada
`CON-INP-007`, riwayat penanggung jawab pada `CON-INP-002`, riwayat status pada `CON-INP-009`, dan
ringkasan pemulangan pada `CON-INP-004`.

### J.3 Kejadian bisnis yang layak diumumkan

Daftar berikut adalah **fakta bisnis**, bukan rancangan mekanisme pengiriman pesan. Capability map
tidak menemukan satu pun sarana antrean pesan atau kotak keluar di dalam source, sehingga cara
mewujudkannya diserahkan ke `/qv-design`.

| ID | Kejadian | Kapan terjadi | Siapa yang mungkin peduli |
| --- | --- | --- | --- |
| `EVT-INP-01` | Episode diaktifkan | Episode menjadi `Admitted` | Billing, interoperabilitas, census |
| `EVT-INP-02` | Tempat tidur dipesan | Pemesanan dibuat | Papan ketersediaan tempat tidur |
| `EVT-INP-03` | Pemesanan gugur | Saat dibaca setelah lewat batas | Papan ketersediaan tempat tidur |
| `EVT-INP-04` | Pasien menempati tempat tidur | Penempatan dibuka | Billing untuk charge kamar, interoperabilitas |
| `EVT-INP-05` | Pasien berpindah tempat tidur | Perpindahan berhasil | Billing bila kelas berubah, interoperabilitas |
| `EVT-INP-06` | DPJP dialihkan | Pengalihan berhasil | Interoperabilitas, laporan |
| `EVT-INP-07` | Pasien diputuskan boleh pulang | Episode menjadi `DischargePending` | Farmasi untuk obat pulang, kasir |
| `EVT-INP-08` | Resume pulang ditandatangani | Penandatanganan | Interoperabilitas, rekam medis |
| `EVT-INP-09` | Episode ditutup | Episode menjadi `Closed` | Billing, interoperabilitas, papan ketersediaan |
| `EVT-INP-10` | Episode dibatalkan | Episode menjadi `Cancelled` | Papan ketersediaan, kunjungan |
| `EVT-INP-11` | Episode ditutup menembus gerbang keuangan | Penutupan oleh supervisor | Laporan pengecualian, keuangan |

---

## K. Dampak billing

**Klasifikasi: berdampak pada charge, tetapi dependency billing belum terselesaikan.**

### K.1 Yang sudah pasti

| Hal | Ketetapannya | Dasar |
| --- | --- | --- |
| Kelas yang ditagihkan | Selalu mengikuti kamar yang ditempati, bukan kelas hak pasien | `RWI-RULE-007` |
| Pasien titipan | Tidak ada pada MVP. Tidak ada kelas hak yang disimpan terpisah | `RWI-RULE-013` |
| Perubahan kelas | Tersimpan sebagai riwayat lewat `CON-INP-007` | `RWI-RULE-007` |
| Kelayakan keuangan | Memblokir penutupan; sumbernya penandaan manual sementara | `RWI-RULE-009`, `RWI-RULE-028` |

### K.2 Yang belum ada dan akibatnya

Modul `BillingManagement` belum punya kemampuan transaksi. Akibatnya **tidak satu pun charge kamar
yang tercatat selama MVP berjalan**, padahal charge kamar per hari adalah komponen terbesar
tagihan rawat inap.

Arsitektur ini tidak menambal keadaan itu, karena menetapkan aturan tarif, pemicu posting, atau
kebijakan payer berarti mengarang kebijakan keuangan. Yang dilakukan arsitektur ini hanya satu:
**menjamin datanya dapat direkonstruksi kelak.**

Contohnya, dari dua baris penempatan Tn. Budi pada bagian F.2, charge kamar dapat dihitung mundur:
2 hari kelas 2 ditambah 2 hari kelas 1. Tidak ada informasi yang hilang, walaupun tidak ada satu
rupiah pun yang diposting saat itu.

### K.3 Risiko yang perlu diketahui pemilik

Bila `BillingManagement` baru operasional setelah ratusan episode berjalan, rekonstruksi charge
kamar untuk episode lama akan menjadi pekerjaan tersendiri. Arsitektur ini membuatnya **mungkin**,
tetapi tidak membuatnya **otomatis**. Keputusan apakah episode lama ikut ditagihkan mundur adalah
keputusan keuangan yang belum ada pemiliknya.

---

## L. Dampak keselamatan klinis

**Klasifikasi: relevan terhadap keselamatan.**

### L.1 Titik yang relevan terhadap keselamatan pada scope ini

| Titik | Kenapa relevan | Bagaimana arsitektur membuat batasnya jelas |
| --- | --- | --- |
| Penempatan pasien pada tempat tidur | Pasien yang ditempatkan pada tempat tidur yang salah atau yang sudah ditempati orang lain | `INV-INP-02` dinyatakan sebagai invariant antar-episode yang wajib dijamin penyimpanan, bukan diserahkan pada kehati-hatian petugas |
| Perpindahan pasien | Pasien tercatat tanpa tempat tidur di tengah proses | `INV-INP-07`, perpindahan utuh atau batal seluruhnya |
| Kewenangan DPJP | Dokter yang bukan penanggung jawab memindahkan pasien | `INV-INP-03` ditambah kewenangan per pasien pada bagian H.2 |
| Kejelasan penanggung jawab | Tidak jelas siapa yang bertanggung jawab pada tanggal tertentu | `CON-INP-002` berbentuk riwayat berperiode, bukan satu kolom yang ditimpa |
| Penutupan episode | Episode ditutup padahal syarat klinisnya belum selesai | Lima syarat penutupan pada G.5; hanya syarat keuangan yang punya jalan keluar |

### L.2 Keputusan keselamatan yang belum terselesaikan

Ketiganya berada **di luar** scope arsitektur ini, dan itu sengaja:

| Butir | Decision ID | Kenapa tidak dirancang di sini |
| --- | --- | --- |
| Isolasi dan pemisahan jenis kelamin | `DEC-INP-004` | Satu-satunya butir `CONFLICT`. Menyentuh pengendalian infeksi dan privasi. Titik penyisipannya sudah disiapkan pada Kelayakan Penempatan |
| Serah terima klinis antar shift | `DEC-INP-006` | Ditandai `SAFETY_CHECK` oleh baseline, dan belum pernah dibahas sama sekali |
| Aturan pasien meninggal dan kabur | `DEC-INP-007` | Sisi klinisnya masih terbuka |

### L.3 Satu hal yang perlu dinyatakan terus terang

Arsitektur ini merancang penempatan tempat tidur **tanpa** aturan isolasi dan tanpa aturan
pemisahan jenis kelamin, karena keduanya belum diputuskan. Artinya, bila `INP-S01` dan `INP-S02`
diimplementasikan apa adanya, sistem akan mengizinkan pasien yang butuh isolasi ditempatkan di
kamar biasa, dan mengizinkan pasien laki-laki dan perempuan sekamar.

Ini bukan kelalaian arsitektur, melainkan keadaan yang sudah disadari dan ditandai sebagai gerbang
keras pada dokumen keputusan. Yang dilakukan arsitektur ini adalah menyiapkan tempat penyisipannya
supaya aturan itu dapat ditambahkan tanpa membongkar apa pun. Modul **tidak boleh** dipakai
melayani pasien sungguhan sebelum `DEC-INP-004` terjawab.

---

## M. Gap arsitektur

| ID | Gap | Sifat | Dampak | Diarahkan ke |
| --- | --- | --- | --- | --- |
| `ARCH-GAP-001` | Sesi koreksi menuntut penyimpanan perubahan pada tingkat kolom, karena status episode tidak berubah selama sesi berjalan sehingga riwayat status tidak mencatat apa pun | Tidak memblokir | Tanpa itu, koreksi menjadi satu-satunya perubahan pada episode yang tidak berjejak | `/qv-design`, sudah dinyatakan pada G.4 |
| `ARCH-GAP-002` | Tidak ada penanda bahwa bayi dirawat gabung dengan ibunya. Dua episode terpisah tanpa hubungan | Tidak memblokir | Sistem tidak dapat menjawab bayi siapa yang ada di boks kamar mana selain lewat kesamaan kamar. Menyentuh kepastian identitas | `/grill-me` |
| `ARCH-GAP-003` | Kepergian fisik pasien tidak dimodelkan sebagai kejadian tersendiri. Tempat tidur baru dilepas saat penutupan administratif | Tidak memblokir | Selama jeda antara pasien pulang dan episode ditutup, tempat tidur terbaca terisi padahal kosong. Daftar pantau dengan ambang 4 jam mengakui jeda ini memang terjadi | `/grill-me` |
| `ARCH-GAP-004` | Tidak ada aturan yang melarang satu pasien punya dua episode rawat inap aktif | Tidak memblokir | Usulan: jadikan invariant `INV-INP-10`. Belum diputuskan, jadi belum dimasukkan | `/grill-me` |
| `ARCH-GAP-005` | Resume pulang tidak menyimpan riwayat versi. Koreksi menimpa isi sebelumnya | Tidak memblokir | Baseline `ID-INP-CAP-019` menanyakan riwayat versi resume. Menyentuh rekam medis | `/grill-me` |
| `ARCH-GAP-006` | Kejadian bisnis pada J.3 belum punya sarana pengiriman. Tidak ditemukan antrean pesan atau kotak keluar di dalam source | Tidak memblokir pada MVP | Selama seluruh konsumen berada di dalam satu aplikasi, pemanggilan langsung memadai. Menjadi masalah begitu `DEC-INP-005` terjawab | `/qv-design` |
| `ARCH-GAP-007` | Ketegangan kepemilikan pada `INT-INP-03`: Rawat Inap menulis kolom milik `CTX-MST` | Tidak memblokir | Sudah diputuskan `RWI-DEC-039` dan dikurangi laporan selisih. Tetap perlu persetujuan pemilik `MasterData` | `RWI-OQ-033`, tindakan organisasi |

Tidak ada satu pun gap di atas yang memaksa pengarangan kebijakan bisnis. Ketujuhnya dicatat apa
adanya, tidak ditambal diam-diam.

---

## N. Kesiapan arsitektur

### N.1 Status

**`DOMAIN_ARCHITECTURE_PARTIAL`**

> **Diperbarui 2 September 2026.** Penilaian pada bagian N adalah penilaian pass pertama dan tetap
> dipertahankan sebagai jejak. Penilaian current untuk scope Dokter Rawat Inap ada pada bagian AB,
> dan berstatus `DOMAIN_ARCHITECTURE_READY`. Status keseluruhan dokumen tetap
> `DOMAIN_ARCHITECTURE_PARTIAL` karena slice lain belum dirancang.

### N.2 Slice yang siap dan berdiri sendiri

Sembilan slice berikut dinyatakan **siap dan berdiri sendiri** dari penilaian parsial ini, dan
boleh diteruskan ke `design-business-module`:

`INP-S01`, `INP-S02`, `INP-S03`, `INP-S04`, `INP-S07` untuk tiga cara pulang, `INP-S08`,
`INP-S12`, `INP-S13`, `INP-S14`

Alasan kesiapannya, diperiksa satu per satu terhadap syarat `DOMAIN_ARCHITECTURE_READY`:

| Syarat kesiapan | Terpenuhi | Keterangan |
| --- | :---: | --- |
| Slice requirement memang layak untuk domain design | Ya | Dinyatakan gerbang kelengkapan requirement revision `1.0` |
| Bounded context dapat dipertahankan | Ya | Dua context, batasnya beralasan, dan ketegangan dengan `CTX-MST` dinyatakan terbuka |
| Ownership yang material sudah terselesaikan | Ya | 14 konsep milik sendiri, 10 konsep dipakai ulang tanpa duplikasi, 8 konsep ditolak beserta alasannya |
| Lifecycle dan invariant penting sudah terwakili | Ya | Sepuluh invariant, dan yang tidak dapat dijaga aggregate dinyatakan terang-terangan |
| Keputusan bisnis pemblokir sudah terselesaikan | Ya | Untuk sembilan slice ini. Yang belum selesai berada di slice yang memang dihentikan |
| Konsekuensi billing yang material sudah eksplisit | Ya | Bagian K, termasuk risiko rekonstruksi mundur |
| Konsekuensi keselamatan klinis sudah eksplisit | Ya | Bagian L, termasuk pernyataan terus terang pada L.3 |

### N.3 Slice yang harus berhenti

`INP-S05`, `INP-S06`, `INP-S09`, `INP-S10`, `INP-S11`, `INP-S15`, serah terima klinis antar shift,
dan dua cara pulang meninggal serta kabur. Seluruhnya menunggu `DEC-INP-001` sampai `DEC-INP-007`.

Tidak satu pun dari kedelapan butir itu boleh diteruskan ke `design-business-module`.

> **Diperbarui 2 September 2026.** Dua butir pertama sudah berubah keadaan. `INP-S06` dan **bagian
> dokter** pada `INP-S05` sudah dirancang pada Bagian Kedua dan boleh diteruskan ke
> `design-business-module`, karena `DEC-INP-001` dan `DEC-INP-008` sudah `CLOSED`. **Bagian
> keperawatan** pada `INP-S05` dan enam butir sisanya tetap berhenti.

### N.4 Handoff ke `design-business-module`

| Field | Nilai |
| --- | --- |
| Modul dan kemampuan | `InPatientManagement` / Rawat Inap, prefix `Inp`, sembilan slice pada N.2 |
| Kesiapan requirement | `PARTIALLY_READY`, revision `1.0` |
| Kesiapan arsitektur | `DOMAIN_ARCHITECTURE_PARTIAL`, revision `0.1`, status `draft` |
| Klasifikasi bukti | Keputusan bisnis dari pemegang sementara; bukti implementasi dari capability map revision `1.2`; baseline `REFERENCE_ONLY` |
| Decision ID yang belum selesai | `DEC-INP-001` s.d. `DEC-INP-007` |
| Gap arsitektur | `ARCH-GAP-001` s.d. `ARCH-GAP-007` |
| Source SHA | Backend `5afb54b`; frontend `dec4fdeff` |
| ID observasi baseline yang dipakai | `ID-INP-INT-001` s.d. `005`, `ID-INP-REG-001`, `ID-INP-CAP-001` s.d. `020` |
| Jejak requirement ke domain | Setiap konsep pada bagian D menyebut aturan bisnis asalnya; setiap slice pada A.1 menyebut kemampuan PRD asalnya |
| Keluaran hilir yang diharapkan | Arsitektur backend dan frontend, ERD per bounded context, kontrak API, state transition, validation, permission/audit, dan strategi test — hanya untuk sembilan slice pada N.2 |

### N.5 Yang tidak boleh diubah diam-diam oleh blueprint hilir

Blueprint final boleh mempertajam kontrak implementasi, tetapi **tidak boleh** mengubah hal
berikut tanpa kembali ke skill hulu:

1. Kepemilikan data pada bagian D. Terutama: pasien, dokter, pegawai, tempat tidur, kamar, unit
   layanan, kelas pasien, dan kunjungan tetap milik modul lain.
2. Kedudukan kolom status tempat tidur sebagai **salinan**, bukan sumber kebenaran.
3. Sepuluh invariant pada bagian E.
4. Bentuk berperiode pada `CON-INP-002`, `CON-INP-003`, dan `CON-INP-007`. Menggantinya dengan
   satu kolom yang ditimpa akan menghapus riwayat yang dibutuhkan resume, billing, dan
   interoperabilitas.
5. Kedudukan sesi koreksi sebagai konsep tersendiri, bukan status episode keenam.

### N.6 Peringatan penutup

Arsitektur ini berstatus `draft`. Tidak ada satu pun bagiannya yang sudah disetujui manusia.

Persetujuan pemilik yang berwenang tetap dibutuhkan, dan sampai hari ini pemilik itu belum
ditunjuk namanya — tercatat sebagai `RWI-OQ-023` sejak Scope Pass 20 Agustus 2026.

---
---

# Bagian Kedua — Amendment Dokter Rawat Inap

> **Cara membaca bagian ini.** Bagian A sampai N di atas adalah arsitektur pass pertama, 21 Agustus
> 2026, yang **sengaja berhenti** pada slice dokumentasi klinis karena `DEC-INP-001` waktu itu masih
> terbuka. Bagian O sampai AB di bawah adalah **amendment** yang melanjutkan pekerjaan itu setelah
> keputusannya turun. Bagian lama tidak dihapus supaya alasan keputusannya tetap dapat ditelusuri;
> bila keduanya berbeda pada scope dokter, **bagian kedua yang berlaku**.

## O. Identitas amendment

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001`, sub-modul `dokter-rawat-inap` |
| Architecture revision | `0.2` — amendment atas revision `0.1` |
| Architecture status | `draft` — belum disetujui manusia |
| Tanggal | 2 September 2026 (`Asia/Jakarta`) |
| **Kesiapan arsitektur scope ini** | **`DOMAIN_ARCHITECTURE_READY`** — rinciannya pada bagian AB |
| Kesiapan arsitektur dokumen keseluruhan | Tetap `DOMAIN_ARCHITECTURE_PARTIAL`, karena slice `INP-S09` s.d. `INP-S11` dan `INP-S15` belum dirancang dan tidak dinilai ulang di sini |
| Kesiapan requirement masukan | **`READY_FOR_DOMAIN_DESIGN`** untuk ketujuh capability, dari [`02-requirement-completeness-gate.md`](./02-requirement-completeness-gate.md) revision `1.3`, SHA-256 `883ed59b48bc10cb2ee9b2e09900c470a63bad9d06a339613aa871d308a70ade` |
| Bukti bisnis | [`00-interview-decisions.md`](../00-interview-decisions.md) revision `8`, SHA-256 `065b5cd5d96576560d9e2ee3a4c97be6f2925cee7cda16dc6d99f8dfa441117f` |
| Bukti keadaan saat ini | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.3` bagian 15, SHA-256 `0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03` |
| Backend snapshot | `93b3227c431401d8f586dec4e1fb25fbf41766e3`, branch `MHamzah` — **dibaca ulang saat amendment ini ditulis dan masih sama persis** dengan snapshot impact scan |
| Frontend snapshot | `863f24b0d1617069310c04e5770b47fd1b518b5b`, branch `HamzahV2` — **dibaca ulang dan masih sama persis** |
| Baseline rujukan | `indonesia-hospital-domain-reference`, seluruhnya `REFERENCE_ONLY`. Tidak ada observasi baru yang dinaikkan menjadi requirement pada amendment ini |
| Prefix modul | Tetap `Inp`. **Amendment ini tidak menambah satu pun konsep berprefix `Inp` untuk dokumentasi klinis** |
| Batas tulis | Hanya dokumen ini. Tidak ada schema, migration, endpoint, UI, task, atau source aplikasi yang dibuat atau diubah |

Karena kedua SHA source terbukti belum bergeser, seluruh fakta implementasi pada bagian 15 capability
map masih berlaku apa adanya untuk amendment ini. Ini bukan asumsi: SHA-nya dibaca ulang, bukan
disalin dari dokumen sebelumnya.

### O.1 Scope yang dirancang

Tujuh kemampuan milik sub-modul `dokter-rawat-inap` sesuai `RWI-DEC-083`, seluruhnya dinyatakan
`READY_FOR_DOMAIN_DESIGN` oleh gerbang requirement revision `1.3` bagian 12.3.

| Kemampuan | Nama pada `PRD-RWI-FINAL-001` | Slice | Status kemampuan pada source | Bukti audit |
| --- | --- | --- | --- | --- |
| `CAP-015` | Supporting Services — laboratorium dan radiologi | `INP-S05` bagian dokter | `Extend` | `DOK-TRC-CAP015` |
| `CAP-020` | Clinical Documentation — SOAP | `INP-S05` | `Repair` | `DOK-TRC-CAP020` |
| `CAP-021` | Clinical Documentation — CPPT | `INP-S05` | `Extend` | `DOK-TRC-CAP021` |
| `CAP-022` | Medical Assessment — kajian medis awal | `INP-S05` | `Reuse with adapter` | `DOK-TRC-CAP022` |
| `CAP-023` | Medication Management — resep dan obat pulang | `INP-S06` | `Extend` | `DOK-TRC-CAP023` |
| `CAP-024` | Physician Procedures — tindakan dokter | `INP-S05` | `Extend` | `DOK-TRC-CAP024` |
| `CAP-025` | Physician Visit — pencatatan visite | `INP-S05` | `Missing` | `DOK-TRC-CAP025` |

### O.2 Yang tetap tidak dirancang, dan alasannya

| Butir | Alasan berhenti | Diarahkan ke |
| --- | --- | --- |
| Bagian keperawatan pada `INP-S05` — pengkajian keperawatan, catatan perawat, daftar pantau perawat | Bukan scope sub-modul ini. Milik sub-modul `keperawatan` sesuai `RWI-DEC-083`, dan belum dinilai ulang oleh gerbang requirement | Amendment tersendiri untuk `keperawatan` |
| `INP-S09` serah terima IGD, `INP-S10` persetujuan umum, `INP-S11` isolasi dan jenis kelamin, `INP-S15` interoperabilitas | Tetap menunggu `DEC-INP-002` s.d. `DEC-INP-005` | `grill-me` |
| Nilai angka batas waktu kajian medis dan verifikasi CPPT | `RWI-RULE-021` belum `approved` dan pemilik klinisnya belum ditunjuk. Arsitektur hanya menyediakan tempat parameternya, tidak mengarang angkanya | `grill-me`, setelah pemilik klinis ditunjuk |
| Pencatatan visite oleh petugas administrasi atas nama dokter | `RWI-RULE-017` current menyatakan kemampuan itu **tidak tersedia** sampai ada kebijakan eksplisit | `grill-me` |
| Aturan tarif, pemicu posting, dan kebijakan penjamin untuk visite | Milik pemilik Billing. `RWI-DEC-085` justru **memisahkan** agregasi tarif dari riwayat klinis | Pemilik `BillingManagement` |
| Bentuk layar, komponen, navigasi, dan endpoint ruang kerja dokter | Bukan pekerjaan arsitektur domain | `design-business-module` |

### O.3 Keputusan yang mengikat amendment ini

| Decision ID | Yang dikunci | Akibat langsung pada arsitektur |
| --- | --- | --- |
| `RWI-DEC-080` | Dokumentasi klinis dokter resmi masuk scope; modul menjadi 28 kemampuan | Tujuh kemampuan ini wajib punya model domain, bukan lagi ditunda |
| `RWI-DEC-081` | Seluruh tabel dokumentasi klinis dimiliki `ClinicalManagement`; Rawat Inap nol tabel | Tidak ada satu pun konsep dokumentasi klinis yang dimiliki `CTX-INP-CARE`. Bagian R menegakkan ini |
| `RWI-DEC-062` | Pemilik `ClinicalManagement`, `PharmacyManagement`, dan `MasterData` menyetujui perubahan lintas modul | Ownership tidak lagi ambigu, sehingga amendment ini tidak perlu mengarang siapa pemiliknya |
| `RWI-DEC-038` dan `RWI-DEC-070` | Pelonggaran antrean, satu konsultasi per kunjungan, dan satu resep aktif per konsultasi — **hanya** untuk kunjungan `Inpatient` dan `Emergency` | Menjadi `INV-DOK-04` dan `INV-DOK-05` pada bagian S |
| `RWI-DEC-083` | Pemetaan tujuh kemampuan ke sub-modul ini | Batas scope amendment |
| `RWI-DEC-084` | Physician Visit adalah **event klinis eksplisit**; tautan SOAP/CPPT opsional; pengiriman ganda dengan kunci yang sama tidak membuat event kedua | Melahirkan `AGG-CLI-VISIT` pada bagian S.1 |
| `RWI-DEC-085` | Setiap visite nyata dihitung satu. Dua visite pada hari yang sama tetap dua. Agregasi Billing terpisah dan tidak boleh mengubah riwayat klinis | Melahirkan `INV-DOK-06` s.d. `INV-DOK-09` dan seluruh bagian Y |
| `RWI-RULE-026` | Rawat Inap tidak membuat tabel klinis tandingan dan tidak membuat antrean semu | Batas keras seluruh bagian R |
| `RWI-RULE-024` dan `RWI-DEC-046` | Obat pulang adalah **jenis resep** pada tabel resep milik Farmasi, bukan daftar terpisah milik Rawat Inap | Bagian S.5 |
| `RWI-RULE-030` | DPJP berperiode; kewenangan per pasien adalah tanggung jawab domain | Bagian V |

### O.4 Yang digantikan oleh amendment ini

| Bagian lama | Keadaannya sekarang |
| --- | --- |
| A.2 baris `INP-S05` dan `INP-S06` | **Digantikan.** Keduanya tidak lagi "sengaja tidak dirancang" untuk bagian dokter |
| C.2 baris `CTX-CLI` dan `CTX-PHM` yang berbunyi "Belum ditentukan" | **Digantikan** oleh bagian Q |
| N.3 yang mendaftar `INP-S05` dan `INP-S06` sebagai slice yang harus berhenti | **Digantikan** untuk bagian dokter. Bagian keperawatan pada `INP-S05` tetap berhenti |
| Seluruh pernyataan dokumen ini yang mengasumsikan modul Radiologi belum ada | **Digantikan.** Radiologi terbukti ada pada `BE@93b3227` |

### O.5 Peta bagian terhadap kontrak keluaran

| Kontrak | Pass pertama | Amendment ini |
| --- | --- | --- |
| Identitas arsitektur | A | O |
| Ubiquitous language | B | P |
| Peta bounded context | C | Q |
| Katalog konsep domain | D | R |
| Model aggregate | E | S |
| Model relasi | F | T |
| Lifecycle dan status | G | U |
| Authorization | H | V |
| Audit dan histori | I | W |
| Integrasi | J | X |
| Dampak billing | K | Y |
| Dampak keselamatan klinis | L | Z |
| Gap arsitektur | M | AA |
| Kesiapan arsitektur | N | AB |

---

## P. Ubiquitous language tambahan

Istilah pada bagian B tetap berlaku. Berikut istilah yang dipakai khusus pada scope dokter.

| Istilah | Makna bisnis yang dipakai di sini | Catatan |
| --- | --- | --- |
| **Visite dokter** | Kunjungan nyata seorang dokter kepada pasien rawat inap, yang dicatat sebagai **satu kejadian tersendiri** | Sejak `RWI-DEC-084` ia bukan lagi akibat dari menulis catatan. Dokter yang datang tanpa menulis apa pun tetap dapat mencatat visitenya |
| **Event visite** | Baris kejadian yang menyatakan "dokter ini benar-benar mendatangi pasien ini pada waktu ini" | Satu event sama dengan satu kunjungan nyata |
| **Kunci permintaan** (*request id* atau *idempotency key*) | Penanda unik satu permintaan simpan yang dikirim aplikasi | Dipakai supaya tombol Simpan yang tertekan dua kali tidak melahirkan dua visite |
| **Catatan dokter** | Satu catatan pemeriksaan dokter berisi SOAP: keluhan pasien, temuan pemeriksaan, penilaian, dan rencana | Pada sistem hari ini catatan dokter dan konsultasi adalah **objek yang sama**, yaitu `TrxDoctorConsultation`, dengan SOAP sebagai isinya |
| **CPPT** | Catatan Perkembangan Pasien Terintegrasi — catatan lintas profesi pada satu lembar yang sama | Dokter, perawat, gizi, dan farmasi menulis di sana. Pemilik kontraknya tetap sub-modul dokter |
| **Verifikasi CPPT** | Pernyataan DPJP bahwa ia sudah membaca dan menyetujui catatan yang ditulis profesi lain | Belum ada pada sistem hari ini; menjadi tambahan pada `CAP-021` |
| **Kajian medis awal** | Pemeriksaan menyeluruh pertama oleh dokter saat pasien mulai dirawat | Berbeda dari pengkajian keperawatan dan berbeda dari catatan SOAP harian |
| **Resep harian** | Resep yang ditulis selama pasien dirawat | Boleh lebih dari satu selama episode berlangsung |
| **Obat pulang** | Resep yang ditulis untuk dibawa pasien pulang | Bukan kemampuan baru; ia **jenis** resep, sesuai `RWI-RULE-024` |
| **Tindakan dokter** | Prosedur medis yang direncanakan atau dikerjakan dokter kepada pasien | Menjadi dasar fakta klinis yang dikirim ke Billing |
| **Penunjang** | Pemeriksaan laboratorium dan radiologi yang dipesan dokter | Pesanannya milik modul Lab dan Radiologi, bukan milik Rawat Inap |
| **Hasil final terverifikasi** | Hasil penunjang yang sudah disahkan petugas berwenang di modul asalnya | Hanya hasil seperti inilah yang boleh ditampilkan sebagai dasar keputusan klinis |
| **Konteks klinis episode** | Jawaban atas pertanyaan "dokumen ini milik pasien mana, kunjungan mana, episode mana, dan apakah dokter ini berwenang" | Dihitung, tidak disimpan. Lihat `CON-INP-015` |
| **Antrean semu** | Baris antrean poliklinik yang dibuat hanya supaya dokumen klinis pasien menginap dapat disimpan | **Dilarang** oleh `RWI-RULE-026` aturan 2 |
| **Fakta klinis** (*clinical milestone fact*) | Pernyataan satu arah dari modul klinis ke Billing bahwa suatu peristiwa klinis benar terjadi | Tidak memuat nominal dan tidak memuat keputusan finansial |

**Dua kata yang sengaja dibedakan.** Dalam percakapan sehari-hari "dokter visite" dan "dokter
menulis SOAP" sering dipakai bergantian. Di dalam arsitektur ini keduanya **dua hal yang berbeda**:
visite adalah kedatangan, SOAP adalah catatan. Satu bisa ada tanpa yang lain. Perbedaan inilah inti
`RWI-DEC-084`, dan menyatukannya kembali berarti membatalkan keputusan pemilik.

---

## Q. Peta bounded context setelah amendment

### Q.1 Context yang sebelumnya "belum ditentukan", kini ditentukan

| ID | Context | Milik modul | Hubungan | Yang dipakai Rawat Inap |
| --- | --- | --- | --- | --- |
| `CTX-CLI` | Dokumentasi Klinis | `ClinicalManagement` | **Pelanggan–pemasok.** Rawat Inap adalah pelanggan yang menyatakan kebutuhan; `ClinicalManagement` adalah pemasok yang memiliki dan mengubah modelnya sendiri | Catatan dokter/SOAP, CPPT, kajian medis, tindakan, dan event visite |
| `CTX-PHM` | Farmasi | `PharmacyManagement` | **Pelanggan–pemasok** | Resep harian, obat pulang, dan status pemenuhannya |
| `CTX-LAB` | Laboratorium | `LaboratoryManagement` | **Pelanggan–pemasok** | Pesanan pemeriksaan dan hasil final terverifikasi |
| `CTX-RAD` | Radiologi | `RadiologyManagement` | **Pelanggan–pemasok**. Context ini **baru diakui** pada amendment ini | Pesanan pemeriksaan, studi, dan hasil final terverifikasi |
| `CTX-MRC` | Integritas Dokumen Rekam Medis | `MedicalRecordManagement` | **Pelanggan–pemasok**. Context ini **baru diakui** pada amendment ini | Penandatanganan dokumen, penguncian, addendum koreksi, dan pendelegasian penulis |
| `CTX-BIL` | Billing | `BillingManagement` | **Hilir satu arah.** Menerima fakta klinis, tidak pernah mengubahnya | Penerimaan fakta tindakan dan — bila kelak diputuskan pemiliknya — agregasi tarif visite |

**Kenapa hubungannya pelanggan–pemasok, bukan bermitra seperti `CTX-MST`.** Pada `CTX-MST` Rawat
Inap benar-benar **menulis** ke kolom milik context lain, yaitu salinan status tempat tidur
(`INT-INP-03`). Pada keenam context di atas, Rawat Inap **tidak menulis apa pun** ke dalam model
mereka. Yang dilakukan Rawat Inap hanya dua: menyediakan konteks episode yang dapat diperiksa, dan
membaca kembali status atau hasil yang mereka sahkan. Perubahan model yang dibutuhkan — misalnya
resolusi episode pada konsultasi — dikerjakan **oleh pemiliknya sendiri**, atas persetujuan yang
sudah diberikan lewat `RWI-DEC-062`.

Perbedaan ini bukan soal istilah. Ia menentukan siapa yang menulis kode di modul mana, dan siapa
yang bertanggung jawab bila datanya salah.

### Q.2 Kenapa Rawat Inap tidak memiliki satu pun konsep dokumentasi klinis

`RWI-DEC-081` sudah menjawabnya sebagai keputusan. Arsitektur menambahkan alasan domainnya, supaya
keputusan itu tidak terasa sewenang-wenang ketika kelak ada yang mengusulkan sebaliknya.

Satu pasien hanya boleh punya **satu tempat rekam medis**. Bila Rawat Inap membuat tabel catatan
dokter sendiri, riwayat Tn. Budi akan terpecah: catatan poliklinik di satu tempat, catatan selama
menginap di tempat lain. Dokter yang membuka riwayat pasien harus membuka dua layar lalu
menggabungkannya sendiri di kepala. Itulah bentuk paling nyata dari bahaya duplikasi — bukan soal
tabel yang boros, melainkan soal dokter yang mengambil keputusan dari separuh riwayat.

| Yang dimiliki Rawat Inap pada scope dokter | Yang **tidak** dimiliki Rawat Inap |
| --- | --- |
| Makna "episode ini sedang berjalan, pasiennya ini, DPJP-nya ini" | Isi catatan dokter, CPPT, kajian medis, resep, tindakan, pesanan penunjang, dan event visite |
| Kewenangan menyatakan bahwa sebuah episode layak menjadi konteks penulisan dokumen | Lifecycle penandatanganan, penguncian, dan addendum dokumen |
| Ruang kerja yang mengumpulkan seluruhnya menjadi satu layar dokter | Sumber kebenaran satu pun data klinis di atas |

### Q.3 Konteks Klinis Episode sebagai kontrak, bukan tabel

Inti teknis seluruh amendment ini adalah satu pertanyaan sederhana: **bagaimana sebuah catatan
dokter tahu bahwa ia milik episode rawat inap yang benar, tanpa membuat antrean semu?**

Hari ini jawabannya belum ada. Bukti `DOK-TRC-INT-01` menunjukkan `DoctorConsultationController`
dan `PatientAssessmentController` hanya mengenali kunjungan yang punya baris IGD (`EmgVisit`);
tidak ada satu pun cabang yang mengenali episode rawat inap. Inilah yang disebut *shared inpatient
clinical context resolver* pada `PRD-RWI-FINAL-001` bagian 30.3, dan berstatus `Missing`.

Arsitektur menjawabnya dengan satu konsep turunan, bukan tabel baru: **Konteks Klinis Episode**
(`CON-INP-015`). Isinya adalah jawaban atas empat pertanyaan yang dihitung saat itu juga:

1. pasien mana — `PatientId`;
2. kunjungan mana — `EncounterId`;
3. episode mana dan apakah masih hidup — `InpatientEpisodeId` beserta statusnya;
4. apakah dokter yang meminta berwenang atas pasien ini — DPJP aktif atau kewenangan lain yang sah.

Konteks ini **dihitung dari data yang sudah ada** — sama seperti Census dan Lama Dirawat pada
bagian D — sehingga tidak menambah satu pun tabel dan tidak dapat basi. Pemilik maknanya adalah
`CTX-INP-CARE`, karena hanya Rawat Inap yang tahu apa artinya "episode masih berjalan". Yang
memakainya adalah `CTX-CLI`, `CTX-PHM`, `CTX-LAB`, dan `CTX-RAD`.

Contoh konkret perbedaannya:

> **Hari ini.** dr. Andi membuka pasien Tn. Budi yang sedang dirawat di Melati 3B, lalu menekan
> Simpan pada catatan SOAP. Permintaan itu ditolak karena tidak membawa nomor antrean, dan Tn. Budi
> memang tidak pernah mengambil nomor antrean — ia sedang berbaring di kamar.
>
> **Setelah aturan ini berlaku.** Permintaan yang sama membawa `EncounterId` milik kunjungan rawat
> inap Tn. Budi. Konteks Klinis Episode menjawab: pasien Tn. Budi, episode `EP-2026-0912`, status
> `Admitted`, DPJP aktif dr. Andi. Catatan tersimpan tanpa satu pun baris antrean dibuat, dan
> laporan antrean poliklinik hari itu tetap bersih.

---

## R. Katalog konsep domain

Klasifikasi tetap sama dengan bagian D: `AGGREGATE_ROOT`, `ENTITY`, `VALUE_OBJECT`,
`REFERENCE_DATA`, `DOMAIN_EVENT`, `EXTERNAL_CONTRACT`. Kepemilikan tetap `Existing`, `Extend`,
`New`, atau `Adapter/View`.

### R.1 Konsep baru yang dimiliki Rawat Inap

Hanya satu, dan ia tidak berbentuk tabel.

| ID | Nama bisnis | Klasifikasi | Ownership | Identitas | Peran dalam lifecycle | Invariant penting | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `CON-INP-015` | Konteks Klinis Episode | `VALUE_OBJECT` | `New` | — | **Dihitung, tidak disimpan.** Dibentuk setiap kali dokumen klinis rawat inap ditulis atau dibaca | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-13` | `RWI-RULE-026`, `PRD-RWI-FINAL-001` bagian 30.3, `DOK-TRC-INT-01` |

### R.2 Konsep milik `ClinicalManagement` yang dipakai dan diperluas

Tidak satu pun konsep berikut disalin ke Rawat Inap. Seluruhnya tetap milik `CTX-CLI`.

| ID | Nama bisnis | Klasifikasi | Ownership | Identitas | Peran dalam lifecycle | Invariant penting | Bukti keadaan saat ini |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `CON-EXT-011` | Catatan Dokter berisi SOAP | `AGGREGATE_ROOT` | `Extend` | Nomor konsultasi | `Draft` lalu `Completed`, dapat `Cancelled`. Koreksi setelah final lewat addendum | `INV-DOK-01` s.d. `INV-DOK-05`, `INV-DOK-10` | `BE@93b3227 Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs#Subjective` — SOAP adalah **isi** konsultasi, bukan objek terpisah |
| `CON-EXT-012` | CPPT — Catatan Perkembangan Pasien Terintegrasi | `AGGREGATE_ROOT` | `Extend` | Nomor catatan | Ditulis lintas profesi, lalu **diverifikasi DPJP**. Verifikasi adalah tambahan baru | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-11` | `.../Models/TrxPatientIntegratedProgressNote.cs` memiliki `EncounterId`, `ProfessionType`, `ProviderUserId`; **tidak** memiliki kolom penanda verifikasi mana pun |
| `CON-EXT-013` | Kajian Medis Awal | `AGGREGATE_ROOT` | `Extend` | Nomor kajian | `Draft` lalu `InProgress` lalu `Completed`, dapat `Cancelled`. Satu kajian medis berlaku untuk satu episode | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03` | `.../Models/TrxPatientAssessment.cs#AssessmentStatus`; isian yang ada hari ini bercorak keperawatan — kesadaran, risiko jatuh, status gizi, kemandirian |
| `CON-EXT-014` | Tindakan Dokter | `AGGREGATE_ROOT` | `Extend` | Nomor tindakan | `Planned` lalu dikerjakan (`IsExecuted`, `PerformedAt`), dapat dibatalkan | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-09` | `.../Models/TrxPatientProcedure.cs#ProcedureStatus`, `#IsExecuted`, `#BillingItemId` |
| `CON-EXT-015` | **Event Visite Dokter** | `AGGREGATE_ROOT` | **`New`** | Nomor event; ditambah **kunci permintaan** sebagai penjaga keunikan | `Recorded` lalu dapat `Cancelled`. Tidak ada status antara dan tidak ada penghapusan | `INV-DOK-06`, `INV-DOK-07`, `INV-DOK-08`, `INV-DOK-09` | Pencarian `PhysicianVisit` pada `Areas` dan `Migrations` di `BE@93b3227` menghasilkan **nol** kecocokan — dibaca ulang saat amendment ini ditulis |
| `CON-EXT-016` | Fakta Klinis untuk Billing | `DOMAIN_EVENT` | `Existing` | `SourceContext` ditambah `SourceAggregateId` ditambah nomor versi | Diterbitkan satu arah setelah peristiwa klinis benar terjadi | `INV-DOK-09` | `.../DTOs/ClinicalMilestoneFactDtos.cs#ClinicalMilestoneFactRequest`; `.../Services/ClinicalMilestoneFactProducer.cs#BuildIdempotencyKey` |

**Kenapa `CON-EXT-015` berstatus `New` padahal pemiliknya `ClinicalManagement`.** `New` di sini
berarti **konsep domainnya belum pernah ada**, bukan berarti Rawat Inap yang membuatnya. Event
visite memperkenalkan identitas sendiri, lifecycle sendiri, aturan keunikan sendiri, dan tanggung
jawab audit sendiri — empat syarat `New` pada kontrak arsitektur. Pemilik dan tempat tinggalnya
tetap `ClinicalManagement`, sesuai `RWI-DEC-081`. Konsep bernama `InpPhysicianVisit` **dilarang**.

**Kenapa `CON-EXT-011` tidak dipecah menjadi "Konsultasi" dan "SOAP" yang terpisah.** Karena pada
sistem yang berjalan hari ini keduanya satu objek: kolom `Subjective`, `Objective`, dan seterusnya
menempel langsung pada `TrxDoctorConsultation`. Memecahnya di tingkat domain akan menciptakan
perbedaan yang tidak dituntut satu pun aturan bisnis. Konsekuensinya justru penting dan sering
disalahpahami: **batas "satu konsultasi per kunjungan" itulah yang membuat dokter hanya bisa
menulis satu catatan SOAP untuk seluruh masa perawatan.** Melonggarkannya (`INT-DOK-02`) bukan soal
administrasi konsultasi, melainkan satu-satunya cara agar catatan harian dokter dapat ditulis
setiap hari.

### R.3 Konsep milik context lain yang dipakai apa adanya

| ID | Nama bisnis | Klasifikasi | Ownership | Pemilik | Dipakai untuk apa |
| --- | --- | --- | --- | --- | --- |
| `CON-EXT-017` | Resep | `AGGREGATE_ROOT` | `Extend` | `CTX-PHM` | Resep harian dan obat pulang. Hari ini setiap resep **wajib** menempel pada satu konsultasi — `TrxPrescription.ConsultationId` tidak boleh kosong |
| `CON-EXT-018` | Jenis Resep — penanda obat pulang | `VALUE_OBJECT` | `Extend` | `CTX-PHM` | Penanda "resep ini obat pulang", disimpan di sisi Farmasi sesuai `RWI-DEC-046` supaya terlihat di layar petugas farmasi |
| `CON-EXT-019` | Pesanan Laboratorium | `AGGREGATE_ROOT` | `Extend` | `CTX-LAB` | Pemesanan pemeriksaan; berjangkar pada `EncounterId` |
| `CON-EXT-020` | Pesanan Radiologi dan Studi | `AGGREGATE_ROOT` | `Extend` | `CTX-RAD` | Pemesanan pencitraan; berjangkar pada `EncounterId` dan modalitas |
| `CON-EXT-021` | Integritas Dokumen Klinis | `ENTITY` | `Existing` | `CTX-MRC` | Penandatanganan, penguncian, dan pembatalan dokumen klinis. Berjangkar pada pasangan jenis dokumen dan Id dokumen |
| `CON-EXT-022` | Addendum Koreksi | `ENTITY` | `Existing` | `CTX-MRC` | Koreksi dokumen yang sudah final tanpa menimpa isi aslinya; menyimpan alasan koreksi dan penanda penulis pengganti |
| `CON-EXT-023` | Pendelegasian Penulis Dokumen | `ENTITY` | `Existing` | `CTX-MRC` | Menjelaskan sah tidaknya dokumen yang ditandatangani orang lain, beserta masa berlakunya |

Konsep `CON-EXT-001` s.d. `CON-EXT-010` pada bagian D tetap berlaku tanpa perubahan.

### R.4 Konsep yang dipertimbangkan lalu ditolak

Dicatat supaya tidak diusulkan ulang.

| Konsep yang ditolak | Kenapa ditolak |
| --- | --- |
| `InpClinicalNote`, `InpPhysicianVisit`, `InpPrescription`, atau tabel dokumentasi `Inp*` lainnya | Langsung melanggar `RWI-DEC-081` dan `RWI-RULE-026` aturan 1. Akibatnya riwayat pasien terpecah dua |
| Entity "Antrean Rawat Inap" supaya dokumen klinis punya jangkar | Persis bentuk **antrean semu** yang dilarang `RWI-RULE-026` aturan 2. Ia juga akan mencemari laporan antrean poliklinik |
| Entity "Ruang Kerja Dokter" | Ruang kerja adalah layar, bukan konsep bisnis. Menurunkan entity dari layar dilarang kontrak arsitektur |
| Entity "Visite" sebagai turunan otomatis catatan SOAP | Sudah `superseded`. `RWI-DEC-084` menetapkan visite sebagai event eksplisit, dan `RWI-AC-151` mengujinya |
| Entity per status dokumen — "Catatan Draft", "Catatan Final" | Status adalah nilai, bukan objek. Sama dengan alasan penolakan pada bagian D.3 |
| Salinan hasil laboratorium dan radiologi di sisi Rawat Inap supaya layar dokter terasa cepat | Menciptakan sumber kebenaran kedua untuk hasil klinis. Hasil yang basi di layar dokter adalah risiko keselamatan, bukan sekadar masalah tampilan |
| Konsep "Jumlah Visite Harian" yang disimpan | Hitungan diturunkan dari event, sama seperti Lama Dirawat pada `CON-INP-012`. Menyimpannya membuat angka klinis dan angka tagihan berpotensi berselisih — persis yang dilarang `RWI-DEC-085` |
| Menumpangkan event visite pada mesin integritas dokumen sebagai jenis dokumen baru | Visite adalah **kejadian**, bukan dokumen bertanda tangan. Ia tidak punya isi naratif yang perlu dikoreksi lewat addendum. Lihat S.1 |

---

## S. Model aggregate

### S.0 Invariant yang berlaku pada seluruh scope dokter

Tiga belas invariant berikut adalah janji yang harus selalu benar. Nomornya dipakai kembali di
seluruh bagian berikutnya.

| ID | Invariant | Kenapa ada | Dasar |
| --- | --- | --- | --- |
| `INV-DOK-01` | Setiap dokumen klinis rawat inap wajib terikat pada **tepat satu** episode yang terbukti, lewat kunjungan yang menjadi jangkarnya | Dokumen tanpa episode tidak dapat ditelusuri, tidak dapat ditagihkan, dan tidak dapat diaudit | `RWI-RULE-005`, `INV-INP-04`, `PRD-RWI-FINAL-001` bagian 30.3 |
| `INV-DOK-02` | Pasien pada dokumen, pasien pada kunjungan, dan pasien pada episode wajib **orang yang sama** | Ini penjaga langsung terhadap salah pasien | Gerbang requirement dimensi 17, butir "mismatch A/B" |
| `INV-DOK-03` | Dokumen klinis **baru** tidak boleh lahir pada episode berstatus `Closed` atau `Cancelled` | Episode yang sudah ditutup adalah riwayat, bukan tempat menulis | `RWI-RULE-020`, `INV-INP-06` |
| `INV-DOK-04` | Satu episode boleh memiliki **banyak** catatan dokter, **banyak** resep, dan **banyak** tindakan sepanjang episode berjalan | Tanpa ini dokter hanya dapat menulis satu catatan dan satu resep untuk seluruh masa perawatan | `RWI-DEC-038`, `RWI-DEC-070`, `RWI-RULE-026` aturan 4 dan 5 |
| `INV-DOK-05` | Pelonggaran pada `INV-DOK-04` hanya berlaku untuk kunjungan bertipe `Inpatient` dan `Emergency`. Perilaku rawat jalan dan medical check-up **tidak berubah sedikit pun** | Perubahan ini menyentuh alur yang sudah melayani pasien | `RWI-RULE-026` aturan 6, `RWI-AC-143` |
| `INV-DOK-06` | Satu **kunci permintaan** menghasilkan **paling banyak satu** event visite | Tombol Simpan yang tertekan dua kali tidak boleh menjadi dua kunjungan dokter | `RWI-DEC-084`, `RWI-AC-152` |
| `INV-DOK-07` | Event visite **tidak diturunkan** dari SOAP atau CPPT. Catatan tanpa event tidak menambah hitungan visite, dan event tanpa catatan tetap sah | Inti keputusan `RWI-DEC-084` | `RWI-AC-150`, `RWI-AC-151` |
| `INV-DOK-08` | Event visite yang dibatalkan **tetap tersimpan** dan tidak ikut dihitung. Tidak ada penghapusan keras dan tidak ada penimpaan diam-diam | Riwayat klinis harus dapat dipertanggungjawabkan | Gerbang requirement bagian 12.2 butir 6 |
| `INV-DOK-09` | Billing boleh **membaca dan mengagregasikan** peristiwa klinis, tetapi tidak boleh mengubah, menggabungkan, atau menghapus satu pun peristiwa klinis | Angka tagihan tidak boleh menulis ulang riwayat medis | `RWI-DEC-085`, `RWI-AC-156` |
| `INV-DOK-10` | Dokumen yang sudah final tidak diubah di tempat. Koreksinya lewat addendum yang menyimpan alasan, penulis, dan waktu | Rekam medis tidak boleh berubah tanpa jejak | `CON-EXT-021`, `CON-EXT-022`, `RWI-DEC-051` |
| `INV-DOK-11` | Verifikasi CPPT hanya sah bila dilakukan **DPJP yang aktif pada episode itu saat verifikasi dilakukan** | Verifikasi oleh dokter yang bukan penanggung jawab tidak bermakna klinis | `RWI-RULE-030`, `RWI-RULE-021` |
| `INV-DOK-12` | Hasil penunjang yang ditampilkan sebagai dasar keputusan klinis hanya yang **final dan terverifikasi**, dan hanya milik episode yang sedang dibuka | Hasil sementara milik pasien lain adalah risiko keselamatan tertinggi pada scope ini | Gerbang requirement dimensi 12 dan 17 |
| `INV-DOK-13` | Setiap perintah klinis memeriksa **kewenangan atas pasien ini**, bukan sekadar kewenangan peran atas endpoint | Bukti `RWI-TF-014` menunjukkan mesin hak akses hanya mengenal peran terhadap endpoint | `RWI-RULE-030` aturan 6, bagian H.2 |

### S.1 `AGG-CLI-VISIT` — Event Visite Dokter

Aggregate ini **belum ada sama sekali** pada source. Ia lahir dari `RWI-DEC-084` dan `RWI-DEC-085`.

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-015` Event Visite Dokter |
| Pemilik | `CTX-CLI`, yaitu `ClinicalManagement` |
| Isi batas | Event itu sendiri beserta tautan opsionalnya ke catatan dokter, CPPT, atau tindakan |
| Di luar batas | Episode, kunjungan, pasien, dokter, catatan klinis — seluruhnya dirujuk lewat identitas saja |
| Alasan batas ini | Satu event menyatakan satu fakta tunggal: dokter ini datang pada waktu ini. Tidak ada bagian lain yang harus berubah bersamanya dalam satu transaksi |

#### Isi minimum satu event

Diambil apa adanya dari `RWI-RULE-017` current, tanpa penambahan.

| Yang disimpan | Kenapa dibutuhkan |
| --- | --- |
| Episode dan kunjungan | Menjawab "visite ini milik perawatan yang mana" — `INV-DOK-01` |
| Pasien | Penjaga salah pasien — `INV-DOK-02` |
| Dokter yang melakukan visite | Subjek fakta yang dicatat |
| Peran atau konteks dokter saat itu — DPJP, konsulen, atau dokter jaga | Dua dokter berbeda peran pada hari yang sama bermakna berbeda secara klinis dan operasional |
| Waktu visite (waktu klinis) | Waktu **kedatangan**, bukan waktu penyimpanan. `RWI-AC-150` mengujinya |
| Pelaku pencatatan | Boleh berbeda dari dokter hanya bila kelak ada kebijakan pendelegasian. Hari ini keduanya sama |
| Kunci permintaan | Penjaga `INV-DOK-06` |
| Tautan opsional ke catatan dokter, CPPT, atau tindakan | Boleh kosong. **Bukan syarat** lahirnya event |
| Jejak audit: pembuat, waktu simpan, dan bila dibatalkan — pembatal, waktu, alasan | `INV-DOK-08` |

#### Invariant yang dijaga aggregate ini

| ID | Bagaimana dijaga di dalam aggregate |
| --- | --- |
| `INV-DOK-06` | Kunci permintaan diperlakukan sebagai identitas kedua. Permintaan kedua dengan kunci sama mengembalikan **event yang sama**, bukan event baru dan bukan pesan kesalahan |
| `INV-DOK-07` | Aggregate ini tidak punya satu pun jalur yang membuat dirinya lahir dari penyimpanan SOAP atau CPPT. Tautan hanya dapat ditambahkan **setelah** event ada |
| `INV-DOK-08` | Pembatalan hanya mengubah status menjadi `Cancelled` dan mengisi alasan. Baris tidak pernah dihapus |

#### Lifecycle

Status awal: `Recorded`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Jejak yang wajib tersimpan |
| --- | --- | --- | --- | --- | --- |
| — | Catat visite | `Recorded` | Dokter yang berwenang atas pasien itu | Episode `Admitted` atau `DischargePending`; pasien cocok; kunci permintaan terisi | Pelaku, waktu simpan, waktu klinis, kunci permintaan |
| `Recorded` | Tautkan dokumen | `Recorded` | Dokter pemilik event | Dokumen milik episode yang sama | Dokumen yang ditautkan, pelaku, waktu |
| `Recorded` | Batalkan karena salah catat | `Cancelled` | Dokter pemilik event, atau supervisor | **Alasan wajib** | Pembatal, waktu, alasan |
| `Cancelled` | — | — | — | Status terminal. Event yang dibatalkan tidak dapat dihidupkan kembali | — |

#### Kenapa koreksi berbentuk batal lalu catat ulang, bukan sunting

Sebuah event visite menyatakan fakta: "dr. Andi datang pukul 07:40". Bila waktunya salah ketik,
yang sesungguhnya terjadi bukan "fakta yang sama dengan angka berbeda", melainkan **fakta yang
berbeda**. Menyuntingnya di tempat akan membuat riwayat berubah tanpa ada yang tahu bahwa dulu ia
pernah berbunyi lain.

Karena itu koreksi dilakukan dengan dua langkah yang keduanya terlihat: event lama dibatalkan
beserta alasannya, lalu event baru dicatat dengan kunci permintaan baru dan menyebut event yang
digantikannya. Auditor tetap melihat keduanya; hitungan hanya menghitung yang tidak dibatalkan.

> **Contoh.** dr. Andi visite pukul 07:40 tetapi tanpa sadar mengisi 17:40. Ia membatalkan event itu
> dengan alasan "salah ketik jam", lalu mencatat event baru pukul 07:40. Riwayat Tn. Budi
> menampilkan dua baris: satu dibatalkan dengan alasannya, satu berlaku. Hitungan visite hari itu
> tetap **1**.

#### Perhitungan visite, dengan angka

Aturan `RWI-DEC-085`: setiap visite nyata yang dicatat sebagai event berbeda dihitung satu.

| Keadaan pada 12 September 2026 | Jumlah event tersimpan | Hitungan klinis/operasional | Dasar |
| --- | ---: | ---: | --- |
| dr. Andi visite pukul 07:40, lalu kembali pukul 16:10 karena kondisi Tn. Budi memburuk | 2 | **2** | `RWI-AC-154` |
| dr. Andi visite sekali, tombol Simpan tertekan dua kali dengan kunci permintaan sama | 1 | **1** | `RWI-AC-152`, `RWI-AC-155` |
| dr. Andi dan dr. Sinta masing-masing visite satu kali | 2 | **2** | `RWI-RULE-017` current |
| dr. Andi menulis SOAP pukul 16:10 tanpa mencatat event visite | 0 | **0** | `RWI-AC-151` |
| dr. Andi mencatat event pukul 07:40, SOAP-nya baru ditulis pukul 07:52 | 1 | **1**, dengan waktu 07:40 | `RWI-AC-150` |
| Salah catat, dibatalkan, lalu dicatat ulang | 2 baris, 1 di antaranya `Cancelled` | **1** | `INV-DOK-08` |

#### Domain event yang diterbitkan

| ID | Kejadian | Kapan | Siapa yang mungkin peduli |
| --- | --- | --- | --- |
| `EVT-DOK-10` | Visite dokter dicatat | Event berstatus `Recorded` | Riwayat klinis, laporan operasional, dan — bila kelak diputuskan pemiliknya — Billing |
| `EVT-DOK-11` | Visite dokter dibatalkan | Event berstatus `Cancelled` | Riwayat klinis, laporan, dan Billing bila fakta terkait pernah dikirim |

### S.2 `AGG-CLI-NOTE` — Catatan Dokter berisi SOAP

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-011` Catatan Dokter |
| Pemilik | `CTX-CLI` |
| Isi batas | Isi SOAP, status catatan, dan penanda waktu klinis |
| Di luar batas | Integritas dan addendum (milik `CTX-MRC`), resep, tindakan, dan event visite |
| Invariant yang dijaga | `INV-DOK-01` s.d. `INV-DOK-05`, `INV-DOK-10` |

Yang **sudah ada** dan dipakai ulang apa adanya: bentuk SOAP, status `Draft` dan `Completed`,
larangan mengubah SOAP pada catatan yang sudah `Completed` atau `Cancelled`, serta penyimpanan
otomatis SOAP.

Yang **wajib ditambahkan**:

| Tambahan | Alasan | Bukti kekurangannya hari ini |
| --- | --- | --- |
| Pengenalan konteks episode pada pembuatan catatan | Tanpa itu catatan pasien menginap tidak dapat disimpan sama sekali | `DOK-TRC-INT-01` |
| Perlindungan jalur tanpa antrean | Cabang tanpa antrean membaca objek antrean yang kosong lalu tetap menulis ke dalamnya | `DOK-TRC-DEF-01`, diperiksa ulang pada `BE@93b3227 DoctorConsultationController.cs` baris 258–265 dan 360–366 |
| Pelonggaran satu konsultasi per kunjungan, terbatas `Inpatient` dan `Emergency` | Agar catatan harian dapat ditulis setiap hari | `DOK-TRC-INT-02` |
| Penjaga episode aktif dan kewenangan dokter | `INV-DOK-03`, `INV-DOK-13` | `DOK-TRC-AUTH-01` |

> **Kenapa perlindungan jalur tanpa antrean disebut sebagai butir arsitektur, bukan sekadar bug.**
> Karena akibatnya menyentuh batas domain. Jalur tanpa antrean adalah **satu-satunya** jalur yang
> dipakai pasien rawat inap. Selama jalur itu meledak, keputusan `RWI-DEC-038` tidak pernah benar-
> benar berlaku, dan seluruh model dokumentasi dokter di atas kertas saja.

### S.3 `AGG-CLI-CPPT` — Catatan Perkembangan Pasien Terintegrasi

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-012` CPPT |
| Pemilik | `CTX-CLI` |
| Isi batas | Isi catatan, profesi penulis, waktu klinis, dan **keadaan verifikasi DPJP** |
| Di luar batas | Catatan dokter, integritas dokumen, dan daftar pantau kepatuhan |
| Invariant yang dijaga | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-11` |

CPPT ditulis lintas profesi — dokter, perawat, gizi, farmasi — pada lembar yang sama. Itu sifat
CPPT, bukan tabrakan kepemilikan: pemilik kontraknya tetap sub-modul dokter sesuai `RWI-DEC-083`.

Yang **wajib ditambahkan** adalah keadaan verifikasi, yang hari ini tidak ada satu kolom pun:

| Yang ditambahkan | Isinya |
| --- | --- |
| Keadaan verifikasi | Belum diverifikasi, atau sudah diverifikasi |
| Pelaku verifikasi | Dokter yang memverifikasi, wajib DPJP aktif saat itu — `INV-DOK-11` |
| Waktu verifikasi | Dipakai menghitung keterlambatan |
| Batas waktu verifikasi | **Parameter yang dapat diubah admin**, bukan angka yang ditanam di program |

**Sifat batas waktu ini tidak menahan apa pun.** `RWI-RULE-021` menegaskan keterlambatan verifikasi
hanya memunculkan episode pada daftar pantau kepatuhan. Perawat tetap dapat mengkaji, dokter tetap
dapat menulis resep, dan episode tetap dapat ditutup. Sifat itu disengaja agar tidak melanggar
`RWI-DEC-009`, yang melarang kelengkapan dokumen dijadikan syarat sebelum dokumentasi klinis boleh
ditulis.

> **Contoh.** Perawat menulis CPPT untuk Ibu Sari pada 13 September pukul **01:15**. Bila batas
> verifikasi disetel 24 jam, tenggatnya jatuh pukul **01:15 tanggal 14 September**. Bila dr. Andi
> baru memverifikasi pukul **06:30 tanggal 14 September**, episode Ibu Sari muncul di daftar pantau
> dengan keterangan terlambat **5 jam 15 menit**. Selama rentang itu tidak ada satu pun tindakan
> yang tertahan.

**Angka 24 jam di atas adalah contoh, bukan ketetapan.** `RWI-RULE-021` belum `approved` dan
pemilik klinisnya belum ditunjuk. Arsitektur menyediakan tempat parameternya; nilainya menunggu
pemilik klinis.

### S.4 `AGG-CLI-ASSESSMENT` — Kajian Medis Awal

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-013` Kajian Medis Awal |
| Pemilik | `CTX-CLI` |
| Isi batas | Isi kajian, status pengerjaan, penulis, dan waktu klinis |
| Di luar batas | Catatan SOAP harian, pengkajian keperawatan, dan daftar pantau |
| Invariant yang dijaga | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03` |

Kajian medis awal adalah **konsep yang berbeda** dari catatan SOAP harian: ia dikerjakan sekali di
awal perawatan, isinya menyeluruh, dan keterlambatannya diukur. Gerbang requirement sudah mengunci
pembedaan ini pada bagian 11.9.

**Bentuk penyimpanannya belum ditetapkan, dan itu memang bukan wewenang bagian ini.** Dua pilihan
yang tersedia:

| Pilihan | Konsekuensinya |
| --- | --- |
| Memakai ulang tabel pengkajian yang ada dengan penanda jenis | Cepat, tetapi isian yang ada hari ini bercorak keperawatan — tingkat kesadaran, risiko jatuh, status gizi, kemandirian. Kajian medis butuh anamnesis, pemeriksaan fisik, diagnosis kerja, dan rencana terapi |
| Bentuk penyimpanan tersendiri di dalam `ClinicalManagement` | Lebih jujur terhadap perbedaan isi, tetapi menambah satu bentuk baru yang harus dirawat |

Amendment ini **tidak memilih** salah satunya, karena keduanya menghasilkan model domain yang sama:
satu konsep, satu pemilik, satu lifecycle. Yang berbeda hanya bentuk penyimpanannya, dan itu
pekerjaan `design-business-module`. Pertanyaannya tercatat sebagai `RWI-DOK-TRQ-001` dan
**tidak memblokir** — gerbang requirement bagian 11.7 sudah menyatakannya demikian.

Satu hal yang **tidak boleh** diputuskan di hilir: memindahkan kepemilikannya ke Rawat Inap.
Pemiliknya tetap `ClinicalManagement` apa pun bentuk penyimpanannya.

### S.5 `AGG-PHM-PRESCRIPTION` — Resep dan Obat Pulang

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-017` Resep |
| Pemilik | `CTX-PHM`, yaitu `PharmacyManagement` |
| Isi batas | Isi resep, jenis resep, status resep, status pembayaran, dan status pemenuhan |
| Di luar batas | Penyiapan, peracikan, dan review obat — seluruhnya tetap di luar scope sesuai `RWI-RULE-024` |
| Invariant yang dijaga | `INV-DOK-01` s.d. `INV-DOK-05` |

Tiga hal yang wajib berubah, dan ketiganya sudah punya dasar keputusan:

| Perubahan | Dasar | Keadaan hari ini |
| --- | --- | --- |
| Boleh lebih dari satu resep aktif selama episode | `RWI-DEC-038`, `RWI-DEC-070` | Resep aktif kedua ditolak — `DOK-TRC-INT-02` |
| Penanda jenis "obat pulang" pada resep | `RWI-RULE-024`, `RWI-DEC-046` | Tidak ditemukan penanda jenis resep pada model — `DOK-TRC-CAP023` |
| Resep dapat lahir dalam konteks episode rawat inap | `RWI-RULE-026` | Setiap resep wajib menempel pada satu konsultasi, dan konsultasi kedua sendiri masih ditolak |

**Satu temuan yang perlu dinyatakan terang-terangan.** Karena resep hari ini **wajib** menempel
pada satu konsultasi, dan konsultasi kedua per kunjungan masih ditolak, maka melonggarkan aturan
resep saja **tidak cukup**. Selama dokter tidak dapat membuat catatan kedua, ia juga tidak
mendapat tempat sah untuk menggantungkan resep kedua. Dua pelonggaran itu harus berjalan bersama.
Inilah alasan `RWI-DEC-070` melonggarkan aturan 3, 4, dan 5 sekaligus, bukan hanya salah satunya.

> **Contoh lima hari.** Tn. Budi dirawat 1–5 September 2026 dan diperiksa setiap hari.
>
> | Hari | Yang terjadi hari ini | Setelah aturan berlaku |
> | --- | --- | --- |
> | 1 | Catatan pertama dan resep pertama berhasil | Sama |
> | 2 | **Ditolak** — "Konsultasi dokter untuk encounter ini sudah ada" | Catatan kedua dan resep hari kedua tersimpan |
> | 3–5 | **Ditolak** dengan pesan yang sama | Berjalan normal; episode berisi 5 catatan dan 5 resep |
> | 5, saat pulang | Tidak ada bentuk obat pulang | Resep keenam ditulis dengan jenis **obat pulang**, terlihat berbeda di layar Farmasi |

Setelah obat diserahkan Farmasi, statusnya dibaca balik dan butir "obat pulang sudah diserahkan"
pada daftar periksa administrasi (`RWI-RULE-018`) tertandai. Butir itu **bukan gerbang tersendiri**,
dan dapat dinonaktifkan admin bila rumah sakit belum menghendakinya menahan penutupan episode.

### S.6 `AGG-CLI-PROCEDURE` — Tindakan Dokter

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-014` Tindakan Dokter |
| Pemilik | `CTX-CLI` |
| Isi batas | Rencana tindakan, pelaksanaan, pelaksana, waktu, dan rujukan tarif |
| Di luar batas | Perhitungan tagihan, penentuan tarif final, dan keputusan penjamin — seluruhnya milik `CTX-BIL` |
| Invariant yang dijaga | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-09` |

Fondasinya sudah kuat: tindakan sudah mengenal keadaan `Planned`, penanda sudah dikerjakan, waktu
pelaksanaan, pelaksana, rujukan tarif, dan penerbitan fakta klinis ke Billing.

| Tambahan yang dibutuhkan | Alasan |
| --- | --- |
| Keterikatan pada episode dan kewenangan dokter | `INV-DOK-01`, `INV-DOK-13`; hari ini tindakan hanya terikat pada kunjungan dan konsultasi |
| Tautan opsional ke event visite | Supaya pertanyaan "tindakan ini dikerjakan pada visite yang mana" dapat dijawab. Tetap **opsional**, sesuai sifat tautan pada `RWI-DEC-084` |
| Perlakuan seragam untuk dua jalur pencatatan | Gerbang requirement `RWI-DOK-RQG-005`: rencana lebih dulu lalu dikerjakan, **atau** langsung dicatat sudah dikerjakan. Keduanya dipertahankan sebagai kemampuan; jangan menjadikan perencanaan sebagai kewajiban |

**Urutan yang tidak boleh dibalik.** Catatan klinis disimpan lebih dulu, fakta klinis dikirim ke
Billing sesudahnya. Bila pengiriman ke Billing gagal, catatan klinis **tetap tersimpan**. Ini bukan
preferensi teknis melainkan aturan keselamatan: kegagalan sistem keuangan tidak boleh menghapus
bukti bahwa tindakan medis pernah dikerjakan.

### S.7 `AGG-LAB-ORDER` dan `AGG-RAD-ORDER` — Penunjang

| Field | Nilai |
| --- | --- |
| Akar | `CON-EXT-019` Pesanan Laboratorium; `CON-EXT-020` Pesanan Radiologi dan Studi |
| Pemilik | `CTX-LAB` dan `CTX-RAD` |
| Isi batas | Pesanan, statusnya, dan hasil yang disahkan modul pemiliknya |
| Di luar batas | Penafsiran klinis hasil dan keputusan terapi |
| Invariant yang dijaga | `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-12` |

**Radiologi sekarang ada.** Modulnya memiliki pesanan, studi, modalitas, migration, pendaftaran
layanan, dan penyaring kunjungan. Seluruh pernyataan pada artefak lama yang menyebut "modul
radiologi belum ada" sudah tidak berlaku.

| Kebutuhan | Keadaan | Yang kurang |
| --- | --- | --- |
| Pesanan berjangkar pada kunjungan | Sudah, pada Lab maupun Radiologi | — |
| Pembuktian kepemilikan episode saat membaca | Belum | Penyaring wajib berdasarkan kunjungan milik episode yang sedang dibuka |
| Penyaringan daftar pesanan per kunjungan | Radiologi sudah; **daftar laboratorium belum** | Kemampuan menyaring daftar pesanan laboratorium per kunjungan |
| Pembacaan hasil final terverifikasi | Belum ditemukan | Kontrak membaca hasil yang sudah disahkan, sesuai `INV-DOK-12` |

**Kenapa penyaring kunjungan cukup untuk membuktikan episode.** Karena `INV-INP-04` sudah menjamin
satu episode menempel pada tepat satu kunjungan, dan satu kunjungan menampung paling banyak satu
episode. Jadi kunjungan sudah cukup menjadi jembatan menuju episode, dan **tidak perlu** menambah
kolom episode pada model milik Lab maupun Radiologi. Ini contoh nyata pemakaian ulang tanpa
menambah kepemilikan baru.

### S.8 Perintah bisnis pada scope dokter

| ID | Perintah | Siapa yang boleh | Prasyarat | Akibat |
| --- | --- | --- | --- | --- |
| `CMD-DOK-01` | Buka Konteks Klinis Episode | Dokter yang berwenang atas pasien itu | Episode ada dan belum `Closed`/`Cancelled`; pasien cocok | Konteks terbentuk; seluruh perintah berikutnya memakainya |
| `CMD-DOK-02` | Tulis atau perbarui catatan dokter (SOAP) | Dokter yang berwenang | Konteks sah; catatan belum final | Catatan tersimpan berstatus `Draft` |
| `CMD-DOK-03` | Finalkan catatan dokter | Penulis catatan | Catatan `Draft`; isi minimum terisi | Catatan `Completed` dan terkunci dari penyuntingan langsung |
| `CMD-DOK-04` | Tambahkan addendum koreksi | Penulis, atau penulis pengganti yang punya pendelegasian sah | Dokumen sudah final; **alasan koreksi wajib** | Addendum bernomor urut tersimpan; isi asli tidak berubah |
| `CMD-DOK-05` | Tulis CPPT | Profesi yang berwenang menulis CPPT | Konteks sah | CPPT tersimpan berstatus belum diverifikasi |
| `CMD-DOK-06` | Verifikasi CPPT | **DPJP aktif episode itu** | CPPT ada; verifikator adalah DPJP aktif saat itu | CPPT terverifikasi; waktu dan pelaku tersimpan |
| `CMD-DOK-07` | Buat atau selesaikan kajian medis awal | Dokter yang berwenang | Konteks sah; belum ada kajian medis berlaku untuk episode itu | Kajian tersimpan, lalu `Completed` |
| `CMD-DOK-08` | Tulis resep harian atau obat pulang | Dokter yang berwenang | Konteks sah; kunjungan bertipe `Inpatient` atau `Emergency` | Resep terkirim ke Farmasi dengan jenisnya |
| `CMD-DOK-09` | Rencanakan atau catat tindakan | Dokter yang berwenang | Konteks sah | Tindakan tersimpan; bila sudah dikerjakan, fakta klinis diterbitkan ke Billing |
| `CMD-DOK-10` | Pesan pemeriksaan laboratorium atau radiologi | Dokter yang berwenang | Konteks sah | Pesanan tercatat di modul pemiliknya |
| `CMD-DOK-11` | Baca hasil penunjang | Dokter yang berwenang, dan peran lain yang berhak melihat | Hasil final terverifikasi dan milik episode yang dibuka | Hasil tampil di ruang kerja |
| `CMD-DOK-12` | Catat event visite | Dokter yang berwenang atas pasien itu | Konteks sah; kunci permintaan terisi | Event `Recorded`; bila kunci sudah pernah dipakai, event yang sama dikembalikan |
| `CMD-DOK-13` | Batalkan event visite | Pemilik event atau supervisor | **Alasan wajib** | Event `Cancelled`, tetap tersimpan, tidak ikut dihitung |

### S.9 Invariant yang **tidak** dapat dijaga satu aggregate

Dinyatakan terbuka, bukan disembunyikan.

| ID | Kenapa di luar jangkauan satu aggregate | Cara menjaganya |
| --- | --- | --- |
| `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03` | Datanya berada di dua context: dokumen di `CTX-CLI`/`CTX-PHM`/`CTX-LAB`/`CTX-RAD`, episode di `CTX-INP-CARE` | Lewat `CON-INP-015` Konteks Klinis Episode yang wajib dipanggil setiap perintah tulis. Bentuk teknisnya adalah pekerjaan `design-business-module` |
| `INV-DOK-05` | Menyangkut perilaku jalur rawat jalan dan medical check-up yang berada di luar scope ini | Uji regresi wajib, sesuai `RWI-DEC-051` dan `RWI-AC-143` |
| `INV-DOK-06` | Keunikan kunci permintaan menyangkut banyak permintaan sekaligus, bukan satu event | Jaminan keunikan pada penyimpanan, bukan pemeriksaan di dalam kode saja |
| `INV-DOK-09` | Billing berada di modul lain | Arah kontrak satu arah pada bagian X, ditambah larangan tegas pada bagian Y |
| `INV-DOK-13` | Mesin hak akses hanya mengenal peran terhadap endpoint | Penjaga kewenangan per pasien di dalam perintah bisnis, sebagaimana bagian H.2 sudah menetapkan untuk episode |

---

## T. Model relasi

### T.1 Relasi yang material

| Sumber | Tujuan | Makna bisnis | Kardinalitas | Wajib | Ketergantungan lifecycle |
| --- | --- | --- | --- | ---: | --- |
| `CON-INP-001` Episode | `CON-EXT-011` Catatan Dokter | Episode menampung catatan dokter selama perawatan | 1 : banyak | Tidak | Catatan tidak ikut mati; ia bagian rekam medis pasien |
| `CON-INP-001` Episode | `CON-EXT-012` CPPT | Episode menampung catatan lintas profesi | 1 : banyak | Tidak | Sama seperti di atas |
| `CON-INP-001` Episode | `CON-EXT-013` Kajian Medis Awal | Episode dikaji sekali di awal | 1 : 0..1 | Tidak sampai kajian dibuat | Tetap tersimpan setelah episode ditutup |
| `CON-INP-001` Episode | `CON-EXT-015` Event Visite | Episode menampung kunjungan-kunjungan dokter | 1 : banyak | Tidak | Tetap tersimpan setelah episode ditutup |
| `CON-INP-001` Episode | `CON-EXT-017` Resep | Episode menampung resep harian dan obat pulang | 1 : banyak | Tidak | Tetap tersimpan |
| `CON-INP-001` Episode | `CON-EXT-014` Tindakan | Episode menampung tindakan dokter | 1 : banyak | Tidak | Tetap tersimpan |
| `CON-INP-001` Episode | `CON-EXT-019`/`020` Pesanan Penunjang | Episode menampung pesanan lab dan radiologi | 1 : banyak | Tidak | Tetap tersimpan |
| `CON-EXT-015` Event Visite | `CON-EXT-008` Dokter | Satu event menyatakan satu dokter | banyak : 1 | Ya | Dokter hidup mandiri |
| `CON-EXT-015` Event Visite | `CON-EXT-011` Catatan Dokter | Event **boleh** ditautkan ke catatan | banyak : 0..banyak | **Tidak** | Tautan boleh kosong; catatan tetap ada tanpa event |
| `CON-EXT-015` Event Visite | `CON-EXT-012` CPPT | Event **boleh** ditautkan ke CPPT | banyak : 0..banyak | **Tidak** | Sama |
| `CON-EXT-015` Event Visite | `CON-EXT-014` Tindakan | Event **boleh** ditautkan ke tindakan | banyak : 0..banyak | **Tidak** | Sama |
| `CON-EXT-011` Catatan Dokter | `CON-EXT-017` Resep | Resep digantungkan pada satu catatan | 1 : banyak | Ya, selama bentuk hari ini dipertahankan | Resep tidak dapat lahir tanpa catatan |
| `CON-EXT-011` Catatan Dokter | `CON-EXT-014` Tindakan | Tindakan digantungkan pada satu catatan | 1 : banyak | Ya, selama bentuk hari ini dipertahankan | Sama |
| `CON-EXT-011`, `012`, `013`, `014` | `CON-EXT-021` Integritas Dokumen | Dokumen klinis ditandatangani dan dikunci | 1 : 0..1 | Tidak | Integritas ikut umur dokumennya |
| `CON-EXT-021` Integritas | `CON-EXT-022` Addendum | Dokumen final dikoreksi dengan addendum bernomor urut | 1 : banyak | Tidak | Addendum tidak dapat dihapus |
| `CON-EXT-014` Tindakan | `CON-EXT-016` Fakta Klinis | Tindakan yang dikerjakan menerbitkan fakta ke Billing | 1 : banyak versi | Tidak | Fakta hidup mandiri di sisi Billing |
| `CON-INP-015` Konteks Klinis | Seluruh dokumen di atas | Konteks membuktikan dokumen milik episode yang benar | dihitung | Ya, pada setiap penulisan | Tidak disimpan, jadi tidak punya umur |

### T.2 Kenapa dokumen klinis tidak ikut mati bersama episode

Ini perbedaan penting dengan bagian F. Konsep milik Rawat Inap seperti pemesanan tempat tidur dan
penandaan daftar periksa **ikut mati** bersama episode, karena maknanya memang hanya ada di dalam
episode itu.

Dokumen klinis tidak begitu. Catatan dokter, CPPT, kajian medis, resep, tindakan, hasil penunjang,
dan event visite adalah **rekam medis pasien**, yang umurnya jauh melampaui satu episode dan diatur
kewajiban penyimpanan rekam medis. Episode hanyalah konteks tempat dokumen itu lahir.

Konsekuensi praktisnya satu dan penting: **menutup atau membatalkan episode tidak boleh menghapus
satu pun dokumen klinis.** Yang berubah hanya kemampuan menulis dokumen baru (`INV-DOK-03`), bukan
keberadaan dokumen lama.

### T.3 Tautan visite yang sengaja dibuat longgar

Tautan antara event visite dan dokumen klinis dibuat **opsional di kedua arah**, dan itu keputusan
sadar dari `RWI-DEC-084`:

| Keadaan | Sah? | Apa artinya |
| --- | :---: | --- |
| Event visite tanpa dokumen apa pun | **Ya** | Dokter datang, memeriksa, dan belum sempat menulis. Kunjungannya tetap tercatat |
| Catatan SOAP tanpa event visite | **Ya** | Dokter menulis tambahan tanpa kedatangan baru. Tidak menambah hitungan visite — `RWI-AC-151` |
| Event visite dengan satu atau beberapa dokumen tertaut | **Ya** | Bentuk paling lengkap |
| Event visite yang **wajib** punya dokumen | **Tidak** | Akan mengembalikan aturan lama yang sudah `superseded` |

Bentuk longgar ini juga yang membuat urutan waktu tidak lagi menjadi masalah: event pukul 07:40
dapat ditautkan ke SOAP yang baru ditulis pukul 07:52 tanpa mengubah waktu visitenya sedikit pun.

---

## U. Model lifecycle, status, dan proses bisnis

Bagian ini menjelaskan **bagaimana pekerjaan dokter benar-benar berjalan**, bukan sekadar daftar
status. Setiap proses ditulis dengan susunan yang sama: tujuan, pelaku, pemicu, prasyarat, langkah,
aturan, perubahan status, jalur tidak normal, dan hasil akhir.

### U.1 Ringkasan status seluruh konsep pada scope dokter

| Konsep | Status yang berlaku | Status terminal | Cara koreksi setelah final |
| --- | --- | --- | --- |
| `CON-EXT-011` Catatan Dokter | `Draft`, `Completed`, `Cancelled` | `Completed`, `Cancelled` | Addendum bernomor urut |
| `CON-EXT-012` CPPT | Belum diverifikasi, Sudah diverifikasi; dapat dibatalkan dengan alasan | Dibatalkan | Addendum |
| `CON-EXT-013` Kajian Medis Awal | `Draft`, `InProgress`, `Completed`, `Cancelled` | `Completed`, `Cancelled` | Addendum |
| `CON-EXT-015` Event Visite | `Recorded`, `Cancelled` | `Cancelled` | Batalkan lalu catat ulang |
| `CON-EXT-017` Resep | Mengikuti lifecycle Farmasi: status resep, status pembayaran, status pemenuhan | Ditentukan Farmasi | Milik Farmasi |
| `CON-EXT-014` Tindakan | `Planned`, dikerjakan, dibatalkan | Dibatalkan atau selesai dikerjakan | Milik `ClinicalManagement`, dengan koreksi fakta ke Billing |
| `CON-EXT-019`/`020` Pesanan Penunjang | Diminta, ditahan, dikerjakan, selesai, dibatalkan — milik modul pemiliknya | Ditentukan modul pemiliknya | Milik modul pemiliknya |

**Satu aturan yang berlaku untuk semuanya:** tidak ada penghapusan keras dan tidak ada penimpaan
diam-diam (`INV-DOK-10`). Yang salah dibatalkan atau dikoreksi dengan jejak, tidak dihilangkan.

### U.2 Proses 1 — Membuka daftar pasien dan konteks klinis

**Tujuan.** Dokter melihat pasien rawat inap yang benar-benar menjadi tanggung jawabnya, lalu
membuka satu pasien sebagai konteks kerja untuk seluruh kegiatan klinis hari itu.

**Pelaku.** DPJP dan dokter jaga ruangan sebagai pengguna utama. Tidak ada peran lain yang memulai
proses ini.

**Pemicu.** Dokter membuka ruang kerja Dokter Rawat Inap.

**Prasyarat.** Ada episode berstatus `Admitted` atau `DischargePending`, dan dokter memiliki
kewenangan atas pasien tersebut.

**Langkah utama.**

1. Dokter membuka daftar pasien rawat inap.
2. Sistem menampilkan pasien berdasarkan **census episode aktif** yang disaring pada dokter yang
   sedang masuk — bukan berdasarkan antrean poliklinik.
3. Dokter memilih satu pasien.
4. Sistem membentuk Konteks Klinis Episode (`CMD-DOK-01`): pasien, kunjungan, episode, status
   episode, dan kewenangan dokter.
5. Seluruh kegiatan berikutnya memakai konteks itu tanpa dokter perlu memilih ulang.

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Daftar pasien berasal dari census episode, tidak boleh dari antrean rawat jalan | `RWI-RULE-026` aturan 2, `DOK-TRC-FE-01` |
| Tidak ada aksi antrean — panggil, lewati, tidak hadir — pada ruang kerja rawat inap | Aksi itu milik alur poliklinik dan tidak punya makna bagi pasien yang berbaring di kamar |
| Konteks wajib membuktikan pasien, kunjungan, dan episode cocok | `INV-DOK-01`, `INV-DOK-02` |

**Perubahan status.** Tidak ada. Membuka konteks adalah pembacaan, bukan perubahan data.

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Episode sudah `Closed` atau `Cancelled` | Konteks tetap dapat dibuka untuk **membaca** riwayat, tetapi seluruh perintah tulis ditolak — `INV-DOK-03` |
| Dokter bukan DPJP dan tidak punya kewenangan lain atas pasien itu | Ditolak dengan pesan yang menyebutkan sebabnya, bukan hanya kode 403 |
| Pasien pada permintaan berbeda dengan pasien pada episode | Ditolak. Ini penjaga salah pasien — `INV-DOK-02` |

**Hasil akhir.** Dokter memegang satu konteks kerja yang sah. Tidak ada satu pun baris antrean yang
dibuat, dan laporan antrean poliklinik tetap bersih.

> **Keadaan hari ini berbeda dari uraian di atas, dan itu diketahui.** Ruang kerja yang sudah
> ter-*commit* memanggil daftar antrean dokter rawat jalan pada tanggal hari ini, lengkap dengan
> aksi panggil, lewati, dan tidak hadir. Statusnya `Conflict` pada `DOK-TRC-FE-01` dan menahan
> sign-off maupun rilis. Arsitektur ini menyatakan bentuk targetnya; perbaikannya adalah pekerjaan
> `design-business-module` dan delivery.

### U.3 Proses 2 — Kajian medis awal (`CAP-022`)

**Tujuan.** Dokter melakukan pemeriksaan menyeluruh pertama saat pasien mulai dirawat, sebagai
dasar seluruh rencana perawatan berikutnya.

**Pelaku.** Dokter yang berwenang atas pasien. DPJP bertanggung jawab atas isinya.

**Pemicu.** Pasien menempati tempat tidur dan episode menjadi `Admitted`.

**Prasyarat.** Konteks klinis sah, dan belum ada kajian medis yang berlaku untuk episode tersebut.

**Langkah utama.**

1. Dokter membuka pasien dari daftar census.
2. Dokter mengisi kajian medis: anamnesis, pemeriksaan fisik, penilaian awal, dan rencana.
3. Dokter menyimpan sebagai konsep bila belum selesai.
4. Dokter menyelesaikan kajian (`CMD-DOK-07`).
5. Sistem mencatat waktu penyelesaian dan menghitung keterlambatan terhadap batas yang disetel.

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Satu episode memiliki paling banyak satu kajian medis awal yang berlaku | `CON-EXT-013` |
| Kajian medis berbeda dari catatan SOAP harian dan dari pengkajian keperawatan | Gerbang requirement bagian 11.9 |
| Batas waktu penyelesaian **tidak menahan** tindakan apa pun | `RWI-RULE-021` |
| Nilai batas waktunya adalah parameter yang dapat diubah admin | `RWI-RULE-021`, `RWI-RULE-034` |

**Perubahan status.**

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Mulai mengisi | `Draft` | Dokter berwenang | Konteks sah |
| `Draft` | Lanjutkan mengisi | `InProgress` | Penulis | — |
| `InProgress` | Selesaikan | `Completed` | Penulis | Isian minimum terpenuhi |
| `Draft`/`InProgress` | Batalkan | `Cancelled` | Penulis atau supervisor | Alasan wajib |
| `Completed` | Koreksi | Tetap `Completed` | Penulis atau penulis pengganti yang sah | Lewat addendum, alasan wajib |

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Kajian belum selesai melewati batas waktu | Episode muncul pada daftar pantau kepatuhan beserta lama keterlambatannya. Tidak ada tindakan yang dihalangi |
| Dokter salah memilih pasien | Ditolak oleh `INV-DOK-02` sebelum tersimpan |
| Episode ditutup sebelum kajian selesai | Kajian tidak dapat diselesaikan sebagai dokumen baru; yang sudah tersimpan tetap ada |

**Hasil akhir.** Episode memiliki satu kajian medis awal yang dapat dibaca seluruh profesi, dan
angka kepatuhannya dapat dilaporkan saat akreditasi.

### U.4 Proses 3 — Catatan dokter harian berisi SOAP (`CAP-020`)

**Tujuan.** Dokter mencatat perkembangan pasien setiap hari sehingga profesi lain dan dokter
pengganti dapat melanjutkan perawatan tanpa bertanya ulang.

**Pelaku.** DPJP dan dokter jaga ruangan.

**Pemicu.** Dokter selesai memeriksa pasien.

**Prasyarat.** Konteks klinis sah dan episode belum `Closed`.

**Langkah utama.**

1. Dokter membuka pasien dari census.
2. Dokter menulis SOAP: keluhan pasien, temuan pemeriksaan, penilaian, dan rencana.
3. Isian tersimpan otomatis sebagai konsep selama dokter mengetik.
4. Dokter menekan Selesai (`CMD-DOK-03`); catatan menjadi final dan terkunci dari penyuntingan.
5. Bila kemudian ada yang perlu dibetulkan, dokter menambah addendum beserta alasannya
   (`CMD-DOK-04`).

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Satu episode boleh memiliki **banyak** catatan dokter | `INV-DOK-04` |
| Pelonggaran hanya untuk kunjungan `Inpatient` dan `Emergency` | `INV-DOK-05` |
| Catatan yang sudah final tidak dapat disunting langsung | `INV-DOK-10` |
| Tidak ada baris antrean yang dibuat | `RWI-RULE-026` aturan 2 |

**Perubahan status.**

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Mulai menulis | `Draft` | Dokter berwenang | Konteks sah |
| `Draft` | Finalkan | `Completed` | Penulis | Isian minimum terpenuhi |
| `Draft` | Batalkan | `Cancelled` | Penulis atau supervisor | Alasan wajib |
| `Completed` | Koreksi | Tetap `Completed` | Penulis atau pengganti yang sah | Addendum, alasan wajib |

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Pasien tidak punya baris antrean — keadaan **normal** bagi pasien menginap | Catatan tetap tersimpan. Hari ini justru inilah yang gagal: jalur tanpa antrean berujung pada kegagalan sistem (kode 500) karena data antrean yang kosong tetap ditulis. Diperbaiki lebih dulu, sebelum kemampuan ini dianggap ada |
| Dokter menulis catatan kedua pada hari yang sama | Diizinkan. Keduanya tersimpan sebagai dua catatan |
| Episode sudah ditutup | Catatan baru ditolak; catatan lama tetap terbaca |

**Hasil akhir.** Episode Tn. Budi yang dirawat lima hari berisi lima catatan dokter yang terbaca
berurutan, bukan satu catatan untuk seluruh masa perawatan.

### U.5 Proses 4 — CPPT dan verifikasi DPJP (`CAP-021`)

**Tujuan.** Seluruh profesi menulis perkembangan pasien pada satu lembar yang sama, dan DPJP
menyatakan sudah membaca serta menyetujui catatan profesi lain.

**Pelaku.** Dokter, perawat, gizi, dan farmasi sebagai penulis. **DPJP aktif** sebagai verifikator.

**Pemicu.** Ada perkembangan pasien yang perlu dicatat, atau ada catatan profesi lain yang menunggu
verifikasi.

**Prasyarat.** Konteks klinis sah; untuk verifikasi, pelakunya adalah DPJP aktif episode saat itu.

**Langkah utama.**

1. Profesi penulis membuka pasien dan menulis CPPT (`CMD-DOK-05`).
2. Catatan tersimpan berstatus belum diverifikasi, dengan profesi, penulis, dan waktu klinisnya.
3. DPJP membuka daftar catatan yang menunggu verifikasi.
4. DPJP memverifikasi (`CMD-DOK-06`); waktu dan pelakunya tersimpan.
5. Catatan yang lewat batas waktu muncul pada daftar pantau kepatuhan.

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Hanya DPJP aktif yang boleh memverifikasi | `INV-DOK-11` |
| Batas waktu verifikasi tidak menahan tindakan apa pun | `RWI-RULE-021` |
| Nilai batasnya dapat diubah admin | `RWI-RULE-034` |
| Catatan yang sudah tersimpan tidak dapat disunting diam-diam | `INV-DOK-10` |

**Perubahan status.**

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Tulis CPPT | Belum diverifikasi | Profesi berwenang | Konteks sah |
| Belum diverifikasi | Verifikasi | Sudah diverifikasi | **DPJP aktif** | Verifikator adalah DPJP saat itu |
| Belum diverifikasi | Batalkan | Dibatalkan | Penulis atau supervisor | Alasan wajib |
| Sudah diverifikasi | Koreksi | Tetap terverifikasi | Penulis atau pengganti sah | Addendum, alasan wajib |

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| DPJP berganti sebelum verifikasi dilakukan | Yang berwenang adalah DPJP yang **aktif saat verifikasi**, bukan yang aktif saat catatan ditulis |
| Verifikasi terlambat | Muncul pada daftar pantau. Perawatan berjalan normal |
| Nilai batas waktu belum ditetapkan pemilik klinis | Parameter dibiarkan kosong dan daftar pantau tidak menampilkan keterlambatan. **Jangan mengarang angka** |

**Hasil akhir.** Satu lembar CPPT terisi lintas profesi, dengan jejak siapa menulis, siapa
memverifikasi, dan kapan.

### U.6 Proses 5 — Mencatat visite dokter (`CAP-025`)

**Tujuan.** Merekam bahwa dokter benar-benar mendatangi pasien, sebagai fakta klinis dan operasional
yang berdiri sendiri.

**Pelaku.** Dokter yang memiliki kewenangan atas pasien. Pencatatan oleh petugas administrasi atas
nama dokter **tidak tersedia** sampai ada kebijakan eksplisit yang menyetujuinya.

**Pemicu.** Dokter selesai mendatangi pasien.

**Prasyarat.** Konteks klinis sah; episode `Admitted` atau `DischargePending`; permintaan membawa
kunci permintaan.

**Langkah utama.**

1. Dokter membuka pasien dari census.
2. Dokter mencatat visite: waktu kedatangan, peran atau konteksnya saat itu, dan catatan singkat
   bila ada (`CMD-DOK-12`).
3. Sistem menyimpan satu event berstatus `Recorded` beserta kunci permintaannya.
4. Bila dokter kemudian menulis SOAP, CPPT, atau mencatat tindakan, dokumen itu **boleh**
   ditautkan ke event — dan boleh juga tidak.
5. Riwayat visite menampilkan seluruh event beserta waktunya.

**Aturan bisnis.**

| Aturan | Contoh berangka | Dasar |
| --- | --- | --- |
| Setiap visite nyata dihitung satu | dr. Andi datang pukul 07:40 dan 16:10 pada 12 September: hitungan **2** | `RWI-DEC-085`, `RWI-AC-154` |
| Kiriman ulang dengan kunci permintaan sama tidak membuat event kedua | Tombol Simpan tertekan dua kali: hitungan tetap **1** | `INV-DOK-06`, `RWI-AC-152` |
| SOAP atau CPPT tanpa event tidak menambah visite | dr. Andi menulis SOAP pukul 16:10 tanpa mencatat event: hitungan **tidak bertambah** | `INV-DOK-07`, `RWI-AC-151` |
| Waktu yang tercatat adalah waktu kedatangan | Event pukul 07:40 tetap 07:40 walaupun SOAP baru ditulis 07:52 | `RWI-AC-150` |
| Agregasi tagihan tidak mengubah riwayat klinis | Billing menggabungkan dua event menjadi satu tagihan harian; riwayat tetap menampilkan dua event lengkap | `INV-DOK-09`, `RWI-AC-156` |

**Perubahan status.**

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Catat visite | `Recorded` | Dokter berwenang | Konteks sah, kunci permintaan terisi |
| `Recorded` | Tautkan dokumen | `Recorded` | Pemilik event | Dokumen milik episode yang sama |
| `Recorded` | Batalkan | `Cancelled` | Pemilik event atau supervisor | Alasan wajib |

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Jaringan putus lalu aplikasi mengirim ulang | Kunci permintaan yang sama mengembalikan **event yang sama**, bukan kesalahan dan bukan event kedua — `RWI-AC-155` |
| Dokter salah mengisi jam | Batalkan dengan alasan, lalu catat ulang. Keduanya tetap terlihat |
| Dokter mencatat visite pada episode yang sudah ditutup | Ditolak — `INV-DOK-03` |
| Dokter mencatat visite untuk pasien yang bukan tanggung jawabnya | Ditolak — `INV-DOK-13` |

**Hasil akhir.** Riwayat visite menampilkan episode, dokter, peran, waktu, pencatat, dan tautan
dokumen bila ada — persis yang diminta `RWI-AC-153`. Angka visite klinis siap dipakai laporan
operasional, terpisah dari angka tagihan.

### U.7 Proses 6 — Resep harian dan obat pulang (`CAP-023`)

**Tujuan.** Dokter meresepkan obat selama perawatan dan saat pasien pulang, lalu mengetahui apakah
obatnya sudah diserahkan.

**Pelaku.** Dokter sebagai penulis resep. Petugas farmasi menyiapkan dan menyerahkan. Petugas
admisi memakai status penyerahan sebagai butir daftar periksa.

**Pemicu.** Dokter memutuskan terapi obat, atau memutuskan pasien boleh pulang.

**Prasyarat.** Konteks klinis sah; kunjungan bertipe `Inpatient` atau `Emergency`.

**Langkah utama.**

1. Dokter menulis resep dari ruang kerja pasien (`CMD-DOK-08`).
2. Dokter menandai jenisnya: resep harian atau **obat pulang**.
3. Resep terkirim ke Farmasi dengan konteks pasien dan kunjungan yang sama.
4. Farmasi menyiapkan dan menyerahkan obat.
5. Status penyerahan dibaca balik oleh Rawat Inap.
6. Untuk obat pulang, butir "obat pulang sudah diserahkan" pada daftar periksa administrasi
   tertandai.

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Boleh lebih dari satu resep aktif selama episode | `INV-DOK-04`, `RWI-DEC-070` |
| Penanda obat pulang disimpan di sisi Farmasi, bukan di Rawat Inap | `RWI-DEC-046` |
| Obat pulang bukan gerbang tersendiri; ia satu butir pada daftar periksa yang sudah ada | `RWI-RULE-024` |
| Butir itu dapat dinonaktifkan admin | `RWI-RULE-024`, `RWI-RULE-034` |
| Penyiapan, peracikan, dan review obat tetap di luar scope | `RWI-RULE-024` |

**Perubahan status.** Lifecycle resep dimiliki Farmasi. Rawat Inap **membaca** status pemenuhan dan
tidak pernah menulisnya.

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Farmasi belum dapat mengembalikan status penyerahan | Butir daftar periksa ditandai manual oleh petugas admisi, sama seperti butir lain |
| Pasien pulang sebelum obat diserahkan | Penutupan episode tertahan pada butir daftar periksa itu, bukan pada gerbang baru |
| Resep kedua ditulis saat resep pertama masih aktif | Diizinkan untuk rawat inap dan IGD. Untuk rawat jalan dan medical check-up **tetap ditolak dengan pesan yang sama persis seperti sebelumnya** — `RWI-AC-143` |

**Hasil akhir.** Seluruh resep selama episode terbaca berurutan, obat pulang dikenali petugas
farmasi di layar mereka sendiri, dan penutupan episode tidak lagi tersangkut pada informasi yang
tidak pernah sampai.

### U.8 Proses 7 — Tindakan dokter (`CAP-024`)

**Tujuan.** Mencatat prosedur medis yang direncanakan atau dikerjakan dokter, sekaligus memberi
tahu Billing bahwa tindakan itu benar terjadi.

**Pelaku.** Dokter sebagai pelaksana. Billing sebagai penerima fakta, bukan sebagai pemberi izin.

**Pemicu.** Dokter merencanakan atau mengerjakan tindakan.

**Prasyarat.** Konteks klinis sah.

**Langkah utama.**

1. Dokter mencatat tindakan, dengan dua jalur yang sama-sama sah: direncanakan lebih dulu, atau
   langsung dicatat sudah dikerjakan (`CMD-DOK-09`).
2. Bila direncanakan, tindakan menunggu pelaksanaan.
3. Saat dikerjakan, dokter menandainya beserta waktu dan pelaksana.
4. **Catatan klinis disimpan lebih dulu.**
5. Sesudah itu fakta klinis diterbitkan ke Billing.
6. Bila tindakan dikerjakan dalam rangkaian satu visite, tindakan **boleh** ditautkan ke event
   visite.

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Perencanaan **tidak wajib**; dua jalur pencatatan dipertahankan | `RWI-DOK-RQG-005` |
| Kegagalan pengiriman ke Billing tidak menghapus catatan klinis | Gerbang requirement dimensi 16 |
| Pengiriman fakta bersifat idempotent — kiriman ulang tidak menghasilkan tagihan ganda | `ClinicalMilestoneFactProducer` sudah membangun kunci idempotency dari identitas dan versi fakta |
| Billing tidak boleh mengubah catatan klinis | `INV-DOK-09` |

**Perubahan status.**

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Rencanakan | `Planned` | Dokter berwenang | Konteks sah |
| `Planned` | Kerjakan | Dikerjakan | Dokter pelaksana | Waktu dan pelaksana terisi |
| — | Catat langsung sudah dikerjakan | Dikerjakan | Dokter pelaksana | Konteks sah |
| `Planned`/Dikerjakan | Batalkan | Dibatalkan | Dokter atau supervisor | Alasan wajib; fakta ke Billing dikoreksi mengikuti kontrak fakta |

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Billing tidak dapat dihubungi | Catatan klinis tetap tersimpan; pengiriman fakta masuk daftar percobaan ulang |
| Tindakan dibatalkan setelah fakta terkirim | Kontrak fakta menyediakan jalur koreksi: bila belum ada tagihan terbentuk, koreksi ditekan; bila keadaan sebelumnya tidak diketahui, wajib rekonsiliasi lebih dulu |
| Tindakan dicatat pada episode yang sudah ditutup | Ditolak — `INV-DOK-03` |

**Hasil akhir.** Tindakan terbaca pada riwayat pasien, dan Billing menerima fakta yang dapat
ditagihkan tanpa satu pun kemungkinan tagihan ganda akibat kiriman berulang.

### U.9 Proses 8 — Pemeriksaan penunjang laboratorium dan radiologi (`CAP-015`)

**Tujuan.** Dokter memesan pemeriksaan penunjang dan membaca hasilnya dari ruang kerja yang sama,
tanpa berpindah modul dan tanpa risiko tertukar pasien.

**Pelaku.** Dokter sebagai pemesan. Petugas laboratorium dan radiologi sebagai pelaksana dan
pengesah hasil.

**Pemicu.** Dokter membutuhkan pemeriksaan penunjang.

**Prasyarat.** Konteks klinis sah.

**Langkah utama.**

1. Dokter memesan pemeriksaan dari ruang kerja pasien (`CMD-DOK-10`).
2. Pesanan tercatat di modul pemiliknya, berjangkar pada kunjungan pasien.
3. Modul pemilik mengerjakan pemeriksaan dan mengesahkan hasilnya.
4. Ruang kerja dokter membaca **hanya hasil final terverifikasi** milik episode yang sedang dibuka
   (`CMD-DOK-11`).
5. Hasil tampil pada linimasa pasien bersama catatan klinis lainnya.

**Aturan bisnis.**

| Aturan | Dasar |
| --- | --- |
| Sumber kebenaran hasil tetap modul Laboratorium dan Radiologi | `RWI-DEC-081` semangatnya, dan pembagian kepemilikan pada gerbang requirement dimensi 10 |
| Hanya hasil final terverifikasi yang boleh menjadi dasar keputusan klinis | `INV-DOK-12` |
| Pesanan dan hasil yang ditampilkan wajib milik episode yang dibuka | `INV-DOK-01`, `INV-DOK-02` |
| Tidak ada salinan hasil di sisi Rawat Inap | Bagian R.4 |

**Perubahan status.** Seluruhnya dimiliki modul Laboratorium dan Radiologi. Rawat Inap tidak
mengubah satu pun status pesanan.

**Jalur tidak normal.**

| Keadaan | Yang harus terjadi |
| --- | --- |
| Hasil belum final | Ditampilkan sebagai "belum final" dan **tidak** boleh disajikan seolah hasil sah |
| Pasien punya dua episode berbeda pada waktu berbeda | Hanya pesanan dan hasil milik kunjungan episode yang sedang dibuka yang tampil |
| Pesanan dibatalkan modul pemilik | Ruang kerja menampilkan pembatalan itu apa adanya |

**Hasil akhir.** Dokter membaca hasil yang benar, milik pasien yang benar, pada episode yang benar
— tanpa Rawat Inap menyimpan satu baris hasil pun.

---

## V. Tanggung jawab authorization

Tidak ada peran baru yang dikarang. Seluruhnya diambil dari aturan yang sudah dikunci dan dari
kemampuan yang sudah terbukti ada pada source.

### V.1 Wewenang per tindakan

| Tindakan | Memulai | Melihat | Mengubah | Menyetujui atau memverifikasi | Membatalkan atau mengoreksi |
| --- | --- | --- | --- | --- | --- |
| Membuka konteks klinis episode | Dokter berwenang atas pasien | Dokter, perawat, DPJP, supervisor | — | — | — |
| Kajian medis awal | Dokter berwenang | Seluruh profesi perawatan pasien itu | Penulis, selama belum final | — | Penulis atau penulis pengganti yang sah, lewat addendum |
| Catatan dokter berisi SOAP | Dokter berwenang | Seluruh profesi perawatan pasien itu | Penulis, selama `Draft` | — | Penulis atau pengganti sah, lewat addendum |
| CPPT | Profesi yang berwenang menulis CPPT | Seluruh profesi perawatan pasien itu | Penulis, selama belum diverifikasi | **DPJP aktif** | Penulis atau pengganti sah, lewat addendum |
| Event visite | Dokter berwenang atas pasien | Dokter, DPJP, supervisor, auditor | Hanya penambahan tautan dokumen | — | Pemilik event atau supervisor, dengan alasan wajib |
| Resep harian dan obat pulang | Dokter berwenang | Dokter, farmasi, admisi | Penulis, mengikuti aturan Farmasi | Farmasi saat menyiapkan dan menyerahkan | Mengikuti aturan Farmasi |
| Tindakan dokter | Dokter berwenang | Dokter, DPJP, supervisor | Dokter pelaksana | — | Dokter atau supervisor, alasan wajib |
| Pesanan penunjang | Dokter berwenang | Dokter, petugas lab/radiologi | Modul pemiliknya | Petugas yang mengesahkan hasil | Modul pemiliknya |
| Pembacaan hasil penunjang | Dokter dan peran lain yang berhak melihat | Sama | — | — | — |

### V.2 Dua tingkat kewenangan, diberlakukan pada scope klinis

Pembedaan pada bagian H.2 berlaku penuh di sini, dan justru **lebih penting** karena menyangkut
dokumen medis.

| Tingkat | Contohnya pada scope dokter | Dijaga di mana | Keadaan hari ini |
| --- | --- | --- | --- |
| **Kewenangan peran** — "peran ini boleh melakukan tindakan ini" | Peran dokter boleh membuat catatan dokter | Mesin hak akses yang sudah ada, berbasis pasangan sumber daya dan aksi | **Sudah ada.** Setiap controller klinis memakai penjaga hak akses |
| **Kewenangan per pasien** — "orang ini boleh melakukan tindakan ini **terhadap pasien ini**" | Hanya dokter yang berwenang atas Tn. Budi yang boleh menulis catatan Tn. Budi; hanya DPJP aktif yang boleh memverifikasi CPPT-nya | **Harus dijaga di dalam perintah bisnis**, bukan di lapisan luar | **Belum ada** pada jalur klinis. Bukti `DOK-TRC-AUTH-01` |

**Kenapa ini bukan sekadar soal keamanan.** Hak akses peran hanya menjawab "boleh membuat catatan
dokter". Ia tidak pernah menjawab "boleh membuat catatan **untuk pasien ini**". Tanpa penjaga
kedua, seorang dokter yang berwenang atas bangsal A tetap dapat menulis pada rekam medis pasien
bangsal B, dan sistem tidak akan menganggapnya salah.

**Fondasinya sudah tersedia dan tidak perlu dibuat dari nol.** Layanan episode sudah memiliki
pemeriksaan dokter aktif per episode, dan pemeriksaan itu sudah dipakai pada jalur perpindahan
tempat tidur serta pemulangan. Yang belum ada adalah **pemanggilannya pada jalur klinis dokter**.

### V.3 Risiko yang melekat dan cara menguranginya

Risiko terbesar bentuk ini adalah **lupa memanggil penjaga** pada satu perintah baru. Risiko yang
sama sudah dicatat sebagai `RWI-RISK-004` untuk episode. Cara menguranginya sama: kewajiban uji
otomatis untuk setiap perintah klinis, sesuai `RWI-DEC-051`.

Keadaan hari ini memperbesar risiko itu, dan perlu dinyatakan terang-terangan: bukti
`DOK-TRC-VER-01` menunjukkan **tidak ditemukan satu pun uji otomatis** untuk konsultasi,
pengkajian, CPPT, tindakan, resep, atau radiologi pada jalur rawat inap. Dua puluh enam uji
fondasi yang lulus hanya menyentuh episode, penugasan, pendaftaran layanan, dan disiplin
laboratorium.

---

## W. Model audit dan histori

### W.1 Perubahan yang wajib punya jejak tahan lama

| Kejadian | Yang wajib tersimpan | Disimpan di mana |
| --- | --- | --- |
| Catatan dokter dibuat, difinalkan, dibatalkan | Penulis, profesi, waktu klinis, waktu simpan, status, alasan pembatalan | `CON-EXT-011` ditambah `CON-EXT-021` |
| Koreksi dokumen final | Nomor urut addendum, penulis, penulis pengganti bila ada, alasan koreksi, waktu tanda tangan | `CON-EXT-022` |
| CPPT ditulis | Profesi, penulis, waktu klinis, isi | `CON-EXT-012` |
| CPPT diverifikasi | Verifikator, waktu verifikasi, keterlambatan bila ada | `CON-EXT-012` |
| Kajian medis dibuat dan diselesaikan | Penulis, waktu mulai, waktu selesai, keterlambatan bila ada | `CON-EXT-013` |
| **Event visite dicatat** | Episode, kunjungan, pasien, dokter, peran, waktu visite, pencatat, kunci permintaan | `CON-EXT-015` |
| **Event visite dibatalkan** | Pembatal, waktu, **alasan** | `CON-EXT-015` |
| Resep ditulis dan jenisnya | Penulis, waktu, jenis resep, status pemenuhan yang dibaca balik | `CON-EXT-017` |
| Tindakan direncanakan dan dikerjakan | Dokter, waktu rencana, waktu pelaksanaan, pelaksana | `CON-EXT-014` |
| Fakta klinis diterbitkan ke Billing | Identitas fakta, versi, kunci idempotency, korelasi, hasil penerbitan | `CON-EXT-016` |
| Pesanan penunjang dan pengesahan hasil | Pemesan, waktu, pengesah hasil | Milik `CTX-LAB` dan `CTX-RAD` |

### W.2 Tiga sifat yang wajib dipenuhi

Sifat pada bagian I.2 berlaku penuh, dengan satu tambahan khusus scope klinis.

| Sifat | Isinya | Dasar |
| --- | --- | --- |
| **Ditulis bersamaan** | Jejak ditulis dalam transaksi yang sama dengan perubahan yang dijejakinya | `RWI-RULE-031` aturan 3 |
| **Satu pintu** | Perubahan status dokumen lewat satu titik, tidak ada jalur yang menyetel status langsung | `RWI-RULE-031` aturan 4 |
| **Tidak dapat diubah** | Jejak tidak dapat disunting dan tidak dapat dihapus; koreksi dilakukan dengan menambah baris baru | `RWI-RULE-031` aturan 5 |
| **Isi medis tidak masuk catatan aktivitas teknis** | Catatan aktivitas untuk pemantauan sistem **tidak boleh** memuat isi klinis pasien | Gerbang requirement dimensi 14 |

### W.3 Yang tidak dianggap jejak audit

Sama seperti bagian I.3: catatan aktivitas teknis bukan jejak audit domain. Ia tidak terikat
transaksi, tidak dapat disaring per episode, dan tidak dapat ditampilkan sebagai riwayat pasien.

Tambahan yang khusus berlaku di sini: **jumlah visite tidak boleh dihitung dari catatan aktivitas
teknis**, melainkan hanya dari event visite yang tidak dibatalkan. Menghitung dari sumber lain akan
melahirkan angka kedua yang berpotensi berselisih — persis yang dilarang `RWI-DEC-085`.

---

## X. Model integrasi

### X.1 Batas internal

| ID | Produsen | Konsumen | Tujuan bisnis | Sumber kebenaran | Arah | Sifat | Status pada source |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `INT-DOK-01` | `CTX-INP-CARE` | `CTX-CLI`, `CTX-PHM`, `CTX-LAB`, `CTX-RAD` | Konteks klinis episode sebagai pengganti antrean | `CTX-INP-CARE` | Baca | Sinkron | **`Missing`** |
| `INT-DOK-02` | `CTX-CLI`, `CTX-PHM` | Dirinya sendiri | Pelonggaran satu konsultasi per kunjungan dan satu resep aktif per konsultasi, terbatas `Inpatient` dan `Emergency` | Pemilik masing-masing | Perubahan aturan internal | Sinkron | **`Extend`** |
| `INT-DOK-03` | `CTX-CLI` | `CTX-BIL` | Fakta klinis tindakan sebagai dasar penagihan | `CTX-CLI` untuk fakta klinis; `CTX-BIL` untuk keputusan finansial | Tulis satu arah | Sinkron dengan kemampuan ulang | **Sudah ada** |
| `INT-DOK-04` | `CTX-LAB` | `CTX-INP-CARE` sebagai pembaca | Hasil laboratorium final terverifikasi | `CTX-LAB` | Baca | Sinkron | `Extend` |
| `INT-DOK-05` | `CTX-RAD` | `CTX-INP-CARE` sebagai pembaca | Hasil radiologi final terverifikasi | `CTX-RAD` | Baca | Sinkron | `Extend` |
| `INT-DOK-06` | `CTX-PHM` | `CTX-INP-CARE` sebagai pembaca | Status pemenuhan resep dan penyerahan obat pulang | `CTX-PHM` | Baca | Sinkron | `Extend` |
| `INT-DOK-07` | `CTX-MRC` | `CTX-CLI` | Penandatanganan, penguncian, dan addendum dokumen klinis | `CTX-MRC` | Dua arah dalam batas dokumen | Sinkron | **Sudah ada** |

**Arah yang tidak boleh dibalik.** `INT-DOK-03` berjalan **satu arah**: modul klinis menyatakan
peristiwa, Billing menerimanya. Tidak ada jalur balik yang mengizinkan Billing mengubah catatan
klinis. Ini penegasan `INV-DOK-09` pada tingkat integrasi.

### X.2 Kontrak yang sudah ada hari ini

Bagian ini menampilkan **kontrak yang benar-benar ada** pada `BE@93b3227`, memakai grup Swagger
apa adanya. Ini bukan rancangan endpoint baru: arsitektur domain tidak merancang endpoint.
Rancangan kontrak target adalah pekerjaan `design-business-module`.

#### Health Services / Inpatient Management / Inpatient Census

Base URL: `api/v1/health-services/inpatient-management/census`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Menampilkan daftar pasien yang sedang dirawat, dapat disaring pada dokter tertentu | `InpatientCensus : Read` | Query penyaring, termasuk dokter | Daftar pasien rawat inap di dalam `ApiResponse<T>` |
| `GET` | `/summary` | Ringkasan jumlah pasien dirawat | `InpatientCensus : Read` | Query penyaring | Ringkasan census |
| `GET` | `/filters/metadata` | Pilihan penyaring untuk layar daftar | `InpatientCensus : Read` | — | Daftar pilihan penyaring |

**Inilah sumber data yang benar untuk daftar pasien dokter**, dan inilah yang tidak dipakai ruang
kerja frontend hari ini.

#### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter baru | `DoctorConsultation : Create` | Data konsultasi, termasuk kunjungan dan antrean bila ada | Catatan yang tersimpan |
| `PATCH` | `/{id}/soap` | Menyimpan otomatis isi SOAP tanpa mengubah isian lain | `DoctorConsultation : Update` | Isi SOAP | Ringkasan hasil penyimpanan |
| `PATCH` | `/{id}/complete` | Memfinalkan catatan | `DoctorConsultation : Update` | — | Catatan berstatus final |
| `PATCH` | `/{id}/cancel` | Membatalkan catatan | `DoctorConsultation : Update` | Alasan | Catatan berstatus batal |
| `GET` | `/{id}/finalization-validation` | Memeriksa kelayakan sebelum difinalkan | `DoctorConsultation : Read` | — | Hasil pemeriksaan |
| `GET` | `/active-by-queue/{queueId}` | Mencari catatan aktif **berdasarkan antrean** | `DoctorConsultation : Read` | — | Catatan aktif |

**Baris terakhir adalah inti masalahnya.** Pencarian catatan aktif hari ini bertumpu pada antrean,
sedangkan pasien rawat inap tidak punya antrean. Padanan berbasis episode belum ada, dan itulah
`INT-DOK-01`.

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian pasien | `PatientAssessment : Create` | Data pengkajian | Pengkajian tersimpan |
| `GET` | `/active-by-encounter/{encounterId}` | Mencari pengkajian aktif berdasarkan kunjungan | `PatientAssessment : Read` | — | Pengkajian aktif |
| `PATCH` | `/{id}/complete` | Menyelesaikan pengkajian | `PatientAssessment : Update` | — | Pengkajian selesai |
| `PATCH` | `/{id}/cancel` | Membatalkan pengkajian | `PatientAssessment : Update` | Alasan | Pengkajian batal |

Pencarian berdasarkan kunjungan sudah tersedia — ini modal yang baik. Yang belum ada adalah
pembuktian bahwa kunjungan itu memang milik episode rawat inap yang sedang dibuka.

#### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menulis CPPT | `PatientIntegratedProgressNote : Create` | Isi catatan, profesi, penulis | Catatan tersimpan |
| `GET` | `/timeline` | Menampilkan linimasa catatan pasien | `PatientIntegratedProgressNote : Read` | Query penyaring | Linimasa catatan |
| `POST` | `/from-consultation/{consultationId}` | Menurunkan CPPT dari catatan dokter | `PatientIntegratedProgressNote : Create` | — | Catatan tersimpan |
| `PATCH` | `/{id}/cancel` | Membatalkan catatan | `PatientIntegratedProgressNote : Update` | Alasan | Catatan batal |

**Tidak ada satu pun endpoint verifikasi DPJP di sini.** Itulah kekurangan utama `CAP-021`.

#### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat rencana tindakan | `PatientProcedure : Create` | Data tindakan, tarif, kunjungan, konsultasi | Tindakan tersimpan |
| `PATCH` | `/{id}/approve` | Menyetujui tindakan | `PatientProcedure : Update` | — | Tindakan disetujui |
| `PATCH` | `/{id}/execute` | Menandai tindakan sudah dikerjakan, lalu menerbitkan fakta klinis ke Billing | `PatientProcedure : Update` | Waktu dan pelaksana | Tindakan terlaksana |

#### Health Services / Pharmacy Management / Prescription

Base URL: `api/v1/health-services/pharmacy-management/prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menulis resep | `Prescription : Create` | Data resep dan konsultasi asalnya | Resep tersimpan |
| `GET` | `/active-by-consultation/{consultationId}` | Mencari resep aktif pada satu konsultasi | `Prescription : Read` | — | Resep aktif |
| `GET` | `/` | Daftar resep | `Prescription : Read` | Query penyaring | Daftar resep |

**Dua hal yang terbaca dari tabel ini.** Pertama, resep dicari lewat konsultasi, bukan lewat
episode — sehingga selama konsultasi kedua ditolak, resep kedua ikut tertahan. Kedua, tidak ada
penanda jenis obat pulang di mana pun.

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan laboratorium | `LabOrder : Create` | Data pesanan dan kunjungan | Pesanan tersimpan |
| `GET` | `/` | Daftar pesanan laboratorium | `LabOrder : Read` | Query penyaring — **tanpa penyaring kunjungan** | Daftar pesanan |
| `PUT` | `/{id}/complete` | Menyelesaikan pemeriksaan | `LabOrder : Process` | — | Pesanan selesai |

#### Health Services / Radiology Management / Rad Order

Base URL: `api/v1/health-services/radiology-management/rad-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan radiologi | `RadOrder : Create` | Data pesanan, kunjungan, modalitas | Pesanan tersimpan |
| `GET` | `/` | Daftar pesanan radiologi, dapat disaring kunjungan | `RadOrder : Read` | Query penyaring termasuk kunjungan | Daftar pesanan |
| `PUT` | `/{id}/schedule` | Menjadwalkan pemeriksaan | `RadOrder : Schedule` | Jadwal | Pesanan terjadwal |
| `PUT` | `/{id}/complete` | Menyelesaikan pemeriksaan | `RadOrder : Process` | — | Pesanan selesai |

Arti kode status yang paling sering muncul, dalam bahasa pengguna:

| Kode | Artinya bagi pengguna |
| --- | --- |
| `200` / `201` | Permintaan berhasil; data tersimpan atau terbaca |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |
| `404` | Data yang dituju tidak ditemukan, misalnya kunjungan yang tidak ada |
| `409` | Bentrok aturan, misalnya catatan dokter untuk kunjungan itu sudah ada |
| `500` | Sistem gagal memproses. **Inilah yang terjadi hari ini pada jalur pasien tanpa antrean** |

### X.3 Kemampuan yang belum ada — `Rencana (belum tersedia)`

| Kemampuan target | Keadaan | Pemilik yang mengerjakan |
| --- | --- | --- |
| Pencarian catatan dokter aktif berdasarkan **episode**, bukan antrean | `Rencana (belum tersedia)` | `ClinicalManagement` |
| Verifikasi CPPT oleh DPJP beserta daftar catatan yang terlambat diverifikasi | `Rencana (belum tersedia)` | `ClinicalManagement` |
| Pencatatan, pembatalan, dan riwayat **event visite** | `Rencana (belum tersedia)` — tidak ada model, migration, endpoint, hak akses, konsumen, maupun uji | `ClinicalManagement` |
| Penanda jenis resep obat pulang dan pembacaan status penyerahannya | `Rencana (belum tersedia)` | `PharmacyManagement` |
| Penyaring kunjungan pada daftar pesanan laboratorium | `Rencana (belum tersedia)` | `LaboratoryManagement` |
| Pembacaan hasil final terverifikasi untuk ruang kerja dokter | `Rencana (belum tersedia)` | `LaboratoryManagement` dan `RadiologyManagement` |
| Sumber daya dan aksi hak akses untuk visite | `Rencana (belum tersedia)` | Platform bersama pemilik capability |

Path, bentuk permintaan, dan bentuk jawaban untuk seluruh baris di atas **sengaja tidak ditulis di
sini**. Menuliskannya berarti merancang endpoint, dan itu di luar wewenang arsitektur domain.

### X.4 Kemampuan mengulang, kegagalan, dan rekonsiliasi

| Kepedulian | Ketetapannya | Dasar |
| --- | --- | --- |
| Kiriman ulang pencatatan visite | Kunci permintaan yang sama mengembalikan event yang sama | `INV-DOK-06`, `RWI-AC-152` |
| Kiriman ulang fakta klinis ke Billing | Kunci idempotency dibangun dari identitas dan versi fakta; kiriman identik dijawab dengan hasil yang sama | `ClinicalMilestoneFactProducer` |
| Billing gagal dihubungi | Catatan klinis tetap tersimpan; penerbitan fakta masuk daftar percobaan ulang | Gerbang requirement dimensi 16 |
| Pembatalan klinis setelah fakta terkirim | Bila belum ada tagihan terbentuk, koreksi ditekan; bila keadaan sebelumnya tidak diketahui, **wajib rekonsiliasi lebih dulu** | Bentuk hasil penerbitan fakta yang sudah ada |
| Penunjang dan Farmasi tidak menjawab | Ruang kerja menampilkan keadaan apa adanya, tidak menebak, dan tidak menyalin hasil ke sisi Rawat Inap | `INV-DOK-12`, bagian R.4 |
| Pemberitahuan otomatis kepada pengguna | **Tidak dirancang.** Requirement hanya mewajibkan daftar pantau dan daftar percobaan ulang | `RWI-DOK-RQG-001` |

### X.5 Kejadian bisnis yang layak diumumkan

Daftar ini adalah **fakta bisnis**, bukan rancangan mekanisme pengiriman pesan. Sama seperti J.3,
cara mewujudkannya diserahkan ke `design-business-module`.

| ID | Kejadian | Kapan terjadi | Siapa yang mungkin peduli |
| --- | --- | --- | --- |
| `EVT-DOK-01` | Kajian medis awal selesai | Kajian berstatus selesai | Daftar pantau kepatuhan, DPJP |
| `EVT-DOK-02` | Catatan dokter difinalkan | Catatan berstatus final | Linimasa pasien, profesi lain |
| `EVT-DOK-03` | Catatan dokter dikoreksi lewat addendum | Addendum ditandatangani | Rekam medis, auditor |
| `EVT-DOK-04` | CPPT ditulis | Catatan tersimpan | DPJP yang harus memverifikasi |
| `EVT-DOK-05` | CPPT diverifikasi | Verifikasi tersimpan | Daftar pantau kepatuhan |
| `EVT-DOK-06` | CPPT melewati batas verifikasi | Saat batas terlampaui | Daftar pantau, laporan akreditasi |
| `EVT-DOK-07` | Resep dikirim ke Farmasi | Resep tersimpan | Farmasi |
| `EVT-DOK-08` | Obat pulang diserahkan | Status penyerahan terbaca | Daftar periksa administrasi, admisi |
| `EVT-DOK-09` | Tindakan dikerjakan | Penandaan pelaksanaan | Billing lewat fakta klinis, rekam medis |
| `EVT-DOK-10` | Visite dokter dicatat | Event `Recorded` | Riwayat klinis, laporan operasional, Billing bila kelak diputuskan |
| `EVT-DOK-11` | Visite dokter dibatalkan | Event `Cancelled` | Riwayat klinis, laporan, Billing bila fakta terkait pernah dikirim |
| `EVT-DOK-12` | Pesanan penunjang dibuat | Pesanan tersimpan | Laboratorium, Radiologi |
| `EVT-DOK-13` | Hasil penunjang final tersedia | Hasil disahkan | Ruang kerja dokter |

### X.6 Batas eksternal

**Tidak ada satu pun batas eksternal yang dirancang pada amendment ini.** Alasannya sama dengan
J.2: `DEC-INP-005` tentang interoperabilitas belum terjawab, sehingga merancang kontraknya berarti
mengarang kebijakan.

Yang sudah disiapkan supaya keputusan itu tidak mahal kelak: seluruh bahan yang biasanya dituntut
pengiriman rekam medis sudah tersimpan dalam bentuk yang dapat dibaca ulang — catatan dokter, CPPT
beserta verifikasinya, kajian medis, tindakan, resep, hasil penunjang, dan **riwayat event visite
beserta waktunya**.

---

## Y. Dampak billing

**Klasifikasi: berdampak pada charge; sebagian dependency billing belum terselesaikan.**

### Y.1 Yang sudah pasti

| Hal | Ketetapannya | Dasar |
| --- | --- | --- |
| Tindakan dokter yang dikerjakan menghasilkan fakta klinis untuk Billing | Sudah berjalan hari ini, memakai kunci idempotency dari identitas dan versi fakta | `ClinicalMilestoneFactProducer`, `DOK-TRC-CAP024` |
| Modul klinis **tidak** menghitung nominal, tidak menetapkan status pembayaran, dan tidak mengambil keputusan finansial | Fakta klinis hanya menyatakan bahwa peristiwa klinis benar terjadi | Bentuk `ClinicalMilestoneFactRequest` |
| Kegagalan Billing tidak menghapus catatan klinis | Catatan disimpan lebih dulu, fakta dikirim sesudahnya | Gerbang requirement dimensi 16 |
| Agregasi tagihan **tidak boleh** mengubah riwayat klinis | Dua event visite tetap dua baris riwayat walaupun ditagihkan sebagai satu | `RWI-DEC-085`, `RWI-AC-156`, `INV-DOK-09` |
| Resep dan obat pulang mengikuti aturan Farmasi dan Billing | Rawat Inap hanya membaca status | `RWI-RULE-024` |

### Y.2 Kenapa pemisahan hitungan visite ini penting

Ini bagian yang paling mudah salah dipahami, jadi perlu contoh berangka.

> **12 September 2026.** dr. Andi visite Tn. Budi pukul **07:40**, lalu kembali pukul **16:10**
> karena kondisi pasien memburuk. Keduanya dicatat sebagai event.
>
> | Sudut pandang | Angkanya | Alasannya |
> | --- | ---: | --- |
> | Riwayat klinis dan laporan operasional | **2** | Dua kunjungan nyata benar-benar terjadi |
> | Tagihan, bila kontrak penjamin hanya membayar satu visite per dokter per tanggal | **1** | Aturan pembayaran milik penjamin, bukan aturan pencatatan medis |
>
> Kedua angka itu **benar pada tempatnya masing-masing**, dan tidak boleh saling menimpa. Yang
> dilarang adalah menghapus salah satu event agar angka klinis menjadi 1, atau menagih dua kali
> karena angka klinis 2.

Sebelum `RWI-DEC-085`, kedua angka itu dipaksa sama — sistem menyimpan satu visite per dokter per
tanggal, sehingga kunjungan sore hari **hilang dari riwayat medis** demi kecocokan dengan aturan
pembayaran. Itulah yang diperbaiki keputusan ini.

### Y.3 Yang belum ada dan akibatnya

| Hal | Keadaan | Akibat |
| --- | --- | --- |
| Kebijakan tarif visite dan pemicu penagihannya | **Belum ada.** Milik pemilik Billing | Amendment ini **tidak** merancang jalur penagihan visite. Event klinis tetap dicatat lengkap sehingga penagihan dapat disusun kapan pun kebijakannya turun |
| Kemampuan transaksi `BillingManagement` untuk rawat inap | Belum operasional, sebagaimana K.2 | Charge kamar dan sebagian charge klinis belum tercatat selama MVP |
| Kontrak status final Farmasi, Laboratorium, dan Radiologi yang disetujui pemiliknya | Belum disetujui | `RWI-DOK-RQG-003`; menahan produksi, bukan menahan desain domain |

Yang dilakukan arsitektur ini sama dengan pass pertama: **menjamin datanya dapat direkonstruksi**.
Setiap event visite menyimpan episode, dokter, peran, dan waktu, sehingga aturan agregasi apa pun
yang kelak disetujui dapat dijalankan mundur tanpa satu pun informasi yang hilang.

---

## Z. Dampak keselamatan klinis

**Klasifikasi: relevan terhadap keselamatan.**

Scope ini menyentuh rekam medis, resep, tindakan, dan hasil pemeriksaan. Kesalahan di sini tidak
berhenti sebagai kesalahan data; ia menjadi kesalahan pengobatan.

### Z.1 Titik yang relevan terhadap keselamatan

| Titik | Kenapa relevan | Bagaimana arsitektur membuat batasnya jelas |
| --- | --- | --- |
| Dokumen menempel pada pasien atau episode yang salah | Salah pasien adalah kesalahan medis paling berat | `INV-DOK-01` dan `INV-DOK-02` mewajibkan kecocokan pasien, kunjungan, dan episode dibuktikan pada **setiap** penulisan |
| Daftar pasien menampilkan pasien yang bukan pasien rawat inap | Dokter dapat menulis pada rekam medis orang lain sambil merasa benar | Daftar wajib berasal dari census episode. Keadaan hari ini `Conflict` dan menahan rilis — `DOK-TRC-FE-01` |
| Dokumentasi gagal disimpan pada jalur pasien menginap | Bila catatan tidak dapat disimpan, dokter kembali menulis di kertas dan riwayat digital menjadi bolong | Perbaikan jalur tanpa antrean dinyatakan sebagai syarat, bukan perbaikan opsional — `DOK-TRC-DEF-01` |
| Dokter yang bukan penanggung jawab menulis atau memverifikasi | Verifikasi kehilangan makna klinis | `INV-DOK-11` dan `INV-DOK-13` |
| Hasil penunjang yang belum final dibaca sebagai hasil sah | Keputusan terapi diambil dari angka yang masih berubah | `INV-DOK-12` |
| Riwayat visite yang tidak lengkap | Kunjungan sore yang hilang membuat perburukan pasien tidak terbaca saat ditelusuri | `INV-DOK-07` dan `INV-DOK-08`, ditambah pemisahan dari agregasi tagihan pada bagian Y |
| Koreksi yang menimpa isi lama | Rekam medis kehilangan kemampuan dipertanggungjawabkan | `INV-DOK-10`: koreksi lewat addendum, pembatalan tetap terlihat |
| Obat pulang tidak terpantau | Pasien pulang tanpa obat yang seharusnya dibawa | Butir daftar periksa administrasi pada `RWI-RULE-018`, dengan penanda manual sebagai jalan keluar |

### Z.2 Keputusan keselamatan yang belum terselesaikan

| Butir | Sifat | Kenapa tidak diselesaikan di sini |
| --- | --- | --- |
| Nilai batas waktu kajian medis dan verifikasi CPPT | Menahan **produksi**, tidak menahan desain | `RWI-RULE-021` belum `approved`; pemilik klinisnya belum ditunjuk. Arsitektur menyediakan parameter, bukan angka |
| Kebijakan pencatatan visite atas nama dokter | Menahan kemampuan itu saja | Belum ada kebijakan eksplisit. `RWI-RULE-017` current menyatakan kemampuan itu tidak tersedia |
| Aturan isolasi dan pemisahan jenis kelamin (`DEC-INP-004`) | Tetap terbuka | Berada di slice lain dan tidak dinilai ulang di sini. Peringatan L.3 tetap berlaku |
| Serah terima klinis antar shift (`DEC-INP-006`) | Tetap terbuka | Belum pernah dibahas |

### Z.3 Satu hal yang perlu dinyatakan terus terang

Tujuh kemampuan ini **belum punya satu pun jaring pengaman otomatis**. Bukti `DOK-TRC-VER-01`
menunjukkan tidak ditemukan uji untuk konsultasi, pengkajian, CPPT, tindakan, resep, radiologi
rawat inap, maupun ruang kerja dokter di frontend.

Artinya, bila kemampuan ini dibangun dalam keadaan sekarang, tidak ada yang memberi tahu bila
pelonggaran aturan rawat inap ikut merusak alur poliklinik yang sudah melayani pasien. Risiko itu
sudah tercatat sebagai `RWI-RISK-002`, dan kewajiban uji regresi sudah dikunci `RWI-DEC-051`.

Arsitektur tidak dapat menutup risiko ini sendiri; yang dapat dilakukannya adalah menyatakannya
sebagai syarat kelayakan produksi, dan itulah yang dilakukan bagian AB.4.

---

## AA. Gap arsitektur

| ID | Gap | Sifat | Dampak | Diarahkan ke |
| --- | --- | --- | --- | --- |
| `ARCH-GAP-008` | Konteks klinis episode (`INT-DOK-01`) belum ada pada source | **Tidak memblokir desain domain**; memblokir implementasi | Selama belum ada, dokumen klinis pasien menginap tidak dapat disimpan sama sekali | `design-business-module` lalu delivery, dikerjakan `ClinicalManagement` |
| `ARCH-GAP-009` | Jalur konsultasi tanpa antrean membaca data antrean yang kosong lalu tetap menulis ke dalamnya | **Tidak memblokir desain**; **memblokir rilis** | Kegagalan sistem (kode 500) pada jalur yang justru dipakai pasien rawat inap dan IGD | Perbaikan pada `ClinicalManagement`, disertai uji regresi IGD |
| `ARCH-GAP-010` | Nilai batas waktu kajian medis dan verifikasi CPPT belum disetujui pemilik klinis | Tidak memblokir desain; **menahan produksi** | Daftar pantau kepatuhan berjalan tanpa angka sampai nilainya ditetapkan | `grill-me` setelah pemilik klinis ditunjuk |
| `ARCH-GAP-011` | Bentuk penyimpanan kajian medis awal belum ditetapkan | Tidak memblokir | Model domainnya sama untuk kedua pilihan; hanya bentuk penyimpanannya berbeda | `design-business-module`, tercatat sebagai `RWI-DOK-TRQ-001` |
| `ARCH-GAP-012` | Kebijakan agregasi tarif visite milik Billing belum ada | Tidak memblokir | Event klinis tetap tercatat lengkap; penagihan visite belum dapat dirancang | Pemilik `BillingManagement` |
| `ARCH-GAP-013` | Ruang kerja dokter di frontend memakai kontrak antrean rawat jalan | Tidak memblokir desain domain; **memblokir sign-off dan rilis** | Berpotensi menampilkan pasien rawat jalan sebagai pasien rawat inap dan mengirim aksi antrean yang salah | `design-business-module` lalu delivery |
| `ARCH-GAP-014` | Daftar pesanan laboratorium belum dapat disaring per kunjungan, dan pembacaan hasil final terverifikasi belum ditemukan | Tidak memblokir desain | Tanpa keduanya, `INV-DOK-12` tidak dapat ditegakkan | `LaboratoryManagement` dan `RadiologyManagement` |
| `ARCH-GAP-015` | Kontrak status final Farmasi, Laboratorium, dan Radiologi belum disetujui pemilik masing-masing | Tidak memblokir desain; **menahan produksi** | Bentuk minimumnya sudah jelas; yang kurang adalah persetujuan | Pemilik modul terkait, `RWI-DOK-RQG-003` |
| `ARCH-GAP-016` | Belum ada satu pun uji otomatis untuk jalur klinis dokter rawat inap | Tidak memblokir desain; **menahan klaim kesiapan** | Perubahan aturan berisiko merusak alur poliklinik tanpa ada yang memberi tahu | Delivery, sesuai `RWI-DEC-051` |
| `ARCH-GAP-017` | Observasi di luar scope: model episode pada source sudah memiliki rujukan episode ibu untuk bayi | Tidak memblokir; **hanya catatan** | `ARCH-GAP-002` pada bagian M mungkin sudah tidak akurat, tetapi slice itu tidak dipindai ulang pada amendment ini | Impact scan berikutnya untuk `episode-rawat-inap` |

Tidak satu pun gap di atas memaksa pengarangan kebijakan bisnis. Seluruhnya dicatat apa adanya.

---

## AB. Kesiapan arsitektur

### AB.1 Status

**`DOMAIN_ARCHITECTURE_READY`** untuk scope amendment ini, yaitu `CAP-015` dan `CAP-020` s.d.
`CAP-025`.

Dokumen ini secara keseluruhan tetap **`DOMAIN_ARCHITECTURE_PARTIAL`**, karena slice `INP-S09`,
`INP-S10`, `INP-S11`, `INP-S15`, serah terima klinis antar shift, dan dua cara pulang meninggal
serta kabur belum dirancang dan tidak dinilai ulang di sini.

### AB.2 Pemeriksaan terhadap syarat `DOMAIN_ARCHITECTURE_READY`

| Syarat kesiapan | Terpenuhi | Keterangan |
| --- | :---: | --- |
| Slice requirement memang layak untuk domain design | Ya | Ketujuh capability `READY_FOR_DOMAIN_DESIGN` pada gerbang requirement revision `1.3` bagian 12.3 |
| Bounded context dapat dipertahankan | Ya | Enam context milik modul lain dinyatakan hubungannya secara eksplisit pada bagian Q, dan tidak satu pun konsep klinis diambil alih Rawat Inap |
| Ownership yang material sudah terselesaikan | Ya | `RWI-DEC-081` mengunci kepemilikan; `RWI-DEC-062` memberi persetujuan pemilik. Satu-satunya ambiguitas yang tersisa adalah **bentuk penyimpanan** kajian medis, dan itu bukan ambiguitas ownership |
| Lifecycle dan invariant penting sudah terwakili | Ya | Tiga belas invariant `INV-DOK-01` s.d. `INV-DOK-13`, lifecycle tujuh konsep, dan lima invariant yang tidak dapat dijaga satu aggregate dinyatakan terbuka pada S.9 |
| Keputusan bisnis pemblokir sudah terselesaikan | Ya | `DEC-INP-008` `CLOSED` oleh `RWI-DEC-084` dan `RWI-DEC-085`. Tidak ada Decision ID bisnis berstatus `BLOCKING` yang tersisa pada scope ini |
| Konsekuensi billing yang material sudah eksplisit | Ya | Bagian Y, termasuk pemisahan angka klinis dari angka tagihan beserta contoh berangka |
| Konsekuensi keselamatan klinis sudah eksplisit | Ya | Bagian Z, termasuk pernyataan terus terang pada Z.3 tentang tidak adanya jaring pengaman otomatis |
| Dapat diserahkan ke penyusunan blueprint final | Ya | Handoff pada AB.5 |

### AB.3 Apa yang boleh berjalan sekarang

| Boleh berjalan | Keterangan |
| --- | --- |
| `design-business-module` amendment untuk ketujuh capability | Menghasilkan arsitektur backend dan frontend, kamus data, kontrak API, state transition, validation, permission dan audit, strategi test, serta PRD ke MVP |
| Penyerapan keberadaan Radiologi ke seluruh artefak dokter | Sudah menjadi fakta arsitektur pada bagian Q dan S.7 |
| Perancangan bentuk ruang kerja dokter berbasis census episode | Bentuk targetnya sudah dinyatakan pada U.2 |

### AB.4 Apa yang tetap harus berhenti

| Harus berhenti | Sampai kapan |
| --- | --- |
| Sign-off dan rilis ruang kerja dokter yang ada sekarang | Sampai sumber datanya berpindah ke census episode dan seluruh semantik antrean dilepas — `ARCH-GAP-013` |
| Klaim bahwa kemampuan dokter rawat inap siap dipakai | Sampai konteks klinis episode ada, jalur tanpa antrean diperbaiki, dan uji otomatis tersedia — `ARCH-GAP-008`, `009`, `016` |
| Penetapan angka batas waktu kajian medis dan verifikasi CPPT | Sampai pemilik klinis ditunjuk dan menyetujuinya — `ARCH-GAP-010` |
| Perancangan penagihan visite | Sampai pemilik Billing menetapkan kebijakannya — `ARCH-GAP-012` |
| Perancangan pencatatan visite atas nama dokter | Sampai ada kebijakan eksplisit yang menyetujuinya |
| Slice keperawatan pada `INP-S05` dan slice `INP-S09` s.d. `INP-S15` | Sampai masing-masing melewati gerbang requirement dan amendment arsitekturnya sendiri |

### AB.5 Handoff ke `design-business-module`

| Field | Nilai |
| --- | --- |
| Modul dan kemampuan | `InPatientManagement` / sub-modul `dokter-rawat-inap`; `CAP-015`, `CAP-020` s.d. `CAP-025` |
| Kesiapan requirement | `READY_FOR_DOMAIN_DESIGN`, gerbang revision `1.3` |
| Kesiapan arsitektur | **`DOMAIN_ARCHITECTURE_READY`** untuk scope ini; dokumen keseluruhan `DOMAIN_ARCHITECTURE_PARTIAL` |
| Architecture revision | `0.2`, status `draft` |
| Klasifikasi bukti | Keputusan pemilik `RWI-DEC-080` s.d. `RWI-DEC-085`; bukti implementasi capability map revision `1.3` bagian 15; baseline `REFERENCE_ONLY` tanpa observasi baru |
| Decision ID yang sudah tertutup | `DEC-INP-001`, `DEC-INP-008`; `RWI-OQ-032`, `RWI-OQ-049` |
| Decision ID yang belum selesai di luar scope | `DEC-INP-002` s.d. `DEC-INP-007` |
| Gap arsitektur | `ARCH-GAP-008` s.d. `ARCH-GAP-017` |
| Source SHA | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| Jejak requirement ke domain | Setiap konsep pada bagian R menyebut bukti asalnya; setiap invariant pada S.0 menyebut aturan bisnisnya; setiap proses pada bagian U menyebut acceptance criteria yang mengujinya |
| Acceptance yang wajib terbawa | `RWI-AC-143` untuk regresi rawat jalan; `RWI-AC-150` s.d. `RWI-AC-156` untuk visite |
| Keluaran hilir yang diharapkan | Arsitektur backend dan frontend, kamus data per bounded context, kontrak API, state transition, validation, permission dan audit, strategi test, serta PRD ke MVP — hanya untuk ketujuh capability ini |

### AB.6 Yang tidak boleh diubah diam-diam oleh blueprint hilir

Blueprint final boleh mempertajam kontrak implementasi, tetapi **tidak boleh** mengubah hal berikut
tanpa kembali ke skill hulu:

1. **Kepemilikan data pada bagian R.** Seluruh dokumentasi klinis, resep, tindakan, pesanan
   penunjang, dan event visite tetap milik modul masing-masing. Rawat Inap tetap nol tabel untuk
   scope ini.
2. **Kedudukan event visite sebagai kejadian mandiri.** Menurunkannya kembali dari SOAP atau CPPT
   berarti membatalkan `RWI-DEC-084`.
3. **Cara menghitung visite.** Setiap visite nyata satu hitungan; kiriman ulang dengan kunci yang
   sama tetap satu. Mengubahnya berarti membatalkan `RWI-DEC-085`.
4. **Pemisahan agregasi tagihan dari riwayat klinis.** Billing membaca, tidak menulis ulang.
5. **Tiga belas invariant pada S.0**, terutama `INV-DOK-01` sampai `INV-DOK-03` yang menjadi
   penjaga salah pasien dan salah episode.
6. **Larangan antrean semu.** Tidak ada bentuk apa pun yang membuat pasien menginap masuk ke daftar
   antrean poliklinik.
7. **Batas pelonggaran.** Hanya kunjungan `Inpatient` dan `Emergency`. Rawat jalan dan medical
   check-up tidak berubah sedikit pun.

### AB.7 Peringatan penutup

Amendment ini berstatus `draft`. Tidak ada satu pun bagiannya yang sudah disetujui manusia, dan
tidak ada satu baris source pun yang diubah olehnya.

Persetujuan pemilik yang berwenang tetap dibutuhkan. Untuk kepemilikan lintas modul, persetujuan
itu **sudah ada** lewat `RWI-DEC-062`. Yang belum ada adalah persetujuan atas dokumen arsitektur
ini sendiri, dan penunjukan pemilik klinis untuk `RWI-RULE-021` — tercatat sejak Scope Pass sebagai
`RWI-OQ-023` dan `RWI-DOK-TRQ-002`.
