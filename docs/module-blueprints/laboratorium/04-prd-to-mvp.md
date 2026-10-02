# Laboratorium — PRD ke MVP

## 1. Identitas Dokumen

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `10` — bagian 23, penyelesaian order, hasil resmi, dan label keadaan (`LAB-DEC-154`..`LAB-DEC-156`), 2026-09-25 — kontrak `r36`/`r14`/`r7` disetujui hari yang sama. Sebelumnya `9` — bagian 22, `EPIC-LAB-16` validasi dan rilis Mikrobiologi, 2026-09-25 — kontraknya disetujui hari yang sama. Sebelumnya `8` — bagian 21, `EPIC-LAB-15` validasi dan rilis Patologi Klinik, 2026-09-25 — kontraknya disetujui hari yang sama. Sebelumnya `7` — bagian 20, `EPIC-LAB-14`, 2026-09-24 |
| Status | `draft` |
| Scope tambahan revision 4 | **`EPIC-LAB-11` Penerimaan Sampling/Specimen** dan gelombang `MVP-5` — lihat bagian 16 |
| Scope tambahan revision 5 | **`EPIC-LAB-12` Konfirmasi Pesanan dan Pembatalan Beralasan** dan gelombang `MVP-5c` — lihat bagian 17. Ditambahkan 2026-09-15 dari rekonsiliasi bukti putaran 2 |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| `approved_by` / `approved_at` | **belum** — approval adalah tindakan manusia |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Masukan | Decisions rev 20; capability map rev 2; `LAB-RCG-001` rev 5; `LAB-DA-001` rev 4; `LAB-REC-001` rev 2; seluruh kontrak `LAB-*-v1` |

> **Dokumen ini menurunkan, tidak menciptakan.** Setiap entity, status, kewenangan, dan endpoint
> yang disebut di sini sudah tercatat lebih dulu pada `02-backend-architecture.md`, `erd/`, atau
> `contracts/`. Tidak ada satu pun yang lahir dari nama epic.

---

## 2. Ringkasan Eksekutif

Modul Laboratorium sudah punya separuh perjalanan yang bekerja di backend: pesanan dokter,
siklus hidup sampel, dan penyerahan fakta tagihan ke Billing — seluruhnya terbukti oleh 31
pengujian otomatis. Tetapi modul itu **belum pernah dipakai satu orang pun**, karena tidak ada
satu layar pun yang dibangun untuknya.

MVP ini menutup tiga hal yang membuat modul belum berguna:

1. **Fondasi penilaian hasil belum ada** — tidak ada tempat menyimpan satuan, batas normal, dan
   batas kritis. Tanpa itu, sistem tidak akan pernah tahu sebuah angka berbahaya atau tidak.
2. **Wadah fisik dan pemeriksaan masih menyatu** — sehingga dua pemeriksaan dari satu tabung
   memaksa dua barcode, dan sistem mengizinkan menolak sebagian tabung, sesuatu yang mustahil
   secara fisik.
3. **Pekerjaan mendesak tidak dapat didahulukan** — tidak ada penanda cito dan tidak ada daftar
   kerja.

Yang **tidak** ditutup MVP ini: pengisian dan validasi hasil. Bagian itu menunggu tanda tangan
klinis yang belum turun.

---

## 3. Masalah Produk

| Masalah | Bukti | Akibat bagi rumah sakit |
|---|---|---|
| Petugas laboratorium tidak punya layar apa pun | `01-existing-capability-map.md#CAP-21` | Alur yang sudah dibangun tidak dapat dipakai; pekerjaan lab tetap manual |
| Batas normal dan batas kritis tidak punya tempat | `#CAP-07` | Hasil tidak dapat dinilai sistem; nilai berbahaya tidak terdeteksi |
| Wadah dan pemeriksaan menyatu | `03-domain-architecture.md#DEC-LAB-008` | Risiko salah label, dan penolakan sebagian yang tidak masuk akal |
| Pesanan mendesak tidak dapat didahulukan | Tidak ada kolom kesegeraan pada `LabOrder@c87d9c0` | Pemeriksaan IGD mengantre di belakang rawat jalan rutin |
| Daftar alasan penolakan tidak dapat dikelola | `#CAP-05` | Petugas memilih "lainnya" sehingga data penolakan kehilangan makna |

---

## 4. Visi Produk

Laboratorium memiliki satu tempat kerja tunggal yang mencatat perjalanan setiap pemeriksaan
dengan jujur — siapa mengambil, siapa menyatakan layak, kapan, dan atas dasar apa — sekaligus
menyerahkan fakta yang tepat kepada Billing tanpa pernah menyentuh angka uang.

---

## 5. Batas MVP

> **Diperluas pada revision 2.** Batas lama dimulai dari "dokter membuat pesanan", yang
> mengandaikan pasien sudah terdaftar. Setelah `LAB-DEC-032` dan `LAB-DEC-035`, MVP dimulai
> lebih hulu: dari pasien yang datang sendiri ke laboratorium.

| Aspek | Isi |
|---|---|
| **Titik mulai** | Pasien tiba di laboratorium — sudah terdaftar, datang langsung, atau dikirim institusi luar |
| **Titik akhir** | Wadah dinyatakan layak atau ditolak, fakta kelayakan tagih tersampaikan ke Billing, dan pekerjaan terlihat pada daftar pantau disiplinnya |
| **Yang tidak termasuk** | Pengisian hasil, validasi, rilis, nilai kritis, koreksi hasil, pemberitahuan, pendaftaran rekam medis |

### Kenapa perluasan ini penting

| Batas lama | Batas baru |
|---|---|
| Pasien datang langsung **tidak dapat dilayani** — harus lewat loket dulu | Petugas lab mendaftarkannya sendiri dari layar Laboratorium |
| Harga pemeriksaan **tidak terlihat** saat memesan | Harga, subtotal, total, dan status cakupan penjamin terlihat sebelum pemeriksaan dikerjakan |
| Satu daftar pesanan untuk semua | Tiga daftar pantau sejajar sesuai cara kerja laboratorium |

Dengan ketiganya, MVP menjadi modul yang **dapat dipakai petugas dari layar pertama**, bukan
potongan alur yang berhenti di tengah.

### Pelaku sasaran

| Pelaku | Yang dilakukan pada MVP ini |
|---|---|
| Dokter pemesan | Membuat pesanan, menambah pemeriksaan, menandai cito |
| Perawat atau flebotomis | Mencatat pengambilan wadah |
| Petugas penerimaan laboratorium | Mencatat wadah tiba |
| Petugas berwenang menetapkan kelayakan | Menyatakan layak, menolak, meminta ambil ulang |
| Kepala instalasi laboratorium | Mengelola batas nilai dan alasan penolakan, memantau keterlambatan cito |
| Pemegang kewenangan persetujuan batas kritis | Menyetujui atau menolak perubahan batas kritis |
| Petugas Billing | Menerima fakta, tanpa perubahan cara kerja |

---

## 6. Kemampuan `MUST HAVE`

| Kemampuan | ID capability map | Disposisi |
|---|---|---|
| Pesanan laboratorium beserta siklus hidupnya | `CAP-01` | `EXTEND` |
| Penandaan cito dan batas waktunya | `CAP-01` | `MISSING / NEW` |
| Siklus hidup wadah fisik | `CAP-02` | `EXTEND` |
| Pemeriksaan terpesan sebagai satuan tersendiri | `CAP-02` | `MISSING / NEW` |
| Riwayat perpindahan status | `CAP-04` | `EXISTING / REUSE` |
| Alasan penolakan sampel beserta pengelolaannya | `CAP-05` | `EXTEND` |
| Batas nilai, batas kritis, dan batas waktu cito | `CAP-07` | `MISSING / NEW` |
| Persetujuan klinis atas perubahan batas kritis | `CAP-07` | `MISSING / NEW` |
| Daftar kerja dan pemantauan keterlambatan cito | `CAP-02`, `CAP-01` | `MISSING / NEW` |
| Fakta kelayakan tagih ke Billing | `CAP-11` | `EXISTING / REUSE`, satuannya `EXTEND` |
| Batas kewenangan finansial | `CAP-12` | `EXISTING / REUSE` |
| Kewenangan per aksi | `CAP-13`, `CAP-14` | `EXISTING / REUSE` |
| Perlindungan dua petugas bertindak bersamaan | `CAP-17` | `EXISTING / REUSE` |
| Layar Laboratorium untuk seluruh kemampuan di atas | `CAP-21` | `MISSING / NEW` |

---

## 7. Kemampuan yang Ditunda

Setiap penundaan menyebut **alasan bersebab** dan **penggantinya selama MVP berjalan**.

| Kemampuan | Alasan penundaan | Pengganti selama MVP |
|---|---|---|
| Pengisian dan validasi hasil (`S4`) | `LAB-SIGN-001` — `LAB-DEC-011` mensyaratkan tanda tangan klinis sebelum desain final | Hasil tetap dicatat di luar sistem seperti sekarang. Sistem sudah menyimpan seluruh riwayat sampai wadah dinyatakan layak, sehingga penelusuran tidak mundur |
| Nilai kritis dan pelaporannya (`S5`) | `LAB-SIGN-001`. Seluk-beluk alurnya — ambang, batas waktu tanggap, eskalasi — masih menunggu `LAB-P0-004` dan `LAB-OPEN-014` | Pelaporan lisan berjalan seperti sekarang. Batas kritis **sudah tersimpan** pada MVP ini, sehingga saat slice dibuka tidak ada pekerjaan data yang tertinggal |
| Koreksi hasil setelah rilis (`S6`) | `LAB-SIGN-001` | Belum ada hasil yang dapat dikoreksi, jadi tidak ada kemampuan yang hilang |
| Pemberitahuan tersimpan (`S8`) | `LAB-COORD-001` — kepemilikannya ada di platform, bukan Laboratorium | Kepala instalasi memakai daftar pantau keterlambatan cito yang **sudah ada** pada MVP ini |
| Pendaftaran hasil ke rekam medis (`S9`) | `LAB-COORD-002` | Belum ada hasil yang perlu didaftarkan |
| Penyuntingan pesanan oleh dokter (`S1b`) | `LAB-AMD-001` — menyentuh keputusan terkunci milik blueprint `rawat-jalan` | Dokter yang salah pesan membatalkan lalu membuat pesanan baru. Aman secara uang karena pembatalan sebelum wadah layak tidak menimbulkan tagihan |
| Sisa katalog laboratorium: jenis sampel, wadah, volume minimal, metode, panel | `LAB-DEC-001` menundanya ke Rilis 2 | Katalog memakai `MstProcedure` yang sudah ada |
| Sambungan otomatis ke alat laboratorium | `LAB-DEC-005` | Belum ada hasil untuk dikirim alat, jadi belum ada yang hilang. Perhatikan `LAB-RISK-001` |

> **Catatan penahan, diperbarui 2026-09-09.** `LAB-COORD-001` dan `LAB-COORD-002` berstatus
> `closed` sejak 2026-09-01 lewat `LAB-REQ-001` butir 7 dan 8. Keduanya tidak lagi menahan `S5`
> maupun `S6`, dan versi terdahulu tabel ini keliru mencantumkannya di sana. `S8` dan `S9` tetap
> tertunda, tetapi yang ditunggu adalah **pengerjaan** kemampuan itu oleh modul pemiliknya —
> platform dan `rekam-medis` — bukan lagi kesepakatannya.
>
> Akibatnya `LAB-SIGN-001` kini satu-satunya penahan `S4`, `S4b`, `S4c`, `S5`, dan `S6`.
> Permintaan tanda tangannya diajukan 2026-09-09 sebagai `LAB-REQ-004`.

> ### ✅ Diperbarui 2026-09-17 — `LAB-SIGN-001` ditutup
>
> `LAB-DEC-079`: ketiga keputusan klinis ditandatangani **apa adanya**, **per disiplin**, oleh
> `DR-LAB-001` (Patologi Klinik), `DR-LAB-002` (Mikrobiologi Klinik), dan `DR-LAB-003`
> (Patologi Anatomi), sesudah `LAB-DEC-078` menetapkan ketiganya pada hari yang sama.
>
> **Tabel di atas tetap berlaku, dan itu yang perlu dibaca tepat.** Kelima slice **belum**
> pindah ke MVP Rilis 1. Yang berubah adalah izin merancangnya; arsitektur domain, amandemen
> kontrak, dan task-nya **nol ada**. Penempatan rilisnya keputusan pemilik modul.
>
> **Dan tiga baris tabel itu punya sisa penahan masing-masing di luar `LAB-SIGN-001`:** `S5`
> masih menunggu `LAB-P0-004` dan `LAB-OPEN-014` — persis seperti yang sudah tertulis di
> barisnya sendiri — `S6` menunggu `LAB-P0-003`, dan `S4b` menunggu `LAB-OPEN-017`. Ditambah
> `LAB-OPEN-034` yang lahir dari bentuk per-disiplin tanda tangannya.

---

## 8. Alur Bisnis Target

1. **Tujuan:** rumah sakit memperoleh catatan tepercaya bahwa sebuah pemeriksaan sah dikerjakan
   dan sah ditagihkan.
2. **Pelaku:** dokter pemesan, perawat, petugas penerimaan, petugas berwenang menetapkan
   kelayakan.
3. **Pemicu:** dokter memutuskan pasien perlu diperiksa laboratorium.
4. **Prasyarat:** pasien punya kunjungan aktif; jenis pemeriksaan ada di katalog dan bertarif;
   batas nilainya sudah diatur.
5. **Langkah utama:**
   1. Dokter membuat pesanan berisi satu atau beberapa pemeriksaan.
   2. Bila mendesak, dokter menandainya cito.
   3. Petugas merencanakan wadah, dan menentukan pemeriksaan mana yang ditopang wadah itu.
   4. Perawat mengambil bahan, memindai barcode wadah.
   5. Wadah tiba di laboratorium dan dicatat penerimaannya.
   6. Petugas berwenang memeriksa kelayakan wadah, lalu menyatakannya layak atau menolaknya.
   7. Bila layak, fakta kelayakan tagih terbit untuk **setiap** pemeriksaan pada wadah itu.
6. **Aturan bisnis:** BR-01 sampai BR-20 pada `00-interview-decisions.md`.
7. **Perubahan status:** `contracts/state-transition-matrix.md`.
8. **Jalur tidak normal:** penolakan, ambil ulang, penahanan, pembatalan — seluruhnya tercatat.
9. **Hasil akhir:** pekerjaan terlihat pada daftar kerja; Billing menerima fakta; seluruh
   perjalanan dapat ditelusuri.

---

## 9. Epic dan Functional Requirement

### `EPIC-LAB-01` — Penandaan Cito dan Batas Waktunya

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-01.1` | Dokter pemesan dapat menandai pesanannya sebagai cito saat membuat maupun sesudahnya | `MISSING / NEW` |
| `FR-01.2` | Hanya dokter pemesan pesanan itu yang boleh menandainya | `MISSING / NEW` |
| `FR-01.3` | Penandaan menyimpan waktu dan pelakunya, serta menghasilkan riwayat | `EXTEND` |
| `FR-01.4` | Setiap jenis pemeriksaan dapat memiliki batas waktu penyelesaian cito | `MISSING / NEW` |

### `EPIC-LAB-02` — Pemisahan Wadah Fisik dan Pemeriksaan Terpesan

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-02.1` | Satu wadah memiliki satu barcode dan dapat menopang beberapa pemeriksaan | `EXTEND` |
| `FR-02.2` | Keputusan layak atau tolak diambil atas wadah, bukan atas pemeriksaan | `EXTEND` |
| `FR-02.3` | Menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya | `MISSING / NEW` |
| `FR-02.4` | Salinan tarif tersimpan pada pemeriksaan, bukan pada wadah | `EXTEND` |
| `FR-02.5` | Ambil ulang memindahkan seluruh pemeriksaan ke wadah pengganti | `EXTEND` |
| `FR-02.6` | Data lama dipindahkan tanpa memutus tautan tagihan yang sudah ada | `MISSING / NEW` |

### `EPIC-LAB-03` — Batas Nilai dan Persetujuan Klinis

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-03.1` | Satu jenis pemeriksaan dapat memiliki beberapa baris batas menurut jenis kelamin dan kelompok umur | `MISSING / NEW` |
| `FR-03.2` | Batas nilai mendukung dua bentuk hasil: angka dan pilihan terbatas | `MISSING / NEW` |
| `FR-03.3` | Batas normal dapat diubah kepala instalasi dan langsung berlaku | `MISSING / NEW` |
| `FR-03.4` | Batas kritis hanya berubah lewat pengajuan yang disetujui pihak klinis | `MISSING / NEW` |
| `FR-03.5` | Seluruh perubahan batas menghasilkan riwayat permanen | `MISSING / NEW` |
| `FR-03.6` | `MstProcedure` tidak bertambah satu kolom pun | `MISSING / NEW` |

### `EPIC-LAB-04` — Daftar Kerja dan Pemantauan Keterlambatan

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-04.1` | Daftar kerja menampilkan pekerjaan yang belum selesai, cito di urutan atas | `MISSING / NEW` |
| `FR-04.2` | Daftar pantau menampilkan pesanan cito yang melewati batas waktunya | `MISSING / NEW` |
| `FR-04.3` | Keterlambatan dihitung sejak wadah dinyatakan layak | `MISSING / NEW` |
| `FR-04.4` | Daftar kerja diturunkan, tidak disimpan sebagai tabel | `MISSING / NEW` |

### `EPIC-LAB-05` — Fakta Kelayakan Tagih per Pemeriksaan

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-05.1` | Satu wadah yang dinyatakan layak menerbitkan fakta sebanyak pemeriksaan yang ditopangnya | `EXTEND` |
| `FR-05.2` | Fakta menunjuk identitas pemeriksaan, bukan wadah | `EXTEND` |
| `FR-05.3` | Penetapan layak berulang tidak menggandakan fakta | `EXISTING / REUSE` |
| `FR-05.4` | Laboratorium tidak memiliki kolom maupun tindakan finansial | `EXISTING / REUSE` |

### `EPIC-LAB-06` — Pengelolaan Alasan Penolakan

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-06.1` | Kepala instalasi dapat menambah, mengubah, mengurutkan, dan menonaktifkan alasan | `MISSING / NEW` |
| `FR-06.2` | Penanda kesalahan internal dan penanda wajib catatan hanya dapat disetel administrator sistem | `MISSING / NEW` |
| `FR-06.3` | Data awal alasan penolakan tersedia sebelum modul dipakai | `MISSING / NEW` |

### `EPIC-LAB-08` — Pendaftaran Pasien Datang Langsung dan Rujukan Luar

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-08.1` | Petugas mencari pasien terdaftar sebelum mendaftarkan yang baru | `MISSING / NEW` |
| `FR-08.2` | Layar pendaftaran berada di modul Laboratorium; kunjungan dibuat Registrasi | `MISSING / NEW` |
| `FR-08.3` | Pendaftaran rujukan luar menyimpan penunjuk instansi dan dokter perujuk, bukan teks bebas | `MISSING / NEW` |
| `FR-08.4` | Permintaan pendaftaran bersifat idempoten; kirim ganda tidak menghasilkan dua kunjungan | `MISSING / NEW` |
| `FR-08.5` | Kegagalan Registrasi diteruskan apa adanya; tidak ada data setengah jadi tersimpan | `MISSING / NEW` |

### `EPIC-LAB-09` — Katalog, Harga, dan Cakupan Penjamin

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-09.1` | Katalog pemeriksaan laboratorium disaring per disiplin | `EXISTING / REUSE` atas `MstProcedure`, ditambah kolom disiplin |
| `FR-09.2` | Harga satuan, subtotal, dan total tampil saat memesan | `EXISTING / REUSE` atas `MstTariff` |
| `FR-09.3` | Status cakupan penjamin tampil per pemeriksaan | `EXISTING / REUSE` atas `MstInsuranceTariff` |
| `FR-09.4` | Seluruh jalur katalog dan tarif **baca saja** | `MISSING / NEW` |
| `FR-09.5` | Pemeriksaan yang disiplinnya tidak sesuai pesanan ditolak | `MISSING / NEW` |

### `EPIC-LAB-10` — Monitoring per Disiplin

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-10.1` | Tiga daftar pantau sejajar: Patologi Klinik, Patologi Anatomi, Mikrobiologi | `MISSING / NEW` |
| `FR-10.2` | Penyaring sama pada ketiganya: pasien, periode, unit, penjamin, status, penanda cito | `MISSING / NEW` |
| `FR-10.3` | Pesanan menyimpan disiplinnya dan tidak dapat berpindah setelah dibuat | `EXTEND` |

### `EPIC-LAB-07` — Layar Laboratorium

| FR | Kebutuhan | Disposisi |
|---|---|---|
| `FR-07.1` | Layar pesanan beserta penandaan cito | `MISSING / NEW` |
| `FR-07.2` | Layar wadah dan pemeriksaan, menampilkan seluruh pemeriksaan sebelum penolakan | `MISSING / NEW` |
| `FR-07.3` | Layar daftar kerja dan daftar pantau keterlambatan | `MISSING / NEW` |
| `FR-07.4` | Layar batas nilai dengan jalur pengajuan untuk batas kritis | `MISSING / NEW` |
| `FR-07.5` | Layar alasan penolakan dengan kolom terkunci yang terlihat | `MISSING / NEW` |

**Tidak ada epic berstatus `OPEN DECISION`.** Seluruh epic di atas berdiri di atas keputusan
yang sudah `approved`, dan seluruh slicenya sudah `DOMAIN_ARCHITECTURE_READY`.

---

## 10. Sasaran Teknis

| Aspek | Rujukan |
|---|---|
| Model status | `contracts/state-transition-matrix.md` |
| Sasaran arsitektur | `02-backend-architecture.md`, `03-frontend-architecture.md` |
| Sasaran kemampuan API | `contracts/api-contract.md` — `LAB-API-v1` |
| Matriks kewenangan | `contracts/permission-audit-matrix.md` — `LAB-PERM-v1` |
| Validasi | `contracts/validation-matrix.md` — `LAB-VAL-v1` |
| Bentuk data | `erd/data-dictionary.md` |

---

## 11. Batas Integrasi dan Billing

| Batas | Ketentuan |
|---|---|
| Laboratorium ke Billing | Satu arah, berisi fakta, idempoten. `contracts/integration-contract.md#INT-01` |
| Wewenang finansial | Laboratorium **tidak punya** `Paid`, penyelesaian pembayaran, persetujuan penjamin, void, refund, maupun pembalikan |
| Ambil ulang karena kesalahan internal | Tidak menambah tanggungan pasien secara otomatis |
| Sistem luar | Tidak ada. `LAB-DEC-005` menetapkan tidak ada sambungan alat pada rilis ini |

### Guardrail regulasi dan keselamatan

| Guardrail | Wujudnya |
|---|---|
| Penelusuran sampel | Setiap wadah punya barcode unik dan tautan pasti ke pesanan, kunjungan, dan pasien |
| Barcode tidak memuat identitas pasien | Sudah dijaga pengujian yang ada |
| Penolakan beralasan terkendali | Alasan bebas tidak diterima |
| Batas kritis terlindungi | Perubahannya memerlukan persetujuan klinis dan seluruhnya berriwayat |
| Riwayat tidak dapat diubah | Seluruh perpindahan status tersimpan permanen |

### Kebutuhan non-fungsional

| Aspek | Ketentuan |
|---|---|
| Konkurensi | Dua petugas bertindak bersamaan atas objek yang sama, hanya satu berhasil |
| Idempotensi | Penetapan layak berulang tidak menggandakan fakta |
| Jejak audit | Setiap perpindahan material menghasilkan satu baris permanen |
| Privasi | Kolom bertanda sensitif tidak masuk logger |
| Penghapusan | Penandaan `IsDelete`, bukan penghapusan baris |

---

## 12. Skenario UAT

Setiap epic `MUST HAVE` memiliki sekurang-kurangnya satu jalur berhasil dan satu jalur gagal.

| Epic | Jalur | Skenario | Hasil yang diharapkan |
|---|---|---|---|
| `EPIC-LAB-01` | Berhasil | dr. Rina menandai pesanannya cito | Pesanan bertanda cito; waktu dan pelaku tercatat |
| `EPIC-LAB-01` | **Gagal** | dr. Budi menandai cito pesanan milik dr. Rina | Ditolak; pesan menyebut hanya dokter pemesan yang boleh |
| `EPIC-LAB-02` | Berhasil | Satu tabung serum menopang Fungsi hati dan Fungsi ginjal, dinyatakan layak | Satu barcode; dua pemeriksaan menjadi layak tagih |
| `EPIC-LAB-02` | **Gagal** | Petugas menolak Fungsi hati saja pada tabung berisi dua pemeriksaan | Ditolak; pesan menjelaskan penolakan berlaku untuk seluruh pemeriksaan pada wadah itu |
| `EPIC-LAB-03` | Berhasil | Kepala instalasi membuat tiga baris batas Hemoglobin: pria dewasa, wanita dewasa, anak | Ketiganya tersimpan dan berlaku bersamaan |
| `EPIC-LAB-03` | **Gagal** | Kepala instalasi mengubah batas kritis Kalium dari 6,0 menjadi 8,0 lewat simpan biasa | Ditolak; batas tetap 6,0; diarahkan ke jalur pengajuan |
| `EPIC-LAB-04` | Berhasil | Pesanan cito IGD masuk saat 14 pesanan rutin menunggu | Pesanan cito berada di urutan pertama daftar kerja |
| `EPIC-LAB-04` | **Gagal** | Pesanan cito Kalium berbatas 60 menit belum selesai setelah 80 menit | Muncul pada daftar pantau keterlambatan dengan kelebihan 20 menit |
| `EPIC-LAB-05` | Berhasil | Wadah dua pemeriksaan Rp150.000 dan Rp120.000 dinyatakan layak | Dua fakta terbit dengan salinan tarif masing-masing; total rujukan Rp270.000 |
| `EPIC-LAB-05` | **Gagal** | Petugas menekan tombol menyatakan layak dua kali | Jumlah fakta tetap dua, bukan empat |
| `EPIC-LAB-06` | Berhasil | Kepala instalasi menambah alasan "Sampel tidak diberi label" | Alasan tersimpan dan langsung dapat dipakai |
| `EPIC-LAB-06` | **Gagal** | Kepala instalasi mengubah penanda kesalahan internal | Ditolak; pesan menjelaskan penanda itu menentukan siapa menanggung biaya |
| `EPIC-LAB-07` | Berhasil | Petugas membuka layar penolakan wadah berisi dua pemeriksaan | Kedua pemeriksaan terlihat, dan peringatan muncul sebelum penolakan dikonfirmasi |
| `EPIC-LAB-07` | **Gagal** | Pengguna tanpa kewenangan membuka layar batas nilai | Tombol tindakan tersembunyi atau nonaktif, bukan gagal saat ditekan |

---

## 13. Definition of Done

Setiap butir dapat dijawab "ya" atau "belum" beserta buktinya.

| No | Butir | Bukti yang diminta |
|---:|---|---|
| 1 | Seluruh FR pada epic `MUST HAVE` terpenuhi | Daftar FR dengan tautan ke commit atau berkas |
| 2 | Seluruh acceptance criteria AC-10 sampai AC-13, AC-17, AC-18, AC-24 sampai AC-26, AC-28, AC-33 sampai AC-38 lulus | Laporan hasil pengujian |
| 3 | Tiga puluh satu pengujian yang sudah ada tetap lulus setelah penyesuaian | Keluaran test runner |
| 4 | Setiap jalur gagal pada bagian 12 benar-benar ditolak sistem | Laporan hasil pengujian |
| 5 | `LAB-OPEN-012` sudah dijawab sebelum migration pemisahan dijalankan | Catatan jumlah baris `TrxLabSpecimen` di lingkungan sasaran |
| 6 | Migration pemisahan tidak memutus tautan `BilChargeLines.SourceItemId` | Pengujian migration |
| 7 | Data master awal alasan penolakan dan batas nilai sudah terisi | Cuplikan isi tabel di lingkungan sasaran |
| 8 | Batas kritis pada data master awal sudah disahkan pihak klinis | Dokumen pengesahan |
| 9 | Kewenangan baru terdaftar otomatis dan menolak pengguna tanpa hak | Uji `403` pada tiap endpoint baru |
| 10 | Kolom bertanda sensitif tidak muncul pada log | Cuplikan log |
| 11 | Tidak ada kolom maupun method finansial pada modul Laboratorium | Pengujian yang sudah ada tetap lulus |
| 12 | Utang teknis struktur folder **tidak** dirapikan diam-diam | Tinjauan perubahan berkas |
| 13 | Seluruh layar menangani muat, kosong, gagal, kirim ganda, dan `403` | Tinjauan tampilan |

---

## 14. Urutan Pengiriman

Diurutkan menurut ketergantungan, bukan tanggal.

| Gelombang | Isi | Kenapa urutan ini | Prasyarat |
|---|---|---|---|
| **`MVP-0`** | `EPIC-LAB-03` batas nilai, `EPIC-LAB-06` alasan penolakan, `EPIC-LAB-09` katalog dan harga | Ketiganya **murni penambahan atau penyajian** — tidak menyentuh satu baris pun kode yang sudah berjalan. `EPIC-LAB-09` bahkan nol tabel baru | `LAB-COORD-005` untuk kolom disiplin |
| **`MVP-1`** | `EPIC-LAB-08` pendaftaran pasien, `EPIC-LAB-01` cito dan duplo | Pendaftaran adalah hulu alur; cito melekat pada pemeriksaan yang dibuat di situ | `LAB-COORD-003`, `LAB-COORD-004` |
| **`MVP-2`** | `EPIC-LAB-02` pemisahan wadah dan pemeriksaan, `EPIC-LAB-05` fakta per pemeriksaan | Satu perubahan struktural yang tidak dapat dipisah; fakta mengikuti satuan baru | **`LAB-OPEN-012` wajib dijawab lebih dulu** |
| **`MVP-3`** | `EPIC-LAB-04` daftar kerja, `EPIC-LAB-10` monitoring per disiplin | Keduanya membutuhkan penanda cito dari `MVP-1` dan satuan pekerjaan dari `MVP-2` | `MVP-1`, `MVP-2` |
| **`MVP-4`** | `EPIC-LAB-07` layar Laboratorium | Layar hanya dapat dibangun setelah perilaku backendnya pasti | `MVP-0` sampai `MVP-3` |
| **`MVP-5a`** | `EPIC-LAB-11` bagian yang bebas hambatan — jenis specimen, volume, waktu penerimaan, Qty, titik kunci, dan menu Penerimaan Sampling/Specimen | Ditambahkan 2026-09-14. Berdiri sebagai gelombang tersendiri agar penahan `LAB-REQ-005` tidak menular ke gelombang yang sudah siap jalan | `MVP-2`, `MVP-4`, dan lima baris satuan `MstMeasurement` dari Master Data |
| **`POST-MVP`** | Slice `S1b`, `S2b`, `S4`, `S4b`, `S4c`, `S5`, `S6`, `S8`, `S9`, `S16`, `S17`, `S18`, `S19` | Seluruhnya masih terblokir pihak di luar modul atau belum diputuskan | `LAB-SIGN-001`, `LAB-P0-001` sampai `LAB-P0-008`, `LAB-COORD-001`, `LAB-COORD-002`, `LAB-AMD-001` |

> **`FR-11.9` dan `FR-11.10` sengaja tidak muncul pada tabel di atas.** Keduanya berstatus
> `OPEN DECISION`, dan kontrak PRD melarang epic berstatus itu masuk gelombang pengiriman mana
> pun. Keduanya dicatat pada bagian 16.6 sebagai **pekerjaan yang belum dijadwalkan**, dan baru
> memperoleh gelombang setelah `LAB-REQ-005` dijawab.

**Perubahan urutan sejak revision 1, dan alasannya.** `EPIC-LAB-09` katalog dan harga naik ke
`MVP-0` karena ternyata **nol tabel baru** — seluruhnya penyajian data milik Master Data. Ia
justru pekerjaan paling ringan dari seluruh gelombang, sekaligus yang paling cepat terlihat
manfaatnya oleh petugas.

`EPIC-LAB-08` pendaftaran ditempatkan sebelum pemisahan wadah karena ia **hulu alur** — tanpa
pendaftaran, pasien datang langsung tidak dapat dilayani sama sekali, sementara pemisahan wadah
hanya memperbaiki bentuk data yang sudah bekerja.

**Catatan tentang `MVP-3`.** Layar untuk kemampuan `MVP-0` sebenarnya dapat dibangun lebih awal
tanpa menunggu `MVP-1`, karena batas nilai dan alasan penolakan tidak tersentuh pemisahan. Bila
rumah sakit membutuhkan hasil terlihat lebih cepat, pemecahan itu sah dan tidak menimbulkan
pekerjaan ulang.

---

## 15. Pertanyaan Terbuka Sebelum Development Lock

| ID | Pertanyaan | Memblokir | Pemilik |
|---|---|:---:|---|
| `LAB-OPEN-012` | Berapa banyak data laboratorium yang sudah terisi di basis data produksi? | **Ya** — memblokir `MVP-1` | Pemilik repository backend + DBA |
| `LAB-OPEN-002` | ~~Di mana `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` yang disebut `AGENTS.md`?~~ **Terjawab 2026-09-01** (`LAB-FACT-007`): `QuilvianEngineeringSkills/agents/rules/backend/engineering/`, keduanya masih berlaku | Tidak — **ditutup** | — |
| `LAB-OPEN-018` | Kapan suite Skill yang memuat `rules/backend/engineering/` dipublikasikan ke marketplace yang terpasang, sehingga rules root runtime memenuhi `AGENTS.md`? | **Ya** — memblokir seluruh implementasi backend | Pemilik repository backend + pemilik suite Skill |
| `LAB-OPEN-019` | Apakah lifecycle `LaboratoryManagement` pada registry dinaikkan dari `PLANNED` ke `ACTIVE`, sehingga `QBE-MOD-002` tidak lagi menahan entity `Lab*`? | **Ya** — memblokir seluruh slice yang membuat entity `Lab*` | Pemilik repository backend |
| `DEC-LAB-005` | Isi data awal batas nilai: pemeriksaan mana saja beserta angkanya, disahkan siapa | **Ya** — memblokir Definition of Done butir 7 dan 8 | Kepala instalasi + pihak klinis |
| `DEC-LAB-006` | Isi data awal alasan penolakan beserta penanda kesalahan internalnya | **Ya** — memblokir Definition of Done butir 7 | Kepala instalasi + Billing |
| — | Siapa pemegang kewenangan `LabCriticalBound : Approve` di rumah sakit ini | **Ya** — memblokir `EPIC-LAB-03` `FR-03.4` | Manajemen rumah sakit |

### Akibat bagi langkah berikutnya

Dokumen ini berstatus **`draft`** dan memuat pertanyaan terbuka yang ditandai memblokir.
Menurut kontrak keluaran, dokumen dengan keadaan seperti ini **tidak boleh** diteruskan ke
`/plan-module-delivery` sebelum kelimanya dijawab.

Yang perlu ditegaskan agar tidak disalahpahami: kelima pertanyaan itu **bukan** pertanyaan
desain. Desainnya sudah lengkap. Kelimanya adalah pertanyaan **kesiapan lapangan** — data,
dokumen tata kelola, dan penunjukan orang. Tidak satu pun akan mengubah arsitektur bila
dijawab.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 3 | 2026-09-09 | Penahan bagian 7 dikoreksi: `LAB-COORD-001` dan `LAB-COORD-002` sudah `closed` sejak 2026-09-01 dan tidak lagi menahan `S5` maupun `S6`. Penahan `S5` yang sebenarnya diperjelas menjadi `LAB-P0-004` dan `LAB-OPEN-014`. Dicatat bahwa `LAB-SIGN-001` kini satu-satunya penahan `S4`, `S4b`, `S4c`, `S5`, dan `S6`, dan permintaan tanda tangannya diajukan sebagai `LAB-REQ-004` | `draft` |
| 2 | 2026-09-01 | Batas MVP diperluas ke hulu: dimulai dari pasien tiba di laboratorium, bukan dari pesanan dibuat. Tiga epic ditambahkan — `EPIC-LAB-08` pendaftaran, `EPIC-LAB-09` katalog dan harga, `EPIC-LAB-10` monitoring per disiplin. Gelombang pengiriman disusun ulang menjadi lima; `EPIC-LAB-09` naik ke `MVP-0` karena nol tabel baru | `draft` |
| 1 | 2026-09-01 | PRD ke MVP pertama. Tujuh epic dengan 30 functional requirement, 14 skenario UAT berpasangan berhasil dan gagal, 13 butir Definition of Done, dan empat gelombang pengiriman. Lima pertanyaan terbuka memblokir handoff ke perencanaan delivery | `draft` |

---

## 16. Amandemen 2026-09-14 — `EPIC-LAB-11` Penerimaan Sampling/Specimen

Menurunkan `LAB-DEC-038` sampai `LAB-DEC-042` dan `LAB-DEC-045` dari decision log revision 26.
Seluruh entity, status, hak akses, dan endpoint yang disebut di bawah **sudah tercatat** pada
`02-backend-architecture.md` bagian 11, `erd/data-dictionary.md` bagian 12, dan
`contracts/api-contract.md` amandemen `r7`.

### 16.1 Batas kemampuan — titik mulai dan titik akhir

| Batas | Isi |
|---|---|
| **Mulai** | Petugas laboratorium membuka menu `Penerimaan Sampling/Specimen` dan mengidentifikasi pasien rujukan luar atau pasien datang langsung |
| **Selesai** | Kelayakan wadah ditetapkan `Layak` atau `Tidak Layak`, daftar pemeriksaan terkunci, dan fakta kelayakan tagih terbit sebanyak baris pemeriksaannya |

**Di luar batas ini**, dan tetap milik alur yang sudah ada: pengisian hasil, validasi, dan
rilis hasil. Penerimaan berakhir tepat ketika pekerjaan analis dimulai.

### 16.2 `EPIC-LAB-11` — functional requirement

| ID | Kebutuhan | Kemampuan asal | Disposisi |
|---|---|---|---|
| `FR-11.1` | Jenis specimen dipilih dari daftar terkendali milik Laboratorium, dengan `Lainnya` yang wajib berketerangan | — | `MISSING / NEW` |
| `FR-11.2` | Pemakaian `Lainnya` terlihat pada daftar pantau kepala instalasi, yang dapat menaikkannya menjadi nilai tetap | — | `MISSING / NEW` |
| `FR-11.3` | Volume specimen tersimpan sebagai angka beserta satuannya, tanpa batas minimum maupun maksimum | — | `MISSING / NEW` |
| `FR-11.4` | Satuan volume dibaca dari `MstMeasurement` yang ber-`IsForLaboratory`, bukan dari daftar milik Laboratorium | `LAB-CAP-*` data induk Master Data | `EXISTING / REUSE` |
| `FR-11.5` | Waktu penerimaan fisik diisi petugas, terpisah dari `ReceivedAt` yang diisi server dan tidak dapat diubah | — | `EXTEND` |
| ~~`FR-11.6`~~ | ~~Qty pada layar memperbanyak baris pemeriksaan~~ | — | **`DICABUT`** `LAB-DEC-050` 2026-09-14 — terbukti tidak dapat dipenuhi; index unik `(SpecimenId, ProcedureId)` menolak baris kedua |
| `FR-11.7` | Daftar pemeriksaan terkunci saat kelayakan wadah ditetapkan — **hanya untuk penambahan**; pembatalan tetap terbuka (`LAB-DEC-049`) | `VAL-18` pada `LabExaminationService` | **`EXISTING / REUSE`** |
| `FR-11.8` | Menu `Penerimaan Sampling/Specimen` berdiri sendiri; layar `lab-orders` yang ada tidak berubah perilakunya | Layar lab pada `9cd4cd03f` | `EXTEND` |
| `FR-11.9` | Petugas dapat mengusulkan instansi perujuk baru tanpa menahan penerimaan pasien | — | **`OPEN DECISION`** |
| `FR-11.10` | Metode pembayaran ditampilkan baca-saja untuk jalur rujukan | — | **`OPEN DECISION`** |

**`FR-11.7` perlu dibaca dengan teliti.** Ia berdisposisi `EXISTING / REUSE`, bukan
`MISSING / NEW`. `LabExaminationService.cs:123@466a7127` sudah menegakkannya sebagai `VAL-18`.
Pekerjaannya bukan membangun, melainkan **memeriksa bahwa jalur hapus memakai penjagaan yang
sama** dan menuliskan pengujian penjaganya (`T-55d`).

**`FR-11.9` dan `FR-11.10` berstatus `OPEN DECISION`**, sehingga menurut kontrak PRD keduanya
**tidak masuk gelombang pengiriman mana pun** sampai `LAB-REQ-005` dijawab.

### 16.3 Kemampuan yang ditunda beserta penggantinya

| Yang ditunda | Alasan | Yang dipakai selama menunggu |
|---|---|---|
| Pengusulan instansi perujuk (`FR-11.9`) | `LAB-COORD-006` — data induk perujuk tidak punya endpoint tulis sama sekali | `VAL-43` tetap berlaku: pendaftaran ditolak `422`, petugas menghubungi bagian data induk. **Jalan buntunya belum hilang** |
| Metode pembayaran (`FR-11.10`) | `LAB-COORD-007` — `Piutang Mitra` belum punya nilai pada `EncounterPaymentType` | Wilayah metode pembayaran pada layar menampilkan *belum dapat ditentukan* |
| Promo terhadap grand total | Di luar scope `LAB-DEC-037`; pemiliknya belum ditetapkan | Tidak ada. Grand total dihitung tanpa promo |
| Penerimaan uang tunai di laboratorium | Ditolak `LAB-DEC-037`; melanggar `RJ-BIL-GATE-DEC-003` | Pasien membayar lewat kasir seperti biasa |
| Batas volume minimal per jenis pemeriksaan | `LAB-DEC-001` menaruhnya di Rilis 2 sebagai bagian katalog mandiri | Petugas menilai kecukupan sampel lewat penetapan kelayakan |

Tabel ini ditulis apa adanya: dua baris pertama berarti **menu ini belum menyelesaikan masalah
yang melahirkannya**. Sampel dari klinik yang belum terdaftar masih tertahan di meja penerimaan.

### 16.4 Skenario UAT

**Jalur berhasil — `UAT-11.1` penerimaan rujukan luar:**

1. Petugas membuka menu Penerimaan Sampling/Specimen, mencari pasien dengan NIK, dan menemukannya.
2. Ia mengisi data pemeriksaan, memilih instansi perujuk **yang sudah ada di daftar**.
3. Ia mencatat wadah: jenis `Blood`, volume `3` `mL`, waktu penerimaan fisik kemarin pukul 21.10.
4. Ia memilih Hemoglobin, Leukosit, dan Trombosit.
5. Ia melihat tabungnya hanya cukup untuk dua, lalu **menghapus Trombosit** — berhasil.
6. Ia menetapkan `Layak`. Dua fakta kelayakan tagih terbit.
7. Percobaan menambah Trombosit sesudahnya **ditolak** dengan pesan `VAL-18`.

**Jalur berhasil — `UAT-11.2` dua pemeriksaan sejenis dipilih sebagai dua butir katalog:**

> **Diganti `LAB-DEC-050` pada 2026-09-14.** Skenario semula memakai Qty `2` pada Glukosa.
> Skenario itu terbukti mustahil: index unik `(SpecimenId, ProcedureId)` menolak baris kedua.

1. Petugas memilih **Glukosa Puasa** dan **Glukosa 2 Jam PP** — dua butir katalog yang berbeda.
2. Setelah tersimpan, layar detail menampilkan **dua baris**, masing-masing dengan nama,
   tarif, dan batas nilainya sendiri.
3. Analis mengisi baris pertama `96` dan baris kedua `143`. **Keduanya tersimpan utuh.**
4. Petugas mencoba menambahkan Glukosa Puasa untuk kedua kalinya pada wadah yang sama.
   **Ditolak** dengan pesan `VAL-07`.

Langkah 4 adalah inti skenario ini: ia membuktikan aturan yang membatalkan `LAB-DEC-038` memang
ditegakkan, bukan sekadar tertulis.

**Jalur gagal — `UAT-11.3` jenis specimen belum terdaftar:**

1. Sampel cairan kista datang pukul 21.00. Jenisnya belum ada di daftar.
2. Petugas memilih `Lainnya` dan menuliskan "cairan kista" pada keterangan.
3. **Penerimaan berhasil disimpan.** Tidak ada penolakan.
4. Keesokan hari kepala instalasi membuka daftar pantau, melihat "cairan kista" dipakai tiga
   kali dalam sebulan, lalu menaikkannya menjadi jenis tetap.

**Jalur gagal — `UAT-11.4` volume tanpa satuan:**

1. Petugas mengetik `5` pada volume lalu menekan simpan tanpa memilih satuan.
2. Ditolak dengan pesan "Pilih satuan volumenya." (`VAL-56`).

**Jalur gagal — `UAT-11.5` instansi perujuk belum terdaftar:**

1. Sampel dari Klinik Sehat Sentosa datang. Kliniknya belum ada di daftar.
2. Pendaftaran **ditolak** `422` `VAL-43`.
3. **Ini jalur gagal yang belum punya jalan keluar** — lihat 16.3. Petugas menghubungi bagian
   data induk, dan sampelnya menunggu.

`UAT-11.5` sengaja dicantumkan walaupun hasilnya kegagalan. Menyembunyikannya akan membuat
pembaca menyangka menu ini sudah utuh.

### 16.5 Definition of Done — `EPIC-LAB-11`

| Butir | Bukti yang menjawabnya |
|---|---|
| Tabel `LabSpecimenType` berdiri beserta unique parsial `Lainnya` | `T-M3` lulus |
| Lima kolom `LabSpecimen` berdiri dan baris lama tetap terbaca | `T-M1`, `T-M2` lulus |
| Tidak ada satu pun properti `Qty` atau `Quantity` pada model Laboratorium | `T-52b` lulus |
| Menambah jenis pemeriksaan yang sama dua kali pada satu wadah **ditolak** | `T-45a` lulus |
| Fakta kelayakan tagih terbit per baris pemeriksaan | `AC-37` yang sudah ada; tidak tersentuh amandemen |
| Jalur **batal** tetap terbuka sesudah kelayakan, dan layar mengatakannya | `T-55e` lulus |
| Specimen berjenis `Lainnya` **tidak pernah** tertahan | `T-59c` lulus |
| Tidak ada jalur kode yang membandingkan volume terhadap batas minimum | `T-63c` lulus |
| `ReceivedAt` tidak dapat diubah endpoint mana pun | `T-65a`, `T-65b` lulus |
| Wadah malam hari muncul pada laporan hari kedatangannya | `T-67a` lulus |
| Perhitungan keterlambatan cito tidak berubah | `T-17a` lulus |
| Seluruh pengujian layar lab yang sudah ada tetap lulus **tanpa disentuh** | `T-76a` lulus |
| Lima baris satuan `IsForLaboratory` terisi | Dikonfirmasi pemilik `master-data` — **bukan** dikerjakan Laboratorium |

Butir terakhir sengaja tidak dapat dijawab Laboratorium sendiri. Tanpa kelima baris itu, kolom
volume tidak dapat dipakai walaupun seluruh kode sudah benar.

### 16.6 Urutan pengiriman — gelombang `MVP-5`

Ditetapkan pemilik modul 2026-09-14. Berdiri **sebagai gelombang tersendiri**, bukan disisipkan
ke `MVP-1` maupun `MVP-2`.

| Gelombang | Isi | Prasyarat |
|---|---|---|
| **`MVP-5a`** | `FR-11.1` sampai `FR-11.5`, `FR-11.7`, dan `FR-11.8` — jenis specimen, volume, waktu penerimaan, titik kunci, dan menu. **`FR-11.6` dicabut** `LAB-DEC-050` | `MVP-2` untuk satuan pemeriksaan; `MVP-4` untuk pola layar. Lima baris `MstMeasurement` dari Master Data |
| **belum dijadwalkan** | `FR-11.9` dan `FR-11.10` | **`LAB-REQ-005` dijawab.** Keduanya berstatus `OPEN DECISION`, sehingga **tidak diberi gelombang** sesuai kontrak PRD |

**Kenapa `FR-11.9` dan `FR-11.10` tidak diberi nama gelombang sama sekali.** Memberi nomor
gelombang kepada pekerjaan yang belum diputuskan membuatnya terlihat terjadwal. Ia akan masuk
rencana kapasitas, dihitung dalam perkiraan, dan pada akhirnya dikerjakan seseorang yang
menyangka keputusannya sudah ada. Selama disposisinya `OPEN DECISION`, keduanya tetap tanpa
gelombang.

**Kenapa gelombang tersendiri, bukan disisipkan.** Penahan `LAB-REQ-005` adalah penahan milik
modul lain. Bila `FR-11.9` dan `FR-11.10` disisipkan ke `MVP-1` atau `MVP-2`, kedua gelombang
itu ikut tertahan menunggu jawaban yang tidak dipegang Laboratorium — termasuk pekerjaan di
dalamnya yang sebenarnya sudah siap jalan. Memisahkannya menjaga penahan tetap berada pada
bagian yang memang tertahan.

### 16.6b Gerbang menuju `plan-module-delivery`

Kontrak PRD menyatakan dokumen dengan pertanyaan memblokir yang belum terjawab **tidak boleh
diteruskan** ke perencanaan delivery. Keadaan `EPIC-LAB-11` pada 2026-09-14:

| Bagian | Pertanyaan memblokir | Boleh direncanakan |
|---|---|---|
| `FR-11.1`..`FR-11.5`, `FR-11.7`, `FR-11.8` (`MVP-5a`) | Tidak ada yang menghalangi perancangan task. `FR-11.6` dicabut `LAB-DEC-050` | **Ya** |
| `FR-11.9`, `FR-11.10` | `LAB-COORD-006`, `LAB-COORD-007` | **Tidak** |
| Kolom volume di dalam `MVP-5a` | Lima baris `MstMeasurement` belum diisi | Task boleh disusun; **pengujiannya belum dapat dijalankan** sampai barisnya ada |

Perencanaan `MVP-5a` karena itu boleh berjalan, dengan satu batas yang harus tertulis pada
roadmapnya: task kolom volume berstatus *menunggu data, bukan menunggu kode* — pola yang sama
dengan `FE-LAB-05` yang layarnya selesai 2026-09-07 tetapi verifikasi manualnya tertahan
karena daftar perujuk kosong.

### 16.7 Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Memblokir |
|---|---|---|
| `LAB-COORD-006` | Pengelolaan data induk instansi perujuk beserta status menunggu persetujuan | **`MVP-5b`** |
| `LAB-COORD-007` | Nilai `EncounterPaymentType` baru untuk piutang mitra | **`MVP-5b`** |
| `LAB-OPEN-024` | Umur usulan, akibat penolakan, dan wewenang penggabungan | `MVP-5b` tahap implementasi |
| Pengisian `MstMeasurement` | Lima baris satuan ber-`IsForLaboratory` | **`MVP-5a`** — memblokir kolom volume, bukan seluruh gelombang |

Tiga yang pertama diajukan sebagai `LAB-REQ-005` pada 2026-09-14. Yang keempat **belum
diajukan** dan perlu dikoordinasikan terpisah dengan pemilik `master-data`.

---

## 17. Amandemen 2026-09-15 — `EPIC-LAB-12` Konfirmasi Pesanan dan Pembatalan Beralasan

**Dasar bukti:** rekonsiliasi putaran 2 atas artifact "Module Artifact - Laboratorium"
(`05-evidence-reconciliation.md` bagian 10). **Dasar keputusan:** `LAB-DEC-061` dan
`LAB-DEC-063`, disetujui pemilik modul 2026-09-15.

### 17.1 Kenapa epic ini berdiri sendiri

Tiga menu pemeriksaan menampilkan kolom **Konfirmasi** dan **Status Pemeriksaan** yang memuat
`Terkonfirmasi` — dua hal yang selama ini tidak punya padanan sama sekali di sistem. `LAB-P0-002`
justru mencatatnya sebagai pertanyaan terbuka sejak 2026-09-01: di mana `Confirmed` berdiri.

Artifact menjawabnya, dan jawabannya membawa serta dua hal lain: **dokter pemeriksa** yang dipilih
saat konfirmasi, dan **alasan pembatalan** yang wajib.

### 17.2 Functional requirement

| ID | Kebutuhan | Turunan |
|---|---|---|
| `FR-11.12` | Petugas mengonfirmasi pesanan **satu kali**, memilih dokter pemeriksa, dan namanya tercatat sebagai konfirmator beserta waktunya | `LAB-DEC-061` |
| `FR-11.13` | Pembatalan pesanan **wajib beralasan**, dan hanya sah selama pesanan belum dikerjakan | `LAB-DEC-063` |

### 17.3 Batas rilis pertama

| Masuk | Tidak masuk |
|---|---|
| Status `Confirmed` beserta konfirmator, waktu, dan dokter pemeriksa | Konfirmasi menjadi **wajib** sebelum `Accepted` — tertahan `LAB-OPEN-027` |
| Endpoint konfirmasi dan layar pop-upnya | Status pembayaran mengunci tombol Proses — tertahan `LAB-COORD-010` |
| Pembatalan beralasan beserta batas statusnya | Aturan **koreksi** hasil — `LAB-P0-003` tetap terbuka untuk bagian itu |
| Print membuka preview lebih dulu | Alert order baru 10 detik |

### 17.4 Definition of Done — `EPIC-LAB-12`

1. `AC-94` sampai `AC-97` terpenuhi dan terbukti.
2. **`T-97c` terbukti**: jalur `Requested` → `Accepted` tanpa konfirmasi masih berjalan. Epic ini
   menambah tahap, bukan menutup jalur yang sudah ada.
3. **`T-97b` terbukti**: pesanan `Requested` dan `Confirmed` tetap dapat dibatalkan.
4. Konfirmator dan waktu konfirmasi **diturunkan server**, tidak pernah dari badan permintaan.
5. Nol permission baru; nol kolom pembayaran pada `LabOrder`.

> **Satu butir DoD sengaja berbentuk ketiadaan perubahan**, sama seperti `EPIC-LAB-11`. Epic ini
> mengandung satu-satunya pengetatan gelombang `MVP-5c` — `VAL-75` — dan pengetatan pada endpoint
> yang sedang dipakai adalah hal yang paling mahal bila tidak dihitung dampaknya lebih dulu.

---

## 18. Amandemen 2026-09-18 — `EPIC-LAB-13` Pengisian hasil Mikrobiologi dan Patologi Anatomi

Menurunkan `02-backend-architecture.md` bagian 14, `erd/` amandemen 2026-09-18, dan usulan
kontrak `LAB-API-v1` `r24`, `LAB-VAL-v1` `r7`, `LAB-PERM-v1` revision 6.

**Seluruh entity, status, permission, dan endpoint yang disebut di bawah sudah tercatat pada
ketiga dokumen itu.** Nol konsep lahir di dokumen ini.

### 18.1 Batas epic ini — titik mulai dan titik akhir

| Batas | Isi |
|---|---|
| **Titik mulai** | Sebuah pemeriksaan Mikrobiologi atau Patologi Anatomi sudah berdiri, wadahnya sudah dinyatakan layak, dan analis membuka layar pengisian hasil |
| **Titik akhir** | Hasilnya tersimpan dan dapat dibaca ulang. **Berhenti di situ** — nol validasi, nol rilis, nol pengiriman |

**Yang dianggap selesai:** analis dapat mengetik hasil kedua disiplin ke dalam sistem, dengan
organisme dan antibiotik terkendali. **Yang tidak berubah sama sekali:** hasilnya masih belum
dapat dinyatakan sah oleh siapa pun, sama seperti Patologi Klinik sejak 2026-09-17.

### 18.2 Kemampuan `MUST HAVE`

| ID kemampuan asal | Kemampuan | Disposisi |
|---|---|---|
| BR-23 bentuk ketiga | Pengisian hasil Mikrobiologi berstruktur | **`MISSING / NEW`** |
| BR-23 bentuk keempat | Pengisian laporan narasi Patologi Anatomi | **`MISSING / NEW`** |
| `LAB-DEC-084` | Data induk organisme dan antibiotik **beserta pengelolaannya** | **`MISSING / NEW`** |
| `LAB-DEC-027` | Perluasan `LabResultForm` menjadi empat bentuk | **`EXTEND`** |
| `S4a` / `BE-LAB-43` | Pola pengisian hasil, snapshot, dan pemisahan waktu | **`EXISTING / REUSE`** |

### 18.3 Functional requirement

| ID | Kebutuhan | Dapat diuji lewat | Disposisi |
|---|---|---|---|
| `FR-13.1` | Analis dapat mencatat status temuan `Normal`/`Positif`/`Negatif` pada pemeriksaan Mikrobiologi | `PUT /lab-examinations/{id}/result/microbiology` | `MISSING / NEW` |
| `FR-13.2` | Analis dapat menambah, menyunting, dan menghapus isolat, masing-masing menunjuk organisme dari daftar terkendali | Endpoint yang sama | `MISSING / NEW` |
| `FR-13.3` | Setiap isolat dapat memuat sejumlah baris kepekaan antibiotik berisi kadar, zona hambat, dan `R`/`I`/`S` | Endpoint yang sama | `MISSING / NEW` |
| `FR-13.4` | Hasil Mikrobiologi tanpa satu pun isolat **diterima** sebagai hasil yang sah | Endpoint yang sama, `isolates` kosong | `MISSING / NEW` |
| `FR-13.5` | Analis dapat mencatat makroskopik, mikroskopik, dan kesimpulan Patologi Anatomi; ketiganya wajib | `PUT /lab-examinations/{id}/result/pathology` | `MISSING / NEW` |
| `FR-13.6` | Kepala instalasi dapat menambah dan menonaktifkan organisme serta antibiotik | Grup Lab Organism dan Lab Antibiotic | `MISSING / NEW` |
| `FR-13.7` | Organisme atau antibiotik yang dinonaktifkan **tidak dapat dipakai pada baris baru**, tetapi hasil lama yang menunjuknya tetap terbaca utuh | `VAL-85`, `VAL-86`, `INV-31` | `MISSING / NEW` |
| `FR-13.8` | Sistem **menolak** pengisian lewat jalur yang tidak sesuai bentuk hasil pemeriksaan | `VAL-84` | `MISSING / NEW` |
| `FR-13.9` | **Nol status hasil** muncul pada basis data maupun jawaban API | `INV-29`, diuji sebagai ketiadaan | `MISSING / NEW` |

### 18.4 Skenario UAT

**Jalur berhasil — Mikrobiologi.** Analis membuka hasil kultur darah pasien Andi. Ia memilih
status temuan `Positif`, menambah isolat `Escherichia coli` dari daftar, lalu menambahkan tiga
baris kepekaan: Ceftriaxone zona 22 mm `S`, Ampicillin zona 8 mm `R`, Ciprofloxacin zona 17 mm
`I`. Ia menekan Simpan. Sistem menyimpan seluruhnya sebagai satu kesatuan, dan hasilnya terbaca
ulang persis seperti yang diketik.

**Jalur gagal — Mikrobiologi.** Analis menambahkan Ceftriaxone dua kali pada isolat yang sama.
Sistem menolak dengan `422` dan pesan *"Antibiotik ini sudah diuji pada kuman tersebut."*
(`VAL-87`). Tidak satu pun baris tersimpan sebagian.

**Jalur berhasil — Patologi Anatomi.** Patolog mengisi ketiga ruas narasi lalu menyimpan.
Hasilnya terbaca ulang utuh.

**Jalur gagal — Patologi Anatomi.** Patolog mengisi makroskopik dan mikroskopik, mengosongkan
kesimpulan, lalu menekan Simpan. Sistem menolak dengan `422` (`VAL-88`). **Tidak** disimpan
sebagai draft.

**Jalur gagal — data induk kosong.** Pada hari pertama, daftar organisme masih kosong. Analis
membuka layar hasil Mikrobiologi dan melihat keterangan *"Daftar organisme belum diisi. Hubungi
kepala instalasi."* — bukan pemilih kosong tanpa penjelasan.

**Jalur gagal — organisme dinonaktifkan.** Kepala instalasi menonaktifkan satu organisme yang
sudah dipakai pada hasil bulan lalu. Hasil lama **tetap terbaca utuh**; yang ditolak hanya
pemakaiannya pada baris baru (`FR-13.7`).

### 18.5 Definition of Done

| # | Butir | Cara menjawabnya |
|---:|---|---|
| 1 | Usulan `LAB-API-v1` `r24`, `LAB-VAL-v1` `r7`, dan `LAB-PERM-v1` rev 6 **disetujui pemilik modul** | Ada tanda tangan pada ketiga dokumen kontrak |
| 2 | Empat tabel baru dan empat kolom `LabExamination` berdiri lewat migration | Migration berjalan dan dapat dimundurkan |
| 3 | Index unik `(LabMicrobiologyIsolateId, LabAntibioticId)` **parsial** — dibatasi `IsDelete = false` | Diperiksa langsung pada basis data, bukan pada niat |
| 4 | Kedua data induk **punya endpoint tulisnya** dan **sudah terisi** | Daftar organisme dan antibiotik tidak kosong |
| 5 | `VAL-83` sampai `VAL-91` ditegakkan | Setiap aturan punya bukti pemeriksaan |
| 6 | **Nol status hasil** bertambah pada `LabExaminationStatus` maupun jawaban API | Dibuktikan **terbalik**: pemeriksaan yang membuktikan ketiadaannya |
| 7 | **Nol tombol Validasi/Rilis** muncul pada kedua layar | Diperiksa pada layar berjalan |
| 8 | Ketiga ruas narasi Patologi Anatomi **tidak muncul** pada logger maupun layar non-klinis | Diperiksa pada log dan pada DTO |

> **Butir 6 dan 7 sengaja berbentuk ketiadaan**, sama seperti dua epic sebelumnya. Keduanya
> adalah hal yang paling mungkin ditambahkan implementer dengan niat baik — status terasa rapi,
> dan tombol Rilis terasa melengkapi layar. Keduanya **melanggar keputusan yang sudah diambil**,
> dan satu-satunya cara mencegahnya adalah menuliskan ketiadaannya sebagai syarat selesai.

### 18.6 Urutan pengiriman

| Gelombang | Isi | Prasyarat |
|---|---|---|
| **`MVP-6a`** | Dua data induk beserta pengelolaannya, lalu **pengisiannya** | Kontrak disetujui |
| **`MVP-6b`** | Pengisian hasil Patologi Anatomi | `MVP-6a` **tidak** menjadi prasyaratnya — PA nol menunjuk data induk |
| **`MVP-6c`** | Pengisian hasil Mikrobiologi | **Wajib sesudah `MVP-6a`**, termasuk pengisian datanya |
| **`POST-MVP`** | Lampiran gambar Patologi Anatomi; antibiogram; validasi dan rilis kedua disiplin | `DEC-LAB-016`; `DEC-LAB-011` |

> **`MVP-6b` sengaja ditempatkan sebelum `MVP-6c`, dan itu keputusan urutan yang berbasis
> risiko.** Patologi Anatomi tidak bergantung pada data induk mana pun, sehingga ia dapat selesai
> penuh tanpa menunggu siapa pun mengisi daftar organisme. Mikrobiologi **tidak bisa** — layar
> yang jadi tetapi daftarnya kosong adalah layar yang tidak dapat dipakai, dan itu keadaan yang
> sudah dua kali terjadi di modul ini lewat `LAB-COORD-006` dan `MST-POS-WRITE`.

### 18.7 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Memblokir? | Pemilik |
|---|---|---|
| ~~Persetujuan `LAB-API-v1` `r24` dan dua kontrak penyertanya~~ | ✅ **Terjawab 2026-09-18** — ketiganya disetujui Yoga Aji Pratama. **Tidak lagi memblokir** | — |
| `DEC-LAB-016` — di mana gambar Patologi Anatomi disimpan | **Tidak** memblokir epic ini; memblokir lampiran gambarnya | Platform + pemilik modul |
| Siapa mengisi daftar organisme dan antibiotik, dan kapan | **Ya** untuk `MVP-6c` | Kepala instalasi + `DR-LAB-002` |
| `DEC-LAB-011` — pemegang kewenangan validasi | **Tidak** memblokir epic ini | Kepala instalasi + manajemen RS |

> ### ✅ Gerbang perencanaan terbuka — 2026-09-18
>
> Butir pertama terjawab: `LAB-API-v1` `r24`, `LAB-VAL-v1` `r7`, dan `LAB-PERM-v1` revision 6
> **disetujui** Yoga Aji Pratama pada 2026-09-18. **`EPIC-LAB-13` boleh diteruskan ke
> `/plan-module-delivery`.**
>
> **Dua butir sisa tetap terbuka dan keduanya bukan penahan epic ini:** `DEC-LAB-016` memblokir
> lampiran gambar Patologi Anatomi saja, dan `DEC-LAB-011` memblokir validasi/rilis yang memang
> di luar scope epic ini.
>
> **Satu butir tetap memblokir gelombang `MVP-6c`, dan ia bukan pekerjaan programmer:** siapa
> mengisi daftar organisme dan antibiotik, dan kapan. Layar Mikrobiologi dengan daftar kosong
> tidak dapat dipakai sama sekali — itu sebabnya `MVP-6b` ditempatkan lebih dulu.

---

## 19. Amandemen 2026-09-18 sore — `EPIC-LAB-13` bagian Patologi Anatomi DIRANCANG ULANG

> **Menggantikan `FR-13.5` dan gelombang `MVP-6b` pada bagian 18.** Bagian Mikrobiologi —
> `FR-13.1`..`FR-13.4`, `FR-13.7`, `FR-13.8`, gelombang `MVP-6a` dan `MVP-6c` — **tetap berlaku
> apa adanya**.

Menurunkan `LAB-DEC-085`..`LAB-DEC-094`, `LAB-DA-001` rev 7 A4, `02-backend-architecture.md`
bagian 15, dan usulan `LAB-API-v1` `r25`.

### 19.1 Batas yang berubah

| Bagian 18 (dicabut) | Bagian 19 (berlaku) |
|---|---|
| Hasil PA = tiga kolom pada `LabExamination` | Laporan PA **per pesanan**, tujuh tabel baru |
| `FR-13.5` tiga ruas wajib | Ruas **bergantung kategori**, kewajibannya dari data induk |
| Titik akhir: hasil tersimpan | Titik akhir: laporan tersimpan **dan dapat dinyatakan selesai** |

### 19.2 Functional requirement — menggantikan `FR-13.5`

| ID | Kebutuhan | Diuji lewat | Disposisi |
|---|---|---|---|
| `FR-13.10` | Dokter pemesan dapat menulis empat ruas konteks klinis pada pesanan PA, dan patolog membacanya | `PUT /lab-orders/{id}/pathology-context` | `MISSING / NEW` |
| `FR-13.11` | Kepala instalasi dapat mengelola parameter, kategori, keberlakuan, dan **pemetaan jenis pemeriksaan** | Grup Lab Pathology Master Data | `MISSING / NEW` |
| `FR-13.12` | Layar laporan menampilkan **hanya parameter yang berlaku** bagi kategori pesanan itu, **tanpa duplikasi** ketika beberapa kategori bertemu | `GET /lab-orders/{id}/pathology-report` | `MISSING / NEW` |
| `FR-13.13` | Patolog dapat menyimpan nilai parameter sebagian, lalu **menyelesaikan** laporan | `PUT` + `POST /finalize` | `MISSING / NEW` |
| `FR-13.14` | `Selesaikan` **ditolak** selama parameter wajib masih kosong, dan jawabannya **menyebut ruas mana saja** | `VAL-95` | `MISSING / NEW` |
| `FR-13.15` | Patolog dapat **membuka kembali** laporan yang sudah selesai, dengan alasan, dan jejaknya tersimpan | `POST /reopen` | `MISSING / NEW` |
| `FR-13.16` | `Waktu Efektif` dan `Waktu Issued` **ditampilkan sebagai turunan**, dan **tidak dapat diketik** | `INV-38` | `MISSING / NEW` |
| `FR-13.17` | Pesanan yang jenis pemeriksaannya **belum dipetakan** menampilkan sebab dan siapa yang mengaturnya — bukan formulir kosong | `VAL-100` | `MISSING / NEW` |
| ~~`FR-13.5`~~ | ~~Tiga ruas narasi wajib~~ | **Dicabut** — digantikan `FR-13.12` sampai `FR-13.14` | — |

### 19.3 Skenario UAT

**Jalur berhasil.** Satu pesanan PA memuat pemeriksaan Histologi **dan** IHK pada jaringan yang
sama. Layar menampilkan **satu** formulir: Makroskopik, Mikroskopik, Kesimpulan, sepuluh ruas
IHK — dan **`Anjuran` muncul sekali**, bukan dua kali. Patolog mengisi seluruhnya, menekan
`Selesaikan`. Laporan terkunci, `Waktu Issued` terisi sendiri.

**Jalur gagal — belum lengkap.** Patolog mengosongkan `Kesimpulan` lalu menekan `Selesaikan`.
Sistem menolak `422` **dan menyebut "Kesimpulan"** — bukan sekadar "laporan belum lengkap".

**Jalur gagal — belum dipetakan.** Hari pertama, pemetaan jenis pemeriksaan belum diisi. Patolog
membuka layar dan melihat *"Jenis pemeriksaan pada pesanan ini belum digolongkan… Hubungi kepala
instalasi."* — bukan formulir kosong tanpa sebab.

**Jalur gagal — sudah selesai.** Patolog mencoba menyunting laporan yang sudah `Selesaikan`.
Ditolak `409`; yang tersedia hanya `Buka Kembali`, dan itu menuntut alasan.

**Jalur gagal — parameter dinonaktifkan.** Kepala instalasi menonaktifkan satu parameter yang
sudah dipakai laporan bulan lalu. **Laporan lama tetap terbaca utuh**; yang ditolak hanya
pemakaiannya pada nilai baru.

### 19.4 Definition of Done — menggantikan butir PA pada 18.5

| # | Butir | Cara menjawabnya |
|---:|---|---|
| 1 | `LAB-API-v1` `r25`, `LAB-VAL-v1` `r8`, `LAB-PERM-v1` rev 7 **disetujui** | Tanda tangan pada ketiga kontrak |
| 2 | Tujuh tabel berdiri; **nol `ALTER TABLE`** pada tabel yang sudah berisi data | Migration berjalan dan dapat dimundurkan |
| 3 | **Ketujuh index unik berbentuk PARSIAL** | Diperiksa pada definisi index di database |
| 4 | Tiga data induk **terisi** — kategori 4, parameter 15, keberlakuan 21 | Diperiksa isinya, bukan tabelnya |
| 5 | **Pemetaan jenis pemeriksaan terisi** untuk seluruh pemeriksaan PA di katalog | Diperiksa isinya |
| 6 | `VAL-92`..`VAL-102` ditegakkan; `VAL-95` **menyebut ruas yang kosong** | Bukti per aturan |
| 7 | **Nol kolom `IssuedAt`/`EffectiveAt`**, **nol kolom status hasil**, **nol kolom `Pathology*` pada `LabExamination`** | Dibuktikan **terbalik** |
| 8 | Isi laporan dan konteks klinis **tidak muncul** pada logger maupun DTO layar non-klinis | Diperiksa pada log dan pada DTO |

> **Butir 7 berbentuk ketiadaan rangkap tiga**, dan itu disengaja: ketiganya pernah dirancang
> lalu dicabut dalam hari yang sama. Yang pernah ditulis di dokumen paling mudah dibangun ulang
> oleh pelaksana yang membaca revisi lama.

### 19.5 Urutan pengiriman — menggantikan `MVP-6b`

| Gelombang | Isi | Prasyarat |
|---|---|---|
| **`MVP-6b1`** | Empat data induk PA beserta endpoint dan **pengisiannya** | Kontrak `r25` disetujui |
| **`MVP-6b2`** | Laporan PA, konteks klinis, dan jalurnya | `MVP-6b1` **termasuk pemetaannya terisi** |
| `POST-MVP` | Gambar, HL7, cetak bilingual, Informasi Specimen, validasi/rilis | `DEC-LAB-016`, `LAB-COORD-012`, `LAB-COORD-013`, `S2b`, `DEC-LAB-011` |

> **`MVP-6b1` dan `MVP-6b2` tidak boleh ditukar.** Layar laporan yang jadi sebelum pemetaan
> terisi akan menampilkan formulir kosong bagi **setiap** pesanan PA — dan itu bukan bug yang
> akan terlihat saat pengujian, sebab pengujiannya memakai data yang sengaja disiapkan.

### 19.6 Pertanyaan terbuka

| Pertanyaan | Memblokir? | Pemilik |
|---|---|---|
| Persetujuan `r25`, `r8`, rev 7 | **Ya** — seluruh bagian PA | Yoga Aji Pratama |
| Siapa mengisi pemetaan jenis pemeriksaan PA, dan kapan | **Ya** untuk `MVP-6b2` | Kepala instalasi + `DR-LAB-003` |
| `DEC-LAB-016`, `LAB-COORD-012`, `LAB-COORD-013`, `S2b` | **Tidak** — masing-masing satu bagian | Platform / pemilik modul |

---

## 20. Amandemen 2026-09-24 — `EPIC-LAB-14` Perluasan hasil Patologi Klinik dan perbaikan `S4b`

Menurunkan `02-backend-architecture.md` bagian 19, `03-frontend-architecture.md` amandemen
2026-09-24, `erd/data-dictionary.md` bagian 16, dan usulan kontrak `LAB-API-v1` `r33`,
`LAB-VAL-v1` `r11`, `LAB-PERM-v1` revision 10, `LAB-STATE-v1` `r4`. Keputusannya berasal dari
amendment pass putaran 14 dan closure pass putaran 15 (decisions rev 71).

**Seluruh entity, permission, dan endpoint yang disebut di bawah sudah tercatat pada dokumen
itu.** Nol konsep lahir di sini. **Nol tabel dan nol kolom baru.**

### 20.1 Batas epic ini — titik mulai dan titik akhir

| Batas | Isi |
|---|---|
| **Titik mulai** | Order Patologi Klinik atau Mikrobiologi sudah berdiri dan wadahnya layak; analis membuka halaman hasil order itu |
| **Titik akhir** | Setiap pemeriksaan tersimpan sebagai Draft atau Final, dengan penanda `L`/`H` dan catatan konsultasi bila ada. **Berhenti di situ** — nol validasi, nol rilis, nol pengiriman |

**Yang dianggap selesai:** analis Patologi Klinik mengisi seluruh pemeriksaan satu order pada
satu halaman dan menyatakannya selesai per pemeriksaan; hanya analis yang dapat menulis hasil;
dan hasil yang sudah Final — Patologi Klinik maupun Mikrobiologi — tidak dapat berubah tanpa
Reopen. **Yang tidak berubah:** hasil masih belum dapat dinyatakan sah oleh siapa pun.

### 20.2 Kemampuan `MUST HAVE`

| ID kemampuan asal | Kemampuan | Disposisi |
|---|---|---|
| `CAP-P14-01` | Analis mengisi hasil Patologi Klinik, pengisi tercatat otomatis | **`EXISTING / REUSE`** |
| `CAP-P14-02` | Hanya pemegang izin hasil yang menulis hasil | **`EXTEND`** — resource turunan baru |
| `CAP-P14-03` | Draft, Final, Reopen per pemeriksaan untuk Patologi Klinik | **`EXTEND`** — logika Mikrobiologi dipakai ulang |
| `CAP-P14-04` | Hasil Final tidak dapat ditimpa | **`EXTEND`** — perbaikan atas yang sudah berdiri |
| `CAP-P14-08` | Konsultasi Patologi Klinik | **`EXTEND`** — kolom sudah ada |
| `CAP-P14-17` | Penanda `L`/`H` berhuruf | **`EXTEND`** |
| `CAP-P14-18` | Halaman hasil Patologi Klinik per order tanpa modal | **`MISSING / NEW`** |

**Yang ditunda, beserta penggantinya selama MVP:**

| Ditunda | Sebab | Pengganti selama MVP |
|---|---|---|
| Validasi, rilis, antrean validasi, *Kembalikan ke analis* | `DEC-LAB-011` `BLOCKING`; `LAB-COORD-016` | Tidak ada pengganti di sistem: hasil belum sah dan **tidak dikirim** ke mana pun — keadaan yang sama dengan hari ini, tidak ada yang hilang |
| Penanda `KRITIS` Patologi Klinik | `S5` — `LAB-P0-004` | Pelaporan nilai kritis tetap berjalan di luar sistem sesuai prosedur yang berlaku hari ini |
| Label order *Selesai* | Bergantung rilis | Daftar pantau menampilkan keadaan per pemeriksaan |

### 20.3 Functional requirement

| ID | Kebutuhan | Dapat diuji lewat | Disposisi |
|---|---|---|---|
| `FR-14.1` | Mengisi hasil, Final, Reopen, dan mencatat konsultasi hanya dapat dilakukan pemegang `LabExaminationResult : Update` | `AC-221`, `AC-222` | `EXTEND` |
| `FR-14.2` | Batal, cito, dan duplo tetap memakai `LabExamination : Update`; dokter pemesan tetap dapat menandai cito | `AC-221` | `EXISTING / REUSE` |
| `FR-14.3` | Menyimpan hasil Patologi Klinik atau Mikrobiologi yang sudah Final ditolak `409`, tanpa perubahan setengah jalan | `VAL-120`, `AC-225` | `EXTEND` |
| `FR-14.4` | Mencatat konsultasi pada hasil Final ditolak `409` | `VAL-121`, `AC-226` | `EXTEND` |
| `FR-14.5` | Final, Reopen, dan konsultasi tersedia bagi Patologi Klinik lewat route netral disiplin; Patologi Anatomi ditolak | `r33` 28.2, `VAL-122`, `AC-196`, `AC-197`, `AC-214` | `EXTEND` |
| `FR-14.6` | Route `/result/microbiology/finalize`, `/reopen`, `/consultation` dicabut; halaman Mikrobiologi beralih ke route netral **pada rilis yang sama** | `r33` 28.4 | `EXTEND` — memecah kompatibilitas, butuh persetujuan eksplisit |
| `FR-14.7` | Seluruh pemeriksaan Patologi Klinik yang tidak batal pada satu order terbaca dalam satu panggilan | `GET /by-order/{labOrderId}/results`, `VAL-123`, `AC-234` | `MISSING / NEW` |
| `FR-14.8` | Hasil angka di luar rujukan tampil dengan huruf `L` atau `H` di layar | `referenceFlag`, `AC-219` | `EXTEND` |
| `FR-14.9` | Hasil pilihan di luar rujukan tampil sebagai teks | `referenceFlag = OutOfReference` | **`OPEN DECISION`** — teksnya menunggu persetujuan (19.10 butir 2) |
| `FR-14.10` | Hasil Patologi Klinik diisi pada halaman per order; Final per pemeriksaan tidak mengunci baris lain; isian baris lain tidak hilang | `AC-234`..`AC-237` | `MISSING / NEW` |
| `FR-14.11` | Dialog isi hasil pada Daftar Kerja dicabut; baris membuka halaman order | `AC-235` | `EXTEND` |
| `FR-14.12` | Halaman Mikrobiologi menampilkan `409` dan `403` secara terbaca tanpa menghilangkan isian | `AC-228` | `EXTEND` |
| `FR-14.13` | Kebijakan izin hasil bagi jabatan analis terpasang dalam jendela rilis yang sama | `AC-223`, `02-backend-architecture.md` 19.7 | `MISSING / NEW` — langkah rilis, bukan kode |

### 20.4 Skenario UAT

**Jalur berhasil — halaman Patologi Klinik.** Analis Sari membuka order Andi dari Daftar Kerja
dan melihat 18 pemeriksaan pada satu tabel. Ia mengisi Kalium 6,4 — tampil `H 6,4` — dan
menekan Final pada baris itu. Hemoglobin masih dapat ia isi. Ia mengisi 17 baris sisanya, dan
menekan Final pada masing-masing.

**Jalur gagal — hasil Final ditimpa.** Rekan Sari, dari tab yang terbuka sejak sebelum Final,
mengirim Kalium 6,1. Sistem menjawab *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih
dulu bila perlu diubah."* Kalium tetap 6,4, dan angka 6,1 masih terlihat di isian rekannya
untuk dibandingkan.

**Jalur gagal — bukan analis.** dr. Rina menandai Kalium cito — berhasil. Dari akun yang sama,
panggilan simpan hasil dijawab `403`, dan halaman order baginya tampil baca-saja.

**Jalur gagal — Mikrobiologi setengah jalan.** Hasil kultur urin Final dengan satu isolat dan
dua belas baris antibiogram. Simpan ulang dengan isolat pengganti ditolak `409`; basis data
tetap memuat satu isolat dan dua belas baris.

**Jalur gagal — disiplin keliru.** Petugas membuka order Mikrobiologi lewat halaman Patologi
Klinik. Sistem menjawab `422` dan halaman menawarkan tautan ke halaman Mikrobiologi.

**Jalur berhasil — Reopen.** Sari menekan Reopen pada Kalium dengan alasan *"Satuan salah
ketik"*, membetulkan, dan Final lagi. `ReopenCount` = 1 dan riwayatnya memuat alasan itu.

### 20.5 Definition of Done

| # | Butir | Cara menjawabnya |
|---:|---|---|
| 1 | `LAB-API-v1` `r33`, `LAB-VAL-v1` `r11`, `LAB-PERM-v1` revision 10, `LAB-STATE-v1` `r4` **disetujui pemilik modul**, termasuk perubahan route yang memecah kompatibilitas | Ada `approved_by`/`approved_at` pada keempatnya |
| 2 | Kelima tindakan hasil memakai `[AccessPermission("LabExaminationResult", "Update")]` berpasangan dengan `[AccessAction]` | Startup Development lolos `PermissionRegistryValidator` |
| 3 | `VAL-120`..`VAL-123` ditegakkan | Setiap aturan punya bukti pemeriksaan |
| 4 | Jalur baca per order memakai **satu** kueri untuk seluruh baris | Diperiksa pada log kueri, bukan niat |
| 5 | Halaman Patologi Klinik per order berdiri; dialog Daftar Kerja **tidak ada lagi** | Diperiksa pada layar berjalan |
| 6 | Kebijakan izin hasil terpasang bagi jabatan analis **saja** | Daftar pemegang pada layar Akses Role; dokter pemesan tidak ada di dalamnya |
| 7 | **Nol kolom dan nol tabel baru**; **nol status hasil baru** | Dibuktikan **terbalik**: nol migration pada rilis ini |
| 8 | **Nol penanda `KRITIS`** dan **nol tombol Validasi/Rilis** pada kedua halaman | Diperiksa pada layar berjalan |
| 9 | **Nol route alias** `/result/microbiology/*` tersisa | Swagger tidak memuatnya |
| 10 | Kebijakan **tidak** disalin otomatis dari `LabExamination : Update` | Nol seeder atau migration kebijakan pada rilis ini |

> **Butir 7-10 sengaja berbentuk ketiadaan.** Keempatnya hal yang paling mungkin ditambahkan
> dengan niat baik: status terasa rapi, tombol Validasi melengkapi layar, alias route terasa
> aman, dan menyalin kebijakan terasa menghemat kerja admin. Yang terakhir **membuka kembali**
> `LAB-CONFLICT-012`.

### 20.6 Urutan pengiriman

| Gelombang | Isi | Prasyarat |
|---|---|---|
| **`MVP-8a`** | **Perbaikan yang sudah berdiri:** izin hasil pada lima tindakan, penjaga Final Mikrobiologi, route netral, dan halaman Mikrobiologi beralih — **backend dan frontend dirilis bersama**, disusul langkah kebijakan 19.7 | Kontrak disetujui; `UNK-P14-01` dibaca admin sebelum rilis |
| **`MVP-8b`** | Backend perluasan Patologi Klinik: penjaga Final `PUT /result`, penjaga disiplin, jalur baca per order, `referenceFlag` | `MVP-8a` |
| **`MVP-8c`** | Frontend halaman Patologi Klinik per order; dialog Daftar Kerja dicabut | `MVP-8b` |
| **`POST-MVP`** | Validasi dan rilis (`S4`/`S4d`), *Kembalikan ke analis*, kewenangan dari kredensial Human Resource; penanda `KRITIS` (`S5`) | `DEC-LAB-011`; `LAB-COORD-016`; `LAB-P0-004` |

> **`MVP-8a` sengaja didahulukan, dan itu keputusan berbasis risiko.** Isinya menutup dua celah
> pada kode yang **sudah berdiri** — dokter pemesan dapat menulis hasil, dan hasil Final dapat
> ditimpa. Keduanya tetap terbuka selama `MVP-8a` belum rilis, sedangkan `MVP-8b` dan `MVP-8c`
> hanya menambah kemampuan. `FR-14.9` **tidak masuk gelombang mana pun** sampai teksnya
> disetujui.

### 20.7 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Memblokir? | Pemilik |
|---|---|---|
| ~~Persetujuan `r33`, `r11`, revision 10, dan `r4`~~ | ✅ **Terjawab 2026-09-24** — keempatnya disetujui Yoga Aji Pratama. **Tidak lagi memblokir** | — |
| ~~Persetujuan pencabutan tiga route Mikrobiologi (19.10 butir 1)~~ | ✅ **Terjawab 2026-09-24** — disetujui bersama `r33`. `FR-14.6` berlaku | — |
| Teks penanda hasil pilihan di luar rujukan (19.10 butir 2) | **Ya** untuk `FR-14.9` saja | Yoga Aji Pratama |
| Jabatan mana yang analis — `UNK-P14-01` | **Ya** untuk **rilis** `MVP-8a`, bukan pengembangannya | Admin sistem + kepala instalasi |
| `DEC-LAB-011`, `LAB-COORD-016` | **Tidak** memblokir epic ini. *`DEC-LAB-011` dijawab sebagian 2026-09-24 (`LAB-DEC-150`); sisanya `LAB-REQ-014`* | dr. Bima Prasetya, Sp.PK; pemilik `human-resource` |

> ### ✅ Gerbang perencanaan terbuka — 2026-09-24
>
> `LAB-API-v1` `r33`, `LAB-VAL-v1` `r11`, `LAB-PERM-v1` revision 10, dan `LAB-STATE-v1` `r4`
> **disetujui** Yoga Aji Pratama pada 2026-09-24, **termasuk** pencabutan tiga route Mikrobiologi.
> **`EPIC-LAB-14` boleh diteruskan ke `/plan-module-delivery`** untuk `MVP-8a`, `MVP-8b`, dan
> `MVP-8c`.
>
> **Yang tetap di luar gelombang mana pun:** `FR-14.9` — teks penanda hasil pilihan — sampai
> teksnya disetujui. **Yang menahan rilis, bukan pengembangan:** `UNK-P14-01`, jabatan mana yang
> analis, wajib dibaca admin sebelum `MVP-8a` dirilis.

---

## 21. Amandemen 2026-09-25 — `EPIC-LAB-15` Validasi dan rilis hasil Patologi Klinik (`S4`)

Menurunkan `02-backend-architecture.md` bagian 20, `03-frontend-architecture.md` amandemen
2026-09-25, `erd/data-dictionary.md` bagian 17, dan usulan kontrak `LAB-API-v1` `r34`,
`LAB-VAL-v1` `r12`, `LAB-PERM-v1` revision 11, `LAB-STATE-v1` `r5`, serta `LAB-INT-v1` `r4` —
**kelimanya disetujui 2026-09-25** (21.7). Keputusannya berasal dari decisions rev 74; arsitektur domainnya
`LAB-DA-001` rev 8 bagian A5, **`DOMAIN_ARCHITECTURE_READY` untuk desain saja**.

**Seluruh entity, permission, dan endpoint yang disebut di bawah sudah tercatat pada dokumen
itu.** Nol konsep lahir di sini.

> **Ini desain, bukan izin pakai.** Pemakaian nyata `S4` tertahan `DEC-LAB-011` sisa (pemvalidasi
> di luar jam kerja dr. Bima), `DEC-LAB-017` (bolehkah dipakai sebelum pelaporan kritis `S5`),
> `DEC-LAB-018` (siapa perilis), dan `LAB-COORD-016` (dua kode di katalog Human Resource). Epic ini
> dirancang supaya **kode boleh dibangun dan dideploy** tanpa membuka pemakaian: selama kebijakan
> dan penunjukan belum diberikan, tidak seorang pun dapat memvalidasi (`02-backend-architecture.md`
> 20.7).

### 21.1 Batas epic ini — titik mulai dan titik akhir

| Batas | Isi |
|---|---|
| **Titik mulai** | Hasil Patologi Klinik sudah **Final** di halaman hasil per order (`EPIC-LAB-14`) |
| **Titik akhir** | Hasil **dirilis**, tercatat pemvalidasi dan perilisnya beserta jabatan saat itu, dan **terdaftar sebagai dokumen di rekam medis** pasien. Order berlabel *Selesai* bila seluruh pemeriksaannya yang tidak batal sudah dirilis. **Berhenti di situ** — nol cetakan, nol pengiriman ke pasien, nol pelaporan nilai kritis, nol koreksi sesudah rilis |

**Yang dianggap selesai:** dokter berkewenangan laboratorium yang ditunjuk dapat memvalidasi hasil
Final; orang kedua yang ditunjuk dapat merilisnya; hasil yang keliru sebelum rilis dapat
dikembalikan beralasan; dan **tanpa penunjukan pada kredensial Human Resource, tidak seorang pun
dapat mengesahkan hasil** — termasuk admin.

### 21.2 Kemampuan `MUST HAVE`

| ID kemampuan asal | Kemampuan | Disposisi | Kenapa wajib |
|---|---|---|---|
| `CAP-P14-06` | Validasi, rilis, antrean validasi, *Kembalikan ke analis* | **`MISSING / NEW`** | Tanpanya tidak satu pun hasil dapat dinyatakan sah |
| `CAP-P14-09` | Penunjukan per orang, per jenis, per disiplin — dari kredensial Human Resource, **fail-closed** | **`EXISTING / REUSE`** — data Human Resource dipakai apa adanya lewat adapter baca baru | Tanpanya kewenangan hanya per jabatan, bertentangan `LAB-DEC-022` |
| `CAP-P14-10` | Lapis jabatan calon pemvalidasi dan perilis | **`EXISTING / REUSE`** — tiga aksi baru lahir dari atribut | Tanpanya analis dapat memvalidasi (`LAB-DEC-150`) |
| `CAP-P14-07` | Daftar alasan koreksi dan pengembalian | **`MISSING / NEW`** — polanya siap | Tanpa isi, hasil tervalidasi yang keliru **tidak dapat dikembalikan** — analis tidak boleh Reopen sesudah validasi |
| Belum ber-ID di capability map — konsep `LAB-DC-058` (A5.4) | Daftar alasan pengecualian empat mata | **`MISSING / NEW`** | Tanpa isi, dokter tunggal pada malam hari **tidak dapat merilis** hasil yang ia validasi sendiri |
| `UNK-01`, dijawab `LAB-DEC-017` | Pendaftaran dokumen hasil ke rekam medis saat rilis | **`EXTEND`** — satu nilai `ClinicalDocumentKind`; service Rekam Medis dipakai apa adanya | `LAB-DEC-017`. **Catatan jujur:** kemampuan `RegisterSignedAsync` ditelusuri bagian 20, **belum** tercatat sebagai CAP pada capability map — impact scan berikutnya wajib menambahkannya |
| `CAP-P14-05` | Label order *Dalam Pemeriksaan*/*Selesai* | **`MISSING / NEW`** — ruas turunan | `AC-198`, `AC-199` |

**Yang ditunda, beserta penggantinya selama MVP:**

| Ditunda | Sebab | Pengganti selama MVP |
|---|---|---|
| Peringatan satu pemegang per shift (`LAB-DEC-022` butir 4) | Butuh data jadwal jaga yang sumbernya belum ditetapkan; bentuknya bergantung jawaban `DEC-LAB-011` sisa | Kepala instalasi memeriksa daftar pemegang kode pada layar kredensial Human Resource yang sudah ada, sebelum menyusun jadwal |
| Cetakan Patologi Klinik dengan *Validasi oleh* dan *Otorisasi oleh* | `S17`; `PRD1-OPEN-01` masih terbuka | Halaman hasil per order menampilkan kedua pengesah beserta penanda pengecualian |
| Validasi dan rilis Mikrobiologi dan Patologi Anatomi | `S4d`, `S4e` — pemegangnya belum ditetapkan (`DEC-LAB-011` sisa) | Hasil keduanya tetap berhenti di Final — keadaan yang sama dengan hari ini, tidak ada yang hilang |
| Pelaporan nilai kritis | `S5` | **Ditentukan `DEC-LAB-017`** — prosedur manual tertulis yang disahkan pihak klinis |
| **Koreksi sesudah rilis** | `S6` — `DEC-LAB-014`, `DEC-LAB-019` | **Belum ada.** Hasil yang sudah dirilis lalu ternyata keliru **tidak dapat diubah di sistem** sampai `S6`. Lihat pertanyaan terbuka 21.7 |

### 21.3 Functional requirement

| ID | Kebutuhan | Dapat diuji lewat | Disposisi |
|---|---|---|---|
| `FR-15.1` | Hasil Patologi Klinik **Final** dapat divalidasi oleh pemegang `LabExaminationResult : Validate` yang **ditunjuk** validasi Patologi Klinik; hasil Draft ditolak | `AC-196`, `AC-229`, `VAL-124`, `VAL-128` | `MISSING / NEW` |
| `FR-15.2` | Hasil **tervalidasi** dapat dirilis oleh pemegang `: Release` yang ditunjuk rilis Patologi Klinik, **per pemeriksaan** | `AC-198`, `AC-217`, `VAL-133` | `MISSING / NEW` |
| `FR-15.3` | Pengisi yang memvalidasi, atau pemvalidasi yang merilis, **wajib** memilih alasan pengecualian; hasil membawa penanda yang terlihat | `AC-01`, `AC-02`, `VAL-129`..`VAL-132` | `MISSING / NEW`. **Bunyi penandanya disetujui kata per kata 2026-09-25** (20.10 butir 5) |
| `FR-15.4` | Penolakan kewenangan **menyebut sebabnya**; data kewenangan kosong atau tidak terbaca **selalu menolak**; Laboratorium nol menulis ke Human Resource | `AC-230`..`AC-233`, `INT-07` | `MISSING / NEW` |
| `FR-15.5` | Setiap validasi menyimpan jabatan pemvalidasi saat itu; setiap rilis menyimpan jabatan perilis | `AC-239` | `MISSING / NEW` — jabatan perilis **usulan** 20.10 butir 1 |
| `FR-15.6` | Hasil tervalidasi yang belum dirilis dapat dikembalikan beralasan; riwayat validasinya tetap; hasil dirilis tidak dapat dikembalikan | `AC-205`..`AC-207` | `MISSING / NEW` |
| `FR-15.7` | Reopen ditolak sesudah validasi | `AC-197`, `VAL-136` | `EXTEND` |
| `FR-15.8` | Setiap rilis mendaftarkan **tepat satu** dokumen ke rekam medis; bila pendaftaran gagal, rilis batal seluruhnya | `INT-08`, `VAL-137` | `EXTEND` |
| `FR-15.9` | Antrean dua tahap — menunggu validasi dan menunggu rilis — dengan cito di atas; barisnya membuka halaman hasil order | `AC-196`, `VAL-139` | `MISSING / NEW` |
| `FR-15.10` | Order berlabel *Dalam Pemeriksaan* atau *Selesai* menurut rilis pemeriksaannya | `AC-198`, `AC-199` | `MISSING / NEW` |
| `FR-15.11` | Pemeriksaan yang sudah dirilis keluar dari Daftar Kerja dan dari daftar keterlambatan cito | `AC-17` | `EXTEND` |
| `FR-15.12` | Dua daftar alasan dikelola kepala instalasi lewat layar; penanda wajib catatan hanya oleh admin sistem; nol hapus | `VAL-140`..`VAL-142` | `MISSING / NEW` |
| `FR-15.13` | Pembatalan pemeriksaan yang hasilnya sudah dirilis ditolak | `VAL-143` | `EXTEND` — **arah sementara** sampai `DEC-LAB-019` |
| `FR-15.14` | Halaman hasil per order menampilkan pengesah, jabatannya, dan penanda pengecualian; tiga aksi per baris; pengisi diminta alasan **sebelum** mengirim | `LAB-FE-004`, `AC-02` | `EXTEND` |
| `FR-15.15` | Dua tindakan pada detik yang sama tidak dapat sama-sama berhasil | Uji konkurensi, `02-backend-architecture.md` 20.1 | `EXTEND` |
| `FR-15.16` | Kebijakan `Validate`/`Release`/`Return`, dua kode katalog, dan penunjukan terpasang | `02-backend-architecture.md` 20.7 | `MISSING / NEW` — **langkah rilis, bukan kode**; tertahan keempat penahan pemakaian |

### 21.4 Skenario UAT

**Jalur berhasil — validasi dan rilis dua orang.** Kalium 4,6 pasien rawat jalan sudah Final pukul
09.10. dr. Contoh membuka antrean *Menunggu Validasi*, membuka order itu, dan memvalidasi pukul
09.20. Perilis pagi membuka antrean *Menunggu Rilis* dan merilis pukul 09.25. Halaman hasil
menampilkan *Validasi oleh: dr. Contoh — Dokter Penanggung Jawab Laboratorium* dan *Otorisasi
oleh: {perilis}*; satu dokumen hasil laboratorium tercatat di rekam medis pasien. **Nama pada
skenario adalah samaran.**

**Jalur berhasil — dokter tunggal malam hari.** Kalium 7,2 pasien IGD Final pukul 02.10. dr.
Contoh — satu-satunya yang bertugas, memegang kode validasi **dan** rilis — memvalidasi pukul 02.12,
lalu merilis dengan alasan *Shift tunggal, tidak ada dokter lain bertugas*. Hasil membawa penanda
*"Dirilis oleh pemvalidasi sendiri"*.

**Jalur gagal — analis memvalidasi.** Analis Sari, yang namanya pernah tercatat pada daftar
lama, menekan Validasi lewat alamat endpoint. Sistem menjawab `403`. Kalium tetap Final.

**Jalur gagal — penunjukan ditangguhkan.** Bagian SDM menangguhkan penunjukan dr. Contoh pukul
10.00. Pukul 10.01 ia menekan Validasi dan membaca *"Penunjukan validasi Patologi Klinik Anda
sedang ditangguhkan."* Hasil tetap menunggu; tidak ada jalur pintas.

**Jalur gagal — sampel tertukar sesudah validasi.** Perilis menemukan tabung Hemoglobin tertukar
sebelum merilis. Ia menekan *Kembalikan ke analis* tanpa memilih alasan — ditolak. Ia memilih
*Sampel tertukar* — hasil kembali Draft, dan riwayat tetap menyebut siapa yang pernah
memvalidasinya.

**Jalur gagal — sudah dirilis.** Perilis mencoba mengembalikan Kalium yang sudah dirilis —
*"Hasil yang sudah dirilis hanya dapat diubah lewat koreksi."* Petugas mencoba membatalkan
pemeriksaan itu — juga ditolak.

**Jalur gagal — rekam medis menolak.** Rilis atas hasil yang kunjungannya tidak sah ditolak
dengan sebabnya; hasil tetap tervalidasi, dan **tidak ada** jejak rilis setengah jadi.

### 21.5 Definition of Done

| # | Butir | Cara menjawabnya |
|---:|---|---|
| 1 | `LAB-API-v1` `r34`, `LAB-VAL-v1` `r12`, `LAB-PERM-v1` revision 11, `LAB-STATE-v1` `r5`, dan `LAB-INT-v1` `r4` **disetujui pemilik modul**, termasuk kesepuluh butir `02-backend-architecture.md` 20.10 | Ada `approved_by`/`approved_at` pada kelimanya |
| 2 | Ketiga aksi memakai `[AccessPermission("LabExaminationResult", "Validate")]`, `"Release"`, dan `"Return"`, masing-masing berpasangan `[AccessAction]` | Startup Development lolos `PermissionRegistryValidator` |
| 3 | `VAL-124`..`VAL-143` ditegakkan | Setiap aturan punya bukti pemeriksaan pada `testing/acceptance-test-matrix.md` amandemen 2026-09-25 |
| 4 | Rilis dan pendaftaran rekam medis tersimpan dalam **satu** penyimpanan | Uji gagal `INT-08`: nol `ReleasedAt` ketika pendaftaran ditolak |
| 5 | Dua tindakan bersamaan menghasilkan satu `200` dan satu `409` | Uji konkurensi berpasangan |
| 6 | Kedua daftar alasan berisi **sekurang-kurangnya satu baris aktif** | Diperiksa pada layar data induk sebelum jendela rilis ditutup |
| 7 | **Nol status hasil baru**; **nol kolom** label order | Dibuktikan **terbalik** pada migration dan `LabExaminationStatus` |
| 8 | **Nol tabel penunjukan milik Laboratorium** dan **nol tulisan** ke tabel Human Resource | Tinjauan kode + `AC-232` |
| 9 | **Nol jalur pintas** saat data kewenangan kosong | Uji `VAL-128` kode belum ada di katalog: setiap validasi ditolak |
| 10 | **Nol tombol Validasi/Rilis di antrean** | Diperiksa pada layar berjalan |
| 11 | Kebijakan `Validate`, `Release`, `Return` **tidak** disalin dari `LabExaminationResult : Update` | Daftar pemegang pada layar Akses Role; nol analis di dalamnya |
| 12 | **Penahan pemakaian terjawab sebelum kebijakan diberikan:** `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, `LAB-COORD-016` | Keempatnya berstatus tertutup pada decision log |

> **Butir 7-11 sengaja berbentuk ketiadaan**, pola yang sama dengan `EPIC-LAB-14`. Kelimanya hal
> yang paling mungkin ditambahkan dengan niat baik: status terasa rapi, salinan penunjukan terasa
> lebih cepat, jalur pintas terasa menolong ketika data SDM belum lengkap, tombol di antrean
> terasa menghemat klik, dan menyalin kebijakan terasa menghemat kerja admin. **Tiga yang tengah
> membuat hasil pasien dapat disahkan orang yang tidak ditunjuk.**

### 21.6 Urutan pengiriman

| Gelombang | Isi | Prasyarat |
|---|---|---|
| **`MVP-9a`** | Backend fondasi: migration, kedua data induk alasan beserta endpoint-nya, `LabClinicalPrivilegeResolver` | `MVP-8` selesai; kontrak `EPIC-LAB-15` disetujui |
| **`MVP-9b`** | Backend tindakan: validasi, rilis beserta pendaftaran rekam medis, pengembalian, penjaga Reopen dan batal, antrean, `resultProgress`, penyesuaian Daftar Kerja | `MVP-9a` |
| **`MVP-9c`** | Frontend: kedua layar data induk, aksi dan pengesah pada halaman hasil per order, antrean validasi | `MVP-9b`; `MVP-8c` |
| **`MVP-9d`** | **Langkah rilis:** isi daftar alasan, dua kode katalog, penunjukan, kebijakan aksi | **`BLOCKED`** — `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, `LAB-COORD-016`; lihat juga 21.7 butir koreksi |
| **`POST-MVP`** | `S4d`, `S4e`, `S5`, `S6`, cetakan (`S17`), peringatan per shift | Penahan masing-masing |

> **`MVP-9a`..`MVP-9c` sengaja dapat dikerjakan sebelum penahan terjawab.** Jawaban keempat
> penahan menentukan **siapa** dan **kapan**, bukan **bentuk** (gerbang 0C.5). Yang tertahan
> hanya `MVP-9d` — dan karena resolver fail-closed, deploy `MVP-9a`..`MVP-9c` **tidak membuka**
> pemakaian sedikit pun.

### 21.7 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Memblokir? | Pemilik |
|---|---|---|
| ~~Persetujuan kelima kontrak dan **kesepuluh butir** `02-backend-architecture.md` 20.10~~ | ✅ **Terjawab 2026-09-25** — disetujui Yoga Aji Pratama, termasuk butir 5 kata per kata dan butir 6 pilihan A (perilis). **Tidak lagi memblokir** | — |
| ~~**`LAB-CONFLICT-014`** — order `Completed` manual lewat `PUT /lab-orders/{id}/complete` versus label *Selesai* turunan~~ | ✅ **Terjawab 2026-09-25 — `LAB-DEC-154`**: order hanya `Completed` bila seluruh pemeriksaan tidak batal sudah dirilis. Yang kini menahan langkah 4 `MVP-9d` adalah **pembangunannya** — `FR-15.17` (bagian 23), kontraknya masih `draft` | — |
| ~~**Bolehkah `S4` dipakai sebelum koreksi `S6` berdiri**~~, dan prosedur apa yang berlaku bagi hasil yang sudah dirilis lalu ternyata keliru? | ✅ **Bagian pertama terjawab 2026-09-25 — `LAB-DEC-155`**: boleh; hasil resmi = Tervalidasi dan Dirilis. **Bagian kedua** dibuka sebagai **`LAB-OPEN-045`** — **diusulkan** menahan langkah 4 `MVP-9d` | Yoga Aji Pratama + `DR-LAB-001` |
| ~~`DEC-LAB-011` sisa, `DEC-LAB-018`~~ | ✅ **Terjawab 2026-09-25** — surat tertulis dr. Bima (`LAB-EVD-010`) → `LAB-DEC-152`, `LAB-DEC-153`: validasi di luar jam kerja oleh dokter lain yang ditetapkan pada disiplin yang sama; **perilis tidak wajib dokter**. Yang tersisa dari keduanya adalah **data** — nama pemegang tambahan dicatat di kredensial Human Resource saat `MVP-9d` | — |
| `DEC-LAB-017`, `LAB-COORD-016` | **Tidak** untuk pengembangan; **ya** untuk `MVP-9d` | Yoga Aji Pratama + `DR-LAB-001`; pemilik `human-resource` |
| **`LAB-OPEN-044`** — isi *aturan laboratorium* tentang calon perilis dan penetapnya | **Ya** untuk `MVP-9d` saja | dr. Bima Prasetya, Sp.PK |
| Dua pemegang validasi Patologi Klinik tercatat — `LAB-DEC-022` butir 3 sebagai syarat rilis (`LAB-DEC-152`) | **Ya** untuk `MVP-9d` saja | dr. Bima Prasetya, Sp.PK selaku penetap |
| `DEC-LAB-019` — batal sesudah rilis | **Tidak** — arah sementara `VAL-143` berlaku | Yoga Aji Pratama |
| `UNK-P14-03` — jabatan mana yang dokter berkewenangan laboratorium dan mana yang calon perilis | **Ya** untuk `MVP-9d` saja | Admin sistem + kepala instalasi |
| Isi awal kedua daftar alasan | **Ya** untuk `MVP-9d` saja | Kepala instalasi |

> ### ✅ Gerbang perencanaan terbuka — 2026-09-25
>
> `LAB-API-v1` `r34`, `LAB-VAL-v1` `r12`, `LAB-PERM-v1` revision 11, `LAB-STATE-v1` `r5`, dan
> `LAB-INT-v1` `r4` **disetujui** Yoga Aji Pratama pada 2026-09-25, beserta kesepuluh butir
> `02-backend-architecture.md` 20.10. **`EPIC-LAB-15` boleh diteruskan ke
> `/plan-module-delivery`** untuk `MVP-9a`..`MVP-9d`.
>
> **Yang tetap tertahan — rilis, bukan pengembangan:** `MVP-9d` menunggu `DEC-LAB-017`,
> `LAB-COORD-016`, `UNK-P14-03`, `LAB-OPEN-044`, dua pemegang validasi Patologi Klinik tercatat,
> isi awal kedua daftar alasan, dan — diusulkan — jawaban tentang pemakaian sebelum koreksi `S6`.
> `LAB-CONFLICT-014` wajib dijawab sebelum `MVP-9d`. *`DEC-LAB-011` sisa dan `DEC-LAB-018`
> tertutup 2026-09-25 lewat `LAB-DEC-152` dan `LAB-DEC-153`.*
>
> *Diperbarui 2026-09-25 malam:* `LAB-CONFLICT-014` **terjawab** `LAB-DEC-154` — kini yang menahan
> langkah 4 adalah **pembangunan** penjaganya (`FR-15.17`, bagian 23). Pertanyaan pemakaian sebelum
> `S6` terjawab sebagian `LAB-DEC-155`; sisanya **`LAB-OPEN-045`**, diusulkan menahan langkah 4.

---

## 22. Amandemen 2026-09-25 (kedua) — `EPIC-LAB-16` Validasi dan rilis hasil Mikrobiologi (`S4d-1`)

Menurunkan `02-backend-architecture.md` bagian 21, `03-frontend-architecture.md` amandemen
2026-09-25 (kedua), `erd/data-dictionary.md` bagian 18, dan usulan kontrak `LAB-API-v1` `r35`,
`LAB-VAL-v1` `r13`, `LAB-STATE-v1` `r6`, serta `LAB-INT-v1` `r5` — **keempatnya disetujui
2026-09-25** (22.7); `LAB-PERM-v1` revision 11 berlaku apa adanya. Arsitektur domain `LAB-DA-001` rev 9 bagian A6.

**Seluruh yang disebut di bawah sudah tercatat pada dokumen itu. Nol konsep, nol tabel, nol kolom
baru.** Epic ini **memperluas `EPIC-LAB-15`**; ia tidak dapat dibangun sebelum `EPIC-LAB-15`.

### 22.1 Batas epic

| Batas | Isi |
|---|---|
| **Titik mulai** | Hasil Mikrobiologi sudah **Final** di Halaman Hasil Mikrobiologi (`S4b`), dengan kualifikasi `Definitif` **atau kosong** |
| **Titik akhir** | Hasil **dirilis**, pengesahnya tercatat dan terbaca pada respons hasil, dan **satu** dokumen terdaftar di rekam medis. **Berhenti di situ** — nol rilis hasil `Sementara`, nol cetakan, nol pengiriman ke pasien, nol pelaporan kritis |

**Yang dianggap selesai:** dokter yang ditunjuk validasi Mikrobiologi dapat memvalidasi hasil
akhir; orang kedua yang ditunjuk dapat merilisnya; pemegang kewenangan Patologi Klinik **tidak**
dapat memvalidasi Mikrobiologi; hasil `Sementara` **tidak pernah** keluar lewat sistem.

### 22.2 Kemampuan `MUST HAVE`

| ID kemampuan asal | Kemampuan | Disposisi |
|---|---|---|
| `CAP-P14-06` | Validasi, rilis, antrean, *Kembalikan ke analis* — bagi Mikrobiologi | **`EXTEND`** — rancangan `EPIC-LAB-15` |
| `CAP-P14-09` | Penunjukan per disiplin dari kredensial Human Resource | **`EXISTING / REUSE`** — dua kode Mikrobiologi |
| `CAP-P14-12` | Penanda kritis Mikrobiologi yang sudah menyala | **`EXISTING / REUSE`** — tidak berubah; alasan `DEC-LAB-017` sejenis ikut menahan pemakaian |

**Yang ditunda, beserta penggantinya selama MVP:**

| Ditunda | Sebab | Pengganti selama MVP |
|---|---|---|
| Validasi dan rilis hasil `Sementara` | `DEC-LAB-020` — keputusan klinis `DR-LAB-002` | Hasil sementara disampaikan di luar sistem sesuai prosedur yang berlaku hari ini; **nol kemunduran** — hari ini tidak satu pun hasil Mikrobiologi dapat dirilis |
| Cetakan Mikrobiologi berisi pengesah | Belum ada komponen cetak Mikrobiologi — `S17` | Halaman hasil menampilkan kedua pengesah |
| Validasi dan rilis Patologi Anatomi | `DEC-LAB-021` | Laporan PA berhenti di Final seperti hari ini |

### 22.3 Functional requirement

| ID | Kebutuhan | Dapat diuji lewat | Disposisi |
|---|---|---|---|
| `FR-16.1` | Ketiga tindakan `EPIC-LAB-15` berlaku bagi Mikrobiologi dengan kode kewenangan **Mikrobiologi** | `AC-241`, `VAL-126` bunyi baru | `EXTEND` |
| `FR-16.2` | Hasil `Sementara` ditolak saat divalidasi maupun dirilis; hasil berkualifikasi kosong diterima | `VAL-144`; `ARCH-GAP-LAB-10` | `EXTEND` — perlakuan kualifikasi kosong **disetujui bersama kontrak** (21.10 butir 2) |
| `FR-16.3` | Isolat, antibiogram, status temuan, dan kualifikasi tidak berubah sesudah validasi | `INV-53`, `VAL-120` | `EXISTING / REUSE` |
| `FR-16.4` | Respons hasil Mikrobiologi memuat pengesah; kosong sebelum pengesahan | `AC-183`; `r35` 30.3 | `EXTEND` |
| `FR-16.5` | Satu dokumen rekam medis per pemeriksaan Mikrobiologi yang dirilis | `INT-08` | `EXTEND` |
| `FR-16.6` | Antrean memuat Patologi Klinik dan Mikrobiologi, tanpa hasil `Sementara` | `VAL-145` | `EXTEND` |
| `FR-16.7` | Label order Mikrobiologi *Dalam Pemeriksaan*/*Selesai* | `AC-199` | `EXTEND` |
| `FR-16.8` | Halaman Hasil Mikrobiologi menampilkan pengesah, penanda, dan tiga tindakan; Validasi tidak ditawarkan pada hasil `Sementara` | `LAB-FE-004` | `EXTEND` |
| `FR-16.9` | Dua kode Mikrobiologi di katalog, penunjukan dr. Nabila dan satu pemegang lain | `02-backend-architecture.md` 21.7 | `MISSING / NEW` — **langkah rilis** |

### 22.4 Skenario UAT

**Jalur berhasil.** Kultur urin *Escherichia coli* `Definitif` Final pukul 09.00. dr. Nabila
memvalidasinya pukul 10.00; perilis merilisnya pukul 10.20. Respons hasil memuat *Validasi oleh:
dr. Nabila* dan *Petugas Otorisasi: {perilis}*; satu dokumen tercatat di rekam medis pasien.

**Jalur gagal — disiplin lain.** dr. Contoh, yang hanya ditunjuk validasi Patologi Klinik, menekan
Validasi pada kultur urin itu → ditolak dengan pesan yang menyebut **Mikrobiologi**.

**Jalur gagal — hasil sementara.** Kultur darah Final `Sementara`. Tombol Validasi tidak
ditawarkan; panggilan langsung ke endpoint dijawab `422`. Hasil itu tidak ada di antrean dokter.

**Jalur gagal — hasil sudah disahkan.** Sesudah kultur urin divalidasi, analis mencoba menambah
isolat kedua → `409`; isolat tetap satu.

### 22.5 Definition of Done

| # | Butir | Cara menjawabnya |
|---:|---|---|
| 1 | `LAB-API-v1` `r35`, `LAB-VAL-v1` `r13`, `LAB-STATE-v1` `r6`, `LAB-INT-v1` `r5` **disetujui**, termasuk kelima butir `02-backend-architecture.md` 21.10 | `approved_by`/`approved_at` pada keempatnya |
| 2 | `VAL-126` bunyi baru, `VAL-144`, `VAL-145` ditegakkan | Baris masing-masing pada matriks uji |
| 3 | `AC-241` terbukti | Uji integrasi dua pengguna |
| 4 | **Nol migration** pada epic ini | Dibuktikan terbalik |
| 5 | **Nol rilis hasil `Sementara`** dan **nol validasi per isolat** | Dibuktikan terbalik |
| 6 | Dua kode Mikrobiologi di katalog; **dua** pemegang validasi Mikrobiologi tercatat | Katalog dan kredensial Human Resource — **langkah rilis** |
| 7 | Penahan pemakaian terjawab: `DEC-LAB-017` sejenis bagi Mikrobiologi, `LAB-COORD-016`, `LAB-OPEN-044` | Decision log |

### 22.6 Urutan pengiriman

| Gelombang | Isi | Prasyarat |
|---|---|---|
| **`MVP-10a`** | Backend: penjaga disiplin dan `Sementara`, kode Mikrobiologi, ruas pengesah respons Mikrobiologi, antrean dua disiplin, `resultProgress` Mikrobiologi | **`MVP-9b` selesai**; kontrak `EPIC-LAB-16` disetujui |
| **`MVP-10b`** | Frontend: Halaman Hasil Mikrobiologi, penyaring disiplin antrean, label order Mikrobiologi | `MVP-10a`; `MVP-9c` |
| **`MVP-10c`** | Langkah rilis: dua kode, penunjukan, kebijakan jabatan Mikrobiologi | **`BLOCKED`** — penahan 22.5 butir 6-7 |
| **`POST-MVP`** | `S4d-2`, `S4e`, cetakan Mikrobiologi (`S17`) | `DEC-LAB-020`, `DEC-LAB-021`, `S17` |

### 22.7 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Memblokir? | Pemilik |
|---|---|---|
| ~~Persetujuan keempat kontrak dan kelima butir 21.10~~ | ✅ **Tertutup 2026-09-25** — disetujui termasuk perubahan bunyi `VAL-126` | Yoga Aji Pratama |
| `DEC-LAB-020` beserta titipan `ARCH-GAP-LAB-10` | **Tidak** untuk epic ini — hasil `Sementara` sudah di luar batasnya | `DR-LAB-002` + Yoga Aji Pratama |
| `DEC-LAB-017` sejenis bagi Mikrobiologi; dua pemegang validasi; `LAB-COORD-016`; `LAB-OPEN-044` | **Tidak** untuk pengembangan; **ya** untuk `MVP-10c` | `DR-LAB-002`; dr. Bima; pemilik `human-resource` |

> ### ✅ Gerbang perencanaan terbuka — 2026-09-25
>
> `LAB-API-v1` `r35`, `LAB-VAL-v1` `r13`, `LAB-STATE-v1` `r6`, dan `LAB-INT-v1` `r5` **disetujui**
> Yoga Aji Pratama pada 2026-09-25, beserta kelima butir `02-backend-architecture.md` 21.10 dan
> **perubahan bunyi `VAL-126`**. **`EPIC-LAB-16` boleh diteruskan ke `/plan-module-delivery`**
> untuk `MVP-10a`..`MVP-10c` — roadmap disusun hari yang sama (`backend-roadmap.md` bagian 6al).
>
> **Prasyarat pengerjaan yang ditegaskan pemilik modul:** gelombang ini *"baru bisa dikerjakan
> setelah MVP-9b selesai"* — seluruh `BE-LAB-73`..`BE-LAB-77` selesai lebih dulu.
>
> **Yang tetap tertahan — rilis, bukan pengembangan:** `MVP-10c` menunggu `DEC-LAB-017` sejenis
> bagi Mikrobiologi, `LAB-COORD-016`, `LAB-OPEN-044`, dua pemegang validasi Mikrobiologi tercatat,
> dan `UNK-P14-03` diperluas — ditambah seluruh penahan `MVP-9d`, sebab `MVP-10c` tidak dapat
> mendahuluinya.

## 23. Amandemen 2026-09-25 (ketiga) — Penyelesaian order, hasil resmi, dan label keadaan

Menurunkan decisions rev 77 — `LAB-DEC-154`, `LAB-DEC-155`, `LAB-DEC-156` dari bukti `LAB-EVD-011` —
beserta `02-backend-architecture.md` bagian 22 dan `03-frontend-architecture.md` amandemen
2026-09-25 (ketiga). Kontrak `LAB-API-v1` `r36`, `LAB-VAL-v1` `r14`, dan `LAB-STATE-v1` `r7`
— **ketiganya disetujui 2026-09-25** (23.5). **Bukan epic baru:** ketiga kebutuhan memperluas `EPIC-LAB-15`.

### 23.1 Functional requirement

| ID | Kebutuhan | Dapat diuji lewat | Disposisi |
|---|---|---|---|
| `FR-15.17` | Order hanya dapat `Completed` bila seluruh pemeriksaan tidak batal sudah **dirilis**; bila tidak, `409` beserta rincian **setiap** pemeriksaan yang menahan. Pemeriksaan tanpa jalur validasi menahan order | `AC-243`, `AC-244`, `AC-245`; `VAL-146` | `EXTEND` — kontrak disetujui; `BE-LAB-81`, gelombang `MVP-9e` |
| `FR-15.18` | Hanya hasil **Tervalidasi dan Dirilis** yang resmi; hasil yang belum dirilis tidak keluar sebagai hasil resmi — nol dokumen rekam medis, nol pengiriman, nol cetak final | `AC-246` | `EXISTING / REUSE` — backend sudah menahannya; **nol kode baru** |
| `FR-15.19` | Keadaan pemeriksaan tampil sebagai *Menunggu Hasil*, *Draft*, *Menunggu Validasi*, *Tervalidasi*, *Dirilis*; tombol Final bertuliskan *Pemeriksaan Selesai* | `AC-247` | `EXTEND` — frontend saja, **nol kontrak**; masuk `FE-LAB-36`, `FE-LAB-39`, `FE-LAB-40`, `FE-LAB-41` |

### 23.2 Skenario UAT

**Jalur berhasil — order selesai.** Order berisi Kalium, Hemoglobin, dan Glukosa. Glukosa dibatalkan
pukul 08.30; Kalium dirilis 09.15; Hemoglobin dirilis 09.50. Pukul 10.00 petugas menekan Selesai → order
`Completed`.

**Jalur gagal — masih ada yang belum dirilis.** Pada order yang sama, pukul 09.45 Hemoglobin baru
divalidasi. Petugas menekan Selesai → ditolak; rincian menyebut *Hemoglobin — Tervalidasi*; order tetap
diproses.

**Jalur gagal — disiplin tanpa validasi.** Order Patologi Anatomi dengan laporan Final → ditolak,
rincian *Histopatologi — Menunggu Validasi*, sampai `S4e` berdiri.

**Label.** Analis menekan **Pemeriksaan Selesai** pada Kalium; layar menulis *Menunggu Validasi* dan
Kalium muncul di antrean dokter.

### 23.3 Definition of Done

| # | Butir | Cara menjawabnya |
|---:|---|---|
| 1 | `r36`, `r14`, `r7` **disetujui**, termasuk keempat butir `02-backend-architecture.md` 22.7 | `approved_by`/`approved_at` pada ketiganya |
| 2 | `VAL-146` ditegakkan; `AC-243`..`AC-245` terbukti | Uji integrasi terhadap aplikasi berjalan |
| 3 | Nol order `Completed` yang masih memuat pemeriksaan tidak batal belum dirilis | Kueri terbalik atas data pengembangan |
| 4 | Kelima label dan tombol *Pemeriksaan Selesai* tampil pada halaman Patologi Klinik dan Mikrobiologi | `AC-247`; laporan task frontend |
| 5 | **Nol migration**, nol permission baru | Dibuktikan terbalik |

### 23.4 Urutan pengiriman

| Kebutuhan | Dibangun kapan | Prasyarat |
|---|---|---|
| `FR-15.17` | `BE-LAB-81`, gelombang **`MVP-9e`** — **sesudah `BE-LAB-76`**, dan **wajib terpasang sebelum langkah 4 `MVP-9d`** | ✅ Kontrak `r36`/`r14`/`r7` disetujui 2026-09-25 |
| `FR-15.18` | Tidak ada pembangunan | — |
| `FR-15.19` | Bersama task frontend yang sudah direncanakan | `LAB-DEC-156` — sudah `approved` |

### 23.5 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Memblokir? | Pemilik |
|---|---|---|
| ~~Persetujuan `r36`, `r14`, `r7` beserta keempat butir 22.7~~ | ✅ **Tertutup 2026-09-25** — disetujui; butir 1 kata per kata | Yoga Aji Pratama |
| **`LAB-OPEN-045`** — prosedur bila hasil yang sudah dirilis ternyata keliru sebelum `S6` ada | **Tidak** untuk pengembangan. **Diusulkan** menahan langkah 4 `MVP-9d` dan langkah 3 `MVP-10c` | Yoga Aji Pratama + `DR-LAB-001` |
| Konfirmasi klinis atas pemakaian rilis sebelum `S6` (`LAB-DEC-155` butir 3) | **Tidak** — diusulkan bersama `LAB-OPEN-045` | `DR-LAB-001` |

> ### ✅ Gerbang perencanaan `FR-15.17` terbuka — 2026-09-25
>
> `LAB-API-v1` `r36`, `LAB-VAL-v1` `r14`, dan `LAB-STATE-v1` `r7` **disetujui** Yoga Aji Pratama
> pada 2026-09-25, beserta keempat butir `02-backend-architecture.md` 22.7. `BE-LAB-81` direncanakan
> hari yang sama sebagai gelombang **`MVP-9e`** (`backend-roadmap.md` 6am.1). `FR-15.19` sejak awal
> tidak menunggu gerbang ini.
