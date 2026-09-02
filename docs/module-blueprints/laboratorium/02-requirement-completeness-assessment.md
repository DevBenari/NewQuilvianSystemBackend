# Laboratorium — Penilaian Kelengkapan Requirement

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Assessment ID | `LAB-RCG-001` |
| Revision | `5` |
| Status | `draft` |
| **Kesiapan keseluruhan** | **`PARTIALLY_READY`** — seluruh blocker yang tersisa berada di luar wewenang pemilik modul |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Masukan | `00-interview-decisions.md` revision 14; `01-existing-capability-map.md` revision 1; `05-evidence-reconciliation.md` revision 2 |
| Tanggal penilaian | 2026-09-01 |
| Sifat | **Read-only** terhadap repository aplikasi |

> **Cara membaca dokumen ini.**
> Dokumen ini menjawab satu pertanyaan saja: **apakah kebutuhan bisnisnya sudah cukup jelas
> untuk mulai merancang?** Ia tidak merancang apa pun. Tidak ada tabel, tidak ada endpoint,
> tidak ada tampilan yang ditentukan di sini.
>
> Penilaian dilakukan **per bagian pekerjaan**, bukan per modul. Jadi mungkin saja bagian
> sampel sudah siap dirancang sementara bagian hasil pemeriksaan masih harus menunggu
> keputusan. Itu hal yang wajar dan justru disengaja.

---

## 0. Penilaian Ulang Revision 3 — Setelah Bukti Lapangan Diadopsi

> **Bagian ini menggantikan peta slice dan verdict pada bagian 5 sampai 9 di bawah.** Isi lama
> dipertahankan sebagai rekam jejak penilaian sebelum bukti lapangan tersedia.

**Pemicu.** Pemilik modul mengadopsi `Analisis_Konsolidasi_Modul_Laboratorium.md` sebagai
baseline requirement lewat `LAB-DEC-025` sampai `LAB-DEC-031` pada 2026-09-01.

### 0.1 Apa yang berubah

| Aspek | Sebelum | Sesudah |
|---|---|---|
| Disiplin yang dilayani | 1 — Patologi Klinik | **3** — Patologi Klinik, Patologi Anatomi, Mikrobiologi |
| Bentuk hasil | 2 | **4** |
| Pendaftaran pasien | Di luar scope | **Di dalam scope** untuk pasien datang langsung dan rujukan luar |
| Katalog dan tarif | Milik Master Data | **Ditampilkan dan dikelola** Laboratorium, keputusan uang tetap Billing |
| Jumlah bagian pekerjaan | 13 | **21** |
| Keputusan tata kelola yang terbuka | 4 | **17** |

### 0.2 Peta bagian pekerjaan yang berlaku

| Slice | Nama bagian | Kesiapan | Yang memblokir |
|---|---|---|---|
| `S1a` | Penanda cito dan duplo pada **pemeriksaan** | **`READY_FOR_DOMAIN_DESIGN`** | — (dampak tarif ditunda, `LAB-OPEN-013`) |
| `S1b` | Penghapusan `Draft` dan penyuntingan pesanan | `BUSINESS_DECISION_REQUIRED` | `LAB-AMD-001`, `LAB-P0-002` |
| `S2` | Siklus hidup wadah fisik | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S2b` | Atribut wadah khas Patologi Anatomi dan Mikrobiologi | `BUSINESS_DECISION_REQUIRED` | Penomoran PA/Sitologi/FNAB belum diputuskan |
| `S3` | Batas nilai dan batas kritis | **`READY_FOR_DOMAIN_DESIGN`** | — (hanya melayani hasil berbentuk angka dan pilihan) |
| `S4` | Pengisian dan validasi hasil Patologi Klinik | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-P0-001`, `LAB-P0-002`, `LAB-P0-005` |
| `S4b` | Pengisian hasil Mikrobiologi | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-P0-001`, `LAB-OPEN-017` |
| `S4c` | Pengisian hasil Patologi Anatomi | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-P0-001` |
| `S5` | Nilai kritis dan pelaporannya | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-COORD-001`, `LAB-P0-004`, `LAB-OPEN-014` |
| `S6` | Koreksi hasil setelah rilis | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-COORD-002`, `LAB-P0-003` |
| `S7` | Daftar kerja dan pemantauan keterlambatan | **`READY_FOR_DOMAIN_DESIGN`** | — (bergantung `S1a`) |
| `S8` | Pemberitahuan tersimpan | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-001` |
| `S9` | Pendaftaran hasil ke rekam medis | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-002` |
| `S10` | Fakta kelayakan tagih ke Billing | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S11` | Master alasan penolakan sampel | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S12` | Tampilan frontend | `PARTIALLY_READY` | Mengikuti bagian backend yang dilayaninya |
| `S13a` | Pendaftaran pasien **datang langsung** | **`READY_FOR_DOMAIN_DESIGN`** | — (`LAB-DEC-032`; kontrak antarmodul `LAB-COORD-003` diselesaikan saat desain) |
| `S13b` | Pendaftaran pasien **rujukan luar** | **`READY_FOR_DOMAIN_DESIGN`** | — (`LAB-DEC-035`; koordinasi data induk perujuk `LAB-COORD-004`) |
| `S14` | Katalog pemeriksaan, tarif, dan cakupan | **`READY_FOR_DOMAIN_DESIGN`** untuk bagian penyajian | Bagian aturan cakupan penjamin tertahan `LAB-P0-007` |
| `S15` | Monitoring per disiplin | **`READY_FOR_DOMAIN_DESIGN`** | — (bergantung `S2`) |
| `S16` | Laporan operasional | `BUSINESS_DECISION_REQUIRED` | Definisi sebelas laporan belum ada |
| `S17` | Label, nota, dan pengiriman hasil ke pasien | `BUSINESS_DECISION_REQUIRED` | Privasi pengiriman belum diputuskan |
| `S18` | Penautan berkas hasil laboratorium eksternal | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-002` |
| `S19` | Order dari MCU | `BUSINESS_DECISION_REQUIRED` | MCU belum pernah dibahas |

### 0.3 Verdict revision 3

**`PARTIALLY_READY`** — **10 bagian siap dirancang, 12 tertahan.**

| Boleh berjalan | Harus berhenti |
|---|---|
| `S1a` penanda cito dan duplo | `S1b` penghapusan `Draft` |
| `S2` siklus hidup wadah | `S2b` atribut wadah PA dan Mikrobiologi |
| `S3` batas nilai | `S4`, `S4b`, `S4c` seluruh pengisian hasil |
| `S7` daftar kerja | `S5` nilai kritis |
| `S10` fakta kelayakan tagih | `S6` koreksi hasil |
| `S11` alasan penolakan | `S8` pemberitahuan |
| **`S13` pendaftaran pasien datang langsung** | `S9` pendaftaran ke rekam medis |
| **`S14` katalog dan tarif — bagian penyajian** | `S16` laporan operasional |
| `S15` monitoring per disiplin | `S17` label, nota, kirim hasil |
| | `S18` berkas hasil eksternal |
| | `S19` order dari MCU |

### 0.4 Yang harus dibaca jujur dari verdict ini

| Tahap | Bagian siap | Total bagian |
|---|---:|---:|
| Sebelum bukti lapangan | 6 | 13 |
| Setelah bukti diadopsi | 7 | 21 |
| Setelah `LAB-DEC-032` dan `LAB-DEC-033` | 9 | 21 |
| **Setelah `LAB-DEC-035` dan `LAB-DEC-036`** | **10** | **21** |

Empat keputusan terakhir membuka bagian yang paling menentukan kegunaan modul:

| Bagian | Kenapa penting | Yang membukanya |
|---|---|---|
| `S13a` pendaftaran pasien datang langsung | Tanpa ini, pasien yang datang sendiri ke laboratorium tidak dapat dilayani sama sekali | `LAB-DEC-032` — Registrasi sudah punya penanda walk-in, jadi Laboratorium tinggal memanggilnya |
| `S13b` pendaftaran pasien rujukan luar | Bagian terbesar pasien laboratorium rujukan | `LAB-DEC-035` — instansi dan dokter perujuk menjadi data induk global |
| `S14` katalog dan tarif | Dipakai sejak layar pemesanan pertama | `LAB-DEC-033` — `MstTariff` sudah menampung semuanya, jadi Laboratorium tinggal menyajikannya |
| `S15` monitoring per disiplin | Tiga daftar sejajar yang dipakai sehari-hari | `LAB-DEC-025` dan `LAB-DEC-036` |

**Pola yang berulang empat kali sekarang.** Sebagian besar terbuka bukan karena ada yang
dibangun, melainkan karena bukti menunjukkan kemampuannya **sudah ada di modul lain** —
addendum rekam medis (`LAB-DEC-020`), penanda walk-in Registrasi (`LAB-DEC-032`), dan tabel
tarif bersama (`LAB-DEC-033`).

Yang **tidak** mengikuti pola itu hanya dua: `LAB-DEC-035` dan `LAB-DEC-036` benar-benar
memerlukan hal baru di modul lain — dua data induk perujuk dan satu kolom klasifikasi disiplin.
Keduanya menjadi `LAB-COORD-004` dan `LAB-COORD-005`.

**Konsekuensi bagi MVP.** Batas MVP pada `04-prd-to-mvp.md` kini dapat diperluas mencakup
pendaftaran pasien datang langsung dan penyajian katalog beserta tarif. Dengan keduanya, rilis
pertama menjadi modul yang **benar-benar dapat dipakai petugas dari layar pertama** — bukan
lagi potongan alur yang berhenti di tengah.

Dokumen PRD ke MVP perlu direvisi untuk memasukkan `S13`, `S14`, dan `S15` ke gelombang
pengiriman.

---

## 1. Scope Penilaian

Yang dinilai adalah kemampuan Rilis 1 modul Laboratorium sebagaimana dibatasi `LAB-DEC-001`
(rilis pertama sampai hasil dirilis) dan `LAB-DEC-002` (Patologi Klinik saja).

Pekerjaan dipecah menjadi **13 bagian** yang masing-masing masih masuk akal bila dipikirkan
sendiri:

| Slice | Nama bagian | Penjelasan singkat |
|---|---|---|
| `S1a` | Penandaan cito pada pesanan | Dokter menandai pesanan sebagai segera |
| `S1b` | Penghapusan status Draft dan jendela penyuntingan dokter | Dokter boleh mengubah pesanan selama sampel belum diambil |
| `S2` | Siklus hidup sampel | Rencana, ambil, terima, nyatakan layak atau tolak, ambil ulang |
| `S3` | Batas nilai dan batas kritis | Satuan, batas normal, batas kritis, batas waktu cito |
| `S4` | Pengisian dan validasi hasil | Analis mengisi angka, petugas lain memvalidasi, lalu dirilis |
| `S5` | Nilai kritis dan pelaporannya | Penandaan bahaya dan catatan pelaporan ke dokter |
| `S6` | Koreksi hasil setelah rilis | Perbaikan hasil dengan riwayat tetap utuh |
| `S7` | Daftar kerja dan pemantauan keterlambatan | Antrean pekerjaan lab, cito di urutan atas |
| `S8` | Pemberitahuan tersimpan | Kotak pemberitahuan dokter |
| `S9` | Pendaftaran hasil ke rekam medis | Hasil tercatat sebagai dokumen klinis |
| `S10` | Fakta kelayakan tagih ke Billing | Laboratorium mengirim fakta, bukan menghitung uang |
| `S11` | Master alasan penolakan sampel | Pengelolaan daftar alasan |
| `S12` | Seluruh tampilan frontend | Dibangun dari nol |

Yang **tidak** dinilai karena sudah dinyatakan di luar scope: Mikrobiologi, Patologi Anatomi,
Bank Darah, stok reagen, dan Radiologi.

---

## 2. Bukti yang Dipakai

| No | Bukti | Wewenang | Keterangan |
|---:|---|---|---|
| 1 | `00-interview-decisions.md` revision 10 | Requirement eksplisit dari pemilik modul | 23 keputusan berstatus `approved` oleh Yoga Aji Pratama pada 2026-09-01 |
| 2 | `RJ-BIL-GATE-DEC-003` pada blueprint `rawat-jalan` | Keputusan lintas modul, status `locked-draft` | Mewariskan `LAB-INH-001` sampai `LAB-INH-013`. **Tata kelola formalnya masih `OPEN`** — tanda tangan Lab, Clinical Governance, dan Billing belum dilampirkan |
| 3 | `01-existing-capability-map.md` revision 1 | Bukti implementasi terverifikasi | 24 kemampuan diklasifikasikan pada BE `c87d9c0` dan FE `688daff90` |
| 4 | Source backend pada `c87d9c0` | Bukti implementasi aktif | Dipakai hanya untuk menjawab "apa yang saat ini ada" |

**Yang tidak tersedia dan perlu dicatat jujur:** tidak ada SOP rumah sakit yang disahkan, tidak
ada notulen rapat, dan tidak ada bukti ClickUp yang disertakan pada sesi ini. Seluruh
requirement berasal dari wawancara langsung dengan pemilik modul. Ini sah menurut urutan
wewenang, tetapi berarti belum ada dokumen kebijakan rumah sakit yang menguatkannya.

---

## 3. Temuan Kelengkapan per Dimensi

Delapan belas dimensi dinilai. Tabel berikut meringkas keadaan seluruh modul; rincian per
bagian pekerjaan ada di bagian 5.

| ID | Dimensi | Keadaan | Catatan |
|---:|---|---|---|
| 01 | Tujuan | `CONFIRMED` | `LAB-DEC-001` menyatakan batas rilis dengan jelas |
| 02 | Aktor | `CONFIRMED` | Ditutup `LAB-DEC-022`: kewenangan validasi dan rilis diberikan per orang, minimal dua pemegang validasi tiap shift |
| 03 | Pemicu / Prasyarat | `CONFIRMED` | Pesanan menempel pada kunjungan yang sudah ada |
| 04 | Alur Utama | `CONFIRMED` | Tujuh langkah tercatat lengkap pada skenario normal |
| 05 | Alur Alternatif / Exception | `CONFIRMED` | Tujuh jalur tidak normal tercatat beserta acuannya |
| 06 | Data Minimum | `CONFIRMED` | Ditutup `LAB-DEC-021`: hasil punya dua bentuk, angka dan pilihan terbatas |
| 07 | Aturan Bisnis / Validation | `CONFIRMED` | 19 aturan bisnis BR-01 sampai BR-19 |
| 08 | Status / Perubahan Status | `CONFLICT` terselesaikan sebagian | `LAB-DEC-015` menghapus `Draft`, tetapi keputusan aslinya milik blueprint lain. Lihat `LAB-AMD-001` |
| 09 | Peran / Authorization | `CONFIRMED` | Ditutup `LAB-DEC-022` dan `LAB-DEC-023`. Perubahan batas kritis kini memerlukan persetujuan klinis |
| 10 | Dependency Antarmodul | `CONFIRMED` | Billing, rekam-medis, master-data, registrasi, platform |
| 11 | Integrasi Internal / Eksternal | `CONFIRMED` | `LAB-DEC-005` menyatakan tidak ada sambungan alat pada Rilis 1 |
| 12 | Hasil Akhir | `CONFIRMED` | Hasil dirilis dan terbaca dokter |
| 13 | Pembatalan / Koreksi | `CONFIRMED` | `LAB-DEC-007` dan `LAB-DEC-020` |
| 14 | Audit / Histori | `CONFIRMED` | `LAB-INH-013`, terbukti ada di `TrxLabTransitionHistory` |
| 15 | Notifikasi | `CONFIRMED` isinya, ownership `MISSING` | `LAB-DEC-012` dan `LAB-DEC-016`; pemiliknya belum disepakati. Lihat `LAB-COORD-001` |
| 16 | Dampak Billing | `CONFIRMED` | `LAB-INH-009` sampai `LAB-INH-012`, sudah terpasang dan teruji |
| 17 | Keselamatan Klinis | **`PROPOSED`** | Tiga keputusan keselamatan disetujui pemilik modul tetapi **belum ditandatangani pihak klinis**. Lihat `LAB-SIGN-001` |
| 18 | Pelaporan / Traceability | `CONFIRMED` sebagian | Pemantauan keterlambatan cito ada. Pelaporan manajerial lab belum dibahas — dinilai `NON_BLOCKING_STANDARD` |

---

## 4. Butir Bermasalah dan Dampaknya

### 4.1 Tiga gap baru yang ditemukan gerbang ini — **seluruhnya sudah ditutup**

Ketiganya belum pernah muncul pada Scope pass maupun Closure pass putaran pertama. Ketiganya
dibawa ke `grill-me` pada hari yang sama dan ditutup pemilik modul.

| Decision ID | Ditutup oleh | Isi keputusan |
|---|---|---|
| `DEC-LAB-001` | `LAB-DEC-022` | Kewenangan validasi dan rilis tetap terpisah, diberikan per orang, dan setiap shift wajib punya minimal dua pemegang kewenangan validasi |
| `DEC-LAB-002` | `LAB-DEC-021` | Hasil punya dua bentuk: angka dan pilihan terbatas. Pemeriksaan berhasil pilihan menyimpan daftar pilihan sah beserta penanda kritisnya |
| `DEC-LAB-003` | `LAB-DEC-023` | Batas normal bebas diubah kepala instalasi; batas kritis memerlukan persetujuan klinis. Seluruh perubahan berriwayat |

Uraian lengkap ketiganya tetap dipertahankan di bawah sebagai rekam jejak mengapa gerbang ini
menahannya, dan agar penilaian berikutnya tidak mengulang analisis yang sama.

#### `DEC-LAB-001` — Peran mana yang memegang kewenangan validasi dan rilis hasil?

| Field | Isi |
|---|---|
| Status bukti | `MISSING` saat dinilai, kini **`CONFIRMED`** |
| Dampak | `BLOCKING` saat dinilai, kini **tertutup** |
| Kemampuan terdampak | `S4`, `S5`, `S6` |
| Pemilik keputusan | Yoga Aji Pratama + Clinical Governance |

**Bukti saat ini.** `LAB-INH-007` menyatakan pengambilan, penerimaan, penetapan layak,
pemrosesan, validasi, dan rilis memakai kewenangan berbeda, dan **jabatan organisasi tidak
otomatis memberi kewenangan**. `BR-01` menyatakan pengisi hasil tidak boleh memvalidasi hasil
yang sama. Keduanya mengatur **hubungan antar kewenangan**, tetapi tidak satu pun menyebut
**siapa yang sebenarnya memegang kewenangan validasi dan rilis** di rumah sakit ini.

**Kenapa ini memblokir.** Tanpa jawabannya, prinsip empat mata tidak bisa dibuktikan bekerja.
Contoh: bila ternyata hanya ada satu orang di rumah sakit yang memegang kewenangan validasi,
maka setiap hasil yang ia periksa sendiri akan selalu melewati jalur pengecualian — dan aturan
empat mata menjadi formalitas kosong. Sebaliknya bila kewenangan validasi diberikan kepada
semua analis, aturan itu hanya mencegah satu orang memvalidasi pekerjaannya sendiri, bukan
menjamin ada pemeriksa yang lebih kompeten.

**Contoh nyata yang harus bisa dijawab.**

> Shift malam Sabtu. Yang bertugas adalah analis Sari dan analis Budi. Tidak ada dokter
> patologi klinik. Sari mengerjakan Kalium pasien Andi. Pertanyaannya: apakah Budi boleh
> memvalidasi dan merilis hasil itu, ataukah hasil harus menunggu dokter patologi klinik
> masuk Senin pagi?
>
> Jawaban "Budi boleh" dan "harus menunggu Senin" menghasilkan rancangan yang berbeda, dan
> keduanya berdampak langsung pada keselamatan pasien.

**Usulan baseline (belum dikonfirmasi, `PROPOSED`).** Kewenangan validasi dan rilis dipisahkan
menjadi kewenangan tersendiri yang diberikan per orang, bukan per jabatan, sehingga rumah sakit
dapat menyesuaikan tanpa mengubah kode.

#### `DEC-LAB-002` — Apakah hasil hanya berupa angka, atau juga hasil kualitatif?

| Field | Isi |
|---|---|
| Status bukti | `MISSING` saat dinilai, kini **`CONFIRMED`** |
| Dampak | `BLOCKING` saat dinilai, kini **tertutup** |
| Kemampuan terdampak | `S3`, `S4` |
| Pemilik keputusan | Yoga Aji Pratama |

**Bukti saat ini.** `BR-04` menetapkan tabel batas nilai berisi satuan hasil, batas normal
bawah dan atas, serta batas kritis bawah dan atas. Seluruhnya **berbentuk angka**. Tidak ada
satu pun keputusan yang menyebut hasil bukan angka.

**Kenapa ini memblokir.** `LAB-DEC-002` membatasi modul pada Patologi Klinik. Tetapi Patologi
Klinik **bukan seluruhnya berupa angka**. Contoh pemeriksaan yang lazim dan hasilnya bukan
angka:

| Pemeriksaan | Bentuk hasil | Bisa disimpan sebagai angka? |
|---|---|---|
| Hemoglobin | 9,4 g/dL | Ya |
| Kalium | 7,2 mmol/L | Ya |
| Protein urin | Negatif, +1, +2, +3, +4 | **Tidak** |
| Glukosa urin | Negatif, +1, +2, +3 | **Tidak** |
| Golongan darah | A, B, AB, O beserta rhesus | **Tidak** |
| Widal | Titer 1/80, 1/160, 1/320 | **Tidak sepenuhnya** |
| Tes kehamilan | Positif atau negatif | **Tidak** |

Bila bentuk hasil tidak diputuskan sekarang, ada dua akibat yang sama-sama mahal: entah
pemeriksaan kualitatif tidak bisa dimasukkan sama sekali, entah petugas memaksakannya sebagai
teks bebas sehingga sistem tidak akan pernah bisa menandai nilai kritis untuknya.

**Contoh nyata.**

> Protein urin pasien Andi keluar +4, yang secara klinis berat. Dengan tabel batas nilai yang
> hanya mengenal angka, sistem tidak punya cara membandingkan "+4" dengan batas kritis mana
> pun. `LAB-DEC-004` tentang nilai kritis menjadi tidak berlaku untuk seluruh kelompok
> pemeriksaan ini.

**Usulan baseline (belum dikonfirmasi, `PROPOSED`).** Hasil dibedakan menjadi hasil bernilai
angka dan hasil bernilai pilihan terbatas. Untuk hasil pilihan, batas kritis dinyatakan sebagai
daftar pilihan yang dianggap kritis, bukan sebagai batas bawah dan atas.

#### `DEC-LAB-003` — Siapa yang berwenang menetapkan dan mengubah batas kritis?

| Field | Isi |
|---|---|
| Status bukti | `MISSING` saat dinilai, kini **`CONFIRMED`** |
| Dampak | `BLOCKING` saat dinilai, kini **tertutup** |
| Kemampuan terdampak | `S3`, `S5` |
| Pemilik keputusan | Yoga Aji Pratama + Clinical Governance |

**Bukti saat ini.** `LAB-DEC-018` memutuskan batas nilai disimpan sebagai tabel milik
Laboratorium dan **dapat diubah kepala instalasi tanpa menerbitkan versi aplikasi baru**.
`LAB-DEC-019` justru mengunci kolom yang berdampak biaya pada tabel alasan penolakan agar hanya
bisa disetel admin sistem.

**Kenapa ini memblokir, dan kenapa ini pertentangan yang halus.** Angka batas kritis menentukan
kapan seorang pasien dinyatakan dalam bahaya. Menurunkan batas kritis atas Kalium dari 6,0
menjadi 8,0 berarti sistem berhenti memperingatkan pada nilai yang sebenarnya berbahaya.
Dampaknya lebih besar daripada penanda biaya yang sudah dikunci `LAB-DEC-019`, tetapi
pengaturannya justru lebih longgar.

**Contoh nyata.**

> Kepala instalasi mengubah batas kritis atas Kalium menjadi 8,0 karena merasa terlalu banyak
> peringatan mengganggu pekerjaan. Sejak saat itu, pasien dengan Kalium 7,2 — nilai yang bisa
> menghentikan jantung — tidak lagi memicu kewajiban pelaporan pada `BR-02`. Tidak ada yang
> melanggar aturan, dan tidak ada yang menyadarinya.

**Usulan baseline (belum dikonfirmasi, `PROPOSED`).** Batas normal boleh diubah kepala
instalasi; batas kritis memerlukan persetujuan pihak klinis, dan setiap perubahannya disimpan
sebagai riwayat bersama pelakunya.

### 4.2 Butir yang sudah tercatat sebelumnya

| ID | Isi | Status bukti | Dampak | Slice terdampak |
|---|---|---|---|---|
| `LAB-SIGN-001` | `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` belum ditandatangani pihak klinis, padahal `LAB-DEC-011` sendiri mensyaratkannya sebelum desain final | `PROPOSED` | **`BLOCKING`** | `S4`, `S5`, `S6` |
| `LAB-AMD-001` | `LAB-DEC-015` menghapus status `Draft` yang dikunci `LAB-INH-001` milik blueprint `rawat-jalan` | `CONFLICT` | **`BLOCKING`** | `S1b` |
| `LAB-COORD-001` | Kepemilikan kemampuan pemberitahuan bersama belum disepakati pemilik platform | `PROPOSED` | **`BLOCKING`** | `S8`, dan sebagian `S5` |
| `LAB-COORD-002` | Penambahan jenis dokumen klinis pada modul `rekam-medis` belum disepakati pemiliknya | `PROPOSED` | **`BLOCKING`** | `S9`, dan sebagian `S6` |
| `LAB-OPEN-002` | ~~`docs/engineering/` dan `.codex/` tidak ditemukan di repository~~ — **ditutup 2026-09-01 oleh `LAB-FACT-007`**: kedua dokumen ada di `QuilvianEngineeringSkills/agents/rules/backend/engineering/` dan masih berlaku; path `docs/engineering/` pada `AGENTS.md` usang | `CONFIRMED` | `RESOLVED` | — |
| `LAB-OPEN-018` | Rules root yang terpasang (`${CLAUDE_PLUGIN_ROOT}/.claude/rules/`) belum memuat subfolder `engineering/`, sehingga gerbang `AGENTS.md` memaksa `BLOCKED — canonical governance unavailable` pada setiap task backend | `CONFIRMED` | `NON_BLOCKING_STANDARD` untuk desain; **memblokir implementasi** | Seluruhnya, tetapi hanya pada tahap implementasi |
| `LAB-OPEN-019` | Registry mencatat `LaboratoryManagement` / prefix `Lab` berstatus `PLANNED`; `QBE-MOD-002` dan `QBE-MOD-003` menahan pembuatan entity operasional `Lab*` pertama | `CONFIRMED` | `NON_BLOCKING_STANDARD` untuk desain; **memblokir implementasi** | Seluruh slice yang membuat entity `Lab*` |
| `LAB-RISK-001` | `LAB-DEC-005` tidak menyiapkan kolom asal hasil, sehingga penyambungan alat kelak memerlukan perubahan struktur | `CONFIRMED` sebagai risiko yang disadari | `NON_BLOCKING_STANDARD` | `S4` |

### 4.3 Butir yang aman dijadikan bawaan atau usulan standar

| ID | Isi | Klasifikasi | Alasan |
|---|---|---|---|
| `DEC-LAB-004` | Apakah daftar kerja dibagi per bagian atau meja kerja, atau satu daftar bersama untuk seluruh petugas lab? | `CONFIGURABLE_DEFAULT` | `LAB-DEC-002` sudah membatasi modul pada satu disiplin, sehingga satu daftar bersama aman sebagai bawaan. Pembagian dapat ditambahkan kemudian tanpa mengubah makna domain |
| `DEC-LAB-005` | Isi data awal batas nilai: pemeriksaan mana saja beserta angkanya | `NON_BLOCKING_STANDARD` | Ini pekerjaan pengisian data, bukan keputusan struktur. Tetapi **wajib selesai sebelum modul dipakai** — tanpa isinya, `LAB-DEC-004` tidak berjalan |
| `DEC-LAB-006` | Isi data awal alasan penolakan sampel | `NON_BLOCKING_STANDARD` | Sama seperti di atas. Bila kosong, petugas tidak bisa menolak sampel sama sekali |
| `DEC-LAB-007` | Laporan manajerial laboratorium: jumlah pemeriksaan, angka penolakan sampel, rata-rata waktu penyelesaian | `NON_BLOCKING_STANDARD` | Belum pernah dibahas. Dapat ditambahkan setelah Rilis 1 tanpa mengubah struktur data, karena seluruh datanya sudah tersimpan |

---

## 5. Kesiapan per Bagian Pekerjaan

| Slice | Nama bagian | Kesiapan | Yang memblokir |
|---|---|---|---|
| `S1a` | Penandaan cito pada pesanan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S1b` | Penghapusan `Draft` dan jendela penyuntingan dokter | `BUSINESS_DECISION_REQUIRED` | `LAB-AMD-001` — pemilik `rawat-jalan` |
| `S2` | Siklus hidup sampel | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S3` | Batas nilai dan batas kritis | **`READY_FOR_DOMAIN_DESIGN`** | — (dibuka `LAB-DEC-021` dan `LAB-DEC-023`) |
| `S4` | Pengisian dan validasi hasil | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001` — tanda tangan klinis. Dua blocker lainnya sudah dibuka `LAB-DEC-021` dan `LAB-DEC-022` |
| `S5` | Nilai kritis dan pelaporannya | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-COORD-001` lewat `S8` |
| `S6` | Koreksi hasil setelah rilis | `BUSINESS_DECISION_REQUIRED` | `LAB-SIGN-001`, `LAB-COORD-002`, bergantung `S4` |
| `S7` | Daftar kerja dan pemantauan keterlambatan | **`READY_FOR_DOMAIN_DESIGN`** | — (bergantung `S1a` yang juga siap) |
| `S8` | Pemberitahuan tersimpan | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-001` — pemilik platform |
| `S9` | Pendaftaran hasil ke rekam medis | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-002` — pemilik `rekam-medis`, bergantung `S4` |
| `S10` | Fakta kelayakan tagih ke Billing | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S11` | Master alasan penolakan sampel | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S12` | Tampilan frontend | `PARTIALLY_READY` | Mengikuti kesiapan bagian backend yang dilayaninya |

**Perubahan sejak revision 1.** `S3` naik dari `BUSINESS_DECISION_REQUIRED` menjadi
`READY_FOR_DOMAIN_DESIGN`. `S4` masih tertahan, tetapi kini hanya oleh satu hal — tanda tangan
klinis — bukan lagi oleh tiga hal.

**Sifat seluruh blocker yang tersisa.** Tidak satu pun dapat diselesaikan pemilik modul
sendirian. Keempatnya memerlukan pihak lain: dokter penanggung jawab lab atau Komite Medis
(`LAB-SIGN-001`), pemilik platform (`LAB-COORD-001`), pemilik `rekam-medis` (`LAB-COORD-002`),
dan pemilik blueprint `rawat-jalan` (`LAB-AMD-001`). Artinya `grill-me` tidak akan membuka
apa-apa lagi tanpa kehadiran pihak-pihak itu.

### Ketergantungan antar bagian

| Bagian | Bergantung pada | Sifat ketergantungan |
|---|---|---|
| `S5` | `S3` | Tidak bisa menandai nilai kritis tanpa batas kritis |
| `S5` | `S8` | Pemberitahuan nilai kritis butuh sarana pemberitahuan |
| `S6` | `S4` | Tidak ada yang bisa dikoreksi sebelum ada hasil |
| `S6` | `S9` | Koreksi setelah kunjungan ditutup memakai addendum rekam medis |
| `S7` | `S1a` | Urutan cito butuh penanda cito |
| `S9` | `S4` | Yang didaftarkan ke rekam medis adalah hasil |
| `S12` | Seluruh bagian backend | Layar hanya bisa dirancang setelah perilakunya jelas |

---

## 6. Apa yang Boleh Berjalan

Enam bagian berikut boleh diteruskan ke `hospital-domain-architect` **sekarang**, karena tidak
ada keputusan bisnis pemblokir yang menghalanginya:

| Slice | Nama | Kenapa aman berjalan |
|---|---|---|
| `S1a` | Penandaan cito | `LAB-DEC-013` sudah lengkap: siapa yang menandai, bagaimana batas waktunya dihitung, dan apa yang dipantau. Tidak menyentuh modul lain |
| `S2` | Siklus hidup sampel | Sudah terbangun penuh dan terbukti 19 pengujian. Keputusannya diwarisi dan tidak ada yang terbuka |
| `S3` | Batas nilai dan batas kritis | **Baru terbuka.** `LAB-DEC-021` menetapkan dua bentuk hasil, `LAB-DEC-023` menetapkan siapa boleh mengubah apa. Struktur datanya kini utuh |
| `S7` | Daftar kerja dan pemantauan keterlambatan | Aturan urutan dan perhitungan keterlambatan sudah jelas. Pembagian daftar per meja kerja dinyatakan `CONFIGURABLE_DEFAULT` |
| `S10` | Fakta kelayakan tagih | Sudah terpasang, terhubung, dan teruji. Batas kewenangan finansial dijaga pengujian otomatis |
| `S11` | Master alasan penolakan | `LAB-DEC-019` menetapkan pengelola dan kolom mana yang terkunci. Data awalnya `NON_BLOCKING` |

Bagian `S12` boleh dirancang **hanya untuk keenam bagian di atas**.

Perlu dicatat bahwa `S3` adalah pekerjaan **baru yang bernilai besar**: tabel batas nilai adalah
fondasi yang harus berdiri sebelum bagian hasil pemeriksaan dapat dibangun. Terbukanya `S3`
membuat gelombang pertama tidak lagi berisi pekerjaan yang sudah selesai saja.

---

## 7. Apa yang Harus Berhenti

Enam bagian berikut **tidak boleh** masuk arsitektur domain sampai pihak yang berwenang
memberi jawabannya:

| Slice | Nama | Harus menunggu | Pihak yang dibutuhkan |
|---|---|---|---|
| `S1b` | Penghapusan `Draft` dan penyuntingan pesanan | `LAB-AMD-001` | Pemilik blueprint `rawat-jalan` + Billing |
| `S4` | Pengisian dan validasi hasil | `LAB-SIGN-001` | Dokter penanggung jawab lab atau Komite Medis |
| `S5` | Nilai kritis dan pelaporannya | `LAB-SIGN-001`, `LAB-COORD-001` | Pihak klinis + pemilik platform |
| `S6` | Koreksi hasil | `LAB-SIGN-001`, `LAB-COORD-002` | Pihak klinis + pemilik `rekam-medis` |
| `S8` | Pemberitahuan tersimpan | `LAB-COORD-001` | Pemilik platform |
| `S9` | Pendaftaran hasil ke rekam medis | `LAB-COORD-002` | Pemilik `rekam-medis` |

**Yang berubah sejak revision 1, dan artinya.** Pada penilaian pertama, tujuh bagian tertahan
dan lima yang boleh jalan sebagian besar adalah pekerjaan yang sudah selesai dibangun. Setelah
tiga gap ditutup pemilik modul, keadaannya berubah: `S3` — tabel batas nilai — terbuka, dan itu
pekerjaan baru yang menjadi fondasi seluruh bagian hasil.

**Yang tetap harus disadari dengan jujur.** Bagian hasil pemeriksaan itu sendiri (`S4` sampai
`S6`) masih tertahan, dan itu adalah inti `LAB-DEC-001`. Bedanya, `S4` kini hanya menunggu
**satu** hal — tanda tangan klinis — bukan lagi tiga hal. Dan tidak satu pun blocker yang
tersisa dapat diselesaikan lewat wawancara lanjutan, karena semuanya memerlukan pihak di luar
modul Laboratorium.

Tiga gap yang ditemukan gerbang ini — kewenangan validasi, bentuk hasil kualitatif, dan wewenang
atas batas kritis — semuanya menyentuh struktur data hasil yang belum ditulis sebaris pun.
Menemukannya sebelum tabel hasil terbentuk jauh lebih murah daripada menemukannya setelah
tabel itu terisi data pasien.

---

## 8. Keputusan yang Dibutuhkan dari Pemilik

Urutan pengerjaan yang disarankan, dari yang membuka paling banyak pekerjaan:

### Sudah selesai pada 2026-09-01

| Keputusan | Ditutup oleh | Membuka bagian |
|---|---|---|
| `DEC-LAB-002` — bentuk nilai hasil | `LAB-DEC-021` | `S3` ✅, membantu `S4` |
| `DEC-LAB-001` — pemegang kewenangan validasi dan rilis | `LAB-DEC-022` | Membantu `S4`, `S5`, `S6` |
| `DEC-LAB-003` — wewenang atas batas kritis | `LAB-DEC-023` | `S3` ✅, membantu `S5` |

### Masih dibutuhkan — seluruhnya dari pihak di luar modul

| Urutan | Keputusan | Pihak yang dibutuhkan | Membuka bagian | Kenapa didahulukan |
|---:|---|---|---|---|
| 1 | `LAB-SIGN-001` — tanda tangan klinis atas `LAB-DEC-003`, `LAB-DEC-004`, `LAB-DEC-007` | Dokter penanggung jawab lab atau Komite Medis | `S4`, `S5`, `S6` | Membuka tiga bagian sekaligus, dan `S4` hanya menunggu ini |
| 2 | `LAB-COORD-002` — jenis dokumen klinis baru | Pemilik `rekam-medis` | `S6`, `S9` | Membuka dua bagian |
| 3 | `LAB-COORD-001` — kepemilikan pemberitahuan | Pemilik platform | `S5`, `S8` | Membuka dua bagian |
| 4 | `LAB-AMD-001` — amandemen `rawat-jalan` | Pemilik `rawat-jalan` + Billing | `S1b` | Membuka satu bagian terkecil |

Keempatnya **tidak dapat** diselesaikan lewat `grill-me` bersama pemilik modul saja, karena
wewenangnya berada di tangan orang lain. Keempatnya juga dapat dikejar **bersamaan**, tidak
perlu berurutan.

---

## 9. Handoff

### Ke `hospital-domain-architect`

| Field | Nilai |
|---|---|
| Modul | `laboratorium` |
| Slice yang dikirim | `S1a`, `S2`, **`S3`**, `S7`, `S10`, `S11` |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, dinyatakan **independen** dari penilaian `PARTIALLY_READY` keseluruhan |
| Snapshot bukti | Decisions revision 10; capability map revision 1; BE `c87d9c0`; FE `688daff90` |
| Decision ID yang belum selesai | `LAB-SIGN-001`, `LAB-AMD-001`, `LAB-COORD-001`, `LAB-COORD-002` — **tidak satu pun menyentuh keenam slice yang dikirim** |
| Decision ID yang mengikat slice yang dikirim | `LAB-DEC-013` (`S1a`, `S7`), `LAB-INH-002` dan `LAB-INH-009` (`S2`, `S10`), `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021`, `LAB-DEC-023` (`S3`), `LAB-DEC-019` (`S11`) |
| Dependency | `S7` bergantung pada `S1a`; keduanya dikirim bersama. `S3` berdiri sendiri dan menjadi fondasi `S4` yang belum dikirim |
| Baseline rujukan | Tidak ada. `indonesia-hospital-domain-reference` **tidak dipakai** pada penilaian ini |
| Keluaran yang diharapkan | Bounded context, batas aggregate, ownership konsep, relasi, lifecycle, dan batas keselamatan klinis untuk keenam slice tersebut |

### Ke `grill-me`

| Field | Nilai |
|---|---|
| Status | **Selesai untuk putaran ini.** Closure pass putaran kedua sudah dijalankan pada 2026-09-01 |
| Yang ditutup | `DEC-LAB-001` → `LAB-DEC-022`; `DEC-LAB-002` → `LAB-DEC-021`; `DEC-LAB-003` → `LAB-DEC-023` |
| Kapan dipanggil lagi | Setelah salah satu dari `LAB-SIGN-001`, `LAB-COORD-001`, `LAB-COORD-002`, atau `LAB-AMD-001` dijawab pihak berwenang, dan jawabannya perlu diterjemahkan menjadi aturan modul |

---

## 10. Batas Dokumen Ini

Dokumen ini **tidak** membuat entity, ERD, kontrak API, migration, desain tampilan, task
implementasi, maupun perubahan ClickUp. Endpoint tidak disajikan di sini karena gerbang ini
menilai kebutuhan bisnis, bukan kontrak teknis; kontrak as-is yang berlaku ada di
`01-existing-capability-map.md` bagian **Kontrak As-Is**.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-01 | Penilaian pertama. 13 bagian pekerjaan dinilai terhadap 18 dimensi. Tiga gap pemblokir baru ditemukan: `DEC-LAB-001`, `DEC-LAB-002`, `DEC-LAB-003`. Lima bagian dinyatakan siap dirancang, tujuh harus berhenti. Kesiapan keseluruhan `PARTIALLY_READY` | `draft` |
| 2 | 2026-09-01 | Ketiga gap ditutup pemilik modul lewat `grill-me` closure pass putaran kedua (`LAB-DEC-021`, `LAB-DEC-022`, `LAB-DEC-023`). Dimensi 02, 06, dan 09 naik menjadi `CONFIRMED`. `S3` naik menjadi siap dirancang sehingga slice yang dikirim menjadi enam. Blocker tersisa empat, seluruhnya memerlukan pihak di luar modul | `draft` |
