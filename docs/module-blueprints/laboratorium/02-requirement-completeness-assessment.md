# Laboratorium — Penilaian Kelengkapan Requirement

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Assessment ID | `LAB-RCG-001` |
| Revision | `16` — bagian 0J.9, 2026-11-27: **sesudah closure pass putaran 36, `S17-1b` naik `READY_FOR_DOMAIN_DESIGN`; `S17-1` utuh siap** (`DEC-LAB-035`/`036` ditutup `LAB-DEC-293`/`298`). Sebelumnya `15` — bagian 0J, 2026-11-27: **`S17-1` penyerahan hasil ke pasien dinilai ulang sesudah putaran 34–35 dan dipecah: `S17-1a` tetap `READY_FOR_DOMAIN_DESIGN`; `S17-1b` syarat persetujuan `BUSINESS_DECISION_REQUIRED`** (`DEC-LAB-035` — `LAB-DEC-182` bertentangan dengan `LAB-DEC-280`); `DEC-LAB-036` (`LAB-DEC-180` vs `LAB-DEC-285`/`289`) dibuka. Sebelumnya `14` — bagian 0I, 2026-11-25: **slice baru `S20` penjadwalan ulang pemeriksaan; `S20-1` sisi Laboratorium naik `READY_FOR_DOMAIN_DESIGN`** (nol `BLOCKING`, nol `CONFLICT` terbuka); `S20-2` layar pemesan dokter ditunda (`LAB-COORD-023`). Sebelumnya `13` — bagian 0H, 2026-10-20: **`S16b` dipecah; `S16b-1` Laporan Pembatalan dan `S16b-2` Statistik Pesanan naik `READY_FOR_DOMAIN_DESIGN`** (nol `BLOCKING`, nol `CONFLICT` terbuka); `S16b-3` tetap ditunda. Sebelumnya `12` — bagian 0G, 2026-10-06: sesudah putaran 21, **delapan slice atau bagian naik `READY_FOR_DOMAIN_DESIGN`** — `S2b-1`, `S2b-3`, `S4d-2`, `S4e`, `S5`, `S6-1`, `S17-1`, `S18`; `S6` dan `S17` dipecah; dua penahan baru dari kode dan alur — `DEC-LAB-030` (`S2b-2`), `DEC-LAB-034` (`S6-2`); `S17-2` menunggu `LAB-COORD-015`; `S16b` ditunda Phase 2. Sebelumnya `11` — bagian 0F, 2026-09-25: sesudah putaran 19, **`S16a` naik `READY_FOR_DOMAIN_DESIGN`**; `S8` dilebur ke `S5`/`S6`; `S19` keluar dari desain; `S2b` dipecah tiga dengan dua penahan baru `DEC-LAB-028`, `DEC-LAB-029`. Sebelumnya `10` — bagian 0E, 2026-09-25: `S2b`, `S8`, `S16`, `S18`, `S19` dinilai ulang; kelimanya tetap `BUSINESS_DECISION_REQUIRED` dengan penahan bernama `DEC-LAB-022`..`027` dan `LAB-COORD-017`. Sebelumnya `9` — bagian 0D, 2026-09-25: `S4d-1` naik `READY_FOR_DOMAIN_DESIGN`; `S4d-2` dan `S4e` tertahan keputusan klinis baru. Sebelumnya `8` — bagian 0C, 2026-09-24: `S4` naik `READY_FOR_DOMAIN_DESIGN` untuk desain saja |
| Status | `draft` |
| **Kesiapan keseluruhan** | **`PARTIALLY_READY`** — **revision 16 (0J.9): `S17-1` utuh `READY_FOR_DOMAIN_DESIGN`; `S17-2` tetap tertahan.** **revision 15 (0J.6): `S17-1a` dinilai ulang, tetap siap; `S17-1b` `BUSINESS_DECISION_REQUIRED` (`DEC-LAB-035`).** **revision 14 (0I.6): `S20-1` ditambahkan ke slice siap; `S20-2` ditunda.** **Revision 13 (0H.6): `S16b-1` dan `S16b-2` ditambahkan ke slice siap.** **Diperbarui revision 12, lihat 0G.13:** delapan slice atau bagian siap dikirim ke arsitektur domain; tiga tertahan (`S2b-2`, `S6-2`, `S17-2`). *Catatan revision 7 di bawah dipertahankan sebagai riwayat.* **Diperbarui revision 7.** Dua slice siap dikirim: `S4b` dan `S4c`. Catatan revision 6 tentang tiga penahan yang dapat ditutup pemilik modul **sudah dipakai habis pada 2026-09-18**, dan hasilnya tercatat 0B. **Sejak putaran itu, nol penahan tersisa yang dapat ditutup pemilik modul sendirian** — seluruhnya wewenang klinis atau manajemen rumah sakit. Lihat 0B.5 |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Masukan | **Revision 14:** `00-interview-decisions.md` revisi 143 (`LAB-DEC-251`..`277`, BR-157); `01-existing-capability-map.md` revision 10 (BE `4d2c0162` / FE `004524f4c`). **Revision 7:** `00-interview-decisions.md` revision 41; `blueprint-manifest.md` revision 56. **Revision 6:** `00-interview-decisions.md` revision 39; `01-existing-capability-map.md` revision 3; `blueprint-manifest.md` revision 54. **Revision 1-2:** decisions revision 14; capability map revision 1; `05-evidence-reconciliation.md` revision 2 |
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
| `S2b` | Atribut wadah khas Patologi Anatomi dan Mikrobiologi | **Dinilai ulang revision 12, lihat 0G:** `S2b-1` lokasi/pola/metode ✅ dan `S2b-3` fiksasi ✅ **`READY_FOR_DOMAIN_DESIGN`**; `S2b-2` seri Sitologi/FNAB `BUSINESS_DECISION_REQUIRED`. Blok dan slide keluar scope | `S2b-2`: `DEC-LAB-030` — `DEC-LAB-022`, `023`, `028`, `029` ✅ ditutup |
| `S3` | Batas nilai dan batas kritis | **`READY_FOR_DOMAIN_DESIGN`** | — (hanya melayani hasil berbentuk angka dan pilihan) |
| `S4a` | **Pengisian** hasil Patologi Klinik | **`READY`** — dipecah `LAB-DEC-076`, 2026-09-17 | **Nol.** `LAB-DEC-005` (hasil diketik manual analis) berdiri tanpa caveat tanda tangan, setara `LAB-DEC-006` yang sudah dibangun penuh |
| `S4` | **Validasi dan rilis** hasil Patologi Klinik — cakupannya dipersempit `LAB-DEC-076` | `BUSINESS_DECISION_REQUIRED` — **dinilai ulang revision 7, lihat 0B** | ~~`LAB-SIGN-001`~~ ✅, ~~`LAB-P0-001`~~ ↓, ~~`LAB-P0-005`~~ ↓, ~~`LAB-P0-002`~~ ✅ — **tersisa `DEC-LAB-011`** |
| `S4b` | Pengisian hasil Mikrobiologi | ✅ **`READY_FOR_DOMAIN_DESIGN`** — **naik revision 7, lihat 0B.1** | **Nol.** `LAB-OPEN-017` dicabut sebagai penahan oleh `LAB-DEC-081` |
| `S4c` | Pengisian hasil Patologi Anatomi | ✅ **`READY_FOR_DOMAIN_DESIGN`** — naik revision 6, lihat 0A.9 | **Nol.** `DEC-LAB-013` ✅ ditutup `LAB-DEC-083`; `S2b` tetap menempel sebagai dependency, bukan penahan |
| `S4d` | **Validasi dan rilis** hasil Mikrobiologi | `S4d-1` ✅ `READY_FOR_DOMAIN_DESIGN` (revision 9, 0D); **`S4d-2` hasil `Sementara` ✅ `READY_FOR_DOMAIN_DESIGN` — revision 12, lihat 0G** | Nol bagi desain — `DEC-LAB-020` ✅ `LAB-DEC-171`; `DEC-LAB-031`, `032` sebelum kontrak |
| `S4e` | **Validasi dan rilis** hasil Patologi Anatomi | ✅ **`READY_FOR_DOMAIN_DESIGN` — revision 12, lihat 0G** | Nol bagi desain — `DEC-LAB-021` ✅ `LAB-DEC-174`; pemakaian nyata menunggu `LAB-SIGN-002` |
| `S5` | Nilai kritis dan pelaporannya | ✅ **`READY_FOR_DOMAIN_DESIGN` — revision 12, lihat 0G** | Nol bagi desain — `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` ✅; `DEC-LAB-033` sebelum kontrak |
| `S6` | Koreksi hasil setelah rilis | **`PARTIALLY_READY` — revision 12, lihat 0G:** `S6-1` kerangka koreksi dan ganti nilai ✅ `READY_FOR_DOMAIN_DESIGN`; `S6-2` tarik hasil tertahan | `S6-2`: `DEC-LAB-034` — `DEC-LAB-014`, `DEC-LAB-019` ✅ |
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
| `S16` | Laporan operasional | **Dipecah — revision 11, lihat 0F.** ✅ **`S16a` `READY_FOR_DOMAIN_DESIGN`** (tiga laporan, `LAB-DEC-159`); `S16b` **ditunda Phase 2** (`LAB-DEC-181`, revision 12). **Revision 13 (0H):** ✅ **`S16b-1`** Laporan Pembatalan dan ✅ **`S16b-2`** Statistik Pesanan `READY_FOR_DOMAIN_DESIGN`; `S16b-3` tetap ditunda | `S16b-3`: katalog laporan dari pemilik modul |
| `S17` | Label, nota, dan pengiriman hasil ke pasien | **`PARTIALLY_READY` — revision 16, lihat 0J.9:** `S17-1` (dokumen berversi, kirim manual, cetak, unduh, syarat persetujuan) ✅ `READY_FOR_DOMAIN_DESIGN`; `S17-2` tertahan `LAB-COORD-015`. *Revision 12, 0G:* `S17-1` dokumen PDF, gerbang, antrean, cetak dan unduh ✅ `READY_FOR_DOMAIN_DESIGN`; `S17-2` jalur publik dan tautan WhatsApp tertahan | `S17-2`: `LAB-COORD-015`, didahului penetapan pemilik keamanan |
| `S18` | Penautan berkas hasil laboratorium eksternal | ✅ **`READY_FOR_DOMAIN_DESIGN` sisi Laboratorium — revision 12, lihat 0G** | Implementasi: `LAB-COORD-018`, `LAB-COORD-019` — `DEC-LAB-016` ✅ `LAB-DEC-185` |
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

## 0G. Penilaian Ulang Revision 12 — sesudah amendment pass putaran 21 (2026-10-06)

> **Bagian ini menggantikan baris `S2b-1`, `S2b-2`, `S2b-3`, `S4d-2`, `S4e`, `S5`, `S6`, `S17`, dan
> `S18` pada tabel kesiapan sebelumnya** — 0B.1, 0D.5, 0F.6, dan peta 0.2. Slice lain tidak dinilai
> ulang.

### 0G.1 Scope dan bukti

| Butir | Isi |
|---|---|
| Slice yang dinilai | `S2b-1`, `S2b-2`, `S2b-3`, `S4d-2`, `S4e`, `S5`, `S6`, `S17`, `S18` — sembilan slice yang penahannya dijawab putaran 21 |
| Yang **tidak** dinilai | `S16b` — ditunda ke Phase 2 oleh `LAB-DEC-181`, dan tidak dinilai sampai katalog laporannya diserahkan. Slice lain berlaku apa adanya |
| Pemicu | Amendment pass putaran 21 — `LAB-DEC-168`..`LAB-DEC-187`, bukti `LAB-EVD-012`, decisions **rev 85** |
| Snapshot kode | Backend `6924b689`, frontend `a1404291d`. Dibaca langsung untuk dua pertanyaan *apa yang ada hari ini*: tempat nomor cetak (`LabOrder.LabReportNumber`, dialokasikan `LabOrderService.cs:727` dan `:926` dari setelan per disiplin `LabDisciplineSetting`), dan layanan penyimpanan berkas (nol) |
| Capability map | Revision 6 (2026-09-25) — **berpotensi basi** terhadap snapshot ini; tidak dipakai sebagai bukti perilaku baru |
| Baseline rujukan | **Tidak dipakai.** `indonesia-hospital-domain-reference` tidak dipanggil; seluruh butir bersumber keputusan modul ini |

### 0G.2 Satu hal tata kelola yang harus dibaca sebelum tabel kesiapan

Revision 6 sampai 11 menyatakan `DEC-LAB-012`, `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-014`, `DEC-LAB-020`,
dan `DEC-LAB-021` **bukan wewenang pemilik modul sendirian**. Putaran 21 menjawabnya sebagai keputusan
pemilik modul, dan **sendiri** memisahkan dua hal lewat `LAB-SIGN-002`: keputusannya sah untuk
**desain**, sedangkan pernyataan klinis wajib ada sebelum **go-live**.

Gerbang ini menerima pemisahan itu dengan satu uji yang diterapkan per keputusan: **bila pernyataan
klinis kelak menolak, apakah rancangan harus dibongkar?** Bila tidak — penolakan hanya mematikan satu
aturan, menambah satu langkah, atau mengubah isi konfigurasi — ratifikasi diklasifikasikan **tidak
memblokir desain**. Bila ya, ia tetap `BLOCKING`.

| Keputusan | Bila pernyataan klinis menolak | Klasifikasi bagi desain |
|---|---|---|
| `LAB-DEC-174` validasi PA oleh penulis | PA kembali ke aturan empat mata baku yang **sudah berlaku** pada dua disiplin lain | Tidak memblokir — **dengan syarat rancangan**: aturan *pengisi ≠ pemvalidasi* ditegakkan **per disiplin**, sehingga penolakan cukup menyalakan kembali aturan itu bagi PA |
| `LAB-DEC-171`..`173` hasil `Sementara` | Tahap `Sementara` tidak dibuka; `S4d-1` tetap utuh | Tidak memblokir — tahap `Sementara` adalah perluasan, bukan pengganti |
| `LAB-DEC-175` penyetuju perubahan nilai kritis | Penyetujunya berganti orang; mekanisme pengusul ≠ penyetuju dan delegasi tercatat tetap | Tidak memblokir |
| `LAB-DEC-176`, `177` alur kritis | Isi SLA dan aturan berubah; `LAB-DEC-004` tidak dilonggarkan, sehingga bentuknya tetap | Tidak memblokir |
| `LAB-DEC-178`..`180` koreksi | Aturannya sudah lebih ketat dari `LAB-DEC-007`; penolakan hanya mungkin ke arah yang lebih ketat lagi, misalnya penyetuju harus Kepala Instalasi | Tidak memblokir |
| `LAB-DEC-182` hasil normal tanpa persetujuan tambahan | Seluruh kategori disetel *sensitif* — mekanisme yang sama | Tidak memblokir |
| `LAB-DEC-168`..`170` atribut wadah PA | Bentuk data berubah, misalnya satu wadah boleh banyak lokasi | **Sisa risiko yang diterima pemilik modul.** Keputusan pemilik sama dengan usulan baseline gerbang `r11`; dicatat terbuka, tidak memblokir |

### 0G.3 `S2b-1` — lokasi, pola, dan metode pengambilan

Penahan tunggalnya, `DEC-LAB-029`, ditutup `LAB-DEC-168`. Dimensi lain sudah terjawab `LAB-DEC-161`
(0F.2).

| Dimensi | Status | Isi |
|---|---|---|
| 01–04, 06–09, 12–14 | `CONFIRMED` | `LAB-DEC-161`, `LAB-DEC-168`; jejak perubahan `LAB-DEC-112` |
| 05 Exception — wadah fisik **tiba** berisi jaringan dari dua lokasi | `MISSING` — `CONFIGURABLE_DEFAULT` | `LAB-DEC-168` melarang keadaan itu **dicatat**, tetapi tidak mengatur wadah tercampur yang **datang**. Dua jalan sudah ada tanpa konsep baru: menolak wadah dengan alasan dari daftar `LAB-DEC-019`, atau menerimanya dengan lokasi `Lainnya` beserta keterangan (`LAB-DEC-161` butir 2). Pilihannya isi daftar alasan, bukan struktur |
| 05 Exception — wadah tercatat lebih sedikit daripada lokasi yang ditemukan patolog | `PROPOSED` — `NON_BLOCKING_STANDARD` | Memakai jalur tambah/koreksi wadah `S2` yang sudah ada. **Perilakunya perlu diverifikasi `trace-existing-capabilities`** sebelum desain mengandalkannya |
| 10, 11 | Tidak material | Nol modul lain |
| 15, 16, 17 | Tidak material | 17: lokasi yang salah diperbaiki sebelum Final; sesudah Final lewat `S6` |
| 18 | `CONFIRMED` | Cetakan menomori lokasi dari wadahnya (`AC-257`) |

**Kesiapan: ✅ `READY_FOR_DOMAIN_DESIGN`.**

### 0G.4 `S2b-2` — seri nomor Sitologi dan FNAB

`DEC-LAB-022` ditutup `LAB-DEC-170`. Pembacaan kode menemukan satu hal yang keputusan itu belum jawab.

| Dimensi | Status | Isi |
|---|---|---|
| 01, 02, 03, 12, 14, 18 | `CONFIRMED` | `LAB-DEC-170`; nomor order global tetap (`LAB-DEC-072`) |
| 06 Data — **tempat nomor cetak** | **`CONFLICT`** | Hari ini nomor cetak tersimpan **satu per order** (`LabOrder.LabReportNumber`), dialokasikan dari setelan **per disiplin**. Laporan PA **satu per order** (`LAB-DEC-085`), dan order hanya dipecah **per disiplin** (`LAB-DEC-055`). Seri baru berkunci **kategori**. Maka satu order PA yang memuat histopatologi **dan** FNAB punya **satu** laporan dan **satu** nomor, tetapi **dua** seri yang sama-sama berhak |
| 07 Aturan — order campuran kategori | **`MISSING`** — **`BLOCKING`** | Tiga jalan, masing-masing mengubah data tersimpan: (a) pemecahan otomatis `LAB-DEC-055` diperluas — order PA dipecah per seri, sehingga satu order = satu laporan = satu seri; (b) order campuran **ditolak** saat dipesan; (c) satu order memegang lebih dari satu nomor cetak — bertentangan dengan `LAB-DEC-085`. Lihat `DEC-LAB-030` |
| 08 Status | Tidak berlaku | Nomor tidak punya lifecycle |
| 09 Kewenangan | `CONFIRMED` | Kepala Instalasi menyetel bentuk dan pemetaan seri (`LAB-DEC-170` butir 3, 5, 7) |
| 13 Koreksi | `CONFIRMED` | Nomor terbit tidak diubah mundur; peringatan `LAB-OPEN-043` |
| 05, 10, 11, 15, 16, 17 | Tidak material | — |

**Contoh kenapa ini menahan desain:**

> Dokter bedah memesan biopsi payudara kiri dan FNAB kelenjar tiroid dalam satu kunjungan. Pemecahan
> `LAB-DEC-055` menghasilkan **satu** order Patologi Anatomi berisi keduanya, dan laporannya satu
> (`LAB-DEC-085`). Histopatologi meminta nomor `26.0919`, FNAB meminta `F26.0112`. Sistem harus memilih
> salah satu — dan nomor yang sudah terbit tidak dapat diubah mundur.

**Kesiapan: `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-030`.** Satu pertanyaan, dan pemilik modul dapat
menjawabnya.

### 0G.5 `S2b-3` — fiksasi

| Dimensi | Status | Isi |
|---|---|---|
| 01, 02, 06, 07, 12, 14 | `CONFIRMED` | `LAB-DEC-169` |
| 03 Pemicu — kapan dicatat | `PROPOSED` — `NON_BLOCKING_STANDARD` | Pengisi yang sah ada dua: yang memfiksasi, atau yang menerima informasinya. Layar milik Laboratorium hanya menjangkau yang kedua. **Usulan:** dicatat pada penerimaan specimen dan dapat disunting sampai Final dengan jejak, pola `LAB-DEC-161`. Petugas ruang tindakan di modul lain **tidak** diberi layar baru |
| 05 Exception — wadah wajib fiksasi tiba tanpa fiksasi | `CONFIGURABLE_DEFAULT` | Alasan penolakan dari daftar `LAB-DEC-019`, atau diterima beserta catatan |
| 07 Aturan — `Tidak Memerlukan Fiksasi` pada pemeriksaan yang disetel wajib | `PROPOSED` — `NON_BLOCKING_STANDARD` | **Ditolak**, sebab setelan kewajiban adalah keputusan Kepala Instalasi; pengecualian per wadah memakai catatan |
| 08 | Tidak berlaku | Fakta per wadah, bukan status |
| 09 | `PROPOSED` — `NON_BLOCKING_STANDARD` | Izin penerimaan specimen yang sudah ada |
| 13 | `PROPOSED` — `NON_BLOCKING_STANDARD` | Sesudah Final: `S6` |
| 10, 11, 15, 16 | Tidak material | — |
| 17 | Material — bentuknya `CONFIRMED` | Jenis dan jam mulai fiksasi memengaruhi mutu pewarnaan IHK; ruasnya wajib ada, isi daftarnya `LAB-OPEN-050` |
| 18 | `CONFIRMED` | Tercetak `Fiksasi:` pada laporan PA |

**Kesiapan: ✅ `READY_FOR_DOMAIN_DESIGN`.**

### 0G.6 `S4d-2` — hasil Mikrobiologi `Sementara`

| Dimensi | Status | Isi |
|---|---|---|
| 01, 02, 04, 06, 07, 09, 12, 14 | `CONFIRMED` | `LAB-DEC-171`; empat mata dan dua lapis per disiplin diwarisi `S4d-1` |
| 03 | `CONFIRMED` | Hasil Final berkualifikasi `Sementara` |
| 05 Exception — **lebih dari satu tahap `Sementara`** | `MISSING` — tidak memblokir desain | BR-68: biakan dibaca bertahap berhari-hari — Gram hari 1, pertumbuhan hari 2, identifikasi hari 3. Lintasan `LAB-EVD-012` menyebut **satu** `Sementara`. Rancangan yang mengizinkan **n** tahap sudah mencakup jawaban *satu*; pembatasannya hanya aturan validasi. Dikunci sebelum kontrak — `DEC-LAB-031` |
| 05 Exception — `Definitif` tidak pernah dapat dihasilkan (biakan terkontaminasi, specimen habis) | `PROPOSED` — `NON_BLOCKING_STANDARD` | Pembatalan sesudah rilis ditolak (`LAB-DEC-179`), sehingga satu-satunya jalan yang sah adalah tahap `Definitif` yang **menyatakan sebabnya**. Nol mekanisme baru |
| 08 Status | `CONFIRMED` | Tahap disimpan sebagai versi; `LAB-DEC-080` utuh — fakta per tahap, bukan status |
| 10 Dependency — **pantau cito `S7` dan laporan `S16a` yang sudah dibangun** | **`MISSING`** — tidak memblokir desain | Keduanya membaca **satu** waktu rilis per pemeriksaan (`AC-17`, `LAB-DEC-159`). Dengan dua rilis, perlu ditetapkan rilis mana yang dihitung. Rancangan menyimpan **kedua** waktu, sehingga strukturnya sama apa pun jawabannya; yang berubah hanya nilai yang dibaca kedua pemakai lama. Dikunci sebelum kontrak — `DEC-LAB-032` |
| 11 Integrasi — rekam medis | `PROPOSED` — `NON_BLOCKING_STANDARD` | `Definitif` sering tiba **sesudah kunjungan rawat jalan ditutup** dan dokumennya terkunci. Mekanisme addendum `LAB-DEC-020` ditulis untuk **koreksi**. **Usulan:** dipakai pula bagi tahap `Definitif` dengan label *tahap definitif*, bukan *koreksi*. **Perlu diverifikasi** apakah dokumen hasil lab `INT-08` (`BE-LAB-70`) menerimanya |
| 13 | `CONFIRMED` | `S6` bagi tahap yang ternyata salah (`LAB-DEC-171` butir 4) |
| 15 | `CONFIRMED` | `LAB-DEC-173` |
| 16 | Tidak material | `LAB-INH-009` |
| 17 | Material — `CONFIRMED` | `LAB-DEC-171`..`173`; ratifikasi `DR-LAB-002` (0G.2) |
| 18 | `CONFIRMED` | Kedua versi terbaca pada riwayat |

**Kesiapan: ✅ `READY_FOR_DOMAIN_DESIGN`.** `DEC-LAB-031` dan `DEC-LAB-032` wajib dijawab **sebelum
kontrak dikunci**, bukan sebelum arsitektur.

### 0G.7 `S4e` — validasi dan rilis Patologi Anatomi

| Dimensi | r9 | **r12** | Isi |
|---|---|---|---|
| 02 Aktor, 07 Aturan, 09 Kewenangan | `CONFLICT` | **`CONFIRMED`** | `LAB-DEC-174`: penulis memvalidasi, Sp.PA lain merilis, analis tidak pernah |
| 04, 05 Alur | `MISSING` | **`CONFIRMED`** | Draft → Final → validasi oleh penulis → rilis oleh Sp.PA lain |
| 05 Exception — perilis tidak sependapat dengan laporan | — | `PROPOSED` — `NON_BLOCKING_STANDARD` | Perilis **mengembalikan** laporan kepada penulis dengan alasan dari daftar — padanan `LAB-DEC-138` |
| 06 Satuan validasi | `PROPOSED` | `PROPOSED` — `NON_BLOCKING_STANDARD` | Per order, mengikuti `LAB-DEC-085` (r9) |
| 09 Penegakan *analis tidak pernah* | — | `PROPOSED` — `NON_BLOCKING_STANDARD` | Pola penegakan *hanya dokter* `LAB-DEC-150` pada `S4` |
| 13 | `PROPOSED` | **`CONFIRMED`** | Sebelum rilis: dikembalikan; sesudah rilis: `S6` |
| 01, 03, 08, 11, 12, 14 | `CONFIRMED` | `CONFIRMED` | r9 |
| 10 | `PROPOSED` | `PROPOSED` | `LAB-COORD-016` — kode PA di katalog HR; penahan go-live |
| 15, 16 | Tidak material | Tidak material | Kritis PA milik `S5` |
| 17 | Material | Material — `CONFIRMED` | `LAB-DEC-174` butir 5: rilis butuh sekurang-kurangnya dua Sp.PA berkewenangan |
| 18 | `PROPOSED` | `PROPOSED` | Baris pengesah pada cetakan PA — `S17` |

**Kesiapan: ✅ `READY_FOR_DOMAIN_DESIGN`**, dengan **syarat rancangan** 0G.2 — empat mata ditegakkan
per disiplin. Pemakaian nyata menunggu tanda tangan `DR-LAB-003` (`LAB-SIGN-002`) dan dua Sp.PA
berkewenangan.

### 0G.8 `S5` — nilai kritis dan pelaporannya

| Dimensi | Status | Isi |
|---|---|---|
| 01, 06, 14, 18 | `CONFIRMED` | `LAB-DEC-004`, `LAB-DEC-136`, `LAB-DEC-176` butir 5 |
| 02 Aktor | `CONFIRMED` | Pelapor: petugas lab. Penerima: dokter pemesan, ditambah DPJP/unit bila episode aktif. Penyetuju perubahan batas: Kepala Instalasi atau delegasinya (`LAB-DEC-175`). **Penerima eskalasi** adalah konfigurasi — isinya `LAB-OPEN-047`, penahan go-live |
| 03 Pemicu — **titik picu** | `CONFIRMED` — turunan langsung | Alur dimulai **saat hasil dirilis**: `LAB-DEC-004` *"tetap dirilis … belum tuntas sampai pelaporan terisi"* dan `LAB-DEC-155` *"yang belum dirilis tidak dipakai untuk keputusan klinis"*. Jam SLA mulai saat rilis. **Usulan** (`NON_BLOCKING_STANDARD`): hasil yang memenuhi aturan kritis sudah ditandai sejak diisi, agar pemvalidasi dan perilis mendahulukannya |
| 04 | `CONFIRMED` | `LAB-DEC-176` |
| 05 Exception — hasil kritis kemudian dikoreksi | `PROPOSED` — `NON_BLOCKING_STANDARD` | Kejadian kritis yang masih terbuka ditutup bertanda *dikoreksi* dengan rujukan versi `S6`; bila nilai koreksinya juga kritis, kejadian baru dibuka |
| 05 Exception — dokter perujuk luar tanpa akun | `PROPOSED` | Pelaporan lisan tetap berjalan; diteruskan ke arsitektur (`LAB-DEC-176`, catatan) |
| 07 Aturan — Patologi Klinik | `CONFIRMED` | Batas kritis pada `LabValueBound` (`S3`) |
| 07 Aturan — Mikrobiologi | `CONFIRMED` bentuknya | `LAB-DEC-103`; isinya `LAB-OPEN-041` |
| 07 Aturan — Patologi Anatomi | `PROPOSED` — `NON_BLOCKING_STANDARD` | Arah turunan `LAB-DEC-176` butir 6: `Status Hasil PA` = Kritis ditambah kondisi dari daftar; isinya `LAB-OPEN-046` |
| 08 Status — *Status Critical* | `PROPOSED` — `NON_BLOCKING_STANDARD` | Dibaca sebagai keadaan **kejadian kritis** — terpicu → diberitahukan → dilaporkan atau tereskalasi → selesai — **bukan** status pemeriksaan. `LAB-DEC-080` utuh |
| 09 Kewenangan — perubahan batas kritis | `CONFIRMED` | `LAB-DEC-175`. **Membuka `AC-33`**, yang tertahan sejak 2026-09-17 |
| 09 Kewenangan — perubahan SLA dan aturan kritis Mikro/PA | `MISSING` — `NON_BLOCKING_STANDARD` | `LAB-DEC-175` hanya menyebut *batas/nilai kritis*. Memanjangkan SLA dari 15 menjadi 60 menit sama berbahayanya dengan menaikkan batas Kalium. **Usulan sisi yang lebih ketat:** ketiganya lewat pengusul ≠ penyetuju yang sama — melonggarkannya kelak hanya mencabut satu langkah. `DEC-LAB-033` |
| 10 Dependency | `CONFIRMED` | `LAB-COORD-017` (pemberitahuan platform — penyediaan), `LAB-COORD-014` (penugasan jaga), dan **episode aktif** dari status kunjungan Registrasi — arti *aktif* perlu diverifikasi `trace-existing-capabilities` |
| 11 | `CONFIRMED` | WhatsApp lewat platform (`LAB-DEC-183`), tanpa data klinis (`LAB-DEC-137`) |
| 12 | `CONFIRMED` | Laporan lengkap; daftar pantau kosong |
| 13 | `PROPOSED` — `NON_BLOCKING_STANDARD` | Catatan pelaporan yang salah isi diperbaiki dengan catatan tambahan, tidak ditimpa |
| 15 | `CONFIRMED` | `LAB-DEC-176` |
| 16 | Tidak material | — |
| 17 | Material — bentuknya `CONFIRMED` | Isi klinis adalah penahan go-live (`LAB-OPEN-041`, `046`, `047`); pemakaian `S4` sebelum `S5` tetap `DEC-LAB-017` |

**Kesiapan: ✅ `READY_FOR_DOMAIN_DESIGN`.** Ketiga penahan sejak revision 6 — `LAB-P0-004`,
`LAB-OPEN-014`, `DEC-LAB-012` — tertutup pada bentuknya. `DEC-LAB-033` dikunci sebelum kontrak.

### 0G.9 `S6` — koreksi hasil sesudah rilis

| Dimensi | Status | Isi |
|---|---|---|
| 01, 02, 04, 06, 07, 09, 14, 15 | `CONFIRMED` | `LAB-DEC-178`; alasan dari daftar `LAB-DEC-082` |
| 03 | `CONFIRMED` | Hasil terrilis ditemukan salah |
| 05 Exception — pengajuan ditolak penyetuju | `PROPOSED` — `NON_BLOCKING_STANDARD` | Pengajuan ditutup *ditolak* beralasan; versi terrilis tetap aktif |
| 05 Exception — dua pengajuan atas hasil yang sama | `PROPOSED` — `NON_BLOCKING_STANDARD` | Satu pengajuan terbuka per hasil |
| 05 Exception — kunjungan sudah ditutup | `CONFIRMED` | Addendum `LAB-DEC-020` |
| 08 Status | `PROPOSED` — `NON_BLOCKING_STANDARD` | Keadaan **pengajuan** (diajukan → disetujui/ditolak) dan **versi** (aktif/`Superseded`/`Invalidated`) — bukan status pemeriksaan. `LAB-DEC-080` utuh |
| 08 Status — **pemeriksaan sesudah tarik hasil** | **`MISSING`** — **`BLOCKING` bagi tarik hasil** | `LAB-DEC-179` menyatakan versi yang ditarik `Invalidated` tanpa pengganti, tetapi tidak menyatakan **apa yang terjadi pada pemeriksaannya**: berhenti selamanya dan pemeriksaan ulang dipesan sebagai pemeriksaan **baru**, atau dibuka kembali untuk specimen baru. Order yang sudah *Selesai* (`LAB-DEC-154`) ikut terdampak, dan penambahan pemeriksaan sesudah kelayakan dikunci (`LAB-DEC-049`). `DEC-LAB-034` |
| 10 Dependency | `CONFIRMED` | Rekam medis (addendum), pemberitahuan platform `LAB-COORD-017`, antrean kirim `S17` (`LAB-DEC-180`) |
| 11 | `CONFIRMED` | Dokumen hasil lab di rekam medis (`INT-08`) |
| 12 | `CONFIRMED` | Versi koreksi aktif; dokter pemesan dan DPJP tahu |
| 13 | `CONFIRMED` | Slice ini sendiri |
| 16 Billing — **tarik hasil dan pemeriksaan ulang** | **`MISSING`** — **`BLOCKING` bagi tarik hasil** | Koreksi tagihan adalah wewenang Billing (`LAB-INH-010`, `LAB-DEC-049`), tetapi Laboratorium harus menyatakan **fakta apa** yang diterbitkannya, dan **siapa menanggung** pemeriksaan ulang bila sebabnya kesalahan internal — pola penanda kesalahan internal `LAB-DEC-019`. Bagian dari `DEC-LAB-034`. **Ganti nilai nol dampak tagihan** |
| 17 | Material — `CONFIRMED` | `LAB-DEC-178`, `179` |
| 18 | `CONFIRMED` | Versi bernomor dan alasan dari daftar dapat dihitung |

**Contoh kenapa tarik hasil belum dapat dirancang:**

> Kalium Andi 6,4 dirilis 10.00, dan ordernya *Selesai*. Pukul 11.00 ketahuan tabungnya milik Budi, dan
> hasilnya ditarik. Andi perlu diambil darah lagi. Bila pemeriksaan Kalium yang lama **dibuka kembali**,
> order *Selesai* harus mundur dan Andi tidak ditagih dua kali. Bila pemeriksaan ulang adalah
> pemeriksaan **baru**, order lama tetap *Selesai* dan Kalium baru ditagih — kecuali ada penanda bahwa
> kesalahannya milik laboratorium. Kedua jalan sah, dan keduanya mengubah data tersimpan serta uang.

**Kesiapan: `PARTIALLY_READY`.**

| Bagian | Kesiapan | Penahan |
|---|---|---|
| **`S6-1`** — kerangka koreksi dan **ganti nilai** | ✅ `READY_FOR_DOMAIN_DESIGN` | Nol |
| **`S6-2`** — **tarik hasil** beserta akibatnya | `BUSINESS_DECISION_REQUIRED` | `DEC-LAB-034` |

`S6-1` berdiri sendiri: pengajuan, alasan, persetujuan, versi, pemberitahuan, dan antrean kirim sama bagi
kedua jenis koreksi; `S6-2` hanya menambah satu jenis beserta akibatnya.

### 0G.10 `S17` — label, nota, dan penyerahan hasil kepada pasien

Penahan lama sejak revision 3 berbunyi *"privasi pengiriman belum diputuskan"*. Putaran 21 memutuskan
**modelnya**; **persetujuan keamanannya** belum ada.

| Dimensi | Status | Isi |
|---|---|---|
| 01, 02, 04, 12 | `CONFIRMED` | `LAB-DEC-067` (satu order satu dokumen), `LAB-DEC-068` (Petugas Lab/Admin), `LAB-DEC-139` (satu gerbang), `LAB-DEC-182` |
| 03 | `CONFIRMED` | Order Selesai (`LAB-DEC-139`, `LAB-DEC-172`) |
| 05 Exception — WhatsApp gagal | `CONFIRMED` | `LAB-DEC-183` butir 5 |
| 05 Exception — dokumen digantikan | `CONFIRMED` | `LAB-DEC-180` |
| 05 Exception — penerima selain pasien | `PROPOSED` — `NON_BLOCKING_STANDARD` | Hanya pasien (`LAB-DEC-067`); penerima lain — perujuk, perusahaan — di luar `S17` (`LAB-DEC-164`) |
| 06 Data | `CONFIRMED` | `LAB-DEC-183` butir 1. **Usulan** (`NON_BLOCKING_STANDARD`): *nomor dokumen* = nomor cetak `LAB-DEC-117`/`170` ditambah versi |
| 07 Aturan — kategori sensitif | `CONFIRMED` bentuknya | Isinya `LAB-OPEN-048` |
| 07 Aturan — **gerbang bagi PA** | `MISSING` — `NON_BLOCKING_STANDARD` | `LAB-DEC-139` butir 6 berlaku *"PK dan Mikrobiologi pada putaran ini"*, sedangkan `LAB-DEC-067` mencakup ketiga disiplin. **Usulan:** satu gerbang bagi ketiganya; laporan PA yang memerlukan pendampingan dokter diatur lewat kategori sensitif |
| 08 Status kirim | `CONFIRMED` | `Pending` → `Sent` → `Delivered`/`Failed` |
| 09 | `CONFIRMED` | `LAB-DEC-068`, `LAB-DEC-182` |
| 10 Dependency | `CONFIRMED` | Pembangkit PDF dan WhatsApp (`LAB-COORD-011` — penyediaan); penyimpanan privat bila PDF disimpan (`LAB-COORD-019`) |
| 11 Integrasi — **akses tanpa login** | **`MISSING`** — **`BLOCKING` bagi bagian publik** | Halaman verifikasi QR dan **tautan aman** pasien menunggu persetujuan pemilik keamanan/platform (`LAB-COORD-015`). **`owners.security` pada manifest masih *belum ditetapkan*** — persetujuan tidak dapat diberikan sampai pemiliknya ada |
| 13 | `CONFIRMED` | `LAB-DEC-180`; token dicabut `LAB-DEC-184` |
| 14 | `CONFIRMED` | Seluruh pengiriman teraudit |
| 15 | `CONFIRMED` | Pasien menerima tautan |
| 16 | Tidak material | Penyerahan tidak menagih |
| 17 | Material — `CONFIRMED` | Pasien hanya menerima dokumen `Definitif` (`LAB-DEC-172`); kategori sensitif (`LAB-DEC-182`) |
| 18 | `CONFIRMED` | QR dan nomor dokumen berversi |

**Bukti cetak yang masih kurang** — `LAB-OPEN-039` (varian Mikrobiologi; Sitologi, IHK, FNAB) dan
`LAB-OPEN-032` (ukuran cetak) — menahan **tata letak**, bukan domain.

**Kesiapan: `PARTIALLY_READY`.**

| Bagian | Kesiapan | Penahan |
|---|---|---|
| **`S17-1`** — dokumen hasil PDF (nomor, versi, QR bertoken), gerbang penyerahan berbasis peran dan kategori sensitif, antrean dan status kirim, cetak dan unduh di Quilvian, Nota dan Label | ✅ `READY_FOR_DOMAIN_DESIGN` | Nol bagi desain; penyediaan PDF dan WhatsApp menahan implementasi |
| **`S17-2`** — halaman verifikasi publik tanpa login, **dan** pengiriman tautan aman lewat WhatsApp | `BUSINESS_DECISION_REQUIRED` | `LAB-COORD-015`, didahului penetapan pemilik keamanan |

`S17-1` berdiri sendiri: token QR diterbitkan dan dicabut di `S17-1`; `S17-2` hanya membuka jalan publik
untuk membacanya dan mengirim tautannya. Selama `S17-2` tertahan, hasil dapat dicetak dan diunduh di
loket, tetapi **tidak** dikirim lewat WhatsApp.

### 0G.11 `S18` — penautan berkas hasil laboratorium eksternal

| Dimensi | r11 | **r12** | Isi |
|---|---|---|---|
| 06 Tempat berkas | `MISSING` — `BLOCKING` | **`CONFIRMED`** | `LAB-DEC-185`. Penyediaannya `LAB-COORD-019` — implementasi |
| 08, 09 Arti `Verified`/`Approved` | `MISSING` — `BLOCKING` | **`CONFIRMED`** dari sisi Laboratorium | `LAB-DEC-186`. **Usulan** (`NON_BLOCKING_STANDARD`): verifikasi dilakukan pengunggah saat unggah, sebab cek dua identitas `LAB-DEC-158` terjadi saat itu |
| 11 Kontrak tulis ke Clinical Management | `MISSING` — `BLOCKING` | **`PROPOSED`** — menahan implementasi | Laboratorium memakai **API** dokumen klinis pasien, bukan menulis tabelnya. Tiga perubahan harus diminta kepada pemiliknya: menerima rujukan berkas dari penyimpanan privat; `EnteredInError` wajib beralasan dan dibatasi pengunggah atau Kepala Instalasi (`LAB-CONFLICT-015`); `Approved` dipisah dari izin `Update` (`LAB-CONFLICT-016`). Bentuknya permintaan lintas modul — pola `BE-EXT` |
| 01, 02, 04, 12, 13, 14, 17 | `CONFIRMED` | `CONFIRMED` | `LAB-DEC-157`, `158` |
| 15, 16 | Tidak material | Tidak material | — |
| 18 | `CONFIRMED` | `CONFIRMED` | Linimasa rekam medis |

**Sisa risiko yang wajib dibaca:** bila pemilik `clinical-management` **menolak** perubahan pada baris 11,
`AC-249` (salah input oleh pengunggah atau Kepala Instalasi) dan `AC-275` (petugas lab ditolak
`Approved`) tidak dapat dipenuhi, dan `LAB-DEC-158`/`186` dibuka kembali. Karena itu permintaan lintas
modul diajukan **sebelum** task Laboratorium pertama, bukan sesudahnya.

**Kesiapan: ✅ `READY_FOR_DOMAIN_DESIGN`** bagi sisi Laboratorium dan rumusan permintaan kepada
Clinical Management. Implementasi tertahan `LAB-COORD-018` (termasuk `LAB-CONFLICT-015`/`016`) dan
`LAB-COORD-019`.

### 0G.12 Decision Log — lima butir baru

Dicatat **tanpa keputusan**; penutupannya lewat `grill-me`.

| Decision ID | Pertanyaan | Slice | Bukti saat ini | Usulan baseline | Pemilik | Status | Dampak |
|---|---|---|---|---|---|---|---|
| `DEC-LAB-030` | Bila satu order Patologi Anatomi memuat pemeriksaan dari **seri nomor berbeda** — misalnya histopatologi dan FNAB — seri mana yang dipakai? | `S2b-2` | `LabOrder.LabReportNumber` satu per order; `LAB-DEC-055` memecah per disiplin; `LAB-DEC-085` satu laporan PA per order; `LAB-DEC-170` seri per kategori | Pemecahan otomatis `LAB-DEC-055` diperluas: order PA dipecah **per seri**, sehingga satu order = satu laporan = satu nomor | Yoga Aji Pratama; konfirmasi `DR-LAB-003` | `OPEN` | **`BLOCKING`** bagi `S2b-2` |
| `DEC-LAB-031` | Bolehkah **lebih dari satu** tahap `Sementara` dirilis sebelum `Definitif`? | `S4d-2` | BR-68: bacaan bertahap; lintasan `LAB-DEC-171` menyebut satu | Boleh; setiap tahap dirilis dengan empat mata; `Definitif` menutup tahapan; pemberitahuan `LAB-DEC-173` sekali, saat `Definitif` | Yoga Aji Pratama + `DR-LAB-002` | `OPEN` | Tidak memblokir desain; **dikunci sebelum kontrak** |
| `DEC-LAB-032` | Bagi pemeriksaan yang dirilis `Sementara` lalu `Definitif`: rilis **mana** yang dipakai **jumlah pemeriksaan** dan **TAT** `S16a`, serta **keterlambatan cito** `S7`? | `S4d-2`, berdampak `S7`/`S16a` | `AC-17`, `LAB-DEC-159` memakai satu waktu rilis; keduanya **sudah dibangun** | Dihitung pada **rilis pertama** — hasil resmi pertama (`LAB-DEC-155`); waktu `Definitif` disimpan dan dapat menjadi ukuran kedua kelak | Yoga Aji Pratama + Kepala Instalasi | `OPEN` | Tidak memblokir desain; **dikunci sebelum kontrak**; mengubah perilaku `S7`/`S16a` yang sudah berjalan |
| `DEC-LAB-033` | Apakah perubahan **`Critical Notification SLA`** dan **aturan kritis Mikrobiologi/PA** melewati persetujuan pengusul ≠ penyetuju yang sama dengan batas kritis (`LAB-DEC-175`)? | `S5` | `LAB-DEC-175` hanya menyebut *batas/nilai kritis* | **Ya** — sisi yang lebih ketat | Yoga Aji Pratama + Kepala Instalasi | `OPEN` | `NON_BLOCKING_STANDARD`; **dikunci sebelum kontrak** |
| `DEC-LAB-034` | Sesudah **tarik hasil**: (1) pemeriksaannya berhenti dan pemeriksaan ulang dipesan **baru**, atau dibuka kembali untuk specimen baru? (2) Order yang sudah *Selesai* tetap Selesai? (3) Fakta apa yang diterbitkan untuk Billing, dan siapa menanggung pemeriksaan ulang bila kesalahannya internal? | `S6-2` | `LAB-DEC-179` (tarik tanpa pengganti); `LAB-DEC-154`; `LAB-DEC-049`; `LAB-INH-010`; pola `LAB-DEC-019` | (1) Pemeriksaan **berhenti**, pemeriksaan ulang adalah pemeriksaan **baru**; (2) order lama tetap Selesai; (3) penarikan tidak menerbitkan fakta tagih baru — alasan koreksi membawa penanda **kesalahan internal** yang dibaca Billing, dan koreksi uangnya tetap wewenang Billing | Yoga Aji Pratama + `billing-kasir` | `OPEN` | **`BLOCKING`** bagi `S6-2` |

### 0G.13 Kesiapan

| Slice | Sebelumnya | **Revision 12** | Penahan atau syarat |
|---|---|---|---|
| `S2b-1` lokasi, pola, metode | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-029` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | Nol |
| `S2b-2` seri Sitologi/FNAB | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-022` | **`BUSINESS_DECISION_REQUIRED`** — penahannya **berganti** | `DEC-LAB-030`, ditemukan dari kode |
| `S2b-3` fiksasi | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-028` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | Nol |
| `S4d-2` hasil `Sementara` | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-020` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | `DEC-LAB-031`, `032` sebelum kontrak |
| `S4e` validasi dan rilis PA | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-021` | ✅ **`READY_FOR_DOMAIN_DESIGN`** | Syarat rancangan 0G.2 |
| `S5` nilai kritis | `BUSINESS_DECISION_REQUIRED` — tiga penahan | ✅ **`READY_FOR_DOMAIN_DESIGN`** | `DEC-LAB-033` sebelum kontrak |
| `S6` koreksi | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-014` | **`PARTIALLY_READY`** — `S6-1` ✅, `S6-2` tertahan | `DEC-LAB-034` |
| `S17` penyerahan ke pasien | `BUSINESS_DECISION_REQUIRED` — privasi | **`PARTIALLY_READY`** — `S17-1` ✅, `S17-2` tertahan | `LAB-COORD-015`, didahului penetapan pemilik keamanan |
| `S18` berkas eksternal | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-016`, `LAB-COORD-018` | ✅ **`READY_FOR_DOMAIN_DESIGN`** — sisi Laboratorium | Implementasi: `LAB-COORD-018`, `LAB-COORD-019` |
| `S16b` delapan laporan | `BUSINESS_DECISION_REQUIRED` — `DEC-LAB-025` | **Ditunda Phase 2** — tidak dinilai | Katalog laporan (`LAB-DEC-181`) |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`** — delapan slice atau bagian naik ke
`READY_FOR_DOMAIN_DESIGN` (`S2b-1`, `S2b-3`, `S4d-2`, `S4e`, `S5`, `S6-1`, `S17-1`, `S18`); tiga bagian
masih tertahan (`S2b-2`, `S6-2`, `S17-2`), dua di antaranya oleh temuan baru gerbang ini.

**Penahan go-live yang ikut setiap handoff** — tidak menahan desain (`LAB-DEC-187`): `LAB-SIGN-002`,
`LAB-OPEN-041`, `LAB-OPEN-046`..`050`, `DEC-LAB-017`, `LAB-OPEN-044`, `LAB-OPEN-045`, `UNK-P14-03`,
validator kedua per disiplin, dan `LAB-COORD-016`.

### 0G.14 Apa yang boleh berjalan dan apa yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| Delapan slice atau bagian ke `hospital-domain-architect` | Desain seri nomor `S2b-2` sampai `DEC-LAB-030` |
| `trace-existing-capabilities` impact scan — capability map rev 6 basi; empat pertanyaan khusus pada 0G.15 | Desain tarik hasil `S6-2` sampai `DEC-LAB-034` |
| `grill-me` atas `DEC-LAB-030`..`034` — seluruhnya berusulan baseline dan dapat dijawab dalam satu sesi | Halaman publik dan pengiriman tautan WhatsApp `S17-2` sampai `LAB-COORD-015` |
| Menyusun permintaan lintas modul `S18` kepada pemilik `clinical-management` | Mengunci kontrak `S4d-2` dan `S5` sebelum `DEC-LAB-031`..`033` dijawab |

### 0G.15 Handoff

**Ke `hospital-domain-architect`:**

| Field | Nilai |
|---|---|
| Slice | `S2b-1`, `S2b-3`, `S4d-2`, `S4e`, `S5`, `S6-1`, `S17-1`, `S18` (sisi Laboratorium) |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, masing-masing **independen** dari bagian yang tertahan |
| Snapshot bukti | Decisions rev 86; `LAB-EVD-012`; backend `6924b689`; frontend `a1404291d`; capability map rev 6 (berpotensi basi) |
| Decision ID yang mengikat | `LAB-DEC-168`..`187`, beserta yang mengikat `S4` dan `S4d-1` (0C.6, 0D.6) |
| Pola yang diwarisi | `S4d-2` dan `S4e` **memperluas** rancangan `S4`/`S4d-1` (`LAB-DA-001` A5) — jangan rancang ulang |
| Syarat rancangan wajib | (1) Empat mata ditegakkan **per disiplin** (0G.2). (2) `S4d-2` menyimpan waktu rilis **setiap tahap**. (3) *Status Critical* adalah keadaan kejadian kritis, bukan status pemeriksaan. (4) `S18` memakai API Clinical Management, bukan tabelnya |
| Usulan yang wajib tetap ditandai usulan | Seluruh baris `PROPOSED` pada 0G.3 sampai 0G.11 |
| Dependency | `LAB-COORD-011`, `017`, `019` (penyediaan); `LAB-COORD-016`; `LAB-COORD-018` |
| Yang **tidak boleh** muncul | Seri nomor per kategori (`S2b-2`); akibat tarik hasil pada pemeriksaan, order, dan tagihan (`S6-2`); jalur tanpa login (`S17-2`); status hasil baru pada pemeriksaan (`LAB-DEC-080`) |

**Ke `trace-existing-capabilities`** (impact scan, sebelum atau sejajar arsitektur): (1) jalur tambah dan
koreksi wadah sesudah diterima (`S2b-1` dimensi 05); (2) dokumen hasil lab `INT-08` dan addendum rekam
medis bagi tahap `Definitif` (`S4d-2` dimensi 11); (3) arti *episode perawatan aktif* dan DPJP pada
kunjungan (`S5` dimensi 10); (4) API dokumen klinis pasien — perilaku `EnteredInError`, `Approved`, dan
unggah (`S18` dimensi 11).

**Ke `grill-me`:** `DEC-LAB-030` dan `DEC-LAB-034` lebih dulu — keduanya membuka bagian yang tertahan —
lalu `DEC-LAB-031`..`033` sebelum kontrak. Kelimanya berusulan baseline.

**Ke pemilik platform dan keamanan:** penetapan `owners.security`, lalu `LAB-COORD-015`.

---

## 0H. Penilaian Ulang Revision 13 — dua laporan `S16b` sesudah putaran 30 dan 31 (2026-10-20)

Diminta pemilik modul sesudah gerbang `/design-business-module` menolak masuk karena `S16b` masih *ditunda Phase 2*
(0G.13). Putaran 30 dan 31 melengkapi **kesebelas butir definisi** `LAB-DEC-181` bagi **dua** laporan; delapan laporan
lain tetap ditunda.

### 0H.1 Identitas penilaian

| Field | Nilai |
|---|---|
| Slice yang dinilai | **`S16b-1`** Laporan Pembatalan Order & Pemeriksaan; **`S16b-2`** Statistik Pesanan Laboratorium. Sisanya menjadi **`S16b-3`** (tetap ditunda) |
| Alasan dipecah dua | Ketergantungannya berbeda: `S16b-1` seluruhnya data Laboratorium tetapi menampilkan **identitas pasien**; `S16b-2` agregat tanpa pasien tetapi **membaca kunjungan milik Registrasi**. Satu dapat tertahan tanpa menahan yang lain |
| Bukti utama | `00-interview-decisions.md` **revisi 115** — putaran 30 (`LAB-DEC-230`..`245`, BR-142..BR-152) dan putaran 31 (`LAB-DEC-246`..`249`, BR-153..BR-156); `LAB-EVD-014` (artifact Menu Laporan v2, disimpan di `evidence/2026-10-20-menu-laporan-v2.md`) |
| Bukti implementasi | `01-existing-capability-map.md` **revision 9** (`CAP-P30-01`..`17`) pada BE `4d3e4f98` / FE `7fb1e1d3e` — sama dengan `HEAD`, **segar**; 18 kueri baca-saja devYoga |
| Wewenang | Seluruh keputusan **disetujui pemilik modul** Yoga Aji Pratama, 2026-10-20, setiap pertanyaan dengan pilihan yang direkomendasikan |
| Baseline rujukan | **Tidak dipakai** — kedua definisi sudah diputuskan pemilik modul |
| Catatan wewenang | Batas scope putaran 30/31 **tidak dikonfirmasi eksplisit** tetapi tidak dibantah sepanjang 21 pertanyaan; seluruh keputusan yang dinilai di sini berada di dalamnya. Tidak menahan |

### 0H.2 `S16b-1` — Laporan Pembatalan Order & Pemeriksaan: kedelapan belas dimensi

| Dimensi | Status | Isi |
|---|---|---|
| 01 Tujuan | `CONFIRMED` | Audit pembatalan — siapa membatalkan apa, kapan, mengapa (`LAB-DEC-240`, `RPT-007`). Pengganti *Audit Delete* artifact (`LAB-DEC-232`) |
| 02 Aktor | `CONFIRMED` | Pembaca: kepala instalasi dan manajemen secara bawaan; admin dapat memberikannya kepada jabatan lain (`LAB-DEC-239`). Pelaku yang dilaporkan: pembatal order dan pembatal pemeriksaan |
| 03 Pemicu / prasyarat | `CONFIRMED` | Pembaca membuka laporan dan memilih periode. **Prasyarat data** sudah berdiri: riwayat `Order.Cancel`/`Examination.Cancel` (`CAP-P30-06`) |
| 04 Alur utama | `CONFIRMED` | Pilih periode dan penyaring → tabel satu baris per peristiwa → buka detail → unduh `.xlsx` atau cetak (BR-151) |
| 05 Alternatif / exception | `CONFIRMED` + `PROPOSED` | **Dikonfirmasi:** pembatalan lama tanpa alasan tampil *Alasan tidak dicatat* (`LAB-DEC-237`); cetak gagal dicatat → tidak dibuka (`LAB-DEC-248`); periode > 366 hari ditolak (`VAL-149`). **Usulan** (`NON_BLOCKING_STANDARD`): periode tanpa pembatalan tampil kosong beralasan, pola `S16a` |
| 06 Data minimum | `CONFIRMED` | Dua belas kolom BR-151, termasuk **No. RM dan nama pasien**, petugas pembatal, status sebelum, *Sudah layak tagih*. Identitas pasien dibaca lewat jalur rincian order yang sudah ada (`LAB-DEC-166`) |
| 07 Aturan / validasi | `CONFIRMED` | Satu baris = satu peristiwa (`LAB-DEC-238`); waktu pembatalan WIB; `VAL-147`..`149`; gugur, koreksi, dan wadah tidak menjadi baris. **Ditambah:** alasan wajib pada pembatalan pemeriksaan dan wadah, penjaga **backend saja**, sesudah penjaga keadaan (`LAB-DEC-237`, `LAB-DEC-247`) |
| 08 Status | `CONFIRMED` — tidak ada status baru | Laporan hanya membaca. `LAB-DEC-237` menambah **syarat** pada transisi batal yang sudah ada, bukan transisi baru |
| 09 Kewenangan | `CONFIRMED` | Izin tersendiri, lihat dan unduh/cetak dipisah (`LAB-DEC-231`, `LAB-DEC-239`); hak baca daftar Laboratorium tidak membukanya. **Usulan** (`CONFIGURABLE_DEFAULT`): *bawaan* = tidak ada kebijakan tersemai, admin memberikan per jabatan — pola `LAB-PERM-v1` 14.6 bagi `S16a` |
| 10 Dependency antarmodul | `CONFIRMED` | Identitas pasien dan unit layanan **dibaca** dari pasien dan kunjungan lewat jalur yang sudah dipakai rincian order. Nol kontrak lintas modul baru |
| 11 Integrasi | `CONFIRMED` | Nol sistem luar. Pustaka `ClosedXML` (sudah terpasang) dan `react-to-print` (`LAB-DEC-233`) |
| 12 Hasil akhir | `CONFIRMED` | Daftar peristiwa pembatalan per periode, total per jenis yang sama di setiap halaman dan di berkas (`AC-332`) |
| 13 Pembatalan / koreksi | `CONFIRMED` | Laporan tidak mengubah data. Pembatalan order tidak mengubah status pemeriksaannya (F30-18, terbukti) — tidak memengaruhi laporan karena baris dibaca dari peristiwa |
| 14 Audit | `CONFIRMED` + `PROPOSED` | **Dikonfirmasi:** unduhan dan cetak dicatat tanpa data pasien (`LAB-DEC-233`, `LAB-DEC-248`). **Usulan** (`NON_BLOCKING_STANDARD`, **wajib terlihat** bagi pemilik modul): *membuka* laporan **tidak** dicatat, mengikuti konvensi `GET` `S16a` — padahal laporan ini, berbeda dari `S16a`, menampilkan identitas pasien. Arsitek domain menimbang apakah pembukaan perlu jejak akses |
| 15 Notifikasi | Tidak berlaku | Laporan tidak memberi tahu siapa pun (`LAB-DEC-163`) |
| 16 Billing | Tidak berlaku | Kolom *Sudah layak tagih* hanya informasi. Penyerahan pembatalan ke Billing sudah ada di alur batal dan tidak disentuh |
| 17 Keselamatan klinis | Tidak berlaku | Membaca saja. Alasan wajib tidak mengubah perilaku klinis; layar batal pemeriksaan tetap tidak dibuka (`VAL-13`) |
| 18 Pelaporan | `CONFIRMED` | Jejak audit pembatalan untuk mutu dan penelusuran. **Privasi:** laporan memuat identitas pasien — berbeda dari `S16a` yang agregat saja (0F.3 dimensi 18); diterima pemilik modul lewat paket Q11 (`LAB-DEC-240`, pilihan A, bukan B *tanpa nama pasien*) |

### 0H.3 `S16b-2` — Statistik Pesanan Laboratorium: kedelapan belas dimensi

| Dimensi | Status | Isi |
|---|---|---|
| 01 Tujuan | `CONFIRMED` | Beban permintaan per jenis kunjungan, unit layanan, dan disiplin (`LAB-DEC-241`, `LAB-DEC-245`) |
| 02 Aktor | `CONFIRMED` | Pembaca: kepala instalasi dan manajemen secara bawaan; admin dapat memberikannya kepada Petugas atau Dokter Laboratorium (`LAB-DEC-245`) |
| 03 Pemicu / prasyarat | `CONFIRMED` | Pembaca membuka laporan dan memilih periode. Data sudah berdiri (`CAP-P30-07`, `CAP-P30-08`) |
| 04 Alur utama | `CONFIRMED` | Pilih periode dan penyaring → tabel Jenis kunjungan → Unit layanan × disiplin (Pesanan, Pemeriksaan) dengan *Sub Total* dan *Total* → unduh atau cetak (BR-152) |
| 05 Alternatif / exception | `CONFIRMED` + `PROPOSED` | **Dikonfirmasi:** pemeriksaan tanpa disiplin → *Belum terklasifikasi*; kunjungan tanpa unit → *Unit tidak tercatat* (praktis tidak terjadi, `ServiceUnitId` wajib — `CAP-P30-08`); cetak gagal dicatat → tidak dibuka. **Usulan** (`NON_BLOCKING_STANDARD`): periode tanpa pesanan tampil kosong beralasan |
| 06 Data minimum | `CONFIRMED` | Order, pemeriksaan, permintaan yang belum masuk wadah, `RequestedAt`/`CreateDateTime`, disiplin, jenis kunjungan, unit layanan. **Tanpa** identitas pasien (`AC-335`) |
| 07 Aturan / validasi | `CONFIRMED` | Waktu diminta WIB (`LAB-DEC-242`); dua angka pesanan dan pemeriksaan (`LAB-DEC-244`); batal, `Draft`, **dan gugur** tidak dihitung — **sama dengan Beranda** (`LAB-DEC-246`, mengamandemen BR-152 butir 8); `VAL-147`..`149` |
| 08 Status | Tidak berlaku | Laporan hanya membaca |
| 09 Kewenangan | `CONFIRMED` | Izin tersendiri, lihat dan unduh/cetak dipisah (`LAB-DEC-231`, `LAB-DEC-245`). Bawaan: `CONFIGURABLE_DEFAULT` seperti `S16b-1` |
| 10 Dependency antarmodul | `CONFIRMED` — **baru bagi laporan** | **Membaca** `EncounterType` dan unit layanan kunjungan milik **Registrasi** (`BC-REG`) beserta nama unit dari Master Data — jalur baca yang sama dengan daftar pantau (`CAP-P30-08`). Ini yang membedakannya dari `S16a` (`LAB-DA-001` A7.3: *nol baca dari `BC-REG`*). Nol tulis, nol kontrak baru; **ditimbang arsitek domain** |
| 11 Integrasi | `CONFIRMED` | Nol sistem luar; `ClosedXML` dan `react-to-print` |
| 12 Hasil akhir | `CONFIRMED` | Tabel agregat per periode yang angkanya sama dengan Beranda pada data yang sama (`AC-333`, `AC-334`, `AC-336`) |
| 13 Pembatalan / koreksi | `CONFIRMED` | Order batal dan pemeriksaannya tidak dihitung — laporan **wajib** mensyaratkan status order sendiri, sebab pembatalan order tidak mengubah status pemeriksaan (F30-18, `CAP-P30-05`) |
| 14 Audit | `CONFIRMED` + `PROPOSED` | Unduhan dan cetak dicatat (`LAB-DEC-233`, `LAB-DEC-248`). **Usulan** (`NON_BLOCKING_STANDARD`): membuka laporan tidak dicatat — aman karena agregat |
| 15 Notifikasi | Tidak berlaku | — |
| 16 Billing | Tidak berlaku | Tanpa nilai uang |
| 17 Keselamatan klinis | Tidak berlaku | Angka agregat |
| 18 Pelaporan | `CONFIRMED` | Satu definisi hitung bersama Beranda. **Akibat yang disadari** (`LAB-DEC-246` butir 4): pemeriksaan gugur yang tidak diambil ulang tidak tampak di hitungan permintaan; penolakannya tetap terbaca di laporan penolakan `S16a` |

**Catatan verifikasi, bukan penahan:** devYoga hanya memuat order **Rawat Jalan** (23 order); pembuktian
pengelompokan Rawat Inap, IGD, dan Kamar Bersalin butuh data uji baru — milik tahap pengujian, dengan izin
tulis tersendiri.

### 0H.4 Butir bermasalah dan dampaknya

| Butir | Status | Dampak | Alasan klasifikasi |
|---|---|---|---|
| Jejak *membuka* Laporan Pembatalan yang memuat identitas pasien | `PROPOSED` | `NON_BLOCKING_STANDARD` | Menambah atau tidak menambah jejak akses tidak mengubah konsep tersimpan, relasi, maupun lifecycle laporan; usulan ditampilkan eksplisit kepada pemilik modul dan arsitek domain |
| Periode tanpa data tampil kosong beralasan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pola `S16a` |
| Bawaan izin = tidak ada kebijakan tersemai | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Jabatan pemegang izin wajar berbeda antar rumah sakit; diberikan admin (`SysAccessPolicies`) |
| Data uji Rawat Inap/IGD/Kamar Bersalin | `MISSING` (data, bukan requirement) | `NON_BLOCKING_STANDARD` | Menyangkut bukti uji, tidak mengubah desain |

**Nol butir `BLOCKING`. Nol `CONFLICT` terbuka** — pertentangan gugur vs Beranda (`CAP-P30-07`) dan layar batal
(`CAP-P30-04`) sudah ditutup `LAB-DEC-246` dan `LAB-DEC-247`.

### 0H.5 Decision Log

**Nol butir baru.** Seluruh keputusan yang dibutuhkan kedua slice sudah tercatat `approved` di decision log
(`LAB-DEC-230`..`249`). `DEC-LAB-025` tetap ditunda bagi `S16b-3`.

### 0H.6 Kesiapan

| Slice | Sebelumnya (0G.13) | **Revision 13** | Penahan atau syarat |
|---|---|---|---|
| `S16b-1` Laporan Pembatalan | Ditunda Phase 2 | ✅ **`READY_FOR_DOMAIN_DESIGN`** | Nol. Syarat rancangan 0H.8 butir 1–3 |
| `S16b-2` Statistik Pesanan | Ditunda Phase 2 | ✅ **`READY_FOR_DOMAIN_DESIGN`** | Nol. Syarat rancangan 0H.8 butir 1, 4, 5 |
| `S16b-3` sisa katalog | Ditunda Phase 2 | **Tetap ditunda** — tidak dinilai | Kesebelas butir per laporan (`LAB-DEC-181`); titik sentuh lintas modul `CAP-P30-13`..`17`; Q2b, `LAB-COORD-021`, `LAB-OPEN-054`, `LAB-OPEN-055` |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`** — dua slice naik; bagian yang tertahan pada 0G.13 tidak disentuh.

### 0H.7 Apa yang boleh berjalan dan apa yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| `S16b-1` dan `S16b-2` ke `hospital-domain-architect` | Desain laporan `S16b-3` mana pun sampai kesebelas butirnya diserahkan |
| Usulan `LAB-DEC-249` kepada pemilik Master Data (`LAB-COORD-021`) | Rincian laporan per kategori sampai `LAB-COORD-021` disetujui |
| Penyiapan data uji Rawat Inap/IGD/Kamar Bersalin — dengan izin tulis tersendiri | Layar batal per pemeriksaan/wadah (`LAB-DEC-247`) |

### 0H.8 Handoff ke `hospital-domain-architect`

| Field | Nilai |
|---|---|
| Slice | `S16b-1`, `S16b-2` |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, masing-masing **independen** dari `S16b-3` dan dari satu sama lain |
| Snapshot bukti | Decisions **rev 115**; `LAB-EVD-014`; capability map **rev 9**; backend `4d3e4f98`; frontend `7fb1e1d3e` |
| Decision ID yang mengikat | `LAB-DEC-230`..`LAB-DEC-249`; diwarisi `LAB-DEC-159`, `LAB-DEC-160`, `LAB-DEC-166`, `LAB-DEC-205`..`207`, `LAB-DEC-181` |
| Pola yang diwarisi | **Memperluas** `S16a` (`LAB-DA-001` A7) — laporan baca-saja tanpa konsep tersimpan; jangan rancang ulang |
| Syarat rancangan wajib | (1) **Nol konsep laporan tersimpan** — dibaca dari fakta yang ada, pola A7.5. (2) `S16b-1` dibaca dari **peristiwa** riwayat transisi, bukan dari status pemeriksaan (F30-18). (3) `S16b-1` memuat **identitas pasien** — arsitek domain menetapkan batas privasinya, termasuk usulan jejak pembukaan 0H.2 dimensi 14. (4) `S16b-2` adalah laporan **pertama yang membaca `BC-REG`** — arsitek domain menetapkan bentuk ketergantungannya (baca langsung lewat jalur daftar pantau, bukan kontrak tulis). (5) Definisi hitung `S16b-2` **satu sumber dengan Beranda** — pola `INV-57` (satu rumus) |
| Ditambahkan pada lintas laporan | Izin per laporan (`LAB-DEC-231`); `.xlsx` + cetak dengan pencatat cetak server **fail-closed** (`LAB-DEC-233`, `LAB-DEC-248`); alasan wajib pembatalan pemeriksaan/wadah, backend saja (`LAB-DEC-237`, `LAB-DEC-247`) |
| Usulan yang wajib tetap ditandai usulan | Seluruh baris `PROPOSED` pada 0H.2 dan 0H.3 |
| Dependency | Registrasi (baca kunjungan), Master Data (nama unit); `LAB-COORD-021` hanya bagi rincian kategori — **tidak** dipakai kedua slice |
| Yang **tidak boleh** muncul | Tabel ringkasan laporan; kolom uang, penjamin, atau fee; rujukan ke laboratorium luar; asal kiosk atau kamar perawatan (`LAB-OPEN-055`); aksi batal per pemeriksaan di layar |

**Ke `grill-me`:** tidak ada yang dibutuhkan kedua slice. Usulan 0H.2 dimensi 14 dapat ditanyakan bila arsitek
domain menilainya perlu keputusan.

---

## 0I. Penilaian Ulang Revision 14 — penjadwalan ulang pemeriksaan `S20` sesudah putaran 32 dan 33 (2026-11-25)

Diminta pemilik modul sesudah gerbang `/design-business-module` menolak masuk: kemampuan *reschedule* (penjadwalan ulang)
pemeriksaan Laboratorium belum pernah dinilai gerbang ini maupun arsitektur domain. Amendment pass putaran 32 dan closure
pass putaran 33 sudah memutuskannya lengkap; bagian ini menilai apakah keputusan itu **cukup untuk mulai merancang**.

*Penjadwalan ulang* berarti memindahkan **tanggal rencana pemeriksaan** sebuah pesanan Lab ke hari lain tanpa
membatalkannya. Contoh: Gula Darah Puasa direncanakan Senin, pasien datang belum puasa, petugas memindahkannya ke Selasa
dengan alasan *pasien belum puasa*. Nomor order dan kunjungannya tetap.

### 0I.1 Identitas penilaian

| Field | Nilai |
|---|---|
| Slice baru | **`S20`** Penjadwalan ulang pemeriksaan Laboratorium, dipecah dua: **`S20-1`** sisi Laboratorium (tanggal rencana pada pesanan, aksi jadwal ulang, riwayat, Daftar Pasien Lab, kiosk, izin); **`S20-2`** pilihan tanggal rencana pada layar pemesan dokter poli/IGD/Rawat Inap |
| Alasan dipecah dua | `S20-2` berada di layar milik modul lain (`LAB-COORD-023`) dan sengaja menyusul (`LAB-DEC-269`). `S20-1` berdiri sendiri: pemanggil yang tidak mengirim tanggal memperoleh *hari ini* |
| Bukti utama | `00-interview-decisions.md` **revisi 143** — putaran 32 (`LAB-DEC-251`..`269`, BR-157, `AC-338`..`352`) dan closure pass putaran 33 (`LAB-DEC-270`..`277`, `AC-353`..`356`) |
| Bukti implementasi | `01-existing-capability-map.md` **revision 10** (`CAP-P32-01`..`15`) pada BE `4d2c0162` / FE `004524f4c` — sama dengan `HEAD`, **segar**; 2 kueri baca-saja devYoga |
| Wewenang | Seluruh keputusan **disetujui pemilik modul** Yoga Aji Pratama, 2026-11-25, setiap pertanyaan dengan pilihan yang direkomendasikan; batas scope dikonfirmasi eksplisit (`LAB-DEC-251`). Yoga juga pemilik modul `mcu`, sehingga `LAB-DEC-272` sah bagi kedua sisi |
| Baseline rujukan | **Tidak dipakai** — keputusan pemilik modul sudah lengkap; tidak ada gap yang butuh baseline rumah sakit Indonesia |
| Hubungan dengan `S1b` | `S1b` (penyuntingan isi pesanan oleh **dokter**, `LAB-AMD-001`, masih `BUSINESS_DECISION_REQUIRED`) **tidak** dinilai ulang dan **tidak** menahan `S20`: jadwal ulang adalah aksi **petugas Lab** atas **tanggal rencana** saja, bukan penyuntingan pemeriksaan yang dipesan. Batas waktunya kebetulan sama — *sebelum sampel diambil* — tanpa saling bergantung |

### 0I.2 `S20-1` — sisi Laboratorium: kedelapan belas dimensi

| Dimensi | Status | Isi |
|---|---|---|
| 01 Tujuan | `CONFIRMED` | Pemeriksaan yang belum bisa dikerjakan pada hari rencananya dipindah tanpa membatalkan pesanan, dengan jejak siapa, kapan, dari-ke tanggal, dan alasan (`LAB-DEC-252`, BR-157) |
| 02 Aktor | `CONFIRMED` | Pelaku: pemegang `LabOrder : Reschedule` — bawaan Analis Laboratorium (seeder salin dari `LabOrder : Hold`) dan Kepala Instalasi (langkah rilis); jabatan lain lewat admin (`LAB-DEC-254`, `LAB-DEC-274`). Pembaca riwayat: pemegang `LabOrder : Read` (`LAB-DEC-268`). Dokter: satu-satunya yang mencabut CITO pemeriksaan (`LAB-DEC-262`). Petugas MCU: pemilik jadwal pesanan bertaut MCU (`LAB-DEC-272`) |
| 03 Pemicu / prasyarat | `CONFIRMED` | Pemicu: pasien belum siap, pasien meminta pindah, pasien tidak datang, atau kendala Lab. Prasyarat: pesanan punya tanggal rencana (pesanan lama = tanggal diminta WIB, `LAB-DEC-253`) |
| 04 Alur utama | `CONFIRMED` | Daftar Pasien Lab bawaan *Tanggal Rencana = hari ini* + *Terlewat* → aksi *Jadwal Ulang* di baris atau detail → dialog tanggal baru + alasan → backend memeriksa penjaga → tanggal berganti + satu baris riwayat → pesanan pindah ke daftar tanggal baru (BR-157 langkah 1–6; `LAB-DEC-258`, `260`, `267`, `277`) |
| 05 Alternatif / exception | `CONFIRMED` | Sampel sudah diambil → ditolak, arahkan ke batal wadah/ambil ulang (`LAB-DEC-270`); wadah `Rejected` → ditolak sampai ambil ulang diminta (`LAB-DEC-276`); kunjungan selesai → ditolak, pesan ulang (`LAB-DEC-261`); CITO aktif → ditolak, CITO permintaan = batal lalu pesan ulang (`LAB-DEC-262`, `271`); pesanan bertaut MCU → ditolak, ke petugas MCU (`LAB-DEC-272`); pasien datang lebih awal/terlambat → tetap dilayani (`LAB-DEC-259`) |
| 06 Data minimum | `CONFIRMED` | Tanggal rencana pada pesanan — **tanggal kalender WIB saja**, disimpan sebagai tanggal murni (`LAB-DEC-263`). Per jadwal ulang: tanggal lama, tanggal baru, alasan teks bebas, pelaku, waktu (`LAB-DEC-252`, `256`). Penanda kelayakan + alasan per pesanan (`LAB-DEC-273`). Data pasien **tidak** bertambah |
| 07 Aturan / validasi | `CONFIRMED` | BR-157 aturan 1–10 sesudah closure 33: tanggal baru ≥ hari ini WIB dan ≠ tanggal saat ini; alasan wajib pola `VAL-74`; jendela `Requested`/`Confirmed`/`OnHold`-nya + wadah nihil/`Planned` (wadah `Cancelled`/`RecollectionRequired` diabaikan, `OnHold` dinilai dari status sebelum ditahan, `Rejected` menolak); kunjungan bukan `Completed`/`Cancelled`/`NoShow`; nol CITO aktif (permintaan `Ordered` ber-CITO atau pemeriksaan ber-CITO yang tidak gugur/batal); tidak bertaut MCU; tanpa batas jumlah/jarak (`LAB-DEC-264`) |
| 08 Status | `CONFIRMED` — **tidak ada status baru** | Jadwal ulang tidak mengubah status pesanan maupun wadah; `OnHold` tetap `OnHold` dengan `StatusBeforeHold` utuh (`LAB-DEC-257`). *Terlewat* adalah penanda yang dihitung saat dibaca, bukan status (`LAB-DEC-260`) |
| 09 Kewenangan | `CONFIRMED` | Aksi izin baru `LabOrder : Reschedule`, terpisah dari `Update`/`Hold` (`LAB-DEC-254`); pemberian bawaan lewat seeder sekali jalan + langkah rilis (`LAB-DEC-274`). Layar: tanpa izin → tombol tidak tampil; berizin tapi tidak layak → nonaktif beralasan (`LAB-DEC-273`). Kepala Instalasi tanpa `LabMonitoring : Read` memakai detail pesanan — pemberian izin daftar **bukan** bagian slice ini |
| 10 Dependency antarmodul | `CONFIRMED` | **Registrasi:** status kunjungan dibaca (`LAB-DEC-261`, jalur `VAL-67`). **MCU:** tautan komponen (`McuOrderComponent.LabOrderId`) dibaca untuk menolak; MCU juga menolak menjadwal ulang komponen yang sudah dipesan, sehingga tanggal tidak dapat bertentangan — `LAB-COORD-022` ditutup (`LAB-DEC-272`). **Administrator:** registri izin dan seeder (`LAB-DEC-274`). **Kiosk:** membaca tanggal rencana, nol tulis (`LAB-DEC-266`). Nol tulis ke modul lain |
| 11 Integrasi | Tidak berlaku | Nol sistem luar, perangkat, atau layanan eksternal. Pemberitahuan WhatsApp ke pasien di luar scope (`S17`, `LAB-DEC-251`) |
| 12 Hasil akhir | `CONFIRMED` | Pesanan tetap satu dengan tanggal rencana terbaru, riwayat lengkap terbaca, tampil di daftar tanggal barunya; tagihan, wadah, status, dan Laporan Pembatalan tidak berubah (BR-157 *Hasil akhir*; `AC-338`..`356`) |
| 13 Pembatalan / koreksi | `CONFIRMED` + `PROPOSED` | **Dikonfirmasi:** jadwal ulang yang keliru dikoreksi dengan jadwal ulang berikutnya (tanpa batas jumlah, riwayat bertambah, `LAB-DEC-264`); jadwal ulang bukan pembatalan (`LAB-DEC-268`). **Usulan** (`NON_BLOCKING_STANDARD`): tidak ada aksi *batalkan jadwal ulang* terpisah — mengembalikan tanggal = jadwal ulang ke tanggal semula, beralasan |
| 14 Audit | `CONFIRMED` | Setiap jadwal ulang tercatat: tanggal lama/baru, alasan, pelaku, waktu (`LAB-DEC-252`); terbaca pemegang `LabOrder : Read` (`LAB-DEC-268`). Pencabutan CITO oleh dokter sudah tercatat lewat mekanisme yang ada |
| 15 Notifikasi | Tidak berlaku | Sistem tidak memberi tahu pasien maupun dokter; pasien dihubungi petugas di luar sistem (contoh `LAB-DEC-260`). Pemberitahuan otomatis = `S17`, di luar scope |
| 16 Billing | `CONFIRMED` — tidak ada dampak | Jadwal ulang tidak membuat, mengubah, atau membalik tagihan; pesanan tetap pada kunjungan asal (`LAB-DEC-252`), dan kunjungan yang sudah tutup ditolak agar tagihan tidak menyusul ke sana (`LAB-DEC-261`). Kelayakan tagih (`S10`) tetap lahir dari wadah, tidak tersentuh. Jalan keluar *batal lalu pesan ulang* memakai alur batal yang sudah ada |
| 17 Keselamatan klinis | `CONFIRMED` | (1) Pemeriksaan mendesak tidak dapat ditunda tanpa keputusan dokter — CITO aktif, termasuk pada permintaan, menolak (`LAB-DEC-262`, `271`). (2) Sampel yang sudah diambil tidak menggantung ke hari lain (`LAB-DEC-270`). (3) Tanggal rencana **penanda, bukan penghalang**: kesiapan klinis pasien (mis. puasa) tetap dinilai petugas, seperti sekarang (`LAB-DEC-259`) |
| 18 Pelaporan | `CONFIRMED` | Tanpa laporan jadwal ulang pada slice ini (`LAB-DEC-268`); penanda *Terlewat* di daftar kerja (`LAB-DEC-260`); jadwal ulang **tidak** masuk Laporan Pembatalan (`CAP-P32-15`). Usulan laporan kelak lewat katalog `S16b` |

### 0I.3 `S20-2` — pilihan tanggal pada layar pemesan dokter

| Dimensi | Status | Isi |
|---|---|---|
| 01–07, 12 | `CONFIRMED` sisi Lab | Bawaan hari ini; pemesan boleh memilih tanggal ke depan, tidak boleh mundur (`LAB-DEC-253`). Backend Lab menerima ruas **opsional** — dirancang bersama `S20-1` |
| 10 Dependency | `MISSING` di luar modul | Layar antrean dokter poli, IGD, dan Rawat Inap milik modul lain (F32-12, `CAP-P32-14`); kesediaan dan jadwal pemiliknya belum ada (`LAB-COORD-023`) |
| Lainnya | Mengikuti `S20-1` | — |

**Kesiapan `S20-2`: ditunda sebagai `LATER SLICE`** — bukan karena keputusan bisnis Lab yang terbuka, melainkan karena
layar pemiliknya. Kontrak backend yang dibutuhkannya (ruas opsional) ikut dirancang di `S20-1`, sehingga `S20-2` kelak
hanya pekerjaan layar modul lain.

### 0I.4 Butir bermasalah dan dampaknya

| Butir | Status | Dampak | Alasan klasifikasi |
|---|---|---|---|
| Dua petugas menjadwalkan ulang pesanan yang sama bersamaan: yang kedua ditolak lewat token konkurensi pesanan yang ada; layar basi tidak terdeteksi karena permintaan tidak membawa versi (`CAP-P32-06`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Perilaku standar pola Lab; tidak mengubah konsep tersimpan. Arsitek domain boleh menimbang apakah aksi ini membawa versi |
| Tidak ada aksi *batalkan jadwal ulang* terpisah (0I.2 dimensi 13) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Koreksi = jadwal ulang berikutnya; riwayat utuh |
| Layar pembuatan pesanan milik Lab sendiri (Penerimaan Sampling, pasien datang langsung, rujukan luar) tidak menampilkan pilihan tanggal; pesanan lahir *hari ini* | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pasien layar itu hadir hari itu; perpindahan tetap lewat jadwal ulang. `LAB-DEC-253` membolehkan pilihan, tidak mewajibkannya di setiap layar |
| `LAB-COORD-023` — layar pemesan dokter | `MISSING` (di luar modul) | `NON_BLOCKING_STANDARD` bagi `S20-1` | Hanya menahan `S20-2`; `S20-1` berdiri dengan bawaan hari ini (`LAB-DEC-269`) |
| `LAB-OPEN-056` — `POST /lab-orders` tanpa `VAL-67` | `MISSING` (keputusan perbaikan) | Di luar `S20` | Temuan impact scan di luar BR-157; `S20-1` memakai penjaga kunjungannya sendiri (`LAB-DEC-261`) |
| Data uji: devYoga tidak punya pesanan `Confirmed` berwadah `Planned` bertanggal lampau, pesanan bertaut MCU di luar paket, maupun pesanan CITO pra-wadah | `MISSING` (data, bukan requirement) | `NON_BLOCKING_STANDARD` | Menyangkut bukti uji; butuh izin tulis tersendiri pada tahap pengujian |

**Nol butir `BLOCKING`. Nol `CONFLICT` terbuka** — ketiga pertentangan capability map revision 10 (`CAP-P32-04`,
`CAP-P32-08`, `CAP-P32-09`) dan rujukan `AvailableActions` (`CAP-P32-11`) sudah ditutup `LAB-DEC-270`..`273`.

### 0I.5 Decision Log

**Nol butir baru.** Seluruh keputusan yang dibutuhkan `S20-1` sudah `approved` (`LAB-DEC-251`..`277`; `LAB-DEC-255`
superseded oleh `LAB-DEC-270`). `S20-2` menunggu `LAB-COORD-023`, koordinasi lintas modul — bukan keputusan bisnis Lab.

### 0I.6 Kesiapan

| Slice | Sebelumnya | **Revision 14** | Penahan atau syarat |
|---|---|---|---|
| `S20-1` sisi Laboratorium | Belum pernah dinilai | ✅ **`READY_FOR_DOMAIN_DESIGN`** | Nol. Syarat rancangan 0I.8 |
| `S20-2` layar pemesan dokter | Belum pernah dinilai | **Ditunda — `LATER SLICE`** | `LAB-COORD-023` (pemilik layar poli/IGD/Rawat Inap) |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`** — satu slice naik; slice lain tidak disentuh.

### 0I.7 Apa yang boleh berjalan dan apa yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| `S20-1` ke `hospital-domain-architect`, lalu `/design-business-module` | Pilihan tanggal di layar dokter poli/IGD/Rawat Inap sampai `LAB-COORD-023` disepakati |
| Ruas tanggal rencana **opsional** pada kedua jalur pembuatan pesanan (dirancang di `S20-1`) | Pencabutan CITO pada **permintaan** — sengaja tidak dibuat (`LAB-DEC-271`) |
| Keputusan `LAB-OPEN-056` oleh pemilik modul, terpisah dari `S20` | Status pesanan baru, laporan jadwal ulang, pembatalan otomatis pesanan terlewat, pemberitahuan pasien |

### 0I.8 Handoff ke `hospital-domain-architect`

| Field | Nilai |
|---|---|
| Slice | `S20-1` |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, **independen** dari `S20-2` dan dari `S1b` |
| Snapshot bukti | Decisions **rev 143**; capability map **rev 10**; backend `4d2c0162`; frontend `004524f4c` |
| Decision ID yang mengikat | `LAB-DEC-251`..`LAB-DEC-277` (tanpa `LAB-DEC-255`, superseded); diwarisi `LAB-DEC-213` (kiosk nol tulis), `LAB-DEC-226`/`227` (CITO pemeriksaan), `LAB-DEC-237` (`VAL-74`), `VAL-67` |
| Syarat rancangan wajib | (1) Tanggal rencana dan riwayatnya **milik pesanan Laboratorium** — tidak dipindah ke Registrasi, MCU, atau kunjungan. (2) **Nol status baru** pada `LAB-STATE-v1`; *Terlewat* dihitung saat dibaca, **nol job terjadwal** (A7.5). (3) Kelayakan jadwal ulang **satu rumus** dipakai penjaga aksi **dan** penanda per pesanan (`LAB-DEC-273`) — pola satu sumber seperti `INV-57`. (4) Penjaga lintas modul hanya **membaca**: status kunjungan (`BC-REG`) dan tautan komponen MCU; nol kontrak tulis baru. (5) Tanggal disimpan sebagai **tanggal murni WIB**, isi awal pesanan lama dari `RequestedAt` WIB (`LAB-DEC-253`, `263`) |
| Usulan yang wajib tetap ditandai usulan | Seluruh baris `PROPOSED` pada 0I.4 |
| Dependency | Registrasi (status kunjungan, baca), MCU (tautan komponen, baca), Administrator (registri izin, seeder), Kiosk (baca) |
| Yang **tidak boleh** muncul | Status *Dijadwalkan Ulang*; job/pembatalan otomatis; batas jumlah atau jarak; penghalang pengerjaan menurut tanggal; endpoint pencabutan CITO permintaan; tulis ke data MCU; laporan jadwal ulang; jam atau sesi pada tanggal rencana |

**Ke `grill-me`:** tidak ada yang dibutuhkan `S20-1`. Usulan 0I.4 dapat ditanyakan bila arsitek domain menilainya perlu
keputusan.

---

## 0J. Penilaian Ulang Revision 15 — penyerahan hasil ke pasien `S17-1` sesudah putaran 34 dan 35 (2026-11-27)

Diminta pemilik modul sesudah gerbang `/design-business-module` menolak masuk untuk `BE-LAB-41`: `S17-1` terakhir dinilai
pada revision 12 (0G.10, 2026-10-06), **sebelum** amendment pass putaran 34 dan closure pass putaran 35 mengubah cara
pengirimannya. Bagian ini menilai apakah keputusan sekarang **cukup untuk mulai merancang**.

*Penyerahan hasil ke pasien* berarti dokumen hasil final satu nomor order sampai ke tangan pasien: dikirim lewat WhatsApp,
dicetak di loket, atau diunduh petugas. Contoh: order `LAB-RSMMC-000040` (Darah Lengkap + Gula Darah Puasa) seluruhnya
dirilis pukul 10.05; Gilang (Analis) mengirim PDF-nya ke WhatsApp pasien lalu mencatat *Terkirim*, dan kolom *Terkirim ke
Pasien* menjadi `1`.

### 0J.1 Identitas penilaian

| Field | Nilai |
|---|---|
| Slice | **`S17-1`** dokumen hasil final, gerbang, kirim, cetak, dan unduh. **`S17-2`** (halaman verifikasi QR publik dan tautan aman) **tidak** dinilai ulang — tetap tertahan `LAB-COORD-015` |
| Pemecahan baru | `S17-1` dipecah dua karena satu pertentangan (0J.4) hanya menyentuh **syarat persetujuan** pada gerbang: **`S17-1a`** dokumen hasil berversi, arsip, *Preview*, salinan WhatsApp, kirim manual dan lognya, nomor tujuan, izin kirim; **`S17-1b`** syarat persetujuan sebelum gerbang terbuka |
| Bukti utama | `00-interview-decisions.md` **revisi 145** — putaran 34 (`LAB-DEC-278`..`288`, `AC-357`..`365`) dan closure putaran 35 (`LAB-DEC-289`..`292`, `AC-366`..`369`); keputusan terdahulu yang masih berlaku: `LAB-DEC-066`..`068`, `LAB-DEC-139`, `LAB-DEC-140`, `LAB-DEC-172`, `LAB-DEC-180`, `LAB-DEC-182`..`185` |
| Bukti implementasi | `01-existing-capability-map.md` **revision 11** (`CAP-P34-01`..`18`) pada BE `cce1dbf5` / FE `11b3aaece`. `HEAD` kini BE `693941d5` / FE `48680f6de`; pergeserannya diperiksa terbatas pada putaran 35 — nol perubahan pada butir `S17` |
| Wewenang | Putaran 34–35 disetujui **pemilik modul** Yoga Aji Pratama, 2026-11-27. Syarat persetujuan klinis **bukan** wewenang pemilik modul sendirian (`LAB-DEC-121`) — pemegangnya Kepala Instalasi dr. Bima Prasetya, Sp.PK (`LAB-SIGN-002`, `LAB-OPEN-057`) |
| Baseline rujukan | **Tidak dipakai** — keputusan yang ada cukup; satu-satunya gap pemblokir adalah pertentangan antar-keputusan, bukan kekosongan kebijakan |

### 0J.2 `S17-1a` — dokumen, kirim manual, cetak, unduh: kedelapan belas dimensi

| Dimensi | Status | Isi |
|---|---|---|
| 01 Tujuan | `CONFIRMED` | Pasien menerima dokumen hasil final yang tidak dapat diedit, dan rumah sakit tahu siapa mengirim apa, kapan, ke nomor mana, versi mana — serta mana yang gagal (`LAB-DEC-066`, `067`, `283`) |
| 02 Aktor | `CONFIRMED` | Pengirim: pemegang izin kirim tersendiri — bawaan Analis Laboratorium; admin lain lewat Role Access (`LAB-DEC-286`, menjabarkan `LAB-DEC-068`). Pembaca riwayat: pemegang izin baca hasil. Petugas *Preview*: petugas Lab berizin baca hasil (`LAB-DEC-287`). Pasien: penerima, tanpa akses sistem |
| 03 Pemicu / prasyarat | `CONFIRMED` | Dokumen terbit ketika **seluruh** pemeriksaan order PK/Mikrobiologi dirilis — order *Selesai*; Mikrobiologi `Sementara` belum *Selesai* (`LAB-DEC-139`, `172`, `284`). Kirim/cetak/unduh hanya lewat gerbang terbuka (`LAB-DEC-139`, `287`) |
| 04 Alur utama | `CONFIRMED` | Rilis lengkap → dokumen versi 1 terbit dan diarsip → (gerbang terbuka) petugas memilih *Kirim Hasil ke Pasien* → sistem membuka WhatsApp ke nomor data induk ternormalkan dan menyediakan salinan PDF bersandi → petugas mengirim dari WhatsApp resmi RS → memilih *Terkirim* → satu baris log, angka naik (`LAB-DEC-279`, `282`..`284`, `290`, `292`; proses bisnis putaran 34 langkah 1–5) |
| 05 Alternatif / exception | `CONFIRMED` | Nomor kosong atau gagal normalisasi → aksi nonaktif beralasan, tanpa isian nomor lain (`LAB-DEC-282`, `292`); nomor tak terdaftar/pasien menolak/lainnya → *Gagal* beralasan, angka tetap, kirim ulang tersedia (`LAB-DEC-283`); kirim ulang tanpa batas (`LAB-DEC-281`); order belum lengkap dirilis atau PA → nonaktif beralasan (`LAB-DEC-139`, `288`); tanpa izin → aksi tidak tampil, `403` (`LAB-DEC-286`) |
| 06 Data minimum | `CONFIRMED` | **Dokumen per versi:** nomor dokumen, nomor versi, status *berlaku*/*sudah digantikan*, waktu terbit, perilis, token QR acak per versi, berkas arsip (`LAB-DEC-140`, `183` butir 1, `284`, `289`, `291`). **Per pengiriman:** pelaku, waktu, nomor tujuan snapshot ternormalkan, saluran *WhatsApp manual*, versi dokumen, hasil *Terkirim*/*Gagal*, alasan gagal + keterangan *Lainnya* (`LAB-DEC-282`, `283`). Angka *Terkirim ke Pasien* **turunan** dari log, bukan disimpan (`LAB-DEC-283`). Tanggal lahir pasien **dibaca** untuk sandi, tidak disalin (`LAB-DEC-290`) |
| 07 Aturan / validasi | `CONFIRMED` | Normalisasi nomor `628…` 10–15 angka (`LAB-DEC-292`); alasan gagal wajib, *Lainnya* wajib keterangan (`LAB-DEC-283`); hanya disiplin PK dan Mikrobiologi (`LAB-DEC-288`); salinan WA bersandi `HHBBTTTT`, arsip dan cetak tanpa sandi, semua dikunci dari pengeditan (`LAB-DEC-284`, `290`); *Preview* bertanda air *BELUM FINAL — bukan untuk pasien* dan tidak dapat dikirim (`LAB-DEC-287`). Gerbang PA yang dulu `MISSING` (0G.10) **tertutup** oleh `LAB-DEC-288` |
| 08 Status | `CONFIRMED` + `CONFLICT` | **Dikonfirmasi:** versi dokumen *berlaku* → *sudah digantikan* (`LAB-DEC-140`, `289`); hasil pengiriman manual hanya *Terkirim* atau *Gagal* — `Pending`/`Delivered` hanya untuk gerbang otomatis kelak (`LAB-DEC-283`). **Bertentangan:** `LAB-DEC-180` (koreksi → antrean `Pending` otomatis) dengan `LAB-DEC-285`/`289` (penanda *Perlu kirim ulang* turunan) — `DEC-LAB-036`, **tidak memblokir `S17-1a`** karena versi 2 belum dapat terbit (0J.4) |
| 09 Kewenangan | `CONFIRMED` | Izin kirim/catat **tersendiri**, tidak menumpang izin rilis atau cetak (`LAB-DEC-286`); gerbang ditegakkan backend pada ketiga jalur (`LAB-DEC-139`, `287`); kiosk dan pasien tidak membaca apa pun dari slice ini |
| 10 Dependency antarmodul | `CONFIRMED` + `PROPOSED` | **Manajemen Pasien:** nomor WhatsApp dan tanggal lahir **dibaca**, nol tulis (`LAB-DEC-282`, `290`, `292`). **Platform:** pembangkit PDF `QuestPDF` tersedia (`CAP-P34-01`); registri izin. **Usulan:** arsip memakai penyimpanan privat per area yang sudah ada (`FileStorage:PrivateRootPath`, pola Radiologi — `CAP-P34-11`, `LAB-OPEN-058`), bukan layanan bersama `LAB-COORD-019` yang belum ada — 0J.4 |
| 11 Integrasi | `CONFIRMED` | **Nol integrasi otomatis:** WhatsApp dibuka di perangkat petugas (`wa.me`), berkas dilampirkan manual; *Terkirim* = pernyataan petugas, bukan bukti gerbang (`LAB-DEC-279`). Gerbang WhatsApp platform tetap `LAB-COORD-011`. Halaman QR publik = `S17-2` (`LAB-DEC-291`) |
| 12 Hasil akhir | `CONFIRMED` | Setiap order PK/Mikrobiologi yang dirilis lengkap punya tepat satu dokumen versi 1 di arsip; setiap upaya kirim tercatat; kolom *Terkirim ke Pasien* berisi angka log, bukan lagi *Belum Terkirim* tetap (`AC-357`..`369`) |
| 13 Pembatalan / koreksi | `CONFIRMED` + `PROPOSED` | **Dikonfirmasi:** log tidak pernah disunting atau dihapus; pengiriman yang keliru dicatat ulang sebagai baris baru (`LAB-DEC-281`, `283`); koreksi hasil → versi baru, tetapi pemicunya `S6` (`LAB-DEC-289`). **Usulan** (`NON_BLOCKING_STANDARD`): tidak ada aksi *batalkan catatan kirim* — petugas yang salah memilih *Terkirim* mencatat baris *Gagal* berketerangan |
| 14 Audit | `CONFIRMED` | Log pengiriman adalah jejak audit; terbit dan unduh dokumen teraudit (`LAB-DEC-182` *seluruh pengiriman teraudit*, `LAB-DEC-185` audit akses) |
| 15 Notifikasi | `CONFIRMED` — tidak ada otomatis | Pesan dikirim manusia; teks pesan disiapkan sistem berisi petunjuk sandi tanpa tanggal lahir (`LAB-DEC-290`) |
| 16 Billing | `CONFIRMED` — tidak ada dampak | Kirim, cetak, dan unduh tidak membuat atau mengubah tagihan; gerbang tidak memeriksa pelunasan (tidak ada keputusan yang memintanya) |
| 17 Keselamatan klinis | `CONFIRMED` | (1) Pasien hanya menerima hasil yang **dirilis empat mata** (`LAB-DEC-280` bagian *Dokter Lab*, `LAB-DEC-150`, `152`). (2) *Preview* tidak pernah sampai ke pasien (`LAB-DEC-287`). (3) Salinan yang salah tujuan tidak terbaca tanpa tanggal lahir (`LAB-DEC-290`). (4) Versi lama terbaca *sudah digantikan* (`LAB-DEC-140`) |
| 18 Pelaporan | `CONFIRMED` | Kolom *Terkirim ke Pasien* dan penanda *Perlu kirim ulang* di Hasil dan Riwayat (`LAB-DEC-283`, `285`); laporan pengiriman tersendiri tidak diminta (katalog `S16b`) |

### 0J.3 `S17-1b` — syarat persetujuan sebelum gerbang terbuka

| Dimensi | Status | Isi |
|---|---|---|
| 02 Aktor, 09 Kewenangan | **`CONFLICT`** — **`BLOCKING`** | Dua keputusan `approved` menyatakan syarat yang berbeda — `DEC-LAB-035` (0J.4) |
| 03 Prasyarat gerbang | `CONFIRMED` sebagian | Order *Selesai* (`LAB-DEC-139`, `172`) **dan** syarat persetujuan — isinya bertentangan |
| 07 Aturan — kategori sensitif | `MISSING` — bergantung `DEC-LAB-035` | Bila `LAB-DEC-182` yang berlaku, isinya `LAB-OPEN-048` (Kepala Instalasi) |
| 13, 14 | Mengikuti `S17-1a` | Persetujuan, bila ada, teraudit |
| Lainnya | Mengikuti `S17-1a` | — |

**Yang aman hari ini di kedua tafsiran:** gerbang **tertutup**. `LAB-DEC-280` menutupnya sampai Profesor dijawab;
`LAB-DEC-182` baru berlaku sesudah konfirmasi Kepala Instalasi (`LAB-SIGN-002`), yang belum ada. Jadi tidak ada pasien
yang menerima apa pun sebelum pertentangan ini diputuskan — yang tertahan adalah **bentuk** syaratnya.

### 0J.4 Butir bermasalah dan dampaknya

| Butir | Status | Dampak | Alasan klasifikasi |
|---|---|---|---|
| **`DEC-LAB-035` — syarat persetujuan kirim.** `LAB-DEC-182` (2026-10-06): persetujuan **berbasis peran** `LabResultPatientDeliveryApprover`, **hasil normal tanpa persetujuan tambahan**, persetujuan kedua hanya bagi kategori sensitif; *mengamandemen* `LAB-DEC-067`/`139` dan *menutup* `LAB-OPEN-029`, menunggu konfirmasi Kepala Instalasi (`LAB-SIGN-002`). `LAB-DEC-280` (2026-11-27): syarat **Profesor tetap** untuk semua hasil, `LAB-OPEN-029` dianggap terbuka, diajukan ke dr. Bima lewat `LAB-OPEN-057`. `LAB-DEC-280` tidak menyebut `LAB-DEC-182` dan tidak menyatakannya *superseded* | `CONFLICT` | **`BLOCKING` bagi `S17-1b`** | Mengubah **authorization** (ada/tidaknya peran penyetuju dan izinnya), **struktur tersimpan** (ada/tidaknya catatan persetujuan per order atau per kategori), dan **hasil bisnis** (hasil normal dapat dikirim sesudah konfirmasi, atau tidak sama sekali sampai Profesor ditetapkan). Pertanyaan ini terlewat saat putaran 34 — persis seperti `LAB-DEC-183` yang ditemukan di tengah putaran itu |
| **`DEC-LAB-036` — antrean kirim versi koreksi.** `LAB-DEC-180` (2026-10-06): versi koreksi atas hasil yang pernah dikirim **masuk antrean `Pending` otomatis**. `LAB-DEC-285`/`289` (putaran 34–35): penanda *Perlu kirim ulang* **turunan**, tanpa baris antrean; `LAB-DEC-283` membatasi `Pending` pada gerbang otomatis. Tidak satu pun menyatakan `LAB-DEC-180` diamandemen | `CONFLICT` | `NON_BLOCKING_STANDARD` bagi `S17-1a`; **`BLOCKING` bagi sambungan `S6-1`** | Versi 2 belum dapat terbit (`LAB-DEC-289`), sehingga tidak ada baris yang berbeda hari ini; status log manual sudah dikunci *Terkirim*/*Gagal*. Penambahan `Pending` kelak bersifat aditif. Wajib diputuskan sebelum `S6-1` menyambung ke `S17` |
| Arsip pada penyimpanan privat per area (`FileStorage:PrivateRootPath`) sebagai ganti layanan bersama `LAB-COORD-019` yang belum ada | `PROPOSED` | `NON_BLOCKING_STANDARD` bagi desain | `LAB-DEC-185` menetapkan **sifatnya** (privat, terautentikasi, teraudit), bukan penyedianya; pola Radiologi memenuhinya. `LAB-COORD-019` menahan **implementasi** sampai pemilik platform atau pemilik modul menerima pola per area — dicatat untuk roadmap |
| Pasien tanpa tanggal lahir → aksi kirim nonaktif beralasan (turunan fail-closed `LAB-DEC-290`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan keselamatan yang belum ditanyakan eksplisit; tidak mengubah konsep tersimpan |
| Tidak ada aksi *batalkan catatan kirim* (0J.2 dimensi 13) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Log tidak disunting; koreksi = baris baru |
| `UNK-P34-01` — kelayakan lisensi `QuestPDF` *Community* | `MISSING` (platform) | `NON_BLOCKING_STANDARD` | Sudah dipakai 16 layanan modul lain; risiko produksi bersama, milik pemilik platform |
| `LAB-OPEN-057` — siapa Profesor dan bentuk persetujuannya | `MISSING` | Bergantung `DEC-LAB-035` | Bila `LAB-DEC-182` yang berlaku, pertanyaan nota berubah menjadi konfirmasi peran penyetuju + isi `LAB-OPEN-048` |
| `LAB-OPEN-058` — kapasitas dan masa simpan arsip | `MISSING` | `NON_BLOCKING_STANDARD` (`LATER SLICE`) | Desain memakai penyimpanan yang ada; masa simpan menyusul |

### 0J.5 Decision Log

| ID | Pertanyaan yang belum terjawab | Bukti saat ini | Usulan | Dampak | Pemilik | Status |
|---|---|---|---|---|---|---|
| `DEC-LAB-035` | Syarat persetujuan sebelum hasil diserahkan ke pasien: **(a)** `LAB-DEC-182` berlaku — peran penyetuju, hasil normal tanpa persetujuan tambahan, kategori sensitif disetel; `LAB-DEC-280` diamandemen dan nota `LAB-OPEN-057` menjadi permintaan konfirmasi `LAB-SIGN-002` + isi `LAB-OPEN-048`; **(b)** `LAB-DEC-280` berlaku — syarat Profesor bagi semua hasil, `LAB-DEC-182` *superseded*; **(c)** lainnya | `LAB-DEC-182`, `LAB-DEC-280`, `LAB-DEC-121`, `LAB-SIGN-002`, `LAB-OPEN-029`, `LAB-OPEN-048`, `LAB-OPEN-057` | **(a)** — keputusan yang lebih rinci dan sudah punya jalur ratifikasi; gerbang tetap tertutup sampai Kepala Instalasi mengonfirmasi. Usulan, bukan keputusan | `BLOCKING` bagi `S17-1b` | Pemilik modul memilih tafsiran; isi klinis dikonfirmasi Kepala Instalasi dr. Bima (`LAB-DEC-121`) | `OPEN` |
| `DEC-LAB-036` | Versi koreksi atas hasil yang pernah dikirim: **(a)** penanda *Perlu kirim ulang* turunan (`LAB-DEC-285`/`289`) menggantikan antrean `Pending` `LAB-DEC-180` selama jalur manual; antrean `Pending` hidup bersama gerbang otomatis; **(b)** `LAB-DEC-180` utuh — koreksi menyisipkan baris log `Pending`; **(c)** lainnya | `LAB-DEC-180`, `LAB-DEC-283`, `LAB-DEC-285`, `LAB-DEC-289` | **(a)** — selaras `LAB-DEC-283`; nol baris tanpa pelaku. Usulan, bukan keputusan | `BLOCKING` bagi sambungan `S6-1`; tidak bagi `S17-1a` | Pemilik modul | `OPEN` |

### 0J.6 Kesiapan

| Slice | Sebelumnya (rev 12) | **Revision 15** | Penahan atau syarat |
|---|---|---|---|
| `S17-1a` dokumen berversi, arsip, *Preview*, salinan WA, kirim manual + log, nomor, izin kirim | `S17-1` `READY_FOR_DOMAIN_DESIGN` — dengan model antrean dan tautan aman | ✅ **`READY_FOR_DOMAIN_DESIGN`** — dinilai ulang atas putaran 34–35 | Syarat rancangan 0J.8; **independen** dari `S17-1b` selama gerbang dimodelkan tertutup |
| `S17-1b` syarat persetujuan pada gerbang | (bagian `S17-1`) | **`BUSINESS_DECISION_REQUIRED`** | `DEC-LAB-035` |
| `S17-2` verifikasi QR publik, tautan aman | Tertahan | **Tetap tertahan** | `LAB-COORD-015` (`LAB-DEC-291`) |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`.**

### 0J.7 Apa yang boleh berjalan dan apa yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| `S17-1a` ke `hospital-domain-architect` | Rancangan peran penyetuju, catatan persetujuan, dan kategori sensitif sampai `DEC-LAB-035` diputuskan |
| `/grill-me` singkat untuk `DEC-LAB-035` dan `DEC-LAB-036` — dapat mendahului atau berjalan sejajar dengan arsitektur domain | Pengiriman nota `LAB-OPEN-057` kepada dr. Bima **dalam bunyi sekarang** — isinya bergantung `DEC-LAB-035` |
| Gerbang dimodelkan **tertutup** dengan alasan yang dapat dibaca layar | Halaman verifikasi publik, tautan aman, gerbang WhatsApp otomatis |

### 0J.8 Handoff ke `hospital-domain-architect`

| Field | Nilai |
|---|---|
| Slice | `S17-1a` |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN`, **independen** dari `S17-1b` dan `S17-2` |
| Snapshot bukti | Decisions **rev 145**; capability map **rev 11**; backend `693941d5`; frontend `48680f6de` |
| Decision ID yang mengikat | `LAB-DEC-278`..`292`; diwarisi `LAB-DEC-066`..`068`, `LAB-DEC-139` (tanpa syarat persetujuannya), `LAB-DEC-140`, `LAB-DEC-150`, `152`, `172`, `LAB-DEC-183` (sebagaimana diamandemen `279`/`284`), `LAB-DEC-184`, `LAB-DEC-185` |
| Syarat rancangan wajib | (1) Dokumen hasil dan log pengiriman **milik Laboratorium**; nomor WhatsApp dan tanggal lahir hanya **dibaca** dari data induk pasien. (2) Dokumen **berversi sejak awal**, hari ini hanya versi 1 yang dapat terbit (`LAB-DEC-289`). (3) Angka *Terkirim ke Pasien* **turunan** log, bukan kolom (`LAB-DEC-283`, bagian 13.1 arsitektur backend). (4) Log tidak pernah disunting/dihapus. (5) **Satu rumus gerbang** dipakai aksi dan penanda layar, ditegakkan backend pada kirim, cetak, dan unduh (`LAB-DEC-139`, `287`); syarat persetujuan dimodelkan sebagai **syarat tertutup beralasan** tanpa entity atau peran penyetuju. (6) Token QR acak per versi, tak dapat diturunkan dari nomor; nol endpoint tanpa login (`LAB-DEC-291`). (7) Salinan WA bersandi dibuat dari arsip saat diminta, tidak diarsip terpisah (`LAB-DEC-290`) |
| Usulan yang wajib tetap ditandai usulan | Seluruh baris `PROPOSED` pada 0J.4 |
| Dependency | Manajemen Pasien (baca), Platform (PDF, registri izin, penyimpanan privat per area — `LAB-COORD-019` untuk implementasi), `S6` (pemicu versi 2, kelak) |
| Yang **tidak boleh** muncul | Status `Pending`/`Delivered` pada jalur manual; kolom counter tersimpan; isian nomor tujuan lain; batas jumlah kirim; endpoint publik; peran/izin penyetuju atau catatan persetujuan; jalur kirim Patologi Anatomi; tulis ke data induk pasien |

**Ke `grill-me`:** `DEC-LAB-035` (memblokir `S17-1b`) dan `DEC-LAB-036` (memblokir sambungan `S6-1`). **Keduanya ditutup closure pass putaran 36 — lihat 0J.9.**

### 0J.9 Penilaian ulang revision 16 — sesudah closure pass putaran 36 (2026-11-27)

Closure pass putaran 36 menutup kedua pertentangan 0J.4 dengan pilihan yang direkomendasikan (decision log **revisi
147**): `DEC-LAB-035` → `LAB-DEC-293` (`LAB-DEC-182` berlaku, `LAB-DEC-280` diamandemen pada syarat Profesor);
`DEC-LAB-036` → `LAB-DEC-298` (penanda turunan menggantikan antrean `Pending` selama jalur manual). Syarat persetujuan
dijabarkan `LAB-DEC-294`..`297`.

| Dimensi `S17-1b` | Sebelumnya (0J.3) | **Revision 16** | Isi |
|---|---|---|---|
| 02 Aktor, 09 Kewenangan | `CONFLICT` — `BLOCKING` | `CONFIRMED` | Penyetuju kedua = pemegang peran `LabResultPatientDeliveryApprover` yang ditunjuk Kepala Instalasi, **bukan perilis** pemeriksaan mana pun pada order (`LAB-DEC-297`); pengubah setelan aktivasi = izin tersendiri, bawaan Kepala Instalasi (`LAB-DEC-296`) |
| 03 Prasyarat gerbang | `CONFIRMED` sebagian | `CONFIRMED` | Tiga syarat per versi: order *Selesai*; disiplin **diaktifkan** (`LAB-DEC-295`, bawaan nonaktif); bila memuat pemeriksaan sensitif, versi itu **disetujui** (`LAB-DEC-294`, `297`) |
| 07 Kategori sensitif | `MISSING` | `CONFIRMED` bentuknya; isi `MISSING` — `CONFIGURABLE_DEFAULT` | Setelan daftar pemeriksaan sensitif dirancang sekarang (`LAB-DEC-294`); isinya `LAB-OPEN-048` diisi Kepala Instalasi sebelum aktivasi. Gerbang tertutup sampai aktivasi, sehingga isi kosong tidak membuka apa pun |
| 08 Status | — | `CONFIRMED` | Aktivasi: nonaktif ↔ aktif, dapat dibalik, tercatat (`LAB-DEC-295`). Persetujuan per versi: belum diputuskan → disetujui / ditolak beralasan (`LAB-DEC-297`) |
| 13 Koreksi | — | `CONFIRMED` + `PROPOSED` | Versi baru butuh persetujuan baru (`LAB-DEC-297`). **Usulan** (turunan tercatat pada `LAB-DEC-297`, `NON_BLOCKING_STANDARD`): penolakan tidak final — dapat disusul persetujuan |
| 14 Audit | — | `CONFIRMED` | Aktivasi, persetujuan, dan penolakan tercatat siapa, kapan, alasan (`LAB-DEC-295`, `297`) |
| 17 Keselamatan klinis | — | `CONFIRMED` | Hasil sensitif tidak sampai ke pasien tanpa orang kedua (`LAB-DEC-182`, `297`); seluruh penyerahan tertutup sampai Kepala Instalasi mengonfirmasi dan mengaktifkan (`LAB-SIGN-002`, `LAB-DEC-295`) |

**Sisa yang tidak memblokir desain:** konfirmasi `LAB-SIGN-002`, isi `LAB-OPEN-048`, dan penunjukan penyetuju lewat nota
`LAB-OPEN-057` (isi baru) — ketiganya menahan **aktivasi**, bukan rancangan maupun pembangunan. `DEC-LAB-036` ditutup,
sehingga sambungan `S6-1` tidak lagi tertahan pada sisi `S17`.

| Slice | Revision 15 | **Revision 16** |
|---|---|---|
| `S17-1a` | ✅ `READY_FOR_DOMAIN_DESIGN` | ✅ `READY_FOR_DOMAIN_DESIGN` |
| `S17-1b` | `BUSINESS_DECISION_REQUIRED` | ✅ **`READY_FOR_DOMAIN_DESIGN`** |
| `S17-2` | Tertahan `LAB-COORD-015` | Tertahan `LAB-COORD-015` |

**Handoff 0J.8 berlaku untuk `S17-1` utuh** dengan perubahan: (a) decision ID yang mengikat ditambah `LAB-DEC-293`..`298`;
`LAB-DEC-280` mengikat hanya bagian *Dokter Lab*, `LAB-DEC-180` tanpa butir antrean; (b) syarat rancangan 5 diganti:
**gerbang satu rumus dengan tiga syarat per versi** (Selesai, aktivasi disiplin, persetujuan sensitif) dipakai aksi dan
penanda layar, ditegakkan backend pada kirim, cetak, dan unduh; (c) dari daftar *yang tidak boleh muncul* dicoret *peran/izin
penyetuju atau catatan persetujuan*; ditambah: *persetujuan oleh perilis order*, *persetujuan yang berlaku lintas versi*,
*gerbang yang terbuka tanpa aktivasi*, *syarat Profesor bergelar*. Snapshot bukti: decisions **rev 147**.

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
| 16 | 2026-11-27 | **`S17-1b` dinilai ulang sesudah closure pass putaran 36** (bagian 0J.9): `DEC-LAB-035` dan `DEC-LAB-036` ditutup `LAB-DEC-293` dan `LAB-DEC-298`; syarat persetujuan dijabarkan `LAB-DEC-294`..`297` (kategori sensitif, setelan aktivasi per disiplin bawaan nonaktif, izin aktivasi tersendiri, penyetuju kedua bukan perilis per versi). **`S17-1` utuh naik `READY_FOR_DOMAIN_DESIGN`**; handoff 0J.8 diperluas. Isi `LAB-OPEN-048`, konfirmasi `LAB-SIGN-002`, dan nota `LAB-OPEN-057` menahan aktivasi saja | `draft` |
| 15 | 2026-11-27 | **`S17-1` penyerahan hasil ke pasien dinilai ulang sesudah amendment pass putaran 34 dan closure pass putaran 35** (bagian 0J), atas decisions rev 145 dan capability map rev 11. `S17-1` dipecah: **`S17-1a`** (dokumen hasil berversi, arsip, *Preview*, salinan WhatsApp bersandi, kirim manual dan lognya, nomor ternormalkan, izin kirim) tetap **`READY_FOR_DOMAIN_DESIGN`**; **`S17-1b`** (syarat persetujuan pada gerbang) **`BUSINESS_DECISION_REQUIRED`**. Dua pertentangan antar-keputusan `approved` ditemukan dan **tidak** diputuskan di sini: **`DEC-LAB-035`** — `LAB-DEC-182` (peran penyetuju, hasil normal tanpa persetujuan tambahan, menutup `LAB-OPEN-029`) vs `LAB-DEC-280` (syarat Profesor tetap, `LAB-OPEN-029` terbuka), `BLOCKING` bagi `S17-1b`; **`DEC-LAB-036`** — `LAB-DEC-180` (antrean `Pending` otomatis) vs `LAB-DEC-285`/`289` (penanda turunan), `BLOCKING` hanya bagi sambungan `S6-1`. Gerbang PA yang `MISSING` pada 0G.10 tertutup oleh `LAB-DEC-288`. Lima usulan `NON_BLOCKING_STANDARD`, termasuk arsip pada penyimpanan privat per area sebagai ganti `LAB-COORD-019` | `draft` |
| 14 | 2026-11-25 | **Slice baru `S20` penjadwalan ulang pemeriksaan dinilai sesudah amendment pass putaran 32 dan closure pass putaran 33** (bagian 0I). `S20` dipecah dua: **`S20-1` sisi Laboratorium** — kedelapan belas dimensi terisi, nol `BLOCKING`, nol `CONFLICT` terbuka, tiga usulan `NON_BLOCKING_STANDARD` (konkurensi layar basi, tanpa aksi batalkan jadwal ulang, layar pembuatan pesanan Lab tanpa pilihan tanggal) → **`READY_FOR_DOMAIN_DESIGN`**; **`S20-2` layar pemesan dokter** ditunda `LATER SLICE` (`LAB-COORD-023`). `S20` dinyatakan independen dari `S1b` (`LAB-AMD-001`). Decision Log: nol butir baru. Handoff 0I.8 ke `hospital-domain-architect` dengan lima syarat rancangan | `draft` |
| 13 | 2026-10-20 | **`S16b` dinilai ulang sesudah amendment pass putaran 30 dan closure pass putaran 31** (bagian 0H). `S16b` dipecah tiga: **`S16b-1` Laporan Pembatalan Order & Pemeriksaan** dan **`S16b-2` Statistik Pesanan Laboratorium** naik **`READY_FOR_DOMAIN_DESIGN`** — kedelapan belas dimensi terjawab (`LAB-DEC-230`..`249`), nol `BLOCKING`, nol `CONFLICT` terbuka; **`S16b-3`** sisa katalog tetap ditunda. Empat usulan bertanda: jejak *membuka* laporan berisi identitas pasien, periode kosong, bawaan izin, data uji non-Rawat Jalan. Handoff 0H.8: syarat nol konsep tersimpan, baca dari peristiwa, batas privasi, baca `BC-REG` pertama bagi laporan, satu rumus dengan Beranda. Bukti: decisions rev 115, capability map rev 9 (BE `4d3e4f98`, FE `7fb1e1d3e`) | `draft` |
| 12 | 2026-10-06 | **Sembilan slice dinilai ulang sesudah amendment pass putaran 21** (bagian 0G). **Delapan slice atau bagian naik `READY_FOR_DOMAIN_DESIGN`:** `S2b-1`, `S2b-3`, `S4d-2`, `S4e`, `S5`, `S6-1`, `S17-1`, `S18` (sisi Laboratorium). **Ratifikasi klinis `LAB-SIGN-002` diuji per keputusan** — tidak memblokir desain selama penolakan kelak tidak membongkar rancangan; satu syarat rancangan lahir darinya: empat mata ditegakkan per disiplin. **Dua penahan baru ditemukan gerbang ini:** `DEC-LAB-030` — nomor cetak tersimpan satu per order (`LabOrder.LabReportNumber`), sehingga order PA campuran histopatologi dan FNAB tidak punya seri yang pasti; `DEC-LAB-034` — akibat tarik hasil pada pemeriksaan, order *Selesai*, dan tagihan pemeriksaan ulang. `S6` dan `S17` dipecah; `S17-2` menunggu `LAB-COORD-015`, dan `owners.security` belum ditetapkan. Tiga butir dikunci sebelum kontrak, tidak memblokir desain: `DEC-LAB-031` (lebih dari satu `Sementara`), `DEC-LAB-032` (rilis mana yang dihitung `S7`/`S16a`), `DEC-LAB-033` (persetujuan perubahan SLA). Titik picu alur kritis ditetapkan **saat rilis** sebagai turunan langsung `LAB-DEC-004` dan `LAB-DEC-155`. `S16b` ditunda Phase 2, tidak dinilai | `draft` |
| 11 | 2026-09-25 | **Kelima slice 0E dinilai ulang sesudah amendment pass putaran 19** (bagian 0F). **`S16a` naik `READY_FOR_DOMAIN_DESIGN`** — kedelapan belas dimensi terjawab atau berusulan tidak memblokir; laporan PA dan Mikrobiologi ditulis *belum dapat dihitung* sampai jalur rilisnya ada. `S8` **dilebur** ke `S5`/`S6`; `S19` **tidak dirancang**; `S18` sisi Laboratorium lengkap, penahannya di luar modul. **`S2b` dipecah tiga**, dan pembacaan ulang cetakan PA `LAB-EVD-005` menemukan dua hal yang belum pernah diputuskan: **lokasi bernomor lebih dari satu** (`DEC-LAB-029`, kardinalitas) dan **ruas Fiksasi** (`DEC-LAB-028`). Isi awal daftar lokasi diusulkan dari 166 baris lokasi SNOMED yang sudah ter-seed nonaktif | `draft` |
| 10 | 2026-09-25 | **`S2b`, `S8`, `S16`, `S18`, `S19` dinilai ulang atas permintaan pemilik modul** (bagian 0E) — kelimanya belum pernah disentuh sejak revision 3. **Nol slice naik.** Penahan lama diganti yang bernama: `S2b` menyempit — nomor PA sudah dijawab `LAB-DEC-117` dan bagian Mikrobiologi terbangun lewat `S4b`; sisanya `DEC-LAB-022` (seri Sitologi/FNAB, pelacakan blok dan slide) dan `DEC-LAB-023` (lokasi dan metode pengambilan). `S8`: `LAB-COORD-001` hanya menyepakati **pemilik** — platform masih nol sarana pemberitahuan pada `cfafad8d`; penahan kini `DEC-LAB-024` dan `LAB-COORD-017`. `S16`: `DEC-LAB-025`; dokumen sumber analisis konsolidasi **tidak tersimpan** di repository. **`S18`: kemampuan dokumen klinis pasien milik Clinical Management ditemukan** — jenis `LaboratoryResult`, sumber `ExternalHospital`, status verifikasi — tanpa unggahan berkas dan tanpa layar, **belum ber-CAP**; `LAB-DEC-030` bertentangan soal pemiliknya → `DEC-LAB-026`, `DEC-LAB-016` diperluas. `S19`: jalur umum sudah menerima kunjungan `MedicalCheckup`; `DEC-LAB-027` | `draft` |
| 9 | 2026-09-25 | **`S4d` dan `S4e` dinilai ulang sesudah jawaban tertulis dr. Bima** (bagian 0D). Kedua penahan lamanya — `DEC-LAB-011` dan `LAB-OPEN-034` — tertutup `LAB-DEC-152`, **tetapi penilaian 18 dimensi menemukan dua keputusan klinis yang belum pernah ditanyakan**, masing-masing dibuka sebagai decision ID baru. **`S4d` dipecah:** `S4d-1` — hasil Mikrobiologi yang bukan `Sementara` — naik **`READY_FOR_DOMAIN_DESIGN`** dan mewarisi rancangan `S4`; `S4d-2` — hasil `Sementara` — tertahan **`DEC-LAB-020`**: kualifikasi `Sementara` sudah diputuskan **dicetak** (`LAB-DEC-114`), tetapi nol keputusan menyatakan apakah ia boleh dirilis dan bagaimana hasil `Definitif` menggantikannya. **`S4e` tetap `BUSINESS_DECISION_REQUIRED`, penahannya berganti** menjadi **`DEC-LAB-021`**: pengisi laporan PA adalah patolog (`LAB-DEC-090`) sedangkan pengisi dilarang memvalidasi hasilnya sendiri (`LAB-DEC-003`) — pertanyaan yang **`LAB-DEC-090` sendiri catat belum terjawab** sejak 2026-09-18. Keduanya wewenang klinis per disiplin (`LAB-DEC-079`): `DR-LAB-002` dan `DR-LAB-003`. `DEC-LAB-017` dinyatakan berlaku sejenis bagi kedua disiplin | `draft` |
| 8 | 2026-09-24 | **`S4` dinilai ulang sesudah `LAB-DEC-150`** (bagian 0C). Kedelapan belas dimensi dinilai; `S4` naik **`READY_FOR_DOMAIN_DESIGN` untuk desain saja**, sebab ketiga penahan yang tersisa menjawab *siapa* dan *kapan*, bukan *bentuk*. **Pemakaian nyata tetap tertahan** oleh `DEC-LAB-011` sisa dan **dua decision ID baru**: `DEC-LAB-017` — bolehkah `S4` dipakai sebelum `S5` pelaporan kritis berdiri — dan `DEC-LAB-018` — siapa pemegang kewenangan **rilis** Patologi Klinik, butir yang `LAB-DEC-150` tidak jawab padahal `LAB-DEC-120` mewajibkan perilis berbeda dari pemvalidasi. Slice lain **tidak** dinilai ulang. Handoff `S4` ke `hospital-domain-architect` | `draft` |
| 7 | 2026-09-18 | **Penilaian ulang sesudah amendment pass putaran 6 menjawab keempat butir yang `r6` tunjuk sebagai milik pemilik modul.** Ditulis sebagai bagian **0B**. `S4b` naik **`READY_FOR_DOMAIN_DESIGN`** menemani `S4c`, sehingga **dua** slice kini dikirim ke arsitektur domain; `S4` turun menjadi **satu** penahan; dua slice **baru** lahir — `S4d` dan `S4e` validasi/rilis Mikrobiologi dan Patologi Anatomi — langsung dengan dua penahan. **Dan satu penilaian `r6` saya koreksi sendiri:** `0A.13` menyatakan `LAB-P0-003` dapat ditutup pemilik modul sendirian; wawancara membuktikan **tidak**. `LAB-REQ-004` bagian 4.5 memuat empat pertanyaan turunan `LAB-DEC-007` dan **tiga di antaranya klinis**. Gerbang `r6` membacanya dari judul — *"aturan pembatalan dan koreksi"* — lalu menyimpulkan ia sejenis `LAB-DEC-063`. Yang ditutup pemilik modul nyata tetapi sebagian: bentuk alasan dan bentuk versi. Sisanya `DEC-LAB-014`, dan `S6` **tidak** terbuka. **Satu penilaian `r6` yang lain ternyata terlalu keras:** `LAB-OPEN-017` ditandai `BLOCKING` bagi `S4b` atas dasar ia ruas di dalam struktur hasil Mikrobiologi — benar sebagai pernyataan struktur, **keliru sebagai penahan rilis**, sebab blueprint sudah menempatkan penanda `Definitif` pada Rilis 2 sejak semula. `LAB-DEC-081` hanya membaca yang sudah tertulis, dan penahan tunggal `S4b` lenyap tanpa satu pun keputusan klinis baru. **Penahan yang kini paling mahal seluruh modul: `DEC-LAB-011`** — ia menahan `S4`, `S4d`, dan `S4e` sekaligus, yaitu **seluruh kemampuan merilis hasil untuk ketiga disiplin**, dan pertanyaannya tetap yang paling sederhana dari semuanya: siapa yang berhak menunjuk pemegang kewenangan validasi. **`grill-me` selesai untuk putaran ini** — nol butir tersisa yang dapat ditutup pemilik modul sendirian. Kesiapan keseluruhan tetap `PARTIALLY_READY` | `draft` |
| 6 | 2026-09-17 | **Penilaian ulang sesudah `LAB-SIGN-001` ditutup `LAB-DEC-079` — tanda tangan klinis diberikan apa adanya, per disiplin, oleh `DR-LAB-001`/`002`/`003`.** Ditulis sebagai bagian **0A**. **Hasilnya satu slice naik dan empat menyempit, nol mundur:** `S4c` pengisian hasil Patologi Anatomi menjadi **`READY_FOR_DOMAIN_DESIGN`** — seluruh dimensinya terisi, bentuk hasilnya sudah dikunci BR-23 (makroskopik, mikroskopik, kesimpulan wajib), dan tanda tangannya kini ada. `S4` turun dari 4 penahan menjadi 2, `S4b` dari 3 menjadi **1**, `S5` dari 4 menjadi 3, `S6` dari 3 menjadi **1**. **Dua penahan diturunkan derajatnya dengan alasan berbukti, bukan dilonggarkan:** `LAB-P0-005` integrasi alat menjadi `NON_BLOCKING_STANDARD` untuk Rilis 1 sebab `LAB-DEC-005` sudah memutuskannya dan `S4a` sudah dibangun di atasnya; `LAB-P0-001` menjadi `NON_BLOCKING_STANDARD` bagi ketiga slice hasil sebab bagiannya yang menyangkut validasi dan rilis sudah dijawab `LAB-DEC-022` lalu disahkan `LAB-DEC-079`, dan memblokir `S4c` atas kewenangan mengisi berarti menerapkan ukuran yang tidak diterapkan pada `S4a`. **Temuan paling mahal justru bukan soal requirement, melainkan soal peta:** `S4b` dan `S4c` bernama *pengisian hasil*, dan **nol slice** memuat validasi serta rilis untuk Mikrobiologi dan Patologi Anatomi — `LAB-DEC-076` memecah `S4` hanya untuk Patologi Klinik. Celah itu tak terlihat selama `LAB-SIGN-001` menahan kelimanya sekaligus; tanda tangan per disiplin membuatnya terlihat sekaligus mendesak, sebab `DR-LAB-002` dan `DR-LAB-003` kini memegang wewenang atas disiplin yang tidak punya tempat menjalankannya. Dibuka `DEC-LAB-013`. **Tiga decision ID baru:** `DEC-LAB-011` (siapa menetapkan pemegang kewenangan validasi, dan jaminan dua per shift — `LAB-REQ-004` bagian 5.1 tidak ikut dijawab), `DEC-LAB-012` (pemberi persetujuan klinis atas perubahan batas kritis — bagian 5.2, juga tidak dijawab), dan `DEC-LAB-013`. **Dua koreksi pembukuan:** `LAB-COORD-001` dan `LAB-COORD-002` masih tertulis sebagai penahan `S5`/`S6` pada bagian 5 dan 7, padahal `closed` sejak 2026-09-01 — bagian itu ditandai rekam jejak, bukan disunting. **Dan satu pernyataan revision 2 dinyatakan tidak berlaku lagi:** *"`grill-me` tidak akan membuka apa-apa lagi tanpa kehadiran pihak-pihak itu"* — hari ini tiga penahan dapat ditutup pemilik modul sendiri (`LAB-P0-002`, `LAB-OPEN-017`, `LAB-P0-003`), dan ketiganya penahan tunggal atau terakhir bagi slice-nya. Kesiapan keseluruhan tetap `PARTIALLY_READY`. Nol entity, ERD, kontrak, migration, atau task diturunkan | `draft` |
| 3-5 | — | **Tidak tercatat.** Header dokumen ini sempat menunjuk revision `3`, `4`, lalu `5`, tetapi baris riwayatnya tidak pernah ditulis. Isi revision 3 masih dapat dibaca pada bagian 0 yang menamai dirinya sendiri *"Penilaian Ulang Revision 3"*; revision 4 dan 5 **tidak dapat dipulihkan** dari dokumen mana pun. Yang diketahui tentang revision 5: ia memecah baris `S4` pada peta 0.2 menjadi `S4a` + `S4` mengikuti `LAB-DEC-076` pada 2026-09-17. Ketiadaan ini dicatat apa adanya, bukan disusun ulang dari ingatan | — |
| 1 | 2026-09-01 | Penilaian pertama. 13 bagian pekerjaan dinilai terhadap 18 dimensi. Tiga gap pemblokir baru ditemukan: `DEC-LAB-001`, `DEC-LAB-002`, `DEC-LAB-003`. Lima bagian dinyatakan siap dirancang, tujuh harus berhenti. Kesiapan keseluruhan `PARTIALLY_READY` | `draft` |
| 2 | 2026-09-01 | Ketiga gap ditutup pemilik modul lewat `grill-me` closure pass putaran kedua (`LAB-DEC-021`, `LAB-DEC-022`, `LAB-DEC-023`). Dimensi 02, 06, dan 09 naik menjadi `CONFIRMED`. `S3` naik menjadi siap dirancang sehingga slice yang dikirim menjadi enam. Blocker tersisa empat, seluruhnya memerlukan pihak di luar modul | `draft` |
