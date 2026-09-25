# Laboratorium — Penilaian Kelengkapan Requirement

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Assessment ID | `LAB-RCG-001` |
| Revision | `11` — bagian 0F, 2026-09-25: sesudah putaran 19, **`S16a` naik `READY_FOR_DOMAIN_DESIGN`**; `S8` dilebur ke `S5`/`S6`; `S19` keluar dari desain; `S2b` dipecah tiga dengan dua penahan baru `DEC-LAB-028`, `DEC-LAB-029`. Sebelumnya `10` — bagian 0E, 2026-09-25: `S2b`, `S8`, `S16`, `S18`, `S19` dinilai ulang; kelimanya tetap `BUSINESS_DECISION_REQUIRED` dengan penahan bernama `DEC-LAB-022`..`027` dan `LAB-COORD-017`. Sebelumnya `9` — bagian 0D, 2026-09-25: `S4d-1` naik `READY_FOR_DOMAIN_DESIGN`; `S4d-2` dan `S4e` tertahan keputusan klinis baru. Sebelumnya `8` — bagian 0C, 2026-09-24: `S4` naik `READY_FOR_DOMAIN_DESIGN` untuk desain saja |
| Status | `draft` |
| **Kesiapan keseluruhan** | **`PARTIALLY_READY`** — **diperbarui revision 7.** Dua slice siap dikirim: `S4b` dan `S4c`. Catatan revision 6 tentang tiga penahan yang dapat ditutup pemilik modul **sudah dipakai habis pada 2026-09-18**, dan hasilnya tercatat 0B. **Sejak putaran itu, nol penahan tersisa yang dapat ditutup pemilik modul sendirian** — seluruhnya wewenang klinis atau manajemen rumah sakit. Lihat 0B.5 |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Masukan | **Revision 7:** `00-interview-decisions.md` revision 41; `blueprint-manifest.md` revision 56. **Revision 6:** `00-interview-decisions.md` revision 39; `01-existing-capability-map.md` revision 3; `blueprint-manifest.md` revision 54. **Revision 1-2:** decisions revision 14; capability map revision 1; `05-evidence-reconciliation.md` revision 2 |
| Tanggal penilaian | 2026-09-01; dinilai ulang 2026-09-17 (revision 6, sesudah `LAB-SIGN-001` ditutup); **dinilai ulang 2026-09-18** (revision 7, sesudah amendment pass putaran 6) |
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
>
> **Dan bagian ini sendiri sudah sebagian digantikan.** Baris `S4`, `S4b`, `S4c`, `S5`, dan `S6`
> pada peta 0.2 dinilai ulang 2026-09-17 — lihat **bagian 0A**. Baris lain peta 0.2 tetap
> berlaku apa adanya.

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
| `S2b` | Atribut wadah khas Patologi Anatomi dan Mikrobiologi | `BUSINESS_DECISION_REQUIRED` — **dinilai ulang revision 11, lihat 0F**: dipecah `S2b-1` lokasi/pola/metode, `S2b-2` seri Sitologi/FNAB, `S2b-3` fiksasi; blok dan slide keluar scope | `DEC-LAB-029`, `DEC-LAB-022`, `DEC-LAB-028` — `DEC-LAB-023` ✅ ditutup `LAB-DEC-161` |
| `S3` | Batas nilai dan batas kritis | **`READY_FOR_DOMAIN_DESIGN`** | — (hanya melayani hasil berbentuk angka dan pilihan) |
| `S4a` | **Pengisian** hasil Patologi Klinik | **`READY`** — dipecah `LAB-DEC-076`, 2026-09-17 | **Nol.** `LAB-DEC-005` (hasil diketik manual analis) berdiri tanpa caveat tanda tangan, setara `LAB-DEC-006` yang sudah dibangun penuh |
| `S4` | **Validasi dan rilis** hasil Patologi Klinik — cakupannya dipersempit `LAB-DEC-076` | `BUSINESS_DECISION_REQUIRED` — **dinilai ulang revision 7, lihat 0B** | ~~`LAB-SIGN-001`~~ ✅, ~~`LAB-P0-001`~~ ↓, ~~`LAB-P0-005`~~ ↓, ~~`LAB-P0-002`~~ ✅ — **tersisa `DEC-LAB-011`** |
| `S4b` | Pengisian hasil Mikrobiologi | ✅ **`READY_FOR_DOMAIN_DESIGN`** — **naik revision 7, lihat 0B.1** | **Nol.** `LAB-OPEN-017` dicabut sebagai penahan oleh `LAB-DEC-081` |
| `S4c` | Pengisian hasil Patologi Anatomi | ✅ **`READY_FOR_DOMAIN_DESIGN`** — naik revision 6, lihat 0A.9 | **Nol.** `DEC-LAB-013` ✅ ditutup `LAB-DEC-083`; `S2b` tetap menempel sebagai dependency, bukan penahan |
| `S4d` | **Validasi dan rilis** hasil Mikrobiologi | `BUSINESS_DECISION_REQUIRED` — **slice baru, didirikan `LAB-DEC-083`**. **Dinilai ulang revision 9, lihat 0D:** `S4d-1` (hasil bukan `Sementara`) `READY_FOR_DOMAIN_DESIGN`; `S4d-2` (hasil `Sementara`) tertahan `DEC-LAB-020` | `DEC-LAB-011`, `LAB-OPEN-034` |
| `S4e` | **Validasi dan rilis** hasil Patologi Anatomi | `BUSINESS_DECISION_REQUIRED` — **slice baru, didirikan `LAB-DEC-083`**. **Dinilai ulang revision 9, lihat 0D:** tetap `BUSINESS_DECISION_REQUIRED`, penahannya berganti menjadi `DEC-LAB-021` | `DEC-LAB-011`, `LAB-OPEN-034` |
| `S5` | Nilai kritis dan pelaporannya | `BUSINESS_DECISION_REQUIRED` — **dinilai ulang revision 6, lihat 0A** | ~~`LAB-SIGN-001`~~ ✅, ~~`LAB-COORD-001`~~ **`closed` 2026-09-01** — **tersisa `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012`** |
| `S6` | Koreksi hasil setelah rilis | `BUSINESS_DECISION_REQUIRED` — **dinilai ulang revision 7, lihat 0B.2** | ~~`LAB-SIGN-001`~~ ✅, ~~`LAB-COORD-002`~~ **`closed` 2026-09-01**, `LAB-P0-003` 🟡 sebagian — **tersisa `DEC-LAB-014`** |
| `S7` | Daftar kerja dan pemantauan keterlambatan | **`READY_FOR_DOMAIN_DESIGN`** | — (bergantung `S1a`) |
| ~~`S8`~~ | Pemberitahuan tersimpan | **DILEBUR ke `S5`/`S6`** oleh `LAB-DEC-163` — revision 11, lihat 0F | `LAB-COORD-017` kini melekat pada `S5` dan `S6` |
| `S9` | Pendaftaran hasil ke rekam medis | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-002` |
| `S10` | Fakta kelayakan tagih ke Billing | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S11` | Master alasan penolakan sampel | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S12` | Tampilan frontend | `PARTIALLY_READY` | Mengikuti bagian backend yang dilayaninya |
| `S13a` | Pendaftaran pasien **datang langsung** | **`READY_FOR_DOMAIN_DESIGN`** | — (`LAB-DEC-032`; kontrak antarmodul `LAB-COORD-003` diselesaikan saat desain) |
| `S13b` | Pendaftaran pasien **rujukan luar** | **`READY_FOR_DOMAIN_DESIGN`** | — (`LAB-DEC-035`; koordinasi data induk perujuk `LAB-COORD-004`) |
| `S14` | Katalog pemeriksaan, tarif, dan cakupan | **`READY_FOR_DOMAIN_DESIGN`** untuk bagian penyajian | Bagian aturan cakupan penjamin tertahan `LAB-P0-007` |
| `S15` | Monitoring per disiplin | **`READY_FOR_DOMAIN_DESIGN`** | — (bergantung `S2`) |
| `S16` | Laporan operasional | **Dipecah — revision 11, lihat 0F.** ✅ **`S16a` `READY_FOR_DOMAIN_DESIGN`** (tiga laporan, `LAB-DEC-159`); `S16b` `BUSINESS_DECISION_REQUIRED` | `S16b`: `DEC-LAB-025` |
| `S17` | Label, nota, dan pengiriman hasil ke pasien | `BUSINESS_DECISION_REQUIRED` | Privasi pengiriman belum diputuskan |
| `S18` | Penautan berkas hasil laboratorium eksternal | `BUSINESS_DECISION_REQUIRED` — **dinilai ulang revision 11, lihat 0F**: sisi Laboratorium lengkap (`LAB-DEC-157`, `LAB-DEC-158`) | Di luar modul: `DEC-LAB-016`, `LAB-COORD-018` |
| ~~`S19`~~ | Order dari MCU | **Tidak dirancang** — jalur umum (`LAB-DEC-164`); revision 11, lihat 0F | Ditutup sesudah verifikasi berjalan pemesanan dari kunjungan `MedicalCheckup` |

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

## 0A. Penilaian Ulang Revision 6 — Setelah Tanda Tangan Klinis Turun

> **Bagian ini menggantikan baris `S4`, `S4b`, `S4c`, `S5`, dan `S6` pada peta 0.2, dan
> menggantikan seluruh bagian 5 sampai 9 sejauh menyangkut kelima slice itu.** Bagian lain peta
> 0.2 tidak tersentuh penilaian ini.

**Pemicu.** `LAB-SIGN-001` ditutup 2026-09-17 oleh `LAB-DEC-079`: `LAB-DEC-003`, `LAB-DEC-004`,
dan `LAB-DEC-007` ditandatangani **apa adanya**, **per disiplin**, oleh `DR-LAB-001` (Patologi
Klinik), `DR-LAB-002` (Mikrobiologi Klinik), dan `DR-LAB-003` (Patologi Anatomi) — sesudah
`LAB-DEC-078` menetapkan ketiganya pada hari yang sama lewat `LAB-REQ-009`.

### 0A.1 Scope penilaian

| Field | Nilai |
|---|---|
| Modul | `laboratorium` (`LAB-BP-001`) |
| Slice yang dinilai ulang | `S4` validasi dan rilis Patologi Klinik; `S4b` pengisian hasil Mikrobiologi; `S4c` pengisian hasil Patologi Anatomi; `S5` nilai kritis dan pelaporannya; `S6` koreksi hasil setelah rilis |
| Yang **tidak** dinilai ulang | Seluruh slice lain pada peta 0.2. `S4a` sudah `READY` dan sudah dibangun; penilaian ini tidak membukanya kembali |
| Sifat | **Read-only** terhadap repository aplikasi. Nol entity, ERD, kontrak API, migration, desain layar, atau task diturunkan di sini |

### 0A.2 Bukti yang dipakai

| Wewenang | Bukti | Untuk pertanyaan |
|---:|---|---|
| 1 | `LAB-DEC-079` — tanda tangan klinis per disiplin, 2026-09-17 | Apakah aturan keselamatan sudah sah |
| 1 | `LAB-DEC-078` — penetapan tiga pemegang wewenang Clinical Governance | Siapa yang berwenang |
| 3 | `LAB-REQ-004` bagian 5, 6, dan 7.2 — dua penetapan dan tiga pertanyaan klinis yang **tidak** ikut dijawab | Apa yang tetap tertahan |
| 4 | `LAB-DEC-022`, `LAB-DEC-023`, `LAB-DEC-027` (BR-23), `LAB-DEC-005`, `LAB-DEC-061` | Kewenangan, batas kritis, bentuk hasil, alat, status |
| 6 | `contracts/permission-audit-matrix.md`; jalur `PUT /lab-examinations/{id}/result` yang dibangun `BE-LAB-43` | Apa yang sudah ada hari ini |

`indonesia-hospital-domain-reference` **tidak dipakai** pada penilaian ini. Seluruh pertanyaan
terjawab dari bukti berwenang milik rumah sakit sendiri, sehingga baseline rujukan tidak
diperlukan — dan ketiadaannya **bukan** bukti bahwa sebuah requirement tidak ada.

### 0A.3 Temuan kelengkapan pada dimensi yang berubah

Hanya dimensi yang **berpindah status** oleh tanda tangan ini yang ditulis. Dimensi lain tidak
tersentuh dan tetap berlaku sebagaimana penilaian sebelumnya.

| Dim | Dimensi | Sebelum | Sesudah | Alasan |
|---:|---|---|---|---|
| 07 | Aturan bisnis / validation | `PROPOSED` | **`CONFIRMED`** | Ketiga aturan keselamatan kini disahkan pihak klinis, bukan hanya pemilik modul |
| 09 | Peran / authorization | `PROPOSED` | **`PROPOSED`** — tidak berpindah | Prinsipnya sah (`LAB-DEC-003` + `LAB-DEC-022`), tetapi **siapa yang menetapkan pemegang kewenangan** masih kosong. Lihat `DEC-LAB-011` |
| 13 | Pembatalan / koreksi | `PROPOSED` | **`CONFIRMED` untuk prinsipnya**, `MISSING` untuk aturannya | `LAB-DEC-007` sah; `LAB-P0-003` aturan pembatalan dan koreksi tetap kosong |
| 17 | Dampak keselamatan klinis | **`PROPOSED`** | **`CONFIRMED`** | Inilah yang sesungguhnya dibeli tanda tangan ini. Dimensi 17 adalah satu-satunya yang naik penuh |
| 08 | Status / perubahan status | `MISSING` | **`MISSING`** — tidak berpindah | `LAB-DEC-003` menyiratkan validasi dan rilis adalah dua tindakan berbeda, tetapi **tidak menetapkan keduanya sebagai status**. Urutan status hasil resmi tetap `LAB-P0-002` |

> **Satu hal perlu dinyatakan supaya tidak salah baca.** Tanda tangan ini menaikkan dimensi
> **keselamatan klinis**, bukan dimensi **lifecycle** maupun **authorization**. Ketiganya sering
> dikira satu hal. Yang disahkan adalah *apa yang benar secara klinis*; yang masih kosong adalah
> *bagaimana sistem menegakkannya* — status apa, dan diberikan siapa kepada siapa.

### 0A.4 Butir `CONFIRMED`, `PROPOSED`, `MISSING`, `CONFLICT`

| Butir | Status | Bukti / alasan |
|---|---|---|
| Prinsip empat mata pada validasi dan rilis | **`CONFIRMED`** | `LAB-DEC-003` ditandatangani `LAB-DEC-079` |
| Nilai kritis tetap dirilis; pelaporan wajib tercatat | **`CONFIRMED`** | `LAB-DEC-004` ditandatangani. **Kerangkanya saja** — daftar isinya tidak |
| Koreksi hasil hanya oleh pemegang kewenangan validasi/rilis | **`CONFIRMED`** | `LAB-DEC-007` ditandatangani |
| Bentuk hasil Patologi Anatomi: makroskopik, mikroskopik, kesimpulan — ketiganya wajib, gambar ≤ 2 MB | **`CONFIRMED`** | BR-23 / `LAB-DEC-027`, `approved` |
| Bentuk hasil Mikrobiologi berstruktur: organisme, antibiotik, kadar, zona mm, `R`/`I`/`S` | **`CONFIRMED`** | BR-23 / `LAB-DEC-027` |
| **Makna penanda `Definitif`** pada hasil Mikrobiologi | **`MISSING`** | `LAB-OPEN-017`, terbuka sejak 2026-09-01. Ia bagian dari struktur hasil Mikrobiologi itu sendiri |
| Sambungan ke alat laboratorium untuk Rilis 1 | **`CONFIRMED` — tidak ada** | `LAB-DEC-005` menetapkannya ditunda **dan** struktur data Rilis 1 sengaja tidak menyediakan tempatnya. `S4a` sudah dibangun di atas keputusan itu |
| Siapa yang berwenang **menetapkan** pemegang kewenangan validasi | **`MISSING`** | `LAB-REQ-004` bagian 5.1 tidak dijawab. `LAB-DEC-022` menetapkan kewenangan diberikan **per orang, bukan per jabatan** — maka harus ada pemberinya, dan pemberinya belum ada |
| Jaminan dua pemegang kewenangan validasi per shift | **`MISSING`** | `LAB-REQ-004` bagian 5.1 tidak dijawab |
| Siapa pemberi persetujuan klinis atas perubahan batas kritis | **`MISSING`** | `LAB-REQ-004` bagian 5.2 tidak dijawab. `AC-33` tetap tidak dapat dibangun |
| Batas waktu tanggap, eskalasi, dan bukti penerimaan nilai kritis | **`MISSING`** | `LAB-P0-004` |
| Apakah Mikrobiologi dan Patologi Anatomi mengenal nilai kritis | **`MISSING`** | `LAB-OPEN-014`. BR-23 sudah menyatakan keduanya **tidak dapat** dinilai lewat perbandingan angka |
| Urutan status hasil resmi — `Validated`, `Released` | **`MISSING`** | `LAB-P0-002`. Komentar `LabExaminationStatus` menahannya secara sengaja, dan `LAB-DEC-076` mempertahankan disiplin itu |
| Aturan pembatalan dan koreksi | **`MISSING`** | `LAB-P0-003` |
| **Apakah kewenangan validasi/rilis melintasi disiplin** | **`MISSING`** | `LAB-OPEN-034`, **baru 2026-09-17**, lahir dari bentuk per-disiplin tanda tangannya |
| **Validasi dan rilis untuk Mikrobiologi dan Patologi Anatomi tidak punya slice** | **`MISSING`** | Lihat `DEC-LAB-013`. Ini temuan peta, bukan temuan requirement |
| Pertentangan antarbukti | **Nol `CONFLICT`** | Tidak ada sumber berwenang yang saling bertentangan pada kelima slice ini |

### 0A.5 Temuan yang paling mahal dari penilaian ini

> **`S4b` dan `S4c` bernama *"pengisian hasil"*, dan tidak ada slice mana pun yang memuat
> validasi serta rilis untuk Mikrobiologi dan Patologi Anatomi.**
>
> `LAB-DEC-076` memecah `S4` menjadi `S4a` (pengisian Patologi Klinik) dan `S4` (validasi dan
> rilis Patologi Klinik). Pemecahan itu benar dan beralasan — tetapi ia dilakukan **hanya untuk
> Patologi Klinik**. `S4b` dan `S4c` tidak ikut dipecah, dan keduanya sejak semula hanya bernama
> pengisian.
>
> Akibatnya: hasil Mikrobiologi dan Patologi Anatomi punya slice untuk **diisi**, nol slice untuk
> **dinyatakan sah**. Sebelum 2026-09-17 celah ini tidak terlihat, karena `LAB-SIGN-001` menahan
> semuanya sekaligus sehingga tidak ada yang perlu dibedakan.
>
> **Tanda tangan per disiplin membuatnya terlihat, dan sekaligus membuatnya mendesak:**
> `DR-LAB-002` dan `DR-LAB-003` kini memegang wewenang klinis atas disiplin yang **tidak punya
> tempat untuk menjalankan wewenang itu**. Ini persis kesalahan yang sudah dibayar modul ini
> lewat `BE-EXT-04` — sebuah sisi berdiri tanpa sisi lainnya — hanya terbalik arahnya.

### 0A.6 Decision Log

Tiga decision ID baru. Ketiganya **bergantung pemilik** dan tidak dijawab di sini.

---

**Decision ID:** `DEC-LAB-011`

**Pertanyaan:** Siapa yang berwenang menetapkan seseorang sebagai pemegang kewenangan validasi,
dan dapatkah rumah sakit menjamin sekurang-kurangnya dua pemegang kewenangan pada setiap shift?

**Kemampuan yang terdampak:** `S4` validasi dan rilis. Menyentuh juga `S4b`/`S4c` bila
`DEC-LAB-013` dijawab dengan memecah keduanya.

**Bukti saat ini:** `LAB-DEC-022` menetapkan kewenangan validasi diberikan **per orang, bukan
per jabatan**, dan setiap shift wajib punya minimal dua pemegang. `LAB-REQ-004` bagian 5.1
menanyakan pemberinya dan jaminannya; bagian itu **tidak dijawab** pada penandatanganan
2026-09-17.

**Usulan baseline:** Tidak ada. Mengusulkan jabatan tertentu berarti menebak tata kelola rumah
sakit, dan `LAB-DEC-022` justru menolak pendekatan per jabatan.

**Dampak:** Model authorization, state machine validasi, dan keselamatan klinis. `LAB-REQ-004`
bagian 5.1 sudah menuliskan kenapa: bila sebuah shift hanya punya satu pemegang kewenangan,
jalur pengecualian empat mata berubah menjadi jalur utama — **pengujiannya tetap lulus dan tidak
ada aturan yang dilanggar**, tetapi prinsipnya berhenti berarti apa pun.

**Pemilik:** Kepala instalasi laboratorium + manajemen rumah sakit. Bukan pemilik modul.

**Status:** `OPEN`

**Dampak implementasi/domain:** `S4` terblokir. Tanpa pemberi kewenangan, sistem tidak punya
cara sah memberikan hak validasi kepada seseorang — dan `LAB-DEC-022` melarang menurunkannya
dari jabatan.

---

**Decision ID:** `DEC-LAB-012`

**Pertanyaan:** Siapa pemegang wewenang menyetujui perubahan batas kritis, bolehkah wewenang itu
didelegasikan, dan cukupkah satu orang atau perlu rapat?

**Kemampuan yang terdampak:** `S3` jalur persetujuan perubahan batas (`AC-33`), dan `S5`.

**Bukti saat ini:** `LAB-DEC-023` mensyaratkan "persetujuan klinis" untuk perubahan batas kritis
dan penanda pilihan yang dianggap kritis. `LAB-REQ-004` bagian 5.2 menanyakan siapa pengisinya;
**tidak dijawab**.

**Usulan baseline:** `LAB-DEC-078` kini menyediakan **kandidat yang sebelumnya tidak ada** —
ketiga pemegang wewenang Clinical Governance per disiplin. Ini `PROPOSED`, bukan `CONFIRMED`:
menyetujui aturan keselamatan dan menyetujui perubahan angka batas adalah dua wewenang berbeda,
dan tidak seorang pun menyatakan keduanya melekat pada orang yang sama.

**Dampak:** Authorization, keselamatan klinis, integritas data batas nilai. `LAB-REQ-004` bagian
5.2 menuliskan keadaan yang dicegah: batas kritis Kalium dinaikkan dari 6,0 menjadi 8,0, dan
sejak itu pasien dengan 7,2 mmol/L berhenti memicu kewajiban pelaporan — tanpa satu pun aturan
dilanggar.

**Pemilik:** Pihak klinis — kemungkinan besar `DR-LAB-001`/`002`/`003`, tetapi **perlu
dinyatakan**.

**Status:** `OPEN`

**Dampak implementasi/domain:** `AC-33` tetap tidak dapat dibangun. `S3` sudah
`READY_FOR_DOMAIN_DESIGN` untuk strukturnya dan **tetap begitu** — yang tertahan hanya jalur
persetujuan perubahannya.

---

**Decision ID:** `DEC-LAB-013`

**Pertanyaan:** Apakah validasi dan rilis untuk Mikrobiologi dan Patologi Anatomi menjadi slice
tersendiri seperti `S4` bagi Patologi Klinik, atau ketiganya ditangani satu slice validasi
lintas disiplin?

**Kemampuan yang terdampak:** `S4b`, `S4c`, dan bentuk akhir `S4`.

**Bukti saat ini:** `LAB-DEC-076` memecah `S4` menjadi `S4a` + `S4` **hanya untuk Patologi
Klinik**. `S4b` dan `S4c` tetap bernama *"pengisian hasil"*. Nol slice memuat validasi dan rilis
untuk kedua disiplin itu. Celah ini tidak pernah terlihat selama `LAB-SIGN-001` menahan kelimanya
sekaligus.

**Usulan baseline:** Cerminkan pemecahan `LAB-DEC-076` pada kedua disiplin lain — `S4b` menjadi
pengisian + validasi/rilis Mikrobiologi, `S4c` demikian pula. `PROPOSED`.

**Dampak:** Cakupan slice, urutan pekerjaan, dan bentuk `LAB-PERM-v1`. **Bertaut erat dengan
`LAB-OPEN-034`**: bila kewenangan validasi tidak melintasi disiplin, ketiganya memang harus
terpisah; bila melintas, satu slice lintas disiplin lebih tepat.

**Pemilik:** Yoga Aji Pratama, bersama ketiga pemegang wewenang klinis.

**Status:** `OPEN`

**Dampak implementasi/domain:** **Tidak** memblokir bagian **pengisian** `S4b`/`S4c`. Memblokir
bagian validasi dan rilis keduanya, yang hari ini tidak punya tempat sama sekali.

---

Satu open question yang sudah tercatat di luar gerbang ini juga mengikat kelima slice, dan
diulang di sini supaya tidak hilang:

| ID | Pertanyaan | Dampak |
|---|---|---|
| `LAB-OPEN-034` | Apakah kewenangan validasi dan rilis melintasi disiplin — bolehkah validator Patologi Klinik memvalidasi hasil Mikrobiologi | **`BLOCKING`** bagi bentuk penegakan kewenangan `S4b`/`S4c`. Menyentuh `LAB-DEC-022`: jaminan dua pemegang per shift menjadi jauh lebih mahal bila harus dipenuhi **per disiplin** — satu ahli Patologi Anatomi yang cuti berarti disiplin itu berhenti sepenuhnya |

### 0A.7 Klasifikasi dampak gap

| Butir | Dampak | Alasan |
|---|---|---|
| `DEC-LAB-011` penetapan pemegang kewenangan validasi | **`BLOCKING`** | Mengubah model authorization dan lifecycle validasi |
| `LAB-P0-002` urutan status hasil resmi | **`BLOCKING`** | Mengubah state machine — kriteria eksplisit kontrak gerbang |
| `LAB-OPEN-017` makna penanda `Definitif` | **`BLOCKING`** | Ia ruas di dalam struktur hasil Mikrobiologi, bukan hiasan di atasnya |
| `LAB-P0-004` alur nilai kritis lengkap | **`BLOCKING`** | Tanpa batas waktu tanggap, daftar pantau dapat dibangun tetapi **tidak dapat menandai mana yang terlambat** |
| `LAB-OPEN-014` nilai kritis Mikrobiologi dan PA | **`BLOCKING`** | Menentukan apakah alur nilai kritis punya dua cabang atau satu |
| `LAB-P0-003` aturan pembatalan dan koreksi | **`BLOCKING`** | Alur koreksi menyentuh data yang sudah dipakai dokter |
| `LAB-OPEN-034` kewenangan lintas disiplin | **`BLOCKING`** bagi `S4b`/`S4c` | Authorization dan kelayakan operasional `LAB-DEC-022` |
| `DEC-LAB-013` slice validasi Mikro dan PA | **`BLOCKING`** bagi bagian validasi keduanya | Cakupan dan ownership slice |
| `DEC-LAB-012` persetujuan perubahan batas kritis | **`BLOCKING`** bagi `AC-33` dan `S5` | Authorization dan integritas batas nilai |
| `LAB-P0-005` integrasi alat laboratorium | **`NON_BLOCKING_STANDARD`** untuk Rilis 1 | **Diturunkan dari `BLOCKING`.** `LAB-DEC-005` sudah memutuskannya: ditunda, dan struktur data Rilis 1 sengaja tidak menyediakan tempatnya. `S4a` sudah dibangun di atas keputusan itu. Tetap terbuka untuk rilis berikutnya |
| `LAB-P0-001` sisa matriks kewenangan | **`NON_BLOCKING_STANDARD`** bagi `S4`, `S4b`, `S4c` | **Diturunkan dari `BLOCKING`.** Bagiannya yang menyangkut validasi dan rilis sudah dijawab `LAB-DEC-022` dan disahkan `LAB-DEC-079`. Sisanya — menerima wadah, memproses sampel, membatalkan, menghapus — milik slice lain. Kewenangan **mengisi** hasil sudah berjalan lewat `LabExamination : Update` sejak `BE-LAB-43`, dan memblokir `S4c` atasnya berarti menerapkan ukuran yang tidak diterapkan pada `S4a` |

### 0A.8 Kesiapan per slice

| Slice | Sebelum | **Sesudah** | Penahan yang tersisa |
|---|---|---|---|
| `S4` validasi dan rilis Patologi Klinik | `BUSINESS_DECISION_REQUIRED` — 4 penahan | **`BUSINESS_DECISION_REQUIRED`** — **2 penahan** | `DEC-LAB-011`, `LAB-P0-002` |
| `S4b` pengisian hasil Mikrobiologi | `BUSINESS_DECISION_REQUIRED` — 3 penahan | **`BUSINESS_DECISION_REQUIRED`** — **1 penahan** | `LAB-OPEN-017` |
| `S4c` pengisian hasil Patologi Anatomi | `BUSINESS_DECISION_REQUIRED` — 2 penahan | ✅ **`READY_FOR_DOMAIN_DESIGN`** | **Nol.** Lihat 0A.9 |
| `S5` nilai kritis dan pelaporannya | `BUSINESS_DECISION_REQUIRED` — 4 penahan | **`BUSINESS_DECISION_REQUIRED`** — **3 penahan** | `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` |
| `S6` koreksi hasil setelah rilis | `BUSINESS_DECISION_REQUIRED` — 3 penahan | **`BUSINESS_DECISION_REQUIRED`** — **1 penahan** | `LAB-P0-003` |

**Dua koreksi pembukuan ikut ditemukan, dan keduanya membuat keadaan lebih baik daripada yang
tercatat.** `LAB-COORD-001` dan `LAB-COORD-002` masih tertulis sebagai penahan `S5` dan `S6`
pada bagian 5 dan 7 dokumen ini. Keduanya **`closed` sejak 2026-09-01**, disetujui
`andryzainhome` dan `sukmagp` lewat `LAB-REQ-001` butir 7 dan 8. `LAB-REQ-004` bagian 7.1 sudah
mencatat koreksi ini pada 2026-09-09; bagian 5 dan 7 di bawah tidak pernah ikut diperbarui.

**Kesiapan keseluruhan tetap `PARTIALLY_READY`** — satu slice naik, empat menyempit, nol slice
mundur.

### 0A.9 Kenapa `S4c` naik, dan apa yang ikut menempel padanya

`S4c` adalah satu-satunya dari kelima slice yang seluruh dimensinya kini terisi:

| Dimensi | Keadaan `S4c` |
|---|---|
| Bentuk dan data minimum hasil | **`CONFIRMED`** — BR-23: makroskopik, mikroskopik, dan kesimpulan wajib; gambar contoh dibatasi 2 MB |
| Aturan keselamatan | **`CONFIRMED`** — ditandatangani `DR-LAB-003` untuk disiplinnya sendiri |
| Kewenangan mengisi | Berjalan lewat model `resource : action` yang sudah ada, sama seperti `S4a` |
| Nilai kritis | **Tidak berlaku** bagi pengisian. BR-23 sudah menyatakan narasi PA tidak dapat dinilai lewat perbandingan angka; percabangannya urusan `S5`, bukan `S4c` |

**Dua hal menempel padanya dan sengaja saya sebut, karena keduanya bukan penahan tetapi akan
terasa bila diabaikan:**

1. **`S2b` — atribut wadah khas Patologi Anatomi** (penomoran blok parafin, slide, sitologi,
   FNAB) masih `BUSINESS_DECISION_REQUIRED`. `S4c` dapat **dirancang** tanpa itu, sebab hasil
   melekat pada pemeriksaan, bukan pada skema penomoran wadah. Tetapi `S4c` yang dibangun
   sementara `S2b` kosong akan berguna separuh saja di lapangan.
2. **`DEC-LAB-013`** — `S4c` adalah **pengisian saja**. Hasil Patologi Anatomi yang sudah diisi
   belum punya jalan menjadi sah, persis seperti keadaan Patologi Klinik sebelum hari ini.
   Merancang `S4c` tanpa menjawab `DEC-LAB-013` mengulang keadaan yang baru saja dibayar mahal.

### 0A.10 Apa yang boleh berjalan

| Yang boleh | Catatan |
|---|---|
| `S4c` diteruskan ke `hospital-domain-architect` | Dinyatakan **independen** dari verdict `PARTIALLY_READY` keseluruhan |
| Seluruh slice yang sudah `READY` sebelumnya | Tidak tersentuh penilaian ini |

### 0A.11 Apa yang harus berhenti

| Yang berhenti | Menunggu | Pihak yang dibutuhkan |
|---|---|---|
| `S4` validasi dan rilis Patologi Klinik | `DEC-LAB-011`, `LAB-P0-002` | Kepala instalasi + manajemen RS; pemilik modul untuk urutan status |
| `S4b` pengisian hasil Mikrobiologi | `LAB-OPEN-017` | Yoga Aji Pratama |
| `S5` nilai kritis | `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` | Ketiga pemegang wewenang klinis |
| `S6` koreksi hasil | `LAB-P0-003` | Yoga Aji Pratama + pihak klinis |
| Bagian **validasi dan rilis** Mikrobiologi dan Patologi Anatomi | `DEC-LAB-013`, `LAB-OPEN-034` | Pemilik modul + ketiga pemegang wewenang klinis |

### 0A.12 Keputusan yang dibutuhkan dari pemilik

Diurutkan dari yang membuka paling banyak.

| Urutan | Keputusan | Membuka | Kenapa didahulukan |
|---:|---|---|---|
| 1 | `LAB-P0-002` — urutan status hasil resmi | `S4`, dan bentuk `S4b`/`S4c` berikutnya | Satu-satunya penahan `S4` yang dapat dijawab **pemilik modul sendiri**. Ia juga prasyarat diam-diam bagi seluruh slice hasil lainnya |
| 2 | `DEC-LAB-011` — pemberi kewenangan validasi dan jaminan dua per shift | `S4` | Penahan `S4` yang kedua; memerlukan pihak di luar modul |
| 3 | `LAB-OPEN-017` — makna penanda `Definitif` | `S4b` | Penahan **tunggal**; pertanyaan terkecil dengan hasil terbesar |
| 4 | `LAB-P0-003` — aturan pembatalan dan koreksi | `S6` | Penahan **tunggal** |
| 5 | `DEC-LAB-013` + `LAB-OPEN-034` — slice dan kewenangan lintas disiplin | Validasi/rilis Mikro dan PA | Sebaiknya dijawab bersama; keduanya pertanyaan yang sama dari dua sisi |
| 6 | `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` | `S5` | Paling banyak penahannya, dan seluruhnya wewenang klinis |

### 0A.13 Handoff

**Ke `hospital-domain-architect`:**

| Field | Nilai |
|---|---|
| Modul | `laboratorium` (`LAB-BP-001`) |
| Slice yang dikirim | **`S4c`** — pengisian hasil Patologi Anatomi |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, dinyatakan **independen** dari verdict `PARTIALLY_READY` keseluruhan |
| Snapshot bukti | Decisions revision 39; capability map revision 3; manifest revision 54; BE `13665452`; FE `686038858` |
| Decision ID yang mengikat slice ini | `LAB-DEC-027` (BR-23, bentuk hasil), `LAB-DEC-079` (tanda tangan `DR-LAB-003`), `LAB-DEC-005` (hasil diketik manual) |
| Decision ID yang belum selesai dan **tidak** menyentuhnya | `DEC-LAB-011`, `LAB-P0-002`, `LAB-OPEN-017`, `LAB-P0-003`, `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` |
| Dependency yang **harus dibawa** | `S2b` belum siap — rancangan tidak boleh mengandaikan skema penomoran wadah PA. `DEC-LAB-013` belum dijawab — rancangan **tidak boleh** mengarang jalur validasi/rilis PA |
| Baseline rujukan | Tidak ada. `indonesia-hospital-domain-reference` tidak dipakai |
| Keluaran yang diharapkan | Bounded context, batas aggregate, ownership, relasi, lifecycle pengisian, dan batas keselamatan klinis untuk `S4c` saja |

**Ke `grill-me`:**

| Field | Nilai |
|---|---|
| Kapan dipanggil | **Sekarang, dan ia akan produktif** — berbeda dari keadaan revision 2 |
| Yang dapat ditutup pemilik modul sendirian | `LAB-P0-002` (urutan status hasil), `LAB-OPEN-017` (makna `Definitif`), `LAB-P0-003` (aturan pembatalan dan koreksi), serta sisi pemilik-modul dari `DEC-LAB-013` |
| Yang **tidak** dapat ditutup pemilik modul | `DEC-LAB-011`, `DEC-LAB-012`, `LAB-P0-004`, `LAB-OPEN-014`, `LAB-OPEN-034` — seluruhnya wewenang klinis atau manajemen |

> **Inilah perubahan terbesar dari penilaian ini, dan ia tidak terlihat dari tabel kesiapan.**
> Revision 2 mencatat: *"`grill-me` tidak akan membuka apa-apa lagi tanpa kehadiran pihak-pihak
> itu"* — seluruh penahan saat itu berada di luar wewenang pemilik modul. **Hari ini tidak lagi
> begitu.** Tiga dari sembilan penahan yang tersisa dapat dijawab pemilik modul dalam satu sesi
> wawancara, dan ketiganya masing-masing adalah penahan **tunggal** atau **terakhir** bagi
> slice-nya. Wawancara pemilik kini kembali menjadi jalur termurah.

### 0A.14 Batas penilaian ini

Gerbang ini **tidak** membuat entity, ERD, kontrak API, migration, desain layar, task
implementasi, maupun perubahan ClickUp. Ia juga **tidak** menjawab satu pun decision ID yang
dibukanya atas nama rumah sakit.

Basis data **tidak** diperiksa — nol klien SQL tersedia pada lingkungan penilaian ini. Seluruh
pernyataan tentang apa yang sudah ada diambil dari source, kontrak terkunci, dan laporan task
yang buktinya tertulis.

---

## 0B. Penilaian Ulang Revision 7 — Setelah Amendment Pass Putaran 6

> **Bagian ini menggantikan tabel kesiapan 0A.8.** Isi 0A dipertahankan sebagai rekam jejak
> penilaian pada hari `LAB-SIGN-001` ditutup.

**Pemicu.** Amendment pass putaran 6 pada 2026-09-18 menjawab **empat** butir yang 0A.13 tunjuk
sebagai milik pemilik modul: `LAB-DEC-080` sampai `LAB-DEC-083`.

### 0B.1 Kesiapan per slice

| Slice | `r6` | **`r7`** | Penahan yang tersisa |
|---|---|---|---|
| `S4` validasi dan rilis Patologi Klinik | 2 penahan | `BUSINESS_DECISION_REQUIRED` — **1 penahan** | `DEC-LAB-011` |
| `S4b` pengisian hasil Mikrobiologi | 1 penahan | ✅ **`READY_FOR_DOMAIN_DESIGN`** — sempat dicabut, **pulih** 2026-09-18 malam | **nol.** `DEC-LAB-015` ditutup `LAB-DEC-084`. Lihat 0B.6 |
| `S4c` pengisian hasil Patologi Anatomi | ✅ `READY` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | **nol** — dependency `DEC-LAB-013` yang 0A.9 sebut kini **tertutup** |
| `S4d` validasi dan rilis Mikrobiologi | **tidak ada** | `BUSINESS_DECISION_REQUIRED` | `DEC-LAB-011`, `LAB-OPEN-034` |
| `S4e` validasi dan rilis Patologi Anatomi | **tidak ada** | `BUSINESS_DECISION_REQUIRED` | `DEC-LAB-011`, `LAB-OPEN-034` |
| `S5` nilai kritis dan pelaporannya | 3 penahan | `BUSINESS_DECISION_REQUIRED` — **3 penahan** | `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` |
| `S6` koreksi hasil setelah rilis | 1 penahan | `BUSINESS_DECISION_REQUIRED` — **1 penahan, kini bernama tepat** | `DEC-LAB-014` |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`.** Dua slice kini siap dikirim, naik dari satu.

### 0B.2 Satu koreksi atas gerbang `r6` sendiri

> **0A.13 menyatakan `LAB-P0-003` dapat ditutup pemilik modul sendirian. Itu keliru, dan
> wawancara yang membuktikannya.**
>
> `LAB-REQ-004` bagian 4.5 memuat **empat** pertanyaan turunan `LAB-DEC-007`, dan **tiga di
> antaranya bersifat klinis**: apakah kewenangan validasi/rilis cukup untuk mengoreksi, siapa
> lagi yang diberi tahu selain dokter pemesan, dan adakah batas waktu koreksi. Gerbang `r6`
> membaca `LAB-P0-003` dari judulnya — *"aturan pembatalan dan koreksi"* — dan menyimpulkan ia
> sejenis dengan `LAB-DEC-063` yang memang keputusan pemilik modul.
>
> Yang ditutup pemilik modul hari ini **nyata tetapi sebagian**: bentuk alasan dan bentuk versi.
> Sisanya dipindahkan ke `DEC-LAB-014` dengan pemilik yang benar. **`S6` karena itu tidak
> terbuka**, dan penilaian `r6` yang menempatkannya pada urutan 4 daftar prioritas 0A.12
> **terlalu optimistis**.

### 0B.3 Butir yang berubah status

| Butir | `r6` | **`r7`** | Sebabnya |
|---|---|---|---|
| Urutan status hasil resmi (`LAB-P0-002`) | `MISSING` / `BLOCKING` | **`CONFIRMED`** | `LAB-DEC-080` — dicatat sebagai fakta, nol status hasil |
| Makna penanda `Definitif` (`LAB-OPEN-017`) | `MISSING` / `BLOCKING` | **`MISSING` / `NON_BLOCKING_STANDARD`** | `LAB-DEC-081` — penandanya dikeluarkan dari Rilis 1, sesuai penempatan Rilis 2 yang **sudah tercatat sejak semula**. Penilaian `BLOCKING` pada `r6` benar sebagai pernyataan struktur, keliru sebagai penahan rilis |
| Slice validasi Mikro dan PA (`DEC-LAB-013`) | `MISSING` / `BLOCKING` | **`CONFIRMED`** | `LAB-DEC-083` — `S4d` dan `S4e` didirikan |
| Bentuk alasan dan versi koreksi | bagian dari `LAB-P0-003` | **`CONFIRMED`** | `LAB-DEC-082` |
| Sisa klinis koreksi | tidak terpisah | **`MISSING` / `BLOCKING`** | `DEC-LAB-014`, baru |

### 0B.4 Penahan yang kini paling mahal

> **`DEC-LAB-011` menahan ketiga slice validasi sekaligus — `S4`, `S4d`, dan `S4e`.**
>
> Sesudah putaran ini ia berdiri sendirian di depan **seluruh kemampuan merilis hasil**, untuk
> ketiga disiplin. Pertanyaannya tetap yang paling sederhana dari semuanya: **siapa yang berhak
> menunjuk seseorang sebagai pemegang kewenangan validasi.**
>
> Ia sudah ditanyakan sejak 2026-09-09 lewat `LAB-REQ-004` bagian 5.1, dan tidak ikut dijawab
> ketika tanda tangannya turun. Ini kedua kalinya modul ini berhenti bukan karena keberatan atas
> isinya, melainkan karena satu pertanyaan penunjukan tidak sampai ke orang yang tepat.
>
> **Diajukan ulang tersendiri pada 2026-09-18 lewat `LAB-REQ-013`**, kepada kepala instalasi,
> memuat **dua pertanyaan saja** — pola `LAB-REQ-009` yang terbukti dijawab dalam hitungan jam,
> bukan pola `LAB-REQ-004` yang membundel delapan butir dan hanya tiga di antaranya turun.

### 0B.5 Handoff

**Ke `hospital-domain-architect`:**

| Field | Nilai |
|---|---|
| Slice yang dikirim | **`S4b`** pengisian hasil Mikrobiologi, dan **`S4c`** pengisian hasil Patologi Anatomi |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, **independen** dari verdict keseluruhan |
| Snapshot bukti | Decisions revision 41; capability map revision 3; manifest revision 56; BE `13665452`; FE `686038858` |
| Decision ID yang mengikat | `LAB-DEC-027` (BR-23 bentuk hasil), `LAB-DEC-079` (tanda tangan per disiplin), `LAB-DEC-080` (fakta, bukan status), `LAB-DEC-081` (`Definitif` di luar Rilis 1), `LAB-DEC-083` (`S4d`/`S4e` terpisah) |
| Batas yang **wajib dihormati** | Keduanya **pengisian saja**. Jalur validasi dan rilis **tidak boleh dirancang** di sini — itu `S4d` dan `S4e`, dan keduanya masih tertahan `DEC-LAB-011` serta `LAB-OPEN-034` |
| Yang **tidak boleh** muncul dalam rancangan | Status hasil dalam bentuk apa pun (`LAB-DEC-080`); penanda `Definitif` (`LAB-DEC-081`); penilaian kritis otomatis atas hasil Mikrobiologi maupun narasi PA (BR-23 menyatakan keduanya tidak dapat dinilai lewat perbandingan angka) |

### 0B.6 Koreksi 2026-09-18 sore — `S4b` dicabut dari daftar siap

> **Gerbang ini menyatakan `S4b` `READY_FOR_DOMAIN_DESIGN` dengan nol penahan. Arsitektur domain
> menemukan satu penahan yang saya lewatkan, dan penilaian itu saya cabut.**
>
> BR-23 memerinci bentuk hasil Mikrobiologi berstruktur — status temuan, **organisme per
> bakteri, antibiotik**, kadar, zona dalam mm, dan `R`/`I`/`S`. Saya membacanya sebagai bentuk
> hasil yang sudah lengkap. Yang tidak saya tanyakan: **apakah organisme dan antibiotik itu data
> induk terkendali, dan siapa pemiliknya.**
>
> BR-23 tidak pernah menyatakannya, dan jawabannya mengubah hal-hal yang material: identitas
> konsep, invariant, bentuk jejak audit, dan mungkin-tidaknya antibiogram rumah sakit disusun.
> Bila teks bebas, satu kuman yang sama akan tertulis `E. coli`, `E.coli`, `Escherichia coli`,
> dan `eschericia coli` — **empat tulisan, satu kuman**. Itu alasan yang **sama persis** dengan
> yang dipakai pemilik modul menutup `LAB-DEC-082` pada alasan koreksi, dan saya tidak
> menerapkannya di sini.
>
> Dibuka `DEC-LAB-015`, `BLOCKING` bagi `S4b`. **`S4c` tidak terkena hal yang sama**, dan
> sebabnya sederhana: narasi Patologi Anatomi **tidak menunjuk data induk apa pun**.
>
> **Ini koreksi ketiga atas gerbang `r6`/`r7` dalam dua hari.** Ketiganya lahir dari sumber yang
> sama: gerbang menilai **kelengkapan keputusan bisnis**, dan sebagian gap hanya muncul ketika
> konsepnya benar-benar dimodelkan. Itu memang urutan yang dirancang — arsitektur berdiri
> sesudah gerbang justru untuk menangkapnya — tetapi tetap perlu dicatat sebagai koreksi, bukan
> sebagai perkembangan yang wajar.
>
> ### ✅ Ditutup pada hari yang sama — 2026-09-18 malam
>
> `LAB-DEC-084` menjawab `DEC-LAB-015`: organisme dan antibiotik menjadi **dua data induk
> terkendali milik Laboratorium**, berprefix `Lab`. **`S4b` pulih menjadi
> `READY_FOR_DOMAIN_DESIGN`**, dan arsitektur domain menaikkannya ke
> `DOMAIN_ARCHITECTURE_READY` pada revision 6.
>
> **Bagian ini tidak dihapus, dan itu disengaja.** Penahannya sempat ada, dan urutannya —
> gerbang menyatakan siap, arsitektur menemukan gap, pemilik modul menutupnya dalam satu
> pertanyaan — adalah justru bukti bahwa kedua tahap itu memang bekerja sebagaimana dirancang.
> Menghapusnya akan membuat gerbang ini terbaca lebih tepat daripada yang sebenarnya.

**Ke `grill-me`:** **selesai untuk putaran ini.** Nol butir tersisa yang dapat ditutup pemilik
modul sendirian. Seluruh sembilan penahan yang tinggal — `DEC-LAB-011`, `DEC-LAB-012`,
`DEC-LAB-014`, `LAB-P0-004`, `LAB-OPEN-014`, `LAB-OPEN-034`, `LAB-OPEN-029`, `LAB-OPEN-030`,
dan sisi klinis `LAB-OPEN-017` — memerlukan pihak klinis atau manajemen rumah sakit.

---

## 0C. Penilaian Ulang Revision 8 — `S4` sesudah jawaban dr. Bima (2026-09-24)

> **Bagian ini menggantikan baris `S4` pada tabel 0B.1.** Slice lain pada 0B.1 **tidak dinilai
> ulang** dan tetap seperti tertulis.

### 0C.1 Scope dan bukti

| Butir | Isi |
|---|---|
| Slice yang dinilai | **`S4` — validasi dan rilis hasil Patologi Klinik**, dipersempit `LAB-DEC-076`. `S4d` dan `S4e` **tidak** dinilai |
| Pemicu | `LAB-DEC-150` (decisions rev 72) menjawab sebagian `DEC-LAB-011` — satu-satunya penahan `S4` pada `r7` |
| Bukti | `00-interview-decisions.md` **revision 72**; `LAB-EVD-009` (jawaban dr. Bima Prasetya, Sp.PK, disampaikan pemilik modul — **bukti tertulis belum dilampirkan**); `01-existing-capability-map.md` revision 5; kontrak `LAB-API-v1` `r33`, `LAB-PERM-v1` rev 10, `LAB-STATE-v1` `r4` (approved 2026-09-24) |
| Snapshot kode | BE `ddeb5ed8`, FE `72607a087` |
| Baseline rujukan | **Tidak dipakai.** `indonesia-hospital-domain-reference` tidak dipanggil; seluruh butir di bawah bersumber keputusan modul ini |

### 0C.2 Temuan per dimensi

| # | Dimensi | Status | Bukti atau gap |
|---:|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Hasil Patologi Klinik dinyatakan sah oleh orang kedua sebelum dokter pemesan memakainya (`LAB-DEC-003`, `LAB-INH-007`) |
| 02 | Aktor | `CONFIRMED` sebagian | Pengisi: analis (`LAB-DEC-134`). Pemvalidasi: dokter berkewenangan laboratorium, pemegang pertama dr. Bima (`LAB-DEC-150`). **Perilis: `MISSING`** — lihat `DEC-LAB-018` |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Hanya hasil **Final** masuk antrean validasi (`LAB-DEC-135`) |
| 04 | Alur utama | `CONFIRMED` | Final → validasi oleh dokter → rilis oleh orang yang **berbeda** dari pemvalidasi (`LAB-DEC-120` butir 2) → dokter pemesan membaca; hasil didaftarkan ke rekam medis (`LAB-DEC-017`, `LAB-COORD-002` sudah disetujui) |
| 05 | Alur alternatif / exception | `CONFIRMED` sebagian | *Kembalikan ke analis* (`LAB-DEC-138`); pengecualian empat mata (`LAB-DEC-003`) — praktis tak terpakai pada PK sebab pengisi dan pemvalidasi selalu dua peran berbeda. **Jam tanpa pemvalidasi: `MISSING`** — `DEC-LAB-011` sisa |
| 06 | Data minimum | `CONFIRMED` | `ValidatedAt`/`ValidatedByUserId`, `ReleasedAt`/`ReleasedByUserId` (`LAB-DEC-080`); snapshot peran validator (`LAB-DEC-150`); alasan pengembalian (`LAB-DEC-138`). Snapshot peran **perilis**: `PROPOSED` — simetris dengan validator, dibutuhkan baris *Otorisasi oleh* (`LAB-DEC-120`) |
| 07 | Aturan bisnis | `CONFIRMED` | Empat mata; dua lapis jabatan-dan-penunjukan per disiplin (`LAB-DEC-142`, `143`, `148`, `150`); validator ≠ perilis (`LAB-DEC-120`) |
| 08 | Status | `CONFIRMED` | Fakta, bukan status (`LAB-DEC-080`); label turunan *Dalam Pemeriksaan*/*Selesai* (`LAB-DEC-135`) |
| 09 | Peran / authorization | `CONFIRMED` bentuknya; **`MISSING` isinya** | Bentuk: izin per aksi + penunjukan lewat kredensial Human Resource (`LAB-DEC-146`, `148`). Isi yang belum ada: pemvalidasi kedua per shift, dan seluruh pemegang **rilis** |
| 10 | Dependency antarmodul | `PROPOSED` | Pembacaan kredensial Human Resource menunggu `LAB-COORD-016` — **tidak** mengubah bentuk domain |
| 11 | Integrasi | `CONFIRMED` | Pendaftaran dokumen ke rekam medis saat rilis — disepakati `LAB-COORD-002` |
| 12 | Hasil akhir | `CONFIRMED` | Hasil tersedia bagi dokter pemesan; order berlabel *Selesai* bila seluruh pemeriksaan tidak batal sudah dirilis |
| 13 | Pembatalan / koreksi | `CONFIRMED` untuk sebelum rilis | `LAB-DEC-138`. Koreksi **sesudah** rilis adalah `S6` — di luar slice ini, tertahan `DEC-LAB-014` |
| 14 | Audit / histori | `CONFIRMED` | Nama, peran, waktu, jejak perubahan (`LAB-DEC-150` butir 4); riwayat pengembalian (`LAB-DEC-138`) |
| 15 | Notifikasi | Tidak material | Pemberitahuan tersimpan diwajibkan untuk nilai kritis dan koreksi (`LAB-DEC-012`) — keduanya `S5`/`S6`, bukan rilis biasa. Nol keputusan mewajibkan kabar pada setiap rilis |
| 16 | Billing | Tidak material | Kelayakan tagih terbit saat wadah dinyatakan layak (`LAB-INH-009`), bukan saat rilis |
| 17 | Keselamatan klinis | **Material** | Dua gap: jam tanpa pemvalidasi (`DEC-LAB-011` sisa), dan rilis hasil kritis sebelum pelaporannya berdiri (`DEC-LAB-017`) |
| 18 | Traceability | `CONFIRMED` | Baris cetak *Validasi oleh* dan *Otorisasi oleh* (`LAB-DEC-120`) |

### 0C.3 Butir bermasalah dan dampaknya

| Butir | Status | Dampak | Kenapa dampaknya itu |
|---|---|---|---|
| Pemvalidasi kedua per shift — `DEC-LAB-011` sisa | `MISSING` | **`BLOCKING` bagi pemakaian nyata**; **tidak** bagi domain design | Jawaban mana pun — dokter kedua, validasi jarak jauh oleh dokter jaga, atau menunggu pagi — memakai bentuk data yang **sama**: penunjukan per orang lewat kredensial. Yang berbeda hanya **siapa** dan **kapan**, dan itu data. Tetapi melepas `S4` ke pemakaian tanpa jawabannya meninggalkan hasil kritis malam hari tanpa jalan validasi |
| Pemegang kewenangan **rilis** Patologi Klinik | `MISSING` | **`BLOCKING` bagi pemakaian nyata**; **tidak** bagi domain design | Dibuka sebagai **`DEC-LAB-018`**. `LAB-DEC-150` menjawab validasi saja, padahal `LAB-DEC-120` mewajibkan perilis berbeda dari pemvalidasi. Bentuk datanya sudah pasti — kode kewenangan *rilis PK* sudah direncanakan `LAB-DEC-148` |
| Rilis hasil kritis sebelum `S5` berdiri | `MISSING` | **`BLOCKING` bagi pemakaian nyata**; **tidak** bagi domain design | Dibuka sebagai **`DEC-LAB-017`**. `LAB-DEC-004` — diteken pihak klinis — mewajibkan hasil kritis dirilis **beserta** catatan pelaporan dan daftar pantau, dan keduanya milik `S5` yang masih tertahan `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` |
| Snapshot peran perilis | `PROPOSED` | `NON_BLOCKING_STANDARD` | Simetris dengan validator; arsitektur domain boleh menetapkannya, dan harus menandainya sebagai usulan |
| Pembacaan kredensial Human Resource | `PROPOSED` | `NON_BLOCKING_STANDARD` bagi desain | `LAB-COORD-016` menahan implementasi, bukan bentuk |

**Contoh kenapa `DEC-LAB-017` tidak boleh diputuskan diam-diam lewat urutan pengiriman.**

> `S4` selesai dibangun dan dipakai mulai Senin. Selasa pukul 02.10 Kalium 7,2 divalidasi dan
> dirilis. Dokter pemesan dapat membacanya — tetapi sistem **tidak** memunculkan formulir
> pelaporan dan **tidak** memasukkannya ke daftar pantau, sebab keduanya `S5`. Bila perawat
> lupa menelepon, tidak ada jaring pengaman di sistem, padahal `LAB-DEC-004` mengandaikannya
> ada. Keputusan *"boleh dipakai sebelum `S5`, dengan prosedur manual sementara"* sah — tetapi
> itu keputusan klinis, bukan akibat urutan task.

### 0C.4 Decision Log — dua butir baru

| Decision ID | Pertanyaan | Slice terdampak | Bukti saat ini | Usulan baseline | Pemilik | Status | Dampak implementasi |
|---|---|---|---|---|---|---|---|
| `DEC-LAB-017` | Bolehkah `S4` dipakai **sebelum** `S5` pelaporan nilai kritis berdiri — dan bila boleh, prosedur manual apa yang menggantikan formulir pelaporan dan daftar pantau? | `S4` pemakaian nyata | `LAB-DEC-004` mewajibkan pelaporan tercatat; `S5` tertahan tiga butir | *(usulan)* Boleh, dengan prosedur pelaporan manual tertulis yang berlaku selama `S5` belum ada | Yoga Aji Pratama + `DR-LAB-001` | `OPEN` | Domain design `S4` **tidak** tertahan; **rilis `S4` ke pemakaian nyata tertahan** |
| `DEC-LAB-018` | Siapa pemegang kewenangan **rilis** (otorisasi) hasil Patologi Klinik, dan apakah wajib dokter? | `S4` pemakaian nyata | `LAB-INH-007`, `LAB-DEC-120`: rilis kewenangan terpisah dan perilis ≠ pemvalidasi; `LAB-DEC-150` hanya menjawab validasi | — | dr. Bima Prasetya, Sp.PK (penetap, `LAB-DEC-150` butir 2) | `OPEN` | Sama dengan `DEC-LAB-017` |

`DEC-LAB-011` **tetap tercatat** dengan sisa yang sudah dirumuskan `LAB-DEC-150` butir 5.

### 0C.5 Kesiapan

| Slice | `r7` | **`r8`** | Penahan tersisa |
|---|---|---|---|
| `S4` validasi dan rilis Patologi Klinik — **domain design** | `BUSINESS_DECISION_REQUIRED` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | **Nol** bagi desain |
| `S4` — **pemakaian nyata** | *(tidak dibedakan)* | **Tertahan** | `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`; implementasi juga menunggu `LAB-COORD-016` |

**Kenapa desain boleh berjalan walau pemakaian tertahan.** Ketiga penahan menjawab **siapa**
dan **kapan**, bukan **bentuk**. Wadah penunjukannya (`LAB-DEC-148`), pembagian per disiplin
(`LAB-DEC-143`), perilis ≠ pemvalidasi (`LAB-DEC-120`), fakta bukan status (`LAB-DEC-080`), dan
jalur pengembalian (`LAB-DEC-138`) semuanya sudah dikunci. Menahan desain sampai nama-nama itu
turun hanya menunda pekerjaan yang hasilnya tidak akan berubah.

**Yang wajib dihormati desain:** gerbang ini **bukan izin rilis**. Arsitektur domain dan desain
`S4` wajib mencatat ketiga penahan pemakaian di atas pada Definition of Done-nya, dan roadmap
wajib menandai task rilisnya `BLOCKED` sampai ketiganya terjawab.

### 0C.6 Handoff

**Ke `hospital-domain-architect`:**

| Field | Nilai |
|---|---|
| Slice yang dikirim | **`S4`** validasi dan rilis hasil Patologi Klinik |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN` — **desain saja** |
| Snapshot bukti | Decisions rev 72; capability map rev 5; BE `ddeb5ed8`; FE `72607a087` |
| Decision ID yang mengikat | `LAB-DEC-003`, `LAB-DEC-080`, `LAB-DEC-120`, `LAB-DEC-135`, `LAB-DEC-138`, `LAB-DEC-142`, `LAB-DEC-143`, `LAB-DEC-146`, `LAB-DEC-148`, `LAB-DEC-150`, `LAB-DEC-017` (rekam medis) |
| Penahan pemakaian yang wajib ikut | `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, `LAB-COORD-016` |
| Batas yang **wajib dihormati** | `S4d`/`S4e` **tidak** ikut; koreksi sesudah rilis (`S6`) **tidak** ikut; formulir pelaporan kritis (`S5`) **tidak** ikut |
| Yang **tidak boleh** muncul dalam rancangan | Status hasil baru (`LAB-DEC-080`); analis sebagai pemvalidasi (`LAB-DEC-150`); daftar penunjukan milik Laboratorium di samping kredensial Human Resource (`LAB-DEC-148`) |

**Ke `grill-me` / pemilik:** `DEC-LAB-017` kepada pemilik modul bersama `DR-LAB-001`;
`DEC-LAB-018` kepada dr. Bima — ditambahkan pada nota `LAB-REQ-014`.

---

## 0D. Penilaian Ulang Revision 9 — `S4d` dan `S4e` sesudah jawaban tertulis dr. Bima (2026-09-25)

> **Bagian ini menggantikan baris `S4d` dan `S4e` pada tabel 0B.1.** Slice lain tidak dinilai
> ulang.

### 0D.1 Scope dan bukti

| Butir | Isi |
|---|---|
| Slice yang dinilai | **`S4d`** — validasi dan rilis hasil **Mikrobiologi**; **`S4e`** — validasi dan rilis hasil **Patologi Anatomi**. Dinilai **terpisah**, sesuai alasan `LAB-DEC-083`: masing-masing boleh maju tanpa menunggu yang lain |
| Pemicu | Amendment pass putaran 17 menutup **kedua** penahan lama keduanya: `DEC-LAB-011` (`LAB-DEC-152` — pemegang validasi per disiplin bernama) dan `LAB-OPEN-034` (validasi tidak lintas disiplin) |
| Bukti | `00-interview-decisions.md` **revision 75**; `LAB-EVD-010` (surat tertulis dr. Bima); `01-existing-capability-map.md` revision 5; rancangan `S4` yang **sudah disetujui** — `02-backend-architecture.md` bagian 20, kontrak `r34`/`r12`/rev 11/`r5`/`INT r4` (2026-09-25) — sebagai pola yang akan diwarisi |
| Snapshot kode | BE `ddeb5ed8`, FE `72607a087` — diperiksa 2026-09-25, tidak bergeser |
| Baseline rujukan | **Tidak dipakai.** `indonesia-hospital-domain-reference` tidak dipanggil; seluruh butir bersumber keputusan modul ini |

**Yang diwarisi dari `S4` tanpa dinilai ulang**, karena keputusannya berlaku lintas disiplin atau
per disiplin dengan bentuk sama: fakta bukan status (`LAB-DEC-080`), empat mata dan
pengecualiannya (`LAB-DEC-003`, `LAB-DEC-120`), dua lapis kewenangan per disiplin
(`LAB-DEC-142`, `143`, `148`), hanya dokter memvalidasi (`LAB-DEC-150`), perilis tidak wajib dokter
(`LAB-DEC-153`), pendaftaran ke rekam medis saat rilis (`LAB-DEC-017`), dan *Kembalikan* sebelum
rilis (`LAB-DEC-138`).

### 0D.2 `S4d` — validasi dan rilis Mikrobiologi

| # | Dimensi | Status | Bukti atau gap |
|---:|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Hasil Mikrobiologi dinyatakan sah oleh dokter berkewenangan sebelum dokter pemesan memakainya; *Final* bukan rilis (`LAB-DEC-097`) |
| 02 | Aktor | `CONFIRMED` | Pengisi: analis (`LAB-DEC-146`). Pemvalidasi: **dr. Nabila Rahmawati, Sp.MK** dan pemegang tambahan yang ditetapkan (`LAB-DEC-152`). Perilis: pemegang kewenangan otorisasi, tidak wajib dokter (`LAB-DEC-153`) |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Hasil Final (`LAB-DEC-097`) — sudah berjalan di kode `S4b` |
| 04 | Alur utama | `CONFIRMED` untuk hasil **Definitif** | Final → validasi → rilis oleh orang berbeda → dokter pemesan membaca; didaftarkan ke rekam medis — pola `S4` |
| 05 | Alur alternatif / exception | **`MISSING`** untuk hasil **Sementara** | `LAB-DEC-114` mengunci kualifikasi `Definitif`/`Sementara` **yang dicetak**, dan BR-68 menyatakan biakan dibaca bertahap berhari-hari. **Nol keputusan** menyatakan apakah hasil `Sementara` boleh divalidasi dan dirilis, dan bila boleh, bagaimana hasil `Definitif` menggantikannya. Lihat `DEC-LAB-020`. *Kembalikan* sebelum rilis: `CONFIRMED` (`LAB-DEC-138`) |
| 06 | Data minimum | `CONFIRMED` | Sama dengan `S4`, per pemeriksaan (`LAB-DEC-085`) |
| 07 | Aturan bisnis | `CONFIRMED` | Empat mata; dua lapis per disiplin; tidak lintas disiplin (`LAB-DEC-152`) |
| 08 | Status | `CONFIRMED` bentuknya; **`MISSING`** untuk rilis bertahap | Fakta bukan status (`LAB-DEC-080`). **Apakah satu pemeriksaan boleh dirilis lebih dari sekali** bergantung `DEC-LAB-020` |
| 09 | Peran / authorization | `CONFIRMED` | `LAB-DEC-152`, `LAB-DEC-153`. Nama pemegang tambahan dan perilis adalah **data** (`LAB-OPEN-044` untuk calon perilis) |
| 10 | Dependency antarmodul | `PROPOSED` | Human Resource — dua kode Mikrobiologi di katalog, bagian dari `LAB-COORD-016` |
| 11 | Integrasi | `CONFIRMED` untuk satu rilis | Rekam medis `LAB-DEC-017`. **Bila rilis bertahap diizinkan**, jumlah dokumen per pemeriksaan ikut berubah — `DEC-LAB-020` |
| 12 | Hasil akhir | `CONFIRMED` | Hasil tersedia bagi dokter pemesan; ruas cetak *Validasi oleh* dan *Petugas Otorisasi* yang hari ini **sengaja kosong** (`LAB-DEC-120`) terisi |
| 13 | Pembatalan / koreksi | `CONFIRMED` sebelum rilis | `LAB-DEC-138`. Sesudah rilis: `S6` |
| 14 | Audit / histori | `CONFIRMED` | Pola `S4` |
| 15 | Notifikasi | Tidak material pada rilis biasa | **Kecuali** bila hasil `Definitif` menggantikan `Sementara` yang sudah dibaca dokter — ikut `DEC-LAB-020` |
| 16 | Billing | Tidak material | Kelayakan tagih terbit saat wadah layak (`LAB-INH-009`) |
| 17 | Keselamatan klinis | **Material** | **Hasil Sementara adalah yang paling cepat dipakai dokter** — pewarnaan Gram hari pertama dapat memulai antibiotik empiris sebelum biakan selesai. Menahannya atau melepasnya adalah keputusan klinis. Nilai kritis Mikrobiologi (`LAB-DEC-103`) milik `S5`; pertanyaan *pemakaian sebelum `S5`* sama dengan `DEC-LAB-017` |
| 18 | Traceability | `CONFIRMED` | Baris cetak *Validasi oleh* dan *Petugas Otorisasi* (`LAB-DEC-120`) |

### 0D.3 `S4e` — validasi dan rilis Patologi Anatomi

| # | Dimensi | Status | Bukti atau gap |
|---:|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Laporan PA dinyatakan sah sebelum dokter pemesan memakainya; *Final* bukan rilis (`LAB-DEC-088`) |
| 02 | Aktor | **`CONFLICT`** | Pengisi laporan: **Dokter Lab — patolog** (`LAB-DEC-090`), sebab makroskopik, mikroskopik, dan kesimpulan adalah **diagnosis**. Pemvalidasi yang disebut: **dr. Citra Maharani, Sp.PA** (`LAB-DEC-152`). Empat mata: pengisi **tidak boleh** memvalidasi hasilnya sendiri (`LAB-DEC-003`). **Ketiganya belum dapat dipenuhi bersamaan** bila patolog penulis dan satu-satunya pemvalidasi adalah orang yang sama. `LAB-DEC-090` **sendiri mencatat** pertanyaan ini belum terjawab. Lihat `DEC-LAB-021` |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Laporan Final (`LAB-DEC-088`) — sudah dirancang `S4c` |
| 04 | Alur utama | **`MISSING`** | Bergantung `DEC-LAB-021`: siapa memvalidasi laporan yang ditulis patolog |
| 05 | Alur alternatif / exception | **`MISSING`** | Sama — bila jalur pengecualian menjadi jalan **setiap** laporan, prinsip empat mata berhenti berarti apa pun; bahaya yang sudah ditulis `LAB-REQ-004` bagian 5.1 |
| 06 | Data minimum | `PROPOSED` — `NON_BLOCKING_STANDARD` | **Satuan validasi mengikuti satuan hasil:** laporan PA **per order** (`LAB-DEC-085`), bukan per pemeriksaan. Tafsiran ini satu-satunya yang koheren — memvalidasi per pemeriksaan berarti memecah satu laporan yang `LAB-DEC-085` justru satukan — tetapi belum dinyatakan eksplisit |
| 07 | Aturan bisnis | **`CONFLICT`** | `LAB-DEC-003` versus `LAB-DEC-090` — butir 02 |
| 08 | Status | `CONFIRMED` bentuknya | Fakta bukan status (`LAB-DEC-080`) |
| 09 | Peran / authorization | **`CONFLICT`** | Butir 02. **`LAB-DEC-079` memberi jalan keluarnya:** aturan empat mata ditandatangani **per disiplin**, dan aturan Patologi Anatomi **boleh berbeda** — wewenang amandemennya milik `DR-LAB-003` sendiri |
| 10 | Dependency antarmodul | `PROPOSED` | Human Resource — dua kode Patologi Anatomi di katalog (`LAB-COORD-016`) |
| 11 | Integrasi | `CONFIRMED` | Rekam medis `LAB-DEC-017` — satu dokumen per laporan |
| 12 | Hasil akhir | `CONFIRMED` | Laporan tersedia bagi dokter pemesan |
| 13 | Pembatalan / koreksi | `PROPOSED` | *Kembalikan* sebelum rilis (`LAB-DEC-138`) dirumuskan untuk **analis**; pada PA penulisnya patolog, sehingga padanannya *kembalikan ke penulis* — bergantung `DEC-LAB-021` |
| 14 | Audit / histori | `CONFIRMED` | Pola `S4` |
| 15 | Notifikasi | Tidak material | Pemberitahuan temuan kritis PA milik `S5` |
| 16 | Billing | Tidak material | `LAB-INH-009` |
| 17 | Keselamatan klinis | **Material** | Diagnosis jaringan — misalnya keganasan — **tidak dapat diperiksa ulang dengan mengulang tes** semudah angka kimia darah. Siapa pemeriksa keduanya adalah inti keselamatannya |
| 18 | Traceability | `PROPOSED` | Baris pengesah pada cetakan PA belum pernah dibuktikan dari cetakan lapangan — milik `S17` |

**Contoh kenapa `DEC-LAB-021` tidak dapat diputuskan diam-diam oleh arsitektur.**

> dr. Citra menulis laporan biopsi payudara: *karsinoma duktal invasif*. Ia menekan Final. Bila
> ia pula satu-satunya pemegang validasi PA, sistem hanya punya tiga jalan: (a) menolak — laporan
> tidak pernah dapat divalidasi; (b) menerima lewat jalur pengecualian — **setiap** laporan PA
> selamanya bertanda *"divalidasi oleh pengisi sendiri"*, dan penanda itu kehilangan arti; atau
> (c) mewajibkan patolog kedua — yang mungkin tidak dimiliki rumah sakit. Ketiganya sah secara
> teknis; **memilihnya adalah keputusan klinis** milik penandatangan aturan PA.

### 0D.4 Decision Log — dua butir baru

| Decision ID | Pertanyaan | Slice terdampak | Bukti saat ini | Usulan baseline | Pemilik | Status | Dampak implementasi |
|---|---|---|---|---|---|---|---|
| `DEC-LAB-020` | **Bolehkah hasil Mikrobiologi berkualifikasi `Sementara` divalidasi dan dirilis? Bila boleh, bagaimana hasil `Definitif` menggantikannya** — sebagai koreksi `S6` (versi lama bertanda *sudah diperbaiki*, dokter diberi tahu), atau sebagai **rilis bertahap** yang sah tanpa dianggap koreksi? | `S4d` bagian hasil `Sementara` | `LAB-DEC-114`, BR-68: kualifikasi dicetak, biakan dibaca bertahap; nol keputusan tentang rilisnya | — *(tidak diusulkan: ketiga jawaban sama sahnya dan berbeda akibat klinisnya)* | `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK + Yoga Aji Pratama | `OPEN` | Desain `S4d` bagian `Sementara` **tertahan**. Bagian hasil `Definitif` **tidak** — 0D.5 |
| `DEC-LAB-021` | **Siapa memvalidasi laporan Patologi Anatomi yang ditulis patolog?** (a) patolog **kedua** yang ditunjuk — empat mata sungguhan; (b) penulisnya sendiri lewat **jalur pengecualian** beralasan; atau (c) **aturan PA tersendiri** — penulis sekaligus pemvalidasi, dan empat mata dipenuhi pada **rilis** oleh orang lain | `S4e` seluruhnya | `LAB-DEC-090` (pengisi patolog) versus `LAB-DEC-003` (pengisi ≠ pemvalidasi); `LAB-DEC-079` (aturan per disiplin boleh berbeda); `LAB-DEC-152` (pemegang PA: dr. Citra) | — *(tidak diusulkan: wewenang klinis `DR-LAB-003`)* | **`DR-LAB-003` dr. Citra Maharani, Sp.PA** — penanda tangan `LAB-DEC-003` untuk PA — bersama Yoga Aji Pratama | `OPEN` | **`S4e` tertahan seluruhnya** |

**`DEC-LAB-017` berlaku sejenis bagi `S4d` dan `S4e`.** Pertanyaannya — bolehkah dipakai sebelum
pelaporan kritis `S5` berdiri — tidak berubah per disiplin; pemiliknya bertambah `DR-LAB-002`
untuk Mikrobiologi dan `DR-LAB-003` untuk Patologi Anatomi. Ia menahan **pemakaian nyata**, bukan
desain. Begitu juga pertanyaan *pemakaian sebelum koreksi `S6`* yang belum ber-Decision ID
(`04-prd-to-mvp.md` 21.7).

### 0D.5 Kesiapan

| Slice | `r7`/`r8` | **`r9`** | Penahan tersisa |
|---|---|---|---|
| **`S4d-1`** — validasi dan rilis hasil Mikrobiologi yang **tidak** berkualifikasi `Sementara` | `BUSINESS_DECISION_REQUIRED` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | **Nol** bagi desain |
| **`S4d-2`** — hasil Mikrobiologi berkualifikasi `Sementara` | *(tidak dibedakan)* | **`BUSINESS_DECISION_REQUIRED`** | `DEC-LAB-020` |
| **`S4e`** — validasi dan rilis Patologi Anatomi | `BUSINESS_DECISION_REQUIRED` | **`BUSINESS_DECISION_REQUIRED`** | **`DEC-LAB-021`** — penahannya **berganti**, bukan hilang |
| `S4d`/`S4e` — pemakaian nyata | — | Tertahan | `DEC-LAB-017` sejenis; dua pemegang validasi per disiplin tercatat (`LAB-DEC-152`); `LAB-COORD-016`; `LAB-OPEN-044`; pertanyaan koreksi sebelum `S6` |

**Kenapa `S4d-1` boleh berdiri sendiri.** Apa pun jawaban `DEC-LAB-020`, rilis hasil **Definitif**
tetap berbentuk sama: (a) bila `Sementara` tidak boleh dirilis, `S4d-1` adalah seluruh `S4d`; (b)
bila `Definitif` menggantikan `Sementara` lewat koreksi, `S4d-1` tetap jalan bagi hasil yang
langsung Definitif; (c) bila rilis bertahap diizinkan, `S4d-1` adalah langkah terakhirnya. **Selama
`DEC-LAB-020` terbuka, hasil `Sementara` tidak dapat dirilis lewat sistem** — dan itu **nol
kemunduran**, sebab hari ini tidak satu pun hasil Mikrobiologi dapat dirilis.

**Kenapa `S4e` tidak dapat dipecah serupa.** Tidak ada bagian laporan PA yang dapat divalidasi tanpa
menjawab siapa pemvalidasinya.

**Yang wajib dihormati hilir:** `S4d-1` **mewarisi rancangan `S4`** — arsitektur domain dan desain
cukup memperluasnya, bukan merancang ulang. Tafsiran `VAL-126` yang hari ini menolak Mikrobiologi
dengan pesan *"belum tersedia"* berubah menjadi penerimaan bagi `S4d-1`, dan menjadi penolakan
bersebab bagi hasil `Sementara`.

### 0D.6 Handoff

**Ke `hospital-domain-architect`:**

| Field | Nilai |
|---|---|
| Slice yang dikirim | **`S4d-1`** — validasi dan rilis hasil Mikrobiologi yang tidak berkualifikasi `Sementara` |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN` — **desain saja** |
| Snapshot bukti | Decisions rev 75; `LAB-EVD-010`; capability map rev 5; BE `ddeb5ed8`; FE `72607a087` |
| Decision ID yang mengikat | Seluruh yang mengikat `S4` (0C.6), ditambah `LAB-DEC-097`, `LAB-DEC-106`, `LAB-DEC-114`, `LAB-DEC-120`, `LAB-DEC-152`, `LAB-DEC-153` |
| Pola yang diwarisi | `LAB-DA-001` rev 8 bagian A5 — **perluas, jangan rancang ulang** |
| Penahan pemakaian yang wajib ikut | `DEC-LAB-017` sejenis; dua pemegang validasi Mikrobiologi tercatat; `LAB-COORD-016`; `LAB-OPEN-044` |
| Batas yang **wajib dihormati** | Hasil `Sementara` **tidak ikut** (`DEC-LAB-020`); `S4e` **tidak ikut** (`DEC-LAB-021`); nilai kritis Mikrobiologi `S5` tidak ikut |
| Yang **tidak boleh** muncul | Rilis kedua atas pemeriksaan yang sama; penurunan `Definitif` dari ada tidaknya konsultasi (`LAB-DEC-114`); pemvalidasi lintas disiplin |

**Ke `grill-me`:** `DEC-LAB-020` kepada `DR-LAB-002` bersama pemilik modul; `DEC-LAB-021` kepada
`DR-LAB-003` bersama pemilik modul. Keduanya wewenang klinis per disiplin (`LAB-DEC-079`), **bukan**
wewenang pemilik modul sendirian — dan bukan pula dr. Bima selaku penetap pemegang.

---

## 0E. Penilaian Ulang Revision 10 — `S2b`, `S8`, `S16`, `S18`, `S19` (2026-09-25)

Diminta pemilik modul pada sesi 2026-09-25: *"nilai ulang: S2b (penomoran PA/Sitologi/FNAB), S16
(definisi 11 laporan), dan S19 (order dari MCU). S8 dan S18 juga belum dinilai ulang, padahal
penahan lamanya sudah tertutup."* Kelima slice **terakhir dinilai pada revision 3** (bagian 0.2,
2026-09-01) dan tidak pernah disentuh penilaian sesudahnya.

### 0E.1 Scope dan bukti

| Butir | Isi |
|---|---|
| Slice yang dinilai | `S2b` atribut wadah khas Patologi Anatomi dan Mikrobiologi; `S8` pemberitahuan tersimpan; `S16` laporan operasional; `S18` penautan berkas hasil laboratorium eksternal; `S19` order dari MCU |
| Yang **tidak** dinilai | Slice lain. Penilaian `S4d`/`S4e` (0D) dan `S4` (0C) berlaku apa adanya |
| Snapshot bukti | decisions **rev 77**; capability map rev 5; `05-evidence-reconciliation.md` rev 4; bukti `LAB-EVD-005`, `LAB-EVD-008`, `LAB-EVD-011`; backend **`cfafad8d`** (branch `yoga`); frontend **`0bcd15724`** (branch `YogaV2`) |
| Bukti implementasi | Dibaca langsung pada kedua SHA untuk tiga pertanyaan *apa yang ada hari ini* — kemampuan pemberitahuan, dokumen klinis pasien, dan jenis kunjungan MCU. **Bukan** impact scan penuh; hasilnya diteruskan ke `trace-existing-capabilities` (0E.8) |
| Baseline rumah sakit Indonesia | Tidak dipakai. Kelima slice tertahan pada pertanyaan **milik rumah sakit ini**, bukan pada kepedulian umum yang belum terlihat |
| **Keterbatasan bukti** | Dokumen sumber `Analisis_Konsolidasi_Modul_Laboratorium.md` — asal *sebelas jenis laporan* (`CAP-015`), *order dari MCU* (`CAP-003`), dan *penautan PDF eksternal* (`CAP-014`, `BP-003`) — **tidak tersimpan di repository**. Isinya hanya dapat dibaca lewat ringkasan `05-evidence-reconciliation.md` bagian 5 dan `LAB-DEC-030` |

### 0E.2 `S2b` — atribut wadah khas Patologi Anatomi dan Mikrobiologi

**Penahan lama:** *"Penomoran PA/Sitologi/FNAB belum diputuskan."* Penahan itu ternyata **tiga
pertanyaan berbeda**, dan satu di antaranya sudah terjawab tanpa tercatat di sini.

| Butir | Status | Dampak | Bukti |
|---|---|---|---|
| Nomor cetak Patologi Anatomi — dialokasikan **per disiplin per tahun**, terpisah dari `OrderNumber` | `CONFIRMED` | — | `LAB-DEC-117` (2026-09-21): PA `26.0919`, Mikro `26-1129`, PK `25039254`; bentuknya disetel per disiplin (`LAB-OPEN-043` ditutup, `r29`) |
| Informasi specimen Mikrobiologi — Spesifik Specimen terkendali, dapat disunting beserta jejaknya | `CONFIRMED`, **sudah dibangun** lewat `S4b` | — | `LAB-DEC-099`, `LAB-DEC-107`, `LAB-DEC-112`, `LAB-DEC-130`; `FE-LAB-32` |
| Apakah **Sitologi dan FNAB** memakai seri nomor **sendiri** atau seri PA | `MISSING` | **`BLOCKING`** — bila seri per kategori, kunci alokasi nomor bergeser dari disiplin ke kategori pemeriksaan | Cetakan Sitologi, IHK, dan FNAB **belum diterima** (`LAB-OPEN-039` sisa); `LAB-DEC-117` hanya berbukti cetakan PA kategori *Histological* |
| Apakah **blok parafin dan slide** dicatat dan dilacak — nomor blok, jumlah slide, tempat arsip, peminjaman | `MISSING` | **`BLOCKING`** — konsep fisik baru dengan lifecycle sendiri, bila masuk scope | Hanya disebut sebagai nama pada keputusan awal (`00-interview-decisions.md` baris 89, 100) dan sebagai **satuan** `blok`/`slide` pada `LAB-REQ-010`. Nol keputusan tentang pelacakannya |
| **Lokasi, Pola, dan Metode Pengambilan Specimen** — ruasnya dibutuhkan | `CONFIRMED` | — | `REC4-NEW-003`..`005`; `PRD1-NEW-06` (`FR-MB-002`); `LAB-DEC-145` butir 4 menahannya di `S2b` |
| Ruas itu melekat di mana (wadah atau pesanan), **siapa** mengisinya (dokter pemesan seperti `LAB-DEC-091`, atau petugas penerima), dan wajib bagi kategori apa | `MISSING` | **`BLOCKING`** — tempat data dan kewenangan menulisnya | Nol bukti |
| **Daftar nilai** lokasi dan metode | `MISSING` | `CONFIGURABLE_DEFAULT` — cukup data induk terkendali, sejalan `LAB-DEC-099` | *"Daftar nilainya belum ada dari lapangan"* (`LAB-DEC-145` butir 4) |

**Contoh kenapa Sitologi/FNAB menentukan bentuk:**

> Bila seri PA dipakai bersama, biopsi payudara dan FNAB tiroid pada hari yang sama bernomor
> `26.0919` dan `26.0920`. Bila FNAB punya seri sendiri, keduanya bisa bernomor `26.0919` dan
> `F26.0112` — dan alokator nomor yang hari ini berkunci *disiplin* harus berkunci *kategori*. Itu
> perubahan pada data yang tersimpan, bukan pada tampilan.

### 0E.3 `S8` — pemberitahuan tersimpan

**Penahan lama:** `LAB-COORD-001`. Ia memang `closed` sejak 2026-09-01 — tetapi yang disetujui
adalah **siapa pemiliknya** (*"dibangun sebagai kemampuan platform, bukan milik Laboratorium"*,
`LAB-REQ-001` butir 7), **bukan** bahwa kemampuannya ada.

| Butir | Status | Dampak | Bukti |
|---|---|---|---|
| Pemberitahuan tersimpan adalah kemampuan **platform** | `CONFIRMED` | — | `LAB-COORD-001` |
| Platform **belum punya** sarana pemberitahuan tersimpan | `CONFIRMED` — fakta kode | **`BLOCKING`** sebagai dependency | Pada `cfafad8d`: nol entity, service, atau hub pemberitahuan; yang ada hanya `Hubs/QueueHub.cs` (antrean) dan `ApprovalInboxService` milik alur kerja Human Resource. **Sama dengan temuan 2026-09-01** |
| Pemakai pemberitahuan yang **sudah** diketahui: nilai kritis `S5` dan koreksi `S6` | `CONFIRMED` | — | `LAB-DEC-004`, `LAB-DEC-136`, `LAB-DEC-137`; `DEC-LAB-014` pertanyaan 2. `LAB-DEC-138`: pengembalian **tanpa** pemberitahuan |
| Kejadian **lain** yang wajib memberi tahu — misalnya *hasil dirilis* kepada dokter pemesan atau DPJP — beserta salurannya (kotak dalam aplikasi, WhatsApp) dan kewajiban tanda baca | `MISSING` | **`BLOCKING`** — kontrak integrasi dengan platform, dan keselamatan klinis bila dokter mengandalkan pemberitahuan yang tidak pernah dikirim | Nol keputusan |

**Temuan yang paling perlu dibaca:** di luar nilai kritis dan koreksi, `S8` **belum punya isi
bisnis sendiri**. Bila `DEC-LAB-024` menjawab *"tidak ada kejadian lain"*, `S8` lebih jujur
dilebur sebagai dependency `S5` dan `S6` — **usulan**, bukan keputusan.

### 0E.4 `S16` — laporan operasional

| Butir | Status | Dampak | Bukti |
|---|---|---|---|
| Laporan operasional masuk scope modul, Rilis 2, **sebelas jenis** | `CONFIRMED` | — | `LAB-DEC-030` (BR-26) |
| Kelompok laporannya: pemeriksaan, statistik pesanan, dokter pengirim, pasien perusahaan, rekap kunjungan/biaya/penerimaan specimen, rujukan, rekonsiliasi, kelompok penyakit | `CONFIRMED` sebagai **kelompok**, bukan sebagai daftar | — | `REC-GAP-007` dari `Bagian Ketiga` `CAP-015` |
| Tiga laporan manajerial — jumlah pemeriksaan, angka penolakan sampel, rata-rata waktu penyelesaian | `PROPOSED` | `NON_BLOCKING_STANDARD` | `DEC-LAB-007` — *"belum pernah dibahas"*; seluruh datanya milik Laboratorium |
| **Nama dan definisi hitungan** kesebelas laporan — kapan waktu penyelesaian mulai dan berakhir, pemeriksaan batal dihitung atau tidak, periode, penyaring, pembaca | `MISSING` | **`BLOCKING`** bagi desain laporan | Dokumen sumber tidak tersimpan (0E.1). Satu definisi yang **sudah** ada: keterlambatan cito dihitung dari `Accepted` sampai `Released` (`AC-17`) |
| Data **lintas modul** — *biaya* (Billing), *pasien perusahaan* (penjamin), *kelompok penyakit* (diagnosis dari modul klinis), *rekonsiliasi* (artinya tidak diketahui) | `MISSING` | **`BLOCKING`** — kontrak baca lintas modul, dan diagnosis adalah data klinis sensitif | Nol keputusan; nol kontrak integrasi |

**Contoh kenapa definisi menentukan angka:**

> Kalium cito diterima 08.00, Final 08.40, dirilis 09.10. Bila waktu penyelesaian berakhir saat
> Final, angkanya 40 menit; bila saat rilis, 70 menit. Pada batas cito 60 menit, laporan yang sama
> menyatakan *tepat waktu* atau *terlambat*.

### 0E.5 `S18` — penautan berkas hasil laboratorium eksternal

**Penahan lama:** `LAB-COORD-002`, `closed` sejak 2026-09-01 — menyetujui satu jenis dokumen hasil
laboratorium di rekam medis, yang kini dibangun bagi hasil **internal** (`INT-08`, `BE-LAB-70`).
**Penilaian ini menemukan kemampuan yang belum pernah dicatat blueprint mana pun.**

| Butir | Status | Dampak | Bukti |
|---|---|---|---|
| Penautan PDF hasil eksternal masuk scope modul Laboratorium, Rilis 2 | `CONFIRMED` | — | `LAB-DEC-030` (BR-26); `REC-GAP-010` |
| **Clinical Management sudah punya dokumen klinis pasien** — `TrxPatientClinicalDocument` dengan jenis `LaboratoryResult`, sumber `ExternalHospital`, ruas `ExternalProviderName`, `ExternalDoctorName`, `ExternalDocumentNumber`, dan status `Draft` → `Uploaded` → `Verified` → `Approved` / `Rejected` / `EnteredInError`; tampil pada linimasa rekam medis sebagai *Dokumen Klinis* | `CONFIRMED` — fakta kode, **belum** diverifikasi berjalan | — | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientClinicalDocument.cs`; `PatientClinicalDocumentController.cs` (`[Tags("Health Services / Clinical Management / Patient Clinical Document")]`, `api/v1/health-services/clinical-management/patient-clinical-documents`); `MedicalRecordTimelineService.cs:398-405`. Berdiri sejak **`9d38d30a` (2026-06-02)** — *dikoreksi capability map revision 6: `58c61a5b` (2026-09-10) yang semula ditulis di sini hanya mengganti nama tabel ke awalan `Trx`*. **Nol kemunculan** pada capability map rev 5 |
| Kemampuan itu **tidak mengunggah maupun menyimpan berkas** — `POST` menerima `FilePath` berupa teks; frontend **nol layar** untuk endpoint itu | `CONFIRMED` — fakta kode | **`BLOCKING`** bagi penyimpanan | `PatientClinicalDocumentController.cs:381`, `:955`; pencarian `patient-clinical-documents` pada frontend: nol |
| **Siapa pemilik** penautan: memakai dokumen klinis pasien milik Clinical Management, atau jalur milik Laboratorium seperti bunyi `LAB-DEC-030` | `CONFLICT` | **`BLOCKING`** — kepemilikan data | `LAB-DEC-030` menyatakan kemampuannya **milik modul Laboratorium**, sedangkan kemampuan sejenis sudah berdiri di modul lain dan belum pernah ditimbang |
| **Di mana berkas PDF disimpan**, dan bolehkah ia terlayani web | `MISSING` | **`BLOCKING`** — privasi | Pertanyaan yang sama dengan `DEC-LAB-016` untuk gambar PA; nol layanan penyimpanan bersama |
| Siapa mengunggah dan memverifikasi, ditautkan ke apa (pasien, kunjungan, atau pesanan lab), dan bagaimana berkas yang salah pasien ditarik | `MISSING` | **`BLOCKING`** — kewenangan dan koreksi | Nol keputusan. Status `EnteredInError` sudah ada pada kemampuan Clinical Management |
| Berkas eksternal **bukan** hasil resmi laboratorium ini — ia dokumen pendukung, tidak divalidasi dan dirilis pemvalidasi rumah sakit | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan `LAB-DEC-155`: hasil resmi = Tervalidasi dan Dirilis |

**Contoh kenapa pemiliknya perlu diputuskan:**

> Pasien rujukan membawa PDF hasil HbA1c dari laboratorium klinik lain. Bila petugas lab mengunggahnya
> lewat layar Laboratorium dan perawat poli mengunggah PDF yang sama lewat dokumen klinis pasien,
> rekam medis pasien memuat **dua** salinan dengan status verifikasi berbeda — dan tidak satu pun
> tahu yang lain ada.

### 0E.6 `S19` — order dari MCU

| Butir | Status | Dampak | Bukti |
|---|---|---|---|
| Order dari MCU masuk scope modul, Rilis 3; **belum pernah dibahas** | `CONFIRMED` | — | `LAB-DEC-030` (BR-26); `REC-GAP-009` dari `Bagian Ketiga` `CAP-003` |
| Kunjungan berjenis MCU **sudah ada**, dan pemesanan Lab **tidak membatasi** jenis kunjungan — kunjungan MCU secara teknis sudah dapat memesan lewat jalur umum | `CONFIRMED` — fakta kode, **belum** diverifikasi berjalan | — | `EncounterType.MedicalCheckup = 4`; `ServiceUnitType`/`ClinicType` `MedicalCheckup = 7`; nol pembatasan jenis kunjungan pada layanan pemesanan Lab; daftar Pemeriksaan dapat disaring per jenis kunjungan (`LabMonitoringService.cs:266-269`) |
| **Tidak ada** modul maupun blueprint MCU | `CONFIRMED` — fakta | — | Nol area dan nol blueprint MCU. Billing memperlakukan MCU seperti rawat jalan untuk PPN (`BKC-DEC-083`); Human Resource punya pemeriksaan kesehatan pegawai (`TrxEmployeeMedicalExamination`) — kesehatan kerja, **bukan** layanan MCU pasien |
| Apakah order MCU **berbeda** dari order kunjungan biasa — paket pemeriksaan, harga paket, klien perusahaan, pemesan non-dokter, hasil gabungan ke laporan MCU, dan penerima hasil selain pasien | `MISSING` | **`BLOCKING`** — tagihan, kewenangan memesan, dan privasi hasil | Nol keputusan; sebagian jawabannya milik modul yang belum ada |

**Contoh kenapa ini bukan sekadar sumber order tambahan:**

> PT Contoh mengirim 40 karyawan untuk paket MCU berisi darah lengkap, gula darah, dan kolesterol
> dengan harga paket Rp 350.000. Bila order dibuat per pemeriksaan lewat jalur umum, tagihannya
> menjadi harga satuan dan hasil tiap karyawan hanya terlihat oleh dokter — padahal perusahaan
> mungkin berhak atas **rekap**, dan karyawan berhak atas **privasi** hasilnya.

### 0E.7 Decision Log — tujuh butir baru

Dicatat **tanpa keputusan**; penutupannya lewat `grill-me`.

| Decision ID | Pertanyaan | Slice | Pemilik yang dibutuhkan | Status | Dampak domain |
|---|---|---|---|---|---|
| `DEC-LAB-022` | Apakah **Sitologi dan FNAB** punya seri nomor cetak sendiri atau ikut seri PA (`LAB-DEC-117`)? Dan apakah **blok parafin dan slide** dicatat dan dilacak sebagai benda fisik — nomor, jumlah, arsip, peminjaman — atau tetap di luar sistem? | `S2b` | Yoga Aji Pratama + `DR-LAB-003` dr. Citra Maharani, Sp.PA | `OPEN` | Kunci alokasi nomor; kemungkinan konsep fisik baru. Penutupnya: cetakan Sitologi/FNAB (`LAB-OPEN-039`) |
| `DEC-LAB-023` | **Lokasi, pola, dan metode pengambilan specimen** melekat di mana (wadah atau pesanan), diisi siapa (dokter pemesan atau petugas penerima), dan wajib bagi kategori apa? | `S2b` | Yoga Aji Pratama + `DR-LAB-002`, `DR-LAB-003` | `OPEN` | Tempat data dan kewenangan tulis. Daftar nilainya `CONFIGURABLE_DEFAULT` |
| `DEC-LAB-024` | **Kejadian laboratorium apa**, selain nilai kritis dan koreksi, yang wajib memberi tahu **siapa**, lewat saluran apa, dan apakah penerima wajib menandai sudah membaca? | `S8` | Yoga Aji Pratama + `DR-LAB-001` | `OPEN` | Isi `S8`; bila *tidak ada*, `S8` dilebur ke `S5`/`S6` |
| `DEC-LAB-025` | **Nama dan definisi** kesebelas laporan operasional: rumus, titik mulai dan akhir waktu, perlakuan pemeriksaan batal, periode, penyaring, pembaca, format ekspor; serta data lintas modul mana yang boleh dibaca | `S16` | Yoga Aji Pratama + kepala instalasi; pemilik `billing-kasir` untuk biaya | `OPEN` | Kontrak baca lintas modul. Tiga laporan `DEC-LAB-007` dapat dipisah lebih dulu bila definisinya disahkan |
| `DEC-LAB-026` | **Siapa pemilik** penautan berkas hasil eksternal — dokumen klinis pasien milik Clinical Management yang sudah ada, atau jalur milik Laboratorium (`LAB-DEC-030`)? Siapa mengunggah dan memverifikasi, ditautkan ke apa, dan bagaimana berkas salah pasien ditarik? | `S18` | Yoga Aji Pratama + pemilik `clinical-management` | `OPEN` | Kepemilikan data, kewenangan, koreksi. **Penyimpanan berkasnya** mengikuti `DEC-LAB-016`, yang kini berlaku juga bagi PDF |
| `DEC-LAB-027` | Apakah **order dari MCU** berbeda dari order kunjungan biasa — paket dan harga paket, klien perusahaan, pemesan non-dokter, hasil gabungan, penerima hasil — atau cukup lewat jalur umum yang sudah ada? | `S19` | Yoga Aji Pratama + pemilik layanan MCU (belum ditetapkan) + `billing-kasir` | `OPEN` | Tagihan, kewenangan memesan, privasi hasil. Bila *cukup jalur umum*, `S19` dapat ditutup sesudah verifikasi berjalan |
| `LAB-COORD-017` | **Penyediaan** kemampuan pemberitahuan tersimpan oleh pemilik platform. `LAB-COORD-001` hanya menyepakati kepemilikannya | `S8`, `S5`, `S6` | Pemilik platform | `OPEN` | Dependency, bukan keputusan bisnis Laboratorium |

`DEC-LAB-016` **diperluas cakupannya**, tidak diganti: pertanyaan penyimpanan gambar PA kini juga
menahan penyimpanan PDF hasil eksternal (`S18`).

### 0E.8 Kesiapan

| Slice | Revision 3 | **Revision 10** | Penahan yang tersisa |
|---|---|---|---|
| `S2b` | `BUSINESS_DECISION_REQUIRED` — penomoran PA/Sitologi/FNAB | **`BUSINESS_DECISION_REQUIRED`** — **menyempit**: nomor PA terjawab `LAB-DEC-117`; Mikrobiologi terbangun lewat `S4b` | `DEC-LAB-022`, `DEC-LAB-023`; bukti `LAB-OPEN-039` |
| `S8` | `BUSINESS_DECISION_REQUIRED` — `LAB-COORD-001` | **`BUSINESS_DECISION_REQUIRED`** — penahannya **berganti**, bukan hilang | `DEC-LAB-024`; dependency `LAB-COORD-017` |
| `S16` | `BUSINESS_DECISION_REQUIRED` — definisi sebelas laporan | **`BUSINESS_DECISION_REQUIRED`** — tetap | `DEC-LAB-025`; dokumen sumber tidak tersimpan |
| `S18` | `BUSINESS_DECISION_REQUIRED` — `LAB-COORD-002` | **`BUSINESS_DECISION_REQUIRED`** — penahannya **berganti**; ditemukan kemampuan yang sudah ada di modul lain | `DEC-LAB-026`, `DEC-LAB-016` diperluas |
| `S19` | `BUSINESS_DECISION_REQUIRED` — MCU belum dibahas | **`BUSINESS_DECISION_REQUIRED`** — tetap, dengan satu fakta baru: jalur umum sudah menerima kunjungan MCU | `DEC-LAB-027` |

**Nol slice naik ke `READY_FOR_DOMAIN_DESIGN`, dan itu hasil yang jujur, bukan kemunduran.** Dua
penahan lama — `LAB-COORD-001` dan `LAB-COORD-002` — memang tertutup, tetapi keduanya menjawab
**siapa pemilik**, bukan **apa yang dibangun**. Kelima slice kini tertahan pertanyaan yang **bernama
dan berpemilik**, bukan lagi kalimat umum.

**Kesiapan keseluruhan tetap `PARTIALLY_READY`.**

### 0E.9 Apa yang boleh berjalan dan apa yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| `trace-existing-capabilities` atas `TrxPatientClinicalDocument` dan pemesanan dari kunjungan MCU — keduanya fakta kode yang belum diverifikasi berjalan dan belum ber-CAP | Arsitektur domain dan desain kelima slice |
| Meminta cetakan Sitologi, IHK, dan FNAB (`LAB-OPEN-039`) — penutup bukti `DEC-LAB-022` | Membangun layar unggah PDF di Laboratorium sebelum `DEC-LAB-026` — berisiko menggandakan kemampuan Clinical Management |
| Mengesahkan definisi tiga laporan `DEC-LAB-007` sebagai langkah pertama `DEC-LAB-025` | Membangun kotak pemberitahuan di Laboratorium — `LAB-COORD-001` menempatkannya di platform |

### 0E.10 Handoff

**Ke `grill-me`:** `DEC-LAB-022`..`DEC-LAB-027`, dengan urutan usulan: `DEC-LAB-026` (kemampuan yang
sudah ada di modul lain paling murah ditimbang), `DEC-LAB-025` (mulai dari tiga laporan
`DEC-LAB-007`), `DEC-LAB-023`, `DEC-LAB-022` (menunggu cetakan), `DEC-LAB-024`, `DEC-LAB-027`.

**Ke `trace-existing-capabilities`:** dokumen klinis pasien Clinical Management dan pemesanan Lab dari
kunjungan MCU — untuk dicatat sebagai `CAP` dengan status berbasis bukti berjalan.

**Ke `hospital-domain-architect`:** **tidak ada** slice yang dikirim.

**Ke pemilik platform:** `LAB-COORD-017`.

---

## 0F. Penilaian Ulang Revision 11 — sesudah amendment pass putaran 19 (2026-09-25)

Menilai ulang kelima slice 0E sesudah pemilik modul menjawab penahannya (`LAB-DEC-157`..`LAB-DEC-164`,
decisions rev 79). Snapshot bukti sama dengan 0E — backend `cfafad8d`, frontend `0bcd15724` —
ditambah bukti cetakan `LAB-EVD-005` yang dibaca ulang untuk `S2b`.

### 0F.1 Peta slice sesudah putaran 19

Putaran 19 **mengubah bentuk** tiga slice, bukan hanya menjawabnya:

| Slice semula | Sekarang | Dasar |
|---|---|---|
| `S2b` | Dipecah: **`S2b-1`** lokasi, pola, dan metode pengambilan; **`S2b-2`** seri nomor Sitologi/FNAB; **`S2b-3`** fiksasi (**baru** — lihat 0F.2). Blok dan slide **keluar scope** | `LAB-DEC-161`, `LAB-DEC-162` |
| `S8` | **Dilebur** menjadi dependency `S5` dan `S6` — tidak dinilai lagi sebagai slice | `LAB-DEC-163` |
| `S16` | Dipecah: **`S16a`** tiga laporan; **`S16b`** delapan laporan lainnya | `LAB-DEC-159` |
| `S18` | Tetap satu slice; sisi bisnis Laboratorium **lengkap** | `LAB-DEC-157`, `LAB-DEC-158` |
| `S19` | **Tidak ada yang dirancang** di Laboratorium; ditutup sesudah verifikasi berjalan | `LAB-DEC-164` |

### 0F.2 `S2b` — dua temuan dari cetakan PA yang belum pernah dibaca untuk slice ini

`LAB-EVD-005` bagian 3 (cetakan PA *Histological*, 2026-09-21) memuat dua ruas wadah:

| Temuan | Status | Dampak | Bukti |
|---|---|---|---|
| **`Lokasi Spesimen` bernomor, lebih dari satu** pada satu laporan PA. Belum diputuskan apakah itu **beberapa wadah** dengan satu lokasi masing-masing, atau **satu wadah** yang memuat beberapa lokasi | `MISSING` | **`BLOCKING`** bagi `S2b-1` — satu nilai atau daftar per wadah adalah bentuk data yang tersimpan. `LAB-DEC-098` sudah mengizinkan **lebih dari satu** Spesifik Specimen per wadah, sehingga kedua bacaan sama-sama mungkin | `LAB-EVD-005` baris 71-72; `LAB-DEC-161` butir 1 menyatakan *melekat pada wadah* tanpa kardinalitas |
| **`Fiksasi`** — contoh *Formalin buffer 10%* | `MISSING` | **`BLOCKING`** bagi fiksasi saja — siapa mencatat (ruang tindakan saat pengambilan, atau laboratorium saat penerimaan) dan dari daftar apa. **Nol kemunculan** di blueprint mana pun: cetakan itu diadopsi putaran 11 (`LAB-DEC-114`..`121`) tanpa ruas ini | `LAB-EVD-005` baris 73 |
| 166 baris *Spesifik Specimen* nonaktif yang menyebut **lokasi tanpa bahan** — misalnya *Specimen from abdominal cavity* — lengkap dengan kode SNOMED CT | `PROPOSED` | `CONFIGURABLE_DEFAULT` — calon isi awal daftar lokasi, bukan keputusan | `LabSpecimenDetailType.cs:80-86`; `LAB-DEC-130` |

**Contoh kenapa kardinalitas lokasi menentukan bentuk data:**

> Mastektomi kiri dikirim dalam dua wadah — (1) *payudara kiri*, (2) *kelenjar getah bening aksila
> kiri*. Bila satu wadah satu lokasi, cetakan menomori wadahnya. Bila satu wadah boleh memuat
> beberapa jaringan, lokasi menjadi daftar di dalam wadah — dan patolog yang memotong jaringan harus
> tahu potongan mana dari lokasi mana.

**Dimensi lain `S2b-1` seluruhnya terjawab** oleh `LAB-DEC-161`: tujuan, aktor (pengisi hasil;
kepala instalasi mengelola daftar dan kewajiban), alur utama sampai Final, jalur `Lainnya`, audit
(`LAB-DEC-112`), koreksi sesudah Final lewat Reopen atau `S6`. Dimensi 15-17 tidak berlaku secara
material; dimensi 16 nol dampak tagihan.

### 0F.3 `S16a` — kedelapan belas dimensi

| Dimensi | Status | Isi |
|---|---|---|
| 01 Tujuan | `CONFIRMED` | Volume, mutu pra-analitik, dan kecepatan laboratorium — `DEC-LAB-007`, disahkan `LAB-DEC-159` |
| 02 Aktor | `CONFIRMED` | Pembaca: kepala instalasi dan manajemen; pemberi izin: admin (`LAB-DEC-160`) |
| 03 Pemicu / prasyarat | `CONFIRMED` | Pembaca membuka laporan. **Prasyarat data:** `ReleasedAt` dibangun `BE-LAB-70` |
| 04 Alur utama | `CONFIRMED` | Pilih periode dan disiplin → tampil → unduh |
| 05 Alternatif / exception | `PROPOSED` — `NON_BLOCKING_STANDARD` | Periode tanpa data tampil kosong beralasan. **Disiplin yang belum punya jalur rilis** — Patologi Anatomi sampai `S4e`, Mikrobiologi sampai `MVP-10` — ditulis ***"belum dapat dihitung"***, **bukan** angka 0 (pola `AC-166`: layar menyatakan, bukan diam) |
| 06 Data minimum | `CONFIRMED` | `ReleasedAt`, keputusan kelayakan beserta `DecidedAt` dan alasannya, disiplin, kesegeraan. Seluruhnya milik Laboratorium |
| 07 Aturan / validasi | `CONFIRMED` | Rumus `LAB-DEC-159`. **Usulan tambahan** (`PROPOSED`, `NON_BLOCKING_STANDARD`): TAT memakai wadah yang **benar-benar melayani** pemeriksaan (`LabExamination.SpecimenId`) — bila wadah pertama ditolak dan diambil ulang, waktu layak wadah pengganti yang dipakai; TAT disajikan rata-rata beserta jumlah pemeriksaan dan jumlah yang terlambat |
| 08 Status | Tidak berlaku | Laporan hanya membaca |
| 09 Kewenangan | `CONFIRMED` | Hak akses tersendiri (`LAB-DEC-160`) |
| 10 Dependency antarmodul | `CONFIRMED` | **Nol** — seluruh data milik Laboratorium |
| 11 Integrasi | `PROPOSED` — `CONFIGURABLE_DEFAULT` | Format unduhan mengikuti pola ekspor proyek; ekspor Excel daftar order sendiri tetap kemampuan Rilis 2 tersendiri (`LAB-DEC-030`) |
| 12 Hasil akhir | `CONFIRMED` | Tiga angka per periode dan disiplin, dengan rincian alasan penolakan serta cito/rutin |
| 13 Pembatalan / koreksi | `CONFIRMED` | Pemeriksaan batal tidak dihitung; laporan tidak mengubah data |
| 14 Audit | `PROPOSED` — `NON_BLOCKING_STANDARD` | Membuka laporan tidak dicatat (konvensi `GET`); **mengunduh** dicatat pelaku dan waktunya |
| 15 Notifikasi | Tidak berlaku | — |
| 16 Billing | Tidak berlaku | Dihitung saat rilis, bukan saat layak tagih |
| 17 Keselamatan klinis | Tidak berlaku | Angka agregat |
| 18 Pelaporan | `CONFIRMED` | Konsisten dengan `AC-17` (`AC-252`). **Usulan** (`PROPOSED`, `NON_BLOCKING_STANDARD`): agregat saja, **tanpa** daftar pasien — privasi |

**Akibat yang perlu diketahui pemilik modul, bukan penahan:** karena jumlah pemeriksaan dihitung saat
rilis, laporan **tidak akan memuat Patologi Anatomi** sampai `S4e` berdiri, dan tidak memuat
Mikrobiologi sampai `MVP-10` dipakai. Usulan dimensi 05 membuat hal itu terbaca di layar.

### 0F.4 `S18` — sisi Laboratorium lengkap, penahan di luar modul

| Dimensi | Status | Isi |
|---|---|---|
| 01, 02, 04, 12 | `CONFIRMED` | `LAB-DEC-157`, `LAB-DEC-158` |
| 06 Data — tempat berkas | `MISSING` | **`BLOCKING`** — `DEC-LAB-016` (privasi; pemilik platform + Yoga Aji Pratama) |
| 08, 09, 11 — status yang ditulis saat unggah, arti `Verified`/`Approved`, kontrak tulis lintas modul | `MISSING` | **`BLOCKING`** — `LAB-COORD-018` (pemilik `clinical-management`) |
| 13, 14, 17 | `CONFIRMED` | Salah input beralasan, tidak dihapus; cek dua identitas |
| Perilaku kemampuan Clinical Management | **Belum diverifikasi berjalan** | `trace-existing-capabilities` — sedang dijadwalkan |

### 0F.5 Decision Log — dua butir baru

| Decision ID | Pertanyaan | Slice | Usulan baseline | Pemilik | Status |
|---|---|---|---|---|---|
| `DEC-LAB-028` | **Apakah fiksasi specimen PA dicatat**, siapa mencatatnya — ruang tindakan saat pengambilan atau laboratorium saat penerimaan — dan dari daftar apa? Cetakan PA memuatnya (*Formalin buffer 10%*) | `S2b-3` | Dicatat per wadah dari daftar terkendali, oleh pihak yang memfiksasi | Yoga Aji Pratama + `DR-LAB-003` | `OPEN` |
| `DEC-LAB-029` | **Satu wadah satu lokasi, atau satu wadah boleh memuat beberapa lokasi bernomor?** | `S2b-1` | Satu wadah satu lokasi — jaringan dari tempat berbeda dikirim dalam wadah berbeda | Yoga Aji Pratama + `DR-LAB-003` | `OPEN` |

`DEC-LAB-022` (seri Sitologi/FNAB), `DEC-LAB-025` (delapan laporan), `DEC-LAB-016`, `LAB-COORD-017`, dan
`LAB-COORD-018` **tetap** seperti dicatat decision log rev 79.

### 0F.6 Kesiapan

| Slice | Revision 10 | **Revision 11** | Penahan tersisa |
|---|---|---|---|
| **`S16a`** tiga laporan | *(bagian `S16`)* | ✅ **`READY_FOR_DOMAIN_DESIGN`** | **Nol.** Enam usulan `NON_BLOCKING_STANDARD`/`CONFIGURABLE_DEFAULT` wajib ikut terlihat |
| `S16b` delapan laporan | *(bagian `S16`)* | `BUSINESS_DECISION_REQUIRED` | `DEC-LAB-025` |
| `S2b-1` lokasi, pola, metode | *(bagian `S2b`)* | `BUSINESS_DECISION_REQUIRED` — **satu pertanyaan kecil** | `DEC-LAB-029` |
| `S2b-2` seri Sitologi/FNAB | *(bagian `S2b`)* | `BUSINESS_DECISION_REQUIRED` | `DEC-LAB-022`; bukti `LAB-OPEN-039` |
| `S2b-3` fiksasi | — | `BUSINESS_DECISION_REQUIRED` — **baru** | `DEC-LAB-028` |
| `S8` | `BUSINESS_DECISION_REQUIRED` | **Dilebur** ke `S5`/`S6` — bukan slice lagi | `LAB-COORD-017` kini melekat pada `S5`/`S6` |
| `S18` | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` — **penahannya di luar modul** | `DEC-LAB-016`, `LAB-COORD-018` |
| `S19` | `BUSINESS_DECISION_REQUIRED` | **Tidak dirancang** — ditutup sesudah verifikasi | Bukti berjalan pemesanan dari kunjungan `MedicalCheckup` |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`.** Satu slice naik; satu slice dilebur; satu slice
keluar dari desain.

### 0F.7 Apa yang boleh berjalan dan apa yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| **`S16a` ke `hospital-domain-architect`** | Desain `S2b-1` sampai `DEC-LAB-029` — satu pertanyaan |
| `trace-existing-capabilities` atas dokumen klinis pasien dan kunjungan MCU | Desain `S18` sampai `DEC-LAB-016` dan `LAB-COORD-018` |
| Menyiapkan isi awal daftar lokasi dari 166 baris lokasi SNOMED yang sudah ter-seed | Seri Sitologi/FNAB, fiksasi, delapan laporan lain |

### 0F.8 Handoff

**Ke `hospital-domain-architect`:**

| Field | Nilai |
|---|---|
| Slice | **`S16a`** — tiga laporan operasional: jumlah pemeriksaan, angka penolakan sampel, waktu penyelesaian |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN` |
| Snapshot bukti | decisions rev 79; backend `cfafad8d`; frontend `0bcd15724` |
| Decision ID yang mengikat | `LAB-DEC-159`, `LAB-DEC-160`, `LAB-DEC-155` (hasil resmi), `AC-17` |
| Usulan yang wajib tetap ditandai usulan | Dimensi 05, 07, 11, 14, 18 pada 0F.3 |
| Dependency | `BE-LAB-70` (`ReleasedAt`); angka Mikrobiologi dan PA bergantung `MVP-10` dan `S4e` |
| Yang **tidak boleh** muncul | Kolom ringkasan tersimpan yang dapat basi; daftar pasien pada laporan; rumus TAT kedua yang berbeda dari `AC-17` |

**Ke `grill-me`:** `DEC-LAB-029` dan `DEC-LAB-028` — keduanya kecil dan dapat dijawab dalam satu sesi.

**Ke `trace-existing-capabilities`:** tetap seperti 0E.10.

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

> ⚠️ **Bagian 5 sampai 9 adalah rekam jejak penilaian 2026-09-01 dan TIDAK berlaku sebagai
> keadaan sekarang.** Yang berlaku: peta **0.2** untuk seluruh slice, dan **0A** untuk `S4`,
> `S4b`, `S4c`, `S5`, dan `S6`.
>
> **Dua kekeliruan pada bagian ini sudah diketahui dan sengaja tidak disunting**, karena
> menyuntingnya akan menghapus jejak bahwa keduanya pernah tercatat: `LAB-COORD-001` dan
> `LAB-COORD-002` masih tertulis sebagai penahan `S5` dan `S6`, padahal keduanya **`closed`
> sejak 2026-09-01** lewat `LAB-REQ-001` butir 7 dan 8. Koreksinya ada di `LAB-REQ-004`
> bagian 7.1 dan di 0A.8.

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
| 11 | 2026-09-25 | **Kelima slice 0E dinilai ulang sesudah amendment pass putaran 19** (bagian 0F). **`S16a` naik `READY_FOR_DOMAIN_DESIGN`** — kedelapan belas dimensi terjawab atau berusulan tidak memblokir; laporan PA dan Mikrobiologi ditulis *belum dapat dihitung* sampai jalur rilisnya ada. `S8` **dilebur** ke `S5`/`S6`; `S19` **tidak dirancang**; `S18` sisi Laboratorium lengkap, penahannya di luar modul. **`S2b` dipecah tiga**, dan pembacaan ulang cetakan PA `LAB-EVD-005` menemukan dua hal yang belum pernah diputuskan: **lokasi bernomor lebih dari satu** (`DEC-LAB-029`, kardinalitas) dan **ruas Fiksasi** (`DEC-LAB-028`). Isi awal daftar lokasi diusulkan dari 166 baris lokasi SNOMED yang sudah ter-seed nonaktif | `draft` |
| 10 | 2026-09-25 | **`S2b`, `S8`, `S16`, `S18`, `S19` dinilai ulang atas permintaan pemilik modul** (bagian 0E) — kelimanya belum pernah disentuh sejak revision 3. **Nol slice naik.** Penahan lama diganti yang bernama: `S2b` menyempit — nomor PA sudah dijawab `LAB-DEC-117` dan bagian Mikrobiologi terbangun lewat `S4b`; sisanya `DEC-LAB-022` (seri Sitologi/FNAB, pelacakan blok dan slide) dan `DEC-LAB-023` (lokasi dan metode pengambilan). `S8`: `LAB-COORD-001` hanya menyepakati **pemilik** — platform masih nol sarana pemberitahuan pada `cfafad8d`; penahan kini `DEC-LAB-024` dan `LAB-COORD-017`. `S16`: `DEC-LAB-025`; dokumen sumber analisis konsolidasi **tidak tersimpan** di repository. **`S18`: kemampuan dokumen klinis pasien milik Clinical Management ditemukan** — jenis `LaboratoryResult`, sumber `ExternalHospital`, status verifikasi — tanpa unggahan berkas dan tanpa layar, **belum ber-CAP**; `LAB-DEC-030` bertentangan soal pemiliknya → `DEC-LAB-026`, `DEC-LAB-016` diperluas. `S19`: jalur umum sudah menerima kunjungan `MedicalCheckup`; `DEC-LAB-027` | `draft` |
| 9 | 2026-09-25 | **`S4d` dan `S4e` dinilai ulang sesudah jawaban tertulis dr. Bima** (bagian 0D). Kedua penahan lamanya — `DEC-LAB-011` dan `LAB-OPEN-034` — tertutup `LAB-DEC-152`, **tetapi penilaian 18 dimensi menemukan dua keputusan klinis yang belum pernah ditanyakan**, masing-masing dibuka sebagai decision ID baru. **`S4d` dipecah:** `S4d-1` — hasil Mikrobiologi yang bukan `Sementara` — naik **`READY_FOR_DOMAIN_DESIGN`** dan mewarisi rancangan `S4`; `S4d-2` — hasil `Sementara` — tertahan **`DEC-LAB-020`**: kualifikasi `Sementara` sudah diputuskan **dicetak** (`LAB-DEC-114`), tetapi nol keputusan menyatakan apakah ia boleh dirilis dan bagaimana hasil `Definitif` menggantikannya. **`S4e` tetap `BUSINESS_DECISION_REQUIRED`, penahannya berganti** menjadi **`DEC-LAB-021`**: pengisi laporan PA adalah patolog (`LAB-DEC-090`) sedangkan pengisi dilarang memvalidasi hasilnya sendiri (`LAB-DEC-003`) — pertanyaan yang **`LAB-DEC-090` sendiri catat belum terjawab** sejak 2026-09-18. Keduanya wewenang klinis per disiplin (`LAB-DEC-079`): `DR-LAB-002` dan `DR-LAB-003`. `DEC-LAB-017` dinyatakan berlaku sejenis bagi kedua disiplin | `draft` |
| 8 | 2026-09-24 | **`S4` dinilai ulang sesudah `LAB-DEC-150`** (bagian 0C). Kedelapan belas dimensi dinilai; `S4` naik **`READY_FOR_DOMAIN_DESIGN` untuk desain saja**, sebab ketiga penahan yang tersisa menjawab *siapa* dan *kapan*, bukan *bentuk*. **Pemakaian nyata tetap tertahan** oleh `DEC-LAB-011` sisa dan **dua decision ID baru**: `DEC-LAB-017` — bolehkah `S4` dipakai sebelum `S5` pelaporan kritis berdiri — dan `DEC-LAB-018` — siapa pemegang kewenangan **rilis** Patologi Klinik, butir yang `LAB-DEC-150` tidak jawab padahal `LAB-DEC-120` mewajibkan perilis berbeda dari pemvalidasi. Slice lain **tidak** dinilai ulang. Handoff `S4` ke `hospital-domain-architect` | `draft` |
| 7 | 2026-09-18 | **Penilaian ulang sesudah amendment pass putaran 6 menjawab keempat butir yang `r6` tunjuk sebagai milik pemilik modul.** Ditulis sebagai bagian **0B**. `S4b` naik **`READY_FOR_DOMAIN_DESIGN`** menemani `S4c`, sehingga **dua** slice kini dikirim ke arsitektur domain; `S4` turun menjadi **satu** penahan; dua slice **baru** lahir — `S4d` dan `S4e` validasi/rilis Mikrobiologi dan Patologi Anatomi — langsung dengan dua penahan. **Dan satu penilaian `r6` saya koreksi sendiri:** `0A.13` menyatakan `LAB-P0-003` dapat ditutup pemilik modul sendirian; wawancara membuktikan **tidak**. `LAB-REQ-004` bagian 4.5 memuat empat pertanyaan turunan `LAB-DEC-007` dan **tiga di antaranya klinis**. Gerbang `r6` membacanya dari judul — *"aturan pembatalan dan koreksi"* — lalu menyimpulkan ia sejenis `LAB-DEC-063`. Yang ditutup pemilik modul nyata tetapi sebagian: bentuk alasan dan bentuk versi. Sisanya `DEC-LAB-014`, dan `S6` **tidak** terbuka. **Satu penilaian `r6` yang lain ternyata terlalu keras:** `LAB-OPEN-017` ditandai `BLOCKING` bagi `S4b` atas dasar ia ruas di dalam struktur hasil Mikrobiologi — benar sebagai pernyataan struktur, **keliru sebagai penahan rilis**, sebab blueprint sudah menempatkan penanda `Definitif` pada Rilis 2 sejak semula. `LAB-DEC-081` hanya membaca yang sudah tertulis, dan penahan tunggal `S4b` lenyap tanpa satu pun keputusan klinis baru. **Penahan yang kini paling mahal seluruh modul: `DEC-LAB-011`** — ia menahan `S4`, `S4d`, dan `S4e` sekaligus, yaitu **seluruh kemampuan merilis hasil untuk ketiga disiplin**, dan pertanyaannya tetap yang paling sederhana dari semuanya: siapa yang berhak menunjuk pemegang kewenangan validasi. **`grill-me` selesai untuk putaran ini** — nol butir tersisa yang dapat ditutup pemilik modul sendirian. Kesiapan keseluruhan tetap `PARTIALLY_READY` | `draft` |
| 6 | 2026-09-17 | **Penilaian ulang sesudah `LAB-SIGN-001` ditutup `LAB-DEC-079` — tanda tangan klinis diberikan apa adanya, per disiplin, oleh `DR-LAB-001`/`002`/`003`.** Ditulis sebagai bagian **0A**. **Hasilnya satu slice naik dan empat menyempit, nol mundur:** `S4c` pengisian hasil Patologi Anatomi menjadi **`READY_FOR_DOMAIN_DESIGN`** — seluruh dimensinya terisi, bentuk hasilnya sudah dikunci BR-23 (makroskopik, mikroskopik, kesimpulan wajib), dan tanda tangannya kini ada. `S4` turun dari 4 penahan menjadi 2, `S4b` dari 3 menjadi **1**, `S5` dari 4 menjadi 3, `S6` dari 3 menjadi **1**. **Dua penahan diturunkan derajatnya dengan alasan berbukti, bukan dilonggarkan:** `LAB-P0-005` integrasi alat menjadi `NON_BLOCKING_STANDARD` untuk Rilis 1 sebab `LAB-DEC-005` sudah memutuskannya dan `S4a` sudah dibangun di atasnya; `LAB-P0-001` menjadi `NON_BLOCKING_STANDARD` bagi ketiga slice hasil sebab bagiannya yang menyangkut validasi dan rilis sudah dijawab `LAB-DEC-022` lalu disahkan `LAB-DEC-079`, dan memblokir `S4c` atas kewenangan mengisi berarti menerapkan ukuran yang tidak diterapkan pada `S4a`. **Temuan paling mahal justru bukan soal requirement, melainkan soal peta:** `S4b` dan `S4c` bernama *pengisian hasil*, dan **nol slice** memuat validasi serta rilis untuk Mikrobiologi dan Patologi Anatomi — `LAB-DEC-076` memecah `S4` hanya untuk Patologi Klinik. Celah itu tak terlihat selama `LAB-SIGN-001` menahan kelimanya sekaligus; tanda tangan per disiplin membuatnya terlihat sekaligus mendesak, sebab `DR-LAB-002` dan `DR-LAB-003` kini memegang wewenang atas disiplin yang tidak punya tempat menjalankannya. Dibuka `DEC-LAB-013`. **Tiga decision ID baru:** `DEC-LAB-011` (siapa menetapkan pemegang kewenangan validasi, dan jaminan dua per shift — `LAB-REQ-004` bagian 5.1 tidak ikut dijawab), `DEC-LAB-012` (pemberi persetujuan klinis atas perubahan batas kritis — bagian 5.2, juga tidak dijawab), dan `DEC-LAB-013`. **Dua koreksi pembukuan:** `LAB-COORD-001` dan `LAB-COORD-002` masih tertulis sebagai penahan `S5`/`S6` pada bagian 5 dan 7, padahal `closed` sejak 2026-09-01 — bagian itu ditandai rekam jejak, bukan disunting. **Dan satu pernyataan revision 2 dinyatakan tidak berlaku lagi:** *"`grill-me` tidak akan membuka apa-apa lagi tanpa kehadiran pihak-pihak itu"* — hari ini tiga penahan dapat ditutup pemilik modul sendiri (`LAB-P0-002`, `LAB-OPEN-017`, `LAB-P0-003`), dan ketiganya penahan tunggal atau terakhir bagi slice-nya. Kesiapan keseluruhan tetap `PARTIALLY_READY`. Nol entity, ERD, kontrak, migration, atau task diturunkan | `draft` |
| 3-5 | — | **Tidak tercatat.** Header dokumen ini sempat menunjuk revision `3`, `4`, lalu `5`, tetapi baris riwayatnya tidak pernah ditulis. Isi revision 3 masih dapat dibaca pada bagian 0 yang menamai dirinya sendiri *"Penilaian Ulang Revision 3"*; revision 4 dan 5 **tidak dapat dipulihkan** dari dokumen mana pun. Yang diketahui tentang revision 5: ia memecah baris `S4` pada peta 0.2 menjadi `S4a` + `S4` mengikuti `LAB-DEC-076` pada 2026-09-17. Ketiadaan ini dicatat apa adanya, bukan disusun ulang dari ingatan | — |
| 1 | 2026-09-01 | Penilaian pertama. 13 bagian pekerjaan dinilai terhadap 18 dimensi. Tiga gap pemblokir baru ditemukan: `DEC-LAB-001`, `DEC-LAB-002`, `DEC-LAB-003`. Lima bagian dinyatakan siap dirancang, tujuh harus berhenti. Kesiapan keseluruhan `PARTIALLY_READY` | `draft` |
| 2 | 2026-09-01 | Ketiga gap ditutup pemilik modul lewat `grill-me` closure pass putaran kedua (`LAB-DEC-021`, `LAB-DEC-022`, `LAB-DEC-023`). Dimensi 02, 06, dan 09 naik menjadi `CONFIRMED`. `S3` naik menjadi siap dirancang sehingga slice yang dikirim menjadi enam. Blocker tersisa empat, seluruhnya memerlukan pihak di luar modul | `draft` |
