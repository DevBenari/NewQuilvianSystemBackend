# MCU (Medical Check-Up) — Penilaian Kelengkapan Requirement

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` |
| Assessment ID | `MCU-RCG-001` |
| Revision | `2` — kesiapan diperbarui sesudah putaran 3 decision log (`MCU-DEC-032`..`035`) |
| Status | `draft` |
| Tanggal | 2026-10-20 |
| Masukan | [00-interview-decisions.md](00-interview-decisions.md) revision 2 (r1 penilaian), revision 3 (r2 penilaian) (`MCU-DEC-001`..`031`); [01-existing-capability-map.md](01-existing-capability-map.md) revision 1 (`MCU-CAP-01`..`38`); [evidence/2026-10-19-kebutuhan-mcu.md](evidence/2026-10-19-kebutuhan-mcu.md) (`MCU-EVD-001`) |
| SHA yang dirujuk | Backend `4d3e4f98`, frontend `7fb1e1d3e` — sama dengan capability map |
| Baseline rujukan | `indonesia-hospital-domain-reference`: **Laboratory** (`PARTIAL`) — `ID-LAB-CAP-011`, `ID-LAB-CAP-012`, `ID-LAB-CAP-013`, `ID-LAB-REG-002`, bagian 8 dan 9. **Occupational Health, Medical Record, Billing & Charge Capture, Notification: `NOT_YET_AVAILABLE`** — tidak ada rujukan canonical khusus MCU |
| Pemilik keputusan | Yoga Aji Pratama (`MCU-DEC-002`) |
| Sifat | Read-only. Tidak ada entity, ERD, kontrak, atau source yang dibuat |

> **Cara membaca dokumen ini.** Penilaian ini menjawab satu pertanyaan: *apakah kebutuhan MCU
> sudah cukup jelas untuk mulai dirancang arsitektur domainnya?* Jawabannya diberikan **per bagian
> modul**, bukan untuk seluruh modul sekaligus. Bagian yang jelas boleh lanjut; bagian yang masih
> punya pertanyaan penting ditahan, dan pertanyaannya diberi nomor `DEC-MCU-xxx` untuk dijawab
> pemilik lewat `/grill-me`.

---

## 0. Pembaruan revision 2 (2026-10-20)

Empat dari lima pertanyaan pemblokir dijawab pemilik pada hari yang sama:

| Pertanyaan | Ditutup oleh | Inti jawaban |
| --- | --- | --- |
| `DEC-MCU-001` | `MCU-DEC-032` | Dokter MCU pemeriksa ditetapkan saat check-in; ia penanggung jawab pesanan, penerima nilai kritis, dan penyusun resume; eskalasi ke kepala unit MCU |
| `DEC-MCU-002` | `MCU-DEC-034` | Nilai jasa medis per komponen di master paket, diserahkan ke Medical Fee saat komponen selesai |
| `DEC-MCU-003` | `MCU-DEC-035` | Rilis pertama tanpa lampiran berkas |
| `DEC-MCU-004` | `MCU-DEC-033` | Peserta baru dibooking dengan identitas sementara (NIK wajib); jadi pasien saat check-in lewat pencocokan NIK |

**Kesiapan revision 2:**

| Slice | Revision 1 | **Revision 2** |
| --- | --- | --- |
| `MCU-S1` Master | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` — kini juga memuat nilai jasa medis per komponen |
| `MCU-S2` Booking | `BUSINESS_DECISION_REQUIRED` | **`READY_FOR_DOMAIN_DESIGN`** |
| `MCU-S3` Pendaftaran & check-in | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** |
| `MCU-S4` Pembayaran & tagihan | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** (S4a dan S4b) |
| `MCU-S5` Pesanan Lab/Rad | `BUSINESS_DECISION_REQUIRED` | **`READY_FOR_DOMAIN_DESIGN`** |
| `MCU-S6` Hasil & kelengkapan | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** — lampiran di luar rilis pertama |
| `MCU-S7` Resume & verifikasi | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` — `MCU-ASM-004` dibawa sebagai asumsi |
| `MCU-S8` Penyerahan & privasi | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` |
| `MCU-S9` Laporan | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` — `DEC-MCU-005` |

**Keseluruhan revision 2: `PARTIALLY_READY`** — delapan slice siap; hanya S9 Laporan yang tertahan, dan
S9 independen dari slice lain karena hanya membaca data. S1–S8 boleh dikirim ke
`/hospital-domain-architect` bersama `MCU-ASM-004`, `MCU-OQ-002`, `MCU-OQ-003`, `MCU-GAP-01`..`07`, dan
`MCU-COORD-001`..`010`.

Bagian 1–9 di bawah adalah penilaian revision 1 dan dipertahankan sebagai jejak.

## 1. Ringkasan (revision 1)

| Hasil | Penjelasan |
| --- | --- |
| **Kesiapan keseluruhan: `PARTIALLY_READY`** | 5 dari 9 bagian siap; 4 bagian tertahan oleh 5 pertanyaan baru |
| Keputusan yang sudah kuat | 31 keputusan `approved` menutup scope, booking, pembayaran, tagihan, status, hasil, privasi, koreksi, dan kanal |
| Celah baru yang ditemukan | 5 pertanyaan memblokir (`DEC-MCU-001`..`005`) — tiga di antaranya tidak muncul pada wawancara karena baru terlihat sesudah membandingkan dengan kode dan rujukan Laboratorium |
| Celah yang tidak memblokir desain | 7 butir usulan standar atau nilai yang dapat dikonfigurasi (`MCU-GAP-01`..`07`) |
| Rekomendasi | Jawab `DEC-MCU-001`..`004` dalam satu putaran `/grill-me` singkat **sebelum** arsitektur domain, karena dua di antaranya menyentuh inti order paket. `DEC-MCU-005` (laporan) boleh menyusul |

**Kenapa tidak langsung lanjut ke arsitektur domain walaupun 5 bagian siap?** Order paket MCU
adalah inti hampir semua bagian. Dua pertanyaan baru — siapa pasiennya saat booking peserta
baru (`DEC-MCU-004`) dan siapa dokter penanggung jawab pesanan Lab/Rad (`DEC-MCU-001`) —
menentukan bentuk inti itu. Merancang bagian yang "siap" lebih dulu berisiko dirombak.

---

## 2. Scope penilaian

Modul `mcu`, dipecah menjadi sembilan bagian (slice). Batasnya mengikuti butir sumber dan
keputusan yang ada, bukan layar atau menu.

| Slice | Nama | Butir sumber | Keputusan utama |
| --- | --- | --- | --- |
| `MCU-S1` | Master paket, komponen, harga, persyaratan, hari buka dan kuota | 1A–1D, 3 | `MCU-DEC-005`, `007`, `012`, `013`, `015`, `020`, `025`, `029` |
| `MCU-S2` | Booking, rekomendasi dokter, konfirmasi WhatsApp, tidak hadir, jadwal ulang | 2, 3, 13B–13D | `MCU-DEC-003`, `006`, `014`, `017`, `029`, `030` |
| `MCU-S3` | Pendaftaran *walk-in* dan check-in (admisi, kiosk), pembuatan kunjungan, cek persyaratan | 4, 5, 13A, 13E | `MCU-DEC-003`, `013`, `017`, `023` |
| `MCU-S4` | Pembayaran, penjaminan perusahaan, tagihan satu baris, harga perusahaan, batal penuh, refund komponen | 11, 1C | `MCU-DEC-004`, `005`, `015`, `020`, `021`, `022`, `025`, `026` |
| `MCU-S5` | Pemecahan paket menjadi pesanan Lab/Radiologi | 13F | `MCU-DEC-005`, `022`, `024` |
| `MCU-S6` | Hasil komponen dan kelengkapan (Pemeriksaan MCU, skrining, status Lab/Rad) | 7, 8 | `MCU-DEC-007`, `009`, `018` |
| `MCU-S7` | Resume hasil MCU, approve dokter, verifikasi, koreksi berversi | 9, 10 | `MCU-DEC-008`, `009`, `011` |
| `MCU-S8` | Penyerahan hasil dan privasi penerima | 12 | `MCU-DEC-010`, `019`, `027` |
| `MCU-S9` | Laporan MCU | 14A–14D | `MCU-OQ-005` |

---

## 3. Bukti yang dipakai

| Wewenang | Bukti | Dipakai untuk |
| ---: | --- | --- |
| 1 | Jawaban pemilik modul pada wawancara 2026-10-20 (`MCU-DEC-001`..`031`) | Apa yang **harus** dibangun |
| 1 | Daftar kebutuhan 14 butir dari pemilik (`MCU-EVD-001`) | Batas kebutuhan awal |
| 2–4 | SOP, notulen, atau ClickUp MCU | **Tidak tersedia.** Tidak ada SOP MCU, contoh formulir hasil, maupun contoh cetakan resume |
| 5 | Baseline rujukan Laboratorium (`REFERENCE_ONLY`) | Deteksi celah: nilai kritis, laporan, koreksi, rekam medis |
| 6 | Capability map revision 1 | Apa yang **sudah ada** hari ini |
| 7 | Frontend v1 MCU | Referensi tampilan saja (`MCU-DEC-016`) |

Tidak ada pertentangan antarbukti yang terbuka: `MCU-CONFLICT-001`..`008` sudah ditutup.

---

## 4. Temuan per dimensi

Kode isi sel: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **–** = tidak berlaku
secara material (alasannya di bagian 4.2).

### 4.1 Matriks

| No | Dimensi | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 | S9 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 01 | Tujuan | C | C | C | C | C | C | C | C | C |
| 02 | Aktor | C | C | C | C | **M** `DEC-MCU-001` | C | C | C | P |
| 03 | Pemicu / prasyarat | C | **M** `DEC-MCU-004` | C | C | C | C | C | C | M |
| 04 | Alur utama | C | C | C | C | C | C | C | C | M |
| 05 | Alur alternatif / exception | C | C | C | C | C | C | C | C | – |
| 06 | Data minimum | P `MCU-GAP-01` | **M** `DEC-MCU-004` | C | C | C | P `MCU-OQ-002`; **M** `DEC-MCU-003` | P `MCU-OQ-003` | P `MCU-OQ-003` | M |
| 07 | Aturan bisnis / validation | C | C | C | C | C | C | C | C | M |
| 08 | Status / perubahan status | – | C | C | C | C | C | C | C | – |
| 09 | Peran / authorization | P `MCU-GAP-02` | C | C | C | **M** `DEC-MCU-001` | P `MCU-GAP-02` | P `MCU-GAP-02`, `MCU-GAP-03` | C | P |
| 10 | Dependency antarmodul | C | C | C | C | C | C | P `MCU-GAP-04` | C | M |
| 11 | Integrasi internal / eksternal | – | C | C | C | C | C | – | C | – |
| 12 | Hasil akhir | C | C | C | C | C | C | C | C | M |
| 13 | Pembatalan / koreksi | C | C | C | C | C | C | C | C | – |
| 14 | Audit / histori | P `MCU-GAP-05` | C | C | C | C | C | C | C | – |
| 15 | Notifikasi | – | C | – | – | **M** `DEC-MCU-001` | – | – | C | – |
| 16 | Dampak billing | C | – | C | C; **M** `DEC-MCU-002` | C | – | – | – | P |
| 17 | Keselamatan klinis | P `MCU-GAP-06` | – | C | – | **M** `DEC-MCU-001` | C | P `MCU-GAP-03` | C | – |
| 18 | Pelaporan / traceability | C | C | C | C | C | C | P `MCU-GAP-04` | C | **M** `DEC-MCU-005` |

### 4.2 Alasan dimensi yang tidak berlaku

| Slice | Dimensi | Alasan |
| --- | --- | --- |
| S1 | 08, 11, 15 | Master hanya aktif/nonaktif dan masa berlaku; tidak ada integrasi luar dan tidak ada kejadian yang perlu diberitahukan |
| S2 | 16, 17 | Booking tidak membuat kunjungan maupun tagihan (`MCU-DEC-003`) dan belum ada tindakan klinis |
| S3 | 15 | Check-in tidak memicu pemberitahuan yang diminta |
| S4 | 15, 17 | Pembayaran tidak punya pemberitahuan yang diminta dan tidak mengubah tindakan klinis |
| S6, S7 | 15 | Pemberitahuan hasil terjadi di S8; nilai kritis dibahas di S5 |
| S6, S7, S8 | 16 | Tagihan paket sudah lengkap di S4; hasil tidak mengubah tagihan |
| S7 | 11 | Tidak ada integrasi luar; tanda tangan elektronik dicatat sebagai `MCU-GAP-03` |
| S9 | 05, 08, 13, 14, 15, 17 | Laporan hanya membaca data yang sudah ada |

---

## 5. Butir yang tidak memblokir desain

| ID | Slice | Butir | Status | Dampak | Penjelasan dan contoh |
| --- | --- | --- | --- | --- | --- |
| `MCU-GAP-01` | S1 | Daftar persyaratan pemeriksaan (puasa, tidak haid, membawa kacamata) dan isinya per komponen | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Bentuknya sudah diputuskan (`MCU-DEC-013`); isinya berbeda antar rumah sakit. Contoh: *Gula Darah Puasa — puasa 10–12 jam* diisi admin, bukan ditanam di sistem |
| `MCU-GAP-02` | S1, S6, S7 | Jabatan mana yang memegang peran *Admin MCU*, *Petugas MCU*, *Dokter MCU*, *Kepala unit MCU* | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Izin di sistem ini diatur per departemen dan posisi. Pembagian tugasnya sudah `CONFIRMED` (bagian 6 decision log); siapa pemegangnya diatur lewat konfigurasi izin |
| `MCU-GAP-03` | S7 | Tanda tangan dokter pada resume — cukup nama dokter dan waktu approve, atau tanda tangan elektronik tersertifikasi; serta syarat sertifikasi dokter pemeriksa kesehatan kerja | `PROPOSED` | `NON_BLOCKING_STANDARD` — `VERIFY_CURRENT_REGULATION` | Usulan standar: resume menyimpan dokter yang approve dan waktunya, dan cetakan menampilkannya. Tanda tangan elektronik tersertifikasi dan syarat sertifikasi dokter adalah topik regulasi yang **belum diverifikasi** — tidak ada sumber terverifikasi pada rujukan |
| `MCU-GAP-04` | S7 | Apakah resume MCU menjadi bagian rekam medis pasien dan tampil di linimasa Rekam Medis, serta masa simpannya | `PROPOSED` | `NON_BLOCKING_STANDARD` — `VERIFY_CURRENT_REGULATION` (`ID-LAB-REG-002`) | Usulan: resume tetap milik MCU dan dibaca Rekam Medis lewat jalur baca, seperti hasil Lab. Rekam Medis sendiri sudah mencatat MCU sebagai produsen hasil yang menyusul (`rekam-medis` `00-interview-decisions.md:81`) |
| `MCU-GAP-05` | S1 | Riwayat perubahan harga dan isi paket | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: perubahan isi paket tidak mengubah order yang sudah dibuat; order menyimpan potret isi paket saat dibuat. Contoh: Paket Basic ditambah Kolesterol pada 1 November — order Budi 27 Oktober tetap tiga komponen |
| `MCU-GAP-06` | S1 | Komponen yang tidak boleh untuk kondisi tertentu (misalnya rontgen pada kehamilan) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Sudah tertangani lewat pembatalan komponen oleh dokter (`MCU-DEC-015`) dan pemeriksaan keselamatan radiasi milik Radiologi (`MstRadSafetyRequirement`). Tidak perlu aturan baru di master MCU |
| `MCU-GAP-07` | S3 | Kelas pasien untuk kunjungan MCU — rawat jalan menentukannya otomatis, MCU belum (`MCU-CAP-02`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Harga paket sudah tetap per baris tarif (`MCU-DEC-025`), sehingga kelas pasien tidak mengubah harga paket. Usulan: memakai kelas bawaan yang sama dengan rawat jalan |
| `MCU-OQ-002` | S6 | Jenis *Pemeriksaan MCU* dan isiannya | `MISSING` (isi) | `CONFIGURABLE_DEFAULT` untuk desain; memblokir implementasi layar | Bentuk domainnya sudah jelas (komponen berhasil isian, pengisi, waktu). Isian per jenis menunggu contoh formulir dari dokter MCU. Lampiran berkas dipisah ke `DEC-MCU-003` |
| `MCU-OQ-003` | S7, S8 | Daftar kategori kelayakan dan bentuk bukti persetujuan pelepasan hasil | `MISSING` (isi) | `CONFIGURABLE_DEFAULT` untuk desain; memblokir implementasi | Keberadaan kategori dan persetujuan sudah diputuskan (`MCU-DEC-010`). Daftarnya diatur sebagai pilihan; bentuk bukti menunggu pemilik privasi/legal |

---

## 6. Decision Log — pertanyaan yang memblokir

### `DEC-MCU-001` — Dokter penanggung jawab pesanan dari paket dan penerima hasil kritis

> **Ditutup revision 2** oleh `MCU-DEC-032`.

| Field | Isi |
| --- | --- |
| Pertanyaan | Pesanan Lab/Radiologi dari paket dibuat oleh sistem, bukan oleh dokter. **Siapa dokter penanggung jawab pesanan itu**, dan **siapa yang menerima serta menindaklanjuti nilai kritis** sebelum resume disusun? |
| Slice terdampak | `MCU-S5`; berimbas ke `MCU-S6` |
| Bukti saat ini | Pesanan Lab punya `InstructingDoctorId` dan `RequestedByUserId`, dan verifikasi instruksi hanya berlaku untuk rawat inap (`LabOrderInstructionVerificationStatus`); pesanan Radiologi punya `InstructingDoctorId`. Laboratorium pernah menandai *pemesan non-dokter* sebagai pertanyaan MCU (`DEC-LAB-027`). Tidak ada keputusan MCU yang menyebut dokter pesanan maupun nilai kritis |
| Baseline (`REFERENCE_ONLY`) | `ID-LAB-CAP-011`: hasil kritis butuh penerima pemberitahuan, eskalasi, konfirmasi penerimaan, dan penanganan bila kontak gagal |
| Contoh kenapa penting | Gula darah puasa Siti keluar 450 mg/dL pukul 09.00. Resume baru disusun esok harinya. Tanpa dokter penanggung jawab, analis Lab tidak tahu harus menelepon siapa, dan Siti pulang tanpa diberi tahu |
| Usulan pilihan | (a) dokter MCU yang ditetapkan saat check-in; (b) dokter MCU yang bertugas hari itu; (c) dokter pemberi rekomendasi untuk pintu RJ/RI/IGD, dokter MCU untuk pintu lain |
| Dampak | Data pesanan, kewenangan, keselamatan klinis, pemberitahuan |
| Pemilik | Yoga Aji Pratama + dokter MCU |
| Status | `OPEN` — **`BLOCKING`** |
| Dampak domain | Menahan desain S5. Tanpa jawaban, bentuk penautan dokter pada order paket dan pesanan turunannya tidak dapat ditetapkan |

### `DEC-MCU-002` — Jasa medis dari paket yang ditagih satu baris

> **Ditutup revision 2** oleh `MCU-DEC-034`.

| Field | Isi |
| --- | --- |
| Pertanyaan | Paket ditagih satu baris dan Lab/Rad tidak menerbitkan tagihan per pemeriksaan (`MCU-DEC-005`, `022`). **Bagaimana jasa medis dokter** — dokter MCU yang memeriksa fisik, dokter radiologi yang membaca Thorax, dokter patologi klinik — **dihitung dari paket?** |
| Slice terdampak | `MCU-S4` bagian jasa medis (`MCU-S4b`) |
| Bukti saat ini | Medical Fee menghitung bagian dokter per baris tagihan (`BilInvoiceItem.DoctorShare`, `MF-DEC-001` di blueprint `medical-fee`). Dengan satu baris paket, tidak ada baris per komponen yang dapat dihitung |
| Contoh kenapa penting | Paket Basic Rp 350.000. Bila dihitung sebagai satu baris, siapa yang mendapat bagian: dokter MCU saja, atau juga dokter radiologi yang membaca Thorax? Bila dokter radiologi tidak mendapat bagian dari paket, ia dibayar untuk Thorax pasien poli tetapi tidak untuk Thorax peserta MCU |
| Usulan pilihan | (a) jasa medis per komponen memakai nilai yang diatur di master paket (mirip nilai pengembalian); (b) jasa medis paket ditetapkan sebagai satu nilai untuk dokter MCU saja; (c) diserahkan ke aturan Medical Fee tanpa pembagian dari MCU |
| Dampak | Konsekuensi finansial, kontrak integrasi dengan `medical-fee`, kemungkinan data tambahan pada master paket |
| Pemilik | Yoga Aji Pratama + pemilik `medical-fee` (+ Keuangan) |
| Status | `OPEN` — **`BLOCKING`** hanya untuk `MCU-S4b` |
| Dampak domain | Tagihan paket (S4a) boleh dirancang; pembagian jasa medis ditahan |

### `DEC-MCU-003` — Lampiran berkas hasil alat

> **Ditutup revision 2** oleh `MCU-DEC-035`.

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah hasil *Pemeriksaan MCU* tertentu — rekaman EKG, grafik audiometri, hasil spirometri — **wajib disimpan sebagai lampiran berkas atau gambar** dari alat, atau cukup isian angka dan kesimpulan? |
| Slice terdampak | `MCU-S6` bagian lampiran |
| Bukti saat ini | Health Services **belum punya** penyimpanan berkas (`CAP-P19-03` di capability map Laboratorium; dicatat ulang sebagai keterbatasan di `MCU-CAP-30`). `MCU-DEC-007` menyebut EKG, audiometri, dan spirometri sebagai contoh komponen |
| Contoh kenapa penting | Kesimpulan EKG Budi *irama sinus normal* diketik dokter. Bila suatu hari dipersoalkan, rekaman EKG aslinya tidak ada di sistem |
| Dampak | Data yang disimpan, ketergantungan ke kemampuan platform yang belum ada, privasi berkas |
| Pemilik | Yoga Aji Pratama + dokter MCU |
| Status | `OPEN` — **`BLOCKING`** hanya untuk lampiran di `MCU-S6` |
| Dampak domain | Isian hasil Pemeriksaan MCU boleh dirancang; lampiran ditahan. Bila jawabannya *wajib*, muncul ketergantungan ke pemilik penyimpanan berkas |

### `DEC-MCU-004` — Peserta yang belum punya nomor rekam medis saat booking

> **Ditutup revision 2** oleh `MCU-DEC-033`.

| Field | Isi |
| --- | --- |
| Pertanyaan | Saat admisi mencatat booking — terutama rombongan perusahaan — **apakah setiap peserta harus sudah terdaftar sebagai pasien (punya No. RM)**, atau booking boleh menyimpan identitas sementara yang baru dijadikan pasien saat check-in? Dan bolehkah booking rombongan dimasukkan sekaligus dari daftar (misalnya berkas Excel dari HRD perusahaan)? |
| Slice terdampak | `MCU-S2`; berimbas ke check-in peserta booking di `MCU-S3` |
| Bukti saat ini | `MCU-DEC-003` memutuskan booking membuat order tanpa kunjungan, tetapi tidak menyebut data pasien. Rombongan 25–40 orang muncul berulang pada contoh keputusan (`MCU-DEC-012`, `029`). Data pasien milik `patient-management` |
| Contoh kenapa penting | PT Contoh mengirim daftar 40 karyawan, 30 di antaranya belum pernah berobat. Bila No. RM wajib saat booking, admisi harus membuat 30 data pasien lebih dulu — termasuk untuk yang akhirnya tidak datang. Bila identitas sementara boleh, saat check-in perlu langkah pencocokan agar tidak terbentuk pasien ganda |
| Dampak | Identitas pasien, integritas data (pasien ganda), relasi order ke pasien, beban kerja admisi |
| Pemilik | Yoga Aji Pratama (+ pemilik `patient-management` untuk titik sentuh) |
| Status | `OPEN` — **`BLOCKING`** |
| Dampak domain | Menahan desain S2 dan check-in peserta booking di S3. Menentukan apakah order paket selalu menunjuk pasien yang sudah ada |

### `DEC-MCU-005` — Definisi laporan MCU

| Field | Isi |
| --- | --- |
| Pertanyaan | Untuk keempat laporan (rekap kunjungan, rekap per metode pembayaran, rekap pemeriksaan dan paket, statistik): apa rumusnya, tanggal mana yang dipakai (booking, check-in, atau diserahkan), bagaimana order batal dan tidak hadir dihitung, penyaringnya, pembacanya, dan format ekspornya? |
| Slice terdampak | `MCU-S9` |
| Bukti saat ini | Hanya judul laporan (`MCU-EVD-001` butir 14); `MCU-OQ-005` |
| Contoh kenapa penting | *Rekap kunjungan Oktober*: apakah Budi yang booking 30 Oktober tetapi check-in 2 November masuk Oktober atau November? |
| Pemilik | Yoga Aji Pratama |
| Status | `OPEN` — **`BLOCKING`** hanya untuk `MCU-S9` |
| Dampak domain | Laporan ditahan; bagian lain tidak terpengaruh, asalkan order menyimpan waktu tiap perpindahan status (sudah tercakup `MCU-DEC-031`) |

---

## 7. Kesiapan per slice

| Slice | Kesiapan | Penahan | Catatan |
| --- | --- | --- | --- |
| `MCU-S1` Master | `READY_FOR_DOMAIN_DESIGN` | — | `MCU-GAP-01`, `02`, `05`, `06` dibawa sebagai usulan |
| `MCU-S2` Booking | `BUSINESS_DECISION_REQUIRED` | `DEC-MCU-004` | — |
| `MCU-S3` Pendaftaran & check-in | `PARTIALLY_READY` | `DEC-MCU-004` untuk check-in peserta booking | *Walk-in* siap: pasien baru didaftarkan lewat jalur pasien yang sudah ada |
| `MCU-S4` Pembayaran & tagihan | `PARTIALLY_READY` | `DEC-MCU-002` untuk `S4b` jasa medis | `S4a` — lunas/terjamin, tagihan satu baris, harga perusahaan, batal, refund — siap |
| `MCU-S5` Pesanan Lab/Rad | `BUSINESS_DECISION_REQUIRED` | `DEC-MCU-001` | Tanda paket dan penahanan tagihan sudah diputuskan; yang kurang dokter penanggung jawab |
| `MCU-S6` Hasil & kelengkapan | `PARTIALLY_READY` | `DEC-MCU-003` untuk lampiran | Isian, kelengkapan, dan kunci skrining siap |
| `MCU-S7` Resume & verifikasi | `READY_FOR_DOMAIN_DESIGN` | — | `MCU-GAP-03`, `04`, `MCU-OQ-003` dibawa sebagai usulan/konfigurasi |
| `MCU-S8` Penyerahan & privasi | `READY_FOR_DOMAIN_DESIGN` | — | Pengiriman otomatis tergantung platform (`MCU-COORD-005`), bukan keputusan bisnis |
| `MCU-S9` Laporan | `BUSINESS_DECISION_REQUIRED` | `DEC-MCU-005` | Tidak termasuk rilis pertama |

**Keseluruhan: `PARTIALLY_READY`.**

**Ketergantungan antarslice.** S5, S6, S7, dan S8 semuanya bergantung pada order paket yang lahir
di S2/S3. Karena itu S7 dan S8 — walaupun siap — **sebaiknya tidak** dirancang terpisah dari order
paket. Bila tetap dikirim lebih dulu, arsitektur domain harus memperlakukan penautan order ke
pasien (`DEC-MCU-004`) dan ke dokter (`DEC-MCU-001`) sebagai hal yang belum diputuskan.

---

## 8. Yang boleh berjalan dan yang harus berhenti

| Boleh berjalan | Harus berhenti |
| --- | --- |
| `/grill-me` amendment pass untuk `DEC-MCU-001`..`004` (empat pertanyaan) | Arsitektur domain S2 dan S5 sebelum `DEC-MCU-004` dan `DEC-MCU-001` terjawab |
| Meminta contoh formulir dan cetakan MCU dari dokter MCU — penutup `MCU-OQ-002`, `MCU-OQ-003`, dan bahan `DEC-MCU-003` | Merancang pembagian jasa medis paket sebelum `DEC-MCU-002` |
| Mendaftarkan prefix modul MCU di registry (`MCU-GOV-001`) — tidak bergantung keputusan apa pun | Merancang laporan MCU sebelum `DEC-MCU-005` |
| Koordinasi dengan pemilik Lab, Rad, Kiosk, Billing atas `MCU-COORD-*` yang sudah jelas isinya | Menyimpan lampiran berkas MCU di modul MCU sebelum `DEC-MCU-003` dan pemilik penyimpanan berkas jelas |

---

## 9. Handoff

**Ke `/grill-me` (amendment pass) — direkomendasikan lebih dulu:** `DEC-MCU-001`, `DEC-MCU-004`
(inti order paket), lalu `DEC-MCU-002`, `DEC-MCU-003`. `DEC-MCU-005` boleh menyusul pada putaran
laporan.

**Ke `/hospital-domain-architect` — sesudah putaran di atas:** seluruh slice yang menjadi
`READY_FOR_DOMAIN_DESIGN`. Bila pemilik memilih jalan lebih cepat, slice yang boleh dikirim
sekarang adalah `MCU-S1`, `MCU-S3` (*walk-in*), `MCU-S4a`, `MCU-S6` (tanpa lampiran), `MCU-S7`, dan
`MCU-S8`, dengan `DEC-MCU-001` dan `DEC-MCU-004` dibawa sebagai penahan terbuka.

Yang dibawa pada handoff:

| Butir | Nilai |
| --- | --- |
| Identitas | Modul `mcu`, `MCU-BP-001`, assessment `MCU-RCG-001` r1 |
| Bukti | Decision log r2, capability map r1, `MCU-EVD-001` |
| Baseline | `ID-LAB-CAP-011`, `ID-LAB-CAP-012`, `ID-LAB-CAP-013`, `ID-LAB-REG-002` (`REFERENCE_ONLY`) |
| Keputusan terbuka | `DEC-MCU-001`..`005`, `MCU-OQ-002`, `MCU-OQ-003` |
| Koordinasi | `MCU-COORD-001`..`007`, `MCU-GOV-001` |
| Keluaran hilir yang diharapkan | `03-domain-architecture.md` — batas domain MCU terhadap Registrasi, Patient Management, Lab, Radiologi, Billing, Medical Fee, Kiosk, Rekam Medis; inti order paket dan siklus statusnya; dampak billing dan keselamatan klinis |
