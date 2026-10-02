# Platform — Requirement Completeness Assessment

| Field | Value |
|---|---|
| Blueprint ID | `PLT-BP-001` |
| Blueprint revision | `1` |
| Assessment revision | `2` — **tidak dinaikkan**; lihat catatan penyegaran bukti di bawah |
| Modul / Menu | Platform — kemampuan lintas modul |
| Kemampuan yang dinilai | **Alokasi nomor bisnis** (`DEC-PLT-001`) |
| Backend SHA | **`4a1da7d`** cabang `sukmagp` — semula `ba75a05`; disegarkan 7 September 2026 |
| Frontend SHA | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` — **tidak bergerak** |
| Decision log | revisi **`2`** — `DEC-PLT-001`..`DEC-PLT-008`, `OQ-PLT-001`..`OQ-PLT-011`, `CONF-PLT-001`..`002`, `NOTE-PLT-001` |
| Capability map | revisi **`3`** — `PLT-CAP-001`..`PLT-CAP-007`, termasuk jawaban `OQ-PLT-008` dan **koreksi bukti `PLT-CAP-001`/`PLT-CAP-007`** |
| Tanggal | `2026-09-04`; **bukti disegarkan** `2026-09-07`; **pemilik ditunjuk** `2026-09-09` |
| Mode | Read-only terhadap repository aplikasi |

> **Penyegaran bukti 7 September 2026 — kesiapan tidak berubah.** Audit terarah memindahkan dua baris
> peta kemampuan: `PLT-CAP-001` dari `Missing` menjadi **`Extend`**, dan `PLT-CAP-007` dari `Missing`
> menjadi **`Repair`**. Mesin alokasi nomor yang atomik ternyata **sudah ada** —
> `BillingNumberSeriesService` beserta tabel `BilNumberSeries` — hanya saja dimiliki Billing dan tanpa
> pintu masuk generik.
>
> **`PLT-SLICE-01` tetap `BUSINESS_DECISION_REQUIRED`, dan blocker-nya tetap `OQ-PLT-007` saja.**
>
> **Pembaruan 9 September 2026 — blocker itu sudah tertutup.** `OQ-PLT-007` dijawab: pemilik kontrak
> engineering backend adalah **`Andry`**, ditunjuk eksplisit oleh pemilik kebutuhan. **Verdict kesiapan
> di bawah sengaja TIDAK diputar sendiri** — menaikkan `PLT-SLICE-01` dari `BUSINESS_DECISION_REQUIRED`
> adalah hasil penilaian, bukan akibat otomatis, dan menuntut `requirement-completeness-gate`
> dijalankan ulang. Dua syarat yang masih nyata sebelum penilaian itu bermakna: **(a)** `Andry` benar-benar
> menurunkan approval atas `DEC-PLT-002`..`008`, yang seluruhnya masih `draft`; **(b)** dimensi
> `NON_BLOCKING_STANDARD` yang masih `MISSING`/`PROPOSED` di bagian 4 ditutup saat desain.
> Yang berubah adalah **bentuk pekerjaannya**, bukan kesiapannya: dari "membangun mesin dari nol"
> menjadi "menggeneralisasi mesin yang sudah terbukti". Audit teknis tidak dapat menutup pertanyaan
> kepemilikan, sehingga kelima keputusan tetap `draft` sampai nama pemiliknya ditunjuk.
>
> Dua pertanyaan baru lahir dari audit itu dan dicatat pada peta kemampuan bagian 7:
> **`OQ-PLT-010`** (bolehkah mesin Billing dinaikkan menjadi milik platform) dan **`OQ-PLT-011`**
> (nomor hangus atau dipakai ulang bila transaksi bisnis gagal setelah alokasi). Keduanya
> **tidak** mengubah kesiapan slice mana pun — `OQ-PLT-010` menentukan pilihan arsitektur, dan
> `OQ-PLT-011` menutup dimensi 05 yang memang sudah berstatus `MISSING` sejak revisi 1.

> **Closure pass 7 September 2026 — dimensi 05 tertutup, kesiapan tetap.** Keduanya sudah dijawab
> pemilik kebutuhan dan direkam sebagai `DEC-PLT-007` (**ekstrak mesin bersama**, konfigurasi Billing
> tetap milik Billing) dan `DEC-PLT-008` (**nomor hangus** bila transaksi dibatalkan). Keduanya
> berstatus **`draft`** karena `OQ-PLT-007` belum tertutup — pemilik kebutuhan menegaskan pemilik
> kontrak engineering backend **belum ditetapkan**, dan tidak boleh disimpulkan dari jejak commit.
>
> **Dimensi 05 "Alur Alternatif / Exception" naik dari `MISSING` menjadi `CONFIRMED`** untuk bagian
> perilaku saat penyimpanan gagal: nomor hangus. Yang masih terbuka dari dimensi itu hanya "apakah
> sistem mencoba ulang dan apa yang dilihat petugas" — bersifat `NON_BLOCKING_STANDARD`.
>
> **Satu pekerjaan baru terlihat, dan itu bukan kabar buruk.** `DEC-PLT-008` menuntut perilaku yang
> **berbeda** dari mesin Billing sekarang (`CONF-PLT-002`), sehingga ekstraksi `DEC-PLT-007` bukan
> pemindahan apa adanya. Ini menambah pekerjaan nyata pada `PLT-SLICE-01` yang sebelumnya tidak
> terlihat. Dampaknya terhadap empat deret Billing yang sudah produksi disimpan sebagai **catatan
> desain tertunda `NOTE-PLT-001`** — **bukan** Open Question dan **bukan** blocker.
>
> **Kesiapan slice tidak berubah:** tetap `BUSINESS_DECISION_REQUIRED`, dan **satu-satunya blocker
> approval dari pass ini adalah `OQ-PLT-007`** — nama Backend Engineering Contract Owner belum
> ditetapkan. Closure pass 7 September 2026 **nol** menambah Open Question maupun blocker baru.

---

## 1. Scope penilaian

Kemampuan ini dipecah menjadi **empat slice** yang masing-masing masih bermakna bila dipikirkan
sendiri. Memblokir keempatnya sekaligus akan menahan pekerjaan yang sebenarnya sudah cukup jelas.

| Slice | Isi | Kenapa dipisah |
| --- | --- | --- |
| `PLT-SLICE-01` | **Mesin alokasi nomor untuk deret baru** | Berdiri sendiri. Inilah yang membuka `BE-BD-003` dan tiga modul lain |
| `PLT-SLICE-02` | **Migrasi deret lama** ke mesin baru | Bergantung `PLT-SLICE-01`, dan menyentuh 106 berkas yang sudah dipakai produksi |
| `PLT-SLICE-03` | **Penyeragaman format dan panjang nomor** | Menyentuh rupa nomor yang sudah terbit; risikonya berbeda dari sekadar mengganti mesin |
| `PLT-SLICE-04` | **Penelusuran nomor kembar yang mungkin sudah terbit** | Menuntut akses data produksi, bukan pembacaan source |

**Di luar penilaian ini:** nomor antrean harian, rename entity, pola akses `ApplicationDbContext`,
dan pola hak akses — seluruhnya dikeluarkan pengguna lewat `DEC-PLT-001`.

---

## 2. Bukti yang dipakai

| Urutan wewenang | Bukti | Dipakai untuk |
| --- | --- | --- |
| 1 — requirement eksplisit user | Jawaban pengguna 4 September 2026 (`DEC-PLT-001`..`005`) | Apa yang seharusnya dibangun |
| 6 — bukti implementasi V2 | `02-existing-capability-map.md` revisi 1 @`ba75a05` | Apa yang saat ini ada |
| 6 — bukti implementasi V2 | Laporan task `BE-RWI-035` (modul Rawat Inap) | Temuan sebelumnya atas cacat yang sama |

**Tidak dipakai:** SOP rumah sakit yang disahkan, notulen rapat, dan bukti ClickUp — tidak ada yang
tersedia untuk kemampuan ini. `indonesia-hospital-domain-reference` **tidak dipanggil**: penomoran
dokumen internal bukan kepedulian domain rumah sakit Indonesia yang butuh baseline rujukan, dan
memaksakannya justru akan menghasilkan usulan yang tidak berdasar.

---

## 3. Temuan kelengkapan per dimensi

Dinilai terhadap `PLT-SLICE-01`, karena slice inilah yang dicalonkan maju.

| # | Dimensi | Status | Catatan |
| --- | --- | --- | --- |
| 01 | Tujuan | `CONFIRMED` | Menerbitkan nomor bisnis yang unik dan tahan permintaan serentak. `DEC-PLT-001`, `FACT-PLT-001`..`005` |
| 02 | Aktor | `CONFIRMED` | Pemakainya adalah service modul saat membuat data. Kepemilikan keputusan dibagi `DEC-PLT-005` |
| 03 | Pemicu / Prasyarat | `CONFIRMED` | Terpicu saat sebuah catatan baru dibuat dan membutuhkan nomor |
| 04 | Alur Utama | `CONFIRMED` | Modul meminta nomor pada deretnya, mesin mengalokasikan satu nomor, nomor menempel pada catatan |
| 05 | Alur Alternatif / Exception | `MISSING` | Perilaku saat alokasi berebut atau penyimpanan gagal setelah nomor diambil belum ditetapkan. `DEC-PLT-002` sudah memastikan nomornya hangus, tetapi belum ada keputusan apakah sistem mencoba ulang dan apa yang dilihat petugas |
| 06 | Data Minimum | `PROPOSED` | Sebuah deret dikenali dari modul, jenis dokumen, dan awalannya — turunan `DEC-PLT-005`, belum dinyatakan resmi |
| 07 | Aturan Bisnis / Validation | `CONFIRMED` | `INV-PLT-001`..`004` |
| 08 | Status / Perubahan Status | **tidak berlaku** | Sebuah nomor tidak punya lifecycle; ia terbit sekali lalu melekat. Yang punya lifecycle adalah catatan yang memakainya, dan itu milik modul masing-masing |
| 09 | Peran / Authorization | `PROPOSED` | Usulan: siapa pun yang berwenang membuat catatannya, berwenang pula memperoleh nomornya — tidak ada hak akses terpisah. Belum dikonfirmasi pemilik |
| 10 | Dependency Antarmodul | `CONFIRMED` | Empat modul menunggu: `bank-darah`, `billing-kasir`, `rawat-inap`, `rawat-jalan` (`FACT-PLT-006`) |
| 11 | Integrasi Eksternal | **tidak berlaku** | Nomor diterbitkan sepenuhnya di dalam sistem. Nol pihak luar menerbitkan maupun memvalidasinya |
| 12 | Hasil Akhir | `CONFIRMED` | `AC-PLT-001`..`006`, seluruhnya ditulis agar dapat diuji |
| 13 | Pembatalan / Koreksi | `CONFIRMED` | `DEC-PLT-002`: nomor tidak pernah kembali maupun dipakai ulang |
| 14 | Audit / Histori | `PROPOSED` | Usulan: catat siapa meminta nomor apa dan kapan. Berguna untuk menelusuri nomor kembar, tetapi belum diputuskan wajib |
| 15 | Notifikasi | **tidak berlaku** | Penerbitan nomor bukan kejadian yang perlu diberitahukan kepada siapa pun |
| 16 | Dampak Billing | **tidak berlaku secara material** | Nomor memang muncul di dokumen tagihan, tetapi `DEC-PLT-004` mempertahankan rupa nomor, sehingga nol dokumen keuangan berubah bentuk |
| 17 | Dampak Keselamatan Klinis | `CONFIRMED` **dan material** | Nomor kembar pada order darah atau rekam medis dapat menautkan dokumen ke pasien yang keliru. `INV-PLT-001` menutup risiko itu untuk deret baru; risiko pada deret lama tetap terbuka sampai `PLT-SLICE-02` |
| 18 | Pelaporan / Traceability | `CONFIRMED` | Nomor adalah kunci penelusuran antar dokumen; `INV-PLT-001` menjaga satu nomor menunjuk tepat satu catatan |

---

## 4. Butir CONFIRMED, PROPOSED, MISSING, dan CONFLICT

| Butir | Status | Dampak | Slice terdampak |
| --- | --- | --- | --- |
| Nomor tidak pernah dipakai ulang (`DEC-PLT-002`) | `CONFIRMED` | — | semua |
| Deret berjalan terus, tidak diulang (`DEC-PLT-004`) | `CONFIRMED` | — | semua |
| Migrasi bertahap menurut risiko (`DEC-PLT-003`) | `CONFIRMED` | — | `01`, `02` |
| Pembagian kewenangan platform/modul (`DEC-PLT-005`) | `CONFIRMED` | — | semua |
| **Nama pemilik yang berwenang menyetujui** (`OQ-PLT-007`) | ✅ **`CONFIRMED`** 9 Sep 2026 — **`Andry`** | — | **semua** |
| Perilaku saat alokasi berebut atau penyimpanan gagal | `MISSING` | `NON_BLOCKING_STANDARD` | `01` |
| Identitas sebuah deret (modul + jenis dokumen + awalan) | `PROPOSED` | `NON_BLOCKING_STANDARD` | `01` |
| Hak akses penerbitan nomor menempel pada hak membuat catatan | `PROPOSED` | `NON_BLOCKING_STANDARD` | `01` |
| Kewajiban mencatat jejak penerbitan nomor | `PROPOSED` | `NON_BLOCKING_STANDARD` | `01` |
| ~~Deret mana yang tanpa index unik~~ (`OQ-PLT-008`) | ✅ **`CONFIRMED`** | — | `02` |
| **Panjang nomor dan nasib deret yang hampir habis** (`OQ-PLT-005`) | `MISSING` | **`BLOCKING`** | `03` |
| Kode fasilitas `RSMMC` ditanam di kode (`OQ-PLT-006`) | `MISSING` | `CONFIGURABLE_DEFAULT` | `03` |
| **Perilaku as-is mengisi celah, bertentangan dengan `DEC-PLT-002`** (`CONF-PLT-001`) | **`CONFLICT`** | **`BLOCKING`** | `02` |
| **Apakah nomor kembar sudah terlanjur terbit** (`OQ-PLT-009`) | `MISSING` | **`BLOCKING`** | `04` |

---

## 5. Decision Log — keputusan pemilik yang dibutuhkan

### `OQ-PLT-007`

**Pertanyaan.** Siapa nama pemegang peran pemilik kontrak engineering backend yang berwenang
menurunkan approval atas `DEC-PLT-002`..`DEC-PLT-005`?

**Kemampuan terdampak.** Seluruh slice.

**Bukti saat ini.** `DEC-PLT-005` menetapkan **perannya**, bukan orangnya. Registry
`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` mencatat kepemilikan prefix per modul, tetapi tidak menyebut
nama pemegang kewenangan kontrak engineering.

**Dampak.** Kelima keputusan tetap `draft`. Merancang arsitektur target di atas keputusan `draft`
berarti membangun di atas dasar yang belum sah.

**Status.** `OPEN`.

**Dampak implementasi.** Menahan seluruh slice. Begitu tertutup, `PLT-SLICE-01` langsung memenuhi
syarat maju.

### `DEC-PLT-006` — baru, lahir dari audit

**Pertanyaan.** Selama masa peralihan `DEC-PLT-003`, deret yang belum dimigrasikan masih mengisi
celah dan karena itu **melanggar `INV-PLT-001`**. Apakah pelanggaran sementara ini diterima secara
resmi, dan sampai kapan?

**Kemampuan terdampak.** `PLT-SLICE-02`.

**Bukti saat ini.** `CONF-PLT-001` pada capability map revisi 1: perilaku as-is memuat seluruh nomor
lalu mengisi celah pertama yang kosong.

**Usulan baseline.** Pelanggaran dinyatakan terbuka beserta daftar deret yang masih melanggar, lalu
ditutup mengikuti urutan migrasi. Usulan ini `PROPOSED`, bukan kebijakan.

**Dampak.** Integritas data dan keselamatan klinis pada deret yang belum dimigrasikan.

**Status.** `OPEN`.

### `OQ-PLT-008` — ✅ **TERTUTUP** 4 September 2026

**Pertanyaan.** Deret mana saja yang **tidak** dilindungi index unik?

**Jawaban berbasis bukti.** Dari **122** deret yang ditulis pembangkit nomor: **93** terlindungi
index unik kolom tunggal, **10+** terlindungi index unik gabungan yang memang bercakupan, dan
**tepat satu** tanpa perlindungan — `MstBank.BankCode`.

**Koreksi terhadap penilaian revisi 1.** Angka "277 dari 422 tidak unik" memakai pembagi yang
keliru; ia menghitung seluruh index yang kebetulan menyebut `Code`/`Number`, bukan deret yang
benar-benar ditulis pembangkit. Setelah diadu dengan pembagi yang benar, perlindungannya justru
hampir menyeluruh.

**Konsekuensi untuk urutan migrasi.** Seluruh deret kritis klinis terlindungi — `MedicalRecordNumber`,
`PatientCode`, `EncounterNumber`, `PaymentSourceNumber`, `SessionCode`. Satu-satunya celah ada pada
master keuangan yang jarang berubah. Artinya urutan migrasi **tidak** perlu ditentukan oleh
ketiadaan index, melainkan oleh **keramaian jalur**.

**Status.** `CLOSED`.

### `OQ-PLT-005`

**Pertanyaan.** Berapa panjang nomor yang ditetapkan, dan bagaimana nasib deret yang sudah mendekati
batas — `LegalEntityController` memakai 3 digit sehingga habis setelah `999`?

**Dampak.** Mengubah rupa nomor yang dipersistensi, sehingga bertentangan dengan niat
`DEC-PLT-004` menjaga kesinambungan bentuk.

**Status.** `OPEN`.

### `OQ-PLT-009`

**Pertanyaan.** Perlukah menelusuri apakah nomor kembar sudah terlanjur terbit pada deret tanpa
index unik?

**Bukti saat ini.** **Tidak dapat dijawab dari source.** Menuntut pemeriksaan data produksi.

**Status.** `OPEN` — berhenti di sini sesuai aturan: dibutuhkan akses lingkungan.

---

## 6. Kesiapan per slice

| Slice | Kesiapan | Blocker |
| --- | --- | --- |
| `PLT-SLICE-01` mesin alokasi deret baru | **`BUSINESS_DECISION_REQUIRED`** | `OQ-PLT-007` **saja** |
| `PLT-SLICE-02` migrasi deret lama | **`BUSINESS_DECISION_REQUIRED`** | `OQ-PLT-007`, `DEC-PLT-006` — `OQ-PLT-008` ✅ tertutup |
| `PLT-SLICE-03` penyeragaman format | **`BUSINESS_DECISION_REQUIRED`** | `OQ-PLT-007`, `OQ-PLT-005` |
| `PLT-SLICE-04` penelusuran nomor kembar | **`BUSINESS_DECISION_REQUIRED`** | `OQ-PLT-009` + akses data produksi |

**Kesiapan kemampuan secara keseluruhan: `BUSINESS_DECISION_REQUIRED`.**

Catatan penting supaya tidak salah baca: `PLT-SLICE-01` **isinya sudah lengkap**. Ketujuh belas
dimensi yang berlaku sudah terjawab, dan satu-satunya yang `MISSING` bersifat
`NON_BLOCKING_STANDARD`. Yang menahannya bukan kekurangan requirement, melainkan **ketiadaan orang
yang berwenang mengesahkannya**. Begitu `OQ-PLT-007` tertutup dan kelima keputusan naik dari
`draft`, slice ini langsung menjadi `READY_FOR_DOMAIN_DESIGN` tanpa penilaian ulang.

**Diperkuat penyegaran bukti 7 September 2026.** Kesiapan `PLT-SLICE-01` **tetap**
`BUSINESS_DECISION_REQUIRED` dengan blocker `OQ-PLT-007` **saja** — audit terarah tidak menambah satu
pun blocker baru pada slice ini. Yang bertambah justru keyakinannya: bukti sekarang menunjukkan
mesin alokasi atomik **sudah ada dan terbukti jalan**, sehingga desain nanti bekerja di atas pola
yang sudah terpakai di 24 lokasi, bukan di atas rancangan yang belum pernah dicoba. Ini menurunkan
risiko desain; ia **tidak** menurunkan syarat approval.

---

## 7. Apa yang boleh berjalan, apa yang harus berhenti

| Boleh berjalan sekarang | Harus berhenti |
| --- | --- |
| Menunjuk pemilik `OQ-PLT-007` | Merancang arsitektur target slice mana pun |
| Menjawab `OQ-PLT-008` lewat penelusuran source — read-only, tidak menunggu siapa pun | Menulis source alokator |
| Menyiapkan `BE-BD-003` sampai batas yang tidak menyentuh penomoran | Migrasi deret lama |
| Mencatat `DEC-PLT-006` untuk diputuskan pemilik | Mengubah panjang atau rupa nomor |

**`BE-BD-003` tetap tertahan — tetapi sifat penahannya berubah pada 7 September 2026.**

Kalimat pada revisi 2 berbunyi "mesinnya belum ada (`PLT-CAP-001` `Missing`)". **Bukti membantah
bagian itu.** Mesin alokasi atomik sudah ada dan dipakai produksi; `PLT-CAP-001` kini **`Extend`**.

Yang menahan `BE-BD-003` karena itu **bukan ketiadaan teknologi, melainkan keputusan kepemilikan**:
bolehkah Bank Darah memakai mesin Billing yang digeneralisasi (`OQ-PLT-010`), atau memakai polanya
sendiri mengikuti konvensi rumah. Keduanya layak secara teknis, dan **tidak satu pun boleh
diputuskan tanpa pemilik** — `OQ-PLT-007` tetap harus tertutup lebih dulu.

Satu catatan untuk penjadwal, bukan keputusan: bila jalur "pola sendiri" yang dipilih, `BE-BD-003`
**tidak** perlu menunggu `PLT-SLICE-01` selesai. Ini memperluas pilihan pemilik, bukan menutupnya.
Status task Bank Darah **tidak diubah** oleh dokumen ini.

---

## 8. Handoff berikutnya

| Kondisi | Skill | Muatan |
| --- | --- | --- |
| Keputusan pemblokir menunggu pemilik | **`grill-me`** lanjutan | `PLT-BP-001` rev 1 · `OQ-PLT-007`, `DEC-PLT-006`, `OQ-PLT-005`, `OQ-PLT-008` · `ba75a05`/`101ec5d3` · kesiapan `BUSINESS_DECISION_REQUIRED` |
| `OQ-PLT-008` butuh bukti source | `trace-existing-capabilities` mode terarah | Daftar deret beserta ada-tidaknya index unik |
| Setelah `OQ-PLT-007` tertutup | `design-business-module` untuk `PLT-SLICE-01` | **Arsitektur domain rumah sakit dilewati** — ini kemampuan infrastruktur, bukan alur kerja rumah sakit, sehingga `hospital-domain-architect` tidak memberi nilai tambah |

`hospital-domain-architect` **sengaja tidak dipakai** untuk kemampuan ini: ia dirancang untuk
menerjemahkan alur kerja rumah sakit menjadi bounded context dan aggregate, sementara alokasi nomor
tidak melintasi bounded context klinis mana pun dan tidak punya lifecycle domain.
