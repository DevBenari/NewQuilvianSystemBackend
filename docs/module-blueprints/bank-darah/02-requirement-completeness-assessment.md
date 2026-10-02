# Bank Darah — Requirement Completeness Assessment

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Blueprint revision | `3` |
| Assessment revision | `2` |
| Modul | Bank Darah (`bank-darah`) |
| Tanggal penilaian | `2026-09-02` — dinilai ulang setelah closure pass wawancara |
| Backend SHA | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Kesiapan modul | **`PARTIALLY_READY`** |
| Mode | Read-only terhadap source aplikasi. Tidak ada entity, ERD, kontrak API, migration, UI, atau task implementasi yang dibuat di sini. |

Gerbang ini menjawab satu pertanyaan: **apakah kebutuhan bisnisnya sudah cukup lengkap dan berbukti
untuk mulai merancang arsitektur target?** Jawabannya dinilai per potongan kemampuan, bukan per
modul.

**Perubahan pada revisi 2.** Closure pass wawancara pada 2026-09-02 menutup sepuluh keputusan
pemblokir. Potongan yang siap dirancang naik dari empat menjadi **delapan dari sepuluh**. Dua yang
tersisa menunggu satu persetujuan lintas modul dan satu keputusan klinis, dan keduanya hanya menahan
bagian yang bergantung padanya.

---

## 1. Scope penilaian

Modul dipecah menjadi sepuluh potongan kemampuan terkecil yang masih bermakna dan dapat dipikirkan
sendiri-sendiri.

| Slice | Nama | Kebutuhan yang dicakup |
| --- | --- | --- |
| `BD-SLICE-01` | Order darah pasien | BR-BD-001, BR-BD-002, BR-BD-003, BR-BD-010, BR-BD-019 |
| `BD-SLICE-02` | Permintaan darah ke PMI | BR-BD-017 |
| `BD-SLICE-03` | Penerimaan fisik kantong | BR-BD-018 |
| `BD-SLICE-04` | Alokasi kantong ke pasien | BR-BD-005, BR-BD-006 |
| `BD-SLICE-05` | Pemberian darah dan pemenuhan sebagian | BR-BD-007, BR-BD-008 |
| `BD-SLICE-06` | Kedaluwarsa order dan kantong menunggu keputusan | Turunan `DEC-BD-006` dan `DEC-BD-007` |
| `BD-SLICE-07` | Pengembalian dan pemakaian ulang kantong | BR-BD-009 |
| `BD-SLICE-08` | Tindakan Bank Darah dan dampak biayanya | BR-BD-004 |
| `BD-SLICE-09` | Label golongan darah | BR-BD-011 |
| `BD-SLICE-10` | Sampling, keterkaitan Laboratorium, HCLAB, laporan, setup | BR-BD-012 sampai BR-BD-016 |

## 2. Bukti yang dipakai

| Urutan wewenang | Bukti | Keterangan |
| --- | --- | --- |
| 1. Requirement eksplisit dari user | `00-interview-decisions.md` revisi 2 — `SCOPE-BD-001`, `DEC-BD-001` sampai `DEC-BD-024` | Dikonfirmasi langsung oleh pemilik kebutuhan pada sesi scope pass dan closure pass, 2 September 2026 |
| 4. Bukti bisnis dari analis | `BUSINESS REQUIREMENTS DOCUMENT (BRD).md`, `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | Baseline SHA `8b298bb`. Belum diverifikasi terhadap berkas evidence aslinya karena `BD-DEP-009` |
| 6. Bukti implementasi V2 | `02-existing-capability-map.md` revisi 2 | Audit read-only terhadap backend `9522caa` dan frontend `afbb8ab` |
| — | `01-prerequisite-readiness.md` revisi 3 | Status 15 dependency; tidak ada lagi yang berstatus `CONFLICT` |

Baseline rumah sakit Indonesia (`indonesia-hospital-domain-reference`) **tidak dipakai** pada
penilaian ini, sehingga tidak ada `baseline_observation_ids` maupun `baseline_source_ids` yang
dibawa ke tahap berikutnya.

**Catatan wewenang bukti.** Tiga berkas evidence yang dirujuk BRD tidak ada di repository
(`BD-DEP-009`). Akibatnya BRD dan PRD hanya dapat diperlakukan sebagai bukti analis, bukan sebagai
requirement rumah sakit yang tersahkan. Yang menaikkan derajat bukti adalah jawaban langsung pemilik
kebutuhan pada sesi wawancara.

---

## 3. Temuan kelengkapan per dimensi

### 3.1 Rangkaian inti — `BD-SLICE-01` sampai `BD-SLICE-04`

| ID | Dimensi | Status | Bukti atau catatan |
| --- | --- | --- | --- |
| 01 | Tujuan | `CONFIRMED` | Memenuhi kebutuhan darah pasien secara tertelusur. BG-BD-001 sampai BG-BD-007 |
| 02 | Aktor | `CONFIRMED` | `DEC-BD-004` dan `DEC-BD-009` membagi peran unit pelayanan, petugas BDRS, dan Dokter BDRS |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Pasien terdaftar, kunjungan aktif, unit pelayanan berwenang (`DEC-BD-012`) |
| 04 | Alur utama | `CONFIRMED` | Order, permintaan ke PMI, penerimaan fisik, alokasi — `DEC-BD-002` sampai `DEC-BD-004` |
| 05 | Alur alternatif / exception | `CONFIRMED` | Kiriman kurang, belum dikirim, order ganda — `DEC-BD-005` dan `DEC-BD-008` |
| 06 | Data minimum | `CONFIRMED` sebagian | `DEC-BD-011` menetapkan komponen, jumlah, golongan yang diminta. `DEC-BD-004` menetapkan pasien, kunjungan, dokter peminta, unit asal, pelaku input. Katalog komponen darahnya ditetapkan sebagai data induk Setup oleh `DEC-BD-024` — lihat `GAP-BD-001` |
| 07 | Aturan bisnis / validation | `CONFIRMED` | `DEC-BD-005` deteksi ganda; `DEC-BD-009` daftar validasi wajib; `DEC-BD-003` larangan stok bebas |
| 08 | Status / perubahan status | `CONFIRMED` | Empat tabel perpindahan status pada `00-interview-decisions.md` §5, termasuk jalur kedaluwarsa setelah `DEC-BD-014` |
| 09 | Peran / authorization | `CONFIRMED` bentuknya | `DEC-BD-009` menetapkan tidak ada gerbang persetujuan; `DEC-BD-012` menetapkan kewenangan unit lewat konfigurasi; pola teknisnya `Ready to reuse` pada `BD-CAP-013`. Pemetaan kode izin per tindakan masih `PROPOSED` — lihat `GAP-BD-002` |
| 10 | Dependency antarmodul | `CONFIRMED` | Pasien, kunjungan, dokter, unit pelayanan, kelas — seluruhnya berbukti pada `BD-CAP-001`, `002`, `004`, `005`, `006` |
| 11 | Integrasi internal / eksternal | `CONFIRMED` | `DEC-BD-002` dan `DEC-BD-010`: PMI satu-satunya penyedia, pengiriman manual, tanpa API pada MVP |
| 12 | Hasil akhir | `CONFIRMED` | Order terpenuhi penuh, kantong berstatus diberikan, riwayat tersimpan |
| 13 | Pembatalan / koreksi | `CONFIRMED` untuk pembatalan order | BR-BD-010 melarang hapus keras; `IdentityModel` menyediakan pembatalan lunak (`BD-CAP-011`). Koreksi kantong dipisah ke `BD-SLICE-07` |
| 14 | Audit / histori | `CONFIRMED` | BG-BD-004; pola riwayat tambah-saja terbukti pada `BD-CAP-009` |
| 15 | Notifikasi | `MISSING` | Tidak dibahas sama sekali. Tidak material untuk struktur domain — lihat `GAP-BD-003` |
| 16 | Dampak billing | Tidak berlaku | Keempat slice ini tidak menimbulkan akibat finansial. Dampak biaya hanya ada pada `BD-SLICE-08` |
| 17 | Dampak keselamatan klinis | `CONFIRMED` batasannya | `INV-BD-011` melarang golongan darah yang belum tervalidasi dipakai untuk keputusan kesesuaian. Keputusan kesesuaian tetap di luar scope, tetapi `DEC-BD-013` mewajibkan buktinya tercatat sebelum pemberian |
| 18 | Pelaporan / traceability | `CONFIRMED` rantainya | BG-BD-001 menetapkan rantai penelusuran. Bentuk laporan dipisah ke `BD-SLICE-10` |

### 3.2 Enam slice sisanya, setelah closure pass

| Slice | Keadaan setelah closure pass | Status | Ringkasan |
| --- | --- | --- | --- |
| `BD-SLICE-05` | Tertutup | `CONFIRMED` | `DEC-BD-013` mewajibkan bukti kecocokan sebelum pemberian; `DEC-BD-017` menetapkan jalur darurat yang tercatat penuh |
| `BD-SLICE-06` | Tertutup | `CONFIRMED` | `DEC-BD-014` menetapkan sinyal penutupan per jenis kunjungan; `DEC-BD-020` menetapkan penutupan administratif permintaan PMI |
| `BD-SLICE-07` | Tertutup | `CONFIRMED` | `DEC-BD-019` menetapkan tiga pilihan akhir kantong menunggu keputusan |
| `BD-SLICE-08` | Sebagian | `CONFIRMED` aturan bisnisnya, `MISSING` persetujuannya | `DEC-BD-021` menetapkan biaya berasal dari tindakan, bukan kantong. Persetujuan pemilik Billing (`DEC-BD-016`) masih terbuka |
| `BD-SLICE-09` | Sebagian | `CONFIRMED` sumber datanya, `MISSING` mekanik labelnya | `DEC-BD-015` menetapkan sumber sah golongan darah. Isi label, syarat cetak, identifier, dan cetak ulang masih `OQ-BD-011` |
| `BD-SLICE-10` | Tertutup | `CONFIRMED` | `DEC-BD-018` sampling, `DEC-BD-015` batas Laboratorium, `DEC-BD-022` HCLAB, `DEC-BD-023` laporan, `DEC-BD-024` setup |

---

## 4. Daftar gap dan dampaknya

| Gap ID | Isi | Status bukti | Dampak | Slice terdampak |
| --- | --- | --- | --- | --- |
| `GAP-BD-001` | Katalog komponen darah belum ada. **Ditutup** `DEC-BD-024`: menjadi data induk pada Setup Bank Darah. Isi katalognya tetap konfigurasi | `CONFIRMED` | `CONFIGURABLE_DEFAULT` | `BD-SLICE-01`, `02`, `04` |
| `GAP-BD-002` | Pemetaan kelompok kewenangan BRD §14 ke kode izin per tindakan belum ditetapkan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Seluruh slice |
| `GAP-BD-003` | Kebutuhan pemberitahuan — misalnya memberi tahu unit pelayanan saat darah tiba — belum dibahas | `MISSING` | `NON_BLOCKING_STANDARD` | `BD-SLICE-02`, `03` |
| `GAP-BD-004` | Bukti pemeriksaan kecocokan sebelum pemberian. **Ditutup** `DEC-BD-013` dan `DEC-BD-017` | `CONFIRMED` | — | `BD-SLICE-05` |
| `GAP-BD-005` | Sinyal penutupan kunjungan. **Ditutup** `DEC-BD-014` | `CONFIRMED` | — | `BD-SLICE-06`, `BD-SLICE-01` |
| `GAP-BD-006` | Aturan pengembalian dan pemakaian ulang kantong. **Ditutup** `DEC-BD-019` | `CONFIRMED` | — | `BD-SLICE-07` |
| `GAP-BD-007` | Penutupan administratif permintaan PMI. **Ditutup** `DEC-BD-020` | `CONFIRMED` | — | `BD-SLICE-06`, `BD-SLICE-02` |
| `GAP-BD-008` | Aturan bisnis biaya **ditutup** `DEC-BD-021`. Tersisa persetujuan pemilik Billing atas penambahan konteks sumber pada `BillingSourceContract` | `MISSING` | **`BLOCKING`** | Penyerahan biaya pada `BD-SLICE-08` |
| `GAP-BD-009` | Sumber sah golongan darah. **Ditutup** `DEC-BD-015` | `CONFIRMED` | — | `BD-SLICE-09` |
| `GAP-BD-010` | Requirement sampling, Laboratorium, HCLAB, laporan, setup. **Ditutup** `DEC-BD-018`, `015`, `022`, `023`, `024` | `CONFIRMED` | — | `BD-SLICE-10` |
| `GAP-BD-013` | Isi label golongan darah, syarat pencetakan, identifier unik, dan perilaku cetak ulang belum ditetapkan | `MISSING` | **`BLOCKING`** | Bagian label pada `BD-SLICE-09` |
| `GAP-BD-014` | Peran pemakai jalur darurat dan peran validator hasil golongan darah belum ditetapkan | `MISSING` | `NON_BLOCKING_STANDARD` untuk rancangan, **`BLOCKING`** untuk implementasi | `BD-SLICE-05`, `BD-SLICE-09` |
| `GAP-BD-015` | Apakah semua komponen darah menuntut bukti kecocokan yang sama | `MISSING` | `NON_BLOCKING_STANDARD` untuk rancangan | `BD-SLICE-05` |
| `GAP-BD-016` | Apakah PMI menerima pengembalian kantong yang sudah keluar | `MISSING` | `NON_BLOCKING_STANDARD` | `BD-SLICE-07` |
| `GAP-BD-011` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada di repository | `MISSING` | `NON_BLOCKING_STANDARD` | Seluruh slice, pada tingkat penelusuran bukti |
| `GAP-BD-012` | Empat asumsi `ASM-BD-001` sampai `ASM-BD-004` belum dibenarkan pemilik | `PROPOSED` | `NON_BLOCKING_STANDARD` | `BD-SLICE-01`, `02`, `09` |

**Kenapa `GAP-BD-001` hanya `CONFIGURABLE_DEFAULT`.** Keputusan bahwa komponen darah harus berupa
katalog dan bukan ketikan bebas sudah tegas pada `DEC-BD-005`. Yang belum ada hanyalah isi
katalognya, dan isi katalog memang wajar berbeda antar rumah sakit. Ia dapat dimodelkan sebagai data
induk yang dapat dikonfigurasi tanpa menyembunyikan keputusan bisnis apa pun.
**Contoh:** MMC bisa saja hanya menangani PRC, TC, dan FFP hari ini, lalu menambah kriopresipitat
tahun depan. Penambahan itu cukup lewat data induk, bukan lewat perubahan rancangan.

---

## 5. Decision Log

### `DEC-BD-013`

**Pertanyaan.** Apakah sistem wajib menyimpan bukti bahwa pemeriksaan kecocokan darah sudah
dilakukan, sebelum kantong boleh ditandai diberikan kepada pasien?

**Kemampuan yang terdampak.** `BD-SLICE-05` pemberian darah.

**Bukti saat ini.** PRD FR-BD-006 hanya menyebut syarat administratif: alokasi sah, order benar,
kantong benar, kantong belum diberikan, pelaku berwenang. BRD §9 mengeluarkan mesin crossmatch dan
matriks kesesuaian klinis dari scope. Tidak ada satu pun bukti yang menyatakan apa yang harus
tercatat sebagai pengganti.

**Usulan baseline.** Mengeluarkan *mesin* crossmatch dari scope tidak sama dengan meniadakan
*catatan* bahwa pemeriksaan kecocokan sudah dilakukan di luar sistem. Usulan fail-closed: kantong
tidak dapat ditandai diberikan sebelum ada catatan bahwa pemeriksaan kecocokan dinyatakan selesai
oleh petugas berwenang, sekalipun pemeriksaannya sendiri dilakukan manual.

**Dampak.** Keselamatan pasien, keabsahan klinis catatan pemberian darah, lifecycle kantong, dan
penelusuran.

**Pemilik.** Pemilik proses klinis bersama pemilik proses BDRS.

**Status.** `CLOSED` pada 2026-09-02 — dijawab `DEC-BD-013` dan `DEC-BD-017`.

**Dampak implementasi / domain.** `BD-SLICE-05` kini siap dirancang penuh, termasuk jalur
daruratnya.

### `DEC-BD-014`

**Pertanyaan.** Untuk tiap jenis kunjungan, sinyal mana yang menandai kunjungan sudah ditutup bagi
Bank Darah?

**Kemampuan yang terdampak.** `BD-SLICE-06`, dan definisi "order aktif" pada `BD-SLICE-01`.

**Bukti saat ini.** `EncounterStatus` punya nilai akhir `Completed`, `Cancelled`, dan `NoShow`, tanpa
nilai bernama `Closed`. Rawat inap punya lapisan tersendiri: `InpEpisode` dengan `EpisodeStatus`,
`DischargeDecidedAt`, `PhysicallyLeftAt`, `ClosedAt`, dan `DischargeType`. Pasien rawat inap yang
sudah pulang belum tentu tercermin pada `EncounterStatus`.

**Usulan baseline.** Tetapkan pemetaan eksplisit per `EncounterType`, dan jawab satu pertanyaan
turunannya: apakah pasien rawat inap yang sudah `PhysicallyLeftAt` tetapi episodenya belum `ClosedAt`
sudah membuat order darahnya kedaluwarsa.

**Dampak.** Lifecycle order, lifecycle kantong, deteksi order ganda, dan integritas data.

**Pemilik.** Pemilik proses BDRS bersama pemilik Registration dan Inpatient.

**Status.** `CLOSED` pada 2026-09-02 — dijawab `DEC-BD-014`.

**Dampak implementasi / domain.** `BD-SLICE-06` siap dirancang. Definisi order aktif kini lengkap
dengan kondisi kedaluwarsa.

### `DEC-BD-015`

**Pertanyaan.** Siapa sumber sah golongan darah dan Rhesus, siapa yang memvalidasinya, dan kapan
label boleh dicetak?

**Kemampuan yang terdampak.** `BD-SLICE-09`, dan setiap pemakaian golongan darah untuk keputusan
kesesuaian.

**Bukti saat ini.** `MstPatient.BloodType` ada dan terisi lewat pendaftaran pasien, tetapi ia data
induk administratif. Tidak ditemukan entity hasil pemeriksaan golongan darah di modul Laboratorium.
`DEC-BD-011` dan `INV-BD-011` sudah menegaskan golongan darah pada permintaan bukan hasil
pemeriksaan yang sah.

**Usulan baseline.** Sampai sumber sah ditetapkan, larang seluruh pemakaian golongan darah untuk
keputusan kesesuaian, dan larang pencetakan label yang menampilkan golongan darah seolah-olah hasil
pemeriksaan.

**Dampak.** Keselamatan pasien, makna klinis, authorization validasi, dan penelusuran.

**Pemilik.** Pemilik proses klinis.

**Status.** `CLOSED` sebagian pada 2026-09-02 — sumber sahnya dijawab `DEC-BD-015`. Mekanik
labelnya masih terbuka sebagai `OQ-BD-011`.

**Dampak implementasi / domain.** Pemeriksaan dan validasi golongan darah siap dirancang. Bagian
label berhenti.

### `DEC-BD-016`

**Pertanyaan.** Apakah pemilik BillingManagement menyetujui penambahan konteks sumber dan jenis efek
biaya untuk Bank Darah pada `BillingSourceContract`?

**Kemampuan yang terdampak.** `BD-SLICE-08`.

**Bukti saat ini.** `BillingSourceContract` memuat daftar sumber tertutup berisi `InternalTest`,
`Prescription`, `Procedure`, `Laboratory`, dan `Radiology`. Bank Darah tidak ada di dalamnya. Pola
pengirimannya sudah mapan lewat `ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync`.

**Usulan baseline.** Ikuti pola Laboratorium apa adanya: satu konteks sumber dan satu jenis efek
biaya untuk Bank Darah, dengan salinan tarif pada baris tindakan supaya pengiriman ulang tetap
menghasilkan isi yang sama.

**Dampak.** Konsekuensi biaya dan kontrak integrasi antarmodul.

**Pemilik.** Pemilik BillingManagement.

**Status.** `OPEN` — satu-satunya keputusan pemblokir lintas modul yang tersisa. Aturan bisnisnya
sudah ditutup `DEC-BD-021`: pemicunya satu tindakan Bank Darah yang selesai, bukan per kantong.

**Dampak implementasi / domain.** Pencatatan tindakan Bank Darah tetap dapat dirancang. Kontrak
penyerahan biayanya yang berhenti.

### Keputusan yang sudah ada dan tetap berlaku

`DEF-BD-001` ditutup `DEC-BD-019` pada 2026-09-02. `DEF-BD-002` ditutup `DEC-BD-020` pada tanggal
yang sama.

Dua penundaan baru muncul dari closure pass: `DEF-BD-003` bukti kecocokan per komponen darah, dan
`DEF-BD-004` peran pemakai jalur darurat sekaligus peran validator hasil golongan darah. Keduanya
menahan implementasi, bukan perancangan.

---

## 6. Kesiapan per slice

| Slice | Kesiapan | Decision ID yang menghalangi |
| --- | --- | --- |
| `BD-SLICE-01` Order darah pasien | **`READY_FOR_DOMAIN_DESIGN`** | — dengan catatan definisi "order aktif" memakai dua kondisi akhir dulu |
| `BD-SLICE-02` Permintaan darah ke PMI | **`READY_FOR_DOMAIN_DESIGN`** | — jalur penutupan administratif dikeluarkan, menunggu `DEF-BD-002` |
| `BD-SLICE-03` Penerimaan fisik kantong | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `BD-SLICE-04` Alokasi kantong ke pasien | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `BD-SLICE-05` Pemberian darah | **`READY_FOR_DOMAIN_DESIGN`** | — peran pemakai jalur darurat (`DEF-BD-004`) menahan implementasi, bukan rancangan |
| `BD-SLICE-06` Kedaluwarsa dan kantong menunggu keputusan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `BD-SLICE-07` Pengembalian kantong | **`READY_FOR_DOMAIN_DESIGN`** | — kegunaan pilihan pengembalian ke PMI bergantung `OQ-BD-010` |
| `BD-SLICE-08` Tindakan Bank Darah dan biayanya | `PARTIALLY_READY` | Pencatatan tindakan siap dirancang; penyerahan biaya menunggu `DEC-BD-016` |
| `BD-SLICE-09` Label golongan darah | `PARTIALLY_READY` | Pemeriksaan dan validasi golongan darah siap dirancang; mekanik label menunggu `OQ-BD-011` |
| `BD-SLICE-10` Sampling, Lab, HCLAB, laporan, setup | **`READY_FOR_DOMAIN_DESIGN`** | — |

**Kesiapan modul: `PARTIALLY_READY`** — delapan slice siap penuh, dua siap sebagian.

## 7. Apa yang boleh berjalan

Delapan slice boleh maju ke perancangan arsitektur target sekarang: `BD-SLICE-01` sampai
`BD-SLICE-07`, ditambah `BD-SLICE-10`. Rangkaian bisnisnya lengkap dari order darah masuk,
permintaan ke PMI, penerimaan kantong, pemeriksaan golongan darah, alokasi, pencatatan bukti
kecocokan, pemberian termasuk jalur daruratnya, kedaluwarsa order, sampai penyelesaian kantong yang
menunggu keputusan.

Perancangan boleh menetapkan kepemilikan data, relasi, lifecycle lengkap termasuk jalur tidak
normal, pengaman konkurensi, jejak audit, dan bentuk hak akses.

**Dua batas yang tersisa untuk perancangnya:**

1. Pencatatan tindakan Bank Darah boleh dirancang, tetapi kontrak penyerahan biaya ke Billing tidak
   dibekukan sampai `DEC-BD-016` disetujui pemilik Billing.
2. Pemeriksaan dan validasi golongan darah boleh dirancang, tetapi bentuk label, syarat pencetakan,
   identifier, dan perilaku cetak ulang tidak dirancang sampai `OQ-BD-011` dijawab.

Dua batas lama sudah dicabut: definisi order aktif kini lengkap dengan kondisi kedaluwarsa, dan
lifecycle kantong kini lengkap sampai keadaan diberikan.

## 8. Apa yang harus berhenti

Hanya dua bagian yang berhenti, dan keduanya bagian dari slice yang selain itu sudah siap:

1. **Kontrak penyerahan biaya ke Billing** — menunggu persetujuan pemilik BillingManagement.
2. **Mekanik label golongan darah** — menunggu keputusan pemilik proses klinis.

Menebak keputusan klinis, keputusan billing, atau kebijakan rumah sakit di dalam rancangan tetap
dilarang.

## 9. Keputusan pemilik yang dibutuhkan

| Decision ID | Pemilik | Pertanyaan pokok |
| --- | --- | --- |
| `DEC-BD-016` | Pemilik BillingManagement | Setuju menambah satu konteks sumber dan satu jenis efek biaya Bank Darah pada kontrak Billing, dengan pemicu tunggal berupa tindakan Bank Darah yang selesai? |
| `OQ-BD-011` | Pemilik proses klinis | Apa isi label golongan darah, kapan boleh dicetak, apa identifier uniknya, dan bagaimana perilaku cetak ulang? |
| `DEF-BD-003` | Pemilik proses klinis | Apakah semua komponen darah menuntut bukti kecocokan yang sama? |
| `DEF-BD-004` | Pemilik proses BDRS dan klinis | Peran mana yang berhak memakai jalur darurat, dan peran mana yang berhak memvalidasi hasil golongan darah? |
| `OQ-BD-010` | Pemilik proses BDRS | Apakah PMI menerima pengembalian kantong yang sudah keluar? |
| — | Pemilik kebutuhan | Sediakan tiga berkas bukti yang dirujuk BRD (`GAP-BD-011`) |
| — | Pemilik proses BDRS | Benarkan atau koreksi enam asumsi `ASM-BD-001` sampai `ASM-BD-005` dan `ASM-BD-007` |

## 10. Handoff berikutnya

### Yang dikirim maju

`BD-SLICE-01` sampai `BD-SLICE-07` dan `BD-SLICE-10` dikirim ke `hospital-domain-architect`. Skill
itu bersifat opsional, tetapi dipakai di sini karena slice-slice tersebut melintasi lima bounded
context — Registration, Patient, Human Resource, Master Data, dan Inpatient — menyentuh data induk
bersama, dan memuat titik keselamatan klinis berupa bukti kecocokan beserta jalur daruratnya.

Bila arsitektur domain dilewati, kedelapan slice boleh langsung ke `design-business-module`.

Field yang dibawa pada handoff:

```yaml
blueprint_id: BD-BP-001
blueprint_revision: 3
input_revision_hash: grill-me-closure-pass-2026-09-02
backend_source_sha: 9522caacf29371b1fddd1584e9a71ad94fe48d19
frontend_source_sha: afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254
current_phase: BD-PH-004
capability_scope: [BD-SLICE-01, BD-SLICE-02, BD-SLICE-03, BD-SLICE-04, BD-SLICE-05, BD-SLICE-06, BD-SLICE-07, BD-SLICE-10]
requirement_readiness: PARTIALLY_READY
requirement_evidence_status: seluruh slice yang dikirim CONFIRMED; tidak ada CONFLICT tersisa
baseline_reference_coverage: NOT_YET_AVAILABLE
blocking_decision_ids: [DEC-BD-016, OQ-BD-011, DEF-BD-003, DEF-BD-004, OQ-BD-010]
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
dependency_ids: [BD-DEP-001, BD-DEP-002, BD-DEP-003, BD-DEP-004, BD-DEP-005, BD-DEP-007, BD-DEP-010, BD-DEP-011, BD-DEP-012, BD-DEP-013, BD-DEP-014]
decision_revision: 2
contract_versions: []
```

### Yang dikembalikan

Closure pass sudah dijalankan dan menutup sepuluh keputusan. Empat butir yang tersisa —
`DEC-BD-016`, `OQ-BD-011`, `DEF-BD-003`, `DEF-BD-004` — dikembalikan ke pemiliknya masing-masing,
bukan ke `grill-me`, karena menunggu persetujuan lintas modul, keputusan klinis, atau keputusan hak
akses menyeluruh. Gerbang ini tidak menjawab keputusan yang bergantung pemilik atas nama rumah
sakit.

### Yang tidak dibuat di sini

Tidak ada entity, ERD, kontrak API, migration, desain UI, task implementasi, maupun perubahan
ClickUp yang dibuat dari penilaian ini.
