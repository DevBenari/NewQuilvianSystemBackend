# Optik — Penilaian Kelengkapan Requirement

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Assessment ID | `OPT-RCG-001` |
| Revision | `2` — kesiapan diperbarui sesudah closure pass lanjutan (`OPT-DEC-042`..`046`) |
| Status | `draft` |
| Tanggal | 2026-11-03 |
| Masukan | [00-interview-decisions.md](00-interview-decisions.md) revision 2 (r1 penilaian), revision 3 (r2 penilaian) (`OPT-DEC-001`..`041`, `OPT-APR-001`..`008`, `OPT-AC-001`..`023`); [01-existing-capability-map.md](01-existing-capability-map.md) revision 1 (`OPT-CAP-01`..`31`) |
| SHA yang dirujuk | Backend `2a5094ca`, frontend `b6dd6c304`, sama dengan capability map |
| Baseline rujukan | `indonesia-hospital-domain-reference` **tidak** memiliki cakupan optik/refraksi (indeks domain tidak memuatnya). Tidak ada observasi `REFERENCE_ONLY` yang dipakai, dan ketiadaan cakupan ini tidak dianggap bukti apa pun |
| Pemilik keputusan | Yoga Aji Pratama (`OPT-DEC-002`) |
| Sifat | Read-only. Tidak ada entity, ERD, kontrak, atau source yang dibuat |

> **Cara membaca dokumen ini.** Penilaian ini menjawab satu pertanyaan: *apakah kebutuhan Optik
> sudah cukup jelas untuk mulai dirancang arsitektur domainnya?* Jawabannya diberikan **per
> bagian modul**. Bagian yang jelas boleh lanjut. Bagian yang masih punya pertanyaan penting
> ditahan, dan pertanyaannya diberi nomor `DEC-OPT-xxx` untuk dijawab pemilik lewat `/grill-me`.

---

## 0. Pembaruan revision 2 (2026-11-03)

Kelima pertanyaan pemblokir dijawab pemilik pada hari yang sama:

| Pertanyaan | Ditutup oleh | Inti jawaban |
| --- | --- | --- |
| `DEC-OPT-001` | `OPT-DEC-042` | Pemeriksaan refraksi ditagih sebagai tindakan bertarif saat disimpan final, jadi atau tidaknya pesanan; jasa medis ke Medical Fee |
| `DEC-OPT-002` | `OPT-DEC-043` | Satu kunjungan pemesanan terbuka sampai pesanan terakhir selesai; keluhan garansi membuat kunjungan baru |
| `DEC-OPT-003` | `OPT-DEC-044` | Minimum 50% dihitung dari bagian pasien sesudah tanggungan penjamin |
| `DEC-OPT-004` | `OPT-DEC-045` | Silinder notasi minus, resep plus ditransposisi, rentang isian ditetapkan, dan hanya Sp.M yang boleh melewati rentang |
| `DEC-OPT-005` | `OPT-DEC-046` | Semua lensa kontak wajib resep lensa kontak berlaku |

**Kesiapan revision 2:**

| Slice | Revision 1 | **Revision 2** |
| --- | --- | --- |
| `OPT-S1` Kunjungan & rujukan | `BUSINESS_DECISION_REQUIRED` | **`READY_FOR_DOMAIN_DESIGN`** |
| `OPT-S2` Pemeriksaan & resep | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** |
| `OPT-S3` Pesanan berpengerjaan | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** |
| `OPT-S4` Penjualan langsung | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** |
| `OPT-S5` Barang & stok | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` |
| `OPT-S6` Tagihan, uang muka, diskon | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`**, sekarang juga memuat tarif pemeriksaan |
| `OPT-S7` Penyerahan, garansi, remake | `PARTIALLY_READY` | **`READY_FOR_DOMAIN_DESIGN`** |
| `OPT-S8` Hasil ke Rekam Medis | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` |
| `OPT-S9` Laporan | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN` |
| `OPT-S10` Pengaturan | `READY_FOR_DOMAIN_DESIGN` | `READY_FOR_DOMAIN_DESIGN`, sekarang juga memuat rentang BC/DIA |
| `OPT-S11` Kacamata BPJS | di luar penilaian | tetap di luar penilaian (`OPT-DEC-030`) |

**Kesiapan modul revision 2:** `READY_FOR_DOMAIN_DESIGN` untuk `OPT-S1`..`S10`. Butir
`PROPOSED` di bagian 5 tetap terlihat sebagai usulan standar dan dikonfirmasi saat desain
atau screen brief. Persetujuan modul lain (`OPT-APR-001`..`011`) menahan task, bukan desain.

Bagian 1–9 di bawah adalah penilaian revision 1 dan dipertahankan sebagai histori.

---

## 1. Ringkasan (revision 1)

| No | Temuan | Akibat |
| ---: | --- | --- |
| 1 | Keputusan Optik sudah rapat: 41 keputusan, alur status disahkan, dan 23 kriteria uji | Sebagian besar bagian siap dirancang |
| 2 | **Belum diputuskan apakah pemeriksaan refraksi itu sendiri ditagih**, serta apakah ada jasa medis untuk RO/Sp.M | Bagian pemeriksaan dan tagihan ditahan (`DEC-OPT-001`) |
| 3 | **Belum diputuskan bagaimana kunjungan berlaku lintas hari.** Pesanan dibuat hari ini, sedangkan pengambilan 2–5 hari kemudian | Kunjungan, pesanan, tagihan, dan penyerahan ditahan (`DEC-OPT-002`) |
| 4 | Syarat uang muka untuk pasien **asuransi/perusahaan** hanya ditulis "mengikuti aturan penjaminnya" dan tidak dapat diuji | Jalur pesanan berpenjamin ditahan; pasien tunai boleh lanjut (`DEC-OPT-003`) |
| 5 | **Notasi silinder dan rentang isian resep** belum ditetapkan. Kesalahan tanda (minus/plus) menghasilkan kacamata yang salah | Bagian resep ditahan untuk aturan validasinya (`DEC-OPT-004`) |
| 6 | Belum jelas apakah **lensa kontak berukuran boleh dibeli tanpa resep** lewat penjualan langsung | Batas penjualan langsung ditahan (`DEC-OPT-005`) |
| 7 | Delapan persetujuan modul lain (`OPT-APR-001`..`008`) belum ada | **Tidak** menahan desain; hanya menahan task yang menyentuh modul itu |

---

## 2. Scope penilaian

| Slice | Isi | Keputusan sumber |
| --- | --- | --- |
| `OPT-S1` Kunjungan & rujukan | Pendaftaran kunjungan unit Optik, rujukan Poli Mata bertaut kunjungan asal | `OPT-DEC-006`, `032`, `037` |
| `OPT-S2` Pemeriksaan & resep | Refraksi, resep kacamata/lensa kontak, resep luar, revisi berversi, kunci resep, STR/SIP, usia resep | `OPT-DEC-005`, `007`, `011`, `012`, `017`, `022`, `023`, `039` |
| `OPT-S3` Pesanan berpengerjaan | Alur status, tujuan pengerjaan, syarat uang muka, batal, tidak diambil | `OPT-DEC-008`..`010`, `013`, `016`, `018`, `028`, `034`, `035`, `038` |
| `OPT-S4` Penjualan langsung | Barang tanpa resep, langsung lunas dan diserahkan | `OPT-DEC-020` |
| `OPT-S5` Barang & stok | Kategori dan atribut barang Optik, depo, potong dan kembali stok | `OPT-DEC-004`, `019`, `031`, `038` |
| `OPT-S6` Tagihan, uang muka, diskon | Sumber tagihan resmi, pembayaran sebagian, diskon berbatas | `OPT-DEC-021`, `027`..`029`, `034`, `035`, `040` |
| `OPT-S7` Penyerahan, garansi, remake | *Fitting*, lunas, masa garansi, keluhan, remake Rp 0 | `OPT-DEC-015`, `041` |
| `OPT-S8` Hasil ke Rekam Medis | Visus, refraksi, resep versi terakhir | `OPT-DEC-024` |
| `OPT-S9` Laporan | Empat laporan inti | `OPT-DEC-025` |
| `OPT-S10` Pengaturan | Angka bawaan yang diatur Admin Optik | `OPT-DEC-009`, `040`, `041` |
| `OPT-S11` Kacamata BPJS | — | **Di luar penilaian**: dipindah ke slice berikutnya (`OPT-DEC-030`) |

---

## 3. Bukti yang dipakai

| Wewenang | Bukti | Dipakai untuk |
| --- | --- | --- |
| 1 — Requirement eksplisit user/pemilik | Jawaban wawancara Yoga Aji Pratama, scope pass dan closure pass 2026-11-03 (`00-interview-decisions.md` r2) | Apa yang seharusnya dibangun |
| 2 — SOP/kebijakan RS | **Tidak ada** SOP Optik yang diserahkan | — |
| 5 — Baseline rujukan | Tidak ada cakupan optik | — |
| 6 — Implementasi V2 | `01-existing-capability-map.md` r1 | Apa yang sudah ada |
| 7 — Legacy V1 | Layar Optik v1 berisi data contoh (`OPT-CAP-30`) | Acuan tampilan saja; tidak dipakai sebagai requirement |

Ketiadaan SOP Optik berarti seluruh aturan bersumber dari jawaban pemilik. Itu sah sebagai
`CONFIRMED` karena pemilik berwenang (`OPT-DEC-002`). Kewenangan RO menurut Permenkes 19/2013
dicatat pemilik sebagai rujukan yang **perlu dikonfirmasi kebijakan RS** (`OPT-DEC-005`), jadi
tetap `PROPOSED` sebagai dasar hukum, walaupun aturan sistemnya `CONFIRMED`.

---

## 4. Temuan per dimensi

### 4.1 Matriks

Kode: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **X** = `CONFLICT`,
**–** = tidak berlaku secara material (alasan di 4.2). Huruf kecil di belakang merujuk butir
di bagian 5 atau 6.

| No | Dimensi | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 | S9 | S10 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 01 | Tujuan | C | C | C | C | C | C | C | C | C | C |
| 02 | Aktor | C | C | C | C | C | C | C | C | P (n9) | C |
| 03 | Pemicu/prasyarat | C | C | C | C | C | C | C | C | – | – |
| 04 | Alur utama | **M (002)** | C | C | C | C | C | **M (002)** | C | C | C |
| 05 | Alternatif/exception | C | C | P (n4) | **M (005)** | P (n4) | C | P (n5) | C | – | – |
| 06 | Data minimum | C | **M (004)**, P (n1) | C | C | P (n2) | C | C | C | P (n9) | C |
| 07 | Aturan/validation | C | **M (004)** | **M (003)** | **M (005)** | C | C | C | C | – | C |
| 08 | Status/perubahan | **M (002)** | C | C | C | – | C | C | – | – | – |
| 09 | Peran/authorization | C | C | C | C | C | C | C | P (n7) | P (n9) | C |
| 10 | Dependency antarmodul | C | C | C | C | C | C | C | C | C | – |
| 11 | Integrasi | – | – | P (n6) | – | – | C | – | C | – | – |
| 12 | Hasil akhir | C | C | C | C | C | C | C | C | C | C |
| 13 | Batal/koreksi | C | C | C | P (n3) | C | C | C | P (n7) | – | C |
| 14 | Audit/histori | C | C | P (n8) | P (n8) | C | C | P (n8) | – | – | C |
| 15 | Notifikasi | – | – | C | – | – | – | – | – | – | – |
| 16 | Billing/charge | **M (002)** | **M (001)** | C, **M (003)** | C | – | **M (001)** | C | – | – | – |
| 17 | Keselamatan klinis | – | **M (004)** | C | **M (005)** | – | – | C | C | – | – |
| 18 | Pelaporan/traceability | C | C | C | C | C | C | C | C | C | – |

### 4.2 Alasan dimensi yang tidak berlaku

| Dimensi | Slice | Alasan |
| --- | --- | --- |
| 03, 05, 07, 08, 13–17 | S9 Laporan | Laporan hanya membaca data yang sudah ada; tidak mengubah status, tagihan, atau data klinis |
| 08 | S5, S8 | Stok mengikuti lifecycle mutasi Farmasi; Rekam Medis hanya membaca |
| 11 | S1, S2, S4, S5, S7, S9, S10 | Tidak ada sistem atau perangkat eksternal di MVP. Autorefraktometer diisi manual karena integrasi alat tidak diminta |
| 15 | Selain S3 | Satu-satunya pemberitahuan adalah daftar "perlu dihubungi" (`OPT-DEC-018`); pesan otomatis di luar scope |
| 16 | S5, S8, S9, S10 | Stok tidak menagih; nilai persediaan milik Farmasi/Accounting |
| 17 | S1, S5, S6, S9, S10 | Tidak menyentuh ukuran lensa maupun identifikasi pasien secara klinis |

---

## 5. Butir yang tidak memblokir desain

| ID | Status | Dampak | Uraian | Usulan |
| --- | --- | --- | --- | --- |
| `n1` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Rincian isian pemeriksaan refraksi di luar resep: visus jauh/dekat, dengan/tanpa koreksi, hasil autorefraktometer, keratometri | Visus jauh dan dekat per mata, tanpa dan dengan koreksi; hasil autorefraktometer per mata; keratometri opsional. Dikonfirmasi pemilik saat screen brief |
| `n2` | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Daftar atribut terstruktur per kategori barang (`OPT-DEC-031`) | Frame: merk, warna, ukuran (lebar lensa-jembatan-gagang), bahan. Lensa: indeks, jenis (single vision/bifokal/progresif), lapisan. Lensa kontak: BC, DIA, masa pakai. Daftar ini dapat diperluas Admin Optik |
| `n3` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pembatalan penjualan langsung yang sudah diserahkan (retur barang) | Retur bukan bagian MVP. Pengembalian dana lewat Billing-Kasir dan stok kembali lewat koreksi Farmasi |
| `n4` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Stok frame habis di antara pesanan disimpan dan dikirim ke pengerjaan | Ketersediaan dicek saat simpan dan saat kirim, tanpa reservasi. Bila habis saat kirim, sistem menolak dengan pesan "Stok frame A di depo Optik tidak cukup". **Contoh:** stok 1, dua pesanan memakai frame yang sama; pesanan yang dikirim lebih dulu lolos, yang kedua ditolak |
| `n5` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Remake memakan stok baru tanpa pendapatan, dan cacat dari lab luar mungkin diklaim ke supplier | Remake memotong stok seperti pesanan biasa dengan tanda "remake". Klaim ke supplier di luar scope |
| `n6` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pengiriman ke lab luar: cukup tujuan dan perkiraan jadi, tanpa integrasi sistem lab | Dicatat manual. Biaya beli lensa custom ke supplier (hutang dagang) milik Finance/Farmasi, di luar scope |
| `n7` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Bila resep direvisi sesudah tampil di Rekam Medis | Rekam Medis selalu membaca versi terakhir. Persetujuan pemilik Rekam Medis di `OPT-APR-007` |
| `n8` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Jejak setiap perpindahan status pesanan | Setiap perpindahan menyimpan status asal, status tujuan, pelaku, waktu, dan alasan (wajib untuk batal dan tutup) |
| `n9` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Kolom laporan dan siapa yang boleh membuka | Kepala Optik dan Admin Optik membuka semua laporan; kolom rinci diusulkan di screen brief |
| `n10` | `CONFIRMED` (fakta audit) | `NON_BLOCKING_STANDARD` | Data unit, depo, profesi RO, dan kategori di basis data belum diperiksa (`OPT-UNK-001`) | Diperiksa saat task implementasi |
| `n11` | `CONFIRMED` | — | Delapan persetujuan modul lain (`OPT-APR-001`..`008`) | Desain boleh berjalan dengan asumsi persetujuan didapat. Task yang menyentuh modul tersebut tertahan sampai persetujuan tercatat |

---

## 6. Decision Log — pertanyaan yang memblokir

### `DEC-OPT-001` — Apakah pemeriksaan refraksi ditagih, dan apakah ada jasa medis?

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemeriksaan refraksi menghasilkan biaya sendiri (jasa pemeriksaan), dan apakah RO/Sp.M menerima jasa medis lewat modul Medical Fee? Apakah biaya itu berlaku juga bila pasien tidak jadi memesan kacamata? |
| Kemampuan terdampak | `OPT-S2`, `OPT-S6` |
| Bukti saat ini | Keputusan hanya mengatur tagihan **barang** (`OPT-DEC-021`, `029`). Billing punya sumber jasa konsultasi dokter (`ConsultationSourceContext`), dan MCU pernah memutuskan hal sejenis (`DEC-MCU-002`) |
| Usulan | Pemeriksaan refraksi ditagih sebagai satu tindakan dari master tarif saat pemeriksaan disimpan final, terlepas dari jadi atau tidaknya pesanan. Jasa medis pemeriksa diserahkan ke Medical Fee bila RS memberlakukannya |
| Dampak | Fakta tagihan baru dari pemeriksaan, titik pemicu tagihan, dan titik sentuh Medical Fee |
| Pemilik | Yoga Aji Pratama; pemilik Billing dan Medical Fee untuk titik sentuhnya |
| Status | `CLOSED` — dijawab `OPT-DEC-042` (2026-11-03) |
| Dampak domain | Penagihan pemeriksaan di S2/S6 ditahan. Struktur resep dan pesanan boleh dirancang |

### `DEC-OPT-002` — Kunjungan Optik yang berlangsung beberapa hari

| Field | Isi |
| --- | --- |
| Pertanyaan | Pesanan dibuat di kunjungan hari ini, tetapi kacamata diambil 2–5 hari kemudian. Apakah pengambilan memakai **kunjungan yang sama** (kunjungan tetap terbuka sampai diserahkan), atau dibuatkan **kunjungan pengambilan baru** yang tertaut ke pesanan? Kapan kunjungan Optik ditutup, dan apa yang terjadi pada tagihannya? |
| Kemampuan terdampak | `OPT-S1`, `OPT-S3`, `OPT-S6`, `OPT-S7` |
| Bukti saat ini | `OPT-DEC-006` mewajibkan kunjungan, tetapi tidak mengatur lintas hari. Tagihan Billing dikelola per kunjungan (`OPT-CAP-24`). Ada layanan latar yang menutup kunjungan kiosk otomatis (capability map MCU `MCU-CAP-07`), sehingga perilaku penutupan kunjungan rawat jalan perlu dipastikan |
| Usulan | Kunjungan pemesanan tetap terbuka sampai seluruh pesanannya `Diserahkan`, `Batal`, atau `Tidak Diambil`, lalu ditutup otomatis. Pengambilan tidak membuat kunjungan baru, sehingga tagihan tetap satu dan pelunasan masuk ke tagihan yang sama. **Contoh:** kunjungan 3 November dibiarkan terbuka, Ibu Sari mengambil pada 5 November dan melunasi di tagihan kunjungan 3 November, lalu kunjungan ditutup |
| Dampak | Lifecycle kunjungan, tagihan, piutang, laporan kunjungan, dan keluhan garansi (apakah keluhan membuat kunjungan baru) |
| Pemilik | Yoga Aji Pratama; pemilik Registrasi dan Billing untuk titik sentuh |
| Status | `CLOSED` — dijawab `OPT-DEC-043` (2026-11-03) |
| Dampak domain | Tautan pesanan ke kunjungan, gerbang lunas, dan penyerahan ditahan |

### `DEC-OPT-003` — Uang muka untuk pasien berpenjamin

| Field | Isi |
| --- | --- |
| Pertanyaan | Untuk pasien asuransi swasta atau perusahaan, kapan pesanan boleh masuk `Dalam Pengerjaan`? Apa arti "penjamin menyetujui" dalam bentuk yang bisa diuji? |
| Kemampuan terdampak | `OPT-S3` (jalur berpenjamin), `OPT-S6` |
| Bukti saat ini | `OPT-DEC-008` hanya menyebut "mengikuti aturan penjaminnya". Billing memecah tagihan menjadi tanggungan penjamin dan bagian pasien (`BillingCoverageAdapter`, `PatientAmount`) |
| Usulan | Minimum 50% dihitung dari **bagian yang dibayar pasien** sesudah tanggungan penjamin. Bila bagian pasien Rp 0, pesanan boleh langsung dikirim. **Contoh:** total Rp 1.200.000, penjamin perusahaan menanggung Rp 1.000.000, bagian pasien Rp 200.000, sehingga uang muka minimal Rp 100.000 |
| Dampak | Gerbang pengerjaan, konsekuensi billing, dan risiko lensa custom tak terbayar |
| Pemilik | Yoga Aji Pratama |
| Status | `CLOSED` — dijawab `OPT-DEC-044` (2026-11-03) |
| Dampak domain | Pasien tunai boleh dirancang penuh. Gerbang jalur berpenjamin ditahan |

### `DEC-OPT-004` — Notasi silinder dan rentang isian resep

| Field | Isi |
| --- | --- |
| Pertanyaan | Resep ditulis dengan notasi silinder minus, plus, atau keduanya (disimpan apa adanya beserta penandanya)? Berapa rentang dan kelipatan yang sah untuk SPH, CYL, AXIS, ADD, PD, BC, dan DIA? |
| Kemampuan terdampak | `OPT-S2`, juga pesanan ke lab (S3) |
| Bukti saat ini | Tidak ada. Contoh v1 memakai silinder minus (`-0.75`) tanpa aturan |
| Usulan | Sistem menyimpan notasi silinder minus. Resep luar bernotasi plus diubah (*transposisi*) saat disalin, dan aslinya tetap tercatat. Rentang usulan: SPH −20,00..+20,00 dan CYL −6,00..0,00 dengan kelipatan 0,25; AXIS 0..180 bilangan bulat, wajib bila CYL ≠ 0; ADD +0,75..+3,50; PD 50..80 mm. **Contoh transposisi:** +1,00 / +0,50 × 90 menjadi +1,50 / −0,50 × 180 |
| Dampak | Makna klinis resep, keselamatan pasien (lensa salah ukur), dan kontrak data ke lab |
| Pemilik | Yoga Aji Pratama, dengan masukan RO/Sp.M |
| Status | `CLOSED` — dijawab `OPT-DEC-045` (2026-11-03) |
| Dampak domain | Aturan validasi resep ditahan. Struktur resep berversi boleh dirancang |

### `DEC-OPT-005` — Lensa kontak berukuran lewat penjualan langsung

| Field | Isi |
| --- | --- |
| Pertanyaan | Bolehkah lensa kontak berukuran (minus/plus) dibeli lewat penjualan langsung tanpa resep, misalnya pembelian ulang merk yang sama? Bagaimana dengan lensa kontak tanpa ukuran (kosmetik)? |
| Kemampuan terdampak | `OPT-S4`, `OPT-S2` |
| Bukti saat ini | `OPT-DEC-020` hanya menyebut cairan, kacamata baca jadi, kacamata hitam, dan aksesoris sebagai barang tanpa resep. `OPT-DEC-022` mewajibkan resep lensa kontak, tetapi tidak menyebut pembelian ulang |
| Usulan | Lensa kontak apa pun (termasuk kosmetik tanpa ukuran) wajib ditautkan ke resep lensa kontak yang masih berlaku (≤ 12 bulan, `OPT-DEC-040`). Pembelian ulang memakai resep yang sama selama masih berlaku |
| Dampak | Keselamatan klinis (lensa kontak tanpa evaluasi berisiko infeksi kornea) dan batas penjualan langsung |
| Pemilik | Yoga Aji Pratama |
| Status | `CLOSED` — dijawab `OPT-DEC-046` (2026-11-03) |
| Dampak domain | Batas barang "tanpa resep" pada S4 ditahan untuk kategori lensa kontak |

---

## 7. Kesiapan per slice

| Slice | Kesiapan | Alasan |
| --- | --- | --- |
| `OPT-S1` Kunjungan & rujukan | `BUSINESS_DECISION_REQUIRED` | `DEC-OPT-002` |
| `OPT-S2` Pemeriksaan & resep | `PARTIALLY_READY` | Struktur resep berversi, kunci, STR/SIP, dan resep luar siap. Validasi isian ditahan `DEC-OPT-004`, tagihan pemeriksaan ditahan `DEC-OPT-001` |
| `OPT-S3` Pesanan berpengerjaan | `PARTIALLY_READY` | Alur status, batal, dan tidak diambil siap untuk pasien tunai. Jalur berpenjamin ditahan `DEC-OPT-003`; tautan kunjungan ditahan `DEC-OPT-002` |
| `OPT-S4` Penjualan langsung | `PARTIALLY_READY` | Siap kecuali kategori lensa kontak (`DEC-OPT-005`) |
| `OPT-S5` Barang & stok | `READY_FOR_DOMAIN_DESIGN` | Menunggu `OPT-APR-005` hanya untuk task |
| `OPT-S6` Tagihan, uang muka, diskon | `PARTIALLY_READY` | Tagihan barang, pembayaran sebagian, dan diskon siap. Tagihan pemeriksaan ditahan `DEC-OPT-001`; berpenjamin ditahan `DEC-OPT-003` |
| `OPT-S7` Penyerahan, garansi, remake | `PARTIALLY_READY` | Garansi dan remake siap. Kunjungan saat pengambilan dan saat keluhan ditahan `DEC-OPT-002` |
| `OPT-S8` Hasil ke Rekam Medis | `READY_FOR_DOMAIN_DESIGN` | Menunggu `OPT-APR-007` hanya untuk task |
| `OPT-S9` Laporan | `READY_FOR_DOMAIN_DESIGN` | Kolom rinci di screen brief |
| `OPT-S10` Pengaturan | `READY_FOR_DOMAIN_DESIGN` | — |

**Kesiapan modul secara keseluruhan:** `PARTIALLY_READY`. `DEC-OPT-002` adalah blocker terluas
karena menyentuh empat slice.

---

## 8. Yang boleh berjalan dan yang harus berhenti

| Boleh berjalan ke arsitektur domain | Harus berhenti sampai keputusan ada |
| --- | --- |
| Master barang Optik dan atributnya, depo, dan potong/kembali stok (S5) | Lifecycle kunjungan Optik lintas hari, termasuk penutupannya (`DEC-OPT-002`) |
| Struktur pemeriksaan dan resep berversi, kunci resep, gerbang STR/SIP, resep luar, dan usia resep (S2 tanpa validasi isian) | Aturan validasi isian resep dan notasi silinder (`DEC-OPT-004`) |
| Lifecycle pesanan tunai, batal, tidak diambil, dan lensa stok/custom (S3 tunai) | Gerbang pengerjaan untuk pasien berpenjamin (`DEC-OPT-003`) |
| Sumber tagihan barang, pembayaran sebagian, dan diskon (S6 barang) | Tagihan pemeriksaan refraksi dan jasa medis (`DEC-OPT-001`) |
| Garansi dan remake (S7 tanpa aturan kunjungan) | Lensa kontak di penjualan langsung (`DEC-OPT-005`) |
| Pembaca Rekam Medis (S8), laporan (S9), pengaturan (S10) | — |

**Ketergantungan:** S3, S6, dan S7 bergantung pada keputusan kunjungan S1 (`DEC-OPT-002`).
Bila arsitektur domain dimulai sebelum `DEC-OPT-002` ditutup, tautan pesanan–kunjungan–tagihan
harus dibiarkan terbuka dan tidak boleh diasumsikan.

---

## 9. Handoff

| Butir | Nilai |
| --- | --- |
| Skill berikutnya yang disarankan | (revision 2) `/hospital-domain-architect` untuk `OPT-S1`..`S10`. (revision 1: `/grill-me` untuk `DEC-OPT-001`..`005`, sudah dijalankan) |
| Sesudah itu | `/hospital-domain-architect` dengan seluruh slice `READY_FOR_DOMAIN_DESIGN` |
| Bila langsung ke arsitektur sekarang | Hanya S5, S8, S9, S10, ditambah bagian siap dari S2/S3/S4/S6/S7 di bagian 8 |
| Yang dipertahankan dalam handoff | `OPT-BP-001`, `OPT-RCG-001` r2, `OPT-DEC-001`..`046`, `DEC-OPT-001`..`005` (ditutup), `OPT-APR-001`..`011`, `OPT-CAP-01`..`31`, SHA backend `2a5094ca`, frontend `b6dd6c304` |
| Baseline yang dipakai | Tidak ada (`indonesia-hospital-domain-reference` tanpa cakupan optik) |
