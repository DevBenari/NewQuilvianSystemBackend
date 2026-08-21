# Rawat Inap — Hospital Domain Architecture

## A. Identitas arsitektur

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Architecture revision | `0.1` |
| Architecture status | `draft` — belum disetujui manusia |
| Tanggal | 21 Agustus 2026 (`Asia/Jakarta`) |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_PARTIAL`** |
| Kesiapan requirement masukan | `PARTIALLY_READY`, dari [`02-requirement-completeness-gate.md`](./02-requirement-completeness-gate.md) revision `1.0`, SHA-256 `cc32db172b2441b2967ce3507c89b81f12fc103bbd3b3a92bc7bc49d77005ffe` |
| Bukti bisnis | [`00-interview-decisions.md`](../00-interview-decisions.md) revision `2`, SHA-256 `1be53ca22ce811ed584135a49f6f51fc2499802b7604878aca7fe1024d3ae435` |
| Bukti keadaan saat ini | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.2` |
| Backend snapshot | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| Frontend snapshot | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| Baseline rujukan | `indonesia-hospital-domain-reference`, `references/inpatient.md`, seluruhnya `REFERENCE_ONLY` |
| Prefix modul | `Inp`, sesuai registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 20, lifecycle `PLANNED` |
| Batas tulis | Hanya dokumen ini. Tidak ada schema, migration, endpoint, UI, task, atau source aplikasi yang dibuat |

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
| `INP-S05` dokumentasi klinis dan visite | Dua alternatifnya menghasilkan model domain yang berbeda total | `DEC-INP-001` |
| `INP-S06` resep dan obat pulang | Bergantung pada keputusan yang sama | `DEC-INP-001` |
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
| `CTX-CLI` | Dokumentasi Klinis | `ClinicalManagement` | **Belum ditentukan** | Di luar scope arsitektur ini, lihat `DEC-INP-001` |
| `CTX-PHM` | Farmasi | `PharmacyManagement` | **Belum ditentukan** | Di luar scope, lihat `DEC-INP-001` |
| `CTX-EMG` | Instalasi Gawat Darurat | `EmergencyInstallationManagement` | **Belum ditentukan** | Di luar scope, lihat `DEC-INP-002` |
| `CTX-BIL` | Billing | `BillingManagement` | Tidak dipakai pada MVP | Digantikan penandaan manual, lihat `RWI-RULE-028` |

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
