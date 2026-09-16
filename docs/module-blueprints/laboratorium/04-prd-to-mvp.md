# Laboratorium — PRD ke MVP

## 1. Identitas Dokumen

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `4` |
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
